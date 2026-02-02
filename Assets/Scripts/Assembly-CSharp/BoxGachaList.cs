using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoxGachaList : OrangeUIBase
{
	[SerializeField]
	private Transform[] m_itemBarRankRef;

	[SerializeField]
	private RectTransform m_scrollViewGachaList;

	[SerializeField]
	private Button m_btnLeftArrow;

	[SerializeField]
	private Button m_btnRightArrow;

	[SerializeField]
	private Image m_btnLeftArrowDisable;

	[SerializeField]
	private Image m_btnRightArrowDisable;

	[SerializeField]
	private OrangeText m_phaseLabel;

	[SerializeField]
	private Image m_pageDotOn;

	[SerializeField]
	private Image m_pageDotOff;

	private EVENT_TABLE m_eventTable;

	private List<NetBoxGachaRecord> m_boxGachaRecords;

	private BOXGACHA_TABLE m_boxGachaTable;

	private List<BOXGACHA_TABLE> m_listBoxGachaTable;

	private int m_currentCycle;

	private int m_currentPhase;

	private int m_selectionPhase;

	private Vector2 m_spacing = new Vector2(40f, 7f);

	private Image[] m_pageDotsArray;

	private List<NetBoxGachaRecord>[] m_phaseBoxGachaRecords;

	public void Setup(EVENT_TABLE eventTable, List<NetBoxGachaRecord> boxGachaRecords)
	{
		BoxGachaStatus value = null;
		m_eventTable = eventTable;
		m_boxGachaRecords = boxGachaRecords;
		m_listBoxGachaTable = ManagedSingleton<OrangeDataManager>.Instance.BOXGACHA_TABLE_DICT.Values.Where((BOXGACHA_TABLE x) => x.n_GROUP == m_eventTable.n_BOXGACHA).ToList();
		m_phaseBoxGachaRecords = new List<NetBoxGachaRecord>[m_listBoxGachaTable.Count];
		for (int i = 0; i < m_listBoxGachaTable.Count; i++)
		{
			int tempIdx = i;
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveBoxGachaRecordReq(m_eventTable.n_ID, m_listBoxGachaTable[i].n_ID, delegate(int param, List<NetBoxGachaRecord> records)
			{
				m_phaseBoxGachaRecords[tempIdx] = records;
			});
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicBoxGachaStatus.TryGetValue(m_eventTable.n_ID, out value))
		{
			m_currentCycle = value.netBoxGachaStatus.CycleCounts;
			ManagedSingleton<OrangeDataManager>.Instance.BOXGACHA_TABLE_DICT.TryGetValue(value.netBoxGachaStatus.CurrentBoxGachaID, out m_boxGachaTable);
			m_currentPhase = 0;
			while (m_currentPhase < m_listBoxGachaTable.Count && m_listBoxGachaTable[m_currentPhase].n_ID != m_boxGachaTable.n_ID)
			{
				m_currentPhase++;
			}
		}
		else
		{
			m_boxGachaTable = m_listBoxGachaTable[0];
		}
		SetupPageDots(m_listBoxGachaTable.Count);
		m_selectionPhase = m_currentPhase;
		UpdateUI();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void SetupPageDots(int pageCount)
	{
		float num = 30f;
		float num2 = num / 2f;
		Vector2 vector = new Vector2((float)(pageCount - 1) * (0f - num2), 0f);
		m_pageDotsArray = new Image[pageCount];
		for (int i = 0; i < pageCount; i++)
		{
			m_pageDotsArray[i] = Object.Instantiate(m_pageDotOff, m_pageDotOff.transform.parent);
			m_pageDotsArray[i].transform.SetSiblingIndex(i);
			m_pageDotsArray[i].transform.localPosition = vector + new Vector2(num * (float)i, 0f);
		}
		m_pageDotOff.gameObject.SetActive(false);
		m_pageDotOn.gameObject.SetActive(true);
		SetPageDotPos(m_selectionPhase);
	}

	private void SetPageDotPos(int pageIndex)
	{
		if (pageIndex <= m_pageDotsArray.Length - 1)
		{
			m_pageDotOn.transform.localPosition = m_pageDotsArray[pageIndex].transform.localPosition;
		}
	}

	private void UpdateUI()
	{
		m_btnLeftArrow.gameObject.SetActive(false);
		m_btnRightArrow.gameObject.SetActive(false);
		m_btnLeftArrowDisable.gameObject.SetActive(true);
		m_btnRightArrowDisable.gameObject.SetActive(true);
		if (m_selectionPhase < m_listBoxGachaTable.Count - 1)
		{
			m_btnRightArrow.gameObject.SetActive(true);
			m_btnRightArrowDisable.gameObject.SetActive(false);
		}
		if (m_selectionPhase > 0)
		{
			m_btnLeftArrow.gameObject.SetActive(true);
			m_btnLeftArrowDisable.gameObject.SetActive(false);
		}
		UpdateGachaItemList(m_listBoxGachaTable[m_selectionPhase].n_ID, m_listBoxGachaTable[m_selectionPhase].n_GACHA);
		if (m_selectionPhase >= m_listBoxGachaTable.Count - 1)
		{
			m_phaseLabel.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BOX_REWARD_LAST_STEP"), m_listBoxGachaTable.Count);
		}
		else
		{
			m_phaseLabel.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BOX_REWARD_LIST_STEP"), m_selectionPhase + 1);
		}
		SetPageDotPos(m_selectionPhase);
	}

	private void UpdateGachaItemList(int boxGachaTable_nID, int boxGachaTable_nGacha)
	{
		Vector2 itemPos = new Vector2(0f, 0f);
		Vector2 sizeDelta = m_itemBarRankRef[0].GetComponent<RectTransform>().sizeDelta;
		List<BOXGACHACONTENT_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.BOXGACHACONTENT_TABLE_DICT.Values.Where((BOXGACHACONTENT_TABLE x) => x.n_GROUP == boxGachaTable_nGacha).ToList();
		foreach (Transform scrollViewGacha in m_scrollViewGachaList)
		{
			Object.Destroy(scrollViewGacha.gameObject);
		}
		foreach (BOXGACHACONTENT_TABLE item in list)
		{
			Transform transform = ((item.n_PICKUP == 1) ? Object.Instantiate(m_itemBarRankRef[0], m_scrollViewGachaList) : Object.Instantiate(m_itemBarRankRef[1], m_scrollViewGachaList));
			transform.transform.localPosition = itemPos;
			transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, item.n_REWARD_ID, item.n_AMOUNT_MAX);
			OrangeText component = transform.transform.Find("TextItemSets").GetComponent<OrangeText>();
			Transform transform2 = transform.transform.Find("CheckMark");
			if ((bool)transform2)
			{
				transform2.gameObject.SetActive(false);
			}
			component.text = string.Format("{0}/{1}", item.n_TOTAL, item.n_TOTAL);
			if (m_selectionPhase <= m_currentPhase)
			{
				StartCoroutine(UpdateCheckMarkItemCount(item, component, transform2));
			}
			itemPos.y -= sizeDelta.y + m_spacing.y;
		}
		AddDummyItemToGachaList(ref itemPos, sizeDelta);
		m_scrollViewGachaList.sizeDelta = new Vector2(m_scrollViewGachaList.sizeDelta.x, 0f - itemPos.y);
	}

	private void AddDummyItemToGachaList(ref Vector2 itemPos, Vector2 sizeDelta)
	{
		Transform transform = Object.Instantiate(m_itemBarRankRef[1], m_scrollViewGachaList);
		transform.transform.localPosition = itemPos;
		transform.transform.Find("TextItemName").GetComponent<OrangeText>().text = ItemIconHelper(transform, OrangeConst.BOX_GACHA_DUMMY_ID, OrangeConst.BOX_GACHA_DUMMY_COUNT);
		OrangeText component = transform.transform.Find("TextItemSets").GetComponent<OrangeText>();
		Transform transform2 = transform.transform.Find("CheckMark");
		if ((bool)transform2)
		{
			transform2.gameObject.SetActive(false);
		}
		component.text = "";
		itemPos.y -= sizeDelta.y + m_spacing.y;
	}

	private IEnumerator UpdateCheckMarkItemCount(BOXGACHACONTENT_TABLE boxGachaContentTable, OrangeText itemSet, Transform checkMarkTrans)
	{
		NetBoxGachaRecord record = null;
		float exitTime = Time.time + 5f;
		if (m_selectionPhase != m_currentPhase)
		{
			while (m_phaseBoxGachaRecords[m_selectionPhase] == null && !(Time.time >= exitTime))
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (m_phaseBoxGachaRecords[m_selectionPhase] != null)
			{
				record = m_phaseBoxGachaRecords[m_selectionPhase].Find((NetBoxGachaRecord x) => x.GachaID == boxGachaContentTable.n_ID);
			}
		}
		else
		{
			record = m_boxGachaRecords.Find((NetBoxGachaRecord x) => x.GachaID == boxGachaContentTable.n_ID);
		}
		if (record != null)
		{
			bool active = record.Count >= boxGachaContentTable.n_TOTAL;
			itemSet.text = string.Format("{0}/{1}", boxGachaContentTable.n_TOTAL - record.Count, boxGachaContentTable.n_TOTAL);
			if ((bool)checkMarkTrans)
			{
				checkMarkTrans.gameObject.SetActive(active);
			}
		}
	}

	public void OnClickLeftArrow()
	{
		if (m_selectionPhase > 0)
		{
			m_selectionPhase--;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		UpdateUI();
	}

	public void OnClickRightArrow()
	{
		if (m_selectionPhase < m_listBoxGachaTable.Count - 1)
		{
			m_selectionPhase++;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		UpdateUI();
	}

	private string ItemIconHelper(Transform target, int itemId, int amount)
	{
		ItemIconWithAmount componentInChildren = target.GetComponentInChildren<ItemIconWithAmount>();
		ITEM_TABLE item;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(itemId, out item))
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out value))
				{
					string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
					componentInChildren.Setup(itemId, p_bundleName, value.s_ICON, OnClickItem);
				}
			}
			else
			{
				componentInChildren.Setup(itemId, AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON, OnClickItem);
			}
			componentInChildren.SetRare(item.n_RARE);
			componentInChildren.SetAmount(amount);
			return ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(item.w_NAME);
		}
		return null;
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out value))
				{
					ui.CanShowHow2Get = false;
					ui.Setup(value, item);
				}
			}
			else
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			}
		});
	}
}
