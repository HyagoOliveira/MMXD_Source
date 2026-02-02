#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeSocket;
using UnityEngine;
using cm;
using enums;

internal class OrangeMatchManager : MonoBehaviourSingleton<OrangeMatchManager>
{
	public enum ConnectingState
	{
		Disconnect = 0,
		Connecting = 1,
		Connected = 2
	}

	public class NetCBData
	{
        [Obsolete]
        public CallbackObj tCB;
	}

	public class NetCBDataObj
	{
		public NetCBData tNCBD;

		public object res;
	}

	private ConnectingState connectingState;

	public Action OnDisconnectEvent;

	private bool assignHandler;

	public bool bIsPublic;

	public string sCondition = "";

	public string sRoomName = "";

	public NetCBData OnRSSetPreparedOrNotCB = new NetCBData();

	public NetCBData OnNTJoinPrepareRoomCB = new NetCBData();

	public NetCBData OnNTLeavePrepareRoomCB = new NetCBData();

	public NetCBData OnNTPVEPrepareRoomStartCB = new NetCBData();

	public NetCBData OnNTPrepareRoomOwnerChangeCB = new NetCBData();

	public NetCBData OnRSChangePreparedSettingCB = new NetCBData();

	public NetCBData OnNTPVPFriendRoomStartCB = new NetCBData();

	public NetCBData OnNTInviteCodeChangeCB = new NetCBData();

	public NetCBData OnNTRoomOwnerCB = new NetCBData();

	public NetCBData OnRSPVPInviteCodeMatchCB = new NetCBData();

	public NetCBData OnNTChangeRoomSettingCB = new NetCBData();

	private List<NetCBDataObj> listNetCBDatas = new List<NetCBDataObj>();

	private Coroutine tCRSNDC;

	public List<MemberInfo> ListMemberInfo = new List<MemberInfo>();

	public bool Disconnect
	{
		get
		{
			return connectingState == ConnectingState.Disconnect;
		}
	}

	public string Host { get; set; }

	public int Port { get; set; }

	public bool SingleMatch { get; set; }

	public string Region { get; private set; }

	public short Capacity { get; private set; }

	public string SelfSealedBattleSetting { get; set; }

	public STAGE_TABLE SelectStageData { get; set; }

	public PVPGameType PvpGameType { get; set; }

	public PVPMatchType PvpMatchType { get; set; }

	public PVPMatchType LastRqPvpMatchType { get; set; }

	public int StageID { get; set; }

	public void CheatSetRegion(string region)
	{
		Region = region.ToUpper();
	}

