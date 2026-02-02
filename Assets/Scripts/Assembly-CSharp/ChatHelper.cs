using System;
using System.Collections.Generic;
using OrangeSocket;
using cc;
using enums;

public class ChatHelper : ManagedSingleton<ChatHelper>
{
	private readonly int[] CHANNEL_COUNT_MAX;

	public override void Initialize()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.GUILD_ID_CHANGED, OnGuildChanged);
	}

	public override void Dispose()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.GUILD_ID_CHANGED, OnGuildChanged);
	}

	public void RetrieveChatLog()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSChatGetMessage, OnRSChatGetMessageCallback);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog.Clear();
		for (int i = 1; i <= 7; i++)
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog[(ChatChannel)i] = new List<SocketChatLogInfo>();
		}
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatGetMessage(1));
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatGetMessage(2));
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatGetMessage(3));
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatGetMessage(4));
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatGetMessage(7));
	}

	private void OnGuildChanged(bool hasGuild)
	{
		List<SocketChatLogInfo> value;
		if (hasGuild)
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatGetMessage(4));
		}
		else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog.TryGetValue(ChatChannel.GuildChannel, out value))
		{
			value.Clear();
		}
	}

	public void OnRSChatGetMessageCallback(object res)
	{
		if (!(res is RSChatGetMessage))
		{
			return;
		}
		RSChatGetMessage rSChatGetMessage = (RSChatGetMessage)res;
		if (rSChatGetMessage.Result != 73500)
		{
			return;
		}
		int playerIDLength = rSChatGetMessage.PlayerIDLength;
		if (playerIDLength <= 0 || playerIDLength != rSChatGetMessage.TargetIDLength || playerIDLength != rSChatGetMessage.ChannelLength || playerIDLength != rSChatGetMessage.UpdateTimeLength || playerIDLength != rSChatGetMessage.MessageListLength)
		{
			return;
		}
		ChatChannel chatChannel = (ChatChannel)rSChatGetMessage.Channel(0);
		if (chatChannel == ChatChannel.FriendChannel)
		{
			int friendChatShowHintTime = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.FriendChatShowHintTime;
			for (int i = 0; i < playerIDLength; i++)
			{
				if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(rSChatGetMessage.PlayerID(i)))
				{
					continue;
				}
				SocketChatLogInfo item = new SocketChatLogInfo
				{
					Channel = rSChatGetMessage.Channel(i),
					PlayerID = rSChatGetMessage.PlayerID(i),
					TargetID = rSChatGetMessage.TargetID(i),
					UpdateTime = CapUtility.UnixTimeToDate(rSChatGetMessage.UpdateTime(i)),
					MessageInfo = rSChatGetMessage.MessageList(i)
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog[chatChannel].Insert(0, item);
				if (friendChatShowHintTime <= 0)
				{
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.FriendChatShowHintTime = (int)MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
				}
				if (friendChatShowHintTime <= rSChatGetMessage.UpdateTime(i))
				{
					if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag.ContainsKey(rSChatGetMessage.PlayerID(i)))
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[rSChatGetMessage.PlayerID(i)] = true;
					}
					else
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag.Add(rSChatGetMessage.PlayerID(i), true);
					}
					if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(rSChatGetMessage.PlayerID(i)))
					{
						ManagedSingleton<FriendHelper>.Instance.ChatDisplayHint = true;
					}
					else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.ContainsKey(rSChatGetMessage.PlayerID(i)))
					{
						ManagedSingleton<FriendHelper>.Instance.ChatDisplayHint = true;
					}
					MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSetPlayerContactByPlayerID(rSChatGetMessage.PlayerID(i));
				}
			}
		}
		else
		{
			List<SocketChatLogInfo> value;
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog.TryGetValue(chatChannel, out value))
			{
				return;
			}
			value.Clear();
			for (int j = 0; j < playerIDLength; j++)
			{
				if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(rSChatGetMessage.PlayerID(j)))
				{
					SocketChatLogInfo item2 = new SocketChatLogInfo
					{
						Channel = rSChatGetMessage.Channel(j),
						PlayerID = rSChatGetMessage.PlayerID(j),
						TargetID = rSChatGetMessage.TargetID(j),
						UpdateTime = CapUtility.UnixTimeToDate(rSChatGetMessage.UpdateTime(j)),
						MessageInfo = rSChatGetMessage.MessageList(j)
					};
					value.Insert(0, item2);
				}
			}
		}
	}

	public void RetrievNTChatMessage()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.NTChatMessage, OnNTChatMessage);
	}

	public void OnNTChatMessage(object res)
	{
		if (!(res is NTChatMessage))
		{
			return;
		}
		NTChatMessage nTChatMessage = (NTChatMessage)res;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(nTChatMessage.PlayerID))
		{
			return;
		}
		ChatChannel chatChannel = (ChatChannel)nTChatMessage.Channel;
		List<SocketChatLogInfo> value;
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicChatLog.TryGetValue(chatChannel, out value))
		{
			return;
		}
		SocketChatLogInfo socketChatLogInfo = new SocketChatLogInfo
		{
			Channel = nTChatMessage.Channel,
			PlayerID = nTChatMessage.PlayerID,
			TargetID = nTChatMessage.TargetID,
			UpdateTime = DateTime.UtcNow,
			MessageInfo = nTChatMessage.Message
		};
		value.Add(socketChatLogInfo);
		if (value.Count > CHANNEL_COUNT_MAX[(int)chatChannel])
		{
			value.RemoveAt(0);
		}
		if (chatChannel == ChatChannel.FriendChannel)
		{
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag.ContainsKey(nTChatMessage.PlayerID))
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[nTChatMessage.PlayerID] = true;
			}
			else
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag.Add(nTChatMessage.PlayerID, true);
			}
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(nTChatMessage.PlayerID))
			{
				ManagedSingleton<FriendHelper>.Instance.ChatDisplayHint = true;
			}
			else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.ContainsKey(nTChatMessage.PlayerID))
			{
				ManagedSingleton<FriendHelper>.Instance.ChatDisplayHint = true;
			}
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSetPlayerContactByPlayerID(nTChatMessage.PlayerID);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SOCKET_NOTIFY_NEW_CHATMESSAGE, socketChatLogInfo);
	}

	public ChatHelper()
	{
		int[] obj = new int[8] { 100, 100, 0, 0, 100, 100, 100, 0 };
		obj[2] = OrangeConst.WORLD_CHANNEL_COUNT;
		obj[3] = OrangeConst.CROSS_CHANNEL_COUNT;
		obj[7] = OrangeConst.FRIEND_CHANNEL_COUNT;
		CHANNEL_COUNT_MAX = obj;
	}
}
