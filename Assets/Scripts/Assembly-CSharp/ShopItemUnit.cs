using System;
using Coffee.UIExtensions;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ShopItemUnit : ScrollIndexCallback
{
	public const int SHOP_CONNECT_TYPE_ITEM = 1;

	public const int SHOP_CONNECT_TYPE_SERVICE = 6;

	[SerializeField]
	private ItemIconWithAmount itemIcon;

	[SerializeField]
	private Image itemIcon2;

	[SerializeField]
	private OrangeText textItemName;

	[SerializeField]
	private Image imgCoin;

	[SerializeField]
	private OrangeText textCoin;

	[SerializeField]
	private GameObject groupTag;

	[SerializeField]
	private Sprite[] sprTags;

	[SerializeField]
	private Color[] outlineColorTags;

	[SerializeField]
	private Image imgTag;

	[SerializeField]
	private OrangeText textTag;

	[SerializeField]
	private UIShadow outlineTag;

	[SerializeField]
	private GameObject groupTime;

	[SerializeField]
	private OrangeText textTime;

	[SerializeField]
	private GameObject groupDaily;

	[SerializeField]
	private OrangeText dailyText;

	[SerializeField]
	private GameObject groupSoldOut;

	[SerializeField]
	private OrangeText textSoldOut;

	[SerializeField]
	private GameObject BgDiscount;

	[SerializeField]
	private GameObject Bg2Discount;

	[SerializeField]
	private GameObject objDisCountLine;

	[SerializeField]
	private Color[] colorTextDiscount;

	[SerializeField]
	private OrangeText textDiscount;

	[SerializeField]
	private Image imgPiece;

	[SerializeField]
	private Image imgCardType;

	[SerializeField]
	protected ShopTopUI parent;

	[HideInInspector]
	public int NowIdx = -1;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickItem;

	private string[] shopTagKeys = new string[2] { "SHOP_TAG_RECOMMEND", "SHOP_TAG_NEW" };

	private bool isIAP;

	private bool canBuy;

	private bool isDateInfinity;

	private bool isSoldOut;

	private int buyAmountMax;

	private decimal cost;

	private DateTime date;

	private SHOP_TABLE shopTable;

	private OrangeProduct product;

	private ITEM_TABLE itemTable;

	private SERVICE_TABLE serviceTable;

	private NetShopRecord netShopRecord;

	private string productTip = string.Empty;

	private ITEM_TABLE costItem;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		canBuy = false;
		if (NowIdx >= parent.ListShopItemNow.Count)
		{
			base.gameObject.SetActive(false);
			return;
		}
		shopTable = parent.ListShopItemNow[NowIdx];
		isIAP = shopTable.n_COIN_ID == 0;
		if (isIAP)
		{
			product = null;
			if (!MonoBehaviourSingleton<OrangeIAP>.Instance.DictProduct.TryGetValue(shopTable.s_PRODUCT_ID, out product))
			{
				base.gameObject.SetActive(false);
				return;
			}
		}
		ShopInfo value = null;
		ManagedSingleton<PlayerNetManager>.Instance.dicShop.TryGetValue(shopTable.n_ID, out value);
		netShopRecord = ((value == null || value.netShopRecord == null) ? null : value.netShopRecord);
		if (ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(shopTable.s_END_TIME))
		{
			isDateInfinity = true;
		}
		else
		{
			isDateInfinity = false;
			date = ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(shopTable.s_END_TIME);
		}
		if (!SetItem() || !SetCost() || !SetTag() || !SetTime() || !SetLimit())
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			canBuy = !isSoldOut;
		}
	}

	private bool SetItem()
	{
		bool flag = false;
		itemTable = null;
		imgPiece.color = Color.clear;
		itemIcon.Clear();
		itemIcon2.color = Color.clear;
		if (shopTable.n_PRODUCT_TYPE == 1)
		{
			flag = ManagedSingleton<OrangeTableHelper>.Instance.GetItem(shopTable.n_PRODUCT_ID, out itemTable);
		}
		else if (shopTable.n_PRODUCT_TYPE == 6)
		{
			flag = ManagedSingleton<OrangeTableHelper>.Instance.GetService(shopTable.n_PRODUCT_ID, out serviceTable);
		}
		if (flag)
		{
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(shopTable.s_ICON))
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_shop, shopTable.s_ICON, delegate(Sprite spr)
				{
					itemIcon2.sprite = spr;
					itemIcon2.color = Color.white;
				});
			}
			else if (itemTable != null)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)itemTable.f_VALUE_Y, out value))
				{
					string s_ICON = value.s_ICON;
					string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
					itemIcon.Setup(NowIdx, p_bundleName, s_ICON);
					itemIcon.SetRare(value.n_RARITY);
					string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(value.n_TYPE);
					imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
					imgCardType.color = Color.white;
				}
				else
				{
					itemIcon.Setup(NowIdx, AssetBundleScriptableObject.Instance.GetIconItem(itemTable.s_ICON), itemTable.s_ICON);
					itemIcon.SetRare(itemTable.n_RARE);
					imgCardType.color = Color.clear;
				}
				itemIcon.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
				itemIcon.SetAmount(shopTable.n_PRODUCT_MOUNT);
				imgPiece.color = ((itemTable.n_TYPE == 4) ? Color.white : Color.clear);
			}
			SetProductNameAndTip();
		}
		return flag;
	}

	private void SetProductNameAndTip()
	{
		string text = string.Empty;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(shopTable.w_PRODUCT_NAME))
		{
			text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(shopTable.w_PRODUCT_NAME);
		}
		else if (itemTable != null)
		{
			text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemTable.w_NAME);
		}
		string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(text, textItemName).Split('\n');
		if (array.Length > 1)
		{
			string text2 = array[0];
			textItemName.text = text2.Substring(0, text2.Length - 3) + "...";
		}
		else
		{
			textItemName.text = text;
		}
		productTip = string.Empty;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(shopTable.w_PRODUCT_TIP))
		{
			productTip = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(shopTable.w_PRODUCT_TIP);
		}
		else if (itemTable != null)
		{
			productTip = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemTable.w_TIP);
		}
	}

	private bool SetCost()
	{
		costItem = null;
		if (isIAP || ManagedSingleton<OrangeTableHelper>.Instance.GetItem(shopTable.n_COIN_ID, out costItem))
		{
			if (!isIAP)
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(costItem.s_ICON), costItem.s_ICON, delegate(Sprite obj)
				{
					imgCoin.sprite = obj;
					imgCoin.color = Color.white;
				});
				cost = shopTable.n_COIN_MOUNT;
				textCoin.text = "x" + cost;
			}
			else
			{
				imgCoin.color = Color.clear;
				cost = product.LocalizedPrice;
				textCoin.text = product.LocalizedPriceString;
			}
			if (shopTable.n_DISCOUNT != 0)
			{
				textCoin.color = colorTextDiscount[1];
				BgDiscount.SetActive(true);
				Bg2Discount.SetActive(true);
				objDisCountLine.SetActive(true);
				if (!isIAP)
				{
					cost = shopTable.n_DISCOUNT;
					textDiscount.text = (isIAP ? cost.ToString() : ("x" + cost));
				}
			}
			else
			{
				textCoin.color = colorTextDiscount[0];
				BgDiscount.SetActive(false);
				Bg2Discount.SetActive(false);
				objDisCountLine.SetActive(false);
				textDiscount.text = string.Empty;
			}
			return true;
		}
		return false;
	}

	private bool SetTag()
	{
		int n_TAG = shopTable.n_TAG;
		switch (n_TAG)
		{
		case 0:
			groupTag.SetActive(false);
			return true;
		case 1:
		case 2:
		{
			groupTag.SetActive(true);
			int num = n_TAG - 1;
			imgTag.sprite = sprTags[num];
			textTag.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(shopTagKeys[num]);
			outlineTag.effectColor = outlineColorTags[num];
			return true;
		}
		default:
			return false;
		}
	}

	private bool SetTime()
	{
		if (isDateInfinity)
		{
			groupTime.SetActive(false);
			return true;
		}
		groupTime.SetActive(true);
		bool remain = true;
		textTime.text = OrangeGameUtility.GetRemainTimeText(CapUtility.DateToUnixTime(date), parent.TimeNow, out remain);
		return remain;
	}

	private bool SetLimit()
	{
		isSoldOut = false;
		int n_LIMIT = shopTable.n_LIMIT;
		if (isIAP)
		{
			if (n_LIMIT == 0)
			{
				groupDaily.SetActive(false);
			}
			switch (shopTable.n_PRODUCT_TYPE)
			{
			case 1:
				SetIsSoldOut(n_LIMIT);
				SetSoldOutObj(isSoldOut, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SHOP_SOLDOUT"));
				break;
			case 6:
				SetIsSoldOut(n_LIMIT);
				if (serviceTable != null)
				{
					string p_remainTimeText = string.Empty;
					isSoldOut = false;
					bool serviceRemainTime = ManagedSingleton<ServiceHelper>.Instance.GetServiceRemainTime(serviceTable.n_ID, out p_remainTimeText);
					SetSoldOutObj(serviceRemainTime, p_remainTimeText);
				}
				else
				{
					SetSoldOutObj(false);
				}
				break;
			default:
				SetSoldOutObj(isSoldOut);
				break;
			}
		}
		else
		{
			SetIsSoldOut(n_LIMIT);
			SetSoldOutObj(isSoldOut, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SHOP_SOLDOUT"));
		}
		return true;
	}

	private void SetIsSoldOut(int limit)
	{
		isSoldOut = false;
		if (limit == 0)
		{
			buyAmountMax = OrangeConst.SHOP_MAX_BUY;
			groupDaily.SetActive(false);
		}
		else if (netShopRecord != null)
		{
			if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(netShopRecord.LastShopTime, (ResetRule)shopTable.n_RESET_RULE))
			{
				netShopRecord.Count = 0;
			}
			if (netShopRecord.Count >= limit)
			{
				isSoldOut = true;
				return;
			}
			groupDaily.SetActive(true);
			buyAmountMax = limit - netShopRecord.Count;
			dailyText.text = string.Format(GetRuleL10nFormat(shopTable.n_RESET_RULE), buyAmountMax, limit);
		}
		else
		{
			groupDaily.SetActive(true);
			buyAmountMax = limit;
			dailyText.text = string.Format(GetRuleL10nFormat(shopTable.n_RESET_RULE), buyAmountMax, limit);
		}
	}

	private void SetSoldOutObj(bool active, string soldOutStr = "")
	{
		if (active)
		{
			groupTag.SetActive(false);
			groupDaily.SetActive(false);
			groupTime.SetActive(false);
			BgDiscount.SetActive(false);
			Bg2Discount.SetActive(false);
			textSoldOut.text = soldOutStr;
			buyAmountMax = 0;
		}
		groupSoldOut.SetActive(active);
	}

	private string GetRuleL10nFormat(int rule)
	{
		string empty = string.Empty;
		switch ((ResetRule)(short)rule)
		{
		default:
			empty = "RESET_TYPE_0";
			break;
		case ResetRule.DailyReset:
			empty = "RESET_TYPE_1";
			break;
		case ResetRule.WeeklyReset:
			empty = "RESET_TYPE_2";
			break;
		case ResetRule.MonthlyReset:
			empty = "RESET_TYPE_3";
			break;
		}
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(empty);
	}

	public void OnClickBuy()
	{
		if (canBuy)
		{
			if (isIAP)
			{
				LoadShopBuyIAP();
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickItem);
			LoadShopBuyUI();
		}
	}

	private void LoadShopBuyUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopBuy", delegate(ShopBuyUI ui)
		{
			int num = 0;
			num = ((!IsJewel(costItem.n_ID)) ? ManagedSingleton<PlayerHelper>.Instance.GetItemValue(costItem.n_ID) : ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel());
			ui.IsInfinity = shopTable.n_LIMIT == 0;
			ui.CostAmount = decimal.ToInt32(cost);
			ui.CostAmountMax = num;
			ui.SprIconCost = imgCoin.sprite;
			ui.Setup(shopTable, buyAmountMax, itemTable, netShopRecord);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		});
	}

	private void LoadShopBuyIAP()
	{
		if (shopTable.n_PRODUCT_TYPE == 1 && shopTable.n_AUTO_OPEN == 0)
		{
			MonoBehaviourSingleton<OrangeIAP>.Instance.DoPurchase(shopTable, product);
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopBuyIAP", delegate(ShopBuyIAPUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.SprIconCost = imgCoin.sprite;
			ui.SprIconProduct = itemIcon2.sprite;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			if (shopTable.n_PRODUCT_TYPE == 6)
			{
				ui.Setup(shopTable, product, serviceTable);
			}
			else
			{
				ui.Setup(shopTable, itemTable, product);
			}
		});
	}

	private bool IsJewel(int p_id)
	{
		if (p_id != OrangeConst.ITEMID_FREE_JEWEL)
		{
			return p_id == OrangeConst.ITEMID_FREE_JEWEL;
		}
		return true;
	}
}
