using UnityEngine;

public class GuildJoinGuildCell : GuildCell<GuildJoinUI>
{
	private NetGuildInfo _guildInfo;

	public void PlaySE(SystemSE eCue)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Play("SystemSE", (int)eCue);
	}

	public void PlaySE(SystemSE02 eCue)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.Play("SystemSE02", (int)eCue);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		RefreshCell();
	}

	public void OnClickGuildInfoBtn()
	{
		_parentUI.OnClickGuildInfoBtn(_guildInfo.GuildID);
	}

	public void OnClickApplyBtn()
	{
		_parentUI.OnClickOneApplyBtn(this, _guildInfo);
	}

	public void OnClickCancelApplyBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnCancelApplyConfirmUILoaded);
	}

	private void OnCancelApplyConfirmUILoaded(CommonUI ui)
	{
		ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SCAN_WARNCANCEL"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), OnCancelApplyConfirm, delegate
		{
		});
	}

	private void OnCancelApplyConfirm()
	{
		_parentUI.OnClickOneCancelApplyBtn(this, _guildId);
	}

	public override void RefreshCell()
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildJoinUI>();
		}
		if (!(_parentUI != null))
		{
			return;
		}
		int index = (_parentUI.CurrentPage - 1) * 20 + _idx;
		NetGuildInfo guildInfo = Singleton<GuildSystem>.Instance.SearchGuildListCache[index];
		_guildInfo = guildInfo;
		if (guildInfo != null)
		{
			_guildId = guildInfo.GuildID;
			GuildSetting guildSetting;
			GuildSetting.TryGetSettingByGuildRank(guildInfo.Rank, out guildSetting);
			base.MemberLimit = guildSetting.MemberLimit;
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
			if (Singleton<GuildSystem>.Instance.ApplyGuildListCache.FindIndex((NetGuildJoinMessageInfo applyInfo) => applyInfo.GuildInfo.GuildID == guildInfo.GuildID) >= 0)
			{
				GameObject cancelBtn = _cancelBtn;
				if ((object)cancelBtn != null)
				{
					cancelBtn.SetActive(true);
				}
				GameObject applyBtn = _applyBtn;
				if ((object)applyBtn != null)
				{
					applyBtn.SetActive(false);
				}
			}
			else
			{
				GameObject applyBtn2 = _applyBtn;
				if ((object)applyBtn2 != null)
				{
					applyBtn2.SetActive(true);
				}
				GameObject cancelBtn2 = _cancelBtn;
				if ((object)cancelBtn2 != null)
				{
					cancelBtn2.SetActive(false);
				}
			}
		}
		else
		{
			_guildName.text = string.Empty;
			_presidentName.text = string.Empty;
		}
	}
}
