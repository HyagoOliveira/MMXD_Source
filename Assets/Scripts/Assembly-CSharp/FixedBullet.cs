#define RELEASE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedBullet : BulletBase
{
	public bool IsActivate;

	public bool IsDestroy;

	private Collider2D _Collider;

	private HashSet<Transform> _ignoreList;

	private OrangeTimer _clearTimer;

	private long _duration;

	private new OrangeTimer ActivateTimer;

	private ContactPoint2D[] hitAry;

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
		}
		else
		{
			Hit(col);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_clearTimer = OrangeTimerManager.GetTimer();
		base.gameObject.AddOrGetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
		IsDestroy = false;
		_ignoreList = new HashSet<Transform>();
		ActivateTimer = OrangeTimerManager.GetTimer();
	}

	public void Active(LayerMask pTargetMask)
	{
		IsDestroy = false;
		TargetMask = pTargetMask;
		TargetMask = (int)TargetMask | (int)BulletMask;
		if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
		}
		CoroutineMove = StartCoroutine(OnStartMove());
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		_transform.position = pPos;
		IsDestroy = false;
		TargetMask = pTargetMask;
		TargetMask = (int)TargetMask | (int)BulletMask;
		CoroutineMove = StartCoroutine(OnStartMove());
		PlayUseSE();
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Debug.Log("NOPE");
	}

	protected override IEnumerator OnStartMove()
	{
		IsActivate = true;
		_Collider.enabled = true;
		_clearTimer.TimerStart();
		ActivateTimer.TimerStart();
		while (!IsDestroy)
		{
			long millisecond = _clearTimer.GetMillisecond();
			if (ActivateTimer.GetMillisecond() > _duration)
			{
				IsDestroy = true;
			}
			if (millisecond >= BulletData.n_FIRE_SPEED)
			{
				_clearTimer.TimerStart();
				_ignoreList.Clear();
			}
			yield return null;
		}
		BackToPool();
	}

	public override void BackToPool()
	{
		IsDestroy = false;
		IsActivate = false;
		_Collider.enabled = false;
		isBuffTrigger = false;
		ClearList();
	}

	public void ClearList()
	{
		_ignoreList.Clear();
	}

	public override void Hit(Collider2D col)
	{
		if (!CheckHitList(ref _ignoreList, col.transform))
		{
			_ignoreList.Add(col.transform);
			CaluDmg(BulletData, col.transform, 0f, 0.5f);
		}
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		BulletID = pData.n_ID;
		BulletData = pData;
		nNetID = nInNetID;
		nRecordID = nInRecordID;
		Owner = owner;
		if (pData.s_FIELD == "null")
		{
			return;
		}
		string[] array = pData.s_FIELD.Split(',');
		_duration = long.Parse(array[5]);
		if ((BulletData.n_FLAG & 1) == 0)
		{
			BlockMask = BulletScriptableObjectInstance.BulletLayerMaskObstacle;
		}
		if (((uint)BulletData.n_FLAG & 2u) != 0)
		{
			BulletMask = BulletScriptableObjectInstance.BulletLayerMaskBullet;
		}
		string text = array[0];
		if (!(text == "0"))
		{
			if (text == "1")
			{
				_Collider = base.gameObject.AddOrGetComponent<CircleCollider2D>();
			}
		}
		else
		{
			_Collider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
		}
		_Collider.offset = new Vector2(long.Parse(array[0]), long.Parse(array[1]));
		_Collider.isTrigger = true;
		_UseSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_USE_SE);
		_HitSE = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(BulletData.s_HIT_SE);
		GetHistGuardSE();
		if (_UseSE[0] != "" && (_UseSE[1].EndsWith("_lp") || _UseSE[1].EndsWith("_lg")))
		{
			checkLoopSE = true;
		}
	}
}
