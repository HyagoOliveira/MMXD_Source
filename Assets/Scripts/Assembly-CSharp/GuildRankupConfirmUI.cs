#define RELEASE
using System;
using System.Collections.Generic;
using enums;

public class GuildRankupConfirmUI : GuildUpgradeConfirmUIBase
{
	public void Setup(int rankBefore, int rankAfter, string title, int ownScore, int ownMoney, int requireScore, int requireMoney, Action onYesCB, Action onNoCB = null)
	{
		GuildSetting guildSetting;
		if (!GuildSetting.TryGetSettingByGuildRank(rankBefore, out guildSetting))
		{
			Debug.LogError(string.Format("Invalid GuildRank : {0} of {1}", rankBefore, "GuildSetting"));
			return;
		}
		GuildSetting guildSetting2;
		if (!GuildSetting.TryGetSettingByGuildRank(rankAfter, out guildSetting2))
		{
			Debug.LogError(string.Format("Invalid GuildRank : {0} of {1}", rankAfter, "GuildSetting"));
			return;
		}
		List<UpgradeInfo> list = new List<UpgradeInfo>();
		if (guildSetting.MemberLimit != guildSetting2.MemberLimit)
		{
			list.Add(new UpgradeInfo(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LEVELUP_MEMBERUP"), guildSetting.MemberLimit, guildSetting2.MemberLimit));
		}
		if (guildSetting.MaxPowerTowerLevel != guildSetting2.MaxPowerTowerLevel)
		{
			list.Add(new UpgradeInfo(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LEVELUP_POWERTOWER"), guildSetting.MaxPowerTowerLevel, guildSetting2.MaxPowerTowerLevel));
		}
		GuildPrivilege guildPrivilege = guildSetting2.OwnPrivilege ^ guildSetting.OwnPrivilege;
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LEVELUP_NEWRANK");
		if (guildPrivilege.HasFlag(GuildPrivilege.GuildHeader1))
		{
			list.Add(new UpgradeInfo(str, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_HEADER_1")));
		}
		if (guildPrivilege.HasFlag(GuildPrivilege.GuildHeader2))
		{
			list.Add(new UpgradeInfo(str, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_HEADER_2")));
		}
		if (guildPrivilege.HasFlag(GuildPrivilege.GuildDeputy))
		{
			list.Add(new UpgradeInfo(str, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_DEPUTY")));
		}
		list.Add(new UpgradeInfo(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LEVELUP_TASKADD"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_COMMENT")));
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		Setup(title, ownScore, ownMoney, requireScore, requireMoney, list, onYesCB, onNoCB);
	}
}
