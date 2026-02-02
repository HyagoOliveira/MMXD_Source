using System;
using StageLib;
using UnityEngine;

public class Valentine2021HumanController : EnemyHumanController
{
	private enum IdleAnimation
	{
		SPECMOTION01 = 0,
		SPECMOTION02 = 1,
		SPECMOTION03 = 2,
		SPECMOTION04 = 3,
		SPECMOTION05 = 4,
		SPECMOTION06 = 5,
		SPECMOTION07 = 6,
		SPECMOTION08 = 7,
		SPECMOTION09 = 8,
		SPECMOTION10 = 9,
		SPECMOTION11 = 10,
		SPECMOTION12 = 11,
		MAX_ANI = 12
	}

	[SerializeField]
	private bool CanMove;

	private int EventID = -1;

	private Transform LGlowStick;

	private Transform RGlowStick;

	private Transform LineUpObj;

	private FxBase LineUpFx;

	private ObjInfoBar InfoBar;

	private int AngryFrame = 80;

	private int WaitAngryFrame;

	private int[] _idleAnimationHash;

	public override string[] GetHumanDependAnimations()
	{
		return new string[12]
		{
			"event_enemy_otaku_idle_01_loop", "event_enemy_otaku_idle_02_loop", "event_enemy_otaku_idle_03_loop", "event_enemy_otaku_idle_04_loop", "event_enemy_otaku_idle_05_loop", "event_enemy_otaku_idle_06_loop", "event_enemy_otaku_idle_07_loop", "event_enemy_otaku_idle_08_loop", "event_enemy_otaku_idle_09_loop", "event_enemy_otaku_idle_10_loop",
			"event_enemy_otaku_idle_11_loop", "event_enemy_otaku_idle_12_loop"
		};
	}

	public override void Unlock()
	{
		_unlockReady = true;
		if (CanMove)
		{
			base.AllowAutoAim = true;
			SetColliderEnable(true);
		}
		if (InGame && (int)Hp > 0)
		{
			Activate = true;
		}
	}

	protected override void AwakeJob()
	{
		base.AwakeJob();
		HashIdleAnimation();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_lineup_angry_frame", 8);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_lineup_heart_frame", 8);
	}

	private void HashIdleAnimation()
	{
		_idleAnimationHash = new int[12];
		_idleAnimationHash[0] = Animator.StringToHash("skillclip0");
		_idleAnimationHash[1] = Animator.StringToHash("skillclip1");
		_idleAnimationHash[2] = Animator.StringToHash("skillclip2");
		_idleAnimationHash[3] = Animator.StringToHash("skillclip3");
		_idleAnimationHash[4] = Animator.StringToHash("skillclip4");
		_idleAnimationHash[5] = Animator.StringToHash("skillclip5");
		_idleAnimationHash[6] = Animator.StringToHash("skillclip6");
		_idleAnimationHash[7] = Animator.StringToHash("skillclip7");
		_idleAnimationHash[8] = Animator.StringToHash("skillclip8");
		_idleAnimationHash[9] = Animator.StringToHash("skillclip9");
		_idleAnimationHash[10] = Animator.StringToHash("skillclip10");
		_idleAnimationHash[11] = Animator.StringToHash("skillclip11");
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
			CanMove = false;
			if ((bool)InfoBar)
			{
				InfoBar.gameObject.SetActive(false);
			}
			ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
			break;
		case SubStatus.Phase2:
			WaitAngryFrame = GameLogicUpdateManager.GameFrame + AngryFrame;
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

	protected override void UpdateAnimation()
	{
		base.UpdateAnimation();
		int num = 0;
		MainStatus mainStatus = _mainStatus;
		if (mainStatus != MainStatus.EventIdle)
		{
			return;
		}
		switch (_subStatus)
		{
		case SubStatus.Phase0:
			switch (AiState)
			{
			case AI_STATE.mob_001:
				num = _idleAnimationHash[OrangeBattleUtility.Random(0, 5) + 1];
				break;
			case AI_STATE.mob_002:
				if ((bool)LGlowStick && (bool)RGlowStick)
				{
					LGlowStick.gameObject.SetActive(false);
					RGlowStick.gameObject.SetActive(false);
				}
				num = _idleAnimationHash[OrangeBattleUtility.Random(6, 10) + 1];
				break;
			default:
				num = _idleAnimationHash[0];
				break;
			}
			_animator.Play(num, 0, 0f);
			break;
		case SubStatus.Phase1:
			num = EnemyHumanController._animationHash[10][_isShoot];
			_animator.Play(num, 0, 0f);
			break;
		case SubStatus.Phase2:
			num = EnemyHumanController._animationHash[0][_isShoot];
			_animator.Play(num, 0, 0f);
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
		base.LogicUpdate();
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
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if (!Target)
				{
					break;
				}
				if (Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) > WalkDistance && !CheckMoveFall(_velocity + VInt3.signRight * base.direction * EnemyHumanController.WalkSpeed))
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
				UpdateDirection();
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
				if (PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 2 || PlayerWeapons[WeaponCurrent].WeaponData.n_TYPE == 4)
				{
					if (Target == null || Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) < WalkDistance - 3f)
					{
						SetStatus(MainStatus.Idle);
					}
				}
				else if (Target == null || Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) < WalkDistance - 1f)
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
			base.AllowAutoAim = false;
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, StartMove);
			SetColliderEnable(false);
			DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			LGlowStick = OrangeBattleUtility.FindChildRecursive(base.transform, "HandSubMesh_L_e");
			RGlowStick = OrangeBattleUtility.FindChildRecursive(base.transform, "HandSubMesh_L_e");
			LineUpObj = OrangeBattleUtility.FindChildRecursive(base.transform, "LineUp");
			if ((bool)LineUpObj)
			{
				switch (AiState)
				{
				case AI_STATE.mob_001:
					LineUpFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_lineup_heart_frame", LineUpObj, Quaternion.identity, Array.Empty<object>());
					break;
				}
			}
			InfoBar = _transform.GetComponentInChildren<ObjInfoBar>();
			SetStatus(MainStatus.EventIdle);
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
	}

	public override void SetEventCtrlID(int eventid)
	{
		EventID = eventid;
	}
}
