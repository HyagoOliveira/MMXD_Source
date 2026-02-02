using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ItemBoxEquipUpgrade2 : OrangeUIBase
{
	private class QuickUpgradeItems
	{
		public int totalMoney;

		public int totalSP;
	}

	[SerializeField]
	private EquipIcon[] m_equipPartIcons;

	[SerializeField]
	private EquipIcon m_equipSelectedIcon;

	[SerializeField]
	private Text m_equipName;

	[SerializeField]
	private Text m_targetLvl;

	[SerializeField]
	private Text m_material1Needed;

	[SerializeField]
	private Text m_material2Needed;

	[SerializeField]
	private Text m_material1Owned;

	[SerializeField]
	private Text m_material2Owned;

	[SerializeField]
	private Text m_powerBefore;

	[SerializeField]
	private Text m_powerAfter;

	[SerializeField]
	private Text m_defenseBefore;

	[SerializeField]
	private Text m_defenseAfter;

	[SerializeField]
	private Text m_lifeBefore;

	[SerializeField]
	private Text m_lifeAfter;

	[SerializeField]
	private Button m_upgradeBtn;

	[SerializeField]
	private Transform m_selectionCheck;

	[SerializeField]
	private Slider m_lvlSlider;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_upgradeSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickUpgradeSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickEquipSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_ChangeLevelSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_CloseWindowSE;

	private bool b_muteSE;

	private int m_selectionIdx;

	private Dictionary<int, NetEquipmentInfo> m_dicNetEquipInfo;

	private NetEquipmentInfo m_netEquipInfo;

	private EquipEnhanceInfo m_equipEnhanceInfo;

	private EQUIP_TABLE m_equipTable;

	private int m_equipBaseLvl;

	private int m_equipStartEnhanceLvl;

	private int m_equipMaxEnhanceLvl;

	private UpgradeEffect m_upgradeEffect;

	private bool m_bEquipmentLvExceed;

	private bool m_bPlayerRankExceed;

	private Dictionary<int, QuickUpgradeItems> m_quickUpgradeTable = new Dictionary<int, QuickUpgradeItems>();

	private int old_targetLevel;

	public void Setup(int selectionIdx = 0)
	{
		base.CloseSE = m_CloseWindowSE;
		m_selectionIdx = selectionIdx;
		b_muteSE = true;
		UpdateAllIcons();
		b_muteSE = false;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void UpdateIcon(EquipIcon icon, int iconIndex)
	{
		NetEquipmentInfo value = null;
		EQUIP_TABLE equip = null;
		int[] array = null;
		if (m_dicNetEquipInfo.TryGetValue(iconIndex + 1, out value) && ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(value.EquipItemID, out equip))
		{
			EquipEnhanceInfo value2 = null;
			ManagedSingleton<PlayerNetManager>.Instance.dicEquipEnhance.TryGetValue((EquipPartType)equip.n_PARTS, out value2);
			array = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(value);
			if (value2 != null)
			{
				icon.SetStarAndLv(array[3], equip.n_LV, value2.netPlayerEquipInfo.EnhanceLv);
			}
			else
			{
				icon.SetStarAndLv(array[3], equip.n_LV);
			}
			icon.SetRare(equip.n_RARE);
			icon.Setup(iconIndex, AssetBundleScriptableObject.Instance.m_iconEquip, equip.s_ICON, OnClickEquip);
		}
		else
		{
			icon.Clear();
		}
	}

	private void UpdateAllIcons()
	{
		m_dicNetEquipInfo = ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip();
		for (int i = 0; i < m_equipPartIcons.Length; i++)
		{
			UpdateIcon(m_equipPartIcons[i], i);
		}
		OnClickEquip(m_selectionIdx);
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	public void OnClickEquip(int idx)
	{
		bool flag = true;
		if (m_selectionIdx == idx)
		{
			flag = false;
		}
		int num = idx + 1;
		m_dicNetEquipInfo.TryGetValue(num, out m_netEquipInfo);
		ManagedSingleton<PlayerNetManager>.Instance.dicEquipEnhance.TryGetValue((EquipPartType)num, out m_equipEnhanceInfo);
		ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(m_netEquipInfo.EquipItemID, out m_equipTable);
		m_equipBaseLvl = m_equipTable.n_LV;
		m_selectionIdx = idx;
		if (m_netEquipInfo != null)
		{
			UpdateUpgradeTable();
			m_selectionCheck.localPosition = m_equipPartIcons[idx].transform.localPosition;
			m_equipName.text = ManagedSingleton<OrangeTextDataManager>.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(m_equipTable.w_NAME);
			UpdateIcon(m_equipSelectedIcon, idx);
			UpdatePlayerItems();
			if (flag && !b_muteSE)
			{
				PlayUISE(m_clickEquipSE);
			}
			bool flag2 = b_muteSE;
			b_muteSE = true;
			m_lvlSlider.minValue = m_equipStartEnhanceLvl;
			m_lvlSlider.maxValue = m_equipMaxEnhanceLvl;
			m_lvlSlider.value = m_equipStartEnhanceLvl;
			OnSliderValueChange();
			b_muteSE = flag2;
		}
	}

	public void OnSliderValueChange()
	{
		int num = (int)m_lvlSlider.value;
		if (old_targetLevel != num && !b_muteSE)
		{
			PlayUISE(m_ChangeLevelSE);
		}
		m_targetLvl.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TARGET_LV"), num, m_equipMaxEnhanceLvl);
		QuickUpgradeItems value;
		if (m_quickUpgradeTable.TryGetValue(num, out value))
		{
			m_material1Needed.text = value.totalSP.ToString();
			m_material2Needed.text = value.totalMoney.ToString();
		}
		else
		{
			m_material1Needed.text = "0";
			m_material2Needed.text = "0";
		}
		old_targetLevel = num;
		UpdateAttributes();
		m_upgradeBtn.interactable = num != m_equipStartEnhanceLvl;
	}

	public void OnClickUpgradeMin()
	{
		m_lvlSlider.value = m_lvlSlider.minValue;
	}

	public void OnClickUpgradeMax()
	{
		m_lvlSlider.value = m_lvlSlider.maxValue;
	}

	public void OnClickUpgradeMinus()
	{
		m_lvlSlider.value--;
	}

	public void OnClickUpgradePlus()
	{
		if (CheckUpgradeLimitation((int)m_lvlSlider.value + 1))
		{
			m_lvlSlider.value++;
		}
	}

	public void OnClickUpgradeBtn()
	{
		if (m_equipEnhanceInfo != null)
		{
			int enhanceLv = m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv;
			ManagedSingleton<PlayerNetManager>.Instance.PowerUpEquipmentReq(m_netEquipInfo.EquipmentID, (short)((int)m_lvlSlider.value - enhanceLv), delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_PLAYER_EQUIPMENT);
				UpdateIcon(m_equipPartIcons[m_selectionIdx], m_selectionIdx);
				OnClickEquip(m_selectionIdx);
				PlayUpgradeEffect();
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickUpgradeSE);
		}
	}

	private bool CheckUpgradeLimitation(int targetLvl)
	{
		if (m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv > m_equipBaseLvl + 10 || targetLvl > m_equipBaseLvl + 10)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("RESTRICT_EQUIP_LV", 1f);
			return false;
		}
		if (m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv > ManagedSingleton<PlayerHelper>.Instance.GetLV() || targetLvl > ManagedSingleton<PlayerHelper>.Instance.GetLV())
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("RESTRICT_EQUIP_RANK", 1f);
			return false;
		}
		return true;
	}

	private void PlayUpgradeEffect()
	{
		if (m_upgradeEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgradeeffect", "UpgradeEffect", delegate(GameObject asset)
			{
				GameObject gameObject = Object.Instantiate(asset, base.transform);
				m_upgradeEffect = gameObject.GetComponent<UpgradeEffect>();
				m_upgradeEffect.Play(m_equipSelectedIcon.transform.position);
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_upgradeSE);
			});
		}
		else
		{
			m_upgradeEffect.Play(m_equipSelectedIcon.transform.position);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_upgradeSE);
		}
	}

	private void UpdateAttributes()
	{
		if (m_equipEnhanceInfo != null)
		{
			m_powerBefore.text = "---";
			m_defenseBefore.text = "---";
			m_lifeBefore.text = "---";
			m_powerAfter.text = "---";
			m_defenseAfter.text = "---";
			m_lifeAfter.text = "---";
			int enhanceLv = m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv;
			EXP_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(enhanceLv, out value))
			{
				int n_EQUIPUP_DEF = value.n_EQUIPUP_DEF;
				int n_EQUIPUP_HP = value.n_EQUIPUP_HP;
				int num = OrangeConst.BP_DEF * n_EQUIPUP_DEF + OrangeConst.BP_HP * n_EQUIPUP_HP;
				m_powerBefore.text = num.ToString();
				m_defenseBefore.text = n_EQUIPUP_DEF.ToString();
				m_lifeBefore.text = n_EQUIPUP_HP.ToString();
			}
			int key = (int)m_lvlSlider.value;
			if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(key, out value))
			{
				int n_EQUIPUP_DEF2 = value.n_EQUIPUP_DEF;
				int n_EQUIPUP_HP2 = value.n_EQUIPUP_HP;
				int num2 = OrangeConst.BP_DEF * n_EQUIPUP_DEF2 + OrangeConst.BP_HP * n_EQUIPUP_HP2;
				m_powerAfter.text = num2.ToString();
				m_defenseAfter.text = n_EQUIPUP_DEF2.ToString();
				m_lifeAfter.text = n_EQUIPUP_HP2.ToString();
			}
		}
	}

	private void UpdatePlayerItems()
	{
		m_material1Owned.text = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.ITEMID_EQUIP_POWERUP).ToString();
		m_material2Owned.text = ManagedSingleton<PlayerHelper>.Instance.GetZenny().ToString();
	}

	private void UpdateUpgradeTable()
	{
		if (m_equipEnhanceInfo == null)
		{
			return;
		}
		int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.ITEMID_EQUIP_POWERUP);
		int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
		int i = m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv;
		int num = 0;
		int num2 = 0;
		m_equipStartEnhanceLvl = (m_equipMaxEnhanceLvl = i);
		m_bEquipmentLvExceed = false;
		m_bPlayerRankExceed = false;
		m_quickUpgradeTable.Clear();
		EXP_TABLE value;
		for (; ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(i, out value); i++)
		{
			if (i > m_equipBaseLvl + 10)
			{
				m_equipMaxEnhanceLvl = i - 1;
				break;
			}
			if (i > ManagedSingleton<PlayerHelper>.Instance.GetLV() || num > zenny || num2 > itemValue)
			{
				break;
			}
			QuickUpgradeItems quickUpgradeItems = new QuickUpgradeItems();
			quickUpgradeItems.totalMoney = num;
			quickUpgradeItems.totalSP = num2;
			m_quickUpgradeTable.Add(i, quickUpgradeItems);
			m_equipMaxEnhanceLvl = i;
			num += value.n_EQUIPUP_MONEY;
			num2 += value.n_EQUIPUP_MATERIAL;
		}
		if (m_equipStartEnhanceLvl > m_equipMaxEnhanceLvl)
		{
			m_equipStartEnhanceLvl = m_equipMaxEnhanceLvl;
		}
	}
}
