#define RELEASE
using System.Collections.Generic;
using UnityEngine;
using enums;

public class GuildLobbyUI : OrangeUIBase
{
	private enum TabIndex
	{
		None = 0,
		MemberList = 1,
		ApplyList = 2,
		InviteList = 3,
		Setup = 4,
		Log = 5
	}

	[SerializeField]
	private GuildMemberListChildUI _uiGuildMemberList;

	[SerializeField]
	private GuildApplyPlayerListChildUI _uiGuildApplyList;

	[SerializeField]
	private GuildInviteChildUI _uiGuildInvite;

	[SerializeField]
	private GuildSetupChildUI _uiGuildSetup;

	[SerializeField]
	private GuildSetupChildUI _uiGuildSetupOld;

	[SerializeField]
	private GuildLogChildUI _uiGuildLog;

	[SerializeField]
	private Transform _storageRoot;

	private StorageComponent _storage;

	private List<StorageInfo> _listStorage = new List<StorageInfo>();

	private bool _isInitializing = true;

	private TabIndex _currentTabIndex;

	public void Setup()
	{
		bool isLeader;
		bool isHeader;
		GuildHeaderPower headerPower;
		Singleton<GuildSystem>.Instance.CheckGuildPrivilege(out isLeader, out isHeader, out headerPower);
		RegenStorageTab(TabIndex.MemberList, isLeader, isHeader, headerPower);
	}

