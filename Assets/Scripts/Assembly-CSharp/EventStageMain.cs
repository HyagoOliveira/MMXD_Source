#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using NaughtyAttributes;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class EventStageMain : OrangeUIBase
{
	[Serializable]
	public struct BgmInfo
	{
		[SerializeField]
		public int iEventID;

		[SerializeField]
		public string acbName;

		[SerializeField]
		public string cueName;

		public BgmInfo(string a, string b)
		{
			iEventID = 0;
			acbName = a;
			cueName = b;
		}

		public BgmInfo(int id, string a, string b)
		{
			iEventID = id;
			acbName = a;
			cueName = b;
		}
	}

	private enum TabType
	{
		SKILLITEM = 0,
		GOLD = 1,
		ARMORITEM = 2,
		EXP = 3,
		SP = 4,
		TIMELIMITED = 5,
		BOSSRUSH = 6,
		MAX = 7
	}

	private enum RankType
	{
		SS = 0,
		S = 1,
		A = 2,
		B = 3,
		C = 4,
		NONE = 5
	}

	private enum ModeType
	{
		NORMAL = 0,
		CHALLENGE = 1,
		NONE = 2
	}

	[SerializeField]
	private OrangeText m_stageTitle;

	[SerializeField]
	private OrangeText m_eventTime;

	[SerializeField]
	private OrangeText m_challengeCount;

	[SerializeField]
	private OrangeText m_energyUseCount;

	[SerializeField]
	private OrangeText m_reportEngergyCount;

	[SerializeField]
	private Transform[] m_difficultyBarMeter;

	[SerializeField]
	private Transform[] m_difficultyBarLock;

	[SerializeField]
	private Transform m_eventReport;

	[SerializeField]
	private Transform m_eventReportBossRush;

	[SerializeField]
	private Button m_difficultyPlus;

	[SerializeField]
	private Button m_difficultyMinus;

	[SerializeField]
	private Button m_btnRules;

	[SerializeField]
	private Button m_btnReward;

	[SerializeField]
	private Button m_btnShop;

	[SerializeField]
	private Button m_btnTasks;

	[SerializeField]
	private Button m_btnSweep;

	[SerializeField]
	private Button m_btnAddEP;

	[SerializeField]
	private ToggleGroup m_toggleGroup;

	[SerializeField]
	private Toggle[] m_stageTabs;

	[SerializeField]
	private Toggle m_limitedTabRef;

	[SerializeField]
	private Toggle m_bossRushTabRef;

	[SerializeField]
	private ItemIconBase m_rewardIcon;

	[SerializeField]
	private Image[] m_rankAlphabets;

	[SerializeField]
	private Transform m_difficultyDialog;

	[SerializeField]
	private Transform m_eventDropPanel;

	[SerializeField]
	private Transform m_rankPanelPos;

	[SerializeField]
	private Transform m_dailyChallengeCountGroup;

	[SerializeField]
	private Transform m_energyUseGroup;

	[SerializeField]
	private Transform m_difficultyGroup;

	[SerializeField]
	private Image imgScenarioOn;

	[Header("LimitedEvent")]
	[SerializeField]
	private OrangeText m_limitedEventCountdown;

	[SerializeField]
	private OrangeText m_limitedEventScore;

	[SerializeField]
	private OrangeText m_limitedEventRank;

	[SerializeField]
	private Transform[] m_limitedEventItems;

	[SerializeField]
	private RankPanelMini m_rankPanelMiniRef;

	[Header("BossRush")]
	[SerializeField]
	private OrangeText m_bossRushClearTimeMin;

	[SerializeField]
	private OrangeText m_bossRushClearTimeSec;

	[SerializeField]
	private OrangeText m_bossRushClearTimeFracSec;

	[SerializeField]
	private OrangeText m_bossRushRank;

	[SerializeField]
	private OrangeText m_bossRushScore;

	[SerializeField]
	private OrangeText m_bossRushBonus;

	[SerializeField]
	private Button m_bossRushBonusBtn;

	[Header("Challenge")]
	[SerializeField]
	private GameObject m_challengeToggleGroup;

	[SerializeField]
	private Toggle m_toggleNormal;

	[SerializeField]
	private Toggle m_toggleChallenge;

	[SerializeField]
	private Transform m_difficultyBarPanel;

	[SerializeField]
	private Transform m_difficultyBarMask;

	[SerializeField]
	private OrangeText m_difficultyBarText;

	[SerializeField]
	private GameObject m_normalModeFX;

	[SerializeField]
	private GameObject m_challengeModeFX;

	private int m_difficultyBarPanelTweenID;

	private int m_difficultyBarMaskTweenID;

	private ModeType m_currentMode = ModeType.NONE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickTapSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_addDiffSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_reward;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_shopSE;

	[Header("BGM")]
	[SerializeField]
	private BgmInfo BGM_BossRush;

	[SerializeField]
	private BgmInfo BGM_TimeLimit;

	[SerializeField]
	private BgmInfo BGM_Default;

	[SerializeField]
	private List<BgmInfo> BGM4Stage = new List<BgmInfo>();

	private bool bChangePage = true;

	private bool repeatScenario;

	private bool bIsInit;

	private const int m_skillItemStageID = 15001;

	private const int m_goldStageID = 15011;

	private const int m_armorItemStageID = 15021;

	private const int m_expStageID = 15031;

	private const int m_spStageID = 15041;

	private const float m_toggleHeight = 180f;

	private List<STAGE_TABLE>[] m_eventStageTables = new List<STAGE_TABLE>[5];

	private List<STAGE_TABLE>[] m_openTimedStageTables;

	private Dictionary<int, int> m_challengeStageDict = new Dictionary<int, int>();

	private List<EVENT_TABLE> m_timedEventTableList;

	private STAGE_TABLE m_currentStageTable;

	private RankPanelMini m_rankPanelMini;

	private EventRankingInfo m_eventRankingInfo;

	private List<STAGE_TABLE> m_bossRushStageTables = new List<STAGE_TABLE>();

	private List<STAGE_TABLE> m_openBossRushStageTables = new List<STAGE_TABLE>();

	private List<EVENT_TABLE> m_bossRushEventTableList;

	private const int m_maxDifficulty = 15;

	private int m_availableDifficulty = 1;

	private int m_currentDifficulty = 1;

	private int m_currentPlayerLV = 1;

	private bool m_currentPlayerCheat;

	private TabType m_currentSelectedTab = TabType.MAX;

	private int m_currentSelectedTabIndex;

	private OrangeBgExt m_bgExt;

	private List<IconBase> m_listReward = new List<IconBase>();

	private StageInfo m_stageInfo;

	private int m_countdownTweenId;

	private int m_timedEventAlphaTweenId;

	private int m_bossRushAlphaTweenId;

	private bool b_ignoreFristSE = true;

	private TabType initTabType = TabType.MAX;

	private int initTabIndex;

	private string[] arrEnergyFormat = new string[2] { "<color=#BEC7D1>{0}/{1}</color>", "<color=#5DDEF4>{0}</color>/{1}" };

	protected override void Awake()
	{
		base.Awake();
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Clear));
		m_currentPlayerCheat = false;
		ManagedSingleton<PlayerHelper>.Instance.GetUseCheatPlugIn();
	}

	private void Clear()
	{
		StageUpdate.AllStageCtrlEvent = repeatScenario;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRepeatScenario = repeatScenario;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref m_difficultyBarPanelTweenID);
		LeanTween.cancel(ref m_difficultyBarMaskTweenID);
		LeanTween.cancel(ref m_bossRushAlphaTweenId);
		LeanTween.cancel(ref m_timedEventAlphaTweenId);
		LeanTween.cancel(ref m_countdownTweenId);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CHARGE_STAMINA, NotifyChargeStamina);
	}

	public void Setup(int n_Main = 0)
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CHARGE_STAMINA, NotifyChargeStamina);
		m_bgExt = Background as OrangeBgExt;
		m_currentPlayerLV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		repeatScenario = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRepeatScenario;
		UpdateEnergyValue();
		IEnumerable<STAGE_TABLE> source = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_TYPE == 2);
		m_eventStageTables[0] = source.Where((STAGE_TABLE x) => x.n_MAIN == 15001).ToList();
		m_eventStageTables[1] = source.Where((STAGE_TABLE x) => x.n_MAIN == 15011).ToList();
		m_eventStageTables[2] = source.Where((STAGE_TABLE x) => x.n_MAIN == 15021).ToList();
		m_eventStageTables[3] = source.Where((STAGE_TABLE x) => x.n_MAIN == 15031).ToList();
		m_eventStageTables[4] = source.Where((STAGE_TABLE x) => x.n_MAIN == 15041).ToList();
		if (m_rankPanelMiniRef != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_rankPanelMiniRef.gameObject, m_rankPanelPos);
			m_rankPanelMini = gameObject.GetComponent<RankPanelMini>();
			SetRankIcon(RankType.NONE);
			SetChallengeTip("");
			AddTimeLimitedEventTabs();
			AddBossRushEventTab();
			SelectTab(n_Main);
			EnableToggles();
		}
		RemoveExpiredTimedEventDifficultyData();
		bIsInit = true;
		if (initTabType != TabType.MAX)
		{
			EventTabHelper(initTabType, initTabIndex);
		}
	}

	public void Start()
	{
		closeCB = (Callback)Delegate.Combine(closeCB, (Callback)delegate
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
		});
	}

	private void RemoveExpiredTimedEventDifficultyData()
	{
		foreach (int item in MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicEventStageDifficulties.Keys.ToList())
		{
			bool flag = false;
			if (item == 15001 || item == 15011 || item == 15021 || item == 15031 || item == 15041)
			{
				continue;
			}
			for (int i = 0; i < m_openTimedStageTables.Length; i++)
			{
				if (m_openTimedStageTables[i] != null && item == m_openTimedStageTables[i][0].n_MAIN)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				Debug.Log("Removing expired timed event save data, id = " + item);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicEventStageDifficulties.Remove(item);
			}
		}
	}

	private void SetupStaminaTimer()
	{
		float delayTime = 1f;
		long countDownSec = GetStaminaRecoverTime();
		m_limitedEventCountdown.text = SecToTimeString(countDownSec);
		LeanTween.cancel(ref m_countdownTweenId);
		m_countdownTweenId = LeanTween.delayedCall(delayTime, (Action)delegate
		{
			countDownSec--;
			if (countDownSec < 0)
			{
				countDownSec = GetStaminaRecoverTime();
				UpdateEnergyValue();
			}
			m_limitedEventCountdown.text = SecToTimeString(countDownSec);
		}).setRepeat(-1).uniqueId;
	}

	private long GetStaminaRecoverTime()
	{
		int eventStamina = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		int eP_MAX = OrangeConst.EP_MAX;
		if (eventStamina >= eP_MAX)
		{
			return 0L;
		}
		long num = 60 * OrangeConst.EP_RECOVER_TIME;
		long num2 = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC - ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.EventActionPointTimer;
		int num3 = (int)(num2 / num);
		return num - (num2 - num3 * num);
	}

	private string SecToTimeString(long timeInSec)
	{
		int num = 0;
		int num2 = 0;
		num = (int)(timeInSec / 60);
		num2 = (int)(timeInSec % 60);
		return num.ToString("00") + ":" + num2.ToString("00");
	}

	public void NotifyChargeStamina()
	{
		UpdateEnergyValue();
		UpdateChallengeCount();
		SetupStaminaTimer();
	}

	private void UpdateEnergyValue()
	{
		int eventStamina = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		int eP_MAX = OrangeConst.EP_MAX;
		m_reportEngergyCount.text = string.Format((eventStamina <= eP_MAX) ? arrEnergyFormat[0] : arrEnergyFormat[1], eventStamina, eP_MAX);
	}

	private void SelectFirstTab()
	{
		if (m_toggleGroup.transform.childCount > 0)
		{
			m_toggleGroup.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
		}
	}

	private void SelectTab(int n_Main)
	{
		int num = 0;
		if (n_Main == 0)
		{
			SelectFirstTab();
			return;
		}
		for (num = 0; num < m_bossRushStageTables.Count; num++)
		{
			if (m_bossRushStageTables[num].n_MAIN == n_Main)
			{
				m_toggleGroup.transform.GetChild(num).GetComponent<Toggle>().isOn = true;
				return;
			}
		}
		for (num = 0; num < m_openTimedStageTables.Length; num++)
		{
			if (m_openTimedStageTables[num] != null && m_openTimedStageTables[num].Count >= 1 && m_openTimedStageTables[num][0].n_MAIN == n_Main)
			{
				m_toggleGroup.transform.GetChild(num + m_bossRushStageTables.Count).GetComponent<Toggle>().isOn = true;
				return;
			}
		}
		for (num = 0; num < m_eventStageTables.Length; num++)
		{
			if (m_eventStageTables[num] != null && m_eventStageTables[num].Count >= 1 && m_eventStageTables[num][0].n_MAIN == n_Main)
			{
				m_stageTabs[num].isOn = true;
				return;
			}
		}
		SelectFirstTab();
	}

	private void AddBossRushEventTab()
	{
		int num = 0;
		m_bossRushEventTableList = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 11 && x.n_TYPE_X == 8).ToList();
		if (m_bossRushEventTableList.Count == 0)
		{
			return;
		}
		SortEventByEndTime(ref m_bossRushEventTableList);
		foreach (EVENT_TABLE eventTable in m_bossRushEventTableList)
		{
			int stageMainID = eventTable.n_TYPE_Y;
			List<STAGE_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_MAIN == stageMainID).ToList();
			if (list.Count == 0 || !ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(eventTable.s_BEGIN_TIME, eventTable.s_REMAIN_TIME, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
			{
				continue;
			}
			m_bossRushStageTables.Add(list[0]);
			Toggle newTab = UnityEngine.Object.Instantiate(m_bossRushTabRef, m_toggleGroup.transform);
			newTab.gameObject.SetActive(true);
			newTab.transform.SetSiblingIndex(num);
			List<STAGE_TABLE> list2 = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_MAIN == eventTable.n_TYPE_Y).ToList();
			if (list2.Count > 0)
			{
				m_openBossRushStageTables.Add(list2[0]);
			}
			string assetName = ((eventTable.s_IMG2 == "null" || string.IsNullOrEmpty(eventTable.s_IMG2)) ? "UI_Event_TabIcon_BossRush_00" : eventTable.s_IMG2.ToString());
			string eventName = ((eventTable.w_NAME == "null" || string.IsNullOrEmpty(eventTable.w_NAME)) ? "" : MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(eventTable.w_NAME));
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_event, assetName, delegate(Sprite obj)
			{
				newTab.transform.Find("ImgIcon").GetComponent<Image>().sprite = obj;
				newTab.transform.Find("ImgIconFX").GetComponent<Image>().sprite = obj;
				newTab.transform.Find("Label").GetComponent<Text>().text = eventName;
			});
			int _index = num;
			newTab.onValueChanged.AddListener(delegate(bool enable)
			{
				if (enable)
				{
					EventTabHelper(TabType.BOSSRUSH, _index);
				}
			});
			m_currentPlayerCheat = ManagedSingleton<PlayerHelper>.Instance.GetUseCheatPlugIn();
			newTab.interactable = !m_currentPlayerCheat;
			if (m_currentPlayerCheat)
			{
				OrangeGameUtility.CreateLockObj(newTab.transform, UIOpenChk.ChkBanEnum.OPENBAN_BOSSRUSH);
			}
			num++;
		}
		RectTransform component = m_toggleGroup.transform.parent.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, component.sizeDelta.y + 180f * (float)num);
	}

	private void AddTimeLimitedEventTabs()
	{
		m_challengeStageDict.Clear();
		int num = 0;
		m_timedEventTableList = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 1 && x.n_TYPE_X == 4).ToList();
		if (m_timedEventTableList.Count == 0)
		{
			return;
		}
		m_openTimedStageTables = new List<STAGE_TABLE>[m_timedEventTableList.Count];
		SortEventByEndTime(ref m_timedEventTableList);
		foreach (EVENT_TABLE eventTable in m_timedEventTableList)
		{
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(eventTable.s_BEGIN_TIME, eventTable.s_REMAIN_TIME, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
			{
				continue;
			}
			Toggle newTab = UnityEngine.Object.Instantiate(m_limitedTabRef, m_toggleGroup.transform);
			newTab.gameObject.SetActive(true);
			newTab.transform.SetSiblingIndex(num);
			m_openTimedStageTables[num] = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_MAIN == eventTable.n_TYPE_Y && x.n_SUB == 1).ToList();
			foreach (STAGE_TABLE item in ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_MAIN == eventTable.n_TYPE_Y && x.n_SUB == 2).ToList())
			{
				m_challengeStageDict.Add(item.n_MAIN, item.n_ID);
			}
			string assetName = ((eventTable.s_IMG2 == "null" || string.IsNullOrEmpty(eventTable.s_IMG2)) ? "UI_Event_TabIcon_SP_01" : eventTable.s_IMG2.ToString());
			string eventName = ((eventTable.w_NAME == "null" || string.IsNullOrEmpty(eventTable.w_NAME)) ? "" : MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(eventTable.w_NAME));
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_event, assetName, delegate(Sprite obj)
			{
				newTab.transform.Find("ImgIcon").GetComponent<Image>().sprite = obj;
				newTab.transform.Find("ImgIconFX").GetComponent<Image>().sprite = obj;
				newTab.transform.Find("Label").GetComponent<Text>().text = eventName;
			});
			int _index = num;
			newTab.onValueChanged.AddListener(delegate(bool enable)
			{
				if (enable)
				{
					EventTabHelper(TabType.TIMELIMITED, _index);
				}
			});
			num++;
		}
		RectTransform component = m_toggleGroup.transform.parent.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.sizeDelta.x, component.sizeDelta.y + 180f * (float)num);
	}

	private void SortEventByEndTime(ref List<EVENT_TABLE> targetTableList)
	{
		targetTableList.Sort(delegate(EVENT_TABLE x, EVENT_TABLE y)
		{
			long value = CapUtility.DateToUnixTime(ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(x.s_END_TIME));
			return CapUtility.DateToUnixTime(ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(y.s_END_TIME)).CompareTo(value);
		});
	}

	private void EnableToggles()
	{
		for (int i = 0; i < 5; i++)
		{
			STAGE_TABLE sTAGE_TABLE = m_eventStageTables[i].FirstOrDefault();
			if (sTAGE_TABLE != null)
			{
				m_stageTabs[i].interactable = m_currentPlayerLV >= sTAGE_TABLE.n_RANK;
				Transform transform = m_stageTabs[i].transform.Find("LockGroup");
				if ((bool)transform)
				{
					transform.gameObject.SetActive(!m_stageTabs[i].interactable);
				}
				Transform transform2 = m_stageTabs[i].transform.Find("Label");
				if ((bool)transform2)
				{
					transform2.gameObject.SetActive(m_stageTabs[i].interactable);
				}
			}
		}
	}

	private void RefreshDifficultyMeter()
	{
		int battlePower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower();
		int num = 0;
		int num2 = 0;
		if (m_currentSelectedTab == TabType.BOSSRUSH)
		{
			return;
		}
		if (m_currentSelectedTab == TabType.TIMELIMITED)
		{
			if (m_openTimedStageTables[m_currentSelectedTabIndex].Count > m_currentDifficulty)
			{
				num = m_openTimedStageTables[m_currentSelectedTabIndex][m_currentDifficulty].n_RANK;
				num2 = m_openTimedStageTables[m_currentSelectedTabIndex][m_currentDifficulty].n_CP;
			}
		}
		else if (m_eventStageTables[(int)m_currentSelectedTab].Count > m_currentDifficulty)
		{
			num = m_eventStageTables[(int)m_currentSelectedTab][m_currentDifficulty].n_RANK;
			num2 = m_eventStageTables[(int)m_currentSelectedTab][m_currentDifficulty].n_CP;
		}
		if (m_currentPlayerLV >= num && battlePower >= num2 && num != 0 && num2 != 0)
		{
			SetChallengeTip(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECOMMEND_NEW_DIFFICULTY"));
		}
		else
		{
			SetChallengeTip("");
		}
		int num3 = 0;
		Transform[] difficultyBarMeter = m_difficultyBarMeter;
		for (int i = 0; i < difficultyBarMeter.Length; i++)
		{
			Transform transform2 = difficultyBarMeter[i];
			if (num3 < m_currentDifficulty)
			{
				m_difficultyBarMeter[num3].gameObject.SetActive(true);
			}
			else
			{
				m_difficultyBarMeter[num3].gameObject.SetActive(false);
			}
			num3++;
		}
		num3 = 0;
		difficultyBarMeter = m_difficultyBarLock;
		for (int i = 0; i < difficultyBarMeter.Length; i++)
		{
			Transform transform3 = difficultyBarMeter[i];
			m_difficultyBarLock[num3].gameObject.SetActive(num3 >= m_availableDifficulty);
			num3++;
		}
	}

	private void SetRankIcon(RankType rank)
	{
		int num = 0;
		Image[] rankAlphabets = m_rankAlphabets;
		for (int i = 0; i < rankAlphabets.Length; i++)
		{
			rankAlphabets[i].gameObject.SetActive(num == (int)rank);
			num++;
		}
	}

	private void SetStageRank()
	{
		SetRankIcon(RankType.NONE);
		if (m_stageInfo != null)
		{
			switch (ManagedSingleton<StageHelper>.Instance.GetStarAmount(m_stageInfo.netStageInfo.Star))
			{
			case 3:
				SetRankIcon(RankType.S);
				break;
			case 2:
				SetRankIcon(RankType.A);
				break;
			case 1:
				SetRankIcon(RankType.B);
				break;
			default:
				SetRankIcon(RankType.C);
				break;
			}
		}
	}

	private void UpdateChallengeCount()
	{
		int availableChallengeCount = ManagedSingleton<StageHelper>.Instance.GetAvailableChallengeCount(m_currentStageTable);
		if (m_currentStageTable.n_PLAY_COUNT == -1)
		{
			m_challengeCount.text = "∞";
		}
		else
		{
			m_challengeCount.text = string.Format("{0}/{1}", availableChallengeCount, m_currentStageTable.n_PLAY_COUNT);
		}
	}

	private void SetChallengeTip(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			m_difficultyDialog.gameObject.SetActive(false);
			return;
		}
		m_difficultyDialog.gameObject.SetActive(true);
		Text componentInChildren = m_difficultyDialog.GetComponentInChildren<Text>();
		if ((bool)componentInChildren)
		{
			componentInChildren.text = text;
		}
	}

	public void OnClickDifficultyPlus()
	{
		int requiredLV = 0;
		if (m_currentDifficulty >= m_availableDifficulty)
		{
			SetChallengeTip(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NOT_AVAILABLE"));
			return;
		}
		if (m_currentSelectedTab != TabType.TIMELIMITED)
		{
			requiredLV = m_eventStageTables[(int)m_currentSelectedTab][m_currentDifficulty].n_RANK;
		}
		else
		{
			requiredLV = m_openTimedStageTables[m_currentSelectedTabIndex][m_currentDifficulty].n_RANK;
		}
		if (m_currentPlayerLV < requiredLV)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
			{
				string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), requiredLV.ToString());
				ui.Setup(p_msg);
			});
		}
		else if (m_currentDifficulty < m_availableDifficulty)
		{
			m_currentDifficulty++;
			UpdateCurrentStageTableWithDifficulty();
			RefreshDifficultyMeter();
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicEventStageDifficulties[m_currentStageTable.n_MAIN] = m_currentDifficulty;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addDiffSE);
		}
	}

	public void OnClickDifficultyMinus()
	{
		if (m_currentDifficulty >= 2)
		{
			m_currentDifficulty--;
			UpdateCurrentStageTableWithDifficulty();
			RefreshDifficultyMeter();
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicEventStageDifficulties[m_currentStageTable.n_MAIN] = m_currentDifficulty;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addDiffSE);
		}
	}

	private void UpdateCurrentStageTableWithDifficulty()
	{
		if (m_currentSelectedTab == TabType.TIMELIMITED)
		{
			m_currentStageTable = m_openTimedStageTables[m_currentSelectedTabIndex][m_currentDifficulty - 1];
			m_availableDifficulty = m_openTimedStageTables[m_currentSelectedTabIndex].Count;
		}
		else if (m_currentSelectedTab == TabType.BOSSRUSH)
		{
			m_currentStageTable = m_bossRushStageTables[m_currentSelectedTabIndex];
			m_availableDifficulty = 1;
		}
		else
		{
			m_currentStageTable = m_eventStageTables[(int)m_currentSelectedTab][m_currentDifficulty - 1];
			m_availableDifficulty = m_eventStageTables[(int)m_currentSelectedTab].Count;
		}
		ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(m_currentStageTable.n_ID, out m_stageInfo);
		SetStageRank();
		UpdateChallengeCount();
		m_stageTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(m_currentStageTable.w_NAME);
		m_btnReward.gameObject.SetActive(IsRewardAvailable());
	}

	private bool IsRewardAvailable()
	{
		EVENT_TABLE selectedTimedEventTable = GetSelectedTimedEventTable();
		List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(m_currentStageTable.n_GET_REWARD);
		if (selectedTimedEventTable != null && (selectedTimedEventTable.n_BOXGACHA != 0 || selectedTimedEventTable.n_POINT != 0 || selectedTimedEventTable.n_RANKING != 0 || listGachaByGroup.Count > 0))
		{
			return true;
		}
		return false;
	}

	private EVENT_TABLE GetSelectedTimedEventTable()
	{
		EVENT_TABLE value = null;
		if (m_currentSelectedTab == TabType.TIMELIMITED)
		{
			if (m_openTimedStageTables[m_currentSelectedTabIndex] != null && ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.TryGetValue(m_openTimedStageTables[m_currentSelectedTabIndex][0].n_MAIN, out value))
			{
				return value;
			}
		}
		else if (m_currentSelectedTab == TabType.BOSSRUSH && m_bossRushEventTableList[m_currentSelectedTabIndex] != null && ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.TryGetValue(m_currentStageTable.n_MAIN, out value))
		{
			return value;
		}
		return null;
	}

	private bool IsTimedEventAndStartable()
	{
		if (m_currentSelectedTab == TabType.TIMELIMITED || m_currentSelectedTab == TabType.BOSSRUSH)
		{
			EVENT_TABLE selectedTimedEventTable = GetSelectedTimedEventTable();
			if (selectedTimedEventTable == null)
			{
				return false;
			}
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(selectedTimedEventTable.s_BEGIN_TIME, selectedTimedEventTable.s_END_TIME, serverUnixTimeNowUTC))
			{
				return false;
			}
		}
		return true;
	}

	public void OnClickSweep()
	{
		if (!CheckBeforeStart() || MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCardCountMax())
		{
			return;
		}
		bool flag = false;
		if (m_stageInfo != null)
		{
			flag = true;
		}
		if (!flag)
		{
			string errorMsg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SWEEP_CORP_RESTRICT");
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(errorMsg, true);
			});
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("SweepDialog", delegate(SweepDialog sweepDialog)
		{
			sweepDialog.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			sweepDialog.Setup(m_currentStageTable, delegate
			{
				UpdateEnergyValue();
				UpdateChallengeCount();
				if (m_currentSelectedTab == TabType.TIMELIMITED)
				{
					UpdateRankingInfoTimeLimited(GetSelectedTimedEventTable());
				}
			});
		});
	}

	private bool CheckBeforeStart()
	{
		if (!IsTimedEventAndStartable())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NOT_AVAILABLE"), true);
			});
			return false;
		}
		StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
		if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(m_currentStageTable, ref condition))
		{
			if (m_currentSelectedTab == TabType.TIMELIMITED)
			{
				ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(m_currentStageTable, condition, delegate
				{
					UpdateEnergyValue();
				});
			}
			else
			{
				switch (condition)
				{
				case StageHelper.StageJoinCondition.COUNT:
				{
					string errorMsg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SWEEP_CORP_LIMIT");
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
					{
						tipUI.Setup(errorMsg, true);
					});
					break;
				}
				case StageHelper.StageJoinCondition.AP:
				{
					string errorMsg2 = "Insufficient AP";
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
					{
						tipUI.Setup(errorMsg2, true);
					});
					break;
				}
				}
			}
			return false;
		}
		return true;
	}

	public void OnClickDeploy()
	{
		if (!CheckBeforeStart())
		{
			return;
		}
		base.CloseSE = SystemSE.NONE;
		Debug.Log("m_currentStageTable.nID = " + m_currentStageTable.n_ID);
		if (m_currentSelectedTab == TabType.TIMELIMITED && m_currentStageTable.n_AP > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("EPRatioDialog", delegate(EPRatioDialog ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(m_currentStageTable);
			});
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
		{
			ui.Setup(m_currentStageTable);
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.ACTIVITYEVENT;
			ManagedSingleton<StageHelper>.Instance.activityEventStageMainID = m_currentStageTable.n_MAIN;
		});
	}

	public void OnClickRules()
	{
		string titleStr = null;
		string ruleStr = null;
		switch (m_currentSelectedTab)
		{
		case TabType.SKILLITEM:
			titleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE");
			ruleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RULE_FSKILL");
			break;
		case TabType.GOLD:
			titleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE");
			ruleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RULE_MONEY");
			break;
		case TabType.ARMORITEM:
			titleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE");
			ruleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RULE_EQUIP");
			break;
		case TabType.EXP:
			titleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE");
			ruleStr = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_RULE_EXP");
			break;
		default:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RuleBonus", delegate(RuleBonusDialog ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(m_currentStageTable);
			});
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(titleStr, ruleStr);
		});
	}

	public void OnClickBonus()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RuleBonus", delegate(RuleBonusDialog ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(m_currentStageTable);
			ui.ForceBonusTab();
		});
	}

	public void OnClickReward()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EventReward", delegate(EventRewardDialog ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				UpdateRankingInfoTimeLimited(GetSelectedTimedEventTable());
			});
			if (m_currentSelectedTab == TabType.TIMELIMITED)
			{
				ui.Setup(GetSelectedTimedEventTable(), m_currentStageTable, m_eventRankingInfo);
			}
			else if (m_currentSelectedTab == TabType.BOSSRUSH)
			{
				ui.Setup(GetSelectedTimedEventTable(), m_currentStageTable, m_eventRankingInfo);
			}
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_reward);
	}

	public void OnClickShop()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_shopSE);
		EVENT_TABLE selectedTimedEventTable = GetSelectedTimedEventTable();
		ManagedSingleton<UILinkHelper>.Instance.LoadUI(selectedTimedEventTable.n_SHOP, delegate
		{
			UpdateRankingInfoTimeLimited(GetSelectedTimedEventTable());
		});
	}

	public void OnClickMission()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_shopSE);
		EVENT_TABLE selectedTimedEventTable = GetSelectedTimedEventTable();
		ManagedSingleton<UILinkHelper>.Instance.LoadUI(selectedTimedEventTable.n_MISSION);
	}

	private int GetLocalDifficultySetting()
	{
		int value = 1;
		if (m_currentSelectedTab == TabType.TIMELIMITED)
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicEventStageDifficulties.TryGetValue(m_openTimedStageTables[m_currentSelectedTabIndex][0].n_MAIN, out value);
		}
		else if (m_currentSelectedTab == TabType.BOSSRUSH)
		{
			value = 1;
		}
		else
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicEventStageDifficulties.TryGetValue(m_eventStageTables[(int)m_currentSelectedTab][0].n_MAIN, out value);
		}
		if (value <= 0)
		{
			return 1;
		}
		return value;
	}

	private void EventTabHelper(TabType tabType, int tabIndex = 0)
	{
		if (!IsInitComplete(tabType, tabIndex))
		{
			return;
		}
		BgmInfo bGM_Default = BGM_Default;
		int num = 0;
		if (tabType == m_currentSelectedTab && m_currentSelectedTabIndex == tabIndex)
		{
			return;
		}
		bChangePage = true;
		if (b_ignoreFristSE)
		{
			b_ignoreFristSE = false;
		}
		else if (tabIndex != m_currentSelectedTabIndex || m_currentSelectedTab != tabType)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTapSE);
		}
		m_currentSelectedTab = tabType;
		m_currentSelectedTabIndex = tabIndex;
		m_currentDifficulty = GetLocalDifficultySetting();
		UpdateCurrentStageTableWithDifficulty();
		EnableChallengeToggle();
		EnableChallengePanel(false);
		m_currentMode = ModeType.NONE;
		m_toggleNormal.isOn = true;
		m_toggleChallenge.isOn = false;
		m_bgExt.ChangeBackground(m_currentStageTable.s_BG);
		m_stageTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(m_currentStageTable.w_NAME);
		switch (tabType)
		{
		case TabType.TIMELIMITED:
		{
			EVENT_TABLE selectedTimedEventTable2 = GetSelectedTimedEventTable();
			num = selectedTimedEventTable2.n_ID;
			SetupStaminaTimer();
			UpdateRankingInfoTimeLimited(selectedTimedEventTable2);
			m_eventTime.text = OrangeGameUtility.DisplayDatePeriod(selectedTimedEventTable2.s_BEGIN_TIME, selectedTimedEventTable2.s_END_TIME);
			m_energyUseCount.text = string.Format("x{0}", m_currentStageTable.n_AP);
			EnableBossRushReportPanel(false);
			EnableTimedEventReportPanel(true);
			m_btnShop.gameObject.SetActive(selectedTimedEventTable2.n_SHOP != 0);
			m_btnTasks.gameObject.SetActive(selectedTimedEventTable2.n_MISSION != 0);
			m_eventDropPanel.gameObject.SetActive(false);
			m_dailyChallengeCountGroup.gameObject.SetActive(false);
			m_energyUseGroup.gameObject.SetActive(true);
			m_difficultyGroup.gameObject.SetActive(true);
			imgScenarioOn.enabled = repeatScenario;
			m_btnSweep.gameObject.SetActive(m_currentStageTable.n_AP > 0);
			if (m_btnAddEP != null)
			{
				m_btnAddEP.gameObject.SetActive(m_currentStageTable.n_AP > 0);
			}
			bGM_Default = BGM_TimeLimit;
			break;
		}
		case TabType.BOSSRUSH:
		{
			EVENT_TABLE selectedTimedEventTable = GetSelectedTimedEventTable();
			num = selectedTimedEventTable.n_ID;
			UpdateRankingInfoBossRush(selectedTimedEventTable);
			m_eventTime.text = OrangeGameUtility.DisplayDatePeriod(selectedTimedEventTable.s_BEGIN_TIME, selectedTimedEventTable.s_END_TIME);
			EnableTimedEventReportPanel(false);
			EnableBossRushReportPanel(true);
			m_btnShop.gameObject.SetActive(selectedTimedEventTable.n_SHOP != 0);
			m_btnTasks.gameObject.SetActive(selectedTimedEventTable.n_MISSION != 0);
			m_eventDropPanel.gameObject.SetActive(false);
			m_dailyChallengeCountGroup.gameObject.SetActive(false);
			m_energyUseGroup.gameObject.SetActive(false);
			m_difficultyGroup.gameObject.SetActive(false);
			m_btnSweep.gameObject.SetActive(false);
			bGM_Default = BGM_BossRush;
			break;
		}
		default:
			m_eventTime.text = string.Empty;
			m_rankPanelMini.gameObject.SetActive(false);
			EnableTimedEventReportPanel(false);
			EnableBossRushReportPanel(false);
			m_btnShop.gameObject.SetActive(false);
			m_btnTasks.gameObject.SetActive(false);
			m_eventDropPanel.gameObject.SetActive(true);
			m_dailyChallengeCountGroup.gameObject.SetActive(true);
			m_energyUseGroup.gameObject.SetActive(false);
			m_difficultyGroup.gameObject.SetActive(true);
			m_btnSweep.gameObject.SetActive(true);
			bGM_Default = BGM_Default;
			break;
		}
		EVENT_TABLE value;
		if (num != 0 && ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(num, out value) && !string.IsNullOrEmpty(value.s_BGM))
		{
			string[] array = value.s_BGM.Split(',');
			if (array.Length == 2)
			{
				bGM_Default.acbName = array[0];
				bGM_Default.cueName = array[1];
			}
		}
		DisplayRewardList();
		RefreshDifficultyMeter();
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(bGM_Default.acbName, bGM_Default.cueName);
		bChangePage = false;
	}

	private void UpdateRankingInfoBossRush(EVENT_TABLE eventTable)
	{
		m_bossRushClearTimeMin.text = "--";
		m_bossRushClearTimeSec.text = "--";
		m_bossRushClearTimeFracSec.text = "--";
		m_bossRushRank.text = "-";
		m_bossRushScore.text = "-";
		m_bossRushBonus.text = "-";
		m_bossRushBonusBtn.gameObject.SetActive(eventTable.n_POINT != 0);
		if (ManagedSingleton<PlayerNetManager>.Instance.dicBossRushInfo.ContainsKey(eventTable.n_ID))
		{
			BossRushInfo bossRushInfo = ManagedSingleton<PlayerNetManager>.Instance.dicBossRushInfo[eventTable.n_ID];
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(bossRushInfo.netBRInfo.ClearTime);
			m_bossRushClearTimeMin.text = string.Format("{0:00}", timeSpan.Minutes);
			m_bossRushClearTimeSec.text = string.Format("{0:00}", timeSpan.Seconds);
			m_bossRushClearTimeFracSec.text = string.Format("{0:00}", timeSpan.Milliseconds / 10);
			if (eventTable.n_POINT != 0)
			{
				m_bossRushScore.text = bossRushInfo.netBRInfo.Score.ToString();
				m_bossRushBonus.text = string.Format("{0}%", bossRushInfo.netBRInfo.TotalBonus * OrangeConst.BOSSRUSH_BOOST);
			}
		}
		m_rankPanelMini.gameObject.SetActive(eventTable.n_RANKING >= 1);
		if (eventTable.n_RANKING == 0)
		{
			return;
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveEventRankingReq(eventTable.n_ID, 1, 10, delegate(List<EventRankingInfo> eventRankingInfoList)
			{
				m_rankPanelMini.Setup(eventRankingInfoList, eventTable.n_ID);
			});
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			ManagedSingleton<PlayerNetManager>.Instance.RetrievePersonnelEventRankingReq(eventTable.n_ID, delegate(EventRankingInfo info)
			{
				m_eventRankingInfo = info;
				m_bossRushRank.text = m_eventRankingInfo.Ranking.ToString();
			});
		}
	}

	private void UpdateRankingInfoTimeLimited(EVENT_TABLE eventTable)
	{
		m_limitedEventScore.text = "-";
		m_limitedEventRank.text = "-";
		if (eventTable.n_COUNTER != 0)
		{
			m_limitedEventScore.text = ManagedSingleton<MissionHelper>.Instance.GetMissionProgressCount(eventTable.n_COUNTER).ToString();
		}
		int rewardItemID = GetEventRewardItemID(eventTable);
		m_limitedEventItems[0].gameObject.SetActive(rewardItemID != 0);
		ITEM_TABLE value;
		if (rewardItemID != 0 && ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(rewardItemID, out value))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(value.s_ICON), value.s_ICON, delegate(Sprite obj)
			{
				m_limitedEventItems[0].gameObject.SetActive(true);
				m_limitedEventItems[0].GetComponentInChildren<Image>().sprite = obj;
				m_limitedEventItems[0].GetComponentInChildren<OrangeText>().text = "x" + ManagedSingleton<PlayerHelper>.Instance.GetItemValue(rewardItemID);
			});
		}
		m_rankPanelMini.gameObject.SetActive(eventTable.n_RANKING >= 1);
		if (eventTable.n_RANKING == 0)
		{
			return;
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveEventRankingReq(eventTable.n_ID, 1, 10, delegate(List<EventRankingInfo> eventRankingInfoList)
			{
				m_rankPanelMini.Setup(eventRankingInfoList, eventTable.n_ID);
			});
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			ManagedSingleton<PlayerNetManager>.Instance.RetrievePersonnelEventRankingReq(eventTable.n_ID, delegate(EventRankingInfo info)
			{
				m_eventRankingInfo = info;
				m_limitedEventRank.text = m_eventRankingInfo.Ranking.ToString();
			});
		}
	}

	private int GetEventRewardItemID(EVENT_TABLE eventTable)
	{
		if (eventTable.n_BOXGACHA != 0)
		{
			List<BOXGACHA_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.BOXGACHA_TABLE_DICT.Values.Where((BOXGACHA_TABLE x) => x.n_GROUP == eventTable.n_BOXGACHA).ToList();
			if (list.Count != 0)
			{
				return list[0].n_COIN_ID;
			}
		}
		else if (eventTable.n_SHOP != 0)
		{
			HOWTOGET_TABLE table;
			if (ManagedSingleton<OrangeDataManager>.Instance.HOWTOGET_TABLE_DICT.TryGetValue(eventTable.n_SHOP, out table))
			{
				foreach (SHOP_TABLE item in ManagedSingleton<OrangeDataManager>.Instance.SHOP_TABLE_DICT.Values.Where((SHOP_TABLE x) => x.n_MAIN_TYPE == 3 && x.n_SUB_TYPE == table.n_VALUE_Y).ToList())
				{
					if (ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(item.s_BEGIN_TIME, item.s_END_TIME, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
					{
						return item.n_COIN_ID;
					}
				}
			}
		}
		return 0;
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (m_currentStageTable.n_GET_MONEY != 0)
		{
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(1, out item))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(item);
				});
			}
			return;
		}
		if (m_currentStageTable.n_GET_EXP != 0)
		{
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(5, out item))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(item);
				});
			}
			return;
		}
		GACHA_TABLE gACHA_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(m_currentStageTable.n_GET_REWARD)[p_idx];
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(gACHA_TABLE.n_REWARD_ID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			});
		}
	}

	private void DisplayRewardList()
	{
		int num = 0;
		foreach (IconBase item in m_listReward)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		m_listReward.Clear();
		Vector3 vector = new Vector3(144f, 0f);
		List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(m_currentStageTable.n_GET_REWARD);
		int count = listGachaByGroup.Count;
		for (int i = 0; i < count; i++)
		{
			ItemIconBase itemIconBase = UnityEngine.Object.Instantiate(m_rewardIcon, m_rewardIcon.transform.parent.transform);
			itemIconBase.transform.localPosition += i * vector;
			if (i >= count)
			{
				itemIconBase.Clear();
			}
			else
			{
				GACHA_TABLE gACHA_TABLE = listGachaByGroup[i];
				NetRewardInfo netGachaRewardInfo = new NetRewardInfo
				{
					RewardType = (sbyte)gACHA_TABLE.n_REWARD_TYPE,
					RewardID = gACHA_TABLE.n_REWARD_ID,
					Amount = gACHA_TABLE.n_AMOUNT_MAX
				};
				string bundlePath = string.Empty;
				string assetPath = string.Empty;
				int rare = 0;
				MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
				itemIconBase.Setup(i, bundlePath, assetPath, OnClickItem);
				itemIconBase.SetRare(rare);
			}
			m_listReward.Add(itemIconBase);
			num++;
		}
		if (m_currentStageTable.n_GET_MONEY != 0)
		{
			ItemIconBase itemIconBase2 = UnityEngine.Object.Instantiate(m_rewardIcon, m_rewardIcon.transform.parent.transform);
			itemIconBase2.transform.localPosition += num * vector;
			NetRewardInfo netGachaRewardInfo2 = new NetRewardInfo
			{
				RewardType = 1,
				RewardID = 1,
				Amount = m_currentStageTable.n_GET_MONEY
			};
			string bundlePath2 = string.Empty;
			string assetPath2 = string.Empty;
			int rare2 = 0;
			MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo2, ref bundlePath2, ref assetPath2, ref rare2);
			itemIconBase2.Setup(num, bundlePath2, assetPath2, OnClickItem);
			itemIconBase2.SetRare(rare2);
			m_listReward.Add(itemIconBase2);
			num++;
		}
		if (m_currentStageTable.n_GET_EXP != 0)
		{
			ItemIconBase itemIconBase3 = UnityEngine.Object.Instantiate(m_rewardIcon, m_rewardIcon.transform.parent.transform);
			itemIconBase3.transform.localPosition += num * vector;
			NetRewardInfo netGachaRewardInfo3 = new NetRewardInfo
			{
				RewardType = 1,
				RewardID = 5,
				Amount = m_currentStageTable.n_GET_EXP
			};
			string bundlePath3 = string.Empty;
			string assetPath3 = string.Empty;
			int rare3 = 0;
			MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo3, ref bundlePath3, ref assetPath3, ref rare3);
			itemIconBase3.Setup(num, bundlePath3, assetPath3, OnClickItem);
			itemIconBase3.SetRare(rare3);
			m_listReward.Add(itemIconBase3);
			num++;
		}
	}

	public void OnClickTabLock(int tabIndex)
	{
		STAGE_TABLE stageTable = m_eventStageTables[tabIndex].FirstOrDefault();
		if (m_currentPlayerLV < stageTable.n_RANK)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
			{
				string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), stageTable.n_RANK.ToString());
				ui.Setup(p_msg);
			});
		}
		else if (m_currentPlayerCheat)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PLUGIN_BANPLAY");
				ui.Setup(str);
			});
		}
	}

	public void OnClickSkillItemTab(bool bEnable)
	{
		if (bEnable)
		{
			EventTabHelper(TabType.SKILLITEM);
		}
	}

	public void OnClickGoldTab(bool bEnable)
	{
		if (bEnable)
		{
			EventTabHelper(TabType.GOLD);
		}
	}

	public void OnClickArmorItemTab(bool bEnable)
	{
		if (bEnable)
		{
			EventTabHelper(TabType.ARMORITEM);
		}
	}

	public void OnClickExpTab(bool bEnable)
	{
		if (bEnable)
		{
			EventTabHelper(TabType.EXP);
		}
	}

	public void OnClickSpTab(bool bEnable)
	{
		if (bEnable)
		{
			EventTabHelper(TabType.SP);
		}
	}

	private bool IsInitComplete(TabType tabType, int tabIndex)
	{
		if (!bIsInit)
		{
			TabType tabType2 = initTabType;
			if ((uint)(tabType2 - 4) > 1u)
			{
				int num = 7;
				initTabType = tabType;
				initTabIndex = tabIndex;
			}
		}
		return bIsInit;
	}

	public override void OnClickCloseBtn()
	{
		Clear();
		base.OnClickCloseBtn();
	}

	public void OnClickAddEventEP()
	{
		if (!IsTimedEventAndStartable())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CANNOT_BUY_EVENT_STAMINA"), true);
			});
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI ui)
		{
			ui.Setup(ChargeType.EventActionPoint);
			ui.closeCB = delegate
			{
				UpdateEnergyValue();
			};
		});
	}

	private void EnableTimedEventReportPanel(bool bEnable, float animTime = 0.2f)
	{
		float num = 0f;
		CanvasGroup eventReportCanvasGroup = m_eventReport.GetComponent<CanvasGroup>();
		num = eventReportCanvasGroup.alpha;
		LeanTween.cancel(ref m_timedEventAlphaTweenId);
		if (bEnable)
		{
			m_timedEventAlphaTweenId = LeanTween.value(num, 1f, animTime).setOnUpdate(delegate(float alpha)
			{
				m_eventReport.gameObject.SetActive(true);
				eventReportCanvasGroup.alpha = alpha;
			}).uniqueId;
			return;
		}
		m_timedEventAlphaTweenId = LeanTween.value(num, 0f, animTime).setOnUpdate(delegate(float alpha)
		{
			eventReportCanvasGroup.alpha = alpha;
		}).setOnComplete((Action)delegate
		{
			m_eventReport.gameObject.SetActive(false);
		})
			.uniqueId;
	}

	private void EnableBossRushReportPanel(bool bEnable, float animTime = 0.2f)
	{
		float num = 0f;
		CanvasGroup eventReportCanvasGroup = m_eventReportBossRush.GetComponent<CanvasGroup>();
		num = eventReportCanvasGroup.alpha;
		LeanTween.cancel(ref m_bossRushAlphaTweenId);
		if (bEnable)
		{
			m_bossRushAlphaTweenId = LeanTween.value(num, 1f, animTime).setOnUpdate(delegate(float alpha)
			{
				m_eventReportBossRush.gameObject.SetActive(true);
				eventReportCanvasGroup.alpha = alpha;
			}).uniqueId;
			return;
		}
		m_bossRushAlphaTweenId = LeanTween.value(num, 0f, animTime).setOnUpdate(delegate(float alpha)
		{
			eventReportCanvasGroup.alpha = alpha;
		}).setOnComplete((Action)delegate
		{
			m_eventReportBossRush.gameObject.SetActive(false);
		})
			.uniqueId;
	}

	private void EnableChallengeToggle()
	{
		if (m_currentSelectedTab == TabType.TIMELIMITED && m_challengeStageDict.ContainsKey(m_currentStageTable.n_MAIN))
		{
			m_challengeToggleGroup.gameObject.SetActive(true);
		}
		else
		{
			m_challengeToggleGroup.gameObject.SetActive(false);
		}
	}

	private void EnableChallengePanel(bool bEnable)
	{
		float time = 0.5f;
		float num = 570f;
		float num2 = 0f;
		float num3 = 1370f;
		float num4 = 815f;
		float to = (bEnable ? num : num3);
		float to2 = (bEnable ? num2 : num4);
		m_difficultyPlus.interactable = !bEnable;
		m_difficultyMinus.interactable = !bEnable;
		SetChallengeTip("");
		LeanTween.cancel(ref m_difficultyBarPanelTweenID);
		RectTransform rt = m_difficultyBarPanel.GetComponent<RectTransform>();
		m_difficultyBarPanelTweenID = LeanTween.value(rt.sizeDelta.x, to, time).setOnUpdate(delegate(float val)
		{
			rt.sizeDelta = new Vector2(val, rt.sizeDelta.y);
		}).setEaseOutExpo()
			.uniqueId;
		LeanTween.cancel(ref m_difficultyBarMaskTweenID);
		RectTransform rt2 = m_difficultyBarMask.GetComponent<RectTransform>();
		m_difficultyBarMaskTweenID = LeanTween.value(rt2.sizeDelta.x, to2, time).setOnUpdate(delegate(float val)
		{
			rt2.sizeDelta = new Vector2(val, rt2.sizeDelta.y);
		}).setEaseOutExpo()
			.uniqueId;
		if (bEnable)
		{
			m_difficultyBarText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SPEEDRUN_CHALLENGE");
		}
		else
		{
			m_difficultyBarText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SELECT_DIFFICULTY");
		}
	}

	public void OnClickChallengeToggle()
	{
		if (m_currentMode != ModeType.CHALLENGE && m_toggleChallenge.isOn)
		{
			m_currentMode = ModeType.CHALLENGE;
			if (!bChangePage)
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			}
			if (m_currentSelectedTab == TabType.TIMELIMITED)
			{
				ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(m_challengeStageDict[m_currentStageTable.n_MAIN], out m_currentStageTable);
				m_availableDifficulty = 1;
				m_stageTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(m_currentStageTable.w_NAME);
				ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(m_currentStageTable.n_ID, out m_stageInfo);
				SetStageRank();
				m_energyUseCount.text = string.Format("x{0}", m_currentStageTable.n_AP);
				m_btnSweep.gameObject.SetActive(false);
				m_energyUseGroup.gameObject.SetActive(false);
			}
			EnableChallengePanel(true);
			m_normalModeFX.SetActive(false);
			m_challengeModeFX.SetActive(true);
			m_btnReward.gameObject.SetActive(IsRewardAvailable());
		}
	}

	public void OnClickNormalToggle()
	{
		if (m_currentMode != 0 && m_toggleNormal.isOn)
		{
			m_currentMode = ModeType.NORMAL;
			if (!bChangePage)
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			}
			if (m_currentSelectedTab == TabType.TIMELIMITED)
			{
				m_currentStageTable = m_openTimedStageTables[m_currentSelectedTabIndex][m_currentDifficulty - 1];
				m_availableDifficulty = m_openTimedStageTables[m_currentSelectedTabIndex].Count;
				m_stageTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(m_currentStageTable.w_NAME);
				ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(m_currentStageTable.n_ID, out m_stageInfo);
				SetStageRank();
				m_energyUseGroup.gameObject.SetActive(true);
				imgScenarioOn.enabled = repeatScenario;
				m_energyUseCount.text = string.Format("x{0}", m_currentStageTable.n_AP);
				m_btnSweep.gameObject.SetActive(m_currentStageTable.n_AP > 0);
			}
			EnableChallengePanel(false);
			m_normalModeFX.SetActive(true);
			m_challengeModeFX.SetActive(false);
			m_btnReward.gameObject.SetActive(IsRewardAvailable());
		}
	}

	private bool IsChallengeMode()
	{
		if (m_challengeToggleGroup == null)
		{
			return false;
		}
		if (!m_challengeToggleGroup.gameObject.activeSelf)
		{
			return false;
		}
		return m_toggleChallenge.isOn;
	}

	public void OnClickRepeatScenarioBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		repeatScenario = !repeatScenario;
		imgScenarioOn.enabled = repeatScenario;
	}
}
