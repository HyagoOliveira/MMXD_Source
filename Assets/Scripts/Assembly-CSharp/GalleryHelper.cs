using System.Collections.Generic;
using System.Linq;
using enums;

public class GalleryHelper : ManagedSingleton<GalleryHelper>
{
	public enum GalleryShowStatus
	{
		ALL = 0,
		UNLOCKED = 1,
		LOCKED = 2
	}

	public class GalleryCellInfo
	{
		public GalleryType tType;

		public int m_objID;

		public int m_progress;

		public bool m_isMask = true;

		public bool m_isCanUnlock;

		public bool m_isCanGetExp;

		public object mTable;

		public WEAPON_TABLE m_weaponTable;

		public CHARACTER_TABLE m_characterTable;

		public GalleryCalcResult m_result;

		public List<GALLERY_TABLE> m_allGallery = new List<GALLERY_TABLE>();

		public List<GALLERY_TABLE> m_lockGallery = new List<GALLERY_TABLE>();

		public List<GALLERY_TABLE> m_unlockGallery = new List<GALLERY_TABLE>();
	}

	private Dictionary<int, GalleryCellInfo>[] m_galleryCellInfo = new Dictionary<int, GalleryCellInfo>[3];

	public bool GalleryDataReaded { get; set; }

	public Dictionary<int, GalleryCellInfo>[] GalleryCellInfos
	{
		get
		{
			return m_galleryCellInfo;
		}
	}

	public bool WeaponHint { get; set; }

	public bool CharacterHint { get; set; }

	public bool CardHint { get; set; }

	public bool DisplayHint
	{
		get
		{
			if (!WeaponHint && !CharacterHint)
			{
				return CardHint;
			}
			return true;
		}
	}

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public List<GALLERY_TABLE> GalleryGetTable(int p_mainID, GalleryType p_type = GalleryType.Character, GalleryShowStatus p_mode = GalleryShowStatus.ALL)
	{
		List<GALLERY_TABLE> listAll = new List<GALLERY_TABLE>();
		List<GALLERY_TABLE> listUnlock = new List<GALLERY_TABLE>();
		List<GALLERY_TABLE> listLock = new List<GALLERY_TABLE>();
		GalleryGetTableAll(p_mainID, p_type, out listAll, out listUnlock, out listLock);
		switch (p_mode)
		{
		case GalleryShowStatus.UNLOCKED:
			return listUnlock;
		case GalleryShowStatus.LOCKED:
			return listLock;
		default:
			return listAll;
		}
	}

