using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

public class CH049_Controller : CharacterControlBase
{
	private readonly int SKILL0_START = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_END = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_CANCEL = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_START = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_CANCEL = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private int skillEndFrame;

	private int skillCancelFrame;

	private ChargeShootObj chargeShootObject;

	private System.Collections.Generic.Dictionary<int, SKILL_TABLE> chargeSkillList = new Better.Dictionary<int, SKILL_TABLE>();

	private System.Collections.Generic.Dictionary<int, SKILL_TABLE> chargeLinkSkillList = new Better.Dictionary<int, SKILL_TABLE>();

	private int[] chargeTime = new int[0];

	private Transform shootPointTransform;

	private Transform shootPointTransform2;

	private SkinnedMeshRenderer tfLHandMesh;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		InitializeExtraMesh();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Bip L Hand", true);
		shootPointTransform2 = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		chargeSkillList.Clear();
		chargeLinkSkillList.Clear();
		chargeTime = new int[0];
		chargeShootObject = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			chargeShootObject.ChargeSE = new string[3] { "SkillSE_CH049_000", "ch049_charge_lp", "ch049_charge_stop" };
		}
		else
		{
			chargeShootObject.ChargeSE = new string[3] { "BattleSE02", "bt_ch049_charge_lp", "bt_ch049_charge_stop" };
		}
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			SKILL_TABLE bulletData = _refEntity.PlayerSkills[i].BulletData;
			if (bulletData == null || bulletData.n_CHARGE_MAX_LEVEL <= 0)
			{
				continue;
			}
			int n_CHARGE_MAX_LEVEL = bulletData.n_CHARGE_MAX_LEVEL;
			chargeTime = new int[n_CHARGE_MAX_LEVEL + 1];
			for (int j = 0; j < n_CHARGE_MAX_LEVEL + 1; j++)
			{
				SKILL_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(bulletData.n_ID + j, out value))
				{
					_refEntity.tRefPassiveskill.ReCalcuSkill(ref value);
					chargeSkillList.Add(j, value);
					SKILL_TABLE value2 = null;
					if (value.n_LINK_SKILL > 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(value.n_LINK_SKILL, out value2))
					{
						_refEntity.tRefPassiveskill.ReCalcuSkill(ref value2);
						chargeLinkSkillList.Add(value.n_ID, value2);
					}
					chargeTime[j] = value.n_CHARGE;
				}
				else
				{
					chargeTime[j] = 0;
				}
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_exechargeshot_003", 2);
	}

	private void InitializeExtraMesh()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "BusterMesh_m");
		Renderer[] extraMeshOpen;
		if (array != null)
		{
			OrangeCharacter refEntity = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[array.Length];
			refEntity.ExtraMeshOpen = extraMeshOpen;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshOpen[i] = array[i].GetComponent<SkinnedMeshRenderer>();
			}
		}
		else
		{
			OrangeCharacter refEntity2 = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[0];
			refEntity2.ExtraMeshOpen = extraMeshOpen;
		}
		OrangeCharacter refEntity3 = _refEntity;
		extraMeshOpen = new SkinnedMeshRenderer[0];
		refEntity3.ExtraMeshClose = extraMeshOpen;
		array = OrangeBattleUtility.FindAllChildRecursive(ref target, "HandMesh_L_c");
		_refEntity._handMesh = new SkinnedMeshRenderer[array.Length];
		for (int j = 0; j < array.Length; j++)
		{
			_refEntity._handMesh[j] = array[j].GetComponent<SkinnedMeshRenderer>();
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1 || id != 0 || _refEntity.PlayerSetting.AutoCharge != 1 || !_refEntity.CheckUseSkillKeyTrigger(id))
		{
			return;
		}
		if (!chargeShootObject.Charging)
		{
			_refEntity.Change_Chargefx_Layer(ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer);
			if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted())
			{
				_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
				chargeShootObject.StartCharge();
			}
		}
		else if (chargeShootObject.Charging)
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.ToggleExtraMesh(true);
			_refEntity.EnableHandMesh(false);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
	}

	public void PlayerHeldSkill(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.PlayerSetting.AutoCharge == 0 && !chargeShootObject.Charging && _refEntity.PlayerSkills[id].MagazineRemain > 0f && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
			chargeShootObject.StartCharge();
		}
	}

	public void PlayerReleaseSkill(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1 || _refEntity.PlayerSkills[id].LastUseTimer.GetMillisecond() < _refEntity.PlayerSkills[id].BulletData.n_FIRE_SPEED || _refEntity.PlayerSkills[id].MagazineRemain <= 0f || _refEntity.PlayerSkills[id].ForceLock || _refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.PlayerSetting.AutoCharge == 0 && chargeShootObject.Charging && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerStopDashing();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SkillEnd = false;
				_refEntity.CurrentActiveSkill = id;
				_refEntity.ToggleExtraMesh(true);
				_refEntity.EnableHandMesh(false);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerStopDashing();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SkillEnd = false;
				_refEntity.CurrentActiveSkill = id;
				_refEntity.EnableHandMesh(true);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_exechargeshot_003", shootPointTransform2, Quaternion.identity, Array.Empty<object>());
				PlaySkillSE("ch049_straight01");
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN || mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
			}
			skillEndFrame = gameFrame + SKILL0_START;
			PlayVoiceSE("v_ch049_skill01");
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			_refEntity.FreshBullet = true;
			_refEntity.IsShoot = 1;
			_refEntity.StartShootTimer();
			SKILL_TABLE sKILL_TABLE = chargeSkillList[_refEntity.PlayerSkills[0].ChargeLevel];
			_refEntity.PushBulletDetail(sKILL_TABLE, _refEntity.PlayerSkills[0].weaponStatus, shootPointTransform, _refEntity.PlayerSkills[0].SkillLV);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill], sKILL_TABLE.n_USE_COST, -1f);
			chargeShootObject.StopCharge();
			skillEndFrame = gameFrame + SKILL0_END;
			skillCancelFrame = gameFrame + SKILL0_CANCEL;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1:
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)130u);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)131u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)132u);
			}
			skillEndFrame = gameFrame + SKILL1_START;
			PlayVoiceSE("v_ch049_skill02");
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
			CreateSkillBullet(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
			skillEndFrame = gameFrame + SKILL1_END;
			skillCancelFrame = gameFrame + SKILL1_CANCEL;
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		int gameFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (gameFrame < skillEndFrame && (!CheckCancelAnimate(_refEntity.CurrentActiveSkill) || gameFrame < skillCancelFrame))
			{
				break;
			}
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_BTSKILL_START)
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
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			_refEntity.IsShoot = 0;
			ResetSkillStatus();
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (gameFrame < skillEndFrame && (!CheckCancelAnimate(_refEntity.CurrentActiveSkill) || gameFrame < skillCancelFrame))
			{
				break;
			}
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == (HumanBase.AnimateId)130u)
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
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			ResetSkillStatus();
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			switch (_refEntity.CurrentActiveSkill)
			{
			case 0:
				_refEntity.SetSpeed(0, 0);
				break;
			case 1:
				_refEntity.SetSpeed(0, 0);
				break;
			}
		}
		ResetSkillStatus();
	}

	private bool CheckCancelAnimate(int skillId)
	{
		return ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID);
	}

	public override void ControlCharacterDead()
	{
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, shootPointTransform, weaponStruct.SkillLV);
	}

	private void ResetSkillStatus()
	{
		_refEntity.ToggleExtraMesh(false);
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInExtraEffect()
	{
		_refEntity.ToggleExtraMesh(false);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		PlaySkillSE("ch049_start01");
		if (_refEntity.CurrentFrame >= 0.8f)
		{
			_refEntity.ToggleExtraMesh(false);
		}
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch049_startin_000";
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch049_skill_01_crouch_up", "ch049_skill_01_crouch_mid", "ch049_skill_01_crouch_down" };
		string[] array2 = new string[3] { "ch049_skill_01_stand_up", "ch049_skill_01_stand_mid", "ch049_skill_01_stand_down" };
		string[] array3 = new string[3] { "ch049_skill_01_jump_up", "ch049_skill_01_jump_mid", "ch049_skill_01_jump_end" };
		string[] array4 = new string[3] { "ch049_skill_02_crouch_up", "ch049_skill_02_crouch_mid", "ch049_skill_02_crouch_down" };
		string[] array5 = new string[3] { "ch049_skill_02_stand_up", "ch049_skill_02_stand_mid", "ch049_skill_02_stand_down" };
		string[] array6 = new string[3] { "ch049_skill_02_jump_up", "ch049_skill_02_jump_mid", "ch049_skill_02_jump_down" };
		return new string[6][] { array, array2, array3, array4, array5, array6 };
	}
}
