using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxEquipCompose : OrangeUIBase
{
	[SerializeField]
	private Toggle m_mainEquipLvlTab;

	[SerializeField]
	private Toggle m_normalEquipTab;

	[SerializeField]
	private Toggle m_advancedEquipTab;

	[SerializeField]
	private EquipIcon m_upperBody;

	[SerializeField]
	private EquipIcon m_lowerBody;

	[SerializeField]
	private EquipIcon m_shoes;

	[SerializeField]
	private EquipIcon m_head;

	[SerializeField]
	private EquipIcon m_hand;

	[SerializeField]
	private EquipIcon m_part;

	[SerializeField]
	private ItemIconBase m_material;

	[SerializeField]
	private Image m_equipIconSelectFrame;

	[SerializeField]
	private OrangeText m_defenseMinMax;

	[SerializeField]
	private OrangeText m_lifeMinMax;

	[SerializeField]
	private OrangeText m_luckMinMax;

	[SerializeField]
	private OrangeText m_playerDefense;

	[SerializeField]
	private OrangeText m_playerLife;

	[SerializeField]
	private OrangeText m_playerLuck;

	[SerializeField]
	private OrangeText m_suitEffectTitle;

	[SerializeField]
	private OrangeText m_suitEffect1;

	[SerializeField]
	private OrangeText m_suitEffect2;

	[SerializeField]
	private OrangeText m_suitEffect3;

	[SerializeField]
	private OrangeText m_materialNum;

	[SerializeField]
	private OrangeText m_equipmentName;

	[SerializeField]
	private Image m_materialBar;

	[SerializeField]
	private Button m_composeBtn;

	[SerializeField]
	private Transform m_naviPos;

	[SerializeField]
	private ScrollRect m_levelTabScrollView;

	private int m_normalUnlockID = 440001;

	private int m_advanceUnlockID = 440002;

	private Dictionary<int, NetEquipmentInfo> m_equippedInfoDict;

	private Dictionary<int, Toggle> m_equipLvlTabDict = new Dictionary<int, Toggle>();

	private List<int> m_equipLvlTabSortedList = new List<int>();

	private List<ComposableEquipment> m_composableEquipmentList = new List<ComposableEquipment>();

	private List<int> m_advanceEquipmentList = new List<int>();

	private EQUIP_TABLE[] m_equipTable = new EQUIP_TABLE[6];

	private int m_currentUnlockID;

	private int m_currentLevel;

	private EQUIP_TABLE m_currentSelection;

	private ITEM_TABLE m_currentItemTable;

	private ToggleGroup m_gradeToggleGroup;

	private bool ignoreFristSE = true;

	private bool ignoreTabSE;

	private int materialRequestCount = -1;

	private void Start()
	{
	}

	public void Setup()
	{
		if (m_naviPos.GetComponentInChildren<StandNaviDb>() == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_2"), "ch_navi_2_db", delegate(GameObject obj)
			{
				StandNaviDb component = Object.Instantiate(obj, m_naviPos, false).GetComponent<StandNaviDb>();
				if ((bool)component)
				{
					component.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL);
				}
			});
		}
		m_gradeToggleGroup = m_normalEquipTab.GetComponentInParent<ToggleGroup>();
		m_equippedInfoDict = ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip();
		foreach (KeyValuePair<int, EQUIP_TABLE> item2 in ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT)
		{
			EQUIP_TABLE value = item2.Value;
			if (value.n_UNLOCK_ID != 0)
			{
				ComposableEquipment item = new ComposableEquipment(value.n_LV, value.n_ID);
				m_composableEquipmentList.Insert(0, item);
				if (value.n_UNLOCK_ID == m_advanceUnlockID)
				{
					m_advanceEquipmentList.Add(value.n_LV);
				}
			}
		}
		CreateTabs();
		if (m_equipLvlTabSortedList.Count > 0)
		{
			OnClickLevelTab(m_equipLvlTabSortedList[0]);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 23);
		ignoreFristSE = false;
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	public void OnClickNormalRank()
	{
		if (m_normalEquipTab.isOn && m_equipLvlTabSortedList.Count != 0)
		{
			if (m_currentUnlockID != m_normalUnlockID && !ignoreTabSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			}
			if (m_currentUnlockID != m_normalUnlockID)
			{
				m_currentUnlockID = m_normalUnlockID;
				ClickNormalRank();
			}
		}
	}

	private void ClickNormalRank()
	{
		SetupIcons(m_currentLevel);
		ignoreTabSE = false;
	}

	public void OnClickAdvanceRank()
	{
		if (m_advancedEquipTab.isOn && m_equipLvlTabSortedList.Count != 0)
		{
			if (m_currentUnlockID != m_advanceUnlockID && !ignoreTabSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			}
			if (m_currentUnlockID != m_advanceUnlockID)
			{
				m_currentUnlockID = m_advanceUnlockID;
				SetupIcons(m_currentLevel);
			}
		}
	}

	public void OnClickComposeBtn()
	{
		if (!ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog() && m_currentSelection != null)
		{
			m_currentItemTable = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[m_currentUnlockID];
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(m_currentItemTable.w_NAME);
			string l10nValue2 = ManagedSingleton<OrangeTextDataManager>.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(m_currentSelection.w_NAME);
			string info = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_MAKE_DETAIL"), l10nValue, l10nValue2);
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
				ui.YesSE = SystemSE.NONE;
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CONFIRM_EQUIP_MAKE"), info, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_NO"), DisplayComposeCompleteDialog);
			});
		}
	}

	private void DisplayComposeCompleteDialog()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		ManagedSingleton<PlayerNetManager>.Instance.ComposeEquipmentReq(m_currentSelection.n_ID, delegate(NetEquipmentInfo res)
		{
			m_naviPos.gameObject.SetActive(false);
			NetEquipmentInfo netEquipmentInfo = res;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EquipGetPopup", delegate(EquipGetPopup ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_EFFECT04);
				ui.Setup(netEquipmentInfo, delegate
				{
					m_naviPos.gameObject.SetActive(true);
				});
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			});
			int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(m_currentUnlockID);
			string text = string.Format("{0}/{1}", itemValue, m_currentSelection.n_UNLOCK_COUNT);
			m_materialNum.text = text;
			float num = (float)itemValue / (float)m_currentSelection.n_UNLOCK_COUNT;
			if (num > 1f)
			{
				num = 1f;
			}
			m_materialBar.transform.localScale = new Vector3(num, 1f, 1f);
			m_composeBtn.interactable = itemValue >= m_currentSelection.n_UNLOCK_COUNT;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_PLAYER_BOX);
		});
	}

	private void OnClickLevelTab(int level)
	{
		bool interactable = m_advanceEquipmentList.Contains(level);
		m_advancedEquipTab.interactable = interactable;
		if (level != m_currentLevel && !ignoreFristSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		}
		m_currentLevel = level;
		m_currentUnlockID = m_normalUnlockID;
		if ((bool)m_gradeToggleGroup)
		{
			ignoreTabSE = true;
			m_normalEquipTab.isOn = true;
			m_gradeToggleGroup.NotifyToggleOn(m_normalEquipTab);
			ClickNormalRank();
		}
	}

	private void CreateTabs()
	{
		int num = 0;
		int num2 = 0;
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		foreach (ComposableEquipment composableEquipment in m_composableEquipmentList)
		{
			if (composableEquipment.m_level <= lV && num != composableEquipment.m_level)
			{
				int callbackLvl;
				num = (callbackLvl = composableEquipment.m_level);
				Toggle toggle = Object.Instantiate(m_mainEquipLvlTab, m_mainEquipLvlTab.transform.parent);
				toggle.transform.localPosition += new Vector3(0f, -130f * (float)num2, 0f);
				toggle.onValueChanged.AddListener(delegate
				{
					OnClickLevelTab(callbackLvl);
				});
				m_equipLvlTabDict.Add(num, toggle);
				if (num2 != 0)
				{
					toggle.isOn = false;
				}
				OrangeText[] componentsInChildren = toggle.GetComponentsInChildren<OrangeText>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_LV_TAG"), num);
				}
				num2++;
			}
		}
		m_mainEquipLvlTab.gameObject.SetActive(false);
		float num3 = 100f;
		float num4 = 150f;
		m_levelTabScrollView.content.sizeDelta = new Vector2(0f, num4 * (float)num2 - 1f + num3);
		m_equipLvlTabSortedList = m_equipLvlTabDict.Keys.ToList();
		m_equipLvlTabSortedList.Sort((int x, int y) => -x.CompareTo(y));
	}

	private void SetupIcons(int level)
	{
		foreach (ComposableEquipment composableEquipment in m_composableEquipmentList)
		{
			EQUIP_TABLE value;
			if (composableEquipment.m_level == level && ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.TryGetValue(composableEquipment.m_id, out value) && value.n_UNLOCK_ID == m_currentUnlockID)
			{
				m_equipTable[value.n_PARTS - 1] = value;
				switch (value.n_PARTS)
				{
				case 1:
					SetupIconHelper(m_upperBody, value);
					break;
				case 2:
					SetupIconHelper(m_lowerBody, value);
					break;
				case 3:
					SetupIconHelper(m_shoes, value);
					break;
				case 4:
					SetupIconHelper(m_head, value);
					break;
				case 5:
					SetupIconHelper(m_hand, value);
					break;
				case 6:
					SetupIconHelper(m_part, value);
					break;
				}
			}
		}
		m_currentItemTable = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[m_currentUnlockID];
		m_material.gameObject.SetActive(true);
		m_material.SetRare(m_currentItemTable.n_RARE);
		m_material.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(m_currentItemTable.s_ICON), m_currentItemTable.s_ICON, OnClickMaterialIcon);
		ClickEquip(4);
	}

	private void OnClickMaterialIcon(int p_idx)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
		{
			ui.Setup(m_currentItemTable, materialRequestCount);
		});
	}

	private void SetupIconHelper(EquipIcon equipIcon, EQUIP_TABLE equipTable)
	{
		equipIcon.gameObject.SetActive(true);
		equipIcon.SetStarAndLv(0, equipTable.n_LV);
		equipIcon.SetRare(equipTable.n_RARE);
		equipIcon.Setup(equipTable.n_PARTS, AssetBundleScriptableObject.Instance.m_iconEquip, equipTable.s_ICON, OnClickEquip);
	}

	private void DisplaySuitEffect()
	{
		SUIT_TABLE sUIT_TABLE = null;
		Dictionary<int, NetEquipmentInfo> dicEquipmentIsEquip = ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip();
		if (m_currentSelection.n_PARTS == 1)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_1 == m_currentSelection.n_ID).FirstOrDefault();
		}
		else if (m_currentSelection.n_PARTS == 2)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_2 == m_currentSelection.n_ID).FirstOrDefault();
		}
		else if (m_currentSelection.n_PARTS == 3)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_3 == m_currentSelection.n_ID).FirstOrDefault();
		}
		else if (m_currentSelection.n_PARTS == 4)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_4 == m_currentSelection.n_ID).FirstOrDefault();
		}
		else if (m_currentSelection.n_PARTS == 5)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_5 == m_currentSelection.n_ID).FirstOrDefault();
		}
		else if (m_currentSelection.n_PARTS == 6)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_6 == m_currentSelection.n_ID).FirstOrDefault();
		}
		if (sUIT_TABLE != null)
		{
			NetEquipmentInfo value = null;
			int num = 0;
			for (int i = 1; i <= 6; i++)
			{
				if (dicEquipmentIsEquip.TryGetValue(i, out value))
				{
					if (value.EquipItemID == sUIT_TABLE.n_EQUIP_1)
					{
						num++;
					}
					else if (value.EquipItemID == sUIT_TABLE.n_EQUIP_2)
					{
						num++;
					}
					else if (value.EquipItemID == sUIT_TABLE.n_EQUIP_3)
					{
						num++;
					}
					else if (value.EquipItemID == sUIT_TABLE.n_EQUIP_4)
					{
						num++;
					}
					else if (value.EquipItemID == sUIT_TABLE.n_EQUIP_5)
					{
						num++;
					}
					else if (value.EquipItemID == sUIT_TABLE.n_EQUIP_6)
					{
						num++;
					}
				}
			}
			m_suitEffectTitle.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SUIT_EFFECT"), num, 6);
			OrangeText suitEffect = m_suitEffect1;
			OrangeText suitEffect2 = m_suitEffect2;
			Color color2 = (m_suitEffect3.color = new Color(m_suitEffect1.color.r, m_suitEffect1.color.g, m_suitEffect1.color.b, 1f));
			Color color4 = (suitEffect2.color = color2);
			suitEffect.color = color4;
			if (sUIT_TABLE.n_SUIT_1 > num)
			{
				m_suitEffect1.color = new Color(m_suitEffect1.color.r, m_suitEffect1.color.g, m_suitEffect1.color.b, 0.5f);
			}
			if (sUIT_TABLE.n_SUIT_2 > num)
			{
				m_suitEffect2.color = new Color(m_suitEffect2.color.r, m_suitEffect2.color.g, m_suitEffect2.color.b, 0.5f);
			}
			if (sUIT_TABLE.n_SUIT_3 > num)
			{
				m_suitEffect3.color = new Color(m_suitEffect3.color.r, m_suitEffect3.color.g, m_suitEffect3.color.b, 0.5f);
			}
			SKILL_TABLE value2;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(sUIT_TABLE.n_EFFECT_1, out value2))
			{
				m_suitEffect1.text = string.Format("({0}){1}", sUIT_TABLE.n_SUIT_1, ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value2.w_TIP));
			}
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(sUIT_TABLE.n_EFFECT_2, out value2))
			{
				m_suitEffect2.text = string.Format("({0}){1}", sUIT_TABLE.n_SUIT_2, ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value2.w_TIP));
			}
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(sUIT_TABLE.n_EFFECT_3, out value2))
			{
				m_suitEffect3.text = string.Format("({0}){1}", sUIT_TABLE.n_SUIT_3, ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value2.w_TIP));
			}
		}
		else
		{
			m_suitEffect1.text = " ";
			m_suitEffect2.text = " ";
			m_suitEffect3.text = " ";
		}
	}

	private void OnClickEquip(int idx)
	{
		if (m_equipTable[idx - 1] != m_currentSelection)
		{
			if (ignoreFristSE)
			{
				ignoreFristSE = !ignoreFristSE;
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			}
		}
		ClickEquip(idx);
	}

	private void ClickEquip(int idx)
	{
		EQUIP_TABLE eQUIP_TABLE = (m_currentSelection = m_equipTable[idx - 1]);
		m_defenseMinMax.text = string.Format("{0}-{1}", eQUIP_TABLE.n_DEF_MIN, eQUIP_TABLE.n_DEF_MAX);
		m_lifeMinMax.text = string.Format("{0}-{1}", eQUIP_TABLE.n_HP_MIN, eQUIP_TABLE.n_HP_MAX);
		m_luckMinMax.text = string.Format("{0}-{1}", eQUIP_TABLE.n_LUK_MIN, eQUIP_TABLE.n_LUK_MAX);
		m_equipmentName.text = ManagedSingleton<OrangeTextDataManager>.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(eQUIP_TABLE.w_NAME);
		m_playerDefense.text = "---";
		m_playerLife.text = "---";
		m_playerLuck.text = "---";
		NetEquipmentInfo value;
		if (m_equippedInfoDict.TryGetValue(idx, out value))
		{
			m_playerDefense.text = value.DefParam.ToString();
			m_playerLife.text = value.HpParam.ToString();
			m_playerLuck.text = value.LukParam.ToString();
		}
		DisplaySuitEffect();
		int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(m_currentUnlockID);
		materialRequestCount = eQUIP_TABLE.n_UNLOCK_COUNT;
		string text = string.Format("{0}/{1}", itemValue, materialRequestCount);
		m_materialNum.text = text;
		float num = (float)itemValue / (float)materialRequestCount;
		if (num > 1f)
		{
			num = 1f;
		}
		m_materialBar.transform.localScale = new Vector3(num, 1f, 1f);
		m_composeBtn.interactable = itemValue >= materialRequestCount;
		switch (idx)
		{
		default:
			m_equipIconSelectFrame.transform.localPosition = m_upperBody.transform.localPosition;
			break;
		case 2:
			m_equipIconSelectFrame.transform.localPosition = m_lowerBody.transform.localPosition;
			break;
		case 3:
			m_equipIconSelectFrame.transform.localPosition = m_shoes.transform.localPosition;
			break;
		case 4:
			m_equipIconSelectFrame.transform.localPosition = m_head.transform.localPosition;
			break;
		case 5:
			m_equipIconSelectFrame.transform.localPosition = m_hand.transform.localPosition;
			break;
		case 6:
			m_equipIconSelectFrame.transform.localPosition = m_part.transform.localPosition;
			break;
		}
	}
}
