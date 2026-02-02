#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CallbackDefs;
using Coffee.UIExtensions;
using Newtonsoft.Json;
using OrangeApi;
using OrangeSocket;
using StageLib;
using UnityEngine;
using cb;
using cc;
using enums;

internal class OrangeBattleServerManager : MonoBehaviourSingleton<OrangeBattleServerManager>
{
	private enum BattleType
	{
		NONE = 0,
		PVE = 1,
		PVP = 2,
		CAMPAIGN = 3
	}

	public enum BattleSyncData
	{
		PLAYER_INPUT = 1,
		PLAYER_DIE = 2,
		PLAYER_CONTROLLER = 3
	}

	public class TowerInfo
	{
		public int nStageID;

		public int[] nBossID = new int[2];

		public int[] nBossHP = new int[2];
	}

	public class SeasonCharaterInfo
	{
		public int m_MHP;

		public int m_HP;

		public double m_Percent;
	}

	private class PvpIDPosition
	{
		public int nID;

		public Vector3 vPos;

		public bool bLookBack;
	}

	public class ReturnGameData
	{
		public bool bIsPvP;

		public bool bIsPvE;

		public string sIP;

		public int nPort;

		public string sRoomID;

		public short nStageType;

		public int nPvPtier;

		public PVPMatchType ePvPType;

		public int nStageID;

		public string SelfSealedBattleSetting;

		public double dSaveTime;

		public bool bCMode;
	}

	private class GoStageData
	{
		public BattleType battleType;

		public int stageId;

		public GoStageData(BattleType battleType, int stageId)
		{
			this.battleType = battleType;
			this.stageId = stageId;
		}
	}

	public List<MemberInfo> ListMemberInfo = new List<MemberInfo>();

	private string _sHostPlayerID = "";

	public string strRoomID = "";

	public int CurrentChallengeTab;

	public int nCurrentTotalWarType;

	public int CurrentNormalDuration = int.MaxValue;

	public int CurrentChallengeDuration = int.MaxValue;

	public int NowSpeedMode = 1;

	public bool SpeedShowUpdateRoot;

	public bool bTowerBase;

	public int Fatigued;

	public STAGE_RULE_TABLE TowerStageRuleTbl;

	private int[] FATIGUED_ABILITY_CORRECTION = new int[4]
	{
		OrangeConst.FATIGUE_POWER_1,
		OrangeConst.FATIGUE_POWER_2,
		OrangeConst.FATIGUE_POWER_3,
		100
	};

	public int CurrentFatiguePower = 100;

	public int CurrentMaxFloor;

	public int CurrentDifficultyType;

	public bool CurrentStageClear;

	public TowerInfo CurrentTowerInfo = new TowerInfo();

	public HUNTERRANK_TABLE CurrentHunterRankTable;

	public HUNTERRANK_TABLE MatchHunterRankTable;

	public int CurrentScore;

	public bool bSeasonHPInit;

	public bool bSeasonBase;

	public int nMainWeaponID;

	public int nSubWeaponID;

	public bool bSeasonCmdFlag;

	public int nWinType;

	public bool bSeasonExpired;

	public int SeasonKillCount;

	public int SeasonBeKilledCount;

	public List<SeasonBattleInfoReq> listCmdSeasonBattleInfoReq = new List<SeasonBattleInfoReq>();

	public Dictionary<int, CharacterColumeSmall.PerCharacterSmallCell> idcSeasonIconFlag = new Dictionary<int, CharacterColumeSmall.PerCharacterSmallCell>();

	public List<int> SeasonCharaterList = new List<int>();

	public List<Dictionary<int, SeasonCharaterInfo>> idcSeasonCharaterInfoList = new List<Dictionary<int, SeasonCharaterInfo>>();

	public bool bCurrentCoopChallengeMode;

	private List<PvpIDPosition> ListPvpIDPosition = new List<PvpIDPosition>();

	private GoStageData goStageData;

	public bool bIsReConnect;

	private bool bWaitConnect;

	private TargetMarkUI targetMarkUI;

	private List<string> listGeneratingPlayer = new List<string>();

	private List<EventManager.StageGeneratePlayer> listCheckGenerateParam = new List<EventManager.StageGeneratePlayer>();

	public ChargeDataScriptObj tChargeDataScriptObj;

	public StageUiDataScriptObj tStageUiDataScriptObj;

	private const double ReturnLastNetGameTimeOut = 600.0;

	private Callback LastCheckReturnCB;

	private int nCheckReturnTimes;

	private readonly int fbArrayOffset = 24;

	private readonly int fbSamplingTimes = 5;

	private Dictionary<string, List<InputInfo>> InputInfoBuff = new Dictionary<string, List<InputInfo>>();

	private Color32[] SeasonColor1 = new Color32[4]
	{
		new Color32(250, 223, 210, byte.MaxValue),
		new Color32(250, 223, 210, byte.MaxValue),
		new Color32(106, 223, 247, byte.MaxValue),
		new Color32(106, 223, 247, byte.MaxValue)
	};

	private Color32[] SeasonColor2 = new Color32[4]
	{
		new Color32(104, 42, 133, byte.MaxValue),
		new Color32(104, 42, 133, byte.MaxValue),
		new Color32(5, 104, 176, byte.MaxValue),
		new Color32(5, 104, 176, byte.MaxValue)
	};

	public string sHostPlayerID
	{
		get
		{
			return _sHostPlayerID;
		}
		set
		{
			_sHostPlayerID = value;
			foreach (MemberInfo item in ListMemberInfo)
			{
				if (item.PlayerId == _sHostPlayerID)
				{
					Debug.LogWarning("Host Set " + item.Nickname + "(" + _sHostPlayerID + ")");
					break;
				}
			}
		}
	}

	public bool IsMultiply
	{
		get
		{
			return goStageData != null;
		}
	}

	public bool IsPvp
	{
		get
		{
			if (goStageData == null)
			{
				return StageUpdate.gbRegisterPvpPlayer;
			}
			return goStageData.battleType == BattleType.PVP;
		}
	}

	public string GetHostPlayerName()
	{
		foreach (MemberInfo item in ListMemberInfo)
		{
			if (item.PlayerId == _sHostPlayerID)
			{
				return item.Nickname;
			}
		}
		return "無";
	}

	public List<NetTowerBossInfo> GetTowerBossInfoList()
	{
		List<NetTowerBossInfo> list = new List<NetTowerBossInfo>();
		for (int i = 0; i < CurrentTowerInfo.nBossID.Length; i++)
		{
			if (CurrentTowerInfo.nBossID[i] != 0)
			{
				NetTowerBossInfo item = new NetTowerBossInfo
				{
					TowerStageID = CurrentTowerInfo.nStageID,
					TowerBossID = CurrentTowerInfo.nBossID[i],
					DeductedHP = CurrentTowerInfo.nBossHP[i]
				};
				list.Add(item);
			}
		}
		return list;
	}

	public void UpdateTowerBossInfo(int EnemyID, int Hp)
	{
		for (int i = 0; i < CurrentTowerInfo.nBossID.Length; i++)
		{
			if (CurrentTowerInfo.nBossID[i] == EnemyID)
			{
				CurrentTowerInfo.nBossHP[i] = Hp;
			}
		}
	}

	public void SetCurrentTowerInfo(int nStageID, int[] bossIDs, int[] bossHPs)
	{
		CurrentTowerInfo.nStageID = nStageID;
		List<NetTowerBossInfo> list = new List<NetTowerBossInfo>();
		for (int i = 0; i < bossIDs.Length; i++)
		{
			int num = bossIDs[i];
			CurrentTowerInfo.nBossID[i] = bossIDs[i];
			CurrentTowerInfo.nBossHP[i] = bossHPs[i];
			NetTowerBossInfo netTowerBossInfo = new NetTowerBossInfo();
			netTowerBossInfo.TowerStageID = nStageID;
			netTowerBossInfo.TowerBossID = bossIDs[i];
			netTowerBossInfo.DeductedHP = bossHPs[i];
			list.Add(netTowerBossInfo);
		}
		if (!ManagedSingleton<PlayerNetManager>.Instance.mmapTowerBossInfoMap.ContainKey(nStageID))
		{
			for (int j = 0; j < list.Count; j++)
			{
				ManagedSingleton<PlayerNetManager>.Instance.mmapTowerBossInfoMap.Add(nStageID, list[j]);
			}
		}
	}

	public int PowerCorrection(int pow)
	{
		if (!bTowerBase)
		{
			return pow;
		}
		CurrentFatiguePower = FATIGUED_ABILITY_CORRECTION[Fatigued];
		return Convert.ToInt32((double)pow * ((double)CurrentFatiguePower / 100.0));
	}

	public float GetCurrentFatiguePower()
	{
		if (!bTowerBase)
		{
			return 1f;
		}
		int num = FATIGUED_ABILITY_CORRECTION[Fatigued];
		return (float)CurrentFatiguePower / 100f;
	}

