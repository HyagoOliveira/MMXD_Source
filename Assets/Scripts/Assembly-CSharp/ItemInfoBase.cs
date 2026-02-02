#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoBase : OrangeUIBase
{
	[SerializeField]
	protected Button btnSell;

	[SerializeField]
	protected ItemIconWithAmount itemIcon;

	[SerializeField]
	protected OrangeText textItemName;

	[SerializeField]
	protected OrangeText textItemTip;

	[SerializeField]
	protected Image imgCardType;

	protected ITEM_TABLE item;

	protected NetItemInfo netItem;

	public bool NeedSE = true;

	public virtual void Setup(ITEM_TABLE p_item, NetItemInfo p_netItem = null, int p_requestCount = 0)
	{
		btnSell.onClick.RemoveAllListeners();
		item = p_item;
		netItem = p_netItem;
		SetItemUiInfo(p_requestCount);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (NeedSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public virtual void Setup(EQUIP_TABLE p_equip)
	{
		btnSell.interactable = false;
		textItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(p_equip.w_NAME);
		textItemTip.alignByGeometry = false;
		textItemTip.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_COMMON_TIP");
		itemIcon.ClearAmount();
		NetRewardInfo netGachaRewardInfo = new NetRewardInfo
		{
			RewardType = 4,
			RewardID = p_equip.n_ID,
			Amount = 1
		};
		string bundlePath = string.Empty;
		string assetPath = string.Empty;
		int rare = 0;
		MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
		itemIcon.Setup(0, bundlePath, assetPath);
		itemIcon.SetRare(rare);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (NeedSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
	}

	public virtual void Setup(CHARACTER_TABLE p_character)
	{
		btnSell.interactable = false;
		textItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(p_character.w_NAME);
		textItemTip.alignByGeometry = false;
		textItemTip.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(p_character.w_TIP);
		itemIcon.ClearAmount();
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_character.n_UNLOCK_ID, out item))
		{
			SetItemIcon();
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (NeedSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
	}

	public virtual void Setup(WEAPON_TABLE p_weaepon)
	{
		btnSell.interactable = false;
		textItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(p_weaepon.w_NAME);
		textItemTip.alignByGeometry = false;
		textItemTip.text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(p_weaepon.w_TIP);
		itemIcon.ClearAmount();
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_weaepon.n_UNLOCK_ID, out item))
		{
			SetItemIcon();
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (NeedSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
	}

	public virtual void Setup(CARD_TABLE p_card, ITEM_TABLE p_item)
	{
		btnSell.interactable = false;
		textItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(p_item.w_NAME);
		textItemTip.alignByGeometry = false;
		textItemTip.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(p_item.w_TIP);
		itemIcon.ClearAmount();
		string s_ICON = p_card.s_ICON;
		string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, p_card.n_PATCH);
		itemIcon.Setup(p_card.n_ID, p_bundleName, s_ICON, OnClickCardInfo);
		itemIcon.SetRare(p_card.n_RARITY);
		string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(p_card.n_TYPE);
		imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
		imgCardType.color = Color.white;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (NeedSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
	}

	protected virtual void SetItemUiInfo(int p_requestCount = 0)
	{
		if (netItem != null)
		{
			if (netItem.Stack > 0 && item.n_SELL_ID != -1)
			{
				itemIcon.SetAmount(netItem.Stack);
				btnSell.onClick.AddListener(OnClickSold);
			}
			else
			{
				itemIcon.ClearAmount();
				btnSell.interactable = false;
			}
		}
		else
		{
			itemIcon.ClearAmount();
			btnSell.interactable = false;
		}
		SetItemIcon();
		textItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(item.w_NAME);
		textItemTip.alignByGeometry = false;
		textItemTip.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(item.w_TIP);
	}

	private void SetItemIcon()
	{
		itemIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON);
		itemIcon.SetRare(item.n_RARE);
	}

	protected virtual void OnClickSold()
	{
		Debug.LogWarning("Should override function:[OnClickSold].");
	}

	private void OnClickCardInfo(int p_idx)
	{
		CARD_TABLE card = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(p_idx, out card))
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardInfo", delegate(CardInfoUI ui)
			{
				ui.bOnlyShowBasic = true;
				ui.bNeedInitList = true;
				ui.nTargetCardSeqID = 0;
				ui.nTargetCardID = card.n_ID;
			});
		}
	}
}
