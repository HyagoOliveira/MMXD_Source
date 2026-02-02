using UnityEngine;

public class CardResetColume : ScrollIndexCallback
{
	public class PerCardCell
	{
		public int nID = -1;

		public int nSeqID = -1;

		public GameObject refRoot;

		public CardBase tCardIconBase;

		public void OnClick(int p_param)
		{
			if (nID != -1)
			{
				Transform transform = refRoot.transform;
				CardResetUI cardResetUI = null;
				while (transform != null && cardResetUI == null)
				{
					cardResetUI = transform.GetComponent<CardResetUI>();
					transform = transform.parent;
				}
				if (cardResetUI != null)
				{
					cardResetUI.OnClickCardResetCell(nSeqID);
				}
			}
		}
	}

	public const int nCount = 5;

	[SerializeField]
	private Transform[] cellroot = new Transform[5];

	[SerializeField]
	private GameObject[] ImgSelecteds = new GameObject[5];

	public PerCardCell[] allPerCardCells;

	public GameObject refPrefab;

	private int nNowIndex = -1;

	private CardResetUI tCardResetUI;

	private void Update()
	{
		if (tCardResetUI == null || allPerCardCells == null)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (allPerCardCells[i] != null)
			{
				if (tCardResetUI.CurrentClickCardSeqID == allPerCardCells[i].nSeqID)
				{
					UpdateCardState(allPerCardCells[i]);
				}
				ImgSelecteds[i].SetActive(tCardResetUI.IsSelected(allPerCardCells[i].nSeqID));
			}
		}
	}

	private void UpdateCardState(PerCardCell pcc)
	{
		NetCardInfo netCardInfo = tCardResetUI.m_listNetCardInfoFiltered.Find((NetCardInfo x) => x.CardSeqID == pcc.nSeqID);
		if (netCardInfo != null)
		{
			pcc.tCardIconBase.CardSetupUpdate(netCardInfo, true);
		}
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
			allPerCardCells = new PerCardCell[5];
			for (int i = 0; i < 5; i++)
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
				allPerCardCells[i].tCardIconBase = allPerCardCells[i].refRoot.GetComponent<CardBase>();
				ImgSelecteds[i].SetActive(false);
			}
		}
		Transform parent = base.transform;
		while (tCardResetUI == null && parent != null)
		{
			tCardResetUI = parent.GetComponent<CardResetUI>();
			parent = parent.parent;
		}
		if (!(tCardResetUI == null))
		{
			nNowIndex = p_idx;
			int j = 0;
			bool whiteColor = false;
			for (int count = tCardResetUI.m_listNetCardInfoFiltered.Count; j < 5 && p_idx * 5 + j < count; j++)
			{
				NetCardInfo netCardInfo = tCardResetUI.m_listNetCardInfoFiltered[p_idx * 5 + j];
				CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[netCardInfo.CardID];
				allPerCardCells[j].refRoot.SetActive(true);
				allPerCardCells[j].nID = cARD_TABLE.n_ID;
				allPerCardCells[j].nSeqID = netCardInfo.CardSeqID;
				CardBase tCardIconBase = allPerCardCells[j].tCardIconBase;
				tCardIconBase.Setup(0, "", "", allPerCardCells[j].OnClick, whiteColor);
				tCardIconBase.CardSetup(netCardInfo, true);
				tCardIconBase.SetEquipCharImage(netCardInfo.CardSeqID);
			}
			for (; j < 5; j++)
			{
				allPerCardCells[j].refRoot.SetActive(false);
			}
		}
	}
}
