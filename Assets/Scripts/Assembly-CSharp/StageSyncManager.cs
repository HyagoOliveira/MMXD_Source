#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using OrangeApi;
using OrangeAudio;
using OrangeSocket;
using StageLib;
using UnityEngine;
using cb;
using enums;

public class StageSyncManager : MonoBehaviourSingleton<StageSyncManager>
{
	private enum SSM
	{
		NONE = 0,
		STRQSTART = 1,
		SYNCTIME = 2,
		RSYNCTIME = 3,
		CHANGEHOST = 4,
		CHECKCONNECT = 5,
		CHECKLOADEND = 6,
		RCHECKLOADEND = 7,
		CHECKREADY = 8,
		REQHOST = 9
	}

	private class AtkTimeMark
	{
		public float fLastAtkTime;

		public Transform tTrans;

		public int nSkillID;
	}

	public bool bAllCheckOK;

	public int nNeedWaitLoadCount = 999;

	public bool bLoadingStage = true;

	private const string NT_PREFIXES = "OGUD";

	private string NT_STRQSTART = "";

	private string NT_OGUDSYNCTIME = "";

	private string NT_ROGUDSYNCTIME = "";

	private string NT_OGUDCHANGEHOST = "";

	private string NT_OGUDCHECKCONNECT = "";

	private string NT_OGUDCHECKLOADEND = "";

	private string NT_ROGUDCHECKLOADEND = "";

	private string NT_OGUDCHECKREADY = "";

	private string NT_OGUDREQHOST = "";

	private string tmpMsg = "";

	private int nSyncID;

	private int nSyncIndex;

	private string[] splitstrs;

	private float fAvgDelayTime;

	private const float fNetWaitTimeOut = 5f;

	private const float fMaxWaitStageStart = 120f;

	[HideInInspector]
	private float fStagePauseStartTime;

	public bool bIgnoreReadyGo;

	private List<AtkTimeMark> AtkTimeMarks = new List<AtkTimeMark>();

	public int nLastSendBattleWinType = -1;

	public float HostAvgDelayTime
	{
		get
		{
			return fAvgDelayTime;
		}
	}

	public bool bPauseAllPlayerInput { get; set; }

