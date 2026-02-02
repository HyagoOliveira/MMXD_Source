using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class CardMainUI : OrangeUIBase
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

	public Button[] CardType;

	public Button[] SortType;

	public Button[] GetTypeBtn;

	public GameObject SortOrderImg;

	private Image[] CardTypeImg;

	private Image[] SortTypeImg;

	private Image[] GetTypeBtnImg;

	public Image MaskImage;

	[SerializeField]
	private OrangeText SlotText;

	[SerializeField]
	private Button ShowProtectedBtn;

	[SerializeField]
	private Button HideProtectedBtn;

	[SerializeField]
	private GameObject FavoriteBtn;

	[SerializeField]
	private Button ShowFavoriteBtn;

	[SerializeField]
	private Button HideFavoriteBtn;

	[SerializeField]
	private Button ResetBtn;

	[SerializeField]
	private Button DeployBtn;

	private List<BtnClickCB> BtnClickCBs = new List<BtnClickCB>();

	[HideInInspector]
	public List<NetCardInfo> m_listNetCardInfo = new List<NetCardInfo>();

	[HideInInspector]
	public List<NetCardInfo> m_listNetCardInfoFiltered = new List<NetCardInfo>();

	private int TweenScaleId;

	public int nProtected;

	private bool bInitSort;

	public int nFavorite;

	private int nWeaponType;

	private int nSortType;

	private int nGetTypeBtn;

	public int CurrentClickCardSeqID;

	private int[] ntyps = new int[6] { 1, 2, 8, 4, 16, 0 };

	private CardColorType tmpCardSortType;

	private EquipHelper.CARD_SORT_KEY tmpCardSortKey;

	public void SetCardType(int nBID)
	{
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardMainSortType & (uint)ntyps[nBID]) == (uint)ntyps[nBID])
		{
			ManagedSingleton<EquipHelper>.Instance.nCardMainSortType = (CardColorType)((int)ManagedSingleton<EquipHelper>.Instance.nCardMainSortType & ~ntyps[nBID]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.nCardMainSortType = (CardColorType)((int)ManagedSingleton<EquipHelper>.Instance.nCardMainSortType | ntyps[nBID]);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		for (int i = 0; i < CardType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardMainSortType & (uint)ntyps[i]) == (uint)ntyps[i])
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
		if (ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey != (EquipHelper.CARD_SORT_KEY)num)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey = (EquipHelper.CARD_SORT_KEY)num;
		}
		for (int i = 0; i < SortType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & (uint)(1 << i)) != 0)
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
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardMainSortType & (uint)ntyps[i]) == (uint)ntyps[i])
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
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & (uint)(1 << j)) != 0)
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
			SortType[j].gameObject.SetActive(true);
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
		ResetBtn.gameObject.SetActive(true);
		DeployBtn.gameObject.SetActive(true);
		FavoriteBtn.gameObject.SetActive(true);
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
			m_listNetCardInfo.Add(list[i].netCardInfo);
		}
		OnSortGo(bUpdateScrollRect, bOffset);
		OnUpdateCardMaxSlot();
		if (ManagedSingleton<EquipHelper>.Instance.CardMainSortDescend == 1)
		{
			SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	public override void OnClickCloseBtn()
	{
		LeanTween.cancel(ref TweenScaleId);
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

	public void OnClickSellBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardSell", delegate(CardSellUI ui)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.Setup();
		});
	}

	public void OnUpdateCardMaxSlot()
	{
		int cARD_INITIAL_SLOT = OrangeConst.CARD_INITIAL_SLOT;
		cARD_INITIAL_SLOT += ManagedSingleton<PlayerHelper>.Instance.GetCardExpansion();
		cARD_INITIAL_SLOT = ((cARD_INITIAL_SLOT > OrangeConst.CARD_MAX_SLOT) ? OrangeConst.CARD_MAX_SLOT : cARD_INITIAL_SLOT);
		SlotText.text = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Count + "/" + cARD_INITIAL_SLOT;
	}

	public void OnClickBuyCardStorageSlot()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardStorageBuy", delegate(CardStorageBuyUI ui)
		{
			ui.IsInfinity = true;
			ui.CostAmount = OrangeConst.CARD_EXPANSION_COST;
			ui.CostAmountMax = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel();
			int num = OrangeConst.CARD_INITIAL_SLOT + ManagedSingleton<PlayerHelper>.Instance.GetCardExpansion();
			int maxCount = (OrangeConst.CARD_MAX_SLOT - num) / OrangeConst.CARD_EXPANSION;
			string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_SLOT_EXPANSION");
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(OrangeConst.CARD_EXPANSION, maxCount, str, 15, 2, delegate(object obj)
			{
				int amount = (int)obj;
				ManagedSingleton<PlayerNetManager>.Instance.ExpandCardStorageReq(amount, delegate
				{
					OnUpdateCardMaxSlot();
				});
			});
		});
	}

	public void OnShowProtected()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		ShowProtectedBtn.gameObject.SetActive(false);
		HideProtectedBtn.gameObject.SetActive(true);
		nProtected = 1;
		ShowFavoriteBtn.gameObject.SetActive(true);
		HideFavoriteBtn.gameObject.SetActive(false);
		nFavorite = 0;
	}

	public void OnHideProtected()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
		ShowProtectedBtn.gameObject.SetActive(true);
		HideProtectedBtn.gameObject.SetActive(false);
		nProtected = 0;
	}

	public void OnClickSortPanelBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		tmpCardSortType = ManagedSingleton<EquipHelper>.Instance.nCardMainSortType;
		tmpCardSortKey = ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey;
		SortRoot.SetActive(true);
		MaskImage.gameObject.SetActive(true);
	}

	public void OnCloseSortRoot()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ManagedSingleton<EquipHelper>.Instance.nCardMainSortType = tmpCardSortType;
		ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey = tmpCardSortKey;
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
			if (value != null && ((uint)ManagedSingleton<EquipHelper>.Instance.nCardMainSortType & (uint)value.n_TYPE) == (uint)value.n_TYPE)
			{
				m_listNetCardInfoFiltered.Add(item);
			}
		}
		m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardID.CompareTo(y.CardID));
		if ((ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY) == EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY)
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
				int num7 = value2.n_RARITY.CompareTo(value3.n_RARITY);
				if (num7 == 0)
				{
					num7 = x.CardID.CompareTo(y.CardID);
				}
				return num7;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR) == EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num6 = x.Star.CompareTo(y.Star);
				if (num6 == 0)
				{
					num6 = x.CardID.CompareTo(y.CardID);
				}
				return num6;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_LV) == EquipHelper.CARD_SORT_KEY.CARD_SORT_LV)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num5 = x.Exp.CompareTo(y.Exp);
				if (num5 == 0)
				{
					num5 = x.CardID.CompareTo(y.CardID);
				}
				return num5;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_FAVORITE) == EquipHelper.CARD_SORT_KEY.CARD_SORT_FAVORITE)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num4 = x.Favorite.CompareTo(y.Favorite);
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
		if (ManagedSingleton<EquipHelper>.Instance.CardMainSortDescend == 1)
		{
			m_listNetCardInfoFiltered.Reverse();
		}
		int count = m_listNetCardInfoFiltered.Count;
		int num = (count - count % 6) / 6;
		if (count % 6 > 0)
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
			int num3 = (num2 - num2 % 6) / 6;
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
		ManagedSingleton<EquipHelper>.Instance.CardMainSortDescend = ((ManagedSingleton<EquipHelper>.Instance.CardMainSortDescend != 1) ? 1 : 0);
		if (ManagedSingleton<EquipHelper>.Instance.CardMainSortDescend == 1)
		{
			SortOrderImg.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImg.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		OnSortGo();
	}

	public void OnCardReset()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardReset", delegate(CardResetUI ui)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		});
	}

	public void OnCardDeploy()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardDeployMain", delegate(CardDeployMain ui)
		{
			CharacterInfo characterInfo = new CharacterInfo();
			characterInfo.netInfo = new NetCharacterInfo();
			characterInfo.netInfo.CharacterID = 1;
			characterInfo.netInfo.Star = 5;
			characterInfo.netInfo.State = 1;
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(characterInfo, true, true);
		});
	}

	public void OnShowFavorite()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		ShowFavoriteBtn.gameObject.SetActive(false);
		HideFavoriteBtn.gameObject.SetActive(true);
		nFavorite = 1;
		ShowProtectedBtn.gameObject.SetActive(true);
		HideProtectedBtn.gameObject.SetActive(false);
		nProtected = 0;
	}

	public void OnHideFavorite()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
		ShowFavoriteBtn.gameObject.SetActive(true);
		HideFavoriteBtn.gameObject.SetActive(false);
		nFavorite = 0;
	}
}
