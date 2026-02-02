using System;
using System.Collections.Generic;
using Better;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class CH033_Controller : CharacterControlBase, ILogicUpdate
{
	private enum Biometal
	{
		NONE = 0,
		MODEL_ZX = 1
	}

	private Biometal biometal;

	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isTriggerSkl;

	private System.Collections.Generic.Dictionary<int, SKILL_TABLE> dictChargeSkl = new Better.Dictionary<int, SKILL_TABLE>();

	private System.Collections.Generic.Dictionary<int, SKILL_TABLE> dictChargeSklLink = new Better.Dictionary<int, SKILL_TABLE>();

	private int[] chargeTime = new int[0];

	private readonly int conditionId = 1085;

	private int conditionDebuffId = 20067;

	private Transform slashPoint;

	private Transform slashFxPoint;

	private SkinnedMeshRenderer[] skinnedMeshesNormal = new SkinnedMeshRenderer[0];

	private SkinnedMeshRenderer[] skinnedMeshesBiometal = new SkinnedMeshRenderer[0];

	private CharacterMaterial sklWeapon2;

	private UpdateTimer timerSkl1 = new UpdateTimer();

	private UpdateTimer timerSkl2 = new UpdateTimer();

	private bool isHitStop;

	private FxBase hitStopFx;

	private ChargeShootObj _refChargeShootObj;

	private CharacterMaterial[] characterMaterials = new CharacterMaterial[2];

	private readonly int SKL0_TRIGGER_START = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_TRIGGER_END_01 = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_TRIGGER_END_02 = (int)(0.333f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_SLASH_01 = (int)(0.163f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_SLASH_02 = (int)(0.443f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_SLASH_03 = (int)(0.81f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_END = (int)(1.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int[] SKL1_TRIGGER_FX = new int[3] { 4, 9, 14 };

	private int[] SKL1_TRIGGER_FX_NOW = new int[3];

	private readonly string SpWeaponMesh = "CH033_WeaponGun";

	private readonly string SpWeaponMesh2 = "CH033_WeaponSaber";

	private readonly string[] normalMesh = new string[3] { "Aile_BodyMesh_c", "Aile_HandMesh_L_c", "Aile_HandMesh_R_c" };

	private readonly string[] biometalMesh = new string[3] { "ModelZ_BodyMesh_c", "ModelZ_HandMesh_L_c", "ModelZ_HandMesh_R_c" };

	private readonly string FX_0_00 = "fxuse_rockon_000";

	private readonly string FX_0_01 = "fxuse_rockon_001";

	private readonly string FX_1_00 = "fxuse_lifemedal_000";

	private readonly string FX_1_01 = "fxuse_rmz_zero_slash_green";

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.STAGE_TIMESCALE_CHANGE, TimeScaleChange);
		Singleton<GenericEventManager>.Instance.AttachEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, ContinueCall);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.STAGE_TIMESCALE_CHANGE, TimeScaleChange);
		Singleton<GenericEventManager>.Instance.DetachEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, ContinueCall);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public void LogicUpdate()
	{
		CheckBiometal();
	}

	public void LeaveRideArmor(RideBaseObj targetRideArmor)
	{
		_refEntity.LeaveRideArmor(targetRideArmor);
		if (biometal == Biometal.NONE)
		{
			_refEntity.UpdateSkillIcon(_refEntity.PlayerSkills);
			_refEntity.PlayerSkills[0].BackupIcon = _refEntity.PlayerSkills[0].Icon;
			_refEntity.PlayerSkills[1].BackupIcon = _refEntity.PlayerSkills[1].Icon;
		}
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		InitThisSkill();
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_CH033_000", "ch033_charge_lp", "ch033_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_ch033_charge_lp", "bt_ch033_charge_stop" };
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.LeaveRideArmorEvt = LeaveRideArmor;
	}

	private void InitExtraMeshData()
	{
		characterMaterials = _refEntity.ModelTransform.GetComponents<CharacterMaterial>();
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, SpWeaponMesh);
		Renderer[] extraMeshOpen;
		if (array != null)
		{
			OrangeCharacter refEntity = _refEntity;
			extraMeshOpen = new MeshRenderer[array.Length];
			refEntity.ExtraMeshOpen = extraMeshOpen;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshOpen[i] = array[i].GetComponent<MeshRenderer>();
			}
		}
		else
		{
			OrangeCharacter refEntity2 = _refEntity;
			extraMeshOpen = new MeshRenderer[0];
			refEntity2.ExtraMeshOpen = extraMeshOpen;
		}
		OrangeCharacter refEntity3 = _refEntity;
		extraMeshOpen = new MeshRenderer[0];
		refEntity3.ExtraMeshClose = extraMeshOpen;
		skinnedMeshesNormal = new SkinnedMeshRenderer[normalMesh.Length];
		for (int j = 0; j < normalMesh.Length; j++)
		{
			Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, normalMesh[j], true);
			if ((bool)transform)
			{
				skinnedMeshesNormal[j] = transform.GetComponent<SkinnedMeshRenderer>();
			}
		}
		skinnedMeshesBiometal = new SkinnedMeshRenderer[biometalMesh.Length];
		for (int k = 0; k < biometalMesh.Length; k++)
		{
			Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, biometalMesh[k], true);
			if ((bool)transform2)
			{
				skinnedMeshesBiometal[k] = transform2.GetComponent<SkinnedMeshRenderer>();
			}
		}
		EnableBiometalMesh(false);
		slashPoint = OrangeBattleUtility.FindChildRecursive(ref target, "SlashPoint", true);
		if (null == slashPoint)
		{
			slashPoint = _refEntity._transform;
		}
		slashFxPoint = OrangeBattleUtility.FindChildRecursive(ref target, "SlashFxPoint", true);
		if (null == slashFxPoint)
		{
			slashFxPoint = _refEntity._transform;
		}
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "GunShootPoint", true);
		sklWeapon2 = OrangeBattleUtility.FindChildRecursive(ref target, SpWeaponMesh2, true).GetComponent<CharacterMaterial>();
		sklWeapon2.Disappear(null, 0f);
	}

	private void InitThisSkill()
	{
		dictChargeSkl.Clear();
		dictChargeSklLink.Clear();
		chargeTime = new int[0];
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[i];
			if (weaponStruct == null || weaponStruct.BulletData.n_COMBO_SKILL == 0)
			{
				continue;
			}
			SKILL_TABLE value = null;
			if (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(weaponStruct.BulletData.n_COMBO_SKILL, out value) || value.n_CHARGE_MAX_LEVEL <= 0)
			{
				continue;
			}
			int n_CHARGE_MAX_LEVEL = value.n_CHARGE_MAX_LEVEL;
			chargeTime = new int[n_CHARGE_MAX_LEVEL + 1];
			for (int j = 0; j < n_CHARGE_MAX_LEVEL + 1; j++)
			{
				SKILL_TABLE value2 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(value.n_ID + j, out value2))
				{
					_refEntity.tRefPassiveskill.ReCalcuSkill(ref value2);
					dictChargeSkl.Add(j, value2);
					SKILL_TABLE value3 = null;
					if (value2.n_LINK_SKILL > 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(value2.n_LINK_SKILL, out value3))
					{
						_refEntity.tRefPassiveskill.ReCalcuSkill(ref value3);
						dictChargeSklLink.Add(value2.n_ID, value3);
					}
					chargeTime[j] = value2.n_CHARGE;
				}
				else
				{
					chargeTime[j] = 0;
				}
			}
		}
		if (_refEntity.tRefPassiveskill.listPassiveskill.Count > 0)
		{
			for (int k = 0; k < _refEntity.tRefPassiveskill.listPassiveskill.Count; k++)
			{
				SKILL_TABLE tSKILL_TABLE = _refEntity.tRefPassiveskill.listPassiveskill[k].tSKILL_TABLE;
				CONDITION_TABLE value4;
				if (tSKILL_TABLE.n_CONDITION_ID > 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(tSKILL_TABLE.n_CONDITION_ID, out value4) && value4.f_EFFECT_X == (float)conditionId)
				{
					conditionDebuffId = value4.n_ID;
					break;
				}
			}
		}
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<SplashBullet>("prefab/bullet/splashbullet", "SplashBullet", 3, null);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_01, 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_slash_000", 3);
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refChargeShootObj.Charging && _refEntity.PlayerSetting.AutoCharge == 1 && id == 0 && _refEntity.selfBuffManager.nMeasureNow > 0)
		{
			PrepareChargeShoot(id);
			return;
		}
		switch (id)
		{
		case 0:
			switch (biometal)
			{
			case Biometal.NONE:
				if (_refEntity.selfBuffManager.nMeasureNow >= 2 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
				{
					endFrame = nowFrame + SKL0_TRIGGER_END_01;
					isTriggerSkl = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
					_refEntity.SkillEnd = false;
					_refEntity.DisableCurrentWeapon();
					_refEntity.CurrentActiveSkill = id;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_00, _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_01, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
					_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
					_refEntity.PlaySE(_refEntity.SkillSEID, "ch033_modelzx");
					_refEntity.PlaySE(_refEntity.VoiceID, "v_ch033_skill01");
				}
				break;
			case Biometal.MODEL_ZX:
				_refEntity.Change_Chargefx_Layer(ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer);
				if (_refEntity.PlayerSetting.AutoCharge == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
				{
					PrepareCharge(id);
				}
				break;
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refChargeShootObj.StopCharge();
			switch (biometal)
			{
			case Biometal.NONE:
				skillEventFrame = nowFrame + SKL0_TRIGGER_START;
				endFrame = nowFrame + SKL0_TRIGGER_END_01;
				isTriggerSkl = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity._transform, Quaternion.identity, Array.Empty<object>());
				_refEntity.PlaySE(_refEntity.SkillSEID, "ch033_livemetal");
				_refEntity.PlaySE(_refEntity.VoiceID, "v_ch033_skill02");
				break;
			case Biometal.MODEL_ZX:
			{
				skillEventFrame = nowFrame + SKL1_TRIGGER_SLASH_01;
				endFrame = nowFrame + SKL1_TRIGGER_END;
				isTriggerSkl = false;
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				sklWeapon2.Appear(null, 0f);
				for (int i = 0; i < SKL1_TRIGGER_FX_NOW.Length; i++)
				{
					SKL1_TRIGGER_FX_NOW[i] = nowFrame + SKL1_TRIGGER_FX[i];
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				_refEntity.PlaySE(_refEntity.SkillSEID, "ch033_zsaber");
				break;
			}
			}
			_refEntity.DisableCurrentWeapon();
			_refEntity.CurrentActiveSkill = id;
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public void PlayerHeldSkill(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge == 0 && !_refChargeShootObj.Charging && biometal == Biometal.MODEL_ZX && _refEntity.CheckUseSkillKeyTriggerEX(id))
		{
			PrepareCharge(id);
		}
	}

	public void PlayerReleaseSkill(int id)
	{
		if (id != 0)
		{
			int num = 1;
			return;
		}
		switch (biometal)
		{
		case Biometal.NONE:
			_refChargeShootObj.StopCharge();
			break;
		case Biometal.MODEL_ZX:
			if (_refEntity.CurrentActiveSkill == -1 && _refChargeShootObj.Charging && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.selfBuffManager.nMeasureNow > 0 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				PrepareChargeShoot(id);
			}
			break;
		}
	}

	private void PrepareCharge(int id)
	{
		if (id != 1 && !_refEntity.PlayerSkills[id].ChargeTimer.IsStarted())
		{
			_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
			_refChargeShootObj.StartCharge();
		}
	}

	private void PrepareChargeShoot(int id)
	{
		if (id != 1)
		{
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			_refEntity.ToggleExtraMesh(true);
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
		}
	}

	public override void CheckSkill()
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1 || _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				if (nowFrame > endFrame)
				{
					if (_refEntity.PlayerSkills[1].MagazineRemain <= 0f)
					{
						timerSkl1.SetStarted(true);
					}
					else
					{
						timerSkl1.TimerStop();
					}
					timerSkl1.SetTime(_refEntity.PlayerSkills[1].LastUseTimer.GetTime());
					if (_refEntity.IsLocalPlayer)
					{
						_refEntity.selfBuffManager.AddBuff(_refEntity.PlayerSkills[0].BulletData.n_CONDITION_ID, 0, 0, _refEntity.PlayerSkills[0].BulletData.n_ID);
					}
					OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[0]);
					endFrame = nowFrame + SKL0_TRIGGER_END_02;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (nowFrame > endFrame)
				{
					CancelSkl000();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				if ((double)_refEntity.CurrentFrame > 0.1)
				{
					_refChargeShootObj.StopCharge();
					if (biometal == Biometal.MODEL_ZX)
					{
						_refEntity.FreshBullet = true;
						_refEntity.IsShoot = 1;
						SKILL_TABLE sKILL_TABLE2 = dictChargeSkl[_refEntity.PlayerSkills[0].ChargeLevel];
						SKILL_TABLE value = null;
						OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[0], sKILL_TABLE2.n_USE_COST, -1f);
						_refEntity.PushBulletDetail(sKILL_TABLE2, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ExtraTransforms[0], _refEntity.PlayerSkills[0].SkillLV);
						if (dictChargeSklLink.TryGetValue(sKILL_TABLE2.n_ID, out value))
						{
							_refEntity.PushBulletDetail(value, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ExtraTransforms[0], _refEntity.PlayerSkills[0].SkillLV);
						}
						_refEntity.PlaySE(_refEntity.VoiceID, "v_ch033_skill01_" + (_refEntity.PlayerSkills[0].ChargeLevel + 1));
						_refEntity.CheckUsePassiveSkill(0, sKILL_TABLE2, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
					}
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
				}
				else
				{
					CheckChargeInputStatus();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				if ((double)_refEntity.CurrentFrame > 0.5)
				{
					_refEntity.IsShoot = 0;
					_refEntity.CurrentActiveSkill = -1;
					ResetLastStatus();
				}
				else
				{
					_refEntity.IsShoot = 0;
					CheckChargeInputStatus();
				}
				break;
			}
			break;
		case 1:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				if (nowFrame > endFrame)
				{
					CancelSkl001();
				}
				else
				{
					if (isTriggerSkl || nowFrame <= skillEventFrame)
					{
						break;
					}
					isTriggerSkl = true;
					WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
					_refEntity.selfBuffManager.AddBuff(weaponStruct.BulletData.n_CONDITION_ID, 0, 0, weaponStruct.BulletData.n_ID);
					int n_LINK_SKILL = weaponStruct.BulletData.n_LINK_SKILL;
					if (n_LINK_SKILL > 0)
					{
						SKILL_TABLE[] fastBulletDatas = weaponStruct.FastBulletDatas;
						foreach (SKILL_TABLE sKILL_TABLE in fastBulletDatas)
						{
							if (sKILL_TABLE.n_ID == n_LINK_SKILL)
							{
								RefPassiveskill.TriggerSkill(sKILL_TABLE, weaponStruct.SkillLV, 4095, _refEntity.selfBuffManager, _refEntity.selfBuffManager, 0);
								break;
							}
						}
					}
					OrangeBattleUtility.UpdateSkillCD(weaponStruct);
					_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[1]);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (nowFrame == SKL1_TRIGGER_FX_NOW[0])
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_01, slashFxPoint, Quaternion.identity, Array.Empty<object>());
					_refEntity.PlaySE(_refEntity.VoiceID, "v_ch033_skill02_1");
				}
				if (nowFrame > skillEventFrame)
				{
					CreateSplashBullet(1);
					skillEventFrame = nowFrame + SKL1_TRIGGER_SLASH_02 - SKL1_TRIGGER_SLASH_01;
					if (biometal == Biometal.MODEL_ZX)
					{
						OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
						_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[1]);
					}
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (nowFrame == SKL1_TRIGGER_FX_NOW[1])
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_01, slashFxPoint, Quaternion.identity, Array.Empty<object>());
					_refEntity.PlaySE(_refEntity.VoiceID, "v_ch033_skill02_2");
				}
				if (nowFrame > skillEventFrame)
				{
					CreateSplashBullet(1);
					skillEventFrame = nowFrame + SKL1_TRIGGER_SLASH_03 - SKL1_TRIGGER_SLASH_02 - SKL1_TRIGGER_SLASH_01;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				if (nowFrame == SKL1_TRIGGER_FX_NOW[2])
				{
					hitStopFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_1_01, slashFxPoint, Quaternion.identity, Array.Empty<object>());
					_refEntity.PlaySE(_refEntity.VoiceID, "v_ch033_skill02_3");
				}
				if (nowFrame > skillEventFrame)
				{
					CreateSplashBullet(1);
					skillEventFrame = endFrame;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				if (isHitStop)
				{
					isHitStop = false;
					skillEventFrame = endFrame - 2;
					if (nowFrame >= endFrame)
					{
						skillEventFrame = nowFrame;
					}
					_refEntity.Animator._animator.speed = 0f;
					endFrame += 5;
					if (hitStopFx != null)
					{
						hitStopFx.pPS.Pause(true);
					}
				}
				else if (nowFrame == skillEventFrame)
				{
					if (hitStopFx != null && hitStopFx.pPS.isPaused)
					{
						hitStopFx.pPS.Play(true);
					}
					_refEntity.Animator._animator.speed = 1f;
				}
				if (nowFrame > endFrame)
				{
					CancelSkl001();
				}
				break;
			}
			break;
		}
	}

    [Obsolete]
    private void CreateSplashBullet(int skillIdx)
	{
		SplashBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SplashBullet>("SplashBullet");
		poolObj.HitCallback = (CallbackObj)Delegate.Combine(poolObj.HitCallback, new CallbackObj(SplashHitCB));
		poolObj.UpdateBulletData(_refEntity.PlayerSkills[skillIdx].FastBulletDatas[1], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		poolObj.SetBulletAtk(_refEntity.PlayerSkills[skillIdx].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
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
			if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_4)
			{
				isHitStop = true;
			}
		}
	}

	private void CheckBiometal()
	{
		switch (biometal)
		{
		case Biometal.NONE:
			if (timerSkl2.IsStarted())
			{
				timerSkl2 += GameLogicUpdateManager.m_fFrameLenMS;
			}
			if (_refEntity.selfBuffManager.nMeasureNow > 0 && _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionId))
			{
				biometal = Biometal.MODEL_ZX;
				EnableBiometalMesh(true);
				_refEntity.PlayerSkills[0].ChargeTime = chargeTime;
				if (_refEntity is OrangeConsoleCharacter)
				{
					(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL0, true);
				}
				if (timerSkl2.IsStarted())
				{
					_refEntity.PlayerSkills[1].MagazineRemain = 0f;
				}
				else
				{
					SKILL_TABLE bulletData2 = _refEntity.PlayerSkills[1].BulletData;
					_refEntity.PlayerSkills[1].MagazineRemain = bulletData2.n_MAGAZINE;
				}
				_refEntity.PlayerSkills[1].LastUseTimer.SetTime(timerSkl2.GetTime());
				_refEntity.PlayerSkills[1].LastUseTimer.SetStarted(timerSkl2.IsStarted());
				timerSkl2.TimerStop();
			}
			break;
		case Biometal.MODEL_ZX:
			if (timerSkl1.IsStarted())
			{
				timerSkl1 += GameLogicUpdateManager.m_fFrameLenMS;
			}
			if (_refEntity.selfBuffManager.nMeasureNow <= 0 || !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionId))
			{
				biometal = Biometal.NONE;
				EnableBiometalMesh(false);
				int n_CHARGE_MAX_LEVEL = _refEntity.PlayerSkills[0].FastBulletDatas[0].n_CHARGE_MAX_LEVEL;
				_refEntity.PlayerSkills[0].ChargeTime = new int[n_CHARGE_MAX_LEVEL + 1];
				for (int i = 0; i <= n_CHARGE_MAX_LEVEL; i++)
				{
					SKILL_TABLE value = null;
					if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(_refEntity.PlayerSkills[0].FastBulletDatas[0].n_ID + i, out value))
					{
						_refEntity.PlayerSkills[0].ChargeTime[i] = value.n_CHARGE;
					}
					else
					{
						_refEntity.PlayerSkills[0].ChargeTime[i] = 0;
					}
				}
				if (_refEntity.PlayerSkills[1].MagazineRemain <= 0f)
				{
					timerSkl2.SetStarted(true);
				}
				else
				{
					timerSkl2.TimerStop();
				}
				timerSkl2.SetTime(_refEntity.PlayerSkills[1].LastUseTimer.GetTime());
				if (timerSkl1.IsStarted())
				{
					_refEntity.PlayerSkills[1].MagazineRemain = 0f;
				}
				else
				{
					SKILL_TABLE bulletData = _refEntity.PlayerSkills[1].BulletData;
					_refEntity.PlayerSkills[1].MagazineRemain = bulletData.n_MAGAZINE;
				}
				_refEntity.PlayerSkills[1].LastUseTimer.SetTime(timerSkl1.GetTime());
				_refEntity.PlayerSkills[1].LastUseTimer.SetStarted(timerSkl1.IsStarted());
				timerSkl1.TimerStop();
				if (_refChargeShootObj.Charging)
				{
					_refChargeShootObj.StopCharge();
				}
				if (_refEntity is OrangeConsoleCharacter)
				{
					(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL0, true);
				}
			}
			else if (StageUpdate.gbIsNetGame && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionDebuffId))
			{
				_refEntity.selfBuffManager.AddBuff(conditionDebuffId, 0, 0, _refEntity.PlayerSkills[0].BulletData.n_ID);
			}
			break;
		}
	}

	private void CheckChargeInputStatus()
	{
		bool below = _refEntity.Controller.Collisions.below;
		if (_refEntity.AnimateID == (HumanBase.AnimateId)131u && below)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.Dashing = false;
			_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			_refEntity.Animator.PlayAnimation(HumanBase.AnimateId.ANI_BTSKILL_START, _refEntity.CurrentFrame);
			return;
		}
		CharacterDirection characterDirection = _refEntity._characterDirection;
		ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT);
		ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT);
		if (_refEntity.AnimateID == (HumanBase.AnimateId)129u || IsHeldSameDirection())
		{
			return;
		}
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.Dashing = false;
			_refEntity.SetHorizontalSpeed(0);
			if (!ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				_refEntity.Animator.PlayAnimation(HumanBase.AnimateId.ANI_BTSKILL_START, _refEntity.CurrentFrame);
			}
		}
		else
		{
			_refEntity.ForceSetAnimateId((HumanBase.AnimateId)131u);
			_refEntity.Animator.PlayAnimation((HumanBase.AnimateId)131u, _refEntity.CurrentFrame);
		}
	}

	private void EnableBiometalMesh(bool p_active)
	{
		bool flag = skinnedMeshesBiometal[1].enabled;
		for (int i = 0; i < skinnedMeshesNormal.Length; i++)
		{
			skinnedMeshesNormal[i].enabled = !p_active;
		}
		for (int j = 0; j < skinnedMeshesBiometal.Length; j++)
		{
			skinnedMeshesBiometal[j].enabled = p_active;
		}
		if (p_active)
		{
			_refEntity._handMesh = new SkinnedMeshRenderer[1] { skinnedMeshesBiometal[1] };
			_refEntity.CharacterMaterials = characterMaterials[0];
		}
		else
		{
			_refEntity._handMesh = new SkinnedMeshRenderer[1] { skinnedMeshesNormal[1] };
			_refEntity.CharacterMaterials = characterMaterials[1];
			skinnedMeshesNormal[1].enabled = flag;
		}
		_refEntity.CharacterMaterials.ResetPrepertyBlock();
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			switch (_refEntity.AnimateID)
			{
			case HumanBase.AnimateId.ANI_WALKBACK:
			case HumanBase.AnimateId.ANI_STEP:
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			default:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case HumanBase.AnimateId.ANI_WALK:
			case HumanBase.AnimateId.ANI_WALKSLASH1:
			case HumanBase.AnimateId.ANI_WALKSLASH2:
			case HumanBase.AnimateId.ANI_WALKSLASH1_END:
			case HumanBase.AnimateId.ANI_WALKSLASH2_END:
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				break;
			case HumanBase.AnimateId.ANI_DASH:
			case HumanBase.AnimateId.ANI_DASH_END:
			case HumanBase.AnimateId.ANI_AIRDASH_END:
			case HumanBase.AnimateId.ANI_DASHSLASH1:
			case HumanBase.AnimateId.ANI_DASHSLASH2:
			case HumanBase.AnimateId.ANI_DASHSLASH1_END:
			case HumanBase.AnimateId.ANI_DASHSLASH2_END:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				break;
			case HumanBase.AnimateId.ANI_JUMP:
			case HumanBase.AnimateId.ANI_JUMPSLASH:
				_refEntity.SetAnimateId((HumanBase.AnimateId)130u);
				break;
			case HumanBase.AnimateId.ANI_FALL:
			case HumanBase.AnimateId.ANI_LAND:
			case HumanBase.AnimateId.ANI_WALLGRAB_BEGIN:
			case HumanBase.AnimateId.ANI_WALLGRAB:
			case HumanBase.AnimateId.ANI_WALLGRAB_END:
			case HumanBase.AnimateId.ANI_WALLGRAB_SLASH:
			case HumanBase.AnimateId.ANI_WALLGRAB_SLASH_END:
			case HumanBase.AnimateId.ANI_WALLKICK:
			case HumanBase.AnimateId.ANI_WALLKICK_END:
				_refEntity.SetAnimateId((HumanBase.AnimateId)131u);
				break;
			case HumanBase.AnimateId.ANI_CROUCH:
			case HumanBase.AnimateId.ANI_CROUCH_END:
			case HumanBase.AnimateId.ANI_CROUCH_UP:
			case HumanBase.AnimateId.ANI_CROUCHSLASH1:
			case HumanBase.AnimateId.ANI_CROUCHSLASH1_END:
				_refEntity.SetAnimateId((HumanBase.AnimateId)132u);
				break;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			_refEntity.ToggleExtraMesh(false);
			if (_refEntity.GetCurrentSkillObj().MagazineRemain > 0f)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			}
			if (_refEntity.CurrentActiveSkill == 1)
			{
				sklWeapon2.Disappear(null, 0f);
			}
		}
		CheckDebuffStatus();
		if (_refChargeShootObj.Charging)
		{
			_refChargeShootObj.StopCharge();
		}
		_refEntity.IsShoot = 0;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.Animator._animator.speed = 1f;
	}

	public override void SetStun(bool enable)
	{
		if (!enable)
		{
			CheckDebuffStatus();
		}
	}

	private void CheckDebuffStatus()
	{
		switch (biometal)
		{
		case Biometal.NONE:
			if (_refEntity.selfBuffManager.CheckHasEffect(113))
			{
				_refEntity.selfBuffManager.RemoveBuff(113);
			}
			break;
		case Biometal.MODEL_ZX:
			if (!_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionDebuffId))
			{
				_refEntity.selfBuffManager.AddBuff(conditionDebuffId, 0, 0, _refEntity.PlayerSkills[0].BulletData.n_ID);
			}
			break;
		}
	}

	private void CancelSkl000()
	{
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		ResetLastStatus();
	}

	private void CancelSkl001()
	{
		hitStopFx = null;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		sklWeapon2.Disappear(null, 0f);
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.Animator._animator.speed < 1f)
		{
			_refEntity.Animator._animator.speed = 1f;
		}
		ResetLastStatus();
	}

	private void ResetLastStatus()
	{
		switch (_refEntity.AnimateID)
		{
		case HumanBase.AnimateId.ANI_BTSKILL_START:
		case (HumanBase.AnimateId)128u:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.WALK, OrangeCharacter.SubStatus.WIN_POSE);
				break;
			}
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case (HumanBase.AnimateId)129u:
			if (_refEntity.Dashing)
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_DASH);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.DASH, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				else
				{
					_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_DASH);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.AIRDASH, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
			}
			else
			{
				_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				ResetLastStatus();
			}
			break;
		default:
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.PlayerStopDashing();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				break;
			}
			if (_refEntity.IgnoreGravity)
			{
				_refEntity.IgnoreGravity = false;
			}
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)132u:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
				break;
			}
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			break;
		}
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.ToggleExtraMesh(false);
		_refEntity.EnableCurrentWeapon();
	}

	private bool IsHeldSameDirection()
	{
		bool flag = _refEntity._characterDirection == CharacterDirection.RIGHT;
		bool flag2 = ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT);
		bool flag3 = ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT);
		if (!(flag && flag3) && !(!flag && flag2))
		{
			return flag3 ^ flag2;
		}
		return false;
	}

	public void TeleportInCharacterDepend()
	{
		skinnedMeshesNormal[1].enabled = true;
		if (_refEntity.CurrentFrame >= 0.5f)
		{
			_refEntity.ToggleExtraMesh(false);
		}
	}

	protected void TimeScaleChange(bool isChange)
	{
		if (isChange)
		{
			_refEntity.Animator._animator.speed = 1f;
		}
		else if (_refEntity.CurrentActiveSkill != 1 && _refEntity.Animator._animator.speed != 1f)
		{
			_refEntity.Animator._animator.speed = 1f;
		}
	}

	private void ContinueCall(string playerId, bool setPos, float posX, float posY, bool? lookback)
	{
		if (!(playerId != _refEntity.sPlayerID))
		{
			biometal = Biometal.NONE;
			EnableBiometalMesh(false);
		}
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch033_skill_01_stand_up", "ch033_skill_01_stand_mid", "ch033_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch033_skill_01_run_up", "ch033_skill_01_run_mid", "ch033_skill_01_run_down" };
		string[] array3 = new string[3] { "ch033_skill_01_dash_up_loop", "ch033_skill_01_dash_mid_loop", "ch033_skill_01_dash_down_loop" };
		string[] array4 = new string[3] { "ch033_skill_01_jump_up_loop", "ch033_skill_01_jump_mid_loop", "ch033_skill_01_jump_down_loop" };
		string[] array5 = new string[3] { "ch033_skill_01_fall_up_loop", "ch033_skill_01_fall_mid_loop", "ch033_skill_01_fall_down_loop" };
		string[] array6 = new string[3] { "ch033_skill_01_crouch_up", "ch033_skill_01_crouch_mid", "ch033_skill_01_crouch_down" };
		return new string[6][] { array, array2, array3, array4, array5, array6 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch033_skill_01_stand_change_start", "ch033_skill_01_stand_change_end", "ch033_skill_01_jump_change_start", "ch033_skill_01_jump_change_end", "ch033_skill_02_stand_uncharge", "ch033_skill_02_jump_uncharge" };
	}
}
