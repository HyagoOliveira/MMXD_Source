#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using PathCreation;
using UnityEngine;
using UnityEngine.UI;

public class GuildMainSceneController : OrangeSceneController
{
	public enum FxName
	{
		fx_guild_building_showup = 0,
		fx_guild_building_levelup = 1
	}

	public enum BuildingType
	{
		Lobby = 0,
		GuildBoss = 1,
		PowerTower = 2,
		Wanted = 3
	}

	public enum BuildingState
	{
		Normal = 0,
		ShowUp = 1,
		RankUp = 2
	}

	[Serializable]
	private class BuildingSetting
	{
		public BuildingType BuildingType;

		public GameObject[] Models;

		public Button EntryButton;

		public PathCreator CameraPath;

		public Vector3 LastCameraAngle;

		public Transform FXAnchor;

		public float FXShowUpScale;

		public float FXLevelUpScale;
	}

	[SerializeField]
	private GuildCameraPathFollower _cameraPathFollower;

	[SerializeField]
	private BuildingSetting[] _buildingSettings;

	public float TIME_BUILDING_SHOWUP_DELAY = 0.5f;

	public float TIME_BUILDING_LEVELUP_DELAY = 0.5f;

	[SerializeField]
	private Camera _modelCamera;

	[SerializeField]
	private GuildMainSceneNPCController _npcController;

	[SerializeField]
	private Text _textCrusadeTime;

	private GuildMainUI _mainUI;

	private long _crusadeStartTime;

	private long _crusadeEndTime;

	private long _crusadeRankingTime;

	private long _crusadeRemainTime;

	private Coroutine _crusadeUpdateCoroutine;

	private bool _isDirectMode;

	public bool bMuteFirstSE;

	private List<GuildTutorialInfo> _tutorialInfoListCache;

	private GuildTutorialInfo _tutorialInfoMain;

	private List<GuildTutorialInfo> _tutorialUnfinishedShowUpInfoList;

	private List<int> _tutorialUnfinishedIDList;

	private bool _hasUnfinishedSceneTutorial;

	private void OnLoadGuildLobbyUIState()
	{
		SetCameraPath(BuildingType.Lobby, LoadGuildLobbyUI);
	}

	private void OnLoadWantedUIState()
	{
		if (_isDirectMode)
		{
			_isDirectMode = false;
			LoadWantedUI();
		}
		else
		{
			SetCameraPath(BuildingType.Wanted, LoadWantedUI);
		}
	}

	private void OnLoadPowerTowerUIState()
	{
		SetCameraPath(BuildingType.PowerTower, LoadPowerTowerUI);
	}

	private void OnLoadGuildBossUIState()
	{
		if (_isDirectMode)
		{
			_isDirectMode = false;
			LoadGuildBossUI();
		}
		else
		{
			SetCameraPath(BuildingType.GuildBoss, LoadGuildBossUI);
		}
	}

	private void SetCameraPath(BuildingType buildingType, Callback onFinished)
	{
		ToggleEntryButton(false);
		BuildingSetting buildingSetting = _buildingSettings.FirstOrDefault((BuildingSetting setting) => setting.BuildingType == buildingType);
		if (buildingSetting != null && buildingSetting.CameraPath != null)
		{
			_cameraPathFollower.SetPathCreator(buildingSetting.CameraPath, buildingSetting.LastCameraAngle);
			MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(onFinished, OrangeSceneManager.LoadingType.BLACK, _cameraPathFollower.MoveTime);
		}
		else if (onFinished != null)
		{
			onFinished();
		}
	}

	private void CheckShowBuildingModel(BuildingType buildingType, string modelName, string modelNamePrev, out BuildingState buildingState)
	{
		BuildingSetting buildingSetting = _buildingSettings.FirstOrDefault((BuildingSetting setting) => setting.BuildingType == buildingType);
		if (buildingSetting != null)
		{
			if (CheckShowBuildingModel(buildingSetting, modelName, modelNamePrev, out buildingState) && buildingSetting.EntryButton != null)
			{
				buildingSetting.EntryButton.gameObject.SetActive(true);
			}
		}
		else
		{
			buildingState = BuildingState.Normal;
		}
	}

