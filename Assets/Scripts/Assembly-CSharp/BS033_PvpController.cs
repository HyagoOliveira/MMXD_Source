using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using StageLib;
using UnityEngine;

public class BS033_PvpController : BS033_EventController
{
	public override void LogicUpdate()
	{
		if (!Activate || !_enemyAutoAimSystem)
		{
			return;
		}
		BaseUpdate();
		switch (_mainStatus)
		{
		case MainStatus.IDLE:
			if (visible)
			{
				if (PlayLoopSEOnce)
				{
					PlaySE("BossSE", 221);
					PlayLoopSEOnce = false;
				}
			}
			else if (!PlayLoopSEOnce)
			{
				PlaySE("BossSE", 222, true);
				PlayLoopSEOnce = true;
			}
			if (AiTimer.GetMillisecond() < EnemyData.n_AI_TIMER)
			{
				break;
			}
			ShuffleArray(ref BulletOrder);
			if (!IsWeaponAvailable(1))
			{
				break;
			}
			EnemyWeapons[1].MagazineRemain = EnemyWeapons[1].BulletData.n_MAGAZINE;
			if (StageUpdate.gbIsNetGame)
			{
				if (StageUpdate.bIsHost)
				{
					StageUpdate.RegisterSendAndRun(sNetSerialID, 1);
					_mainStatus = MainStatus.IdleWaitNet;
				}
			}
			else
			{
				_mainStatus = MainStatus.MISSILE;
			}
			break;
		case MainStatus.MISSILE:
		{
			int num = 1;
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
		Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		int num = nSet - 1;
		int num2 = 2;
		_mainStatus = MainStatus.MISSILE;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		return BaseHurt(tHurtPassParam);
	}
}
