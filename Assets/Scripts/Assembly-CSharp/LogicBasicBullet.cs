using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicBasicBullet : BulletBase, ILogicUpdate, IManagedLateUpdateBehavior
{
	public enum BulletPhase
	{
		Move = 0,
		BOOM = 1,
		End = 2
	}

	protected Collider2D _hitCollider;

	protected HashSet<Transform> _hitList;

	protected Dictionary<Transform, int> _hitCount;

	protected Rigidbody2D _rigidbody2D;

	protected int _nowLogicFrame;

	protected int _endLogicFrame;

	protected int _logicLength;

	protected VInt _timeDelta;

	protected VInt3 _nowPos;

	protected VInt3 _endPos;

	protected Vector3 _speed = Vector3.zero;

	protected Vector3 _lastPosition = Vector3.zero;

	protected float _distanceDelta;

	protected float _moveDistance;

	protected float _amplitude = 1f;

	protected float _omega = 30f;

	protected VInt3 _linePos;

	protected BulletPhase mainPhase;

	private Vector3 From = Vector3.right;

	protected override void Awake()
	{
		base.Awake();
		_hitList = new HashSet<Transform>();
		_hitCount = new Dictionary<Transform, int>();
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	protected virtual void OnTriggerEnter2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	protected virtual void OnTriggerStay2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	public virtual void OnTriggerHit(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0)
		{
			return;
		}
		if (((uint)BulletData.n_FLAG & (true ? 1u : 0u)) != 0 && ((1 << col.gameObject.layer) & (int)BlockMask) != 0)
		{
			if (!col.GetComponent<StageHurtObj>())
			{
				return;
			}
			needWeaponImpactSE = false;
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

	public override void Hit(Collider2D col)
	{
		if (CheckHitList(ref _hitList, col.transform))
		{
			return;
		}
		_hitList.Add(col.transform);
		int value = -1;
		_hitCount.TryGetValue(col.transform, out value);
		if (value == -1)
		{
			_hitCount.Add(col.transform, value);
		}
		else
		{
			value++;
			_hitCount[col.transform] = value;
			if (BulletData.n_DAMAGE_COUNT > 0 && value >= BulletData.n_DAMAGE_COUNT)
			{
				return;
			}
		}
		if (HitCallback != null)
		{
			HitCallback(col);
		}
		PlayHitFx(col.transform);
		CaluDmg(BulletData, col.transform);
		bool flag = true;
		if (BulletData.n_THROUGH > 0)
		{
			nThrough--;
			if (nThrough > 0)
			{
				flag = false;
			}
		}
		if (flag)
		{
			mainPhase = BulletPhase.End;
		}
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		DoActive(pTarget);
	}

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		DoActive(pTarget);
	}

	protected virtual void DoActive(IAimTarget pTarget)
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		_hitCollider = GetComponent<Collider2D>();
		if (_rigidbody2D != null)
		{
			_rigidbody2D.WakeUp();
		}
		if (activeTracking)
		{
			Target = pTarget;
			NeutralAIS.UpdateAimRange(TrackingData.f_RANGE);
		}
		if (!isSubBullet)
		{
			int n_NUM_SHOOT = BulletData.n_NUM_SHOOT;
			int num = 0;
		}
		ActivateTimer.TimerStart();
		_lastPosition = _transform.localPosition;
		CaluLogicFrame(BulletData.n_SPEED, BulletData.f_DISTANCE, Direction);
		PlayUseFx();
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		if (BulletData.n_THROUGH > 0)
		{
			nThrough = BulletData.n_THROUGH / 100;
		}
		else
		{
			nThrough = 0;
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		mainPhase = BulletPhase.Move;
		Target = null;
		_hitList.Clear();
		_hitCount.Clear();
		if (_rigidbody2D != null)
		{
			_rigidbody2D.Sleep();
		}
		isSubBullet = false;
		_moveDistance = 0f;
	}

	public virtual void LogicUpdate()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		switch (mainPhase)
		{
		case BulletPhase.Move:
			MoveBullet();
			break;
		case BulletPhase.End:
			if (BulletData.n_TYPE != 3 && _hitList.Count == 0 && BulletData.f_RANGE == 0f)
			{
				PlayEndFx();
			}
			BackToPool();
			break;
		case BulletPhase.BOOM:
			break;
		}
	}

	protected virtual void MoveBullet()
	{
		if (CheckTracking())
		{
			MoveTypeTracking();
		}
		else
		{
			switch (BulletData.n_SHOTLINE)
			{
			case 0:
				MoveTypeLine();
				break;
			case 4:
				MoveTypeWave();
				break;
			}
		}
		_lastPosition = _transform.localPosition;
	}

	public virtual void LateUpdateFunc()
	{
		if (CheckTracking())
		{
			Vector3 translation = _speed * Time.deltaTime;
			Vector3 position = _transform.position;
			_transform.Translate(translation);
		}
		else
		{
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, _nowPos.vec3, _distanceDelta);
		}
	}

	protected void CaluLogicFrame(float speed, float distance, Vector3 direction)
	{
		float num = distance / speed;
		_logicLength = (int)(num / GameLogicUpdateManager.m_fFrameLen);
		_timeDelta = new VInt(num / (float)_logicLength);
		_nowPos = new VInt3(_transform.localPosition);
		_endPos = new VInt3(_nowPos.vec3 + direction * distance);
		_speed = new Vector3((_endPos.vec3.x - _nowPos.vec3.x) / num, (_endPos.vec3.y - _nowPos.vec3.y) / num, 0f);
		_distanceDelta = 0f;
		_nowLogicFrame = 0;
		_endLogicFrame = _nowLogicFrame + _logicLength;
	}

	protected void MoveTypeLine()
	{
		_nowLogicFrame++;
		_moveDistance += Vector3.Distance(_lastPosition, _transform.localPosition);
		if (_nowLogicFrame > _endLogicFrame)
		{
			mainPhase = BulletPhase.End;
			return;
		}
		_nowPos += new VInt3(_speed * _timeDelta.scalar);
		float num = Vector3.Distance(base.transform.localPosition, _nowPos.vec3);
		_distanceDelta = num * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	protected void MoveTypeWave()
	{
		_nowLogicFrame++;
		_moveDistance += Vector3.Distance(_lastPosition, _transform.localPosition);
		if (_nowLogicFrame > _endLogicFrame)
		{
			mainPhase = BulletPhase.End;
			return;
		}
		if (_nowLogicFrame == 1)
		{
			_linePos = _nowPos;
		}
		_linePos += new VInt3(_speed * _timeDelta.scalar);
		VInt3 vInt = new VInt3(base.transform.up * _amplitude * Mathf.Sin(_omega * Time.fixedTime) * Time.timeScale);
		_nowPos = _linePos + vInt;
		_distanceDelta = Vector3.Distance(base.transform.localPosition, _nowPos.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	protected void MoveTypeTracking()
	{
		_moveDistance += Vector3.Distance(_lastPosition, _transform.localPosition);
		if (_moveDistance > BulletData.f_DISTANCE)
		{
			mainPhase = BulletPhase.End;
			return;
		}
		if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
		{
			Target = NeutralAIS.GetClosetEnemy();
		}
		if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
		{
			Target = NeutralAIS.GetClosetPlayer();
		}
		if (Target == null && (refPBMShoter.SOB == null || refPBMShoter.SOB.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer))
		{
			Target = NeutralAIS.GetClosetPvpPlayer();
		}
		if (Target != null)
		{
			DoAim(Target);
		}
		_speed = Velocity;
	}

	protected bool CheckTracking()
	{
		if (!activeTracking || TrackingData == null || ActivateTimer.GetMillisecond() < TrackingData.n_BEGINTIME_1 || ActivateTimer.GetMillisecond() >= TrackingData.n_ENDTIME_1)
		{
			return false;
		}
		return true;
	}

	protected void DoAim(IAimTarget target = null)
	{
		if (target != null)
		{
			float num = Vector2.SignedAngle(From, Direction);
			float num2 = Vector2.SignedAngle(From, (target.AimTransform.position + target.AimPoint - _transform.position).normalized);
			float num3 = Mathf.Abs(num2 - num);
			if (num3 > 180f)
			{
				num2 = (float)(-Math.Sign(num2)) * (360f - Mathf.Abs(num2));
				num3 = Mathf.Abs(num2 - num);
			}
			float num4 = num + (float)Math.Sign(num2 - num) * Math.Min(num3, (float)(720 * TrackingData.n_POWER) * 0.01f * Time.deltaTime);
			Direction = new Vector3(Mathf.Cos(num4 * ((float)Math.PI / 180f)), Mathf.Sin(num4 * ((float)Math.PI / 180f)), 0f).normalized;
			_transform.eulerAngles = new Vector3(0f, 0f, num4);
		}
	}

	protected T CreateSubBullet<T>(int skillId) where T : BulletBase
	{
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[skillId];
		if (sKILL_TABLE.n_TYPE != 1)
		{
			return null;
		}
		T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(sKILL_TABLE.s_MODEL);
		WeaponStatus weaponStatus = new WeaponStatus
		{
			nHP = nHp,
			nATK = nOriginalATK,
			nCRI = nOriginalCRI,
			nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck),
			nCriDmgPercent = nCriDmgPercent,
			nReduceBlockPercent = nReduceBlockPercent,
			nWeaponCheck = nWeaponCheck,
			nWeaponType = nWeaponType
		};
		PerBuffManager.BuffStatus tBuffStatus = new PerBuffManager.BuffStatus
		{
			fAtkDmgPercent = fDmgFactor - 100f,
			fCriPercent = fCriFactor - 100f,
			fCriDmgPercent = fCriDmgFactor - 100f,
			fMissPercent = fMissFactor,
			refPBM = refPBMShoter,
			refPS = refPSShoter
		};
		poolObj.UpdateBulletData(sKILL_TABLE, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.isSubBullet = true;
		poolObj.SetBulletAtk(weaponStatus, tBuffStatus);
		poolObj.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
		poolObj.Active(_transform.position, Direction, TargetMask);
		return poolObj;
	}

	protected T CreateSubBullet<T>(SKILL_TABLE skillData) where T : BulletBase
	{
		if (skillData.n_TYPE != 1)
		{
			return null;
		}
		T poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<T>(skillData.s_MODEL);
		WeaponStatus weaponStatus = new WeaponStatus
		{
			nHP = nHp,
			nATK = nOriginalATK,
			nCRI = nOriginalCRI,
			nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck),
			nCriDmgPercent = nCriDmgPercent,
			nReduceBlockPercent = nReduceBlockPercent,
			nWeaponCheck = nWeaponCheck,
			nWeaponType = nWeaponType
		};
		PerBuffManager.BuffStatus tBuffStatus = new PerBuffManager.BuffStatus
		{
			fAtkDmgPercent = fDmgFactor - 100f,
			fCriPercent = fCriFactor - 100f,
			fCriDmgPercent = fCriDmgFactor - 100f,
			fMissPercent = fMissFactor,
			refPBM = refPBMShoter,
			refPS = refPSShoter
		};
		poolObj.UpdateBulletData(skillData, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.isSubBullet = true;
		poolObj.SetBulletAtk(weaponStatus, tBuffStatus);
		poolObj.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
		poolObj.Active(_transform.position, Direction, TargetMask);
		return poolObj;
	}

	protected void PlayUseFx()
	{
		if (!string.IsNullOrEmpty(FxImpact))
		{
			if (MasterTransform != null)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform, Quaternion.identity, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, base.transform, Quaternion.identity, Array.Empty<object>());
			}
		}
	}

	protected void PlayHitFx(Transform target)
	{
		if (string.IsNullOrEmpty(FxImpact))
		{
			return;
		}
		Vector3 p_worldPos = _transform.position;
		if ((bool)target)
		{
			Vector3 vector = target.position - _transform.position;
			float distance = Vector3.Distance(_transform.position, target.position);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, vector, distance);
			if ((bool)raycastHit2D)
			{
				p_worldPos = raycastHit2D.point;
			}
			else
			{
				StageObjParam component = target.GetComponent<StageObjParam>();
				if ((bool)component)
				{
					IAimTarget aimTarget = component.tLinkSOB as IAimTarget;
					if (aimTarget != null && aimTarget.AimTransform != null)
					{
						p_worldPos = aimTarget.AimPosition;
					}
				}
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, p_worldPos, BulletQuaternion, Array.Empty<object>());
		if (isHitBlock || needPlayEndSE || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}

	protected void PlayEndFx()
	{
		if (!string.IsNullOrEmpty(FxImpact))
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxEnd, _transform.position, BulletQuaternion, Array.Empty<object>());
			if (needPlayEndSE)
			{
				PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
			}
		}
	}

	protected float EaseOutCubic(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return end * (value * value * value + 1f) + start;
	}

	public static float EaseOutQuint(float start, float end, float value)
	{
		value -= 1f;
		end -= start;
		return end * (value * value * value * value * value + 1f) + start;
	}
}
