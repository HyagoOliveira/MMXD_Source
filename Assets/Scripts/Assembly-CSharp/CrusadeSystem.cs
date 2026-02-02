#define RELEASE
using System;
using System.Collections.Generic;
using OrangeApi;

public class CrusadeSystem : Singleton<CrusadeSystem>
{
	private NetCrusadePlayerInfo _playerInfoCache;

	public CrusadeBossTarget LastTargetBoss;

	public int LastTargetGuardianIndex;

	public int LastSelectedGuardianBuffIndex;

	public int PassiveSkillID;

	public int EventID { get; private set; }

	public int EventStartTime { get; private set; }

	public int EventEndTime { get; private set; }

	public bool CanPlayThisEvent { get; private set; }

	public bool HasEvent
	{
		get
		{
			return EventID > 0;
		}
	}

	public NetCrusadePlayerInfo PlayerInfoCache
	{
		get
		{
			return _playerInfoCache;
		}
		private set
		{
			_playerInfoCache = value;
			Action onPlayerInfoCacheUpdateEvent = this.OnPlayerInfoCacheUpdateEvent;
			if (onPlayerInfoCacheUpdateEvent != null)
			{
				onPlayerInfoCacheUpdateEvent();
			}
		}
	}

	public NetCrusadeBossInfo BossInfoCache { get; private set; }

	public List<NetCrusadeGuardInfo> GuardInfoCacheList { get; private set; } = new List<NetCrusadeGuardInfo>();


	public List<NetCrusadeBattleRecord> BattleRecordListCache { get; private set; } = new List<NetCrusadeBattleRecord>();


	public List<NetCrusadeOnTimeRecord> OnTimeRecordListCache { get; private set; } = new List<NetCrusadeOnTimeRecord>();


	public NetRewardsEntity RewardEntityCache { get; private set; }

	public event Action<Code> OnRetrieveCrusadeInfoEvent;

	public event Action OnRetrieveCrusadeInfoOnceEvent;

	public event Action OnCheckCrusadeInfoEvent;

	public event Action OnCrusadeStartEvent;

	public event Action<CrusadeEndRes> OnCrusadeEndOnceEvent;

	public event Action OnCrusadeEndEvent;

	public event Action<NetChargeInfo> OnAddChallengeCountEvent;

	public event Action<int, CrusadeEventRankingInfo> OnRetrievePersonalEventRankingEvent;

	public event Action<int, List<CrusadeEventRankingInfo>> OnRetrieveEventRankingEvent;

	public event Action OnPlayerInfoCacheUpdateEvent;

	public void RetrieveCrusadeInfo()
	{
		ManagedSingleton<PlayerNetManager>.Instance.CrusadeRetrieveCrusadeInfo(OnRetrieveCrusadeInfoRes);
	}

	public void CheckCrusadeInfo()
	{
		ManagedSingleton<PlayerNetManager>.Instance.CrusadeCheckCrusadeInfo(OnCheckCrusadeInfoRes);
	}

	public void StartCrusade(int stageId)
	{
		ManagedSingleton<PlayerNetManager>.Instance.CrusadeStart(stageId, OnCrusadeStartRes);
	}

	public void EndCrusade(int stageId, sbyte result, sbyte star, int score, int power, int duration, short resolutionW, short resolutionH)
	{
		ManagedSingleton<PlayerNetManager>.Instance.CrusadeEnd(stageId, result, star, score, power, duration, resolutionW, resolutionH, OnCrusadeEndRes);
	}

	public void AddChallengeCount()
	{
		ManagedSingleton<PlayerNetManager>.Instance.CrusadeLimitResetReq(OnCrusadeLimitResetRes);
	}

	public void RetrievePersonalEventRanking(int eventId)
	{
		ManagedSingleton<PlayerNetManager>.Instance.CrusadeRetrievePersonalEventRankingReq(eventId, OnRetrievePersonalEventRankingRes);
	}

	public void RetrieveEventRanking(int eventId, int rankingStart = 1, int rankingEnd = 3)
	{
		ManagedSingleton<PlayerNetManager>.Instance.CrusadeRetrieveEventRankingReq(eventId, rankingStart, rankingEnd, OnRetrieveEventRankingRes);
	}

