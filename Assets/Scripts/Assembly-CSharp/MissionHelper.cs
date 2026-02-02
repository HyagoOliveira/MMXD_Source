using System;
using System.Collections.Generic;
using System.Linq;
using enums;

public class MissionHelper : ManagedSingleton<MissionHelper>
{
	public enum MissionStatus
	{
		RUNNING = 1,
		DONE = 2,
		ALL = 3
	}

	private class DummyMissionInfo : MissionInfo
	{
	}

	private MultiMap<MissionSubType, MISSION_TABLE> mmapPresetAchievementByMissionSubType = new MultiMap<MissionSubType, MISSION_TABLE>();

	private List<MISSION_TABLE> listTemporaryMissionDataForUIView = new List<MISSION_TABLE>();

	private int currentMonthlyCounterID;

	private readonly string nullStr = "null";

	public int FillteredDataCount
	{
		get
		{
			return listTemporaryMissionDataForUIView.Count;
		}
		private set
		{
		}
	}

	public bool DisplayHint
	{
		get
		{
			bool flag = ManagedSingleton<PlayerHelper>.Instance.GetLV() >= OrangeConst.OPENRANK_DAILY_MISSION;
			List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(EventType.EVENT_MISSION, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
			foreach (MISSION_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values)
			{
				if (!ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(value.s_BEGIN_TIME, value.s_END_TIME, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC) || !ManagedSingleton<OrangeTableHelper>.Instance.IsTimeAfterDate(value.s_CREATE_TIME, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PlayerCreateTime) || (ManagedSingleton<PlayerHelper>.Instance.GetLV() > value.n_END_RANK && !CheckMissionCompleted(value.n_ID)))
				{
					continue;
				}
				switch ((MissionType)(short)value.n_TYPE)
				{
				case MissionType.Daily:
					if (flag && !CheckMissionRewardRetrieved(value.n_ID) && CheckMissionCompleted(value.n_ID))
					{
						return true;
					}
					break;
				case MissionType.Weekly:
				case MissionType.Achievement:
					if (!CheckMissionRewardRetrieved(value.n_ID) && CheckMissionCompleted(value.n_ID))
					{
						return true;
					}
					break;
				case MissionType.Activity:
					foreach (EVENT_TABLE item in eventTableByType)
					{
						if (item.n_TYPE == 9 && item.n_TYPE_X == value.n_SUB_TYPE && !CheckMissionRewardRetrieved(value.n_ID) && CheckMissionCompleted(value.n_ID))
						{
							return true;
						}
					}
					break;
				case MissionType.MonthActivity:
					if ((value.n_CONDITION_X != 1 || CurrentMontlyActivityPaid) && !CheckMissionRewardRetrieved(value.n_ID) && CheckMissionCompleted(value.n_ID))
					{
						return true;
					}
					break;
				}
			}
			return false;
		}
	}

	public bool DisplayDailySuggest
	{
		get
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() < OrangeConst.OPENRANK_DAILY_MISSION)
			{
				return false;
			}
			foreach (MISSION_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values)
			{
				if (value.n_TYPE == 1 && ManagedSingleton<PlayerHelper>.Instance.GetLV() < value.n_END_RANK && ManagedSingleton<OrangeTableHelper>.Instance.IsTimeAfterDate(value.s_CREATE_TIME, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PlayerCreateTime) && !CheckMissionCompleted(value.n_ID))
				{
					return true;
				}
			}
			return false;
		}
	}

	public int CurrentMonthlyActivityCounterID
	{
		get
		{
			if (currentMonthlyCounterID != 0)
			{
				return currentMonthlyCounterID;
			}
			foreach (MISSION_TABLE item in ManagedSingleton<OrangeTableHelper>.Instance.GetMissionTableByType(MissionType.MonthActivity, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
			{
				if (item.n_COUNTER != 0)
				{
					currentMonthlyCounterID = item.n_COUNTER;
					break;
				}
			}
			return currentMonthlyCounterID;
		}
	}

	public int CurrentMonthlyActivityValue
	{
		get
		{
			return GetMissionCounter(CurrentMonthlyActivityCounterID);
		}
	}

	public bool CurrentMontlyActivityPaid
	{
		get
		{
			EVENT_TABLE eventTableByCounter = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByCounter(CurrentMonthlyActivityCounterID);
			if (eventTableByCounter == null)
			{
				return false;
			}
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(eventTableByCounter.s_BEGIN_TIME, eventTableByCounter.s_END_TIME, serverUnixTimeNowUTC))
			{
				return false;
			}
			ShopInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicShop.TryGetValue(eventTableByCounter.n_TYPE_X, out value))
			{
				return !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(value.netShopRecord.LastShopTime, ResetRule.MonthlyReset);
			}
			return false;
		}
	}

	public override void Initialize()
	{
		listTemporaryMissionDataForUIView.Clear();
		mmapPresetAchievementByMissionSubType.Clear();
		foreach (MISSION_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values)
		{
			if (value.n_TYPE == 3 && value.n_SUB_TYPE != 0)
			{
				mmapPresetAchievementByMissionSubType.Add((MissionSubType)value.n_SUB_TYPE, value);
			}
		}
		currentMonthlyCounterID = 0;
	}

	public override void Reset()
	{
		base.Reset();
		Initialize();
		if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "hometop")
		{
			TurtorialUI.requiredTutorialItemID.Clear();
			TurtorialUI.ClearTutorialFlag();
		}
	}

	public override void Dispose()
	{
		listTemporaryMissionDataForUIView.Clear();
		mmapPresetAchievementByMissionSubType.Clear();
	}

	public MISSION_TABLE GetUIViewData(int idx)
	{
		if (listTemporaryMissionDataForUIView[idx] != null)
		{
			return listTemporaryMissionDataForUIView[idx];
		}
		return null;
	}

	public void CollectUIViewData(MissionType type, MissionStatus status, int subType)
	{
		List<MISSION_TABLE> missionDataByType = GetMissionDataByType(type);
		List<MISSION_TABLE> list = new List<MISSION_TABLE>();
		List<MISSION_TABLE> list2 = new List<MISSION_TABLE>();
		for (int num = missionDataByType.Count - 1; num >= 0; num--)
		{
			MISSION_TABLE mISSION_TABLE = missionDataByType[num];
			if (mISSION_TABLE == null)
			{
				missionDataByType.RemoveAt(num);
			}
			else if (!ManagedSingleton<OrangeTableHelper>.Instance.IsTimeAfterDate(mISSION_TABLE.s_CREATE_TIME, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PlayerCreateTime))
			{
				missionDataByType.RemoveAt(num);
			}
			else
			{
				bool flag = true;
				switch ((MissionOpenCondition)(short)mISSION_TABLE.n_OPEN_CONDITION)
				{
				case MissionOpenCondition.MissionComplete:
				{
					NetMissionInfo missionNetInfo = GetMissionNetInfo(mISSION_TABLE.n_OPEN_CONDITION_X);
					if (missionNetInfo == null || missionNetInfo.Received != 1)
					{
						flag = false;
					}
					break;
				}
				case MissionOpenCondition.LevelReach:
					if (ManagedSingleton<PlayerHelper>.Instance.GetLV() < mISSION_TABLE.n_OPEN_CONDITION_X)
					{
						flag = false;
					}
					break;
				case MissionOpenCondition.PassStage:
					if (!CheckStagePassed(mISSION_TABLE.n_OPEN_CONDITION_X))
					{
						flag = false;
					}
					break;
				}
				if (!flag)
				{
					missionDataByType.RemoveAt(num);
				}
				else
				{
					NetMissionInfo missionNetInfo2 = GetMissionNetInfo(mISSION_TABLE.n_ID);
					if (missionNetInfo2 != null)
					{
						missionDataByType.RemoveAt(num);
						if (missionNetInfo2.Received == 1)
						{
							list2.Add(mISSION_TABLE);
						}
						else
						{
							list.Add(mISSION_TABLE);
						}
					}
					else if (CheckMissionCompleted(mISSION_TABLE.n_ID))
					{
						missionDataByType.RemoveAt(num);
						list.Add(mISSION_TABLE);
					}
					else if (ManagedSingleton<PlayerHelper>.Instance.GetLV() > mISSION_TABLE.n_END_RANK && mISSION_TABLE.n_END_RANK != 0)
					{
						missionDataByType.RemoveAt(num);
					}
				}
			}
		}
		listTemporaryMissionDataForUIView.Clear();
		switch (status)
		{
		default:
			listTemporaryMissionDataForUIView.AddRange(list);
			listTemporaryMissionDataForUIView.AddRange(missionDataByType);
			listTemporaryMissionDataForUIView.AddRange(list2);
			break;
		case MissionStatus.RUNNING:
			listTemporaryMissionDataForUIView.AddRange(list);
			listTemporaryMissionDataForUIView.AddRange(missionDataByType);
			break;
		case MissionStatus.DONE:
			listTemporaryMissionDataForUIView.AddRange(list2);
			break;
		}
		if (type != MissionType.Achievement && type != MissionType.Activity)
		{
			return;
		}
		for (int num2 = listTemporaryMissionDataForUIView.Count - 1; num2 >= 0; num2--)
		{
			MISSION_TABLE mISSION_TABLE2 = listTemporaryMissionDataForUIView[num2];
			if (mISSION_TABLE2.n_SUB_TYPE != subType && mISSION_TABLE2.n_SUB_TYPE != 0)
			{
				listTemporaryMissionDataForUIView.RemoveAt(num2);
			}
		}
	}

	public int GetActivityValue(MissionType type, MissionSubType subType)
	{
		int num = 0;
		foreach (KeyValuePair<int, MissionInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicMission)
		{
			if (ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.ContainsKey(item.Value.netMissionInfo.MissionID))
			{
				MISSION_TABLE mISSION_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT[item.Value.netMissionInfo.MissionID];
				if (mISSION_TABLE != null && mISSION_TABLE.n_TYPE == (int)type && mISSION_TABLE.n_SUB_TYPE == (int)subType && item.Value.netMissionInfo.Received == 1)
				{
					num += mISSION_TABLE.n_ACTIVITY;
				}
			}
		}
		return num;
	}

	public int GetActivityValue(MissionType type)
	{
		int num = 0;
		foreach (KeyValuePair<int, MissionInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicMission)
		{
			if (ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.ContainsKey(item.Value.netMissionInfo.MissionID))
			{
				MISSION_TABLE mISSION_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT[item.Value.netMissionInfo.MissionID];
				if (mISSION_TABLE != null && mISSION_TABLE.n_TYPE == (int)type && item.Value.netMissionInfo.Received == 1)
				{
					num += mISSION_TABLE.n_ACTIVITY;
				}
			}
		}
		return num;
	}

	public int GetBossRushScore(int eventID)
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.dicBossRushInfo.ContainsKey(eventID))
		{
			return ManagedSingleton<PlayerNetManager>.Instance.dicBossRushInfo[eventID].netBRInfo.Score;
		}
		return 0;
	}

	public bool CheckMissionCompleted(int missionID)
	{
		if (CheckPreMissionPassed(missionID))
		{
			if (GetMissionProgressCount(missionID) < GetMissionProgressTotalCount(missionID))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CheckMissionCompletedLong(int missionID)
	{
		if (CheckPreMissionPassed(missionID))
		{
			if (GetMissionProgressCountLong(missionID) < GetMissionProgressTotalCount(missionID))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CheckPlayerCreateTime(int missionID)
	{
		if (ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.ContainsKey(missionID))
		{
			MISSION_TABLE mISSION_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT[missionID];
			if (mISSION_TABLE != null)
			{
				return ManagedSingleton<OrangeTableHelper>.Instance.IsTimeAfterDate(mISSION_TABLE.s_CREATE_TIME, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PlayerCreateTime);
			}
		}
		return false;
	}

	public int GetMissionProgressCount(int missionID)
	{
		MISSION_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.TryGetValue(missionID, out value);
		if (value != null)
		{
			if (value.n_COUNTER > 0)
			{
				return GetMissionCounter(value.n_COUNTER);
			}
			switch ((MissionCondition)(short)value.n_CONDITION)
			{
			case MissionCondition.SpecificStageCompleted:
				if (!CheckStagePassed(value.n_CONDITION_X))
				{
					return 0;
				}
				return 1;
			case MissionCondition.PlayerLevelReached:
				return ManagedSingleton<PlayerHelper>.Instance.GetLV();
			case MissionCondition.BattlePowerReached:
				return ManagedSingleton<PlayerHelper>.Instance.GetBattlePower();
			case MissionCondition.ArenaRankingReached:
			case MissionCondition.EnhanceWeaponCountReached:
				return GetWeaponEnhanceCount(value.n_CONDITION_Y);
			case MissionCondition.EnhanceWeaponExpertCountReached:
				return GetWeaponExpertCount(value.n_CONDITION_Y);
			case MissionCondition.ChipLevelReached:
				return GetChipEnhanceCount(value.n_CONDITION_Y);
			case MissionCondition.ChipAnalysisLevelReached:
				return GetChipAnalyzeCount(value.n_CONDITION_Y);
			case MissionCondition.WeaponTypeCollectionReached:
				return GetWeaponTypeCount(value.n_CONDITION_Y);
			case MissionCondition.CharacterCollectionReached:
				return ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Count;
			case MissionCondition.ChipCollectionReached:
				return ManagedSingleton<PlayerNetManager>.Instance.dicChip.Count;
			case MissionCondition.SecretStageStartCollectionReached:
				return GetTotalSecretStageCount();
			case MissionCondition.ActivityReached:
				return GetActivityValue(MissionType.Daily);
			case MissionCondition.StageClearDifficultyAccumulated:
			case MissionCondition.ActivityAccumulated:
				return GetMissionCounter(value.n_COUNTER);
			case MissionCondition.BossRushScoreReached:
				return GetBossRushScore(value.n_CONDITION_X);
			case MissionCondition.RaidBossScoreReached:
				return ManagedSingleton<PlayerNetManager>.Instance.nRaidBossSocre;
			case MissionCondition.TWSuppressScoreReached:
				return ManagedSingleton<PlayerNetManager>.Instance.nTWSuppressScore;
			case MissionCondition.TWLightningScoreReached:
				return ManagedSingleton<PlayerNetManager>.Instance.nTWLightningScore;
			case MissionCondition.TWCrusadeScoreReached:
				return ManagedSingleton<PlayerNetManager>.Instance.nTWCrusadeScore;
			case MissionCondition.TAStageRankReached:
				return GetMissionCounter(value.n_COUNTER);
			case MissionCondition.CrusadeScoreReached:
				return 0;
			case MissionCondition.ScenarioStageStarReached:
			case MissionCondition.BossChallengeStageStarReached:
			case MissionCondition.TeamUpStageStarReached:
				return GetSeriesStageTotalStar(value.n_CONDITION_X);
			case MissionCondition.CharacterStarReached:
				return GetCharaterStar(value.n_CONDITION_X, value.n_CONDITION_Y);
			case MissionCondition.StageClearStarReached:
				return GetStagePassedAndStar(value.n_CONDITION_X, value.n_CONDITION_Y);
			case MissionCondition.WeaponTypeRarityTotalStarReached:
				return GetWeaponTypeRarityTotalStar(value.n_CONDITION_X, value.n_CONDITION_Y, value.n_CONDITION_Z);
			case MissionCondition.CardRarityStarReached:
				return GetCardRarityStar(value.n_CONDITION_X, value.n_CONDITION_Y, value.n_CONDITION_Z);
			case MissionCondition.CardRarityLevelReached:
				return GetCardRarityLevel(value.n_CONDITION_X, value.n_CONDITION_Y, value.n_CONDITION_Z);
			case MissionCondition.CardTypeReached:
				return GetCardType(value.n_CONDITION_X, value.n_CONDITION_Y);
			case MissionCondition.SkillResearchLevelReached:
				return GetFinalStrikeTotalLevel(value.n_CONDITION_X);
			case MissionCondition.BenchLevelReached:
				return GetBackupWeaponSlotTotalLevel(value.n_CONDITION_X);
			case MissionCondition.CharacterAndWeaponAndCardGalleryLevelReached:
				if (!CheckCharacterAndWeaponAndCardGalleryLevel(value.n_CONDITION_X, value.n_CONDITION_Y, value.n_CONDITION_Z))
				{
					return 0;
				}
				return 1;
			case MissionCondition.WeaponStarAndExpertReached:
				return GetWeaponStarAndExpert(value.n_CONDITION_X, value.n_CONDITION_Y, value.n_CONDITION_Z);
			}
		}
		return 0;
	}

	public long GetMissionProgressCountLong(int missionID)
	{
		MISSION_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.TryGetValue(missionID, out value))
		{
			return 0L;
		}
		if (value.n_COUNTER > 0)
		{
			return GetMissionCounter(value.n_COUNTER);
		}
		MissionCondition missionCondition = (MissionCondition)value.n_CONDITION;
		if (missionCondition == MissionCondition.CrusadeScoreReached)
		{
			if (Singleton<CrusadeSystem>.Instance.PlayerInfoCache == null)
			{
				return 0L;
			}
			return Singleton<CrusadeSystem>.Instance.PlayerInfoCache.Score;
		}
		return 0L;
	}

	public int GetMissionProgressTotalCount(int missionID)
	{
		if (ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.ContainsKey(missionID))
		{
			MISSION_TABLE mISSION_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT[missionID];
			if (mISSION_TABLE != null)
			{
				switch ((MissionCondition)(short)mISSION_TABLE.n_CONDITION)
				{
				case MissionCondition.SpecificStageCompleted:
				case MissionCondition.StageClearStarReached:
				case MissionCondition.CharacterAndWeaponAndCardGalleryLevelReached:
				case MissionCondition.TAStageRankReached:
					return 1;
				case MissionCondition.PlayerLevelReached:
				case MissionCondition.BattlePowerReached:
				case MissionCondition.ArenaRankingReached:
				case MissionCondition.TotalPlayDaysAccumulated:
				case MissionCondition.ConstantlyLoginAccumulated:
				case MissionCondition.EnhanceWeaponCountReached:
				case MissionCondition.EnhanceWeaponExpertCountReached:
				case MissionCondition.ChipLevelReached:
				case MissionCondition.ChipAnalysisLevelReached:
				case MissionCondition.WeaponTypeCollectionReached:
				case MissionCondition.CharacterCollectionReached:
				case MissionCondition.ChipCollectionReached:
				case MissionCondition.SecretStageStartCollectionReached:
				case MissionCondition.CardRarityStarReached:
				case MissionCondition.CardRarityLevelReached:
				case MissionCondition.SkillResearchLevelReached:
				case MissionCondition.BenchLevelReached:
				case MissionCondition.ResearchAccumulated:
				case MissionCondition.ArenaAccumulated:
				case MissionCondition.PVPAccumulated:
				case MissionCondition.PVPWinAccumulated:
				case MissionCondition.GachaAccumulated:
				case MissionCondition.GiveActionPointAccumulated:
				case MissionCondition.ConsumeJewelAccumulated:
				case MissionCondition.CompletedDailyMissionAccumulated:
				case MissionCondition.KillEnemyAccumulated:
					return mISSION_TABLE.n_CONDITION_X;
				case MissionCondition.WeaponTypeStageClearAccumulated:
				case MissionCondition.CharacterStarReached:
				case MissionCondition.CardTypeReached:
				case MissionCondition.StageClearAccumulated:
				case MissionCondition.StageClearDifficultyAccumulated:
				case MissionCondition.ActivityReached:
				case MissionCondition.GainItemAccumulated:
				case MissionCondition.SpecificStageCompletedAccumulated:
				case MissionCondition.SpecificStageWinAccumulated:
				case MissionCondition.ActivityAccumulated:
				case MissionCondition.BossRushScoreReached:
				case MissionCondition.RaidBossScoreReached:
				case MissionCondition.CrusadeScoreReached:
				case MissionCondition.SpecificGacahId:
				case MissionCondition.SpecificGacahGroup:
				case MissionCondition.MainStageClearReached:
				case MissionCondition.TWSuppressScoreReached:
				case MissionCondition.TWLightningScoreReached:
				case MissionCondition.TWCrusadeScoreReached:
				case MissionCondition.ScenarioStageStarReached:
				case MissionCondition.BossChallengeStageStarReached:
				case MissionCondition.TeamUpStageStarReached:
					return mISSION_TABLE.n_CONDITION_Y;
				case MissionCondition.WeaponTypeRarityTotalStarReached:
				case MissionCondition.WeaponStarAndExpertReached:
				case MissionCondition.RankAchievementReached:
				case MissionCondition.GachaCountRankingReached:
					return mISSION_TABLE.n_CONDITION_Z;
				}
			}
		}
		return 255;
	}

	public NetMissionInfo GetMissionNetInfo(int missionID)
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.dicMission.ContainsKey(missionID))
		{
			return ManagedSingleton<PlayerNetManager>.Instance.dicMission[missionID].netMissionInfo;
		}
		return null;
	}

	public List<MISSION_TABLE> GetMissionDataByType(MissionType type)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == (int)type).ToList();
	}

	public List<MISSION_TABLE> GetMissionByTypeAndCondition(MissionType type, MissionCondition condition)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == (int)type && x.n_CONDITION == (int)condition).ToList();
	}

	public List<MISSION_TABLE> GetMissionByTypeAndSubType(MissionType type, int n_SUB_TYPE)
	{
		return ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == (int)type && x.n_SUB_TYPE == n_SUB_TYPE).ToList();
	}

	public int GetMissionCounter(int counterID)
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.dicMissionProgress.ContainsKey(counterID))
		{
			return ManagedSingleton<PlayerNetManager>.Instance.dicMissionProgress[counterID].netMissionProgressInfo.Count;
		}
		return 0;
	}

	public bool CheckMissionRewardRetrieved(int missionID)
	{
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicMission.ContainsKey(missionID))
		{
			return false;
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicMission[missionID].netMissionInfo.Received <= 0)
		{
			return false;
		}
		return true;
	}

	public bool CheckStagePassed(int stageID)
	{
		foreach (KeyValuePair<int, StageInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicStage)
		{
			if (item.Value.netStageInfo.StageID == stageID)
			{
				return true;
			}
		}
		return false;
	}

	public int GetSeriesStageTotalStar(int stageSeries)
	{
		int num = 0;
		foreach (KeyValuePair<int, StageInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicStage)
		{
			STAGE_TABLE stage = null;
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetStage(item.Value.netStageInfo.StageID, out stage) && stage.n_MAIN == stageSeries)
			{
				num += ManagedSingleton<StageHelper>.Instance.GetStarAmount(item.Value.netStageInfo.Star);
			}
		}
		return num;
	}

	public int GetCharaterStar(int CharaterID, int Star)
	{
		CharacterInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(CharaterID, out value))
		{
			return value.netInfo.Star;
		}
		return 0;
	}

	public int GetStagePassedAndStar(int StageID, int Star)
	{
		StageInfo value = null;
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(StageID, out value))
		{
			return 0;
		}
		int num = 0;
		foreach (StageClearStar value2 in Enum.GetValues(typeof(StageClearStar)))
		{
			if (value2 != StageClearStar.FullStar && Convert.ToBoolean((int)value2 & (int)value.netStageInfo.Star))
			{
				num++;
			}
		}
		if (num >= Star)
		{
			return 1;
		}
		return 0;
	}

	public int GetWeaponTypeRarityTotalStar(int Type, int Rarity, int TotalStar)
	{
		List<WEAPON_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.Values.Where((WEAPON_TABLE q) => (q.n_TYPE & Type) > 0 && q.n_RARITY >= Rarity).ToList();
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			WeaponInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(list[i].n_ID, out value))
			{
				num += value.netInfo.Star;
			}
		}
		return num;
	}

	public int GetCardRarityStar(int Count, int Rarity, int Star)
	{
		List<int> list = new List<int>();
		int count = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList.Count;
		for (int i = 0; i < count; i++)
		{
			int galleryMainID = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList[i].GalleryMainID;
			CARD_TABLE value = null;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(galleryMainID, out value) || value.n_RARITY < Rarity)
			{
				continue;
			}
			List<int> galleryIDList = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList[i].GalleryIDList;
			for (int j = 0; j < galleryIDList.Count; j++)
			{
				GALLERY_TABLE value2 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT.TryGetValue(galleryIDList[j], out value2) && value2.n_CONDITION == 2 && value2.n_CONDITION_X >= Star)
				{
					list.Add(galleryMainID);
					if (list.Count < Count)
					{
						break;
					}
					return list.Count;
				}
			}
		}
		if (list.Count >= Count)
		{
			return list.Count;
		}
		int[] array = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Keys.ToArray();
		foreach (int key in array)
		{
			CardInfo cardInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCard[key];
			CARD_TABLE value3 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(cardInfo.netCardInfo.CardID, out value3) && value3.n_RARITY >= Rarity && cardInfo.netCardInfo.Star >= Star && !list.Contains(cardInfo.netCardInfo.CardID))
			{
				list.Add(cardInfo.netCardInfo.CardID);
				if (list.Count >= Count)
				{
					return list.Count;
				}
			}
		}
		return list.Count;
	}

	public int GetCardRarityLevel(int Count, int Rarity, int Level)
	{
		List<int> list = new List<int>();
		int count = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList.Count;
		for (int i = 0; i < count; i++)
		{
			int galleryMainID = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList[i].GalleryMainID;
			CARD_TABLE value = null;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(galleryMainID, out value) || value.n_RARITY < Rarity)
			{
				continue;
			}
			List<int> galleryIDList = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList[i].GalleryIDList;
			for (int j = 0; j < galleryIDList.Count; j++)
			{
				GALLERY_TABLE value2 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT.TryGetValue(galleryIDList[j], out value2) && value2.n_CONDITION == 10 && value2.n_CONDITION_X >= Level)
				{
					list.Add(galleryMainID);
					if (list.Count < Count)
					{
						break;
					}
					return list.Count;
				}
			}
		}
		if (list.Count >= Count)
		{
			return list.Count;
		}
		int[] array = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Keys.ToArray();
		foreach (int key in array)
		{
			CardInfo cardInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCard[key];
			CARD_TABLE value3 = null;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(cardInfo.netCardInfo.CardID, out value3))
			{
				continue;
			}
			EXP_TABLE cardExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(cardInfo.netCardInfo.Exp);
			if (value3.n_RARITY >= Rarity && cardExpTable.n_ID >= Level && !list.Contains(cardInfo.netCardInfo.CardID))
			{
				list.Add(cardInfo.netCardInfo.CardID);
				if (list.Count >= Count)
				{
					return list.Count;
				}
			}
		}
		return list.Count;
	}

	public int GetCardType(int Type, int Count)
	{
		List<int> list = new List<int>();
		int count = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList.Count;
		for (int i = 0; i < count; i++)
		{
			int galleryMainID = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList[i].GalleryMainID;
			CARD_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(galleryMainID, out value) && value.n_TYPE == Type)
			{
				list.Add(galleryMainID);
				if (list.Count >= Count)
				{
					return list.Count;
				}
			}
		}
		if (list.Count >= Count)
		{
			return list.Count;
		}
		int[] array = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Keys.ToArray();
		foreach (int key in array)
		{
			CardInfo cardInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCard[key];
			CARD_TABLE value2 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(cardInfo.netCardInfo.CardID, out value2) && value2.n_TYPE == Type && !list.Contains(cardInfo.netCardInfo.CardID))
			{
				list.Add(cardInfo.netCardInfo.CardID);
				if (list.Count >= Count)
				{
					return list.Count;
				}
			}
		}
		return list.Count;
	}

	public int GetFinalStrikeTotalLevel(int Level)
	{
		int num = 0;
		int[] array = ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.Keys.ToArray();
		foreach (int key in array)
		{
			FinalStrikeInfo finalStrikeInfo = ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike[key];
			num += finalStrikeInfo.netFinalStrikeInfo.Level;
		}
		return num;
	}

	public int GetBackupWeaponSlotTotalLevel(int Level)
	{
		int num = 0;
		List<BenchInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			num += list[i].netBenchInfo.Level;
		}
		return num;
	}

	public bool CheckCharacterAndWeaponAndCardGalleryLevel(int CharacterLevel, int WeaponLevel, int CardLevel)
	{
		if (ManagedSingleton<GalleryHelper>.Instance.GalleryGetCharactersExp().m_lv < CharacterLevel)
		{
			return false;
		}
		if (ManagedSingleton<GalleryHelper>.Instance.GalleryGetWeaponsExp().m_lv < WeaponLevel)
		{
			return false;
		}
		if (ManagedSingleton<GalleryHelper>.Instance.GalleryGetCardsExp().m_lv < CardLevel)
		{
			return false;
		}
		return true;
	}

	public int GetWeaponStarAndExpert(int WeaponID, int Star, int ExpertLevel)
	{
		int num = 0;
		WeaponInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(WeaponID, out value) && value.netInfo.Star >= Star && value.netExpertInfos != null)
		{
			for (int i = 0; i < value.netExpertInfos.Count; i++)
			{
				num += value.netExpertInfos[i].ExpertLevel;
			}
		}
		return num;
	}

	public bool CheckPreMissionPassed(int missionID)
	{
		if (!ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.ContainsKey(missionID))
		{
			return false;
		}
		MISSION_TABLE mISSION_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT[missionID];
		if (mISSION_TABLE == null)
		{
			return false;
		}
		switch ((MissionOpenCondition)(short)mISSION_TABLE.n_OPEN_CONDITION)
		{
		case MissionOpenCondition.MissionComplete:
		{
			NetMissionInfo missionNetInfo = GetMissionNetInfo(mISSION_TABLE.n_OPEN_CONDITION_X);
			if (missionNetInfo == null || missionNetInfo.Received != 1)
			{
				return false;
			}
			break;
		}
		case MissionOpenCondition.LevelReach:
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() < mISSION_TABLE.n_OPEN_CONDITION_X)
			{
				return false;
			}
			break;
		case MissionOpenCondition.PassStage:
			if (!CheckStagePassed(mISSION_TABLE.n_OPEN_CONDITION_X))
			{
				return false;
			}
			break;
		}
		return true;
	}

	public bool HasNewAccountEventMission(MissionType type, int subType)
	{
		foreach (MISSION_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values)
		{
			if (value.n_TYPE == (int)type && value.n_SUB_TYPE == subType && (ManagedSingleton<PlayerHelper>.Instance.GetLV() < value.n_END_RANK || value.n_END_RANK == 0) && ManagedSingleton<OrangeTableHelper>.Instance.IsTimeAfterDate(value.s_CREATE_TIME, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PlayerCreateTime) && (value.n_TYPE != 4 || value.n_SUB_TYPE != 15 || !ManagedSingleton<PlayerNetManager>.Instance.dicMission.ContainsKey(value.n_ID) || ManagedSingleton<PlayerNetManager>.Instance.dicMission[value.n_ID].netMissionInfo.Received == 0))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckHasNewPlayerEvent()
	{
		foreach (MISSION_TABLE item in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == 4 && x.n_SUB_TYPE == 15 && (x.s_CREATE_TIME == "null" || CapUtility.DateToUnixTime(ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(x.s_CREATE_TIME)) < ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PlayerCreateTime)).ToList())
		{
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicMission.ContainsKey(item.n_ID) || ManagedSingleton<PlayerNetManager>.Instance.dicMission[item.n_ID].netMissionInfo.Received == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMissionToRetrieve(MissionType type, int subType)
	{
		List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(EventType.EVENT_MISSION, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
		foreach (MISSION_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values)
		{
			if (value.n_TYPE != (int)type || (value.n_SUB_TYPE != subType && subType != 0))
			{
				continue;
			}
			bool flag = ((value.n_TYPE != 4) ? true : false);
			if (!flag)
			{
				foreach (EVENT_TABLE item in eventTableByType)
				{
					if (item.n_TYPE == 9 && item.n_TYPE_X == value.n_SUB_TYPE)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag && !CheckMissionRewardRetrieved(value.n_ID) && CheckMissionCompleted(value.n_ID))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMissionToRetrieve(MissionCondition condition)
	{
		List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(EventType.EVENT_MISSION, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
		foreach (MISSION_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values)
		{
			if (value.n_CONDITION != (int)condition)
			{
				continue;
			}
			bool flag = ((value.n_TYPE != 4) ? true : false);
			if (!flag)
			{
				foreach (EVENT_TABLE item in eventTableByType)
				{
					if (item.n_TYPE == 9 && item.n_TYPE_X == value.n_SUB_TYPE)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag && !CheckMissionRewardRetrieved(value.n_ID) && CheckMissionCompleted(value.n_ID))
			{
				return true;
			}
		}
		return false;
	}

	public List<MISSION_TABLE> GetMissionCouldBeRetrievedList(MissionType type, int subType)
	{
		List<MISSION_TABLE> list = new List<MISSION_TABLE>();
		foreach (MISSION_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values)
		{
			if (value.n_TYPE == (int)type && (value.n_SUB_TYPE == subType || subType == 0) && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(value.s_BEGIN_TIME, value.s_END_TIME, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC) && !CheckMissionRewardRetrieved(value.n_ID) && CheckMissionCompleted(value.n_ID))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public List<int> GetMissionCouldBeRetrievedListIncludePredict(MissionType type, int subType, out bool epExcluded)
	{
		epExcluded = false;
		int num = 0;
		int num2 = 20;
		int num3 = 20;
		List<int> list = new List<int>();
		do
		{
			List<MISSION_TABLE> missionCouldBeRetrievedList = GetMissionCouldBeRetrievedList(type, subType);
			if (missionCouldBeRetrievedList.Count == 0)
			{
				break;
			}
			int num4 = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
			for (int num5 = missionCouldBeRetrievedList.Count - 1; num5 >= 0; num5--)
			{
				MISSION_TABLE mISSION_TABLE = missionCouldBeRetrievedList[num5];
				if (mISSION_TABLE.n_EP > 0)
				{
					num4 += mISSION_TABLE.n_EP;
					if (num4 > OrangeConst.EP_MAX)
					{
						missionCouldBeRetrievedList.RemoveAt(num5);
						epExcluded = true;
					}
				}
			}
			List<int> list2 = new List<int>();
			foreach (MISSION_TABLE item in missionCouldBeRetrievedList)
			{
				list2.Add(item.n_ID);
				if (list.Count + list2.Count >= num3)
				{
					break;
				}
			}
			num++;
			list.AddRange(list2);
			foreach (int item2 in list2)
			{
				DummyMissionInfo value = new DummyMissionInfo
				{
					netMissionInfo = new NetMissionInfo
					{
						MissionID = item2,
						Received = 1
					}
				};
				ManagedSingleton<PlayerNetManager>.Instance.dicMission.Add(item2, value);
			}
		}
		while (num < num2 && list.Count <= num3);
		foreach (MissionInfo item3 in ManagedSingleton<PlayerNetManager>.Instance.dicMission.Values.Where((MissionInfo x) => x is DummyMissionInfo).ToList())
		{
			ManagedSingleton<PlayerNetManager>.Instance.dicMission.Remove(item3.netMissionInfo.MissionID);
		}
		return list;
	}

	public void FillterMissionByProgress(List<MISSION_TABLE> listMission)
	{
		for (int num = listMission.Count - 1; num >= 0; num--)
		{
			MISSION_TABLE mISSION_TABLE = listMission[num];
			if (GetMissionCounter(mISSION_TABLE.n_COUNTER) >= mISSION_TABLE.n_LIMIT)
			{
				listMission.RemoveAt(num);
			}
		}
	}

	public List<MISSION_TABLE> CheckTeamUpWithFriend(List<string> listTeamMemberID)
	{
		bool flag = false;
		foreach (string item in listTeamMemberID)
		{
			if (ManagedSingleton<FriendHelper>.Instance.IsFriend(item))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return null;
		}
		List<MISSION_TABLE> missionByTypeAndCondition = GetMissionByTypeAndCondition(MissionType.MultiPlayer, MissionCondition.TeamUpWithFriendAccumulated);
		FillterMissionByProgress(missionByTypeAndCondition);
		return missionByTypeAndCondition;
	}

	public List<int> CheckTeamUpWithGuildMember()
	{
		return null;
	}

	public List<MISSION_TABLE> CheckTeamUpWithLevelGapPlayer(List<int> listTeamMemberLevel)
	{
		List<MISSION_TABLE> missionByTypeAndCondition = GetMissionByTypeAndCondition(MissionType.MultiPlayer, MissionCondition.TeamUpLevelGapPlayerAccumulated);
		FillterMissionByProgress(missionByTypeAndCondition);
		for (int num = missionByTypeAndCondition.Count - 1; num >= 0; num--)
		{
			MISSION_TABLE mISSION_TABLE = missionByTypeAndCondition[num];
			bool flag = false;
			foreach (int item in listTeamMemberLevel)
			{
				if (Math.Abs(ManagedSingleton<PlayerHelper>.Instance.GetLV() - item) >= mISSION_TABLE.n_CONDITION_X)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				missionByTypeAndCondition.RemoveAt(num);
			}
		}
		return missionByTypeAndCondition;
	}

	public List<MISSION_TABLE> CheckTeamUpWithNearbyPlayer(List<float> listLongitude, List<float> listLatitude)
	{
		if (listLongitude.Count != listLatitude.Count)
		{
			return null;
		}
		List<MISSION_TABLE> missionByTypeAndCondition = GetMissionByTypeAndCondition(MissionType.MultiPlayer, MissionCondition.TeamUpWithNearbyPlayerAccumulated);
		FillterMissionByProgress(missionByTypeAndCondition);
		for (int num = missionByTypeAndCondition.Count - 1; num >= 0; num--)
		{
			MISSION_TABLE mISSION_TABLE = missionByTypeAndCondition[num];
			bool flag = false;
			for (int i = 0; i < listLongitude.Count; i++)
			{
			}
			if (!flag)
			{
				missionByTypeAndCondition.RemoveAt(num);
			}
		}
		return missionByTypeAndCondition;
	}

	public List<MISSION_TABLE> CheckTeamUpWithAssignedPlace()
	{
		List<MISSION_TABLE> missionByTypeAndCondition = GetMissionByTypeAndCondition(MissionType.MultiPlayer, MissionCondition.TeamUpInAssignedPlaceAccumulated);
		FillterMissionByProgress(missionByTypeAndCondition);
		for (int num = missionByTypeAndCondition.Count - 1; num >= 0; num--)
		{
			MISSION_TABLE mISSION_TABLE = missionByTypeAndCondition[num];
		}
		return missionByTypeAndCondition;
	}

	public List<MISSION_TABLE> CheckTeamUpWithAssignedCharacter(List<int> listCharacter)
	{
		List<MISSION_TABLE> missionByTypeAndCondition = GetMissionByTypeAndCondition(MissionType.MultiPlayer, MissionCondition.TeamUpWithAssignedCharacterAccumulated);
		FillterMissionByProgress(missionByTypeAndCondition);
		for (int num = missionByTypeAndCondition.Count - 1; num >= 0; num--)
		{
			MISSION_TABLE mISSION_TABLE = missionByTypeAndCondition[num];
			foreach (int item in listCharacter)
			{
				if (item != mISSION_TABLE.n_CONDITION_X && item != mISSION_TABLE.n_CONDITION_Y && item != mISSION_TABLE.n_CONDITION_Z)
				{
					missionByTypeAndCondition.RemoveAt(num);
					break;
				}
			}
		}
		return missionByTypeAndCondition;
	}

	public List<MISSION_TABLE> CheckTeamUpWithAssignedWeapon(List<int> listWeapon)
	{
		List<MISSION_TABLE> missionByTypeAndCondition = GetMissionByTypeAndCondition(MissionType.MultiPlayer, MissionCondition.TeamUpWithAssignedWeaponAccumulated);
		FillterMissionByProgress(missionByTypeAndCondition);
		for (int num = missionByTypeAndCondition.Count - 1; num >= 0; num--)
		{
			MISSION_TABLE mISSION_TABLE = missionByTypeAndCondition[num];
			foreach (int item in listWeapon)
			{
				if (item != mISSION_TABLE.n_CONDITION_X && item != mISSION_TABLE.n_CONDITION_Y && item != mISSION_TABLE.n_CONDITION_Z)
				{
					missionByTypeAndCondition.RemoveAt(num);
					break;
				}
			}
		}
		return missionByTypeAndCondition;
	}

	public List<MISSION_TABLE> CheckTeamUpWithAssignedWeaponType(List<int> listWeaponType)
	{
		List<MISSION_TABLE> missionByTypeAndCondition = GetMissionByTypeAndCondition(MissionType.MultiPlayer, MissionCondition.TeamUpWithAssignedWeaponTypeAccumulated);
		FillterMissionByProgress(missionByTypeAndCondition);
		for (int num = missionByTypeAndCondition.Count - 1; num >= 0; num--)
		{
			MISSION_TABLE mISSION_TABLE = missionByTypeAndCondition[num];
			foreach (int item in listWeaponType)
			{
				if (item != mISSION_TABLE.n_CONDITION_X && item != mISSION_TABLE.n_CONDITION_Y && item != mISSION_TABLE.n_CONDITION_Z)
				{
					missionByTypeAndCondition.RemoveAt(num);
					break;
				}
			}
		}
		return missionByTypeAndCondition;
	}

	public int GetWeaponEnhanceCount(int enhanceLv)
	{
		int num = 0;
		foreach (WeaponInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Values)
		{
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(value.netInfo.Exp) >= enhanceLv)
			{
				num++;
			}
		}
		return num;
	}

	public int GetWeaponExpertCount(int count)
	{
		int num = 0;
		foreach (WeaponInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Values)
		{
			int num2 = 0;
			foreach (NetWeaponExpertInfo netExpertInfo in value.netExpertInfos)
			{
				num2 += netExpertInfo.ExpertLevel;
			}
			if (num2 >= count)
			{
				num++;
			}
		}
		return num;
	}

	public int GetChipEnhanceCount(int enhanceLv)
	{
		int num = 0;
		foreach (ChipInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicChip.Values)
		{
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetChipRank(value.netChipInfo.Exp) >= enhanceLv)
			{
				num++;
			}
		}
		return num;
	}

	public int GetChipAnalyzeCount(int analyzeLv)
	{
		int num = 0;
		foreach (ChipInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicChip.Values)
		{
			if (value.netChipInfo.Analyse >= analyzeLv)
			{
				num++;
			}
		}
		return num;
	}

	public int GetWeaponTypeCount(int weaponType)
	{
		int num = 0;
		foreach (WeaponInfo value2 in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Values)
		{
			WEAPON_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(value2.netInfo.WeaponID, out value) && Convert.ToBoolean(value.n_TYPE & weaponType))
			{
				num++;
			}
		}
		return num;
	}

	public int GetTotalSecretStageCount()
	{
		int num = 0;
		foreach (StageInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicStage.Values)
		{
			num += value.StageSecretList.Count;
		}
		return num;
	}

	public int GetSubTypeMissionTotalActivity(MissionSubType type)
	{
		if (mmapPresetAchievementByMissionSubType.ContainKey(type))
		{
			return mmapPresetAchievementByMissionSubType[type].Sum((MISSION_TABLE x) => x.n_ACTIVITY);
		}
		return 0;
	}

	public void ResetMonthlyActivityCounterID()
	{
		currentMonthlyCounterID = 0;
	}
}