	private bool CheckShowBuildingModel(BuildingSetting buildingSetting, string modelName, string modelNamePrev, out BuildingState buildingState)
	{
		buildingState = BuildingState.Normal;
		if (string.IsNullOrEmpty(modelName))
		{
			return false;
		}
		GameObject gameObject = buildingSetting.Models.FirstOrDefault((GameObject go) => go.name == modelName);
		if (gameObject == null)
		{
			Debug.LogError("Invalid ModelName : " + modelName);
			return false;
		}
		string triggerName = "Guild_" + modelName;
		GuildTutorialInfo tutorialInfoShowUp = _tutorialUnfinishedShowUpInfoList.FirstOrDefault((GuildTutorialInfo info) => triggerName.StartsWith(info.TriggerKey));
		if (tutorialInfoShowUp != null)
		{
			Debug.LogWarning(string.Format("GuildBuilding ShowUp Tutorial = {0}/{1}", tutorialInfoShowUp.TutorialID, tutorialInfoShowUp.TriggerKey));
		}
		if (tutorialInfoShowUp != null)
		{
			buildingState = BuildingState.ShowUp;
			PlayBuildingModelShowUp(gameObject, buildingSetting, delegate
			{
				TurtorialUI.CheckTurtorialID(tutorialInfoShowUp.TutorialID);
			});
			return true;
		}
		if (_hasUnfinishedSceneTutorial || TurtorialUI.IsTutorialing())
		{
			ShowBuildingModel(gameObject);
			return true;
		}
		if (modelName != modelNamePrev)
		{
			GameObject modelPrev = buildingSetting.Models.FirstOrDefault((GameObject go) => go.name == modelNamePrev);
			if (modelPrev == null)
			{
				Debug.LogError("Invalid ModelNamePrev : " + modelNamePrev);
				return false;
			}
			ShowBuildingModel(modelPrev);
			PlayBuildingModelLevelUp(gameObject, buildingSetting, delegate
			{
				modelPrev.SetActive(false);
			});
			return true;
		}
		ShowBuildingModel(gameObject);
		return true;
	}