	private void OnRetrieveCrusadeInfoRes(RetrieveCrusadeInfoRes res)
	{
		Code code = (Code)res.Code;
		switch (code)
		{
		case Code.CRUSADE_GET_INFO_SUCCESS:
		{
			EventID = res.EventID;
			EventStartTime = res.StartTime;
			EventEndTime = res.EndTime;
			PlayerInfoCache = res.CrusadePlayerInfo;
			BossInfoCache = res.CrusadeBossInfo;
			GuardInfoCacheList = res.CrusadeGuardInfoList ?? new List<NetCrusadeGuardInfo>();
			BattleRecordListCache = res.CrusadeBattleRecordList ?? new List<NetCrusadeBattleRecord>();
			OnTimeRecordListCache = res.CrusadeOnTimeRecordList ?? new List<NetCrusadeOnTimeRecord>();
			RewardEntityCache = res.RewardEntities;
			Debug.Log(string.Format("res.RegisterGuildID = {0}", res.RegisterGuildID));
			Debug.Log(string.Format("HasGuild = {0}, GuildID = {1}", Singleton<GuildSystem>.Instance.HasGuild, Singleton<GuildSystem>.Instance.GuildId));
			CanPlayThisEvent = Singleton<GuildSystem>.Instance.HasGuild && res.RegisterGuildID == Singleton<GuildSystem>.Instance.GuildId;
			Action<Code> onRetrieveCrusadeInfoEvent2 = this.OnRetrieveCrusadeInfoEvent;
			if (onRetrieveCrusadeInfoEvent2 != null)
			{
				onRetrieveCrusadeInfoEvent2(code);
			}
			Action onRetrieveCrusadeInfoOnceEvent = this.OnRetrieveCrusadeInfoOnceEvent;
			if (onRetrieveCrusadeInfoOnceEvent != null)
			{
				onRetrieveCrusadeInfoOnceEvent();
			}
			this.OnRetrieveCrusadeInfoOnceEvent = null;
			break;
		}
		case Code.CRUSADE_EVENT_NO_OPEN_DATA:
		{
			EventID = 0;
			CanPlayThisEvent = false;
			Action<Code> onRetrieveCrusadeInfoEvent3 = this.OnRetrieveCrusadeInfoEvent;
			if (onRetrieveCrusadeInfoEvent3 != null)
			{
				onRetrieveCrusadeInfoEvent3(code);
			}
			Action onRetrieveCrusadeInfoOnceEvent2 = this.OnRetrieveCrusadeInfoOnceEvent;
			if (onRetrieveCrusadeInfoOnceEvent2 != null)
			{
				onRetrieveCrusadeInfoOnceEvent2();
			}
			this.OnRetrieveCrusadeInfoOnceEvent = null;
			break;
		}
		default:
		{
			HandleErrorCode("OnRetrieveCrusadeInfoRes", code);
			Action<Code> onRetrieveCrusadeInfoEvent = this.OnRetrieveCrusadeInfoEvent;
			if (onRetrieveCrusadeInfoEvent != null)
			{
				onRetrieveCrusadeInfoEvent(code);
			}
			break;
		}
		}
	}

	private void OnCheckCrusadeInfoRes(CheckCrusadeInfoRes res)
	{
		Code code = (Code)res.Code;
		if (code == Code.CRUSADE_CHECK_INFO_SUCCESS)
		{
			BossInfoCache = res.CrusadeBossInfo;
			GuardInfoCacheList = res.CrusadeGuardInfoList ?? new List<NetCrusadeGuardInfo>();
			Action onCheckCrusadeInfoEvent = this.OnCheckCrusadeInfoEvent;
			if (onCheckCrusadeInfoEvent != null)
			{
				onCheckCrusadeInfoEvent();
			}
		}
		else
		{
			HandleErrorCode("OnCheckCrusadeInfoRes", code);
		}
	}

	private void OnCrusadeStartRes(CrusadeStartRes res)
	{
		Code code = (Code)res.Code;
		if (code == Code.CRUSADE_START_SUCCESS)
		{
			Action onCrusadeStartEvent = this.OnCrusadeStartEvent;
			if (onCrusadeStartEvent != null)
			{
				onCrusadeStartEvent();
			}
		}
		else
		{
			HandleErrorCode("OnCrusadeStartRes", code);
		}
	}

	private void OnCrusadeEndRes(CrusadeEndRes res)
	{
		Code code = (Code)res.Code;
		if (code == Code.CRUSADE_END_SUCCESS)
		{
			PlayerInfoCache = res.CrusadePlayerInfo;
			BossInfoCache = res.CrusadeBossInfo;
			GuardInfoCacheList = res.CrusadeGuardInfoList ?? new List<NetCrusadeGuardInfo>();
			BattleRecordListCache = res.CrusadeBattleRecordList ?? new List<NetCrusadeBattleRecord>();
			OnTimeRecordListCache = res.CrusadeOnTimeRecordList ?? new List<NetCrusadeOnTimeRecord>();
			RewardEntityCache = res.RewardEntities;
			Action onCrusadeEndEvent = this.OnCrusadeEndEvent;
			if (onCrusadeEndEvent != null)
			{
				onCrusadeEndEvent();
			}
			Action<CrusadeEndRes> onCrusadeEndOnceEvent = this.OnCrusadeEndOnceEvent;
			if (onCrusadeEndOnceEvent != null)
			{
				onCrusadeEndOnceEvent(res);
			}
			this.OnCrusadeEndOnceEvent = null;
		}
		else
		{
			HandleErrorCode("OnCrusadeEndRes", code);
		}
	}

