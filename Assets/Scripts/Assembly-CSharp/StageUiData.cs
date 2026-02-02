using System;

[Serializable]
public class StageUiData
{
	[Serializable]
	public enum STAGE_UI_ENUM
	{
		NONE = 0,
		ShowKillScoreUI = 1,
		ShowGetItemUI = 2,
		ShowBattleScoreUI = 3,
		ShowGetItemUI2 = 4,
		ShowContributionNowNum = 5
	}

	public int n_TYPE;

	public int n_MAIN;

	public STAGE_UI_ENUM nUIType;

	public int nParam0;

	public bool bParam0;

	public bool bParam1;

	public string sIgnoreStageID;

	public string[] sStarGoalKey;

	public StageUiData(int nType, int nMain, STAGE_UI_ENUM enumUiType, int inParam0 = 0, bool ibParam0 = false, bool ibParam1 = false, string isIgnoreStageID = "")
	{
		n_TYPE = nType;
		n_MAIN = nMain;
		nUIType = enumUiType;
		nParam0 = inParam0;
		bParam0 = ibParam0;
		bParam1 = ibParam1;
		sIgnoreStageID = isIgnoreStageID;
		sStarGoalKey = null;
	}
}
