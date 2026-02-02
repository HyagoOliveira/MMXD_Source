using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;
using enums;

public class PerBuffManager
{
	public enum BUFF_TYPE
	{
		NONE = 0,
		BUFF_ADD_ATK_DMG = 1,
		BUFF_REDUCE_DMG = 2,
		BUFF_ADD_CRI_PERCENT = 3,
		BUFF_ADD_CRI_DMG = 4,
		BUFF_ADD_MOVESPEED = 5,
		BUFF_ENERGY_SHIELD = 6,
		BUFF_UNBREAK = 7,
		BUFF_PREVENT_DEBUFF = 8,
		BUFF_GAIN_PASSIVE_SKILL = 11,
		BUFF_HEAL_ENHANCE = 13,
		DEBUFF_DE_ATK_DMG = 101,
		DEBUFF_INCREASE_DMG = 102,
		DEBUFF_DE_CRI_PERCENT = 103,
		DEBUFF_DE_CRI_DMG = 104,
		DEBUFF_DE_MOVESPEED = 105,
		DEBUFF_PERIOD_DMG = 106,
		DEBUFF_STOP_ACT = 107,
		DEBUFF_STOP_WEAPON = 108,
		DEBUFF_SKILLDMG_ADD = 109,
		DEBUFF_BAN_WEAPON = 110,
		DEBUFF_BAN_SKILL = 111,
		DEBUFF_ANTI_BUFF = 112,
		DEBUFF_MEASURE_REDUCE = 113,
		DEBUFF_DE_MOVESPEED_SP = 114,
		DEBUFF_MARKED = 115,
		DEBUFF_BLIND = 116,
		DEBUFF_AXL_VALENTINE_MARKED = 117,
		DEBUFF_REVERSE_RIGHT_AND_LEFT = 118,
		DEBUFF_BAN_AUTO_AIM = 119,
		DEBUFF_BLACK = 120,
		DEBUFF_NOMOVE = 121,
		DEBUFF_MISS = 122,
		DEBUFF_SCARE = 123,
		DEBUFF_HEAL_WORSEN = 124,
		DEBUFF_END = 125
	}

	public enum BUFF_CONDITION_TARGET
	{
		TARGET = 0,
		SELF = 1
	}

	public enum CHECH_BUFF_TYPE
	{
		BUFF = 0,
		DEBUFF = 1,
		ALL = 2
	}

	public enum ADD_BUFF_BIT_PARAM
	{
		None = 0,
		CheckIgnoreWaitNetSync = 1,
		AddNoCheckCollider = 2,
		playSE = 4
	}

	public class BuffStatus
	{
		public float fAtkDmgPercent;

		public float fCriDmgPercent;

		public float fCriPercent;

		public float fMoveSpeed;

		public float fReduceDmgPercent;

		public float fMissPercent;

		public int nEnergyShield;

		public int nEnergyShieldMax;

		public int nLaskBlackStack;

		public int nHealEnhance;

		public PerBuffManager refPBM;

		public RefPassiveskill refPS;

		public int nHP
		{
			get
			{
				if (refPBM == null || refPBM.tSOB == null)
				{
					return 0;
				}
				return refPBM.tSOB.Hp;
			}
		}

		public int nMAXHP
		{
			get
			{
				if (refPBM == null || refPBM.tSOB == null)
				{
					return 0;
				}
				return refPBM.tSOB.MaxHp;
			}
		}
	}

	public List<PerBuff> listBuffs = new List<PerBuff>();

	private BuffStatus m_sBuffStatus = new BuffStatus();

	private List<FxObjLink> FxObjLinks = new List<FxObjLink>();

	private StageObjBase tSOB;

	public int nMeasureNow;

	public int nMeasureMax;

	private float fAddMeasureTime;

	private float fLastAddMeasueTime;

	private bool bUpdateBarUI;

	private bool bNeedCalcuStatus;

	private bool bTmpBool;

	private PerBuff tUpdatePerBuff;

	private PerBuff tCpyPerBuff;

	private PerBuff tAddPerBuff;

	public float fInitAtkDmgPercent;

	private HurtPassParam tHurtPassParam = new HurtPassParam();

	private const float minSpdRate = -100f;

	public BuffStatus sBuffStatus
	{
		get
		{
			return m_sBuffStatus;
		}
	}

	public StageObjBase SOB
	{
		get
		{
			return tSOB;
		}
	}

	public event Action<PerBuffManager> UpdateBuffBar;

	public void Init(StageObjBase SetSOB)
	{
		tSOB = SetSOB;
		if (tSOB.tRefPassiveskill == null)
		{
			tSOB.tRefPassiveskill = new RefPassiveskill();
		}
		sBuffStatus.refPS = tSOB.tRefPassiveskill;
		nMeasureMax = sBuffStatus.refPS.nMeasureMaxValue;
		nMeasureNow = sBuffStatus.refPS.nMeasureInitValue;
		fAddMeasureTime = (float)sBuffStatus.refPS.nAddMeasureTime * 0.001f;
		fLastAddMeasueTime = 0f;
		m_sBuffStatus.refPBM = this;
		StopLoopSE();
		ClearBuff();
		for (int i = 0; i < FxObjLinks.Count; i++)
		{
			if (FxObjLinks[i].tObj != null)
			{
				FxObjLinks[i].tObj.SetActive(false);
			}
		}
		FxObjLinks.Clear();
		fInitAtkDmgPercent = 0f;
		if (BattleInfoUI.Instance != null)
		{
			OrangeCharacter orangeCharacter = tSOB as OrangeCharacter;
			foreach (EVENT_TABLE item in BattleInfoUI.Instance.tNowEvent)
			{
				switch ((SpType)(short)item.n_SP_TYPE)
				{
				case SpType.SP_WEAPON:
				{
					if (!(orangeCharacter != null))
					{
						break;
					}
					int[] weaponList = orangeCharacter.SetPBP.WeaponList;
					foreach (int num in weaponList)
					{
						if (item.n_SP_ID == num)
						{
							fInitAtkDmgPercent += item.n_BONUS_RATE - 100;
						}
					}
					break;
				}
				case SpType.SP_CHARACTER:
					if (orangeCharacter != null && item.n_SP_ID == orangeCharacter.SetPBP.CharacterID)
					{
						fInitAtkDmgPercent += item.n_BONUS_RATE - 100;
					}
					break;
				}
			}
		}
		CalcuStatus();
		bUpdateBarUI = true;
	}

	private void Check_Skill_ICON(OrangeCharacter mOC, CONDITION_TABLE mCT, bool isADD)
	{
		if (mOC == null || mCT == null || (mCT.n_EFFECT < 1000 && mCT.n_LINK == 0) || !mOC.IsLocalPlayer || mOC.UsingVehicle || !(mOC != null))
		{
			return;
		}
		if (mOC.PlayerSkills[0].BulletData.n_COMBO_SKILL == (int)mCT.f_EFFECT_X || (mOC.PlayerSkills[0].BulletData.n_COMBO_SKILL == mCT.n_LINK && mOC.PlayerSkills[0].BulletData.n_COMBO_SKILL != 0))
		{
			SKILL_TABLE sKILL_TABLE = ((!isADD) ? mOC.PlayerSkills[0].BulletData : ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[mOC.PlayerSkills[0].BulletData.n_COMBO_SKILL]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(sKILL_TABLE.s_ICON), sKILL_TABLE.s_ICON, delegate(Sprite obj)
			{
				if (obj != null)
				{
					mOC.ForceChangeSkillIcon(1, obj);
				}
			});
		}
		else
		{
			if (mOC.PlayerSkills[1].BulletData.n_COMBO_SKILL != (int)mCT.f_EFFECT_X && mOC.PlayerSkills[1].BulletData.n_COMBO_SKILL != mCT.n_LINK)
			{
				return;
			}
			SKILL_TABLE value;
			if (isADD)
			{
				ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(mOC.PlayerSkills[1].BulletData.n_COMBO_SKILL, out value);
			}
			else
			{
				value = mOC.PlayerSkills[1].BulletData;
			}
			if (value == null)
			{
				return;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON, delegate(Sprite obj)
			{
				if (obj != null)
				{
					mOC.ForceChangeSkillIcon(2, obj);
				}
			});
		}
	}

