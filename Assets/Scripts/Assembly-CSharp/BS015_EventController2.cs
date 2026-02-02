#define RELEASE
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class BS015_EventController2 : BS015_Controller
{
	private Transform eventTarget;

	private bool _startBattle;

	protected override void Awake()
	{
		base.Awake();
		base.AllowAutoAim = false;
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		BaseUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		AI_STATE aiState;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (!(_currentFrame > 1f) || !Controller.BelowInBypassRange)
			{
				break;
			}
			if (_startBattle)
			{
				UpdateRandomState();
				break;
			}
			aiState = AiState;
			if (aiState != AI_STATE.mob_005)
			{
				SetStatus(MainStatus.DashAttack);
			}
			else
			{
				SetStatus(MainStatus.SwingAttack);
			}
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs15_run", _efxpointTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs15_run", _efxpointTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				if (!Controller.BelowInBypassRange)
				{
					SetStatus(MainStatus.Fall);
				}
				else if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
				{
					SetStatus(MainStatus.Hurt);
					PlaySE("BossSE", 211);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Fall:
			if ((bool)Controller.BelowInBypassRange)
			{
				eventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
				if ((bool)eventTarget && Math.Sign(eventTarget.position.x - base.AimTransform.position.x) != base.direction)
				{
					SetStatus(MainStatus.Turn);
				}
				else
				{
					SetStatus(MainStatus.Idle);
				}
				float num = Vector2.Distance(eventTarget.position, base.AimTransform.position);
				Debug.Log("Distance = " + num);
				if (eventTarget.name == "EventSpotBattle")
				{
					base.AllowAutoAim = true;
					_startBattle = true;
				}
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			if (!Controller.BelowInBypassRange)
			{
				SetStatus(MainStatus.Fall);
				break;
			}
			if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
			{
				SetStatus(MainStatus.Hurt);
				PlaySE("BossSE", 211);
			}
			if ((double)_currentFrame > 1.0)
			{
				if (_mainStatus == MainStatus.SwingAttack)
				{
					SetStatus(MainStatus.SwingAttackStart);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs15_atk", _efxpointTransform2, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_maoh_000", _efxpointTransform2, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
				else
				{
					SetStatus(MainStatus.SwingAttack);
				}
			}
			break;
		case MainStatus.Hurt:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)Controller.BelowInBypassRange)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 2.0)
				{
					SetStatus(MainStatus.Turn);
					PlaySE("BossSE", 215);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Turn:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				LeanTween.value(base.direction * 90, -base.direction * 90, 1f).setOnUpdate(delegate(float f)
				{
					ModelTransform.eulerAngles = Vector3.up * f;
				}).setOnComplete((Action)delegate
				{
					base.direction *= -1;
					SetStatus(_mainStatus, SubStatus.Phase2);
				});
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase2:
				SetStatus(MainStatus.Idle);
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Dead:
			break;
		}
		UpdateGravity();
		Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen);
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		_velocityExtra = VInt3.zero;
		FalldownUpdate();
		aiState = AiState;
		if (aiState == AI_STATE.mob_005)
		{
			eventTarget = FreeAutoAimSystem.GetClosestEventSpot(base.AimTransform);
			float num2 = Vector2.Distance(eventTarget.position, base.AimTransform.position);
			Debug.Log("Distance = " + num2);
			if (!_startBattle && eventTarget.name == "EventSpotBattle")
			{
				base.AllowAutoAim = true;
				_startBattle = true;
			}
		}
	}

	protected override void UpdateCollider()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Fall:
		case MainStatus.Dead:
		case MainStatus.Hurt:
		case MainStatus.Turn:
			if (_maceCollideBullet.IsActivate)
			{
				_maceCollideBullet.IsDestroy = true;
			}
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				_maceCollideBullet.UpdateBulletData(EnemyWeapons[1 + (_startBattle ? 2 : 0)].BulletData);
				_maceCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_maceCollideBullet.BackToPool();
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_005)
				{
					_maceCollideBullet.Active(neutralMask);
				}
				else
				{
					_maceCollideBullet.Active(targetMask);
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				_maceCollideBullet.UpdateBulletData(EnemyWeapons[2 + (_startBattle ? 2 : 0)].BulletData);
				_maceCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_maceCollideBullet.BackToPool();
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_005)
				{
					_maceCollideBullet.Active(neutralMask);
				}
				else
				{
					_maceCollideBullet.Active(targetMask);
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (!_startBattle)
		{
			return Hp;
		}
		return BaseHurt(tHurtPassParam);
	}

	public override void BackToPool()
	{
		base.BackToPool();
		PlayBossSE("BossSE", 214);
		PlayBossSE("BossSE", 210);
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		_maceCollideBullet.BackToPool();
		BackToPool();
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		_bDeadCallResult = false;
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_005)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
		}
	}
}
