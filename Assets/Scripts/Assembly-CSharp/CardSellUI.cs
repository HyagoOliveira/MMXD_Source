using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardSellUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect m_scrollRect;

	[SerializeField]
	private CardSellCell m_cardSellCell;

	[SerializeField]
	private Text m_moneyCount;

	[SerializeField]
	private LoopVerticalScrollRect m_levelToggleScrollRect;

	[SerializeField]
	private CardToggle m_CardToggleRef;

	[SerializeField]
	private Button m_confirmBtn;

	[SerializeField]
	private Text TextNotification;

	[SerializeField]
	private Text TextCardBook;

	private List<NetCardInfo> m_listNetCardInfo = new List<NetCardInfo>();

	private List<NetCardInfo> m_listNetCardInfoFiltered = new List<NetCardInfo>();

	private List<int> m_sortedLevelList = new List<int>();

	private Dictionary<int, NetCardInfo> m_dictSelectedCardInfo = new Dictionary<int, NetCardInfo>();

	private Dictionary<int, bool> m_dictLevelToggle = new Dictionary<int, bool>();

	private List<NetCharacterCardSlotInfo> tCharacterCardSlotInfoList = new List<NetCharacterCardSlotInfo>();

	public void Setup(bool bForceSelectAll = false)
	{
		List<CardInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].netCardInfo.Protected == 0 && list[i].netCardInfo.Favorite == 0)
			{
				m_listNetCardInfo.Add(list[i].netCardInfo);
			}
		}
		CollectEquipmentLevels(bForceSelectAll);
		RefreshEquipmentList();
		m_moneyCount.text = "x0";
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		TextCardBook.gameObject.SetActive(true);
	}

	public void SetCardIcon(CardSellCell p_unit)
	{
		NetCardInfo netCardInfo = m_listNetCardInfoFiltered[p_unit.NowIdx];
		CARD_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netCardInfo.CardID, out value);
		if (value != null)
		{
			int cardRank = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(netCardInfo.Exp);
			p_unit.CardIcon.SetStarAndLv(netCardInfo.Star, cardRank);
			p_unit.CardIcon.SetRare(value.n_RARITY);
			p_unit.CardIcon.SetTypeImage(value.n_TYPE);
			p_unit.CardIcon.SetLockImage(netCardInfo.Protected == 1);
			p_unit.CardIcon.SetFavoriteImage(netCardInfo.Favorite == 1);
			string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
			string s_ICON = value.s_ICON;
			p_unit.CardIcon.Setup(p_unit.NowIdx, p_bundleName, s_ICON, OnClickCard);
			p_unit.SetCardID(netCardInfo.CardSeqID, netCardInfo.CardID);
			p_unit.SetSelection(false);
		}
		else
		{
			p_unit.CardIcon.Clear();
		}
	}

	public void SetupLevelToggle(EquipmentDismantleToggle levelToggle)
	{
		levelToggle.GetToggle().onValueChanged.RemoveAllListeners();
		levelToggle.Setup(m_sortedLevelList[levelToggle.GetIndex()]);
		levelToggle.GetToggle().isOn = m_dictLevelToggle[levelToggle.GetLevel()];
		levelToggle.GetToggle().onValueChanged.AddListener(delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			m_dictLevelToggle[levelToggle.GetLevel()] = !m_dictLevelToggle[levelToggle.GetLevel()];
			if (!m_dictLevelToggle[levelToggle.GetLevel()])
			{
				RemoveLevelFromEquipmentList(levelToggle.GetLevel());
			}
			RefreshEquipmentList();
		});
	}

	private void RemoveLevelFromEquipmentList(int level)
	{
		KeyValuePair<int, NetCardInfo>[] array = m_dictSelectedCardInfo.Where((KeyValuePair<int, NetCardInfo> x) => ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[x.Value.CardID].n_EXP == level).ToArray();
		foreach (KeyValuePair<int, NetCardInfo> keyValuePair in array)
		{
			m_dictSelectedCardInfo.Remove(keyValuePair.Key);
		}
	}

	private void RefreshEquipmentList()
	{
		m_dictSelectedCardInfo.Clear();
		m_listNetCardInfoFiltered.Clear();
		if (m_dictSelectedCardInfo.Count == 0)
		{
			m_confirmBtn.interactable = false;
		}
		UpdateCheckCardUesdData();
		foreach (NetCardInfo item in m_listNetCardInfo)
		{
			CARD_TABLE value;
			if (!CheckCardUesd(item.CardSeqID) && ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(item.CardID, out value) && ((uint)ManagedSingleton<EquipHelper>.Instance.nCardMainSortType & (uint)value.n_TYPE) == (uint)value.n_TYPE)
			{
				m_listNetCardInfoFiltered.Add(item);
				int n_RARITY = value.n_RARITY;
				bool value2 = false;
				if (m_dictLevelToggle.TryGetValue(n_RARITY, out value2) && value2)
				{
					m_dictSelectedCardInfo.Add(item.CardSeqID, item);
				}
			}
		}
		m_confirmBtn.interactable = m_dictSelectedCardInfo.Count > 0;
		OnSortGo();
		UpdateDismantleResult(0.3f);
	}

	private void CollectEquipmentLevels(bool bForceSelectAll = false)
	{
		bool value = true;
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>(m_dictLevelToggle);
		m_dictLevelToggle.Clear();
		foreach (NetCardInfo item in m_listNetCardInfo)
		{
			CARD_TABLE value2;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(item.CardID, out value2))
			{
				continue;
			}
			int n_RARITY = value2.n_RARITY;
			if (!m_dictLevelToggle.ContainsKey(n_RARITY))
			{
				if (dictionary.TryGetValue(n_RARITY, out value))
				{
					m_dictLevelToggle.Add(n_RARITY, bForceSelectAll || value);
				}
				else
				{
					m_dictLevelToggle.Add(n_RARITY, false);
				}
			}
		}
		m_dictLevelToggle.Keys.ToList().Sort((int x, int y) => x.CompareTo(y));
		m_sortedLevelList = m_dictLevelToggle.Keys.ToList();
		m_sortedLevelList.Sort((int x, int y) => x.CompareTo(y));
		if ((bool)m_CardToggleRef)
		{
			m_levelToggleScrollRect.OrangeInit(m_CardToggleRef, m_dictLevelToggle.Count, m_dictLevelToggle.Count);
		}
	}

	public void SetupCardToggle(CardToggle cardToggle)
	{
		cardToggle.GetToggle().onValueChanged.RemoveAllListeners();
		cardToggle.Setup(m_sortedLevelList[cardToggle.GetIndex()]);
		cardToggle.GetToggle().isOn = m_dictLevelToggle[cardToggle.GetRarity()];
		cardToggle.GetToggle().onValueChanged.AddListener(delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			m_dictLevelToggle[cardToggle.GetRarity()] = !m_dictLevelToggle[cardToggle.GetRarity()];
			if (!m_dictLevelToggle[cardToggle.GetRarity()])
			{
				RemoveLevelFromEquipmentList(cardToggle.GetRarity());
			}
			RefreshEquipmentList();
		});
	}

	private void OnClickCard(int p_idx)
	{
		NetCardInfo netCardInfo = m_listNetCardInfoFiltered[p_idx];
		CARD_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netCardInfo.CardID, out value);
		if (value != null)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			if (m_dictSelectedCardInfo.ContainsKey(netCardInfo.CardSeqID))
			{
				m_dictSelectedCardInfo.Remove(netCardInfo.CardSeqID);
			}
			else
			{
				m_dictSelectedCardInfo.Add(netCardInfo.CardSeqID, netCardInfo);
			}
			m_confirmBtn.interactable = m_dictSelectedCardInfo.Count > 0;
			UpdateDismantleResult(0.3f);
		}
	}

	private void UpdateDismantleResult(float timer = 0f)
	{
		int num = 0;
		foreach (KeyValuePair<int, NetCardInfo> item in m_dictSelectedCardInfo)
		{
			NetCardInfo value = item.Value;
			EXP_TABLE cardExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(value.Exp);
			CARD_TABLE value2 = null;
			ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(value.CardID, out value2);
			if (value2 != null && cardExpTable != null)
			{
				num += value2.n_MONEY * cardExpTable.n_ID;
			}
		}
		LeanTween.value(int.Parse(m_moneyCount.text.Substring(1)), num, timer).setOnUpdate(delegate(float val)
		{
			m_moneyCount.text = string.Format("x{0}", (int)val);
		});
		TextNotification.gameObject.SetActive(CheckSellCardRarity(4));
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
		m_dictSelectedCardInfo.Clear();
		if (m_scrollRect.prefabSource.pbo != null)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(m_scrollRect.prefabSource.pbo.itemName);
		}
		if (m_levelToggleScrollRect.prefabSource.pbo != null)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(m_levelToggleScrollRect.prefabSource.pbo.itemName);
		}
	}

	public void OnExecuteCardSell()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_STORE02);
		List<int> list = new List<int>();
		List<NetCardInfo> list2 = m_dictSelectedCardInfo.Values.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			list.Add(list2[i].CardSeqID);
		}
		ManagedSingleton<PlayerNetManager>.Instance.CardSellReq(list, delegate(List<NetRewardInfo> rewardList)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(rewardList);
			});
			CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
			if (uI != null)
			{
				uI.Setup();
			}
			base.CloseSE = SystemSE.NONE;
			OnClickCloseBtn();
		});
	}

	private bool CheckSellCardRarity(int nRarity)
	{
		List<NetCardInfo> list = m_dictSelectedCardInfo.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			CARD_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(list[i].CardID, out value);
			if (value != null && value.n_RARITY >= nRarity)
			{
				return true;
			}
		}
		return false;
	}

	public void OnClickDismantleBtn()
	{
		if (m_dictSelectedCardInfo.Count == 0)
		{
			return;
		}
		if (CheckSellCardRarity(4))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardSellConfirm", delegate(CardSellConfirmUI ui)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_SELL_CONFIRM");
				string str2 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_SELL_RARITY_CONFIRM");
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.Setup(m_dictSelectedCardInfo.Values.ToList(), str, str2, false, OnExecuteCardSell);
			});
		}
		else
		{
			OnExecuteCardSell();
		}
	}

	public bool IsLevelToggleSelected(int level)
	{
		bool value = false;
		m_dictLevelToggle.TryGetValue(level, out value);
		return value;
	}

	public bool IsSelected(int CardSeqID)
	{
		return m_dictSelectedCardInfo.ContainsKey(CardSeqID);
	}

	public void OnSelectCardSellIcon(CardSellCell icon)
	{
		icon.ToggleSelection();
	}

	public void OnSortGo()
	{
		m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardID.CompareTo(y.CardID));
		if ((ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY) == EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				CARD_TABLE value = null;
				CARD_TABLE value2 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(x.CardID, out value);
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(y.CardID, out value2);
				if (value == null || value2 == null)
				{
					return 0;
				}
				int num3 = value.n_RARITY.CompareTo(value2.n_RARITY);
				if (num3 == 0)
				{
					num3 = x.CardID.CompareTo(y.CardID);
				}
				return num3;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR) == EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num2 = x.Star.CompareTo(y.Star);
				if (num2 == 0)
				{
					num2 = x.CardID.CompareTo(y.CardID);
				}
				return num2;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardMainSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_LV) == EquipHelper.CARD_SORT_KEY.CARD_SORT_LV)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num = x.Exp.CompareTo(y.Exp);
				if (num == 0)
				{
					num = x.CardID.CompareTo(y.CardID);
				}
				return num;
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
		m_scrollRect.OrangeInit(m_cardSellCell, m_listNetCardInfoFiltered.Count, m_listNetCardInfoFiltered.Count);
	}

	private void UpdateCheckCardUesdData()
	{
		List<int> list = ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.Keys.ToList();
		tCharacterCardSlotInfoList.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			int key = list[i];
			Dictionary<int, NetCharacterCardSlotInfo> value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.TryGetValue(key, out value))
			{
				tCharacterCardSlotInfoList.AddRange(value.Values.ToList());
			}
		}
	}

	private bool CheckCardUesd(int SeqID)
	{
		for (int i = 0; i < tCharacterCardSlotInfoList.Count; i++)
		{
			if (tCharacterCardSlotInfoList[i].CardSeqID == SeqID)
			{
				return true;
			}
		}
		return false;
	}
}
