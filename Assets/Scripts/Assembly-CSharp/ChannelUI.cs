#define RELEASE
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CallbackDefs;
using OrangeApi;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cc;
using enums;

public class ChannelUI : OrangeUIBase
{
	private enum SubBtn
	{
		Info = 0,
		Black = 1,
		Private = 2,
		Friend = 3
	}

	[Header("Left Tab")]
	[SerializeField]
	private GameObject storageRoot;

	[Header("Sub Menu")]
	[SerializeField]
	private CommonSubMenu subMenu;

	private List<StorageInfo> listStorage = new List<StorageInfo>();

	[Header("Message Dialog")]
	[SerializeField]
	private MessageDialogUI msgDialog;

	[SerializeField]
	private GameObject inputArea;

	[SerializeField]
	private Button sendButton;

	[SerializeField]
	private InputField InputFieldText;

	[SerializeField]
	private OrangeText PlaceHoderText;

	[SerializeField]
	private LoopVerticalScrollRect ScrollRect;

	[SerializeField]
	private MessageNote ScrollCell;

	public string m_currentPID;

	public string m_currentName;

	public List<SocketChatLogInfo> SocketChatLogCache = new List<SocketChatLogInfo>();

	private ChatChannel _prevChannel;

	private ChatChannel _currentChannel = ChatChannel.SystemChannel;

	private Callback _OnUpdateScrollRectFixCallback;

	private Dictionary<ChatChannel, string> _channelTitle = new Dictionary<ChatChannel, string>
	{
		{
			ChatChannel.SystemChannel,
			"CHANNEL_TYPE_SYSTEM"
		},
		{
			ChatChannel.ZoneChannel,
			"CHANNEL_TYPE_WORLD"
		},
		{
			ChatChannel.CrossZoneChannel,
			"CHANNEL_TYPE_SERVER"
		},
		{
			ChatChannel.GuildChannel,
			"CHANNEL_TYPE_GUILD"
		},
		{
			ChatChannel.TeamChannel,
			"CHANNEL_TYPE_TEAM"
		},
		{
			ChatChannel.SeasonTeamChannel,
			"CHANNEL_TYPE_SQUAD"
		},
		{
			ChatChannel.FriendChannel,
			"CHANNEL_TYPE_FRIEND"
		}
	};

	private Dictionary<ChatChannel, string> _channelAreaKey = new Dictionary<ChatChannel, string>
	{
		{
			ChatChannel.SystemChannel,
			"INPUT_TEXT"
		},
		{
			ChatChannel.ZoneChannel,
			"INPUT_TEXT"
		},
		{
			ChatChannel.CrossZoneChannel,
			"INPUT_TEXT_CROSS_SERVER"
		},
		{
			ChatChannel.GuildChannel,
			"INPUT_TEXT"
		},
		{
			ChatChannel.TeamChannel,
			"INPUT_TEXT"
		},
		{
			ChatChannel.SeasonTeamChannel,
			"CHANNEL_TYPE_SERVER"
		},
		{
			ChatChannel.FriendChannel,
			"INPUT_TEXT"
		}
	};

	private int inputCharLimit = 10;

	public bool delaySend;

	private static bool bScrolling;

	protected override void Awake()
	{
		base.Awake();
		inputCharLimit = InputFieldText.characterLimit;
	}

	private void setCheckEmoji()
	{
		InputFieldText.onValidateInput = CheckForEmoji;
		InputFieldText.characterLimit = 0;
	}

	public void OnInputAreaClick()
	{
	}

	public void Setup(string p_friendPID, string p_friendName)
	{
		setCheckEmoji();
		m_currentPID = p_friendPID;
		m_currentName = p_friendName;
		inputArea.SetActive(true);
		_currentChannel = ChatChannel.FriendChannel;
		SetInputAreaPrompt(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(_channelAreaKey[_currentChannel]));
		ChangeChannel(ChatChannel.FriendChannel);
	}

	public void SetupTeam()
	{
		setCheckEmoji();
		inputArea.SetActive(true);
		_currentChannel = ChatChannel.TeamChannel;
		SetInputAreaPrompt(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(_channelAreaKey[_currentChannel]));
		ChangeChannel(ChatChannel.TeamChannel);
	}

