#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using NaughtyAttributes;
using OrangeApi;
using OrangeSocket;
using OrangeUIAnimEnums;
using UnityEngine;
using UnityEngine.UI;
using cc;
using enums;

public class UI_Challenge : OrangeUIBase
{
	public enum ChallengeTab
	{
		CG_BOSS = 0,
		CG_TOWER = 1,
		CG_SPEED = 2,
		CG_END = 3
	}

	public enum TowerDifficultyType
	{
		TOWER_NORMAL = 0,
		TOWER_HARD = 1,
		TOWER_CHALLENGE = 2
	}

	private const int visualCount = 8;

	private string phaseFormat = "Phase {0}";

	private AnimStatus animState;

	[SerializeField]
	private GameObject delayVisualObj;

	[SerializeField]
	private OrangeText textPhase;

	[SerializeField]
	private StarClearComponent phase;

	[SerializeField]
	private Transform ConditionParent;

	[SerializeField]
	private BossChallengeUnit[] arrayUnit = new BossChallengeUnit[8];

	[SerializeField]
	private GameObject arrowObjLast;

	[SerializeField]
	private GameObject arrowObjNext;

	private List<STAGE_TABLE> listStage = new List<STAGE_TABLE>();

	private int nowPage;

	private int maxPage;

	[SerializeField]
	private Canvas RootTrail;

	[SerializeField]
	private Canvas RootBossChallenge;

	[SerializeField]
	private Canvas RootSpeed;

	[SerializeField]
	private Button ChapterSweepBtn;

	[Header("Tower Tab")]
	[SerializeField]
	private Image BackgroundImage;

	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private TowerFloorUnit unit;

	[SerializeField]
	private Text FloorNameText;

	[SerializeField]
	private Text FloorPowerText;

	[SerializeField]
	private GameObject[] IconRoots;

	[SerializeField]
	private GameObject[] BossInfoRoots;

	[SerializeField]
	private Image[] BossIconRoots;

	[SerializeField]
	private Text[] BossHPText;

	[SerializeField]
	private Image[] BossHPImage;

	[SerializeField]
	private Button normalBtn;

	[SerializeField]
	private Button hardBtn;

	[SerializeField]
	private Button[] DiffBtn;

	[SerializeField]
	private GameObject normalBGRoot;

	[SerializeField]
	private GameObject hardBGRoot;

	[SerializeField]
	private GameObject[] DiffBGRoot;

	[SerializeField]
	private Image StageRuleIconImage;

	[SerializeField]
	private Text StageRuleText;

	[SerializeField]
	private Button GoBtn;

	[SerializeField]
	private GameObject[] ArrowRoot;

	[SerializeField]
	private Slider TowerSlider;

	[SerializeField]
	private Image LevelUpImage;

	[SerializeField]
	private Text TowerLevelUpText;

	[SerializeField]
	private Text DailyRewardText;

	[SerializeField]
	private Text TotalRewardText;

	[SerializeField]
	private Button GetRewardBtn;

	[SerializeField]
	private GameObject rewardIconRoot;

	[SerializeField]
	private GameObject RewardRoot;

	[SerializeField]
	private GameObject LockGroup;

	[SerializeField]
	private Text EventTimeText;

	[SerializeField]
	private GameObject TowerEventTimeRoot;

	private List<STAGE_TABLE> listNormalStageData;

	private List<STAGE_TABLE> listHardStageData;

	private List<STAGE_TABLE> listChallengeStageData;

	private List<STAGE_TABLE> listStageData;

	private TowerFloorUnit selectUnit;

	private int[] stageSkills = new int[3];

	private int nowSelect;

	private TowerDifficultyType TowerDiffType;

	private int CurrentSkillRule;

	private int CurrentSkillRuleMax;

	private bool CurrentTabState = true;

	private bool bGoBtnActive = true;

	private OrangeBgExt m_bgExt;

	private bool TowerInitFlag;

	private bool bTowerLevelLock = true;

	private bool bPlayEndDragSound;

	private OrangeScrollSePlayer scrollSePlayer;

	[SerializeField]
	private GameObject TabSpeed;

	[SerializeField]
	private GameObject LockSpeedGroup;

	[SerializeField]
	private Image PowerStageFrame;

	[SerializeField]
	private Image ChallengePowerStageFrame;

	[SerializeField]
	private GameObject SpeedUpdateRoot;

	[SerializeField]
	private Image SpeedStageImage;

	[SerializeField]
	private Text StageNameText;

	[SerializeField]
	private Text MyRecordText;

	[SerializeField]
	private Text BestRecordText;

	[SerializeField]
	private GameObject[] SpeedRewardRoots;

	[SerializeField]
	private Text[] SpeedRankTexts;

	[SerializeField]
	private Image[] RankImageRoots;

	[SerializeField]
	private Image RankImageA;

	[SerializeField]
	private Image RankImageB;

	[SerializeField]
	private Text BestPlayerNameText;

	[SerializeField]
	private GameObject RankRoot;

	[SerializeField]
	private Text[] RankingPlayerNameTexts;

	[SerializeField]
	private Image RankFlagImage1;

	[SerializeField]
	private Image RankFlagImage2;

	[SerializeField]
	private Image RankFlagImage3;

	[SerializeField]
	private Image RankBarImage;

	[SerializeField]
	private Image BtnRewardEventImage;

	[SerializeField]
	private Image TabSpeedEventImage;

	private Color clear = Color.clear;

	private Color white = Color.white;

	private bool bSpeedDataAllReady;

	private int CurrentSelectTab;

	private bool bSpeedLevelLock = true;

	private int NowSpeedPowerStageID;

	private int NowSpeedChallengeStageID;

	private int NowSpeedPowerScore = int.MaxValue;

	private int NowSpeedChallengeScore = int.MaxValue;

	private int NowSpeedMode = 1;

	private STAGE_TABLE NowSpeedPowerStageTable;

	private STAGE_TABLE NowSpeedChallengeStageTable;

	private List<NetTAStageInfo> NetTAStageInfoList;

	private int[] SpeedRankScore = new int[5];

	private bool bSpeedRewardEvent;

	[Header("Deep Record")]
	[SerializeField]
	private Canvas CanvasDeepRecord;

	[SerializeField]
	private Button BtnDeepRecord;

	[SerializeField]
	private GameObject GoDeepRecordLock;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_reward;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_speedChangeSE = SystemSE.CRI_SYSTEMSE_SYS_CURSOR07;

	private ChallengeTab CurrentChallengeTab;

	private WaitForSeconds waitForSec;

	private StarConditionComponent starConditionComponent;

	private bool b_playDelay;

	private bool bShieldPlaySound;

	private EventRankingInfo m_EventRankingInfo;

	private STAGE_TABLE m_StageTable;

	private EVENT_TABLE m_EventTable;

	public int NowPage
	{
		get
		{
			return nowPage;
		}
		set
		{
			nowPage = value;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.BossChallengePage = nowPage;
		}
	}

	public bool IsBossChallenge
	{
		get
		{
			return CurrentSelectTab == 0;
		}
	}

