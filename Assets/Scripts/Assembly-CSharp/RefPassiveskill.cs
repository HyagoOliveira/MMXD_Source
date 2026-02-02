using System;
using System.Collections.Generic;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;
using enums;

public class RefPassiveskill
{
	public class PassiveskillStatus
	{
		public SKILL_TABLE tSKILL_TABLE;

		public SKILL_TABLE tCACHE_SKILL_TABLE;

		public float fLastTriggerTime = -999f;

		public int nWeaponCheck = 4;

		public int nSkillLevel = 1;

		public bool bCheckUse;

		public bool bIsChipSkill;

		public int nUseCount;

		public int nExtendCount;

		public float fExReloadTime = -999f;

		public float fLastKeyTriggerTime = -999f;

		private bool bNeedCD;

		public float GetNowLeftTime(float fNowTime)
		{
			return (fNowTime - fLastTriggerTime) * 1000f;
		}

		public void UpdateUseTime(float fNowTime)
		{
			fLastTriggerTime = fNowTime;
			nUseCount++;
			bNeedCD = true;
		}

		public float GetCDPercent(float fNowTime)
		{
			if (!bNeedCD)
			{
				return 1f;
			}
			float nowLeftTime = GetNowLeftTime(fNowTime);
			if (tSKILL_TABLE.n_MAGAZINE > 0)
			{
				if (nUseCount >= tSKILL_TABLE.n_MAGAZINE)
				{
					if (nowLeftTime >= (float)tSKILL_TABLE.n_RELOAD)
					{
						bNeedCD = false;
						return 1f;
					}
					return nowLeftTime / (float)tSKILL_TABLE.n_RELOAD;
				}
				return 1f;
			}
			if (nowLeftTime < (float)tSKILL_TABLE.n_RELOAD)
			{
				return nowLeftTime / (float)tSKILL_TABLE.n_RELOAD;
			}
			bNeedCD = false;
			return 1f;
		}

		public float GetUseCDPercent(float fNowTime)
		{
			if (!bNeedCD)
			{
				return 1f;
			}
			float nowLeftTime = GetNowLeftTime(fNowTime);
			if (tSKILL_TABLE.n_MAGAZINE > 0)
			{
				if (nUseCount >= tSKILL_TABLE.n_MAGAZINE)
				{
					if (nowLeftTime >= (float)tSKILL_TABLE.n_RELOAD)
					{
						bNeedCD = false;
						return 1f;
					}
					return nowLeftTime / (float)tSKILL_TABLE.n_RELOAD;
				}
				if (nowLeftTime < (float)tSKILL_TABLE.n_FIRE_SPEED)
				{
					return nowLeftTime / (float)tSKILL_TABLE.n_FIRE_SPEED;
				}
			}
			else if (nowLeftTime < (float)tSKILL_TABLE.n_RELOAD)
			{
				return nowLeftTime / (float)tSKILL_TABLE.n_RELOAD;
			}
			bNeedCD = false;
			return 1f;
		}

		public void CopyTo(PassiveskillStatus target)
		{
			target.tSKILL_TABLE = tSKILL_TABLE;
			target.fLastTriggerTime = fLastTriggerTime;
			target.nWeaponCheck = nWeaponCheck;
			target.nSkillLevel = nSkillLevel;
			target.bCheckUse = bCheckUse;
			target.bIsChipSkill = bIsChipSkill;
			target.nUseCount = nUseCount;
			target.nExtendCount = nExtendCount;
			target.fExReloadTime = fExReloadTime;
			target.fLastKeyTriggerTime = fLastKeyTriggerTime;
			target.bNeedCD = bNeedCD;
		}

		public bool CheckCanUse(bool bUsePassiveskill, int inWeaponCheck1, int inWeaponCheck2, int nSkillID, float fNowTime)
		{
			if (tSKILL_TABLE.n_ID != Singleton<CrusadeSystem>.Instance.PassiveSkillID && (nWeaponCheck & inWeaponCheck1) == 0)
			{
				return false;
			}
			if (!bUsePassiveskill && bCheckUse)
			{
				return false;
			}
			switch (tSKILL_TABLE.n_TRIGGER)
			{
			default:
				if (tSKILL_TABLE.n_TRIGGER_X != 0 && (tSKILL_TABLE.n_TRIGGER_X & inWeaponCheck2) == 0)
				{
					return false;
				}
				if (tSKILL_TABLE.n_TRIGGER_Y != 0 && tSKILL_TABLE.n_TRIGGER_Y != nSkillID)
				{
					return false;
				}
				break;
			case 3:
			case 4:
			case 5:
			case 6:
			case 8:
			case 10:
			case 15:
			case 16:
			case 18:
			case 19:
			case 20:
			case 22:
			case 23:
			case 24:
			case 25:
			case 26:
			case 27:
			case 28:
			case 29:
			case 30:
			case 31:
				break;
			}
			if (tSKILL_TABLE.n_MAGAZINE > 0)
			{
				if (nUseCount >= tSKILL_TABLE.n_MAGAZINE)
				{
					if (!(GetNowLeftTime(fNowTime) >= (float)tSKILL_TABLE.n_RELOAD))
					{
						return false;
					}
					nUseCount = 0;
				}
				else if (GetNowLeftTime(fNowTime) < (float)tSKILL_TABLE.n_FIRE_SPEED)
				{
					return false;
				}
			}
			else if (GetNowLeftTime(fNowTime) < (float)tSKILL_TABLE.n_RELOAD)
			{
				return false;
			}
			if (tSKILL_TABLE.n_TRIGGER == 21 && tSKILL_TABLE.n_EFFECT == 23 && GetNowLeftTime(fNowTime) >= (float)tSKILL_TABLE.n_RELOAD && fExReloadTime < fNowTime)
			{
				fExReloadTime = fNowTime;
				nExtendCount = 0;
			}
			return true;
		}
	}

	public enum SKILL_STATUS_ADD_TYPE
	{
		none = 0,
		HP = 1,
		ATK = 2,
		DEF = 3,
		CRI = 4,
		RCRI = 5,
		CRIDMG = 6,
		RCRIDMG = 7,
		HIT = 8,
		DODGE = 9,
		MAX_NUM = 10
	}

	public enum TYPE101_TRIGGER_DESC
	{
		none = 0,
		USE_RIGHT_NOW = 1,
		HIT_RIGHT_NOW = 2,
		EQUIP_WPCONDITION_BUFF = 3,
		HURT_RIGHT_NOW = 4,
		EQUIP_HPCONDITION_BUFF = 5,
		EQUIP_BUFFCHECK_BUFF = 6,
		NO_WEAPON_ENERGY_RIGHT_NOW = 7,
		WEAPON_ENERGY_RIGHT_NOW = 8,
		SKILL_KILL_RIGHT_NOW = 9,
		MEASUREVALUE_RIGHT_NOW = 10,
		BULLET_END = 11,
		USE_SKILL_AND_HIT_SP_STATUS = 15,
		TRIGGER_PER_SECOND = 16,
		TRIGGER_WEAPON_ATTACK_RIGHT_NOW = 18,
		TRIGGER_HIT_TARGET_TYPE_RIGHT_NOW = 19,
		TRIGGER_RESISTANCE_TARGET_TYPE_RIGHT_NOW = 20,
		HIT_EX_RIGHT_NOW = 21,
		HIT_EX2_RIGHT_NOW = 22,
		HIT_EX3_RIGHT_NOW = 23,
		USE_KEY_RIGHT_NOW = 24,
		HIT_EX4_RIGHT_NOW = 25,
		TRIGGER_CALU_DMG = 26,
		TRIGGER_DEL_BUFF_NOW = 27,
		TRIGGER_PET_ON_STAGE = 28,
		TRIGGER_PET_DEACTIVE = 29,
		TRIGGER_LINE_SKILL = 30,
		HIT_HAVE_BUFF_TARGET = 31,
		HIT_EX5_RIGHT_NOW = 32
	}

	public enum PS_NEFFECT_CHECK
	{
		none = 0,
		NORMAL_DMG = 1,
		NORMAL_HEAL = 2,
		ADDSTATUS = 3,
		CDSKILL = 4,
		ADDITIONAL_DMG_BUFF = 5,
		ADDITIONAL_REDMG_BUFF = 6,
		ADDMAGAZINE = 7,
		ADDMEASUREVALUE = 8,
		ADDBATTLESCORE = 10,
		DMG_BYMAXHP = 12,
		DMG_BYMAXHPX = 13,
		DMG_DISREGARD_SKILL = 14,
		CALL_PET = 16,
		DEL_BUFF_OP = 18,
		DEL_BUFF_N = 19,
		DEL_DEBUFF = 20,
		PREVENT_DEBUFF = 21,
		STEAL_BUFF = 23,
		CATCH_PLAYER = 24,
		RATIO_STATUS = 25,
		ADD_STAGE_TIME = 26,
		FLY_MODE = 27,
		APPLY_BUFF_STATUS = 28,
		BUFF_STACK_DOWN = 29,
		DMG_BYMAXHP_HOLD_BACK = 30,
		CDSKILL_EX = 31,
		ADD_FS_MAGAZINE = 101,
		STAGE_REWARD_BOOST = 102
	}

	public enum PS_NEFFECT102_TYPE
	{
		EXP_BOOST = 1,
		DROP_BOOST = 2,
		MATURITY_BOOST = 3,
		MONEY_BOOST = 4
	}

	public enum PS_USE_TYPE
	{
		PASSIVE_SKILL = 101,
		PARAMS_REPLACE = 102,
		SPECIAL_COUNT = 103,
		PREVENT_BUFF = 104
	}

	public List<PassiveskillStatus> listPassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listHitPassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listUsePassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listEquipPassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listHurtPassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listAddStatusPassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listPerventbuffPassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listPerSecPassiveSkill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listRatioStatusPassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listUseKeyPassiveskill = new List<PassiveskillStatus>();

	public List<PassiveskillStatus> listBuffPassiveskill = new List<PassiveskillStatus>();

	public bool bUsePassiveskill = true;

	public PassiveskillStatus tMainPassiveskillStatus;

	public PassiveskillStatus tSubPassiveskillStatus;

	public int nMeasureMaxValue;

	public int nMeasureInitValue;

	public int nAddMeasureTime;

	private List<int> randomIndexList = new List<int>();

	public float fNowTotalLeftTime;

	private int[][] nTypeAddStatus;

	private int[][] nTypeRatioStatus;

	private const int nStatusNum = 7;

