#define RELEASE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OrangeApi;
using OrangeSocket;
using UnityEngine;
using cc;
using enums;

public class GuildSystem : Singleton<GuildSystem>
{
	private class MemberInfoComparer : Comparer<NetMemberInfo>
	{
		public override int Compare(NetMemberInfo x, NetMemberInfo y)
		{
			if (x.Privilege != y.Privilege)
			{
				if (x.Privilege <= y.Privilege)
				{
					return -1;
				}
				return 1;
			}
			SocketPlayerHUD value;
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(x.MemberId, out value))
			{
				return 1;
			}
			SocketPlayerHUD value2;
			if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(y.MemberId, out value2))
			{
				return -1;
			}
			if (value.m_Level != value2.m_Level)
			{
				if (value.m_Level <= value2.m_Level)
				{
					return 1;
				}
				return -1;
			}
			int value3;
			if (!Singleton<GuildSystem>.Instance.PlayerBusyStatusCache.TryGetValue(x.MemberId, out value3))
			{
				return 1;
			}
			int value4;
			if (!Singleton<GuildSystem>.Instance.PlayerBusyStatusCache.TryGetValue(y.MemberId, out value4))
			{
				return -1;
			}
			if (value3 > value4)
			{
				return 1;
			}
			return -1;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass325_0
	{
		public bool isSelf;

		public GuildEventMemberKicked data;

		public GuildSystem _003C_003E4__this;

		internal void _003COnSocketGuildEvent_003Eg__action_007C0()
		{
			if (isSelf)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_WARN2"), _003C_003E4__this.TIP_DIALOG_TIME);
			}
			Action<string, bool> onSocketMemberKickedEvent = _003C_003E4__this.OnSocketMemberKickedEvent;
			if (onSocketMemberKickedEvent != null)
			{
				onSocketMemberKickedEvent(data.MemberId, isSelf);
			}
		}
	}

	private const int GUILD_ID_UNDEFINED = 0;

	private const int GUILD_PRIVILEGE_UNDEFINED = -1;

	public GuildMainSceneController MainSceneController;

	private int _guildId;

	public bool DisableCheckLeaderReplace;

	private bool _loadGuildSceneAfterGetGuildInfo;

	private bool _guildStateChanged;

	private MemberInfoComparer _memberInfoComparer = new MemberInfoComparer();

	private NetGuildInfo _guildInfoCache;

	private List<NetMemberInfo> _memberInfoListCache = new List<NetMemberInfo>();

	private List<NetGuildJoinMessageInfo> _inviteGuildListCache = new List<NetGuildJoinMessageInfo>();

	private List<NetPlayerJoinMessageInfo> _applyPlayerListCache = new List<NetPlayerJoinMessageInfo>();

	public int SearchGuildListCount;

	public List<NetGuildInfo> SearchGuildListCache = new List<NetGuildInfo>();

	private readonly float TIP_DIALOG_TIME = 2f;

	private ConcurrentQueue<Action<List<SocketGuildInfo>>> _onSocketGetGuildInfoResCallbackQueue = new ConcurrentQueue<Action<List<SocketGuildInfo>>>();

	private ConcurrentQueue<Action<List<SocketGuildMemberInfo>>> _onSocketGetGuildMemberInfoResCallbackQueue = new ConcurrentQueue<Action<List<SocketGuildMemberInfo>>>();

	private ConcurrentQueue<Action<List<string>>> _onSocketGetGuildPlayerIdListResCallbackQueue = new ConcurrentQueue<Action<List<string>>>();

	private ConcurrentQueue<Action> _onSocketRefreshCommunitySocketInfoCallbackQueue = new ConcurrentQueue<Action>();

	[CompilerGenerated]
	private readonly List<GuildTutorialInfo> _003CTutorialInfoList_003Ek__BackingField = (from table in ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.Values
		where table.s_TRIGGER_KEY.StartsWith("Guild_")
		orderby table.n_ID
		select new GuildTutorialInfo
		{
			IsMain = (table.s_TRIGGER_KEY == "Guild_Main"),
			TutorialID = table.n_ID,
			TriggerKey = table.s_TRIGGER_KEY,
			IsBuildingShowUp = table.s_TRIGGER_KEY.StartsWith("Guild_GU")
		}).ToList();

	public bool HasGuild
	{
		get
		{
			return GuildId > 0;
		}
	}

	public int GuildId
	{
		get
		{
			return _guildId;
		}
		private set
		{
			bool num = value > 0 && _guildId != value;
			_guildId = value;
			if (num)
			{
				ReqGetGuildRankRead();
				Singleton<CrusadeSystem>.Instance.RetrieveCrusadeInfo();
				Singleton<PowerTowerSystem>.Instance.ReqGetPowerPillarInfo();
			}
		}
	}

	public bool HasInviteGuild { get; private set; }

	public bool HasApplyPlayer { get; private set; }

	public bool HasEddieReward { get; private set; }

	public int GuildRankRead { get; private set; }

	public GuildSetting GuildSetting { get; private set; }

	public NetMemberInfo SelfMemberInfo { get; private set; }

	public NetGuildInfo GuildInfoCache
	{
		get
		{
			return _guildInfoCache;
		}
		private set
		{
			bool num = _guildInfoCache == null && value != null;
			_guildInfoCache = value;
			GuildId = ((_guildInfoCache != null) ? _guildInfoCache.GuildID : 0);
			if (num)
			{
				ReqCheckReplaceLeader();
			}
		}
	}

	public List<NetBuildingInfo> BuildingInfoListCache { get; private set; } = new List<NetBuildingInfo>();


	public List<NetMemberInfo> MemberInfoListCache
	{
		get
		{
			return _memberInfoListCache;
		}
		private set
		{
			_memberInfoListCache = value;
			if (_memberInfoListCache != null && _memberInfoListCache.Count > 0)
			{
				_memberInfoListCache.Sort(_memberInfoComparer);
				SelfMemberInfo = null;
				if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData != null && !string.IsNullOrEmpty(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID))
				{
					NetMemberInfo memberInfo;
					if (TryGetMemberInfo(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID, out memberInfo))
					{
						SelfMemberInfo = memberInfo;
					}
					else
					{
						Debug.LogError("Failed to get NetMemberInfo of Self");
					}
				}
			}
			else
			{
				SelfMemberInfo = null;
			}
		}
	}

	public List<NetGuildJoinMessageInfo> InviteGuildListCache
	{
		get
		{
			return _inviteGuildListCache;
		}
		private set
		{
			_inviteGuildListCache = value;
			HasInviteGuild = _inviteGuildListCache.Count > 0;
		}
	}

	public List<NetGuildJoinMessageInfo> ApplyGuildListCache { get; private set; } = new List<NetGuildJoinMessageInfo>();


	public List<NetGuildLog> LogListCache { get; private set; } = new List<NetGuildLog>();


	public List<NetPlayerJoinMessageInfo> ApplyPlayerListCache
	{
		get
		{
			return _applyPlayerListCache;
		}
		private set
		{
			_applyPlayerListCache = value;
			HasApplyPlayer = _applyPlayerListCache.Count > 0;
		}
	}

	public List<NetPlayerJoinMessageInfo> InvitePlayerListCache { get; private set; } = new List<NetPlayerJoinMessageInfo>();


	public Dictionary<string, int> PlayerBusyStatusCache { get; private set; } = new Dictionary<string, int>();


	public List<GuildTutorialInfo> TutorialInfoList
	{
		[CompilerGenerated]
		get
		{
			return _003CTutorialInfoList_003Ek__BackingField;
		}
	}

	public event Action<Code> OnGetCheckGuildStateEvent;

	public event Action OnGetCheckGuildStateOnceEvent;

	public event Action<Code> OnGetGuildInfoEvent;

	public event Action OnGetGuildInfoOnceEvent;

	public event Action<Code> OnCreateGuildEvent;

	public event Action<Code, NetGuildInfo> OnRankupGuildEvent;

	public event Action<Code, NetGuildInfo> OnEditBadgeEvent;

	public event Action<Code> OnJoinGuildEvent;

	public event Action<Code> OnCancelJoinGuildEvent;

	public event Action<Code> OnLeaveGuildEvent;

	public event Action<Code> OnRemoveGuildEvent;

	public event Action<Code> OnGetMemberInfoListEvent;

	public event Action<Code> OnGetApplyGuildListEvent;

	public event Action<Code> OnGetInviteGuildListEvent;

	public event Action<Code> OnAgreeGuildInviteEvent;

	public event Action<Code> OnRefuseGuildInviteEvent;

	public event Action<Code> OnInvitePlayerEvent;

	public event Action<Code> OnCancelInvitePlayerEvent;

	public event Action<Code> OnSearchGuildEvent;

	public event Action<Code> OnGetInvitePlayerListEvent;

	public event Action<Code> OnGetApplyPlayerListEvent;

	public event Action<Code> OnAgreePlayerApplyEvent;

	public event Action<Code> OnRefusePlayerApplyEvent;

	public event Action<Code> OnChangeMemberPrivilegeEvent;

	public event Action<Code, string> OnKickMemberEvent;

	public event Action<Code, NetGuildInfo> OnChangeGuildNameEvent;

	public event Action<Code, NetGuildInfo> OnChangeGuildPowerDemandEvent;

	public event Action<Code, NetGuildInfo> OnChangeGuildApplyTypeEvent;

	public event Action<Code, NetGuildInfo> OnChangeGuildHeaderPowerEvent;

	public event Action<Code, NetGuildInfo> OnChangeGuildAnnouncementEvent;

	public event Action<Code, NetGuildInfo> OnChangeGuildIntroductionEvent;

	public event Action<Code, List<NetGuildLog>> OnGetGuildLogEvent;

	public event Action<Code, NetGuildInfo> OnDonateEvent;

	public event Action<Code, NetGuildInfo> OnEddieDonateEvent;

	public event Action<Code, int, int, List<NetEddieBoxGachaRecord>, string> OnGetEddieBoxGachaRecordEvent;

	public event Action<Code, List<NetRewardInfo>> OnGetEddieRewardEvent;

	public event Action OnConfirmChangeSceneEvent;

	public event Action OnSocketMemberJoinedEvent;

	public event Action<string, bool> OnSocketMemberKickedEvent;

	public event Action<bool> OnSocketMemberPrivilegeChangedEvent;

	public event Action OnSocketHeaderPowerChangedEvent;

	public event Action OnSocketGuildRemovedEvent;

	public event Action OnSocketGuildRankupEvent;

	public GuildSystem()
	{
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnServerConnectedEvent += OnCommunityServerReconnected;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSSetGuildId, OnSocketSetGuildIdRes);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSSetGuildInfo, OnSocketSetGuildInfoRes);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetGuildInfo, OnSocketGetGuildInfoRes);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSSetGuildMemberInfo, OnSocketSetGuildMemberInfoRes);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetGuildMemberInfo, OnSocketGetGuildMemberInfoRes);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSSetGuildPlayerId, OnSocketSetGuildPlayerIdRes);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetGuildPlayerIdList, OnSocketGetGuildPlayerIdListRes);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.NTGuildEvent, OnSocketGuildEvent);
	}

	private void OnCommunityServerReconnected()
	{
		Debug.LogWarning("[OnCommunityServerReconnected]");
		ReqCheckGuildState();
	}

	public void OpenGuildLobbyScene(Action onFinished = null)
	{
		GuildUIHelper.OpenLoadingUI(delegate
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_CANVAS, false);
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.AdditiveScene("GuildMain", delegate
			{
				GuildUIHelper.CloseLoadingUI(onFinished);
			}, string.Format("prefab/fx/{0}", GuildMainSceneNPCController.FxName.fx_guild_teleport_in), string.Format("prefab/fx/{0}", GuildMainSceneNPCController.FxName.fx_guild_teleport_out), string.Format("prefab/fx/{0}", GuildMainSceneController.FxName.fx_guild_building_showup), string.Format("prefab/fx/{0}", GuildMainSceneController.FxName.fx_guild_building_levelup));
		});
	}

	public void CloseGuildLobbyScene(Action onFinished = null)
	{
		GuildUIHelper.OpenLoadingUI(delegate
		{
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.UnloadScene("GuildMain", delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_CANVAS, true);
				GuildUIHelper.CloseLoadingUI(onFinished);
			});
		});
	}

	public void ClearCacheData()
	{
		Debug.Log("[GuildSystem.ClearCacheData]");
		HasEddieReward = false;
		GuildId = 0;
		GuildInfoCache = null;
		GuildSetting = null;
		BuildingInfoListCache.Clear();
		MemberInfoListCache = new List<NetMemberInfo>();
		InvitePlayerListCache.Clear();
		ApplyGuildListCache.Clear();
		ApplyPlayerListCache = new List<NetPlayerJoinMessageInfo>();
		LogListCache.Clear();
	}

	private void ReplaceMemberInfo(List<NetMemberInfo> changedMemberInfos)
	{
		if (changedMemberInfos == null)
		{
			Debug.LogError("Try replace with null MemberInfos'");
			return;
		}
		List<NetMemberInfo> list = MemberInfoListCache.ToList();
		foreach (NetMemberInfo changedMemberInfo in changedMemberInfos)
		{
			int num = list.FindIndex((NetMemberInfo memberInfo) => memberInfo.MemberId == changedMemberInfo.MemberId);
			if (num >= 0)
			{
				list.RemoveAt(num);
				list.Insert(num, changedMemberInfo);
			}
		}
		MemberInfoListCache = list;
	}

	public void ReqCheckGuildState()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqCheckGuildState(OnGetCheckGuildStateRes);
	}

	public void ReqGetGuildInfo(bool needLoadGuildScene = false)
	{
		_loadGuildSceneAfterGetGuildInfo = needLoadGuildScene;
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqGetGuildInfo(OnGetGuildInfoRes);
	}

	public void ReqGetMemberInfoList()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqGetMemberInfoList(OnGetMemberInfoListRes);
	}

	public void ReqGetApplyPlayerList()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqGetApplyPlayerList(OnGetApplyPlayerListRes);
	}

	public void ReqAgreePlayerApply()
	{
		List<string> list = ApplyPlayerListCache.Select((NetPlayerJoinMessageInfo info) => info.PlayerID).ToList();
		if (list.Count != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			ReqAgreePlayerApply(list);
		}
	}

	public void ReqAgreePlayerApply(string playerId)
	{
		ReqAgreePlayerApply(new List<string> { playerId });
	}

	public void ReqAgreePlayerApply(List<string> playerIdList)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqAgreePlayerApply(playerIdList, OnAgreePlayerApplyRes);
	}

	public void ReqRefusePlayerApply()
	{
		List<string> list = ApplyPlayerListCache.Select((NetPlayerJoinMessageInfo info) => info.PlayerID).ToList();
		if (list.Count != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
			ReqRefusePlayerApply(list);
		}
	}

	public void ReqRefusePlayerApply(string playerId)
	{
		ReqRefusePlayerApply(new List<string> { playerId });
	}

	public void ReqRefusePlayerApply(List<string> playerIdList)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqRefusePlayerApply(playerIdList, OnRefusePlayerApplyRes);
	}

	public void ReqGetInvitePlayerList()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqGetInvitePlayerList(OnGetInvitePlayerListRes);
	}

	public void ReqEditBadge(int badgeIndex, float badgeColor)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqEditBadge(badgeIndex, Mathf.RoundToInt(badgeColor * 360f), OnEditBadgeRes);
	}

	public void ReqCreateGuild(string guildName, string introdution, string announcement, int badgeIndex, float badgeColor, GuildApplyType applyType, int powerDemand)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqCreateGuild(guildName, introdution, announcement, badgeIndex, Mathf.RoundToInt(badgeColor * 360f), (int)applyType, powerDemand, OnCreateGuildRes);
	}

	public void ReqJoinGuild(int guildId, int power, string applyMsg = "")
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqJoinGuild(guildId, power, applyMsg, OnJoinGuildRes);
	}

	public void ReqCancelJoinGuild(int guildId)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqCancelJoinGuild(guildId, OnCancelJoinGuildRes);
	}

	public void ReqRankupGuild()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqRankupGuild(OnRankupGuildRes);
	}

	public void ReqLeaveGuild()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqLeaveGuild(OnLeaveGuildRes);
	}

	public void ReqRemoveGuild()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqRemoveGuild(OnRemoveGuildRes);
	}

	public void ReqGetApplyGuildList()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqGetApplyGuildList(OnGetApplyGuildListRes);
	}

	public void ReqGetInviteGuildList()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqGetInviteGuildList(OnGetInviteGuildListRes);
	}

	public void ReqAgreeGuildInvite(int guildId)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqAgreeGuildInvite(guildId, OnAgreeGuildInviteRes);
	}

	public void ReqRefuseGuildInvite(int guildId)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqRefuseGuildInvite(guildId, OnRefuseGuildInviteRes);
	}

	public void ReqInvitePlayer(string playerId, string inviteMsg)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqInvitePlayer(playerId, inviteMsg, OnInvitePlayerRes);
	}

	public void ReqCancelInvitePlayer()
	{
		ReqCancelInvitePlayer(InvitePlayerListCache.Select((NetPlayerJoinMessageInfo info) => info.PlayerID).ToList());
	}

	public void ReqCancelInvitePlayer(string playerId)
	{
		ReqCancelInvitePlayer(new List<string> { playerId });
	}

	public void ReqCancelInvitePlayer(List<string> playerIdList)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqCancelInvitePlayer(playerIdList, OnCancelInvitePlayerRes);
	}

	public void ReqSearchGuild(string searchString, int offset = 0, int maxPower = 0)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqSearchGuild(searchString, offset, maxPower, OnSearchGuildRes);
	}

	public void ReqChangeGuildName(string newGuildName)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqChangeGuildName(newGuildName, OnChangeGuildNameRes);
	}

	public void ReqChangeMemberPrivilege(string playerId, int privilege)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqChangeMemberPrivilege(playerId, privilege, OnChangeMemberPrivilegeRes);
	}

	public void ReqKickMember(string playerId)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqKickMember(playerId, OnKickMemberRes);
	}

	public void ReqChangeGuildPowerDemand(int powerDemand)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqChangeGuildPowerDemand(powerDemand, OnChangeGuildPowerDemandRes);
	}

	public void ReqChangeGuildApplyType(GuildApplyType applyType)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqChangeApplyType((int)applyType, OnChangeGuildApplyTypeRes);
	}

	public void ReqChangeGuildHeaderPower(int headerPower)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqChangeHeaderPower(headerPower, OnChangeHeaderPowerRes);
	}

	public void ReqChangeGuildAnnouncement(string announcement)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqChangeAnnouncement(announcement, OnChangeGuildAnnouncementRes);
	}

	public void ReqChangeGuildIntroduction(string introduction)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqChangeIntroduction(introduction, OnChangeGuildIntroductionRes);
	}

	public void ReqGetGuildLog()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqGetLog(OnGetGuildLogRes);
	}

	public void ReqDonate(int amount)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqDonate(amount, OnDonateRes);
	}

	public void ReqEddieDonate(int amount)
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqEddieDonate(amount, OnEddieDonateRes);
	}

	public void ReqGetEddieBoxGachaRecord()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqRetrieveEddieBoxGachaRecord(OnGetEddieBoxGachaRecordRes);
	}

	public void ReqReceiveEddieReward()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqReceiveEddieReward(OnGetEddieRewardRes);
	}

	public void ReqCheckReplaceLeader()
	{
		if (DisableCheckLeaderReplace)
		{
			Debug.LogWarning(string.Format("{0} is {1} and skip Check", "DisableCheckLeaderReplace", DisableCheckLeaderReplace));
		}
		else
		{
			ManagedSingleton<PlayerNetManager>.Instance.GuildReqCheckReplaceLeader(OnGuildCheckReplaceLeaderRes);
		}
	}

	public void ReqGetGuildRankRead()
	{
		ManagedSingleton<PlayerNetManager>.Instance.GuildReqGetGuildRankRead(OnGetGuildRankReadRes);
	}

	public void ReqSetGuildRankRead()
	{
		if (_guildInfoCache != null && _guildInfoCache.Rank > GuildRankRead)
		{
			ManagedSingleton<PlayerNetManager>.Instance.GuildReqSetGuildRankRead(_guildInfoCache.Rank, OnSetGuildRankReadRes);
		}
	}

	private void OnGetCheckGuildStateRes(GuildCheckGuildStateRes resp)
	{
		Debug.Log("[OnGetCheckGuildStateRes]");
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CHECK_GUILD_STATE_SUCCESS)
		{
			GuildId = resp.GuildId;
			HasInviteGuild = resp.HasInviteGuild > 0;
			HasEddieReward = resp.HasEddieReward > 0;
			Action action = delegate
			{
				SendSocketSetGuildIdReq(GuildId);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			};
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLogin)
			{
				if (action != null)
				{
					action();
				}
			}
			else
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnConnectedEvent += action;
			}
			Action<Code> onGetCheckGuildStateEvent = this.OnGetCheckGuildStateEvent;
			if (onGetCheckGuildStateEvent != null)
			{
				onGetCheckGuildStateEvent(code);
			}
			Action onGetCheckGuildStateOnceEvent = this.OnGetCheckGuildStateOnceEvent;
			if (onGetCheckGuildStateOnceEvent != null)
			{
				onGetCheckGuildStateOnceEvent();
			}
			this.OnGetCheckGuildStateOnceEvent = null;
		}
		else
		{
			HandleErrorCode("OnGetCheckGuildStateRes", code);
		}
	}

	private void OnGetGuildInfoRes(GuildGetGuildInfoRes resp)
	{
		Debug.Log("[OnGetGuildInfoRes]");
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_GET_INFO_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			BuildingInfoListCache = resp.BuildingInfo;
			MemberInfoListCache = resp.MemberInfo;
			InvitePlayerListCache = resp.InviteInfoList;
			ApplyPlayerListCache = resp.ApplyInfoList;
			ManagedSingleton<PlayerNetManager>.Instance.SetMissionProgress(resp);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			if (GuildInfoCache != null)
			{
				RefreshGuildSetting();
				SendSocketSetGuildIdReq(GuildId);
				if (GuildRankRead <= 0)
				{
					Debug.LogWarning("第二階段既有公會玩家防呆，強制設值");
					GuildRankRead = GuildInfoCache.Rank;
					ReqSetGuildRankRead();
				}
				OnGetGuildInfoNotNull();
			}
			else
			{
				SendSocketSetGuildIdReq(0);
				OnGetGuildInfoNull();
			}
		}
		else
		{
			HandleErrorCode("ReqGetGuildInfo", code);
		}
	}

	private void OnGetMemberInfoListRes(GuildGetGuildMemberListRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_GET_GUILD_MEMBER_LIST_SUCCESS)
		{
			MemberInfoListCache = resp.MemberInfo;
			IEnumerable<string> targetIds = MemberInfoListCache.Select((NetMemberInfo memberInfo) => memberInfo.MemberId);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onGetMemberInfoListEvent2 = this.OnGetMemberInfoListEvent;
				if (onGetMemberInfoListEvent2 != null)
				{
					onGetMemberInfoListEvent2(ackCode);
				}
			});
		}
		else
		{
			HandleErrorCode("OnGetMemberInfoListRes", ackCode);
			Action<Code> onGetMemberInfoListEvent = this.OnGetMemberInfoListEvent;
			if (onGetMemberInfoListEvent != null)
			{
				onGetMemberInfoListEvent(ackCode);
			}
		}
	}

	private void OnGetApplyPlayerListRes(GuildGetApplyPlayerListRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_GET_JOIN_APPLY_LIST_SUCCESS)
		{
			ApplyPlayerListCache = resp.ApplyInfoList;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			IEnumerable<string> targetIds = resp.ApplyInfoList.Select((NetPlayerJoinMessageInfo info) => info.PlayerID);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onGetApplyPlayerListEvent2 = this.OnGetApplyPlayerListEvent;
				if (onGetApplyPlayerListEvent2 != null)
				{
					onGetApplyPlayerListEvent2(ackCode);
				}
			});
		}
		else
		{
			HandleErrorCode("OnGetApplyPlayerListRes", ackCode);
			Action<Code> onGetApplyPlayerListEvent = this.OnGetApplyPlayerListEvent;
			if (onGetApplyPlayerListEvent != null)
			{
				onGetApplyPlayerListEvent(ackCode);
			}
		}
	}

	private void OnAgreePlayerApplyRes(GuildAgreePlayerJoinRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_AGREE_JOIN_APPLY_SUCCESS)
		{
			MemberInfoListCache = resp.MemberInfo;
			ApplyPlayerListCache = resp.ApplyInfoList;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			List<NetMemberInfo> list = MemberInfoListCache.Where((NetMemberInfo memberInfo) => resp.PlayerIDList.Contains(memberInfo.MemberId)).ToList();
			if (list != null)
			{
				SendSocketSetGuildMemberInfoReq(GuildId, list);
				SendSocketSetGuildPlayerIdReq(GuildId, resp.PlayerIDList, true);
			}
			SendSocketMemberJoindEvent(GuildId, resp.PlayerIDList);
			IEnumerable<string> targetIds = MemberInfoListCache.Select((NetMemberInfo memberInfo) => memberInfo.MemberId);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onAgreePlayerApplyEvent2 = this.OnAgreePlayerApplyEvent;
				if (onAgreePlayerApplyEvent2 != null)
				{
					onAgreePlayerApplyEvent2(ackCode);
				}
			});
		}
		else
		{
			HandleErrorCode("OnAgreePlayerApplyRes", ackCode);
			Action<Code> onAgreePlayerApplyEvent = this.OnAgreePlayerApplyEvent;
			if (onAgreePlayerApplyEvent != null)
			{
				onAgreePlayerApplyEvent(ackCode);
			}
		}
	}

	private void OnRefusePlayerApplyRes(GuildRefusePlayerJoinRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_REFUSE_JOIN_APPLY_SUCCESS)
		{
			ApplyPlayerListCache = resp.ApplyInfoList;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			Action<Code> onRefusePlayerApplyEvent = this.OnRefusePlayerApplyEvent;
			if (onRefusePlayerApplyEvent != null)
			{
				onRefusePlayerApplyEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnRefusePlayerApplyRes", code);
			Action<Code> onRefusePlayerApplyEvent2 = this.OnRefusePlayerApplyEvent;
			if (onRefusePlayerApplyEvent2 != null)
			{
				onRefusePlayerApplyEvent2(code);
			}
		}
	}

	private void OnGetInvitePlayerListRes(GuildGetInvitePlayerListRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_GET_INVITE_LIST_SUCCESS)
		{
			InvitePlayerListCache = resp.InviteInfoList;
			IEnumerable<string> targetIds = resp.InviteInfoList.Select((NetPlayerJoinMessageInfo info) => info.PlayerID);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onGetInvitePlayerListEvent2 = this.OnGetInvitePlayerListEvent;
				if (onGetInvitePlayerListEvent2 != null)
				{
					onGetInvitePlayerListEvent2(ackCode);
				}
			});
		}
		else
		{
			HandleErrorCode("OnGetInvitePlayerListRes", ackCode);
			Action<Code> onGetInvitePlayerListEvent = this.OnGetInvitePlayerListEvent;
			if (onGetInvitePlayerListEvent != null)
			{
				onGetInvitePlayerListEvent(ackCode);
			}
		}
	}

	private void OnEditBadgeRes(GuildEditBadgeRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_EDIT_BADGE_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			SendSocketSetGuildInfoReq(resp.GuildInfo);
			Action<Code, NetGuildInfo> onEditBadgeEvent = this.OnEditBadgeEvent;
			if (onEditBadgeEvent != null)
			{
				onEditBadgeEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnEditBadgeRes", code);
			Action<Code, NetGuildInfo> onEditBadgeEvent2 = this.OnEditBadgeEvent;
			if (onEditBadgeEvent2 != null)
			{
				onEditBadgeEvent2(code, resp.GuildInfo);
			}
		}
	}

	private void OnCreateGuildRes(GuildCreateGuildRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CREATE_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			BuildingInfoListCache = resp.BuildingInfo;
			MemberInfoListCache = resp.MemberInfo;
			RefreshGuildSetting();
			SendSocketSetGuildIdReq(GuildId, true);
			SendSocketSetGuildInfoReq(GuildInfoCache);
			SendSocketSetGuildMemberInfoReq(GuildId, SelfMemberInfo);
			SendSocketSetGuildPlayerIdReq(GuildId, true);
			Action<Code> onCreateGuildEvent = this.OnCreateGuildEvent;
			if (onCreateGuildEvent != null)
			{
				onCreateGuildEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnCreateGuildRes", code);
			Action<Code> onCreateGuildEvent2 = this.OnCreateGuildEvent;
			if (onCreateGuildEvent2 != null)
			{
				onCreateGuildEvent2(code);
			}
		}
	}

	private void OnRankupGuildRes(GuildRankUpRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_RANK_UP_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			RefreshGuildSetting();
			SendSocketSetGuildInfoReq(resp.GuildInfo);
			SendSocketGuildRankupEvent();
			Action<Code, NetGuildInfo> onRankupGuildEvent = this.OnRankupGuildEvent;
			if (onRankupGuildEvent != null)
			{
				onRankupGuildEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnRankupGuildRes", code);
			Action<Code, NetGuildInfo> onRankupGuildEvent2 = this.OnRankupGuildEvent;
			if (onRankupGuildEvent2 != null)
			{
				onRankupGuildEvent2(code, resp.GuildInfo);
			}
		}
	}

	private void OnJoinGuildRes(GuildJoinGuildRes resp)
	{
		Code code = (Code)resp.Code;
		switch (code)
		{
		case Code.GUILD_JOIN_FREE_ADD_SUCCESS:
		{
			if (resp.GuildInfo != null)
			{
				SendSocketSetGuildIdReq(resp.GuildInfo.GuildID, true);
			}
			GuildInfoCache = resp.GuildInfo;
			BuildingInfoListCache = resp.BuildingInfo;
			MemberInfoListCache = resp.MemberInfo;
			RefreshGuildSetting();
			if (HasGuild)
			{
				SendSocketSetGuildMemberInfoReq(GuildId, SelfMemberInfo);
				SendSocketSetGuildPlayerIdReq(GuildId, true);
			}
			Action<Code> onJoinGuildEvent2 = this.OnJoinGuildEvent;
			if (onJoinGuildEvent2 != null)
			{
				onJoinGuildEvent2(code);
			}
			break;
		}
		case Code.GUILD_JOIN_APPLY_SUCCESS:
		{
			GuildInfoCache = resp.GuildInfo;
			BuildingInfoListCache = resp.BuildingInfo;
			MemberInfoListCache = resp.MemberInfo;
			ApplyGuildListCache = resp.ApplyGuildInfoList;
			Action<Code> onJoinGuildEvent3 = this.OnJoinGuildEvent;
			if (onJoinGuildEvent3 != null)
			{
				onJoinGuildEvent3(code);
			}
			break;
		}
		default:
		{
			HandleErrorCode("OnJoinGuildRes", code);
			Action<Code> onJoinGuildEvent = this.OnJoinGuildEvent;
			if (onJoinGuildEvent != null)
			{
				onJoinGuildEvent(code);
			}
			break;
		}
		}
	}

	private void OnCancelJoinGuildRes(GuildCancelJoinGuildRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_REFUSE_JOIN_APPLY_SUCCESS)
		{
			ApplyGuildListCache = resp.ApplyGuildInfoList;
			Action<Code> onCancelJoinGuildEvent = this.OnCancelJoinGuildEvent;
			if (onCancelJoinGuildEvent != null)
			{
				onCancelJoinGuildEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnCancelJoinGuildRes", code);
			Action<Code> onCancelJoinGuildEvent2 = this.OnCancelJoinGuildEvent;
			if (onCancelJoinGuildEvent2 != null)
			{
				onCancelJoinGuildEvent2(code);
			}
		}
	}

	private void OnLeaveGuildRes(GuildLeaveGuildRes resp)
	{
		Code code = (Code)resp.Code;
		if ((uint)(code - 105300) <= 2u)
		{
			SocketGuildMemberInfo data = new SocketGuildMemberInfo
			{
				PlayerId = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID,
				GuildId = 0,
				GuildPrivilege = -1
			};
			SendSocketSetGuildMemberInfoReq(data);
			SendSocketSetGuildPlayerIdReq(GuildId, false);
			if (code != Code.GUILD_LEAVE_NO_MEMBER_REMOVE_SUCCESS && code == Code.GUILD_LEAVE_CHANGE_LEADER_SUCCESS)
			{
				if (!string.IsNullOrEmpty(resp.NewLeaderID))
				{
					SocketGuildMemberInfo socketGuildMemberInfo = new SocketGuildMemberInfo
					{
						PlayerId = resp.NewLeaderID,
						GuildId = GuildId,
						GuildPrivilege = 1
					};
					SendSocketSetGuildMemberInfoReq(socketGuildMemberInfo);
					SendSocketMemberPrivilegeChangedEvent(socketGuildMemberInfo);
				}
				else
				{
					Debug.LogError("New LeaderID is null!!!!");
				}
			}
			ClearCacheData();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			SendSocketSetGuildIdReq(0, true);
			Action<Code> onLeaveGuildEvent = this.OnLeaveGuildEvent;
			if (onLeaveGuildEvent != null)
			{
				onLeaveGuildEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnLeaveGuildRes", code);
			Action<Code> onLeaveGuildEvent2 = this.OnLeaveGuildEvent;
			if (onLeaveGuildEvent2 != null)
			{
				onLeaveGuildEvent2(code);
			}
		}
	}

	private void OnRemoveGuildRes(GuildRemoveGuildRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_REMOVE_SUCCESS)
		{
			GuildEventGuildRemoved data = new GuildEventGuildRemoved();
			SendSocketGuildEvent(data);
			SocketGuildMemberInfo socketGuildMemberInfo = new SocketGuildMemberInfo
			{
				GuildId = 0,
				GuildPrivilege = -1
			};
			foreach (string playerID in resp.PlayerIDList)
			{
				socketGuildMemberInfo.PlayerId = playerID;
				SendSocketSetGuildMemberInfoReq(socketGuildMemberInfo);
			}
			SendSocketSetGuildPlayerIdReq(GuildId, resp.PlayerIDList, false);
			ClearCacheData();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			SendSocketSetGuildIdReq(0, true);
			Action<Code> onRemoveGuildEvent = this.OnRemoveGuildEvent;
			if (onRemoveGuildEvent != null)
			{
				onRemoveGuildEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnRemoveGuildRes", code);
			Action<Code> onRemoveGuildEvent2 = this.OnRemoveGuildEvent;
			if (onRemoveGuildEvent2 != null)
			{
				onRemoveGuildEvent2(code);
			}
		}
	}

	private void OnGetApplyGuildListRes(GuildGetApplyGuildListRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_GET_JOIN_APPLY_LIST_BY_PLAYER_SUCCESS)
		{
			ApplyGuildListCache = resp.ApplyGuildInfoList;
			IEnumerable<string> targetIds = ApplyGuildListCache.Select((NetGuildJoinMessageInfo info) => info.GuildInfo.LeaderPlayerID);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onGetApplyGuildListEvent = this.OnGetApplyGuildListEvent;
				if (onGetApplyGuildListEvent != null)
				{
					onGetApplyGuildListEvent(ackCode);
				}
			});
		}
		else
		{
			HandleErrorCode("OnGetApplyGuildListRes", ackCode);
		}
	}

	private void OnGetInviteGuildListRes(GuildGetInviteGuildListRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_GET_INVITE_LIST_BY_PLAYER_SUCCESS)
		{
			InviteGuildListCache = resp.InviteGuildInfoList;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			IEnumerable<string> targetIds = InviteGuildListCache.Select((NetGuildJoinMessageInfo info) => info.GuildInfo.LeaderPlayerID);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onGetInviteGuildListEvent = this.OnGetInviteGuildListEvent;
				if (onGetInviteGuildListEvent != null)
				{
					onGetInviteGuildListEvent(ackCode);
				}
			});
		}
		else
		{
			HandleErrorCode("OnGetInviteGuildListRes", ackCode);
		}
	}

	private void OnAgreeGuildInviteRes(GuildAgreeGuildInviteRes resp)
	{
		Code ackCode = (Code)resp.Code;
		switch (ackCode)
		{
		case Code.GUILD_AGREE_INVITE_SUCCESS:
		{
			GuildInfoCache = resp.GuildInfo;
			BuildingInfoListCache = resp.BuildingInfo;
			MemberInfoListCache = resp.MemberInfo;
			ApplyPlayerListCache = resp.ApplyInfoList;
			InvitePlayerListCache = resp.InviteInfoList;
			RefreshGuildSetting();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			SendSocketSetGuildIdReq(GuildId, true);
			SendSocketSetGuildMemberInfoReq(GuildId, SelfMemberInfo);
			SendSocketSetGuildPlayerIdReq(GuildId, true);
			Action<Code> onAgreeGuildInviteEvent2 = this.OnAgreeGuildInviteEvent;
			if (onAgreeGuildInviteEvent2 != null)
			{
				onAgreeGuildInviteEvent2(ackCode);
			}
			break;
		}
		case Code.GUILD_INVITE_NOT_FOUND_DATA:
		{
			InviteGuildListCache = resp.InviteGuildInfoList;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			IEnumerable<string> targetIds = InviteGuildListCache.Select((NetGuildJoinMessageInfo info) => info.GuildInfo.LeaderPlayerID);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onAgreeGuildInviteEvent3 = this.OnAgreeGuildInviteEvent;
				if (onAgreeGuildInviteEvent3 != null)
				{
					onAgreeGuildInviteEvent3(ackCode);
				}
			});
			break;
		}
		default:
		{
			HandleErrorCode("OnAgreeGuildInviteRes", ackCode);
			Action<Code> onAgreeGuildInviteEvent = this.OnAgreeGuildInviteEvent;
			if (onAgreeGuildInviteEvent != null)
			{
				onAgreeGuildInviteEvent(ackCode);
			}
			break;
		}
		}
	}

	private void OnRefuseGuildInviteRes(GuildRefuseGuildInviteRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_REFUSE_INVITE_SUCCESS || code == Code.GUILD_INVITE_NOT_FOUND_DATA)
		{
			InviteGuildListCache = resp.InviteGuildInfoList;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			IEnumerable<string> targetIds = InviteGuildListCache.Select((NetGuildJoinMessageInfo info) => info.GuildInfo.LeaderPlayerID);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onRefuseGuildInviteEvent2 = this.OnRefuseGuildInviteEvent;
				if (onRefuseGuildInviteEvent2 != null)
				{
					onRefuseGuildInviteEvent2(ackCode);
				}
			});
		}
		else
		{
			HandleErrorCode("OnRefuseGuildInviteRes", ackCode);
			Action<Code> onRefuseGuildInviteEvent = this.OnRefuseGuildInviteEvent;
			if (onRefuseGuildInviteEvent != null)
			{
				onRefuseGuildInviteEvent(ackCode);
			}
		}
	}

	private void OnInvitePlayerRes(GuildInvitePlayerRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_INVITE_SUCCESS)
		{
			InvitePlayerListCache = resp.InviteInfoList;
			Action<Code> onInvitePlayerEvent = this.OnInvitePlayerEvent;
			if (onInvitePlayerEvent != null)
			{
				onInvitePlayerEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnInvitePlayerRes", code);
			Action<Code> onInvitePlayerEvent2 = this.OnInvitePlayerEvent;
			if (onInvitePlayerEvent2 != null)
			{
				onInvitePlayerEvent2(code);
			}
		}
	}

	private void OnCancelInvitePlayerRes(GuildCancelInvitePlayerRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_REFUSE_INVITE_SUCCESS)
		{
			InvitePlayerListCache = resp.InviteInfoList;
			Action<Code> onCancelInvitePlayerEvent = this.OnCancelInvitePlayerEvent;
			if (onCancelInvitePlayerEvent != null)
			{
				onCancelInvitePlayerEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnCancelInvitePlayerRes", code);
			Action<Code> onCancelInvitePlayerEvent2 = this.OnCancelInvitePlayerEvent;
			if (onCancelInvitePlayerEvent2 != null)
			{
				onCancelInvitePlayerEvent2(code);
			}
		}
	}

	private void OnSearchGuildRes(GuildSearchGuildRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_SEARCH_SUCCESS)
		{
			SearchGuildListCount = resp.TotalCount;
			SearchGuildListCache.AddRange(resp.GuildInfoList);
			IEnumerable<string> targetIds = resp.GuildInfoList.Select((NetGuildInfo guildInfo) => guildInfo.LeaderPlayerID);
			RefreshBusyStatusAndSearchHUD(targetIds, delegate
			{
				Action<Code> onSearchGuildEvent = this.OnSearchGuildEvent;
				if (onSearchGuildEvent != null)
				{
					onSearchGuildEvent(ackCode);
				}
			});
		}
		else
		{
			HandleErrorCode("OnSearchGuildRes", ackCode);
		}
	}

	private void OnChangeGuildNameRes(GuildChangeNameRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CHANGE_NAME_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			SendSocketSetGuildInfoReq(resp.GuildInfo);
			Action<Code, NetGuildInfo> onChangeGuildNameEvent = this.OnChangeGuildNameEvent;
			if (onChangeGuildNameEvent != null)
			{
				onChangeGuildNameEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnChangeGuildNameRes", code);
			Action<Code, NetGuildInfo> onChangeGuildNameEvent2 = this.OnChangeGuildNameEvent;
			if (onChangeGuildNameEvent2 != null)
			{
				onChangeGuildNameEvent2(code, null);
			}
		}
	}

	private void OnChangeMemberPrivilegeRes(GuildChangePrivilegeRes resp)
	{
		Code code = (Code)resp.Code;
		switch (code)
		{
		case Code.GUILD_CHANGE_PRIVILEGE_SUCCESS:
		{
			GuildInfoCache = resp.GuildInfo;
			ReplaceMemberInfo(resp.ChangedMemberInfo);
			SendSocketSetGuildMemberInfoReq(GuildId, resp.ChangedMemberInfo);
			SendSocketMemberPrivilegeChangedEvent(resp.ChangedMemberInfo);
			Action<Code> onChangeMemberPrivilegeEvent2 = this.OnChangeMemberPrivilegeEvent;
			if (onChangeMemberPrivilegeEvent2 != null)
			{
				onChangeMemberPrivilegeEvent2(code);
			}
			break;
		}
		case Code.GUILD_CHANGE_PRIVILEGE_FAIL:
		case Code.GUILD_PRIVILEGE_TYPE_FAIL:
		{
			Action<Code> onChangeMemberPrivilegeEvent = this.OnChangeMemberPrivilegeEvent;
			if (onChangeMemberPrivilegeEvent != null)
			{
				onChangeMemberPrivilegeEvent(code);
			}
			break;
		}
		default:
			HandleErrorCode("OnChangeMemberPrivilegeRes", code);
			break;
		}
	}

	private void OnKickMemberRes(GuildKickMemberRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_KICK_MEMBER_SUCCESS)
		{
			MemberInfoListCache = resp.MemberInfo;
			SocketGuildMemberInfo data = new SocketGuildMemberInfo
			{
				PlayerId = resp.PlayerID,
				GuildId = 0,
				GuildPrivilege = -1
			};
			SendSocketSetGuildMemberInfoReq(data);
			SendSocketSetGuildPlayerIdReq(GuildId, resp.PlayerID, false);
			Action<Code, string> onKickMemberEvent = this.OnKickMemberEvent;
			if (onKickMemberEvent != null)
			{
				onKickMemberEvent(code, resp.PlayerID);
			}
			SendSocketMemberKickedEvent(resp.PlayerID);
		}
		else
		{
			HandleErrorCode("OnKickMemberRes", code);
		}
	}

	private void OnChangeGuildPowerDemandRes(GuildChangePowerDemandRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CHANGE_POWER_DEMAND_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			Action<Code, NetGuildInfo> onChangeGuildPowerDemandEvent = this.OnChangeGuildPowerDemandEvent;
			if (onChangeGuildPowerDemandEvent != null)
			{
				onChangeGuildPowerDemandEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnChangeGuildPowerDemandRes", code);
			Action<Code, NetGuildInfo> onChangeGuildPowerDemandEvent2 = this.OnChangeGuildPowerDemandEvent;
			if (onChangeGuildPowerDemandEvent2 != null)
			{
				onChangeGuildPowerDemandEvent2(code, null);
			}
		}
	}

	private void OnChangeGuildApplyTypeRes(GuildChangeApplyTypeRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CHANGE_APPLY_TYPE_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			Action<Code, NetGuildInfo> onChangeGuildApplyTypeEvent = this.OnChangeGuildApplyTypeEvent;
			if (onChangeGuildApplyTypeEvent != null)
			{
				onChangeGuildApplyTypeEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnChangeGuildApplyTypeRes", code);
			Action<Code, NetGuildInfo> onChangeGuildApplyTypeEvent2 = this.OnChangeGuildApplyTypeEvent;
			if (onChangeGuildApplyTypeEvent2 != null)
			{
				onChangeGuildApplyTypeEvent2(code, resp.GuildInfo);
			}
		}
	}

	private void OnChangeHeaderPowerRes(GuildChangeHeaderPowerRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CHANGE_HEADER_POWER_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			GuildEventHeaderPowerChanged data = new GuildEventHeaderPowerChanged
			{
				NewHeaderPower = GuildInfoCache.HeaderPower
			};
			SendSocketGuildEvent(data);
			Action<Code, NetGuildInfo> onChangeGuildHeaderPowerEvent = this.OnChangeGuildHeaderPowerEvent;
			if (onChangeGuildHeaderPowerEvent != null)
			{
				onChangeGuildHeaderPowerEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnChangeHeaderPowerRes", code);
			Action<Code, NetGuildInfo> onChangeGuildHeaderPowerEvent2 = this.OnChangeGuildHeaderPowerEvent;
			if (onChangeGuildHeaderPowerEvent2 != null)
			{
				onChangeGuildHeaderPowerEvent2(code, null);
			}
		}
	}

	private void OnChangeGuildAnnouncementRes(GuildChangeBoardRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CHANGE_BOARD_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			Action<Code, NetGuildInfo> onChangeGuildAnnouncementEvent = this.OnChangeGuildAnnouncementEvent;
			if (onChangeGuildAnnouncementEvent != null)
			{
				onChangeGuildAnnouncementEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnChangeGuildAnnouncementRes", code);
			Action<Code, NetGuildInfo> onChangeGuildAnnouncementEvent2 = this.OnChangeGuildAnnouncementEvent;
			if (onChangeGuildAnnouncementEvent2 != null)
			{
				onChangeGuildAnnouncementEvent2(code, resp.GuildInfo);
			}
		}
	}

	private void OnChangeGuildIntroductionRes(GuildChangeIntroductionRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CHANGE_INTRODUCTION_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			Action<Code, NetGuildInfo> onChangeGuildIntroductionEvent = this.OnChangeGuildIntroductionEvent;
			if (onChangeGuildIntroductionEvent != null)
			{
				onChangeGuildIntroductionEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnChangeGuildIntroductionRes", code);
			Action<Code, NetGuildInfo> onChangeGuildIntroductionEvent2 = this.OnChangeGuildIntroductionEvent;
			if (onChangeGuildIntroductionEvent2 != null)
			{
				onChangeGuildIntroductionEvent2(code, resp.GuildInfo);
			}
		}
	}

	private void OnGetGuildLogRes(GuildGetLogRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_GET_LOG_SUCCESS)
		{
			LogListCache = (from log in resp.GuildLog
				orderby log.LogTime descending, log.LogType descending
				select log).ToList();
			IEnumerable<string> targetIds = LogListCache.Select((NetGuildLog info) => info.PlayerID);
			SearchHUD(targetIds, delegate
			{
				Action<Code, List<NetGuildLog>> onGetGuildLogEvent2 = this.OnGetGuildLogEvent;
				if (onGetGuildLogEvent2 != null)
				{
					onGetGuildLogEvent2(ackCode, resp.GuildLog);
				}
			});
		}
		else
		{
			HandleErrorCode("OnGetGuildLogRes", ackCode);
			Action<Code, List<NetGuildLog>> onGetGuildLogEvent = this.OnGetGuildLogEvent;
			if (onGetGuildLogEvent != null)
			{
				onGetGuildLogEvent(ackCode, null);
			}
		}
	}

	private void OnDonateRes(GuildDonateRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_DONATE_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			Action<Code, NetGuildInfo> onDonateEvent = this.OnDonateEvent;
			if (onDonateEvent != null)
			{
				onDonateEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnDonateRes", code);
			Action<Code, NetGuildInfo> onDonateEvent2 = this.OnDonateEvent;
			if (onDonateEvent2 != null)
			{
				onDonateEvent2(code, null);
			}
		}
	}

	private void OnEddieDonateRes(GuildEddieDonateRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_EDDIE_DONATE_SUCCESS)
		{
			GuildInfoCache = resp.GuildInfo;
			Action<Code, NetGuildInfo> onEddieDonateEvent = this.OnEddieDonateEvent;
			if (onEddieDonateEvent != null)
			{
				onEddieDonateEvent(code, resp.GuildInfo);
			}
		}
		else
		{
			HandleErrorCode("OnEddieDonateRes", code);
			Action<Code, NetGuildInfo> onEddieDonateEvent2 = this.OnEddieDonateEvent;
			if (onEddieDonateEvent2 != null)
			{
				onEddieDonateEvent2(code, null);
			}
		}
	}

	private void OnGetEddieBoxGachaRecordRes(GuildRetrieveEddieBoxGachaRecordRes resp)
	{
		Code ackCode = (Code)resp.Code;
		Code code = ackCode;
		if (code == Code.GUILD_GET_EDDIE_BOX_GACHA_RECORD_SUCCESS)
		{
			Action action = delegate
			{
				Action<Code, int, int, List<NetEddieBoxGachaRecord>, string> onGetEddieBoxGachaRecordEvent2 = this.OnGetEddieBoxGachaRecordEvent;
				if (onGetEddieBoxGachaRecordEvent2 != null)
				{
					onGetEddieBoxGachaRecordEvent2(ackCode, resp.GuildRankRecord, resp.EddieMoneyRecord, resp.EddieBoxGachaRecordList, resp.MVPPlayerID);
				}
			};
			if (!string.IsNullOrEmpty(resp.MVPPlayerID))
			{
				SearchHUD(resp.MVPPlayerID, action);
			}
			else
			{
				action();
			}
		}
		else
		{
			HandleErrorCode("OnGetEddieBoxGachaRecordRes", ackCode);
			Action<Code, int, int, List<NetEddieBoxGachaRecord>, string> onGetEddieBoxGachaRecordEvent = this.OnGetEddieBoxGachaRecordEvent;
			if (onGetEddieBoxGachaRecordEvent != null)
			{
				onGetEddieBoxGachaRecordEvent(ackCode, 0, 0, null, string.Empty);
			}
		}
	}

	private void OnGetEddieRewardRes(GuildReceiveEddieRewardRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_RECEIVE_EDDIE_REWARD_SUCCESS)
		{
			HasEddieReward = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, false);
			Action<Code, List<NetRewardInfo>> onGetEddieRewardEvent = this.OnGetEddieRewardEvent;
			if (onGetEddieRewardEvent != null)
			{
				onGetEddieRewardEvent(code, resp.RewardEntities.RewardList);
			}
		}
		else
		{
			HandleErrorCode("OnGetEddieRewardRes", code);
			Action<Code, List<NetRewardInfo>> onGetEddieRewardEvent2 = this.OnGetEddieRewardEvent;
			if (onGetEddieRewardEvent2 != null)
			{
				onGetEddieRewardEvent2(code, null);
			}
		}
	}

	private void OnGuildCheckReplaceLeaderRes(GuildCheckReplaceLeaderRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_CHECK_REPLACE_LEADER_SUCCESS)
		{
			string oldLeaderID = resp.OldLeaderID;
			string newLeaderID = resp.NewLeaderID;
			if (!string.IsNullOrEmpty(oldLeaderID) && !string.IsNullOrEmpty(newLeaderID) && oldLeaderID != newLeaderID)
			{
				SocketGuildMemberInfo socketGuildMemberInfo = new SocketGuildMemberInfo
				{
					PlayerId = oldLeaderID,
					GuildId = GuildId,
					GuildPrivilege = 1024
				};
				SendSocketSetGuildMemberInfoReq(socketGuildMemberInfo);
				SocketGuildMemberInfo socketGuildMemberInfo2 = new SocketGuildMemberInfo
				{
					PlayerId = newLeaderID,
					GuildId = GuildId,
					GuildPrivilege = 1
				};
				SendSocketSetGuildMemberInfoReq(socketGuildMemberInfo2);
				SendSocketMemberPrivilegeChangedEvent(new SocketGuildMemberInfo[2] { socketGuildMemberInfo, socketGuildMemberInfo2 });
			}
		}
	}

	private void OnGetGuildRankReadRes(GuildGetReadGuildRankRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_GET_READ_GUILD_RANK_SUCCESS)
		{
			GuildRankRead = resp.ReadGuildRank;
			Debug.LogWarning(string.Format("Get GuildRankRead : {0}", GuildRankRead));
		}
	}

	private void OnSetGuildRankReadRes(GuildSetReadGuildRankRes resp)
	{
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_SET_READ_GUILD_RANK_SUCCESS)
		{
			GuildRankRead = resp.ReadGuildRank;
			Debug.LogWarning(string.Format("Set GuildRankRead : {0}", GuildRankRead));
		}
	}

	public void RefreshCommunityPlayerGuildInfoCache(List<string> playerIds, Action callback, bool isChargeMode = false)
	{
		if (callback == null)
		{
			Debug.LogError("[RefreshCommunityPlayerGuildInfoCache] Try Refresh with null callback");
			return;
		}
		if (playerIds == null || playerIds.Count == 0)
		{
			Debug.Log("[RefreshCommunityPlayerGuildInfoCache] No target");
			if (callback != null)
			{
				callback();
			}
			return;
		}
		_onSocketRefreshCommunitySocketInfoCallbackQueue.Enqueue(callback);
		SendSocketGetGuildMemberInfoReq(playerIds, delegate(List<SocketGuildMemberInfo> memberInfos)
		{
			if (isChargeMode)
			{
				foreach (SocketGuildMemberInfo memberInfo in memberInfos)
				{
					MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SocketGuildMemberInfoCache[memberInfo.PlayerId] = memberInfo;
				}
			}
			else
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RefreshSocketGuildMemberInfoCache(memberInfos);
			}
			OnGetSocketGuildMemberInfo(memberInfos, isChargeMode);
		});
	}

	private void OnGetSocketGuildMemberInfo(List<SocketGuildMemberInfo> memberInfos, bool isChargeMode = false)
	{
		List<int> list = (from memberInfo in memberInfos
			select memberInfo.GuildId into guildId
			where guildId > 0
			select guildId).Distinct().ToList();
		if (isChargeMode)
		{
			list = list.Except(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SocketGuildInfoCache.Keys).ToList();
		}
		SendSocketGetGuildInfoReq(list, delegate(List<SocketGuildInfo> guildInfos)
		{
			if (isChargeMode)
			{
				foreach (SocketGuildInfo guildInfo in guildInfos)
				{
					MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SocketGuildInfoCache[guildInfo.GuildId] = guildInfo;
				}
			}
			else
			{
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RefreshSocketGuildInfoCache(guildInfos);
			}
			OnGetSocketGuildInfo(guildInfos);
		});
	}

	private void OnGetSocketGuildInfo(List<SocketGuildInfo> guildInfos)
	{
		Action result;
		if (_onSocketRefreshCommunitySocketInfoCallbackQueue.TryDequeue(out result))
		{
			if (result != null)
			{
				result();
			}
		}
		else
		{
			Debug.LogError("Dequeue callback Error");
		}
	}

	public void SendSocketSetGuildIdReq(int guildId, bool guildStateChanged = false)
	{
		_guildStateChanged = guildStateChanged;
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetGuildId(guildId));
	}

	public void SendSocketSetGuildInfoReq(NetGuildInfo guildInfo)
	{
		if (guildInfo == null)
		{
			Debug.LogError("[SendSocketSetGuildInfoReq] Try Update null GuildInfo");
			return;
		}
		SocketGuildInfo data = new SocketGuildInfo
		{
			GuildId = guildInfo.GuildID,
			GuildName = guildInfo.GuildName,
			GuildRank = guildInfo.Rank,
			GuildBadge = guildInfo.Badge
		};
		SendSocketSetGuildInfoReq(data);
	}

	private void SendSocketSetGuildInfoReq(SocketGuildInfo data)
	{
		string guildInfoJSON = JsonHelper.Serialize(data);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetGuildInfo(guildInfoJSON, string.Empty, 0));
	}

	public void SendSocketGetGuildInfoReq(List<int> guildIds, Action<List<SocketGuildInfo>> callback)
	{
		if (callback == null)
		{
			Debug.LogError("[SendSocketGetGuildInfoReq] Try get SocketGuildInfo with null callback");
		}
		else if (guildIds == null || guildIds.Count == 0)
		{
			Debug.Log("[SendSocketGetGuildInfoReq] No target");
			if (callback != null)
			{
				callback(new List<SocketGuildInfo>());
			}
		}
		else
		{
			_onSocketGetGuildInfoResCallbackQueue.Enqueue(callback);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetGuildInfo(guildIds.ToArray()));
		}
	}

	public void SendSocketSetGuildMemberInfoReq(int guildId, NetMemberInfo memberInfo)
	{
		if (memberInfo == null)
		{
			Debug.LogError("[SendSocketSetGuildMemberInfoReq] Try Update null MemberInfo");
			return;
		}
		SendSocketSetGuildMemberInfoReq(guildId, new List<NetMemberInfo> { memberInfo });
	}

	public void SendSocketSetGuildMemberInfoReq(int guildId, List<NetMemberInfo> memberInfos)
	{
		if (memberInfos == null)
		{
			Debug.LogError("[SendSocketSetGuildMemberInfoReq] Try Update null MemberInfos");
			return;
		}
		foreach (NetMemberInfo memberInfo in memberInfos)
		{
			SocketGuildMemberInfo data = new SocketGuildMemberInfo
			{
				PlayerId = memberInfo.MemberId,
				GuildId = guildId,
				GuildPrivilege = memberInfo.Privilege
			};
			SendSocketSetGuildMemberInfoReq(data);
		}
	}

	private void SendSocketSetGuildMemberInfoReq(SocketGuildMemberInfo data)
	{
		string memberInfoJSON = JsonHelper.Serialize(data);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetGuildMemberInfo(data.PlayerId, memberInfoJSON));
	}

	public void SendSocketGetGuildMemberInfoReq(List<string> playerIds, Action<List<SocketGuildMemberInfo>> callback)
	{
		if (callback == null)
		{
			Debug.LogError("[SendSocketGetGuildMemberInfoReq] Try get SocketGuildMemberInfo with null callback");
		}
		else if (playerIds == null || playerIds.Count == 0)
		{
			Debug.Log("[SendSocketGetGuildMemberInfoReq] No target");
			if (callback != null)
			{
				callback(new List<SocketGuildMemberInfo>());
			}
		}
		else
		{
			_onSocketGetGuildMemberInfoResCallbackQueue.Enqueue(callback);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetGuildMemberInfo(playerIds.ToArray()));
		}
	}

	public void SendSocketSetGuildPlayerIdReq(int guildId, bool isAdd)
	{
		SendSocketSetGuildPlayerIdReq(guildId, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID, isAdd);
	}

	public void SendSocketSetGuildPlayerIdReq(int guildId, string playerId, bool isAdd)
	{
		SendSocketSetGuildPlayerIdReq(guildId, new List<string> { playerId }, isAdd);
	}

	public void SendSocketSetGuildPlayerIdReq(int guildId, List<string> playerIdList, bool isAdd)
	{
		if (playerIdList == null)
		{
			Debug.LogError("PlayerIdList is null");
			return;
		}
		foreach (string playerId in playerIdList)
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetGuildPlayerId(guildId, playerId, isAdd ? 1 : 0));
		}
	}

	public void SendSocketGetGuildPlayerIdListReq(int guildId, Action<List<string>> callback)
	{
		if (callback == null)
		{
			Debug.LogError("[SendSocketGetGuildPlayerIdListReq] Try get PlayerIdList with null callback");
			return;
		}
		_onSocketGetGuildPlayerIdListResCallbackQueue.Enqueue(callback);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetGuildPlayerIdList(guildId));
	}

	private void SendSocketMemberJoindEvent(int guildId, List<string> playerIds)
	{
		GuildEventMemberJoined data = new GuildEventMemberJoined
		{
			GuildId = guildId
		};
		foreach (string playerId in playerIds)
		{
			SendSocketGuildEvent(data, playerId);
		}
	}

	private void SendSocketMemberPrivilegeChangedEvent(List<NetMemberInfo> changedMemberInfos)
	{
		if (changedMemberInfos != null)
		{
			GuildEventMemberPrivilegeChanged data = new GuildEventMemberPrivilegeChanged
			{
				ChangedPrivilegeInfos = changedMemberInfos.Select((NetMemberInfo memberInfo) => new GuildEventMemberPrivilegeChangedInfo
				{
					MemberId = memberInfo.MemberId,
					NewPrivilege = memberInfo.Privilege
				}).ToArray()
			};
			SendSocketGuildEvent(data);
		}
	}

	private void SendSocketMemberPrivilegeChangedEvent(SocketGuildMemberInfo changedMemberInfo)
	{
		SendSocketMemberPrivilegeChangedEvent(new SocketGuildMemberInfo[1] { changedMemberInfo });
	}

	private void SendSocketMemberPrivilegeChangedEvent(SocketGuildMemberInfo[] changedMemberInfos)
	{
		if (changedMemberInfos != null)
		{
			GuildEventMemberPrivilegeChanged data = new GuildEventMemberPrivilegeChanged
			{
				ChangedPrivilegeInfos = changedMemberInfos.Select((SocketGuildMemberInfo memberInfo) => new GuildEventMemberPrivilegeChangedInfo
				{
					MemberId = memberInfo.PlayerId,
					NewPrivilege = memberInfo.GuildPrivilege
				}).ToArray()
			};
			SendSocketGuildEvent(data);
		}
	}

	private void SendSocketMemberKickedEvent(string memberId)
	{
		GuildEventMemberKicked data = new GuildEventMemberKicked
		{
			MemberId = memberId
		};
		SendSocketGuildEvent(data);
	}

	private void SendSocketGuildRankupEvent()
	{
		NetGuildInfo guildInfoCache = GuildInfoCache;
		GuildEventGuildRankup data = new GuildEventGuildRankup
		{
			GuildRank = guildInfoCache.Rank,
			Score = guildInfoCache.Score,
			Money = guildInfoCache.Money
		};
		SendSocketGuildEvent(data);
	}

	private void SendSocketGuildEvent(GuildEventDataBase data, string targetPlayerId = "")
	{
		string eventJSON = JsonHelper.Serialize(data);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGuildEvent(targetPlayerId, eventJSON));
	}

	private void OnSocketSetGuildIdRes(object obj)
	{
		if (!(obj is RSSetGuildId))
		{
			Debug.LogError("obj is not RSSetGuildId");
			return;
		}
		Code result = (Code)((RSSetGuildId)obj).Result;
		if (result == Code.COMMUNITY_SET_GUILD_ID_SUCCESS)
		{
			if (_guildStateChanged)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.GUILD_ID_CHANGED, HasGuild);
			}
		}
		else
		{
			Debug.LogError(string.Format("{0}", result));
		}
	}

	private void OnSocketSetGuildInfoRes(object obj)
	{
		if (!(obj is RSSetGuildInfo))
		{
			Debug.LogError("obj is not RSSetGuildInfo");
			return;
		}
		Code result = (Code)((RSSetGuildInfo)obj).Result;
		if (result != Code.COMMUNITY_SET_GUILD_INFO_SUCCESS)
		{
			Debug.LogError(string.Format("{0}", result));
		}
	}

	private void OnSocketGetGuildInfoRes(object obj)
	{
		List<SocketGuildInfo> list = new List<SocketGuildInfo>();
		if (!(obj is RSGetGuildInfo))
		{
			Debug.LogError("obj is not RSGetGuildInfo");
		}
		else
		{
			RSGetGuildInfo rSGetGuildInfo = (RSGetGuildInfo)obj;
			Code result = (Code)rSGetGuildInfo.Result;
			if (result == Code.COMMUNITY_GET_GUILD_INFO_SUCCESS)
			{
				for (int i = 0; i < rSGetGuildInfo.GuildInfoJSONLength; i++)
				{
					string value = rSGetGuildInfo.GuildInfoJSON(i);
					SocketGuildInfo obj2;
					if (!string.IsNullOrEmpty(value) && JsonHelper.TryDeserialize<SocketGuildInfo>(value, out obj2) && obj2 != null)
					{
						list.Add(obj2);
					}
				}
			}
			else
			{
				Debug.LogError(string.Format("{0}", result));
			}
		}
		Action<List<SocketGuildInfo>> result2;
		if (_onSocketGetGuildInfoResCallbackQueue.TryDequeue(out result2))
		{
			if (result2 != null)
			{
				result2(list);
			}
		}
		else
		{
			Debug.LogError("Dequeue callback Error");
		}
	}

	private void OnSocketSetGuildMemberInfoRes(object obj)
	{
		if (!(obj is RSSetGuildMemberInfo))
		{
			Debug.LogError("obj is not RSSetGuildMemberInfo");
			return;
		}
		Code result = (Code)((RSSetGuildMemberInfo)obj).Result;
		if (result != Code.COMMUNITY_SET_GUILD_MEMBER_INFO_SUCCESS)
		{
			Debug.LogError(string.Format("{0}", result));
		}
	}

	private void OnSocketGetGuildMemberInfoRes(object obj)
	{
		List<SocketGuildMemberInfo> list = new List<SocketGuildMemberInfo>();
		if (!(obj is RSGetGuildMemberInfo))
		{
			Debug.LogError("obj is not RSGetGuildMemberInfo");
		}
		else
		{
			RSGetGuildMemberInfo rSGetGuildMemberInfo = (RSGetGuildMemberInfo)obj;
			Code result = (Code)rSGetGuildMemberInfo.Result;
			if (result == Code.COMMUNITY_GET_GUILD_MEMBER_INFO_SUCCESS)
			{
				for (int i = 0; i < rSGetGuildMemberInfo.MemberInfoJSONLength; i++)
				{
					string value = rSGetGuildMemberInfo.MemberInfoJSON(i);
					SocketGuildMemberInfo obj2;
					if (!string.IsNullOrEmpty(value) && JsonHelper.TryDeserialize<SocketGuildMemberInfo>(value, out obj2) && obj2 != null)
					{
						list.Add(obj2);
					}
				}
			}
			else
			{
				Debug.LogError(string.Format("{0}", result));
			}
		}
		Action<List<SocketGuildMemberInfo>> result2;
		if (_onSocketGetGuildMemberInfoResCallbackQueue.TryDequeue(out result2))
		{
			if (result2 != null)
			{
				result2(list);
			}
		}
		else
		{
			Debug.LogError("Dequeue callback Error");
		}
	}

	private void OnSocketSetGuildPlayerIdRes(object obj)
	{
		if (!(obj is RSSetGuildPlayerId))
		{
			Debug.LogError("obj is not RSSetGuildPlayerId");
			return;
		}
		Code result = (Code)((RSSetGuildPlayerId)obj).Result;
		if (result != Code.COMMUNITY_SET_GUILD_PLAYER_ID_SUCCESS)
		{
			Debug.LogError(string.Format("{0}", result));
		}
	}

	private void OnSocketGetGuildPlayerIdListRes(object obj)
	{
		if (!(obj is RSGetGuildPlayerIdList))
		{
			Debug.LogError("obj is not RSGetGuildPlayerIdList");
			return;
		}
		RSGetGuildPlayerIdList rSGetGuildPlayerIdList = (RSGetGuildPlayerIdList)obj;
		Code result = (Code)rSGetGuildPlayerIdList.Result;
		List<string> list = new List<string>();
		if (result == Code.COMMUNITY_GET_GUILD_PLAYER_ID_LIST_SUCCESS)
		{
			for (int i = 0; i < rSGetGuildPlayerIdList.PlayerIDListLength; i++)
			{
				string text = rSGetGuildPlayerIdList.PlayerIDList(i);
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
			}
		}
		else
		{
			Debug.LogError(string.Format("{0}", result));
		}
		Action<List<string>> result2;
		if (_onSocketGetGuildPlayerIdListResCallbackQueue.TryDequeue(out result2))
		{
			if (result2 != null)
			{
				result2(list);
			}
		}
		else
		{
			Debug.LogError("Dequeue callback Error");
		}
	}

	private void OnSocketGuildEvent(object obj)
	{
		if (!(obj is NTGuildEvent))
		{
			Debug.LogError("obj is not NTGuildEvent");
			return;
		}
		string eventJSON = ((NTGuildEvent)obj).EventJSON;
		GuildEventDataBase guildEventDataBase = JsonHelper.Deserialize<GuildEventDataBase>(eventJSON);
		if (guildEventDataBase == null)
		{
			Debug.LogError("baseData is null");
			return;
		}
		switch (guildEventDataBase.OpCode)
		{
		case SocketGuildEventType.MemberJoined:
		{
			GuildEventMemberJoined guildEventMemberJoined = JsonHelper.Deserialize<GuildEventMemberJoined>(eventJSON);
			GuildId = guildEventMemberJoined.GuildId;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, true);
			SendSocketSetGuildIdReq(GuildId, true);
			Action onSocketMemberJoinedEvent = this.OnSocketMemberJoinedEvent;
			if (onSocketMemberJoinedEvent != null)
			{
				onSocketMemberJoinedEvent();
			}
			break;
		}
		case SocketGuildEventType.MemberKicked:
		{
			_003C_003Ec__DisplayClass325_0 _003C_003Ec__DisplayClass325_ = new _003C_003Ec__DisplayClass325_0();
			_003C_003Ec__DisplayClass325_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass325_.data = JsonHelper.Deserialize<GuildEventMemberKicked>(eventJSON);
			_003C_003Ec__DisplayClass325_.isSelf = ManagedSingleton<PlayerHelper>.Instance.CheckPlayerIsSelf(_003C_003Ec__DisplayClass325_.data.MemberId);
			if (_003C_003Ec__DisplayClass325_.isSelf)
			{
				GuildId = 0;
				ClearCacheData();
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, true);
				SendSocketSetGuildIdReq(0, true);
			}
			if (GuildUIHelper.IsStateLocked)
			{
				GuildUIHelper.OnStateUnlockedEvent += _003C_003Ec__DisplayClass325_._003COnSocketGuildEvent_003Eg__action_007C0;
			}
			else
			{
				_003C_003Ec__DisplayClass325_._003COnSocketGuildEvent_003Eg__action_007C0();
			}
			break;
		}
		case SocketGuildEventType.MemberPrivilegeChanged:
		{
			GuildEventMemberPrivilegeChanged guildEventMemberPrivilegeChanged = JsonHelper.Deserialize<GuildEventMemberPrivilegeChanged>(eventJSON);
			bool isSelfPrivilegeChanged = false;
			GuildEventMemberPrivilegeChangedInfo[] changedPrivilegeInfos = guildEventMemberPrivilegeChanged.ChangedPrivilegeInfos;
			foreach (GuildEventMemberPrivilegeChangedInfo guildEventMemberPrivilegeChangedInfo in changedPrivilegeInfos)
			{
				if (ManagedSingleton<PlayerHelper>.Instance.CheckPlayerIsSelf(guildEventMemberPrivilegeChangedInfo.MemberId))
				{
					isSelfPrivilegeChanged = true;
				}
			}
			if (isSelfPrivilegeChanged)
			{
				OnGetGuildInfoOnceEvent += delegate
				{
					Action<bool> onSocketMemberPrivilegeChangedEvent2 = this.OnSocketMemberPrivilegeChangedEvent;
					if (onSocketMemberPrivilegeChangedEvent2 != null)
					{
						onSocketMemberPrivilegeChangedEvent2(isSelfPrivilegeChanged);
					}
				};
				ReqGetGuildInfo();
			}
			else
			{
				Action<bool> onSocketMemberPrivilegeChangedEvent = this.OnSocketMemberPrivilegeChangedEvent;
				if (onSocketMemberPrivilegeChangedEvent != null)
				{
					onSocketMemberPrivilegeChangedEvent(isSelfPrivilegeChanged);
				}
			}
			break;
		}
		case SocketGuildEventType.HeaderPowerChanged:
		{
			GuildEventHeaderPowerChanged guildEventHeaderPowerChanged = JsonHelper.Deserialize<GuildEventHeaderPowerChanged>(eventJSON);
			GuildInfoCache.HeaderPower = guildEventHeaderPowerChanged.NewHeaderPower;
			Action onSocketHeaderPowerChangedEvent = this.OnSocketHeaderPowerChangedEvent;
			if (onSocketHeaderPowerChangedEvent != null)
			{
				onSocketHeaderPowerChangedEvent();
			}
			break;
		}
		case SocketGuildEventType.GuildRemoved:
			JsonHelper.Deserialize<GuildEventGuildRemoved>(eventJSON);
			ClearCacheData();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_GUILD_HINT, true);
			SendSocketSetGuildIdReq(0, true);
			if (GuildUIHelper.IsStateLocked)
			{
				GuildUIHelper.OnStateUnlockedEvent += _003COnSocketGuildEvent_003Eg__action_007C325_2;
			}
			else
			{
				_003COnSocketGuildEvent_003Eg__action_007C325_2();
			}
			break;
		case SocketGuildEventType.GuildRankup:
		{
			GuildEventGuildRankup guildEventGuildRankup = JsonHelper.Deserialize<GuildEventGuildRankup>(eventJSON);
			NetGuildInfo guildInfoCache = GuildInfoCache;
			guildInfoCache.Rank = guildEventGuildRankup.GuildRank;
			guildInfoCache.Score = guildEventGuildRankup.Score;
			guildInfoCache.Money = guildEventGuildRankup.Money;
			RefreshGuildSetting();
			Action onSocketGuildRankupEvent = this.OnSocketGuildRankupEvent;
			if (onSocketGuildRankupEvent != null)
			{
				onSocketGuildRankupEvent();
			}
			break;
		}
		}
	}

	public string GetGuildRankString(int guildRank)
	{
		switch (guildRank)
		{
		case 1:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CLASS_E");
		case 2:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CLASS_D");
		case 3:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CLASS_C");
		case 4:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CLASS_B");
		case 5:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CLASS_A");
		case 6:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CLASS_S");
		case 7:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CLASS_SS");
		default:
			return string.Empty;
		}
	}

	public bool TryGetMemberInfo(string memberId, out NetMemberInfo memberInfo)
	{
		memberInfo = _memberInfoListCache.FirstOrDefault((NetMemberInfo info) => info.MemberId == memberId);
		return memberInfo != null;
	}

	public void CheckGuildPrivilege(out bool isLeader, out bool isHeader, out GuildHeaderPower headerPower)
	{
		if (!HasGuild)
		{
			isLeader = false;
			isHeader = false;
			headerPower = GuildHeaderPower.None;
		}
		else
		{
			GuildPrivilege guildPrivilege = (GuildPrivilege)SelfMemberInfo.Privilege;
			isLeader = guildPrivilege == GuildPrivilege.GuildLeader;
			isHeader = guildPrivilege < GuildPrivilege.GuildMember;
			headerPower = (GuildHeaderPower)GuildInfoCache.HeaderPower;
		}
	}

	public bool CheckHasInvitePower()
	{
		bool isLeader;
		bool isHeader;
		GuildHeaderPower headerPower;
		CheckGuildPrivilege(out isLeader, out isHeader, out headerPower);
		if (!isLeader)
		{
			if (isHeader)
			{
				return headerPower.HasFlag(GuildHeaderPower.Invite);
			}
			return false;
		}
		return true;
	}

	public void ConfirmChangeScene()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnConfirmChangeSceneUILoaded);
	}

	private void OnConfirmChangeSceneUILoaded(CommonUI ui)
	{
		ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_JOINED"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), OnConfirmChangeScene);
	}

	private void OnConfirmChangeScene()
	{
		ReqGetGuildInfo(true);
		Action onConfirmChangeSceneEvent = this.OnConfirmChangeSceneEvent;
		if (onConfirmChangeSceneEvent != null)
		{
			onConfirmChangeSceneEvent();
		}
	}

	private void OnGetGuildInfoNotNull()
	{
		Action<Code> onGetGuildInfoEvent = this.OnGetGuildInfoEvent;
		if (onGetGuildInfoEvent != null)
		{
			onGetGuildInfoEvent(Code.GUILD_GET_INFO_SUCCESS);
		}
		Action onGetGuildInfoOnceEvent = this.OnGetGuildInfoOnceEvent;
		if (onGetGuildInfoOnceEvent != null)
		{
			onGetGuildInfoOnceEvent();
		}
		this.OnGetGuildInfoOnceEvent = null;
		if (_loadGuildSceneAfterGetGuildInfo)
		{
			_loadGuildSceneAfterGetGuildInfo = false;
			OpenGuildLobbyScene();
		}
	}

	private void OnGetGuildInfoNull()
	{
		Action<Code> onGetGuildInfoEvent = this.OnGetGuildInfoEvent;
		if (onGetGuildInfoEvent != null)
		{
			onGetGuildInfoEvent(Code.GUILD_GET_INFO_SUCCESS);
		}
		Action onGetGuildInfoOnceEvent = this.OnGetGuildInfoOnceEvent;
		if (onGetGuildInfoOnceEvent != null)
		{
			onGetGuildInfoOnceEvent();
		}
		this.OnGetGuildInfoOnceEvent = null;
		_loadGuildSceneAfterGetGuildInfo = false;
	}

	public void RefreshBusyStatusAndSearchHUD(string targetId, Action p_cb)
	{
		RefreshBusyStatusAndSearchHUD(new List<string> { targetId }, p_cb);
	}

	public void RefreshBusyStatusAndSearchHUD(IEnumerable<string> targetIds, Action p_cb)
	{
		if (targetIds.Count() == 0)
		{
			Action action = p_cb;
			if (action != null)
			{
				action();
			}
			return;
		}
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetBusyList, delegate(object socketResp)
		{
			OnGetBusyListRes(socketResp, targetIds, p_cb);
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		}, 0, true);
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetBusyList(targetIds.ToArray()));
	}

	private void OnGetBusyListRes(object socketResp, IEnumerable<string> targetIds, Action p_cb)
	{
		if (socketResp is RSGetBusyList)
		{
			RSGetBusyList rSGetBusyList = (RSGetBusyList)socketResp;
			if (rSGetBusyList.Result == 70303)
			{
				for (int i = 0; i < rSGetBusyList.PlayerIDListLength; i++)
				{
					PlayerBusyStatusCache[rSGetBusyList.PlayerIDList(i)] = rSGetBusyList.BusyList(i);
				}
				SearchHUD(targetIds, p_cb);
			}
			else
			{
				Debug.LogError(string.Format("Result = {0}", (Code)rSGetBusyList.Result));
			}
		}
		else
		{
			Debug.LogError("socketResp Error");
		}
	}

	public void SearchHUD(string targetId, Action p_cb)
	{
		SearchHUD(new List<string> { targetId }, p_cb);
	}

	public void SearchHUD(IEnumerable<string> targetIds, Action p_cb)
	{
		if (targetIds.Count() == 0)
		{
			Action action = p_cb;
			if (action != null)
			{
				action();
			}
			return;
		}
		targetIds = targetIds.Except(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.Keys);
		if (targetIds.Count() == 0)
		{
			Action action2 = p_cb;
			if (action2 != null)
			{
				action2();
			}
		}
		else
		{
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerHUDList, delegate(object socketResp)
			{
				OnSearchHUDRes(socketResp, p_cb);
			}, 0, true);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUDList(targetIds.ToArray()));
		}
	}

	private void OnSearchHUDRes(object socketResp, Action p_cb)
	{
		if (socketResp is RSGetPlayerHUDList)
		{
			RSGetPlayerHUDList rs = (RSGetPlayerHUDList)socketResp;
			if (rs.Result == 70300)
			{
				UpdateHUD(rs);
				if (p_cb != null)
				{
					p_cb();
				}
			}
			else
			{
				Debug.LogError(string.Format("Result = {0}", (Code)rs.Result));
			}
		}
		else
		{
			Debug.LogError("socketResp Error");
		}
	}

	private void UpdateHUD(RSGetPlayerHUDList rs)
	{
		for (int i = 0; i < rs.PlayerHUDLength; i++)
		{
			SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(rs.PlayerHUD(i));
			if (socketPlayerHUD == null)
			{
				Debug.LogError(string.Format("Null player HUD {0}", i));
			}
			else
			{
				ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketPlayerHUD.m_PlayerId, socketPlayerHUD);
			}
		}
	}

	private void RefreshGuildSetting()
	{
		if (HasGuild && (GuildSetting == null || GuildSetting.GuildRank != _guildInfoCache.Rank))
		{
			GuildSetting guildSetting;
			if (GuildSetting.TryGetSettingByGuildRank(_guildInfoCache.Rank, out guildSetting, true))
			{
				GuildSetting = guildSetting;
			}
			else
			{
				Debug.LogError(string.Format("Failed to get {0} of Rank {1}", "GuildSetting", _guildInfoCache.Rank));
			}
		}
	}

	private void HandleErrorCode(string method, int ackCode)
	{
		HandleErrorCode(method, (Code)ackCode);
	}

	private void HandleErrorCode(string method, Code ackCode)
	{
		switch (ackCode)
		{
		case Code.GUILD_ALREADY_ONE:
			CommonUIHelper.ShowCommonTipUI("GUILD_SETUP_WARN3");
			break;
		case Code.GUILD_NAME_HAS_BEEN_USED:
			CommonUIHelper.ShowCommonTipUI("GUILD_NAME_REPEAT");
			break;
		case Code.GUILD_NOT_ENOUGHT_POWER:
			CommonUIHelper.ShowCommonTipUI("GUILD_POWER_LACK");
			break;
		case Code.GUILD_DATA_LOCKED:
			CommonUIHelper.ShowCommonTipUI("SYSTEM_BUSY");
			break;
		case Code.GUILD_NOT_FOUND_DATA:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_WARN3");
			break;
		case Code.GUILD_HALLINVITE_MAX_ERROR:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITEOVER");
			break;
		case Code.GUILD_INVITE_MAX:
			CommonUIHelper.ShowCommonTipUI("GUILD_INVITEMAX");
			break;
		case Code.GUILD_INVITED_FAIL:
			CommonUIHelper.ShowCommonTipUI("GUILD_HALL_INVITEING");
			break;
		case Code.GUILD_LEVEL_UP_MATERIAL_ERROR:
			CommonUIHelper.ShowCommonTipUI("GUILD_MONEY_LACK");
			break;
		case Code.GUILD_CHECK_HEADER_POWER_FAIL:
			CommonUIHelper.ShowCommonTipUI("GUILD_PERMISSION_REVOKED");
			break;
		case Code.GUILD_INFO_MEMBER_INFO_FAIL:
		case Code.GUILD_GUILD_INFO_FAIL:
		case Code.GUILD_ITEM_NOT_ENOUGHT_JEWEL:
		case Code.GUILD_NOT_LEADER:
		case Code.GUILD_MEMBER_NOT_HIRING:
		case Code.GUILD_NAME_FORBIDDEN:
		case Code.GUILD_PLAYER_INFO_NOT_FOUND:
		case Code.GUILD_INVITE_NOT_FOUND_DATA:
		case Code.GUILD_KICK_PRIVILEGE_FAIL:
		case Code.GUILD_INPUT_TEXT_OVER_MAX:
		case Code.GUILD_INPUT_APPLY_TYPE_ERROR:
		case Code.GUILD_INPUT_POWER_DEMAND_ERROR:
		case Code.GUILD_INPUT_HEADER_POWER_ERROR:
		case Code.GUILD_GET_GUILD_MAIN_ERROR:
		case Code.GUILD_GET_POWER_TOWER_TABLE_ERROR:
		case Code.GUILD_GET_POWER_PILLAR_DATA_ERROR:
		case Code.GUILD_GET_ORE_DATA_ERROR:
		case Code.GUILD_APPLIED_FAIL:
		case Code.GUILD_CHANGE_PRIVILEGE_FAIL:
		case Code.GUILD_KICK_SELF_FAIL:
		case Code.GUILD_ITEM_NOT_ENOUGHT_MONEY:
		case Code.GUILD_NOT_ENOUGHT_MATERIAL:
		case Code.GUILD_ORE_NOT_FOUND:
		case Code.GUILD_ORE_TABLE_ERROR:
		case Code.GUILD_ORE_LEVEL_UP_ERROR:
		case Code.GUILD_PILLAR_USED_ERROR:
		case Code.GUILD_ORE_OPENING_ERROR:
		case Code.GUILD_ORE_GROUP_ID_ERROR:
		case Code.GUILD_PILLAR_NOT_FOUND:
		case Code.GUILD_PRIVILEGE_TARGET_FAIL:
		case Code.GUILD_PRIVILEGE_TYPE_FAIL:
			Debug.LogError(string.Format("[{0}] Unhandled Error Code : {1}", method, ackCode));
			CommonUIHelper.ShowCommonTipUI(string.Format("ERROR : {0}", (int)ackCode), false);
			break;
		case Code.GUILD_MEMBER_MAX:
		case Code.GUILD_MEMBER_NOT_FOUND_DATA:
		case Code.GUILD_APPLY_NOT_FOUND_DATA:
		case Code.GUILD_LEAVE_COOLING_FAIL:
			Debug.Log(string.Format("[{0}] Customs Handling Error Code : {1}", method, ackCode));
			break;
		default:
			Debug.LogError(string.Format("[{0}] Unhandled Error Code : {1}", method, ackCode));
			break;
		}
	}

	[CompilerGenerated]
	private void _003COnSocketGuildEvent_003Eg__action_007C325_2()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_HALL_WARN3"), TIP_DIALOG_TIME);
		Action onSocketGuildRemovedEvent = this.OnSocketGuildRemovedEvent;
		if (onSocketGuildRemovedEvent != null)
		{
			onSocketGuildRemovedEvent();
		}
	}
}
