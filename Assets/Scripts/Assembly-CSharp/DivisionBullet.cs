using System.Collections;
using UnityEngine;

public class DivisionBullet : BasicBullet
{
	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3
	}

	[SerializeField]
	protected int _divisionNum = 2;

	[SerializeField]
	protected float _divideDistance = 0.2f;

	[SerializeField]
	protected float _divisionSpace = 0.5f;

	[SerializeField]
	protected int _divideSpeed = 2;

	[SerializeField]
	protected int _divisionTime = 1000;

	private SubStatus _subStatus;

	protected bool _divisionFlag;

	public float _shiftSpace;

	private OrangeTimer _timer;

	protected override void Awake()
	{
		base.Awake();
		Phase = BulletPhase.Normal;
		_subStatus = SubStatus.Phase0;
		_timer = OrangeTimerManager.GetTimer();
	}

	public override void OnStart()
	{
		base.OnStart();
		_divisionFlag = false;
		if (isSubBullet)
		{
			Phase = BulletPhase.Normal;
			_subStatus = SubStatus.Phase2;
			_timer.TimerStart();
		}
		else
		{
			Phase = BulletPhase.Normal;
			_subStatus = SubStatus.Phase0;
		}
	}

	public override void LateUpdateFunc()
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
				MoveBullet();
				if (Mathf.Abs(Vector3.Distance(beginPosition, _transform.position)) > _divideDistance)
				{
					Velocity = Vector3.zero;
					_subStatus = SubStatus.Phase1;
					_timer.TimerStart();
				}
				break;
			case SubStatus.Phase1:
				if (_timer.GetMillisecond() > 500)
				{
					DivideBullet();
				}
				break;
			case SubStatus.Phase2:
				if (isSubBullet && Mathf.Abs(Vector3.Distance(MasterTransform.position, _transform.position)) >= _shiftSpace)
				{
					Velocity = new Vector3(0f, 0f, 0f);
				}
				if (_timer.GetMillisecond() > _divisionTime)
				{
					Velocity = new Vector3(BulletData.n_SPEED, 0f, 0f);
					_subStatus = SubStatus.Phase3;
				}
				MoveBullet();
				break;
			case SubStatus.Phase3:
				MoveBullet();
				break;
			}
			if (Mathf.Abs(Vector3.Distance(beginPosition, _transform.position)) > BulletData.f_DISTANCE)
			{
				Phase = BulletPhase.Result;
				_subStatus = SubStatus.Phase0;
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

	public override void Hit(Collider2D col)
	{
		base.Hit(col);
	}

	public override void BackToPool()
	{
		_subStatus = SubStatus.Phase0;
		base.BackToPool();
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	protected void DivideBullet()
	{
		_divisionFlag = true;
		PoolManager instance = MonoBehaviourSingleton<PoolManager>.Instance;
		for (int i = 0; i < _divisionNum; i++)
		{
			DivisionBullet poolObj = instance.GetPoolObj<DivisionBullet>(BulletData.s_MODEL);
			poolObj.UpdateBulletData(BulletData, Owner);
			poolObj.BulletLevel = BulletLevel;
			poolObj.fMissFactor = fMissFactor;
			poolObj.refPBMShoter = refPBMShoter;
			poolObj.refPSShoter = refPSShoter;
			poolObj.isSubBullet = true;
			poolObj.HitCount = HitCount;
			poolObj._shiftSpace = (float)(i / 2 + 1) * _divisionSpace;
			poolObj.Active(base.transform, Direction, TargetMask);
			int num = (((i & 1) == 0) ? 1 : (-1));
			poolObj.Velocity = new Vector3((float)(-_divideSpeed) * 0.5f, _divideSpeed * num, 0f);
		}
		Velocity = new Vector3(0f, 0f, 0f);
		_subStatus = SubStatus.Phase2;
		_timer.TimerStart();
	}
}
