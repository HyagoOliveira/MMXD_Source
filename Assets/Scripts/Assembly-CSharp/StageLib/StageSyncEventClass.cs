#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using OrangeApi;
using UnityEngine;
using enums;

namespace StageLib
{
	public class StageSyncEventClass
	{
		public class SyncOCData
		{
			[JsonProperty("a")]
			public string sPlayerID;

			[JsonProperty("b")]
			public Vector3 tOcPos;

			[JsonProperty("c")]
			public int nLookDir;

			[JsonProperty("d")]
			public int nHP;

			[JsonProperty("e")]
			public int nRecord;

			[JsonProperty("f")]
			public int nCharID;

			[JsonProperty("g")]
			public int WeaponCurrent;

			[JsonProperty("h")]
			public int nMeasureNow;

			[JsonProperty("i")]
			public bool bUsePassiveskill;

			[JsonProperty("j")]
			public int HealHp;

			[JsonProperty("k")]
			public int DmgHp;

			[JsonProperty("l")]
			public List<PerBuff> listSyncBuff;
		}

		public delegate void CallDelegate(string sMsg);

		private class SortStageSLBaseHelper : IComparer
		{
			int IComparer.Compare(object a, object b)
			{
				StageSLBase tStageSLBase = (StageSLBase)a;
				StageSLBase tStageSLBase2 = (StageSLBase)b;
				string[] syncKeys = StageSLBase.GetSyncKeys(tStageSLBase);
				string[] syncKeys2 = StageSLBase.GetSyncKeys(tStageSLBase2);
				int num = int.Parse(syncKeys[0]);
				int num2 = int.Parse(syncKeys2[0]);
				while (true)
				{
					if (num > num2)
					{
						return 1;
					}
					if (num < num2)
					{
						return -1;
					}
					if (syncKeys[1] == "" && syncKeys2[1] == "")
					{
						return 0;
					}
					if (syncKeys[1] == "")
					{
						return 1;
					}
					if (syncKeys2[1] == "")
					{
						break;
					}
					syncKeys = StageSLBase.GetSyncKeys(syncKeys[1]);
					syncKeys2 = StageSLBase.GetSyncKeys(syncKeys2[1]);
					num = int.Parse(syncKeys[0]);
					num2 = int.Parse(syncKeys2[0]);
				}
				return -1;
			}
		}

		private string[] splitstrs;

		private PvpBarUI tPvpBarUI;

		private OrangeCharacter tOC;

		private string sParam0;

		private string sParam1;

		private int nParam0;

		private int nParam1;

		private int nParam2;

		private float fParam0;

		private float fParam1;

		private VInt3 vi3Param0;

		private List<SyncOCData> listSyncOCData = new List<SyncOCData>();

		public StageUpdate _tStageUpdate;

		private List<CallDelegate> listSyncAction = new List<CallDelegate>();

		private int nMaxCount;

		public StageUpdate tStageUpdate
		{
			get
			{
				if (_tStageUpdate == null)
				{
					_tStageUpdate = StageResManager.GetStageUpdate();
				}
				return _tStageUpdate;
			}
		}

		public StageSyncEventClass()
		{
			Type type = GetType();
			for (int i = 0; i < 23; i++)
			{
				STAGE_EVENT sTAGE_EVENT = (STAGE_EVENT)i;
				MethodInfo method = type.GetMethod(sTAGE_EVENT.ToString());
				if (method != null)
				{
					listSyncAction.Add((CallDelegate)Delegate.CreateDelegate(typeof(CallDelegate), this, method));
				}
				else
				{
					Debug.LogError("StageSyncEventClass Has No " + sTAGE_EVENT);
				}
			}
			nMaxCount = listSyncAction.Count;
		}

		public void OnSyncStageEvent(string sIDKey, int nKey1, string sMsg)
		{
			if (nKey1 >= 0 && nKey1 <= nMaxCount)
			{
				listSyncAction[nKey1](sMsg);
			}
			else
			{
				Debug.LogError("Out STAGE_EVENT Range :" + nKey1);
			}
		}

		public void NONE(string sMsg)
		{
		}

		public void STAGE_VOICE_ON(string sMsg)
		{
			sParam0 = sMsg + "連接語音了!!";
		}

		public void STAGE_ENEMYEVENT_STOP(string sMsg)
		{
		}

