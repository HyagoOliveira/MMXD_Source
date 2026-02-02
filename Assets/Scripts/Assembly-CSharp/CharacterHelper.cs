using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using enums;

public class CharacterHelper : ManagedSingleton<CharacterHelper>
{
	public enum SortType
	{
		RARITY = 0,
		STAR = 1,
		FAVORITE = 2,
		BATTLE = 3,
		EXPLORE = 4,
		ACTION = 5,
		TOTAL = 6
	}

	public enum SortStatus
	{
		OBTAINED = 0,
		FRAGMENT = 1,
		ALL = 2
	}

	public enum UpgradesFlag
	{
		NONE = 0,
		UNLOCK = 1,
		SKILL = 2,
		STAR = 4,
		SKIN = 8,
		DNA = 0x10
	}

	private List<CharacterInfo> m_sortedCharacterInfoList = new List<CharacterInfo>();

	private Dictionary<int, UpgradesFlag> m_characterUpgradeDict = new Dictionary<int, UpgradesFlag>();

	private bool m_bUpgradeChecking;

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
		m_sortedCharacterInfoList.Clear();
		m_characterUpgradeDict.Clear();
	}

	public SortType GetCharacterUISortType()
	{
		return (SortType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortType;
	}

	public SortStatus GetCharacterUISortStatus()
	{
		return (SortStatus)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortStatus;
	}

	public bool GetCharacterUISortDescend()
	{
		return MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortDescend;
	}

	public List<CharacterInfo> GetSortedCharacterList()
	{
		if (m_sortedCharacterInfoList.Count == 0)
		{
			SortCharacterList();
		}
		return m_sortedCharacterInfoList;
	}

	public List<CharacterInfo> SortCharacterListNoFragmentNoSave()
	{
		SortType lastCharacterUISortType = (SortType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortType;
		SortStatus sortStatus = SortStatus.OBTAINED;
		bool lastCharacterUISortDescend = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortDescend;
		return SortCharacterList(lastCharacterUISortType, sortStatus, lastCharacterUISortDescend, false);
	}

	public List<CharacterInfo> SortCharacterList()
	{
		SortType lastCharacterUISortType = (SortType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortType;
		SortStatus lastCharacterUISortStatus = (SortStatus)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortStatus;
		bool lastCharacterUISortDescend = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortDescend;
		return SortCharacterList(lastCharacterUISortType, lastCharacterUISortStatus, lastCharacterUISortDescend);
	}

	public List<CharacterInfo> SortCharacterList(SortType sortType, SortStatus sortStatus, bool sortDescend, bool bSave = true)
	{
		Dictionary<int, CHARACTER_TABLE> dictionary = new Dictionary<int, CHARACTER_TABLE>(ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT);
		List<CharacterInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Values.ToList();
		List<CharacterInfo> list2 = new List<CharacterInfo>();
		foreach (KeyValuePair<int, CHARACTER_TABLE> item2 in dictionary.Where((KeyValuePair<int, CHARACTER_TABLE> kvp) => kvp.Value.n_ENABLE_FLAG == 0).ToList())
		{
			dictionary.Remove(item2.Key);
		}
		m_sortedCharacterInfoList.Clear();
		switch (sortStatus)
		{
		case SortStatus.OBTAINED:
			foreach (CharacterInfo item3 in list)
			{
				if (item3.netInfo != null)
				{
					m_sortedCharacterInfoList.Add(item3);
				}
			}
			break;
		case SortStatus.FRAGMENT:
			foreach (CharacterInfo item4 in list)
			{
				if (item4.netInfo != null)
				{
					m_sortedCharacterInfoList.Add(item4);
					dictionary.Remove(item4.netInfo.CharacterID);
				}
			}
			foreach (CHARACTER_TABLE value in dictionary.Values)
			{
				int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(value.n_UNLOCK_ID);
				if (itemValue > 0)
				{
					CharacterInfo characterInfo2 = new CharacterInfo();
					characterInfo2.netInfo = new NetCharacterInfo();
					characterInfo2.netInfo.CharacterID = value.n_ID;
					characterInfo2.netInfo.Star = 0;
					characterInfo2.netInfo.State = 2;
					if (itemValue >= value.n_UNLOCK_COUNT)
					{
						list2.Add(characterInfo2);
					}
					else
					{
						m_sortedCharacterInfoList.Add(characterInfo2);
					}
				}
			}
			break;
		case SortStatus.ALL:
			foreach (CharacterInfo item5 in list)
			{
				if (item5.netInfo != null)
				{
					m_sortedCharacterInfoList.Add(item5);
					dictionary.Remove(item5.netInfo.CharacterID);
				}
			}
			foreach (CHARACTER_TABLE value2 in dictionary.Values)
			{
				CharacterInfo characterInfo = new CharacterInfo();
				characterInfo.netInfo = new NetCharacterInfo();
				characterInfo.netInfo.CharacterID = value2.n_ID;
				characterInfo.netInfo.Star = 0;
				characterInfo.netInfo.State = 2;
				if (ManagedSingleton<PlayerHelper>.Instance.GetItemValue(value2.n_UNLOCK_ID) >= value2.n_UNLOCK_COUNT)
				{
					list2.Add(characterInfo);
				}
				else
				{
					m_sortedCharacterInfoList.Add(characterInfo);
				}
			}
			break;
		}
		switch (sortType)
		{
		case SortType.RARITY:
			if (sortDescend)
			{
				m_sortedCharacterInfoList = (from x in m_sortedCharacterInfoList
					orderby x.netInfo.State, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[x.netInfo.CharacterID].n_RARITY descending, x.netInfo.CharacterID
					select x).ToList();
				list2 = (from x in list2
					orderby x.netInfo.State, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[x.netInfo.CharacterID].n_RARITY descending, x.netInfo.CharacterID
					select x).ToList();
			}
			else
			{
				m_sortedCharacterInfoList = (from x in m_sortedCharacterInfoList
					orderby x.netInfo.State descending, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[x.netInfo.CharacterID].n_RARITY, x.netInfo.CharacterID descending
					select x).ToList();
				list2 = (from x in list2
					orderby x.netInfo.State descending, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[x.netInfo.CharacterID].n_RARITY, x.netInfo.CharacterID descending
					select x).ToList();
			}
			break;
		case SortType.STAR:
			if (sortDescend)
			{
				m_sortedCharacterInfoList = (from x in m_sortedCharacterInfoList
					orderby x.netInfo.State, x.netInfo.Star descending, x.netInfo.CharacterID
					select x).ToList();
				list2 = (from x in list2
					orderby x.netInfo.State, x.netInfo.Star descending, x.netInfo.CharacterID
					select x).ToList();
			}
			else
			{
				m_sortedCharacterInfoList = (from x in m_sortedCharacterInfoList
					orderby x.netInfo.State descending, x.netInfo.Star, x.netInfo.CharacterID descending
					select x).ToList();
				list2 = (from x in list2
					orderby x.netInfo.State descending, x.netInfo.Star, x.netInfo.CharacterID descending
					select x).ToList();
			}
			break;
		case SortType.FAVORITE:
			if (sortDescend)
			{
				m_sortedCharacterInfoList = (from x in m_sortedCharacterInfoList
					orderby x.netInfo.State, x.netInfo.Favorite descending, x.netInfo.CharacterID
					select x).ToList();
				list2 = (from x in list2
					orderby x.netInfo.State, x.netInfo.Favorite descending, x.netInfo.CharacterID
					select x).ToList();
			}
			else
			{
				m_sortedCharacterInfoList = (from x in m_sortedCharacterInfoList
					orderby x.netInfo.State descending, x.netInfo.Favorite, x.netInfo.CharacterID descending
					select x).ToList();
				list2 = (from x in list2
					orderby x.netInfo.State descending, x.netInfo.Favorite, x.netInfo.CharacterID descending
					select x).ToList();
			}
			break;
		case SortType.BATTLE:
		case SortType.EXPLORE:
		case SortType.ACTION:
		case SortType.TOTAL:
			bSave = false;
			if (sortDescend)
			{
				m_sortedCharacterInfoList = (from x in m_sortedCharacterInfoList
					orderby x.netInfo.State, DeepRecordHelper.GetCharacterRecordVal(sortType, x) descending, x.netInfo.CharacterID
					select x).ToList();
			}
			else
			{
				m_sortedCharacterInfoList = (from x in m_sortedCharacterInfoList
					orderby x.netInfo.State, DeepRecordHelper.GetCharacterRecordVal(sortType, x), x.netInfo.CharacterID descending
					select x).ToList();
			}
			break;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			m_sortedCharacterInfoList.Insert(i, list2[i]);
		}
		if (bSave)
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortType = (int)sortType;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortStatus = (int)sortStatus;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastCharacterUISortDescend = sortDescend;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		}
		if (ManagedSingleton<EquipHelper>.Instance.listCharacterCompelled.Count > 0)
		{
			for (int num = ManagedSingleton<EquipHelper>.Instance.listCharacterCompelled.Count - 1; num >= 0; num--)
			{
				for (int j = 0; j < m_sortedCharacterInfoList.Count; j++)
				{
					if (m_sortedCharacterInfoList[j].netInfo.CharacterID == ManagedSingleton<EquipHelper>.Instance.listCharacterCompelled[num])
					{
						CharacterInfo item = m_sortedCharacterInfoList[j];
						m_sortedCharacterInfoList.RemoveAt(j);
						m_sortedCharacterInfoList.Insert(0, item);
						break;
					}
				}
			}
			ManagedSingleton<EquipHelper>.Instance.listCharacterCompelled.Clear();
		}
		return m_sortedCharacterInfoList;
	}

	public IEnumerator CheckCharacterUpgradesCBByID(int nID = 0, Callback p_cb = null)
	{
		if (!m_bUpgradeChecking)
		{
			m_bUpgradeChecking = true;
			m_characterUpgradeDict.Clear();
			List<CharacterInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Values.ToList();
			foreach (CharacterInfo item in list)
			{
				UpgradesFlag upgradesFlag = UpdateCharacterUpgradesFlag(item.netInfo.CharacterID);
				if (upgradesFlag != 0)
				{
					m_characterUpgradeDict[item.netInfo.CharacterID] = upgradesFlag;
				}
				if (nID == item.netInfo.CharacterID && p_cb != null)
				{
					p_cb();
					p_cb = null;
				}
				yield return null;
			}
		}
		m_bUpgradeChecking = false;
	}

	public IEnumerator CheckCharacterUpgrades(Callback p_cb = null)
	{
		bool bCallbackInvoked = false;
		if (!m_bUpgradeChecking)
		{
			m_bUpgradeChecking = true;
			m_characterUpgradeDict.Clear();
			List<CharacterInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Values.ToList();
			foreach (CharacterInfo item in list)
			{
				UpgradesFlag upgradesFlag = UpdateCharacterUpgradesFlag(item.netInfo.CharacterID);
				if (upgradesFlag != 0)
				{
					m_characterUpgradeDict[item.netInfo.CharacterID] = upgradesFlag;
					if (!bCallbackInvoked && p_cb != null)
					{
						p_cb();
						bCallbackInvoked = true;
					}
				}
				yield return null;
			}
		}
		m_bUpgradeChecking = false;
	}

	public bool IsUpgradeChecking()
	{
		return m_bUpgradeChecking;
	}

	public bool IsUpgradeAvailable()
	{
		return m_characterUpgradeDict.Count > 0;
	}

	public bool IsCharacterUpgradeAvailable(int characterID)
	{
		UpgradesFlag value;
		if (m_characterUpgradeDict.TryGetValue(characterID, out value) && value != 0)
		{
			return true;
		}
		return false;
	}

	public UpgradesFlag GetCharacterUpgradesFlag(int characterID)
	{
		UpgradesFlag value;
		if (m_characterUpgradeDict.TryGetValue(characterID, out value) && value != 0)
		{
			return value;
		}
		return UpgradesFlag.NONE;
	}

	private UpgradesFlag UpdateCharacterUpgradesFlag(int characterID)
	{
		UpgradesFlag upgradesFlag = UpgradesFlag.NONE;
		int firstNotEnoughItemID = 0;
		int num = 0;
		CharacterInfo info;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(characterID, out info))
		{
			Dictionary<int, STAR_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.n_TYPE == 1 && enumerator.Current.Value.n_MAINID == info.netInfo.CharacterID && enumerator.Current.Value.n_STAR == info.netInfo.Star)
				{
					if (ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(enumerator.Current.Value.n_MATERIAL, out firstNotEnoughItemID))
					{
						upgradesFlag |= UpgradesFlag.STAR;
					}
					break;
				}
			}
			foreach (SKIN_TABLE item in ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.Values.Where((SKIN_TABLE x) => x.n_MAINID == info.netInfo.CharacterID).ToList())
			{
				if (!info.netSkinList.Contains(item.n_ID))
				{
					num = ((item.n_UNLOCK_ID != OrangeConst.ITEMID_FREE_JEWEL) ? ManagedSingleton<PlayerHelper>.Instance.GetItemValue(item.n_UNLOCK_ID) : ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel());
					if (num >= item.n_UNLOCK_COUNT)
					{
						upgradesFlag |= UpgradesFlag.SKIN;
						break;
					}
				}
			}
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[info.netInfo.CharacterID];
			NetCharacterSkillInfo value = null;
			foreach (CharacterSkillSlot value2 in Enum.GetValues(typeof(CharacterSkillSlot)))
			{
				int num2 = 0;
				bool flag = false;
				if (value2 >= CharacterSkillSlot.PassiveSkill1 && value2 <= CharacterSkillSlot.PassiveSkill6 && !info.netSkillDic.TryGetValue(value2, out value))
				{
					switch (value2)
					{
					case CharacterSkillSlot.PassiveSkill1:
						num2 = cHARACTER_TABLE.n_PASSIVE_UNLOCK1;
						flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(cHARACTER_TABLE.n_PASSIVE_MATERIAL1, out firstNotEnoughItemID);
						break;
					case CharacterSkillSlot.PassiveSkill2:
						num2 = cHARACTER_TABLE.n_PASSIVE_UNLOCK2;
						flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(cHARACTER_TABLE.n_PASSIVE_MATERIAL2, out firstNotEnoughItemID);
						break;
					case CharacterSkillSlot.PassiveSkill3:
						num2 = cHARACTER_TABLE.n_PASSIVE_UNLOCK3;
						flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(cHARACTER_TABLE.n_PASSIVE_MATERIAL3, out firstNotEnoughItemID);
						break;
					case CharacterSkillSlot.PassiveSkill4:
						num2 = cHARACTER_TABLE.n_PASSIVE_UNLOCK4;
						flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(cHARACTER_TABLE.n_PASSIVE_MATERIAL4, out firstNotEnoughItemID);
						break;
					case CharacterSkillSlot.PassiveSkill5:
						num2 = cHARACTER_TABLE.n_PASSIVE_UNLOCK5;
						flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(cHARACTER_TABLE.n_PASSIVE_MATERIAL5, out firstNotEnoughItemID);
						break;
					case CharacterSkillSlot.PassiveSkill6:
						num2 = cHARACTER_TABLE.n_PASSIVE_UNLOCK6;
						flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(cHARACTER_TABLE.n_PASSIVE_MATERIAL6, out firstNotEnoughItemID);
						break;
					}
					if (num2 <= info.netInfo.Star && flag)
					{
						upgradesFlag |= UpgradesFlag.SKILL;
						break;
					}
				}
			}
			DNA_TABLE[] array = ManagedSingleton<OrangeDataManager>.Instance.DNA_TABLE_DICT.Values.Where((DNA_TABLE x) => x.n_CHARACTER == characterID && x.n_TYPE == 0).ToArray();
			foreach (DNA_TABLE dNA_TABLE in array)
			{
				if (!info.netDNAInfoDic.ContainsKey(dNA_TABLE.n_SLOT) && info.netInfo.Star >= dNA_TABLE.n_STAR && ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(dNA_TABLE.n_COST_ID, out firstNotEnoughItemID))
				{
					upgradesFlag |= UpgradesFlag.DNA;
				}
			}
		}
		return upgradesFlag;
	}
}
