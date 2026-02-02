using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using enums;

public class ExtendDataHelper : ManagedSingleton<ExtendDataHelper>
{
	private enum BANNER_DISABLETYPE
	{
		DEFAULT = 0,
		SHOP_RECORD = 1,
		GACHA_RECORD = 2
	}

	private Dictionary<int, EVENT_TABLE> dicCombinedEventData = new Dictionary<int, EVENT_TABLE>();

	private Dictionary<int, GACHALIST_TABLE> dicCombinedGachaListData = new Dictionary<int, GACHALIST_TABLE>();

	private Dictionary<int, GACHA_TABLE> dicCombinedGachaInfoData = new Dictionary<int, GACHA_TABLE>();

	private Dictionary<int, SHOP_TABLE> dicCombinedShopInfoData = new Dictionary<int, SHOP_TABLE>();

	private Dictionary<int, ITEM_TABLE> dicCombinedItemInfoData = new Dictionary<int, ITEM_TABLE>();

	private Dictionary<int, BANNER_TABLE> dicCombinedBannerInfoData = new Dictionary<int, BANNER_TABLE>();

	private bool netEventConverted;

	private bool netGachaEventConverted;

	private bool netGachaInfoConverted;

	private bool netShopInfoConverted;

	private bool netItemInfoConverted;

	private bool netBannerInfoConverted;

	public Dictionary<int, EVENT_TABLE> EVENT_TABLE_DICT
	{
		get
		{
			ConvertEventData();
			return dicCombinedEventData;
		}
	}

	public Dictionary<int, GACHALIST_TABLE> GACHALIST_TABLE_DICT
	{
		get
		{
			ConvertGachaEventData();
			return dicCombinedGachaListData;
		}
	}

	public Dictionary<int, GACHA_TABLE> GACHA_TABLE_DICT
	{
		get
		{
			ConvertGachaInfoData();
			return dicCombinedGachaInfoData;
		}
	}

	public Dictionary<int, SHOP_TABLE> SHOP_TABLE_DICT
	{
		get
		{
			ConvertShopInfoData();
			return dicCombinedShopInfoData;
		}
	}

	public Dictionary<int, ITEM_TABLE> ITEM_TABLE_DICT
	{
		get
		{
			ConvertItemInfoData();
			return dicCombinedItemInfoData;
		}
	}

	public Dictionary<int, BANNER_TABLE> BANNER_TABLE_DICT
	{
		get
		{
			ConvertBannerInfoData();
			return dicCombinedBannerInfoData;
		}
	}

	public override void Initialize()
	{
		Reset();
	}

	public override void Dispose()
	{
		Reset();
	}

	public override void Reset()
	{
		base.Reset();
		netEventConverted = false;
		dicCombinedEventData.Clear();
		netGachaEventConverted = false;
		dicCombinedGachaListData.Clear();
		netGachaInfoConverted = false;
		dicCombinedGachaInfoData.Clear();
		netShopInfoConverted = false;
		dicCombinedShopInfoData.Clear();
		netItemInfoConverted = false;
		dicCombinedItemInfoData.Clear();
		netBannerInfoConverted = false;
		dicCombinedBannerInfoData.Clear();
	}

