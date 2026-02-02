using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageUiDataScriptObj : ScriptableObject
{
	public List<StageClearMap> listStageClearMap = new List<StageClearMap>();

	public List<StageUiData> listStageUiDatas = new List<StageUiData>();

	public void InitDefaultData()
	{
		listStageUiDatas.Add(new StageUiData(2, 15001, StageUiData.STAGE_UI_ENUM.ShowKillScoreUI));
		listStageUiDatas.Add(new StageUiData(2, 15021, StageUiData.STAGE_UI_ENUM.ShowGetItemUI));
		listStageUiDatas.Add(new StageUiData(2, 15031, StageUiData.STAGE_UI_ENUM.ShowBattleScoreUI));
		listStageUiDatas.Add(new StageUiData(4, 10002, StageUiData.STAGE_UI_ENUM.ShowGetItemUI2, 2, true, true));
		listStageUiDatas.Add(new StageUiData(4, 10012, StageUiData.STAGE_UI_ENUM.ShowGetItemUI2, 4, false, true));
		listStageUiDatas.Add(new StageUiData(4, 10015, StageUiData.STAGE_UI_ENUM.ShowGetItemUI2, 5, false, true));
		listStageUiDatas.Add(new StageUiData(4, 10016, StageUiData.STAGE_UI_ENUM.ShowGetItemUI2, 6, false, true));
		listStageUiDatas.Add(new StageUiData(4, 10014, StageUiData.STAGE_UI_ENUM.ShowBattleScoreUI, 1, true));
		listStageUiDatas.Add(new StageUiData(4, 10019, StageUiData.STAGE_UI_ENUM.ShowGetItemUI2, 7, false, true));
		listStageUiDatas.Add(new StageUiData(4, 10020, StageUiData.STAGE_UI_ENUM.ShowGetItemUI2, 2, true));
		listStageUiDatas.Add(new StageUiData(9, -1, StageUiData.STAGE_UI_ENUM.ShowContributionNowNum));
	}

	public StageUiData GetStageUiData(STAGE_TABLE tSTAGE_TABLE)
	{
		for (int num = listStageUiDatas.Count - 1; num >= 0; num--)
		{
			StageUiData stageUiData = listStageUiDatas[num];
			if (stageUiData.n_TYPE == tSTAGE_TABLE.n_TYPE && (stageUiData.n_MAIN == tSTAGE_TABLE.n_MAIN || stageUiData.n_MAIN == -1))
			{
				if (stageUiData.sIgnoreStageID != null && stageUiData.sIgnoreStageID != "")
				{
					string[] array = stageUiData.sIgnoreStageID.Split(',');
					foreach (string s in array)
					{
						if (tSTAGE_TABLE.n_ID == int.Parse(s))
						{
							return null;
						}
					}
				}
				return listStageUiDatas[num];
			}
		}
		IOrderedEnumerable<STAGE_TABLE> source = from x in ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values
			where x.s_STAGE == tSTAGE_TABLE.s_STAGE && x.n_MAIN != tSTAGE_TABLE.n_MAIN
			orderby x.n_ID descending
			select x;
		if (source.Count() > 0)
		{
			STAGE_TABLE[] array2 = source.ToArray();
			foreach (STAGE_TABLE sTAGE_TABLE in array2)
			{
				for (int num2 = listStageUiDatas.Count - 1; num2 >= 0; num2--)
				{
					StageUiData stageUiData2 = listStageUiDatas[num2];
					if (stageUiData2.n_TYPE == sTAGE_TABLE.n_TYPE && (stageUiData2.n_MAIN == sTAGE_TABLE.n_MAIN || stageUiData2.n_MAIN == -1))
					{
						return listStageUiDatas[num2];
					}
				}
			}
		}
		return null;
	}
}