	private void OnCrusadeLimitResetRes(CrusadeLimitResetRes res)
	{
		Code code = (Code)res.Code;
		if (code == Code.CRUSADE_BUY_COUNT_SUCCESS)
		{
			PlayerInfoCache = res.CrusadePlayerInfo;
			Action<NetChargeInfo> onAddChallengeCountEvent = this.OnAddChallengeCountEvent;
			if (onAddChallengeCountEvent != null)
			{
				onAddChallengeCountEvent(res.ChargeInfo);
			}
		}
		else
		{
			HandleErrorCode("OnCrusadeLimitResetRes", code);
		}
	}

	private void OnRetrievePersonalEventRankingRes(RetrievePersonalCrusadeEventRankingRes res)
	{
		Code code = (Code)res.Code;
		if (code == Code.CRUSADE_GET_RANKING_SUCCESS)
		{
			Action<int, CrusadeEventRankingInfo> onRetrievePersonalEventRankingEvent = this.OnRetrievePersonalEventRankingEvent;
			if (onRetrievePersonalEventRankingEvent != null)
			{
				onRetrievePersonalEventRankingEvent(res.EventID, res.CrusadeEventRanking);
			}
		}
		else
		{
			HandleErrorCode("OnRetrievePersonalEventRankingRes", code);
		}
	}

	private void OnRetrieveEventRankingRes(RetrieveCrusadeEventRankingRes res)
	{
		Code code = (Code)res.Code;
		if (code == Code.CRUSADE_GET_RANKING_SUCCESS)
		{
			Action<int, List<CrusadeEventRankingInfo>> onRetrieveEventRankingEvent = this.OnRetrieveEventRankingEvent;
			if (onRetrieveEventRankingEvent != null)
			{
				onRetrieveEventRankingEvent(res.EventID, res.CrusadeEventRankingList ?? new List<CrusadeEventRankingInfo>());
			}
		}
		else
		{
			HandleErrorCode("OnRetrieveEventRankingRes", code);
		}
	}

	public bool CheckInEventTime()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		if (EventStartTime <= serverUnixTimeNowUTC)
		{
			return serverUnixTimeNowUTC <= EventEndTime;
		}
		return false;
	}

	public int GetCrusadeBonus()
	{
		int num = 0;
		foreach (CharacterInfo value4 in ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Values)
		{
			NetCharacterInfo netInfo = value4.netInfo;
			CHARACTER_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(netInfo.CharacterID, out value))
			{
				switch (value.n_RARITY)
				{
				case 3:
					num += netInfo.Star * OrangeConst.GUILD_CRUSADE_ADDCHARA_B;
					break;
				case 4:
					num += netInfo.Star * OrangeConst.GUILD_CRUSADE_ADDCHARA_A;
					break;
				case 5:
					num += netInfo.Star * OrangeConst.GUILD_CRUSADE_ADDCHARA_S;
					break;
				}
			}
		}
		foreach (WeaponInfo value5 in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Values)
		{
			NetWeaponInfo netInfo2 = value5.netInfo;
			WEAPON_TABLE value2;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(netInfo2.WeaponID, out value2))
			{
				switch (value2.n_RARITY)
				{
				case 2:
					num += netInfo2.Star * OrangeConst.GUILD_CRUSADE_ADDWEAPON_C;
					break;
				case 3:
					num += netInfo2.Star * OrangeConst.GUILD_CRUSADE_ADDWEAPON_B;
					break;
				case 4:
					num += netInfo2.Star * OrangeConst.GUILD_CRUSADE_ADDWEAPON_A;
					break;
				case 5:
					num += netInfo2.Star * OrangeConst.GUILD_CRUSADE_ADDWEAPON_S;
					break;
				}
			}
		}
		foreach (CardInfo value6 in ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values)
		{
			NetCardInfo netCardInfo = value6.netCardInfo;
			CARD_TABLE value3;
			if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netCardInfo.CardID, out value3))
			{
				switch (value3.n_RARITY)
				{
				case 3:
					num += netCardInfo.Star * OrangeConst.GUILD_CRUSADE_ADDCARD_B;
					break;
				case 4:
					num += netCardInfo.Star * OrangeConst.GUILD_CRUSADE_ADDCARD_A;
					break;
				case 5:
					num += netCardInfo.Star * OrangeConst.GUILD_CRUSADE_ADDCARD_S;
					break;
				}
			}
		}
		return num;
	}

	private void HandleErrorCode(string method, int ackCode)
	{
		HandleErrorCode(method, (Code)ackCode);
	}

	private void HandleErrorCode(string method, Code ackCode)
	{
		Debug.LogError(string.Format("[{0}] Unhandled Error Code : {1}", method, ackCode));
	}
}
