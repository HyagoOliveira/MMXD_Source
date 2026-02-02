using System;
using UnityEngine;

public class CH075_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected bool bDashShot;

	protected SKILL_TABLE _tSkill1_LinkSkill;

	protected bool bManualAim;

	protected Vector3 vSkill0ShootDir = Vector3.right;

	protected Vector3 vBellPostion;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch075_skill_02_stand", "ch075_skill_02_jump", "ch075_skill_02_crouch" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch075_skill_01_stand_up", "ch075_skill_01_stand_mid", "ch075_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch075_skill_01_jump_up", "ch075_skill_01_jump_mid", "ch075_skill_01_jump_down" };
		string[] array3 = new string[3] { "ch075_skill_01_crouch_up", "ch075_skill_01_crouch_mid", "ch075_skill_01_crouch_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override void Start()
	{
		base.Start();
		InitLinkSkill();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bellfield_000", 2);
	}

	private void InitLinkSkill()
	{
		if (_tSkill1_LinkSkill == null && _refEntity.PlayerSkills[1].BulletData.n_LINK_SKILL != 0)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out _tSkill1_LinkSkill))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref _tSkill1_LinkSkill);
				GameObject obj = new GameObject();
				CH075_BellFieldDummyBullet go = obj.AddComponent<CH075_BellFieldDummyBullet>();
				obj.name = _tSkill1_LinkSkill.s_MODEL;
				obj.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CH075_BellFieldDummyBullet>(go, _tSkill1_LinkSkill.s_MODEL);
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
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
		if (id == 1 && (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill1(id);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill0(id);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		string cue = "";
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
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				cue = "v_aa_skill05";
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				cue = "v_aa_skill05";
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				cue = "v_aa_skill05";
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				cue = "v_aa_skill04";
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				cue = "v_aa_skill04";
				break;
			}
			PlayVoiceSE(cue);
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
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IsShoot = 0;
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IsShoot = 0;
				SkillEndChnageToIdle(true);
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		{
			Vector3 value = _refEntity.ShootDirection;
			if (bDashShot)
			{
				value = Vector3.right * _refEntity.direction;
			}
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV, value);
			if (_tSkill1_LinkSkill != null)
			{
				_refEntity.PushBulletDetail(_tSkill1_LinkSkill, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV, value);
			}
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ModelTransform);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bellfield_000", vBellPostion, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			break;
		}
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if (curSubStatus != OrangeCharacter.SubStatus.SKILL0 && curSubStatus != OrangeCharacter.SubStatus.SKILL1)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
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
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (bManualAim)
			{
				_refEntity.ShootDirection = vSkill0ShootDir;
				_refEntity.Animator.SetAttackLayerActive(vSkill0ShootDir);
			}
			else if (vSkill0ShootDir != _refEntity.ShootDirection)
			{
				_refEntity.Animator.SetAttackLayerActive(_refEntity.ShootDirection);
			}
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.3f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.3f)
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.IsShoot = 0;
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.1f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (!bInSkill && CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.7f)
			{
				SkipSkill1Animation();
			}
			break;
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
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		TurnToAimTarget();
		bManualAim = !_refEntity.UseAutoAim;
		vSkill0ShootDir = _refEntity.ShootDirection;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
		}
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1)
		{
			SkillEndChnageToIdle();
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private void UseSkill1(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.DASH || _refEntity.CurMainStatus == OrangeCharacter.MainStatus.AIRDASH)
		{
			bDashShot = true;
			_refEntity.PlayerStopDashing();
		}
		else
		{
			bDashShot = false;
			_refEntity.PlayerStopDashing();
			TurnToAimTarget();
		}
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			vBellPostion = _refEntity.ModelTransform.position + new Vector3(0f, 2f, 0f);
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			vBellPostion = _refEntity.ModelTransform.position + new Vector3(0f, 3f, 0f);
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		_refEntity.IsShoot = 0;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1)
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
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.Animator._animator.speed = 1f;
		bInSkill = false;
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
				_refEntity.EnableHandMesh(true);
			}
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			break;
		}
	}
}
