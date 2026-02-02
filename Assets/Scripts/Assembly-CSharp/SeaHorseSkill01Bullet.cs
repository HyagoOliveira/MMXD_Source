using System;
using System.Collections;
using UnityEngine;

public class SeaHorseSkill01Bullet : BasicBullet
{
	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3
	}

	protected SubStatus _subStatus;

	protected int _direction;

	protected int _upDirection;

	protected int _turnCount;

	protected OrangeTimer _timer;

	protected override void Awake()
	{
		base.Awake();
		_timer = OrangeTimerManager.GetTimer();
	}

	public bool IsIdle()
	{
		if (Phase == BulletPhase.Normal && _subStatus == SubStatus.Phase0)
		{
			return true;
		}
		return false;
	}

	public void Shoot()
	{
		if (Phase == BulletPhase.Normal && _subStatus == SubStatus.Phase0)
		{
			PlayUseSE();
			PlaySE("BossSE02", "bs013_toxic06");
			_subStatus = SubStatus.Phase1;
		}
	}

	protected void MyMoveBullet()
	{
		oldPos = _transform.position;
		Vector3 translation = Velocity * Time.deltaTime;
		_transform.Translate(translation);
		if (BulletData.n_SHOTLINE == 3 || BulletData.n_SHOTLINE == 4)
		{
			_transform.position += base.transform.up * amplitude * Mathf.Sin(omega * Time.fixedTime) * Time.timeScale;
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
			case SubStatus.Phase1:
				_direction = ((Direction.x > 0f) ? 1 : (-1));
				_upDirection = ((Direction.x > 0f) ? 1 : (-1));
				if ((bool)Physics2D.Raycast(_transform.position, Vector2.right * _direction, DefaultRadiusX + 0.3f, BlockMask))
				{
					PlaySE("BossSE02", "bs013_toxic07");
					_subStatus = SubStatus.Phase2;
					_turnCount++;
					_direction *= -1;
					_timer.TimerStart();
					float num3 = Mathf.Cos((float)Math.PI / 6f);
					if (Velocity.x > 0f)
					{
						Velocity = new Vector3((float)(-BulletData.n_SPEED) * num3, (float)(BulletData.n_SPEED * _upDirection) * (1f - num3), 0f);
					}
					else
					{
						Velocity = new Vector3((float)BulletData.n_SPEED * num3, (float)(BulletData.n_SPEED * _upDirection) * (1f - num3), 0f);
					}
				}
				MyMoveBullet();
				break;
			case SubStatus.Phase2:
				if ((bool)Physics2D.Raycast(_transform.position, Vector2.right * _direction, DefaultRadiusX + 0.3f, BlockMask))
				{
					PlaySE("BossSE02", "bs013_toxic07");
					_turnCount++;
					_direction *= -1;
					_timer.TimerStart();
					float num = Mathf.Cos((float)Math.PI / 6f);
					if (Velocity.x > 0f)
					{
						Velocity = new Vector3((float)(-BulletData.n_SPEED) * num, (float)(BulletData.n_SPEED * _upDirection) * (1f - num), 0f);
					}
					else
					{
						Velocity = new Vector3((float)BulletData.n_SPEED * num, (float)(BulletData.n_SPEED * _upDirection) * (1f - num), 0f);
					}
				}
				if (_timer.GetMillisecond() > 350)
				{
					_timer.TimerStop();
					int num2 = ((Velocity.x > 0f) ? 1 : (-1));
					CreateLinkBullet((float)(4 * num2) * Direction.x);
					CreateLinkBullet((float)(8 * num2) * Direction.x);
				}
				MyMoveBullet();
				break;
			case SubStatus.Phase0:
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
			if (isHitBlock)
			{
				CreateLinkBullet(4f);
				CreateLinkBullet(8f);
				CreateLinkBullet(-4f);
				CreateLinkBullet(-8f);
			}
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
		_turnCount = 0;
		base.BackToPool();
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	protected virtual void CreateLinkBullet(float speed)
	{
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_LINK_SKILL];
		if (sKILL_TABLE.n_TYPE == 1)
		{
			DropBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<DropBullet>(sKILL_TABLE.s_MODEL);
			poolObj.UpdateBulletData(sKILL_TABLE, Owner);
			poolObj.BulletLevel = BulletLevel;
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
			poolObj.SetSpeed(Vector3.right * speed);
			poolObj.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
			poolObj.Active(_transform.position, Vector3.right, TargetMask);
		}
	}
}
