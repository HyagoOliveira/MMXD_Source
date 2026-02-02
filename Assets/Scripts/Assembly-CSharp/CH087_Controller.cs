using System;
using System.Collections.Generic;
using UnityEngine;

public class CH087_Controller : CharacterControllerProxyBaseGen3
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0_STAND_START = 65u,
		ANI_SKILL0_STAND_LOOP = 66u,
		ANI_SKILL1_STAND_START = 67u,
		ANI_SKILL1_STAND_LOOP = 68u,
		ANI_SKILL1_STAND_END = 69u,
		ANI_SKILL0_STAND_END = 127u
	}

	private enum FxName
	{
		fxuse_sfchargeshot_000 = 0,
		fxuse_sfchargeshot_001 = 1
	}

	public float TIME_SKILL_0_END_SHOOT = 0.1f;

	private Vector3 _shootVector;

	public float TIME_SKILL_1_LOOP = 0.25f;

	public float TIME_SKILL_1_CANCEL = 0.05f;

	public float JUMP_ANIMATION_SPEED_SKILL_1 = 2f;

	public float JUMP_SPEED_X_SKILL_1 = 0.8f;

	public float JUMP_SPEED_Y_SKILL_1 = 0.8f;

	public float MOVE_SPEED_X_SKILL_1 = 4f;

	private GameObject _fxBeamObject;

	private ParticleSystem _fxBeamObjectFX;

	private SkinnedMeshRenderer _busterMesh;

	private SkinnedMeshRenderer _handMesh;

	private void ClearBeamFX()
	{
		_fxBeamObject.SetActive(false);
		_fxBeamObjectFX.Stop();
	}

	private void ActionStatusChanged_0_0()
	{
		SetIgnoreGravity();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
		_handMesh.enabled = false;
		_busterMesh.enabled = true;
		SetSkillFrame(TIME_SKILL_0_END_SHOOT);
	}

	private void ActionStatusChanged_0_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		PlayVoiceSE("v_fo_skill01");
		_refEntity.IsShoot = 0;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, _shootVector);
		PlaySkillSE("fo_buster01");
		float num = Mathf.Atan2(_shootVector.y, _shootVector.x);
		Vector3 one = Vector3.one;
		if (Mathf.Abs(num) - 90f > 0f)
		{
			num = (180f - Mathf.Abs(num)) * Mathf.Sign(num);
			one.x = -1f;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_sfchargeshot_001.ToString(), _refEntity.ExtraTransforms[0].position, Quaternion.Euler(new Vector3(0f, 0f, num * 57.29578f)), new object[1] { one });
	}

	private void ActionStatusChanged_1_0()
	{
		SetIgnoreGravity(false);
		PlayVoiceSE("v_fo_skill03");
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		if (_refEntity.IsInGround)
		{
			SetSpeed((float)OrangeCharacter.WalkSpeed * JUMP_SPEED_X_SKILL_1 * (float)_refEntity.direction, (float)OrangeCharacter.JumpSpeed * JUMP_SPEED_Y_SKILL_1);
		}
		PlaySkillSE("fo_lazer");
		_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
		base.AnimatorSpeed = JUMP_ANIMATION_SPEED_SKILL_1;
	}

	private void ActionStatusChanged_1_1()
	{
		EnableColliderBullet();
		_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
		base.AnimatorSpeed = 1f;
		_handMesh.enabled = false;
		_busterMesh.enabled = true;
		_fxBeamObject.SetActive(true);
		_fxBeamObjectFX.Play();
		SetSpeed((float)OrangeCharacter.DashSpeed * MOVE_SPEED_X_SKILL_1 * (float)_refEntity.direction, 0f);
		SetSkillFrame(TIME_SKILL_1_LOOP);
	}

	private void ActionStatusChanged_1_2()
	{
		DisableColliderBullet();
		_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
		_handMesh.enabled = true;
		_busterMesh.enabled = false;
		ClearBeamFX();
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
		_handMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "HandMesh_L_m").GetComponent<SkinnedMeshRenderer>();
		_busterMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "BusterMesh_m").GetComponent<SkinnedMeshRenderer>();
		_fxBeamObject = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "p_sfbeam_000", true).gameObject;
		_fxBeamObjectFX = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "p_sfbeam_Fx", true).GetComponent<ParticleSystem>();
	}

	public override void Start()
	{
		base.Start();
		_fxBeamObject.SetLayer(ManagedSingleton<OrangeLayerManager>.Instance.FxLayer, true);
		ClearBeamFX();
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
					OnAnimationEnd = base.ActionSetNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_1,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
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
		return new string[5] { "ch087_skill_01_stand_start", "ch087_skill_01_stand_loop", "ch087_skill_02_stand_start", "ch087_skill_02_stand_loop", "ch087_skill_02_stand_end" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch087_login", "ch087_logout", "ch087_win" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		return new string[1][] { new string[3] { "ch087_skill_01_stand_end_up", "ch087_skill_01_stand_end_mid", "ch087_skill_01_stand_end_down" } };
	}

	protected override void TeleportInCharacterDepend()
	{
		ToggleWeapon(WeaponState.TELEPORT_IN);
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
		case WeaponState.TELEPORT_OUT:
			ToggleNormalWeapon(false);
			_handMesh.enabled = true;
			_busterMesh.enabled = false;
			break;
		case WeaponState.SKILL_0:
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			_handMesh.enabled = true;
			_busterMesh.enabled = false;
			break;
		default:
			ToggleNormalWeapon(true);
			_handMesh.enabled = false;
			_busterMesh.enabled = false;
			break;
		}
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			ClearBeamFX();
		}
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
		_shootVector = _refEntity.ShootDirection;
		int num = Math.Sign(_refEntity.ShootDirection.x);
		_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
		base.OnPlayerReleaseSkill0(skillID);
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		base.OnPlayerPressSkill1(skillID);
	}

	protected override void SetSkillEnd()
	{
		ClearBeamFX();
		base.SetSkillEnd();
	}
}
