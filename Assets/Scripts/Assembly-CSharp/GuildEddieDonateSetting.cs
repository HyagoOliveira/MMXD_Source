#define RELEASE
using System.Collections.Generic;
using System.Linq;

public class GuildEddieDonateSetting
{
	public int GroupId;

	public int Threshold;

	public int GachaId;

	public GuildEddieDonateSetting(BOXGACHA_TABLE attrData)
	{
		GroupId = attrData.n_GROUP;
		Threshold = attrData.n_PRE;
		GachaId = attrData.n_GACHA;
	}

	public static bool TryGetSettingsByGroup(int groupId, out List<GuildEddieDonateSetting> eddieDonateSettings)
	{
		List<BOXGACHA_TABLE> list = (from attrData in ManagedSingleton<OrangeDataManager>.Instance.BOXGACHA_TABLE_DICT.Values
			where attrData.n_GROUP == groupId
			orderby attrData.n_PRE
			select attrData).ToList();
		if (list.Count != 4)
		{
			Debug.LogError(string.Format("EddieDonateSetting Count of Group {0} Mismatch : {1} != {2}", groupId, list.Count, 4));
			eddieDonateSettings = null;
			return false;
		}
		eddieDonateSettings = list.Select((BOXGACHA_TABLE attrData) => new GuildEddieDonateSetting(attrData)).ToList();
		return true;
	}

	public static bool TryGetSettingsByGuildRank(int rank, out List<GuildEddieDonateSetting> eddieDonateSettings)
	{
		GUILD_MAIN value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.GUILD_MAIN_DICT.TryGetValue(rank, out value))
		{
			Debug.LogError(string.Format("Invalid Guild Rank {0}", rank));
			eddieDonateSettings = null;
			return false;
		}
		return TryGetSettingsByGroup(value.n_GUILD_BOX, out eddieDonateSettings);
	}
}
