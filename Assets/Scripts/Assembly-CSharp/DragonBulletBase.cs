using UnityEngine;

public class DragonBulletBase<T> : BasicBullet where T : DragonBulletBase<T>
{
	public enum Partition
	{
		Head = 0,
		Body = 1,
		Tail = 2
	}

	protected float DelayTime;

	protected float DistanceToPrevBullet;

	private PoolManager _poolManager;

	private OrangeTimer _timer;

	protected Partition PartitionType { get; private set; }

	protected T PrevBullet { get; private set; }

	protected T NextBullet { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		_poolManager = MonoBehaviourSingleton<PoolManager>.Instance;
		_timer = OrangeTimerManager.GetTimer();
		DelayTime = 200f;
	}

	public override void OnStart()
	{
		base.OnStart();
		_timer.TimerStart();
		oldPos = _transform.position;
		if (PartitionType != 0)
		{
			return;
		}
		T val = this as T;
		int n_LINK_SKILL = val.BulletData.n_LINK_SKILL;
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
		for (int i = 0; i < sKILL_TABLE.n_MAGAZINE; i++)
		{
			val = CreateLinkBullet(sKILL_TABLE, Partition.Body, val);
			if (val == null)
			{
				break;
			}
		}
		if ((bool)val)
		{
			n_LINK_SKILL = val.BulletData.n_LINK_SKILL;
			sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL];
			CreateLinkBullet(sKILL_TABLE, Partition.Tail, val);
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

	public virtual void SetDistance()
	{
		DistanceToPrevBullet = 1f;
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
			if ((float)_timer.GetMillisecond() > DelayTime)
			{
				if (PartitionType == Partition.Head)
				{
					MoveBullet();
				}
				else
				{
					FollowPrevBullet();
				}
				float num = BulletData.f_DISTANCE;
				if (FreeDISTANCE > 0f)
				{
					num = FreeDISTANCE;
				}
				if (Vector3.Distance(beginPosition, _transform.position) > num)
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
			if (NextBullet != null)
			{
				NextBullet.PrevBullet = PrevBullet;
				if (transform != null && (PartitionType == Partition.Head || PrevBullet == null))
				{
					IAimTarget component = transform.GetComponent<IAimTarget>();
					NextBullet.UpdateDirection(component);
				}
			}
			Phase = BulletPhase.BackToPool;
			break;
		case BulletPhase.BackToPool:
			Stop();
			BackToPool();
			break;
		}
		if (PartitionType != 0 || !activeTracking || TrackingData == null || ActivateTimer.GetMillisecond() < TrackingData.n_BEGINTIME_1 || ActivateTimer.GetMillisecond() >= TrackingData.n_ENDTIME_1)
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

	private void FollowPrevBullet()
	{
		if (PrevBullet == null)
		{
			MoveBullet();
			return;
		}
		Vector3 vector = PrevBullet.transform.position - _transform.position;
		float magnitude = vector.magnitude;
		if (magnitude > DistanceToPrevBullet)
		{
			Vector3 normalized = vector.normalized;
			float z = Vector2.SignedAngle(Vector2.right, normalized);
			_transform.position += (magnitude - DistanceToPrevBullet) * normalized;
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
		_timer.TimerStop();
		PartitionType = Partition.Head;
		PrevBullet = null;
		NextBullet = null;
	}

	private T CreateLinkBullet(SKILL_TABLE skillTable, Partition partitionType, T prevBullet)
	{
		if (skillTable.n_TYPE != 1)
		{
			return null;
		}
		T poolObj = _poolManager.GetPoolObj<T>(skillTable.s_MODEL);
		poolObj.UpdateBulletData(skillTable, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
		poolObj.SetBulletAtk(new WeaponStatus
		{
			nHP = nHp,
			nATK = nOriginalATK,
			nCRI = nOriginalCRI,
			nHIT = nHit - refPSShoter.GetAddStatus(8, nWeaponCheck),
			nCriDmgPercent = nCriDmgPercent,
			nReduceBlockPercent = nReduceBlockPercent,
			nWeaponCheck = nWeaponCheck,
			nWeaponType = nWeaponType
		}, new PerBuffManager.BuffStatus
		{
			fAtkDmgPercent = fDmgFactor - 100f,
			fCriPercent = fCriFactor - 100f,
			fCriDmgPercent = fCriDmgFactor - 100f,
			fMissPercent = fMissFactor,
			refPBM = refPBMShoter,
			refPS = refPSShoter
		});
		prevBullet.NextBullet = poolObj;
		poolObj.PrevBullet = prevBullet;
		poolObj.PartitionType = partitionType;
		poolObj.SetDistance();
		poolObj.Active(_transform.position, Direction, TargetMask);
		return poolObj;
	}
}
