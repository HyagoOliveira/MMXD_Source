public class GuildSearchGuildCell : GuildCell<GuildSearchUI>
{
	private NetGuildInfo _guildInfo;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		RefreshCell();
	}

	public override void RefreshCell()
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildSearchUI>();
		}
		if (!(_parentUI != null))
		{
			return;
		}
		int index = (_parentUI.CurrentPage - 1) * 20 + _idx;
		NetGuildInfo netGuildInfo = (_guildInfo = Singleton<GuildSystem>.Instance.SearchGuildListCache[index]);
		if (netGuildInfo != null)
		{
			_guildId = netGuildInfo.GuildID;
			GuildSetting guildSetting;
			GuildSetting.TryGetSettingByGuildRank(netGuildInfo.Rank, out guildSetting);
			base.MemberLimit = guildSetting.MemberLimit;
			_guildName.text = netGuildInfo.GuildName;
			_guildBadge.SetBadgeIndex(netGuildInfo.Badge);
			_guildBadge.SetBadgeColor((float)netGuildInfo.BadgeColor / 360f);
			SocketPlayerHUD value;
			_presidentName.text = (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(netGuildInfo.LeaderPlayerID, out value) ? value.m_Name : "---");
			_guildIntroduction.text = netGuildInfo.Introduction;
			_memberCount.text = string.Format("{0}/{1}", netGuildInfo.MemberCount, guildSetting.MemberLimit);
			switch (netGuildInfo.ApplyType)
			{
			case 0:
				_applyType.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_ADDFREE");
				break;
			case 1:
				_applyType.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_ADDCHECK");
				break;
			case 2:
				_applyType.text = "--------";
				break;
			}
			_applyLimit.text = netGuildInfo.PowerDemand.ToString();
			_rankImage.ChangeImage(netGuildInfo.Rank - 1);
		}
		else
		{
			_guildName.text = string.Empty;
			_presidentName.text = string.Empty;
		}
	}

	public void OnClickGuildInfoBtn()
	{
		_parentUI.OnClickGuildInfoBtn(_guildInfo.GuildID);
	}
}
