using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class StageRewardCell : ScrollIndexCallback
{
	public class PerRewardCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public StageLoadIcon frm;

		public StageLoadIcon iconbg;

		public StageLoadIcon iconimg;

		public Text icontext;

		public void OnClick(object p_param)
		{
			int nID2 = nID;
			int num = -1;
		}
	}

	public const int nCellCount = 5;

	private PerRewardCell[] tPerRewardCell;

	public override void ScrollCellIndex(int p_idx)
	{
		if (tPerRewardCell == null)
		{
			tPerRewardCell = new PerRewardCell[5];
			for (int i = 0; i < 5; i++)
			{
				tPerRewardCell[i] = new PerRewardCell();
				Transform transform = base.transform.Find("ItemButton" + i);
				if (transform != null)
				{
					tPerRewardCell[i].refRoot = transform.gameObject;
					tPerRewardCell[i].frm = transform.Find("frmbg").GetComponent<StageLoadIcon>();
					tPerRewardCell[i].iconbg = transform.GetComponent<StageLoadIcon>();
					tPerRewardCell[i].iconimg = transform.Find("itemicon").GetComponent<StageLoadIcon>();
					tPerRewardCell[i].icontext = transform.Find("Text").GetComponent<Text>();
				}
			}
		}
		for (int j = 0; j < 5; j++)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_UPDATESTAGEREWARD, tPerRewardCell[j], p_idx * 5 + j);
		}
	}
}
