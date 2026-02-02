using System;
using UnityEngine;
using enums;

public class RMZ_ZeroController : CharacterControlBase
{
	private enum SkillAnimationID
	{
		ANI_SKILL0_jump_get_nothing_start = 65,
		ANI_SKILL0_jump_get_nothing_end = 66,
		ANI_SKILL0_jump_get_something_start = 67,
		ANI_SKILL0_jump_get_something_end = 68,
		ANI_SKILL0_jump_rare_get_nothing_start = 69,
		ANI_SKILL0_jump_rare_get_nothing_end = 70,
		ANI_SKILL0_jump_rare_get_something_start = 71,
		ANI_SKILL0_jump_rare_get_something_end = 72,
		ANI_SKILL0_stand_get_nothing_start = 73,
		ANI_SKILL0_stand_get_nothing_end = 74,
		ANI_SKILL0_stand_get_something_start = 75,
		ANI_SKILL0_stand_get_something_end = 76,
		ANI_SKILL0_stand_rare_get_nothing_start = 77,
		ANI_SKILL0_stand_rare_get_nothing_end = 78,
		ANI_SKILL0_stand_rare_get_something_start = 79,
		ANI_SKILL0_stand_rare_get_something_end = 80,
		ANI_SKILL1_hold_shield_crouch_end = 81,
		ANI_SKILL1_hold_shield_crouch_loop = 82,
		ANI_SKILL1_hold_shield_crouch_start = 83,
		ANI_SKILL1_hold_shield_dash_end = 84,
		ANI_SKILL1_hold_shield_dash_loop = 85,
		ANI_SKILL1_hold_shield_dash_start = 86,
		ANI_SKILL1_hold_shield_fall_loop = 87,
		ANI_SKILL1_hold_shield_jump_loop = 88,
		ANI_SKILL1_hold_shield_jump_start = 89,
		ANI_SKILL1_hold_shield_jump_to_fall = 90,
		ANI_SKILL1_hold_shield_landing = 91,
		ANI_SKILL1_hold_shield_run_loop = 92,
		ANI_SKILL1_hold_shield_run_start = 93,
		ANI_SKILL1_hold_shield_stand_loop = 94,
		ANI_SKILL1_hold_shield_wallgrab_loop = 95,
		ANI_SKILL1_hold_shield_wallgrab_start = 96,
		ANI_SKILL1_hold_shield_wallgrab_step = 97,
		ANI_SKILL1_hold_shield_walljump_loop = 98,
		ANI_SKILL1_hold_shield_walljump_start = 99,
		ANI_SKILL1_jump_start = 100,
		ANI_SKILL1_jump_end = 101,
		ANI_SKILL1_stand_start = 102,
		ANI_SKILL1_stand_end = 103
	}

	private enum ShieldStstus
	{
		NONE = 0,
		SHIELD_SKILL_ATTACK_START = 1,
		SHIELD_SKILL_ATTACK_END = 2
	}

	private bool bInSkill;

	private bool bSkill0Hit;

	private Transform _tfShield;

	private PlayerCollider _shieldCollider;

	private Transform _tfRMZSaber;

	private CharacterMaterial _tfRMZSaberMaterial;

	private ShieldStstus nShieldStatus;

	private bool bShieldMode;

	private int nShieldType;

	private OrangeTimer _shieldTimer;

	private long nShieldTime;

	private int nLastSkill1Index;

	protected OrangeCharacter.MainStatus _lastMainStatus;

