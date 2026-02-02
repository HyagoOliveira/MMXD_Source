using System;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ItemBoxEquipUpgrade : OrangeUIBase
{
	[SerializeField]
	private EquipIcon m_equipIcon;

	[SerializeField]
	private Text m_equipName;

	[SerializeField]
	private Text m_powerAttrBefore;

	[SerializeField]
	private Text m_powerAttrAfter;

	[SerializeField]
	private Text m_defenseAttrBefore;

	[SerializeField]
	private Text m_defenseAttrAfter;

	[SerializeField]
	private Text m_lifeAttrBefore;

	[SerializeField]
	private Text m_lifeAttrAfter;

	[SerializeField]
	private Text m_material1Needed;

	[SerializeField]
	private Text m_material2Needed;

	[SerializeField]
	private Text m_material1Owned;

	[SerializeField]
	private Text m_material2Owned;

	[SerializeField]
	private Text m_currentLevel;

	[SerializeField]
	private Text m_nextLevel;

	[SerializeField]
	private Image m_material1NeededIcon;

	[SerializeField]
	private Image m_material2NeededIcon;

	[SerializeField]
	private Image m_material1OwnedIcon;

	[SerializeField]
	private Image m_material2OwnedIcon;

	[SerializeField]
	private Button m_upgradeBtn;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_upgradeSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickUpgradeSE;

	private EquipEnhanceInfo m_equipEnhanceInfo;

	private NetEquipmentInfo m_netEquipInfo;

	private EQUIP_TABLE m_equipTable;

	private UpgradeEffect m_upgradeEffect;

	private int m_playerLvl;

	private int m_equipLvl;

	private void Start()
	{
	}

	public void Setup(NetEquipmentInfo equipInfo)
	{
		m_netEquipInfo = equipInfo;
		ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(m_netEquipInfo.EquipItemID, out m_equipTable);
		ManagedSingleton<PlayerNetManager>.Instance.dicEquipEnhance.TryGetValue((EquipPartType)m_equipTable.n_PARTS, out m_equipEnhanceInfo);
		m_playerLvl = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		m_equipLvl = m_equipTable.n_LV;
		SetEquipIcon();
		SetupAttributes();
		SetupUpgradeRequirements();
	}

	public override void OnClickCloseBtn()
	{
		ItemBoxUI componentInChildren = base.transform.parent.GetComponentInChildren<ItemBoxUI>();
		if ((bool)componentInChildren)
		{
			componentInChildren.EnableItemPanel(true);
			componentInChildren.EnableEquipSelectionFrame(false);
		}
		base.OnClickCloseBtn();
	}

	public void OnClickUpgradeBtn()
	{
		if (IsEquipMaxLvlReached())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_EQUIP_LV"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
				{
				});
			});
		}
		else if (IsEquipLvlExceedPlayerLvl())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_EQUIP_RANK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
				{
				});
			});
		}
		else if (CheckAndBuyMaterial())
		{
			ManagedSingleton<PlayerNetManager>.Instance.PowerUpEquipmentReq(m_netEquipInfo.EquipmentID, 1, delegate
			{
				SetupAttributes();
				SetupUpgradeRequirements();
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_PLAYER_EQUIPMENT);
				PlayUpgradeEffect();
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickUpgradeSE);
		}
	}

	private bool CheckAndBuyMaterial()
	{
		int enhanceLv = m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv;
		EXP_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(enhanceLv, out value))
		{
			int n_EQUIPUP_MONEY = value.n_EQUIPUP_MONEY;
			int materialNeeded = value.n_EQUIPUP_MATERIAL;
			int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
			int num = 0;
			ItemInfo value2;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(OrangeConst.ITEMID_EQUIP_POWERUP, out value2))
			{
				num = value2.netItemInfo.Stack;
			}
			if (num < materialNeeded)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ITEM_TABLE value3 = null;
					ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(OrangeConst.ITEMID_EQUIP_POWERUP, out value3);
					ui.Setup(value3, null, materialNeeded);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
					{
						SetupUpgradeRequirements();
					});
				});
				return false;
			}
			if (zenny < n_EQUIPUP_MONEY)
			{
				return false;
			}
		}
		return true;
	}

	private void PlayUpgradeEffect()
	{
		if (m_upgradeEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgradeeffect", "UpgradeEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_upgradeEffect = gameObject.GetComponent<UpgradeEffect>();
				m_upgradeEffect.Play(m_equipIcon.transform.position);
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_upgradeSE);
			});
		}
		else
		{
			m_upgradeEffect.Play(m_equipIcon.transform.position);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_upgradeSE);
		}
	}

	private bool IsEquipMaxLvlReached()
	{
		if (m_equipEnhanceInfo == null)
		{
			return false;
		}
		return m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv >= m_equipLvl + 10;
	}

	private bool IsEquipLvlExceedPlayerLvl()
	{
		if (m_equipEnhanceInfo == null)
		{
			return false;
		}
		return m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv >= m_playerLvl;
	}

	private bool IsUpgradeMaterialSufficient()
	{
		int enhanceLv = m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv;
		EXP_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(enhanceLv, out value))
		{
			int n_EQUIPUP_MONEY = value.n_EQUIPUP_MONEY;
			int n_EQUIPUP_MATERIAL = value.n_EQUIPUP_MATERIAL;
			int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
			int num = 0;
			ItemInfo value2;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(OrangeConst.ITEMID_EQUIP_POWERUP, out value2))
			{
				num = value2.netItemInfo.Stack;
			}
			if (num >= n_EQUIPUP_MATERIAL && zenny >= n_EQUIPUP_MONEY)
			{
				return true;
			}
		}
		return false;
	}

	private void SetEquipIcon()
	{
		int[] equipRank = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(m_netEquipInfo);
		m_equipIcon.SetStarAndLv(equipRank[3], m_equipTable.n_LV);
		m_equipIcon.SetRare(m_equipTable.n_RARE);
		m_equipIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconEquip, m_equipTable.s_ICON);
	}

	private void SetupAttributes()
	{
		if (m_equipEnhanceInfo != null)
		{
			int enhanceLv = m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv;
			m_currentLevel.text = enhanceLv.ToString();
			m_nextLevel.text = (enhanceLv + 1).ToString();
			m_powerAttrBefore.text = "---";
			m_defenseAttrBefore.text = "---";
			m_lifeAttrBefore.text = "---";
			m_powerAttrAfter.text = "---";
			m_defenseAttrAfter.text = "---";
			m_lifeAttrAfter.text = "---";
			EXP_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(enhanceLv, out value))
			{
				int n_EQUIPUP_DEF = value.n_EQUIPUP_DEF;
				int n_EQUIPUP_HP = value.n_EQUIPUP_HP;
				int num = OrangeConst.BP_DEF * n_EQUIPUP_DEF + OrangeConst.BP_HP * n_EQUIPUP_HP;
				m_powerAttrBefore.text = num.ToString();
				m_defenseAttrBefore.text = n_EQUIPUP_DEF.ToString();
				m_lifeAttrBefore.text = n_EQUIPUP_HP.ToString();
			}
			if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(enhanceLv + 1, out value))
			{
				int n_EQUIPUP_DEF2 = value.n_EQUIPUP_DEF;
				int n_EQUIPUP_HP2 = value.n_EQUIPUP_HP;
				int num2 = OrangeConst.BP_DEF * n_EQUIPUP_DEF2 + OrangeConst.BP_HP * n_EQUIPUP_HP2;
				m_powerAttrAfter.text = num2.ToString();
				m_defenseAttrAfter.text = n_EQUIPUP_DEF2.ToString();
				m_lifeAttrAfter.text = n_EQUIPUP_HP2.ToString();
			}
		}
		m_equipName.text = ManagedSingleton<OrangeTextDataManager>.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(m_equipTable.w_NAME);
	}

	private void SetupUpgradeRequirements()
	{
		if (m_equipEnhanceInfo == null)
		{
			return;
		}
		int enhanceLv = m_equipEnhanceInfo.netPlayerEquipInfo.EnhanceLv;
		EXP_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(enhanceLv, out value))
		{
			m_material1Needed.text = value.n_EQUIPUP_MATERIAL.ToString();
			m_material2Needed.text = value.n_EQUIPUP_MONEY.ToString();
			ItemInfo value2;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(OrangeConst.ITEMID_EQUIP_POWERUP, out value2))
			{
				m_material1Owned.text = value2.netItemInfo.Stack.ToString();
			}
			else
			{
				m_material1Owned.text = "0";
			}
			m_material2Owned.text = ManagedSingleton<PlayerHelper>.Instance.GetZenny().ToString();
			m_material1Owned.color = m_material1Needed.color;
			m_material2Owned.color = m_material1Needed.color;
			if (value2.netItemInfo.Stack < value.n_EQUIPUP_MATERIAL)
			{
				m_material1Owned.color = Color.red;
			}
			if (ManagedSingleton<PlayerHelper>.Instance.GetZenny() < value.n_EQUIPUP_MONEY)
			{
				m_material2Owned.color = Color.red;
			}
		}
		if (!IsEquipMaxLvlReached() && !IsEquipLvlExceedPlayerLvl() && IsUpgradeMaterialSufficient())
		{
			m_upgradeBtn.GetComponent<Image>().color = Color.white;
		}
		else
		{
			m_upgradeBtn.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.6f);
		}
	}
}