		public void STAGE_PLAYER_REBORN(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			if (splitstrs[1] == "0")
			{
				tOC = StageUpdate.GetPlayerByID(splitstrs[0]);
				if ((tOC != null && (int)tOC.Hp > 0) || BattleInfoUI.Instance.CheckPlayerCanReborn(splitstrs[0]))
				{
					StageUpdate.SyncStageObj(3, 3, splitstrs[0] + ",1," + splitstrs[2], true, true);
				}
			}
			else if (splitstrs[1] == "1")
			{
				if (!(splitstrs[0] == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify))
				{
					return;
				}
				tOC = StageUpdate.GetMainPlayerOC();
				if (tOC != null && (int)tOC.Hp > 0)
				{
					StageUpdate.SyncStageObj(3, 3, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + ",2," + splitstrs[2] + "," + ManagedSingleton<InputStorage>.Instance.GetInputRecordNO(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify), true);
					BattleInfoUI.Instance.CloseSendRebornMsgtCoroutine();
				}
				else
				{
					if (!BattleInfoUI.Instance.CheckPlayerCanReborn(splitstrs[0]))
					{
						return;
					}
					BattleInfoUI.Instance.CloseSendRebornMsgtCoroutine();
					StageUpdate.SyncStageObj(3, 3, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + ",2," + splitstrs[2] + "," + ManagedSingleton<InputStorage>.Instance.GetInputRecordNO(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify), true);
					if (splitstrs[2] == "1")
					{
						ManagedSingleton<PlayerNetManager>.Instance.StageContinueReq(ManagedSingleton<StageHelper>.Instance.nLastStageID, delegate
						{
							Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, false, 0f, 0f, null);
							BattleInfoUI.Instance.SwitchOptionBtn(true, 1);
							BattleInfoUI.Instance.UnRegisterRebornPlayerList(splitstrs[0]);
						});
					}
					else
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, false, 0f, 0f, null);
						BattleInfoUI.Instance.SwitchOptionBtn(true, 1);
						BattleInfoUI.Instance.UnRegisterRebornPlayerList(splitstrs[0]);
					}
				}
			}
			else
			{
				if (!(splitstrs[1] == "2"))
				{
					return;
				}
				BattleInfoUI.Instance.UnRegisterRebornPlayerList(splitstrs[0]);
				if (splitstrs.Length >= 5)
				{
					ManagedSingleton<InputStorage>.Instance.SetInputRecordNO(splitstrs[0], int.Parse(splitstrs[5]));
					if (splitstrs.Length >= 6)
					{
						if (splitstrs[6] == "1")
						{
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CONTINUE_PLATER, splitstrs[0], true, float.Parse(splitstrs[3]), float.Parse(splitstrs[4]), (bool?)true);
						}
						else
						{
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CONTINUE_PLATER, splitstrs[0], true, float.Parse(splitstrs[3]), float.Parse(splitstrs[4]), (bool?)false);
						}
					}
					else
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, splitstrs[0], true, float.Parse(splitstrs[3]), float.Parse(splitstrs[4]), null);
					}
				}
				else
				{
					ManagedSingleton<InputStorage>.Instance.SetInputRecordNO(splitstrs[0], int.Parse(splitstrs[3]));
					Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, splitstrs[0], false, 0f, 0f, null);
				}
			}
		}

		public void STAGE_SYNC_COUNTDOWN(string sMsg)
		{
			fParam0 = float.Parse(sMsg);
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && (BattleInfoUI.Instance.NowStageTable.n_MAIN != 90001 || BattleInfoUI.Instance.NowStageTable.n_SUB != 3))
			{
				tPvpBarUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpBarUI>("UI_PvpBar");
				if (tPvpBarUI != null)
				{
					tPvpBarUI.SetTime(fParam0);
				}
			}
			else if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowStageCountDownTime(fParam0);
			}
		}

		public void STAGE_SYNC_PVPUI(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			tPvpBarUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpBarUI>("UI_PvpBar");
			if (tPvpBarUI != null)
			{
				tPvpBarUI.UpdageLifeByNet(int.Parse(splitstrs[0]), int.Parse(splitstrs[1]), float.Parse(splitstrs[2]));
			}
		}

		public void STAGE_SYNC_SCORE(string sMsg)
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count <= 0)
			{
				return;
			}
			splitstrs = sMsg.Split(',');
			nParam0 = 0;
			int.TryParse(splitstrs[1], out nParam0);
			for (int num = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count - 1; num >= 0; num--)
			{
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num].PlayerId == splitstrs[0])
				{
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num].nScore += nParam0;
					break;
				}
			}
		}

		public void STAGE_SYNC_RECONNECTEVENT(string sMsg)
		{
			if (StageUpdate.bIsHost)
			{
				MonoBehaviourSingleton<StageSyncManager>.Instance.SendChangeHost("," + MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
			}
			tStageUpdate.StartCoroutine(WaitAllPlayerOnAndSendReConnectData(sMsg));
		}

		public void STAGE_SYNC_OBJ(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify != splitstrs[0])
			{
				return;
			}
			int num = int.Parse(splitstrs[1]);
			int nIndex = 2;
			if (num != 1)
			{
				StageUpdate.bWaitReconnect = false;
				if (tStageUpdate.tWaitReconnectCoroutine != null)
				{
					tStageUpdate.StopCoroutine(tStageUpdate.tWaitReconnectCoroutine);
					tStageUpdate.tWaitReconnectCoroutine = null;
				}
			}
			switch (num)
			{
			case 0:
				STAGE_SYNC_OBJ_PLAYER(sMsg);
				break;
			case 1:
				STAGE_SYNC_OBJ_USETIME(nIndex, splitstrs);
				break;
			case 2:
				STAGE_SYNC_OBJ_MEMBERINFO(nIndex, splitstrs);
				break;
			case 3:
				STAGE_SYNC_OBJ_SEASONDATA(sMsg);
				break;
			case 4:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
				tStageUpdate.tStageOpenCommonTask.CloseCommon();
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect = false;
				break;
			case 5:
				STAGE_SYNC_OBJ_ENEMY_DATA(nIndex, splitstrs);
				break;
			case 6:
				STAGE_SYNC_OBJ_GAMESAVEDATA(sMsg);
				break;
			}
		}

		private void STAGE_SYNC_OBJ_PLAYER(string sMsg)
		{
			sParam0 = sMsg;
			sParam0 = sParam0.Substring(sParam0.IndexOf(',') + 1);
			sParam0 = sParam0.Substring(sParam0.IndexOf(',') + 1);
			listSyncOCData = JsonConvert.DeserializeObject<List<SyncOCData>>(sParam0);
			for (int num = listSyncOCData.Count - 1; num >= 0; num--)
			{
				SyncOCData syncOCData = listSyncOCData[num];
				VInt3 vInt = new VInt3(syncOCData.tOcPos);
				ManagedSingleton<InputStorage>.Instance.AddInputData(syncOCData.sPlayerID);
				ManagedSingleton<InputStorage>.Instance.SetInputRecordNO(syncOCData.sPlayerID, syncOCData.nRecord);
				bool flag = false;
				for (int num2 = StageUpdate.runPlayers.Count - 1; num2 >= 0; num2--)
				{
					if (StageUpdate.runPlayers[num2].sPlayerID == syncOCData.sPlayerID)
					{
						if (syncOCData.nCharID == StageUpdate.runPlayers[num2].CharacterID)
						{
							StageUpdate.runPlayers[num2].Controller.LogicPosition = new VInt3(vInt.vec3);
							StageUpdate.runPlayers[num2].transform.localPosition = vInt.vec3;
							StageUpdate.runPlayers[num2].vLastMovePt = vInt.vec3;
							if (StageUpdate.runPlayers[num2].WeaponCurrent != syncOCData.WeaponCurrent)
							{
								StageUpdate.runPlayers[num2].PlayerPressSelectCB();
							}
							if ((int)StageUpdate.runPlayers[num2].Hp <= 0)
							{
								if (syncOCData.nHP <= 0)
								{
									if (syncOCData.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
									{
										BattleInfoUI.Instance.ShowContinueSelect();
									}
								}
								else
								{
									Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, syncOCData.sPlayerID, false, 0f, 0f, null);
									StageUpdate.runPlayers[num2].Hp = syncOCData.nHP;
									StageUpdate.runPlayers[num2].selfBuffManager.SyncByNetBuff(syncOCData.listSyncBuff, true);
								}
							}
							else
							{
								StageUpdate.runPlayers[num2].Hp = syncOCData.nHP;
								StageUpdate.runPlayers[num2].UpdateHurtAction();
								if (syncOCData.nHP <= 0)
								{
									StageUpdate.runPlayers[num2].PlayerDead();
								}
								StageUpdate.runPlayers[num2].selfBuffManager.nMeasureNow = syncOCData.nMeasureNow;
								StageUpdate.runPlayers[num2].selfBuffManager.SyncByNetBuff(syncOCData.listSyncBuff, true);
								if (StageUpdate.runPlayers[num2].tRefPassiveskill.bUsePassiveskill != syncOCData.bUsePassiveskill)
								{
									StageUpdate.runPlayers[num2].PlayerPressChip();
								}
								StageUpdate.runPlayers[num2].HealHp = syncOCData.HealHp;
								StageUpdate.runPlayers[num2].DmgHp = syncOCData.DmgHp;
							}
							flag = true;
							if (syncOCData.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
							{
								StageUpdate.runPlayers[num2].IsLocalPlayer = true;
							}
							OrangeNetCharacter orangeNetCharacter = StageUpdate.runPlayers[num2] as OrangeNetCharacter;
							if (orangeNetCharacter != null)
							{
								orangeNetCharacter.ClearCommandQueue();
							}
						}
						else
						{
							StageUpdate.runPlayers[num2].PlayerDead(true);
						}
						break;
					}
				}
				if (!flag)
				{
					EventManager.StageGeneratePlayer stageGeneratePlayer = new EventManager.StageGeneratePlayer();
					stageGeneratePlayer.nMode = 1;
					stageGeneratePlayer.sPlayerID = syncOCData.sPlayerID;
					stageGeneratePlayer.vPos = vInt.vec3;
					stageGeneratePlayer.nHP = syncOCData.nHP;
					stageGeneratePlayer.nCharacterID = syncOCData.nCharID;
					stageGeneratePlayer.WeaponCurrent = syncOCData.WeaponCurrent;
					stageGeneratePlayer.bLookDir = false;
					if (syncOCData.nLookDir == -1)
					{
						stageGeneratePlayer.bLookDir = true;
					}
					stageGeneratePlayer.nMeasureNow = syncOCData.nMeasureNow;
					stageGeneratePlayer.bUsePassiveskill = syncOCData.bUsePassiveskill;
					stageGeneratePlayer.HealHp = syncOCData.HealHp;
					stageGeneratePlayer.DmgHp = syncOCData.DmgHp;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_GENERATE_PVE_PLAYER, stageGeneratePlayer);
				}
			}
		}

		private void STAGE_SYNC_OBJ_USETIME(int nIndex, string[] splitstrs)
		{
			tStageUpdate.fStageUseTime = float.Parse(splitstrs[nIndex++]);
			if (splitstrs.Length > nIndex)
			{
				tStageUpdate.gbAddStageUseTime = false;
				tStageUpdate.bWaitNetStageUseTime = false;
			}
		}

		private void STAGE_SYNC_OBJ_MEMBERINFO(int nIndex, string[] splitstrs)
		{
			while (nIndex < splitstrs.Length)
			{
				string text = splitstrs[nIndex++];
				int nKillNum = int.Parse(splitstrs[nIndex++]);
				int nKillEnemyNum = int.Parse(splitstrs[nIndex++]);
				int nLifePercent = int.Parse(splitstrs[nIndex++]);
				int nScore = int.Parse(splitstrs[nIndex++]);
				int nNowCharacterID = int.Parse(splitstrs[nIndex++]);
				int nALLDMG = int.Parse(splitstrs[nIndex++]);
				for (int i = 0; i < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count; i++)
				{
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId == text)
					{
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nKillNum = nKillNum;
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nKillEnemyNum = nKillEnemyNum;
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nLifePercent = nLifePercent;
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nScore = nScore;
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nNowCharacterID = nNowCharacterID;
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].nALLDMG = nALLDMG;
					}
				}
			}
		}

		private void STAGE_SYNC_OBJ_SEASONDATA(string sMsg)
		{
			sMsg = sMsg.Substring(sMsg.IndexOf(',') + 1);
			sMsg = sMsg.Substring(sMsg.IndexOf(',') + 1);
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.idcSeasonCharaterInfoList = JsonConvert.DeserializeObject<List<Dictionary<int, OrangeBattleServerManager.SeasonCharaterInfo>>>(sMsg.Substring(0, sMsg.IndexOf('#')));
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.listCmdSeasonBattleInfoReq = JsonConvert.DeserializeObject<List<SeasonBattleInfoReq>>(sMsg.Substring(sMsg.IndexOf('#') + 1));
		}

		private void STAGE_SYNC_OBJ_ENEMY_DATA(int nIndex, string[] splitstrs)
		{
			List<StageUpdate.EnemyCtrlID> list = new List<StageUpdate.EnemyCtrlID>();
			for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
			{
				if (((uint)StageUpdate.runEnemys[i].nEnemyBitParam & 0x10u) != 0)
				{
					list.Add(StageUpdate.runEnemys[i]);
				}
			}
			bool flag = true;
			bool flag2 = false;
			string text = "";
			float num = 0f;
			float num2 = 0f;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int[] array = null;
			nIndex++;
			while (nIndex < splitstrs.Length)
			{
				text = splitstrs[nIndex++];
				int nMobGroupID = int.Parse(splitstrs[nIndex++]);
				int nBitParam = int.Parse(splitstrs[nIndex++]);
				int nSCEID = int.Parse(splitstrs[nIndex++]);
				float fSetAimRange = float.Parse(splitstrs[nIndex++]);
				float fSetAimRangeY = float.Parse(splitstrs[nIndex++]);
				float fSetOffsetX = float.Parse(splitstrs[nIndex++]);
				float fSetOffsetY = float.Parse(splitstrs[nIndex++]);
				num = float.Parse(splitstrs[nIndex++]);
				num2 = float.Parse(splitstrs[nIndex++]);
				num3 = int.Parse(splitstrs[nIndex++]);
				num4 = int.Parse(splitstrs[nIndex++]);
				num5 = int.Parse(splitstrs[nIndex++]);
				num6 = int.Parse(splitstrs[nIndex++]);
				num7 = int.Parse(splitstrs[nIndex++]);
				array = new int[num7];
				for (int j = 0; j < num7; j++)
				{
					array[j] = int.Parse(splitstrs[nIndex++]);
				}
				flag = true;
				if (num6 == 1)
				{
					flag = false;
				}
				flag2 = false;
				for (int num8 = list.Count - 1; num8 >= 0; num8--)
				{
					if (list[num8].mEnemy != null && list[num8].mEnemy.sNetSerialID == text)
					{
						list[num8].mEnemy.SetPositionAndRotation(new Vector3(num, num2, 0f), flag);
						list[num8].mEnemy.Hp = num3;
						list[num8].mEnemy.HealHp = num4;
						list[num8].mEnemy.DmgHp = num5;
						list[num8].mEnemy.Hurt(new HurtPassParam());
						for (int k = 0; k < list[num8].mEnemy.PartHp.Length && k < num7; k++)
						{
							list[num8].mEnemy.PartHp[k] = array[k];
						}
						list.RemoveAt(num8);
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemy(nMobGroupID, text, nBitParam, nSCEID, fSetAimRange, fSetAimRangeY, fSetOffsetX, fSetOffsetY);
					enemyControllerBase.SetPositionAndRotation(new Vector3(num, num2, 0f), flag);
					enemyControllerBase.Hp = num3;
					enemyControllerBase.HealHp = num4;
					enemyControllerBase.DmgHp = num5;
					enemyControllerBase.Hurt(new HurtPassParam());
					for (int l = 0; l < enemyControllerBase.PartHp.Length && l < num7; l++)
					{
						enemyControllerBase.PartHp[l] = array[l];
					}
					enemyControllerBase.SetActive(true);
				}
			}
			for (int num9 = list.Count - 1; num9 >= 0; num9--)
			{
				list[num9].mEnemy.Hp = 0;
				list[num9].mEnemy.Hurt(new HurtPassParam());
			}
			list.Clear();
		}

		private void STAGE_SYNC_OBJ_GAMESAVEDATA(string sMsg)
		{
			sParam0 = sMsg;
			sParam0 = sParam0.Substring(sParam0.IndexOf(',') + 1);
			sParam0 = sParam0.Substring(sParam0.IndexOf(',') + 1);
			StageUpdate.SetPerGameSavaData(JsonConvert.DeserializeObject<List<string>>(sParam0));
		}

		public void STAGE_PLAYER_CHANGECHARACTER(string sMsg)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ChangeNextCharacter(sMsg);
		}

		public void STAGE_SYNC_BATTLETIMER(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowStageStartTimer(float.Parse(splitstrs[0]), float.Parse(splitstrs[1]));
			}
		}

		public void STAGE_SET_RECONNECT(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			if (splitstrs[0] == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				if (!MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.isStart)
				{
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect = true;
					return;
				}
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect = true;
				StageUpdate.ReqChangeHost();
				tStageUpdate.SendReConnectMsg();
			}
		}

		public void STAGE_SYNC_FLAG_SCORE(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.NetSetFlagScore(int.Parse(splitstrs[0]), int.Parse(splitstrs[1]), int.Parse(splitstrs[2]));
			}
		}

		public void STAGE_SYNC_STAGEFALL(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			if (splitstrs.Length > 1)
			{
				OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
				string text = splitstrs[1];
				if (text == "1")
				{
					if (mainPlayerOC == null)
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect = false;
						tStageUpdate.IsEnd = true;
						ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
						tStageUpdate.tStageOpenCommonTask.OpenCommon(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TIP"), BattleInfoUI.Instance.StageOutGO);
					}
					else
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
						tStageUpdate.tStageOpenCommonTask.CloseCommon();
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
					}
				}
				else
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect = false;
					tStageUpdate.IsEnd = true;
					ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
					tStageUpdate.tStageOpenCommonTask.OpenCommon(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TIP"), BattleInfoUI.Instance.StageOutGO);
				}
			}
			else if (!StageUpdate.bIsHost)
			{
				BattleInfoUI.Instance.ShowStageLostByContinueRoot();
			}
		}

		public void STAGE_SYNC_PVPENDDATA(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			int num = int.Parse(splitstrs[1]);
			if (num == 2)
			{
				MonoBehaviourSingleton<StageSyncManager>.Instance.SendStageEndAndOpenPvpEndUI(num, int.Parse(splitstrs[2]) == 1);
			}
			else if (int.Parse(splitstrs[0]) == MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify))
			{
				MonoBehaviourSingleton<StageSyncManager>.Instance.SendStageEndAndOpenPvpEndUI(num, int.Parse(splitstrs[2]) == 1);
			}
			else
			{
				MonoBehaviourSingleton<StageSyncManager>.Instance.SendStageEndAndOpenPvpEndUI(1 - num, int.Parse(splitstrs[2]) == 1);
			}
		}

		public void STAGE_SYNC_WAITSTAGECTRL(string sMsg)
		{
			tStageUpdate.fWaitReconnectCoroutineTime = 1f;
		}

		public void STAGE_SYNC_PLAYERGIVEUP(string sMsg)
		{
			tOC = StageUpdate.GetPlayerByID(sMsg);
			if (!StageUpdate.gbRegisterPvpPlayer && tOC != null)
			{
				tStageUpdate.StartCoroutine(WaitReMovePlayerCoroutine(sMsg));
			}
		}

		public void STAGE_SYNC_PLAYERSPAWN(string sMsg)
		{
			tStageUpdate.StartCoroutine(NotifyPlayerNetCtrlOnCoroutine(sMsg));
		}

		public void STAGE_ADD_COUNTDOWN(string sMsg)
		{
		}

		private IEnumerator NotifyPlayerNetCtrlOnCoroutine(string sPlayerID)
		{
			while (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count != 0 && StageUpdate.GetPlayerByID(sPlayerID) == null)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.PLAYERBUILD_PLAYER_NETCTRLON, sPlayerID, false);
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		public void STAGE_SYNC_PLAYEROUTGAME(string sMsg)
		{
			splitstrs = sMsg.Split(',');
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SetPlayerInGame(splitstrs[0], splitstrs[1] == "1");
			if (StageUpdate.GetPlayerByID(splitstrs[0]) != null)
			{
				BattleInfoUI.Instance.RemoveOrangeCharacter(StageUpdate.GetPlayerByID(splitstrs[0]));
			}
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayingPlayerCount(true) == 0)
			{
				StageUpdate.gbGeneratePvePlayer = false;
			}
		}

		public void STAGE_SYNC_DMGERROR_ENDGAME(string sMsg)
		{
			tStageUpdate.IsEnd = true;
			ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
			tStageUpdate.tStageOpenCommonTask.OpenCommon(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DAMAGE_OVER_DETECT"), BattleInfoUI.Instance.StageOutGO);
		}

		public void STAGE_SYNC_BATTLEEND_RQ(string sMsg)
		{
			if (MonoBehaviourSingleton<StageSyncManager>.Instance.nLastSendBattleWinType != -1)
			{
				bool bIsSeason = false;
				STAGE_TABLE value;
				if (BattleInfoUI.Instance != null)
				{
					bIsSeason = BattleInfoUI.Instance.NowStageTable.n_MAIN == 90000 && BattleInfoUI.Instance.NowStageTable.n_SUB == 1;
				}
				else if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0 && ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value))
				{
					bIsSeason = value.n_MAIN == 90000 && value.n_SUB == 1;
				}
				MonoBehaviourSingleton<StageSyncManager>.Instance.HostSendBattleEndToOther(MonoBehaviourSingleton<StageSyncManager>.Instance.nLastSendBattleWinType, bIsSeason, sMsg);
			}
		}

		public void STAGE_START_SUMMON_ENEMY(string sMsg)
		{
		}

		private string GetNetEnemySyncString()
		{
			string text = "," + StageUpdate.runEnemys.Count;
			for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
			{
				if (((uint)StageUpdate.runEnemys[i].nEnemyBitParam & 0x10u) != 0 && StageUpdate.runEnemys[i].mEnemy != null)
				{
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.sNetSerialID;
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.EnemyID;
					text = text + "," + StageUpdate.runEnemys[i].nEnemyBitParam;
					text = text + "," + StageUpdate.runEnemys[i].nSCEID;
					text = text + "," + StageUpdate.runEnemys[i].fSetAimRange.ToString("0.000");
					text = text + "," + StageUpdate.runEnemys[i].fSetAimRangeY.ToString("0.000");
					text = text + "," + StageUpdate.runEnemys[i].fSetOffsetX.ToString("0.000");
					text = text + "," + StageUpdate.runEnemys[i].fSetOffsetY.ToString("0.000");
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.transform.position.x;
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.transform.position.y;
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.Hp;
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.HealHp;
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.DmgHp;
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.direction;
					text = text + "," + StageUpdate.runEnemys[i].mEnemy.PartHp.Length;
					for (int j = 0; j < StageUpdate.runEnemys[i].mEnemy.PartHp.Length; j++)
					{
						text = text + "," + StageUpdate.runEnemys[i].mEnemy.PartHp[j];
					}
				}
			}
			return text;
		}

		private string GetPlayerSyncString(string sPlayerID, bool bAll = true)
		{
			listSyncOCData.Clear();
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				if (StageUpdate.runPlayers[num].sPlayerID == sPlayerID || bAll)
				{
					SyncOCData syncOCData = new SyncOCData();
					syncOCData.tOcPos = StageUpdate.runPlayers[num].Controller.LogicPosition.vec3;
					syncOCData.sPlayerID = StageUpdate.runPlayers[num].sPlayerID;
					syncOCData.nLookDir = (int)StageUpdate.runPlayers[num]._characterDirection;
					syncOCData.nHP = StageUpdate.runPlayers[num].Hp;
					syncOCData.nRecord = ManagedSingleton<InputStorage>.Instance.GetInputRecordNO(StageUpdate.runPlayers[num].sPlayerID);
					syncOCData.nCharID = StageUpdate.runPlayers[num].CharacterID;
					syncOCData.WeaponCurrent = StageUpdate.runPlayers[num].WeaponCurrent;
					syncOCData.nMeasureNow = StageUpdate.runPlayers[num].selfBuffManager.nMeasureNow;
					syncOCData.bUsePassiveskill = StageUpdate.runPlayers[num].tRefPassiveskill.bUsePassiveskill;
					syncOCData.HealHp = StageUpdate.runPlayers[num].HealHp;
					syncOCData.DmgHp = StageUpdate.runPlayers[num].DmgHp;
					syncOCData.listSyncBuff = StageUpdate.runPlayers[num].selfBuffManager.listBuffs;
					listSyncOCData.Add(syncOCData);
				}
			}
			return JsonConvert.SerializeObject(listSyncOCData);
		}

		public void SendSyncMemberInfo(string sPlayerID)
		{
			string text = sPlayerID + "," + 2;
			foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
			{
				text = text + "," + item.PlayerId;
				text = text + "," + item.nKillNum;
				text = text + "," + item.nKillEnemyNum;
				text = text + "," + item.nLifePercent;
				text = text + "," + item.nScore;
				text = text + "," + item.nNowCharacterID;
				text = text + "," + item.nALLDMG;
			}
			StageUpdate.SyncStageObj(3, 7, text);
		}

		public static string MakeStageSyncSeasonData()
		{
			return string.Concat(string.Concat("," + 3, ",", JsonConvert.SerializeObject(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.idcSeasonCharaterInfoList)), "#", JsonConvert.SerializeObject(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.listCmdSeasonBattleInfoReq));
		}

		private IEnumerator WaitReMovePlayerCoroutine(string sPlayerID)
		{
			OrangeCharacter tOC = StageUpdate.GetPlayerByID(sPlayerID);
			if (tOC == null)
			{
				yield break;
			}
			while (tStageUpdate.nRunStageCtrlCount > 0)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				if (tOC.IsDead())
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, tOC.sNetSerialID, false, 0f, 0f, null);
				}
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_DESTROY_ED, tOC, true);
			tOC.SetActiveFalse();
		}

		private IEnumerator WaitAllPlayerOnAndSendReConnectData(string smsg)
		{
			string[] splitstrs = smsg.Split(',');
			if (!StageUpdate.bIsHost)
			{
				yield break;
			}
			if (tStageUpdate.IsEnd)
			{
				if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
				{
					SendSyncMemberInfo(splitstrs[0]);
					StageUpdate.SyncStageObj(3, 13, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + "," + (int)ManagedSingleton<StageHelper>.Instance.eLastStageResult, true, true);
				}
				yield break;
			}
			while (true)
			{
				bool flag = false;
				foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
				{
					bool flag2 = false;
					foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
					{
						if (item.PlayerId == runPlayer.sNetSerialID)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			float fSendWait = 0f;
			while (true)
			{
				if (!StageUpdate.bIsHost)
				{
					yield break;
				}
				if (tStageUpdate.nRunStageCtrlCount <= 0)
				{
					break;
				}
				fSendWait += Time.deltaTime;
				if (fSendWait >= 0.8f)
				{
					StageUpdate.SyncStageObj(3, 15, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true, true);
					fSendWait = 0f;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (!StageUpdate.bIsHost)
			{
				yield break;
			}
			if (StageUpdate.bWaitReconnect && StageUpdate.bIsHost)
			{
				Debug.LogError("送重連訊息後發現主機已經跑了");
				tStageUpdate.PauseCommonOut();
				yield break;
			}
			StageSLBase[] allTypeOfObjs = StageResManager.GetAllTypeOfObjs<StageSLBase>();
			Array.Sort(allTypeOfObjs, new SortStageSLBaseHelper());
			for (int i = 0; i < allTypeOfObjs.Length; i++)
			{
				allTypeOfObjs[i].SyncNowStatus();
			}
			string text = splitstrs[0] + "," + 5;
			text += GetNetEnemySyncString();
			StageUpdate.SyncStageObj(3, 7, text);
			for (int j = 0; j < StageUpdate.runPlayers.Count; j++)
			{
				if (StageUpdate.runPlayers[j].sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					Vector3 position = StageUpdate.runPlayers[j].transform.position;
					StageUpdate.SyncLockRangeByList(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, position.x, position.y, position.z);
					break;
				}
			}
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ReStageCountDownTime();
				BattleInfoUI.Instance.ReSendSyncFlagScore();
			}
			text = splitstrs[0] + "," + 6;
			text = text + "," + JsonConvert.SerializeObject(StageUpdate.GetPerGameSaveData());
			StageUpdate.SyncStageObj(3, 7, text);
			text = splitstrs[0] + "," + 0;
			text = text + "," + GetPlayerSyncString(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
			StageUpdate.SyncStageObj(3, 7, text);
			text = splitstrs[0] + "," + 1 + "," + tStageUpdate.fStageUseTime;
			StageUpdate.SyncStageObj(3, 7, text);
			SendSyncMemberInfo(splitstrs[0]);
			tPvpBarUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpBarUI>("UI_PvpBar");
			if (tPvpBarUI != null)
			{
				tPvpBarUI.SyncPlayerLife();
			}
			if (BattleInfoUI.Instance != null && BattleInfoUI.Instance.NowStageTable.n_MAIN == 90000 && BattleInfoUI.Instance.NowStageTable.n_SUB == 1)
			{
				text = splitstrs[0] + MakeStageSyncSeasonData();
				StageUpdate.SyncStageObj(3, 7, text);
			}
			text = splitstrs[0] + "," + 4;
			StageUpdate.SyncStageObj(3, 7, text);
		}
	}
}
