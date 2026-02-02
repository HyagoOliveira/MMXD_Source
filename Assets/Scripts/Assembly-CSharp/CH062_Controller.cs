using System;
using System.Collections.Generic;
using UnityEngine;

public class CH062_Controller : CharacterControllerProxyBaseGen3
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL1_STAND_START = 65u,
		ANI_SKILL1_STAND_END = 66u,
		ANI_SKILL1_CROUCH_START = 67u,
		ANI_SKILL1_CROUCH_END = 68u,
		ANI_SKILL1_JUMP_START = 69u,
		ANI_SKILL1_JUMP_END = 70u
	}

	private enum FxName
	{
		fxuse_novacircle_000 = 0,
		fxuse_xfield_000 = 1
	}

	public float TIME_SKILL_0_SHOOT = 0.5f;

	public float TIME_SKILL_0_CANCEL = 0.2f;

	public float TIME_SKILL_1_CANCEL = 0.15f;

	private SkinnedMeshRenderer _busterMesh;

	private SkinnedMeshRenderer _wingMesh;

	private ParticleSystem _wingEffect;

	private ShieldBullet _shieldBullet;

	private ChargeShootObj _chargeShootObj;

	private GameObject _chargeFxObj;

	private void ShootChargeBuster(int skillID)
	{
		if (_refEntity.PlayerSkills[skillID].ChargeLevel <= 0)
		{
			_chargeShootObj.StopCharge();
			_chargeFxObj.SetActive(false);
			return;
		}
		_refEntity.Animator.SetAnimatorEquip(1);
		ToggleWeapon(WeaponState.SKILL_0);
		_chargeShootObj.ShootChargeBuster(skillID);
		_refEntity.EnableHandMesh(false);
		_chargeFxObj.SetActive(false);
	}

	private void UpdateShieldState()
	{
		int n_CONDITION_ID = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		if (_refEntity.IsDead())
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(n_CONDITION_ID);
		}
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
		{
			ToggleShield(true);
		}
		else
		{
			ToggleShield(false);
		}
	}

	private void ToggleShield(bool isEnable)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[0];
		int n_CONDITION_ID = sKILL_TABLE.n_CONDITION_ID;
		if (isEnable)
		{
			if (_shieldBullet.bIsEnd)
			{
				_shieldBullet.UpdateBulletData(sKILL_TABLE, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_shieldBullet.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_shieldBullet.BulletLevel = weaponStruct.SkillLV;
				_shieldBullet.BindBuffId(n_CONDITION_ID, _refEntity.IsLocalPlayer);
				_shieldBullet.Active(_refEntity.transform, Quaternion.identity, _refEntity.TargetMask, true, (weaponStruct.BulletData.n_TRACKING > 0) ? _refEntity.PlayerAutoAimSystem.AutoAimTarget : null);
			}
		}
		else
		{
			if (!_shieldBullet.bIsEnd)
			{
				_shieldBullet.Reset_Duration_Time();
				_shieldBullet.BackToPool();
			}
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
			{
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(n_CONDITION_ID);
			}
		}
	}

	private void ActionStatusChanged_1_0()
	{
		SetIgnoreGravity();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_novacircle_000.ToString(), _refEntity.AimTransform, Quaternion.identity, Array.Empty<object>());
		ToggleShield(true);
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			else
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
		}
		else
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
		}
	}

	private void ActionStatusChanged_1_1()
	{
		_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
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
		_wingMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "WingMesh_g").GetComponent<SkinnedMeshRenderer>();
	}

	public override void Start()
	{
		base.Start();
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_shieldBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ShieldBullet>(_refEntity.PlayerSkills[1].BulletData.s_MODEL);
		_chargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		_chargeFxObj = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "fxduring_divechargeshot_002").gameObject;
		_chargeFxObj.SetActive(false);
		_wingEffect = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "CH062_WingEffect").GetComponent<ParticleSystem>();
		_wingEffect.Play(true);
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
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch062_dive_trigger_stand_start", "ch062_dive_trigger_stand_end", "ch062_dive_trigger_crouch_start", "ch062_dive_trigger_crouch_end", "ch062_dive_trigger_jump_start", "ch062_dive_trigger_jump_end" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[8] { "login", "logout", "win", "buster_stand_charge_atk", "buster_crouch_charge_atk", "buster_jump_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk" };
		target = new string[8] { "ch062_login", "ch062_logout", "ch062_win", "ch062_skill_01_stand", "ch062_skill_01_crouch", "ch062_skill_01_jump", "ch062_skill_01_fall", "ch062_skill_01_wallgrab" };
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
		_chargeFxObj.SetActive(false);
		if (_refEntity.CurSubStatus != 0)
		{
			return;
		}
		float currentFrame = _refEntity.CurrentFrame;
		if (currentFrame > 1.5f && currentFrame <= 2f)
		{
			if (_wingMesh.enabled)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_xfield_000.ToString(), _wingMesh.transform.position, Quaternion.identity, Array.Empty<object>());
			}
			ToggleExtraTransforms(false);
		}
	}

	protected override void StageTeleportInCharacterDepend()
	{
		if (_wingMesh != null && _wingMesh.enabled)
		{
			StopAllCoroutines();
			return;
		}
		ToggleExtraTransforms(false);
		StartCoroutine(ToggleExtraTransforms(true, 0.6f));
	}

	protected override void StageTeleportOutCharacterDepend()
	{
		if (base.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT)
		{
			StartCoroutine(ToggleExtraTransforms(false, 0.2f));
		}
		else if (!_refEntity.Animator.IsDefaultAnimator)
		{
			StartCoroutine(ToggleExtraTransforms(false, 0.2f));
		}
	}

	public override void ControlCharacterDead()
	{
		ToggleExtraTransforms(false);
	}

	public override void ControlCharacterContinue()
	{
		ToggleExtraTransforms(false);
		StartCoroutine(ToggleExtraTransforms(true, 0.6f));
	}

	protected override void ToggleExtraTransforms(bool isActive)
	{
		_wingMesh.enabled = isActive;
		if (isActive)
		{
			_wingEffect.Play(true);
		}
		else
		{
			_wingEffect.Stop(true);
		}
	}

	protected override bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		int n_CONDITION_ID = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(n_CONDITION_ID);
		}
		_chargeFxObj.SetActive(false);
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
				_chargeFxObj.SetActive(true);
			}
			else
			{
				ShootChargeBuster((int)skillID);
			}
		}
		else
		{
			_refEntity.Animator.SetAnimatorEquip(1);
			ToggleWeapon(WeaponState.SKILL_0);
			_refEntity.PlayerShootBuster(weaponStruct, true, (int)skillID, 0);
			_refEntity.EnableHandMesh(false);
		}
	}

	protected override void OnPlayerReleaseSkill0(SkillID skillID)
	{
		if (_refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger((int)skillID))
		{
			ShootChargeBuster((int)skillID);
		}
	}

	protected override void OnPlayerPressSkill1(SkillID skillID)
	{
		PlayVoiceSE("v_xd_skill02");
		PlaySkillSE("xd_novaring02");
		base.OnPlayerPressSkill1(skillID);
	}

	protected override void OnLogicUpdate()
	{
		UpdateShieldState();
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

	protected override void SetSkillEnd()
	{
		_chargeFxObj.SetActive(false);
		base.SetSkillEnd();
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
}
