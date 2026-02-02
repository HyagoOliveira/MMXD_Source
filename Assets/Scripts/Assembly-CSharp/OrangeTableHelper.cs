#define RELEASE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using UnityEngine.Events;
using enums;

public class OrangeTableHelper : ManagedSingleton<OrangeTableHelper>
{
	public enum MOB_SP_FLAG
	{
		MOB_SP_NONE = 0,
		MOB_SP_BOSS = 1,
		MOB_SP_ALL = 16777215
	}

	public class EventsDef
	{
		public bool bResetSize;

		public string sDBEffect = "";

		public int nSibIndex;

		public string sIconStr = "";

		public int nFontSize;

		public string m_subName { get; set; }

		public Color m_color { get; set; }

		public UnityAction<List<EVENT_TABLE>> m_action { get; set; }
	}

	private class EventGroup
	{
		public string iconName;

		public string eventName;

		public List<EVENT_TABLE> list = new List<EVENT_TABLE>();
	}

	public class BonusInfoDef
	{
		public string m_icon { get; set; }

		public string m_localization { get; set; }
	}

	private static CHARACTER_TABLE s_characterTable;

	private static int s_characterID;

	private CHARACTER_TABLE[] cacheCharacterTables;

	private string s_cacheModelExtraSize = string.Empty;

	private float[] useSize = new float[0];

	private static WEAPON_TABLE s_weaponTable;

	private static int s_weaponID;

	private int n_CacheExp;

	private EXP_TABLE nCacheExpTable;

	private int expMaxLevel = -1;

	public Dictionary<int, ObscuredString> dicStageVaild = new Dictionary<int, ObscuredString>();

	private StageType cacheStageType;

	private List<STAGE_TABLE> listCacheStageTableByType = new List<STAGE_TABLE>();

	private int cacheStageMain = -1;

	private List<STAGE_TABLE> listCacheStageTableByMain = new List<STAGE_TABLE>();

	private Dictionary<enums.EventType, EventsDef> eventDef = new Dictionary<enums.EventType, EventsDef>
	{
		{
			enums.EventType.EVENT_STAGE,
			new EventsDef
			{
				m_subName = "01",
				m_color = new Color(0f, 0.4836108f, 0.8113208f),
				m_action = EventTypeNone
			}
		},
		{
			enums.EventType.EVENT_DROP,
			new EventsDef
			{
				m_subName = "01",
				m_color = new Color(0f, 0.4836108f, 0.8113208f),
				m_action = EventType2_OnClick
			}
		},
		{
			enums.EventType.EVENT_LOGIN,
			new EventsDef
			{
				m_subName = "02",
				m_color = new Color(0.6627451f, 0.4039216f, 0.1058824f),
				m_action = EventTypeNone
			}
		},
		{
			enums.EventType.EVENT_BONUS,
			new EventsDef
			{
				m_subName = "03",
				m_color = new Color(0f, 0.4836108f, 0.8113208f),
				m_action = EventTypeNone
			}
		},
		{
			enums.EventType.EVENT_PVPSEASON,
			new EventsDef
			{
				m_subName = "01",
				m_color = new Color(0.6627451f, 0.4039216f, 0.1058824f),
				m_action = EventTypeNone
			}
		},
		{
			enums.EventType.EVENT_LABO,
			new EventsDef
			{
				m_subName = "02",
				m_color = new Color(0.6627451f, 0.4039216f, 0.1058824f),
				m_action = EventType6_OnClick
			}
		},
		{
			enums.EventType.EVENT_RAID_BOSS,
			new EventsDef
			{
				m_subName = "03",
				m_color = new Color(0.9882352f, 0f, 1f),
				m_action = EventType13_OnClick,
				bResetSize = true,
				sDBEffect = "RaidBossEffect",
				nSibIndex = 2
			}
		},
		{
			enums.EventType.EVENT_MISSION,
			new EventsDef
			{
				m_subName = "03",
				m_color = new Color(0.050980393f, 0.3019608f, 0.2627451f),
				m_action = EventType9_OnClick,
				bResetSize = true,
				sDBEffect = "BeginnerDB",
				sIconStr = "Banner_EventOP_002"
			}
		},
		{
			enums.EventType.EVENT_CRUSADE,
			new EventsDef
			{
				m_subName = "03",
				m_color = new Color(0.9882352f, 0f, 1f),
				m_action = EventType15_OnClick,
				bResetSize = true,
				sDBEffect = "CrusadeEffect",
				nSibIndex = 2
			}
		},
		{
			enums.EventType.EVENT_TOTALWAR,
			new EventsDef
			{
				m_subName = "04",
				m_color = new Color(0f, 29f / 85f, 26f / 85f),
				m_action = EventType17_OnClick,
				bResetSize = true,
				sDBEffect = "",
				nSibIndex = 2,
				nFontSize = 28
			}
		}
	};

