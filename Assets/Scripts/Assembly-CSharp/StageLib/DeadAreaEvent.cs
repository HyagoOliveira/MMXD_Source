using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class DeadAreaEvent : EventPointBase
	{
		public class DeadAreaSL
		{
			public bool bCheckPlayer = true;

			public bool bCheckEnemy = true;

			public bool bMoveByNotify = true;

			public bool bEnemyByNotify = true;

			public int nType;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DX;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DY;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DW = 1f;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DH = 1f;

			public List<Vector3> ReturnPoints = new List<Vector3>();
		}

		public bool bCheckPlayer = true;

		public bool bCheckEnemy = true;

		public bool bMoveByNotify = true;

		public bool bEnemyByNotify = true;

		public int nType;

		private List<string> listEventPlayers = new List<string>();

		private const float fDeadWaitTime = 1f;

		public PosShowSet ReturnObjs = new PosShowSet();

		private Vector3 tNowPos;

		private Vector3 tLastPos;

		private Vector3 vMax;

		private Vector3 vMin;

		private void Awake()
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.RemoveDeadAreaEvent>(EventManager.ID.REMOVE_DEAD_AREA_EVENT, RemovePlayerInEventByEventManager);
		}

		private void OnDestroy()
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.RemoveDeadAreaEvent>(EventManager.ID.REMOVE_DEAD_AREA_EVENT, RemovePlayerInEventByEventManager);
		}

		protected override void UpdateEvent()
		{
			if (!bCheck)
			{
				return;
			}
			vMax = EventB2D.bounds.max;
			vMin = EventB2D.bounds.min;
			if (bCheckPlayer)
			{
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					if (!StageUpdate.runPlayers[num].bIsNpcCpy)
					{
						if (!StageUpdate.runPlayers[num].UsingVehicle)
						{
							if (StageUpdate.runPlayers[num].Controller.Collider2D.enabled)
							{
								tLastPos = StageUpdate.runPlayers[num].vLastMovePt;
								tNowPos = StageUpdate.runPlayers[num].transform.position;
								if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
								{
									if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
									{
										OnEventPlayer(StageUpdate.runPlayers[num]);
									}
									else if ((tLastPos.y < vMin.y && tNowPos.y > vMax.y) || (tLastPos.y > vMax.y && tNowPos.y < vMin.y))
									{
										OnEventPlayer(StageUpdate.runPlayers[num]);
									}
								}
								else if (vMin.y < tNowPos.y && vMax.y > tNowPos.y)
								{
									if (vMin.x < tNowPos.x && vMax.x > tNowPos.x)
									{
										OnEventPlayer(StageUpdate.runPlayers[num]);
									}
									else if ((tLastPos.x < vMin.x && tNowPos.x > vMax.x) || (tLastPos.x > vMax.x && tNowPos.x < vMin.x))
									{
										OnEventPlayer(StageUpdate.runPlayers[num]);
									}
								}
							}
						}
						else
						{
							RideArmorController component = StageUpdate.runPlayers[num].transform.root.GetComponent<RideArmorController>();
							if (component != null)
							{
								tNowPos = component.transform.position;
								if (component.Controller.Collider2D.enabled && vMax.x > tNowPos.x && vMax.y > tNowPos.y && vMin.x < tNowPos.x && vMin.y < tNowPos.y)
								{
									component.PlayerDead();
								}
							}
						}
					}
				}
			}
			if (!bCheckEnemy)
			{
				return;
			}
			for (int num2 = StageUpdate.runEnemys.Count - 1; num2 >= 0; num2--)
			{
				Vector3 position = StageUpdate.runEnemys[num2].mEnemy.transform.position;
				if (vMax.x > position.x && vMax.y > position.y && vMin.x < position.x && vMin.y < position.y)
				{
					OnEventEnemy(StageUpdate.runEnemys[num2].mEnemy);
				}
			}
		}

		public void AddOneReturnPoint(Vector3? setPos = null)
		{
			ReturnObjs.AddShowObj(base.gameObject, "重生點", new Color(0.3f, 0.3f, 0.9f), setPos);
		}

		private void RemovePlayerInEventByEventManager(EventManager.RemoveDeadAreaEvent tRemoveDeadAreaEvent)
		{
			RemovePlayerInEvent(tRemoveDeadAreaEvent.tOC);
		}

		private void RemovePlayerInEvent(OrangeCharacter tPlayer)
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (listEventPlayers.Contains(tPlayer.sPlayerID))
			{
				listEventPlayers.Remove(tPlayer.sPlayerID);
				tPlayer.GetComponent<LockRangeObj>().enabled = true;
				tPlayer.DeadAreaLockInputing = false;
			}
			if (stageUpdate != null && stageUpdate.tDictDeadAreaEventCoroutine.ContainsKey(tPlayer.GetInstanceID()))
			{
				stageUpdate.StopCoroutine(stageUpdate.tDictDeadAreaEventCoroutine[tPlayer.GetInstanceID()]);
				stageUpdate.tDictDeadAreaEventCoroutine.Remove(tPlayer.GetInstanceID());
			}
		}

		private IEnumerator NoNotifyPlayerCoroutine(OrangeCharacter tPlayer, bool bUseLastStandPt = false)
		{
			string sPlayerID = tPlayer.sPlayerID;
			LockRangeObj component = tPlayer.GetComponent<LockRangeObj>();
			if (component == null)
			{
				RemovePlayerInEvent(tPlayer);
				yield break;
			}
			if (!component.enabled)
			{
				RemovePlayerInEvent(tPlayer);
				yield break;
			}
			Vector3 tPos = ((!bUseLastStandPt) ? ReturnObjs.GetCloseReturnPos(tPlayer.transform.position, base.transform.position) : (tPlayer.vLastStandPt + Vector3.up));
			component.enabled = false;
			tPlayer.StopPlayer();
			tPlayer.DeadAreaLockInputing = true;
			tPlayer.DeadAreaLockEvt.CheckTargetToInvoke(true);
			while (MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera == null)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			while (tPlayer.Controller.Collider2D.bounds.max.y > MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position.y - ManagedSingleton<StageHelper>.Instance.fCameraHHalf)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0 && tPlayer.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && (int)tPlayer.Hp > 0 && tPlayer.HurtPercent(ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID].n_ENV_DAMAGE) > 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Play("HitSE", 71);
			}
			float fStartTime = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup - fStartTime <= 0.2f)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			Vector3 vPlayerLockPos = tPlayer.transform.position;
			while (Time.realtimeSinceStartup - fStartTime <= 0.5f)
			{
				tPlayer.transform.localPosition = vPlayerLockPos;
				tPlayer.vLastMovePt = vPlayerLockPos;
				tPlayer.Controller.LogicPosition = new VInt3(vPlayerLockPos);
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (tPlayer.UsingVehicle)
			{
				tPlayer.PlayerPressSelectCB();
			}
			else if (!tPlayer.IsStandJumpCB())
			{
				tPlayer.PlayerPressJumpCB();
				tPlayer.ConnectStandardCtrlCB();
			}
			while (Time.realtimeSinceStartup - fStartTime <= 1f || (int)tPlayer.Hp <= 0)
			{
				tPlayer.transform.localPosition = vPlayerLockPos;
				tPlayer.vLastMovePt = vPlayerLockPos;
				tPlayer.Controller.LogicPosition = new VInt3(vPlayerLockPos);
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			tPlayer.Controller.LogicPosition = new VInt3(tPos);
			tPlayer.transform.localPosition = tPos;
			if (bUseLastStandPt)
			{
				tPos -= Vector3.up;
			}
			tPlayer.vLastMovePt = tPos;
			tPlayer.vLastStandPt = tPos;
			tPlayer.DeadAreaLockInputing = false;
			tPlayer.DeadAreaLockEvt.CheckTargetToInvoke(false);
			Bounds bounds = tPlayer.Controller.Collider2D.bounds;
			if (tPlayer.IsLocalPlayer)
			{
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
			RemovePlayerInEvent(tPlayer);
		}

		public void OnEventPlayer(OrangeCharacter tPlayer)
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (bMoveByNotify)
			{
				for (int i = 0; i < listEventPlayers.Count; i++)
				{
					if (listEventPlayers[i] == tPlayer.sPlayerID)
					{
						return;
					}
				}
				listEventPlayers.Add(tPlayer.sPlayerID);
				if (!stageUpdate.tDictDeadAreaEventCoroutine.ContainsKey(tPlayer.GetInstanceID()))
				{
					stageUpdate.tDictDeadAreaEventCoroutine.Add(tPlayer.GetInstanceID(), stageUpdate.StartCoroutine(NoNotifyPlayerCoroutine(tPlayer, true)));
				}
				else
				{
					listEventPlayers.Remove(tPlayer.sPlayerID);
				}
				return;
			}
			for (int j = 0; j < listEventPlayers.Count; j++)
			{
				if (listEventPlayers[j] == tPlayer.sPlayerID)
				{
					return;
				}
			}
			listEventPlayers.Add(tPlayer.sPlayerID);
			if (!stageUpdate.tDictDeadAreaEventCoroutine.ContainsKey(tPlayer.GetInstanceID()))
			{
				stageUpdate.tDictDeadAreaEventCoroutine.Add(tPlayer.GetInstanceID(), stageUpdate.StartCoroutine(NoNotifyPlayerCoroutine(tPlayer)));
			}
			else
			{
				listEventPlayers.Remove(tPlayer.sPlayerID);
			}
		}

		public void OnEventEnemy(EnemyControllerBase tEnemy)
		{
			if ((int)tEnemy.Hp > 0 && !tEnemy.IsImmunityDeadArea())
			{
				tEnemy.bNeedDead = true;
			}
			bool bEnemyByNotify2 = bEnemyByNotify;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.3f, 0.5f, 0.9f);
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		public override int GetTypeID()
		{
			return 9;
		}

		public override string GetTypeString()
		{
			return StageObjType.DEADAREA_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			DeadAreaSL deadAreaSL = new DeadAreaSL();
			deadAreaSL.bCheckPlayer = bCheckPlayer;
			deadAreaSL.bCheckEnemy = bCheckEnemy;
			deadAreaSL.bMoveByNotify = bMoveByNotify;
			deadAreaSL.bEnemyByNotify = bEnemyByNotify;
			deadAreaSL.nType = nType;
			deadAreaSL.B2DX = EventB2D.offset.x;
			deadAreaSL.B2DY = EventB2D.offset.y;
			deadAreaSL.B2DW = EventB2D.size.x;
			deadAreaSL.B2DH = EventB2D.size.y;
			for (int i = 0; i < ReturnObjs.Count; i++)
			{
				deadAreaSL.ReturnPoints.Add(ReturnObjs.GetPosByIndex(i));
			}
			string text = JsonConvert.SerializeObject(deadAreaSL, Formatting.None, JsonHelper.IgnoreLoopSetting());
			text = text.Replace(",", ";");
			return typeString + text;
		}

		public override void LoadByString(string sLoad)
		{
			DeadAreaSL deadAreaSL = JsonUtility.FromJson<DeadAreaSL>(sLoad.Substring(GetTypeString().Length).Replace(";", ","));
			bCheckPlayer = deadAreaSL.bCheckPlayer;
			bCheckEnemy = deadAreaSL.bCheckEnemy;
			bMoveByNotify = deadAreaSL.bMoveByNotify;
			bEnemyByNotify = deadAreaSL.bEnemyByNotify;
			nType = deadAreaSL.nType;
			EventB2D = GetComponent<BoxCollider2D>();
			EventB2D.offset = new Vector2(deadAreaSL.B2DX, deadAreaSL.B2DY);
			EventB2D.size = new Vector2(deadAreaSL.B2DW, deadAreaSL.B2DH);
			for (int i = 0; i < deadAreaSL.ReturnPoints.Count; i++)
			{
				AddOneReturnPoint(deadAreaSL.ReturnPoints[i]);
			}
		}

		public override bool IsNeedClip()
		{
			return false;
		}
	}
}
