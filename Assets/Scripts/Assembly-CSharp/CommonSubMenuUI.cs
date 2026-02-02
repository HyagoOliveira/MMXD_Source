using System;
using CallbackDefs;
using OrangeSocket;
using UnityEngine;
using cc;

public class CommonSubMenuUI : CommonPlayerFloatUIBase
{
	[SerializeField]
	protected GameObject _buttonPlayerInfo;

	[SerializeField]
	protected GameObject _buttonBlockPlayer;

	[SerializeField]
	protected GameObject _buttonPrivateChat;

	[SerializeField]
	protected GameObject _buttonInviteFriend;

	protected string _playerName;

	public event Callback OnClosePrivateChatEvent;

	public override void Setup(string playerId, Vector3 tarPos)
	{
		base.Setup(playerId, tarPos);
		SocketPlayerHUD value;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out value))
		{
			_playerName = value.m_Name;
		}
		else
		{
			_playerName = "---";
		}
		bool flag = ManagedSingleton<PlayerHelper>.Instance.CheckPlayerIsSelf(playerId);
		GameObject buttonBlockPlayer = _buttonBlockPlayer;
		if ((object)buttonBlockPlayer != null)
		{
			buttonBlockPlayer.SetActive(!flag && !MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(playerId));
		}
		GameObject buttonInviteFriend = _buttonInviteFriend;
		if ((object)buttonInviteFriend != null)
		{
			buttonInviteFriend.SetActive(!flag && !MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(playerId));
		}
	}

	protected override void OnDestroy()
	{
		this.OnClosePrivateChatEvent = null;
		base.OnDestroy();
	}

	protected virtual void OnEnable()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSBlackGetList, OnRSBlackGetList);
	}

	protected virtual void OnDisable()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSBlackGetList, OnRSBlackGetList);
	}

	public virtual void OnClickPlayerInfoButton()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<PlayerInfoMainUI>("UI_PlayerInfoMain", OnPlayerInfoMainUILoaded);
	}

	public virtual void OnClickBlockPlayerButton()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnBlockConfirmUILoaded);
	}

	public virtual void OnClickPrivateChatButton()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Channel", delegate(ChannelUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, this.OnClosePrivateChatEvent);
			ui.Setup(_playerId, string.Empty);
			OnClickCloseBtn();
		});
	}

	public virtual void OnClickInviteFriendButton()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<FriendInviteUI>("UI_FriendInvite", OnFriendInviteUILoaded);
	}

	private void OnFriendInviteUILoaded(FriendInviteUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_playerId);
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
	}

	private void OnPlayerInfoMainUILoaded(PlayerInfoMainUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_playerId);
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
	}

	private void OnBlockConfirmUILoaded(CommonUI ui)
	{
		ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		ui.SetupYesNO(string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP")), string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_BLACK_LIST_CONFIRM"), _playerName), string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK")), string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL")), OnBlockPlayerConfirmEvent);
	}

	private void OnBlockPlayerConfirmEvent()
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBlackAdd(_playerId));
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Remove(_playerId);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.Remove(_playerId);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendDelete(_playerId));
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendMessage, OnRSGetList, 0, true);
	}

	private void OnRSGetList(object obj)
	{
		if (obj is RSFriendMessage)
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBlackGetList());
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
		}
	}

	private void OnRSBlackGetList(object obj)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		OnClickBackground();
	}
}
