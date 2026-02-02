#define RELEASE
using System.Collections.Generic;
using System.Linq;
using enums;

public class GuildSetting
{
	public int GuildRank { get; private set; }

	public int MemberLimit { get; private set; }

	public GuildPrivilege OwnPrivilege { get; private set; }

	public int RankupScore { get; private set; }

	public int RankupMoney { get; private set; }

	public int MaxPowerTowerLevel { get; private set; }

	public int EddieDonateGroup { get; private set; }

	public List<GuildEddieDonateSetting> EddieDonateSettings { get; private set; } = new List<GuildEddieDonateSetting>();


	public int EddieDonateValueMax { get; private set; }

	public string ModelNameLobby { get; private set; }

	public string ModelNameGuildBoss { get; private set; }

	public string ModelNameWanted { get; private set; }

	public string ModelNamePowerTower { get; private set; }

	public GuildSetting()
	{
	}

	public GuildSetting(GUILD_MAIN data, bool getEddieDonateSettings = false)
	{
		GuildRank = data.n_ID;
		MemberLimit = data.n_GUILD_NUMBER;
		OwnPrivilege = (GuildPrivilege)data.n_GUILD_CLASS;
		RankupScore = data.n_GUILD_INTEGRAL;
		RankupMoney = data.n_GUILD_MONEY;
		MaxPowerTowerLevel = data.n_POWER_UPMAX;
		ModelNameLobby = ((data.s_GUILD_MODEL_1 != "null") ? data.s_GUILD_MODEL_1.ToString() : string.Empty);
		ModelNameGuildBoss = ((data.s_GUILD_MODEL_2 != "null") ? data.s_GUILD_MODEL_2.ToString() : string.Empty);
		ModelNameWanted = ((data.s_GUILD_MODEL_3 != "null") ? data.s_GUILD_MODEL_3.ToString() : string.Empty);
		ModelNamePowerTower = ((data.s_GUILD_MODEL_4 != "null") ? data.s_GUILD_MODEL_4.ToString() : string.Empty);
		if (getEddieDonateSettings)
		{
			EddieDonateGroup = data.n_GUILD_BOX;
			EddieDonateSettings.Clear();
			EddieDonateValueMax = 1;
			List<GuildEddieDonateSetting> eddieDonateSettings;
			if (GuildEddieDonateSetting.TryGetSettingsByGroup(EddieDonateGroup, out eddieDonateSettings))
			{
				EddieDonateSettings.AddRange(eddieDonateSettings);
				EddieDonateValueMax = eddieDonateSettings.Last().Threshold;
			}
		}
	}

	public static bool TryGetSettingByGuildRank(int rank, out GuildSetting guildSetting, bool getEddieDonateSettings = false)
	{
		GUILD_MAIN value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.GUILD_MAIN_DICT.TryGetValue(rank, out value))
		{
			Debug.LogWarning(string.Format("Invalid Guild Rank {0}", rank));
			guildSetting = null;
			return false;
		}
		guildSetting = new GuildSetting(value, getEddieDonateSettings);
		return true;
	}
}