	public bool CheckSeasonCharaterList()
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList.Count < 3)
		{
			return false;
		}
		for (int i = 0; i < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList.Count; i++)
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList[i] <= 0)
			{
				return false;
			}
		}
		return true;
	}

	private void OnCreateRSGetPlayerHUDCallback(object res)
	{
		if (!(res is RSGetPlayerHUD))
		{
			return;
		}
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSGetPlayerHUD, OnCreateRSGetPlayerHUDCallback);
		RSGetPlayerHUD rSGetPlayerHUD = (RSGetPlayerHUD)res;
		if (rSGetPlayerHUD.Result == 70300)
		{
			SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(rSGetPlayerHUD.PlayerHUD);
			if (socketPlayerHUD != null)
			{
				ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketPlayerHUD.m_PlayerId, socketPlayerHUD);
			}
		}
	}

	public void SetPlayerHUD(string PlayerID)
	{
		if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.ContainsKey(PlayerID))
		{
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerHUD, OnCreateRSGetPlayerHUDCallback);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUD(PlayerID));
		}
	}

	public void OnSetSeasonKillCount(int KillCount, int BeKilledCount)
	{
		SeasonKillCount = ((KillCount > SeasonKillCount) ? KillCount : SeasonKillCount);
		SeasonBeKilledCount = ((BeKilledCount > SeasonBeKilledCount) ? BeKilledCount : SeasonBeKilledCount);
	}

	public void OnInitSeasonCharaterMaxHPList()
	{
		nWinType = 0;
		bSeasonHPInit = false;
		bSeasonCmdFlag = true;
		SeasonKillCount = 0;
		SeasonBeKilledCount = 0;
		idcSeasonCharaterInfoList.Clear();
		listCmdSeasonBattleInfoReq.Clear();
		idcSeasonCharaterInfoList.Add(new Dictionary<int, SeasonCharaterInfo>());
		idcSeasonCharaterInfoList.Add(new Dictionary<int, SeasonCharaterInfo>());
	}

	public void OnSetSeasonCharaterMaxHP(int team, int cid, int mhp)
	{
		if (!idcSeasonCharaterInfoList[team].ContainsKey(cid))
		{
			SeasonCharaterInfo seasonCharaterInfo = new SeasonCharaterInfo();
			seasonCharaterInfo.m_MHP = mhp;
			seasonCharaterInfo.m_HP = mhp;
			seasonCharaterInfo.m_Percent = 0.0;
			idcSeasonCharaterInfoList[team].Add(cid, seasonCharaterInfo);
		}
	}

	public void OnSetSeasonCharaterHurtPercent(int team, int cid, double pct)
	{
		if (idcSeasonCharaterInfoList[team].ContainsKey(cid))
		{
			idcSeasonCharaterInfoList[team][cid].m_Percent += pct;
		}
	}

	public string SeasonGetEnemyPlayerID(string PlayerID)
	{
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (ListMemberInfo[i].PlayerId != PlayerID)
			{
				return ListMemberInfo[i].PlayerId;
			}
		}
		return null;
	}

	public void SeasonAddPlayerDMG(int team, string sPlayerID, int nDmg)
	{
		if (!bSeasonHPInit)
		{
			if (idcSeasonCharaterInfoList.Count == 0)
			{
				idcSeasonCharaterInfoList.Add(new Dictionary<int, SeasonCharaterInfo>());
				idcSeasonCharaterInfoList.Add(new Dictionary<int, SeasonCharaterInfo>());
			}
			for (int i = 0; i < ListMemberInfo.Count; i++)
			{
				for (int j = 0; j < ListMemberInfo[i].netSealBattleSettingInfo.CharacterList.Count; j++)
				{
					int characterID = ListMemberInfo[i].netSealBattleSettingInfo.CharacterList[j].CharacterID;
					OnSetSeasonCharaterMaxHP(i, characterID, 1000);
				}
			}
			bSeasonHPInit = true;
		}
		int index = ((team != 1) ? 1 : 0);
		OrangeCharacter playerByID = StageUpdate.GetPlayerByID(SeasonGetEnemyPlayerID(sPlayerID));
		if (null != playerByID)
		{
			int characterID2 = playerByID.CharacterID;
			idcSeasonCharaterInfoList[index][characterID2].m_MHP = playerByID.MaxHp;
			idcSeasonCharaterInfoList[index][characterID2].m_HP = playerByID.Hp;
			int characterID3 = ListMemberInfo[team].netSealBattleSettingInfo.CharacterList[ListMemberInfo[team].nNowCharacterID].CharacterID;
			double pct = (double)nDmg / (double)(int)playerByID.MaxHp * 100.0;
			OnSetSeasonCharaterHurtPercent(team, characterID3, pct);
		}
	}

	public bool IsPlayerID(string sID)
	{
		for (int num = ListMemberInfo.Count - 1; num >= 0; num--)
		{
			if (ListMemberInfo[num].PlayerId == sID)
			{
				return true;
			}
		}
		return false;
	}

	public void AddPlayerDMG(string sPlayerID, int nDmg, int nRealDmg, bool bPlayer = true)
	{
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if (sPlayerID == "" || (StageUpdate.gbRegisterPvpPlayer && !bPlayer) || stageUpdate == null || stageUpdate.IsEnd)
		{
			return;
		}
		int count = ListMemberInfo.Count;
		for (int i = 0; i < count; i++)
		{
			if (ListMemberInfo[i].PlayerId == sPlayerID)
			{
				ListMemberInfo[i].nALLDMG += nDmg;
				if (BattleInfoUI.Instance != null && BattleInfoUI.Instance.NowStageTable.n_MAIN == 90000 && BattleInfoUI.Instance.NowStageTable.n_SUB == 1 && bPlayer)
				{
					SeasonAddPlayerDMG(i, sPlayerID, nRealDmg);
				}
				return;
			}
		}
		OrangeCharacter playerByID = StageUpdate.GetPlayerByID(sPlayerID);
		if (!(playerByID == null))
		{
			MemberInfo memberInfo = new MemberInfo(sPlayerID, playerByID.sPlayerName, 0, new NetSealBattleSettingInfo());
			memberInfo.nALLDMG = nDmg;
			ListMemberInfo.Add(memberInfo);
		}
	}

	public void AddPlayerKillNum(string sPlayerID)
	{
		int count = ListMemberInfo.Count;
		for (int i = 0; i < count; i++)
		{
			if (ListMemberInfo[i].PlayerId == sPlayerID)
			{
				ListMemberInfo[i].nKillNum++;
				break;
			}
		}
	}

	public void AddPlayerKillEnemyNum(string sPlayerID)
	{
		int count = ListMemberInfo.Count;
		for (int i = 0; i < count; i++)
		{
			if (ListMemberInfo[i].PlayerId == sPlayerID)
			{
				ListMemberInfo[i].nKillEnemyNum++;
				return;
			}
		}
		OrangeCharacter playerByID = StageUpdate.GetPlayerByID(sPlayerID);
		if (!(playerByID == null))
		{
			MemberInfo memberInfo = new MemberInfo(sPlayerID, playerByID.sPlayerName, 0, new NetSealBattleSettingInfo());
			memberInfo.nKillEnemyNum = 1;
			ListMemberInfo.Add(memberInfo);
		}
	}

	public int GetMainPlayerKillEnemyNum()
	{
		int count = ListMemberInfo.Count;
		for (int i = 0; i < count; i++)
		{
			if (ListMemberInfo[i].PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				return ListMemberInfo[i].nKillEnemyNum;
			}
		}
		return 0;
	}

	public bool CheckSeasonKillEnemyNum()
	{
		int count = ListMemberInfo.Count;
		for (int i = 0; i < count; i++)
		{
			if (ListMemberInfo[i].nKillNum >= 3)
			{
				return true;
			}
		}
		if (SeasonKillCount >= 3 || SeasonBeKilledCount >= 3)
		{
			return true;
		}
		return false;
	}

	public void ResetCheckConnectTime(string sPlayerID)
	{
		for (int num = ListMemberInfo.Count - 1; num >= 0; num--)
		{
			if (ListMemberInfo[num].PlayerId == sPlayerID)
			{
				ListMemberInfo[num].fLastCheckConnectTime = 0f;
				break;
			}
		}
	}

	public bool UpdateCheckConnectTime(float fTimeEscape, float fTimeLimit, float fChangeToPauseTime = 10f)
	{
		bool result = false;
		for (int num = ListMemberInfo.Count - 1; num >= 0; num--)
		{
			ListMemberInfo[num].fLastCheckConnectTime += fTimeEscape;
			if (ListMemberInfo[num].bInGame && !ListMemberInfo[num].bInPause && ListMemberInfo[num].fLastCheckConnectTime >= fTimeLimit)
			{
				if (ListMemberInfo[num].fLastCheckConnectTime >= fTimeLimit + fChangeToPauseTime)
				{
					ListMemberInfo[num].bInPause = true;
				}
				result = true;
			}
		}
		return result;
	}

	public void SetPlayerInGame(string sPlayerID, bool bInGame)
	{
		int i = ListMemberInfo.Count - 1;
		while (i >= 0)
		{
			if (ListMemberInfo[i].PlayerId == sPlayerID)
			{
				if (ListMemberInfo[i].bInGame && !bInGame)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReport", delegate(PvpReportUI ui)
					{
						ui.SetMsg(ListMemberInfo[i].Nickname, "", "CORP_LEAVE_ROOM");
					});
				}
				ListMemberInfo[i].bInGame = bInGame;
				break;
			}
			int num = i - 1;
			i = num;
		}
	}

	public float GetCheckConnectTime(string sPlayerID)
	{
		for (int num = ListMemberInfo.Count - 1; num >= 0; num--)
		{
			if (ListMemberInfo[num].PlayerId == sPlayerID)
			{
				return ListMemberInfo[num].fLastCheckConnectTime;
			}
		}
		return 0f;
	}

	public void SetPlayerPause(string sPlayerID, bool bIsPause)
	{
		for (int num = ListMemberInfo.Count - 1; num >= 0; num--)
		{
			if (ListMemberInfo[num].PlayerId == sPlayerID)
			{
				ListMemberInfo[num].bInPause = bIsPause;
			}
		}
	}

	public bool CheckPlayerPause(string sPlayerID)
	{
		for (int num = ListMemberInfo.Count - 1; num >= 0; num--)
		{
			if (ListMemberInfo[num].PlayerId == sPlayerID)
			{
				return ListMemberInfo[num].bInPause;
			}
		}
		return false;
	}

	public bool CheckPlayerIsInGame(string sPlayerID)
	{
		for (int num = ListMemberInfo.Count - 1; num >= 0; num--)
		{
			if (ListMemberInfo[num].PlayerId == sPlayerID)
			{
				return ListMemberInfo[num].bInGame;
			}
		}
		return false;
	}

	public int GetPausePlayerCount()
	{
		int num = 0;
		for (int num2 = ListMemberInfo.Count - 1; num2 >= 0; num2--)
		{
			if (ListMemberInfo[num2].bInGame && ListMemberInfo[num2].bInPause)
			{
				num++;
			}
		}
		return num;
	}

	public int GetPlayingPlayerCount(bool bIgnoreSelf = false, bool bCheckOne = false)
	{
		int num = 0;
		if (bCheckOne && ListMemberInfo.Count == 1 && ListMemberInfo[0].PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			return 1;
		}
		for (int num2 = ListMemberInfo.Count - 1; num2 >= 0; num2--)
		{
			if (ListMemberInfo[num2].bInGame && !ListMemberInfo[num2].bInPause && (!bIgnoreSelf || MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify != ListMemberInfo[num2].PlayerId))
			{
				num++;
			}
		}
		return num;
	}

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageGeneratePlayer>(EventManager.ID.STAGE_GENERATE_PVE_PLAYER, GeneratePlayer);
		Singleton<GenericEventManager>.Instance.AttachEvent<Vector3, int, bool>(EventManager.ID.STAGE_REGISTER_PVP_SPAWNPOS, RegisterPvPSpawnPos);
	}

	protected override void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageGeneratePlayer>(EventManager.ID.STAGE_GENERATE_PVE_PLAYER, GeneratePlayer);
		Singleton<GenericEventManager>.Instance.DetachEvent<Vector3, int, bool>(EventManager.ID.STAGE_REGISTER_PVP_SPAWNPOS, RegisterPvPSpawnPos);
		base.OnDestroy();
	}

	public void GeneratePlayer(EventManager.StageGeneratePlayer tStageGeneratePlayer)
	{
		Vector3 vPos = tStageGeneratePlayer.vPos;
		switch (tStageGeneratePlayer.nMode)
		{
		case 0:
		{
			int num = GetMainPlayerTeam() - 1;
			listGeneratingPlayer.Clear();
			for (int j = 0; j < ListMemberInfo.Count; j++)
			{
				SpawnPlayer(ListMemberInfo[j], num == ListMemberInfo[j].Team, vPos, new Vector3(0.5f * (float)j, 0f, 0f));
			}
			break;
		}
		case 1:
		{
			listCheckGenerateParam.Add(tStageGeneratePlayer);
			if (CheckGeneratingPlayer(tStageGeneratePlayer.sPlayerID))
			{
				break;
			}
			int num = GetMainPlayerTeam() - 1;
			int i = 0;
			while (i < ListMemberInfo.Count)
			{
				if (tStageGeneratePlayer.sPlayerID == ListMemberInfo[i].PlayerId)
				{
					ListMemberInfo[i].nNowCharacterID = 0;
					while (ListMemberInfo[i].nNowCharacterID < ListMemberInfo[i].netSealBattleSettingInfo.CharacterList.Count && ListMemberInfo[i].netSealBattleSettingInfo.CharacterList[ListMemberInfo[i].nNowCharacterID].CharacterID != tStageGeneratePlayer.nCharacterID)
					{
						ListMemberInfo[i].nNowCharacterID++;
					}
					SpawnPlayer(ListMemberInfo[i], num == ListMemberInfo[i].Team, vPos, new Vector3(0f, 0f, 0f), tStageGeneratePlayer.bLookDir, delegate
					{
						if (ListMemberInfo[i].PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
						{
							EventManager.StageCameraFocus p_param = new EventManager.StageCameraFocus
							{
								bLock = true,
								bRightNow = true
							};
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, p_param);
						}
						StageUpdate stageUpdate = StageResManager.GetStageUpdate();
						if (stageUpdate != null)
						{
							StageUpdate.ReqChangeHost();
							if (StageUpdate.bIsHost)
							{
								Debug.LogError("net spawn player but is host");
								StageUpdate.bIsHost = false;
							}
							stageUpdate.SendReConnectMsg();
						}
					});
					break;
				}
				int num2 = i + 1;
				i = num2;
			}
			break;
		}
		case 2:
		{
			int num = GetMainPlayerTeam() - 1;
			if (ListMemberInfo.Count > tStageGeneratePlayer.nID)
			{
				SpawnPlayer(ListMemberInfo[tStageGeneratePlayer.nID], num == ListMemberInfo[tStageGeneratePlayer.nID].Team, vPos, new Vector3(0f, 0f, 0f));
			}
			break;
		}
		}
	}

	public bool HasPlayer(string sPlayerID)
	{
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (sPlayerID == ListMemberInfo[i].PlayerId)
			{
				return true;
			}
		}
		return false;
	}

	public int GetPlayerTeam(string sPlayerID)
	{
		int result = 1;
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (sPlayerID == ListMemberInfo[i].PlayerId)
			{
				result = ListMemberInfo[i].Team + 1;
				break;
			}
		}
		return result;
	}

	public int GetPlayerTeam(OrangeCharacter tOC)
	{
		return GetPlayerTeam(tOC.sPlayerID);
	}

	public int GetMainPlayerTeam()
	{
		return GetPlayerTeam(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
	}

	public string GetPlayerName(string sPlayerID)
	{
		string result = "";
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (sPlayerID == ListMemberInfo[i].PlayerId)
			{
				result = ListMemberInfo[i].Nickname;
				break;
			}
		}
		return result;
	}

	public int GetMainPlayerPower()
	{
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == ListMemberInfo[i].PlayerId)
			{
				return ListMemberInfo[i].nALLDMG;
			}
		}
		return 0;
	}

	public void SpawnPlayer(MemberInfo tMemberInfo, bool isTeamMember, Vector3 tPos, Vector3 tOffsetPos, bool bLookBack = false, Action<OrangeCharacter> endcb = null)
	{
		if (CheckGeneratingPlayer(tMemberInfo.PlayerId))
		{
			return;
		}
		AddGeneratingPlayer(tMemberInfo.PlayerId);
		GameObject obj = new GameObject();
		PlayerBuilder playerBuilder = obj.AddComponent<PlayerBuilder>();
		playerBuilder.CreateAtStart = false;
		bool isLocalPlayer = tMemberInfo.PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		if (tMemberInfo.netSealBattleSettingInfo.CharacterList.Count > tMemberInfo.nNowCharacterID)
		{
			playerBuilder.SetPBP.CharacterID = tMemberInfo.netSealBattleSettingInfo.CharacterList[tMemberInfo.nNowCharacterID].CharacterID;
			playerBuilder.SetPBP.CharacterSkinID = tMemberInfo.netSealBattleSettingInfo.CharacterList[tMemberInfo.nNowCharacterID].Skin;
		}
		else
		{
			playerBuilder.SetPBP.CharacterID = tMemberInfo.netSealBattleSettingInfo.CharacterList[tMemberInfo.netSealBattleSettingInfo.CharacterList.Count - 1].CharacterID;
			playerBuilder.SetPBP.CharacterSkinID = tMemberInfo.netSealBattleSettingInfo.CharacterList[tMemberInfo.netSealBattleSettingInfo.CharacterList.Count - 1].Skin;
		}
		playerBuilder.bShowStartEffect = true;
		playerBuilder.IsLocalPlayer = isLocalPlayer;
		playerBuilder.uid = tMemberInfo.PlayerId;
		playerBuilder.SetPBP.WeaponList[0] = tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo.WeaponID;
		playerBuilder.SetPBP.WeaponList[1] = tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo.WeaponID;
		playerBuilder.SetPBP.WeaponChipList[0] = tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo.Chip;
		playerBuilder.SetPBP.WeaponChipList[1] = tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo.Chip;
		obj.transform.position = tPos - tOffsetPos;
		obj.transform.localScale = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		playerBuilder.SetPBP.sPlayerID = tMemberInfo.PlayerId;
		playerBuilder.SetPBP.sPlayerName = tMemberInfo.Nickname;
		playerBuilder.SetPBP.netControllerSetting = tMemberInfo.netSealBattleSettingInfo.ControllerInfo;
		if (bLookBack)
		{
			playerBuilder.SetPBP.tSetCharacterDir = CharacterDirection.LEFT;
		}
		else
		{
			playerBuilder.SetPBP.tSetCharacterDir = CharacterDirection.RIGHT;
		}
		WeaponInfo weaponInfo = new WeaponInfo();
		WeaponStatus chipStatus = new WeaponStatus();
		foreach (NetChipInfo totalChip in tMemberInfo.netSealBattleSettingInfo.TotalChipList)
		{
			ChipInfo chipInfo = new ChipInfo();
			chipInfo.netChipInfo = totalChip;
			if (IsPvp)
			{
				chipInfo.netChipInfo.Exp = 0;
			}
			if (chipInfo.netChipInfo.ChipID == tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo.Chip || chipInfo.netChipInfo.ChipID == tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo.Chip)
			{
				chipStatus += ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(chipInfo, 0, false, false, null, true, tMemberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
			}
			else
			{
				chipStatus += ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(chipInfo, 0, false, false, null, false, tMemberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
			}
		}
		playerBuilder.SetPBP.chipStatus = chipStatus;
		weaponInfo.netInfo = tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo;
		if (IsPvp)
		{
			playerBuilder.SetPBP.mainWStatus = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(weaponInfo, -weaponInfo.netInfo.Exp, false, null, null, tMemberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		}
		else
		{
			foreach (NetWeaponExpertInfo weaponExpert in tMemberInfo.netSealBattleSettingInfo.WeaponExpertList)
			{
				weaponInfo.AddNetWeaponExpertInfo(weaponExpert);
			}
			playerBuilder.SetPBP.mainWStatus = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(weaponInfo, 0, false, null, null, tMemberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		}
		weaponInfo = new WeaponInfo();
		weaponInfo.netInfo = tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo;
		if (IsPvp)
		{
			playerBuilder.SetPBP.subWStatus = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(weaponInfo, -weaponInfo.netInfo.Exp, false, null, null, tMemberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		}
		else
		{
			foreach (NetWeaponExpertInfo weaponExpert2 in tMemberInfo.netSealBattleSettingInfo.WeaponExpertList)
			{
				weaponInfo.AddNetWeaponExpertInfo(weaponExpert2);
			}
			playerBuilder.SetPBP.subWStatus = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(weaponInfo, 0, false, null, null, tMemberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		}
		playerBuilder.SetPBP.tRefPassiveskill = new RefPassiveskill();
		foreach (NetWeaponSkillInfo weaponSkill in tMemberInfo.netSealBattleSettingInfo.WeaponSkillList)
		{
			if (IsPvp)
			{
				weaponSkill.Level = 1;
			}
			if (bCurrentCoopChallengeMode && ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
			{
				weaponSkill.Level = (byte)ManagedSingleton<StageHelper>.Instance.StatusCorrection(weaponSkill.Level, StageHelper.STAGE_RULE_STATUS.SKILL_LV);
			}
			if (weaponSkill.WeaponID == tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo.WeaponID)
			{
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(weaponSkill, 4, weaponSkill.Level);
			}
			else if (weaponSkill.WeaponID == tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo.WeaponID)
			{
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(weaponSkill, 8, weaponSkill.Level);
			}
		}
		foreach (NetWeaponDiVESkillInfo weaponDiVESkill in tMemberInfo.netSealBattleSettingInfo.WeaponDiVESkillList)
		{
			if (weaponDiVESkill.WeaponID == tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo.WeaponID)
			{
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(weaponDiVESkill.SkillID, 4);
			}
			else if (weaponDiVESkill.WeaponID == tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo.WeaponID)
			{
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(weaponDiVESkill.SkillID, 8);
			}
		}
		foreach (NetChipInfo totalChip2 in tMemberInfo.netSealBattleSettingInfo.TotalChipList)
		{
			if (totalChip2.ChipID == tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo.Chip)
			{
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(totalChip2, 4, 1, true);
			}
			if (totalChip2.ChipID == tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo.Chip)
			{
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(totalChip2, 8, 1, true);
			}
		}
		foreach (NetCharacterSkillInfo characterSkill in tMemberInfo.netSealBattleSettingInfo.CharacterSkillList)
		{
			if (characterSkill.CharacterID == playerBuilder.SetPBP.CharacterID)
			{
				if (IsPvp)
				{
					characterSkill.Level = 1;
				}
				if (characterSkill.Slot == 1)
				{
					playerBuilder.SetPBP.EnhanceEXIndex[0] = characterSkill.Extra;
				}
				else if (characterSkill.Slot == 2)
				{
					playerBuilder.SetPBP.EnhanceEXIndex[1] = characterSkill.Extra;
				}
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(characterSkill);
			}
		}
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[playerBuilder.SetPBP.CharacterID];
		playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL1);
		playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL2);
		playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL3);
		foreach (NetCharacterPassiveSkillInfo extraPassiveSkillInfo in tMemberInfo.netSealBattleSettingInfo.ExtraPassiveSkillInfoList)
		{
			if (extraPassiveSkillInfo.CharacterID == playerBuilder.SetPBP.CharacterID)
			{
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(extraPassiveSkillInfo.SkillID, 65535, extraPassiveSkillInfo.Level);
			}
		}
		List<int> charactertCardSkillList = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetCharactertCardSkillList(cHARACTER_TABLE.n_ID, tMemberInfo.netSealBattleSettingInfo.CardInfoList, tMemberInfo.netSealBattleSettingInfo.CharacterCardSlotInfoList);
		if (charactertCardSkillList != null)
		{
			for (int i = 0; i < charactertCardSkillList.Count; i++)
			{
				playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(charactertCardSkillList[i]);
			}
		}
		bool flag = false;
		if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0 && StageUpdate.gbGeneratePvePlayer && !StageUpdate.gbRegisterPvpPlayer)
		{
			flag = true;
		}
		if (!IsPvp && !flag && tMemberInfo.netSealBattleSettingInfo.EquipmentList.Count > 0)
		{
			Dictionary<int, SUIT_TABLE>.Enumerator enumerator7 = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.GetEnumerator();
			List<int> list = new List<int>();
			for (int j = 0; j < tMemberInfo.netSealBattleSettingInfo.EquipmentList.Count; j++)
			{
				EquipInfo equipInfo = new EquipInfo();
				equipInfo.netEquipmentInfo = tMemberInfo.netSealBattleSettingInfo.EquipmentList[j];
				list.Add(equipInfo.netEquipmentInfo.EquipItemID);
			}
			int num = 0;
			while (enumerator7.MoveNext())
			{
				num = 0;
				int[] array = new int[6]
				{
					enumerator7.Current.Value.n_EQUIP_1,
					enumerator7.Current.Value.n_EQUIP_2,
					enumerator7.Current.Value.n_EQUIP_3,
					enumerator7.Current.Value.n_EQUIP_4,
					enumerator7.Current.Value.n_EQUIP_5,
					enumerator7.Current.Value.n_EQUIP_6
				};
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					int[] array2 = array;
					for (int k = 0; k < array2.Length; k++)
					{
						if (array2[k] == list[num2])
						{
							num++;
						}
					}
				}
				if (num >= enumerator7.Current.Value.n_SUIT_1)
				{
					playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(enumerator7.Current.Value.n_EFFECT_1, 64);
				}
				if (num >= enumerator7.Current.Value.n_SUIT_2)
				{
					playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(enumerator7.Current.Value.n_EFFECT_2, 64);
				}
				if (num >= enumerator7.Current.Value.n_SUIT_3)
				{
					playerBuilder.SetPBP.tRefPassiveskill.AddPassivesSkill(enumerator7.Current.Value.n_EFFECT_3, 64);
				}
			}
		}
		playerBuilder.SetPBP.tRefPassiveskill.ReCalcuPassiveskillSelf();
		if (IsPvp)
		{
			playerBuilder.SetPBP.tPlayerStatus = ManagedSingleton<StatusHelper>.Instance.GetPlayerStatusX(1);
		}
		else
		{
			playerBuilder.SetPBP.tPlayerStatus = ManagedSingleton<StatusHelper>.Instance.GetPlayerStatusX(tMemberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		}
		if (!IsPvp)
		{
			playerBuilder.SetPBP.tPlayerStatus += ManagedSingleton<StatusHelper>.Instance.GetMemberEquipStatus(tMemberInfo);
			foreach (NetFinalStrikeInfo totalFS in tMemberInfo.netSealBattleSettingInfo.TotalFSList)
			{
				FinalStrikeInfo finalStrikeInfo = new FinalStrikeInfo();
				finalStrikeInfo.netFinalStrikeInfo = totalFS;
				playerBuilder.SetPBP.tPlayerStatus += ManagedSingleton<StatusHelper>.Instance.GetFinalStrikeStatusX(finalStrikeInfo);
				if (tMemberInfo.netSealBattleSettingInfo.PlayerInfo.MainWeaponFSID == totalFS.FinalStrikeID)
				{
					playerBuilder.SetPBP.FSkillList[0] = finalStrikeInfo;
				}
				if (tMemberInfo.netSealBattleSettingInfo.PlayerInfo.SubWeaponFSID == totalFS.FinalStrikeID)
				{
					playerBuilder.SetPBP.FSkillList[1] = finalStrikeInfo;
				}
			}
			playerBuilder.SetPBP.tPlayerStatus += ManagedSingleton<StatusHelper>.Instance.GetIllustrationStatus(tMemberInfo.netSealBattleSettingInfo);
			playerBuilder.SetPBP.tPlayerStatus += ManagedSingleton<StatusHelper>.Instance.GetBackupWeaponStatus(false, tMemberInfo.netSealBattleSettingInfo.BenchSlotInfoList, tMemberInfo.netSealBattleSettingInfo.BenchWeaponInfoList, tMemberInfo.netSealBattleSettingInfo.WeaponExpertList, tMemberInfo.netSealBattleSettingInfo.WeaponSkillList);
			playerBuilder.SetPBP.tPlayerStatus += ManagedSingleton<StatusHelper>.Instance.GetCardSystemStatus(false, tMemberInfo.netSealBattleSettingInfo.CharacterList[0].CharacterID, tMemberInfo.netSealBattleSettingInfo.CharacterList, tMemberInfo.netSealBattleSettingInfo.CardInfoList, tMemberInfo.netSealBattleSettingInfo.CharacterCardSlotInfoList);
			playerBuilder.SetPBP.tPlayerStatus += ManagedSingleton<StatusHelper>.Instance.GetSkinStatus(tMemberInfo.netSealBattleSettingInfo.TotalCharacterSkinList);
		}
		playerBuilder.CreatePlayer(delegate(object[] p_param2)
		{
			OrangeCharacter orangeCharacter = p_param2[0] as OrangeCharacter;
			if (isLocalPlayer)
			{
				orangeCharacter.gameObject.SetActive(true);
			}
			else
			{
				ManagedSingleton<InputStorage>.Instance.AddInputData(orangeCharacter.UserID.ToString());
				if (goStageData != null && goStageData.battleType == BattleType.PVP)
				{
					if (isTeamMember)
					{
						orangeCharacter.TargetMask = ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
					}
					else
					{
						orangeCharacter.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer;
						orangeCharacter.TargetMask = ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerUseMask;
					}
				}
				else
				{
					orangeCharacter.TargetMask = ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
				}
			}
			if (InputInfoBuff.ContainsKey(orangeCharacter.UserID) && orangeCharacter is OrangeNetCharacter)
			{
				OrangeNetCharacter orangeNetCharacter = orangeCharacter as OrangeNetCharacter;
				for (int l = 0; l < InputInfoBuff[orangeCharacter.UserID].Count; l++)
				{
					orangeNetCharacter.RecvNetworkRuntimeData(InputInfoBuff[orangeCharacter.UserID][l]);
				}
				InputInfoBuff.Remove(orangeCharacter.UserID);
			}
			if (endcb != null)
			{
				endcb(orangeCharacter);
			}
		});
		if (IsPvp && MonoBehaviourSingleton<OrangeMatchManager>.Instance.LastRqPvpMatchType <= PVPMatchType.FriendOneVSOne && targetMarkUI == null)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TargetMark", delegate(TargetMarkUI ui)
			{
				targetMarkUI = ui;
				targetMarkUI.ApplyLateUpdate();
			});
		}
	}

	public void AddGeneratingPlayer(string sPlayerID)
	{
		if (!listGeneratingPlayer.Contains(sPlayerID))
		{
			listGeneratingPlayer.Add(sPlayerID);
		}
	}

	public void RemoveGeneratingPlayer(string sPlayerID)
	{
		if (listGeneratingPlayer.Contains(sPlayerID))
		{
			listGeneratingPlayer.Remove(sPlayerID);
		}
		OrangeCharacter playerByID = StageUpdate.GetPlayerByID(sPlayerID);
		bool flag = false;
		EventManager.StageGeneratePlayer stageGeneratePlayer = null;
		for (int num = listCheckGenerateParam.Count - 1; num >= 0; num--)
		{
			if (!(listCheckGenerateParam[num].sPlayerID != sPlayerID))
			{
				if (listCheckGenerateParam[num].nCharacterID != 0 && listCheckGenerateParam[num].nCharacterID != playerByID.CharacterID)
				{
					flag = true;
					stageGeneratePlayer = listCheckGenerateParam[num];
					listCheckGenerateParam.RemoveAt(num);
					break;
				}
				playerByID.Controller.LogicPosition = new VInt3(listCheckGenerateParam[num].vPos);
				playerByID.transform.localPosition = listCheckGenerateParam[num].vPos;
				playerByID.WeaponCurrent = listCheckGenerateParam[num].WeaponCurrent;
				OrangeNetCharacter orangeNetCharacter = playerByID as OrangeNetCharacter;
				if (orangeNetCharacter != null)
				{
					orangeNetCharacter.ClearCommandQueue();
				}
				playerByID.selfBuffManager.nMeasureNow = listCheckGenerateParam[num].nMeasureNow;
				if (playerByID.tRefPassiveskill.bUsePassiveskill != listCheckGenerateParam[num].bUsePassiveskill)
				{
					playerByID.PlayerPressChip();
				}
				playerByID.WeaponCurrent = listCheckGenerateParam[num].WeaponCurrent;
				if (listCheckGenerateParam[num].nHP <= 0)
				{
					playerByID.Hp = 0;
					playerByID.RemovePlayerObjInfoBar();
					playerByID.NullHurtAction();
				}
				else if ((int)playerByID.MaxHp > listCheckGenerateParam[num].nHP)
				{
					playerByID.Hp = listCheckGenerateParam[num].nHP;
				}
				playerByID.UpdateHurtAction();
				playerByID.HealHp = listCheckGenerateParam[num].HealHp;
				playerByID.DmgHp = listCheckGenerateParam[num].DmgHp;
				break;
			}
		}
		for (int num2 = listCheckGenerateParam.Count - 1; num2 >= 0; num2--)
		{
			if (!(listCheckGenerateParam[num2].sPlayerID != sPlayerID))
			{
				listCheckGenerateParam.RemoveAt(num2);
			}
		}
		if (flag)
		{
			if (InputInfoBuff.ContainsKey(sPlayerID))
			{
				InputInfoBuff.Remove(sPlayerID);
			}
			playerByID.PlayerDead(true);
			if (stageGeneratePlayer != null)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_GENERATE_PVE_PLAYER, stageGeneratePlayer);
			}
		}
	}

	public bool CheckGeneratingPlayer(string sPlayerID)
	{
		if (listGeneratingPlayer.Contains(sPlayerID))
		{
			return true;
		}
		return false;
	}

	public void SetPvpGoStageData(int nStageID)
	{
		goStageData = new GoStageData(BattleType.PVP, nStageID);
	}

	public void RegisterPvPSpawnPos(Vector3 tPos, int tID, bool bLookBack)
	{
		PvpIDPosition pvpIDPosition = new PvpIDPosition();
		pvpIDPosition.nID = tID;
		pvpIDPosition.vPos = tPos;
		pvpIDPosition.bLookBack = bLookBack;
		ListPvpIDPosition.Add(pvpIDPosition);
		if (bIsReConnect || MonoBehaviourSingleton<OrangeMatchManager>.Instance.LastRqPvpMatchType > PVPMatchType.FriendOneVSOne || ListPvpIDPosition.Count != 2)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == ListMemberInfo[i].PlayerId)
			{
				num = ListMemberInfo[i].Team;
			}
		}
		for (int j = 0; j < ListMemberInfo.Count; j++)
		{
			int team = ListMemberInfo[j].Team;
			SpawnPlayer(ListMemberInfo[j], num == team, ListPvpIDPosition[team].vPos, new Vector3(0.5f * (float)j, 0f, 0f), ListPvpIDPosition[team].bLookBack);
		}
	}

	public void ChangeNextCharacter(string sPlayerID)
	{
		if (StageUpdate.GetPlayerByID(sPlayerID) != null || CheckGeneratingPlayer(sPlayerID))
		{
			return;
		}
		if (InputInfoBuff.ContainsKey(sPlayerID))
		{
			InputInfoBuff.Remove(sPlayerID);
		}
		MonoBehaviourSingleton<StageSyncManager>.Instance.bIgnoreReadyGo = true;
		int num = GetMainPlayerTeam() - 1;
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (!(ListMemberInfo[i].PlayerId == sPlayerID))
			{
				continue;
			}
			ListMemberInfo[i].nNowCharacterID++;
			int team = ListMemberInfo[i].Team;
			SpawnPlayer(ListMemberInfo[i], num == team, ListPvpIDPosition[team].vPos, new Vector3(0.5f * (float)i, 0f, 0f), ListPvpIDPosition[team].bLookBack, delegate(OrangeCharacter oc)
			{
				if (sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					EventManager.StageCameraFocus p_param = new EventManager.StageCameraFocus
					{
						bLock = true,
						bRightNow = true
					};
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, p_param);
					string lockRangeListStr = StageUpdate.GetLockRangeListStr(new List<string>());
					string text = "";
					if (lockRangeListStr.LastIndexOf(',') == -1)
					{
						text = lockRangeListStr;
						lockRangeListStr = "";
					}
					else
					{
						text = lockRangeListStr.Substring(lockRangeListStr.LastIndexOf(',') + 1);
						lockRangeListStr = lockRangeListStr.Substring(0, lockRangeListStr.LastIndexOf(','));
					}
					Vector3 position = oc.transform.position;
					StageResManager.GetStageUpdate().OnSyncStageObj(text, 3, sPlayerID + "," + position.x + "," + position.y + "," + position.z + "," + lockRangeListStr);
				}
			});
			break;
		}
	}

	public void RebornPlayer(OrangeCharacter oc)
	{
		if (oc.IsAlive())
		{
			return;
		}
		int index = 0;
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (ListMemberInfo[i].PlayerId == oc.sPlayerID)
			{
				index = ListMemberInfo[i].Team;
				break;
			}
		}
		Vector2 origin = ListPvpIDPosition[index].vPos;
		RaycastHit2D[] array = Physics2D.RaycastAll(origin, Vector2.down, float.PositiveInfinity, LayerMask.GetMask("Block", "SemiBlock"));
		float num = float.MaxValue;
		if (array != null)
		{
			RaycastHit2D[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				RaycastHit2D raycastHit2D = array2[j];
				if (raycastHit2D.distance < num)
				{
					origin = raycastHit2D.point;
					num = raycastHit2D.distance;
				}
			}
		}
		if (ListPvpIDPosition[index].bLookBack)
		{
			StageUpdate.SyncStageObj(3, 3, oc.sPlayerID + ",2,0," + origin.x + "," + origin.y + "," + ManagedSingleton<InputStorage>.Instance.GetInputRecordNO(oc.sPlayerID) + ",1", true);
		}
		else
		{
			StageUpdate.SyncStageObj(3, 3, oc.sPlayerID + ",2,0," + origin.x + "," + origin.y + "," + ManagedSingleton<InputStorage>.Instance.GetInputRecordNO(oc.sPlayerID) + ",0", true);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CONTINUE_PLATER, oc.sPlayerID, true, origin.x, origin.y, (bool?)ListPvpIDPosition[index].bLookBack);
	}

	public void CheckReturnLastNetGame(Callback cb)
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/scriptdatas", "StageUiData", delegate(StageUiDataScriptObj asset)
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj = asset;
			});
		}
		if (StageUpdate.nReConnectMode == 0)
		{
			cb.CheckTargetToInvoke();
			return;
		}
		if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLogin)
		{
			cb.CheckTargetToInvoke();
			return;
		}
		string recoveryNetGameData = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType = (PVPGameType)PlayerPrefs.GetInt("PvpGameType", 0);
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.LastRqPvpMatchType = (PVPMatchType)PlayerPrefs.GetInt("PvpMatchType", 0);
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList = JsonConvert.DeserializeObject<List<int>>(PlayerPrefs.GetString("SeasonCharaterList"));
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList == null)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList = new List<int>();
		}
		if (string.IsNullOrEmpty(recoveryNetGameData))
		{
			cb.CheckTargetToInvoke();
		}
		else
		{
			if (bWaitConnect)
			{
				return;
			}
			LastCheckReturnCB = cb;
			nCheckReturnTimes++;
			if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "StageTest")
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.OnInitSeasonCharaterMaxHPList();
			}
			ReturnGameData tReturnGameData = JsonConvert.DeserializeObject<ReturnGameData>(recoveryNetGameData);
			if ((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds - tReturnGameData.dSaveTime > 600.0)
			{
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
				cb.CheckTargetToInvoke();
				return;
			}
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID = tReturnGameData.nStageID;
			STAGE_TABLE value = null;
			if (!ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID, out value))
			{
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
				cb.CheckTargetToInvoke();
				return;
			}
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode = tReturnGameData.bCMode;
			bIsReConnect = true;
			bWaitConnect = true;
			if (!(tReturnGameData.sIP != "") || !(tReturnGameData.sRoomID != "") || tReturnGameData.nStageID == 0)
			{
				return;
			}
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = tReturnGameData.SelfSealedBattleSetting;
			if (tReturnGameData.bIsPvE)
			{
				ReadyToGoPVE(tReturnGameData.sIP, tReturnGameData.nPort, tReturnGameData.sRoomID, tReturnGameData.nStageType, tReturnGameData.nStageID, cb);
				return;
			}
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = tReturnGameData.ePvPType;
			int nSeasonID = 0;
			if (value.n_TYPE == 1000)
			{
				long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
				List<EVENT_TABLE> elist = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_PVPSEASON, serverUnixTimeNowUTC);
				if (elist == null || elist.Count == 0)
				{
					Debug.Log("SEASON FINISHED!!");
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
					cb.CheckTargetToInvoke();
				}
				else if (CurrentHunterRankTable == null)
				{
					ManagedSingleton<PlayerNetManager>.Instance.RetrieveSeasonInfoReq(delegate(RetrieveSeasonInfoRes res)
					{
						nSeasonID = elist[0].n_ID;
						List<NetSeasonInfo> seasonInfoList = res.SeasonInfoList;
						for (int i = 0; i < seasonInfoList.Count; i++)
						{
							if (seasonInfoList[i].SeasonID == nSeasonID)
							{
								List<HUNTERRANK_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT.Values.ToList();
								for (int j = 0; j < list.Count; j++)
								{
									if (seasonInfoList[i].Score <= list[j].n_PT_MAX)
									{
										CurrentHunterRankTable = list[j];
										break;
									}
								}
								break;
							}
						}
						if (CurrentHunterRankTable == null)
						{
							Debug.Log("SEASON DATA COULDN'T MATCH!!");
							MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
							cb.CheckTargetToInvoke();
						}
						else
						{
							ReadyToGoPVP(tReturnGameData.nPvPtier, tReturnGameData.ePvPType, tReturnGameData.sIP, tReturnGameData.nPort, tReturnGameData.sRoomID, delegate
							{
								cb.CheckTargetToInvoke();
							});
						}
					});
				}
				else
				{
					ReadyToGoPVP(tReturnGameData.nPvPtier, tReturnGameData.ePvPType, tReturnGameData.sIP, tReturnGameData.nPort, tReturnGameData.sRoomID, delegate
					{
						cb.CheckTargetToInvoke();
					});
				}
			}
			else
			{
				ReadyToGoPVP(tReturnGameData.nPvPtier, tReturnGameData.ePvPType, tReturnGameData.sIP, tReturnGameData.nPort, tReturnGameData.sRoomID, delegate
				{
					cb.CheckTargetToInvoke();
				});
			}
		}
	}

	private void BattleServerLogin(string ip, int port, Callback p_cb)
	{
		goStageData = null;
		MonoBehaviourSingleton<CBSocketClient>.Instance.ConnectToServer(ip, port, delegate(bool connected)
		{
			if (connected)
			{
				MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.RSBattleLogin, delegate(object res)
				{
					RSBattleLogin rSBattleLogin = (RSBattleLogin)res;
					if (rSBattleLogin.Result == 50000)
					{
						Debug.Log("Battle Server is connected!");
						p_cb.CheckTargetToInvoke();
						MonoBehaviourSingleton<CBSocketClient>.Instance.StartBeating();
					}
					else if (rSBattleLogin.Result == 50052)
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg("NETWORK_NOT_REACHABLE_DESC_2", delegate
						{
							BattleServerLogin(ip, port, p_cb);
						}, delegate
						{
							BattleServerLogout();
							if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene == "hometop")
							{
								MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, false);
							}
							else
							{
								StageUpdateOpenPauseCommonOut();
							}
						});
					}
					else
					{
						BattleServerLogout();
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rSBattleLogin.Result);
					}
				}, 0, true);
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBattleLogin(SocketCommon.ProtocolVersion, MonoBehaviourSingleton<GameServerService>.Instance.ServiceToken, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname));
			}
			else
			{
				StageUpdate.SetMainPlayerOCNotLocalPlayer();
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg("NETWORK_NOT_REACHABLE_DESC_2", delegate
				{
					BattleServerLogin(ip, port, p_cb);
				}, delegate
				{
					BattleServerLogout();
					StageUpdateOpenPauseCommonOut();
				});
			}
		}, DisconnectCallback, SocketIOErrorCallback);
	}

	private void StageUpdateOpenPauseCommonOut()
	{
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if (stageUpdate != null)
		{
			stageUpdate.PauseCommonOut();
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, false);
		}
	}

	private void CheckWhenDisconnect()
	{
		if (bWaitConnect && nCheckReturnTimes > 5)
		{
			LastCheckReturnCB.CheckTargetToInvoke();
			LastCheckReturnCB = null;
			return;
		}
		if (LastCheckReturnCB == null)
		{
			nCheckReturnTimes = 0;
			LastCheckReturnCB = StageUpdateOpenPauseCommonOut;
		}
		bWaitConnect = false;
		Debug.LogWarning("偵測到斷線準備重連" + nCheckReturnTimes);
		CheckReturnLastNetGame(LastCheckReturnCB);
		if (ListMemberInfo.Count <= 0)
		{
			return;
		}
		MemberInfo memberInfo = ListMemberInfo.First((MemberInfo x) => x.PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
		if (memberInfo == null)
		{
			return;
		}
		if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Contains(memberInfo))
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Remove(memberInfo);
		}
		memberInfo.bInGame = false;
		MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount--;
		bool flag = false;
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (ListMemberInfo[i].bInGame)
			{
				flag = true;
			}
		}
		if (!(sHostPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && flag))
		{
			return;
		}
		for (int j = 0; j < ListMemberInfo.Count; j++)
		{
			if (ListMemberInfo[j].bInGame)
			{
				sHostPlayerID = ListMemberInfo[j].PlayerId;
				break;
			}
		}
		if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == sHostPlayerID)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_HOST, true);
		}
		else
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_HOST, false);
		}
	}

	private void DisconnectCallback()
	{
		Debug.Log("[Battle] DisconnectCallback");
		CheckWhenDisconnect();
	}

	private void SocketIOErrorCallback(bool connected)
	{
		Debug.Log("[Battle] SocketIOErrorCallback");
		if (connected)
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
			ui.SetupConfirmByKey("COMMON_TIP", "NETWORK_SOCKET_IO_ERROR", "COMMON_OK", delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, false);
			});
		});
	}

	public void BattleServerLogout()
	{
		bIsReConnect = false;
		bWaitConnect = false;
		LastCheckReturnCB = null;
		nCheckReturnTimes = 0;
		RemoveHandler();
		if (targetMarkUI != null)
		{
			targetMarkUI.RemoveLateUpdate();
		}
		goStageData = null;
		MonoBehaviourSingleton<CBSocketClient>.Instance.Disconnect();
		ListPvpIDPosition.Clear();
		ListMemberInfo.Clear();
		listCmdSeasonBattleInfoReq.Clear();
		listGeneratingPlayer.Clear();
		InputInfoBuff.Clear();
		listCheckGenerateParam.Clear();
	}

	public void ReadyToGoPVE(string ip, int port, string roomId, short stageType, int stageid, Callback failcb = null)
	{
		BattleServerLogin(ip, port, delegate
		{
			AddHandler();
			bWaitConnect = false;
			LastCheckReturnCB = null;
			nCheckReturnTimes = 0;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.RSCreateJoinPVELockedRoom, delegate(object res)
			{
				RSCreateJoinPVELockedRoom rSCreateJoinPVELockedRoom = (RSCreateJoinPVELockedRoom)res;
				if (bIsReConnect && rSCreateJoinPVELockedRoom.Result != 52000)
				{
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
					BattleServerLogout();
					if (failcb != null)
					{
						failcb();
					}
				}
				else if (rSCreateJoinPVELockedRoom.Result != 52000)
				{
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
					switch ((Code)rSCreateJoinPVELockedRoom.Result)
					{
					case Code.BATTLE_LOCKEDROOM_ALREADY_REMOVED:
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("BATTLEROOM_REMOVED");
						break;
					case Code.BATTLE_LOCKEDROOM_CLOSING:
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("BATTLEROOM_CLOSING");
						break;
					default:
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rSCreateJoinPVELockedRoom.Result, false);
						break;
					}
					BattleServerLogout();
				}
				else
				{
					MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogout();
				}
			}, 0, true);
			goStageData = new GoStageData(BattleType.PVE, stageid);
			MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount = 999;
			ManagedSingleton<StageHelper>.Instance.nLastStageID = stageid;
			ReturnGameData value = new ReturnGameData
			{
				bIsPvP = false,
				bIsPvE = true,
				sIP = ip,
				nPort = port,
				sRoomID = roomId,
				nStageType = stageType,
				nStageID = stageid,
				SelfSealedBattleSetting = MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting,
				dSaveTime = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
				bCMode = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode
			};
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = JsonConvert.SerializeObject(value);
			MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQCreateJoinPVELockedRoom(stageType, stageid, roomId, MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting));
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
			strRoomID = roomId;
		});
	}

    [Obsolete]
    public void ReadyToGoPVP(int pvptier, PVPMatchType pvptype, string ip, int port, string roomId, CallbackObj failcb = null, Callback successCB = null)
	{
		BattleServerLogin(ip, port, delegate
		{
			AddHandler();
			bWaitConnect = false;
			LastCheckReturnCB = null;
			nCheckReturnTimes = 0;
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.RSCreateJoinPVPLockedRoom, delegate(object res)
			{
				RSCreateJoinPVPLockedRoom rSCreateJoinPVPLockedRoom = (RSCreateJoinPVPLockedRoom)res;
				if (bIsReConnect && rSCreateJoinPVPLockedRoom.Result != 52000)
				{
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
					BattleServerLogout();
					failcb.CheckTargetToInvoke(rSCreateJoinPVPLockedRoom.Result);
				}
				else if (rSCreateJoinPVPLockedRoom.Result != 52000)
				{
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
					switch ((Code)rSCreateJoinPVPLockedRoom.Result)
					{
					case Code.BATTLE_LOCKEDROOM_ALREADY_REMOVED:
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("BATTLEROOM_REMOVED");
						break;
					case Code.BATTLE_LOCKEDROOM_CLOSING:
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("BATTLEROOM_CLOSING");
						break;
					default:
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rSCreateJoinPVPLockedRoom.Result, false);
						break;
					}
					BattleServerLogout();
					failcb.CheckTargetToInvoke(rSCreateJoinPVPLockedRoom.Result);
				}
				else
				{
					MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogout();
				}
			}, 0, true);
			goStageData = new GoStageData(BattleType.PVP, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID);
			MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount = 999;
			ManagedSingleton<StageHelper>.Instance.nLastStageID = MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID;
			ReturnGameData value = new ReturnGameData
			{
				bIsPvP = true,
				bIsPvE = false,
				sIP = ip,
				nPort = port,
				sRoomID = roomId,
				nPvPtier = pvptier,
				ePvPType = pvptype,
				nStageID = MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID,
				SelfSealedBattleSetting = MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting,
				dSaveTime = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
				bCMode = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode
			};
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = JsonConvert.SerializeObject(value);
			PlayerPrefs.SetInt("PvpGameType", (int)MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType);
			PlayerPrefs.SetInt("PvpMatchType", (int)MonoBehaviourSingleton<OrangeMatchManager>.Instance.LastRqPvpMatchType);
			MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQCreateJoinPVPLockedRoom(pvptier, (int)pvptype, roomId, MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting));
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
			strRoomID = roomId;
			successCB.CheckTargetToInvoke();
		});
	}

	public void ShowPvpReport(string killer, string Killed, string sDiePlayerID)
	{
		if (!IsPvp)
		{
			return;
		}
		string[] array = new string[2] { "12C2FFFF", "FF3B3CFF" };
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (ListMemberInfo[i].Nickname == killer && ListMemberInfo[i].PlayerId != sDiePlayerID)
			{
				killer = "<color=#" + array[ListMemberInfo[i].Team] + ">" + killer + "</color>";
			}
			if (ListMemberInfo[i].Nickname == Killed && ListMemberInfo[i].PlayerId == sDiePlayerID)
			{
				Killed = "<color=#" + array[ListMemberInfo[i].Team] + ">" + Killed + "</color>";
			}
		}
		PvpReportUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpReportUI>("UI_PvpReport");
		if ((bool)uI)
		{
			uI.SetMsg(killer, Killed);
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReport", delegate(PvpReportUI ui)
		{
			ui.SetMsg(killer, Killed);
		});
	}

	private void AddHandler()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTEveryoneJoinedLockedRoom, OnNTEveryoneJoinedLockedRoom);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTLeaveBattleRoom, OnNTLeaveBattleRoom);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.RSLeaveBattleRoom, OnRSLeaveBattleRoom);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTBattleRoomOwnerChange, OnNTBattleRoomOwnerChange);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTBroadcastToRoomIncludeSelf, NTBroadcastToRoomIncludeSelf);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTBroadcastBinaryToRoom, OnNTBroadcastBinaryToRoom);
	}

	private void RemoveHandler()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTEveryoneJoinedLockedRoom, OnNTEveryoneJoinedLockedRoom);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTLeaveBattleRoom, OnNTLeaveBattleRoom);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.RSLeaveBattleRoom, OnRSLeaveBattleRoom);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTBattleRoomOwnerChange, OnNTBattleRoomOwnerChange);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTBroadcastToRoomIncludeSelf, NTBroadcastToRoomIncludeSelf);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTBroadcastBinaryToRoom, OnNTBroadcastBinaryToRoom);
	}

	private void AddMemberInfo(ref NTEveryoneJoinedLockedRoom tNTEveryoneJoinedLockedRoom, bool bNeedBorn = false)
	{
		int playeridLength = tNTEveryoneJoinedLockedRoom.PlayeridLength;
		for (int i = 0; i < playeridLength; i++)
		{
			bool flag = false;
			foreach (MemberInfo tInfo in ListMemberInfo)
			{
				if (!(tInfo.PlayerId == tNTEveryoneJoinedLockedRoom.Playerid(i)))
				{
					continue;
				}
				flag = true;
				if (!tInfo.bInGame)
				{
					tInfo.bInGame = true;
					tInfo.fLastCheckConnectTime = 0f;
					tInfo.bLoadEnd = true;
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReport", delegate(PvpReportUI ui)
					{
						ui.SetMsg(tInfo.Nickname, "", "CORP_JOIN_ROOM");
					});
				}
				break;
			}
			if (flag)
			{
				continue;
			}
			NetSealBattleSettingInfo netSealBattleSettingInfo = ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(tNTEveryoneJoinedLockedRoom.Unsealedbattlesetting(i));
			if (netSealBattleSettingInfo != null)
			{
				MemberInfo info = new MemberInfo(tNTEveryoneJoinedLockedRoom.Playerid(i), tNTEveryoneJoinedLockedRoom.NickName(i), tNTEveryoneJoinedLockedRoom.Realm(i), netSealBattleSettingInfo);
				ListMemberInfo.Add(info);
				if (bNeedBorn)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReport", delegate(PvpReportUI ui)
					{
						ui.SetMsg(info.Nickname, "", "CORP_JOIN_ROOM");
					});
				}
			}
			else
			{
				Debug.LogError(string.Format("ParserUnsealedBattleSetting Fail! => Length[{0}] i[{1}] Setting[{2}]", playeridLength, i, tNTEveryoneJoinedLockedRoom.Unsealedbattlesetting(i)));
			}
		}
	}

	private void OnNTEveryoneJoinedLockedRoom(object res)
	{
		if (MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount != 999)
		{
			if (res is NTEveryoneJoinedLockedRoom)
			{
				NTEveryoneJoinedLockedRoom tNTEveryoneJoinedLockedRoom = (NTEveryoneJoinedLockedRoom)res;
				LockStepController.LockStepTargetTimeFrame = tNTEveryoneJoinedLockedRoom.LockStepTimeFrame;
				if (tNTEveryoneJoinedLockedRoom.PlayeridLength > MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount)
				{
					MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount = tNTEveryoneJoinedLockedRoom.PlayeridLength;
				}
				AddMemberInfo(ref tNTEveryoneJoinedLockedRoom, true);
			}
			return;
		}
		if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene == "StageTest")
		{
			if (StageUpdate.nReConnectMode == 0)
			{
				StageResManager.GetStageUpdate().SendReConnectMsg();
				return;
			}
			if (StageUpdate.nReConnectMode == 1)
			{
				if (!(res is NTEveryoneJoinedLockedRoom))
				{
					return;
				}
				NTEveryoneJoinedLockedRoom tNTEveryoneJoinedLockedRoom2 = (NTEveryoneJoinedLockedRoom)res;
				LockStepController.LockStepTargetTimeFrame = tNTEveryoneJoinedLockedRoom2.LockStepTimeFrame;
				MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount = tNTEveryoneJoinedLockedRoom2.PlayeridLength;
				AddMemberInfo(ref tNTEveryoneJoinedLockedRoom2, true);
				if (bIsReConnect && StageUpdate.gbStageReady)
				{
					StageUpdate stageUpdate = StageResManager.GetStageUpdate();
					StageUpdate.ReqChangeHost();
					if (StageUpdate.bIsHost)
					{
						Debug.LogError("just reconnect end but is host");
						StageUpdate.bIsHost = false;
					}
					stageUpdate.SendReConnectMsg();
				}
				return;
			}
		}
		if (!(res is NTEveryoneJoinedLockedRoom))
		{
			return;
		}
		ListMemberInfo.Clear();
		ListPvpIDPosition.Clear();
		NTEveryoneJoinedLockedRoom tNTEveryoneJoinedLockedRoom3 = (NTEveryoneJoinedLockedRoom)res;
		LockStepController.LockStepTargetTimeFrame = tNTEveryoneJoinedLockedRoom3.LockStepTimeFrame;
		STAGE_TABLE sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[goStageData.stageId];
		if (sTAGE_TABLE.n_MAIN == 90000 && sTAGE_TABLE.n_SUB == 1 && tNTEveryoneJoinedLockedRoom3.PlayeridLength != 2)
		{
			return;
		}
		if (tNTEveryoneJoinedLockedRoom3.PlayeridLength < 2 && MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType == PVPMatchType.FriendOneVSOne)
		{
			Debug.Log("Guest join failed.");
			MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQLeaveBattleRoom(strRoomID));
			return;
		}
		if (sTAGE_TABLE.n_STAGE_RULE > 0)
		{
			ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = sTAGE_TABLE.n_STAGE_RULE;
		}
		else
		{
			ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = 0;
		}
		if (bCurrentCoopChallengeMode && sTAGE_TABLE.n_TYPE == 5 && sTAGE_TABLE.n_SECRET != 0)
		{
			ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = sTAGE_TABLE.n_SECRET;
		}
		AddMemberInfo(ref tNTEveryoneJoinedLockedRoom3);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSetPlayerContact(tNTEveryoneJoinedLockedRoom3);
		sHostPlayerID = tNTEveryoneJoinedLockedRoom3.Ownerid;
		StageUpdate.bIsHost = sHostPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount = ListMemberInfo.Count;
		OrangeSceneManager.LoadingType loadingType = OrangeSceneManager.LoadingType.DEFAULT;
		if (goStageData.battleType == BattleType.PVP)
		{
			loadingType = OrangeSceneManager.LoadingType.PVP;
			StageUpdate.SetStageName(sTAGE_TABLE.s_STAGE, sTAGE_TABLE.n_DIFFICULTY, false, false, true);
			int num = 0;
			for (int i = 0; i < ListMemberInfo.Count; i++)
			{
				if (ListMemberInfo[i].PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					num = ListMemberInfo[i].Team;
					break;
				}
			}
			for (int j = 0; j < ListMemberInfo.Count; j++)
			{
				if (ListMemberInfo[j].Team == num)
				{
					MonoBehaviourSingleton<VoiceChatManager>.Instance.SetVoiceServerName(ListMemberInfo[j].PlayerId + num);
					break;
				}
			}
			if (bIsReConnect)
			{
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", loadingType);
			}
			else if (sTAGE_TABLE.n_MAIN == 90000 && sTAGE_TABLE.n_SUB == 1)
			{
				NetSealBattleSettingInfo playerBattleSetting = null;
				NetSealBattleSettingInfo rivalBattleSetting = null;
				for (int k = 0; k < ListMemberInfo.Count; k++)
				{
					if (ListMemberInfo[k].PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
					{
						playerBattleSetting = ListMemberInfo[k].netSealBattleSettingInfo;
						continue;
					}
					rivalBattleSetting = ListMemberInfo[k].netSealBattleSettingInfo;
					SetPlayerHUD(ListMemberInfo[k].PlayerId);
				}
				ManagedSingleton<PlayerNetManager>.Instance.SeasonBattleStartReq(playerBattleSetting, rivalBattleSetting, delegate(SeasonBattleStartRes Res)
				{
					if (Res.Code == 27150)
					{
						MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, true, delegate
						{
							MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
							{
								ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
								ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
								ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_SEASON_END"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
							});
						});
					}
					else
					{
						MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSetBusy(UserStatus.Fighting);
						MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", loadingType);
					}
				});
			}
			else
			{
				ManagedSingleton<PlayerNetManager>.Instance.StageStartReq(sTAGE_TABLE.n_ID, sTAGE_TABLE.s_STAGE, ManagedSingleton<StageHelper>.Instance.GetStageCrc(sTAGE_TABLE.n_ID), delegate
				{
					MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", loadingType);
				});
			}
			return;
		}
		loadingType = OrangeSceneManager.LoadingType.STAGE;
		MonoBehaviourSingleton<VoiceChatManager>.Instance.SetVoiceServerName(ListMemberInfo[0].PlayerId);
		StageUpdate.SetStageName(sTAGE_TABLE.s_STAGE, sTAGE_TABLE.n_DIFFICULTY, false, true);
		if (bIsReConnect)
		{
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", loadingType);
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.StageStartReq(sTAGE_TABLE.n_ID, sTAGE_TABLE.s_STAGE, ManagedSingleton<StageHelper>.Instance.GetStageCrc(sTAGE_TABLE.n_ID), delegate
		{
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", loadingType, null, false);
		});
	}

	private void OnRSLeaveBattleRoom(object res)
	{
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.BattleServerLogout();
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	private void OnNTLeaveBattleRoom(object res)
	{
		if (!(res is NTLeaveBattleRoom))
		{
			return;
		}
		NTLeaveBattleRoom rs = (NTLeaveBattleRoom)res;
		Debug.LogWarning("[NTLeaveBattleRoom] Playerid:" + rs.Playerid);
		MemberInfo memberInfo = null;
		if (ListMemberInfo.Count == 0)
		{
			return;
		}
		StartCoroutine(CheckOCCoroutine(rs.Playerid));
		memberInfo = ListMemberInfo.First((MemberInfo x) => x.PlayerId == rs.Playerid);
		if (memberInfo == null)
		{
			return;
		}
		if (memberInfo.bInGame)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReport", delegate(PvpReportUI ui)
			{
				ui.SetMsg(memberInfo.Nickname, "", "CORP_LEAVE_ROOM");
			});
		}
		if (MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Contains(memberInfo))
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo.Remove(memberInfo);
		}
		memberInfo.bInGame = false;
		MonoBehaviourSingleton<StageSyncManager>.Instance.nNeedWaitLoadCount--;
		bool flag = false;
		for (int i = 0; i < ListMemberInfo.Count; i++)
		{
			if (ListMemberInfo[i].bInGame)
			{
				flag = true;
			}
		}
		if (!(sHostPlayerID == rs.Playerid && flag))
		{
			return;
		}
		for (int j = 0; j < ListMemberInfo.Count; j++)
		{
			if (ListMemberInfo[j].bInGame)
			{
				sHostPlayerID = ListMemberInfo[j].PlayerId;
				break;
			}
		}
		if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == sHostPlayerID)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_HOST, true);
		}
		else
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_HOST, false);
		}
	}

	private IEnumerator CheckOCCoroutine(string sPlayerID)
	{
		while (ListMemberInfo.Count != 0 && StageUpdate.GetPlayerByID(sPlayerID) == null)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.PLAYERBUILD_PLAYER_NETCTRLON, sPlayerID, true);
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private void OnNTBattleRoomOwnerChange(object res)
	{
		if (res is NTBattleRoomOwnerChange)
		{
			NTBattleRoomOwnerChange nTBattleRoomOwnerChange = (NTBattleRoomOwnerChange)res;
			Debug.LogWarning("[NTBroadcastToRoom] Playerid:" + nTBattleRoomOwnerChange.Playerid);
			sHostPlayerID = nTBattleRoomOwnerChange.Playerid;
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == nTBattleRoomOwnerChange.Playerid)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_HOST, true);
			}
			else
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_HOST, false);
			}
		}
	}

	private void OnNTBroadcastBinaryToRoom(object res)
	{
		if (StageUpdate.gbIsNetGame && res is NTBroadcastBinaryToRoom)
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(TransferNTBroadcastBinaryToRoomToByte((NTBroadcastBinaryToRoom)res)));
			binaryReader.BaseStream.Position = 0L;
			switch ((BattleSyncData)binaryReader.ReadByte())
			{
			case BattleSyncData.PLAYER_DIE:
				PlayerDie(binaryReader);
				break;
			case BattleSyncData.PLAYER_CONTROLLER:
				PlayerControllerChange(binaryReader);
				break;
			}
		}
	}

	private void NTBroadcastToRoomIncludeSelf(object res)
	{
		if (StageUpdate.gbIsNetGame && res is NTBroadcastToRoomIncludeSelf)
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(TransferNTBroadcastToRoomIncludeSelfToByte((NTBroadcastToRoomIncludeSelf)res)));
			binaryReader.BaseStream.Position = 0L;
			BattleSyncData battleSyncData = (BattleSyncData)binaryReader.ReadByte();
			if (battleSyncData == BattleSyncData.PLAYER_INPUT)
			{
				PlayerInput(binaryReader);
			}
		}
	}

	private byte[] TransferNTBroadcastBinaryToRoomToByte(NTBroadcastBinaryToRoom ntPro)
	{
		byte[] array = new byte[ntPro.ByteActionLength];
		try
		{
			byte[] array2 = ntPro.GetByteActionBytes().Value.Array;
			for (int i = 1; i <= fbSamplingTimes; i++)
			{
				int num = CapUtility.Random(0, ntPro.ByteActionLength - 1);
				if (array2[num + fbArrayOffset] != ntPro.ByteAction(num))
				{
					throw new Exception("Sampling Failed.");
				}
			}
			Array.Copy(array2, fbArrayOffset, array, 0, ntPro.ByteActionLength);
		}
		catch (Exception ex)
		{
			Debug.Log(string.Format("Transfer failed. Err=[{0}].", ex.Message));
			for (int j = 0; j < ntPro.ByteActionLength; j++)
			{
				array[j] = ntPro.ByteAction(j);
			}
		}
		return array;
	}

	private byte[] TransferNTBroadcastToRoomIncludeSelfToByte(NTBroadcastToRoomIncludeSelf ntPro)
	{
		byte[] array = new byte[ntPro.ByteActionLength];
		try
		{
			byte[] array2 = ntPro.GetByteActionBytes().Value.Array;
			for (int i = 1; i <= fbSamplingTimes; i++)
			{
				int num = CapUtility.Random(0, ntPro.ByteActionLength - 1);
				if (array2[num + fbArrayOffset] != ntPro.ByteAction(num))
				{
					throw new Exception("Sampling Failed.");
				}
			}
			Array.Copy(array2, fbArrayOffset, array, 0, ntPro.ByteActionLength);
		}
		catch (Exception ex)
		{
			Debug.Log(string.Format("Transfer failed. Err=[{0}].", ex.Message));
			for (int j = 0; j < ntPro.ByteActionLength; j++)
			{
				array[j] = ntPro.ByteAction(j);
			}
		}
		return array;
	}

	private void PlayerInput(BinaryReader br)
	{
		if (StageUpdate.bWaitReconnect)
		{
			return;
		}
		string text = br.ReadExString();
		OrangeCharacter playerByID = StageUpdate.GetPlayerByID(text);
		if (playerByID != null)
		{
			if (playerByID is OrangeNetCharacter)
			{
				(playerByID as OrangeNetCharacter).RecvNetworkRuntimeData(br);
			}
			return;
		}
		if (InputInfoBuff.ContainsKey(text))
		{
			InputInfo inputInfo = new InputInfo();
			int count = InputInfoBuff[text].Count;
			InputInfoBuff[text][count - 1].CopyTo(inputInfo);
			inputInfo.CombineRuntimeDiff(br);
			InputInfoBuff[text].Add(inputInfo);
		}
		else
		{
			InputInfo inputInfo2 = new InputInfo();
			inputInfo2.CombineRuntimeDiff(br);
			InputInfoBuff.Add(text, new List<InputInfo>());
			InputInfoBuff[text].Add(inputInfo2);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.PLAYERBUILD_PLAYER_NETCTRLON, text, false);
	}

	private void PlayerDie(BinaryReader br)
	{
		string text = br.ReadExString();
		string killer = br.ReadExString();
		string killed = br.ReadExString();
		ShowPvpReport(killer, killed, text);
		if (!(text == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify))
		{
			OrangeCharacter playerByID = StageUpdate.GetPlayerByID(text);
			if (playerByID != null)
			{
				playerByID.DieFromServer();
			}
		}
	}

	private void PlayerControllerChange(BinaryReader br)
	{
		string text = br.ReadExString();
		string value = br.ReadExString();
		if (text == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			return;
		}
		OrangeCharacter playerByID = StageUpdate.GetPlayerByID(text);
		if (playerByID != null)
		{
			Setting setting = JsonConvert.DeserializeObject<Setting>(value);
			if (setting != null)
			{
				playerByID.PlayerSetting = setting;
			}
		}
	}

	public HUNTERRANK_TABLE GetHunterRankTable(int Socre)
	{
		List<HUNTERRANK_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT.Values.ToList();
		HUNTERRANK_TABLE result = null;
		for (int i = 0; i < list.Count; i++)
		{
			result = list[i];
			if (Socre <= list[i].n_PT_MAX)
			{
				break;
			}
		}
		return result;
	}

	public bool IsHunterRankMax(int Socre)
	{
		List<HUNTERRANK_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (i == list.Count - 1)
			{
				return true;
			}
			HUNTERRANK_TABLE hUNTERRANK_TABLE = list[i];
			if (Socre <= list[i].n_PT_MAX)
			{
				break;
			}
		}
		return false;
	}

	public void SetSeasonIconFlag(int n_ID, CharacterColumeSmall.PerCharacterSmallCell cell)
	{
		if (idcSeasonIconFlag.ContainsKey(n_ID))
		{
			idcSeasonIconFlag[n_ID] = cell;
		}
		else
		{
			idcSeasonIconFlag.Add(n_ID, cell);
		}
	}

	private string GetSeasonStr(int num)
	{
		switch (num)
		{
		default:
			return string.Empty;
		case 1:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_RANKING_CHARA1");
		case 2:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_RANKING_CHARA2");
		case 3:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PVP_RANKING_CHARA3");
		}
	}

	public void ModifySeasonIconFlag(bool bShow, int n_ID, int num)
	{
		if (num <= 0)
		{
			bShow = false;
		}
		if (idcSeasonIconFlag.ContainsKey(n_ID) && idcSeasonIconFlag[n_ID].imgPlaying != null)
		{
			idcSeasonIconFlag[n_ID].imgPlaying.gameObject.SetActive(bShow);
			idcSeasonIconFlag[n_ID].textPlaying.text = GetSeasonStr(num);
			idcSeasonIconFlag[n_ID].textPlaying.color = SeasonColor1[num];
			idcSeasonIconFlag[n_ID].textPlaying.GetComponent<UIShadow>().effectColor = SeasonColor2[num];
		}
	}

	public int CheckSeasonCharaterNum(int n_ID)
	{
		for (int i = 0; i < SeasonCharaterList.Count; i++)
		{
			if (SeasonCharaterList[i] == n_ID)
			{
				return i + 1;
			}
		}
		return 0;
	}

	private int GetCardTypeIndex(int typ)
	{
		return (int)Math.Log(typ, 2.0);
	}

	public List<int> GetCharactertCardSkillList(int CharacterID, List<NetCardInfo> CardInfoList = null, List<NetCharacterCardSlotInfo> CharacterCardSlotInfoList = null)
	{
		List<int> list = new List<int>();
		NetCharacterCardSlotInfo[] array = new NetCharacterCardSlotInfo[3];
		NetCardInfo[] array2 = new NetCardInfo[3];
		CARD_TABLE[] array3 = new CARD_TABLE[3];
		int[] array4 = new int[6];
		if (CardInfoList != null && CharacterCardSlotInfoList != null)
		{
			for (int i = 0; i < CharacterCardSlotInfoList.Count; i++)
			{
				if (CharacterCardSlotInfoList[i].CharacterID != CharacterID)
				{
					continue;
				}
				int num = CharacterCardSlotInfoList[i].CharacterCardSlot - 1;
				if (num < 0 || num >= array.Length)
				{
					continue;
				}
				int CardSeqID = CharacterCardSlotInfoList[i].CardSeqID;
				NetCardInfo netCardInfo = CardInfoList.Find((NetCardInfo x) => x.CardSeqID == CardSeqID);
				if (netCardInfo != null)
				{
					int cardID = netCardInfo.CardID;
					if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.ContainsKey(cardID))
					{
						CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[cardID];
						int cardTypeIndex = GetCardTypeIndex(cARD_TABLE.n_TYPE);
						array4[cardTypeIndex]++;
						array[num] = CharacterCardSlotInfoList[i];
						array3[num] = cARD_TABLE;
						array2[num] = netCardInfo;
					}
				}
			}
		}
		else if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.ContainsKey(CharacterID))
		{
			List<NetCharacterCardSlotInfo> list2 = ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo[CharacterID].Values.ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				int num2 = list2[j].CharacterCardSlot - 1;
				if (num2 < 0 || num2 >= array.Length)
				{
					continue;
				}
				int cardSeqID = list2[j].CardSeqID;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(cardSeqID))
				{
					int cardID2 = ManagedSingleton<PlayerNetManager>.Instance.dicCard[cardSeqID].netCardInfo.CardID;
					if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.ContainsKey(cardID2))
					{
						CARD_TABLE cARD_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[cardID2];
						int cardTypeIndex2 = GetCardTypeIndex(cARD_TABLE2.n_TYPE);
						array4[cardTypeIndex2]++;
						array[num2] = list2[j];
						array3[num2] = cARD_TABLE2;
						array2[num2] = ManagedSingleton<PlayerNetManager>.Instance.dicCard[cardSeqID].netCardInfo;
					}
				}
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] == null)
			{
				continue;
			}
			CARD_TABLE cARD_TABLE3 = array3[k];
			int[] array5 = new int[6] { cARD_TABLE3.n_SKILL1_RANK0, cARD_TABLE3.n_SKILL1_RANK1, cARD_TABLE3.n_SKILL1_RANK2, cARD_TABLE3.n_SKILL1_RANK3, cARD_TABLE3.n_SKILL1_RANK4, cARD_TABLE3.n_SKILL1_RANK5 };
			int[] array6 = new int[6] { cARD_TABLE3.n_SKILL2_RANK0, cARD_TABLE3.n_SKILL2_RANK1, cARD_TABLE3.n_SKILL2_RANK2, cARD_TABLE3.n_SKILL2_RANK3, cARD_TABLE3.n_SKILL2_RANK4, cARD_TABLE3.n_SKILL2_RANK5 };
			bool flag = true;
			int[] array7 = new int[array4.Length];
			array4.CopyTo(array7, 0);
			if (cARD_TABLE3.s_SKILL1_COMBINATION != "null")
			{
				string[] array8 = cARD_TABLE3.s_SKILL1_COMBINATION.Split(',');
				for (int l = 0; l < array8.Length; l++)
				{
					int typ = int.Parse(array8[l]);
					if (array7[GetCardTypeIndex(typ)] < 1)
					{
						flag = false;
						break;
					}
					array7[GetCardTypeIndex(typ)]--;
				}
			}
			else if (cARD_TABLE3.n_SKILL1_CHARAID != CharacterID)
			{
				flag = false;
			}
			if (flag)
			{
				list.Add(array5[array2[k].Star]);
			}
			flag = true;
			array7 = new int[array4.Length];
			array4.CopyTo(array7, 0);
			if (cARD_TABLE3.s_SKILL2_COMBINATION != "null")
			{
				string[] array9 = cARD_TABLE3.s_SKILL2_COMBINATION.Split(',');
				for (int m = 0; m < array9.Length; m++)
				{
					int typ2 = int.Parse(array9[m]);
					if (array7[GetCardTypeIndex(typ2)] < 1)
					{
						flag = false;
						break;
					}
					array7[GetCardTypeIndex(typ2)]--;
				}
			}
			else if (cARD_TABLE3.n_SKILL2_CHARAID != CharacterID)
			{
				flag = false;
			}
			if (flag)
			{
				list.Add(array6[array2[k].Star]);
			}
		}
		return list;
	}

	public bool CheckCharactertCardSkillActive(int CharacterID, int SkillID, int SeqID, List<NetCharacterCardSlotInfo> tmpList = null)
	{
		new List<int>();
		NetCharacterCardSlotInfo[] array = new NetCharacterCardSlotInfo[3];
		CardInfo[] array2 = new CardInfo[3];
		CARD_TABLE[] array3 = new CARD_TABLE[3];
		int[] array4 = new int[6];
		if (tmpList == null || tmpList.Count <= 0)
		{
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.ContainsKey(CharacterID))
			{
				tmpList = ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo[CharacterID].Values.ToList();
			}
			else
			{
				tmpList = new List<NetCharacterCardSlotInfo>();
				tmpList.Add(new NetCharacterCardSlotInfo());
				tmpList.Add(new NetCharacterCardSlotInfo());
				tmpList.Add(new NetCharacterCardSlotInfo());
			}
		}
		for (int i = 0; i < tmpList.Count; i++)
		{
			if (tmpList[i] == null)
			{
				tmpList[i] = new NetCharacterCardSlotInfo();
			}
		}
		for (int j = 0; j < tmpList.Count; j++)
		{
			int num = tmpList[j].CharacterCardSlot - 1;
			if (num < 0 || num >= array.Length)
			{
				continue;
			}
			int cardSeqID = tmpList[j].CardSeqID;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(cardSeqID))
			{
				int cardID = ManagedSingleton<PlayerNetManager>.Instance.dicCard[cardSeqID].netCardInfo.CardID;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.ContainsKey(cardID))
				{
					CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[cardID];
					int cardTypeIndex = GetCardTypeIndex(cARD_TABLE.n_TYPE);
					array4[cardTypeIndex]++;
					array[num] = tmpList[j];
					array3[num] = cARD_TABLE;
					array2[num] = ManagedSingleton<PlayerNetManager>.Instance.dicCard[cardSeqID];
				}
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] == null)
			{
				continue;
			}
			CARD_TABLE cARD_TABLE2 = array3[k];
			int[] array5 = new int[6] { cARD_TABLE2.n_SKILL1_RANK0, cARD_TABLE2.n_SKILL1_RANK1, cARD_TABLE2.n_SKILL1_RANK2, cARD_TABLE2.n_SKILL1_RANK3, cARD_TABLE2.n_SKILL1_RANK4, cARD_TABLE2.n_SKILL1_RANK5 };
			int[] array6 = new int[6] { cARD_TABLE2.n_SKILL2_RANK0, cARD_TABLE2.n_SKILL2_RANK1, cARD_TABLE2.n_SKILL2_RANK2, cARD_TABLE2.n_SKILL2_RANK3, cARD_TABLE2.n_SKILL2_RANK4, cARD_TABLE2.n_SKILL2_RANK5 };
			bool flag = false;
			int[] array7 = new int[array4.Length];
			array4.CopyTo(array7, 0);
			if (cARD_TABLE2.s_SKILL1_COMBINATION != "null")
			{
				string[] array8 = cARD_TABLE2.s_SKILL1_COMBINATION.Split(',');
				for (int l = 0; l < array8.Length; l++)
				{
					int typ = int.Parse(array8[l]);
					if (array7[GetCardTypeIndex(typ)] < 1)
					{
						flag = false;
						break;
					}
					array7[GetCardTypeIndex(typ)]--;
					flag = true;
				}
			}
			else if (cARD_TABLE2.n_SKILL1_CHARAID == CharacterID)
			{
				flag = true;
			}
			int num2 = 0;
			if (flag)
			{
				num2 = array5[array2[k].netCardInfo.Star];
				if (SkillID == num2 && SeqID == array2[k].netCardInfo.CardSeqID)
				{
					return true;
				}
			}
			flag = false;
			array7 = new int[array4.Length];
			array4.CopyTo(array7, 0);
			if (cARD_TABLE2.s_SKILL2_COMBINATION != "null")
			{
				string[] array9 = cARD_TABLE2.s_SKILL2_COMBINATION.Split(',');
				for (int m = 0; m < array9.Length; m++)
				{
					int typ2 = int.Parse(array9[m]);
					if (array7[GetCardTypeIndex(typ2)] < 1)
					{
						flag = false;
						break;
					}
					array7[GetCardTypeIndex(typ2)]--;
					flag = true;
				}
			}
			else if (cARD_TABLE2.n_SKILL2_CHARAID == CharacterID)
			{
				flag = true;
			}
			if (flag)
			{
				num2 = array6[array2[k].netCardInfo.Star];
				if (SkillID == num2 && SeqID == array2[k].netCardInfo.CardSeqID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckCardCountMax()
	{
		int count = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Count;
		int num = OrangeConst.CARD_INITIAL_SLOT + ManagedSingleton<PlayerHelper>.Instance.GetCardExpansion();
		num = ((num > OrangeConst.CARD_MAX_SLOT) ? OrangeConst.CARD_MAX_SLOT : num);
		if (count > num)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardMsg", delegate(CardMsgUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Yes1SE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				ui.Yes2SE = SystemSE.NONE;
				ui.SetupCardMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_SLOT_MAX"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("HOMETOP_CARD"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_SLOT_EXPANSION"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CardMainUI>("UI_CardMain", delegate
					{
					});
				}, delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardStorageBuy", delegate(CardStorageBuyUI csbui)
					{
						csbui.IsInfinity = true;
						csbui.CostAmount = OrangeConst.CARD_EXPANSION_COST;
						csbui.CostAmountMax = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel();
						int num2 = OrangeConst.CARD_INITIAL_SLOT + ManagedSingleton<PlayerHelper>.Instance.GetCardExpansion();
						int maxCount = (OrangeConst.CARD_MAX_SLOT - num2) / OrangeConst.CARD_EXPANSION;
						string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_SLOT_EXPANSION");
						csbui.PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
						csbui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
						csbui.Setup(OrangeConst.CARD_EXPANSION, maxCount, str, 15, 2, delegate(object obj)
						{
							int amount = (int)obj;
							ManagedSingleton<PlayerNetManager>.Instance.ExpandCardStorageReq(amount, delegate
							{
							});
						});
					});
				});
			});
			return true;
		}
		return false;
	}
}
