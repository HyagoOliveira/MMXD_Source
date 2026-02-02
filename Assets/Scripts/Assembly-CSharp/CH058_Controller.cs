using System;
using UnityEngine;

public class CH058_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected Vector3 vBeamShootDir = Vector3.right;

	private ShieldBullet tShieldBullet;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch058_skill_02_stand", "ch058_skill_02_jump", "ch058_skill_02_crouch" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch058_skill_01_stand_up", "ch058_skill_01_stand_mid", "ch058_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch058_skill_01_jump_up", "ch058_skill_01_jump_mid", "ch058_skill_01_jump_down" };
		string[] array3 = new string[3] { "ch058_skill_01_crouch_up", "ch058_skill_01_crouch_mid", "ch058_skill_01_crouch_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_heartbeam_000", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
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
		int n_CONDITION_ID = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		if (tShieldBullet != null)
		{
			if (tShieldBullet.bIsEnd)
			{
				tShieldBullet = null;
			}
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID) && tShieldBullet == null)
		{
			CreateShieldBullet(_refEntity.PlayerSkills[1]);
		}
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
				PlayVoiceSE("v_ir_skill01");
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ir_skill02");
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
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				_refEntity.Animator._animator.speed = 1.25f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				_refEntity.Animator._animator.speed = 1.25f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				_refEntity.Animator._animator.speed = 1.25f;
				break;
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
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (!_refEntity.UseAutoAim)
			{
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV, vBeamShootDir);
			}
			else
			{
				if (_refEntity.IAimTargetLogicUpdate != null)
				{
					Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.ExtraTransforms[0].position, _refEntity.IAimTargetLogicUpdate);
					if (vector.HasValue)
					{
						_refEntity._characterDirection = ((vector.Value.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
						_refEntity.UpdateDirection();
					}
				}
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			}
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
		{
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.transform);
			int n_CONDITION_ID = wsSkill.FastBulletDatas[0].n_CONDITION_ID;
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
			{
				CreateShieldBullet(wsSkill);
			}
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			break;
		}
		}
	}

	protected void CreateShieldBullet(WeaponStruct wsSkill)
	{
		tShieldBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ShieldBullet>(wsSkill.BulletData.s_MODEL);
		tShieldBullet.UpdateBulletData(wsSkill.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		tShieldBullet.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		tShieldBullet.BulletLevel = wsSkill.SkillLV;
		tShieldBullet.BindBuffId(wsSkill.BulletData.n_CONDITION_ID, _refEntity.IsLocalPlayer);
		tShieldBullet.Active(_refEntity.transform, Quaternion.identity, _refEntity.TargetMask, true);
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("ir_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch058_startin_000";
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		int n_CONDITION_ID = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(n_CONDITION_ID);
		}
		if (tShieldBullet != null)
		{
			tShieldBullet.BackToPool();
			tShieldBullet = null;
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
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
			if (!_refEntity.UseAutoAim || bInSkill)
			{
				_refEntity.ShootDirection = vBeamShootDir;
				_refEntity.Animator.SetAttackLayerActive(vBeamShootDir);
			}
			else if (vBeamShootDir != _refEntity.ShootDirection)
			{
				_refEntity.Animator.SetAttackLayerActive(_refEntity.ShootDirection);
			}
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.4f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.8f)
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
			else if (bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (!bInSkill && CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.5f)
			{
				SkillEndChnageToIdle();
			}
			break;
		}
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		PlaySkillSE("ir_heartbeam");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_heartbeam_000", _refEntity.transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		vBeamShootDir = _refEntity.ShootDirection;
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
		_refEntity.Animator._animator.speed = 1f;
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
