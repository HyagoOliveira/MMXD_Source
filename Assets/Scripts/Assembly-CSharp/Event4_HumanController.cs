using System;
using StageLib;
using UnityEngine;

public class Event4_HumanController : EnemyHumanController
{
	[SerializeField]
	private bool CanMove;

	private int EventID = -1;

	private Transform LGlowStick;

	private Transform RGlowStick;

	private Transform LineUpObj;

	private FxBase LineUpFx;

	private ObjInfoBar InfoBar;

	private float ShotAngle = 90f;

	private int WaitAngryFrame;

	[SerializeField]
	private float BornWaitTime = 2f;

	private int BornWaitFrame;

	private bool CanAttack;

	private Vector3 EndPos;

	protected override void AwakeJob()
	{
		base.AwakeJob();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_lineup_angry_frame", 3);
	}

	protected override void SetStatus(int mainStatus, int subStatus = 0)
	{
		base.SetStatus(mainStatus, subStatus);
		MainStatus mainStatus2 = _mainStatus;
		if (mainStatus2 != MainStatus.EventIdle)
		{
			return;
		}
		switch (_subStatus)
		{
		case SubStatus.Phase0:
			_velocity = VInt3.zero;
			ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
			break;
		case SubStatus.Phase2:
			WaitAngryFrame = GameLogicUpdateManager.GameFrame;
			if ((bool)LineUpFx)
			{
				LineUpFx.BackToPool();
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
			}
			if ((bool)LineUpObj)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_lineup_angry_frame", LineUpObj, Quaternion.identity, Array.Empty<object>());
			}
			break;
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate || !BuildDone || PlayerWeapons == null || PlayerWeapons[0].BulletData == null)
		{
			return;
		}
		for (int i = 0; i < PlayerWeapons.Length; i++)
		{
			PlayerWeapons[i].ChargeTimer += GameLogicUpdateManager.m_fFrameLenMS;
			PlayerWeapons[i].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
		}
		if (!CanAttack && GameLogicUpdateManager.GameFrame > BornWaitFrame)
		{
			CanAttack = true;
		}
		BaseLogicUpdate();
		foreach (BulletDetails bullet in bulletList)
		{
			if (bullet.ShootTransform != null)
			{
				CreateBulletDetail(bullet.bulletData, bullet.refWS, bullet.ShootTransform, bullet.nRecordID, bullet.nBulletRecordID);
			}
			else
			{
				CreateBulletDetail(bullet.bulletData, bullet.refWS, bullet.ShootPosition, bullet.nRecordID, bullet.nBulletRecordID);
			}
		}
		bulletList.Clear();
		UpdateMagazine(ref PlayerWeapons);
		MainStatus mainStatus = _mainStatus;
		if ((uint)(mainStatus - 1000) > 1u)
		{
			UpdateAimDirection();
		}
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (CanMove)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						EndPos = Target.Controller.LogicPosition.vec3;
					}
					else
					{
						EndPos = _transform.position + Vector3.right * base.direction * 2f;
					}
				}
				else
				{
					EndPos = _transform.position + Vector3.right * base.direction * 2f;
					_shootDirection = Quaternion.AngleAxis((ShotAngle - 90f) * (float)(-base.direction), Vector3.forward) * (Vector3.right * base.direction);
					_animator.SetFloat(hashDirection, ShotAngle / 180f);
					Target = null;
				}
				if (!CanAttack)
				{
					break;
				}
				if (Mathf.Abs(EndPos.x - _transform.position.x) > WalkDistance && !CheckMoveFall(_velocity + VInt3.signRight * base.direction * EnemyHumanController.WalkSpeed))
				{
					SetStatus(MainStatus.Walk);
				}
				else if (_bDoSummon)
				{
					SetStatus(MainStatus.Summon);
				}
				else if (PlayerWeapons[WeaponCurrent].LastUseTimer.GetMillisecond() > PlayerWeapons[WeaponCurrent].BulletData.n_FIRE_SPEED && PlayerWeapons[WeaponCurrent].MagazineRemain > 0f)
				{
					if (FristAttackWait && PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE != 8)
					{
						_isShoot = 1;
						if (PlayerWeapons[WeaponCurrent].GatlingSpinner != null)
						{
							PlayerWeapons[WeaponCurrent].GatlingSpinner.Activate = true;
						}
						if (!AIHarbingerTimer.IsStarted())
						{
							AIHarbingerTimer.TimerStart();
						}
						if (AIHarbingerTimer.GetMillisecond() > 500)
						{
							AIHarbingerTimer.TimerStop();
							FristAttackWait = false;
						}
					}
					else
					{
						PlayerShootBuster(ref PlayerWeapons[WeaponCurrent], 0);
					}
				}
				else if (PlayerWeapons[WeaponCurrent].MagazineRemain == 0f && _isShoot == 0)
				{
					SetStatus(MainStatus.Crouch);
				}
				if (CanMove)
				{
					UpdateDirection();
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Walk:
			if (CheckMoveFall(_velocity))
			{
				SetStatus(MainStatus.Idle);
				break;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (CanMove)
				{
					if ((bool)Target)
					{
						EndPos = Target.Controller.LogicPosition.vec3;
					}
					else
					{
						EndPos = _transform.position + Vector3.right * base.direction * 2f;
					}
				}
				else
				{
					EndPos = _transform.position + Vector3.right * base.direction * 2f;
					Target = null;
				}
				if (PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 2 || PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 4)
				{
					if (Target == null || Mathf.Abs(EndPos.x - _transform.position.x) < WalkDistance - 3f)
					{
						SetStatus(MainStatus.Idle);
					}
				}
				else if (Target == null || Mathf.Abs(EndPos.x - _transform.position.x) < WalkDistance - 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Crouch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && PlayerWeapons[WeaponCurrent].MagazineRemain > 0f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					FristAttackWait = true;
				}
				break;
			}
			break;
		case MainStatus.EventIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (CanMove)
				{
					SetStatus(MainStatus.EventIdle, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.EventIdle);
				}
				break;
			case SubStatus.Phase2:
				if (WaitAngryFrame < GameLogicUpdateManager.GameFrame)
				{
					if ((bool)InfoBar)
					{
						InfoBar.gameObject.SetActive(true);
					}
					SetColliderEnable(true);
					base.AllowAutoAim = true;
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.EventWalk:
			if (_subStatus == SubStatus.Phase0 && _currentFrame > 1f)
			{
				SetStatus(_mainStatus, SubStatus.Phase1);
			}
			break;
		}
	}

	public override void SetActiveReal(bool isActive)
	{
		base.SetActiveReal(isActive);
		CanMove = false;
		if (isActive)
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
			DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			LGlowStick = OrangeBattleUtility.FindChildRecursive(base.transform, "HandSubMesh_L_e");
			RGlowStick = OrangeBattleUtility.FindChildRecursive(base.transform, "HandSubMesh_L_e");
			LineUpObj = OrangeBattleUtility.FindChildRecursive(base.transform, "LineUp");
			if ((bool)LineUpObj && AiState != 0)
			{
				int num = 1;
			}
			InfoBar = _transform.GetComponentInChildren<ObjInfoBar>();
			UpdateDirection(base.direction);
			CanAttack = false;
			BornWaitFrame = GameLogicUpdateManager.GameFrame + (int)(BornWaitTime * 20f);
			EnableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			SetStatus(MainStatus.Idle);
		}
		else
		{
			if ((bool)LineUpFx)
			{
				LineUpFx.BackToPool();
			}
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		base.SetPositionAndRotation(pos, bBack);
	}

	public void StartMove(EventManager.StageEventCall tStageEventCall)
	{
		if (EventID == -1)
		{
			return;
		}
		int nID = tStageEventCall.nID;
		if ((bool)LGlowStick && (bool)RGlowStick)
		{
			LGlowStick.gameObject.SetActive(false);
			RGlowStick.gameObject.SetActive(false);
		}
		if (nID != EventID)
		{
			return;
		}
		CanMove = true;
		EnableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
		for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
		{
			if (StageUpdate.runEnemys[i].mEnemy.gameObject.GetInstanceID() == base.gameObject.GetInstanceID())
			{
				StageUpdate.runEnemys[i].nEnemyBitParam = StageUpdate.runEnemys[i].nEnemyBitParam | 1;
			}
		}
		SetStatus(MainStatus.EventIdle);
	}

	public override void SetEventCtrlID(int eventid)
	{
		EventID = eventid;
	}

	public override void SetStageCustomParams(int nStageCustomType, int[] nStageCustomParams)
	{
		if (nStageCustomType == StageEnemy.GetStageCustomParamsType(EnemyData.s_MODEL))
		{
			ShotAngle = nStageCustomParams[0];
		}
	}
}
