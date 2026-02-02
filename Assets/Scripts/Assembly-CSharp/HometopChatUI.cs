#define RELEASE
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using enums;

public class HometopChatUI : MonoBehaviour
{
	private List<string> messageList = new List<string>();

	[SerializeField]
	private OrangeChatText text;

	[SerializeField]
	private ChatChannel[] channelFilter;

	private string[] tempHelloWorld = new string[12]
	{
		"TESTTEST", "測試啦", "安安", "路過炒氣氛。", "轉蛋暴死!!", "ハロー", "Hi~", "안녕하세요", "Bonjour!", "привет",
		"Saluton", "Hej"
	};

	private string formatMsg = "【<color={3}>{0}</color>】{1}：{2}";

	private string[] channelColor = new string[7] { "white", "red", "#6eda00", "orange", "green", "#24c9ff", "maroon" };

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

	private void OnHrefClick(string hrefName)
	{
		Debug.Log("click hyper link = " + hrefName);
	}

	private void Start()
	{
		text.onHrefClick.AddListener(OnHrefClick);
		Singleton<GenericEventManager>.Instance.AttachEvent<SocketChatLogInfo>(EventManager.ID.SOCKET_NOTIFY_NEW_CHATMESSAGE, OnSocketNotifyNewChatMessage);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<SocketChatLogInfo>(EventManager.ID.SOCKET_NOTIFY_NEW_CHATMESSAGE, OnSocketNotifyNewChatMessage);
	}

	public void Clear()
	{
		text.text = "";
		messageList = new List<string>();
	}

	private void OnSocketNotifyNewChatMessage(SocketChatLogInfo chatInfo)
	{
		OnReceiveNewMessage(chatInfo.PlayerID, (ChatChannel)chatInfo.Channel, chatInfo.MessageInfo);
		RefreshMessage();
	}

	private void OnReceiveNewMessage(string playerId, ChatChannel channelType, string messageJson)
	{
		MessageStruct obj;
		if (!ManagedSingleton<FriendHelper>.Instance.IsBlack(playerId) && channelFilter.Length != 0 && channelFilter.Contains(channelType) && JsonHelper.TryDeserialize<MessageStruct>(messageJson, out obj) && obj.EmotionIconID <= 0 && obj.EmotionPkgID <= 0)
		{
			string message = text.ProgLanguage(obj.RichText);
			OrangeDataReader.Instance.BlurChatMessage(ref message);
			string item = string.Format(formatMsg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(_channelTitle[channelType]), obj.NickName, message, channelColor[(int)channelType]);
			messageList.Add(item);
			while (messageList.Count > 3)
			{
				messageList.RemoveAt(0);
			}
		}
	}

	private void RefreshMessage()
	{
		text.text = string.Join("\n", messageList);
	}
}
