using System;
using UnityEngine;

public class CH074_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected SkinnedMeshRenderer _tfLHandMesh;

	protected ChargeShootObj _refChargeShootObj;

	protected FxBase _fbGigaAttackUseFx;

	protected FxBase _fbGigaAttackDuringFx;

	private readonly int hashDirection = Animator.StringToHash("fDirection");

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch074_skill_02_stand", "ch074_skill_02_jump", "ch074_skill_02_crouch" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[4] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[4] { "ch074_skill_01_stand", "ch074_skill_01_fall", "ch074_skill_01_wallgrab", "ch074_skill_01_crouch" };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void Start()
	{
		base.Start();
		InitLinkSkill();
		InitPet();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m", true);
		_tfLHandMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		_tfLHandMesh.enabled = true;
		_refChargeShootObj = _refEntity.ChargeObject;
		_refChargeShootObj.StopCharge();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_kaigigaattack_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_kaigigaattack_000", 2);
	}

	private void InitLinkSkill()
	{
	}

	private void InitPet()
	{
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			CancelSkill0();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			CancelSkill1();
		}
		_refEntity.CurrentActiveSkill = -1;
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
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[0].Reload_index == 0)
				{
					_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
				}
				else if (_refEntity.CurrentActiveSkill == -1)
				{
					UseSkill0(id);
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				UseSkill0(id);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill1(id);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1)
		{
			if (id != 0)
			{
				int num = 1;
			}
			else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill0(id);
			}
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
				ToggleWeapon(-2);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				ToggleWeapon(-3);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
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
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_2:
				SkillEndChnageToIdle();
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 49) <= 2u)
			{
				_fbGigaAttackDuringFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxduring_kaigigaattack_000", _refEntity.AimTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.AimTransform, wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.AimTransform);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
			}
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
	}

	public void PlayerHeldSkill(int id)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.HURT:
			return;
		case OrangeCharacter.MainStatus.SKILL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 19) <= 1u)
			{
				return;
			}
			break;
		}
		}
		if (_refEntity.PlayerSetting.AutoCharge == 0 && !_refEntity.PlayerSkills[id].ForceLock && _refEntity.PlayerSkills[id].Reload_index == 0 && !_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[id].FastBulletDatas[0].n_CHARGE_MAX_LEVEL != 0 && _refEntity.CheckUseSkillKeyTriggerEX(id))
		{
			_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
		}
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
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) > 1u && (uint)(curSubStatus - 49) <= 2u)
		{
			if (bInSkill && _refEntity.CurrentFrame > 0.2f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.5f)
			{
				SkipSkill1Animation();
			}
			else if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
		}
	}

	private void TurnToAimTarget()
	{
		Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
		if (vector.HasValue)
		{
			int num = Math.Sign(vector.Value.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void UseSkill0(int skillId)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		_refEntity.CurrentActiveSkill = 0;
		ToggleWeapon(1);
		_refChargeShootObj.StopCharge();
		Vector3 vector = _refEntity.ShootDirection;
		if (IsDashStatus())
		{
			vector = Vector3.right * _refEntity.direction;
		}
		if (weaponStruct.ChargeLevel == 2)
		{
			_refEntity.IsShoot = 1;
			_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, vector);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel].n_USE_COST, -1f);
			if (_refEntity.IsLocalPlayer)
			{
				_refEntity.TriggerComboSkillBuff(_refEntity.PlayerSkills[0].FastBulletDatas[2].n_ID);
			}
		}
		else if (weaponStruct.Reload_index != 0)
		{
			_refEntity.IsShoot = 1;
			_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[weaponStruct.Reload_index], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, vector);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_USE_COST, -1f);
			_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_ID);
		}
		else
		{
			_refEntity.IsShoot = (sbyte)(weaponStruct.ChargeLevel + 1);
			_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, vector);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel].n_USE_COST, -1f);
		}
		_refEntity.StartShootTimer();
		_refEntity.Animator.SetAnimatorEquip(1);
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, vector)) / 180f;
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
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		TurnToAimTarget();
		PlaySkillSE("xk_homing01");
		_fbGigaAttackUseFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_kaigigaattack_000", _refEntity.AimTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
		}
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		SkillEndChnageToIdle();
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
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
		_refEntity.GravityMultiplier = new VInt(1f);
		_refEntity.Animator._animator.speed = 1f;
		bInSkill = false;
		if (_fbGigaAttackUseFx != null)
		{
			_fbGigaAttackUseFx.BackToPool();
			_fbGigaAttackUseFx = null;
		}
		if (_fbGigaAttackDuringFx != null)
		{
			_fbGigaAttackDuringFx.BackToPool();
			_fbGigaAttackDuringFx = null;
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

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -3:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_tfLHandMesh.enabled = false;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfWeaponMesh.enabled = false;
			break;
		}
	}
}
