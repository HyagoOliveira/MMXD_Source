#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CallbackDefs;
using DragonBones;
using Newtonsoft.Json;
using OrangeApi;
using OrangeAudio;
using OrangeSocket;
using UnityEngine;
using UnityEngine.SceneManagement;
using cb;
using enums;

namespace StageLib
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(StageResManager))]
	public class StageUpdate : MonoBehaviour, IManagedUpdateBehavior
	{
		public class EnemyCtrlID
		{
			public enum ENEMY_BIT_DESC
			{
				NONE = 0,
				SHOW_HINT = 1,
				BOOS_SHOW_HINT = 2,
				JUMP_DOWN = 4,
				DEAD_SHOCK = 8,
				NET_SYNC = 0x10
			}

			public EnemyControllerBase mEnemy;

			public int nEnemyBitParam;

			public int nSCEID;

			public float fSetAimRange;

			public float fSetAimRangeY;

			public float fSetOffsetX;

			public float fSetOffsetY;
		}

		public class LoadCallBackObj
		{
			public delegate void CallStageLoadEnd(LoadCallBackObj tObj, UnityEngine.Object asset);

			public object objParam0 = 0;

			public int i;

			public object loadStageObjData;

			public CallStageLoadEnd lcb;

			private bool _bLoadEnd;

			private LoadCallBackObj refSelf;

			public bool bLoadEnd
			{
				get
				{
					return _bLoadEnd;
				}
			}

			public LoadCallBackObj()
			{
				refSelf = this;
				lcb = StandLoadCB;
			}

			public void LoadCB(UnityEngine.Object asset)
			{
				if (_bLoadEnd)
				{
					Debug.LogError("已經載完了卻又呼叫一次，請通知程式，這很嚴重.");
					return;
				}
				if (lcb != null)
				{
					lcb(this, asset);
				}
				_bLoadEnd = true;
				refSelf = null;
			}

			public void LoadCBNoParam()
			{
				if (_bLoadEnd)
				{
					Debug.LogError("已經載完了卻又呼叫一次，請通知程式，這很嚴重.");
					return;
				}
				if (lcb != null)
				{
					lcb(this, null);
				}
				_bLoadEnd = true;
				refSelf = null;
			}

			public void LoadUIComplete<T>(T ui) where T : Component
			{
				if (_bLoadEnd)
				{
					Debug.LogError("已經載完了卻又呼叫一次，請通知程式，這很嚴重.");
					return;
				}
				if (lcb != null)
				{
					lcb(this, ui);
				}
				_bLoadEnd = true;
				refSelf = null;
			}

			private void StandLoadCB(LoadCallBackObj tLCB, object asset)
			{
				StageResManager.RemoveLCB(tLCB);
			}
		}

		public delegate void OnSyncStageFunc(string sIDKey, int nKey1, string smsg);

		public class MixBojSyncData
		{
			private OnSyncStageFunc[] listMixSyncFunc = new OnSyncStageFunc[10];

			private void ReserveaySyncMixStageFunc(int nCount)
			{
				if (nCount > listMixSyncFunc.Length - 1)
				{
					int i;
					for (i = listMixSyncFunc.Length; i - 1 < nCount; i += 10)
					{
					}
					OnSyncStageFunc[] array = new OnSyncStageFunc[i];
					for (int j = 0; j < listMixSyncFunc.Length; j++)
					{
						array[j] = listMixSyncFunc[j];
					}
					listMixSyncFunc = array;
				}
			}

			public void SetSyncMixStageFunc(string sID, OnSyncStageFunc tOnSyncStageFunc)
			{
				if (sID.Length != 0)
				{
					int num = sID.LastIndexOf('-');
					int result;
					if (num != -1)
					{
						int.TryParse(sID.Substring(num + 1), out result);
					}
					else
					{
						int.TryParse(sID, out result);
					}
					ReserveaySyncMixStageFunc(result);
					if (listMixSyncFunc[result] != null)
					{
						Debug.Log(string.Concat("SetSyncMixStageFunc ", sID, " overlap ", result, " ", listMixSyncFunc[result], " to", tOnSyncStageFunc));
					}
					listMixSyncFunc[result] = tOnSyncStageFunc;
				}
			}

			public void OnSyncStageFunc(string sIDKey, int nKey1, string smsg)
			{
				if (sIDKey.Length != 0)
				{
					int num = sIDKey.IndexOf('-');
					string sIDKey2 = "";
					if (num != -1)
					{
						sIDKey2 = sIDKey.Substring(num + 1);
						sIDKey = sIDKey.Substring(0, num);
					}
					int result;
					int.TryParse(sIDKey, out result);
					if (result >= 0 && result < listMixSyncFunc.Length && listMixSyncFunc[result] != null)
					{
						listMixSyncFunc[result](sIDKey2, nKey1, smsg);
					}
				}
			}
		}

		public const string STAGEDATA_PREFAB = "jsondatas/stagedata/";

		public static string gStageFolder = "/RockmanUnity/StageData/";

		public static string gStageName = "NewStage01";

		public static int gDifficulty = 1;

		public const string gEnemyHeader = "_e";

		private const string STAGE_PREFIXES = "STSYNC";

		public float fStageClipWidth = 25f;

		[HideInInspector]
		public bool bUseAssetBundle;

		public static int nReConnectMode = 1;

		public static bool bIsHost = true;

		public static bool bMainPlayerOK = false;

		public static bool gbGeneratePvePlayer = false;

		public static bool gbRegisterPvpPlayer = false;

		public static StageMode StageMode = StageMode.Normal;

		[HideInInspector]
		public float fStageUseTime;

		[HideInInspector]
		public int nReBornCount;

		[HideInInspector]
		public int nBtnStatus = 255;

		[HideInInspector]
		private bool bAddStageUseTime = true;

		[HideInInspector]
		public bool bWaitNetStageUseTime;

		[HideInInspector]
		public static bool bWaitReconnect = false;

		[HideInInspector]
		public Coroutine tWaitReconnectCoroutine;

		[HideInInspector]
		public Coroutine tWaitGameReadyAndSendReConnectMsgCoroutine;

		[HideInInspector]
		public Coroutine tWaitPauseCoroutine;

		[HideInInspector]
		public Coroutine tUnSlowStageCoroutine;

		[HideInInspector]
		public Coroutine tCheckIsBossAppearCoroutine;

		[HideInInspector]
		public Dictionary<int, Coroutine> tDictDeadAreaEventCoroutine = new Dictionary<int, Coroutine>();

		private float fTimeDeltaTimeTmp;

		public const float fWaitReConnectTimeLimit = 1f;

		private const int nReConnectTryMaxTimes = 5;

		[ReadOnly]
		public float fWaitReconnectCoroutineTime;

		private bool bIsEnd;

		private float fSendCheckConnectTime = 0.5f;

		private const float fRESEND_WAIT_TIME = 0.5f;

		private const float fWaitConnectTimeLimit = 3f;

		private bool bLoadStageResEnd;

		public bool bIsHaveEventStageEnd;

		private bool bIsWaitingStageEndRes;

		private StageEndRes lastStageEndRes;

		public static bool AllStageCtrlEvent = false;

		private static List<EnemyCtrlID> EnemySets = new List<EnemyCtrlID>();

		private static List<OrangeCharacter> runPlayerList = new List<OrangeCharacter>();

		private List<StageObjBase> runSOBList = new List<StageObjBase>();

		private static List<string> listLockRange = new List<string>();

		private static List<BulletBase> listBullet = new List<BulletBase>();

		private List<sbyte> listStageSecert = new List<sbyte>();

		private static List<GameObject> EndNeedRemoveObjs = new List<GameObject>();

		private List<StageQuest> listStageQuests = new List<StageQuest>();

		private List<StageDataPoint> listStageDataPoint = new List<StageDataPoint>();

		private List<string> listPerGameSaveData = new List<string>();

		private List<SyncBullet> listSyncBullet = new List<SyncBullet>();

		private static OrangeCharacter MainOC = null;

		private static bool bStageReady = false;

		private List<LoadCallBackObj> mLoadCallBackObj = new List<LoadCallBackObj>();

		private List<Camera> mAllCameras = new List<Camera>();

		private List<StageStartPoint> tStartGameObj = new List<StageStartPoint>();

		private List<StageObjGroupData> mstageObjGroupDatas = new List<StageObjGroupData>();

		private StageObjGroupData CheckAloneObjGroup = new StageObjGroupData();

		private int nNeedLoadCount;

		private Dictionary<int, UnityEngine.Object> LoadAssets = new Dictionary<int, UnityEngine.Object>();

		private static int nNetSerialID = 0;

		private float fWaitSlowTime;

		private bool bLockRewardProcess = true;

		private static bool bIsRewardUI = false;

		public StageOpenCommonTask tStageOpenCommonTask = new StageOpenCommonTask();

		[HideInInspector]
		public CommonUI tEndCommonUI;

		private List<AniSpeedData> listSaveAniSpeed = new List<AniSpeedData>();

		private List<DragonSpeedData> listDragonSpeedData = new List<DragonSpeedData>();

		public StageSyncEventClass tStageSyncEventClass = new StageSyncEventClass();

		private OnSyncStageFunc[] aySyncStageFunc = new OnSyncStageFunc[10];

		private List<MixBojSyncData> listMixBojSync = new List<MixBojSyncData>();

		private float fNowUpdateTimeDelta;

		public const float fConstUpdateMinTime = 0.016f;

		private static List<EventPointBase> listUpdataEvents = new List<EventPointBase>();

		private static List<EventPointBase> listAllEvents = new List<EventPointBase>();

		[ReadOnly]
		public int nRunStageCtrlCount;

		public string sLastLockRangeSyncID = "";

		private HurtPassParam tHurtPassParam = new HurtPassParam();

		private static EnemyControllerBase tTmpEnemyControllerBase;

		private List<float> cmin = new List<float>();

		private List<float> cmax = new List<float>();

		private int nSyncStageObjID;

		private string nSyncStageObjsIDKey;

		private int nCheckDmg;

		private bool bHasSkill;

		private SKILL_TABLE tCheckSkillTable;

		private WeaponStruct tCheckWeaponStruct;

		private int nSOBType;

		private int nBulletIndex;

		public static bool gbIsNetGame
		{
			get
			{
				if (!gbGeneratePvePlayer)
				{
					return gbRegisterPvpPlayer;
				}
				return true;
			}
		}

		public bool gbAddStageUseTime
		{
			get
			{
				return bAddStageUseTime;
			}
			set
			{
				bAddStageUseTime = value;
				BattleInfoUI.Instance.SwitchStageCountDownTime(value);
			}
		}

		public bool IsEnd
		{
			get
			{
				return bIsEnd;
			}
			set
			{
				if (value)
				{
					bIsEnd = true;
				}
			}
		}

		public static bool gbStageReady
		{
			get
			{
				if (bStageReady)
				{
					return MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.isStart;
				}
				return false;
			}
			set
			{
			}
		}

		public static List<EventPointBase> listAllUpdataEvent
		{
			get
			{
				return listUpdataEvents;
			}
		}

		public static List<EventPointBase> listAllEvent
		{
			get
			{
				return listAllEvents;
			}
		}

		public static bool IsCanGamePause
		{
			get
			{
				if ((bool)MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_BattleSetting"))
				{
					MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_BattleSetting").OnClickCloseBtn();
				}
				if ((bool)MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_GamePause"))
				{
					MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_GamePause").OnClickCloseBtn();
				}
				if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !MonoBehaviourSingleton<OrangeGameManager>.Instance.bLastGamePause && !(MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_BattleSetting") != null) && !(MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_GamePause") != null) && !MonoBehaviourSingleton<UIManager>.Instance.bLockTurtorial)
				{
					return !MonoBehaviourSingleton<UIManager>.Instance.IsBlockCloseing;
				}
				return false;
			}
		}

		public static List<EnemyCtrlID> runEnemys
		{
			get
			{
				return EnemySets;
			}
		}

		public static List<OrangeCharacter> runPlayers
		{
			get
			{
				return runPlayerList;
			}
		}

		public static List<BulletBase> runBulletSets
		{
			get
			{
				return listBullet;
			}
		}

		private void Start()
		{
			bUseAssetBundle = AssetBundleScriptableObject.Instance.m_useAssetBundle;
			MonoBehaviourSingleton<StageSyncManager>.Instance.bLoadingStage = true;
			StageResManager.loadingCbMax = 0;
			Singleton<GenericEventManager>.Instance.AttachEvent<OrangeCharacter>(EventManager.ID.STAGE_PLAYER_SPWAN_ED, RegisterPlayer);
			Singleton<GenericEventManager>.Instance.AttachEvent<OrangeCharacter, bool>(EventManager.ID.STAGE_PLAYER_DESTROY_ED, UnRegisterPlayer);
			Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.STAGE_UPDATE_HOST, UpdateHost);
			Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.STAGE_END_REPORT, ShowStageRewardUI);
			Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.STAGE_ALLOK, StageStart);
			Singleton<GenericEventManager>.Instance.AttachEvent<BulletBase>(EventManager.ID.STAGE_BULLET_REGISTER, RegisterBullet);
			Singleton<GenericEventManager>.Instance.AttachEvent<BulletBase>(EventManager.ID.STAGE_BULLET_UNREGISTER, UnRegisterBullet);
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.RegisterStageParam>(EventManager.ID.REGISTER_STAGE_PARAM, RegisterStageParam);
			Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BATTLE_START, Init);
			Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SWITCH_SCENE, SWITCH_SCENE);
			bLoadStageResEnd = false;
			ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Win;
			MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTBroadcastToRoom, OnNTSyncStageObj);
			mstageObjGroupDatas.Clear();
			StageResManager.LoadInfoBar();
			nNetSerialID = 7;
			StageResManager.InitStageBattleUI(LoadBattleInfoUIEndCB);
		}

		private void OnDestroy()
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<OrangeCharacter>(EventManager.ID.STAGE_PLAYER_SPWAN_ED, RegisterPlayer);
			Singleton<GenericEventManager>.Instance.DetachEvent<OrangeCharacter, bool>(EventManager.ID.STAGE_PLAYER_DESTROY_ED, UnRegisterPlayer);
			Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.STAGE_UPDATE_HOST, UpdateHost);
			Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.STAGE_END_REPORT, ShowStageRewardUI);
			Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.STAGE_ALLOK, StageStart);
			Singleton<GenericEventManager>.Instance.DetachEvent<BulletBase>(EventManager.ID.STAGE_BULLET_REGISTER, RegisterBullet);
			Singleton<GenericEventManager>.Instance.DetachEvent<BulletBase>(EventManager.ID.STAGE_BULLET_UNREGISTER, UnRegisterBullet);
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.RegisterStageParam>(EventManager.ID.REGISTER_STAGE_PARAM, RegisterStageParam);
			Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BATTLE_START, Init);
			Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SWITCH_SCENE, SWITCH_SCENE);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTBroadcastToRoom, OnNTSyncStageObj);
			MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
			ManagedSingleton<StageHelper>.Instance.nLastStageRuleID = 0;
			ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = 0;
			ClearList();
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.OnClickCloseBtn();
			}
			StageResManager.ResetStageUpdate();
			bWaitReconnect = false;
		}

		public static string GetNowHostPlayerID()
		{
			return MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID;
		}

		public static void ReqChangeHost()
		{
			if (!bIsHost)
			{
				return;
			}
			string text = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + ",";
			bool flag = false;
			foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
			{
				if (item.PlayerId != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && item.bInGame && !item.bInPause)
				{
					text += item.PlayerId;
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID = item.PlayerId;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				bIsHost = false;
				MonoBehaviourSingleton<StageSyncManager>.Instance.SendChangeHost(text);
			}
		}

		private static void ClearList()
		{
			runPlayerList.Clear();
			EnemySets.Clear();
			listLockRange.Clear();
			listUpdataEvents.Clear();
			listAllEvents.Clear();
			listBullet.Clear();
			EndNeedRemoveObjs.Clear();
			if (StageResManager.GetStageUpdate() != null)
			{
				StageResManager.GetStageUpdate().listStageQuests.Clear();
				StageResManager.GetStageUpdate().listStageDataPoint.Clear();
				StageResManager.GetStageUpdate().listPerGameSaveData.Clear();
			}
			StageResManager.RemoveAllLockEvent();
		}

		private IEnumerator StartStageCoroutine()
		{
			WaitForSecondsRealtime wfs = new WaitForSecondsRealtime(0.1f);
			StageHelper.StageCharacterStruct stageCharacterStruct = ManagedSingleton<StageHelper>.Instance.GetStageCharacterStruct();
			if (gbIsNetGame)
			{
				foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
				{
					StageResManager.LoadAllPassSkill(stageCharacterStruct, item);
					StageResManager.LoadWeaponAB(item.netSealBattleSettingInfo.MainWeaponInfo.WeaponID);
					StageResManager.LoadWeaponAB(item.netSealBattleSettingInfo.SubWeaponInfo.WeaponID);
					foreach (NetCharacterInfo character in item.netSealBattleSettingInfo.CharacterList)
					{
						StageResManager.LoadPlayerAB(character.CharacterID, character.Skin, item.netSealBattleSettingInfo.CharacterSkillList);
						StageResManager.LoadMotion(character.CharacterID, item.netSealBattleSettingInfo.MainWeaponInfo.WeaponID, item.netSealBattleSettingInfo.SubWeaponInfo.WeaponID);
					}
					for (int num = item.netSealBattleSettingInfo.TotalFSList.Count - 1; num >= 0; num--)
					{
						if (item.netSealBattleSettingInfo.TotalFSList[num].FinalStrikeID == item.netSealBattleSettingInfo.PlayerInfo.MainWeaponFSID)
						{
							StageResManager.LoadBulletBySkillTable(ManagedSingleton<OrangeTableHelper>.Instance.getFS_SkillTable(item.netSealBattleSettingInfo.TotalFSList[num].FinalStrikeID, item.netSealBattleSettingInfo.TotalFSList[num].Level, item.netSealBattleSettingInfo.TotalFSList[num].Star));
						}
						if (item.netSealBattleSettingInfo.TotalFSList[num].FinalStrikeID == item.netSealBattleSettingInfo.PlayerInfo.SubWeaponFSID)
						{
							StageResManager.LoadBulletBySkillTable(ManagedSingleton<OrangeTableHelper>.Instance.getFS_SkillTable(item.netSealBattleSettingInfo.TotalFSList[num].FinalStrikeID, item.netSealBattleSettingInfo.TotalFSList[num].Level, item.netSealBattleSettingInfo.TotalFSList[num].Star));
						}
					}
				}
			}
			else
			{
				StageResManager.LoadWeaponAB(stageCharacterStruct.MainWeaponID);
				StageResManager.LoadWeaponAB(stageCharacterStruct.SubWeaponID);
				StageResManager.LoadPlayerAB(stageCharacterStruct.StandbyChara, stageCharacterStruct.Skin, stageCharacterStruct.listNetCharacterSkillInfos);
				StageResManager.LoadMotion(stageCharacterStruct.StandbyChara, stageCharacterStruct.MainWeaponID, stageCharacterStruct.SubWeaponID);
				StageResManager.LoadAllPassSkill(stageCharacterStruct);
				FinalStrikeInfo value;
				if (stageCharacterStruct.MainWeaponFSID != 0)
				{
					ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(stageCharacterStruct.MainWeaponFSID, out value);
					if (value != null)
					{
						StageResManager.LoadBulletBySkillTable(ManagedSingleton<OrangeTableHelper>.Instance.getFS_SkillTable(value.netFinalStrikeInfo.FinalStrikeID, value.netFinalStrikeInfo.Level, value.netFinalStrikeInfo.Star));
					}
				}
				if (stageCharacterStruct.SubWeaponFSID != 0)
				{
					ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(stageCharacterStruct.SubWeaponFSID, out value);
					if (value != null)
					{
						StageResManager.LoadBulletBySkillTable(ManagedSingleton<OrangeTableHelper>.Instance.getFS_SkillTable(value.netFinalStrikeInfo.FinalStrikeID, value.netFinalStrikeInfo.Level, value.netFinalStrikeInfo.Star));
					}
				}
			}
			StageResManager.LoadFx("FX_MOB_EXPLODE0");
			if (BattleInfoUI.Instance.NowStageTable.w_BOSS_INTRO != "null")
			{
				StageResManager.LoadObject(string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, "St_Enemy_" + BattleInfoUI.Instance.NowStageTable.w_BOSS_INTRO), "St_Enemy_" + BattleInfoUI.Instance.NowStageTable.w_BOSS_INTRO);
			}
			LoadCallBackObj loadCallBackObj = new LoadCallBackObj();
			mLoadCallBackObj.Add(loadCallBackObj);
			MonoBehaviourSingleton<UIManager>.Instance.PreloadUI("UI_Ready", loadCallBackObj.LoadCBNoParam);
			DISC_TABLE value2 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(stageCharacterStruct.MainWeaponChipID, out value2) && value2.s_ICON != "" && value2.s_ICON != "null")
			{
				LoadCallBackObj loadCallBackObj2 = new LoadCallBackObj();
				mLoadCallBackObj.Add(loadCallBackObj2);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.m_iconChip, value2.s_ICON, loadCallBackObj2.LoadCB);
			}
			value2 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(stageCharacterStruct.SubWeaponChipID, out value2) && value2.s_ICON != "" && value2.s_ICON != "null")
			{
				LoadCallBackObj loadCallBackObj3 = new LoadCallBackObj();
				mLoadCallBackObj.Add(loadCallBackObj3);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.m_iconChip, value2.s_ICON, loadCallBackObj3.LoadCB);
			}
			STAGE_TABLE nowStageTable = BattleInfoUI.Instance.NowStageTable;
			List<string> alreadyPreload = new List<string>();
			PreloadStageBGM(alreadyPreload, nowStageTable.s_BGM);
			PreloadStageBGM(alreadyPreload, nowStageTable.s_BOSSENTRY_BGM);
			PreloadStageBGM(alreadyPreload, nowStageTable.s_BOSSBATTLE_BGM);
			yield return CoroutineDefine._1sec;
			bool bLoadAll;
			do
			{
				bLoadAll = true;
				float num2 = StageResManager.CheckLoadEnd();
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_STAGE_RES_PROGRESS, num2);
				if (num2 != 1f)
				{
					bLoadAll = false;
				}
				for (int i = 0; i < mLoadCallBackObj.Count; i++)
				{
					if (!mLoadCallBackObj[i].bLoadEnd)
					{
						bLoadAll = false;
						break;
					}
				}
				yield return wfs;
			}
			while (!bLoadAll);
			while (MonoBehaviourSingleton<OrangeSceneManager>.Instance.ListExtraLoadingAssets.Count > 0)
			{
				yield return wfs;
			}
			while (!MonoBehaviourSingleton<EnemyHumanResourceManager>.Instance.IsLoadDone())
			{
				yield return wfs;
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_STAGE_RES_PROGRESS, 1f);
			while (MonoBehaviourSingleton<OrangeSceneManager>.Instance.IsLoading)
			{
				yield return wfs;
			}
			bLoadStageResEnd = true;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_RESTART, gbGeneratePvePlayer || gbRegisterPvpPlayer, bIsHost);
		}

		private void PreloadStageBGM(List<string> alreadyPreload, string tableField)
		{
			if (tableField != null && tableField != "" && tableField != "null")
			{
				string[] array = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(tableField);
				if (!alreadyPreload.Contains(array[0]))
				{
					LoadCallBackObj loadCallBackObj = new LoadCallBackObj();
					mLoadCallBackObj.Add(loadCallBackObj);
					MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(array[0], 1, loadCallBackObj.LoadCBNoParam);
					alreadyPreload.Add(array[0]);
				}
			}
		}

		public void StageStart()
		{
			if (bStageReady || !bLoadStageResEnd)
			{
				return;
			}
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if ((bool)rootGameObjects[i].GetComponentInChildren<MeshFilter>(true))
				{
					rootGameObjects[i].AddOrGetComponent<StageBatchingRoot>();
				}
			}
			SetSyncStageFunc(0.ToString(), OnSyncStageHP);
			SetSyncStageFunc(1.ToString(), OnSyncStageEnemyAction);
			SetSyncStageFunc(2.ToString(), OnSyncPlayerAction);
			SetSyncStageFunc(3.ToString(), tStageSyncEventClass.OnSyncStageEvent);
			SetSyncStageFunc(4.ToString(), OnSyncStatus);
			SetSyncStageFunc(5.ToString(), OnSyncStageObjAction);
			SetSyncStageFunc(6.ToString(), OnSyncBulletAction);
			Light[] array = OrangeSceneManager.FindObjectsOfTypeCustom<Light>();
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].type != LightType.Directional)
				{
					array[j].gameObject.SetActive(false);
				}
			}
			ManagedSingleton<InputStorage>.Instance.ClearInputData();
			ManagedSingleton<StageHelper>.Instance.bEnemyActive = true;
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
			MonoBehaviourSingleton<UpdateManager>.Instance.Pause = false;
			MonoBehaviourSingleton<StageSyncManager>.Instance.bPauseAllPlayerInput = false;
			fStageUseTime = 0f;
			for (int k = 0; k < mAllCameras.Count; k++)
			{
				mAllCameras[k].gameObject.SetActive(true);
			}
			ManagedSingleton<StageHelper>.Instance.fCameraHHalf = Mathf.Tan(0.5f * mAllCameras[0].fieldOfView * ((float)Math.PI / 180f)) * Mathf.Abs(mAllCameras[0].transform.position.z);
			ManagedSingleton<StageHelper>.Instance.fCameraWHalf = ManagedSingleton<StageHelper>.Instance.fCameraHHalf * mAllCameras[0].aspect;
			bStageReady = true;
			if (bWaitReconnect)
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect = true;
			}
			for (int l = 0; l < tStartGameObj.Count; l++)
			{
				if (tStartGameObj[l].bMoveCamera)
				{
					EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
					if (!gbRegisterPvpPlayer || (gbRegisterPvpPlayer && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam() == tStartGameObj[l].StartID + 1))
					{
						stageCameraFocus.roominpos = tStartGameObj[l].transform.position;
						stageCameraFocus.nMode = 2;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
					}
					if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect)
					{
						stageCameraFocus.nMode = 0;
						stageCameraFocus.bLock = true;
						stageCameraFocus.bRightNow = true;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
					}
				}
			}
			EventManager.LockRangeParam lockRangeParam = new EventManager.LockRangeParam();
			lockRangeParam.nMode = 3;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCK_RANGE, lockRangeParam);
			if (GetComponent<VisualDamageSystem>() == null)
			{
				base.gameObject.AddComponent<VisualDamageSystem>();
			}
			GoCheckUI.InitTempPlayerData();
			UnityEngine.Transform canvasUI = MonoBehaviourSingleton<UIManager>.Instance.CanvasUI;
			if (canvasUI == null)
			{
				return;
			}
			canvasUI = MonoBehaviourSingleton<UIManager>.Instance.JoystickPanelParent;
			if (!(canvasUI == null))
			{
				Canvas component = canvasUI.GetComponent<Canvas>();
				if (!(component == null))
				{
					component.enabled = true;
					CloseWaitStartStageBlackCB();
				}
			}
		}

		private void CloseWaitStartStageBlackCB()
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
			{
				STAGE_TABLE nowStageTable = BattleInfoUI.Instance.NowStageTable;
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
				{
					if (nowStageTable.n_MAIN == 90001 && (nowStageTable.n_SUB == 1 || nowStageTable.n_SUB == 2))
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpBar", delegate(PvpBarUI ui)
						{
							ui.Setup();
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_START);
						});
					}
					else if (nowStageTable.n_MAIN == 90000 && nowStageTable.n_SUB == 1)
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpBar", delegate(PvpBarUI ui)
						{
							ui.Setup();
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_START);
						});
					}
					else
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_START);
					}
				}
				else
				{
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj == null)
					{
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj = new StageUiDataScriptObj();
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj.InitDefaultData();
					}
					StageUiData stageUiData = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj.GetStageUiData(nowStageTable);
					if (stageUiData != null)
					{
						switch (stageUiData.nUIType)
						{
						case StageUiData.STAGE_UI_ENUM.ShowKillScoreUI:
						{
							int num = 0;
							int[] array = new int[3] { nowStageTable.n_CLEAR_VALUE1, nowStageTable.n_CLEAR_VALUE2, nowStageTable.n_CLEAR_VALUE3 };
							for (int i = 0; i < array.Length; i++)
							{
								if (array[i] > num)
								{
									num = array[i];
								}
							}
							BattleInfoUI.Instance.ShowKillScoreUI(num, stageUiData.nParam0);
							break;
						}
						case StageUiData.STAGE_UI_ENUM.ShowGetItemUI:
							BattleInfoUI.Instance.ShowGetItemUI();
							break;
						case StageUiData.STAGE_UI_ENUM.ShowBattleScoreUI:
							if (stageUiData.bParam0)
							{
								BattleInfoUI.Instance.nCampaignTotalScore = 0;
							}
							BattleInfoUI.Instance.ShowBattleScoreUI(stageUiData.nParam0);
							break;
						case StageUiData.STAGE_UI_ENUM.ShowGetItemUI2:
							BattleInfoUI.Instance.ShowLoveScoreUI(stageUiData.nParam0, stageUiData.bParam0, stageUiData.bParam1);
							break;
						case StageUiData.STAGE_UI_ENUM.ShowContributionNowNum:
							BattleInfoUI.Instance.ShowContributionNowNum();
							break;
						}
					}
					if (nowStageTable.n_TYPE == 10 && StageMode == StageMode.Contribute)
					{
						BattleInfoUI.Instance.ShowContributionNowNum();
					}
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_START);
				}
				if (nowStageTable.s_BGM != null && nowStageTable.s_BGM != "" && nowStageTable.s_BGM != "null")
				{
					string[] array2 = ManagedSingleton<OrangeTableHelper>.Instance.ParseSE(nowStageTable.s_BGM);
					MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(array2[0], array2[1]);
				}
			}, 2f);
		}

		public void Init()
		{
			for (int i = 0; i < tStartGameObj.Count; i++)
			{
				tStartGameObj[i].gameObject.SetActive(true);
				if (!gbIsNetGame)
				{
					break;
				}
			}
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect)
			{
				ReqChangeHost();
				SyncStageObj(3, 17, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true, true);
				SendReConnectMsg();
			}
		}

		public void SWITCH_SCENE()
		{
			if (listBullet.Count > 0)
			{
				BackAllBullet();
			}
		}

		public void SendReConnectMsg()
		{
			if (nReConnectMode == 0)
			{
				PauseCommonOut();
			}
			else
			{
				if (nReConnectMode != 1 || !gbIsNetGame)
				{
					return;
				}
				if (IsEnd)
				{
					Debug.LogError("SendReConnectMsg but is IsEnd ");
				}
				else if (!gbStageReady)
				{
					if (tWaitGameReadyAndSendReConnectMsgCoroutine == null)
					{
						tWaitGameReadyAndSendReConnectMsgCoroutine = StartCoroutine(WaitGameReadyAndSendReConnectMsgCoroutine());
					}
				}
				else if (bIsHost)
				{
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect)
					{
						PauseCommonOut();
						return;
					}
					Debug.LogError("SendReConnectMsg but is host ");
					bWaitReconnect = false;
					MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 2);
				}
				else if (tWaitReconnectCoroutine == null)
				{
					tWaitReconnectCoroutine = StartCoroutine(WaitReconnectCoroutine(nRunStageCtrlCount > 0));
					bWaitReconnect = true;
					MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 2);
					if (nRunStageCtrlCount <= 0)
					{
						WaitNetMsgCommon();
						SyncStageObj(3, 6, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true, true);
					}
				}
			}
		}

		private IEnumerator WaitGameReadyAndSendReConnectMsgCoroutine()
		{
			while (!gbStageReady)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			tWaitGameReadyAndSendReConnectMsgCoroutine = null;
			if (bIsHost)
			{
				Debug.LogError("SendReConnectMsg but is host WaitGameReadyAndSendReConnectMsgCoroutine");
				ReqChangeHost();
				if (bIsHost)
				{
					bIsHost = false;
				}
			}
			SendReConnectMsg();
		}

		private IEnumerator WaitReconnectCoroutine(bool bNeedCheckStageCtrl)
		{
			while (bNeedCheckStageCtrl)
			{
				if (nRunStageCtrlCount <= 0)
				{
					WaitNetMsgCommon();
					MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 2);
					SyncStageObj(3, 6, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true, true);
					break;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
			fWaitReconnectCoroutineTime = 1f;
			int nCheckCount = 0;
			while (bWaitReconnect)
			{
				fWaitReconnectCoroutineTime -= Time.deltaTime;
				if (bIsHost)
				{
					Debug.LogError("送重連訊息後發現主機已經跑了");
					PauseCommonOut(true);
					break;
				}
				if (fWaitReconnectCoroutineTime <= 0f)
				{
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayingPlayerCount(true, true) == 0)
					{
						Debug.LogWarning("主機也暫停了，等主機解暫停!!!");
						MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
						MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 2);
						SyncStageObj(3, 6, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true, true);
						fWaitReconnectCoroutineTime = 1f;
						WaitNetMsgCommon();
						continue;
					}
					if (nCheckCount >= 5)
					{
						PauseCommonOut(true);
						break;
					}
					nCheckCount++;
					Debug.LogWarning("重連訊息重送次數:" + nCheckCount);
					fWaitReconnectCoroutineTime = 1f;
					MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 2);
					SyncStageObj(3, 6, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true, true);
				}
				else
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			tWaitReconnectCoroutine = null;
		}

		public void PauseCommonOut(bool bNoCheckEnd = false)
		{
			if (bNoCheckEnd || !IsEnd)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
				bWaitReconnect = false;
				IsEnd = true;
				ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
				StageResManager.RemoveAllLockEvent();
				tStageOpenCommonTask.OpenCommon(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TIP"), BattleInfoUI.Instance.StageOutGO);
			}
		}

		private void WaitNetMsgCommon()
		{
			if (!IsEnd)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
				tStageOpenCommonTask.OpenCommon(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_UNSTABLE"));
			}
		}

		private IEnumerator SendCheckConnectCoroutine()
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			while (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.UpdateCheckConnectTime(1f, 3f))
			{
				MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 0);
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ResetCheckConnectTime(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
				yield return CoroutineDefine._1sec;
			}
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
			tStageOpenCommonTask.CloseCommon();
		}

		public static void StopStageAllEvent()
		{
			bStageReady = false;
			for (int i = 0; i < runPlayerList.Count; i++)
			{
				runPlayerList[i].EventLockInputing = true;
				runPlayerList[i].gameObject.layer = 0;
			}
			int n_TYPE = BattleInfoUI.Instance.NowStageTable.n_TYPE;
			if ((uint)(n_TYPE - 9) <= 1u)
			{
				for (int j = 0; j < EnemySets.Count; j++)
				{
					EnemySets[j].mEnemy.Activate = false;
				}
			}
			else
			{
				for (int k = 0; k < EnemySets.Count; k++)
				{
					EnemySets[k].mEnemy.NullHurtAction();
					EnemySets[k].mEnemy.BackToPool();
				}
			}
			foreach (OrangeCharacter runPlayer in runPlayerList)
			{
				runPlayer.PlayerAutoAimSystem.SetEnable(false);
			}
			for (int num = EndNeedRemoveObjs.Count - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(EndNeedRemoveObjs[num]);
			}
			EndNeedRemoveObjs.Clear();
		}

		public static void SetStageName(string sName, int nDifficulty = 1, bool bReturnToBootup = false, bool bGeneratePvePlayer = false, bool bRegisterPvpPlayer = false)
		{
			string text = sName.Insert(sName.IndexOf("_e1"), "_standalone");
			string text2 = text;
			string text3 = "";
			int num = text2.IndexOf('/');
			if (num == -1)
			{
				num = text2.IndexOf('_');
				text3 = text2.Substring(0, num);
			}
			else
			{
				text3 = text2.Substring(0, num);
				text2 = text2.Substring(num + 1);
			}
			num = text2.LastIndexOf("_e");
			if (num != -1)
			{
				text2 = text2.Substring(0, num);
			}
			if (MonoBehaviourSingleton<AssetsBundleManager>.Instance.IsBundleExistInList("jsondatas/stagedata/" + text2))
			{
				sName = text;
			}
			if (MonoBehaviourSingleton<AssetsBundleManager>.Instance.IsBundleExistInList("jsondatas/stagedata/" + text3 + "/" + text2))
			{
				sName = text;
			}
			gStageName = sName;
			gDifficulty = nDifficulty;
			gbGeneratePvePlayer = bGeneratePvePlayer;
			gbRegisterPvpPlayer = bRegisterPvpPlayer;
			bMainPlayerOK = false;
			MonoBehaviourSingleton<StageSyncManager>.Instance.nLastSendBattleWinType = -1;
		}

		public void AddSubLoadAB(LoadCallBackObj tSubLoad)
		{
			mLoadCallBackObj.Add(tSubLoad);
		}

		private static int GetNetSerialID()
		{
			return nNetSerialID++;
		}

		public static void RegisterEventUpdate(EventPointBase tEPB)
		{
			if (!(tEPB == null) && !listUpdataEvents.Contains(tEPB))
			{
				listUpdataEvents.Add(tEPB);
			}
		}

		public static void RemoveEventUpdate(EventPointBase tEPB)
		{
			if (!(tEPB == null) && listUpdataEvents.Contains(tEPB))
			{
				listUpdataEvents.Remove(tEPB);
			}
		}

		public static void RegisterToAllEventList(EventPointBase tEPB)
		{
			if (!(tEPB == null) && !listAllEvents.Contains(tEPB))
			{
				listAllEvents.Add(tEPB);
			}
		}

		public static void RemoveToAllEventList(EventPointBase tEPB)
		{
			if (!(tEPB == null) && listAllEvents.Contains(tEPB))
			{
				listAllEvents.Remove(tEPB);
			}
		}

		public static EventPointBase GetEventBySyncID(string sSyncID)
		{
			foreach (EventPointBase listAllEvent in listAllEvents)
			{
				if (listAllEvent.sSyncID == sSyncID)
				{
					return listAllEvent;
				}
			}
			return null;
		}

		public void UpdateFunc()
		{
			StageUpdateCall();
		}

		public void StageUpdateCall()
		{
			if (!bStageReady || bIsEnd)
			{
				return;
			}
			fTimeDeltaTimeTmp = Time.deltaTime;
			if (bMainPlayerOK && bAddStageUseTime)
			{
				fStageUseTime += fTimeDeltaTimeTmp;
			}
			fNowUpdateTimeDelta += fTimeDeltaTimeTmp;
			while (fNowUpdateTimeDelta >= 0.016f)
			{
				for (int num = listUpdataEvents.Count - 1; num >= 0; num--)
				{
					listUpdataEvents[num].UpdateEventBase();
				}
				fNowUpdateTimeDelta -= 0.016f;
			}
			if (mAllCameras.Count > 0)
			{
				GetRunClipGroups(mAllCameras[0].transform.position);
			}
			if (gbRegisterPvpPlayer && !MonoBehaviourSingleton<OrangeGameManager>.Instance.bLastGamePause && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.UpdateCheckConnectTime(fTimeDeltaTimeTmp, 3f))
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
				OpenNetWaitCommonUI();
				StartCoroutine(SendCheckConnectCoroutine());
			}
		}

		private void OpenNetWaitCommonUI()
		{
			if (!IsEnd)
			{
				tStageOpenCommonTask.OpenCommon(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_UNSTABLE"));
			}
		}

		private void LateUpdate()
		{
			if (gbRegisterPvpPlayer)
			{
				fSendCheckConnectTime -= Time.deltaTime;
				if (fSendCheckConnectTime <= 0f)
				{
					fSendCheckConnectTime = 0.5f;
					MonoBehaviourSingleton<StageSyncManager>.Instance.SendPlayerCheckConnect(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, 0);
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ResetCheckConnectTime(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
				}
			}
		}

		public static void RegisterPetSendAndRun(string sNetSyncID, int nSetKey, string sOther = "", bool bNotCheckHost = false)
		{
			SyncStageObj(5, 0, sNetSyncID + "," + nSetKey + "," + sOther, bNotCheckHost);
			MonoBehaviourSingleton<StageSyncManager>.Instance.WaitAvgPetDelayRun(sNetSyncID, nSetKey, sOther);
		}

		public static void RegisterSyncBulletSendAndRun(string sNetSyncID, int nSetKey, string sOther = "", bool bNotCheckHost = false)
		{
			SyncStageObj(6, 0, sNetSyncID + "," + nSetKey + "," + sOther, bNotCheckHost);
			MonoBehaviourSingleton<StageSyncManager>.Instance.WaitAvgBulletDelayRun(sNetSyncID, nSetKey, sOther);
		}

		public static void RegisterSendAndRun(string sNetSyncID, int nSetKey, string sOther = "", bool bNotCheckHost = false)
		{
			SyncStageObj(1, 0, sNetSyncID + "," + nSetKey + "," + sOther, bNotCheckHost);
			MonoBehaviourSingleton<StageSyncManager>.Instance.WaitAvgDelayRun(sNetSyncID, nSetKey, sOther);
		}

		public void ChangeStageByName(string stagename, int diff = 1)
		{
			StageUpdate stageUpdate = null;
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int num = runEnemys.Count - 1; num >= 0; num--)
			{
				runEnemys[num].mEnemy.BackToPool();
			}
			BackAllBullet();
			listStageSecert.Clear();
			listStageQuests.Clear();
			listStageDataPoint.Clear();
			listPerGameSaveData.Clear();
			listSyncBullet.Clear();
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].name == "StageUpdate")
				{
					stageUpdate = rootGameObjects[i].GetComponent<StageUpdate>();
				}
				else if (rootGameObjects[i].GetComponent<StageGroupRoot>() != null)
				{
					for (int j = 0; j < rootGameObjects[i].transform.childCount; j++)
					{
						list.Add(rootGameObjects[i].transform.GetChild(j).gameObject);
					}
				}
				else
				{
					list.Add(rootGameObjects[i]);
				}
			}
			bStageReady = false;
			for (int k = 0; k < stageUpdate.mAllCameras.Count; k++)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_DELETE_CHECK, stageUpdate.mAllCameras[0].gameObject);
			}
			stageUpdate.mAllCameras.Clear();
			for (int l = 0; l < list.Count; l++)
			{
				UnityEngine.Object.Destroy(list[l]);
			}
			tStartGameObj.Clear();
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera = null;
			MonoBehaviourSingleton<InputManager>.Instance.DestroyVirtualPad();
			mLoadCallBackObj = new List<LoadCallBackObj>();
			mstageObjGroupDatas = new List<StageObjGroupData>();
			CheckAloneObjGroup = new StageObjGroupData();
			SetStageName(stagename, diff);
			LoadStage(stageUpdate);
		}

		public static void RunTestStage(StageOnceCoroutine.StageOnceParam tStageOnceParam)
		{
			LoadCallBackObj loadCallBackObj = new LoadCallBackObj();
			loadCallBackObj.loadStageObjData = tStageOnceParam.param1;
			loadCallBackObj.lcb = delegate(LoadCallBackObj tObj, UnityEngine.Object asset)
			{
				GoCheckUI obj = asset as GoCheckUI;
				obj.Setup(null);
				obj.sGoToStageName = tObj.loadStageObjData as string;
				obj.nGoToDifficult = (int)tStageOnceParam.param2;
			};
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GoCheckUI>("UI_GoCheck", loadCallBackObj.LoadCB);
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		}

		public void GetRunClipGroups(Vector3 p)
		{
			cmin.Clear();
			cmax.Clear();
			cmin.Add(p.x - fStageClipWidth);
			cmax.Add(p.x + fStageClipWidth);
			for (int num = runPlayerList.Count - 1; num >= 0; num--)
			{
				Vector3 position = runPlayerList[num].transform.position;
				cmin.Add(position.x - fStageClipWidth);
				cmax.Add(position.x + fStageClipWidth);
			}
			for (int num2 = EnemySets.Count - 1; num2 >= 0; num2--)
			{
				Vector3 position2 = EnemySets[num2].mEnemy.transform.position;
				cmin.Add(position2.x - fStageClipWidth);
				cmax.Add(position2.x + fStageClipWidth);
			}
			for (int num3 = runSOBList.Count - 1; num3 >= 0; num3--)
			{
				Vector3 position3 = runSOBList[num3].transform.position;
				cmin.Add(position3.x - fStageClipWidth);
				cmax.Add(position3.x + fStageClipWidth);
			}
			if (gbRegisterPvpPlayer)
			{
				for (int num4 = tStartGameObj.Count - 1; num4 >= 0; num4--)
				{
					Vector3 position4 = tStartGameObj[num4].transform.position;
					cmin.Add(position4.x - fStageClipWidth);
					cmax.Add(position4.x + fStageClipWidth);
				}
			}
			bool flag = false;
			for (int num5 = mstageObjGroupDatas.Count - 1; num5 >= 0; num5--)
			{
				flag = false;
				for (int num6 = cmin.Count - 1; num6 >= 0; num6--)
				{
					if (cmax[num6] > mstageObjGroupDatas[num5].fClipMinx && cmin[num6] < mstageObjGroupDatas[num5].fClipMaxx)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					if (!mstageObjGroupDatas[num5].bInRun)
					{
						mstageObjGroupDatas[num5].SetActuveAll(true);
					}
					mstageObjGroupDatas[num5].bInRun = true;
					mstageObjGroupDatas[num5].UpdateRender(p);
				}
				else
				{
					if (mstageObjGroupDatas[num5].bInRun)
					{
						mstageObjGroupDatas[num5].UpdateRender(p);
						mstageObjGroupDatas[num5].SetActuveNoEnemy(false);
					}
					mstageObjGroupDatas[num5].bInRun = false;
				}
			}
		}

		public void GetStageSceneInB2D(BoxCollider2D tCheckC2D, ref List<StageSceneObjParam> tOutList, Vector3? vExpand = null)
		{
			Bounds tCheckBounds = tCheckC2D.bounds;
			if (vExpand.HasValue)
			{
				tCheckBounds.Expand(vExpand ?? Vector3.zero);
			}
			foreach (StageObjGroupData mstageObjGroupData in mstageObjGroupDatas)
			{
				if (!mstageObjGroupData.bInRun)
				{
					continue;
				}
				mstageObjGroupData.CheckDataParams();
				for (int i = 0; i < mstageObjGroupData.Datas.Count; i++)
				{
					foreach (StageSceneObjParam item in mstageObjGroupData.DataParams[i])
					{
						if (item.CheckIntersectB2D(ref tCheckBounds))
						{
							tOutList.Add(item);
						}
					}
				}
			}
			CheckAloneObjGroup.CheckDataParams();
			for (int j = 0; j < CheckAloneObjGroup.Datas.Count; j++)
			{
				foreach (StageSceneObjParam item2 in CheckAloneObjGroup.DataParams[j])
				{
					if (item2.CheckIntersectB2D(ref tCheckBounds))
					{
						tOutList.Add(item2);
					}
				}
			}
		}

		public void GetStageSceneObjContainPoint(Vector3 tPos, ref List<StageSceneObjParam> tOutList)
		{
			foreach (StageObjGroupData mstageObjGroupData in mstageObjGroupDatas)
			{
				if (!mstageObjGroupData.bInRun)
				{
					continue;
				}
				mstageObjGroupData.CheckDataParams();
				for (int i = 0; i < mstageObjGroupData.Datas.Count; i++)
				{
					foreach (StageSceneObjParam item in mstageObjGroupData.DataParams[i])
					{
						if (item.CheckContainPoint(tPos))
						{
							tOutList.Add(item);
						}
					}
				}
			}
			CheckAloneObjGroup.CheckDataParams();
			for (int j = 0; j < CheckAloneObjGroup.Datas.Count; j++)
			{
				foreach (StageSceneObjParam item2 in CheckAloneObjGroup.DataParams[j])
				{
					if (item2.CheckContainPoint(tPos))
					{
						tOutList.Add(item2);
					}
				}
			}
		}

		private void ReserveaySyncStageFunc(int nCount)
		{
			if (nCount > aySyncStageFunc.Length - 1)
			{
				int i;
				for (i = aySyncStageFunc.Length; i - 1 < nCount; i += 10)
				{
				}
				OnSyncStageFunc[] array = new OnSyncStageFunc[i];
				for (int j = 0; j < aySyncStageFunc.Length; j++)
				{
					array[j] = aySyncStageFunc[j];
				}
				aySyncStageFunc = array;
			}
		}

		public void SetSyncStageFunc(string sID, OnSyncStageFunc tOnSyncStageFunc)
		{
			if (sID.Length != 0 && sID.LastIndexOf('-') == -1)
			{
				int result;
				int.TryParse(sID, out result);
				ReserveaySyncStageFunc(result);
				aySyncStageFunc[result] = tOnSyncStageFunc;
			}
		}

		public void AddMixBojSyncData(MixBojSyncData tMixBojSyncData)
		{
			listMixBojSync.Add(tMixBojSyncData);
		}

		public void LoadStageJSonABEnd(LoadCallBackObj tObj, UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				LoadAssets.Add(tObj.i, asset);
				if (LoadAssets.Count == nNeedLoadCount)
				{
					LoadStage(this, "", LoadAssets);
				}
			}
		}

		public static string GetPropertyStr(GameObject tObj)
		{
			string text = "";
			tObj.GetComponent<StageEnemy>();
			tObj.GetComponent<StageStartPoint>();
			tObj.GetComponent<StageEndPoint>();
			Camera component = tObj.GetComponent<Camera>();
			Light component2 = tObj.GetComponent<Light>();
			tObj.GetComponent<LockRangeEvent>();
			tObj.GetComponent<EnemyEventPoint>();
			int num = 0;
			StageSLBase[] components = tObj.GetComponents<StageSLBase>();
			if (components.Length == 1)
			{
				text = components[0].GetTypeID() + "," + components[0].GetSaveString();
				num++;
			}
			else
			{
				num = components.Length;
			}
			if (component != null)
			{
				text = 4.ToString();
				num++;
			}
			if (component2 != null)
			{
				text = 5.ToString();
				num++;
			}
			if (num > 1)
			{
				text = 99.ToString();
				for (int i = 0; i < components.Length; i++)
				{
					text = text + "," + components[i].GetSaveString();
				}
			}
			if (num == 0)
			{
				text = 0.ToString();
				Renderer component3 = tObj.GetComponent<Renderer>();
				if (component3 != null)
				{
					text = text + "," + component3.lightmapIndex;
					text = text + "," + component3.lightmapScaleOffset.x;
					text = text + "," + component3.lightmapScaleOffset.y;
					text = text + "," + component3.lightmapScaleOffset.z;
					text = text + "," + component3.lightmapScaleOffset.w;
				}
				Renderer[] componentsInChildren = tObj.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (componentsInChildren[j].GetInstanceID() != tObj.GetInstanceID())
					{
						string text2 = "";
						UnityEngine.Transform parent = componentsInChildren[j].transform;
						while (parent.GetInstanceID() != tObj.transform.GetInstanceID())
						{
							text2 = ((text2.Length <= 0) ? parent.gameObject.name : (parent.gameObject.name + "/" + text2));
							parent = parent.parent;
						}
						text = text + "," + text2;
						text = text + "," + componentsInChildren[j].lightmapIndex;
						text = text + "," + componentsInChildren[j].lightmapScaleOffset.x;
						text = text + "," + componentsInChildren[j].lightmapScaleOffset.y;
						text = text + "," + componentsInChildren[j].lightmapScaleOffset.z;
						text = text + "," + componentsInChildren[j].lightmapScaleOffset.w;
					}
				}
			}
			return text;
		}

		public static int LoadProperty(StageObjGroupData tStageObjGroupData, GameObject newObj, string strProperty, StageUpdate tStageUpdate = null, string sSyncID = "", MixBojSyncData tOtherMixBojSyncData = null)
		{
			if (strProperty == "" || strProperty == null)
			{
				strProperty = 0.ToString();
			}
			string[] array = strProperty.Split(',');
			StageObjType stageObjType = (StageObjType)int.Parse(array[0]);
			bool flag = true;
			StageEnemy stageEnemy = null;
			StageSLBase stageSLBase = null;
			Renderer renderer = null;
			Vector4 zero = Vector4.zero;
			int num = 1;
			UnityEngine.Transform transform = null;
			switch (stageObjType)
			{
			case StageObjType.PREFAB_OBJ:
				if (array.Length > 1)
				{
					renderer = newObj.GetComponent<Renderer>();
					if (renderer != null)
					{
						renderer.lightmapIndex = int.Parse(array[num++]);
						zero.x = float.Parse(array[num++]);
						zero.y = float.Parse(array[num++]);
						zero.z = float.Parse(array[num++]);
						zero.w = float.Parse(array[num++]);
						renderer.lightmapScaleOffset = zero;
					}
					while (array.Length > num)
					{
						transform = newObj.transform.Find(array[num++]);
						if (transform == null)
						{
							break;
						}
						renderer = transform.GetComponent<Renderer>();
						if (renderer == null)
						{
							break;
						}
						renderer.lightmapIndex = int.Parse(array[num++]);
						zero.x = float.Parse(array[num++]);
						zero.y = float.Parse(array[num++]);
						zero.z = float.Parse(array[num++]);
						zero.w = float.Parse(array[num++]);
						renderer.lightmapScaleOffset = zero;
					}
				}
				if (tStageUpdate != null || tOtherMixBojSyncData != null)
				{
					newObj.AddOrGetComponent<StageSceneObjParam>();
				}
				break;
			case StageObjType.LIGHT_OBJ:
			case StageObjType.BG_OBJ:
				flag = false;
				break;
			case StageObjType.CAMERA_OBJ:
				if (tStageUpdate != null)
				{
					tStageUpdate.mAllCameras.Add(newObj.GetComponent<Camera>());
				}
				flag = false;
				break;
			case StageObjType.MIX_OBJ:
				return (int)stageObjType;
			case StageObjType.START_OBJ:
				stageSLBase = newObj.GetComponent<StageSLBase>();
				if (tStageUpdate != null)
				{
					tStageUpdate.tStartGameObj.Add((StageStartPoint)stageSLBase);
					stageSLBase.gameObject.SetActive(false);
				}
				if (stageSLBase != null)
				{
					stageSLBase.sSyncID = sSyncID;
					if (tStageUpdate != null)
					{
						tStageUpdate.SetSyncStageFunc(sSyncID, stageSLBase.OnSyncStageObj);
					}
					else if (tOtherMixBojSyncData != null)
					{
						tOtherMixBojSyncData.SetSyncMixStageFunc(sSyncID, stageSLBase.OnSyncStageObj);
					}
					stageSLBase.LoadByString(array[1]);
					flag = stageSLBase.IsNeedClip();
				}
				break;
			default:
				stageSLBase = newObj.GetComponent<StageSLBase>();
				if (stageSLBase != null)
				{
					stageSLBase.sSyncID = sSyncID;
					if (tStageUpdate != null)
					{
						tStageUpdate.SetSyncStageFunc(sSyncID, stageSLBase.OnSyncStageObj);
					}
					else if (tOtherMixBojSyncData != null)
					{
						tOtherMixBojSyncData.SetSyncMixStageFunc(sSyncID, stageSLBase.OnSyncStageObj);
					}
					stageSLBase.LoadByString(array[1]);
					flag = stageSLBase.IsNeedClip();
					if (stageSLBase as EventPointBase != null)
					{
						RegisterToAllEventList(stageSLBase as EventPointBase);
					}
				}
				break;
			}
			if (tStageUpdate != null)
			{
				if (flag)
				{
					if (newObj.layer != 2)
					{
						newObj.SetActive(false);
						if (stageEnemy != null)
						{
							tStageObjGroupData.EnemyDatas.Add(stageEnemy);
						}
						else
						{
							tStageObjGroupData.AddDatas(newObj);
						}
					}
				}
				else if (stageSLBase != null && stageSLBase.IsNeedCheckClipAlone())
				{
					tStageUpdate.CheckAloneObjGroup.Datas.Add(newObj);
				}
			}
			else if (tStageObjGroupData != null)
			{
				tStageObjGroupData.Datas.Add(newObj);
			}
			return (int)stageObjType;
		}

		private void LoadBattleInfoUIEndCB()
		{
			LoadStage(this);
		}

		public static List<StageData> LoadStage(StageUpdate tStageUpdate, string filepath = "", Dictionary<int, UnityEngine.Object> assets = null)
		{
			string text = Application.dataPath + gStageFolder + gStageName + ".json";
			if (!filepath.Equals(""))
			{
				text = filepath;
				string text2 = Path.GetFileNameWithoutExtension(text);
				int num = text2.LastIndexOf("_e");
				if (num != -1)
				{
					text2 = text2.Substring(0, num);
				}
				gStageName = text2;
			}
			if (tStageUpdate != null)
			{
				bStageReady = false;
				tStageUpdate.mstageObjGroupDatas.Clear();
				tStageUpdate.aySyncStageFunc = new OnSyncStageFunc[10];
				tStageUpdate.mAllCameras.Clear();
				ClearList();
				tStageUpdate.nNeedLoadCount = 0;
				if (assets == null)
				{
					tStageUpdate.LoadAssets.Clear();
					string text3 = gStageName;
					string text4 = "";
					int num2 = text3.IndexOf('/');
					if (num2 == -1)
					{
						num2 = text3.IndexOf('_');
						text4 = text3.Substring(0, num2);
					}
					else
					{
						text4 = text3.Substring(0, num2);
						text3 = text3.Substring(num2 + 1);
					}
					num2 = text3.LastIndexOf("_e");
					if (num2 != -1)
					{
						tStageUpdate.nNeedLoadCount++;
					}
					string text5 = text3;
					tStageUpdate.nNeedLoadCount++;
					LoadCallBackObj loadCallBackObj = null;
					if (num2 != -1)
					{
						text5 = text3.Substring(0, num2);
						loadCallBackObj = new LoadCallBackObj();
						loadCallBackObj.i = 0;
						loadCallBackObj.lcb = tStageUpdate.LoadStageJSonABEnd;
						tStageUpdate.mLoadCallBackObj.Add(loadCallBackObj);
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("jsondatas/stagedata/" + text4 + "/" + text5, text5, loadCallBackObj.LoadCB);
						loadCallBackObj = new LoadCallBackObj();
						loadCallBackObj.i = 0;
						loadCallBackObj.lcb = tStageUpdate.LoadStageJSonABEnd;
						tStageUpdate.mLoadCallBackObj.Add(loadCallBackObj);
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("jsondatas/stagedata/" + text5, text5, loadCallBackObj.LoadCB);
					}
					loadCallBackObj = new LoadCallBackObj();
					loadCallBackObj.i = 1;
					loadCallBackObj.lcb = tStageUpdate.LoadStageJSonABEnd;
					tStageUpdate.mLoadCallBackObj.Add(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("jsondatas/stagedata/" + text4 + "/" + text5, text3, loadCallBackObj.LoadCB);
					loadCallBackObj = new LoadCallBackObj();
					loadCallBackObj.i = 1;
					loadCallBackObj.lcb = tStageUpdate.LoadStageJSonABEnd;
					tStageUpdate.mLoadCallBackObj.Add(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("jsondatas/stagedata/" + text3, text3, loadCallBackObj.LoadCB);
					return null;
				}
			}
			if (assets != null || File.Exists(text))
			{
				string text6 = "";
				List<StageData> list = new List<StageData>();
				if (tStageUpdate != null)
				{
					tStageUpdate.mLoadCallBackObj.Clear();
				}
				if (assets != null)
				{
					for (int i = 0; i < assets.Count; i++)
					{
						text6 = ((TextAsset)assets[i]).text;
						list.Add(LoadPerStageData(text6, tStageUpdate));
					}
				}
				else
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					string text7 = text.Substring(0, text.LastIndexOf(fileNameWithoutExtension));
					int num3 = fileNameWithoutExtension.LastIndexOf("_e");
					if (num3 != -1)
					{
						fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, num3);
						if (File.Exists(text7 + fileNameWithoutExtension + ".json"))
						{
							text6 = File.ReadAllText(text7 + fileNameWithoutExtension + ".json", Encoding.UTF8);
							list.Add(LoadPerStageData(text6, tStageUpdate));
						}
					}
					if (File.Exists(text))
					{
						text6 = File.ReadAllText(text, Encoding.UTF8);
						list.Add(LoadPerStageData(text6, tStageUpdate));
					}
				}
				if (tStageUpdate != null)
				{
					for (int j = 0; j < tStageUpdate.mLoadCallBackObj.Count; j++)
					{
						if (tStageUpdate.mLoadCallBackObj[j].loadStageObjData != null)
						{
							StageObjData stageObjData = tStageUpdate.mLoadCallBackObj[j].loadStageObjData as StageObjData;
							int num4 = stageObjData.path.LastIndexOf("/");
							string text8 = stageObjData.path.Substring(num4 + 1);
							text8 = text8.Substring(0, text8.LastIndexOf("."));
							MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(stageObjData.bunldepath, text8, tStageUpdate.mLoadCallBackObj[j].LoadCB);
						}
					}
				}
				if (tStageUpdate != null && list.Count > 0)
				{
					tStageUpdate.fStageClipWidth = list[0].fStageClipWidth;
					tStageUpdate.StartCoroutine("StartStageCoroutine");
				}
				return list;
			}
			return null;
		}

		public static void InitObjGroupRoot(string sGroupID, GameObject tObj)
		{
			if (sGroupID != "")
			{
				tObj.transform.parent = GetGroupRoot(sGroupID).transform;
			}
		}

		public static GameObject GetGroupRoot(string groupname)
		{
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].GetComponent<StageGroupRoot>() != null && rootGameObjects[i].name == groupname)
				{
					return rootGameObjects[i];
				}
			}
			GameObject obj = new GameObject();
			obj.name = groupname;
			obj.transform.position = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.transform.rotation = Quaternion.identity;
			obj.AddComponent<StageGroupRoot>().SetSGroupID(groupname);
			return obj;
		}

		private static int GetStageObjDatanetID(StageObjData tStageObjData)
		{
			if (tStageObjData == null || tStageObjData.property == "")
			{
				return 0;
			}
			StageObjType stageObjType = (StageObjType)int.Parse(tStageObjData.property.Split(',')[0]);
			if (stageObjType == StageObjType.PREFAB_OBJ || (uint)(stageObjType - 4) <= 2u)
			{
				return 0;
			}
			return GetNetSerialID();
		}

		private static StageData LoadPerStageData(string jstr, StageUpdate tStageUpdate)
		{
			StageData stageData = StageData.LoadByJSONStr(jstr);
			int num = 0;
			int num2 = 0;
			num = stageData.Datas.Count;
			for (int i = 0; i < num; i++)
			{
				StageObjGroupData stageObjGroupData = new StageObjGroupData();
				stageObjGroupData.fClipMinx = stageData.Datas[i].fClipMinx;
				stageObjGroupData.fClipMaxx = stageData.Datas[i].fClipMaxx;
				if (tStageUpdate != null)
				{
					tStageUpdate.mstageObjGroupDatas.Add(stageObjGroupData);
				}
				num2 = stageData.Datas[i].Datas.Count;
				for (int j = 0; j < num2; j++)
				{
					if (stageData.Datas[i].Datas[j].bunldepath == "")
					{
						Debug.LogError("prefab has no bundle path:" + stageData.Datas[i].Datas[j].path);
						continue;
					}
					LoadCallBackObj loadCallBackObj = new LoadCallBackObj();
					loadCallBackObj.i = tStageUpdate.mstageObjGroupDatas.Count - 1;
					loadCallBackObj.loadStageObjData = stageData.Datas[i].Datas[j];
					loadCallBackObj.lcb = tStageUpdate.StageLoadEndCall;
					loadCallBackObj.objParam0 = GetStageObjDatanetID(stageData.Datas[i].Datas[j]).ToString();
					tStageUpdate.mLoadCallBackObj.Add(loadCallBackObj);
				}
			}
			if (((uint)stageData.nVer & (true ? 1u : 0u)) != 0)
			{
				int num3 = gStageName.LastIndexOf("_e");
				string text = gStageName;
				if (num3 != -1)
				{
					text = text.Substring(0, num3);
				}
				LoadCallBackObj loadCallBackObj2 = new LoadCallBackObj();
				loadCallBackObj2.i = 0;
				loadCallBackObj2.lcb = tStageUpdate.StageLoadLightMapListCall;
				tStageUpdate.mLoadCallBackObj.Add(loadCallBackObj2);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("lightmaps/" + text, "lightmaplist", loadCallBackObj2.LoadCB);
			}
			else
			{
				LightmapSettings.lightmaps = new LightmapData[0];
			}
			return stageData;
		}

		private void StageLoadEndCall(LoadCallBackObj tObj, UnityEngine.Object asset)
		{
			int i = tObj.i;
			if (!(asset == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset) as GameObject;
				StageObjData stageObjData = (StageObjData)tObj.loadStageObjData;
				InitObjGroupRoot(stageObjData.sGroupID, gameObject);
				gameObject.transform.position = stageObjData.position;
				gameObject.transform.localScale = stageObjData.scale;
				gameObject.transform.rotation = stageObjData.rotate;
				gameObject.name = stageObjData.name;
				LoadProperty(mstageObjGroupDatas[i], gameObject, stageObjData.property, this, tObj.objParam0 as string);
			}
		}

		private void StageLoadLightMapListCall(LoadCallBackObj tObj, UnityEngine.Object asset)
		{
			StageLightMapJson stageLightMapJson = JsonUtility.FromJson<StageLightMapJson>(((TextAsset)asset).text);
			LightmapSettings.lightmaps = new LightmapData[0];
			int num = gStageName.LastIndexOf("_e");
			string text = gStageName;
			if (num != -1)
			{
				text = text.Substring(0, num);
			}
			for (int i = 0; i < stageLightMapJson.Datas.Count; i++)
			{
				string text2 = stageLightMapJson.Datas[i];
				if (text2.Contains("Lightmap-"))
				{
					int num2 = text2.IndexOf("Lightmap-") + "Lightmap-".Length;
					if (text2.Contains("_comp_dir"))
					{
						int num3 = text2.IndexOf("_comp_dir");
						int i2 = int.Parse(text2.Substring(num2, num3 - num2));
						LoadCallBackObj loadCallBackObj = new LoadCallBackObj();
						loadCallBackObj.i = i2;
						loadCallBackObj.lcb = StageLoadLightMapDirCall;
						mLoadCallBackObj.Add(loadCallBackObj);
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("lightmaps/" + text, text2, loadCallBackObj.LoadCB);
					}
					if (text2.Contains("_comp_light"))
					{
						int num4 = text2.IndexOf("_comp_light");
						int i3 = int.Parse(text2.Substring(num2, num4 - num2));
						LoadCallBackObj loadCallBackObj2 = new LoadCallBackObj();
						loadCallBackObj2.i = i3;
						loadCallBackObj2.lcb = StageLoadLightMapColorCall;
						mLoadCallBackObj.Add(loadCallBackObj2);
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("lightmaps/" + text, text2, loadCallBackObj2.LoadCB);
					}
				}
			}
		}

		private void StageLoadLightMapDirCall(LoadCallBackObj tObj, UnityEngine.Object asset)
		{
			LightmapData[] array = null;
			if (LightmapSettings.lightmaps.Length <= tObj.i)
			{
				array = new LightmapData[tObj.i + 1];
				for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
				{
					array[i] = LightmapSettings.lightmaps[i];
				}
				for (int j = LightmapSettings.lightmaps.Length; j < tObj.i + 1; j++)
				{
					array[j] = new LightmapData();
				}
				LightmapSettings.lightmaps = array;
			}
			array = LightmapSettings.lightmaps;
			array[tObj.i].lightmapDir = asset as Texture2D;
			LightmapSettings.lightmaps = array;
			Debug.Log("jimmytsai111");
		}

		private void StageLoadLightMapColorCall(LoadCallBackObj tObj, UnityEngine.Object asset)
		{
			LightmapData[] array = null;
			if (LightmapSettings.lightmaps.Length <= tObj.i)
			{
				array = new LightmapData[tObj.i + 1];
				for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
				{
					array[i] = LightmapSettings.lightmaps[i];
				}
				for (int j = LightmapSettings.lightmaps.Length; j < tObj.i + 1; j++)
				{
					array[j] = new LightmapData();
				}
				LightmapSettings.lightmaps = array;
			}
			array = LightmapSettings.lightmaps;
			array[tObj.i].lightmapColor = asset as Texture2D;
			LightmapSettings.lightmaps = array;
			Debug.Log("jimmytsai222");
		}

		public static GameObject[] GetStageAllObjs()
		{
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].name == "StageUpdate")
				{
					continue;
				}
				if (rootGameObjects[i].GetComponent<StageGroupRoot>() != null)
				{
					UnityEngine.Transform transform = rootGameObjects[i].transform;
					for (int j = 0; j < transform.childCount; j++)
					{
						UnityEngine.Transform child = transform.GetChild(j);
						if (child.GetComponent<StageGroupRoot>() != null)
						{
							Debug.LogError("只能有一層喔!!");
						}
						else
						{
							list.Add(child.gameObject);
						}
					}
				}
				else
				{
					list.Add(rootGameObjects[i]);
				}
			}
			return list.ToArray();
		}

		public static List<StageObjGroupData> ClipGroup(float fClipWidth, int nMode = 0, bool bRunTime = false)
		{
			GameObject[] stageAllObjs = GetStageAllObjs();
			List<GameObject> list = new List<GameObject>();
			List<StageObjGroupData> list2 = new List<StageObjGroupData>();
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			for (int i = 0; i < stageAllObjs.Length; i++)
			{
				if (!(stageAllObjs[i].name == "StageUpdate") && stageAllObjs[i].GetComponent<StageStartPoint>() != null)
				{
					num = stageAllObjs[i].transform.position.x;
					num2 = (num3 = num);
					num4 = (num5 = stageAllObjs[i].transform.position.y);
					break;
				}
			}
			for (int j = 0; j < stageAllObjs.Length; j++)
			{
				if (stageAllObjs[j].name == "StageUpdate")
				{
					continue;
				}
				switch (nMode)
				{
				case 1:
					if (stageAllObjs[j].GetComponent<StageSLBase>() != null && !stageAllObjs[j].GetComponent<StageSLBase>().IsMapDependObj())
					{
						continue;
					}
					break;
				case 2:
					if (stageAllObjs[j].GetComponent<StageSLBase>() == null || (stageAllObjs[j].GetComponent<StageSLBase>() != null && stageAllObjs[j].GetComponent<StageSLBase>().IsMapDependObj()))
					{
						continue;
					}
					break;
				}
				list.Add(stageAllObjs[j]);
				if (stageAllObjs[j].transform.position.x > num3)
				{
					num3 = stageAllObjs[j].transform.position.x;
				}
				if (stageAllObjs[j].transform.position.x < num2)
				{
					num2 = stageAllObjs[j].transform.position.x;
				}
				if (stageAllObjs[j].transform.position.y < num4)
				{
					num4 = stageAllObjs[j].transform.position.y;
				}
				if (stageAllObjs[j].transform.position.y > num5)
				{
					num5 = stageAllObjs[j].transform.position.y;
				}
			}
			float num6 = num - fClipWidth / 2f;
			float num7 = num + fClipWidth / 2f;
			if (num2 < num6)
			{
				num7 = num7 - (num6 - num2) - 1f;
				num6 = num2 - 1f;
			}
			while (list.Count > 0)
			{
				StageObjGroupData stageObjGroupData = new StageObjGroupData();
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].transform.GetComponent<StageSLBase>() == null)
					{
						float[] objMinx = StageResManager.GetObjMinx(list[k].transform);
						if (objMinx[1] - objMinx[0] > fClipWidth * 0.5f)
						{
							Debug.LogWarning("Find Large Width Stage Obj!! " + list[k].gameObject.name);
							StageObjGroupData stageObjGroupData2 = new StageObjGroupData();
							if (bRunTime)
							{
								StageEnemy component = list[k].GetComponent<StageEnemy>();
								Camera component2 = list[k].GetComponent<Camera>();
								Light component3 = list[k].GetComponent<Light>();
								StageSLBase component4 = list[k].GetComponent<StageSLBase>();
								if (component2 == null && component3 == null && list[k].layer != 2 && (component4 == null || component4.IsNeedClip()))
								{
									list[k].SetActive(false);
									if (component != null)
									{
										stageObjGroupData2.EnemyDatas.Add(component);
									}
									else
									{
										stageObjGroupData2.Datas.Add(list[k]);
									}
								}
							}
							else
							{
								stageObjGroupData2.Datas.Add(list[k]);
							}
							stageObjGroupData2.fClipMinx = objMinx[0];
							stageObjGroupData2.fClipMaxx = objMinx[1];
							list2.Add(stageObjGroupData2);
							list.RemoveAt(k);
							k--;
							continue;
						}
					}
					if ((!(list[k].transform.position.x > num6) || !(list[k].transform.position.x < num7)) && (!(list[k].transform.position.x < num6) || !(list[k].transform.position.x < num7)))
					{
						continue;
					}
					if (bRunTime)
					{
						StageEnemy component5 = list[k].GetComponent<StageEnemy>();
						Camera component6 = list[k].GetComponent<Camera>();
						Light component7 = list[k].GetComponent<Light>();
						StageSLBase component8 = list[k].GetComponent<StageSLBase>();
						if (component6 == null && component7 == null && list[k].layer != 2 && (component8 == null || component8.IsNeedClip()))
						{
							list[k].SetActive(false);
							if (component5 != null)
							{
								stageObjGroupData.EnemyDatas.Add(component5);
							}
							else
							{
								stageObjGroupData.Datas.Add(list[k]);
							}
						}
					}
					else
					{
						stageObjGroupData.Datas.Add(list[k]);
					}
					list.RemoveAt(k);
					k--;
				}
				if (stageObjGroupData.Datas.Count != 0 || stageObjGroupData.EnemyDatas.Count != 0)
				{
					stageObjGroupData.fClipMinx = num6;
					stageObjGroupData.fClipMaxx = num7;
					list2.Add(stageObjGroupData);
				}
				num6 += fClipWidth;
				num7 += fClipWidth;
			}
			return list2;
		}

		public static void LockAllAnimator(bool bLock)
		{
			for (int i = 0; i < EnemySets.Count; i++)
			{
				if (EnemySets[i].mEnemy != null && (bool)EnemySets[i].mEnemy.gameObject)
				{
					EnemySets[i].mEnemy.LockAnimator(bLock);
				}
			}
			for (int j = 0; j < runPlayerList.Count; j++)
			{
				if (runPlayerList[j] != null && (bool)runPlayerList[j].gameObject)
				{
					runPlayerList[j].LockAnimator(bLock);
				}
			}
			for (int k = 0; k < listAllEvents.Count; k++)
			{
				if (listAllEvents[k] != null && (bool)listAllEvents[k].gameObject)
				{
					listAllEvents[k].LockAnimator(bLock);
				}
			}
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate != null)
			{
				for (int l = 0; l < stageUpdate.runSOBList.Count; l++)
				{
					if (stageUpdate.runSOBList[l] != null)
					{
						stageUpdate.runSOBList[l].LockAnimator(bLock);
					}
				}
				if (bLock)
				{
					UnityArmatureComponent[] array = OrangeSceneManager.FindObjectsOfTypeCustom<UnityArmatureComponent>();
					foreach (UnityArmatureComponent unityArmatureComponent in array)
					{
						if (unityArmatureComponent.animation != null && unityArmatureComponent.animation.timeScale != 0f)
						{
							DragonSpeedData dragonSpeedData = new DragonSpeedData();
							dragonSpeedData.tDragonBone = unityArmatureComponent;
							dragonSpeedData.fSpeed = unityArmatureComponent.animation.timeScale;
							stageUpdate.listDragonSpeedData.Add(dragonSpeedData);
							unityArmatureComponent.animation.timeScale = 0f;
						}
					}
				}
				else if (stageUpdate.listDragonSpeedData.Count > 0)
				{
					foreach (DragonSpeedData listDragonSpeedDatum in stageUpdate.listDragonSpeedData)
					{
						UnityArmatureComponent tDragonBone = listDragonSpeedDatum.tDragonBone;
						if (tDragonBone != null && (bool)tDragonBone.gameObject)
						{
							listDragonSpeedDatum.tDragonBone.animation.timeScale = listDragonSpeedDatum.fSpeed;
						}
					}
					stageUpdate.listDragonSpeedData.Clear();
				}
			}
			MonoBehaviourSingleton<FxManager>.Instance.LockFx(bLock);
		}

		public static EnemyCtrlID GetEnemyCtrlIDByNetSerialID(string sNetSerialID)
		{
			for (int i = 0; i < EnemySets.Count; i++)
			{
				if (EnemySets[i].mEnemy.sNetSerialID == sNetSerialID)
				{
					return EnemySets[i];
				}
			}
			return null;
		}

		public static void GetObjBySCEID(int nSCEID, out EnemyControllerBase tECB, out OrangeCharacter tOC)
		{
			if (nSCEID == 0)
			{
				tECB = null;
				tOC = null;
				return;
			}
			for (int i = 0; i < EnemySets.Count; i++)
			{
				if (EnemySets[i].nSCEID == nSCEID)
				{
					tECB = EnemySets[i].mEnemy;
					tOC = null;
					return;
				}
			}
			for (int j = 0; j < runPlayerList.Count; j++)
			{
				StageObjParam component = runPlayerList[j].GetComponent<StageObjParam>();
				if (component != null && component.nSCEID == nSCEID)
				{
					tECB = null;
					tOC = runPlayerList[j];
					return;
				}
			}
			StageObjParam[] array = OrangeSceneManager.FindObjectsOfTypeCustom<StageObjParam>();
			foreach (StageObjParam stageObjParam in array)
			{
				if (stageObjParam.nSCEID == nSCEID)
				{
					tECB = stageObjParam.transform.GetComponent<EnemyControllerBase>();
					tOC = stageObjParam.transform.GetComponent<OrangeCharacter>();
					return;
				}
			}
			tECB = null;
			tOC = null;
		}

		public static void SetEnemyActBySCEID(int nSCEID, int nActID, string sParam, Callback tCB)
		{
			if (nSCEID == 0)
			{
				if (tCB != null)
				{
					tCB();
				}
				return;
			}
			bool flag = false;
			for (int i = 0; i < EnemySets.Count; i++)
			{
				if (EnemySets[i].nSCEID == nSCEID)
				{
					EnemySets[i].mEnemy.UpdateStatus(nActID, sParam, tCB);
					Animator animator = EnemySets[i].mEnemy._animator;
					if (EnemySets[i].mEnemy._animator == null)
					{
						animator = EnemySets[i].mEnemy.transform.GetComponentInChildren<Animator>();
					}
					animator.SetInteger("NowStatus", nActID);
					EnemySets[i].mEnemy.tAnimationCB = tCB;
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			StageObjParam[] array = OrangeSceneManager.FindObjectsOfTypeCustom<StageObjParam>();
			foreach (StageObjParam stageObjParam in array)
			{
				if (stageObjParam.nSCEID != nSCEID)
				{
					continue;
				}
				tTmpEnemyControllerBase = stageObjParam.transform.GetComponent<EnemyControllerBase>();
				if (tTmpEnemyControllerBase != null)
				{
					tTmpEnemyControllerBase.UpdateStatus(nActID, sParam, tCB);
					Animator animator2 = tTmpEnemyControllerBase._animator;
					if (tTmpEnemyControllerBase._animator == null)
					{
						animator2 = tTmpEnemyControllerBase.transform.GetComponentInChildren<Animator>();
					}
					animator2.SetInteger("NowStatus", nActID);
					tTmpEnemyControllerBase.tAnimationCB = tCB;
				}
			}
		}

		public static EnemyControllerBase StageSpawnEnemyByMob(MOB_TABLE tMOB_TABLE, string sNetSyncID, int nBitParam = 0, int nSCEID = 0, float fSetAimRange = 0f, bool bNeedAutoManage = true)
		{
			tTmpEnemyControllerBase = StageResManager.CreateEnemyByMob(tMOB_TABLE);
			if (tTmpEnemyControllerBase == null)
			{
				return null;
			}
			AddEnemyToEnemyCtrlID(tTmpEnemyControllerBase, sNetSyncID, nBitParam, nSCEID, fSetAimRange);
			if (bNeedAutoManage)
			{
				tTmpEnemyControllerBase.HurtActions += StageHurtCB;
			}
			return tTmpEnemyControllerBase;
		}

		public static void StageHurtCB(StageObjBase tSOB)
		{
			if ((int)tSOB.Hp <= 0)
			{
				if (tSOB.GetSOBType() == 2)
				{
					RemoveEnemy(tSOB as EnemyControllerBase);
				}
				tSOB.HurtActions -= StageHurtCB;
			}
		}

		public static void TowerStageHurtCB(StageObjBase tSOB)
		{
			if (tSOB.GetSOBType() == 2)
			{
				EnemyControllerBase enemyControllerBase = tSOB as EnemyControllerBase;
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.UpdateTowerBossInfo(enemyControllerBase.EnemyID, tSOB.Hp);
			}
			if ((int)tSOB.Hp <= 0)
			{
				if (tSOB.GetSOBType() == 2)
				{
					RemoveEnemy(tSOB as EnemyControllerBase);
				}
				tSOB.HurtActions -= TowerStageHurtCB;
			}
		}

		public static EnemyControllerBase StageSpawnEnemy(int nMobGroupID, string sNetSyncID, int nBitParam = 0, int nSCEID = 0, float fSetAimRange = 0f, float fSetAimRangeY = 0f, float fSetOffsetX = 0f, float fSetOffsetY = 0f)
		{
			tTmpEnemyControllerBase = StageResManager.CreateEnemy(nMobGroupID);
			if (tTmpEnemyControllerBase == null)
			{
				return null;
			}
			AddEnemyToEnemyCtrlID(tTmpEnemyControllerBase, sNetSyncID, nBitParam, nSCEID, fSetAimRange, fSetAimRangeY, fSetOffsetX, fSetOffsetY);
			if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentStageClear && ManagedSingleton<PlayerNetManager>.Instance.mmapTowerBossInfoMap.ContainKey(ManagedSingleton<StageHelper>.Instance.nLastStageID))
			{
				List<NetTowerBossInfo> list = ManagedSingleton<PlayerNetManager>.Instance.mmapTowerBossInfoMap[ManagedSingleton<StageHelper>.Instance.nLastStageID];
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].TowerBossID == tTmpEnemyControllerBase.EnemyData.n_ID)
					{
						tTmpEnemyControllerBase.HurtActions += TowerStageHurtCB;
						tTmpEnemyControllerBase.Hp = list[i].DeductedHP;
						if ((int)tTmpEnemyControllerBase.Hp <= 0)
						{
							tTmpEnemyControllerBase.UpdateHurtAction();
							tTmpEnemyControllerBase.BackToPool();
							tTmpEnemyControllerBase = null;
						}
						break;
					}
				}
			}
			return tTmpEnemyControllerBase;
		}

		private static void AddEnemyToEnemyCtrlID(EnemyControllerBase tEnemy, string sNetSyncID, int nBitParam = 0, int nSCEID = 0, float fSetAimRange = 0f, float fSetAimRangeY = 0f, float fSetOffsetX = 0f, float fSetOffsetY = 0f)
		{
			EnemyCtrlID objFromPool = StageResManager.GetObjFromPool<EnemyCtrlID>();
			objFromPool.mEnemy = tEnemy;
			tEnemy.sNetSerialID = sNetSyncID;
			objFromPool.nEnemyBitParam = nBitParam;
			objFromPool.fSetAimRange = fSetAimRange;
			objFromPool.fSetAimRangeY = fSetAimRangeY;
			objFromPool.fSetOffsetX = fSetOffsetX;
			objFromPool.fSetOffsetY = fSetOffsetY;
			tEnemy.gameObject.AddOrGetComponent<StageObjParam>().nSCEID = nSCEID;
			objFromPool.nSCEID = nSCEID;
			objFromPool.mEnemy.SetAimRange(fSetAimRange, fSetAimRangeY, fSetOffsetX, fSetOffsetY);
			objFromPool.mEnemy.DisableMoveFall = (nBitParam & 4) != 0;
			objFromPool.mEnemy.bDeadShock = (nBitParam & 8) == 0;
			EnemySets.Add(objFromPool);
			if (ManagedSingleton<OrangeTableHelper>.Instance.IsBossSP(tEnemy.EnemyData) && !gbRegisterPvpPlayer)
			{
				BattleInfoUI.Instance.IsBossAppear = true;
			}
		}

		public static void RemoveEnemy(EnemyControllerBase tRemove)
		{
			int count = EnemySets.Count;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				if (EnemySets[i].mEnemy.GetInstanceID() == tRemove.GetInstanceID())
				{
					flag = ManagedSingleton<OrangeTableHelper>.Instance.IsBossSP(EnemySets[i].mEnemy.EnemyData);
					StageResManager.RemoveEnemy(tRemove);
					StageResManager.BackObjToPool(EnemySets[i]);
					EnemySets.RemoveAt(i);
					break;
				}
			}
			if (!(BattleInfoUI.Instance != null && BattleInfoUI.Instance.IsBossAppear && flag))
			{
				return;
			}
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate != null)
			{
				if (stageUpdate.tCheckIsBossAppearCoroutine != null)
				{
					stageUpdate.StopCoroutine(stageUpdate.tCheckIsBossAppearCoroutine);
				}
				stageUpdate.tCheckIsBossAppearCoroutine = stageUpdate.StartCoroutine(stageUpdate.CheckIsBossAppear());
			}
		}

		private IEnumerator CheckIsBossAppear()
		{
			float fWaitTime = 6f;
			while (fWaitTime > 0f)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				fWaitTime -= Time.deltaTime;
				while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			while (nRunStageCtrlCount > 0)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (BattleInfoUI.Instance != null && BattleInfoUI.Instance.IsBossAppear)
			{
				int count = EnemySets.Count;
				BattleInfoUI.Instance.IsBossAppear = false;
				for (int i = 0; i < count; i++)
				{
					if (ManagedSingleton<OrangeTableHelper>.Instance.IsBossSP(EnemySets[i].mEnemy.EnemyData) && !gbRegisterPvpPlayer)
					{
						BattleInfoUI.Instance.IsBossAppear = true;
						break;
					}
				}
			}
			tCheckIsBossAppearCoroutine = null;
		}

		public static void CheckAndRemoveEnemy()
		{
			for (int num = EnemySets.Count - 1; num >= 0; num--)
			{
				if ((int)EnemySets[num].mEnemy.Hp == 0)
				{
					StageResManager.RemoveEnemy(EnemySets[num].mEnemy);
					StageResManager.BackObjToPool(EnemySets[num]);
					EnemySets.RemoveAt(num);
				}
			}
		}

		public void PadInitCB(object p_param)
		{
			VirtualButton virtualButton = null;
			VirtualPadSystem virtualPadSystem = p_param as VirtualPadSystem;
			for (int i = 0; i < 5; i++)
			{
				bool active = (nBtnStatus & (1 << i)) != 0;
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
		}

		public void RegisterStageObjBase(StageObjBase tSOB)
		{
			if (!runSOBList.Contains(tSOB))
			{
				runSOBList.Add(tSOB);
			}
		}

		public void UnRegisterStageObjBase(StageObjBase tSOB)
		{
			if (runSOBList.Contains(tSOB))
			{
				runSOBList.Remove(tSOB);
			}
		}

		public void RegisterSyncBullet(SyncBullet tSB)
		{
			if (!listSyncBullet.Contains(tSB))
			{
				listSyncBullet.Add(tSB);
			}
		}

		public void UnRegisterSyncBullet(SyncBullet tSB)
		{
			if (listSyncBullet.Contains(tSB))
			{
				listSyncBullet.Remove(tSB);
			}
		}

		public void RegisterPlayer(OrangeCharacter tPlayer)
		{
			if (!runPlayerList.Contains(tPlayer))
			{
				runPlayerList.Add(tPlayer);
			}
			if (!tPlayer.gameObject.GetComponent<LockRangeObj>())
			{
				tPlayer.gameObject.AddComponent<LockRangeObj>().Init();
			}
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == tPlayer.sPlayerID)
			{
				MainOC = tPlayer;
				nReBornCount++;
				if (!bMainPlayerOK)
				{
					bMainPlayerOK = true;
					MonoBehaviourSingleton<InputManager>.Instance.LoadVirtualPad(4, false, PadInitCB);
					if (bIsHost && BattleInfoUI.Instance != null)
					{
						BattleInfoUI.Instance.ShowStageCountDownTime();
					}
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect)
					{
						EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
						stageCameraFocus.bLock = true;
						stageCameraFocus.bRightNow = true;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
					}
				}
			}
			if ((int)tPlayer.Hp > 0)
			{
				StageResManager.CreateHpBarToPlayer(tPlayer);
			}
			if (tPlayer.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer)
			{
				if (!tPlayer.IsLocalPlayer)
				{
					if (!tPlayer.bIsNpcCpy)
					{
						BattleInfoUI.Instance.AddOrangeCharacter(tPlayer);
					}
				}
				else
				{
					BattleInfoUI.Instance.AddOrangeCharacter(tPlayer);
				}
			}
			tPlayer.gameObject.SetActive(true);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_PLAYER_LIST);
			if (mAllCameras.Count > 0)
			{
				GetRunClipGroups(mAllCameras[0].transform.position);
			}
			if (BattleInfoUI.Instance.NowStageTable.n_MAIN == 90000 && BattleInfoUI.Instance.NowStageTable.n_TYPE == 1000)
			{
				tPlayer.selfBuffManager.AddBuff(OrangeConst.REBOOT_BUFFID, 0, 0, 0, tPlayer.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, "", 7);
			}
		}

		public int GetMainPlayerHpPercent()
		{
			if (MainOC.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				return (int)MainOC.Hp * 100 / (int)MainOC.MaxHp;
			}
			for (int i = 0; i < runPlayerList.Count; i++)
			{
				if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == runPlayerList[i].sPlayerID)
				{
					MainOC = runPlayerList[i];
					return (int)runPlayerList[i].Hp * 100 / (int)runPlayerList[i].MaxHp;
				}
			}
			return 100;
		}

		public void UnRegisterPlayer(OrangeCharacter tPlayer, bool bNeedRemove)
		{
			if (bNeedRemove && runPlayerList.Contains(tPlayer))
			{
				for (int i = 0; i < runPlayerList.Count; i++)
				{
					if (runPlayerList[i].GetInstanceID() == tPlayer.GetInstanceID())
					{
						BattleInfoUI.Instance.RemoveOrangeCharacter(runPlayerList[i]);
						runPlayerList.RemoveAt(i);
						break;
					}
				}
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_UPDATE_PLAYER_LIST);
			PlayerRebornTask(tPlayer, bNeedRemove);
		}

		public void PlayerRebornTask(OrangeCharacter tPlayer, bool bNeedRemove)
		{
			if (!(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == tPlayer.sPlayerID) || bNeedRemove)
			{
				return;
			}
			if (!IsCanGamePause)
			{
				if (tWaitPauseCoroutine != null)
				{
					StopCoroutine(tWaitPauseCoroutine);
				}
				tWaitPauseCoroutine = StartCoroutine(WaitPauseCoroutine());
				return;
			}
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowContinueSelect();
			}
			if (!gbIsNetGame && !IsEnd)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
			}
		}

		public void StopPlayerRebornTask(OrangeCharacter tPlayer, bool bNeedRemove)
		{
			if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == tPlayer.sPlayerID && !bNeedRemove)
			{
				if (BattleInfoUI.Instance != null)
				{
					BattleInfoUI.Instance.CloseContinueUI();
				}
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
				if (tWaitPauseCoroutine != null)
				{
					StopCoroutine(tWaitPauseCoroutine);
					tWaitPauseCoroutine = null;
				}
			}
		}

		private IEnumerator WaitPauseCoroutine()
		{
			while (!IsCanGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (BattleInfoUI.Instance != null)
			{
				BattleInfoUI.Instance.ShowContinueSelect();
			}
			if (!gbIsNetGame && !IsEnd)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
			}
			tWaitPauseCoroutine = null;
		}

		public static OrangeCharacter GetPlayerIntersectC2B(BoxCollider2D tB2d)
		{
			if (tB2d == null)
			{
				return null;
			}
			Bounds tAB = tB2d.bounds;
			foreach (OrangeCharacter runPlayer in runPlayerList)
			{
				if ((int)runPlayer.Hp > 0)
				{
					Bounds tBB = runPlayer.Controller.Collider2D.bounds;
					if (StageResManager.CheckBoundsIntersectNoZEffect(ref tAB, ref tBB))
					{
						return runPlayer;
					}
				}
			}
			return null;
		}

		public static OrangeCharacter GetNearestPlayerByVintPos(VInt3 tPos, long nMaxRange = long.MaxValue, bool bCheckHP = true)
		{
			long num = long.MaxValue;
			OrangeCharacter result = null;
			if (nMaxRange == 0L)
			{
				nMaxRange = long.MaxValue;
			}
			foreach (OrangeCharacter runPlayer in runPlayerList)
			{
				if ((!bCheckHP || (int)runPlayer.Hp > 0) && runPlayer.AllowAutoAim)
				{
					long num2 = 9999L;
					num2 = ((!runPlayer.UsingVehicle || !(runPlayer.refRideBaseObj != null)) ? (runPlayer.Controller.LogicPosition - tPos).sqrMagnitudeLong : (runPlayer.refRideBaseObj.Controller.LogicPosition - tPos).sqrMagnitudeLong);
					if (num2 < num && num2 < nMaxRange && (int)runPlayer.Hp > 0)
					{
						num = num2;
						result = runPlayer;
					}
				}
			}
			return result;
		}

		public static bool IsAllPlayerDead(bool bUseCheckHP = false)
		{
			bool result = true;
			bool flag = true;
			for (int i = 0; i < runPlayerList.Count; i++)
			{
				if (gbIsNetGame)
				{
					foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
					{
						if (item.PlayerId == runPlayerList[i].sPlayerID)
						{
							flag = item.bInGame && !item.bInPause;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
				if (bUseCheckHP)
				{
					if (!runPlayerList[i].bIsNpcCpy && (int)runPlayerList[i].Hp > 0)
					{
						result = false;
					}
				}
				else if (!runPlayerList[i].bIsNpcCpy && !runPlayerList[i].IsDead())
				{
					result = false;
				}
			}
			return result;
		}

		public static OrangeCharacter GetPlayerByID(string playerid)
		{
			for (int i = 0; i < runPlayerList.Count; i++)
			{
				if (runPlayerList[i].sPlayerID == playerid)
				{
					return runPlayerList[i];
				}
			}
			return null;
		}

		public static UnityEngine.Transform GetMainPlayerTrans()
		{
			if (MainOC != null && MainOC.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				return MainOC.gameObject.transform;
			}
			for (int i = 0; i < runPlayerList.Count; i++)
			{
				if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == runPlayerList[i].sPlayerID)
				{
					MainOC = runPlayerList[i];
					return runPlayerList[i].gameObject.transform;
				}
			}
			return null;
		}

		public static UnityEngine.Transform GetHostPlayerTrans()
		{
			if (gbIsNetGame)
			{
				for (int i = 0; i < runPlayerList.Count; i++)
				{
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.sHostPlayerID == runPlayerList[i].sPlayerID)
					{
						return runPlayerList[i].gameObject.transform;
					}
				}
				return null;
			}
			return GetMainPlayerTrans();
		}

		public static OrangeCharacter GetMainPlayerOC()
		{
			if (MainOC != null && MainOC.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				return MainOC;
			}
			for (int i = 0; i < runPlayerList.Count; i++)
			{
				if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == runPlayerList[i].sPlayerID)
				{
					MainOC = runPlayerList[i];
					return runPlayerList[i];
				}
			}
			return null;
		}

		public static void SetMainPlayerOCNotLocalPlayer()
		{
			OrangeCharacter mainPlayerOC = GetMainPlayerOC();
			if (mainPlayerOC != null)
			{
				mainPlayerOC.IsLocalPlayer = false;
			}
		}

		public static bool IsRewardUI()
		{
			return bIsRewardUI;
		}

		public static void go2home()
		{
			bIsRewardUI = false;
		}

		public static void setPVPResultUIFlag()
		{
			bIsRewardUI = true;
		}

		public void RegisterBullet(BulletBase tBB)
		{
			if (!listBullet.Contains(tBB))
			{
				listBullet.Add(tBB);
			}
		}

		public void UnRegisterBullet(BulletBase tBB)
		{
			if (tBB.refPSShoter != null)
			{
				if (tBB.refPBMShoter != null)
				{
					if (tBB.refPBMShoter.SOB != null)
					{
						if (tBB.refPBMShoter.SOB.GetSOBType() == 2 && bIsHost)
						{
							tBB.refPSShoter.BulletEndTrigger(tBB, ref tBB.refPBMShoter, tBB.CreateBulletDetail);
						}
						else if (tBB.refPBMShoter.SOB.sNetSerialID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
						{
							tBB.refPSShoter.BulletEndTrigger(tBB, ref tBB.refPBMShoter, tBB.CreateBulletDetail);
						}
					}
				}
				else
				{
					Debug.LogError("Bullet refPBMShoter is null. " + tBB.itemName);
				}
			}
			if (listBullet.Contains(tBB))
			{
				listBullet.Remove(tBB);
			}
			for (int num = EnemySets.Count - 1; num >= 0; num--)
			{
				EnemySets[num].mEnemy.CheckDmgStack(tBB.nRecordID, tBB.nNetID);
			}
			for (int num2 = runPlayerList.Count - 1; num2 >= 0; num2--)
			{
				runPlayerList[num2].CheckDmgStack(tBB.nRecordID, tBB.nNetID);
			}
		}

		public static int NowHasBullet(int nRecordID, int nNetID)
		{
			for (int num = listBullet.Count - 1; num >= 0; num--)
			{
				if (listBullet[num].nRecordID == nRecordID && listBullet[num].nNetID == nNetID)
				{
					return num;
				}
			}
			return -1;
		}

		public static void BackAllBullet()
		{
			for (int num = listBullet.Count - 1; num >= 0; num--)
			{
				listBullet[num].BackToPool();
			}
		}

		public static void BackAllBulletByDisconnect()
		{
			for (int num = listBullet.Count - 1; num >= 0; num--)
			{
				listBullet[num].BackToPoolByDisconnet();
			}
		}

		public void UpdateHost(bool bHost)
		{
			bIsHost = bHost;
		}

		public void RegisterStageParam(EventManager.RegisterStageParam tRegisterStageParam)
		{
			if (tRegisterStageParam.nMode == 0)
			{
				listStageSecert.Add((sbyte)tRegisterStageParam.nStageSecert);
			}
		}

		public static void RegisterEndRemoveObj(GameObject tObj)
		{
			if (!EndNeedRemoveObjs.Contains(tObj))
			{
				EndNeedRemoveObjs.Add(tObj);
			}
		}

		public static void RemoveEndRemoveObj(GameObject tObj)
		{
			if (EndNeedRemoveObjs.Contains(tObj))
			{
				EndNeedRemoveObjs.Remove(tObj);
			}
		}

		public void AddStageQuest(int nID, int nType, int[] nParams)
		{
			for (int num = listStageQuests.Count - 1; num >= 0; num--)
			{
				if (listStageQuests[num].nID == nID)
				{
					return;
				}
			}
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			switch (nType)
			{
			case 2:
				Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
				Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
				break;
			case 4:
			{
				for (int i = 0; i < nParams.Length - 1; i += 3)
				{
					EnemyControllerBase tECB;
					OrangeCharacter tOC;
					GetObjBySCEID(nParams[i * 3], out tECB, out tOC);
					if (tECB != null)
					{
						if ((float)(int)tECB.Hp / (float)(int)tECB.MaxHp * 100f <= (float)nParams[i * 3 + 1])
						{
							return;
						}
					}
					else if (!(tOC != null) || (float)(int)tOC.Hp / (float)(int)tOC.MaxHp * 100f <= (float)nParams[i * 3 + 1])
					{
						return;
					}
				}
				stageEventCall.nID = nParams[nParams.Length - 1];
				stageEventCall.tTransform = GetHostPlayerTrans();
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
				return;
			}
			case 5:
				nParams[2] = BattleInfoUI.Instance.nGetCampaignScore;
				if (nParams[1] != 0)
				{
					BattleInfoUI.Instance.ShowKillScoreUI(10000, nParams[1] - 1, delegate(ScoreUIBase tUI)
					{
						tUI.SetNParam0(nParams[2]);
					});
				}
				break;
			}
			StageQuest stageQuest = new StageQuest();
			stageQuest.nID = nID;
			stageQuest.nType = nType;
			stageQuest.nParams = nParams;
			listStageQuests.Add(stageQuest);
		}

		public void RemoveStageQuest(int nID)
		{
			for (int num = listStageQuests.Count - 1; num >= 0; num--)
			{
				StageQuest stageQuest = listStageQuests[num];
				if (stageQuest.nID == nID)
				{
					int num2 = 0;
					int num3 = 0;
					switch (stageQuest.nType)
					{
					case 2:
						Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
						break;
					case 5:
					{
						for (int j = 3; j < stageQuest.nParams.Length; j++)
						{
							if (BattleInfoUI.Instance.nGetCampaignScore < stageQuest.nParams[j] + stageQuest.nParams[2])
							{
								continue;
							}
							if (((uint)stageQuest.nParams[0] & (true ? 1u : 0u)) != 0)
							{
								if (stageQuest.nParams[j] > num2)
								{
									num2 = stageQuest.nParams[j];
									num3 = stageQuest.nParams[j + 1];
								}
							}
							else
							{
								EventManager.StageEventCall stageEventCall2 = new EventManager.StageEventCall();
								stageEventCall2.nID = stageQuest.nParams[j + 1];
								stageEventCall2.tTransform = GetHostPlayerTrans();
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall2);
							}
						}
						if (((uint)stageQuest.nParams[0] & (true ? 1u : 0u)) != 0 && num3 != 0)
						{
							EventManager.StageEventCall stageEventCall3 = new EventManager.StageEventCall();
							stageEventCall3.nID = num3;
							stageEventCall3.tTransform = GetHostPlayerTrans();
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall3);
						}
						BattleInfoUI.Instance.RemoveKillScoreUI(stageQuest.nParams[1] - 1);
						break;
					}
					case 6:
					{
						int num4;
						for (num4 = 0; num4 < listStageQuests[num].nParams.Length - 1; num4 += 3)
						{
							listStageQuests[num].bIsEnd = true;
							for (int i = 1; i <= listStageQuests[num].nParams[num4]; i++)
							{
								if (listStageQuests[num].nParams[num4 + i * 3 + 1] > listStageQuests[num].nParams[num4 + i * 3 + 2])
								{
									listStageQuests[num].bIsEnd = false;
									break;
								}
							}
							if (listStageQuests[num].bIsEnd)
							{
								EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
								stageEventCall.nID = stageQuest.nParams[num4 + 1];
								stageEventCall.tTransform = GetHostPlayerTrans();
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
								break;
							}
							num4 += listStageQuests[num].nParams[num4] * 3;
						}
						break;
					}
					}
					listStageQuests.RemoveAt(num);
					break;
				}
			}
			for (int num5 = listStageQuests.Count - 1; num5 >= 0; num5--)
			{
				int nType = listStageQuests[num5].nType;
				if (nType == 2)
				{
					Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
					Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
				}
			}
		}

		public void CallOnByID(EventManager.StageEventCall tStageEventCall)
		{
			int nID = tStageEventCall.nID;
			if (nID != 0)
			{
				TriggerStageQuest(null, null, null, nID);
			}
		}

		public void TriggerStageQuest(SKILL_TABLE pData, PerBuffManager refPBMShoter, PerBuffManager targetBuffManager, int nEventID = 0)
		{
			for (int num = listStageQuests.Count - 1; num >= 0; num--)
			{
				if (listStageQuests[num].bIsEnd)
				{
					continue;
				}
				switch (listStageQuests[num].nType)
				{
				case 1:
				{
					if (targetBuffManager == null || targetBuffManager.SOB.GetSOBType() != 2 || (int)targetBuffManager.SOB.Hp > 0)
					{
						continue;
					}
					tTmpEnemyControllerBase = targetBuffManager.SOB as EnemyControllerBase;
					for (int m = 0; m < listStageQuests[num].nParams.Length - 1; m += 3)
					{
						if (listStageQuests[num].nParams[m] == tTmpEnemyControllerBase.EnemyData.n_GROUP)
						{
							listStageQuests[num].nParams[m + 2]++;
						}
					}
					listStageQuests[num].bIsEnd = true;
					for (int n = 0; n < listStageQuests[num].nParams.Length - 1; n += 3)
					{
						if (listStageQuests[num].nParams[n + 1] > listStageQuests[num].nParams[n + 2])
						{
							listStageQuests[num].bIsEnd = false;
							break;
						}
					}
					break;
				}
				case 2:
				{
					for (int j = 0; j < listStageQuests[num].nParams.Length - 1; j += 3)
					{
						if (nEventID != 0 && listStageQuests[num].nParams[j] == nEventID)
						{
							listStageQuests[num].nParams[j + 2]++;
						}
					}
					listStageQuests[num].bIsEnd = true;
					for (int k = 0; k < listStageQuests[num].nParams.Length - 1; k += 3)
					{
						if (listStageQuests[num].nParams[k + 1] > listStageQuests[num].nParams[k + 2])
						{
							listStageQuests[num].bIsEnd = false;
							break;
						}
					}
					break;
				}
				case 3:
				{
					listStageQuests[num].bIsEnd = true;
					for (int l = 0; l < listStageQuests[num].nParams.Length - 1; l += 3)
					{
						if (!refPBMShoter.CheckHasEffect(listStageQuests[num].nParams[l], listStageQuests[num].nParams[l + 1]))
						{
							listStageQuests[num].bIsEnd = false;
							break;
						}
					}
					break;
				}
				case 6:
				{
					if (targetBuffManager == null || targetBuffManager.SOB.GetSOBType() != 2 || (int)targetBuffManager.SOB.Hp > 0)
					{
						continue;
					}
					tTmpEnemyControllerBase = targetBuffManager.SOB as EnemyControllerBase;
					for (int i = 0; i < listStageQuests[num].nParams.Length - 1; i += 3)
					{
						if (listStageQuests[num].nParams[i] == tTmpEnemyControllerBase.EnemyData.n_GROUP)
						{
							listStageQuests[num].nParams[i + 2]++;
						}
					}
					break;
				}
				}
				if (listStageQuests[num].bIsEnd)
				{
					EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
					stageEventCall.nID = listStageQuests[num].nParams[listStageQuests[num].nParams.Length - 1];
					stageEventCall.tTransform = GetHostPlayerTrans();
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
					RemoveStageQuest(listStageQuests[num].nID);
				}
			}
		}

		public void AddStageDataPoint(StageDataPoint tStageDataPoint)
		{
			if (!listStageDataPoint.Contains(tStageDataPoint))
			{
				listStageDataPoint.Add(tStageDataPoint);
			}
		}

		public List<StageDataPoint> GetAllStageDataPoints()
		{
			return listStageDataPoint;
		}

		public static void SlowStage(float fSlowFactor = 0.1f, float fTimeUnSlow = 0.3f)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_TIMESCALE_CHANGE, true);
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (!(stageUpdate == null))
			{
				if (stageUpdate.listSaveAniSpeed.Count > 0)
				{
					UnSlowStage();
					Debug.LogWarning("UnSlowStage SlowStage");
				}
				Debug.Log("SlowStage");
				Animator[] array = OrangeSceneManager.FindObjectsOfTypeCustom<Animator>();
				for (int i = 0; i < array.Length; i++)
				{
					AniSpeedData aniSpeedData = new AniSpeedData();
					aniSpeedData.tAniPlayer = array[i];
					aniSpeedData.fSpeed = aniSpeedData.tAniPlayer.speed;
					aniSpeedData.fSetSeed = aniSpeedData.fSpeed * fSlowFactor;
					stageUpdate.listSaveAniSpeed.Add(aniSpeedData);
					aniSpeedData.tAniPlayer.speed = aniSpeedData.fSetSeed;
				}
				Time.timeScale = fSlowFactor;
				if (stageUpdate.tUnSlowStageCoroutine != null)
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.StopCoroutine(stageUpdate.tUnSlowStageCoroutine);
				}
				stageUpdate.tUnSlowStageCoroutine = MonoBehaviourSingleton<OrangeGameManager>.Instance.StartCoroutine(MonoBehaviourSingleton<StageSyncManager>.Instance.UnSlowStageCoroutine(fTimeUnSlow));
			}
		}

		public static void UnSlowStage()
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_TIMESCALE_CHANGE, false);
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate == null)
			{
				return;
			}
			Debug.Log("UnSlowStage");
			for (int i = 0; i < stageUpdate.listSaveAniSpeed.Count; i++)
			{
				if (stageUpdate.listSaveAniSpeed[i].tAniPlayer != null && stageUpdate.listSaveAniSpeed[i].tAniPlayer.speed == stageUpdate.listSaveAniSpeed[i].fSetSeed)
				{
					stageUpdate.listSaveAniSpeed[i].tAniPlayer.speed = stageUpdate.listSaveAniSpeed[i].fSpeed;
				}
			}
			stageUpdate.listSaveAniSpeed.Clear();
			if (Time.timeScale != 1f)
			{
				Time.timeScale = 1f;
			}
			if (stageUpdate.tUnSlowStageCoroutine != null)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.StopCoroutine(stageUpdate.tUnSlowStageCoroutine);
			}
			stageUpdate.tUnSlowStageCoroutine = null;
		}

		public int GetNowStageStart()
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp || ManagedSingleton<StageHelper>.Instance.nLastStageID == 0)
			{
				return 0;
			}
			STAGE_TABLE sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID];
			int[] array = new int[3] { sTAGE_TABLE.n_CLEAR1, sTAGE_TABLE.n_CLEAR2, sTAGE_TABLE.n_CLEAR3 };
			int[] array2 = new int[3] { sTAGE_TABLE.n_CLEAR_VALUE1, sTAGE_TABLE.n_CLEAR_VALUE2, sTAGE_TABLE.n_CLEAR_VALUE3 };
			int num = 0;
			for (int i = 0; i < 3; i++)
			{
				switch (array[i])
				{
				case 1:
					num |= 1 << i;
					break;
				case 2:
					if (fStageUseTime < (float)array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 3:
					if (GetMainPlayerHpPercent() > array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 4:
					if (nReBornCount < array2[i] + 1)
					{
						num |= 1 << i;
					}
					break;
				case 5:
					if (BattleInfoUI.Instance.nGetCampaignScore > array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 6:
					if (BattleInfoUI.Instance.GetCountScore(2) > 0)
					{
						num |= 1 << i;
					}
					break;
				case 7:
					if (BattleInfoUI.Instance.GetCountScore(3) > 0)
					{
						num |= 1 << i;
					}
					break;
				case 8:
					if (BattleInfoUI.Instance.nGetnGetItemValue >= (float)array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 9:
					if (BattleInfoUI.Instance.nGetnBattleScoreValue >= (float)array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 10:
					if (BattleInfoUI.Instance.fCountDownTimerValue < (float)array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 11:
					if (BattleInfoUI.Instance.GetCountScore(0) >= array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 12:
					if (BattleInfoUI.Instance.GetCountScore(1) >= array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 13:
					if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower() >= array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 14:
					if (BattleInfoUI.Instance.GetCountScore(1) >= array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 15:
				{
					OrangeCharacter mainPlayerOC = GetMainPlayerOC();
					if (mainPlayerOC != null && mainPlayerOC.selfBuffManager.sBuffStatus.nLaskBlackStack >= array2[i])
					{
						num |= 1 << i;
					}
					break;
				}
				case 16:
					if (BattleInfoUI.Instance.GetCountScore(1) >= array2[i])
					{
						num |= 1 << i;
					}
					break;
				case 17:
					if (BattleInfoUI.Instance.GetCountScore(1) <= array2[i])
					{
						num |= 1 << i;
					}
					break;
				}
			}
			return num;
		}

		public void ShowStageRewardUI()
		{
			if (bIsEnd)
			{
				Debug.LogError("Game Is End!!!");
				return;
			}
			bIsEnd = true;
			if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
			{
				int n_TYPE = BattleInfoUI.Instance.NowStageTable.n_TYPE;
				if (n_TYPE == 10)
				{
					if (StageMode == StageMode.Contribute)
					{
						ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Win;
					}
				}
				else
				{
					ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Win;
				}
			}
			for (int num = listAllEvent.Count - 1; num >= 0; num--)
			{
				listAllEvent[num].StopEvent();
			}
			gbAddStageUseTime = false;
			BattleInfoUI.Instance.SwitchOptionBtn(false);
			StartCoroutine(ShowStageRewardUICoroutine());
		}

		public IEnumerator ShowStageRewardUICoroutine()
		{
			if (gbIsNetGame)
			{
				if (bIsHost)
				{
					foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
					{
						if (item.PlayerId != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
						{
							string smsg = item.PlayerId + "," + 1 + "," + fStageUseTime + ",1";
							SyncStageObj(3, 7, smsg);
						}
					}
				}
				else
				{
					bWaitNetStageUseTime = true;
					float fLimitWaitTime = 5f;
					while (bWaitNetStageUseTime && fLimitWaitTime > 0f)
					{
						yield return CoroutineDefine._waitForEndOfFrame;
						fLimitWaitTime -= Time.deltaTime;
					}
				}
			}
			OrangeCharacter oc = GetMainPlayerOC();
			int n_TYPE;
			while (oc == null || oc.IsDead())
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				bool flag = false;
				n_TYPE = BattleInfoUI.Instance.NowStageTable.n_TYPE;
				if ((uint)(n_TYPE - 9) <= 1u)
				{
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
			n_TYPE = BattleInfoUI.Instance.NowStageTable.n_TYPE;
			if ((uint)(n_TYPE - 9) <= 1u)
			{
				for (int num = EnemySets.Count - 1; num >= 0; num--)
				{
					EnemySets[num].mEnemy.Activate = false;
				}
				for (int num2 = runPlayerList.Count - 1; num2 >= 0; num2--)
				{
					if ((int)runPlayerList[num2].Hp > 0)
					{
						runPlayerList[num2].StopPlayer();
					}
				}
			}
			else
			{
				for (int num3 = EnemySets.Count - 1; num3 >= 0; num3--)
				{
					EnemySets[num3].mEnemy.Hp = 0;
					EnemySets[num3].mEnemy.SoundSource.f_vol = 0f;
					EnemySets[num3].mEnemy.Hurt(new HurtPassParam());
				}
				for (int num4 = runPlayerList.Count - 1; num4 >= 0; num4--)
				{
					runPlayerList[num4].gameObject.layer = 0;
					if (!runPlayerList[num4].LockInput)
					{
						runPlayerList[num4].StopPlayer();
						runPlayerList[num4].EventLockInputing = true;
						runPlayerList[num4].selfBuffManager.ClearBuff();
					}
				}
			}
			CheckCoopExtraRewardCompleted(ref ManagedSingleton<StageHelper>.Instance.ListAchievedMissionID);
			BackAllBullet();
			fWaitSlowTime = Time.realtimeSinceStartup;
			if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
			{
				if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
				{
					if (StageMode == StageMode.Contribute)
					{
						ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Win;
					}
					lastStageEndRes = null;
					bIsWaitingStageEndRes = true;
					StartCoroutine(WaitStageEndRes());
					StageEndReq stageEndReq = new StageEndReq();
					stageEndReq.StageID = ManagedSingleton<StageHelper>.Instance.nLastStageID;
					stageEndReq.Star = (sbyte)GetNowStageStart();
					stageEndReq.Result = (sbyte)ManagedSingleton<StageHelper>.Instance.eLastStageResult;
					stageEndReq.StageSecretList = listStageSecert;
					stageEndReq.Score = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower();
					stageEndReq.Power = ManagedSingleton<StageHelper>.Instance.nLastOCPower;
					stageEndReq.Duration = (int)Mathf.Floor(fStageUseTime * 1000f);
					stageEndReq.AchievedMissionID = ManagedSingleton<StageHelper>.Instance.ListAchievedMissionID;
					stageEndReq.KillCount = (short)MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerKillEnemyNum();
					MonoBehaviourSingleton<OrangeGameManager>.Instance.StageEndReq(stageEndReq, OnStageEndRes);
				}
			}
			else
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_STAGEREWARD", delegate(StageRewardUI ui)
				{
					ui.refStageUpdate = this;
				});
			}
		}

		private void CheckCoopExtraRewardCompleted(ref List<int> achievedMissionIDList)
		{
			if (BattleInfoUI.Instance.NowStageTable.n_TYPE != 5)
			{
				return;
			}
			int count = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count;
			List<MISSION_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == 5 && x.n_SUB_TYPE == BattleInfoUI.Instance.NowStageTable.n_ID).ToList();
			List<MISSION_TABLE> collection = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.Values.Where((MISSION_TABLE x) => x.n_TYPE == 5 && x.n_SUB_TYPE == 0).ToList();
			list.AddRange(collection);
			if (list.Count <= 0)
			{
				return;
			}
			foreach (MISSION_TABLE item in list)
			{
				if (item.n_CONDITION == 2006 && count >= item.n_CONDITION_X && ((item.n_CONDITION_Y == 1 && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode) || item.n_CONDITION_Y == 0))
				{
					achievedMissionIDList.Add(item.n_ID);
					Debug.Log("Coop mission completed, ID = " + item.n_ID);
				}
			}
		}

		private void OnStageEndRes(object p_param)
		{
			lastStageEndRes = p_param as StageEndRes;
			bIsWaitingStageEndRes = false;
		}

		private IEnumerator WaitStageEndRes()
		{
			Debug.Log("Wait StageEndRes Start");
			bool setWinPose = false;
			if (BattleInfoUI.Instance.NowStageTable.n_TYPE == 10 && StageMode == StageMode.Normal)
			{
				setWinPose = true;
			}
			while (bIsWaitingStageEndRes)
			{
				if (setWinPose)
				{
					OrangeBattleUtility.CurrentCharacter.SetWinPose(null);
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			Debug.Log("Wait StageEndRes End");
			StartCoroutine(VictoryCoroutine(lastStageEndRes));
		}

		private IEnumerator VictoryCoroutine(object p_param)
		{
			Debug.Log("VictoryCoroutine Step 0");
			while (nRunStageCtrlCount > 0)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			Debug.Log("VictoryCoroutine Step 1");
			yield return CoroutineDefine._waitForEndOfFrame;
			bLockRewardProcess = true;
			if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
			{
				TurtorialUI.CheckTurtorialStageEnd(ManagedSingleton<StageHelper>.Instance.nLastStageID, UnLockRewardProcess);
			}
			else
			{
				bLockRewardProcess = false;
			}
			Debug.Log("VictoryCoroutine Step 2");
			float fExploreEndBGTimeOut = 2f;
			while (BattleInfoUI.Instance.CanvasExploreEndBG.enabled && !(fExploreEndBGTimeOut <= 0f))
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				fExploreEndBGTimeOut -= Time.deltaTime;
			}
			Debug.Log("VictoryCoroutine Step 3");
			OrangeCharacter oc = GetMainPlayerOC();
			if (listSaveAniSpeed.Count > 0)
			{
				while (Time.realtimeSinceStartup - fWaitSlowTime < 3f)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			else
			{
				while (Time.realtimeSinceStartup - fWaitSlowTime < 1f)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			Debug.Log("VictoryCoroutine Step 4");
			yield return WaitLockRewardProcess();
			while (TurtorialUI.IsTutorialing())
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			UnSlowStage();
			Debug.Log("VictoryCoroutine Step 5");
			StopStageAllEvent();
			Debug.Log("VictoryCoroutine Step 6");
			if (BattleInfoUI.Instance != null)
			{
				bLockRewardProcess = true;
				BattleInfoUI.Instance.CheckStartStageClear(UnLockRewardProcess);
			}
			Debug.Log("VictoryCoroutine Step 7");
			yield return WaitLockRewardProcess();
			while (!BattleInfoUI.Instance.IsCanTeleportOut)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			while (!IsCanGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			Debug.Log("VictoryCoroutine Step 8");
			if (gbIsNetGame)
			{
				OrangeCharacter mainOc = null;
				foreach (OrangeCharacter runPlayer in runPlayerList)
				{
					if (runPlayer.IsLocalPlayer)
					{
						mainOc = runPlayer;
						continue;
					}
					StartCoroutine(SetOcTeleportOut(runPlayer));
					yield return new WaitForSeconds(0.8f);
				}
				while (mainOc != null && (mainOc.IsDead() || (!mainOc.CheckActStatusEvt(0, -1) && !mainOc.CheckActStatusEvt(12, 1))))
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				StartCoroutine(SetOcTeleportOut(mainOc));
			}
			else
			{
				bool flag = true;
				if (oc == null || oc.UsingVehicle || BattleInfoUI.Instance.NowStageTable.n_TYPE == 9)
				{
					flag = false;
				}
				if (BattleInfoUI.Instance.NowStageTable.n_TYPE == 10 && (StageMode == StageMode.Contribute || ManagedSingleton<StageHelper>.Instance.eLastStageResult != StageResult.Win))
				{
					flag = false;
				}
				Debug.Log(string.Format("waitTeleportTime = {0}", flag));
				if (flag)
				{
					while (oc.IsDead())
					{
						yield return CoroutineDefine._waitForEndOfFrame;
					}
					UnityEngine.Transform transform = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform;
					if (oc.Controller.Collider2D.bounds.max.y >= transform.position.y - ManagedSingleton<StageHelper>.Instance.fCameraHHalf && oc.Controller.Collider2D.bounds.min.y <= transform.position.y + ManagedSingleton<StageHelper>.Instance.fCameraHHalf && oc.Controller.Collider2D.bounds.max.x >= transform.position.x - ManagedSingleton<StageHelper>.Instance.fCameraWHalf && oc.Controller.Collider2D.bounds.min.x <= transform.position.x + ManagedSingleton<StageHelper>.Instance.fCameraWHalf)
					{
						while (oc.IsDead() || (!oc.CheckActStatusEvt(0, -1) && !oc.CheckActStatusEvt(12, 1)))
						{
							yield return CoroutineDefine._waitForEndOfFrame;
						}
						oc.IsTeleporting = true;
						if (oc.CharacterData.n_WEAPON_OUT != -1)
						{
							oc.DisableCurrentWeapon();
							oc.EnableSkillWeapon(oc.CharacterData.n_WEAPON_OUT, delegate
							{
								oc.SetTeleportOutPose();
							});
						}
						else
						{
							oc.SetTeleportOutPose();
						}
					}
				}
			}
			Debug.Log("VictoryCoroutine Step 9");
			int n_TYPE = BattleInfoUI.Instance.NowStageTable.n_TYPE;
			if (n_TYPE != 9 && (BattleInfoUI.Instance.NowStageTable.n_TYPE != 10 || (StageMode != StageMode.Contribute && ManagedSingleton<StageHelper>.Instance.eLastStageResult == StageResult.Win)))
			{
				float fTeleportTimeOut = 20f;
				while (oc != null && !oc.UsingVehicle && oc.IsTeleporting)
				{
					if (!oc.CheckActStatusEvt(12, -1))
					{
						oc.SetTeleportOutPose();
					}
					fTeleportTimeOut -= Time.deltaTime;
					if (fTeleportTimeOut <= 0f)
					{
						break;
					}
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			Debug.Log("VictoryCoroutine Step 10");
			float fBGMTimeOut = 5f;
			while (MonoBehaviourSingleton<AudioManager>.Instance.IsNowBGMPlaying)
			{
				fBGMTimeOut -= Time.deltaTime;
				if (fBGMTimeOut <= 0f)
				{
					break;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			Debug.Log("VictoryCoroutine Step 101");
			while (!IsCanGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			Debug.Log("VictoryCoroutine Step 102");
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.Stop();
			Debug.Log("VictoryCoroutine Step 103");
			while (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			Debug.Log("VictoryCoroutine Step 104");
			bIsRewardUI = true;
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.StopAllExceptSE();
			Debug.Log("VictoryCoroutine Step 11");
			StageEndRes stageEndRes = p_param as StageEndRes;
			Code code = Code.STAGE_END_SUCCESS;
			if (stageEndRes != null)
			{
				code = (Code)stageEndRes.Code;
			}
			Debug.Log("VictoryCoroutine Step 12 - " + code);
			switch (code)
			{
			default:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(code);
				break;
			case Code.STAGE_END_SUCCESS:
			case Code.RAID_END_SUCCESS:
			case Code.CRUSADE_END_SUCCESS:
			case Code.TOTALWAR_BATTLE_END_SUCCESS:
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_STAGEREWARD", delegate(StageRewardUI ui)
				{
					ui.SetStageEndRes(p_param, fStageUseTime);
					ui.refStageUpdate = this;
				});
				break;
			case Code.STAGE_END_ERROR_RESOLUTION:
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
				break;
			}
		}

		private IEnumerator SetOcTeleportOut(OrangeCharacter tOC)
		{
			if (tOC == null)
			{
				yield break;
			}
			while (tOC.IsDead() || (!tOC.CheckActStatusEvt(0, -1) && !tOC.CheckActStatusEvt(12, 1)))
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			tOC.IsTeleporting = true;
			if (tOC.CharacterData.n_WEAPON_OUT != -1)
			{
				tOC.DisableCurrentWeapon();
				tOC.EnableSkillWeapon(tOC.CharacterData.n_WEAPON_OUT, delegate
				{
					tOC.SetTeleportOutPose();
				});
			}
			else if ((bool)tOC.GetComponent<FerhamController>())
			{
				if (!tOC.bIsNpcCpy)
				{
					tOC.SetTeleportOutPose();
				}
			}
			else
			{
				tOC.SetTeleportOutPose();
			}
		}

		private void UnLockRewardProcess()
		{
			bLockRewardProcess = false;
		}

		private IEnumerator WaitLockRewardProcess()
		{
			while (bLockRewardProcess)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		public static IEnumerator WaitGamePauseProcess()
		{
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		public static IEnumerator WaitGamePauseProcessTime(float fTime)
		{
			while (fTime > 0f)
			{
				while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
				fTime -= Time.deltaTime;
			}
		}

		public static void RemoveAllLockRange()
		{
			listLockRange.Clear();
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (stageUpdate != null)
			{
				stageUpdate.sLastLockRangeSyncID = "";
			}
		}

		public static void RemoveLastLockRange()
		{
			if (listLockRange.Count > 0)
			{
				listLockRange.RemoveAt(listLockRange.Count - 1);
			}
		}

		public static void RemoveLockRange(string sSyncID, string sPlayerID = "", bool bRemoveAfterLock = false)
		{
			if (bRemoveAfterLock)
			{
				for (int num = listLockRange.Count - 1; num >= 0; num--)
				{
					if (sPlayerID == "")
					{
						if (listLockRange[num].Contains(sSyncID))
						{
							while (listLockRange.Count > num + 1)
							{
								listLockRange.RemoveAt(listLockRange.Count - 1);
							}
							break;
						}
					}
					else if (listLockRange[num] == sSyncID + "#-#" + sPlayerID)
					{
						while (listLockRange.Count > num + 1)
						{
							listLockRange.RemoveAt(listLockRange.Count - 1);
						}
						break;
					}
				}
			}
			for (int num2 = listLockRange.Count - 1; num2 >= 0; num2--)
			{
				if (sPlayerID == "")
				{
					if (listLockRange[num2].Contains(sSyncID))
					{
						listLockRange.RemoveAt(num2);
					}
				}
				else if (listLockRange[num2] == sSyncID + "#-#" + sPlayerID)
				{
					listLockRange.RemoveAt(num2);
				}
			}
		}

		public static void AddLockRangeList(string sSyncID, string sPlayerID = "")
		{
			if (sPlayerID == "")
			{
				if (listLockRange.Contains(sSyncID))
				{
					listLockRange.Remove(sSyncID);
				}
				listLockRange.Add(sSyncID);
			}
			else
			{
				if (listLockRange.Contains(sSyncID + "#-#" + sPlayerID))
				{
					listLockRange.Remove(sSyncID + "#-#" + sPlayerID);
				}
				listLockRange.Add(sSyncID + "#-#" + sPlayerID);
			}
		}

		public static bool CheckLockRangeList(string sSyncID, string sPlayerID = "", bool bCheckIsLast = false)
		{
			if (sPlayerID == "")
			{
				if (bCheckIsLast)
				{
					int num = listLockRange.Count - 1;
					while (num >= 0)
					{
						if (listLockRange[num] == sSyncID)
						{
							return true;
						}
						if (listLockRange[num].Contains("#-#"))
						{
							if (listLockRange[num].Contains(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify))
							{
								return false;
							}
							num--;
							continue;
						}
						return false;
					}
					if (listLockRange[listLockRange.Count - 1] == sSyncID)
					{
						return true;
					}
				}
				else if (listLockRange.Contains(sSyncID))
				{
					return true;
				}
			}
			else if (bCheckIsLast)
			{
				string text = sSyncID + "#-#" + sPlayerID;
				for (int num2 = listLockRange.Count - 1; num2 >= 0; num2--)
				{
					if (listLockRange[num2] == text)
					{
						return true;
					}
					if (!listLockRange[num2].Contains("#-#"))
					{
						return false;
					}
				}
			}
			else if (listLockRange.Contains(sSyncID + "#-#" + sPlayerID))
			{
				return true;
			}
			return false;
		}

		public static bool CheckLockRangeArray(string[] sSyncIDs, int StartIndex = 0)
		{
			if (sSyncIDs.Length - 4 > listLockRange.Count - 1)
			{
				return false;
			}
			for (int i = 0; i + StartIndex < sSyncIDs.Length && i < listLockRange.Count; i++)
			{
				if (sSyncIDs[i + StartIndex] != listLockRange[i])
				{
					return false;
				}
			}
			return true;
		}

		public static string GetLockRangeListStr(List<string> ListIgnoreID)
		{
			string text = "";
			for (int i = 0; i < listLockRange.Count; i++)
			{
				if (!ListIgnoreID.Contains(listLockRange[i]))
				{
					if (text != "")
					{
						text += ",";
					}
					text += listLockRange[i];
				}
			}
			return text;
		}

		public static void CheckLastLockRangeBeforeSendNetLockMsg(string sCheckID)
		{
			if (listLockRange.Count <= 0)
			{
				return;
			}
			int num = listLockRange.Count - 1;
			while (num >= 0 && listLockRange[num].Contains("#-#"))
			{
				if (listLockRange[num].EndsWith(sCheckID))
				{
					string text = listLockRange[num].Substring(0, listLockRange[num].IndexOf("#-#"));
					{
						foreach (MemberInfo item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo)
						{
							if (item.PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && !CheckLockRangeList(text, item.PlayerId))
							{
								StageResManager.GetStageUpdate().OnSyncStageObj(text, 4, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + ",0,0,0");
							}
							AddLockRangeList(text, item.PlayerId);
						}
						break;
					}
				}
				num--;
			}
		}

		public static void SyncLockRangeByList(string sPlayerID, float x, float y, float z)
		{
			if (listLockRange.Count != 0)
			{
				string text = listLockRange[listLockRange.Count - 1];
				List<string> list = new List<string>();
				list.Add(text);
				if (text.Contains("#-#"))
				{
					sPlayerID = text.Substring(text.IndexOf("#-#") + "#-#".Length);
					text = text.Substring(0, text.IndexOf("#-#"));
				}
				SyncStageObj(text, 3, sPlayerID + "," + x + "," + y + "," + z + "," + GetLockRangeListStr(list), true);
			}
		}

		public static void SyncStageObj(string sIDKey, int nKey1, string smsg, bool bNoCheckHost = false, bool bNoCheckReConnect = false)
		{
			if ((bNoCheckHost || bIsHost) && gbIsNetGame && (bNoCheckReConnect || !bWaitReconnect))
			{
				string text = "STSYNC,";
				text = text + sIDKey + "," + nKey1 + "," + smsg;
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(text));
			}
		}

		public static void SyncStageObj(int nID, int nKey1, string smsg, bool bNoCheckHost = false, bool bNoCheckReConnect = false)
		{
			if ((bNoCheckHost || bIsHost) && gbIsNetGame && (bNoCheckReConnect || !bWaitReconnect))
			{
				string text = "STSYNC,";
				text = text + nID + "," + nKey1 + "," + smsg;
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(text));
			}
		}

		public void OnNTSyncStageObj(object obj)
		{
			if (gbIsNetGame)
			{
				NTBroadcastToRoom nTBroadcastToRoom = (NTBroadcastToRoom)obj;
				if (nTBroadcastToRoom.Action.StartsWith("STSYNC"))
				{
					string[] array = new string[3];
					string text = nTBroadcastToRoom.Action.Substring("STSYNC".Length + 1);
					int num = text.IndexOf(',');
					int num2 = text.IndexOf(',', num + 1);
					array[0] = text.Substring(0, num);
					array[1] = text.Substring(num + 1, num2 - num - 1);
					array[2] = text.Substring(num2 + 1);
					int nKey = int.Parse(array[1]);
					OnSyncStageObj(array[0], nKey, array[2]);
				}
			}
		}

		public void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
			if (sIDKey.Length != 0)
			{
				int num = sIDKey.IndexOf('-');
				if (num != -1)
				{
					nSyncStageObjsIDKey = sIDKey.Substring(num + 1);
					sIDKey = sIDKey.Substring(0, num);
				}
				else
				{
					nSyncStageObjsIDKey = "";
				}
				int.TryParse(sIDKey, out nSyncStageObjID);
				if (nSyncStageObjID >= 0 && aySyncStageFunc.Length > nSyncStageObjID && aySyncStageFunc[nSyncStageObjID] != null)
				{
					aySyncStageFunc[nSyncStageObjID](nSyncStageObjsIDKey, nKey1, smsg);
				}
				else
				{
					Debug.Log("OnSyncStageObj " + nSyncStageObjID + " is null.");
				}
			}
		}

		public StageObjBase GetSOBByNetSerialID(string sNetSerialID)
		{
			foreach (OrangeCharacter runPlayer in runPlayerList)
			{
				if (runPlayer.sPlayerID == sNetSerialID)
				{
					return runPlayer;
				}
			}
			for (int i = 0; i < EnemySets.Count; i++)
			{
				if (EnemySets[i].mEnemy.sNetSerialID == sNetSerialID)
				{
					return EnemySets[i].mEnemy;
				}
			}
			for (int j = 0; j < runSOBList.Count; j++)
			{
				if (runSOBList[j].sNetSerialID == sNetSerialID)
				{
					return runSOBList[j];
				}
			}
			return null;
		}

		private void CheckBulletDmgOK(OrangeCharacter tOC, BulletBase.DmgStack tDmgStack, StageObjBase tSOB)
		{
			switch (tDmgStack.nWeaponCheck)
			{
			case 4:
				tCheckWeaponStruct = tOC.PlayerWeapons[0];
				break;
			case 8:
				tCheckWeaponStruct = tOC.PlayerWeapons[1];
				break;
			case 1:
				tCheckWeaponStruct = tOC.PlayerSkills[0];
				break;
			case 2:
				tCheckWeaponStruct = tOC.PlayerSkills[1];
				break;
			case 16:
				tCheckWeaponStruct = tOC.PlayerFSkills[0];
				break;
			case 32:
				tCheckWeaponStruct = tOC.PlayerFSkills[1];
				break;
			default:
				return;
			}
			bHasSkill = false;
			SKILL_TABLE[] fastBulletDatas = tCheckWeaponStruct.FastBulletDatas;
			foreach (SKILL_TABLE sKILL_TABLE in fastBulletDatas)
			{
				if (sKILL_TABLE.n_ID == tDmgStack.nSkillID)
				{
					tCheckSkillTable = sKILL_TABLE;
					bHasSkill = true;
					nCheckDmg = BulletBase.CalclDmgOnlyByData(tOC, tCheckWeaponStruct.weaponStatus, tCheckWeaponStruct.SkillLV, sKILL_TABLE, tSOB, tDmgStack);
					break;
				}
			}
			if (!bHasSkill)
			{
				tCheckSkillTable = tOC.tRefPassiveskill.GetSkillTable(tDmgStack.nSkillID);
				if (tCheckSkillTable != null)
				{
					bHasSkill = true;
					nCheckDmg = BulletBase.CalclDmgOnlyByData(tOC, tCheckWeaponStruct.weaponStatus, tCheckWeaponStruct.SkillLV, tCheckSkillTable, tSOB, tDmgStack);
				}
			}
			if (!bHasSkill)
			{
				Debug.LogError("No Skill?? " + tDmgStack.nSkillID + " GGGGG");
			}
			else if ((int)tDmgStack.nDmg > 1 && (float)(int)tDmgStack.nDmg > (float)nCheckDmg * 1.1f)
			{
				BulletBase.CalclDmgOnlyByData(tOC, tCheckWeaponStruct.weaponStatus, tCheckWeaponStruct.SkillLV, tCheckSkillTable, tSOB, tDmgStack, true);
				Debug.LogError(string.Concat("GGGG1 Error Dmg ", tDmgStack.nDmg, " > ", nCheckDmg, " , ", tDmgStack.nLastHitStatus, " , ", tDmgStack.nWeaponCheck, " , ", tDmgStack.nSkillID));
				IsEnd = true;
				ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
				tStageOpenCommonTask.OpenCommon(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DAMAGE_OVER_DETECT"), BattleInfoUI.Instance.StageOutGO);
				SyncStageObj(3, 19, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true, true);
			}
		}

		private void CheckBulletDmgOK(BulletBase tBB, BulletBase.DmgStack tDmgStack, StageObjBase tSOB)
		{
			nCheckDmg = tBB.CalclDmgOnlyByLastHitStatus(tSOB, tDmgStack);
			if ((int)tDmgStack.nDmg > 1 && (float)(int)tDmgStack.nDmg > (float)nCheckDmg * 1.1f)
			{
				tBB.CalclDmgOnlyByLastHitStatus(tSOB, tDmgStack, true);
				Debug.LogError(string.Concat("GGGG2 Error Dmg ", tDmgStack.nDmg, " > ", nCheckDmg, " , ", tDmgStack.nLastHitStatus, " , ", tDmgStack.nWeaponCheck, " , ", tDmgStack.nSkillID));
				IsEnd = true;
				ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
				tStageOpenCommonTask.OpenCommon(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_END_TITLE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DAMAGE_OVER_DETECT"), BattleInfoUI.Instance.StageOutGO);
				SyncStageObj(3, 19, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true, true);
			}
		}

		private void OnSyncStageHP(string sIDKey, int nKey1, string smsg)
		{
			if (bWaitReconnect)
			{
				return;
			}
			BulletBase.DmgStack dmgStack = JsonConvert.DeserializeObject<BulletBase.DmgStack>(smsg, new JsonConverter[1]
			{
				new ObscuredValueConverter()
			});
			StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(dmgStack.sPlayerID);
			OrangeCharacter playerByID = GetPlayerByID(dmgStack.sShotPlayerID);
			nSOBType = 0;
			if (sOBByNetSerialID != null)
			{
				if ((int)sOBByNetSerialID.Hp <= 0)
				{
					return;
				}
				nSOBType = sOBByNetSerialID.GetSOBType();
			}
			if ((int)dmgStack.nDmg > 0 && playerByID != null)
			{
				if (sOBByNetSerialID != null && ((OrangeConst.DMG_DETECT_PVP_FLAG == 1 && gbRegisterPvpPlayer) || (OrangeConst.DMG_DETECT_CORP_FLAG == 1 && gbGeneratePvePlayer)))
				{
					if ((int)dmgStack.nRecordID != 0 && (int)dmgStack.nNetID == 0 && (nBulletIndex = NowHasBullet(dmgStack.nRecordID, dmgStack.nNetID)) >= 0)
					{
						CheckBulletDmgOK(listBullet[nBulletIndex], dmgStack, sOBByNetSerialID);
					}
					else
					{
						CheckBulletDmgOK(playerByID, dmgStack, sOBByNetSerialID);
					}
				}
				if (dmgStack.sShotPlayerID != dmgStack.sPlayerID)
				{
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerDMG(dmgStack.sShotPlayerID, dmgStack.nDmg, (int)dmgStack.nHP - (int)dmgStack.nEndHP, nSOBType == 1);
				}
			}
			if (sOBByNetSerialID != null)
			{
				if ((int)dmgStack.nEndHP <= 0 && playerByID != null && (int)sOBByNetSerialID.Hp > 0 && !sOBByNetSerialID.bIsNpcCpy)
				{
					if (nSOBType == 1)
					{
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerKillNum(dmgStack.sShotPlayerID);
					}
					else if (nSOBType == 2)
					{
						MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.AddPlayerKillEnemyNum(dmgStack.sShotPlayerID);
					}
				}
				if ((int)dmgStack.nRecordID != 0 && ((int)dmgStack.nNetID != 0 || nSOBType != 1) && (!(playerByID != null) || playerByID.GetNowRecordNO() >= (int)dmgStack.nRecordID) && (nBulletIndex = NowHasBullet(dmgStack.nRecordID, dmgStack.nNetID)) >= 0 && listBullet[nBulletIndex] as CollideBullet == null && listBullet[nBulletIndex].GetBulletData.n_ROLLBACK == 0)
				{
					sOBByNetSerialID.listDmgStack.Add(dmgStack);
					return;
				}
			}
			if (!(sOBByNetSerialID == null))
			{
				sOBByNetSerialID.RunDmgStack(dmgStack);
			}
		}

		private void OnSyncStageEnemyAction(string sIDKey, int nKey1, string smsg)
		{
			if (nRunStageCtrlCount > 0)
			{
				return;
			}
			new StageCtrlInsTruction().tStageCtrl = 70;
			int num = smsg.IndexOf(',');
			string text = smsg.Substring(0, num);
			smsg = smsg.Substring(num + 1);
			num = smsg.IndexOf(',');
			int nSet = int.Parse(smsg.Substring(0, num));
			string smsg2 = "";
			if (num != -1)
			{
				smsg2 = smsg.Substring(num + 1);
			}
			if (bWaitReconnect && nRunStageCtrlCount > 0)
			{
				return;
			}
			for (int num2 = EnemySets.Count - 1; num2 >= 0; num2--)
			{
				if (EnemySets[num2].mEnemy.sNetSerialID == text)
				{
					EnemySets[num2].mEnemy.UpdateStatus(nSet, smsg2);
					return;
				}
			}
			Debug.Log("No nNetSerialID " + nKey1 + " OnSyncStageEnemyAction");
		}

		private void OnSyncPlayerAction(string sIDKey, int nKey1, string smsg)
		{
			string[] array = smsg.Split(',');
			StageCtrlInsTruction stageCtrlInsTruction = new StageCtrlInsTruction();
			stageCtrlInsTruction.tStageCtrl = nKey1;
			stageCtrlInsTruction.fTime = float.Parse(array[1]);
			stageCtrlInsTruction.nParam1 = 0f;
			stageCtrlInsTruction.nParam2 = 0f;
			Vector3 vector = default(Vector3);
			for (int i = 0; i < runPlayerList.Count; i++)
			{
				if (!(runPlayerList[i].sPlayerID == array[0]))
				{
					continue;
				}
				if (stageCtrlInsTruction.tStageCtrl == 71)
				{
					int.Parse(array[2]);
					vector.x = float.Parse(array[3]);
					vector.y = float.Parse(array[4]);
					vector.z = float.Parse(array[5]);
				}
				else if (stageCtrlInsTruction.tStageCtrl == 73)
				{
					stageCtrlInsTruction.nParam1 = float.Parse(array[2]);
					if (runPlayerList[i].transform.position.x > stageCtrlInsTruction.nParam1)
					{
						stageCtrlInsTruction.tStageCtrl = 21;
					}
					else
					{
						stageCtrlInsTruction.tStageCtrl = 20;
					}
					runPlayerList[i].ObjCtrl(runPlayerList[i].gameObject, stageCtrlInsTruction);
				}
				else if (stageCtrlInsTruction.tStageCtrl == 3 || stageCtrlInsTruction.tStageCtrl == 4)
				{
					stageCtrlInsTruction.nParam1 = (bool.Parse(array[2]) ? 1 : 0);
					runPlayerList[i].ObjCtrl(runPlayerList[i].gameObject, stageCtrlInsTruction);
				}
				else if (stageCtrlInsTruction.tStageCtrl == 21 || stageCtrlInsTruction.tStageCtrl == 20)
				{
					stageCtrlInsTruction.nParam1 = float.Parse(array[3]);
					stageCtrlInsTruction.nParam2 = float.Parse(array[4]);
					bool.Parse(array[2]);
					runPlayerList[i].ObjCtrl(runPlayerList[i].gameObject, stageCtrlInsTruction);
				}
				else
				{
					runPlayerList[i].ObjCtrl(runPlayerList[i].gameObject, stageCtrlInsTruction);
				}
				break;
			}
		}

		private void OnStageSyncEvent(string sIDKey, int nKey1, string smsg)
		{
		}

		private void OnSyncStatus(string sIDKey, int nKey1, string smsg)
		{
			switch (nKey1)
			{
			case 1:
			{
				string[] array = smsg.Split(',');
				int num2 = int.Parse(array[1]);
				int iD = int.Parse(array[2]);
				int nMaxHP = int.Parse(array[3]);
				int nskillid = int.Parse(array[4]);
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					if (num2 != 0)
					{
						sOBByNetSerialID.selfBuffManager.AddBuff(num2, iD, nMaxHP, nskillid, true, array[5], 4);
					}
					else
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, sOBByNetSerialID.GetDamageTextPos(), 0, sOBByNetSerialID.GetSOBLayerMask(), VisualDamage.DamageType.Resist);
					}
				}
				break;
			}
			case 2:
			{
				BulletBase.NetBulletData netBulletData = JsonConvert.DeserializeObject<BulletBase.NetBulletData>(smsg, new JsonConverter[1]
				{
					new ObscuredValueConverter()
				});
				if (netBulletData.sNetSerialID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					Debug.LogError("收到其他玩家幫我產子彈，如果剛剛有縮桌面請忽視!!");
					break;
				}
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(netBulletData.sNetSerialID);
				if (sOBByNetSerialID != null && sOBByNetSerialID.GetSOBType() == 1)
				{
					OrangeCharacter orangeCharacter2 = sOBByNetSerialID as OrangeCharacter;
					if (orangeCharacter2.GetNowRecordNO() < netBulletData.nRecordNO)
					{
						OrangeNetCharacter orangeNetCharacter = orangeCharacter2 as OrangeNetCharacter;
						if (orangeNetCharacter != null)
						{
							orangeNetCharacter.AddBulletToUpdateShot(netBulletData);
						}
						break;
					}
				}
				ShotBulletByNetBulletData(netBulletData);
				break;
			}
			case 3:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					int nAdd = int.Parse(array[1]);
					sOBByNetSerialID.selfBuffManager.AddMeasure(nAdd);
				}
				break;
			}
			case 4:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (!(sOBByNetSerialID != null))
				{
					break;
				}
				OrangeCharacter orangeCharacter = sOBByNetSerialID as OrangeCharacter;
				if (!(orangeCharacter != null))
				{
					break;
				}
				int nPetID = int.Parse(array[1]);
				int nSetNumID = int.Parse(array[2]);
				CharacterControlBase component = orangeCharacter.GetComponent<CharacterControlBase>();
				if ((bool)component)
				{
					if (array.Length > 3)
					{
						Vector3 value = new Vector3(float.Parse(array[3]), float.Parse(array[4]), float.Parse(array[5]));
						component.CallPet(nPetID, true, nSetNumID, value);
					}
					else
					{
						component.CallPet(nPetID, true, nSetNumID, null);
					}
				}
				break;
			}
			case 5:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					int tEffectID = int.Parse(array[1]);
					int num = int.Parse(array[2]);
					if (num == 0)
					{
						sOBByNetSerialID.selfBuffManager.RemoveBuff(tEffectID, false);
						break;
					}
					sOBByNetSerialID.selfBuffManager.CheckDelBuffTrigger(num, 2);
					sOBByNetSerialID.selfBuffManager.RemoveBuffByBuffID(num, false);
				}
				break;
			}
			case 6:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					int buffId = int.Parse(array[1]);
					string playerId = array[2];
					sOBByNetSerialID.selfBuffManager.RemoveMarkedEffect(buffId, playerId, false);
				}
				break;
			}
			case 7:
				listPerGameSaveData.Add(smsg);
				break;
			case 8:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					StageOutDead component3 = sOBByNetSerialID.GetComponent<StageOutDead>();
					if (component3 != null)
					{
						component3.CheckCountDown();
					}
				}
				break;
			}
			case 9:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					StageOutDead component2 = sOBByNetSerialID.GetComponent<StageOutDead>();
					if (component2 != null)
					{
						component2.ReSetCountDown();
					}
				}
				break;
			}
			case 10:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					sOBByNetSerialID.PlaySE(array[1], array[2]);
				}
				break;
			}
			case 11:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(array[1], sOBByNetSerialID.AimPosition, Quaternion.identity, Array.Empty<object>());
				}
				break;
			}
			case 12:
			{
				string[] array = smsg.Split(',');
				StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(array[0]);
				if (sOBByNetSerialID != null)
				{
					int iD = int.Parse(array[1]);
					sOBByNetSerialID.UpdatePassiveUseTime(iD);
				}
				break;
			}
			}
		}

		public static List<string> GetPerGameSaveData()
		{
			if (StageResManager.GetStageUpdate() != null)
			{
				return StageResManager.GetStageUpdate().listPerGameSaveData;
			}
			return new List<string>();
		}

		public static void SetPerGameSavaData(List<string> listSet)
		{
			if (StageResManager.GetStageUpdate() != null)
			{
				StageResManager.GetStageUpdate().listPerGameSaveData = listSet;
			}
		}

		public static void AddPerGameSavaData(string sData)
		{
			if (StageResManager.GetStageUpdate() != null)
			{
				StageResManager.GetStageUpdate().listPerGameSaveData.Add(sData);
			}
		}

		public void ShotBulletByNetBulletData(BulletBase.NetBulletData tNBD)
		{
			PerBuffManager.BuffStatus objFromPool = StageResManager.GetObjFromPool<PerBuffManager.BuffStatus>();
			WeaponStatus objFromPool2 = StageResManager.GetObjFromPool<WeaponStatus>();
			StageObjBase sOBByNetSerialID = GetSOBByNetSerialID(tNBD.sNetSerialID);
			objFromPool2.nHP = tNBD.nHP;
			objFromPool2.nATK = tNBD.nATK;
			objFromPool2.nCRI = tNBD.nCRI;
			objFromPool2.nHIT = tNBD.nHIT;
			objFromPool2.nWeaponCheck = tNBD.nWeaponCheck;
			objFromPool2.nWeaponType = tNBD.nWeaponType;
			objFromPool2.nCriDmgPercent = tNBD.nCriDmgPercent;
			objFromPool2.nReduceBlockPercent = tNBD.nReduceBlockPercent;
			objFromPool.fAtkDmgPercent = tNBD.fAtkDmgPercent;
			objFromPool.fCriPercent = tNBD.fCriPercent;
			objFromPool.fCriDmgPercent = tNBD.fCriDmgPercent;
			objFromPool.fMissPercent = tNBD.fMissPercent;
			if (sOBByNetSerialID != null)
			{
				objFromPool.refPBM = sOBByNetSerialID.selfBuffManager;
				objFromPool.refPS = sOBByNetSerialID.tRefPassiveskill;
			}
			else
			{
				objFromPool.refPBM = null;
				objFromPool.refPS = null;
			}
			SKILL_TABLE value = new SKILL_TABLE();
			bool flag = false;
			try
			{
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(tNBD.nSkillID))
				{
					value = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[tNBD.nSkillID].GetSkillTableByValue();
				}
				NetDiffDataPacker netDiffDataPacker = JsonHelper.Deserialize<NetDiffDataPacker>(tNBD.tNetSkillTable);
				if (netDiffDataPacker != null)
				{
					Dictionary<int, object> dictionary = new Dictionary<int, object>();
					foreach (NetDiffData item in netDiffDataPacker.Vec)
					{
						dictionary.Add(item.key, item.value);
					}
					value.CombineDiffDictionary(dictionary);
				}
				flag = true;
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message);
			}
			if (!flag)
			{
				ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(tNBD.nSkillID, out value);
			}
			if (value == null)
			{
				return;
			}
			BulletBase bulletBase = CreateBulletDetail(value);
			if (bulletBase == null)
			{
				return;
			}
			IAimTarget pTarget = null;
			if (tNBD.sTargerNetID != "")
			{
				pTarget = GetSOBByNetSerialID(tNBD.sTargerNetID) as IAimTarget;
			}
			if (sOBByNetSerialID != null)
			{
				bulletBase.UpdateBulletData(value, sOBByNetSerialID.GetSOBName(), tNBD.nRecordNO, tNBD.nBulletID, tNBD.nDirect);
				bulletBase.SetBulletAtk(objFromPool2, objFromPool);
				bulletBase.TargetAimPositionOnActive(tNBD.vTargetAimPos);
				bulletBase.transform.position = tNBD.vPos;
				if (sOBByNetSerialID.GetSOBType() == 1)
				{
					if (tNBD.sShotTransPath != "")
					{
						UnityEngine.Transform transform = null;
						transform = ((!(tNBD.sShotTransPath == "root")) ? sOBByNetSerialID.transform.Find(tNBD.sShotTransPath) : sOBByNetSerialID.transform);
						if (transform != null)
						{
							bulletBase.Active(transform, tNBD.vShotDir, (sOBByNetSerialID as OrangeCharacter).TargetMask, pTarget);
						}
						else
						{
							bulletBase.Active(bulletBase.transform, tNBD.vShotDir, (sOBByNetSerialID as OrangeCharacter).TargetMask, pTarget);
						}
					}
					else
					{
						bulletBase.Active(bulletBase.transform, tNBD.vShotDir, (sOBByNetSerialID as OrangeCharacter).TargetMask, pTarget);
					}
				}
				else
				{
					bulletBase.Active(bulletBase.transform, tNBD.vShotDir, tNBD.nTargetMask, pTarget);
				}
				bulletBase.transform.position = tNBD.vPos;
			}
			else
			{
				bulletBase.UpdateBulletData(value, "", tNBD.nRecordNO, tNBD.nBulletID, tNBD.nDirect);
				bulletBase.SetBulletAtk(objFromPool2, objFromPool);
				bulletBase.TargetAimPositionOnActive(tNBD.vTargetAimPos);
				bulletBase.transform.position = tNBD.vPos;
				bulletBase.Active(bulletBase.transform, tNBD.vShotDir, tNBD.nTargetMask, pTarget);
			}
			StageResManager.BackObjToPool(objFromPool2);
			StageResManager.BackObjToPool(objFromPool);
		}

		private void OnSyncStageObjAction(string sIDKey, int nKey1, string smsg)
		{
			int num = smsg.IndexOf(',');
			string text = smsg.Substring(0, num);
			smsg = smsg.Substring(num + 1);
			num = smsg.IndexOf(',');
			int nSet = int.Parse(smsg.Substring(0, num));
			string smsg2 = "";
			if (num != -1)
			{
				smsg2 = smsg.Substring(num + 1);
			}
			if (bWaitReconnect && nRunStageCtrlCount > 0)
			{
				return;
			}
			for (int num2 = runSOBList.Count - 1; num2 >= 0; num2--)
			{
				if (runSOBList[num2].sNetSerialID == text)
				{
					runSOBList[num2].UpdateStatus(nSet, smsg2);
					return;
				}
			}
			Debug.Log("No nNetSerialID " + nKey1 + " OnSyncStageObjAction");
		}

		private void OnSyncBulletAction(string sIDKey, int nKey1, string smsg)
		{
			int num = smsg.IndexOf(',');
			string text = smsg.Substring(0, num);
			smsg = smsg.Substring(num + 1);
			num = smsg.IndexOf(',');
			int nSet = int.Parse(smsg.Substring(0, num));
			string smsg2 = "";
			if (num != -1)
			{
				smsg2 = smsg.Substring(num + 1);
			}
			if (bWaitReconnect && nRunStageCtrlCount > 0)
			{
				return;
			}
			for (int num2 = listSyncBullet.Count - 1; num2 >= 0; num2--)
			{
				if (listSyncBullet[num2].sNetSerialID == text)
				{
					listSyncBullet[num2].SyncStatus(nSet, smsg2);
					return;
				}
			}
			Debug.Log("No nNetSerialID " + nKey1 + " OnSyncBulletAction");
		}

		private BulletBase CreateBulletDetail(string bulletData)
		{
			SKILL_TABLE sKILL_TABLE = new SKILL_TABLE();
			sKILL_TABLE.ConvertFromString(bulletData);
			return CreateBulletDetail(sKILL_TABLE);
		}

		private BulletBase CreateBulletDetail(SKILL_TABLE bulletData)
		{
			if (bulletData != null)
			{
				BulletBase bulletBase = null;
				PoolManager instance = MonoBehaviourSingleton<PoolManager>.Instance;
				switch ((BulletType)(short)bulletData.n_TYPE)
				{
				case BulletType.Continuous:
					bulletBase = instance.GetPoolObj<ContinuousBullet>(bulletData.s_MODEL);
					break;
				case BulletType.Spray:
					bulletBase = instance.GetPoolObj<SprayBullet>(bulletData.s_MODEL);
					break;
				case BulletType.Collide:
					bulletBase = instance.GetPoolObj<CollideBullet>(bulletData.s_MODEL);
					((CollideBullet)bulletBase).bNeedBackPoolModelName = true;
					break;
				case BulletType.LrColliderBulle:
					bulletBase = instance.GetPoolObj<LrColliderBullet>(bulletData.s_MODEL);
					break;
				default:
					bulletBase = instance.GetPoolObj<BulletBase>(bulletData.s_MODEL);
					break;
				}
				if ((bool)bulletBase)
				{
					return bulletBase;
				}
			}
			return null;
		}

		public static void AddStringMsg(string msg)
		{
		}
	}
}
