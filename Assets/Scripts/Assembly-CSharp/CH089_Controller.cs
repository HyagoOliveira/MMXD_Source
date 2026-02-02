using System;
using System.Collections.Generic;
using UnityEngine;

public class CH089_Controller : CharacterControllerProxyBaseGen3
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL1 = 65u,
		ANI_SKILL0 = 127u
	}

	private enum FxName
	{
		fxuse_icecream_000 = 0
	}

	public float SKILL_0_SHIFT_DIS = 0.8f;

	public float TIME_SKILL_0_START = 0.15f;

	public float TIME_SKILL_0_CANCEL = 0.05f;

	public float TIME_SKILL_0_END = 0.5f;

	public float SKILL_1_ANIMATION_SPEED = 2f;

	public float TIME_SKILL_1_START = 0.3f;

	public float TIME_SKILL_1_CANCEL = 0.15f;

	public float SKILL_1_SHIFT_Y = 2f;

	private SkinnedMeshRenderer _icecreamMeshObsolete;

	private SkinnedMeshRenderer _icecreamMesh;

	private SkinnedMeshRenderer _swimringMesh;

	private SkinnedMeshRenderer _weaponMesh;

	private void ActionStatusChanged_0_0()
	{
		SetIgnoreGravity();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		PlayVoiceSE("v_pl_skill03");
		_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
		SetSkillFrame(TIME_SKILL_0_START);
	}

	private void ActionStatusChanged_0_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.IsShoot = 0;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.AimPosition + _refEntity.ShootDirection * SKILL_0_SHIFT_DIS, weaponStruct.SkillLV, _refEntity.ShootDirection);
		SetSkillCancelFrame(TIME_SKILL_0_CANCEL);
		SetSkillFrame(TIME_SKILL_0_END);
	}

	private void ActionStatusChanged_1_0()
	{
		SetIgnoreGravity();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		PlayVoiceSE("v_pl_skill02");
		_refEntity.SoundSource.PlaySE(_refEntity.SkillSEID, "pl_icemissile01", 0.2f);
		_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
		base.AnimatorSpeed = SKILL_1_ANIMATION_SPEED;
		SetSkillFrame(TIME_SKILL_1_START);
	}

	private void ActionStatusChanged_1_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.IsShoot = 0;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ModelTransform.position + new Vector3(0f, SKILL_1_SHIFT_Y, 0f), weaponStruct.SkillLV, Vector3.up);
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
		_icecreamMeshObsolete = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "IceMesh_c").GetComponent<SkinnedMeshRenderer>();
		_icecreamMeshObsolete.enabled = false;
		_icecreamMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "LoginIceMesh_c").GetComponent<SkinnedMeshRenderer>();
		_swimringMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "LifebuoyMesh_c", true).GetComponent<SkinnedMeshRenderer>();
		_weaponMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "WeaponMesh_c").GetComponent<SkinnedMeshRenderer>();
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
					OnLogicUpdate = base.ActionCheckSkillCancelOrEnd
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_0,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_1,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch089_skill_02_stand" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch089_login", "ch089_logout", "ch089_win" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		return new string[1][] { new string[3] { "ch089_skill_01_stand_up", "ch089_skill_01_stand_mid", "ch089_skill_01_stand_down" } };
	}

	protected override void TeleportInCharacterDepend()
	{
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID == HumanBase.AnimateId.ANI_TELEPORT_IN_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 0.5f && currentFrame <= 2f)
			{
				_icecreamMesh.enabled = false;
			}
			if (currentFrame > 0.82f && currentFrame <= 2f)
			{
				_swimringMesh.enabled = false;
			}
		}
	}

	protected override void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 0.5f)
			{
				float num = 2f;
			}
		}
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		switch (weaponState)
		{
		case WeaponState.TELEPORT_IN:
			ToggleNormalWeapon(false);
			_weaponMesh.enabled = false;
			break;
		case WeaponState.TELEPORT_OUT:
			ToggleNormalWeapon(false);
			ToggleExtraTransforms(false);
			break;
		case WeaponState.SKILL_0:
			ToggleNormalWeapon(false);
			ToggleExtraTransforms(false);
			_weaponMesh.enabled = true;
			break;
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			ToggleExtraTransforms(false);
			break;
		default:
			ToggleNormalWeapon(true);
			ToggleExtraTransforms(false);
			break;
		}
	}

	protected override void ToggleExtraTransforms(bool isActive)
	{
		_icecreamMesh.enabled = isActive;
		_swimringMesh.enabled = isActive;
		_weaponMesh.enabled = isActive;
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerReleaseSkill0Events[0] = OnPlayerReleaseSkill0;
		OnPlayerPressSkill1Events[0] = OnPlayerPressSkill1;
	}

	protected override void OnPlayerReleaseSkill0(SkillID skillID)
	{
		IAimTarget autoAimTarget = base.AimSystem.AutoAimTarget;
		if (_refEntity.UseAutoAim && autoAimTarget != null)
		{
			_refEntity.ShootDirection = (autoAimTarget.AimPosition - _refEntity.AimPosition).normalized;
		}
		int num = Math.Sign(_refEntity.ShootDirection.x);
		_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
		base.OnPlayerReleaseSkill0(skillID);
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		IAimTarget autoAimTarget = base.AimSystem.AutoAimTarget;
		if (autoAimTarget != null)
		{
			int num = Math.Sign((autoAimTarget.AimPosition - _refEntity.AimPosition).normalized.x);
			_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
		}
		base.OnPlayerPressSkill1(skillID);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_icecream_000.ToString(), _refEntity._transform, OrangeCharacter.NormalQuaternion, new Vector3(base.SkillFXDirection, 1f, 1f), Array.Empty<object>());
	}
}
