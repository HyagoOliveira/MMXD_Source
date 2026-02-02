using System;
using UnityEngine;

public class CH086_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected SkinnedMeshRenderer _tfSwordMesh;

	protected SkinnedMeshRenderer _tfRHandMesh;

	protected SkinnedMeshRenderer _tfBusterMesh;

	protected ParticleSystem _psModelFx;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch086_skill_01_stand" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch086_skill_02_stand_up", "ch086_skill_02_stand_mid", "ch086_skill_02_stand_down" };
		string[] array2 = new string[3] { "ch086_skill_02_stand_up", "ch086_skill_02_stand_mid", "ch086_skill_02_stand_down" };
		string[] array3 = new string[3] { "ch086_skill_02_stand_up", "ch086_skill_02_stand_mid", "ch086_skill_02_stand_down" };
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
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill1ShotPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_m", true);
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_g", true);
		_tfSwordMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		_tfSwordMesh.enabled = false;
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		_tfBusterMesh = transform3.GetComponent<SkinnedMeshRenderer>();
		_tfBusterMesh.enabled = false;
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_R_c", true);
		_tfRHandMesh = transform4.GetComponent<SkinnedMeshRenderer>();
		_tfRHandMesh.enabled = true;
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "mode_fx", true);
		_psModelFx = transform5.GetComponent<ParticleSystem>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_darksword_000", 5);
	}

	private void InitLinkSkill()
	{
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
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
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
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
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_darksword_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ModelTransform);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ExtraTransforms[2]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
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
		if ((bool)_psModelFx)
		{
			_psModelFx.Stop();
		}
	}

	public void StageTeleportInCharacterDepend()
	{
		if ((bool)_psModelFx)
		{
			_psModelFx.Play();
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
			if (bInSkill && _refEntity.CurrentFrame > 0.2f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.5f)
			{
				SkipSkill0Animation();
			}
			else if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (bInSkill && _refEntity.CurrentFrame > 0.23f)
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
		PlayVoiceSE("v_re2_skill01");
		PlaySkillSE("re2_sword01");
		if (_refEntity.Controller.Collisions.below)
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
		if (!_refEntity.BulletCollider.bIsEnd)
		{
			_refEntity.BulletCollider.BackToPool();
		}
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
		if (_refEntity.UseAutoAim)
		{
			TurnToAimTarget();
		}
		PlayVoiceSE("v_re2_skill02");
		if (_refEntity.Controller.Collisions.below)
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
			_tfRHandMesh.enabled = true;
			_tfWeaponMesh.enabled = false;
			_tfSwordMesh.enabled = false;
			_tfBusterMesh.enabled = false;
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfRHandMesh.enabled = true;
			_tfWeaponMesh.enabled = false;
			_tfSwordMesh.enabled = false;
			_tfBusterMesh.enabled = false;
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfRHandMesh.enabled = true;
			_tfWeaponMesh.enabled = false;
			_tfSwordMesh.enabled = false;
			_tfBusterMesh.enabled = false;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_tfSwordMesh.enabled = true;
			_tfRHandMesh.enabled = false;
			_tfBusterMesh.enabled = false;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfBusterMesh.enabled = true;
			_tfWeaponMesh.enabled = false;
			_tfSwordMesh.enabled = false;
			_tfRHandMesh.enabled = false;
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfRHandMesh.enabled = true;
			_tfWeaponMesh.enabled = false;
			_tfSwordMesh.enabled = false;
			_tfBusterMesh.enabled = false;
			break;
		}
	}
}
