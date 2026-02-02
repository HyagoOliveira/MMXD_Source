#define RELEASE
using UnityEngine;

public class HometopGuildUI : MonoBehaviour
{
	[SerializeField]
	private HometopUI _homeTopUI;

	private Canvas _ownCanvas;

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnJoinGuildEvent += OnJoinGuildEvent;
		Singleton<GuildSystem>.Instance.OnCreateGuildEvent += OnCreateGuildEvent;
		Singleton<GuildSystem>.Instance.OnAgreeGuildInviteEvent += OnAgreeGuildInviteEvent;
		Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent += OnConfirmChangeSceneEvent;
		_ownCanvas = GetComponent<Canvas>();
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnJoinGuildEvent -= OnJoinGuildEvent;
		Singleton<GuildSystem>.Instance.OnCreateGuildEvent -= OnCreateGuildEvent;
		Singleton<GuildSystem>.Instance.OnAgreeGuildInviteEvent -= OnAgreeGuildInviteEvent;
		Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent -= OnConfirmChangeSceneEvent;
		_ownCanvas = null;
	}

	public void OnClickGuildJoinBtn()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			_homeTopUI.OwnCanvasGroup.blocksRaycasts = false;
			Singleton<GuildSystem>.Instance.OnGetCheckGuildStateOnceEvent += OnCheckGuildStateByGuildJoin;
			Singleton<GuildSystem>.Instance.ReqCheckGuildState();
		}
	}

	public void OnClickGuildCreateBtn()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() >= OrangeConst.GUILD_FOUND_LEVEL)
			{
				_homeTopUI.OwnCanvasGroup.blocksRaycasts = false;
				Singleton<GuildSystem>.Instance.OnGetCheckGuildStateOnceEvent += OnCheckGuildStateByGuildCreate;
				Singleton<GuildSystem>.Instance.ReqCheckGuildState();
			}
			else
			{
				CommonUIHelper.ShowCommonTipUI(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK", OrangeConst.GUILD_FOUND_LEVEL), false);
			}
		}
	}

	public void OnClickGuildInviteBtn()
	{
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnCheckCommunityServerConnected())
		{
			_homeTopUI.OwnCanvasGroup.blocksRaycasts = false;
			Singleton<GuildSystem>.Instance.OnGetCheckGuildStateOnceEvent += OnCheckGuildStateByGuildInvite;
			Singleton<GuildSystem>.Instance.ReqCheckGuildState();
		}
	}

	private void OnCheckGuildStateByGuildJoin()
	{
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			Singleton<GuildSystem>.Instance.OnGetGuildInfoOnceEvent += OnGetGuildInfoByGuildJoin;
			Singleton<GuildSystem>.Instance.ReqGetGuildInfo(true);
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildJoinUI>("UI_GuildJoin", OnLoadGuildJoinUI);
		}
	}

	private void OnCheckGuildStateByGuildCreate()
	{
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			Singleton<GuildSystem>.Instance.OnGetGuildInfoOnceEvent += OnGetGuildInfoByGuildCreate;
			Singleton<GuildSystem>.Instance.ReqGetGuildInfo(true);
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildCreateUI>("UI_GuildCreate", OnLoadGuildCreateUI);
		}
	}

	private void OnCheckGuildStateByGuildInvite()
	{
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			Singleton<GuildSystem>.Instance.OnGetGuildInfoOnceEvent += OnGetGuildInfoByGuildInvite;
			Singleton<GuildSystem>.Instance.ReqGetGuildInfo(true);
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildInviteGuildListUI>("UI_GuildInviteGuildList", OnLoadGuildInviteGuildListUI);
		}
	}

	private void OnGetGuildInfoByGuildJoin()
	{
		_homeTopUI.OwnCanvasGroup.blocksRaycasts = true;
		CloseUI();
	}

	private void OnGetGuildInfoByGuildCreate()
	{
		_homeTopUI.OwnCanvasGroup.blocksRaycasts = true;
		CloseUI();
	}

	private void OnGetGuildInfoByGuildInvite()
	{
		_homeTopUI.OwnCanvasGroup.blocksRaycasts = true;
		CloseUI();
	}

	private void OnLoadGuildJoinUI(GuildJoinUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup();
		_homeTopUI.OwnCanvasGroup.blocksRaycasts = true;
	}

	private void OnLoadGuildCreateUI(GuildCreateUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup();
		_homeTopUI.OwnCanvasGroup.blocksRaycasts = true;
	}

	private void OnLoadGuildInviteGuildListUI(GuildInviteGuildListUI ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup();
		_homeTopUI.OwnCanvasGroup.blocksRaycasts = true;
	}

	private void OnJoinGuildEvent(Code ackCode)
	{
		Debug.Log("[OnJoinGuildEvent]");
		if ((uint)(ackCode - 105201) <= 1u && Singleton<GuildSystem>.Instance.GuildInfoCache != null)
		{
			LoadGuildMainScene();
		}
	}

	private void OnCreateGuildEvent(Code ackCode)
	{
		Debug.Log("[OnCreateGuildEvent]");
		if (ackCode == Code.GUILD_CREATE_SUCCESS && Singleton<GuildSystem>.Instance.GuildInfoCache != null)
		{
			LoadGuildMainScene();
		}
	}

	private void OnAgreeGuildInviteEvent(Code ackCode)
	{
		Debug.Log("[OnAgreeGuildInviteEvent]");
		if (ackCode == Code.GUILD_AGREE_INVITE_SUCCESS && Singleton<GuildSystem>.Instance.GuildInfoCache != null)
		{
			LoadGuildMainScene();
		}
	}

	private void OnConfirmChangeSceneEvent()
	{
		CloseUI();
	}

	private void LoadGuildMainScene()
	{
		if (_ownCanvas.enabled)
		{
			Singleton<GuildSystem>.Instance.OpenGuildLobbyScene();
			CloseUI();
		}
	}

	private void CloseUI()
	{
		HometopUI homeTopUI = _homeTopUI;
		if ((object)homeTopUI != null)
		{
			homeTopUI.OnClickBoard_To_Main();
		}
	}
}
