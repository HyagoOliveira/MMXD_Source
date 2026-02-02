using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageCtrlEvent : EventPointBase
	{
		[Serializable]
		public class StageCtrlDataSL
		{
			[JsonConverter(typeof(FloatConverter))]
			public float B2DX;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DY;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DW = 1f;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DH = 1f;

			public StageCtrlInsTruction[] StageCtrlInsTructions;

			public int nSetID;

			public bool bUB2D = true;

			public bool bDDF;

			public int nDF;

			public bool bNoUnlockRange;
		}

		private delegate bool StageCtrlEventFunc(StageCtrlInsTruction tStageCtrlInsTruction);

		private enum NET_SYNC_STEP
		{
			WAITSTART = 0,
			WAITSTARTRES = 1,
			RUNSCE = 2,
			STEPMSG = 3,
			WAITEND = 4,
			WAITENDRES = 5,
			NET_CONTINUE_PLAY = 6,
			CALLONBYID = 7,
			RECONNECTMSG = 8,
			SETPLAYERSTART = 9,
			SETPLAYERPOS = 10
		}

		public bool bNoUnlockRange;

		public List<StageCtrlInsTruction> StageCtrlInsTructions = new List<StageCtrlInsTruction>();

		private bool bStartEvent;

		private float fLeftTime;

		private GameObject tObj;

		private bool bLockWait;

		private bool bNeedResumTimer;

		private bool bSendSuccess;

		private const float fConstCheckTime = 10f;

		private int nStartIndex;

		private bool bHasLockPlayerInput;

		private const float fTimeOutLimit = 10f;

		private Coroutine tNowCheckCoroutine;

		private const float fTeleportWaitTimemultiPlay = 3f;

		private const float fTeleportWaitTime = 0f;

		private StageCtrlEventFunc[] StageCtrlEventFuncs;

		private CommonUI tPauseCommonUI;

		private string sTriggerPlayerID = "";

		private List<OrangeCharacter> listLockPlayer = new List<OrangeCharacter>();

		private Vector3 vMax;

		private Vector3 vMin;

		private Vector3 tLastPos;

		private Vector3 tNowPos;

		private int BEAT_SYNC_CHANNEL = 999;

		public override void Init()
		{
			StageCtrlEventFuncs = new StageCtrlEventFunc[77];
			for (int i = 0; i < StageCtrlEventFuncs.Length; i++)
			{
				StageCtrlEventFuncs[i] = null;
			}
			StageCtrlEventFuncs[1] = CTRL_LOCK_INPUT;
			StageCtrlEventFuncs[2] = CTRL_UNLOCK_INPUT;
			StageCtrlEventFuncs[6] = CTRL_MOVE_ATKBTN;
			StageCtrlEventFuncs[14] = CTRL_EVENT_CALL;
			StageCtrlEventFuncs[15] = CTRL_SHOW_WARING;
			StageCtrlEventFuncs[16] = CTRL_SHOW_MSG;
			StageCtrlEventFuncs[17] = CTRL_SHOW_ENEMYINFO;
			StageCtrlEventFuncs[20] = CTRL_MOVE_RIGHTPOS;
			StageCtrlEventFuncs[22] = CTRL_MOVE_CAMERA;
			StageCtrlEventFuncs[23] = CTRL_CLEAR_MOB;
			StageCtrlEventFuncs[24] = CTRL_MOB_PLAYSHOWANI;
			StageCtrlEventFuncs[25] = CTRL_MOB_ADDBOSSHPBAR;
			StageCtrlEventFuncs[26] = CTRL_SHOW_READYGO;
			StageCtrlEventFuncs[27] = CTRL_CAMERA_FOLLOW;
			StageCtrlEventFuncs[28] = CTRL_CAMERA_NOFOLLOW;
			StageCtrlEventFuncs[29] = CTRL_CAMERA_MOVEFOLLOW;
			StageCtrlEventFuncs[30] = CTRL_PLAYER_TELEPORT;
			StageCtrlEventFuncs[33] = CTRL_RUN_TUTO;
			StageCtrlEventFuncs[31] = CTRL_SHOW_ALWAYSHINTMESSAGE;
			StageCtrlEventFuncs[32] = CTRL_SHOW_TIMEHINTMESSAGE;
			StageCtrlEventFuncs[34] = CTRL_CLOSE_PLAYERACTIVE;
			StageCtrlEventFuncs[35] = CTRL_OPEN_PLAYERACTIVE;
			StageCtrlEventFuncs[36] = CTRL_SHOW_DRAGONBONE;
			StageCtrlEventFuncs[37] = CTRL_CLOSE_DRAGONBONE;
			StageCtrlEventFuncs[38] = CTRL_WAIT_PLAYERACT;
			StageCtrlEventFuncs[39] = CTRL_WAIT_BULLETIN;
			StageCtrlEventFuncs[40] = CTRL_SET_OBJ_ACT;
			StageCtrlEventFuncs[42] = CTRL_SET_OBJ_ANIMATOR;
			StageCtrlEventFuncs[43] = CTRL_SET_DEAD_CALLEVENT;
			StageCtrlEventFuncs[44] = CTRL_OBJ_TO_OBJ;
			StageCtrlEventFuncs[45] = CTRL_CREATE_NPCPLAYER_TO_OBJ;
			StageCtrlEventFuncs[46] = CTRL_MOVE_OBJ_TO_OBJ;
			StageCtrlEventFuncs[47] = CTRL_SHOT_BULLET;
			StageCtrlEventFuncs[48] = CTRL_SET_OBJ_DIR;
			StageCtrlEventFuncs[49] = CTRL_SET_MAINPLAYER_SCEID;
			StageCtrlEventFuncs[50] = CTRL_CALL_STAGE_END;
			StageCtrlEventFuncs[51] = CTRL_SET_OBJ_LAYER;
			StageCtrlEventFuncs[52] = CTRL_PLAY_FX;
			StageCtrlEventFuncs[53] = CTRL_SWITCH_SHOWPAD;
			StageCtrlEventFuncs[54] = CTRL_SWITCH_STAGETIME;
			StageCtrlEventFuncs[55] = CTRL_START_TIMER;
			StageCtrlEventFuncs[56] = CTRL_SHOW_RANGEBAR;
			StageCtrlEventFuncs[58] = CTRL_OBJ_ADDBUFF;
			StageCtrlEventFuncs[59] = CTRL_WAIT_OBJ_SPAWN;
			StageCtrlEventFuncs[60] = CTRL_WAIT_ENEMYACT;
			StageCtrlEventFuncs[61] = CTRL_PLAY_BGM;
			StageCtrlEventFuncs[62] = CTRL_PLAYER_TELEPORTEFFECT;
			StageCtrlEventFuncs[63] = CTRL_SHOW_BOSSHPBAR;
			StageCtrlEventFuncs[64] = CTRL_CLEAR_MOB_NOEVENT;
			StageCtrlEventFuncs[65] = CTRL_SHOW_CTRLHINT;
			StageCtrlEventFuncs[66] = CTRL_ROOMIN_WIMPOSE;
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
		}

		private void OnDestroy()
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
		}

		protected override void UpdateEvent()
		{
			if (bCheck && bUseBoxCollider2D)
			{
				vMax = EventB2D.bounds.max;
				vMin = EventB2D.bounds.min;
				if (!StageUpdate.bIsHost)
				{
					if (!bHasLockPlayerInput)
					{
						return;
					}
					for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
					{
						if (!StageUpdate.runPlayers[num].bIsNpcCpy && !StageUpdate.runPlayers[num].IsDead())
						{
							if (sTriggerPlayerID == "")
							{
								if (!listLockPlayer.Contains(StageUpdate.runPlayers[num]))
								{
									tLastPos = StageUpdate.runPlayers[num].vLastMovePt;
									tNowPos = StageUpdate.runPlayers[num].transform.position;
									if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
									{
										if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
										{
											StageUpdate.runPlayers[num].StopPlayer();
											StageUpdate.runPlayers[num].EventLockInputing = true;
											listLockPlayer.Add(StageUpdate.runPlayers[num]);
										}
										else if ((tLastPos.y < vMin.y && tNowPos.y > vMax.y) || (tLastPos.y > vMax.y && tNowPos.y < vMin.y))
										{
											StageUpdate.runPlayers[num].StopPlayer();
											StageUpdate.runPlayers[num].EventLockInputing = true;
											listLockPlayer.Add(StageUpdate.runPlayers[num]);
										}
									}
									else if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
									{
										if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
										{
											StageUpdate.runPlayers[num].StopPlayer();
											StageUpdate.runPlayers[num].EventLockInputing = true;
											listLockPlayer.Add(StageUpdate.runPlayers[num]);
										}
										else if ((tLastPos.x < vMin.x && tNowPos.x > vMax.x) || (tLastPos.x > vMax.x && tNowPos.x < vMin.x))
										{
											StageUpdate.runPlayers[num].StopPlayer();
											StageUpdate.runPlayers[num].EventLockInputing = true;
											listLockPlayer.Add(StageUpdate.runPlayers[num]);
										}
									}
								}
							}
							else if (StageUpdate.runPlayers[num].sPlayerID == sTriggerPlayerID)
							{
								tLastPos = StageUpdate.runPlayers[num].vLastMovePt;
								tNowPos = StageUpdate.runPlayers[num].transform.position;
								if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
								{
									if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
									{
										StageUpdate.runPlayers[num].StopPlayer();
										StageUpdate.runPlayers[num].EventLockInputing = true;
									}
									else if ((tLastPos.y < vMin.y && tNowPos.y > vMax.y) || (tLastPos.y > vMax.y && tNowPos.y < vMin.y))
									{
										StageUpdate.runPlayers[num].StopPlayer();
										StageUpdate.runPlayers[num].EventLockInputing = true;
									}
								}
								else if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
								{
									if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
									{
										StageUpdate.runPlayers[num].StopPlayer();
										StageUpdate.runPlayers[num].EventLockInputing = true;
									}
									else if ((tLastPos.x < vMin.x && tNowPos.x > vMax.x) || (tLastPos.x > vMax.x && tNowPos.x < vMin.x))
									{
										StageUpdate.runPlayers[num].StopPlayer();
										StageUpdate.runPlayers[num].EventLockInputing = true;
									}
								}
							}
						}
					}
					return;
				}
				if (listLockPlayer.Count > 0)
				{
					for (int num2 = listLockPlayer.Count - 1; num2 >= 0; num2--)
					{
						listLockPlayer[num2].EventLockInputing = false;
					}
					listLockPlayer.Clear();
				}
				for (int num3 = StageUpdate.runPlayers.Count - 1; num3 >= 0; num3--)
				{
					if (!StageUpdate.runPlayers[num3].bIsNpcCpy && !StageUpdate.runPlayers[num3].IsDead() && (!(sTriggerPlayerID != "") || !(sTriggerPlayerID != StageUpdate.runPlayers[num3].sPlayerID)))
					{
						tLastPos = StageUpdate.runPlayers[num3].vLastMovePt;
						tNowPos = StageUpdate.runPlayers[num3].transform.position;
						if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
						{
							if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
							{
								OnEvent(StageUpdate.runPlayers[num3].transform);
								if (!bCheck)
								{
									return;
								}
							}
							else if ((tLastPos.y < vMin.y && tNowPos.y > vMax.y) || (tLastPos.y > vMax.y && tNowPos.y < vMin.y))
							{
								OnEvent(StageUpdate.runPlayers[num3].transform);
								if (!bCheck)
								{
									return;
								}
							}
						}
						else if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
						{
							if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
							{
								OnEvent(StageUpdate.runPlayers[num3].transform);
								if (!bCheck)
								{
									return;
								}
							}
							else if ((tLastPos.x < vMin.x && tNowPos.x > vMax.x) || (tLastPos.x > vMax.x && tNowPos.x < vMin.x))
							{
								OnEvent(StageUpdate.runPlayers[num3].transform);
								if (!bCheck)
								{
									return;
								}
							}
						}
					}
				}
			}
			OnLateUpdate();
		}

		public void CallOnByID(EventManager.StageEventCall tStageEventCall)
		{
			int nID = tStageEventCall.nID;
			if (nID == 0)
			{
				STAGE_EVENT nStageEvent = tStageEventCall.nStageEvent;
				int num = 2;
			}
			else
			{
				if (nSetID != nID)
				{
					return;
				}
				Transform transform = tStageEventCall.tTransform;
				if (transform == null)
				{
					transform = StageUpdate.GetMainPlayerTrans();
				}
				if (StageUpdate.bIsHost)
				{
					OrangeCharacter orangeCharacter = transform.GetComponent<OrangeCharacter>();
					if (orangeCharacter == null)
					{
						transform = StageUpdate.GetMainPlayerTrans();
						orangeCharacter = transform.GetComponent<OrangeCharacter>();
					}
					if (orangeCharacter == null && StageUpdate.runPlayers.Count > 0)
					{
						orangeCharacter = StageUpdate.runPlayers[0];
						transform = orangeCharacter.transform;
					}
					if ((bool)orangeCharacter)
					{
						StageUpdate.SyncStageObj(sSyncID, 7, orangeCharacter.sPlayerID);
					}
					OnEvent(transform);
				}
			}
		}

		public override void OnEvent(Transform TriggerTransform)
		{
			if (!bCheck || bStartEvent)
			{
				return;
			}
			tObj = TriggerTransform.gameObject;
			fLeftTime = 0f;
			nStartIndex = 0;
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				if (StageUpdate.runPlayers[num].gameObject.GetInstanceID() == tObj.GetInstanceID())
				{
					if (StageUpdate.gbIsNetGame && bHasLockPlayerInput)
					{
						StageUpdate.runPlayers[num].StopPlayer();
						StageUpdate.runPlayers[num].EventLockInputing = true;
					}
					WaitPlayerNoCommand(StageUpdate.runPlayers[num]);
					break;
				}
			}
		}

		private void UnLockRangeFocus()
		{
			if (!bNoUnlockRange)
			{
				EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
				stageCameraFocus.bLock = false;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
				StageResManager.RegisterLockEvent(new TriggerLockEventData
				{
					fMax = 9999f,
					fMin = -9999f,
					fBtn = -9999f,
					fTop = 9999f,
					nType = 0,
					fSpeed = null,
					bSetFocus = false
				});
			}
		}

		private void WaitPlayerNoCommand(OrangeCharacter tOC)
		{
			bCheck = false;
			if (!bStartEvent)
			{
				tObj = tOC.gameObject;
				sTriggerPlayerID = tOC.sPlayerID;
				StageUpdate.SyncStageObj(sSyncID, 9, fLeftTime.ToString("0.000") + "," + sTriggerPlayerID);
				bStartEvent = true;
				UnLockRangeFocus();
				StageResManager.GetStageUpdate().nRunStageCtrlCount++;
				StageUpdate.AddStringMsg(base.gameObject.name + " Start");
			}
		}

		public override void StopEvent()
		{
			StopAllCoroutines();
			if (!bStartEvent)
			{
				return;
			}
			bStartEvent = false;
			StageResManager.GetStageUpdate().nRunStageCtrlCount--;
			if (bHasLockPlayerInput)
			{
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					StageUpdate.runPlayers[num].EventLockInputing = false;
				}
			}
		}

		public override void OnLateUpdate()
		{
			if (!bStartEvent || bLockWait)
			{
				return;
			}
			float num = fLeftTime + 0.016f;
			float num2 = 0f;
			for (int i = 0; i < nStartIndex; i++)
			{
				num2 += StageCtrlInsTructions[i].fWait;
			}
			string text = "";
			while (nStartIndex < StageCtrlInsTructions.Count)
			{
				StageCtrlInsTruction stageCtrlInsTruction = StageCtrlInsTructions[nStartIndex];
				num2 += stageCtrlInsTruction.fWait;
				if (!(fLeftTime <= num2) || !(num > num2))
				{
					break;
				}
				text = ((text.Length <= 0) ? nStartIndex.ToString() : (text + "," + nStartIndex));
				if (StageCtrlEventFuncs[stageCtrlInsTruction.tStageCtrl] != null)
				{
					if (StageCtrlEventFuncs[stageCtrlInsTruction.tStageCtrl](stageCtrlInsTruction))
					{
						break;
					}
				}
				else if (stageCtrlInsTruction.tStageCtrl == 41)
				{
					STAGE_TABLE value;
					ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value);
					bool flag = false;
					if (value != null && value.n_TYPE != 4)
					{
						if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.ContainsKey(ManagedSingleton<StageHelper>.Instance.nLastStageID) || (value != null && value.n_DIFFICULTY > 1))
						{
							flag = true;
						}
					}
					else if (value != null)
					{
						Dictionary<int, StageInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicStage.GetEnumerator();
						while (enumerator.MoveNext())
						{
							STAGE_TABLE value2;
							if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(enumerator.Current.Value.netStageInfo.StageID, out value2) && value2.n_MAIN == value.n_MAIN)
							{
								flag = true;
								break;
							}
						}
					}
					if (!StageUpdate.AllStageCtrlEvent && flag)
					{
						nStartIndex++;
						int num3 = Mathf.RoundToInt(stageCtrlInsTruction.nParam1) - 1;
						if (num3 >= StageCtrlInsTructions.Count || num3 == -1)
						{
							nStartIndex = StageCtrlInsTructions.Count;
						}
						else
						{
							while (nStartIndex < num3)
							{
								num2 += StageCtrlInsTructions[nStartIndex].fWait;
								fLeftTime += StageCtrlInsTructions[nStartIndex].fWait;
								num += StageCtrlInsTructions[nStartIndex].fWait;
								nStartIndex++;
							}
							nStartIndex--;
						}
					}
				}
				else if (stageCtrlInsTruction.tStageCtrl == 57)
				{
					nStartIndex++;
					int num4 = Mathf.RoundToInt(stageCtrlInsTruction.nParam1) - 1;
					if (num4 >= StageCtrlInsTructions.Count || num4 == -1)
					{
						nStartIndex = StageCtrlInsTructions.Count;
					}
					else
					{
						while (nStartIndex < num4)
						{
							num2 += StageCtrlInsTructions[nStartIndex].fWait;
							fLeftTime += StageCtrlInsTructions[nStartIndex].fWait;
							num += StageCtrlInsTructions[nStartIndex].fWait;
							nStartIndex++;
						}
						nStartIndex--;
					}
				}
				else
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL, tObj, stageCtrlInsTruction);
					StageUpdate.CheckAndRemoveEnemy();
				}
				nStartIndex++;
			}
			if (!bLockWait)
			{
				fLeftTime += 0.016f;
			}
			if (text.Length > 0)
			{
				StageUpdate.SyncStageObj(sSyncID, 3, text + "," + fLeftTime.ToString("0.000"), true);
			}
			if (nStartIndex >= StageCtrlInsTructions.Count)
			{
				bStartEvent = false;
				StageResManager.GetStageUpdate().nRunStageCtrlCount--;
			}
		}

		private bool CheckLockReturn()
		{
			if (bLockWait)
			{
				return true;
			}
			nStartIndex--;
			return false;
		}

		private bool CTRL_LOCK_INPUT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (!MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput)
			{
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					StageUpdate.runPlayers[num].StopPlayer();
					CharacterControlBase component = StageUpdate.runPlayers[num].GetComponent<CharacterControlBase>();
					if ((bool)component)
					{
						component.RemovePet();
					}
				}
				MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput = true;
			}
			return false;
		}

		private bool CTRL_UNLOCK_INPUT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput = false;
			return false;
		}

		private bool CTRL_MOVE_ATKBTN(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int num = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			if (num == 0)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL, tObj, tStageCtrlInsTruction);
				StageUpdate.CheckAndRemoveEnemy();
			}
			else
			{
				EnemyControllerBase tECB;
				OrangeCharacter tOC;
				StageUpdate.GetObjBySCEID(num, out tECB, out tOC);
				if (tOC != null)
				{
					tOC.ObjCtrl(tOC.gameObject, tStageCtrlInsTruction);
				}
			}
			return false;
		}

		private bool CTRL_EVENT_CALL(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int nID = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = nID;
			if (tObj != null)
			{
				stageEventCall.tTransform = tObj.transform;
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			return false;
		}

		private bool CTRL_SHOW_WARING(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (tStageCtrlInsTruction.nParam2 != 0f)
			{
				bLockWait = true;
				NotifyCallBack notifyCallBack = new NotifyCallBack();
				notifyCallBack.cbparam = ContinuePlay;
				notifyCallBack.nParam0 = ++nStartIndex;
				int p_param = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_WARING, p_param, notifyCallBack);
				return CheckLockReturn();
			}
			int p_param2 = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			Singleton<GenericEventManager>.Instance.NotifyEvent<int, NotifyCallBack>(EventManager.ID.STAGE_EVENT_WARING, p_param2, null);
			return false;
		}

		private bool CTRL_SHOW_MSG(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack newNCB = new NotifyCallBack();
			newNCB.cbparam = ContinuePlay;
			newNCB.nParam0 = ++nStartIndex;
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate.gbAddStageUseTime)
			{
				stageUpdate.gbAddStageUseTime = false;
				bNeedResumTimer = true;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Dialog", delegate(DialogUI ui)
			{
				ui.Setup((int)tStageCtrlInsTruction.nParam1, newNCB.CallCB);
			});
			if ((int)tStageCtrlInsTruction.nParam1 == 406401)
			{
				OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
				if (mainPlayerOC != null)
				{
					mainPlayerOC.StopAllLoopSE();
				}
			}
			return CheckLockReturn();
		}

		private bool CTRL_SHOW_ENEMYINFO(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = NetContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SHOW_ENEMYINFO, (int)tStageCtrlInsTruction.nParam1, notifyCallBack);
			return CheckLockReturn();
		}

		private bool CTRL_MOVE_RIGHTPOS(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			OrangeCharacter component = tObj.GetComponent<OrangeCharacter>();
			if (component != null)
			{
				bLockWait = true;
				RunUpdateClass runUpdateClass = new RunUpdateClass();
				NotifyCallBack notifyCallBack = new NotifyCallBack();
				notifyCallBack.cbparam = ContinuePlay;
				notifyCallBack.nParam0 = ++nStartIndex;
				runUpdateClass.oParams = new object[6];
				runUpdateClass.oParams[0] = component;
				runUpdateClass.oParams[4] = notifyCallBack;
				runUpdateClass.tEndCB = (Action<RunUpdateClass>)Delegate.Combine(runUpdateClass.tEndCB, new Action<RunUpdateClass>(WaitObjMoveToPosEndCB));
				tStageCtrlInsTruction.RemoveCB = runUpdateClass.EndCallBack;
				component.ObjCtrl(tObj, tStageCtrlInsTruction);
				return CheckLockReturn();
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL, tObj, tStageCtrlInsTruction);
			StageUpdate.CheckAndRemoveEnemy();
			return false;
		}

		private bool CTRL_MOVE_CAMERA(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			RunUpdateClass runUpdateClass = new RunUpdateClass();
			runUpdateClass.tUpdateCB = (Action<RunUpdateClass>)Delegate.Combine(runUpdateClass.tUpdateCB, new Action<RunUpdateClass>(UpdateMoveCamera));
			runUpdateClass.fParams = new float[5];
			if (tStageCtrlInsTruction.sMsg != null)
			{
				string[] array = tStageCtrlInsTruction.sMsg.Split(',');
				if (array.Length != 0 && array[0] == "1")
				{
					tStageCtrlInsTruction.nParam1 = tObj.transform.position.x;
					tStageCtrlInsTruction.nParam2 = tObj.transform.position.y + 0.6f;
				}
			}
			Vector3 position = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position;
			runUpdateClass.fParams[0] = tStageCtrlInsTruction.fTime * 0.47f;
			runUpdateClass.fParams[1] = (tStageCtrlInsTruction.nParam1 - position.x) / runUpdateClass.fParams[0];
			runUpdateClass.fParams[2] = (tStageCtrlInsTruction.nParam2 - position.y) / runUpdateClass.fParams[0];
			runUpdateClass.fParams[3] = 0f;
			runUpdateClass.fParams[4] = 0f;
			runUpdateClass.oParams = new object[2];
			runUpdateClass.oParams[0] = tStageCtrlInsTruction;
			runUpdateClass.oParams[1] = StartCoroutine(UpdateMoveCameraCC(runUpdateClass));
			SiCoroutineUpdate = (Action<EventPointBase>)Delegate.Combine(SiCoroutineUpdate, new Action<EventPointBase>(runUpdateClass.UpdateCall));
			return false;
		}

		private bool CTRL_CLEAR_MOB(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = 0;
			stageEventCall.nStageEvent = STAGE_EVENT.STAGE_ENEMYEVENT_STOP;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL, tObj, tStageCtrlInsTruction);
			StageUpdate.CheckAndRemoveEnemy();
			return false;
		}

		private bool CTRL_MOB_PLAYSHOWANI(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			NotifyCallBack notifyCallBack2 = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			notifyCallBack2.cb = CheckSend;
			bLockWait = true;
			bSendSuccess = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL, tObj, tStageCtrlInsTruction);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL_PLAYSHOWANI, notifyCallBack, notifyCallBack2);
			StageUpdate.CheckAndRemoveEnemy();
			if (!bSendSuccess)
			{
				StartCoroutine(ReSentCoroutine(tStageCtrlInsTruction));
				return true;
			}
			StartCoroutine(ContinuePlayTimeOut(notifyCallBack));
			return CheckLockReturn();
		}

		private bool CTRL_MOB_ADDBOSSHPBAR(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			BattleInfoUI.Instance.FullBossBarHP((int)tStageCtrlInsTruction.nParam1, 0, notifyCallBack.CallCB);
			return CheckLockReturn();
		}

		private bool CTRL_SHOW_READYGO(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			MonoBehaviourSingleton<StageSyncManager>.Instance.ShowReadyGo(notifyCallBack.CallCB);
			return CheckLockReturn();
		}

		private bool CTRL_CAMERA_FOLLOW(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
			stageCameraFocus.bLock = true;
			stageCameraFocus.bRightNow = true;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			return false;
		}

		private bool CTRL_CAMERA_NOFOLLOW(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
			stageCameraFocus.bLock = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			return false;
		}

		private bool CTRL_CAMERA_MOVEFOLLOW(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (tObj != null)
			{
				StartCoroutine(MoveFollowCameraCoroutine(tObj.transform.position, tObj));
			}
			return false;
		}

		private bool CTRL_PLAYER_TELEPORT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (tObj != null)
			{
				Vector3 localPosition = tObj.transform.localPosition;
				if (tStageCtrlInsTruction.nParam1 != 0f && tStageCtrlInsTruction.nParam2 != 0f)
				{
					localPosition.x = tStageCtrlInsTruction.nParam1;
					localPosition.y = tStageCtrlInsTruction.nParam2;
				}
				float fWaitTime = (StageUpdate.gbIsNetGame ? 3f : 0f);
				bool bSetLookDir = false;
				bool bLookFront = false;
				bool bTeleportWaitTime = false;
				if (tStageCtrlInsTruction.sMsg != null && tStageCtrlInsTruction.sMsg != "")
				{
					string[] array = tStageCtrlInsTruction.sMsg.Split(',');
					if (array.Length >= 2)
					{
						bSetLookDir = array[0] == "1";
						bLookFront = array[1] == "1";
						if (array.Length >= 3)
						{
							bTeleportWaitTime = array[2] == "1";
						}
					}
				}
				bLockWait = true;
				NotifyCallBack notifyCallBack = new NotifyCallBack();
				notifyCallBack.cbparam = ContinuePlay;
				notifyCallBack.nParam0 = ++nStartIndex;
				StartCoroutine(WaitTeleportCoroutine(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, localPosition, fWaitTime, notifyCallBack, false, bSetLookDir, bLookFront, bTeleportWaitTime));
			}
			return CheckLockReturn();
		}

		private bool CTRL_RUN_TUTO(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate.gbAddStageUseTime)
			{
				stageUpdate.gbAddStageUseTime = false;
				bNeedResumTimer = true;
			}
			int num = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			TurtorialUI.CheckTurtorialID(num, notifyCallBack.CallCB);
			if (num == 402801)
			{
				OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
				if (mainPlayerOC != null)
				{
					mainPlayerOC.StopAllLoopSE();
				}
			}
			return CheckLockReturn();
		}

		private bool CTRL_SHOW_ALWAYSHINTMESSAGE(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (BattleInfoUI.Instance != null)
			{
				int nParam = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
				int nParam2 = Mathf.RoundToInt(tStageCtrlInsTruction.nParam2);
				BattleInfoUI.Instance.ShowAlwaysHintMsg(tStageCtrlInsTruction.sMsg, nParam, nParam2);
			}
			return false;
		}

		private bool CTRL_SHOW_TIMEHINTMESSAGE(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (BattleInfoUI.Instance != null)
			{
				int nParam = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
				int nParam2 = Mathf.RoundToInt(tStageCtrlInsTruction.nParam2);
				BattleInfoUI.Instance.ShowTimeHintMsg(tStageCtrlInsTruction.sMsg, nParam, nParam2, tStageCtrlInsTruction.fTime);
			}
			return false;
		}

		private bool CTRL_CLOSE_PLAYERACTIVE(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (tObj != null)
			{
				OrangeCharacter component = tObj.GetComponent<OrangeCharacter>();
				if (component != null)
				{
					component.Activate = false;
					Animator[] componentsInChildren = component.GetComponentsInChildren<Animator>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].enabled = false;
					}
				}
			}
			return false;
		}

		private bool CTRL_OPEN_PLAYERACTIVE(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (tObj != null)
			{
				OrangeCharacter component = tObj.GetComponent<OrangeCharacter>();
				if (component != null)
				{
					component.Activate = true;
					Animator[] componentsInChildren = component.GetComponentsInChildren<Animator>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].enabled = true;
					}
				}
			}
			return false;
		}

		private bool CTRL_SHOW_DRAGONBONE(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (tStageCtrlInsTruction.sMsg != "")
			{
				int nTmpID = Mathf.RoundToInt(tStageCtrlInsTruction.fTime);
				string[] array = tStageCtrlInsTruction.sMsg.Split(',');
				string sTmpName = array[0];
				int nSenarioID = 0;
				bool bLockHint = false;
				string sLockName = "";
				if (array.Length >= 2)
				{
					nSenarioID = int.Parse(array[1]);
				}
				if (array.Length >= 3)
				{
					bLockHint = array[2] == "1";
				}
				if (array.Length >= 4)
				{
					sLockName = array[3];
				}
				bLockWait = true;
				NotifyCallBack newNCB = new NotifyCallBack();
				newNCB.cbparam = ContinuePlay;
				newNCB.nParam0 = ++nStartIndex;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("dragonbones/" + sTmpName, sTmpName, delegate(GameObject tdgbs)
				{
					Transform uI_Parent = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent;
					if (uI_Parent != null)
					{
						GameObject gameObject = new GameObject();
						gameObject.transform.SetParent(uI_Parent);
						float num = tStageCtrlInsTruction.nParam1;
						float num2 = tStageCtrlInsTruction.nParam2;
						if (!(sLockName == ""))
						{
							Canvas component = uI_Parent.parent.GetComponent<Canvas>();
							Transform transform = uI_Parent.parent.Find(sLockName);
							if (transform != null)
							{
								Vector2 sizeDelta = ((RectTransform)transform).sizeDelta;
								Vector3 position = transform.position;
								Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(component.worldCamera, position);
								Vector2 localPoint;
								RectTransformUtility.ScreenPointToLocalPointInRectangle(component.GetComponent<RectTransform>(), screenPoint, component.worldCamera, out localPoint);
								num += localPoint.x + (((RectTransform)transform).pivot.x - 0.5f) * sizeDelta.x;
								num2 += localPoint.y + (((RectTransform)transform).pivot.y - 0.5f) * sizeDelta.y;
							}
						}
						gameObject.transform.localPosition = new Vector3(num, num2, 0f);
						gameObject.transform.localScale = Vector3.one;
						gameObject.name = sTmpName + nTmpID;
						UnityEngine.Object.Instantiate(tdgbs, gameObject.transform);
						StageUpdate.RegisterEndRemoveObj(gameObject);
						if (nSenarioID != 0)
						{
							TurtorialUI.CheckStageTurtorialID(nSenarioID, gameObject.transform, bLockHint, newNCB.CallCB);
						}
						else
						{
							newNCB.CallCB();
						}
					}
					else
					{
						newNCB.CallCB();
					}
				});
			}
			return CheckLockReturn();
		}

		private bool CTRL_CLOSE_DRAGONBONE(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			Transform uI_Parent = MonoBehaviourSingleton<UIManager>.Instance.UI_Parent;
			if (uI_Parent != null)
			{
				int num = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
				string[] array = tStageCtrlInsTruction.sMsg.Split(',');
				if (array.Length >= 2)
				{
					int.Parse(array[1]);
				}
				string text = array[0];
				Transform transform = uI_Parent.Find(text + num);
				if (transform != null)
				{
					StageUpdate.RemoveEndRemoveObj(transform.gameObject);
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
			return false;
		}

		private bool CTRL_WAIT_PLAYERACT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			int mainstatus = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			int substatus = Mathf.RoundToInt(tStageCtrlInsTruction.nParam2);
			StartCoroutine(WaitObjActCoroutine(tObj, mainstatus, substatus, notifyCallBack));
			return CheckLockReturn();
		}

		private bool CTRL_WAIT_BULLETIN(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			StartCoroutine(WaitBulletInCoroutine(notifyCallBack));
			return CheckLockReturn();
		}

		private bool CTRL_SET_OBJ_ACT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int nSCEID = Mathf.RoundToInt(tStageCtrlInsTruction.fTime);
			int num = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			bool flag = false;
			if (tStageCtrlInsTruction.nParam2 != 0f)
			{
				flag = true;
			}
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			if (!flag)
			{
				if (tECB != null)
				{
					if (tStageCtrlInsTruction.sMsg == "SetFly")
					{
						tECB.IgnoreGravity = true;
					}
					else if (tStageCtrlInsTruction.sMsg == "UnSetFly")
					{
						tECB.IgnoreGravity = false;
					}
					else if (tStageCtrlInsTruction.sMsg == "OnActive")
					{
						tECB.Activate = true;
					}
					else if (tStageCtrlInsTruction.sMsg == "OffActive")
					{
						tECB.Activate = false;
					}
					else
					{
						StageUpdate.SetEnemyActBySCEID(nSCEID, num, tStageCtrlInsTruction.sMsg, null);
					}
				}
				else if (tOC != null)
				{
					if (tStageCtrlInsTruction.sMsg.StartsWith("Telopout"))
					{
						tOC.TeleportOut();
						if (tStageCtrlInsTruction.sMsg.Substring(tStageCtrlInsTruction.sMsg.IndexOf(',') + 1) == "1")
						{
							tOC.RemovePlayerObjInfoBar();
						}
					}
					else if (tStageCtrlInsTruction.sMsg == "PlayCharge1")
					{
						tOC.PlaySE("BattleSE", "bt_xu_charge_lp");
						ChargeShootObj component = tOC.GetComponent<ChargeShootObj>();
						if ((bool)component)
						{
							component.PlayCharge(true, false);
						}
					}
					else if (tStageCtrlInsTruction.sMsg == "PlayCharge2")
					{
						ChargeShootObj component2 = tOC.GetComponent<ChargeShootObj>();
						if ((bool)component2)
						{
							component2.PlayCharge(false, true);
						}
					}
					else if (tStageCtrlInsTruction.sMsg == "DisableWeapon")
					{
						tOC.DisableCurrentWeapon();
					}
					else if (tStageCtrlInsTruction.sMsg == "Dash")
					{
						StageCtrlInsTruction stageCtrlInsTruction = new StageCtrlInsTruction();
						stageCtrlInsTruction.tStageCtrl = 7;
						stageCtrlInsTruction.fTime = 0.5f;
						tOC.ObjCtrl(tOC.gameObject, stageCtrlInsTruction);
					}
					else if (tStageCtrlInsTruction.sMsg == "UnDash")
					{
						StageCtrlInsTruction stageCtrlInsTruction2 = new StageCtrlInsTruction();
						stageCtrlInsTruction2.tStageCtrl = 8;
						tOC.ObjCtrl(tOC.gameObject, stageCtrlInsTruction2);
					}
					else if (tStageCtrlInsTruction.sMsg == "Skill0")
					{
						StageCtrlInsTruction stageCtrlInsTruction3 = new StageCtrlInsTruction();
						stageCtrlInsTruction3.tStageCtrl = 10;
						stageCtrlInsTruction3.fTime = 0.5f;
						tOC.ObjCtrl(tOC.gameObject, stageCtrlInsTruction3);
					}
					else if (tStageCtrlInsTruction.sMsg == "Skill1")
					{
						StageCtrlInsTruction stageCtrlInsTruction4 = new StageCtrlInsTruction();
						stageCtrlInsTruction4.tStageCtrl = 11;
						stageCtrlInsTruction4.fTime = 0.5f;
						tOC.ObjCtrl(tOC.gameObject, stageCtrlInsTruction4);
					}
					else if (tStageCtrlInsTruction.sMsg == "SetFly")
					{
						tOC.IgnoreGravity = true;
					}
					else if (tStageCtrlInsTruction.sMsg == "UnSetFly")
					{
						tOC.IgnoreGravity = false;
					}
					else if (tStageCtrlInsTruction.sMsg.StartsWith("UnRide"))
					{
						string text = tStageCtrlInsTruction.sMsg.Substring(tStageCtrlInsTruction.sMsg.IndexOf(',') + 1);
						if (tOC.refRideBaseObj != null)
						{
							if (text == "1")
							{
								tOC.refRideBaseObj.UnRide(true);
							}
							else
							{
								tOC.refRideBaseObj.UnRide(false);
							}
						}
					}
					else
					{
						tOC.transform.Find("model").GetComponent<Animator>().SetInteger("NowStatus", num);
						tOC.PlaySE("BattleSE", "bt_xu_charge_stop");
						ChargeShootObj component3 = tOC.GetComponent<ChargeShootObj>();
						if ((bool)component3)
						{
							component3.PlayCharge(false, false);
						}
					}
				}
			}
			else
			{
				if (tECB != null)
				{
					bLockWait = true;
					NotifyCallBack notifyCallBack = new NotifyCallBack();
					notifyCallBack.cbparam = ContinuePlay;
					notifyCallBack.nParam0 = ++nStartIndex;
					StageUpdate.SetEnemyActBySCEID(nSCEID, num, tStageCtrlInsTruction.sMsg, notifyCallBack.CallCB);
					return CheckLockReturn();
				}
				if (tOC != null)
				{
					Transform transform = tOC.transform.Find("model");
					Animator component4 = transform.GetComponent<Animator>();
					if (transform.GetComponent<StageAnimationEvent>() == null)
					{
						transform.gameObject.AddComponent<StageAnimationEvent>();
					}
					bLockWait = true;
					NotifyCallBack notifyCallBack2 = new NotifyCallBack();
					notifyCallBack2.cbparam = ContinuePlay;
					notifyCallBack2.nParam0 = ++nStartIndex;
					tOC.tAnimationCB = notifyCallBack2.CallCB;
					component4.SetInteger("NowStatus", num);
					tOC.PlaySE("BattleSE", "bt_xu_charge_stop");
					ChargeShootObj component5 = tOC.GetComponent<ChargeShootObj>();
					if ((bool)component5)
					{
						component5.PlayCharge(false, false);
					}
					return CheckLockReturn();
				}
			}
			return false;
		}

		private bool CTRL_SET_OBJ_ANIMATOR(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int nSCEID = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			bool flag = false;
			if (array.Length > 1 && array[1] == "1")
			{
				flag = true;
			}
			Animator tAnimator = null;
			if (tStageCtrlInsTruction.nParam2 == 0f)
			{
				EnemyControllerBase tECB;
				OrangeCharacter tOC;
				StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
				if (tECB != null)
				{
					Transform transform = tECB.transform.Find("model");
					tAnimator = transform.GetComponent<Animator>();
				}
				else if (tOC != null)
				{
					Transform transform2 = tOC.transform.Find("model");
					tAnimator = transform2.GetComponent<Animator>();
					if (flag)
					{
						tOC.DisableSkillWeapon(0);
						tOC.DisableSkillWeapon(1);
						tOC.DisableCurrentWeapon();
					}
					if (array[0] == "ReAnimator")
					{
						tAnimator.runtimeAnimatorController = tOC.GetComponent<StageObjParam>().tDefaultAnimator;
						tOC.EnableCurrentWeapon();
						return false;
					}
					tOC.GetComponent<StageObjParam>().tDefaultAnimator = tAnimator.runtimeAnimatorController;
				}
			}
			else
			{
				OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
				if (mainPlayerOC != null)
				{
					Transform transform3 = mainPlayerOC.transform.Find("model");
					tAnimator = transform3.GetComponent<Animator>();
					if (flag)
					{
						if (mainPlayerOC.PlayerPressSelectCB != new Callback(mainPlayerOC.PlayerPressSelect))
						{
							bool bPauseAllPlayerInput = MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput;
							MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput = false;
							mainPlayerOC.PlayerPressSelectCB.CheckTargetToInvoke();
							mainPlayerOC.CheckSkillEvt.CheckTargetToInvoke();
							MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput = bPauseAllPlayerInput;
						}
						mainPlayerOC.DisableSkillWeapon(0);
						mainPlayerOC.DisableSkillWeapon(1);
						mainPlayerOC.DisableCurrentWeapon();
					}
					if (array[0] == "ReAnimator")
					{
						tAnimator.runtimeAnimatorController = mainPlayerOC.GetComponent<StageObjParam>().tDefaultAnimator;
						mainPlayerOC.EnableCurrentWeapon();
						return false;
					}
					mainPlayerOC.GetComponent<StageObjParam>().tDefaultAnimator = tAnimator.runtimeAnimatorController;
				}
			}
			if (tAnimator != null)
			{
				bLockWait = true;
				NotifyCallBack newNCB = new NotifyCallBack();
				newNCB.cbparam = ContinuePlay;
				newNCB.nParam0 = ++nStartIndex;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/" + array[0], array[0], delegate(UnityEngine.Object assobj)
				{
					if (assobj != null)
					{
						tAnimator.runtimeAnimatorController = assobj as RuntimeAnimatorController;
					}
					newNCB.CallCB();
				});
				return CheckLockReturn();
			}
			return false;
		}

		private bool CTRL_SET_DEAD_CALLEVENT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int nSCEID = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			int nEventID = Mathf.RoundToInt(tStageCtrlInsTruction.nParam2);
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			if (tECB != null)
			{
				tECB.gameObject.AddOrGetComponent<StageObjParam>().nEventID = nEventID;
			}
			return false;
		}

		private bool CTRL_OBJ_TO_OBJ(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int nSCEID = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			float nParam = tStageCtrlInsTruction.nParam2;
			float num = 0f;
			bool bEnemy = false;
			if (array.Length > 1)
			{
				num = float.Parse(array[0]);
				bEnemy = array[1] == "1";
			}
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			Vector3 position = base.transform.position;
			position = ((tECB != null) ? (tECB.transform.position + Vector3.right * tECB.direction * nParam + Vector3.up * num) : ((!(tOC != null)) ? new Vector3(nParam, num, 0f) : (tOC.transform.position + Vector3.right * (float)tOC._characterDirection * nParam + Vector3.up * num)));
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			StartCoroutine(WaitPlayerToObj(position, notifyCallBack, bEnemy));
			return CheckLockReturn();
		}

		private bool CTRL_CREATE_NPCPLAYER_TO_OBJ(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			int characterID = int.Parse(array[0]);
			int num = int.Parse(array[1]);
			int nSCEID = int.Parse(array[2]);
			float num2 = float.Parse(array[3]);
			float num3 = float.Parse(array[4]);
			int nSetSCEID = int.Parse(array[5]);
			bool bIsEnemy = false;
			if (array.Length >= 7)
			{
				bIsEnemy = GetBoolBySaveStr(array[6]);
			}
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			GameObject obj = new GameObject();
			PlayerBuilder playerBuilder = obj.AddComponent<PlayerBuilder>();
			playerBuilder.SetPBP.CharacterID = characterID;
			playerBuilder.bShowStartEffect = false;
			Vector3 position = base.transform.position;
			if (tECB != null)
			{
				position = tECB.transform.position + Vector3.right * tECB.direction * num2 + Vector3.up * num3;
				playerBuilder.SetPBP.tSetCharacterDir = (CharacterDirection)(-tECB.direction);
			}
			else if (tOC != null)
			{
				position = tOC.transform.position + Vector3.right * (float)tOC._characterDirection * num2 + Vector3.up * num3;
				playerBuilder.SetPBP.tSetCharacterDir = (CharacterDirection)(0 - tOC._characterDirection);
			}
			else
			{
				position = new Vector3(num2, num3, 0f);
				playerBuilder.SetPBP.tSetCharacterDir = CharacterDirection.RIGHT;
			}
			playerBuilder.SetPBP.WeaponList[0] = num;
			playerBuilder.SetPBP.WeaponList[1] = 0;
			playerBuilder.IsJustNPC = true;
			playerBuilder.CreateAtStart = false;
			playerBuilder.uid = "NPC" + nSetSCEID;
			playerBuilder.SetPBP.sPlayerName = " ";
			playerBuilder.SetPBP.sPlayerID = "NPC_";
			obj.transform.position = position;
			obj.transform.localScale = Vector3.one;
			obj.transform.rotation = Quaternion.identity;
			bLockWait = true;
			NotifyCallBack newNCB = new NotifyCallBack();
			newNCB.cbparam = ContinuePlay;
			newNCB.nParam0 = ++nStartIndex;
			playerBuilder.CreatePlayer(delegate(object[] objs)
			{
				OrangeCharacter orangeCharacter = objs[0] as OrangeCharacter;
				ManagedSingleton<InputStorage>.Instance.AddInputData(orangeCharacter.UserID.ToString());
				orangeCharacter.gameObject.AddOrGetComponent<StageObjParam>().nSCEID = nSetSCEID;
				if (bIsEnemy)
				{
					orangeCharacter.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer;
					orangeCharacter.TargetMask = ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerUseMask;
				}
				newNCB.CallCB();
			});
			return CheckLockReturn();
		}

		private bool CTRL_MOVE_OBJ_TO_OBJ(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			int nSCEID = int.Parse(array[0]);
			int nSCEID2 = int.Parse(array[1]);
			float num = float.Parse(array[2]);
			float num2 = 0f;
			if (array.Length > 3)
			{
				num2 = float.Parse(array[3]);
			}
			bool flag = false;
			if (array.Length > 4)
			{
				flag = GetBoolBySaveStr(array[4]);
			}
			bool flag2 = true;
			if (array.Length > 5)
			{
				flag2 = GetBoolBySaveStr(array[5]);
			}
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			EnemyControllerBase tECB2;
			OrangeCharacter tOC2;
			StageUpdate.GetObjBySCEID(nSCEID2, out tECB2, out tOC2);
			Vector3 vector = base.transform.position;
			if (tECB2 != null)
			{
				vector = tECB2.transform.position + Vector3.right * tECB2.direction * num + Vector3.up * num2;
			}
			else if (tOC2 != null)
			{
				vector = tOC2.transform.position + Vector3.right * (float)tOC2._characterDirection * num + Vector3.up * num2;
			}
			else if (tECB != null)
			{
				vector = ((num2 != 0f) ? new Vector3(num, num2, 0f) : new Vector3(num, tECB.transform.position.y, 0f));
			}
			else if (tOC != null)
			{
				vector = ((num2 != 0f) ? new Vector3(num, num2, 0f) : new Vector3(num, tOC.transform.position.y, 0f));
			}
			if (tStageCtrlInsTruction.fTime == 0f)
			{
				if (tOC != null)
				{
					tOC.transform.position = vector;
					tOC.Controller.LogicPosition = new VInt3(tOC.transform.localPosition);
				}
				else if (tECB != null)
				{
					tECB.transform.position = vector;
					tECB.Controller.LogicPosition = new VInt3(tECB.transform.localPosition);
				}
				return false;
			}
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			RunUpdateClass runUpdateClass = new RunUpdateClass();
			runUpdateClass.tUpdateCB = (Action<RunUpdateClass>)Delegate.Combine(runUpdateClass.tUpdateCB, new Action<RunUpdateClass>(WaitObjMoveToPos));
			runUpdateClass.fParams = new float[1];
			runUpdateClass.fParams[0] = tStageCtrlInsTruction.fTime;
			runUpdateClass.oParams = new object[6];
			runUpdateClass.oParams[0] = tOC;
			runUpdateClass.oParams[1] = tECB;
			runUpdateClass.oParams[2] = vector;
			if (tOC != null)
			{
				runUpdateClass.oParams[3] = (vector - tOC.transform.position) / tStageCtrlInsTruction.fTime;
			}
			else if (tECB != null)
			{
				runUpdateClass.oParams[3] = (vector - tECB.transform.position) / tStageCtrlInsTruction.fTime;
			}
			runUpdateClass.oParams[4] = null;
			if (flag)
			{
				runUpdateClass.oParams[4] = notifyCallBack;
			}
			else
			{
				notifyCallBack.CallCB();
			}
			runUpdateClass.oParams[5] = flag2;
			SiCoroutineUpdate = (Action<EventPointBase>)Delegate.Combine(SiCoroutineUpdate, new Action<EventPointBase>(runUpdateClass.UpdateCall));
			return CheckLockReturn();
		}

		private bool CTRL_SHOT_BULLET(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			int nSCEID = int.Parse(array[0]);
			float num = float.Parse(array[1]);
			float num2 = float.Parse(array[2]);
			int nSkillID = int.Parse(array[4]);
			float num3 = float.Parse(array[5]);
			float y = float.Parse(array[6]);
			float z = float.Parse(array[7]);
			bool flag = array[8] == "1";
			bool flag2 = array[9] == "1";
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			Vector3 tPos = base.transform.position;
			EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
			int num4 = 1;
			if (tECB != null)
			{
				if (array[3] != "")
				{
					Transform transform = OrangeBattleUtility.FindChildRecursive(tECB.transform, array[3]);
					if (transform != null)
					{
						num4 = tECB.direction;
						tPos = transform.position;
					}
					else
					{
						num4 = tECB.direction;
						tPos = tECB.transform.position + Vector3.right * num4 * num + Vector3.up * num2;
					}
				}
				else
				{
					num4 = tECB.direction;
					tPos = tECB.transform.position + Vector3.right * num4 * num + Vector3.up * num2;
				}
			}
			else if (tOC != null)
			{
				if (array[3] != "")
				{
					Transform transform2 = OrangeBattleUtility.FindChildRecursive(tOC.transform, array[3]);
					if (transform2 != null)
					{
						num4 = (int)tOC._characterDirection;
						tPos = transform2.position;
					}
					else
					{
						num4 = (int)tOC._characterDirection;
						tPos = tOC.transform.position + Vector3.right * num4 * num + Vector3.up * num2;
					}
				}
				else
				{
					num4 = (int)tOC._characterDirection;
					tPos = tOC.transform.position + Vector3.right * num4 * num + Vector3.up * num2;
				}
			}
			stageSkillAtkTargetParam.tDir = new Vector3(num3 * (float)num4, y, z).normalized;
			stageSkillAtkTargetParam.nSkillID = nSkillID;
			stageSkillAtkTargetParam.bAtkNoCast = false;
			stageSkillAtkTargetParam.tPos = tPos;
			stageSkillAtkTargetParam.bBuff = false;
			stageSkillAtkTargetParam.tTrans = null;
			stageSkillAtkTargetParam.tLM = 0;
			if (flag)
			{
				stageSkillAtkTargetParam.tLM = (int)stageSkillAtkTargetParam.tLM + (int)BulletScriptableObject.Instance.BulletLayerMaskPlayer;
			}
			if (flag2)
			{
				stageSkillAtkTargetParam.tLM = (int)stageSkillAtkTargetParam.tLM + (int)BulletScriptableObject.Instance.BulletLayerMaskEnemy;
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
			return false;
		}

		private bool CTRL_SET_OBJ_DIR(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			int nSCEID = int.Parse(array[0]);
			int nSCEID2 = int.Parse(array[1]);
			bool flag = array[2] == "1";
			bool num = array[3] == "1";
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			EnemyControllerBase tECB2;
			OrangeCharacter tOC2;
			StageUpdate.GetObjBySCEID(nSCEID2, out tECB2, out tOC2);
			if (num)
			{
				OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
				if (tECB2 != null)
				{
					if (mainPlayerOC.transform.position.x > tECB2.transform.position.x)
					{
						mainPlayerOC._characterDirection = CharacterDirection.LEFT;
					}
					else
					{
						mainPlayerOC._characterDirection = CharacterDirection.RIGHT;
					}
				}
				else if (tOC2 != null)
				{
					if (mainPlayerOC.transform.position.x > tOC2.transform.position.x)
					{
						mainPlayerOC._characterDirection = CharacterDirection.LEFT;
					}
					else
					{
						mainPlayerOC._characterDirection = CharacterDirection.RIGHT;
					}
				}
				else if (flag)
				{
					mainPlayerOC._characterDirection = CharacterDirection.LEFT;
				}
				else
				{
					mainPlayerOC._characterDirection = CharacterDirection.RIGHT;
				}
			}
			else if (tECB != null)
			{
				if (tECB2 != null)
				{
					if (tECB.transform.position.x > tECB2.transform.position.x)
					{
						tECB.SetPositionAndRotation(tECB.transform.position, true);
					}
					else
					{
						tECB.SetPositionAndRotation(tECB.transform.position, false);
					}
				}
				else if (tOC2 != null)
				{
					if (tECB.transform.position.x > tOC2.transform.position.x)
					{
						tECB.SetPositionAndRotation(tECB.transform.position, true);
					}
					else
					{
						tECB.SetPositionAndRotation(tECB.transform.position, false);
					}
				}
				else if (flag)
				{
					tECB.SetPositionAndRotation(tECB.transform.position, true);
				}
				else
				{
					tECB.SetPositionAndRotation(tECB.transform.position, false);
				}
			}
			else if (tOC != null)
			{
				if (tECB2 != null)
				{
					if (tOC.transform.position.x > tECB2.transform.position.x)
					{
						tOC._characterDirection = CharacterDirection.LEFT;
					}
					else
					{
						tOC._characterDirection = CharacterDirection.RIGHT;
					}
				}
				else if (tOC2 != null)
				{
					if (tOC.transform.position.x > tOC2.transform.position.x)
					{
						tOC._characterDirection = CharacterDirection.LEFT;
					}
					else
					{
						tOC._characterDirection = CharacterDirection.RIGHT;
					}
				}
				else if (flag)
				{
					tOC._characterDirection = CharacterDirection.LEFT;
				}
				else
				{
					tOC._characterDirection = CharacterDirection.RIGHT;
				}
			}
			return false;
		}

		private bool CTRL_SET_MAINPLAYER_SCEID(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int nSCEID = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			StageUpdate.GetMainPlayerOC().gameObject.AddOrGetComponent<StageObjParam>().nSCEID = nSCEID;
			return false;
		}

		private bool CTRL_CALL_STAGE_END(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
			return false;
		}

		private bool CTRL_SET_OBJ_LAYER(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int num = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
			switch (num)
			{
			case 1:
				mainPlayerOC.gameObject.layer = 14;
				break;
			case 2:
				mainPlayerOC.gameObject.layer = 0;
				break;
			}
			return false;
		}

		private bool CTRL_PLAY_FX(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			string text = array[0];
			int nSCEID = 0;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			if (array.Length > 2)
			{
				nSCEID = int.Parse(array[1]);
				flag = array[2] == "1";
			}
			if (array.Length > 6)
			{
				flag2 = array[3] == "1";
				flag3 = array[4] == "1";
				flag4 = array[5] == "1";
				flag5 = array[6] == "1";
			}
			if (array.Length > 7)
			{
				flag6 = array[7] == "1";
			}
			Vector3 vector = new Vector3(tStageCtrlInsTruction.nParam1, tStageCtrlInsTruction.nParam2, 0f);
			switch (text)
			{
			case "ShowExplodeWhite":
				BattleInfoUI.Instance.ShowExplodeWhite();
				break;
			case "CloseExplodeWhite":
				BattleInfoUI.Instance.CloseExplodeWhite();
				break;
			case "ShowExplodeBlack":
				BattleInfoUI.Instance.ShowExplodeBlack();
				break;
			case "CloseExplodeBlack":
				BattleInfoUI.Instance.CloseExplodeBlack();
				break;
			case "Snow":
				if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect == 0)
				{
					return false;
				}
				if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.ResolutionRate == 0.5f)
				{
					return false;
				}
				if (flag)
				{
					Transform transform7 = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.transform.Find("Snow");
					if (transform7 != null)
					{
						FxBase component4 = transform7.GetComponent<FxBase>();
						if (component4 != null)
						{
							component4.StopEmittingBackToPool();
						}
					}
				}
				else
				{
					Transform pTransform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
					StageFXParam stageFXParam = new StageFXParam();
					stageFXParam.fPlayTime = 99999f;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("Snow", pTransform, Quaternion.Euler(0f, 0f, 0f), new object[1] { stageFXParam });
				}
				break;
			default:
			{
				if (flag && (flag2 || flag3 || flag4 || flag5 || flag6))
				{
					Transform transform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
					for (int num = transform.childCount - 1; num >= 0; num--)
					{
						Transform child = transform.GetChild(num);
						if (child.gameObject.name.Contains(text))
						{
							FxBase component = child.GetComponent<FxBase>();
							if ((bool)component)
							{
								component.BackToPool();
							}
						}
					}
					break;
				}
				if (flag2)
				{
					Transform transform2 = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
					vector.x = 0f - ManagedSingleton<StageHelper>.Instance.fCameraWHalf;
					vector.y = 0f;
					vector.z = 0f - transform2.position.z;
					MonoBehaviourSingleton<FxManager>.Instance.PlayWihtOffset<FxBase>(text, transform2, Quaternion.Euler(0f, 0f, 0f), vector, Array.Empty<object>());
					break;
				}
				if (flag3)
				{
					Transform transform3 = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
					vector.x = ManagedSingleton<StageHelper>.Instance.fCameraWHalf;
					vector.y = 0f;
					vector.z = 0f - transform3.position.z;
					MonoBehaviourSingleton<FxManager>.Instance.PlayWihtOffset<FxBase>(text, transform3, Quaternion.Euler(0f, 0f, 0f), vector, Array.Empty<object>());
					break;
				}
				if (flag4)
				{
					Transform transform4 = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
					vector.x = 0f;
					vector.y = ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
					vector.z = 0f - transform4.position.z;
					MonoBehaviourSingleton<FxManager>.Instance.PlayWihtOffset<FxBase>(text, transform4, Quaternion.Euler(0f, 0f, 0f), vector, Array.Empty<object>());
					break;
				}
				if (flag5)
				{
					Transform transform5 = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
					vector.x = 0f;
					vector.y = 0f - ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
					vector.z = 0f - transform5.position.z;
					MonoBehaviourSingleton<FxManager>.Instance.PlayWihtOffset<FxBase>(text, transform5, Quaternion.Euler(0f, 0f, 0f), vector, Array.Empty<object>());
					break;
				}
				if (flag6)
				{
					Transform transform6 = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
					vector.x = 0f;
					vector.y = 0f;
					vector.z = 0f - transform6.position.z;
					MonoBehaviourSingleton<FxManager>.Instance.PlayWihtOffset<FxBase>(text, transform6, Quaternion.Euler(0f, 0f, 0f), vector, Array.Empty<object>());
					break;
				}
				EnemyControllerBase tECB;
				OrangeCharacter tOC;
				StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
				if (tOC != null)
				{
					if (flag)
					{
						for (int num2 = tOC.transform.childCount - 1; num2 >= 0; num2--)
						{
							Transform child2 = tOC.transform.GetChild(num2);
							if (child2.gameObject.name.Contains(text))
							{
								FxBase component2 = child2.GetComponent<FxBase>();
								if ((bool)component2)
								{
									component2.BackToPool();
								}
							}
						}
					}
					else
					{
						MonoBehaviourSingleton<FxManager>.Instance.PlayWihtOffset<FxBase>(text, tOC.transform, Quaternion.Euler(0f, 0f, 0f), vector, Array.Empty<object>());
					}
				}
				else if (tECB != null)
				{
					if (flag)
					{
						for (int num3 = tECB.transform.childCount - 1; num3 >= 0; num3--)
						{
							Transform child3 = tECB.transform.GetChild(num3);
							if (child3.gameObject.name.Contains(text))
							{
								FxBase component3 = child3.GetComponent<FxBase>();
								if ((bool)component3)
								{
									component3.BackToPool();
								}
							}
						}
					}
					else
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(text, tOC.transform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					}
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(text, vector, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
				break;
			}
			}
			return false;
		}

		private bool CTRL_SWITCH_SHOWPAD(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			Transform canvasUI = MonoBehaviourSingleton<UIManager>.Instance.CanvasUI;
			if (canvasUI == null)
			{
				return false;
			}
			canvasUI = MonoBehaviourSingleton<UIManager>.Instance.JoystickPanelParent;
			if (canvasUI == null)
			{
				return false;
			}
			Canvas component = canvasUI.GetComponent<Canvas>();
			if (component == null)
			{
				return false;
			}
			if (tStageCtrlInsTruction.nParam1 != 0f)
			{
				component.enabled = true;
			}
			else
			{
				component.enabled = false;
			}
			return false;
		}

		private bool CTRL_SWITCH_STAGETIME(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate == null)
			{
				return false;
			}
			if (tStageCtrlInsTruction.nParam1 != 0f)
			{
				stageUpdate.gbAddStageUseTime = true;
			}
			else
			{
				stageUpdate.gbAddStageUseTime = false;
			}
			return false;
		}

		private bool CTRL_START_TIMER(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int num = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowStageStartTimer(num);
			}
			return false;
		}

		private bool CTRL_SHOW_RANGEBAR(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			switch (Mathf.RoundToInt(tStageCtrlInsTruction.fTime))
			{
			case 0:
				BattleInfoUI.Instance.ShowLRRangeBar(tStageCtrlInsTruction.nParam1, tStageCtrlInsTruction.nParam2);
				break;
			case 1:
				BattleInfoUI.Instance.ShowLRRangeBar(tStageCtrlInsTruction.nParam2, tStageCtrlInsTruction.nParam1);
				break;
			case 2:
				BattleInfoUI.Instance.ShowTBRangeBar(tStageCtrlInsTruction.nParam2, tStageCtrlInsTruction.nParam1);
				break;
			case 3:
				BattleInfoUI.Instance.ShowTBRangeBar(tStageCtrlInsTruction.nParam1, tStageCtrlInsTruction.nParam2);
				break;
			}
			return false;
		}

		private bool CTRL_OBJ_ADDBUFF(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			int nSCEID = int.Parse(array[0]);
			int num = int.Parse(array[1]);
			bool flag = array[2] == "1";
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			if (tECB != null)
			{
				if (flag)
				{
					tECB.selfBuffManager.AddBuff(num, 0, 0, 0);
				}
				else
				{
					tECB.selfBuffManager.RemoveBuff(num, false);
				}
			}
			else if (tOC != null)
			{
				if (flag)
				{
					tOC.selfBuffManager.AddBuff(num, 0, 0, 0);
				}
				else
				{
					tOC.selfBuffManager.RemoveBuff(num, false);
				}
			}
			return false;
		}

		private bool CTRL_WAIT_OBJ_SPAWN(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int nSceID = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			StartCoroutine(WaitObjSpawn(nSceID, notifyCallBack));
			return CheckLockReturn();
		}

		private bool CTRL_WAIT_ENEMYACT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack notifyCallBack = new NotifyCallBack();
			notifyCallBack.cbparam = ContinuePlay;
			notifyCallBack.nParam0 = ++nStartIndex;
			int nSCEID = Mathf.RoundToInt(tStageCtrlInsTruction.fTime);
			int mainstatus = Mathf.RoundToInt(tStageCtrlInsTruction.nParam1);
			int substatus = Mathf.RoundToInt(tStageCtrlInsTruction.nParam2);
			EnemyControllerBase tECB;
			OrangeCharacter tOC;
			StageUpdate.GetObjBySCEID(nSCEID, out tECB, out tOC);
			if (tECB != null)
			{
				StartCoroutine(WaitObjActCoroutine(tECB.gameObject, mainstatus, substatus, notifyCallBack));
			}
			else
			{
				bLockWait = false;
			}
			return CheckLockReturn();
		}

		private bool CTRL_PLAY_BGM(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			int num = int.Parse(array[2]);
			if (num <= 3)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYBGM, num, array[0], array[1]);
			}
			else if (num == BEAT_SYNC_CHANNEL)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYBGM_BEAT_SYNC, num, array[0], array[1]);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlayExceptSE(array[0], array[1]);
			}
			return false;
		}

		private bool CTRL_PLAYER_TELEPORTEFFECT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (tObj != null)
			{
				Vector3 localPosition = tObj.transform.localPosition;
				if (tStageCtrlInsTruction.nParam1 != 0f && tStageCtrlInsTruction.nParam2 != 0f)
				{
					localPosition.x = tStageCtrlInsTruction.nParam1;
					localPosition.y = tStageCtrlInsTruction.nParam2;
				}
				float fWaitTime = (StageUpdate.gbIsNetGame ? 3f : 0f);
				bool bSetLookDir = false;
				bool bLookFront = false;
				bool bTeleportWaitTime = false;
				if (tStageCtrlInsTruction.sMsg != null && tStageCtrlInsTruction.sMsg != "")
				{
					string[] array = tStageCtrlInsTruction.sMsg.Split(',');
					if (array.Length >= 2)
					{
						bSetLookDir = array[0] == "1";
						bLookFront = array[1] == "1";
						if (array.Length >= 3)
						{
							bTeleportWaitTime = array[2] == "1";
						}
					}
				}
				bLockWait = true;
				NotifyCallBack notifyCallBack = new NotifyCallBack();
				notifyCallBack.cbparam = ContinuePlay;
				notifyCallBack.nParam0 = ++nStartIndex;
				StartCoroutine(WaitTeleportCoroutine(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, localPosition, fWaitTime, notifyCallBack, true, bSetLookDir, bLookFront, bTeleportWaitTime));
			}
			return CheckLockReturn();
		}

		private bool CTRL_SHOW_BOSSHPBAR(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			if (tStageCtrlInsTruction.nParam1 == 1f)
			{
				if (BattleInfoUI.Instance != null)
				{
					BattleInfoUI.Instance.SetHiddenBossBar(false);
				}
			}
			else
			{
				BattleInfoUI.Instance.SetHiddenBossBar(true);
			}
			return false;
		}

		private bool CTRL_CLEAR_MOB_NOEVENT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			int count = StageUpdate.runEnemys.Count;
			List<StageUpdate.EnemyCtrlID> runEnemys = StageUpdate.runEnemys;
			for (int num = count - 1; num >= 0; num--)
			{
				runEnemys[num].mEnemy.NullHurtAction();
				runEnemys[num].mEnemy.BackToPool();
				StageResManager.BackObjToPool(runEnemys[num]);
				runEnemys.RemoveAt(num);
			}
			return false;
		}

		private bool CTRL_SHOW_CTRLHINT(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			bLockWait = true;
			NotifyCallBack newNCB = new NotifyCallBack();
			newNCB.cbparam = ContinuePlay;
			newNCB.nParam0 = ++nStartIndex;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CtrlHint", delegate(CtrlHintUI ui)
			{
				ui.Setup();
				ui.closeCB = newNCB.CallCB;
			});
			return CheckLockReturn();
		}

		private bool CTRL_ROOMIN_WIMPOSE(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			string[] array = tStageCtrlInsTruction.sMsg.Split(',');
			bool bCallStageEnd = false;
			bool bPlayWinPose = false;
			bool bStageClear = false;
			if (array.Length >= 3)
			{
				bCallStageEnd = GetBoolBySaveStr(array[0]);
				bPlayWinPose = GetBoolBySaveStr(array[1]);
				bStageClear = GetBoolBySaveStr(array[2]);
			}
			StartCoroutine(WaitOCStandCorotine(bCallStageEnd, bPlayWinPose, bStageClear));
			return false;
		}

		private IEnumerator WaitOCStandCorotine(bool bCallStageEnd, bool bPlayWinPose, bool bStageClear)
		{
			OrangeCharacter OC = StageUpdate.GetMainPlayerOC();
			CTRL_LOCK_INPUT(null);
			while (!OC.CheckActStatus(0, 3) || !OC.Controller.BelowInBypassRange)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			Vector3 tPos = OC.transform.position;
			tPos.y += OC.Controller.Collider2D.bounds.size.y;
			if (bPlayWinPose)
			{
				OC.ClosePadAndPlayerUI();
				OC.LockControl();
			}
			EventManager.StageCameraFocus tStageCameraFocus = new EventManager.StageCameraFocus();
			if (bStageClear)
			{
				BattleInfoUI.Instance.CheckStartStageClear(delegate
				{
					tStageCameraFocus.nMode = 1;
					tStageCameraFocus.roominpos = tPos;
					tStageCameraFocus.fRoomInTime = 1f;
					tStageCameraFocus.fRoomOutTime = -1f;
					tStageCameraFocus.fRoomInFov = 9f;
					tStageCameraFocus.bDontPlayMotion = !bPlayWinPose;
					tStageCameraFocus.bCallStageEnd = bCallStageEnd;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, tStageCameraFocus);
					OC.UseTeleportFollowCamera = true;
				});
			}
			else
			{
				tStageCameraFocus.nMode = 1;
				tStageCameraFocus.roominpos = tPos;
				tStageCameraFocus.fRoomInTime = 1f;
				tStageCameraFocus.fRoomOutTime = -1f;
				tStageCameraFocus.fRoomInFov = 9f;
				tStageCameraFocus.bDontPlayMotion = !bPlayWinPose;
				tStageCameraFocus.bCallStageEnd = bCallStageEnd;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, tStageCameraFocus);
			}
		}

		private IEnumerator WaitTeleportCoroutine(string sPlayerID, Vector3 tPos, float fWaitTime, NotifyCallBack tNCB, bool bShowEffect = false, bool bSetLookDir = false, bool bLookFront = false, bool bTeleportWaitTime = false)
		{
			StageResManager.GetStageUpdate();
			OrangeCharacter tOC3 = StageUpdate.GetPlayerByID(sPlayerID);
			if (Vector3.SqrMagnitude(tOC3.transform.localPosition - tPos) < 0.5f)
			{
				if (bTeleportWaitTime)
				{
					if (bShowEffect)
					{
						fWaitTime += 1f;
					}
					while (fWaitTime > 0f)
					{
						fWaitTime -= Time.deltaTime;
						yield return CoroutineDefine._waitForEndOfFrame;
					}
				}
				ContinuePlay(tNCB);
				yield break;
			}
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowTeleportCD(fWaitTime, tPos);
			}
			while (fWaitTime > 0f)
			{
				tOC3 = StageUpdate.GetPlayerByID(sPlayerID);
				if (tOC3 == null)
				{
					ContinuePlay(tNCB);
					yield break;
				}
				if (Vector3.SqrMagnitude(tOC3.transform.localPosition - tPos) < 0.5f)
				{
					if (BattleInfoUI.Instance != null)
					{
						BattleInfoUI.Instance.ShowTeleportCD(-1f, null);
					}
					ContinuePlay(tNCB);
					yield break;
				}
				fWaitTime -= Time.deltaTime;
				if (BattleInfoUI.Instance != null)
				{
					BattleInfoUI.Instance.ShowTeleportCD(fWaitTime, tPos);
				}
				yield return CoroutineDefine._waitForEndOfFrame;
				yield return StageUpdate.WaitGamePauseProcess();
			}
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowTeleportCD(-1f, null);
			}
			tOC3 = StageUpdate.GetPlayerByID(sPlayerID);
			if (tOC3 == null)
			{
				ContinuePlay(tNCB);
				yield break;
			}
			if (Vector3.SqrMagnitude(tOC3.transform.localPosition - tPos) < 0.5f)
			{
				ContinuePlay(tNCB);
				yield break;
			}
			if (bShowEffect)
			{
				tOC3.TeleportOut();
				while (true)
				{
					tOC3 = StageUpdate.GetPlayerByID(sPlayerID);
					if (tOC3 == null)
					{
						ContinuePlay(tNCB);
						yield break;
					}
					if (!tOC3.IsTeleporting)
					{
						break;
					}
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			yield return StageUpdate.WaitGamePauseProcess();
			EventManager.RemoveDeadAreaEvent objFromPool = StageResManager.GetObjFromPool<EventManager.RemoveDeadAreaEvent>();
			objFromPool.tOC = tOC3;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.REMOVE_DEAD_AREA_EVENT, objFromPool);
			EventManager.StageCameraFocus stageCameraFocus;
			if (tOC3.IsLocalPlayer && MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera != null)
			{
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>().StopCameraShake();
				stageCameraFocus = new EventManager.StageCameraFocus();
				stageCameraFocus.bLock = false;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			}
			tOC3.vLastMovePt = tPos;
			tOC3.vLastStandPt = tPos;
			tOC3.transform.localPosition = tPos;
			tOC3.Controller.LogicPosition = new VInt3(tPos);
			if (bSetLookDir)
			{
				tOC3.direction = (bLookFront ? 1 : (-1));
			}
			stageCameraFocus = new EventManager.StageCameraFocus();
			stageCameraFocus.bLock = true;
			stageCameraFocus.bRightNow = true;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(sSyncID, 10, tOC3.sNetSerialID + "," + tPos.x + "," + tPos.y, true);
			}
			if (bShowEffect)
			{
				yield return CoroutineDefine._0_3sec;
				tOC3 = StageUpdate.GetPlayerByID(sPlayerID);
				if (tOC3 == null)
				{
					ContinuePlay(tNCB);
					yield break;
				}
				tOC3.TeleportIn();
				while (true)
				{
					tOC3 = StageUpdate.GetPlayerByID(sPlayerID);
					if (tOC3 == null)
					{
						ContinuePlay(tNCB);
						yield break;
					}
					if (!tOC3.IsTeleporting)
					{
						break;
					}
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			ContinuePlay(tNCB);
		}

		private IEnumerator CheckNotInGameTeleportCoroutine(OrangeCharacter tOC, Vector3 tPos, float fWaitTime)
		{
			while (fWaitTime > 0f)
			{
				if (Vector3.SqrMagnitude(tOC.transform.localPosition - tPos) < 0.5f)
				{
					yield break;
				}
				fWaitTime -= Time.deltaTime;
				if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckPlayerIsInGame(tOC.sPlayerID))
				{
					break;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			tOC.transform.localPosition = tPos;
			tOC.Controller.LogicPosition = new VInt3(tPos);
		}

		private IEnumerator WaitObjSpawn(int nSceID, NotifyCallBack tNCB)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			OrangeCharacter tOC = null;
			EnemyControllerBase tEC = null;
			while (tOC == null && tEC == null)
			{
				StageUpdate.GetObjBySCEID(nSceID, out tEC, out tOC);
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (tEC != null)
			{
				while (!tEC.Controller.Collider2D.enabled)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			if (tNCB != null)
			{
				tNCB.CallCB();
			}
		}

		private IEnumerator WaitPlayerToObj(Vector3 tPos, NotifyCallBack tNCB, bool bEnemy)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			bool bCheck = true;
			while (bCheck)
			{
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					if (Vector3.Distance(tPos, StageUpdate.runPlayers[num].transform.position) <= 0.3f)
					{
						if (tNCB != null)
						{
							tNCB.CallCB();
						}
						bCheck = false;
						break;
					}
				}
				if (bEnemy && bCheck)
				{
					for (int num2 = StageUpdate.runEnemys.Count - 1; num2 >= 0; num2--)
					{
						if (Vector3.Distance(tPos, StageUpdate.runEnemys[num2].mEnemy.transform.position) <= 0.3f)
						{
							if (tNCB != null)
							{
								tNCB.CallCB();
							}
							bCheck = false;
							break;
						}
					}
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		private void WaitObjMoveToPosEndCB(RunUpdateClass tRUC)
		{
			if (tRUC.oParams[0] != null)
			{
				NotifyCallBack notifyCallBack = tRUC.oParams[4] as NotifyCallBack;
				if (notifyCallBack != null)
				{
					notifyCallBack.CallCB();
				}
			}
		}

		private void WaitObjMoveToPos(RunUpdateClass tRUC)
		{
			if (tRUC.oParams[0] != null)
			{
				OrangeCharacter orangeCharacter = tRUC.oParams[0] as OrangeCharacter;
				if ((bool)tRUC.oParams[5])
				{
					StageCtrlInsTruction stageCtrlInsTruction = new StageCtrlInsTruction();
					stageCtrlInsTruction.fTime = tRUC.fParams[0];
					stageCtrlInsTruction.nParam1 = ((Vector3)tRUC.oParams[2]).x;
					if (((Vector3)tRUC.oParams[2]).x > orangeCharacter.transform.position.x)
					{
						stageCtrlInsTruction.tStageCtrl = 20;
					}
					else
					{
						stageCtrlInsTruction.tStageCtrl = 21;
					}
					tRUC.tEndCB = (Action<RunUpdateClass>)Delegate.Combine(tRUC.tEndCB, new Action<RunUpdateClass>(WaitObjMoveToPosEndCB));
					stageCtrlInsTruction.RemoveCB = tRUC.EndCallBack;
					orangeCharacter.ObjCtrl(orangeCharacter.gameObject, stageCtrlInsTruction);
					tRUC.bIsEnd = true;
				}
				else
				{
					if (!(tRUC.fParams[0] > 0f))
					{
						return;
					}
					orangeCharacter.transform.position += (Vector3)tRUC.oParams[3] * 0.016f * 2.15f;
					orangeCharacter.Controller.LogicPosition = new VInt3(orangeCharacter.transform.localPosition);
					tRUC.fParams[0] -= 0.034400005f;
					if (tRUC.fParams[0] <= 0f)
					{
						tRUC.bIsEnd = true;
						NotifyCallBack notifyCallBack = tRUC.oParams[4] as NotifyCallBack;
						if (notifyCallBack != null)
						{
							notifyCallBack.CallCB();
						}
					}
				}
			}
			else
			{
				if (tRUC.oParams[1] == null)
				{
					return;
				}
				EnemyControllerBase enemyControllerBase = tRUC.oParams[1] as EnemyControllerBase;
				if (!(tRUC.fParams[0] > 0f))
				{
					return;
				}
				enemyControllerBase.transform.position += (Vector3)tRUC.oParams[3] * 0.016f * 2.15f;
				enemyControllerBase.Controller.LogicPosition = new VInt3(enemyControllerBase.transform.localPosition);
				tRUC.fParams[0] -= 0.034400005f;
				if (tRUC.fParams[0] <= 0f)
				{
					tRUC.bIsEnd = true;
					NotifyCallBack notifyCallBack2 = tRUC.oParams[4] as NotifyCallBack;
					if (notifyCallBack2 != null)
					{
						notifyCallBack2.CallCB();
					}
				}
			}
		}

		private IEnumerator WaitObjActCoroutine(GameObject tPlayer, int mainstatus, int substatus, NotifyCallBack tNCB)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			if (tPlayer == null)
			{
				yield break;
			}
			StageObjBase tSOB = tPlayer.GetComponent<StageObjBase>();
			if (!(tSOB == null))
			{
				while (!tSOB.CheckActStatus(mainstatus, substatus))
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				NetContinuePlay(tNCB);
			}
		}

		private IEnumerator WaitEnemyActCoroutine(int nSceID, int mainstatus, int substatus, NotifyCallBack tNCB)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			if (nSceID == 0)
			{
				yield break;
			}
			EnemyControllerBase tECB = null;
			OrangeCharacter tOC = null;
			StageUpdate.GetObjBySCEID(nSceID, out tECB, out tOC);
			if (!(tECB == null))
			{
				while (!tOC.CheckActStatusEvt(mainstatus, substatus))
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				NetContinuePlay(tNCB);
			}
		}

		private IEnumerator WaitBulletInCoroutine(NotifyCallBack tNCB)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			bool bBreak = false;
			while (true)
			{
				Vector3 max = EventB2D.bounds.max;
				Vector3 min = EventB2D.bounds.min;
				for (int num = StageUpdate.runBulletSets.Count - 1; num >= 0; num--)
				{
					Vector3 position = StageUpdate.runBulletSets[num].transform.position;
					if (max.x > position.x && max.y > position.y && min.x < position.x && min.y < position.y)
					{
						bBreak = true;
						break;
					}
				}
				if (bBreak)
				{
					break;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			NetContinuePlay(tNCB);
		}

		private IEnumerator MoveFollowCameraCoroutine(Vector3 tPos, GameObject tPlayer)
		{
			while (Vector3.SqrMagnitude(tPlayer.transform.position - tPos) < 0.05f)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
			stageCameraFocus.bLock = true;
			stageCameraFocus.bRightNow = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
		}

		private IEnumerator UpdateMoveCameraCC(RunUpdateClass tRUC)
		{
			Transform mainCameraTransform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
			while (true)
			{
				Vector3 position = mainCameraTransform.position;
				if (tRUC.fParams[3] != 0f)
				{
					float num = tRUC.fParams[1] * Time.deltaTime;
					if (Mathf.Abs(tRUC.fParams[3]) > Mathf.Abs(num))
					{
						position.x += num;
						tRUC.fParams[3] -= num;
					}
					else
					{
						position.x += tRUC.fParams[3];
						tRUC.fParams[3] = 0f;
					}
				}
				if (tRUC.fParams[4] != 0f)
				{
					float num = tRUC.fParams[2] * Time.deltaTime;
					if (Mathf.Abs(tRUC.fParams[4]) > Mathf.Abs(num))
					{
						position.y += num;
						tRUC.fParams[4] -= num;
					}
					else
					{
						position.y += tRUC.fParams[4];
						tRUC.fParams[4] = 0f;
					}
				}
				mainCameraTransform.position = position;
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		private void UpdateMoveCamera(RunUpdateClass tRUC)
		{
			Transform transform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
			if (tRUC.fParams[0] > 0f)
			{
				if (tRUC.fParams[0] > 0.016f)
				{
					tRUC.fParams[0] -= 0.016f;
					tRUC.fParams[3] = tRUC.fParams[3] + tRUC.fParams[1] * 0.016f;
					tRUC.fParams[4] = tRUC.fParams[4] + tRUC.fParams[2] * 0.016f;
					return;
				}
				Vector3 position = transform.position;
				transform.position = new Vector3((tRUC.oParams[0] as StageCtrlInsTruction).nParam1, (tRUC.oParams[0] as StageCtrlInsTruction).nParam2, position.z);
				tRUC.bIsEnd = true;
				tRUC.fParams[0] = 0f;
				if (tRUC.oParams[1] != null)
				{
					StopCoroutine((Coroutine)tRUC.oParams[1]);
				}
				tRUC.oParams[1] = null;
			}
			else
			{
				Vector3 position2 = transform.position;
				transform.position = new Vector3((tRUC.oParams[0] as StageCtrlInsTruction).nParam1, (tRUC.oParams[0] as StageCtrlInsTruction).nParam2, position2.z);
				tRUC.bIsEnd = true;
				if (tRUC.oParams[1] != null)
				{
					StopCoroutine((Coroutine)tRUC.oParams[1]);
				}
				tRUC.oParams[1] = null;
			}
		}

		public void NetContinuePlay(NotifyCallBack tNCB)
		{
			StageUpdate.SyncStageObj(sSyncID, 6, tNCB.nParam0.ToString(), true);
			ContinuePlay(tNCB);
		}

		public void ContinuePlay(NotifyCallBack tNCB)
		{
			if (tNCB.nParam0 == nStartIndex)
			{
				bLockWait = false;
				if (bNeedResumTimer)
				{
					bNeedResumTimer = false;
					StageResManager.GetStageUpdate().gbAddStageUseTime = true;
				}
			}
		}

		private void OpenNetPauseCommonUI()
		{
			if (!(tPauseCommonUI == null))
			{
				return;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				if (tPauseCommonUI != null)
				{
					tPauseCommonUI.OnClickCloseBtn();
				}
				tPauseCommonUI = ui;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_UNSTABLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), OnClosePauseCommonUI);
			});
		}

		private void OnClosePauseCommonUI()
		{
			tPauseCommonUI = null;
		}

		private IEnumerator NetWaitContinuePlay(int nWaitIndex)
		{
			while (nStartIndex < nWaitIndex)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			NotifyCallBack tNCB = new NotifyCallBack();
			while (nStartIndex == nWaitIndex && bLockWait && nStartIndex < StageCtrlInsTructions.Count)
			{
				int tStageCtrl = StageCtrlInsTructions[nStartIndex - 1].tStageCtrl;
				tNCB.nParam0 = nStartIndex;
				ContinuePlay(tNCB);
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		public void CheckSend()
		{
			bSendSuccess = true;
		}

		private IEnumerator ReSentCoroutine(StageCtrlInsTruction tStageCtrlInsTruction)
		{
			NotifyCallBack newNCB = new NotifyCallBack();
			NotifyCallBack newNCB2 = new NotifyCallBack();
			newNCB.cbparam = ContinuePlay;
			newNCB.nParam0 = nStartIndex;
			newNCB2.cb = CheckSend;
			float fCheckStartTime = Time.realtimeSinceStartup;
			while (!bSendSuccess)
			{
				if (Time.realtimeSinceStartup - fCheckStartTime > 10f)
				{
					newNCB.CallCB();
					break;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_OBJ_CTRL_PLAYSHOWANI, newNCB, newNCB2);
			}
			StageUpdate.CheckAndRemoveEnemy();
		}

		private IEnumerator ContinuePlayTimeOut(NotifyCallBack tNotifyCallBack)
		{
			float fCheckStartTime = Time.realtimeSinceStartup;
			while (!(Time.realtimeSinceStartup - fCheckStartTime > 10f))
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			tNotifyCallBack.CallCB();
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.6f, 0.5f, 0.3f);
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		public override int GetTypeID()
		{
			return 12;
		}

		public override string GetTypeString()
		{
			return StageObjType.STAGECTRL_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			string text = JsonConvert.SerializeObject(new StageCtrlDataSL
			{
				nSetID = nSetID,
				B2DX = EventB2D.offset.x,
				B2DY = EventB2D.offset.y,
				B2DW = EventB2D.size.x,
				B2DH = EventB2D.size.y,
				bUB2D = bUseBoxCollider2D,
				bDDF = bDifficultDepend,
				nDF = nDifficultSet,
				bNoUnlockRange = bNoUnlockRange,
				StageCtrlInsTructions = StageCtrlInsTructions.ToArray()
			}, Formatting.None, JsonHelper.IgnoreLoopSetting());
			text = text.Replace(",", ";");
			return typeString + text;
		}

		public override void LoadByString(string sLoad)
		{
			StageCtrlDataSL stageCtrlDataSL = JsonUtility.FromJson<StageCtrlDataSL>(sLoad.Substring(GetTypeString().Length).Replace(";", ","));
			for (int i = 0; i < stageCtrlDataSL.StageCtrlInsTructions.Length; i++)
			{
				StageCtrlInsTructions.Add(stageCtrlDataSL.StageCtrlInsTructions[i]);
				if (stageCtrlDataSL.StageCtrlInsTructions[i].tStageCtrl == 1)
				{
					bHasLockPlayerInput = true;
				}
			}
			nSetID = stageCtrlDataSL.nSetID;
			EventB2D = GetComponent<BoxCollider2D>();
			EventB2D.offset = new Vector2(stageCtrlDataSL.B2DX, stageCtrlDataSL.B2DY);
			EventB2D.size = new Vector2(stageCtrlDataSL.B2DW, stageCtrlDataSL.B2DH);
			bUseBoxCollider2D = stageCtrlDataSL.bUB2D;
			bDifficultDepend = stageCtrlDataSL.bDDF;
			nDifficultSet = stageCtrlDataSL.nDF;
			bNoUnlockRange = stageCtrlDataSL.bNoUnlockRange;
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			for (int j = 0; j < stageCtrlDataSL.StageCtrlInsTructions.Length; j++)
			{
				if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 47)
				{
					StageResManager.LoadBullet(int.Parse(stageCtrlDataSL.StageCtrlInsTructions[j].sMsg.Split(',')[4]));
				}
				else if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 42)
				{
					StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
					stageUpdate.AddSubLoadAB(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("model/animator/" + stageCtrlDataSL.StageCtrlInsTructions[j].sMsg, stageCtrlDataSL.StageCtrlInsTructions[j].sMsg, loadCallBackObj.LoadCB);
				}
				else if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 58)
				{
					StageResManager.LoadBuff(Mathf.RoundToInt(stageCtrlDataSL.StageCtrlInsTructions[j].nParam2));
				}
				else if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 52)
				{
					string[] array = stageCtrlDataSL.StageCtrlInsTructions[j].sMsg.Split(',');
					if ("ShowExplodeWhite" != array[0] && "CloseExplodeWhite" != array[0] && "ShowExplodeBlack" != array[0] && "CloseExplodeBlack" != array[0])
					{
						StageResManager.LoadFx(array[0]);
					}
				}
				else if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 61)
				{
					string[] array2 = stageCtrlDataSL.StageCtrlInsTructions[j].sMsg.Split(',');
					int p_channel = int.Parse(array2[2]);
					MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(array2[0], p_channel);
				}
				else if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 33)
				{
					int num = Mathf.RoundToInt(stageCtrlDataSL.StageCtrlInsTructions[j].nParam1);
					TUTORIAL_TABLE value;
					while (ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(num, out value))
					{
						if (value.n_SCENARIO != 0)
						{
							StageResManager.LoadScenario(value.n_SCENARIO);
						}
						if (value.n_SAVE == -1)
						{
							break;
						}
						num = TurtorialUI.GetNextID(num);
					}
				}
				else if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 16)
				{
					StageResManager.LoadScenario((int)stageCtrlDataSL.StageCtrlInsTructions[j].nParam1);
				}
				else if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 36)
				{
					string text = stageCtrlDataSL.StageCtrlInsTructions[j].sMsg.Split(',')[0];
					StageResManager.LoadObject("dragonbones/" + text, text);
				}
				else if (stageCtrlDataSL.StageCtrlInsTructions[j].tStageCtrl == 50)
				{
					StageResManager.GetStageUpdate().bIsHaveEventStageEnd = true;
				}
			}
		}

		public override bool IsNeedClip()
		{
			return false;
		}

		private IEnumerator WaitPlayerLocking(OrangeCharacter tOC)
		{
			while (!tOC.EventLockInputing)
			{
				if (StageUpdate.bIsHost)
				{
					yield break;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (!StageUpdate.bIsHost)
			{
				StageUpdate stageUpdate = StageResManager.GetStageUpdate();
				if (!(stageUpdate != null) || !stageUpdate.IsEnd)
				{
					WaitPlayerNoCommand(tOC);
				}
			}
		}

		public override void SyncNowStatus()
		{
			if (!bCheck)
			{
				string smsg = nStartIndex + "," + fLeftTime + "," + GetBoolSaveStr(bStartEvent) + "," + GetBoolSaveStr(bCheck);
				StageUpdate.SyncStageObj(sSyncID, 8, smsg, true);
			}
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
			base.OnSyncStageObj(sIDKey, nKey1, smsg);
			switch (nKey1)
			{
			case 9:
			{
				string[] array4 = smsg.Split(',');
				sTriggerPlayerID = array4[1];
				if (listLockPlayer.Count > 0)
				{
					for (int num = listLockPlayer.Count - 1; num >= 0; num--)
					{
						if (listLockPlayer[num].sPlayerID != sTriggerPlayerID)
						{
							listLockPlayer[num].EventLockInputing = false;
						}
						else
						{
							WaitPlayerNoCommand(listLockPlayer[num]);
						}
					}
					listLockPlayer.Clear();
				}
				if (!bCheck)
				{
					break;
				}
				for (int num2 = StageUpdate.runPlayers.Count - 1; num2 >= 0; num2--)
				{
					if (StageUpdate.runPlayers[num2].sPlayerID == sTriggerPlayerID)
					{
						if (bHasLockPlayerInput)
						{
							StartCoroutine(WaitPlayerLocking(StageUpdate.runPlayers[num2]));
						}
						else
						{
							WaitPlayerNoCommand(StageUpdate.runPlayers[num2]);
						}
					}
				}
				break;
			}
			case 3:
			{
				string[] array2 = smsg.Split(',');
				for (int i = 0; i < array2.Length - 1; i++)
				{
					int result;
					if (int.TryParse(array2[i], out result) && result > nStartIndex)
					{
						StageCtrlInsTruction stageCtrlInsTruction = StageCtrlInsTructions[result];
						if (stageCtrlInsTruction.tStageCtrl == 25)
						{
							BattleInfoUI.Instance.FullBossBarHP((int)stageCtrlInsTruction.nParam1);
						}
					}
				}
				break;
			}
			case 6:
			{
				int result2;
				if (int.TryParse(smsg, out result2) && (result2 > nStartIndex || (result2 == nStartIndex && bLockWait)))
				{
					StartCoroutine(NetWaitContinuePlay(result2));
				}
				break;
			}
			case 7:
			{
				string[] array5 = smsg.Split(',');
				sTriggerPlayerID = array5[0];
				tObj = null;
				for (int num3 = StageUpdate.runPlayers.Count - 1; num3 >= 0; num3--)
				{
					if (StageUpdate.runPlayers[num3].sPlayerID == sTriggerPlayerID)
					{
						if (StageUpdate.gbIsNetGame && bHasLockPlayerInput)
						{
							StageUpdate.runPlayers[num3].StopPlayer();
							StageUpdate.runPlayers[num3].EventLockInputing = true;
						}
						tObj = StageUpdate.runPlayers[num3].gameObject;
						WaitPlayerNoCommand(StageUpdate.runPlayers[num3]);
						break;
					}
				}
				break;
			}
			case 8:
			{
				string[] array3 = smsg.Split(',');
				StopAllCoroutines();
				if (bCheck || !bStartEvent)
				{
					nStartIndex = int.Parse(array3[0]);
					fLeftTime = float.Parse(array3[1]);
					bStartEvent = GetBoolBySaveStr(array3[2]);
					bCheck = GetBoolBySaveStr(array3[3]);
				}
				break;
			}
			case 10:
			{
				string[] array = smsg.Split(',');
				OrangeCharacter playerByID = StageUpdate.GetPlayerByID(array[0]);
				if (playerByID != null)
				{
					Vector3 vector = default(Vector3);
					vector.x = float.Parse(array[1]);
					vector.y = float.Parse(array[2]);
					playerByID.vLastMovePt = vector;
					playerByID.transform.localPosition = vector;
					playerByID.Controller.LogicPosition = new VInt3(vector);
				}
				break;
			}
			}
		}
	}
}
