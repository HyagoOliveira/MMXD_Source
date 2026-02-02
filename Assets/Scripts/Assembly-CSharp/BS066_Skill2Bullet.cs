using System;
using UnityEngine;

public class BS066_Skill2Bullet : BasicBullet, ILogicUpdate
{
	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3
	}

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

	[SerializeField]
	private int DefaultTrackTimes = 2;

	private int TrackTimes;

	[SerializeField]
	private float NextTrackTime = 1.5f;

	[SerializeField]
	private bool LastTrackStop = true;

	[SerializeField]
	private string[] sRestartSE;

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

	public override void Active(Transform pTransform, Vector3 EndPos, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, EndPos, pTargetMask, pTarget);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		if (pTarget != null)
		{
			Target = pTarget;
		}
		NeutralAIS.UpdateAimRange(BulletData.f_DISTANCE * 3f);
		CaluLogicFrame(EndPos);
		TrackTimes = DefaultTrackTimes;
	}

	public override void Active(Vector3 pPos, Vector3 EndPos, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, EndPos, pTargetMask, pTarget);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		if (pTarget != null)
		{
			Target = pTarget;
		}
		NeutralAIS.UpdateAimRange(BulletData.f_DISTANCE * 3f);
		CaluLogicFrame(EndPos);
		TrackTimes = DefaultTrackTimes;
	}

	protected void CaluLogicFrame(Vector3 EndPos)
	{
		_transform.position = new Vector3(_transform.position.x, _transform.position.y, 0f);
		endPos = new VInt3(EndPos);
		nowPos = new VInt3(_transform.position);
		float num = Vector3.Distance(nowPos.vec3, endPos.vec3) / (float)BulletData.n_SPEED;
		logicLength = (int)(num / GameLogicUpdateManager.m_fFrameLen);
		timeDelta = new VInt(num / (float)logicLength);
		_speed = (endPos.vec3 - nowPos.vec3).normalized * BulletData.n_SPEED;
		nowLogicFrame = 0;
		endLogicFrame = nowLogicFrame + logicLength;
	}

	public virtual void LogicUpdate()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		float num = BulletData.f_DISTANCE;
		switch (Phase)
		{
		case BulletPhase.Normal:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				nowLogicFrame++;
				nowPos += new VInt3(_speed * timeDelta.scalar);
				distanceDelta = Vector3.Distance(_transform.position, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
				if (FreeDISTANCE > 0f)
				{
					num = FreeDISTANCE;
				}
				if (nowLogicFrame > endLogicFrame || CheckHitTarget())
				{
					_bClearHit = false;
					nowLogicFrame = 0;
					endLogicFrame = (int)(NextTrackTime / GameLogicUpdateManager.m_fFrameLen);
					distanceDelta = 0f;
					_subStatus = SubStatus.Phase1;
				}
				break;
			case SubStatus.Phase1:
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
				if (!_bClearHit && (float)nowLogicFrame > 0.5f / GameLogicUpdateManager.m_fFrameLen)
				{
					_bClearHit = true;
					_hitList.Clear();
				}
				else
				{
					if (nowLogicFrame <= endLogicFrame)
					{
						break;
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
					if (Target != null)
					{
						CaluLogicFrame(Target.AimTransform.position + Target.AimPoint);
						_hitList.Clear();
						_subStatus = SubStatus.Phase2;
						if (sRestartSE.Length >= 2)
						{
							PlaySE(sRestartSE[0], sRestartSE[1]);
						}
						TrackTimes--;
					}
				}
				break;
			case SubStatus.Phase2:
				nowLogicFrame++;
				nowPos += new VInt3(_speed * timeDelta.scalar);
				distanceDelta = Vector3.Distance(_transform.position, nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
				if (FreeDISTANCE > 0f)
				{
					num = FreeDISTANCE;
				}
				else
				{
					heapDistance += Vector2.Distance(lastPosition, _transform.position);
				}
				lastPosition = _transform.position;
				if (nowLogicFrame > endLogicFrame)
				{
					if (TrackTimes > 0)
					{
						_bClearHit = false;
						nowLogicFrame = 0;
						endLogicFrame = (int)(NextTrackTime / GameLogicUpdateManager.m_fFrameLen);
						_subStatus = SubStatus.Phase1;
					}
					else if (LastTrackStop)
					{
						Phase = BulletPhase.Result;
					}
					else if (heapDistance > num)
					{
						Phase = BulletPhase.Result;
					}
				}
				break;
			}
			break;
		case BulletPhase.Result:
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

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset);
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
			if (needPlayEndSE)
			{
				PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
			}
		}
	}

	protected override void MoveBullet()
	{
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		_transform.position = Vector3.MoveTowards(_transform.position, nowPos.vec3, distanceDelta);
	}

	protected bool CheckHitTarget()
	{
		if (Target != null && Mathf.Abs(Vector3.Distance(_vTargetPos, _transform.position)) < 1f)
		{
			return true;
		}
		return false;
	}

	protected void FindTarget()
	{
		if (Target != null)
		{
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

	public void SetNextTrackTime(float time)
	{
		NextTrackTime = time;
	}
}
