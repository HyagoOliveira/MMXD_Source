using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CallbackDefs;
using DragonBones;
using NaughtyAttributes;
using OrangeApi;
using OrangeAudio;
using OrangeSocket;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class BattleInfoUI : OrangeUIBase
{
	private class EnemyHintObj
	{
		public GameObject root;

		public UnityEngine.Transform HintImgTrans;

		public UnityEngine.Transform BossHintImgTrans;

		public UnityEngine.Transform MarkImgTrans;

		public EnemyHintObj(GameObject tObj)
		{
			root = tObj;
			if (root != null)
			{
				HintImgTrans = root.transform.Find("EnemyHintImg");
				BossHintImgTrans = root.transform.Find("BossHintImg");
				MarkImgTrans = BossHintImgTrans.transform.Find("MarkImg");
				SetMode(0);
			}
			else
			{
				HintImgTrans = null;
				BossHintImgTrans = null;
				MarkImgTrans = null;
			}
			root.SetActive(false);
		}

		public void SetMode(int nMode)
		{
			switch (nMode)
			{
			case 0:
				HintImgTrans.gameObject.SetActive(true);
				BossHintImgTrans.gameObject.SetActive(false);
				break;
			case 1:
				HintImgTrans.gameObject.SetActive(false);
				BossHintImgTrans.gameObject.SetActive(true);
				MarkImgTrans.transform.rotation = Quaternion.identity;
				break;
			}
		}
	}

	private enum CONTINUE_MODE
	{
		FREE_PLAY = 0,
		SINGLE_PLAY = 1,
		SELFDEAD_COUNTDOWN = 2,
		ALLDEAD_COUNTDOWN = 3,
		STAGE_FAIL = 4,
		NONE_CONTINUE = 5
	}

	private class PlayinfoOC
	{
		public OrangeCharacter tOC;

		public int nIndex = -1;

		public float fContinueTime;

		public float fTimeLastAutoBorn;
	}

	[Header("PlayerInfo")]
	public Canvas[] PlayerInfo;

	public StageLoadIcon[] headicon;

	public Image[] playerhp;

	public OrangeText[] score;

	public GameObject[] BgMaskImg;

	public Text[] reborntime;

	public Image playersp;

	[Header("BossTopBar")]
	public Canvas BossTopBar;

	private List<BossBarGroup> listBossBarGroups = new List<BossBarGroup>();

	private int nBossBarIndex;

	public Action AddHpFullCB;

	[Header("WaringUI")]
	public Canvas WaringRoot;

	public UnityArmatureComponent Warning;

	public Canvas CautionRoot;

	public UnityArmatureComponent Caution;

	[Header("StageClearUI")]
	public Canvas StageClearRoot;

	public UnityArmatureComponent StageClearDB;

	[Header("REVISEDUI")]
	public Canvas REVISEDRoot;

	public UnityArmatureComponent REVISEDDB;

	[Header("BattleFinish")]
	public Canvas BattleFinishRoot;

	public UnityArmatureComponent BattleFinishDB;

	[Header("ExploreEndBG")]
	public Canvas CanvasExploreEndBG;

	public Image ExploreEndBG;

	private Coroutine tTalkEnd;

	private Coroutine tExploreStageEnd;

	[Header("ContinueRoot")]
	public Canvas ContinueRoot;

	public GameObject[] ContinueSubRoot;

	public Text ContinueTitle;

	public Text[] ContinueCost;

	public StageLoadIcon[] ContinueItemIcon;

	public Text[] ContinueItemText;

	public Text ContinueCDTime;

	public Text ContinueFailTime;

	public Text AutoContinueTime;

	public Button ReButton;

	public Button ExitButton;

	public Button PowerHintBtn;

	private Coroutine tContinueCheckHostOutCoroutine;

	private Coroutine tContinueCoroutine;

	[Header("FlagModeUI")]
	public Canvas FlagBattleBar;

	public Text FlagBattleTime;

	public Image LeftBar;

	public Text LeftValue;

	public Text RightValue;

	private float fSyncFlagWait;

	private const float FSyncFlagWaitTime = 1f;

	[Header("StandObj")]
	public Canvas StandObj;

	[Header("HintMsg")]
	public Text AlwaysHitMsg;

	public Image TimeHitBg;

	public Text TimeHitMsg;

	[Header("CountDownTime")]
	public Text CountDownTime;

	private int nCountDownTimeMode;

	private float fCountDownTimeModeParam;

	[Header("EnemyHint")]
	public GameObject EnemyHint;

	public Action lastDBCB;

	[Header("RangerBar")]
	public Image LRRangeBar;

	public Image TBRangeBar;

	[Header("GoBattleHint")]
	public CanvasGroup GoRoot;

	public CanvasGroup BattleRoot;

	public Image MoveArrow;

	public Image XScale;

	private Dictionary<int, ScoreUIBase> tKillActivityUI;

	private Dictionary<int, ScoreUIBase> tBattleScoreUI;

	[Header("KillActivityUI")]
	public Text GetItemText;

	public Text ContributionNowNum;

	public Text TCDText;

	private Vector3? vTeleportPos;

	public Canvas DangerBG;

	public Canvas LoveScoreRoot;

	public Text GetCandyText;

	public Text GetHeartText;

	public StageLoadIcon CandyImg;

	private Vector3 MoveArrowOrignPos;

	private Vector3 MoveArrowTmp;

	private Color MoveColor = Color.white;

	private bool bMoveFront = true;

	private int nGoHintMode;

	private const float fMoveArrowSpeed = 20f;

	[Header("BossDieEvent")]
	[SerializeField]
	public float cameraZoomInTime = 1f;

	[SerializeField]
	public float cameraZoomInRate = 1f;

	[SerializeField]
	public bool isClearPlayFrist;

	[BoxGroup("Sound")]
	[SerializeField]
	private BGM00 m_challengeWin;

	[BoxGroup("Sound")]
	[SerializeField]
	private BGM00 m_stageWin;

	[BoxGroup("Sound")]
	[SerializeField]
	private BGM00 m_bossEntry;

	[BoxGroup("Sound")]
	[SerializeField]
	private BGM01 m_bossVAVAEntry = BGM01.CRI_BGM01_BGM_EV_VAVA01;

	[BoxGroup("Sound")]
	[SerializeField]
	private BGM01 m_mbossEntry;

	[BoxGroup("Sound")]
	[SerializeField]
	private BattleSE m_waring;

	[BoxGroup("Sound")]
	[SerializeField]
	private BattleSE m_caution;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_countinueSE;

	public bool IsBossAppear;

	public static BattleInfoUI Instance;

	private NotifyCallBack EnemyInfoCB;

	private NotifyCallBack LastDBNotifyCB;

	private CanvasGroup EnemyInfoGroup;

	private bool bLockVoiceHint;

	private bool bBreakVoiceHintCor;

	public Text[] ServerResponseTime;

	public GameObject option;

	private bool[] bShowOption = new bool[3] { false, true, false };

	public const float fSWD = -58f;

	private float fTimeLastAutoBorn;

	private float fDTime;

	private float fContinueTime;

	private float fFailedTime;

	private int nContinueMode;

	private List<RebornPlayerData> listRebornPlayerID = new List<RebornPlayerData>();

	private const float fRebornNetDelayLimit = 3f;

	private float nStageTimeCountDown;

	private Dictionary<int, int> DicCountScore = new Dictionary<int, int>();

	private int[] nTeamValue = new int[2];

	private int nFlagWinValue = 500;

	private bool bIsShowStageClear;

	private bool isBossHpSe;

	private bool isClearBgmPlay;

	private int CountDownSEPhase = 10;

	private int TPCountDownSEPhase = 3;

	private int SelfDeadCountDownPhase = OrangeConst.CORP_AUTO_CONTINUE - 1;

	private int AllDeadCountDownPhase = OrangeConst.CORP_FAILED_TIME - 1;

	private OrangeTimer clearBGMTimer;

	private readonly uint nHightResponse = 100u;

	private readonly uint nMidResponse = 250u;

	private int nCampaignScore;

	public int nCampaignTotalScore = 10000;

	private bool isClickQuiteBtn;

	private OrangeCharacter _oc;

	private STAGE_TABLE tSTAGE_TABLE;

	public List<EVENT_TABLE> tNowEvent = new List<EVENT_TABLE>();

	private List<int> listBattleInfoUpdate = new List<int>();

	private bool bShowEnemyHint;

	private float cameraHHalf = 1080f;

	private float cameraWHalf = 1920f;

	private List<EnemyHintObj> listEnemyHintObjs = new List<EnemyHintObj>();

	private List<PlayinfoOC> listPlayinfoOC = new List<PlayinfoOC>();

	private int _lockStepWaitingDot;

	private bool _bIsSteamPauseEnabled;

	public bool IsCanTeleportOut { get; set; }

	public bool IsPlayClearBgm
	{
		get
		{
			return isClearBgmPlay;
		}
	}

	public float fCountDownTimerValue
	{
		get
		{
			return nStageTimeCountDown;
		}
	}

	public int nBattleScore
	{
		get
		{
			if (!DicCountScore.ContainsKey(0))
			{
				DicCountScore.Add(0, 0);
			}
			return DicCountScore[0];
		}
		private set
		{
			if (!DicCountScore.ContainsKey(0))
			{
				DicCountScore.Add(0, 0);
			}
			DicCountScore[0] = value;
		}
	}

	public float nGetnGetItemValue
	{
		get
		{
			return nBattleScore;
		}
	}

	public float nGetnBattleScoreValue
	{
		get
		{
			return nBattleScore + nCampaignScore;
		}
	}

	public int nGetCampaignScore
	{
		get
		{
			return nCampaignScore;
		}
	}

	private OrangeCharacter OC
	{
		get
		{
			if (_oc == null)
			{
				_oc = StageUpdate.GetMainPlayerOC();
			}
			return _oc;
		}
	}

	public STAGE_TABLE NowStageTable
	{
		get
		{
			return tSTAGE_TABLE;
		}
	}

	protected override void Awake()
	{
		ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out tSTAGE_TABLE);
		if (tSTAGE_TABLE == null)
		{
			tSTAGE_TABLE = new STAGE_TABLE();
		}
		base.Awake();
		clearBGMTimer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<InputManager>.Instance.gamepadDetachedEvent -= TryPause;
		MonoBehaviourSingleton<InputManager>.Instance.gamepadDetachedEvent += TryPause;
		Singleton<GenericEventManager>.Instance.AttachEvent<int, NotifyCallBack>(EventManager.ID.STAGE_EVENT_WARING, EventCallBack);
		Singleton<GenericEventManager>.Instance.AttachEvent<int, NotifyCallBack>(EventManager.ID.STAGE_SHOW_ENEMYINFO, EventShowEnemyInfo);
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.STAGE_UPDATE_HOST, UpdateHost);
		Singleton<GenericEventManager>.Instance.AttachEvent<int, int>(EventManager.ID.STAGE_PLAYER_INFLAG_RANGE, UpdatePlayerInFlagRange);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.PLAYERBUILD_PLAYER_SPAWN, ReadyGoEndNotify);
		Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.LockRangeParam>(EventManager.ID.LOCK_RANGE, EventLockRange);
		Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.BattleInfoUpdate>(EventManager.ID.BATTLE_INFO_UPDATE, BattleInfoUpdate);
		for (int i = 0; i < PlayerInfo.Length; i++)
		{
			PlayerInfo[i].enabled = false;
			BgMaskImg[i].SetActive(false);
		}
		playersp.fillAmount = 1f;
		listBossBarGroups.Add(new BossBarGroup(BossTopBar));
		BossTopBar.enabled = false;
		CanvasExploreEndBG.enabled = false;
		playersp.transform.parent.gameObject.SetActive(false);
		Instance = this;
		WaringRoot.enabled = false;
		Warning.enabled = false;
		CautionRoot.enabled = false;
		Caution.enabled = false;
		StageClearRoot.enabled = false;
		StartCoroutine(LoadStageClearCoroutine());
		REVISEDRoot.enabled = false;
		REVISEDDB.enabled = false;
		ContinueRoot.enabled = false;
		StandObj = AlwaysHitMsg.transform.parent.GetComponent<Canvas>();
		StandObj.enabled = true;
		CloseBattleInfoUIAllUI();
		EnemyHint.GetComponent<Canvas>().enabled = true;
		listEnemyHintObjs.Add(new EnemyHintObj(EnemyHint));
		LeftBar.fillAmount = 0.5f;
		bIsShowStageClear = false;
		SwitchOptionBtn(false);
		bool isBossAppear = (IsCanTeleportOut = false);
		IsBossAppear = isBossAppear;
		ServerResponseTime[0].transform.parent.parent.GetComponent<Canvas>().enabled = true;
		tNowEvent = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_BONUS, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
		for (int num = tNowEvent.Count - 1; num >= 0; num--)
		{
			if (tNowEvent[num].n_BONUS_TYPE != 7 || tNowEvent[num].n_TYPE_Y != NowStageTable.n_MAIN || tNowEvent[num].n_TYPE_X != NowStageTable.n_TYPE)
			{
				tNowEvent.RemoveAt(num);
			}
		}
		if ((bool)GoRoot)
		{
			MoveArrowOrignPos = MoveArrow.transform.localPosition;
			GoRoot.alpha = 0f;
		}
		for (int j = 0; j < ServerResponseTime.Length; j++)
		{
			if (ServerResponseTime[j].transform.parent.gameObject.activeSelf)
			{
				ServerResponseTime[j].transform.parent.gameObject.SetActive(false);
			}
		}
		isClickQuiteBtn = false;
		base._EscapeEvent = EscapeEvent.CUSTOM;
	}

	private IEnumerator LoadStageClearCoroutine()
	{
		while (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		string text = "stageclear";
		foreach (StageClearMap item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj.listStageClearMap)
		{
			if (item.nStageMainID == 0 || (item.nStageMainID == tSTAGE_TABLE.n_MAIN && item.nStageSubID == tSTAGE_TABLE.n_SUB))
			{
				text = item.sStageClearDB;
			}
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, text), text + "db", delegate(GameObject obj)
		{
			if (!(obj == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(obj, StageClearRoot.transform);
				StageClearDB = gameObject.GetComponent<UnityArmatureComponent>();
			}
		});
	}

	protected override void DoCustomEscapeEvent()
	{
		OnClickOption();
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<InputManager>.Instance.gamepadDetachedEvent -= TryPause;
		if (isBossHpSe)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_BOSSHP01_STOP);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.IsBossAppear = false;
		if (Time.timeScale != 1f)
		{
			Time.timeScale = 1f;
		}
		Instance = null;
		Singleton<GenericEventManager>.Instance.DetachEvent<int, NotifyCallBack>(EventManager.ID.STAGE_EVENT_WARING, EventCallBack);
		Singleton<GenericEventManager>.Instance.DetachEvent<int, NotifyCallBack>(EventManager.ID.STAGE_SHOW_ENEMYINFO, EventShowEnemyInfo);
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.STAGE_UPDATE_HOST, UpdateHost);
		Singleton<GenericEventManager>.Instance.DetachEvent<int, int>(EventManager.ID.STAGE_PLAYER_INFLAG_RANGE, UpdatePlayerInFlagRange);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.PLAYERBUILD_PLAYER_SPAWN, ReadyGoEndNotify);
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.LockRangeParam>(EventManager.ID.LOCK_RANGE, EventLockRange);
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.BattleInfoUpdate>(EventManager.ID.BATTLE_INFO_UPDATE, BattleInfoUpdate);
		MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = false;
	}

	private void Update()
	{
		if (!StageUpdate.gbStageReady)
		{
			return;
		}
		if (nGoHintMode == 0)
		{
			if ((tSTAGE_TABLE.n_MAIN == 90000 && tSTAGE_TABLE.n_SUB == 1) || (tSTAGE_TABLE.n_MAIN == 90001 && (tSTAGE_TABLE.n_SUB == 1 || tSTAGE_TABLE.n_SUB == 2)))
			{
				GoRoot.alpha = 0f;
				nGoHintMode = 3;
			}
			else
			{
				GoRoot.alpha = 1f;
				nGoHintMode = 1;
			}
		}
		else if (nGoHintMode == 1)
		{
			if (bMoveFront)
			{
				MoveArrowTmp = MoveArrow.transform.localPosition;
				MoveArrowTmp.x += 20f * Time.deltaTime;
				if (MoveArrowTmp.x - MoveArrowOrignPos.x > 5f)
				{
					MoveArrowTmp.x = MoveArrowOrignPos.x + 5f - (MoveArrowTmp.x - MoveArrowOrignPos.x - 5f);
					bMoveFront = false;
				}
				MoveArrow.transform.localPosition = MoveArrowTmp;
			}
			else
			{
				MoveArrowTmp = MoveArrow.transform.localPosition;
				MoveArrowTmp.x -= 20f * Time.deltaTime;
				if (MoveArrowTmp.x - MoveArrowOrignPos.x < -5f)
				{
					MoveArrowTmp.x = MoveArrowOrignPos.x - 5f - (MoveArrowTmp.x - MoveArrowOrignPos.x + 5f);
					bMoveFront = true;
				}
				MoveArrow.transform.localPosition = MoveArrowTmp;
			}
		}
		else if (nGoHintMode == 2)
		{
			MoveArrowTmp.x += Time.deltaTime * 0.7f;
			MoveArrowTmp.y = MoveArrowTmp.x;
			XScale.transform.localScale = MoveArrowTmp;
			if (MoveArrowTmp.x > 2.5f)
			{
				XScale.color = Color.white;
				MoveArrowTmp = Vector3.one;
				XScale.transform.localScale = MoveArrowTmp;
			}
			else if (MoveArrowTmp.x > 1.2f)
			{
				MoveColor.a = (1.6f - MoveArrowTmp.x) * 2.5f;
				if (MoveColor.a < 0f)
				{
					MoveColor.a = 0f;
				}
				XScale.color = MoveColor;
			}
		}
		if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			for (int num = listBossBarGroups.Count - 1; num >= 0; num--)
			{
				listBossBarGroups[num].CheckAniBar();
			}
			if (nCountDownTimeMode != 0)
			{
				if (nCountDownTimeMode == 2)
				{
					nStageTimeCountDown += Time.deltaTime;
				}
				else if (nCountDownTimeMode == 1)
				{
					nStageTimeCountDown -= Time.deltaTime;
				}
				if (nCountDownTimeMode == 1 && nStageTimeCountDown <= 0f)
				{
					CountDownTime.text = string.Format("00:00:00");
					CountDownTime.color = Color.red;
					FlagBattleTime.text = string.Format("00:00");
					FlagBattleTime.color = Color.red;
					ReStageCountDownTime();
					CheckStageSuccess();
				}
				else if (nCountDownTimeMode == 2 && nStageTimeCountDown >= fCountDownTimeModeParam)
				{
					TimeSpan timeSpan = TimeSpan.FromSeconds(fCountDownTimeModeParam);
					CountDownTime.text = string.Format("{0}:{1:D2}", Mathf.FloorToInt((float)timeSpan.TotalSeconds), timeSpan.Milliseconds / 10);
					CountDownTime.color = Color.white;
					ReStageCountDownTime();
					CheckStageSuccess();
				}
				else if (nCountDownTimeMode < 996)
				{
					if ((int)nStageTimeCountDown % 2 == 0)
					{
						ReStageCountDownTime();
					}
					TimeSpan timeSpan2 = TimeSpan.FromSeconds(nStageTimeCountDown);
					if (nCountDownTimeMode == 1)
					{
						CountDownTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan2.Minutes, timeSpan2.Seconds, timeSpan2.Milliseconds / 10);
						FlagBattleTime.text = string.Format("{0:D2}:{1:D2}", timeSpan2.Minutes, timeSpan2.Seconds);
						if (nStageTimeCountDown <= 10f)
						{
							CountDownTime.color = Color.red;
							FlagBattleTime.color = Color.red;
							DangerBG.enabled = true;
							CountDownSE(timeSpan2.Seconds, ref CountDownSEPhase);
						}
						else
						{
							CountDownTime.color = Color.white;
							FlagBattleTime.color = Color.white;
							DangerBG.enabled = false;
						}
					}
					else if (nCountDownTimeMode == 2)
					{
						CountDownTime.text = string.Format("{0}:{1:D2}", Mathf.FloorToInt((float)timeSpan2.TotalSeconds), timeSpan2.Milliseconds / 10);
						CountDownTime.color = Color.white;
					}
				}
			}
			fSyncFlagWait += Time.deltaTime;
			if (fSyncFlagWait >= 1f)
			{
				fSyncFlagWait = 0f;
				ReSendSyncFlagScore();
			}
		}
		if (ServerResponseTime == null || !(MonoBehaviourSingleton<CBSocketClient>.Instance != null))
		{
			return;
		}
		for (int i = 0; i < ServerResponseTime.Length; i++)
		{
			ServerResponseTime[i].transform.parent.gameObject.SetActive(false);
		}
		if (!StageUpdate.gbIsNetGame)
		{
			return;
		}
		if (MonoBehaviourSingleton<CBSocketClient>.Instance.Connected())
		{
			uint packageAverageTime = (uint)MonoBehaviourSingleton<CBSocketClient>.Instance.PackageAverageTime;
			uint[] array = new uint[4] { 0u, nHightResponse, nMidResponse, 4294967295u };
			if (ServerResponseTime.Length != array.Length - 1)
			{
				return;
			}
			for (int j = 0; j < ServerResponseTime.Length; j++)
			{
				if (array[j] < packageAverageTime && packageAverageTime < array[j + 1])
				{
					ServerResponseTime[j].transform.parent.gameObject.SetActive(true);
					ServerResponseTime[j].text = string.Format("{0} ms", packageAverageTime);
					if (LockStepController.LockStepPause)
					{
						_lockStepWaitingDot++;
						string text = "Waiting.";
						ServerResponseTime[j].text = text.PadRight(text.Length + _lockStepWaitingDot / 10 % 6, '.');
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < ServerResponseTime.Length; k++)
			{
				ServerResponseTime[ServerResponseTime.Length - 1].transform.parent.gameObject.SetActive(true);
				ServerResponseTime[ServerResponseTime.Length - 1].text = "Disconnect!!";
			}
		}
	}

	public void CloseBattleInfoUIAllUI()
	{
		if (tKillActivityUI != null)
		{
			foreach (ScoreUIBase value in tKillActivityUI.Values)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
			tKillActivityUI.Clear();
			tKillActivityUI = null;
		}
		if (tBattleScoreUI != null)
		{
			foreach (ScoreUIBase value2 in tBattleScoreUI.Values)
			{
				UnityEngine.Object.Destroy(value2.gameObject);
			}
			tBattleScoreUI.Clear();
			tBattleScoreUI = null;
		}
		AlwaysHitMsg.text = "";
		TimeHitMsg.transform.parent.gameObject.SetActive(false);
		CountDownTime.transform.parent.gameObject.SetActive(false);
		LRRangeBar.transform.parent.gameObject.SetActive(false);
		TBRangeBar.transform.parent.gameObject.SetActive(false);
		GetItemText.transform.parent.gameObject.SetActive(false);
		TCDText.transform.parent.gameObject.SetActive(false);
		ContributionNowNum.transform.parent.gameObject.SetActive(false);
		LoveScoreRoot.enabled = false;
		DangerBG.enabled = false;
		FlagBattleBar.enabled = false;
		if ((bool)GoRoot)
		{
			GoRoot.alpha = 0f;
		}
		if ((bool)BattleRoot)
		{
			BattleRoot.alpha = 0f;
		}
	}

	private void BattleInfoUpdate(EventManager.BattleInfoUpdate tBattleInfoUpdate)
	{
		listBattleInfoUpdate.Add(tBattleInfoUpdate.nType);
		if (listBattleInfoUpdate.Count == 1)
		{
			RunShowBattleInfo();
		}
	}

	private void RunShowBattleInfo()
	{
		if (listBattleInfoUpdate.Count > 0)
		{
			switch (listBattleInfoUpdate[0])
			{
			case 1:
				StartCoroutine(ShowXCaleCoroutine());
				break;
			case 2:
				StartCoroutine(ShowMoveArrowCoroutine());
				break;
			default:
				listBattleInfoUpdate.RemoveAt(0);
				RunShowBattleInfo();
				break;
			}
		}
	}

	private IEnumerator ShowXCaleCoroutine()
	{
		if ((tSTAGE_TABLE.n_MAIN == 90000 && tSTAGE_TABLE.n_SUB == 1) || (tSTAGE_TABLE.n_MAIN == 90001 && (tSTAGE_TABLE.n_SUB == 1 || tSTAGE_TABLE.n_SUB == 2)))
		{
			listBattleInfoUpdate.RemoveAt(0);
			RunShowBattleInfo();
			yield break;
		}
		if (BattleRoot.alpha == 1f)
		{
			listBattleInfoUpdate.RemoveAt(0);
			RunShowBattleInfo();
			yield break;
		}
		BattleRoot.alpha = 1f;
		Vector3 vScale = new Vector3(3f, 3f, 1f);
		BattleRoot.transform.localScale = vScale;
		XScale.transform.localScale = Vector3.one;
		while (vScale.x > 0.5f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			vScale.x -= Time.deltaTime * 6f;
			vScale.y = vScale.x;
			BattleRoot.transform.localScale = vScale;
			GoRoot.alpha -= Time.deltaTime * 0.9f;
		}
		BattleRoot.transform.localScale = Vector3.one;
		while (GoRoot.alpha > 0f)
		{
			GoRoot.alpha -= Time.deltaTime * 0.9f;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		GoRoot.alpha = 0f;
		MoveArrow.transform.localPosition = MoveArrowOrignPos;
		nGoHintMode = 2;
		listBattleInfoUpdate.RemoveAt(0);
		RunShowBattleInfo();
	}

	private IEnumerator ShowMoveArrowCoroutine()
	{
		if ((tSTAGE_TABLE.n_MAIN == 90000 && tSTAGE_TABLE.n_SUB == 1) || (tSTAGE_TABLE.n_MAIN == 90001 && (tSTAGE_TABLE.n_SUB == 1 || tSTAGE_TABLE.n_SUB == 2)))
		{
			listBattleInfoUpdate.RemoveAt(0);
			RunShowBattleInfo();
			yield break;
		}
		if (GoRoot.alpha == 1f)
		{
			listBattleInfoUpdate.RemoveAt(0);
			RunShowBattleInfo();
			yield break;
		}
		GoRoot.alpha = 1f;
		Vector3 vScale = new Vector3(3f, 3f, 1f);
		GoRoot.transform.localScale = vScale;
		MoveArrow.transform.localPosition = MoveArrowOrignPos;
		while (vScale.x > 0.5f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			vScale.x -= Time.deltaTime * 6f;
			vScale.y = vScale.x;
			GoRoot.transform.localScale = vScale;
			BattleRoot.alpha -= Time.deltaTime * 0.9f;
		}
		GoRoot.transform.localScale = Vector3.one;
		while (BattleRoot.alpha > 0f)
		{
			BattleRoot.alpha -= Time.deltaTime * 0.9f;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		BattleRoot.alpha = 0f;
		XScale.transform.localScale = Vector3.one;
		nGoHintMode = 1;
		listBattleInfoUpdate.RemoveAt(0);
		RunShowBattleInfo();
	}

	private void EventLockRange(EventManager.LockRangeParam tLockRangeParam)
	{
		if (tLockRangeParam.nMode == 3)
		{
			cameraHHalf = ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
			cameraWHalf = ManagedSingleton<StageHelper>.Instance.fCameraWHalf;
		}
	}

	public void SwitchStageCountDownTime(bool bSwitch)
	{
		if (bSwitch)
		{
			if (nCountDownTimeMode > 996)
			{
				nCountDownTimeMode = 999 - nCountDownTimeMode;
			}
		}
		else if (nCountDownTimeMode < 996)
		{
			nCountDownTimeMode = 999 - nCountDownTimeMode;
		}
	}

	public void ShowStageCountDownTime(float fNowTime = 0f)
	{
		if (tSTAGE_TABLE.n_TIME > 0)
		{
			if ((!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp || (tSTAGE_TABLE.n_MAIN == 90001 && tSTAGE_TABLE.n_SUB == 3)) && nCountDownTimeMode != 999)
			{
				nStageTimeCountDown = (float)tSTAGE_TABLE.n_TIME - fNowTime;
				if (nStageTimeCountDown <= 0f)
				{
					nStageTimeCountDown = 0.01f;
				}
				TimeSpan timeSpan = TimeSpan.FromSeconds(nStageTimeCountDown);
				nCountDownTimeMode = 1;
				CountDownTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
				FlagBattleTime.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
				if (tSTAGE_TABLE.n_MAIN == 90001 && tSTAGE_TABLE.n_SUB == 3)
				{
					CountDownTime.transform.parent.gameObject.SetActive(false);
				}
				else
				{
					CountDownTime.transform.parent.gameObject.SetActive(true);
				}
				StageUpdate.SyncStageObj(3, 4, (fNowTime - MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime).ToString());
			}
		}
		else
		{
			CountDownTime.transform.parent.gameObject.SetActive(false);
		}
	}

	public void ShowStageStartTimer(float fLimitTime = 0f, float fNowTime = 0f)
	{
		if (nCountDownTimeMode != 999)
		{
			nStageTimeCountDown = fNowTime;
			fCountDownTimeModeParam = fLimitTime;
			TimeSpan timeSpan = TimeSpan.FromSeconds(nStageTimeCountDown);
			nCountDownTimeMode = 2;
			CountDownTime.text = string.Format("{0}.{1:D2}", Mathf.FloorToInt((float)timeSpan.TotalSeconds), timeSpan.Milliseconds / 10);
			FlagBattleTime.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
			CountDownTime.transform.parent.gameObject.SetActive(true);
			StageUpdate.SyncStageObj(3, 4, fCountDownTimeModeParam + "," + nStageTimeCountDown);
		}
	}

	public void AddStageTimerTime(float fAddTime)
	{
		if (nCountDownTimeMode == 1)
		{
			nStageTimeCountDown += fAddTime;
		}
		else if (nCountDownTimeMode == 2)
		{
			nStageTimeCountDown -= fAddTime;
		}
		foreach (EventPointBase item in StageUpdate.listAllEvent)
		{
			if (item.GetTypeID() == 10)
			{
				MapObjEvent mapObjEvent = item as MapObjEvent;
				if (mapObjEvent.mapEvent == MapObjEvent.MapEventEnum.COUNTDOWN_EVENT)
				{
					mapObjEvent.AddCountDownEventTime(fAddTime);
				}
			}
		}
	}

	public void ReStageCountDownTime()
	{
		if (!CountDownTime.transform.parent.gameObject.activeSelf && !FlagBattleBar.enabled)
		{
			return;
		}
		if (nCountDownTimeMode == 1)
		{
			if (tSTAGE_TABLE.n_TIME > 0)
			{
				StageUpdate.SyncStageObj(3, 4, ((float)tSTAGE_TABLE.n_TIME - nStageTimeCountDown - MonoBehaviourSingleton<StageSyncManager>.Instance.HostAvgDelayTime).ToString());
			}
		}
		else if (nCountDownTimeMode == 2)
		{
			StageUpdate.SyncStageObj(3, 4, fCountDownTimeModeParam + "," + nStageTimeCountDown);
		}
	}

	public void ShowTeleportCD(float fWaitTime, Vector3? tPos)
	{
		vTeleportPos = tPos;
		if (!(TCDText != null))
		{
			return;
		}
		if (fWaitTime <= 0f)
		{
			TCDText.transform.parent.gameObject.SetActive(false);
			TPCountDownSEPhase = 3;
			return;
		}
		TCDText.transform.parent.gameObject.SetActive(true);
		int num = Mathf.RoundToInt(fWaitTime);
		TCDText.text = num.ToString();
		if ((int)OC.Hp > 0)
		{
			CountDownSE(num, ref TPCountDownSEPhase, 2);
		}
	}

	public bool IsTeloportIng()
	{
		return TCDText.transform.parent.gameObject.activeSelf;
	}

	public Vector3? GetTeleportPos()
	{
		return vTeleportPos;
	}

	public void ShowGetItemUI()
	{
		GetItemText.transform.parent.gameObject.SetActive(true);
		GetItemText.text = "<color=#02E9FF>X " + nBattleScore + "</color>";
	}

	public void ShowBattleScoreUI(int nID = 0)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("ui/battleinfo/BattleScoreRoot" + nID, "BattleScoreRoot" + nID, delegate(GameObject asset)
		{
			if (!(asset == null))
			{
				if (tBattleScoreUI != null)
				{
					if (tBattleScoreUI.ContainsKey(nID))
					{
						UnityEngine.Object.Destroy(tBattleScoreUI[nID].gameObject);
						tBattleScoreUI.Remove(nID);
					}
				}
				else
				{
					tBattleScoreUI = new Dictionary<int, ScoreUIBase>();
				}
				ScoreUIBase component = UnityEngine.Object.Instantiate(asset, StandObj.transform).GetComponent<ScoreUIBase>();
				component.Init();
				tBattleScoreUI.Add(nID, component);
			}
		});
	}

	public void ShowLoveScoreUI(int nType = 0, bool bShowHeart = true, bool bShowCandy = false)
	{
		LoveScoreRoot.enabled = true;
		if (!DicCountScore.ContainsKey(1))
		{
			DicCountScore.Add(1, 0);
		}
		CandyImg.CheckLoadT<Sprite>("ui/ui_battleinfo", "UI_battle_event_icon_item" + nType.ToString("00"), delegate
		{
			CandyImg.SetNativeSize();
		});
		GetHeartText.transform.parent.gameObject.SetActive(bShowHeart);
		GetCandyText.transform.parent.gameObject.SetActive(bShowCandy);
		GetCandyText.text = "X" + DicCountScore[1];
		GetHeartText.text = "X" + nBattleScore;
	}

	public void ShowContributionNowNum()
	{
		ContributionNowNum.transform.parent.gameObject.SetActive(true);
		ContributionNowNum.text = "0";
		StartCoroutine(UpdateContributionNowNum());
	}

	private IEnumerator UpdateContributionNowNum()
	{
		int nNowShowNum = 0;
		if (StageResManager.GetStageUpdate() == null)
		{
			yield break;
		}
		while (true)
		{
			yield return CoroutineDefine._0_3sec;
			int mainPlayerPower = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower();
			while (nNowShowNum < mainPlayerPower)
			{
				if (mainPlayerPower - nNowShowNum < 10)
				{
					nNowShowNum = mainPlayerPower;
				}
				nNowShowNum += (mainPlayerPower - nNowShowNum) / 10;
				if (mainPlayerPower < nNowShowNum)
				{
					nNowShowNum = mainPlayerPower;
				}
				ContributionNowNum.text = nNowShowNum.ToString();
				CheckShowNumLimit(ref nNowShowNum);
				yield return CoroutineDefine._waitForEndOfFrame;
				mainPlayerPower = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower();
			}
			CheckShowNumLimit(ref nNowShowNum);
		}
	}

	private void CheckShowNumLimit(ref int nNowShowNum)
	{
		switch (tSTAGE_TABLE.n_TYPE)
		{
		case 9:
			if (nNowShowNum >= OrangeConst.RAID_DAMAGE_LIMIT)
			{
				nNowShowNum = OrangeConst.RAID_DAMAGE_LIMIT;
				ContributionNowNum.text = nNowShowNum.ToString();
				ContributionNowNum.color = Color.red;
			}
			break;
		case 10:
			if (nNowShowNum >= OrangeConst.GUILD_CRUSADE_LIMIT)
			{
				nNowShowNum = OrangeConst.GUILD_CRUSADE_LIMIT;
				ContributionNowNum.text = nNowShowNum.ToString();
				ContributionNowNum.color = Color.red;
			}
			break;
		}
	}

	public int GetCountScore(int nID)
	{
		if (DicCountScore.ContainsKey(nID))
		{
			return DicCountScore[nID];
		}
		return 0;
	}

	public void UpdateCountScoreUI(int nID, int nGetScore)
	{
		if (DicCountScore.ContainsKey(nID))
		{
			DicCountScore[nID] += nGetScore;
		}
		else
		{
			DicCountScore.Add(nID, nGetScore);
		}
		int num = nBattleScore;
		switch (nID)
		{
		case 0:
			GetItemText.text = "<color=#02E9FF>X " + num + "</color>";
			GetHeartText.text = "X" + num;
			break;
		case 1:
			GetCandyText.text = "X" + DicCountScore[1];
			break;
		}
	}

	private void CheckStageSuccess()
	{
		nCountDownTimeMode = 999;
		switch (tSTAGE_TABLE.n_TYPE)
		{
		case 1:
			ShowStageLostByContinueRoot();
			break;
		case 6:
			ShowStageLostByContinueRoot();
			break;
		case 2:
			ShowStageLostByContinueRoot();
			break;
		case 3:
			ShowStageLostByContinueRoot();
			break;
		case 4:
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
			break;
		case 5:
			ShowStageLostByContinueRoot();
			break;
		case 8:
			ShowStageLostByContinueRoot();
			break;
		case 1001:
			if (tSTAGE_TABLE.n_MAIN == 90001 && tSTAGE_TABLE.n_SUB == 3)
			{
				if (nTeamValue[MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam() - 1] == nTeamValue[1 - (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam() - 1)])
				{
					BattleEnd(2);
				}
				else if (nTeamValue[MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam() - 1] >= nTeamValue[1 - (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam() - 1)])
				{
					BattleEnd(1);
				}
				else
				{
					BattleEnd(0);
				}
			}
			break;
		case 9:
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
			break;
		case 10:
			if (StageUpdate.StageMode == StageMode.Contribute)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
			}
			else
			{
				ShowStageLostByContinueRoot();
			}
			break;
		default:
			ShowStageLostByContinueRoot();
			break;
		}
	}

	public void ShowEnemyHint()
	{
		if (!bShowEnemyHint)
		{
			bShowEnemyHint = true;
			StartCoroutine(ShowEnemyHintCoroutine());
		}
	}

	private IEnumerator ShowEnemyHintCoroutine()
	{
		OrangeCharacter tOC = null;
		while (tOC == null)
		{
			tOC = StageUpdate.GetPlayerByID(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		Vector2 UISize = ((RectTransform)base.transform).rect.max;
		while (bShowEnemyHint)
		{
			if (!StageUpdate.gbStageReady)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				continue;
			}
			Vector3 position = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position;
			int i = 0;
			for (int num = StageUpdate.runEnemys.Count - 1; num >= 0; num--)
			{
				if (((uint)StageUpdate.runEnemys[num].nEnemyBitParam & (true ? 1u : 0u)) != 0 && StageUpdate.runEnemys[num].mEnemy.IsAlive())
				{
					Vector3 centerPos = StageUpdate.runEnemys[num].mEnemy.Controller.GetCenterPos();
					centerPos -= position;
					if (Mathf.Abs(centerPos.x) - StageUpdate.runEnemys[num].mEnemy.Controller.Collider2D.bounds.extents.x > cameraWHalf || Mathf.Abs(centerPos.y) - StageUpdate.runEnemys[num].mEnemy.Controller.Collider2D.bounds.extents.y > cameraHHalf)
					{
						if (listEnemyHintObjs.Count <= i)
						{
							GameObject tObj = UnityEngine.Object.Instantiate(listEnemyHintObjs[0].root, listEnemyHintObjs[0].root.transform.parent);
							listEnemyHintObjs.Add(new EnemyHintObj(tObj));
						}
						Vector2 to = centerPos;
						to.Normalize();
						float num2 = Mathf.Sign(0f - Vector2.down.y * to.x);
						float z = Vector2.Angle(Vector2.down, to) * num2;
						float num3 = float.MaxValue;
						if (to.x != 0f)
						{
							num3 = UISize.x / to.x;
						}
						float num4 = float.MaxValue;
						if (to.y != 0f)
						{
							num4 = UISize.y / to.y;
						}
						if (Mathf.Abs(num3) > Mathf.Abs(num4))
						{
							to *= num4;
						}
						else
						{
							to *= num3;
						}
						listEnemyHintObjs[i].root.transform.localRotation = Quaternion.Euler(0f, 0f, z);
						listEnemyHintObjs[i].HintImgTrans.localPosition = new Vector3(0f, 0f - to.magnitude + 65f, 0f);
						listEnemyHintObjs[i].BossHintImgTrans.localPosition = new Vector3(0f, 0f - to.magnitude + 65f, 0f);
						if ((StageUpdate.runEnemys[num].nEnemyBitParam & 2) == 0)
						{
							listEnemyHintObjs[i].SetMode(0);
						}
						else
						{
							listEnemyHintObjs[i].SetMode(1);
						}
						listEnemyHintObjs[i].root.SetActive(true);
						i++;
					}
				}
			}
			for (; i < listEnemyHintObjs.Count; i++)
			{
				listEnemyHintObjs[i].root.SetActive(false);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	public void ShowExplodeBG(GameObject targetGameObject, bool bshowEndPos = true, bool bNeedStageEnd = true)
	{
		ExploreEndBG.color = Color.clear;
		CanvasExploreEndBG.enabled = true;
		StageUpdate.UnSlowStage();
		float exploreTime = 4f;
		if (NowStageTable != null)
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (NowStageTable.n_TYPE == 8 && stageUpdate != null && stageUpdate.bIsHaveEventStageEnd)
			{
				exploreTime = 2f;
			}
		}
		StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, exploreTime, delegate(float f)
		{
			ExploreEndBG.color = new Color(f, f, f, f);
		}, delegate
		{
			if (targetGameObject != null)
			{
				EnemyControllerBase component = targetGameObject.GetComponent<EnemyControllerBase>();
				if ((bool)component)
				{
					component.SetActive(false);
				}
				else
				{
					targetGameObject.SetActive(false);
				}
			}
			if (!bshowEndPos && bNeedStageEnd)
			{
				StartCoroutine(CheckMediumBossBGM());
			}
			StartCoroutine(StageResManager.TweenFloatCoroutine(1f, 0f, exploreTime, delegate(float f)
			{
				ExploreEndBG.color = new Color(f, f, f, f);
			}, delegate
			{
				CanvasExploreEndBG.enabled = false;
				if (targetGameObject != null)
				{
					EnemyControllerBase component2 = targetGameObject.GetComponent<EnemyControllerBase>();
					if ((bool)component2)
					{
						component2.DeadPlayCompleted = true;
						if (ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(component2.EnemyData))
						{
							StageUpdate stageUpdate2 = StageResManager.GetStageUpdate();
							if (stageUpdate2 != null && stageUpdate2.bIsHaveEventStageEnd)
							{
								Instance.SwitchOptionBtn(true);
							}
						}
					}
				}
				if (bshowEndPos)
				{
					if (tTalkEnd == null)
					{
						tTalkEnd = StartCoroutine(TalkEnd());
					}
				}
				else if (bNeedStageEnd && tExploreStageEnd == null)
				{
					tExploreStageEnd = StartCoroutine(ExploreStageEnd());
				}
			}));
		}));
	}

	private IEnumerator ExploreStageEnd()
	{
		while (TurtorialUI.IsTutorialing())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (StageUpdate.GetMainPlayerOC() != null && StageUpdate.GetMainPlayerOC().IsDead())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		while (StageResManager.GetStageUpdate().nRunStageCtrlCount > 0)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (StageUpdate.GetMainPlayerOC() != null && StageUpdate.GetMainPlayerOC().IsDead())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
		tExploreStageEnd = null;
	}

	private IEnumerator TalkEnd()
	{
		while (TurtorialUI.IsTutorialing())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (StageUpdate.GetMainPlayerOC() != null && StageUpdate.GetMainPlayerOC().IsDead())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		while (StageResManager.GetStageUpdate().nRunStageCtrlCount > 0)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (StageUpdate.GetMainPlayerOC() != null && StageUpdate.GetMainPlayerOC().IsDead())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
		{
			runPlayer.ClosePadAndPlayerUI();
			runPlayer.LockControl();
		}
		if (!isClearPlayFrist)
		{
			Vector3 position = StageUpdate.GetMainPlayerOC().transform.position;
			position.y += StageUpdate.GetMainPlayerOC().Controller.Collider2D.bounds.size.y * cameraZoomInRate;
			EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
			stageCameraFocus.nMode = 1;
			stageCameraFocus.roominpos = position;
			stageCameraFocus.fRoomInTime = cameraZoomInTime;
			stageCameraFocus.fRoomOutTime = -1f;
			stageCameraFocus.fRoomInFov = 9f;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			OC.UseTeleportFollowCamera = true;
			yield return new WaitForSeconds(1f);
			CheckStartStageClear();
		}
		else
		{
			CheckStartStageClear(delegate
			{
				Vector3 position2 = StageUpdate.GetMainPlayerOC().transform.position;
				position2.y += StageUpdate.GetMainPlayerOC().Controller.Collider2D.bounds.size.y;
				EventManager.StageCameraFocus p_param = new EventManager.StageCameraFocus
				{
					nMode = 1,
					roominpos = position2,
					fRoomInTime = 1f,
					fRoomOutTime = -1f,
					fRoomInFov = 9f
				};
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, p_param);
				OC.UseTeleportFollowCamera = true;
			});
		}
		tTalkEnd = null;
	}

	public void ShowExplodeWhite()
	{
		CanvasExploreEndBG.enabled = true;
		ExploreEndBG.color = new Color(0f, 0f, 0f, 0f);
		StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, 2f, delegate(float f)
		{
			ExploreEndBG.color = new Color(f, f, f, f);
		}, null));
	}

	public void CloseExplodeWhite()
	{
		CanvasExploreEndBG.enabled = true;
		ExploreEndBG.color = new Color(1f, 1f, 1f, 1f);
		StartCoroutine(StageResManager.TweenFloatCoroutine(1f, 0f, 2f, delegate(float f)
		{
			ExploreEndBG.color = new Color(f, f, f, f);
		}, delegate
		{
			CanvasExploreEndBG.enabled = false;
		}));
	}

	public void ShowExplodeBlack()
	{
		CanvasExploreEndBG.enabled = true;
		ExploreEndBG.color = new Color(0f, 0f, 0f, 0f);
		StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, 2f, delegate(float f)
		{
			ExploreEndBG.color = new Color(0f, 0f, 0f, f);
		}, null));
	}

	public void CloseExplodeBlack()
	{
		CanvasExploreEndBG.enabled = true;
		ExploreEndBG.color = new Color(0f, 0f, 0f, 1f);
		StartCoroutine(StageResManager.TweenFloatCoroutine(1f, 0f, 2f, delegate(float f)
		{
			ExploreEndBG.color = new Color(0f, 0f, 0f, f);
		}, delegate
		{
			CanvasExploreEndBG.enabled = false;
		}));
	}

	public void HurtCBPlayer(StageObjBase tSOB)
	{
		OrangeCharacter orangeCharacter = tSOB as OrangeCharacter;
		if ((int)orangeCharacter.Hp <= 0)
		{
			orangeCharacter.HurtActions -= HurtCBPlayer;
			for (int i = 0; i < listPlayinfoOC.Count; i++)
			{
				if (listPlayinfoOC[i].tOC.GetInstanceID() != orangeCharacter.GetInstanceID())
				{
					continue;
				}
				int nIndex = listPlayinfoOC[i].nIndex;
				listPlayinfoOC[i].fContinueTime = OrangeConst.CORP_AUTO_CONTINUE;
				listPlayinfoOC[i].fTimeLastAutoBorn = Time.realtimeSinceStartup;
				playerhp[nIndex].fillAmount = 0f;
				if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
				{
					if (StageUpdate.gbIsNetGame)
					{
						BgMaskImg[nIndex].SetActive(true);
						reborntime[nIndex].text = OrangeConst.CORP_AUTO_CONTINUE.ToString("0");
					}
					else
					{
						BgMaskImg[nIndex].SetActive(false);
						reborntime[nIndex].text = "";
					}
					if (listPlayinfoOC[i].tOC.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
					{
						StartCoroutine(PlayerContinueCoroutine(listPlayinfoOC[i]));
					}
				}
				else if (tSTAGE_TABLE.n_MAIN == 90001 && tSTAGE_TABLE.n_SUB == 3)
				{
					BgMaskImg[nIndex].SetActive(true);
					reborntime[nIndex].text = OrangeConst.PVP_3VS3_CONTINUE_TIME.ToString();
					StartCoroutine(WaitAndReBorn(i, listPlayinfoOC[i].tOC, OrangeConst.PVP_3VS3_CONTINUE_TIME));
				}
				else
				{
					reborntime[nIndex].text = "";
					BgMaskImg[nIndex].SetActive(false);
				}
				break;
			}
			return;
		}
		for (int j = 0; j < listPlayinfoOC.Count && j < PlayerInfo.Length; j++)
		{
			if (listPlayinfoOC[j].tOC.GetInstanceID() == orangeCharacter.GetInstanceID())
			{
				int nIndex2 = listPlayinfoOC[j].nIndex;
				playerhp[nIndex2].fillAmount = (float)(int)orangeCharacter.Hp / (float)(int)orangeCharacter.MaxHp;
				if ((int)orangeCharacter.Hp > 0 && playerhp[nIndex2].fillAmount < 0.01f)
				{
					playerhp[nIndex2].fillAmount = 0.01f;
				}
				break;
			}
		}
	}

	private IEnumerator WaitAndReBorn(int nIndex, OrangeCharacter oc, float fTime)
	{
		float fNowWaitTime2 = fTime;
		while (fNowWaitTime2 > 0f)
		{
			reborntime[nIndex].text = fNowWaitTime2.ToString("0");
			yield return CoroutineDefine._waitForEndOfFrame;
			if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				fNowWaitTime2 -= Time.deltaTime;
			}
		}
		if (oc.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.RebornPlayer(oc);
			yield break;
		}
		fNowWaitTime2 = OrangeConst.PVP_3VS3_CONTINUE_TIME;
		while ((int)oc.Hp <= 0)
		{
			if (fNowWaitTime2 <= 0f && oc.bNeedUpdateAlways)
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.RebornPlayer(oc);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
			if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				fNowWaitTime2 -= Time.deltaTime;
			}
		}
	}

	private IEnumerator PlayerContinueCoroutine(PlayinfoOC tPlayinfoOC)
	{
		if (!(tPlayinfoOC.tOC.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify))
		{
			while (tPlayinfoOC.fContinueTime > 0f)
			{
				yield return new WaitForSeconds(1f);
				fDTime = Time.realtimeSinceStartup - tPlayinfoOC.fTimeLastAutoBorn;
				tPlayinfoOC.fTimeLastAutoBorn = Time.realtimeSinceStartup;
				tPlayinfoOC.fContinueTime -= fDTime;
				reborntime[tPlayinfoOC.nIndex].text = tPlayinfoOC.fContinueTime.ToString("0");
			}
		}
	}

	public void AddOrangeCharacter(OrangeCharacter tOC)
	{
		for (int i = 0; i < listPlayinfoOC.Count; i++)
		{
			if (listPlayinfoOC[i].tOC.GetInstanceID() == tOC.GetInstanceID())
			{
				int nIndex = listPlayinfoOC[i].nIndex;
				playerhp[nIndex].fillAmount = (float)(int)tOC.Hp / (float)(int)tOC.MaxHp;
				BgMaskImg[nIndex].SetActive(false);
				listPlayinfoOC[i].fContinueTime = 0f;
				tOC.HurtActions += HurtCBPlayer;
				return;
			}
			if (listPlayinfoOC[i].tOC.sPlayerID == tOC.sPlayerID)
			{
				RelpaceOrangeCharacter(listPlayinfoOC[i].tOC, tOC);
				return;
			}
		}
		if (listPlayinfoOC.Count < PlayerInfo.Length)
		{
			tOC.HurtActions += HurtCBPlayer;
			int count = listPlayinfoOC.Count;
			PlayinfoOC playinfoOC = new PlayinfoOC();
			playinfoOC.tOC = tOC;
			listPlayinfoOC.Add(playinfoOC);
			UpdatePlayerInfo();
		}
	}

	public void RelpaceOrangeCharacter(OrangeCharacter tOldOC, OrangeCharacter tNewOC)
	{
		for (int i = 0; i < listPlayinfoOC.Count; i++)
		{
			if (listPlayinfoOC[i].tOC.GetInstanceID() == tOldOC.GetInstanceID())
			{
				tOldOC.HurtActions -= HurtCBPlayer;
				tNewOC.HurtActions += HurtCBPlayer;
				listPlayinfoOC[i].tOC = tNewOC;
				UpdatePlayerInfo();
				break;
			}
		}
	}

	public void RemoveOrangeCharacter(OrangeCharacter tOC)
	{
		for (int num = listPlayinfoOC.Count - 1; num >= 0; num--)
		{
			if (listPlayinfoOC[num].tOC.GetInstanceID() == tOC.GetInstanceID())
			{
				tOC.HurtActions -= HurtCBPlayer;
				listPlayinfoOC[num].tOC = null;
				listPlayinfoOC.RemoveAt(num);
				break;
			}
		}
		UpdatePlayerInfo();
	}

	public void UpdatePlayerInfo()
	{
		int num = 0;
		int num2 = 1;
		for (num = 0; num < PlayerInfo.Length; num++)
		{
			PlayerInfo[num].enabled = false;
		}
		for (num = 0; num < listPlayinfoOC.Count && num < PlayerInfo.Length; num++)
		{
			OrangeCharacter tOC = listPlayinfoOC[num].tOC;
			if (tOC.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				listPlayinfoOC[num].nIndex = 0;
			}
			else
			{
				listPlayinfoOC[num].nIndex = num2;
				num2++;
			}
			int nIndex = listPlayinfoOC[num].nIndex;
			PlayerInfo[num].enabled = true;
			SKIN_TABLE value = null;
			if (tOC.SetPBP.CharacterSkinID != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(tOC.SetPBP.CharacterSkinID, out value))
			{
				headicon[nIndex].CheckLoad(AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + value.s_ICON), "icon_" + value.s_ICON);
			}
			else
			{
				headicon[nIndex].CheckLoad(AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + tOC.CharacterData.s_ICON), "icon_" + tOC.CharacterData.s_ICON);
			}
			score[nIndex].text = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(tOC.SetPBP.mainWStatus, tOC.SetPBP.subWStatus, tOC.SetPBP.tPlayerStatus + tOC.SetPBP.chipStatus).ToString();
			playerhp[nIndex].fillAmount = (float)(int)tOC.Hp / (float)(int)tOC.MaxHp;
			BgMaskImg[nIndex].SetActive(false);
		}
	}

	public void UpdateHost(bool bHost)
	{
		UpdatePlayerInfo();
	}

	public void ShowFlagModeUI(int nNeedValue = 500)
	{
		nFlagWinValue = nNeedValue;
		if (!FlagBattleBar.enabled)
		{
			FlagBattleBar.enabled = true;
			for (int i = 0; i < nTeamValue.Length; i++)
			{
				nTeamValue[i] = nFlagWinValue;
			}
			LeftBar.fillAmount = 0.5f;
		}
	}

	public void ReSendSyncFlagScore()
	{
		if (FlagBattleBar.enabled && StageUpdate.bIsHost)
		{
			StageUpdate.SyncStageObj(3, 12, nTeamValue[0] + "," + nTeamValue[1] + "," + nFlagWinValue);
		}
	}

	public void NetSetFlagScore(int nTV0, int nTV1, int nWinValue)
	{
		ShowFlagModeUI(nWinValue);
		nTeamValue[0] = nTV0;
		nTeamValue[1] = nTV1;
		LeftBar.fillAmount = (float)nTeamValue[0] / ((float)nFlagWinValue * 2f);
		if (nTeamValue[0] > nFlagWinValue)
		{
			LeftValue.text = (nTeamValue[0] - nFlagWinValue).ToString();
		}
		else
		{
			LeftValue.text = "0";
		}
		if (nTeamValue[1] > nFlagWinValue)
		{
			RightValue.text = (nTeamValue[1] - nFlagWinValue).ToString();
		}
		else
		{
			RightValue.text = "0";
		}
		if (nTeamValue[0] >= nFlagWinValue * 2)
		{
			BattleEnd((1 == MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam()) ? 1 : 0);
		}
		if (nTeamValue[1] >= nFlagWinValue * 2)
		{
			BattleEnd((2 == MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam()) ? 1 : 0);
		}
	}

	public void UpdatePlayerInFlagRange(int nTeam, int nValue)
	{
		if (nTeam > 0)
		{
			nTeamValue[nTeam - 1] += nValue;
			nTeamValue[1 - (nTeam - 1)] -= nValue;
			LeftBar.fillAmount = (float)nTeamValue[0] / ((float)nFlagWinValue * 2f);
			if (nTeamValue[0] > nFlagWinValue)
			{
				LeftValue.text = (nTeamValue[0] - nFlagWinValue).ToString();
			}
			else
			{
				LeftValue.text = "0";
			}
			if (nTeamValue[1] > nFlagWinValue)
			{
				RightValue.text = (nTeamValue[1] - nFlagWinValue).ToString();
			}
			else
			{
				RightValue.text = "0";
			}
			if (nTeamValue[nTeam - 1] >= nFlagWinValue * 2)
			{
				BattleEnd((nTeam == MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerTeam()) ? 1 : 0);
			}
		}
	}

	private void ReadyGoEndNotify()
	{
		MonoBehaviourSingleton<LegionManager>.Instance.callLight(true, 65280);
		SwitchOptionBtn(true);
	}

	public void SwitchOptionBtn(bool bShow, int nIndex = 0)
	{
		if (StageResManager.GetStageUpdate().IsEnd)
		{
			bShow = false;
		}
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
		{
			bShowOption[nIndex] = bShow;
		}
		else
		{
			bShowOption[nIndex] = false;
		}
		option.SetActive(bShowOption[0] && bShowOption[1] && bShowOption[2]);
	}

	public void CloseShowCountDown()
	{
		if (CountDownTime.transform.parent.gameObject.activeSelf)
		{
			CountDownTime.transform.parent.gameObject.SetActive(false);
		}
	}

	public void ShowCountDown(float fV)
	{
		CountDownTime.text = fV.ToString("0.00").Replace('.', ':');
		if (!CountDownTime.transform.parent.gameObject.activeSelf)
		{
			CountDownTime.transform.parent.gameObject.SetActive(true);
		}
	}

	public void ShowKillScoreUI(int nTotalScore = 10000, int nUIID = 0, Callback<ScoreUIBase> cb = null)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("ui/battleinfo/KillActivityRoot" + nUIID, "KillActivityRoot" + nUIID, delegate(GameObject asset)
		{
			if (!(asset == null))
			{
				if (tKillActivityUI != null)
				{
					if (tKillActivityUI.ContainsKey(nUIID))
					{
						UnityEngine.Object.Destroy(tKillActivityUI[nUIID].gameObject);
						tKillActivityUI.Remove(nUIID);
					}
				}
				else
				{
					tKillActivityUI = new Dictionary<int, ScoreUIBase>();
				}
				ScoreUIBase component = UnityEngine.Object.Instantiate(asset, StandObj.transform).GetComponent<ScoreUIBase>();
				component.Init();
				if (cb != null)
				{
					cb(component);
				}
				tKillActivityUI.Add(nUIID, component);
			}
		});
		nCampaignTotalScore = nTotalScore;
	}

	public void RemoveKillScoreUI(int nUIID = 0)
	{
		if (tKillActivityUI != null)
		{
			if (tKillActivityUI.ContainsKey(nUIID))
			{
				UnityEngine.Object.Destroy(tKillActivityUI[nUIID].gameObject);
				tKillActivityUI.Remove(nUIID);
			}
			if (tKillActivityUI.Count == 0)
			{
				tKillActivityUI = null;
			}
		}
	}

	public void UpdateKillScoreUI(int nGetScore)
	{
		if (nCampaignTotalScore > 0)
		{
			nCampaignScore += nGetScore;
		}
	}

	public void BattleEnd(int nWinType)
	{
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if (!StageUpdate.gbStageReady || stageUpdate.IsEnd)
		{
			return;
		}
		PvpReportUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<PvpReportUI>("UI_PvpReport");
		if ((bool)uI)
		{
			uI.SetCanvasAlpha(0f);
		}
		StageUpdate.StopStageAllEvent();
		MonoBehaviourSingleton<UpdateManager>.Instance.Pause = true;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.Stop();
		MonoBehaviourSingleton<InputManager>.Instance.gamepadDetachedEvent -= TryPause;
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		foreach (PlayinfoOC item in listPlayinfoOC)
		{
			item.tOC.StopAllLoopSE();
		}
		LeanTween.value(0.1f, 1f, 2f).setOnUpdate(delegate(float f)
		{
			Time.timeScale = f;
		}).setOnComplete((Action)delegate
		{
			if (StageUpdate.bIsHost)
			{
				bool bIsSeason = false;
				STAGE_TABLE value;
				if (Instance != null)
				{
					bIsSeason = Instance.NowStageTable.n_MAIN == 90000 && Instance.NowStageTable.n_SUB == 1;
				}
				else if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0 && ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value))
				{
					bIsSeason = value.n_MAIN == 90000 && value.n_SUB == 1;
				}
				MonoBehaviourSingleton<StageSyncManager>.Instance.HostSendBattleEndToOther(nWinType, bIsSeason);
				MonoBehaviourSingleton<StageSyncManager>.Instance.SendStageEndAndOpenPvpEndUI(nWinType, bIsSeason, () =>
				{
					base.OnClickCloseBtn();
				});
			}
			else
			{
				_003C_003En__0();
				MonoBehaviourSingleton<StageSyncManager>.Instance.RequestGetBattleEndMsg();
			}
		})
			.setIgnoreTimeScale(true);
	}

	private IEnumerator ShowAlphaCoroutine(CanvasGroup tObj, float fTime = 1f)
	{
		float fDA = 1f / fTime;
		while (tObj.alpha < 1f)
		{
			tObj.alpha += fDA * Time.deltaTime;
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		tObj.alpha = 1f;
	}

	public void ShowContinueSelect()
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp || ContinueRoot.enabled)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Pause(AudioChannelType.Sound);
		SwitchOptionBtn(false, 1);
		ExitButton.interactable = true;
		ReButton.interactable = true;
		ContinueRoot.enabled = true;
		CanvasGroup component = ContinueRoot.GetComponent<CanvasGroup>();
		if (component != null)
		{
			component.alpha = 0f;
			StartCoroutine(ShowAlphaCoroutine(component, 1.5f));
		}
		for (int i = 0; i < ContinueSubRoot.Length; i++)
		{
			ContinueSubRoot[i].SetActive(false);
		}
		ITEM_TABLE value;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(OrangeConst.ITEMID_CONTINUE, out value))
		{
			value = new ITEM_TABLE();
		}
		int num = 0;
		ItemInfo value2;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(OrangeConst.ITEMID_CONTINUE, out value2))
		{
			num = value2.netItemInfo.Stack;
		}
		int cORP_AUTO_CONTINUE = OrangeConst.CORP_AUTO_CONTINUE;
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
		for (int j = 0; j < ContinueCost.Length; j++)
		{
			ContinueCost[j].text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_CONTINUE_CONTENT"), l10nValue);
			ContinueItemIcon[j].CheckLoad(AssetBundleScriptableObject.Instance.GetIconItem(value.s_ICON), value.s_ICON);
			ContinueItemText[j].text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SHOW_ITEM_COUNT"), num);
		}
		ContinueSubRoot[0].transform.Find("ContinueCost").GetComponent<Text>().text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_CONTINUE_CONTENT_FREE"), OrangeConst.FREE_CONTINUE_RANK, l10nValue);
		ContinueTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BATTLE_CONTINUE_TITLE");
		if (NowStageTable.n_TYPE == 2)
		{
			ShowStageLostByContinueRoot();
		}
		else if (NowStageTable.n_TYPE == 6 || NowStageTable.n_TYPE == 8 || NowStageTable.n_TYPE == 9 || NowStageTable.n_TYPE == 10)
		{
			ShowStageLostByContinueRoot();
		}
		else if (StageUpdate.gbGeneratePvePlayer)
		{
			fTimeLastAutoBorn = Time.realtimeSinceStartup;
			fContinueTime = OrangeConst.CORP_AUTO_CONTINUE;
			fFailedTime = OrangeConst.CORP_FAILED_TIME;
			if (!StageUpdate.IsAllPlayerDead(true))
			{
				nContinueMode = 2;
			}
			else
			{
				nContinueMode = 3;
			}
			ContinueSubRoot[nContinueMode].SetActive(true);
			if (num > 0)
			{
				ReButton.interactable = true;
			}
			else
			{
				ReButton.interactable = false;
			}
			PowerHintBtn.gameObject.SetActive(false);
			ReButton.gameObject.SetActive(true);
			SelfDeadCountDownPhase = OrangeConst.CORP_AUTO_CONTINUE - 1;
			AllDeadCountDownPhase = OrangeConst.CORP_FAILED_TIME - 1;
			if (tContinueCoroutine != null)
			{
				StopCoroutine(tContinueCoroutine);
			}
			tContinueCoroutine = StartCoroutine(ContinueCoroutine());
		}
		else
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() >= OrangeConst.FREE_CONTINUE_RANK)
			{
				nContinueMode = 1;
				ContinueSubRoot[nContinueMode].SetActive(true);
				if (num > 0)
				{
					ReButton.interactable = true;
				}
				else
				{
					ReButton.interactable = false;
				}
			}
			else
			{
				nContinueMode = 0;
				ContinueSubRoot[nContinueMode].SetActive(true);
			}
			PowerHintBtn.gameObject.SetActive(false);
			if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.Count == 0)
			{
				ExitButton.interactable = false;
			}
			ReButton.gameObject.SetActive(true);
		}
		MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = true;
	}

	public void ContinuePlayGo()
	{
		MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = false;
		ExitButton.interactable = false;
		ReButton.interactable = false;
		MonoBehaviourSingleton<AudioManager>.Instance.Resume(AudioChannelType.Sound);
		nContinueMode = 5;
		if (tContinueCoroutine != null)
		{
			StopCoroutine(tContinueCoroutine);
		}
		tContinueCoroutine = null;
		ContinueRoot.enabled = false;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_countinueSE);
		if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == "LocalPlayer")
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, false, 0f, 0f, null);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
			SwitchOptionBtn(true, 1);
			return;
		}
		if (StageUpdate.gbIsNetGame)
		{
			StartSendRebornMsgCoroutine(1);
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.StageContinueReq(ManagedSingleton<StageHelper>.Instance.nLastStageID, delegate
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, false, 0f, 0f, null);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
			SwitchOptionBtn(true, 1);
		});
	}

	public void CloseContinueUI()
	{
		nContinueMode = 5;
		if (tContinueCoroutine != null)
		{
			StopCoroutine(tContinueCoroutine);
		}
		tContinueCoroutine = null;
		ContinueRoot.enabled = false;
		MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = false;
	}

	private void StartSendRebornMsgCoroutine(int nType)
	{
		if (tContinueCheckHostOutCoroutine != null)
		{
			StopCoroutine(tContinueCheckHostOutCoroutine);
		}
		tContinueCheckHostOutCoroutine = StartCoroutine(SendRebornMsgCoroutine(nType));
	}

	public void CloseSendRebornMsgtCoroutine()
	{
		if (tContinueCheckHostOutCoroutine != null)
		{
			StopCoroutine(tContinueCheckHostOutCoroutine);
			tContinueCheckHostOutCoroutine = null;
		}
	}

	private IEnumerator SendRebornMsgCoroutine(int nType)
	{
		OrangeCharacter tOC = StageUpdate.GetMainPlayerOC();
		if (tOC == null)
		{
			yield break;
		}
		float fTimeLeft = 1f;
		while ((int)tOC.Hp <= 0)
		{
			if (StageUpdate.bIsHost && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayingPlayerCount(true) == 0)
			{
				if (nType == 1)
				{
					ManagedSingleton<PlayerNetManager>.Instance.StageContinueReq(ManagedSingleton<StageHelper>.Instance.nLastStageID, delegate
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, false, 0f, 0f, null);
						Instance.SwitchOptionBtn(true, 1);
						UnRegisterRebornPlayerList(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
					});
				}
				else
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent<string, bool, float, float, bool?>(EventManager.ID.STAGE_CONTINUE_PLATER, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, false, 0f, 0f, null);
					Instance.SwitchOptionBtn(true, 1);
					UnRegisterRebornPlayerList(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
				}
				tContinueCheckHostOutCoroutine = null;
				yield break;
			}
			fTimeLeft += Time.deltaTime;
			if (fTimeLeft >= 1f)
			{
				fTimeLeft = 0f;
				StageUpdate.SyncStageObj(3, 3, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify + ",0," + nType, true);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		tContinueCheckHostOutCoroutine = null;
	}

	public void PowerHintOn()
	{
		switch (NowStageTable.n_TYPE)
		{
		case 9:
			nContinueMode = 5;
			ContinueRoot.enabled = false;
			MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.GUIDE;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			return;
		case 10:
			if (StageUpdate.StageMode == StageMode.Contribute)
			{
				nContinueMode = 5;
				ContinueRoot.enabled = false;
				MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.GUIDE;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
				return;
			}
			break;
		}
		ManagedSingleton<StageHelper>.Instance.ListAchievedMissionID.Clear();
		StageUpdate.StopStageAllEvent();
		MonoBehaviourSingleton<UpdateManager>.Instance.Pause = true;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.Stop();
		StageEndReq stageEndReq = new StageEndReq();
		stageEndReq.StageID = ManagedSingleton<StageHelper>.Instance.nLastStageID;
		stageEndReq.Star = 0;
		stageEndReq.Result = 2;
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.GUIDE;
		stageEndReq.Power = ManagedSingleton<StageHelper>.Instance.nLastOCPower;
		stageEndReq.KillCount = (short)MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerKillEnemyNum();
		stageEndReq.AchievedMissionID = ManagedSingleton<StageHelper>.Instance.ListAchievedMissionID;
		stageEndReq.TowerBossInfoList = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetTowerBossInfoList();
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsStageVaild(stageEndReq.StageID))
		{
			MonoBehaviourSingleton<ACTkManager>.Instance.SetDetected();
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.StageEndReq(stageEndReq, delegate
		{
			OnClickCloseBtn();
			if (StageUpdate.gbIsNetGame)
			{
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQLeaveBattleRoom(MonoBehaviourSingleton<VoiceChatManager>.Instance.GetRoomID));
			}
			else
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.BattleServerLogout();
			}
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.TIP, null, false);
		});
	}

	private IEnumerator ContinueCoroutine()
	{
		while (nContinueMode > 1 && nContinueMode < 4)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			if (nContinueMode <= 1 || nContinueMode >= 4)
			{
				break;
			}
			fDTime = Time.realtimeSinceStartup - fTimeLastAutoBorn;
			fTimeLastAutoBorn = Time.realtimeSinceStartup;
			for (int i = 0; i < listPlayinfoOC.Count; i++)
			{
				if (listPlayinfoOC[i].tOC.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && (int)listPlayinfoOC[i].tOC.Hp > 0)
				{
					nContinueMode = 5;
					ContinueRoot.enabled = false;
					tContinueCoroutine = null;
					yield break;
				}
			}
			if (!StageUpdate.IsAllPlayerDead(true) || listRebornPlayerID.Count > 0)
			{
				if (nContinueMode == 3)
				{
					ContinueSubRoot[nContinueMode].SetActive(false);
					nContinueMode = 2;
					ContinueSubRoot[nContinueMode].SetActive(true);
					SelfDeadCountDownPhase = (int)fContinueTime - 1;
				}
				fContinueTime -= fDTime;
				ContinueCDTime.text = fContinueTime.ToString("0");
				CountDownSE(int.Parse(ContinueCDTime.text), ref SelfDeadCountDownPhase, 2, true);
				for (int j = 0; j < listPlayinfoOC.Count; j++)
				{
					if (listPlayinfoOC[j].tOC.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
					{
						int nIndex = listPlayinfoOC[j].nIndex;
						reborntime[nIndex].text = fContinueTime.ToString("0");
						break;
					}
				}
			}
			else
			{
				if (nContinueMode == 2)
				{
					ContinueSubRoot[nContinueMode].SetActive(false);
					nContinueMode = 3;
					ContinueSubRoot[nContinueMode].SetActive(true);
					fFailedTime = OrangeConst.CORP_FAILED_TIME;
					AllDeadCountDownPhase = (int)fFailedTime - 1;
				}
				if (fFailedTime > 0f)
				{
					fFailedTime -= fDTime;
					fContinueTime -= fDTime;
					if (fFailedTime <= 0f)
					{
						fFailedTime = 0f;
					}
					ContinueFailTime.text = fFailedTime.ToString("0");
					AutoContinueTime.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("AUTO_REBOOT_COUNTDOWN"), fContinueTime.ToString("0"));
					CountDownSE(int.Parse(ContinueFailTime.text), ref AllDeadCountDownPhase, 1, true);
					for (int k = 0; k < listPlayinfoOC.Count; k++)
					{
						if (listPlayinfoOC[k].tOC.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
						{
							int nIndex2 = listPlayinfoOC[k].nIndex;
							reborntime[nIndex2].text = fContinueTime.ToString("0");
							break;
						}
					}
				}
				if (fFailedTime <= 0f && StageUpdate.bIsHost)
				{
					ShowStageLostByContinueRoot();
				}
			}
			if (fContinueTime <= 0f && nContinueMode != 4)
			{
				nContinueMode = 5;
				ContinueRoot.enabled = false;
				MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = false;
				StartSendRebornMsgCoroutine(0);
			}
		}
		tContinueCoroutine = null;
	}

	public bool CheckPlayerCanReborn(string sPlayerID)
	{
		if (nContinueMode == 4)
		{
			return false;
		}
		RebornPlayerData rebornPlayerData = null;
		foreach (RebornPlayerData item in listRebornPlayerID)
		{
			if (item.sPlayerID == sPlayerID)
			{
				if (Time.realtimeSinceStartup - item.fCallTime > 3f)
				{
					rebornPlayerData = item;
					break;
				}
				return false;
			}
		}
		if (nContinueMode == 3 && fFailedTime <= 0f)
		{
			return false;
		}
		if (rebornPlayerData == null)
		{
			rebornPlayerData = StageResManager.GetObjFromPool<RebornPlayerData>();
			listRebornPlayerID.Add(rebornPlayerData);
		}
		rebornPlayerData.sPlayerID = sPlayerID;
		rebornPlayerData.fCallTime = Time.realtimeSinceStartup;
		return true;
	}

	public void UnRegisterRebornPlayerList(string sPlayerID)
	{
		for (int num = listRebornPlayerID.Count - 1; num >= 0; num--)
		{
			if (listRebornPlayerID[num].sPlayerID == sPlayerID)
			{
				StageResManager.BackObjToPool(listRebornPlayerID[num]);
				listRebornPlayerID.RemoveAt(num);
				break;
			}
		}
	}

	public void ShowStageLostByContinueRoot()
	{
		ContinueRoot.enabled = true;
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		ExitButton.interactable = true;
		ReButton.interactable = true;
		for (int i = 0; i < ContinueSubRoot.Length; i++)
		{
			ContinueSubRoot[i].SetActive(false);
		}
		nContinueMode = 4;
		if (!ContinueSubRoot[nContinueMode].activeSelf)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
		ContinueSubRoot[nContinueMode].SetActive(true);
		ContinueTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MISSION_FAILED_TITLE");
		Instance.SwitchOptionBtn(false, 1);
		PowerHintBtn.gameObject.SetActive(true);
		ReButton.gameObject.SetActive(false);
		MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
		int n_TYPE = NowStageTable.n_TYPE;
		if ((uint)(n_TYPE - 9) > 1u)
		{
			StageUpdate.SyncStageObj(3, 13, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
			StageResManager.GetStageUpdate().IsEnd = true;
			ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
		}
		MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
		BattleSettingUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_BattleSetting");
		if (uI != null)
		{
			uI.OnClickCloseBtn();
		}
	}

	private void EventCallBack(int nType, NotifyCallBack ncb)
	{
		LastDBNotifyCB = ncb;
		switch (nType)
		{
		case 0:
			StartWaring();
			break;
		case 1:
			StartCaution();
			break;
		default:
			StartWaring();
			break;
		}
		EventManager.BattleInfoUpdate battleInfoUpdate = new EventManager.BattleInfoUpdate();
		battleInfoUpdate.nType = 1;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.BATTLE_INFO_UPDATE, battleInfoUpdate);
	}

	public void InitEnemyInfo(int nStageID)
	{
		if (tSTAGE_TABLE.w_BOSS_INTRO != null && !(tSTAGE_TABLE.w_BOSS_INTRO == "") && !(tSTAGE_TABLE.w_BOSS_INTRO == "null"))
		{
			MonoBehaviourSingleton<UIManager>.Instance.PreloadUI("UI_BossIntro");
		}
	}

	private void EventShowEnemyInfo(int unknown, NotifyCallBack ncb)
	{
		EnemyInfoCB = ncb;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossIntro", delegate(BossIntroUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				CloseEnemyInfo();
			});
			ui.Setup(ManagedSingleton<StageHelper>.Instance.nLastStageID);
		});
	}

	public void CloseEnemyInfo()
	{
		if (EnemyInfoCB != null)
		{
			EnemyInfoCB.CallCB();
			EnemyInfoCB = null;
		}
	}

	public void StartWaring()
	{
		MonoBehaviourSingleton<LegionManager>.Instance.callLight(true, 16711680);
		MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(m_waring);
		WaringRoot.enabled = true;
		Warning.enabled = true;
		Warning.animation.Stop();
		Warning.AddEventListener("complete", WaringPlayComplete);
		Warning.animation.Play(Warning.animation.animationNames[0], 1);
		IsBossAppear = true;
		MonoBehaviourSingleton<AudioManager>.Instance.IsBossAppear = true;
	}

	public void StartCaution()
	{
		CautionRoot.enabled = true;
		Caution.enabled = true;
		Caution.animation.Stop();
		Caution.AddEventListener("complete", CautionPlayComplete);
		Caution.animation.Play(Caution.animation.animationNames[0], 1);
		MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(m_caution);
		if (NowStageTable.n_MAIN != 15031)
		{
			IsBossAppear = true;
			MonoBehaviourSingleton<AudioManager>.Instance.IsBossAppear = true;
		}
	}

	private void WaringPlayComplete(string type, EventObject eventObject)
	{
		WaringRoot.enabled = false;
		Warning.enabled = false;
		CautionRoot.enabled = false;
		Caution.enabled = false;
		if (!IsPlayClearBgm)
		{
			if (NowStageTable.s_BOSSENTRY_BGM != "null" && NowStageTable.s_BOSSENTRY_BGM != null)
			{
				string[] array = NowStageTable.s_BOSSENTRY_BGM.Split(',');
				MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(array[0], array[1]);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", (int)m_bossEntry);
			}
			if (LastDBNotifyCB != null)
			{
				LastDBNotifyCB.CallCB();
				LastDBNotifyCB = null;
			}
		}
	}

	private void CautionPlayComplete(string type, EventObject eventObject)
	{
		WaringRoot.enabled = false;
		Warning.enabled = false;
		CautionRoot.enabled = false;
		Caution.enabled = false;
		if (NowStageTable.s_BOSSENTRY_BGM != "null" && NowStageTable.s_BOSSENTRY_BGM != null)
		{
			string[] array = NowStageTable.s_BOSSENTRY_BGM.Split(',');
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(array[0], array[1]);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", (int)m_mbossEntry);
		}
		if (LastDBNotifyCB != null)
		{
			LastDBNotifyCB.CallCB();
			LastDBNotifyCB = null;
		}
	}

	public void CheckStartStageClear(Action cb = null)
	{
		if (!bIsShowStageClear)
		{
			bIsShowStageClear = true;
			switch (tSTAGE_TABLE.n_TYPE)
			{
			case 1:
				Instance.StartStageClear(cb);
				break;
			case 2:
				Instance.StartStageClear(cb);
				break;
			case 3:
				Instance.StartREVISED(cb);
				break;
			case 4:
				Instance.StartStageClear(cb);
				break;
			case 5:
				Instance.StartStageClear(cb);
				break;
			case 1001:
				Instance.StartStageClear(cb);
				break;
			case 9:
				Instance.StartBattleFinish(cb);
				break;
			case 10:
				if (StageUpdate.StageMode == StageMode.Contribute)
				{
					Instance.StartBattleFinish(cb);
				}
				else
				{
					Instance.StartStageClear(cb);
				}
				break;
			default:
				Instance.StartStageClear(cb);
				break;
			}
		}
		else
		{
			cb();
		}
	}

	private void StartStageClear(Action cb = null)
	{
		if (StageClearDB == null)
		{
			string text = "stageclear";
			foreach (StageClearMap item in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj.listStageClearMap)
			{
				if (item.nStageMainID == 0 || (item.nStageMainID == tSTAGE_TABLE.n_MAIN && item.nStageSubID == tSTAGE_TABLE.n_SUB))
				{
					text = item.sStageClearDB;
				}
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, text), text + "db", delegate(GameObject obj)
			{
				if (!(obj == null))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(obj, StageClearRoot.transform);
					StageClearDB = gameObject.GetComponent<UnityArmatureComponent>();
					StartStageClearDB(cb);
				}
			});
		}
		else
		{
			StartStageClearDB(cb);
		}
	}

	private void StartStageClearDB(Action cb = null)
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsMultiply)
		{
			foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
			{
				runPlayer.StopAllLoopSE();
			}
			OC.ClosePadAndPlayerUI();
		}
		else
		{
			OC.ClosePadAndPlayerUI();
			OC.StopAllLoopSE();
		}
		if (!isClearBgmPlay)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", (int)m_stageWin);
			isClearBgmPlay = true;
			clearBGMTimer.TimerStart();
			StartCoroutine(BGMCountDown());
		}
		StageClearRoot.enabled = true;
		StageClearDB.enabled = true;
		StageClearDB.animation.Stop();
		lastDBCB = cb;
		StageClearDB.AddEventListener("complete", StageClearPlayComplete);
		StageClearDB.animation.Play(StageClearDB.animation.animationNames[0], 1);
	}

	private void StageClearPlayComplete(string type, EventObject eventObject)
	{
		StageClearRoot.enabled = false;
		StageClearDB.enabled = false;
		if (lastDBCB != null)
		{
			lastDBCB();
			lastDBCB = null;
		}
	}

	public void StartREVISED(Action cb = null)
	{
		OC.ClosePadAndPlayerUI();
		OC.StopAllLoopSE();
		REVISEDRoot.enabled = true;
		REVISEDDB.enabled = true;
		REVISEDDB.animation.Stop();
		if (!isClearBgmPlay)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", (int)m_challengeWin);
			clearBGMTimer.TimerStart();
			StartCoroutine(BGMCountDown());
		}
		lastDBCB = cb;
		REVISEDDB.AddEventListener("complete", REVISEDPlayComplete);
		REVISEDDB.animation.Play(REVISEDDB.animation.animationNames[0], 1);
	}

	private void REVISEDPlayComplete(string type, EventObject eventObject)
	{
		REVISEDRoot.enabled = false;
		if (lastDBCB != null)
		{
			lastDBCB();
			lastDBCB = null;
		}
	}

	public void StartBattleFinish(Action cb = null)
	{
		OC.ClosePadAndPlayerUI();
		OC.StopAllLoopSE();
		BattleFinishRoot.enabled = true;
		BattleFinishDB.enabled = true;
		BattleFinishDB.animation.Stop();
		if (!isClearBgmPlay)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", (int)m_challengeWin);
			clearBGMTimer.TimerStart();
			StartCoroutine(BGMCountDown());
		}
		lastDBCB = cb;
		BattleFinishDB.AddEventListener("complete", BattleFinishPlayComplete);
		BattleFinishDB.animation.Play(REVISEDDB.animation.animationNames[0], 1);
	}

	private void BattleFinishPlayComplete(string type, EventObject eventObject)
	{
		BattleFinishRoot.enabled = false;
		if (lastDBCB != null)
		{
			lastDBCB();
			lastDBCB = null;
		}
	}

	public void InitBar(StageObjBase tSOB, int nline, int MaxValue, int NowValue, string sModle = "", bool bDeadHiden = true)
	{
		switch (tSTAGE_TABLE.n_TYPE)
		{
		case 9:
			StartCoroutine(FullBossHPCoroutine(tSOB));
			return;
		case 10:
			if (StageUpdate.StageMode == StageMode.Contribute)
			{
				StartCoroutine(FullBossHPCoroutine(tSOB));
				return;
			}
			break;
		}
		List<BossBarGroup> list = new List<BossBarGroup>();
		for (int num = listBossBarGroups.Count - 1; num >= 0; num--)
		{
			if (!listBossBarGroups[num].BossTopBar.enabled)
			{
				list.Add(listBossBarGroups[num]);
				listBossBarGroups.RemoveAt(num);
				nBossBarIndex--;
				if (nBossBarIndex < 0)
				{
					nBossBarIndex = 0;
				}
			}
		}
		if (list.Count > 0)
		{
			listBossBarGroups.AddRange(list);
			list.Clear();
		}
		while (listBossBarGroups.Count < nBossBarIndex + 1)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(listBossBarGroups[0].BossTopBar.gameObject, listBossBarGroups[0].BossTopBar.transform.parent);
			listBossBarGroups.Add(new BossBarGroup(gameObject.GetComponent<Canvas>()));
		}
		listBossBarGroups[nBossBarIndex].InitBar(tSOB, nline, MaxValue, NowValue, sModle);
		listBossBarGroups[nBossBarIndex].bDeadHiden = bDeadHiden;
		if (nBossBarIndex > 0)
		{
			listBossBarGroups[nBossBarIndex].BossTopBar.transform.SetSiblingIndex(listBossBarGroups[nBossBarIndex - 1].BossTopBar.transform.GetSiblingIndex() + 1);
		}
		nBossBarIndex++;
		UpdateBossBarPos();
		IsBossAppear = true;
		MonoBehaviourSingleton<AudioManager>.Instance.IsBossAppear = true;
	}

	public void SetHiddenBossBar(bool bHidden)
	{
		for (int i = 0; i < listBossBarGroups.Count; i++)
		{
			listBossBarGroups[i].BossTopBar.enabled = !bHidden;
		}
		if (!bHidden)
		{
			UpdateBossBarPos();
		}
	}

	private IEnumerator FullBossHPCoroutine(StageObjBase tSOB)
	{
		while (true)
		{
			tSOB.HealHp = (int)tSOB.HealHp + ((int)tSOB.MaxHp - (int)tSOB.Hp);
			tSOB.Hp = tSOB.MaxHp;
			yield return CoroutineDefine._1sec;
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
	}

	public void UpdateBossBarPos()
	{
		float num = -81.2f;
		for (int i = 0; i < listBossBarGroups.Count; i++)
		{
			if (listBossBarGroups[i].BossTopBar.enabled)
			{
				((RectTransform)listBossBarGroups[i].BossTopBar.transform).anchoredPosition = new Vector2(num, -284.7f);
				num += -100f;
			}
		}
	}

	private void FullBossEnd()
	{
		bool flag = true;
		for (int num = listBossBarGroups.Count - 1; num >= 0; num--)
		{
			if (!listBossBarGroups[num].bFullHpEnd)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			isBossHpSe = false;
			if (AddHpFullCB != null)
			{
				AddHpFullCB();
			}
			AddHpFullCB = null;
		}
	}

	private void FullHPEndPlayBGM()
	{
		STAGE_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value))
		{
			if (value.s_BOSSBATTLE_BGM != "null" && value.s_BOSSBATTLE_BGM != null)
			{
				string[] array = value.s_BOSSBATTLE_BGM.Split(',');
				MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(array[0], array[1]);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", 24);
			}
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", 24);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Play("BattleSE", "bt_bosshp01_stop");
	}

	public void FullBossBarHP(int nTime, int tHP = 0, Action fullcb = null)
	{
		AddHpFullCB = (Action)Delegate.Combine(AddHpFullCB, new Action(FullHPEndPlayBGM));
		AddHpFullCB = (Action)Delegate.Combine(AddHpFullCB, fullcb);
		int num = 0;
		if (listBossBarGroups.Count == 0)
		{
			FullBossEnd();
			return;
		}
		for (int num2 = listBossBarGroups.Count - 1; num2 >= 0; num2--)
		{
			num = ((tHP != 0) ? tHP : listBossBarGroups[num2].LinkSOBHp);
			if (listBossBarGroups[num2].nNowValue == num)
			{
				listBossBarGroups[num2].bFullHpEnd = true;
				FullBossEnd();
			}
			else if (listBossBarGroups[num2].AddHpCoroutine == null)
			{
				isBossHpSe = true;
				AddHpFullCB = (Action)Delegate.Combine(AddHpFullCB, fullcb);
				BossBarGroup bossBarGroup = listBossBarGroups[num2];
				bossBarGroup.AddHpFullCB = (Action)Delegate.Combine(bossBarGroup.AddHpFullCB, new Action(FullBossEnd));
				listBossBarGroups[num2].AddHpCoroutine = StartCoroutine(listBossBarGroups[num2].FullBossHpBar(nTime, num));
			}
		}
	}

	public void ShowAlwaysHintMsg(string sMsg, int nParam1, int nParam2)
	{
		if (sMsg == "")
		{
			AlwaysHitMsg.text = "";
		}
		else
		{
			AlwaysHitMsg.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(sMsg), nParam1, nParam2);
		}
	}

	public void ShowTimeHintMsg(string sMsg, int nParam1, int nParam2, float fTime = 0f)
	{
		if (sMsg == "")
		{
			TimeHitMsg.text = "";
			TimeHitMsg.transform.parent.gameObject.SetActive(false);
			return;
		}
		TimeHitMsg.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(sMsg), nParam1, nParam2);
		TimeHitMsg.transform.parent.gameObject.SetActive(true);
		if (fTime == 0f)
		{
			fTime = 2f;
		}
		StartCoroutine(CloseTimeHintMsg(fTime));
	}

	public void ShowLRRangeBar(float fFront = 0f, float fBack = 100f)
	{
		if ((bool)LRRangeBar)
		{
			LRRangeBar.transform.parent.gameObject.SetActive(true);
			LRRangeBar.fillAmount = 0f;
			StartCoroutine(LRRangeCoroutine(fFront, fBack));
		}
	}

	private IEnumerator LRRangeCoroutine(float fFront, float fBack)
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		OrangeCharacter tOC = OC;
		if (tOC == null)
		{
			yield break;
		}
		Vector3 zero2 = Vector3.zero;
		float fLastX = fFront;
		while (true)
		{
			Vector3 position = tOC.transform.position;
			if (fBack > fFront)
			{
				LRRangeBar.fillOrigin = 0;
				if (position.x < fFront)
				{
					LRRangeBar.fillAmount = (fLastX - fFront) / (fBack - fFront);
				}
				else if (position.x > fBack)
				{
					LRRangeBar.fillAmount = 1f;
				}
				else if (fLastX < position.x)
				{
					fLastX = position.x;
					LRRangeBar.fillAmount = (fLastX - fFront) / (fBack - fFront);
				}
			}
			else
			{
				LRRangeBar.fillOrigin = 1;
				if (position.x > fFront)
				{
					LRRangeBar.fillAmount = (fFront - fLastX) / (fFront - fBack);
				}
				else if (position.x < fBack)
				{
					LRRangeBar.fillAmount = 1f;
				}
				else if (fLastX > position.x)
				{
					fLastX = position.x;
					LRRangeBar.fillAmount = (fFront - fLastX) / (fFront - fBack);
				}
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	public void ShowTBRangeBar(float fFront = 0f, float fBack = 100f)
	{
		if ((bool)TBRangeBar)
		{
			TBRangeBar.transform.parent.gameObject.SetActive(true);
			TBRangeBar.gameObject.SetActive(false);
			StartCoroutine(TBRangeCoroutine(fFront, fBack));
		}
	}

	private IEnumerator TBRangeCoroutine(float fFront, float fBack)
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		OrangeCharacter tOC = OC;
		if (tOC == null)
		{
			yield break;
		}
		Vector3 zero2 = Vector3.zero;
		float fLastY = fFront;
		Vector3 tBarPos = TBRangeBar.transform.localPosition;
		TBRangeBar.gameObject.SetActive(true);
		while (true)
		{
			Vector3 position = tOC.transform.position;
			if (fBack > fFront)
			{
				if (position.y < fFront)
				{
					float num = (fLastY - fFront) / (fBack - fFront);
					tBarPos.y = -195f + 390f * num;
					TBRangeBar.transform.localPosition = tBarPos;
				}
				else if (position.y > fBack)
				{
					tBarPos.y = 195f;
					TBRangeBar.transform.localPosition = tBarPos;
				}
				else if (fLastY < position.y)
				{
					fLastY = position.y;
					float num = (fLastY - fFront) / (fBack - fFront);
					tBarPos.y = -195f + 390f * num;
					TBRangeBar.transform.localPosition = tBarPos;
				}
			}
			else if (position.y > fFront)
			{
				float num = (fFront - fLastY) / (fFront - fBack);
				tBarPos.y = 195f - 390f * num;
				TBRangeBar.transform.localPosition = tBarPos;
			}
			else if (position.y < fBack)
			{
				tBarPos.y = -195f;
				TBRangeBar.transform.localPosition = tBarPos;
			}
			else if (fLastY > position.y)
			{
				fLastY = position.y;
				float num = (fFront - fLastY) / (fFront - fBack);
				tBarPos.y = 195f - 390f * num;
				TBRangeBar.transform.localPosition = tBarPos;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private IEnumerator CloseTimeHintMsg(float fTime = 2f)
	{
		Color back = TimeHitMsg.color;
		Color closecolor = back;
		float d = ((!(fTime > 2f)) ? (closecolor.a / fTime) : (closecolor.a / 2f));
		for (fTime -= 2f; fTime > 0f; fTime -= Time.deltaTime)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		while (closecolor.a > 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			closecolor.a -= d * Time.deltaTime;
			TimeHitMsg.color = closecolor;
			TimeHitBg.color = closecolor;
		}
		TimeHitMsg.text = "";
		TimeHitMsg.color = back;
		TimeHitBg.color = back;
		TimeHitMsg.transform.parent.gameObject.SetActive(false);
	}

	public void OnClickOption()
	{
		if (Time.timeScale != 1f || (bool)MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_BattleSetting") || !StageUpdate.IsCanGamePause || !option.activeSelf)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Pause(AudioChannelType.Sound);
		ContinueRoot.enabled = false;
		if (!StageUpdate.gbIsNetGame && tSTAGE_TABLE.n_TYPE != 8 && tSTAGE_TABLE.n_TYPE != 7 && tSTAGE_TABLE.n_TYPE != 13)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
		}
		else
		{
			MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = true;
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.Count == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupYesNoByKey("COMMON_TIP", "TUTORIAL_SKIP_CONFIRM", "COMMON_OK", "COMMON_CANCEL", delegate
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
					if (!StageUpdate.gbIsNetGame && tSTAGE_TABLE.n_TYPE != 8)
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
					}
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
				}, delegate
				{
					if (!StageUpdate.gbIsNetGame && tSTAGE_TABLE.n_TYPE != 8)
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
					}
				});
			});
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BattleSetting", delegate(BattleSettingUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
			BattleSettingUI battleSettingUI = ui;
			battleSettingUI.closeCB = (Callback)Delegate.Combine(battleSettingUI.closeCB, (Callback)delegate
			{
				if (!StageUpdate.gbIsNetGame)
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
				}
				if (ui.Quit)
				{
					isClickQuiteBtn = true;
					StageUpdate.ReqChangeHost();
					StageUpdate.SyncStageObj(3, 16, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true);
					StageResManager.GetStageUpdate().IsEnd = true;
					ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Escape;
					StageOutGORun();
				}
			});
		});
	}

	public void OnClickExitBtn()
	{
		isClickQuiteBtn = true;
		StageOutGO();
	}

	public void StageOutGO()
	{
		MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = false;
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		ExitButton.interactable = false;
		ReButton.interactable = false;
		switch (NowStageTable.n_TYPE)
		{
		case 9:
			nContinueMode = 5;
			ContinueRoot.enabled = false;
			MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
			return;
		case 10:
			nContinueMode = 5;
			ContinueRoot.enabled = false;
			MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
			MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
			if (StageUpdate.StageMode == StageMode.Contribute)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
				return;
			}
			break;
		}
		ManagedSingleton<StageHelper>.Instance.eLastStageResult = StageResult.Lose;
		StageUpdate.ReqChangeHost();
		StageUpdate.SyncStageObj(3, 16, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, true);
		MonoBehaviourSingleton<AudioManager>.Instance.Resume(AudioChannelType.Sound);
		StageOutGORun();
	}

	public void StageOutGORun()
	{
		CanvasExploreEndBG.enabled = false;
		ExploreEndBG.color = new Color(0f, 0f, 0f, 0.5f);
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if ((bool)stageUpdate)
		{
			stageUpdate.IsEnd = true;
		}
		StageUpdate.StopStageAllEvent();
		MonoBehaviourSingleton<UpdateManager>.Instance.Pause = true;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.Stop();
		LeanTween.resumeAll();
		ManagedSingleton<StageHelper>.Instance.ListAchievedMissionID.Clear();
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
		{
			StageEndReq stageEndReq = new StageEndReq();
			stageEndReq.StageID = ManagedSingleton<StageHelper>.Instance.nLastStageID;
			stageEndReq.Star = 0;
			stageEndReq.Result = (sbyte)ManagedSingleton<StageHelper>.Instance.eLastStageResult;
			stageEndReq.Power = ManagedSingleton<StageHelper>.Instance.nLastOCPower;
			stageEndReq.Score = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower();
			stageEndReq.KillCount = (short)MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerKillEnemyNum();
			stageEndReq.PVPMatchType = (sbyte)MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsStageVaild(stageEndReq.StageID))
			{
				MonoBehaviourSingleton<ACTkManager>.Instance.SetDetected();
				return;
			}
			ManagedSingleton<PlayerNetManager>.Instance.StageEndReq(stageEndReq, delegate
			{
				OnClickCloseBtn();
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.BattleServerLogout();
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.TIP);
			});
			return;
		}
		StageEndReq stageEndReq2 = new StageEndReq();
		stageEndReq2.StageID = ManagedSingleton<StageHelper>.Instance.nLastStageID;
		stageEndReq2.Star = 0;
		stageEndReq2.Result = (sbyte)ManagedSingleton<StageHelper>.Instance.eLastStageResult;
		stageEndReq2.AchievedMissionID = ManagedSingleton<StageHelper>.Instance.ListAchievedMissionID;
		stageEndReq2.Power = ManagedSingleton<StageHelper>.Instance.nLastOCPower;
		stageEndReq2.Score = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower();
		stageEndReq2.KillCount = (short)MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerKillEnemyNum();
		stageEndReq2.TowerBossInfoList = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetTowerBossInfoList();
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsStageVaild(stageEndReq2.StageID))
		{
			MonoBehaviourSingleton<ACTkManager>.Instance.SetDetected();
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.StageEndReq(stageEndReq2, delegate
		{
			OnClickCloseBtn();
			if (StageUpdate.gbIsNetGame)
			{
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQLeaveBattleRoom(MonoBehaviourSingleton<VoiceChatManager>.Instance.GetRoomID));
			}
			else
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.BattleServerLogout();
			}
			StartCoroutine(waitSE2BackHome());
		});
	}

	private IEnumerator waitSE2BackHome()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		MonoBehaviourSingleton<AudioManager>.Instance.StopAllExceptSE();
		if (isClickQuiteBtn)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		}
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.TIP, null, false);
	}

	private IEnumerator BGMCountDown()
	{
		while (clearBGMTimer.GetMillisecond() < 5600)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		IsCanTeleportOut = true;
	}

	private IEnumerator CheckMediumBossBGM()
	{
		while (TurtorialUI.IsTutorialing())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
		int n_TYPE = tSTAGE_TABLE.n_TYPE;
		if (n_TYPE == 3)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", (int)m_challengeWin);
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			isClearBgmPlay = true;
			clearBGMTimer.TimerStart();
			StartCoroutine(BGMCountDown());
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", (int)m_stageWin);
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			isClearBgmPlay = true;
			clearBGMTimer.TimerStart();
			StartCoroutine(BGMCountDown());
		}
		OC.StopAllLoopSE();
	}

	private void CountDownSE(float time, ref int phase, int SEtype = 1, bool Ignore0 = false)
	{
		if ((!Ignore0 || phase != 0) && (float)phase >= time)
		{
			if (SEtype == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_TIMER);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_TIMER02);
			}
			phase--;
		}
	}

	public override void DoJoystickEvent()
	{
		if (bShowOption[0] && bShowOption[1])
		{
			TryPause();
		}
	}

	private void TryPause()
	{
		if (ManagedSingleton<InputStorage>.Instance.IsPressed(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, ButtonId.START) && !StageUpdate.gbIsNetGame && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !_bIsSteamPauseEnabled && tSTAGE_TABLE.n_TYPE != 8 && tSTAGE_TABLE.n_TYPE != 7 && tSTAGE_TABLE.n_TYPE != 13 && tSTAGE_TABLE.n_TYPE != 12 && tSTAGE_TABLE.n_TYPE != 11 && !StageUpdate.GetMainPlayerOC().IsDead())
		{
			OnSteamGamePause();
		}
	}

	private void OnSteamGamePause()
	{
		if (Time.timeScale != 1f || (bool)MonoBehaviourSingleton<UIManager>.Instance.GetUI<BattleSettingUI>("UI_BattleSetting") || !StageUpdate.IsCanGamePause)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Pause(AudioChannelType.Sound);
		_bIsSteamPauseEnabled = true;
		MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = true;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GamePause", delegate(GamePause ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.Setup();
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Resume(AudioChannelType.Sound);
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
				MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause = false;
				_bIsSteamPauseEnabled = false;
			});
		});
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.OnClickCloseBtn();
	}
}