	public PassiveskillStatus AddPassivesSkill(int nID, int tWeaponCheck = 65535, int nSetSkillLV = 1, bool bCheckUse = false, bool bIsChipSkill = false)
	{
		PassiveskillStatus result = null;
		SKILL_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nID, out value))
		{
			if (value.n_USE_TYPE == 102)
			{
				result = AddPassivesSkillToList(ref listPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
			}
			else if (value.n_USE_TYPE == 101)
			{
				switch (value.n_EFFECT)
				{
				case 5:
					result = AddPassivesSkillToList(ref listEquipPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				case 6:
					result = AddPassivesSkillToList(ref listEquipPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				}
				switch (value.n_TRIGGER)
				{
				case 1:
				case 7:
				case 8:
				case 10:
				case 18:
					result = AddPassivesSkillToList(ref listUsePassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				case 2:
				case 6:
				case 9:
				case 11:
				case 15:
				case 21:
				case 22:
				case 23:
				case 25:
				case 31:
				case 32:
					result = AddPassivesSkillToList(ref listHitPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				case 4:
					result = AddPassivesSkillToList(ref listHurtPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				case 16:
					result = AddPassivesSkillToList(ref listPerSecPassiveSkill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				case 19:
				case 20:
				case 28:
				case 29:
				case 30:
					result = AddPassivesSkillToList(ref listEquipPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				case 24:
					result = AddPassivesSkillToList(ref listUseKeyPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				case 27:
					result = AddPassivesSkillToList(ref listBuffPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
					break;
				case 0:
					switch (value.n_EFFECT)
					{
					case 3:
						result = AddPassivesSkillToList(ref listAddStatusPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
						AddStatus(value, nSetSkillLV, tWeaponCheck);
						break;
					case 7:
						result = AddPassivesSkillToList(ref listHitPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
						break;
					case 14:
						result = AddPassivesSkillToList(ref listHurtPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
						break;
					case 25:
						result = AddPassivesSkillToList(ref listRatioStatusPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
						RatioStatus(value, nSetSkillLV, 64);
						break;
					case 27:
						result = AddPassivesSkillToList(ref listAddStatusPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
						break;
					}
					break;
				}
			}
			else if (value.n_USE_TYPE == 103)
			{
				result = AddPassivesSkillToList(ref listAddStatusPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
				nMeasureMaxValue = value.n_MAGAZINE;
				nMeasureInitValue = value.n_RELOAD / 1000;
				if (nMeasureInitValue > nMeasureMaxValue)
				{
					nMeasureInitValue = nMeasureMaxValue;
				}
				nAddMeasureTime = value.n_FIRE_SPEED;
			}
			else if (value.n_USE_TYPE == 104)
			{
				result = AddPassivesSkillToList(ref listPerventbuffPassiveskill, value, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
			}
			if (value.n_MOTION_DEF != 0 && !haveAddedPassiveSkill(value.n_MOTION_DEF))
			{
				AddPassivesSkill(value.n_MOTION_DEF, tWeaponCheck, nSetSkillLV, bCheckUse, bIsChipSkill);
			}
		}
		return result;
	}

	private bool haveAddedPassiveSkill(int skillId)
	{
		if (listPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listHitPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listUsePassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listEquipPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listHurtPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listAddStatusPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listPerventbuffPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listPerSecPassiveSkill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listRatioStatusPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listUseKeyPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		if (listBuffPassiveskill.Find((PassiveskillStatus x) => x.tSKILL_TABLE.n_ID == skillId) != null)
		{
			return true;
		}
		return false;
	}

	private void ResetStatus(bool bReset = false, bool bResetRatio = false)
	{
		if (nTypeAddStatus == null)
		{
			nTypeAddStatus = new int[10][];
			for (int i = 0; i < 10; i++)
			{
				nTypeAddStatus[i] = new int[7];
			}
		}
		else if (bReset)
		{
			for (int j = 0; j < 10; j++)
			{
				for (int k = 0; k < 7; k++)
				{
					nTypeAddStatus[j][k] = 0;
				}
			}
		}
		if (nTypeRatioStatus == null)
		{
			nTypeRatioStatus = new int[10][];
			for (int l = 0; l < 10; l++)
			{
				nTypeRatioStatus[l] = new int[7];
			}
		}
		else
		{
			if (!bResetRatio)
			{
				return;
			}
			for (int m = 0; m < 10; m++)
			{
				for (int n = 0; n < 7; n++)
				{
					nTypeRatioStatus[m][n] = 0;
				}
			}
		}
	}

	public void TriggerBuff(SKILL_TABLE tSKILL_TABLE, PerBuffManager tTargetBM, PerBuffManager tShoterBM, int nAtkParam = 0, int nMaxHP = 0, int nSkillID = 0, bool bNetAdd = false)
	{
		if (tSKILL_TABLE.n_CONDITION_ID == 0 || tSKILL_TABLE.n_CONDITION_RATE < OrangeBattleUtility.Random(0, 10000))
		{
			return;
		}
		string sAdderID = "";
		if (tShoterBM != null)
		{
			sAdderID = tShoterBM.SOB.sNetSerialID;
		}
		if (tSKILL_TABLE.n_CONDITION_TARGET == 0)
		{
			if (tTargetBM != null)
			{
				tTargetBM.AddBuff(tSKILL_TABLE.n_CONDITION_ID, nAtkParam, nMaxHP, nSkillID, bNetAdd, sAdderID, 4);
			}
		}
		else if (tSKILL_TABLE.n_CONDITION_TARGET == 1 && tShoterBM != null)
		{
			tShoterBM.AddBuff(tSKILL_TABLE.n_CONDITION_ID, nAtkParam, nMaxHP, nSkillID, bNetAdd, sAdderID, 4);
		}
	}

	private void AddStatus(SKILL_TABLE tSKILL_TABLE, int nSkillLV, int nWeaponCheck)
	{
		ResetStatus();
		int num = (int)tSKILL_TABLE.f_EFFECT_X;
		int num2 = (int)tSKILL_TABLE.f_EFFECT_Y + (int)tSKILL_TABLE.f_EFFECT_Z * nSkillLV;
		for (int i = 0; i < 6; i++)
		{
			if ((nWeaponCheck & (1 << i)) != 0)
			{
				nTypeAddStatus[num][i] += num2;
			}
		}
		if (nWeaponCheck == 64)
		{
			nTypeAddStatus[num][6] += num2;
		}
	}

	public int GetAddStatus(int nType, int nWType)
	{
		ResetStatus();
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if ((nWType & (1 << i)) != 0)
			{
				num += nTypeAddStatus[nType][i];
			}
		}
		return num + nTypeAddStatus[nType][6];
	}

	private void RatioStatus(SKILL_TABLE tSKILL_TABLE, int nSkillLV, int nWeaponCheck)
	{
		ResetStatus();
		int num = (int)tSKILL_TABLE.f_EFFECT_X;
		int num2 = (int)tSKILL_TABLE.f_EFFECT_Y + (int)tSKILL_TABLE.f_EFFECT_Z * nSkillLV;
		for (int i = 0; i < 6; i++)
		{
			if ((nWeaponCheck & (1 << i)) != 0)
			{
				nTypeRatioStatus[num][i] += num2;
			}
		}
		if (nWeaponCheck == 64)
		{
			nTypeRatioStatus[num][6] += num2;
		}
	}

	public float GetRatioStatus(int nType, int nWType)
	{
		ResetStatus();
		int num = 100;
		for (int i = 0; i < 6; i++)
		{
			if ((nWType & (1 << i)) != 0)
			{
				num += nTypeRatioStatus[nType][i];
			}
		}
		num += nTypeRatioStatus[nType][6];
		if (num > 100 + OrangeConst.ATTRIBUTE_BOOST_LIMIT)
		{
			num = 100 + OrangeConst.ATTRIBUTE_BOOST_LIMIT;
		}
		else if (num < 0)
		{
			num = 0;
		}
		return (float)num * 0.01f;
	}

	public float GetWeaponChipBulletPercent(int nWeaponCheck)
	{
		switch (nWeaponCheck)
		{
		case 4:
			if (tMainPassiveskillStatus == null)
			{
				break;
			}
			if (tMainPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE > 0)
			{
				if (tMainPassiveskillStatus.nUseCount >= tMainPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE)
				{
					float num2 = tMainPassiveskillStatus.GetNowLeftTime(fNowTotalLeftTime) / (float)tMainPassiveskillStatus.tSKILL_TABLE.n_RELOAD;
					if (num2 >= 1f)
					{
						tMainPassiveskillStatus.nUseCount = 0;
						return 1f;
					}
					return num2;
				}
				return (float)(tMainPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE - tMainPassiveskillStatus.nUseCount) / (float)tMainPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE;
			}
			return 0f;
		case 8:
			if (tSubPassiveskillStatus == null)
			{
				break;
			}
			if (tSubPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE > 0)
			{
				if (tSubPassiveskillStatus.nUseCount >= tSubPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE)
				{
					float num = tSubPassiveskillStatus.GetNowLeftTime(fNowTotalLeftTime) / (float)tSubPassiveskillStatus.tSKILL_TABLE.n_RELOAD;
					if (num >= 1f)
					{
						tSubPassiveskillStatus.nUseCount = 0;
						return 1f;
					}
					return num;
				}
				return (float)(tSubPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE - tSubPassiveskillStatus.nUseCount) / (float)tSubPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE;
			}
			return 0f;
		}
		return 0f;
	}

	public float GetWeaponChipSkillCD(int nWeaponCheck)
	{
		switch (nWeaponCheck)
		{
		case 4:
			if (tMainPassiveskillStatus != null)
			{
				return tMainPassiveskillStatus.GetCDPercent(fNowTotalLeftTime);
			}
			break;
		case 8:
			if (tSubPassiveskillStatus != null)
			{
				return tSubPassiveskillStatus.GetCDPercent(fNowTotalLeftTime);
			}
			break;
		}
		return 0f;
	}

	public float GetWeaponChipUseCD(int nWeaponCheck)
	{
		switch (nWeaponCheck)
		{
		case 4:
			if (tMainPassiveskillStatus != null)
			{
				return tMainPassiveskillStatus.GetUseCDPercent(fNowTotalLeftTime);
			}
			break;
		case 8:
			if (tSubPassiveskillStatus != null)
			{
				return tSubPassiveskillStatus.GetUseCDPercent(fNowTotalLeftTime);
			}
			break;
		}
		return 0f;
	}

	private PassiveskillStatus AddPassivesSkillToList(ref List<PassiveskillStatus> tListPassiveSkill, SKILL_TABLE tSKILL_TABLE, int tWeaponCheck, int nSetSkillLV, bool bCheckUse, bool bIsChipSkill)
	{
		for (int i = 0; i < tListPassiveSkill.Count; i++)
		{
			if (tListPassiveSkill[i].tSKILL_TABLE.n_ID == tSKILL_TABLE.n_ID)
			{
				tListPassiveSkill[i].nWeaponCheck |= tWeaponCheck;
				return tListPassiveSkill[i];
			}
		}
		PassiveskillStatus passiveskillStatus = new PassiveskillStatus();
		passiveskillStatus.tSKILL_TABLE = tSKILL_TABLE;
		passiveskillStatus.nWeaponCheck = tWeaponCheck;
		passiveskillStatus.nSkillLevel = nSetSkillLV;
		passiveskillStatus.bCheckUse = bCheckUse;
		passiveskillStatus.bIsChipSkill = bIsChipSkill;
		tListPassiveSkill.Add(passiveskillStatus);
		return passiveskillStatus;
	}

	public void AddPassivesSkill(NetCharacterSkillInfo tNetCharacterSkillInfo, int tWeaponCheck = 16777215, bool bCheckUse = false)
	{
		CHARACTER_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(tNetCharacterSkillInfo.CharacterID, out value))
		{
			return;
		}
		switch (tNetCharacterSkillInfo.Slot)
		{
		case 1:
			switch ((CharacterSkillEnhanceSlot)tNetCharacterSkillInfo.Extra)
			{
			case CharacterSkillEnhanceSlot.EX_SKILL1:
				AddPassivesSkill(value.n_SKILL1_EX1, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL2:
				AddPassivesSkill(value.n_SKILL1_EX2, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL3:
				AddPassivesSkill(value.n_SKILL1_EX3, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
				break;
			}
			break;
		case 2:
			switch ((CharacterSkillEnhanceSlot)tNetCharacterSkillInfo.Extra)
			{
			case CharacterSkillEnhanceSlot.EX_SKILL1:
				AddPassivesSkill(value.n_SKILL2_EX1, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL2:
				AddPassivesSkill(value.n_SKILL2_EX2, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL3:
				AddPassivesSkill(value.n_SKILL2_EX3, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
				break;
			}
			break;
		case 3:
			AddPassivesSkill(value.n_PASSIVE_1, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
			break;
		case 4:
			AddPassivesSkill(value.n_PASSIVE_2, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
			break;
		case 5:
			AddPassivesSkill(value.n_PASSIVE_3, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
			break;
		case 6:
			AddPassivesSkill(value.n_PASSIVE_4, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
			break;
		case 7:
			AddPassivesSkill(value.n_PASSIVE_5, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
			break;
		case 8:
			AddPassivesSkill(value.n_PASSIVE_6, tWeaponCheck, tNetCharacterSkillInfo.Level, bCheckUse);
			break;
		}
	}

	public void AddPassivesSkill(NetWeaponSkillInfo tNetWeaponSkillInfo, int tWeaponCheck = 16777215, int nSetSkillLV = 1, bool bCheckUse = false)
	{
		WEAPON_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(tNetWeaponSkillInfo.WeaponID, out value))
		{
			switch (tNetWeaponSkillInfo.Slot)
			{
			case 1:
				AddPassivesSkill(value.n_PASSIVE_1, tWeaponCheck, nSetSkillLV, bCheckUse);
				break;
			case 2:
				AddPassivesSkill(value.n_PASSIVE_2, tWeaponCheck, nSetSkillLV, bCheckUse);
				break;
			case 3:
				AddPassivesSkill(value.n_PASSIVE_3, tWeaponCheck, nSetSkillLV, bCheckUse);
				break;
			case 4:
				AddPassivesSkill(value.n_PASSIVE_4, tWeaponCheck, nSetSkillLV, bCheckUse);
				break;
			case 5:
				AddPassivesSkill(value.n_PASSIVE_5, tWeaponCheck, nSetSkillLV, bCheckUse);
				break;
			case 6:
				AddPassivesSkill(value.n_PASSIVE_6, tWeaponCheck, nSetSkillLV, bCheckUse);
				break;
			}
		}
	}

	public void AddPassivesSkill(NetChipInfo tNetChipInfo, int tWeaponCheck = 16777215, int nSetSkillLV = 1, bool bCheckUse = false)
	{
		DISC_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(tNetChipInfo.ChipID, out value))
		{
			return;
		}
		int[] array = new int[7] { value.n_SKILL_0, value.n_SKILL_1, value.n_SKILL_2, value.n_SKILL_3, value.n_SKILL_4, value.n_SKILL_5, value.n_SKILL_6 };
		if (tNetChipInfo.Star < 0 || tNetChipInfo.Star >= array.Length)
		{
			UnityEngine.Debug.LogError("Chip Start Error : " + tNetChipInfo.Star);
			return;
		}
		if (tWeaponCheck == 4)
		{
			tMainPassiveskillStatus = AddPassivesSkill(array[tNetChipInfo.Star], tWeaponCheck, nSetSkillLV, bCheckUse, true);
		}
		if (tWeaponCheck == 8)
		{
			tSubPassiveskillStatus = AddPassivesSkill(array[tNetChipInfo.Star], tWeaponCheck, nSetSkillLV, bCheckUse, true);
		}
	}

	public void RemovePassiveSkill(int nID)
	{
		for (int num = listPassiveskill.Count - 1; num >= 0; num--)
		{
			if (listPassiveskill[num].tSKILL_TABLE.n_ID == nID)
			{
				listPassiveskill.RemoveAt(num);
			}
		}
		for (int num2 = listHitPassiveskill.Count - 1; num2 >= 0; num2--)
		{
			if (listHitPassiveskill[num2].tSKILL_TABLE.n_ID == nID)
			{
				listHitPassiveskill.RemoveAt(num2);
			}
		}
		for (int num3 = listUsePassiveskill.Count - 1; num3 >= 0; num3--)
		{
			if (listUsePassiveskill[num3].tSKILL_TABLE.n_ID == nID)
			{
				listUsePassiveskill.RemoveAt(num3);
			}
		}
		for (int num4 = listEquipPassiveskill.Count - 1; num4 >= 0; num4--)
		{
			if (listEquipPassiveskill[num4].tSKILL_TABLE.n_ID == nID)
			{
				listEquipPassiveskill.RemoveAt(num4);
			}
		}
		for (int num5 = listHurtPassiveskill.Count - 1; num5 >= 0; num5--)
		{
			if (listHurtPassiveskill[num5].tSKILL_TABLE.n_ID == nID)
			{
				listHurtPassiveskill.RemoveAt(num5);
			}
		}
		for (int num6 = listAddStatusPassiveskill.Count - 1; num6 >= 0; num6--)
		{
			if (listAddStatusPassiveskill[num6].tSKILL_TABLE.n_ID == nID)
			{
				listAddStatusPassiveskill.RemoveAt(num6);
			}
		}
		for (int num7 = listPerventbuffPassiveskill.Count - 1; num7 >= 0; num7--)
		{
			if (listPerventbuffPassiveskill[num7].tSKILL_TABLE.n_ID == nID)
			{
				listPerventbuffPassiveskill.RemoveAt(num7);
			}
		}
		for (int num8 = listPerSecPassiveSkill.Count - 1; num8 >= 0; num8--)
		{
			if (listPerSecPassiveSkill[num8].tSKILL_TABLE.n_ID == nID)
			{
				listPerSecPassiveSkill.RemoveAt(num8);
			}
		}
		for (int num9 = listRatioStatusPassiveskill.Count - 1; num9 >= 0; num9--)
		{
			if (listRatioStatusPassiveskill[num9].tSKILL_TABLE.n_ID == nID)
			{
				listRatioStatusPassiveskill.RemoveAt(num9);
			}
		}
		for (int num10 = listUseKeyPassiveskill.Count - 1; num10 >= 0; num10--)
		{
			if (listUseKeyPassiveskill[num10].tSKILL_TABLE.n_ID == nID)
			{
				listUseKeyPassiveskill.RemoveAt(num10);
			}
		}
		for (int num11 = listBuffPassiveskill.Count - 1; num11 >= 0; num11--)
		{
			if (listBuffPassiveskill[num11].tSKILL_TABLE.n_ID == nID)
			{
				listBuffPassiveskill.RemoveAt(num11);
			}
		}
	}

	public void ResetAllPassiveskillData()
	{
		for (int i = 0; i < listHitPassiveskill.Count; i++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listHitPassiveskill[i].tSKILL_TABLE.n_ID, out listHitPassiveskill[i].tSKILL_TABLE);
		}
		for (int j = 0; j < listUsePassiveskill.Count; j++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listUsePassiveskill[j].tSKILL_TABLE.n_ID, out listUsePassiveskill[j].tSKILL_TABLE);
		}
		for (int k = 0; k < listEquipPassiveskill.Count; k++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listEquipPassiveskill[k].tSKILL_TABLE.n_ID, out listEquipPassiveskill[k].tSKILL_TABLE);
		}
		for (int l = 0; l < listHurtPassiveskill.Count; l++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listHurtPassiveskill[l].tSKILL_TABLE.n_ID, out listHurtPassiveskill[l].tSKILL_TABLE);
		}
		for (int m = 0; m < listAddStatusPassiveskill.Count; m++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listAddStatusPassiveskill[m].tSKILL_TABLE.n_ID, out listAddStatusPassiveskill[m].tSKILL_TABLE);
		}
		for (int n = 0; n < listPerventbuffPassiveskill.Count; n++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listPerventbuffPassiveskill[n].tSKILL_TABLE.n_ID, out listPerventbuffPassiveskill[n].tSKILL_TABLE);
		}
		for (int num = 0; num < listPerSecPassiveSkill.Count; num++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listPerSecPassiveSkill[num].tSKILL_TABLE.n_ID, out listPerSecPassiveSkill[num].tSKILL_TABLE);
		}
		for (int num2 = 0; num2 < listRatioStatusPassiveskill.Count; num2++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listRatioStatusPassiveskill[num2].tSKILL_TABLE.n_ID, out listRatioStatusPassiveskill[num2].tSKILL_TABLE);
		}
		for (int num3 = 0; num3 < listUseKeyPassiveskill.Count; num3++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listUseKeyPassiveskill[num3].tSKILL_TABLE.n_ID, out listUseKeyPassiveskill[num3].tSKILL_TABLE);
		}
		for (int num4 = 0; num4 < listBuffPassiveskill.Count; num4++)
		{
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(listBuffPassiveskill[num4].tSKILL_TABLE.n_ID, out listBuffPassiveskill[num4].tSKILL_TABLE);
		}
	}

	public void ReCalcuPassiveskillSelf()
	{
		for (int i = 0; i < listHitPassiveskill.Count; i++)
		{
			ReCalcuSkill(ref listHitPassiveskill[i].tSKILL_TABLE);
		}
		for (int j = 0; j < listUsePassiveskill.Count; j++)
		{
			ReCalcuSkill(ref listUsePassiveskill[j].tSKILL_TABLE);
		}
		for (int k = 0; k < listEquipPassiveskill.Count; k++)
		{
			ReCalcuSkill(ref listEquipPassiveskill[k].tSKILL_TABLE);
		}
		for (int l = 0; l < listHurtPassiveskill.Count; l++)
		{
			ReCalcuSkill(ref listHurtPassiveskill[l].tSKILL_TABLE);
		}
		for (int m = 0; m < listAddStatusPassiveskill.Count; m++)
		{
			ReCalcuSkill(ref listAddStatusPassiveskill[m].tSKILL_TABLE);
		}
		for (int n = 0; n < listPerSecPassiveSkill.Count; n++)
		{
			ReCalcuSkill(ref listPerSecPassiveSkill[n].tSKILL_TABLE);
		}
		for (int num = 0; num < listRatioStatusPassiveskill.Count; num++)
		{
			ReCalcuSkill(ref listRatioStatusPassiveskill[num].tSKILL_TABLE);
		}
		for (int num2 = 0; num2 < listUseKeyPassiveskill.Count; num2++)
		{
			ReCalcuSkill(ref listUseKeyPassiveskill[num2].tSKILL_TABLE);
		}
		for (int num3 = 0; num3 < listBuffPassiveskill.Count; num3++)
		{
			ReCalcuSkill(ref listBuffPassiveskill[num3].tSKILL_TABLE);
		}
	}

	public static void RecalcuSkillBySkill(ref SKILL_TABLE tSKILL_TABLE, SKILL_TABLE refSKILL_TABLE)
	{
		if (refSKILL_TABLE.n_USE_TYPE != 102 || refSKILL_TABLE.n_TRIGGER_X != tSKILL_TABLE.n_ID)
		{
			return;
		}
		SKILL_TABLE sKILL_TABLE = new SKILL_TABLE();
		PropertyInfo[] properties = refSKILL_TABLE.GetType().GetProperties();
		foreach (PropertyInfo obj in properties)
		{
			object value = obj.GetValue(tSKILL_TABLE, null);
			obj.SetValue(sKILL_TABLE, value, null);
		}
		properties = refSKILL_TABLE.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.Name == "n_ID" || propertyInfo.Name == "n_LVMAX" || propertyInfo.Name == "n_USE_TYPE" || propertyInfo.Name == "n_TRIGGER_X" || propertyInfo.Name == "s_NAME" || propertyInfo.Name == "s_ICON" || propertyInfo.Name == "s_SHOWCASE" || propertyInfo.Name == "w_NAME" || propertyInfo.Name == "w_TIP")
			{
				continue;
			}
			if (propertyInfo.PropertyType == typeof(int))
			{
				int num = (int)propertyInfo.GetValue(refSKILL_TABLE, null);
				if (num != 0)
				{
					propertyInfo.SetValue(sKILL_TABLE, num, null);
				}
			}
			else if (propertyInfo.PropertyType == typeof(float))
			{
				float num2 = (float)propertyInfo.GetValue(refSKILL_TABLE, null);
				if (num2 != 0f)
				{
					propertyInfo.SetValue(sKILL_TABLE, num2, null);
				}
			}
			else if (propertyInfo.PropertyType == typeof(string))
			{
				string text = (string)propertyInfo.GetValue(refSKILL_TABLE, null);
				if (text != "null")
				{
					propertyInfo.SetValue(sKILL_TABLE, text, null);
				}
			}
			else if (propertyInfo.PropertyType == typeof(ObscuredInt))
			{
				ObscuredInt obscuredInt = (ObscuredInt)propertyInfo.GetValue(refSKILL_TABLE, null);
				if ((int)obscuredInt != 0)
				{
					propertyInfo.SetValue(sKILL_TABLE, obscuredInt, null);
				}
			}
			else if (propertyInfo.PropertyType == typeof(ObscuredFloat))
			{
				ObscuredFloat obscuredFloat = (ObscuredFloat)propertyInfo.GetValue(refSKILL_TABLE, null);
				if ((float)obscuredFloat != 0f)
				{
					propertyInfo.SetValue(sKILL_TABLE, obscuredFloat, null);
				}
			}
			else if (propertyInfo.PropertyType == typeof(ObscuredString))
			{
				ObscuredString obscuredString = (ObscuredString)propertyInfo.GetValue(refSKILL_TABLE, null);
				if (obscuredString != (ObscuredString)"null")
				{
					propertyInfo.SetValue(sKILL_TABLE, obscuredString, null);
				}
			}
		}
		tSKILL_TABLE = sKILL_TABLE;
	}

	public void ReCalcuSkill(ref SKILL_TABLE tSKILL_TABLE)
	{
		if (tSKILL_TABLE == null)
		{
			return;
		}
		for (int i = 0; i < listPassiveskill.Count; i++)
		{
			RecalcuSkillBySkill(ref tSKILL_TABLE, listPassiveskill[i].tSKILL_TABLE);
		}
		if (listAddStatusPassiveskill.Count > 0)
		{
			ResetStatus(true);
			for (int num = listAddStatusPassiveskill.Count - 1; num >= 0; num--)
			{
				if (listAddStatusPassiveskill[num].tSKILL_TABLE.n_USE_TYPE == 101)
				{
					if (listAddStatusPassiveskill[num].tSKILL_TABLE.n_TRIGGER == 0)
					{
						int n_EFFECT = listAddStatusPassiveskill[num].tSKILL_TABLE.n_EFFECT;
						if (n_EFFECT == 3)
						{
							AddStatus(listAddStatusPassiveskill[num].tSKILL_TABLE, listAddStatusPassiveskill[num].nSkillLevel, listAddStatusPassiveskill[num].nWeaponCheck);
						}
					}
				}
				else if (listAddStatusPassiveskill[num].tSKILL_TABLE.n_USE_TYPE == 103)
				{
					nMeasureMaxValue = listAddStatusPassiveskill[num].tSKILL_TABLE.n_MAGAZINE;
					nMeasureInitValue = listAddStatusPassiveskill[num].tSKILL_TABLE.n_RELOAD / 1000;
					if (nMeasureInitValue > nMeasureMaxValue)
					{
						nMeasureInitValue = nMeasureMaxValue;
					}
					nAddMeasureTime = listAddStatusPassiveskill[num].tSKILL_TABLE.n_FIRE_SPEED;
				}
			}
		}
		ResetStatus(false, true);
		if (listRatioStatusPassiveskill.Count <= 0)
		{
			return;
		}
		for (int num2 = listRatioStatusPassiveskill.Count - 1; num2 >= 0; num2--)
		{
			if (listRatioStatusPassiveskill[num2].tSKILL_TABLE.n_USE_TYPE == 101 && listRatioStatusPassiveskill[num2].tSKILL_TABLE.n_TRIGGER == 0)
			{
				int n_EFFECT = listRatioStatusPassiveskill[num2].tSKILL_TABLE.n_EFFECT;
				if (n_EFFECT == 25)
				{
					RatioStatus(listRatioStatusPassiveskill[num2].tSKILL_TABLE, listRatioStatusPassiveskill[num2].nSkillLevel, listRatioStatusPassiveskill[num2].nWeaponCheck);
				}
			}
		}
	}

	public int GetSkillDmgBuff(SKILL_TABLE tSKILL_TABLE, int nWeaponCheck, int nWeaponType, PerBuffManager refPBMShoter, PerBuffManager targetBuffManager, bool bPetBullet)
	{
		int num = 0;
		int count = listEquipPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listEquipPassiveskill[i];
			num2 = 0;
			int n_EFFECT = passiveskillStatus.tSKILL_TABLE.n_EFFECT;
			if ((uint)(n_EFFECT - 5) <= 1u && refPBMShoter.SOB.GetSOBType() == 1)
			{
				num2 = refPBMShoter.SOB.GetCurrentWeaponCheck();
			}
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, nWeaponCheck | num2, nWeaponCheck, 0, fNowTotalLeftTime))
			{
				continue;
			}
			switch (passiveskillStatus.tSKILL_TABLE.n_TRIGGER)
			{
			case 3:
				if (refPBMShoter.SOB.GetSOBType() == 1)
				{
					OrangeCharacter orangeCharacter2 = refPBMShoter.SOB as OrangeCharacter;
					if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0)
					{
						bool flag2 = false;
						if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & orangeCharacter2.PlayerWeapons[0].WeaponData.n_TYPE) != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y & orangeCharacter2.PlayerWeapons[1].WeaponData.n_TYPE) != 0)
						{
							flag2 = true;
						}
						else if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & orangeCharacter2.PlayerWeapons[1].WeaponData.n_TYPE) != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y & orangeCharacter2.PlayerWeapons[0].WeaponData.n_TYPE) != 0)
						{
							flag2 = true;
						}
						if (!flag2)
						{
							continue;
						}
					}
					else if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & orangeCharacter2.GetCurrentWeaponObj().WeaponData.n_TYPE) == 0)
					{
						continue;
					}
				}
				else if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & nWeaponType) == 0)
				{
					continue;
				}
				break;
			case 5:
			{
				int num3 = 0;
				if (refPBMShoter.sBuffStatus.nMAXHP > 0)
				{
					num3 = refPBMShoter.sBuffStatus.nHP * 100 / refPBMShoter.sBuffStatus.nMAXHP;
				}
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X > num3 || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y < num3)
				{
					continue;
				}
				break;
			}
			case 6:
				if (targetBuffManager == null || !targetBuffManager.CheckHasEffect(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X))
				{
					continue;
				}
				break;
			case 7:
			{
				OrangeCharacter orangeCharacter3 = refPBMShoter.SOB as OrangeCharacter;
				if (!orangeCharacter3)
				{
					continue;
				}
				switch (nWeaponCheck)
				{
				case 1:
					if (orangeCharacter3.PlayerSkills[0].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 2:
					if (orangeCharacter3.PlayerSkills[1].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 4:
					if (orangeCharacter3.PlayerWeapons[0].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 8:
					if (orangeCharacter3.PlayerWeapons[1].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				default:
					continue;
				}
				break;
			}
			case 8:
			{
				OrangeCharacter orangeCharacter3 = refPBMShoter.SOB as OrangeCharacter;
				if (!orangeCharacter3)
				{
					continue;
				}
				float num5 = 0f;
				if (nWeaponCheck != 4)
				{
					if (nWeaponCheck != 8)
					{
						continue;
					}
					num5 = orangeCharacter3.PlayerWeapons[1].MagazineRemain * 100f / (float)orangeCharacter3.PlayerWeapons[1].BulletData.n_MAGAZINE;
				}
				else
				{
					num5 = orangeCharacter3.PlayerWeapons[0].MagazineRemain * 100f / (float)orangeCharacter3.PlayerWeapons[0].BulletData.n_MAGAZINE;
				}
				if (num5 < (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X || num5 > (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)
				{
					continue;
				}
				break;
			}
			case 10:
				if (refPBMShoter.nMeasureNow < passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0 && refPBMShoter.nMeasureNow > passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z))
				{
					continue;
				}
				break;
			case 19:
			{
				bool flag4 = false;
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 1)
				{
					if (targetBuffManager != null && targetBuffManager.SOB.GetSOBType() == 2)
					{
						EnemyControllerBase enemyControllerBase = targetBuffManager.SOB as EnemyControllerBase;
						if (enemyControllerBase.EnemyData != null && ManagedSingleton<OrangeTableHelper>.Instance.IsBossSP(enemyControllerBase.EnemyData) && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y == 0 || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y == tSKILL_TABLE.n_ID))
						{
							flag4 = true;
						}
					}
				}
				else if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 2 && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
				{
					if (targetBuffManager != null && targetBuffManager.SOB.GetSOBType() == 1 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y == 0 || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y == tSKILL_TABLE.n_ID))
					{
						flag4 = true;
					}
				}
				else if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 4 && targetBuffManager != null && targetBuffManager.SOB.GetSOBType() == 2)
				{
					EnemyControllerBase enemyControllerBase2 = targetBuffManager.SOB as EnemyControllerBase;
					if (enemyControllerBase2.EnemyData != null && ManagedSingleton<OrangeTableHelper>.Instance.IsZakoSP(enemyControllerBase2.EnemyData))
					{
						flag4 = true;
					}
				}
				if (!flag4)
				{
					continue;
				}
				break;
			}
			case 15:
			{
				bool flag3 = false;
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == tSKILL_TABLE.n_ID && targetBuffManager != null && targetBuffManager.CheckHasEffect(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y) && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z == 0 || targetBuffManager.CheckHasEffectByCONDITIONID(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z)))
				{
					flag3 = true;
				}
				if (!flag3)
				{
					continue;
				}
				break;
			}
			case 25:
			{
				if (bPetBullet || refPBMShoter == null || refPBMShoter.SOB == null || targetBuffManager == null || targetBuffManager.SOB == null || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & nWeaponCheck) == 0))
				{
					continue;
				}
				float num4 = Vector2.Distance(targetBuffManager.SOB.transform.position, refPBMShoter.SOB.transform.position);
				if (num4 < (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y || num4 >= (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z)
				{
					continue;
				}
				break;
			}
			case 26:
				if (refPBMShoter == null || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && !refPBMShoter.CheckHasEffect(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X)) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 && !refPBMShoter.CheckHasEffectByCONDITIONID(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)))
				{
					continue;
				}
				break;
			case 28:
				if (refPBMShoter.SOB.GetSOBType() == 1)
				{
					OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
					if (orangeCharacter == null)
					{
						continue;
					}
					bool flag = false;
					if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && orangeCharacter.CheckPetActiveEvt(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X))
					{
						flag = true;
					}
					if (!flag && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 && orangeCharacter.CheckPetActiveEvt(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y))
					{
						flag = true;
					}
					if (!flag && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0 && orangeCharacter.CheckPetActiveEvt(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z))
					{
						flag = true;
					}
					if (!flag)
					{
						continue;
					}
				}
				break;
			default:
				continue;
			}
			n_EFFECT = passiveskillStatus.tSKILL_TABLE.n_EFFECT;
			if (n_EFFECT == 5 && passiveskillStatus.GetNowLeftTime(fNowTotalLeftTime) >= (float)passiveskillStatus.tSKILL_TABLE.n_RELOAD && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE >= OrangeBattleUtility.Random(0, 10000))
			{
				passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
				num += Mathf.RoundToInt(passiveskillStatus.tSKILL_TABLE.f_EFFECT_X);
			}
		}
		return num;
	}

	public int GetSkillResistanceSPBuff(SKILL_TABLE tSTShooter, int nWeaponCheck, int hp, int maxhp, PerBuffManager refPBMShooter, PerBuffManager refPBMTarget, bool isPetBullet)
	{
		int num = 0;
		int count = listEquipPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		int num2 = 0;
		if (hp < 0)
		{
			hp = 0;
		}
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listEquipPassiveskill[i];
			num2 = 0;
			switch (passiveskillStatus.tSKILL_TABLE.n_EFFECT)
			{
			case 5:
				if (refPBMTarget.SOB.GetSOBType() == 1)
				{
					num2 = refPBMTarget.SOB.GetCurrentWeaponCheck();
				}
				break;
			case 6:
				if (refPBMTarget.SOB.GetSOBType() == 1)
				{
					num2 = refPBMTarget.SOB.GetCurrentWeaponCheck();
					nWeaponCheck = 0;
				}
				break;
			}
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, nWeaponCheck | num2, nWeaponCheck, 0, fNowTotalLeftTime))
			{
				continue;
			}
			switch (passiveskillStatus.tSKILL_TABLE.n_TRIGGER)
			{
			case 3:
			{
				if (refPBMTarget.SOB.GetSOBType() != 1)
				{
					break;
				}
				OrangeCharacter orangeCharacter3 = refPBMTarget.SOB as OrangeCharacter;
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0)
				{
					bool flag2 = false;
					if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & orangeCharacter3.PlayerWeapons[0].WeaponData.n_TYPE) != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y & orangeCharacter3.PlayerWeapons[1].WeaponData.n_TYPE) != 0)
					{
						flag2 = true;
					}
					else if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & orangeCharacter3.PlayerWeapons[1].WeaponData.n_TYPE) != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y & orangeCharacter3.PlayerWeapons[0].WeaponData.n_TYPE) != 0)
					{
						flag2 = true;
					}
					if (!flag2)
					{
						continue;
					}
				}
				else if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & orangeCharacter3.GetCurrentWeaponObj().WeaponData.n_TYPE) == 0)
				{
					continue;
				}
				break;
			}
			case 5:
			{
				int num5 = 0;
				num5 = hp * 100 / maxhp;
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X > num5 || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y < num5)
				{
					continue;
				}
				break;
			}
			case 7:
			{
				OrangeCharacter orangeCharacter2 = refPBMTarget.SOB as OrangeCharacter;
				if (!orangeCharacter2)
				{
					continue;
				}
				switch (nWeaponCheck)
				{
				case 1:
					if (orangeCharacter2.PlayerSkills[0].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 2:
					if (orangeCharacter2.PlayerSkills[1].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 4:
					if (orangeCharacter2.PlayerWeapons[0].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 8:
					if (orangeCharacter2.PlayerWeapons[1].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				default:
					continue;
				}
				break;
			}
			case 8:
			{
				OrangeCharacter orangeCharacter2 = refPBMTarget.SOB as OrangeCharacter;
				if (!orangeCharacter2)
				{
					continue;
				}
				float num4 = 0f;
				switch (nWeaponCheck)
				{
				case 1:
					num4 = orangeCharacter2.PlayerSkills[0].MagazineRemain * 100f / (float)orangeCharacter2.PlayerSkills[0].BulletData.n_MAGAZINE;
					break;
				case 2:
					num4 = orangeCharacter2.PlayerSkills[1].MagazineRemain * 100f / (float)orangeCharacter2.PlayerSkills[1].BulletData.n_MAGAZINE;
					break;
				case 4:
					num4 = orangeCharacter2.PlayerWeapons[0].MagazineRemain * 100f / (float)orangeCharacter2.PlayerWeapons[0].BulletData.n_MAGAZINE;
					break;
				case 8:
					num4 = orangeCharacter2.PlayerWeapons[1].MagazineRemain * 100f / (float)orangeCharacter2.PlayerWeapons[1].BulletData.n_MAGAZINE;
					break;
				default:
					continue;
				}
				if (num4 < (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X || num4 > (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)
				{
					continue;
				}
				break;
			}
			case 10:
				if (refPBMTarget.nMeasureNow < passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X || refPBMTarget.nMeasureNow > passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)
				{
					continue;
				}
				break;
			case 20:
			{
				bool flag3 = false;
				bool flag4 = false;
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 1)
				{
					if (refPBMShooter != null && refPBMShooter.SOB.GetSOBType() == 2)
					{
						EnemyControllerBase enemyControllerBase = refPBMShooter.SOB as EnemyControllerBase;
						if (enemyControllerBase.EnemyData != null && ManagedSingleton<OrangeTableHelper>.Instance.IsBossSP(enemyControllerBase.EnemyData))
						{
							flag3 = true;
						}
					}
				}
				else if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 2 && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
				{
					if (refPBMShooter != null && refPBMShooter.SOB.GetSOBType() == 1)
					{
						flag3 = true;
					}
				}
				else if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 3)
				{
					flag3 = isPetBullet;
				}
				else if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 4 && refPBMShooter != null && refPBMShooter.SOB.GetSOBType() == 2)
				{
					EnemyControllerBase enemyControllerBase2 = refPBMShooter.SOB as EnemyControllerBase;
					if (enemyControllerBase2.EnemyData != null && ManagedSingleton<OrangeTableHelper>.Instance.IsZakoSP(enemyControllerBase2.EnemyData))
					{
						flag3 = true;
					}
				}
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y == 0 || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y == tSTShooter.n_ID)
				{
					flag4 = true;
				}
				if (!flag3 || !flag4)
				{
					continue;
				}
				break;
			}
			case 25:
			{
				if (isPetBullet || refPBMShooter == null || refPBMShooter.SOB == null || refPBMTarget == null || refPBMTarget.SOB == null || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & num2) == 0))
				{
					continue;
				}
				float num3 = Vector2.Distance(refPBMTarget.SOB.transform.position, refPBMShooter.SOB.transform.position);
				if (num3 < (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y || num3 >= (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z)
				{
					continue;
				}
				break;
			}
			case 26:
				if (refPBMTarget == null || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && !refPBMTarget.CheckHasEffect(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X)) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 && !refPBMTarget.CheckHasEffectByCONDITIONID(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)))
				{
					continue;
				}
				break;
			case 28:
				if (refPBMTarget.SOB.GetSOBType() == 1)
				{
					OrangeCharacter orangeCharacter = refPBMTarget.SOB as OrangeCharacter;
					if (orangeCharacter == null)
					{
						continue;
					}
					bool flag = false;
					if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && orangeCharacter.CheckPetActiveEvt(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X))
					{
						flag = true;
					}
					if (!flag && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 && orangeCharacter.CheckPetActiveEvt(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y))
					{
						flag = true;
					}
					if (!flag && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0 && orangeCharacter.CheckPetActiveEvt(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z))
					{
						flag = true;
					}
					if (!flag)
					{
						continue;
					}
				}
				break;
			default:
				continue;
			}
			int n_EFFECT = passiveskillStatus.tSKILL_TABLE.n_EFFECT;
			if (n_EFFECT == 6 && passiveskillStatus.GetNowLeftTime(fNowTotalLeftTime) >= (float)passiveskillStatus.tSKILL_TABLE.n_RELOAD && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE >= OrangeBattleUtility.Random(0, 10000))
			{
				passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
				num += Mathf.RoundToInt(passiveskillStatus.tSKILL_TABLE.f_EFFECT_X);
			}
		}
		return num;
	}

	public void HitSkillTrigger(SKILL_TABLE pData, int nWeaponCheck, PerBuffManager refPBMShoter, PerBuffManager targetBuffManager, int nAtkParam, int nAtk, bool bPetBullet, Action<SKILL_TABLE> tCB)
	{
		int count = listHitPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listHitPassiveskill[i];
			switch (passiveskillStatus.tSKILL_TABLE.n_TRIGGER)
			{
			case 9:
				if (targetBuffManager == null || (int)targetBuffManager.SOB.Hp > 0)
				{
					continue;
				}
				break;
			case 6:
				if (targetBuffManager == null || !targetBuffManager.CheckHasEffect(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X))
				{
					continue;
				}
				break;
			case 15:
			{
				bool flag = false;
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == pData.n_ID && targetBuffManager != null && targetBuffManager.CheckHasEffect(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y) && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z == 0 || targetBuffManager.CheckHasEffectByCONDITIONID(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z)))
				{
					flag = true;
				}
				if (!flag)
				{
					continue;
				}
				break;
			}
			case 22:
				if (passiveskillStatus.tSKILL_TABLE.n_ID == pData.n_ID || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & nWeaponCheck) == 0) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != pData.n_ID) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0 && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z == pData.n_ID))
				{
					continue;
				}
				break;
			case 23:
				if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & nWeaponCheck) == 0) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != pData.n_ID) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0 && (targetBuffManager == null || targetBuffManager.SOB == null || !CheckSOBType(targetBuffManager.SOB, passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z))))
				{
					continue;
				}
				break;
			case 25:
			{
				if (bPetBullet || refPBMShoter == null || refPBMShoter.SOB == null || targetBuffManager == null || targetBuffManager.SOB == null || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & nWeaponCheck) == 0))
				{
					continue;
				}
				float num = Vector2.Distance(targetBuffManager.SOB.transform.position, refPBMShoter.SOB.transform.position);
				if (num < (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y || num >= (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z)
				{
					continue;
				}
				break;
			}
			case 31:
				if (refPBMShoter == null || refPBMShoter.SOB == null || targetBuffManager == null || targetBuffManager.SOB == null || !targetBuffManager.CheckHasEffect(passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != pData.n_ID) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0 && ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z == 1 && targetBuffManager.SOB.sNetSerialID == refPBMShoter.SOB.sNetSerialID) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z == 2 && targetBuffManager.SOB.sNetSerialID != refPBMShoter.SOB.sNetSerialID))))
				{
					continue;
				}
				break;
			case 11:
				continue;
			}
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, nWeaponCheck, nWeaponCheck, pData.n_ID, fNowTotalLeftTime) || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE < OrangeBattleUtility.Random(0, 10000))
			{
				continue;
			}
			if (targetBuffManager == null)
			{
				if (passiveskillStatus.tSKILL_TABLE.n_TYPE != 0 && passiveskillStatus.tSKILL_TABLE.s_MODEL != "null" && tCB != null)
				{
					passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
					StageUpdate.SyncStageObj(4, 12, refPBMShoter.SOB.sNetSerialID + "," + passiveskillStatus.tSKILL_TABLE.n_ID, true);
					tCB(passiveskillStatus.tSKILL_TABLE);
				}
				continue;
			}
			passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
			StageUpdate.SyncStageObj(4, 12, refPBMShoter.SOB.sNetSerialID + "," + passiveskillStatus.tSKILL_TABLE.n_ID, true);
			CONDITION_TABLE value = null;
			if (passiveskillStatus.tSKILL_TABLE.n_CONDITION_ID > 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(passiveskillStatus.tSKILL_TABLE.n_CONDITION_ID, out value))
			{
				if (value.n_EFFECT == 6)
				{
					TriggerBuff(passiveskillStatus.tSKILL_TABLE, targetBuffManager, refPBMShoter, nAtk, 0, pData.n_ID);
				}
				else
				{
					TriggerBuff(passiveskillStatus.tSKILL_TABLE, targetBuffManager, refPBMShoter, nAtkParam, 0, pData.n_ID);
				}
			}
			else
			{
				TriggerBuff(passiveskillStatus.tSKILL_TABLE, targetBuffManager, refPBMShoter, nAtkParam, 0, pData.n_ID);
			}
			if (passiveskillStatus.tSKILL_TABLE.n_TYPE != 0)
			{
				if (passiveskillStatus.tSKILL_TABLE.s_MODEL != "null" && tCB != null)
				{
					tCB(passiveskillStatus.tSKILL_TABLE);
				}
			}
			else if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER == 21)
			{
				if (passiveskillStatus.tSKILL_TABLE.n_EFFECT == 2)
				{
					TriggerSkillEx(ref passiveskillStatus, nWeaponCheck, refPBMShoter, targetBuffManager, nAtk);
				}
				else
				{
					TriggerSkillEx(ref passiveskillStatus, nWeaponCheck, refPBMShoter, targetBuffManager, nAtkParam);
				}
			}
			else if (passiveskillStatus.tSKILL_TABLE.n_EFFECT == 2)
			{
				TriggerSkill(passiveskillStatus.tSKILL_TABLE, passiveskillStatus.nSkillLevel, nWeaponCheck, refPBMShoter, targetBuffManager, nAtk);
			}
			else
			{
				TriggerSkill(passiveskillStatus.tSKILL_TABLE, passiveskillStatus.nSkillLevel, nWeaponCheck, refPBMShoter, targetBuffManager, nAtkParam);
			}
		}
	}

	public SKILL_TABLE GetSkillTable(int nSkillID)
	{
		for (int i = 0; i < listHitPassiveskill.Count; i++)
		{
			if (listHitPassiveskill[i].tSKILL_TABLE.n_ID == nSkillID)
			{
				return listHitPassiveskill[i].tSKILL_TABLE;
			}
		}
		for (int j = 0; j < listUsePassiveskill.Count; j++)
		{
			if (listUsePassiveskill[j].tSKILL_TABLE.n_ID == nSkillID)
			{
				return listUsePassiveskill[j].tSKILL_TABLE;
			}
		}
		for (int k = 0; k < listEquipPassiveskill.Count; k++)
		{
			if (listEquipPassiveskill[k].tSKILL_TABLE.n_ID == nSkillID)
			{
				return listEquipPassiveskill[k].tSKILL_TABLE;
			}
		}
		for (int l = 0; l < listHurtPassiveskill.Count; l++)
		{
			if (listHurtPassiveskill[l].tSKILL_TABLE.n_ID == nSkillID)
			{
				return listHurtPassiveskill[l].tSKILL_TABLE;
			}
		}
		return null;
	}

	public void UseSkillTrigger(int id, int ntriggerskillid, WeaponStatus tWeaponStatus, ref PerBuffManager selfBuffManager, Action<SKILL_TABLE> pcb)
	{
		PassiveskillStatus passiveskillStatus = null;
		int nMeasureNow = selfBuffManager.nMeasureNow;
		for (int i = 0; i < listUsePassiveskill.Count; i++)
		{
			passiveskillStatus = listUsePassiveskill[i];
			if (14701 <= passiveskillStatus.tSKILL_TABLE.n_ID && passiveskillStatus.tSKILL_TABLE.n_ID <= 14800)
			{
				if (passiveskillStatus.tCACHE_SKILL_TABLE != null)
				{
					passiveskillStatus.tSKILL_TABLE = passiveskillStatus.tCACHE_SKILL_TABLE;
				}
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(passiveskillStatus.tSKILL_TABLE.n_LINK_SKILL))
				{
					SKILL_TABLE sKILL_TABLE = passiveskillStatus.tSKILL_TABLE;
					randomIndexList.Clear();
					while (sKILL_TABLE != null)
					{
						randomIndexList.Add(sKILL_TABLE.n_ID);
						if (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(sKILL_TABLE.n_LINK_SKILL))
						{
							break;
						}
						sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[sKILL_TABLE.n_LINK_SKILL];
					}
					if (randomIndexList.Count > 0)
					{
						int num = OrangeBattleUtility.Random(0, randomIndexList.Count);
						if (num != 0)
						{
							passiveskillStatus.tCACHE_SKILL_TABLE = passiveskillStatus.tSKILL_TABLE;
							passiveskillStatus.tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[randomIndexList[num]];
						}
					}
				}
			}
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, tWeaponStatus.nWeaponCheck, id, ntriggerskillid, fNowTotalLeftTime))
			{
				continue;
			}
			switch (passiveskillStatus.tSKILL_TABLE.n_TRIGGER)
			{
			case 7:
			{
				OrangeCharacter orangeCharacter = selfBuffManager.SOB as OrangeCharacter;
				if (!orangeCharacter)
				{
					break;
				}
				switch (id)
				{
				case 1:
					if (orangeCharacter.PlayerSkills[0].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 2:
					if (orangeCharacter.PlayerSkills[1].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 4:
					if (orangeCharacter.PlayerWeapons[0].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				case 8:
					if (orangeCharacter.PlayerWeapons[1].MagazineRemain > 0f)
					{
						continue;
					}
					break;
				default:
					continue;
				}
				break;
			}
			case 8:
			{
				OrangeCharacter orangeCharacter = selfBuffManager.SOB as OrangeCharacter;
				if ((bool)orangeCharacter)
				{
					float num2 = 0f;
					switch (id)
					{
					case 1:
						num2 = orangeCharacter.PlayerSkills[0].MagazineRemain * 100f / (float)orangeCharacter.PlayerSkills[0].BulletData.n_MAGAZINE;
						break;
					case 2:
						num2 = orangeCharacter.PlayerSkills[1].MagazineRemain * 100f / (float)orangeCharacter.PlayerSkills[1].BulletData.n_MAGAZINE;
						break;
					case 4:
						num2 = orangeCharacter.PlayerWeapons[0].MagazineRemain * 100f / (float)orangeCharacter.PlayerWeapons[0].BulletData.n_MAGAZINE;
						break;
					case 8:
						num2 = orangeCharacter.PlayerWeapons[1].MagazineRemain * 100f / (float)orangeCharacter.PlayerWeapons[1].BulletData.n_MAGAZINE;
						break;
					default:
						continue;
					}
					if (num2 < (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X || num2 > (float)passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)
					{
						continue;
					}
				}
				break;
			}
			case 10:
				if (nMeasureNow < passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X || nMeasureNow > passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)
				{
					continue;
				}
				break;
			case 18:
			{
				OrangeCharacter orangeCharacter = selfBuffManager.SOB as OrangeCharacter;
				if (orangeCharacter == null || ntriggerskillid != 0 || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & orangeCharacter.GetCurrentWeaponObj().WeaponData.n_TYPE) == 0 || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y == 1 && orangeCharacter.Controller.Collisions.below))
				{
					continue;
				}
				break;
			}
			case 11:
				continue;
			}
			if ((passiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2 && (nMeasureNow < passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z || selfBuffManager.nMeasureNow < passiveskillStatus.tSKILL_TABLE.n_USE_COST)) || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE < OrangeBattleUtility.Random(0, 10000))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2)
			{
				selfBuffManager.AddMeasure(-passiveskillStatus.tSKILL_TABLE.n_USE_COST, true);
				CinnamonController component = selfBuffManager.SOB.GetComponent<CinnamonController>();
				if (component != null)
				{
					component.PlayMeasureSE(-passiveskillStatus.tSKILL_TABLE.n_USE_COST);
				}
			}
			StanrdTriggerPassiveskillNoCheck(passiveskillStatus, tWeaponStatus, selfBuffManager, pcb);
		}
	}

	public void BulletEndTrigger(BulletBase tBB, ref PerBuffManager selfBuffManager, Action<SKILL_TABLE> pcb)
	{
		PassiveskillStatus passiveskillStatus = null;
		int nMeasureNow = selfBuffManager.nMeasureNow;
		WeaponStatus weaponStatus = null;
		for (int i = 0; i < listHitPassiveskill.Count; i++)
		{
			passiveskillStatus = listHitPassiveskill[i];
			if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER != 11)
			{
				continue;
			}
			if (weaponStatus == null)
			{
				weaponStatus = new WeaponStatus();
				weaponStatus.nHP = tBB.nHp;
				weaponStatus.nATK = tBB.nAtk;
				weaponStatus.nCRI = tBB.nCri;
				weaponStatus.nHIT = tBB.nHit;
				weaponStatus.nCriDmgPercent = tBB.nCriDmgPercent;
				weaponStatus.nReduceBlockPercent = tBB.nReduceBlockPercent;
				weaponStatus.nWeaponCheck = tBB.nWeaponCheck;
				weaponStatus.nWeaponType = tBB.nWeaponType;
			}
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, weaponStatus.nWeaponCheck, weaponStatus.nWeaponCheck, tBB.GetBulletData.n_ID, fNowTotalLeftTime) || (passiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2 && (nMeasureNow < passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z || selfBuffManager.nMeasureNow < passiveskillStatus.tSKILL_TABLE.n_USE_COST)) || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE < OrangeBattleUtility.Random(0, 10000))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2)
			{
				selfBuffManager.AddMeasure(-passiveskillStatus.tSKILL_TABLE.n_USE_COST, true);
				CinnamonController component = selfBuffManager.SOB.GetComponent<CinnamonController>();
				if (component != null)
				{
					component.PlayMeasureSE(-passiveskillStatus.tSKILL_TABLE.n_USE_COST);
				}
			}
			StanrdTriggerPassiveskillNoCheck(passiveskillStatus, weaponStatus, selfBuffManager, pcb);
		}
	}

	public void PerSecTrigger(bool isMoving, WeaponStatus tWeaponStatus, ref PerBuffManager selfBuffManager, Action<SKILL_TABLE> pcb)
	{
		PassiveskillStatus passiveskillStatus = null;
		for (int i = 0; i < listPerSecPassiveSkill.Count; i++)
		{
			passiveskillStatus = listPerSecPassiveSkill[i];
			if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 1 && isMoving)
			{
				passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0)
			{
				int num = selfBuffManager.sBuffStatus.nHP * 100 / selfBuffManager.sBuffStatus.nMAXHP;
				if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y > num || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z < num)
				{
					continue;
				}
			}
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, tWeaponStatus.nWeaponCheck, tWeaponStatus.nWeaponCheck, 0, fNowTotalLeftTime) || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE < OrangeBattleUtility.Random(0, 10000))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_RELOAD >= 600000)
			{
				bool flag = false;
				OrangeCharacter orangeCharacter = selfBuffManager.SOB as OrangeCharacter;
				List<string> perGameSaveData = StageUpdate.GetPerGameSaveData();
				for (int j = 0; j < perGameSaveData.Count; j++)
				{
					string[] array = perGameSaveData[j].Split(',');
					if (array.Length >= 3 && array[0] == orangeCharacter.sNetSerialID && array[2] == passiveskillStatus.tSKILL_TABLE.n_ID.ToString())
					{
						if (passiveskillStatus.nWeaponCheck == 4 || passiveskillStatus.nWeaponCheck == 8)
						{
							flag = true;
						}
						else if (array[1] == orangeCharacter.CharacterID.ToString())
						{
							flag = true;
						}
						break;
					}
				}
				if (flag)
				{
					passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
					continue;
				}
				StageUpdate.AddPerGameSavaData(orangeCharacter.sNetSerialID + "," + orangeCharacter.CharacterID + "," + passiveskillStatus.tSKILL_TABLE.n_ID);
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(4, 7, orangeCharacter.sNetSerialID + "," + orangeCharacter.CharacterID + "," + passiveskillStatus.tSKILL_TABLE.n_ID, true);
				}
			}
			StanrdTriggerPassiveskillNoCheck(passiveskillStatus, tWeaponStatus, selfBuffManager, pcb);
		}
	}

	private void StanrdTriggerPassiveskillNoCheck(PassiveskillStatus tPassiveskillStatus, WeaponStatus tWeaponStatus, PerBuffManager selfBuffManager, Action<SKILL_TABLE> pcb)
	{
		tPassiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
		if (tPassiveskillStatus.tSKILL_TABLE.n_TARGET == 1 && tPassiveskillStatus.tSKILL_TABLE.n_TYPE != 0 && pcb != null)
		{
			pcb(tPassiveskillStatus.tSKILL_TABLE);
		}
		if (tPassiveskillStatus.tSKILL_TABLE.n_TARGET == 2)
		{
			GameObject gameObject = null;
			if (selfBuffManager.SOB != null)
			{
				gameObject = selfBuffManager.SOB.gameObject;
			}
			List<OrangeCharacter> list = new List<OrangeCharacter>();
			if (tPassiveskillStatus.tSKILL_TABLE.s_FIELD != "null")
			{
				string[] array = tPassiveskillStatus.tSKILL_TABLE.s_FIELD.Split(',');
				string text = array[0];
				Collider2D collider2D;
				if (!(text == "0"))
				{
					if (text == "1")
					{
						collider2D = gameObject.AddComponent<CircleCollider2D>();
						((CircleCollider2D)collider2D).radius = float.Parse(array[3]);
					}
					else
					{
						collider2D = gameObject.AddComponent<BoxCollider2D>();
						((BoxCollider2D)collider2D).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
					}
				}
				else
				{
					collider2D = gameObject.AddComponent<BoxCollider2D>();
					((BoxCollider2D)collider2D).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
				}
				collider2D.offset = new Vector2(float.Parse(array[1]), float.Parse(array[2]));
				Bounds bounds = collider2D.bounds;
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.runPlayers[num].UsingVehicle)
					{
						RideArmorController component = StageUpdate.runPlayers[num].transform.root.GetComponent<RideArmorController>();
						if (component.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer && bounds.Intersects(component.Controller.Collider2D.bounds))
						{
							list.Add(component.MasterPilot);
						}
					}
					else if (StageUpdate.runPlayers[num].gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer && bounds.Intersects(StageUpdate.runPlayers[num].Controller.Collider2D.bounds))
					{
						list.Add(StageUpdate.runPlayers[num]);
					}
				}
				UnityEngine.Object.Destroy(collider2D);
			}
			else
			{
				for (int num2 = StageUpdate.runPlayers.Count - 1; num2 >= 0; num2--)
				{
					if (StageUpdate.runPlayers[num2].UsingVehicle)
					{
						RideArmorController component2 = StageUpdate.runPlayers[num2].transform.root.GetComponent<RideArmorController>();
						if (component2.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)
						{
							list.Add(component2.MasterPilot);
						}
					}
					else if (StageUpdate.runPlayers[num2].gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)
					{
						list.Add(StageUpdate.runPlayers[num2]);
					}
				}
			}
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				if (list[num3].UsingVehicle)
				{
					RideArmorController component3 = list[num3].transform.root.GetComponent<RideArmorController>();
					TriggerBuff(tPassiveskillStatus.tSKILL_TABLE, component3.selfBuffManager, selfBuffManager, tWeaponStatus.nATK, selfBuffManager.SOB.MaxHp, tPassiveskillStatus.tSKILL_TABLE.n_ID);
					TriggerSkill(tPassiveskillStatus.tSKILL_TABLE, tPassiveskillStatus.nSkillLevel, tWeaponStatus.nWeaponCheck, selfBuffManager, component3.selfBuffManager, tWeaponStatus.nATK);
				}
				else
				{
					TriggerBuff(tPassiveskillStatus.tSKILL_TABLE, list[num3].selfBuffManager, selfBuffManager, tWeaponStatus.nATK, selfBuffManager.SOB.MaxHp, tPassiveskillStatus.tSKILL_TABLE.n_ID);
					TriggerSkill(tPassiveskillStatus.tSKILL_TABLE, tPassiveskillStatus.nSkillLevel, tWeaponStatus.nWeaponCheck, selfBuffManager, list[num3].selfBuffManager, tWeaponStatus.nATK);
				}
			}
		}
		else
		{
			if (tPassiveskillStatus.tSKILL_TABLE.n_TARGET != 3)
			{
				return;
			}
			TriggerBuff(tPassiveskillStatus.tSKILL_TABLE, null, selfBuffManager, tWeaponStatus.nATK, selfBuffManager.SOB.MaxHp, tPassiveskillStatus.tSKILL_TABLE.n_ID);
			if (tPassiveskillStatus.tSKILL_TABLE.n_TYPE == 5)
			{
				if (pcb != null)
				{
					pcb(tPassiveskillStatus.tSKILL_TABLE);
				}
			}
			else
			{
				TriggerSkill(tPassiveskillStatus.tSKILL_TABLE, tPassiveskillStatus.nSkillLevel, tWeaponStatus.nWeaponCheck, selfBuffManager, selfBuffManager, tWeaponStatus.nATK);
			}
		}
	}

	public void HurtTrigger(ref ObscuredInt nDmg, ObscuredInt nWC, ref PerBuffManager selfBuffManager, Action<SKILL_TABLE> pcb)
	{
		int count = listHurtPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		int num = 0;
		OrangeCharacter orangeCharacter = selfBuffManager.SOB as OrangeCharacter;
		if (orangeCharacter != null)
		{
			orangeCharacter.UseHitSE = true;
		}
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listHurtPassiveskill[i];
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, nWC, nWC, 0, fNowTotalLeftTime) || (passiveskillStatus.tSKILL_TABLE.n_TRIGGER == 10 && (selfBuffManager.nMeasureNow < passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X || selfBuffManager.nMeasureNow > passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)) || passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE < OrangeBattleUtility.Random(0, 10000))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 0)
			{
				if (passiveskillStatus.tSKILL_TABLE.n_CONDITION_RATE > 0 && passiveskillStatus.tSKILL_TABLE.n_CONDITION_RATE >= OrangeBattleUtility.Random(0, 10000))
				{
					passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
					if (passiveskillStatus.tSKILL_TABLE.n_CONDITION_TARGET == 1)
					{
						if (orangeCharacter != null)
						{
							selfBuffManager.AddBuff(passiveskillStatus.tSKILL_TABLE.n_CONDITION_ID, orangeCharacter.PlayerWeapons[orangeCharacter.WeaponCurrent].weaponStatus.nATK, orangeCharacter.MaxHp, passiveskillStatus.tSKILL_TABLE.n_ID, false, selfBuffManager.SOB.sNetSerialID, 4);
						}
						else
						{
							selfBuffManager.AddBuff(passiveskillStatus.tSKILL_TABLE.n_CONDITION_ID, 0, 0, passiveskillStatus.tSKILL_TABLE.n_ID, false, selfBuffManager.SOB.sNetSerialID, 4);
						}
						CONDITION_TABLE value = null;
						if (ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(passiveskillStatus.tSKILL_TABLE.n_CONDITION_ID, out value) && value.n_IGNORE_HITSE == 1)
						{
							orangeCharacter.UseHitSE = false;
						}
					}
				}
				switch (passiveskillStatus.tSKILL_TABLE.n_EFFECT)
				{
				case 7:
					if (orangeCharacter != null)
					{
						orangeCharacter.AddMagazine(nWC, (int)passiveskillStatus.tSKILL_TABLE.f_EFFECT_X);
					}
					break;
				case 4:
					if (orangeCharacter != null)
					{
						orangeCharacter.CDSkill((int)passiveskillStatus.tSKILL_TABLE.f_EFFECT_X - 1);
					}
					break;
				case 8:
					passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
					num = Mathf.RoundToInt(passiveskillStatus.tSKILL_TABLE.f_EFFECT_X);
					selfBuffManager.AddMeasure(num, true);
					break;
				case 16:
					if (orangeCharacter != null)
					{
						passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
						CharacterControlBase component = orangeCharacter.GetComponent<CharacterControlBase>();
						if ((bool)component)
						{
							component.CallPet((int)passiveskillStatus.tSKILL_TABLE.f_EFFECT_X, true, -1, null);
						}
					}
					break;
				case 2:
					if (orangeCharacter != null && orangeCharacter.IsLocalPlayer)
					{
						passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
						TriggerSkill(passiveskillStatus.tSKILL_TABLE, passiveskillStatus.nSkillLevel, 4095, orangeCharacter.selfBuffManager, orangeCharacter.selfBuffManager, orangeCharacter.PlayerWeapons[orangeCharacter.WeaponCurrent].weaponStatus.nATK);
					}
					break;
				case 1:
					if (orangeCharacter != null && orangeCharacter.IsLocalPlayer)
					{
						passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
						if (pcb != null)
						{
							pcb(passiveskillStatus.tSKILL_TABLE);
						}
					}
					break;
				case 0:
					if (orangeCharacter != null && orangeCharacter.IsLocalPlayer && passiveskillStatus.tSKILL_TABLE.n_TYPE == 5)
					{
						passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
						if (pcb != null)
						{
							pcb(passiveskillStatus.tSKILL_TABLE);
						}
					}
					break;
				case 31:
					if (orangeCharacter != null)
					{
						orangeCharacter.CDSkillEx((int)passiveskillStatus.tSKILL_TABLE.f_EFFECT_X - 1, (int)passiveskillStatus.tSKILL_TABLE.f_EFFECT_Y);
					}
					break;
				}
			}
			else if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 1)
			{
				passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
				if (pcb != null)
				{
					pcb(passiveskillStatus.tSKILL_TABLE);
				}
			}
		}
	}

	public bool CheclIgnoreSkill(SKILL_TABLE tSKILL_TABLE, int nWC)
	{
		int count = listHurtPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listHurtPassiveskill[i];
			int n_EFFECT = passiveskillStatus.tSKILL_TABLE.n_EFFECT;
			if (n_EFFECT == 14 && passiveskillStatus.CheckCanUse(bUsePassiveskill, nWC, nWC, 0, fNowTotalLeftTime) && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE >= OrangeBattleUtility.Random(0, 10000) && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 0 && passiveskillStatus.tSKILL_TABLE.f_EFFECT_X == (float)tSKILL_TABLE.n_ID)
			{
				passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
				return true;
			}
		}
		return false;
	}

	public void StanrdTriggerSkill(SKILL_TABLE tSKILL_TABLE, int nWC, int nATK, PerBuffManager selfBuffManager, Action<SKILL_TABLE> pcb)
	{
		if (tSKILL_TABLE.n_TARGET == 1 && tSKILL_TABLE.n_TYPE != 0 && pcb != null)
		{
			pcb(tSKILL_TABLE);
		}
		if (tSKILL_TABLE.n_TARGET == 0)
		{
			TriggerBuff(tSKILL_TABLE, null, selfBuffManager, nATK, selfBuffManager.SOB.MaxHp, tSKILL_TABLE.n_ID);
			TriggerSkill(tSKILL_TABLE, 1, nWC, selfBuffManager, selfBuffManager, nATK);
		}
		else if (tSKILL_TABLE.n_TARGET == 2)
		{
			GameObject gameObject = null;
			if (selfBuffManager.SOB != null)
			{
				gameObject = selfBuffManager.SOB.gameObject;
			}
			List<OrangeCharacter> list = new List<OrangeCharacter>();
			if (tSKILL_TABLE.s_FIELD != "null")
			{
				string[] array = tSKILL_TABLE.s_FIELD.Split(',');
				string text = array[0];
				Collider2D collider2D;
				if (!(text == "0"))
				{
					if (text == "1")
					{
						collider2D = gameObject.AddComponent<CircleCollider2D>();
						((CircleCollider2D)collider2D).radius = float.Parse(array[3]);
					}
					else
					{
						collider2D = gameObject.AddComponent<BoxCollider2D>();
						((BoxCollider2D)collider2D).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
					}
				}
				else
				{
					collider2D = gameObject.AddComponent<BoxCollider2D>();
					((BoxCollider2D)collider2D).size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
				}
				collider2D.offset = new Vector2(float.Parse(array[1]), float.Parse(array[2]));
				Bounds bounds = collider2D.bounds;
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.runPlayers[num].UsingVehicle)
					{
						RideArmorController component = StageUpdate.runPlayers[num].transform.root.GetComponent<RideArmorController>();
						if (component.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer && bounds.Intersects(component.Controller.Collider2D.bounds))
						{
							list.Add(component.MasterPilot);
						}
					}
					else if (StageUpdate.runPlayers[num].gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer && bounds.Intersects(StageUpdate.runPlayers[num].Controller.Collider2D.bounds))
					{
						list.Add(StageUpdate.runPlayers[num]);
					}
				}
				UnityEngine.Object.Destroy(collider2D);
			}
			else
			{
				for (int num2 = StageUpdate.runPlayers.Count - 1; num2 >= 0; num2--)
				{
					if (StageUpdate.runPlayers[num2].UsingVehicle)
					{
						RideArmorController component2 = StageUpdate.runPlayers[num2].transform.root.GetComponent<RideArmorController>();
						if (component2.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)
						{
							list.Add(component2.MasterPilot);
						}
					}
					else if (StageUpdate.runPlayers[num2].gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)
					{
						list.Add(StageUpdate.runPlayers[num2]);
					}
				}
			}
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				if (list[num3].UsingVehicle)
				{
					RideArmorController component3 = list[num3].transform.root.GetComponent<RideArmorController>();
					TriggerBuff(tSKILL_TABLE, component3.selfBuffManager, selfBuffManager, nATK, selfBuffManager.SOB.MaxHp, tSKILL_TABLE.n_ID);
					TriggerSkill(tSKILL_TABLE, 1, nWC, selfBuffManager, component3.selfBuffManager, nATK);
				}
				else
				{
					TriggerBuff(tSKILL_TABLE, list[num3].selfBuffManager, selfBuffManager, nATK, selfBuffManager.SOB.MaxHp, tSKILL_TABLE.n_ID);
					TriggerSkill(tSKILL_TABLE, 1, nWC, selfBuffManager, list[num3].selfBuffManager, nATK);
				}
			}
		}
		else if (tSKILL_TABLE.n_TARGET == 3)
		{
			TriggerBuff(tSKILL_TABLE, null, selfBuffManager, nATK, selfBuffManager.SOB.MaxHp, tSKILL_TABLE.n_ID);
			TriggerSkill(tSKILL_TABLE, 1, nWC, selfBuffManager, selfBuffManager, nATK);
		}
	}

	public static void TriggerSkill(SKILL_TABLE tSKILL_TABLE, int nSkillLevel, int nWeaponCheck, PerBuffManager refPBMShoter, PerBuffManager refPBMHit, int nAtkParam)
	{
		string text = "";
		if (refPBMShoter != null && refPBMShoter.SOB as OrangeCharacter != null)
		{
			text = refPBMShoter.SOB.sNetSerialID;
		}
		switch (tSKILL_TABLE.n_EFFECT)
		{
		case 0:
			if (refPBMHit != null)
			{
				ShowSkillTableFxSE(tSKILL_TABLE.s_HIT_FX, tSKILL_TABLE.s_HIT_SE, refPBMHit.SOB.transform);
			}
			break;
		case 1:
		{
			UnityEngine.Debug.LogError("這邊目前不應該進來，如發現此LOG請回報XD");
			if (refPBMHit == null)
			{
				break;
			}
			float num = (float)nAtkParam * (tSKILL_TABLE.f_EFFECT_X + tSKILL_TABLE.f_EFFECT_Y * (float)nSkillLevel);
			if (refPBMHit.SOB != null)
			{
				if ((bool)refPBMHit.SOB.IsUnBreak() || (bool)refPBMHit.SOB.IsUnBreakX() || refPBMHit.SOB.IsInvincible)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, refPBMHit.SOB.GetDamageTextPos(), 0, refPBMHit.SOB.GetSOBLayerMask(), VisualDamage.DamageType.Reduce);
					break;
				}
				HurtPassParam hurtPassParam2 = new HurtPassParam();
				hurtPassParam2.dmg = (int)num;
				refPBMHit.SOB.Hurt(hurtPassParam2);
			}
			break;
		}
		case 2:
		{
			PerBuffManager perBuffManager = refPBMHit;
			if (tSKILL_TABLE.n_CONDITION_TARGET == 1)
			{
				perBuffManager = refPBMShoter;
			}
			if (perBuffManager == null)
			{
				break;
			}
			string s_USE_SE = tSKILL_TABLE.s_USE_SE;
			float num = 0f;
			if (!(perBuffManager.SOB != null) || (int)perBuffManager.SOB.Hp <= 0)
			{
				break;
			}
			num += ((float)nAtkParam * tSKILL_TABLE.f_EFFECT_X + tSKILL_TABLE.f_EFFECT_Y + (float)(int)perBuffManager.SOB.MaxHp * tSKILL_TABLE.f_EFFECT_Z) / 100f;
			if (num > 0f)
			{
				num = (int)(num * (float)(100 + perBuffManager.sBuffStatus.nHealEnhance) / 100f);
				BulletBase.tNetDmgStack.nHP = perBuffManager.SOB.Hp;
				perBuffManager.SOB.Heal((int)num);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, perBuffManager.SOB.GetDamageTextPos(), (int)num, perBuffManager.SOB.GetSOBLayerMask(), VisualDamage.DamageType.Recover);
				perBuffManager.SOB.UpdateHurtAction();
				BulletBase.tNetDmgStack.sPlayerID = perBuffManager.SOB.sNetSerialID;
				BulletBase.tNetDmgStack.sShotPlayerID = text;
				BulletBase.tNetDmgStack.nDmg = -(int)num;
				BulletBase.tNetDmgStack.nRecordID = perBuffManager.SOB.GetNowRecordNO();
				BulletBase.tNetDmgStack.nNetID = 0;
				BulletBase.tNetDmgStack.sOwner = "STO";
				BulletBase.tNetDmgStack.nSubPartID = 0;
				BulletBase.tNetDmgStack.nDamageType = 0;
				BulletBase.tNetDmgStack.nWeaponType = 0;
				BulletBase.tNetDmgStack.nWeaponCheck = 0;
				BulletBase.tNetDmgStack.nSkillID = tSKILL_TABLE.n_ID;
				BulletBase.tNetDmgStack.nEndHP = perBuffManager.SOB.Hp;
				StageObjBase sOB2 = perBuffManager.SOB;
				sOB2.HealHp = (int)sOB2.HealHp + ((int)perBuffManager.SOB.Hp - (int)BulletBase.tNetDmgStack.nHP);
				BulletBase.tNetDmgStack.nDmgHP = perBuffManager.SOB.DmgHp;
				BulletBase.tNetDmgStack.nHealHP = perBuffManager.SOB.HealHp;
				BulletBase.tNetDmgStack.nBreakEnergyShieldBuffID = 0;
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(0, 0, JsonConvert.SerializeObject(BulletBase.tNetDmgStack, Formatting.None, new ObscuredValueConverter()), true);
				}
			}
			ShowSkillTableFxSE(tSKILL_TABLE.s_HIT_FX, "null", perBuffManager.SOB.transform);
			break;
		}
		case 3:
			if (refPBMHit != null && refPBMHit.SOB != null)
			{
				refPBMHit.SOB.tRefPassiveskill.AddStatus(tSKILL_TABLE, nSkillLevel, nWeaponCheck);
			}
			break;
		case 4:
			if (refPBMShoter != null && refPBMShoter.SOB != null)
			{
				OrangeCharacter orangeCharacter10 = refPBMShoter.SOB as OrangeCharacter;
				if (orangeCharacter10 != null)
				{
					orangeCharacter10.CDSkill((int)tSKILL_TABLE.f_EFFECT_X - 1);
				}
			}
			break;
		case 7:
			if (refPBMShoter != null && refPBMShoter.SOB != null)
			{
				OrangeCharacter orangeCharacter7 = refPBMShoter.SOB as OrangeCharacter;
				if (orangeCharacter7 != null)
				{
					orangeCharacter7.AddMagazine(nWeaponCheck, (int)tSKILL_TABLE.f_EFFECT_X);
				}
			}
			break;
		case 8:
			if (refPBMShoter != null)
			{
				refPBMShoter.AddMeasure((int)tSKILL_TABLE.f_EFFECT_X);
				CinnamonController component2 = refPBMShoter.SOB.GetComponent<CinnamonController>();
				if (component2 != null)
				{
					component2.PlayMeasureSE((int)tSKILL_TABLE.f_EFFECT_X);
				}
			}
			break;
		case 12:
		case 13:
		case 30:
		{
			if (refPBMHit == null || !(refPBMHit.SOB != null))
			{
				break;
			}
			float num;
			if (tSKILL_TABLE.n_EFFECT == 12)
			{
				num = Mathf.RoundToInt((float)(int)refPBMHit.SOB.MaxHp * tSKILL_TABLE.f_EFFECT_X * 0.01f);
			}
			else if (tSKILL_TABLE.n_EFFECT == 30)
			{
				num = Mathf.RoundToInt((float)(int)refPBMHit.SOB.MaxHp * tSKILL_TABLE.f_EFFECT_X * 0.01f) + Mathf.RoundToInt((float)(int)refPBMHit.SOB.Hp * tSKILL_TABLE.f_EFFECT_Y * 0.01f);
				if (num >= (float)(int)refPBMHit.SOB.Hp - tSKILL_TABLE.f_EFFECT_Z)
				{
					num = Mathf.RoundToInt((float)(int)refPBMHit.SOB.Hp - tSKILL_TABLE.f_EFFECT_Z);
				}
			}
			else
			{
				num = Mathf.RoundToInt((float)((int)refPBMHit.SOB.MaxHp + refPBMHit.sBuffStatus.nEnergyShield) * tSKILL_TABLE.f_EFFECT_X * 0.01f);
			}
			if (!(num > 0f))
			{
				break;
			}
			if ((refPBMHit.SOB.IsInvincible || (bool)refPBMHit.SOB.IsUnBreak() || (bool)refPBMHit.SOB.IsUnBreakX()) && tSKILL_TABLE.n_EFFECT != 13)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, refPBMHit.SOB.GetDamageTextPos(), 0, refPBMHit.SOB.GetSOBLayerMask(), VisualDamage.DamageType.Reduce);
			}
			else
			{
				if (refPBMHit.CheclIgnoreSkill(tSKILL_TABLE) || refPBMHit.SOB.tRefPassiveskill.CheclIgnoreSkill(tSKILL_TABLE, refPBMHit.SOB.GetCurrentWeaponCheck()))
				{
					break;
				}
				BulletBase.tNetDmgStack.nHP = refPBMHit.SOB.Hp;
				BulletBase.tNetDmgStack.nEnergyShield = refPBMHit.sBuffStatus.nEnergyShield;
				HurtPassParam hurtPassParam = new HurtPassParam();
				hurtPassParam.dmg = (int)num;
				BulletBase.tNetDmgStack.nBreakEnergyShieldBuffID = 0;
				refPBMHit.SOB.Hurt(hurtPassParam);
				BulletBase.tNetDmgStack.nHP = (int)refPBMHit.SOB.MaxHp - (int)refPBMHit.SOB.DmgHp + (int)refPBMHit.SOB.HealHp;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, refPBMHit.SOB.GetDamageTextPos(), (int)BulletBase.tNetDmgStack.nHP - (int)refPBMHit.SOB.Hp, refPBMHit.SOB.GetSOBLayerMask(), VisualDamage.DamageType.Normal);
				BulletBase.tNetDmgStack.sPlayerID = refPBMHit.SOB.sNetSerialID;
				BulletBase.tNetDmgStack.sShotPlayerID = text;
				BulletBase.tNetDmgStack.nDmg = (int)num;
				BulletBase.tNetDmgStack.nRecordID = refPBMHit.SOB.GetNowRecordNO();
				BulletBase.tNetDmgStack.nNetID = 0;
				BulletBase.tNetDmgStack.sOwner = "STO";
				BulletBase.tNetDmgStack.nSubPartID = 0;
				BulletBase.tNetDmgStack.nDamageType = 0;
				BulletBase.tNetDmgStack.nWeaponType = 0;
				BulletBase.tNetDmgStack.nWeaponCheck = 0;
				BulletBase.tNetDmgStack.nEndHP = refPBMHit.SOB.Hp;
				StageObjBase sOB = refPBMHit.SOB;
				sOB.DmgHp = (int)sOB.DmgHp + ((int)BulletBase.tNetDmgStack.nHP - (int)refPBMHit.SOB.Hp);
				BulletBase.tNetDmgStack.nDmgHP = refPBMHit.SOB.DmgHp;
				BulletBase.tNetDmgStack.nHealHP = refPBMHit.SOB.HealHp;
				BulletBase.tNetDmgStack.nSkillID = 0;
				if (refPBMHit.SOB.sNetSerialID != text)
				{
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerDMG(text, BulletBase.tNetDmgStack.nDmg, (int)BulletBase.tNetDmgStack.nHP - (int)BulletBase.tNetDmgStack.nEndHP, refPBMHit.SOB as OrangeCharacter != null);
				}
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(0, 0, JsonConvert.SerializeObject(BulletBase.tNetDmgStack, Formatting.None, new ObscuredValueConverter()), true);
				}
			}
			ShowSkillTableFxSE(tSKILL_TABLE.s_HIT_FX, tSKILL_TABLE.s_HIT_SE, refPBMHit.SOB.transform);
			break;
		}
		case 10:
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.UpdateCountScoreUI((int)tSKILL_TABLE.f_EFFECT_Y, Mathf.CeilToInt(tSKILL_TABLE.f_EFFECT_X));
			}
			if (refPBMHit != null)
			{
				ShowSkillTableFxSE(tSKILL_TABLE.s_HIT_FX, tSKILL_TABLE.s_HIT_SE, refPBMHit.SOB.transform);
			}
			break;
		case 16:
		{
			if (refPBMShoter == null || !(refPBMShoter.SOB != null))
			{
				break;
			}
			OrangeCharacter orangeCharacter6 = refPBMShoter.SOB as OrangeCharacter;
			if (orangeCharacter6 != null)
			{
				CharacterControlBase component = orangeCharacter6.GetComponent<CharacterControlBase>();
				if ((bool)component)
				{
					component.CallPet((int)tSKILL_TABLE.f_EFFECT_X, false, -1, null);
				}
			}
			break;
		}
		case 18:
			if (tSKILL_TABLE.n_TARGET == 3)
			{
				if (refPBMShoter != null)
				{
					OrangeCharacter orangeCharacter4 = refPBMShoter.SOB as OrangeCharacter;
					if ((bool)orangeCharacter4)
					{
						refPBMShoter.RemoveBuffByBullet(tSKILL_TABLE, orangeCharacter4.sNetSerialID, PerBuffManager.CHECH_BUFF_TYPE.BUFF);
					}
					else
					{
						refPBMShoter.RemoveBuffByBullet(tSKILL_TABLE, string.Empty, PerBuffManager.CHECH_BUFF_TYPE.BUFF);
					}
				}
			}
			else if (refPBMHit != null)
			{
				OrangeCharacter orangeCharacter5 = refPBMHit.SOB as OrangeCharacter;
				if ((bool)orangeCharacter5)
				{
					refPBMHit.RemoveBuffByBullet(tSKILL_TABLE, orangeCharacter5.sNetSerialID, PerBuffManager.CHECH_BUFF_TYPE.BUFF);
				}
				else
				{
					refPBMHit.RemoveBuffByBullet(tSKILL_TABLE, string.Empty, PerBuffManager.CHECH_BUFF_TYPE.BUFF);
				}
			}
			break;
		case 19:
			if (tSKILL_TABLE.n_TARGET == 3)
			{
				if (refPBMShoter != null)
				{
					OrangeCharacter orangeCharacter8 = refPBMShoter.SOB as OrangeCharacter;
					if ((bool)orangeCharacter8)
					{
						refPBMShoter.RemoveBuffByBullet(tSKILL_TABLE, orangeCharacter8.sNetSerialID, PerBuffManager.CHECH_BUFF_TYPE.BUFF);
					}
					else
					{
						refPBMShoter.RemoveBuffByBullet(tSKILL_TABLE, string.Empty, PerBuffManager.CHECH_BUFF_TYPE.BUFF);
					}
				}
			}
			else if (refPBMHit != null)
			{
				OrangeCharacter orangeCharacter9 = refPBMHit.SOB as OrangeCharacter;
				if ((bool)orangeCharacter9)
				{
					refPBMHit.RemoveBuffByBullet(tSKILL_TABLE, orangeCharacter9.sNetSerialID, PerBuffManager.CHECH_BUFF_TYPE.BUFF);
				}
				else
				{
					refPBMHit.RemoveBuffByBullet(tSKILL_TABLE, string.Empty, PerBuffManager.CHECH_BUFF_TYPE.BUFF);
				}
			}
			break;
		case 20:
			if (tSKILL_TABLE.n_TARGET == 3)
			{
				if (refPBMShoter != null)
				{
					OrangeCharacter orangeCharacter2 = refPBMShoter.SOB as OrangeCharacter;
					if ((bool)orangeCharacter2)
					{
						refPBMShoter.RemoveBuffByBullet(tSKILL_TABLE, orangeCharacter2.sNetSerialID, PerBuffManager.CHECH_BUFF_TYPE.DEBUFF);
					}
					else
					{
						refPBMShoter.RemoveBuffByBullet(tSKILL_TABLE, string.Empty, PerBuffManager.CHECH_BUFF_TYPE.DEBUFF);
					}
				}
			}
			else if (refPBMHit != null)
			{
				OrangeCharacter orangeCharacter3 = refPBMHit.SOB as OrangeCharacter;
				if ((bool)orangeCharacter3)
				{
					refPBMHit.RemoveBuffByBullet(tSKILL_TABLE, orangeCharacter3.sNetSerialID, PerBuffManager.CHECH_BUFF_TYPE.DEBUFF);
				}
				else
				{
					refPBMHit.RemoveBuffByBullet(tSKILL_TABLE, string.Empty, PerBuffManager.CHECH_BUFF_TYPE.DEBUFF);
				}
			}
			break;
		case 26:
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.AddStageTimerTime(tSKILL_TABLE.f_EFFECT_X);
			}
			break;
		case 28:
			if (tSKILL_TABLE.n_TARGET == 3 && refPBMShoter != null)
			{
				refPBMShoter.AddBuffAndApplyCurrentBuffStatus(tSKILL_TABLE);
			}
			else if (refPBMHit != null)
			{
				refPBMHit.AddBuffAndApplyCurrentBuffStatus(tSKILL_TABLE);
			}
			break;
		case 29:
			if (tSKILL_TABLE.n_TARGET == 3 && refPBMShoter != null)
			{
				if (refPBMShoter.CheckHasEffectByCONDITIONID((int)tSKILL_TABLE.f_EFFECT_X))
				{
					refPBMShoter.BuffStackDown((int)tSKILL_TABLE.f_EFFECT_X, (int)tSKILL_TABLE.f_EFFECT_Y);
				}
			}
			else if (refPBMHit != null)
			{
				refPBMHit.BuffStackDown((int)tSKILL_TABLE.f_EFFECT_X, (int)tSKILL_TABLE.f_EFFECT_Y);
			}
			break;
		case 31:
			if (refPBMShoter != null && refPBMShoter.SOB != null)
			{
				OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
				if (orangeCharacter != null)
				{
					orangeCharacter.CDSkillEx((int)tSKILL_TABLE.f_EFFECT_X - 1, (int)tSKILL_TABLE.f_EFFECT_Y);
				}
			}
			break;
		case 5:
		case 6:
		case 9:
		case 11:
		case 14:
		case 15:
		case 17:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 27:
			break;
		}
	}

	public static void TriggerSkillEx(ref PassiveskillStatus tPassiveskillStatus, int nWeaponCheck, PerBuffManager refPBMShoter, PerBuffManager refPBMHit, int nAtkParam)
	{
		int n_EFFECT = tPassiveskillStatus.tSKILL_TABLE.n_EFFECT;
		if (n_EFFECT == 23)
		{
			if (refPBMHit != null && refPBMShoter != null)
			{
				StageObjBase sOB = refPBMHit.SOB;
				int num = Mathf.RoundToInt(tPassiveskillStatus.tSKILL_TABLE.f_EFFECT_Z);
				if (num == 0)
				{
					int nMaxCount = 999;
					refPBMHit.StealBuffByBullet(tPassiveskillStatus.tSKILL_TABLE, refPBMShoter, refPBMHit, ref nMaxCount);
				}
				else if (num > tPassiveskillStatus.nExtendCount)
				{
					int nMaxCount2 = num - tPassiveskillStatus.nExtendCount;
					refPBMHit.StealBuffByBullet(tPassiveskillStatus.tSKILL_TABLE, refPBMShoter, refPBMHit, ref nMaxCount2);
					tPassiveskillStatus.nExtendCount = num - nMaxCount2;
				}
			}
		}
		else
		{
			TriggerSkill(tPassiveskillStatus.tSKILL_TABLE, tPassiveskillStatus.nSkillLevel, nWeaponCheck, refPBMShoter, refPBMHit, nAtkParam);
		}
	}

	private static void ShowSkillTableFxSE(string hitfx, string se, Transform transform)
	{
		string[] array = null;
		StageObjBase component = transform.GetComponent<StageObjBase>();
		if (se != "null")
		{
			array = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(se);
			if ((bool)component)
			{
				component.PlaySE(array[0], array[1]);
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(4, 10, component.sNetSerialID + "," + array[0] + "," + array[0], true);
				}
			}
		}
		if (!(hitfx != "null"))
		{
			return;
		}
		if ((bool)component)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(hitfx, component.AimPosition, Quaternion.identity, Array.Empty<object>());
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(4, 11, component.sNetSerialID + "," + hitfx, true);
			}
		}
		else
		{
			IAimTarget componentInParent = transform.GetComponentInParent<IAimTarget>();
			if (componentInParent != null)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(hitfx, componentInParent.AimTransform.position + componentInParent.AimPosition, Quaternion.identity, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(hitfx, transform, Quaternion.identity, Array.Empty<object>());
			}
		}
	}

	public void PreloadAllPassiveskill(Action<SKILL_TABLE> LoadSkillBulletCB, Action<string> LoadBuffFxCB)
	{
		if (LoadSkillBulletCB != null)
		{
			for (int i = 0; i < listPassiveskill.Count; i++)
			{
				if (listPassiveskill[i].tSKILL_TABLE.s_MODEL != "null")
				{
					LoadSkillBulletCB(listPassiveskill[i].tSKILL_TABLE);
				}
			}
			for (int j = 0; j < listUsePassiveskill.Count; j++)
			{
				if (listUsePassiveskill[j].tSKILL_TABLE.s_MODEL != "null")
				{
					LoadSkillBulletCB(listUsePassiveskill[j].tSKILL_TABLE);
				}
			}
			for (int k = 0; k < listHitPassiveskill.Count; k++)
			{
				if (listHitPassiveskill[k].tSKILL_TABLE.s_MODEL != "null")
				{
					LoadSkillBulletCB(listHitPassiveskill[k].tSKILL_TABLE);
				}
			}
			for (int l = 0; l < listHurtPassiveskill.Count; l++)
			{
				if (listHurtPassiveskill[l].tSKILL_TABLE.s_MODEL != "null")
				{
					LoadSkillBulletCB(listHurtPassiveskill[l].tSKILL_TABLE);
				}
			}
			for (int m = 0; m < listEquipPassiveskill.Count; m++)
			{
				if (listEquipPassiveskill[m].tSKILL_TABLE.s_MODEL == "p_DUMMY")
				{
					LoadSkillBulletCB(listEquipPassiveskill[m].tSKILL_TABLE);
				}
			}
		}
		if (LoadBuffFxCB != null)
		{
			CONDITION_TABLE value = null;
			for (int n = 0; n < listPassiveskill.Count; n++)
			{
				if (listPassiveskill[n].tSKILL_TABLE.n_CONDITION_ID != 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(listPassiveskill[n].tSKILL_TABLE.n_CONDITION_ID, out value))
				{
					LoadBuffFxCB(value.s_DURING_FX);
					LoadBuffFxCB(value.s_HIT_FX);
					PreloadSkillBulletbyCondition(LoadSkillBulletCB, value);
				}
			}
			for (int num = 0; num < listUsePassiveskill.Count; num++)
			{
				if (listUsePassiveskill[num].tSKILL_TABLE.n_CONDITION_ID != 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(listUsePassiveskill[num].tSKILL_TABLE.n_CONDITION_ID, out value))
				{
					LoadBuffFxCB(value.s_DURING_FX);
					LoadBuffFxCB(value.s_HIT_FX);
					PreloadSkillBulletbyCondition(LoadSkillBulletCB, value);
				}
			}
			for (int num2 = 0; num2 < listHitPassiveskill.Count; num2++)
			{
				if (listHitPassiveskill[num2].tSKILL_TABLE.n_CONDITION_ID != 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(listHitPassiveskill[num2].tSKILL_TABLE.n_CONDITION_ID, out value))
				{
					LoadBuffFxCB(value.s_DURING_FX);
					LoadBuffFxCB(value.s_HIT_FX);
					PreloadSkillBulletbyCondition(LoadSkillBulletCB, value);
				}
			}
			for (int num3 = 0; num3 < listHurtPassiveskill.Count; num3++)
			{
				if (listHurtPassiveskill[num3].tSKILL_TABLE.n_CONDITION_ID != 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(listHurtPassiveskill[num3].tSKILL_TABLE.n_CONDITION_ID, out value))
				{
					LoadBuffFxCB(value.s_DURING_FX);
					LoadBuffFxCB(value.s_HIT_FX);
					PreloadSkillBulletbyCondition(LoadSkillBulletCB, value);
				}
			}
		}
		PreloadPassiveskillBullet(ref listPerSecPassiveSkill, LoadSkillBulletCB, LoadBuffFxCB);
		PreloadPassiveskillBullet(ref listUseKeyPassiveskill, LoadSkillBulletCB, LoadBuffFxCB);
		PreloadPassiveskillBullet(ref listBuffPassiveskill, LoadSkillBulletCB, LoadBuffFxCB);
	}

	private void PreloadPassiveskillBullet(ref List<PassiveskillStatus> listPassiveskill, Action<SKILL_TABLE> LoadSkillBulletCB, Action<string> LoadBuffFxCB)
	{
		if (LoadSkillBulletCB != null)
		{
			for (int i = 0; i < listPassiveskill.Count; i++)
			{
				if (listPassiveskill[i].tSKILL_TABLE.s_MODEL != "null")
				{
					LoadSkillBulletCB(listPassiveskill[i].tSKILL_TABLE);
				}
			}
		}
		if (LoadBuffFxCB == null)
		{
			return;
		}
		CONDITION_TABLE value = null;
		for (int j = 0; j < listPassiveskill.Count; j++)
		{
			if (listPassiveskill[j].tSKILL_TABLE.n_CONDITION_ID != 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(listPassiveskill[j].tSKILL_TABLE.n_CONDITION_ID, out value))
			{
				LoadBuffFxCB(value.s_DURING_FX);
				LoadBuffFxCB(value.s_HIT_FX);
			}
		}
	}

	private void PreloadSkillBulletbyCondition(Action<SKILL_TABLE> LoadSkillBulletCB, CONDITION_TABLE tCONDITION_TABLE)
	{
		SKILL_TABLE value;
		if (tCONDITION_TABLE.n_EFFECT == 11 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue((int)tCONDITION_TABLE.f_EFFECT_X, out value))
		{
			LoadSkillBulletCB(value);
		}
	}

	public bool CheckPreventBuffPS(int debuffEffectId)
	{
		bool flag = false;
		int num = 0;
		for (int i = 0; i < listPerventbuffPassiveskill.Count; i++)
		{
			if (listPerventbuffPassiveskill[i].tSKILL_TABLE.n_TRIGGER_X == debuffEffectId)
			{
				flag = true;
				num += listPerventbuffPassiveskill[i].tSKILL_TABLE.n_TRIGGER_RATE;
			}
		}
		if (flag && num >= OrangeBattleUtility.Random(0, 10000))
		{
			return true;
		}
		return false;
	}

	public bool CheckSOBType(StageObjBase sob, int type)
	{
		if (sob == null)
		{
			return false;
		}
		bool result = false;
		switch (type)
		{
		case 0:
			result = true;
			break;
		case 1:
			if (sob.GetSOBType() == 2)
			{
				EnemyControllerBase enemyControllerBase = sob as EnemyControllerBase;
				if (enemyControllerBase.EnemyData != null && ManagedSingleton<OrangeTableHelper>.Instance.IsBossSP(enemyControllerBase.EnemyData))
				{
					result = true;
				}
			}
			break;
		case 2:
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && sob.GetSOBType() == 1)
			{
				result = true;
			}
			break;
		}
		return result;
	}

	public bool HaveFlyMode()
	{
		for (int i = 0; i < listAddStatusPassiveskill.Count; i++)
		{
			if (listAddStatusPassiveskill[i].tSKILL_TABLE.n_EFFECT == 27)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetFlyParam(out SKILL_TABLE skillTable)
	{
		for (int i = 0; i < listAddStatusPassiveskill.Count; i++)
		{
			if (listAddStatusPassiveskill[i].tSKILL_TABLE.n_EFFECT == 27)
			{
				skillTable = listAddStatusPassiveskill[i].tSKILL_TABLE;
				return true;
			}
		}
		skillTable = null;
		return false;
	}

	public void UseKeyTrigger(ButtonId buttonId, WeaponStatus tWeaponStatus, ref PerBuffManager selfBuffManager, Action<SKILL_TABLE> pcb)
	{
		int count = listUseKeyPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listUseKeyPassiveskill[i];
			OrangeCharacter orangeCharacter = selfBuffManager.SOB as OrangeCharacter;
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, tWeaponStatus.nWeaponCheck, tWeaponStatus.nWeaponCheck, 0, fNowTotalLeftTime))
			{
				continue;
			}
			int num = 1 << (int)buttonId;
			if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X & num) == 0 || ((bool)orangeCharacter && orangeCharacter.UsingVehicle && passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0)
			{
				if (buttonId != ButtonId.JUMP || orangeCharacter.GetJumpCount() <= 0)
				{
					continue;
				}
				int num2 = 1 << orangeCharacter.GetJumpCount() - 1;
				if ((passiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z & num2) == 0)
				{
					continue;
				}
			}
			if (fNowTotalLeftTime - passiveskillStatus.fLastKeyTriggerTime < 0.09f)
			{
				continue;
			}
			passiveskillStatus.fLastKeyTriggerTime = fNowTotalLeftTime;
			if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE < UnityEngine.Random.Range(0, 10000))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2)
			{
				selfBuffManager.AddMeasure(-passiveskillStatus.tSKILL_TABLE.n_USE_COST, true);
				CinnamonController component = selfBuffManager.SOB.GetComponent<CinnamonController>();
				if (component != null)
				{
					component.PlayMeasureSE(-passiveskillStatus.tSKILL_TABLE.n_USE_COST);
				}
			}
			StanrdTriggerPassiveskillNoCheck(passiveskillStatus, tWeaponStatus, selfBuffManager, pcb);
		}
	}

	public void DelBuffTrigger(int buffId, int triggerType, ref PerBuffManager selfBuffManager)
	{
		OrangeCharacter orangeCharacter = selfBuffManager.SOB as OrangeCharacter;
		if (orangeCharacter == null || orangeCharacter.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify || (int)orangeCharacter.Hp <= 0 || buffId <= 0)
		{
			return;
		}
		CONDITION_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(buffId, out value))
		{
			return;
		}
		int count = listBuffPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		int num = orangeCharacter.GetCurrentWeaponCheck();
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listBuffPassiveskill[i];
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, num, num, 0, fNowTotalLeftTime))
			{
				continue;
			}
			int n_TRIGGER = passiveskillStatus.tSKILL_TABLE.n_TRIGGER;
			if (n_TRIGGER != 27 || !CheckTriggerBuffGone(triggerType, ref value, ref passiveskillStatus, ref selfBuffManager) || !CheckTriggerGeneral(ref passiveskillStatus, ref selfBuffManager))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2)
			{
				selfBuffManager.AddMeasure(-passiveskillStatus.tSKILL_TABLE.n_USE_COST, true);
				CinnamonController component = selfBuffManager.SOB.GetComponent<CinnamonController>();
				if (component != null)
				{
					component.PlayMeasureSE(-passiveskillStatus.tSKILL_TABLE.n_USE_COST);
				}
			}
			StanrdTriggerPassiveskillNoCheck(passiveskillStatus, orangeCharacter.GetCurrentWeaponObj().weaponStatus, selfBuffManager, orangeCharacter.CreateBulletByLastWSTranform);
		}
	}

	public void PetDeactiveTrigger(int petId, int triggerType, WeaponStatus tWeaponStatus, ref PerBuffManager selfBuffManager, Action<SKILL_TABLE> pcb)
	{
		OrangeCharacter orangeCharacter = selfBuffManager.SOB as OrangeCharacter;
		if (orangeCharacter == null || orangeCharacter.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify || (int)orangeCharacter.Hp <= 0 || petId <= 0)
		{
			return;
		}
		int count = listEquipPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		int num = tWeaponStatus.nWeaponCheck;
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listEquipPassiveskill[i];
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, num, num, 0, fNowTotalLeftTime))
			{
				continue;
			}
			int n_TRIGGER = passiveskillStatus.tSKILL_TABLE.n_TRIGGER;
			if (n_TRIGGER != 29 || !CheckTriggerPetDeactive(petId, triggerType, ref passiveskillStatus, ref selfBuffManager) || !CheckTriggerGeneral(ref passiveskillStatus, ref selfBuffManager))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2)
			{
				selfBuffManager.AddMeasure(-passiveskillStatus.tSKILL_TABLE.n_USE_COST, true);
				CinnamonController component = selfBuffManager.SOB.GetComponent<CinnamonController>();
				if (component != null)
				{
					component.PlayMeasureSE(-passiveskillStatus.tSKILL_TABLE.n_USE_COST);
				}
			}
			StanrdTriggerPassiveskillNoCheck(passiveskillStatus, orangeCharacter.GetCurrentWeaponObj().weaponStatus, selfBuffManager, pcb);
		}
	}

	public bool LineSkillTrigger(float distance, int nWeaponCheck, PerBuffManager refPBMShoter, PerBuffManager targetBuffManager, Action<SKILL_TABLE> pcb)
	{
		bool result = false;
		OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
		if (orangeCharacter == null)
		{
			return false;
		}
		if (orangeCharacter.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify || (int)orangeCharacter.Hp <= 0)
		{
			return false;
		}
		int count = listEquipPassiveskill.Count;
		PassiveskillStatus passiveskillStatus = null;
		for (int i = 0; i < count; i++)
		{
			passiveskillStatus = listEquipPassiveskill[i];
			if (!passiveskillStatus.CheckCanUse(bUsePassiveskill, nWeaponCheck, nWeaponCheck, 0, fNowTotalLeftTime))
			{
				continue;
			}
			int n_TRIGGER = passiveskillStatus.tSKILL_TABLE.n_TRIGGER;
			if (n_TRIGGER != 30 || !CheckTriggerLineSkill(distance, ref passiveskillStatus, ref refPBMShoter) || !CheckTriggerGeneral(ref passiveskillStatus, ref refPBMShoter))
			{
				continue;
			}
			if (passiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2)
			{
				refPBMShoter.AddMeasure(-passiveskillStatus.tSKILL_TABLE.n_USE_COST, true);
				CinnamonController component = refPBMShoter.SOB.GetComponent<CinnamonController>();
				if (component != null)
				{
					component.PlayMeasureSE(-passiveskillStatus.tSKILL_TABLE.n_USE_COST);
				}
			}
			WeaponStatus weaponStatus = orangeCharacter.GetCurrentWeaponObj().weaponStatus;
			if (passiveskillStatus.tSKILL_TABLE.n_TARGET == 1)
			{
				passiveskillStatus.UpdateUseTime(fNowTotalLeftTime);
				bool flag = false;
				bool flag2 = false;
				if (targetBuffManager != null)
				{
					flag = targetBuffManager.CheckHasEffect(7);
					flag2 = targetBuffManager.CheckHasEffect(9);
				}
				if (!flag2)
				{
					TriggerBuff(passiveskillStatus.tSKILL_TABLE, targetBuffManager, refPBMShoter, weaponStatus.nATK, 0, passiveskillStatus.tSKILL_TABLE.n_ID);
				}
				if (passiveskillStatus.tSKILL_TABLE.n_TYPE != 0)
				{
					if (passiveskillStatus.tSKILL_TABLE.s_MODEL != "null" && pcb != null)
					{
						pcb(passiveskillStatus.tSKILL_TABLE);
					}
				}
				else if (!flag && !flag2)
				{
					TriggerSkill(passiveskillStatus.tSKILL_TABLE, passiveskillStatus.nSkillLevel, nWeaponCheck, refPBMShoter, targetBuffManager, weaponStatus.nATK);
				}
			}
			else
			{
				StanrdTriggerPassiveskillNoCheck(passiveskillStatus, weaponStatus, refPBMShoter, pcb);
			}
			if (passiveskillStatus.tSKILL_TABLE.n_TRIGGER_X == 1)
			{
				result = true;
			}
		}
		return result;
	}

	protected bool CheckTriggerGeneral(ref PassiveskillStatus tPassiveskillStatus, ref PerBuffManager selfBuffManager)
	{
		if (tPassiveskillStatus == null || selfBuffManager == null)
		{
			return false;
		}
		if (tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_RATE < UnityEngine.Random.Range(0, 10000))
		{
			return false;
		}
		int nMeasureNow = selfBuffManager.nMeasureNow;
		if (tPassiveskillStatus.tSKILL_TABLE.n_MAGAZINE_TYPE == 2 && (nMeasureNow < tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z || selfBuffManager.nMeasureNow < tPassiveskillStatus.tSKILL_TABLE.n_USE_COST))
		{
			return false;
		}
		return true;
	}

	protected bool CheckTriggerBuffGone(int triggerType, ref CONDITION_TABLE tConditionTable, ref PassiveskillStatus tPassiveskillStatus, ref PerBuffManager selfBuffManager)
	{
		if (tConditionTable == null || tPassiveskillStatus == null || selfBuffManager == null)
		{
			return false;
		}
		if (tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && tConditionTable.n_EFFECT != tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_X)
		{
			return false;
		}
		if (tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0 && tConditionTable.n_ID != tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y)
		{
			return false;
		}
		if (tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0)
		{
			if (triggerType <= 0)
			{
				return false;
			}
			int num = 1 << triggerType - 1;
			if ((tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z & num) == 0)
			{
				return false;
			}
		}
		return true;
	}

	protected bool CheckTriggerPetDeactive(int petId, int triggerType, ref PassiveskillStatus tPassiveskillStatus, ref PerBuffManager selfBuffManager)
	{
		if (tPassiveskillStatus == null || selfBuffManager == null)
		{
			return false;
		}
		if (tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != 0 && tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_X != petId)
		{
			return false;
		}
		if (tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y != 0)
		{
			if (triggerType <= 0)
			{
				return false;
			}
			int num = 1 << triggerType - 1;
			if ((tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y & num) == 0)
			{
				return false;
			}
		}
		if (tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z != 0)
		{
			return false;
		}
		return true;
	}

	protected bool CheckTriggerLineSkill(float distance, ref PassiveskillStatus tPassiveskillStatus, ref PerBuffManager selfBuffManager)
	{
		if (tPassiveskillStatus == null || selfBuffManager == null || distance < 0f)
		{
			return false;
		}
		if (distance < (float)tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Y || distance >= (float)tPassiveskillStatus.tSKILL_TABLE.n_TRIGGER_Z)
		{
			return false;
		}
		return true;
	}

	public bool IsSpecialCount(out SKILL_TABLE sklTable)
	{
		for (int num = listAddStatusPassiveskill.Count - 1; num >= 0; num--)
		{
			SKILL_TABLE tSKILL_TABLE = listAddStatusPassiveskill[num].tSKILL_TABLE;
			if (tSKILL_TABLE.n_USE_TYPE == 103 && tSKILL_TABLE.n_TRIGGER_X > 0 && tSKILL_TABLE.n_TRIGGER_Y > 0)
			{
				sklTable = tSKILL_TABLE;
				return true;
			}
		}
		sklTable = null;
		return false;
	}

	public void NetUpdateUseTime(int ID)
	{
		foreach (PassiveskillStatus item in listHitPassiveskill)
		{
			if (item.tSKILL_TABLE.n_ID == ID)
			{
				item.UpdateUseTime(fNowTotalLeftTime);
				break;
			}
		}
	}
}
