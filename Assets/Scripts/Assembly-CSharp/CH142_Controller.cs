using System;
using System.Collections;
using UnityEngine;

public class CH142_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	[Header("億兆爆破")]
	private int _gigaCrushHoverFrame;

	private CollideBullet _gigaCrushCollider;

	protected Vector3 Skill1Directioon = Vector3.right;

	protected int _enhanceSlot1;

	private SKILL_TABLE linkSkl1_ex0_link;

	private SKILL_TABLE linkSkl1_ex2_link;

	private CharacterMaterial cmSaber;

	private SkinnedMeshRenderer tfBusterMesh;

	private SkinnedMeshRenderer tfHandMesh;

	private Transform _tfWind;

	private ParticleSystem _wingEffect;

	private Vector3 shootDirection = Vector3.right;

	private OrangeTimer NOVASTRIKETimer;

	private readonly int hashDirection = Animator.StringToHash("fDirection");

	protected ChargeShootObj _refChargeShootObj;

	private readonly string FX_001_EX1_000 = "fxuse_gigacrush_000";

	private readonly string FX_001_EX1_002 = "fxuse_spx_burst_001";

	private readonly string FX_001_EX2_000 = "fxduring_spx_novastrike_000";

	private readonly string FX_001_EX3_000 = "fxuse_spx_slash_000";

	private readonly int SKL1_EX1_TRIGGER = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_EX1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_EX1_END_BREAK = (int)(0.6f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_XDive2", "xd2_charge_lp", "xd2_charge_stop" };
			_refChargeShootObj.ChargeLV2SE = "xd2_chargelvup";
			_refChargeShootObj.ChargeLV3SE = "xd2_chargemax";
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_xd2_charge_lp", "bt_xd2_charge_stop" };
			_refChargeShootObj.ChargeLV2SE = "bt_xd2_chargelvup";
			_refChargeShootObj.ChargeLV3SE = "bt_xd2_chargemax";
		}
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		ManagedSingleton<CharacterControlHelper>.Instance.InitEnhanceSkill(_refEntity, 1, new int[4] { 24031, 24031, 24033, 24034 }, ref _enhanceSlot1);
		Transform transform = new GameObject("CustomShootPoint1").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "Saber_008_G", true).gameObject;
		if ((bool)gameObject)
		{
			cmSaber = gameObject.GetComponent<CharacterMaterial>();
		}
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		tfBusterMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		tfBusterMesh.enabled = false;
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m", true);
		tfHandMesh = transform3.GetComponent<SkinnedMeshRenderer>();
		_tfWind = OrangeBattleUtility.FindChildRecursive(ref target, "Fx_XWing", true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		switch (_enhanceSlot1)
		{
		case 0:
		case 1:
			if (_gigaCrushCollider == null)
			{
				GameObject gameObject2 = new GameObject();
				gameObject2.name = "GigaCrushCollider";
				_gigaCrushCollider = gameObject2.AddComponent<CollideBullet>();
			}
			ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BeamBullet>(_refEntity, 1, out linkSkl1_ex0_link);
			_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[1];
			break;
		case 2:
			_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ModelTransform;
			break;
		case 3:
			ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BeamBullet>(_refEntity, 1, out linkSkl1_ex2_link);
			_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[1];
			break;
		}
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		NOVASTRIKETimer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001_EX1_000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001_EX1_002, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001_EX2_000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001_EX3_000, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
		{
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[id];
			if (weaponStruct.Reload_index > 0)
			{
				PlayVoiceSE("v_xd2_skill01");
				ShootChargeBuster(0);
			}
			else if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!weaponStruct.ChargeTimer.IsStarted())
				{
					weaponStruct.ChargeTimer.TimerStart();
					_refChargeShootObj.StartCharge();
					break;
				}
				if (weaponStruct.ChargeLevel == 2)
				{
					PlayVoiceSE("v_xd2_skill01");
				}
				ShootChargeBuster(id);
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				PlayVoiceSE("v_xd2_skill01");
				_refEntity.Animator.SetAnimatorEquip(1);
				ToggleBuster(true);
				_refEntity.PlayerShootBuster(weaponStruct, true, id, 0);
				_refEntity.EnableHandMesh(false);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				_refEntity.CheckUsePassiveSkill(id, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
			}
			break;
		}
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.selfBuffManager.nMeasureNow >= _refEntity.PlayerSkills[id].BulletData.n_USE_COST && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				switch (_enhanceSlot1)
				{
				case 0:
				case 1:
					PlayVoiceSE("v_xd2_skill02");
					UpdateMeasureCD(id);
					_refEntity.CurrentActiveSkill = id;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[1]);
					_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f));
					_gigaCrushHoverFrame = 3;
					break;
				case 2:
					PlayVoiceSE("v_xd2_skill03");
					UpdateMeasureCD(id);
					skillEventFrame = GameLogicUpdateManager.GameFrame + ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_PREPARE_FRAME;
					ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Prepare(_refEntity, 1);
					UsePassiveSkill(1, false);
					break;
				case 3:
					PlayVoiceSE("v_xd2_skill04");
					PlaySkillSE("xd2_sword");
					UpdateMeasureCD(id);
					_refEntity.CurrentActiveSkill = id;
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_EX1_END_BREAK;
					ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_EX1_TRIGGER, SKL1_EX1_END, OrangeCharacter.SubStatus.SKILL1_6, out skillEventFrame, out endFrame);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)72u, (HumanBase.AnimateId)73u, (HumanBase.AnimateId)74u);
					UsePassiveSkill(1, false);
					_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
					ToggleSaber(true);
					break;
				}
			}
			break;
		}
	}

	private void UpdateMeasureCD(int p_id)
	{
		_refEntity.selfBuffManager.AddMeasure(-_refEntity.PlayerSkills[p_id].BulletData.n_USE_COST);
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
		}
		else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			if (_refEntity.PlayerSkills[id].ChargeLevel == 2)
			{
				PlayVoiceSE("v_xd2_skill01");
			}
			ShootChargeBuster(id);
		}
	}

	private void ShootChargeBuster(int skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[skillID];
		if (weaponStruct.ChargeLevel == 0 && weaponStruct.Reload_index == 0)
		{
			_refChargeShootObj.StopCharge();
			return;
		}
		_refEntity.Animator.SetAnimatorEquip(1);
		ToggleBuster(true);
		_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
		_refEntity.EnableHandMesh(false);
		switch (weaponStruct.ChargeLevel)
		{
		case 1:
			_refChargeShootObj.ShootChargeBuster(skillID);
			break;
		case 2:
		{
			_refChargeShootObj.StopCharge(skillID);
			_refChargeShootObj.ResetChargeSEFlg();
			SKILL_TABLE sKILL_TABLE2 = weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel];
			_refEntity.CurrentActiveSkill = skillID;
			PushComboSkillBullet(skillID, weaponStruct, sKILL_TABLE2);
			OrangeBattleUtility.UpdateSkillCD(weaponStruct, sKILL_TABLE2.n_USE_COST, -1f);
			if (_refEntity.IsLocalPlayer)
			{
				_refEntity.RemoveComboSkillBuff(sKILL_TABLE2.n_ID);
				_refEntity.TriggerComboSkillBuff(sKILL_TABLE2.n_ID);
			}
			break;
		}
		case 0:
		{
			SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[weaponStruct.Reload_index];
			_refEntity.CurrentActiveSkill = skillID;
			PushComboSkillBullet(skillID, weaponStruct, sKILL_TABLE);
			if (weaponStruct.Reload_index == weaponStruct.FastBulletDatas.Length - 1)
			{
				OrangeBattleUtility.UpdateSkillCD(weaponStruct, sKILL_TABLE.n_USE_COST, -1f);
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
				}
			}
			else if (_refEntity.IsLocalPlayer)
			{
				_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
				_refEntity.TriggerComboSkillBuff(sKILL_TABLE.n_ID);
			}
			break;
		}
		}
	}

	private void PushComboSkillBullet(int skillID, WeaponStruct skillData, SKILL_TABLE comboSkillData)
	{
		_refEntity.IsShoot = 2;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(comboSkillData, skillData.weaponStatus, _refEntity.ExtraTransforms[0], skillData.SkillLV);
		_refEntity.CheckUsePassiveSkill(skillID, comboSkillData, skillData.weaponStatus, _refEntity.ExtraTransforms[0]);
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		_refEntity.CancelBusterChargeAtk();
		OnSkillEnd();
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

	private void UsePassiveSkill(int _idx, bool _cd)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[_idx];
		if (_cd)
		{
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		}
		_refEntity.CheckUsePassiveSkill(_idx, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CheckSkillEndByShootTimer())
		{
			ToggleBuster(false);
			_refEntity.EnableCurrentWeapon();
		}
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged())
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
		case OrangeCharacter.SubStatus.SKILL1:
			if (_gigaCrushHoverFrame > 0)
			{
				_gigaCrushHoverFrame--;
				if (_gigaCrushHoverFrame > 0)
				{
					_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.4f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.4f));
					break;
				}
				_refEntity.IgnoreGravity = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				_gigaCrushHoverFrame = 5;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (_gigaCrushHoverFrame > 0)
			{
				_gigaCrushHoverFrame--;
				if (_gigaCrushHoverFrame <= 0)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			if (_gigaCrushHoverFrame > 0)
			{
				_gigaCrushHoverFrame--;
				if (_gigaCrushHoverFrame == 10)
				{
					int nowRecordNO = _refEntity.GetNowRecordNO();
					_gigaCrushCollider.UpdateBulletData(_refEntity.PlayerSkills[1].BulletData, _refEntity.sPlayerName, nowRecordNO, _refEntity.nBulletRecordID++);
					_gigaCrushCollider.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
					_gigaCrushCollider.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
					_gigaCrushCollider.Active(base.transform, Vector2.right * (float)_refEntity._characterDirection, _refEntity.TargetMask);
					_refEntity.IsShoot = 1;
				}
				if (_gigaCrushHoverFrame <= 0)
				{
					_refEntity.IsShoot = 0;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_6:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				shootDirection = ((_refEntity.direction == 1) ? Vector3.right : Vector3.left);
				_refEntity.ShootDirection = shootDirection;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct.ShootTransform[0], MagazineType.NORMAL, -1, 0, false);
				isSkillEventEnd = true;
				if (linkSkl1_ex2_link != null)
				{
					PushLinkSkl(linkSkl1_ex2_link, _refEntity.PlayerSkills[1].ShootTransform[0], shootDirection);
				}
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_001_EX3_000, _refEntity.AimTransform.position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.RIDE_ARMOR:
			if (_refEntity.CurrentActiveSkill != 1)
			{
				_refEntity.CurrentActiveSkill = 1;
			}
			if (nowFrame >= skillEventFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Prepare_To_Loop(_refEntity, NOVASTRIKETimer, 1, false, false);
				PlaySkillSE("xd2_nova");
			}
			break;
		case OrangeCharacter.SubStatus.IDLE:
			ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Loop(_refEntity, NOVASTRIKETimer, 1);
			break;
		}
	}

	private void PushLinkSkl(SKILL_TABLE bulletData, Transform shootTransform, Vector3? ShotDir = null)
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		_refEntity.PushBulletDetail(bulletData, currentSkillObj.weaponStatus, shootTransform, currentSkillObj.SkillLV, ShotDir);
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus != 0)
			{
				int num = 1;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				PlaySkillSE("xd2_world01");
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_001_EX1_000, _refEntity.ModelTransform.position + new Vector3(0f, 0.7f, 0f), Quaternion.identity, Array.Empty<object>());
				PlaySkillSE("xd2_world02");
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_001_EX1_002, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				_gigaCrushHoverFrame = 15;
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.EnableCurrentWeapon();
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.IDLE:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_001_EX2_000, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && mainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.SkillEnd = true;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
			case OrangeCharacter.SubStatus.SKILL1_4:
				break;
			}
		}
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		ToggleSaber(false);
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != (HumanBase.AnimateId)72u)
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

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		ToggleSaber(false);
		ToggleBuster(false);
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.BulletCollider != null && _refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			ToggleSaber(false);
			tfBusterMesh.enabled = false;
			_refEntity.EnableCurrentWeapon();
		}
	}

	public override void ControlCharacterDead()
	{
		_gigaCrushHoverFrame = 0;
		ToggleWing(false);
		ToggleSaber(false);
	}

	private void ToggleBuster(bool active)
	{
		tfBusterMesh.enabled = active;
		tfHandMesh.enabled = !active;
	}

	private void ToggleSaber(bool enable)
	{
		if ((bool)cmSaber)
		{
			if (enable)
			{
				cmSaber.Appear();
			}
			else
			{
				cmSaber.Disappear();
			}
		}
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
		if (_tfWind != null && _tfWind.gameObject.activeSelf)
		{
			StopAllCoroutines();
			return;
		}
		ToggleWing(false);
		StopAllCoroutines();
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
		_tfWind.gameObject.SetActive(isActive);
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) <= 2u)
		{
			_refEntity._characterDirection = ((Skill1Directioon.x >= 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_refEntity.BulletCollider != null && _refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length == 2)
		{
			int num = (int)parameters[0];
			int num2 = (int)parameters[1];
			if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT && (int)_refEntity.Hp > 0 && num == 0 && _refEntity.PlayerSkills[0].Reload_index != num2)
			{
				_refEntity.PlayerSkills[0].Reload_index = num2;
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[10] { "ch142_skill_02_ex2_step1_start", "ch142_skill_02_ex1_step1_loop", "ch142_skill_02_ex1_step2_start", "ch142_skill_02_ex1_step2_loop", "ch142_skill_02_ex1_step3_start", "ch142_skill_02_ex1_step3_loop", "ch142_skill_02_ex1_step3_end", "ch142_skill_02_ex3_crouch", "ch142_skill_02_ex3_stand", "ch142_skill_02_ex3_jump" };
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch142_skill_02_ex2_step2_start", "ch142_skill_02_ex2_step2_loop" };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}
}
