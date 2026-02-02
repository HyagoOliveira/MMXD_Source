using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Better;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class ShopTopUI : OrangeUIBase
{
	public enum ShopSelectTab
	{
		directproduct = 0,
		item_shop = 1
	}

	private enum ShopMainType
	{
		resident = 1,
		token = 2,
		fes = 3,
		limit = 5,
		skin = 6,
		card = 7,
		dark_code_coin = 8,
		promos = 9
	}

	public enum ShopSubType
	{
		sub_1 = 1,
		sub_2 = 2,
		sub_3 = 3
	}

	private ShopSelectTab shopSelectTab = ShopSelectTab.item_shop;

	private List<StorageInfo> listStorage = new List<StorageInfo>();

	[SerializeField]
	private Transform storageRoot;

	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private ShopItemUnit shopItemUnit;

	[SerializeField]
	private ShopItemUnit iapItemUnit;

	[SerializeField]
	private ItemBoxTab tabSub1;

	[SerializeField]
	private ItemBoxTab tabSub2;

	[SerializeField]
	private ItemBoxTab tabSub3;

	[SerializeField]
	private Image imgCost;

	[SerializeField]
	private OrangeText textCost;

	[SerializeField]
	private OrangeText textDialog;

	[SerializeField]
	private Transform stParent;

	[SerializeField]
	private GridLayoutGroup contentGridLayout;

	[SerializeField]
	private Canvas groupCost;

	[SerializeField]
	private Canvas groupCostJewel;

	[SerializeField]
	private OrangeText textCostJewel;

	[SerializeField]
	private OrangeText textCostJewelFree;

	[SerializeField]
	private Canvas shopBlock;

	[BoxGroup("Sound")]
	[SerializeField]
	private BGM01 m_bgm;

	private List<SHOP_TABLE> listShopItemAll = new List<SHOP_TABLE>();

	public List<SHOP_TABLE> ListShopItemNow = new List<SHOP_TABLE>();

	private RectOffset offsetShop;

	private RectOffset offsetIAP;

	private Vector2 cellSizeShop = new Vector2(285f, 300f);

	private Vector2 cellSizeIAP = new Vector2(500f, 300f);

	private Vector2 spacingShop = new Vector2(67.73f, 55f);

	private Vector2 spacingIAP = new Vector2(10f, 110f);

	private int constraintCountShop = 3;

	private int constraintCountIAP = 2;

	private string[] m_preSceneBgm;

	private ShopSubType nowSub = ShopSubType.sub_1;

	private List<ShopMainType> listMain = new List<ShopMainType>();

	private bool isIAPInit;

	private Coroutine coroutineHandle;

	private ShopMainType shopMainType = ShopMainType.resident;

	private Coroutine coroutineDisplayDialog;

	private WaitForSeconds sec = new WaitForSeconds(0.05f);

	private bool showJewelInfo;

	private readonly int PRODUCT_MAIN_TYPE = 4;

	private readonly int PRODUCT_PASS_TYPE;

	private bool isStop;

	public long TimeNow { get; private set; }

	public int DefaultSubIdx { get; set; }

	protected override void Awake()
	{
		base.Awake();
		offsetShop = new RectOffset(36, 0, 50, 50);
		offsetIAP = new RectOffset(21, 0, 66, 66);
		DefaultSubIdx = 0;
	}

	public void Setup(ShopSelectTab p_selectTab, ShopSubType p_nowSub = ShopSubType.sub_1)
	{
		shopBlock.enabled = true;
		isIAPInit = false;
		shopSelectTab = p_selectTab;
		nowSub = p_nowSub;
		m_preSceneBgm = new string[2]
		{
			MonoBehaviourSingleton<AudioManager>.Instance.bgmSheet,
			MonoBehaviourSingleton<AudioManager>.Instance.bgmCue
		};
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveShopRecordReq(delegate
		{
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			listShopItemAll = ManagedSingleton<ExtendDataHelper>.Instance.GetShopTableByOpening(serverUnixTimeNowUTC);
			TimeNow = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowLocale;
			coroutineHandle = StartCoroutine(OnStartUpdateTime());
			foreach (ShopMainType value in Enum.GetValues(typeof(ShopMainType)))
			{
				MainTypeExist(value);
			}
			CreateNewStorageTab();
		});
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_0"), "ch_navi_0_db", delegate(GameObject obj)
		{
			StandNaviDb component = UnityEngine.Object.Instantiate(obj, stParent, false).GetComponent<StandNaviDb>();
			if ((bool)component)
			{
				component.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL);
			}
			PlayBgm();
		});
	}

	private void CreateNewStorageTab()
	{
		StorageInfo storageInfo = new StorageInfo("SHOP_TAB_000_000", false, 0, OnClickTab);
		StorageInfo storageInfo2 = new StorageInfo("SHOP_TAB_001_000", false, listMain.Count);
		for (int i = 0; i < listMain.Count; i++)
		{
			storageInfo2.Sub[i] = new StorageInfo(GetSubKeyByMainType(listMain[i]), false, 0, OnClickTab);
			storageInfo2.Sub[i].Param = new object[2]
			{
				ShopSelectTab.item_shop,
				listMain[i]
			};
		}
		storageInfo.Param = new object[1] { ShopSelectTab.directproduct };
		listStorage.Add(storageInfo);
		listStorage.Add(storageInfo2);
		int num = 0;
		if (DefaultSubIdx > 0 && listMain.Count > 0)
		{
			num = listMain.FindIndex((ShopMainType x) => x == (ShopMainType)DefaultSubIdx);
			if (num == -1)
			{
				num = 0;
			}
		}
		StorageGenerator.Load("StorageComp00", listStorage, (int)shopSelectTab, num, storageRoot);
	}

	private void PlayBgm()
	{
		StartCoroutine(DelayPlaySE(0.5f));
		MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
	}

	public void GoShop(ShopSelectTab p_tab)
	{
		if (listStorage.Count >= 2 && p_tab != shopSelectTab)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK16);
			switch (p_tab)
			{
			case ShopSelectTab.directproduct:
				listStorage[0].ClickCb.CheckTargetToInvoke(listStorage[0]);
				break;
			default:
				listStorage[1].ClickCb.CheckTargetToInvoke(listStorage[1]);
				break;
			}
		}
	}

	private void OnClickTab(object p_param)
	{
		scrollRect.ClearCells();
		StorageInfo storageInfo = (StorageInfo)p_param;
		ShopSelectTab shopSelectTab = (ShopSelectTab)storageInfo.Param[0];
		if (this.shopSelectTab != shopSelectTab)
		{
			nowSub = ShopSubType.sub_1;
		}
		this.shopSelectTab = shopSelectTab;
		switch (shopSelectTab)
		{
		case ShopSelectTab.directproduct:
			contentGridLayout.SetPadding(offsetIAP);
			contentGridLayout.UpdateValue(cellSizeIAP, spacingIAP, constraintCountIAP);
			OnClickSubTab(nowSub);
			break;
		case ShopSelectTab.item_shop:
			contentGridLayout.SetPadding(offsetShop);
			contentGridLayout.UpdateValue(cellSizeShop, spacingShop, constraintCountShop);
			shopMainType = (ShopMainType)storageInfo.Param[1];
			OnClickSubTab(nowSub);
			break;
		}
		UpdateTab();
	}

	private void SetItemShop(ShopMainType type)
	{
		shopMainType = type;
		int num = (int)shopMainType;
		int num2 = (int)nowSub;
		if (num == 1 && num2 == 2)
		{
			ListShopItemNow = GetShopListCharacterSpecific(num, num2);
		}
		else if (num == 1 && num2 == 3)
		{
			ListShopItemNow = GetShopListWeaponSpecific(num, num2);
		}
		else
		{
			ListShopItemNow = GetShopListByMainAndSubType(num, num2);
		}
		scrollRect.OrangeInit(shopItemUnit, 10, ListShopItemNow.Count);
		tabSub1.AddBtnCB(delegate
		{
			OnClickSubTab(ShopSubType.sub_1);
		});
		tabSub2.AddBtnCB(delegate
		{
			OnClickSubTab(ShopSubType.sub_2);
		});
		tabSub3.AddBtnCB(delegate
		{
			OnClickSubTab(ShopSubType.sub_3);
		});
	}

	private List<SHOP_TABLE> GetShopListByMainType(int typeMain)
	{
		return listShopItemAll.Where((SHOP_TABLE x) => x.n_MAIN_TYPE == typeMain).ToList();
	}

	private List<SHOP_TABLE> GetShopListByMainAndSubType(int typeMain, int tpyeSub)
	{
		return listShopItemAll.Where((SHOP_TABLE x) => x.n_MAIN_TYPE == typeMain && x.n_SUB_TYPE == tpyeSub && CheckPre(ref x)).ToList();
	}

	private List<SHOP_TABLE> GetShopListWeaponSpecific(int typeMain, int tpyeSub)
	{
		List<int> listWeaponShardId = new List<int>();
		foreach (WeaponInfo value2 in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Values)
		{
			WEAPON_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(value2.netInfo.WeaponID, out value))
			{
				int n_UNLOCK_ID = value.n_UNLOCK_ID;
				if (n_UNLOCK_ID != 0)
				{
					listWeaponShardId.Add(n_UNLOCK_ID);
				}
			}
		}
		return listShopItemAll.Where((SHOP_TABLE x) => x.n_MAIN_TYPE == typeMain && x.n_SUB_TYPE == tpyeSub && CheckPre(ref x) && x.n_PRODUCT_TYPE == 1 && listWeaponShardId.Contains(x.n_PRODUCT_ID)).ToList();
	}

	private List<SHOP_TABLE> GetShopListCharacterSpecific(int typeMain, int tpyeSub)
	{
		List<int> listCharacterShardId = new List<int>();
		foreach (CharacterInfo value2 in ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Values)
		{
			CHARACTER_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(value2.netInfo.CharacterID, out value))
			{
				int n_UNLOCK_ID = value.n_UNLOCK_ID;
				if (n_UNLOCK_ID != 0)
				{
					listCharacterShardId.Add(n_UNLOCK_ID);
				}
			}
		}
		return listShopItemAll.Where((SHOP_TABLE x) => x.n_MAIN_TYPE == typeMain && x.n_SUB_TYPE == tpyeSub && CheckPre(ref x) && x.n_PRODUCT_TYPE == 1 && listCharacterShardId.Contains(x.n_PRODUCT_ID)).ToList();
	}

	private void SetDialog()
	{
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SHOP_TALK_DEFAULT");
		if (coroutineDisplayDialog != null)
		{
			StopCoroutine(coroutineDisplayDialog);
		}
		coroutineDisplayDialog = StartCoroutine(OnStartDisplayDialog(str));
	}

	private IEnumerator OnStartDisplayDialog(string val)
	{
		textDialog.text = string.Empty;
		for (int i = 0; i < val.Length; i++)
		{
			yield return sec;
			textDialog.text += val[i];
		}
	}

	private void SetCoinData()
	{
		ITEM_TABLE item = null;
		groupCost.enabled = false;
		groupCostJewel.enabled = false;
		showJewelInfo = false;
		if (shopSelectTab == ShopSelectTab.directproduct)
		{
			ManagedSingleton<OrangeTableHelper>.Instance.GetItem(OrangeConst.ITEMID_JEWEL, out item);
			textCostJewel.text = ManagedSingleton<PlayerHelper>.Instance.GetPaidJewel().ToString();
			textCostJewelFree.text = ManagedSingleton<PlayerHelper>.Instance.GetFreeJewel().ToString();
			groupCostJewel.enabled = true;
		}
		else
		{
			if (ListShopItemNow.Count <= 0)
			{
				return;
			}
			int n_COIN_ID = ListShopItemNow[0].n_COIN_ID;
			if (n_COIN_ID == OrangeConst.ITEMID_JEWEL || n_COIN_ID == OrangeConst.ITEMID_FREE_JEWEL)
			{
				showJewelInfo = true;
				ManagedSingleton<OrangeTableHelper>.Instance.GetItem(OrangeConst.ITEMID_JEWEL, out item);
				textCost.text = (ManagedSingleton<PlayerHelper>.Instance.GetPaidJewel() + ManagedSingleton<PlayerHelper>.Instance.GetFreeJewel()).ToString();
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON, delegate(Sprite obj)
				{
					imgCost.color = Color.white;
					imgCost.sprite = obj;
					groupCost.enabled = true;
				});
			}
			else if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(n_COIN_ID, out item))
			{
				textCost.text = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(item.n_ID).ToString();
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON, delegate(Sprite obj)
				{
					imgCost.color = Color.white;
					imgCost.sprite = obj;
					groupCost.enabled = true;
				});
			}
		}
	}

	private string GetSubKeyByMainType(ShopMainType type)
	{
		switch (type)
		{
		case ShopMainType.resident:
			return "SHOP_TAB_001_001";
		case ShopMainType.token:
			return "SHOP_TAB_001_002";
		case ShopMainType.fes:
			return "SHOP_TAB_001_003";
		case ShopMainType.limit:
			return "SHOP_TAB_001_005";
		case ShopMainType.skin:
			return "SHOP_TAB_001_006";
		case ShopMainType.card:
			return "SHOP_TAB_001_007";
		case ShopMainType.dark_code_coin:
			return "SHOP_TAB_001_008";
		case ShopMainType.promos:
			return "SHOP_TAB_001_009";
		default:
			return string.Empty;
		}
	}

	private void MainTypeExist(ShopMainType p_type)
	{
		int type = (int)p_type;
		if (listShopItemAll.Exists((SHOP_TABLE x) => x.n_MAIN_TYPE == type))
		{
			listMain.Add(p_type);
		}
	}

	private bool SubTypeExist(ref List<SHOP_TABLE> refList, int type)
	{
		return refList.Exists((SHOP_TABLE x) => x.n_SUB_TYPE == type);
	}

	private void UpdateTab()
	{
		switch (shopSelectTab)
		{
		default:
			tabSub3.gameObject.SetActive(false);
			tabSub2.gameObject.SetActive(false);
			tabSub1.gameObject.SetActive(false);
			break;
		case ShopSelectTab.directproduct:
		{
			tabSub3.gameObject.SetActive(false);
			tabSub1.gameObject.SetActive(false);
			tabSub2.gameObject.SetActive(false);
			List<SHOP_TABLE> refList2 = GetShopListByMainType(PRODUCT_MAIN_TYPE);
			if (SubTypeExist(ref refList2, 1))
			{
				tabSub1.gameObject.SetActive(true);
				SHOP_TABLE sHOP_TABLE4 = refList2.First((SHOP_TABLE x) => x.n_SUB_TYPE == 1);
				tabSub1.SetTextStr(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(sHOP_TABLE4.w_SHEET_NAME));
			}
			if (SubTypeExist(ref refList2, 2))
			{
				tabSub2.gameObject.SetActive(true);
				SHOP_TABLE sHOP_TABLE5 = refList2.First((SHOP_TABLE x) => x.n_SUB_TYPE == 2);
				tabSub2.SetTextStr(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(sHOP_TABLE5.w_SHEET_NAME));
			}
			if (SubTypeExist(ref refList2, 3))
			{
				tabSub3.gameObject.SetActive(true);
				SHOP_TABLE sHOP_TABLE6 = refList2.First((SHOP_TABLE x) => x.n_SUB_TYPE == 3);
				tabSub3.SetTextStr(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(sHOP_TABLE6.w_SHEET_NAME));
			}
			break;
		}
		case ShopSelectTab.item_shop:
		{
			tabSub3.gameObject.SetActive(false);
			tabSub2.gameObject.SetActive(false);
			tabSub1.gameObject.SetActive(false);
			List<SHOP_TABLE> refList = GetShopListByMainType((int)shopMainType);
			bool[] array = new bool[3];
			if (SubTypeExist(ref refList, 1))
			{
				tabSub1.gameObject.SetActive(true);
				SHOP_TABLE sHOP_TABLE = refList.First((SHOP_TABLE x) => x.n_SUB_TYPE == 1);
				tabSub1.SetTextStr(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(sHOP_TABLE.w_SHEET_NAME));
				array[0] = true;
			}
			if (SubTypeExist(ref refList, 2))
			{
				tabSub2.gameObject.SetActive(true);
				SHOP_TABLE sHOP_TABLE2 = refList.FirstOrDefault((SHOP_TABLE x) => x.n_SUB_TYPE == 2);
				tabSub2.SetTextStr(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(sHOP_TABLE2.w_SHEET_NAME));
				array[1] = true;
			}
			if (SubTypeExist(ref refList, 3))
			{
				tabSub3.gameObject.SetActive(true);
				SHOP_TABLE sHOP_TABLE3 = refList.First((SHOP_TABLE x) => x.n_SUB_TYPE == 3);
				tabSub3.SetTextStr(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(sHOP_TABLE3.w_SHEET_NAME));
				array[2] = true;
			}
			if (array[(int)(nowSub - 1)])
			{
				break;
			}
			nowSub = ShopSubType.sub_1;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i])
				{
					nowSub = (ShopSubType)(i + 1);
					break;
				}
			}
			OnClickSubTab(nowSub);
			break;
		}
		}
	}

	private void OnClickSubTab(ShopSubType shopSubType)
	{
		shopBlock.enabled = true;
		ListShopItemNow.Clear();
		nowSub = shopSubType;
		tabSub1.UpdateState(nowSub != ShopSubType.sub_1);
		tabSub2.UpdateState(nowSub != ShopSubType.sub_2);
		tabSub3.UpdateState(nowSub != ShopSubType.sub_3);
		switch (shopSelectTab)
		{
		case ShopSelectTab.directproduct:
			if (!isIAPInit)
			{
				MonoBehaviourSingleton<OrangeIAP>.Instance.Init(delegate
				{
					SetProductShop();
					SetDialog();
					SetCoinData();
					isIAPInit = true;
				});
			}
			else
			{
				SetProductShop();
				SetDialog();
				SetCoinData();
			}
			break;
		case ShopSelectTab.item_shop:
			SetItemShop(shopMainType);
			SetDialog();
			SetCoinData();
			break;
		}
		LeanTween.delayedCall(base.gameObject, 0.2f, (Action)delegate
		{
			shopBlock.enabled = false;
		});
	}

	public void RefreashShop()
	{
		OnClickSubTab(nowSub);
		ShopBuyIAPUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ShopBuyIAPUI>("UI_ShopBuyIAP");
		if (uI != null)
		{
			uI.CloseSE = SystemSE.NONE;
			uI.OnClickCloseBtn();
		}
	}

	public void RefreashCost()
	{
		SetCoinData();
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_SHOP, RefreashShop);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_TOPBAR_DATA, RefreashCost);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_SHOP, RefreashShop);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_TOPBAR_DATA, RefreashCost);
		StopCoroutine(coroutineHandle);
		MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(shopItemUnit.itemName);
		MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(iapItemUnit.itemName);
		MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
	}

	private IEnumerator OnStartUpdateTime()
	{
		while (true)
		{
			yield return CoroutineDefine._1sec;
			TimeNow++;
		}
	}

	private void SetProductShop()
	{
		SHOP_TABLE refTable = new SHOP_TABLE();
		Better.Dictionary<string, OrangeProduct> dictProduct = MonoBehaviourSingleton<OrangeIAP>.Instance.DictProduct;
		ListShopItemNow = (from x in GetShopListByMainAndSubType(PRODUCT_MAIN_TYPE, (int)nowSub)
			where dictProduct.ContainsKey(x.s_PRODUCT_ID) && CheckPre(ref refTable) && !CheckIAPLimit(ref refTable)
			select x).ToList();
		scrollRect.OrangeInit(iapItemUnit, 10, ListShopItemNow.Count);
		tabSub1.AddBtnCB(delegate
		{
			OnClickSubTab(ShopSubType.sub_1);
		});
		tabSub2.AddBtnCB(delegate
		{
			OnClickSubTab(ShopSubType.sub_2);
		});
		tabSub3.AddBtnCB(delegate
		{
			OnClickSubTab(ShopSubType.sub_3);
		});
	}

	private bool CheckPre(ref SHOP_TABLE p_shopTable)
	{
		if (p_shopTable.n_PRE != 0)
		{
			ShopInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicShop.TryGetValue(p_shopTable.n_PRE, out value) && value.netShopRecord != null)
			{
				SHOP_TABLE value2 = null;
				if (ManagedSingleton<ExtendDataHelper>.Instance.SHOP_TABLE_DICT.TryGetValue(p_shopTable.n_PRE, out value2))
				{
					if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(value.netShopRecord.LastShopTime, (ResetRule)value2.n_RESET_RULE))
					{
						value.netShopRecord.Count = 0;
					}
					if (value2.n_LIMIT <= value.netShopRecord.Count)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	private bool CheckIAPLimit(ref SHOP_TABLE p_shopTable)
	{
		if (p_shopTable.n_LIMIT != 0 && p_shopTable.n_RESET_RULE == 0)
		{
			ShopInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicShop.TryGetValue(p_shopTable.n_ID, out value))
			{
				return value.netShopRecord.Count >= p_shopTable.n_LIMIT;
			}
		}
		return false;
	}

	public void OnClickSystemSE(int cuid)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE((SystemSE)cuid);
	}

	private IEnumerator DelayPlaySE(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (!isStop)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 46);
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", (int)m_bgm);
		}
		yield return null;
	}

	protected override bool IsEscapeVisible()
	{
		if (shopBlock.enabled)
		{
			return false;
		}
		if (shopSelectTab == ShopSelectTab.directproduct && !isIAPInit)
		{
			return false;
		}
		return base.IsEscapeVisible();
	}

	public override bool CanBackToHometop()
	{
		if (shopSelectTab == ShopSelectTab.directproduct && !isIAPInit)
		{
			return false;
		}
		return base.CanBackToHometop();
	}

	public override void OnClickCloseBtn()
	{
		if (shopSelectTab != 0 || isIAPInit)
		{
			PlayPreBgm();
			base.OnClickCloseBtn();
		}
	}

	public void OnClickShowCost()
	{
		if (showJewelInfo)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
			{
				string p_msg = string.Format("{0}\n{1}:{2} {3}:{4}", MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIAMOND_OWNED"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIAMOND_PAY"), ManagedSingleton<PlayerHelper>.Instance.GetPaidJewel().ToString(), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DIAMOND_FREE"), ManagedSingleton<PlayerHelper>.Instance.GetFreeJewel().ToString());
				ui.Delay = 2f;
				ui.Setup(p_msg, true);
			});
		}
	}

	private void PlayPreBgm()
	{
		isStop = true;
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(m_preSceneBgm[0], m_preSceneBgm[1]);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LIBRARY_UPDATE_MAIN_UI);
	}
}
