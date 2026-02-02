#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OrangeApi;
using OrangeSocket;
using cc;

public class PowerTowerSystem : Singleton<PowerTowerSystem>
{
	private class NetOreInfoComparer : IEqualityComparer<NetOreInfo>
	{
		public bool Equals(NetOreInfo x, NetOreInfo y)
		{
			return GetHashCode(x) == GetHashCode(y);
		}

		public int GetHashCode(NetOreInfo oreInfo)
		{
			return oreInfo.OreGroup * 1000 + oreInfo.OreLevel;
		}
	}

	private readonly NetOreInfoComparer _netOreInfoComparer = new NetOreInfoComparer();

	private List<NetOreInfo> _oreInfoListCache = new List<NetOreInfo>();

	[CompilerGenerated]
	private readonly List<POWER_TABLE> _003CPowerTowerRankupSettings_003Ek__BackingField = (from attrData in ManagedSingleton<OrangeDataManager>.Instance.POWER_TABLE_DICT.Values
		group attrData by attrData.n_POWER_LIMIT into g
		select g.OrderBy((POWER_TABLE attrData) => attrData.n_ID).First() into attrData
		orderby attrData.n_ID
		select attrData).ToList();

	[CompilerGenerated]
	private readonly Dictionary<int, Dictionary<int, OreInfoData>> _003COreInfoDataDict_003Ek__BackingField = (from data in ManagedSingleton<OrangeDataManager>.Instance.ORE_TABLE_DICT.Values
		group data by data.n_ORE_GROUP).ToDictionary((IGrouping<int, ORE_TABLE> g) => g.Key, (IGrouping<int, ORE_TABLE> g) => g.ToDictionary((ORE_TABLE data) => data.n_ORE_LEVEL, (ORE_TABLE data) => new OreInfoData(data, true)));

	public List<PowerPillarInfoData> PowerPillarInfoDataListCache { get; private set; } = new List<PowerPillarInfoData>();


	public List<OreInfoData> OreInfoDataListCache { get; private set; } = new List<OreInfoData>();


	public List<POWER_TABLE> PowerTowerRankupSettings
	{
		[CompilerGenerated]
		get
		{
			return _003CPowerTowerRankupSettings_003Ek__BackingField;
		}
	}

	public Dictionary<int, Dictionary<int, OreInfoData>> OreInfoDataDict
	{
		[CompilerGenerated]
		get
		{
			return _003COreInfoDataDict_003Ek__BackingField;
		}
	}

	public event Action<Code> OnGetPowerPillarInfoEvent;

	public event Action OnGetPowerPillarInfoOnceEvent;

	public event Action<Code> OnOpenPowerPillarEvent;

	public event Action<Code> OnClosePowerPillarEvent;

	public event Action<Code> OnGetOreInfoEvent;

	public event Action OnGetOreInfoOnceEvent;

	public event Action<Code> OnOreLevelUpEvent;

	public event Action<Code> OnRankupEvent;

	public event Action OnPowerPillarChangedEvent;

	public event Action OnOreChangedEvent;

	public event Action OnSocketPowerTowerRankupEvent;

	public event Action OnSocketPowerPillarChangedEvent;

	public event Action OnSocketOreChangedEvent;

