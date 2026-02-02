public class GuildApplyGuildCell : GuildCell<GuildApplyGuildListUI>
{
	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		RefreshCell();
	}

	public void OnClickGuildInfoBtn()
	{
		_parentUI.OnClickGuildInfoBtn(_guildId);
	}

	public void OnClickCancelApplyBtn()
	{
		_parentUI.OnClickOneCancelApplyBtn(_guildId);
	}

	public override void RefreshCell()
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildApplyGuildListUI>();
		}
		if (!(_parentUI != null))
		{
			return;
		}
		NetGuildJoinMessageInfo netGuildJoinMessageInfo = Singleton<GuildSystem>.Instance.ApplyGuildListCache[_idx];
		if (netGuildJoinMessageInfo != null)
		{
			NetGuildInfo guildInfo = netGuildJoinMessageInfo.GuildInfo;
			GuildSetting guildSetting;
			GuildSetting.TryGetSettingByGuildRank(guildInfo.Rank, out guildSetting);
			base.MemberLimit = guildSetting.MemberLimit;
			_guildId = guildInfo.GuildID;
			_guildName.text = guildInfo.GuildName;
			_guildBadge.SetBadgeIndex(guildInfo.Badge);
			_guildBadge.SetBadgeColor((float)guildInfo.BadgeColor / 360f);
			SocketPlayerHUD value;
			_presidentName.text = (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(guildInfo.LeaderPlayerID, out value) ? value.m_Name : "---");
			_guildIntroduction.text = guildInfo.Introduction;
			_memberCount.text = string.Format("{0}/{1}", guildInfo.MemberCount, guildSetting.MemberLimit);
			switch (guildInfo.ApplyType)
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
			_applyLimit.text = guildInfo.PowerDemand.ToString();
			_rankImage.ChangeImage(guildInfo.Rank - 1);
		}
		else
		{
			_guildName.text = string.Empty;
			_presidentName.text = string.Empty;
		}
	}
}
