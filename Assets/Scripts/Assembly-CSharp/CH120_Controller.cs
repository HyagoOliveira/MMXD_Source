using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

public class CH120_Controller : CharacterControllerProxyBaseGen4
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0_EX2_STAND = 65u,
		ANI_SKILL0_EX2_CROUCH = 66u,
		ANI_SKILL0_EX2_JUMP = 67u,
		ANI_SKILL0_STAND = 127u,
		ANI_SKILL0_CROUCH = 128u,
		ANI_SKILL0_JUMP = 129u,
		ANI_SKILL1_STAND_START = 130u,
		ANI_SKILL1_CROUCH_START = 131u,
		ANI_SKILL1_JUMP_START = 132u,
		ANI_SKILL1_STAND_LOOP = 133u,
		ANI_SKILL1_CROUCH_LOOP = 134u,
		ANI_SKILL1_JUMP_LOOP = 135u,
		ANI_SKILL1_STAND_END = 136u,
		ANI_SKILL1_CROUCH_END = 137u,
		ANI_SKILL1_JUMP_END = 138u
	}

	private enum FxName
	{
		fxuse_vanishingworldEX_000 = 0
	}

	private const int SKILL_0_EX_HELLWHEEL = 2;

	public float TIME_SKILL_0_CANCEL = 0.2f;

	public float SKILL_1_SHOOT_OFFSET = 0.5f;

	public float TIME_SKILL_1_LOOP = 0.5f;

	public float TIME_SKILL_1_CANCEL = 0.02f;

	public float SKILL_1_FX_OFFSET_Y = 1f;

	public float SKILL_1_FX_OFFSET_Y_CROUCH = 0.5f;

	private ChargeShootObj _chargeShootObj;

	private Vector3 _skill1ShootDirection;

	private OrangeConsoleCharacter _refConsolePlayer;

	private SkinnedMeshRenderer _busterMeshL;

	private SkinnedMeshRenderer _busterMeshR;

	private SkinnedMeshRenderer _handMeshL;

	private SkinnedMeshRenderer _handMeshR;

	private void ForceUpdateVirtualButtonAnalog()
	{
		if (!_refEntity.IsLocalPlayer)
		{
			return;
		}
		if (_refEntity.PlayerSkills[0].EnhanceEXIndex == 2)
		{
			OrangeConsoleCharacter refConsolePlayer = _refConsolePlayer;
			if ((object)refConsolePlayer != null)
			{
				refConsolePlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, false);
			}
		}
		OrangeConsoleCharacter refConsolePlayer2 = _refConsolePlayer;
		if ((object)refConsolePlayer2 != null)
		{
			refConsolePlayer2.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
		}
	}

	private void ShootChargeBuster(int skillID)
	{
		if (_refEntity.PlayerSkills[skillID].ChargeLevel <= 0)
		{
			_chargeShootObj.StopCharge();
			return;
		}
		_refEntity.Animator.SetAnimatorEquip(1);
		ToggleWeapon(WeaponState.SKILL_0);
		_chargeShootObj.ShootChargeBuster(skillID, true);
		_refEntity.EnableHandMesh(false);
	}

	private void ActionStatusChanged_0_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		PlayVoiceSE("v_re_skill03_2");
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			}
			else
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
		}
		_refEntity.IsShoot = 3;
		_refEntity.StartShootTimer();
		int enhanceEXIndex = weaponStruct.EnhanceEXIndex;
		if (enhanceEXIndex == 2)
		{
			ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[2], Mathf.Sign(_refEntity.ShootDirection.x) * Vector3.right, MagazineType.NORMAL, -1, 1, false);
		}
		else
		{
			_refEntity.PlayerShootBuster(weaponStruct, true, base.CurActiveSkill, 0);
		}
		ToggleWeapon(WeaponState.SKILL_0);
		SetSkillCancelFrame(TIME_SKILL_0_CANCEL);
	}

	private void ActionStatusChanged_1_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		PlayVoiceSE("v_re_skill04");
		_skill1ShootDirection = _refEntity.ShootDirection;
		_refEntity.direction = ((_skill1ShootDirection.x != 0f) ? Math.Sign(_skill1ShootDirection.x) : _refEntity.direction);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)131u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)130u);
			}
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)132u);
		}
	}

	private void ActionStatusChanged_1_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.IsShoot = 3;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0].position + _skill1ShootDirection * SKILL_1_SHOOT_OFFSET, weaponStruct.SkillLV, _skill1ShootDirection);
		if (weaponStruct.FastBulletDatas.Length > 1)
		{
			_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[1], weaponStruct.weaponStatus, _refEntity.ModelTransform, weaponStruct.SkillLV);
		}
		float num = SKILL_1_FX_OFFSET_Y;
		switch ((SkillAnimationId)_refEntity.AnimateID)
		{
		case SkillAnimationId.ANI_SKILL1_STAND_START:
			_refEntity.SetAnimateId((HumanBase.AnimateId)133u);
			break;
		case SkillAnimationId.ANI_SKILL1_CROUCH_START:
			_refEntity.SetAnimateId((HumanBase.AnimateId)134u);
			num = SKILL_1_FX_OFFSET_Y_CROUCH;
			break;
		case SkillAnimationId.ANI_SKILL1_JUMP_START:
			_refEntity.SetAnimateId((HumanBase.AnimateId)135u);
			break;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_vanishingworldEX_000.ToString(), _refEntity.ModelTransform.position + Vector3.up * num, Quaternion.identity, Array.Empty<object>());
		SetSkillFrame(TIME_SKILL_1_LOOP);
	}

	private void ActionLogicUpdate_1_1()
	{
		_refEntity.ShootDirection = _skill1ShootDirection;
		_refEntity.Animator.SetAttackLayerActive(_skill1ShootDirection);
		ActionCheckNextSkillStatus();
	}

	private void ActionStatusChanged_1_2()
	{
		switch ((SkillAnimationId)_refEntity.AnimateID)
		{
		case SkillAnimationId.ANI_SKILL1_STAND_LOOP:
			_refEntity.SetAnimateId((HumanBase.AnimateId)136u);
			break;
		case SkillAnimationId.ANI_SKILL1_CROUCH_LOOP:
			_refEntity.SetAnimateId((HumanBase.AnimateId)137u);
			break;
		case SkillAnimationId.ANI_SKILL1_JUMP_LOOP:
			_refEntity.SetAnimateId((HumanBase.AnimateId)138u);
			break;
		}
		SetSkillCancelFrame(TIME_SKILL_1_CANCEL);
	}

	private void ActionLogicUpdate_1_2()
	{
		_refEntity.ShootDirection = _skill1ShootDirection;
		_refEntity.Animator.SetAttackLayerActive(_skill1ShootDirection);
		ActionCheckSkillCancel();
	}

	public override void Awake()
	{
		base.Awake();
		if (_refEntity is OrangeConsoleCharacter)
		{
			_refConsolePlayer = _refEntity as OrangeConsoleCharacter;
		}
		_refEntity.ExtraTransforms = new Transform[3]
		{
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "R WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "Skill0HellWheelShootPosition", true)
		};
		_busterMeshL = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "BusterMesh_L_m", true).GetComponent<SkinnedMeshRenderer>();
		_busterMeshR = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "BusterMesh_R_m", true).GetComponent<SkinnedMeshRenderer>();
		_handMeshL = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "HandMesh_L_m", true).GetComponent<SkinnedMeshRenderer>();
		_handMeshR = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "HandMesh_R_m", true).GetComponent<SkinnedMeshRenderer>();
	}

	public override void Start()
	{
		base.Start();
		_refEntity.AnimatorModelShiftYOverride = new Better.Dictionary<OrangeCharacter.MainStatus, float>();
		_chargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			_chargeShootObj.ChargeSE = new string[3] { "SkillSE_RMEXE", "re_charge02_lp", "re_charge02_stop" };
			_chargeShootObj.ChargeLV3SE = "re_chargemax02";
		}
		else
		{
			_chargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_re_charge02_lp", "bt_re_charge02_stop" };
			_chargeShootObj.ChargeLV3SE = "bt_re_chargemax02";
		}
		_chargeShootObj.ShootChargeVoiceSE = "v_re_skill03_1";
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		int _enhanceSlot = -1;
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 0, new int[4] { 21601, 21601, 21604, 21601 }, ref _enhanceSlot, true);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		weaponStruct.ShootTransform[0] = _refEntity.ExtraTransforms[0];
		if (weaponStruct.EnhanceEXIndex == 2 && !weaponStruct.BulletData.s_MODEL.IsNullString())
		{
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<CH101_HellthRowingBullet>("prefab/bullet/" + weaponStruct.BulletData.s_MODEL, weaponStruct.BulletData.s_MODEL, 2, null);
		}
		weaponStruct = _refEntity.PlayerSkills[1];
		if (weaponStruct.FastBulletDatas.Length > 1)
		{
			string s_MODEL = weaponStruct.FastBulletDatas[1].s_MODEL;
			if (!s_MODEL.IsNullString())
			{
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<CollideBullet>("prefab/bullet/" + s_MODEL, s_MODEL, 2, null);
			}
		}
		InitializeSkillDependDelegators(new System.Collections.Generic.Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData>
		{
			{
				OrangeCharacter.SubStatus.SKILL0,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_0,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			},
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
					OnLogicUpdate = ActionLogicUpdate_1_2
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch120_skill_01_stand_mid", "ch120_skill_01_crouch_mid", "ch120_skill_01_jump_mid" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		return new string[12][]
		{
			new string[3] { "ch120_skill_01_stand_up", "ch120_skill_01_stand_mid", "ch120_skill_01_stand_down" },
			new string[3] { "ch120_skill_01_crouch_up", "ch120_skill_01_crouch_mid", "ch120_skill_01_crouch_down" },
			new string[3] { "ch120_skill_01_jump_up", "ch120_skill_01_jump_mid", "ch120_skill_01_jump_down" },
			new string[3] { "ch120_skill_02_stand_up_start", "ch120_skill_02_stand_mid_start", "ch120_skill_02_stand_down_start" },
			new string[3] { "ch120_skill_02_crouch_up_start", "ch120_skill_02_crouch_mid_start", "ch120_skill_02_crouch_down_start" },
			new string[3] { "ch120_skill_02_jump_up_start", "ch120_skill_02_jump_mid_start", "ch120_skill_02_jump_down_start" },
			new string[3] { "ch120_skill_02_stand_up_loop", "ch120_skill_02_stand_mid_loop", "ch120_skill_02_stand_down_loop" },
			new string[3] { "ch120_skill_02_crouch_up_loop", "ch120_skill_02_crouch_mid_loop", "ch120_skill_02_crouch_down_loop" },
			new string[3] { "ch120_skill_02_jump_up_loop", "ch120_skill_02_jump_mid_loop", "ch120_skill_02_jump_down_loop" },
			new string[3] { "ch120_skill_02_stand_up_end", "ch120_skill_02_stand_mid_end", "ch120_skill_02_stand_down_end" },
			new string[3] { "ch120_skill_02_crouch_up_end", "ch120_skill_02_crouch_mid_end", "ch120_skill_02_crouch_down_end" },
			new string[3] { "ch120_skill_02_jump_up_end", "ch120_skill_02_jump_mid_end", "ch120_skill_02_jump_down_end" }
		};
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[8] { "login", "logout", "win", "buster_stand_charge_atk", "buster_crouch_charge_atk", "buster_jump_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk" };
		target = new string[8] { "ch120_login", "ch120_logout", "ch120_win", "ch120_skill_01_stand_mid", "ch120_skill_01_crouch_mid", "ch120_skill_01_jump_mid", "ch120_skill_01_jump_mid", "ch120_skill_01_jump_mid" };
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
			_busterMeshL.enabled = false;
			_busterMeshR.enabled = false;
			_refEntity.EnableHandMesh(true);
			break;
		case WeaponState.SKILL_0:
			ToggleNormalWeapon(false);
			_busterMeshL.enabled = true;
			_busterMeshR.enabled = false;
			_handMeshL.enabled = false;
			_handMeshR.enabled = true;
			break;
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			_busterMeshL.enabled = true;
			_busterMeshR.enabled = true;
			_handMeshL.enabled = false;
			_handMeshR.enabled = false;
			break;
		default:
			_busterMeshL.enabled = false;
			_busterMeshR.enabled = false;
			_handMeshL.enabled = true;
			_handMeshR.enabled = true;
			ToggleNormalWeapon(true);
			break;
		}
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerPressSkill0Events[0] = OnPlayerPressSkill0;
		OnPlayerReleaseSkill0Events[0] = OnPlayerReleaseSkill0;
		OnPlayerReleaseSkill1Events[0] = OnPlayerReleaseSkill1;
	}

	protected override void OnPlayerPressSkill0(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		int enhanceEXIndex = weaponStruct.EnhanceEXIndex;
		if (enhanceEXIndex == 2)
		{
			base.OnPlayerPressSkill0(skillID);
			return;
		}
		if (_refEntity.PlayerSetting.AutoCharge == 1)
		{
			if (!weaponStruct.ChargeTimer.IsStarted())
			{
				weaponStruct.ChargeTimer.TimerStart();
				_chargeShootObj.StartCharge();
			}
			else
			{
				ShootChargeBuster((int)skillID);
			}
			return;
		}
		_refEntity.Animator.SetAnimatorEquip(1);
		ToggleWeapon(WeaponState.SKILL_0);
		PlayVoiceSE("v_re_skill03_1");
		_chargeShootObj.StopCharge();
		Vector3 value = (_refEntity.IsDashing ? (Vector3.right * _refEntity.direction) : _refEntity.ShootDirection);
		_refEntity.PlayerShootBuster(weaponStruct, true, (int)skillID, weaponStruct.ChargeLevel, value);
		_refEntity.CheckUsePassiveSkill((int)skillID, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
		_refEntity.EnableHandMesh(false);
	}

	protected override void OnPlayerReleaseSkill0(SkillID skillID)
	{
		int enhanceEXIndex = _refEntity.PlayerSkills[(int)skillID].EnhanceEXIndex;
		if (enhanceEXIndex != 2 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger((int)skillID))
		{
			ShootChargeBuster((int)skillID);
		}
	}

	protected override void OnPlayerReleaseSkill1(SkillID skillID)
	{
		base.OnPlayerReleaseSkill1(skillID);
	}

	protected override void LogicUpdateCharacterDepend()
	{
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CheckSkillEndByShootTimer())
		{
			ToggleWeapon(WeaponState.NORMAL);
		}
		base.LogicUpdateCharacterDepend();
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void ClearSkill()
	{
		switch ((SkillID)_refEntity.CurrentActiveSkill)
		{
		case SkillID.SKILL_0:
			_refEntity.CancelBusterChargeAtk();
			SetSkillEnd();
			break;
		case SkillID.SKILL_1:
			SetSkillEnd();
			break;
		}
	}

	protected override void OnLogicUpdate()
	{
		ForceUpdateVirtualButtonAnalog();
	}
}
