#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StageLib
{
	[ExecuteInEditMode]
	public class MapObjEvent : EventPointBase
	{
		[Serializable]
		public class MapObjData
		{
			public int mapEvent;

			public Vector3 MoveToPos;

			public float fDelayTime;

			public float fMoveTime;

			public bool bLoop;

			public int nType;

			public bool bCheckPlayer = true;

			public bool bCheckEnemy;

			public bool bCheckRideObj = true;

			public bool bRunAtInit;

			public bool bUB2D = true;

			public int nCommonBit;

			public string bmgs = "";

			public string bmge = "";

			public int nSetID;

			public AnimationCurve mspd = new AnimationCurve();

			public float B2DX;

			public float B2DY;

			public float B2DW = 1f;

			public float B2DH = 1f;

			public List<StageObjData> Datas = new List<StageObjData>();
		}

		public enum MapEventEnum
		{
			NONE = 0,
			MOVETO = 1,
			TELEPORT = 2,
			PLAYEFFECT = 3,
			STAGEEND = 4,
			MOVETOBYTOUCH = 5,
			ADDFORCE = 6,
			DISAPPERLOOP = 7,
			TRANSPARENTBYIN = 8,
			PLAYBGM = 9,
			CAMERA_FOV = 10,
			MOVECURVE = 11,
			MOVEREPEATE = 12,
			CONTINUEMOVE = 13,
			START_FALL = 14,
			TRIGGER_SKILL = 15,
			STAGE_ITEM = 16,
			PLAYANIMATION = 17,
			CLOSEOBJCOLLIDER = 18,
			OPENOBJCOLLIDER = 19,
			SWITCH_CTRLBTN = 20,
			SWITCH_MEMOBJ = 21,
			PLAY_FLAG_MODE = 22,
			CHANGE_WEATHER = 23,
			FLAG_SETTING = 24,
			RIDE_OBJ = 25,
			PATH_OBJ_MOVE = 26,
			CIRCLE_OBJ_MOVE = 27,
			ANGLE_OBJ_MOVE = 28,
			CALL_EVENT_ID = 29,
			ROTATE_TARGETOBJ = 30,
			SHOW_RANKFX = 31,
			SET_OBJ_STATUS = 32,
			ADD_STAGE_QUEST = 33,
			INRANGE_BUFF = 34,
			RIDET_OBJ = 35,
			PERIOD_CALL = 36,
			SEQ_DESTORYLOOP = 37,
			COUNTDOWN_EVENT = 38,
			RE_CALL_EVENT = 39,
			SIMULATE_FALLOBJ = 40,
			SHOT_ITEM_BULLET = 41,
			SWING_HAMMER = 42,
			COUNTUP_EVENT = 43,
			TRANSPARENTBYIN2 = 44,
			MAX_NUM = 45
		}

		private class TrigerData
		{
			public delegate void TrigerCB(TrigerData tTrigerData);

			public Transform tTriggerTransform;

			public float fTimeLeft;

			public Vector3 EndPos;

			public int nParam1;

			public object tObj1;

			public object tObj2;

			public List<Transform> listTriCollisionTrans = new List<Transform>();

			public TrigerCB tTrigerCBNoParam;

			public Callback BackCB;

			public Vector3 vMax = Vector3.zero;

			public Vector3 vMin = Vector3.zero;

			public void CallCBNoParam()
			{
				if (tTrigerCBNoParam != null)
				{
					tTrigerCBNoParam(this);
				}
			}
		}

		[Serializable]
		public class MutliMove
		{
			[JsonConverter(typeof(Vector3Converter))]
			public Vector3 tMovePos;

			[JsonConverter(typeof(FloatConverter))]
			public float fMoveTime;

			[JsonConverter(typeof(FloatConverter))]
			public float fDT;

			public int nIntParam1;
		}

		[Serializable]
		public class MutliAni
		{
			public string aniName = "";

			public int nIntParam1;

			public int nIntParam2;
		}

		[Serializable]
		public class MutliSubSave
		{
			public List<MutliMove> listMutliMove = new List<MutliMove>();

			public List<MutliAni> listMutliAni = new List<MutliAni>();

			public int nBitParam0;
		}

		private delegate void StartUpdateCall(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null);

		private delegate void MapObjUpdateCall();

		private enum TIMELINE_STATE
		{
			TS_INIT = 0,
			TS_START = 1,
			TS_MOVE01 = 2,
			TS_MOVE02 = 3,
			TS_MOVE03 = 4,
			TS_MOVE04 = 5,
			TS_MOVE05 = 6,
			TS_MOVE06 = 7,
			TS_MOVE07 = 8,
			TS_MOVE08 = 9,
			TS_MOVE09 = 10,
			TS_MOVE10 = 11,
			TS_END = 12,
			TS_RESTART = 13,
			TS_PAUSE = 14,
			TS_CONTINUE = 15,
			TS_MAX = 16
		}

		private enum SYNC_TYPE
		{
			SYNC_TIME = 0,
			SYNC_EVENT = 1,
			SYNC_RECONNECT = 2
		}

		private enum SYNC_FLAG_TYPE
		{
			NONE = 0,
			SYNC_CHANGE = 1,
			SYNC_TIME_SCORE = 2
		}

		private enum CHECK_DIR
		{
			TOP = 0,
			DOWN = 1,
			RIGHT = 2,
			LEFT = 3
		}

		public MapEventEnum mapEvent;

		public float fDelayTime;

		public float fMoveTime;

		public bool bLoop;

		public int nType;

		public bool bCheckPlayer = true;

		public bool bCheckEnemy;

		public bool bCheckRideObj = true;

		public bool bRunAtInit;

		public string bmgs = "";

		public string bmge = "";

		public AnimationCurve mspd = new AnimationCurve();

		public int nCommonBit;

		public Action MoveObjStart;

		public Action MoveObjEnd;

		private bool bMovePlay;

		private MapObjData LoadMapObjData;

		private bool bStartUpdate;

		private bool bDiedPause;

		private Vector3 EventInitPos;

		private Vector3 StartPos;

		private Vector3 NowPos;

		private Vector3 EndPos;

		private List<GameObject> tmplistNeedUnMove;

		private GameObject[] RuntimeNeedUnMove;

		private List<Transform> listCollisionTrans = new List<Transform>();

		private Vector3 vMoveVector;

		private float fMoveSpeed;

		private float fTimeLeft;

		private float fTimeLeftDelay;

		private float fNowUpdateTimeDelta;

		private float fTotalTimeLeft;

		private bool isFlagSEing;

		private const float fPathBias = 0.005f;

		private StageUpdate.MixBojSyncData tMixBojSyncData;

		public Vector3? tMoveToObj;

		private List<TrigerData> TrigerDatas = new List<TrigerData>();

		private List<TrigerData> OldTrigerDatas = new List<TrigerData>();

		private float[] ArrayFloats;

		private uint[] ArrayUInts;

		private CountDownBar tCountDownBar;

		private Bounds tmpBoundA;

		private Bounds tmpBoundB;

		private Color tTmpColor;

		public List<MutliMove> listMutliMove = new List<MutliMove>();

		public List<MutliAni> listMutliAni = new List<MutliAni>();

		public int nBitParam0;

		private string[] DisapperIn;

		private string[] DisapperOut;

		private int nMoveStep;

		private int nCallCount;

		private List<Controller2D> listController2D = new List<Controller2D>();

		private StartUpdateCall[] StartUpdateCalls;

		private MapObjUpdateCall[] updateCalls;

		private TIMELINE_STATE ts_State;

		public override void Init()
		{
			updateCalls = new MapObjUpdateCall[46];
			StartUpdateCalls = new StartUpdateCall[46];
			for (int i = 0; i < updateCalls.Length; i++)
			{
				updateCalls[i] = NoneCall;
				StartUpdateCalls[i] = null;
			}
			StartUpdateCalls[1] = StartMoveTo;
			StartUpdateCalls[2] = StartUpdateTeleport;
			StartUpdateCalls[3] = StartPlayEffect;
			StartUpdateCalls[4] = StartStageEnd;
			StartUpdateCalls[5] = StartMoveToByTouch;
			StartUpdateCalls[6] = StartAddForce;
			StartUpdateCalls[7] = StartDisapperLoop;
			StartUpdateCalls[8] = StartTransParentByIn;
			StartUpdateCalls[9] = StartPlayBgm;
			StartUpdateCalls[10] = StartCameraFov;
			StartUpdateCalls[11] = StartMoveCurve;
			StartUpdateCalls[12] = StartMoveRepeate;
			StartUpdateCalls[13] = StartContinueMove;
			StartUpdateCalls[14] = StartStartFall;
			StartUpdateCalls[15] = StartTriggerSkill;
			StartUpdateCalls[16] = StartStageItem;
			StartUpdateCalls[17] = StartPlayAnimation;
			StartUpdateCalls[18] = StartCloseObjCollider;
			StartUpdateCalls[19] = StartOpenObjCollider;
			StartUpdateCalls[20] = StartSwitchCtrlBtn;
			StartUpdateCalls[21] = StartSwitchMemObj;
			StartUpdateCalls[22] = StartPlayFlagMode;
			StartUpdateCalls[23] = StartChangeWeather;
			StartUpdateCalls[24] = StartFlagSetting;
			StartUpdateCalls[25] = StartRideObj;
			StartUpdateCalls[26] = StartPathObjMove;
			StartUpdateCalls[27] = StartCircleObjMove;
			StartUpdateCalls[28] = StartAngleObjMove;
			StartUpdateCalls[29] = StartCallEventID;
			StartUpdateCalls[30] = StartRotateTargetObj;
			StartUpdateCalls[31] = StartShowRankFX;
			StartUpdateCalls[32] = StartSetObjStatus;
			StartUpdateCalls[33] = StartAddStageQuest;
			StartUpdateCalls[34] = StartInRangeBuff;
			StartUpdateCalls[35] = StartRidetObj;
			StartUpdateCalls[36] = StartPeriodCall;
			StartUpdateCalls[37] = StartSeqDestoryLoop;
			StartUpdateCalls[38] = StartCountDownEvent;
			StartUpdateCalls[39] = StartReCallEvent;
			StartUpdateCalls[40] = StartSimulateFallObj;
			StartUpdateCalls[41] = StartShotItemBullet;
			StartUpdateCalls[42] = StartSwingHammer;
			StartUpdateCalls[43] = StartCountUpEvent;
			StartUpdateCalls[44] = StartTransParentByIn2;
			updateCalls[1] = MoveMapTo;
			updateCalls[2] = TeleportTo;
			updateCalls[3] = PlayEffect;
			updateCalls[5] = MoveMapTo;
			updateCalls[6] = AddForceRange;
			updateCalls[7] = DisapperLoop;
			updateCalls[8] = TransparentByObjIn;
			updateCalls[11] = MoveCruve;
			updateCalls[12] = MoveMapRepeate;
			updateCalls[13] = MoveMapContiune;
			updateCalls[15] = StageAtkBlock;
			updateCalls[16] = StageItemCD;
			updateCalls[21] = SwitchMemObj;
			updateCalls[22] = PlayFlagCheck;
			updateCalls[25] = RideObj;
			updateCalls[26] = PathObjMove;
			updateCalls[27] = CircleObjMove;
			updateCalls[28] = AngleObjMove;
			updateCalls[30] = RotateTargetObj;
			updateCalls[34] = InRangeBuff;
			updateCalls[35] = RidetObj;
			updateCalls[36] = PeriodCall;
			updateCalls[37] = SeqDestoryLoop;
			updateCalls[38] = CountDownEvent;
			updateCalls[39] = ReCallEvent;
			updateCalls[40] = SimulateFallObj;
			updateCalls[41] = ShotItemBullet;
			updateCalls[42] = SwingHammer;
			updateCalls[43] = CountUpEvent;
			updateCalls[44] = TransparentByObjIn2;
			updateCalls[45] = NoneCall;
			TrigerDatas.Clear();
			OldTrigerDatas.Clear();
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
			EventInitPos = base.transform.position;
			if (bRunAtInit)
			{
				StageUpdate stageUpdate = StageResManager.GetStageUpdate();
				if (stageUpdate != null)
				{
					stageUpdate.StartCoroutine(RunAtInitCoroutine());
				}
			}
		}

		private IEnumerator RunAtInitCoroutine()
		{
			while (!StageUpdate.gbStageReady)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			OnEvent(null);
		}

		private void OnDestroy()
		{
			if (isFlagSEing)
			{
				PlaySE("BattleSE", "bt_pvp01_stop");
				isFlagSEing = false;
			}
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
		}

		private void OnDrawGizmos()
		{
			Color color = new Color(0.3f, 0.9f, 0.9f, 1f);
			new Color(1f, 0f, 1f, 1f);
			new Color(0f, 0f, 1f, 1f);
			Gizmos.color = color;
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		private void DrawArrow(Vector3 vPos, Vector3 vDir)
		{
			Vector3 vector = vPos;
			vector += vDir;
			Vector3 to = vector;
			float x = vDir.x;
			vDir.x = vDir.y;
			vDir.y = 0f - x;
			vector += vDir * 0.5f;
			to -= vDir * 0.5f;
			Gizmos.DrawLine(vPos, vector);
			Gizmos.DrawLine(vPos, to);
			Gizmos.DrawLine(vector, to);
		}

		public override void StopEvent()
		{
		}

		protected override void UpdateEvent()
		{
			if (bCheck && bUseBoxCollider2D)
			{
				if (bCheckPlayer)
				{
					tmpBoundA = EventB2D.bounds;
					for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
					{
						OrangeCharacter orangeCharacter = StageUpdate.runPlayers[num];
						if (!(orangeCharacter.refRideBaseObj != null) && !orangeCharacter.bIsNpcCpy && (int)orangeCharacter.Hp > 0 && orangeCharacter.gameObject.activeSelf && orangeCharacter.Controller.Collider2D.enabled)
						{
							tmpBoundB = orangeCharacter.Controller.Collider2D.bounds;
							if (StageResManager.CheckBoundsIntersectNoZEffect(ref tmpBoundA, ref tmpBoundB))
							{
								OnEvent(orangeCharacter.transform);
							}
						}
					}
				}
				if (bCheckRideObj)
				{
					tmpBoundA = EventB2D.bounds;
					for (int num2 = StageUpdate.runPlayers.Count - 1; num2 >= 0; num2--)
					{
						OrangeCharacter orangeCharacter = StageUpdate.runPlayers[num2];
						if (orangeCharacter.refRideBaseObj != null && !orangeCharacter.refRideBaseObj.bIsNpcCpy && (int)orangeCharacter.refRideBaseObj.Hp > 0 && orangeCharacter.refRideBaseObj.Controller.Collider2D.enabled)
						{
							tmpBoundB = orangeCharacter.refRideBaseObj.Controller.Collider2D.bounds;
							if (StageResManager.CheckBoundsIntersectNoZEffect(ref tmpBoundA, ref tmpBoundB))
							{
								OnEvent(orangeCharacter.refRideBaseObj.transform);
							}
						}
					}
				}
				if (bCheckEnemy)
				{
					tmpBoundA = EventB2D.bounds;
					for (int num3 = StageUpdate.runEnemys.Count - 1; num3 >= 0; num3--)
					{
						StageUpdate.EnemyCtrlID enemyCtrlID = StageUpdate.runEnemys[num3];
						Vector3 position = enemyCtrlID.mEnemy.transform.position;
						if (!enemyCtrlID.mEnemy.bIsNpcCpy && (int)enemyCtrlID.mEnemy.Hp > 0 && enemyCtrlID.mEnemy.Controller.Collider2D.enabled)
						{
							tmpBoundB = enemyCtrlID.mEnemy.Controller.Collider2D.bounds;
							if (StageResManager.CheckBoundsIntersectNoZEffect(ref tmpBoundA, ref tmpBoundB))
							{
								OnEvent(enemyCtrlID.mEnemy.transform);
							}
						}
					}
				}
			}
			OnLateUpdate();
		}

		public override void OnEvent(Transform TriggerTransform)
		{
			if (mapEvent == MapEventEnum.TRIGGER_SKILL)
			{
				fTimeLeftDelay = fDelayTime;
			}
			else if (mapEvent != MapEventEnum.PLAY_FLAG_MODE && mapEvent != MapEventEnum.RIDE_OBJ && mapEvent != MapEventEnum.CONTINUEMOVE && mapEvent != MapEventEnum.ROTATE_TARGETOBJ && mapEvent != MapEventEnum.COUNTDOWN_EVENT && mapEvent != MapEventEnum.SWING_HAMMER && mapEvent != MapEventEnum.COUNTUP_EVENT)
			{
				if (bStartUpdate)
				{
					nCallCount++;
					return;
				}
				if (mapEvent == MapEventEnum.MOVETO && ((uint)nType & (true ? 1u : 0u)) != 0)
				{
					if (nMoveStep == 0 || (bLoop && nMoveStep == listMutliMove.Count + 1))
					{
						fTimeLeftDelay = fDelayTime;
					}
					else if (nMoveStep <= listMutliMove.Count)
					{
						fTimeLeftDelay = listMutliMove[nMoveStep - 1].fDT;
					}
				}
				else
				{
					fTimeLeftDelay = fDelayTime;
				}
			}
			else if (!bStartUpdate)
			{
				fTimeLeftDelay = fDelayTime;
			}
			StartUpdate(base.transform.position, tMoveToObj ?? Vector3.zero, TriggerTransform);
		}

		public void CallOnByID(EventManager.StageEventCall tStageEventCall)
		{
			int nID = tStageEventCall.nID;
			if (nID == 0 || nSetID != nID)
			{
				return;
			}
			Transform transform = tStageEventCall.tTransform;
			if (transform == null)
			{
				transform = StageUpdate.GetMainPlayerTrans();
			}
			if (mapEvent == MapEventEnum.MOVETO && ((uint)nType & 0x40u) != 0)
			{
				if (bStartUpdate)
				{
					bStartUpdate = false;
				}
				else if (bCheck)
				{
					OnEvent(transform);
				}
			}
			else if (mapEvent == MapEventEnum.RIDET_OBJ)
			{
				if (listMutliAni.Count > 0 && listMutliAni[0].nIntParam1 == 0)
				{
					listMutliAni[0].nIntParam1 = 1;
					OnEvent(transform);
				}
			}
			else
			{
				OnEvent(transform);
			}
		}

		private void StartUpdate(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (StartUpdateCalls[(int)mapEvent] != null)
			{
				StartUpdateCalls[(int)mapEvent](tNowEventPos, tMoveToPos, TriggerTransform);
			}
		}

		private void StartMoveTo(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			float fMoveTime2 = fMoveTime;
			ts_State = TIMELINE_STATE.TS_START;
			bStartUpdate = true;
		}

		private void StartUpdateTeleport(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (!(TriggerTransform == null))
			{
				TrigerData trigerData = new TrigerData();
				trigerData.tTriggerTransform = TriggerTransform;
				trigerData.fTimeLeft = fMoveTime;
				trigerData.EndPos = tMoveToPos;
				TrigerDatas.Add(trigerData);
				bStartUpdate = true;
			}
		}

		private void StartPlayEffect(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			foreach (TrigerData oldTrigerData in OldTrigerDatas)
			{
				if (oldTrigerData.tTriggerTransform == TriggerTransform)
				{
					return;
				}
			}
			TrigerData trigerData = new TrigerData();
			trigerData.tTriggerTransform = TriggerTransform;
			trigerData.fTimeLeft = fDelayTime;
			if (TriggerTransform != null)
			{
				trigerData.tTriggerTransform = TriggerTransform;
				trigerData.EndPos = TriggerTransform.position;
			}
			else
			{
				trigerData.tTriggerTransform = base.transform;
				trigerData.EndPos = base.transform.position;
			}
			if (((uint)listMutliAni[0].nIntParam1 & (true ? 1u : 0u)) != 0)
			{
				bLoop = false;
				trigerData.EndPos = base.transform.position;
			}
			TrigerDatas.Add(trigerData);
			bStartUpdate = true;
			fTimeLeftDelay = 0f;
		}

		private void StartStageEnd(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			if (((uint)nType & (true ? 1u : 0u)) != 0)
			{
				string[] array = listMutliAni[0].aniName.Split(',');
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYBGM, listMutliAni[0].nIntParam1, array[0], array[1]);
			}
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(sSyncID, 1, "");
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
		}

		private void StartMoveToByTouch(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			StartPos = tNowEventPos;
			EndPos = tMoveToPos;
			vMoveVector = EndPos - StartPos;
			fMoveSpeed = Vector3.Distance(StartPos, EndPos);
			fMoveSpeed /= fMoveTime;
			vMoveVector = vMoveVector.normalized;
			fTimeLeft = fMoveTime;
			NowPos = base.transform.position;
			CheckNeedUnMove();
			bStartUpdate = true;
		}

		private void StartAddForce(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (TriggerTransform == null)
			{
				return;
			}
			OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
			if (component != null)
			{
				component.AddForce(new VInt3(new Vector3(fDelayTime, fMoveTime, 0f)));
				return;
			}
			RideArmorController component2 = TriggerTransform.GetComponent<RideArmorController>();
			if (component2 != null)
			{
				component2.AddForce(new VInt3(new Vector3(fDelayTime, fMoveTime, 0f)));
				return;
			}
			EnemyControllerBase component3 = TriggerTransform.GetComponent<EnemyControllerBase>();
			if (component3 != null)
			{
				component3.AddForce(new VInt3(new Vector3(fDelayTime, fMoveTime, 0f)));
			}
		}

		private void StartDisapperLoop(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			bStartUpdate = true;
			StageSceneObjParam[] componentsInChildren = base.transform.GetComponentsInChildren<StageSceneObjParam>();
			foreach (StageSceneObjParam stageSceneObjParam in componentsInChildren)
			{
				stageSceneObjParam.CheckAlphaMaterial();
				TrigerDatas.Add(new TrigerData());
				TrigerDatas[TrigerDatas.Count - 1].tObj1 = stageSceneObjParam;
			}
			if (TrigerDatas.Count == 0)
			{
				bStartUpdate = false;
			}
			tTmpColor = Color.white;
			if (((uint)nBitParam0 & (true ? 1u : 0u)) != 0)
			{
				tTmpColor.a = 0f;
			}
			if (((uint)nBitParam0 & 4u) != 0)
			{
				DisapperIn = listMutliAni[1].aniName.Split(',');
				DisapperOut = listMutliAni[0].aniName.Split(',');
			}
		}

		private void StartTransParentByIn(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bStartUpdate = true;
			bCheck = false;
			for (int i = 0; i < listMutliAni.Count; i++)
			{
				Transform transform = base.transform.Find(listMutliAni[i].aniName);
				if (transform != null)
				{
					MeshRenderer component = transform.GetComponent<MeshRenderer>();
					if (component.material.shader.name != "StageLib/DiffuseAlpha")
					{
						Material material = new Material(Shader.Find("StageLib/DiffuseAlpha"));
						material.mainTexture = component.material.mainTexture;
						component.material = material;
					}
					tTmpColor = component.material.GetColor("_Color");
					listMutliAni[i].nIntParam1 = (listMutliAni[i].nIntParam2 = (int)(tTmpColor.a * 100f));
					component.material.SetColor("_Color", tTmpColor);
				}
			}
		}

		private void StartPlayBgm(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			if (nType <= 3)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYBGM, nType, bmgs, bmge);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlayExceptSE(bmgs, bmge);
			}
		}

		private void StartCameraFov(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			CameraControl targetCamera = OrangeSceneManager.FindObjectOfTypeCustom<CameraControl>();
			LeanTween.value(targetCamera.DesignFov, fDelayTime, 0.5f).setOnUpdate(delegate(float f)
			{
				targetCamera.UpdateCameraFov(f);
			});
		}

		private void StartMoveCurve(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			fTimeLeft = fMoveTime;
			NowPos = base.transform.position;
			CheckNeedUnMove();
			AddSelfToTrigger();
			bStartUpdate = true;
		}

		private void StartMoveRepeate(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			StartPos = tNowEventPos;
			EndPos = tMoveToPos;
			vMoveVector = EndPos - StartPos;
			fMoveSpeed = Vector3.Distance(StartPos, EndPos);
			fMoveSpeed /= fMoveTime;
			vMoveVector = vMoveVector.normalized;
			fTimeLeft = fMoveTime;
			base.transform.position = StartPos;
			NowPos = StartPos;
			CheckNeedUnMove();
			AddSelfToTrigger();
			bStartUpdate = true;
		}

		private void StartContinueMove(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (bStartUpdate)
			{
				listMutliMove[0].nIntParam1 = 1;
				return;
			}
			CheckNeedUnMove();
			if (RuntimeNeedUnMove.Length == 0)
			{
				return;
			}
			bCheck = false;
			StartPos = tNowEventPos;
			EndPos = tMoveToPos;
			if (((uint)nType & (true ? 1u : 0u)) != 0 && ((uint)nType & 2u) != 0)
			{
				EndPos.z = StartPos.z;
			}
			else if (((uint)nType & (true ? 1u : 0u)) != 0)
			{
				EndPos.y = StartPos.y;
				EndPos.z = StartPos.z;
			}
			else if (((uint)nType & 2u) != 0)
			{
				EndPos.x = StartPos.x;
				EndPos.z = StartPos.z;
			}
			vMoveVector = EndPos - StartPos;
			fMoveSpeed = Vector3.Distance(StartPos, EndPos);
			fMoveSpeed /= fMoveTime;
			vMoveVector = vMoveVector.normalized;
			fTimeLeft = fMoveTime;
			base.transform.position = StartPos;
			NowPos = StartPos;
			bStartUpdate = true;
			if (listMutliMove[0].fDT != 0f)
			{
				listMutliMove[0].tMovePos.x = fMoveSpeed / (listMutliMove[0].fDT / 0.016f);
			}
			else
			{
				listMutliMove[0].tMovePos.x = 0f;
			}
			if (listMutliMove[0].fMoveTime != 0f)
			{
				listMutliMove[0].tMovePos.y = fMoveSpeed / (listMutliMove[0].fMoveTime / 0.016f);
				listMutliMove[0].tMovePos.z = fMoveSpeed;
				fMoveSpeed = 0f;
			}
			float num = 0f;
			TrigerDatas.Clear();
			Vector3 vMax = Vector3.zero;
			Vector3 vMin = Vector3.zero;
			if (RuntimeNeedUnMove.Length != 0)
			{
				vMax = (vMin = RuntimeNeedUnMove[0].transform.position);
			}
			for (int i = 0; i < RuntimeNeedUnMove.Length; i++)
			{
				num = Vector3.Dot(vMoveVector, RuntimeNeedUnMove[i].transform.position);
				bool flag = false;
				foreach (TrigerData trigerData2 in TrigerDatas)
				{
					if (Mathf.Abs(trigerData2.fTimeLeft - num) <= 0.001f)
					{
						trigerData2.listTriCollisionTrans.Add(RuntimeNeedUnMove[i].transform);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					TrigerData trigerData = new TrigerData();
					trigerData.fTimeLeft = num;
					trigerData.EndPos = vMoveVector * num;
					trigerData.listTriCollisionTrans.Add(RuntimeNeedUnMove[i].transform);
					TrigerDatas.Add(trigerData);
				}
				Collider2D[] componentsInChildren = RuntimeNeedUnMove[i].transform.GetComponentsInChildren<Collider2D>();
				if (componentsInChildren == null || componentsInChildren.Length == 0)
				{
					continue;
				}
				Collider2D[] array = componentsInChildren;
				foreach (Collider2D collider2D in array)
				{
					listCollisionTrans.Add(collider2D.transform);
					Bounds bounds = collider2D.bounds;
					if (vMax.x < bounds.max.x)
					{
						vMax.x = bounds.max.x;
					}
					if (vMax.y < bounds.max.y)
					{
						vMax.y = bounds.max.y;
					}
					if (vMin.x > bounds.min.x)
					{
						vMin.x = bounds.min.x;
					}
					if (vMin.y > bounds.min.y)
					{
						vMin.y = bounds.min.y;
					}
				}
			}
			if (vMax.x < StartPos.x)
			{
				vMax.x = StartPos.x;
			}
			if (vMax.y < StartPos.y)
			{
				vMax.y = StartPos.y;
			}
			if (vMin.x > StartPos.x)
			{
				vMin.x = StartPos.x;
			}
			if (vMin.y > StartPos.y)
			{
				vMin.y = StartPos.y;
			}
			if (vMax.x < EndPos.x)
			{
				vMax.x = EndPos.x;
			}
			if (vMax.y < EndPos.y)
			{
				vMax.y = EndPos.y;
			}
			if (vMin.x > EndPos.x)
			{
				vMin.x = EndPos.x;
			}
			if (vMin.y > EndPos.y)
			{
				vMin.y = EndPos.y;
			}
			if (TrigerDatas.Count < 3)
			{
				bStartUpdate = false;
				return;
			}
			float num2 = 0f;
			for (int k = 0; k < TrigerDatas.Count; k++)
			{
				int index = k;
				num = TrigerDatas[k].fTimeLeft;
				for (int l = k; l < TrigerDatas.Count; l++)
				{
					num2 = TrigerDatas[l].fTimeLeft;
					if (num2 > num)
					{
						index = l;
						num = num2;
					}
				}
				TrigerData value = TrigerDatas[k];
				TrigerDatas[k] = TrigerDatas[index];
				TrigerDatas[index] = value;
			}
			for (int m = 1; m < TrigerDatas.Count; m++)
			{
				TrigerDatas[m].tObj1 = (TrigerDatas[m].fTimeLeft - TrigerDatas[m - 1].fTimeLeft) * vMoveVector;
			}
			TrigerDatas[0].tObj1 = TrigerDatas[TrigerDatas.Count - 1].tObj1;
			TrigerDatas[0].fTimeLeft = Vector3.Dot(vMoveVector, EndPos);
			TrigerDatas[0].vMax = vMax;
			TrigerDatas[0].vMin = vMin;
		}

		private void StartStartFall(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate != null)
			{
				stageUpdate.StartCoroutine(StartFallCoroutine());
			}
		}

		private void StartTriggerSkill(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
			stageSkillAtkTargetParam.nSkillID = nType;
			stageSkillAtkTargetParam.bAtkNoCast = bLoop;
			stageSkillAtkTargetParam.tPos = tNowEventPos;
			stageSkillAtkTargetParam.tDir = (tMoveToPos - tNowEventPos).normalized;
			stageSkillAtkTargetParam.bBuff = false;
			if (bLoop && TriggerTransform != null)
			{
				stageSkillAtkTargetParam.tTrans = TriggerTransform;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
			}
			else
			{
				stageSkillAtkTargetParam.tTrans = base.transform;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
			}
		}

		private void StartStageItem(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (!bCheck && !bStartUpdate)
			{
				StageUpdate stageUpdate = StageResManager.GetStageUpdate();
				if (stageUpdate != null)
				{
					stageUpdate.StartCoroutine(StageItemCoroutine());
				}
				return;
			}
			bCheck = false;
			fTimeLeft = fMoveTime;
			fTimeLeftDelay = 0f;
			if (StageUpdate.bIsHost)
			{
				if (TriggerTransform != null)
				{
					bool flag = false;
					OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
					if ((bool)component)
					{
						if (StageUpdate.gbIsNetGame)
						{
							StageUpdate.SyncStageObj(sSyncID, 1, "1," + nType + "," + component.sPlayerID);
						}
						flag = component.GetOCSoundVolume() > 0f;
					}
					else
					{
						RideArmorController component2 = TriggerTransform.GetComponent<RideArmorController>();
						if (StageUpdate.gbIsNetGame)
						{
							StageUpdate.SyncStageObj(sSyncID, 1, "1," + nType + "," + component2.MasterPilot.sPlayerID);
						}
						flag = component2.MasterPilot.GetOCSoundVolume() > 0f;
					}
					if (listMutliMove[nType].nIntParam1 != 0)
					{
						EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
						stageSkillAtkTargetParam.tTrans = TriggerTransform;
						stageSkillAtkTargetParam.nSkillID = listMutliMove[nType].nIntParam1;
						stageSkillAtkTargetParam.bAtkNoCast = true;
						stageSkillAtkTargetParam.tPos = tNowEventPos;
						stageSkillAtkTargetParam.tDir = tMoveToPos;
						stageSkillAtkTargetParam.bBuff = true;
						if (flag)
						{
							BattleSE cueid = BattleSE.CRI_BATTLESE_BT_GETITEM01;
							if ((bool)component)
							{
								component.GetOCSoundVolume();
							}
							if (stageSkillAtkTargetParam.nSkillID == 960006 || stageSkillAtkTargetParam.nSkillID == 960007)
							{
								cueid = BattleSE.CRI_BATTLESE_BT_GETITEM02;
							}
							base.SoundSource.PlaySE("BattleSE", (int)cueid);
						}
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
					}
					if (listMutliAni[nType].nIntParam1 != 0)
					{
						EventManager.RegisterStageParam registerStageParam = new EventManager.RegisterStageParam();
						registerStageParam.nMode = 0;
						registerStageParam.nStageSecert = listMutliAni[nType].nIntParam1;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.REGISTER_STAGE_PARAM, registerStageParam);
					}
					if (listMutliAni[nType].aniName != null)
					{
						string[] array = listMutliAni[nType].aniName.Split(',');
						if (array.Length != 0)
						{
							int result = 0;
							if (int.TryParse(array[0], out result) && result != 0)
							{
								EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
								stageEventCall.nID = result;
								stageEventCall.tTransform = TriggerTransform;
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
							}
						}
					}
				}
				bStartUpdate = true;
			}
			else
			{
				bool flag2 = false;
				OrangeCharacter component3 = TriggerTransform.GetComponent<OrangeCharacter>();
				if ((bool)component3)
				{
					flag2 = component3.GetOCSoundVolume() > 0f;
					component3.GetOCSoundVolume();
				}
				else
				{
					RideArmorController component4 = TriggerTransform.GetComponent<RideArmorController>();
					flag2 = component4.MasterPilot.GetOCSoundVolume() > 0f;
					component4.MasterPilot.GetOCSoundVolume();
				}
				if (flag2)
				{
					PlaySE("BattleSE", "bt_getitem01");
				}
			}
			for (int num = base.transform.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(base.transform.GetChild(num).gameObject);
			}
		}

		private void StartPlayAnimation(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			string[] array = listMutliAni[nMoveStep].aniName.Split(',');
			Animator[] componentsInChildren = base.gameObject.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (array.Length > 1)
				{
					base.SoundSource.PlaySE(array[1], array[2]);
				}
				componentsInChildren[i].Play(array[0]);
				GateSE component = componentsInChildren[i].GetComponent<GateSE>();
				if (component != null && i == 1)
				{
					component.noPlaySE = true;
				}
			}
			bool flag = false;
			if (listMutliAni[nMoveStep].nIntParam1 == 1)
			{
				flag = true;
			}
			BoxCollider2D[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<BoxCollider2D>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				if (componentsInChildren2[j].gameObject.GetInstanceID() != base.gameObject.GetInstanceID())
				{
					componentsInChildren2[j].enabled = flag;
				}
			}
			nMoveStep++;
			if (nMoveStep >= listMutliAni.Count)
			{
				nMoveStep = 0;
			}
		}

		private void StartCloseObjCollider(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			BoxCollider2D[] componentsInChildren = base.gameObject.GetComponentsInChildren<BoxCollider2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.GetInstanceID() != base.gameObject.GetInstanceID())
				{
					componentsInChildren[i].enabled = false;
				}
			}
		}

		private void StartOpenObjCollider(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			BoxCollider2D[] componentsInChildren = base.gameObject.GetComponentsInChildren<BoxCollider2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.GetInstanceID() != base.gameObject.GetInstanceID())
				{
					componentsInChildren[i].enabled = true;
				}
			}
		}

		private void StartSwitchCtrlBtn(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			CheckSwutchBtn();
		}

		private void StartSwitchMemObj(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if ((nType & 1) == 0)
			{
				if (fTimeLeft != 0f && !(Time.realtimeSinceStartup - fTimeLeft > fMoveTime))
				{
					return;
				}
				nType |= 1;
				int childCount = base.transform.childCount;
				for (int i = 0; i < childCount; i++)
				{
					base.transform.GetChild(i).gameObject.SetActive(true);
				}
				if (((uint)nType & 4u) != 0)
				{
					if (TrigerDatas.Count == 0)
					{
						StageSceneObjParam[] componentsInChildren = base.transform.GetComponentsInChildren<StageSceneObjParam>();
						foreach (StageSceneObjParam stageSceneObjParam in componentsInChildren)
						{
							stageSceneObjParam.CheckAlphaMaterial();
							TrigerDatas.Add(new TrigerData());
							TrigerDatas[TrigerDatas.Count - 1].tObj1 = stageSceneObjParam;
						}
					}
					tTmpColor = Color.white;
					tTmpColor.a = 0f;
					foreach (TrigerData trigerData in TrigerDatas)
					{
						((StageSceneObjParam)trigerData.tObj1).SetSceneObjAlpha(tTmpColor);
						((StageSceneObjParam)trigerData.tObj1).SwitchB2DInStageSceneObj(false);
					}
					bStartUpdate = true;
					listMutliMove[0].fDT = listMutliMove[0].fMoveTime;
				}
				fTimeLeft = Time.realtimeSinceStartup;
				if (((uint)nType & 2u) != 0)
				{
					bCheck = false;
				}
				else if (!bLoop)
				{
					bCheck = false;
				}
			}
			else
			{
				if ((nType & 1) == 0 || (fTimeLeft != 0f && !(Time.realtimeSinceStartup - fTimeLeft > fMoveTime)))
				{
					return;
				}
				fTimeLeft = Time.realtimeSinceStartup;
				nType &= -2;
				if ((nType & 4) == 0)
				{
					int childCount2 = base.transform.childCount;
					for (int k = 0; k < childCount2; k++)
					{
						base.transform.GetChild(k).gameObject.SetActive(false);
					}
				}
				else
				{
					if (TrigerDatas.Count == 0)
					{
						StageSceneObjParam[] componentsInChildren = base.transform.GetComponentsInChildren<StageSceneObjParam>();
						foreach (StageSceneObjParam stageSceneObjParam2 in componentsInChildren)
						{
							stageSceneObjParam2.CheckAlphaMaterial();
							TrigerDatas.Add(new TrigerData());
							TrigerDatas[TrigerDatas.Count - 1].tObj1 = stageSceneObjParam2;
						}
					}
					tTmpColor = Color.white;
					foreach (TrigerData trigerData2 in TrigerDatas)
					{
						((StageSceneObjParam)trigerData2.tObj1).SetSceneObjAlpha(tTmpColor);
						((StageSceneObjParam)trigerData2.tObj1).SwitchB2DInStageSceneObj(true);
					}
					bStartUpdate = true;
					listMutliMove[0].fDT = listMutliMove[0].fMoveTime;
					PlaySE(listMutliAni[0].aniName);
				}
				if (((uint)nType & 2u) != 0)
				{
					bCheck = false;
				}
				else if (!bLoop)
				{
					bCheck = false;
				}
			}
		}

		private void StartPlayFlagMode(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bStartUpdate = true;
			if (TriggerTransform == null)
			{
				return;
			}
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				if (trigerData2.tTriggerTransform == TriggerTransform)
				{
					return;
				}
			}
			TrigerData trigerData = new TrigerData();
			trigerData.tTriggerTransform = TriggerTransform;
			trigerData.fTimeLeft = 0f;
			trigerData.EndPos = tMoveToPos;
			TrigerDatas.Add(trigerData);
		}

		private void StartChangeWeather(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.WEATHER_SYSTEM_CTRL, listMutliMove[0].nIntParam1 == 1, listMutliMove[1].nIntParam1, bLoop);
		}

		private void StartFlagSetting(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowFlagModeUI(nType);
			}
		}

		private void StartRideObj(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (TriggerTransform != null)
			{
				OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
				if (component == null)
				{
					return;
				}
				bool flag = false;
				foreach (TrigerData trigerData2 in TrigerDatas)
				{
					if ((trigerData2.tObj1 as OrangeCharacter).sPlayerID == component.sPlayerID)
					{
						return;
					}
				}
				if (component.CheckAvalibaleForRideArmorEvt())
				{
					flag = true;
				}
				if (!flag)
				{
					return;
				}
			}
			bStartUpdate = true;
			listController2D.Clear();
			listController2D.Add(base.transform.GetComponentInChildren<Controller2D>());
			if (!(TriggerTransform != null))
			{
				return;
			}
			OrangeCharacter component2 = TriggerTransform.GetComponent<OrangeCharacter>();
			if (component2 != null)
			{
				TrigerData trigerData = new TrigerData();
				TrigerDatas.Add(trigerData);
				trigerData.tTriggerTransform = TriggerTransform;
				trigerData.tObj1 = component2;
				if (component2.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					trigerData.tObj2 = "localPlayer";
				}
				else
				{
					trigerData.tObj2 = component2.sPlayerID;
				}
				component2.PlayerReleaseLeftCB();
				component2.PlayerReleaseRightCB();
				component2.PlayerPressLeftCB = NoneCall;
				component2.PlayerPressRightCB = NoneCall;
				component2.PlayerHeldLeftCB = NoneCall;
				component2.PlayerHeldRightCB = NoneCall;
				component2.PlayerReleaseLeftCB = NoneCall;
				component2.PlayerReleaseRightCB = NoneCall;
				trigerData.BackCB = component2.PlayerPressJumpCB;
				trigerData.tTrigerCBNoParam = JumpRelease;
				component2.PlayerPressJumpCB = trigerData.CallCBNoParam;
				base.transform.GetComponentInChildren<TramSE>().PlayTramSE();
				Animation[] componentsInChildren = base.transform.GetComponentsInChildren<Animation>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Play();
				}
				ParticleSystem[] componentsInChildren2 = base.transform.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].Play();
				}
			}
		}

		private void StartPathObjMove(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			CheckNeedUnMove();
			if (RuntimeNeedUnMove.Length == 0)
			{
				return;
			}
			TrigerDatas.Clear();
			OldTrigerDatas.Clear();
			TrigerData trigerData = new TrigerData();
			trigerData.EndPos = base.transform.position;
			OldTrigerDatas.Add(trigerData);
			trigerData = new TrigerData();
			trigerData.EndPos = tMoveToObj ?? Vector3.zero;
			OldTrigerDatas.Add(trigerData);
			foreach (MutliMove item in listMutliMove)
			{
				trigerData = new TrigerData();
				trigerData.EndPos = item.tMovePos;
				OldTrigerDatas.Add(trigerData);
			}
			trigerData = new TrigerData();
			trigerData.EndPos = base.transform.position;
			OldTrigerDatas.Add(trigerData);
			GameObject[] runtimeNeedUnMove = RuntimeNeedUnMove;
			foreach (GameObject gameObject in runtimeNeedUnMove)
			{
				float num = 0.5f;
				int nParam = 0;
				Vector3 position = Vector3.zero;
				bool flag = false;
				for (int j = 0; j < OldTrigerDatas.Count - 1; j++)
				{
					Vector3 vector = Vector3.Project(gameObject.transform.position - OldTrigerDatas[j].EndPos, OldTrigerDatas[j + 1].EndPos - OldTrigerDatas[j].EndPos);
					vector += OldTrigerDatas[j].EndPos;
					if (((OldTrigerDatas[j].EndPos.x + 0.005f > vector.x && vector.x > OldTrigerDatas[j + 1].EndPos.x - 0.005f) || (OldTrigerDatas[j].EndPos.x - 0.005f < vector.x && vector.x < OldTrigerDatas[j + 1].EndPos.x + 0.005f)) && ((OldTrigerDatas[j].EndPos.y + 0.005f > vector.y && vector.y > OldTrigerDatas[j + 1].EndPos.y - 0.005f) || (OldTrigerDatas[j].EndPos.y - 0.005f < vector.y && vector.y < OldTrigerDatas[j + 1].EndPos.y + 0.005f)))
					{
						float num2 = Vector3.Distance(gameObject.transform.position, vector);
						if (num > num2)
						{
							nParam = j;
							num = num2;
							position = vector;
							flag = true;
						}
					}
				}
				if (flag)
				{
					gameObject.transform.position = position;
					trigerData = new TrigerData();
					trigerData.tTriggerTransform = gameObject.transform;
					trigerData.nParam1 = nParam;
					BoxCollider2D[] componentsInChildren = gameObject.transform.GetComponentsInChildren<BoxCollider2D>();
					Vector3 position2 = trigerData.tTriggerTransform.position;
					Vector3 vMax = position2;
					BoxCollider2D[] array = componentsInChildren;
					foreach (BoxCollider2D boxCollider2D in array)
					{
						trigerData.listTriCollisionTrans.Add(boxCollider2D.transform);
						Bounds bounds = boxCollider2D.bounds;
						if (vMax.x < bounds.max.x)
						{
							vMax.x = bounds.max.x;
						}
						if (vMax.y < bounds.max.y)
						{
							vMax.y = bounds.max.y;
						}
						if (position2.x > bounds.min.x)
						{
							position2.x = bounds.min.x;
						}
						if (position2.y > bounds.min.y)
						{
							position2.y = bounds.min.y;
						}
					}
					trigerData.vMax = vMax;
					trigerData.vMin = position2;
					TrigerDatas.Add(trigerData);
				}
				else
				{
					gameObject.SetActive(false);
				}
			}
			float num3 = 0f;
			for (int l = 0; l < OldTrigerDatas.Count - 1; l++)
			{
				num3 += Vector3.Distance(OldTrigerDatas[l].EndPos, OldTrigerDatas[l + 1].EndPos);
			}
			fMoveSpeed = num3 / fMoveTime;
			fTimeLeft = 0f;
			bCheck = false;
			bStartUpdate = true;
		}

		private void StartCircleObjMove(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			if (bStartUpdate)
			{
				return;
			}
			fMoveSpeed = 360f / fMoveTime;
			CheckNeedUnMove();
			if (base.transform.childCount <= 0)
			{
				return;
			}
			TrigerDatas.Clear();
			for (int num = base.transform.childCount - 1; num >= 0; num--)
			{
				Transform child = base.transform.GetChild(num);
				TrigerData trigerData = new TrigerData();
				trigerData.tTriggerTransform = child;
				trigerData.EndPos = child.position;
				bool flag = false;
				foreach (MutliAni item in listMutliAni)
				{
					if (child.name == item.aniName)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					trigerData.nParam1 = 1;
					BoxCollider2D[] componentsInChildren = child.GetComponentsInChildren<BoxCollider2D>();
					Vector3 position = trigerData.tTriggerTransform.position;
					Vector3 vMax = position;
					BoxCollider2D[] array = componentsInChildren;
					foreach (BoxCollider2D boxCollider2D in array)
					{
						if (!(boxCollider2D.transform.GetComponent<StageSLBase>() != null))
						{
							trigerData.listTriCollisionTrans.Add(boxCollider2D.transform);
							Bounds bounds = boxCollider2D.bounds;
							if (vMax.x < bounds.max.x)
							{
								vMax.x = bounds.max.x;
							}
							if (vMax.y < bounds.max.y)
							{
								vMax.y = bounds.max.y;
							}
							if (position.x > bounds.min.x)
							{
								position.x = bounds.min.x;
							}
							if (position.y > bounds.min.y)
							{
								position.y = bounds.min.y;
							}
						}
					}
					trigerData.vMax = vMax;
					trigerData.vMin = position;
					if (trigerData.listTriCollisionTrans.Count == 0)
					{
						trigerData.nParam1 = 0;
					}
					TrigerDatas.Add(trigerData);
				}
			}
			OldTrigerDatas.Clear();
			for (int j = 0; j < listMutliAni.Count; j++)
			{
				if (listMutliAni[j].nIntParam1 == 1)
				{
					Transform transform = base.transform.Find(listMutliAni[j].aniName);
					if (transform != null)
					{
						TrigerData trigerData2 = new TrigerData();
						trigerData2.tTriggerTransform = transform;
						trigerData2.EndPos = transform.position;
						OldTrigerDatas.Add(trigerData2);
					}
				}
			}
			vMoveVector.x = 0f;
			fTimeLeft = 0f;
			bStartUpdate = true;
		}

		private void StartAngleObjMove(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			if (bStartUpdate)
			{
				return;
			}
			fMoveSpeed = 0f;
			CheckNeedUnMove();
			if (RuntimeNeedUnMove.Length == 0)
			{
				return;
			}
			TrigerDatas.Clear();
			GameObject[] runtimeNeedUnMove = RuntimeNeedUnMove;
			foreach (GameObject gameObject in runtimeNeedUnMove)
			{
				TrigerData trigerData = new TrigerData();
				trigerData.tTriggerTransform = gameObject.transform;
				trigerData.EndPos = gameObject.transform.position;
				bool flag = false;
				for (int j = 0; j < listMutliAni.Count; j++)
				{
					if (gameObject.transform.name == listMutliAni[j].aniName)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				BoxCollider2D[] componentsInChildren = gameObject.transform.GetComponentsInChildren<BoxCollider2D>();
				Vector3 position = trigerData.tTriggerTransform.position;
				Vector3 vMax = position;
				BoxCollider2D[] array = componentsInChildren;
				foreach (BoxCollider2D boxCollider2D in array)
				{
					trigerData.listTriCollisionTrans.Add(boxCollider2D.transform);
					Bounds bounds = boxCollider2D.bounds;
					if (vMax.x < bounds.max.x)
					{
						vMax.x = bounds.max.x;
					}
					if (vMax.y < bounds.max.y)
					{
						vMax.y = bounds.max.y;
					}
					if (position.x > bounds.min.x)
					{
						position.x = bounds.min.x;
					}
					if (position.y > bounds.min.y)
					{
						position.y = bounds.min.y;
					}
				}
				trigerData.vMax = vMax;
				trigerData.vMin = position;
				TrigerDatas.Add(trigerData);
			}
			OldTrigerDatas.Clear();
			for (int l = 0; l < listMutliAni.Count; l++)
			{
				if (listMutliAni[l].nIntParam1 == 1)
				{
					Transform transform = base.transform.Find(listMutliAni[l].aniName);
					if (transform != null)
					{
						TrigerData trigerData2 = new TrigerData();
						trigerData2.tTriggerTransform = transform;
						trigerData2.EndPos = transform.position;
						OldTrigerDatas.Add(trigerData2);
					}
				}
			}
			vMoveVector = listMutliMove[0].tMovePos;
			bStartUpdate = true;
		}

		private void StartCallEventID(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (nType != 0)
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = nType;
				stageEventCall.tTransform = TriggerTransform;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
			bCheck = false;
		}

		private void StartRotateTargetObj(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (bStartUpdate)
			{
				if (Mathf.Abs(listMutliMove[0].tMovePos.y) < 0.05f)
				{
					bStartUpdate = false;
					return;
				}
				nType = 2;
				fMoveTime = listMutliMove[0].fMoveTime / (listMutliMove[0].tMovePos.y / 0.016f);
			}
			else
			{
				if (listMutliMove[0].fMoveTime == 0f)
				{
					return;
				}
				bStartUpdate = true;
				for (int num = listMutliAni.Count - 1; num >= 0; num--)
				{
					Transform transform = base.transform.Find(listMutliAni[num].aniName);
					if (transform != null)
					{
						TrigerData trigerData = new TrigerData();
						trigerData.tTriggerTransform = transform;
						trigerData.EndPos = transform.position;
						OldTrigerDatas.Add(trigerData);
					}
				}
				vMoveVector.x = 0f;
				nType = 0;
				if (Mathf.Abs(listMutliMove[0].tMovePos.x) < 0.05f)
				{
					vMoveVector.x = listMutliMove[0].fMoveTime;
				}
				else
				{
					nType = 1;
					fMoveTime = listMutliMove[0].fMoveTime / (listMutliMove[0].tMovePos.x / 0.016f);
				}
				if (listMutliMove[0].nIntParam1 == 0)
				{
					EndPos = Vector3.right;
				}
				if (listMutliMove[0].nIntParam1 == 1)
				{
					EndPos = Vector3.up;
				}
				if (listMutliMove[0].nIntParam1 == 2)
				{
					EndPos = Vector3.forward;
				}
				if (Mathf.Abs(listMutliMove[0].tMovePos.z) > 0.05f)
				{
					fMoveSpeed = Mathf.Abs(listMutliMove[0].tMovePos.z);
				}
				else
				{
					fMoveSpeed = 0f;
				}
			}
		}

		private void StartShowRankFX(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
			{
				int nowStageStart = StageResManager.GetStageUpdate().GetNowStageStart();
				int index = 0;
				for (int i = 0; i < 3; i++)
				{
					if ((nowStageStart & (1 << i)) != 0)
					{
						index = 3 - i;
						break;
					}
				}
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(listMutliAni[index].aniName, base.transform.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(listMutliAni[0].aniName, base.transform.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
		}

		private void StartSetObjStatus(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (TriggerTransform == null)
			{
				return;
			}
			bLoop = true;
			for (int num = TrigerDatas.Count - 1; num >= 0; num--)
			{
				if (TrigerDatas[num].tTriggerTransform == TriggerTransform)
				{
					if (fMoveTime == 0f || Time.realtimeSinceStartup - TrigerDatas[num].fTimeLeft < fMoveTime)
					{
						return;
					}
					TrigerDatas[num].fTimeLeft = Time.realtimeSinceStartup;
					bLoop = false;
					break;
				}
			}
			if (bLoop)
			{
				TrigerData trigerData = new TrigerData();
				trigerData.tTriggerTransform = TriggerTransform;
				trigerData.fTimeLeft = Time.realtimeSinceStartup;
				TrigerDatas.Add(trigerData);
			}
			OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
			EnemyControllerBase component2 = TriggerTransform.GetComponent<EnemyControllerBase>();
			if (component != null)
			{
				if (listMutliAni[0].aniName == "AddMask")
				{
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/stage/stagepack", "SpriteMask", delegate(GameObject obj)
					{
						if (obj != null)
						{
							OrangeCharacter component3 = TriggerTransform.GetComponent<OrangeCharacter>();
							GameObject gameObject = UnityEngine.Object.Instantiate(obj, TriggerTransform);
							if (component3 != null)
							{
								gameObject.transform.position = component3.Controller.Collider2D.bounds.center;
							}
							else
							{
								gameObject.transform.localPosition = Vector3.zero;
							}
							gameObject.gameObject.name = "SpriteMask";
						}
					});
				}
				else if (listMutliAni[0].aniName == "DelMask")
				{
					Transform transform = TriggerTransform.Find("SpriteMask");
					if (transform != null)
					{
						UnityEngine.Object.Destroy(transform.gameObject);
					}
				}
			}
			else if (component2 != null)
			{
				component2.UpdateStatus(listMutliAni[0].nIntParam1, listMutliAni[0].aniName);
			}
		}

		private void StartAddStageQuest(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			if (bLoop)
			{
				if (StageResManager.GetStageUpdate() != null)
				{
					StageResManager.GetStageUpdate().RemoveStageQuest(nType);
				}
				return;
			}
			List<int> list = new List<int>();
			if (listMutliAni[0].nIntParam1 == 6)
			{
				for (int i = 1; i < listMutliAni.Count; i++)
				{
					string[] array = ((!(listMutliAni[i].aniName == "")) ? listMutliAni[i].aniName.Split('#') : new string[0]);
					if (array.Length != 0)
					{
						list.Add(array.Length / 2);
					}
					else
					{
						list.Add(0);
					}
					list.Add(listMutliAni[i].nIntParam2);
					list.Add(0);
					for (int j = 0; j < array.Length; j += 2)
					{
						list.Add(int.Parse(array[j]));
						list.Add(int.Parse(array[j + 1]));
						list.Add(0);
					}
				}
				list.Add(listMutliAni[0].nIntParam2);
			}
			else
			{
				for (int k = 1; k < listMutliAni.Count; k++)
				{
					list.Add(listMutliAni[k].nIntParam1);
					list.Add(listMutliAni[k].nIntParam2);
					list.Add(0);
				}
				list.Add(listMutliAni[0].nIntParam2);
			}
			if (StageResManager.GetStageUpdate() != null)
			{
				StageResManager.GetStageUpdate().AddStageQuest(nType, listMutliAni[0].nIntParam1, list.ToArray());
			}
		}

		private void StartInRangeBuff(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (TriggerTransform == null)
			{
				return;
			}
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				if (trigerData2.tTriggerTransform == TriggerTransform)
				{
					return;
				}
			}
			OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
			if (component == null)
			{
				return;
			}
			tmpBoundA = EventB2D.bounds;
			if (!(component.refRideBaseObj != null) && !component.bIsNpcCpy && (int)component.Hp > 0 && component.Controller.Collider2D.enabled)
			{
				tmpBoundB = component.Controller.Collider2D.bounds;
				if (StageResManager.CheckBoundsContainNoZEffect(ref tmpBoundA, ref tmpBoundB))
				{
					bStartUpdate = true;
					TrigerData trigerData = new TrigerData();
					trigerData.tTriggerTransform = TriggerTransform;
					trigerData.fTimeLeft = 0f;
					trigerData.tObj1 = component;
					TrigerDatas.Add(trigerData);
				}
			}
		}

		private void StartRidetObj(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (listMutliAni.Count <= 0 || listMutliAni[0].aniName == "" || listMutliAni[0].nIntParam1 == 1)
			{
				return;
			}
			Transform transform = base.transform.Find(listMutliAni[0].aniName);
			if (transform == null)
			{
				return;
			}
			if (TriggerTransform != null)
			{
				OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
				if (component == null)
				{
					return;
				}
				bool flag = false;
				foreach (TrigerData trigerData2 in TrigerDatas)
				{
					if ((trigerData2.tObj1 as OrangeCharacter).sPlayerID == component.sPlayerID)
					{
						return;
					}
				}
				if (component.CheckAvalibaleForRideArmorEvt())
				{
					flag = true;
				}
				if (!EventB2D.OverlapPoint(component.transform.position))
				{
					flag = false;
				}
				if (!flag)
				{
					return;
				}
			}
			if (!bStartUpdate)
			{
				bStartUpdate = true;
				OldTrigerDatas.Clear();
				OldTrigerDatas.Add(new TrigerData());
				OldTrigerDatas[0].tTriggerTransform = transform;
				OldTrigerDatas[0].tObj1 = transform.rotation;
				OldTrigerDatas[0].tObj2 = transform.GetComponentInChildren<Controller2D>();
				fTotalTimeLeft = 0f;
			}
			Collider2D[] componentsInChildren = base.transform.GetComponentsInChildren<Collider2D>();
			foreach (Collider2D collider2D in componentsInChildren)
			{
				if (!listCollisionTrans.Contains(collider2D.transform))
				{
					listCollisionTrans.Add(collider2D.transform);
				}
			}
			if (!(TriggerTransform != null))
			{
				return;
			}
			OrangeCharacter component2 = TriggerTransform.GetComponent<OrangeCharacter>();
			if (component2 != null)
			{
				TrigerData trigerData = new TrigerData();
				TrigerDatas.Add(trigerData);
				trigerData.tTriggerTransform = TriggerTransform;
				trigerData.tObj1 = component2;
				if (component2.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					trigerData.tObj2 = "localPlayer";
				}
				else
				{
					trigerData.tObj2 = component2.sPlayerID;
				}
				trigerData.EndPos = component2.transform.position;
				component2.Controller.LogicPosition = new VInt3(trigerData.EndPos);
				component2.PlayerReleaseLeftCB();
				component2.PlayerReleaseRightCB();
				component2.PlayerPressLeftCB = NoneCall;
				component2.PlayerPressRightCB = NoneCall;
				component2.PlayerHeldLeftCB = NoneCall;
				component2.PlayerHeldRightCB = NoneCall;
				component2.PlayerReleaseLeftCB = NoneCall;
				component2.PlayerReleaseRightCB = NoneCall;
				trigerData.BackCB = component2.PlayerPressJumpCB;
				trigerData.tTrigerCBNoParam = JumpRelease;
				component2.PlayerPressJumpCB = trigerData.CallCBNoParam;
				Animation[] componentsInChildren2 = base.transform.GetComponentsInChildren<Animation>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].Play();
				}
				ParticleSystem[] componentsInChildren3 = base.transform.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren3.Length; i++)
				{
					componentsInChildren3[i].Play();
				}
			}
		}

		private void StartPeriodCall(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			bStartUpdate = true;
			fTotalTimeLeft = fMoveTime;
			if (!(TriggerTransform != null) || TriggerTransform.GetComponent<OrangeCharacter>() == null)
			{
				return;
			}
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				if (trigerData2.tTriggerTransform == TriggerTransform)
				{
					return;
				}
			}
			TrigerData trigerData = new TrigerData();
			trigerData.tTriggerTransform = TriggerTransform;
			TrigerDatas.Add(trigerData);
		}

		private void StartSeqDestoryLoop(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			CheckNeedUnMove();
			TrigerDatas.Clear();
			GameObject[] runtimeNeedUnMove = RuntimeNeedUnMove;
			foreach (GameObject gameObject in runtimeNeedUnMove)
			{
				TrigerData trigerData = new TrigerData();
				trigerData.tTriggerTransform = gameObject.transform;
				trigerData.EndPos = gameObject.transform.position;
				TrigerDatas.Add(trigerData);
			}
			bool flag = true;
			if (((uint)nType & 2u) != 0)
			{
				flag = false;
			}
			else if (((uint)nType & 4u) != 0 && TriggerTransform != null && TriggerTransform.transform.position.x > EventB2D.bounds.center.x)
			{
				flag = false;
			}
			if (flag)
			{
				TrigerDatas.Sort(delegate(TrigerData x, TrigerData y)
				{
					if (x.EndPos.x == y.EndPos.x)
					{
						return 0;
					}
					if (x.EndPos.x > y.EndPos.x)
					{
						return 1;
					}
					return (x.EndPos.x < y.EndPos.x) ? (-1) : 0;
				});
			}
			else
			{
				TrigerDatas.Sort(delegate(TrigerData x, TrigerData y)
				{
					if (x.EndPos.x == y.EndPos.x)
					{
						return 0;
					}
					if (x.EndPos.x < y.EndPos.x)
					{
						return 1;
					}
					return (x.EndPos.x > y.EndPos.x) ? (-1) : 0;
				});
			}
			bStartUpdate = true;
		}

		private void StartCountDownEvent(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (bStartUpdate)
			{
				BattleInfoUI.Instance.CloseShowCountDown();
				bStartUpdate = false;
				return;
			}
			bCheck = false;
			bStartUpdate = true;
			fTimeLeft = fMoveTime;
			BattleInfoUI.Instance.ShowCountDown(fTimeLeft);
		}

		private void StartReCallEvent(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bCheck = false;
			bStartUpdate = true;
			if (fTimeLeft == 0f)
			{
				if (nType != 0)
				{
					EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
					stageEventCall.nID = nType;
					stageEventCall.tTransform = TriggerTransform;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
				}
				fTimeLeft = fMoveTime;
			}
			if (!(TriggerTransform != null))
			{
				return;
			}
			OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
			if (component == null)
			{
				return;
			}
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				if (trigerData2.tTriggerTransform == TriggerTransform)
				{
					return;
				}
			}
			TrigerData trigerData = new TrigerData();
			trigerData.tTriggerTransform = TriggerTransform;
			trigerData.tObj1 = component;
			TrigerDatas.Add(trigerData);
		}

		private void StartSimulateFallObj(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			foreach (Transform listCollisionTran in listCollisionTrans)
			{
				TrigerData trigerData = new TrigerData();
				trigerData.tObj1 = listCollisionTran.GetComponent<BoxCollider2D>();
				if (trigerData.tObj1 != null)
				{
					TrigerDatas.Add(trigerData);
				}
			}
			if (TrigerDatas.Count <= 0)
			{
				return;
			}
			bCheck = false;
			bStartUpdate = true;
			vMoveVector = Vector3.zero;
			Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue);
			Vector3 vMax = new Vector3(float.MinValue, float.MinValue);
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				Bounds bounds = (trigerData2.tObj1 as BoxCollider2D).bounds;
				if (vMax.x < bounds.max.x)
				{
					vMax.x = bounds.max.x;
				}
				if (vMax.y < bounds.max.y)
				{
					vMax.y = bounds.max.y;
				}
				if (vMin.x > bounds.min.x)
				{
					vMin.x = bounds.min.x;
				}
				if (vMin.y > bounds.min.y)
				{
					vMin.y = bounds.min.y;
				}
			}
			TrigerDatas[0].vMax = vMax;
			TrigerDatas[0].vMin = vMin;
			MOECollisionUseData mOECollisionUseData = new MOECollisionUseData();
			TrigerDatas[0].tObj2 = mOECollisionUseData;
			mOECollisionUseData.bPlaySE = new bool[3];
			fTimeLeft = listMutliMove[0].fMoveTime;
			NowPos = base.transform.localPosition;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
		}

		private void StartShotItemBullet(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (!bCheck && !bStartUpdate)
			{
				StageUpdate stageUpdate = StageResManager.GetStageUpdate();
				if (stageUpdate != null)
				{
					stageUpdate.StartCoroutine(StageItemCoroutine());
				}
			}
		}

		private void StartSwingHammer(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (!bStartUpdate)
			{
				bStartUpdate = true;
				vMoveVector = Vector3.zero;
				TrigerDatas.Clear();
			}
			if (!(TriggerTransform != null))
			{
				return;
			}
			while (listMutliMove.Count < 2)
			{
				listMutliMove.Add(new MutliMove());
			}
			if (!(Mathf.Abs(vMoveVector.x) < 5f))
			{
				return;
			}
			SKILL_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nType, out value);
			OrangeCharacter component = TriggerTransform.GetComponent<OrangeCharacter>();
			if (value != null)
			{
				if ((component != null && (bool)component.IsUnBreakX()) || MonoBehaviourSingleton<StageSyncManager>.Instance.CheckAtkTime(value, TriggerTransform))
				{
					return;
				}
				EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
				stageSkillAtkTargetParam.nSkillID = nType;
				stageSkillAtkTargetParam.bAtkNoCast = true;
				stageSkillAtkTargetParam.tPos = tNowEventPos;
				stageSkillAtkTargetParam.tDir = (tMoveToPos - tNowEventPos).normalized;
				stageSkillAtkTargetParam.bBuff = false;
				stageSkillAtkTargetParam.tTrans = TriggerTransform;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
			}
			if (listMutliMove[0].tMovePos.y == 0f || !(component != null))
			{
				return;
			}
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				if ((trigerData2.tObj1 as OrangeCharacter).GetInstanceID() == component.GetInstanceID())
				{
					return;
				}
			}
			TrigerData trigerData = new TrigerData();
			trigerData.tObj1 = component;
			trigerData.vMin = new Vector3(0f, listMutliMove[0].tMovePos.y, 0f);
			trigerData.EndPos = listMutliMove[0].tMovePos;
			trigerData.EndPos.x = listMutliMove[1].tMovePos.x;
			TrigerDatas.Add(trigerData);
		}

		private void StartCountUpEvent(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			if (bStartUpdate)
			{
				BattleInfoUI.Instance.CloseShowCountDown();
				bStartUpdate = false;
			}
			else
			{
				bCheck = false;
				bStartUpdate = true;
				BattleInfoUI.Instance.ShowCountDown(0f);
			}
		}

		private void StartTransParentByIn2(Vector3 tNowEventPos, Vector3 tMoveToPos, Transform TriggerTransform = null)
		{
			bStartUpdate = true;
			bCheck = false;
			List<int> list = new List<int>();
			if (TrigerDatas.Count == 1)
			{
				list = TrigerDatas[0].tObj2 as List<int>;
				for (int i = 0; i < list.Count; i += 2)
				{
					list[i] = list[i + 1];
				}
				return;
			}
			MeshRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<MeshRenderer>();
			List<MeshRenderer> list2 = new List<MeshRenderer>();
			List<int> list3 = new List<int>();
			for (int j = 0; j < listMutliAni.Count; j++)
			{
				Transform transform = base.transform.Find(listMutliAni[j].aniName);
				if (!list3.Contains(transform.gameObject.GetInstanceID()))
				{
					list3.Add(transform.gameObject.GetInstanceID());
				}
			}
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				if (!list3.Contains(meshRenderer.gameObject.GetInstanceID()))
				{
					list2.Add(meshRenderer);
				}
			}
			for (int l = 0; l < list2.Count; l++)
			{
				MeshRenderer meshRenderer2 = list2[l];
				if (meshRenderer2.material.shader.name != "StageLib/DiffuseAlpha")
				{
					Material material = new Material(Shader.Find("StageLib/DiffuseAlpha"));
					material.mainTexture = meshRenderer2.material.mainTexture;
					meshRenderer2.material = material;
				}
				tTmpColor = meshRenderer2.material.GetColor("_Color");
				list.Add((int)(tTmpColor.a * 100f));
				list.Add((int)(tTmpColor.a * 100f));
				meshRenderer2.material.SetColor("_Color", tTmpColor);
			}
			TrigerDatas.Clear();
			TrigerData trigerData = new TrigerData();
			trigerData.tObj1 = list2;
			trigerData.tObj2 = list;
			TrigerDatas.Add(trigerData);
		}

		private void AddSelfToTrigger()
		{
			TrigerDatas.Clear();
			TrigerData trigerData = new TrigerData();
			BoxCollider2D[] componentsInChildren = base.transform.GetComponentsInChildren<BoxCollider2D>();
			Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue);
			Vector3 vMax = new Vector3(float.MinValue, float.MinValue);
			BoxCollider2D[] array = componentsInChildren;
			foreach (BoxCollider2D boxCollider2D in array)
			{
				if (!(boxCollider2D.transform == base.transform))
				{
					trigerData.listTriCollisionTrans.Add(boxCollider2D.transform);
					Bounds bounds = boxCollider2D.bounds;
					if (vMax.x < bounds.max.x)
					{
						vMax.x = bounds.max.x;
					}
					if (vMax.y < bounds.max.y)
					{
						vMax.y = bounds.max.y;
					}
					if (vMin.x > bounds.min.x)
					{
						vMin.x = bounds.min.x;
					}
					if (vMin.y > bounds.min.y)
					{
						vMin.y = bounds.min.y;
					}
				}
			}
			trigerData.vMax = vMax;
			trigerData.vMin = vMin;
			TrigerDatas.Add(trigerData);
		}

		private void plrcb()
		{
			CharacterDirection characterDirection = (CharacterDirection)0;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(listMutliAni[0].aniName, ButtonId.RIGHT))
			{
				characterDirection = CharacterDirection.RIGHT;
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsHeld(listMutliAni[0].aniName, ButtonId.LEFT))
			{
				characterDirection = CharacterDirection.LEFT;
			}
			fMoveSpeed += (float)characterDirection * fMoveTime;
		}

		private void JumpRelease(TrigerData tTrigerData)
		{
			OrangeCharacter orangeCharacter = tTrigerData.tObj1 as OrangeCharacter;
			orangeCharacter.ConnectStandardCtrlCB();
			orangeCharacter.PlayerPressJumpCB = tTrigerData.BackCB;
			TrigerDatas.Remove(tTrigerData);
			bCheck = true;
			if (!orangeCharacter.bLockInputCtrl)
			{
				orangeCharacter.PlayerPressJumpCB();
				orangeCharacter.AddForce(new VInt3(new Vector3(fMoveSpeed, 0f, 0f)));
				StartCoroutine(CheckPlayerLeave(orangeCharacter));
			}
		}

		private IEnumerator CheckPlayerLeave(OrangeCharacter tOC)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			float fLastMoveSpeed = fMoveSpeed;
			float fAddTime = 0.3f;
			while (fAddTime > 0f)
			{
				fAddTime -= Time.deltaTime;
				tOC.AddForce(new VInt3(new Vector3(fLastMoveSpeed * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen), 0f, 0f)));
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		private void CheckSwutchBtn()
		{
			VirtualButton virtualButton = null;
			VirtualPadSystem virtualPadSystem = MonoBehaviourSingleton<InputManager>.Instance.VirtualPadSystem;
			if (virtualPadSystem == null || !virtualPadSystem.IsInit)
			{
				StartCoroutine(DelayCheckPadBtn());
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				bool active = (nType & (1 << i)) != 0;
				switch (i)
				{
				case 0:
					virtualButton = virtualPadSystem.GetButton(ButtonId.SHOOT);
					break;
				case 1:
					virtualButton = virtualPadSystem.GetButton(ButtonId.JUMP);
					break;
				case 2:
					virtualButton = virtualPadSystem.GetButton(ButtonId.DASH);
					break;
				case 3:
					virtualButton = virtualPadSystem.GetButton(ButtonId.SKILL0);
					break;
				case 4:
					virtualButton = virtualPadSystem.GetButton(ButtonId.SKILL1);
					break;
				}
				if (virtualButton != null)
				{
					virtualButton.gameObject.SetActive(active);
				}
			}
			GameObject gameObject = GameObject.Find("StageUpdate");
			if (gameObject != null)
			{
				gameObject.GetComponent<StageUpdate>().nBtnStatus = nType;
			}
		}

		private IEnumerator DelayCheckPadBtn()
		{
			while (!MonoBehaviourSingleton<InputManager>.Instance.VirtualPadSystem)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			VirtualPadSystem vps = MonoBehaviourSingleton<InputManager>.Instance.VirtualPadSystem;
			while (!vps.IsInit)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			CheckSwutchBtn();
		}

		public override void OnLateUpdate()
		{
			if (!bStartUpdate)
			{
				return;
			}
			fNowUpdateTimeDelta += 0.016f;
			if (fTimeLeftDelay > 0f)
			{
				if (mapEvent == MapEventEnum.MOVETO)
				{
					fTotalTimeLeft += 0.016f;
				}
				if (!(fNowUpdateTimeDelta > fTimeLeftDelay))
				{
					return;
				}
				fNowUpdateTimeDelta -= fTimeLeftDelay;
				fTimeLeftDelay = 0f;
				if (fNowUpdateTimeDelta < 0.016f)
				{
					return;
				}
			}
			if (fNowUpdateTimeDelta >= 0.016f)
			{
				while (fNowUpdateTimeDelta >= 0.016f)
				{
					updateCalls[(int)mapEvent]();
					fNowUpdateTimeDelta -= 0.016f;
				}
				fNowUpdateTimeDelta = 0f;
			}
		}

		private void NoneCall()
		{
		}

		private void PlayEffect()
		{
			for (int num = TrigerDatas.Count - 1; num >= 0; num--)
			{
				TrigerData trigerData = TrigerDatas[num];
				if (trigerData.fTimeLeft > 0.016f)
				{
					trigerData.fTimeLeft -= 0.016f;
				}
				else
				{
					trigerData.fTimeLeft = 0f;
				}
				if (trigerData.fTimeLeft == 0f)
				{
					if (listMutliAni.Count > 0)
					{
						StageFXParam stageFXParam = new StageFXParam();
						tTmpColor.r = listMutliMove[0].tMovePos.x;
						tTmpColor.g = listMutliMove[0].tMovePos.y;
						tTmpColor.b = listMutliMove[0].tMovePos.z;
						tTmpColor.a = listMutliMove[0].fMoveTime;
						stageFXParam.tColor = tTmpColor;
						if (bLoop)
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(listMutliAni[0].aniName, trigerData.tTriggerTransform, Quaternion.Euler(0f, 0f, 0f), new object[1] { stageFXParam });
						}
						else
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(listMutliAni[0].aniName, trigerData.EndPos, Quaternion.Euler(0f, 0f, 0f), new object[1] { stageFXParam });
						}
					}
					trigerData.fTimeLeft = fMoveTime;
					OldTrigerDatas.Add(trigerData);
					TrigerDatas.RemoveAt(num);
				}
			}
			for (int num2 = OldTrigerDatas.Count - 1; num2 >= 0; num2--)
			{
				TrigerData trigerData2 = OldTrigerDatas[num2];
				if (trigerData2.fTimeLeft > 0.016f)
				{
					trigerData2.fTimeLeft -= 0.016f;
				}
				else
				{
					trigerData2.fTimeLeft = 0f;
				}
				if (trigerData2.fTimeLeft == 0f)
				{
					OldTrigerDatas.RemoveAt(num2);
				}
			}
			if (TrigerDatas.Count == 0 && OldTrigerDatas.Count == 0)
			{
				bStartUpdate = false;
			}
		}

		private void TeleportTo()
		{
			int num = TrigerDatas.Count;
			for (int i = 0; i < num; i++)
			{
				TrigerData trigerData = TrigerDatas[i];
				if (trigerData.fTimeLeft > 0.016f)
				{
					trigerData.fTimeLeft -= 0.016f;
				}
				else
				{
					trigerData.fTimeLeft = 0f;
				}
				if (trigerData.fTimeLeft == 0f && trigerData.tTriggerTransform != null)
				{
					TrigerDatas.RemoveAt(i);
					num--;
					i--;
				}
			}
			if (num == 0)
			{
				bStartUpdate = false;
			}
		}

		private void CheckNeedUnMove()
		{
			if (tmplistNeedUnMove != null)
			{
				if (RuntimeNeedUnMove != null)
				{
					for (int i = 0; i < RuntimeNeedUnMove.Length; i++)
					{
						if (!tmplistNeedUnMove.Contains(RuntimeNeedUnMove[i]))
						{
							tmplistNeedUnMove.Add(RuntimeNeedUnMove[i]);
						}
					}
				}
				RuntimeNeedUnMove = tmplistNeedUnMove.ToArray();
				tmplistNeedUnMove.Clear();
				tmplistNeedUnMove = null;
			}
			if (RuntimeNeedUnMove == null)
			{
				RuntimeNeedUnMove = new GameObject[0];
			}
		}

		private bool CheckObjCollision(Controller2D tController2D, int ndir, float moveLen, bool bIsSelfCheck, out float dis, List<Transform> listCheckTrans)
		{
			Vector2 vDir = Vector2.down;
			switch (ndir)
			{
			case 0:
				vDir = Vector2.up;
				break;
			case 1:
				vDir = Vector2.down;
				break;
			case 2:
				vDir = Vector2.right;
				break;
			case 3:
				vDir = Vector2.left;
				break;
			}
			RaycastHit2D raycastHit2D = ((!bIsSelfCheck) ? StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, vDir, moveLen, tController2D.collisionMask, listCheckTrans) : StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, vDir, moveLen, tController2D.collisionMask, null, listCheckTrans));
			if ((bool)raycastHit2D)
			{
				dis = raycastHit2D.distance;
				return true;
			}
			dis = 0f;
			return false;
		}

		private bool CheckObjAndMove(Controller2D tController2D, Vector3 dis, List<Transform> listCheckTrans)
		{
			float dis2 = 0f;
			Vector2 vector = dis.xy();
			float magnitude = vector.magnitude;
			vector /= magnitude;
			Vector3 vector2 = new Vector3(vector.x, vector.y, 0f);
			RaycastHit2D raycastHit2D;
			if (CheckObjCollision(tController2D, 1, 0.1f, true, out dis2, listCheckTrans))
			{
				raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, vector, magnitude, (int)tController2D.collisionMask + (int)tController2D.collisionMaskThrough, listCheckTrans);
				if ((bool)raycastHit2D)
				{
					tController2D.transform.localPosition = tController2D.transform.localPosition + vector2 * raycastHit2D.distance;
					tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + vector2 * raycastHit2D.distance);
					return true;
				}
				tController2D.transform.localPosition = tController2D.transform.localPosition + dis;
				tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + dis);
				return false;
			}
			if (tController2D.UseSticky && CheckObjCollision(tController2D, 0, 0.1f, true, out dis2, listCheckTrans))
			{
				raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, vector, magnitude, (int)tController2D.collisionMask + (int)tController2D.collisionMaskThrough, listCheckTrans);
				if ((bool)raycastHit2D)
				{
					tController2D.transform.localPosition = tController2D.transform.localPosition + vector2 * raycastHit2D.distance;
					tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + vector2 * raycastHit2D.distance);
					return true;
				}
				tController2D.transform.localPosition = tController2D.transform.localPosition + dis;
				tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + dis);
				return false;
			}
			if (tController2D.UseSticky && CheckObjCollision(tController2D, 3, 0.1f, true, out dis2, listCheckTrans))
			{
				raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, vector, magnitude, (int)tController2D.collisionMask + (int)tController2D.collisionMaskThrough, listCheckTrans);
				if ((bool)raycastHit2D)
				{
					tController2D.transform.localPosition = tController2D.transform.localPosition + vector2 * raycastHit2D.distance;
					tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + vector2 * raycastHit2D.distance);
					return true;
				}
				tController2D.transform.localPosition = tController2D.transform.localPosition + dis;
				tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + dis);
				return false;
			}
			if (tController2D.UseSticky && CheckObjCollision(tController2D, 2, 0.1f, true, out dis2, listCheckTrans))
			{
				raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, vector, magnitude, (int)tController2D.collisionMask + (int)tController2D.collisionMaskThrough, listCheckTrans);
				if ((bool)raycastHit2D)
				{
					tController2D.transform.localPosition = tController2D.transform.localPosition + vector2 * raycastHit2D.distance;
					tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + vector2 * raycastHit2D.distance);
					return true;
				}
				tController2D.transform.localPosition = tController2D.transform.localPosition + dis;
				tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + dis);
				return false;
			}
			raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, -vector, magnitude, (int)tController2D.collisionMask + (int)tController2D.collisionMaskThrough, null, listCheckTrans);
			if ((bool)raycastHit2D)
			{
				raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, vector, magnitude - raycastHit2D.distance, (int)tController2D.collisionMask + (int)tController2D.collisionMaskThrough, listCheckTrans);
				if ((bool)raycastHit2D)
				{
					tController2D.transform.localPosition = tController2D.transform.localPosition + vector2 * raycastHit2D.distance;
					tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + vector2 * raycastHit2D.distance);
					return true;
				}
				tController2D.transform.localPosition = tController2D.transform.localPosition + vector2 * (magnitude - raycastHit2D.distance);
				tController2D.LogicPosition = new VInt3(tController2D.LogicPosition.vec3 + vector2 * (magnitude - raycastHit2D.distance));
				return false;
			}
			return false;
		}

		private void CheckPlayerEnemyAndMove(Vector3 dis, Vector3 vMax, Vector3 vMin, List<Transform> listCheckTrans = null, bool bPinchDead = false, int nPinchSkill = 0)
		{
			if (listCheckTrans == null)
			{
				listCheckTrans = listCollisionTrans;
			}
			Vector2 zero = Vector2.zero;
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				if (StageUpdate.runPlayers[num].UsingVehicle)
				{
					RideArmorController component = StageUpdate.runPlayers[num].transform.root.GetComponent<RideArmorController>();
					Vector3 position = component.transform.position;
					zero = component.Controller.Collider2D.size;
					zero *= 2f;
					if (position.x >= vMin.x - zero.x && position.x <= vMax.x + zero.x && position.y >= vMin.y - zero.y && position.y <= vMax.y + zero.y && CheckObjAndMove(component.Controller, dis, listCheckTrans) && bPinchDead)
					{
						component.Hp = 0;
						component.Hurt(new HurtPassParam());
						OrangeCharacter orangeCharacter = StageUpdate.runPlayers[num];
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_REBORNEVENT, orangeCharacter, orangeCharacter.transform.position);
					}
				}
				else
				{
					Vector3 position2 = StageUpdate.runPlayers[num].transform.position;
					zero = StageUpdate.runPlayers[num].Controller.Collider2D.size;
					zero *= 2f;
					if (position2.x >= vMin.x - zero.x && position2.x <= vMax.x + zero.x && position2.y >= vMin.y - zero.y && position2.y <= vMax.y + zero.y && CheckObjAndMove(StageUpdate.runPlayers[num].Controller, dis, listCheckTrans) && bPinchDead)
					{
						EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
						stageSkillAtkTargetParam.nSkillID = nPinchSkill;
						stageSkillAtkTargetParam.bAtkNoCast = true;
						stageSkillAtkTargetParam.tPos = Vector3.zero;
						stageSkillAtkTargetParam.tDir = Vector3.zero;
						stageSkillAtkTargetParam.bBuff = false;
						stageSkillAtkTargetParam.tTrans = StageUpdate.runPlayers[num].transform;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
						OrangeCharacter orangeCharacter2 = StageUpdate.runPlayers[num];
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_REBORNEVENT, orangeCharacter2, orangeCharacter2.transform.position);
					}
				}
			}
			for (int num2 = StageUpdate.runEnemys.Count - 1; num2 >= 0; num2--)
			{
				Vector3 position3 = StageUpdate.runEnemys[num2].mEnemy.transform.position;
				zero = StageUpdate.runEnemys[num2].mEnemy.Controller.Collider2D.size;
				zero *= 2f;
				if (position3.x >= vMin.x - zero.x && position3.x <= vMax.x + zero.x && position3.y >= vMin.y - zero.y && position3.y <= vMax.y + zero.y)
				{
					CheckObjAndMove(StageUpdate.runEnemys[num2].mEnemy.Controller, dis, listCheckTrans);
				}
			}
		}

		private void MoveMapTime()
		{
			Vector3 vDecPt = Vector3.zero;
			if (TrigerDatas.Count == 0)
			{
				return;
			}
			Vector3 v;
			if (fTimeLeft > 0.016f)
			{
				v = vMoveVector * (0.016f * fMoveSpeed);
				AutoLinkLib.DivVector3DecimalPoint(ref v, ref vDecPt, 3);
				TrigerDatas[0].EndPos += vDecPt;
				AutoLinkLib.DivVector3DecimalPoint(ref TrigerDatas[0].EndPos, ref vDecPt, 3);
				v += TrigerDatas[0].EndPos;
				TrigerDatas[0].EndPos = vDecPt;
				NowPos += v;
				fTimeLeft -= 0.016f;
				fTotalTimeLeft += 0.016f;
				if (((uint)nType & 0x80u) != 0)
				{
					if (listMutliAni[0].nIntParam2 > 0)
					{
						listMutliAni[0].nIntParam2 -= 16;
					}
					else
					{
						CameraControl component = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>();
						if (component != null)
						{
							component.CameraMove(v);
						}
					}
				}
				CheckPlayerEnemyAndMove(v, TrigerDatas[0].vMax, TrigerDatas[0].vMin, TrigerDatas[0].listTriCollisionTrans, (nType & 0x100) != 0, listMutliAni[1].nIntParam1);
				TrigerDatas[0].vMax += v;
				TrigerDatas[0].vMin += v;
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(sSyncID, 0, fTimeLeft.ToString("0.000") + "," + fTotalTimeLeft.ToString("0.000"));
				}
			}
			else
			{
				v = EndPos - NowPos;
				NowPos = EndPos;
				AutoLinkLib.DivVector3DecimalPoint(ref v, ref vDecPt, 3);
				TrigerDatas[0].EndPos += vDecPt;
				AutoLinkLib.DivVector3DecimalPoint(ref TrigerDatas[0].EndPos, ref vDecPt, 3);
				v += TrigerDatas[0].EndPos;
				if (((uint)nType & 0x80u) != 0)
				{
					if (listMutliAni[0].nIntParam2 > 0)
					{
						listMutliAni[0].nIntParam2 -= 16;
					}
					else
					{
						CameraControl component2 = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>();
						if (component2 != null)
						{
							component2.CameraMove(v);
						}
					}
				}
				CheckPlayerEnemyAndMove(v, TrigerDatas[0].vMax, TrigerDatas[0].vMin, TrigerDatas[0].listTriCollisionTrans, (nType & 0x100) != 0, listMutliAni[1].nIntParam1);
				TrigerDatas[0].vMax += v;
				TrigerDatas[0].vMin += v;
				fTotalTimeLeft += fTimeLeft;
				fTimeLeft = 0f;
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(sSyncID, 0, fTimeLeft.ToString("0.000") + "," + fTotalTimeLeft.ToString("0.000"));
				}
			}
			base.transform.position = NowPos;
			if (((uint)nType & 8u) != 0)
			{
				EventManager.LockRangeParam lockRangeParam = new EventManager.LockRangeParam();
				lockRangeParam.fMinX = 0f;
				lockRangeParam.fMaxX = 0f;
				lockRangeParam.fMinY = 0f;
				lockRangeParam.fMaxY = 0f;
				lockRangeParam.nNoBack = 0;
				if (((uint)nType & 0x800u) != 0)
				{
					lockRangeParam.nNoBack = 2;
				}
				else if (((uint)nType & 0x1000u) != 0)
				{
					lockRangeParam.nNoBack = 4;
				}
				else if (((uint)nType & 0x2000u) != 0)
				{
					lockRangeParam.nNoBack = 8;
				}
				else if (((uint)nType & 0x4000u) != 0)
				{
					lockRangeParam.nNoBack = 16;
				}
				if (((uint)nType & 0x20u) != 0)
				{
					lockRangeParam.nNoBack = 1;
				}
				lockRangeParam.nNoBack |= 1;
				lockRangeParam.fSpeed = null;
				lockRangeParam.bSetFocus = false;
				lockRangeParam.nMode = 2;
				lockRangeParam.vDir = v;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCK_RANGE, lockRangeParam);
			}
		}

		private void MoveToGetNextPos()
		{
			float num = fMoveTime;
			if (nMoveStep == 0)
			{
				StartPos = EventInitPos;
				EndPos = tMoveToObj ?? Vector3.zero;
				num = fMoveTime;
			}
			else
			{
				if (nMoveStep > listMutliMove.Count)
				{
					bStartUpdate = false;
					return;
				}
				StartPos = EndPos;
				EndPos = listMutliMove[nMoveStep - 1].tMovePos;
				num = listMutliMove[nMoveStep - 1].fMoveTime;
				if (nMoveStep == listMutliMove.Count && bLoop)
				{
					nMoveStep = -1;
				}
			}
			nMoveStep++;
			vMoveVector = EndPos - StartPos;
			fMoveSpeed = Vector3.Distance(StartPos, EndPos);
			if (num > 0f)
			{
				fMoveSpeed /= num;
			}
			vMoveVector = vMoveVector.normalized;
			fTimeLeft = num;
			NowPos = base.transform.position;
			AddSelfToTrigger();
			TrigerDatas[0].EndPos = Vector3.zero;
		}

		private void MoveMapTo()
		{
			if (((uint)nType & 0x10u) != 0)
			{
				if (StageUpdate.IsAllPlayerDead())
				{
					if (!bDiedPause)
					{
						bDiedPause = true;
					}
					return;
				}
				if (bDiedPause)
				{
					bDiedPause = false;
				}
			}
			switch (ts_State)
			{
			case TIMELINE_STATE.TS_START:
				MoveToGetNextPos();
				if (fMoveSpeed == 0f)
				{
					ts_State = TIMELINE_STATE.TS_MOVE02;
				}
				else
				{
					ts_State = TIMELINE_STATE.TS_MOVE01;
				}
				break;
			case TIMELINE_STATE.TS_MOVE01:
			{
				Action moveObjStart = MoveObjStart;
				if (moveObjStart != null)
				{
					moveObjStart();
				}
				ts_State = TIMELINE_STATE.TS_MOVE02;
				break;
			}
			case TIMELINE_STATE.TS_MOVE02:
			{
				MoveMapTime();
				if (fTimeLeft != 0f)
				{
					break;
				}
				bool flag = false;
				if (nMoveStep == 1)
				{
					if (listMutliAni.Count > 0 && listMutliAni[0].nIntParam1 != 0)
					{
						EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
						stageEventCall.nID = listMutliAni[0].nIntParam1;
						stageEventCall.tTransform = StageUpdate.GetHostPlayerTrans();
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
						flag = true;
					}
				}
				else if (nMoveStep > 1 && listMutliMove.Count > nMoveStep - 2 && listMutliMove[nMoveStep - 2].nIntParam1 != 0)
				{
					EventManager.StageEventCall stageEventCall2 = new EventManager.StageEventCall();
					stageEventCall2.nID = listMutliMove[nMoveStep - 2].nIntParam1;
					stageEventCall2.tTransform = StageUpdate.GetHostPlayerTrans();
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall2);
					flag = true;
				}
				if (nCallCount > 0)
				{
					nCallCount--;
				}
				else if (((uint)nType & (true ? 1u : 0u)) != 0)
				{
					if ((nType & 4) == 0)
					{
						bCheck = true;
						bStartUpdate = false;
						flag = true;
					}
				}
				else if (!bLoop)
				{
					bCheck = true;
					bStartUpdate = false;
					flag = true;
				}
				if (nMoveStep == 0 || (bLoop && nMoveStep == listMutliMove.Count + 1))
				{
					fTimeLeftDelay = fDelayTime;
				}
				else if (nMoveStep <= listMutliMove.Count)
				{
					fTimeLeftDelay = listMutliMove[nMoveStep - 1].fDT;
				}
				if (fTimeLeftDelay != 0f || flag)
				{
					if (fMoveSpeed != 0f)
					{
						Action moveObjEnd2 = MoveObjEnd;
						if (moveObjEnd2 != null)
						{
							moveObjEnd2();
						}
					}
					ts_State = TIMELINE_STATE.TS_START;
				}
				else
				{
					MoveToGetNextPos();
				}
				break;
			}
			case TIMELINE_STATE.TS_MOVE05:
			{
				Action moveObjEnd = MoveObjEnd;
				if (moveObjEnd != null)
				{
					moveObjEnd();
				}
				ts_State = TIMELINE_STATE.TS_MOVE06;
				break;
			}
			case TIMELINE_STATE.TS_MOVE03:
			case TIMELINE_STATE.TS_MOVE04:
			case TIMELINE_STATE.TS_MOVE06:
				break;
			}
		}

		private void MoveMapRepeate()
		{
			MoveMapTime();
			if (fTimeLeft == 0f)
			{
				if (bLoop)
				{
					StartUpdate(StartPos, EndPos);
				}
				else
				{
					bStartUpdate = false;
				}
			}
		}

		private void MoveCruve()
		{
			if (TrigerDatas.Count == 0)
			{
				return;
			}
			CheckNeedUnMove();
			if (fTimeLeft > 0f)
			{
				float num = 0.016f;
				Vector3 zero = Vector3.zero;
				float num2 = 0.033f;
				while (num > 0f && fTimeLeft > 0f)
				{
					num2 = ((!(num > 0.033f)) ? (num + 0.001f) : 0.033f);
					num -= num2;
					fTimeLeft -= num2;
					if (fTimeLeft < 0f)
					{
						fTimeLeft = 0f;
					}
					float num3 = mspd.Evaluate(fMoveTime - fTimeLeft);
					zero += num3 * num2 * Vector3.right;
				}
				NowPos += zero;
				CheckPlayerEnemyAndMove(zero, TrigerDatas[0].vMax, TrigerDatas[0].vMin);
				TrigerDatas[0].vMax += zero;
				TrigerDatas[0].vMin += zero;
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(sSyncID, 0, 0.016f.ToString("0.000"));
				}
			}
			base.transform.position = NowPos;
			if (fTimeLeft == 0f)
			{
				bStartUpdate = false;
			}
		}

		private void MoveMapContiune()
		{
			if (TrigerDatas.Count < 3)
			{
				return;
			}
			for (int num = TrigerDatas.Count - 2; num >= 0; num--)
			{
				if (Vector3.Dot(vMoveVector, TrigerDatas[num].EndPos) > TrigerDatas[0].fTimeLeft)
				{
					if (num == 0)
					{
						Vector3 endPos = TrigerDatas[num].EndPos;
						endPos = TrigerDatas[TrigerDatas.Count - 2].EndPos + (Vector3)TrigerDatas[num].tObj1;
						endPos -= TrigerDatas[num].EndPos;
						foreach (Transform listTriCollisionTran in TrigerDatas[num].listTriCollisionTrans)
						{
							listTriCollisionTran.position += endPos;
						}
						TrigerDatas[num].EndPos = TrigerDatas[num].EndPos + endPos;
					}
					else
					{
						Vector3 endPos2 = TrigerDatas[num].EndPos;
						endPos2 = TrigerDatas[num - 1].EndPos + (Vector3)TrigerDatas[num].tObj1;
						endPos2 -= TrigerDatas[num].EndPos;
						foreach (Transform listTriCollisionTran2 in TrigerDatas[num].listTriCollisionTrans)
						{
							listTriCollisionTran2.position += endPos2;
						}
						TrigerDatas[num].EndPos = TrigerDatas[num].EndPos + endPos2;
					}
				}
			}
			Vector3 vector = vMoveVector * (0.016f * fMoveSpeed);
			if (listCollisionTrans.Count > 0)
			{
				CheckPlayerEnemyAndMove(vector, TrigerDatas[0].vMax, TrigerDatas[0].vMin, listCollisionTrans, (nType & 4) != 0, listMutliMove[0].nIntParam1);
			}
			for (int num2 = TrigerDatas.Count - 2; num2 >= 0; num2--)
			{
				TrigerDatas[num2].EndPos += vector;
				foreach (Transform listTriCollisionTran3 in TrigerDatas[num2].listTriCollisionTrans)
				{
					listTriCollisionTran3.position += vector;
				}
			}
			if (listMutliMove[0].nIntParam1 == 1)
			{
				fMoveSpeed -= listMutliMove[0].tMovePos.x;
				if (fMoveSpeed <= 0f)
				{
					fMoveSpeed = 0f;
					bStartUpdate = false;
				}
			}
			else if (listMutliMove[0].tMovePos.z > fMoveSpeed)
			{
				fMoveSpeed += listMutliMove[0].tMovePos.y;
				if (fMoveSpeed > listMutliMove[0].tMovePos.z)
				{
					fMoveSpeed = listMutliMove[0].tMovePos.z;
				}
			}
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(sSyncID, 0, 0.016f.ToString("0.000"));
			}
		}

		private IEnumerator StartFallCoroutine()
		{
			int nchildcount2 = base.transform.childCount;
			List<FallingFloor> FallingFloors = new List<FallingFloor>();
			yield return new WaitForSeconds(fTimeLeftDelay);
			for (int j = 0; j < nchildcount2; j++)
			{
				FallingFloor[] componentsInChildren = base.transform.GetChild(j).GetComponentsInChildren<FallingFloor>(true);
				if (componentsInChildren != null)
				{
					FallingFloors.AddRange(componentsInChildren);
				}
			}
			Transform tFollowTrans = null;
			Vector3 tOriginPos = Vector3.zero;
			Vector3 zero = Vector3.zero;
			if (listMutliAni.Count > 0 && listMutliAni[0].aniName != "")
			{
				tFollowTrans = base.transform.Find(listMutliAni[0].aniName);
				tOriginPos = tFollowTrans.position;
			}
			nchildcount2 = FallingFloors.Count;
			for (int k = 0; k < nchildcount2 - 1; k++)
			{
				int index = k;
				float y = FallingFloors[k].transform.position.y;
				for (int l = k + 1; l < nchildcount2; l++)
				{
					if (FallingFloors[l].transform.position.y < y)
					{
						y = FallingFloors[l].transform.position.y;
						index = l;
					}
				}
				FallingFloor value = FallingFloors[k];
				FallingFloors[k] = FallingFloors[index];
				FallingFloors[index] = value;
			}
			int i = 0;
			while (i < nchildcount2)
			{
				if (bLoop)
				{
					FallingFloors[i].bNeedAttack = true;
					FallingFloors[i].nSkillID = nType;
				}
				if (listMutliAni.Count > 0 && ((uint)listMutliAni[0].nIntParam1 & (true ? 1u : 0u)) != 0)
				{
					FallingFloors[i].bRebornEvent = true;
				}
				FallingFloors[i].TriggerFall();
				if (fMoveTime > 0f)
				{
					yield return new WaitForSecondsRealtime(fMoveTime);
				}
				int num = i + 1;
				i = num;
			}
			while (tFollowTrans != null)
			{
				Vector3 vector = tFollowTrans.position - tOriginPos;
				tOriginPos = tFollowTrans.position;
				for (int m = 0; m < nchildcount2; m++)
				{
					FallingFloors[m].transform.localPosition -= vector;
					FallingFloors[m].Controller.LogicPosition = new VInt3(FallingFloors[m].Controller.LogicPosition.vec3 - vector);
				}
				base.transform.position += vector;
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		private void StageAtkBlock()
		{
			if ((nBitParam0 & 1) == 0)
			{
				return;
			}
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			List<StageSceneObjParam> tOutList = new List<StageSceneObjParam>();
			stageUpdate.GetStageSceneInB2D(EventB2D, ref tOutList);
			bool flag = false;
			foreach (StageSceneObjParam item in tOutList)
			{
				if (!flag)
				{
					flag = (item.bCanPlayBrokenSE = true);
				}
				item.WoundedStageSceneObj();
				item.BrokenStageSceneObj(fMoveTime, 1);
			}
		}

		private IEnumerator StageItemCoroutine()
		{
			while (!StageUpdate.gbStageReady)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			float fCheckTime = 0f;
			while (!StageUpdate.bIsHost)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				if (!bCheck)
				{
					fCheckTime += Time.deltaTime;
					if (fCheckTime >= 2f && fCheckTime >= fTimeLeftDelay)
					{
						StageUpdate.SyncStageObj(sSyncID, 1, "2,0", true);
						fCheckTime = 0f;
					}
				}
			}
			if (!bCheck)
			{
				fTimeLeft = 0f;
				fTimeLeftDelay = fDelayTime;
				bStartUpdate = true;
			}
		}

		private void StageItemCD()
		{
			if (!StageUpdate.bIsHost)
			{
				return;
			}
			fTimeLeft -= 0.016f;
			if (!(fTimeLeft <= 0f))
			{
				return;
			}
			fTimeLeft = 0f;
			float num = 0f;
			for (int i = 0; i < listMutliMove.Count; i++)
			{
				if (listMutliAni[i].nIntParam2 == 0 || listMutliAni[i].nIntParam2 == StageUpdate.gDifficulty)
				{
					num += listMutliMove[i].fMoveTime;
				}
			}
			float num2 = UnityEngine.Random.Range(0f, num);
			num = 0f;
			nType = -1;
			for (int j = 0; j < listMutliMove.Count; j++)
			{
				if (listMutliAni[j].nIntParam2 == 0 || listMutliAni[j].nIntParam2 == StageUpdate.gDifficulty)
				{
					num += listMutliMove[j].fMoveTime;
					if (num >= num2)
					{
						nType = j;
						break;
					}
				}
			}
			if (nType == -1)
			{
				bCheck = false;
				bStartUpdate = false;
				return;
			}
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(sSyncID, 1, "0," + nType);
			}
			StageResManager.LoadStageItemModel(listMutliMove[nType].nIntParam1, base.transform);
			bCheck = true;
			bStartUpdate = false;
		}

		private void SwitchMemObj()
		{
			listMutliMove[0].fDT -= 0.016f;
			if ((nType & 1) == 0)
			{
				tTmpColor = Color.white;
				tTmpColor.a = listMutliMove[0].fDT / listMutliMove[0].fMoveTime;
				float num = 0.016f / listMutliMove[0].fMoveTime;
				foreach (TrigerData trigerData in TrigerDatas)
				{
					((StageSceneObjParam)trigerData.tObj1).SetSceneObjAlpha(tTmpColor);
					if (tTmpColor.a < 0.05f && tTmpColor.a + num > 0.05f)
					{
						((StageSceneObjParam)trigerData.tObj1).SwitchB2DInStageSceneObj(false);
					}
				}
				if (listMutliMove[0].fDT <= 0f)
				{
					for (int num2 = base.transform.childCount - 1; num2 >= 0; num2--)
					{
						base.transform.GetChild(num2).gameObject.SetActive(false);
					}
					bStartUpdate = false;
				}
				return;
			}
			tTmpColor = Color.white;
			tTmpColor.a = 1f - listMutliMove[0].fDT / listMutliMove[0].fMoveTime;
			float num3 = 0.016f / listMutliMove[0].fMoveTime;
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				((StageSceneObjParam)trigerData2.tObj1).SetSceneObjAlpha(tTmpColor);
				if (tTmpColor.a > 0.05f && tTmpColor.a - num3 < 0.05f)
				{
					((StageSceneObjParam)trigerData2.tObj1).SwitchB2DInStageSceneObj(true);
				}
			}
			if (listMutliMove[0].fDT <= 0f)
			{
				for (int num4 = base.transform.childCount - 1; num4 >= 0; num4--)
				{
					base.transform.GetChild(num4).gameObject.SetActive(true);
				}
				bStartUpdate = false;
			}
		}

		private void PlayFlagCheck()
		{
			ArrayUInts[0] = (uint)nType;
			ArrayUInts[1] = 0u;
			ArrayUInts[2] = 0u;
			for (int i = 0; i < OldTrigerDatas.Count; i++)
			{
				bool flag = false;
				for (int j = 0; j < TrigerDatas.Count; j++)
				{
					if (OldTrigerDatas[i].tTriggerTransform == TrigerDatas[j].tTriggerTransform)
					{
						TrigerDatas.RemoveAt(j);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					OldTrigerDatas.RemoveAt(i);
					i--;
				}
			}
			int num = 0;
			for (int k = 0; k < OldTrigerDatas.Count; k++)
			{
				num |= 1 << OldTrigerDatas[k].nParam1 - 1;
				ArrayUInts[OldTrigerDatas[k].nParam1]++;
			}
			ArrayUInts[3] = 255u;
			ArrayUInts[4] = 255u;
			ArrayUInts[5] = 255u;
			ArrayUInts[6] = 255u;
			if (nType != 0)
			{
				foreach (MutliAni item in listMutliAni)
				{
					if (item.nIntParam1 == nType)
					{
						ArrayUInts[3] = (uint)item.nIntParam2 & 0xFFu;
						ArrayUInts[4] = (uint)(item.nIntParam2 & 0xFF00) >> 8;
						ArrayUInts[5] = (uint)(item.nIntParam2 & 0xFF0000) >> 16;
						ArrayUInts[6] = (uint)item.nIntParam2 >> 24;
						break;
					}
				}
			}
			NowPos.x = -9999f;
			if (num != 3)
			{
				for (int l = 0; l < ArrayFloats.Length; l++)
				{
					if (l + 1 == nType)
					{
						continue;
					}
					if (ArrayUInts[l + 1] != 0)
					{
						switch (ArrayUInts[l + 1])
						{
						case 2u:
							ArrayFloats[l] += 0.0208f;
							break;
						case 3u:
							ArrayFloats[l] += 0.032f;
							break;
						default:
							ArrayFloats[l] += 0.016f;
							break;
						}
						if (!tCountDownBar.gameObject.activeSelf)
						{
							tCountDownBar.gameObject.SetActive(true);
						}
						tCountDownBar.SetFValue(ArrayFloats[l] / listMutliMove[0].fMoveTime);
						if ((num & 4) == 0 && (num & 2) == 0)
						{
							if (!isFlagSEing)
							{
								PlaySE("BattleSE", "bt_pvp01_lp");
								isFlagSEing = true;
							}
						}
						else if (isFlagSEing)
						{
							PlaySE("BattleSE", "bt_pvp01_stop");
							isFlagSEing = false;
						}
					}
					else if (ArrayFloats[l] > 0f)
					{
						ArrayFloats[l] -= 0.016f;
						if (ArrayFloats[l] <= 0f)
						{
							ArrayFloats[l] = 0f;
							if (tCountDownBar.gameObject.activeSelf)
							{
								tCountDownBar.gameObject.SetActive(false);
							}
						}
						else
						{
							tCountDownBar.SetFValue(ArrayFloats[l] / listMutliMove[0].fMoveTime);
						}
						if (isFlagSEing)
						{
							PlaySE("BattleSE", "bt_pvp01_stop");
							isFlagSEing = false;
						}
					}
					if (ArrayFloats[l] > listMutliMove[0].fMoveTime)
					{
						if (NowPos.x < ArrayFloats[l])
						{
							NowPos.x = ArrayFloats[l];
							ArrayUInts[0] = (uint)(l + 1);
						}
					}
					else
					{
						if (!(ArrayFloats[l] > 0f) || !(ArrayFloats[l] > NowPos.x))
						{
							continue;
						}
						NowPos.x = ArrayFloats[l];
						foreach (MutliAni item2 in listMutliAni)
						{
							if (item2.nIntParam1 == l + 1)
							{
								float num2 = ArrayFloats[l] / listMutliMove[0].fMoveTime;
								ArrayUInts[3] = (uint)((float)ArrayUInts[3] * (1f - num2) + (float)(item2.nIntParam2 & 0xFF) * num2);
								ArrayUInts[4] = (uint)((float)ArrayUInts[4] * (1f - num2) + (float)((item2.nIntParam2 & 0xFF00) >> 8) * num2);
								ArrayUInts[5] = (uint)((float)ArrayUInts[5] * (1f - num2) + (float)((item2.nIntParam2 & 0xFF0000) >> 16) * num2);
								ArrayUInts[6] = (uint)((float)ArrayUInts[6] * (1f - num2) + (float)(item2.nIntParam2 >> 24) * num2);
								break;
							}
						}
					}
				}
			}
			if (ArrayUInts[0] != (uint)nType)
			{
				if (StageUpdate.bIsHost)
				{
					if (tCountDownBar.gameObject.activeSelf)
					{
						tCountDownBar.gameObject.SetActive(false);
					}
					nType = (int)ArrayUInts[0];
					for (int num3 = base.transform.childCount - 1; num3 >= 0; num3--)
					{
						GameObject gameObject = base.transform.GetChild(num3).gameObject;
						for (int m = 0; m < listMutliAni.Count; m++)
						{
							if (listMutliAni[m].aniName == gameObject.name)
							{
								gameObject.SetActive(listMutliAni[m].nIntParam1 == nType);
							}
						}
					}
					foreach (MutliAni item3 in listMutliAni)
					{
						if (item3.nIntParam1 == nType)
						{
							ArrayUInts[3] = (uint)item3.nIntParam2 & 0xFFu;
							ArrayUInts[4] = (uint)((item3.nIntParam2 & 0xFF00) >> 8);
							ArrayUInts[5] = (uint)((item3.nIntParam2 & 0xFF0000) >> 16);
							ArrayUInts[6] = (uint)(item3.nIntParam2 >> 24);
							break;
						}
					}
					tTmpColor.r = (float)ArrayUInts[3] / 255f;
					tTmpColor.g = (float)ArrayUInts[4] / 255f;
					tTmpColor.b = (float)ArrayUInts[5] / 255f;
					tTmpColor.a = (float)ArrayUInts[6] / 255f;
					SetOtherFXColor(tTmpColor);
					for (int n = 0; n < ArrayFloats.Length; n++)
					{
						ArrayFloats[n] = 0f;
					}
					fTimeLeft = 0f;
					if (StageUpdate.gbIsNetGame)
					{
						StageUpdate.SyncStageObj(sSyncID, 1, 1 + "," + nType);
					}
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_INFLAG_RANGE, nType, listMutliMove[0].nIntParam1);
					PvpReportUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpReportUI>("UI_PvpReport");
					if ((bool)uI)
					{
						for (int num4 = 0; num4 < OldTrigerDatas.Count; num4++)
						{
							if (OldTrigerDatas[num4].nParam1 == nType && OldTrigerDatas[num4].tObj1 != null)
							{
								string[] array = new string[2] { "12C2FFFF", "FF3B3CFF" };
								uI.SetMsg("<color=#" + array[nType - 1] + ">" + (OldTrigerDatas[num4].tObj1 as OrangeCharacter).sPlayerName + "</color>", "", bmge);
								break;
							}
						}
					}
					else
					{
						string sPlayerName = "";
						for (int num5 = 0; num5 < OldTrigerDatas.Count; num5++)
						{
							if (OldTrigerDatas[num5].nParam1 == nType && OldTrigerDatas[num5].tObj1 != null)
							{
								string[] array2 = new string[2] { "12C2FFFF", "FF3B3CFF" };
								sPlayerName = "<color=#" + array2[nType - 1] + ">" + (OldTrigerDatas[num5].tObj1 as OrangeCharacter).sPlayerName + "</color>";
								break;
							}
						}
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReport", delegate(PvpReportUI ui)
						{
							ui.SetMsg(sPlayerName, "", bmge);
						});
					}
				}
				PlaySE("BattleSE", "bt_pvp01_stop");
				PlaySE("BattleSE", "bt_pvp02");
				isFlagSEing = false;
			}
			else
			{
				fTimeLeft += 0.016f;
				tTmpColor.r = (float)ArrayUInts[3] / 255f;
				tTmpColor.g = (float)ArrayUInts[4] / 255f;
				tTmpColor.b = (float)ArrayUInts[5] / 255f;
				tTmpColor.a = (float)ArrayUInts[6] / 255f;
				SetOtherFXColor(tTmpColor);
			}
			for (int num6 = 0; num6 < TrigerDatas.Count; num6++)
			{
				OrangeCharacter component = TrigerDatas[num6].tTriggerTransform.GetComponent<OrangeCharacter>();
				if ((bool)component)
				{
					TrigerDatas[num6].nParam1 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(component);
					TrigerDatas[num6].tObj1 = component;
					if (TrigerDatas[num6].nParam1 != nType)
					{
						if (!tCountDownBar.gameObject.activeSelf)
						{
							tCountDownBar.gameObject.SetActive(true);
						}
						tCountDownBar.SetFValue(ArrayFloats[TrigerDatas[num6].nParam1 - 1] / listMutliMove[0].fMoveTime);
					}
				}
				OldTrigerDatas.Add(TrigerDatas[num6]);
			}
			if (StageUpdate.bIsHost)
			{
				if (fTimeLeft >= listMutliMove[1].fMoveTime)
				{
					fTimeLeft -= listMutliMove[1].fMoveTime;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYER_INFLAG_RANGE, nType, listMutliMove[1].nIntParam1);
				}
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(sSyncID, 0, fTimeLeft.ToString("0.000") + "," + ArrayFloats[0].ToString("0.000") + "," + ArrayFloats[1].ToString("0.000"));
				}
			}
		}

		private void SetOtherFXColor(Color tColor)
		{
			int childCount = base.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				GameObject gameObject = base.transform.GetChild(i).gameObject;
				bool flag = false;
				for (int j = 0; j < listMutliAni.Count; j++)
				{
					if (listMutliAni[j].aniName == gameObject.name)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					pvp_event component = gameObject.GetComponent<pvp_event>();
					if (component != null)
					{
						component.SetColor(tColor);
					}
				}
			}
		}

		private void RideObj()
		{
			if (((uint)nType & (true ? 1u : 0u)) != 0)
			{
				if (fMoveSpeed >= 0f && fMoveSpeed < listMutliMove[0].tMovePos.x)
				{
					fMoveSpeed += listMutliMove[0].tMovePos.x * 0.016f;
				}
				else if (fMoveSpeed < 0f && fMoveSpeed > 0f - listMutliMove[0].tMovePos.x)
				{
					fMoveSpeed += (0f - listMutliMove[0].tMovePos.x) * 0.016f;
				}
			}
			if (vMoveVector.y > 0f)
			{
				vMoveVector.y -= listMutliMove[0].tMovePos.y * 0.016f * listMutliMove[0].tMovePos.z;
			}
			else
			{
				vMoveVector.y -= listMutliMove[0].tMovePos.y * 0.016f;
			}
			Vector2 vector = new Vector2(fMoveSpeed * 0.016f, vMoveVector.y * 0.016f);
			float num = 0f;
			NowPos = base.transform.localPosition;
			Vector2 zero = Vector2.zero;
			Vector2 vector2 = vector;
			List<Transform> list = new List<Transform>();
			bool flag = false;
			bool flag2 = false;
			while (vector2.x != 0f || vector2.y != 0f)
			{
				vector = vector2;
				num = vector.magnitude;
				NowPos = base.transform.position;
				if (listController2D.Count > 0)
				{
					RaycastHit2D raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheck(listController2D[0], new Vector3(zero.x, zero.y), vector / num, num, listController2D[0].collisionMask, listCollisionTrans);
					if ((bool)raycastHit2D)
					{
						bool flag3 = true;
						float num2 = Vector2.Dot(vector / num, raycastHit2D.normal);
						flag = false;
						if (raycastHit2D.normal == Vector2.zero)
						{
							zero += vector;
							vector2 = Vector2.zero;
							flag = true;
						}
						else if (num2 > 0.95f)
						{
							zero += vector;
							vector2 = Vector2.zero;
							flag = true;
						}
						else if (Vector2.Dot(raycastHit2D.normal, Vector2.up) > 0.95f)
						{
							if (raycastHit2D.distance == 0f)
							{
								Vector2 vector3 = Vector2.up * 0.02f;
								RaycastHit2D raycastHit2D2;
								do
								{
									raycastHit2D2 = StageResManager.ObjMoveCollisionWithBoxCheck(listController2D[0], new Vector3(zero.x + vector3.x, zero.y + vector3.y), vector / num, num, listController2D[0].collisionMask, listCollisionTrans);
									if ((bool)raycastHit2D2 && Vector2.Dot(raycastHit2D2.normal, Vector2.up) > 0.95f)
									{
										vector3 += Vector2.up * 0.02f;
									}
								}
								while ((bool)raycastHit2D2 && Vector2.Dot(raycastHit2D2.normal, Vector2.up) > 0.95f);
								if (vector2.y < 0f)
								{
									vector2.y = 0f;
								}
								zero += vector3;
								if ((bool)raycastHit2D2)
								{
									flag = false;
									raycastHit2D = raycastHit2D2;
								}
								else
								{
									flag2 = true;
									flag = true;
									if (vMoveVector.y < 0f)
									{
										vMoveVector.y = 0f;
									}
								}
							}
							else
							{
								vector = vector.normalized * raycastHit2D.distance;
								vector2 -= vector;
								vector2.y = 0f;
								flag2 = true;
								vMoveVector.y = 0f;
								zero += vector;
								flag = true;
							}
						}
						else if (num2 < -0.9f)
						{
							Vector2 vector4 = vector / num;
							float x = vector4.x;
							if (x > 0f)
							{
								vector4.x = 0f - vector4.y;
								vector4.y = x;
							}
							else
							{
								vector4.x = vector4.y;
								vector4.y = 0f - x;
							}
							vector4 *= 0.05f;
							RaycastHit2D raycastHit2D2 = StageResManager.ObjMoveCollisionWithBoxCheck(listController2D[0], new Vector3(zero.x + vector4.x, zero.y + vector4.y), vector / num, num, listController2D[0].collisionMask, listCollisionTrans);
							if (!raycastHit2D2)
							{
								zero += vector4;
								vector = vector.normalized * raycastHit2D.distance;
								vector2 -= vector;
								zero += vector;
								flag3 = false;
								flag = true;
							}
						}
						if (!flag)
						{
							vector = vector.normalized * raycastHit2D.distance;
							vector2 -= vector;
							if (((uint)nType & 2u) != 0 && ((Vector2.Dot(Vector2.left, raycastHit2D.normal) > 0.95f && vector.x > 0f) || (Vector2.Dot(Vector2.right, raycastHit2D.normal) > 0.95f && vector.x < 0f)))
							{
								vector2.x = 0f - vector2.x;
								fMoveSpeed = 0f - fMoveSpeed;
							}
							else
							{
								Vector2 vector5 = raycastHit2D.normal * Vector2.Dot(vector2, raycastHit2D.normal);
								vector2 -= vector5;
								zero += vector;
								vector = new Vector2(fMoveSpeed, vMoveVector.y);
								vector5 = raycastHit2D.normal * Vector2.Dot(vector, raycastHit2D.normal);
								vector -= vector5;
								fMoveSpeed = vector.x;
								vMoveVector.y = vector.y;
							}
						}
						if (flag3)
						{
							list.Add(raycastHit2D.transform);
							listCollisionTrans.Add(raycastHit2D.transform);
						}
					}
					else
					{
						zero += vector;
						vector2 = Vector2.zero;
					}
				}
				else
				{
					zero += vector;
					vector2 = Vector2.zero;
				}
			}
			if (flag2)
			{
				fMoveSpeed -= listMutliMove[1].tMovePos.x * 0.016f;
			}
			Vector3 zero2 = Vector3.zero;
			if (TrigerDatas.Count > 0)
			{
				for (int num3 = TrigerDatas.Count - 1; num3 >= 0; num3--)
				{
					zero2 = new Vector3(zero.x, zero.y, 0f);
					TrigerData trigerData = TrigerDatas[num3];
					OrangeCharacter orangeCharacter = trigerData.tObj1 as OrangeCharacter;
					Controller2D controller = orangeCharacter.Controller;
					if (!orangeCharacter.Controller.Collider2D.bounds.Intersects(EventB2D.bounds))
					{
						TrigerDatas.Remove(trigerData);
						orangeCharacter.ConnectStandardCtrlCB();
						orangeCharacter.PlayerPressJumpCB = trigerData.BackCB;
						bCheck = true;
					}
					else
					{
						controller.UpdateRaycastOrigins();
						if (zero2.x != 0f)
						{
							RaycastHit2D raycastHit2D = controller.ObjectMeeting(0f - zero2.x, 0f, controller.collisionMask);
							if ((bool)raycastHit2D && !orangeCharacter.IsDead() && listController2D.Count > 0 && raycastHit2D.transform == listController2D[0].transform)
							{
								EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
								stageSkillAtkTargetParam.nSkillID = 950001;
								stageSkillAtkTargetParam.bAtkNoCast = true;
								stageSkillAtkTargetParam.tPos = orangeCharacter.transform.position;
								stageSkillAtkTargetParam.tDir = zero2.normalized;
								stageSkillAtkTargetParam.bBuff = false;
								stageSkillAtkTargetParam.tTrans = orangeCharacter.transform;
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
							}
							raycastHit2D = controller.ObjectMeeting(zero2.x, 0f, controller.collisionMask);
							if ((bool)raycastHit2D)
							{
								if (zero2.x > 0f)
								{
									zero2.x = raycastHit2D.distance;
								}
								else
								{
									zero2.x = 0f - raycastHit2D.distance;
								}
							}
						}
						if (zero2.y != 0f)
						{
							RaycastHit2D raycastHit2D = controller.ObjectMeeting(0f, zero2.y, controller.collisionMask);
							if ((bool)raycastHit2D)
							{
								if (zero2.y > 0f)
								{
									zero2.y = raycastHit2D.distance;
								}
								else if (listController2D.Count > 0 && raycastHit2D.transform != listController2D[0].transform)
								{
									zero2.y = 0f - raycastHit2D.distance;
								}
							}
						}
						controller.transform.position = controller.transform.position + zero2;
						controller.LogicPosition = new VInt3(controller.LogicPosition.vec3 + zero2);
					}
				}
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
			}
			zero2 = new Vector3(zero.x, zero.y, 0f);
			NowPos += zero2;
			base.transform.position = NowPos;
			foreach (Transform item in list)
			{
				if (listCollisionTrans.Contains(item))
				{
					listCollisionTrans.Remove(item);
				}
			}
		}

		private void PathObjMove()
		{
			int num = 0;
			Vector3 zero = Vector3.zero;
			float num2 = 0f;
			Vector3 zero2 = Vector3.zero;
			Vector3 zero3 = Vector3.zero;
			foreach (TrigerData trigerData in TrigerDatas)
			{
				num2 = fMoveSpeed * 0.016f;
				zero2 = Vector3.zero;
				while (num2 > 0f)
				{
					num = trigerData.nParam1;
					zero = (OldTrigerDatas[num + 1].EndPos - OldTrigerDatas[num].EndPos).normalized;
					zero3 = trigerData.tTriggerTransform.position + zero2;
					float num3 = Vector3.Distance(OldTrigerDatas[num + 1].EndPos, zero3);
					if (num2 > num3)
					{
						zero2 += OldTrigerDatas[num + 1].EndPos - zero3;
						num2 -= num3;
						trigerData.nParam1++;
						if (trigerData.nParam1 >= OldTrigerDatas.Count - 1)
						{
							trigerData.nParam1 = 0;
						}
					}
					else
					{
						zero2 += zero * num2;
						num2 = 0f;
					}
				}
				CheckPlayerEnemyAndMove(zero2, trigerData.vMax, trigerData.vMin, trigerData.listTriCollisionTrans);
				trigerData.vMax += zero2;
				trigerData.vMin += zero2;
				trigerData.tTriggerTransform.position += zero2;
			}
			fTimeLeft += 0.016f;
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(sSyncID, 0, fTimeLeft.ToString("0.000"));
			}
		}

		private void CircleObjMove()
		{
			CircleObjMove(0.016f);
		}

		private void CircleObjMove(float fUpdateTime)
		{
			float num = fMoveSpeed * fUpdateTime;
			if (bLoop)
			{
				num = 0f - num;
			}
			vMoveVector.x += num;
			if (vMoveVector.x < -360f)
			{
				vMoveVector.x += 360f;
			}
			if (vMoveVector.x > 360f)
			{
				vMoveVector.x -= 360f;
			}
			Vector3 position = base.transform.position;
			foreach (TrigerData trigerData in TrigerDatas)
			{
				Vector3 localPosition = trigerData.tTriggerTransform.localPosition;
				Vector3 vector = Quaternion.Euler(0f, 0f, num) * localPosition;
				Vector3 vector2 = vector - localPosition;
				if (trigerData.nParam1 == 1)
				{
					CheckPlayerEnemyAndMove(vector2, trigerData.vMax, trigerData.vMin, trigerData.listTriCollisionTrans);
				}
				trigerData.vMax += vector2;
				trigerData.vMin += vector2;
				trigerData.EndPos = vector;
			}
			foreach (TrigerData oldTrigerData in OldTrigerDatas)
			{
				oldTrigerData.fTimeLeft += num;
				if (oldTrigerData.fTimeLeft > 180f)
				{
					oldTrigerData.fTimeLeft = 0f;
				}
				if (oldTrigerData.fTimeLeft < -180f)
				{
					oldTrigerData.fTimeLeft = 0f;
				}
				oldTrigerData.tTriggerTransform.localRotation = Quaternion.AngleAxis(oldTrigerData.fTimeLeft, Vector3.up);
			}
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				trigerData2.tTriggerTransform.localPosition = trigerData2.EndPos;
			}
			fTimeLeft += fUpdateTime;
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(sSyncID, 0, fTimeLeft.ToString("0.000"));
			}
		}

		private void AngleObjMove()
		{
			float num = vMoveVector.x * 0.016f;
			fMoveSpeed += num;
			if (nType == 0)
			{
				vMoveVector.x -= vMoveVector.y * 0.016f;
				if (vMoveVector.x < 0f - listMutliMove[0].tMovePos.x)
				{
					vMoveVector.x = 0f - listMutliMove[0].tMovePos.x;
				}
				if (fMoveSpeed < 0f)
				{
					nType = 1;
				}
			}
			else if (nType == 1)
			{
				vMoveVector.x += vMoveVector.y * 0.016f;
				if (vMoveVector.x > listMutliMove[0].tMovePos.x)
				{
					vMoveVector.x = listMutliMove[0].tMovePos.x;
				}
				if (fMoveSpeed > 0f)
				{
					nType = 0;
				}
			}
			Vector3 position = base.transform.position;
			foreach (TrigerData trigerData in TrigerDatas)
			{
				Vector3 localPosition = trigerData.tTriggerTransform.localPosition;
				Vector3 vector = Quaternion.Euler(0f, 0f, num) * localPosition;
				Vector3 vector2 = vector - localPosition;
				CheckPlayerEnemyAndMove(vector2, trigerData.vMax, trigerData.vMin, trigerData.listTriCollisionTrans);
				trigerData.vMax += vector2;
				trigerData.vMin += vector2;
				trigerData.EndPos = vector;
			}
			foreach (TrigerData oldTrigerData in OldTrigerDatas)
			{
				oldTrigerData.fTimeLeft += num;
				if (oldTrigerData.fTimeLeft > 180f)
				{
					oldTrigerData.fTimeLeft -= 180f;
				}
				if (oldTrigerData.fTimeLeft < -180f)
				{
					oldTrigerData.fTimeLeft += 180f;
				}
				oldTrigerData.tTriggerTransform.localRotation = Quaternion.AngleAxis(oldTrigerData.fTimeLeft, Vector3.up);
			}
			foreach (TrigerData trigerData2 in TrigerDatas)
			{
				trigerData2.tTriggerTransform.localPosition = trigerData2.EndPos;
			}
		}

		private void RotateTargetObj()
		{
			vMoveVector.y = vMoveVector.x * 0.016f;
			foreach (TrigerData oldTrigerData in OldTrigerDatas)
			{
				oldTrigerData.fTimeLeft += vMoveVector.y;
				if (oldTrigerData.fTimeLeft > 180f)
				{
					oldTrigerData.fTimeLeft -= 360f;
				}
				if (oldTrigerData.fTimeLeft < -180f)
				{
					oldTrigerData.fTimeLeft += 360f;
				}
				oldTrigerData.tTriggerTransform.localRotation = Quaternion.AngleAxis(oldTrigerData.fTimeLeft, EndPos);
			}
			if (fMoveSpeed > 0f)
			{
				fMoveSpeed -= Mathf.Abs(vMoveVector.y);
				if (fMoveSpeed <= 0f)
				{
					bStartUpdate = false;
				}
			}
			if (nType == 0)
			{
				return;
			}
			if (nType == 1)
			{
				if (Mathf.Abs(vMoveVector.x) < Mathf.Abs(listMutliMove[0].fMoveTime))
				{
					vMoveVector.x += fMoveTime;
				}
				if (Mathf.Abs(vMoveVector.x) >= Mathf.Abs(listMutliMove[0].fMoveTime))
				{
					vMoveVector.x = listMutliMove[0].fMoveTime;
					nType = 0;
				}
			}
			else if (nType == 2)
			{
				if (Mathf.Abs(vMoveVector.x) > Mathf.Abs(fMoveTime))
				{
					vMoveVector.x -= fMoveTime;
					return;
				}
				vMoveVector.x = 0f;
				nType = 0;
				bStartUpdate = false;
			}
		}

		private void InRangeBuff()
		{
			tmpBoundA = EventB2D.bounds;
			for (int num = TrigerDatas.Count - 1; num >= 0; num--)
			{
				OrangeCharacter orangeCharacter = TrigerDatas[num].tObj1 as OrangeCharacter;
				if (orangeCharacter.bIsNpcCpy || (int)orangeCharacter.Hp <= 0)
				{
					TrigerDatas.RemoveAt(num);
				}
				else if (!orangeCharacter.Controller.Collider2D.enabled)
				{
					TrigerDatas.RemoveAt(num);
				}
				else
				{
					tmpBoundB = orangeCharacter.Controller.Collider2D.bounds;
					if (!StageResManager.CheckBoundsContainNoZEffect(ref tmpBoundA, ref tmpBoundB))
					{
						if (((uint)nBitParam0 & (true ? 1u : 0u)) != 0 && listMutliAni[0].nIntParam1 != 0)
						{
							orangeCharacter.selfBuffManager.RemoveBuffByCONDITIONID(listMutliAni[0].nIntParam1);
						}
						TrigerDatas.RemoveAt(num);
					}
					else
					{
						TrigerDatas[num].fTimeLeft -= 0.016f;
						if (TrigerDatas[num].fTimeLeft <= 0f && listMutliAni[0].nIntParam1 != 0)
						{
							orangeCharacter.selfBuffManager.AddBuff(listMutliAni[0].nIntParam1, 0, orangeCharacter.MaxHp, 0);
							TrigerDatas[num].fTimeLeft = fMoveTime;
						}
					}
				}
			}
			if (TrigerDatas.Count == 0)
			{
				bStartUpdate = false;
			}
		}

		private void RidetObj()
		{
			Vector2 vector;
			if (listMutliAni[0].nIntParam1 == 0)
			{
				if (((uint)nType & (true ? 1u : 0u)) != 0)
				{
					if (((uint)nType & 8u) != 0)
					{
						if (vMoveVector.x < listMutliMove[0].tMovePos.x)
						{
							vMoveVector.x = listMutliMove[0].tMovePos.x;
						}
					}
					else if (vMoveVector.x >= 0f && vMoveVector.x < listMutliMove[0].tMovePos.x)
					{
						vMoveVector.x = listMutliMove[0].tMovePos.x;
					}
					else if (vMoveVector.x < 0f && vMoveVector.x > 0f - listMutliMove[0].tMovePos.x)
					{
						vMoveVector.x = 0f - listMutliMove[0].tMovePos.x;
					}
				}
				if (vMoveVector.y > 0f)
				{
					vMoveVector.y -= listMutliMove[0].tMovePos.y * 0.016f * listMutliMove[0].tMovePos.z;
				}
				else
				{
					vMoveVector.y -= listMutliMove[0].tMovePos.y * 0.016f;
				}
			}
			else
			{
				vector = vMoveVector.normalized;
				vMoveVector.x -= vector.x * listMutliMove[1].tMovePos.z * 0.016f;
				vMoveVector.y -= vector.y * listMutliMove[1].tMovePos.z * 0.016f;
				if (vMoveVector.x * vector.x < 0f)
				{
					vMoveVector.x = 0f;
				}
				if (vMoveVector.y * vector.y < 0f)
				{
					vMoveVector.y = 0f;
				}
			}
			if (vMoveVector.x == 0f && vMoveVector.y == 0f)
			{
				bStartUpdate = false;
				Animation[] componentsInChildren = base.transform.GetComponentsInChildren<Animation>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Stop();
				}
				ParticleSystem[] componentsInChildren2 = base.transform.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].Stop();
				}
				if (TrigerDatas.Count > 0)
				{
					for (int num = TrigerDatas.Count - 1; num >= 0; num--)
					{
						TrigerData trigerData = TrigerDatas[num];
						OrangeCharacter obj = trigerData.tObj1 as OrangeCharacter;
						TrigerDatas.Remove(trigerData);
						obj.ConnectStandardCtrlCB();
						obj.PlayerPressJumpCB = trigerData.BackCB;
					}
				}
				return;
			}
			vector = vMoveVector * 0.016f;
			float num2 = 0f;
			NowPos = base.transform.localPosition;
			Vector2 zero = Vector2.zero;
			Vector2 vector2 = vector;
			Vector2 vector3 = vector;
			List<Transform> list = new List<Transform>();
			bool flag = false;
			bool flag2 = false;
			Controller2D controller2D = OldTrigerDatas[0].tObj2 as Controller2D;
			float num3 = 0f;
			int num4 = 0;
			while (vector2.x != 0f || vector2.y != 0f)
			{
				vector = vector2;
				vector3 = vector.normalized;
				vector.y += num3;
				num2 = vector.magnitude;
				NowPos = base.transform.position;
				num4++;
				RaycastHit2D raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheckX(controller2D.Collider2D, new Vector3(zero.x, zero.y), vector / num2, num2, controller2D.collisionMask, listCollisionTrans);
				if ((bool)raycastHit2D)
				{
					bool flag3 = true;
					float num5 = Vector2.Dot(vector3, raycastHit2D.normal);
					flag = false;
					if (raycastHit2D.normal == Vector2.zero)
					{
						zero += vector;
						vector2 = Vector2.zero;
						flag = true;
					}
					else if (num5 > 0.95f)
					{
						vector.y -= num3;
						num3 = 0f;
						zero += vector;
						vector2 = Vector2.zero;
						flag = true;
					}
					else if (Vector2.Dot(raycastHit2D.normal, Vector2.up) > 0.97f)
					{
						if (raycastHit2D.distance == 0f)
						{
							if (vector.y < 0f)
							{
								vector.y = 0f;
							}
							num2 = vector.magnitude;
							Vector2 vector4 = Vector2.up * 0.02f;
							RaycastHit2D raycastHit2D2;
							do
							{
								raycastHit2D2 = StageResManager.ObjMoveCollisionWithBoxCheckX(controller2D.Collider2D, new Vector3(zero.x + vector4.x, zero.y + vector4.y), vector / num2, num2, controller2D.collisionMask, listCollisionTrans);
								if ((bool)raycastHit2D2 && Vector2.Dot(raycastHit2D2.normal, Vector2.up) > 0.95f)
								{
									vector4 += Vector2.up * 0.02f;
								}
							}
							while ((bool)raycastHit2D2 && Vector2.Dot(raycastHit2D2.normal, Vector2.up) > 0.95f);
							if (vector2.y < 0f)
							{
								vector2.y = 0f;
							}
							zero += vector4;
							if ((bool)raycastHit2D2)
							{
								flag = false;
								raycastHit2D = raycastHit2D2;
							}
							else
							{
								flag2 = true;
								flag = true;
								if (vMoveVector.y < 0f)
								{
									vMoveVector.y = 0f;
								}
							}
							num3 = 0f;
						}
						else
						{
							vector = vector.normalized * raycastHit2D.distance;
							vector2 -= vector;
							num3 = 0f;
							vector2.y = 0f;
							flag2 = true;
							vMoveVector.y = 0f;
							zero += vector;
							flag = true;
						}
					}
					else if (num5 < -0.9f)
					{
						Vector2 vector5 = vector3;
						float x = vector5.x;
						if (x > 0f)
						{
							vector5.x = 0f - vector5.y;
							vector5.y = x;
						}
						else
						{
							vector5.x = vector5.y;
							vector5.y = 0f - x;
						}
						vector5 *= 0.05f;
						RaycastHit2D raycastHit2D2 = StageResManager.ObjMoveCollisionWithBoxCheckX(controller2D.Collider2D, new Vector3(zero.x + vector5.x, zero.y + vector5.y), vector / num2, num2, controller2D.collisionMask, listCollisionTrans);
						if (!raycastHit2D2)
						{
							zero += vector5;
							vector = vector.normalized * raycastHit2D.distance;
							vector2 -= vector;
							zero += vector;
							num3 = 0f;
							flag3 = false;
							flag = true;
						}
					}
					if (!flag)
					{
						vector = vector.normalized * raycastHit2D.distance;
						if (vector2.y > 0f)
						{
							if (vector.y < 0f)
							{
								vector2.x -= vector.x;
							}
							else
							{
								vector2 -= vector;
								if (vector2.y < 0f)
								{
									vector2.y = 0f;
								}
							}
						}
						else
						{
							vector2 -= vector;
							if (vector2.y > 0f)
							{
								vector2.y = 0f;
							}
						}
						if (((uint)nType & 2u) != 0 && ((Vector2.Dot(Vector2.left, raycastHit2D.normal) > 0.95f && vector.x > 0f) || (Vector2.Dot(Vector2.right, raycastHit2D.normal) > 0.95f && vector.x < 0f)))
						{
							vector2.x = 0f - vector2.x;
							vMoveVector.x = 0f - vMoveVector.x;
							zero += vector;
							num3 = 0f;
						}
						else
						{
							zero += vector;
							if (!(Vector2.Dot(vector2, raycastHit2D.normal) > 0f))
							{
								Vector2 vector6 = raycastHit2D.normal * Vector2.Dot(vector2, raycastHit2D.normal);
								vector2 -= vector6;
								vector = new Vector2(vMoveVector.x, vMoveVector.y);
								vector6 = raycastHit2D.normal * Vector2.Dot(vector, raycastHit2D.normal);
								vector -= vector6;
								vMoveVector.x = vector.x;
								vMoveVector.y = vector.y;
							}
							num3 = 0f;
						}
					}
					if (flag3)
					{
						list.Add(raycastHit2D.transform);
						listCollisionTrans.Add(raycastHit2D.transform);
					}
					continue;
				}
				if (num4 == 1 && ((uint)nType & 0x10u) != 0)
				{
					num3 = -3f;
					continue;
				}
				zero += vector;
				vector2 = Vector2.zero;
				Vector2 vector7 = zero;
				float num6 = 57.29578f * Mathf.Acos(Vector2.Dot(Vector2.right, vector7.normalized));
				if (vector7.y > 0f)
				{
					num6 = 0f - num6;
				}
				if (OldTrigerDatas[0].fTimeLeft > num6)
				{
					if (OldTrigerDatas[0].EndPos.x > 0f)
					{
						OldTrigerDatas[0].EndPos.x = 0f;
					}
					OldTrigerDatas[0].EndPos.x += (num6 - OldTrigerDatas[0].fTimeLeft) * 0.03f;
					OldTrigerDatas[0].fTimeLeft += OldTrigerDatas[0].EndPos.x;
					if (OldTrigerDatas[0].fTimeLeft < num6)
					{
						OldTrigerDatas[0].fTimeLeft = num6;
					}
				}
				else if (OldTrigerDatas[0].fTimeLeft < num6)
				{
					if (OldTrigerDatas[0].EndPos.x < 0f)
					{
						OldTrigerDatas[0].EndPos.x = 0f;
					}
					OldTrigerDatas[0].EndPos.x += (num6 - OldTrigerDatas[0].fTimeLeft) * 0.03f;
					OldTrigerDatas[0].fTimeLeft += OldTrigerDatas[0].EndPos.x;
					if (OldTrigerDatas[0].fTimeLeft > num6)
					{
						OldTrigerDatas[0].fTimeLeft = num6;
					}
				}
				if (Mathf.Abs(OldTrigerDatas[0].fTimeLeft) > listMutliMove[1].tMovePos.y)
				{
					if (OldTrigerDatas[0].fTimeLeft > 0f)
					{
						OldTrigerDatas[0].fTimeLeft = listMutliMove[1].tMovePos.y;
					}
					else
					{
						OldTrigerDatas[0].fTimeLeft = 0f - listMutliMove[1].tMovePos.y;
					}
				}
				OldTrigerDatas[0].tTriggerTransform.rotation = (Quaternion)OldTrigerDatas[0].tObj1;
				OldTrigerDatas[0].tTriggerTransform.RotateAround(OldTrigerDatas[0].tTriggerTransform.position, Vector3.back, OldTrigerDatas[0].fTimeLeft);
				if ((bool)StageResManager.ObjOverlapWithBoxCheckX(controller2D.Collider2D, Vector2.zero, controller2D.collisionMask, listCollisionTrans))
				{
					OldTrigerDatas[0].fTimeLeft = OldTrigerDatas[0].EndPos.y;
					OldTrigerDatas[0].tTriggerTransform.rotation = (Quaternion)OldTrigerDatas[0].tObj1;
					OldTrigerDatas[0].tTriggerTransform.RotateAround(OldTrigerDatas[0].tTriggerTransform.position, Vector3.back, OldTrigerDatas[0].fTimeLeft);
				}
				else
				{
					OldTrigerDatas[0].EndPos.y = OldTrigerDatas[0].fTimeLeft;
				}
			}
			if (flag2 && vMoveVector.x != 0f)
			{
				if (vMoveVector.x > 0f)
				{
					vMoveVector.x -= listMutliMove[1].tMovePos.x * 0.016f;
					if (vMoveVector.x <= 0f)
					{
						vMoveVector.x = 0f;
					}
				}
				else
				{
					vMoveVector.x += listMutliMove[1].tMovePos.x * 0.016f;
					if (vMoveVector.x >= 0f)
					{
						vMoveVector.x = 0f;
					}
				}
			}
			Vector3 zero2 = Vector3.zero;
			if (TrigerDatas.Count > 0)
			{
				for (int num7 = TrigerDatas.Count - 1; num7 >= 0; num7--)
				{
					TrigerData trigerData2 = TrigerDatas[num7];
					OrangeCharacter orangeCharacter = trigerData2.tObj1 as OrangeCharacter;
					Controller2D controller = orangeCharacter.Controller;
					if (!orangeCharacter.CheckActStatusEvt(4, -1))
					{
						if (trigerData2.nParam1 == 1)
						{
							trigerData2.EndPos = controller.transform.position;
							trigerData2.nParam1 = 0;
						}
						else
						{
							zero2 = trigerData2.EndPos - controller.transform.position;
							controller.transform.position = trigerData2.EndPos;
							controller.LogicPosition = new VInt3(controller.LogicPosition.vec3 + zero2);
						}
					}
					else
					{
						trigerData2.nParam1 = 1;
					}
					zero2 = new Vector3(zero.x, zero.y, 0f);
					num2 = zero2.magnitude;
					vector3 = Quaternion.AngleAxis(OldTrigerDatas[0].EndPos.y, Vector3.back) * Vector3.right;
					vector = controller2D.transform.position;
					float num8 = vector3.y / vector3.x * vector.x + vector.y;
					RaycastHit2D raycastHit2D;
					while ((bool)(raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheckX(controller.Collider2D, Vector2.zero, zero2 / num2, num2, controller.collisionMask, listCollisionTrans)))
					{
						if (raycastHit2D.point.x * (vector3.y / vector3.x) + raycastHit2D.point.y - num8 > 0f)
						{
							list.Add(raycastHit2D.transform);
							listCollisionTrans.Add(raycastHit2D.transform);
						}
						else
						{
							zero2 = zero2 / num2 * raycastHit2D.distance;
						}
					}
					controller.transform.position = controller.transform.position + zero2;
					controller.LogicPosition = new VInt3(controller.LogicPosition.vec3 + zero2);
					if (!orangeCharacter.CheckActStatusEvt(4, -1))
					{
						trigerData2.EndPos = controller.transform.position;
					}
					if (!orangeCharacter.Controller.Collider2D.bounds.Intersects(EventB2D.bounds))
					{
						TrigerDatas.Remove(trigerData2);
						orangeCharacter.ConnectStandardCtrlCB();
						orangeCharacter.PlayerPressJumpCB = trigerData2.BackCB;
						bCheck = true;
					}
				}
			}
			if (((uint)nType & 4u) != 0)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
			}
			zero2 = new Vector3(zero.x, zero.y, 0f);
			NowPos += zero2;
			base.transform.position = NowPos;
			foreach (Transform item in list)
			{
				if (listCollisionTrans.Contains(item))
				{
					listCollisionTrans.Remove(item);
				}
			}
			fTotalTimeLeft += 0.016f;
			if (listMutliMove[0].fMoveTime > 0f && fTotalTimeLeft > listMutliMove[0].fMoveTime)
			{
				listMutliAni[0].nIntParam1 = 1;
			}
		}

		private void PeriodCall()
		{
			if (TrigerDatas.Count == 0)
			{
				bStartUpdate = false;
				return;
			}
			fTotalTimeLeft += 0.016f;
			if (fTotalTimeLeft >= fMoveTime)
			{
				fTotalTimeLeft = 0f;
				if (nType != 0)
				{
					EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
					stageEventCall.nID = nType;
					stageEventCall.tTransform = TrigerDatas[0].tTriggerTransform;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
				}
			}
		}

		private void SeqDestoryLoop()
		{
			if (nMoveStep == 0)
			{
				fTimeLeft -= 0.016f;
				if (!(fTimeLeft <= 0f))
				{
					return;
				}
				bool flag = false;
				int count = TrigerDatas.Count;
				for (int i = 0; i < count; i++)
				{
					if (TrigerDatas[i].nParam1 == 0)
					{
						StageSceneObjParam component = TrigerDatas[i].tTriggerTransform.GetComponent<StageSceneObjParam>();
						component.WoundedStageSceneObj();
						component.BrokenStageSceneObj(0.5f);
						TrigerDatas[i].nParam1 = 1;
						fTimeLeft = listMutliMove[0].fMoveTime;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					nMoveStep = 1;
					fTimeLeft = listMutliMove[0].fDT;
				}
			}
			else
			{
				if (nMoveStep != 1)
				{
					return;
				}
				fTimeLeft -= 0.016f;
				if (!(fTimeLeft <= 0f))
				{
					return;
				}
				bool flag2 = false;
				int count2 = TrigerDatas.Count;
				for (int j = 0; j < count2; j++)
				{
					if (TrigerDatas[j].nParam1 == 1)
					{
						TrigerDatas[j].tTriggerTransform.GetComponent<StageSceneObjParam>().RestoreSceneObj();
						TrigerDatas[j].nParam1 = 0;
						fTimeLeft = listMutliMove[0].fMoveTime;
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					nMoveStep = 0;
					if (bLoop)
					{
						fTimeLeftDelay = fDelayTime;
						return;
					}
					bStartUpdate = false;
					bCheck = true;
				}
			}
		}

		private void CountDownEvent()
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (!(stageUpdate != null) || stageUpdate.gbAddStageUseTime)
			{
				fTimeLeft -= 0.016f;
				fTotalTimeLeft -= 0.016f;
				if (fTimeLeft <= -5f)
				{
					BattleInfoUI.Instance.CloseShowCountDown();
					bStartUpdate = false;
				}
				else if (fTimeLeft <= 0f && fTimeLeft + 0.016f >= 0f)
				{
					BattleInfoUI.Instance.ShowCountDown(0f);
					EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
					stageEventCall.nID = nType;
					stageEventCall.tTransform = StageUpdate.GetHostPlayerTrans();
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
				}
				else if (fTimeLeft >= 0f)
				{
					BattleInfoUI.Instance.ShowCountDown(fTimeLeft);
				}
				if (StageUpdate.gbIsNetGame)
				{
					StageUpdate.SyncStageObj(sSyncID, 0, fTimeLeft.ToString("0.000") + "," + fTotalTimeLeft.ToString("0.000"));
				}
			}
		}

		private void ReCallEvent()
		{
			tmpBoundA = EventB2D.bounds;
			for (int num = TrigerDatas.Count - 1; num >= 0; num--)
			{
				OrangeCharacter orangeCharacter = (OrangeCharacter)TrigerDatas[num].tObj1;
				tmpBoundB = orangeCharacter.Controller.Collider2D.bounds;
				if (!StageResManager.CheckBoundsIntersectNoZEffect(ref tmpBoundA, ref tmpBoundB))
				{
					TrigerDatas.RemoveAt(num);
				}
			}
			if (TrigerDatas.Count == 0)
			{
				bCheck = true;
				bStartUpdate = false;
				return;
			}
			fTimeLeft -= 0.016f;
			if (fTimeLeft <= 0f)
			{
				fTimeLeft = fMoveTime;
				if (nType != 0)
				{
					EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
					stageEventCall.nID = nType;
					stageEventCall.tTransform = (TrigerDatas[0].tObj1 as OrangeCharacter).transform;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
				}
			}
		}

		private void SimulateFallObj()
		{
			fTotalTimeLeft += 0.016f;
			if (StageUpdate.gbIsNetGame && StageUpdate.bIsHost)
			{
				StageUpdate.SyncStageObj(sSyncID, 0, fTimeLeft.ToString("0.000") + "," + fTotalTimeLeft.ToString("0.000"));
			}
			if (((uint)nType & (true ? 1u : 0u)) != 0 && fTimeLeft > 0f)
			{
				fTimeLeft -= 0.016f;
				fMoveSpeed += 0.016f;
				if (fMoveSpeed > 0.03f)
				{
					fMoveSpeed = 0f;
					nMoveStep = 1 - nMoveStep;
				}
				if (fTimeLeft > 0f)
				{
					base.transform.localPosition = NowPos + new Vector3(-0.1f + (float)nMoveStep * 0.1f, 0f, 0f);
				}
				else
				{
					base.transform.localPosition = NowPos;
				}
				return;
			}
			MOECollisionUseData mOECollisionUseData = TrigerDatas[0].tObj2 as MOECollisionUseData;
			mOECollisionUseData.bNotFall = Mathf.Abs(vMoveVector.y) < 0.001f;
			mOECollisionUseData.bIsStop = Mathf.Abs(vMoveVector.x) < 0.001f && Mathf.Abs(vMoveVector.y) < 0.001f;
			for (int i = 0; i < mOECollisionUseData.bPlaySE.Length; i++)
			{
				mOECollisionUseData.bPlaySE[i] = false;
			}
			vMoveVector.y -= fMoveTime * 0.016f;
			mOECollisionUseData.dis.x = vMoveVector.x * 0.016f;
			mOECollisionUseData.dis.y = vMoveVector.y * 0.016f;
			mOECollisionUseData.fTestLen = 0f;
			mOECollisionUseData.fTmpLen = 0f;
			NowPos = base.transform.localPosition;
			mOECollisionUseData.tSelfHit = (mOECollisionUseData.tTmpHit = (mOECollisionUseData.tCheckHitX = default(RaycastHit2D)));
			mOECollisionUseData.vTmpMove = Vector3.zero;
			mOECollisionUseData.nextdis = mOECollisionUseData.dis;
			mOECollisionUseData.IsCheckEnd = false;
			while (mOECollisionUseData.nextdis.x != 0f || mOECollisionUseData.nextdis.y != 0f)
			{
				mOECollisionUseData.dis = mOECollisionUseData.nextdis;
				mOECollisionUseData.fTmpLen = (mOECollisionUseData.fTestLen = mOECollisionUseData.dis.magnitude);
				mOECollisionUseData.disnormal = mOECollisionUseData.dis / mOECollisionUseData.fTestLen;
				NowPos = base.transform.position;
				mOECollisionUseData.tSelfHit = default(RaycastHit2D);
				foreach (TrigerData trigerData in TrigerDatas)
				{
					mOECollisionUseData.tTmpHit = StageResManager.ObjMoveCollisionWithBoxCheckX(trigerData.tObj1 as BoxCollider2D, mOECollisionUseData.vTmpMove, mOECollisionUseData.disnormal, mOECollisionUseData.fTmpLen, LayerMask.GetMask("Block"), listCollisionTrans);
					if ((bool)mOECollisionUseData.tTmpHit)
					{
						if ((bool)(mOECollisionUseData.tStageHurtObj = mOECollisionUseData.tTmpHit.transform.GetComponent<StageHurtObj>()))
						{
							mOECollisionUseData.tStageHurtObj.BrkoenAll();
							PlaySE(listMutliAni[2].aniName, ref mOECollisionUseData.bPlaySE[2]);
						}
						else
						{
							mOECollisionUseData.fTmpLen = mOECollisionUseData.tTmpHit.distance;
							mOECollisionUseData.tSelfHit = mOECollisionUseData.tTmpHit;
						}
					}
				}
				if ((bool)mOECollisionUseData.tSelfHit)
				{
					if (mOECollisionUseData.bNotFall)
					{
						mOECollisionUseData.bNotFall = false;
					}
					mOECollisionUseData.bNeedAddTransList = true;
					mOECollisionUseData.fDirDotAns = Vector2.Dot(mOECollisionUseData.disnormal, mOECollisionUseData.tSelfHit.normal);
					mOECollisionUseData.IsCheckEnd = false;
					if (mOECollisionUseData.tSelfHit.normal == Vector2.zero)
					{
						mOECollisionUseData.vTmpMove.x += mOECollisionUseData.dis.x;
						mOECollisionUseData.vTmpMove.y += mOECollisionUseData.dis.y;
						mOECollisionUseData.nextdis = Vector2.zero;
						mOECollisionUseData.IsCheckEnd = true;
					}
					else if (mOECollisionUseData.fDirDotAns > 0.95f)
					{
						mOECollisionUseData.vTmpMove.x += mOECollisionUseData.dis.x;
						mOECollisionUseData.vTmpMove.y += mOECollisionUseData.dis.y;
						mOECollisionUseData.nextdis = Vector2.zero;
						mOECollisionUseData.IsCheckEnd = true;
					}
					else if (!(mOECollisionUseData.fDirDotAns < 0.05f) || !(mOECollisionUseData.fDirDotAns > -0.05f))
					{
						if (mOECollisionUseData.fDirDotAns < -0.9f)
						{
							mOECollisionUseData.fVBias = mOECollisionUseData.disnormal;
							mOECollisionUseData.fTmp1 = mOECollisionUseData.fVBias.x;
							mOECollisionUseData.bTmp1 = true;
							if (mOECollisionUseData.fTmp1 > 0f)
							{
								mOECollisionUseData.fVBias.x = 0f - mOECollisionUseData.fVBias.y;
								mOECollisionUseData.fVBias.y = mOECollisionUseData.fTmp1;
							}
							else if (mOECollisionUseData.fTmp1 < 0f)
							{
								mOECollisionUseData.fVBias.x = mOECollisionUseData.fVBias.y;
								mOECollisionUseData.fVBias.y = 0f - mOECollisionUseData.fTmp1;
							}
							else
							{
								mOECollisionUseData.bTmp1 = false;
							}
							if (mOECollisionUseData.bTmp1)
							{
								mOECollisionUseData.fVBias *= 0.05f;
								mOECollisionUseData.fTmpLen = mOECollisionUseData.fTestLen;
								mOECollisionUseData.tCheckHitX = default(RaycastHit2D);
								mOECollisionUseData.vTmpMove2 = mOECollisionUseData.vTmpMove;
								mOECollisionUseData.vTmpMove2.x += mOECollisionUseData.fVBias.x;
								mOECollisionUseData.vTmpMove2.y += mOECollisionUseData.fVBias.y;
								foreach (TrigerData trigerData2 in TrigerDatas)
								{
									mOECollisionUseData.tTmpHit = StageResManager.ObjMoveCollisionWithBoxCheckX(trigerData2.tObj1 as BoxCollider2D, mOECollisionUseData.vTmpMove2, mOECollisionUseData.disnormal, mOECollisionUseData.fTmpLen, LayerMask.GetMask("Block"), listCollisionTrans);
									if ((bool)mOECollisionUseData.tTmpHit)
									{
										mOECollisionUseData.fTmpLen = mOECollisionUseData.tTmpHit.distance;
										mOECollisionUseData.tCheckHitX = mOECollisionUseData.tTmpHit;
									}
								}
							}
							if (mOECollisionUseData.bTmp1 && !mOECollisionUseData.tCheckHitX)
							{
								mOECollisionUseData.vTmpMove.x += mOECollisionUseData.fVBias.x;
								mOECollisionUseData.vTmpMove.y += mOECollisionUseData.fVBias.y;
								mOECollisionUseData.dis = mOECollisionUseData.disnormal * mOECollisionUseData.tSelfHit.distance;
								mOECollisionUseData.nextdis -= mOECollisionUseData.dis;
								mOECollisionUseData.vTmpMove.x += mOECollisionUseData.dis.x;
								mOECollisionUseData.vTmpMove.y += mOECollisionUseData.dis.y;
								mOECollisionUseData.bNeedAddTransList = false;
								mOECollisionUseData.IsCheckEnd = true;
								if (!mOECollisionUseData.bIsStop)
								{
									PlaySE(listMutliAni[0].aniName, ref mOECollisionUseData.bPlaySE[0]);
								}
							}
							else if (!mOECollisionUseData.bIsStop)
							{
								PlaySE(listMutliAni[2].aniName, ref mOECollisionUseData.bPlaySE[2]);
							}
						}
						else if (!mOECollisionUseData.bIsStop)
						{
							PlaySE(listMutliAni[0].aniName, ref mOECollisionUseData.bPlaySE[0]);
						}
					}
					if (!mOECollisionUseData.IsCheckEnd)
					{
						mOECollisionUseData.dis = mOECollisionUseData.disnormal * mOECollisionUseData.tSelfHit.distance;
						mOECollisionUseData.nextdis -= mOECollisionUseData.dis;
						if (((uint)nType & 2u) != 0 && ((Vector2.Dot(Vector2.left, mOECollisionUseData.tSelfHit.normal) > 0.95f && mOECollisionUseData.dis.x > 0f) || (Vector2.Dot(Vector2.right, mOECollisionUseData.tSelfHit.normal) > 0.95f && mOECollisionUseData.dis.x < 0f)))
						{
							mOECollisionUseData.nextdis.x = 0f - mOECollisionUseData.nextdis.x;
							vMoveVector.x = 0f - vMoveVector.x;
						}
						else
						{
							mOECollisionUseData.vReflect = mOECollisionUseData.tSelfHit.normal * Vector2.Dot(mOECollisionUseData.nextdis, mOECollisionUseData.tSelfHit.normal);
							mOECollisionUseData.nextdis -= mOECollisionUseData.vReflect;
							mOECollisionUseData.vTmpMove.x += mOECollisionUseData.dis.x;
							mOECollisionUseData.vTmpMove.y += mOECollisionUseData.dis.y;
							mOECollisionUseData.dis.x = vMoveVector.x;
							mOECollisionUseData.dis.y = vMoveVector.y;
							mOECollisionUseData.vReflect = mOECollisionUseData.tSelfHit.normal * Vector2.Dot(mOECollisionUseData.dis, mOECollisionUseData.tSelfHit.normal);
							mOECollisionUseData.dis -= mOECollisionUseData.vReflect;
							vMoveVector.x = mOECollisionUseData.dis.x;
							vMoveVector.y = mOECollisionUseData.dis.y;
							if (!mOECollisionUseData.bIsStop && Mathf.Abs(vMoveVector.x) < 0.001f && Mathf.Abs(vMoveVector.y) < 0.001f)
							{
								mOECollisionUseData.bIsStop = true;
								PlaySE(listMutliAni[1].aniName, ref mOECollisionUseData.bPlaySE[1]);
							}
						}
					}
					if (!mOECollisionUseData.bNeedAddTransList)
					{
						continue;
					}
					mOECollisionUseData.tmpUse.Add(mOECollisionUseData.tSelfHit.transform);
					listCollisionTrans.Add(mOECollisionUseData.tSelfHit.transform);
					if (mOECollisionUseData.tmpUse.Count <= 0)
					{
						continue;
					}
					mOECollisionUseData.fTmpLen = (mOECollisionUseData.fTestLen = mOECollisionUseData.nextdis.magnitude);
					mOECollisionUseData.disnormal = mOECollisionUseData.nextdis / mOECollisionUseData.fTestLen;
					mOECollisionUseData.listNormal.Clear();
					foreach (TrigerData trigerData3 in TrigerDatas)
					{
						mOECollisionUseData.nHit = StageResManager.ObjMoveCollisionNumWithBoxCheckX(trigerData3.tObj1 as BoxCollider2D, mOECollisionUseData.vTmpMove, mOECollisionUseData.disnormal, mOECollisionUseData.fTmpLen, LayerMask.GetMask("Block"));
						if (mOECollisionUseData.nHit <= 0)
						{
							continue;
						}
						RaycastHit2D[] hitArray = StageResManager.GetHitArray();
						for (int j = 0; j < mOECollisionUseData.nHit; j++)
						{
							mOECollisionUseData.tTmpHit = hitArray[j];
							if (mOECollisionUseData.tTmpHit.distance == 0f && mOECollisionUseData.tmpUse.Contains(mOECollisionUseData.tTmpHit.transform))
							{
								mOECollisionUseData.listNormal.Add(mOECollisionUseData.tTmpHit.normal);
							}
						}
					}
					mOECollisionUseData.bHit = true;
					while (mOECollisionUseData.bHit)
					{
						mOECollisionUseData.bHit = false;
						foreach (Vector3 item in mOECollisionUseData.listNormal)
						{
							if (Vector2.Dot(mOECollisionUseData.nextdis, -item) > 0.001f)
							{
								mOECollisionUseData.vReflect = item * Vector2.Dot(mOECollisionUseData.nextdis, item);
								mOECollisionUseData.nextdis -= mOECollisionUseData.vReflect;
								mOECollisionUseData.bHit = true;
							}
							if (Vector2.Dot(vMoveVector, -item) > 0.001f)
							{
								mOECollisionUseData.dis.x = vMoveVector.x;
								mOECollisionUseData.dis.y = vMoveVector.y;
								mOECollisionUseData.vReflect = item * Vector2.Dot(mOECollisionUseData.dis, item);
								mOECollisionUseData.dis -= mOECollisionUseData.vReflect;
								vMoveVector.x = mOECollisionUseData.dis.x;
								vMoveVector.y = mOECollisionUseData.dis.y;
								mOECollisionUseData.bHit = true;
							}
						}
						if (Mathf.Abs(mOECollisionUseData.nextdis.x) < 0.001f)
						{
							mOECollisionUseData.nextdis.x = 0f;
						}
						if (Mathf.Abs(mOECollisionUseData.nextdis.y) < 0.001f)
						{
							mOECollisionUseData.nextdis.y = 0f;
						}
						if (Mathf.Abs(vMoveVector.x) < 0.001f)
						{
							vMoveVector.x = 0f;
						}
						if (Mathf.Abs(vMoveVector.y) < 0.001f)
						{
							vMoveVector.y = 0f;
						}
						if (mOECollisionUseData.nextdis.x == 0f && mOECollisionUseData.nextdis.y == 0f && vMoveVector.x == 0f && vMoveVector.y == 0f)
						{
							mOECollisionUseData.bHit = false;
						}
					}
				}
				else
				{
					mOECollisionUseData.vTmpMove.x += mOECollisionUseData.dis.x;
					mOECollisionUseData.vTmpMove.y += mOECollisionUseData.dis.y;
					mOECollisionUseData.nextdis = Vector2.zero;
				}
			}
			if (mOECollisionUseData.vTmpMove.x == 0f && mOECollisionUseData.vTmpMove.y == 0f)
			{
				bStartUpdate = false;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
				PlaySE(listMutliAni[1].aniName, ref mOECollisionUseData.bPlaySE[1]);
				return;
			}
			NowPos += mOECollisionUseData.vTmpMove;
			base.transform.position = NowPos;
			foreach (Transform item2 in mOECollisionUseData.tmpUse)
			{
				if (listCollisionTrans.Contains(item2))
				{
					listCollisionTrans.Remove(item2);
				}
			}
			mOECollisionUseData.tmpUse.Clear();
			TrigerDatas[0].vMax += mOECollisionUseData.vTmpMove;
			TrigerDatas[0].vMin += mOECollisionUseData.vTmpMove;
			CheckPlayerEnemyAndMove(mOECollisionUseData.vTmpMove, TrigerDatas[0].vMax, TrigerDatas[0].vMin, listCollisionTrans, (nType & 4) != 0, listMutliAni[0].nIntParam1);
			if (((uint)nType & 0x100u) != 0)
			{
				TrigerDatas[0].fTimeLeft += 0.016f;
				if (TrigerDatas[0].fTimeLeft >= 3f)
				{
					bStartUpdate = false;
				}
			}
			else
			{
				if ((nType & 8) == 0)
				{
					return;
				}
				Transform transform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
				bool flag = true;
				foreach (TrigerData trigerData4 in TrigerDatas)
				{
					BoxCollider2D boxCollider2D = trigerData4.tObj1 as BoxCollider2D;
					if (boxCollider2D.bounds.max.y >= transform.position.y - ManagedSingleton<StageHelper>.Instance.fCameraHHalf && boxCollider2D.bounds.min.y <= transform.position.y + ManagedSingleton<StageHelper>.Instance.fCameraHHalf && boxCollider2D.bounds.max.x >= transform.position.x - ManagedSingleton<StageHelper>.Instance.fCameraWHalf && boxCollider2D.bounds.min.x <= transform.position.x + ManagedSingleton<StageHelper>.Instance.fCameraWHalf)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					nType |= 256;
				}
			}
		}

		private void ShotItemBullet()
		{
			fTimeLeft -= 0.016f;
			if (!(fTimeLeft <= 0f))
			{
				return;
			}
			fTimeLeft = 0f;
			float num = 0f;
			for (int i = 0; i < listMutliMove.Count; i++)
			{
				if (listMutliAni[i].nIntParam2 == 0 || listMutliAni[i].nIntParam2 == StageUpdate.gDifficulty)
				{
					num += listMutliMove[i].fMoveTime;
				}
			}
			float num2 = UnityEngine.Random.Range(0f, num);
			num = 0f;
			nType = -1;
			for (int j = 0; j < listMutliMove.Count; j++)
			{
				if (listMutliAni[j].nIntParam2 == 0 || listMutliAni[j].nIntParam2 == StageUpdate.gDifficulty)
				{
					num += listMutliMove[j].fMoveTime;
					if (num >= num2)
					{
						nType = j;
						break;
					}
				}
			}
			if (nType == -1)
			{
				bCheck = false;
				bStartUpdate = false;
				return;
			}
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(sSyncID, 1, "0," + nType);
			}
			StageResManager.LoadStageItemModel(listMutliMove[nType].nIntParam1, null, base.transform.position, tMoveToObj);
			bCheck = false;
			bStartUpdate = false;
		}

		private void SwingHammer()
		{
			vMoveVector.x += fMoveTime * 0.016f;
			if (fMoveTime > 0f && vMoveVector.x > listMutliMove[0].tMovePos.x)
			{
				fMoveTime = 0f - fMoveTime;
			}
			else if (fMoveTime < 0f && vMoveVector.x < 0f - listMutliMove[0].tMovePos.x)
			{
				fMoveTime = 0f - fMoveTime;
			}
			base.transform.localRotation = Quaternion.Euler(vMoveVector);
			for (int num = TrigerDatas.Count - 1; num >= 0; num--)
			{
				if (TrigerDatas[num].EndPos.x > 0f)
				{
					TrigerDatas[num].EndPos.x -= 0.016f;
				}
				else if (TrigerDatas[num].EndPos.z >= 0f)
				{
					TrigerDatas[num].EndPos.z -= 0.016f;
					OrangeCharacter orangeCharacter = TrigerDatas[num].tObj1 as OrangeCharacter;
					if (orangeCharacter != null)
					{
						if (!orangeCharacter.IsDead())
						{
							orangeCharacter.AddShift(new VInt3(TrigerDatas[num].vMin));
						}
						if (orangeCharacter.CheckActStatus(0, -1) || orangeCharacter.CheckActStatus(3, -1))
						{
							TrigerDatas[num].EndPos.z = 0f;
						}
					}
					if (TrigerDatas[num].EndPos.z <= 0f)
					{
						TrigerDatas.RemoveAt(num);
						num--;
					}
				}
			}
		}

		private void CountUpEvent()
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate != null && !stageUpdate.gbAddStageUseTime)
			{
				return;
			}
			fTimeLeft += 0.016f;
			fTotalTimeLeft += 0.016f;
			BattleInfoUI.Instance.ShowCountDown(fTimeLeft);
			foreach (MutliAni item in listMutliAni)
			{
				if ((float)item.nIntParam1 < fTimeLeft && (float)item.nIntParam1 > fTimeLeft - 0.016f)
				{
					EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
					stageEventCall.nID = item.nIntParam2;
					stageEventCall.tTransform = StageUpdate.GetHostPlayerTrans();
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
				}
			}
			if (StageUpdate.gbIsNetGame)
			{
				StageUpdate.SyncStageObj(sSyncID, 0, fTimeLeft.ToString("0.000") + "," + fTotalTimeLeft.ToString("0.000"));
			}
		}

		private void TransparentByObjIn2()
		{
			if (TrigerDatas.Count == 0)
			{
				return;
			}
			List<MeshRenderer> list = TrigerDatas[0].tObj1 as List<MeshRenderer>;
			List<int> list2 = TrigerDatas[0].tObj2 as List<int>;
			for (int i = 0; i < list.Count; i++)
			{
				MeshRenderer meshRenderer = list[i];
				if (meshRenderer.material.shader.name != "StageLib/DiffuseAlpha")
				{
					Material material = new Material(Shader.Find("StageLib/DiffuseAlpha"));
					material.mainTexture = meshRenderer.material.mainTexture;
					meshRenderer.material = material;
				}
				Color color = meshRenderer.material.GetColor("_Color");
				list2[i * 2]--;
				if ((float)list2[i * 2] <= fMoveTime * 100f)
				{
					list2[i * 2] = (int)(fMoveTime * 100f);
					if (((uint)nType & (true ? 1u : 0u)) != 0)
					{
						Collider2D[] componentsInChildren = meshRenderer.GetComponentsInChildren<Collider2D>();
						for (int j = 0; j < componentsInChildren.Length; j++)
						{
							componentsInChildren[j].enabled = false;
						}
					}
				}
				color.a = (float)list2[i * 2] * 0.01f;
				meshRenderer.material.SetColor("_Color", color);
			}
			if (bCheckPlayer)
			{
				Vector3 max = EventB2D.bounds.max;
				Vector3 min = EventB2D.bounds.min;
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					Vector3 position = StageUpdate.runPlayers[num].transform.position;
					if (max.x > position.x && max.y > position.y && min.x < position.x && min.y < position.y)
					{
						return;
					}
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				MeshRenderer meshRenderer2 = list[k];
				Color color = meshRenderer2.material.GetColor("_Color");
				color.a = (float)list2[k * 2 + 1] * 0.01f;
				meshRenderer2.material.SetColor("_Color", color);
				if (((uint)nType & (true ? 1u : 0u)) != 0)
				{
					Collider2D[] componentsInChildren = meshRenderer2.GetComponentsInChildren<Collider2D>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].enabled = true;
					}
				}
			}
			bCheck = true;
			bStartUpdate = false;
		}

		public void AddCountDownEventTime(float fAddTime)
		{
			if (mapEvent == MapEventEnum.COUNTDOWN_EVENT && bStartUpdate && fTimeLeft > 0f)
			{
				fTimeLeft += fAddTime;
			}
		}

		private void AddForceRange()
		{
		}

		private void DisapperLoop()
		{
			switch (ts_State)
			{
			case TIMELINE_STATE.TS_INIT:
			case TIMELINE_STATE.TS_START:
				fTimeLeft = fMoveTime;
				if (((uint)nBitParam0 & (true ? 1u : 0u)) != 0)
				{
					tTmpColor.a = 0f;
					ts_State = TIMELINE_STATE.TS_MOVE01;
					if (((uint)nBitParam0 & 4u) != 0)
					{
						PlaySE(DisapperIn[0], DisapperIn[1]);
					}
				}
				else
				{
					tTmpColor = Color.white;
					ts_State = TIMELINE_STATE.TS_MOVE05;
					if (((uint)nBitParam0 & 4u) != 0)
					{
						PlaySE(DisapperOut[0], DisapperOut[1]);
					}
				}
				break;
			case TIMELINE_STATE.TS_MOVE01:
				fTimeLeft -= 0.016f;
				tTmpColor.a = (fMoveTime - fTimeLeft) / fMoveTime;
				if (fTimeLeft <= 0f)
				{
					fTimeLeft = listMutliMove[0].fMoveTime;
					ts_State = TIMELINE_STATE.TS_MOVE02;
				}
				break;
			case TIMELINE_STATE.TS_MOVE02:
				fTimeLeft -= 0.016f;
				if (fTimeLeft <= 0f)
				{
					fTimeLeft = fMoveTime;
					ts_State = TIMELINE_STATE.TS_MOVE03;
					if (((uint)nBitParam0 & 4u) != 0)
					{
						PlaySE(DisapperOut[0], DisapperOut[1]);
					}
				}
				break;
			case TIMELINE_STATE.TS_MOVE03:
				fTimeLeft -= 0.016f;
				tTmpColor.a = fTimeLeft / fMoveTime;
				if (fTimeLeft <= 0f)
				{
					ts_State = TIMELINE_STATE.TS_END;
				}
				break;
			case TIMELINE_STATE.TS_MOVE05:
				fTimeLeft -= 0.016f;
				tTmpColor.a = fTimeLeft / fMoveTime;
				if (fTimeLeft <= 0f)
				{
					tTmpColor.a = 0f;
					fTimeLeft = listMutliMove[0].fMoveTime;
					ts_State = TIMELINE_STATE.TS_MOVE06;
				}
				break;
			case TIMELINE_STATE.TS_MOVE06:
				fTimeLeft -= 0.016f;
				if (fTimeLeft <= 0f)
				{
					fTimeLeft = fMoveTime;
					ts_State = TIMELINE_STATE.TS_MOVE07;
					if (((uint)nBitParam0 & 4u) != 0)
					{
						PlaySE(DisapperIn[0], DisapperIn[1]);
					}
				}
				break;
			case TIMELINE_STATE.TS_MOVE07:
				fTimeLeft -= 0.016f;
				tTmpColor.a = (fMoveTime - fTimeLeft) / fMoveTime;
				if (fTimeLeft <= 0f)
				{
					ts_State = TIMELINE_STATE.TS_END;
				}
				break;
			case TIMELINE_STATE.TS_END:
				ts_State = TIMELINE_STATE.TS_START;
				if (((uint)nBitParam0 & 2u) != 0)
				{
					bCheck = true;
					bStartUpdate = false;
				}
				else if (bLoop)
				{
					fTimeLeftDelay = fDelayTime + listMutliMove[0].tMovePos.x;
				}
				else
				{
					bStartUpdate = false;
				}
				break;
			}
			if (tTmpColor.a < 0f)
			{
				tTmpColor.a = 0f;
			}
			if (tTmpColor.a > 1f)
			{
				tTmpColor.a = 1f;
			}
			switch (ts_State)
			{
			case TIMELINE_STATE.TS_MOVE01:
			case TIMELINE_STATE.TS_MOVE03:
			case TIMELINE_STATE.TS_MOVE05:
			case TIMELINE_STATE.TS_MOVE07:
				SetTrigersData(true);
				break;
			case TIMELINE_STATE.TS_START:
			case TIMELINE_STATE.TS_MOVE02:
			case TIMELINE_STATE.TS_MOVE06:
			case TIMELINE_STATE.TS_END:
				if (tTmpColor.a < 0.1f)
				{
					tTmpColor.a = 0f;
					SetTrigersData(false);
				}
				else if (tTmpColor.a > 0.9f)
				{
					tTmpColor.a = 1f;
					SetTrigersData(true);
				}
				break;
			case TIMELINE_STATE.TS_MOVE04:
			case TIMELINE_STATE.TS_MOVE08:
			case TIMELINE_STATE.TS_MOVE09:
			case TIMELINE_STATE.TS_MOVE10:
				break;
			}
		}

		private void SetTrigersData(bool b2d)
		{
			foreach (TrigerData trigerData in TrigerDatas)
			{
				((StageSceneObjParam)trigerData.tObj1).SetSceneObjAlpha(tTmpColor);
				((StageSceneObjParam)trigerData.tObj1).SwitchB2DInStageSceneObj(b2d);
			}
		}

		private void TransparentByObjIn()
		{
			for (int i = 0; i < listMutliAni.Count; i++)
			{
				Transform transform = base.transform.Find(listMutliAni[i].aniName);
				if (!(transform != null))
				{
					continue;
				}
				MeshRenderer component = transform.GetComponent<MeshRenderer>();
				if (component.material.shader.name != "StageLib/DiffuseAlpha")
				{
					Material material = new Material(Shader.Find("StageLib/DiffuseAlpha"));
					material.mainTexture = component.material.mainTexture;
					component.material = material;
				}
				Color color = component.material.GetColor("_Color");
				listMutliAni[i].nIntParam1--;
				if ((float)listMutliAni[i].nIntParam1 <= fMoveTime * 100f)
				{
					listMutliAni[i].nIntParam1 = (int)(fMoveTime * 100f);
					if (((uint)nType & (true ? 1u : 0u)) != 0)
					{
						Collider2D[] componentsInChildren = transform.GetComponentsInChildren<Collider2D>();
						for (int j = 0; j < componentsInChildren.Length; j++)
						{
							componentsInChildren[j].enabled = false;
						}
					}
				}
				color.a = (float)listMutliAni[i].nIntParam1 * 0.01f;
				component.material.SetColor("_Color", color);
			}
			if (bCheckPlayer)
			{
				Vector3 max = EventB2D.bounds.max;
				Vector3 min = EventB2D.bounds.min;
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					Vector3 position = StageUpdate.runPlayers[num].transform.position;
					if (max.x > position.x && max.y > position.y && min.x < position.x && min.y < position.y)
					{
						return;
					}
				}
			}
			for (int k = 0; k < listMutliAni.Count; k++)
			{
				Transform transform2 = base.transform.Find(listMutliAni[k].aniName);
				if (!(transform2 != null))
				{
					continue;
				}
				MeshRenderer component2 = transform2.GetComponent<MeshRenderer>();
				Color color = component2.material.GetColor("_Color");
				color.a = (float)listMutliAni[k].nIntParam2 * 0.01f;
				component2.material.SetColor("_Color", color);
				if (((uint)nType & (true ? 1u : 0u)) != 0)
				{
					Collider2D[] componentsInChildren = transform2.GetComponentsInChildren<Collider2D>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].enabled = true;
					}
				}
			}
			bCheck = true;
			bStartUpdate = false;
		}

		public override void LockAnimator(bool bLock)
		{
			Animator[] componentsInChildren = base.transform.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = !bLock;
			}
		}

		public override int GetTypeID()
		{
			return 10;
		}

		public override string GetTypeString()
		{
			return StageObjType.MAPEVENT_OBJ.ToString();
		}

		public override bool IsCanAddChild()
		{
			return true;
		}

		public override bool IsMapDependObj()
		{
			switch (mapEvent)
			{
			case MapEventEnum.MOVETO:
			case MapEventEnum.PLAYEFFECT:
			case MapEventEnum.MOVETOBYTOUCH:
			case MapEventEnum.DISAPPERLOOP:
			case MapEventEnum.TRANSPARENTBYIN:
			case MapEventEnum.MOVECURVE:
			case MapEventEnum.MOVEREPEATE:
			case MapEventEnum.CONTINUEMOVE:
			case MapEventEnum.START_FALL:
			case MapEventEnum.TRIGGER_SKILL:
			case MapEventEnum.SWITCH_MEMOBJ:
			case MapEventEnum.PLAY_FLAG_MODE:
			case MapEventEnum.RIDE_OBJ:
			case MapEventEnum.PATH_OBJ_MOVE:
			case MapEventEnum.CIRCLE_OBJ_MOVE:
			case MapEventEnum.ANGLE_OBJ_MOVE:
			case MapEventEnum.ROTATE_TARGETOBJ:
				return true;
			default:
				return false;
			}
		}

		public override string GetSaveString()
		{
			return GetTypeString();
		}

		public override void LoadByString(string sLoad)
		{
			string text = sLoad.Substring(GetTypeString().Length);
			text = text.Replace(";" + text[0], ",");
			text = text.Substring(1);
			LoadMapObjData = JsonUtility.FromJson<MapObjData>(text);
			mapEvent = (MapEventEnum)LoadMapObjData.mapEvent;
			EventB2D = GetComponent<BoxCollider2D>();
			EventB2D.offset = new Vector2(LoadMapObjData.B2DX, LoadMapObjData.B2DY);
			EventB2D.size = new Vector2(LoadMapObjData.B2DW, LoadMapObjData.B2DH);
			bCheckPlayer = LoadMapObjData.bCheckPlayer;
			bCheckEnemy = LoadMapObjData.bCheckEnemy;
			bCheckRideObj = LoadMapObjData.bCheckRideObj;
			bUseBoxCollider2D = LoadMapObjData.bUB2D;
			bRunAtInit = LoadMapObjData.bRunAtInit;
			nCommonBit = LoadMapObjData.nCommonBit;
			if (mapEvent == MapEventEnum.PLAYBGM)
			{
				bmgs = LoadMapObjData.bmgs;
				bmge = LoadMapObjData.bmge;
			}
			else
			{
				if (LoadMapObjData.bmgs != "")
				{
					MutliSubSave mutliSubSave = JsonUtility.FromJson<MutliSubSave>(LoadMapObjData.bmgs);
					listMutliMove = mutliSubSave.listMutliMove;
					listMutliAni = mutliSubSave.listMutliAni;
					nBitParam0 = mutliSubSave.nBitParam0;
				}
				bmgs = "";
				bmge = LoadMapObjData.bmge;
			}
			tMoveToObj = LoadMapObjData.MoveToPos + base.transform.position;
			fDelayTime = LoadMapObjData.fDelayTime;
			fMoveTime = LoadMapObjData.fMoveTime;
			bLoop = LoadMapObjData.bLoop;
			nType = LoadMapObjData.nType;
			nSetID = LoadMapObjData.nSetID;
			mspd = LoadMapObjData.mspd;
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			StageUpdate stageUpdate = null;
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].name == "StageUpdate")
				{
					stageUpdate = rootGameObjects[i].GetComponent<StageUpdate>();
					break;
				}
			}
			if (mapEvent == MapEventEnum.TRIGGER_SKILL)
			{
				StageResManager.LoadBullet(nType);
				if (listMutliAni.Count == 0)
				{
					listMutliAni.Add(new MutliAni());
				}
				if (listMutliAni != null && listMutliAni.Count == 1 && (listMutliAni[0].nIntParam1 & 1) == 1)
				{
					string[] array = listMutliAni[0].aniName.Split(',');
					if (array != null && array.Length >= 2)
					{
						base.SoundSource.ActivePlaySE(array[0], array[1]);
					}
				}
				bStartUpdate = true;
			}
			int count = LoadMapObjData.Datas.Count;
			tmplistNeedUnMove = new List<GameObject>();
			tMixBojSyncData = new StageUpdate.MixBojSyncData();
			if (stageUpdate != null)
			{
				stageUpdate.SetSyncStageFunc(sSyncID, tMixBojSyncData.OnSyncStageFunc);
				stageUpdate.AddMixBojSyncData(tMixBojSyncData);
				tMixBojSyncData.SetSyncMixStageFunc(sSyncID + "-0", OnSyncStageObj);
			}
			for (int j = 0; j < count; j++)
			{
				bool flag = (bool)stageUpdate;
				if (LoadMapObjData.Datas[j].bunldepath == "")
				{
					Debug.LogError("prefab has no bundle path:" + LoadMapObjData.Datas[j].path);
					continue;
				}
				int num = LoadMapObjData.Datas[j].path.LastIndexOf("/");
				string text2 = LoadMapObjData.Datas[j].path.Substring(num + 1);
				text2 = text2.Substring(0, text2.Length - 7);
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.loadStageObjData = LoadMapObjData.Datas[j];
				loadCallBackObj.i = j;
				loadCallBackObj.lcb = StageLoadEndCall;
				loadCallBackObj.objParam0 = sSyncID + "-" + (j + 1);
				stageUpdate.AddSubLoadAB(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(LoadMapObjData.Datas[j].bunldepath, text2, loadCallBackObj.LoadCB);
			}
			sSyncID += "-0";
			if (mapEvent == MapEventEnum.MOVETO)
			{
				while (listMutliAni.Count < 4)
				{
					listMutliAni.Add(new MutliAni());
				}
				if (((uint)nType & 2u) != 0)
				{
					for (int num2 = listMutliMove.Count - 2; num2 >= 0; num2--)
					{
						listMutliMove.Add(listMutliMove[num2]);
					}
					MutliMove mutliMove = new MutliMove();
					mutliMove.tMovePos = tMoveToObj ?? Vector3.zero;
					mutliMove.fDT = fDelayTime;
					mutliMove.fMoveTime = fMoveTime;
					if (listMutliAni.Count > 0)
					{
						mutliMove.nIntParam1 = listMutliAni[0].nIntParam1;
					}
					listMutliMove.Add(mutliMove);
					if (!bLoop)
					{
						mutliMove = new MutliMove();
						mutliMove.tMovePos = base.transform.position;
						mutliMove.fDT = fDelayTime;
						mutliMove.fMoveTime = fMoveTime;
						listMutliMove.Add(mutliMove);
					}
				}
				if ((nType & 1) == 0)
				{
					listMutliMove.Clear();
				}
				if (bLoop)
				{
					MutliMove mutliMove2 = new MutliMove();
					mutliMove2.tMovePos = base.transform.position;
					mutliMove2.fDT = fDelayTime;
					if (((uint)nType & 0x400u) != 0)
					{
						mutliMove2.fMoveTime = (float)listMutliAni[1].nIntParam2 * 0.001f;
					}
					else
					{
						mutliMove2.fMoveTime = fMoveTime;
					}
					mutliMove2.nIntParam1 = listMutliAni[0].nIntParam1;
					listMutliMove.Add(mutliMove2);
				}
				if ((nType & 0x200) == 0 || MoveObjStart != null)
				{
					return;
				}
				if (!string.IsNullOrEmpty(listMutliAni[2].aniName))
				{
					DisapperIn = listMutliAni[2].aniName.Split(',');
					MoveObjStart = delegate
					{
						base.SoundSource.PlaySE(DisapperIn[0], DisapperIn[1]);
					};
				}
				if (!string.IsNullOrEmpty(listMutliAni[3].aniName))
				{
					DisapperOut = listMutliAni[3].aniName.Split(',');
					MoveObjEnd = delegate
					{
						base.SoundSource.PlaySE(DisapperOut[0], DisapperOut[1]);
					};
				}
			}
			else if (mapEvent == MapEventEnum.STAGE_ITEM || mapEvent == MapEventEnum.SHOT_ITEM_BULLET)
			{
				bCheck = false;
				while (listMutliMove.Count > listMutliAni.Count)
				{
					listMutliAni.Add(new MutliAni());
				}
				for (int k = 0; k < listMutliMove.Count; k++)
				{
					StageResManager.LoadStageItemModel(listMutliMove[k].nIntParam1);
				}
				if (nSetID == 0 && stageUpdate != null)
				{
					stageUpdate.StartCoroutine(StageItemCoroutine());
				}
			}
			else if (mapEvent == MapEventEnum.PLAY_FLAG_MODE)
			{
				bRunAtInit = true;
				ArrayFloats = new float[2];
				ArrayUInts = new uint[8];
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/ObjCountDownBar", "ObjCountDownBar", delegate(GameObject obj)
				{
					if (!(obj == null))
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(obj, base.transform);
						gameObject.name = "ObjCountDownBar";
						tCountDownBar = gameObject.GetComponent<CountDownBar>();
						tCountDownBar.gameObject.SetActive(false);
					}
				});
			}
			else if (mapEvent == MapEventEnum.FLAG_SETTING)
			{
				bRunAtInit = true;
			}
			else if (mapEvent == MapEventEnum.PLAYEFFECT)
			{
				if (listMutliAni.Count > 0)
				{
					StageResManager.LoadFx(listMutliAni[0].aniName);
				}
			}
			else if (mapEvent == MapEventEnum.CONTINUEMOVE)
			{
				while (listMutliMove.Count < 1)
				{
					listMutliMove.Add(new MutliMove());
				}
			}
			else if (mapEvent == MapEventEnum.SHOW_RANKFX)
			{
				for (int l = 0; l < 4; l++)
				{
					StageResManager.LoadFx(listMutliAni[l].aniName);
				}
			}
			else if (mapEvent == MapEventEnum.STAGEEND)
			{
				StageResManager.GetStageUpdate().bIsHaveEventStageEnd = true;
			}
			else if (mapEvent == MapEventEnum.SIMULATE_FALLOBJ)
			{
				while (listMutliAni.Count < 3)
				{
					listMutliAni.Add(new MutliAni());
				}
			}
			else if (mapEvent == MapEventEnum.SWITCH_MEMOBJ)
			{
				while (listMutliAni.Count < 1)
				{
					listMutliAni.Add(new MutliAni());
				}
			}
			else if (bRunAtInit && mapEvent == MapEventEnum.PLAYBGM)
			{
				bCheck = false;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYBGM, nType, bmgs, bmge);
				bRunAtInit = false;
			}
		}

		private void StageLoadEndCall(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(asset) as GameObject;
			StageObjData stageObjData = (StageObjData)tObj.loadStageObjData;
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = stageObjData.position;
			gameObject.transform.localScale = stageObjData.scale;
			gameObject.transform.localRotation = stageObjData.rotate;
			gameObject.name = stageObjData.name;
			int objtype = StageUpdate.LoadProperty(null, gameObject, stageObjData.property, null, tObj.objParam0 as string, tMixBojSyncData);
			InitNewObj(objtype, gameObject);
		}

		private void InitNewObj(int objtype, GameObject newgobj)
		{
			if (objtype == 0 || objtype == 6)
			{
				tmplistNeedUnMove.Add(newgobj);
				BoxCollider2D[] componentsInChildren = newgobj.GetComponentsInChildren<BoxCollider2D>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (!listCollisionTrans.Contains(componentsInChildren[i].transform))
					{
						listCollisionTrans.Add(componentsInChildren[i].transform);
					}
				}
			}
			if (mapEvent == MapEventEnum.STAGE_ITEM)
			{
				newgobj.SetActive(false);
			}
			else if (mapEvent == MapEventEnum.SWITCH_MEMOBJ)
			{
				if ((nType & 1) == 0)
				{
					newgobj.SetActive(false);
				}
			}
			else if (mapEvent == MapEventEnum.PLAY_FLAG_MODE)
			{
				for (int j = 0; j < listMutliAni.Count; j++)
				{
					if (listMutliAni[j].aniName == newgobj.name && listMutliAni[j].nIntParam1 != nType)
					{
						newgobj.SetActive(false);
					}
				}
			}
			else if (mapEvent == MapEventEnum.RIDE_OBJ)
			{
				Animation[] componentsInChildren2 = newgobj.GetComponentsInChildren<Animation>();
				for (int k = 0; k < componentsInChildren2.Length; k++)
				{
					componentsInChildren2[k].Stop();
				}
				ParticleSystem[] componentsInChildren3 = newgobj.GetComponentsInChildren<ParticleSystem>();
				for (int k = 0; k < componentsInChildren3.Length; k++)
				{
					componentsInChildren3[k].Pause();
				}
			}
			else if (mapEvent == MapEventEnum.TRANSPARENTBYIN)
			{
				for (int l = 0; l < listMutliAni.Count; l++)
				{
					Transform transform = base.transform.Find(listMutliAni[l].aniName);
					if (transform != null)
					{
						MeshRenderer component = transform.GetComponent<MeshRenderer>();
						if (component.material.shader.name != "StageLib/DiffuseAlpha")
						{
							Material material = new Material(Shader.Find("StageLib/DiffuseAlpha"));
							material.mainTexture = component.material.mainTexture;
							component.material = material;
						}
						tTmpColor = component.material.GetColor("_Color");
						listMutliAni[l].nIntParam1 = (int)(tTmpColor.a * 100f);
						component.material.SetColor("_Color", tTmpColor);
					}
				}
			}
			else
			{
				if (mapEvent != MapEventEnum.DISAPPERLOOP)
				{
					return;
				}
				StageSceneObjParam[] componentsInChildren4 = newgobj.GetComponentsInChildren<StageSceneObjParam>();
				tTmpColor = Color.white;
				tTmpColor.a = 0f;
				StageSceneObjParam[] array = componentsInChildren4;
				foreach (StageSceneObjParam stageSceneObjParam in array)
				{
					stageSceneObjParam.CheckAlphaMaterial();
					if (((uint)nBitParam0 & (true ? 1u : 0u)) != 0)
					{
						stageSceneObjParam.SetSceneObjAlpha(tTmpColor);
						stageSceneObjParam.SwitchB2DInStageSceneObj(false);
					}
				}
			}
		}

		private void TouchCallBack(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
		{
		}

		public override bool IsNeedClip()
		{
			MapEventEnum mapEventEnum = mapEvent;
			if (mapEventEnum == MapEventEnum.START_FALL)
			{
				return true;
			}
			return false;
		}

		public override bool IsNeedCheckClipAlone()
		{
			return true;
		}

		public override void SyncNowStatus()
		{
			base.SyncNowStatus();
			string text = "";
			List<FallingFloor> list = new List<FallingFloor>();
			switch (mapEvent)
			{
			case MapEventEnum.MOVETO:
			case MapEventEnum.MOVEREPEATE:
			{
				text = nMoveStep.ToString();
				text = text + "," + fMoveSpeed.ToString("0.00000");
				text = text + "," + GetVector3SaveStr(vMoveVector);
				text = text + "," + fTimeLeft.ToString("0.00000");
				text = text + "," + fTotalTimeLeft.ToString("0.00000");
				text = text + "," + GetVector3SaveStr(NowPos);
				text = text + "," + GetVector3SaveStr(StartPos);
				text = text + "," + GetVector3SaveStr(EndPos);
				text = text + "," + nCallCount;
				text = text + "," + GetBoolSaveStr(bCheck);
				text = text + "," + GetBoolSaveStr(bStartUpdate);
				text = text + "," + GetVector3SaveStr(base.transform.position);
				string text2 = text;
				int num = (int)ts_State;
				text = text2 + "," + num;
				break;
			}
			case MapEventEnum.RIDE_OBJ:
				text = GetVector3SaveStr(NowPos);
				text = text + "," + fMoveSpeed.ToString("0.00000") + "," + vMoveVector.y.ToString("0.00000");
				text = text + "," + GetBoolSaveStr(bCheck);
				text = text + "," + GetBoolSaveStr(bStartUpdate);
				text = text + "," + GetVector3SaveStr(base.transform.position);
				if (TrigerDatas.Count != 0)
				{
					text = text + "," + TrigerDatas.Count;
					foreach (TrigerData trigerData in TrigerDatas)
					{
						TrigerData trigerDatum = trigerData;
						OrangeCharacter orangeCharacter = TrigerDatas[0].tObj1 as OrangeCharacter;
						text = text + "," + orangeCharacter.sPlayerID;
					}
				}
				else
				{
					text += ",0";
				}
				break;
			case MapEventEnum.CIRCLE_OBJ_MOVE:
				text = vMoveVector.x.ToString("0.00000");
				foreach (TrigerData oldTrigerData in OldTrigerDatas)
				{
					text = text + "," + oldTrigerData.fTimeLeft.ToString("0.00000");
				}
				text = text + "," + fTimeLeft.ToString("0.00000");
				text = text + "," + GetBoolSaveStr(bCheck);
				text = text + "," + GetBoolSaveStr(bStartUpdate);
				break;
			case MapEventEnum.ANGLE_OBJ_MOVE:
				text = nType.ToString();
				text = text + "," + fMoveSpeed.ToString("0.00000");
				text = text + "," + vMoveVector.x.ToString("0.00000");
				foreach (TrigerData oldTrigerData2 in OldTrigerDatas)
				{
					text = text + "," + oldTrigerData2.fTimeLeft.ToString("0.00000");
				}
				text = text + "," + GetBoolSaveStr(bCheck);
				text = text + "," + GetBoolSaveStr(bStartUpdate);
				break;
			case MapEventEnum.PLAY_FLAG_MODE:
				text = nType.ToString();
				text = text + "," + ArrayFloats[0];
				text = text + "," + ArrayFloats[1];
				text = text + "," + fTimeLeft.ToString("0.00000");
				text = text + "," + OldTrigerDatas.Count;
				foreach (TrigerData oldTrigerData3 in OldTrigerDatas)
				{
					OrangeCharacter component = oldTrigerData3.tTriggerTransform.GetComponent<OrangeCharacter>();
					text = ((!component) ? (text + ",") : (text + "," + component.sPlayerID));
				}
				text = text + "," + GetBoolSaveStr(bCheck);
				text = text + "," + GetBoolSaveStr(bStartUpdate);
				break;
			case MapEventEnum.STAGE_ITEM:
				if (bCheck)
				{
					StageUpdate.SyncStageObj(sSyncID, 1, "0," + nType);
				}
				break;
			case MapEventEnum.START_FALL:
			{
				text += GetBoolSaveStr(bCheck);
				text = text + "," + GetBoolSaveStr(bStartUpdate);
				text = text + "," + GetVector3SaveStr(base.transform.position);
				for (int i = 0; i < base.transform.childCount; i++)
				{
					FallingFloor[] componentsInChildren = base.transform.GetChild(i).GetComponentsInChildren<FallingFloor>();
					if (componentsInChildren != null)
					{
						list.AddRange(componentsInChildren);
					}
				}
				text = text + "," + list.Count;
				for (int j = 0; j < list.Count; j++)
				{
					text = text + "," + list[j].gameObject.name;
					text = text + "," + GetVector3SaveStr(list[j].transform.position);
				}
				break;
			}
			case MapEventEnum.SIMULATE_FALLOBJ:
				text = GetBoolSaveStr(bCheck);
				text = text + "," + GetBoolSaveStr(bStartUpdate);
				text = text + "," + fTimeLeft.ToString("0.00000");
				text = text + "," + fTotalTimeLeft.ToString("0.00000");
				text = text + "," + GetVector3SaveStr(vMoveVector);
				text = text + "," + GetVector3SaveStr(base.transform.position);
				text = text + "," + nType;
				break;
			}
			if (text != "")
			{
				StageUpdate.SyncStageObj(sSyncID, 2, text);
			}
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
			if (!StageUpdate.gbStageReady)
			{
				return;
			}
			base.OnSyncStageObj(sIDKey, nKey1, smsg);
			if (mapEvent == MapEventEnum.STAGEEND)
			{
				if (bCheck)
				{
					if (((uint)nType & (true ? 1u : 0u)) != 0)
					{
						string[] array = listMutliAni[0].aniName.Split(',');
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_PLAYBGM, listMutliAni[0].nIntParam1, array[0], array[1]);
					}
					bCheck = false;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
				}
				return;
			}
			switch (nKey1)
			{
			case 0:
			{
				if (((uint)nCommonBit & (true ? 1u : 0u)) != 0)
				{
					break;
				}
				CheckNeedUnMove();
				string[] array7 = smsg.Split(',');
				float num16 = float.Parse(array7[0]);
				if (!bStartUpdate && bCheck)
				{
					break;
				}
				switch (mapEvent)
				{
				case MapEventEnum.MOVETO:
				{
					float num17 = float.Parse(array7[1]);
					num16 -= MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime;
					bool flag = (nType & 0x10) != 0;
					nType &= -17;
					while (num17 > fTotalTimeLeft && bStartUpdate)
					{
						OnLateUpdate();
					}
					if (flag)
					{
						nType |= 16;
					}
					break;
				}
				case MapEventEnum.MOVEREPEATE:
				{
					float num17 = float.Parse(array7[1]);
					num16 -= MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime;
					while (num17 > fTotalTimeLeft && bStartUpdate)
					{
						MoveMapRepeate();
					}
					break;
				}
				case MapEventEnum.PATH_OBJ_MOVE:
					num16 += MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime;
					while (fTimeLeft + 0.016f < num16)
					{
						PathObjMove();
					}
					break;
				case MapEventEnum.CIRCLE_OBJ_MOVE:
					num16 += MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime;
					while (fTimeLeft > num16 + 0.016f)
					{
						CircleObjMove(-0.016f);
					}
					while (fTimeLeft + 0.016f < num16)
					{
						CircleObjMove(0.016f);
					}
					break;
				case MapEventEnum.COUNTDOWN_EVENT:
					fTimeLeft = num16;
					fTotalTimeLeft = float.Parse(array7[1]);
					fTimeLeft += 0.016f;
					fTotalTimeLeft += 0.016f;
					CountDownEvent();
					break;
				case MapEventEnum.SIMULATE_FALLOBJ:
				{
					float num17 = float.Parse(array7[1]);
					while (num17 > fTotalTimeLeft && bStartUpdate)
					{
						SimulateFallObj();
					}
					break;
				}
				case MapEventEnum.PLAY_FLAG_MODE:
					fTimeLeft = num16;
					ArrayFloats[0] = float.Parse(array7[1]);
					ArrayFloats[1] = float.Parse(array7[2]);
					break;
				}
				break;
			}
			case 1:
				if (mapEvent == MapEventEnum.PLAY_FLAG_MODE)
				{
					string[] array3 = smsg.Split(',');
					int num8 = int.Parse(array3[1]);
					if (array3[0] == 1.ToString())
					{
						if (num8 == nType)
						{
							break;
						}
						nType = num8;
						int childCount2 = base.transform.childCount;
						for (int n = 0; n < childCount2; n++)
						{
							GameObject gameObject2 = base.transform.GetChild(n).gameObject;
							for (int num9 = 0; num9 < listMutliAni.Count; num9++)
							{
								if (listMutliAni[num9].aniName == gameObject2.name)
								{
									gameObject2.SetActive(listMutliAni[num9].nIntParam1 == nType);
								}
							}
						}
						ArrayUInts[3] = 255u;
						ArrayUInts[4] = 255u;
						ArrayUInts[5] = 255u;
						ArrayUInts[6] = 255u;
						foreach (MutliAni item in listMutliAni)
						{
							if (item.nIntParam1 == nType)
							{
								ArrayUInts[3] = (uint)item.nIntParam2 & 0xFFu;
								ArrayUInts[4] = (uint)((item.nIntParam2 & 0xFF00) >> 8);
								ArrayUInts[5] = (uint)((item.nIntParam2 & 0xFF0000) >> 16);
								ArrayUInts[6] = (uint)(item.nIntParam2 >> 24);
								break;
							}
						}
						tTmpColor.r = (float)ArrayUInts[3] / 255f;
						tTmpColor.g = (float)ArrayUInts[4] / 255f;
						tTmpColor.b = (float)ArrayUInts[5] / 255f;
						tTmpColor.a = (float)ArrayUInts[6] / 255f;
						SetOtherFXColor(tTmpColor);
						for (int num10 = 0; num10 < ArrayFloats.Length; num10++)
						{
							ArrayFloats[num10] = 0f;
						}
						tCountDownBar.gameObject.SetActive(false);
						fTimeLeft = 0f;
						PvpReportUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpReportUI>("UI_PvpReport");
						if ((bool)uI)
						{
							for (int num11 = 0; num11 < OldTrigerDatas.Count; num11++)
							{
								if (OldTrigerDatas[num11].nParam1 == nType && OldTrigerDatas[num11].tObj1 != null)
								{
									string[] array4 = new string[2] { "12C2FFFF", "FF3B3CFF" };
									uI.SetMsg("<color=#" + array4[nType - 1] + ">" + (OldTrigerDatas[num11].tObj1 as OrangeCharacter).sPlayerName + "</color>", "", bmge);
									break;
								}
							}
							break;
						}
						string sPlayerName = "";
						for (int num12 = 0; num12 < OldTrigerDatas.Count; num12++)
						{
							if (OldTrigerDatas[num12].nParam1 == nType && OldTrigerDatas[num12].tObj1 != null)
							{
								string[] array5 = new string[2] { "12C2FFFF", "FF3B3CFF" };
								sPlayerName = "<color=#" + array5[nType - 1] + ">" + (OldTrigerDatas[num12].tObj1 as OrangeCharacter).sPlayerName + "</color>";
								break;
							}
						}
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReport", delegate(PvpReportUI ui)
						{
							ui.SetMsg(sPlayerName, "", bmge);
						});
					}
					else if (array3[0] == 2.ToString())
					{
						fTimeLeft = float.Parse(array3[2]) + MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime;
					}
				}
				else
				{
					if (mapEvent != MapEventEnum.STAGE_ITEM)
					{
						break;
					}
					string[] array6 = smsg.Split(',');
					int num13 = int.Parse(array6[0]);
					nType = int.Parse(array6[1]);
					switch (num13)
					{
					case 0:
					{
						for (int num15 = base.transform.childCount - 1; num15 >= 0; num15--)
						{
							UnityEngine.Object.Destroy(base.transform.GetChild(num15).gameObject);
						}
						StageResManager.LoadStageItemModel(listMutliMove[nType].nIntParam1, base.transform);
						bCheck = true;
						break;
					}
					case 1:
					{
						OrangeCharacter playerByID2 = StageUpdate.GetPlayerByID(array6[2]);
						if ((bool)playerByID2)
						{
							EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
							stageSkillAtkTargetParam.tTrans = playerByID2.transform;
							stageSkillAtkTargetParam.nSkillID = listMutliMove[nType].nIntParam1;
							stageSkillAtkTargetParam.bAtkNoCast = true;
							stageSkillAtkTargetParam.tPos = base.transform.position;
							stageSkillAtkTargetParam.tDir = tMoveToObj ?? Vector3.zero;
							stageSkillAtkTargetParam.bBuff = true;
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
							if (!playerByID2.IsLocalPlayer)
							{
								playerByID2.PlaySE("BattleSE", 11);
							}
						}
						bCheck = false;
						for (int num14 = base.transform.childCount - 1; num14 >= 0; num14--)
						{
							UnityEngine.Object.Destroy(base.transform.GetChild(num14).gameObject);
						}
						break;
					}
					case 2:
						if (bCheck && StageUpdate.gbIsNetGame)
						{
							StageUpdate.SyncStageObj(sSyncID, 1, "0," + nType);
						}
						break;
					}
				}
				break;
			case 2:
			{
				string[] array2 = smsg.Split(',');
				int num = 0;
				switch (mapEvent)
				{
				case MapEventEnum.MOVETO:
				case MapEventEnum.MOVEREPEATE:
					nMoveStep = int.Parse(array2[num++]);
					fMoveSpeed = float.Parse(array2[num++]);
					vMoveVector = GetVector3BySaveStr(array2, num);
					num += 3;
					fTimeLeft = float.Parse(array2[num++]);
					fTotalTimeLeft = float.Parse(array2[num++]);
					NowPos = GetVector3BySaveStr(array2, num);
					num += 3;
					StartPos = GetVector3BySaveStr(array2, num);
					num += 3;
					EndPos = GetVector3BySaveStr(array2, num);
					num += 3;
					nCallCount = int.Parse(array2[num++]);
					bCheck = GetBoolBySaveStr(array2[num++]);
					bStartUpdate = GetBoolBySaveStr(array2[num++]);
					base.transform.position = GetVector3BySaveStr(array2, num);
					num += 3;
					ts_State = (TIMELINE_STATE)int.Parse(array2[num++]);
					CheckNeedUnMove();
					AddSelfToTrigger();
					break;
				case MapEventEnum.RIDE_OBJ:
				{
					NowPos = GetVector3BySaveStr(array2, num);
					num += 3;
					fMoveSpeed = float.Parse(array2[num++]);
					vMoveVector.y = float.Parse(array2[num++]);
					bCheck = GetBoolBySaveStr(array2[num++]);
					bStartUpdate = GetBoolBySaveStr(array2[num++]);
					base.transform.position = GetVector3BySaveStr(array2, num);
					num += 3;
					int num4 = int.Parse(array2[num++]);
					if (num4 > 0)
					{
						for (int num5 = num4 - 1; num5 >= 0; num5--)
						{
							TrigerDatas.Add(new TrigerData());
							TrigerDatas[TrigerDatas.Count - 1].tObj1 = StageUpdate.GetPlayerByID(array2[num++]);
							if (TrigerDatas[TrigerDatas.Count - 1].tObj1 != null)
							{
								OrangeCharacter orangeCharacter = TrigerDatas[TrigerDatas.Count - 1].tObj1 as OrangeCharacter;
								TrigerDatas[TrigerDatas.Count - 1].tTriggerTransform = orangeCharacter.transform;
							}
						}
					}
					else
					{
						TrigerDatas.Clear();
					}
					break;
				}
				case MapEventEnum.CIRCLE_OBJ_MOVE:
					vMoveVector.y = float.Parse(array2[num++]) - vMoveVector.x;
					vMoveVector.x += vMoveVector.y;
					foreach (TrigerData trigerData2 in TrigerDatas)
					{
						Vector3 localPosition = trigerData2.tTriggerTransform.localPosition;
						Vector3 vector = Quaternion.Euler(0f, 0f, vMoveVector.y) * localPosition;
						Vector3 vector2 = vector - localPosition;
						CheckPlayerEnemyAndMove(vector2, trigerData2.vMax, trigerData2.vMin, trigerData2.listTriCollisionTrans);
						trigerData2.vMax += vector2;
						trigerData2.vMin += vector2;
						trigerData2.tTriggerTransform.localPosition = vector;
					}
					foreach (TrigerData oldTrigerData in OldTrigerDatas)
					{
						oldTrigerData.fTimeLeft = float.Parse(array2[num++]);
						oldTrigerData.tTriggerTransform.localRotation = Quaternion.AngleAxis(oldTrigerData.fTimeLeft, Vector3.up);
					}
					fTimeLeft = float.Parse(array2[num++]);
					bCheck = GetBoolBySaveStr(array2[num++]);
					bStartUpdate = GetBoolBySaveStr(array2[num++]);
					break;
				case MapEventEnum.ANGLE_OBJ_MOVE:
				{
					nType = int.Parse(array2[num++]);
					float num3 = float.Parse(array2[num++]);
					vMoveVector.x = float.Parse(array2[num++]);
					foreach (TrigerData trigerData3 in TrigerDatas)
					{
						Vector3 localPosition2 = trigerData3.tTriggerTransform.localPosition;
						Vector3 vector3 = Quaternion.Euler(0f, 0f, num3 - fMoveSpeed) * localPosition2;
						Vector3 vector4 = vector3 - localPosition2;
						CheckPlayerEnemyAndMove(vector4, trigerData3.vMax, trigerData3.vMin, trigerData3.listTriCollisionTrans);
						trigerData3.vMax += vector4;
						trigerData3.vMin += vector4;
						trigerData3.tTriggerTransform.localPosition = vector3;
					}
					fMoveSpeed = num3;
					foreach (TrigerData oldTrigerData2 in OldTrigerDatas)
					{
						oldTrigerData2.fTimeLeft = float.Parse(array2[num++]);
						oldTrigerData2.tTriggerTransform.localRotation = Quaternion.AngleAxis(oldTrigerData2.fTimeLeft, Vector3.up);
					}
					bCheck = GetBoolBySaveStr(array2[num++]);
					bStartUpdate = GetBoolBySaveStr(array2[num++]);
					break;
				}
				case MapEventEnum.PLAY_FLAG_MODE:
				{
					nMoveStep = int.Parse(array2[num++]);
					ArrayFloats[0] = float.Parse(array2[num++]);
					ArrayFloats[1] = float.Parse(array2[num++]);
					fTimeLeft = float.Parse(array2[num++]);
					if (nMoveStep != nType)
					{
						nType = nMoveStep;
						int childCount = base.transform.childCount;
						for (int i = 0; i < childCount; i++)
						{
							GameObject gameObject = base.transform.GetChild(i).gameObject;
							for (int j = 0; j < listMutliAni.Count; j++)
							{
								if (listMutliAni[j].aniName == gameObject.name)
								{
									gameObject.SetActive(listMutliAni[j].nIntParam1 == nType);
								}
							}
						}
						ArrayUInts[3] = 255u;
						ArrayUInts[4] = 255u;
						ArrayUInts[5] = 255u;
						ArrayUInts[6] = 255u;
						foreach (MutliAni item2 in listMutliAni)
						{
							if (item2.nIntParam1 == nType)
							{
								ArrayUInts[3] = (uint)item2.nIntParam2 & 0xFFu;
								ArrayUInts[4] = (uint)((item2.nIntParam2 & 0xFF00) >> 8);
								ArrayUInts[5] = (uint)((item2.nIntParam2 & 0xFF0000) >> 16);
								ArrayUInts[6] = (uint)(item2.nIntParam2 >> 24);
								break;
							}
						}
						tTmpColor.r = (float)ArrayUInts[3] / 255f;
						tTmpColor.g = (float)ArrayUInts[4] / 255f;
						tTmpColor.b = (float)ArrayUInts[5] / 255f;
						tTmpColor.a = (float)ArrayUInts[6] / 255f;
						SetOtherFXColor(tTmpColor);
					}
					nMoveStep = int.Parse(array2[num++]);
					OldTrigerDatas.Clear();
					for (int k = 0; k < nMoveStep; k++)
					{
						OrangeCharacter playerByID = StageUpdate.GetPlayerByID(array2[num++]);
						if ((bool)playerByID)
						{
							TrigerData trigerData = new TrigerData();
							trigerData.tTriggerTransform = playerByID.transform;
							trigerData.nParam1 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(playerByID);
							OldTrigerDatas.Add(trigerData);
						}
					}
					bCheck = GetBoolBySaveStr(array2[num++]);
					bStartUpdate = GetBoolBySaveStr(array2[num++]);
					break;
				}
				case MapEventEnum.START_FALL:
				{
					if (!GetBoolBySaveStr(array2[num]) && bCheck)
					{
						StartCoroutine(StartFallCoroutine());
					}
					bCheck = GetBoolBySaveStr(array2[num++]);
					bStartUpdate = GetBoolBySaveStr(array2[num++]);
					base.transform.position = GetVector3BySaveStr(array2, num);
					num += 3;
					List<FallingFloor> list = new List<FallingFloor>();
					for (int l = 0; l < base.transform.childCount; l++)
					{
						FallingFloor[] componentsInChildren = base.transform.GetChild(l).GetComponentsInChildren<FallingFloor>();
						if (componentsInChildren != null)
						{
							list.AddRange(componentsInChildren);
						}
					}
					int num6 = int.Parse(array2[num++]);
					for (int m = 0; m < num6; m++)
					{
						string text = array2[num++];
						Vector3 vector3BySaveStr = GetVector3BySaveStr(array2, num);
						num += 3;
						int num7 = 0;
						while (num7 < list.Count)
						{
							if (list[m].gameObject.name == text)
							{
								list[m].transform.position = vector3BySaveStr;
								break;
							}
							m++;
						}
					}
					break;
				}
				case MapEventEnum.SIMULATE_FALLOBJ:
				{
					bool num2 = bCheck;
					bCheck = GetBoolBySaveStr(array2[num++]);
					bStartUpdate = GetBoolBySaveStr(array2[num++]);
					if (num2 != bCheck && bStartUpdate)
					{
						StartSimulateFallObj(base.transform.position, tMoveToObj ?? Vector3.zero);
					}
					fTimeLeft = float.Parse(array2[num++]);
					fTotalTimeLeft = float.Parse(array2[num++]);
					vMoveVector = GetVector3BySaveStr(array2, num);
					num += 3;
					base.transform.position = GetVector3BySaveStr(array2, num);
					num += 3;
					nType = int.Parse(array2[num++]);
					break;
				}
				}
				break;
			}
			}
		}

		protected void PlaySE(string allname)
		{
			string[] array = allname.Split(',');
			if (array.Length == 2)
			{
				PlaySE(array[0], array[1]);
			}
		}

		protected void PlaySE(string allname, ref bool bCheckPlayed)
		{
			if (!bCheckPlayed)
			{
				bCheckPlayed = true;
				PlaySE(allname);
			}
		}

		protected void PlaySE(string acb, string cueName)
		{
			base.SoundSource.PlaySE(acb, cueName);
		}
	}
}
