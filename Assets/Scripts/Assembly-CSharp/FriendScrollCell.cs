#define RELEASE
using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class FriendScrollCell : ScrollIndexCallback
{
	public enum FriendSelectType : short
	{
		FriendList = 0,
		InviteReceive = 1,
		InviteRequest = 2,
		BlackList = 3
	}

	private FriendMainUI parentFriendMainUI;

	[SerializeField]
	private Text PlayerName;

	[SerializeField]
	private Button RewardBtn;

	[SerializeField]
	private Text RewardText;

	[SerializeField]
	private Text InviteMessageText;

	[SerializeField]
	private GameObject[] OnlineImage;

	[SerializeField]
	private Text StatusMessage;

	[SerializeField]
	private GameObject PlayerIcon;

	[SerializeField]
	private Text PlayerLevelText;

	[SerializeField]
	private GameObject[] ButtonObject;

	[SerializeField]
	private GameObject NewChatFlagObject;

	[SerializeField]
	private Button AgreeInviteBtn;

	[SerializeField]
	private Image[] LineImage;

	[SerializeField]
	private Transform PlayerSignRoot;

	[SerializeField]
	private GameObject SignObject;

	private int idx;

	private string pid = "";

	private SocketPlayerHUD pHUD;

	private string InviteMessage;

	private Color32[] colors = new Color32[5]
	{
		new Color32(252, 242, 0, byte.MaxValue),
		new Color32(117, 111, 29, byte.MaxValue),
		new Color32(183, 183, 183, byte.MaxValue),
		new Color32(92, byte.MaxValue, 222, byte.MaxValue),
		new Color32(107, 194, 222, byte.MaxValue)
	};

	public void SetPlayerSignIcon(int n_ID = 0, bool bOwner = false)
	{
		if (PlayerSignRoot != null && SignObject != null)
		{
			int childCount = PlayerSignRoot.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(PlayerSignRoot.transform.GetChild(i).gameObject);
			}
			if (n_ID > 0)
			{
				GameObject obj = UnityEngine.Object.Instantiate(SignObject, PlayerSignRoot.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(PlayerSignRoot);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<CommonSignBase>().SetupSign(n_ID, bOwner);
			}
		}
	}

	private void Update()
	{
		NewChatFlagObject.SetActive(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetFriendChatIconFlag(pid));
		bool flag = !MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.ContainsKey(pid);
		RewardBtn.interactable = flag;
		RewardText.color = (flag ? colors[0] : colors[1]);
	}

	private void SetButton(int typ)
	{
		for (int i = 0; i < ButtonObject.Length; i++)
		{
			ButtonObject[i].SetActive(false);
		}
		ButtonObject[typ].SetActive(true);
	}

	private void SetStatusMessage(int busy)
	{
		string statusMessage = string.Empty;
		if (ManagedSingleton<PlayerHelper>.Instance.GetOnlineStatus(busy, out statusMessage))
		{
			OnlineImage[0].SetActive(false);
			OnlineImage[1].SetActive(true);
			StatusMessage.color = colors[3];
		}
		else
		{
			OnlineImage[0].SetActive(true);
			OnlineImage[1].SetActive(false);
			StatusMessage.color = colors[2];
		}
		StatusMessage.text = statusMessage;
	}

	public override void ScrollCellIndex(int p_idx)
	{
		parentFriendMainUI = GetComponentInParent<FriendMainUI>();
		idx = p_idx;
		base.name = "friend_" + p_idx;
		InviteMessage = "";
		InviteMessageText.gameObject.SetActive(false);
		int currentType = parentFriendMainUI.GetCurrentType();
		switch (currentType)
		{
		case 3:
		{
			SocketBlackInfo socketBlackInfo = parentFriendMainUI.BlackList[p_idx];
			pHUD = JsonHelper.Deserialize<SocketPlayerHUD>(socketBlackInfo.FriendPlayerHUD);
			SetStatusMessage(socketBlackInfo.Busy);
			pid = socketBlackInfo.BlackPlayerID;
			SetButton(2);
			break;
		}
		case 2:
		{
			SocketFriendInviteReceiveInfo socketFriendInviteReceiveInfo = parentFriendMainUI.FriendInviteReceiveList[p_idx];
			pHUD = JsonHelper.Deserialize<SocketPlayerHUD>(socketFriendInviteReceiveInfo.FriendPlayerHUD);
			pid = socketFriendInviteReceiveInfo.TargetPlayerID;
			SetStatusMessage(socketFriendInviteReceiveInfo.Busy);
			InviteMessage = socketFriendInviteReceiveInfo.InviteMessage;
			InviteMessageText.gameObject.SetActive(true);
			OrangeDataReader.Instance.BlurChatMessage(ref InviteMessage);
			InviteMessageText.text = InviteMessage;
			SetButton(1);
			break;
		}
		default:
		{
			int currentTopMenu = parentFriendMainUI.GetCurrentTopMenu();
			if (1 == currentTopMenu)
			{
				SocketFriendFollowInfo socketFriendFollowInfo = parentFriendMainUI.FriendFollowList[p_idx];
				pHUD = JsonHelper.Deserialize<SocketPlayerHUD>(socketFriendFollowInfo.FriendPlayerHUD);
				SetStatusMessage(socketFriendFollowInfo.Busy);
				pid = socketFriendFollowInfo.FriendPlayerID;
				RewardBtn.gameObject.SetActive(true);
			}
			else if (2 == currentTopMenu)
			{
				SocketContactInfo socketContactInfo = parentFriendMainUI.ContactList[p_idx];
				pHUD = JsonHelper.Deserialize<SocketPlayerHUD>(socketContactInfo.PlayerHUD);
				SetStatusMessage(socketContactInfo.Busy);
				pid = socketContactInfo.PlayerID;
				RewardBtn.gameObject.SetActive(false);
			}
			else
			{
				SocketFriendInfo socketFriendInfo = parentFriendMainUI.FriendList[p_idx];
				pHUD = JsonHelper.Deserialize<SocketPlayerHUD>(socketFriendInfo.FriendPlayerHUD);
				SetStatusMessage(socketFriendInfo.Busy);
				pid = socketFriendInfo.FriendPlayerID;
				RewardBtn.gameObject.SetActive(true);
			}
			RewardText.text = "x" + OrangeConst.GIFT_AP;
			RewardText.color = colors[0];
			RewardBtn.interactable = !MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRewardRequest.ContainsKey(pid);
			if (!RewardBtn.interactable)
			{
				RewardText.color = colors[1];
			}
			SetButton(0);
			NewChatFlagObject.SetActive(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetFriendChatIconFlag(pid));
			break;
		}
		case 1:
			break;
		}
		if (pHUD == null)
		{
			pHUD = new SocketPlayerHUD();
			pHUD.m_Name = "";
			pHUD.m_StandbyCharID = 1;
			pHUD.m_Level = 1;
			pHUD.m_IconNumber = 900001;
		}
		PlayerName.text = pHUD.m_Name;
		PlayerLevelText.text = "Lv" + pHUD.m_Level;
		SetPlayerSignIcon(pHUD.m_TitleNumber);
		bool flag = currentType != 2;
		LineImage[0].gameObject.SetActive(flag);
		LineImage[1].gameObject.SetActive(!flag);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(PlayerIcon.transform, pHUD.m_IconNumber, new Vector3(0.7f, 0.7f, 0.7f), false, OnShowTooltip);
	}

	public void OnShowTooltip()
	{
		parentFriendMainUI.OnSetCurrentTouchIndex(idx, pid, pHUD.m_Name);
		Vector3 position = PlayerName.GetComponent<RectTransform>().position;
		parentFriendMainUI.OnShowTooltip(true, position);
	}

	public void OnGiveReward()
	{
		RewardBtn.interactable = false;
		RewardText.color = colors[1];
		parentFriendMainUI.OnGiveReward(pid);
	}

	public void OnAgreeFriendInvite()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(pid))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_BLACK_LIST"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
		parentFriendMainUI.OnAgreeFriendInvite(pid);
		if (OrangeConst.FRIEND_LIMIT > MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count)
		{
			AgreeInviteBtn.interactable = false;
		}
	}

	public void OnDisagreeFriendInvite()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
		parentFriendMainUI.OnDisagreeFriendInvite(pid);
	}

	public void OnDeleteBlack()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		parentFriendMainUI.OnDeleteBlack(pid);
	}

	public void OnDeleteBlackConfirm()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendConfirm", delegate(FriendConfirmUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			string p_title = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"));
			string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REMOVE_BLACK_LIST_CONFIRM"), pHUD.m_Name);
			string p_textYes = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"));
			string p_textNo = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"));
			ui.SetupYesNO(p_title, p_msg, p_textYes, p_textNo, delegate
			{
				ui.CloseSE = SystemSE.NONE;
				OnDeleteBlack();
			});
		});
	}

	public void OnCloseFriendChatIconFlag()
	{
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[pid] = false;
		ManagedSingleton<FriendHelper>.Instance.OnUpdateDisplayHint(3);
		NewChatFlagObject.SetActive(ManagedSingleton<FriendHelper>.Instance.ChatDisplayHint);
	}

	public void OnClickChannelBtn()
	{
		Debug.Log("OnClickChannelBtn");
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnGetFriendChatIconFlag(pid))
		{
			NewChatFlagObject.SetActive(false);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendChatIconFlag[pid] = false;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Channel", delegate(ChannelUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnCloseFriendChatIconFlag));
			ui.Setup(pid, PlayerName.text);
		});
	}
}