	public void OnClickSearchGuildBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildSearchUI>("UI_GuildSearch", OnSearchGuildUILoaded);
	}

	public void OnClickLeaveGuildBtn()
	{
		if (Singleton<CrusadeSystem>.Instance.CheckInEventTime())
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WARN_LEAVE"));
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnLeaveGuildConfirmUILoaded);
		}
	}

	private void OnSearchGuildUILoaded(GuildSearchUI ui)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup();
	}

	private void OnLeaveGuildConfirmUILoaded(CommonUI ui)
	{
		ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		ui.SetupYesNoByKey("COMMON_TIP", "GUILD_HALL_OUTDEFINE", "COMMON_YES", "COMMON_NO", OnLeaveGuildConfirm);
	}

	private void OnLeaveGuildConfirm()
	{
		Singleton<GuildSystem>.Instance.ReqLeaveGuild();
	}

	public void OnClickRankupBtn()
	{
		Singleton<GuildSystem>.Instance.ReqRankupGuild();
	}

	private void OnDonateUILoaded(GuildDonateUI ui)
	{
		int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
		ui.Setup(zenny);
	}

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnClickCloseBtn);
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.UPDATE_GUILD_HINT, UpdateTabRedDot);
		Singleton<GuildSystem>.Instance.OnLeaveGuildEvent += OnLeaveGuildEvent;
		Singleton<GuildSystem>.Instance.OnRemoveGuildEvent += OnRemoveGuildEvent;
		Singleton<GuildSystem>.Instance.OnGetMemberInfoListEvent += OnGetMemberInfoListEvent;
		Singleton<GuildSystem>.Instance.OnGetApplyPlayerListEvent += OnGetApplyPlayerListEvent;
		Singleton<GuildSystem>.Instance.OnGetInvitePlayerListEvent += OnGetInvitePlayerListEvent;
		Singleton<GuildSystem>.Instance.OnGetGuildLogEvent += OnGetGuildLogEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent += OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent += OnSocketHeaderPowerChangedEvent;
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnClickCloseBtn);
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.UPDATE_GUILD_HINT, UpdateTabRedDot);
		Singleton<GuildSystem>.Instance.OnLeaveGuildEvent -= OnLeaveGuildEvent;
		Singleton<GuildSystem>.Instance.OnRemoveGuildEvent -= OnRemoveGuildEvent;
		Singleton<GuildSystem>.Instance.OnGetMemberInfoListEvent -= OnGetMemberInfoListEvent;
		Singleton<GuildSystem>.Instance.OnGetApplyPlayerListEvent -= OnGetApplyPlayerListEvent;
		Singleton<GuildSystem>.Instance.OnGetInvitePlayerListEvent -= OnGetInvitePlayerListEvent;
		Singleton<GuildSystem>.Instance.OnGetGuildLogEvent -= OnGetGuildLogEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent -= OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent -= OnSocketHeaderPowerChangedEvent;
	}

	private void RegenStorageTab(TabIndex selectTabIndex, bool isLeader, bool isHeader, GuildHeaderPower headerPower)
	{
		if (_storage != null)
		{
			Object.Destroy(_storage.gameObject);
		}
		_listStorage.Clear();
		_listStorage.Add(new StorageInfo("GUILD_HALL_LIST", false, 0, OnClickTab)
		{
			Param = new object[1] { TabIndex.MemberList }
		});
		if (isLeader || (isHeader && headerPower.HasFlag(GuildHeaderPower.Apply)))
		{
			_listStorage.Add(new StorageInfo("GUILD_HALL_APPLY", false, 0, OnClickTab, CheckTabRedDot)
			{
				Param = new object[1] { TabIndex.ApplyList }
			});
		}
		if (isLeader || (isHeader && headerPower.HasFlag(GuildHeaderPower.Invite)))
		{
			_listStorage.Add(new StorageInfo("GUILD_HALL_INVITE", false, 0, OnClickTab, CheckTabRedDot)
			{
				Param = new object[1] { TabIndex.InviteList }
			});
		}
		if (isLeader)
		{
			_listStorage.Add(new StorageInfo("GUILD_HALL_SETUP", false, 0, OnClickTab)
			{
				Param = new object[1] { TabIndex.Setup }
			});
		}
		_listStorage.Add(new StorageInfo("GUILD_HALL_LOG", false, 0, OnClickTab)
		{
			Param = new object[1] { TabIndex.Log }
		});
		int p_defaultIdx = _listStorage.FindIndex((StorageInfo storageInfo) => (TabIndex)storageInfo.Param[0] == selectTabIndex);
		StorageGenerator.Load("StorageComp00", _listStorage, p_defaultIdx, 0, _storageRoot, OnStorageLoaded);
	}

	private void OnStorageLoaded(GameObject goStorage)
	{
		_storage = goStorage.GetComponent<StorageComponent>();
	}

	private void OnClearScrollRect()
	{
		_uiGuildMemberList.CloseUI();
		_uiGuildApplyList.CloseUI();
		_uiGuildInvite.CloseUI();
		_uiGuildSetup.CloseUI();
		_uiGuildSetupOld.CloseUI();
		_uiGuildLog.CloseUI();
		_uiGuildMemberList.Clear();
		_uiGuildApplyList.Clear();
		_uiGuildLog.Clear();
	}

	private void OnClickTab(object p_param)
	{
		StorageInfo storageInfo = (StorageInfo)p_param;
		SelectTab((TabIndex)storageInfo.Param[0]);
	}

	private void SelectTab(TabIndex tabIndex)
	{
		Debug.Log(string.Format("[{0}] {1}", "SelectTab", tabIndex));
		if (_currentTabIndex == tabIndex)
		{
			return;
		}
		_currentTabIndex = tabIndex;
		MonoBehaviourSingleton<UIManager>.Instance.Block(true);
		switch (_currentTabIndex)
		{
		case TabIndex.MemberList:
			if (!_isInitializing)
			{
				Singleton<GuildSystem>.Instance.ReqGetMemberInfoList();
				break;
			}
			SwitchToMemberListUI();
			_isInitializing = false;
			break;
		case TabIndex.ApplyList:
			Singleton<GuildSystem>.Instance.ReqGetApplyPlayerList();
			break;
		case TabIndex.InviteList:
			Singleton<GuildSystem>.Instance.ReqGetInvitePlayerList();
			break;
		case TabIndex.Setup:
			SwitchToSetupUI();
			break;
		case TabIndex.Log:
			Singleton<GuildSystem>.Instance.ReqGetGuildLog();
			break;
		}
	}

	private void SwitchToMemberListUI()
	{
		OnClearScrollRect();
		_uiGuildMemberList.OpenUI();
		_uiGuildMemberList.Setup();
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
	}

	private void SwitchToApplyPlayerListUI()
	{
		OnClearScrollRect();
		_uiGuildApplyList.OpenUI();
		_uiGuildApplyList.Setup();
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
	}

	private void SwitchToInvitePlayerListUI()
	{
		OnClearScrollRect();
		_uiGuildInvite.OpenUI();
		_uiGuildInvite.Setup();
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
	}

	private void SwitchToSetupUI()
	{
		OnClearScrollRect();
		_uiGuildSetup.OpenUI();
		_uiGuildSetup.Setup();
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
	}

	private void SwitchToLogUI()
	{
		OnClearScrollRect();
		_uiGuildLog.OpenUI();
		_uiGuildLog.Setup();
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
	}

	private void OnLeaveGuildEvent(Code ackCode)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnLeaveGuildEvent", ackCode));
		if ((uint)(ackCode - 105300) <= 2u)
		{
			OnClickCloseBtn();
		}
	}

	private void OnRemoveGuildEvent(Code ackCode)
	{
		Debug.Log(string.Format("[{0}] AckCode = {1}", "OnRemoveGuildEvent", ackCode));
		if (ackCode == Code.GUILD_REMOVE_SUCCESS)
		{
			OnClickCloseBtn();
		}
	}

	private void OnGetMemberInfoListEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_GET_GUILD_MEMBER_LIST_SUCCESS)
		{
			Debug.Log(string.Format("[{0}] {1}", "OnGetMemberInfoListEvent", Singleton<GuildSystem>.Instance.MemberInfoListCache.Count));
			SwitchToMemberListUI();
		}
	}

	private void OnGetApplyPlayerListEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_GET_JOIN_APPLY_LIST_SUCCESS)
		{
			Debug.Log(string.Format("[{0}] {1}", "OnGetApplyPlayerListEvent", Singleton<GuildSystem>.Instance.ApplyPlayerListCache.Count));
			SwitchToApplyPlayerListUI();
		}
	}

	private void OnGetInvitePlayerListEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_GET_INVITE_LIST_SUCCESS)
		{
			Debug.Log(string.Format("[{0}] {1}", "OnGetInvitePlayerListEvent", Singleton<GuildSystem>.Instance.InvitePlayerListCache.Count));
			SwitchToInvitePlayerListUI();
		}
	}

	private void OnGetGuildLogEvent(Code ackCode, List<NetGuildLog> guildLogList)
	{
		Debug.Log(string.Format("[{0}] {1}", "OnGetGuildLogEvent", guildLogList.Count));
		if (ackCode == Code.GUILD_GET_LOG_SUCCESS)
		{
			SwitchToLogUI();
		}
	}

	private void OnSocketMemberPrivilegeChangedEvent(bool isSelfPrivilegeChanged)
	{
		if (isSelfPrivilegeChanged)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_RANKALTER"));
			bool isLeader;
			bool isHeader;
			GuildHeaderPower headerPower;
			Singleton<GuildSystem>.Instance.CheckGuildPrivilege(out isLeader, out isHeader, out headerPower);
			TabIndex selectTabIndex = CheckTabPrivilege(isLeader, isHeader, headerPower);
			RegenStorageTab(selectTabIndex, isLeader, isHeader, headerPower);
		}
	}

	private void OnSocketHeaderPowerChangedEvent()
	{
		bool isLeader;
		bool isHeader;
		GuildHeaderPower headerPower;
		Singleton<GuildSystem>.Instance.CheckGuildPrivilege(out isLeader, out isHeader, out headerPower);
		if (!isLeader && isHeader)
		{
			TabIndex selectTabIndex = CheckTabPrivilege(isLeader, isHeader, headerPower);
			RegenStorageTab(selectTabIndex, isLeader, isHeader, headerPower);
		}
	}

	private TabIndex CheckTabPrivilege(bool isLeader, bool isHeader, GuildHeaderPower headerPower)
	{
		if (isHeader)
		{
			switch (_currentTabIndex)
			{
			case TabIndex.ApplyList:
				if (!headerPower.HasFlag(GuildHeaderPower.Apply))
				{
					return TabIndex.MemberList;
				}
				break;
			case TabIndex.InviteList:
				if (!headerPower.HasFlag(GuildHeaderPower.Invite))
				{
					return TabIndex.MemberList;
				}
				break;
			case TabIndex.Setup:
				if (!isLeader)
				{
					return TabIndex.MemberList;
				}
				break;
			default:
				return TabIndex.MemberList;
			case TabIndex.MemberList:
			case TabIndex.Log:
				break;
			}
		}
		else
		{
			TabIndex currentTabIndex = _currentTabIndex;
			if (currentTabIndex != TabIndex.MemberList && currentTabIndex != TabIndex.Log)
			{
				return TabIndex.MemberList;
			}
		}
		return _currentTabIndex;
	}

	private void UpdateTabRedDot(bool isSocketEvent)
	{
		_storage.UpdateHint();
	}

	private bool CheckTabRedDot(object[] param)
	{
		if (param == null || param.Length == 0)
		{
			return false;
		}
		TabIndex tabIndex = (TabIndex)param[0];
		if (tabIndex == TabIndex.ApplyList)
		{
			return Singleton<GuildSystem>.Instance.HasApplyPlayer;
		}
		return false;
	}

	public void OnClickToggle()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
	}
}
