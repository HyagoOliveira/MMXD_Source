using System;
using System.Collections.Generic;
using System.Linq;
using Better;
using enums;

public class EquipHelper : ManagedSingleton<EquipHelper>
{
	public enum WEAPON_SORT_KEY
	{
		NONE = 0,
		WEAPON_SORT_RARITY = 1,
		WEAPON_SORT_STAR = 2,
		WEAPON_SORT_LV = 4,
		WEAPON_SORT_UPGRADE = 8,
		WEAPON_SORT_FAVORITE = 16,
		WEAPON_SORT_MAX = 255
	}

	public enum WEAPON_GET_TYPE
	{
		NONE = 0,
		WEAPON_GETED = 1,
		WEAPON_FRAGS = 2,
		WEAPON_ALL = 4,
		WEAPON_MAX = 255
	}

	public enum CARD_SORT_KEY
	{
		NONE = 0,
		CARD_SORT_RARITY = 1,
		CARD_SORT_STAR = 2,
		CARD_SORT_LV = 4,
		CARD_SORT_GET_TIME = 8,
		CARD_SORT_FAVORITE = 16,
		CARD_SORT_EXCLUSIVE = 32,
		CARDN_SORT_MAX = 255
	}

	public enum CARD_GET_TYPE
	{
		NONE = 0,
		CARD_GETED = 1,
		CARD_FRAGS = 2,
		CARD_ALL = 4,
		CARD_MAX = 255
	}

	public enum SIGN_SORT_KEY
	{
		NONE = 0,
		SIGN_SORT_RARITY = 1,
		SIGN_SORT_GET_TIME = 2,
		SIGN_SORT_ID = 4,
		SIGN_SORT_MAX = 255
	}

	public enum SIGN_GET_TYPE
	{
		NONE = 0,
		SIGN_GETED = 1,
		SIGN_FRAGS = 2,
		SIGN_ALL = 4,
		SIGN_MAX = 255
	}

	public List<int> listCharacterCompelled = new List<int>();

	public List<int> listWeaponCompelled = new List<int>();

	public List<int> listChipCompelled = new List<int>();

	private List<int> listHasWeapons = new List<int>();

	private List<int> listFragWeapons = new List<int>();

	private List<int> listHasChips = new List<int>();

	private List<int> listFragChips = new List<int>();

	private System.Collections.Generic.Dictionary<int, int> dicCharacterEquipSeqID = new System.Collections.Generic.Dictionary<int, int>();

	public List<int> listSignCompelled = new List<int>();