	public Dictionary<BonusType, BonusInfoDef> dicBonusIcon = new Dictionary<BonusType, BonusInfoDef>
	{
		{
			BonusType.BONUS_EXP,
			new BonusInfoDef
			{
				m_icon = "icon_ITEM_000_003",
				m_localization = "BONUS_TYPE_1"
			}
		},
		{
			BonusType.BONUS_GOLD,
			new BonusInfoDef
			{
				m_icon = "icon_ITEM_000_000",
				m_localization = "BONUS_TYPE_2"
			}
		},
		{
			BonusType.BONUS_PROF,
			new BonusInfoDef
			{
				m_icon = "icon_ITEM_000_006",
				m_localization = "BONUS_TYPE_3"
			}
		},
		{
			BonusType.BONUS_APREDUCE,
			new BonusInfoDef
			{
				m_icon = "icon_ITEM_000_002",
				m_localization = "BONUS_TYPE_4"
			}
		},
		{
			BonusType.BONUS_DROPRATE,
			new BonusInfoDef
			{
				m_icon = "icon_ITEM_000_013",
				m_localization = "BONUS_TYPE_5"
			}
		},
		{
			BonusType.BONUS_DROPAMOUNT,
			new BonusInfoDef
			{
				m_icon = "icon_ITEM_000_014",
				m_localization = "BONUS_TYPE_6"
			}
		},
		{
			BonusType.BONUS_SPFORCES,
			new BonusInfoDef
			{
				m_icon = "icon_ITEM_000_015",
				m_localization = "EVENT_DAMAGE_BOOST"
			}
		}
	};

	private int cacheMultiPlayGachaType = -1;

	private List<PVP_REWARD_TABLE> cacheListPvpRewardTable = new List<PVP_REWARD_TABLE>();

	private AREA_TABLE keepArea;

	private HashSet<string> HashABManagerTablePath = new HashSet<string>();

	private CultureInfo provider = CultureInfo.InvariantCulture;

	private readonly string[] dataTimeformat = new string[2] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd H:mm:ss" };

	private int serverTimeZone = 8;

	private readonly string nullStr = "null";

	private readonly string dummyStr = "DUMMY";

	public int ServerTimeZone
	{
		get
		{
			return serverTimeZone;
		}
		set
		{
			serverTimeZone = value;
		}
	}

	public override void Initialize()
	{
		expMaxLevel = -1;
		ABManagerTableInit();
	}

	public override void Dispose()
	{
	}

	public CHARACTER_TABLE GetCharacterTable(int p_characterID)
	{
		if (p_characterID != s_characterID || s_characterTable == null)
		{
			s_characterID = p_characterID;
			ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(s_characterID, out s_characterTable);
		}
		return s_characterTable;
	}

	public List<CHARACTER_TABLE> GetAllCharacterList()
	{
		return ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.Values.ToList();
	}

	public CHARACTER_TABLE GetStandByChara(int characterId)
	{
		CHARACTER_TABLE value;
		ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(characterId, out value);
		return value;
	}

	public CHARACTER_TABLE[] GetCharacterByLoading()
	{
		if (cacheCharacterTables == null)
		{
			cacheCharacterTables = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.Values.Where((CHARACTER_TABLE x) => x.n_LOADING == 1).ToArray();
		}
		return cacheCharacterTables;
	}

