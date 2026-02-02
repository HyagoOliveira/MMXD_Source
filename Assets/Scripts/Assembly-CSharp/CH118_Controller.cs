using System;
using UnityEngine;

public class CH118_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMeshL;

	protected SkinnedMeshRenderer _tfWeaponMeshR;

	protected SkinnedMeshRenderer _tfLHandMesh;

	protected ChargeShootObj _refChargeShootObj;

	[Header("億兆爆破")]
	private int _gigaCrushHoverFrame;

	private CollideBullet _gigaCrushCollider;

	public ch_007_Skill_effect GigaCrushEffect;

	protected Vector3 Skill1Directioon = Vector3.right;

	private SKILL_TABLE _iceCreamBullet;

	private readonly int hashDirection = Animator.StringToHash("fDirection");

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch118_skill_02_prepare_loop", "ch118_skill_02_prepare_to_charge", "ch118_skill_02_charge_loop", "ch118_skill_02_charge_to_burst", "ch118_skill_02_burst_loop", "ch118_skill_02_brust_to_fall" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_crouch_charge_atk" };
		target = new string[3] { "ch081_skill_01_stand", "ch081_skill_01_fall", "ch081_skill_01_crouch" };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		InitLinkSkill();
	}

	private void InitLinkSkill()
	{
		SKILL_TABLE value = null;
		if (value != null || _refEntity.PlayerSkills[1].BulletData.n_LINK_SKILL == 0)
		{
			return;
		}
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out value))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref value);
			_iceCreamBullet = value;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(_iceCreamBullet.s_MODEL) && !MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(_iceCreamBullet.s_MODEL))
			{
				BulletBase.PreloadBullet<BasicBullet>(_iceCreamBullet);
			}
		}
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		if (_gigaCrushCollider == null)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = "GigaCrushCollider";
			_gigaCrushCollider = gameObject.AddComponent<CollideBullet>();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_gigacrush_000");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_gigacrush_001");
		GigaCrushEffect = OrangeBattleUtility.FindChildRecursive(base.transform, "mesheffect").GetComponent<ch_007_Skill_effect>();
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_L_m");
		if (transform != null)
		{
			_tfWeaponMeshL = transform.GetComponent<SkinnedMeshRenderer>();
			_tfWeaponMeshL.enabled = false;
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_R_m");
		if (transform != null)
		{
			_tfWeaponMeshR = transform.GetComponent<SkinnedMeshRenderer>();
			_tfWeaponMeshR.enabled = false;
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m");
		if (transform != null)
		{
			_tfLHandMesh = transform.GetComponent<SkinnedMeshRenderer>();
			_tfLHandMesh.enabled = true;
		}
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			CancelSkill0();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			_refEntity.SkillEnd = true;
		}
		_refEntity.CurrentActiveSkill = -1;
		ToggleWeapon(0);
	}

	public override void CheckSkill()
	{
		if (!_refEntity.IsAnimateIDChanged())
		{
			UpdateSkill();
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
		}
		else
		{
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				return;
			}
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[0].Reload_index == 0)
				{
					_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
					_refChargeShootObj.StartCharge();
				}
				else if (_refEntity.CurrentActiveSkill == -1)
				{
					UseSkill0(id);
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				UseSkill0(id);
			}
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.selfBuffManager.nMeasureNow >= _refEntity.PlayerSkills[id].BulletData.n_USE_COST && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.selfBuffManager.AddMeasure(-_refEntity.PlayerSkills[id].BulletData.n_USE_COST);
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[1]);
				_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f));
				_gigaCrushHoverFrame = 3;
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				DebutOrClearStageToggleWeapon(false);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				DebutOrClearStageToggleWeapon(false);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				PlayVoiceSE("v_x2_skill02");
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				PlaySkillSE("x2_giga01");
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_gigacrush_000", base.transform.position + new Vector3(0f, 0.7f, 0f), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_gigacrush_001", base.transform.position + new Vector3(0f, 0.7f, 0f), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				GigaCrushEffect.ActiveEffect(true);
				_gigaCrushHoverFrame = 15;
				PlaySkillSE("x2_giga02");
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.EnableCurrentWeapon();
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				_refEntity.IgnoreGravity = false;
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				ToggleWeapon(0);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				GigaCrushEffect.ActiveEffect(false);
				if (_refEntity.GetCurrentWeaponObj().ChipEfx != null && _refEntity.GetCurrentWeaponObj().chip_switch)
				{
					_refEntity.GetCurrentWeaponObj().ChipEfx.isActive = false;
					_refEntity.GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(_refEntity.tRefPassiveskill.bUsePassiveskill);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.SkillEnd = true;
				GigaCrushEffect.ActiveEffect(false);
				if (_refEntity.GetCurrentWeaponObj().ChipEfx != null && _refEntity.GetCurrentWeaponObj().chip_switch)
				{
					_refEntity.GetCurrentWeaponObj().ChipEfx.isActive = false;
					_refEntity.GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(_refEntity.tRefPassiveskill.bUsePassiveskill);
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
	}

	public override void ControlCharacterDead()
	{
		_gigaCrushHoverFrame = 0;
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	private void UpdateSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.CheckSkillEndByShootTimer();
			if (_refEntity.SkillEnd)
			{
				ToggleWeapon(0);
			}
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				if (_gigaCrushHoverFrame > 0)
				{
					_gigaCrushHoverFrame--;
					if (_gigaCrushHoverFrame > 0)
					{
						_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.4f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.4f));
						break;
					}
					_refEntity.IgnoreGravity = true;
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
					_gigaCrushHoverFrame = 5;
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (_gigaCrushHoverFrame > 0)
				{
					_gigaCrushHoverFrame--;
					if (_gigaCrushHoverFrame <= 0)
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				if (_gigaCrushHoverFrame <= 0)
				{
					break;
				}
				_gigaCrushHoverFrame--;
				if (_gigaCrushHoverFrame == 10)
				{
					int nowRecordNO = _refEntity.GetNowRecordNO();
					_gigaCrushCollider.UpdateBulletData(_refEntity.PlayerSkills[1].BulletData, _refEntity.sPlayerName, nowRecordNO, _refEntity.nBulletRecordID++);
					_gigaCrushCollider.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
					_gigaCrushCollider.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
					_gigaCrushCollider.Active(base.transform, Vector2.right * (float)_refEntity._characterDirection, _refEntity.TargetMask);
					_refEntity.IsShoot = 1;
					_refEntity.PushBulletDetail(_iceCreamBullet, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.AimTransform.position, _refEntity.PlayerSkills[1].SkillLV, Vector3.up);
				}
				if (_gigaCrushHoverFrame <= 0)
				{
					_refEntity.IsShoot = 0;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
					GigaCrushEffect.ActiveEffect(false);
					if (_refEntity.GetCurrentWeaponObj().ChipEfx != null && _refEntity.GetCurrentWeaponObj().chip_switch)
					{
						_refEntity.GetCurrentWeaponObj().ChipEfx.isActive = false;
						_refEntity.GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(_refEntity.tRefPassiveskill.bUsePassiveskill);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_3:
				break;
			}
		}
		else if (GigaCrushEffect.isActive)
		{
			GigaCrushEffect.ActiveEffect(false);
			if (_refEntity.GetCurrentWeaponObj().ChipEfx != null && _refEntity.GetCurrentWeaponObj().chip_switch)
			{
				_refEntity.GetCurrentWeaponObj().ChipEfx.isActive = false;
				_refEntity.GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(_refEntity.tRefPassiveskill.bUsePassiveskill);
			}
		}
	}

	private void UseSkill0(int skillId)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		if (_refEntity.PlayerSkills[0].WeaponMesh[0] != null)
		{
			_refEntity.PlayerSkills[0].WeaponMesh[0].Disappear();
		}
		ToggleWeapon(1);
		_refEntity.CurrentActiveSkill = 0;
		_refChargeShootObj.StopCharge();
		Vector3? shotDir = null;
		if (IsDashStatus())
		{
			shotDir = Vector3.right * _refEntity.direction;
		}
		sbyte chargeLevel = weaponStruct.ChargeLevel;
		if ((uint)chargeLevel > 1u)
		{
			if (chargeLevel == 2)
			{
				goto IL_017f;
			}
		}
		else
		{
			if (weaponStruct.Reload_index > 1)
			{
				goto IL_017f;
			}
			PlayVoiceSE("v_x2_skill01");
			_refEntity.IsShoot = (sbyte)(weaponStruct.ChargeLevel + 1);
			_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, shotDir);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel].n_USE_COST, -1f);
		}
		goto IL_0306;
		IL_017f:
		if (weaponStruct.Reload_index == 0)
		{
			weaponStruct.Reload_index = 2;
		}
		PlayVoiceSE("v_x2_skill01");
		PlaySkillSE("x2_w_chargeshot_l");
		_refEntity.IsShoot = 1;
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[weaponStruct.Reload_index], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, shotDir);
		_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
		if (weaponStruct.Reload_index < _refEntity.PlayerSkills[0].FastBulletDatas.Length - 1)
		{
			if (_refEntity.IsLocalPlayer)
			{
				int reload_index = weaponStruct.Reload_index;
				OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[reload_index].n_USE_COST, -1f);
				_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[reload_index].n_ID);
				_refEntity.TriggerComboSkillBuff(_refEntity.PlayerSkills[0].FastBulletDatas[reload_index].n_ID);
			}
		}
		else
		{
			OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_USE_COST, -1f);
			_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_ID);
			weaponStruct.Reload_index = 0;
		}
		goto IL_0306;
		IL_0306:
		_refEntity.StartShootTimer();
		_refEntity.Animator.SetAnimatorEquip(1);
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, _refEntity.ShootDirection)) / 180f;
		_refEntity.Animator._animator.SetFloat(hashDirection, value);
	}

	private bool IsDashStatus()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if ((uint)(curMainStatus - 4) <= 1u && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			return true;
		}
		return false;
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		_refEntity.CancelBusterChargeAtk();
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		SkillEndChnageToIdle();
	}

	private void UseSkill1(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 0;
		ToggleWeapon(2);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch067_skill1_000", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
		}
	}

	private void CancelSkill1(bool isCrouch)
	{
		SkillEndChnageToIdle(isCrouch);
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 1:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		case 0:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		bInSkill = false;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 49) <= 2u)
			{
				_refEntity.ShootDirection = ((_refEntity._characterDirection == CharacterDirection.RIGHT) ? Vector3.right : Vector3.left);
			}
		}
		ToggleWeapon(0);
		if (isCrouch)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void DebutOrClearStageToggleWeapon(bool bDebut)
	{
		ToggleWeapon(-1);
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMeshL.enabled = false;
			_tfWeaponMeshR.enabled = false;
			_tfLHandMesh.enabled = true;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMeshL.enabled = true;
			_tfWeaponMeshR.enabled = false;
			_tfLHandMesh.enabled = false;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMeshL.enabled = false;
			_tfWeaponMeshR.enabled = false;
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfWeaponMeshL.enabled = false;
			_tfWeaponMeshR.enabled = false;
			break;
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) <= 2u)
		{
			_refEntity._characterDirection = ((Skill1Directioon.x >= 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length == 2)
		{
			int num = (int)parameters[0];
			int num2 = (int)parameters[1];
			if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT && (int)_refEntity.Hp > 0 && num == 0 && _refEntity.PlayerSkills[0].Reload_index != num2)
			{
				_refEntity.PlayerSkills[0].Reload_index = num2;
			}
		}
	}
}
