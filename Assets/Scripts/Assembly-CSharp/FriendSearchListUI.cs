using System;
using System.Collections.Generic;
using CallbackDefs;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cc;

public class FriendSearchListUI : OrangeUIBase
{
	public class SocketSearchInfo
	{
		public string m_PlayerID;

		public string m_PlayerName;

		public SocketPlayerHUD m_PlayerHUD;
	}

	private List<SocketSearchInfo> m_SearchInfo = new List<SocketSearchInfo>();

	[SerializeField]
	private LoopVerticalScrollRect ScrollRect;

	[SerializeField]
	private FriendSearchScrollCell ScrollRectCell;

	[SerializeField]
	private Canvas canvasNoResultMsg;

	private string SearchName;

	private bool bIsResetup;

	public void Setup(string nam)
	{
		bIsResetup = false;
		SearchName = nam;
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendSearchByName(nam));
	}

	public void ReSetup()
	{
		bIsResetup = true;
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendSearchByName(SearchName));
	}

	private void Start()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendSearchByName, OnCreateRSFriendSearchByNameCallback);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSFriendSearchByName, OnCreateRSFriendSearchByNameCallback);
	}

	public string GetPlayerName(int idx)
	{
		return m_SearchInfo[idx].m_PlayerName;
	}

	public int GetPlayerLevel(int idx)
	{
		return m_SearchInfo[idx].m_PlayerHUD.m_Level;
	}

	public int GetPlayerIcon(int idx)
	{
		return m_SearchInfo[idx].m_PlayerHUD.m_IconNumber;
	}

	public void OnSendInviteRequest(int idx)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInvite", delegate(FriendInviteUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ReSetup));
			ui.Setup(m_SearchInfo[idx].m_PlayerID);
		});
	}

	public string GetPlayerID(int idx)
	{
		return m_SearchInfo[idx].m_PlayerID;
	}

	public void OnCreateRSFriendSearchByNameCallback(object res)
	{
		if (!(res is RSFriendSearchByName))
		{
			return;
		}
		RSFriendSearchByName rSFriendSearchByName = (RSFriendSearchByName)res;
		m_SearchInfo.Clear();
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		int num = rSFriendSearchByName.PlayerIDLength;
		for (int i = 0; i < rSFriendSearchByName.PlayerIDLength; i++)
		{
			SocketSearchInfo socketSearchInfo = new SocketSearchInfo();
			socketSearchInfo.m_PlayerID = rSFriendSearchByName.PlayerID(i);
			socketSearchInfo.m_PlayerName = rSFriendSearchByName.PlayerName(i);
			string text = rSFriendSearchByName.PlayerHUD(i);
			if (text == "")
			{
				num--;
				continue;
			}
			socketSearchInfo.m_PlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(text);
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == socketSearchInfo.m_PlayerID || MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(socketSearchInfo.m_PlayerID) || MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(socketSearchInfo.m_PlayerID) || MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.ContainsKey(socketSearchInfo.m_PlayerID))
			{
				num--;
			}
			else
			{
				m_SearchInfo.Add(socketSearchInfo);
			}
		}
		if ((0 >= num || 30 < num) && !bIsResetup)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_SEARCH_FINISH"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			canvasNoResultMsg.enabled = true;
		}
		else
		{
			canvasNoResultMsg.enabled = false;
			ScrollRect.OrangeInit(ScrollRectCell, 5, num);
		}
	}
}
