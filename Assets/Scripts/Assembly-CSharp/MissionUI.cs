using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class MissionUI : OrangeUIBase
{
	[Header("Side Menu")]
	[SerializeField]
	private Transform storageRoot;

	private List<StorageInfo> listStorage = new List<StorageInfo>();

	[Header("Main Info")]
	[SerializeField]
	private GameObject objTopDailyBanner;

	[SerializeField]
	private GameObject objTopAchievementBanner;

	[SerializeField]
	private GameObject objTopEventBanner;

	[SerializeField]
	private Transform trmBannerRoot;

	[SerializeField]
	private GameObject objBottomPanel;

	[SerializeField]
	private GameObject objBottomSubDaily;

	[SerializeField]
	private GameObject objBottomSubAchievement;

	[SerializeField]
	private GameObject objBottomSubEvent;

	[SerializeField]
	private Text textBottomSubDaily;

	[SerializeField]
	private Text textBottomSubAchievement;

	[SerializeField]
	private Text textBottomSubEvent;

	[SerializeField]
	private Transform trmNaviParent;

	[SerializeField]
	private Transform starConditionRoot;

	private StarConditionComponent starConditionComp;

	[SerializeField]
	private GridLayoutGroup CommonACHTab;

	[SerializeField]
	private MissionSubButton subButtonCloner;

	[SerializeField]
	private Button btnRetrieveAll;

	[Header("ScrollView")]
	[SerializeField]
	private LoopVerticalScrollRect scrollRectMission;

	[SerializeField]
	private MissionCell missionCell;

	[Header("ScrollViewActivity")]
	[SerializeField]
	private Canvas canvasActivityTab;

	[SerializeField]
	private LoopVerticalScrollRect scrollRectActivity;

	[SerializeField]
	private MonthActivityCell monthActivityCell;

	[SerializeField]
	private Text totalActivityText;

	[SerializeField]
	private GameObject MonthlyActivityinfo;

	[SerializeField]
	private Text TimeText;

	[SerializeField]
	private GameObject lockIcon;

	[SerializeField]
	private GameObject payMask;

	[SerializeField]
	private Button BuyPassBtn;

	[SerializeField]
	private Text BuyPassPrice;

	[SerializeField]
	private GameObject BuyObject;

	[SerializeField]
	private GameObject BoughtObject;

	[SerializeField]
	private Button AllRetrieveBtn;

	[SerializeField]
	private Canvas canvasNoResultMsg;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickSubSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickGetAllSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickMonthlyInfo;

	private List<MISSION_TABLE> listMonthActivityPool = new List<MISSION_TABLE>();

	private List<MISSION_TABLE> listMonthActivityPaid = new List<MISSION_TABLE>();

	private int currentPayItemID;

	private MissionSubType currentMissionSubType;

	private bool useGetAllVoice;

	private int openRank;

	private UIOpenChk.OpenStateEnum dailyStatus;

	private int originalAP;

	private int originalEP;

	private const int SUBTAB = 3;

	private Dictionary<MissionSubType, MissionSubButton> dicSubButton = new Dictionary<MissionSubType, MissionSubButton>();

	private OrangeProduct activityProduct;

	private SHOP_TABLE activityShopTable;

	private List<NetRewardInfo> rewardList = new List<NetRewardInfo>();

	private int CurrentRewardIndex;

	private Transform[] transformLawBtns = new Transform[2];

	private MissionType SelectedPage { get; set; }

	private MissionHelper.MissionStatus SelectedStatus { get; set; }

	private MissionSubType SelectedAchievementSubType { get; set; }

	private int SelectedActivitySubType { get; set; }

	public void Setup(MissionType type = MissionType.Daily, MissionHelper.MissionStatus status = MissionHelper.MissionStatus.RUNNING, MissionSubType subType = MissionSubType.Stage)
	{
		SelectedPage = type;
		SelectedStatus = status;
		SelectedAchievementSubType = subType;
		SelectedActivitySubType = 0;
		PrepareMonthlyActivityData();
		dailyStatus = UIOpenChk.GetOpenState(UIOpenChk.ChkUIEnum.OPENRANK_DAILY_MISSION, out openRank);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[1] { AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall" }, delegate
		{
			CreateNewStorageTab(GetStorageIdx(type), 0);
			CreateStarCondition();
			CreateSubTypeButton();
			UpdateMissionList();
			LoadLawBtns();
		}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
		if (!(trmNaviParent.GetComponentInChildren<StandNaviDb>() == null))
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_0"), "ch_navi_0_db", delegate(GameObject obj)
		{
			trmNaviParent.gameObject.SetActive(((SelectedPage == MissionType.Daily || SelectedPage == MissionType.Activity) && dailyStatus == UIOpenChk.OpenStateEnum.OPEN) ? true : false);
			StandNaviDb component = UnityEngine.Object.Instantiate(obj, trmNaviParent, false).GetComponent<StandNaviDb>();
			if ((bool)component)
			{
				component.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL);
			}
			if (trmNaviParent.gameObject.activeSelf)
			{
				bool flag = false;
				if (ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(MissionType.Daily, 0))
				{
					flag = true;
				}
				for (int i = 1; i <= 6; i++)
				{
					if (ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(MissionType.Achievement, i))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 30);
				}
				else if (ManagedSingleton<MissionHelper>.Instance.DisplayDailySuggest)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 47);
				}
				else
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 48);
				}
			}
		});
	}

	private void CreateNewStorageTab(int defaultIdx, int defultSubIdx)
	{
		if (dailyStatus != 0)
		{
			StorageInfo storageInfo = new StorageInfo("MISSION_DAILY", false, 0, OnClickDaily, DisplayNew, DisplayDailySuggest);
			storageInfo.Param = new object[1] { MissionType.Daily };
			listStorage.Add(storageInfo);
		}
		StorageInfo storageInfo2 = new StorageInfo("MISSION_ACHIEVEMENT", false, 3, null, DisplayNew);
		storageInfo2.Param = new object[1] { MissionType.Achievement };
		for (int i = 0; i < 3; i++)
		{
			storageInfo2.Sub[i] = new StorageInfo(GetSubTextByIndex(i), false, 0, OnClickAchievement)
			{
				Param = new object[1] { GetSubStatusByIndex(i) }
			};
		}
		listStorage.Add(storageInfo2);
		List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_MISSION, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
		for (int num = eventTableByType.Count - 1; num >= 0; num--)
		{
			if (!ManagedSingleton<MissionHelper>.Instance.HasNewAccountEventMission(MissionType.Activity, eventTableByType[num].n_TYPE_X))
			{
				eventTableByType.RemoveAt(num);
			}
		}
		if (eventTableByType.Count > 0)
		{
			StorageInfo storageInfo3 = new StorageInfo("MISSION_EVENT", false, eventTableByType.Count, null, DisplayNew);
			storageInfo3.Param = new object[1] { MissionType.Activity };
			listStorage.Add(storageInfo3);
			int num2 = 0;
			foreach (EVENT_TABLE item2 in eventTableByType)
			{
				storageInfo3.Sub[num2] = new StorageInfo(item2.w_NAME, false, 0, OnClickEvent, DisplayEventSubNew)
				{
					Param = new object[1] { item2.n_TYPE_X }
				};
				num2++;
			}
		}
		if (UIOpenChk.GetOpenState(UIOpenChk.ChkUIEnum.OPENRANK_MONTHLY_ACTIVE, out openRank) != 0)
		{
			StorageInfo item = new StorageInfo("MONTHLY_ACTIVE", false, 0, OnClickActivityTab, HasMonthlyActivityRewardToRetrieve);
			listStorage.Add(item);
		}
		StorageGenerator.Load("StorageComp00", listStorage, defaultIdx, defultSubIdx, storageRoot);
	}

	private bool DisplayNew(object[] param)
	{
		if (param == null || param.Length == 0)
		{
			return false;
		}
		MissionType missionType = (MissionType)param[0];
		switch (missionType)
		{
		case MissionType.Daily:
			if (!ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(MissionType.Daily, 0))
			{
				return ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(MissionCondition.ActivityReached);
			}
			return true;
		case MissionType.Achievement:
			return ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(missionType, 0);
		case MissionType.Activity:
		{
			List<EVENT_TABLE> eventTableByType = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_MISSION, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
			for (int num = eventTableByType.Count - 1; num >= 0; num--)
			{
				if (!ManagedSingleton<MissionHelper>.Instance.HasNewAccountEventMission(MissionType.Activity, eventTableByType[num].n_TYPE_X))
				{
					eventTableByType.RemoveAt(num);
				}
			}
			foreach (EVENT_TABLE item in eventTableByType)
			{
				if (ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(MissionType.Activity, item.n_TYPE_X))
				{
					return true;
				}
			}
			return false;
		}
		default:
			return false;
		}
	}

	private bool DisplayDailySuggest(object[] param)
	{
		return ManagedSingleton<MissionHelper>.Instance.DisplayDailySuggest;
	}

	private bool DisplayEventSubNew(object[] param)
	{
		if (param == null || param.Length == 0)
		{
			return false;
		}
		int subType = (int)param[0];
		return ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(MissionType.Activity, subType);
	}

	private int GetStorageIdx(MissionType type)
	{
		int num = 0;
		UIOpenChk.OpenStateEnum openState = UIOpenChk.GetOpenState(UIOpenChk.ChkUIEnum.OPENRANK_DAILY_MISSION, out openRank);
		switch (type)
		{
		case MissionType.Daily:
			return 0;
		case MissionType.Achievement:
			num = 1;
			break;
		case MissionType.Activity:
			num = ((ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_MISSION, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC).Count > 0) ? 2 : 0);
			break;
		default:
			return 0;
		}
		if (openState == UIOpenChk.OpenStateEnum.LOCK)
		{
			num--;
		}
		return num;
	}

	private string GetSubTextByIndex(int idx)
	{
		switch (idx)
		{
		case 0:
			return "MISSION_ACHIEVEMENT_ALL";
		case 1:
			return "MISSION_ACHIEVEMENT_RESEARCHING";
		case 2:
			return "MISSION_ACHIEVEMENT_CLEAR";
		default:
			return string.Empty;
		}
	}

	private MissionHelper.MissionStatus GetSubStatusByIndex(int idx)
	{
		switch (idx)
		{
		case 0:
			return MissionHelper.MissionStatus.ALL;
		case 1:
			return MissionHelper.MissionStatus.RUNNING;
		case 2:
			return MissionHelper.MissionStatus.DONE;
		default:
			return MissionHelper.MissionStatus.ALL;
		}
	}

	private void CreateStarCondition()
	{
		if (!(starConditionComp == null))
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/StarConditionComp", "StarConditionComp", delegate(GameObject obj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj, starConditionRoot);
			starConditionComp = gameObject.GetComponent<StarConditionComponent>();
			HorizontalLayoutGroup componentInChildren = gameObject.GetComponentInChildren<HorizontalLayoutGroup>();
			if ((bool)componentInChildren)
			{
				componentInChildren.spacing = 180f;
			}
			starConditionComp.Setup(113, 0, 0, UpdateMissionList);
		});
	}

	private void SetStarCondition()
	{
		if (starConditionComp != null)
		{
			starConditionComp.Setup(113, 0, 0, UpdateMissionList);
		}
	}

	public void CreateSubTypeButton()
	{
		dicSubButton.Clear();
		new List<string>();
		subButtonCloner.gameObject.SetActive(true);
		List<string> list = new List<string> { "", "ACHIEVEMENT_GROUP_STAGE", "ACHIEVEMENT_GROUP_LEVELING", "ACHIEVEMENT_GROUP_PVP", "ACHIEVEMENT_GROUP_COLLECTION", "ACHIEVEMENT_GROUP_OTHERS", "SIGN_NAME" };
		CommonACHTab.spacing = new Vector2(-60f, 0f);
		subButtonCloner.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		for (int i = 1; i <= 6; i++)
		{
			MissionSubType missionSubType = (MissionSubType)i;
			string fileName = string.Format("UI_missionACH_Tab0{0}_normal", i);
			if (i == (int)SelectedAchievementSubType)
			{
				fileName = string.Format("UI_missionACH_Tab0{0}_click", i);
			}
			MissionSubButton missionSubButton = UnityEngine.Object.Instantiate(subButtonCloner, objTopAchievementBanner.transform);
			missionSubButton.SetImage(MissionSubButton.ImageType.MAIN, fileName);
			missionSubButton.type = missionSubType;
			missionSubButton.title.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(list[i]);
			missionSubButton.clickCb = SubTypeMissionCB;
			missionSubButton.UpdateContent();
			dicSubButton.Add(missionSubType, missionSubButton);
		}
		subButtonCloner.gameObject.SetActive(false);
	}

	public void SubTypeMissionCB(MissionSubType subType)
	{
		foreach (KeyValuePair<MissionSubType, MissionSubButton> item in dicSubButton)
		{
			string fileName = string.Format("UI_missionACH_Tab0{0}_normal", (int)item.Key);
			if (item.Value.type == subType)
			{
				fileName = string.Format("UI_missionACH_Tab0{0}_click", (int)item.Key);
				if (currentMissionSubType != subType)
				{
					currentMissionSubType = subType;
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSubSE);
				}
			}
			item.Value.SetImage(MissionSubButton.ImageType.MAIN, fileName);
		}
		SelectedAchievementSubType = subType;
		OnClickAchievement(new StorageInfo("", false, 0)
		{
			Param = new object[1] { SelectedStatus }
		});
	}

	public void SwitchToSettledPanel(MissionType type)
	{
		canvasActivityTab.enabled = false;
		scrollRectMission.ClearCells();
		scrollRectMission.gameObject.SetActive((type != MissionType.MonthActivity) ? true : false);
		if (type != MissionType.MonthActivity)
		{
			scrollRectMission.OrangeInit(missionCell, 3, ManagedSingleton<MissionHelper>.Instance.FillteredDataCount);
		}
		SwitchTopPanel(type);
		SwitchBottomPanel(type);
		SwitchMiddlePanel();
	}

	public void SwitchTopPanel(MissionType type)
	{
		trmNaviParent.gameObject.SetActive((type == MissionType.Daily || type == MissionType.Activity) ? true : false);
		objTopDailyBanner.SetActive(type == MissionType.Daily);
		objTopAchievementBanner.SetActive(type == MissionType.Achievement);
		objTopEventBanner.SetActive(type == MissionType.Activity);
	}

	public void SwitchBottomPanel(MissionType type)
	{
		objBottomPanel.SetActive((type != MissionType.MonthActivity) ? true : false);
		objBottomSubDaily.SetActive(type == MissionType.Daily);
		objBottomSubAchievement.SetActive(type == MissionType.Achievement);
		objBottomSubEvent.SetActive(type == MissionType.Activity);
	}

	public void SwitchMiddlePanel()
	{
		canvasNoResultMsg.enabled = ManagedSingleton<MissionHelper>.Instance.FillteredDataCount == 0;
	}

	public void OnClickDaily(object p_param)
	{
		SelectedPage = MissionType.Daily;
		SelectedStatus = MissionHelper.MissionStatus.RUNNING;
		ManagedSingleton<MissionHelper>.Instance.CollectUIViewData(SelectedPage, SelectedStatus, 0);
		SwitchToSettledPanel(SelectedPage);
		textBottomSubDaily.text = OrangeGameUtility.GetRemainTimeText(MonoBehaviourSingleton<OrangeGameManager>.Instance.serverInfo.DailyResetInfo.CurrentResetTime);
		btnRetrieveAll.interactable = ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(SelectedPage, 0);
		SetStarCondition();
		UpdateLawBtnStatus(false);
	}

	public void OnClickAchievement(object p_param)
	{
		StorageInfo storageInfo = (StorageInfo)p_param;
		SelectedPage = MissionType.Achievement;
		SelectedStatus = (MissionHelper.MissionStatus)storageInfo.Param[0];
		ManagedSingleton<MissionHelper>.Instance.CollectUIViewData(SelectedPage, SelectedStatus, (int)SelectedAchievementSubType);
		SwitchToSettledPanel(SelectedPage);
		textBottomSubAchievement.text = ManagedSingleton<MissionHelper>.Instance.GetActivityValue(SelectedPage).ToString();
		btnRetrieveAll.interactable = SelectedStatus != MissionHelper.MissionStatus.DONE && (ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(SelectedPage, (int)SelectedAchievementSubType) ? true : false);
		foreach (KeyValuePair<MissionSubType, MissionSubButton> item in dicSubButton)
		{
			item.Value.UpdateContent();
		}
		UpdateLawBtnStatus(false);
	}

	public void OnClickEvent(object p_param)
	{
		StorageInfo storageInfo = (StorageInfo)p_param;
		SelectedPage = MissionType.Activity;
		SelectedStatus = MissionHelper.MissionStatus.RUNNING;
		SelectedActivitySubType = (int)storageInfo.Param[0];
		ManagedSingleton<MissionHelper>.Instance.CollectUIViewData(SelectedPage, SelectedStatus, SelectedActivitySubType);
		SwitchToSettledPanel(SelectedPage);
		textBottomSubEvent.text = string.Empty;
		foreach (EVENT_TABLE item in ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_MISSION, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
		{
			if (item.n_TYPE_X == SelectedActivitySubType)
			{
				textBottomSubEvent.text = OrangeGameUtility.GetRemainTimeText(ManagedSingleton<OrangeTableHelper>.Instance.ServerDateToUTC(item.s_END_TIME));
				LoadBanner(item.s_IMG);
				break;
			}
		}
		btnRetrieveAll.interactable = ManagedSingleton<MissionHelper>.Instance.HasMissionToRetrieve(SelectedPage, SelectedActivitySubType);
		UpdateLawBtnStatus(false);
	}

	public void LoadBanner(string image)
	{
		List<BANNER_TABLE> bannerInfo = new List<BANNER_TABLE>();
		BANNER_TABLE bANNER_TABLE = new BANNER_TABLE();
		bANNER_TABLE.s_IMG = image;
		bannerInfo.Add(bANNER_TABLE);
		OrangeBannerComponent comp = trmBannerRoot.GetComponentInChildren<OrangeBannerComponent>();
		if (comp == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/BannerComp", "BannerComp", delegate(GameObject obj)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(obj, trmBannerRoot);
				comp = gameObject.GetComponent<OrangeBannerComponent>();
				comp.Setup(trmBannerRoot, bannerInfo, new Vector2Int(512, 128), new Vector2(0f, 0f));
			});
		}
		else
		{
			comp.Setup(trmBannerRoot, bannerInfo, new Vector2Int(512, 128), new Vector2(0f, 0f));
		}
	}

	public void OnRetrieveAll()
	{
		if (SelectedPage == MissionType.Daily || SelectedPage == MissionType.Achievement || SelectedPage == MissionType.Activity)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickGetAllSE);
			originalAP = ManagedSingleton<PlayerHelper>.Instance.GetStamina();
			originalEP = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
			int subType = 0;
			if (SelectedPage == MissionType.Achievement)
			{
				subType = (int)SelectedAchievementSubType;
			}
			else if (SelectedPage == MissionType.Activity)
			{
				subType = SelectedActivitySubType;
			}
			bool epExcluded;
			List<int> missionCouldBeRetrievedListIncludePredict = ManagedSingleton<MissionHelper>.Instance.GetMissionCouldBeRetrievedListIncludePredict(SelectedPage, subType, out epExcluded);
			if (missionCouldBeRetrievedListIncludePredict.Count <= 0 && epExcluded)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("EPMAX_MESSAGE", 1f);
			}
			else
			{
				OnStartRetrieveAll(new List<NetRewardInfo>(), missionCouldBeRetrievedListIncludePredict);
			}
		}
	}

	private void OnStartRetrieveAll(List<NetRewardInfo> rewardList, List<int> missionList)
	{
		if (missionList.Count > 0)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(missionList, delegate(object p_param)
			{
				List<NetRewardInfo> list = p_param as List<NetRewardInfo>;
				if (list == null)
				{
					list = new List<NetRewardInfo>();
				}
				list.AddRange(rewardList);
				int subType = 0;
				if (SelectedPage == MissionType.Achievement)
				{
					subType = (int)SelectedAchievementSubType;
				}
				else if (SelectedPage == MissionType.Activity)
				{
					subType = SelectedActivitySubType;
				}
				bool epExcluded;
				List<int> missionCouldBeRetrievedListIncludePredict = ManagedSingleton<MissionHelper>.Instance.GetMissionCouldBeRetrievedListIncludePredict(SelectedPage, subType, out epExcluded);
				if (missionCouldBeRetrievedListIncludePredict.Count > 0)
				{
					OnStartRetrieveAll(list, missionCouldBeRetrievedListIncludePredict);
				}
				else
				{
					OnStartShowRewardPopop(list);
				}
			});
		}
		else
		{
			OnStartShowRewardPopop(rewardList);
		}
	}

	private void OnStartShowRewardPopop(List<NetRewardInfo> rewardList)
	{
		int aPCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetStamina() - originalAP, 0);
		int ePCount = Math.Max(ManagedSingleton<PlayerHelper>.Instance.GetEventStamina() - originalEP, 0);
		MonoBehaviourSingleton<OrangeGameManager>.Instance.AddAPEPToRewardList(ref rewardList, aPCount, ePCount);
		if (rewardList.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLvUp)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform();
					});
				}
				ui.Setup(rewardList);
			});
		}
		else
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform();
		}
		UpdateMissionList();
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MISSION);
	}

	private void UpdateStorageHintAdapter(IconHintChk.HINT_TYPE notifyType)
	{
		UpdateStorageHint();
	}

	public void UpdateStorageHint()
	{
		StorageComponent componentInChildren = GetComponentInChildren<StorageComponent>();
		if ((bool)componentInChildren)
		{
			componentInChildren.UpdateHint();
		}
	}

	public void UpdateMissionList()
	{
		switch (SelectedPage)
		{
		case MissionType.Daily:
			OnClickDaily(null);
			break;
		case MissionType.Achievement:
			OnClickAchievement(new StorageInfo("", false, 0)
			{
				Param = new object[1] { SelectedStatus }
			});
			break;
		case MissionType.Activity:
			OnClickEvent(new StorageInfo("", false, 0)
			{
				Param = new object[1] { SelectedActivitySubType }
			});
			break;
		}
		UpdateStorageHint();
	}

	public void PrepareMonthlyActivityData()
	{
		bool flag = false;
		foreach (MISSION_TABLE item in ManagedSingleton<OrangeTableHelper>.Instance.GetMissionTableByType(MissionType.MonthActivity, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
		{
			if (item.n_CONDITION_X == 0)
			{
				listMonthActivityPool.Add(item);
			}
			else
			{
				listMonthActivityPaid.Add(item);
			}
			if (!flag)
			{
				currentPayItemID = item.n_CONDITION_Z;
				TimeText.text = OrangeGameUtility.DisplayDatePeriod(item.s_BEGIN_TIME, item.s_END_TIME);
				flag = true;
			}
		}
	}

	public void OnUpdateActivityList()
	{
		scrollRectActivity.ClearCells();
		scrollRectActivity.gameObject.SetActive(true);
	}

	public void OnOpenMonthlyActivityinfo(bool atv)
	{
		if (atv)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickMonthlyInfo);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		}
		MonthlyActivityinfo.SetActive(atv);
	}

	public void OnMonthlyActivityBuyPass()
	{
		if (!ManagedSingleton<MissionHelper>.Instance.CurrentMontlyActivityPaid)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendConfirm", delegate(FriendConfirmUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				string p_title = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MONTHLY_ACTIVE_REWARD_UPGRADE"));
				string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MONTHLY_ACTIVE_UPGRADE_CONFIRM"));
				string p_textYes = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"));
				string p_textNo = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"));
				ui.SetupYesNO(p_title, p_msg, p_textYes, p_textNo, OnBuyPass);
			});
		}
	}

	public void OnBuyPass()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
	}

	public MISSION_TABLE OnGetMissionTable(int idx, bool paid)
	{
		if (paid)
		{
			return listMonthActivityPaid[idx];
		}
		return listMonthActivityPool[idx];
	}

	public void OnSetAllRetrieveBtn(bool b)
	{
		AllRetrieveBtn.interactable = b;
	}

	public void OnClickActivityTab(object p_param)
	{
		SelectedPage = MissionType.MonthActivity;
		SwitchToSettledPanel(SelectedPage);
		MonthlyActivityinfo.SetActive(false);
		OnUpdateActivityList();
		totalActivityText.text = ManagedSingleton<MissionHelper>.Instance.CurrentMonthlyActivityValue.ToString();
		bool currentMontlyActivityPaid = ManagedSingleton<MissionHelper>.Instance.CurrentMontlyActivityPaid;
		BuyPassBtn.interactable = !currentMontlyActivityPaid;
		BuyObject.SetActive(!currentMontlyActivityPaid);
		BoughtObject.SetActive(currentMontlyActivityPaid);
		lockIcon.SetActive(!currentMontlyActivityPaid);
		payMask.SetActive(!currentMontlyActivityPaid);
		SetScrollRectActivity();
		if (!currentMontlyActivityPaid && (activityProduct == null || activityShopTable == null))
		{
			MonoBehaviourSingleton<OrangeIAP>.Instance.Init(delegate
			{
				bool flag = false;
				EVENT_TABLE eventTableByCounter = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByCounter(ManagedSingleton<MissionHelper>.Instance.CurrentMonthlyActivityCounterID);
				if (eventTableByCounter != null)
				{
					if (ManagedSingleton<ExtendDataHelper>.Instance.SHOP_TABLE_DICT.TryGetValue(eventTableByCounter.n_TYPE_X, out activityShopTable) && MonoBehaviourSingleton<OrangeIAP>.Instance.DictProduct.TryGetValue(activityShopTable.s_PRODUCT_ID, out activityProduct))
					{
						flag = true;
						BuyPassPrice.text = activityProduct.LocalizedPriceString;
					}
					BuyPassBtn.interactable = flag;
					if (!flag)
					{
						BuyPassPrice.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("IAP_UNAVAILABLE");
					}
				}
			});
		}
		canvasActivityTab.enabled = true;
		OnResetAllRetrieveBtn();
		UpdateLawBtnStatus(true);
	}

	public void SetScrollRectActivity()
	{
		scrollRectActivity.OrangeInit(monthActivityCell, 5, listMonthActivityPool.Count);
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (!enable)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
		}
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SWITCH_SCENE, Clear);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_SHOP, RefreashUI);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CHANGE_DAY, DayChange);
		Singleton<GenericEventManager>.Instance.AttachEvent<IconHintChk.HINT_TYPE>(EventManager.ID.UPDATE_HOMETOP_HINT, UpdateStorageHintAdapter);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SWITCH_SCENE, Clear);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_SHOP, RefreashUI);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CHANGE_DAY, DayChange);
		Singleton<GenericEventManager>.Instance.DetachEvent<IconHintChk.HINT_TYPE>(EventManager.ID.UPDATE_HOMETOP_HINT, UpdateStorageHintAdapter);
		MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
	}

	private void Clear()
	{
		OnClickCloseBtn();
	}

	public void ShowRewardPopup()
	{
		if (rewardList != null && rewardList.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(rewardList);
			});
			OnClickActivityTab(null);
		}
	}

	public void CheckRetrieveRewardNext()
	{
		CurrentRewardIndex++;
		if (CurrentRewardIndex < listMonthActivityPool.Count)
		{
			MISSION_TABLE mISSION_TABLE = OnGetMissionTable(CurrentRewardIndex, false);
			if (mISSION_TABLE == null)
			{
				return;
			}
			if (ManagedSingleton<MissionHelper>.Instance.CurrentMonthlyActivityValue >= mISSION_TABLE.n_CONDITION_Y)
			{
				RetrieveRewardLoop();
				return;
			}
		}
		ShowRewardPopup();
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.MISSION);
	}

	public void RetrievePaidRewardLoop()
	{
		MISSION_TABLE mISSION_TABLE = OnGetMissionTable(CurrentRewardIndex, true);
		bool flag = false;
		if (ManagedSingleton<MissionHelper>.Instance.CurrentMontlyActivityPaid && mISSION_TABLE != null && !ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(mISSION_TABLE.n_ID) && ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(mISSION_TABLE.n_ID))
		{
			flag = true;
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(mISSION_TABLE.n_ID, delegate(object p_param)
			{
				rewardList.AddRange(p_param as List<NetRewardInfo>);
				CheckRetrieveRewardNext();
			});
		}
		if (!flag)
		{
			CheckRetrieveRewardNext();
		}
	}

	public void RetrieveRewardLoop()
	{
		MISSION_TABLE mISSION_TABLE = OnGetMissionTable(CurrentRewardIndex, false);
		if (mISSION_TABLE == null)
		{
			return;
		}
		if (!ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(mISSION_TABLE.n_ID))
		{
			if (ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(mISSION_TABLE.n_ID))
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(mISSION_TABLE.n_ID, delegate(object p_param)
				{
					rewardList.AddRange(p_param as List<NetRewardInfo>);
					RetrievePaidRewardLoop();
				});
			}
		}
		else
		{
			RetrievePaidRewardLoop();
		}
	}

	public void OnClickAllRetrieveBtn()
	{
		CurrentRewardIndex = 0;
		rewardList.Clear();
		RetrieveRewardLoop();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickGetAllSE);
	}

	public bool HasMonthlyActivityRewardToRetrieve(object[] param)
	{
		for (int i = 0; i < listMonthActivityPool.Count; i++)
		{
			MISSION_TABLE mISSION_TABLE = OnGetMissionTable(i, false);
			if (ManagedSingleton<MissionHelper>.Instance.CurrentMonthlyActivityValue >= mISSION_TABLE.n_CONDITION_Y)
			{
				if (!ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(mISSION_TABLE.n_ID) && ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(mISSION_TABLE.n_ID))
				{
					return true;
				}
				MISSION_TABLE mISSION_TABLE2 = OnGetMissionTable(i, true);
				if (ManagedSingleton<MissionHelper>.Instance.CurrentMontlyActivityPaid && mISSION_TABLE2 != null && !ManagedSingleton<MissionHelper>.Instance.CheckMissionRewardRetrieved(mISSION_TABLE2.n_ID) && ManagedSingleton<MissionHelper>.Instance.CheckMissionCompleted(mISSION_TABLE2.n_ID))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnResetAllRetrieveBtn()
	{
		OnSetAllRetrieveBtn(HasMonthlyActivityRewardToRetrieve(null));
		UpdateStorageHint();
	}

	public void OnClickBuyUpgrade()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.SetupYesNoByKey("MONTHLY_ACTIVE_REWARD_UPGRADE", "MONTHLY_ACTIVE_UPGRADE_CONFIRM", "COMMON_OK", "COMMON_CANCEL", delegate
			{
				EVENT_TABLE eventTableByCounter = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByCounter(ManagedSingleton<MissionHelper>.Instance.CurrentMonthlyActivityCounterID);
				if (eventTableByCounter != null && ManagedSingleton<ExtendDataHelper>.Instance.SHOP_TABLE_DICT.TryGetValue(eventTableByCounter.n_TYPE_X, out activityShopTable) && MonoBehaviourSingleton<OrangeIAP>.Instance.DictProduct.TryGetValue(activityShopTable.s_PRODUCT_ID, out activityProduct) && activityShopTable != null && activityProduct != null)
				{
					MonoBehaviourSingleton<OrangeIAP>.Instance.DoPurchase(activityShopTable, activityProduct);
				}
			});
		});
	}

	private void UpdateLawBtnStatus(bool active)
	{
		Transform[] array = transformLawBtns;
		foreach (Transform transform in array)
		{
			if ((bool)transform && transform.gameObject.activeSelf != active)
			{
				transform.gameObject.SetActive(active);
			}
		}
	}

	private void LoadLawBtns()
	{
		if (!transformLawBtns[0])
		{
			CommonAssetHelper.LoadLawObj("BtnJPLaw", base.transform, new Vector3(-775f, -425f, 0f), delegate(Transform t)
			{
				transformLawBtns[0] = t;
				t.gameObject.SetActive(false);
			});
		}
		if (!transformLawBtns[1])
		{
			CommonAssetHelper.LoadLawObj("BtnJPFund", base.transform, new Vector3(-775f, -495f, 0f), delegate(Transform t)
			{
				transformLawBtns[1] = t;
				t.gameObject.SetActive(false);
			});
		}
	}

	public void RefreashUI()
	{
		OnClickActivityTab(null);
	}

	public void DayChange()
	{
		MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
	}
}
