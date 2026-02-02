using UnityEngine;

public class SignColume : ScrollIndexCallback
{
	public class PerCardCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public CommonSignBase tSignBase;

		public void OnClick(int p_param)
		{
			int nID2 = nID;
			int num = -1;
		}
	}

	public const int nCount = 3;

	[SerializeField]
	private Transform[] cellroot = new Transform[3];

	public PerCardCell[] allPerCardCells;

	public GameObject refPrefab;

	private PlayerCustomizeUI tPlayerCustomizeUI;

	private void Update()
	{
		bool flag = tPlayerCustomizeUI == null;
	}

	private void UpdateCardState(PerCardCell pcc)
	{
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (allPerCardCells == null)
		{
			allPerCardCells = new PerCardCell[3];
			for (int i = 0; i < 3; i++)
			{
				allPerCardCells[i] = new PerCardCell();
				if (cellroot[i].transform.childCount > 0)
				{
					for (int num = cellroot[i].transform.childCount - 1; num >= 0; num--)
					{
						Object.Destroy(cellroot[i].transform.GetChild(num).gameObject);
					}
				}
				allPerCardCells[i].refRoot = Object.Instantiate(refPrefab, cellroot[i]);
				allPerCardCells[i].tSignBase = allPerCardCells[i].refRoot.GetComponent<CommonSignBase>();
			}
		}
		Transform parent = base.transform;
		while (tPlayerCustomizeUI == null && parent != null)
		{
			tPlayerCustomizeUI = parent.GetComponent<PlayerCustomizeUI>();
			parent = parent.parent;
		}
		if (!(tPlayerCustomizeUI == null))
		{
			int j = 0;
			for (int count = tPlayerCustomizeUI.m_listItemTableFiltered.Count; j < 3 && p_idx * 3 + j < count; j++)
			{
				allPerCardCells[j].tSignBase.Setup(tPlayerCustomizeUI.m_listItemTableFiltered[p_idx * 3 + j].n_ID, true, true);
				allPerCardCells[j].refRoot.SetActive(true);
				allPerCardCells[j].nID = tPlayerCustomizeUI.m_listItemTableFiltered[p_idx * 3 + j].n_ID;
			}
			for (; j < 3; j++)
			{
				allPerCardCells[j].refRoot.SetActive(false);
			}
		}
	}
}
