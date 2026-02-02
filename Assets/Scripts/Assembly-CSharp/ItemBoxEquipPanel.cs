using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxEquipPanel : MonoBehaviour
{
	[SerializeField]
	private StarClearComponent m_starRootDefense;

	[SerializeField]
	private StarClearComponent m_starRootLife;

	[SerializeField]
	private StarClearComponent m_starRootLuck;

	[SerializeField]
	private EquipIcon m_equipIcon;

	[SerializeField]
	private Text m_equipmentName;

	[SerializeField]
	private Text m_firePower;

	[SerializeField]
	private Text m_defenseNum;

	[SerializeField]
	private Text m_lifeNum;

	[SerializeField]
	private Text m_luckNum;

	[SerializeField]
	private Transform m_defenseBar;

	[SerializeField]
	private Transform m_lifeBar;

	[SerializeField]
	private Transform m_luckBar;

	[SerializeField]
	private Button m_panelBtn;

	[SerializeField]
	private Text m_defenseDiff;

	[SerializeField]
	private Text m_lifeDiff;

	[SerializeField]
	private Text m_luckDiff;

	[SerializeField]
	private OrangeText m_suitEffectTitle;

	[SerializeField]
	private OrangeText m_suitEffect1;

	[SerializeField]
	private OrangeText m_suitEffect2;

	[SerializeField]
	private OrangeText m_suitEffect3;

	[SerializeField]
	private OrangeText m_suitEffect4;

	[SerializeField]
	private OrangeText m_suitEffect5;

	private Dictionary<int, NetEquipmentInfo> m_dicNetEquipInfo;

	private NetEquipmentInfo m_netEquipSelected;

	private EQUIP_TABLE m_selectedEquipTable;

	private void Start()
	{
	}

	public void Setup(NetEquipmentInfo netEquip)
	{
		m_starRootDefense.SetActiveStar(0);
		m_starRootLife.SetActiveStar(0);
		m_starRootLuck.SetActiveStar(0);
		m_defenseDiff.gameObject.SetActive(false);
		m_lifeDiff.gameObject.SetActive(false);
		m_luckDiff.gameObject.SetActive(false);
		m_dicNetEquipInfo = ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip();
		m_netEquipSelected = netEquip;
		ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(m_netEquipSelected.EquipItemID, out m_selectedEquipTable);
		SetEquipIcon();
	}

	private void SetEquipIcon()
	{
		EQUIP_TABLE equip = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(m_netEquipSelected.EquipItemID, out equip))
		{
			int[] equipRank = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(m_netEquipSelected);
			m_equipIcon.SetStarAndLv(equipRank[3], equip.n_LV);
			m_equipIcon.SetRare(equip.n_RARE);
			m_equipIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconEquip, equip.s_ICON);
			m_equipmentName.text = ManagedSingleton<OrangeTextDataManager>.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(equip.w_NAME);
			m_defenseNum.text = string.Format("{0}", m_netEquipSelected.DefParam);
			m_lifeNum.text = string.Format("{0}", m_netEquipSelected.HpParam);
			m_luckNum.text = string.Format("{0}", m_netEquipSelected.LukParam);
			float x = (float)(m_netEquipSelected.DefParam - equip.n_DEF_MIN) / (float)(equip.n_DEF_MAX - equip.n_DEF_MIN);
			float x2 = (float)(m_netEquipSelected.HpParam - equip.n_HP_MIN) / (float)(equip.n_HP_MAX - equip.n_HP_MIN);
			float x3 = (float)(m_netEquipSelected.LukParam - equip.n_LUK_MIN) / (float)(equip.n_LUK_MAX - equip.n_LUK_MIN);
			m_defenseBar.localScale = new Vector3(x, 1f, 1f);
			m_lifeBar.localScale = new Vector3(x2, 1f, 1f);
			m_luckBar.localScale = new Vector3(x3, 1f, 1f);
			m_starRootDefense.SetActiveStar(equipRank[0]);
			m_starRootLife.SetActiveStar(equipRank[1]);
			m_starRootLuck.SetActiveStar(equipRank[2]);
			DisplaySuitEffect();
			int num = OrangeConst.BP_DEF * m_netEquipSelected.DefParam + OrangeConst.BP_HP * m_netEquipSelected.HpParam;
			m_firePower.text = string.Format("{0}", num);
		}
	}

	private void DisplaySuitEffect()
	{
		SUIT_TABLE sUIT_TABLE = null;
		if (m_selectedEquipTable.n_PARTS == 1)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_1 == m_selectedEquipTable.n_ID).FirstOrDefault();
		}
		else if (m_selectedEquipTable.n_PARTS == 2)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_2 == m_selectedEquipTable.n_ID).FirstOrDefault();
		}
		else if (m_selectedEquipTable.n_PARTS == 3)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_3 == m_selectedEquipTable.n_ID).FirstOrDefault();
		}
		else if (m_selectedEquipTable.n_PARTS == 4)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_4 == m_selectedEquipTable.n_ID).FirstOrDefault();
		}
		else if (m_selectedEquipTable.n_PARTS == 5)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_5 == m_selectedEquipTable.n_ID).FirstOrDefault();
		}
		else if (m_selectedEquipTable.n_PARTS == 6)
		{
			sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_6 == m_selectedEquipTable.n_ID).FirstOrDefault();
		}
		if (sUIT_TABLE != null)
		{
			NetEquipmentInfo value = null;
			int num = 0;
			for (int i = 1; i <= 6; i++)
			{
				if (m_dicNetEquipInfo.TryGetValue(i, out value))
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
			m_suitEffect4.text = " ";
			m_suitEffect5.text = " ";
		}
		else
		{
			m_suitEffectTitle.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SUIT_EFFECT"), 0, 6);
			m_suitEffect1.text = " ";
			m_suitEffect2.text = " ";
			m_suitEffect3.text = " ";
			m_suitEffect4.text = " ";
			m_suitEffect5.text = " ";
		}
	}

	private void DisplaySuitEffectOld()
	{
		List<SUIT_TABLE> list = null;
		if (m_selectedEquipTable.n_PARTS == 1)
		{
			list = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_1 == m_selectedEquipTable.n_ID).ToList();
		}
		else if (m_selectedEquipTable.n_PARTS == 2)
		{
			list = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_2 == m_selectedEquipTable.n_ID).ToList();
		}
		else if (m_selectedEquipTable.n_PARTS == 3)
		{
			list = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_3 == m_selectedEquipTable.n_ID).ToList();
		}
		else if (m_selectedEquipTable.n_PARTS == 4)
		{
			list = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_4 == m_selectedEquipTable.n_ID).ToList();
		}
		else if (m_selectedEquipTable.n_PARTS == 5)
		{
			list = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_5 == m_selectedEquipTable.n_ID).ToList();
		}
		else if (m_selectedEquipTable.n_PARTS == 6)
		{
			list = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_6 == m_selectedEquipTable.n_ID).ToList();
		}
		if (list.Count > 0)
		{
			SKILL_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(list[0].n_EFFECT_1, out value))
			{
				m_suitEffect1.text = string.Format("({0}){1}", list[0].n_SUIT_1, ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP));
			}
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(list[0].n_EFFECT_2, out value))
			{
				m_suitEffect2.text = string.Format("({0}){1}", list[0].n_SUIT_2, ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP));
			}
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(list[0].n_EFFECT_3, out value))
			{
				m_suitEffect3.text = string.Format("({0}){1}", list[0].n_SUIT_3, ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP));
			}
			m_suitEffect4.text = " ";
			m_suitEffect5.text = " ";
		}
	}

	public void SetButtonFunction(string name, Action callback, bool bInteractable = true)
	{
		if (!m_panelBtn)
		{
			return;
		}
		m_panelBtn.onClick.RemoveAllListeners();
		if (callback != null)
		{
			m_panelBtn.onClick.AddListener(delegate
			{
				callback();
			});
		}
		OrangeText componentInChildren = m_panelBtn.GetComponentInChildren<OrangeText>();
		m_panelBtn.interactable = bInteractable;
		componentInChildren.text = name;
		if (bInteractable)
		{
			componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 1f);
		}
		else
		{
			componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 0.5f);
		}
	}

	public void ShowDiff(NetEquipmentInfo equipInfo)
	{
		int num = m_netEquipSelected.DefParam - equipInfo.DefParam;
		int num2 = m_netEquipSelected.HpParam - equipInfo.HpParam;
		int num3 = m_netEquipSelected.LukParam - equipInfo.LukParam;
		m_defenseDiff.gameObject.SetActive(true);
		m_lifeDiff.gameObject.SetActive(true);
		m_luckDiff.gameObject.SetActive(true);
		m_defenseDiff.text = ((num >= 0) ? "+" : "") + num;
		m_lifeDiff.text = ((num2 >= 0) ? "+" : "") + num2;
		m_luckDiff.text = ((num3 >= 0) ? "+" : "") + num3;
		if (num < 0)
		{
			m_defenseDiff.color = new Color(1f, 0f, 0f);
		}
		if (num2 < 0)
		{
			m_lifeDiff.color = new Color(1f, 0f, 0f);
		}
		if (num3 < 0)
		{
			m_luckDiff.color = new Color(1f, 0f, 0f);
		}
	}
}
