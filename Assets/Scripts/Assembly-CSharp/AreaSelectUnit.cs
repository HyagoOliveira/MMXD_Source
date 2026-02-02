using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class AreaSelectUnit : ScrollIndexCallback
{
	private enum AreaState
	{
		UnSelected = 0,
		Selected = 1,
		Locked = 2
	}

	[SerializeField]
	private StoryStageSelectUI parent;

	[SerializeField]
	private GameObject[] LabelObjs;

	[SerializeField]
	private Image imgRewardHint;

	[SerializeField]
	private Text[] txtAreaName = new Text[3];

	private int nAreaID;

	private bool bShowReceived;

	private AreaState areaState = AreaState.Locked;

	public override void ScrollCellIndex(int p_idx)
	{
		nAreaID = p_idx + 1;
		if (nAreaID == parent.CurrentArea)
		{
			areaState = AreaState.Selected;
		}
		else
		{
			areaState = AreaState.UnSelected;
			if (p_idx > 0)
			{
				foreach (STAGE_TABLE item in GetListStageByTypeAreaDifficulty(StageType.Scenario, p_idx, (int)parent.CurrentDifficulty))
				{
					StageInfo value = null;
					ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(item.n_ID, out value);
					if (value == null)
					{
						areaState = AreaState.Locked;
						break;
					}
				}
			}
		}
		SetAreaLabel(areaState, nAreaID);
	}

	private List<STAGE_TABLE> GetListStageByTypeAreaDifficulty(StageType p_type, int mainAreaID, int difficulty)
	{
		int type = (int)p_type;
		return ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_TYPE == type && x.n_MAIN == mainAreaID && x.n_DIFFICULTY == difficulty).ToList();
	}

	private bool SetAreaLabel(AreaState state, int areaID)
	{
		GameObject[] labelObjs = LabelObjs;
		for (int i = 0; i < labelObjs.Length; i++)
		{
			labelObjs[i].SetActive(false);
		}
		LabelObjs[(int)state].SetActive(true);
		string p_key = string.Format("MAIN_STAGE_{0}", areaID);
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(p_key);
		Text[] array = txtAreaName;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].text = areaID + ". " + l10nValue;
		}
		List<MISSION_TABLE> list = null;
		list = ((parent.CurrentDifficulty != 0) ? ManagedSingleton<OrangeTableHelper>.Instance.GetListMissionByConditionDifficulty(1001, areaID, (int)parent.CurrentDifficulty) : ManagedSingleton<OrangeTableHelper>.Instance.GetListMissionByCondition(1001, areaID));
		MissionInfo value = null;
		bShowReceived = true;
		for (int j = 0; j < list.Count; j++)
		{
			if (ManagedSingleton<PlayerNetManager>.Instance.dicMission.TryGetValue(list[j].n_ID, out value))
			{
				if (value.netMissionInfo.Received != 0)
				{
					bShowReceived = false;
				}
				continue;
			}
			if (GetStageConditionValue(areaID, (int)parent.CurrentDifficulty) >= list[j].n_CONDITION_Y)
			{
				bShowReceived = true;
				break;
			}
			bShowReceived = false;
		}
		imgRewardHint.gameObject.SetActive(bShowReceived);
		return true;
	}

	private int GetStageConditionValue(int p_fillter, int p_difficulty)
	{
		List<NetStageInfo> listNetStageByMainId = ManagedSingleton<StageHelper>.Instance.GetListNetStageByMainId(p_fillter);
		int num = 0;
		if (listNetStageByMainId == null)
		{
			return num;
		}
		foreach (NetStageInfo item in listNetStageByMainId)
		{
			STAGE_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(item.StageID, out value))
			{
				if (p_difficulty == 0)
				{
					num += ManagedSingleton<StageHelper>.Instance.GetStarAmount(item.Star);
				}
				else if (value.n_DIFFICULTY == p_difficulty)
				{
					num += ManagedSingleton<StageHelper>.Instance.GetStarAmount(item.Star);
				}
			}
		}
		return num;
	}

	public void OnClickUnit()
	{
		parent.OnClickAreaSelectUnit(nAreaID);
	}
}
