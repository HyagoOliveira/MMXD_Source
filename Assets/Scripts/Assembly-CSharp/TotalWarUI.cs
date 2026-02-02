using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;

public class TotalWarUI : OrangeUIBase
{
	private class OneTypeRecord
	{
		public GameObject RTypeOn;

		public GameObject RTypeOff;

		public Text RTypeScore;

		public GameObject RTypeChar;

		public GameObject RTypeWeapon;
	}

	[Header("TotalWarUI")]
	public GameObject refCommonIconBase;

	public GameObject[] BTypeOn;

	public GameObject[] BTypeOff;

	public Button[] BTypeBtn;

	public GameObject[] RType0Char;

	public GameObject[] RType1Char;

	public GameObject[] RType2Char;

	public GameObject[] RType0Weapon;

	public GameObject[] RType1Weapon;

	public GameObject[] RType2Weapon;

	public Text[] RType0Score;

	public Text[] RType1Score;

	public Text[] RType2Score;

	public GameObject[] RType0On;

	public GameObject[] RType1On;

	public GameObject[] RType2On;

	public GameObject[] RType0Off;

	public GameObject[] RType1Off;

	public GameObject[] RType2Off;

	public Text EventTimeText;

	public Text RankText;

	public Text ScoreText;

	public Transform RankPanelPos;

	public RankPanelMini m_rankPanelMiniRef;

	private RankPanelMini m_rankPanelMini;

	public GameObject ReplaceRootOne;

	public GameObject ReplaceRootTwo;

	private Coroutine tObjScaleCoroutine;

	private string[] BGMs = new string[2] { "BGM02", "bgm_sys_totalwar" };

	private bool bLockNet;

	private int nEventID;

	private int nTotalScore;

	private int nRank;

	private STAGE_TABLE[] allTotalWarStages;

	[HideInInspector]
	private STAGE_TABLE m_currentStageTable;

	private List<int> listUsedPlayerID = new List<int>();

	private List<int> listUsedWeaponID = new List<int>();

	private List<NetTWStageRecord> listNetTWBattleRecord = new List<NetTWStageRecord>();

	private NetTWBattleRecord TmpRecord;

	private int nStartTime;

	private int nEndTime;

	private OneTypeRecord[][] AllRTypeRecord = new OneTypeRecord[3][];

	private float fNowValue;

