using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cc;

public class FriendInviteUI : OrangeUIBase
{
	[SerializeField]
	private Text PlayerNameText;

	[SerializeField]
	private Text PlayePowerText;

	[SerializeField]
	private Text PlayeLevelText;

	[SerializeField]
	private Text InviteMessageText;

	[SerializeField]
	private Button InviteConfirmBtn;

	[SerializeField]
	private GameObject PlayerIcon;

	[SerializeField]
	private InputField InputFieldText;

	[SerializeField]
	private Transform PlayerSignRoot;

	[SerializeField]
	private GameObject SignObject;

	private string TargetID;

	public void SetPlayerSignIcon(int n_ID = 0, bool bOwner = false)
	{
		if (PlayerSignRoot != null && SignObject != null)
		{
			int childCount = PlayerSignRoot.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Object.Destroy(PlayerSignRoot.transform.GetChild(i).gameObject);
			}
			if (n_ID > 0)
			{
				GameObject obj = Object.Instantiate(SignObject, PlayerSignRoot.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(PlayerSignRoot);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<CommonSignBase>().SetupSign(n_ID, bOwner);
			}
		}
	}

	public void Setup(string pid)
	{
		InputFieldText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_MESSAGE_PRESET");
		TargetID = pid;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerHUD, OnCreateRSGetTargetInfoCallback, 0, true);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUD(TargetID));
	}

	private void OnEnable()
	{
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			if (MonoBehaviourSingleton<UIManager>.Instance.IsActive("UI_GuildMain") || MonoBehaviourSingleton<UIManager>.Instance.IsActive("UI_Channel"))
			{
				Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent += OnSocketMemberKickedEvent;
				Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent += OnSocketGuildRemovedEvent;
			}
		}
		else
		{
			Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent += OnConfirmChangeSceneEvent;
		}
	}

	private void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent -= OnConfirmChangeSceneEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent -= OnSocketMemberKickedEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent -= OnSocketGuildRemovedEvent;
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	public void OnCreateRSGetTargetInfoCallback(object res)
	{
		if (!(res is RSGetPlayerHUD))
		{
			return;
		}
		RSGetPlayerHUD rSGetPlayerHUD = (RSGetPlayerHUD)res;
		if (rSGetPlayerHUD.Result == 70350)
		{
			OnClickCloseBtn();
			return;
		}
		SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(rSGetPlayerHUD.PlayerHUD);
		if (socketPlayerHUD == null)
		{
			OnClickCloseBtn();
			return;
		}
		PlayerNameText.text = socketPlayerHUD.m_Name;
		PlayePowerText.text = socketPlayerHUD.m_Power.ToString();
		PlayeLevelText.text = "Lv" + socketPlayerHUD.m_Level;
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(PlayerIcon.transform, socketPlayerHUD.m_IconNumber, new Vector3(0.7f, 0.7f, 0.7f), false);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnInviteConfirm()
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			return;
		}
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Count >= OrangeConst.FRIEND_LIMIT)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_MAX"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicBlack.ContainsKey(TargetID))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_BLACK_LIST"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
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
		InviteConfirmBtn.interactable = false;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendInviteRequest.ContainsKey(TargetID))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
				ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				ui.MuteSE = true;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ADD_FRIEND_FINISH"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), OnClickCloseBtn);
			});
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendMessage, OnFriendInvitecCallback);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInvite(TargetID, InviteMessageText.text));
		}
	}

	public void OnFriendInvitecCallback(object res)
	{
		if (res is RSFriendMessage)
		{
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSFriendMessage, OnFriendInvitecCallback);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendInviteGetRequestList, OnRSFriendInviteGetRequestList);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendInviteGetRequestList());
		}
	}

	public void OnRSFriendInviteGetRequestList(object res)
	{
		if (res is RSFriendInviteGetRequestList)
		{
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSFriendInviteGetRequestList, OnRSFriendInviteGetRequestList);
			ManagedSingleton<FriendHelper>.Instance.OnUpdateFriendInviteRequest(res);
			base.CloseSE = SystemSE.NONE;
			OnClickCloseBtn();
		}
	}

	private void OnConfirmChangeSceneEvent()
	{
		OnClickCloseBtn();
	}

	private void OnSocketMemberKickedEvent(string memberId, bool isSelf)
	{
		if (isSelf)
		{
			OnClickCloseBtn();
		}
	}

	private void OnSocketGuildRemovedEvent()
	{
		OnClickCloseBtn();
	}
}