	public void InitStageEvents()
	{
		NT_STRQSTART = "OGUD" + 1 + ",";
		NT_OGUDSYNCTIME = "OGUD" + 2 + ",";
		NT_ROGUDSYNCTIME = "OGUD" + 3 + ",";
		NT_OGUDCHANGEHOST = "OGUD" + 4 + ",";
		NT_OGUDCHECKCONNECT = "OGUD" + 5 + ",";
		NT_OGUDCHECKLOADEND = "OGUD" + 6 + ",";
		NT_ROGUDCHECKLOADEND = "OGUD" + 7 + ",";
		NT_OGUDCHECKREADY = "OGUD" + 8 + ",";
		NT_OGUDREQHOST = "OGUD" + 9 + ",";
		Singleton<GenericEventManager>.Instance.AttachEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, ObjCtrl);
		Singleton<GenericEventManager>.Instance.AttachEvent<bool, bool>(EventManager.ID.STAGE_RESTART, StageRqStart);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTBroadcastToRoom, OnNTSyncStageObj);
		Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageSkillAtkTargetParam>(EventManager.ID.STAGE_SKILL_ATK_TARGET, SkillAttackTarget);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Singleton<GenericEventManager>.Instance.DetachEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, ObjCtrl);
		Singleton<GenericEventManager>.Instance.DetachEvent<bool, bool>(EventManager.ID.STAGE_RESTART, StageRqStart);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTBroadcastToRoom, OnNTSyncStageObj);
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageSkillAtkTargetParam>(EventManager.ID.STAGE_SKILL_ATK_TARGET, SkillAttackTarget);
	}

	private void OnApplicationPause(bool pause)
	{
		if (StageUpdate.nReConnectMode == 0)
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if ((stageUpdate != null && stageUpdate.IsEnd) || stageUpdate == null)
			{
				return;
			}
			if (StageUpdate.gbGeneratePvePlayer)
			{
				StageUpdate.ReqChangeHost();
				StageUpdate.SyncStageObj(3, 18, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + ",0", true);
				StageUpdate.bIsHost = true;
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
				StageUpdate.gbGeneratePvePlayer = false;
				foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
				{
					if (item.PlayerId != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
					{
						if (StageUpdate.GetPlayerByID(item.PlayerId) != null)
						{
							BattleInfoUI.Instance.RemoveOrangeCharacter(StageUpdate.GetPlayerByID(item.PlayerId));
						}
						item.bInGame = false;
					}
				}
				MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
				OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
				if (mainPlayerOC != null)
				{
					mainPlayerOC.PlayerReleaseLeftCB();
					mainPlayerOC.PlayerReleaseRightCB();
					mainPlayerOC.PlayerReleaseDownCB();
				}
			}
			else if (StageUpdate.gbRegisterPvpPlayer)
			{
				stageUpdate.PauseCommonOut();
			}
		}
		else
		{
			if (StageUpdate.nReConnectMode != 1)
			{
				return;
			}
			if (StageUpdate.gbIsNetGame)
			{
				if (pause)
				{
					StageUpdate.ReqChangeHost();
					MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 1);
					fStagePauseStartTime = Time.realtimeSinceStartup;
					if (!StageUpdate.bIsHost && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count > 0)
					{
						StageUpdate.bWaitReconnect = true;
					}
					return;
				}
				StageUpdate stageUpdate2 = StageResManager.GetStageUpdate();
				if (stageUpdate2 == null)
				{
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count > 0)
					{
						StageUpdate.bWaitReconnect = true;
					}
					return;
				}
				StageUpdate.BackAllBulletByDisconnect();
				if (!stageUpdate2.IsEnd)
				{
					MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
					OrangeCharacter mainPlayerOC2 = StageUpdate.GetMainPlayerOC();
					if (mainPlayerOC2 != null)
					{
						mainPlayerOC2.PlayerReleaseLeftCB();
						mainPlayerOC2.PlayerReleaseRightCB();
						mainPlayerOC2.PlayerReleaseDownCB();
						mainPlayerOC2.SetLockBulletForNextStatus();
					}
					if (StageUpdate.gbStageReady)
					{
						stageUpdate2.fStageUseTime += Time.realtimeSinceStartup - fStagePauseStartTime;
					}
					stageUpdate2.SendReConnectMsg();
				}
			}
			else
			{
				if (pause)
				{
					return;
				}
				StageUpdate stageUpdate3 = StageResManager.GetStageUpdate();
				if (!(stageUpdate3 == null) && !stageUpdate3.IsEnd && !MonoBehaviourSingleton<UIManager>.Instance.GetUI<DialogUI>("UI_Dialog"))
				{
					MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
					OrangeCharacter mainPlayerOC3 = StageUpdate.GetMainPlayerOC();
					if (mainPlayerOC3 != null)
					{
						mainPlayerOC3.PlayerReleaseLeftCB();
						mainPlayerOC3.PlayerReleaseRightCB();
						mainPlayerOC3.PlayerReleaseDownCB();
					}
					if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading && BattleInfoUI.Instance != null)
					{
						BattleInfoUI.Instance.OnClickOption();
					}
				}
			}
		}
	}

	private void ObjCtrl(GameObject tObj, StageCtrlInsTruction tSCE)
	{
		switch (tSCE.tStageCtrl)
		{
		case 18:
		{
			ManagedSingleton<StageHelper>.Instance.bEnemyActive = false;
			for (int num2 = StageUpdate.runBulletSets.Count - 1; num2 >= 0; num2--)
			{
				StageUpdate.runBulletSets[num2].PauseUseSE(true);
			}
			break;
		}
		case 19:
			ManagedSingleton<StageHelper>.Instance.bEnemyActive = true;
			if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				for (int num = StageUpdate.runBulletSets.Count - 1; num >= 0; num--)
				{
					StageUpdate.runBulletSets[num].PauseUseSE(false);
				}
			}
			break;
		}
	}

	public void OnNTSyncStageObj(object obj)
	{
		NTBroadcastToRoom nTBroadcastToRoom = (NTBroadcastToRoom)obj;
		if (!nTBroadcastToRoom.Action.StartsWith("OGUD"))
		{
			return;
		}
		tmpMsg = nTBroadcastToRoom.Action.Substring("OGUD".Length);
		nSyncIndex = tmpMsg.IndexOf(',');
		nSyncID = int.Parse(tmpMsg.Substring(0, nSyncIndex));
		tmpMsg = tmpMsg.Substring(nSyncIndex + 1);
		switch (nSyncID)
		{
		case 1:
			if (!StageUpdate.gbStageReady && !bLoadingStage)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_ALLOK);
			}
			break;
		case 2:
			splitstrs = tmpMsg.Split(',');
			tmpMsg = NT_ROGUDSYNCTIME + splitstrs[0];
			fAvgDelayTime = float.Parse(splitstrs[1]);
			MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(tmpMsg));
			break;
		case 3:
			if (StageUpdate.bIsHost)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				float num = float.Parse(tmpMsg);
				realtimeSinceStartup -= num;
				realtimeSinceStartup *= 0.5f;
				if (fAvgDelayTime == 0f)
				{
					fAvgDelayTime += realtimeSinceStartup;
					break;
				}
				fAvgDelayTime += realtimeSinceStartup;
				fAvgDelayTime *= 0.5f;
			}
			break;
		case 4:
			splitstrs = tmpMsg.Split(',');
			if (splitstrs[1] == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				StageUpdate.bIsHost = true;
			}
			else
			{
				StageUpdate.bIsHost = false;
			}
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID = splitstrs[1];
			break;
		case 5:
			splitstrs = tmpMsg.Split(',');
			switch (int.Parse(splitstrs[1]))
			{
			case 0:
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ResetCheckConnectTime(splitstrs[0]);
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckPlayerPause(splitstrs[0]))
				{
					MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(splitstrs[0], 3);
				}
				break;
			case 1:
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SetPlayerPause(splitstrs[0], true);
				break;
			case 2:
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SetPlayerPause(splitstrs[0], false);
				break;
			case 3:
				if (splitstrs[0] == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 2);
				}
				break;
			}
			break;
		case 6:
			if (bLoadingStage)
			{
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_ROGUDCHECKLOADEND + MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + ",0"));
			}
			else
			{
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_ROGUDCHECKLOADEND + MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + ",1"));
			}
			break;
		case 7:
			splitstrs = tmpMsg.Split(',');
			if (splitstrs[1] == "0")
			{
				bAllCheckOK = false;
			}
			break;
		case 8:
			if (StageUpdate.bIsHost && StageUpdate.gbStageReady)
			{
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_STRQSTART + MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify));
			}
			break;
		case 9:
			if (!StageUpdate.bWaitReconnect)
			{
				splitstrs = tmpMsg.Split(',');
				if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == splitstrs[0])
				{
					SendAndChangeHostSelf();
				}
			}
			break;
		}
	}

	public void SendChangeHost(string sPlayerIDHostID)
	{
		MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_OGUDCHANGEHOST + sPlayerIDHostID));
	}

	public void SendPlayerCheckConnect(string sPlayerID, int nPauseMode)
	{
		MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_OGUDCHECKCONNECT + sPlayerID + "," + nPauseMode));
	}

	public void SendPlayerReqHostList(List<string> listPlayerIDs, string oldHost)
	{
		StartCoroutine(SendPlayerReqHostListCoroutine(listPlayerIDs, oldHost));
	}

	private void SendAndChangeHostSelf()
	{
		MonoBehaviourSingleton<StageSyncManager>.Instance.SendChangeHost("," + MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_HOST, true);
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
	}

	private IEnumerator SendPlayerReqHostListCoroutine(List<string> listPlayerIDs, string oldHost)
	{
		while (oldHost == MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID && listPlayerIDs.Count > 0)
		{
			string sTargetPlayerID = listPlayerIDs[0];
			listPlayerIDs.RemoveAt(0);
			Debug.LogError("像順位1玩家發送轉HOST要求 並移除");
			if (sTargetPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				Debug.LogError("自己是主機 發送訊息");
				SendAndChangeHostSelf();
				continue;
			}
			float fTotalTime = 0f;
			while (oldHost == MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID)
			{
				Debug.LogError("發送要求訊息 " + sTargetPlayerID + " " + MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerName(sTargetPlayerID));
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_OGUDREQHOST + sTargetPlayerID));
				yield return CoroutineDefine._1sec;
				fTotalTime += 1f;
				if (fTotalTime >= 5f)
				{
					break;
				}
			}
		}
	}

	public void LoadPlayerEnd(string sPlayerID)
	{
		int num = 0;
		foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
		{
			if (item.PlayerId == sPlayerID)
			{
				item.bLoadEnd = true;
			}
			if (!item.bLoadEnd)
			{
				num++;
			}
		}
		if (num <= 0)
		{
			if (!bIgnoreReadyGo)
			{
				StartCoroutine(ReadyGoCoroutine(NotifyPLAYERBUILD_PLAYER_SPAWN));
			}
			else
			{
				NotifyPLAYERBUILD_PLAYER_SPAWN();
			}
		}
	}

	private void NotifyPLAYERBUILD_PLAYER_SPAWN()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.PLAYERBUILD_PLAYER_SPAWN);
		bIgnoreReadyGo = true;
	}

	public void ShowReadyGo(Action cb)
	{
		StartCoroutine(ReadyGoCoroutine(cb));
	}

	private IEnumerator ReadyGoCoroutine(Action cb)
	{
		while (MonoBehaviourSingleton<UIManager>.Instance.IsLoading)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		UI_Ready uiReady = null;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Ready", delegate(UI_Ready ui)
		{
			uiReady = ui;
			uiReady.Play();
		});
		while (!uiReady)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		while (!uiReady.Complete)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (cb != null)
		{
			cb();
		}
	}

	private void StageRqStart(bool bNetGame, bool bIsHost)
	{
		bLoadingStage = false;
		if (bNetGame)
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect)
			{
				StartStageUpdateCoroutine(SyncInitHostTime());
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_ALLOK);
			}
			else
			{
				StartStageUpdateCoroutine(SyncInitHostTime());
			}
		}
		else
		{
			fAvgDelayTime = 0f;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_ALLOK);
		}
	}

	private void SkillAttackTarget(EventManager.StageSkillAtkTargetParam tStageSkillAtkTargetParam)
	{
		OrangeCharacter orangeCharacter = null;
		EnemyControllerBase enemyControllerBase = null;
		RideArmorController rideArmorController = null;
		if (tStageSkillAtkTargetParam.tTrans != null)
		{
			orangeCharacter = tStageSkillAtkTargetParam.tTrans.GetComponent<OrangeCharacter>();
			enemyControllerBase = tStageSkillAtkTargetParam.tTrans.GetComponent<EnemyControllerBase>();
			if (orangeCharacter == null)
			{
				rideArmorController = tStageSkillAtkTargetParam.tTrans.root.GetComponent<RideArmorController>();
			}
		}
		bool flag = false;
		SKILL_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(tStageSkillAtkTargetParam.nSkillID, out value))
		{
			return;
		}
		if (tStageSkillAtkTargetParam.bAtkNoCast)
		{
			if (CheckAtkTime(value, tStageSkillAtkTargetParam.tTrans))
			{
				return;
			}
			if (orangeCharacter != null)
			{
				flag = true;
				if (StageUpdate.gbIsNetGame)
				{
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.HasPlayer(orangeCharacter.sPlayerID))
					{
						if (orangeCharacter.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
						{
							flag = false;
						}
					}
					else if (!StageUpdate.bIsHost)
					{
						flag = false;
					}
				}
				if (flag)
				{
					RefPassiveskill.TriggerSkill(value, 1, 4095, null, orangeCharacter.selfBuffManager, 0);
					if (value.n_CONDITION_ID != 0 && value.n_CONDITION_RATE >= UnityEngine.Random.Range(0, 10000))
					{
						if (orangeCharacter.IsNearPlayer())
						{
							orangeCharacter.selfBuffManager.AddBuff(value.n_CONDITION_ID, 0, 0, value.n_ID, false, orangeCharacter.sNetSerialID, 4);
						}
						else
						{
							orangeCharacter.selfBuffManager.AddBuff(value.n_CONDITION_ID, 0, 0, value.n_ID, false, orangeCharacter.sNetSerialID);
						}
					}
				}
				AtkTimeMark atkTimeMark = new AtkTimeMark();
				atkTimeMark.nSkillID = tStageSkillAtkTargetParam.nSkillID;
				atkTimeMark.fLastAtkTime = Time.realtimeSinceStartup;
				atkTimeMark.tTrans = tStageSkillAtkTargetParam.tTrans;
				AtkTimeMarks.Add(atkTimeMark);
			}
			else if (rideArmorController != null)
			{
				flag = true;
				if (StageUpdate.gbIsNetGame)
				{
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.HasPlayer(rideArmorController.MasterPilot.sPlayerID))
					{
						if (rideArmorController.MasterPilot.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
						{
							flag = false;
						}
					}
					else if (!StageUpdate.bIsHost)
					{
						flag = false;
					}
				}
				if (flag)
				{
					RefPassiveskill.TriggerSkill(value, 1, 4095, null, rideArmorController.selfBuffManager, 0);
					if (value.n_CONDITION_ID != 0 && value.n_CONDITION_RATE >= UnityEngine.Random.Range(0, 10000))
					{
						if (rideArmorController.MasterPilot.IsNearPlayer())
						{
							rideArmorController.selfBuffManager.AddBuff(value.n_CONDITION_ID, 0, 0, value.n_ID, false, rideArmorController.sNetSerialID, 4);
						}
						else
						{
							rideArmorController.selfBuffManager.AddBuff(value.n_CONDITION_ID, 0, 0, value.n_ID, false, rideArmorController.sNetSerialID);
						}
					}
				}
				AtkTimeMark atkTimeMark2 = new AtkTimeMark();
				atkTimeMark2.nSkillID = tStageSkillAtkTargetParam.nSkillID;
				atkTimeMark2.fLastAtkTime = Time.realtimeSinceStartup;
				atkTimeMark2.tTrans = tStageSkillAtkTargetParam.tTrans;
				AtkTimeMarks.Add(atkTimeMark2);
			}
			else
			{
				if (!(enemyControllerBase != null))
				{
					return;
				}
				flag = true;
				if ((value.n_EFFECT == 12 || value.n_EFFECT == 13) && value.f_EFFECT_X >= 100f)
				{
					flag = true;
				}
				else if (!StageUpdate.bIsHost)
				{
					flag = false;
				}
				if (flag)
				{
					RefPassiveskill.TriggerSkill(value, 1, 4095, null, enemyControllerBase.selfBuffManager, 0);
					if (value.n_CONDITION_ID != 0 && value.n_CONDITION_RATE >= UnityEngine.Random.Range(0, 10000))
					{
						enemyControllerBase.selfBuffManager.AddBuff(value.n_CONDITION_ID, 0, 0, value.n_ID);
					}
				}
				AtkTimeMark atkTimeMark3 = new AtkTimeMark();
				atkTimeMark3.nSkillID = tStageSkillAtkTargetParam.nSkillID;
				atkTimeMark3.fLastAtkTime = Time.realtimeSinceStartup;
				atkTimeMark3.tTrans = tStageSkillAtkTargetParam.tTrans;
				AtkTimeMarks.Add(atkTimeMark3);
			}
		}
		else
		{
			if (CheckAtkTime(value, tStageSkillAtkTargetParam.tTrans))
			{
				return;
			}
			AtkTimeMark atkTimeMark4 = new AtkTimeMark();
			atkTimeMark4.nSkillID = tStageSkillAtkTargetParam.nSkillID;
			atkTimeMark4.fLastAtkTime = Time.realtimeSinceStartup;
			atkTimeMark4.tTrans = tStageSkillAtkTargetParam.tTrans;
			AtkTimeMarks.Add(atkTimeMark4);
			BulletBase bulletBase = null;
			if (value.s_MODEL != "DUMMY")
			{
				bulletBase = (((short)value.n_TYPE != 9) ? ((BulletBase)MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>(value.s_MODEL)) : ((BulletBase)MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<LrColliderBullet>(value.s_MODEL)));
			}
			if (bulletBase == null)
			{
				bulletBase = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>("PoolColliderBullet");
			}
			if ((bool)bulletBase)
			{
				bulletBase.UpdateBulletData(value);
				WeaponStatus weaponStatus = new WeaponStatus();
				bulletBase.SetBulletAtk(weaponStatus, new PerBuffManager.BuffStatus());
				bulletBase.isForceSE = true;
				if (tStageSkillAtkTargetParam.tTrans != null)
				{
					bulletBase.Active(tStageSkillAtkTargetParam.tTrans, tStageSkillAtkTargetParam.tDir, tStageSkillAtkTargetParam.tLM);
				}
				else
				{
					bulletBase.Active(tStageSkillAtkTargetParam.tPos, tStageSkillAtkTargetParam.tDir, tStageSkillAtkTargetParam.tLM);
				}
			}
		}
	}

	public bool CheckAtkTime(SKILL_TABLE tSKILL_TABLE, Transform tTransform)
	{
		for (int i = 0; i < AtkTimeMarks.Count; i++)
		{
			if (AtkTimeMarks[i].nSkillID == tSKILL_TABLE.n_ID && AtkTimeMarks[i].tTrans == tTransform)
			{
				if ((Time.realtimeSinceStartup - AtkTimeMarks[i].fLastAtkTime) * 1000f < (float)tSKILL_TABLE.n_RELOAD)
				{
					return true;
				}
				AtkTimeMarks.RemoveAt(i);
				break;
			}
		}
		return false;
	}

	private IEnumerator SyncInitHostTime()
	{
		float fMaxWaitTime = 0f;
		while (!StageUpdate.gbStageReady)
		{
			if (StageUpdate.bIsHost)
			{
				bAllCheckOK = true;
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_OGUDCHECKLOADEND + MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify));
				float fTimeLimitCheck = 0f;
				while (fTimeLimitCheck < 5f)
				{
					fTimeLimitCheck += Time.deltaTime;
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				if (StageUpdate.bIsHost && bAllCheckOK)
				{
					break;
				}
			}
			else
			{
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_OGUDCHECKREADY + MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify));
			}
			if (nNeedWaitLoadCount == 1 && !StageUpdate.bIsHost)
			{
				StageUpdate.bIsHost = true;
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
			fMaxWaitTime += Time.deltaTime;
			if (fMaxWaitTime > 120f)
			{
				break;
			}
		}
		if (StageUpdate.bIsHost && !StageUpdate.gbStageReady)
		{
			MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(NT_STRQSTART + MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify));
		}
		if (!StageUpdate.gbStageReady)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_ALLOK);
		}
		if (nNeedWaitLoadCount == 1 && !StageUpdate.bIsHost)
		{
			StageUpdate.bIsHost = true;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		}
		fAvgDelayTime = 0f;
		StartStageUpdateCoroutine(SyncHostTime());
	}

	private void StartStageUpdateCoroutine(IEnumerator tCoroutine)
	{
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if (stageUpdate != null)
		{
			stageUpdate.StartCoroutine(tCoroutine);
		}
	}

	private IEnumerator SyncHostTime()
	{
		while (true)
		{
			if (StageUpdate.bIsHost)
			{
				string action = NT_OGUDSYNCTIME + Time.realtimeSinceStartup + "," + fAvgDelayTime;
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(action));
				yield return CoroutineDefine._1sec;
				yield return CoroutineDefine._1sec;
			}
			else
			{
				yield return CoroutineDefine._1sec;
				yield return CoroutineDefine._1sec;
			}
		}
	}

	public void WaitAvgPetDelayRun(string sNetSyncID, int nSetKey, string sOther)
	{
		StartCoroutine(AvgDelayPetRunCall(sNetSyncID, nSetKey, sOther));
	}

	private IEnumerator AvgDelayPetRunCall(string sNetSyncID, int nSetKey, string sOther)
	{
		yield return new WaitForSecondsRealtime(fAvgDelayTime);
		StageCtrlInsTruction stageCtrlInsTruction = new StageCtrlInsTruction();
		stageCtrlInsTruction.tStageCtrl = 76;
		Singleton<GenericEventManager>.Instance.NotifyEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, null, stageCtrlInsTruction);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL_PET_ACTION, sNetSyncID, nSetKey, sOther);
	}

	public void WaitAvgBulletDelayRun(string sNetSyncID, int nSetKey, string sOther)
	{
		StartCoroutine(AvgDelayBulletRunCall(sNetSyncID, nSetKey, sOther));
	}

	private IEnumerator AvgDelayBulletRunCall(string sNetSyncID, int nSetKey, string sOther)
	{
		yield return new WaitForSecondsRealtime(fAvgDelayTime);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL_BULLET_ACTION, sNetSyncID, nSetKey, sOther);
	}

	public void WaitAvgDelayRun(string sNetSyncID, int nSetKey, string sOther)
	{
		StartCoroutine(AvgDelayRunCall(sNetSyncID, nSetKey, sOther));
	}

	private IEnumerator AvgDelayRunCall(string sNetSyncID, int nSetKey, string sOther)
	{
		yield return new WaitForSecondsRealtime(fAvgDelayTime);
		StageCtrlInsTruction stageCtrlInsTruction = new StageCtrlInsTruction();
		stageCtrlInsTruction.tStageCtrl = 70;
		Singleton<GenericEventManager>.Instance.NotifyEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, null, stageCtrlInsTruction);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL_ENEMY_ACTION, sNetSyncID, nSetKey, sOther);
	}

	public IEnumerator UnSlowStageCoroutine(float fWaitTime)
	{
		while (fWaitTime > 0f)
		{
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
			fWaitTime -= Time.deltaTime;
		}
		StageUpdate.UnSlowStage();
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if (!(stageUpdate == null))
		{
			stageUpdate.tUnSlowStageCoroutine = null;
		}
	}

	public int GetFSSkillID(int nFSID)
	{
		FinalStrikeInfo value;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(nFSID, out value))
		{
			FS_TABLE fS_TABLE = null;
			Dictionary<int, FS_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.n_FS_ID == value.netFinalStrikeInfo.FinalStrikeID && enumerator.Current.Value.n_LV == value.netFinalStrikeInfo.Level)
				{
					fS_TABLE = enumerator.Current.Value;
					break;
				}
			}
			if (fS_TABLE != null)
			{
				return (new int[7] { fS_TABLE.n_SKILL_0, fS_TABLE.n_SKILL_1, fS_TABLE.n_SKILL_2, fS_TABLE.n_SKILL_3, fS_TABLE.n_SKILL_4, fS_TABLE.n_SKILL_5, fS_TABLE.n_SKILL_6 })[value.netFinalStrikeInfo.Star];
			}
		}
		return 0;
	}

	public void RequestGetBattleEndMsg()
	{
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if (stageUpdate != null)
		{
			stageUpdate.StartCoroutine(WaitSendRequestGetBattleEndMsg());
		}
	}

	private IEnumerator WaitSendRequestGetBattleEndMsg()
	{
		StageUpdate tStageUpdate = StageResManager.GetStageUpdate();
		float fTimeWait = 3f;
		bool bIsNeedEnd = false;
		while (!tStageUpdate.IsEnd)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			fTimeWait -= Time.deltaTime;
			if (StageUpdate.bIsHost)
			{
				bIsNeedEnd = true;
				break;
			}
			if (fTimeWait <= 0f)
			{
				StageUpdate.SyncStageObj(3, 20, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
				fTimeWait = 3f;
			}
		}
		if (bIsNeedEnd)
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
			MonoBehaviourSingleton<StageSyncManager>.Instance.HostSendBattleEndToOther(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nWinType, bIsSeason);
			MonoBehaviourSingleton<StageSyncManager>.Instance.SendStageEndAndOpenPvpEndUI(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nWinType, bIsSeason);
		}
	}

	public void HostSendBattleEndToOther(int nWinType, bool bIsSeason, string sPlayerID = "")
	{
		nLastSendBattleWinType = nWinType;
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		string text = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify) + "," + nWinType;
		text = ((!bIsSeason) ? (text + ",0") : (text + ",1"));
		string text2 = "";
		if (bIsSeason)
		{
			text2 = StageSyncEventClass.MakeStageSyncSeasonData();
		}
		if (sPlayerID == "")
		{
			foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
			{
				if (item.PlayerId != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					stageUpdate.tStageSyncEventClass.SendSyncMemberInfo(item.PlayerId);
					if (text2 != "")
					{
						StageUpdate.SyncStageObj(3, 7, item.PlayerId + text2);
					}
				}
			}
		}
		else
		{
			stageUpdate.tStageSyncEventClass.SendSyncMemberInfo(sPlayerID);
			if (text2 != "")
			{
				StageUpdate.SyncStageObj(3, 7, sPlayerID + text2);
			}
		}
		StageUpdate.SyncStageObj(3, 14, text);
	}

	public void SendStageEndAndOpenPvpEndUI(int nWinType, bool bIsSeason = false, Action tCB = null)
	{
		if (StageResManager.GetStageUpdate().IsEnd)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		MonoBehaviourSingleton<AudioManager>.Instance.ClearBattleLoopList();
		StageEndReq tStageEndReq = new StageEndReq();
		tStageEndReq.StageID = ManagedSingleton<StageHelper>.Instance.nLastStageID;
		tStageEndReq.Star = 0;
		tStageEndReq.PVPMatchType = (sbyte)MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType;
		switch (nWinType)
		{
		case 0:
			tStageEndReq.Result = 2;
			break;
		case 1:
			tStageEndReq.Result = 1;
			break;
		case 2:
			tStageEndReq.Result = 3;
			break;
		default:
			tStageEndReq.Result = 2;
			break;
		}
		if (bIsSeason)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nWinType = nWinType;
			if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckSeasonKillEnemyNum())
			{
				nWinType = 2;
				tStageEndReq.Result = 3;
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nWinType = nWinType;
			}
		}
		if (StageUpdate.GetMainPlayerOC() == null && !bIsSeason)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (!(stageUpdate == null))
			{
				stageUpdate.IsEnd = true;
				ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), BattleInfoUI.Instance.StageOutGO);
				});
			}
			return;
		}
		tStageEndReq.Power = ManagedSingleton<StageHelper>.Instance.nLastOCPower;
		tStageEndReq.Score = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower();
		tStageEndReq.KillCount = (short)MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerKillEnemyNum();
		StageResManager.GetStageUpdate().IsEnd = true;
		if (bIsSeason)
		{
			SeasonBattleInfoV3Req seasonBattleInfoV3Req = new SeasonBattleInfoV3Req();
			for (int i = 0; i < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.listCmdSeasonBattleInfoReq.Count; i++)
			{
				SeasonBattleInfoReq seasonBattleInfoReq = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.listCmdSeasonBattleInfoReq[i];
				seasonBattleInfoV3Req.PlayerIDList.Add(seasonBattleInfoReq.PlayerID);
				seasonBattleInfoV3Req.CharacterIdList.Add(seasonBattleInfoReq.CharacterId);
				seasonBattleInfoV3Req.HP = seasonBattleInfoReq.HP;
				seasonBattleInfoV3Req.KillCount = seasonBattleInfoReq.KillCount;
				seasonBattleInfoV3Req.BeKilledCount = seasonBattleInfoReq.BeKilledCount;
				seasonBattleInfoV3Req.ResolutionW = seasonBattleInfoReq.ResolutionW;
				seasonBattleInfoV3Req.ResolutionH = seasonBattleInfoReq.ResolutionH;
			}
			MonoBehaviourSingleton<GameServerService>.Instance.SendRequest(seasonBattleInfoV3Req, delegate(SeasonBattleInfoV3Res rs)
			{
				if (rs.Code == 27150)
				{
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonExpired = true;
				}
				StageEndReq(tStageEndReq, delegate
				{
					int Result = ((nWinType == 1) ? 1 : 0);
					ManagedSingleton<PlayerNetManager>.Instance.SeasonBattleEndReq(Result, delegate
					{
						Result = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nWinType;
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpEnd", delegate(PvpEndUI ui)
						{
							ui.Setup(Result);
							if (tCB != null)
							{
								tCB();
							}
						});
						if (Result == 1)
						{
							MonoBehaviourSingleton<SteamManager>.Instance.AchievedAchievement(SteamAchievement.ST_ACHIEVEMENT_18);
						}
					});
				});
				if (rs.SeasonInfo != null)
				{
					ManagedSingleton<PlayerHelper>.Instance.UpdateMatchHunterRankTable(rs.SeasonInfo.Score);
				}
			});
		}
		else
		{
			StageEndReq(tStageEndReq, delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpEnd", delegate(PvpEndUI ui)
				{
					ui.Setup(nWinType);
					if (tCB != null)
					{
						tCB();
					}
				});
			});
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.listCmdSeasonBattleInfoReq.Clear();
	}

	private void StageEndReq(StageEndReq req, Callback cb)
	{
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsStageVaild(req.StageID))
		{
			MonoBehaviourSingleton<ACTkManager>.Instance.SetDetected();
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.StageEndReq(req, delegate(StageEndRes obj)
		{
			if (obj != null && obj.Code == 11153)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.SetupConfirmByKey("COMMON_TIP", "RESOLUTION_ABNORMAL_DETECT", "COMMON_OK", delegate
					{
						OrangeDataReader.Instance.DeleteTableAll();
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
						MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
						{
							MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
						});
					});
				}, true);
			}
			else
			{
				cb.CheckTargetToInvoke();
			}
		});
	}
}
