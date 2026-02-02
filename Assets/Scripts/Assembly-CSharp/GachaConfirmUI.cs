using NaughtyAttributes;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;

public class GachaConfirmUI : OrangeUIBase
{
	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private OrangeText textMsg;

	[SerializeField]
	private OrangeText textCostBefore;

	[SerializeField]
	private OrangeText textCostAfter;

	[SerializeField]
	private GameObject groupCost;

	[SerializeField]
	private GameObject groupEx;

	[SerializeField]
	private OrangeText textExBefore;

	[SerializeField]
	private OrangeText textExAfter;

	[SerializeField]
	private Image imgCost;

	[SerializeField]
	private Image imgEx;

	[SerializeField]
	private Button gachaBtn;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_gachaSE;

	[SerializeField]
	private SystemSE m_addJewelSE;

	private GACHALIST_TABLE gachalistTable;

	private ITEM_TABLE costItem;

	private int ownAmount;

	private Transform objLaw;

	private bool isClick;

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CHARGE_STAMINA, RefreashUI);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CHARGE_STAMINA, RefreashUI);
	}

	private void RefreashUI()
	{
		if (gachalistTable != null && costItem != null)
		{
			Setup(gachalistTable, costItem);
		}
	}

	public void Setup(GACHALIST_TABLE p_gachalistTable, ITEM_TABLE p_costItem)
	{
		gachalistTable = p_gachalistTable;
		costItem = p_costItem;
		gachaBtn.onClick.RemoveAllListeners();
		bool flag = costItem.n_ID == OrangeConst.ITEMID_FREE_JEWEL || costItem.n_ID == OrangeConst.ITEMID_JEWEL;
		if (flag && objLaw == null)
		{
			CommonAssetHelper.LoadLawObj("BtnJPLaw", base.transform, new Vector3(0f, -370f, 0f), delegate(Transform t)
			{
				objLaw = t;
			});
		}
		if (CanGacha())
		{
			gachaBtn.interactable = true;
			gachaBtn.onClick.AddListener(delegate
			{
				OnClickGacha(gachalistTable.n_ID);
			});
			textCostAfter.color = Color.white;
		}
		else
		{
			if (flag)
			{
				gachaBtn.interactable = true;
				gachaBtn.onClick.AddListener(OnClickLinkBuyJewel);
			}
			else if (costItem.n_ID == OrangeConst.ITEMID_MONEY)
			{
				gachaBtn.interactable = true;
				gachaBtn.onClick.AddListener(OnClickBuyMoney);
			}
			else
			{
				gachaBtn.interactable = false;
			}
			textCostAfter.color = Color.red;
		}
		SetUiInfo();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private bool CanGacha()
	{
		if (costItem != null)
		{
			if (costItem.n_ID == OrangeConst.ITEMID_FREE_JEWEL)
			{
				ownAmount = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel();
				return ownAmount >= gachalistTable.n_COIN_MOUNT;
			}
			ownAmount = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(costItem.n_ID);
			return ownAmount >= gachalistTable.n_COIN_MOUNT;
		}
		return true;
	}

	private void SetUiInfo()
	{
		textTitle.UpdateText(gachalistTable.w_BUTTON_TEXT);
		string empty = string.Empty;
		if (costItem != null)
		{
			if (gachalistTable.n_COIN_MOUNT > 0)
			{
				empty = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr((gachalistTable.n_TYPE == 2) ? "ROULETTE_MESSAGE" : "GACHA_MESSAGE"), gachalistTable.n_COIN_MOUNT, ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(costItem.w_NAME), gachalistTable.n_GACHACOUNT_1 + gachalistTable.n_GACHACOUNT_2);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(costItem.s_ICON), costItem.s_ICON, delegate(Sprite obj)
				{
					imgCost.sprite = obj;
				});
				groupCost.SetActive(true);
				textCostBefore.text = ownAmount.ToString("#,0");
				textCostAfter.text = (ownAmount - gachalistTable.n_COIN_MOUNT).ToString("#,0");
			}
			else
			{
				empty = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GACHA_MESSAGE_FREE");
				groupCost.SetActive(false);
			}
		}
		else
		{
			empty = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GACHA_MESSAGE_FREE");
			groupCost.SetActive(false);
		}
		empty = empty + "\n" + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(gachalistTable.w_CONFIRM_TIP);
		textMsg.alignByGeometry = false;
		textMsg.text = empty;
		if (gachalistTable.n_BONUS != 0)
		{
			groupEx.SetActive(true);
			ITEM_TABLE item = null;
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(gachalistTable.n_BONUS, out item))
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON, delegate(Sprite obj)
				{
					imgEx.sprite = obj;
				});
				int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(gachalistTable.n_BONUS);
				textExBefore.text = itemValue.ToString();
				textExAfter.text = (itemValue + gachalistTable.n_BONUS_COUNT).ToString();
			}
			else
			{
				groupEx.SetActive(false);
			}
		}
		else
		{
			groupEx.SetActive(false);
		}
	}

	private void OnClickLinkBuyJewel()
	{
		base.CloseSE = SystemSE.NONE;
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.SetupYesNoByKey("COMMON_TIP", "DIAMOND_OUT", "COMMON_OK", "COMMON_CANCEL", delegate
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI shopUI)
				{
					shopUI.EnableBackToHometop = false;
					shopUI.Setup(ShopTopUI.ShopSelectTab.directproduct);
				});
			});
			OnClickCloseBtn();
		});
	}

	private void OnClickBuyMoney()
	{
	}

	private void OnClickGacha(int gachaId)
	{
		if (isClick)
		{
			return;
		}
		isClick = true;
		ManagedSingleton<PlayerNetManager>.Instance.GachaReq(gachaId, delegate(GachaRes res)
		{
			switch (gachalistTable.n_PERFORM)
			{
			default:
				base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE02;
				OnClickCloseBtn();
				MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SHOP);
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui2)
					{
						MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
						{
							ui2.Setup(res.RewardEntities);
						});
					});
				}, OrangeSceneManager.LoadingType.WHITE);
				break;
			case 1:
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_gachaSE);
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("Gacha", OrangeSceneManager.LoadingType.TIP, delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
					{
						OrangeSceneManager.FindObjectOfTypeCustom<GachaSceneController>().Init(res.RewardEntities);
					});
				}, false);
				break;
			case 2:
				base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE02;
				OnClickCloseBtn();
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.GACHA_PRIZE_START, res);
				break;
			}
		});
	}
}
