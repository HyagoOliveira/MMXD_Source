#define RELEASE
using System;
using StageLib;
using UnityEngine;

public class BS033_ChallengeController : BS033_EventController
{
	public override void LogicUpdate()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.IDLE:
		{
			if (PlayLoopSEOnce)
			{
				PlayBossSE("BossSE", 221);
				PlayLoopSEOnce = false;
			}
			if (AiTimer.GetMillisecond() < EnemyData.n_AI_TIMER)
			{
				break;
			}
			ShuffleArray(ref BulletOrder);
			int[] bulletOrder = BulletOrder;
			foreach (int num2 in bulletOrder)
			{
				if (num2 == 0 || !IsWeaponAvailable(num2))
				{
					continue;
				}
				EnemyWeapons[num2].MagazineRemain = EnemyWeapons[num2].BulletData.n_MAGAZINE;
				if (StageUpdate.gbIsNetGame)
				{
					if (StageUpdate.bIsHost)
					{
						StageUpdate.RegisterSendAndRun(sNetSerialID, num2);
						_mainStatus = MainStatus.IdleWaitNet;
					}
					break;
				}
				switch (num2)
				{
				default:
					_mainStatus = MainStatus.MGUN;
					break;
				case 2:
					_mainStatus = MainStatus.MISSILE;
					break;
				case 3:
					_mainStatus = MainStatus.EGG;
					break;
				}
				break;
			}
			break;
		}
		case MainStatus.MGUN:
		{
			int num = 1;
			if (EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (EnemyWeapons[num].LastUseTimer.IsStarted() && EnemyWeapons[num].LastUseTimer.GetMillisecond() <= EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					break;
				}
				if (_aimIk.solver.target == null)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						_aimIk.solver.target = Target.AimTransform;
					}
				}
				_shootDirection = (_gunShootPointTransform.position - _gunTransform.position).normalized;
				BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _gunShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				EnemyWeapons[num].LastUseTimer.TimerStart();
				EnemyWeapons[num].MagazineRemain -= EnemyWeapons[num].BulletData.n_USE_COST;
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.MISSILE:
		{
			int num = 2;
			if (EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (EnemyWeapons[num].LastUseTimer.IsStarted() && EnemyWeapons[num].LastUseTimer.GetMillisecond() <= EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					break;
				}
				Vector3 vector = Vector3.zero;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					vector = Target.AimTransform.position;
				}
				switch ((int)EnemyWeapons[num].MagazineRemain % 2)
				{
				case 0:
				{
					_shootDirection = (vector - _leftMissileShootPointTransform.position).normalized;
					if (Vector2.Angle(Vector3.right * base.direction, _shootDirection) >= 45f || vector == Vector3.zero)
					{
						_shootDirection = Vector3.right * base.direction;
					}
					BasicBullet basicBullet = (BasicBullet)BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _leftMissileShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					break;
				}
				case 1:
				{
					_shootDirection = (vector - _rightMissileShootPointTransform.position).normalized;
					if (Vector2.Angle(Vector3.right * base.direction, _shootDirection) >= 45f || vector == Vector3.zero)
					{
						_shootDirection = Vector3.right * base.direction;
					}
					BasicBullet basicBullet2 = (BasicBullet)BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _rightMissileShootPointTransform, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					break;
				}
				}
				EnemyWeapons[num].LastUseTimer.TimerStart();
				EnemyWeapons[num].MagazineRemain -= 1f;
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.EGG:
		{
			int num = 3;
			MOB_TABLE value;
			if (EnemyWeapons.Length > 3 && ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT.TryGetValue((int)EnemyWeapons[num].BulletData.f_EFFECT_X, out value) && EnemyWeapons[num].MagazineRemain > 0f)
			{
				if (!EnemyWeapons[num].LastUseTimer.IsStarted() || EnemyWeapons[num].LastUseTimer.GetMillisecond() > EnemyWeapons[num].BulletData.n_FIRE_SPEED)
				{
					EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(value, sNetSerialID + "0");
					if ((bool)enemyControllerBase)
					{
						enemyControllerBase.UpdateEnemyID(value.n_ID);
						enemyControllerBase.SetPositionAndRotation(_assTransform.position + Vector3.right * base.direction * 1.385f, base.direction == -1);
						enemyControllerBase.SetActive(true);
					}
					EnemyWeapons[num].LastUseTimer.TimerStart();
					EnemyWeapons[num].MagazineRemain -= 1f;
				}
			}
			else
			{
				_mainStatus = MainStatus.IDLE;
				AiTimer.TimerStart();
			}
			break;
		}
		case MainStatus.EXPLODE:
		{
			ExcludePlayer(0, -1);
			ExcludeEnemy(0, -1);
			IgnoreGravity = false;
			if (!Controller.Collisions.below)
			{
				break;
			}
			RaycastHit2D raycastHit2D = Controller.SolidMeeting(0f, -1f);
			if (!raycastHit2D)
			{
				break;
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 5f, false);
			_characterMaterial.UpdateTex(0);
			FallingFloor component = raycastHit2D.collider.GetComponent<FallingFloor>();
			if ((bool)component)
			{
				if (_triggerFloorFlag > 0)
				{
					_triggerFloorFlag--;
					component.TriggerFall();
					break;
				}
				Debug.Log("Found FallingFloor");
				OrangeBattleUtility.LockPlayer();
				_aimIk.enabled = false;
				StartCoroutine(BossDieFlow(base.AimTransform));
			}
			else
			{
				Debug.Log("NotFound FallingFloor");
				OrangeBattleUtility.LockPlayer();
				base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.ObstacleLayer;
				Activate = false;
				base.enabled = false;
				_aimIk.enabled = false;
				StartCoroutine(BossDieFlow(base.AimTransform));
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.IdleWaitNet:
			break;
		}
		if (_mainStatus != MainStatus.EXPLODE)
		{
			_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * 1000f);
			UpdateMagazine();
		}
		if (_transform.position.x > _targetXPos)
		{
			_velocity.x = -3000;
		}
		else
		{
			_velocity.x = 0;
		}
		_animator.Play(_animatorHash[(int)_mainStatus]);
	}
}
