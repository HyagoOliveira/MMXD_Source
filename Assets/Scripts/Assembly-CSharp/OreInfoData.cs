#define RELEASE
using System.Collections.Generic;
using System.Linq;

public class OreInfoData
{
	public int ID { get; private set; }

	public OreType Type { get; private set; }

	public int Group { get; private set; }

	public int Level { get; private set; }

	public string StageInfo { get; private set; }

	public List<int> AffectedStageType { get; private set; }

	public bool EnableOnChallengeMode { get; private set; }

	public int ItemID { get; private set; }

	public List<int> AttachSkillIDList { get; private set; }

	public List<SKILL_TABLE> SkillAttrDataList { get; private set; }

	public SKILL_TABLE MainSkillAttrData { get; private set; }

	public int LevelUpMoney { get; private set; }

	public int OpenLimitType { get; private set; }

	public int OpenLimitValue { get; private set; }

	public ORE_TABLE AttrData { get; private set; }

	public OreInfoData NextLevelInfoData { get; private set; }

	public OreInfoData(NetOreInfo oreInfo)
	{
		Dictionary<int, ORE_TABLE>.ValueCollection values = ManagedSingleton<OrangeDataManager>.Instance.ORE_TABLE_DICT.Values;
		ORE_TABLE attrData = values.FirstOrDefault((ORE_TABLE data) => data.n_ORE_GROUP == oreInfo.OreGroup && data.n_ORE_LEVEL == oreInfo.OreLevel);
		if (attrData == null)
		{
			Debug.LogError(string.Format("[{0}.ctor] Failed to get {1} of Group : {2}, Level : {3}", "OreInfoData", "ORE_TABLE", oreInfo.OreGroup, oreInfo.OreLevel));
			return;
		}
		Initialize(attrData);
		ORE_TABLE oRE_TABLE = values.FirstOrDefault((ORE_TABLE data) => data.n_ORE_GROUP == attrData.n_ORE_GROUP && data.n_ORE_LEVEL == attrData.n_ORE_LEVEL + 1);
		if (oRE_TABLE != null)
		{
			NextLevelInfoData = new OreInfoData(oRE_TABLE);
		}
	}

	public OreInfoData(ORE_TABLE attrData, bool setNextLevel = false)
	{
		Initialize(attrData);
		if (setNextLevel)
		{
			ORE_TABLE oRE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.ORE_TABLE_DICT.Values.FirstOrDefault((ORE_TABLE data) => data.n_ORE_GROUP == attrData.n_ORE_GROUP && data.n_ORE_LEVEL == attrData.n_ORE_LEVEL + 1);
			if (oRE_TABLE != null)
			{
				NextLevelInfoData = new OreInfoData(oRE_TABLE);
			}
		}
	}

	private void Initialize(ORE_TABLE attrData)
	{
		AttrData = attrData;
		ID = attrData.n_ID;
		Type = (OreType)attrData.n_ORE_TYPE;
		Group = attrData.n_ORE_GROUP;
		Level = attrData.n_ORE_LEVEL;
		StageInfo = attrData.s_ORE_TIP;
		AffectedStageType = (from value in attrData.s_ORE_STAGE.Split(',')
			select int.Parse(value)).ToList();
		ItemID = attrData.n_ORE_ICON;
		AttachSkillIDList = new List<int>();
		if (attrData.n_ORE_SKILL_1 > 0)
		{
			AttachSkillIDList.Add(attrData.n_ORE_SKILL_1);
		}
		if (attrData.n_ORE_SKILL_2 > 0)
		{
			AttachSkillIDList.Add(attrData.n_ORE_SKILL_2);
		}
		EnableOnChallengeMode = attrData.n_ORE_CHALLENGE > 0;
		LevelUpMoney = attrData.n_ORE_UPMONEY;
		OpenLimitType = attrData.n_ORE_OPEN;
		OpenLimitValue = attrData.n_ORE_VALUE;
		SKILL_TABLE value2;
		SkillAttrDataList = AttachSkillIDList.Select((int skillId) => (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(skillId, out value2)) ? null : value2).ToList();
		MainSkillAttrData = SkillAttrDataList[0];
	}

	public void CheckLevelUpState(NetGuildInfo guildInfo, out bool hasNextLevel, out bool canLevelUp)
	{
		if (NextLevelInfoData == null)
		{
			hasNextLevel = false;
			canLevelUp = false;
			return;
		}
		hasNextLevel = true;
		int openLimitType = NextLevelInfoData.OpenLimitType;
		if (guildInfo.PowerTower >= NextLevelInfoData.OpenLimitValue)
		{
			canLevelUp = true;
		}
		else
		{
			canLevelUp = false;
		}
	}

	public override string ToString()
	{
		return string.Format("{0}({1}:{2} / {3}:{4} / {5}:{6} / {7}:{8})", "OreInfoData", "ID", ID, "Type", Type, "Group", Group, "Level", Level);
	}
}
