using System;
using System.Collections.Generic;
using UnityEngine;

public class CH077_Controller : CharacterControllerProxyBaseGen4
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL1_START = 65u,
		ANI_SKILL1_LOOP = 66u,
		ANI_SKILL1_END = 67u
	}

	private enum FxName
	{
		fxduring_ch077_novastrike_000 = 0,
		fxduring_ch077_novastrike_001 = 1,
		fxduring_ch077_novastrike_002 = 2
	}

	public float TIME_SKILL_1_LOOP = 0.25f;

	public float TIME_SKILL_1_CANCEL = 0.05f;

	public float JUMP_ANIMATION_SPEED_SKILL_1 = 2f;

	public float JUMP_SPEED_X_SKILL_1 = 0.15f;

	public float JUMP_SPEED_Y_SKILL_1 = 0.15f;

	public float MOVE_SPEED_X_SKILL_1 = 4f;

	private ChargeShootObj _chargeShootObj;

	private FxBase _fxNovaStrike;

	private SkinnedMeshRenderer _busterMesh;

	private void ShootChargeBuster(int skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[skillID];
		if (weaponStruct.ChargeLevel == 0 && weaponStruct.Reload_index == 0)
		{
			_chargeShootObj.StopCharge();
			return;
		}
		_refEntity.Animator.SetAnimatorEquip(1);
		ToggleWeapon(WeaponState.SKILL_0);
		_refEntity.EnableHandMesh(false);
		switch (weaponStruct.ChargeLevel)
		{
		case 1:
			_chargeShootObj.ShootChargeBuster(skillID);
			break;
		case 2:
		{
			_chargeShootObj.StopCharge(skillID);
			SKILL_TABLE sKILL_TABLE2 = weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel];
			_refEntity.CurrentActiveSkill = skillID;
			PushComboSkillBullet(weaponStruct, sKILL_TABLE2);
			_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct, sKILL_TABLE2.n_USE_COST, -1f);
			if (_refEntity.IsLocalPlayer)
			{
				_refEntity.RemoveComboSkillBuff(sKILL_TABLE2.n_ID);
				_refEntity.TriggerComboSkillBuff(sKILL_TABLE2.n_ID);
			}
			break;
		}
		case 0:
		{
			SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[weaponStruct.Reload_index];
			_refEntity.CurrentActiveSkill = skillID;
			PushComboSkillBullet(weaponStruct, sKILL_TABLE);
			if (weaponStruct.Reload_index == weaponStruct.FastBulletDatas.Length - 1)
			{
				_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
				OrangeBattleUtility.UpdateSkillCD(weaponStruct, sKILL_TABLE.n_USE_COST, -1f);
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
				}
			}
			else if (_refEntity.IsLocalPlayer)
			{
				_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
				_refEntity.TriggerComboSkillBuff(sKILL_TABLE.n_ID);
			}
			break;
		}
		}
	}

	private void PushComboSkillBullet(WeaponStruct skillData, SKILL_TABLE comboSkillData)
	{
		_refEntity.IsShoot = 2;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(comboSkillData, skillData.weaponStatus, _refEntity.ExtraTransforms[0], skillData.SkillLV);
	}

	private void ActionStatusChanged_1_0()
	{
		SetIgnoreGravity(false);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.selfBuffManager.AddMeasure(-weaponStruct.BulletData.n_USE_COST);
		if (_refEntity.IsInGround)
		{
			SetSpeed((float)OrangeCharacter.WalkSpeed * JUMP_SPEED_X_SKILL_1 * (float)_refEntity.direction, (float)OrangeCharacter.JumpSpeed * JUMP_SPEED_Y_SKILL_1);
		}
		_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
		base.AnimatorSpeed = JUMP_ANIMATION_SPEED_SKILL_1;
	}

	private void ActionStatusChanged_1_1()
	{
		EnableColliderBullet();
		_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
		base.AnimatorSpeed = 1f;
		SetSpeed((float)OrangeCharacter.DashSpeed * MOVE_SPEED_X_SKILL_1 * (float)_refEntity.direction, 0f);
		_fxNovaStrike = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fxduring_ch077_novastrike_000.ToString(), _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxduring_ch077_novastrike_002.ToString(), _refEntity.ModelTransform.position, (_refEntity.direction > 0) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
		SetSkillFrame(TIME_SKILL_1_LOOP);
	}

	private void ActionLogicUpdate_1_1()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxduring_ch077_novastrike_001.ToString(), _refEntity.ModelTransform.position, (_refEntity.direction > 0) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
		ActionCheckNextSkillStatus();
	}

	private void ActionStatusChanged_1_2()
	{
		DisableColliderBullet();
		_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
		if (_fxNovaStrike != null)
		{
			_fxNovaStrike.BackToPool();
			_fxNovaStrike = null;
		}
		ResetSpeed();
		SetSkillCancelFrame(TIME_SKILL_1_CANCEL);
	}

	public override void Awake()
	{
		base.Awake();
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "R WeaponPoint", true)
		};
		_busterMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "BusterMesh_m").GetComponent<SkinnedMeshRenderer>();
	}

	public override void Start()
	{
		base.Start();
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.selfBuffManager.nMeasureMax = _refEntity.PlayerSkills[1].BulletData.n_MAGAZINE * 100;
		_chargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		InitializeSkillDependDelegators(new Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData>
		{
			{
				OrangeCharacter.SubStatus.SKILL1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_0,
					OnAnimationEnd = base.ActionSetNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_1,
					OnLogicUpdate = ActionLogicUpdate_1_1
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_2,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_2,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch077_skill_02_start", "ch077_skill_02_loop", "ch077_skill_02_end" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[8] { "login", "logout", "win", "buster_stand_charge_atk", "buster_crouch_charge_atk", "buster_jump_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk" };
		target = new string[8] { "ch077_login", "ch077_logout", "ch077_win", "ch077_skill_01_stand", "ch077_skill_01_crouch", "ch077_skill_01_jump", "ch077_skill_01_fall", "ch077_skill_01_wallgrab" };
	}

	protected override void TeleportInCharacterDepend()
	{
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID == HumanBase.AnimateId.ANI_TELEPORT_IN_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
		}
	}

	protected override void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
		}
	}

	protected override void StageTeleportInCharacterDepend()
	{
	}

	protected override void StageTeleportOutCharacterDepend()
	{
	}

	public override void ControlCharacterContinue()
	{
	}

	protected override void ToggleExtraTransforms(bool isActive)
	{
	}

	protected override bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		return base.OnEnterRideArmor(targetRideArmor);
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		switch (weaponState)
		{
		case WeaponState.TELEPORT_IN:
		case WeaponState.TELEPORT_OUT:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			_busterMesh.enabled = false;
			break;
		case WeaponState.SKILL_0:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(false);
			_busterMesh.enabled = true;
			break;
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			_busterMesh.enabled = false;
			break;
		default:
			ToggleNormalWeapon(true);
			_busterMesh.enabled = false;
			break;
		}
	}

	protected override void LogicUpdateCharacterDepend()
	{
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CheckSkillEndByShootTimer())
		{
			ToggleWeapon(WeaponState.NORMAL);
		}
		base.LogicUpdateCharacterDepend();
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerPressSkill0Events[0] = OnPlayerPressSkill0;
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		int n_CHARGE_MAX_LEVEL = weaponStruct.BulletData.n_CHARGE_MAX_LEVEL;
		int num = _refEntity.PlayerSkills[0].FastBulletDatas.Length;
		for (int i = weaponStruct.BulletData.n_CHARGE_MAX_LEVEL + 1; i < weaponStruct.FastBulletDatas.Length; i++)
		{
			OnPlayerPressSkill0Events[i] = OnPlayerPressSkill0_Stock;
		}
		OnPlayerReleaseSkill0Events[0] = OnPlayerReleaseSkill0;
		OnPlayerPressSkill1Events[0] = OnPlayerPressSkill1;
	}

	protected override void OnPlayerPressSkill0(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		if (_refEntity.PlayerSetting.AutoCharge == 1)
		{
			if (!weaponStruct.ChargeTimer.IsStarted())
			{
				weaponStruct.ChargeTimer.TimerStart();
				_chargeShootObj.StartCharge();
			}
			else
			{
				PlayVoiceSE("v_x4_skill01");
				ShootChargeBuster((int)skillID);
			}
		}
		else
		{
			PlayVoiceSE("v_x4_skill01");
			_refEntity.Animator.SetAnimatorEquip(1);
			ToggleWeapon(WeaponState.SKILL_0);
			_refEntity.PlayerShootBuster(weaponStruct, true, (int)skillID, 0);
			_refEntity.EnableHandMesh(false);
		}
	}

	private void OnPlayerPressSkill0_Stock(SkillID skillID)
	{
		PlayVoiceSE("v_x4_skill01");
		ShootChargeBuster((int)skillID);
	}

	protected override void OnPlayerReleaseSkill0(SkillID skillID)
	{
		if (_refEntity.PlayerSetting.AutoCharge != 1)
		{
			PlayVoiceSE("v_x4_skill01");
			ShootChargeBuster((int)skillID);
		}
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		if (_refEntity.selfBuffManager.nMeasureNow >= weaponStruct.BulletData.n_USE_COST)
		{
			PlayVoiceSE("v_x4_skill02");
			base.OnPlayerPressSkill1(skillID);
		}
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	protected override void SetSkillEnd()
	{
		if (_fxNovaStrike != null)
		{
			_fxNovaStrike.BackToPool();
			_fxNovaStrike = null;
		}
		base.SetSkillEnd();
	}

	protected override bool CheckCanTriggerSkill(SkillID skillId)
	{
		if (skillId == SkillID.SKILL_0 && skillId == (SkillID)_refEntity.CurrentActiveSkill)
		{
			return _refEntity.CheckUseSkillKeyTrigger((int)skillId, false);
		}
		return _refEntity.CheckUseSkillKeyTrigger((int)skillId);
	}
}
