using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CallbackDefs;
using UnityEngine;
using enums;

public class StageHelper : ManagedSingleton<StageHelper>
{
	public enum StageJoinCondition
	{
		NONE = 0,
		AP = 1,
		RANK = 2,
		PRE = 3,
		COUNT = 4,
		TIME = 5,
		CHEATBANED = 6
	}

	public enum STAGE_END_GO
	{
		HOMETOP = 0,
		STORYSTAGESELECT = 1,
		COOPSTAGESELECT = 2,
		PVPROOMSELECT = 3,
		BOSSCHALLENGE = 4,
		GACHA = 5,
		LOGIN_BONUS = 6,
		PVPRANDOMMATCHING = 7,
		GUIDE = 8,
		SEASONRANDOMMATCHING = 9,
		SEASON = 10,
		ACTIVITYEVENT = 11,
		WOLRDBOSS = 12,
		CRUSADE = 13,
		FRIENDPVPCREATEPRIVATEROOM = 14,
		FRIENDPVPJOINPRIVATEROOM = 15,
		TOTALWAR = 16
	}

	public enum STAGE_RULE_STATUS
	{
		HP = 0,
		ATK = 1,
		DEF = 2,
		CRI = 3,
		CRI_RESIST = 4,
		CRIDMG = 5,
		CRIDMG_RESIST = 6,
		DODGE = 7,
		HIT = 8,
		PARRY = 9,
		PARRY_RESIST = 10,
		PARRY_DEF = 11,
		SKILL_LV = 12
	}

	public struct StageCharacterStruct
	{
		public int MainWeaponID;

		public int SubWeaponID;

		public int StandbyChara;

		public ushort MainWeaponChipID;

		public ushort SubWeaponChipID;

		public int MainWeaponFSID;

		public int SubWeaponFSID;

		public int Skin;

		public List<NetCharacterSkillInfo> listNetCharacterSkillInfos;
	}

	public bool bEnemyActive = true;

	public int nLastStageID;

	public int nLastStageRuleID;

	public int nLastStageRuleID_Status;

	public int nLastOCPower;

	public float fCameraHHalf = 10f;

	public float fCameraWHalf = 10f;

	public STAGE_END_GO nStageEndGoUI;

	public object[] nStageEndParam;

	public StageClearStar eLastStageClearStart = StageClearStar.First;

	public StageResult eLastStageResult = StageResult.Win;

	public List<int> ListAchievedMissionID = new List<int>();

