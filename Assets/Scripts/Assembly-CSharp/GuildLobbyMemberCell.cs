using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildLobbyMemberCell : ScrollIndexCallback
{
	[SerializeField]
	private Text _playerLevel;

	[SerializeField]
	private GameObject _playerIconRoot;

	[SerializeField]
	private Text _playerName;

	[SerializeField]
	private Button _chatButton;

	[SerializeField]
	private GameObject _newChatFlag;

	[SerializeField]
	private OnlineStatusHelper _onlineStatus;

	[SerializeField]
	private Text _eventScore;

	[SerializeField]
	private Text _donateScore;

	[SerializeField]
	private GuildPrivilegeHelper _privilegeHelper;

	private int _idx;

	private string _playerId;

	private GuildMemberListChildUI _parentUI;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildMemberListChildUI>();
		}
		NetMemberInfo netMemberInfo = Singleton<GuildSystem>.Instance.MemberInfoListCache[p_idx];
		_playerId = netMemberInfo.MemberId;
		GuildUIHelper.SetPlayerHUDData(_playerId, _playerName, _playerLevel, _playerIconRoot);
		GuildUIHelper.SetOnlineStatus(_playerId, _onlineStatus);
		_eventScore.text = netMemberInfo.Score.ToString();
		_donateScore.text = netMemberInfo.EddieMoney.ToString("#,0");
		_privilegeHelper.Setup((GuildPrivilege)netMemberInfo.Privilege);
		UpdateChatButton();
	}

	public void OnClickPlayerIconBtn()
	{
		if (_parentUI != null)
		{
			Vector3 position = _playerIconRoot.GetComponent<RectTransform>().position;
			_parentUI.OnClickMemberCell(_playerId, position);
		}
	}

	public void OnClickPrivateChatBtn()
	{
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[_playerId] = false;
		UpdateChatButton();
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Channel", delegate(ChannelUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			SocketPlayerHUD value;
			string p_friendName = (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(_playerId, out value) ? value.m_Name : "---");
			ui.Setup(_playerId, p_friendName);
		});
	}

	private void UpdateChatButton()
	{
		_chatButton.gameObject.SetActive(!ManagedSingleton<PlayerHelper>.Instance.CheckPlayerIsSelf(_playerId));
		_chatButton.interactable = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(_playerId);
		_newChatFlag.SetActive(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetFriendChatIconFlag(_playerId));
	}
}
