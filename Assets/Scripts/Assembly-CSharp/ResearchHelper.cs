using System;
using System.Collections.Generic;
using enums;

public class ResearchHelper : ManagedSingleton<ResearchHelper>
{
	public enum FreeSearchItem : short
	{
		FREERESEARCH_SLOT1 = 1,
		FREERESEARCH_SLOT2 = 2,
		FREERESEARCH_SLOT3 = 3,
		FREERESEARCH_SLOT4 = 4
	}

	public enum ResearchNaviTalk : short
	{
		FREE_RESEARCH_GET = 0,
		FREE_RESEARCH_MISS = 1,
		FREE_RESEARCH_ING = 2,
		FREE_RESEARCH_ALLCLEAR = 3,
		RESEARCH_FINISH = 4,
		RESEARCH_ING = 5,
		RESEARCH_DEFAULT = 6,
		MAX = 7
	}

	private List<RESEARCH_TABLE> listTemporaryResearchDataForUIView = new List<RESEARCH_TABLE>();

	public int FillteredDataCount
	{
		get
		{
			return listTemporaryResearchDataForUIView.Count;
		}
		private set
		{
		}
	}

	public bool DisplayHint
	{
		get
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() < OrangeConst.OPENRANK_RESEARCH)
			{
				return false;
			}
			if (IsAnyNormalResearchDone())
			{
				return true;
			}
			if (IsAnyFreeResearchCouldBeRetrieved())
			{
				return true;
			}
			return false;
		}
	}

	public override void Initialize()
	{
	}

	public override void Reset()
	{
		base.Reset();
		listTemporaryResearchDataForUIView.Clear();
	}

	public override void Dispose()
	{
		listTemporaryResearchDataForUIView.Clear();
	}

	public RESEARCH_TABLE GetUIViewData(int idx)
	{
		if (listTemporaryResearchDataForUIView[idx] != null)
		{
			return listTemporaryResearchDataForUIView[idx];
		}
		return null;
	}

	public void CollectUIViewData(int level, bool haveRewardItem = true, bool removeRunoutData = true, bool preResearchCheck = true)
	{
		listTemporaryResearchDataForUIView.Clear();
		foreach (KeyValuePair<int, RESEARCH_TABLE> item in ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT)
		{
			if (item.Value.n_RESEARCH_LV != level)
			{
				continue;
			}
			if (GetResearchInfo(item.Value.n_ID) != null)
			{
				listTemporaryResearchDataForUIView.Add(item.Value);
			}
			else
			{
				if (item.Value.n_ITEMID == 0 && haveRewardItem)
				{
					continue;
				}
				if (removeRunoutData)
				{
					NetResearchRecord researchRecord = GetResearchRecord(item.Value.n_ID);
					if (researchRecord != null && researchRecord.Count >= item.Value.n_LIMIT && item.Value.n_RESET_RULE == 0)
					{
						continue;
					}
				}
				if (item.Value.n_PRE != 0 && preResearchCheck)
				{
					NetResearchRecord researchRecord2 = GetResearchRecord(item.Value.n_PRE);
					if (researchRecord2 == null)
					{
						continue;
					}
					if (researchRecord2.TotalCount == 1)
					{
						NetResearchInfo researchInfo = GetResearchInfo(item.Value.n_PRE);
						if (researchInfo != null && researchInfo.FinishTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
						{
							continue;
						}
					}
				}
				listTemporaryResearchDataForUIView.Add(item.Value);
			}
		}
	}

	public int GetIdleResearchSlot()
	{
		int num = int.MaxValue;
		foreach (KeyValuePair<int, NetResearchInfo> item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch)
		{
			if (item.Value.ResearchID == 0 && item.Value.StartTime == 0 && item.Value.FinishTime == 0 && item.Value.Slot < num)
			{
				num = item.Value.Slot;
			}
		}
		return num;
	}

	public NetResearchRecord GetResearchRecord(int researchID)
	{
		foreach (NetResearchRecord item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.listResearchRecord)
		{
			if (item.ResearchID == researchID)
			{
				return item;
			}
		}
		return null;
	}

	public NetResearchInfo GetResearchInfo(int researchID)
	{
		foreach (KeyValuePair<int, NetResearchInfo> item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch)
		{
			if (item.Value.ResearchID == researchID)
			{
				return item.Value;
			}
		}
		return null;
	}

	public bool IsAnyNormalResearchDoneByLevel(int level)
	{
		foreach (KeyValuePair<int, NetResearchInfo> item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch)
		{
			RESEARCH_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT.TryGetValue(item.Value.ResearchID, out value) && value.n_RESEARCH_LV == level && item.Value != null && item.Value.StartTime != 0 && item.Value.FinishTime != 0 && item.Value.FinishTime <= MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyNormalResearchDone()
	{
		foreach (KeyValuePair<int, NetResearchInfo> item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch)
		{
			if (item.Value != null && item.Value.StartTime != 0 && item.Value.FinishTime != 0 && item.Value.FinishTime <= MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyFreeResearchCouldBeRetrieved()
	{
		foreach (FreeSearchItem value in Enum.GetValues(typeof(FreeSearchItem)))
		{
			RESEARCH_TABLE rESEARCH_TABLE = ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT[(int)value];
			if (rESEARCH_TABLE == null || rESEARCH_TABLE.n_RESEARCH_LV != 0)
			{
				continue;
			}
			bool flag = false;
			if (rESEARCH_TABLE.n_GET_TIME != 0)
			{
				int num2 = rESEARCH_TABLE.n_GET_TIME / 100;
				int num3 = rESEARCH_TABLE.n_GET_TIME % 100;
				int num4 = MonoBehaviourSingleton<OrangeGameManager>.Instance.serverInfo.DailyResetInfo.PreResetTime + num2 * 3600;
				int num5 = MonoBehaviourSingleton<OrangeGameManager>.Instance.serverInfo.DailyResetInfo.PreResetTime + num3 * 3600;
				if (MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC >= num4 && MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC < num5)
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			NetFreeResearchInfo netFreeResearchInfo = null;
			foreach (NetFreeResearchInfo item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.listFreeResearch)
			{
				if (item != null && item.ResearchID == (int)value)
				{
					netFreeResearchInfo = item;
					break;
				}
			}
			if (netFreeResearchInfo == null)
			{
				return true;
			}
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(netFreeResearchInfo.LastRetrieveTime, ResetRule.DailyReset))
			{
				return true;
			}
		}
		return false;
	}

	public ResearchNaviTalk GetNormalResearchTalk()
	{
		ResearchNaviTalk researchNaviTalk = ResearchNaviTalk.RESEARCH_DEFAULT;
		foreach (KeyValuePair<int, NetResearchInfo> item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch)
		{
			if (item.Value != null && item.Value.StartTime != 0 && item.Value.FinishTime != 0)
			{
				if (item.Value.FinishTime <= MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
				{
					return ResearchNaviTalk.RESEARCH_FINISH;
				}
				if (researchNaviTalk > ResearchNaviTalk.RESEARCH_ING)
				{
					researchNaviTalk = ResearchNaviTalk.RESEARCH_ING;
				}
			}
		}
		return researchNaviTalk;
	}

	public ResearchNaviTalk GetFreeResearchTalk()
	{
		ResearchNaviTalk researchNaviTalk = ResearchNaviTalk.FREE_RESEARCH_ALLCLEAR;
		foreach (FreeSearchItem value in Enum.GetValues(typeof(FreeSearchItem)))
		{
			RESEARCH_TABLE rESEARCH_TABLE = ManagedSingleton<OrangeDataManager>.Instance.RESEARCH_TABLE_DICT[(int)value];
			if (rESEARCH_TABLE == null || rESEARCH_TABLE.n_RESEARCH_LV != 0)
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (rESEARCH_TABLE.n_GET_TIME != 0)
			{
				int num2 = rESEARCH_TABLE.n_GET_TIME / 100;
				int num3 = rESEARCH_TABLE.n_GET_TIME % 100;
				int num4 = MonoBehaviourSingleton<OrangeGameManager>.Instance.serverInfo.DailyResetInfo.PreResetTime + num2 * 3600;
				int num5 = MonoBehaviourSingleton<OrangeGameManager>.Instance.serverInfo.DailyResetInfo.PreResetTime + num3 * 3600;
				if (MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC >= num4 && MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC < num5)
				{
					flag = true;
				}
				else if (MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC > num5)
				{
					flag2 = true;
				}
				else
				{
					flag3 = true;
				}
			}
			else
			{
				flag = true;
			}
			NetFreeResearchInfo netFreeResearchInfo = null;
			foreach (NetFreeResearchInfo item in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.listFreeResearch)
			{
				if (item != null && item.ResearchID == (int)value)
				{
					netFreeResearchInfo = item;
					break;
				}
			}
			if (flag)
			{
				if (netFreeResearchInfo == null || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(netFreeResearchInfo.LastRetrieveTime, ResetRule.DailyReset))
				{
					return ResearchNaviTalk.FREE_RESEARCH_GET;
				}
			}
			else if (flag2)
			{
				if ((netFreeResearchInfo == null || !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(netFreeResearchInfo.LastRetrieveTime, ResetRule.DailyReset)) && researchNaviTalk > ResearchNaviTalk.FREE_RESEARCH_MISS)
				{
					researchNaviTalk = ResearchNaviTalk.FREE_RESEARCH_MISS;
				}
			}
			else if (flag3 && researchNaviTalk > ResearchNaviTalk.FREE_RESEARCH_ING)
			{
				researchNaviTalk = ResearchNaviTalk.FREE_RESEARCH_ING;
			}
		}
		return researchNaviTalk;
	}

	public EXP_TABLE GetResearchRowByExp(int exp)
	{
		EXP_TABLE result = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[0];
		foreach (KeyValuePair<int, EXP_TABLE> item in ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT)
		{
			if (item.Value.n_TOTAL_RESEARCHEXP > exp)
			{
				return item.Value;
			}
			if (item.Value.n_TOTAL_RESEARCHEXP != 0)
			{
				result = item.Value;
			}
		}
		return result;
	}

	public EXP_TABLE GetResearchRowByLevel(int level)
	{
		if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.ContainsKey(level))
		{
			return ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[level];
		}
		return null;
	}

	public List<int> GetListFinishTime()
	{
		List<int> list = new List<int>();
		if (ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch != null && ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch.Count > 0)
		{
			foreach (NetResearchInfo value in ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch.Values)
			{
				if (value.FinishTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
				{
					list.Add(value.FinishTime);
				}
			}
		}
		return list;
	}
}
