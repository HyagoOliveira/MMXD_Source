using System;
using System.Collections;
using UnityEngine;

public class CH128_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private ChargeShootObj _refChargeShootObj;

	protected int _enhanceSlot0;

	protected int _enhanceSlot1;

	private SKILL_TABLE linkSkl1;

	private CharacterMaterial weaponBuster;

	private CharacterMaterial weaponSword;

	private SkinnedMeshRenderer handMesh_L_c;

	private SkinnedMeshRenderer spMeshRenderer;

	private ParticleSystem _wingEffect;

	private Vector3 shootDirCache = Vector3.right;

	private readonly string FX_1_00 = "fxuse_LifeSrd_000";

	private readonly string FX_1_01 = "fxuse_LifeSrd_001";

	private readonly int SKL0_TRIGGER = 1;

	private readonly int SKL0_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER_FX = (int)(0.16f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER_1ST = (int)(0.27f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_TRIGGER_2ND = (int)(0.55f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_BREAK = (int)(0.75f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		_refEntity.PlayTeleportInVoice = false;
	}

	private void InitializeSkill()
	{
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 0, new int[4] { 22701, 22701, 22701, 22704 }, ref _enhanceSlot0);
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 1, new int[4] { 22731, 22731, 22731, 22733 }, ref _enhanceSlot1);
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BasicBullet>(_refEntity, 1, out linkSkl1);
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_m", true);
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_c", true);
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "Wing_Mwsh_g", true);
		weaponBuster = transform2.GetComponent<CharacterMaterial>();
		weaponSword = transform3.GetComponent<CharacterMaterial>();
		handMesh_L_c = transform4.GetComponent<SkinnedMeshRenderer>();
		spMeshRenderer = transform5.GetComponent<SkinnedMeshRenderer>();
		ToggleSkillWeapon(-1, false);
		_wingEffect = OrangeBattleUtility.FindChildRecursive(ref target, "CH062_WingEffect").GetComponent<ParticleSystem>();
		_wingEffect.Play(true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_01, 2);
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		_refChargeShootObj.ShootChargeVoiceSE = "v_re_skill03_1";
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_RMEXE", "re_charge03_lp", "re_charge03_stop" };
			_refChargeShootObj.ChargeLV3SE = "re_chargemax03";
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_re_charge03_lp", "bt_re_charge03_stop" };
			_refChargeShootObj.ChargeLV3SE = "bt_re_chargemax03";
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
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
		{
			int enhanceSlot = _enhanceSlot0;
			if (((uint)enhanceSlot > 2u && enhanceSlot == 3) || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted())
				{
					_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
					_refChargeShootObj.StartCharge();
				}
				else
				{
					ShootChargeBuster(true);
				}
			}
			else
			{
				ShootChargeBuster(false);
			}
			break;
		}
		case 1:
		{
			int enhanceSlot = _enhanceSlot1;
			if (((uint)enhanceSlot <= 2u || enhanceSlot != 3) && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER_FX, SKL1_TRIGGER_1ST, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				ToggleSkillWeapon(1, true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			break;
		}
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			switch (_enhanceSlot0)
			{
			case 0:
			case 1:
			case 2:
				if (_refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
				{
					ShootChargeBuster(true);
				}
				break;
			case 3:
				if (_refEntity.CheckUseSkillKeyTrigger(id))
				{
					_refEntity.CurrentActiveSkill = id;
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
					_refEntity.IsShoot = 1;
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, (HumanBase.AnimateId)128u, (HumanBase.AnimateId)129u);
					_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
					ToggleSkillWeapon(0, true);
				}
				break;
			}
			break;
		case 1:
		{
			int enhanceSlot = _enhanceSlot1;
			if (enhanceSlot == 3 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
				_refEntity.IsShoot = 1;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER_FX, SKL1_TRIGGER_1ST, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				ToggleSkillWeapon(1, true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			break;
		}
		}
	}

	private void ShootChargeBuster(bool chkChargeLV)
	{
		_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
		ToggleSkillWeapon(0, true);
		if (chkChargeLV)
		{
			if (_refEntity.PlayerSkills[0].ChargeLevel <= 0)
			{
				_refChargeShootObj.StopCharge();
				PlayVoiceSE("v_re_skill03_1");
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[0], true, 0, 0, null, false);
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			}
			else
			{
				_refChargeShootObj.ShootChargeBuster(0, false, false);
			}
		}
		else
		{
			PlayVoiceSE("v_re_skill03_1");
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[0], true, 0, 0, null, false);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
		}
		_refEntity.Animator.SetAnimatorEquip(1);
	}

	public override void CheckSkill()
	{
		if (_enhanceSlot0 <= 2 && _refEntity.CurrentActiveSkill == 0 && _refEntity.CheckSkillEndByShootTimer())
		{
			ToggleSkillWeapon(0, false);
		}
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		nowFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.PlayerSkills[0].ShootTransform[0], MagazineType.ENERGY, -1, 1);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				isSkillEventEnd = false;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.NORMAL, -1, 0, false);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_TRIGGER_2ND - SKL1_TRIGGER_1ST, SKL1_END - SKL1_TRIGGER_1ST, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_01, _refEntity.ModelTransform.position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				if (linkSkl1 != null)
				{
					PushLinkSkl(linkSkl1, _refEntity.ModelTransform, false, null);
				}
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nowFrame >= endFrame)
			{
				isSkillEventEnd = false;
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				shootDirCache = _refEntity.ShootDirection;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.NORMAL, -1, 1, false);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL1_TRIGGER_2ND - SKL1_TRIGGER_1ST, SKL1_END - SKL1_TRIGGER_1ST, OrangeCharacter.SubStatus.SKILL1_3, out skillEventFrame, out endFrame);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_01, _refEntity.PlayerSkills[1].ShootTransform[0].position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				if (linkSkl1 != null)
				{
					PushLinkSkl(linkSkl1, _refEntity.ModelTransform, false, shootDirCache);
				}
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		}
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		ToggleSkillWeapon(-1, false);
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_SKILL_START && animateID != HumanBase.AnimateId.ANI_BTSKILL_START)
		{
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
		}
		else
		{
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
	}

	private void PushLinkSkl(SKILL_TABLE bulletData, Transform shootTransform, bool triggerPassiveSkl, Vector3? shootDir)
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		_refEntity.PushBulletDetail(bulletData, currentSkillObj.weaponStatus, shootTransform, currentSkillObj.SkillLV, shootDir);
		if (triggerPassiveSkl)
		{
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, bulletData, currentSkillObj.weaponStatus, shootTransform);
		}
	}

	private void ToggleSkillWeapon(int currentSkl, bool enable)
	{
		switch (currentSkl)
		{
		case 0:
			if (enable)
			{
				weaponBuster.Appear(null, 0f);
				handMesh_L_c.enabled = false;
			}
			else
			{
				weaponBuster.Disappear(null, 0f);
				handMesh_L_c.enabled = true;
			}
			return;
		case 1:
			if (enable)
			{
				weaponSword.Appear(null, 0f);
			}
			else
			{
				weaponSword.Disappear(null, 0f);
			}
			return;
		}
		if (enable)
		{
			weaponBuster.Appear(null, 0f);
			weaponSword.Appear(null, 0f);
			handMesh_L_c.enabled = false;
		}
		else
		{
			weaponBuster.Disappear(null, 0f);
			weaponSword.Disappear(null, 0f);
			handMesh_L_c.enabled = true;
		}
	}

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		ToggleSkillWeapon(-1, false);
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		_refEntity.EnableCurrentWeapon();
		ToggleSkillWeapon(-1, false);
	}

	public override void ControlCharacterDead()
	{
		ToggleWing(false);
		ToggleSkillWeapon(-1, false);
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	private void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				ToggleWing(false);
			}
		}
	}

	protected void StageTeleportInCharacterDepend()
	{
		if (spMeshRenderer != null && spMeshRenderer.enabled)
		{
			StopAllCoroutines();
			return;
		}
		ToggleWing(false);
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	protected void StageTeleportOutCharacterDepend()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT)
		{
			StartCoroutine(OnToggleWing(false, 0.2f));
		}
		else if (!_refEntity.Animator.IsDefaultAnimator)
		{
			StartCoroutine(OnToggleWing(false, 0.2f));
		}
	}

	private IEnumerator OnToggleWing(bool isActive, float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleWing(isActive);
	}

	private void ToggleWing(bool isActive)
	{
		spMeshRenderer.enabled = isActive;
		if (isActive)
		{
			_wingEffect.Play(true);
		}
		else
		{
			_wingEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch128_skill_02_crouch", "ch128_skill_02_stand", "ch128_skill_02_jump" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch128_skill_01_crouch_up", "ch128_skill_01_crouch_mid", "ch128_skill_01_crouch_down" };
		string[] array2 = new string[3] { "ch128_skill_01_stand_up", "ch128_skill_01_stand_mid", "ch128_skill_01_stand_down" };
		string[] array3 = new string[3] { "ch128_skill_01_jump_up", "ch128_skill_01_jump_mid", "ch128_skill_01_jump_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[2] { "buster_stand_charge_atk", "buster_crouch_charge_atk" };
		target = new string[2] { "ch128_skill_01_stand_mid", "ch128_skill_01_crouch_mid" };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}
}
