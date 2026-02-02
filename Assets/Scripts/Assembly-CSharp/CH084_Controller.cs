using System;
using System.Collections.Generic;
using UnityEngine;

public class CH084_Controller : CharacterControllerProxyBaseGen3
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0_STAND = 65u,
		ANI_SKILL0_CROUCH = 66u,
		ANI_SKILL0_JUMP = 67u,
		ANI_SKILL1_START = 68u,
		ANI_SKILL1_LOOP = 69u,
		ANI_SKILL1_END = 70u
	}

	private enum FxName
	{
		fxuse_dsaber_000 = 0
	}

	public float TIME_SKILL_0_SHOOT = 0.2f;

	public float TIME_SKILL_0_CANCEL = 0.2f;

	public float SKILL_0_FX_SHIFT_X = 2f;

	public float SKILL_0_FX_SHIFT_Y = 0.7f;

	public float TIME_SKILL_1_DASH = 0.1f;

	public float TIME_SKILL_1_SABER_HIDE = 0.18f;

	public float TIME_SKILL_1_CANCEL = 0.2f;

	public float JUMP_SPEED_X_SKILL_1 = 10f;

	public float JUMP_SPEED_Y_SKILL_1 = 0.8f;

	private bool _isSkill1Jump;

	private SkinnedMeshRenderer _glassMesh;

	private SkinnedMeshRenderer _busterMesh;

	private SkinnedMeshRenderer _saberMesh;

	private GameObject _saberEffectObject;

	private int _frameSaberHide;

	private void ActionStatusChanged_0_0()
	{
		SetIgnoreGravity();
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
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
		SetSkillFrame(TIME_SKILL_0_SHOOT);
	}

	private void ActionStatusChanged_0_1()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, _refEntity.ShootDirection);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_dsaber_000.ToString(), _refEntity.ModelTransform.position + new Vector3((float)_refEntity.direction * SKILL_0_FX_SHIFT_X, SKILL_0_FX_SHIFT_Y, 0f), (_refEntity.direction > 0) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, new object[1]
		{
			new Vector3(_refEntity.direction, 1f)
		});
		SetSkillCancelFrame(TIME_SKILL_0_CANCEL);
	}

	private void ActionStatusChanged_1_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
		_isSkill1Jump = !_refEntity.IsInGround;
	}

	private void ActionStatusChanged_1_1()
	{
		SetSpeed((float)OrangeCharacter.WalkSpeed * JUMP_SPEED_X_SKILL_1 * (float)_refEntity.direction, (float)OrangeCharacter.JumpSpeed * JUMP_SPEED_Y_SKILL_1);
		_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
		SetSkillFrame(TIME_SKILL_1_DASH);
	}

	private void ActionStatusChanged_1_2()
	{
		ResetSpeed();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.IsShoot = 0;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.Controller.LogicPosition.vec3, weaponStruct.SkillLV);
		_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
		SetSkillCancelFrame(TIME_SKILL_1_CANCEL);
		_frameSaberHide = base.NowFrame + ConvertTimeToFrame(TIME_SKILL_1_SABER_HIDE);
	}

	protected void ActionSetNextSkillStatusSkill_1_0()
	{
		if (_isSkill1Jump)
		{
			_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 2);
		}
		else
		{
			ActionSetNextSkillStatus();
		}
	}

	protected void ActionCheckSkillCancel_1_2()
	{
		if (CheckFrameEnd(_frameSaberHide))
		{
			_saberEffectObject.SetActive(false);
		}
		if (CheckSkillCancel() && CheckIsAnyHeld())
		{
			ActionSetSkillEnd();
		}
	}

	public override void Awake()
	{
		base.Awake();
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "R WeaponPoint", true)
		};
		_glassMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "GlassesMesh_m").GetComponent<SkinnedMeshRenderer>();
		_busterMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "BusterMesh_G").GetComponent<SkinnedMeshRenderer>();
		_saberMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "SaberMesh_m").GetComponent<SkinnedMeshRenderer>();
		_saberEffectObject = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "DynamoSaber").gameObject;
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
					OnAnimationEnd = ActionSetNextSkillStatusSkill_1_0
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
					OnLogicUpdate = ActionCheckSkillCancel_1_2
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch084_skill_01_stand", "ch084_skill_01_crouch", "ch084_skill_01_jump", "ch084_skill_02_start", "ch084_skill_02_loop", "ch084_skill_02_end" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "login", "logout", "win" };
		target = new string[3] { "ch084_login", "ch084_logout", "ch084_win" };
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
		_glassMesh.enabled = isActive;
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
			_saberMesh.enabled = false;
			_saberEffectObject.SetActive(false);
			break;
		case WeaponState.SKILL_0:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			_busterMesh.enabled = false;
			_saberMesh.enabled = false;
			_saberEffectObject.SetActive(false);
			break;
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			_refEntity.EnableHandMesh(true);
			_busterMesh.enabled = false;
			_saberMesh.enabled = true;
			_saberEffectObject.SetActive(true);
			break;
		default:
			ToggleNormalWeapon(true);
			_busterMesh.enabled = false;
			_saberMesh.enabled = false;
			_saberEffectObject.SetActive(false);
			break;
		}
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerReleaseSkill0Events[0] = OnPlayerReleaseSkill0;
		OnPlayerPressSkill1Events[0] = OnPlayerPressSkill1;
	}

	protected override void OnPlayerReleaseSkill0(SkillID skillID)
	{
		PlayVoiceSE("v_dy_skill01");
		base.OnPlayerReleaseSkill0(skillID);
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		PlayVoiceSE("v_dy_skill02");
		PlaySkillSE("dy_tsubame");
		base.OnPlayerPressSkill1(skillID);
	}
}
