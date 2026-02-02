using System;
using StageLib;
using UnityEngine;
using enums;

public class CH043_Controller : CharacterControlBase, ILogicUpdate
{
	private bool bInSkill;

	private ChargeShootObj _refChargeShootObj;

	private SkinnedMeshRenderer tfLHandMesh;

	private SkinnedMeshRenderer tfLBusterMesh;

	private SkinnedMeshRenderer tfRHandMesh;

	private SkinnedMeshRenderer tfRBusterMesh;

	private bool bGoldMode;

	private int nConditionId = -1;

	private CharacterMaterial cmLBuster;

	private CharacterMaterial cmRBuster;

	private OrangeTimer _goldModePoseDelayTimer;

	private int nDefGoldModePoseDelyFrame = 20;

	private int nFlyFrame;

	private bool bCheckJumpFlag = true;

	private OrangeTimer _upDashTimer;

	private FxBase _fxUpDash;

	[SerializeField]
	private float upDashSpeed = 1f;

	private readonly int hashDirection = Animator.StringToHash("fDirection");

	private float[] originalSpecularVal = new float[3];

	private float[] goldModeSpecularVal = new float[3] { 0.02f, 0.82f, 0.04f };

	public override string[] GetCharacterDependAnimations()
	{
		return new string[19]
		{
			"ch043_skill_01_stand_1st_shot_start", "ch043_skill_01_jump_1st_shot_start", "ch043_skill_01_crouch_1st_shot_start", "ch043_skill_01_stand_1st_shot_end", "ch043_skill_01_jump_1st_shot_end", "ch043_skill_01_crouch_1st_shot_end", "ch043_skill_01_stand_2nd_shot_start", "ch043_skill_01_jump_2nd_shot_start", "ch043_skill_01_crouch_2nd_shot_start", "ch043_skill_01_stand_2nd_shot_end",
			"ch043_skill_01_jump_2nd_shot_end", "ch043_skill_01_crouch_2nd_shot_end", "ch043_skill_02_change_hype_mode_stand_start", "ch043_skill_02_change_hype_mode_stand_end", "ch043_skill_02_change_hype_mode_jump_start", "ch043_skill_02_change_hype_mode_jump_end", "ch043_skill_02_air_dash_start", "ch043_skill_02_air_dash_loop", "ch043_skill_02_air_dash_end"
		};
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void Start()
	{
		base.Start();
		nConditionId = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		InitExtraMeshData();
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { _refEntity.SkillSEID, "ch043_charge_lp", "ch043_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_ch043_charge_lp", "bt_ch043_charge_stop" };
		}
		GetEntitySpecularData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_ccs_point", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m");
		tfLHandMesh = transform.GetComponent<SkinnedMeshRenderer>();
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "CH043_Buster_G_L");
		tfLBusterMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		cmLBuster = transform2.GetComponent<CharacterMaterial>();
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_R_m");
		tfRHandMesh = transform3.GetComponent<SkinnedMeshRenderer>();
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "CH043_Buster_G_R");
		tfRBusterMesh = transform4.GetComponent<SkinnedMeshRenderer>();
		cmRBuster = transform4.GetComponent<CharacterMaterial>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_crosschargeshot_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_crosschargeshot_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_headjump_000", 3);
		_goldModePoseDelayTimer = OrangeTimerManager.GetTimer();
		_upDashTimer = OrangeTimerManager.GetTimer();
	}

	protected void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	protected void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public void LogicUpdate()
	{
		CheckGoldMode();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
		_refEntity.PlayerPressDashCB = PlayerPressDash;
		_refEntity.PlayerReleaseDashCB = PlayerReleaseDash;
		_refEntity.PlayerPressJumpCB = PlayerPressJump;
		_refEntity.PlayerReleaseJumpCB = PlayerReleaseJump;
		_refEntity.PlayerResetPressJumpCB = PlayerResetPressJump;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.PlayerSkillLandCB = PlayerSkillLand;
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			_refEntity.SkillEnd = true;
			ToggleLeftBuster(false, true);
			ToggleRightBuster(false);
			_refEntity.EnableCurrentWeapon();
			break;
		case 1:
			if ((bool)_refEntity.IsUnBreakX())
			{
				if (_refEntity.UsingVehicle && !bGoldMode)
				{
					if (bInSkill)
					{
						bInSkill = false;
						OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1], 0f, _refEntity.PlayerSkills[1].BulletData.n_MAGAZINE);
					}
					else
					{
						EnableGoldMode(true);
					}
				}
				_refEntity.EnableWeaponMesh(_refEntity.GetCurrentWeaponObj());
				_goldModePoseDelayTimer.TimerStop();
				return;
			}
			_refEntity.SkillEnd = true;
			ToggleLeftBuster(false, true);
			ToggleRightBuster(false);
			_refEntity.EnableCurrentWeapon();
			_goldModePoseDelayTimer.TimerStop();
			break;
		}
		if (_refEntity.UsingVehicle && bGoldMode)
		{
			bCheckJumpFlag = false;
		}
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableWeaponMesh(_refEntity.GetCurrentWeaponObj());
	}

	private void CheckJumpSkill()
	{
		if (_refEntity.Controller.Collisions.below && (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL || _refEntity.CurMainStatus == OrangeCharacter.MainStatus.IDLE))
		{
			bCheckJumpFlag = true;
		}
	}

	protected void CheckGoldMode()
	{
		if (StageUpdate.gbIsNetGame)
		{
			bool flag = _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(nConditionId);
			if (flag != bGoldMode)
			{
				EnableGoldMode(flag);
			}
		}
		else if (bGoldMode && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(nConditionId))
		{
			EnableGoldMode(false);
		}
	}

	public override void CheckSkill()
	{
		if (bGoldMode)
		{
			CheckJumpSkill();
		}
		if (_refEntity.IsAnimateIDChanged())
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.PlayerSkills[0].Reload_index == 0 && _refEntity.IsShoot == 0)
		{
			_refEntity.SkillEnd = true;
			ToggleLeftBuster(false, true);
			ToggleRightBuster(false);
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			else if (_refEntity.CurrentFrame > 0.3f && bInSkill)
			{
				bInSkill = false;
				_refChargeShootObj.StopCharge();
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ExtraTransforms[1]);
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.TriggerComboSkillBuff(_refEntity.PlayerSkills[0].FastBulletDatas[2].n_ID);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (CheckCancelAnimate(0))
			{
				CancelSkill0();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			}
			else if (_refEntity.CurrentFrame > 0.3f && bInSkill)
			{
				SKILL_TABLE sKILL_TABLE = _refEntity.PlayerSkills[0].FastBulletDatas[_refEntity.PlayerSkills[0].Reload_index];
				bInSkill = false;
				_refChargeShootObj.StopCharge();
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj(), sKILL_TABLE.n_USE_COST, -100f);
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ExtraTransforms[0]);
				_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (CheckCancelAnimate(0))
			{
				CancelSkill0();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (_refEntity.CurrentFrame > 0.5f && bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (_goldModePoseDelayTimer.IsStarted() && !bInSkill && _goldModePoseDelayTimer.GetTicks(1) > nDefGoldModePoseDelyFrame)
			{
				_goldModePoseDelayTimer.TimerStop();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (CheckCancelAnimate(1))
			{
				CancelSkill1();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			_refEntity.IgnoreGravity = false;
			if (nFlyFrame > 0)
			{
				nFlyFrame--;
				_refEntity.SetSpeed(0, Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * upDashSpeed));
			}
			if ((double)_refEntity.Velocity.y < (double)OrangeCharacter.JumpSpeed * 0.4)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			if (CheckCancelAnimate(2))
			{
				CancelUpDash();
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			return;
		}
		if (_refEntity.PlayerSetting.AutoCharge == 1)
		{
			if (!_refEntity.PlayerSkills[0].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[0].Reload_index == 0)
			{
				ChargeStart(id);
			}
			else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ShootSkill0();
			}
		}
		else if ((_refEntity.CurrentActiveSkill == -1 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1) && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			ShootSkill0();
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ShootSkill0();
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.PlayerStopDashing();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
			}
			break;
		}
	}

	public void PlayerHeldSkill(int id)
	{
		if (_refEntity.PlayerSetting.AutoCharge == 0)
		{
			ChargeStart(id);
		}
	}

	public void PlayerPressDash(object tParam)
	{
		if (!_refEntity.Controller.Collisions.below && IsHeldUpCustom())
		{
			if (_refEntity.CanUseDash())
			{
				_refEntity.UseDashChance();
				DoUpDash();
			}
			else if (bCheckJumpFlag && bGoldMode && (_refEntity.CurrentActiveSkill == -1 || (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_4)))
			{
				bCheckJumpFlag = false;
				DoUpDash();
				_refEntity.Check3rdJumpTrigger(false);
			}
		}
		else
		{
			_refEntity.PlayerPressDash(tParam);
		}
	}

	public void PlayerReleaseDash()
	{
		if (_upDashTimer.IsStarted())
		{
			if (_refEntity.PlayerSetting.JumpClassic != 0)
			{
				CancelUpDash();
			}
		}
		else
		{
			_refEntity.PlayerReleaseDash();
		}
	}

	public void PlayerPressJump()
	{
		if (bGoldMode)
		{
			if (!_refEntity.CanUseDash() && _refEntity.CurrentActiveSkill == -1 && bCheckJumpFlag)
			{
				bCheckJumpFlag = false;
				DoUpDash();
				_refEntity.Check3rdJumpTrigger(false);
			}
			else
			{
				_refEntity.PlayerPressJump();
			}
		}
		else
		{
			_refEntity.PlayerPressJump();
		}
	}

	public void PlayerReleaseJump()
	{
		if (_upDashTimer.IsStarted())
		{
			if (_refEntity.PlayerSetting.JumpClassic != 0)
			{
				CancelUpDash();
			}
		}
		else
		{
			_refEntity.PlayerReleaseJump();
		}
	}

	public void PlayerResetPressJump()
	{
		_refEntity.PlayerPressJumpCB = PlayerPressJump;
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus != 0)
			{
				int num = 1;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				bInSkill = true;
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				ToggleLeftBuster(true, false);
				ToggleRightBuster(true);
				if (!_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
					break;
				}
				_refEntity.SetHorizontalSpeed(0);
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SkillEnd = true;
				ToggleLeftBuster(false, true);
				ToggleRightBuster(false);
				if (!_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
					break;
				}
				_refEntity.SetHorizontalSpeed(0);
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				bInSkill = true;
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				ToggleLeftBuster(true, false);
				if (!_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
					break;
				}
				_refEntity.SetHorizontalSpeed(0);
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				if (!_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
					break;
				}
				_refEntity.SetHorizontalSpeed(0);
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				bInSkill = true;
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				ToggleLeftBuster(false, false);
				if ((bool)tfLHandMesh && !tfLHandMesh.enabled)
				{
					tfLHandMesh.enabled = true;
				}
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)77u);
					break;
				}
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)79u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)78u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)80u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)81u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)82u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)83u);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				ToggleRightBuster(false);
				ToggleLeftBuster(false, true);
				_refEntity.EnableCurrentWeapon();
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SkillEnd = true;
				ToggleLeftBuster(false, true);
				ToggleRightBuster(false);
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				if (!_goldModePoseDelayTimer.IsStarted())
				{
					PlayVoiceSE("v_ch043_skill02");
					PlaySkillSE("ch043_armor");
					EnableGoldMode(true);
					_goldModePoseDelayTimer.TimerStart();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SkillEnd = true;
				ToggleLeftBuster(false, true);
				ToggleRightBuster(false);
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.IgnoreGravity = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				nFlyFrame = 3;
				_refEntity.SetSpeed(0, Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * upDashSpeed));
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				CancelUpDash();
				break;
			}
			break;
		}
	}

	public void PlayerSkillLand()
	{
		_refEntity.SetHorizontalSpeed(0);
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.PlayerStopDashing();
			_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.PlayerStopDashing();
			_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		_refEntity.StartShootTimer();
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				PlayVoiceSE("v_ch043_skill01");
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[2], wsSkill.weaponStatus, _refEntity.ExtraTransforms[1], wsSkill.SkillLV, (float)_refEntity._characterDirection * Vector3.right);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				PlayVoiceSE("v_ch043_skill01_01");
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[wsSkill.Reload_index], wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV, (float)_refEntity._characterDirection * Vector3.right);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV, (float)_refEntity._characterDirection * Vector3.right);
				break;
			}
		}
		else
		{
			_refEntity.IsShoot = (sbyte)(wsSkill.ChargeLevel + 1);
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[wsSkill.ChargeLevel], wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
		}
	}

	public override void ControlCharacterDead()
	{
	}

	public override void ExtraVariableInit()
	{
		if (bGoldMode && !_refEntity.CanUseDash())
		{
			bCheckJumpFlag = false;
		}
	}

	public void TeleportOutCharacterDepend()
	{
	}

	public void StageTeleportOutCharacterDepend()
	{
	}

	private bool CheckCancelAnimate(int skillId)
	{
		switch (skillId)
		{
		case 0:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
			{
				return true;
			}
			break;
		case 1:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		case 2:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		ToggleLeftBuster(false, true);
		ToggleRightBuster(false);
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.IgnoreGravity = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void ChargeStart(int id)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.HURT:
			return;
		case OrangeCharacter.MainStatus.SKILL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL0_1)
			{
				return;
			}
			break;
		}
		}
		if (!_refEntity.PlayerSkills[id].ForceLock && _refEntity.PlayerSkills[id].Reload_index == 0 && !_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[id].FastBulletDatas[0].n_CHARGE_MAX_LEVEL != 0 && _refEntity.CheckUseSkillKeyTriggerEX(id))
		{
			_refEntity.PlayerSkills[0].ChargeTimer.TimerStart();
			_refChargeShootObj.StartCharge();
		}
	}

	private void ShootSkill0()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		_refEntity.CurrentActiveSkill = 0;
		if (weaponStruct.ChargeLevel == 2)
		{
			_refEntity.SkillEnd = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_crosschargeshot_000", _refEntity.ExtraTransforms[2].position, Quaternion.identity, Array.Empty<object>());
			return;
		}
		if (weaponStruct.Reload_index != 0)
		{
			_refEntity.SkillEnd = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_crosschargeshot_001", _refEntity.ExtraTransforms[2].position, Quaternion.identity, Array.Empty<object>());
			return;
		}
		_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
		_refEntity.Animator.SetAnimatorEquip(1);
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, (float)_refEntity._characterDirection * Vector3.right)) / 180f;
		_refEntity.Animator._animator.SetFloat(hashDirection, value);
		ToggleLeftBuster(true, false);
		_refChargeShootObj.StopCharge();
		OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel].n_USE_COST, -1f);
		CreateSkillBullet(weaponStruct);
		_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		ToggleLeftBuster(false, true);
		ToggleRightBuster(false);
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.Dashing = false;
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		ToggleLeftBuster(false, true);
		ToggleRightBuster(false);
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.IgnoreGravity = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void EnableGoldMode(bool active)
	{
		bGoldMode = active;
		if (bGoldMode)
		{
			_refEntity.CharacterMaterials.UpdateTex(0);
			_refEntity.CharacterMaterials.UpdateSpecColorTex(1);
			if ((bool)cmLBuster)
			{
				cmLBuster.UpdateTex(0);
				cmLBuster.UpdateSpecColorTex(1);
			}
			if ((bool)cmRBuster)
			{
				cmRBuster.UpdateTex(0);
				cmRBuster.UpdateSpecColorTex(1);
			}
		}
		else
		{
			_refEntity.CharacterMaterials.UpdateTex();
			_refEntity.CharacterMaterials.UpdateSpecColorTex();
			if ((bool)cmLBuster)
			{
				cmLBuster.UpdateTex();
				cmLBuster.UpdateSpecColorTex();
			}
			if ((bool)cmRBuster)
			{
				cmRBuster.UpdateTex();
				cmRBuster.UpdateSpecColorTex();
			}
		}
		SetEntitySpecularData();
	}

	private void ToggleLeftBuster(bool enable, bool haveNormalWeapon)
	{
		if (enable)
		{
			if ((bool)tfLBusterMesh)
			{
				tfLBusterMesh.enabled = true;
			}
			if ((bool)tfLHandMesh)
			{
				tfLHandMesh.enabled = false;
			}
			return;
		}
		switch ((WeaponType)(short)_refEntity.GetCurrentWeaponObj().WeaponData.n_TYPE)
		{
		case WeaponType.Melee:
		case WeaponType.DualGun:
		case WeaponType.MGun:
		case WeaponType.Gatling:
		case WeaponType.Launcher:
			if ((bool)tfLHandMesh)
			{
				tfLHandMesh.enabled = true;
			}
			break;
		default:
			if ((bool)tfLHandMesh)
			{
				if (haveNormalWeapon)
				{
					tfLHandMesh.enabled = false;
				}
				else
				{
					tfLHandMesh.enabled = true;
				}
			}
			break;
		}
		if ((bool)tfLBusterMesh)
		{
			tfLBusterMesh.enabled = false;
		}
	}

	private void ToggleRightBuster(bool enable)
	{
		if ((bool)tfRBusterMesh)
		{
			tfRBusterMesh.enabled = enable;
		}
		if ((bool)tfRHandMesh)
		{
			tfRHandMesh.enabled = !enable;
		}
	}

	private bool IsHeldUpCustom()
	{
		if (!ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.UP))
		{
			return false;
		}
		ManagedSingleton<InputStorage>.Instance.GetInputInfo(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
		VInt2 vInt = new VInt2(MonoBehaviourSingleton<InputManager>.Instance.VirtualPadSystem.VirtualAnalogStickInstance.GetStickValue());
		if (vInt == VInt2.zero)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
			{
				return false;
			}
			return true;
		}
		int num = 300;
		if (vInt.x > -num && vInt.x < num)
		{
			return true;
		}
		return false;
	}

	private void DoUpDash()
	{
		_refEntity.CurrentActiveSkill = 1;
		_refEntity.SkillEnd = false;
		_refEntity.IgnoreGravity = true;
		_refEntity.SetSpeed(0, Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.05f));
		_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
		ToggleLeftBuster(false, false);
		_upDashTimer.TimerStart();
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
		if ((bool)_fxUpDash)
		{
			_fxUpDash.BackToPool();
		}
		_fxUpDash = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_headjump_000", _refEntity.transform, Quaternion.identity, Array.Empty<object>());
		PlaySkillSE("ch043_airdash");
	}

	private void CancelUpDash()
	{
		_upDashTimer.TimerStop();
		_refEntity.SkillEnd = true;
		ToggleLeftBuster(false, true);
		_refEntity.IgnoreGravity = false;
		if (!_refEntity.IsStun)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
		if ((bool)_fxUpDash)
		{
			_fxUpDash.BackToPool();
			_fxUpDash = null;
		}
	}

	private void GetEntitySpecularData()
	{
		originalSpecularVal[0] = _refEntity.CharacterMaterials.GetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Smoothness);
		originalSpecularVal[1] = _refEntity.CharacterMaterials.GetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_SpecSmooth);
		originalSpecularVal[2] = _refEntity.CharacterMaterials.GetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_GradientMax);
	}

	private void SetEntitySpecularData()
	{
		if (bGoldMode)
		{
			_refEntity.CharacterMaterials.SetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Smoothness, goldModeSpecularVal[0]);
			_refEntity.CharacterMaterials.SetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_SpecSmooth, goldModeSpecularVal[1]);
			_refEntity.CharacterMaterials.SetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_GradientMax, goldModeSpecularVal[2]);
		}
		else
		{
			_refEntity.CharacterMaterials.SetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Smoothness, originalSpecularVal[0]);
			_refEntity.CharacterMaterials.SetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_SpecSmooth, originalSpecularVal[1]);
			_refEntity.CharacterMaterials.SetPropertyFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_GradientMax, originalSpecularVal[2]);
		}
	}
}
