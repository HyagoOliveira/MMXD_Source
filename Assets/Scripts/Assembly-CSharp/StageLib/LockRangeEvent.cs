#define RELEASE
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class LockRangeEvent : EventPointBase
	{
		private enum LRE_NET_EVENT
		{
			JUST_TRIGGER = 0,
			FILL_LOCKLIST = 1,
			SET_CHECK = 2,
			COPY_LIST = 3,
			COPY_LIST_TRIGGER = 4
		}

		public class LockRangeSL
		{
			[JsonConverter(typeof(FloatConverter))]
			public float fMin;

			[JsonConverter(typeof(FloatConverter))]
			public float fMax;

			[JsonConverter(typeof(FloatConverter))]
			public float fTop;

			[JsonConverter(typeof(FloatConverter))]
			public float fBtn;

			public int nType;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DX;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DY;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DW = 1f;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DH = 1f;

			[JsonConverter(typeof(FloatConverter))]
			public float fSpeed = 10f;

			public int nSetID;

			public string sMsg = "";
		}

		private GameObject[] minmaxobj;

		public float fMin;

		public float fMax;

		public float fTop;

		public float fBtn;

		public int nType;

		public float fSpeed = 10f;

		public bool bLockNet;

		public float fOY;

		public bool bSlowWhenMove;

		public bool bFirstTImeNoSlowWhenMove;

		public bool bChangeToBattleHint;

		public bool bChangeToGoArrowHint;

		public bool bSavePlayerIDFalg;

		private string sTriggerNetID = "";

		private const float fCWaitLockTime = 3f;

		private Vector3 tNowPos;

		private Vector3 tLastPos;

		private Vector3 vMax;

		private Vector3 vMin;

		public void Awake()
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
		}

		public void OnDestroy()
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
		}

		public override void Init()
		{
		}

		protected override void UpdateEvent()
		{
			if (!bCheck || !bUseBoxCollider2D)
			{
				return;
			}
			vMax = EventB2D.bounds.max;
			vMin = EventB2D.bounds.min;
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				if (StageUpdate.runPlayers[num].UsingVehicle)
				{
					RideArmorController component = StageUpdate.runPlayers[num].transform.root.GetComponent<RideArmorController>();
					if (component.MasterPilot.bIsNpcCpy)
					{
						continue;
					}
					if (bSavePlayerIDFalg)
					{
						if (StageUpdate.CheckLockRangeList(sSyncID, component.MasterPilot.sPlayerID))
						{
							continue;
						}
					}
					else if (!bLockNet && component.MasterPilot.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
					{
						continue;
					}
					tNowPos = component.MasterPilot.transform.position;
					if (vMax.x > tNowPos.x && vMax.y > tNowPos.y && vMin.x < tNowPos.x && vMin.y < tNowPos.y)
					{
						OnEvent(component.transform);
						if (!bCheck)
						{
							break;
						}
					}
				}
				else
				{
					if (StageUpdate.runPlayers[num].bIsNpcCpy)
					{
						continue;
					}
					if (bSavePlayerIDFalg)
					{
						if (StageUpdate.CheckLockRangeList(sSyncID, StageUpdate.runPlayers[num].sPlayerID))
						{
							continue;
						}
					}
					else if (!bLockNet && StageUpdate.runPlayers[num].sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
					{
						continue;
					}
					tLastPos = StageUpdate.runPlayers[num].vLastMovePt;
					tNowPos = StageUpdate.runPlayers[num].transform.position;
					if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
					{
						if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
						{
							OnEvent(StageUpdate.runPlayers[num].transform);
							if (!bCheck)
							{
								break;
							}
						}
						else if ((tLastPos.y < vMin.y && tNowPos.y > vMax.y) || (tLastPos.y > vMax.y && tNowPos.y < vMin.y))
						{
							OnEvent(StageUpdate.runPlayers[num].transform);
							if (!bCheck)
							{
								break;
							}
						}
					}
					else
					{
						if (!(vMin.y < tNowPos.y) || !(vMax.y > tNowPos.y))
						{
							continue;
						}
						if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
						{
							OnEvent(StageUpdate.runPlayers[num].transform);
							if (!bCheck)
							{
								break;
							}
						}
						else if ((tLastPos.x < vMin.x && tNowPos.x > vMax.x) || (tLastPos.x > vMax.x && tNowPos.x < vMin.x))
						{
							OnEvent(StageUpdate.runPlayers[num].transform);
							if (!bCheck)
							{
								break;
							}
						}
					}
				}
			}
		}

		public override void OnEvent(Transform TriggerTransform)
		{
			if (!bCheck)
			{
				return;
			}
			sTriggerNetID = "";
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				RideArmorController component = TriggerTransform.GetComponent<RideArmorController>();
				if ((bool)component)
				{
					if (StageUpdate.runPlayers[num].gameObject.GetInstanceID() == component.MasterPilot.gameObject.GetInstanceID())
					{
						sTriggerNetID = StageUpdate.runPlayers[num].sPlayerID;
						break;
					}
				}
				else if (StageUpdate.runPlayers[num].gameObject.GetInstanceID() == TriggerTransform.gameObject.GetInstanceID())
				{
					sTriggerNetID = StageUpdate.runPlayers[num].sPlayerID;
					break;
				}
			}
			Vector3 position = TriggerTransform.position;
			if (bLockNet && sTriggerNetID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				List<string> list = new List<string>();
				list.Add(sSyncID);
				StageUpdate.CheckLastLockRangeBeforeSendNetLockMsg(sTriggerNetID);
				StageUpdate.SyncStageObj(sSyncID, 0, sTriggerNetID + "," + position.x + "," + position.y + "," + position.z + "," + StageUpdate.GetLockRangeListStr(list), true);
			}
			if (IsSelfLockRange())
			{
				StageUpdate.AddLockRangeList(sSyncID, sTriggerNetID);
				if (sTriggerNetID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					return;
				}
			}
			else
			{
				StageUpdate.AddLockRangeList(sSyncID);
				bCheck = false;
			}
			if ((bChangeToBattleHint || bChangeToGoArrowHint) && BattleInfoUI.Instance != null)
			{
				EventManager.BattleInfoUpdate battleInfoUpdate = new EventManager.BattleInfoUpdate();
				if (bChangeToBattleHint)
				{
					battleInfoUpdate.nType = 1;
				}
				if (bChangeToGoArrowHint)
				{
					battleInfoUpdate.nType = 2;
				}
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_INFO_UPDATE, battleInfoUpdate);
			}
			TriggerLockEventData triggerLockEventData = new TriggerLockEventData();
			triggerLockEventData.sTriggerPlayerID = sTriggerNetID;
			triggerLockEventData.sSyncID = sSyncID;
			triggerLockEventData.vTriggerPos = position;
			triggerLockEventData.fMax = fMax;
			triggerLockEventData.fMin = fMin;
			triggerLockEventData.fBtn = fBtn;
			triggerLockEventData.fTop = fTop;
			triggerLockEventData.nType = nType;
			triggerLockEventData.fSpeed = fSpeed;
			triggerLockEventData.bLockNet = bLockNet;
			triggerLockEventData.fOY = fOY;
			if (bFirstTImeNoSlowWhenMove)
			{
				bFirstTImeNoSlowWhenMove = false;
				triggerLockEventData.bSlowWhenMove = false;
			}
			else
			{
				triggerLockEventData.bSlowWhenMove = bSlowWhenMove;
			}
			if (sTriggerNetID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				triggerLockEventData.fWaitLockTime = 3f;
			}
			StageResManager.RegisterLockEvent(triggerLockEventData);
		}

		public bool IsSelfLockRange()
		{
			if (bSavePlayerIDFalg)
			{
				return !bLockNet;
			}
			return false;
		}

		public bool CheckTriggerPlayer()
		{
			if (IsSelfLockRange() && sTriggerNetID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				return false;
			}
			return true;
		}

		public static bool CheckPlayerPosition(OrangeCharacter tOC, Vector3? tTriggerPos, float fMax, float fMin, float fTop, float fBtm)
		{
			Controller2D controller = tOC.Controller;
			if (tOC.UsingVehicle)
			{
				controller = tOC.transform.root.GetComponent<RideArmorController>().Controller;
			}
			Vector3 size = controller.Collider2D.bounds.size;
			Vector3 vec = controller.LogicPosition.vec3;
			bool flag = false;
			if (vec.x - size.x < fMin)
			{
				flag = true;
			}
			if (vec.x + size.x > fMax)
			{
				flag = true;
			}
			if (vec.y + size.y > fTop)
			{
				flag = true;
			}
			if (vec.y < fBtm)
			{
				flag = true;
			}
			if (flag)
			{
				if (tTriggerPos.HasValue)
				{
					controller.LogicPosition = new VInt3(tTriggerPos ?? Vector3.zero);
					if (!tOC.UsingVehicle)
					{
						tOC.StageTeleportOutCharacterDependEvt.CheckTargetToInvoke();
						tOC.vLastMovePt = tTriggerPos ?? Vector3.zero;
						tOC.vLastStandPt = tTriggerPos ?? Vector3.zero;
						tOC.transform.position = tTriggerPos ?? Vector3.zero;
						tOC.StageTeleportInCharacterDependEvt.CheckTargetToInvoke();
					}
					else
					{
						controller.transform.position = tTriggerPos ?? Vector3.zero;
					}
				}
				return true;
			}
			return false;
		}

		private void Initminmaxobj()
		{
			if (minmaxobj != null)
			{
				return;
			}
			while (base.transform.childCount > 0)
			{
				Object.DestroyImmediate(base.transform.GetChild(0).gameObject);
			}
			minmaxobj = new GameObject[4];
			for (int i = 0; i < minmaxobj.Length; i++)
			{
				minmaxobj[i] = new GameObject();
				minmaxobj[i].transform.parent = base.transform;
				minmaxobj[i].name = "Edge";
				switch (i)
				{
				case 0:
					minmaxobj[i].transform.position = new Vector3(fMin, base.transform.position.y, 0f);
					break;
				case 1:
					minmaxobj[i].transform.position = new Vector3(fMax, base.transform.position.y, 0f);
					break;
				case 2:
					minmaxobj[i].transform.position = new Vector3(base.transform.position.x, fTop, 0f);
					break;
				case 3:
					minmaxobj[i].transform.position = new Vector3(base.transform.position.x, fBtn, 0f);
					break;
				}
				PosShower posShower = minmaxobj[i].AddComponent<PosShower>();
				if (i < 2)
				{
					posShower.rect = new Vector3(0.2f, 100f, 1f);
				}
				else
				{
					posShower.rect = new Vector3(100f, 0.2f, 1f);
				}
				posShower.bWire = false;
			}
		}

		public GameObject GetPosObj(int n)
		{
			Initminmaxobj();
			if (n < 0 || n >= minmaxobj.Length)
			{
				return null;
			}
			return minmaxobj[n];
		}

		public void CallOnByID(EventManager.StageEventCall tStageEventCall)
		{
			int nID = tStageEventCall.nID;
			if (nID != 0 && nSetID == nID)
			{
				Transform transform = tStageEventCall.tTransform;
				if (transform == null)
				{
					transform = StageUpdate.GetMainPlayerTrans();
				}
				bCheck = true;
				OnEvent(transform);
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 0.5f, 0.3f);
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		public override int GetTypeID()
		{
			return 7;
		}

		public override string GetTypeString()
		{
			return StageObjType.LOCK_RANGE_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			LockRangeSL obj = new LockRangeSL
			{
				fMin = fMin,
				fMax = fMax,
				fTop = fTop,
				fBtn = fBtn,
				nType = nType,
				B2DX = EventB2D.offset.x,
				B2DY = EventB2D.offset.y,
				B2DW = EventB2D.size.x,
				B2DH = EventB2D.size.y,
				fSpeed = fSpeed,
				nSetID = nSetID,
				sMsg = ""
			};
			obj.sMsg += GetBoolSaveStr(bLockNet);
			obj.sMsg = obj.sMsg + "," + fOY.ToString("0.000");
			obj.sMsg = obj.sMsg + "," + GetBoolSaveStr(bUseBoxCollider2D);
			obj.sMsg = obj.sMsg + "," + GetBoolSaveStr(bSlowWhenMove);
			obj.sMsg = obj.sMsg + "," + GetBoolSaveStr(bFirstTImeNoSlowWhenMove);
			obj.sMsg = obj.sMsg + "," + GetBoolSaveStr(bChangeToBattleHint);
			obj.sMsg = obj.sMsg + "," + GetBoolSaveStr(bSavePlayerIDFalg);
			obj.sMsg = obj.sMsg + "," + GetBoolSaveStr(bChangeToGoArrowHint);
			string text = JsonConvert.SerializeObject(obj, Formatting.None, JsonHelper.IgnoreLoopSetting());
			text = text.Replace(",", ";");
			return typeString + text;
		}

		public override void LoadByString(string sLoad)
		{
			LockRangeSL lockRangeSL = JsonUtility.FromJson<LockRangeSL>(sLoad.Substring(GetTypeString().Length).Replace(";", ","));
			fMin = lockRangeSL.fMin;
			fMax = lockRangeSL.fMax;
			fTop = lockRangeSL.fTop;
			fBtn = lockRangeSL.fBtn;
			nType = lockRangeSL.nType;
			fSpeed = lockRangeSL.fSpeed;
			nSetID = lockRangeSL.nSetID;
			if (lockRangeSL.sMsg != "")
			{
				string[] array = lockRangeSL.sMsg.Split(',');
				int num = 0;
				if (array.Length >= num + 1)
				{
					bLockNet = GetBoolBySaveStr(array[num]);
				}
				num++;
				if (array.Length >= num + 1)
				{
					float.TryParse(array[1], out fOY);
				}
				num++;
				if (array.Length >= num + 1)
				{
					bUseBoxCollider2D = GetBoolBySaveStr(array[num]);
				}
				num++;
				if (array.Length >= num + 1)
				{
					bSlowWhenMove = GetBoolBySaveStr(array[num]);
				}
				num++;
				if (array.Length >= num + 1)
				{
					bFirstTImeNoSlowWhenMove = GetBoolBySaveStr(array[num]);
				}
				num++;
				if (array.Length >= num + 1)
				{
					bChangeToBattleHint = GetBoolBySaveStr(array[num]);
				}
				num++;
				if (array.Length >= num + 1)
				{
					bSavePlayerIDFalg = GetBoolBySaveStr(array[num]);
				}
				num++;
				if (array.Length >= num + 1)
				{
					bChangeToGoArrowHint = GetBoolBySaveStr(array[num]);
				}
			}
			EventB2D = GetComponent<BoxCollider2D>();
			EventB2D.offset = new Vector2(lockRangeSL.B2DX, lockRangeSL.B2DY);
			EventB2D.size = new Vector2(lockRangeSL.B2DW, lockRangeSL.B2DH);
		}

		public override void SyncNowStatus()
		{
			StageUpdate.SyncStageObj(sSyncID, 2, GetBoolSaveStr(bCheck), true);
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
				if (StageUpdate.bWaitReconnect)
				{
					return;
				}
				break;
			case 2:
				bCheck = GetBoolBySaveStr(smsg);
				return;
			case 3:
				StageUpdate.RemoveAllLockRange();
				StageResManager.RemoveAllLockEvent();
				break;
			}
			string[] array = smsg.Split(',');
			if (bCheck || nKey1 == 1 || nKey1 == 3 || nKey1 == 4)
			{
				Vector3 value = default(Vector3);
				value.x = float.Parse(array[1]);
				value.y = float.Parse(array[2]);
				value.z = float.Parse(array[3]);
				StageUpdate stageUpdate = StageResManager.GetStageUpdate();
				TriggerLockEventData triggerLockEventData;
				if (stageUpdate != null)
				{
					if (nKey1 == 3)
					{
						triggerLockEventData = new TriggerLockEventData();
						triggerLockEventData.sTriggerPlayerID = "";
						triggerLockEventData.vTriggerPos = value;
						triggerLockEventData.fMax = 9999f;
						triggerLockEventData.fMin = -9999f;
						triggerLockEventData.fBtn = -9999f;
						triggerLockEventData.fTop = 9999f;
						triggerLockEventData.nType = 5;
						triggerLockEventData.bLockNet = false;
						triggerLockEventData.fWaitLockTime = 0f;
						StageResManager.RegisterLockEvent(triggerLockEventData);
					}
					string text = array[0] + "," + array[1] + "," + array[2] + "," + array[3];
					if (array.Length > 4 && array[4] != "")
					{
						for (int i = 4; i < array.Length; i++)
						{
							string text2 = array[i];
							if (text2.Contains("#-#"))
							{
								text = text2.Substring(text2.IndexOf("#-#") + "#-#".Length);
								text2 = text2.Substring(0, text2.IndexOf("#-#"));
								text = text + "," + array[1] + "," + array[2] + "," + array[3];
							}
							else
							{
								text = array[0] + "," + array[1] + "," + array[2] + "," + array[3];
							}
							if (nKey1 == 3)
							{
								stageUpdate.OnSyncStageObj(text2.ToString(), 4, text);
							}
							else if (!StageUpdate.CheckLockRangeList(text2))
							{
								stageUpdate.OnSyncStageObj(text2.ToString(), 1, text);
							}
						}
					}
				}
				if (IsSelfLockRange())
				{
					StageUpdate.AddLockRangeList(sSyncID, array[0]);
					Debug.LogWarning("DEBUG NetDiv RegisterLockEvent : " + base.gameObject.name + " , " + sSyncID + " , nKey1 = " + nKey1);
					if (array[0] != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
					{
						return;
					}
				}
				else
				{
					StageUpdate.AddLockRangeList(sSyncID);
					bCheck = false;
				}
				triggerLockEventData = new TriggerLockEventData();
				triggerLockEventData.sTriggerPlayerID = array[0];
				triggerLockEventData.sSyncID = sSyncID;
				triggerLockEventData.vTriggerPos = value;
				triggerLockEventData.fMax = fMax;
				triggerLockEventData.fMin = fMin;
				triggerLockEventData.fBtn = fBtn;
				triggerLockEventData.fTop = fTop;
				triggerLockEventData.nType = nType;
				triggerLockEventData.fSpeed = fSpeed;
				triggerLockEventData.bLockNet = bLockNet;
				triggerLockEventData.fOY = fOY;
				if (nKey1 == 1 || nKey1 == 4)
				{
					triggerLockEventData.fWaitLockTime = 0f;
				}
				else
				{
					triggerLockEventData.fWaitLockTime = 3f;
					if ((bChangeToBattleHint || bChangeToGoArrowHint) && BattleInfoUI.Instance != null)
					{
						EventManager.BattleInfoUpdate battleInfoUpdate = new EventManager.BattleInfoUpdate();
						if (bChangeToBattleHint)
						{
							battleInfoUpdate.nType = 1;
						}
						if (bChangeToGoArrowHint)
						{
							battleInfoUpdate.nType = 2;
						}
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_INFO_UPDATE, battleInfoUpdate);
					}
				}
				if (bFirstTImeNoSlowWhenMove)
				{
					bFirstTImeNoSlowWhenMove = false;
					triggerLockEventData.bSlowWhenMove = false;
				}
				else
				{
					triggerLockEventData.bSlowWhenMove = bSlowWhenMove;
				}
				StageResManager.RegisterLockEvent(triggerLockEventData);
			}
			else
			{
				Vector3 vector = default(Vector3);
				vector.x = float.Parse(array[1]);
				vector.y = float.Parse(array[2]);
				vector.z = float.Parse(array[3]);
				if (!StageUpdate.CheckLockRangeArray(array, 4))
				{
					StageResManager.GetStageUpdate().OnSyncStageObj(sSyncID, 3, smsg);
				}
			}
		}
	}
}
