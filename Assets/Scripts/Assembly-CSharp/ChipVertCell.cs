using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChipVertCell : ScrollIndexCallback
{
	public class PerChipCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public Image tImage;

		public Text tText;

		public Button tBtn;

		public void OnClick()
		{
			if (nID != -1)
			{
				ChipInfoUI chipInfoUI = null;
				Transform transform = refRoot.transform;
				while (chipInfoUI == null)
				{
					transform = transform.parent;
					chipInfoUI = transform.GetComponent<ChipInfoUI>();
				}
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
			}
		}
	}

	public PerChipCell tChipCells;

	public override void ScrollCellIndex(int p_idx)
	{
		if (tChipCells == null)
		{
			tChipCells = new PerChipCell();
			Transform transform = base.transform.Find("Button");
			tChipCells.tBtn = transform.gameObject.GetComponent<Button>();
			tChipCells.tImage = transform.Find("SelImage").GetComponent<Image>();
			tChipCells.tText = transform.Find("Text").GetComponent<Text>();
			tChipCells.tBtn.onClick.AddListener(tChipCells.OnClick);
		}
		Dictionary<int, ChipInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicChip.GetEnumerator();
		int num = -1;
		while (enumerator.MoveNext())
		{
			num++;
			if (num == p_idx)
			{
				break;
			}
		}
		if (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.ContainsKey(enumerator.Current.Value.netChipInfo.ChipID))
		{
			DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[enumerator.Current.Value.netChipInfo.ChipID];
			tChipCells.nID = enumerator.Current.Value.netChipInfo.ChipID;
			tChipCells.tText.text = ManagedSingleton<OrangeTextDataManager>.Instance.DISCTEXT_TABLE_DICT.GetL10nValue(dISC_TABLE.w_NAME);
		}
	}
}