	public void UpdateBuffTime()
	{
		sBuffStatus.refPS.fNowTotalLeftTime += GameLogicUpdateManager.m_fFrameLen;
		if (nMeasureMax > 0)
		{
			if (nMeasureMax > nMeasureNow)
			{
				fLastAddMeasueTime += GameLogicUpdateManager.m_fFrameLen;
				while (fLastAddMeasueTime > fAddMeasureTime)
				{
					nMeasureNow++;
					fLastAddMeasueTime -= fAddMeasureTime;
					bUpdateBarUI = true;
				}
				if (nMeasureNow >= nMeasureMax)
				{
					nMeasureNow = nMeasureMax;
					fLastAddMeasueTime = 0f;
				}
			}
			else if (fLastAddMeasueTime > 0f)
			{
				fLastAddMeasueTime = 0f;
				bUpdateBarUI = true;
			}
			PerBuff perBuff = null;
			if (nMeasureNow > 0 && CheckHasEffect(113, out perBuff) && perBuff.fLeftTime >= perBuff.refCTable.f_EFFECT_Z)
			{
				perBuff.fLeftTime = 0f;
				nMeasureNow--;
				bUpdateBarUI = true;
				if (nMeasureNow <= 0)
				{
					RemoveBuffByCONDITIONID(perBuff.nBuffID);
					int num = (int)perBuff.refCTable.f_EFFECT_X;
					if (num > 0)
					{
						CheckDelBuffTrigger(perBuff.nBuffID, 1);
						RemoveBuffByCONDITIONID(num);
					}
				}
			}
		}
		if (listBuffs.Count > 0)
		{
			bool flag = false;
			if (tSOB.GetSOBType() == 1 && !(tSOB as OrangeCharacter).bNeedUpdateAlways && StageUpdate.gbIsNetGame)
			{
				flag = true;
			}
			for (int num2 = listBuffs.Count - 1; num2 >= 0; num2--)
			{
				tUpdatePerBuff = listBuffs[num2];
				if (tUpdatePerBuff.bWaitNetDel)
				{
					return;
				}
				if (tUpdatePerBuff.bWaitNetSyncTime && flag)
				{
					tUpdatePerBuff.fWaitNetSyncTimeOut -= GameLogicUpdateManager.m_fFrameLen;
					if (tUpdatePerBuff.fWaitNetSyncTimeOut <= 0f)
					{
						tUpdatePerBuff.bWaitNetSyncTime = false;
					}
					return;
				}
				tUpdatePerBuff.fDuration -= GameLogicUpdateManager.m_fFrameLen;
				tUpdatePerBuff.fLeftTime += GameLogicUpdateManager.m_fFrameLen;
				if (tUpdatePerBuff.refCTable.n_EFFECT == 106 && tUpdatePerBuff.fLeftTime >= tUpdatePerBuff.refCTable.f_EFFECT_Z)
				{
					tUpdatePerBuff.fLeftTime -= tUpdatePerBuff.refCTable.f_EFFECT_Z;
					if (tSOB != null && (int)tSOB.Hp > 0)
					{
						if (StageUpdate.gbIsNetGame)
						{
							if (CheckCalcuNetGameDmg())
							{
								string playerName = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerName(tUpdatePerBuff.sPlayerID);
								if ((bool)tSOB.IsUnBreak() || (bool)tSOB.IsUnBreakX() || tSOB.IsInvincible)
								{
									tHurtPassParam.dmg = 0;
								}
								else
								{
									tHurtPassParam.dmg = tUpdatePerBuff.nStack;
								}
								tHurtPassParam.owner = playerName;
								tHurtPassParam.nSubPartID = 0;
								tHurtPassParam.SetIsPlayer(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPlayerID(tUpdatePerBuff.sPlayerID));
								BulletBase.tNetDmgStack.sPlayerID = tSOB.sNetSerialID;
								BulletBase.tNetDmgStack.sShotPlayerID = tUpdatePerBuff.sPlayerID;
								BulletBase.tNetDmgStack.nDmg = tUpdatePerBuff.nStack;
								BulletBase.tNetDmgStack.nRecordID = tSOB.GetNowRecordNO();
								BulletBase.tNetDmgStack.nNetID = 0;
								BulletBase.tNetDmgStack.sOwner = playerName;
								BulletBase.tNetDmgStack.nSubPartID = 0;
								BulletBase.tNetDmgStack.nDamageType = 0;
								BulletBase.tNetDmgStack.nWeaponCheck = 0;
								BulletBase.tNetDmgStack.nWeaponType = 0;
								BulletBase.tNetDmgStack.nHP = tSOB.Hp;
								BulletBase.tNetDmgStack.nEnergyShield = tSOB.selfBuffManager.sBuffStatus.nEnergyShield;
								BulletBase.tNetDmgStack.nBreakEnergyShieldBuffID = 0;
								tSOB.Hurt(tHurtPassParam);
								BulletBase.tNetDmgStack.nHP = (int)tSOB.MaxHp - (int)tSOB.DmgHp + (int)tSOB.HealHp;
								BulletBase.tNetDmgStack.nEndHP = tSOB.Hp;
								StageObjBase stageObjBase = tSOB;
								stageObjBase.DmgHp = (int)stageObjBase.DmgHp + ((int)BulletBase.tNetDmgStack.nHP - (int)tSOB.Hp);
								BulletBase.tNetDmgStack.nDmgHP = tSOB.DmgHp;
								BulletBase.tNetDmgStack.nHealHP = tSOB.HealHp;
								BulletBase.tNetDmgStack.nSkillID = 0;
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, tSOB.GetDamageTextPos(), (int)BulletBase.tNetDmgStack.nHP - (int)BulletBase.tNetDmgStack.nEndHP, tSOB.GetSOBLayerMask(), VisualDamage.DamageType.Normal);
								StageUpdate.SyncStageObj(0, 0, JsonConvert.SerializeObject(BulletBase.tNetDmgStack, Formatting.None, new ObscuredValueConverter()), true);
								CheckAddPlayerDamage();
								if ((int)tSOB.Hp <= 0)
								{
									if ((bool)(tSOB as OrangeCharacter))
									{
										MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerKillNum(tUpdatePerBuff.sPlayerID);
									}
									else if ((bool)(tSOB as EnemyControllerBase))
									{
										MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerKillEnemyNum(tUpdatePerBuff.sPlayerID);
										BattleInfoUI.Instance.UpdateKillScoreUI((tSOB as EnemyControllerBase).EnemyData.n_SCORE);
										if (StageResManager.GetStageUpdate() != null)
										{
											StageResManager.GetStageUpdate().TriggerStageQuest(null, null, tSOB.selfBuffManager);
										}
									}
									num2 = -1;
								}
							}
						}
						else
						{
							if ((bool)tSOB.IsUnBreak() || (bool)tSOB.IsUnBreakX() || tSOB.IsInvincible)
							{
								tHurtPassParam.dmg = 0;
							}
							else
							{
								tHurtPassParam.dmg = tUpdatePerBuff.nStack;
							}
							tHurtPassParam.nSubPartID = 0;
							tHurtPassParam.SetIsPlayer(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPlayerID(tUpdatePerBuff.sPlayerID));
							BulletBase.tNetDmgStack.nDmg = tUpdatePerBuff.nStack;
							BulletBase.tNetDmgStack.nHP = tSOB.Hp;
							BulletBase.tNetDmgStack.nEndHP = tSOB.Hurt(tHurtPassParam);
							BulletBase.tNetDmgStack.nHP = (int)tSOB.MaxHp - (int)tSOB.DmgHp + (int)tSOB.HealHp;
							StageObjBase stageObjBase2 = tSOB;
							stageObjBase2.DmgHp = (int)stageObjBase2.DmgHp + ((int)BulletBase.tNetDmgStack.nHP - (int)tSOB.Hp);
							BulletBase.tNetDmgStack.nDmgHP = tSOB.DmgHp;
							BulletBase.tNetDmgStack.nHealHP = tSOB.HealHp;
							CheckAddPlayerDamage();
							if ((int)tSOB.Hp <= 0)
							{
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, tSOB.GetDamageTextPos(), tUpdatePerBuff.nStack, tSOB.GetSOBLayerMask(), VisualDamage.DamageType.Normal);
								if ((bool)(tSOB as EnemyControllerBase))
								{
									MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerKillEnemyNum(tUpdatePerBuff.sPlayerID);
									BattleInfoUI.Instance.UpdateKillScoreUI((tSOB as EnemyControllerBase).EnemyData.n_SCORE);
									if (StageResManager.GetStageUpdate() != null)
									{
										StageResManager.GetStageUpdate().TriggerStageQuest(null, null, tSOB.selfBuffManager);
									}
								}
								num2 = -1;
							}
							else
							{
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, tSOB.GetDamageTextPos(), (int)BulletBase.tNetDmgStack.nHP - (int)BulletBase.tNetDmgStack.nEndHP, tSOB.GetSOBLayerMask(), VisualDamage.DamageType.Normal);
							}
						}
					}
				}
				if (tUpdatePerBuff.fDuration <= 0f)
				{
					CheckDelBuffTrigger(tUpdatePerBuff.nBuffID, 1);
					RemoveBuffIndex(num2, false);
					bNeedCalcuStatus = true;
				}
			}
			bUpdateBarUI = true;
		}
		if (bNeedCalcuStatus)
		{
			CalcuStatus();
		}
		if (bUpdateBarUI && this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
		bUpdateBarUI = false;
		bNeedCalcuStatus = false;
	}

	private void CheckAddPlayerDamage()
	{
		bool flag = false;
		if (BattleInfoUI.Instance != null && (BattleInfoUI.Instance.NowStageTable.n_TYPE == 9 || BattleInfoUI.Instance.NowStageTable.n_TYPE == 10))
		{
			EnemyControllerBase enemyControllerBase = tSOB as EnemyControllerBase;
			if (enemyControllerBase != null && ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(enemyControllerBase.EnemyData))
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerDMG(tUpdatePerBuff.sPlayerID, BulletBase.tNetDmgStack.nDmg, (int)BulletBase.tNetDmgStack.nHP - (int)BulletBase.tNetDmgStack.nEndHP, tSOB as OrangeCharacter != null);
		}
	}

	private bool CheckCalcuNetGameDmg()
	{
		if (tUpdatePerBuff.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			return true;
		}
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPlayerID(tUpdatePerBuff.sPlayerID))
		{
			if (tSOB.GetSOBType() == 1)
			{
				if (tSOB.CheckIsLocalPlayer())
				{
					return true;
				}
			}
			else if (StageUpdate.bIsHost)
			{
				return true;
			}
		}
		return false;
	}

	public void SwitchBuffFX(bool bShow)
	{
		if (bShow)
		{
			for (int num = listBuffs.Count - 1; num >= 0; num--)
			{
				TriggerBuffFX(listBuffs[num].refCTable, false, false);
			}
			return;
		}
		for (int num2 = FxObjLinks.Count - 1; num2 >= 0; num2--)
		{
			if (!(FxObjLinks[num2].tObj == null))
			{
				FxBase component = FxObjLinks[num2].tObj.GetComponent<FxBase>();
				if (component == null)
				{
					FxObjLinks[num2].tObj.SetActive(false);
				}
				else
				{
					component.BackToPool();
				}
				FxObjLinks.RemoveAt(num2);
			}
		}
	}

	public void CpyToPBM(PerBuffManager tPBM)
	{
		tPBM.nMeasureNow = nMeasureNow;
		tPBM.fLastAddMeasueTime = fLastAddMeasueTime;
		int count = listBuffs.Count;
		PerBuff perBuff = null;
		for (int i = 0; i < count; i++)
		{
			perBuff = new PerBuff();
			perBuff.nBuffID = listBuffs[i].nBuffID;
			perBuff.nStack = listBuffs[i].nStack;
			perBuff.refCTable = listBuffs[i].refCTable;
			perBuff.fDuration = listBuffs[i].fDuration;
			perBuff.fLeftTime = listBuffs[i].fLeftTime;
			perBuff.nOtherParam1 = listBuffs[i].nOtherParam1;
			perBuff.sPlayerID = listBuffs[i].sPlayerID;
			tPBM.listBuffs.Add(perBuff);
			tPBM.TriggerBuffFX(perBuff.refCTable, false);
		}
		tPBM.CalcuStatus();
		if (this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
	}

	public List<PerBuff> GetSelfSyncBuffList()
	{
		List<PerBuff> list = new List<PerBuff>();
		for (int i = 0; i < listBuffs.Count; i++)
		{
			PerBuff perBuff = new PerBuff();
			PerBuff perBuff2 = listBuffs[i];
			perBuff.refCTable = perBuff2.refCTable;
			perBuff.fLeftTime = perBuff2.fLeftTime;
			perBuff.bWaitNetSyncAdd = perBuff2.bWaitNetSyncAdd;
			perBuff.bWaitNetSyncStack = perBuff2.bWaitNetSyncStack;
			perBuff.bWaitNetSyncTime = perBuff2.bWaitNetSyncTime;
			perBuff.bWaitNetDel = perBuff2.bWaitNetDel;
			perBuff.fWaitNetSyncTimeOut = perBuff2.fWaitNetSyncTimeOut;
			perBuff.nBuffID = perBuff2.nBuffID;
			perBuff.nStack = perBuff2.nStack;
			perBuff.fDuration = perBuff2.fDuration;
			perBuff.nOtherParam1 = perBuff2.nOtherParam1;
			perBuff.sPlayerID = perBuff2.sPlayerID;
			list.Add(perBuff);
		}
		return list;
	}

	public void SyncByNetBuff(List<PerBuff> tListBuff, bool bIgnoreWaitNetSync = false)
	{
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			bTmpBool = false;
			tUpdatePerBuff = listBuffs[num];
			for (int num2 = tListBuff.Count - 1; num2 >= 0; num2--)
			{
				tCpyPerBuff = tListBuff[num2];
				if (tUpdatePerBuff.nBuffID == tCpyPerBuff.nBuffID)
				{
					if (tUpdatePerBuff.bWaitNetSyncStack)
					{
						if (tUpdatePerBuff.nStack == tCpyPerBuff.nStack)
						{
							tUpdatePerBuff.bWaitNetSyncStack = false;
						}
						else
						{
							float num3 = (float)tUpdatePerBuff.refCTable.n_DURATION - GameLogicUpdateManager.m_fFrameLen;
							if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && tUpdatePerBuff.refCTable.n_EFFECT == 107)
							{
								num3 = (float)tUpdatePerBuff.refCTable.n_DURATION * (float)OrangeConst.PVP_STUN_MODIFY / 100f - GameLogicUpdateManager.m_fFrameLen;
							}
							if (tCpyPerBuff.fDuration >= num3 - 1E-05f)
							{
								tUpdatePerBuff.bWaitNetSyncStack = false;
								tUpdatePerBuff.nStack = tCpyPerBuff.nStack;
							}
						}
					}
					else
					{
						if (tUpdatePerBuff.nStack != tCpyPerBuff.nStack)
						{
							bNeedCalcuStatus = true;
						}
						tUpdatePerBuff.nStack = tCpyPerBuff.nStack;
					}
					if (tUpdatePerBuff.bWaitNetDel)
					{
						float num4 = (float)tUpdatePerBuff.refCTable.n_DURATION - GameLogicUpdateManager.m_fFrameLen;
						if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && tUpdatePerBuff.refCTable.n_EFFECT == 107)
						{
							num4 = (float)tUpdatePerBuff.refCTable.n_DURATION * (float)OrangeConst.PVP_STUN_MODIFY / 100f - GameLogicUpdateManager.m_fFrameLen;
						}
						if (tCpyPerBuff.fDuration >= num4 - 1E-05f)
						{
							bNeedCalcuStatus = true;
							tUpdatePerBuff.bWaitNetDel = false;
						}
					}
					tUpdatePerBuff.fDuration = tCpyPerBuff.fDuration;
					tUpdatePerBuff.nOtherParam1 = tCpyPerBuff.nOtherParam1;
					tUpdatePerBuff.sPlayerID = tCpyPerBuff.sPlayerID;
					tUpdatePerBuff.bWaitNetSyncAdd = false;
					tUpdatePerBuff.bWaitNetSyncTime = false;
					tListBuff.RemoveAt(num2);
					bTmpBool = true;
					break;
				}
			}
			if (!bTmpBool && (!tUpdatePerBuff.bWaitNetSyncAdd || bIgnoreWaitNetSync))
			{
				tUpdatePerBuff.bWaitNetDel = true;
				CheckDelBuffTrigger(tUpdatePerBuff.nBuffID, 0);
				RemoveBuffIndex(num, false);
				bNeedCalcuStatus = true;
			}
		}
		for (int num5 = tListBuff.Count - 1; num5 >= 0; num5--)
		{
			AddBuff(tListBuff[num5], false, false);
			bNeedCalcuStatus = true;
		}
		if (bNeedCalcuStatus)
		{
			CalcuStatus();
		}
		if (this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
	}

	public void SyncByNetMsgString(string sNetMsg)
	{
		List<PerBuff> tListBuff = JsonConvert.DeserializeObject<List<PerBuff>>(sNetMsg);
		SyncByNetBuff(tListBuff);
	}

	public void RemoveFxObjLink(string fxstr)
	{
		if (fxstr == null || !(fxstr != "") || !(fxstr != "null"))
		{
			return;
		}
		for (int i = 0; i < FxObjLinks.Count; i++)
		{
			if (!(FxObjLinks[i].sBuffName == fxstr))
			{
				continue;
			}
			FxObjLinks[i].nRefCount--;
			if (FxObjLinks[i].nRefCount != 0)
			{
				break;
			}
			if (FxObjLinks[i].tObj == null)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.StartCoroutine(CheckFxAndRemoveCoroutine(FxObjLinks[i]));
			}
			else
			{
				FxBase component = FxObjLinks[i].tObj.GetComponent<FxBase>();
				if (component == null)
				{
					FxObjLinks[i].tObj.SetActive(false);
				}
				else
				{
					component.BackToPool();
				}
			}
			FxObjLinks.RemoveAt(i);
			break;
		}
	}

	private IEnumerator CheckFxAndRemoveCoroutine(FxObjLink tFxObjLink)
	{
		while (tFxObjLink.tObj == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		FxBase component = tFxObjLink.tObj.GetComponent<FxBase>();
		if (component == null)
		{
			tFxObjLink.tObj.SetActive(false);
		}
		else
		{
			component.BackToPool();
		}
	}

	private void BUFF_ENERGY_SHIELD_FUNC(CONDITION_TABLE tCONDITION_TABLE, PerBuff tTargetPerBuff, int nAtk, int nMaxHP)
	{
		tTargetPerBuff.nStack = Mathf.RoundToInt((tCONDITION_TABLE.f_EFFECT_X * (float)nAtk + (float)nMaxHP * tCONDITION_TABLE.f_EFFECT_Z) * 0.01f);
		tTargetPerBuff.nOtherParam1 = tTargetPerBuff.nStack;
	}

	private void DEBUFF_PERIOD_DMG_FUNC(CONDITION_TABLE tCONDITION_TABLE, PerBuff tTargetPerBuff, int nAtk, string sAdderID)
	{
		if (tCONDITION_TABLE.f_EFFECT_X != 0f)
		{
			int num = (int)((float)nAtk * tCONDITION_TABLE.f_EFFECT_X * 0.01f);
			if (num > tTargetPerBuff.nStack)
			{
				tTargetPerBuff.nStack = num;
				tTargetPerBuff.sPlayerID = sAdderID;
			}
			if (tTargetPerBuff.nStack <= 0)
			{
				tTargetPerBuff.nStack = 1;
			}
		}
	}

	public static void TriggerBuffer(SKILL_TABLE tSKILL_TABLE, PerBuffManager tTargetBM, PerBuffManager tShoterBM, int nAtkParam = 0, int nMaxHP = 0, int nSkillID = 0, bool bNetAdd = false)
	{
		if (tSKILL_TABLE.n_CONDITION_ID == 0 || tSKILL_TABLE.n_CONDITION_RATE < OrangeBattleUtility.Random(0, 10000))
		{
			return;
		}
		string sAdderID = "";
		if (tShoterBM != null)
		{
			sAdderID = tShoterBM.tSOB.sNetSerialID;
		}
		if (tSKILL_TABLE.n_CONDITION_TARGET == 0)
		{
			if (tTargetBM != null)
			{
				tTargetBM.AddBuff(tSKILL_TABLE.n_CONDITION_ID, nAtkParam, nMaxHP, nSkillID, bNetAdd, sAdderID);
			}
		}
		else if (tSKILL_TABLE.n_CONDITION_TARGET == 1 && tShoterBM != null)
		{
			tShoterBM.AddBuff(tSKILL_TABLE.n_CONDITION_ID, nAtkParam, nMaxHP, nSkillID, bNetAdd, sAdderID);
		}
	}

	public void AddBuff(int nBuffID, int nAtk, int nMaxHP, int nskillid, bool bIsNetAdd = false, string sAdderID = "", int nBitParam = 0)
	{
		CONDITION_TABLE value = null;
		bool flag = false;
		bool flag2 = (nBitParam & 4) != 0;
		OrangeCharacter orangeCharacter = null;
		EnemyControllerBase enemyControllerBase = null;
		if (tSOB != null)
		{
			if (tSOB.GetSOBType() == 1)
			{
				orangeCharacter = tSOB as OrangeCharacter;
			}
			else if (tSOB.GetSOBType() == 2)
			{
				enemyControllerBase = tSOB as EnemyControllerBase;
			}
		}
		if (nBuffID > 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(nBuffID, out value))
		{
			int count = listBuffs.Count;
			bNeedCalcuStatus = false;
			if (!bIsNetAdd)
			{
				if (value.n_NOT_REMOVABLE == 0)
				{
					if (value.n_EFFECT > 0 && value.n_EFFECT < 101)
					{
						if (CheckHasAntiBuff(value.n_ID, value.n_EFFECT))
						{
							DoAntiBuff();
							return;
						}
					}
					else if (value.n_EFFECT >= 101 && value.n_EFFECT < 1000 && CheckHasEffect(8))
					{
						return;
					}
					if (sBuffStatus.refPS.CheckPreventBuffPS(value.n_EFFECT))
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, tSOB.GetDamageTextPos(), 0, tSOB.GetSOBLayerMask(), VisualDamage.DamageType.Resist);
						StageUpdate.SyncStageObj(4, 1, tSOB.sNetSerialID + "," + 0 + "," + nAtk + "," + nMaxHP + "," + nskillid + "," + sAdderID, true);
						return;
					}
				}
				if (tSOB.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					StageUpdate.SyncStageObj(4, 1, tSOB.sNetSerialID + "," + nBuffID + "," + nAtk + "," + nMaxHP + "," + nskillid + "," + sAdderID, true);
				}
			}
			else if (tSOB != null && tSOB.GetSOBType() == 1 && (nBitParam & 2) == 0 && !orangeCharacter.Controller.Collider2D.enabled)
			{
				return;
			}
			for (int i = 0; i < count; i++)
			{
				tAddPerBuff = listBuffs[i];
				if (tAddPerBuff.nBuffID != value.n_ID)
				{
					continue;
				}
				if (tAddPerBuff.bWaitNetDel)
				{
					tAddPerBuff.bWaitNetDel = false;
				}
				switch (value.n_EFFECT)
				{
				case 109:
					if (tAddPerBuff.nOtherParam1 != nskillid || tAddPerBuff.sPlayerID != sAdderID)
					{
						continue;
					}
					if (value.n_MAX_STACK > tAddPerBuff.nStack)
					{
						bNeedCalcuStatus = true;
						tAddPerBuff.nStack++;
					}
					break;
				case 115:
				case 117:
					if (tAddPerBuff.sPlayerID != sAdderID)
					{
						continue;
					}
					break;
				case 6:
					BUFF_ENERGY_SHIELD_FUNC(value, tAddPerBuff, nAtk, nMaxHP);
					bNeedCalcuStatus = true;
					break;
				case 106:
					DEBUFF_PERIOD_DMG_FUNC(value, tAddPerBuff, nAtk, sAdderID);
					break;
				case 112:
					tAddPerBuff.nStack = Mathf.RoundToInt(value.f_EFFECT_X);
					break;
				case 120:
					tAddPerBuff.nStack += (int)value.f_EFFECT_X;
					bNeedCalcuStatus = true;
					if (tAddPerBuff.nStack > value.n_MAX_STACK)
					{
						tAddPerBuff.nStack = value.n_MAX_STACK;
					}
					break;
				default:
					if (value.n_MAX_STACK > tAddPerBuff.nStack)
					{
						bNeedCalcuStatus = true;
						tAddPerBuff.nStack++;
					}
					break;
				}
				tAddPerBuff.fDuration = value.n_DURATION;
				if (SOB != null && SOB.GetSOBType() == 1)
				{
					tAddPerBuff.bWaitNetSyncStack = (tAddPerBuff.bWaitNetSyncTime = SOB.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
				}
				else
				{
					tAddPerBuff.bWaitNetSyncTime = false;
					tAddPerBuff.bWaitNetSyncStack = false;
				}
				tAddPerBuff.bWaitNetSyncAdd = !bIsNetAdd;
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && value.n_EFFECT == 107)
				{
					tAddPerBuff.fDuration = tAddPerBuff.fDuration * (float)OrangeConst.PVP_STUN_MODIFY / 100f;
				}
				if (bNeedCalcuStatus)
				{
					CalcuStatus();
				}
				if (value.s_HIT_SE != "null")
				{
					string[] array = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(value.s_HIT_SE);
					if (orangeCharacter != null)
					{
						if ((int)orangeCharacter.Hp > 0)
						{
							orangeCharacter.PlaySE(array[0], array[1]);
						}
					}
					else if (enemyControllerBase != null && (int)enemyControllerBase.Hp > 0)
					{
						enemyControllerBase.PlaySE(array[0], array[1]);
					}
				}
				TriggerBuffFX(value, false);
				if (this.UpdateBuffBar != null)
				{
					this.UpdateBuffBar(this);
				}
				if (StageResManager.GetStageUpdate() != null)
				{
					StageResManager.GetStageUpdate().TriggerStageQuest(null, SOB.selfBuffManager, null);
				}
				return;
			}
			tAddPerBuff = new PerBuff();
			tAddPerBuff.refCTable = value;
			tAddPerBuff.fDuration = value.n_DURATION;
			tAddPerBuff.nBuffID = value.n_ID;
			tAddPerBuff.sPlayerID = sAdderID;
			if (SOB != null && SOB.GetSOBType() == 1)
			{
				tAddPerBuff.bWaitNetSyncTime = SOB.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
			}
			else
			{
				tAddPerBuff.bWaitNetSyncTime = false;
			}
			tAddPerBuff.bWaitNetSyncAdd = !bIsNetAdd;
			tAddPerBuff.bWaitNetSyncStack = false;
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && value.n_EFFECT == 107)
			{
				tAddPerBuff.fDuration = tAddPerBuff.fDuration * (float)OrangeConst.PVP_STUN_MODIFY / 100f;
			}
			switch (value.n_EFFECT)
			{
			case 6:
				BUFF_ENERGY_SHIELD_FUNC(value, tAddPerBuff, nAtk, nMaxHP);
				break;
			case 106:
				DEBUFF_PERIOD_DMG_FUNC(value, tAddPerBuff, nAtk, sAdderID);
				break;
			case 112:
				tAddPerBuff.nStack = Mathf.RoundToInt(value.f_EFFECT_X);
				break;
			case 107:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_STOP_ACT))
				{
					if (orangeCharacter != null)
					{
						orangeCharacter.SetStun(true);
					}
					if (enemyControllerBase != null)
					{
						enemyControllerBase.SetStun(true);
					}
				}
				tAddPerBuff.nStack++;
				break;
			case 108:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_STOP_ACT) && orangeCharacter != null && !orangeCharacter.IsJacking)
				{
					OrangeCharacter playerByID = StageUpdate.GetPlayerByID(sAdderID);
					if (playerByID != null)
					{
						orangeCharacter.StartCoroutine(orangeCharacter.SetKnockOutWithAttacker(playerByID));
					}
				}
				break;
			case 109:
				tAddPerBuff.nOtherParam1 = nskillid;
				tAddPerBuff.nStack++;
				break;
			case 110:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_BAN_WEAPON) && orangeCharacter != null)
				{
					orangeCharacter.SetBanWeapon(true);
				}
				tAddPerBuff.nStack++;
				break;
			case 111:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_BAN_SKILL) && orangeCharacter != null)
				{
					orangeCharacter.SetBanSkill(true);
				}
				tAddPerBuff.nStack++;
				break;
			case 11:
				if (orangeCharacter != null)
				{
					if ((int)value.f_EFFECT_X != 0)
					{
						RefPassiveskill.PassiveskillStatus newPassiveSkill = orangeCharacter.tRefPassiveskill.AddPassivesSkill((int)value.f_EFFECT_X);
						orangeCharacter.CheckBuffGainNewPassiveSkill(ref newPassiveSkill);
						orangeCharacter.CheckBuffGainPassiveSkill((int)value.f_EFFECT_X);
					}
					if ((int)value.f_EFFECT_Y != 0)
					{
						RefPassiveskill.PassiveskillStatus newPassiveSkill2 = orangeCharacter.tRefPassiveskill.AddPassivesSkill((int)value.f_EFFECT_Y);
						orangeCharacter.CheckBuffGainNewPassiveSkill(ref newPassiveSkill2);
						orangeCharacter.CheckBuffGainPassiveSkill((int)value.f_EFFECT_Y);
					}
					if ((int)value.f_EFFECT_Z != 0)
					{
						RefPassiveskill.PassiveskillStatus newPassiveSkill3 = orangeCharacter.tRefPassiveskill.AddPassivesSkill((int)value.f_EFFECT_Z);
						orangeCharacter.CheckBuffGainNewPassiveSkill(ref newPassiveSkill3);
						orangeCharacter.CheckBuffGainPassiveSkill((int)value.f_EFFECT_Z);
					}
				}
				tAddPerBuff.nStack++;
				break;
			case 116:
				if (orangeCharacter != null && orangeCharacter.IsLocalPlayer && orangeCharacter.IsAlive())
				{
					string p_param = ((value.s_UICAMERA_FX == "null") ? "fxcamera_blind" : value.s_UICAMERA_FX.ToString());
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_EFFECT_CTRL, true, p_param);
				}
				tAddPerBuff.nStack++;
				break;
			case 118:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_REVERSE_RIGHT_AND_LEFT) && orangeCharacter != null)
				{
					orangeCharacter.SetReverseRightAndLeft(true);
				}
				tAddPerBuff.nStack++;
				break;
			case 119:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_BAN_AUTO_AIM) && orangeCharacter != null)
				{
					orangeCharacter.SetBanAutoAim(true);
				}
				tAddPerBuff.nStack++;
				break;
			case 120:
				tAddPerBuff.nStack += (int)value.f_EFFECT_X;
				if (tAddPerBuff.nStack > value.n_MAX_STACK)
				{
					tAddPerBuff.nStack = value.n_MAX_STACK;
				}
				break;
			case 121:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_NOMOVE) && orangeCharacter != null)
				{
					orangeCharacter.SetNoMove(true);
				}
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_BAN_SKILL) && orangeCharacter != null)
				{
					orangeCharacter.SetBanSkill(true);
				}
				tAddPerBuff.nStack++;
				break;
			case 123:
			{
				if (!(orangeCharacter != null))
				{
					break;
				}
				orangeCharacter.LockControl();
				StageCtrlInsTruction stageCtrlInsTruction = new StageCtrlInsTruction();
				StageObjBase sOBByNetSerialID = StageResManager.GetStageUpdate().GetSOBByNetSerialID(sAdderID);
				if (sOBByNetSerialID != null)
				{
					if (sOBByNetSerialID.transform.position.x <= SOB.transform.position.x)
					{
						stageCtrlInsTruction.tStageCtrl = 67;
					}
					else
					{
						stageCtrlInsTruction.tStageCtrl = 68;
					}
				}
				else
				{
					stageCtrlInsTruction.tStageCtrl = 68;
				}
				stageCtrlInsTruction.fTime = tAddPerBuff.fDuration + 2f;
				orangeCharacter.ObjCtrl(orangeCharacter.gameObject, stageCtrlInsTruction);
				break;
			}
			default:
				if (value.n_MAX_STACK > tAddPerBuff.nStack)
				{
					tAddPerBuff.nStack++;
				}
				break;
			}
			if (value.n_STACK_RULE == 2)
			{
				if (CheckHasEffect(value.n_EFFECT))
				{
					return;
				}
			}
			else if (value.n_STACK_RULE == 3 || value.n_STACK_RULE == 4)
			{
				CheckDelBuffTrigger(value.n_ID, 4);
				flag = RemoveBuffEx(value.n_EFFECT);
			}
			if (value.n_REMOVE > 0)
			{
				CheckDelBuffTrigger(value.n_ID, 4);
				RemoveBuffByBuffID(value.n_REMOVE);
			}
			Check_Skill_ICON(orangeCharacter, value, true);
			if (!flag || value.n_STACK_RULE == 4)
			{
				listBuffs.Add(tAddPerBuff);
				if ((int)tSOB.Hp > 0)
				{
					if (value.s_HIT_SE != "null")
					{
						string[] array2 = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(value.s_HIT_SE);
						if (flag2)
						{
							if (orangeCharacter != null)
							{
								orangeCharacter.PlaySE(array2[0], array2[1]);
							}
							if (enemyControllerBase != null)
							{
								enemyControllerBase.PlaySE(array2[0], array2[1]);
							}
						}
					}
					if (value.s_DURING_SE != "null")
					{
						string[] array3 = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(value.s_DURING_SE);
						if (flag2)
						{
							if (orangeCharacter != null)
							{
								orangeCharacter.PlaySE(array3[0], array3[1]);
							}
							if (enemyControllerBase != null)
							{
								enemyControllerBase.PlaySE(array3[0], array3[1]);
							}
						}
					}
				}
				TriggerBuffFX(value, true);
				CalcuStatus();
			}
		}
		else if (nBuffID < 0)
		{
			if (!bIsNetAdd)
			{
				StageUpdate.SyncStageObj(4, 1, tSOB.sNetSerialID + "," + nBuffID + "," + nAtk + "," + nMaxHP + "," + nskillid + "," + sAdderID, true);
			}
			int count2 = listBuffs.Count;
			for (int j = 0; j < count2; j++)
			{
				tAddPerBuff = listBuffs[j];
				if (tAddPerBuff.nBuffID == nBuffID)
				{
					tAddPerBuff.nStack++;
					tAddPerBuff.fDuration = 99999f;
					CalcuStatus();
					return;
				}
			}
			tAddPerBuff = new PerBuff();
			tAddPerBuff.refCTable = new CONDITION_TABLE();
			tAddPerBuff.refCTable.n_ID = nBuffID;
			tAddPerBuff.refCTable.n_NOT_REMOVABLE = 1;
			tAddPerBuff.fDuration = 99999f;
			tAddPerBuff.nBuffID = nBuffID;
			tAddPerBuff.sPlayerID = sAdderID;
			if (SOB != null && SOB.GetSOBType() == 1)
			{
				tAddPerBuff.bWaitNetSyncTime = SOB.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
			}
			else
			{
				tAddPerBuff.bWaitNetSyncTime = false;
			}
			tAddPerBuff.bWaitNetSyncAdd = !bIsNetAdd;
			tAddPerBuff.bWaitNetSyncStack = false;
			tAddPerBuff.nStack = 1;
			listBuffs.Add(tAddPerBuff);
			CalcuStatus();
		}
		if (StageResManager.GetStageUpdate() != null)
		{
			StageResManager.GetStageUpdate().TriggerStageQuest(null, SOB.selfBuffManager, null);
		}
		if (this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
	}

	private void AddBuff(PerBuff tAddBuff, bool bNeedCalsu = true, bool bNeedUpdateBar = true, bool ReplaceDuplicate = false)
	{
		OrangeCharacter orangeCharacter = null;
		EnemyControllerBase enemyControllerBase = null;
		if (tSOB != null)
		{
			if (tSOB.GetSOBType() == 1)
			{
				orangeCharacter = tSOB as OrangeCharacter;
			}
			else if (tSOB.GetSOBType() == 2)
			{
				enemyControllerBase = tSOB as EnemyControllerBase;
			}
		}
		CONDITION_TABLE value = null;
		if (tAddBuff.nBuffID > 0 && ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(tAddBuff.nBuffID, out value))
		{
			if (value.n_NOT_REMOVABLE == 0)
			{
				if (value.n_EFFECT >= 101 && value.n_EFFECT < 1000 && CheckHasEffect(8))
				{
					return;
				}
				if (sBuffStatus.refPS.CheckPreventBuffPS(value.n_EFFECT))
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, tSOB.GetDamageTextPos(), 0, tSOB.GetSOBLayerMask(), VisualDamage.DamageType.Resist);
					return;
				}
			}
			if (ReplaceDuplicate)
			{
				for (int i = 0; i < listBuffs.Count; i++)
				{
					if (tAddBuff.nBuffID == listBuffs[i].nBuffID)
					{
						RemoveBuffByCONDITIONID(listBuffs[i].nBuffID);
						break;
					}
				}
			}
			tAddPerBuff = new PerBuff();
			tAddPerBuff.refCTable = value;
			tAddPerBuff.fDuration = tAddBuff.fDuration;
			tAddPerBuff.nBuffID = value.n_ID;
			switch (value.n_EFFECT)
			{
			case 6:
				if (tAddBuff.nOtherParam1 > tAddBuff.nStack)
				{
					tAddPerBuff.nStack = tAddBuff.nStack;
					tAddPerBuff.nOtherParam1 = tAddBuff.nOtherParam1;
				}
				else
				{
					tAddPerBuff.nOtherParam1 = (tAddPerBuff.nStack = tAddBuff.nStack);
				}
				break;
			case 106:
				tAddPerBuff.nStack = tAddBuff.nStack;
				tAddPerBuff.sPlayerID = tAddBuff.sPlayerID;
				break;
			case 107:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_STOP_ACT))
				{
					if (orangeCharacter != null)
					{
						orangeCharacter.SetStun(true);
					}
					if (enemyControllerBase != null)
					{
						enemyControllerBase.SetStun(true);
					}
				}
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 109:
				tAddPerBuff.nOtherParam1 = tAddBuff.nOtherParam1;
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 110:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_BAN_WEAPON) && orangeCharacter != null)
				{
					orangeCharacter.SetBanWeapon(true);
				}
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 111:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_BAN_SKILL) && orangeCharacter != null)
				{
					orangeCharacter.SetBanSkill(true);
				}
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 11:
				if (orangeCharacter != null)
				{
					if ((int)value.f_EFFECT_X != 0)
					{
						RefPassiveskill.PassiveskillStatus newPassiveSkill = orangeCharacter.tRefPassiveskill.AddPassivesSkill((int)value.f_EFFECT_X);
						orangeCharacter.CheckBuffGainNewPassiveSkill(ref newPassiveSkill);
						orangeCharacter.CheckBuffGainPassiveSkill((int)value.f_EFFECT_X);
					}
					if ((int)value.f_EFFECT_Y != 0)
					{
						RefPassiveskill.PassiveskillStatus newPassiveSkill2 = orangeCharacter.tRefPassiveskill.AddPassivesSkill((int)value.f_EFFECT_Y);
						orangeCharacter.CheckBuffGainNewPassiveSkill(ref newPassiveSkill2);
						orangeCharacter.CheckBuffGainPassiveSkill((int)value.f_EFFECT_Y);
					}
					if ((int)value.f_EFFECT_Z != 0)
					{
						RefPassiveskill.PassiveskillStatus newPassiveSkill3 = orangeCharacter.tRefPassiveskill.AddPassivesSkill((int)value.f_EFFECT_Z);
						orangeCharacter.CheckBuffGainNewPassiveSkill(ref newPassiveSkill3);
						orangeCharacter.CheckBuffGainPassiveSkill((int)value.f_EFFECT_Z);
					}
				}
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 116:
				if (orangeCharacter != null && orangeCharacter.IsLocalPlayer && orangeCharacter.IsAlive())
				{
					string p_param = ((value.s_UICAMERA_FX == "null") ? "fxcamera_blind" : value.s_UICAMERA_FX.ToString());
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_EFFECT_CTRL, true, p_param);
				}
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 118:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_REVERSE_RIGHT_AND_LEFT) && orangeCharacter != null)
				{
					orangeCharacter.SetReverseRightAndLeft(true);
				}
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 119:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_BAN_AUTO_AIM) && orangeCharacter != null)
				{
					orangeCharacter.SetBanAutoAim(true);
				}
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 121:
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_NOMOVE) && orangeCharacter != null)
				{
					orangeCharacter.SetNoMove(true);
				}
				if (!IsHasBuffType(BUFF_TYPE.DEBUFF_BAN_SKILL) && orangeCharacter != null)
				{
					orangeCharacter.SetBanSkill(true);
				}
				tAddPerBuff.nStack++;
				break;
			default:
				tAddPerBuff.nStack = tAddBuff.nStack;
				break;
			case 108:
				break;
			}
			if (value.n_STACK_RULE == 2)
			{
				if (CheckHasEffect(value.n_EFFECT))
				{
					return;
				}
			}
			else if (value.n_STACK_RULE == 3 || value.n_STACK_RULE == 4)
			{
				RemoveBuff(value.n_EFFECT, true, true);
			}
			if (value.n_REMOVE > 0)
			{
				CheckDelBuffTrigger(value.n_ID, 4);
				RemoveBuffByBuffID(value.n_REMOVE);
			}
			listBuffs.Add(tAddPerBuff);
			Check_Skill_ICON(orangeCharacter, value, true);
			TriggerBuffFX(value, true);
			if (bNeedCalsu)
			{
				CalcuStatus();
			}
		}
		else if (tAddBuff.nBuffID < 0)
		{
			tAddPerBuff = new PerBuff();
			tAddPerBuff.refCTable = new CONDITION_TABLE();
			tAddPerBuff.refCTable.n_ID = tAddBuff.nBuffID;
			tAddPerBuff.refCTable.n_NOT_REMOVABLE = 1;
			tAddPerBuff.nBuffID = tAddBuff.nBuffID;
			tAddPerBuff.nStack = tAddBuff.nStack;
			tAddPerBuff.fDuration = tAddBuff.fDuration;
			tAddPerBuff.nOtherParam1 = tAddBuff.nOtherParam1;
			tAddPerBuff.sPlayerID = tAddBuff.sPlayerID;
			listBuffs.Add(tAddPerBuff);
			if (bNeedCalsu)
			{
				CalcuStatus();
			}
		}
		if (bNeedUpdateBar && this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
	}

	private void TriggerBuffFX(CONDITION_TABLE tCONDITION_TABLE, bool bAddRef, bool bShowHitFx = true)
	{
		if (tSOB != null && tSOB.IsHidden)
		{
			return;
		}
		Transform fXShowTrans = tSOB.GetFXShowTrans();
		if (fXShowTrans != null)
		{
			if (bShowHitFx && tCONDITION_TABLE.s_HIT_FX != "null")
			{
				MonoBehaviourSingleton<FxManager>.Instance.PlayWihtOffset<FxBase>(tCONDITION_TABLE.s_HIT_FX, fXShowTrans, Quaternion.identity, tSOB.AimPoint, Array.Empty<object>());
			}
			if (tCONDITION_TABLE.s_DURING_FX != "null")
			{
				AddDuringFX(tCONDITION_TABLE.s_DURING_FX, bAddRef);
			}
		}
	}

	public void AddDuringFX(string name, bool bAddRef = false)
	{
		for (int i = 0; i < FxObjLinks.Count; i++)
		{
			if (FxObjLinks[i].sBuffName == name)
			{
				if (bAddRef)
				{
					FxObjLinks[i].nRefCount++;
				}
				return;
			}
		}
		StageFXParam stageFXParam = new StageFXParam();
		stageFXParam.tFOL = new FxObjLink();
		stageFXParam.tFOL.sBuffName = name;
		stageFXParam.tFOL.nRefCount++;
		stageFXParam.fPlayTime = 999f;
		OrangeCharacter orangeCharacter = tSOB as OrangeCharacter;
		if ((bool)orangeCharacter)
		{
			Vector3 one = Vector3.one;
			string s_ANIMATOR = orangeCharacter.CharacterData.s_ANIMATOR;
			one = ((s_ANIMATOR == "femalesmallcontroller") ? new Vector3(0.9f, 0.9f, 0.9f) : ((!(s_ANIMATOR == "malelargecontroller")) ? Vector3.one : new Vector3(1.2f, 1.2f, 1.2f)));
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(name, tSOB.GetFXShowTrans(), Quaternion.identity, one, new object[1] { stageFXParam });
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(name, tSOB.GetFXShowTrans(), Quaternion.identity, new object[1] { stageFXParam });
		}
		if (stageFXParam.tFOL.tObj != null)
		{
			stageFXParam.tFOL.tObj.transform.localPosition = tSOB.AimPoint;
		}
		FxObjLinks.Add(stageFXParam.tFOL);
	}

	public bool CheclIgnoreSkill(SKILL_TABLE tSKILL_TABLE)
	{
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].refCTable.n_EFFECT == 10 && (int)listBuffs[num].refCTable.f_EFFECT_X == tSKILL_TABLE.n_ID)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckHasEffect(int tEffectID, int nStack = 1)
	{
		int count = listBuffs.Count;
		if (nStack > 1)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				if (listBuffs[i].refCTable.n_EFFECT == tEffectID && listBuffs[i].IsBuffOk())
				{
					num += listBuffs[i].nStack;
				}
			}
			if (num >= nStack)
			{
				return true;
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				if (listBuffs[j].refCTable.n_EFFECT == tEffectID && listBuffs[j].IsBuffOk())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckHasEffect(int tEffectID, out PerBuff perBuff)
	{
		int count = listBuffs.Count;
		perBuff = null;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_EFFECT == tEffectID && listBuffs[i].IsBuffOk())
			{
				perBuff = listBuffs[i];
				return true;
			}
		}
		return false;
	}

	public bool CheckHasMarkedEffect(int markEffectId, string playerId, out PerBuff perBuff)
	{
		int count = listBuffs.Count;
		perBuff = null;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_EFFECT == markEffectId && listBuffs[i].sPlayerID == playerId)
			{
				perBuff = listBuffs[i];
				return true;
			}
		}
		return false;
	}

	public void RemoveMarkedEffect(int buffId, string playerId, bool bNeedSendSync)
	{
		bool flag = false;
		bool flag2 = false;
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].nBuffID == buffId && listBuffs[i].sPlayerID == playerId)
			{
				if (bNeedSendSync && StageUpdate.gbIsNetGame && tSOB != null)
				{
					StageUpdate.SyncStageObj(4, 6, tSOB.sNetSerialID + "," + listBuffs[i].nBuffID + "," + listBuffs[i].sPlayerID, true);
				}
				CheckDelBuffTrigger(buffId, 2);
				RemoveBuffIndex(i);
				flag2 = true;
				flag = true;
				break;
			}
		}
		if (flag2 && this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
		if (flag)
		{
			CalcuStatus();
		}
	}

	public bool CheckHasAntiBuff(int newBuffId, int newBuffEffectId)
	{
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_EFFECT == 112 && listBuffs[i].IsBuffOk() && (listBuffs[i].refCTable.f_EFFECT_Y == 0f || listBuffs[i].refCTable.f_EFFECT_Y == (float)newBuffId) && (listBuffs[i].refCTable.f_EFFECT_Z == 0f || listBuffs[i].refCTable.f_EFFECT_Z == (float)newBuffEffectId))
			{
				return true;
			}
		}
		return false;
	}

	public float GetEffectXByIDParam(int tEffectID, int nOtherParamCheck1, PerBuffManager refPBMShoter)
	{
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_EFFECT == tEffectID && listBuffs[i].nOtherParam1 == nOtherParamCheck1 && listBuffs[i].sPlayerID == refPBMShoter.SOB.sNetSerialID && listBuffs[i].IsBuffOk())
			{
				return listBuffs[i].refCTable.f_EFFECT_X * (float)listBuffs[i].nStack;
			}
		}
		return 0f;
	}

	public bool CheckHasEffectByCONDITIONID(int CONDITIONID, int nStack = 0)
	{
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_ID == CONDITIONID && listBuffs[i].IsBuffOk() && listBuffs[i].nStack >= nStack)
			{
				return true;
			}
		}
		return false;
	}

	public void DoAntiBuff()
	{
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_EFFECT == 112)
			{
				listBuffs[i].nStack--;
				if (listBuffs[i].nStack <= 0)
				{
					CheckDelBuffTrigger(listBuffs[i].nBuffID, 2);
					RemoveBuffIndex(i);
					bNeedCalcuStatus = true;
				}
				break;
			}
		}
		if (bNeedCalcuStatus)
		{
			bNeedCalcuStatus = false;
			CalcuStatus();
			if (this.UpdateBuffBar != null)
			{
				this.UpdateBuffBar(this);
			}
		}
	}

	public void RemoveBuffByCONDITIONID(int CONDITIONID, bool bJustRemove = true)
	{
		bool flag = false;
		bool flag2 = false;
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].refCTable.n_ID == CONDITIONID)
			{
				RemoveBuffIndex(num);
				flag2 = true;
				flag = true;
			}
		}
		if (flag2 && this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
		if (flag)
		{
			CalcuStatus();
		}
	}

	private bool RemoveBuffIndex(int nIndex, bool bJustRemove = true)
	{
		if (nIndex >= listBuffs.Count)
		{
			return false;
		}
		OrangeCharacter orangeCharacter = null;
		if (tSOB.GetSOBType() == 1)
		{
			orangeCharacter = tSOB as OrangeCharacter;
		}
		else if (tSOB.GetSOBType() == 2)
		{
			StageObjBase tSOB2 = tSOB;
		}
		PerBuff perBuff = listBuffs[nIndex];
		if (!NetCheckRemoveBuffIndex(nIndex))
		{
			return false;
		}
		if (perBuff.refCTable.s_DURING_SE != "null" && tSOB != null)
		{
			string[] array = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(perBuff.refCTable.s_DURING_SE);
			tSOB.PlaySE(array[0], array[2]);
		}
		RemoveFxObjLink(perBuff.refCTable.s_DURING_FX);
		if (!bJustRemove && !IsHasBuffType(perBuff.refCTable.n_EFFECT) && tSOB != null)
		{
			switch (perBuff.refCTable.n_EFFECT)
			{
			case 107:
				tSOB.SetStun(false);
				break;
			case 110:
				tSOB.SetBanWeapon(false);
				break;
			case 111:
				tSOB.SetBanSkill(false);
				break;
			case 121:
				tSOB.SetNoMove(false);
				tSOB.SetBanSkill(false);
				break;
			case 118:
				tSOB.SetReverseRightAndLeft(false);
				break;
			case 119:
				tSOB.SetBanAutoAim(false);
				break;
			case 123:
				if (orangeCharacter != null)
				{
					orangeCharacter.LockInput = false;
				}
				break;
			}
		}
		if (perBuff.refCTable.n_EFFECT == 11 && (bool)orangeCharacter)
		{
			if ((int)perBuff.refCTable.f_EFFECT_X != 0)
			{
				orangeCharacter.tRefPassiveskill.RemovePassiveSkill((int)perBuff.refCTable.f_EFFECT_X);
				orangeCharacter.CheckBuffGainPassiveSkill((int)perBuff.refCTable.f_EFFECT_X);
			}
			if ((int)perBuff.refCTable.f_EFFECT_Y != 0)
			{
				orangeCharacter.tRefPassiveskill.RemovePassiveSkill((int)perBuff.refCTable.f_EFFECT_Y);
				orangeCharacter.CheckBuffGainPassiveSkill((int)perBuff.refCTable.f_EFFECT_Y);
			}
			if ((int)perBuff.refCTable.f_EFFECT_Z != 0)
			{
				orangeCharacter.tRefPassiveskill.RemovePassiveSkill((int)perBuff.refCTable.f_EFFECT_Z);
				orangeCharacter.CheckBuffGainPassiveSkill((int)perBuff.refCTable.f_EFFECT_Z);
			}
		}
		if (perBuff.refCTable.n_EFFECT == 116 && (bool)orangeCharacter && orangeCharacter.IsLocalPlayer)
		{
			string targetFxName = ((perBuff.refCTable.s_UICAMERA_FX == "null") ? "fxcamera_blind" : perBuff.refCTable.s_UICAMERA_FX.ToString());
			CONDITION_TABLE[] tCONDITION_TABLES;
			if (!CheckHasBuffs(BUFF_TYPE.DEBUFF_BLIND, out tCONDITION_TABLES) || !tCONDITION_TABLES.Any((CONDITION_TABLE condition) => condition.s_UICAMERA_FX == "null" || condition.s_UICAMERA_FX == targetFxName))
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_EFFECT_CTRL, false, targetFxName);
			}
		}
		Check_Skill_ICON(orangeCharacter, perBuff.refCTable, false);
		return true;
	}

	public bool CheckBuffIdHasNotRemovable(int tBuffID)
	{
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].refCTable.n_ID == tBuffID)
			{
				if (listBuffs[num].refCTable.n_NOT_REMOVABLE <= 0)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public void RemoveBuff(int tEffectID, bool bJustRemove = true, bool bReplace = false)
	{
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].refCTable.n_EFFECT == tEffectID)
			{
				int triggerType = (bReplace ? 4 : 2);
				CheckDelBuffTrigger(listBuffs[num].nBuffID, triggerType);
				RemoveBuffIndex(num, bJustRemove);
				bUpdateBarUI = true;
			}
		}
	}

	private bool RemoveBuffEx(int tEffectID, bool bJustRemove = true)
	{
		bool result = false;
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].refCTable.n_EFFECT == tEffectID && listBuffs[num].refCTable.n_STACK_RULE == 4)
			{
				result = true;
			}
			if (listBuffs[num].refCTable.n_EFFECT == tEffectID && listBuffs[num].refCTable.n_STACK_RULE != 4)
			{
				RemoveBuffIndex(num, bJustRemove);
			}
		}
		return result;
	}

	public void RemoveBuffByBuffID(int buffId, bool bJustRemove = true)
	{
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].nBuffID == buffId)
			{
				RemoveBuffIndex(num, bJustRemove);
				bUpdateBarUI = true;
				bNeedCalcuStatus = true;
				break;
			}
		}
	}

	public void RemoveBuffByBullet(SKILL_TABLE tSKILL_TABLE, string sNetSerialID, CHECH_BUFF_TYPE buffType)
	{
		bool flag = !string.IsNullOrEmpty(sNetSerialID);
		bool flag2 = false;
		if ((int)tSKILL_TABLE.f_EFFECT_Y == 0)
		{
			if ((int)tSKILL_TABLE.f_EFFECT_X == 0)
			{
				for (int num = listBuffs.Count - 1; num >= 0; num--)
				{
					PerBuff perBuff = listBuffs[num];
					if (CheckBuffType(perBuff.refCTable.n_EFFECT, buffType) && perBuff.refCTable.n_NOT_REMOVABLE == 0)
					{
						if (flag && StageUpdate.gbIsNetGame)
						{
							StageUpdate.SyncStageObj(4, 5, sNetSerialID + "," + 0 + "," + perBuff.nBuffID, true);
						}
						CheckDelBuffTrigger(perBuff.nBuffID, 2);
						RemoveBuffByBuffID(perBuff.nBuffID, false);
						bNeedCalcuStatus = true;
						flag2 = true;
					}
				}
			}
			else
			{
				for (int num2 = listBuffs.Count - 1; num2 >= 0; num2--)
				{
					PerBuff perBuff2 = listBuffs[num2];
					if (CheckBuffType(perBuff2.refCTable.n_EFFECT, buffType) && perBuff2.refCTable.n_NOT_REMOVABLE == 0 && perBuff2.refCTable.n_EFFECT == (int)tSKILL_TABLE.f_EFFECT_X)
					{
						if (flag && StageUpdate.gbIsNetGame)
						{
							StageUpdate.SyncStageObj(4, 5, sNetSerialID + "," + 0 + "," + perBuff2.nBuffID, true);
						}
						CheckDelBuffTrigger(perBuff2.nBuffID, 2);
						RemoveBuffByBuffID(perBuff2.nBuffID, false);
						bNeedCalcuStatus = true;
						flag2 = true;
					}
				}
			}
		}
		else if ((int)tSKILL_TABLE.f_EFFECT_Y > 0)
		{
			if ((int)tSKILL_TABLE.f_EFFECT_X > 0)
			{
				int num3 = (int)tSKILL_TABLE.f_EFFECT_Y;
				for (int num4 = listBuffs.Count - 1; num4 >= 0; num4--)
				{
					if (listBuffs[num4].refCTable.n_EFFECT == (int)tSKILL_TABLE.f_EFFECT_X)
					{
						int nBuffID = listBuffs[num4].nBuffID;
						if (CheckBuffType((int)tSKILL_TABLE.f_EFFECT_X, buffType) && listBuffs[num4].refCTable.n_NOT_REMOVABLE == 0)
						{
							if (flag && StageUpdate.gbIsNetGame)
							{
								StageUpdate.SyncStageObj(4, 5, sNetSerialID + "," + 0 + "," + nBuffID, true);
							}
							CheckDelBuffTrigger(nBuffID, 2);
							RemoveBuffByBuffID(nBuffID, false);
							flag2 = true;
							num3--;
							if (num3 <= 0)
							{
								break;
							}
						}
					}
				}
			}
			else if ((int)tSKILL_TABLE.f_EFFECT_X == 0)
			{
				List<int> list = new List<int>();
				for (int i = 0; i < listBuffs.Count; i++)
				{
					if (CheckBuffType(listBuffs[i].refCTable.n_EFFECT, buffType) && listBuffs[i].refCTable.n_NOT_REMOVABLE == 0)
					{
						list.Add(listBuffs[i].nBuffID);
					}
				}
				for (int j = 0; j < (int)tSKILL_TABLE.f_EFFECT_Y; j++)
				{
					if (list.Count <= 0)
					{
						break;
					}
					int index = OrangeBattleUtility.Random(0, list.Count);
					int num5 = list[index];
					if (flag && StageUpdate.gbIsNetGame)
					{
						StageUpdate.SyncStageObj(4, 5, sNetSerialID + "," + 0 + "," + num5, true);
					}
					flag2 = true;
					CheckDelBuffTrigger(num5, 2);
					RemoveBuffByBuffID(num5, false);
					list.RemoveAt(index);
				}
			}
		}
		if (flag2)
		{
			OrangeCharacter orangeCharacter = SOB as OrangeCharacter;
			if ((bool)orangeCharacter)
			{
				string[] array = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(tSKILL_TABLE.s_USE_SE);
				if (array.Length >= 2)
				{
					orangeCharacter.PlaySE(array[0], array[1]);
				}
			}
		}
		if (bNeedCalcuStatus)
		{
			bNeedCalcuStatus = false;
			CalcuStatus();
		}
	}

	public void StealBuffByBullet(SKILL_TABLE tSKILL_TABLE, PerBuffManager refPBMShoter, PerBuffManager refPBMHit, ref int nMaxCount)
	{
		OrangeCharacter orangeCharacter = refPBMHit.SOB as OrangeCharacter;
		OrangeCharacter orangeCharacter2 = refPBMShoter.SOB as OrangeCharacter;
		if ((int)tSKILL_TABLE.f_EFFECT_Y == 0)
		{
			if ((int)tSKILL_TABLE.f_EFFECT_X == 0)
			{
				for (int num = refPBMHit.listBuffs.Count - 1; num >= 0; num--)
				{
					int n_EFFECT = refPBMHit.listBuffs[num].refCTable.n_EFFECT;
					int n_ID = refPBMHit.listBuffs[num].refCTable.n_ID;
					if (CheckBuffType(n_EFFECT, CHECH_BUFF_TYPE.BUFF) && !CheckBuffIdHasNotRemovable(n_ID) && refPBMHit.listBuffs[num].refCTable.n_STACK_RULE != 4)
					{
						if (orangeCharacter2 != null && orangeCharacter2.IsLocalPlayer)
						{
							if (CheckHasAntiBuff(refPBMHit.listBuffs[num].refCTable.n_ID, refPBMHit.listBuffs[num].refCTable.n_EFFECT))
							{
								refPBMShoter.DoAntiBuff();
							}
							else
							{
								refPBMShoter.AddBuff(refPBMHit.listBuffs[num], false, false, true);
							}
						}
						if (orangeCharacter != null && StageUpdate.gbIsNetGame)
						{
							StageUpdate.SyncStageObj(4, 5, orangeCharacter.sNetSerialID + "," + 0 + "," + refPBMHit.listBuffs[num].nBuffID, true);
						}
						CheckDelBuffTrigger(refPBMHit.listBuffs[num].nBuffID, 2);
						RemoveBuffIndex(num);
						bNeedCalcuStatus = true;
						nMaxCount--;
						if (nMaxCount == 0)
						{
							break;
						}
					}
				}
			}
			else
			{
				for (int num2 = refPBMHit.listBuffs.Count - 1; num2 >= 0; num2--)
				{
					int n_EFFECT2 = refPBMHit.listBuffs[num2].refCTable.n_EFFECT;
					int n_ID2 = refPBMHit.listBuffs[num2].refCTable.n_ID;
					if (n_EFFECT2 == (int)tSKILL_TABLE.f_EFFECT_X && CheckBuffType(n_EFFECT2, CHECH_BUFF_TYPE.BUFF) && !CheckBuffIdHasNotRemovable(n_ID2) && refPBMHit.listBuffs[num2].refCTable.n_STACK_RULE != 4)
					{
						if (orangeCharacter2 != null && orangeCharacter2.IsLocalPlayer)
						{
							if (refPBMShoter.CheckHasAntiBuff(refPBMHit.listBuffs[num2].refCTable.n_ID, refPBMHit.listBuffs[num2].refCTable.n_EFFECT))
							{
								refPBMShoter.DoAntiBuff();
							}
							else
							{
								refPBMShoter.AddBuff(refPBMHit.listBuffs[num2], false, false, true);
							}
						}
						if (orangeCharacter != null && StageUpdate.gbIsNetGame)
						{
							StageUpdate.SyncStageObj(4, 5, orangeCharacter.sNetSerialID + "," + 0 + "," + refPBMHit.listBuffs[num2].nBuffID, true);
						}
						CheckDelBuffTrigger(refPBMHit.listBuffs[num2].nBuffID, 2);
						RemoveBuffIndex(num2);
						bNeedCalcuStatus = true;
						nMaxCount--;
						if (nMaxCount == 0)
						{
							break;
						}
					}
				}
			}
		}
		else if ((int)tSKILL_TABLE.f_EFFECT_X == 0)
		{
			List<int> list = new List<int>();
			for (int num3 = refPBMHit.listBuffs.Count - 1; num3 >= 0; num3--)
			{
				int n_EFFECT3 = refPBMHit.listBuffs[num3].refCTable.n_EFFECT;
				int n_ID3 = refPBMHit.listBuffs[num3].refCTable.n_ID;
				if (CheckBuffType(n_EFFECT3, CHECH_BUFF_TYPE.BUFF) && !CheckBuffIdHasNotRemovable(n_ID3) && refPBMHit.listBuffs[num3].refCTable.n_STACK_RULE != 4)
				{
					list.Add(refPBMHit.listBuffs[num3].nBuffID);
				}
			}
			for (int i = 0; i < (int)tSKILL_TABLE.f_EFFECT_Y; i++)
			{
				if (list.Count <= 0)
				{
					break;
				}
				if (nMaxCount == 0)
				{
					break;
				}
				int index = OrangeBattleUtility.Random(0, list.Count);
				for (int num4 = refPBMHit.listBuffs.Count - 1; num4 >= 0; num4--)
				{
					if (list[index] == refPBMHit.listBuffs[num4].nBuffID)
					{
						if (orangeCharacter2 != null && orangeCharacter2.IsLocalPlayer)
						{
							if (refPBMShoter.CheckHasAntiBuff(refPBMHit.listBuffs[num4].refCTable.n_ID, refPBMHit.listBuffs[num4].refCTable.n_EFFECT))
							{
								refPBMShoter.DoAntiBuff();
							}
							else
							{
								refPBMShoter.AddBuff(refPBMHit.listBuffs[num4], false, false, true);
							}
						}
						if (orangeCharacter != null && StageUpdate.gbIsNetGame)
						{
							StageUpdate.SyncStageObj(4, 5, orangeCharacter.sNetSerialID + "," + 0 + "," + refPBMHit.listBuffs[num4].nBuffID, true);
						}
						CheckDelBuffTrigger(list[index], 2);
						RemoveBuffIndex(num4);
						list.RemoveAt(index);
						bNeedCalcuStatus = true;
						nMaxCount--;
						break;
					}
				}
			}
		}
		else
		{
			int num5 = (int)tSKILL_TABLE.f_EFFECT_Y;
			for (int num6 = refPBMHit.listBuffs.Count - 1; num6 >= 0; num6--)
			{
				int n_EFFECT4 = refPBMHit.listBuffs[num6].refCTable.n_EFFECT;
				int n_ID4 = refPBMHit.listBuffs[num6].refCTable.n_ID;
				if (n_EFFECT4 == (int)tSKILL_TABLE.f_EFFECT_X && CheckBuffType(n_EFFECT4, CHECH_BUFF_TYPE.BUFF) && !CheckBuffIdHasNotRemovable(n_ID4) && refPBMHit.listBuffs[num6].refCTable.n_STACK_RULE != 4)
				{
					if (orangeCharacter2 != null && orangeCharacter2.IsLocalPlayer)
					{
						if (refPBMShoter.CheckHasAntiBuff(refPBMHit.listBuffs[num6].refCTable.n_ID, refPBMHit.listBuffs[num6].refCTable.n_EFFECT))
						{
							refPBMShoter.DoAntiBuff();
						}
						else
						{
							refPBMShoter.AddBuff(refPBMHit.listBuffs[num6], false, false, true);
						}
					}
					if (orangeCharacter != null && StageUpdate.gbIsNetGame)
					{
						StageUpdate.SyncStageObj(4, 5, orangeCharacter.sNetSerialID + "," + 0 + "," + refPBMHit.listBuffs[num6].nBuffID, true);
					}
					CheckDelBuffTrigger(refPBMHit.listBuffs[num6].nBuffID, 2);
					RemoveBuffIndex(num6);
					bNeedCalcuStatus = true;
					num5--;
					nMaxCount--;
					if (num5 <= 0 || nMaxCount == 0)
					{
						break;
					}
				}
			}
		}
		if (bNeedCalcuStatus)
		{
			CalcuStatus();
			refPBMShoter.CalcuStatus();
		}
		if (this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
	}

	private bool CheckBuffType(int effectId, CHECH_BUFF_TYPE buffType)
	{
		if (buffType == CHECH_BUFF_TYPE.ALL || (buffType == CHECH_BUFF_TYPE.BUFF && effectId < 101) || (buffType == CHECH_BUFF_TYPE.DEBUFF && effectId >= 101 && effectId < 125))
		{
			return true;
		}
		return false;
	}

	public void ClearBuff()
	{
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			RemoveBuffIndex(0);
		}
		CalcuStatus();
		if (this.UpdateBuffBar != null)
		{
			this.UpdateBuffBar(this);
		}
	}

	public void AddBuffAndApplyCurrentBuffStatus(SKILL_TABLE tSKILL_TABLE)
	{
		PerBuff perBuff = listBuffs.Find((PerBuff x) => x.nBuffID == (int)tSKILL_TABLE.f_EFFECT_X);
		if (perBuff == null)
		{
			return;
		}
		tAddPerBuff = listBuffs.Find((PerBuff x) => x.nBuffID == (int)tSKILL_TABLE.f_EFFECT_Y);
		if (tAddPerBuff != null)
		{
			if (tAddPerBuff.bWaitNetDel)
			{
				tAddPerBuff.bWaitNetDel = false;
			}
			tAddPerBuff.nStack = perBuff.nStack;
			tAddPerBuff.fDuration = perBuff.fDuration;
			tAddPerBuff.fLeftTime = perBuff.fLeftTime;
		}
		else
		{
			int num = (int)tSKILL_TABLE.f_EFFECT_Y;
			CONDITION_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(num, out value);
			if (value != null)
			{
				tAddPerBuff = new PerBuff();
				tAddPerBuff.refCTable = value;
				tAddPerBuff.nBuffID = num;
				tAddPerBuff.sPlayerID = perBuff.sPlayerID;
				tAddPerBuff.fDuration = perBuff.fDuration;
				tAddPerBuff.nStack = perBuff.nStack;
				tAddPerBuff.nOtherParam1 = perBuff.nOtherParam1;
				listBuffs.Add(tAddPerBuff);
			}
		}
		CalcuStatus();
		if (SOB != null && SOB.GetSOBType() == 1)
		{
			tAddPerBuff.bWaitNetSyncTime = SOB.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		}
		else
		{
			tAddPerBuff.bWaitNetSyncTime = false;
		}
		tAddPerBuff.bWaitNetSyncAdd = true;
	}

	public void AddMeasure(int nAdd, bool bNeedSync = false)
	{
		if (nMeasureNow < nMeasureMax || nAdd < 0)
		{
			nMeasureNow += nAdd;
			if (nMeasureNow > nMeasureMax)
			{
				nMeasureNow = nMeasureMax;
			}
			if (nMeasureNow < 0)
			{
				nMeasureNow = 0;
			}
			if (this.UpdateBuffBar != null)
			{
				this.UpdateBuffBar(this);
			}
		}
		if (bNeedSync)
		{
			StageUpdate.SyncStageObj(4, 3, tSOB.sNetSerialID + "," + nAdd, true);
		}
	}

	private bool NetCheckRemoveBuffIndex(int nIndex)
	{
		if (m_sBuffStatus.refPBM.SOB != null && m_sBuffStatus.refPBM.SOB.GetSOBType() == 1)
		{
			OrangeCharacter orangeCharacter = m_sBuffStatus.refPBM.SOB as OrangeCharacter;
			if (orangeCharacter.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && !listBuffs[nIndex].bWaitNetDel && !orangeCharacter.bNeedUpdateAlways && !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckPlayerPause(orangeCharacter.sNetSerialID))
			{
				listBuffs[nIndex].bWaitNetDel = true;
				listBuffs[nIndex].bWaitNetSyncAdd = false;
				return false;
			}
		}
		listBuffs.RemoveAt(nIndex);
		return true;
	}

	public int ReduceDmgByEnergyShild(int nDmg)
	{
		if (m_sBuffStatus.nEnergyShield > 0 && nDmg > 0)
		{
			PerBuff perBuff = null;
			for (int i = 0; i < listBuffs.Count; i++)
			{
				perBuff = listBuffs[i];
				if (!perBuff.bWaitNetDel && perBuff.refCTable.n_EFFECT == 6)
				{
					if (nDmg < perBuff.nStack)
					{
						perBuff.nStack -= nDmg;
						perBuff.bWaitNetSyncStack = true;
						nDmg = 0;
						break;
					}
					nDmg -= perBuff.nStack;
					CheckDelBuffTrigger(perBuff.nBuffID, 3);
					if (RemoveBuffIndex(i))
					{
						i--;
					}
				}
			}
			CalcuStatus();
			if (this.UpdateBuffBar != null)
			{
				this.UpdateBuffBar(this);
			}
		}
		return nDmg;
	}

	public int GetEnergyShildBuffId()
	{
		if (m_sBuffStatus.nEnergyShield > 0)
		{
			foreach (PerBuff listBuff in listBuffs)
			{
				if (listBuff.refCTable.n_EFFECT == 6)
				{
					return listBuff.nBuffID;
				}
			}
		}
		return 0;
	}

	public bool IsHasBuffType(BUFF_TYPE tBUFF_TYPE)
	{
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].refCTable.n_EFFECT == (int)tBUFF_TYPE)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHasBuffType(int tBUFF_TYPE)
	{
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].refCTable.n_EFFECT == tBUFF_TYPE)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHasType(int type)
	{
		for (int num = listBuffs.Count - 1; num >= 0; num--)
		{
			if (listBuffs[num].refCTable.n_TYPE == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckHasBuffs(BUFF_TYPE tBUFF_TYPE, out CONDITION_TABLE[] tCONDITION_TABLES)
	{
		tCONDITION_TABLES = (from buff in listBuffs
			where buff.refCTable.n_EFFECT == (int)tBUFF_TYPE
			select buff.refCTable).ToArray();
		if (tCONDITION_TABLES != null)
		{
			return tCONDITION_TABLES.Length != 0;
		}
		return false;
	}

	public void CalcuStatus()
	{
		bNeedCalcuStatus = false;
		m_sBuffStatus.fAtkDmgPercent = fInitAtkDmgPercent;
		m_sBuffStatus.fCriDmgPercent = 0f;
		m_sBuffStatus.fCriPercent = 0f;
		m_sBuffStatus.fMoveSpeed = 0f;
		m_sBuffStatus.fReduceDmgPercent = 0f;
		m_sBuffStatus.fMissPercent = 0f;
		m_sBuffStatus.nEnergyShield = 0;
		m_sBuffStatus.nEnergyShieldMax = 0;
		m_sBuffStatus.nHealEnhance = 0;
		m_sBuffStatus.refPBM = this;
		float num = 0f;
		int num2 = 0;
		int num3 = 1;
		OrangeCharacter orangeCharacter = tSOB as OrangeCharacter;
		int count = listBuffs.Count;
		PerBuff perBuff = null;
		for (int i = 0; i < count; i++)
		{
			perBuff = listBuffs[i];
			if (perBuff.bWaitNetDel)
			{
				continue;
			}
			switch (perBuff.refCTable.n_EFFECT)
			{
			case 1:
				m_sBuffStatus.fAtkDmgPercent += perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 2:
				m_sBuffStatus.fReduceDmgPercent += perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 3:
				m_sBuffStatus.fCriPercent += perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 4:
				m_sBuffStatus.fCriDmgPercent += perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 5:
				num += perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 6:
				m_sBuffStatus.nEnergyShield += perBuff.nStack;
				m_sBuffStatus.nEnergyShieldMax += perBuff.nOtherParam1;
				break;
			case 101:
				m_sBuffStatus.fAtkDmgPercent -= perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 102:
				m_sBuffStatus.fReduceDmgPercent -= perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 103:
				m_sBuffStatus.fCriPercent -= perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 104:
				m_sBuffStatus.fCriDmgPercent -= perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 105:
			case 114:
				num -= perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 120:
				num2 += perBuff.nStack;
				if (perBuff.refCTable.n_MAX_STACK > num3)
				{
					num3 = perBuff.refCTable.n_MAX_STACK;
				}
				break;
			case 122:
				m_sBuffStatus.fMissPercent += perBuff.refCTable.f_EFFECT_X * (float)perBuff.nStack;
				break;
			case 13:
				m_sBuffStatus.nHealEnhance += (int)(perBuff.refCTable.f_EFFECT_X + perBuff.refCTable.f_EFFECT_Y * (float)perBuff.nStack);
				break;
			case 124:
				m_sBuffStatus.nHealEnhance -= (int)(perBuff.refCTable.f_EFFECT_X + perBuff.refCTable.f_EFFECT_Y * (float)perBuff.nStack);
				break;
			}
		}
		if (num < -100f)
		{
			num = -100f;
		}
		m_sBuffStatus.fMoveSpeed = num;
		if (orangeCharacter != null && num2 > 0)
		{
			if (num2 > m_sBuffStatus.nLaskBlackStack)
			{
				m_sBuffStatus.nLaskBlackStack = num2;
			}
			if (m_sBuffStatus.nLaskBlackStack > num3)
			{
				m_sBuffStatus.nLaskBlackStack = num3;
			}
			num2 = m_sBuffStatus.nLaskBlackStack;
			float num4 = 1f - (float)num2 / (float)num3;
			orangeCharacter.CharacterMaterials.MultiColorColor(num4, num4, num4);
			if (orangeCharacter.PlayerWeapons[0].WeaponMesh != null && orangeCharacter.PlayerWeapons[0].WeaponMesh[0] != null)
			{
				orangeCharacter.PlayerWeapons[0].WeaponMesh[0].MultiColorColor(num4, num4, num4);
			}
			if (orangeCharacter.PlayerWeapons[1].WeaponMesh != null && orangeCharacter.PlayerWeapons[1].WeaponMesh[0] != null)
			{
				orangeCharacter.PlayerWeapons[1].WeaponMesh[0].MultiColorColor(num4, num4, num4);
			}
		}
		if (tSOB != null)
		{
			tSOB.BuffChangeCheck();
		}
	}

	public int GetBuffStack(int tEffectID)
	{
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_EFFECT == tEffectID)
			{
				return listBuffs[i].nStack;
			}
		}
		return -1;
	}

	public void BuffStackDown(int buffID, int subValue)
	{
		for (int i = 0; i < listBuffs.Count; i++)
		{
			if (listBuffs[i].refCTable.n_ID == buffID && listBuffs[i].IsBuffOk())
			{
				if (listBuffs[i].nStack > subValue)
				{
					listBuffs[i].nStack -= subValue;
					break;
				}
				CheckDelBuffTrigger(listBuffs[i].nBuffID, 2);
				RemoveBuffIndex(i);
				break;
			}
		}
	}

	public void SetBuffTime(int tEffectID, float time)
	{
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_EFFECT == tEffectID)
			{
				listBuffs[i].fDuration = time;
			}
		}
	}

	public void SetBuffTimeID(int tBuffID, float time)
	{
		int count = listBuffs.Count;
		for (int i = 0; i < count; i++)
		{
			if (listBuffs[i].refCTable.n_ID == tBuffID)
			{
				listBuffs[i].fDuration = time;
			}
		}
	}

	public void StopLoopSE()
	{
		OrangeCriSource component = tSOB.gameObject.GetComponent<OrangeCriSource>();
		if (!(component != null))
		{
			return;
		}
		foreach (PerBuff listBuff in listBuffs)
		{
			string[] array = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(listBuff.refCTable.s_DURING_SE);
			component.PlaySE(array[0], array[2]);
		}
	}

	public void CheckDelBuffTrigger(int buffId, int triggerType, bool forceTrigger = false)
	{
		OrangeCharacter orangeCharacter = tSOB as OrangeCharacter;
		if (!(orangeCharacter == null) && triggerType > 0 && (CheckHasEffectByCONDITIONID(buffId) || forceTrigger))
		{
			orangeCharacter.tRefPassiveskill.DelBuffTrigger(buffId, triggerType, ref orangeCharacter.selfBuffManager);
		}
	}
}
