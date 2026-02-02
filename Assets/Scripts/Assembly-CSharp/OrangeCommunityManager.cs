#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CallbackDefs;
using Newtonsoft.Json;
using OrangeSocket;
using UnityEngine;
using cb;
using cc;
using enums;

public class OrangeCommunityManager : MonoBehaviourSingleton<OrangeCommunityManager>
{
	private enum ConnectingState
	{
		Disconnect = 0,
		Connecting = 1,
		Connected = 2
	}

	public class TargetRankingInfo
	{
		public string m_PlayerId;

		public string m_Name;

		public int m_Level;

		public int m_StandbyCharID;

		public int m_Rank;

		public int m_Score;

		public int m_MainWeaponID;

		public int m_BastWeaponID;

		public int m_StandbyCharSkin;

		public int m_MainWeaponSkin;

		public int m_BastWeaponSkin;
	}

	private ConnectingState connectingState;

	private bool bApplicationQuit;

	public bool RankingUIFlag;

	public bool bInitChatLog;

	private bool _repeatConnect;

	private readonly int CHECK_CONNECTION = 30;

	private readonly int UPDATE_COMMUNITY_INFO = 300;

	private readonly int RECONNECT_RETRY_TIMES = 5;

	private readonly float RECONNECT_RETRY_WAIT = 5f;

	private int _retryRemain;

	public Dictionary<ChatChannel, List<SocketChatLogInfo>> dicChatLog = new Dictionary<ChatChannel, List<SocketChatLogInfo>>();

	public Dictionary<string, bool> dicFriendChatIconFlag = new Dictionary<string, bool>();

	public Dictionary<RankType, List<SocketRankingInfo>> dicRankingList = new Dictionary<RankType, List<SocketRankingInfo>>();

	public Dictionary<RankType, List<SocketRankingInfo>> dicFriendRankingList = new Dictionary<RankType, List<SocketRankingInfo>>();

	public Dictionary<RankType, List<SocketRankingInfo>> dicPersonalRankingList = new Dictionary<RankType, List<SocketRankingInfo>>();

	public Dictionary<RankType, SocketRankingTypeInfo> dicPlayerRank = new Dictionary<RankType, SocketRankingTypeInfo>();

	public Dictionary<RankType, SocketRankingTypeInfo> dicPlayerFriendRank = new Dictionary<RankType, SocketRankingTypeInfo>();

	public static List<SocketRankingInfo> m_RankingInfo = new List<SocketRankingInfo>();

	public static TargetRankingInfo m_TargetRankingInfo = new TargetRankingInfo();

	public Dictionary<string, SocketFriendInfo> dicFriend = new Dictionary<string, SocketFriendInfo>();

	public Dictionary<string, SocketBlackInfo> dicBlack = new Dictionary<string, SocketBlackInfo>();

	public Dictionary<string, SocketFriendInviteRequestInfo> dicFriendInviteRequest = new Dictionary<string, SocketFriendInviteRequestInfo>();

	public Dictionary<string, SocketFriendInviteReceiveInfo> dicFriendInviteReceive = new Dictionary<string, SocketFriendInviteReceiveInfo>();

	public Dictionary<string, SocketFriendRewardRequestInfo> dicFriendRewardRequest = new Dictionary<string, SocketFriendRewardRequestInfo>();

	public Dictionary<string, SocketFriendRewardReceiveInfo> dicFriendRewardReceive = new Dictionary<string, SocketFriendRewardReceiveInfo>();

	public int RewardUesdCount;

	public Dictionary<string, SocketFriendFollowInfo> dicFriendFollow = new Dictionary<string, SocketFriendFollowInfo>();

	public Dictionary<string, SocketContactInfo> dicContact = new Dictionary<string, SocketContactInfo>();

	public Dictionary<string, SocketPlayerHUD> dicHUD = new Dictionary<string, SocketPlayerHUD>();

	public SocketPlayerInfoTmp tmpPlayerInfo;

	public string strPlayerInfoJson;

	public string strPlayerHUDJson;

	private bool RepeatConnect
	{
		get
		{
			return _repeatConnect;
		}
		set
		{
			_repeatConnect = value;
			Debug.LogWarning(string.Format("Set {0} => {1}", "RepeatConnect", _repeatConnect));
		}
	}

	public Dictionary<int, SocketGuildInfo> SocketGuildInfoCache { get; private set; } = new Dictionary<int, SocketGuildInfo>();


