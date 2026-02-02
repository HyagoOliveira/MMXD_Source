using System;
using StageLib;
using UnityEngine;

public class BS072_Controller : BS015_Controller
{
	protected bool _bShowExplodeBG = true;

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		if (AiState == AI_STATE.mob_001)
		{
			_bShowExplodeBG = false;
			_bDeadCallResult = false;
			BattleInfoUI.Instance.IsBossAppear = true;
		}
		else
		{
			_bShowExplodeBG = true;
			_bDeadCallResult = true;
		}
	}

	protected override void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Fall:
			_velocity.x = 0;
			PlayBossSE("BossSE", 213);
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE", 214);
				PlayBossSE("BossSE", 209);
				_velocity.x = 0;
				break;
			case SubStatus.Phase1:
				_velocity.x = Math.Sign(base.direction) * DashSpeed;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			PlayBossSE("BossSE", 214);
			PlayBossSE("BossSE", 209);
			_velocity.x = Math.Sign(base.direction) * MoveSpeed;
			break;
		case MainStatus.Dead:
			PlayBossSE("BossSE", 210);
			PlayBossSE("BossSE", 214);
			_velocity.x = 0;
			_collideBullet.BackToPool();
			OrangeBattleUtility.LockPlayer();
			PlayBossSE("HitSE", 103);
			PlayBossSE("HitSE", 104);
			StartCoroutine(MBossExplosionSE());
			if (base.AimTransform != null)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE2", base.AimTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE2", new Vector3(base.transform.position.x, base.transform.position.y + 1f, base.transform.position.z), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			if (_bShowExplodeBG)
			{
				if (_bDeadCallResult)
				{
					BattleInfoUI.Instance.ShowExplodeBG(base.gameObject, false);
				}
				else
				{
					BattleInfoUI.Instance.ShowExplodeBG(base.gameObject, false, false);
				}
			}
			else
			{
				ExplodeCustom();
			}
			break;
		case MainStatus.Hurt:
			PlayBossSE("BossSE", 210);
			PlayBossSE("BossSE", 214);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = new VInt3(-_velocity.x, IntMath.Abs(_velocity.x) * 2, 0);
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Turn:
			_velocity.x = 0;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
	}

	protected void ExplodeCustom()
	{
		StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, 3.2f, delegate
		{
		}, delegate
		{
			if (base.gameObject != null)
			{
				EnemyControllerBase component = base.gameObject.GetComponent<EnemyControllerBase>();
				if ((bool)component)
				{
					component.SetActive(false);
				}
				else
				{
					base.gameObject.SetActive(false);
				}
			}
			StartCoroutine(StageResManager.TweenFloatCoroutine(1f, 0f, 3.2f, delegate
			{
			}, delegate
			{
				if (base.gameObject != null)
				{
					EnemyControllerBase component2 = base.gameObject.GetComponent<EnemyControllerBase>();
					if ((bool)component2)
					{
						component2.DeadPlayCompleted = true;
						if (ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(component2.EnemyData))
						{
							StageUpdate stageUpdate = StageResManager.GetStageUpdate();
							if (stageUpdate != null && stageUpdate.bIsHaveEventStageEnd)
							{
								BattleInfoUI.Instance.SwitchOptionBtn(true);
							}
						}
					}
				}
			}));
		}));
	}
}
