using UnityEngine;
using UnityEngine.UI;

public class GuildInviteChildUI : OrangeChildUIBase
{
	[SerializeField]
	private Text _textMemberCount;

	[SerializeField]
	private InputField _inputSearchId;

	private string _searchId;

	private bool _isButtonLock;

	public void OnEnable()
	{
		int count = Singleton<GuildSystem>.Instance.MemberInfoListCache.Count;
		_textMemberCount.text = string.Format("{0}/{1}", count, Singleton<GuildSystem>.Instance.GuildSetting.MemberLimit);
		Singleton<GuildSystem>.Instance.OnGetInvitePlayerListEvent += OnGetInvitePlayerListEvent;
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnGetInvitePlayerListEvent -= OnGetInvitePlayerListEvent;
	}

	public override void Setup()
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public void OnClickInviteListBtn()
	{
		if (!_isButtonLock)
		{
			_isButtonLock = true;
			MonoBehaviourSingleton<AudioManager>.Instance.Play("SystemSE", 18);
			Singleton<GuildSystem>.Instance.ReqGetInvitePlayerList();
		}
	}

	public void OnClickInviteSearchBtn()
	{
		if (!_isButtonLock)
		{
			_isButtonLock = true;
			_searchId = _inputSearchId.text.ToUpper();
			if (Singleton<GuildSystem>.Instance.InvitePlayerListCache.FindIndex((NetPlayerJoinMessageInfo inviteInfo) => inviteInfo.PlayerID == _searchId) >= 0)
			{
				CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITEING");
				_isButtonLock = false;
			}
			else if (Singleton<GuildSystem>.Instance.InvitePlayerListCache.Count >= OrangeConst.GUILD_HALLINVITE_MAX)
			{
				CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITEOVER");
				_isButtonLock = false;
			}
			else if (string.IsNullOrEmpty(_searchId) || string.IsNullOrWhiteSpace(_searchId))
			{
				CommonUIHelper.ShowCommonTipUI("INPUT_TEXT");
				_isButtonLock = false;
			}
			else
			{
				Singleton<GuildSystem>.Instance.RefreshBusyStatusAndSearchHUD(_searchId, OnSearchHUDRes);
			}
		}
	}

	private void OnGetInvitePlayerListEvent(Code ackCode)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildInvitePlayerListUI>("UI_GuildInvitePlayerList", OnGuildInviteListUILoaded);
	}

	private void OnGuildInviteListUILoaded(GuildInvitePlayerListUI ui)
	{
		ui.Setup();
		_isButtonLock = false;
	}

	private void OnSearchHUDRes()
	{
		SocketPlayerHUD value;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(_searchId, out value) && value.m_ServerID == MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildInviteSearchListUI>("UI_GuildInviteSearchList", OnGuildInviteSearchListUILoaded);
			return;
		}
		CommonUIHelper.ShowCommonTipUI("ADD_FRIEND_SEARCH_FINISH");
		_isButtonLock = false;
	}

	private void OnGuildInviteSearchListUILoaded(GuildInviteSearchListUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_searchId);
		_isButtonLock = false;
	}
}
