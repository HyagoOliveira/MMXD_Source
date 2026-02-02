#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using DragonBones;
using Facebook.Unity;
using OrangeApi;
using StageLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using enums;

internal class OrangeGameManager : MonoBehaviourSingleton<OrangeGameManager>
{
	public const string ORANGE_L10N_KEY_LANGUAGE = "ORANGE_L10N_KEY_LANGUAGE";

	public const string ORANGE_SETTING_SE_VOLUME = "ORANGE_SETTING_SE_VOLUME";

	public const float RESOLUTION_LOW = 0.5f;

	public const float RESOLUTION_MEDIUM = 0.75f;

	public const float RESOLUTION_HIGH = 1f;

	public const int DESIGN_WIDTH = 1920;

	public const int DESIGN_HEIGHT = 1080;

	private bool _IsGamePause;

	private bool _bLastGamePause;

	private int nWaitFrameCount = -1;

	private Callback LoginToGameServiceCB;

	private int RequestMax;

	private int RequestNow;

	public ServerInfo serverInfo = new ServerInfo();

	private bool resolutionInit;

	private long pauseTimeTicks;

	public string AppVersion
	{
		get
		{
			string text = "";
			int num = OrangeDataReader.Instance[CapDataReader.CONST_CONVERT_VERSION_KEY];
			if (num != 0)
			{
				text = num.ToString();
				text = text.Insert(Math.Max(0, text.Length - 4), ".");
				text = text.Insert(Math.Max(0, text.Length - 7), ".");
			}
			string aPP_VERSION = ManagedSingleton<ServerConfig>.Instance.APP_VERSION;
			return string.Format("Ver.{0} (Source.{1}{2:00}{3:00}.{4})", aPP_VERSION, ManagedSingleton<ServerConfig>.Instance.PatchVer.ToString().PadRight(3, '0'), (GetDeviceType() == enums.DeviceType.IOS) ? ApiCommon.ProtocolVersionIOS : ApiCommon.ProtocolVersionAndroid, SocketCommon.ProtocolVersion, text);
		}
	}

	public bool IsLvUp { get; set; }

	public bool bLastGamePause
	{
		get
		{
			return _bLastGamePause;
		}
	}

	public bool IsGamePause
	{
		get
		{
			return _IsGamePause;
		}
		set
		{
			Debug.Log("[Start Pause Game] NowTime:" + Time.timeSinceLevelLoad);
			if (_bLastGamePause != value)
			{
				_bLastGamePause = value;
				nWaitFrameCount = 2;
			}
		}
	}

	public bool IsLogin { get; private set; }

	public string WebToken { get; private set; }

	public long ServerUnixTimeNowUTC
	{
		get
		{
			return CapUtility.DateToUnixTime(ServerTimeNowUTC);
		}
	}

	public DateTime ServerTimeNowUTC
	{
		get
		{
			return DateTime.UtcNow.AddSeconds(serverInfo.ClientDifferTime);
		}
	}

	public long ServerUnixTimeNowLocale
	{
		get
		{
			return CapUtility.DateToUnixTime(ServerTimeNowLocale);
		}
	}

	public DateTime ServerTimeNowLocale
	{
		get
		{
			return DateTime.UtcNow.AddHours(serverInfo.TimeZone).AddSeconds(serverInfo.ClientDifferTime);
		}
	}

	public int ScreenWidth
	{
		get
		{
			return Screen.width;
		}
	}

	public int ScreenHeight
	{
		get
		{
			return Screen.height;
		}
	}

	public float ScreenRate { get; set; }

