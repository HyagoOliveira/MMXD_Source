using UnityEngine;

public class OceanSpiritBullet : BasicBullet
{
	public enum PartType
	{
		Head = 0,
		Body = 1,
		Tail = 2
	}

	protected OrangeTimer _timer;

	public PartType _partType;

	private float _frontDistance;

	private OceanSpiritBullet _frontBullet;

	private OceanSpiritBullet _backBullet;

	protected override void Awake()
	{
		base.Awake();
		_timer = OrangeTimerManager.GetTimer();
	}

	public override void OnStart()
	{
		base.OnStart();
		_timer.TimerStart();
		oldPos = _transform.position;
		if (_partType != 0)
		{
			return;
		}
		OceanSpiritBullet oceanSpiritBullet = this;
		int n_LINK_SKILL = oceanSpiritBullet.BulletData.n_LINK_SKILL;
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
		for (int i = 0; i < sKILL_TABLE.n_MAGAZINE; i++)
		{
			oceanSpiritBullet = CreateLinkBullet(n_LINK_SKILL, PartType.Body, oceanSpiritBullet);
			if (oceanSpiritBullet == null)
			{
				break;
			}
		}
		if ((bool)oceanSpiritBullet)
		{
			n_LINK_SKILL = oceanSpiritBullet.BulletData.n_LINK_SKILL;
			CreateLinkBullet(n_LINK_SKILL, PartType.Tail, oceanSpiritBullet);
		}
	}

	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nNetID);
		if (nDirection == 1)
		{
			_transform.localScale = Vector3.one;
		}
		else
		{
			_transform.localScale = new Vector3(1f, -1f, -1f);
		}
	}

	public void SetPartType(PartType part)
	{
		_partType = part;
		if (_partType == PartType.Head)
		{
			_frontDistance = 0f;
		}
		else if (_partType == PartType.Body)
		{
			if ((bool)_frontBullet && _frontBullet._partType == PartType.Head)
			{
				_frontDistance = 1f;
			}
			else
			{
				_frontDistance = 0.75f;
			}
		}
		else
		{
			_frontDistance = 0.85f;
		}
	}

	public override void LateUpdateFunc()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		Transform transform = null;
		switch (Phase)
		{
		case BulletPhase.Normal:
			if (_timer.GetMillisecond() > 200)
			{
				if (_partType == PartType.Head)
				{
					MoveBullet();
				}
				else
				{
					FollowFront();
				}
				float num = BulletData.f_DISTANCE;
				if (FreeDISTANCE > 0f)
				{
					num = FreeDISTANCE;
				}
				if (Mathf.Abs(Vector3.Distance(beginPosition, _transform.position)) > num)
				{
					Phase = BulletPhase.Result;
				}
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
					if (transform == null)
					{
						transform = hit;
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
			if (_backBullet != null)
			{
				_backBullet._frontBullet = _frontBullet;
				if (transform != null && (_partType == PartType.Head || _frontBullet == null))
				{
					IAimTarget component = transform.GetComponent<IAimTarget>();
					_backBullet.UpdateDirection(component);
				}
			}
			Phase = BulletPhase.BackToPool;
			break;
		case BulletPhase.BackToPool:
			Stop();
			BackToPool();
			break;
		}
		if (_partType != 0 || !activeTracking || TrackingData == null || ActivateTimer.GetMillisecond() < TrackingData.n_BEGINTIME_1 || ActivateTimer.GetMillisecond() >= TrackingData.n_ENDTIME_1)
		{
			return;
		}
		if (((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
		{
			if (Target == null)
			{
				Target = NeutralAIS.GetClosetEnemy();
			}
			if (Target != null)
			{
				DoAim(Target);
			}
		}
		else if (((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
		{
			if (Target == null || (Target as OrangeCharacter != null && !(Target as OrangeCharacter).Controller.enabled))
			{
				Target = NeutralAIS.GetClosetPlayer();
			}
			if (Target != null)
			{
				DoAim(Target);
			}
		}
	}

	private void FollowFront()
	{
		if (_frontBullet == null)
		{
			MoveBullet();
			return;
		}
		float num = Mathf.Abs(Vector3.Distance(_frontBullet.transform.position, _transform.position));
		if (num > _frontDistance)
		{
			Vector3 normalized = (_frontBullet.transform.position - _transform.position).normalized;
			float z = Vector2.SignedAngle(Vector2.right, normalized);
			_transform.position += (num - _frontDistance) * normalized;
			_transform.eulerAngles = new Vector3(0f, 0f, z);
		}
	}

	public void UpdateDirection(IAimTarget target)
	{
		if (target != null && target.AimTransform != null)
		{
			Direction = (target.AimTransform.position + target.AimPoint - _transform.position).normalized;
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
		_partType = PartType.Head;
		_timer.TimerStop();
		_frontBullet = null;
		_backBullet = null;
	}

	private OceanSpiritBullet CreateLinkBullet(int linkSkillId, PartType partType, OceanSpiritBullet frontBullet)
	{
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[linkSkillId];
		if (sKILL_TABLE.n_TYPE != 1)
		{
			return null;
		}
		OceanSpiritBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<OceanSpiritBullet>(sKILL_TABLE.s_MODEL);
		poolObj.UpdateBulletData(sKILL_TABLE, Owner);
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
		frontBullet._backBullet = poolObj;
		poolObj._frontBullet = frontBullet;
		poolObj.SetBulletAtk(weaponStatus, buffStatus);
		poolObj.SetPartType(partType);
		poolObj.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
		poolObj.Active(_transform.position, Direction, TargetMask);
		return poolObj;
	}
}
