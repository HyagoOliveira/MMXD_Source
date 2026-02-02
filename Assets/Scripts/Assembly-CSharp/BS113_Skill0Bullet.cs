using System;
using UnityEngine;

public class BS113_Skill0Bullet : SeaHorseSkill01Bullet
{
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
					float num2 = Mathf.Cos((float)Math.PI / 6f);
					if (Velocity.x > 0f)
					{
						Velocity = new Vector3((float)(-BulletData.n_SPEED) * num2, (float)(BulletData.n_SPEED * _upDirection) * (1f - num2), 0f);
					}
					else
					{
						Velocity = new Vector3((float)BulletData.n_SPEED * num2, (float)(BulletData.n_SPEED * _upDirection) * (1f - num2), 0f);
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
					CreateLinkBullet(Vector3.up + Vector3.right);
					CreateLinkBullet(Vector3.up - Vector3.right);
					CreateLinkBullet(-Vector3.up + Vector3.right);
					CreateLinkBullet(-Vector3.up - Vector3.right);
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
				CreateLinkBullet(Vector3.up + Vector3.right);
				CreateLinkBullet(Vector3.up - Vector3.right);
				CreateLinkBullet(-Vector3.up + Vector3.right);
				CreateLinkBullet(-Vector3.up - Vector3.right);
			}
			Stop();
			BackToPool();
			break;
		case BulletPhase.Splash:
		case BulletPhase.Boomerang:
			break;
		}
	}

	protected void CreateLinkBullet(Vector3 direction)
	{
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_LINK_SKILL];
		if (sKILL_TABLE.n_TYPE == 1)
		{
			BasicBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>(sKILL_TABLE.s_MODEL);
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
			poolObj.transform.SetPositionAndRotation(_transform.position, Quaternion.identity);
			poolObj.Active(_transform.position, direction, TargetMask);
		}
	}
}
