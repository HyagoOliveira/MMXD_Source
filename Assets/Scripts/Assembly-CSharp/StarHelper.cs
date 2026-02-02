using System.Collections.Generic;
using System.Linq;
using enums;

public class StarHelper : ManagedSingleton<StarHelper>
{
	public enum CostSearchType
	{
		NextLevel = 0,
		MaxLevel = 1
	}

	public struct UnlockData
	{
		public StarTableType StarType;

		public int Id;
	}

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public bool GetUnlockDataByItem(ITEM_TABLE itemTable, out UnlockData unlockData)
	{
		unlockData = default(UnlockData);
		if (itemTable.n_TYPE != 4)
		{
			return false;
		}
		switch ((ShardType)(short)itemTable.n_TYPE_X)
		{
		case ShardType.Character:
		{
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.Values.FirstOrDefault((CHARACTER_TABLE x) => x.n_UNLOCK_ID == itemTable.n_ID);
			if (cHARACTER_TABLE == null)
			{
				return false;
			}
			unlockData.Id = cHARACTER_TABLE.n_ID;
			unlockData.StarType = StarTableType.Character;
			break;
		}
		case ShardType.Weapon:
		{
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.Values.FirstOrDefault((WEAPON_TABLE x) => x.n_UNLOCK_ID == itemTable.n_ID);
			if (wEAPON_TABLE == null)
			{
				return false;
			}
			unlockData.Id = wEAPON_TABLE.n_ID;
			unlockData.StarType = StarTableType.Weapon;
			break;
		}
		case ShardType.FS:
		{
			FS_TABLE fS_TABLE = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.FirstOrDefault((FS_TABLE x) => x.n_UNLOCK_ID == itemTable.n_ID);
			if (fS_TABLE == null)
			{
				return false;
			}
			unlockData.Id = fS_TABLE.n_ID;
			unlockData.StarType = StarTableType.FinalStrike;
			break;
		}
		case ShardType.Chip:
		{
			DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.Values.FirstOrDefault((DISC_TABLE x) => x.n_UNLOCK_ID == itemTable.n_ID);
			if (dISC_TABLE == null)
			{
				return false;
			}
			unlockData.Id = dISC_TABLE.n_ID;
			unlockData.StarType = StarTableType.BossWafer;
			break;
		}
		default:
			return false;
		}
		return true;
	}

	public bool GetUpgradeCostMaterialAmount(int id, StarTableType starType, CostSearchType costSearchType, out int cost)
	{
		cost = 0;
		int num = -1;
		int num2 = -1;
		switch (starType)
		{
		case StarTableType.Character:
		{
			CharacterInfo value4;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(id, out value4))
			{
				num = value4.netInfo.CharacterID;
				num2 = value4.netInfo.Star;
				break;
			}
			CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(id);
			if (characterTable != null)
			{
				cost = characterTable.n_UNLOCK_COUNT;
			}
			break;
		}
		case StarTableType.Weapon:
		{
			WeaponInfo value2;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(id, out value2))
			{
				num = value2.netInfo.WeaponID;
				num2 = value2.netInfo.Star;
				break;
			}
			WEAPON_TABLE weaponTable = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponTable(id);
			if (weaponTable != null)
			{
				cost = weaponTable.n_UNLOCK_COUNT;
			}
			break;
		}
		case StarTableType.FinalStrike:
		{
			FinalStrikeInfo value3;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(id, out value3))
			{
				num = value3.netFinalStrikeInfo.FinalStrikeID;
				num2 = value3.netFinalStrikeInfo.Star;
			}
			break;
		}
		case StarTableType.BossWafer:
		{
			ChipInfo value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(id, out value))
			{
				num = value.netChipInfo.ChipID;
				num2 = value.netChipInfo.Star;
			}
			break;
		}
		}
		if (num == -1 || num2 == -1)
		{
			return false;
		}
		switch (costSearchType)
		{
		case CostSearchType.NextLevel:
			cost = GetCostByStarTable((int)starType, num, num2);
			break;
		case CostSearchType.MaxLevel:
		{
			int num3 = num2;
			cost = 0;
			while (true)
			{
				int costByStarTable = GetCostByStarTable((int)starType, num, num3);
				if (costByStarTable == 0)
				{
					break;
				}
				cost += costByStarTable;
				num3++;
			}
			break;
		}
		}
		return cost != 0;
	}

	private int GetCostByStarTable(int chkStarType, int chkId, int chkStar)
	{
		Dictionary<int, STAR_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext() && (enumerator.Current.Value.n_TYPE != chkStarType || enumerator.Current.Value.n_MAINID != chkId || enumerator.Current.Value.n_STAR != chkStar))
		{
		}
		STAR_TABLE value = enumerator.Current.Value;
		MATERIAL_TABLE value2;
		if (value != null && ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(value.n_MATERIAL, out value2))
		{
			return value2.n_MATERIAL_MOUNT1;
		}
		return 0;
	}
}
