using UnityEngine;
using UnityEngine.UI;

public class GuildInviteGuildCell : GuildCell<GuildInviteGuildListUI>
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

	public void OnClickAgreeBtn()
	{
		_parentUI.OnClickOneAgreeBtn(this, _guildId);
	}

	public void OnClickRefuseBtn()
	{
		_parentUI.OnClickOneRefuseBtn(this, _guildId);
	}

	public override void RefreshCell()
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildInviteGuildListUI>();
		}
		if (!(_parentUI != null))
		{
			return;
		}
		NetGuildJoinMessageInfo netGuildJoinMessageInfo = Singleton<GuildSystem>.Instance.InviteGuildListCache[_idx];
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
				_applyLimit.text = "自由加入";
				break;
			case 1:
				_applyLimit.text = "需要審核";
				break;
			case 2:
				_applyLimit.text = "禁止加入";
				break;
			}
			Text applyLimit = _applyLimit;
			applyLimit.text = applyLimit.text + "\n戰力需求：\n" + ((guildInfo.PowerDemand > 0) ? guildInfo.PowerDemand.ToString() : "無");
			_rankImage.ChangeImage(guildInfo.Rank - 1);
			if (Singleton<GuildSystem>.Instance.InviteGuildListCache.FindIndex((NetGuildJoinMessageInfo inviteInfo) => inviteInfo.GuildInfo.GuildID == guildInfo.GuildID) >= 0)
			{
				GameObject agreeBtn = _agreeBtn;
				if ((object)agreeBtn != null)
				{
					agreeBtn.SetActive(true);
				}
				GameObject refuseBtn = _refuseBtn;
				if ((object)refuseBtn != null)
				{
					refuseBtn.SetActive(true);
				}
			}
			else if (Singleton<GuildSystem>.Instance.ApplyGuildListCache.FindIndex((NetGuildJoinMessageInfo applyInfo) => applyInfo.GuildInfo.GuildID == guildInfo.GuildID) >= 0)
			{
				GameObject agreeBtn2 = _agreeBtn;
				if ((object)agreeBtn2 != null)
				{
					agreeBtn2.SetActive(false);
				}
				GameObject refuseBtn2 = _refuseBtn;
				if ((object)refuseBtn2 != null)
				{
					refuseBtn2.SetActive(false);
				}
			}
			else
			{
				GameObject agreeBtn3 = _agreeBtn;
				if ((object)agreeBtn3 != null)
				{
					agreeBtn3.SetActive(false);
				}
				GameObject refuseBtn3 = _refuseBtn;
				if ((object)refuseBtn3 != null)
				{
					refuseBtn3.SetActive(false);
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
