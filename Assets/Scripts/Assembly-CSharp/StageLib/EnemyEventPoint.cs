#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class EnemyEventPoint : EventPointBase
	{
		[Serializable]
		public class EnemySpawnData
		{
			public float fTime;

			public int nID;

			public int nDeadID;

			public bool bBack;

			public int nPosID;

			public int nSCEID;

			public int nStickWall;

			public float fAiRange;

			public float fAiRangeY;

			public float fOffsetX;

			public float fOffsetY;

			public List<int> lsDlItm = new List<int>();

			public int nPatrolId;

			public int nEventCtrlID;

			public bool bIsLoop;

			public int nMoveSpeed;

			public List<Vector3> lsPatrolPath = new List<Vector3>();

			public int nSummonEventId = 999;

			public int nStageCustomType;

			public int[] nStageCustomParams = new int[3];

			public float nIntParam;

			[NonSerialized]
			public EnemyControllerBase targetEnemy;

			[NonSerialized]
			public string sNetSerialID = "";

			[NonSerialized]
			public int nStartIndex = -1;

			public EnemySpawnData(float time, int id, int deadID, int pid)
			{
				fTime = time;
				nID = id;
				nDeadID = deadID;
				nPosID = pid;
			}
		}

		[Serializable]
		public class GroupSpawnData
		{
			public List<EnemySpawnData> mEnemySpawnDatas = new List<EnemySpawnData>();

			public int nDeadID = -1;

			[NonSerialized]
			public int nEnemyStartIndex;

			public void SortByTime()
			{
				int count = mEnemySpawnDatas.Count;
				bool flag = false;
				for (int i = 0; i < count; i++)
				{
					flag = false;
					for (int j = 1; j < count - i; j++)
					{
						if (mEnemySpawnDatas[j - 1].fTime > mEnemySpawnDatas[j].fTime)
						{
							EnemySpawnData value = mEnemySpawnDatas[j - 1];
							mEnemySpawnDatas[j - 1] = mEnemySpawnDatas[j];
							mEnemySpawnDatas[j] = value;
							flag = true;
						}
					}
					if (!flag)
					{
						break;
					}
				}
			}
		}

		public class GroupSpawnSL
		{
			public bool bRunAtStart;

			public bool bUnLockRange = true;

			public int nEndEventID;

			public int nEndParam1;

			public int nSetID;

			public bool bDead;

			[JsonConverter(typeof(FloatConverter))]
			public float fDTime;

			public string sSave = "";

			[JsonConverter(typeof(FloatConverter))]
			public float B2DX;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DY;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DW = 1f;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DH = 1f;

			public List<Vector3> SpawnPoints = new List<Vector3>();

			public List<GroupSpawnData> SpawnGroups = new List<GroupSpawnData>();
		}

		private enum SYNC_PATTERN
		{
			PATTERN_START = 0,
			PATTERN_SPAWN = 1,
			PATTERN_DEAD = 2,
			PATTERN_ENDEVENT = 3,
			PATTERN_NOTIFY = 4,
			PATTERN_RECONNECT = 5,
			PATTERN_REBORN = 6,
			PATTERN_RESET = 7,
			PATTERN_REBORNON = 8
		}

		public PosShowSet SpawnPoints = new PosShowSet();

		public List<GroupSpawnData> SpawnGroups = new List<GroupSpawnData>();

		public bool bRunAtStart;

		public bool bUnLockRange = true;

		public int nEndEventID;

		public int nEndParam1;

		public int nowEditGroupID = -1;

		public bool bDead;

		public float fDTime;

		public bool bShowEnemyHint;

		public bool bCanCallReRun;

		public int nUnlockRangeID;

		public int nStartEventID;

		public bool bCheckBornInB2D;

		private bool _bStartEvent;

		private float _nowEventTime;

		private float _nextTime;

		private int _nowEventGroup;

		private GroupSpawnData targetGroupSpawnData;

		private int _nCount;

		private int _nCount2;

		private int _nowStartIndex;

		private int _nEnemyCount;

		private List<EnemySpawnData> _inGameEnemy = new List<EnemySpawnData>();

		private List<string> _listNetDeadNetSerialIDs = new List<string>();

		private List<EnemyControllerBase> _deadPlayEnemy = new List<EnemyControllerBase>();

		private int _summonState;

		private bool _bRebornFlag;

		private int _nowRebornIndex;

		private float _nowRebornTime;

		private float _nextRebornTime;

		private List<int> _listNonRebornPoint = new List<int>();

		private string _sTriggerNetID = "";

		private Vector3 tNowPos;

		private Vector3 tLastPos;

		private Vector3 vMax;

		private Vector3 vMin;

		private void Awake()
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
		}

		private void OnDestroy()
		{
			ClearEnemy();
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
		}

		public override void Init()
		{
		}

		protected override void UpdateEvent()
		{
			if (bCheck && bUseBoxCollider2D)
			{
				vMax = EventB2D.bounds.max;
				vMin = EventB2D.bounds.min;
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.runPlayers[num].UsingVehicle)
					{
						RideArmorController component = StageUpdate.runPlayers[num].transform.root.GetComponent<RideArmorController>();
						if (!component.MasterPilot.bIsNpcCpy)
						{
							tNowPos = component.transform.position;
							if (vMax.x > tNowPos.x && vMax.y > tNowPos.y && vMin.x < tNowPos.x && vMin.y < tNowPos.y)
							{
								OnEvent(component.transform);
								if (!bCheck)
								{
									return;
								}
							}
						}
					}
					else if (!StageUpdate.runPlayers[num].bIsNpcCpy)
					{
						tLastPos = StageUpdate.runPlayers[num].vLastMovePt;
						tNowPos = StageUpdate.runPlayers[num].transform.position;
						if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
						{
							if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
							{
								OnEvent(StageUpdate.runPlayers[num].transform);
								if (!bCheck)
								{
									return;
								}
							}
							else if ((tLastPos.y < vMin.y && tNowPos.y > vMax.y) || (tLastPos.y > vMax.y && tNowPos.y < vMin.y))
							{
								OnEvent(StageUpdate.runPlayers[num].transform);
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
								OnEvent(StageUpdate.runPlayers[num].transform);
								if (!bCheck)
								{
									return;
								}
							}
							else if ((tLastPos.x < vMin.x && tNowPos.x > vMax.x) || (tLastPos.x > vMax.x && tNowPos.x < vMin.x))
							{
								OnEvent(StageUpdate.runPlayers[num].transform);
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

		private void ClearEnemy()
		{
			for (int num = SpawnGroups.Count - 1; num >= 0; num--)
			{
				for (int num2 = SpawnGroups[num].mEnemySpawnDatas.Count - 1; num2 >= 0; num2--)
				{
					if (SpawnGroups[num].mEnemySpawnDatas[num2].targetEnemy != null)
					{
						StageUpdate.RemoveEnemy(SpawnGroups[num].mEnemySpawnDatas[num2].targetEnemy);
						SpawnGroups[num].mEnemySpawnDatas[num2].targetEnemy = null;
					}
				}
			}
		}

		public bool CheckShowBattleing()
		{
			if (nUnlockRangeID != 0 && !CheckUnLockRangeTriggerID())
			{
				return false;
			}
			if (bUnLockRange || nEndEventID == 17)
			{
				return true;
			}
			if (nEndEventID == 26)
			{
				for (int num = StageUpdate.listAllEvent.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.listAllEvent[num].nSetID == nEndParam1)
					{
						EnemyEventPoint enemyEventPoint = StageUpdate.listAllEvent[num] as EnemyEventPoint;
						if (enemyEventPoint != null && enemyEventPoint.CheckShowBattleing())
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private bool CheckUnLockRangeTriggerID()
		{
			for (int num = StageUpdate.listAllEvent.Count - 1; num >= 0; num--)
			{
				if (StageUpdate.listAllEvent[num].nSetID == nUnlockRangeID && StageUpdate.listAllEvent[num].GetTypeID() == 7)
				{
					(StageUpdate.listAllEvent[num] as LockRangeEvent).CheckTriggerPlayer();
					return true;
				}
			}
			return false;
		}

		public IEnumerator ReStartEvent(bool bNextGroup = false)
		{
			_nowEventTime = 0f;
			if (bNextGroup)
			{
				_nowEventGroup++;
			}
			else
			{
				_nowEventGroup = 0;
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(sSyncID, 0, _nowEventGroup + "," + _sTriggerNetID, true);
					yield return new WaitForSecondsRealtime(MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime);
				}
				if (_bStartEvent)
				{
					yield break;
				}
				TriggerStartEventCall();
				if (CheckShowBattleing())
				{
					EventManager.BattleInfoUpdate battleInfoUpdate = new EventManager.BattleInfoUpdate();
					battleInfoUpdate.nType = 1;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_INFO_UPDATE, battleInfoUpdate);
				}
			}
			_nowStartIndex = 0;
			if (_nowEventGroup < SpawnGroups.Count)
			{
				InitSpawnData();
				_bStartEvent = true;
				yield break;
			}
			_bStartEvent = false;
			StageUpdate.SyncStageObj(sSyncID, 3, nEndEventID.ToString(), true);
			TriggerUnLockRange();
			TriggerEndEventCall();
			_listNetDeadNetSerialIDs.Clear();
		}

		private void InitSpawnData()
		{
			if (_nowEventGroup >= SpawnGroups.Count)
			{
				return;
			}
			targetGroupSpawnData = SpawnGroups[_nowEventGroup];
			if (targetGroupSpawnData.nDeadID != -1)
			{
				_nEnemyCount = 0;
				for (int num = targetGroupSpawnData.mEnemySpawnDatas.Count - 1; num >= 0; num--)
				{
					if (targetGroupSpawnData.nDeadID == targetGroupSpawnData.mEnemySpawnDatas[num].nDeadID)
					{
						_nEnemyCount++;
					}
				}
			}
			else
			{
				_nEnemyCount = targetGroupSpawnData.mEnemySpawnDatas.Count;
			}
		}

		public void AddOneSpawnPoint(Vector3? setPos = null)
		{
			SpawnPoints.AddShowObj(base.gameObject, "生怪點", new Color(0.3f, 0.6f, 0.3f), setPos);
		}

		public void CallOnByID(EventManager.StageEventCall tStageEventCall)
		{
			int nID = tStageEventCall.nID;
			if (nID == 0)
			{
				STAGE_EVENT nStageEvent = tStageEventCall.nStageEvent;
				if (nStageEvent == STAGE_EVENT.STAGE_ENEMYEVENT_STOP && _summonState > 0)
				{
					_summonState = 2;
				}
			}
			else
			{
				if (nSetID != nID)
				{
					return;
				}
				Transform tTransform = tStageEventCall.tTransform;
				if (!base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(true);
				}
				if (tStageEventCall.nStageEvent == STAGE_EVENT.STAGE_START_SUMMON_ENEMY)
				{
					_summonState = 1;
				}
				if (bCanCallReRun && !bCheck)
				{
					if (!_bStartEvent)
					{
						if (_nowEventGroup >= SpawnGroups.Count)
						{
							bCheck = true;
							ClearInGameEnemy();
						}
					}
					else if (_nowStartIndex >= _nCount && !_bRebornFlag)
					{
						_nowRebornIndex = 0;
						_nowRebornTime = 0f;
						_nextRebornTime = 0f;
						_listNetDeadNetSerialIDs.Clear();
						_listNonRebornPoint.Clear();
						for (int i = 0; i < _inGameEnemy.Count; i++)
						{
							if (!_listNonRebornPoint.Contains(_inGameEnemy[i].nPosID))
							{
								_listNonRebornPoint.Add(_inGameEnemy[i].nPosID);
							}
						}
						while (_nowRebornIndex < targetGroupSpawnData.mEnemySpawnDatas.Count)
						{
							EnemySpawnData enemySpawnData = targetGroupSpawnData.mEnemySpawnDatas[_nowRebornIndex];
							if (!_listNonRebornPoint.Contains(enemySpawnData.nPosID))
							{
								_bRebornFlag = true;
								break;
							}
							_nowRebornTime = enemySpawnData.fTime;
							_nowRebornIndex++;
						}
						string text = _listNonRebornPoint.Count.ToString();
						for (int j = 0; j < _listNonRebornPoint.Count; j++)
						{
							text = text + "," + _listNonRebornPoint[j];
						}
						StageUpdate.SyncStageObj(sSyncID, 8, _nowEventGroup + "," + _nowRebornIndex + "," + text, true);
					}
				}
				OnEvent(tTransform);
			}
		}

		public override void OnEvent(Transform TriggerTransform)
		{
			if (!bCheck)
			{
				return;
			}
			if (TriggerTransform == null)
			{
				Debug.LogError("TriggerTransform is null.");
				return;
			}
			StageObjBase component = TriggerTransform.GetComponent<StageObjBase>();
			if (component == null)
			{
				Debug.LogError("TriggerTransform has no StageObjBase Component.");
				return;
			}
			bCheck = false;
			_sTriggerNetID = component.sNetSerialID;
			_bRebornFlag = false;
			UpdateCall = OnLateUpdate;
			StartCoroutine(ReStartEvent());
		}

		private void CheckDeadToEnemyCount(EnemySpawnData tEnemySpawnData)
		{
			if (targetGroupSpawnData.nDeadID != -1)
			{
				if (targetGroupSpawnData.nDeadID == tEnemySpawnData.nDeadID)
				{
					_nEnemyCount--;
				}
			}
			else
			{
				_nEnemyCount--;
			}
		}

		private void CheckDeadToEnemyCount(int nDeadID)
		{
			if (targetGroupSpawnData.nDeadID != -1)
			{
				if (targetGroupSpawnData.nDeadID == nDeadID)
				{
					_nEnemyCount--;
				}
			}
			else
			{
				_nEnemyCount--;
			}
		}

		private void AddOutDead(EnemyControllerBase tEnemyControllerBase)
		{
			if (nUnlockRangeID != 0)
			{
				for (int num = StageUpdate.listAllEvent.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.listAllEvent[num].nSetID == nUnlockRangeID && StageUpdate.listAllEvent[num].GetTypeID() == 7)
					{
						if ((StageUpdate.listAllEvent[num] as LockRangeEvent).CheckTriggerPlayer())
						{
							tEnemyControllerBase.gameObject.AddComponent<StageOutDead>().fDeadTime = fDTime;
						}
						return;
					}
				}
			}
			tEnemyControllerBase.gameObject.AddComponent<StageOutDead>().fDeadTime = fDTime;
		}

		public override void OnLateUpdate()
		{
			if (bRunAtStart)
			{
				_sTriggerNetID = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
				UpdateCall = OnLateUpdate;
				StartCoroutine(ReStartEvent());
				bCheck = false;
				bRunAtStart = false;
			}
			if (!_bStartEvent)
			{
				return;
			}
			if (_summonState == 2)
			{
				UpdateEnemyDead();
				return;
			}
			if (bShowEnemyHint && BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowEnemyHint();
			}
			_nextTime = _nowEventTime + 0.016f;
			_nCount = targetGroupSpawnData.mEnemySpawnDatas.Count;
			bool flag = false;
			while (_nowStartIndex < _nCount)
			{
				EnemySpawnData enemySpawnData = targetGroupSpawnData.mEnemySpawnDatas[_nowStartIndex];
				_bRebornFlag = false;
				if ((!(_nowEventTime <= enemySpawnData.fTime) || !(_nextTime > enemySpawnData.fTime)) && enemySpawnData.fTime != 0f)
				{
					break;
				}
				string text = sSyncID + (targetGroupSpawnData.nEnemyStartIndex + _nowStartIndex);
				if (_listNetDeadNetSerialIDs.Contains(text))
				{
					CheckDeadToEnemyCount(enemySpawnData);
				}
				else if (SpawnEnemy(enemySpawnData, text, _nowStartIndex, false, true))
				{
					flag = true;
				}
				_nowStartIndex++;
			}
			if (flag)
			{
				StageUpdate.SyncStageObj(sSyncID, 1, _nowEventGroup + "," + (_nowStartIndex - 1), true);
			}
			_nowEventTime = _nextTime;
			if (_bRebornFlag)
			{
				flag = false;
				for (_nextRebornTime = _nowRebornTime + 0.016f; _nowRebornIndex < _nCount; _nowRebornIndex++)
				{
					EnemySpawnData enemySpawnData2 = targetGroupSpawnData.mEnemySpawnDatas[_nowRebornIndex];
					if ((!(_nowRebornTime <= enemySpawnData2.fTime) || !(_nextRebornTime > enemySpawnData2.fTime)) && enemySpawnData2.fTime != 0f)
					{
						break;
					}
					if (!_listNonRebornPoint.Contains(enemySpawnData2.nPosID))
					{
						string sTmpSerialID = sSyncID + (targetGroupSpawnData.nEnemyStartIndex + _nowRebornIndex);
						if (SpawnEnemy(enemySpawnData2, sTmpSerialID, _nowRebornIndex))
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					StageUpdate.SyncStageObj(sSyncID, 6, _nowEventGroup + "," + (_nowRebornIndex - 1), true);
				}
				_nowRebornTime = _nextRebornTime;
				if (_nowRebornIndex == _nCount)
				{
					_bRebornFlag = false;
				}
			}
			UpdateEnemyDead();
			if (_nEnemyCount <= 0 && _deadPlayEnemy.Count <= 0)
			{
				_bStartEvent = false;
				StartCoroutine(ReStartEvent(true));
			}
		}

		private void UpdateEnemyDead()
		{
			if (_deadPlayEnemy.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < _deadPlayEnemy.Count; i++)
			{
				if (!_deadPlayEnemy[i] || _deadPlayEnemy[i].DeadPlayCompleted)
				{
					StageUpdate.RemoveEnemy(_deadPlayEnemy[i]);
					_deadPlayEnemy.RemoveAt(i);
					i--;
				}
			}
		}

		private void CheckEnemyPosition(EnemyControllerBase tECB, Vector3 tPosition, int nStickWall, bool bBack)
		{
			if (nStickWall != 0)
			{
				float num = 9999f;
				Vector2[] array = new Vector2[4]
				{
					Vector2.down,
					Vector2.up,
					Vector2.left,
					Vector2.right
				};
				float z = 0f;
				float[] array2 = new float[4] { 0f, 180f, 270f, 90f };
				Vector3 vector = tPosition;
				for (int i = 0; i < 4; i++)
				{
					if ((nStickWall & (1 << i)) == 0)
					{
						continue;
					}
					RaycastHit2D[] array3 = Physics2D.RaycastAll(vector, array[i], float.PositiveInfinity, LayerMask.GetMask("Block", "SemiBlock"));
					for (int j = 0; j < array3.Length; j++)
					{
						RaycastHit2D raycastHit2D = array3[j];
						if (raycastHit2D.transform != tECB.Controller.Collider2D.transform && raycastHit2D.distance < num)
						{
							tPosition = raycastHit2D.point;
							num = raycastHit2D.distance;
							z = array2[i];
						}
					}
				}
				tECB.SetPositionAndRotation(tPosition, bBack);
				if (bBack)
				{
					tECB.transform.localRotation = tECB.transform.localRotation * Quaternion.Euler(0f, 0f, z);
				}
				else
				{
					tECB.transform.localRotation = tECB.transform.localRotation * Quaternion.Euler(0f, 0f, z);
				}
			}
			else
			{
				tECB.SetPositionAndRotation(tPosition, bBack);
			}
		}

		private void CheckPatrolPath(EnemyControllerBase tECB, EnemySpawnData tSpawnData)
		{
			if (tSpawnData.nPatrolId != 0)
			{
				tECB.SetPatrolPath(tSpawnData.bIsLoop, tSpawnData.nMoveSpeed, tSpawnData.lsPatrolPath.ToArray());
			}
		}

		private void CheckEventCtrlID(EnemyControllerBase tECB, EnemySpawnData tSpawnData)
		{
			if (tSpawnData.nEventCtrlID != 0)
			{
				tECB.SetEventCtrlID(tSpawnData.nEventCtrlID);
			}
		}

		private void CheckIntParameter(EnemyControllerBase tECB, EnemySpawnData tSpawnData)
		{
			if (tSpawnData.nIntParam != 0f)
			{
				tECB.SetFloatParameter(tSpawnData.nIntParam);
			}
		}

		private void SetEnemyDead(int i)
		{
			CheckDeadToEnemyCount(_inGameEnemy[i]);
			StageOutDead component = _inGameEnemy[i].targetEnemy.GetComponent<StageOutDead>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			_inGameEnemy[i].targetEnemy.HurtActions -= HurtCB;
			if (((uint)_inGameEnemy[i].nStickWall & 0x40u) != 0)
			{
				int num = 0;
				for (int j = 0; j < _inGameEnemy[i].lsDlItm.Count; j += 2)
				{
					num += _inGameEnemy[i].lsDlItm[j + 1];
				}
				num = OrangeBattleUtility.Random(0, num);
				for (int k = 0; k < _inGameEnemy[i].lsDlItm.Count; k += 2)
				{
					if (_inGameEnemy[i].lsDlItm[k + 1] > num)
					{
						StageResManager.LoadStageItemModel(_inGameEnemy[i].lsDlItm[k], null, _inGameEnemy[i].targetEnemy.Controller.Collider2D.bounds.center);
						break;
					}
				}
			}
			if (!_inGameEnemy[i].targetEnemy.DeadPlayCompleted)
			{
				_deadPlayEnemy.Add(_inGameEnemy[i].targetEnemy);
			}
			if (ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(_inGameEnemy[i].targetEnemy.EnemyData) && _inGameEnemy.Count == 1)
			{
				bool flag = true;
				foreach (EventPointBase item in StageUpdate.listAllEvent)
				{
					if (item.GetTypeID() == 8 && item.bCheckAble)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					BattleInfoUI.Instance.SwitchOptionBtn(false);
				}
			}
			if (_inGameEnemy[i].targetEnemy.DeadPlayCompleted)
			{
				StageUpdate.RemoveEnemy(_inGameEnemy[i].targetEnemy);
			}
			_inGameEnemy[i].targetEnemy = null;
			_inGameEnemy.RemoveAt(i);
		}

		public void HurtCB(StageObjBase tSOB)
		{
			if ((int)tSOB.Hp > 0)
			{
				return;
			}
			for (int num = _inGameEnemy.Count - 1; num >= 0; num--)
			{
				if (_inGameEnemy[num].targetEnemy != null && _inGameEnemy[num].sNetSerialID == tSOB.sNetSerialID)
				{
					if (!_listNetDeadNetSerialIDs.Contains(tSOB.sNetSerialID))
					{
						StageUpdate.SyncStageObj(sSyncID, 2, _nowEventGroup + "," + _inGameEnemy[num].sNetSerialID, true);
					}
					SetEnemyDead(num);
					break;
				}
			}
			tSOB.HurtActions -= HurtCB;
			if (_nEnemyCount == 0 && nEndEventID == 17 && _nowEventGroup == SpawnGroups.Count - 1)
			{
				StageUpdate.SlowStage();
			}
		}

		private void CheckSpawnGroups()
		{
			int count = SpawnPoints.Count;
			for (int i = 0; i < SpawnGroups.Count; i++)
			{
				for (int j = 0; j < SpawnGroups[i].mEnemySpawnDatas.Count; j++)
				{
					if (SpawnGroups[i].mEnemySpawnDatas[j].nPosID < 0 || SpawnGroups[i].mEnemySpawnDatas[j].nPosID >= count)
					{
						SpawnGroups[i].mEnemySpawnDatas.RemoveAt(j);
						j--;
					}
				}
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.3f, 0.9f, 0.3f);
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		public override int GetTypeID()
		{
			return 8;
		}

		public override string GetTypeString()
		{
			return StageObjType.ENEMYEP_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			updatePatrolPathData();
			CheckSpawnGroups();
			GroupSpawnSL groupSpawnSL = new GroupSpawnSL();
			groupSpawnSL.SpawnGroups = SpawnGroups;
			groupSpawnSL.nSetID = nSetID;
			groupSpawnSL.SpawnPoints.Clear();
			groupSpawnSL.B2DX = EventB2D.offset.x;
			groupSpawnSL.B2DY = EventB2D.offset.y;
			groupSpawnSL.B2DW = EventB2D.size.x;
			groupSpawnSL.B2DH = EventB2D.size.y;
			groupSpawnSL.bDead = bDead;
			groupSpawnSL.fDTime = fDTime;
			groupSpawnSL.sSave = "";
			groupSpawnSL.sSave = GetBoolSaveStr(bShowEnemyHint);
			groupSpawnSL.sSave = groupSpawnSL.sSave + "," + nStartEventID;
			groupSpawnSL.sSave = groupSpawnSL.sSave + "," + GetBoolSaveStr(bUseBoxCollider2D);
			groupSpawnSL.sSave = groupSpawnSL.sSave + "," + GetBoolSaveStr(bCanCallReRun);
			groupSpawnSL.sSave = groupSpawnSL.sSave + "," + nUnlockRangeID;
			groupSpawnSL.sSave = groupSpawnSL.sSave + "," + GetBoolSaveStr(bCheckBornInB2D);
			for (int i = 0; i < SpawnPoints.Count; i++)
			{
				groupSpawnSL.SpawnPoints.Add(SpawnPoints.GetPosByIndex(i));
			}
			groupSpawnSL.bRunAtStart = bRunAtStart;
			groupSpawnSL.bUnLockRange = bUnLockRange;
			groupSpawnSL.nEndEventID = nEndEventID;
			groupSpawnSL.nEndParam1 = nEndParam1;
			string text = JsonConvert.SerializeObject(groupSpawnSL, Formatting.None, JsonHelper.IgnoreLoopSetting());
			text = text.Replace(",", ";");
			return typeString + text;
		}

		private void updatePatrolPathData()
		{
		}

		public override void LoadByString(string sLoad)
		{
			GroupSpawnSL groupSpawnSL = JsonUtility.FromJson<GroupSpawnSL>(sLoad.Substring(GetTypeString().Length).Replace(";", ","));
			SpawnGroups.Clear();
			EventB2D = GetComponent<BoxCollider2D>();
			EventB2D.offset = new Vector2(groupSpawnSL.B2DX, groupSpawnSL.B2DY);
			EventB2D.size = new Vector2(groupSpawnSL.B2DW, groupSpawnSL.B2DH);
			SpawnGroups = groupSpawnSL.SpawnGroups;
			SpawnPoints.Clear();
			_nCount = groupSpawnSL.SpawnPoints.Count;
			for (int i = 0; i < _nCount; i++)
			{
				AddOneSpawnPoint(groupSpawnSL.SpawnPoints[i]);
			}
			CheckSpawnGroups();
			bRunAtStart = groupSpawnSL.bRunAtStart;
			bUnLockRange = groupSpawnSL.bUnLockRange;
			nSetID = groupSpawnSL.nSetID;
			nEndEventID = groupSpawnSL.nEndEventID;
			nEndParam1 = groupSpawnSL.nEndParam1;
			bDead = groupSpawnSL.bDead;
			fDTime = groupSpawnSL.fDTime;
			if (groupSpawnSL.sSave != "")
			{
				string[] array = groupSpawnSL.sSave.Split(',');
				if (array.Length >= 1)
				{
					bShowEnemyHint = GetBoolBySaveStr(array[0]);
				}
				if (array.Length >= 2)
				{
					nStartEventID = int.Parse(array[1]);
				}
				if (array.Length >= 3)
				{
					bUseBoxCollider2D = GetBoolBySaveStr(array[2]);
				}
				if (array.Length >= 4)
				{
					bCanCallReRun = GetBoolBySaveStr(array[3]);
				}
				if (array.Length >= 5)
				{
					nUnlockRangeID = int.Parse(array[4]);
				}
				if (array.Length >= 6)
				{
					bCheckBornInB2D = GetBoolBySaveStr(array[5]);
				}
			}
			_nCount = SpawnGroups.Count;
			int num = 0;
			for (int j = 0; j < SpawnGroups.Count; j++)
			{
				SpawnGroups[j].SortByTime();
				_nCount2 = SpawnGroups[j].mEnemySpawnDatas.Count;
				for (int k = 0; k < _nCount2; k++)
				{
					StageResManager.LoadEnemy(SpawnGroups[j].mEnemySpawnDatas[k].nID);
					if (((uint)SpawnGroups[j].mEnemySpawnDatas[k].nStickWall & 0x40u) != 0)
					{
						for (int l = 0; l < SpawnGroups[j].mEnemySpawnDatas[k].lsDlItm.Count; l += 2)
						{
							StageResManager.LoadStageItemModel(SpawnGroups[j].mEnemySpawnDatas[k].lsDlItm[l]);
						}
					}
				}
				SpawnGroups[j].nEnemyStartIndex = num;
				num += _nCount2;
			}
		}

		public override void SyncNowStatus()
		{
			if (!bCheck)
			{
				string text = _inGameEnemy.Count.ToString();
				for (int i = 0; i < _inGameEnemy.Count; i++)
				{
					if (_inGameEnemy[i].targetEnemy != null)
					{
						text = text + "," + _inGameEnemy[i].nStartIndex;
						text = text + "," + _inGameEnemy[i].targetEnemy.transform.position.x;
						text = text + "," + _inGameEnemy[i].targetEnemy.transform.position.y;
						text = text + "," + _inGameEnemy[i].targetEnemy.Hp;
						text = text + "," + _inGameEnemy[i].targetEnemy.HealHp;
						text = text + "," + _inGameEnemy[i].targetEnemy.DmgHp;
						text = text + "," + _inGameEnemy[i].targetEnemy.direction;
						for (int j = 0; j < _inGameEnemy[i].targetEnemy.PartHp.Length; j++)
						{
							text = text + "," + _inGameEnemy[i].targetEnemy.PartHp[j];
						}
					}
				}
				StageUpdate.SyncStageObj(sSyncID, 5, _sTriggerNetID + "," + _nowEventGroup + "," + _nowStartIndex + "," + _nowEventTime + "," + text, true);
			}
			else
			{
				StageUpdate.SyncStageObj(sSyncID, 7, "", true);
			}
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
			base.OnSyncStageObj(sIDKey, nKey1, smsg);
			if (StageUpdate.bWaitReconnect && StageResManager.GetStageUpdate().nRunStageCtrlCount > 0)
			{
				return;
			}
			switch (nKey1)
			{
			case 0:
			{
				string[] array6 = smsg.Split(',');
				int num16 = int.Parse(array6[0]);
				if (num16 < 0 || num16 >= SpawnGroups.Count)
				{
					break;
				}
				if (!_bStartEvent && bCheck)
				{
					_bStartEvent = true;
					_nowEventGroup = num16;
					_nowEventTime = 0f;
					_nowStartIndex = 0;
					InitSpawnData();
					UpdateCall = OnLateUpdate;
					_sTriggerNetID = array6[1];
					TriggerStartEventCall();
					if (CheckShowBattleing())
					{
						EventManager.BattleInfoUpdate battleInfoUpdate2 = new EventManager.BattleInfoUpdate();
						battleInfoUpdate2.nType = 1;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_INFO_UPDATE, battleInfoUpdate2);
					}
				}
				bCheck = false;
				break;
			}
			case 1:
			{
				string[] array2 = smsg.Split(',');
				int num2 = int.Parse(array2[0]);
				int num3 = int.Parse(array2[1]);
				if (!_bStartEvent)
				{
					if (!bCheck)
					{
						break;
					}
					_nowEventGroup = num2;
					_nowStartIndex = 0;
					_nowEventTime = 0f;
					InitSpawnData();
					_bStartEvent = true;
					UpdateCall = OnLateUpdate;
					bCheck = false;
				}
				else if (_nowEventGroup < num2)
				{
					_nowEventGroup = num2;
					_nowStartIndex = 0;
					_nowEventTime = 0f;
					InitSpawnData();
					bCheck = false;
				}
				if (_nowStartIndex > num3)
				{
					break;
				}
				while (_nowStartIndex < num3 + 1)
				{
					EnemySpawnData tEnemySpawnData = targetGroupSpawnData.mEnemySpawnDatas[_nowStartIndex];
					string text = sSyncID + (targetGroupSpawnData.nEnemyStartIndex + _nowStartIndex);
					if (_listNetDeadNetSerialIDs.Contains(text))
					{
						CheckDeadToEnemyCount(tEnemySpawnData);
					}
					else
					{
						SpawnEnemy(tEnemySpawnData, text, _nowStartIndex, false);
					}
					_nowStartIndex++;
				}
				break;
			}
			case 2:
			{
				string[] array5 = smsg.Split(',');
				int.Parse(array5[0]);
				string text3 = array5[1];
				if (!_bStartEvent)
				{
					_listNetDeadNetSerialIDs.Add(text3);
					break;
				}
				_listNetDeadNetSerialIDs.Add(text3);
				for (int num15 = _inGameEnemy.Count - 1; num15 >= 0; num15--)
				{
					if (_inGameEnemy[num15].targetEnemy != null && _inGameEnemy[num15].sNetSerialID == text3)
					{
						_inGameEnemy[num15].targetEnemy.Hp = 0;
						_inGameEnemy[num15].targetEnemy.Hurt(new HurtPassParam());
						break;
					}
				}
				break;
			}
			case 3:
			{
				if (!_bStartEvent)
				{
					break;
				}
				for (int num4 = _inGameEnemy.Count - 1; num4 >= 0; num4--)
				{
					if (_inGameEnemy[num4].targetEnemy != null)
					{
						_inGameEnemy[num4].targetEnemy.Hp = 0;
						_inGameEnemy[num4].targetEnemy.Hurt(new HurtPassParam());
					}
				}
				_inGameEnemy.Clear();
				_nEnemyCount = 0;
				bCheck = false;
				_bStartEvent = false;
				_nowStartIndex = 0;
				_nowEventGroup = SpawnGroups.Count;
				TriggerUnLockRange();
				TriggerEndEventCall();
				_listNetDeadNetSerialIDs.Clear();
				break;
			}
			case 5:
			{
				string[] array3 = smsg.Split(',');
				int num5 = 0;
				_sTriggerNetID = array3[num5++];
				int num6 = int.Parse(array3[num5++]);
				int num7 = int.Parse(array3[num5++]);
				float nowEventTime = float.Parse(array3[num5++]);
				List<EnemySpawnData> inGameEnemy = _inGameEnemy;
				if (!bCheck)
				{
					if (num6 >= SpawnGroups.Count)
					{
						_bStartEvent = false;
						ClearInGameEnemy();
						break;
					}
					if (_nowEventGroup == num6)
					{
						if (targetGroupSpawnData == null || _nowStartIndex == 0)
						{
							InitSpawnData();
						}
						_nowEventTime = nowEventTime;
					}
					else
					{
						_nowEventGroup = num6;
						_nowStartIndex = 0;
						_nowEventTime = nowEventTime;
						InitSpawnData();
						inGameEnemy = _inGameEnemy;
					}
				}
				else
				{
					if (num6 >= SpawnGroups.Count)
					{
						bCheck = false;
						_bStartEvent = false;
						_nowEventGroup = SpawnGroups.Count;
						break;
					}
					bCheck = false;
					_nowEventGroup = num6;
					_nowStartIndex = 0;
					_nowEventTime = nowEventTime;
					InitSpawnData();
					if (CheckShowBattleing())
					{
						EventManager.BattleInfoUpdate battleInfoUpdate = new EventManager.BattleInfoUpdate();
						battleInfoUpdate.nType = 1;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_INFO_UPDATE, battleInfoUpdate);
					}
				}
				_bStartEvent = true;
				if (_nowEventGroup >= SpawnGroups.Count)
				{
					_bStartEvent = false;
					ClearInGameEnemy();
					break;
				}
				while (_nowStartIndex < num7)
				{
					EnemySpawnData enemySpawnData = targetGroupSpawnData.mEnemySpawnDatas[_nowStartIndex];
					if (targetGroupSpawnData.nDeadID != -1)
					{
						if (targetGroupSpawnData.nDeadID == enemySpawnData.nDeadID)
						{
							_nEnemyCount--;
						}
					}
					else
					{
						_nEnemyCount--;
					}
					_nowStartIndex++;
				}
				_inGameEnemy = new List<EnemySpawnData>();
				int.Parse(array3[num5++]);
				while (num5 < array3.Length)
				{
					int num8 = int.Parse(array3[num5++]);
					float x = float.Parse(array3[num5++]);
					float y = float.Parse(array3[num5++]);
					int num9 = int.Parse(array3[num5++]);
					int num10 = int.Parse(array3[num5++]);
					int num11 = int.Parse(array3[num5++]);
					int num12 = int.Parse(array3[num5++]);
					EnemySpawnData enemySpawnData2 = targetGroupSpawnData.mEnemySpawnDatas[num8];
					string sTmpSerialID = sSyncID + (targetGroupSpawnData.nEnemyStartIndex + num8);
					if (inGameEnemy.Contains(enemySpawnData2))
					{
						inGameEnemy.Remove(enemySpawnData2);
					}
					bool bBack = true;
					if (num12 == 1)
					{
						bBack = false;
					}
					if (enemySpawnData2.targetEnemy != null)
					{
						enemySpawnData2.targetEnemy.SetPositionAndRotation(new Vector3(x, y, 0f), bBack);
						enemySpawnData2.targetEnemy.Hp = num9;
						enemySpawnData2.targetEnemy.HealHp = num10;
						enemySpawnData2.targetEnemy.DmgHp = num11;
						enemySpawnData2.targetEnemy.Hurt(new HurtPassParam());
						for (int j = 0; j < enemySpawnData2.targetEnemy.PartHp.Length; j++)
						{
							enemySpawnData2.targetEnemy.PartHp[j] = int.Parse(array3[num5++]);
						}
						_inGameEnemy.Add(enemySpawnData2);
						continue;
					}
					Vector3 value = new Vector3(x, y, 0f);
					if (!SpawnEnemy(enemySpawnData2, sTmpSerialID, num8, true, false, value))
					{
						return;
					}
					enemySpawnData2.targetEnemy.Hp = num9;
					enemySpawnData2.targetEnemy.HealHp = num10;
					enemySpawnData2.targetEnemy.DmgHp = num11;
					enemySpawnData2.targetEnemy.Hurt(new HurtPassParam());
					for (int k = 0; k < enemySpawnData2.targetEnemy.PartHp.Length; k++)
					{
						enemySpawnData2.targetEnemy.PartHp[k] = int.Parse(array3[num5++]);
					}
					if (enemySpawnData2.targetEnemy.EnemyData.n_TYPE == 2 || enemySpawnData2.targetEnemy.EnemyData.n_TYPE == 5)
					{
						BattleInfoUI.Instance.FullBossBarHP(100, num9);
						if (enemySpawnData2.targetEnemy.EnemyData.n_TYPE == 5)
						{
							BattleInfoUI.Instance.SetHiddenBossBar(false);
						}
					}
				}
				for (int l = 0; l < inGameEnemy.Count; l++)
				{
					if (inGameEnemy[l].targetEnemy != null)
					{
						inGameEnemy[l].targetEnemy.HurtActions -= HurtCB;
						inGameEnemy[l].targetEnemy.Hp = 0;
						inGameEnemy[l].targetEnemy.Hurt(new HurtPassParam());
					}
				}
				inGameEnemy.Clear();
				break;
			}
			case 6:
			{
				string[] array4 = smsg.Split(',');
				int num13 = int.Parse(array4[0]);
				int num14 = int.Parse(array4[1]);
				if (!_bStartEvent || _nowEventGroup != num13 || _nowRebornIndex > num14)
				{
					break;
				}
				while (_nowRebornIndex < num14 + 1)
				{
					EnemySpawnData tEnemySpawnData2 = targetGroupSpawnData.mEnemySpawnDatas[_nowRebornIndex];
					string text2 = sSyncID + (targetGroupSpawnData.nEnemyStartIndex + _nowRebornIndex);
					if (_listNetDeadNetSerialIDs.Contains(text2))
					{
						CheckDeadToEnemyCount(tEnemySpawnData2);
					}
					else
					{
						bool flag = true;
						for (int m = 0; m < _inGameEnemy.Count; m++)
						{
							if (_inGameEnemy[m].targetEnemy != null && _inGameEnemy[m].targetEnemy.sNetSerialID == text2)
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							SpawnEnemy(tEnemySpawnData2, text2, _nowRebornIndex);
						}
					}
					_nowRebornIndex++;
				}
				break;
			}
			case 7:
			{
				if (bCheck)
				{
					break;
				}
				for (int i = 0; i < _inGameEnemy.Count; i++)
				{
					if (_inGameEnemy[i].targetEnemy != null)
					{
						_inGameEnemy[i].targetEnemy.HurtActions -= HurtCB;
						_inGameEnemy[i].targetEnemy.Hp = 0;
						_inGameEnemy[i].targetEnemy.Hurt(new HurtPassParam());
					}
				}
				bCheck = true;
				_bStartEvent = false;
				_nowEventGroup = 0;
				targetGroupSpawnData = null;
				_nEnemyCount = 0;
				_nowStartIndex = 0;
				_listNetDeadNetSerialIDs.Clear();
				_inGameEnemy.Clear();
				_bRebornFlag = false;
				UpdateCall = UpdateEvent;
				break;
			}
			case 8:
				if (!bCheck && _bStartEvent && _nowStartIndex >= _nCount && !_bRebornFlag)
				{
					string[] array = smsg.Split(',');
					int num = 0;
					int.Parse(array[num++]);
					int nowRebornIndex = int.Parse(array[num++]);
					_nowRebornIndex = nowRebornIndex;
					if (_nowRebornIndex == 0)
					{
						_nowRebornTime = 0f;
					}
					else
					{
						_nowRebornTime = targetGroupSpawnData.mEnemySpawnDatas[_nowRebornIndex - 1].fTime;
					}
					_nextRebornTime = 0f;
					_listNetDeadNetSerialIDs.Clear();
					_listNonRebornPoint.Clear();
					_bRebornFlag = true;
					int.Parse(array[num++]);
					while (num < array.Length)
					{
						_listNonRebornPoint.Add(int.Parse(array[num++]));
					}
				}
				break;
			}
		}

		private void TriggerStartEventCall()
		{
			if (nStartEventID != 0)
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = nStartEventID;
				StageObjBase sOBByNetSerialID = StageResManager.GetStageUpdate().GetSOBByNetSerialID(_sTriggerNetID);
				if (sOBByNetSerialID != null)
				{
					stageEventCall.tTransform = sOBByNetSerialID.transform;
				}
				else
				{
					stageEventCall.tTransform = null;
				}
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
		}

		private void TriggerEndEventCall()
		{
			if (nEndEventID == 0)
			{
				return;
			}
			EventManager.ID iD = (EventManager.ID)nEndEventID;
			switch (iD)
			{
			case EventManager.ID.STAGE_EVENT_CALL:
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = nEndParam1;
				StageObjBase sOBByNetSerialID = StageResManager.GetStageUpdate().GetSOBByNetSerialID(_sTriggerNetID);
				if (sOBByNetSerialID != null)
				{
					stageEventCall.tTransform = sOBByNetSerialID.transform;
				}
				else
				{
					stageEventCall.tTransform = null;
				}
				Singleton<GenericEventManager>.Instance.NotifyEvent(iD, stageEventCall);
				break;
			}
			case EventManager.ID.CAMERA_SHAKE:
				Singleton<GenericEventManager>.Instance.NotifyEvent(iD, (float)nEndParam1, false);
				break;
			case EventManager.ID.STAGE_END_REPORT:
				Singleton<GenericEventManager>.Instance.NotifyEvent(iD);
				break;
			}
		}

		private void TriggerUnLockRange()
		{
			if (!bUnLockRange)
			{
				return;
			}
			if (nUnlockRangeID != 0)
			{
				for (int num = StageUpdate.listAllEvent.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.listAllEvent[num].nSetID == nUnlockRangeID)
					{
						LockRangeEvent lockRangeEvent = StageUpdate.listAllEvent[num] as LockRangeEvent;
						if (lockRangeEvent != null)
						{
							string sPlayerID = "";
							if (lockRangeEvent.IsSelfLockRange())
							{
								sPlayerID = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
							}
							if (StageUpdate.CheckLockRangeList(StageUpdate.listAllEvent[num].sSyncID, sPlayerID, true))
							{
								StageResManager.RegisterLockEvent(new TriggerLockEventData
								{
									fMax = 9999f,
									fMin = -9999f,
									fBtn = -9999f,
									fTop = 9999f,
									nType = 5,
									fSpeed = null,
									bSetFocus = false
								});
								EventManager.BattleInfoUpdate battleInfoUpdate = new EventManager.BattleInfoUpdate();
								battleInfoUpdate.nType = 2;
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_INFO_UPDATE, battleInfoUpdate);
							}
							StageUpdate.RemoveLockRange(StageUpdate.listAllEvent[num].sSyncID, "", lockRangeEvent.bLockNet);
						}
					}
				}
			}
			else
			{
				StageResManager.RegisterLockEvent(new TriggerLockEventData
				{
					fMax = 9999f,
					fMin = -9999f,
					fBtn = -9999f,
					fTop = 9999f,
					nType = 5,
					fSpeed = null,
					bSetFocus = false
				});
				StageUpdate.RemoveLastLockRange();
				EventManager.BattleInfoUpdate battleInfoUpdate = new EventManager.BattleInfoUpdate();
				battleInfoUpdate.nType = 2;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_INFO_UPDATE, battleInfoUpdate);
			}
		}

		private void ClearInGameEnemy()
		{
			for (int num = _inGameEnemy.Count - 1; num >= 0; num--)
			{
				if (_inGameEnemy[num].targetEnemy != null)
				{
					_inGameEnemy[num].targetEnemy.Hp = 0;
					_inGameEnemy[num].targetEnemy.Hurt(new HurtPassParam());
				}
			}
			_inGameEnemy.Clear();
		}

		private bool SpawnEnemy(EnemySpawnData tEnemySpawnData, string sTmpSerialID, int nStartIndex, bool bNeedAddEnemyCount = true, bool bNeedCheckBornInB2D = false, Vector3? vPos = null)
		{
			bool result = false;
			int num = 0;
			if (bShowEnemyHint)
			{
				num = 1;
				if (((uint)tEnemySpawnData.nStickWall & 0x10u) != 0)
				{
					num |= 2;
				}
			}
			if (((uint)tEnemySpawnData.nStickWall & 0x20u) != 0)
			{
				num |= 4;
			}
			if (((uint)tEnemySpawnData.nStickWall & 0x100u) != 0)
			{
				num |= 8;
			}
			EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemy(tEnemySpawnData.nID, sTmpSerialID, num, tEnemySpawnData.nSCEID, tEnemySpawnData.fAiRange, tEnemySpawnData.fAiRangeY, tEnemySpawnData.fOffsetX, tEnemySpawnData.fOffsetY);
			if (enemyControllerBase != null)
			{
				Vector3 vector = vPos ?? SpawnPoints.GetPosByIndex(tEnemySpawnData.nPosID);
				enemyControllerBase.HurtActions += HurtCB;
				tEnemySpawnData.sNetSerialID = enemyControllerBase.sNetSerialID;
				CheckEnemyPosition(enemyControllerBase, vector, tEnemySpawnData.nStickWall, tEnemySpawnData.bBack);
				CheckPatrolPath(enemyControllerBase, tEnemySpawnData);
				CheckEventCtrlID(enemyControllerBase, tEnemySpawnData);
				CheckIntParameter(enemyControllerBase, tEnemySpawnData);
				enemyControllerBase.SetSummonEventID(tEnemySpawnData.nSummonEventId);
				enemyControllerBase.SetStageCustomParams(tEnemySpawnData.nStageCustomType, tEnemySpawnData.nStageCustomParams);
				if (bNeedCheckBornInB2D && bCheckBornInB2D)
				{
					List<StageSceneObjParam> tOutList = new List<StageSceneObjParam>();
					SpawnPoints.CheckPosIsInSceneObjB2D(tEnemySpawnData.nPosID, ref tOutList, vector);
					if (tOutList.Count > 0)
					{
						foreach (StageSceneObjParam item in tOutList)
						{
							item.AddBrokenActiveEnemyID(enemyControllerBase.sNetSerialID);
						}
					}
					if (tOutList.Count != 0 && ((int)enemyControllerBase.Controller.collisionMask | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer)) != 0)
					{
						ObjInfoBar componentInChildren = enemyControllerBase.transform.GetComponentInChildren<ObjInfoBar>();
						if (componentInChildren != null)
						{
							componentInChildren.gameObject.SetActive(false);
						}
						enemyControllerBase.OnlySwitchMaterial(true);
					}
					else
					{
						enemyControllerBase.SetActive(true);
					}
				}
				else
				{
					enemyControllerBase.SetActive(true);
				}
				if (bDead)
				{
					AddOutDead(enemyControllerBase);
				}
				tEnemySpawnData.targetEnemy = enemyControllerBase;
				tEnemySpawnData.nStartIndex = nStartIndex;
				_inGameEnemy.Add(tEnemySpawnData);
				if (bNeedAddEnemyCount)
				{
					_nEnemyCount++;
				}
				result = true;
			}
			else
			{
				CheckDeadToEnemyCount(tEnemySpawnData);
			}
			return result;
		}
	}
}