	public void PlayBuildingFXTest()
	{
		BuildingSetting[] buildingSettings = _buildingSettings;
		foreach (BuildingSetting obj in buildingSettings)
		{
			Transform fXAnchor = obj.FXAnchor;
			float fXLevelUpScale = obj.FXLevelUpScale;
			if (!(fXAnchor == null))
			{
				FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FxName.fx_guild_building_levelup.ToString(), fXAnchor, Quaternion.identity, Array.Empty<object>());
				if (fxBase != null)
				{
					fxBase.transform.localScale = new Vector3(fXLevelUpScale, 1f, fXLevelUpScale);
				}
				else
				{
					Debug.LogError("Failed to load Building FX");
				}
			}
		}
	}

	private void PlayBuildingModelShowUp(GameObject model, BuildingSetting buildingSetting, Action onFinished = null)
	{
		ShowBuildingModel(model, onFinished, true, TIME_BUILDING_SHOWUP_DELAY, buildingSetting.FXAnchor, FxName.fx_guild_building_showup.ToString(), buildingSetting.FXShowUpScale);
	}

	private void PlayBuildingModelLevelUp(GameObject model, BuildingSetting buildingSetting, Action onFinished = null)
	{
		ShowBuildingModel(model, onFinished, true, TIME_BUILDING_LEVELUP_DELAY, buildingSetting.FXAnchor, FxName.fx_guild_building_levelup.ToString(), buildingSetting.FXLevelUpScale);
	}

	private void ShowBuildingModel(GameObject model, Action onFinished = null, bool isDelay = false, float delayTime = 0f, Transform fxAnchor = null, string fxName = "", float fxScale = 1f)
	{
		if (model == null)
		{
			return;
		}
		if (!isDelay)
		{
			model.SetActive(true);
			if (onFinished != null)
			{
				onFinished();
			}
		}
		else
		{
			StartCoroutine(DelayShowBuildingModelCoroutine(model, delayTime, fxName, fxAnchor, fxScale, onFinished));
		}
	}

	private IEnumerator DelayShowBuildingModelCoroutine(GameObject model, float delayTime, string fxName, Transform fxAnchor, float fxScale, Action onFinished)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Block(true);
		FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(fxName, (fxAnchor != null) ? fxAnchor.position : model.transform.position, Quaternion.identity, Array.Empty<object>());
		if (fxBase != null)
		{
			fxBase.transform.localScale = new Vector3(fxScale, 1f, fxScale);
		}
		else
		{
			Debug.LogError("Failed to load Building FX");
		}
		yield return new WaitForSeconds(delayTime);
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
		ShowBuildingModel(model, onFinished);
	}

	public void OnClickGuildLobbyBtn()
	{
		PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		LockUIState(ClickGuildLobbyBtnAction);
	}

	private void ClickGuildLobbyBtnAction()
	{
		Singleton<GuildSystem>.Instance.OnGetMemberInfoListEvent += OnGetMemberInfoListEvent;
		Singleton<GuildSystem>.Instance.ReqGetMemberInfoList();
	}

	public void OnClickWantedBtn()
	{
		TriggerWantedBtnAction();
	}

	private void ClickWantedBtnAction()
	{
		PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		Singleton<WantedSystem>.Instance.ReqRetrieveWantedInfo(true);
	}

	public void OnClickGuildBossBtn()
	{
		PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		TriggerGuildBossBtnAction();
	}

	public void OnClickPowerTowerBtn()
	{
		PlaySE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		LockUIState(ClickPowerTowerBtnAction);
	}

	private void ClickPowerTowerBtnAction()
	{
		Singleton<PowerTowerSystem>.Instance.OnGetPowerPillarInfoOnceEvent += OnGetPowerPillarInfoOnceEvent;
		Singleton<PowerTowerSystem>.Instance.ReqGetPowerPillarInfo();
	}

	private void ClickGuildBossBtnAction()
	{
		Singleton<CrusadeSystem>.Instance.OnRetrieveCrusadeInfoOnceEvent += OnRetrieveCrusadeInfoOnceEvent;
		Singleton<CrusadeSystem>.Instance.RetrieveCrusadeInfo();
	}

	public void OnClickGuildBossEntryHintBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.Block(true);
		LoadCrusadeEntryHintUI();
	}

	private void PlaySE(SystemSE eCue)
	{
		if (bMuteFirstSE)
		{
			bMuteFirstSE = false;
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(eCue);
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void SceneInit()
	{
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 3);
		});
		LoadGuildMainUI();
		InitializeTutorialInfo();
		RefreshTutorialState();
		RefreshCityModel();
		CheckCrusadeEvent();
	}

	protected void OnEnable()
	{
        Singleton<GuildSystem>.Instance.MainSceneController = this;
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.TOGGLE_GUILD_SCENE_RENDER, OnToggleGuildSceneRenderEvent);
		Singleton<CrusadeSystem>.Instance.OnRetrieveCrusadeInfoEvent += OnRetrieveCrusadeInfoEvent;
		Singleton<GuildSystem>.Instance.OnRankupGuildEvent += OnRankupGuildEvent;
		Singleton<WantedSystem>.Instance.OnRetrieveWantedInfoEvent += OnRetrieveWantedInfoEvent;
		Singleton<PowerTowerSystem>.Instance.OnGetPowerPillarInfoEvent += OnGetPowerPillarInfoEvent;
	}

	protected override void OnDisable()
	{
		Singleton<GuildSystem>.Instance.MainSceneController = null;
		if (_crusadeUpdateCoroutine != null)
		{
			StopCoroutine(_crusadeUpdateCoroutine);
		}
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.TOGGLE_GUILD_SCENE_RENDER, OnToggleGuildSceneRenderEvent);
		Singleton<CrusadeSystem>.Instance.OnRetrieveCrusadeInfoEvent -= OnRetrieveCrusadeInfoEvent;
		Singleton<GuildSystem>.Instance.OnRankupGuildEvent -= OnRankupGuildEvent;
		Singleton<WantedSystem>.Instance.OnRetrieveWantedInfoEvent -= OnRetrieveWantedInfoEvent;
		Singleton<PowerTowerSystem>.Instance.OnGetPowerPillarInfoEvent -= OnGetPowerPillarInfoEvent;
		base.OnDisable();
	}

	private void OnToggleGuildSceneRenderEvent(bool isOn)
	{
		_modelCamera.enabled = isOn;
		_npcController.IsPaused = !isOn;
	}

	private void CloseGuildScene()
	{
		Singleton<GuildSystem>.Instance.CloseGuildLobbyScene();
	}

	private void RefreshGuildScene()
	{
		if (!_mainUI.IsLock)
		{
			_mainUI.RefreshGuildInfo();
			RefreshTutorialState();
			RefreshCityModel();
		}
	}

	private void RefreshCityModel()
	{
		Debug.Log("RefreshCityModel");
		GuildSetting guildSetting = Singleton<GuildSystem>.Instance.GuildSetting;
		if (guildSetting == null)
		{
			return;
		}
		GuildSetting guildSetting2;
		GuildSetting guildSetting3 = (GuildSetting.TryGetSettingByGuildRank(Singleton<GuildSystem>.Instance.GuildRankRead, out guildSetting2) ? guildSetting2 : guildSetting);
		ToggleEntryButton(false);
		BuildingSetting[] buildingSettings = _buildingSettings;
		for (int i = 0; i < buildingSettings.Length; i++)
		{
			buildingSettings[i].Models.ForEach(delegate(GameObject go)
			{
				go.SetActive(false);
			});
		}
		BuildingState buildingState;
		CheckShowBuildingModel(BuildingType.Lobby, guildSetting.ModelNameLobby, guildSetting3.ModelNameLobby, out buildingState);
		CheckShowBuildingModel(BuildingType.GuildBoss, guildSetting.ModelNameGuildBoss, guildSetting3.ModelNameGuildBoss, out buildingState);
		if (_tutorialInfoMain != null && _tutorialUnfinishedIDList.Contains(_tutorialInfoMain.TutorialID))
		{
			Singleton<GuildSystem>.Instance.ReqSetGuildRankRead();
			TurtorialUI.OnTutorialFinishedEvent += OnTutorialFinishedEvent;
			return;
		}
		CheckShowBuildingModel(BuildingType.Wanted, guildSetting.ModelNameWanted, guildSetting3.ModelNameWanted, out buildingState);
		if (buildingState == BuildingState.ShowUp)
		{
			Singleton<GuildSystem>.Instance.ReqSetGuildRankRead();
			return;
		}
		CheckShowBuildingModel(BuildingType.PowerTower, guildSetting.ModelNamePowerTower, guildSetting3.ModelNamePowerTower, out buildingState);
		if (buildingState == BuildingState.ShowUp)
		{
			Singleton<GuildSystem>.Instance.ReqSetGuildRankRead();
		}
		else if (TurtorialUI.IsTutorialing())
		{
			Singleton<GuildSystem>.Instance.ReqSetGuildRankRead();
		}
		else if (Singleton<GuildSystem>.Instance.GuildRankRead < Singleton<GuildSystem>.Instance.GuildInfoCache.Rank)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<TipResultUI>("UI_TipResult", OnRankupTipResultUILoaded);
		}
	}

	private void OnRankupTipResultUILoaded(TipResultUI ui)
	{
		string guildRankString = Singleton<GuildSystem>.Instance.GetGuildRankString(Singleton<GuildSystem>.Instance.GuildInfoCache.Rank);
		ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LEVELUP_BULLETIN", guildRankString));
		Singleton<GuildSystem>.Instance.ReqSetGuildRankRead();
	}

	public void TriggerGuildBossBtnAction(bool isDirectMode = false)
	{
		_isDirectMode = isDirectMode;
		LockUIState(ClickGuildBossBtnAction);
	}

	public void TriggerWantedBtnAction(bool isDirectMode = false)
	{
		_isDirectMode = isDirectMode;
		LockUIState(ClickWantedBtnAction);
	}

	private void ToggleEntryButton(bool isActive)
	{
		BuildingSetting[] buildingSettings = _buildingSettings;
		foreach (BuildingSetting buildingSetting in buildingSettings)
		{
			if (buildingSetting.EntryButton != null)
			{
				buildingSetting.EntryButton.gameObject.SetActive(isActive);
			}
		}
	}

	private void CheckCrusadeEvent()
	{
		long nowTime = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowLocale;
		List<EVENT_TABLE> list = (from attrData in ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values
			where attrData.n_TYPE == 15 && nowTime < CapUtility.DateToUnixTime(DateTime.Parse(attrData.s_REMAIN_TIME))
			orderby CapUtility.DateToUnixTime(DateTime.Parse(attrData.s_BEGIN_TIME))
			select attrData).ToList();
		if (list.Count == 0)
		{
			BuildingSetting buildingSetting = _buildingSettings.FirstOrDefault((BuildingSetting setting) => setting.BuildingType == BuildingType.GuildBoss);
			if (buildingSetting != null && buildingSetting.EntryButton != null)
			{
				buildingSetting.EntryButton.interactable = false;
			}
			_textCrusadeTime.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CRUSADE_COMINGSOON");
		}
		else
		{
			EVENT_TABLE targetEvent = list.First();
			ResetCrusadeTimeRoutine(targetEvent);
		}
	}

	private void ResetCrusadeTimeRoutine(EVENT_TABLE targetEvent)
	{
		_crusadeStartTime = CapUtility.DateToUnixTime(DateTime.Parse(targetEvent.s_BEGIN_TIME));
		_crusadeEndTime = CapUtility.DateToUnixTime(DateTime.Parse(targetEvent.s_END_TIME));
		_crusadeRankingTime = CapUtility.DateToUnixTime(DateTime.Parse(targetEvent.s_RANKING_TIME));
		_crusadeRemainTime = CapUtility.DateToUnixTime(DateTime.Parse(targetEvent.s_REMAIN_TIME));
		if (_crusadeUpdateCoroutine != null)
		{
			StopCoroutine(_crusadeUpdateCoroutine);
		}
		_crusadeUpdateCoroutine = StartCoroutine(UpdateCrusadeTime());
	}

	private IEnumerator UpdateCrusadeTime()
	{
		BuildingSetting buildingSetting = _buildingSettings.FirstOrDefault((BuildingSetting setting) => setting.BuildingType == BuildingType.GuildBoss);
		while (true)
		{
			if (buildingSetting != null && buildingSetting.EntryButton != null)
			{
				buildingSetting.EntryButton.interactable = true;
			}
			long serverUnixTimeNowLocale = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowLocale;
			if (serverUnixTimeNowLocale <= _crusadeStartTime)
			{
				if (buildingSetting != null && buildingSetting.EntryButton != null)
				{
					buildingSetting.EntryButton.interactable = false;
				}
				_textCrusadeTime.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CRUSADE_START", OrangeGameUtility.GetTimeText(_crusadeStartTime - serverUnixTimeNowLocale));
			}
			else if (serverUnixTimeNowLocale <= _crusadeEndTime)
			{
				_textCrusadeTime.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CRUSADE_END", OrangeGameUtility.GetTimeText(_crusadeEndTime - serverUnixTimeNowLocale));
			}
			else if (serverUnixTimeNowLocale <= _crusadeRankingTime)
			{
				_textCrusadeTime.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CRUSADE_SETTLEMENT", OrangeGameUtility.GetTimeText(_crusadeRankingTime - serverUnixTimeNowLocale));
			}
			else
			{
				if (serverUnixTimeNowLocale > _crusadeRemainTime)
				{
					break;
				}
				_textCrusadeTime.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_CRUSADE_KEEP", OrangeGameUtility.GetTimeText(_crusadeRemainTime - serverUnixTimeNowLocale));
			}
			yield return CoroutineDefine._1sec;
		}
		CheckCrusadeEvent();
	}

	private void OnGetMemberInfoListEvent(Code ackCode)
	{
		Singleton<GuildSystem>.Instance.OnGetMemberInfoListEvent -= OnGetMemberInfoListEvent;
		if (ackCode == Code.GUILD_GET_GUILD_MEMBER_LIST_SUCCESS)
		{
			GuildUIHelper.DoStateAction(OnLoadGuildLobbyUIState);
		}
		else
		{
			UnlockUIState();
		}
	}

	private void OnRetrieveWantedInfoEvent(Code ackCode)
	{
		if (ackCode == Code.WANTED_GET_INFO_SUCCESS)
		{
			GuildUIHelper.DoStateAction(OnLoadWantedUIState);
		}
		else
		{
			UnlockUIState();
		}
	}

	private void OnGetPowerPillarInfoEvent(Code ackCode)
	{
		if (ackCode != Code.GUILD_GET_POWER_PILLAR_INFO_SUCCESS)
		{
			UnlockUIState();
		}
	}

	private void OnGetPowerPillarInfoOnceEvent()
	{
		Singleton<PowerTowerSystem>.Instance.OnGetOreInfoOnceEvent += OnGetOreInfoOnceEvent;
		Singleton<PowerTowerSystem>.Instance.ReqGetOreInfo();
	}

	private void OnGetOreInfoOnceEvent()
	{
		GuildUIHelper.DoStateAction(OnLoadPowerTowerUIState);
	}

	private void OnRankupGuildEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		if (ackCode == Code.GUILD_RANK_UP_SUCCESS)
		{
			RefreshCityModel();
		}
	}

	private void OnRetrieveCrusadeInfoEvent(Code ackCode)
	{
		if (ackCode == Code.CRUSADE_GET_INFO_SUCCESS || ackCode == Code.CRUSADE_EVENT_NO_OPEN_DATA)
		{
			if (Singleton<CrusadeSystem>.Instance.HasEvent)
			{
				int eventId = Singleton<CrusadeSystem>.Instance.EventID;
				EVENT_TABLE eVENT_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT.Values.FirstOrDefault((EVENT_TABLE attrData) => attrData.n_ID == eventId && attrData.n_TYPE == 15);
				if (eVENT_TABLE != null)
				{
					ResetCrusadeTimeRoutine(eVENT_TABLE);
				}
			}
		}
		else
		{
			UnlockUIState();
		}
	}

	private void OnRetrieveCrusadeInfoOnceEvent()
	{
		if (!Singleton<CrusadeSystem>.Instance.HasEvent)
		{
			Debug.LogError("No GuildBoss Event");
			UnlockUIState();
		}
		else
		{
			GuildUIHelper.DoStateAction(OnLoadGuildBossUIState);
		}
	}

	private void InitializeTutorialInfo()
	{
		_tutorialInfoListCache = Singleton<GuildSystem>.Instance.TutorialInfoList;
		_tutorialInfoMain = _tutorialInfoListCache.FirstOrDefault((GuildTutorialInfo info) => info.IsMain);
	}

	private void RefreshTutorialState()
	{
		_tutorialUnfinishedIDList = _tutorialInfoListCache.Select((GuildTutorialInfo info) => info.TutorialID).Except(ManagedSingleton<PlayerNetManager>.Instance.TutorialList).ToList();
		_tutorialUnfinishedShowUpInfoList = _tutorialInfoListCache.Where((GuildTutorialInfo info) => info.IsBuildingShowUp && _tutorialUnfinishedIDList.Contains(info.TutorialID)).ToList();
		_hasUnfinishedSceneTutorial = (_tutorialInfoMain != null && _tutorialUnfinishedIDList.Contains(_tutorialInfoMain.TutorialID)) || _tutorialUnfinishedShowUpInfoList.Count > 0;
	}

	private void OnTutorialFinishedEvent()
	{
		RefreshTutorialState();
		RefreshCityModel();
	}

	private void LoadGuildMainUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildMainUI>("UI_GuildMain", OnGuildMainUILoaded);
	}

	private void LoadGuildLobbyUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildLobbyUI>("UI_GuildLobby", OnGuildLobbyUILoaded);
	}

	private void LoadWantedUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<WantedMainUI>("UI_WantedMain", OnWantedUILoaded);
	}

	private void LoadPowerTowerUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<PowerTowerUI>("UI_PowerTower", OnPowerTowerUILoaded);
	}

	private void LoadGuildBossUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildBossMainUI>("UI_GuildBossMain", OnGuildBossUILoaded);
	}

	private void LoadCrusadeEntryHintUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CrusadeEntryHintUI>("UI_CrusadeEntryHint", OnCrusadeEntryHintUILoaded);
	}

	private void OnGuildMainUILoaded(GuildMainUI ui)
	{
		if (CheckGuildOnUILoaded(ui))
		{
			ui.Setup();
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(CloseGuildScene));
			_mainUI = ui;
		}
	}

	private void OnGuildLobbyUILoaded(GuildLobbyUI ui)
	{
		if (CheckGuildOnUILoaded(ui))
		{
			ui.Setup();
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnGuildLobbyUIClosed));
			OnBuildingUILoaded();
		}
	}

	private void OnWantedUILoaded(WantedMainUI ui)
	{
		if (CheckGuildOnUILoaded(ui))
		{
			ui.Setup();
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnWantedUIClosed));
			OnBuildingUILoaded();
		}
	}

	private void OnPowerTowerUILoaded(PowerTowerUI ui)
	{
		if (CheckGuildOnUILoaded(ui))
		{
			ui.Setup();
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnPowerTowerUIClosed));
			OnBuildingUILoaded();
		}
	}

	private void OnGuildBossUILoaded(GuildBossMainUI ui)
	{
		if (CheckGuildOnUILoaded(ui))
		{
			ui.Setup();
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnGuildBossUIClosed));
			OnBuildingUILoaded();
		}
	}

	private bool CheckGuildOnUILoaded(OrangeUIBase ui)
	{
		if (!Singleton<GuildSystem>.Instance.HasGuild)
		{
			OrangeUIBase orangeUIBase = ui;
			orangeUIBase.loadedCB = (Callback)Delegate.Combine(orangeUIBase.loadedCB, (Callback)delegate
			{
				ui.OnClickCloseBtn();
				UnlockUIState();
			});
			return false;
		}
		return true;
	}

	private void OnBuildingUILoaded()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.TOGGLE_GUILD_SCENE_RENDER, false);
		_cameraPathFollower.ClearPath();
		MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(UnlockUIState);
	}

	private void OnCrusadeEntryHintUILoaded(CrusadeEntryHintUI ui)
	{
		PlaySE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup();
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
	}

	private void OnGuildLobbyUIClosed()
	{
		OnBuildingUIClosed();
	}

	private void OnGuildBossUIClosed()
	{
		OnBuildingUIClosed();
	}

	private void OnWantedUIClosed()
	{
		OnBuildingUIClosed();
	}

	private void OnPowerTowerUIClosed()
	{
		OnBuildingUIClosed();
	}

	private void OnBuildingUIClosed()
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.TOGGLE_GUILD_SCENE_RENDER, true);
		RefreshGuildScene();
	}

	private void LockUIState(Action onFinished)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Block(true);
		GuildUIHelper.LockState(onFinished);
	}

	private void UnlockUIState()
	{
		GuildUIHelper.UnlockState();
		MonoBehaviourSingleton<UIManager>.Instance.Block(false);
	}
}