	private void LateUpdate()
	{
		if (nWaitFrameCount < 0)
		{
			return;
		}
		nWaitFrameCount--;
		if (nWaitFrameCount != -1)
		{
			return;
		}
		if (_IsGamePause != _bLastGamePause)
		{
			_IsGamePause = _bLastGamePause;
			StageUpdate.LockAllAnimator(_bLastGamePause);
			if (_bLastGamePause)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.GAME_PAUSE, true);
				OrangeTimerManager.PauseAll();
			}
			else
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.GAME_PAUSE, false);
				OrangeTimerManager.ResumeAll();
			}
		}
		MonoBehaviourSingleton<UpdateManager>.Instance.Pause = _IsGamePause;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.isPause = _IsGamePause;
		Debug.Log("[Pause Game Success] NowTime:" + Time.timeSinceLevelLoad);
	}

	public void GetPatchVersion(Callback p_cb)
	{
		MonoBehaviourSingleton<GlobalServerService>.Instance.SendRequest(new RetrievePatchVersionReq(), delegate(RetrievePatchVersionRes res)
		{
			switch (GetDeviceType())
			{
			default:
				ManagedSingleton<ServerConfig>.Instance.PatchVer = res.IOS;
				break;
			case enums.DeviceType.Android:
			case enums.DeviceType.Standalone:
				ManagedSingleton<ServerConfig>.Instance.PatchVer = res.Android;
				break;
			}
			if (MonoBehaviourSingleton<SteamManager>.Instance.AppID != res.Standalone)
			{
				Application.Quit();
			}
			else
			{
				p_cb.CheckTargetToInvoke();
			}
		});
	}

	private bool ReloadOrangeData()
	{
		if (!(MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "hometop"))
		{
			return GetServerDataCRC() != OrangeDataReader.Instance.GetOrangeDataCRC();
		}
		return true;
	}

	private bool ReloadOrangeExData()
	{
		if (!(MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "hometop"))
		{
			return GetServerExDataCRC() != OrangeDataReader.Instance.GetExOrangeDataCRC();
		}
		return true;
	}

	private IEnumerator ReDownloadOrangeData(Callback p_cb)
	{
		if (ReloadOrangeData())
		{
			yield return OrangeDataReader.Instance.LoadDesignsData();
		}
		if (ReloadOrangeExData())
		{
			yield return OrangeDataReader.Instance.LoadOperationData(ManagedSingleton<PlayerNetManager>.Instance.listAllowExTable);
		}
		yield return null;
		p_cb.CheckTargetToInvoke();
	}

	public void LoginToGameService(string serverUrl, Callback p_cb)
	{
		SingletonManager.Reset();
		MonoBehaviourSingleton<CCSocketClient>.Instance.SetSyncNetworkFrequency(100);
		Clean();
		ManagedSingleton<PlayerNetManager>.Instance.LoginToGameService(serverUrl, GetDeviceType(), delegate(LoginToServerRes res)
		{
			if (res.Code == 1000)
			{
				LoginToGameServiceCB = p_cb;
				serverInfo.DataCRC = res.DataCRC;
				serverInfo.ExDataCRC = res.ExDataCRC;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.CurrentPlayerID = res.PlayerID;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
				MonoBehaviourSingleton<GlobalServerService>.Instance.ServiceToken = res.Token;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_PLAYER_IDENTIFY);
				SetServerTimeInfo(res);
				ManagedSingleton<PlayerHelper>.Instance.SetUseCheatPlugin(res);
				ManagedSingleton<PlayerNetManager>.Instance.RetrievePlayerInfo(RetrievePlayerInfoCallback);
			}
			else
			{
				string key = "LOGIN_FAILED";
				if (res.Code == 1051)
				{
					key = "ACCOUNT_BAN";
				}
				else if (res.Code == 1058)
				{
					key = "LOGIN_USER_OVER";
				}
				LoginToGameServiceCB = null;
				ShowMessageAndReturnTitle(key);
			}
		});
	}

	public void Clean()
	{
		RequestNow = 0;
		serverInfo = new ServerInfo();
		IsLogin = false;
	}

	private void RetrievePlayerInfoCallback(RetrievePlayerInfoRes res)
	{
		if (res.Code == 10050)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_InputPlayerName", delegate(InputNameUI ui)
			{
				ui.Setup(delegate(object name)
				{
					ManagedSingleton<PlayerNetManager>.Instance.CreateNewPlayer((string)name, delegate
					{
						MonoBehaviourSingleton<KochavaEventManager>.Instance.SendEvent_NewAccount();
						ManagedSingleton<PlayerNetManager>.Instance.RetrievePlayerInfo(RetrievePlayerInfoCallback);
					});
				});
			});
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOGIN_CANCEL);
		}
		else if (res.Code == 10000)
		{
			StartRequestServerData();
		}
		else
		{
			ShowErrorMsg((Code)res.Code, false);
		}
	}

	private void OnGetCheckGuildStateEvent()
	{
		Debug.Log("[OrangeGameManager] OnGetCheckGuildStateEvent");
		CheckLoginRequestComplete();
	}

	private void StartRequestServerData()
	{
		List<Callback> list = new List<Callback>();
		list.Add(delegate
		{
			ManagedSingleton<PlayerNetManager>.Instance.LoginRetrieveCollector1Req(ManagedSingleton<ServerStatusHelper>.Instance.IsSendDeviceInfo ? GetDeviceInfo() : null, CheckServerDataRequestComplete);
		});
		list.Add(delegate
		{
			ManagedSingleton<PlayerNetManager>.Instance.LoginRetrieveCollector2Req(CheckServerDataRequestComplete);
		});
		list.Add(delegate
		{
			ManagedSingleton<PlayerNetManager>.Instance.LoginRetrieveCollector3Req(CheckServerDataRequestComplete);
		});
		list.Add(delegate
		{
			ManagedSingleton<PlayerNetManager>.Instance.LoginRetrieveCollector4Req(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.IDFA, "", CheckServerDataRequestComplete);
		});
		RequestNow = 0;
		RequestMax = list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			list[i]();
		}
	}

	private void CheckServerDataRequestComplete()
	{
		RequestNow++;
		if (RequestNow >= RequestMax)
		{
			StartLogin();
		}
	}

	private void StartLogin()
	{
		List<Callback> list = new List<Callback>();
		list.Add(delegate
		{
			LoginRetrieveCollectorClientReq(CheckLoginRequestComplete);
		});
		Singleton<GuildSystem>.Instance.OnGetCheckGuildStateOnceEvent += OnGetCheckGuildStateEvent;
		list.Add(delegate
		{
			Singleton<GuildSystem>.Instance.ReqCheckGuildState();
		});
		RequestNow = 0;
		RequestMax = list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			list[i]();
		}
	}

	private void LoginRetrieveCollectorClientReq(Callback p_cb)
	{
		p_cb.CheckTargetToInvoke();
	}

	private void CheckLoginRequestComplete()
	{
		RequestNow++;
		if (RequestNow >= RequestMax)
		{
			Debug.Log("CheckLoginRequestComplete!!");
			StartCoroutine(ReDownloadOrangeData(new Callback(CompleteRequestDownload)));
		}
	}

	public void CommunityLoginCallbackEvent()
	{
		IsLogin = true;
		if (LoginToGameServiceCB != null)
		{
			LoginToGameServiceCB.CheckTargetToInvoke();
			LoginToGameServiceCB = null;
		}
	}

	public enums.DeviceType GetDeviceType()
	{
		return enums.DeviceType.Standalone;
	}

	public void WeaponWield(int p_weaponID, WeaponWieldType p_wieldPart, Callback p_cb, bool notifyUpdateRenderWeapon = true)
	{
		if (p_wieldPart == WeaponWieldType.MainWeapon && p_weaponID == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("WEAPONWIELD_NO_EMPTY"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		int originalMainId = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		ManagedSingleton<PlayerNetManager>.Instance.WeaponWieldReq(p_weaponID, p_wieldPart, delegate(WeaponWieldRes res)
		{
			if (notifyUpdateRenderWeapon && originalMainId != res.PlayerInfo.MainWeaponID)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_WEAPON, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID);
			}
			if (p_cb != null)
			{
				p_cb();
			}
		});
	}

	public void CharacterStandby(int p_characterId, Callback p_cb, bool notifyUpdateRenderCharacter = true)
	{
		ManagedSingleton<PlayerNetManager>.Instance.CharacterStandbyReq(p_characterId, delegate
		{
			if (notifyUpdateRenderCharacter)
			{
				CharacterInfo characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
				SKIN_TABLE p_param = null;
				if (characterInfo.netInfo.Skin > 0)
				{
					p_param = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[characterInfo.netInfo.Skin];
				}
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_CHARACTER, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara], ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID], p_param);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

    [Obsolete]
    public void StageEndReq(StageEndReq tStageEndReq, CallbackObj p_cb)
	{
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsStageVaild(tStageEndReq.StageID))
		{
			MonoBehaviourSingleton<ACTkManager>.Instance.SetDetected();
			return;
		}
		int nowLv = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		ManagedSingleton<PlayerNetManager>.Instance.StageEndReq(tStageEndReq, delegate(StageEndRes obj)
		{
			ManagedSingleton<StageHelper>.Instance.ListAchievedMissionID.Clear();
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() > nowLv)
			{
				IsLvUp = true;
			}
			StageEndRes stageEndRes;
			if ((stageEndRes = obj) != null && stageEndRes.Code == 11100)
			{
				MonoBehaviourSingleton<SteamManager>.Instance.TriggerStagePassed(tStageEndReq.StageID, tStageEndReq.Star);
				MonoBehaviourSingleton<SteamManager>.Instance.TriggerDeepElementGot(ManagedSingleton<MissionHelper>.Instance.GetTotalSecretStageCount());
			}
			p_cb.CheckTargetToInvoke(obj);
		});
	}

    [Obsolete]
    public void StageSweepReq(int p_stageID, int p_sweepCount, CallbackObj p_cb = null)
	{
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsStageVaild(p_stageID))
		{
			MonoBehaviourSingleton<ACTkManager>.Instance.SetDetected();
			return;
		}
		int nowLv = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		ManagedSingleton<PlayerNetManager>.Instance.StageSweepReq(p_stageID, p_sweepCount, delegate(NetRewardsEntity res)
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() > nowLv)
			{
				IsLvUp = true;
			}
			p_cb.CheckTargetToInvoke(res);
		}, delegate(StageSweepRes res)
		{
			if (res.Code == 11200)
			{
				MonoBehaviourSingleton<SteamManager>.Instance.TriggerStagePassed(p_stageID, 7);
				MonoBehaviourSingleton<SteamManager>.Instance.TriggerDeepElementGot(ManagedSingleton<MissionHelper>.Instance.GetTotalSecretStageCount());
			}
		});
	}

    [Obsolete]
    public void ReceiveMissionRewardReq(List<int> p_missionID, CallbackObj p_cb)
	{
		int nowLv = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		ManagedSingleton<PlayerNetManager>.Instance.ReceiveMissionRewardReq(p_missionID.ToList(), delegate(List<NetRewardInfo> obj)
		{
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() > nowLv)
			{
				IsLvUp = true;
			}
			p_cb.CheckTargetToInvoke(obj);
		});
	}

    [Obsolete]
    public void ReceiveMissionRewardReq(int p_missionID, CallbackObj p_cb)
	{
		ReceiveMissionRewardReq(new List<int> { p_missionID }, p_cb);
	}

	public void RetrieveResetTime(Callback p_cb = null)
	{
		if (!IsLogin || TurtorialUI.IsTutorialing())
		{
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveResetTimeReq(delegate(RetrieveResetTimeRes res)
		{
			UpdateCurrentServerTime(res.CurrentTime);
			bool num = IsPassedResetDate(ServerUnixTimeNowUTC, ResetRule.DailyReset);
			if (res.DayResetTime != null)
			{
				serverInfo.DailyResetInfo = res.DayResetTime;
			}
			if (res.WeekResetTime != null)
			{
				serverInfo.WeeklyResetInfo = res.WeekResetTime;
			}
			if (res.MonthResetTime != null)
			{
				serverInfo.MonthlyResetInfo = res.MonthResetTime;
			}
			WebToken = res.WebToken;
			if (num)
			{
				p_cb = null;
				Clean();
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.SetupConfirmByKey("COMMON_TIP", "DAY_ALREADY_CHANGE", "COMMON_OK", delegate
					{
						LoginToGameService(ManagedSingleton<ServerConfig>.Instance.GetPreviousSelectedServerUrl(), delegate
						{
							ManagedSingleton<MissionHelper>.Instance.ResetMonthlyActivityCounterID();
							MonoBehaviourSingleton<GameServerService>.Instance.DayChange = true;
							MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
						});
					});
				}, true, true);
			}
			else
			{
				p_cb.CheckTargetToInvoke();
			}
		}, 0);
	}

	private void SetServerTimeInfo(LoginToServerRes res)
	{
		serverInfo.DailyResetTime = res.DailyResetTime;
		serverInfo.TimeZone = res.TimeZone;
		if (res.DayResetTime != null)
		{
			serverInfo.DailyResetInfo = res.DayResetTime;
		}
		if (res.WeekResetTime != null)
		{
			serverInfo.WeeklyResetInfo = res.WeekResetTime;
		}
		if (res.MonthResetTime != null)
		{
			serverInfo.MonthlyResetInfo = res.MonthResetTime;
		}
		UpdateCurrentServerTime(res.CurrentTime);
		ManagedSingleton<OrangeTableHelper>.Instance.ServerTimeZone = res.TimeZone;
	}

	public void UpdateCurrentServerTime(int p_currentTime)
	{
		serverInfo.ClientDifferTime = 0L;
		serverInfo.ClientDifferTime = p_currentTime - ServerUnixTimeNowUTC;
		if (Math.Abs(serverInfo.ClientDifferTime) < 10)
		{
			serverInfo.ClientDifferTime = 0L;
		}
	}

	public uint GetServerDataCRC()
	{
		if (serverInfo != null)
		{
			return serverInfo.DataCRC;
		}
		return 0u;
	}

	public uint GetServerExDataCRC()
	{
		if (serverInfo != null)
		{
			return serverInfo.ExDataCRC;
		}
		return 0u;
	}

	public bool IsPassedResetDate(long checkTime, ResetRule rule)
	{
		if (serverInfo.DailyResetInfo == null || serverInfo.WeeklyResetInfo == null || serverInfo.MonthlyResetInfo == null)
		{
			return false;
		}
		switch (rule)
		{
		case ResetRule.DailyReset:
			if (checkTime < serverInfo.DailyResetInfo.PreResetTime || checkTime > serverInfo.DailyResetInfo.CurrentResetTime)
			{
				return true;
			}
			break;
		case ResetRule.WeeklyReset:
			if (checkTime < serverInfo.WeeklyResetInfo.PreResetTime || checkTime > serverInfo.WeeklyResetInfo.CurrentResetTime)
			{
				return true;
			}
			break;
		case ResetRule.MonthlyReset:
			if (checkTime < serverInfo.MonthlyResetInfo.PreResetTime || checkTime > serverInfo.MonthlyResetInfo.CurrentResetTime)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public long GetNextDay()
	{
		return serverInfo.DailyResetInfo.CurrentResetTime;
	}

	public long GetNextWeek()
	{
		return serverInfo.WeeklyResetInfo.CurrentResetTime;
	}

	public long GetNextMonth()
	{
		return serverInfo.MonthlyResetInfo.CurrentResetTime;
	}

	public void InitResolution()
	{
		Debug.LogFormat("Screen Width:{0} , Screen Height:{1}", ScreenWidth, ScreenHeight);
		MonoBehaviourSingleton<UIManager>.Instance.SafeAreaRect = Screen.safeArea;
		Debug.LogFormat("SafeArea:{0}", MonoBehaviourSingleton<UIManager>.Instance.SafeAreaRect.ToString());
		ScreenRate = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.ResolutionRate;
		resolutionInit = true;
		SetDesignContentScale();
	}

	public void SetDesignContentScale()
	{
		if (resolutionInit)
		{
			int num = ScreenWidth;
			int num2 = ScreenHeight;
			if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.IsBattleScene)
			{
				num = Mathf.CeilToInt((float)ScreenWidth * ScreenRate);
				num2 = Mathf.CeilToInt((float)ScreenHeight * ScreenRate);
			}
			Debug.Log(string.Format("Set Screen Resolution Width:{0} , Height:{1}", num, num2));
			Screen.SetResolution(num, num2, Screen.fullScreen);
		}
	}

	public void ShowTipDialog(string p_tip, float delay = 0.5f)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			ui.Delay = delay;
			ui.Setup(p_tip, true);
		});
	}

	public void ShowTipDialog(string p_tip, int alertSE, SystemSE closeSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			ui.alertSE = alertSE;
			ui.CloseSE = closeSE;
			ui.Setup(p_tip, true);
		});
	}

	public void ShowTipDialogByKey(string p_tipKey, float delay = 0.5f)
	{
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_tipKey);
		ShowTipDialog(str, delay);
	}

	public void ShowErrorMsg(Code p_code, bool returnTitle = true)
	{
		Debug.Log(p_code);
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GAME_ERROR_MSG_DESC"), (int)p_code, "");
			bool ret = returnTitle;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			if (returnTitle)
			{
				text += MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GAME_ERROR_MSG_RETURN");
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_HOME;
			}
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), text, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
				if (ret)
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_HOME;
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
					MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
					{
						MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
					});
				}
			});
		}, true);
	}

	public void ShowConfirmMsg(string p_key, Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupConfirmByKey("COMMON_TIP", p_key, "COMMON_OK", p_cb);
		}, true);
	}

	public void ShowMessageAndReturnTitle(string key)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.SetupConfirmByKey("COMMON_TIP", key, "COMMON_OK", delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
				MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
				{
					MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
				});
			});
		}, true, true);
	}

	public void ShowCommonMsg(string p_msg, Callback p_cbYes, Callback p_cbNo, SystemSE YesSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL, UIManager.EffectType eftClose = UIManager.EffectType.EXPAND)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.effectTypeClose = eftClose;
			ui.YesSE = YesSE;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), p_msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), p_cbYes, p_cbNo);
		}, true);
	}

	public void ShowRetryMsg(string descKey, Callback p_cbYes, Callback p_cbNo)
	{
		CommonUI commonUI = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<CommonUI>("UI_CommonMsg", true, true);
		commonUI.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
		commonUI.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		commonUI.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		commonUI.SetupYesNoByKey("NETWORK_NOT_REACHABLE_TITLE", descKey, "COMMON_YES", "COMMON_NO", delegate
		{
			p_cbYes.CheckTargetToInvoke();
		}, p_cbNo);
	}

	public void ShowRetryMsg(Callback p_cbYes, Callback p_cbNo)
	{
		CommonUI commonUI = MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUISync<CommonUI>("UI_CommonMsg", true, true);
		commonUI.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
		commonUI.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		commonUI.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		commonUI.SetupYesNoByKey("NETWORK_NOT_REACHABLE_TITLE", "NETWORK_NOT_REACHABLE_DESC_1", "COMMON_YES", "COMMON_NO", delegate
		{
			p_cbYes.CheckTargetToInvoke();
		}, p_cbNo);
	}

	public void AddAPEPToRewardList(ref List<NetRewardInfo> rewardList, int APCount, int EPCount)
	{
		if (APCount > 0)
		{
			rewardList.Add(new NetRewardInfo
			{
				RewardType = 1,
				RewardID = OrangeConst.ITEMID_AP,
				Amount = APCount
			});
		}
		if (EPCount > 0)
		{
			rewardList.Add(new NetRewardInfo
			{
				RewardType = 1,
				RewardID = OrangeConst.ITEMID_EVENTAP,
				Amount = EPCount
			});
		}
	}

	public void ShowGetAPEPDialog(int APCount, int EPCount)
	{
		List<NetRewardInfo> rewardList = new List<NetRewardInfo>();
		if (APCount > 0)
		{
			rewardList.Add(new NetRewardInfo
			{
				RewardType = 1,
				RewardID = OrangeConst.ITEMID_AP,
				Amount = APCount
			});
		}
		if (EPCount > 0)
		{
			rewardList.Add(new NetRewardInfo
			{
				RewardType = 1,
				RewardID = OrangeConst.ITEMID_EVENTAP,
				Amount = EPCount
			});
		}
		if (rewardList.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(rewardList);
			});
		}
	}

	public void BackSplashAction()
	{
		Screen.SetResolution(ScreenWidth, ScreenHeight, Screen.fullScreen);
		LeanTween.cancelAll();
		UnityFactory.factory.Clear();
		BaseObject.ClearPool(null);
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if ((gameObject.hideFlags & HideFlags.HideInHierarchy) != HideFlags.HideInHierarchy && !gameObject.GetComponent<EventManager>() && !gameObject.GetComponent<SignalDispatcher>() && !gameObject.GetComponent<LeanTween>() && !gameObject.GetComponent<RogManager>() && !gameObject.GetComponent<FacebookGameObject>() && !gameObject.GetComponent<Kochava>() && gameObject.scene.isLoaded)
			{
				gameObject.transform.SetParentNull();
			}
		}
		MonoBehaviourSingleton<EventManager>.Instance.DetachAllEvent();
		MonoBehaviourSingleton<SignalDispatcher>.Instance.Clean();
		SceneManager.LoadScene("splash", LoadSceneMode.Single);
	}

	public int[] GetRewardSpritePath(NetRewardInfo netGachaRewardInfo, ref string bundlePath, ref string assetPath, ref int rare)
	{
		int[] array = new int[2] { netGachaRewardInfo.RewardType, 0 };
		if (netGachaRewardInfo.Amount > 0)
		{
			switch ((RewardType)(short)array[0])
			{
			case RewardType.Item:
			{
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[netGachaRewardInfo.RewardID];
				bundlePath = AssetBundleScriptableObject.Instance.GetIconItem(iTEM_TABLE.s_ICON);
				assetPath = iTEM_TABLE.s_ICON;
				rare = iTEM_TABLE.n_RARE;
				array[1] = iTEM_TABLE.n_TYPE;
				break;
			}
			case RewardType.Weapon:
			{
				WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netGachaRewardInfo.RewardID];
				bundlePath = AssetBundleScriptableObject.Instance.m_iconWeapon;
				assetPath = wEAPON_TABLE.s_ICON;
				rare = wEAPON_TABLE.n_RARITY;
				break;
			}
			case RewardType.Character:
			{
				CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[netGachaRewardInfo.RewardID];
				assetPath = "icon_" + cHARACTER_TABLE.s_ICON;
				bundlePath = AssetBundleScriptableObject.Instance.GetIconCharacter(assetPath);
				rare = cHARACTER_TABLE.n_RARITY;
				break;
			}
			case RewardType.Equipment:
			{
				EQUIP_TABLE eQUIP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[netGachaRewardInfo.RewardID];
				bundlePath = AssetBundleScriptableObject.Instance.m_iconEquip;
				assetPath = eQUIP_TABLE.s_ICON;
				rare = eQUIP_TABLE.n_RARE;
				break;
			}
			case RewardType.Chip:
			{
				DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[netGachaRewardInfo.RewardID];
				bundlePath = AssetBundleScriptableObject.Instance.m_iconChip;
				assetPath = dISC_TABLE.s_ICON;
				rare = dISC_TABLE.n_RARITY;
				break;
			}
			case RewardType.Card:
			{
				CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[netGachaRewardInfo.RewardID];
				bundlePath = AssetBundleScriptableObject.Instance.m_iconCard;
				assetPath = cARD_TABLE.s_ICON;
				rare = cARD_TABLE.n_RARITY;
				break;
			}
			}
		}
		return array;
	}

	public bool GetRewardSpritePath(GACHA_TABLE gachaTable, out string bundlePath, out string assetPath)
	{
		switch ((RewardType)(short)gachaTable.n_REWARD_TYPE)
		{
		case RewardType.Item:
		{
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[gachaTable.n_REWARD_ID];
			bundlePath = AssetBundleScriptableObject.Instance.GetIconItem(iTEM_TABLE.s_ICON);
			assetPath = iTEM_TABLE.s_ICON;
			return true;
		}
		case RewardType.Weapon:
		{
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[gachaTable.n_REWARD_ID];
			bundlePath = AssetBundleScriptableObject.Instance.m_iconWeapon;
			assetPath = wEAPON_TABLE.s_ICON;
			return true;
		}
		case RewardType.Character:
		{
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[gachaTable.n_REWARD_ID];
			assetPath = "icon_" + cHARACTER_TABLE.s_ICON;
			bundlePath = AssetBundleScriptableObject.Instance.GetIconCharacter(assetPath);
			return true;
		}
		case RewardType.Equipment:
		{
			EQUIP_TABLE eQUIP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[gachaTable.n_REWARD_ID];
			bundlePath = AssetBundleScriptableObject.Instance.m_iconEquip;
			assetPath = eQUIP_TABLE.s_ICON;
			return true;
		}
		case RewardType.Chip:
		{
			DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[gachaTable.n_REWARD_ID];
			bundlePath = AssetBundleScriptableObject.Instance.m_iconChip;
			assetPath = dISC_TABLE.s_ICON;
			return true;
		}
		case RewardType.Card:
		{
			CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[gachaTable.n_REWARD_ID];
			bundlePath = AssetBundleScriptableObject.Instance.m_iconCard;
			assetPath = cARD_TABLE.s_ICON;
			return true;
		}
		default:
			bundlePath = string.Empty;
			assetPath = string.Empty;
			return false;
		}
	}

	public float GetRenderTextureRate()
	{
		return 1f;
	}

	public NetDeviceInfo GetDeviceInfo()
	{
		try
		{
			return new NetDeviceInfo
			{
				DeviceModel = SystemInfo.deviceModel,
				DeviceUniqueId = SystemInfo.deviceUniqueIdentifier,
				GraphicsDeviceName = SystemInfo.graphicsDeviceName,
				GraphicsMemorySize = SystemInfo.graphicsMemorySize,
				SystemMemorySize = SystemInfo.systemMemorySize,
				OperatingSystem = SystemInfo.operatingSystem
			};
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
			return null;
		}
	}

	public void DisplayLvPerform(Callback p_cb = null)
	{
		if (!IsLvUp)
		{
			p_cb.CheckTargetToInvoke();
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_lvup", delegate(LvupUI ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, p_cb);
			IsLvUp = false;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Play();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.PLAYER_LEVEL_UP);
		});
	}

	public void DisplayPowerupPerform()
	{
		int currentBattlePower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower();
		int localBattlePower = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.BattlePowerMax;
		if (localBattlePower < currentBattlePower)
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.BattlePowerMax = currentBattlePower;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_powerup", delegate(PowerupUI ui)
			{
				ui.Play(currentBattlePower, currentBattlePower - localBattlePower);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
			});
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_BATTLE_POWER, currentBattlePower);
	}

	public RenderTextureFormat GetHDRTextureFormat()
	{
		RenderTextureFormat result = RenderTextureFormat.ARGBHalf;
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			return result;
		}
		return RenderTextureFormat.Default;
	}

	public void Quit()
	{
		Debug.Log("Quit");
		Application.Quit();
	}

	private void Awake()
	{
		resolutionInit = false;
		IsLvUp = false;
		ScreenRate = 1f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			pauseTimeTicks = DateTime.UtcNow.Ticks;
		}
		else
		{
			if (Math.Abs(DateTime.UtcNow.Ticks - pauseTimeTicks) > 5400000000L)
			{
				RetrieveResetTime();
			}
			SetDesignContentScale();
		}
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnApplicationPause(paused);
	}

    private void CompleteRequestDownload()
	{
		if (this.GetServerDataCRC() != OrangeDataReader.Instance.GetOrangeDataCRC() || this.GetServerExDataCRC() != OrangeDataReader.Instance.GetExOrangeDataCRC())

        {
        global::Debug.LogError(string.Format("Planner Client {0} / Server {1}, Operation Client {2} / Server {3}!", new object[]
        {
                OrangeDataReader.Instance.GetOrangeDataCRC(),
                this.GetServerDataCRC(),
                OrangeDataReader.Instance.GetExOrangeDataCRC(),
                this.GetServerExDataCRC()
        }));
        MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", delegate (CommonUI ui)
        {
            ui.SetupConfirmByKey("COMMON_TIP", "SYSTEM_PATCHVER_CHANGED", "COMMON_OK", delegate
            {
                Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.PATCH_CHANGE);
                OrangeDataReader.Instance.DeleteTableAll();
                MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_HOME);
                MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
                {
                    MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
                });
            });
        }, true, false);
        return;
    }
		if (this.ReloadOrangeExData())

        {
        OrangeDataReader.Instance.LoadExtData(ManagedSingleton<PlayerNetManager>.Instance.RawExtData);
    }
		if (this.ReloadOrangeData())

        {
        OrangeConst.ConstInit();
        ManagedSingleton<OrangeDataManager>.Instance.Reset();
        ManagedSingleton<OrangeDataManager>.Instance.Initialize();
        ManagedSingleton<OrangeTextDataManager>.Instance.Reset();
        ManagedSingleton<OrangeTextDataManager>.Instance.Initialize();
        MonoBehaviourSingleton<LocalizationManager>.Instance.LoadOrangeTextTable();
        ManagedSingleton<OrangeTableHelper>.Instance.StageVaildInit();
    }
    MonoBehaviourSingleton<OrangeCommunityManager>.Instance.CommunityServerLogout();
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnConnectedEvent += this.CommunityLoginCallbackEvent;
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RegistConnectedCall();
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.CommunityServerLogin();
	}
}
