using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class ShopBuyUI : OrangeUIBase
{
	[SerializeField]
	private RectTransform groupRt;

	[SerializeField]
	private ItemIconWithAmount itemIcon;

	[SerializeField]
	private OrangeText textItemName;

	[SerializeField]
	private OrangeText textItemTip;

	[SerializeField]
	private Button btnBuy;

	[SerializeField]
	private Text textBuy;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Text textCost;

	[SerializeField]
	private Image iconCost;

	[SerializeField]
	private GameObject extraGroup;

	[SerializeField]
	private HorizontalLayoutGroup giftUnitParent;

	[SerializeField]
	private ItemIconBase extraGiftUnit;

	[SerializeField]
	private Color[] textOutlineColor = new Color[2]
	{
		new Color(0.14509805f, 0.16862746f, 10f / 51f),
		Color.black
	};

	[SerializeField]
	private Color[] textColor = new Color[2]
	{
		Color.white,
		new Color(1f, 0.12156863f, 0.101960786f)
	};

	[SerializeField]
	private UIShadow outlineEft;

	[SerializeField]
	private Image imgPiece;

	[SerializeField]
	private Image imgCardType;

	[SerializeField]
	private Canvas canvasPieceInfoTop;

	[SerializeField]
	private Canvas canvasPieceInfoBottom;

	[SerializeField]
	private Image imgPieceFill;

	[SerializeField]
	private Text textPieceOwn;

	[SerializeField]
	private Text textPieceNext;

	[SerializeField]
	private Text textItemOwn;

	private Vector2[] sizeDelta = new Vector2[2]
	{
		new Vector2(1143f, 750f),
		new Vector2(1143f, 950f)
	};

	private SHOP_TABLE shopData;

	private ITEM_TABLE itemData;

	private NetShopRecord netShopRecord;

	private string amountFormat = "{0}/{1}";

	private string costValueFormat = "x {0}";

	private int amountMax;

	private int amountCache;

	private bool isBuying;

	private bool isSlider = true;

	private float sliderAddValue;

	private SystemSE BtnSE = SystemSE.CRI_SYSTEMSE_SYS_OK03;

	private int amountCostNow;

	private int amountNow;

	private List<GACHA_TABLE> listGift = new List<GACHA_TABLE>();

	public bool IsInfinity { get; set; }

	public int CostAmount { get; set; }

	public int CostAmountMax { get; set; }

	public Sprite SprIconCost { get; set; }

	public void Setup(SHOP_TABLE p_shopData, int p_buytMax, ITEM_TABLE p_itemData, NetShopRecord p_record = null)
	{
		shopData = p_shopData;
		itemData = p_itemData;
		iconCost.sprite = SprIconCost;
		netShopRecord = p_record;
		extraGroup.gameObject.SetActive(shopData.n_AUTO_OPEN == 1);
		groupRt.sizeDelta = sizeDelta[shopData.n_AUTO_OPEN];
		SetGiftData();
		if (p_itemData.n_TYPE == 5 && p_itemData.n_TYPE_X == 1 && (int)p_itemData.f_VALUE_Y > 0)
		{
			CARD_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)p_itemData.f_VALUE_Y, out value))
			{
				string s_ICON = value.s_ICON;
				string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
				itemIcon.Setup(itemData.n_ID, p_bundleName, s_ICON, OnClickItem);
				itemIcon.SetRare(value.n_RARITY);
				string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(value.n_TYPE);
				imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
				imgCardType.color = Color.white;
			}
		}
		else
		{
			itemIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(itemData.s_ICON), itemData.s_ICON);
			itemIcon.SetRare(itemData.n_RARE);
			imgCardType.color = Color.clear;
		}
		itemIcon.SetAmount(shopData.n_PRODUCT_MOUNT);
		textItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemData.w_NAME);
		imgPiece.color = ((itemData.n_TYPE == 4) ? Color.white : Color.clear);
		textItemTip.alignByGeometry = false;
		textItemTip.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemData.w_TIP);
		amountMax = p_buytMax;
		sliderAddValue = 1f / (float)amountMax;
		textBuy.text = string.Format(amountFormat, 0, amountMax.ToString("#,0"));
		textCost.text = string.Format(costValueFormat, 0);
		btnBuy.interactable = false;
		slider.onValueChanged.AddListener(OnSliderValueChange);
		DisplayStackValue();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDisable()
	{
		slider.onValueChanged.RemoveListener(OnSliderValueChange);
	}

	private void OnSliderValueChange(float val)
	{
		amountNow = (int)((float)amountMax * val + 0.5f);
		if (amountNow != amountCache && amountNow >= 0 && amountNow <= amountMax)
		{
			amountCache = amountNow;
			amountCostNow = amountCache * CostAmount;
			textBuy.text = string.Format(amountFormat, amountCache.ToString("#,0"), amountMax.ToString("#,0"));
			textCost.text = string.Format(costValueFormat, amountCostNow.ToString("#,0"));
			if (isSlider)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
			}
			else
			{
				isSlider = true;
			}
			if (amountNow == 0)
			{
				textCost.color = textColor[0];
				outlineEft.effectColor = textOutlineColor[0];
				btnBuy.interactable = false;
			}
			else if (amountCostNow <= CostAmountMax)
			{
				textCost.color = textColor[0];
				outlineEft.effectColor = textOutlineColor[0];
				btnBuy.interactable = true;
			}
			else
			{
				textCost.color = textColor[1];
				outlineEft.effectColor = textOutlineColor[1];
				btnBuy.interactable = false;
			}
		}
	}

	private void SetGiftData()
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
			ItemIconBase itemIconBase = Object.Instantiate(extraGiftUnit, parent);
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
			MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
			itemIconBase.SetRare(rare);
			if (gACHA_TABLE.n_REWARD_TYPE == 1)
			{
				itemIconBase.Setup(i, bundlePath, assetPath, ShowItemInfo);
			}
			else
			{
				itemIconBase.Setup(i, bundlePath, assetPath);
			}
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
		isSlider = false;
		if (amountCache <= 0 || isBuying)
		{
			return;
		}
		isBuying = true;
		ManagedSingleton<PlayerNetManager>.Instance.PurchaseItemReq(shopData.n_ID, amountCache, delegate(List<NetRewardInfo> rewardList, NetShopRecord record, short buyAmount)
		{
			netShopRecord = record;
			if (!IsInfinity)
			{
				amountMax -= buyAmount;
				sliderAddValue = 1f / (float)amountMax;
			}
			CostAmountMax -= buyAmount * CostAmount;
			amountNow = 0;
			slider.value = 0f;
			OnSliderValueChange(0f);
			if (rewardList != null && rewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					int n_COIN_ID = shopData.n_COIN_ID;
					if (n_COIN_ID == 1 || (uint)(n_COIN_ID - 2) > 1u)
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE02);
					}
					else
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE01);
					}
					ui.Setup(rewardList, 0.3f);
				});
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SHOP);
			isBuying = false;
			if (amountMax <= 0)
			{
				base.CloseSE = SystemSE.NONE;
				OnClickCloseBtn();
			}
			else
			{
				DisplayStackValue();
			}
		});
	}

	public void OnClickAddBtn(int i)
	{
		if (i > 0)
		{
			if (slider.value != 1f)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
			}
			isSlider = false;
			slider.value += (float)i * sliderAddValue;
		}
		else if (i < 0)
		{
			if (slider.value != 0f)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
			}
			isSlider = false;
			slider.value += (float)i * sliderAddValue;
		}
	}

	public void OnClickMaxSellBtn()
	{
		isSlider = false;
		if (slider.value != 1f)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
		}
		slider.value = 1f;
	}

	public void OnClickMinSellBtn()
	{
		isSlider = false;
		if (slider.value != sliderAddValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
		}
		slider.value = sliderAddValue;
	}

	private void DisplayStackValue()
	{
		StarHelper.UnlockData unlockData;
		if (ManagedSingleton<StarHelper>.Instance.GetUnlockDataByItem(itemData, out unlockData))
		{
			int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(itemData.n_ID);
			int cost;
			if (ManagedSingleton<StarHelper>.Instance.GetUpgradeCostMaterialAmount(unlockData.Id, unlockData.StarType, StarHelper.CostSearchType.MaxLevel, out cost))
			{
				int cost2;
				ManagedSingleton<StarHelper>.Instance.GetUpgradeCostMaterialAmount(unlockData.Id, unlockData.StarType, StarHelper.CostSearchType.NextLevel, out cost2);
				canvasPieceInfoTop.enabled = true;
				canvasPieceInfoBottom.enabled = true;
				textPieceOwn.text = string.Format("{0}/{1}", itemValue, cost);
				textPieceNext.text = string.Format("{0}/{1}", itemValue, cost2);
				imgPieceFill.fillAmount = Mathf.Clamp01((float)itemValue / (float)cost2);
			}
			else if (cost == 0)
			{
				canvasPieceInfoTop.enabled = false;
				canvasPieceInfoBottom.enabled = true;
				textPieceNext.text = string.Format("{0}/Max", itemValue);
				imgPieceFill.fillAmount = 1f;
			}
			else
			{
				canvasPieceInfoTop.enabled = false;
				canvasPieceInfoBottom.enabled = true;
				textPieceNext.text = string.Format("{0}/{1}", itemValue, cost);
				imgPieceFill.fillAmount = Mathf.Clamp01((float)itemValue / (float)cost);
			}
		}
		else
		{
			canvasPieceInfoTop.enabled = false;
			canvasPieceInfoBottom.enabled = false;
			if (shopData.n_AUTO_OPEN == 1)
			{
				textItemOwn.text = string.Empty;
			}
			else if (itemData.n_TYPE == 5 && itemData.n_TYPE_X == 1 && (int)itemData.f_VALUE_Y > 0)
			{
				textItemOwn.text = string.Format("{0} {1}", MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_OWNED"), ManagedSingleton<PlayerHelper>.Instance.GetCardStackValue((int)itemData.f_VALUE_Y));
			}
			else
			{
				textItemOwn.text = string.Format("{0} {1}", MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_OWNED"), ManagedSingleton<PlayerHelper>.Instance.GetItemValue(itemData.n_ID));
			}
		}
	}

	public void OnClickSystemSE(int cuid)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE((SystemSE)cuid);
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(p_idx, out item) && item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardInfo", delegate(CardInfoUI ui)
			{
				ui.bOnlyShowBasic = true;
				ui.bNeedInitList = true;
				ui.nTargetCardSeqID = 0;
				ui.nTargetCardID = (int)item.f_VALUE_Y;
			});
		}
	}
}
