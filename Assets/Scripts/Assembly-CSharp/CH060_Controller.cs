using System;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class CH060_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private SKILL_TABLE linkSkl;

	private CharacterDirection skl1Direction = CharacterDirection.LEFT;

	private Transform shootPointTransform;

	private GameObject SaberMesh_L_c;

	private GameObject SaberMesh_R_c;

	private MeleeWeaponTrail saberTrail_L;

	private MeleeWeaponTrail saberTrail_R;

	public PetControllerBase mBit;

	private int nBitID = -1;

	private long nBitTime;

	private int nMineCount;

	protected List<SCH004Controller> _liPets = new List<SCH004Controller>();

	private readonly string sSaberMesh_L_c = "SaberMesh_L_c";

	private readonly string sSaberMesh_R_c = "SaberMesh_R_c";

	private readonly string sWeaponBone_L = "WeaponBone_L";

	private readonly string sWeaponBone_R = "WeaponBone_R";

	private readonly string sFX_SKL1_00 = "fxuse_supersonicboom_000";

	private readonly string sFX_SKL1_01 = "fxuse_supersonicboom_001";

	private readonly string sFxuseCutIn = "fxdemo_tornado_001";

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly int SKL0_0_TRIGGER = (int)(0.365f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_0_START_END = (int)(0.633f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_0_START_END_BREAK = (int)(0.433 / (double)GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_0_END_END = (int)(0.22f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_TRIGGER = (int)(0.109f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_END_END = (int)(0.233f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_2_TRIGGER = (int)(0.334f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_2_START_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_2_START_END_BREAK = (int)(0.467 / (double)GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_2_END_END = (int)(0.333f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_START_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_END = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_END_BREAK = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER_LINK = 2;

	private readonly int RELOAD_INDEX_MAX = 2;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.9f)
		{
			UpdateCustomWeaponRenderer(false);
		}
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 0.3f && currentFrame <= 1f)
			{
				UpdateCustomWeaponRenderer(false);
			}
		}
	}

	private void InitializeSkill()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFX_SKL1_00);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFX_SKL1_01);
		shootPointTransform = new GameObject(sCustomShootPoint).transform;
		shootPointTransform.SetParent(base.transform);
		shootPointTransform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		SaberMesh_L_c = OrangeBattleUtility.FindChildRecursive(ref target, sSaberMesh_L_c, true).gameObject;
		SaberMesh_L_c.SetActive(true);
		SaberMesh_R_c = OrangeBattleUtility.FindChildRecursive(ref target, sSaberMesh_R_c, true).gameObject;
		SaberMesh_R_c.SetActive(true);
		saberTrail_L = OrangeBattleUtility.FindChildRecursive(ref target, sWeaponBone_L, true).GetComponent<MeleeWeaponTrail>();
		saberTrail_R = OrangeBattleUtility.FindChildRecursive(ref target, sWeaponBone_R, true).GetComponent<MeleeWeaponTrail>();
		saberTrail_L.Emit = false;
		saberTrail_R.Emit = false;
		linkSkl = null;
		for (int i = 0; i < _refEntity.PlayerSkills.Length; i++)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[i];
			if (weaponStruct != null && weaponStruct.BulletData.n_LINK_SKILL != 0 && linkSkl == null && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out linkSkl))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkl);
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<BasicBullet>("prefab/bullet/" + linkSkl.s_MODEL, linkSkl.s_MODEL, 2, null);
				break;
			}
		}
		if (_refEntity.tRefPassiveskill.listUsePassiveskill.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < _refEntity.tRefPassiveskill.listUsePassiveskill.Count; j++)
		{
			SKILL_TABLE tSKILL_TABLE = _refEntity.tRefPassiveskill.listUsePassiveskill[j].tSKILL_TABLE;
			if (tSKILL_TABLE.n_EFFECT == 16)
			{
				InitPetMode(tSKILL_TABLE);
				break;
			}
		}
	}

	private void InitPetMode(SKILL_TABLE useSkl)
	{
		nBitID = (int)useSkl.f_EFFECT_X;
		nBitTime = (long)(useSkl.f_EFFECT_Z * 1000f);
		CreatePet(nBitID, 1);
	}

	protected void CreatePet(int petID, int skillIndex)
	{
		PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
		petBuilder.PetID = petID;
		petBuilder.follow_skill_id = skillIndex;
		petBuilder.CreatePet(delegate(SCH004Controller obj)
		{
			obj.Set_follow_Player(_refEntity, false);
			obj.SetFollowEnabled(false);
			PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[petID];
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SCH004Controller>(obj, pET_TABLE.s_MODEL, 6);
			obj.SetParams(pET_TABLE.s_MODEL, 0L, 0, null, 0L);
		});
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				UpdateCustomWeaponRenderer(true);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				UpdateCustomWeaponRenderer(true);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				SetAnimate((HumanBase.AnimateId)71u, (HumanBase.AnimateId)68u, HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				SetAnimate((HumanBase.AnimateId)80u, (HumanBase.AnimateId)77u, (HumanBase.AnimateId)74u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SetAnimate((HumanBase.AnimateId)89u, (HumanBase.AnimateId)86u, (HumanBase.AnimateId)83u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
			case OrangeCharacter.SubStatus.SKILL0_4:
			case OrangeCharacter.SubStatus.SKILL0_5:
				_refEntity.SetAnimateId(_refEntity.AnimateID + 2);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFX_SKL1_00, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				_refEntity.IgnoreGravity = true;
				SetAnimate((HumanBase.AnimateId)98u, (HumanBase.AnimateId)95u, (HumanBase.AnimateId)92u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				SetAnimate((HumanBase.AnimateId)99u, (HumanBase.AnimateId)96u, (HumanBase.AnimateId)93u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SetAnimate((HumanBase.AnimateId)100u, (HumanBase.AnimateId)97u, (HumanBase.AnimateId)94u);
				break;
			}
			break;
		}
	}

	private void SetAnimate(HumanBase.AnimateId crouch, HumanBase.AnimateId stand, HumanBase.AnimateId jump)
	{
		switch (_refEntity.AnimateID)
		{
		case HumanBase.AnimateId.ANI_CROUCH:
		case HumanBase.AnimateId.ANI_CROUCH_END:
		case (HumanBase.AnimateId)71u:
		case (HumanBase.AnimateId)72u:
		case (HumanBase.AnimateId)73u:
		case (HumanBase.AnimateId)80u:
		case (HumanBase.AnimateId)81u:
		case (HumanBase.AnimateId)82u:
		case (HumanBase.AnimateId)89u:
		case (HumanBase.AnimateId)90u:
			_refEntity.SetAnimateId(crouch);
			return;
		}
		if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
		{
			_refEntity.SetAnimateId(stand);
			return;
		}
		_refEntity.IgnoreGravity = true;
		_refEntity.SetAnimateId(jump);
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UpdateCustomWeaponRenderer(true);
			skillEventFrame = GameLogicUpdateManager.GameFrame + SKL1_START_END;
			endFrame = GameLogicUpdateManager.GameFrame + SKL1_START_END;
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			skl1Direction = _refEntity._characterDirection;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((_refEntity.CurrentActiveSkill == -1 || IsWaitingCombo(id)) && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UpdateCustomWeaponRenderer(true, true);
			_refEntity.CurrentActiveSkill = id;
			_refEntity.IsShoot = 1;
			OrangeCharacter.SubStatus subStatus = OrangeCharacter.SubStatus.SKILL0;
			switch (_refEntity.GetCurrentSkillObj().Reload_index)
			{
			default:
				skillEventFrame = GameLogicUpdateManager.GameFrame + SKL0_0_TRIGGER;
				endFrame = GameLogicUpdateManager.GameFrame + SKL0_0_START_END;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_0_START_END_BREAK;
				subStatus = OrangeCharacter.SubStatus.SKILL0;
				break;
			case 1:
				skillEventFrame = GameLogicUpdateManager.GameFrame + SKL0_1_TRIGGER;
				endFrame = GameLogicUpdateManager.GameFrame + SKL0_1_START_END;
				subStatus = OrangeCharacter.SubStatus.SKILL0_1;
				break;
			case 2:
				skillEventFrame = GameLogicUpdateManager.GameFrame + SKL0_2_TRIGGER;
				endFrame = GameLogicUpdateManager.GameFrame + SKL0_2_START_END;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_2_START_END_BREAK;
				subStatus = OrangeCharacter.SubStatus.SKILL0_2;
				break;
			}
			isSkillEventEnd = false;
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, subStatus);
		}
	}

	private bool IsWaitingCombo(int id)
	{
		if (id != 0)
		{
			return false;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 22) > 1u)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override void CheckSkill()
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				if (nowFrame >= endFrame)
				{
					_refEntity.CurrentActiveSkill = -1;
					OnSkillEnd(_refEntity.CurSubStatus + 3, SKL0_0_END_END);
				}
				else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					OnSkill_0_Trigger();
				}
				else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
				{
					endFrame = nowFrame + 1;
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (nowFrame >= endFrame)
				{
					_refEntity.CurrentActiveSkill = -1;
					OnSkillEnd(_refEntity.CurSubStatus + 3, SKL0_1_END_END);
				}
				else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					OnSkill_0_Trigger();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				if (nowFrame >= endFrame)
				{
					OnSkillEnd(_refEntity.CurSubStatus + 3, SKL0_2_END_END);
				}
				else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					OnSkill_0_Trigger();
				}
				else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
				{
					endFrame = nowFrame + 1;
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				if (nowFrame >= endFrame)
				{
					_refEntity.IgnoreGravity = false;
					isSkillEventEnd = false;
					_refEntity.SkillEnd = true;
					_refEntity.CurrentActiveSkill = -1;
					_refEntity.EnableCurrentWeapon();
					ResetToIdle();
				}
				else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
				{
					endFrame = nowFrame + 1;
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
			case OrangeCharacter.SubStatus.SKILL0_4:
				break;
			}
			break;
		case -1:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 22) <= 1u && nowFrame >= endFrame)
			{
				_refEntity.IgnoreGravity = false;
				isSkillEventEnd = false;
				_refEntity.SkillEnd = true;
				_refEntity.CurrentActiveSkill = -1;
				_refEntity.EnableCurrentWeapon();
				ResetToIdle();
			}
			break;
		}
		case 1:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				if (nowFrame >= endFrame)
				{
					if (_refEntity.Dashing)
					{
						_refEntity.PlayerStopDashing();
					}
					_refEntity.SetSpeed((int)skl1Direction * OrangeCharacter.WalkSpeed, (int)((float)OrangeCharacter.JumpSpeed * 0.5f));
					_refEntity.StopShootTimer();
					WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
					SKILL_TABLE bulletData = currentSkillObj.BulletData;
					_refEntity.SetSpeed((int)skl1Direction * (int)((float)OrangeCharacter.DashSpeed * 2.5f), 0);
					_refEntity.BulletCollider.UpdateBulletData(bulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
					_refEntity.BulletCollider.SetBulletAtk(currentSkillObj.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
					_refEntity.BulletCollider.BulletLevel = currentSkillObj.SkillLV;
					_refEntity.BulletCollider.Active(_refEntity.TargetMask);
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[_refEntity.CurrentActiveSkill]);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFX_SKL1_01, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
					OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
					isSkillEventEnd = false;
					skillEventFrame = GameLogicUpdateManager.GameFrame + SKL1_TRIGGER_LINK;
					OnSkillEnd(OrangeCharacter.SubStatus.SKILL1_1, SKL1_LOOP_END);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (nowFrame >= endFrame)
				{
					_refEntity.SetSpeed(0, 0);
					_refEntity.BulletCollider.BackToPool();
					endFrame = GameLogicUpdateManager.GameFrame + SKL1_END_END;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				}
				else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
				{
					Vector3 value = ((skl1Direction == CharacterDirection.LEFT) ? new Vector3(-1f, 0f, 0f) : new Vector3(1f, 0f, 0f));
					WeaponStruct currentSkillObj2 = _refEntity.GetCurrentSkillObj();
					isSkillEventEnd = true;
					if (linkSkl != null)
					{
						_refEntity.PushBulletDetail(linkSkl, currentSkillObj2.weaponStatus, shootPointTransform, currentSkillObj2.SkillLV, value);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (nowFrame >= endFrame)
				{
					endFrame = GameLogicUpdateManager.GameFrame + SKL1_END_END_BREAK;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				if (nowFrame >= endFrame)
				{
					_refEntity.IgnoreGravity = false;
					_refEntity.SkillEnd = true;
					_refEntity.CurrentActiveSkill = -1;
					_refEntity.EnableCurrentWeapon();
					isSkillEventEnd = false;
					ResetToIdle();
				}
				else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
				{
					endFrame = nowFrame + 1;
				}
				break;
			}
			break;
		}
	}

	private void UpdateCustomWeaponRenderer(bool enableWeapon, bool enableTrail = false)
	{
		SaberMesh_L_c.SetActive(enableWeapon);
		SaberMesh_R_c.SetActive(enableWeapon);
		saberTrail_L.Emit = enableTrail;
		saberTrail_R.Emit = enableTrail;
	}

	private void OnSkill_0_Trigger()
	{
		WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
		int reload_index = currentSkillObj.Reload_index;
		SKILL_TABLE sklTable = currentSkillObj.FastBulletDatas[reload_index];
		CreateSkill(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill], sklTable);
		_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[_refEntity.CurrentActiveSkill], null, reload_index);
		ResetReloadIndex(0, reload_index);
		isSkillEventEnd = true;
	}

	private void OnSkillEnd(OrangeCharacter.SubStatus nextSubStatus, int nextEndFrame)
	{
		_refEntity.IsShoot = 0;
		isSkillEventEnd = false;
		endFrame = GameLogicUpdateManager.GameFrame + nextEndFrame;
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, nextSubStatus);
	}

	private void CreateSkill(WeaponStruct weaponStruct, SKILL_TABLE sklTable)
	{
		_refEntity.FreshBullet = true;
		_refEntity.PushBulletDetail(sklTable, weaponStruct.weaponStatus, shootPointTransform, weaponStruct.SkillLV);
	}

	private void ResetReloadIndex(int currentSklId, int nowIdx)
	{
		if (nowIdx == RELOAD_INDEX_MAX)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[currentSklId];
			ComboCheckData[] comboCheckDatas = weaponStruct.ComboCheckDatas;
			for (int i = 0; i < comboCheckDatas.Length; i++)
			{
				_refEntity.RemoveComboSkillBuff(comboCheckDatas[i].nComboSkillID);
			}
			weaponStruct.Reload_index = 0;
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		}
	}

	private void ResetToIdle()
	{
		UpdateCustomWeaponRenderer(false);
		switch (_refEntity.AnimateID)
		{
		case (HumanBase.AnimateId)70u:
		case (HumanBase.AnimateId)79u:
		case (HumanBase.AnimateId)88u:
		case (HumanBase.AnimateId)97u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case (HumanBase.AnimateId)67u:
		case (HumanBase.AnimateId)76u:
		case (HumanBase.AnimateId)85u:
		case (HumanBase.AnimateId)94u:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)73u:
		case (HumanBase.AnimateId)82u:
		case (HumanBase.AnimateId)91u:
		case (HumanBase.AnimateId)100u:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.BulletCollider.BackToPool();
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			UpdateCustomWeaponRenderer(false);
			_refEntity.EnableCurrentWeapon();
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		if (petID != nBitID || (!_refEntity.IsLocalPlayer && nSetNumID == -1))
		{
			return;
		}
		if (nSetNumID == -1)
		{
			nMineCount++;
			if (nMineCount < 0)
			{
				nMineCount = 0;
			}
		}
		else
		{
			nMineCount = nSetNumID;
		}
		PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[nBitID];
		SCH004Controller poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SCH004Controller>(pET_TABLE.s_MODEL);
		poolObj.PetID = nBitID;
		poolObj.SetOwner(_refEntity, nMineCount);
		poolObj.SetParams(pET_TABLE.s_MODEL, nBitTime, 0, null, 0L);
		poolObj.SetPositionAndRotation(_refEntity._transform.position + Vector3.up, false);
		poolObj.UseSignedAngle = true;
		poolObj.activeSE = new string[4] { "BattleSE02", "bt_bit01", "", "0.5" };
		poolObj.unactiveSE = new string[2] { "BattleSE02", "bt_bit02" };
		poolObj.boomSE = new string[2] { "HiSE", "ht_electric" };
		if (nSetNumID == -1)
		{
			StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + nMineCount, true);
		}
		PlayPetSE();
		poolObj.SetActive(true);
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(poolObj);
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) <= 3u)
		{
			_refEntity._characterDirection = skl1Direction;
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
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

	public bool CheckPetActive(int petId)
	{
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
			else if (_liPets[num].PetID == petId)
			{
				return true;
			}
		}
		return false;
	}

	public override string GetTeleportInExtraEffect()
	{
		return sFxuseCutIn;
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[36]
		{
			"ch060_skill_02_jump_step1_start", "ch060_skill_02_jump_step1_loop", "ch060_skill_02_jump_step1_end", "ch060_skill_02_stand_step1_start", "ch060_skill_02_stand_step1_loop", "ch060_skill_02_stand_step1_end", "ch060_skill_02_crouch_step1_start", "ch060_skill_02_crouch_step1_loop", "ch060_skill_02_crouch_step1_end", "ch060_skill_02_jump_step2_start",
			"ch060_skill_02_jump_step2_loop", "ch060_skill_02_jump_step2_end", "ch060_skill_02_stand_step2_start", "ch060_skill_02_stand_step2_loop", "ch060_skill_02_stand_step2_end", "ch060_skill_02_crouch_step2_start", "ch060_skill_02_crouch_step2_loop", "ch060_skill_02_crouch_step2_end", "ch060_skill_02_jump_step3_start", "ch060_skill_02_jump_step3_loop",
			"ch060_skill_02_jump_step3_end", "ch060_skill_02_stand_step3_start", "ch060_skill_02_stand_step3_loop", "ch060_skill_02_stand_step3_end", "ch060_skill_02_crouch_step3_start", "ch060_skill_02_crouch_step3_loop", "ch060_skill_02_crouch_step3_end", "ch060_skill_01_jump_start", "ch060_skill_01_jump_loop", "ch060_skill_01_jump_end",
			"ch060_skill_01_stand_start", "ch060_skill_01_stand_loop", "ch060_skill_01_stand_end", "ch060_skill_01_crouch_start", "ch060_skill_01_crouch_loop", "ch060_skill_01_crouch_end"
		};
	}
}