	protected OrangeCharacter.SubStatus _lastSubStatus;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[39]
		{
			"ch015_skill_01_jump_get_nothing_start", "ch015_skill_01_jump_get_nothing_end", "ch015_skill_01_jump_get_something_start", "ch015_skill_01_jump_get_something_end", "ch015_skill_01_jump_rare_get_nothing_start", "ch015_skill_01_jump_rare_get_nothing_end", "ch015_skill_01_jump_rare_get_something_start", "ch015_skill_01_jump_rare_get_something_end", "ch015_skill_01_stand_get_nothing_start", "ch015_skill_01_stand_get_nothing_end",
			"ch015_skill_01_stand_get_something_start", "ch015_skill_01_stand_get_something_end", "ch015_skill_01_stand_rare_get_nothing_start", "ch015_skill_01_stand_rare_get_nothing_end", "ch015_skill_01_stand_rare_get_something_start", "ch015_skill_01_stand_rare_get_something_end", "ch015_skill_02_hold_shield_crouch_end", "ch015_skill_02_hold_shield_crouch_loop", "ch015_skill_02_hold_shield_crouch_start", "ch015_skill_02_hold_shield_dash_end",
			"ch015_skill_02_hold_shield_dash_loop", "ch015_skill_02_hold_shield_dash_start", "ch015_skill_02_hold_shield_fall_loop", "ch015_skill_02_hold_shield_jump_loop", "ch015_skill_02_hold_shield_jump_start", "ch015_skill_02_hold_shield_jump_to_fall", "ch015_skill_02_hold_shield_landing", "ch015_skill_02_hold_shield_run_loop", "ch015_skill_02_hold_shield_run_start", "ch015_skill_02_hold_shield_stand_loop",
			"ch015_skill_02_hold_shield_wallgrab_loop", "ch015_skill_02_hold_shield_wallgrab_start", "ch015_skill_02_hold_shield_wallgrab_step", "ch015_skill_02_hold_shield_walljump_loop", "ch015_skill_02_hold_shield_walljump_start", "ch015_skill_02_jump_start", "ch015_skill_02_jump_end", "ch015_skill_02_stand_start", "ch015_skill_02_stand_end"
		};
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[2];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_tfShield = OrangeBattleUtility.FindChildRecursive(ref target, "p_shieldboomerang_000", true);
		_shieldCollider = _tfShield.GetComponentInChildren<PlayerCollider>();
		_shieldCollider.SetDmgReduceOwner(_refEntity._transform.GetComponent<StageObjParam>());
		_tfRMZSaber = OrangeBattleUtility.FindChildRecursive(ref target, "RMZ_Saber", true);
		_tfRMZSaberMaterial = _tfRMZSaber.GetComponent<CharacterMaterial>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_zeroknuckle_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_zeroknuckle_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_zeroknuckle_shield_000", 2);
		_shieldTimer = OrangeTimerManager.GetTimer();
		bShieldMode = true;
		SetShield(false, true);
		_tfRMZSaberMaterial.Disappear();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerPressSkillCB = PlayerPressSkill;
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.GetCurrentAimRangeEvt = GetCurrentAimRange;
		_refEntity.OverrideAnimatorParamtersEvt = OverrideAnimatorParamters;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.CheckDashLockEvt = CheckDashLock;
		_refEntity.EnterRideArmorEvt = OnEnterRideArmor;
	}

	public override void CheckSkill()
	{
		if (bShieldMode && nShieldStatus == ShieldStstus.NONE && (_shieldTimer.GetMillisecond() > nShieldTime || ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.SHOOT)))
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[nLastSkill1Index + 1].n_ID);
			SetShield(false);
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
			if (!_refEntity.Dashing)
			{
				ResetToIdle();
			}
			_refEntity.EnableCurrentWeapon();
		}
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill >= 0 && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL && nShieldStatus != 0)
		{
			_refEntity.SkillEnd = true;
			ResetToIdle();
			_refEntity.IgnoreGravity = false;
			_refEntity.CurrentActiveSkill = -1;
			SetShieldStatus(ShieldStstus.NONE);
			return;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (bInSkill)
			{
				bInSkill = false;
				bSkill0Hit = false;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				_refEntity.PlaySE(_refEntity.VoiceID, "v_zz_skill01");
				_refEntity.PlaySE(_refEntity.SkillSEID, "zz_knuckle");
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (CheckCancelAnimate(0))
			{
				_refEntity.SkillEnd = true;
				bSkill0Hit = false;
				ResetToIdle();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (bInSkill)
			{
				if (!_refEntity.PreBelow && _refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.IgnoreGravity = false;
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				}
				else
				{
					bInSkill = false;
					bSkill0Hit = false;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
					CreateSkillBullet(_refEntity.GetCurrentSkillObj());
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
					_refEntity.PlaySE(_refEntity.VoiceID, "v_zz_skill01");
					_refEntity.PlaySE(_refEntity.SkillSEID, "zz_knuckle");
				}
			}
			else if (!_refEntity.PreBelow && _refEntity.Controller.Collisions.below)
			{
				if (bSkill0Hit)
				{
					_refEntity.Dashing = false;
					_refEntity.IgnoreGravity = false;
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				}
				else
				{
					_refEntity.SkillEnd = true;
					bSkill0Hit = false;
					_refEntity.Dashing = false;
					_refEntity.SetSpeed(0, 0);
					ResetToIdle();
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (CheckCancelAnimate(0))
			{
				_refEntity.SkillEnd = true;
				bSkill0Hit = false;
				_refEntity.Dashing = false;
				_refEntity.IgnoreGravity = false;
				_refEntity.SetSpeed(0, 0);
				ResetToIdle();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			switch (nShieldStatus)
			{
			case ShieldStstus.SHIELD_SKILL_ATTACK_START:
				if (!_refEntity.PreBelow)
				{
					_refEntity.IgnoreGravity = true;
				}
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.CurrentFrame > 0.5f && bInSkill)
				{
					bInSkill = false;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
					CreateSkillBullet(_refEntity.PlayerSkills[1]);
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0], null, nLastSkill1Index);
				}
				break;
			case ShieldStstus.SHIELD_SKILL_ATTACK_END:
				if (_refEntity.CurrentFrame > 1f)
				{
					_refEntity.SkillEnd = true;
					ResetToIdle();
					_refEntity.IgnoreGravity = false;
					_refEntity.CurrentActiveSkill = -1;
					SetShieldStatus(ShieldStstus.NONE);
				}
				break;
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			bSkill0Hit = false;
			break;
		case 1:
			if (_refEntity.PlayerSkills[1].MagazineRemain > 0f)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
			}
			if (nShieldStatus == ShieldStstus.NONE)
			{
				_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[nLastSkill1Index + 1].n_ID);
			}
			else
			{
				_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[nLastSkill1Index].n_ID);
				nShieldStatus = ShieldStstus.NONE;
			}
			SetShield(false);
			bInSkill = false;
			break;
		}
		_refEntity.Dashing = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length != 2)
		{
			return;
		}
		int num = (int)parameters[0];
		int num2 = (int)parameters[1];
		if (num != 1)
		{
			return;
		}
		if (num2 == 0)
		{
			SetShield(false);
			if (_refEntity is OrangeConsoleCharacter)
			{
				OrangeConsoleCharacter obj = _refEntity as OrangeConsoleCharacter;
				obj.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
				obj.ClearVirtualButtonStick(VirtualButtonId.SKILL1);
			}
		}
		else if (!_refEntity.IsStun)
		{
			SetShield(true);
			if (_refEntity is OrangeConsoleCharacter)
			{
				(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
			}
			ResetToIdle();
		}
	}

	private void SetShield(bool setEnable, bool isInitialize = false)
	{
		if (bShieldMode == setEnable)
		{
			return;
		}
		_refEntity.IgnoreGravity = false;
		bShieldMode = setEnable;
		if (bShieldMode)
		{
			_refEntity.DisableCurrentWeapon();
			_refEntity.EnableHandMesh(true);
			_refEntity.StopShootTimer();
			_refEntity.SkillEnd = false;
			_refEntity.PlayerAutoAimSystem.UpdateAimRange(_refEntity.PlayerSkills[1].BulletData.f_DISTANCE);
			_tfShield.gameObject.SetActive(true);
			ParticleSystem componentInChildren = _tfShield.GetComponentInChildren<ParticleSystem>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Play();
				_refEntity.PlaySE(_refEntity.SkillSEID, "zz_boomerang01");
				_refEntity.PlaySE(_refEntity.VoiceID, "v_zz_skill02_1");
			}
			_shieldTimer.TimerStart();
		}
		else
		{
			if (!isInitialize)
			{
				_refEntity.EnableCurrentWeapon();
			}
			_shieldTimer.TimerStop();
			_refEntity.UpdateAimRangeByWeapon(_refEntity.GetCurrentWeaponObj());
			_tfShield.gameObject.SetActive(false);
			ParticleSystem componentInChildren2 = _tfShield.GetComponentInChildren<ParticleSystem>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.Stop();
			}
		}
	}

	public override bool CheckMyShield(Transform tfObject)
	{
		return _shieldCollider.transform == tfObject;
	}

	public override PlayerCollider GetMyShield(bool checkUsing = true)
	{
		if (checkUsing && !bShieldMode)
		{
			return null;
		}
		return _shieldCollider;
	}

	public override int ShieldDmgReduce(HurtPassParam tHurtPassParam)
	{
		int result = 100;
		bool flag = tHurtPassParam.IsHitShield;
		if (!tHurtPassParam.IsHitShield && bShieldMode && ((tHurtPassParam.S_Direction.x > 0f && _refEntity._characterDirection == CharacterDirection.LEFT) || (tHurtPassParam.S_Direction.x < 0f && _refEntity._characterDirection == CharacterDirection.RIGHT)) && !tHurtPassParam.IsThrough && !tHurtPassParam.IsSplash && tHurtPassParam.Skill_Type == 1 && tHurtPassParam.wpnType != WeaponType.Spray && tHurtPassParam.wpnType != WeaponType.SprayHeavy)
		{
			flag = true;
		}
		if (flag && _refEntity.PlayerSkills[1].BulletData.n_EFFECT == 15)
		{
			result = (int)_refEntity.PlayerSkills[1].BulletData.f_EFFECT_Y;
		}
		return result;
	}

	public bool CheckDashLock()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			if (!bShieldMode)
			{
				return nShieldType == 0;
			}
			return false;
		}
		return false;
	}

	private bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_refEntity.CurrentActiveSkill == 1)
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkill1Index + 1].n_ID);
			SetShield(false);
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public void PlayerPressSkill(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && _refEntity.PlayerSkills[id].LastUseTimer.GetMillisecond() >= _refEntity.PlayerSkills[id].BulletData.n_FIRE_SPEED && !(_refEntity.PlayerSkills[id].MagazineRemain <= 0f) && !_refEntity.PlayerSkills[id].ForceLock && (_refEntity.CurrentActiveSkill == -1 || _refEntity.CurrentActiveSkill == 1))
		{
			_refEntity.PreBelow = _refEntity.Controller.Collisions.below;
			PlayerPressSkillCharacterCall(id);
		}
	}

	public void PlayerReleaseSkill(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && _refEntity.PlayerSkills[id].LastUseTimer.GetMillisecond() >= _refEntity.PlayerSkills[id].BulletData.n_FIRE_SPEED && !(_refEntity.PlayerSkills[id].MagazineRemain <= 0f) && !_refEntity.PlayerSkills[id].ForceLock && (_refEntity.CurrentActiveSkill == -1 || _refEntity.CurrentActiveSkill == 1))
		{
			PlayerReleaseSkillCharacterCall(id);
			_refEntity.PreBelow = _refEntity.Controller.Collisions.below;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill == 1 && bShieldMode && nShieldStatus == ShieldStstus.NONE)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkill1Index + 1].n_ID);
				SetShield(false);
				_refEntity.CurrentActiveSkill = -1;
			}
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetSpeed(0, 0);
				}
				_refEntity.StopShootTimer();
				int num = Math.Sign(_refEntity.ShootDirection.x);
				if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(_refEntity.ShootDirection.x) > 0.05f)
				{
					_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				}
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				}
			}
			break;
		case 1:
		{
			ParticleSystem componentInChildren = _tfShield.GetComponentInChildren<ParticleSystem>();
			if ((bool)componentInChildren && componentInChildren.isStopped)
			{
				OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
				int num2 = 50;
			}
			break;
		}
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id != 1 || (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1) || !_refEntity.CheckUseSkillKeyTrigger(id, !bShieldMode))
		{
			return;
		}
		_refEntity.CurrentActiveSkill = id;
		_refEntity.SkillEnd = false;
		bInSkill = true;
		nLastSkill1Index = _refEntity.PlayerSkills[1].Reload_index;
		_shieldTimer.TimerStart();
		if (_refEntity.PlayerSkills[1].BulletData.n_EFFECT == 15)
		{
			if (_refEntity.PlayerSkills[1].BulletData.f_EFFECT_X == 0f)
			{
				nShieldType = 0;
			}
			else
			{
				nShieldType = 1;
			}
			nShieldTime = (long)(_refEntity.PlayerSkills[1].BulletData.f_EFFECT_Z * 1000f);
		}
		if (!bShieldMode)
		{
			_refEntity.IsShoot = 0;
			_lastMainStatus = _refEntity.CurMainStatus;
			_lastSubStatus = _refEntity.CurSubStatus;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_zeroknuckle_shield_000", _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_zeroknuckle_shield_000", _refEntity.ExtraTransforms[0].position, Quaternion.identity, Array.Empty<object>());
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[nLastSkill1Index].n_ID);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus == OrangeCharacter.SubStatus.WIN_POSE)
			{
				_tfRMZSaberMaterial.Appear();
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0]);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
			{
				_refEntity.EnableCurrentWeapon();
				_tfShield.gameObject.SetActive(false);
				ParticleSystem componentInChildren = _tfShield.GetComponentInChildren<ParticleSystem>();
				if ((bool)componentInChildren)
				{
					componentInChildren.Stop();
				}
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetSpeed(0, 0);
				}
				_refEntity.IgnoreGravity = false;
				SetShieldStatus(ShieldStstus.SHIELD_SKILL_ATTACK_START);
				break;
			}
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (bSkill0Hit)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				break;
			}
			_refEntity.SkillEnd = true;
			bSkill0Hit = false;
			_refEntity.IsShoot = 0;
			_refEntity.EnableCurrentWeapon();
			ResetToIdle();
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SkillEnd = true;
			bSkill0Hit = false;
			_refEntity.IgnoreGravity = false;
			_refEntity.EnableCurrentWeapon();
			ResetToIdle();
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (bSkill0Hit)
			{
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
				}
				break;
			}
			_refEntity.SkillEnd = true;
			bSkill0Hit = false;
			_refEntity.IsShoot = 0;
			_refEntity.EnableCurrentWeapon();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				ResetToIdle();
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.SkillEnd = true;
			bSkill0Hit = false;
			_refEntity.IgnoreGravity = false;
			_refEntity.EnableCurrentWeapon();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				ResetToIdle();
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
		{
			ShieldStstus shieldStstus = nShieldStatus;
			if (shieldStstus != ShieldStstus.SHIELD_SKILL_ATTACK_START)
			{
				int num = 2;
			}
			else
			{
				SetShieldStatus(ShieldStstus.SHIELD_SKILL_ATTACK_END);
			}
			break;
		}
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, Vector2.right * (float)_refEntity._characterDirection, true, null, BulletHitCB);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_zeroknuckle_000", _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_zeroknuckle_001", _refEntity.ExtraTransforms[0].position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.PlaySE(_refEntity.VoiceID, "v_zz_skill02_2");
				_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[_refEntity.CurrentActiveSkill], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[1], weaponStruct.SkillLV);
				break;
			}
		}
	}

	public void BulletHitCB(object obj)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL0 || curSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
			{
				bSkill0Hit = true;
			}
		}
	}

	private void SetShieldStatus(ShieldStstus shieldStatus, bool resetFrame = true)
	{
		nShieldStatus = shieldStatus;
		SkillAnimationID skillAnimationID = SkillAnimationID.ANI_SKILL1_hold_shield_stand_loop;
		switch (nShieldStatus)
		{
		default:
			_refEntity.IgnoreGravity = false;
			ResetToIdle();
			return;
		case ShieldStstus.SHIELD_SKILL_ATTACK_START:
			skillAnimationID = ((!_refEntity.Controller.Collisions.below && !_refEntity.Controller.Collisions.JSB_below) ? SkillAnimationID.ANI_SKILL1_jump_start : SkillAnimationID.ANI_SKILL1_stand_start);
			break;
		case ShieldStstus.SHIELD_SKILL_ATTACK_END:
			skillAnimationID = ((!_refEntity.Controller.Collisions.below && !_refEntity.Controller.Collisions.JSB_below) ? SkillAnimationID.ANI_SKILL1_jump_end : SkillAnimationID.ANI_SKILL1_stand_end);
			break;
		}
		if (resetFrame)
		{
			_refEntity.SetAnimateId((HumanBase.AnimateId)skillAnimationID);
		}
		else
		{
			_refEntity.ForceSetAnimateId((HumanBase.AnimateId)skillAnimationID);
		}
	}

	public void OverrideAnimatorParamters()
	{
		if (bShieldMode)
		{
			short animateUpperID = _refEntity.AnimationParams.AnimateUpperID;
			switch (_refEntity.AnimateID)
			{
			case HumanBase.AnimateId.ANI_STAND:
				animateUpperID = 94;
				break;
			case HumanBase.AnimateId.ANI_STEP:
				animateUpperID = 93;
				break;
			case HumanBase.AnimateId.ANI_WALK:
				animateUpperID = 92;
				break;
			case HumanBase.AnimateId.ANI_CROUCH:
				animateUpperID = 83;
				break;
			case HumanBase.AnimateId.ANI_CROUCH_END:
				animateUpperID = 82;
				break;
			case HumanBase.AnimateId.ANI_CROUCH_UP:
				animateUpperID = 81;
				break;
			case HumanBase.AnimateId.ANI_JUMP:
				animateUpperID = 89;
				break;
			case HumanBase.AnimateId.ANI_FALL:
				animateUpperID = 90;
				break;
			case HumanBase.AnimateId.ANI_LAND:
				animateUpperID = 91;
				break;
			case HumanBase.AnimateId.ANI_DASH:
				animateUpperID = 85;
				break;
			case HumanBase.AnimateId.ANI_DASH_END:
				animateUpperID = 84;
				break;
			case HumanBase.AnimateId.ANI_WALLGRAB_BEGIN:
				animateUpperID = 97;
				break;
			case HumanBase.AnimateId.ANI_WALLGRAB:
				animateUpperID = 96;
				break;
			case HumanBase.AnimateId.ANI_WALLGRAB_END:
				animateUpperID = 95;
				break;
			case HumanBase.AnimateId.ANI_WALLKICK:
				animateUpperID = 99;
				break;
			case HumanBase.AnimateId.ANI_WALLKICK_END:
				animateUpperID = 98;
				break;
			}
			_refEntity.AnimationParams.AnimateUpperID = animateUpperID;
		}
	}

	private void ResetToIdle()
	{
		if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
		{
			_refEntity.IgnoreGravity = false;
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_lastMainStatus = OrangeCharacter.MainStatus.IDLE;
			_lastSubStatus = OrangeCharacter.SubStatus.IDLE;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_lastMainStatus = OrangeCharacter.MainStatus.FALL;
			_lastSubStatus = OrangeCharacter.SubStatus.TELEPORT_POSE;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	public override float GetCurrentAimRange()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 49) <= 1u)
			{
				return _refEntity.PlayerSkills[1].BulletData.f_DISTANCE;
			}
		}
		return _refEntity.GetCurrentAimRange();
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		if (skilliD == 0 && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
		{
			return true;
		}
		return false;
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			_refEntity.EnableCurrentWeapon();
		}
	}
}
