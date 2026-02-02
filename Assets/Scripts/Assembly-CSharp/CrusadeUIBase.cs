#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public abstract class CrusadeUIBase : OrangeUIBase
{
	private EVENT_TABLE _currentEventTable;

	private STAGE_TABLE _currentStageTable;

	private List<STAGE_TABLE> _stageTableList = new List<STAGE_TABLE>();

	[SerializeField]
	private CrusadeBossHpBarHelper _hpBarHelper;

	[SerializeField]
	private CrusadeGuardianUnit[] _guardianUnitList;

	[SerializeField]
	private Image _imageBossIcon;

	[SerializeField]
	private GameObject _goMainBossSelected;

	[SerializeField]
	private Text _textEventTime;

	[SerializeField]
	private GameObject _eventInfoRoot;

	[SerializeField]
	private Text _textChallengeNum;

	[SerializeField]
	private Text _textContributionNum;

	[SerializeField]
	private Text _textRanking;

	[SerializeField]
	private Text _textBonusNum;

	[SerializeField]
	private GameObject _bottomPanelRoot;

	[SerializeField]
	private CrusadeGuildRankingPanelMini _rankPanelMini;

	[SerializeField]
	private CrusadeBattleSituationHelper _battleSituationHelper;

	[SerializeField]
	private Button _btnDeploy;

	private int _rankNum;

	private bool _isNetLocking;

	[BoxGroup("Sound")]
	[Tooltip("戰鬥準備")]
	[SerializeField]
	private SystemSE m_goCheckBtn = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	private Coroutine _coroutineGuardianCountDown;

	private bool bMuteSE;

	protected virtual void OnEnable()
	{
		MonoBehaviourSingleton<UIManager>.Instance.OnUILinkPrepareEvent += OnUILinkPrepareEvent;
		Singleton<CrusadeSystem>.Instance.OnRetrieveCrusadeInfoEvent += OnRetrieveCrusadeInfoEvent;
		Singleton<CrusadeSystem>.Instance.OnCheckCrusadeInfoEvent += OnCheckCrusadeInfoEvent;
		Singleton<CrusadeSystem>.Instance.OnRetrieveEventRankingEvent += OnRetrieveEventRankingEvent;
		Singleton<CrusadeSystem>.Instance.OnRetrievePersonalEventRankingEvent += OnRetrievePersonalEventRankingEvent;
		Singleton<CrusadeSystem>.Instance.OnPlayerInfoCacheUpdateEvent += OnCrusadePlayerInfoUpdate;
	}

	protected virtual void OnDisable()
	{
		if (_coroutineGuardianCountDown != null)
		{
			StopCoroutine(_coroutineGuardianCountDown);
		}
		MonoBehaviourSingleton<UIManager>.Instance.OnUILinkPrepareEvent -= OnUILinkPrepareEvent;
		Singleton<CrusadeSystem>.Instance.OnRetrieveCrusadeInfoEvent -= OnRetrieveCrusadeInfoEvent;
		Singleton<CrusadeSystem>.Instance.OnCheckCrusadeInfoEvent -= OnCheckCrusadeInfoEvent;
		Singleton<CrusadeSystem>.Instance.OnRetrieveEventRankingEvent -= OnRetrieveEventRankingEvent;
		Singleton<CrusadeSystem>.Instance.OnRetrievePersonalEventRankingEvent -= OnRetrievePersonalEventRankingEvent;
		Singleton<CrusadeSystem>.Instance.OnPlayerInfoCacheUpdateEvent -= OnCrusadePlayerInfoUpdate;
	}

	protected virtual void OnUILinkPrepareEvent()
	{
	}

	public virtual void Setup()
	{
		_textEventTime.text = string.Empty;
		_eventInfoRoot.SetActive(false);
		_hpBarHelper.gameObject.SetActive(false);
		_bottomPanelRoot.SetActive(false);
		UpdateCrusadeInfo();
		switch (Singleton<CrusadeSystem>.Instance.LastTargetBoss)
		{
		case CrusadeBossTarget.MainBoss:
			OnSelectMainBossEvent();
			break;
		case CrusadeBossTarget.Guardian:
			bMuteSE = true;
			if (Singleton<CrusadeSystem>.Instance.LastTargetGuardianIndex == Singleton<CrusadeSystem>.Instance.LastSelectedGuardianBuffIndex)
			{
				OnSelectMainBossEvent();
			}
			else
			{
				OnSelectGuardianEvent(Singleton<CrusadeSystem>.Instance.LastTargetGuardianIndex);
			}
			break;
		}
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	protected virtual void UpdateCrusadeInfo()
	{
		Singleton<CrusadeSystem>.Instance.PassiveSkillID = 0;
		if (InitializeStageData())
		{
			UpdateEventTime();
			UpdateBossInfo();
			InitializeGuardianInfo();
			UpdateEventScoreInfo();
			_bottomPanelRoot.SetActive(true);
			RetrieveEventRanking();
			_battleSituationHelper.ChargeTimeInfoData(Singleton<CrusadeSystem>.Instance.OnTimeRecordListCache);
			ShowReward();
			UpdateBattleEntryState();
		}
	}

	private void UpdateEventTime()
	{
		DateTime uNIX_EPOCH_TIME_LOCAL = DateTimeHelper.UNIX_EPOCH_TIME_LOCAL;
		TimeSpan timeSpan = TimeSpan.FromSeconds(Singleton<CrusadeSystem>.Instance.EventStartTime);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(Singleton<CrusadeSystem>.Instance.EventEndTime);
		DateTime date = uNIX_EPOCH_TIME_LOCAL + timeSpan;
		DateTime date2 = uNIX_EPOCH_TIME_LOCAL + timeSpan2;
		_textEventTime.text = date.ToFullDateString() + " ~ " + date2.ToFullDateString();
	}

	private void RetrieveEventRanking()
	{
		Singleton<CrusadeSystem>.Instance.RetrieveEventRanking(Singleton<CrusadeSystem>.Instance.EventID);
	}

	private void RetrievePersonalEventRanking()
	{
		Singleton<CrusadeSystem>.Instance.RetrievePersonalEventRanking(Singleton<CrusadeSystem>.Instance.EventID);
	}

	private void OnRetrieveEventRankingEvent(int eventId, List<CrusadeEventRankingInfo> rankingInfoList)
	{
		if (eventId != Singleton<CrusadeSystem>.Instance.EventID)
		{
			Debug.LogError(string.Format("EventId mismatch : {0} != {1}", eventId, Singleton<CrusadeSystem>.Instance.EventID));
		}
		else
		{
			UpdateEventRanking(eventId, rankingInfoList);
		}
	}

	private void OnRetrievePersonalEventRankingEvent(int eventId, CrusadeEventRankingInfo rankingInfo)
	{
		if (eventId != Singleton<CrusadeSystem>.Instance.EventID)
		{
			Debug.LogError(string.Format("EventId mismatch : {0} != {1}", eventId, Singleton<CrusadeSystem>.Instance.EventID));
		}
		else
		{
			UpdatePersonalEventRanking(rankingInfo);
		}
	}

	protected virtual void UpdateEventRanking(int eventId, List<CrusadeEventRankingInfo> rankingInfoList)
	{
		_rankPanelMini.Setup(rankingInfoList, eventId);
	}

	protected virtual void UpdatePersonalEventRanking(CrusadeEventRankingInfo rankingInfo)
	{
		_textRanking.text = rankingInfo.Ranking.ToString();
		_rankNum = rankingInfo.Ranking;
	}

	private bool InitializeStageData()
	{
		int eventID = Singleton<CrusadeSystem>.Instance.EventID;
		EVENT_TABLE eventTable;
		if (!ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(eventID, out eventTable))
		{
			Debug.LogError(string.Format("Invalid EventId : {0}", eventID));
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
			return false;
		}
		STAGE_TABLE[] array = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE stageData) => stageData.n_MAIN == eventTable.n_TYPE_Y).ToArray();
		if (array.Length == 0)
		{
			Debug.LogError(string.Format("No Stage for EventId : {0}", eventID));
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
			return false;
		}
		_currentEventTable = eventTable;
		_stageTableList.AddRange(array);
		_stageTableList.Sort((STAGE_TABLE x, STAGE_TABLE y) => x.n_ID.CompareTo(y.n_ID));
		return true;
	}

	private void ShowReward()
	{
		NetRewardsEntity rewardEntities = Singleton<CrusadeSystem>.Instance.RewardEntityCache;
		if (rewardEntities != null && rewardEntities.RewardList != null && rewardEntities.RewardList.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(rewardEntities.RewardList);
			});
		}
	}

	public void UpdateBossInfo()
	{
		NetCrusadeBossInfo bossInfoCache = Singleton<CrusadeSystem>.Instance.BossInfoCache;
		int index = Mathf.Min(bossInfoCache.BossStep - 1, 4);
		_currentStageTable = _stageTableList[index];
		int result;
		int.TryParse(_currentStageTable.w_BOSS_INTRO, out result);
		int hpSet = 0;
		int setNum = 0;
		MOB_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT.TryGetValue(result, out value))
		{
			hpSet = value.n_HP;
			setNum = value.n_HP_STEP;
		}
		_hpBarHelper.gameObject.SetActive(true);
		_hpBarHelper.Setup(bossInfoCache.BossStep, bossInfoCache.TotalHP, hpSet, setNum);
	}

	private void UpdateEventScoreInfo()
	{
		_eventInfoRoot.SetActive(true);
		NetCrusadePlayerInfo playerInfoCache = Singleton<CrusadeSystem>.Instance.PlayerInfoCache;
		_textChallengeNum.text = string.Format("{0}/{1}", playerInfoCache.BattleCount, OrangeConst.GUILD_CRUSADE_PLAYNUMBER);
		_textContributionNum.text = playerInfoCache.Score.ToString();
		_textBonusNum.text = string.Format("{0:0.0}%", (float)Singleton<CrusadeSystem>.Instance.GetCrusadeBonus() * 0.01f);
		RetrievePersonalEventRanking();
	}

	private void UpdateBattleEntryState()
	{
		_btnDeploy.interactable = Singleton<CrusadeSystem>.Instance.CheckInEventTime();
	}

	public void OnSelfEventRankingInfoUpdate(EventRankingInfo rankingInfo)
	{
		_textRanking.text = rankingInfo.Ranking.ToString();
		_rankNum = rankingInfo.Ranking;
	}

	public void OnCrusadePlayerInfoUpdate()
	{
		UpdateEventScoreInfo();
		UpdateBattleEntryState();
	}

	public void OnClickRules()
	{
		if (_currentStageTable != null && !_isNetLocking)
		{
			LoadRuleBonusDialogUI();
		}
	}

	public void OnClickBattleRecord()
	{
		if (_currentStageTable != null && !_isNetLocking)
		{
			LoadBattleRecordUI();
		}
	}

	public void OnClickReward()
	{
		if (_currentStageTable != null && !_isNetLocking)
		{
			LoadBossRewardUI();
		}
	}

	public void OnClickBattleDeployBtn()
	{
		NetCrusadePlayerInfo playerInfoCache = Singleton<CrusadeSystem>.Instance.PlayerInfoCache;
		if (!_isNetLocking)
		{
			if (Singleton<CrusadeSystem>.Instance.LastTargetBoss == CrusadeBossTarget.MainBoss && playerInfoCache.BattleCount <= 0)
			{
				LoadCheckChargeStaminaUI();
				return;
			}
			if (!Singleton<CrusadeSystem>.Instance.CheckInEventTime())
			{
				CommonUIHelper.ShowCommonTipUI("EVENT_OUTDATE");
				return;
			}
			if (!Singleton<CrusadeSystem>.Instance.CanPlayThisEvent)
			{
				CommonUIHelper.ShowCommonTipUI("GUILD_CRUSADE_WARN_1");
				return;
			}
			_isNetLocking = true;
			Singleton<CrusadeSystem>.Instance.CheckCrusadeInfo();
		}
	}

	private void InitializeGuardianInfo()
	{
		if (_coroutineGuardianCountDown != null)
		{
			StopCoroutine(_coroutineGuardianCountDown);
		}
		if (InitGuardianInfo())
		{
			_coroutineGuardianCountDown = StartCoroutine(UpdateGuardianInfoRoutine());
		}
	}

	private bool InitGuardianInfo()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<CrusadeGuardianUnit> list = new List<CrusadeGuardianUnit>();
		EVENT_TABLE eventAttrData;
		if (!ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(Singleton<CrusadeSystem>.Instance.EventID, out eventAttrData))
		{
			Debug.LogError(string.Format("Invalid EventID of EVENT_TABLE_DICT : {0}", Singleton<CrusadeSystem>.Instance.EventID));
			return false;
		}
		STAGE_TABLE[] source = (from data in ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values
			where data.n_TYPE == eventAttrData.n_TYPE_X && data.n_MAIN == eventAttrData.n_TYPE_Y
			orderby data.n_SUB
			select data).ToArray();
		STAGE_TABLE sTAGE_TABLE = source.FirstOrDefault((STAGE_TABLE data) => data.n_SUB == 1);
		_imageBossIcon.enabled = false;
		if (sTAGE_TABLE != null)
		{
			if (!sTAGE_TABLE.s_ICON.IsNullString())
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconCrusade, sTAGE_TABLE.s_ICON, delegate(Sprite sprite)
				{
					_imageBossIcon.sprite = sprite;
					_imageBossIcon.enabled = true;
				});
			}
			else
			{
				Debug.LogError("Invalid IconName of Boss");
			}
		}
		else
		{
			Debug.LogError("No StageAttrData of Boss");
		}
		STAGE_TABLE[] array = (from data in source
			where data.n_SUB > 1
			orderby data.n_SUB
			select data).ToArray();
		for (int i = 0; i < Singleton<CrusadeSystem>.Instance.GuardInfoCacheList.Count; i++)
		{
			if (i >= _guardianUnitList.Length)
			{
				Debug.LogError(string.Format("Index {0} out of Range GuardianUnit {1}", i, _guardianUnitList.Length));
				break;
			}
			if (i >= array.Length)
			{
				Debug.LogError(string.Format("Index {0} out of Range GuardianStageAttrDatas {1}", i, array.Length));
				break;
			}
			NetCrusadeGuardInfo guardInfo = Singleton<CrusadeSystem>.Instance.GuardInfoCacheList[i];
			STAGE_TABLE sTAGE_TABLE2 = array[i];
			CrusadeGuardianUnit crusadeGuardianUnit = _guardianUnitList[i];
			if (crusadeGuardianUnit.Setup(i, serverUnixTimeNowUTC, sTAGE_TABLE2.s_ICON, sTAGE_TABLE2.n_SECRET, guardInfo, OnSelectGuardianEvent, OnSelectGuardianBuffEvent))
			{
				list.Add(crusadeGuardianUnit);
			}
		}
		if (list.Count > 0)
		{
			bMuteSE = true;
			CrusadeBossTarget lastTargetBoss = Singleton<CrusadeSystem>.Instance.LastTargetBoss;
			if (lastTargetBoss == CrusadeBossTarget.Guardian && list.Find((CrusadeGuardianUnit guardianUnit) => guardianUnit.GuardianIndex == Singleton<CrusadeSystem>.Instance.LastTargetGuardianIndex) != null)
			{
				Singleton<CrusadeSystem>.Instance.LastTargetBoss = CrusadeBossTarget.MainBoss;
			}
			CrusadeGuardianUnit crusadeGuardianUnit2 = list.Find((CrusadeGuardianUnit guardianUnit) => guardianUnit.GuardianIndex == Singleton<CrusadeSystem>.Instance.LastSelectedGuardianBuffIndex);
			if (crusadeGuardianUnit2 != null)
			{
				OnSelectGuardianBuffEvent(crusadeGuardianUnit2.GuardianIndex);
			}
			else
			{
				OnSelectGuardianBuffEvent(list[0].GuardianIndex);
			}
		}
		else
		{
			Singleton<CrusadeSystem>.Instance.LastSelectedGuardianBuffIndex = -1;
		}
		return list.Count > 0;
	}

	private IEnumerator UpdateGuardianInfoRoutine()
	{
		while (!UpdateGuardianInfo())
		{
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(1f);
		Singleton<CrusadeSystem>.Instance.RetrieveCrusadeInfo();
	}

	private bool UpdateGuardianInfo()
	{
		long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		bool result = false;
		for (int i = 0; i < Singleton<CrusadeSystem>.Instance.GuardInfoCacheList.Count && i < _guardianUnitList.Length; i++)
		{
			NetCrusadeGuardInfo guardInfo = Singleton<CrusadeSystem>.Instance.GuardInfoCacheList[i];
			if (_guardianUnitList[i].UpdateInfo(serverUnixTimeNowUTC, guardInfo))
			{
				result = true;
			}
		}
		return result;
	}

	private void OnRetrieveCrusadeInfoEvent(Code ackCode)
	{
		if (ackCode == Code.CRUSADE_GET_INFO_SUCCESS || ackCode == Code.CRUSADE_EVENT_NO_OPEN_DATA)
		{
			UpdateCrusadeInfo();
		}
	}

	private void OnCheckCrusadeInfoEvent()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_goCheckBtn);
		UpdateBossInfo();
		LoadGoCheckUI();
	}

	public void OnClickAddChallengeNumBtn()
	{
		if (!Singleton<CrusadeSystem>.Instance.CheckInEventTime())
		{
			CommonUIHelper.ShowCommonTipUI("CANNOT_BUY_CHALLENGE_COUNT");
		}
		else if (!Singleton<CrusadeSystem>.Instance.CanPlayThisEvent)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_CRUSADE_WARN_1");
		}
		else
		{
			LoadChargeStaminaUI();
		}
	}

	public void OnClickDamageBounsHintsBtn()
	{
		LoadRuleBonusDialogUI();
	}

	public override void OnClickCloseBtn()
	{
		if (!_isNetLocking)
		{
			base.OnClickCloseBtn();
		}
	}

	private void LoadGoCheckUI()
	{
		_isNetLocking = true;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GoCheckUI>("UI_GoCheck", OnGoCheckUILoaded);
	}

	private void OnGoCheckUILoaded(GoCheckUI ui)
	{
		ui.nStartTime = Singleton<CrusadeSystem>.Instance.EventStartTime;
		ui.nEndTime = Singleton<CrusadeSystem>.Instance.EventEndTime;
		switch (Singleton<CrusadeSystem>.Instance.LastTargetBoss)
		{
		case CrusadeBossTarget.MainBoss:
		{
			NetCrusadePlayerInfo playerInfoCache = Singleton<CrusadeSystem>.Instance.PlayerInfoCache;
			ui.listUsedPlayerID.AddRange(playerInfoCache.UsedCharacterIDList.Select((string id) => int.Parse(id)));
			ui.listUsedWeaponID.AddRange(playerInfoCache.UsedWeaponIDList.Select((string id) => int.Parse(id)));
			ui.Setup(_currentStageTable, StageMode.Contribute);
			break;
		}
		case CrusadeBossTarget.Guardian:
		{
			NetCrusadeGuardInfo netCrusadeGuardInfo = Singleton<CrusadeSystem>.Instance.GuardInfoCacheList[Singleton<CrusadeSystem>.Instance.LastTargetGuardianIndex];
			ui.Setup(netCrusadeGuardInfo.StageID);
			break;
		}
		}
		_isNetLocking = false;
	}

	private void LoadBossRewardUI()
	{
		_isNetLocking = true;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildBossRewardUI>("UI_GuildBossReward", OnBossRewardUILoaded);
	}

	private void OnBossRewardUILoaded(GuildBossRewardUI ui)
	{
		int eventID = Singleton<CrusadeSystem>.Instance.EventID;
		NetCrusadePlayerInfo playerInfoCache = Singleton<CrusadeSystem>.Instance.PlayerInfoCache;
		NetCrusadeBossInfo bossInfoCache = Singleton<CrusadeSystem>.Instance.BossInfoCache;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(eventID, playerInfoCache.Score, bossInfoCache.BossStep, _currentStageTable, _rankNum);
		_isNetLocking = false;
	}

	private void LoadBattleRecordUI()
	{
		_isNetLocking = true;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CrusadeRecordUI>("UI_CrusadeRecord", OnBattleRecordUILoaded);
	}

	private void OnBattleRecordUILoaded(CrusadeRecordUI ui)
	{
		List<NetCrusadeBattleRecord> battleRecordListCache = Singleton<CrusadeSystem>.Instance.BattleRecordListCache;
		NetCrusadePlayerInfo playerInfoCache = Singleton<CrusadeSystem>.Instance.PlayerInfoCache;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(battleRecordListCache, playerInfoCache.Score);
		_isNetLocking = false;
	}

	private void LoadCheckChargeStaminaUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupYesNoByKey("COMMON_TIP", "PLAY_COUNT_OUT_CHARGE", "COMMON_OK", "COMMON_CANCEL", LoadChargeStaminaUI);
		});
	}

	private void LoadChargeStaminaUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<ChargeStaminaUI>("UI_ChargeStamina", OnChargeStaminaUILoaded);
	}

	private void OnChargeStaminaUILoaded(ChargeStaminaUI ui)
	{
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(ChargeType.CrusadeChallenge);
		ui.closeCB = delegate
		{
			OnCrusadePlayerInfoUpdate();
		};
	}

	private void LoadRuleBonusDialogUI()
	{
		_isNetLocking = true;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<RuleBonusDialog>("UI_RuleBonus", OnRuleBonusDialogUILoaded);
	}

	private void OnRuleBonusDialogUILoaded(RuleBonusDialog ui)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_currentStageTable);
		_isNetLocking = false;
	}

	private void OnSelectGuardianEvent(int guardianIndex)
	{
		if (!_guardianUnitList[guardianIndex].isGoSelect)
		{
			PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		_guardianUnitList.ForEach(delegate(CrusadeGuardianUnit unit)
		{
			unit.TargetSelected = false;
		});
		Singleton<CrusadeSystem>.Instance.LastTargetBoss = CrusadeBossTarget.Guardian;
		Singleton<CrusadeSystem>.Instance.LastTargetGuardianIndex = guardianIndex;
		_guardianUnitList[guardianIndex].TargetSelected = true;
		_goMainBossSelected.SetActive(false);
	}

	private void OnSelectGuardianBuffEvent(int guardianIndex)
	{
		if (!_guardianUnitList[guardianIndex].isGoSelect && !_guardianUnitList[guardianIndex].BuffSelected)
		{
			PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		}
		_guardianUnitList.ForEach(delegate(CrusadeGuardianUnit unit)
		{
			unit.BuffSelected = false;
		});
		Singleton<CrusadeSystem>.Instance.LastSelectedGuardianBuffIndex = guardianIndex;
		_guardianUnitList[guardianIndex].BuffSelected = true;
		Singleton<CrusadeSystem>.Instance.PassiveSkillID = _guardianUnitList[guardianIndex].BustedSkill;
	}

	public void OnSelectMainBossEvent()
	{
		_guardianUnitList.ForEach(delegate(CrusadeGuardianUnit unit)
		{
			unit.TargetSelected = false;
		});
		Singleton<CrusadeSystem>.Instance.LastTargetBoss = CrusadeBossTarget.MainBoss;
		if (!_goMainBossSelected.activeSelf)
		{
			PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		_goMainBossSelected.SetActive(true);
	}

	private void PlaySE(SystemSE eCue)
	{
		if (bMuteSE)
		{
			bMuteSE = false;
		}
		else
		{
			PlayUISE(eCue);
		}
	}
}
