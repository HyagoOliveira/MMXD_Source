using System;
using StageLib;
using UnityEngine;
using enums;

public class BluesController : CharacterControlBase
{
	private bool bInShootBullet;

	private int shield_type;

	private Transform mShieldBackPosition;

	private Transform mShieldMesh_m;

	private Transform mShieldMeshFx_m;

	private Transform mShieldCollider;

	private ParticleSystem mfxuse_bruceskill1;

	private OrangeTimer _skillTime;

	private long shield_time;

	private bool isShield_cancel;

	private int nLaskSkillIndex0;

	private bool bInSkill;

	private bool bDashLock;

	private SkinnedMeshRenderer[] _RhandMesh;

	private bool _shieldEnable = true;

	private ChargeShootObj _refChargeShootObj;

	private float _overrideAppearTime = 0.01f;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[26]
		{
			"ch016_skill_01_shield_crouch_end", "ch016_skill_01_shield_crouch_loop", "ch016_skill_01_shield_crouch_start", "ch016_skill_01_shield_dash_end", "ch016_skill_01_shield_dash_loop", "ch016_skill_01_shield_dash_start", "ch016_skill_01_shield_fall_loop", "ch016_skill_01_shield_jump_loop", "ch016_skill_01_shield_jump_start", "ch016_skill_01_shield_jump_to_fall",
			"ch016_skill_01_shield_landing", "ch016_skill_01_shield_run_loop", "ch016_skill_01_shield_run_start", "ch016_skill_01_shield_slide_end", "ch016_skill_01_shield_slide_loop", "ch016_skill_01_shield_slide_start", "ch016_skill_01_shield_stand_loop", "ch016_skill_01_shield_wallgrab_loop", "ch016_skill_01_shield_wallgrab_start", "ch016_skill_01_shield_wallgrab_step",
			"ch016_skill_01_shield_walljump_loop", "ch016_skill_01_shield_walljump_start", "ch016_skill_01_shield_weak_loop", "ch016_skill_01_shield_atk_start", "ch016_skill_01_shield_atk_loop", "ch016_skill_01_shield_atk_end"
		};
	}

	private void SetShield(bool setEnable, bool bReset = true)
	{
		if (_shieldEnable == setEnable)
		{
			return;
		}
		_shieldEnable = setEnable;
		if (setEnable)
		{
			mShieldMesh_m.SetParent(_refEntity.ExtraTransforms[0], false);
			mShieldMesh_m.localRotation = Quaternion.Euler(0f, 0f, -90f);
			mShieldCollider.gameObject.SetActive(true);
			if (mShieldMeshFx_m != null)
			{
				mShieldMeshFx_m.gameObject.SetActive(true);
			}
			_refEntity.PlayerStopDashing();
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			if (!_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			if (_skillTime != null)
			{
				_skillTime.TimerStart();
			}
			return;
		}
		mShieldMesh_m.SetParent(mShieldBackPosition, false);
		mShieldMesh_m.localRotation = Quaternion.Euler(0f, 0f, 0f);
		mShieldCollider.gameObject.SetActive(false);
		if (mShieldMeshFx_m != null)
		{
			mShieldMeshFx_m.gameObject.SetActive(false);
		}
		if (_skillTime != null)
		{
			_skillTime.TimerStop();
		}
		bInSkill = false;
		if (bReset)
		{
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_20 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_21 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_22))
			{
				_refEntity.IgnoreGravity = false;
			}
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
		}
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_5))
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		bInShootBullet = false;
	}

	public void OverrideAnimatorParamters()
	{
		if (_shieldEnable)
		{
			short animateUpperID = _refEntity.AnimationParams.AnimateUpperID;
			switch (_refEntity.AnimateID)
			{
			case HumanBase.AnimateId.ANI_STAND:
				animateUpperID = 81;
				break;
			case HumanBase.AnimateId.ANI_STEP:
				animateUpperID = 77;
				break;
			case HumanBase.AnimateId.ANI_WALK:
				animateUpperID = 76;
				break;
			case HumanBase.AnimateId.ANI_CROUCH:
				animateUpperID = 67;
				break;
			case HumanBase.AnimateId.ANI_CROUCH_END:
				animateUpperID = 66;
				break;
			case HumanBase.AnimateId.ANI_CROUCH_UP:
				animateUpperID = 65;
				break;
			case HumanBase.AnimateId.ANI_JUMP:
				animateUpperID = 73;
				break;
			case HumanBase.AnimateId.ANI_FALL:
				animateUpperID = 74;
				break;
			case HumanBase.AnimateId.ANI_LAND:
				animateUpperID = 75;
				break;
			case HumanBase.AnimateId.ANI_DASH:
				animateUpperID = 69;
				break;
			case HumanBase.AnimateId.ANI_DASH_END:
				animateUpperID = 68;
				break;
			case HumanBase.AnimateId.ANI_WALLGRAB_BEGIN:
				animateUpperID = 86;
				break;
			case HumanBase.AnimateId.ANI_WALLGRAB:
				animateUpperID = 83;
				break;
			case HumanBase.AnimateId.ANI_WALLGRAB_END:
				animateUpperID = 84;
				break;
			case HumanBase.AnimateId.ANI_WALLKICK:
				animateUpperID = 86;
				break;
			case HumanBase.AnimateId.ANI_WALLKICK_END:
				animateUpperID = 85;
				break;
			}
			_refEntity.AnimationParams.AnimateUpperID = animateUpperID;
		}
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		mShieldBackPosition = OrangeBattleUtility.FindChildRecursive(ref target, "ShieldBackPosition", true);
		mShieldMesh_m = OrangeBattleUtility.FindChildRecursive(ref target, "ShieldMesh_m", true);
		mShieldMeshFx_m = OrangeBattleUtility.FindChildRecursive(ref target, "ShieldMeshEfx_m", true);
		mShieldCollider = OrangeBattleUtility.FindChildRecursive(ref target, "ShieldCollider", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_bruceskill1", true);
		if (transform != null)
		{
			mfxuse_bruceskill1 = transform.GetComponent<ParticleSystem>();
		}
		SetShield(false, false);
		StageObjParam stageObjParam = mShieldCollider.gameObject.AddOrGetComponent<StageObjParam>();
		stageObjParam.nSubPartID = 1;
		stageObjParam.tLinkSOB = _refEntity;
		_refEntity.GuardTransform.Add(1);
		StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			BulletBase component = ((GameObject)obj).GetComponent<BulletBase>();
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BasicBullet>(UnityEngine.Object.Instantiate(component), "p_bbuster_002", 5);
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/p_bbuster_002", "p_bbuster_002", loadCallBackObj.LoadCB);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bshield_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bshield_002", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_GuardHurt_000", 2);
		_skillTime = OrangeTimerManager.GetTimer();
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		_refChargeShootObj.ChargeMaxSE = AudioManager.FormatEnum2Name(SkillSE_BLUES.CRI_SKILLSE_BLUES_BL_CHARGEMAX.ToString());
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_BLUES", "bl_charge_lp", "bl_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_bl_charge_lp", "bt_bl_charge_stop" };
		}
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(base.transform, "HandMesh_R");
		_RhandMesh = new SkinnedMeshRenderer[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_RhandMesh[i] = array[i].GetComponent<SkinnedMeshRenderer>();
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
		_refEntity.PlayerPressSkillCB = PlayerPressSkill;
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckDashLockEvt = CheckDashLock;
		_refEntity.OverrideAnimatorParamtersEvt = OverrideAnimatorParamters;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.GuardCalculateEvt = GuardCalculate;
		_refEntity.GuardHurtEvt = GuardHurt;
		_refEntity.CheckAvalibaleForRideArmorEvt = CheckAvalibaleForRideArmor;
		_refEntity.EnterRideArmorEvt = OnEnterRideArmor;
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length != 2)
		{
			return;
		}
		int num = (int)parameters[0];
		int num2 = (int)parameters[1];
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.TELEPORT_IN || _refEntity.CurMainStatus == OrangeCharacter.MainStatus.TELEPORT_OUT || (int)_refEntity.Hp <= 0 || num != 1)
		{
			return;
		}
		if (num2 == 0)
		{
			if (_refEntity.CurrentActiveSkill != 0 && _refEntity.refRideBaseObj == null)
			{
				_refEntity.EnableCurrentWeapon();
			}
			_refEntity.PlayerStopDashing();
			_refEntity.SetSpeed(0, 0);
			SetShield(false, _refEntity.CurrentActiveSkill == 1);
		}
		else if (!_refEntity.IsStun)
		{
			_refEntity.DisableCurrentWeapon();
			SetShield(true);
			_refEntity.ClearSlashCollider();
		}
	}

	private void RestShieldSkill()
	{
		_refEntity.IgnoreGravity = false;
		if (!_refEntity.Dashing)
		{
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.WALK, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else if ((bool)_refEntity.Controller.BelowInBypassRange)
			{
				if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_7 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_8 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_9))
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
			}
			else if (_refEntity.Velocity.y > 0)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.JUMP, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
		}
		else if ((bool)_refEntity.Controller.BelowInBypassRange)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.DASH, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.AIRDASH, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
		_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[1].n_ID);
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
		bInShootBullet = false;
		_refEntity.CurrentActiveSkill = -1;
		if (mfxuse_bruceskill1 != null)
		{
			mfxuse_bruceskill1.Stop();
		}
	}

	public override void CheckSkill()
	{
		if (_shieldEnable && _skillTime.GetMillisecond() > shield_time)
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
			RestShieldSkill();
		}
		if (_refEntity.IsAnimateIDChanged() || !bInSkill)
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.PlayerSkills[0].Reload_index == 0 && _refEntity.IsShoot == 0)
		{
			ToggleRightBuster(false);
			_refEntity.SkillEnd = true;
		}
		if (_refEntity.CurrentActiveSkill == 1 && ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.SHOOT))
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			RestShieldSkill();
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL1_20:
			if (_refEntity.CurrentFrame > 0.5f && !bInShootBullet)
			{
				bInShootBullet = true;
				_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 3.5f), 0);
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[1].FastBulletDatas[nLaskSkillIndex0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0], null, nLaskSkillIndex0);
				if (_refEntity.PlayerSkills[1].BulletData.n_RELOAD > 0)
				{
					OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
				}
			}
			bDashLock = true;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_3:
		case OrangeCharacter.SubStatus.SKILL1_4:
		case OrangeCharacter.SubStatus.SKILL1_5:
		case OrangeCharacter.SubStatus.SKILL1_6:
			if (shield_type == 1)
			{
				ResetToIdle();
			}
			else if (!_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetHorizontalSpeed(0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
			}
			else
			{
				_refEntity.SetHorizontalSpeed(0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_22:
			if (_refEntity.CurrentFrame > 0.1f)
			{
				if (!_refEntity.Controller.Collisions.below)
				{
					_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[nLaskSkillIndex0].n_ID);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
					_refEntity.UpdateAimRangeByWeapon(_refEntity.GetCurrentWeaponObj());
					_refEntity.CurrentActiveSkill = -1;
					_refEntity.SkillEnd = true;
					bInSkill = false;
				}
				else
				{
					ResetToIdle();
				}
				bDashLock = false;
				_refEntity.IgnoreGravity = false;
				bInShootBullet = false;
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			ToggleRightBuster(false);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentSkillObj());
			_refEntity.EnableCurrentWeapon();
			_refEntity.CancelBusterChargeAtk();
			break;
		case 1:
			if (_refEntity.GetCurrentSkillObj().MagazineRemain > 0f)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			}
			_refEntity.EnableCurrentWeapon();
			SetShield(false);
			_refEntity.UpdateAimRangeByWeapon(_refEntity.GetCurrentWeaponObj());
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[1].n_ID);
			break;
		}
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.CurrentActiveSkill = -1;
		bInShootBullet = false;
		bDashLock = false;
		if (mfxuse_bruceskill1 != null)
		{
			mfxuse_bruceskill1.Stop();
		}
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		bDashLock = false;
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetAnimateId((HumanBase.AnimateId)81u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetAnimateId((HumanBase.AnimateId)77u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_5:
			_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_6:
			_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_7:
			_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_8:
			_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_9:
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			break;
		case OrangeCharacter.SubStatus.SKILL1_10:
			_refEntity.SetAnimateId((HumanBase.AnimateId)80u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_11:
			_refEntity.SetAnimateId((HumanBase.AnimateId)79u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_12:
			_refEntity.SetAnimateId((HumanBase.AnimateId)78u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_13:
			_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_14:
			_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_15:
			_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_16:
			_refEntity.SetAnimateId((HumanBase.AnimateId)83u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_17:
			_refEntity.SetAnimateId((HumanBase.AnimateId)82u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_18:
			_refEntity.SetAnimateId((HumanBase.AnimateId)86u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_19:
			_refEntity.SetAnimateId((HumanBase.AnimateId)85u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_20:
			_refEntity.SetAnimateId((HumanBase.AnimateId)88u);
			break;
		case OrangeCharacter.SubStatus.SKILL1_21:
			_refEntity.SetAnimateId((HumanBase.AnimateId)89u);
			if (mfxuse_bruceskill1 != null)
			{
				mfxuse_bruceskill1.Play();
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bshield_002", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			break;
		case OrangeCharacter.SubStatus.SKILL1_22:
			_refEntity.SetAnimateId((HumanBase.AnimateId)90u);
			_refEntity.SetSpeed(0, 0);
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		default:
			if (shield_time == 1)
			{
				ResetToIdle();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_20:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_21);
			break;
		case OrangeCharacter.SubStatus.SKILL1_21:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_22);
			if (mfxuse_bruceskill1 != null)
			{
				mfxuse_bruceskill1.Stop();
			}
			_refEntity.BulletCollider.BackToPool();
			break;
		case OrangeCharacter.SubStatus.SKILL1_22:
			_refEntity.EnableCurrentWeapon();
			SetShield(false);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			_refEntity.UpdateAimRangeByWeapon(_refEntity.GetCurrentWeaponObj());
			_refEntity.CurrentActiveSkill = -1;
			_refEntity.SkillEnd = true;
			bInSkill = false;
			_refEntity.IgnoreGravity = false;
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[nLaskSkillIndex0].n_ID);
			_refEntity.BulletCollider.BackToPool();
			break;
		case OrangeCharacter.SubStatus.SKILL0:
			break;
		}
	}

	public override void ExtraVariableInit()
	{
		if (mfxuse_bruceskill1 != null)
		{
			mfxuse_bruceskill1.Stop();
		}
		_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[1].n_ID);
	}

	public void PlayerPressSkill(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT)
		{
			if (_refEntity.PlayerSkills[id].LastUseTimer.GetMillisecond() < _refEntity.PlayerSkills[id].BulletData.n_FIRE_SPEED || _refEntity.PlayerSkills[id].MagazineRemain <= 0f || _refEntity.PlayerSkills[id].ForceLock || (_refEntity.CurrentActiveSkill != -1 && _refEntity.CurrentActiveSkill != 1))
			{
				isShield_cancel = false;
				return;
			}
			_refEntity.PreBelow = _refEntity.Controller.Collisions.below;
			PlayerPressSkillCharacterCall(id);
		}
	}

	public void PlayerReleaseSkill(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT)
		{
			if (_refEntity.PlayerSetting.AutoCharge == 0)
			{
				_refChargeShootObj.StopCharge(id);
			}
			if (_refEntity.PlayerSkills[id].LastUseTimer.GetMillisecond() >= _refEntity.PlayerSkills[id].BulletData.n_FIRE_SPEED && !(_refEntity.PlayerSkills[id].MagazineRemain <= 0f) && !_refEntity.PlayerSkills[id].ForceLock && (_refEntity.CurrentActiveSkill == -1 || _refEntity.CurrentActiveSkill == 1))
			{
				PlayerReleaseSkillCharacterCall(id);
				_refEntity.PreBelow = _refEntity.Controller.Collisions.below;
			}
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (bInShootBullet)
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
			isShield_cancel = false;
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted())
				{
					_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
					_refChargeShootObj.StartCharge();
					break;
				}
				if (_refEntity.CurrentActiveSkill == 1)
				{
					_refEntity.GetCurrentSkillObj().Reload_index = 1;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
					RestShieldSkill();
				}
				_refEntity.SkillEnd = false;
				bInSkill = true;
				ShootChargeBuster(id);
			}
			else
			{
				if (_refEntity.CurrentActiveSkill == 1)
				{
					_refEntity.GetCurrentSkillObj().Reload_index = 1;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
					RestShieldSkill();
				}
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.PlaySE(_refEntity.VoiceID, 7);
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, 0);
				_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
			}
			break;
		case 1:
			if (!_refEntity.CheckUseSkillKeyTrigger(id, false))
			{
				break;
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			nLaskSkillIndex0 = _refEntity.GetCurrentSkillObj().Reload_index;
			if (nLaskSkillIndex0 == 0)
			{
				_skillTime.TimerStart();
				if (_refEntity is OrangeConsoleCharacter)
				{
					(_refEntity as OrangeConsoleCharacter).ClearVirtualButtonStick(VirtualButtonId.SHOOT);
				}
				if (_refEntity.GetCurrentSkillObj().BulletData.n_EFFECT == 15)
				{
					if (_refEntity.GetCurrentSkillObj().BulletData.f_EFFECT_X == 0f)
					{
						shield_type = 0;
					}
					else
					{
						shield_type = 1;
					}
					shield_time = (long)(_refEntity.GetCurrentSkillObj().BulletData.f_EFFECT_Z * 1000f);
				}
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bshield_000", _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
				_refEntity.PlaySE(_refEntity.SkillSEID, 7);
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.PlayerSkills[0].ChargeTimer.IsStarted())
				{
					isShield_cancel = true;
				}
				_refEntity.StopShootTimer();
				_refEntity.IsShoot = 0;
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0], null, nLaskSkillIndex0);
			}
			else
			{
				_skillTime.TimerStop();
				_refEntity.SkillEnd = false;
				_refEntity.IgnoreGravity = true;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_20);
				_refEntity.PlaySE(_refEntity.VoiceID, 10);
			}
			break;
		}
	}

	protected bool CheckDashLock()
	{
		if (bDashLock)
		{
			return true;
		}
		if (_refEntity.CurrentActiveSkill != -1)
		{
			if (!_shieldEnable)
			{
				return shield_type == 0;
			}
			return false;
		}
		return false;
	}

	public void PlayerHeldSkill(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && _refEntity.PlayerSetting.AutoCharge == 0 && !_refEntity.PlayerSkills[id].ForceLock && !_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[id].FastBulletDatas[0].n_CHARGE_MAX_LEVEL != 0 && !isShield_cancel && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
		}
	}

	public bool CheckAvalibaleForRideArmor()
	{
		if (_refEntity.CheckActStatusEvt(3, -1) || _refEntity.CheckActStatusEvt(0, -1))
		{
			return true;
		}
		if (_refEntity.PlayerSkills[1].ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
		{
			_refEntity.SetSpeed(0, 0);
			return true;
		}
		return false;
	}

	private bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_refEntity.CurrentActiveSkill == 1)
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			RestShieldSkill();
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp <= 0)
		{
			return;
		}
		if (id != 0)
		{
			int num = 1;
		}
		else if (_refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id, false))
		{
			if (_refEntity.CurrentActiveSkill == 1)
			{
				_refEntity.GetCurrentSkillObj().Reload_index = 1;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				RestShieldSkill();
			}
			_refEntity.SkillEnd = false;
			bInSkill = true;
			ShootChargeBuster(id);
		}
	}

	public void GuardHurt(HurtPassParam tHurtPassParam)
	{
		if (_refEntity.GuardTransform.Contains(tHurtPassParam.nSubPartID))
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_GuardHurt_000", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			PlaySE("WeaponSE", 124);
		}
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[4] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[4] { "ch016_skill_02_stand", "ch016_skill_02_fall", "ch016_skill_02_wallgrab", "ch016_skill_02_crouch" };
	}

	public override bool GuardCalculate(HurtPassParam tHurtPassParam)
	{
		if (!_refEntity.GuardTransform.Contains(tHurtPassParam.nSubPartID))
		{
			return false;
		}
		if (((_refEntity.GuardTransform.Count > 0 && tHurtPassParam.S_Direction.x > 0f && _refEntity._characterDirection == CharacterDirection.LEFT) || (tHurtPassParam.S_Direction.x < 0f && _refEntity._characterDirection == CharacterDirection.RIGHT)) && mShieldMeshFx_m != null && !tHurtPassParam.IsThrough && !tHurtPassParam.IsSplash && mShieldMeshFx_m.gameObject.activeInHierarchy && tHurtPassParam.Skill_Type == 1 && tHurtPassParam.wpnType != WeaponType.Spray && tHurtPassParam.wpnType != WeaponType.SprayHeavy)
		{
			return true;
		}
		return false;
	}

	private void ShootChargeBuster(int id)
	{
		_refChargeShootObj.StopCharge(id);
		if (_refEntity.PlayerSkills[id].ChargeLevel > 0)
		{
			if (_refEntity.PlayerSkills[id].ChargeLevel < 3)
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 7);
			}
			else
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			bool flag = _refEntity.PlayerSkills[id].ChargeLevel >= 2;
			if (flag)
			{
				_refEntity.DisableCurrentWeapon();
				_refEntity.Animator.SetAnimatorEquip(_refEntity.PlayerSkills[id].WeaponData.n_TYPE);
				ToggleRightBuster(true);
			}
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel, null, !flag);
			_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
		}
	}

	private void ToggleRightBuster(bool enable)
	{
		if (enable)
		{
			_refEntity.PlayerSkills[0].WeaponMesh[1].Appear(null, _overrideAppearTime);
		}
		else
		{
			_refEntity.PlayerSkills[0].WeaponMesh[1].Disappear(null, _overrideAppearTime);
		}
		SkinnedMeshRenderer[] rhandMesh = _RhandMesh;
		for (int i = 0; i < rhandMesh.Length; i++)
		{
			rhandMesh[i].enabled = !enable;
		}
	}

	private void ResetToIdle()
	{
		if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
		{
			_refEntity.IgnoreGravity = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}
}