	public Dictionary<string, SocketGuildMemberInfo> SocketGuildMemberInfoCache { get; private set; } = new Dictionary<string, SocketGuildMemberInfo>();


	public event Action OnConnectedEvent;

	public event Action OnServerConnectedEvent;

	private bool IsForceConnectingMode()
	{
		string nowScene = MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene;
		if (nowScene == "GuildMain")
		{
			return true;
		}
		return false;
	}

	public void OnApplicationQuit()
	{
		bApplicationQuit = true;
	}

	public void OnApplicationPause(bool paused)
	{
		bApplicationQuit = paused;
	}

	public void UpdateRankingInfo()
	{
		if (MonoBehaviourSingleton<CCSocketClient>.Instance.Connected())
		{
			ManagedSingleton<RankingHelper>.Instance.RetrieveRankingList();
			ManagedSingleton<RankingHelper>.Instance.RetrievePersonalRankingList();
		}
	}

	public void OnSetBusy(UserStatus busy)
	{
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetBusyStatus((int)busy));
	}

	public void UpdateCommunityData()
	{
		ManagedSingleton<FriendHelper>.Instance.RetrieveFriendList();
		ManagedSingleton<FriendHelper>.Instance.RetrieveBlackList();
		ManagedSingleton<FriendHelper>.Instance.RetrievFriendInviteRequestList();
		ManagedSingleton<FriendHelper>.Instance.RetrievFriendInviteReceiveList();
		ManagedSingleton<FriendHelper>.Instance.RetrievFriendGetGaveRewardList();
		ManagedSingleton<FriendHelper>.Instance.RetrievFriendGetRewardList();
		ManagedSingleton<FriendHelper>.Instance.RetrievContactList();
		ManagedSingleton<FriendHelper>.Instance.OnUpdateDisplayHint();
		ManagedSingleton<FriendHelper>.Instance.RetrievCommunityMessage();
		if (!IsInvoking("UpdateRankingInfo"))
		{
			InvokeRepeating("UpdateRankingInfo", 3f, 600f);
		}
	}

	private void OnCheckServerConnected()
	{
		Debug.Log(string.Format("[{0}] RepeatConnect = {1}", "OnCheckServerConnected", RepeatConnect));
		if (RepeatConnect)
		{
			if (_retryRemain > 0)
			{
				Debug.LogWarning("[OnCheckServerConnected] Is already reconnecting");
			}
			else if (!MonoBehaviourSingleton<CCSocketClient>.Instance.Connected())
			{
				_retryRemain = RECONNECT_RETRY_TIMES;
				RegistConnectedCall();
				ConnectedToCommunityServer();
			}
		}
	}

	public bool OnCheckCommunityServerConnected()
	{
		bool num = MonoBehaviourSingleton<CCSocketClient>.Instance.Connected();
		if (!num)
		{
			CancelInvoke("OnCheckServerConnected");
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_NOT_REACHABLE_DESC_3"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
				ui.closeCB = OnCheckServerConnected;
				_retryRemain = 0;
			});
		}
		return num;
	}

	public void RegistConnectedCall()
	{
		OnConnectedEvent -= UpdateCommunityData;
		OnConnectedEvent += UpdateCommunityData;
	}

	public void CommunityServerLogin()
	{
		switch (connectingState)
		{
		case ConnectingState.Connected:
		{
			Action onConnectedEvent = this.OnConnectedEvent;
			if (onConnectedEvent != null)
			{
				onConnectedEvent();
			}
			this.OnConnectedEvent = null;
			break;
		}
		case ConnectingState.Connecting:
			Debug.LogWarning("Community Server連線中... 等待時不應再次連線");
			break;
		default:
			connectingState = ConnectingState.Connecting;
			ConnectedToCommunityServer();
			break;
		}
	}

	private void ConnectedToCommunityServer()
	{
		string host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Community.Host;
		int port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Community.Port;
		if (IsForceConnectingMode())
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		}
		MonoBehaviourSingleton<CCSocketClient>.Instance.ConnectToServer(host, port, ConnectCallback, DisconnectCallback, SocketIOErrorCallback);
	}

	public void CommunityServerLogout()
	{
		bInitChatLog = false;
		RepeatConnect = false;
		CancelInvoke();
		MonoBehaviourSingleton<CCSocketClient>.Instance.Disconnect();
		connectingState = ConnectingState.Disconnect;
	}

	private void ConnectCallback(bool connected)
	{
		if (connected)
		{
			if (IsForceConnectingMode())
			{
				MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
			}
			Action onServerConnectedEvent = this.OnServerConnectedEvent;
			if (onServerConnectedEvent != null)
			{
				onServerConnectedEvent();
			}
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSChatLogin, delegate(object res)
			{
				object obj;
				if ((obj = res) is RSChatLogin)
				{
					RSChatLogin rSChatLogin = (RSChatLogin)obj;
					if (rSChatLogin.Result != 70000)
					{
						Debug.LogWarning("Community Server is down! res:" + rSChatLogin.Result);
						if (rSChatLogin.Result == 1058)
						{
							MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("LOGIN_USER_OVER");
						}
						else if (rSChatLogin.Result == 70050)
						{
							MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("AUTHENTICATION_EXPIRED");
						}
						else
						{
							MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rSChatLogin.Result);
						}
					}
					else
					{
						connectingState = ConnectingState.Connected;
						Debug.Log("Community Server is connected!");
						Action onConnectedEvent = this.OnConnectedEvent;
						if (onConnectedEvent != null)
						{
							onConnectedEvent();
						}
						this.OnConnectedEvent = null;
						RepeatConnect = true;
						_retryRemain = 0;
						Invoke("UpdateCommunityPlayerInfo", 3f);
						InvokeRepeating("OnCheckServerConnected", 0f, CHECK_CONNECTION);
						InvokeRepeating("UpdateCommunityPlayerInfo", UPDATE_COMMUNITY_INFO, UPDATE_COMMUNITY_INFO);
					}
				}
			}, 0, true);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatLogin(SocketCommon.ProtocolVersion, MonoBehaviourSingleton<GameServerService>.Instance.ServiceToken, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID));
		}
		else
		{
			if (bApplicationQuit)
			{
				return;
			}
			_retryRemain--;
			Debug.LogError(string.Format("Retry Remain = {0}", _retryRemain));
			if (_retryRemain <= 0)
			{
				if (!IsForceConnectingMode())
				{
					Debug.LogWarning("Now Scene = " + MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene + " and stop reconnect");
					return;
				}
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg(delegate
				{
					_retryRemain = RECONNECT_RETRY_TIMES;
					ConnectedToCommunityServer();
				}, delegate
				{
					CommunityServerLogout();
					if (IsForceConnectingMode())
					{
						MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
					}
					MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
					{
						MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch");
					});
				});
			}
			else
			{
				Invoke("ConnectedToCommunityServer", RECONNECT_RETRY_WAIT);
			}
		}
	}

	private void DisconnectCallback()
	{
		Debug.Log(string.Format("[{0}] DisconnectCallback, connectingState = {1}", "DisconnectCallback", connectingState));
		switch (connectingState)
		{
		case ConnectingState.Disconnect:
		case ConnectingState.Connected:
			connectingState = ConnectingState.Disconnect;
			CancelInvoke("OnCheckServerConnected");
			OnCheckServerConnected();
			break;
		case ConnectingState.Connecting:
			Debug.Log("[DisconnectCallback] Is already connecting");
			break;
		}
	}

	private void SocketIOErrorCallback(bool connected)
	{
		Debug.Log("[Community] SocketIOErrorCallback");
		if (IsForceConnectingMode())
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		}
		connectingState = ConnectingState.Disconnect;
		if (RepeatConnect)
		{
			return;
		}
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg(delegate
		{
			ConnectedToCommunityServer();
		}, delegate
		{
			CommunityServerLogout();
			MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
			{
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch");
			});
		});
	}

	public void SetPlayerHUD(int server_id, string name, string p_info)
	{
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerHUD(server_id, name, p_info));
	}

	public bool OnGetFriendChatIconFlag(string pid)
	{
		if (!dicFriendChatIconFlag.ContainsKey(pid))
		{
			return false;
		}
		return dicFriendChatIconFlag[pid];
	}

	public void RefreshSocketGuildInfoCache(List<SocketGuildInfo> guildInfos)
	{
		SocketGuildInfoCache.Clear();
		if (guildInfos != null && guildInfos.Count > 0)
		{
			SocketGuildInfoCache = guildInfos.ToDictionary((SocketGuildInfo guildinfo) => guildinfo.GuildId, (SocketGuildInfo guildinfo) => guildinfo);
		}
	}

	public void RefreshSocketGuildMemberInfoCache(List<SocketGuildMemberInfo> memberInfos)
	{
		SocketGuildMemberInfoCache.Clear();
		if (memberInfos != null && memberInfos.Count > 0)
		{
			SocketGuildMemberInfoCache = memberInfos.ToDictionary((SocketGuildMemberInfo memberInfo) => memberInfo.PlayerId, (SocketGuildMemberInfo memberInfo) => memberInfo);
		}
	}

	public void SetPlayerRankInfo(RankType rt, int score, int prm)
	{
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerRankInfo((int)rt, ManagedSingleton<PlayerHelper>.Instance.GetLV(), score, prm));
	}

	private int GetWeaponPower(int wid)
	{
		return ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(wid).nBattlePower;
	}

	private RankNameStars.RANK GetWeaponRarity(int wid)
	{
		return (RankNameStars.RANK)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[wid].n_RARITY;
	}

	private int GetWeaponSatr(int wid)
	{
		WeaponInfo value;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(wid, out value))
		{
			return value.netInfo.Star;
		}
		return 0;
	}

	public void UpdatePlayerHUD()
	{
		if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLogin && ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			SocketPlayerHUD socketPlayerHUD = new SocketPlayerHUD();
			socketPlayerHUD.m_PlayerId = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
			socketPlayerHUD.m_Name = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			socketPlayerHUD.m_IconUrl = "";
			socketPlayerHUD.m_IconNumber = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID;
			socketPlayerHUD.m_FrameNumber = 1;
			socketPlayerHUD.m_Frame = 0;
			socketPlayerHUD.m_Level = ManagedSingleton<PlayerHelper>.Instance.GetLV();
			socketPlayerHUD.m_Exp = ManagedSingleton<PlayerHelper>.Instance.GetExp();
			socketPlayerHUD.m_ServerID = MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID;
			socketPlayerHUD.m_Power = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower();
			socketPlayerHUD.m_GuildID = 0;
			socketPlayerHUD.m_GuildName = "";
			socketPlayerHUD.m_StandbyCharID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara;
			socketPlayerHUD.m_StandbyCharSkin = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[socketPlayerHUD.m_StandbyCharID].netInfo.Skin;
			socketPlayerHUD.m_MainWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
			socketPlayerHUD.m_SubWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
			socketPlayerHUD.m_MainWeaponSkin = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[socketPlayerHUD.m_MainWeaponID].netInfo.Skin;
			socketPlayerHUD.m_TitleNumber = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID;
			if (socketPlayerHUD.m_SubWeaponID != 0)
			{
				socketPlayerHUD.m_SubWeaponSkin = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[socketPlayerHUD.m_SubWeaponID].netInfo.Skin;
			}
			strPlayerHUDJson = JsonHelper.Serialize(socketPlayerHUD);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerHUD(socketPlayerHUD.m_ServerID, socketPlayerHUD.m_Name, strPlayerHUDJson));
			ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketPlayerHUD.m_PlayerId, strPlayerHUDJson);
		}
	}

	public void UpdateCommunityPlayerInfo()
	{
		if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLogin && ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			SocketPlayerHUD socketPlayerHUD = new SocketPlayerHUD();
			socketPlayerHUD.m_PlayerId = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
			socketPlayerHUD.m_Name = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			socketPlayerHUD.m_IconUrl = "";
			socketPlayerHUD.m_IconNumber = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID;
			socketPlayerHUD.m_FrameNumber = 1;
			socketPlayerHUD.m_Frame = 0;
			socketPlayerHUD.m_Level = ManagedSingleton<PlayerHelper>.Instance.GetLV();
			socketPlayerHUD.m_Exp = ManagedSingleton<PlayerHelper>.Instance.GetExp();
			socketPlayerHUD.m_ServerID = MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID;
			socketPlayerHUD.m_Power = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower();
			socketPlayerHUD.m_GuildID = 0;
			socketPlayerHUD.m_GuildName = "";
			socketPlayerHUD.m_StandbyCharID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara;
			socketPlayerHUD.m_StandbyCharSkin = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[socketPlayerHUD.m_StandbyCharID].netInfo.Skin;
			socketPlayerHUD.m_MainWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
			socketPlayerHUD.m_SubWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
			socketPlayerHUD.m_MainWeaponSkin = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[socketPlayerHUD.m_MainWeaponID].netInfo.Skin;
			socketPlayerHUD.m_TitleNumber = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID;
			if (socketPlayerHUD.m_SubWeaponID != 0)
			{
				socketPlayerHUD.m_SubWeaponSkin = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[socketPlayerHUD.m_SubWeaponID].netInfo.Skin;
			}
			strPlayerHUDJson = JsonHelper.Serialize(socketPlayerHUD);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerHUD(socketPlayerHUD.m_ServerID, socketPlayerHUD.m_Name, strPlayerHUDJson));
			ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketPlayerHUD.m_PlayerId, strPlayerHUDJson);
			PlayerStatus playerFinalStatus = ManagedSingleton<StatusHelper>.Instance.GetPlayerFinalStatus();
			SocketPlayerInfo socketPlayerInfo = new SocketPlayerInfo();
			socketPlayerInfo.m_HP = playerFinalStatus.nHP;
			socketPlayerInfo.m_ATK = playerFinalStatus.nATK;
			socketPlayerInfo.m_DEF = playerFinalStatus.nDEF;
			socketPlayerInfo.m_HIT = playerFinalStatus.nHIT;
			socketPlayerInfo.m_BLK = playerFinalStatus.nDOD;
			socketPlayerInfo.m_AVD = playerFinalStatus.nLuck;
			socketPlayerInfo.m_CRI_Rate = ((int)playerFinalStatus.nCRI + OrangeConst.PLAYER_CRI_BASE) / 100;
			socketPlayerInfo.m_CRI_DMG = 100 + ((int)playerFinalStatus.nCriDmgPercent + OrangeConst.PLAYER_CRIDMG_BASE) / 100;
			socketPlayerInfo.m_CRI_OFT = playerFinalStatus.nReduceCriPercent;
			socketPlayerInfo.m_CRI_DeRate = playerFinalStatus.nBlockPercent;
			socketPlayerInfo.m_BLK_DeRate = ((int)playerFinalStatus.nBlockDmgPercent + OrangeConst.PLAYER_PARRYDEF_BASE) / 100;
			socketPlayerInfo.m_BLK_OFT = playerFinalStatus.nReduceBlockPercent;
			string s = JsonConvert.SerializeObject(ManagedSingleton<PlayerNetManager>.Instance.dicCharacter);
			byte[] inArray = LZ4Helper.EncodeWithHeader(Encoding.UTF8.GetBytes(s));
			socketPlayerInfo.m_CharJSON = "";
			string charJSON = Convert.ToBase64String(inArray);
			s = JsonConvert.SerializeObject(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon);
			byte[] inArray2 = LZ4Helper.EncodeWithHeader(Encoding.UTF8.GetBytes(s));
			socketPlayerInfo.m_WeaponJSON = "";
			string weaponJSON = Convert.ToBase64String(inArray2);
			s = JsonConvert.SerializeObject(ManagedSingleton<PlayerNetManager>.Instance.dicChip);
			byte[] inArray3 = LZ4Helper.EncodeWithHeader(Encoding.UTF8.GetBytes(s));
			socketPlayerInfo.m_ChipJSON = "";
			string chipJSON = Convert.ToBase64String(inArray3);
			s = JsonConvert.SerializeObject(ManagedSingleton<PlayerNetManager>.Instance.dicEquip.Where((KeyValuePair<int, EquipInfo> q) => q.Value.netEquipmentInfo.Equip == 1).ToDictionary((KeyValuePair<int, EquipInfo> p) => p.Key, (KeyValuePair<int, EquipInfo> p) => p.Value));
			byte[] inArray4 = LZ4Helper.EncodeWithHeader(Encoding.UTF8.GetBytes(s));
			socketPlayerInfo.m_EquipJSON = "";
			string equipJSON = Convert.ToBase64String(inArray4);
			s = JsonConvert.SerializeObject(ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike);
			byte[] inArray5 = LZ4Helper.EncodeWithHeader(Encoding.UTF8.GetBytes(s));
			socketPlayerInfo.m_FinalStrikeJSON = "";
			string finalStrikeJSON = Convert.ToBase64String(inArray5);
			s = JsonConvert.SerializeObject(OnGetSignData());
			byte[] inArray6 = LZ4Helper.EncodeWithHeader(Encoding.UTF8.GetBytes(s));
			socketPlayerInfo.m_TitleJSON = "";
			string titleJSON = Convert.ToBase64String(inArray6);
			strPlayerInfoJson = JsonHelper.Serialize(socketPlayerInfo);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerInfo(strPlayerHUDJson, strPlayerInfoJson, charJSON, weaponJSON, chipJSON, equipJSON, finalStrikeJSON, titleJSON));
			tmpPlayerInfo = default(SocketPlayerInfoTmp);
			tmpPlayerInfo.PlayerHUD = strPlayerHUDJson;
			tmpPlayerInfo.InfoJSON = strPlayerInfoJson;
			tmpPlayerInfo.CharJSON = charJSON;
			tmpPlayerInfo.WeaponJSON = weaponJSON;
			tmpPlayerInfo.ChipJSON = chipJSON;
			tmpPlayerInfo.EquipJSON = equipJSON;
			tmpPlayerInfo.FinalStrikeJSON = finalStrikeJSON;
			tmpPlayerInfo.TitleJSON = titleJSON;
			int[] rankData = new int[13]
			{
				0,
				ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(),
				socketPlayerHUD.m_Level,
				0,
				ManagedSingleton<MissionHelper>.Instance.GetActivityValue(MissionType.Achievement),
				0,
				0,
				0,
				ManagedSingleton<MissionHelper>.Instance.GetMissionCounter(300501),
				0,
				0,
				0,
				0
			};
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerRankInfos(rankData, ManagedSingleton<PlayerHelper>.Instance.GetLV(), OnGetBestWeaponID(false), 0));
			OnGetBestWeaponID(true);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatSetShield((!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community) ? 1 : 0));
			Screen.sleepTimeout = (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep ? (-2) : (-1));
		}
	}

	public List<int> OnGetSignData()
	{
		List<int> list = new List<int>();
		Dictionary<int, CUSTOMIZE_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.CUSTOMIZE_TABLE_DICT.Where((KeyValuePair<int, CUSTOMIZE_TABLE> q) => q.Value.n_TYPE == 1).ToDictionary((KeyValuePair<int, CUSTOMIZE_TABLE> q) => q.Value.n_ID, (KeyValuePair<int, CUSTOMIZE_TABLE> q) => q.Value).GetEnumerator();
		while (enumerator.MoveNext())
		{
			CUSTOMIZE_TABLE value = enumerator.Current.Value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_GET_VALUE1) && !list.Contains(value.n_ID))
			{
				list.Add(value.n_ID);
			}
		}
		return list;
	}

	public int OnGetBestWeaponID(bool bUpdateServer)
	{
		Dictionary<int, WeaponInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.GetEnumerator();
		int num = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		int num2 = GetWeaponPower(num);
		while (enumerator.MoveNext())
		{
			WeaponInfo value = enumerator.Current.Value;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(value.netInfo.WeaponID))
			{
				int weaponPower = GetWeaponPower(value.netInfo.WeaponID);
				if (weaponPower > num2)
				{
					num2 = weaponPower;
					num = value.netInfo.WeaponID;
				}
			}
		}
		if (bUpdateServer)
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerRankInfo(3, ManagedSingleton<PlayerHelper>.Instance.GetLV(), num2, num));
		}
		return num;
	}

	public void OnGetPlayerContactList()
	{
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerContactList());
	}

	public void OnSetPlayerContact(NTEveryoneJoinedLockedRoom rs)
	{
		if (rs.PlayeridLength <= 0)
		{
			return;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < rs.PlayeridLength; i++)
		{
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify != rs.Playerid(i))
			{
				list.Add(rs.Playerid(i));
			}
		}
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerContact(list.ToArray()));
		Invoke("OnGetPlayerContactList", 1.2f);
	}

	public void OnSetPlayerContactByPlayerID(string pid)
	{
		List<string> list = new List<string>();
		if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify != pid)
		{
			list.Add(pid);
		}
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQSetPlayerContact(list.ToArray()));
		Invoke("OnGetPlayerContactList", 1.2f);
	}

	public void OnSendBattleInvite(string inviteId, string host, int port, string roomId, int stageId, int capacity, bool isFriendBattle = false)
	{
		string text = JsonHelper.Serialize(new BattleInviteInfo
		{
			InviterName = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname,
			Host = host,
			Port = port,
			RoomId = roomId,
			StageId = stageId,
			Capacity = capacity
		});
		text = text.Replace("@", "\\u0040");
		if (isFriendBattle)
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendBattleInvite(inviteId, text, 1));
		}
		else
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQBattleInvite(inviteId, text, 1));
		}
	}

	public void SetPlayerIcon(Transform ob, int icon, Vector3 v3, bool bOwn, Callback cb = null)
	{
		int childCount = ob.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(ob.GetChild(i).gameObject);
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "PlayerIconBase", "PlayerIconBase", delegate(GameObject asset)
		{
			GameObject obj = UnityEngine.Object.Instantiate(asset, ob.transform);
			obj.GetComponent<PlayerIconBase>().Setup(icon, bOwn, cb);
			obj.transform.localScale = v3;
		});
	}
}