	private int nNowType
	{
		get
		{
			return MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nCurrentTotalWarType;
		}
		set
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nCurrentTotalWarType = value;
		}
	}

	public void Setup()
	{
		SwitchBtnType(nNowType);
		ReplaceRootOne.SetActive(false);
		ReplaceRootTwo.SetActive(false);
		AllRTypeRecord[0] = new OneTypeRecord[3];
		AllRTypeRecord[1] = new OneTypeRecord[3];
		AllRTypeRecord[2] = new OneTypeRecord[3];
		for (int i = 0; i < RType0On.Length; i++)
		{
			AllRTypeRecord[0][i] = new OneTypeRecord();
			AllRTypeRecord[0][i].RTypeOn = RType0On[i];
			AllRTypeRecord[0][i].RTypeOff = RType0Off[i];
			AllRTypeRecord[0][i].RTypeScore = RType0Score[i];
			AllRTypeRecord[0][i].RTypeWeapon = RType0Weapon[i];
			AllRTypeRecord[0][i].RTypeChar = RType0Char[i];
			AllRTypeRecord[1][i] = new OneTypeRecord();
			AllRTypeRecord[1][i].RTypeOn = RType1On[i];
			AllRTypeRecord[1][i].RTypeOff = RType1Off[i];
			AllRTypeRecord[1][i].RTypeScore = RType1Score[i];
			AllRTypeRecord[1][i].RTypeWeapon = RType1Weapon[i];
			AllRTypeRecord[1][i].RTypeChar = RType1Char[i];
			AllRTypeRecord[2][i] = new OneTypeRecord();
			AllRTypeRecord[2][i].RTypeOn = RType2On[i];
			AllRTypeRecord[2][i].RTypeOff = RType2Off[i];
			AllRTypeRecord[2][i].RTypeScore = RType2Score[i];
			AllRTypeRecord[2][i].RTypeWeapon = RType2Weapon[i];
			AllRTypeRecord[2][i].RTypeChar = RType2Char[i];
		}
		ResetAllRTypeRecord();
		long now = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		EventTimeText.text = "";
		List<EVENT_TABLE> list = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 17 && x.n_HOMETOP == 1 && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(x.s_BEGIN_TIME, (x.s_REMAIN_TIME == "null") ? x.s_END_TIME : x.s_REMAIN_TIME, now)).ToList();
		if (list.Count > 0)
		{
			CheckPlayBGM(list[0].n_ID);
		}
		if (m_rankPanelMiniRef != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_rankPanelMiniRef.gameObject, RankPanelPos);
			m_rankPanelMini = gameObject.GetComponent<RankPanelMini>();
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveTotalWarInfoReq(ManagedSingleton<PlayerHelper>.Instance.RetrieveTotalWarInfoCB);
		m_currentStageTable = new STAGE_TABLE();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public override void OnClickCloseBtn()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
		base.OnClickCloseBtn();
	}

	private void ResetAllRTypeRecord()
	{
		for (int i = 0; i < RType0On.Length; i++)
		{
			RType0On[i].SetActive(false);
			RType0Off[i].SetActive(true);
			RType1On[i].SetActive(false);
			RType1Off[i].SetActive(true);
			RType2On[i].SetActive(false);
			RType2Off[i].SetActive(true);
			for (int j = 0; j < 3; j++)
			{
				for (int num = AllRTypeRecord[i][j].RTypeChar.transform.childCount - 1; num >= 0; num--)
				{
					UnityEngine.Object.Destroy(AllRTypeRecord[i][j].RTypeChar.transform.GetChild(num).gameObject);
				}
			}
			for (int k = 0; k < 3; k++)
			{
				for (int num2 = AllRTypeRecord[i][k].RTypeWeapon.transform.childCount - 1; num2 >= 0; num2--)
				{
					UnityEngine.Object.Destroy(AllRTypeRecord[i][k].RTypeWeapon.transform.GetChild(num2).gameObject);
				}
			}
		}
	}

	private void ShowAllRTypeRecord()
	{
		ResetAllRTypeRecord();
		for (int i = 0; i < listNetTWBattleRecord.Count; i++)
		{
			listNetTWBattleRecord[i].TWBattleRecords.Sort(delegate(NetTWBattleRecord a, NetTWBattleRecord b)
			{
				if (a.Score < b.Score)
				{
					return 1;
				}
				if (a.Score > b.Score)
				{
					return -1;
				}
				if (a.BattleTime < b.BattleTime)
				{
					return 1;
				}
				return (a.BattleTime > b.BattleTime) ? (-1) : 0;
			});
		}
		int[] array = new int[3];
		for (int j = 0; j < listNetTWBattleRecord.Count; j++)
		{
			int num = listNetTWBattleRecord[j].StageType - 11;
			if (listNetTWBattleRecord[j].StageType < 3)
			{
				num = listNetTWBattleRecord[j].StageType;
			}
			switch (num)
			{
			case 0:
				ManagedSingleton<PlayerNetManager>.Instance.nTWSuppressScore = listNetTWBattleRecord[j].StageScore;
				break;
			case 1:
				ManagedSingleton<PlayerNetManager>.Instance.nTWLightningScore = listNetTWBattleRecord[j].StageScore;
				break;
			case 2:
				ManagedSingleton<PlayerNetManager>.Instance.nTWCrusadeScore = listNetTWBattleRecord[j].StageScore;
				break;
			}
			if (array[num] >= 3)
			{
				continue;
			}
			for (int k = 0; k < listNetTWBattleRecord[j].TWBattleRecords.Count; k++)
			{
				if (array[num] < 3)
				{
					SetRTypeScore(AllRTypeRecord[num], array[num], listNetTWBattleRecord[j].TWBattleRecords[k].UseCharacter, listNetTWBattleRecord[j].TWBattleRecords[k].UseMainWeapon, listNetTWBattleRecord[j].TWBattleRecords[k].Score);
					array[num]++;
				}
			}
		}
	}

	public void UpdateRetrieveTotalWarInfoRes(RetrieveTotalWarInfoRes res)
	{
		EVENT_TABLE tEVENT_TABLE;
		if (!ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(res.EventID, out tEVENT_TABLE))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
			return;
		}
		nEventID = res.EventID;
		CheckPlayBGM(res.EventID);
		IEnumerable<STAGE_TABLE> enumerable = null;
		enumerable = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.Where((STAGE_TABLE stagedata) => (stagedata.n_MAIN == tEVENT_TABLE.n_TYPE_Y) ? true : false);
		if (enumerable.Count() <= 0)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
			return;
		}
		allTotalWarStages = enumerable.ToArray();
		if (allTotalWarStages == null)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
			return;
		}
		for (int i = 0; i < allTotalWarStages.Length; i++)
		{
			if (allTotalWarStages[i].n_TYPE == nNowType + 11)
			{
				m_currentStageTable = allTotalWarStages[i];
				break;
			}
		}
		nStartTime = res.StartTime;
		nEndTime = res.EndTime;
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
		TimeSpan timeSpan = TimeSpan.FromSeconds(res.StartTime);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(res.EndTime);
		EventTimeText.text = (dateTime + timeSpan).ToString("yyyy/MM/dd hh:mm tt") + " ~ " + (dateTime + timeSpan2).ToString("yyyy/MM/dd hh:mm tt");
		nTotalScore = res.TotalScore;
		ScoreText.text = res.TotalScore.ToString();
		listNetTWBattleRecord.Clear();
		listNetTWBattleRecord.AddRange(res.BattleRecordList);
		listUsedPlayerID.Clear();
		listUsedWeaponID.Clear();
		foreach (string usedCharacterID in res.UsedCharacterIDList)
		{
			listUsedPlayerID.Add(int.Parse(usedCharacterID));
		}
		foreach (string usedWeaponID in res.UsedWeaponIDList)
		{
			listUsedWeaponID.Add(int.Parse(usedWeaponID));
		}
		ShowAllRTypeRecord();
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null && m_rankPanelMini != null)
		{
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveEventRankingReq(res.EventID, 1, 10, ManagedSingleton<PlayerHelper>.Instance.RetrieveTotalWarRankingCB);
			ManagedSingleton<PlayerNetManager>.Instance.RetrievePersonnelEventRankingReq(res.EventID, ManagedSingleton<PlayerHelper>.Instance.RetrieveSelfTotalWarRankingInfoCB);
		}
		if (res.TmpRecord == null || res.TmpRecord.UseCharacter == null || res.TmpRecord.UseMainWeapon == null)
		{
			return;
		}
		int num = 0;
		NetTWBattleRecord[] array = new NetTWBattleRecord[3];
		TmpRecord = (array[0] = res.TmpRecord);
		for (int j = 0; j < listNetTWBattleRecord.Count; j++)
		{
			for (int k = 0; k < listNetTWBattleRecord[j].TWBattleRecords.Count; k++)
			{
				if (res.TmpRecord.UseCharacter.CharacterID == listNetTWBattleRecord[j].TWBattleRecords[k].UseCharacter.CharacterID || res.TmpRecord.UseMainWeapon.WeaponID == listNetTWBattleRecord[j].TWBattleRecords[k].UseMainWeapon.WeaponID)
				{
					num++;
					if (num < 3)
					{
						array[num] = listNetTWBattleRecord[j].TWBattleRecords[k];
					}
				}
			}
		}
		if (num != 0)
		{
			ShowReplaceRoot(num, array);
		}
	}

	public void UpdateRetrieveTotalWarRankingRes(object list)
	{
		List<EventRankingInfo> eventRankingInfoList = list as List<EventRankingInfo>;
		m_rankPanelMini.Setup(eventRankingInfoList, nEventID);
	}

	public void UpdateSelfEventRankingInfo(EventRankingInfo tEventRankingInfo)
	{
		if (nRank != 0 && nRank != tEventRankingInfo.Ranking && (tEventRankingInfo.Ranking <= 10 || nRank <= 10))
		{
			ManagedSingleton<PlayerNetManager>.Instance.RetrieveEventRankingReq(nEventID, 1, 10, ManagedSingleton<PlayerHelper>.Instance.RetrieveTotalWarRankingCB);
		}
		nRank = tEventRankingInfo.Ranking;
		RankText.text = tEventRankingInfo.Ranking.ToString();
		ScoreText.text = tEventRankingInfo.Score.ToString();
	}

	private void SwitchBtnType(int nIndex)
	{
		for (int i = 0; i < BTypeOn.Length; i++)
		{
			if (i == nIndex)
			{
				BTypeOn[i].SetActive(true);
				BTypeOff[i].SetActive(false);
				BTypeBtn[i].interactable = false;
			}
			else
			{
				BTypeOn[i].SetActive(false);
				BTypeOff[i].SetActive(true);
				BTypeBtn[i].interactable = true;
			}
		}
	}

	public void OnClickBType(int nIndex)
	{
		if (nNowType != nIndex)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
		}
		SwitchBtnType(nIndex);
		nNowType = nIndex;
		if (allTotalWarStages == null)
		{
			return;
		}
		for (int i = 0; i < allTotalWarStages.Length; i++)
		{
			if (allTotalWarStages[i].n_TYPE == nNowType + 11)
			{
				m_currentStageTable = allTotalWarStages[i];
				break;
			}
		}
	}

	private void SetRTypeScore(OneTypeRecord[] tOneTypeRecords, int nIndex, RBCharacterInfo UseCharacter, RBWeaponInfo UseMainWeapon, int nScore)
	{
		tOneTypeRecords[nIndex].RTypeOn.SetActive(true);
		tOneTypeRecords[nIndex].RTypeOff.SetActive(false);
		tOneTypeRecords[nIndex].RTypeScore.text = nScore.ToString();
		CommonIconBase component = UnityEngine.Object.Instantiate(refCommonIconBase, tOneTypeRecords[nIndex].RTypeWeapon.transform).GetComponent<CommonIconBase>();
		CommonIconBase component2 = UnityEngine.Object.Instantiate(refCommonIconBase, tOneTypeRecords[nIndex].RTypeChar.transform).GetComponent<CommonIconBase>();
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[UseCharacter.CharacterID];
		component2.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON);
		component2.SetOtherInfoRB(UseCharacter, false);
		component2.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
		component.Setup(p_assetName: ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[UseMainWeapon.WeaponID].s_ICON, p_idx: 0, p_bundleName: AssetBundleScriptableObject.Instance.m_iconWeapon);
		component.SetOtherInfoRB(UseMainWeapon, CommonIconBase.WeaponEquipType.Main);
		component.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
	}

	private void CheckPlayBGM(int eventID)
	{
		EVENT_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.TryGetValue(eventID, out value) && !string.IsNullOrEmpty(value.s_BGM))
		{
			string[] array = value.s_BGM.Split(',');
			if (array.Length == 2)
			{
				BGMs = array;
			}
		}
		MonoBehaviourSingleton<AudioManager>.Instance.NotifyPlayBGM(1, BGMs[0], BGMs[1]);
	}

	public void OnClickRules()
	{
		if (m_currentStageTable != null && !bLockNet)
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
		if (m_currentStageTable != null && !bLockNet)
		{
			bLockNet = true;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TotalWarRecord", delegate(TotalWarRecordUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(listNetTWBattleRecord, nTotalScore);
				bLockNet = false;
			});
		}
	}

	public void OnClickReward()
	{
		if (m_currentStageTable == null || bLockNet)
		{
			return;
		}
		bLockNet = true;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TotalWarReward", delegate(TotalWarRewardUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			int[] array = new int[3];
			foreach (NetTWStageRecord item in listNetTWBattleRecord)
			{
				if (item.StageType >= 11)
				{
					array[item.StageType - 11] = item.StageScore;
				}
				else if (item.StageType >= 0 && item.StageType < 3)
				{
					array[item.StageType] = item.StageScore;
				}
			}
			bool inbCanGet = false;
			if (nStartTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC || MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC > nEndTime)
			{
				inbCanGet = true;
			}
			ui.Setup(nEventID, array, m_currentStageTable, nRank, inbCanGet);
			bLockNet = false;
		});
	}

	public void OnGoToGoCheck()
	{
		if (m_currentStageTable == null)
		{
			return;
		}
		if (nStartTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC || MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC > nEndTime)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupConfirmByKey("COMMON_TIP", "EVENT_OUTDATE", "COMMON_OK", delegate
				{
				});
			}, true);
		}
		else if (!bLockNet)
		{
			bLockNet = true;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
			{
				ui.listUsedPlayerID.AddRange(listUsedPlayerID);
				ui.listUsedWeaponID.AddRange(listUsedWeaponID);
				ui.nStartTime = nStartTime;
				ui.nEndTime = nEndTime;
				ui.Setup(m_currentStageTable);
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.TOTALWAR;
				bLockNet = false;
			});
		}
	}

	public void ShowReplaceRoot(int nReplaceCount, NetTWBattleRecord[] inNetTWBattleRecord)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		switch (nReplaceCount)
		{
		case 1:
		{
			ReplaceRootOne.transform.localScale = new Vector3(0f, 0f, 1f);
			ObjScale(0f, 1f, 0.2f, ReplaceRootOne, null);
			ReplaceRootOne.SetActive(true);
			GameObject tObj4 = ReplaceRootOne.transform.Find("BattleRecordGroup0").gameObject;
			SetReplaceRecord(inNetTWBattleRecord[0], tObj4, true);
			GameObject tObj5 = ReplaceRootOne.transform.Find("BattleRecordGroup1").gameObject;
			SetReplaceRecord(inNetTWBattleRecord[1], tObj5);
			break;
		}
		case 2:
		{
			ReplaceRootTwo.transform.localScale = new Vector3(0f, 0f, 1f);
			ObjScale(0f, 1f, 0.2f, ReplaceRootTwo, null);
			ReplaceRootTwo.SetActive(true);
			GameObject tObj = ReplaceRootTwo.transform.Find("BattleRecordGroup0").gameObject;
			SetReplaceRecord(inNetTWBattleRecord[0], tObj, true);
			GameObject tObj2 = ReplaceRootTwo.transform.Find("BattleRecordGroup1").gameObject;
			SetReplaceRecord(inNetTWBattleRecord[1], tObj2);
			GameObject tObj3 = ReplaceRootTwo.transform.Find("BattleRecordGroup2").gameObject;
			SetReplaceRecord(inNetTWBattleRecord[2], tObj3);
			break;
		}
		}
	}

	public void OnConfirmReplace()
	{
		if (m_currentStageTable != null && !bLockNet)
		{
			bLockNet = true;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			ManagedSingleton<PlayerNetManager>.Instance.TotalWarRecordReplaceReq(true, ManagedSingleton<PlayerHelper>.Instance.TotalWarRecordReplaceResCB);
		}
	}

	public void TotalWarRecordReplaceResCB(TotalWarRecordReplaceRes res)
	{
		bLockNet = false;
		if (ReplaceRootOne.activeSelf)
		{
			ObjScale(1f, 0f, 0.2f, ReplaceRootOne, delegate
			{
				ReplaceRootOne.SetActive(false);
			});
		}
		if (ReplaceRootTwo.activeSelf)
		{
			ObjScale(1f, 0f, 0.2f, ReplaceRootTwo, delegate
			{
				ReplaceRootTwo.SetActive(false);
			});
		}
		if (TmpRecord != null)
		{
			for (int i = 0; i < listNetTWBattleRecord.Count; i++)
			{
				for (int num = listNetTWBattleRecord[i].TWBattleRecords.Count - 1; num >= 0; num--)
				{
					if (TmpRecord.UseCharacter.CharacterID == listNetTWBattleRecord[i].TWBattleRecords[num].UseCharacter.CharacterID || TmpRecord.UseMainWeapon.WeaponID == listNetTWBattleRecord[i].TWBattleRecords[num].UseMainWeapon.WeaponID)
					{
						listNetTWBattleRecord[i].TWBattleRecords.RemoveAt(num);
					}
				}
				if (listNetTWBattleRecord[i].StageType == TmpRecord.StageType)
				{
					listNetTWBattleRecord[i].TWBattleRecords.Add(TmpRecord);
				}
			}
			listUsedPlayerID.Clear();
			listUsedWeaponID.Clear();
			foreach (string usedCharacterID in res.UsedCharacterIDList)
			{
				listUsedPlayerID.Add(int.Parse(usedCharacterID));
			}
			foreach (string usedWeaponID in res.UsedWeaponIDList)
			{
				listUsedWeaponID.Add(int.Parse(usedWeaponID));
			}
			TmpRecord = null;
		}
		for (int j = 0; j < res.StageScoreList.Count; j++)
		{
			for (int k = 0; k < listNetTWBattleRecord.Count; k++)
			{
				if (listNetTWBattleRecord[k].StageType == res.StageScoreList[j].StageType)
				{
					listNetTWBattleRecord[k].StageScore = res.StageScoreList[j].StageScore;
				}
			}
		}
		ShowAllRTypeRecord();
		ManagedSingleton<PlayerNetManager>.Instance.RetrievePersonnelEventRankingReq(nEventID, ManagedSingleton<PlayerHelper>.Instance.RetrieveSelfTotalWarRankingInfoCB);
	}

	public void OnCloseReplaceOne()
	{
		TmpRecord = null;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ManagedSingleton<PlayerNetManager>.Instance.TotalWarRecordReplaceReq(false, ManagedSingleton<PlayerHelper>.Instance.TotalWarRecordReplaceResCB);
	}

	public void OnCloseReplaceTwo()
	{
		TmpRecord = null;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ManagedSingleton<PlayerNetManager>.Instance.TotalWarRecordReplaceReq(false, ManagedSingleton<PlayerHelper>.Instance.TotalWarRecordReplaceResCB);
	}

	private void SetReplaceRecord(NetTWBattleRecord tNetTWBattleRecord, GameObject tObj, bool bCheckNewRecord = false)
	{
		Text component = tObj.transform.Find("RecordTimeText").GetComponent<Text>();
		Text component2 = tObj.transform.Find("RecordScoreText").GetComponent<Text>();
		GameObject gameObject = tObj.transform.Find("playericonroot").gameObject;
		GameObject gameObject2 = tObj.transform.Find("mainweaponroot").gameObject;
		GameObject[] array = new GameObject[3];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = tObj.transform.Find("RType" + i).gameObject;
			array[i].SetActive(false);
		}
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
		TimeSpan timeSpan = TimeSpan.FromSeconds(tNetTWBattleRecord.BattleTime);
		component.text = (dateTime + timeSpan).ToString("yyyy/MM/dd hh:mm tt");
		component2.text = tNetTWBattleRecord.Score.ToString();
		CommonIconBase component3 = UnityEngine.Object.Instantiate(refCommonIconBase, gameObject2.transform).GetComponent<CommonIconBase>();
		CommonIconBase component4 = UnityEngine.Object.Instantiate(refCommonIconBase, gameObject.transform).GetComponent<CommonIconBase>();
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[tNetTWBattleRecord.UseCharacter.CharacterID];
		component4.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON);
		component4.SetOtherInfoRB(tNetTWBattleRecord.UseCharacter, false);
		component4.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
		component3.Setup(p_assetName: ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[tNetTWBattleRecord.UseMainWeapon.WeaponID].s_ICON, p_idx: 0, p_bundleName: AssetBundleScriptableObject.Instance.m_iconWeapon);
		component3.SetOtherInfoRB(tNetTWBattleRecord.UseMainWeapon, CommonIconBase.WeaponEquipType.Main);
		component3.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
		array[tNetTWBattleRecord.StageType - 11].SetActive(true);
		if (bCheckNewRecord)
		{
			Transform obj = tObj.transform.Find("NewRecord");
			int num = tNetTWBattleRecord.StageType - 11;
			if (num < 0)
			{
				num = tNetTWBattleRecord.StageType;
			}
			bool active = false;
			if (listNetTWBattleRecord.Count() > 0 && listNetTWBattleRecord.Count() > num && listNetTWBattleRecord[num].TWBattleRecords.Count() > 0 && tNetTWBattleRecord.Score > listNetTWBattleRecord[num].TWBattleRecords[0].Score)
			{
				active = true;
			}
			obj.gameObject.SetActive(active);
		}
	}

	private void ObjScale(float fStart, float fEnd, float fTime, GameObject tObj, Action endcb)
	{
		if (tObjScaleCoroutine != null)
		{
			StopCoroutine(tObjScaleCoroutine);
		}
		if (fStart > fEnd)
		{
			if (fStart >= fNowValue && fNowValue >= fEnd)
			{
				fStart = fNowValue;
			}
		}
		else if (fStart <= fNowValue && fNowValue <= fEnd)
		{
			fStart = fNowValue;
		}
		tObjScaleCoroutine = StartCoroutine(ObjScaleCoroutine(fStart, fEnd, fTime, tObj, endcb));
	}

	private IEnumerator ObjScaleCoroutine(float fStart, float fEnd, float fTime, GameObject tObj, Action endcb)
	{
		fNowValue = fStart;
		float fLeftTime = fTime;
		float fD = (fEnd - fStart) / fTime;
		Vector3 nowScale = new Vector3(fNowValue, fNowValue, 1f);
		tObj.transform.localScale = nowScale;
		Transform tBGClick = tObj.transform.Find("BGClick");
		if (tBGClick != null)
		{
			tBGClick.gameObject.SetActive(false);
		}
		while (fLeftTime > 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			float deltaTime = Time.deltaTime;
			fLeftTime -= deltaTime;
			fNowValue += fD * deltaTime;
			nowScale.x = fNowValue;
			nowScale.y = fNowValue;
			tObj.transform.localScale = nowScale;
		}
		nowScale.x = fEnd;
		nowScale.y = fEnd;
		tObj.transform.localScale = nowScale;
		if (tBGClick != null)
		{
			tBGClick.gameObject.SetActive(true);
		}
		if (endcb != null)
		{
			endcb();
		}
	}
}
