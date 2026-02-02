using System;
using UnityEngine;

public class ShingetsurinBullet : BasicBullet, ILogicUpdate
{
	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3
	}

	[SerializeField]
	private float Phase1StopTime = 1.5f;

	[SerializeField]
	private float ClearHitTime = 0.5f;

	protected SubStatus _subStatus;

	protected Vector3 _vTargetPos = Vector3.zero;

	protected bool _bClearHit;

	protected int nowLogicFrame;

	protected int endLogicFrame;

	protected int logicLength;

	protected VInt timeDelta;

	protected VInt3 nowPos;

	protected VInt3 endPos;

	protected Vector3 _speed = Vector3.zero;

	protected float distanceDelta;

	protected override void Awake()
	{
		base.Awake();
		ForceAIS = true;
	}

	protected new void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public override void OnStart()
	{
		base.OnStart();
		_subStatus = SubStatus.Phase0;
		_bClearHit = false;
		if (Target == null && refPBMShoter.SOB as OrangeCharacter != null)
		{
			OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
			Target = orangeCharacter.PlayerAutoAimSystem.AutoAimTarget;
		}
		if (Target != null)
		{
			_vTargetPos = Target.AimTransform.position + Target.AimPoint;
			_vTargetPos.z = _transform.position.z;
		}
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		if (pTarget != null)
		{
			Target = pTarget;
		}
		NeutralAIS.UpdateAimRange(BulletData.f_DISTANCE);
		CaluLogicFrame();
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		if (pTarget != null)
		{
			Target = pTarget;
		}
		NeutralAIS.UpdateAimRange(BulletData.f_DISTANCE);
		CaluLogicFrame();
	}

	protected void CaluLogicFrame()
	{
		float num = BulletData.f_DISTANCE / (float)BulletData.n_SPEED;
		logicLength = (int)(num / GameLogicUpdateManager.m_fFrameLen);
		timeDelta = new VInt(num / (float)logicLength);
		nowPos = new VInt3(_transform.localPosition);
		endPos = new VInt3(nowPos.vec3 + Direction * BulletData.f_DISTANCE);
		_speed = new Vector3((endPos.vec3.x - nowPos.vec3.x) / num, (endPos.vec3.y - nowPos.vec3.y) / num, 0f);
		nowLogicFrame = 0;
		endLogicFrame = nowLogicFrame + logicLength;
	}

	public virtual void LogicUpdate()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		switch (Phase)
		{
		case BulletPhase.Normal:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				DoPhase0();
				break;
			case SubStatus.Phase1:
				DoPhase1();
				break;
			case SubStatus.Phase2:
				DoPhase2();
				break;
			}
			break;
		case BulletPhase.Result:
			Phase_Result();
			break;
		case BulletPhase.BackToPool:
			Stop();
			BackToPool();
			break;
		case BulletPhase.Splash:
		case BulletPhase.Boomerang:
			break;
		}
	}

	public override void LateUpdateFunc()
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && Phase == BulletPhase.Normal)
		{
			SubStatus subStatus = _subStatus;
			if ((uint)subStatus <= 2u)
			{
				MoveBullet();
			}
		}
	}

	protected void Phase_Result()
	{
		if (BulletData.n_THROUGH == 0)
		{
			foreach (Transform hit in _hitList)
			{
				CaluDmg(BulletData, hit);
				if (nThrough > 0)
				{
					nThrough--;
				}
			}
		}
		if (BulletData.n_TYPE != 3)
		{
			if (_hitList.Count != 0 || BulletData.f_RANGE != 0f)
			{
				GenerateImpactFx();
			}
			else
			{
				GenerateEndFx();
			}
		}
		Phase = BulletPhase.BackToPool;
	}

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset, 1 << (int)TargetMask);
		if ((bool)raycastHit2D)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		if (isHitBlock || needPlayEndSE || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}

	protected override void GenerateEndFx(bool bPlaySE = true)
	{
		if (bPlaySE && needPlayEndSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
		if (!(FxEnd == "") && FxEnd != null)
		{
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
			if ((bool)raycastHit2D)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
			}
		}
	}

	protected override void MoveBullet()
	{
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, nowPos.vec3, distanceDelta);
	}

	protected virtual bool CheckHitTarget()
	{
		if (Target != null && Mathf.Abs(Vector3.Distance(_vTargetPos, _transform.position)) < 1f)
		{
			return true;
		}
		return false;
	}

	protected void FindTarget()
	{
		if (Target != null || refPBMShoter == null || refPBMShoter.SOB == null)
		{
			return;
		}
		if (refPBMShoter != null && refPBMShoter.SOB.GetSOBType() == 1 && refPBMShoter.SOB.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			GetTargetByPerGameSaveData();
			return;
		}
		if (((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
		{
			Target = NeutralAIS.GetClosetEnemy();
			if (Target != null && Target.AimTransform != null)
			{
				_vTargetPos = Target.AimTransform.position + Target.AimPoint;
				_vTargetPos.z = _transform.position.z;
			}
		}
		else if (((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
		{
			Target = NeutralAIS.GetClosetPlayer();
			if (Target != null && Target.AimTransform != null)
			{
				_vTargetPos = Target.AimTransform.position + Target.AimPoint;
				_vTargetPos.z = _transform.position.z;
			}
		}
		if (Target == null)
		{
			Target = NeutralAIS.GetClosetPvpPlayer();
			if (Target != null && Target.AimTransform != null)
			{
				_vTargetPos = Target.AimTransform.position + Target.AimPoint;
				_vTargetPos.z = _transform.position.z;
			}
		}
		SendTargetMsg();
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_bClearHit = false;
		Target = null;
		_vTargetPos = Vector3.zero;
		_subStatus = SubStatus.Phase0;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	protected virtual void DoPhase0()
	{
		nowLogicFrame++;
		nowPos += new VInt3(_speed * timeDelta.scalar);
		distanceDelta = Vector3.Distance(base.transform.localPosition, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		FindTarget();
		if (nowLogicFrame > endLogicFrame || CheckHitTarget())
		{
			_bClearHit = false;
			nowLogicFrame = 0;
			endLogicFrame = (int)(Phase1StopTime / GameLogicUpdateManager.m_fFrameLen);
			_subStatus = SubStatus.Phase1;
		}
	}

	protected virtual void DoPhase1()
	{
		nowLogicFrame++;
		_capsuleCollider.offset = Vector2.zero;
		if (Target != null)
		{
			StageObjBase stageObjBase = Target as StageObjBase;
			if (stageObjBase != null && (int)stageObjBase.Hp <= 0)
			{
				Target = null;
			}
		}
		FindTarget();
		if (!_bClearHit && (float)nowLogicFrame > ClearHitTime / GameLogicUpdateManager.m_fFrameLen)
		{
			_bClearHit = true;
			_hitList.Clear();
		}
		else
		{
			if (nowLogicFrame <= endLogicFrame)
			{
				return;
			}
			if (Target == null)
			{
				if (Vector3.Distance(_transform.position, MasterTransform.position) >= 0.5f)
				{
					Vector3 vector = -(_transform.position - MasterTransform.position);
					vector.z = 0f;
					Direction = ((vector == Vector3.zero) ? Direction : vector.normalized);
				}
				_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
			}
			else
			{
				if (Vector3.Distance(Target.AimTransform.position + Target.AimPoint, _transform.position) >= 0.5f)
				{
					Vector3 vector2 = Target.AimTransform.position + Target.AimPoint - _transform.position;
					vector2.z = 0f;
					Direction = ((vector2 == Vector3.zero) ? Direction : vector2.normalized);
				}
				_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
			}
			CaluLogicFrame();
			_hitList.Clear();
			_subStatus = SubStatus.Phase2;
		}
	}

	protected virtual void DoPhase2()
	{
		nowLogicFrame++;
		nowPos += new VInt3(_speed * timeDelta.scalar);
		distanceDelta = Vector3.Distance(base.transform.localPosition, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		if (nowLogicFrame > endLogicFrame)
		{
			Phase = BulletPhase.Result;
			_subStatus = SubStatus.Phase3;
		}
	}
}