	public void Setup(ChatChannel ch = ChatChannel.SystemChannel)
	{
		setCheckEmoji();
		inputArea.SetActive(false);
		_currentChannel = ch;
		SetInputAreaPrompt(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(_channelAreaKey[_currentChannel]));
		CreateNewStorageTab();
	}

	private void CreateNewStorageTab()
	{
		int p_defaultIdx = 0;
		listStorage.Add(new StorageInfo(_channelTitle[ChatChannel.SystemChannel], false, 0, OnClickTab)
		{
			Param = new object[1] { ChatChannel.SystemChannel }
		});
		if (_currentChannel == ChatChannel.SystemChannel)
		{
			p_defaultIdx = listStorage.Count - 1;
		}
		listStorage.Add(new StorageInfo(_channelTitle[ChatChannel.ZoneChannel], false, 0, OnClickTab)
		{
			Param = new object[1] { ChatChannel.ZoneChannel }
		});
		if (_currentChannel == ChatChannel.ZoneChannel)
		{
			p_defaultIdx = listStorage.Count - 1;
		}
		listStorage.Add(new StorageInfo(_channelTitle[ChatChannel.CrossZoneChannel], false, 0, OnClickTab)
		{
			Param = new object[1] { ChatChannel.CrossZoneChannel }
		});
		if (_currentChannel == ChatChannel.CrossZoneChannel)
		{
			p_defaultIdx = listStorage.Count - 1;
		}
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			listStorage.Add(new StorageInfo(_channelTitle[ChatChannel.GuildChannel], false, 0, OnClickTab)
			{
				Param = new object[1] { ChatChannel.GuildChannel }
			});
			if (_currentChannel == ChatChannel.GuildChannel)
			{
				p_defaultIdx = listStorage.Count - 1;
			}
		}
		StorageGenerator.Load("StorageComp00", listStorage, p_defaultIdx, 0, storageRoot.transform);
	}

	public ChatChannel OnGetCurrentChannel()
	{
		return _currentChannel;
	}

	public void OnClickTab(object p_param)
	{
		StorageInfo storageInfo = (StorageInfo)p_param;
		ChangeChannel((ChatChannel)storageInfo.Param[0]);
	}

	private void SetInputAreaPrompt(string prompt)
	{
		PlaceHoderText.text = prompt;
	}

	private void ChangeChannel(ChatChannel ch)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		_prevChannel = _currentChannel;
		_currentChannel = ch;
		if (_currentChannel == ChatChannel.SystemChannel)
		{
			inputArea.SetActive(false);
		}
		else
		{
			inputArea.SetActive(true);
		}
		if (_currentChannel == ChatChannel.FriendChannel)
		{
			msgDialog.Title = m_currentName;
		}
		else
		{
			msgDialog.Title = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(_channelTitle[_currentChannel]);
		}
		SetInputAreaPrompt(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(_channelAreaKey[_currentChannel]));
		ScrollRect.ClearCells();
		RefreshChatLogCache();
		ChatChannel currentChannel = _currentChannel;
		if (currentChannel == ChatChannel.SystemChannel)
		{
			OnUpdateScrollRect();
			return;
		}
		_OnUpdateScrollRectFixCallback = OnUpdateScrollRect;
		Invoke("OnUpdateScrollRectFixCallback", 5f);
		List<string> playerIds = SocketChatLogCache.Select((SocketChatLogInfo chatInfo) => chatInfo.PlayerID).Distinct().ToList();
		Singleton<GuildSystem>.Instance.RefreshCommunityPlayerGuildInfoCache(playerIds, delegate
		{
			OnUpdateScrollRect();
		});
	}

	public void OnUpdateScrollRectFixCallback()
	{
		if (_OnUpdateScrollRectFixCallback != null)
		{
			OnUpdateScrollRect();
		}
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SWITCH_SCENE, Clear);
		Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent += OnConfirmChangeSceneEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent += OnSocketGuildRemovedEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent += OnSocketMemberKickedEvent;
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SWITCH_SCENE, Clear);
		Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent -= OnConfirmChangeSceneEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent -= OnSocketGuildRemovedEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent -= OnSocketMemberKickedEvent;
	}

	private void Clear()
	{
		OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		if (_currentChannel == ChatChannel.FriendChannel && _prevChannel != ChatChannel.FriendChannel)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
			ChangeChannel(_prevChannel);
			storageRoot.gameObject.SetActive(true);
		}
		else
		{
			base.OnClickCloseBtn();
		}
	}

	private string GetTargetID()
	{
		switch (_currentChannel)
		{
		case ChatChannel.FriendChannel:
			return m_currentPID;
		case ChatChannel.SeasonTeamChannel:
			return m_currentPID;
		case ChatChannel.TeamChannel:
			return m_currentPID;
		case ChatChannel.GuildChannel:
			if (!Singleton<GuildSystem>.Instance.HasGuild)
			{
				return string.Empty;
			}
			return string.Format("Guild{0}", Singleton<GuildSystem>.Instance.GuildId);
		default:
			return string.Empty;
		}
	}

	private IEnumerator CancelWaitFlag()
	{
		yield return new WaitForSeconds(5f);
		delaySend = false;
	}

	public void ShowDelayMessage()
	{
		CommonUI msgUI = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<CommonUI>("UI_CommonMsg", false, true);
		msgUI.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
		msgUI.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHANNEL_TALK_LIMIT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
		{
			msgUI.CloseSE = SystemSE.NONE;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		});
	}

	private void ShowInBlackMsg()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
		{
			string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_BLACK_TALK");
			tipUI.Setup(str, true);
			InputFieldText.text = "";
		});
	}

	public void OnSendMessage(int ch)
	{
		if (string.IsNullOrEmpty(InputFieldText.text))
		{
			return;
		}
		if (delaySend)
		{
			ShowDelayMessage();
		}
		else if (_currentChannel == ChatChannel.FriendChannel && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(m_currentPID))
		{
			ShowInBlackMsg();
		}
		else if (_currentChannel == ChatChannel.CrossZoneChannel)
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.ITEMID_CHANNEL_SERVER) > 0)
			{
				ManagedSingleton<PlayerNetManager>.Instance.RetrieveUseChatItemReq(3, delegate(RetrieveUseChatItemRes res)
				{
					SendMsg(ch, res.Token);
					InputFieldText.text = "";
				});
				return;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MATERIAL_NOT_ENOUGH");
				tipUI.Setup(str, true);
			});
			InputFieldText.text = "";
		}
		else
		{
			SendMsg(ch);
			InputFieldText.text = "";
		}
	}

	private bool IsReachable()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}

	private void SendMsg(int ch, string token = "")
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		delaySend = true;
		if (IsReachable())
		{
			MessageStruct msg = new MessageStruct
			{
				NickName = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname,
				RichText = InputFieldText.text,
				EmotionIconID = -1,
				EmotionPkgID = -1
			};
			string message = JsonHelper.Serialize(msg);
			if (_currentChannel == ChatChannel.TeamChannel)
			{
				List<string> list = (from a in MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo
					where a.Nickname != msg.NickName
					select a into p
					select p.PlayerId).ToList();
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatTargetListSendMessage((int)_currentChannel, MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID, list.ToArray(), message, token));
			}
			else
			{
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatSendMessage((int)_currentChannel, MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID, GetTargetID(), message, token));
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_SEND);
		}
		else
		{
			ShowNetWorkErrorDialog();
		}
		StartCoroutine("CancelWaitFlag");
	}

	private void ShowNetWorkErrorDialog()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_NOT_REACHABLE_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
			});
		});
	}

	public void SendEmotionMsg(int pkg, int emotion)
	{
		if (_currentChannel == ChatChannel.FriendChannel && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(m_currentPID))
		{
			ShowInBlackMsg();
		}
		else if (_currentChannel == ChatChannel.CrossZoneChannel)
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.ITEMID_CHANNEL_SERVER) > 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_SEND);
				ManagedSingleton<PlayerNetManager>.Instance.RetrieveUseChatItemReq(3, delegate(RetrieveUseChatItemRes res)
				{
					SendEmoMsg(pkg, emotion, res.Token);
				});
			}
			else
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
				{
					string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MATERIAL_NOT_ENOUGH");
					tipUI.Setup(str, true);
				});
			}
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_SEND);
			SendEmoMsg(pkg, emotion);
		}
	}

	private void SendEmoMsg(int pkg, int emotion, string token = "")
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		delaySend = true;
		if (IsReachable())
		{
			MessageStruct msg = new MessageStruct
			{
				NickName = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname,
				RichText = "",
				EmotionIconID = emotion,
				EmotionPkgID = pkg
			};
			string message = JsonHelper.Serialize(msg);
			if (_currentChannel == ChatChannel.TeamChannel)
			{
				List<string> list = (from a in MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo
					where a.Nickname != msg.NickName
					select a into p
					select p.PlayerId).ToList();
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatTargetListSendMessage((int)_currentChannel, MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID, list.ToArray(), message, token));
			}
			else
			{
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatSendMessage((int)_currentChannel, MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID, GetTargetID(), message, token));
			}
		}
		else
		{
			ShowNetWorkErrorDialog();
		}
		StartCoroutine("CancelWaitFlag");
	}

	private bool RefreshChatLogCache()
	{
		SocketChatLogCache.Clear();
		List<SocketChatLogInfo> value;
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog.TryGetValue(_currentChannel, out value))
		{
			Debug.LogError("Failed to get ChatLogs'");
			return false;
		}
		switch (_currentChannel)
		{
		case ChatChannel.FriendChannel:
			SocketChatLogCache.AddRange(value.Where((SocketChatLogInfo p) => p.PlayerID == m_currentPID || p.TargetID == m_currentPID));
			break;
		case ChatChannel.GuildChannel:
			SocketChatLogCache.AddRange(value);
			break;
		default:
			SocketChatLogCache.AddRange(value);
			break;
		}
		SocketChatLogCache.LastOrDefault();
		return true;
	}

	public void OnUpdateScrollRect()
	{
		CancelInvoke("OnUpdateScrollRectFixCallback");
		_OnUpdateScrollRectFixCallback = null;
		if (ScrollRect == null)
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
			return;
		}
		ScrollRect.totalCount = SocketChatLogCache.Count;
		RefillCellsFromEnd();
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	private void RefillCellsFromEnd()
	{
		if (!(ScrollRect == null))
		{
			ScrollRect.movementType = LoopScrollRect.MovementType.Clamped;
			ScrollRect.RefillCellsFromEnd();
			Invoke("DelaySetMovementType", 0.5f);
		}
	}

	private void DelaySetMovementType()
	{
		ScrollRect.movementType = LoopScrollRect.MovementType.Elastic;
	}

	public void OnUpdateSameScrollRect()
	{
		if (ScrollRect == null)
		{
			return;
		}
		int count = SocketChatLogCache.Count;
		if (ScrollRect.ItemEnd < ScrollRect.prefabSource.count)
		{
			if (ScrollRect.totalCount != count)
			{
				ScrollRect.totalCount = count;
				ScrollRect.RefillCellsFromEnd();
			}
			return;
		}
		if (ScrollRect.totalCount == count && ScrollRect.ItemStart != 0)
		{
			ScrollRect.ItemStart--;
			ScrollRect.ItemEnd--;
		}
		ScrollRect.totalCount = count;
		RefillCellsFromEnd();
	}

	private void OnSocketNotifyNewChatMessage(SocketChatLogInfo chatInfo)
	{
		if (chatInfo.Channel != (int)_currentChannel || (_currentChannel == ChatChannel.FriendChannel && m_currentPID != chatInfo.PlayerID && MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify != chatInfo.PlayerID))
		{
			return;
		}
		SocketChatLogCache.Add(chatInfo);
		string playerID = chatInfo.PlayerID;
		if (string.IsNullOrEmpty(playerID))
		{
			OnUpdateSameScrollRect();
			return;
		}
		SocketGuildMemberInfo value;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SocketGuildMemberInfoCache.TryGetValue(playerID, out value))
		{
			OnUpdateSameScrollRect();
			return;
		}
		Singleton<GuildSystem>.Instance.RefreshCommunityPlayerGuildInfoCache(new List<string> { playerID }, delegate
		{
			OnUpdateSameScrollRect();
		}, true);
	}

	private void SetSubMenuBtn(SubBtn btn, bool active)
	{
		subMenu.subButtons[(int)btn].gameObject.SetActive(active);
	}

	public void ShowSubMenu(string pid, string pname, Vector3 wposition)
	{
		m_currentPID = pid;
		m_currentName = pname;
		SetSubMenuBtn(SubBtn.Black, !ManagedSingleton<FriendHelper>.Instance.IsBlack(m_currentPID));
		SetSubMenuBtn(SubBtn.Friend, !ManagedSingleton<FriendHelper>.Instance.IsFriend(m_currentPID));
		subMenu.OnShowSubMenu(wposition);
	}

	public void OnClickPlayerInfo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		subMenu.gameObject.SetActive(false);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PlayerInfoMain", delegate(PlayerInfoMainUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(m_currentPID);
		});
	}

	public void OnClickBlockPlayer()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		OnAddBlackConfirm();
	}

	public void OnClickJoinFriend()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.Count >= OrangeConst.FRIEND_INVITE_LIMIT)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_ADD_FRIEND_LIMIT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		subMenu.gameObject.SetActive(false);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInvite", delegate(FriendInviteUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(m_currentPID);
		});
	}

	public void OnClickPrivateMsg()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		subMenu.gameObject.SetActive(false);
		storageRoot.gameObject.SetActive(false);
		ChangeChannel(ChatChannel.FriendChannel);
	}

	public void InvokeUpdateFriendList()
	{
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBlackGetList());
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	private void OnAddBlack()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			subMenu.gameObject.SetActive(false);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBlackAdd(m_currentPID));
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Remove(m_currentPID);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.Remove(m_currentPID);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendDelete(m_currentPID));
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
			Invoke("InvokeUpdateFriendList", 1.2f);
		}
	}

	private void OnAddBlackConfirm()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.Count >= OrangeConst.BLACKLIST_LIMIT)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_BLACK_LIST_LIMIT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
		else
		{
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(m_currentPID))
			{
				return;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendConfirm", delegate(FriendConfirmUI ui)
			{
				string p_title = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"));
				string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_BLACK_LIST_CONFIRM"), m_currentName);
				string p_textYes = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"));
				string p_textNo = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"));
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupYesNO(p_title, p_msg, p_textYes, p_textNo, delegate
				{
					ui.CloseSE = SystemSE.NONE;
					OnAddBlack();
				});
			});
		}
	}

	private void OnCreateRSGetPlayerHUDCallback(object res)
	{
		if (!(res is RSGetPlayerHUD))
		{
			return;
		}
		RSGetPlayerHUD rSGetPlayerHUD = (RSGetPlayerHUD)res;
		if (rSGetPlayerHUD.Result == 70300)
		{
			SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(rSGetPlayerHUD.PlayerHUD);
			if (socketPlayerHUD != null)
			{
				ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketPlayerHUD.m_PlayerId, socketPlayerHUD);
			}
		}
	}

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<SocketChatLogInfo>(EventManager.ID.SOCKET_NOTIFY_NEW_CHATMESSAGE, OnSocketNotifyNewChatMessage);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerHUD, OnCreateRSGetPlayerHUDCallback);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<SocketChatLogInfo>(EventManager.ID.SOCKET_NOTIFY_NEW_CHATMESSAGE, OnSocketNotifyNewChatMessage);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSGetPlayerHUD, OnCreateRSGetPlayerHUDCallback);
	}

	private char CheckForEmoji(string test, int charIndex, char addedChar)
	{
		InputFieldText.textComponent.alignByGeometry = false;
		if (inputCharLimit <= charIndex)
		{
			return '\0';
		}
		if (new Regex("[\\p{Cs}]|[\\u00a9]|[\\u00ae]|[\\u2000-\\u2e7f]|[\\ud83c[\\ud000-\\udfff]]|[\\ud83d[\\ud000-\\udfff]]|[\\ud83e[\\ud000-\\udfff]]").IsMatch(addedChar.ToString()))
		{
			return '\0';
		}
		return addedChar;
	}

	private void OnConfirmChangeSceneEvent()
	{
		OnClickCloseBtn();
	}

	private void OnSocketMemberKickedEvent(string memberId, bool isSelf)
	{
		if (isSelf)
		{
			base.OnClickCloseBtn();
		}
	}

	private void OnSocketGuildRemovedEvent()
	{
		base.OnClickCloseBtn();
	}
}
