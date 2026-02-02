using System;
using System.Collections.Generic;
using UnityEngine;

public class CH078_Controller : CharacterControllerProxyBaseGen4
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL1_STAND = 65u,
		ANI_SKILL1_CROUCH = 66u,
		ANI_SKILL1_JUMP = 67u
	}

	private enum FxName
	{
		fxuse_gaiachargeshot_000 = 0,
		fxuse_gaiagigaattack_000 = 1,
		fxuse_gaiagigaattack_001 = 2
	}

	public float DASH_SPEED_MULTIPLIER = 0.8f;

	public float TIME_SKILL_1_START = 0.2f;

	public float TIME_SKILL_1_CANCEL = 0.2f;

	public float FX_SKILL_1_OFFSET_X = 3.8f;

	public float FX_SKILL_1_OFFSET_Y = 1f;

	public float FX_SKILL_1_SCALE = 4f;

	private ChargeShootObj _chargeShootObj;

	private SkinnedMeshRenderer _busterMesh;

	private FxBase _fxSkill1Ref;

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.LOCK_WALL_JUMP, CancelWallClimbing);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.LOCK_WALL_JUMP, CancelWallClimbing);
	}

	private void CancelWallClimbing()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.WALLGRAB)
		{
			ManagedSingleton<CharacterControlHelper>.Instance.OffesetEntity(_refEntity, ManagedSingleton<CharacterControlHelper>.Instance.ClimbingOffset * _refEntity.direction);
		}
	}

	private void ShootChargeBuster(int skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[skillID];
		if (_refEntity.PlayerSkills[skillID].ChargeLevel <= 0)
		{
			_chargeShootObj.StopCharge();
			return;
		}
		_refEntity.Animator.SetAnimatorEquip(1);
		ToggleWeapon(WeaponState.SKILL_0);
		_chargeShootObj.ShootChargeBuster(skillID);
		_refEntity.EnableHandMesh(false);
	}

	private void ActionStatusChanged_1_0()
	{
		ResetSpeed();
		SetIgnoreGravity();
		ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		OrangeBattleUtility.UpdateSkillMeasure(_refEntity, weaponStruct);
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
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
		}
		_fxSkill1Ref = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fxuse_gaiagigaattack_000.ToString(), _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
		SetSkillFrame(TIME_SKILL_1_START);
	}

	private void ActionStatusChanged_1_1()
	{
		if (_fxSkill1Ref != null)
		{
			_fxSkill1Ref.BackToPool();
			_fxSkill1Ref = null;
		}
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
		_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
		_fxSkill1Ref = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fxuse_gaiagigaattack_001.ToString(), _refEntity.ModelTransform.position + new Vector3(FX_SKILL_1_OFFSET_X * (float)_refEntity.direction, FX_SKILL_1_OFFSET_Y), Quaternion.identity, Array.Empty<object>());
		_fxSkill1Ref.transform.localScale = Vector3.one * FX_SKILL_1_SCALE;
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
		return new string[3] { "ch078_skill_02_stand", "ch078_skill_02_crouch", "ch078_skill_02_jump" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[6] { "login", "logout", "win", "buster_stand_charge_atk", "buster_crouch_charge_atk", "buster_jump_charge_atk" };
		target = new string[6] { "ch078_login", "ch078_logout", "ch078_win", "ch078_skill_01_stand", "ch078_skill_01_crouch", "ch078_skill_01_jump" };
	}

	public override void GetUniqueWeaponMotion(out string[] source, out string[] target)
	{
		source = new string[1] { "_wallgrab_loop" };
		target = new string[1] { "_wallgrab_static_loop" };
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
			_refEntity.EnableHandMesh(false);
			_busterMesh.enabled = true;
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
		OnPlayerPressSkill1Events[0] = OnPlayerPressSkill1;
		OnPlayerReleaseSkill0Events[0] = OnPlayerReleaseSkill0;
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
				ShootChargeBuster((int)skillID);
			}
		}
		else
		{
			PlayVoiceSE("v_xg_skill01");
			_refEntity.Animator.SetAnimatorEquip(1);
			ToggleWeapon(WeaponState.SKILL_0);
			_refEntity.PlayerShootBuster(weaponStruct, true, (int)skillID, 0);
			_refEntity.EnableHandMesh(false);
		}
	}

	protected override void OnPlayerReleaseSkill0(SkillID skillID)
	{
		if (_refEntity.PlayerSetting.AutoCharge != 1)
		{
			ShootChargeBuster((int)skillID);
		}
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		if (_refEntity.selfBuffManager.nMeasureNow >= weaponStruct.BulletData.n_USE_COST)
		{
			PlayVoiceSE("v_xg_skill02");
			PlaySkillSE("xg_giga01");
			base.OnPlayerPressSkill1(skillID);
		}
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	protected override void SetSkillEnd()
	{
		if (_fxSkill1Ref != null)
		{
			_fxSkill1Ref.BackToPool();
			_fxSkill1Ref = null;
		}
		base.SetSkillEnd();
	}

	public override int DashSpeed()
	{
		return (int)((float)OrangeCharacter.DashSpeed * DASH_SPEED_MULTIPLIER);
	}

	public override int WallSlideGravity()
	{
		return 0;
	}
}
