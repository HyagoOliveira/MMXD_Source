using System;
using CallbackDefs;
using UnityEngine;

public class ChipsColume : ScrollIndexCallback
{
	public class PeChipCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public CommonIconBase tChipIconBase;

		public void OnClick(int p_param)
		{
			if (nID == -1)
			{
				return;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CHIPINFO", delegate(ChipInfoUI ui)
			{
				ChipMainUI chipMainUI = null;
				Transform transform = refRoot.transform;
				while (transform != null && chipMainUI == null)
				{
					chipMainUI = transform.GetComponent<ChipMainUI>();
					transform = transform.parent;
				}
				if (!(chipMainUI == null))
				{
					ui.listHasChips = chipMainUI.listHasChips;
					ui.listFragChips = chipMainUI.listFragChips;
					ui.nTargetChipID = nID;
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
					{
						ChipMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ChipMainUI>("UI_CHIPMAIN");
						if ((bool)uI)
						{
							uI.InitLVSR();
						}
					});
				}
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		}
	}

	public const int nCount = 6;

	[SerializeField]
	private Transform[] cellroot = new Transform[6];

	public PeChipCell[] allPeChipCells;

	public GameObject refPrefab;

	public int oldsort;

	private int nNowIndex = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		if (allPeChipCells == null)
		{
			allPeChipCells = new PeChipCell[6];
			for (int i = 0; i < 6; i++)
			{
				allPeChipCells[i] = new PeChipCell();
				if (cellroot[i].transform.childCount > 0)
				{
					for (int num = cellroot[i].transform.childCount - 1; num >= 0; num--)
					{
						UnityEngine.Object.Destroy(cellroot[i].transform.GetChild(num).gameObject);
					}
				}
				allPeChipCells[i].refRoot = UnityEngine.Object.Instantiate(refPrefab, cellroot[i]);
				allPeChipCells[i].tChipIconBase = allPeChipCells[i].refRoot.GetComponent<CommonIconBase>();
			}
		}
		nNowIndex = p_idx;
		ChipMainUI chipMainUI = null;
		Transform parent = base.transform;
		while (parent != null && chipMainUI == null)
		{
			chipMainUI = parent.GetComponent<ChipMainUI>();
			parent = parent.parent;
		}
		if (chipMainUI == null)
		{
			for (int j = 0; j < 6; j++)
			{
				allPeChipCells[j].refRoot.SetActive(false);
			}
			return;
		}
		int num2 = chipMainUI.listHasChips.Count + chipMainUI.listFragChips.Count;
		int k = 0;
		bool flag = false;
		for (; k < 6 && p_idx * 6 + k < num2; k++)
		{
			ChipInfo value = null;
			int num3 = p_idx * 6 + k;
			if (!ManagedSingleton<EquipHelper>.Instance.bChipSortDescend)
			{
				if (num3 >= chipMainUI.listFragChips.Count)
				{
					int index = num3 - chipMainUI.listFragChips.Count;
					if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(chipMainUI.listHasChips[index], out value))
					{
						value = new ChipInfo();
						value.netChipInfo = new NetChipInfo();
						value.netChipInfo.ChipID = chipMainUI.listHasChips[index];
					}
				}
				else if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(chipMainUI.listFragChips[num3], out value))
				{
					value = new ChipInfo();
					value.netChipInfo = new NetChipInfo();
					value.netChipInfo.ChipID = chipMainUI.listFragChips[num3];
				}
			}
			else if (num3 >= chipMainUI.listHasChips.Count)
			{
				int index = num3 - chipMainUI.listHasChips.Count;
				if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(chipMainUI.listFragChips[index], out value))
				{
					value = new ChipInfo();
					value.netChipInfo = new NetChipInfo();
					value.netChipInfo.ChipID = chipMainUI.listFragChips[index];
				}
			}
			else if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(chipMainUI.listHasChips[num3], out value))
			{
				value = new ChipInfo();
				value.netChipInfo = new NetChipInfo();
				value.netChipInfo.ChipID = chipMainUI.listHasChips[num3];
			}
			DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[value.netChipInfo.ChipID];
			allPeChipCells[k].refRoot.SetActive(true);
			allPeChipCells[k].nID = dISC_TABLE.n_ID;
			flag = ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(value.netChipInfo.ChipID);
			CommonIconBase tChipIconBase = allPeChipCells[k].tChipIconBase;
			tChipIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconChip, dISC_TABLE.s_ICON, allPeChipCells[k].OnClick, flag);
			tChipIconBase.SetOtherInfo(value.netChipInfo, true);
		}
		for (; k < 6; k++)
		{
			allPeChipCells[k].refRoot.SetActive(false);
		}
	}
}