	public void Update()
	{
		if (CurrentChallengeTab == ChallengeTab.CG_SPEED)
		{
			bool flag = NowSpeedMode == 1;
			PowerStageFrame.color = (flag ? white : clear);
			ChallengePowerStageFrame.color = (flag ? clear : white);
		}
	}

	public void Start()
	{
		scrollSePlayer = scrollRect.content.GetComponent<OrangeScrollSePlayer>();
		scrollSePlayer.enabled = false;
		NowPage = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.BossChallengePage;
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", 41);
		m_bgExt = Background as OrangeBgExt;
		listStage = ManagedSingleton<OrangeTableHelper>.Instance.GetListStageByType(StageType.BossChallenge);
		Debug.Log("[BossChallenge] Num:" + listStage.Count);
		maxPage = ((listStage.Count % 8 == 0) ? (listStage.Count / 8 - 1) : (listStage.Count / 8));
		if (nowPage > maxPage)
		{
			NowPage = 0;
		}
		UpdateChapterSweepBtn();
		bTowerLevelLock = GetTowerMainID() <= 0;
		if (!bTowerLevelLock)
		{
			bTowerLevelLock = OrangeConst.OPENRANK_TOWER > ManagedSingleton<PlayerHelper>.Instance.GetLV();
		}
		LockGroup.gameObject.SetActive(bTowerLevelLock);
		bSpeedLevelLock = OrangeConst.OPENRANK_SPEEDRUN > ManagedSingleton<PlayerHelper>.Instance.GetLV();
		LockSpeedGroup.gameObject.SetActive(bSpeedLevelLock);
		if (!bSpeedLevelLock && ManagedSingleton<PlayerHelper>.Instance.GetUseCheatPlugIn())
		{
			TabSpeed.GetComponent<Button>().interactable = false;
			OrangeGameUtility.CreateLockObj(TabSpeed.transform, UIOpenChk.ChkBanEnum.OPENBAN_SPEED);
		}
		bSpeedRewardEvent = CheckSpeedRewardEvent();
		BtnRewardEventImage.gameObject.SetActive(bSpeedRewardEvent);
		TabSpeedEventImage.gameObject.SetActive(bSpeedRewardEvent);
		UpdateDeepRecordBtnStatus();
		switch ((ChallengeTab)MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeTab)
		{
		default:
			CurrentSelectTab = 0;
			StartCoroutine(OnsSetUnit());
			break;
		case ChallengeTab.CG_TOWER:
			bShieldPlaySound = true;
			CurrentSelectTab = 1;
			ChangeRootTab(1);
			bShieldPlaySound = false;
			break;
		case ChallengeTab.CG_SPEED:
			bShieldPlaySound = true;
			NowSpeedMode = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.NowSpeedMode;
			CurrentSelectTab = 2;
			ChangeRootTab(2);
			bShieldPlaySound = false;
			break;
		}
		closeCB = (Callback)Delegate.Combine(closeCB, (Callback)delegate
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
		});
	}

	private IEnumerator OnsSetUnit()
	{
		m_bgExt.ChangeBackground("Bg_Bosschallenge_BG00");
		for (int j = 0; j < arrayUnit.Length; j++)
		{
			arrayUnit[j].SetInvisable();
		}
		animState = AnimStatus.LOADING;
		waitForSec = new WaitForSeconds(0.15f);
		bool goNext = false;
		textPhase.text = string.Format(phaseFormat, nowPage + 1);
		arrowObjLast.SetActive(HasLastPage());
		arrowObjNext.SetActive(HasNextPage());
		phase.SetActiveStar(nowPage + 1);
		if (starConditionComponent != null)
		{
			if (listStage.Count > 0)
			{
				starConditionComponent.Setup(1002, listStage[nowPage * 8].n_MAIN, listStage[nowPage * 8].n_DIFFICULTY);
			}
			goNext = true;
			animState = AnimStatus.SHOWING;
		}
		else
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/StarConditionComp", "StarConditionComp", delegate(GameObject obj)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(obj, ConditionParent);
				starConditionComponent = gameObject.GetComponent<StarConditionComponent>();
				if (listStage.Count > 0)
				{
					starConditionComponent.Setup(1002, listStage[nowPage * 8].n_MAIN, listStage[nowPage * 8].n_DIFFICULTY);
				}
				goNext = true;
				animState = AnimStatus.SHOWING;
			});
		}
		while (!goNext)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		int playerRank = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		int unitIdx = 0;
		for (int i = nowPage * 8; i < nowPage * 8 + 8; i++)
		{
			yield return waitForSec;
			if (listStage.Count > i)
			{
				StageInfo value = null;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(listStage[i].n_ID, out value))
				{
					arrayUnit[unitIdx].Setup(i, ManagedSingleton<StageHelper>.Instance.GetStarAmount(value.netStageInfo.Star), true, OnClickUnit);
				}
				else
				{
					bool canChallenge = listStage[i].n_RANK <= playerRank;
					arrayUnit[unitIdx].Setup(i, 0, canChallenge, OnClickUnit);
				}
			}
			else
			{
				arrayUnit[unitIdx].Setup(i, 0, false, null);
			}
			unitIdx++;
		}
		animState = AnimStatus.COMPLETE;
		yield return null;
	}

	public void OnClickScreen()
	{
		switch (animState)
		{
		case AnimStatus.SHOWING:
		{
			BossChallengeUnit[] array = arrayUnit;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].IgonreTween();
			}
			waitForSec = null;
			break;
		}
		case AnimStatus.LOADING:
		case AnimStatus.COMPLETE:
			break;
		}
	}

	private void OnClickUnit(int p_idx)
	{
		if (animState != AnimStatus.COMPLETE)
		{
			return;
		}
		STAGE_TABLE stage = listStage[p_idx];
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossStand", delegate(BossStandUI bossStandUI)
		{
			bossStandUI.Setup(stage.w_BOSS_INTRO);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChallengePopup", delegate(UI_ChallengePopup uiChallenge)
			{
				uiChallenge.Setup(stage);
				uiChallenge.closeCB = delegate
				{
					bossStandUI.OnClickCloseBtn();
				};
			});
		});
	}

	public void OnClickPage(int add)
	{
		if (animState == AnimStatus.COMPLETE && nowPage + add <= maxPage && nowPage + add >= 0)
		{
			NowPage += add;
			StartCoroutine(OnsSetUnit());
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		}
	}

	private bool HasNextPage()
	{
		if (nowPage + 1 <= maxPage)
		{
			return true;
		}
		return false;
	}

	private bool HasLastPage()
	{
		if (nowPage - 1 >= 0)
		{
			return true;
		}
		return false;
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SWITCH_SCENE, Clear);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SWITCH_SCENE, Clear);
	}

	private void Clear()
	{
		OnClickCloseBtn();
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	private int GetNowTowerIndex()
	{
		List<STAGE_TABLE> list = listNormalStageData;
		if (TowerDiffType == TowerDifficultyType.TOWER_HARD)
		{
			list = listHardStageData;
		}
		else if (TowerDiffType == TowerDifficultyType.TOWER_CHALLENGE)
		{
			list = listChallengeStageData;
		}
		for (int i = 0; i < list.Count; i++)
		{
			STAGE_TABLE sTAGE_TABLE = list[i];
			StageInfo value = null;
			ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(sTAGE_TABLE.n_ID, out value);
			if (value == null || ManagedSingleton<StageHelper>.Instance.GetStarAmount(value.netStageInfo.Star) <= 0)
			{
				StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
				if (ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(sTAGE_TABLE, ref condition))
				{
					return i;
				}
			}
		}
		return 0;
	}

	public int GetStageDataCount()
	{
		if (TowerDiffType == TowerDifficultyType.TOWER_HARD)
		{
			return listHardStageData.Count;
		}
		if (TowerDiffType == TowerDifficultyType.TOWER_CHALLENGE)
		{
			return listChallengeStageData.Count;
		}
		return listNormalStageData.Count;
	}

	public int GetTowerMainID()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_TOWER, serverUnixTimeNowUTC);
		if (eventTableByType == null || eventTableByType.Count == 0)
		{
			return 0;
		}
		EVENT_TABLE eVENT_TABLE = eventTableByType[0];
		TowerEventTimeRoot.SetActive(false);
		return eVENT_TABLE.n_TYPE_Y;
	}

	public void CloseConnecting()
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	private void UpdateStageData(int mid)
	{
		if (listStageData == null)
		{
			listStageData = ManagedSingleton<OrangeTableHelper>.Instance.GetListStageByMain(mid);
		}
		if (listStageData == null || listStageData.Count < 1)
		{
			return;
		}
		if (listStageData[0].n_SUB <= 1)
		{
			listStageData.Reverse();
		}
		if (listNormalStageData != null && listHardStageData != null && listChallengeStageData != null)
		{
			return;
		}
		listNormalStageData = new List<STAGE_TABLE>();
		listHardStageData = new List<STAGE_TABLE>();
		listChallengeStageData = new List<STAGE_TABLE>();
		for (int i = 0; i < listStageData.Count; i++)
		{
			if (listStageData[i].n_DIFFICULTY == 1)
			{
				listNormalStageData.Add(listStageData[i]);
			}
			else if (listStageData[i].n_DIFFICULTY == 2)
			{
				listHardStageData.Add(listStageData[i]);
			}
			else if (listStageData[i].n_DIFFICULTY == 3)
			{
				listChallengeStageData.Add(listStageData[i]);
			}
		}
	}

	public void OnSetTowerUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		Invoke("CloseConnecting", 1f);
		TowerDiffType = (TowerDifficultyType)MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentDifficultyType;
		UpdateDifficultyBG();
		int towerMainID = GetTowerMainID();
		if (towerMainID <= 0)
		{
			return;
		}
		UpdateStageData(towerMainID);
		if (listChallengeStageData.Count <= 0)
		{
			DiffBtn[2].gameObject.SetActive(false);
		}
		TowerSlider.minValue = 1f;
		TowerSlider.maxValue = GetStageDataCount();
		scrollRect.ThresholdPlus = 200f;
		nowSelect = GetNowTowerIndex();
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentMaxFloor > 0)
		{
			List<STAGE_TABLE> list = listNormalStageData;
			if (TowerDiffType == TowerDifficultyType.TOWER_HARD)
			{
				list = listHardStageData;
			}
			else if (TowerDiffType == TowerDifficultyType.TOWER_CHALLENGE)
			{
				list = listChallengeStageData;
			}
			TowerLevelUpText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOWER_LEVEL_CLEAR"), list[MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentMaxFloor].n_SUB);
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentMaxFloor > nowSelect)
			{
				Invoke("OnShowLevelUp", 2f);
			}
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentMaxFloor = 0;
		scrollRect.OrangeInit(unit, GetStageDataCount(), GetStageDataCount());
		scrollRect.ApplySnapSpeed(1, 0, nowSelect, 300000f, OnBeginDragCB, OnEndDragCB);
		if (!TowerInitFlag)
		{
			TowerInitFlag = true;
			if (nowSelect != 0)
			{
				Invoke("TowerInitMoveToNowTowerIndex", 0.5f);
			}
		}
		OnUpdateRewardData();
		StartCoroutine("CheckDragEnd");
	}

	private IEnumerator CheckDragEnd()
	{
		yield return null;
	}

	public void OnClickChangeRootTab(int typ)
	{
		if (CurrentSelectTab != typ)
		{
			ChangeRootTab(typ);
		}
	}

	public void ChangeRootTab(int typ)
	{
		bool currentTabState = true;
		scrollRect.StopAllCoroutines();
		if (scrollSePlayer.enabled)
		{
			scrollSePlayer.enabled = false;
		}
		switch ((ChallengeTab)typ)
		{
		case ChallengeTab.CG_BOSS:
			currentTabState = true;
			CurrentChallengeTab = ChallengeTab.CG_BOSS;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeTab = 0;
			RootTrail.enabled = false;
			RootSpeed.enabled = false;
			RootBossChallenge.enabled = true;
			StartCoroutine(OnsSetUnit());
			break;
		case ChallengeTab.CG_TOWER:
			if (bTowerLevelLock)
			{
				if (GetTowerMainID() <= 0)
				{
					OnEventNotAvailableTip();
				}
				else
				{
					OnTowerLockTip();
				}
				return;
			}
			currentTabState = false;
			CurrentChallengeTab = ChallengeTab.CG_TOWER;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeTab = 1;
			RootTrail.enabled = true;
			RootSpeed.enabled = false;
			RootBossChallenge.enabled = false;
			bPlayEndDragSound = false;
			OnSetTowerUI();
			break;
		case ChallengeTab.CG_SPEED:
			if (bSpeedLevelLock)
			{
				OnSpeedLockTip();
				return;
			}
			currentTabState = false;
			CurrentChallengeTab = ChallengeTab.CG_SPEED;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeTab = 2;
			RootTrail.enabled = false;
			RootSpeed.enabled = true;
			RootBossChallenge.enabled = false;
			OnSetSpeedUI();
			break;
		}
		ShieldPlaySound(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		CurrentSelectTab = typ;
		CurrentTabState = currentTabState;
	}

	public void SetData(GameObject obj, ref int idx)
	{
		STAGE_TABLE sTAGE_TABLE = null;
		sTAGE_TABLE = ((TowerDiffType == TowerDifficultyType.TOWER_HARD) ? listHardStageData[idx] : ((TowerDiffType != TowerDifficultyType.TOWER_CHALLENGE) ? listNormalStageData[idx] : listChallengeStageData[idx]));
		StageInfo value = null;
		ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(sTAGE_TABLE.n_ID, out value);
		obj.GetComponent<TowerFloorUnit>().SetSubUnitData(sTAGE_TABLE, value);
	}

	public void Rebuild(GameObject obj)
	{
		TowerFloorUnit component = obj.GetComponent<TowerFloorUnit>();
		RectTransform content = scrollRect.content;
		for (int i = 0; i < content.childCount; i++)
		{
			TowerFloorUnit component2 = content.GetChild(i).GetComponent<TowerFloorUnit>();
			if (null != component2 && component2 != component)
			{
				component2.SetUnitActive(false, 1);
				component2.SetUnitActive(true, 0);
			}
		}
		nowSelect = component.NowIdx;
		selectUnit = component;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentStageClear = component.isClear;
		UpdateTowerInfo();
		CancelInvoke("OnSetTowerSliderValue");
		Invoke("OnSetTowerSliderValue", 0.2f);
	}

	public void OnSetTowerSliderValue()
	{
		TowerSlider.value = TowerSlider.maxValue - (float)nowSelect;
	}

	private void UpdateSkillRuleInfo()
	{
		if (stageSkills[CurrentSkillRule] > 0)
		{
			SKILL_TABLE skillTbl = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[stageSkills[CurrentSkillRule]];
			StageRuleText.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl.w_TIP);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(skillTbl.s_ICON), skillTbl.s_ICON, delegate(Sprite asset)
			{
				if (asset != null)
				{
					StageRuleIconImage.sprite = asset;
				}
				else
				{
					Debug.LogWarning("SkillButton.Setup: unable to load sprite " + skillTbl.s_ICON);
				}
			});
			StageRuleIconImage.gameObject.SetActive(true);
		}
		else
		{
			StageRuleIconImage.gameObject.SetActive(false);
			StageRuleText.text = "";
		}
	}

	private void UpdateTowerInfo()
	{
		FloorNameText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOWER_LEVEL"), selectUnit.StageData.n_SUB);
		FloorPowerText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUIDE_SUGGEST") + selectUnit.StageData.n_CP;
		CurrentSkillRule = 0;
		CurrentSkillRuleMax = 1;
		if (selectUnit.StageData.n_STAGE_RULE != 0)
		{
			STAGE_RULE_TABLE sTAGE_RULE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT[selectUnit.StageData.n_STAGE_RULE];
			stageSkills[0] = sTAGE_RULE_TABLE.n_PASSIVE_SKILL1;
			stageSkills[1] = sTAGE_RULE_TABLE.n_PASSIVE_SKILL2;
			stageSkills[2] = sTAGE_RULE_TABLE.n_PASSIVE_SKILL3;
			if (stageSkills[1] != 0)
			{
				CurrentSkillRuleMax++;
			}
			if (stageSkills[2] != 0)
			{
				CurrentSkillRuleMax++;
			}
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.TowerStageRuleTbl = sTAGE_RULE_TABLE;
		}
		UpdateTowerRulePage(CurrentSkillRule);
		List<GACHA_TABLE> listGacha = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(selectUnit.StageData.n_FIRST_REWARD);
		int i = 0;
		while (i < IconRoots.Length)
		{
			GameObject obj = IconRoots[i];
			int childCount = obj.transform.childCount;
			for (int j = 0; j < childCount; j++)
			{
				UnityEngine.Object.Destroy(obj.transform.GetChild(j).gameObject);
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
			{
				CommonIconBase componentInChildren = UnityEngine.Object.Instantiate(asset, obj.transform).GetComponentInChildren<CommonIconBase>();
				componentInChildren.gameObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
				componentInChildren.gameObject.SetActive(true);
				rewardIconRoot.gameObject.SetActive(true);
				if (i >= listGacha.Count)
				{
					componentInChildren.gameObject.SetActive(false);
				}
				else
				{
					ITEM_TABLE item;
					if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(listGacha[i].n_REWARD_ID, out item))
					{
						if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
						{
							componentInChildren.SetItemWithAmountForCard(listGacha[i].n_REWARD_ID, listGacha[i].n_AMOUNT_MAX, OnClickCardInfo);
						}
						else
						{
							componentInChildren.SetItemWithAmount(listGacha[i].n_REWARD_ID, listGacha[i].n_AMOUNT_MAX, OnClickItem);
						}
					}
					componentInChildren.gameObject.SetActive(true);
				}
			});
			int num = i + 1;
			i = num;
		}
		OnSetBackgroundImage();
		BossInfoRoots[0].gameObject.SetActive(false);
		BossInfoRoots[1].gameObject.SetActive(false);
		string w_BOSS_INTRO = selectUnit.StageData.w_BOSS_INTRO;
		if (!(w_BOSS_INTRO != "null"))
		{
			return;
		}
		string[] array = w_BOSS_INTRO.Split(',');
		int[] array2 = new int[2];
		int[] array3 = new int[2];
		if (array2.Length < 1)
		{
			return;
		}
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] == null)
			{
				continue;
			}
			int num2 = int.Parse(array[k]);
			if (!ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT.ContainsKey(num2))
			{
				continue;
			}
			array2[k] = num2;
			BossInfoRoots[k].gameObject.SetActive(true);
			MOB_TABLE mOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[num2];
			array3[k] = mOB_TABLE.n_HP;
			if (k == 0)
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconChip, mOB_TABLE.s_ICON, delegate(Sprite asset)
				{
					if (asset != null)
					{
						BossIconRoots[0].sprite = asset;
					}
				});
				continue;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconChip, mOB_TABLE.s_ICON, delegate(Sprite asset)
			{
				if (asset != null)
				{
					BossIconRoots[1].sprite = asset;
				}
			});
		}
		for (int l = 0; l < array2.Length; l++)
		{
			if (array2[l] == 0)
			{
				continue;
			}
			int num3 = array2[l];
			if (!ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT.ContainsKey(num3))
			{
				continue;
			}
			MOB_TABLE mOB_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[num3];
			int num4 = mOB_TABLE2.n_HP;
			if (ManagedSingleton<PlayerNetManager>.Instance.mmapTowerBossInfoMap.ContainKey(selectUnit.StageData.n_ID))
			{
				List<NetTowerBossInfo> list = ManagedSingleton<PlayerNetManager>.Instance.mmapTowerBossInfoMap[selectUnit.StageData.n_ID];
				for (int m = 0; m < list.Count; m++)
				{
					if (list[m].TowerBossID == num3)
					{
						num4 = list[m].DeductedHP;
						array2[l] = num3;
						array3[l] = num4;
						break;
					}
				}
			}
			if (selectUnit.isClear || num4 < 0)
			{
				num4 = 0;
			}
			num4 = ((num4 > mOB_TABLE2.n_HP) ? mOB_TABLE2.n_HP : num4);
			float x = Mathf.Clamp((float)num4 / (float)mOB_TABLE2.n_HP, 0f, 1f);
			BossHPImage[l].transform.localScale = new Vector3(x, 1f, 1f);
			BossHPText[l].gameObject.SetActive(false);
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SetCurrentTowerInfo(selectUnit.StageData.n_ID, array2, array3);
	}

	public void OnPointerDownSlider()
	{
	}

	public void OnPointerUpSlider()
	{
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			});
		}
	}

	private void OnBeginDragCB(object p_param)
	{
		RectTransform rectTransform = p_param as RectTransform;
		if (null != rectTransform)
		{
			TowerFloorUnit component = rectTransform.GetComponent<TowerFloorUnit>();
			if (null != component)
			{
				component.SetSubUnitData(component.StageData, component.NetStageInfo);
				LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
			}
			scrollSePlayer.enabled = true;
			scrollSePlayer.ResetDis();
		}
	}

	private void OnEndDragCB(object p_param)
	{
		(p_param as RectTransform).GetComponent<TowerFloorUnit>().SetMainUnitData();
		OnPlayEndDragSound(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		bPlayEndDragSound = true;
		scrollSePlayer.enabled = false;
	}

	public void OnClickUnit(RectTransform p_rect)
	{
		if (nowSelect != p_rect.GetComponent<TowerFloorUnit>().NowIdx)
		{
			if (null != selectUnit)
			{
				selectUnit.SetSubUnitData(selectUnit.StageData, selectUnit.NetStageInfo);
			}
			scrollSePlayer.enabled = true;
			scrollSePlayer.ResetDis();
			scrollRect.OnTween(p_rect);
		}
	}

	public void OnClickGoCheck()
	{
		if (!bGoBtnActive)
		{
			return;
		}
		int n_ID = selectUnit.StageData.n_ID;
		StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
		if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(selectUnit.StageData, ref condition))
		{
			ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(selectUnit.StageData, condition);
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS02_STOP);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentMaxFloor = GetNowTowerIndex();
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentDifficultyType = (int)TowerDiffType;
			ui.Setup(selectUnit.StageData);
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.BOSSCHALLENGE;
		});
	}

	public void UpdateDifficultyBG()
	{
		for (int i = 0; i < DiffBtn.Length; i++)
		{
			if (TowerDiffType == (TowerDifficultyType)i)
			{
				DiffBtn[i].interactable = false;
				DiffBGRoot[i].SetActive(true);
			}
			else
			{
				DiffBtn[i].interactable = true;
				DiffBGRoot[i].SetActive(false);
			}
		}
	}

	public void OnChangeDifficultyType(int typ)
	{
		TowerDiffType = (TowerDifficultyType)typ;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentDifficultyType = typ;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentMaxFloor = 0;
		ShieldPlaySound(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07, true);
		UpdateDifficultyBG();
		ChangeRootTab(1);
	}

	public int GetDifficultyType()
	{
		return (int)TowerDiffType;
	}

	public int GetRoofStatus()
	{
		return GetStageDataCount();
	}

	public void OnSetGoBtn(bool atv)
	{
		bGoBtnActive = atv;
		GoBtn.interactable = bGoBtnActive;
	}

	public void OnClickTowerRulePage(int add)
	{
		if (CurrentSkillRule + add <= 2 && CurrentSkillRule + add >= 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
			UpdateTowerRulePage(add);
		}
	}

	private void UpdateTowerRulePage(int add)
	{
		if (CurrentSkillRule + add <= 2 && CurrentSkillRule + add >= 0)
		{
			CurrentSkillRule += add;
			ArrowRoot[0].gameObject.SetActive(true);
			ArrowRoot[1].gameObject.SetActive(true);
			if (CurrentSkillRule == 0)
			{
				ArrowRoot[0].gameObject.SetActive(false);
			}
			if (CurrentSkillRule + 1 == CurrentSkillRuleMax)
			{
				ArrowRoot[1].gameObject.SetActive(false);
			}
			UpdateSkillRuleInfo();
		}
	}

	public void OnSetBackgroundImage()
	{
		m_bgExt.ChangeBackground(selectUnit.StageData.s_BG);
	}

	public void OnTowerSliderChange()
	{
		int num = Convert.ToInt32(TowerSlider.maxValue) - Convert.ToInt32(TowerSlider.value);
		if (nowSelect != num)
		{
			nowSelect = num;
			scrollRect.ApplySnapSpeed(1, 0, num, 10000f, OnBeginDragCB, OnEndDragCB);
			if (bPlayEndDragSound && !scrollSePlayer.enabled)
			{
				scrollSePlayer.enabled = true;
				scrollSePlayer.ResetDis();
			}
		}
	}

	private IEnumerator PlayCursor06()
	{
		while (b_playDelay)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_SCROLL02);
			yield return new WaitForSeconds(0.3f);
		}
	}

	public void OnCloseLevelUp()
	{
		LevelUpImage.gameObject.SetActive(false);
	}

	public void OnShowLevelUp()
	{
		LevelUpImage.gameObject.SetActive(true);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT01);
		Invoke("OnCloseLevelUp", 6f);
	}

	public void OnMoveToNowTowerIndex()
	{
		int nowTowerIndex = GetNowTowerIndex();
		if (nowTowerIndex != nowSelect)
		{
			scrollRect.ApplySnapSpeed(1, 0, nowTowerIndex, 300000f, OnBeginDragCB, OnEndDragCB);
		}
	}

	public void TowerInitMoveToNowTowerIndex()
	{
		nowSelect = GetNowTowerIndex();
		bPlayEndDragSound = false;
		scrollRect.ApplySnapSpeed(1, 0, nowSelect, 300000f, OnBeginDragCB, OnEndDragCB);
	}

	public void OnClickRules()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOWER_RULE"));
		});
	}

	private int GetDailyRewardAmount()
	{
		int num = 0;
		for (int i = 0; i < listNormalStageData.Count; i++)
		{
			STAGE_TABLE sTAGE_TABLE = listNormalStageData[i];
			StageInfo value = null;
			ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(sTAGE_TABLE.n_ID, out value);
			if (value != null && ManagedSingleton<StageHelper>.Instance.GetStarAmount(value.netStageInfo.Star) > 0)
			{
				continue;
			}
			StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
			if (ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(sTAGE_TABLE, ref condition))
			{
				if (i + 1 < listNormalStageData.Count)
				{
					num += listNormalStageData[i + 1].n_GET_REWARD;
				}
				break;
			}
		}
		for (int j = 0; j < listHardStageData.Count; j++)
		{
			STAGE_TABLE sTAGE_TABLE2 = listHardStageData[j];
			StageInfo value2 = null;
			ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(sTAGE_TABLE2.n_ID, out value2);
			if (value2 != null && ManagedSingleton<StageHelper>.Instance.GetStarAmount(value2.netStageInfo.Star) > 0)
			{
				continue;
			}
			StageHelper.StageJoinCondition condition2 = StageHelper.StageJoinCondition.NONE;
			if (ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(sTAGE_TABLE2, ref condition2))
			{
				if (j + 1 < listHardStageData.Count)
				{
					num += listHardStageData[j + 1].n_GET_REWARD;
				}
				break;
			}
		}
		return num;
	}

	public void OnUpdateRewardData()
	{
		DailyRewardText.text = GetDailyRewardAmount().ToString();
		int itemID = OrangeConst.TOWER_DAILY_REWARD_DUMMY;
		bool active = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(itemID);
		RewardRoot.SetActive(active);
		int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(OrangeConst.TOWER_DAILY_REWARD_DUMMY);
		TotalRewardText.text = itemValue.ToString();
		GetRewardBtn.gameObject.SetActive(itemValue > 0);
		int childCount = rewardIconRoot.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(rewardIconRoot.transform.GetChild(i).gameObject);
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			CommonIconBase componentInChildren = UnityEngine.Object.Instantiate(asset, rewardIconRoot.transform).GetComponentInChildren<CommonIconBase>();
			componentInChildren.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
			componentInChildren.gameObject.SetActive(true);
			rewardIconRoot.gameObject.SetActive(true);
			componentInChildren.SetupItem(itemID, 0, OnClickItem);
		});
	}

	public void OnGetReward()
	{
		ManagedSingleton<PlayerNetManager>.Instance.TransferBackupItemReq(delegate(List<NetRewardInfo> obj)
		{
			List<NetRewardInfo> rewardList = new List<NetRewardInfo>();
			rewardList.Clear();
			rewardList.AddRange(obj);
			if (rewardList != null && rewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(rewardList);
					OnUpdateRewardData();
				});
			}
		});
	}

	public void OnEventNotAvailableTip()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NOT_AVAILABLE");
			ui.Setup(str);
		});
	}

	public void OnTowerLockTip()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), OrangeConst.OPENRANK_TOWER.ToString());
			ui.Setup(p_msg);
		});
	}

	public void ShieldPlaySound(SystemSE sc, bool exec = false)
	{
		if (!bShieldPlaySound || exec)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(sc);
			if (!bShieldPlaySound)
			{
				bShieldPlaySound = true;
				Invoke("OnShieldPlaySound", 0.3f);
			}
		}
	}

	public void OnShieldPlaySound()
	{
		bShieldPlaySound = false;
	}

	public void OnPlayEndDragSound(SystemSE sc)
	{
		if (bPlayEndDragSound)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(sc);
		}
	}

	private void OnClickCardInfo(int p_idx)
	{
		ITEM_TABLE item = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item) || item.n_TYPE != 5 || item.n_TYPE_X != 1 || (int)item.f_VALUE_Y <= 0)
		{
			return;
		}
		CARD_TABLE card = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out card))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(card, item);
			});
		}
	}

	public string sec_to_hms(long duration)
	{
		if (duration <= 0 || duration >= int.MaxValue)
		{
			return "--:--.---";
		}
		TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(duration));
		string result = "";
		if (timeSpan.Minutes > 0)
		{
			result = string.Format("{0:00}", timeSpan.Minutes) + ":" + string.Format("{0:00}", timeSpan.Seconds) + "." + string.Format("{0:000}", timeSpan.Milliseconds);
		}
		if (timeSpan.Hours == 0 && timeSpan.Minutes == 0)
		{
			result = "00:" + string.Format("{0:00}", timeSpan.Seconds) + "." + string.Format("{0:000}", timeSpan.Milliseconds);
		}
		return result;
	}

	public string sec_to_ms(long duration)
	{
		if (duration > 1000)
		{
			duration = (long)Mathf.Floor(duration / 1000);
		}
		TimeSpan timeSpan = new TimeSpan(0, 0, Convert.ToInt32(duration));
		string result = "";
		if (timeSpan.Minutes > 0)
		{
			result = string.Format("{0:00}", timeSpan.Minutes) + ":" + string.Format("{0:00}", timeSpan.Seconds);
		}
		if (timeSpan.Minutes == 0)
		{
			result = "00:" + string.Format("{0:00}", timeSpan.Seconds);
		}
		return result;
	}

	private void SetSpeedStageID(int sid)
	{
		STAGE_TABLE stbl;
		if (!ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(sid, out stbl))
		{
			return;
		}
		List<STAGE_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE x) => x.n_MAIN == stbl.n_MAIN).ToList();
		if (list == null || list.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			STAGE_TABLE sTAGE_TABLE = list[i];
			if (sTAGE_TABLE.n_DIFFICULTY == 1 && sTAGE_TABLE.n_SUB == stbl.n_SUB)
			{
				NowSpeedPowerStageID = sTAGE_TABLE.n_ID;
				NowSpeedPowerStageTable = sTAGE_TABLE;
			}
			else if (sTAGE_TABLE.n_DIFFICULTY == 2 && sTAGE_TABLE.n_SUB == stbl.n_SUB)
			{
				NowSpeedChallengeStageID = sTAGE_TABLE.n_ID;
				NowSpeedChallengeStageTable = sTAGE_TABLE;
			}
		}
	}

	public void OnChangeSpeedMode(int mod)
	{
		if (NowSpeedMode != mod)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_speedChangeSE);
			NowSpeedMode = mod;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.NowSpeedMode = NowSpeedMode;
			OnUpdateSpeedUI();
			OnUpdateRecord();
			UpdateRankingPlayerInfo(OnGetSpeedStageID());
		}
	}

	public void OnSetSpeedUI()
	{
		SpeedStageImage.color = clear;
		bSpeedDataAllReady = false;
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveTAStageInfoReq(delegate(RetrieveTAStageInfoRes res)
		{
			SetSpeedStageID(res.StageID);
			NetTAStageInfoList = res.GroupRecordList;
			List<string> list = new List<string>();
			for (int i = 0; i < NetTAStageInfoList.Count; i++)
			{
				if (!(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == NetTAStageInfoList[i].PlayerID))
				{
					list.Add(NetTAStageInfoList[i].PlayerID);
				}
			}
			if (list.Count > 0)
			{
				MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerHUDList, OnCreateRSGetPlayerHUDListCallback, 0, true);
				MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUDList(list.ToArray()));
			}
			else
			{
				bSpeedDataAllReady = true;
			}
			OnUpdateSpeedUI();
			OnUpdateRecord();
			Invoke("OnDelayUpdateRecordInfo", 1.2f);
		});
	}

	public void OnDelayUpdateRecordInfo()
	{
		UpdateRankingPlayerInfo(OnGetSpeedStageID());
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SpeedShowUpdateRoot && (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentNormalDuration > NowSpeedPowerScore || MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeDuration > NowSpeedChallengeScore))
		{
			Invoke("ShowSpeedUpdateRoot", 1.5f);
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SpeedShowUpdateRoot = false;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentNormalDuration = NowSpeedPowerScore;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeDuration = NowSpeedChallengeScore;
		}
	}

	public void ShowSpeedUpdateRoot()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT02);
		SpeedUpdateRoot.SetActive(true);
		Invoke("CloseSpeedUpdateRoot", 3f);
	}

	public void CloseSpeedUpdateRoot()
	{
		SpeedUpdateRoot.SetActive(false);
	}

	public void ReupdateStageFrame()
	{
		bool flag = NowSpeedMode == 1;
		UpdateSpeedModeBg();
		PowerStageFrame.color = (flag ? white : clear);
		ChallengePowerStageFrame.color = (flag ? clear : white);
	}

	private void UpdateSpeedModeBg()
	{
		int nowSpeedMode = NowSpeedMode;
		if (nowSpeedMode != 1)
		{
			m_bgExt.ChangeBackground("UI_speedmode_BG_chall");
		}
		else
		{
			m_bgExt.ChangeBackground("UI_speedmode_BG_fire");
		}
	}

	private Sprite UpdateSocreRankImage(int score)
	{
		for (int i = 1; i <= 3; i++)
		{
			if (score <= SpeedRankScore[i] * 1000)
			{
				return RankImageRoots[i - 1].sprite;
			}
		}
		return RankImageRoots[3].sprite;
	}

	public void OnUpdateSocre()
	{
		RankImageA.gameObject.SetActive(false);
		RankImageB.gameObject.SetActive(false);
		RankFlagImage1.gameObject.SetActive(false);
		RankFlagImage2.gameObject.SetActive(false);
		RankFlagImage3.gameObject.SetActive(false);
		if (NetTAStageInfoList == null)
		{
			return;
		}
		for (int i = 0; i < NetTAStageInfoList.Count; i++)
		{
			NetTAStageInfo netTAStageInfo = NetTAStageInfoList[i];
			if (!(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify != netTAStageInfo.PlayerID))
			{
				NowSpeedPowerScore = netTAStageInfo.NormalDuration;
				RankImageA.gameObject.SetActive(NowSpeedPowerScore < int.MaxValue);
				RankImageA.sprite = UpdateSocreRankImage(NowSpeedPowerScore);
				NowSpeedChallengeScore = netTAStageInfo.ChallengeDuration;
				RankImageB.gameObject.SetActive(NowSpeedChallengeScore < int.MaxValue);
				RankImageB.sprite = UpdateSocreRankImage(NowSpeedChallengeScore);
			}
		}
	}

	private float GetRankFlagImagePos(int score)
	{
		int num = score - SpeedRankScore[0] * 1000;
		num = ((num >= 0) ? num : 0);
		int num2 = SpeedRankScore[4] * 1000 - SpeedRankScore[0] * 1000;
		num = ((num <= num2) ? (num2 - num) : 0);
		return Mathf.Clamp((float)num / (float)num2, 0f, 1f);
	}

	private void UpdateRankingPlayerInfo(int StageID)
	{
		RankRoot.SetActive(false);
		BestRecordText.text = sec_to_hms(0L);
		MyRecordText.text = sec_to_hms(0L);
		RankBarImage.transform.localScale = new Vector3(0f, 1f, 1f);
		List<NetTAStageInfo> netTAStageInfoList = NetTAStageInfoList;
		if (netTAStageInfoList.Count <= 0)
		{
			return;
		}
		int num = int.MaxValue;
		int score = int.MaxValue;
		int num2 = int.MaxValue;
		RankRoot.SetActive(true);
		num2 = ((NowSpeedMode != 1) ? netTAStageInfoList[0].ChallengeDuration : netTAStageInfoList[0].NormalDuration);
		BestRecordText.text = sec_to_hms(num2);
		for (int i = 0; i < RankingPlayerNameTexts.Length; i++)
		{
			if (i < netTAStageInfoList.Count)
			{
				num = ((NowSpeedMode != 1) ? netTAStageInfoList[i].ChallengeDuration : netTAStageInfoList[i].NormalDuration);
				if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == netTAStageInfoList[i].PlayerID)
				{
					score = num;
					MyRecordText.text = sec_to_hms(num);
					RankingPlayerNameTexts[i].text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
				}
				else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.ContainsKey(netTAStageInfoList[i].PlayerID))
				{
					RankingPlayerNameTexts[i].text = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD[netTAStageInfoList[i].PlayerID].m_Name;
				}
				else
				{
					RankingPlayerNameTexts[i].text = netTAStageInfoList[i].PlayerID;
				}
				if (num <= 0 || num >= int.MaxValue)
				{
					RankingPlayerNameTexts[i].text = "";
				}
			}
			else
			{
				RankingPlayerNameTexts[i].text = "";
			}
		}
		float rankFlagImagePos = GetRankFlagImagePos(num2);
		RankBarImage.transform.localScale = new Vector3(rankFlagImagePos, 1f, 1f);
		if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == netTAStageInfoList[0].PlayerID)
		{
			BestPlayerNameText.text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			RankFlagImage2.gameObject.SetActive(true);
			Vector3 vector = RankFlagImage2.rectTransform.anchoredPosition;
			RankFlagImage2.rectTransform.anchoredPosition = new Vector3(720f * rankFlagImagePos, vector.y, vector.z);
			return;
		}
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.ContainsKey(netTAStageInfoList[0].PlayerID))
		{
			BestPlayerNameText.text = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD[netTAStageInfoList[0].PlayerID].m_Name;
		}
		else
		{
			BestPlayerNameText.text = netTAStageInfoList[0].PlayerID;
		}
		RankFlagImage1.gameObject.SetActive(true);
		RankFlagImage3.gameObject.SetActive(true);
		float rankFlagImagePos2 = GetRankFlagImagePos(score);
		Vector3 vector2 = RankFlagImage1.rectTransform.anchoredPosition;
		RankFlagImage1.rectTransform.anchoredPosition = new Vector3(720f * rankFlagImagePos, vector2.y, vector2.z);
		vector2 = RankFlagImage3.rectTransform.anchoredPosition;
		RankFlagImage3.rectTransform.anchoredPosition = new Vector3(720f * rankFlagImagePos2, vector2.y, vector2.z);
	}

	public void OnUpdateRecord()
	{
		OnUpdateSocre();
		if (NetTAStageInfoList == null || NetTAStageInfoList.Count <= 0)
		{
			return;
		}
		MyRecordText.text = sec_to_hms(0L);
		if (NowSpeedMode == 1)
		{
			NetTAStageInfoList.Sort((NetTAStageInfo x, NetTAStageInfo y) => x.NormalDuration.CompareTo(y.NormalDuration));
		}
		else
		{
			NetTAStageInfoList.Sort((NetTAStageInfo x, NetTAStageInfo y) => x.ChallengeDuration.CompareTo(y.ChallengeDuration));
		}
	}

	public void OnUpdateSpeedUI()
	{
		STAGE_TABLE sTAGE_TABLE = OnGetSpeedStageTable();
		if (sTAGE_TABLE == null)
		{
			return;
		}
		StageNameText.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(sTAGE_TABLE.w_NAME);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "background/" + sTAGE_TABLE.s_BG, sTAGE_TABLE.s_BG, delegate(Sprite obj)
		{
			if (null != obj)
			{
				SpeedStageImage.color = white;
				SpeedStageImage.sprite = obj;
			}
		});
		ReupdateStageFrame();
		SpeedRankTexts[0].text = sec_to_ms(sTAGE_TABLE.n_CLEAR_VALUE1);
		SpeedRankTexts[1].text = sec_to_ms(sTAGE_TABLE.n_CLEAR_VALUE2);
		SpeedRankTexts[2].text = sec_to_ms(sTAGE_TABLE.n_CLEAR_VALUE3);
		SpeedRankTexts[3].text = sec_to_ms(sTAGE_TABLE.n_CLEAR_VALUE3 + (sTAGE_TABLE.n_CLEAR_VALUE2 - sTAGE_TABLE.n_CLEAR_VALUE1));
		SpeedRankScore[0] = sTAGE_TABLE.n_CLEAR_VALUE1 - (sTAGE_TABLE.n_CLEAR_VALUE2 - sTAGE_TABLE.n_CLEAR_VALUE1);
		SpeedRankScore[1] = sTAGE_TABLE.n_CLEAR_VALUE1;
		SpeedRankScore[2] = sTAGE_TABLE.n_CLEAR_VALUE2;
		SpeedRankScore[3] = sTAGE_TABLE.n_CLEAR_VALUE3;
		SpeedRankScore[4] = sTAGE_TABLE.n_CLEAR_VALUE3 + (sTAGE_TABLE.n_CLEAR_VALUE2 - sTAGE_TABLE.n_CLEAR_VALUE1);
		OnUpdateSpeedReward(sTAGE_TABLE.n_GET_REWARD);
	}

	private void SetSpeedRewardIcon(int itemID, int itemAmount, Transform tf)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			CommonIconBase componentInChildren = UnityEngine.Object.Instantiate(asset, tf).GetComponentInChildren<CommonIconBase>();
			componentInChildren.gameObject.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
			componentInChildren.gameObject.SetActive(true);
			componentInChildren.SetupItem(itemID, itemID, OnClickItem);
		});
	}

	public void OnUpdateSpeedReward(int group)
	{
		List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(group);
		for (int i = 0; i < SpeedRewardRoots.Length; i++)
		{
			int childCount = SpeedRewardRoots[i].transform.childCount;
			for (int j = 0; j < childCount; j++)
			{
				UnityEngine.Object.Destroy(SpeedRewardRoots[i].transform.GetChild(j).gameObject);
			}
			if (i < listGachaByGroup.Count)
			{
				SetSpeedRewardIcon(listGachaByGroup[i].n_REWARD_ID, listGachaByGroup[i].n_AMOUNT_MAX, SpeedRewardRoots[i].transform);
			}
		}
	}

	public int OnGetSpeedStageID()
	{
		if (NowSpeedMode == 1)
		{
			return NowSpeedPowerStageID;
		}
		return NowSpeedChallengeStageID;
	}

	public STAGE_TABLE OnGetSpeedStageTable()
	{
		if (NowSpeedMode == 1)
		{
			return NowSpeedPowerStageTable;
		}
		return NowSpeedChallengeStageTable;
	}

	public void OnClickSpeedGoCheck()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS02_STOP);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentNormalDuration = NowSpeedPowerScore;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeDuration = NowSpeedChallengeScore;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.NowSpeedMode = NowSpeedMode;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SpeedShowUpdateRoot = true;
			ui.Setup(OnGetSpeedStageID());
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.BOSSCHALLENGE;
		});
	}

	public void OnCreateRSGetPlayerHUDListCallback(object res)
	{
		if (!(res is RSGetPlayerHUDList))
		{
			return;
		}
		RSGetPlayerHUDList rSGetPlayerHUDList = (RSGetPlayerHUDList)res;
		if (rSGetPlayerHUDList.Result == 70300)
		{
			for (int i = 0; i < rSGetPlayerHUDList.PlayerHUDLength; i++)
			{
				SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(rSGetPlayerHUDList.PlayerHUD(i));
				if (socketPlayerHUD != null)
				{
					ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketPlayerHUD.m_PlayerId, socketPlayerHUD);
				}
			}
		}
		bSpeedDataAllReady = true;
	}

	public void OnSpeedLockTip()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), OrangeConst.OPENRANK_SPEEDRUN.ToString());
			ui.Setup(p_msg);
		});
	}

	public void OnClickSpeedRules()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SPEEDRUN_RULE"));
		});
	}

	public void OnClickReward()
	{
		m_StageTable = OnGetSpeedStageTable();
		if (m_StageTable == null)
		{
			return;
		}
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_TIME_ATTACK, serverUnixTimeNowUTC);
		if (eventTableByType != null && eventTableByType.Count > 0)
		{
			m_EventTable = eventTableByType[0];
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EventReward", delegate(EventRewardDialog ui)
		{
			int prank = 0;
			int crank = 0;
			NetTAStageInfo myNetTAStageInfo = GetMyNetTAStageInfo();
			if (myNetTAStageInfo != null)
			{
				prank = myNetTAStageInfo.NormalRank;
				crank = myNetTAStageInfo.ChallengeRank;
			}
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SpeedEventSetup(m_EventTable, m_StageTable, m_EventRankingInfo, prank, crank, NowSpeedMode, bSpeedRewardEvent);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_reward);
		});
	}

	public bool CheckSpeedRewardEvent()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_TIME_ATTACK, serverUnixTimeNowUTC);
		if (eventTableByType == null || eventTableByType.Count == 0)
		{
			return false;
		}
		return true;
	}

	public int GetMySpeedRank()
	{
		string playerIdentify = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		for (int i = 0; i < NetTAStageInfoList.Count; i++)
		{
			if (NetTAStageInfoList[i].PlayerID == playerIdentify)
			{
				if (NowSpeedMode == 1)
				{
					return NetTAStageInfoList[i].NormalRank;
				}
				return NetTAStageInfoList[i].ChallengeRank;
			}
		}
		return 0;
	}

	public NetTAStageInfo GetMyNetTAStageInfo()
	{
		string playerIdentify = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		for (int i = 0; i < NetTAStageInfoList.Count; i++)
		{
			if (NetTAStageInfoList[i].PlayerID == playerIdentify)
			{
				return NetTAStageInfoList[i];
			}
		}
		return null;
	}

	public void OnClickSweep()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossChallengeSweep", delegate(BossChallengeSweepUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup();
		});
	}

	private void UpdateChapterSweepBtn()
	{
		if (ChapterSweepBtn == null)
		{
			return;
		}
		int num = 0;
		foreach (STAGE_TABLE item in listStage)
		{
			StageInfo value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(item.n_ID, out value) && ManagedSingleton<StageHelper>.Instance.GetStarAmount(value.netStageInfo.Star) >= 3)
			{
				num++;
			}
			if (num >= 2)
			{
				ChapterSweepBtn.gameObject.SetActive(true);
				return;
			}
		}
		ChapterSweepBtn.gameObject.SetActive(false);
	}

	private void UpdateDeepRecordBtnStatus()
	{
		CanvasDeepRecord.enabled = true;
		GoDeepRecordLock.gameObject.SetActive(OrangeConst.RECORD_OPEN_LV > ManagedSingleton<PlayerHelper>.Instance.GetLV());
		if (!GoDeepRecordLock.gameObject.activeSelf && ManagedSingleton<PlayerHelper>.Instance.GetUseCheatPlugIn())
		{
			BtnDeepRecord.interactable = false;
			OrangeGameUtility.CreateLockObj(BtnDeepRecord.transform, UIOpenChk.ChkBanEnum.OEPNBAN_DEEP_RECORD);
		}
	}

	public void OnClickDeepRecordBtn()
	{
		if (GoDeepRecordLock.gameObject.activeSelf)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESEARCH_CONDITION"), OrangeConst.RECORD_OPEN_LV));
		}
		else
		{
			ManagedSingleton<DeepRecordHelper>.Instance.RetrieveRecordGridInfoReq();
		}
	}
}
