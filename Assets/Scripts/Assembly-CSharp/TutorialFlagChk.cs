using System.Collections.Generic;
using CallbackDefs;

public class TutorialFlagChk : MonoBehaviourSingleton<TutorialFlagChk>
{
	private enum HARD_FIX_SAVEPOINT
	{
		STAGE_1_1 = 10001,
		GACHA_FIRST = 10701,
		STAGE_1_2 = 12001,
		WEAPON_FIRST_UPGRADE = 12901,
		STAGE_1_3 = 14001,
		STAGE_1_4 = 15001,
		RESEARCH_FIELD = 15601,
		RESEARCH_FIRST = 15801,
		RESEARCH_COMPLETE = 16301,
		WEAPON_FIRST_UNLOCK = 16800,
		CHARACTER_SKILL_FIRST_LVUP = 19001,
		WEAPON_FIRST_ADVANCED = 21001,
		CHARACTER_FIRST_STAR_LVUP = 24001,
		CHIP_FIRST_UNLOCK = 27001,
		FINAL_STRIKE_FIRST_LVUP = 110001,
		EQUIP_FIRST_COMPOSE = 130001,
		EQUIP_FIRST_LVUP = 160001
	}

	private const int SABER_ID = 101001;

	private const int SABER_PIECE_ID = 601001;

	public void SetTutorialAllFlag(Callback p_cb)
	{
		if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLogin)
		{
			return;
		}
		List<TUTORIAL_TABLE> listTutorialBySave = ManagedSingleton<PlayerHelper>.Instance.GetListTutorialBySave();
		Queue<int> queue = new Queue<int>();
		foreach (TUTORIAL_TABLE item in listTutorialBySave)
		{
			if (UseHardFix(item))
			{
				queue.Enqueue(item.n_SAVE);
			}
		}
		SetAllFlg(queue, p_cb);
	}

	private void SetAllFlg(Queue<int> p_quque, Callback p_cb)
	{
		if (p_quque.Count > 0)
		{
			ManagedSingleton<PlayerNetManager>.Instance.TurtorialFlagRq(p_quque.Dequeue(), delegate
			{
				SetAllFlg(p_quque, p_cb);
			});
		}
		else
		{
			p_cb.CheckTargetToInvoke();
		}
	}

	private bool UseHardFix(TUTORIAL_TABLE p_table)
	{
		if (ManagedSingleton<PlayerHelper>.Instance.GetLV() < p_table.n_PRE)
		{
			return false;
		}
		switch ((HARD_FIX_SAVEPOINT)p_table.n_SAVE)
		{
		case HARD_FIX_SAVEPOINT.STAGE_1_1:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.Count >= 1)
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.GACHA_FIRST:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicGacha.Count >= 1)
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.STAGE_1_2:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.Count >= 2)
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.WEAPON_FIRST_UPGRADE:
			foreach (KeyValuePair<int, WeaponInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon)
			{
				if (item.Value.netInfo != null && item.Value.netInfo.Exp > 0)
				{
					return true;
				}
			}
			break;
		case HARD_FIX_SAVEPOINT.STAGE_1_3:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.Count >= 3)
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.STAGE_1_4:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.Count >= 4)
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.RESEARCH_FIELD:
		{
			SortedDictionary<int, NetResearchInfo> dicResearch = ManagedSingleton<PlayerNetManager>.Instance.researchInfo.dicResearch;
			if (dicResearch != null && dicResearch.Count > 0)
			{
				return true;
			}
			break;
		}
		case HARD_FIX_SAVEPOINT.RESEARCH_FIRST:
		{
			List<NetResearchRecord> listResearchRecord = ManagedSingleton<PlayerNetManager>.Instance.researchInfo.listResearchRecord;
			if (listResearchRecord != null && listResearchRecord.Count > 0)
			{
				return true;
			}
			break;
		}
		case HARD_FIX_SAVEPOINT.RESEARCH_COMPLETE:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(601001))
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.WEAPON_FIRST_UNLOCK:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(101001))
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.CHARACTER_SKILL_FIRST_LVUP:
			foreach (CharacterInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Values)
			{
				foreach (NetCharacterSkillInfo value2 in value.netSkillDic.Values)
				{
					if (value2.Level > 1)
					{
						return true;
					}
				}
			}
			break;
		case HARD_FIX_SAVEPOINT.WEAPON_FIRST_ADVANCED:
			foreach (WeaponInfo value3 in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Values)
			{
				if (value3.netExpertInfos == null)
				{
					continue;
				}
				for (int i = 0; i < value3.netExpertInfos.Count; i++)
				{
					if (value3.netExpertInfos[i].ExpertLevel > 0)
					{
						return true;
					}
				}
			}
			break;
		case HARD_FIX_SAVEPOINT.CHARACTER_FIRST_STAR_LVUP:
			foreach (CharacterInfo value4 in ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Values)
			{
				if (value4.netInfo != null && value4.netInfo.Star > 0)
				{
					return true;
				}
			}
			break;
		case HARD_FIX_SAVEPOINT.CHIP_FIRST_UNLOCK:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.Count > 0)
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.FINAL_STRIKE_FIRST_LVUP:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.Count <= 0)
			{
				break;
			}
			foreach (FinalStrikeInfo value5 in ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.Values)
			{
				if (value5.netFinalStrikeInfo.Level >= 2)
				{
					return true;
				}
			}
			break;
		case HARD_FIX_SAVEPOINT.EQUIP_FIRST_COMPOSE:
			if (ManagedSingleton<PlayerNetManager>.Instance.dicEquip.Count > 0)
			{
				return true;
			}
			break;
		case HARD_FIX_SAVEPOINT.EQUIP_FIRST_LVUP:
			foreach (EquipEnhanceInfo value6 in ManagedSingleton<PlayerNetManager>.Instance.dicEquipEnhance.Values)
			{
				if (value6.netPlayerEquipInfo != null && value6.netPlayerEquipInfo.EnhanceLv > 0)
				{
					return true;
				}
			}
			break;
		}
		return false;
	}
}