	public PowerTowerSystem()
	{
		Singleton<GuildSystem>.Instance.OnLeaveGuildEvent += OnLeaveGuildEvent;
		Singleton<GuildSystem>.Instance.OnRemoveGuildEvent += OnRemoveGuildEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent += OnSocketMemberKickedEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent += OnSocketGuildRemoveEvent;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.NTGuildEvent, OnSocketPowerTowerEvent);
	}

	private void ClearCacheData()
	{
		PowerPillarInfoDataListCache.Clear();
		Action onPowerPillarChangedEvent = this.OnPowerPillarChangedEvent;
		if (onPowerPillarChangedEvent != null)
		{
			onPowerPillarChangedEvent();
		}
		_oreInfoListCache.Clear();
		OreInfoDataListCache.Clear();
		Action onOreChangedEvent = this.OnOreChangedEvent;
		if (onOreChangedEvent != null)
		{
			onOreChangedEvent();
		}
	}

	public void ClearCacheList()
	{
		PowerPillarInfoDataListCache.Clear();
		_oreInfoListCache.Clear();
		OreInfoDataListCache.Clear();
	}

	public void ReqGetPowerPillarInfo()
	{
		ManagedSingleton<PlayerNetManager>.Instance.PowerTowerReqGetPowerPillarInfo(OnGetPowerPillarInfoRes);
	}

	public void ReqOpenPowerPillar(int pillarId, int oreGroup, int oreLv)
	{
		ManagedSingleton<PlayerNetManager>.Instance.PowerTowerReqOpenPowerPillar(pillarId, oreGroup, oreLv, OnOpenPowerPillarRes);
	}

	public void ReqClosePowerPillar(int oreId)
	{
		ManagedSingleton<PlayerNetManager>.Instance.PowerTowerReqClosePowerPillar(oreId, OnClosePowerPillarRes);
	}

	public void ReqGetOreInfo()
	{
		ManagedSingleton<PlayerNetManager>.Instance.PowerTowerReqGetOreInfo(OnGetOreInfoRes);
	}

	public void ReqOreLevelUp(int oreGroup, int oreLv)
	{
		ManagedSingleton<PlayerNetManager>.Instance.PowerTowerReqOreLevelUp(oreGroup, oreLv, OnOreLevelUpRes);
	}

	public void ReqRankup()
	{
		ManagedSingleton<PlayerNetManager>.Instance.PowerTowerReqRankup(OnRankupRes);
	}

	private void OnGetPowerPillarInfoRes(GuildGetPowerPillarInfoRes resp)
	{
		Debug.Log("[OnGetPowerPillarInfoRes]");
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_GET_POWER_PILLAR_INFO_SUCCESS)
		{
			RefreshPowerPillarInfoDataList(resp.PillarInfo);
			Action onGetPowerPillarInfoOnceEvent = this.OnGetPowerPillarInfoOnceEvent;
			if (onGetPowerPillarInfoOnceEvent != null)
			{
				onGetPowerPillarInfoOnceEvent();
			}
			this.OnGetPowerPillarInfoOnceEvent = null;
			Action<Code> onGetPowerPillarInfoEvent = this.OnGetPowerPillarInfoEvent;
			if (onGetPowerPillarInfoEvent != null)
			{
				onGetPowerPillarInfoEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnGetPowerPillarInfoRes", code);
			Action<Code> onGetPowerPillarInfoEvent2 = this.OnGetPowerPillarInfoEvent;
			if (onGetPowerPillarInfoEvent2 != null)
			{
				onGetPowerPillarInfoEvent2(code);
			}
		}
	}

	private void OnOpenPowerPillarRes(GuildPowerPillarOpenRes resp)
	{
		Debug.Log("[OnOpenPowerPillarRes]");
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_ORE_OPEN_SUCCESS)
		{
			RefreshPowerPillarInfoDataList(resp.PillarInfo);
			RefreshGuildInfo(resp.GuildInfo);
			SendSocketPowerPillarChangedEvent();
			Action<Code> onOpenPowerPillarEvent = this.OnOpenPowerPillarEvent;
			if (onOpenPowerPillarEvent != null)
			{
				onOpenPowerPillarEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnOpenPowerPillarRes", code);
			Action<Code> onOpenPowerPillarEvent2 = this.OnOpenPowerPillarEvent;
			if (onOpenPowerPillarEvent2 != null)
			{
				onOpenPowerPillarEvent2(code);
			}
		}
	}

	private void OnClosePowerPillarRes(GuildPowerPillarCloseRes resp)
	{
		Debug.Log("[OnClosePowerPillarRes]");
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_ORE_CLOSE_SUCCESS)
		{
			RefreshPowerPillarInfoDataList(resp.PillarInfo);
			RefreshGuildInfo(resp.GuildInfo);
			SendSocketPowerPillarChangedEvent();
			Action<Code> onClosePowerPillarEvent = this.OnClosePowerPillarEvent;
			if (onClosePowerPillarEvent != null)
			{
				onClosePowerPillarEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnClosePowerPillarRes", code);
			Action<Code> onClosePowerPillarEvent2 = this.OnClosePowerPillarEvent;
			if (onClosePowerPillarEvent2 != null)
			{
				onClosePowerPillarEvent2(code);
			}
		}
	}

	private void OnGetOreInfoRes(GuildGetOreInfoRes resp)
	{
		Debug.Log("[OnGetOreInfoRes]");
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_GET_ORE_INFO_SUCCESS)
		{
			_oreInfoListCache = resp.OreInfo;
			RegenOreInfoDataList();
			Action onOreChangedEvent = this.OnOreChangedEvent;
			if (onOreChangedEvent != null)
			{
				onOreChangedEvent();
			}
			Action onGetOreInfoOnceEvent = this.OnGetOreInfoOnceEvent;
			if (onGetOreInfoOnceEvent != null)
			{
				onGetOreInfoOnceEvent();
			}
			this.OnGetOreInfoOnceEvent = null;
			Action<Code> onGetOreInfoEvent = this.OnGetOreInfoEvent;
			if (onGetOreInfoEvent != null)
			{
				onGetOreInfoEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnGetOreInfoRes", code);
			Action<Code> onGetOreInfoEvent2 = this.OnGetOreInfoEvent;
			if (onGetOreInfoEvent2 != null)
			{
				onGetOreInfoEvent2(code);
			}
		}
	}

	private void OnOreLevelUpRes(GuildOreLevelUpRes resp)
	{
		Debug.Log("[OnOreLevelUpRes]");
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_ORE_LEVEL_UP_SUCCESS)
		{
			RefreshGuildInfo(resp.GuildInfo);
			List<NetOreInfo> changedOreInfoList = resp.OreInfo.Except(_oreInfoListCache, _netOreInfoComparer).ToList();
			_oreInfoListCache = resp.OreInfo;
			RegenOreInfoDataList();
			SendSocketOreChangedEvent(changedOreInfoList);
			Action onOreChangedEvent = this.OnOreChangedEvent;
			if (onOreChangedEvent != null)
			{
				onOreChangedEvent();
			}
			Action<Code> onOreLevelUpEvent = this.OnOreLevelUpEvent;
			if (onOreLevelUpEvent != null)
			{
				onOreLevelUpEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnOreLevelUpRes", code);
			Action<Code> onOreLevelUpEvent2 = this.OnOreLevelUpEvent;
			if (onOreLevelUpEvent2 != null)
			{
				onOreLevelUpEvent2(code);
			}
		}
	}

	private void OnRankupRes(GuildPowerTowerRankUpRes resp)
	{
		Debug.Log("[OnRankupRes]");
		Code code = (Code)resp.Code;
		if (code == Code.GUILD_GET_POWER_TOWER_RANK_UP_SUCCESS)
		{
			RefreshPowerPillarInfoDataList(resp.PillarInfo);
			RefreshGuildInfo(resp.GuildInfo);
			RegenOreInfoDataList();
			SendSocketPowerTowerRankupEvent();
			Action onOreChangedEvent = this.OnOreChangedEvent;
			if (onOreChangedEvent != null)
			{
				onOreChangedEvent();
			}
			Action<Code> onRankupEvent = this.OnRankupEvent;
			if (onRankupEvent != null)
			{
				onRankupEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnRankupRes", code);
			Action<Code> onRankupEvent2 = this.OnRankupEvent;
			if (onRankupEvent2 != null)
			{
				onRankupEvent2(code);
			}
		}
	}

	private void SendSocketPowerTowerRankupEvent()
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		GuildEventPowerTowerRankup data = new GuildEventPowerTowerRankup
		{
			PowerTowerRank = guildInfoCache.PowerTower,
			Score = guildInfoCache.Score,
			Money = guildInfoCache.Money,
			PillarInfoDataList = PowerPillarInfoDataListCache
		};
		SendSocketPowerTowerEvent(data);
	}

	private void SendSocketPowerPillarChangedEvent()
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		GuildEventPowerPillarChanged data = new GuildEventPowerPillarChanged
		{
			Money = guildInfoCache.Money,
			PillarInfoDataList = PowerPillarInfoDataListCache
		};
		SendSocketPowerTowerEvent(data);
	}

	private void SendSocketOreChangedEvent(List<NetOreInfo> changedOreInfoList)
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		GuildEventOreChanged data = new GuildEventOreChanged
		{
			Money = guildInfoCache.Money,
			ChangedOreInfoList = changedOreInfoList
		};
		SendSocketPowerTowerEvent(data);
	}

	private void SendSocketPowerTowerEvent(GuildEventDataBase data)
	{
		string eventJSON = JsonHelper.Serialize(data);
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGuildEvent(string.Empty, eventJSON));
	}

	private void OnSocketPowerTowerEvent(object obj)
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
		case SocketGuildEventType.PowerTowerRankup:
		{
			GuildEventPowerTowerRankup guildEventPowerTowerRankup = JsonHelper.Deserialize<GuildEventPowerTowerRankup>(eventJSON);
			NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
			guildInfoCache.PowerTower = guildEventPowerTowerRankup.PowerTowerRank;
			guildInfoCache.Score = guildEventPowerTowerRankup.Score;
			guildInfoCache.Money = guildEventPowerTowerRankup.Money;
			PowerPillarInfoDataListCache = guildEventPowerTowerRankup.PillarInfoDataList;
			PowerPillarInfoDataListCache.ForEach(delegate(PowerPillarInfoData pillarInfoData)
			{
				pillarInfoData.RefreshOreInfo();
			});
			RegenOreInfoDataList();
			Action onSocketPowerTowerRankupEvent = this.OnSocketPowerTowerRankupEvent;
			if (onSocketPowerTowerRankupEvent != null)
			{
				onSocketPowerTowerRankupEvent();
			}
			break;
		}
		case SocketGuildEventType.PowerPillarChanged:
		{
			GuildEventPowerPillarChanged guildEventPowerPillarChanged = JsonHelper.Deserialize<GuildEventPowerPillarChanged>(eventJSON);
			Singleton<GuildSystem>.Instance.GuildInfoCache.Money = guildEventPowerPillarChanged.Money;
			PowerPillarInfoDataListCache = guildEventPowerPillarChanged.PillarInfoDataList;
			PowerPillarInfoDataListCache.ForEach(delegate(PowerPillarInfoData pillarInfoData)
			{
				pillarInfoData.RefreshOreInfo();
			});
			Action onSocketPowerPillarChangedEvent = this.OnSocketPowerPillarChangedEvent;
			if (onSocketPowerPillarChangedEvent != null)
			{
				onSocketPowerPillarChangedEvent();
			}
			break;
		}
		case SocketGuildEventType.OreChanged:
		{
			GuildEventOreChanged guildEventOreChanged = JsonHelper.Deserialize<GuildEventOreChanged>(eventJSON);
			Singleton<GuildSystem>.Instance.GuildInfoCache.Money = guildEventOreChanged.Money;
			foreach (NetOreInfo changedOreInfo in guildEventOreChanged.ChangedOreInfoList)
			{
				NetOreInfo value;
				if (_oreInfoListCache.TryGetValue((NetOreInfo info) => info.OreGroup == changedOreInfo.OreGroup, out value))
				{
					value.OreLevel = changedOreInfo.OreLevel;
				}
				else
				{
					_oreInfoListCache.Add(changedOreInfo);
				}
			}
			RegenOreInfoDataList();
			Action onSocketOreChangedEvent = this.OnSocketOreChangedEvent;
			if (onSocketOreChangedEvent != null)
			{
				onSocketOreChangedEvent();
			}
			break;
		}
		}
	}

	private void OnLeaveGuildEvent(Code ackCode)
	{
		if ((uint)(ackCode - 105300) <= 2u)
		{
			ClearCacheData();
		}
	}

	private void OnRemoveGuildEvent(Code ackCode)
	{
		if (ackCode == Code.GUILD_REMOVE_SUCCESS)
		{
			ClearCacheData();
		}
	}

	private void OnSocketGuildRemoveEvent()
	{
		ClearCacheData();
	}

	private void OnSocketMemberKickedEvent(string memberId, bool isSelf)
	{
		if (isSelf)
		{
			ClearCacheData();
		}
	}

	public bool IsChallengeMode(STAGE_TABLE stageTable)
	{
		if (stageTable != null)
		{
			int n_TYPE = stageTable.n_TYPE;
			if (n_TYPE != 5 && n_TYPE == 6)
			{
				return stageTable.n_DIFFICULTY >= 3;
			}
		}
		return ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 1;
	}

	public List<PowerPillarInfoData> GetEffectivePowerPillarInfoDataList(STAGE_TABLE stageTable)
	{
		int stageType = ((stageTable != null) ? stageTable.n_TYPE : 0);
		DateTime nowTime = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC;
		return PowerPillarInfoDataListCache.Where((PowerPillarInfoData pillarInfo) => pillarInfo.OreInfo != null && pillarInfo.OreInfo.AffectedStageType.Contains(stageType) && (pillarInfo.OreInfo.EnableOnChallengeMode || !IsChallengeMode(stageTable)) && pillarInfo.ExpireTime > nowTime).ToList();
	}

	private void RefreshGuildInfo(NetGuildInfo newGuildInfo)
	{
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		guildInfoCache.PowerTower = newGuildInfo.PowerTower;
		guildInfoCache.Money = newGuildInfo.Money;
		guildInfoCache.Score = newGuildInfo.Score;
	}

	private void RefreshPowerPillarInfoDataList(List<NetPowerPillarInfo> powerPillarInfoList)
	{
		DateTime nowTime = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC;
		PowerPillarInfoDataListCache = powerPillarInfoList.Select((NetPowerPillarInfo pillarInfo) => new PowerPillarInfoData(nowTime, pillarInfo)).ToList();
		Action onPowerPillarChangedEvent = this.OnPowerPillarChangedEvent;
		if (onPowerPillarChangedEvent != null)
		{
			onPowerPillarChangedEvent();
		}
	}

	private void RegenOreInfoDataList()
	{
		OreInfoDataListCache.Clear();
		Dictionary<int, Dictionary<int, OreInfoData>>.Enumerator enumerator = OreInfoDataDict.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, Dictionary<int, OreInfoData>> current = enumerator.Current;
			int group = current.Key;
			NetOreInfo netOreInfo = _oreInfoListCache.FirstOrDefault((NetOreInfo data) => data.OreGroup == group);
			int num = ((netOreInfo == null) ? 1 : netOreInfo.OreLevel);
			OreInfoData value;
			if (current.Value.TryGetValue(num, out value))
			{
				if (Singleton<GuildSystem>.Instance.GuildInfoCache.PowerTower >= value.AttrData.n_ORE_VALUE)
				{
					OreInfoDataListCache.Add(value);
				}
				else if (netOreInfo != null)
				{
					Debug.LogError(string.Format("Invalid NetOreInfo {0}, Value : {1} > PowerTower Rank : {2}", value.ID, value.AttrData.n_ORE_VALUE, Singleton<GuildSystem>.Instance.GuildInfoCache.PowerTower));
				}
			}
			else
			{
				Debug.LogError(string.Format("No OreInfoData of Group : {0}, Level : {1}", group, num));
			}
		}
		enumerator.Dispose();
	}

	private void HandleErrorCode(string method, int ackCode)
	{
		HandleErrorCode(method, (Code)ackCode);
	}

	private void HandleErrorCode(string method, Code ackCode)
	{
		switch (ackCode)
		{
		case Code.GUILD_NOT_ENOUGHT_MATERIAL:
			CommonUIHelper.ShowCommonTipUI("GUILD_MONEY_LACK");
			break;
		case Code.GUILD_CHECK_HEADER_POWER_FAIL:
			CommonUIHelper.ShowCommonTipUI("GUILD_PERMISSION_REVOKED");
			break;
		case Code.GUILD_PILLAR_OPEN_ORE_LEVEL_ERROR:
		case Code.GUILD_PILLAR_USED_ERROR:
		case Code.GUILD_ORE_OPENING_ERROR:
		case Code.GUILD_PILLAR_NOT_USED_ERROR:
		case Code.GUILD_ORE_NOT_OPENING_ERROR:
			Debug.Log(string.Format("[{0}] Customs Handling Error Code : {1}", method, ackCode));
			break;
		default:
			Debug.LogError(string.Format("[{0}] Unhandled Error Code : {1}", method, ackCode));
			break;
		}
	}
}
