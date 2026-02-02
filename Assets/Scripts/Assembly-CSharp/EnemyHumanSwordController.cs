using System.Collections.Generic;
using UnityEngine;

public class EnemyHumanSwordController : EnemyHumanController
{
	protected enum SwordAnimateId
	{
		ANI_STAND_SLASH1_START = 0,
		ANI_STAND_SLASH2_START = 1,
		ANI_STAND_SLASH3_START = 2,
		ANI_STAND_SLASH4_START = 3,
		ANI_STAND_SLASH5_START = 4,
		ANI_STAND_SLASH1_END = 5,
		ANI_STAND_SLASH2_END = 6,
		ANI_STAND_SLASH3_END = 7,
		ANI_STAND_SLASH4_END = 8,
		ANI_STAND_SLASH5_END = 9,
		MAX_ANI = 10
	}

	protected int[] _swordAnimationHash;

	protected int SlashCount;

	protected bool Slashing;

	private List<SlashDetails> _listSlashCache = new List<SlashDetails>();

	protected float _prevFrame;

	protected override void AwakeJob()
	{
		_shootTimer = OrangeTimerManager.GetTimer();
		EnemyHumanController.WalkSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerWalkSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		EnemyHumanController.JumpSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerJumpSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		EnemyHumanController.DashSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerDashSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AutoReloadDelay = OrangeConst.AUTO_RELOAD;
		AutoReloadPercent = (float)OrangeConst.AUTO_RELOAD_PERCENT * 0.01f;
		EnemyHumanController.UpdateAnimationHash();
		HashSwordAnimation();
	}

	protected virtual void HashSwordAnimation()
	{
		_swordAnimationHash = new int[10];
		_swordAnimationHash[0] = Animator.StringToHash("melee_stand_atk1_start");
		_swordAnimationHash[5] = Animator.StringToHash("melee_stand_atk1_end");
		_swordAnimationHash[1] = Animator.StringToHash("melee_stand_atk2_start");
		_swordAnimationHash[6] = Animator.StringToHash("melee_stand_atk2_end");
		_swordAnimationHash[2] = Animator.StringToHash("melee_stand_atk3_start");
		_swordAnimationHash[7] = Animator.StringToHash("melee_stand_atk3_end");
		_swordAnimationHash[3] = Animator.StringToHash("melee_stand_atk4_start");
		_swordAnimationHash[8] = Animator.StringToHash("melee_stand_atk4_end");
		_swordAnimationHash[4] = Animator.StringToHash("melee_stand_atk5_start");
		_swordAnimationHash[9] = Animator.StringToHash("melee_stand_atk5_end");
	}

