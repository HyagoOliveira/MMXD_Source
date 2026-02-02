#define RELEASE
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;

public class ch014_skill01_Bullet : BulletBase
{
	public new enum ColliderType
	{
		Box = 0,
		Circle = 1
	}

	public enum SpecialColliderID
	{
		X4UAPlasma = 10720,
		X4UAPlasma1 = 10721,
		X4UAPlasma2 = 10722,
		X4UAPlasma3 = 10723
	}

	[HideInInspector]
	public bool IsActivate;

	[HideInInspector]
	public bool IsDestroy;

	public new Callback HitCallback;

	private Camera _mainCamera;

	public bool AlwaysFaceCamera;

	private Collider2D _hitCollider;

	private HashSet<Transform> _ignoreList;

	private OrangeTimer _clearTimer;

	private Rigidbody2D _rigidbody2D;

	private long _duration = -1L;

	private long _hurtCycle;

	private OrangeTimer _durationTimer;

	public Transform pDirTrans;

	public Transform firstpoint;

	public Transform endpoint;

	public Transform[] lineS;

	private float[] lineS_Width;

	public Vector3 offset_pos;

	private void OnTriggerStay2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || !IsActivate || ((1 << col.gameObject.layer) & (int)TargetMask) == 0 || col.isTrigger)
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
			return;
		}
		PlayerCollider component2 = col.GetComponent<PlayerCollider>();
		if (component2 != null && component2.IsDmgReduceShield())
		{
			Hit(col);
		}
		else if (col.gameObject.GetComponentInParent<StageHurtObj>() != null)
		{
			Hit(col);
		}
	}

	private void OnCollisionStay2D(Collision2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || !IsActivate || col.transform.IsChildOf(_transform.root) || ((1 << col.gameObject.layer) & (int)TargetMask) == 0)
		{
			return;
		}
		StageObjParam component = col.collider.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col.collider);
			}
			return;
		}
		PlayerCollider component2 = col.gameObject.GetComponent<PlayerCollider>();
		if (component2 != null && component2.IsDmgReduceShield())
		{
			Hit(col.collider);
		}
		else if (col.gameObject.GetComponentInParent<StageHurtObj>() != null)
		{
			Hit(col.collider);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		IsDestroy = false;
		_ignoreList = new HashSet<Transform>();
		_mainCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
		_durationTimer = OrangeTimerManager.GetTimer();
		_clearTimer = OrangeTimerManager.GetTimer();
		lineS_Width = new float[lineS.Length];
		for (int i = 0; i < lineS.Length; i++)
		{
			LineRenderer component = lineS[i].GetComponent<LineRenderer>();
			lineS_Width[i] = component.startWidth;
		}
	}

	public void Active(LayerMask pTargetMask)
	{
		IsDestroy = false;
		TargetMask = pTargetMask;
		TargetMask = (int)TargetMask | (int)BulletMask;
		TargetMask = (int)TargetMask | (int)BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
		}
		CoroutineMove = StartCoroutine(OnStartMove());
		_rigidbody2D.WakeUp();
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		_transform.position = pTransform.position;
		_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		Direction = pDirection;
		Active(pTargetMask);
		PlayUseSE();
		Update_Effect();
	}

	public override void Active(Vector3 pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		_transform.position = pTransform;
		_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		Direction = pDirection;
		Active(pTargetMask);
		PlayUseSE();
		Update_Effect();
	}

	protected override IEnumerator OnStartMove()
	{
		IsActivate = true;
		_hitCollider.enabled = true;
		_clearTimer.TimerStart();
		_durationTimer.TimerStart();
		while (!IsDestroy)
		{
			if (_clearTimer.GetMillisecond() >= _hurtCycle)
			{
				_clearTimer.TimerStart();
				_ignoreList.Clear();
				_rigidbody2D.WakeUp();
			}
			if (_duration != -1 && _durationTimer.GetMillisecond() > _duration)
			{
				IsDestroy = true;
			}
			if (AlwaysFaceCamera)
			{
				base.transform.LookAt(_mainCamera.transform.position, -Vector3.up);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		BackToPool();
		yield return null;
	}

	public override void BackToPool()
	{
		IsDestroy = false;
		IsActivate = false;
		if ((bool)_hitCollider)
		{
			_hitCollider.enabled = false;
		}
		_ignoreList.Clear();
		_rigidbody2D.Sleep();
		isBuffTrigger = false;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_UNREGISTER, this);
		base.BackToPool();
	}

	public override void Hit(Collider2D col)
	{
		Collider2D collider2D = CheckHitTargetAndShield(_hitCollider, col);
		if (!CheckHitList(ref _ignoreList, collider2D.transform))
		{
			if (HitCallback != null)
			{
				HitCallback();
			}
			_ignoreList.Add(col.transform);
			CaluDmg(BulletData, col.transform, 0f, 0.5f);
		}
	}

	private void Update_Effect()
	{
		float num = ((BoxCollider2D)_hitCollider).size.x - offset_pos.x;
		for (int i = 0; i < lineS.Length; i++)
		{
			lineS[i].GetComponent<LineRenderer>().SetPosition(1, new Vector3(0f - num, 0f, 0f));
		}
		endpoint.localPosition = new Vector3(0f, 0f, num);
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		BulletID = pData.n_ID;
		BulletData = pData;
		nNetID = nInNetID;
		nRecordID = nInRecordID;
		Owner = owner;
		if (_hitCollider != null)
		{
			Object.Destroy(_hitCollider);
		}
		if ((BulletData.n_FLAG & 1) == 0)
		{
			BlockMask = BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		}
		if (((uint)BulletData.n_FLAG & 2u) != 0)
		{
			BulletMask = BulletScriptableObjectInstance.BulletLayerMaskBullet;
		}
		if (pData.s_FIELD == "null")
		{
			_hitCollider = base.gameObject.AddOrGetComponent<Collider2D>();
		}
		else
		{
			string[] array = pData.s_FIELD.Split(',');
			string text = array[0];
			if (!(text == "0"))
			{
				if (text == "1")
				{
					_hitCollider = base.gameObject.AddComponent<CircleCollider2D>();
					((CircleCollider2D)_hitCollider).radius = float.Parse(array[3]);
				}
			}
			else
			{
				_hitCollider = base.gameObject.AddComponent<BoxCollider2D>();
				((BoxCollider2D)_hitCollider).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
			}
			_duration = long.Parse(array[5]);
			_hurtCycle = long.Parse(array[6]);
			_hitCollider.offset = new Vector2(float.Parse(array[1]), float.Parse(array[2]));
			for (int i = 0; i < lineS.Length; i++)
			{
				LineRenderer component = lineS[i].GetComponent<LineRenderer>();
				float num = float.Parse(array[4]) / 2f;
				float startWidth = (component.endWidth = lineS_Width[i] * num);
				component.startWidth = startWidth;
			}
		}
		_hitCollider.isTrigger = true;
		_UseSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_USE_SE);
		if (_UseSE[0] != "" && (_UseSE[1].EndsWith("_lp") || _UseSE[1].EndsWith("_lg")))
		{
			checkLoopSE = true;
		}
		_HitSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_HIT_SE);
		GetHistGuardSE();
	}
}