	public int activityEventStageMainID;

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public List<NetStageInfo> GetListNetStageByMainId(int p_mainId, bool p_sort = false)
	{
		List<NetStageInfo> list = new List<NetStageInfo>();
		foreach (StageInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicStage.Values)
		{
			STAGE_TABLE stage = null;
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetStage(value.netStageInfo.StageID, out stage) && stage.n_MAIN == p_mainId)
			{
				list.Add(value.netStageInfo);
			}
		}
		if (p_sort)
		{
			list.Sort((NetStageInfo x, NetStageInfo y) => x.StageID.CompareTo(y.StageID));
		}
		return list;
	}

	public List<NetStageInfo> GetListNetStageByType(StageType stageType, bool p_sort = false)
	{
		List<NetStageInfo> list = new List<NetStageInfo>();
		foreach (StageInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicStage.Values)
		{
			STAGE_TABLE stage = null;
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetStage(value.netStageInfo.StageID, out stage) && stage.n_TYPE == (int)stageType)
			{
				list.Add(value.netStageInfo);
			}
		}
		if (p_sort)
		{
			list.Sort((NetStageInfo x, NetStageInfo y) => x.StageID.CompareTo(y.StageID));
		}
		return list;
	}

	public int GetCoopRewardCount()
	{
		int num = 0;
		foreach (NetStageInfo item in GetListNetStageByType(StageType.TeamUp))
		{
			num += item.ClearCount;
		}
		return Mathf.Clamp(OrangeConst.CORP_REWARD_COUNT - num, 0, OrangeConst.CORP_REWARD_COUNT);
	}

	public int GetStarAmount(int star)
	{
		int num = 0;
		int num2 = 7;
		for (int i = 0; i < num2; i++)
		{
			if ((star & (1 << i)) != 0)
			{
				num++;
			}
		}
		return num;
	}

	public int GetAvailableChallengeCount(STAGE_TABLE stageTable)
	{
		StageInfo value = null;
		if (stageTable.n_PLAY_COUNT != -1)
		{
			if (stageTable.n_TYPE == 2)
			{
				int num = 0;
				foreach (STAGE_TABLE item in ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_TYPE == 2 && x.n_MAIN == stageTable.n_MAIN).ToList())
				{
					if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(item.n_ID, out value))
					{
						num += value.netStageInfo.ClearCount;
					}
				}
				if (stageTable.n_PLAY_COUNT <= num)
				{
					return 0;
				}
				return stageTable.n_PLAY_COUNT - num;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(stageTable.n_ID, out value))
			{
				int clearCount = value.netStageInfo.ClearCount;
				if (stageTable.n_PLAY_COUNT <= clearCount)
				{
					return 0;
				}
				return stageTable.n_PLAY_COUNT - clearCount;
			}
		}
		return 0;
	}

	public bool IsStageConditionOK(STAGE_TABLE stageTable, ref StageJoinCondition condition, int count = 1)
	{
		if (stageTable == null)
		{
			condition = StageJoinCondition.NONE;
			return false;
		}
		if (stageTable.n_RANK > ManagedSingleton<PlayerHelper>.Instance.GetLV())
		{
			condition = StageJoinCondition.RANK;
			return false;
		}
		StageInfo value = null;
		int result = 0;
		int.TryParse(stageTable.s_PRE, out result);
		if (result != 0 && !ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(result, out value))
		{
			condition = StageJoinCondition.PRE;
			return false;
		}
		if (stageTable.n_PLAY_COUNT != -1)
		{
			if (stageTable.n_TYPE == 2)
			{
				int num = 0;
				foreach (STAGE_TABLE item in ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_TYPE == 2 && x.n_MAIN == stageTable.n_MAIN).ToList())
				{
					if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(item.n_ID, out value))
					{
						num += value.netStageInfo.ClearCount;
					}
				}
				if (stageTable.n_PLAY_COUNT - num <= 0)
				{
					condition = StageJoinCondition.COUNT;
					return false;
				}
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(stageTable.n_ID, out value) && stageTable.n_PLAY_COUNT - value.netStageInfo.ClearCount <= 0)
			{
				condition = StageJoinCondition.COUNT;
				return false;
			}
		}
		if (stageTable.n_TYPE != 8)
		{
			int num2 = ((stageTable.n_TYPE == 4) ? ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() : ManagedSingleton<PlayerHelper>.Instance.GetStamina());
			if (stageTable.n_AP * count > num2)
			{
				condition = StageJoinCondition.AP;
				return false;
			}
		}
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(stageTable.s_BEGIN_TIME, stageTable.s_END_TIME, serverUnixTimeNowUTC))
		{
			condition = StageJoinCondition.TIME;
			return false;
		}
		if (stageTable.n_TYPE == 5 && ManagedSingleton<PlayerHelper>.Instance.GetUseCheatPlugIn())
		{
			condition = StageJoinCondition.CHEATBANED;
			return false;
		}
		return true;
	}

	public void DisplayConditionInfo(STAGE_TABLE stage, StageJoinCondition condition, Callback p_cb = null)
	{
		switch (condition)
		{
		case StageJoinCondition.AP:
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				string p_descKey = "STAMINA_OUT";
				ChargeType type = ChargeType.ActionPoint;
				if (stage.n_TYPE == 4)
				{
					type = ChargeType.EventActionPoint;
					p_descKey = "EVENT_STAMINA_OUT";
				}
				ui.SetupYesNoByKey("COMMON_TIP", p_descKey, "COMMON_OK", "COMMON_CANCEL", delegate
				{
					ui.CloseSE = SystemSE.NONE;
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI newUI)
					{
						newUI.Setup(type);
						newUI.closeCB = p_cb;
					});
				});
			});
			break;
		case StageJoinCondition.RANK:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), stage.n_RANK.ToString());
				tipUI.Setup(p_msg, true);
			});
			break;
		case StageJoinCondition.PRE:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				STAGE_TABLE stage2 = null;
				string p_msg2 = string.Empty;
				int result = 0;
				int.TryParse(stage.s_PRE, out result);
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetStage(result, out stage2))
				{
					p_msg2 = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PRE_STAGE"), ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(stage2.w_NAME));
				}
				tipUI.Setup(p_msg2, true);
			});
			break;
		case StageJoinCondition.COUNT:
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupYesNoByKey("COMMON_TIP", "PLAY_COUNT_OUT_CHARGE", "COMMON_OK", "COMMON_CANCEL", delegate
				{
					ui.CloseSE = SystemSE.NONE;
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI newUI)
					{
						newUI.Setup(ChargeType.BossChallenge, stage.n_ID);
						newUI.closeCB = p_cb;
					});
				});
			});
			break;
		case StageJoinCondition.TIME:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_STAGE_TIME");
				tipUI.Setup(str, true);
			});
			break;
		case StageJoinCondition.CHEATBANED:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str2 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PLUGIN_BANSTAGENO");
				tipUI.Setup(str2, true);
			});
			break;
		}
	}

	public string[] GetStageClearMsg(STAGE_TABLE p_stage)
	{
		List<string> list = new List<string>();
		string[] array = new string[3]
		{
			p_stage.n_CLEAR_VALUE1.ToString(),
			p_stage.n_CLEAR_VALUE2.ToString(),
			p_stage.n_CLEAR_VALUE3.ToString()
		};
		StageUiData stageUiData = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj.GetStageUiData(p_stage);
		for (int i = 0; i < 3; i++)
		{
			if (stageUiData != null && stageUiData.sStarGoalKey != null && stageUiData.sStarGoalKey.Length > i && stageUiData.sStarGoalKey[i] != "")
			{
				array[i] = GetLocalString(stageUiData.sStarGoalKey[i]);
			}
		}
		list.Add(string.Format(GetStageClearFormatText(p_stage.n_CLEAR1), array[0]));
		list.Add(string.Format(GetStageClearFormatText(p_stage.n_CLEAR2), array[1]));
		list.Add(string.Format(GetStageClearFormatText(p_stage.n_CLEAR3), array[2]));
		return list.ToArray();
	}

	public static string[] GetClearMapTable()
	{
		List<FieldInfo> list = new List<FieldInfo>();
		FieldInfo[] fields = typeof(LOCALIZATION_KEY).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && fieldInfo.Name.StartsWith("STAGE_CLEAR_VALUE_"))
			{
				list.Add(fieldInfo);
			}
		}
		bool flag = false;
		for (int j = 1; j < list.Count; j++)
		{
			flag = false;
			for (int k = 0; k < list.Count - j; k++)
			{
				int num = int.Parse(list[k].Name.Substring("STAGE_CLEAR_VALUE_".Length));
				int num2 = int.Parse(list[k + 1].Name.Substring("STAGE_CLEAR_VALUE_".Length));
				if (num > num2)
				{
					FieldInfo value = list[k + 1];
					list[k + 1] = list[k];
					list[k] = value;
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		List<string> list2 = new List<string>();
		list2.Add("");
		for (int l = 0; l < list.Count; l++)
		{
			list2.Add(list[l].GetValue(null) as string);
		}
		return list2.ToArray();
	}

	public static string GetLocalString(string sTableKey)
	{
		string[] array = sTableKey.Split(',');
		if (array.Length < 2)
		{
			return "";
		}
		switch (array[0])
		{
		case "LOCALIZATION_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(array[1]);
		case "SKILLTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "CHARATEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "WEAPONTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "CARDTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.CARDTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "SKINTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.SKINTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "ITEMTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "EQUIPTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "DISCTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.DISCTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "SCENARIOTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.SCENARIOTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "STAGETEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "MISSIONTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.MISSIONTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "MAILTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.MAILTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "HOWTOGETTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.HOWTOGETTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "RECORDTIP_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(array[1]);
		case "RANDOMNAME_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.RANDOMNAME_TABLE_DICT.GetL10nValue(array[1]);
		case "TIP_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.TIP_TABLE_DICT.GetL10nValue(array[1]);
		case "WANTEDTEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.WANTEDTEXT_TABLE_DICT.GetL10nValue(array[1]);
		case "AREATEXT_TABLE":
			return ManagedSingleton<OrangeTextDataManager>.Instance.AREATEXT_TABLE_DICT.GetL10nValue(array[1]);
		default:
			return "";
		}
	}

	private string GetStageClearFormatText(int p_type)
	{
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(GetClearMapTable()[p_type]);
	}

	public int GetExtraResetCount(int stageID)
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.ContainsKey(stageID))
		{
			return ManagedSingleton<PlayerNetManager>.Instance.dicStage[stageID].netStageInfo.ExtraResetCount;
		}
		return 0;
	}

	public uint GetStageCrc(int stageID)
	{
		STAGE_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(stageID, out value))
		{
			return MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetCrcByBundleName("jsondatas/stagedata/" + value.s_STAGE);
		}
		return 0u;
	}

	public StageCharacterStruct GetStageCharacterStruct()
	{
		STAGE_RULE_TABLE value = null;
		bool num = ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT.TryGetValue(nLastStageRuleID, out value);
		StageCharacterStruct result = default(StageCharacterStruct);
		if (num)
		{
			result.MainWeaponID = value.n_MAIN_WEAPON;
			result.SubWeaponID = value.n_SUB_WEAPON;
			result.StandbyChara = value.n_CHARACTER;
			result.listNetCharacterSkillInfos = new List<NetCharacterSkillInfo>();
			result.MainWeaponChipID = (ushort)value.n_MAIN_DISC;
			result.SubWeaponChipID = (ushort)value.n_SUB_DISC;
			result.MainWeaponFSID = 0;
			result.SubWeaponFSID = 0;
		}
		else
		{
			result.MainWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
			result.SubWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
			result.StandbyChara = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara;
			result.listNetCharacterSkillInfos = new List<NetCharacterSkillInfo>();
			foreach (NetCharacterSkillInfo value4 in ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[result.StandbyChara].netSkillDic.Values)
			{
				result.listNetCharacterSkillInfos.Add(value4);
			}
			WeaponInfo value2 = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(result.MainWeaponID, out value2))
			{
				result.MainWeaponChipID = value2.netInfo.Chip;
			}
			else
			{
				result.MainWeaponChipID = 0;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(result.SubWeaponID, out value2))
			{
				result.SubWeaponChipID = value2.netInfo.Chip;
			}
			else
			{
				result.SubWeaponChipID = 0;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponFSID > 0)
			{
				result.MainWeaponFSID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponFSID;
			}
			else
			{
				result.MainWeaponFSID = 0;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponFSID > 0)
			{
				result.SubWeaponFSID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponFSID;
			}
			else
			{
				result.SubWeaponFSID = 0;
			}
		}
		CharacterInfo value3 = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(result.StandbyChara, out value3))
		{
			result.Skin = value3.netInfo.Skin;
		}
		return result;
	}

	public int StatusCorrection(int pow, STAGE_RULE_STATUS type)
	{
		int num = -1;
		STAGE_RULE_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT.TryGetValue(nLastStageRuleID_Status, out value))
		{
			switch (type)
			{
			case STAGE_RULE_STATUS.HP:
				num = value.n_HP;
				break;
			case STAGE_RULE_STATUS.ATK:
				num = value.n_ATK;
				break;
			case STAGE_RULE_STATUS.DEF:
				num = value.n_DEF;
				break;
			case STAGE_RULE_STATUS.CRI:
				num = value.n_CRI;
				break;
			case STAGE_RULE_STATUS.CRI_RESIST:
				num = value.n_CRI_RESIST;
				break;
			case STAGE_RULE_STATUS.CRIDMG:
				num = value.n_CRIDMG;
				break;
			case STAGE_RULE_STATUS.CRIDMG_RESIST:
				num = value.n_CRIDMG_RESIST;
				break;
			case STAGE_RULE_STATUS.DODGE:
				num = value.n_DODGE;
				break;
			case STAGE_RULE_STATUS.HIT:
				num = value.n_HIT;
				break;
			case STAGE_RULE_STATUS.PARRY:
				num = value.n_PARRY;
				break;
			case STAGE_RULE_STATUS.PARRY_RESIST:
				num = value.n_PARRY_RESIST;
				break;
			case STAGE_RULE_STATUS.PARRY_DEF:
				num = value.n_PARRY_DEF;
				break;
			case STAGE_RULE_STATUS.SKILL_LV:
				num = value.n_SKILL_LV;
				break;
			}
		}
		if (num == -1)
		{
			num = pow;
		}
		return num;
	}

	public bool StageRuleFS(int fsIndex)
	{
		STAGE_RULE_TABLE value = null;
		bool result = true;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT.TryGetValue(nLastStageRuleID_Status, out value) && ((fsIndex == 0 && value.n_MAIN_FS == 0) || (fsIndex == 1 && value.n_SUB_FS == 0)))
		{
			result = false;
		}
		return result;
	}
}
