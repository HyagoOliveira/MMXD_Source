using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageRebounEvent : EventPointBase
	{
		public class StageRebounSL
		{
			[JsonConverter(typeof(FloatConverter))]
			public float B2DX;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DY;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DW = 1f;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DH = 1f;

			public List<Vector3> ReturnPoints = new List<Vector3>();

			public string sSave = "";
		}

		public enum BIT_SET
		{
			NONE = 0,
			CHECK_ENTER_RANGE = 1,
			WAIT_REBORN = 2,
			DELAY_DEADUI = 4
		}

		public PosShowSet ReturnObjs = new PosShowSet();

		public int nBitParam;

		public int nRebornEventID;

		public int nRebornEventID2;

		public float fRebornWaitTime;

		private Bounds tmpBoundA;

		private Bounds tmpBoundB;

		private List<string> sRebounIDList = new List<string>();

		public void Awake()
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<OrangeCharacter, bool>(EventManager.ID.STAGE_PLAYER_DESTROY_ED, UnRegisterPlayer);
			Singleton<GenericEventManager>.Instance.AttachEvent<OrangeCharacter, Vector3>(EventManager.ID.STAGE_REBORNEVENT, GoRebornPos);
		}

		public void OnDestroy()
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<OrangeCharacter, bool>(EventManager.ID.STAGE_PLAYER_DESTROY_ED, UnRegisterPlayer);
			Singleton<GenericEventManager>.Instance.DetachEvent<OrangeCharacter, Vector3>(EventManager.ID.STAGE_REBORNEVENT, GoRebornPos);
		}

		protected override void UpdateEvent()
		{
			if ((nBitParam & 1) == 0)
			{
				return;
			}
			tmpBoundA = EventB2D.bounds;
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				if (!(StageUpdate.runPlayers[num].refRideBaseObj != null) && StageUpdate.runPlayers[num].IsCanTriggerEvent())
				{
					tmpBoundB = StageUpdate.runPlayers[num].Controller.Collider2D.bounds;
					if (StageResManager.CheckBoundsIntersectNoZEffect(ref tmpBoundA, ref tmpBoundB))
					{
						OnEvent(StageUpdate.runPlayers[num].transform);
					}
				}
			}
		}

		public override void OnEvent(Transform TriggerTransform)
		{
			OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
			if (!sRebounIDList.Contains(component.sNetSerialID))
			{
				sRebounIDList.Add(component.sNetSerialID);
				StartCoroutine(ReBounCoroutine(component));
			}
		}

		public void UnRegisterPlayer(OrangeCharacter tPlayer, bool bNeedRemove)
		{
			if (!bNeedRemove)
			{
				Vector3 max = EventB2D.bounds.max;
				Vector3 min = EventB2D.bounds.min;
				Vector3 position = tPlayer.transform.position;
				if (!(max.x < position.x) && !(max.y < position.y) && !(min.x > position.x) && !(min.y > position.y) && !sRebounIDList.Contains(tPlayer.sNetSerialID))
				{
					sRebounIDList.Add(tPlayer.sNetSerialID);
					StartCoroutine(ReBounCoroutine(tPlayer));
				}
			}
		}

		private IEnumerator ReBounCoroutine(OrangeCharacter tPlayer)
		{
			StageUpdate tStageUpdate = StageResManager.GetStageUpdate();
			if (nRebornEventID != 0)
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = nRebornEventID;
				stageEventCall.tTransform = tPlayer.transform;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
			if (((uint)nBitParam & 4u) != 0)
			{
				tStageUpdate.StopPlayerRebornTask(tPlayer, false);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
			if (fRebornWaitTime > 0f)
			{
				for (float fWaitTime = fRebornWaitTime; fWaitTime >= 0f; fWaitTime -= Time.deltaTime)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			if (((uint)nBitParam & 4u) != 0)
			{
				tStageUpdate.PlayerRebornTask(tPlayer, false);
			}
			if (((uint)nBitParam & 2u) != 0)
			{
				while (tPlayer.IsDead())
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			if (nRebornEventID2 != 0)
			{
				EventManager.StageEventCall stageEventCall2 = new EventManager.StageEventCall();
				stageEventCall2.nID = nRebornEventID2;
				stageEventCall2.tTransform = tPlayer.transform;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall2);
			}
			Vector3 closeReturnPos = ReturnObjs.GetCloseReturnPos(tPlayer.transform.position, base.transform.position);
			tPlayer.Controller.LogicPosition = new VInt3(closeReturnPos);
			tPlayer.transform.localPosition = closeReturnPos;
			Bounds bounds = tPlayer.Controller.Collider2D.bounds;
			EventManager.LockRangeParam lockRangeParam = new EventManager.LockRangeParam();
			lockRangeParam.fMinX = bounds.min.x;
			lockRangeParam.fMaxX = bounds.max.x;
			lockRangeParam.fMinY = bounds.min.y;
			lockRangeParam.fMaxY = bounds.max.y;
			lockRangeParam.nNoBack = 0;
			lockRangeParam.fSpeed = 0f;
			lockRangeParam.bSetFocus = false;
			lockRangeParam.nMode = 1;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCK_RANGE, lockRangeParam);
			sRebounIDList.Remove(tPlayer.sNetSerialID);
		}

		public void GoRebornPos(OrangeCharacter tPlayer, Vector3 tPos)
		{
			Vector3 max = EventB2D.bounds.max;
			Vector3 min = EventB2D.bounds.min;
			if (!(max.x < tPos.x) && !(max.y < tPos.y) && !(min.x > tPos.x) && !(min.y > tPos.y))
			{
				tPos = ReturnObjs.GetCloseReturnPos(tPlayer.transform.position, base.transform.position);
				tPlayer.Controller.LogicPosition = new VInt3(tPos);
				tPlayer.transform.localPosition = tPos;
				if (tPlayer.sNetSerialID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					Bounds bounds = tPlayer.Controller.Collider2D.bounds;
					EventManager.LockRangeParam lockRangeParam = new EventManager.LockRangeParam();
					lockRangeParam.fMinX = bounds.min.x;
					lockRangeParam.fMaxX = bounds.max.x;
					lockRangeParam.fMinY = bounds.min.y;
					lockRangeParam.fMaxY = bounds.max.y;
					lockRangeParam.nNoBack = 0;
					lockRangeParam.fSpeed = 0f;
					lockRangeParam.bSetFocus = false;
					lockRangeParam.nMode = 1;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCK_RANGE, lockRangeParam);
				}
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.3f, 0.7f, 0.6f);
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		public void AddOneReturnPoint(Vector3? setPos = null)
		{
			ReturnObjs.AddShowObj(base.gameObject, "重生點", new Color(0.3f, 0.3f, 0.9f), setPos);
		}

		public override int GetTypeID()
		{
			return 15;
		}

		public override string GetTypeString()
		{
			return StageObjType.STAGEREBORN_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			StageRebounSL stageRebounSL = new StageRebounSL();
			stageRebounSL.B2DX = EventB2D.offset.x;
			stageRebounSL.B2DY = EventB2D.offset.y;
			stageRebounSL.B2DW = EventB2D.size.x;
			stageRebounSL.B2DH = EventB2D.size.y;
			for (int i = 0; i < ReturnObjs.Count; i++)
			{
				stageRebounSL.ReturnPoints.Add(ReturnObjs.GetPosByIndex(i));
			}
			stageRebounSL.sSave = nBitParam.ToString();
			stageRebounSL.sSave = stageRebounSL.sSave + "," + nRebornEventID;
			stageRebounSL.sSave = stageRebounSL.sSave + "," + nRebornEventID2;
			stageRebounSL.sSave = stageRebounSL.sSave + "," + fRebornWaitTime;
			string text = JsonConvert.SerializeObject(stageRebounSL, Formatting.None, JsonHelper.IgnoreLoopSetting());
			text = text.Replace(",", ";");
			return typeString + text;
		}

		public override void LoadByString(string sLoad)
		{
			StageRebounSL stageRebounSL = JsonUtility.FromJson<StageRebounSL>(sLoad.Substring(GetTypeString().Length).Replace(";", ","));
			EventB2D = GetComponent<BoxCollider2D>();
			EventB2D.offset = new Vector2(stageRebounSL.B2DX, stageRebounSL.B2DY);
			EventB2D.size = new Vector2(stageRebounSL.B2DW, stageRebounSL.B2DH);
			for (int i = 0; i < stageRebounSL.ReturnPoints.Count; i++)
			{
				AddOneReturnPoint(stageRebounSL.ReturnPoints[i]);
			}
			if (stageRebounSL.sSave != "")
			{
				string[] array = stageRebounSL.sSave.Split(',');
				nBitParam = 0;
				if (array.Length != 0)
				{
					nBitParam = int.Parse(array[0]);
				}
				nRebornEventID = 0;
				if (array.Length > 1)
				{
					nRebornEventID = int.Parse(array[1]);
				}
				nRebornEventID2 = 0;
				if (array.Length > 2)
				{
					nRebornEventID2 = int.Parse(array[2]);
				}
				fRebornWaitTime = 0f;
				if (array.Length > 3)
				{
					fRebornWaitTime = float.Parse(array[3]);
				}
			}
		}

		public override bool IsNeedClip()
		{
			return false;
		}
	}
}
