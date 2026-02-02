using System;
using System.Collections;
using UnityEngine;

public class SlicingBullet : LockingBullet
{
	[SerializeField]
	private SplashBullet splashBullet;

	[SerializeField]
	private string[] BoomSE;

	private long hitCycle = long.MaxValue;

	private int splashCountMax;

	private int splashCountNow;

	private OrangeTimer splashTimer;

	private bool hitNothing;

	private Transform hitOtherTransform;

	private Vector3 slicingOffset = Vector3.zero;

	private readonly int hitChkFrame = 8;

	private int hitChkEndFrame;

	protected override bool CanTriggerHit
	{
		get
		{
			return Phase < BulletPhase.Result;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		splashTimer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SplashBullet>(UnityEngine.Object.Instantiate(splashBullet), splashBullet.gameObject.name, 5);
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		hitChkEndFrame = GameLogicUpdateManager.GameFrame + hitChkFrame;
		hitCycle = TrackingData.n_ENDTIME_2;
		splashCountMax = 1 + (TrackingData.n_ENDTIME_3 - TrackingData.n_ENDTIME_1) / TrackingData.n_ENDTIME_2;
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		SetTarget();
		splashCountNow = 0;
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		SetTarget();
		splashCountNow = 0;
	}

	protected override bool IsTargetWithinRange(IAimTarget aimTarget)
	{
		return aimTarget != null;
	}

	protected override IEnumerator OnStartMove()
	{
		splashTimer.TimerStop();
		while (Phase <= BulletPhase.Result)
		{
			if (splashTimer.GetMillisecond() >= hitCycle)
			{
				splashTimer.TimerStart();
				CreateSplash();
			}
			if (splashCountNow >= splashCountMax)
			{
				Phase = BulletPhase.Result;
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return null;
	}

	private void SetTarget()
	{
		if (Target != null)
		{
			_transform.SetParent(Target.AimTransform);
			_transform.localPosition = Target.AimPoint;
		}
		else
		{
			CheckHit(tStageObjParam);
		}
	}

	protected override void PhaseNormal()
	{
		if (GameLogicUpdateManager.GameFrame >= hitChkEndFrame)
		{
			hitNothing = true;
		}
		if (GameLogicUpdateManager.GameFrame >= trackingEndFrame)
		{
			Phase = BulletPhase.Boomerang;
		}
	}

	protected override void PhaseSplash()
	{
	}

	protected override void DoBoomerang()
	{
		switch (BulletID)
		{
		case 17118:
			PlaySE("HitSE", "ht_guard04");
			break;
		case 17119:
			PlaySE("SkillSE_MH2", "mh2_tama04");
			break;
		}
		splashCountNow = 0;
		splashTimer.TimerStart();
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, false);
		base.DoBoomerang();
	}

	protected override void PhaseResult()
	{
		Phase = BulletPhase.BackToPool;
	}

	public override void Hit(Collider2D col)
	{
		if (!hitNothing && hitOtherTransform == null && Target == null)
		{
			StageObjParam component = col.transform.GetComponent<StageObjParam>();
			CheckHit(component);
		}
	}

	private void CheckHit(StageObjParam hitParam)
	{
		if (hitParam != null)
		{
			IAimTarget aimTarget = hitParam.tLinkSOB as IAimTarget;
			if (aimTarget != null)
			{
				Target = aimTarget;
				SetTarget();
			}
			else
			{
				_transform.SetParentNull();
				hitOtherTransform = hitParam.transform;
				slicingOffset = hitParam.transform.InverseTransformPoint(_transform.position);
			}
		}
	}

	private void CreateSplash()
	{
		SplashBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SplashBullet>(splashBullet.gameObject.name);
		poolObj._transform.SetParentNull();
		poolObj.UpdateBulletData(BulletData, Owner);
		poolObj.BulletLevel = BulletLevel;
		WeaponStatus weaponStatus = new WeaponStatus();
		weaponStatus.nHP = nHp;
		weaponStatus.nATK = nOriginalATK;
		weaponStatus.nCRI = nOriginalCRI;
		weaponStatus.nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck);
		weaponStatus.nCriDmgPercent = nCriDmgPercent;
		weaponStatus.nReduceBlockPercent = nReduceBlockPercent;
		weaponStatus.nWeaponCheck = nWeaponCheck;
		weaponStatus.nWeaponType = nWeaponType;
		PerBuffManager.BuffStatus buffStatus = new PerBuffManager.BuffStatus();
		buffStatus.fAtkDmgPercent = fDmgFactor - 100f;
		buffStatus.fCriPercent = fCriFactor - 100f;
		buffStatus.fCriDmgPercent = fCriDmgFactor - 100f;
		buffStatus.fMissPercent = fMissFactor;
		buffStatus.refPBM = refPBMShoter;
		buffStatus.refPS = refPSShoter;
		poolObj.SetBulletAtk(weaponStatus, buffStatus);
		poolObj.Active(TargetMask, false);
		if (Target != null)
		{
			poolObj._transform.position = Target.AimPosition;
		}
		else
		{
			poolObj._transform.position = _transform.transform.position;
		}
		GenerateImpactFx();
		splashCountNow++;
	}

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
		if (isHitBlock || needPlayEndSE || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}

	public override void BackToPool()
	{
		Target = null;
		hitOtherTransform = null;
		splashCountNow = 0;
		splashTimer.TimerStop();
		hitNothing = false;
		base.BackToPool();
	}

	protected override void SetBulletRotation()
	{
		if (Phase < BulletPhase.BackToPool)
		{
			if (hitOtherTransform != null)
			{
				_transform.position = hitOtherTransform.position + slicingOffset;
			}
			else if (Target != null)
			{
				_transform.localPosition = Target.AimPoint;
			}
		}
	}
}
