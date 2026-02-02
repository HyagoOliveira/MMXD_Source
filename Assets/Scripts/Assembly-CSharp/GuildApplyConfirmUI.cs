using UnityEngine;
using UnityEngine.UI;

public class GuildApplyConfirmUI : OrangeUIBase
{
	[SerializeField]
	private Text _textGuildName;

	[SerializeField]
	private Text _textGuildPresident;

	[SerializeField]
	private Text _textGuildIntroduction;

	[SerializeField]
	private Text _textGuildMember;

	[SerializeField]
	private CommonGuildBadge _guildBadge;

	[SerializeField]
	private ImageSpriteSwitcher _guildRank;

	[SerializeField]
	private InputField _inputApplyMsg;

	private NetGuildInfo _targetGuild;

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnJoinGuildEvent += OnJoinGuildEvent;
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnJoinGuildEvent -= OnJoinGuildEvent;
	}

	public void Setup(NetGuildInfo guildInfo)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_targetGuild = guildInfo;
		_inputApplyMsg.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_PRESET");
		GuildSetting guildSetting;
		GuildSetting.TryGetSettingByGuildRank(guildInfo.Rank, out guildSetting);
		_textGuildName.text = guildInfo.GuildName;
		_textGuildPresident.text = string.Empty;
		Singleton<GuildSystem>.Instance.SearchHUD(guildInfo.LeaderPlayerID, OnCheckLeaderHUD);
		_textGuildIntroduction.text = guildInfo.Introduction;
		_textGuildMember.text = string.Format("{0}/{1}", guildInfo.MemberCount, guildSetting.MemberLimit);
		_guildRank.ChangeImage(guildInfo.Rank - 1);
		_guildBadge.SetBadgeIndex(guildInfo.Badge);
		_guildBadge.SetBadgeColor((float)guildInfo.BadgeColor / 360f);
	}

	private void OnCheckLeaderHUD()
	{
		SocketPlayerHUD value;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(_targetGuild.LeaderPlayerID, out value))
		{
			_textGuildPresident.text = value.m_Name;
		}
	}

	public void OnClickConfirmBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		Singleton<GuildSystem>.Instance.ReqJoinGuild(_targetGuild.GuildID, ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(), _inputApplyMsg.text);
	}

	private void OnJoinGuildEvent(Code ackCode)
	{
		OnClickCloseBtn();
	}
}
