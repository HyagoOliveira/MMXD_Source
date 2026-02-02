using System;
using UnityEngine;

public class CH101_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected ObjInfoBar mEffect_Hide_obj;

	protected SkinnedMeshRenderer _tfWeaponLMesh;

	protected SkinnedMeshRenderer _tfWeaponRMesh;

	protected Vector3 _vSkill2ShootDirection;

	protected ParticleSystem _psFxSkill1;

	protected ParticleSystem _psFxAura;

	protected ParticleSystem _psFxAura2;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[12]
		{
			"ch101_skill_01_stand", "ch101_skill_01_jump", "ch101_skill_01_crouch", "ch101_skill_02_stand_start", "ch101_skill_02_stand_loop", "ch101_skill_02_stand_end", "ch101_skill_02_jump_start", "ch101_skill_02_jump_loop", "ch101_skill_02_jump_end", "ch101_skill_02_crouch_start",
			"ch101_skill_02_crouch_loop", "ch101_skill_02_crouch_end"
		};
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
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill0ShootPosition", true);
		_refEntity.ExtraTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill1ShootPosition", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_L_m", true);
		_tfWeaponLMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponLMesh.enabled = false;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_R_m", true);
		_tfWeaponRMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponRMesh.enabled = false;
		_psFxSkill1 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_chaosnightmare_001_", true).GetComponent<ParticleSystem>();
		_psFxSkill1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		_psFxAura = OrangeBattleUtility.FindChildRecursive(ref target, "tttt_002 (1)", true).GetComponent<ParticleSystem>();
		_psFxAura2 = OrangeBattleUtility.FindChildRecursive(ref target, "fxdemo_ch079_body_digital_code", true).GetComponent<ParticleSystem>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_hellthrowing_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_chaosnightmare_000", 5);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
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
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill0(id);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill1(id);
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
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_psFxSkill1.Play();
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				TurnToShootDirection(_vSkill2ShootDirection);
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_psFxSkill1.Play();
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				TurnToShootDirection(_vSkill2ShootDirection);
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_6:
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_7:
				_psFxSkill1.Play();
				_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_8:
				TurnToShootDirection(_vSkill2ShootDirection);
				_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
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
				if ((bool)mEffect_Hide_obj)
				{
					mEffect_Hide_obj.gameObject.SetActive(true);
				}
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_6:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_7);
				break;
			case OrangeCharacter.SubStatus.SKILL1_8:
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
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV, Vector3.right * _refEntity.direction);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.AimTransform);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
			case OrangeCharacter.SubStatus.SKILL1_5:
			case OrangeCharacter.SubStatus.SKILL1_8:
				_psFxSkill1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[3], wsSkill.SkillLV, _vSkill2ShootDirection);
				break;
			}
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
	}

	public void StageTeleportOutCharacterDepend()
	{
		if ((bool)_psFxAura)
		{
			_psFxAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		if ((bool)_psFxAura2)
		{
			_psFxAura2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	public void StageTeleportInCharacterDepend()
	{
		if ((bool)_psFxAura)
		{
			_psFxAura.Play(true);
		}
		if ((bool)_psFxAura2)
		{
			_psFxAura2.Play(true);
		}
	}

	public void TeleportInExtraEffect()
	{
		mEffect_Hide_obj = _refEntity.transform.GetComponentInChildren<ObjInfoBar>();
		if ((bool)mEffect_Hide_obj)
		{
			mEffect_Hide_obj.gameObject.SetActive(false);
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
			else if (bInSkill && _refEntity.CurrentFrame > 0.1f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.35f)
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_4:
		case OrangeCharacter.SubStatus.SKILL1_7:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, _refEntity.CurSubStatus + 1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
		case OrangeCharacter.SubStatus.SKILL1_5:
		case OrangeCharacter.SubStatus.SKILL1_8:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill)
			{
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				bInSkill = false;
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.4f)
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
			if (_refEntity.direction != num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void TurnToShootDirection(Vector3 dir)
	{
		int num = Math.Sign(dir.x);
		if (_refEntity.direction != num && Mathf.Abs(dir.x) > 0.05f)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			_refEntity.ShootDirection = dir;
		}
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 0;
		ToggleWeapon(1);
		PlayVoiceSE("v_fg_skill03");
		PlaySkillSE("fg_rolling01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_hellthrowing_000", _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
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
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
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
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		if (_refEntity.UseAutoAim)
		{
			TurnToAimTarget();
		}
		_vSkill2ShootDirection = _refEntity.ShootDirection;
		PlayVoiceSE("v_fg_skill04");
		PlaySkillSE("fg_chaos");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_chaosnightmare_000", _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
		_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.AimTransform);
		OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_6);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
		}
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		_psFxSkill1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		if (_refEntity.CurSubStatus >= OrangeCharacter.SubStatus.SKILL1_6)
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
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_8)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
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
		_refEntity.GravityMultiplier = new VInt(1f);
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

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -3:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponLMesh.enabled = false;
			_tfWeaponRMesh.enabled = false;
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponLMesh.enabled = false;
			_tfWeaponRMesh.enabled = false;
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponLMesh.enabled = false;
			_tfWeaponRMesh.enabled = false;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponLMesh.enabled = false;
			_tfWeaponRMesh.enabled = false;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponLMesh.enabled = false;
			_tfWeaponRMesh.enabled = false;
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfWeaponLMesh.enabled = false;
			_tfWeaponRMesh.enabled = false;
			break;
		}
	}
}