	protected override void InitializeWeaponStruct(ref WeaponStruct[] pWeaponStructs)
	{
		Transform[][] array = new Transform[pWeaponStructs.Length][];
		if (pWeaponStructs[0] == null)
		{
			return;
		}
		pWeaponStructs[0].ShootTransform = new Transform[10];
		pWeaponStructs[0].WeaponMesh = new CharacterMaterial[10];
		if (pWeaponStructs[0].WeaponData.n_SKILL == 0)
		{
			pWeaponStructs[0].BulletData = new SKILL_TABLE();
		}
		else
		{
			pWeaponStructs[0].BulletData = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[pWeaponStructs[0].WeaponData.n_SKILL];
		}
		array[0] = OrangeBattleUtility.FindAllChildRecursive(base.transform, "NormalWeapon" + 0);
		if (pWeaponStructs[0].WeaponData != null && (short)pWeaponStructs[0].WeaponData.n_TYPE != 8)
		{
			return;
		}
		pWeaponStructs[0].WeaponTrial = array[0][0].GetComponentInChildren<MeleeWeaponTrail>();
		pWeaponStructs[0].SlashObject = OrangeBattleUtility.FindChildRecursive(base.transform, "SlashEfx");
		if ((bool)pWeaponStructs[0].SlashObject)
		{
			pWeaponStructs[0].SlashEfxCmp = pWeaponStructs[0].SlashObject.transform.GetComponent<SlashEfx>();
			pWeaponStructs[0].SlashEfxCmp.InitSlashData(pWeaponStructs[0].WeaponData.s_MODEL, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[AvatarID].s_ANIMATOR.Substring(0, 4).Equals("male"), base.transform, Vector3.one);
		}
		GameObject gameObject = new GameObject("Melee_Bullet");
		pWeaponStructs[0].MeleeBullet = gameObject.AddComponent<MeleeBullet>();
		pWeaponStructs[0].MeleeBullet.UpdateWeaponData(pWeaponStructs[0].BulletData, base.name);
		pWeaponStructs[0].MeleeBullet.transform.SetParent(OrangeBattleUtility.FindChildRecursive(array[0][0], "tip0"));
		pWeaponStructs[0].MeleeBullet.transform.SetParent(base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		if (array[0] != null)
		{
			for (int i = 0; i < array[0].Length; i++)
			{
				if (array[0][i] != null)
				{
					pWeaponStructs[0].WeaponMesh[i] = array[0][i].GetComponent<CharacterMaterial>();
				}
			}
		}
		pWeaponStructs[0].ForceLock = false;
		pWeaponStructs[0].ChargeTimer = new UpdateTimer();
		pWeaponStructs[0].LastUseTimer = new UpdateTimer();
		pWeaponStructs[0].LastUseTimer.TimerStart();
		pWeaponStructs[0].LastUseTimer += (float)(pWeaponStructs[0].BulletData.n_RELOAD / 2);
		pWeaponStructs[0].MagazineRemain = pWeaponStructs[0].BulletData.n_MAGAZINE;
		DeActivateMeleeAttack(pWeaponStructs[0]);
		SKILL_TABLE[] array2 = new SKILL_TABLE[5];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = pWeaponStructs[0].BulletData;
		}
		pWeaponStructs[0].FastBulletDatas = array2;
	}

