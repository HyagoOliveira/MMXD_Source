using System;
using UnityEngine;

public class CH034_Controller : CharacterControlBase
{
	protected ChargeShootObj _refChargeShootObj;

	protected SkinnedMeshRenderer _tfLGunMesh;

	protected SkinnedMeshRenderer _tfLSaberMainMesh;

	protected SkinnedMeshRenderer _tfLSaberSubMesh;

	protected bool bInSkill;

	protected bool bFxFlag;

	protected bool bMaxCharge;

	protected int nLastSkillIndex0;

	protected ParticleSystem fxResident;

	protected Transform _tfFxLogoutPoint;

	private string[] fxNames = new string[5] { "fxuse_ch034_005", "fxuse_ch034_006", "fxuse_ch034_007", "fxuse_ch034_010", "fxuse_ch034_startin_001" };

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch034_skill_01_step2_stand", "ch034_skill_01_step2_jump", "ch034_skill_01_step2_crouch", "ch034_skill_02_step1_stand", "ch034_skill_02_step1_jump", "ch034_skill_02_step2_stand", "ch034_skill_02_step2_jump" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch034_skill_01_step1_stand_up", "ch034_skill_01_step1_stand_mid", "ch034_skill_01_step1_stand_down" };
		string[] array2 = new string[3] { "ch034_skill_01_step1_jump_up", "ch034_skill_01_step1_jump_mid", "ch034_skill_01_step1_jump_down" };
		string[] array3 = new string[3] { "ch034_skill_01_step1_crouch_up", "ch034_skill_01_step1_crouch_mid", "ch034_skill_01_step1_crouch_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		_refChargeShootObj.ChargeStartParticleSystem[1].transform.SetParent(_refEntity.transform.root, false);
		_refChargeShootObj.ChargeLv1ParticleSystem[1].transform.SetParent(_refEntity.transform.root, false);
		_refChargeShootObj.ChargeLv2ParticleSystem[1].transform.SetParent(_refEntity.transform.root, false);
		_refChargeShootObj.ChargeLv3ParticleSystem[1].transform.SetParent(_refEntity.transform.root, false);
		string[] array = fxNames;
		foreach (string p_fxName in array)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(p_fxName, 2);
		}
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[4];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill0_ShootPoint", true);
		_refEntity.ExtraTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "SlashPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "GunMesh_c");
		_tfLGunMesh = transform.GetComponent<SkinnedMeshRenderer>();
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMainMesh_m");
		_tfLSaberMainMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "SaberSubMesh_g");
		_tfLSaberSubMesh = transform3.GetComponent<SkinnedMeshRenderer>();
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "CH034_000_FX", true);
		fxResident = transform4.GetComponent<ParticleSystem>();
		if (null != fxResident)
		{
			fxResident.Stop(true);
		}
		_tfFxLogoutPoint = OrangeBattleUtility.FindChildRecursive(ref target, "FxLogoutPoint", true);
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
		_refEntity.PlayerSkillLandCB = PlayerSkillLand;
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.SkillEnd = true;
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			_refEntity.SkillEnd = true;
		}
		_refEntity.CurrentActiveSkill = -1;
		ToggleLeftWeapon(0);
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged())
		{
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
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 0.1f && bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0))
			{
				bool isCrouch = _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2;
				CancelSkill0(isCrouch);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_4:
		case OrangeCharacter.SubStatus.SKILL0_5:
			if (_refEntity.CurrentFrame > 0.1f && bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0))
			{
				bool isCrouch2 = _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5;
				CancelSkill0(isCrouch2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_6:
		case OrangeCharacter.SubStatus.SKILL0_7:
		case OrangeCharacter.SubStatus.SKILL0_8:
			if (_refEntity.CurrentFrame > 0.25f && bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0))
			{
				OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
				int num = 27;
				CancelSkill0(false);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (bInSkill && _refEntity.CurrentFrame > 0.06f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (bFxFlag && _refEntity.CurrentFrame > 0.1f)
			{
				bFxFlag = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch034_006", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			}
			if (_refEntity.CurrentFrame > 0.18f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (bFxFlag && _refEntity.CurrentFrame > 0.29f)
			{
				bFxFlag = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch034_007", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			}
			else if (_refEntity.CurrentFrame > 0.5f && CheckCancelAnimate(1))
			{
				CancelSkill1();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (bInSkill && _refEntity.CurrentFrame > 0.05f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (_refEntity.CurrentFrame > 0.3f && CheckCancelAnimate(1))
			{
				CancelSkill1();
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[0].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[0].Reload_index == 0)
				{
					ChargeStart(id);
				}
				else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
				{
					ShootSkill0();
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ShootSkill0();
			}
			break;
		case 1:
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[1].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[1].Reload_index == 0)
				{
					ChargeStart(id);
				}
				else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
				{
					ShootSkill1();
				}
			}
			break;
		}
	}

	protected void PlayerHeldSkill(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && _refEntity.PlayerSetting.AutoCharge == 0 && !_refEntity.PlayerSkills[id].ForceLock && !_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[id].FastBulletDatas[0].n_CHARGE_MAX_LEVEL != 0 && (id != 0 || _refEntity.PlayerSkills[id].Reload_index <= 0) && _refEntity.CheckUseSkillKeyTriggerEX(id))
		{
			_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
			_refChargeShootObj.StartCharge(id);
		}
	}

	public void PlayerReleaseSkill(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.HURT)
		{
			return;
		}
		if (_refEntity.PlayerSkills[id].LastUseTimer.GetMillisecond() < _refEntity.PlayerSkills[id].BulletData.n_FIRE_SPEED || _refEntity.PlayerSkills[id].MagazineRemain <= 0f || _refEntity.PlayerSkills[id].ForceLock || _refEntity.CurrentActiveSkill != -1)
		{
			if (_refEntity.PlayerSetting.AutoCharge == 0)
			{
				_refChargeShootObj.StopCharge(id);
			}
		}
		else
		{
			_refEntity.PlayerReleaseSkillCharacterCallCB.CheckTargetToInvoke(id);
			_refEntity.PreBelow = _refEntity.Controller.Collisions.below;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ShootSkill0();
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ShootSkill1();
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN || mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_5:
			_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_6:
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			break;
		case OrangeCharacter.SubStatus.SKILL0_7:
			_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_8:
			_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			bFxFlag = true;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch034_005", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			bFxFlag = true;
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			bFxFlag = true;
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch034_010", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
			}
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
				ToggleLeftWeapon(0);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
			case OrangeCharacter.SubStatus.SKILL0_4:
			case OrangeCharacter.SubStatus.SKILL0_5:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_6:
			case OrangeCharacter.SubStatus.SKILL0_7:
			case OrangeCharacter.SubStatus.SKILL0_8:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
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
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[wsSkill.ChargeLevel], wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
			_refChargeShootObj.StopCharge();
			if (bMaxCharge)
			{
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.TriggerComboSkillBuff(wsSkill.FastBulletDatas[2].n_ID);
				}
				nLastSkillIndex0 = 3;
			}
			else
			{
				OrangeBattleUtility.UpdateSkillCD(wsSkill, wsSkill.FastBulletDatas[wsSkill.ChargeLevel].n_USE_COST, -1f);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_4:
		case OrangeCharacter.SubStatus.SKILL0_5:
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[nLastSkillIndex0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], null, nLastSkillIndex0);
			if (_refEntity.IsLocalPlayer)
			{
				int num = nLastSkillIndex0;
				_refEntity.RemoveComboSkillBuff(wsSkill.FastBulletDatas[num].n_ID);
				_refEntity.TriggerComboSkillBuff(wsSkill.FastBulletDatas[num].n_ID);
			}
			nLastSkillIndex0 = 4;
			break;
		case OrangeCharacter.SubStatus.SKILL0_6:
		case OrangeCharacter.SubStatus.SKILL0_7:
		case OrangeCharacter.SubStatus.SKILL0_8:
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[nLastSkillIndex0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], null, nLastSkillIndex0);
			OrangeBattleUtility.UpdateSkillCD(wsSkill, wsSkill.FastBulletDatas[nLastSkillIndex0].n_USE_COST, -1f);
			_refEntity.RemoveComboSkillBuff(wsSkill.FastBulletDatas[nLastSkillIndex0].n_ID);
			nLastSkillIndex0 = 0;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ModelTransform);
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[1], wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ExtraTransforms[3]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill, wsSkill.FastBulletDatas[1].n_USE_COST, -1f);
			_refChargeShootObj.StopCharge(1);
			break;
		}
	}

	public override void ControlCharacterDead()
	{
		nLastSkillIndex0 = 0;
	}

	public void TeleportOutCharacterDepend()
	{
		if (fxResident != null && _tfFxLogoutPoint != null)
		{
			fxResident.transform.parent = _tfFxLogoutPoint;
			fxResident.transform.localPosition = Vector3.zero;
		}
	}

	public void StageTeleportOutCharacterDepend()
	{
		if (fxResident != null && _tfFxLogoutPoint != null)
		{
			fxResident.gameObject.SetActive(false);
		}
	}

	public void StageTeleportInCharacterDepend()
	{
		if (fxResident != null && _tfFxLogoutPoint != null)
		{
			fxResident.gameObject.SetActive(true);
			fxResident.Play(true);
		}
	}

	public void TeleportInCharacterDepend()
	{
		ToggleLeftWeapon(-1);
		if (fxResident != null && !fxResident.isPlaying)
		{
			fxResident.Play(true);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch034_startin_001", _refEntity._transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		}
	}

	public void TeleportInExtraEffect()
	{
	}

	public void PlayerSkillLand()
	{
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SetHorizontalSpeed(0);
			if (bInSkill)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				break;
			}
			_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			_refEntity.Animator.PlayAnimation(HumanBase.AnimateId.ANI_BTSKILL_START, _refEntity.CurrentFrame);
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			_refEntity.SetHorizontalSpeed(0);
			if (bInSkill)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
				break;
			}
			_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			_refEntity.Animator.PlayAnimation(HumanBase.AnimateId.ANI_BTSKILL_START, _refEntity.CurrentFrame);
			break;
		case OrangeCharacter.SubStatus.SKILL0_7:
			_refEntity.SetHorizontalSpeed(0);
			if (bInSkill)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_6);
				break;
			}
			_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			_refEntity.Animator.PlayAnimation(HumanBase.AnimateId.ANI_SKILL_START, _refEntity.CurrentFrame);
			break;
		}
	}

	private void ChargeStart(int id)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.HURT:
			return;
		case OrangeCharacter.MainStatus.SKILL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 19) <= 5u)
			{
				return;
			}
			break;
		}
		}
		if (!_refEntity.PlayerSkills[id].ForceLock && _refEntity.PlayerSkills[id].Reload_index == 0 && !_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[id].FastBulletDatas[0].n_CHARGE_MAX_LEVEL != 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
			_refChargeShootObj.StartCharge(id);
		}
	}

	private void ShootSkill0()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		OrangeCharacter.SubStatus subStatus = OrangeCharacter.SubStatus.SKILL0;
		_refEntity.CurrentActiveSkill = 0;
		_refEntity.SkillEnd = false;
		bInSkill = true;
		if (weaponStruct.ChargeLevel == 2)
		{
			bMaxCharge = true;
			ToggleLeftWeapon(1);
			_refEntity.IsShoot = (sbyte)(weaponStruct.ChargeLevel + 1);
			subStatus = OrangeCharacter.SubStatus.SKILL0;
		}
		else if (weaponStruct.Reload_index == 3 || nLastSkillIndex0 == 3)
		{
			bMaxCharge = true;
			ToggleLeftWeapon(1);
			_refEntity.IsShoot = 3;
			subStatus = OrangeCharacter.SubStatus.SKILL0_3;
		}
		else if (weaponStruct.Reload_index == 4)
		{
			bMaxCharge = true;
			ToggleLeftWeapon(2);
			_refEntity.IsShoot = 3;
			subStatus = OrangeCharacter.SubStatus.SKILL0_6;
		}
		else
		{
			bMaxCharge = false;
			ToggleLeftWeapon(1);
			_refEntity.IsShoot = (sbyte)(weaponStruct.ChargeLevel + 1);
			nLastSkillIndex0 = 0;
			subStatus = OrangeCharacter.SubStatus.SKILL0;
		}
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, subStatus + 2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, subStatus + 0);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, subStatus + 1);
		}
	}

	private void ShootSkill1()
	{
		WeaponStruct obj = _refEntity.PlayerSkills[1];
		_refEntity.CurrentActiveSkill = 1;
		_refEntity.SkillEnd = false;
		bInSkill = true;
		if (obj.ChargeLevel == 1)
		{
			ToggleLeftWeapon(2);
			_refEntity.IsShoot = 1;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
		}
		else
		{
			ToggleLeftWeapon(2);
			_refChargeShootObj.StopCharge(1);
			_refEntity.IsShoot = 2;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	private void ToggleLeftWeapon(int status)
	{
		switch (status)
		{
		case -1:
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			_tfLGunMesh.enabled = false;
			_tfLSaberMainMesh.enabled = false;
			_tfLSaberSubMesh.enabled = false;
			break;
		case 0:
			_refEntity.EnableCurrentWeapon();
			_tfLGunMesh.enabled = false;
			_tfLSaberMainMesh.enabled = false;
			_tfLSaberSubMesh.enabled = false;
			break;
		case 1:
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			_tfLGunMesh.enabled = true;
			_tfLSaberMainMesh.enabled = false;
			_tfLSaberSubMesh.enabled = false;
			break;
		case 2:
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			_tfLGunMesh.enabled = false;
			_tfLSaberMainMesh.enabled = true;
			_tfLSaberSubMesh.enabled = true;
			break;
		}
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		if (skilliD == 0)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
			{
				return true;
			}
		}
		else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
		{
			return true;
		}
		return false;
	}

	private void CancelSkill0(bool isCrouch)
	{
		SkillEndChnageToIdle(isCrouch, true);
	}

	private void CancelSkill1()
	{
		SkillEndChnageToIdle();
	}

	private void SkillEndChnageToIdle(bool isCrouch = false, bool keepDashing = false)
	{
		_refEntity.SkillEnd = true;
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.Dashing = false;
		}
		else
		{
			_refEntity.Dashing = keepDashing && _refEntity.Dashing;
		}
		_refEntity.IgnoreGravity = false;
		bInSkill = false;
		ToggleLeftWeapon(0);
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
}