	public float[] GetCharacterTableExtraSize(string s_ModelExtraSize)
	{
		if (s_cacheModelExtraSize == s_ModelExtraSize)
		{
			return useSize;
		}
		string[] array = s_ModelExtraSize.Split(',');
		useSize = new float[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			useSize[i] = float.Parse(array[i]);
		}
		s_cacheModelExtraSize = s_ModelExtraSize;
		return useSize;
	}

	public WEAPON_TABLE GetWeaponTable(int p_weaponID)
	{
		if (p_weaponID != s_weaponID || s_weaponTable == null)
		{
			s_weaponID = p_weaponID;
			ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(s_weaponID, out s_weaponTable);
		}
		return s_weaponTable;
	}

	public SKILL_TABLE[] GetEnemyAllSkillData(MOB_TABLE p_mob)
	{
		Dictionary<int, SKILL_TABLE> sKILL_TABLE_DICT = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT;
		List<SKILL_TABLE> p_list = new List<SKILL_TABLE>();
		TryGetValue2List(p_mob.n_SKILL_1, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_2, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_3, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_4, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_5, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_6, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_7, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_8, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_9, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_10, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_11, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_mob.n_SKILL_12, sKILL_TABLE_DICT, ref p_list);
		return p_list.ToArray();
	}

	public SKILL_TABLE[] GetPetAllSkillData(PET_TABLE p_pet)
	{
		Dictionary<int, SKILL_TABLE> sKILL_TABLE_DICT = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT;
		List<SKILL_TABLE> p_list = new List<SKILL_TABLE>();
		TryGetValue2List(p_pet.n_SKILL_0, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_pet.n_SKILL_1, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_pet.n_SKILL_2, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_pet.n_SKILL_3, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_pet.n_SKILL_4, sKILL_TABLE_DICT, ref p_list);
		TryGetValue2List(p_pet.n_SKILL_5, sKILL_TABLE_DICT, ref p_list);
		return p_list.ToArray();
	}

	private void TryGetValue2List<T_Key, T_Value>(T_Key p_key, Dictionary<T_Key, T_Value> p_dict, ref List<T_Value> p_list)
	{
		T_Value value;
		if (p_dict.TryGetValue(p_key, out value))
		{
			p_list.Add(value);
		}
	}

	public SKILL_TABLE getFS_SkillTable(int nFSID, int nLV, int nStar)
	{
		SKILL_TABLE result = null;
		FS_TABLE fS_TABLE = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Where((KeyValuePair<int, FS_TABLE> x) => x.Value.n_FS_ID == nFSID).ToDictionary((KeyValuePair<int, FS_TABLE> x) => x.Value.n_LV, (KeyValuePair<int, FS_TABLE> x) => x.Value)[nLV];
		switch (nStar)
		{
		case 0:
			result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[fS_TABLE.n_SKILL_0];
			break;
		case 1:
			result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[fS_TABLE.n_SKILL_1];
			break;
		case 2:
			result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[fS_TABLE.n_SKILL_2];
			break;
		case 3:
			result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[fS_TABLE.n_SKILL_3];
			break;
		case 4:
			result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[fS_TABLE.n_SKILL_4];
			break;
		case 5:
			result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[fS_TABLE.n_SKILL_5];
			break;
		}
		return result;
	}

	public EXP_TABLE ReduceLVByCheckPlayerExp(int restrictExp, EXP_TABLE defaultTABLE)
	{
		if (restrictExp == 0)
		{
			return defaultTABLE;
		}
		EXP_TABLE result = defaultTABLE;
		while (defaultTABLE.n_TOTAL_RANKEXP - defaultTABLE.n_RANKEXP > restrictExp)
		{
			int key = defaultTABLE.n_ID - 1;
			if (!ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(key, out defaultTABLE))
			{
				return result;
			}
		}
		return defaultTABLE;
	}

