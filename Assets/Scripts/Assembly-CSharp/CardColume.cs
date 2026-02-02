using UnityEngine;

public class CardColume : ScrollIndexCallback
{
	public class PerCardCell
	{
		public int nID = -1;

		public int nSeqID = -1;

		public int nProtected;

		public GameObject refRoot;

		public CardBase tCardIconBase;

		public int nFavorite;

		public void OnClick(int p_param)
		{
			if (nID == -1)
			{
				return;
			}
			Transform transform = refRoot.transform;
			CardMainUI tCardMainUI = null;
			while (transform != null && tCardMainUI == null)
			{
				tCardMainUI = transform.GetComponent<CardMainUI>();
				transform = transform.parent;
			}
			if (tCardMainUI != null)
			{
				tCardMainUI.CurrentClickCardSeqID = nSeqID;
				if (tCardMainUI.nProtected == 1)
				{
					int pot2 = ((nProtected == 0) ? 1 : 0);
					ManagedSingleton<PlayerNetManager>.Instance.ProtectedCardReq(nSeqID, pot2, delegate
					{
						nProtected = pot2;
						tCardIconBase.OnSetLockImage(nProtected == 1);
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
						for (int j = 0; j < tCardMainUI.m_listNetCardInfoFiltered.Count; j++)
						{
							if (tCardMainUI.m_listNetCardInfoFiltered[j].CardSeqID == nSeqID)
							{
								tCardMainUI.m_listNetCardInfoFiltered[j].Protected = (sbyte)nProtected;
								break;
							}
						}
					});
					return;
				}
				if (tCardMainUI.nFavorite == 1)
				{
					int pot = ((nFavorite == 0) ? 1 : 0);
					ManagedSingleton<PlayerNetManager>.Instance.FavoriteCardReq(nSeqID, pot, delegate
					{
						nFavorite = pot;
						tCardIconBase.OnSetFavoriteImage(nFavorite == 1);
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
						for (int i = 0; i < tCardMainUI.m_listNetCardInfoFiltered.Count; i++)
						{
							if (tCardMainUI.m_listNetCardInfoFiltered[i].CardSeqID == nSeqID)
							{
								tCardMainUI.m_listNetCardInfoFiltered[i].Favorite = (sbyte)nFavorite;
								break;
							}
						}
					});
					return;
				}
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardInfo", delegate(CardInfoUI ui)
			{
				if (tCardMainUI != null)
				{
					ui.bNeedInitList = true;
				}
				ui.nTargetCardSeqID = nSeqID;
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		}
	}

	public const int nCount = 6;

	[SerializeField]
	private Transform[] cellroot = new Transform[6];

	[SerializeField]
	private GameObject[] LockImages = new GameObject[6];

	[SerializeField]
	private GameObject[] FavoriteImages = new GameObject[6];

	public PerCardCell[] allPerCardCells;

	public GameObject refPrefab;

	private int nNowIndex = -1;

	private int nProtected;

	private CardMainUI tCardMainUI;

	private int nFavorite;

    [System.Obsolete]
    private void Update()
	{
		if (tCardMainUI == null)
		{
			return;
		}
		nProtected = tCardMainUI.nProtected;
		bool flag = nProtected == 1;
		nFavorite = tCardMainUI.nFavorite;
		bool flag2 = nFavorite == 1;
		if (allPerCardCells == null)
		{
			return;
		}
		for (int i = 0; i < 6; i++)
		{
			if (allPerCardCells[i] != null)
			{
				bool active = allPerCardCells[i].nProtected != 0 && flag && allPerCardCells[i].refRoot.active;
				LockImages[i].SetActive(active);
				bool active2 = allPerCardCells[i].nFavorite != 0 && flag2 && allPerCardCells[i].refRoot.active;
				FavoriteImages[i].SetActive(active2);
				if (tCardMainUI.CurrentClickCardSeqID == allPerCardCells[i].nSeqID)
				{
					UpdateCardState(allPerCardCells[i]);
				}
			}
		}
	}

	private void UpdateCardState(PerCardCell pcc)
	{
		NetCardInfo netCardInfo = tCardMainUI.m_listNetCardInfoFiltered.Find((NetCardInfo x) => x.CardSeqID == pcc.nSeqID);
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
			allPerCardCells = new PerCardCell[6];
			for (int i = 0; i < 6; i++)
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
			}
		}
		Transform parent = base.transform;
		while (tCardMainUI == null && parent != null)
		{
			tCardMainUI = parent.GetComponent<CardMainUI>();
			parent = parent.parent;
		}
		if (!(tCardMainUI == null))
		{
			nNowIndex = p_idx;
			int j = 0;
			bool whiteColor = false;
			int count = tCardMainUI.m_listNetCardInfoFiltered.Count;
			nProtected = tCardMainUI.nProtected;
			nFavorite = tCardMainUI.nFavorite;
			for (; j < 6 && p_idx * 6 + j < count; j++)
			{
				NetCardInfo netCardInfo = tCardMainUI.m_listNetCardInfoFiltered[p_idx * 6 + j];
				CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[netCardInfo.CardID];
				allPerCardCells[j].refRoot.SetActive(true);
				allPerCardCells[j].nID = cARD_TABLE.n_ID;
				allPerCardCells[j].nSeqID = netCardInfo.CardSeqID;
				allPerCardCells[j].nProtected = netCardInfo.Protected;
				allPerCardCells[j].nFavorite = netCardInfo.Favorite;
				CardBase tCardIconBase = allPerCardCells[j].tCardIconBase;
				tCardIconBase.Setup(0, "", "", allPerCardCells[j].OnClick, whiteColor);
				tCardIconBase.CardSetup(netCardInfo, true);
				tCardIconBase.SetEquipCharImage(netCardInfo.CardSeqID);
			}
			for (; j < 6; j++)
			{
				allPerCardCells[j].refRoot.SetActive(false);
			}
		}
	}
}
