using System.Collections.Generic;
using CallbackDefs;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class CardStorageBuyUI : OrangeUIBase
{
	[SerializeField]
	private RectTransform groupRt;

	[SerializeField]
	private ItemIconWithAmount itemIcon;

	[SerializeField]
	private OrangeText TitleNameText;

	[SerializeField]
	private OrangeText IconNameText;

	[SerializeField]
	private OrangeText IconMessageText;

	[SerializeField]
	private Image IconImage;

	[SerializeField]
	private Image CoinImage;

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

	private Vector2[] sizeDelta = new Vector2[2]
	{
		new Vector2(1143f, 700f),
		new Vector2(1143f, 897f)
	};

	private SHOP_TABLE shopData;

	private ITEM_TABLE itemData;

	private NetShopRecord netShopRecord;

	private string amountFormat = "{0}/{1}";

	private string costValueFormat = "x {0}";

	private int amountMax;

	private int amountCache = -1;

	private bool isBuying;

	private bool isSlider = true;
    [System.Obsolete]
    private CallbackObj ClickBuyCB;

	private SystemSE BtnSE = SystemSE.CRI_SYSTEMSE_SYS_OK03;

	private int amountCostNow;

	private int amountNow;

	private List<GACHA_TABLE> listGift = new List<GACHA_TABLE>();

	public bool IsInfinity { get; set; }

	public int CostAmount { get; set; }

	public int CostAmountMax { get; set; }

	public Sprite SprIconCost { get; set; }

    [System.Obsolete]
    public void Setup(int BuyCount, int MaxCount, string TitleName, int IconID, int CoinID, CallbackObj p_cb = null)
	{
		ClickBuyCB = p_cb;
		TitleNameText.text = TitleName;
		amountMax = MaxCount;
		slider.minValue = 0f;
		slider.maxValue = amountMax;
		btnBuy.interactable = false;
		ITEM_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.ITEM_TABLE_DICT.TryGetValue(IconID, out value);
		if (value != null)
		{
			IconNameText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			IconMessageText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
			itemIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(value.s_ICON), value.s_ICON);
			itemIcon.SetRare(value.n_RARE);
			itemIcon.SetAmount(BuyCount);
		}
		value = null;
		ManagedSingleton<OrangeDataManager>.Instance.ITEM_TABLE_DICT.TryGetValue(CoinID, out value);
		if (value != null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(value.s_ICON), value.s_ICON, delegate(Sprite obj)
			{
				if (obj != null)
				{
					CoinImage.sprite = obj;
				}
			});
		}
		slider.onValueChanged.AddListener(OnSliderValueChange);
		isSlider = false;
		OnSliderValueChange(0f);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDisable()
	{
		slider.onValueChanged.RemoveListener(OnSliderValueChange);
	}

	private void OnSliderValueChange(float val)
	{
		amountNow = Mathf.RoundToInt(val);
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
		if (ClickBuyCB != null)
		{
			ClickBuyCB.CheckTargetToInvoke(amountCache);
			base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
			OnClickCloseBtn();
		}
	}

	public void OnClickAddBtn(int i)
	{
		if (i > 0)
		{
			if (slider.value != slider.maxValue)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
			}
			isSlider = false;
			slider.value += 1f;
		}
		else if (i < 0)
		{
			if (slider.value != slider.minValue)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
			}
			isSlider = false;
			slider.value += -1f;
		}
	}

	public void OnClickMaxSellBtn()
	{
		isSlider = false;
		if (slider.value != slider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
		}
		slider.value = slider.maxValue;
	}

	public void OnClickMinSellBtn()
	{
		isSlider = false;
		if (slider.value != slider.minValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(BtnSE);
		}
		slider.value = slider.minValue;
	}

	public void OnClickSystemSE(int cuid)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE((SystemSE)cuid);
	}
}
