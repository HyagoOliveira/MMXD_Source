using System.Collections.Generic;
using System.Linq;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cc;
using enums;

public class FriendInviteListUI : OrangeUIBase
{
	public class SocketInviteListInfo
	{
		public string m_PlayerID;

		public SocketPlayerHUD m_PlayerHUD;
	}

	private List<SocketInviteListInfo> m_InviteListInfo = new List<SocketInviteListInfo>();

	[SerializeField]
	private LoopVerticalScrollRect ScrollRect;

	[SerializeField]
	private FriendInviteScrollCell ScrollRectCell;

	[SerializeField]
	private Text TitleText;

	[SerializeField]
	private Text MessageText;

	[SerializeField]
	private GameObject AllRewardBtn;

	[SerializeField]
	private GameObject AllCancelBtn;

	[SerializeField]
	private Image imgEmptyMsgBg;

	[SerializeField]
	private OrangeText textEmptyMsg;

	private int CurrentType;

	private int CurrentUsedCount;

	private int GiftAPLimit;

	public void Setup()
	{
		CurrentType = 0;
		TitleText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_REQUEST_LIST"));
		OnCreateFriendInviteRequestList();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void SetupReward()
	{
		GiftAPLimit = OrangeConst.GIFT_AP_LIMIT + ManagedSingleton<ServiceHelper>.Instance.GetServiceBonusValue(ServiceType.FriendApCountIncrease);
		CurrentType = 1;
		AllRewardBtn.SetActive(true);
		int count = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Count;
		TitleText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_GET_AP"));
		MessageText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_GIFT_AP_COUNT"), MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RewardUesdCount, GiftAPLimit);
		CurrentUsedCount = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RewardUesdCount;
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		m_InviteListInfo.Clear();
		ScrollRect.OrangeInit(ScrollRectCell, 5, count);
		UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_GET_AP"), count);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void Start()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendMessage, OnFriendInviteCallback);
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSFriendMessage, OnFriendInviteCallback);
	}

	public int OnGetCurrentType()
	{
		return CurrentType;
	}

	public string GetPlayerName(int idx)
	{
		return m_InviteListInfo[idx].m_PlayerHUD.m_Name;
	}

	public string GetPlayerID(int idx)
	{
		return m_InviteListInfo[idx].m_PlayerID;
	}

	public int GetPlayerLevel(int idx)
	{
		return m_InviteListInfo[idx].m_PlayerHUD.m_Level;
	}

	public int GetPlayerIcon(int idx)
	{
		return m_InviteListInfo[idx].m_PlayerHUD.m_IconNumber;
	}

	public void OnCancelInviteRequest(int idx)
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteCancel(m_InviteListInfo[idx].m_PlayerID));
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.Remove(m_InviteListInfo[idx].m_PlayerID);
			OnCreateFriendInviteRequestList();
		}
	}

	public void OnFriendInviteCallback(object res)
	{
		bool flag = res is RSFriendMessage;
	}

	public void OnRSFriendInviteGetRequestList(object res)
	{
		if (res is RSFriendInviteGetRequestList)
		{
			OnClickCloseBtn();
		}
	}

	public void OnUseReward(string pid)
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		if (CurrentUsedCount >= GiftAPLimit)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_GIFT_AP_LIMIT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ITEM02);
		List<string> list = new List<string>();
		list.Add(pid);
		int apAmount = OrangeConst.GIFT_AP * list.Count;
		if (apAmount + ManagedSingleton<PlayerHelper>.Instance.GetStamina() > OrangeConst.MAX_AP)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("APMAX_MESSAGE", 1f);
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveFriendGiftAPReq(list, delegate
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowGetAPEPDialog(apAmount, 0);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RewardUesdCount++;
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Remove(pid);
			SetupReward();
		});
	}

	public void OnUseAllReward()
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected() || CurrentType == 0)
		{
			return;
		}
		if (CurrentUsedCount >= GiftAPLimit)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_GIFT_AP_LIMIT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		List<string> list = new List<string>();
		Dictionary<string, SocketFriendRewardReceiveInfo>.Enumerator enumerator = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.GetEnumerator();
		int num = 0;
		while (enumerator.MoveNext())
		{
			SocketFriendRewardReceiveInfo value = enumerator.Current.Value;
			if (value.RewardUsed == 0)
			{
				list.Add(value.PlayerID);
				num++;
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RewardUesdCount++;
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendUseReward(value.PlayerID));
				if (CurrentUsedCount + num >= GiftAPLimit)
				{
					break;
				}
			}
		}
		if (list.Count > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ITEM02);
		}
		int apAmount = OrangeConst.GIFT_AP * list.Count;
		if (apAmount + ManagedSingleton<PlayerHelper>.Instance.GetStamina() > OrangeConst.MAX_AP)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("APMAX_MESSAGE", 1f);
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Remove(list[i]);
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveFriendGiftAPReq(list, delegate
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowGetAPEPDialog(apAmount, 0);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetRewardList());
			SetupReward();
		});
	}

	public void OnCreateFriendInviteRequestList()
	{
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		m_InviteListInfo.Clear();
		List<SocketFriendInviteRequestInfo> list = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			SocketInviteListInfo socketInviteListInfo = new SocketInviteListInfo();
			socketInviteListInfo.m_PlayerID = list[i].TargetPlayerID;
			string friendPlayerHUD = list[i].FriendPlayerHUD;
			if (!(friendPlayerHUD == ""))
			{
				socketInviteListInfo.m_PlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(friendPlayerHUD);
				m_InviteListInfo.Add(socketInviteListInfo);
			}
		}
		ScrollRect.OrangeInit(ScrollRectCell, 5, m_InviteListInfo.Count);
		MessageText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_REQUEST_FRIEND_MAX"), list.Count, OrangeConst.FRIEND_INVITE_LIMIT);
		UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_REQUEST_LIST"), m_InviteListInfo.Count);
	}

	private void UpdateEmptyMsg(string msg, int listCount)
	{
		if (!(textEmptyMsg == null))
		{
			if (listCount > 0)
			{
				imgEmptyMsgBg.color = Color.clear;
				textEmptyMsg.text = string.Empty;
			}
			else
			{
				imgEmptyMsgBg.color = new Color(0.7f, 0.7f, 0.7f, 1f);
				textEmptyMsg.text = msg.ToString();
			}
		}
	}
}
