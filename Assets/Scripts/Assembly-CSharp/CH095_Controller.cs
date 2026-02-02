using System;
using System.Collections.Generic;
using UnityEngine;

public class CH095_Controller : CharacterControllerProxyBaseGen3
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0 = 65u,
		ANI_SKILL1 = 66u
	}

	private enum FxName
	{
		fxuse_batammo_000 = 0,
		fxuse_ghostfire_000 = 1
	}

	public float TIME_SKILL_0_START = 0.1f;

	public float TIME_SKILL_0_CANCEL = 0.2f;

	public float SKILL_0_SHIFT_Y = 1f;

	public float TIME_SKILL_1_CANCEL = 0.5f;

	public float SKILL_1_FX_SHIFT_X = 0.6f;

	public float SKILL_1_FX_SHIFT_Y = 1.8f;

	private void ActionStatusChanged_0_0()
	{
		SetIgnoreGravity();
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
		SetSkillFrame(TIME_SKILL_0_START);
	}

	private void ActionStatusChanged_0_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.IsShoot = 0;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ModelTransform.position + new Vector3(0f, SKILL_0_SHIFT_Y, 0f), weaponStruct.SkillLV, Vector3.up);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_batammo_000.ToString(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		SetSkillCancelFrame(TIME_SKILL_0_CANCEL);
	}

	private void ActionStatusChanged_1_0()
	{
		SetIgnoreGravity();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.IsShoot = 0;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_ghostfire_000.ToString(), _refEntity.ModelTransform.position + new Vector3(SKILL_1_FX_SHIFT_X * (float)_refEntity.direction, SKILL_1_FX_SHIFT_Y, 0f), Quaternion.identity, Array.Empty<object>());
		_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
		_refEntity.Animator._animator.speed = 2f;
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
	}

	public override void Start()
	{
		base.Start();
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		InitializeSkillDependDelegators(new Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData>
		{
			{
				OrangeCharacter.SubStatus.SKILL0,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_0,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_1,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_0,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch095_skill_01_stand", "ch095_skill_02_stand" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch095_login", "ch095_logout", "ch095_win" };
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
			if (currentFrame > 1.5f)
			{
				float num = 2f;
			}
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
			break;
		case WeaponState.SKILL_0:
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			break;
		default:
			ToggleNormalWeapon(true);
			break;
		}
	}

	protected override void OnPlayerPressSkill0(SkillID skillID)
	{
		PlayVoiceSE("v_sg_skill02");
		PlaySkillSE("sg_bat");
		base.OnPlayerPressSkill0(skillID);
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		PlayVoiceSE("v_sg_skill01");
		base.OnPlayerPressSkill1(skillID);
	}
}
