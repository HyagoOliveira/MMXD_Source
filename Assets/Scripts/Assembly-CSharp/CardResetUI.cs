using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class CardResetUI : OrangeUIBase
{
	public class BtnClickCB
	{
		public int nBtnID;

		public bool bIsLock;

		public Action<int> action;

		public void OnClick()
		{
			if (!bIsLock && action != null)
			{
				action(nBtnID);
			}
		}
	}

	public LoopVerticalScrollRect m_ScrollRect;

	public GameObject SortRoot;

	public GameObject[] RewardIconRoot;

	public Text CostText;

	public Button ResetBtn;

	public Image CostIconRoot;

	public Button[] CardType;

	public Button[] SortType;

	public Button[] GetTypeBtn;

	private Image[] CardTypeImg;

	private Image[] SortTypeImg;

	private Image[] GetTypeBtnImg;

	public Image MaskImage;

	public Color[] textColor = new Color[2]
	{
		Color.white,
		new Color(1f, 0.12156863f, 0.101960786f)
	};

	private List<BtnClickCB> BtnClickCBs = new List<BtnClickCB>();

	[HideInInspector]
	public List<NetCardInfo> m_listNetCardInfo = new List<NetCardInfo>();

	[HideInInspector]
	public List<NetCardInfo> m_listNetCardInfoFiltered = new List<NetCardInfo>();

	[HideInInspector]
	public Dictionary<int, NetCardInfo> m_dictSelectedCardInfo = new Dictionary<int, NetCardInfo>();

	private int TweenScaleId;

	public int nProtected;

	private bool bInitSort;

	public int CurrentClickCardSeqID;

	private int[] ntyps = new int[6] { 1, 2, 8, 4, 16, 0 };

	private CardColorType tmpCardSortType;

	private EquipHelper.CARD_SORT_KEY tmpCardSortKey;

	public void SetCardType(int nBID)
	{
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardResetSortType & (uint)ntyps[nBID]) == (uint)ntyps[nBID])
		{
			ManagedSingleton<EquipHelper>.Instance.nCardResetSortType = (CardColorType)((int)ManagedSingleton<EquipHelper>.Instance.nCardResetSortType & ~ntyps[nBID]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.nCardResetSortType = (CardColorType)((int)ManagedSingleton<EquipHelper>.Instance.nCardResetSortType | ntyps[nBID]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		for (int i = 0; i < CardType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardResetSortType & (uint)ntyps[i]) == (uint)ntyps[i])
			{
				CardTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				CardTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	public void SetSortType(int nBID)
	{
		int num = 1 << nBID;
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey & (uint)num) != 0)
		{
			ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey = (EquipHelper.CARD_SORT_KEY)((int)ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey & ~num);
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey = (EquipHelper.CARD_SORT_KEY)((int)ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey | num);
		}
		if (ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey == (EquipHelper.CARD_SORT_KEY)num)
		{
			ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey = (EquipHelper.CARD_SORT_KEY)((int)ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey & ~num);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey = (EquipHelper.CARD_SORT_KEY)num;
		}
		for (int i = 0; i < SortType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey & (uint)(1 << i)) != 0)
			{
				SortTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	private void UpdateButtonType()
	{
		for (int i = 0; i < CardType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardResetSortType & (uint)ntyps[i]) == (uint)ntyps[i])
			{
				CardTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				CardTypeImg[i].gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < SortType.Length; j++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey & (uint)(1 << j)) != 0)
			{
				SortTypeImg[j].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[j].gameObject.SetActive(false);
			}
		}
	}

	private void InitSortBtn()
	{
		BtnClickCBs.Clear();
		CardTypeImg = new Image[CardType.Length];
		for (int i = 0; i < CardType.Length; i++)
		{
			BtnClickCB btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = i;
			btnClickCB.action = (Action<int>)Delegate.Combine(btnClickCB.action, new Action<int>(SetCardType));
			CardType[i].onClick.RemoveAllListeners();
			CardType[i].onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			CardTypeImg[i] = CardType[i].transform.Find("Image").GetComponent<Image>();
		}
		SortTypeImg = new Image[SortType.Length];
		for (int j = 0; j < SortType.Length; j++)
		{
			BtnClickCB btnClickCB2 = new BtnClickCB();
			btnClickCB2.nBtnID = j;
			btnClickCB2.action = (Action<int>)Delegate.Combine(btnClickCB2.action, new Action<int>(SetSortType));
			SortType[j].onClick.RemoveAllListeners();
			SortType[j].onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(btnClickCB2);
			SortTypeImg[j] = SortType[j].transform.Find("Image").GetComponent<Image>();
		}
		UpdateButtonType();
		SortRoot.SetActive(false);
	}

	private void Start()
	{
		CostText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SHOW_ITEM_COUNT"), 0);
		ITEM_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.ITEM_TABLE_DICT.TryGetValue(OrangeConst.ITEMID_FREE_JEWEL, out value);
		if (value != null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(value.s_ICON), value.s_ICON, delegate(Sprite obj)
			{
				if (obj != null)
				{
					CostIconRoot.sprite = obj;
				}
			});
		}
		Setup();
	}

	public void Setup(bool bUpdateScrollRect = true, bool bOffset = false)
	{
		ManagedSingleton<EquipHelper>.Instance.ResetCardEquipCharInfo();
		if (!bInitSort)
		{
			InitSortBtn();
		}
		m_listNetCardInfo.Clear();
		List<CardInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(list[i].netCardInfo.Exp) > 1)
			{
				m_listNetCardInfo.Add(list[i].netCardInfo);
			}
		}
		OnSortGo(bUpdateScrollRect, bOffset);
		ResetBtn.interactable = m_dictSelectedCardInfo.Count > 0;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	public override void OnClickCloseBtn()
	{
		LeanTween.cancel(ref TweenScaleId);
		CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
		if (uI != null)
		{
			uI.Setup();
		}
		base.OnClickCloseBtn();
	}

	private void SortDialogMaskFade(float fStart, float fEnd, float fTime)
	{
		LeanTween.cancel(ref TweenScaleId);
		MaskImage.gameObject.SetActive(true);
		TweenScaleId = LeanTween.value(MaskImage.gameObject, fStart, fEnd, fTime).setOnUpdate(delegate(float alpha)
		{
			MaskImage.color = new Color(0f, 0f, 0f, alpha);
		}).setOnComplete((Action)delegate
		{
			TweenScaleId = -1;
			if (fEnd == 0f)
			{
				MaskImage.gameObject.SetActive(false);
			}
		})
			.uniqueId;
	}

	private IEnumerator ObjScaleCoroutine(float fStart, float fEnd, float fTime, GameObject tObj, Action endcb)
	{
		float fNowValue = fStart;
		float fLeftTime = fTime;
		float fD = (fEnd - fStart) / fTime;
		Vector3 nowScale = new Vector3(fNowValue, fNowValue, 1f);
		tObj.transform.localScale = nowScale;
		while (fLeftTime > 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			float deltaTime = Time.deltaTime;
			fLeftTime -= deltaTime;
			fNowValue = (nowScale.y = (nowScale.x = fNowValue + fD * deltaTime));
			tObj.transform.localScale = nowScale;
		}
		nowScale.x = fEnd;
		nowScale.y = fEnd;
		tObj.transform.localScale = nowScale;
		if (endcb != null)
		{
			endcb();
		}
	}

	public void OnClickSortPanelBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		tmpCardSortType = ManagedSingleton<EquipHelper>.Instance.nCardResetSortType;
		tmpCardSortKey = ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey;
		SortRoot.SetActive(true);
		MaskImage.gameObject.SetActive(true);
	}

	public void OnCloseSortRoot()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ManagedSingleton<EquipHelper>.Instance.nCardResetSortType = tmpCardSortType;
		ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey = tmpCardSortKey;
		UpdateButtonType();
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
	}

	public void OnClickSortGo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		OnSortGo();
	}

	public void OnSortGo(bool bUpdateScrollRect = true, bool bOffset = false)
	{
		m_listNetCardInfoFiltered.Clear();
		foreach (NetCardInfo item in m_listNetCardInfo)
		{
			CARD_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(item.CardID, out value);
			if (value != null && ((uint)ManagedSingleton<EquipHelper>.Instance.nCardResetSortType & (uint)value.n_TYPE) == (uint)value.n_TYPE)
			{
				m_listNetCardInfoFiltered.Add(item);
			}
		}
		m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardID.CompareTo(y.CardID));
		if ((ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY) == EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				CARD_TABLE value2 = null;
				CARD_TABLE value3 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(x.CardID, out value2);
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(y.CardID, out value3);
				if (value2 == null || value3 == null)
				{
					return 0;
				}
				int num6 = value2.n_RARITY.CompareTo(value3.n_RARITY);
				if (num6 == 0)
				{
					num6 = x.CardID.CompareTo(y.CardID);
				}
				return num6;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR) == EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num5 = x.Star.CompareTo(y.Star);
				if (num5 == 0)
				{
					num5 = x.CardID.CompareTo(y.CardID);
				}
				return num5;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardResetSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_LV) == EquipHelper.CARD_SORT_KEY.CARD_SORT_LV)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num4 = x.Exp.CompareTo(y.Exp);
				if (num4 == 0)
				{
					num4 = x.CardID.CompareTo(y.CardID);
				}
				return num4;
			});
		}
		else
		{
			m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardSeqID.CompareTo(y.CardSeqID));
		}
		if (ManagedSingleton<EquipHelper>.Instance.CardSortDescend == 1)
		{
			m_listNetCardInfoFiltered.Reverse();
		}
		int count = m_listNetCardInfoFiltered.Count;
		int num = (count - count % 5) / 5;
		if (count % 5 > 0)
		{
			num++;
		}
		if (bUpdateScrollRect)
		{
			m_ScrollRect.totalCount = num;
			m_ScrollRect.RefillCells();
		}
		else if (CurrentClickCardSeqID != 0 && bOffset)
		{
			int num2 = 0;
			for (num2 = 0; num2 < m_listNetCardInfoFiltered.Count && m_listNetCardInfoFiltered[num2].CardSeqID != CurrentClickCardSeqID; num2++)
			{
			}
			num2 = ((num2 < m_listNetCardInfoFiltered.Count) ? num2 : 0);
			int num3 = (num2 - num2 % 5) / 5;
			m_ScrollRect.totalCount = num;
			num3 = ((num3 == num - 1) ? (num - 2) : num3);
			num3 = ((num3 >= 0) ? num3 : 0);
			m_ScrollRect.RefillCells(num3);
		}
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
	}

	public void OnSortOrder()
	{
		ManagedSingleton<EquipHelper>.Instance.CardSortDescend = ((ManagedSingleton<EquipHelper>.Instance.CardSortDescend != 1) ? 1 : 0);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		OnSortGo();
	}

	public bool IsSelected(int CardSeqID)
	{
		return m_dictSelectedCardInfo.ContainsKey(CardSeqID);
	}

	public void OnClickCardResetCell(int CardSeqID)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		if (m_dictSelectedCardInfo.ContainsKey(CardSeqID))
		{
			m_dictSelectedCardInfo.Remove(CardSeqID);
		}
		else if (ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(CardSeqID))
		{
			CardInfo value = null;
			ManagedSingleton<PlayerNetManager>.Instance.dicCard.TryGetValue(CardSeqID, out value);
			if (value != null)
			{
				m_dictSelectedCardInfo.Add(CardSeqID, value.netCardInfo);
			}
		}
		CostText.color = textColor[0];
		GameObject obj = RewardIconRoot[0];
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(obj.transform.GetChild(i).gameObject);
		}
		ResetBtn.interactable = m_dictSelectedCardInfo.Count > 0;
		int num = m_dictSelectedCardInfo.Count * OrangeConst.CARD_RESET_COST;
		if (num <= 0)
		{
			CostText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SHOW_ITEM_COUNT"), 0);
			return;
		}
		CostText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SHOW_ITEM_COUNT"), num);
		if (ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel() < num)
		{
			CostText.color = textColor[1];
		}
		int TotalCardExp = 0;
		List<NetCardInfo> list = m_dictSelectedCardInfo.Values.ToList();
		for (int j = 0; j < list.Count; j++)
		{
			TotalCardExp += list[j].Exp;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			CommonIconBase componentInChildren = UnityEngine.Object.Instantiate(asset, obj.transform).GetComponentInChildren<CommonIconBase>();
			componentInChildren.gameObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
			componentInChildren.gameObject.SetActive(true);
			ITEM_TABLE item;
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(OrangeConst.ITEMID_CARD_TAKEOUT, out item))
			{
				int amount = (int)Math.Ceiling((float)TotalCardExp / item.f_VALUE_X);
				componentInChildren.SetItemWithAmount(OrangeConst.ITEMID_CARD_TAKEOUT, amount, OnClickItem);
			}
		});
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			});
		}
	}

	public void OnUpdateCostInfo()
	{
		int num = m_dictSelectedCardInfo.Count * OrangeConst.CARD_RESET_COST;
		if (ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel() < num)
		{
			CostText.color = textColor[1];
		}
		else
		{
			CostText.color = textColor[0];
		}
	}

	public void OnClickCardResetBtn()
	{
		int num = m_dictSelectedCardInfo.Count * OrangeConst.CARD_RESET_COST;
		if (ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel() < num)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIAMOND_OUT"), delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI ui)
				{
					ui.Setup(ShopTopUI.ShopSelectTab.directproduct);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnUpdateCostInfo));
				});
			}, null);
			return;
		}
		List<NetCardInfo> list = m_dictSelectedCardInfo.Values.ToList();
		if (list.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Protected == 1)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardSellConfirm", delegate(CardSellConfirmUI ui)
				{
					PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_ERROR);
					string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_RESET_WARN2");
					string str2 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_RESET_WARN3");
					ui.Setup(m_dictSelectedCardInfo.Values.ToList(), str, str2, true, OnCardReset);
				});
				return;
			}
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		OnCardReset();
	}

	public void OnCardReset()
	{
		List<int> cardSeqIDList = m_dictSelectedCardInfo.Keys.ToList();
		ManagedSingleton<PlayerNetManager>.Instance.CardResetReq(cardSeqIDList, delegate(List<NetRewardInfo> res)
		{
			if (res != null && res.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(res);
					base.CloseSE = SystemSE.NONE;
					OnClickCloseBtn();
				});
			}
		});
	}
}
