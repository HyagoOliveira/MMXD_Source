#define RELEASE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamBullet : BulletBase
{
	[SerializeField]
	private float colliderExtraSizeX;

	[HideInInspector]
	public bool IsActivate;

	[HideInInspector]
	public bool IsDestroy;

	protected Collider2D _hitCollider;

	protected HashSet<Transform> _ignoreList;

	protected long _duration = -1L;

	protected long _hurtCycle;

	protected OrangeTimer _durationTimer;

	protected OrangeTimer _clearTimer;

	protected Rigidbody2D _rigidbody2D;

	protected Camera _mainCamera;

	public bool AlwaysFaceCamera;

	public Transform fxEndpoint;

	public Transform[] fxLine;

	public ColliderType currentColliderType = ColliderType.Undefined;

	private void OnTriggerStay2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	private void OnCollisionStay2D(Collision2D col)
	{
		OnTriggerHit(col.collider);
	}

	public void OnTriggerHit(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || !IsActivate || IsDestroy || col == null || ((1 << col.gameObject.layer) & (int)TargetMask) == 0 || col.isTrigger)
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

	public override void Hit(Collider2D col)
	{
		Collider2D collider2D = CheckHitTargetAndShield(_hitCollider, col);
		if (!CheckHitList(ref _ignoreList, collider2D.transform))
		{
			_ignoreList.Add(collider2D.transform);
			CaluDmg(BulletData, collider2D.transform, 0f, 0.5f);
		}
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		BulletID = pData.n_ID;
		BulletData = pData;
		nNetID = nInNetID;
		nRecordID = nInRecordID;
		Owner = owner;
		if ((BulletData.n_FLAG & 1) == 0)
		{
			BlockMask = BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		}
		else
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
					UpdateCollider(ColliderType.Circle);
					((CircleCollider2D)_hitCollider).radius = float.Parse(array[3]);
					colliderExtraSizeX = 0f;
					currentColliderType = ColliderType.Circle;
				}
			}
			else
			{
				UpdateCollider(ColliderType.Box);
				((BoxCollider2D)_hitCollider).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
			}
			_duration = long.Parse(array[5]);
			_hurtCycle = long.Parse(array[6]);
			_hitCollider.offset = new Vector2(float.Parse(array[1]), float.Parse(array[2]));
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

	private void UpdateCollider(ColliderType nextColliderType)
	{
		switch (nextColliderType)
		{
		case ColliderType.Box:
			if ((bool)_hitCollider)
			{
				if (!_hitCollider.GetType().IsSubclassOf(typeof(BoxCollider2D)))
				{
					_hitCollider.enabled = false;
					_hitCollider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
				}
			}
			else
			{
				_hitCollider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
			}
			currentColliderType = ColliderType.Box;
			break;
		case ColliderType.Circle:
			if ((bool)_hitCollider)
			{
				if (!_hitCollider.GetType().IsSubclassOf(typeof(CircleCollider2D)))
				{
					_hitCollider.enabled = false;
					_hitCollider = base.gameObject.AddOrGetComponent<CircleCollider2D>();
				}
			}
			else
			{
				_hitCollider = base.gameObject.AddOrGetComponent<CircleCollider2D>();
			}
			currentColliderType = ColliderType.Circle;
			break;
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
		_rigidbody2D.simulated = true;
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		_transform.position = pTransform.position;
		_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		_rigidbody2D.simulated = true;
		Direction = pDirection;
		Active(pTargetMask);
		base.SoundSource.UpdateDistanceCall();
		PlayUseSE();
		Update_Effect();
		UpdateExtraColliderSize();
	}

	public override void Active(Vector3 pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
		_transform.position = pTransform;
		_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		_rigidbody2D.WakeUp();
		_rigidbody2D.simulated = true;
		Direction = pDirection;
		Active(pTargetMask);
		base.SoundSource.UpdateDistanceCall();
		ForcePlayUSE();
		Update_Effect();
		UpdateExtraColliderSize();
	}

	protected override IEnumerator OnStartMove()
	{
		IsActivate = true;
		if (!_hitCollider.isActiveAndEnabled)
		{
			_hitCollider.enabled = true;
		}
		_clearTimer.TimerStart();
		_durationTimer.TimerStart();
		while (!IsDestroy)
		{
			if (_clearTimer.GetMillisecond() >= _hurtCycle)
			{
				_clearTimer.TimerStart();
				_ignoreList.Clear();
				_rigidbody2D.WakeUp();
				_rigidbody2D.simulated = true;
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
		isSubBullet = false;
		IsDestroy = false;
		IsActivate = false;
		_ignoreList.Clear();
		_rigidbody2D.simulated = false;
		_rigidbody2D.Sleep();
		isBuffTrigger = false;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_BULLET_UNREGISTER, this);
		base.BackToPool();
	}

	protected virtual void Update_Effect()
	{
		float x = ((BoxCollider2D)_hitCollider).size.x;
		for (int i = 0; i < fxLine.Length; i++)
		{
			LineRenderer component = fxLine[i].GetComponent<LineRenderer>();
			if ((bool)component)
			{
				component.SetPosition(1, new Vector3(0f - x, 0f, 0f));
			}
		}
		fxEndpoint.localPosition = new Vector3(0f, 0f, x);
	}

	protected void UpdateExtraColliderSize()
	{
		if (colliderExtraSizeX != 0f)
		{
			BoxCollider2D obj = (BoxCollider2D)_hitCollider;
			Vector2 size = obj.size;
			obj.size = new Vector2(size.x + colliderExtraSizeX, size.y);
		}
	}
}
