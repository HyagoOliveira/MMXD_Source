using System.Collections.Generic;
using System.Linq;
using enums;

public class HintHelper : ManagedSingleton<HintHelper>
{
	public bool DisplayPvpRewardHint
	{
		get
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() < OrangeConst.OPENRANK_PVP)
			{
				return false;
			}
			MultiPlayGachaType[] array = new MultiPlayGachaType[2]
			{
				MultiPlayGachaType.PVP_DAILY,
				MultiPlayGachaType.PVP_WEEKLY
			};
			foreach (MultiPlayGachaType multiPlayGachaType in array)
			{
				List<PVP_REWARD_TABLE> pvpRewardTableByType = ManagedSingleton<OrangeTableHelper>.Instance.GetPvpRewardTableByType(multiPlayGachaType);
				List<PVP_REWARD_TABLE> list = (from x in pvpRewardTableByType
					group x by x.n_COUNTER into x
					select x.First()).ToList();
				int count = list.Count;
				int num = 0;
				for (int j = 0; j < count; j++)
				{
					int counterKey = list[j].n_COUNTER;
					int missionCounter = ManagedSingleton<MissionHelper>.Instance.GetMissionCounter(counterKey);
					if (missionCounter <= 0)
					{
						continue;
					}
					PVP_REWARD_TABLE[] array2 = (from x in pvpRewardTableByType
						where x.n_COUNTER == counterKey
						orderby x.n_CONDITION_X
						select x).ToArray();
					for (int k = 0; k < array2.Length; k++)
					{
						if (array2[k].n_CONDITION_X <= missionCounter)
						{
							num += array2[k].n_GACHACOUNT;
						}
					}
				}
				num -= ManagedSingleton<PlayerNetManager>.Instance.mmapMultiPlayGachaRecord[multiPlayGachaType].Count;
				if (num > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool DisplayGuideHint
	{
		get
		{
			int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
			BPGUIDE_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.BPGUIDE_TABLE_DICT.TryGetValue(lV, out value) && value.n_TOTAL_BP > ManagedSingleton<PlayerHelper>.Instance.GetBattlePower())
			{
				return true;
			}
			return false;
		}
	}

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	private void UpdateFinalStrikeDirty()
	{
		if (!FinalStrikeInfo.bIsAnyDirty)
		{
			return;
		}
		FinalStrikeInfo.bIsAnyDirty = false;
		ManagedSingleton<PlayerNetManager>.Instance.nTotalFSLv = 0;
		Dictionary<int, FinalStrikeInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ManagedSingleton<PlayerNetManager>.Instance.nTotalFSLv += enumerator.Current.Value.netFinalStrikeInfo.Level;
		}
		ManagedSingleton<PlayerNetManager>.Instance.bIsAnyFinalStrikeCanStrengthen = false;
		FS_TABLE[] array = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE obj) => obj.n_LV == 1).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (IsFinalStrikeCanUnLock(array[i].n_FS_ID) || IsFinalStrikeCanStarUp(array[i].n_FS_ID))
			{
				ManagedSingleton<PlayerNetManager>.Instance.bIsAnyFinalStrikeCanStrengthen = true;
				break;
			}
		}
	}

	public bool IsFinalStrikeCanUnLock(int nFSID)
	{
		FinalStrikeInfo value;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(nFSID, out value))
		{
			return false;
		}
		if (FinalStrikeInfo.bIsAnyDirty)
		{
			UpdateFinalStrikeDirty();
		}
		List<FS_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == nFSID).ToList();
		if (list.Count > 0)
		{
			FS_TABLE fS_TABLE = list[0];
			int num = 0;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(fS_TABLE.n_UNLOCK_ID))
			{
				num = ManagedSingleton<PlayerNetManager>.Instance.dicItem[fS_TABLE.n_UNLOCK_ID].netItemInfo.Stack;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.nTotalFSLv >= fS_TABLE.n_UNLOCK_LV)
			{
				return num >= fS_TABLE.n_UNLOCK_COUNT;
			}
			return false;
		}
		return false;
	}

	public bool IsFinalStrikeCanStarUp(int nFSID)
	{
		if (FinalStrikeInfo.bIsItmeUpdateDirty)
		{
			FinalStrikeInfo.bIsItmeUpdateDirty = false;
			foreach (FinalStrikeInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.Values)
			{
				value.bIsDirty = true;
			}
		}
		FinalStrikeInfo tFinalStrikeInfo;
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(nFSID, out tFinalStrikeInfo))
		{
			return false;
		}
		if (!tFinalStrikeInfo.bIsDirty)
		{
			return tFinalStrikeInfo.bIsCanStarUp;
		}
		tFinalStrikeInfo.bIsDirty = false;
		tFinalStrikeInfo.bIsCanStarUp = false;
		List<FS_TABLE> fsTable_lvl1 = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == nFSID && x.n_LV == 1).ToList();
		List<STAR_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values.Where((STAR_TABLE x) => x.n_TYPE == 3 && x.n_MAINID == fsTable_lvl1[0].n_FS_ID && x.n_STAR == tFinalStrikeInfo.netFinalStrikeInfo.Star).ToList();
		if (list.Count > 0)
		{
			if (list[0].n_MATERIAL == 0)
			{
				tFinalStrikeInfo.bIsCanStarUp = false;
			}
			else
			{
				tFinalStrikeInfo.bIsCanStarUp = true;
			}
			int firstNotEnoughItemID = 0;
			tFinalStrikeInfo.bIsCanStarUp &= ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(list[0].n_MATERIAL, out firstNotEnoughItemID);
		}
		return tFinalStrikeInfo.bIsCanStarUp;
	}

	public bool IsAnyFinalStrikeCanStrengthen()
	{
		if (FinalStrikeInfo.bIsAnyDirty)
		{
			UpdateFinalStrikeDirty();
		}
		return ManagedSingleton<PlayerNetManager>.Instance.bIsAnyFinalStrikeCanStrengthen;
	}
}
