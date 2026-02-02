#define RELEASE
using UnityEngine;

public class CardIconCell : ScrollIndexCallback
{
	public GameObject refCardBase;

	public int nID = -1;

	public int nSeqID = -1;

	private NetCardInfo TargetNetCardInfo;

	public GameObject refRoot;

	public CommonIconBase tCardIconBase;

	private GameObject tMark;

	private int nLastIndex = -1;

	private CardInfoUI tCardInfoUI;

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, UpdateCheckMainSunWeapon);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, UpdateCheckMainSunWeapon);
	}

	private void Update()
	{
		if (tMark != null && tCardInfoUI != null)
		{
			tMark.gameObject.SetActive(tCardInfoUI.nTargetCardSeqID == nSeqID);
		}
	}

	public void UpdateCheckMainSunWeapon()
	{
		ScrollCellIndex(nLastIndex);
	}

	public void OnClick(int p_param)
	{
		if (nID != -1 && nSeqID != -1)
		{
			CardInfoUI cardInfoUI = null;
			Transform parent = base.transform;
			while ((bool)parent && cardInfoUI == null)
			{
				cardInfoUI = parent.GetComponent<CardInfoUI>();
				parent = parent.parent;
			}
			if (cardInfoUI != null)
			{
				cardInfoUI.ChangeCard(nSeqID);
			}
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (refRoot == null)
		{
			tCardIconBase = base.gameObject.transform.GetComponentInChildren<CommonIconBase>();
			if (tCardIconBase != null)
			{
				Object.Destroy(tCardIconBase.gameObject);
			}
			refRoot = Object.Instantiate(refCardBase, base.gameObject.transform);
			tCardIconBase = refRoot.GetComponent<CommonIconBase>();
			tMark = base.transform.Find("Image").gameObject;
			tMark.SetActive(false);
		}
		if (p_idx == -1)
		{
			return;
		}
		if (tMark == null)
		{
			tMark = base.transform.Find("Image").gameObject;
		}
		tCardInfoUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardInfoUI>("UI_CardInfo");
		nLastIndex = p_idx;
		if (tCardInfoUI == null)
		{
			TargetNetCardInfo = null;
			refRoot.SetActive(false);
			return;
		}
		int count = tCardInfoUI.listHasCards.Count;
		if (nLastIndex >= 0 && nLastIndex < count)
		{
			CardInfo cardInfo = ManagedSingleton<EquipHelper>.Instance.GetSortedCardInfo(nLastIndex);
			if (cardInfo == null)
			{
				cardInfo = new CardInfo();
				cardInfo.netCardInfo = new NetCardInfo();
				cardInfo.netCardInfo.CardID = tCardInfoUI.listHasCards[nLastIndex].netCardInfo.CardID;
			}
			NetCardInfo netCardInfo = cardInfo.netCardInfo;
			if (cardInfo == null)
			{
				TargetNetCardInfo = null;
				refRoot.SetActive(false);
			}
			else if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.ContainsKey(netCardInfo.CardID))
			{
				refRoot.SetActive(true);
				CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[netCardInfo.CardID];
				bool whiteColor = true;
				tCardIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, "icon_Buster_005_000", OnClick, whiteColor);
				nID = netCardInfo.CardID;
				nSeqID = netCardInfo.CardSeqID;
				TargetNetCardInfo = netCardInfo;
				if (tCardInfoUI.nTargetCardSeqID == netCardInfo.CardID)
				{
					tMark.SetActive(true);
				}
				else
				{
					tMark.SetActive(false);
				}
				tCardIconBase.SetCardInfo(netCardInfo);
			}
			else
			{
				TargetNetCardInfo = null;
				refRoot.SetActive(false);
				Debug.LogError("Use Invalid CardID :" + netCardInfo.CardID);
			}
		}
		else
		{
			TargetNetCardInfo = null;
			refRoot.SetActive(false);
		}
	}
}