	protected override void SetStatus(int mainStatus, int subStatus = 0)
	{
		_mainStatus = (MainStatus)mainStatus;
		_subStatus = (SubStatus)subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			switch (_subStatus)
			{
			case SubStatus.Phase2:
				SetMeleeStatus(PlayerWeapons[0], SlashCount);
				break;
			case SubStatus.Phase3:
				DeActivateMeleeAttack(PlayerWeapons[0]);
				break;
			}
			break;
		case MainStatus.Hurt:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Walk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = base.direction * EnemyHumanController.StepSpeed;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * EnemyHumanController.WalkSpeed;
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	protected override void UpdateAnimation()
	{
		int stateNameHash = 0;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				stateNameHash = EnemyHumanController._animationHash[0][_isShoot];
				break;
			case SubStatus.Phase1:
				stateNameHash = EnemyHumanController._animationHash[10][_isShoot];
				break;
			case SubStatus.Phase2:
				stateNameHash = _swordAnimationHash[SlashCount];
				break;
			case SubStatus.Phase3:
				stateNameHash = _swordAnimationHash[5 + SlashCount - 1];
				break;
			}
			break;
		case MainStatus.Fall:
			stateNameHash = EnemyHumanController._animationHash[9][_isShoot];
			break;
		case MainStatus.Hurt:
			if (IsStun)
			{
				stateNameHash = EnemyHumanController._animationHash[24][_isShoot];
				_isShoot = 0;
			}
			else
			{
				stateNameHash = EnemyHumanController._animationHash[23][_isShoot];
			}
			break;
		case MainStatus.Walk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				stateNameHash = EnemyHumanController._animationHash[22][_isShoot];
				break;
			case SubStatus.Phase1:
				stateNameHash = EnemyHumanController._animationHash[2][_isShoot];
				break;
			}
			break;
		}
		_animator.Play(stateNameHash, 0, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate || !BuildDone || PlayerWeapons == null || PlayerWeapons[0].BulletData == null)
		{
			return;
		}
		PlayerWeapons[0].ChargeTimer += GameLogicUpdateManager.m_fFrameLenMS;
		PlayerWeapons[0].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SlashCount = 0;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					if (Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) > WalkDistance && !CheckMoveFall(_velocity + VInt3.signRight * base.direction * EnemyHumanController.WalkSpeed))
					{
						SetStatus(MainStatus.Walk);
					}
					else if (PlayerWeapons[WeaponCurrent].LastUseTimer.GetMillisecond() > PlayerWeapons[WeaponCurrent].BulletData.n_RELOAD && Mathf.Abs(Target._transform.position.y - _transform.position.y) < 1.5f)
					{
						SetStatus(MainStatus.Idle, SubStatus.Phase2);
					}
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
				if (_currentFrame > 1f)
				{
					SlashCount++;
					if (SlashCount >= PlayerWeapons[WeaponCurrent].BulletData.n_MAGAZINE)
					{
						Slashing = false;
						SetStatus(MainStatus.Idle, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Idle, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					PlayerWeapons[WeaponCurrent].LastUseTimer.TimerStart();
					SetStatus(MainStatus.Idle);
				}
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
				if (Target == null || (double)Mathf.Abs(Target.GetTargetPoint().x - _transform.position.x) < (double)WalkDistance - 1.5)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Crouch:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Debut:
		case MainStatus.Dash:
		case MainStatus.AirDash:
		case MainStatus.Jump:
		case MainStatus.Fall:
		case MainStatus.Dead:
		case MainStatus.Hurt:
			break;
		}
	}

	public override void UpdateFunc()
	{
		if (Activate)
		{
			base.UpdateFunc();
			if (Slashing && (bool)PlayerWeapons[0].MeleeBullet)
			{
				UpdateSlashCollider();
			}
		}
	}

	protected void SetMeleeStatus(WeaponStruct weaponStruct, int slashcount)
	{
		WEAPON_TABLE weaponData = weaponStruct.WeaponData;
		SlashType slashType = (SlashType)(0 + slashcount);
		if (_mainStatus == MainStatus.Idle)
		{
			if ((short)weaponData.n_TYPE == 8)
			{
				ActivateMeleeAttack(weaponStruct, slashType);
			}
		}
		else if ((short)weaponData.n_TYPE == 8)
		{
			ActivateMeleeAttack(weaponStruct, slashType);
		}
	}

	protected void ActivateMeleeAttack(WeaponStruct targetWeapon, SlashType slashType = SlashType.None)
	{
		bool flag = slashType != SlashType.None;
		if (flag)
		{
			if ((bool)targetWeapon.SlashObject && (bool)targetWeapon.SlashEfxCmp)
			{
				if (base.direction == -1)
				{
					targetWeapon.SlashEfxCmp.ActivateMeleeEffect(flag, slashType, OrangeCharacter.ReversedQuaternion, true);
				}
				else
				{
					targetWeapon.SlashEfxCmp.ActivateMeleeEffect(flag, slashType, OrangeCharacter.NormalQuaternion, false);
				}
			}
			SlashDetails item = new SlashDetails
			{
				MeleeBullet = targetWeapon.MeleeBullet,
				Info = targetWeapon.MeleeBullet.GetSlashCollider((int)slashType),
				SlashType = slashType,
				TargetWeaponStruct = targetWeapon
			};
			_listSlashCache.Add(item);
			targetWeapon.MeleeBullet.SetDestroy(OrangeCharacter.MainStatus.NONE, OrangeCharacter.SubStatus.NONE);
			targetWeapon.MeleeBullet.UpdateCollider((int)slashType, base.direction == -1);
			targetWeapon.MeleeBullet.SetBulletAtk(targetWeapon.weaponStatus, selfBuffManager.sBuffStatus, EnemyData);
			targetWeapon.MeleeBullet.BulletLevel = targetWeapon.SkillLV;
			if (slashType == SlashType.Skill)
			{
				targetWeapon.MeleeBullet.UpdateBulletData(targetWeapon.FastBulletDatas[0]);
			}
			else
			{
				targetWeapon.MeleeBullet.UpdateBulletData(targetWeapon.FastBulletDatas[SlashCount]);
			}
			if (slashType == SlashType.StandSlash5)
			{
				targetWeapon.MeleeBullet.isUseHitStop2Self = true;
			}
		}
		else
		{
			DeActivateMeleeAttack(targetWeapon);
		}
		Slashing = true;
	}

	protected void DeActivateMeleeAttack(WeaponStruct targetWeapon)
	{
		if ((bool)targetWeapon.SlashObject && (bool)targetWeapon.SlashEfxCmp)
		{
			targetWeapon.SlashEfxCmp.DeActivateMeleeEffect();
		}
		if (targetWeapon.MeleeBullet.IsActivate)
		{
			targetWeapon.MeleeBullet.SetDestroy(OrangeCharacter.MainStatus.NONE, OrangeCharacter.SubStatus.NONE);
			targetWeapon.MeleeBullet.isUseHitStop2Self = false;
		}
	}

	private void UpdateSlashCollider()
	{
		if (_listSlashCache.Count == 0)
		{
			return;
		}
		SlashDetails slashDetails = _listSlashCache[0];
		if (slashDetails.MeleeBullet == null)
		{
			_prevFrame = _currentFrame;
			_listSlashCache.Clear();
			return;
		}
		if (_prevFrame < slashDetails.Info.timing && _currentFrame >= slashDetails.Info.timing)
		{
			if (!slashDetails.MeleeBullet.IsActivate)
			{
				slashDetails.MeleeBullet.Active(slashDetails.SlashType, targetMask, slashDetails.TargetWeaponStruct);
			}
			else
			{
				slashDetails.MeleeBullet.ClearList();
			}
			_listSlashCache.Clear();
		}
		_prevFrame = _currentFrame;
	}

	public override void SetActiveReal(bool isActive)
	{
		if (IsStun)
		{
			SetStun(false);
		}
		Controller.enabled = isActive;
		SetColliderEnable(isActive);
		Slashing = false;
		if (isActive)
		{
			_transform.SetParent(null);
			Controller.LogicPosition = new VInt3(_transform.localPosition);
			_velocityExtra = VInt3.zero;
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, true);
			EnableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			_animator.SetFloat(hashSpeedMultiplier, 1f);
			SetStatus(MainStatus.Idle);
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Appear();
			}
		}
		else
		{
			BuildDone = false;
			UpdateHurtAction();
			AiTimer.TimerStop();
			_listSlashCache.Clear();
			DeActivateMeleeAttack(PlayerWeapons[0]);
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, false);
			selfBuffManager.StopLoopSE();
			DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Disappear(delegate
				{
					PlayerWeapons[0].MeleeBullet.BackToPool();
					Object.Destroy(PlayerWeapons[0].MeleeBullet.gameObject);
					Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
					Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "AimIcon2");
					if ((bool)transform)
					{
						transform.SetParent(null);
					}
					if ((bool)CurrentEnemyHumanModel)
					{
						CurrentEnemyHumanModel.transform.SetParentNull();
						CurrentEnemyHumanModel.BackToPool();
						for (int i = 0; i < CurrentEnemyHumanWeapon.Length; i++)
						{
							if (CurrentEnemyHumanWeapon[i] != null)
							{
								CurrentEnemyHumanWeapon[i].transform.SetParentNull();
								CurrentEnemyHumanWeapon[i].BackToPool();
							}
						}
						ModelTransform = null;
					}
					MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
				});
			}
		}
		Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive && isActive;
		if (!isActive)
		{
			bNeedDead = false;
		}
	}
}
