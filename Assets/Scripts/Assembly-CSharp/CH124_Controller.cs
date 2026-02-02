using System;
using CallbackDefs;
using UnityEngine;

public class CH124_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	protected int _enhanceSlot0;

	protected int _enhanceSlot1;

	private string fxSlash = string.Empty;

	private string fxSlashJump = string.Empty;

	private Transform slashPoint;

	private Transform slashFxPoint;

	private ChargeShootObj _refChargeShootObj;

	private bool inChargeShoot;

	private SKILL_TABLE raisingSkl;

	private SKILL_TABLE linkSkl;

	[SerializeField]
	private int risingSpdX = 4000;

	[SerializeField]
	private int risingSpdY = 8000;

	[SerializeField]
	private float risingTime = 0.3f;

	private CharacterMaterial weaponGun;

	private CharacterMaterial weaponSword;

	private CH124_LoginController loginController;

	private bool isTeleportInStart;

	private bool isTeleportInChange;

	private Vector3 gunRotate = new Vector3(0f, 0f, 90f);

	private readonly string SpWeaponMesh = "CH124_WeaponGun";

	private readonly string SpWeaponMesh2 = "CH124_WeaponSaber";

	private readonly string GunShootPoint = "GunShootPoint";

	private readonly string SlashPoint = "SlashPoint";

	private readonly string SlashFxPoint = "SlashFxPoint";

	private readonly string FX_1_00 = "fxuse_zxasaber_000";

	private readonly string FX_1_01 = "fxuse_zxasaber_001";

	private readonly string FX_1_02 = "fxuse_zxasaber_002";

	private readonly string FX_1_10 = "fxuse_zxasaberjump_000";

	private readonly string FX_1_11 = "fxuse_zxasaberjump_001";

	private readonly string FX_1_12 = "fxuse_zxasaberjump_002";

	private readonly string FX_0_03 = "fxuse_zxabuster_003";

	private readonly string FX_TELEPORT = "fxdemo_zxa_005";

	private readonly int SKL0_TRIGGER = 1;

	private readonly int SKL0_END = (int)(0.233f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.233f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_SLASH_01 = (int)(0.125f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_SLASH_02 = (int)(0.416f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_SLASH_03 = (int)(0.724f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_FX = 3;

	private int SKL1_TRIGGER_FX_NOW;

	public override void Start()
	{
		base.Start();
		loginController = _refEntity.ModelTransform.GetComponent<CH124_LoginController>();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.OverrideAnimatorParamtersEvt = OverrideAnimatorParamters;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
	}

	public void TeleportInCharacterDepend()
	{
		if (!isTeleportInStart)
		{
			isTeleportInStart = true;
			if ((bool)loginController)
			{
				loginController.Play(HumanBase.AnimateId.ANI_TELEPORT_IN_POSE);
				loginController.UpdateMask(0, 3);
				_refEntity.CharacterMaterials.Disappear(null, 0f);
			}
		}
		if (_refEntity.CurrentFrame >= 0.25f && !isTeleportInChange)
		{
			isTeleportInChange = true;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_TELEPORT, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			if ((bool)loginController)
			{
				loginController.Disappear();
				_refEntity.CharacterMaterials.Appear(null, 0f);
			}
		}
	}

	protected void InitializeSkill()
	{
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 0, new int[4] { 22001, 22001, 22005, 22006 }, ref _enhanceSlot0);
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 1, new int[4] { 22031, 22031, 22034, 22037 }, ref _enhanceSlot1);
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		if (weaponStruct != null && weaponStruct.BulletData.n_LINK_SKILL != 0 && raisingSkl == null && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out raisingSkl))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref raisingSkl);
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(raisingSkl.n_LINK_SKILL, out linkSkl))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl);
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<BasicBullet>("prefab/bullet/" + linkSkl.s_MODEL, linkSkl.s_MODEL, 2, null);
			}
		}
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, GunShootPoint, true);
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[2];
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, SpWeaponMesh, true);
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, SpWeaponMesh2, true);
		weaponGun = transform.GetComponent<CharacterMaterial>();
		weaponSword = transform2.GetComponent<CharacterMaterial>();
		ToggleSkillWeapon(-1, false);
		slashPoint = OrangeBattleUtility.FindChildRecursive(ref target, SlashPoint, true);
		if (null == slashPoint)
		{
			slashPoint = _refEntity._transform;
		}
		slashFxPoint = OrangeBattleUtility.FindChildRecursive(ref target, SlashFxPoint, true);
		if (null == slashFxPoint)
		{
			slashFxPoint = _refEntity._transform;
		}
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<SplashBullet>("prefab/bullet/splashbullet", "SplashBullet", 3, null);
		switch (_enhanceSlot1)
		{
		default:
			fxSlash = FX_1_00;
			fxSlashJump = FX_1_10;
			break;
		case 2:
			fxSlash = FX_1_01;
			fxSlashJump = FX_1_11;
			break;
		case 3:
			fxSlash = FX_1_02;
			fxSlashJump = FX_1_12;
			break;
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_03, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_TELEPORT, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxSlash, 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxSlashJump, 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_slash_000", 3);
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		_refChargeShootObj.ShootChargeVoiceSE = "v_ai2_skill01";
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_AILE2", "ai2_charge01_lp", "ai2_charge01_stop" };
			_refChargeShootObj.ChargeLV3SE = "ai2_charge02";
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_ai2_charge01_lp", "bt_ai2_charge01_stop" };
			_refChargeShootObj.ChargeLV3SE = "bt_ai2_charge02";
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
		{
			int enhanceSlot = _enhanceSlot0;
			if (((uint)enhanceSlot > 1u && (uint)(enhanceSlot - 2) <= 1u) || !_refEntity.CheckUseSkillKeyTrigger(id))
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
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlaySkillSE("ai2_saber01");
				_refEntity.CurrentActiveSkill = id;
				SKL1_TRIGGER_FX_NOW = GameLogicUpdateManager.GameFrame + SKL1_TRIGGER_FX;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER_SLASH_01, SKL1_TRIGGER_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				_refEntity.IsShoot = 0;
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)71u);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				ToggleSkillWeapon(1, true);
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(1, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[1]);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			return;
		}
		switch (_enhanceSlot0)
		{
		case 0:
		case 1:
			if (_refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ShootChargeBuster(true);
			}
			break;
		case 2:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ai2_skill02");
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				_refEntity.IsShoot = 1;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)132u, HumanBase.AnimateId.ANI_BTSKILL_START, (HumanBase.AnimateId)130u);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				ToggleSkillWeapon(0, true);
			}
			break;
		case 3:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ai2_skill02");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_03, _refEntity.AimTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				_refEntity.IsShoot = 1;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)132u, HumanBase.AnimateId.ANI_BTSKILL_START, (HumanBase.AnimateId)130u);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				ToggleSkillWeapon(0, true);
			}
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_enhanceSlot0 <= 1)
		{
			if (_refEntity.CurrentActiveSkill != 0)
			{
				inChargeShoot = false;
			}
			else if (_refEntity.CheckSkillEndByShootTimer())
			{
				inChargeShoot = false;
				ToggleSkillWeapon(0, false);
			}
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
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				ToggleSkillWeapon(0, false);
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 1);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				ToggleSkillWeapon(0, false);
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 1);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame == SKL1_TRIGGER_FX_NOW)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fxSlash, _refEntity.AimTransform.position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				_refEntity.IgnoreGravity = true;
			}
			if (nowFrame > skillEventFrame)
			{
				CreateSplashBullet(1);
				int num3 = SKL1_TRIGGER_SLASH_02 - SKL1_TRIGGER_SLASH_01;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, num3, num3, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nowFrame > skillEventFrame)
			{
				CreateSplashBullet(1);
				int num = SKL1_TRIGGER_SLASH_03 - SKL1_TRIGGER_SLASH_02 - SKL1_TRIGGER_SLASH_01;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, num, num, OrangeCharacter.SubStatus.SKILL1_3, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (nowFrame > skillEventFrame)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fxSlashJump, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				CreateSplashBullet(1);
				skillEventFrame = endFrame;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			if (nowFrame >= endFrame)
			{
				isSkillEventEnd = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				int num2 = (int)(risingTime / GameLogicUpdateManager.m_fFrameLen);
				int enhanceSlot = _enhanceSlot1;
				int p_sklTriggerFrame = (((uint)enhanceSlot > 2u && enhanceSlot == 3) ? 1 : Mathf.Clamp(num2 / 2, 1, endFrame));
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, p_sklTriggerFrame, num2, OrangeCharacter.SubStatus.SKILL1_5, out skillEventFrame, out endFrame);
				_refEntity.IgnoreGravity = true;
				_refEntity.SetSpeed((int)_refEntity._characterDirection * risingSpdX, risingSpdY);
				CreateColliderBullet();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_5:
			if (nowFrame >= endFrame)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.BulletCollider.BackToPool();
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, 1, 1, OrangeCharacter.SubStatus.SKILL1_6, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)73u);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				Vector3 value = ((_refEntity._characterDirection == CharacterDirection.LEFT) ? new Vector3(-1f, 0f, 0f) : new Vector3(1f, 0f, 0f));
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				isSkillEventEnd = true;
				if (linkSkl != null)
				{
					_refEntity.PushBulletDetail(linkSkl, currentSkillObj.weaponStatus, slashPoint, currentSkillObj.SkillLV, value);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_6:
			if (nowFrame >= endFrame)
			{
				ToggleSkillWeapon(1, false);
				OnSkillEnd();
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
		_refEntity.BulletCollider.BackToPool();
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
		{
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			HumanBase.AnimateId animateID = _refEntity.AnimateID;
			if (animateID != (HumanBase.AnimateId)132u)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	public override void ClearSkill()
	{
		ToggleSkillWeapon(-1, false);
		_refEntity.CancelBusterChargeAtk();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			ToggleSkillWeapon(-1, false);
			_refEntity.EnableCurrentWeapon();
		}
	}

	private void ToggleSkillWeapon(int currentSkl, bool enable)
	{
		switch (currentSkl)
		{
		case 0:
			if (enable)
			{
				weaponGun.Appear(null, 0f);
				ToggleGunRotate(true);
			}
			else
			{
				weaponGun.Disappear(null, 0f);
				ToggleGunRotate(true);
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
			weaponGun.Appear(null, 0f);
			weaponSword.Appear(null, 0f);
		}
		else
		{
			ToggleGunRotate(true);
			weaponGun.Disappear(null, 0f);
			weaponSword.Disappear(null, 0f);
		}
	}

	private void ToggleGunRotate(bool setIdentity)
	{
		if (setIdentity)
		{
			weaponGun.transform.localRotation = Quaternion.identity;
		}
		else
		{
			weaponGun.transform.localRotation = Quaternion.Euler(gunRotate);
		}
	}

	private void ShootChargeBuster(bool chkChargeLV)
	{
		inChargeShoot = true;
		_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
		ToggleSkillWeapon(0, true);
		if (chkChargeLV)
		{
			if (_refEntity.PlayerSkills[0].ChargeLevel <= 0)
			{
				_refChargeShootObj.StopCharge();
				PlayVoiceSE("v_ai2_skill01");
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[0], true, 0, 0);
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			}
			else
			{
				_refChargeShootObj.ShootChargeBuster(0);
			}
		}
		else
		{
			PlayVoiceSE("v_ai2_skill01");
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[0], true, 0, 0);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
		}
		_refEntity.Animator.SetAnimatorEquip(1);
		OverrideAnimatorParamters();
	}

	private void CreateColliderBullet()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
		_refEntity.FreshBullet = true;
		_refEntity.BulletCollider.UpdateBulletData(raisingSkl, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
		_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
	}

    [Obsolete]
    private void CreateSplashBullet(int skillIdx)
	{
		SplashBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SplashBullet>("SplashBullet");
		poolObj.HitCallback = (CallbackObj)Delegate.Combine(poolObj.HitCallback, new CallbackObj(SplashHitCB));
		poolObj.UpdateBulletData(_refEntity.PlayerSkills[skillIdx].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
		poolObj.SetBulletAtk(_refEntity.PlayerSkills[skillIdx].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		poolObj.SetOffset(new Vector2(1.5f * (float)_refEntity.direction, 0f));
		poolObj.BulletLevel = _refEntity.PlayerSkills[skillIdx].SkillLV;
		poolObj.transform.position = slashPoint.position;
		poolObj.Active(_refEntity.TargetMask, false);
	}

	private void SplashHitCB(object t)
	{
		Transform transform = t as Transform;
		if (transform != null && (transform.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer || transform.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer))
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_slash_000", transform.position, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0, 90)), Array.Empty<object>());
		}
	}

	public void OverrideAnimatorParamters()
	{
		if (inChargeShoot)
		{
			ToggleGunRotate(true);
			short animateUpperID = _refEntity.AnimationParams.AnimateUpperID;
			switch ((HumanBase.AnimateId)_refEntity.AnimationParams.AnimateUpperID)
			{
			case HumanBase.AnimateId.ANI_STAND:
			case HumanBase.AnimateId.ANI_STAND_SKILL:
				animateUpperID = 127;
				break;
			case HumanBase.AnimateId.ANI_JUMP:
				animateUpperID = 131;
				break;
			case HumanBase.AnimateId.ANI_FALL:
				animateUpperID = 131;
				break;
			case HumanBase.AnimateId.ANI_CROUCH_END:
				animateUpperID = 132;
				break;
			case HumanBase.AnimateId.ANI_DASH:
				animateUpperID = 129;
				break;
			default:
				ToggleGunRotate(false);
				break;
			}
			_refEntity.AnimationParams.AnimateUpperID = animateUpperID;
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[9] { "ch124_skill_02_crouch_start", "ch124_skill_02_crouch_loop", "ch124_skill_02_crouch_end", "ch124_skill_02_stand_start", "ch124_skill_02_stand_loop", "ch124_skill_02_stand_end", "ch124_skill_02_jump_start", "ch124_skill_02_jump_loop", "ch124_skill_02_jump_end" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch124_skill_01_stand_up", "ch124_skill_01_stand_mid", "ch124_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch124_skill_01_run_up", "ch124_skill_01_run_mid", "ch124_skill_01_run_down" };
		string[] array3 = new string[3] { "ch124_skill_01_dash_up_loop", "ch124_skill_01_dash_mid_loop", "ch124_skill_01_dash_down_loop" };
		string[] array4 = new string[3] { "ch124_skill_01_jump_up_loop", "ch124_skill_01_jump_mid_loop", "ch124_skill_01_jump_down_loop" };
		string[] array5 = new string[3] { "ch124_skill_01_fall_up_loop", "ch124_skill_01_fall_mid_loop", "ch124_skill_01_fall_down_loop" };
		string[] array6 = new string[3] { "ch124_skill_01_crouch_up", "ch124_skill_01_crouch_mid", "ch124_skill_01_crouch_down" };
		return new string[6][] { array, array2, array3, array4, array5, array6 };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}
}
