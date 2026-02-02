#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using Better;
using CallbackDefs;
using OrangeApi;
using Steamworks;
using enums;

public class OrangeIAP : MonoBehaviourSingleton<OrangeIAP>, IOrangeIAP
{
	private bool m_PurchaseInProgress;

	private Callback onPurchaseInit;
    [Obsolete]
    private CallbackObjs onPurchaseSuccessRes;
    [Obsolete]
    private CallbackObj onPurchaseFailRes;

	private SHOP_TABLE shopTable;

	private string _currentPurchaseOrderID = "";

	private EmptyBlockUI blockUI;

	private OrangeProduct product;

	public const int SHOP_CONNECT_TYPE_ITEM = 1;

	public const int SHOP_CONNECT_TYPE_SERVICE = 6;

	public bool Initialized { get; private set; }

	public IAPStoreType Store { get; private set; }

	public Better.Dictionary<string, OrangeProduct> DictProduct { get; private set; }

	public bool InProgress
	{
		get
		{
			return m_PurchaseInProgress;
		}
	}

	private void SaveTempIAPReceipt(IAPReceipt tempReceipt)
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicNewReceipt.Add((sbyte)tempReceipt.StoreType + tempReceipt.TransactionID, tempReceipt);
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	private void ShowMsg(string msg)
	{
		if (!string.IsNullOrEmpty(msg))
		{
			CommonUI msgUI = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<CommonUI>("UI_CommonMsg", false, true);
			msgUI.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
			msgUI.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
				msgUI.CloseSE = SystemSE.NONE;
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
			});
		}
	}

	public void CheckClientNewReceipt(int idx, List<IAPReceipt> listReceipt, List<NetRewardInfo> listNetReward, Callback p_cb = null)
	{
		if (listReceipt == null || idx >= listReceipt.Count)
		{
			if (listNetReward != null && listNetReward.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<CommonUI>("UI_CommonMsg", false, true).SetupConfirmByKey("COMMON_TIP", "PURCHASE_COMPLETE", "COMMON_OK", delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
					{
						ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
						{
							CheckInProgressIAP(p_cb);
						});
						ui.Setup(listNetReward);
					});
				});
			}
			else
			{
				CheckInProgressIAP(p_cb);
			}
			return;
		}
		List<NetRewardInfo> listReward = listNetReward;
		IAPReceipt iAPReceipt = listReceipt[idx];
		if (string.IsNullOrEmpty(iAPReceipt.ProductID))
		{
			idx++;
			CheckClientNewReceipt(idx, listReceipt, listReward, p_cb);
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.IAPExchangReq(iAPReceipt, delegate(int shopItemId, List<NetPlayerServiceInfo> serviceInfoList, NetRewardsEntity rewardsEntity, string msgToPlayer)
		{
			if (rewardsEntity != null && rewardsEntity.RewardList != null && rewardsEntity.RewardList.Count > 0)
			{
				listReward.AddRange(rewardsEntity.RewardList);
			}
			idx++;
			CheckClientNewReceipt(idx, listReceipt, listReward, p_cb);
		});
	}

	private void CheckInProgressIAP(Callback p_cb)
	{
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.IAPInProgress)
		{
			Init(p_cb);
		}
		else
		{
			p_cb.CheckTargetToInvoke();
		}
	}

	public bool IsPayLimit(int readyToCost)
	{
		return false;
	}

	private void Awake()
	{
		Initialized = false;
		Store = IAPStoreType.Steam;
	}

	public void Init(Callback initCB)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		Initialized = false;
		onPurchaseInit = initCB;
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveIAPProductReq(delegate(List<NetIAPProductInfo> p_param)
		{
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			List<string> listVaildProduct = new List<string>();
			for (int i = 0; i < p_param.Count; i++)
			{
				NetIAPProductInfo netIAPProductInfo = p_param[i];
				if (ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(netIAPProductInfo.BeginTime, netIAPProductInfo.EndTime, serverUnixTimeNowUTC))
				{
					listVaildProduct.Add(netIAPProductInfo.ProductID);
				}
			}
			DictProduct = GetDictProductVaild(ref listVaildProduct);
			Initialized = true;
			onPurchaseInit.CheckTargetToInvoke();
			TransactionComplete();
		});
	}

	public Better.Dictionary<string, OrangeProduct> GetDictProductVaild(ref List<string> listVaildProduct)
	{
		Better.Dictionary<string, OrangeProduct> dictionary = new Better.Dictionary<string, OrangeProduct>();
		List<SHOP_TABLE> shopListByIAP = ManagedSingleton<ExtendDataHelper>.Instance.GetShopListByIAP(Store);
		foreach (string product in listVaildProduct)
		{
			SHOP_TABLE sHOP_TABLE = shopListByIAP.FirstOrDefault((SHOP_TABLE x) => x.s_PRODUCT_ID == product);
			if (sHOP_TABLE != null)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ASIA_VERSION_DEFAULT_CURRENCY");
				dictionary.Add(product, new OrangeProduct
				{
					ProductID = product,
					LocalizedPriceString = string.Format(string.Format("{0} {1}", str, sHOP_TABLE.n_COIN_MOUNT)),
					LocalizedPrice = sHOP_TABLE.n_COIN_MOUNT
				});
			}
		}
		return dictionary;
	}

	public void DoPurchase(SHOP_TABLE p_shopTable, OrangeProduct p_product)
	{
		if (m_PurchaseInProgress)
		{
			Debug.Log("[OrangeIAP] Please wait, purchase in progress");
			ShowMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("IAP_FAIL_1_EXISTING_PURCHASE_PENDING"));
		}
		else if (p_shopTable == null)
		{
			Debug.LogError("[OrangeIAP] ShopTable Not Exist.");
			ShowMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("IAP_FAIL_2_PRODUCT_UNAVAILABLE"));
		}
		else if (!ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(p_shopTable.s_BEGIN_TIME, p_shopTable.s_END_TIME, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
		{
			Debug.LogError("[OrangeIAP] Product Out of Date. " + p_shopTable.n_ID);
			ShowMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("IAP_FAIL_2_PRODUCT_UNAVAILABLE"));
		}
		else
		{
			BuyItem(p_product, p_shopTable);
		}
	}

	private void BuyItem(OrangeProduct p_product, SHOP_TABLE p_shopTable)
	{
		if (p_shopTable.s_PRODUCT_ID != p_product.ProductID)
		{
			Debug.LogError("[OrangeIAP] ProductID Not Matched!! ShopTable=>" + p_shopTable.s_PRODUCT_ID + " , OrangeProduct=>" + p_product.ProductID);
			return;
		}
		string text = string.Empty;
		ITEM_TABLE item;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(p_shopTable.w_PRODUCT_NAME))
		{
			text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_shopTable.w_PRODUCT_NAME);
		}
		else if (p_shopTable.n_PRODUCT_TYPE == 1 && ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_shopTable.n_PRODUCT_ID, out item))
		{
			text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(item.w_NAME);
		}
		if (string.IsNullOrEmpty(text))
		{
			ShowMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STEAM_NO_SHOPITEM_DESC"));
			return;
		}
		SteamFriends.OnGameOverlayActivated += OnGameOverlayActivated;
		ManagedSingleton<PlayerNetManager>.Instance.StartSteamPurchaseReq(MonoBehaviourSingleton<SteamManager>.Instance.GetUserTicket(), p_shopTable.n_ID, p_shopTable.s_PRODUCT_ID, text, delegate(StartSteamPurchaseRes res)
		{
			if (res.Code == 20301)
			{
				ShowMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STEAM_TRANSACION_FAILED"));
				TransactionComplete();
			}
			else
			{
				_currentPurchaseOrderID = res.OrderID;
				blockUI = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<EmptyBlockUI>("UI_EmptyBlock", true);
				blockUI.SetBlock(true);
				m_PurchaseInProgress = true;
				onPurchaseSuccessRes = null;
				onPurchaseFailRes = null;
				shopTable = p_shopTable;
				product = p_product;
			}
		});
	}

	private void OnGameOverlayActivated(bool activated)
	{
		Debug.Log(string.Format("OnGameOverlayActivated => {0}", activated));
		if (!activated)
		{
			SteamFriends.OnGameOverlayActivated -= OnGameOverlayActivated;
			if (!MonoBehaviourSingleton<SteamManager>.Instance.SteamConnected)
			{
				TransactionComplete();
				Debug.Log("在Overlay的狀態下失去連線並關閉，恢復Game本身對UI的鎖定!!");
			}
		}
	}

	private void TransactionComplete()
	{
		_currentPurchaseOrderID = string.Empty;
		shopTable = null;
		product = null;
		m_PurchaseInProgress = false;
		if ((bool)blockUI)
		{
			blockUI.OnClickCloseBtn();
			blockUI = null;
		}
	}

	public void OnMicroTxnAuthorizationResponse(AppId appId, ulong orderId, bool tradeProceed)
	{
		if (!tradeProceed)
		{
			ShowMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STEAM_TRANSACTION_CANCELED"));
			if (orderId.ToString() != _currentPurchaseOrderID)
			{
				Debug.LogError(string.Format("Steam OrderID not match!! Steam Api orderID =>{0} GameService response orderID => {1}", orderId, _currentPurchaseOrderID));
			}
			else
			{
				ManagedSingleton<PlayerNetManager>.Instance.CancelSteamPurchaseReq(_currentPurchaseOrderID, null);
			}
			TransactionComplete();
		}
		else if ((uint)appId != MonoBehaviourSingleton<SteamManager>.Instance.AppID)
		{
			ShowMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STEAM_APPID_NOT_MATCHED"));
			TransactionComplete();
		}
		else
		{
			Exchange(orderId);
		}
	}

	private void Exchange(ulong orderId)
	{
		IAPReceipt iapReceipt = new IAPReceipt
		{
			Payload = "",
			ProductID = product.ProductID,
			TransactionID = orderId.ToString(),
			ShopItemID = shopTable.n_ID,
			StoreType = Store
		};
		Debug.Log("[OrangeIAP] buy success.");
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STORE03);
		ManagedSingleton<PlayerNetManager>.Instance.IAPExchangReq(iapReceipt, delegate(int shopItemId, List<NetPlayerServiceInfo> serviceInfoList, NetRewardsEntity rewardsEntity, string msgToPlayer)
		{
			if (rewardsEntity != null && rewardsEntity.RewardList != null && rewardsEntity.RewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(rewardsEntity.RewardList);
				});
			}
			ShowMsg(msgToPlayer);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SHOP);
			TransactionComplete();
		});
		if ((bool)blockUI)
		{
			blockUI.OnClickCloseBtn();
			blockUI = null;
		}
	}
}