	private void AddAllHandler()
	{
		if (!assignHandler)
		{
			assignHandler = true;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSSetPreparedOrNot, OnRSSetPreparedOrNot);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTJoinPrepareRoom, OnNTJoinPrepareRoom);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTLeavePrepareRoom, OnNTLeavePrepareRoom);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTPVEPrepareRoomStart, OnNTPVEPrepareRoomStart);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTPrepareRoomOwnerChange, OnNTPrepareRoomOwnerChange);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTChangePrepareSetting, OnRSChangePreparedSetting);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSChangePrepareSetting, OnRSChangePreparedSetting);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTSetPreparedOrNot, OnNTSetPreparedOrNot);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTPVPPersonnelMatched, OnNTPVPPersonnelMatched, 0, true);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTPVPFriendRoomStart, OnNTPVPFriendRoomStart);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTInviteCodeChange, OnNTInviteCodeChange);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTRoomOwner, OnNTRoomOwner);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPInviteCodeMatch, OnRSPVPInviteCodeMatch);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.NTChangeRoomSetting, OnNTChangeRoomSetting);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSChangeRoomSetting, OnNTChangeRoomSetting);
		}
	}

	private void RemoveAllHandler()
	{
		if (assignHandler)
		{
			assignHandler = false;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSSetPreparedOrNot, OnRSSetPreparedOrNot);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTJoinPrepareRoom, OnNTJoinPrepareRoom);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTLeavePrepareRoom, OnNTLeavePrepareRoom);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTPVEPrepareRoomStart, OnNTPVEPrepareRoomStart);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTPrepareRoomOwnerChange, OnNTPrepareRoomOwnerChange);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTChangePrepareSetting, OnRSChangePreparedSetting);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSChangePrepareSetting, OnRSChangePreparedSetting);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTSetPreparedOrNot, OnNTSetPreparedOrNot);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTPVPFriendRoomStart, OnNTPVPFriendRoomStart);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTInviteCodeChange, OnNTInviteCodeChange);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTRoomOwner, OnNTRoomOwner);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSPVPInviteCodeMatch, OnRSPVPInviteCodeMatch);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.NTChangeRoomSetting, OnNTChangeRoomSetting);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSChangeRoomSetting, OnNTChangeRoomSetting);
		}
	}

	private IEnumerator CheckReSentNetDataCoroutine()
	{
		while (listNetCBDatas.Count > 0)
		{
			if (listNetCBDatas[0].tNCBD.tCB != null)
			{
				listNetCBDatas[0].tNCBD.tCB(listNetCBDatas[0].res);
				listNetCBDatas.RemoveAt(0);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		tCRSNDC = null;
	}

	private void WaitUIToReSent(NetCBData tNCB, object res)
	{
		NetCBDataObj item = new NetCBDataObj
		{
			tNCBD = tNCB,
			res = res
		};
		listNetCBDatas.Add(item);
		if (tCRSNDC == null)
		{
			tCRSNDC = StartCoroutine(CheckReSentNetDataCoroutine());
		}
	}

	private void OnRSSetPreparedOrNot(object res)
	{
		if (OnRSSetPreparedOrNotCB.tCB != null)
		{
			OnRSSetPreparedOrNotCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnRSSetPreparedOrNotCB, res);
		}
	}

	private void OnNTJoinPrepareRoom(object res)
	{
		if (OnNTJoinPrepareRoomCB.tCB != null)
		{
			OnNTJoinPrepareRoomCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnNTJoinPrepareRoomCB, res);
		}
	}

	private void OnNTLeavePrepareRoom(object res)
	{
		if (OnNTLeavePrepareRoomCB.tCB != null)
		{
			OnNTLeavePrepareRoomCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnNTLeavePrepareRoomCB, res);
		}
	}

	private void OnNTPVEPrepareRoomStart(object res)
	{
		if (OnNTPVEPrepareRoomStartCB.tCB != null)
		{
			OnNTPVEPrepareRoomStartCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnNTPVEPrepareRoomStartCB, res);
		}
	}

	private void OnNTPrepareRoomOwnerChange(object res)
	{
		if (OnNTPrepareRoomOwnerChangeCB.tCB != null)
		{
			OnNTPrepareRoomOwnerChangeCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnNTPrepareRoomOwnerChangeCB, res);
		}
	}

	private void OnNTPVPFriendRoomStart(object res)
	{
		if (OnNTPVPFriendRoomStartCB.tCB != null)
		{
			OnNTPVPFriendRoomStartCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnNTPVPFriendRoomStartCB, res);
		}
	}

	private void OnNTChangeRoomSetting(object res)
	{
		if (OnNTChangeRoomSettingCB.tCB != null)
		{
			OnNTChangeRoomSettingCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnNTChangeRoomSettingCB, res);
		}
	}

	private void OnRSChangePreparedSetting(object res)
	{
		if (OnRSChangePreparedSettingCB.tCB != null)
		{
			OnRSChangePreparedSettingCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnRSChangePreparedSettingCB, res);
		}
	}

	private void OnNTSetPreparedOrNot(object res)
	{
		object obj;
		if (!((obj = res) is NTSetPreparedOrNot))
		{
			return;
		}
		NTSetPreparedOrNot rs = (NTSetPreparedOrNot)obj;
		if (ListMemberInfo.Count == 0)
		{
			StartCoroutine(WaitListMemberCoroutine(rs));
			return;
		}
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (ListMemberInfo[i].PlayerId == rs.Playerid)
			{
				ListMemberInfo[i].bPrepared = rs.Prepared;
			}
		}
	}

	private void OnNTInviteCodeChange(object res)
	{
		if (OnNTInviteCodeChangeCB.tCB != null)
		{
			OnNTInviteCodeChangeCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnNTInviteCodeChangeCB, res);
		}
	}

	private void OnNTRoomOwner(object res)
	{
		if (OnNTRoomOwnerCB.tCB != null)
		{
			OnNTRoomOwnerCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnNTRoomOwnerCB, res);
		}
	}

	private void OnRSPVPInviteCodeMatch(object res)
	{
		if (OnRSPVPInviteCodeMatchCB.tCB != null)
		{
			OnRSPVPInviteCodeMatchCB.tCB(res);
		}
		else
		{
			WaitUIToReSent(OnRSPVPInviteCodeMatchCB, res);
		}
	}

	private IEnumerator WaitListMemberCoroutine(NTSetPreparedOrNot rs)
	{
		while (ListMemberInfo.Count == 0)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (ListMemberInfo[i].PlayerId == rs.Playerid)
			{
				ListMemberInfo[i].bPrepared = rs.Prepared;
			}
		}
	}

	private void OnNTPVPPersonnelMatched(object res)
	{
		object obj;
		if (!((obj = res) is NTPVPPersonnelMatched))
		{
			return;
		}
		NTPVPPersonnelMatched nTPVPPersonnelMatched = (NTPVPPersonnelMatched)obj;
		int pvptier = 0;
		if (PvpGameType == PVPGameType.OneVSOneSeason)
		{
			pvptier = ManagedSingleton<PlayerHelper>.Instance.GetSeasonTier();
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ReadyToGoPVP(pvptier, PvpMatchType, nTPVPPersonnelMatched.Ip, nTPVPPersonnelMatched.Port, nTPVPPersonnelMatched.Roomid, delegate
		{
			PvpMatchUI uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpMatchUI>("UI_PvpMatch");
			if (uI2 != null)
			{
				uI2.matchSucess = false;
				uI2.OnClickCloseBtn();
			}
		});
		PvpMatchUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpMatchUI>("UI_PvpMatch");
		if (uI != null)
		{
			uI.matchSucess = true;
			uI.RemoveHandler();
		}
	}

	public void MatchServerLogin(Callback p_cb, bool autoSealBattleSetting = true)
	{
		AddAllHandler();
		switch (connectingState)
		{
		case ConnectingState.Connected:
			if (autoSealBattleSetting)
			{
				ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
				{
					SelfSealedBattleSetting = setting;
					p_cb.CheckTargetToInvoke();
				});
			}
			else
			{
				p_cb.CheckTargetToInvoke();
			}
			return;
		case ConnectingState.Connecting:
			Debug.LogWarning("Match Server連線中... 等待時不應再次連線");
			return;
		}
		if (string.IsNullOrEmpty(Region))
		{
			MonoBehaviourSingleton<LocateManager>.Instance.FindLocate(delegate(object p_param)
			{
				if (p_param is AREA_TABLE)
				{
					Region = (p_param as AREA_TABLE).s_REGION.ToUpper();
					connectingState = ConnectingState.Connecting;
					ConnectedToMatchServer(p_cb, autoSealBattleSetting);
				}
			}, LocateManager.LocaleTarget.BelongPVPRegion);
		}
		else
		{
			connectingState = ConnectingState.Connecting;
			ConnectedToMatchServer(p_cb, autoSealBattleSetting);
		}
	}

	private void ConnectedToMatchServer(Callback p_cb, bool autoSealBattleSetting = true)
	{
		MonoBehaviourSingleton<CMSocketClient>.Instance.ConnectToServer(Host, Port, delegate(bool connected)
		{
			if (connected)
			{
				connectingState = ConnectingState.Connected;
				MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSMatchLogin, delegate(object res)
				{
					object obj;
					if ((obj = res) is RSMatchLogin)
					{
						RSMatchLogin rSMatchLogin = (RSMatchLogin)obj;
						if (rSMatchLogin.Result != 60000)
						{
							MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rSMatchLogin.Result);
						}
						else
						{
							Debug.Log(string.Format("Match Server is connected! 分配到的位址:{0}:{1}", rSMatchLogin.Ip, rSMatchLogin.Port));
							Host = rSMatchLogin.Ip;
							Port = rSMatchLogin.Port;
							if (autoSealBattleSetting)
							{
								ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
								{
									SelfSealedBattleSetting = setting;
									p_cb.CheckTargetToInvoke();
								});
							}
							else
							{
								p_cb.CheckTargetToInvoke();
							}
						}
					}
				}, 0, true);
				MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQMatchLogin(SocketCommon.ProtocolVersion, MonoBehaviourSingleton<GameServerService>.Instance.ServiceToken, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname, Region));
			}
			else
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg("NETWORK_NOT_REACHABLE_DESC_2", delegate
				{
					ConnectedToMatchServer(p_cb, autoSealBattleSetting);
				}, delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_STOP);
					MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(false);
				});
			}
		}, DisconnectCallback, SocketIOErrorCallback);
	}

	public void MatchServerLogout()
	{
		MonoBehaviourSingleton<CMSocketClient>.Instance.Disconnect();
		connectingState = ConnectingState.Disconnect;
	}

	private void DisconnectCallback()
	{
		Debug.Log("[Match] DisconnectCallback");
		connectingState = ConnectingState.Disconnect;
		ListMemberInfo.Clear();
		Action onDisconnectEvent = OnDisconnectEvent;
		if (onDisconnectEvent != null)
		{
			onDisconnectEvent();
		}
		OnDisconnectEvent = null;
		RemoveAllHandler();
	}

	private void SocketIOErrorCallback(bool connected)
	{
		Debug.Log("[Match] SocketIOErrorCallback");
		connectingState = ConnectingState.Disconnect;
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ACCESS01_STOP);
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
			ui.SetupConfirmByKey("COMMON_TIP", "NETWORK_SOCKET_IO_ERROR", "COMMON_OK", delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
				MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(false);
			});
		});
	}

	public void CraeteCoopRoom(STAGE_TABLE p_stageData, bool p_isPublic, string p_condition, string p_roomName, short p_capacity, Action<object> handler)
	{
		StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
		if (SelfSealedBattleSetting == null)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIP_WAITFORMATCH"), 42);
			return;
		}
		SelectStageData = p_stageData;
		if (ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(SelectStageData, ref condition))
		{
			Capacity = p_capacity;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSCreatePVEPrepareRoom, handler, 0, true);
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQCreatePVEPrepareRoom((short)p_stageData.n_TYPE, p_stageData.n_ID, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname, p_isPublic, p_condition, p_roomName, p_capacity, SelfSealedBattleSetting));
			bIsPublic = p_isPublic;
			sCondition = p_condition;
			sRoomName = p_roomName;
		}
		else
		{
			ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(SelectStageData, condition);
		}
	}

	public void OnCreateRoomMainUI(object res, STAGE_TABLE stageTable, Callback roomRefreshCB)
	{
		object obj;
		if (!((obj = res) is RSCreatePVEPrepareRoom))
		{
			return;
		}
		RSCreatePVEPrepareRoom rs = (RSCreatePVEPrepareRoom)obj;
		if (rs.Result != 61000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rs.Result, false);
			roomRefreshCB.CheckTargetToInvoke();
			return;
		}
		NetSealBattleSettingInfo setting = null;
		if (!ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(rs.Unsealedbattlesetting, out setting))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(Code.MATCH_CREATEROOM_FAIL, false);
			return;
		}
		Host = rs.Ip;
		Port = rs.Port;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CoopRoomMain", delegate(CoopRoomMainUI ui)
		{
			ui.StageTable = stageTable;
			ui.IsRoomMaster = true;
			ui.RoomId = rs.Roomid;
			ui.RoomRefreshCB = roomRefreshCB;
			ui.Setup(setting);
		});
	}

	public void CreatePVPPrepareRoom(PVPMatchType matchType, int stageId, int tier, bool isPublic, string roomName, bool bIsContinue, Action<object> handler)
	{
		Capacity = 2;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSCreatePVPPrepareRoom, handler, 0, true);
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQCreatePVPPrepareRoom(tier, (int)matchType, stageId, SelfSealedBattleSetting, isPublic, roomName, bIsContinue));
	}

	public void PVPContinueRoomFind(string playerid, Action<object> handler)
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVPContinueRoomFind, handler, 0, true);
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPContinueRoomFind(playerid));
	}

	public void SearchCoopRoomList(STAGE_TABLE p_stageData, Action<object> handler)
	{
		if (connectingState == ConnectingState.Connected)
		{
			SelectStageData = p_stageData;
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVEPrepareRoomList((short)p_stageData.n_TYPE, p_stageData.n_ID));
		}
	}

	public void SearchCoopRoomListV2(List<short> listType, List<int> listStageID)
	{
		if (connectingState == ConnectingState.Connected)
		{
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVEPrepareRoomListV2(listType.ToArray(), listStageID.ToArray()));
		}
	}

	public void SearchFriendPVPRoomList(STAGE_TABLE p_stageData, int pvpTier, int pvpType, Action<object> handler)
	{
		if (connectingState == ConnectingState.Connected)
		{
			SelectStageData = p_stageData;
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPPrepareRoomList(pvpTier, pvpType, StageID));
		}
	}

	public void SearchFriendPVPTargetRoomList(STAGE_TABLE p_stageData, int pvpTier, int pvpType, string[] targetIDList, Action<object> handler)
	{
		if (connectingState == ConnectingState.Connected)
		{
			SelectStageData = p_stageData;
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQPVPPrepareTargetRoomList(pvpTier, pvpType, StageID, targetIDList));
		}
	}

	public void JoinRoom(string host, int port, string roomId, int capacity)
	{
		StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
		if (ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(SelectStageData, ref condition))
		{
			Capacity = (short)capacity;
			if (Host != host || Port != port)
			{
				MatchServerLogout();
				Host = host;
				Port = port;
				MatchServerLogin(delegate
				{
					JoinRoom(host, port, roomId, capacity);
				});
				return;
			}
			if (connectingState == ConnectingState.Disconnect)
			{
				MatchServerLogin(delegate
				{
					Debug.Log("JoinRoom reconnect MatchServer.");
				});
			}
			MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQJoinPrepareRoom(roomId, SelfSealedBattleSetting));
		}
		else
		{
			ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(SelectStageData, condition);
		}
	}

	public void JoinRoomFriendBattle(string host, int port, string roomId, int capacity)
	{
		if (SelectStageData == null)
		{
			Debug.LogWarning("JoinRoomFriendBattle: SelectStageData should not equal to null.");
		}
		StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
		if (ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(SelectStageData, ref condition))
		{
			Capacity = (short)capacity;
			if (Host != host || Port != port)
			{
				Debug.Log("JoinRoomFriendBattle, changing server to match with host.");
				MatchServerLogout();
				Host = host;
				Port = port;
				MatchServerLogin(delegate
				{
					JoinRoomFriendBattle(host, port, roomId, capacity);
				});
			}
			else if (connectingState == ConnectingState.Disconnect)
			{
				MatchServerLogin(delegate
				{
					Debug.Log("JoinRoom reconnect MatchServer.");
					MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQJoinPrepareRoom(roomId, SelfSealedBattleSetting));
				});
			}
			else
			{
				MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQJoinPrepareRoom(roomId, SelfSealedBattleSetting));
			}
		}
		else
		{
			ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(SelectStageData, condition);
		}
	}

	public void PVPRandomMatching(Callback closecb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpMatch", delegate(PvpMatchUI ui)
		{
			if (closecb != null)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, closecb);
			}
			ui.Init();
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null, 0f);
		});
	}

	private void KickRoomMember(string playerid, Action<object> handler)
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSKickRoomMember, handler, 0, true);
		MonoBehaviourSingleton<CMSocketClient>.Instance.SendProtocol(FlatBufferCMHelper.CreateRQKickRoomMember(playerid));
	}

	public NetSealBattleSettingInfo GetSelfNetSealBattleSettingInfo()
	{
		new NetSealBattleSettingInfo();
		List<int> list = new List<int>();
		list.Add(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara);
		int mainWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		int subWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
		return ManagedSingleton<PlayerNetManager>.Instance.GetNetSealBattleSettingInfo(list, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.GetControllerSetting(), mainWeaponID, subWeaponID);
	}

	public NetSealBattleSettingInfo GetRandomNetSealBattleSettingInfo()
	{
		new NetSealBattleSettingInfo();
		List<int> list = new List<int>();
		List<int> list2 = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.Keys.Where((int x) => x < 800000).ToList();
		int item = list2[OrangeBattleUtility.Random(1, list2.Count)];
		list.Add(item);
		list2 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.Keys.Where((int x) => x >= 100001 && x < 105901).ToList();
		int nMainWeaponID = list2[OrangeBattleUtility.Random(0, list2.Count)];
		list2 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.Keys.Where((int x) => x >= 100001 && x < 105901 && x != nMainWeaponID).ToList();
		int nSubWeaponID = list2[OrangeBattleUtility.Random(0, list2.Count)];
		return ManagedSingleton<PlayerNetManager>.Instance.GetNetSealBattleSettingInfo(list, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.GetControllerSetting(), nMainWeaponID, nSubWeaponID, true);
	}
}
