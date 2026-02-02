using System.Collections.Generic;
using System.Linq;
using OrangeSocket;
using cc;
using enums;

public class FriendHelper : ManagedSingleton<FriendHelper>
{
	public enum DISPLAY_HINT_CHECK_POINT
	{
		FRIEND_COUNT = 0,
		INVITE_RECEIVE = 1,
		REWARD_RECEIVE = 2
	}

	public bool DisplayHint;

	public bool ChatDisplayHint;

	public bool InviteDisplayHint;

	public bool RewardDisplayHint;

	private int[] SAVE_INT = new int[3];

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public void RetrieveFriendList()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendGetList, OnRSFriendList);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
	}

	public void OnRSFriendList(object res)
	{
		if (!(res is RSFriendGetList))
		{
			return;
		}
		RSFriendGetList rSFriendGetList = (RSFriendGetList)res;
		if (rSFriendGetList.Result != 71000)
		{
			return;
		}
		int friendIDLength = rSFriendGetList.FriendIDLength;
		if (friendIDLength != rSFriendGetList.FriendBusyLength || friendIDLength != rSFriendGetList.FriendRewardGaveLength)
		{
			return;
		}
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Clear();
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.Clear();
		for (int i = 0; i < friendIDLength; i++)
		{
			SocketFriendInfo socketFriendInfo = new SocketFriendInfo
			{
				FriendPlayerID = rSFriendGetList.FriendID(i),
				Status = (UserStatus)rSFriendGetList.FriendBusy(i),
				Busy = rSFriendGetList.FriendBusy(i),
				Follow = rSFriendGetList.FriendFollow(i),
				FriendPlayerHUD = rSFriendGetList.PlayerHUD(i)
			};
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Add(socketFriendInfo.FriendPlayerID, socketFriendInfo);
			if (1 == rSFriendGetList.FriendFollow(i))
			{
				SocketFriendFollowInfo value = new SocketFriendFollowInfo
				{
					FriendPlayerID = rSFriendGetList.FriendID(i),
					Status = (UserStatus)rSFriendGetList.FriendBusy(i),
					Busy = rSFriendGetList.FriendBusy(i),
					Follow = rSFriendGetList.FriendFollow(i),
					FriendPlayerHUD = rSFriendGetList.PlayerHUD(i)
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.Add(socketFriendInfo.FriendPlayerID, value);
			}
			ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketFriendInfo.FriendPlayerID, socketFriendInfo.FriendPlayerHUD);
		}
		SAVE_INT[0] = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count;
	}

	public void RetrieveBlackList()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSBlackGetList, OnRSBlackGetList);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBlackGetList());
	}

	public void OnRSBlackGetList(object res)
	{
		if (!(res is RSBlackGetList))
		{
			return;
		}
		RSBlackGetList rSBlackGetList = (RSBlackGetList)res;
		if (rSBlackGetList.Result != 72000)
		{
			return;
		}
		int blackIDLength = rSBlackGetList.BlackIDLength;
		if (blackIDLength == rSBlackGetList.BlackBusyLength)
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.Clear();
			for (int i = 0; i < blackIDLength; i++)
			{
				SocketBlackInfo socketBlackInfo = new SocketBlackInfo
				{
					BlackPlayerID = rSBlackGetList.BlackID(i),
					Status = (UserStatus)rSBlackGetList.BlackBusy(i),
					Busy = rSBlackGetList.BlackBusy(i),
					FriendPlayerHUD = rSBlackGetList.PlayerHUD(i)
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.Add(socketBlackInfo.BlackPlayerID, socketBlackInfo);
				ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketBlackInfo.BlackPlayerID, socketBlackInfo.FriendPlayerHUD);
			}
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.bInitChatLog)
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.bInitChatLog = true;
				ManagedSingleton<ChatHelper>.Instance.RetrieveChatLog();
				ManagedSingleton<ChatHelper>.Instance.RetrievNTChatMessage();
			}
		}
	}

	public void RetrievFriendInviteRequestList()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendInviteGetRequestList, OnRSFriendInviteGetRequestList, 0, true);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteGetRequestList());
	}

	public void OnRSFriendInviteGetRequestList(object res)
	{
		if (res is RSFriendInviteGetRequestList)
		{
			OnUpdateFriendInviteRequest(res);
		}
	}

	public void OnUpdateFriendInviteRequest(object res)
	{
		RSFriendInviteGetRequestList rSFriendInviteGetRequestList = (RSFriendInviteGetRequestList)res;
		if (rSFriendInviteGetRequestList.Result == 71600)
		{
			int friendIDLength = rSFriendInviteGetRequestList.FriendIDLength;
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.Clear();
			for (int i = 0; i < friendIDLength; i++)
			{
				SocketFriendInviteRequestInfo socketFriendInviteRequestInfo = new SocketFriendInviteRequestInfo
				{
					TargetPlayerID = rSFriendInviteGetRequestList.FriendID(i),
					FriendPlayerHUD = rSFriendInviteGetRequestList.PlayerHUD(i)
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.Add(socketFriendInviteRequestInfo.TargetPlayerID, socketFriendInviteRequestInfo);
				ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketFriendInviteRequestInfo.TargetPlayerID, socketFriendInviteRequestInfo.FriendPlayerHUD);
			}
		}
	}

	public void RetrievFriendInviteReceiveList()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendInviteGetReceiveList, OnRSFriendInviteGetReceiveList);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteGetReceiveList());
	}

	public void OnRSFriendInviteGetReceiveList(object res)
	{
		if (!(res is RSFriendInviteGetReceiveList))
		{
			return;
		}
		RSFriendInviteGetReceiveList rSFriendInviteGetReceiveList = (RSFriendInviteGetReceiveList)res;
		if (rSFriendInviteGetReceiveList.Result != 71600)
		{
			return;
		}
		int friendIDLength = rSFriendInviteGetReceiveList.FriendIDLength;
		if (friendIDLength == rSFriendInviteGetReceiveList.FriendMessageLength)
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Clear();
			for (int i = 0; i < friendIDLength; i++)
			{
				SocketFriendInviteReceiveInfo socketFriendInviteReceiveInfo = new SocketFriendInviteReceiveInfo
				{
					TargetPlayerID = rSFriendInviteGetReceiveList.FriendID(i),
					InviteMessage = rSFriendInviteGetReceiveList.FriendMessage(i),
					Busy = rSFriendInviteGetReceiveList.FriendBusy(i),
					FriendPlayerHUD = rSFriendInviteGetReceiveList.PlayerHUD(i)
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Add(socketFriendInviteReceiveInfo.TargetPlayerID, socketFriendInviteReceiveInfo);
				ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketFriendInviteReceiveInfo.TargetPlayerID, socketFriendInviteReceiveInfo.FriendPlayerHUD);
			}
			SAVE_INT[1] = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Count;
			OnUpdateDisplayHint(1);
			FriendMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<FriendMainUI>("UI_FriendMain");
			if (uI != null)
			{
				uI.SetInviteReceiveList();
			}
		}
	}

	public void RetrievFriendGetGaveRewardList()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendGetGaveRewardList, OnRSFriendGetGaveRewardList);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetGaveRewardList());
	}

	public void OnRSFriendGetGaveRewardList(object res)
	{
		if (!(res is RSFriendGetGaveRewardList))
		{
			return;
		}
		RSFriendGetGaveRewardList rSFriendGetGaveRewardList = (RSFriendGetGaveRewardList)res;
		if (rSFriendGetGaveRewardList.Result == 71603)
		{
			int friendIDLength = rSFriendGetGaveRewardList.FriendIDLength;
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.Clear();
			for (int i = 0; i < friendIDLength; i++)
			{
				SocketFriendRewardRequestInfo socketFriendRewardRequestInfo = new SocketFriendRewardRequestInfo
				{
					PlayerID = rSFriendGetGaveRewardList.FriendID(i)
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.Add(socketFriendRewardRequestInfo.PlayerID, socketFriendRewardRequestInfo);
			}
		}
	}

	public void RetrievFriendGetRewardList()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendGetRewardList, OnRSFriendGetRewardList);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetRewardList());
	}

	public void OnRSFriendGetRewardList(object res)
	{
		if (!(res is RSFriendGetRewardList))
		{
			return;
		}
		RSFriendGetRewardList rs = (RSFriendGetRewardList)res;
		if (rs.Result != 71604)
		{
			return;
		}
		int arrayLength = rs.FriendIDLength;
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RewardUesdCount = 0;
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Clear();
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveGiverListReq(delegate(List<string> plist)
		{
			if (plist != null)
			{
				int i;
				for (i = 0; i < arrayLength; i++)
				{
					if (plist.Exists((string x) => x == rs.FriendID(i)))
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RewardUesdCount++;
					}
					else
					{
						SocketFriendRewardReceiveInfo socketFriendRewardReceiveInfo = new SocketFriendRewardReceiveInfo
						{
							PlayerID = rs.FriendID(i),
							RewardUsed = 0,
							FriendPlayerHUD = rs.PlayerHUD(i)
						};
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Add(socketFriendRewardReceiveInfo.PlayerID, socketFriendRewardReceiveInfo);
						ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketFriendRewardReceiveInfo.PlayerID, socketFriendRewardReceiveInfo.FriendPlayerHUD);
					}
				}
				SAVE_INT[2] = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Count;
				OnUpdateDisplayHint(2);
			}
		});
	}

	public void RetrievContactList()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerContactList, OnRSGetPlayerContactList);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerContactList());
	}

	public void OnRSGetPlayerContactList(object res)
	{
		if (!(res is RSGetPlayerContactList))
		{
			return;
		}
		RSGetPlayerContactList rSGetPlayerContactList = (RSGetPlayerContactList)res;
		if (rSGetPlayerContactList.Result != 71600)
		{
			return;
		}
		int playerIDLength = rSGetPlayerContactList.PlayerIDLength;
		if (playerIDLength == rSGetPlayerContactList.PlayerHUDLength)
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact.Clear();
			for (int i = 0; i < playerIDLength; i++)
			{
				SocketContactInfo socketContactInfo = new SocketContactInfo
				{
					PlayerID = rSGetPlayerContactList.PlayerID(i),
					PlayerHUD = rSGetPlayerContactList.PlayerHUD(i),
					Busy = rSGetPlayerContactList.PlayerBusy(i)
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact.Add(socketContactInfo.PlayerID, socketContactInfo);
				ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketContactInfo.PlayerID, socketContactInfo.PlayerHUD);
			}
		}
	}

	public void RetrievCommunityMessage()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.NTCommunityMessage, OnNTCommunityMessage);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.NTCommunityBroadcastMessage, OnNTCommunityBroadcastMessage);
	}

	public void OnNTCommunityMessage(object res)
	{
		if (!(res is NTCommunityMessage))
		{
			return;
		}
		NTCommunityMessage nTCommunityMessage = (NTCommunityMessage)res;
		if (1 == nTCommunityMessage.SendType)
		{
			DisplayHint = true;
			switch ((FriendMessageType)(short)nTCommunityMessage.SendValue)
			{
			case FriendMessageType.FriendInvite:
			case FriendMessageType.InviteCancel:
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteGetReceiveList());
				break;
			case FriendMessageType.InviteAgree:
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
				MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendInviteGetRequestList, OnRSFriendInviteGetRequestList, 0, true);
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteGetRequestList());
				break;
			case FriendMessageType.InviteDisagree:
				MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendInviteGetRequestList, OnRSFriendInviteGetRequestList, 0, true);
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteGetRequestList());
				break;
			case FriendMessageType.FriendDelete:
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
				break;
			case FriendMessageType.FriendGiveReward:
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetRewardList());
				break;
			}
		}
		else
		{
			if (2 != nTCommunityMessage.SendType && 5 != nTCommunityMessage.SendType)
			{
				return;
			}
			bool bIsFriendPVP = 5 == nTCommunityMessage.SendType;
			if (!MonoBehaviourSingleton<UIManager>.Instance.IsConnecting() && !MonoBehaviourSingleton<UIManager>.Instance.IsLoading && (!(MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "hometop") || !(MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "GuildMain")) && !(MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpMatchUI>("UI_PvpMatch") != null) && !(MonoBehaviourSingleton<UIManager>.Instance.GetUI<FriendBattleUI>("UI_FriendBattle") != null) && !(MonoBehaviourSingleton<UIManager>.Instance.GetUI<FriendPVPRoomMain>("UI_FriendPVPRoomMain") != null) && !(MonoBehaviourSingleton<UIManager>.Instance.GetUI<CoopRoomMainUI>("UI_CoopRoomMain") != null) && !(MonoBehaviourSingleton<UIManager>.Instance.GetUI<CtcWebView>("UI_WebView") != null) && !TurtorialUI.IsTutorialing())
			{
				BattleInviteInfo p_info = JsonHelper.Deserialize<BattleInviteInfo>(nTCommunityMessage.SendString);
				BattleInviteMessageType battleInviteMessageType = (BattleInviteMessageType)nTCommunityMessage.SendValue;
				if (battleInviteMessageType == BattleInviteMessageType.BattleInvite)
				{
					DoBattleInviteEvent(p_info, bIsFriendPVP);
				}
			}
		}
	}

	public void OnNTCommunityBroadcastMessage(object res)
	{
		if (!(res is NTCommunityBroadcastMessage))
		{
			return;
		}
		NTCommunityBroadcastMessage nTCommunityBroadcastMessage = (NTCommunityBroadcastMessage)res;
		if (1 != nTCommunityBroadcastMessage.SendType)
		{
			return;
		}
		switch ((FriendMessageType)(short)nTCommunityBroadcastMessage.SendValue)
		{
		case FriendMessageType.FriendLogin:
		case FriendMessageType.FriendOutQuest:
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend[nTCommunityBroadcastMessage.SendID].Busy = 1;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive[nTCommunityBroadcastMessage.SendID].Busy = 1;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact[nTCommunityBroadcastMessage.SendID].Busy = 1;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack[nTCommunityBroadcastMessage.SendID].Busy = 1;
			}
			break;
		case FriendMessageType.FriendLogout:
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend[nTCommunityBroadcastMessage.SendID].Busy = (int)MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive[nTCommunityBroadcastMessage.SendID].Busy = (int)MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact[nTCommunityBroadcastMessage.SendID].Busy = (int)MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack[nTCommunityBroadcastMessage.SendID].Busy = (int)MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			}
			break;
		case FriendMessageType.FriendInQuest:
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend[nTCommunityBroadcastMessage.SendID].Busy = 2;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive[nTCommunityBroadcastMessage.SendID].Busy = 2;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact[nTCommunityBroadcastMessage.SendID].Busy = 2;
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(nTCommunityBroadcastMessage.SendID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack[nTCommunityBroadcastMessage.SendID].Busy = 2;
			}
			break;
		case FriendMessageType.FriendStateChange:
			break;
		}
	}

	private void DoBattleInviteEvent(BattleInviteInfo p_info, bool bIsFriendPVP = false)
	{
		STAGE_TABLE targetStage = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetStage(p_info.StageId, out targetStage))
		{
			return;
		}
		BattleInviteUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleInviteUI>("UI_BattleInvite");
		if (uI == null)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP03);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BattleInvite", delegate(BattleInviteUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(p_info, targetStage, bIsFriendPVP);
			});
		}
		else
		{
			uI.QueueInvite.Enqueue(p_info);
		}
	}

	public void OnNTFriendStatusChange(object res)
	{
	}

	public void OnUpdateDisplayHint(int typ = 0)
	{
		if (typ == 0)
		{
			foreach (KeyValuePair<int, int> item in new Dictionary<int, int>(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicFriendDisplay))
			{
				if (SAVE_INT.Length > item.Key)
				{
					if (SAVE_INT[item.Key] != item.Value)
					{
						DisplayHint = true;
					}
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicFriendDisplay[item.Key] = SAVE_INT[item.Key];
				}
			}
		}
		if (typ == 1 || typ == 0)
		{
			InviteDisplayHint = false;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count < OrangeConst.FRIEND_LIMIT && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Count > 0)
			{
				InviteDisplayHint = true;
			}
		}
		if (typ == 2 || typ == 0)
		{
			RewardDisplayHint = false;
			int num = OrangeConst.GIFT_AP_LIMIT + ManagedSingleton<ServiceHelper>.Instance.GetServiceBonusValue(ServiceType.FriendApCountIncrease);
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RewardUesdCount < num && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Count > 0)
			{
				RewardDisplayHint = true;
			}
		}
		if (typ != 3 && typ != 0)
		{
			return;
		}
		ChatDisplayHint = false;
		List<SocketFriendInfo> list = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			string friendPlayerID = list[i].FriendPlayerID;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag.ContainsKey(friendPlayerID) && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[friendPlayerID])
			{
				ChatDisplayHint = true;
				break;
			}
		}
		List<SocketContactInfo> list2 = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact.Values.ToList();
		for (int j = 0; j < list2.Count; j++)
		{
			string playerID = list2[j].PlayerID;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag.ContainsKey(playerID) && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[playerID])
			{
				ChatDisplayHint = true;
				break;
			}
		}
	}

	public bool OnGetFriendDisplayHint()
	{
		OnUpdateDisplayHint();
		if (!InviteDisplayHint && !RewardDisplayHint)
		{
			return ChatDisplayHint;
		}
		return true;
	}

	public bool OnGetFriendDisplayHintByType(int typ = 0)
	{
		OnUpdateDisplayHint();
		if (typ == 1)
		{
			if (!RewardDisplayHint)
			{
				return ChatDisplayHint;
			}
			return true;
		}
		if (!InviteDisplayHint)
		{
			return RewardDisplayHint;
		}
		return true;
	}

	public bool IsFriend(string friendPlayerID)
	{
		return MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(friendPlayerID);
	}

	public bool IsBlack(string PlayerID)
	{
		return MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(PlayerID);
	}

	public bool IsFriendInviteRequest(string PlayerID)
	{
		return MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.ContainsKey(PlayerID);
	}

	public bool IsFriendInviteReceive(string PlayerID)
	{
		return MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.ContainsKey(PlayerID);
	}
}
