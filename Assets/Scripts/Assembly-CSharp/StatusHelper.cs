#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using enums;

public class StatusHelper : ManagedSingleton<StatusHelper>
{
	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public WeaponStatus GetWeaponStatus(int nTargetWeaponID, int bAddLVExp = 0, bool bAddStar = false, int[] AddProfs = null)
	{
		if (nTargetWeaponID == 0 || !ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(nTargetWeaponID))
		{
			return new WeaponStatus();
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID))
		{
			WeaponInfo tWeaponInfo = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nTargetWeaponID];
			return GetWeaponStatusX(tWeaponInfo, bAddLVExp, bAddStar, AddProfs);
		}
		return new WeaponStatus();
	}

	public WeaponStatus GetWeaponStatusX(WeaponInfo tWeaponInfo, int bAddLVExp = 0, bool bAddStar = false, int[] AddProfs = null, Action<string> debugcb = null, int NetplayerExp = -1)
	{
		WeaponStatus weaponStatus = new WeaponStatus();
		NetWeaponInfo netInfo = tWeaponInfo.netInfo;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		if (tWeaponInfo == null || netInfo == null || netInfo.WeaponID == 0 || !ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(netInfo.WeaponID))
		{
			return weaponStatus;
		}
		WEAPON_TABLE tWEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netInfo.WeaponID];
		EXP_TABLE eXP_TABLE = null;
		STAR_TABLE sTAR_TABLE = null;
		weaponStatus.nWeaponType = tWEAPON_TABLE.n_TYPE;
		Dictionary<int, EXP_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.GetEnumerator();
		bAddLVExp += netInfo.Exp;
		while (enumerator.MoveNext())
		{
			if (bAddLVExp < enumerator.Current.Value.n_TOTAL_WEAPONEXP && enumerator.Current.Value.n_TOTAL_WEAPONEXP - bAddLVExp <= enumerator.Current.Value.n_WEAPONEXP)
			{
				eXP_TABLE = enumerator.Current.Value;
				break;
			}
		}
		if (eXP_TABLE == null)
		{
			Debug.LogWarning("算武器(ID" + netInfo.WeaponID + ")數值沒有找到經驗表喔!!");
			return weaponStatus;
		}
		eXP_TABLE = ((NetplayerExp < 0) ? ManagedSingleton<OrangeTableHelper>.Instance.ReduceLVByCheckPlayerExp(ManagedSingleton<PlayerHelper>.Instance.GetExp(), eXP_TABLE) : ManagedSingleton<OrangeTableHelper>.Instance.ReduceLVByCheckPlayerExp(NetplayerExp, eXP_TABLE));
		int num7 = netInfo.Star;
		if (bAddStar && num7 < 5)
		{
			num7++;
		}
		Dictionary<int, STAR_TABLE>.Enumerator enumerator2 = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			if (enumerator2.Current.Value.n_TYPE == 2 && enumerator2.Current.Value.n_MAINID == netInfo.WeaponID && num7 == enumerator2.Current.Value.n_STAR)
			{
				sTAR_TABLE = enumerator2.Current.Value;
				break;
			}
		}
		List<UPGRADE_TABLE> list = (from p in ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT
			where p.Value.n_GROUP == tWEAPON_TABLE.n_UPGRADE
			orderby p.Value.n_LV
			select p.Value).ToList();
		float num8 = 0f;
		float num9 = 0f;
		if (sTAR_TABLE != null)
		{
			num8 = sTAR_TABLE.f_HP;
			num9 = sTAR_TABLE.f_ATK;
		}
		else
		{
			Debug.LogWarning("算武器(ID" + netInfo.WeaponID + ")數值沒有找到升星表喔!!");
		}
		num += (int)Mathf.Floor((float)eXP_TABLE.n_WEAPON_HP * (1f + num8) * tWEAPON_TABLE.f_PARAM);
		num2 += (int)Mathf.Floor((float)eXP_TABLE.n_WEAPON_ATK * (1f + num9) * tWEAPON_TABLE.f_PARAM);
		num6 += num * OrangeConst.BP_HP + num2 * OrangeConst.BP_ATK;
		num3 += eXP_TABLE.n_WEAPON_CRI;
		num4 += eXP_TABLE.n_WEAPON_HIT;
		if (tWeaponInfo.netExpertInfos != null)
		{
			for (int i = 0; i < tWeaponInfo.netExpertInfos.Count; i++)
			{
				int nExpertLV = tWeaponInfo.netExpertInfos[i].ExpertLevel;
				if (AddProfs != null && AddProfs.Length >= tWeaponInfo.netExpertInfos[i].ExpertType)
				{
					nExpertLV += AddProfs[tWeaponInfo.netExpertInfos[i].ExpertType - 1];
					bool flag = false;
					while (!flag)
					{
						if (ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT.Where((KeyValuePair<int, UPGRADE_TABLE> tU) => tU.Value.n_GROUP == tWEAPON_TABLE.n_UPGRADE && tU.Value.n_LV == nExpertLV).Count() > 0)
						{
							flag = true;
						}
						if (!flag)
						{
							nExpertLV--;
							if (nExpertLV <= 0)
							{
								nExpertLV = 0;
								flag = true;
							}
						}
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].n_LV == nExpertLV)
					{
						if (tWeaponInfo.netExpertInfos[i].ExpertType == 1)
						{
							num2 += list[j].n_ATK;
							num6 += list[j].n_HP;
						}
						else if (tWeaponInfo.netExpertInfos[i].ExpertType == 2)
						{
							num += list[j].n_HP;
							num6 += list[j].n_HP;
						}
						else if (tWeaponInfo.netExpertInfos[i].ExpertType == 4)
						{
							num4 += list[j].n_HIT;
							num6 += list[j].n_HP;
						}
						else if (tWeaponInfo.netExpertInfos[i].ExpertType == 3)
						{
							num3 += list[j].n_CRI;
							num6 += list[j].n_HP;
						}
						else if (tWeaponInfo.netExpertInfos[i].ExpertType == 5)
						{
							num5 += list[j].n_LUK;
							num6 += list[j].n_HP;
						}
						break;
					}
				}
			}
		}
		weaponStatus.nHP = num;
		weaponStatus.nATK = num2;
		weaponStatus.nCRI = num3;
		weaponStatus.nHIT = num4;
		weaponStatus.nLuck = num5;
		weaponStatus.nBattlePower = num6;
		return weaponStatus;
	}

	public WeaponStatus GetEquipStatus(int nTargetEquipID)
	{
		EQUIP_TABLE value = null;
		EquipInfo value2 = null;
		EquipEnhanceInfo value3 = null;
		if (nTargetEquipID == 0 || !ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.TryGetValue(nTargetEquipID, out value))
		{
			return new WeaponStatus();
		}
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicEquip.TryGetValue(nTargetEquipID, out value2))
		{
			return new WeaponStatus();
		}
		ManagedSingleton<PlayerNetManager>.Instance.dicEquipEnhance.TryGetValue((EquipPartType)value.n_PARTS, out value3);
		return GetEquipStatusX(value2, value3);
	}

	public WeaponStatus GetEquipStatusX(EquipInfo tEquipInfo, EquipEnhanceInfo tEquipEnhanceInfo)
	{
		WeaponStatus weaponStatus = new WeaponStatus();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (tEquipInfo == null || tEquipInfo.netEquipmentInfo == null || tEquipInfo.netEquipmentInfo.EquipmentID == 0)
		{
			return weaponStatus;
		}
		num2 += tEquipInfo.netEquipmentInfo.DefParam;
		num += tEquipInfo.netEquipmentInfo.HpParam;
		num3 += tEquipInfo.netEquipmentInfo.LukParam;
		if (tEquipEnhanceInfo != null && tEquipEnhanceInfo.netPlayerEquipInfo != null)
		{
			EXP_TABLE value = null;
			int num5 = tEquipEnhanceInfo.netPlayerEquipInfo.EnhanceLv;
			if (num5 < 0)
			{
				num5 = 0;
			}
			if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(num5, out value))
			{
				num += value.n_EQUIPUP_HP;
				num2 += value.n_EQUIPUP_DEF;
			}
		}
		num4 += num * OrangeConst.BP_HP + num2 * OrangeConst.BP_DEF;
		weaponStatus.nHP = num;
		weaponStatus.nLuck = num3;
		weaponStatus.nDEF = num2;
		weaponStatus.nBattlePower = num4;
		return weaponStatus;
	}

	public WeaponStatus GetChipStatus(int nTargetChipID, int bAddLVExp = 0, bool bAddStar = false, bool bAddAnalyse = false)
	{
		if (nTargetChipID == 0 || !ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.ContainsKey(nTargetChipID))
		{
			return new WeaponStatus();
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID))
		{
			ChipInfo tChipInfo = ManagedSingleton<PlayerNetManager>.Instance.dicChip[nTargetChipID];
			return GetChipStatusX(tChipInfo, bAddLVExp, bAddStar, bAddAnalyse);
		}
		return new WeaponStatus();
	}

	public WeaponStatus GetChipStatusX(ChipInfo tChipInfo, int bAddLVExp = 0, bool bAddStar = false, bool bAddAnalyse = false, Action<string> debugcb = null, bool? refbIsMainSubChip = null, int NetplayerExp = -1)
	{
		WeaponStatus weaponStatus = new WeaponStatus();
		NetChipInfo netChipInfo = tChipInfo.netChipInfo;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (tChipInfo == null || netChipInfo == null || netChipInfo.ChipID == 0 || !ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.ContainsKey(netChipInfo.ChipID))
		{
			return weaponStatus;
		}
		DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[netChipInfo.ChipID];
		EXP_TABLE eXP_TABLE = null;
		STAR_TABLE sTAR_TABLE = null;
		Dictionary<int, EXP_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.GetEnumerator();
		bAddLVExp += netChipInfo.Exp;
		while (enumerator.MoveNext())
		{
			if (bAddLVExp < enumerator.Current.Value.n_TOTAL_DISCEXP && enumerator.Current.Value.n_TOTAL_DISCEXP - bAddLVExp <= enumerator.Current.Value.n_DISCEXP)
			{
				eXP_TABLE = enumerator.Current.Value;
				break;
			}
		}
		if (eXP_TABLE == null)
		{
			Debug.LogWarning("算晶片(ID" + netChipInfo.ChipID + ")數值沒有找到經驗值表喔!!");
			return weaponStatus;
		}
		eXP_TABLE = ((NetplayerExp < 0) ? ManagedSingleton<OrangeTableHelper>.Instance.ReduceLVByCheckPlayerExp(ManagedSingleton<PlayerHelper>.Instance.GetExp(), eXP_TABLE) : ManagedSingleton<OrangeTableHelper>.Instance.ReduceLVByCheckPlayerExp(NetplayerExp, eXP_TABLE));
		int num5 = netChipInfo.Star;
		if (bAddStar && num5 < 5)
		{
			num5++;
		}
		Dictionary<int, STAR_TABLE>.Enumerator enumerator2 = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			if (enumerator2.Current.Value.n_TYPE == 4 && enumerator2.Current.Value.n_MAINID == netChipInfo.ChipID && num5 == enumerator2.Current.Value.n_STAR)
			{
				sTAR_TABLE = enumerator2.Current.Value;
				break;
			}
		}
		if (sTAR_TABLE != null)
		{
			num += (int)Mathf.Floor((float)eXP_TABLE.n_DISC_HP * (1f + sTAR_TABLE.f_HP) * dISC_TABLE.f_PARAM);
			num2 += (int)Mathf.Floor((float)eXP_TABLE.n_DISC_ATK * (1f + sTAR_TABLE.f_ATK) * dISC_TABLE.f_PARAM);
			num3 += (int)Mathf.Floor((float)eXP_TABLE.n_DISC_DEF * (1f + sTAR_TABLE.f_DEF) * dISC_TABLE.f_PARAM);
		}
		else
		{
			Debug.LogWarning("算晶片(ID" + netChipInfo.ChipID + ")數值沒有找到升星表表喔!!");
			num += (int)Mathf.Floor((float)eXP_TABLE.n_DISC_HP * dISC_TABLE.f_PARAM);
			num2 += (int)Mathf.Floor((float)eXP_TABLE.n_DISC_ATK * dISC_TABLE.f_PARAM);
			num3 += (int)Mathf.Floor((float)eXP_TABLE.n_DISC_DEF * dISC_TABLE.f_PARAM);
		}
		bool flag = false;
		if (!refbIsMainSubChip.HasValue)
		{
			WeaponInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID, out value) && value.netInfo.Chip == tChipInfo.netChipInfo.ChipID)
			{
				flag = true;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID, out value) && value.netInfo.Chip == tChipInfo.netChipInfo.ChipID)
			{
				flag = true;
			}
		}
		else
		{
			flag = refbIsMainSubChip ?? false;
		}
		if (!flag)
		{
			int[] array = new int[7]
			{
				0,
				OrangeConst.DISC_ANALYSE_1,
				OrangeConst.DISC_ANALYSE_2,
				OrangeConst.DISC_ANALYSE_3,
				OrangeConst.DISC_ANALYSE_4,
				OrangeConst.DISC_ANALYSE_5,
				OrangeConst.DISC_ANALYSE_5
			};
			int num6 = tChipInfo.netChipInfo.Analyse;
			if (bAddAnalyse)
			{
				num6++;
			}
			num = (int)Mathf.Floor((float)num * 0.01f * (float)array[num6]);
			num2 = (int)Mathf.Floor((float)num2 * 0.01f * (float)array[num6]);
			num3 = (int)Mathf.Floor((float)num3 * 0.01f * (float)array[num6]);
		}
		num4 += num * OrangeConst.BP_HP + num2 * OrangeConst.BP_ATK + num3 * OrangeConst.BP_DEF;
		weaponStatus.nHP = num;
		weaponStatus.nATK = num2;
		weaponStatus.nDEF = num3;
		weaponStatus.nBattlePower = num4;
		return weaponStatus;
	}

	public WeaponStatus GetMemberChipStatus(MemberInfo memberInfo, int NetplayerExp = -1)
	{
		WeaponStatus result = new WeaponStatus();
		foreach (NetChipInfo totalChip in memberInfo.netSealBattleSettingInfo.TotalChipList)
		{
			ChipInfo chipInfo = new ChipInfo();
			chipInfo.netChipInfo = totalChip;
			if (chipInfo.netChipInfo.ChipID == memberInfo.netSealBattleSettingInfo.MainWeaponInfo.Chip || chipInfo.netChipInfo.ChipID == memberInfo.netSealBattleSettingInfo.SubWeaponInfo.Chip)
			{
				result += GetChipStatusX(chipInfo, 0, false, false, null, true, NetplayerExp);
			}
			else
			{
				result += GetChipStatusX(chipInfo, 0, false, false, null, false, NetplayerExp);
			}
		}
		return result;
	}

	public WeaponStatus GetAllChipStatus()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.dicChip == null)
		{
			return new WeaponStatus();
		}
		Dictionary<int, ChipInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicChip.GetEnumerator();
		WeaponStatus result = new WeaponStatus();
		while (enumerator.MoveNext())
		{
			result += GetChipStatus(enumerator.Current.Value.netChipInfo.ChipID);
		}
		return result;
	}

	public WeaponStatus GetFinalStrikeStatus(int nWeaponFSID)
	{
		if (nWeaponFSID == 0 || !ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.ContainsKey(nWeaponFSID))
		{
			return new WeaponStatus();
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.ContainsKey(nWeaponFSID))
		{
			FinalStrikeInfo tFinalStrikeInfo = ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike[nWeaponFSID];
			return GetFinalStrikeStatusX(tFinalStrikeInfo);
		}
		return new WeaponStatus();
	}

	public WeaponStatus GetFinalStrikeStatusX(FinalStrikeInfo tFinalStrikeInfo)
	{
		WeaponStatus weaponStatus = new WeaponStatus();
		if (tFinalStrikeInfo == null)
		{
			return weaponStatus;
		}
		ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.GetEnumerator();
		IEnumerable<KeyValuePair<int, FS_TABLE>> source = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Where((KeyValuePair<int, FS_TABLE> obj) => obj.Value.n_FS_ID == tFinalStrikeInfo.netFinalStrikeInfo.FinalStrikeID && obj.Value.n_LV == tFinalStrikeInfo.netFinalStrikeInfo.Level);
		if (source.Count() == 0)
		{
			return weaponStatus;
		}
		IEnumerable<KeyValuePair<int, STAR_TABLE>> source2 = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Where((KeyValuePair<int, STAR_TABLE> obj) => obj.Value.n_TYPE == 3 && obj.Value.n_MAINID == 1 && obj.Value.n_STAR == tFinalStrikeInfo.netFinalStrikeInfo.Star);
		if (source2.Count() == 0)
		{
			return weaponStatus;
		}
		FS_TABLE value = source.ElementAt(0).Value;
		STAR_TABLE value2 = source2.ElementAt(0).Value;
		weaponStatus.nATK = (int)weaponStatus.nATK + (int)Mathf.Floor((float)value.n_ATK * (1f + value2.f_ATK));
		weaponStatus.nHP = (int)weaponStatus.nHP + (int)Mathf.Floor((float)value.n_HP * (1f + value2.f_HP));
		weaponStatus.nDEF = (int)weaponStatus.nDEF + (int)Mathf.Floor((float)value.n_DEF * (1f + value2.f_DEF));
		return weaponStatus;
	}

	public WeaponStatus GetAllFSStatus()
	{
		WeaponStatus result = new WeaponStatus();
		if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike == null)
		{
			return result;
		}
		foreach (KeyValuePair<int, FinalStrikeInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike)
		{
			result += GetFinalStrikeStatusX(item.Value);
		}
		return result;
	}

	public PlayerStatus GetPlayerStatus()
	{
		new PlayerStatus();
		return GetPlayerStatusX(ManagedSingleton<PlayerHelper>.Instance.GetExp());
	}

	public PlayerStatus GetIllustrationStatus(NetSealBattleSettingInfo netSealedInfo)
	{
		if (netSealedInfo == null || netSealedInfo.GalleryLevelList.Count <= 1)
		{
			return new PlayerStatus();
		}
		int characterGalleryLevel = 0;
		int weaponGalleryLevel = 0;
		int cardGalleryLevel = 0;
		foreach (GalleryType value in Enum.GetValues(typeof(GalleryType)))
		{
			if (netSealedInfo.GalleryLevelList.Count > (int)value)
			{
				switch (value)
				{
				case GalleryType.Character:
					characterGalleryLevel = netSealedInfo.GalleryLevelList[(int)value];
					break;
				case GalleryType.Weapon:
					weaponGalleryLevel = netSealedInfo.GalleryLevelList[(int)value];
					break;
				case GalleryType.Card:
					cardGalleryLevel = netSealedInfo.GalleryLevelList[(int)value];
					break;
				}
			}
		}
		return GetIllustrationStatus(characterGalleryLevel, weaponGalleryLevel, cardGalleryLevel);
	}

	public PlayerStatus GetIllustrationStatus(int characterGalleryLevel = 0, int weaponGalleryLevel = 0, int cardGalleryLevel = 0)
	{
		if (characterGalleryLevel == 0)
		{
			characterGalleryLevel = ManagedSingleton<GalleryHelper>.Instance.GalleryGetCharactersExp().m_lv;
		}
		if (weaponGalleryLevel == 0)
		{
			weaponGalleryLevel = ManagedSingleton<GalleryHelper>.Instance.GalleryGetWeaponsExp().m_lv;
		}
		if (cardGalleryLevel == 0)
		{
			cardGalleryLevel = ManagedSingleton<GalleryHelper>.Instance.GalleryGetCardsExp().m_lv;
		}
		int[] obj = new int[3] { characterGalleryLevel, weaponGalleryLevel, cardGalleryLevel };
		PlayerStatus playerStatus = new PlayerStatus();
		int[] array = obj;
		foreach (int key in array)
		{
			EXP_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(key, out value))
			{
				playerStatus.nATK = (int)playerStatus.nATK + value.n_GALLERY_ATK;
				playerStatus.nHP = (int)playerStatus.nHP + value.n_GALLERY_HP;
				playerStatus.nDEF = (int)playerStatus.nDEF + value.n_GALLERY_DEF;
			}
		}
		return playerStatus;
	}

	public PlayerStatus GetBackupWeaponStatus(bool bOwner, List<NetBenchInfo> NetBenchInfoList = null, List<NetWeaponInfo> NetWeaponInfoList = null, List<NetWeaponExpertInfo> NetWeaponExpertInfoList = null, List<NetWeaponSkillInfo> NetWeaponSkillInfoList = null)
	{
		PlayerStatus playerStatus = new PlayerStatus();
		if (bOwner)
		{
			NetBenchInfoList = ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.Values.Select((BenchInfo x) => x.netBenchInfo).ToList();
			NetWeaponInfoList = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Values.Select((WeaponInfo x) => x.netInfo).ToList();
		}
		List<NetBenchInfo> list = NetBenchInfoList;
		if (list == null || NetWeaponInfoList == null)
		{
			return playerStatus;
		}
		for (int i = 0; i < list.Count; i++)
		{
			int slot = list[i].BenchSlot;
			int lv = list[i].Level;
			int wid = list[i].WeaponID;
			if (wid <= 0)
			{
				continue;
			}
			BACKUP_TABLE bACKUP_TABLE = null;
			List<BACKUP_TABLE> list2 = (from p in ManagedSingleton<OrangeDataManager>.Instance.BACKUP_TABLE_DICT
				where p.Value.n_SLOT == slot && p.Value.n_SLOT_LV == lv
				select p into o
				select o.Value).ToList();
			if (list2.Count <= 0)
			{
				continue;
			}
			bACKUP_TABLE = list2[0];
			WeaponInfo weaponInfo = null;
			if (bOwner)
			{
				weaponInfo = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[wid];
			}
			else
			{
				weaponInfo = new WeaponInfo();
				weaponInfo.netInfo = NetWeaponInfoList.Find((NetWeaponInfo x) => x.WeaponID == wid);
				weaponInfo.netExpertInfos = NetWeaponExpertInfoList.Where((NetWeaponExpertInfo x) => x.WeaponID == wid).ToList();
				weaponInfo.netSkillInfos = NetWeaponSkillInfoList.Where((NetWeaponSkillInfo x) => x.WeaponID == wid).ToList();
			}
			WeaponStatus weaponStatusX = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(weaponInfo, 0, false, null, delegate
			{
			});
			int n_STATUS_RATE = bACKUP_TABLE.n_STATUS_RATE;
			playerStatus.nATK = (int)playerStatus.nATK + (int)Mathf.Floor((float)(int)weaponStatusX.nATK * 0.01f * (float)n_STATUS_RATE);
			playerStatus.nHP = (int)playerStatus.nHP + (int)Mathf.Floor((float)(int)weaponStatusX.nHP * 0.01f * (float)n_STATUS_RATE);
			playerStatus.nDEF = (int)playerStatus.nDEF + (int)Mathf.Floor((float)(int)weaponStatusX.nDEF * 0.01f * (float)n_STATUS_RATE);
		}
		return playerStatus;
	}

	public PlayerStatus GetCardSystemStatus(bool bOwner, int CharacterID, List<NetCharacterInfo> NetCharacterInfoList = null, List<NetCardInfo> NetCardInfoList = null, List<NetCharacterCardSlotInfo> NetCharacterCardSlotInfoList = null)
	{
		PlayerStatus playerStatus = new PlayerStatus();
		if (bOwner)
		{
			CharacterInfo value = null;
			NetCharacterInfoList = new List<NetCharacterInfo>();
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(CharacterID, out value))
			{
				NetCharacterInfoList.Add(value.netInfo);
			}
			NetCardInfoList = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.Select((CardInfo x) => x.netCardInfo).ToList();
			List<NetCharacterCardSlotInfo> list = new List<NetCharacterCardSlotInfo>();
			for (int i = 0; i < NetCharacterInfoList.Count; i++)
			{
				int characterID = NetCharacterInfoList[i].CharacterID;
				Dictionary<int, NetCharacterCardSlotInfo> value2 = null;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.TryGetValue(characterID, out value2))
				{
					list.AddRange(value2.Values.ToList());
				}
			}
			NetCharacterCardSlotInfoList = list;
		}
		if (NetCardInfoList == null || NetCharacterCardSlotInfoList == null || NetCharacterInfoList == null)
		{
			return playerStatus;
		}
		Dictionary<int, CardInfo> dictionary = new Dictionary<int, CardInfo>();
		foreach (NetCardInfo NetCardInfo in NetCardInfoList)
		{
			dictionary.Value(NetCardInfo.CardSeqID).netCardInfo = NetCardInfo;
		}
		Dictionary<int, Dictionary<int, NetCharacterCardSlotInfo>> dictionary2 = new Dictionary<int, Dictionary<int, NetCharacterCardSlotInfo>>();
		if (NetCharacterCardSlotInfoList != null)
		{
			foreach (NetCharacterCardSlotInfo NetCharacterCardSlotInfo in NetCharacterCardSlotInfoList)
			{
				dictionary2.Value(NetCharacterCardSlotInfo.CharacterID).Value(NetCharacterCardSlotInfo.CharacterCardSlot).CardSeqID = NetCharacterCardSlotInfo.CardSeqID;
				dictionary2.Value(NetCharacterCardSlotInfo.CharacterID).Value(NetCharacterCardSlotInfo.CharacterCardSlot).CharacterCardSlot = NetCharacterCardSlotInfo.CharacterCardSlot;
				dictionary2.Value(NetCharacterCardSlotInfo.CharacterID).Value(NetCharacterCardSlotInfo.CharacterCardSlot).CharacterID = NetCharacterCardSlotInfo.CharacterID;
			}
		}
		if (dictionary2.ContainsKey(CharacterID))
		{
			List<NetCharacterCardSlotInfo> list2 = dictionary2[CharacterID].Values.ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				int cardSeqID = list2[j].CardSeqID;
				if (dictionary.ContainsKey(cardSeqID))
				{
					NetCardInfo netCardInfo = dictionary[cardSeqID].netCardInfo;
					EXP_TABLE cardExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(netCardInfo.Exp);
					CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[netCardInfo.CardID];
					playerStatus.nATK = (int)playerStatus.nATK + (int)Mathf.Floor((float)cardExpTable.n_CARD_ATK * cARD_TABLE.f_PARAM_ATK * (1f + (float)netCardInfo.Star * cARD_TABLE.f_RANKUP));
					playerStatus.nHP = (int)playerStatus.nHP + (int)Mathf.Floor((float)cardExpTable.n_CARD_HP * cARD_TABLE.f_PARAM_HP * (1f + (float)netCardInfo.Star * cARD_TABLE.f_RANKUP));
					playerStatus.nDEF = (int)playerStatus.nDEF + (int)Mathf.Floor((float)cardExpTable.n_CARD_DEF * cARD_TABLE.f_PARAM_DEF * (1f + (float)netCardInfo.Star * cARD_TABLE.f_RANKUP));
				}
			}
		}
		return playerStatus;
	}

	public PlayerStatus GetPlayerStatusWithEquip()
	{
		new PlayerStatus();
		return GetPlayerStatusX(ManagedSingleton<PlayerHelper>.Instance.GetExp()) + GetIllustrationStatus() + GetAllFSStatus() + GetAllEquipStatus() + GetBackupWeaponStatus(true) + GetCardSystemStatus(true, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara) + GetSkinStatus(ManagedSingleton<PlayerNetManager>.Instance.mmapSkin.Values);
	}

	public PlayerStatus GetAllEquipStatus()
	{
		PlayerStatus result = new PlayerStatus();
		KeyValuePair<int, EquipInfo>[] array = ManagedSingleton<PlayerNetManager>.Instance.dicEquip.Where((KeyValuePair<int, EquipInfo> obj) => obj.Value.netEquipmentInfo.Equip != 0).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			EquipInfo value = array[i].Value;
			EquipEnhanceInfo value2 = null;
			EQUIP_TABLE value3 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.TryGetValue(value.netEquipmentInfo.EquipItemID, out value3))
			{
				ManagedSingleton<PlayerNetManager>.Instance.dicEquipEnhance.TryGetValue((EquipPartType)value3.n_PARTS, out value2);
				result += GetEquipStatusX(value, value2);
			}
		}
		return result;
	}

	public PlayerStatus GetSkinStatus(List<NetCharacterSkinInfo> tNetCharacterSkinInfo)
	{
		PlayerStatus playerStatus = new PlayerStatus();
		for (int num = tNetCharacterSkinInfo.Count - 1; num >= 0; num--)
		{
			SKIN_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(tNetCharacterSkinInfo[num].SkinId, out value))
			{
				playerStatus.nHP = (int)playerStatus.nHP + value.n_HP;
				playerStatus.nATK = (int)playerStatus.nATK + value.n_ATK;
				playerStatus.nDEF = (int)playerStatus.nDEF + value.n_DEF;
			}
		}
		return playerStatus;
	}

	public PlayerStatus GetPlayerStatusX(int nExp)
	{
		PlayerStatus playerStatus = new PlayerStatus();
		EXP_TABLE expTable = ManagedSingleton<OrangeTableHelper>.Instance.GetExpTable(nExp);
		playerStatus.nLV = expTable.n_ID;
		playerStatus.nHP = expTable.n_RANK_HP;
		playerStatus.nATK = expTable.n_RANK_ATK;
		playerStatus.nDEF = expTable.n_RANK_DEF;
		playerStatus.nDOD = expTable.n_RANK_DOD;
		return playerStatus;
	}

	public PlayerStatus GetPlayerFinalStatus()
	{
		PlayerStatus playerStatus = GetPlayerStatus();
		WeaponStatus weaponStatus = GetWeaponStatus(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID);
		PlayerStatus playerStatus2 = playerStatus + weaponStatus;
		weaponStatus = GetWeaponStatus(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID);
		PlayerStatus playerStatus3 = playerStatus2 + weaponStatus;
		weaponStatus = GetAllChipStatus();
		return playerStatus3 + weaponStatus + GetIllustrationStatus() + GetAllFSStatus() + GetAllEquipStatus() + GetBackupWeaponStatus(true) + GetCardSystemStatus(true, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara) + GetSkinStatus(ManagedSingleton<PlayerNetManager>.Instance.mmapSkin.Values);
	}

	public WeaponStatus GetMemberEquipStatus(MemberInfo p_memberInfo)
	{
		WeaponStatus result = new WeaponStatus();
		for (int i = 0; i < p_memberInfo.netSealBattleSettingInfo.EquipmentList.Count; i++)
		{
			EquipInfo equipInfo = new EquipInfo();
			equipInfo.netEquipmentInfo = p_memberInfo.netSealBattleSettingInfo.EquipmentList[i];
			EquipEnhanceInfo equipEnhanceInfo = new EquipEnhanceInfo();
			EQUIP_TABLE equipTable = null;
			if (p_memberInfo.netSealBattleSettingInfo != null && equipInfo.netEquipmentInfo != null && ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(equipInfo.netEquipmentInfo.EquipItemID, out equipTable))
			{
				NetPlayerEquipInfo netPlayerEquipInfo = p_memberInfo.netSealBattleSettingInfo.PlayerEquipList.FirstOrDefault((NetPlayerEquipInfo x) => x.Slot == equipTable.n_PARTS);
				equipEnhanceInfo.netPlayerEquipInfo = netPlayerEquipInfo;
			}
			result += GetEquipStatusX(equipInfo, equipEnhanceInfo);
		}
		return result;
	}
}
