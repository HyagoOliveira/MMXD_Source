#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cc;

public class FriendMainUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect ScrollRect;

	[SerializeField]
	private FriendScrollCell ScrollRectCell;

	[SerializeField]
	private Button[] TopMenu;

	[SerializeField]
	private Text[] TopMenuText;

	[SerializeField]
	private Image FriendListHintImage;

	[SerializeField]
	private Image ContactListHintImage;

	[SerializeField]
	private Button[] TopInviteMenu;

	[SerializeField]
	private GameObject[] FriendTypeBtn;

	private List<StorageInfo> listStorage = new List<StorageInfo>();

	[SerializeField]
	private Transform storageRoot;

	[SerializeField]
	private GameObject MenuFriendList;

	[SerializeField]
	private GameObject MenuInviteList;

	[SerializeField]
	private Text FriendCountText;

	[SerializeField]
	private Text FriendCountText2;

	[SerializeField]
	private Text BlackCountText;

	[SerializeField]
	private InputField InputFieldObject;

	[SerializeField]
	private GameObject Tooltip;

	[SerializeField]
	private GameObject TooltipMeun;

	[SerializeField]
	private GameObject[] TooltipFollowBtn;

	[SerializeField]
	private GameObject[] TooltipInviteBtn;

	[SerializeField]
	private GameObject[] TooltipBlackBtn;

	[SerializeField]
	private Button AllAgreeFriendInviteBtn;

	[SerializeField]
	private Button AllDisagreeFriendInviteBtn;

	[SerializeField]
	private Button GiveAllBtn;

	[SerializeField]
	private Image RewardHintImage;

	[SerializeField]
	private Image imgEmptyMsgBg;

	[SerializeField]
	private OrangeText textEmptyMsg;

	private int CurrentTopMenu;

	private int CurrentType;

	private int CurrentTopInviteMenu;

	private int CurrentTouchIndex;

	private string CurrentTouchPlayerID;

	private string CurrentTouchPlayerName;

	private string CurrentInviteAgreePID;

	public List<SocketBlackInfo> BlackList { get; private set; }

	public List<SocketFriendInviteReceiveInfo> FriendInviteReceiveList { get; private set; }

	public List<SocketFriendFollowInfo> FriendFollowList { get; private set; }

	public List<SocketContactInfo> ContactList { get; private set; }

	public List<SocketFriendInfo> FriendList { get; private set; }

	public void Setup()
	{
		ManagedSingleton<FriendHelper>.Instance.DisplayHint = false;
		OnCloseTooltip(false);
		CurrentType = 0;
		SelectMenuList(0);
		OnSelectFriendTypeBtn(0);
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.FriendChatShowHintTime = (int)MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	private void Start()
	{
		CreateNewStorageTab();
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendMessage, OnRSFriendInviteAgree);
	}

	private void Update()
	{
		ManagedSingleton<FriendHelper>.Instance.OnUpdateDisplayHint(2);
		RewardHintImage.gameObject.SetActive(ManagedSingleton<FriendHelper>.Instance.RewardDisplayHint);
		OnUpdateStorageHint();
		bool active = false;
		List<SocketFriendInfo> list = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			string friendPlayerID = list[i].FriendPlayerID;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag.ContainsKey(friendPlayerID) && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[friendPlayerID])
			{
				active = true;
				break;
			}
		}
		bool active2 = false;
		List<SocketContactInfo> list2 = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact.Values.ToList();
		for (int j = 0; j < list2.Count; j++)
		{
			string playerID = list2[j].PlayerID;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag.ContainsKey(playerID) && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[playerID])
			{
				active2 = true;
				break;
			}
		}
		FriendListHintImage.gameObject.SetActive(active);
		ContactListHintImage.gameObject.SetActive(active2);
		if (CurrentType == 2)
		{
			FriendCountText2.text = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count + "/" + OrangeConst.FRIEND_LIMIT;
		}
		else if (CurrentType == 1)
		{
			FriendCountText.text = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count + "/" + OrangeConst.FRIEND_LIMIT;
		}
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSFriendMessage, OnRSFriendInviteAgree);
	}

	private void SelectMenuList(int typ)
	{
		if (typ == 0)
		{
			MenuFriendList.SetActive(true);
			MenuInviteList.SetActive(false);
			UpdateGiveAPForListBtn();
		}
		else
		{
			MenuFriendList.SetActive(false);
			MenuInviteList.SetActive(true);
		}
	}

	private void SetTooltipFollowBtn(int typ)
	{
		for (int i = 0; i < TooltipFollowBtn.Length; i++)
		{
			TooltipFollowBtn[i].SetActive(false);
		}
		TooltipFollowBtn[typ].SetActive(true);
	}

	private void SetTooltipInviteBtn(int typ)
	{
		for (int i = 0; i < TooltipInviteBtn.Length; i++)
		{
			TooltipInviteBtn[i].SetActive(false);
		}
		TooltipInviteBtn[typ].SetActive(true);
	}

	private void SetTooltipBlackBtn(int typ)
	{
		for (int i = 0; i < TooltipBlackBtn.Length; i++)
		{
			TooltipBlackBtn[i].SetActive(false);
		}
		TooltipBlackBtn[typ].SetActive(true);
	}

	public void OnSetCurrentTouchIndex(int idx, string pid, string nam)
	{
		CurrentTouchIndex = idx;
		CurrentTouchPlayerID = pid;
		CurrentTouchPlayerName = nam;
	}

	public void OnCloseTooltip(bool b)
	{
		Tooltip.SetActive(b);
		TooltipMeun.SetActive(b);
	}

	public void OnShowTooltip(bool b, Vector3 pos)
	{
		if (CurrentType != 0)
		{
			return;
		}
		if (b)
		{
			SetTooltipFollowBtn(1);
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.ContainsKey(CurrentTouchPlayerID))
			{
				SetTooltipFollowBtn(0);
			}
			SetTooltipInviteBtn(1);
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.ContainsKey(CurrentTouchPlayerID))
			{
				SetTooltipInviteBtn(0);
				TooltipFollowBtn[0].SetActive(false);
				TooltipFollowBtn[1].SetActive(false);
			}
			SetTooltipBlackBtn(0);
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(CurrentTouchPlayerID))
			{
				SetTooltipBlackBtn(1);
				TooltipInviteBtn[0].SetActive(false);
				TooltipInviteBtn[1].SetActive(false);
			}
			Tooltip.SetActive(b);
			TooltipMeun.SetActive(b);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			Vector3 position = new Vector3(pos.x - 10f, pos.y + 2f, pos.z);
			TooltipMeun.GetComponent<RectTransform>().position = position;
		}
		else
		{
			Tooltip.SetActive(b);
			TooltipMeun.SetActive(b);
		}
	}

	private void OnSetFriendList(int typ)
	{
		switch (typ)
		{
		case 1:
			SetFriendFollowList();
			break;
		case 2:
			SetFriendHotList();
			break;
		default:
			SetFriendList();
			break;
		}
	}

	private void SetFriendList()
	{
		FriendList = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Values.ToList();
		FriendList.Sort((SocketFriendInfo x, SocketFriendInfo y) => (x.Busy <= 30 || y.Busy <= 30) ? x.Busy.CompareTo(y.Busy) : y.Busy.CompareTo(x.Busy));
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		ScrollRect.OrangeInit(ScrollRectCell, 5, FriendList.Count);
		FriendCountText.text = FriendList.Count + "/" + OrangeConst.FRIEND_LIMIT;
		UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_FRIEND_LIST"), FriendList.Count);
	}

	private void SetFriendFollowList()
	{
		FriendFollowList = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.Values.ToList();
		FriendFollowList.Sort((SocketFriendFollowInfo x, SocketFriendFollowInfo y) => (x.Busy <= 30 || y.Busy <= 30) ? x.Busy.CompareTo(y.Busy) : y.Busy.CompareTo(x.Busy));
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		ScrollRect.OrangeInit(ScrollRectCell, 5, FriendFollowList.Count);
		FriendCountText.text = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count + "/" + OrangeConst.FRIEND_LIMIT;
		UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_FAVORITE_FRIEND"), FriendFollowList.Count);
	}

	private void SetFriendHotList()
	{
		ContactList = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicContact.Values.ToList();
		ContactList.Sort((SocketContactInfo x, SocketContactInfo y) => (x.Busy <= 30 || y.Busy <= 30) ? x.Busy.CompareTo(y.Busy) : y.Busy.CompareTo(x.Busy));
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		ScrollRect.OrangeInit(ScrollRectCell, 5, ContactList.Count);
		FriendCountText.text = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count + "/" + OrangeConst.FRIEND_LIMIT;
		UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_RECENT_PLAYER"), ContactList.Count);
	}

	public void SetInviteReceiveListCommand()
	{
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteGetReceiveList());
		Invoke("SetInviteReceiveList", 1.2f);
	}

	public void SetInviteReceiveList()
	{
		if (CurrentType == 2)
		{
			FriendInviteReceiveList = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Values.ToList();
			FriendInviteReceiveList.Sort((SocketFriendInviteReceiveInfo x, SocketFriendInviteReceiveInfo y) => (x.Busy <= 30 || y.Busy <= 30) ? x.Busy.CompareTo(y.Busy) : y.Busy.CompareTo(x.Busy));
			if (ScrollRect != null)
			{
				ScrollRect.ClearCells();
			}
			ScrollRect.OrangeInit(ScrollRectCell, 5, FriendInviteReceiveList.Count);
			FriendCountText2.text = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count + "/" + OrangeConst.FRIEND_LIMIT;
			bool active = FriendInviteReceiveList.Count > 0;
			AllAgreeFriendInviteBtn.gameObject.SetActive(active);
			AllDisagreeFriendInviteBtn.gameObject.SetActive(active);
			UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_FRIEND_REQUEST"), FriendInviteReceiveList.Count);
		}
	}

	private void SetBlackList()
	{
		BlackList = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.Values.ToList();
		BlackList.Sort((SocketBlackInfo x, SocketBlackInfo y) => (x.Busy <= 30 || y.Busy <= 30) ? x.Busy.CompareTo(y.Busy) : y.Busy.CompareTo(x.Busy));
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		ScrollRect.OrangeInit(ScrollRectCell, 5, BlackList.Count);
		BlackCountText.text = BlackList.Count + "/" + OrangeConst.BLACKLIST_LIMIT;
		UpdateEmptyMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EMPTY_BLACK_LIST"), BlackList.Count);
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	public int GetCurrentTopMenu()
	{
		return CurrentTopMenu;
	}

	public int GetCurrentType()
	{
		return CurrentType;
	}

	public void OnClickTopMenu(int typ)
	{
		if (CurrentTopMenu != typ)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			OnSelectTopMenu(typ);
		}
	}

	public void OnSelectTopMenu(int typ)
	{
		Color32[] array = new Color32[2]
		{
			new Color32(52, 47, 58, byte.MaxValue),
			new Color32(185, 234, byte.MaxValue, byte.MaxValue)
		};
		for (int i = 0; i < TopMenu.Length; i++)
		{
			TopMenu[i].interactable = true;
			TopMenuText[i].color = array[1];
		}
		TopMenu[typ].interactable = false;
		TopMenuText[typ].color = array[0];
		CurrentTopMenu = typ;
		OnSetFriendList(CurrentTopMenu);
	}

	public void OnSelectTopInviteMenu(int typ)
	{
		for (int i = 0; i < TopInviteMenu.Length; i++)
		{
			TopInviteMenu[i].interactable = true;
		}
		TopInviteMenu[typ].interactable = false;
		CurrentTopInviteMenu = typ;
	}

	public void OnSelectFriendTypeBtn(int typ)
	{
		for (int i = 0; i < FriendTypeBtn.Length; i++)
		{
			FriendTypeBtn[i].SetActive(false);
		}
		FriendTypeBtn[typ].SetActive(true);
		ManagedSingleton<FriendHelper>.Instance.OnUpdateDisplayHint();
		RewardHintImage.gameObject.SetActive(ManagedSingleton<FriendHelper>.Instance.RewardDisplayHint);
	}

	public void OnSearchPlayerInfo()
	{
		OnOpenFriendSearchList();
	}

	public void OnShowPlayerInfo()
	{
		OnCloseTooltip(false);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PlayerInfoMain", delegate(PlayerInfoMainUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(CurrentTouchPlayerID);
		});
	}

	public void OnDeleteFriend()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			OnCloseTooltip(false);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Remove(CurrentTouchPlayerID);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.Remove(CurrentTouchPlayerID);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendDelete(CurrentTouchPlayerID));
			OnSetFriendList(CurrentTopMenu);
		}
	}

	public void OnDeleteFriendConfirm()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendConfirm", delegate(FriendConfirmUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			string p_title = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"));
			string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_REMOVE_FRIEND_CONFIRM"), CurrentTouchPlayerName);
			string p_textYes = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"));
			string p_textNo = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"));
			ui.SetupYesNO(p_title, p_msg, p_textYes, p_textNo, delegate
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				OnDeleteFriend();
			});
		});
	}

	public void InvokeUpdateFriendList()
	{
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBlackGetList());
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public void OnAddBlack()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			OnCloseTooltip(false);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBlackAdd(CurrentTouchPlayerID));
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Remove(CurrentTouchPlayerID);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.Remove(CurrentTouchPlayerID);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendDelete(CurrentTouchPlayerID));
			OnSetFriendList(CurrentTopMenu);
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
			Invoke("InvokeUpdateFriendList", 1.2f);
		}
	}

	public void OnAddBlackConfirm()
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
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(CurrentTouchPlayerID))
			{
				return;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendConfirm", delegate(FriendConfirmUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				string p_title = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"));
				string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_BLACK_LIST_CONFIRM"), CurrentTouchPlayerName);
				string p_textYes = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"));
				string p_textNo = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"));
				ui.SetupYesNO(p_title, p_msg, p_textYes, p_textNo, OnAddBlack, delegate
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				});
			});
		}
	}

	public void OnAddFollow()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			OnCloseTooltip(false);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFollowAdd(CurrentTouchPlayerID));
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
			OnSetFriendList(CurrentTopMenu);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
		}
	}

	public void OnDeleteFollow()
	{
		OnCloseTooltip(false);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendFollow.Remove(CurrentTouchPlayerID);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFollowDelete(CurrentTouchPlayerID));
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
		OnSetFriendList(CurrentTopMenu);
	}

	public void OnDeleteFollowConfirm()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendConfirm", delegate(FriendConfirmUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
			string p_title = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"));
			string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SOCIAL_REMOVE_FAVORITE_CONFIRM"), CurrentTouchPlayerName);
			string p_textYes = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"));
			string p_textNo = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"));
			ui.SetupYesNO(p_title, p_msg, p_textYes, p_textNo, OnDeleteFollow, delegate
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			});
		});
	}

	public void OnInviteFriend()
	{
		OnCloseTooltip(false);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInvite", delegate(FriendInviteUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(CurrentTouchPlayerID);
		});
	}

	public void OnTooltipDeleteBlack()
	{
		OnCloseTooltip(false);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		OnDeleteBlack(CurrentTouchPlayerID);
	}

	public void OnGiveReward(string pid)
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			ManagedSingleton<PlayerNetManager>.Instance.GiveAPToFriendReq(new List<string> { pid }, delegate
			{
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGiveReward(pid));
				SocketFriendRewardRequestInfo socketFriendRewardRequestInfo = new SocketFriendRewardRequestInfo
				{
					PlayerID = pid
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.Add(socketFriendRewardRequestInfo.PlayerID, socketFriendRewardRequestInfo);
			});
		}
	}

	private void UpdateGiveAPForListBtn()
	{
		List<string> list = new List<string>();
		List<string> list2 = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Keys.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.ContainsKey(list2[i]))
			{
				list.Add(list2[i]);
			}
		}
		GiveAllBtn.interactable = list.Count > 0;
	}

	public void OnGiveAPForList()
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		List<string> list = new List<string>();
		List<string> list2 = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Keys.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.ContainsKey(list2[i]))
			{
				list.Add(list2[i]);
			}
		}
		if (list.Count <= 0)
		{
			GiveAllBtn.interactable = false;
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ITEM01);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGiveRewardByList(list.ToArray()));
		for (int j = 0; j < list.Count; j++)
		{
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.ContainsKey(list[j]))
			{
				SocketFriendRewardRequestInfo socketFriendRewardRequestInfo = new SocketFriendRewardRequestInfo
				{
					PlayerID = list[j]
				};
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.Add(socketFriendRewardRequestInfo.PlayerID, socketFriendRewardRequestInfo);
			}
		}
		ManagedSingleton<PlayerNetManager>.Instance.GiveAPToFriendReq(list);
		GiveAllBtn.interactable = false;
	}

	public void OnAgreeFriendInvite(string pid)
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		if (OrangeConst.FRIEND_LIMIT <= MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_MAX"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
		else
		{
			CurrentInviteAgreePID = pid;
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteAgree(pid));
		}
	}

	public void OnRSFriendInviteAgree(object res)
	{
		if (!(res is RSFriendMessage))
		{
			return;
		}
		if (((RSFriendMessage)res).Result == 71250)
		{
			for (int i = 0; i < 5; i++)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_TARGET_MAX"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
				});
			}
			return;
		}
		if (CurrentInviteAgreePID != null)
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Remove(CurrentInviteAgreePID);
		}
		CurrentInviteAgreePID = "";
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
		SetInviteReceiveList();
		Invoke("SetInviteReceiveListCommand", 1.2f);
	}

	public void OnAllAgreeFriendInvite()
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected() || !MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		int num = OrangeConst.FRIEND_LIMIT - MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count;
		if (num <= 0 || MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Count > num)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_OVER"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < num && i < MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Count; i++)
		{
			string targetPlayerID = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Values.ToList()[i].TargetPlayerID;
			list.Add(targetPlayerID);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteAgree(targetPlayerID));
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
		Invoke("InvokeRemoveFriendInviteAllAgreeHandler", 1.2f);
	}

	public void InvokeRemoveFriendInviteAllAgreeHandler()
	{
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
		SetInviteReceiveList();
		Invoke("SetInviteReceiveListCommand", 1.2f);
	}

	public void OnDisagreeFriendInvite(string pid)
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Remove(pid);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteDisagree(pid));
			SetInviteReceiveList();
			Invoke("SetInviteReceiveListCommand", 1.2f);
		}
	}

	public void OnAllDisagreeFriendInvite()
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		List<SocketFriendInviteReceiveInfo> list = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Values.ToList();
		if (list.Count > 0)
		{
			List<string> list2 = new List<string>();
			for (int i = 0; i < list.Count; i++)
			{
				string targetPlayerID = list[i].TargetPlayerID;
				list2.Add(targetPlayerID);
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteDisagree(targetPlayerID));
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
			for (int j = 0; j < list2.Count; j++)
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteReceive.Remove(list2[j]);
			}
			SetInviteReceiveList();
			Invoke("SetInviteReceiveListCommand", 1.2f);
		}
	}

	public void OnDeleteBlack(string pid)
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.Remove(pid);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBlackDelete(pid));
			SetBlackList();
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
			Invoke("InvokeUpdateFriendList", 1.2f);
		}
	}

	public void ClearInputFieldText()
	{
		InputFieldObject.text = "";
	}

	public void OnOpenFriendSearchList()
	{
		if (InputFieldObject.text.Length <= 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INPUT_TEXT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
		else
		{
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendSearchByName, OnCreateRSFriendSearchByNameCallback, 0, true);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendSearchByName(InputFieldObject.text));
		}
	}

	public void OnCreateRSFriendSearchByNameCallback(object res)
	{
		if (!(res is RSFriendSearchByName))
		{
			return;
		}
		RSFriendSearchByName rs = (RSFriendSearchByName)res;
		if (rs.PlayerIDLength <= 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ClearInputFieldText));
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_SEARCH_FINISH"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInvite", delegate(FriendInviteUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ClearInputFieldText));
				ui.Setup(rs.PlayerID(0));
			});
		}
	}

	public void OnInviteRequestListClose()
	{
		OnSelectTopInviteMenu(1);
	}

	public void OnInviteRequestList()
	{
		OnSelectTopInviteMenu(0);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInviteList", delegate(FriendInviteListUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnInviteRequestListClose));
			ui.Setup();
		});
	}

	public void OnFriendRewardReceiveList()
	{
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveGiverListReq(delegate(List<string> plist)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInviteList", delegate(FriendInviteListUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				if (plist != null)
				{
					for (int i = 0; i < plist.Count; i++)
					{
						if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.ContainsKey(plist[i]))
						{
							MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardReceive.Remove(plist[i]);
						}
					}
					MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RewardUesdCount = plist.Count;
				}
				ui.SetupReward();
			});
		});
	}

	private void CreateNewStorageTab()
	{
		StorageInfo storageInfo = new StorageInfo("SOCIAL_MY_FRIEND", ManagedSingleton<FriendHelper>.Instance.OnGetFriendDisplayHintByType(1), 0, OnClickTab_000, OnCheckFriendDisplayHint);
		StorageInfo storageInfo2 = new StorageInfo("SOCIAL_ADD_FRIEND", false, 0, OnClickTab_001);
		StorageInfo storageInfo3 = new StorageInfo("SOCIAL_FRIEND_REQUEST", ManagedSingleton<FriendHelper>.Instance.InviteDisplayHint, 0, OnClickTab_002, OnCheckInviteDisplayHint);
		StorageInfo storageInfo4 = new StorageInfo("SOCIAL_BLACK_LIST", false, 0, OnClickTab_003);
		storageInfo.Param = new object[1] { 0 };
		storageInfo2.Param = new object[1] { 1 };
		storageInfo3.Param = new object[1] { 2 };
		storageInfo4.Param = new object[1] { 3 };
		listStorage.Add(storageInfo);
		listStorage.Add(storageInfo2);
		listStorage.Add(storageInfo3);
		listStorage.Add(storageInfo4);
		StorageGenerator.Load("StorageComp00", listStorage, 0, 0, storageRoot, delegate
		{
			Debug.Log("Load StorageComp00 Complete");
		});
	}

	public void OnClickTab_000(object p_param)
	{
		int num = (int)((StorageInfo)p_param).Param[0];
		CurrentType = 0;
		SelectMenuList(0);
		OnSelectFriendTypeBtn(0);
		OnSelectTopMenu(0);
	}

	public void OnClickTab_001(object p_param)
	{
		int num = (int)((StorageInfo)p_param).Param[0];
		CurrentType = 1;
		OnSelectTopInviteMenu(1);
		SelectMenuList(1);
	}

	public void OnClickTab_002(object p_param)
	{
		int num = (int)((StorageInfo)p_param).Param[0];
		CurrentType = 2;
		SelectMenuList(0);
		OnSelectFriendTypeBtn(1);
		SetInviteReceiveList();
	}

	public void OnClickTab_003(object p_param)
	{
		int num = (int)((StorageInfo)p_param).Param[0];
		CurrentType = 3;
		SelectMenuList(0);
		OnSelectFriendTypeBtn(2);
		SetBlackList();
	}

	public void OnClickTab_111(object p_param)
	{
		int num = (int)((StorageInfo)p_param).Param[0];
	}

	public void OnUpdateStorageHint()
	{
		StorageComponent componentInChildren = GetComponentInChildren<StorageComponent>();
		if ((bool)componentInChildren)
		{
			componentInChildren.UpdateHint();
		}
	}

	public bool OnCheckFriendDisplayHint(object[] param)
	{
		return ManagedSingleton<FriendHelper>.Instance.OnGetFriendDisplayHintByType(1);
	}

	public bool OnCheckInviteDisplayHint(object[] param)
	{
		return ManagedSingleton<FriendHelper>.Instance.InviteDisplayHint;
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
