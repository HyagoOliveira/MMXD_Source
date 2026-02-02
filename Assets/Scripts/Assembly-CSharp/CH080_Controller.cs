using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

public class CH080_Controller : CharacterControllerProxyBaseGen3
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0_STAND = 65u,
		ANI_SKILL0_CROUCH = 66u,
		ANI_SKILL0_JUMP = 67u,
		ANI_SKILL1_STAND = 127u,
		ANI_SKILL1_CROUCH = 128u,
		ANI_SKILL1_JUMP = 129u
	}

	private enum FxName
	{
		fxuse_rollswing_000 = 0
	}

	public float TIME_SKILL_0_START = 0.1f;

	public float TIME_SKILL_0_CANCEL = 0.4f;

	public float SKILL_0_FX_SHIFT_X = -0.1f;

	public float SKILL_0_FX_SHIFT_Y = 1.4f;

	public float SKILL_0_FX_SHIFT_Y_CROUCH = 0.9f;

	public float SKILL_1_SHIFT_Y = 1.1f;

	public float SKILL_1_SHIFT_Y_JUMP = 1.1f;

	public float SKILL_1_SHIFT_Y_CROUCH = 0.7f;

	public float SKILL_1_SHIFT_DIS = 0.5f;

	public float TIME_SKILL_1_CANCEL = 0.2f;

	private SkinnedMeshRenderer _busterMesh;

	private SkinnedMeshRenderer _bandMesh;

	private FxBase _fxSkill0;

	private SKILL_TABLE _linkSkill0;

	private bool _isEnableLinkSkill0;

	private void ActionStatusChanged_0_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
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
		_isEnableLinkSkill0 = false;
		SetSkillFrame(TIME_SKILL_0_START);
	}

	private void ActionStatusChanged_0_1()
	{
		float y = SKILL_0_FX_SHIFT_Y;
		if (_refEntity.IsInGround && _refEntity.IsCrouching)
		{
			y = SKILL_0_FX_SHIFT_Y_CROUCH;
		}
		_fxSkill0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fxuse_rollswing_000.ToString(), _refEntity.ModelTransform.position + new Vector3((_refEntity.direction > 0) ? SKILL_0_FX_SHIFT_X : (0f - SKILL_0_FX_SHIFT_X), y, 0f), (_refEntity.direction > 0) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
		SetSkillCancelFrame(TIME_SKILL_0_CANCEL);
	}

	protected void ActionCheckSkillCancel_0_1()
	{
		if (CheckSkillCancel() && CheckIsAnyHeld())
		{
			ActionSetSkillEnd_0_1();
		}
	}

	protected void ActionSetSkillEnd_0_1()
	{
		_isEnableLinkSkill0 = true;
		SetSkillEnd();
	}

	private void ActionStatusChanged_1_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		float y = SKILL_1_SHIFT_Y;
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				y = SKILL_1_SHIFT_Y_CROUCH;
			}
			else
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			}
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
			y = SKILL_1_SHIFT_Y_JUMP;
		}
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ModelTransform.position + new Vector3(0f, y, 0f) + _refEntity.ShootDirection * SKILL_1_SHIFT_DIS, weaponStruct.SkillLV, _refEntity.ShootDirection);
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
		_bandMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "BodyMesh_a").GetComponent<SkinnedMeshRenderer>();
	}

	public override void Start()
	{
		base.Start();
		_refEntity.AnimatorModelShiftYOverride = new Better.Dictionary<OrangeCharacter.MainStatus, float>
		{
			{
				OrangeCharacter.MainStatus.TELEPORT_IN,
				0.05f
			},
			{
				OrangeCharacter.MainStatus.TELEPORT_OUT,
				0.1f
			},
			{
				OrangeCharacter.MainStatus.CROUCH,
				-0.05f
			},
			{
				OrangeCharacter.MainStatus.SKILL,
				0.1f
			},
			{
				OrangeCharacter.MainStatus.GIGA_ATTACK,
				0.1f
			}
		};
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		InitializeSkillDependDelegators(new System.Collections.Generic.Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData>
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
					OnAnimationEnd = ActionSetSkillEnd_0_1,
					OnLogicUpdate = ActionCheckSkillCancel_0_1
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
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		SKILL_TABLE value;
		if (weaponStruct.BulletData.n_LINK_SKILL != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out value))
		{
			_linkSkill0 = value;
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref _linkSkill0);
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch080_skill_01_stand", "ch080_skill_01_crouch", "ch080_skill_01_jump" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		return new string[3][]
		{
			new string[3] { "ch080_skill_02_stand_up", "ch080_skill_02_stand_mid", "ch080_skill_02_stand_down" },
			new string[3] { "ch080_skill_02_crouch_up", "ch080_skill_02_crouch_mid", "ch080_skill_02_crouch_down" },
			new string[3] { "ch080_skill_02_jump_up", "ch080_skill_02_jump_mid", "ch080_skill_02_jump_down" }
		};
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch080_login", "ch080_logout", "ch080_win" };
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
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				ToggleExtraTransforms(false);
			}
		}
	}

	protected override void StageTeleportInCharacterDepend()
	{
		StartCoroutine(ToggleExtraTransforms(true, 0.6f));
	}

	protected override void StageTeleportOutCharacterDepend()
	{
		StartCoroutine(ToggleExtraTransforms(false, 0.2f));
	}

	public override void ControlCharacterDead()
	{
		StartCoroutine(ToggleExtraTransforms(false, 0.5f));
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(ToggleExtraTransforms(true, 0.6f));
	}

	protected override void ToggleExtraTransforms(bool isActive)
	{
		_bandMesh.enabled = isActive;
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
			_bandMesh.enabled = true;
			break;
		case WeaponState.SKILL_0:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			_busterMesh.enabled = false;
			_bandMesh.enabled = false;
			break;
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			_busterMesh.enabled = true;
			_bandMesh.enabled = true;
			break;
		default:
			ToggleNormalWeapon(true);
			_busterMesh.enabled = false;
			_bandMesh.enabled = true;
			break;
		}
	}

	protected override void SetSkillEnd()
	{
		if (base.CurActiveSkill == 0)
		{
			if (_fxSkill0 != null)
			{
				_fxSkill0.BackToPool();
				_fxSkill0 = null;
			}
			if (_isEnableLinkSkill0 && _linkSkill0 != null)
			{
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
				_refEntity.PushBulletDetail(_linkSkill0, weaponStruct.weaponStatus, _refEntity.ModelTransform, weaponStruct.SkillLV);
			}
		}
		base.SetSkillEnd();
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerPressSkill0Events[0] = OnPlayerPressSkill0;
		OnPlayerReleaseSkill1Events[0] = OnPlayerReleaseSkill1;
	}

	protected override void OnPlayerPressSkill0(SkillID skillID)
	{
		PlaySkillSE("rx_rollwhip01");
		base.OnPlayerPressSkill0(skillID);
	}

	protected override void OnPlayerReleaseSkill1(SkillID skillID)
	{
		PlaySkillSE("rx_rollarrow");
		base.OnPlayerReleaseSkill1(skillID);
	}
}
