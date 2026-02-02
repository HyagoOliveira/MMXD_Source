using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class WorldBossEventUI : OrangeUIBase
{
	private class TimeInfoData
	{
		public string TimeInfoText;

		public string AttackTime;

		public string sPlayerID;

		public int nPortraitID;
	}

	public STAGE_TABLE m_currentStageTable;

	private List<KeyValuePair<int, STAGE_TABLE>> listStages = new List<KeyValuePair<int, STAGE_TABLE>>();

	public Text EventTimeText;

	public GameObject EventInfoRoot;

	public Text TextChallengeNum;

	public Text TextContributionNum;

	public Text TextScoreNum;

	public Text TextBonusNum;

	public GameObject BtnAddChallengeNum;

	public Transform RankPanelPos;

	public RankPanelMini m_rankPanelMiniRef;

	private RankPanelMini m_rankPanelMini;

	public GameObject BossHpBarRoot;

	public Image BossHpBar;

	public Text BossHpBarText;

	public Text BossThText;

	public Text ThText;

	public GameObject BottomPanel;

	public Button BtnDeploy;

	public GameObject BattleSituationRoot;

	public GameObject[] TimeInfo;

	public Text[] TimeInfoText;

	public Text[] AttackTime;

	private RetrieveRaidBossInfoRes tGetResCache;

	private Coroutine refTimeCoroutine;

	private long nBossMaxHP;

	private long nBossSetpHP;

	private int nBossSetNum = 1;

	private int nRankNum;

	private bool bLockNet;

	[BoxGroup("Sound")]
	[Tooltip("戰鬥準備")]
	[SerializeField]
	private SystemSE m_goCheckBtn = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	[BoxGroup("Sound")]
	[Tooltip("活動BGM")]
	[SerializeField]
	private List<EventStageMain.BgmInfo> BGM4Event = new List<EventStageMain.BgmInfo>();

	private List<TimeInfoData> listTimeInfoDatas = new List<TimeInfoData>();

	public void Setup()
	{
		long now = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<EVENT_TABLE> list = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 13 && x.n_HOMETOP == 1 && x.n_TYPE_X == 9 && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, (x.s_REMAIN_TIME == "null") ? x.s_END_TIME : x.s_REMAIN_TIME, now)).ToList();
		if (list.Count > 0)
		{
			CheckPlayBGM(list[0].n_ID);
		}
		EventInfoRoot.SetActive(false);
		BossHpBarRoot.SetActive(false);
		BottomPanel.SetActive(false);
		EventTimeText.text = "";
		m_currentStageTable = null;
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveRaidBossInfoReq(ManagedSingleton<PlayerHelper>.Instance.RetrieveRaidBossInfoCB);
		if (m_rankPanelMiniRef != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_rankPanelMiniRef.gameObject, RankPanelPos);
			m_rankPanelMini = gameObject.GetComponent<RankPanelMini>();
		}
		TextBonusNum.text = ((float)ManagedSingleton<PlayerHelper>.Instance.GetRaidBossBounes() * 0.01f).ToString("0.0") + "%";
		BtnDeploy = BottomPanel.transform.Find("BtnDeploy").GetComponent<Button>();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void CheckPlayBGM(int eventID)
	{
		foreach (EventStageMain.BgmInfo item in BGM4Event)
		{
			if (eventID == item.iEventID)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.NotifyPlayBGM(1, item.acbName, item.cueName);
				break;
			}
		}
	}

	public void UpdateRetrieveRaidBossInfoRes(RetrieveRaidBossInfoRes res)
	{
		EVENT_TABLE tEVENT_TABLE;
		if (!ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(res.EventID, out tEVENT_TABLE))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
			return;
		}
		IEnumerable<KeyValuePair<int, STAGE_TABLE>> enumerable = null;
		enumerable = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Where((KeyValuePair<int, STAGE_TABLE> stagedata) => (stagedata.Value.n_MAIN == tEVENT_TABLE.n_TYPE_Y) ? true : false);
		if (enumerable.Count() <= 0)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
			return;
		}
		listStages.AddRange(enumerable.ToArray());
		listStages.Sort((KeyValuePair<int, STAGE_TABLE> x, KeyValuePair<int, STAGE_TABLE> y) => x.Key.CompareTo(y.Key));
		if (res.RBBossInfo.BossStep < 5)
		{
			m_currentStageTable = listStages[res.RBBossInfo.BossStep - 1].Value;
		}
		else
		{
			m_currentStageTable = listStages[4].Value;
		}
		(Background as OrangeBgExt).ChangeBackground(m_currentStageTable.s_BG);
		tGetResCache = res;
		EventInfoRoot.SetActive(true);
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
		TimeSpan timeSpan = TimeSpan.FromSeconds(res.StartTime);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(res.EndTime);
		EventTimeText.text = (dateTime + timeSpan).ToString("yyyy/MM/dd hh:mm tt") + " ~ " + (dateTime + timeSpan2).ToString("yyyy/MM/dd hh:mm tt");
		TextChallengeNum.text = res.RBPlayerInfo.BattleCount + "/5";
		BtnDeploy.interactable = res.RBPlayerInfo.BattleCount > 0;
		TextContributionNum.text = res.RBPlayerInfo.Score.ToString();
		TextScoreNum.text = "0";
		ManagedSingleton<PlayerNetManager>.Instance.nRaidBossSocre = res.RBPlayerInfo.Score;
		BossHpBarRoot.SetActive(true);
		int result = 0;
		int.TryParse(m_currentStageTable.w_BOSS_INTRO, out result);
		nBossMaxHP = 0L;
		MOB_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT.TryGetValue(result, out value))
		{
			nBossSetpHP = value.n_HP;
			nBossSetNum = value.n_HP_STEP;
			nBossMaxHP = (long)value.n_HP * (long)value.n_HP_STEP;
		}
		UpdateBossInfo();
		BottomPanel.SetActive(true);
		for (int i = 0; i < tGetResCache.RBOnTimeRecordList.Count; i++)
		{
			TimeInfoData timeInfoData = new TimeInfoData();
			timeInfoData.TimeInfoText = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_DAMAGE_INFO"), tGetResCache.RBOnTimeRecordList[i].NickName, tGetResCache.RBOnTimeRecordList[i].Score);
			timeSpan = TimeSpan.FromSeconds(tGetResCache.RBOnTimeRecordList[i].BattleTime);
			timeInfoData.AttackTime = (dateTime + timeSpan).ToString("HH:mm");
			timeInfoData.sPlayerID = tGetResCache.RBOnTimeRecordList[i].PlayerID;
			timeInfoData.nPortraitID = tGetResCache.RBOnTimeRecordList[i].PortraitID;
			listTimeInfoDatas.Add(timeInfoData);
		}
		if (refTimeCoroutine == null)
		{
			refTimeCoroutine = StartCoroutine(TimeCoroutine());
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null && m_rankPanelMini != null)
		{
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveEventRankingReq(res.EventID, 1, 10, ManagedSingleton<PlayerHelper>.Instance.RetrieveRaidBossEventRankingCB);
			ManagedSingleton<PlayerNetManager>.Instance.RetrievePersonnelEventRankingReq(res.EventID, ManagedSingleton<PlayerHelper>.Instance.RetrieveSelfRaidBossEventRankingInfoCB);
		}
		if (tGetResCache != null && tGetResCache.RewardEntities != null && tGetResCache.RewardEntities.RewardList != null && tGetResCache.RewardEntities.RewardList.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(tGetResCache.RewardEntities.RewardList);
			});
		}
	}

	public void UpdateBossInfo()
	{
		if (nBossMaxHP > tGetResCache.RBBossInfo.TotalHP)
		{
			BossHpBar.fillAmount = (float)tGetResCache.RBBossInfo.TotalHP / (float)nBossMaxHP;
		}
		else
		{
			BossHpBar.fillAmount = 1f;
		}
		BossHpBarText.text = tGetResCache.RBBossInfo.TotalHP + "/" + nBossSetpHP * nBossSetNum;
		BossThText.text = tGetResCache.RBBossInfo.BossStep.ToString();
		if (tGetResCache.RBBossInfo.BossStep < 4)
		{
			ThText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_STEP_" + tGetResCache.RBBossInfo.BossStep);
		}
		else if (tGetResCache.RBBossInfo.BossStep > 20 && tGetResCache.RBBossInfo.BossStep % 10 < 4 && tGetResCache.RBBossInfo.BossStep % 10 >= 1)
		{
			ThText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_STEP_" + tGetResCache.RBBossInfo.BossStep % 10);
		}
		else
		{
			ThText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_STEP_4");
		}
	}

	public void UpdateRetrieveRaidBossEventRankingRes(object list)
	{
		List<EventRankingInfo> list2 = list as List<EventRankingInfo>;
		m_rankPanelMini.Setup(list2, tGetResCache.EventID);
		foreach (EventRankingInfo item in list2)
		{
			if (item.PlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				TextScoreNum.text = item.Ranking.ToString();
				nRankNum = item.Ranking;
				break;
			}
		}
	}

	public void UpdateSelfEventRankingInfo(EventRankingInfo tEventRankingInfo)
	{
		TextScoreNum.text = tEventRankingInfo.Ranking.ToString();
		nRankNum = tEventRankingInfo.Ranking;
	}

	public void UpdateRBPlayerInfo(NetRBPlayerInfo tNetRBPlayerInfo)
	{
		if (tGetResCache != null)
		{
			tGetResCache.RBPlayerInfo = tNetRBPlayerInfo;
			TextChallengeNum.text = tGetResCache.RBPlayerInfo.BattleCount + "/5";
			BtnDeploy.interactable = tGetResCache.RBPlayerInfo.BattleCount > 0;
		}
	}

	private IEnumerator TimeCoroutine()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		float frontpos = TimeInfo[0].transform.position.x;
		float backpos = TimeInfo[2].transform.position.x;
		float diffx = 0f;
		BattleSituationRoot.SetActive(true);
		if (listTimeInfoDatas.Count <= 2)
		{
			if (listTimeInfoDatas.Count == 0)
			{
				BattleSituationRoot.SetActive(false);
			}
			if (listTimeInfoDatas.Count >= 1)
			{
				SetTimeAttackData(0, listTimeInfoDatas[0]);
			}
			else
			{
				SetTimeAttackData(0, new TimeInfoData());
			}
			if (listTimeInfoDatas.Count >= 2)
			{
				SetTimeAttackData(1, listTimeInfoDatas[1]);
			}
			else
			{
				SetTimeAttackData(1, new TimeInfoData());
			}
			yield break;
		}
		SetTimeAttackData(0, listTimeInfoDatas[0]);
		SetTimeAttackData(1, listTimeInfoDatas[1]);
		SetTimeAttackData(2, listTimeInfoDatas[2]);
		float fWaitTime = 3f;
		bool bChange = false;
		while (true)
		{
			if (fWaitTime > 0f)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				fWaitTime -= Time.deltaTime;
				continue;
			}
			for (int i = 0; i < TimeInfo.Length; i++)
			{
				TimeInfo[i].transform.position = new Vector3(TimeInfo[i].transform.position.x - 0.5f, TimeInfo[i].transform.position.y, TimeInfo[i].transform.position.z);
				if (i == 1 && TimeInfo[i].transform.position.x <= frontpos)
				{
					bChange = true;
					diffx = TimeInfo[i].transform.position.x - frontpos;
				}
			}
			if (bChange)
			{
				bChange = false;
				fWaitTime = 3f;
				for (int j = 0; j < TimeInfo.Length; j++)
				{
					TimeInfo[j].transform.position = new Vector3(TimeInfo[j].transform.position.x - diffx, TimeInfo[j].transform.position.y, TimeInfo[j].transform.position.z);
				}
				GameObject gameObject = TimeInfo[0];
				TimeInfo[0].transform.position = new Vector3(backpos, TimeInfo[0].transform.position.y, TimeInfo[0].transform.position.z);
				TimeInfo[0] = TimeInfo[1];
				TimeInfo[1] = TimeInfo[2];
				TimeInfo[2] = gameObject;
				Text text = TimeInfoText[0];
				TimeInfoText[0] = TimeInfoText[1];
				TimeInfoText[1] = TimeInfoText[2];
				TimeInfoText[2] = text;
				text = AttackTime[0];
				AttackTime[0] = AttackTime[1];
				AttackTime[1] = AttackTime[2];
				AttackTime[2] = text;
				if (listTimeInfoDatas.Count > 10)
				{
					listTimeInfoDatas.RemoveAt(0);
				}
				else
				{
					TimeInfoData item = listTimeInfoDatas[0];
					listTimeInfoDatas.RemoveAt(0);
					listTimeInfoDatas.Add(item);
				}
				SetTimeAttackData(0, listTimeInfoDatas[0]);
				SetTimeAttackData(1, listTimeInfoDatas[1]);
				SetTimeAttackData(2, listTimeInfoDatas[2]);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private void SetTimeAttackData(int nIndex, TimeInfoData tTID)
	{
		if (tTID.TimeInfoText == null || tTID.TimeInfoText == "")
		{
			TimeInfoText[nIndex].text = "-----";
		}
		else
		{
			TimeInfoText[nIndex].text = tTID.TimeInfoText;
		}
		AttackTime[nIndex].text = tTID.AttackTime;
		Transform ob = AttackTime[nIndex].transform.parent.Find("imgbg");
		if (tTID.TimeInfoText != null && tTID.TimeInfoText != "")
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(ob, tTID.nPortraitID, new Vector3(1f, 1f, 1f), false);
		}
	}

	public void OnClickRules()
	{
		if (m_currentStageTable != null && tGetResCache != null && !bLockNet)
		{
			bLockNet = true;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RuleBonus", delegate(RuleBonusDialog ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(m_currentStageTable);
				bLockNet = false;
			});
		}
	}

	public void OnClickBattleRecord()
	{
		if (m_currentStageTable != null && tGetResCache != null && !bLockNet)
		{
			bLockNet = true;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WorldBossRecord", delegate(WorldBossRecordUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(tGetResCache.RBBattleRecordList, tGetResCache.RBPlayerInfo.Score);
				bLockNet = false;
			});
		}
	}

	public void OnClickReward()
	{
		if (m_currentStageTable != null && tGetResCache != null && !bLockNet)
		{
			bLockNet = true;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WorldBossReward", delegate(WorldBossRewardUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(tGetResCache.EventID, tGetResCache.RBPlayerInfo.Score, tGetResCache.RBBossInfo.BossStep, m_currentStageTable, nRankNum);
				bLockNet = false;
			});
		}
	}

	public void OnGoToGoCheckUI()
	{
		if (bLockNet || tGetResCache.RBPlayerInfo.BattleCount <= 0)
		{
			return;
		}
		if (tGetResCache.StartTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC || MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC > tGetResCache.EndTime)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupConfirmByKey("COMMON_TIP", "EVENT_OUTDATE", "COMMON_OK", delegate
				{
				});
			}, true);
			return;
		}
		bLockNet = true;
		ManagedSingleton<PlayerNetManager>.Instance.CheckRaidBossInfoReq(delegate(CheckRaidBossInfoRes res)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_goCheckBtn);
			if (res.RBBossInfo.BossStep != tGetResCache.RBBossInfo.BossStep)
			{
				tGetResCache.RBBossInfo = res.RBBossInfo;
				UpdateBossInfo();
				if (listStages.Count > 0)
				{
					if (res.RBBossInfo.BossStep < 5)
					{
						m_currentStageTable = listStages[res.RBBossInfo.BossStep - 1].Value;
					}
					else
					{
						m_currentStageTable = listStages[4].Value;
					}
				}
			}
			tGetResCache.RBBossInfo = res.RBBossInfo;
			UpdateBossInfo();
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
			{
				foreach (string usedCharacterID in tGetResCache.RBPlayerInfo.UsedCharacterIDList)
				{
					ui.listUsedPlayerID.Add(int.Parse(usedCharacterID));
				}
				foreach (string usedWeaponID in tGetResCache.RBPlayerInfo.UsedWeaponIDList)
				{
					ui.listUsedWeaponID.Add(int.Parse(usedWeaponID));
				}
				ui.nStartTime = tGetResCache.StartTime;
				ui.nEndTime = tGetResCache.EndTime;
				ui.Setup(m_currentStageTable);
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.WOLRDBOSS;
				bLockNet = false;
			});
		});
	}

	public void OnAddChallengeNum()
	{
		if (tGetResCache == null)
		{
			return;
		}
		if (tGetResCache.StartTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC || MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC > tGetResCache.EndTime)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupConfirmByKey("COMMON_TIP", "EVENT_OUTDATE", "COMMON_OK", delegate
				{
				});
			}, true);
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI ui)
		{
			ui.Setup(ChargeType.RaidBossChallenge);
			ui.closeCB = delegate
			{
				TextChallengeNum.text = tGetResCache.RBPlayerInfo.BattleCount + "/5";
				BtnDeploy.interactable = tGetResCache.RBPlayerInfo.BattleCount > 0;
			};
		});
	}

	public void OnDmgBounsHints()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RuleBonus", delegate(RuleBonusDialog ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(m_currentStageTable);
		});
	}

	public override void OnClickCloseBtn()
	{
		if (!bLockNet)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
			base.OnClickCloseBtn();
		}
	}
}
