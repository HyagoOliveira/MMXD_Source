using System;
using UnityEngine;

public class CH048_Controller : CharacterControlBase
{
	private bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected ParticleSystem _fxWeapon;

	protected CH048_BeamBullet _beam;

	protected int _nLockSkill1Direction;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch048_skill_01_stand", "ch048_skill_01_jump", "ch048_skill_01_crouch" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch048_skill_02_stand_up", "ch048_skill_02_stand_mid", "ch048_skill_02_stand_down" };
		string[] array2 = new string[3] { "ch048_skill_02_jump_up", "ch048_skill_02_jump_mid", "ch048_skill_02_jump_down" };
		string[] array3 = new string[3] { "ch048_skill_02_crouch_up", "ch048_skill_02_crouch_mid", "ch048_skill_02_crouch_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "normal_damage_start", "normal_damage_loop", "normal_damage_end" };
		target = new string[3] { "ch048_hurt_start", "ch048_hurt_loop", "ch048_hurt_end" };
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[4];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill01_ShootPoint", true);
		_refEntity.ExtraTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill01PS_ShootPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "SickleMesh_m");
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "SickleFX");
		_fxWeapon = transform2.GetComponent<ParticleSystem>();
		_fxWeapon.Stop();
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<CH048_BeamBullet>("prefab/bullet/p_eyebeam_001", "p_eyebeam_001", 4, null);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch048_skill_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch048_skill_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch048_skill_002", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
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
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_sg02_skill01");
				_refEntity.SoundSource.PlaySE(_refEntity.SkillSEID, "sg02_kama", 0.3f);
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_ch048_skill_000", _refEntity.transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_sg02_skill02");
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_ch048_skill_001", _refEntity.transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				UseSkill1(id);
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
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
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
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
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
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_2:
				_tfWeaponMesh.enabled = false;
				_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_ch048_skill_002", _refEntity.ExtraTransforms[0].position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ExtraTransforms[3], Vector3.up);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				_nLockSkill1Direction = (int)_refEntity._characterDirection;
				break;
			}
		}
	}

	public override void ControlCharacterDead()
	{
	}

	public void TeleportOutCharacterDepend()
	{
	}

	public void StageTeleportOutCharacterDepend()
	{
		_tfWeaponMesh.enabled = false;
		_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}

	public void StageTeleportInCharacterDepend()
	{
		_tfWeaponMesh.enabled = false;
		_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}

	public void TeleportInCharacterDepend()
	{
		if (!_tfWeaponMesh.enabled)
		{
			DebutOrClearStageToggleWeapon(true);
		}
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("sg02_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch048_startin_000";
	}

	protected void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) <= 2u)
		{
			if (_nLockSkill1Direction != 0)
			{
				_refEntity._characterDirection = (CharacterDirection)_nLockSkill1Direction;
			}
			else
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	private void UpdateSkill()
	{
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
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.3f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0))
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.25f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (!bInSkill && CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.8f)
			{
				SkipSkill1Animation();
			}
			break;
		}
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
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
		_nLockSkill1Direction = 0;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
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

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1)
		{
			SkillEndChnageToIdle();
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
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
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
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
		if (bDebut)
		{
			ToggleWeapon(-2);
		}
		else
		{
			ToggleWeapon(-1);
		}
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_fxWeapon.Play();
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_fxWeapon.Play();
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
			_tfWeaponMesh.enabled = false;
			_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			break;
		}
	}
}
