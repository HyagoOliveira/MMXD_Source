using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ShopBuyIAPUI : OrangeUIBase
{
	[SerializeField]
	private Image iconProduct;

	[SerializeField]
	private Image iconCost;

	[SerializeField]
	private Text textCost;

	[SerializeField]
	private OrangeText textItemName;

	[SerializeField]
	private OrangeText textItemTip;

	[SerializeField]
	private Button btnBuy;

	[SerializeField]
	private HorizontalLayoutGroup giftUnitParent;

	[SerializeField]
	private ShopBuyIAPUIUnit iapUnit;

	private SHOP_TABLE shopData;

	private ITEM_TABLE itemData;

	private SERVICE_TABLE serviceData;

	private OrangeProduct product;

	private List<GACHA_TABLE> listGift = new List<GACHA_TABLE>();

	public Sprite SprIconProduct { get; set; }

	public Sprite SprIconCost { get; set; }

	protected override void Awake()
	{
		base.Awake();
		btnBuy.interactable = false;
	}

	public void Setup(SHOP_TABLE p_shopData, OrangeProduct p_product, SERVICE_TABLE p_serviceData)
	{
		shopData = p_shopData;
		product = p_product;
		serviceData = p_serviceData;
		textItemName.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(shopData.w_PRODUCT_NAME);
		textItemTip.alignByGeometry = false;
		textItemTip.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(shopData.w_PRODUCT_TIP);
		SetIconAndCost();
		SetGiftDataByServiceTable();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void SetGiftDataByServiceTable()
	{
		Transform parent = giftUnitParent.transform;
		List<SERVICE_TABLE> serviceListByGroup = ManagedSingleton<OrangeTableHelper>.Instance.GetServiceListByGroup(serviceData.n_GROUP);
		string extraMsg = string.Empty;
		ITEM_TABLE itemTable = null;
		for (int i = 0; i < serviceListByGroup.Count; i++)
		{
			SERVICE_TABLE service = serviceListByGroup[i];
			GACHA_TABLE gACHA_TABLE = new GACHA_TABLE();
			gACHA_TABLE.n_REWARD_ID = service.n_TYPE_2;
			int n_TYPE = service.n_TYPE;
			if (n_TYPE != 0)
			{
				if (n_TYPE != 1)
				{
					continue;
				}
				extraMsg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REWARD_TAG_1");
			}
			if (GetItemTable(ref service, out itemTable))
			{
				string s_ICON = itemTable.s_ICON;
				int n_RARE = itemTable.n_RARE;
				listGift.Add(gACHA_TABLE);
				ShopBuyIAPUIUnit shopBuyIAPUIUnit = Object.Instantiate(iapUnit, parent);
				shopBuyIAPUIUnit.SetRare(n_RARE);
				shopBuyIAPUIUnit.Setup(i, AssetBundleScriptableObject.Instance.GetIconItem(s_ICON), s_ICON, ShowItemInfo);
				shopBuyIAPUIUnit.SetAmount(service.n_TYPE_3);
				shopBuyIAPUIUnit.SetExtraMsg(extraMsg);
				shopBuyIAPUIUnit.SetPiece((ItemType)itemTable.n_TYPE);
				shopBuyIAPUIUnit.SetRareItemEffect(itemTable.n_RARE == 5);
			}
		}
	}

	private bool GetItemTable(ref SERVICE_TABLE service, out ITEM_TABLE itemTable)
	{
		itemTable = null;
		if (service.n_TYPE_1 == 1)
		{
			return ManagedSingleton<OrangeTableHelper>.Instance.GetItem(service.n_TYPE_2, out itemTable);
		}
		return false;
	}

	public void Setup(SHOP_TABLE p_shopData, ITEM_TABLE p_itemData, OrangeProduct p_product)
	{
		shopData = p_shopData;
		itemData = p_itemData;
		product = p_product;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(shopData.w_PRODUCT_NAME))
		{
			textItemName.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(shopData.w_PRODUCT_NAME);
		}
		else
		{
			textItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemData.w_NAME);
		}
		textItemTip.alignByGeometry = false;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(shopData.w_PRODUCT_TIP))
		{
			textItemTip.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(shopData.w_PRODUCT_TIP);
		}
		else
		{
			textItemTip.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemData.w_TIP);
		}
		SetIconAndCost();
		SetGiftDataByItemTable();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void SetGiftDataByItemTable()
	{
		if (shopData.n_AUTO_OPEN == 0)
		{
			return;
		}
		listGift = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup((int)itemData.f_VALUE_X);
		Transform parent = giftUnitParent.transform;
		GACHA_TABLE gACHA_TABLE = null;
		for (int i = 0; i < listGift.Count; i++)
		{
			ShopBuyIAPUIUnit shopBuyIAPUIUnit = Object.Instantiate(iapUnit, parent);
			gACHA_TABLE = listGift[i];
			NetRewardInfo netGachaRewardInfo = new NetRewardInfo
			{
				RewardType = (sbyte)gACHA_TABLE.n_REWARD_TYPE,
				RewardID = gACHA_TABLE.n_REWARD_ID,
				Amount = gACHA_TABLE.n_AMOUNT_MAX
			};
			string bundlePath = string.Empty;
			string assetPath = string.Empty;
			int rare = 0;
			int[] rewardSpritePath = MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
			if (rewardSpritePath[1] == 0)
			{
				rewardSpritePath[1] = 1;
			}
			shopBuyIAPUIUnit.SetRare(rare);
			if (gACHA_TABLE.n_REWARD_TYPE == 1)
			{
				shopBuyIAPUIUnit.Setup(i, bundlePath, assetPath, ShowItemInfo);
			}
			else
			{
				shopBuyIAPUIUnit.Setup(i, bundlePath, assetPath);
			}
			shopBuyIAPUIUnit.SetAmount(gACHA_TABLE.n_AMOUNT_MIN);
			shopBuyIAPUIUnit.SetPiece((ItemType)rewardSpritePath[1]);
			shopBuyIAPUIUnit.SetRareItemEffect(rare == 5);
		}
	}

	private void SetIconAndCost()
	{
		iconCost.sprite = SprIconCost;
		iconCost.color = ((SprIconCost == null) ? Color.clear : Color.white);
		iconProduct.sprite = SprIconProduct;
		iconProduct.color = ((SprIconProduct == null) ? Color.clear : Color.white);
		textCost.text = product.LocalizedPriceString;
		if (serviceData != null)
		{
			string p_remainTimeText = string.Empty;
			bool serviceRemainTime = ManagedSingleton<ServiceHelper>.Instance.GetServiceRemainTime(serviceData.n_ID, out p_remainTimeText);
			btnBuy.interactable = !serviceRemainTime;
		}
		else
		{
			btnBuy.interactable = true;
		}
	}

	private void ShowItemInfo(int p_idx)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(listGift[p_idx].n_REWARD_ID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			});
		}
	}

	public void OnClickBuy()
	{
		MonoBehaviourSingleton<OrangeIAP>.Instance.DoPurchase(shopData, product);
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}
}
