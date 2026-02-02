using System;
using System.Collections.Generic;
using System.Linq;
using OrangeApi;
using UnityEngine;
using enums;

public class PlayerHelper : ManagedSingleton<PlayerHelper>
{
	private int cacheExp = -1;

	private int cacheLv = -1;

	private bool CheatPlayer;

	private int LockTime;

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public bool CheckPlayerIsSelf(string playerId)
	{
		if (!string.IsNullOrEmpty(playerId) && !string.IsNullOrEmpty(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID))
		{
			return MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID == playerId;
		}
		return false;
	}

	public int GetItemValue(int nItemID)
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(nItemID) && ManagedSingleton<PlayerNetManager>.Instance.dicItem[nItemID].netItemInfo != null)
		{
			return ManagedSingleton<PlayerNetManager>.Instance.dicItem[nItemID].netItemInfo.Stack;
		}
		return 0;
	}

	public int GetZenny()
	{
		return GetItemValue(OrangeConst.ITEMID_MONEY);
	}

	public int GetFreeJewel()
	{
		return GetItemValue(OrangeConst.ITEMID_FREE_JEWEL);
	}

	public int GetPaidJewel()
	{
		return GetItemValue(OrangeConst.ITEMID_JEWEL);
	}

	public int GetTotalJewel()
	{
		return GetFreeJewel() + GetPaidJewel();
	}

	public int GetSkillPoint()
	{
		return GetItemValue(OrangeConst.ITEMID_SKILL_POINT);
	}

	public int GetProfPoint()
	{
		return GetItemValue(OrangeConst.ITEMID_SHARE_PROF);
	}

	public int GetResearchExp()
	{
		return GetItemValue(OrangeConst.ITEMID_RESEARCH_EXP);
	}

	public int GetSeasonTier()
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.MatchHunterRankTable == null)
		{
			return 1;
		}
		return MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.MatchHunterRankTable.n_MAIN_RANK;
	}

	public int GetExp()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo == null)
		{
			return 0;
		}
		return ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Exp;
	}

	public int GetStaminaLimit()
	{
		int lV = GetLV();
		return ManagedSingleton<OrangeTableHelper>.Instance.GetStaminaLimit(lV);
	}

	public int GetStamina()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo == null)
		{
			return 0;
		}
		int num = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.ActionPoint;
		int staminaLimit = GetStaminaLimit();
		if (num < staminaLimit)
		{
			num += Mathf.Clamp((int)((MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC - ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.ActionPointTimer) / (60 * OrangeConst.AP_RECOVER_TIME)), 0, staminaLimit - num);
		}
		return num;
	}

	public int GetEventStamina()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo == null)
		{
			return 0;
		}
		int num = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.EventActionPoint;
		int eP_MAX = OrangeConst.EP_MAX;
		if (num < eP_MAX)
		{
			num += Mathf.Clamp((int)((MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC - ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.EventActionPointTimer) / (60 * OrangeConst.EP_RECOVER_TIME)), 0, eP_MAX - num);
		}
		return num;
	}

	public int GetLV()
	{
		int exp = GetExp();
		if (exp == cacheExp)
		{
			return cacheLv;
		}
		cacheExp = exp;
		EXP_TABLE expTable = ManagedSingleton<OrangeTableHelper>.Instance.GetExpTable(cacheExp);
		cacheLv = expTable.n_ID;
		return cacheLv;
	}

	public EXP_TABLE GetExpTable()
	{
		return ManagedSingleton<OrangeTableHelper>.Instance.GetExpTable(GetExp());
	}

	public bool CheckMaterialEnough(int materialID, out int firstNotEnoughItemID)
	{
		firstNotEnoughItemID = 0;
		MATERIAL_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(materialID, out value))
		{
			return false;
		}
		if (value.n_MONEY > ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel())
		{
			return false;
		}
		foreach (KeyValuePair<int, int> item in new List<KeyValuePair<int, int>>
		{
			new KeyValuePair<int, int>(value.n_MATERIAL_1, value.n_MATERIAL_MOUNT1),
			new KeyValuePair<int, int>(value.n_MATERIAL_2, value.n_MATERIAL_MOUNT2),
			new KeyValuePair<int, int>(value.n_MATERIAL_3, value.n_MATERIAL_MOUNT3),
			new KeyValuePair<int, int>(value.n_MATERIAL_4, value.n_MATERIAL_MOUNT4),
			new KeyValuePair<int, int>(value.n_MATERIAL_5, value.n_MATERIAL_MOUNT5)
		})
		{
			if (item.Key == 0 || item.Value == 0)
			{
				continue;
			}
			if (item.Key == OrangeConst.ITEMID_FREE_JEWEL)
			{
				if (item.Value > GetTotalJewel())
				{
					firstNotEnoughItemID = item.Key;
					return false;
				}
			}
			else if (item.Value > GetItemValue(item.Key))
			{
				firstNotEnoughItemID = item.Key;
				return false;
			}
		}
		return true;
	}

	public List<NetItemInfo> GetListNetItemByType(ItemType p_itemType)
	{
		List<NetItemInfo> list = new List<NetItemInfo>();
		foreach (ItemInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicItem.Values)
		{
			if (value.netItemInfo.Stack > 0)
			{
				ITEM_TABLE item = null;
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(value.netItemInfo.ItemID, out item) && item.n_TYPE == (int)p_itemType)
				{
					list.Add(value.netItemInfo);
				}
			}
		}
		return list.OrderBy((NetItemInfo x) => x.ItemID).ToList();
	}

	public NetSealBattleSettingInfo ParserUnsealedBattleSetting(string p_setting)
	{
		return JsonHelper.Deserialize<NetSealBattleSettingInfo>(p_setting);
	}

	public bool ParserUnsealedBattleSetting(string p_setting, out NetSealBattleSettingInfo netSealBattleSettingInfo)
	{
		return JsonHelper.TryDeserialize<NetSealBattleSettingInfo>(p_setting, out netSealBattleSettingInfo);
	}

	public List<TUTORIAL_TABLE> GetListTutorialBySave()
	{
		return ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.Values.Where((TUTORIAL_TABLE x) => x.n_SAVE > 0 && !TutorialDone(x.n_SAVE)).ToList();
	}

	public int GetCurrentTurtorialID()
	{
		int num = 1;
		int num2 = ManagedSingleton<PlayerNetManager>.Instance.TutorialList.Count - 1;
		while (num2 >= 0 && ManagedSingleton<PlayerNetManager>.Instance.TutorialList[num2] >= num)
		{
			TUTORIAL_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(ManagedSingleton<PlayerNetManager>.Instance.TutorialList[num2], out value) && value.n_PRE == 0 && num < value.n_ID)
			{
				num = value.n_ID;
			}
			num2--;
		}
		return num;
	}

	public int GetTrutorialNonLinear(string sTriggerCheck = "")
	{
		int nPlayerLV = GetLV();
		IEnumerable<KeyValuePair<int, TUTORIAL_TABLE>> source = ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.Where(delegate(KeyValuePair<int, TUTORIAL_TABLE> obj)
		{
			if (sTriggerCheck != "")
			{
				if (TurtorialUI.GetTriggerName(obj.Value.s_TRIGGER) == sTriggerCheck || obj.Value.s_TRIGGER == "UI_Hometop")
				{
					if (obj.Value.n_PRE != 0 && !ManagedSingleton<PlayerNetManager>.Instance.TutorialList.Contains(obj.Key))
					{
						return obj.Value.n_PRE <= nPlayerLV;
					}
					return false;
				}
				return false;
			}
			return obj.Value.n_PRE != 0 && !ManagedSingleton<PlayerNetManager>.Instance.TutorialList.Contains(obj.Key) && obj.Value.n_PRE <= nPlayerLV;
		});
		int result = 0;
		if (source.Count() > 0)
		{
			result = source.ElementAt(0).Key;
		}
		return result;
	}

	public bool TutorialDone(int tutorialFlag)
	{
		return ManagedSingleton<PlayerNetManager>.Instance.TutorialList.Contains(tutorialFlag);
	}

	public int GetBattlePower()
	{
		PlayerStatus playerFinalStatus = ManagedSingleton<StatusHelper>.Instance.GetPlayerFinalStatus();
		return 0 + ((int)playerFinalStatus.nHP * OrangeConst.BP_HP + (int)playerFinalStatus.nATK * OrangeConst.BP_ATK + (int)playerFinalStatus.nDEF * OrangeConst.BP_DEF);
	}

	public int GetBattlePower(WeaponStatus wmainstatus, WeaponStatus wsubstatus, PlayerStatus pallstatus, bool bWeaponUIModel = false)
	{
		int num = 0;
		if (bWeaponUIModel)
		{
			num += (int)wmainstatus.nBattlePower;
			num += (int)wsubstatus.nBattlePower;
		}
		else
		{
			num += (int)wmainstatus.nHP * OrangeConst.BP_HP + (int)wmainstatus.nATK * OrangeConst.BP_ATK + (int)wmainstatus.nDEF * OrangeConst.BP_DEF;
			num += (int)wsubstatus.nHP * OrangeConst.BP_HP + (int)wsubstatus.nATK * OrangeConst.BP_ATK + (int)wsubstatus.nDEF * OrangeConst.BP_DEF;
		}
		return num + ((int)pallstatus.nHP * OrangeConst.BP_HP + (int)pallstatus.nATK * OrangeConst.BP_ATK + (int)pallstatus.nDEF * OrangeConst.BP_DEF);
	}

	public int GetCurrentWeaponLvUpPower(NetWeaponInfo netWeaponInfo)
	{
		int weaponRank = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(netWeaponInfo.Exp);
		WEAPON_TABLE weaponTable = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponTable(netWeaponInfo.WeaponID);
		EXP_TABLE eXP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[weaponRank];
		int weaponType = 2;
		STAR_TABLE sTAR_TABLE = (from x in ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values
			where x.n_TYPE == weaponType
			where x.n_MAINID == netWeaponInfo.WeaponID
			select x).FirstOrDefault((STAR_TABLE x) => x.n_STAR == netWeaponInfo.Star);
		float num = weaponTable.f_PARAM;
		float num2 = weaponTable.f_PARAM;
		if (sTAR_TABLE != null)
		{
			num *= 1f + sTAR_TABLE.f_HP;
			num2 *= 1f + sTAR_TABLE.f_ATK;
		}
		return (int)((float)eXP_TABLE.n_WEAPON_HP * num) * OrangeConst.BP_HP + (int)((float)eXP_TABLE.n_WEAPON_ATK * num2) * OrangeConst.BP_ATK;
	}

	public int GetCurrentWeaponExpertPower(List<NetWeaponExpertInfo> listWeaponExpertInfo)
	{
		int num = 0;
		if (listWeaponExpertInfo != null)
		{
			foreach (NetWeaponExpertInfo expertInfo in listWeaponExpertInfo)
			{
				UPGRADE_TABLE value = ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT.FirstOrDefault((KeyValuePair<int, UPGRADE_TABLE> x) => x.Value.n_LV == expertInfo.ExpertLevel).Value;
				num += value.n_HP;
			}
		}
		return num;
	}

	public int GetChipPower()
	{
		int[] array = new int[2]
		{
			ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID,
			ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID
		};
		int[] array2 = new int[2];
		float num = 0f;
		for (int i = 0; i < array.Length; i++)
		{
			WeaponInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(array[i], out value))
			{
				ChipInfo value2 = null;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(value.netInfo.Chip, out value2))
				{
					array2[i] = value2.netChipInfo.ChipID;
				}
			}
		}
		int type = 4;
		foreach (ChipInfo chip in ManagedSingleton<PlayerNetManager>.Instance.dicChip.Values)
		{
			int tempChipId = chip.netChipInfo.ChipID;
			float num2 = ((tempChipId == array2[0] || tempChipId == array2[1]) ? 1f : GetChopAnalyseRate(chip.netChipInfo.Analyse));
			EXP_TABLE chipExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetChipExpTable(chip.netChipInfo.Exp);
			DISC_TABLE value3 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(tempChipId, out value3))
			{
				float num3 = 1f;
				float num4 = 1f;
				float num5 = 1f;
				STAR_TABLE sTAR_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values.Where((STAR_TABLE x) => x.n_TYPE == type && x.n_MAINID == tempChipId).FirstOrDefault((STAR_TABLE x) => x.n_STAR == chip.netChipInfo.Star);
				if (sTAR_TABLE != null)
				{
					num3 = 1f + sTAR_TABLE.f_ATK;
					num4 = 1f + sTAR_TABLE.f_DEF;
					num5 = 1f + sTAR_TABLE.f_HP;
				}
				num += (float)((int)(num2 * value3.f_PARAM * num3 * (float)chipExpTable.n_DISC_ATK) * OrangeConst.BP_ATK + (int)(num2 * value3.f_PARAM * num4 * (float)chipExpTable.n_DISC_DEF) * OrangeConst.BP_DEF + (int)(num2 * value3.f_PARAM * num5 * (float)chipExpTable.n_DISC_HP) * OrangeConst.BP_HP);
			}
		}
		return Mathf.FloorToInt(num);
	}

	private float GetChopAnalyseRate(sbyte p_analyse)
	{
		return (float)(new int[7]
		{
			0,
			OrangeConst.DISC_ANALYSE_1,
			OrangeConst.DISC_ANALYSE_2,
			OrangeConst.DISC_ANALYSE_3,
			OrangeConst.DISC_ANALYSE_4,
			OrangeConst.DISC_ANALYSE_5,
			OrangeConst.DISC_ANALYSE_5
		})[p_analyse] * 0.01f;
	}

	public int GetFsPower()
	{
		int fsType = 3;
		float num = 0f;
		foreach (FinalStrikeInfo fs in ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.Values)
		{
			if (fs.netFinalStrikeInfo.Level == 0)
			{
				continue;
			}
			FS_TABLE fS_TABLE = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == fs.netFinalStrikeInfo.FinalStrikeID).FirstOrDefault((FS_TABLE x) => x.n_LV == fs.netFinalStrikeInfo.Level);
			if (fS_TABLE != null)
			{
				STAR_TABLE sTAR_TABLE = (from x in ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values
					where x.n_TYPE == fsType
					where x.n_MAINID == fs.netFinalStrikeInfo.FinalStrikeID
					select x).FirstOrDefault((STAR_TABLE x) => x.n_STAR == fs.netFinalStrikeInfo.Star);
				if (sTAR_TABLE != null)
				{
					num += (float)((int)((float)fS_TABLE.n_ATK * (1f + sTAR_TABLE.f_ATK)) * OrangeConst.BP_ATK + (int)((float)fS_TABLE.n_DEF * (1f + sTAR_TABLE.f_DEF)) * OrangeConst.BP_DEF + (int)((float)fS_TABLE.n_HP * (1f + sTAR_TABLE.f_HP)) * OrangeConst.BP_HP);
				}
			}
		}
		return Mathf.FloorToInt(num);
	}

	public int GetEquipPower()
	{
		int num = 0;
		foreach (NetEquipmentInfo value in ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip().Values)
		{
			num += value.DefParam * OrangeConst.BP_DEF + value.HpParam * OrangeConst.BP_HP;
		}
		return num;
	}

	public int GetEquipEnhancePower()
	{
		int num = 0;
		foreach (EquipPartType value3 in Enum.GetValues(typeof(EquipPartType)))
		{
			EquipEnhanceInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicEquipEnhance.TryGetValue(value3, out value))
			{
				EXP_TABLE value2 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(value.netPlayerEquipInfo.EnhanceLv, out value2))
				{
					num += value2.n_EQUIPUP_DEF * OrangeConst.BP_DEF + value2.n_EQUIPUP_HP * OrangeConst.BP_HP;
				}
			}
		}
		return num;
	}

	public int GetBackupWeaponSlotPower(int slot)
	{
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.ContainsKey(slot))
		{
			return 0;
		}
		NetBenchInfo info = ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo[slot].netBenchInfo;
		if (info.WeaponID <= 0)
		{
			return 0;
		}
		BACKUP_TABLE bACKUP_TABLE = null;
		List<BACKUP_TABLE> list = (from p in ManagedSingleton<OrangeDataManager>.Instance.BACKUP_TABLE_DICT
			where p.Value.n_SLOT == info.BenchSlot && p.Value.n_SLOT_LV == info.Level
			select p into o
			select o.Value).ToList();
		if (list.Count > 0)
		{
			bACKUP_TABLE = list[0];
			WeaponInfo tWeaponInfo = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[info.WeaponID];
			WeaponStatus weaponStatusX = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(tWeaponInfo, 0, false, null, delegate
			{
			});
			int n_STATUS_RATE = bACKUP_TABLE.n_STATUS_RATE;
			return Convert.ToInt32((int)weaponStatusX.nATK * n_STATUS_RATE / 100) * OrangeConst.BP_ATK + Convert.ToInt32((int)weaponStatusX.nHP * n_STATUS_RATE / 100) * OrangeConst.BP_HP + Convert.ToInt32((int)weaponStatusX.nDEF * n_STATUS_RATE / 100) * OrangeConst.BP_DEF;
		}
		return 0;
	}

	public int GetBackupWeaponAllPower()
	{
		int num = 0;
		List<BenchInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			num += GetBackupWeaponSlotPower(list[i].netBenchInfo.BenchSlot);
		}
		return num;
	}

	public int GetCardExpansion()
	{
		return GetItemValue(OrangeConst.ITEMID_CARD_SLOT);
	}

	public int GetCardStackValue(int nCardID)
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCard != null)
		{
			return ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.Where((CardInfo x) => x.netCardInfo.CardID == nCardID).Count();
		}
		return 0;
	}

	public int GetCardDeployExpansion()
	{
		return GetItemValue(OrangeConst.CARD_DEPLOY_ITEM);
	}

	public void GoCheckRenderTextureCB(UnityEngine.Object obj)
	{
		GoCheckUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<GoCheckUI>("UI_GoCheck");
		if (uI != null)
		{
			uI.RenderTextureCB(obj);
		}
	}

	public void RetrieveRaidBossInfoCB(RetrieveRaidBossInfoRes res)
	{
		WorldBossEventUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<WorldBossEventUI>("UI_WORLDBOSSEVENT");
		if (uI != null)
		{
			uI.UpdateRetrieveRaidBossInfoRes(res);
		}
	}

	public int GetRaidBossBounes()
	{
		int num = 0;
		foreach (KeyValuePair<int, CharacterInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicCharacter)
		{
			CHARACTER_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(item.Value.netInfo.CharacterID, out value))
			{
				switch (value.n_RARITY)
				{
				case 3:
					num += item.Value.netInfo.Star * OrangeConst.RAID_BONUS_CHARA_B;
					break;
				case 4:
					num += item.Value.netInfo.Star * OrangeConst.RAID_BONUS_CHARA_A;
					break;
				case 5:
					num += item.Value.netInfo.Star * OrangeConst.RAID_BONUS_CHARA_S;
					break;
				}
			}
		}
		foreach (KeyValuePair<int, WeaponInfo> item2 in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon)
		{
			WEAPON_TABLE value2;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(item2.Value.netInfo.WeaponID, out value2))
			{
				switch (value2.n_RARITY)
				{
				case 2:
					num += item2.Value.netInfo.Star * OrangeConst.RAID_BONUS_WEAPON_C;
					break;
				case 3:
					num += item2.Value.netInfo.Star * OrangeConst.RAID_BONUS_WEAPON_B;
					break;
				case 4:
					num += item2.Value.netInfo.Star * OrangeConst.RAID_BONUS_WEAPON_A;
					break;
				case 5:
					num += item2.Value.netInfo.Star * OrangeConst.RAID_BONUS_WEAPON_S;
					break;
				}
			}
		}
		return num;
	}

	public void RetrieveRaidBossEventRankingCB(object list)
	{
		WorldBossEventUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<WorldBossEventUI>("UI_WORLDBOSSEVENT");
		if (uI != null)
		{
			uI.UpdateRetrieveRaidBossEventRankingRes(list);
		}
	}

	public void RetrieveSelfRaidBossEventRankingInfoCB(object p_param)
	{
		WorldBossEventUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<WorldBossEventUI>("UI_WORLDBOSSEVENT");
		if (uI != null)
		{
			uI.UpdateSelfEventRankingInfo((EventRankingInfo)p_param);
		}
	}

	public void RetrieveTotalWarInfoCB(RetrieveTotalWarInfoRes res)
	{
		TotalWarUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<TotalWarUI>("UI_TotalWar");
		if (uI != null)
		{
			uI.UpdateRetrieveTotalWarInfoRes(res);
		}
	}

	public void TotalWarRecordReplaceResCB(TotalWarRecordReplaceRes res)
	{
		TotalWarUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<TotalWarUI>("UI_TotalWar");
		if (uI != null)
		{
			uI.TotalWarRecordReplaceResCB(res);
		}
	}

	public void RetrieveTotalWarRankingCB(object list)
	{
		TotalWarUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<TotalWarUI>("UI_TotalWar");
		if (uI != null)
		{
			uI.UpdateRetrieveTotalWarRankingRes(list);
		}
	}

	public void RetrieveSelfTotalWarRankingInfoCB(object p_param)
	{
		TotalWarUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<TotalWarUI>("UI_TotalWar");
		if (uI != null)
		{
			uI.UpdateSelfEventRankingInfo((EventRankingInfo)p_param);
		}
	}

	public void UpdateMatchHunterRankTable(int score)
	{
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentScore = score;
		List<HUNTERRANK_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.HUNTERRANK_TABLE_DICT.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.MatchHunterRankTable = list[i];
			if (score <= list[i].n_PT_MAX)
			{
				break;
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		CheatPlayer = false;
		LockTime = 0;
	}

	public void SetUseCheatPlugin(LoginToServerRes res)
	{
		if (res != null)
		{
			if (res.CheatType != 0)
			{
				LockTime = res.CheatExpireTime;
				CheatPlayer = true;
			}
			else
			{
				CheatPlayer = false;
			}
		}
	}

	public bool GetUseCheatPlugIn()
	{
		if (!CheatPlayer)
		{
			return CheatPlayer;
		}
		long num = LockTime - MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		if (num < 0)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.RetrieveResetTime();
			num = LockTime - MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		}
		if (CheatPlayer)
		{
			return num > 0;
		}
		return false;
	}

	public string GetCheatExpireTime(int reducetime = 0)
	{
		return OrangeGameUtility.GetTimeText(LockTime - reducetime);
	}

	public bool GetOnlineStatus(int busy, out string statusMessage)
	{
		busy = ((busy == 0) ? 30 : busy);
		bool result = false;
		long num = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC - busy;
		statusMessage = string.Concat(num);
		if (busy < 30)
		{
			result = true;
			if (busy == 2)
			{
				statusMessage = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FRIENDLIST_STATUS_BATTLE");
			}
			else
			{
				statusMessage = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FRIENDLIST_STATUS_ONLINE");
			}
		}
		else if (num > 604800)
		{
			statusMessage = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FRIEND_OFFLINE_MORE_THAN_DAYS"), 7);
		}
		else if (num > 86400)
		{
			int num2 = (int)(num / 86400);
			statusMessage = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FRIEND_OFFLINE_MORE_THAN_DAYS"), num2);
		}
		else if (num > 3600)
		{
			int num3 = (int)(num / 3600);
			statusMessage = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FRIEND_OFFLINE_LESS_THAN_DAY"), num3);
		}
		else if (num > 60)
		{
			int num4 = (int)(num / 60);
			statusMessage = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FRIEND_OFFLINE_LESS_THAN_HOURS"), num4);
		}
		else
		{
			statusMessage = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STATUS_OFFLINE");
		}
		return result;
	}
}