	public WeaponType nWeaponSortType
	{
		get
		{
			return (WeaponType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastWeaponSortType;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastWeaponSortType = (int)value;
		}
	}

	public WEAPON_SORT_KEY nWeaponSortKey
	{
		get
		{
			return (WEAPON_SORT_KEY)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastWeaponSortKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastWeaponSortKey = (int)value;
		}
	}

	public WEAPON_GET_TYPE nWeaponGetKey
	{
		get
		{
			return (WEAPON_GET_TYPE)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastWeaponSortStatus;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastWeaponSortStatus = (int)value;
		}
	}

	public int WeaponSortDescend
	{
		get
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastWeaponSortDescend)
			{
				return 0;
			}
			return 1;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastWeaponSortDescend = value == 1;
		}
	}

	public WeaponType nChipSortType
	{
		get
		{
			return (WeaponType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChipSortType;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChipSortType = (int)value;
		}
	}

	public WEAPON_SORT_KEY nChipSortKey
	{
		get
		{
			return (WEAPON_SORT_KEY)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChipSortKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChipSortKey = (int)value;
		}
	}

	public WEAPON_GET_TYPE nChipGetKey
	{
		get
		{
			return (WEAPON_GET_TYPE)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChipGetKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChipGetKey = (int)value;
		}
	}

	public bool bChipSortDescend
	{
		get
		{
			return MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChipSortDescend;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChipSortDescend = value;
		}
	}

	public CardColorType nCardSortType
	{
		get
		{
			return (CardColorType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardSortType;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardSortType = (int)value;
		}
	}

	public CardColorType nCardMainSortType
	{
		get
		{
			return (CardColorType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardMainSortType;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardMainSortType = (int)value;
		}
	}

	public CardColorType nCardInfoSortType
	{
		get
		{
			return (CardColorType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardInfoSortType;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardInfoSortType = (int)value;
		}
	}

	public CARD_SORT_KEY nCardSortKey
	{
		get
		{
			return (CARD_SORT_KEY)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardSortKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardSortKey = (int)value;
		}
	}

	public CARD_SORT_KEY nCardDeploySortKey
	{
		get
		{
			return (CARD_SORT_KEY)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardDeploySortKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardDeploySortKey = (int)value;
		}
	}

	public CARD_SORT_KEY nCardMainSortKey
	{
		get
		{
			return (CARD_SORT_KEY)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardMainSortKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardMainSortKey = (int)value;
		}
	}

	public CARD_SORT_KEY nCardInfoSortKey
	{
		get
		{
			return (CARD_SORT_KEY)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardInfoSortKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardInfoSortKey = (int)value;
		}
	}

	public int CardSortDescend
	{
		get
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardSortDescend)
			{
				return 0;
			}
			return 1;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardSortDescend = value == 1;
		}
	}

	public int CardDeploySortDescend
	{
		get
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardDeploySortDescend)
			{
				return 0;
			}
			return 1;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardDeploySortDescend = value == 1;
		}
	}

	public int CardMainSortDescend
	{
		get
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardMainSortDescend)
			{
				return 0;
			}
			return 1;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardMainSortDescend = value == 1;
		}
	}

	public int CardInfoSortDescend
	{
		get
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardInfoSortDescend)
			{
				return 0;
			}
			return 1;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardInfoSortDescend = value == 1;
		}
	}

	public CardColorType nCardResetSortType
	{
		get
		{
			return (CardColorType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardResetSortType;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardResetSortType = (int)value;
		}
	}

	public CARD_SORT_KEY nCardResetSortKey
	{
		get
		{
			return (CARD_SORT_KEY)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardResetSortKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCardResetSortKey = (int)value;
		}
	}

	public SIGN_SORT_KEY nSignSortKey
	{
		get
		{
			return (SIGN_SORT_KEY)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastSignSortKey;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastSignSortKey = (int)value;
		}
	}

	public SIGN_GET_TYPE nSignGetKey
	{
		get
		{
			return (SIGN_GET_TYPE)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastSignSortStatus;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastSignSortStatus = (int)value;
		}
	}

	public int SignSortDescend
	{
		get
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastSignSortDescend)
			{
				return 0;
			}
			return 1;
		}
		set
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastSignSortDescend = value == 1;
		}
	}

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	private int ValueToStars(float val)
	{
		if (val > 0.85f)
		{
			return 3;
		}
		if (val >= 0.65f)
		{
			return 2;
		}
		if (val >= 0.4f)
		{
			return 1;
		}
		return 0;
	}

	public bool bIsShowGetedWeapon()
	{
		if ((nWeaponGetKey & WEAPON_GET_TYPE.WEAPON_ALL) == 0)
		{
			return (nWeaponGetKey & WEAPON_GET_TYPE.WEAPON_GETED) != 0;
		}
		return true;
	}

	public bool bIsShowFragWeapon()
	{
		return (nWeaponGetKey & WEAPON_GET_TYPE.WEAPON_FRAGS) != 0;
	}

	public bool bIsShowAllWeapon()
	{
		return (nWeaponGetKey & WEAPON_GET_TYPE.WEAPON_ALL) != 0;
	}

	public bool bIsShowGetedChip()
	{
		if ((nChipGetKey & WEAPON_GET_TYPE.WEAPON_ALL) == 0)
		{
			return (nChipGetKey & WEAPON_GET_TYPE.WEAPON_GETED) != 0;
		}
		return true;
	}

	public bool bIsShowFragChip()
	{
		return (nChipGetKey & WEAPON_GET_TYPE.WEAPON_FRAGS) != 0;
	}

	public bool bIsShowAllChip()
	{
		return (nChipGetKey & WEAPON_GET_TYPE.WEAPON_ALL) != 0;
	}

	public List<int> GetUnlockedWeaponList()
	{
		return listHasWeapons;
	}

	public List<int> GetFragmentWeaponList()
	{
		return listFragWeapons;
	}

	public void SortWeaponListForGoCheck()
	{
		WeaponType weaponType = nWeaponSortType;
		WEAPON_GET_TYPE wEAPON_GET_TYPE = nWeaponGetKey;
		nWeaponSortType = WeaponType.All;
		nWeaponGetKey = WEAPON_GET_TYPE.WEAPON_GETED;
		SortWeaponList();
		nWeaponSortType = weaponType;
		nWeaponGetKey = wEAPON_GET_TYPE;
	}

	public void SortWeaponList()
	{
		listHasWeapons.Clear();
		listFragWeapons.Clear();
		System.Collections.Generic.Dictionary<int, WEAPON_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.GetEnumerator();
		List<WeaponInfo> list = new List<WeaponInfo>();
		List<int> list2 = new List<int>();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.n_ENABLE_FLAG == 0 || ((uint)enumerator.Current.Value.n_TYPE & (uint)nWeaponSortType) == 0)
			{
				continue;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(enumerator.Current.Key))
			{
				listHasWeapons.Add(enumerator.Current.Key);
				list.Add(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[enumerator.Current.Key]);
			}
			else if (bIsShowAllWeapon())
			{
				if (ManagedSingleton<PlayerHelper>.Instance.GetItemValue(enumerator.Current.Value.n_UNLOCK_ID) >= enumerator.Current.Value.n_UNLOCK_COUNT)
				{
					list2.Add(enumerator.Current.Key);
				}
				else
				{
					listFragWeapons.Add(enumerator.Current.Key);
				}
			}
			else if (bIsShowFragWeapon())
			{
				int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(enumerator.Current.Value.n_UNLOCK_ID);
				if (itemValue >= enumerator.Current.Value.n_UNLOCK_COUNT)
				{
					list2.Add(enumerator.Current.Key);
				}
				else if (itemValue > 0)
				{
					listFragWeapons.Add(enumerator.Current.Key);
				}
			}
		}
		WeaponInfo[] array = SortWeaponData(list).ToArray();
		listHasWeapons.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			listHasWeapons.Add(array[i].netInfo.WeaponID);
		}
		list.Clear();
		foreach (int listFragWeapon in listFragWeapons)
		{
			WeaponInfo weaponInfo = new WeaponInfo();
			weaponInfo.netInfo = new NetWeaponInfo();
			weaponInfo.netInfo.WeaponID = listFragWeapon;
			list.Add(weaponInfo);
		}
		array = SortWeaponData(list).ToArray();
		listFragWeapons.Clear();
		for (int j = 0; j < array.Length; j++)
		{
			listFragWeapons.Add(array[j].netInfo.WeaponID);
		}
		list.Clear();
		foreach (int item in list2)
		{
			WeaponInfo weaponInfo2 = new WeaponInfo();
			weaponInfo2.netInfo = new NetWeaponInfo();
			weaponInfo2.netInfo.WeaponID = item;
			list.Add(weaponInfo2);
		}
		array = SortWeaponData(list).ToArray();
		list2.Clear();
		for (int k = 0; k < array.Length; k++)
		{
			list2.Add(array[k].netInfo.WeaponID);
		}
		if (listWeaponCompelled.Count > 0)
		{
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID != 0)
			{
				listHasWeapons.Remove(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID);
				listHasWeapons.Insert(0, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID);
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID != 0)
			{
				listHasWeapons.Remove(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID);
				listHasWeapons.Insert(1, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID);
			}
		}
		for (int l = 0; l < listWeaponCompelled.Count; l++)
		{
			listHasWeapons.Remove(listWeaponCompelled[l]);
			listFragWeapons.Remove(listWeaponCompelled[l]);
			list2.Remove(listWeaponCompelled[l]);
		}
		for (int m = 0; m < list2.Count; m++)
		{
			if (Convert.ToBoolean(WeaponSortDescend))
			{
				listHasWeapons.Insert(m, list2[m]);
			}
			else
			{
				listFragWeapons.Insert(m, list2[m]);
			}
		}
		for (int n = 0; n < listWeaponCompelled.Count; n++)
		{
			listHasWeapons.Insert(n, listWeaponCompelled[n]);
		}
		listWeaponCompelled.Clear();
	}

	private IOrderedEnumerable<WeaponInfo> SortWeaponData(List<WeaponInfo> selectweapons)
	{
		IOrderedEnumerable<WeaponInfo> source = null;
		bool flag = Convert.ToBoolean(WeaponSortDescend);
		if ((nWeaponSortKey & WEAPON_SORT_KEY.WEAPON_SORT_RARITY) != 0)
		{
			source = ((!flag) ? selectweapons.OrderBy((WeaponInfo obj) => ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[obj.netInfo.WeaponID].n_RARITY) : selectweapons.OrderByDescending((WeaponInfo obj) => ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[obj.netInfo.WeaponID].n_RARITY));
		}
		else if ((nWeaponSortKey & WEAPON_SORT_KEY.WEAPON_SORT_STAR) != 0)
		{
			source = ((!flag) ? selectweapons.OrderBy((WeaponInfo obj) => obj.netInfo.Star) : selectweapons.OrderByDescending((WeaponInfo obj) => obj.netInfo.Star));
		}
		else if ((nWeaponSortKey & WEAPON_SORT_KEY.WEAPON_SORT_LV) != 0)
		{
			source = ((!flag) ? selectweapons.OrderBy((WeaponInfo obj) => obj.netInfo.Exp) : selectweapons.OrderByDescending((WeaponInfo obj) => obj.netInfo.Exp));
		}
		else if ((nWeaponSortKey & WEAPON_SORT_KEY.WEAPON_SORT_UPGRADE) != 0)
		{
			source = ((!flag) ? selectweapons.OrderBy(delegate(WeaponInfo obj)
			{
				int num2 = 0;
				if (obj.netExpertInfos != null)
				{
					for (int j = 0; j < obj.netExpertInfos.Count; j++)
					{
						num2 += obj.netExpertInfos[j].ExpertLevel;
					}
				}
				return num2;
			}) : selectweapons.OrderByDescending(delegate(WeaponInfo obj)
			{
				int num = 0;
				if (obj.netExpertInfos != null)
				{
					for (int i = 0; i < obj.netExpertInfos.Count; i++)
					{
						num += obj.netExpertInfos[i].ExpertLevel;
					}
				}
				return num;
			}));
		}
		else if ((nWeaponSortKey & WEAPON_SORT_KEY.WEAPON_SORT_FAVORITE) != 0)
		{
			source = ((!flag) ? selectweapons.OrderBy((WeaponInfo obj) => obj.netInfo.Favorite) : selectweapons.OrderByDescending((WeaponInfo obj) => obj.netInfo.Favorite));
		}
		if (flag)
		{
			return source.ThenBy((WeaponInfo obj) => obj.netInfo.WeaponID);
		}
		return source.ThenByDescending((WeaponInfo obj) => obj.netInfo.WeaponID);
	}

	public WeaponInfo GetSortedWeaponInfo(int index)
	{
		WeaponInfo value = new WeaponInfo();
		value.netInfo = new NetWeaponInfo();
		value.netExpertInfos = new List<NetWeaponExpertInfo>();
		value.netSkillInfos = new List<NetWeaponSkillInfo>();
		List<int> unlockedWeaponList = GetUnlockedWeaponList();
		List<int> fragmentWeaponList = GetFragmentWeaponList();
		if (WeaponSortDescend == 1)
		{
			if (index >= unlockedWeaponList.Count)
			{
				value.netInfo.WeaponID = fragmentWeaponList[index - unlockedWeaponList.Count];
			}
			else
			{
				ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(unlockedWeaponList[index], out value);
			}
		}
		else if (index >= fragmentWeaponList.Count)
		{
			ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(unlockedWeaponList[index - fragmentWeaponList.Count], out value);
		}
		else
		{
			value.netInfo.WeaponID = fragmentWeaponList[index];
		}
		return value;
	}

	public bool IsAnyWeaponCanUpgradeStar()
	{
		System.Collections.Generic.Dictionary<int, WeaponInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (IsCanWeaponUpgradeStart(enumerator.Current.Value.netInfo))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCanWeaponUpgradeStart(NetWeaponInfo p_netWeapon)
	{
		if (p_netWeapon.Star >= 5)
		{
			return false;
		}
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(p_netWeapon.WeaponID))
		{
			return false;
		}
		IEnumerable<STAR_TABLE> source = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values.Where((STAR_TABLE obj) => (obj.n_TYPE == 2 && obj.n_MAINID == p_netWeapon.WeaponID && obj.n_STAR == p_netWeapon.Star) ? true : false);
		if (source.Count() == 0)
		{
			return false;
		}
		STAR_TABLE sTAR_TABLE = source.ElementAt(0);
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[sTAR_TABLE.n_MATERIAL];
		int[] array = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i];
			if (num != 0)
			{
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[num];
				int num2 = 0;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
				}
				if (num2 < array2[i])
				{
					return false;
				}
			}
		}
		return true;
	}

	public List<int> GetUnlockedChipList()
	{
		return listHasChips;
	}

	public List<int> GetFragmentChipList()
	{
		return listFragChips;
	}

	public void SortChipList()
	{
		listHasChips.Clear();
		listFragChips.Clear();
		System.Collections.Generic.Dictionary<int, DISC_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.GetEnumerator();
		List<ChipInfo> list = new List<ChipInfo>();
		List<int> list2 = new List<int>();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.n_ENABLE_FLAG == 0 || ((uint)enumerator.Current.Value.n_WEAPON_TYPE & (uint)nChipSortType) == 0)
			{
				continue;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(enumerator.Current.Key))
			{
				listHasChips.Add(enumerator.Current.Key);
				list.Add(ManagedSingleton<PlayerNetManager>.Instance.dicChip[enumerator.Current.Key]);
			}
			else if (bIsShowAllChip())
			{
				if (ManagedSingleton<PlayerHelper>.Instance.GetItemValue(enumerator.Current.Value.n_UNLOCK_ID) >= enumerator.Current.Value.n_UNLOCK_COUNT)
				{
					list2.Add(enumerator.Current.Key);
				}
				else
				{
					listFragChips.Add(enumerator.Current.Key);
				}
			}
			else if (bIsShowFragChip())
			{
				int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(enumerator.Current.Value.n_UNLOCK_ID);
				if (itemValue >= enumerator.Current.Value.n_UNLOCK_COUNT)
				{
					list2.Add(enumerator.Current.Key);
				}
				else if (itemValue > 0)
				{
					listFragChips.Add(enumerator.Current.Key);
				}
			}
		}
		ChipInfo[] array = SortChipData(list).ToArray();
		listHasChips.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			listHasChips.Add(array[i].netChipInfo.ChipID);
		}
		list.Clear();
		foreach (int listFragChip in listFragChips)
		{
			ChipInfo chipInfo = new ChipInfo();
			chipInfo.netChipInfo = new NetChipInfo();
			chipInfo.netChipInfo.ChipID = listFragChip;
			list.Add(chipInfo);
		}
		array = SortChipData(list).ToArray();
		listFragChips.Clear();
		for (int j = 0; j < array.Length; j++)
		{
			listFragChips.Add(array[j].netChipInfo.ChipID);
		}
		list.Clear();
		foreach (int item in list2)
		{
			ChipInfo chipInfo2 = new ChipInfo();
			chipInfo2.netChipInfo = new NetChipInfo();
			chipInfo2.netChipInfo.ChipID = item;
			list.Add(chipInfo2);
		}
		array = SortChipData(list).ToArray();
		list2.Clear();
		for (int k = 0; k < array.Length; k++)
		{
			list2.Add(array[k].netChipInfo.ChipID);
		}
		for (int l = 0; l < listChipCompelled.Count; l++)
		{
			listHasChips.Remove(listChipCompelled[l]);
			listFragChips.Remove(listChipCompelled[l]);
			list2.Remove(listChipCompelled[l]);
		}
		for (int m = 0; m < list2.Count; m++)
		{
			if (Convert.ToBoolean(bChipSortDescend))
			{
				listHasChips.Insert(m, list2[m]);
			}
			else
			{
				listFragChips.Insert(m, list2[m]);
			}
		}
		for (int n = 0; n < listChipCompelled.Count; n++)
		{
			listHasChips.Insert(n, listChipCompelled[n]);
		}
		listChipCompelled.Clear();
	}

	private IOrderedEnumerable<ChipInfo> SortChipData(List<ChipInfo> selectchips)
	{
		IOrderedEnumerable<ChipInfo> source = null;
		bool flag = Convert.ToBoolean(bChipSortDescend);
		if ((nChipSortKey & WEAPON_SORT_KEY.WEAPON_SORT_RARITY) != 0)
		{
			source = ((!flag) ? selectchips.OrderBy((ChipInfo obj) => ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[obj.netChipInfo.ChipID].n_RARITY) : selectchips.OrderByDescending((ChipInfo obj) => ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[obj.netChipInfo.ChipID].n_RARITY));
		}
		else if ((nChipSortKey & WEAPON_SORT_KEY.WEAPON_SORT_STAR) != 0)
		{
			source = ((!flag) ? selectchips.OrderBy((ChipInfo obj) => obj.netChipInfo.Star) : selectchips.OrderByDescending((ChipInfo obj) => obj.netChipInfo.Star));
		}
		else if ((nChipSortKey & WEAPON_SORT_KEY.WEAPON_SORT_LV) != 0)
		{
			source = ((!flag) ? selectchips.OrderBy((ChipInfo obj) => obj.netChipInfo.Exp) : selectchips.OrderByDescending((ChipInfo obj) => obj.netChipInfo.Exp));
		}
		else if ((nChipSortKey & WEAPON_SORT_KEY.WEAPON_SORT_UPGRADE) != 0)
		{
			source = ((!flag) ? selectchips.OrderBy((ChipInfo obj) => obj.netChipInfo.Analyse) : selectchips.OrderByDescending((ChipInfo obj) => obj.netChipInfo.Analyse));
		}
		if (flag)
		{
			return source.ThenBy((ChipInfo x) => x.netChipInfo.ChipID);
		}
		return source.ThenByDescending((ChipInfo x) => x.netChipInfo.ChipID);
	}

	public bool IsAnyChipCanUp()
	{
		System.Collections.Generic.Dictionary<int, ChipInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicChip.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (IsCanChipUpgradeStart(enumerator.Current.Value.netChipInfo) || IsCanChipAnalyse(enumerator.Current.Value.netChipInfo))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCanChipUpgradeStart(NetChipInfo tNetChipInfo)
	{
		if (tNetChipInfo.Star >= 5)
		{
			return false;
		}
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(tNetChipInfo.ChipID))
		{
			return false;
		}
		IEnumerable<STAR_TABLE> source = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values.Where((STAR_TABLE obj) => (obj.n_TYPE == 4 && obj.n_MAINID == tNetChipInfo.ChipID && tNetChipInfo.Star == obj.n_STAR) ? true : false);
		if (source.Count() == 0)
		{
			return false;
		}
		STAR_TABLE sTAR_TABLE = source.ElementAt(0);
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[sTAR_TABLE.n_MATERIAL];
		int[] array = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i];
			if (num != 0)
			{
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[num];
				int num2 = 0;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
				}
				if (num2 < array2[i])
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsCanChipAnalyse(NetChipInfo tNetChipInfo)
	{
		if (tNetChipInfo.Analyse >= 5)
		{
			return false;
		}
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(tNetChipInfo.ChipID))
		{
			return false;
		}
		DISC_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(tNetChipInfo.ChipID, out value))
		{
			return false;
		}
		int[] array = new int[5] { value.n_ANALYSE_1, value.n_ANALYSE_2, value.n_ANALYSE_3, value.n_ANALYSE_4, value.n_ANALYSE_5 };
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[array[tNetChipInfo.Analyse]];
		int[] array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] array3 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] != 0)
			{
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[array2[i]];
				int num = 0;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
				{
					num = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
				}
				if (num < array3[i])
				{
					return false;
				}
			}
		}
		return true;
	}

	public int GetWeaponBenchSlot(int WeaponID)
	{
		NetBenchInfo netBenchInfo = ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.Values.Select((BenchInfo x) => x.netBenchInfo).ToList().Find((NetBenchInfo x) => x.WeaponID == WeaponID);
		if (netBenchInfo != null)
		{
			return netBenchInfo.BenchSlot;
		}
		return 0;
	}

	public System.Collections.Generic.Dictionary<int, NetEquipmentInfo> GetDicEquipmentIsEquip()
	{
		System.Collections.Generic.Dictionary<int, NetEquipmentInfo> dictionary = new Better.Dictionary<int, NetEquipmentInfo>();
		foreach (EquipInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicEquip.Values)
		{
			if (value.netEquipmentInfo.Equip == 1)
			{
				EQUIP_TABLE equip = null;
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(value.netEquipmentInfo.EquipItemID, out equip))
				{
					dictionary.Add(equip.n_PARTS, value.netEquipmentInfo);
				}
			}
		}
		return dictionary;
	}

	public List<NetEquipmentInfo> GetListNetEquipment()
	{
		List<NetEquipmentInfo> list = new List<NetEquipmentInfo>();
		foreach (EquipInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicEquip.Values)
		{
			list.Add(value.netEquipmentInfo);
		}
		list.Sort((NetEquipmentInfo x, NetEquipmentInfo y) => x.EquipItemID.CompareTo(y.EquipItemID));
		return list;
	}

	public List<NetEquipmentInfo> GetListNetEquipmentExceptEquipped()
	{
		List<NetEquipmentInfo> list = new List<NetEquipmentInfo>();
		foreach (EquipInfo value2 in ManagedSingleton<PlayerNetManager>.Instance.dicEquip.Values)
		{
			if (value2.netEquipmentInfo.Equip == 0)
			{
				list.Add(value2.netEquipmentInfo);
			}
		}
		for (int i = 1; i < list.Count; i++)
		{
			for (int j = 0; j < list.Count - i; j++)
			{
				if (list[j].EquipItemID > list[j + 1].EquipItemID)
				{
					NetEquipmentInfo value = list[j];
					list[j] = list[j + 1];
					list[j + 1] = value;
				}
			}
		}
		return list;
	}

	public bool IsEquipmentLimitReached()
	{
		return ManagedSingleton<EquipHelper>.Instance.GetListNetEquipmentExceptEquipped().Count >= OrangeConst.EQUIP_MAX_SLOT;
	}

	public bool ShowEquipmentLimitReachedDialog()
	{
		if (IsEquipmentLimitReached())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_SLOT_MAX"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
				{
				});
			});
			return true;
		}
		return false;
	}

	public int[] GetEquipRank(NetEquipmentInfo netEquipmentInfo)
	{
		int[] array = new int[4];
		EQUIP_TABLE equip = null;
		float num = 0f;
		float val = 0f;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(netEquipmentInfo.EquipItemID, out equip))
		{
			for (int i = 0; i < 3; i++)
			{
				switch (i)
				{
				case 0:
					array[i] = GetEquipSingleRank(netEquipmentInfo.DefParam, equip.n_DEF_MAX, equip.n_DEF_MIN, out val);
					break;
				case 1:
					array[i] = GetEquipSingleRank(netEquipmentInfo.HpParam, equip.n_HP_MAX, equip.n_HP_MIN, out val);
					break;
				case 2:
					array[i] = GetEquipSingleRank(netEquipmentInfo.LukParam, equip.n_LUK_MAX, equip.n_LUK_MIN, out val);
					break;
				}
				num += val;
			}
		}
		array[3] = ValueToStars(num / 3f);
		return array;
	}

	private int GetEquipSingleRank(int value, int valueMax, int valueMin, out float val)
	{
		val = (float)(value - valueMin) / (float)(valueMax - valueMin);
		return ValueToStars(val);
	}

	public CardInfo GetSortedCardInfo(int index)
	{
		CardInfo value = new CardInfo();
		value.netCardInfo = new NetCardInfo();
		List<CardInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
		if (CardSortDescend == 1)
		{
			if (index >= list.Count)
			{
				value.netCardInfo.CardSeqID = list[index - list.Count].netCardInfo.CardSeqID;
				value.netCardInfo.CardID = list[index - list.Count].netCardInfo.CardID;
			}
			else
			{
				ManagedSingleton<PlayerNetManager>.Instance.dicCard.TryGetValue(list[index].netCardInfo.CardSeqID, out value);
			}
		}
		else if (index >= list.Count)
		{
			ManagedSingleton<PlayerNetManager>.Instance.dicCard.TryGetValue(listHasWeapons[index - listFragWeapons.Count], out value);
		}
		else
		{
			value.netCardInfo.CardSeqID = list[index].netCardInfo.CardSeqID;
			value.netCardInfo.CardID = list[index].netCardInfo.CardID;
		}
		return value;
	}

	public void ResetCardEquipCharInfo()
	{
		dicCharacterEquipSeqID.Clear();
		foreach (CharacterInfo value2 in ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Values)
		{
			int characterID = value2.netInfo.CharacterID;
			System.Collections.Generic.Dictionary<int, NetCharacterCardSlotInfo> value = null;
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.TryGetValue(characterID, out value))
			{
				continue;
			}
			foreach (NetCharacterCardSlotInfo value3 in value.Values)
			{
				if (value3.CardSeqID > 0 && !dicCharacterEquipSeqID.ContainsKey(value3.CardSeqID))
				{
					dicCharacterEquipSeqID.Add(value3.CardSeqID, characterID);
				}
			}
		}
	}

	public int GetCardEquipCharacterID(int SeqID)
	{
		if (dicCharacterEquipSeqID.ContainsKey(SeqID))
		{
			return dicCharacterEquipSeqID[SeqID];
		}
		return 0;
	}
}