	public void GalleryGetTableAll(int p_mainID, GalleryType p_type, out List<GALLERY_TABLE> listAll, out List<GALLERY_TABLE> listUnlock, out List<GALLERY_TABLE> listLock)
	{
		List<GALLERY_TABLE> list = new List<GALLERY_TABLE>();
		List<GALLERY_TABLE> lUlock = new List<GALLERY_TABLE>();
		List<GALLERY_TABLE> lLock = new List<GALLERY_TABLE>();
		list = (from a in ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT
			where a.Value.n_TYPE == (int)p_type && a.Value.n_MAINID == p_mainID
			select a into p
			select p.Value).ToList();
		if (ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryList.Count != 0)
		{
			list.ForEach(delegate(GALLERY_TABLE tbl)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryList.Any((NetGalleryInfo a) => tbl.n_ID == a.GalleryID))
				{
					lUlock.Add(tbl);
				}
				else
				{
					lLock.Add(tbl);
				}
			});
		}
		else
		{
			lLock = list;
		}
		listAll = list;
		listUnlock = lUlock;
		listLock = lLock;
	}

	public void GalleryGetCardTableAll(int p_mainID, out List<GALLERY_TABLE> listAll, out List<GALLERY_TABLE> listUnlock, out List<GALLERY_TABLE> listLock)
	{
		List<GALLERY_TABLE> list = new List<GALLERY_TABLE>();
		List<GALLERY_TABLE> lUlock = new List<GALLERY_TABLE>();
		List<GALLERY_TABLE> lLock = new List<GALLERY_TABLE>();
		GalleryType p_type = GalleryType.Card;
		int groupID = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[p_mainID].n_GALLERY_MODEL;
		list = (from a in ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT
			where a.Value.n_TYPE == (int)p_type && a.Value.n_MAINID == groupID
			select a into p
			select p.Value).ToList();
		if (ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList.Count != 0)
		{
			NetGalleryMainIdInfo cardInfo = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList.Find((NetGalleryMainIdInfo a) => a.GalleryMainID == p_mainID);
			if (cardInfo != null)
			{
				list.ForEach(delegate(GALLERY_TABLE galleryTbl)
				{
					if (cardInfo.GalleryIDList.Any((int b) => b == galleryTbl.n_ID))
					{
						lUlock.Add(galleryTbl);
					}
					else
					{
						lLock.Add(galleryTbl);
					}
				});
			}
			else
			{
				lLock = list;
			}
		}
		else
		{
			lLock = list;
		}
		listAll = list;
		listUnlock = lUlock;
		listLock = lLock;
	}

	public GalleryCalcResult GalleryCalculationProgress(int p_mainID, GalleryType p_type)
	{
		GalleryCalcResult cResult = new GalleryCalcResult();
		List<GALLERY_TABLE> list = new List<GALLERY_TABLE>();
		if (p_type == GalleryType.Card)
		{
			int groupID = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[p_mainID].n_GALLERY_MODEL;
			list = (from a in ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT
				where a.Value.n_TYPE == (int)p_type && a.Value.n_MAINID == groupID
				select a.Value).ToList();
			cResult.m_b = list.Count;
			NetGalleryMainIdInfo netGalleryMainIdInfo = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryCardList.Find((NetGalleryMainIdInfo p) => p.GalleryMainID == p_mainID);
			if (netGalleryMainIdInfo != null)
			{
				netGalleryMainIdInfo.GalleryIDList.ForEach(delegate(int intID)
				{
					cResult.m_a++;
					cResult.m_totalExp += ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT[intID].n_EXP;
				});
			}
		}
		else
		{
			list = (from a in ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT
				where a.Value.n_TYPE == (int)p_type && a.Value.n_MAINID == p_mainID
				select a.Value).ToList();
			cResult.m_b = list.Count;
			list.ForEach(delegate(GALLERY_TABLE tbl)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryList.Any((NetGalleryInfo p) => p.GalleryID == tbl.n_ID))
				{
					cResult.m_a++;
					cResult.m_totalExp += tbl.n_EXP;
				}
			});
		}
		return cResult;
	}

	public GalleryCalcResult GalleryGetTotalExp()
	{
		GalleryCalcResult galleryCalcResult = new GalleryCalcResult();
		galleryCalcResult.m_totalExp = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryExpList.Sum((NetGalleryExpInfo p) => p.Exp);
		galleryCalcResult.m_lv = GetExpTable(galleryCalcResult.m_totalExp).n_ID;
		return galleryCalcResult;
	}

	public GalleryCalcResult GalleryGetGalleryTypeExp(GalleryType type)
	{
		GalleryCalcResult galleryCalcResult = new GalleryCalcResult();
		sbyte searchType = (sbyte)type;
		galleryCalcResult.m_totalExp = ManagedSingleton<PlayerNetManager>.Instance.galleryInfo.GalleryExpList.Where((NetGalleryExpInfo a) => a.GalleryType == searchType).Sum((NetGalleryExpInfo p) => p.Exp);
		EXP_TABLE expTable = GetExpTable(galleryCalcResult.m_totalExp);
		galleryCalcResult.m_lv = expTable.n_ID;
		return galleryCalcResult;
	}

	public GalleryCalcResult GalleryGetCharactersExp()
	{
		return GalleryGetGalleryTypeExp(GalleryType.Character);
	}

	public GalleryCalcResult GalleryGetWeaponsExp()
	{
		return GalleryGetGalleryTypeExp(GalleryType.Weapon);
	}

	public GalleryCalcResult GalleryGetCardsExp()
	{
		return GalleryGetGalleryTypeExp(GalleryType.Card);
	}

	public EXP_TABLE GetExpTable(int nExp)
	{
		EXP_TABLE result = null;
		foreach (EXP_TABLE value in ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.Values)
		{
			if (value.n_TOTAL_GALLERYEXP > nExp)
			{
				return value;
			}
			result = value;
		}
		return result;
	}

	public bool GalleryCheckUnlock(int p_galleryID, int cardID = 0)
	{
		GALLERY_TABLE tbl = ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT[p_galleryID];
		return GalleryCheckUnlock(tbl, cardID);
	}

	public bool GalleryCheckUnlock(GALLERY_TABLE tbl, int cardID = 0)
	{
		int num = 0;
		if (tbl.n_TYPE == 1)
		{
			CharacterInfo value;
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(tbl.n_MAINID, out value))
			{
				return false;
			}
			switch ((GalleryCondition)(short)tbl.n_CONDITION)
			{
			case GalleryCondition.FirstGet:
				return true;
			case GalleryCondition.UpgradeStar:
				if (value.netInfo.Star >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			case GalleryCondition.GetSkillId:
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(value.netInfo.CharacterID) == null)
				{
					return false;
				}
				return value.netSkinList.Any((int p) => p == tbl.n_CONDITION_X);
			case GalleryCondition.CompleteStage:
				if (value.netInfo.PveCount >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			case GalleryCondition.CompleteMultiplay:
				if (value.netInfo.PvpCount >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			case GalleryCondition.UnlockPassiveSkill:
				num = value.netSkillDic.Where((KeyValuePair<CharacterSkillSlot, NetCharacterSkillInfo> p) => p.Key >= CharacterSkillSlot.PassiveSkill1 && p.Key <= CharacterSkillSlot.PassiveSkill7).Count();
				return tbl.n_CONDITION_X <= num;
			case GalleryCondition.CharacterSkillAccumulated:
				if (value.netSkillDic.Sum((KeyValuePair<CharacterSkillSlot, NetCharacterSkillInfo> p) => p.Value.Level) >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			}
		}
		else if (tbl.n_TYPE == 2)
		{
			WeaponInfo value2;
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(tbl.n_MAINID, out value2))
			{
				return false;
			}
			ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponTable(value2.netInfo.WeaponID);
			switch ((GalleryCondition)(short)tbl.n_CONDITION)
			{
			case GalleryCondition.FirstGet:
				return true;
			case GalleryCondition.UpgradeStar:
				if (value2.netInfo.Star >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			case GalleryCondition.GetSkillId:
				return false;
			case GalleryCondition.CompleteStage:
				if (value2.netInfo.PveCount >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			case GalleryCondition.CompleteMultiplay:
				if (value2.netInfo.PvpCount >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			case GalleryCondition.WeaponUpgradeLevel:
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(value2.netInfo.Exp) >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			case GalleryCondition.WeaponUpgradeExpert:
				if (value2.netExpertInfos != null && value2.netExpertInfos.Sum((NetWeaponExpertInfo p) => p.ExpertLevel) >= tbl.n_CONDITION_X)
				{
					return true;
				}
				break;
			case GalleryCondition.UnlockPassiveSkill:
				if (value2.netSkillInfos != null)
				{
					return tbl.n_CONDITION_X <= value2.netSkillInfos.Count;
				}
				break;
			}
		}
		else if (tbl.n_TYPE == 3)
		{
			List<NetCardInfo> list = (from p in ManagedSingleton<PlayerNetManager>.Instance.dicCard
				where p.Value.netCardInfo.CardID == cardID
				select p into v
				select v.Value.netCardInfo).ToList();
			if (list == null)
			{
				return false;
			}
			switch ((GalleryCondition)(short)tbl.n_CONDITION)
			{
			case GalleryCondition.FirstGet:
				return true;
			case GalleryCondition.UpgradeStar:
				foreach (NetCardInfo item in list)
				{
					if (item.Star >= tbl.n_CONDITION_X)
					{
						return true;
					}
				}
				break;
			case GalleryCondition.GetSkillId:
				return false;
			case GalleryCondition.CompleteStage:
				return false;
			case GalleryCondition.CompleteMultiplay:
				return false;
			case GalleryCondition.WeaponUpgradeLevel:
				return false;
			case GalleryCondition.WeaponUpgradeExpert:
				return false;
			case GalleryCondition.UnlockPassiveSkill:
				return false;
			case GalleryCondition.CardLevel:
				foreach (NetCardInfo item2 in list)
				{
					if (ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(item2.Exp) >= tbl.n_CONDITION_X)
					{
						return true;
					}
				}
				break;
			}
		}
		return false;
	}

	public bool GalleryCompletionRateRetrieved(GalleryType type, int mainId)
	{
		return false;
	}

	public void BuildGalleryInfo()
	{
		Dictionary<int, GALLERY_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.GALLERY_TABLE_DICT.GetEnumerator();
		m_galleryCellInfo[0] = new Dictionary<int, GalleryCellInfo>();
		m_galleryCellInfo[1] = new Dictionary<int, GalleryCellInfo>();
		m_galleryCellInfo[2] = new Dictionary<int, GalleryCellInfo>();
		Dictionary<int, GalleryCellInfo> dictionary = new Dictionary<int, GalleryCellInfo>();
		Dictionary<int, GalleryCellInfo> dictionary2 = new Dictionary<int, GalleryCellInfo>();
		Dictionary<int, GalleryCellInfo> dictionary3 = new Dictionary<int, GalleryCellInfo>();
		int num;
		while (enumerator.MoveNext())
		{
			GALLERY_TABLE value = enumerator.Current.Value;
			num = value.n_TYPE - 1;
			GalleryCellInfo value2;
			if (!m_galleryCellInfo[num].TryGetValue(value.n_MAINID, out value2))
			{
				value2 = new GalleryCellInfo();
				value2.m_objID = value.n_MAINID;
				value2.tType = (GalleryType)value.n_TYPE;
				ManagedSingleton<GalleryHelper>.Instance.GalleryGetTableAll(value.n_MAINID, (GalleryType)value.n_TYPE, out value2.m_allGallery, out value2.m_unlockGallery, out value2.m_lockGallery);
				switch (num)
				{
				case 1:
					value2.m_weaponTable = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[value.n_MAINID];
					value2.mTable = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[value.n_MAINID];
					break;
				case 0:
					value2.m_characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[value.n_MAINID];
					value2.mTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[value.n_MAINID];
					break;
				}
				m_galleryCellInfo[num].ContainsAdd(value.n_MAINID, value2);
			}
		}
		bool flag2 = (CardHint = false);
		bool characterHint = (WeaponHint = flag2);
		CharacterHint = characterHint;
		num = 0;
		Dictionary<int, GalleryCellInfo>.Enumerator enumerator2 = m_galleryCellInfo[num].GetEnumerator();
		while (enumerator2.MoveNext())
		{
			GalleryCellInfo value3 = enumerator2.Current.Value;
			int objID = value3.m_objID;
			value3.m_isMask = !value3.m_unlockGallery.Any((GALLERY_TABLE u) => (short)u.n_CONDITION == 1);
			if (!value3.m_isMask)
			{
				CharacterHint |= (value3.m_isCanGetExp = value3.m_lockGallery.Any((GALLERY_TABLE ck) => ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(ck.n_ID)));
				value3.m_result = ManagedSingleton<GalleryHelper>.Instance.GalleryCalculationProgress(objID, GalleryType.Character);
				value3.m_progress = (int)((float)value3.m_result.m_a / (float)value3.m_result.m_b * 100f);
				if (value3.m_isCanGetExp)
				{
					dictionary.Add(enumerator2.Current.Key, value3);
				}
				else
				{
					dictionary2.Add(enumerator2.Current.Key, value3);
				}
			}
			else if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.ContainsKey(objID))
			{
				value3.m_isCanUnlock = true;
				CharacterHint = true;
				dictionary3.Add(enumerator2.Current.Key, value3);
			}
			else
			{
				dictionary2.Add(enumerator2.Current.Key, value3);
			}
		}
		foreach (KeyValuePair<int, GalleryCellInfo> item in dictionary3)
		{
			dictionary.Add(item.Key, item.Value);
		}
		foreach (KeyValuePair<int, GalleryCellInfo> item2 in dictionary2)
		{
			dictionary.Add(item2.Key, item2.Value);
		}
		m_galleryCellInfo[num] = dictionary;
		dictionary = new Dictionary<int, GalleryCellInfo>();
		dictionary3 = new Dictionary<int, GalleryCellInfo>();
		dictionary2 = new Dictionary<int, GalleryCellInfo>();
		num = 1;
		enumerator2 = m_galleryCellInfo[num].GetEnumerator();
		while (enumerator2.MoveNext())
		{
			GalleryCellInfo value4 = enumerator2.Current.Value;
			int objID2 = value4.m_objID;
			value4.m_isMask = !value4.m_unlockGallery.Any((GALLERY_TABLE u) => (short)u.n_CONDITION == 1);
			if (!value4.m_isMask)
			{
				WeaponHint |= (value4.m_isCanGetExp = value4.m_lockGallery.Any((GALLERY_TABLE ck) => ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(ck.n_ID)));
				value4.m_result = ManagedSingleton<GalleryHelper>.Instance.GalleryCalculationProgress(objID2, GalleryType.Weapon);
				value4.m_progress = (int)((float)value4.m_result.m_a / (float)value4.m_result.m_b * 100f);
				if (value4.m_isCanGetExp)
				{
					dictionary.Add(enumerator2.Current.Key, value4);
				}
				else
				{
					dictionary2.Add(enumerator2.Current.Key, value4);
				}
			}
			else if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(objID2))
			{
				value4.m_isCanUnlock = true;
				WeaponHint = true;
				dictionary3.Add(enumerator2.Current.Key, value4);
			}
			else
			{
				dictionary2.Add(enumerator2.Current.Key, value4);
			}
		}
		foreach (KeyValuePair<int, GalleryCellInfo> item3 in dictionary3)
		{
			dictionary.Add(item3.Key, item3.Value);
		}
		foreach (KeyValuePair<int, GalleryCellInfo> item4 in dictionary2)
		{
			dictionary.Add(item4.Key, item4.Value);
		}
		m_galleryCellInfo[num] = dictionary;
		dictionary = new Dictionary<int, GalleryCellInfo>();
		dictionary3 = new Dictionary<int, GalleryCellInfo>();
		dictionary2 = new Dictionary<int, GalleryCellInfo>();
		num = 2;
		Dictionary<int, CARD_TABLE>.Enumerator enumerator4 = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.GetEnumerator();
		while (enumerator4.MoveNext())
		{
			CARD_TABLE ctbl = enumerator4.Current.Value;
			GalleryCellInfo value2 = new GalleryCellInfo();
			value2.m_objID = ctbl.n_ID;
			value2.tType = GalleryType.Card;
			value2.mTable = ctbl;
			ManagedSingleton<GalleryHelper>.Instance.GalleryGetCardTableAll(ctbl.n_ID, out value2.m_allGallery, out value2.m_unlockGallery, out value2.m_lockGallery);
			if (value2.m_unlockGallery.Count != 0)
			{
				value2.m_isMask = false;
			}
			if (value2.m_isMask)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCard.Any((KeyValuePair<int, CardInfo> p) => p.Value.netCardInfo.CardID == ctbl.n_ID))
				{
					CardHint |= true;
					value2.m_isCanUnlock = true;
					dictionary3.Add(ctbl.n_ID, value2);
				}
				else
				{
					dictionary2.Add(ctbl.n_ID, value2);
				}
				continue;
			}
			value2.m_result = ManagedSingleton<GalleryHelper>.Instance.GalleryCalculationProgress(ctbl.n_ID, GalleryType.Card);
			value2.m_progress = (int)((float)value2.m_result.m_a / (float)value2.m_result.m_b * 100f);
			foreach (GALLERY_TABLE item5 in value2.m_lockGallery)
			{
				if (ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(item5, ctbl.n_ID))
				{
					CardHint |= true;
					value2.m_isCanGetExp = true;
					break;
				}
			}
			if (value2.m_isCanGetExp)
			{
				dictionary.Add(ctbl.n_ID, value2);
			}
			else
			{
				dictionary2.Add(ctbl.n_ID, value2);
			}
		}
		foreach (KeyValuePair<int, GalleryCellInfo> item6 in dictionary3)
		{
			dictionary.Add(item6.Key, item6.Value);
		}
		foreach (KeyValuePair<int, GalleryCellInfo> item7 in dictionary2)
		{
			dictionary.Add(item7.Key, item7.Value);
		}
		m_galleryCellInfo[num] = dictionary;
	}
}