	private void ConvertNetDataToExcelDataFormat(object src, object tar)
	{
		PropertyInfo[] properties = tar.GetType().GetProperties();
		FieldInfo[] fields = src.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (fieldInfo.Name == propertyInfo.Name)
				{
					propertyInfo.SetValue(tar, fieldInfo.GetValue(src), null);
					break;
				}
			}
		}
	}

	private void ConvertEventData()
	{
		if (netEventConverted)
		{
			return;
		}
		netEventConverted = true;
		dicCombinedEventData.Clear();
		foreach (EventExInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicEventEx.Values)
		{
			EVENT_TABLE eVENT_TABLE = CloneNetEventDataToEventTable(value.netEventExInfo);
			dicCombinedEventData.Add(eVENT_TABLE.n_ID, eVENT_TABLE);
		}
		foreach (EVENT_TABLE value2 in ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.Values)
		{
			if (!dicCombinedEventData.ContainsKey(value2.n_ID))
			{
				dicCombinedEventData.Add(value2.n_ID, value2);
			}
		}
		Region region = Region.All;
		region = Region.ASIA;
		dicCombinedEventData = dicCombinedEventData.Where((KeyValuePair<int, EVENT_TABLE> x) => ((uint)x.Value.n_AREA_VERSION & (uint)region) != 0).ToDictionary((KeyValuePair<int, EVENT_TABLE> x) => x.Key, (KeyValuePair<int, EVENT_TABLE> x) => x.Value);
	}

	private EVENT_TABLE CloneNetEventDataToEventTable(NetEventEx netInfo)
	{
		return new EVENT_TABLE
		{
			n_ID = netInfo.n_ID,
			n_TYPE = netInfo.n_TYPE,
			n_TYPE_X = netInfo.n_TYPE_X,
			n_TYPE_Y = netInfo.n_TYPE_Y,
			s_IMG = netInfo.s_IMG,
			n_BONUS_TYPE = netInfo.n_BONUS_TYPE,
			n_BONUS_RATE = netInfo.n_BONUS_RATE,
			n_SP_TYPE = netInfo.n_SP_TYPE,
			n_SP_ID = netInfo.n_SP_ID,
			n_DROP_ITEM = netInfo.n_DROP_ITEM,
			n_DROP_RATE = netInfo.n_DROP_RATE,
			n_COUNTER = netInfo.n_COUNTER,
			n_LIMIT = netInfo.n_LIMIT,
			n_RESET_RULE = netInfo.n_RESET_RULE,
			n_RANKING = netInfo.n_RANKING,
			n_POINT = netInfo.n_POINT,
			n_BOXGACHA = netInfo.n_BOXGACHA,
			n_RESULT = netInfo.n_RESULT,
			n_SHOP = netInfo.n_SHOP,
			n_MISSION = netInfo.n_MISSION,
			n_HOMETOP = netInfo.n_HOMETOP,
			w_NAME = netInfo.w_NAME,
			s_BEGIN_TIME = netInfo.s_BEGIN_TIME,
			s_END_TIME = netInfo.s_END_TIME,
			s_RANKING_TIME = netInfo.s_RANKING_TIME,
			s_REMAIN_TIME = netInfo.s_REMAIN_TIME
		};
	}

	private void ConvertGachaEventData()
	{
		if (netGachaEventConverted)
		{
			return;
		}
		netGachaEventConverted = true;
		dicCombinedGachaListData.Clear();
		foreach (GachaEventExInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicGachaEventEx.Values)
		{
			GACHALIST_TABLE gACHALIST_TABLE = CloneNetGachaEventDataToGachaListTable(value.netGachaEventExInfo);
			dicCombinedGachaListData.Add(gACHALIST_TABLE.n_ID, gACHALIST_TABLE);
		}
		foreach (GACHALIST_TABLE value2 in ManagedSingleton<OrangeDataManager>.Instance.GACHALIST_TABLE_DICT.Values)
		{
			if (!dicCombinedGachaListData.ContainsKey(value2.n_ID))
			{
				dicCombinedGachaListData.Add(value2.n_ID, value2);
			}
		}
	}

	private GACHALIST_TABLE CloneNetGachaEventDataToGachaListTable(NetGachaEventInfoEx netInfo)
	{
		return new GACHALIST_TABLE
		{
			n_ID = netInfo.n_ID,
			s_NAME = netInfo.s_NAME,
			n_TYPE = netInfo.n_TYPE,
			n_GROUP = netInfo.n_GROUP,
			n_SORT = netInfo.n_SORT,
			s_IMG = netInfo.s_IMG,
			n_COIN_ID = netInfo.n_COIN_ID,
			n_COIN_MOUNT = netInfo.n_COIN_MOUNT,
			n_LIMIT = netInfo.n_LIMIT,
			n_RESET_RULE = netInfo.n_RESET_RULE,
			n_PRE = netInfo.n_PRE,
			n_SHOWTYPE = netInfo.n_SHOWTYPE,
			n_RANKING_BONUS = netInfo.n_RANKING_BONUS,
			n_BONUS = netInfo.n_BONUS,
			n_BONUS_COUNT = netInfo.n_BONUS_COUNT,
			n_SHOPID = netInfo.n_SHOPID,
			n_GACHAID_1 = netInfo.n_GACHAID_1,
			n_GACHACOUNT_1 = netInfo.n_GACHACOUNT_1,
			n_GACHAID_2 = netInfo.n_GACHAID_2,
			n_GACHACOUNT_2 = netInfo.n_GACHACOUNT_2,
			n_LUCKY = netInfo.n_LUCKY,
			n_LUCKY_GACHA = netInfo.n_LUCKY_GACHA,
			n_BUTTON_IMG = netInfo.n_BUTTON_IMG,
			n_SHOWRATE = netInfo.n_SHOWRATE,
			n_PERFORM = netInfo.n_PERFORM,
			w_BUTTON_TEXT = netInfo.w_BUTTON_TEXT,
			w_BUTTON_TIP = netInfo.w_BUTTON_TIP,
			w_CONFIRM_TIP = netInfo.w_CONFIRM_TIP,
			s_BEGIN_TIME = netInfo.s_BEGIN_TIME,
			s_END_TIME = netInfo.s_END_TIME
		};
	}

	private void ConvertGachaInfoData()
	{
		if (netGachaInfoConverted)
		{
			return;
		}
		netGachaInfoConverted = true;
		dicCombinedGachaInfoData.Clear();
		foreach (GachaExInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicGachaEx.Values)
		{
			GACHA_TABLE gACHA_TABLE = CloneNetGachaDataToGachaTable(value.netGachaExInfo);
			dicCombinedGachaInfoData.Add(gACHA_TABLE.n_ID, gACHA_TABLE);
		}
		foreach (GACHA_TABLE value2 in ManagedSingleton<OrangeDataManager>.Instance.GACHA_TABLE_DICT.Values)
		{
			if (!dicCombinedGachaInfoData.ContainsKey(value2.n_ID))
			{
				dicCombinedGachaInfoData.Add(value2.n_ID, value2);
			}
		}
	}

	private GACHA_TABLE CloneNetGachaDataToGachaTable(NetGachaInfoEx netInfo)
	{
		return new GACHA_TABLE
		{
			n_ID = netInfo.n_ID,
			n_GROUP = netInfo.n_GROUP,
			n_TYPE = netInfo.n_TYPE,
			n_RANK = netInfo.n_RANK,
			n_REWARD_TYPE = netInfo.n_REWARD_TYPE,
			n_REWARD_ID = netInfo.n_REWARD_ID,
			n_AMOUNT_MIN = netInfo.n_AMOUNT_MIN,
			n_AMOUNT_MAX = netInfo.n_AMOUNT_MAX,
			n_PICKUP = netInfo.n_PICKUP,
			n_VALUE = netInfo.n_VALUE
		};
	}

	private void ConvertShopInfoData()
	{
		if (netShopInfoConverted)
		{
			return;
		}
		netShopInfoConverted = true;
		dicCombinedShopInfoData.Clear();
		foreach (ShopExInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicShopEx.Values)
		{
			SHOP_TABLE sHOP_TABLE = CloneNetShopDataToShopTable(value.netShopExInfo);
			dicCombinedShopInfoData.Add(sHOP_TABLE.n_ID, sHOP_TABLE);
		}
		foreach (SHOP_TABLE value2 in ManagedSingleton<OrangeDataManager>.Instance.SHOP_TABLE_DICT.Values)
		{
			if (!dicCombinedShopInfoData.ContainsKey(value2.n_ID))
			{
				dicCombinedShopInfoData.Add(value2.n_ID, value2);
			}
		}
		Region region = Region.All;
		region = Region.ASIA;
		dicCombinedShopInfoData = dicCombinedShopInfoData.Where((KeyValuePair<int, SHOP_TABLE> x) => ((uint)x.Value.n_AREA_VERSION & (uint)region) != 0).ToDictionary((KeyValuePair<int, SHOP_TABLE> x) => x.Key, (KeyValuePair<int, SHOP_TABLE> x) => x.Value);
	}

	private SHOP_TABLE CloneNetShopDataToShopTable(NetShopInfoEx netInfo)
	{
		return new SHOP_TABLE
		{
			n_ID = netInfo.n_ID,
			n_MAIN_TYPE = netInfo.n_MAIN_TYPE,
			n_SUB_TYPE = netInfo.n_SUB_TYPE,
			w_SHEET_NAME = netInfo.w_SHEET_NAME,
			n_SORT = netInfo.n_SORT,
			n_TAG = netInfo.n_TAG,
			s_ICON = netInfo.s_ICON,
			n_PLATFORM = netInfo.n_PLATFORM,
			s_PRODUCT_ID = netInfo.s_PRODUCT_ID,
			n_PRODUCT_TYPE = netInfo.n_PRODUCT_TYPE,
			n_AUTO_OPEN = netInfo.n_AUTO_OPEN,
			n_PRODUCT_ID = netInfo.n_PRODUCT_ID,
			n_PRODUCT_MOUNT = netInfo.n_PRODUCT_MOUNT,
			n_VIP = netInfo.n_VIP,
			n_LIMIT = netInfo.n_LIMIT,
			n_RESET_RULE = netInfo.n_RESET_RULE,
			n_PRE = netInfo.n_PRE,
			n_COIN_ID = netInfo.n_COIN_ID,
			n_COIN_MOUNT = netInfo.n_COIN_MOUNT,
			n_DISCOUNT = netInfo.n_DISCOUNT,
			n_BONUS_COIN = netInfo.n_BONUS_COIN,
			s_BEGIN_TIME = netInfo.s_BEGIN_TIME,
			s_END_TIME = netInfo.s_END_TIME,
			w_PRODUCT_NAME = netInfo.w_PRODUCT_NAME,
			w_PRODUCT_TIP = netInfo.w_PRODUCT_TIP
		};
	}

	private void ConvertItemInfoData()
	{
		if (netItemInfoConverted)
		{
			return;
		}
		netItemInfoConverted = true;
		dicCombinedItemInfoData.Clear();
		foreach (ItemExInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicItemEx.Values)
		{
			ITEM_TABLE iTEM_TABLE = CloneNetItemDataToItemTable(value.netItemExInfo);
			dicCombinedItemInfoData.Add(iTEM_TABLE.n_ID, iTEM_TABLE);
		}
		foreach (ITEM_TABLE value2 in ManagedSingleton<OrangeDataManager>.Instance.ITEM_TABLE_DICT.Values)
		{
			if (!dicCombinedItemInfoData.ContainsKey(value2.n_ID))
			{
				dicCombinedItemInfoData.Add(value2.n_ID, value2);
			}
		}
	}

	private ITEM_TABLE CloneNetItemDataToItemTable(NetItemInfoEx netInfo)
	{
		return new ITEM_TABLE
		{
			n_ID = netInfo.n_ID,
			n_TYPE = netInfo.n_TYPE,
			n_TYPE_X = netInfo.n_TYPE_X,
			s_ICON = netInfo.s_ICON,
			n_RARE = netInfo.n_RARE,
			f_VALUE_X = netInfo.f_VALUE_X,
			f_VALUE_Y = netInfo.f_VALUE_Y,
			n_SELL_ID = netInfo.n_SELL_ID,
			n_SELL_COUNT = netInfo.n_SELL_COUNT,
			n_MAX = netInfo.n_MAX,
			n_FAKEITEM = netInfo.n_FAKEITEM,
			s_HOWTOGET = netInfo.s_HOWTOGET,
			w_NAME = netInfo.w_NAME,
			w_TIP = netInfo.w_TIP
		};
	}

	private void ConvertBannerInfoData()
	{
		if (netBannerInfoConverted)
		{
			return;
		}
		netBannerInfoConverted = true;
		dicCombinedBannerInfoData.Clear();
		foreach (BannerExInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicBannerEx.Values)
		{
			BANNER_TABLE bANNER_TABLE = CloneNetBannerDataToItemTable(value.netBannerExInfo);
			dicCombinedBannerInfoData.Add(bANNER_TABLE.n_ID, bANNER_TABLE);
		}
		foreach (BANNER_TABLE value2 in ManagedSingleton<OrangeDataManager>.Instance.BANNER_TABLE_DICT.Values)
		{
			if (!dicCombinedBannerInfoData.ContainsKey(value2.n_ID))
			{
				dicCombinedBannerInfoData.Add(value2.n_ID, value2);
			}
		}
	}

	private BANNER_TABLE CloneNetBannerDataToItemTable(NetBannerEx netInfo)
	{
		return new BANNER_TABLE
		{
			n_ID = netInfo.n_ID,
			n_SORT = netInfo.n_SORT,
			s_IMG = netInfo.s_IMG,
			n_UILINK = netInfo.n_UILINK,
			s_URL = netInfo.s_URL,
			n_DISABLETYPE = netInfo.n_DISABLETYPE,
			n_DISABLETYPE_X = netInfo.n_DISABLETYPE_X,
			s_BEGIN_TIME = netInfo.s_BEGIN_TIME,
			s_END_TIME = netInfo.s_END_TIME
		};
	}

	public List<EVENT_TABLE> GetEventTableByType(EventType p_eventType, long now)
	{
		int currentType = (int)p_eventType;
		bool bHasNewPlayerMission = ManagedSingleton<MissionHelper>.Instance.CheckHasNewPlayerEvent();
		return EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == currentType && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, x.s_END_TIME, now) && (x.n_TYPE_X != 15 || bHasNewPlayerMission)).ToList();
	}

	public List<EVENT_TABLE> GetEventTableRemainByType(EventType p_eventType, long now)
	{
		int currentType = (int)p_eventType;
		return EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == currentType && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_END_TIME, (x.s_REMAIN_TIME == "null") ? x.s_END_TIME : x.s_REMAIN_TIME, now)).ToList();
	}

	public EVENT_TABLE GetEventTableByCounter(int n_COUNTER)
	{
		return EVENT_TABLE_DICT.Values.FirstOrDefault((EVENT_TABLE x) => x.n_COUNTER == n_COUNTER);
	}

	public List<GACHA_TABLE> GetListGachaByGroup(int group)
	{
		return GACHA_TABLE_DICT.Values.Where((GACHA_TABLE x) => x.n_GROUP == group).ToList();
	}

	public bool GetFirstGachaTableByGroup(int group, out GACHA_TABLE gachaTable)
	{
		gachaTable = GACHA_TABLE_DICT.Values.FirstOrDefault((GACHA_TABLE x) => x.n_GROUP == group);
		return gachaTable != null;
	}

	public bool IsCeilingExist(int gachaId, out List<GACHA_TABLE> listExchange)
	{
		int findType = 3;
		List<GACHA_TABLE> list = null;
		list = GACHA_TABLE_DICT.Values.Where((GACHA_TABLE x) => x.n_GROUP == gachaId && x.n_TYPE == findType).ToList();
		listExchange = list;
		if (listExchange != null)
		{
			return listExchange.Count > 0;
		}
		return false;
	}

	public bool IsCeilingExist(int gachaId)
	{
		int findType = 3;
		return GACHA_TABLE_DICT.Values.Where((GACHA_TABLE x) => x.n_GROUP == gachaId).FirstOrDefault((GACHA_TABLE x) => x.n_TYPE == findType) != null;
	}

	public int GetGachaDrawCount(List<GACHALIST_TABLE> gachaList)
	{
		int num = 0;
		int num2 = 0;
		foreach (GACHALIST_TABLE gacha in gachaList)
		{
			GachaInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicGacha.TryGetValue(gacha.n_ID, out value) && value.netGachaEventRecord != null)
			{
				num += value.netGachaEventRecord.SetupDrawCount;
				num2 += value.netGachaEventRecord.TotalDrawCount;
			}
		}
		return num2 - num * OrangeConst.GACHA_SELECT_MAX;
	}

	public List<GACHALIST_TABLE> GetGachaListTableByOepening(long now, int n_type)
	{
		return GACHALIST_TABLE_DICT.Values.Where((GACHALIST_TABLE x) => x.n_TYPE == n_type && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, x.s_END_TIME, now) && ConfirmGachaVisible(x)).ToList();
	}

	private bool ConfirmGachaVisible(GACHALIST_TABLE gachaListTable)
	{
		if (gachaListTable.n_SHOWTYPE == 1)
		{
			return ManagedSingleton<PlayerHelper>.Instance.GetItemValue(gachaListTable.n_COIN_ID) > 0;
		}
		return true;
	}

	public List<SHOP_TABLE> GetShopTableByOpening(long now)
	{
		int storeType = 2;
		storeType = 32;
		return (from x in SHOP_TABLE_DICT.Values
			where ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, x.s_END_TIME, now)
			where x.n_PLATFORM == 0 || (x.n_PLATFORM & storeType) == storeType
			orderby x.n_SORT, x.n_ID
			select x).ToList();
	}

	public IEnumerable<SHOP_TABLE> GetIEnumerableShopTableByOpening(long now)
	{
		return (from x in SHOP_TABLE_DICT.Values
			where ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, x.s_END_TIME, now)
			orderby x.n_SORT, x.n_ID
			select x).ToList();
	}

	public List<SHOP_TABLE> GetShopListByIAP(IAPStoreType store)
	{
		int confirmType = (int)store;
		return (from x in GetIEnumerableShopTableByOpening(MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
			where x.n_COIN_ID == 0 && (x.n_PLATFORM & confirmType) == confirmType
			select x).ToList();
	}

	public List<BANNER_TABLE> GetBannerListByOpening()
	{
		long now = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		return (from x in BANNER_TABLE_DICT.Values
			where ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, x.s_END_TIME, now) && ConfirmBannerVisible(x)
			orderby x.n_SORT, x.n_ID
			select x).ToList();
	}

	private bool ShowBannerByShopRecord(SHOP_TABLE shopTable)
	{
		ShopInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicShop.TryGetValue(shopTable.n_ID, out value) && value.netShopRecord != null)
		{
			return MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(value.netShopRecord.LastShopTime, (ResetRule)shopTable.n_RESET_RULE);
		}
		return true;
	}

	private bool ShowBannerByGachaRecord(GACHALIST_TABLE table)
	{
		GachaInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicGacha.TryGetValue(table.n_ID, out value) && value.netGachaEventRecord != null)
		{
			return MonoBehaviourSingleton<OrangeGameManager>.Instance.IsPassedResetDate(value.netGachaEventRecord.LastDrawTime, (ResetRule)table.n_RESET_RULE);
		}
		return true;
	}

	public bool ConfirmBannerVisible(BANNER_TABLE banner)
	{
		switch ((BANNER_DISABLETYPE)banner.n_DISABLETYPE)
		{
		default:
			return true;
		case BANNER_DISABLETYPE.SHOP_RECORD:
		{
			SHOP_TABLE value2 = null;
			if (SHOP_TABLE_DICT.TryGetValue(banner.n_DISABLETYPE_X, out value2))
			{
				return ShowBannerByShopRecord(value2);
			}
			break;
		}
		case BANNER_DISABLETYPE.GACHA_RECORD:
		{
			GACHALIST_TABLE value = null;
			if (GACHALIST_TABLE_DICT.TryGetValue(banner.n_DISABLETYPE_X, out value))
			{
				return ShowBannerByGachaRecord(value);
			}
			break;
		}
		}
		return true;
	}
}