	public EXP_TABLE GetExpTable(int nExp)
	{
		if (n_CacheExp > 0 && n_CacheExp == nExp)
		{
			return nCacheExpTable;
		}
		nCacheExpTable = null;
		Dictionary<int, EXP_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext() && (nExp >= enumerator.Current.Value.n_TOTAL_RANKEXP || enumerator.Current.Value.n_TOTAL_RANKEXP - nExp > enumerator.Current.Value.n_RANKEXP))
		{
		}
		nCacheExpTable = enumerator.Current.Value;
		if (nCacheExpTable == null)
		{
			if (nExp > 0)
			{
				nCacheExpTable = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.Last().Value;
			}
			else
			{
				nCacheExpTable = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.First().Value;
			}
		}
		n_CacheExp = nExp;
		return nCacheExpTable;
	}

	public int GetStaminaLimit(int p_level)
	{
		EXP_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(p_level, out value))
		{
			return value.n_STAMINA;
		}
		return 0;
	}

	public int GetWeaponRank(int p_exp)
	{
		Dictionary<int, EXP_TABLE> eXP_TABLE_DICT = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT;
		int result = 0;
		for (int i = 0; i < eXP_TABLE_DICT.Count && p_exp >= eXP_TABLE_DICT[i].n_TOTAL_WEAPONEXP; i++)
		{
			result = i + 1;
		}
		return result;
	}

	public int GetChipRank(int p_exp)
	{
		Dictionary<int, EXP_TABLE> eXP_TABLE_DICT = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT;
		int result = 0;
		for (int i = 0; i < eXP_TABLE_DICT.Count && p_exp >= eXP_TABLE_DICT[i].n_TOTAL_DISCEXP; i++)
		{
			result = i + 1;
		}
		return result;
	}

	public EXP_TABLE GetChipExpTable(int p_exp)
	{
		Dictionary<int, EXP_TABLE> eXP_TABLE_DICT = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT;
		int key = 0;
		for (int i = 0; i < eXP_TABLE_DICT.Count && p_exp >= eXP_TABLE_DICT[i].n_TOTAL_DISCEXP; i++)
		{
			key = i + 1;
		}
		return eXP_TABLE_DICT[key];
	}

	public int GetExpMaxLevel()
	{
		if (expMaxLevel <= 0)
		{
			expMaxLevel = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.Keys.LastOrDefault();
		}
		return expMaxLevel;
	}

	public int GetCardRank(int p_exp)
	{
		Dictionary<int, EXP_TABLE> eXP_TABLE_DICT = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT;
		int num = 0;
		for (int i = 0; i < eXP_TABLE_DICT.Count && p_exp >= eXP_TABLE_DICT[i].n_TOTAL_CARDEXP; i++)
		{
			num = i + 1;
		}
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		return (num > lV) ? lV : num;
	}

	public EXP_TABLE GetCardExpTable(int p_exp)
	{
		Dictionary<int, EXP_TABLE> eXP_TABLE_DICT = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT;
		int num = 0;
		for (int i = 0; i < eXP_TABLE_DICT.Count && p_exp >= eXP_TABLE_DICT[i].n_TOTAL_CARDEXP; i++)
		{
			num = i + 1;
		}
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		num = ((num > lV) ? lV : num);
		return eXP_TABLE_DICT[num];
	}

	public bool GetStage(int p_stageId, out STAGE_TABLE stage)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(p_stageId, out stage);
	}

	public void StageVaildInit()
	{
		dicStageVaild.Clear();
		foreach (STAGE_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values)
		{
			dicStageVaild.Add(value.n_ID, MD5Crypto.Encode(value.s_STAGE));
		}
	}

	public bool IsStageVaild(int p_stageId)
	{
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Count != dicStageVaild.Count)
		{
			return true;
		}
		STAGE_TABLE stage;
		ObscuredString value;
		if (GetStage(p_stageId, out stage) && dicStageVaild.TryGetValue(p_stageId, out value))
		{
			return MD5Crypto.Encode(stage.s_STAGE) == value.ToString();
		}
		return false;
	}

	public bool GetStageRuleUI(int p_stageRuleId, out STAGE_RULE_TABLE stageRule)
	{
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT.TryGetValue(p_stageRuleId, out stageRule) && stageRule.n_UI >= 0)
		{
			return true;
		}
		return false;
	}

	public List<STAGE_TABLE> GetListStageByType(StageType p_type)
	{
		if (cacheStageType != 0 && cacheStageType == p_type)
		{
			return listCacheStageTableByType;
		}
		listCacheStageTableByType.Clear();
		int type = (int)p_type;
		listCacheStageTableByType = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_TYPE == type).ToList();
		cacheStageType = p_type;
		return listCacheStageTableByType;
	}

	public List<STAGE_TABLE> GetListStageByMain(int p_main)
	{
		if (cacheStageMain > 0 && cacheStageMain == p_main)
		{
			return listCacheStageTableByMain;
		}
		listCacheStageTableByMain.Clear();
		listCacheStageTableByMain = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_MAIN == p_main).ToList();
		cacheStageMain = p_main;
		return listCacheStageTableByMain;
	}

	public SCENARIO_TABLE[] GetScenarioGroupData(int p_id)
	{
		int targetGroup = ManagedSingleton<OrangeDataManager>.Instance.SCENARIO_TABLE_DICT[p_id].n_GROUP;
		return ManagedSingleton<OrangeDataManager>.Instance.SCENARIO_TABLE_DICT.Where((KeyValuePair<int, SCENARIO_TABLE> x) => x.Value.n_GROUP == targetGroup).ToDictionary((KeyValuePair<int, SCENARIO_TABLE> x) => x.Value.n_ID, (KeyValuePair<int, SCENARIO_TABLE> x) => x.Value).Values.ToArray();
	}

	public bool GetItem(int p_itemId, out ITEM_TABLE item)
	{
		return ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(p_itemId, out item);
	}

	public int GetItemMax(int p_itemId)
	{
		ITEM_TABLE value;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(p_itemId, out value))
		{
			return value.n_MAX;
		}
		return 0;
	}

	public string GetItemName(int p_itemId)
	{
		ITEM_TABLE value;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(p_itemId, out value))
		{
			return ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
		}
		return string.Empty;
	}

	public int[] ParseHowToGetRow(string row)
	{
		if (row == nullStr)
		{
			return new int[0];
		}
		string[] array = row.Split(',');
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = int.Parse(array[i]);
		}
		return array2;
	}

	public bool GetEquip(int p_equipId, out EQUIP_TABLE equip)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.TryGetValue(p_equipId, out equip);
	}

	public MOB_TABLE[] GetAllMob()
	{
		return ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT.Values.ToArray();
	}

	public MOB_TABLE[] GetMobArrayFromGroup(int p_group)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT.Where((KeyValuePair<int, MOB_TABLE> x) => x.Value.n_GROUP == p_group).ToDictionary((KeyValuePair<int, MOB_TABLE> x) => x.Value.n_ID, (KeyValuePair<int, MOB_TABLE> x) => x.Value).Values.ToArray();
	}

	public MOB_TABLE GetMob(int p_idx)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[p_idx];
	}

	public bool IsBoss(MOB_TABLE tMob)
	{
		if (tMob.n_TYPE == 2 || tMob.n_TYPE == 4 || tMob.n_TYPE == 5)
		{
			return true;
		}
		return false;
	}

	public bool IsBossSP(MOB_TABLE tMob)
	{
		if (((uint)tMob.n_SP_FLAG & (true ? 1u : 0u)) != 0)
		{
			return true;
		}
		return false;
	}

	public bool IsZakoSP(MOB_TABLE tMob)
	{
		if (tMob.n_SP_FLAG == 0)
		{
			return true;
		}
		return false;
	}

	public List<MISSION_TABLE> GetListMissionByCondition(int p_condition, int p_main)
	{
		List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_CONDITION == p_condition && x.n_CONDITION_X == p_main).ToList();
		list.Sort((MISSION_TABLE x, MISSION_TABLE y) => x.n_CONDITION_Y.CompareTo(y.n_CONDITION_Y));
		return list;
	}

	public List<MISSION_TABLE> GetListMissionByConditionDifficulty(int p_condition, int p_main, int p_difficulty)
	{
		List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_CONDITION == p_condition && x.n_CONDITION_X == p_main && x.n_CONDITION_Z == p_difficulty).ToList();
		list.Sort((MISSION_TABLE x, MISSION_TABLE y) => x.n_CONDITION_Y.CompareTo(y.n_CONDITION_Y));
		return list;
	}

	public List<MISSION_TABLE> GetMissionTableByType(MissionType p_missionType, long now)
	{
		int missionType = (int)p_missionType;
		return ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == missionType && IsOpeningDate(x.s_BEGIN_TIME, x.s_END_TIME, now)).ToList();
	}

	private static void EventType2_OnClick(List<EVENT_TABLE> lt)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
		{
			ui.Setup(lt);
		});
	}

	private static void EventType6_OnClick(List<EVENT_TABLE> lt)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Minigame01", delegate(Minigame01UI ui)
		{
			ui.Setup();
		});
	}

	private static void EventType9_OnClick(List<EVENT_TABLE> lt)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Mission", delegate(MissionUI ui)
		{
			ui.Setup(MissionType.Activity);
		});
	}

	private static void EventType13_OnClick(List<EVENT_TABLE> lt)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WORLDBOSSEVENT", delegate(WorldBossEventUI ui)
		{
			ui.Setup();
		});
	}

	private static void EventType17_OnClick(List<EVENT_TABLE> lt)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TotalWar", delegate(TotalWarUI ui)
		{
			ui.Setup();
		});
	}

	private static void EventType15_OnClick(List<EVENT_TABLE> lt)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			Singleton<GuildSystem>.Instance.OnGetCheckGuildStateOnceEvent += OnGetCheckGuildStateEvent;
			Singleton<GuildSystem>.Instance.ReqCheckGuildState();
		}
	}

	private static void OnGetCheckGuildStateEvent()
	{
		if (!Singleton<GuildSystem>.Instance.HasGuild)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_BANNER_ERROR");
			return;
		}
		Singleton<CrusadeSystem>.Instance.OnRetrieveCrusadeInfoOnceEvent += OnRetrieveCrusadeInfoOnceEvent;
		Singleton<CrusadeSystem>.Instance.RetrieveCrusadeInfo();
	}

	private static void OnRetrieveCrusadeInfoOnceEvent()
	{
		if (!Singleton<CrusadeSystem>.Instance.HasEvent)
		{
			Debug.LogError("No GuildBoss Event");
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildBossMainUI>("UI_GuildBossMain", OnGuildBossUILoaded);
		}
	}

	private static void OnGuildBossUILoaded(GuildBossMainUI ui)
	{
		ui.Setup();
	}

	private static void EventTypeNone(List<EVENT_TABLE> lt)
	{
	}

	public EventsDef GetEventsDef(enums.EventType eType)
	{
		return eventDef[eType];
	}

	public bool IsAnyEventBonusByType(StageType p_stageType)
	{
		if (p_stageType == StageType.None)
		{
			return false;
		}
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<EVENT_TABLE> list = (from p in ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_BONUS, serverUnixTimeNowUTC)
			where p.n_TYPE_X == (int)p_stageType
			select p).ToList();
		if (list == null || list.Count == 0)
		{
			return false;
		}
		return true;
	}

	public List<PVP_REWARD_TABLE> GetPvpRewardTableByType(MultiPlayGachaType p_type)
	{
		int currentType = (int)p_type;
		if (cacheMultiPlayGachaType > 0 && cacheMultiPlayGachaType == currentType)
		{
			return cacheListPvpRewardTable;
		}
		cacheListPvpRewardTable = ManagedSingleton<OrangeDataManager>.Instance.PVP_REWARD_TABLE_DICT.Values.Where((PVP_REWARD_TABLE x) => x.n_TYPE == currentType).ToList();
		cacheMultiPlayGachaType = currentType;
		return cacheListPvpRewardTable;
	}

	public int GetPvpRewardGachaGroupId(MultiPlayGachaType p_type)
	{
		int currentType = (int)p_type;
		return ManagedSingleton<OrangeDataManager>.Instance.PVP_REWARD_TABLE_DICT.Values.FirstOrDefault((PVP_REWARD_TABLE x) => x.n_TYPE == currentType).n_GACHAID;
	}

	public bool GetService(int p_Id, out SERVICE_TABLE service)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.SERVICE_TABLE_DICT.TryGetValue(p_Id, out service);
	}

	public List<SERVICE_TABLE> GetServiceListByGroup(int p_group)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.SERVICE_TABLE_DICT.Values.Where((SERVICE_TABLE x) => x.n_GROUP == p_group).ToList();
	}

	public AREA_TABLE GetSaveArea()
	{
		if (keepArea == null)
		{
			string code = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Locate;
			keepArea = ManagedSingleton<OrangeDataManager>.Instance.AREA_TABLE_DICT.Values.FirstOrDefault((AREA_TABLE x) => x.s_CODE == code);
			if (keepArea == null)
			{
				keepArea = ManagedSingleton<OrangeDataManager>.Instance.AREA_TABLE_DICT[218];
			}
		}
		return keepArea;
	}

	private void ABManagerTableInit()
	{
		HashABManagerTablePath = new HashSet<string>();
		foreach (ABMANAGER_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.ABMANAGER_TABLE_DICT.Values)
		{
			HashABManagerTablePath.Add(value.s_PATH);
		}
	}

	public bool ABIgonreFromTable(string path)
	{
		return HashABManagerTablePath.Contains(path);
	}

	public bool IsOpeningDate(string p_beginTime, string p_endTime, long p_now)
	{
		if (IsNullOrEmpty(p_beginTime) || IsNullOrEmpty(p_endTime))
		{
			return true;
		}
		try
		{
			long num = CapUtility.DateToUnixTime(ParseDate(p_beginTime).AddHours(-ServerTimeZone));
			long num2 = CapUtility.DateToUnixTime(ParseDate(p_endTime).AddHours(-ServerTimeZone));
			if (p_now >= num && p_now < num2)
			{
				return true;
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.Log(string.Format("p_beginTime={0}, p_endTime={1} msg={2}", p_beginTime, p_endTime, ex.Message));
		}
		return false;
	}

	public bool IsOpeningDate(int p_beginTime, int p_endTime, long p_now)
	{
		if (p_now >= p_beginTime)
		{
			return p_now < p_endTime;
		}
		return false;
	}

	public DateTime ParseDate(string p_time)
	{
		return DateTime.ParseExact(p_time, dataTimeformat, provider, DateTimeStyles.None);
	}

	public bool IsTimeAfterDate(string p_checkDate, long p_checkTime)
	{
		if (IsNullOrEmpty(p_checkDate))
		{
			return true;
		}
		try
		{
			long num = CapUtility.DateToUnixTime(ParseDate(p_checkDate).AddHours(-ServerTimeZone));
			if (p_checkTime >= num)
			{
				return true;
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.Log(string.Format("p_checkDate={0}, msg={1}", p_checkDate, ex.Message));
		}
		return false;
	}

	public string ServerDateToLocalDate(string serverData)
	{
		return DateTime.ParseExact(serverData, dataTimeformat, provider, DateTimeStyles.None).AddHours(-serverTimeZone).AddHours(CapUtility.GetTimeZoneOffSet())
			.ToString(dataTimeformat[0]);
	}

	public long ServerDateToUTC(string serverData)
	{
		return CapUtility.DateToUnixTime(ParseDate(serverData).AddHours(-ServerTimeZone));
	}

	public bool IsNullOrEmpty(string s)
	{
		if (!(s == nullStr))
		{
			return string.IsNullOrEmpty(s);
		}
		return true;
	}

	public bool IsDummyOrEmpty(string s)
	{
		if (!(s == dummyStr))
		{
			return string.IsNullOrEmpty(s);
		}
		return true;
	}

	public string[] ParseSE(string p_tableKey)
	{
		string[] array = new string[3]
		{
			string.Empty,
			string.Empty,
			string.Empty
		};
		if (!IsNullOrEmpty(p_tableKey))
		{
			string[] array2 = p_tableKey.Split(',');
			array[0] = array2[0];
			array[1] = array2[1];
			if (array2.Length > 2)
			{
				array[2] = array2[2];
			}
		}
		return array;
	}

	public int GetCardTypeIndex(int typ)
	{
		return (int)Math.Log(typ, 2.0);
	}

	public string GetCardTypeAssetName(int typ)
	{
		return "ui_iconsource_card_ball_s_" + ((CardColorType)typ).ToString().ToLower();
	}

	public bool IsCard(ITEM_TABLE itemTable)
	{
		if (itemTable.n_TYPE == 5 && itemTable.n_TYPE_X == 1)
		{
			return (int)itemTable.f_VALUE_Y > 0;
		}
		return false;
	}
}
