using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class TitleNewUI : OrangeUIBase
{
	[SerializeField]
	private Button btnStart;

	[SerializeField]
	private Text textVersion;

	[SerializeField]
	private Button btnZone;

	[SerializeField]
	private Text textZoneName;

	[SerializeField]
	private Text textPlayerID;

	[SerializeField]
	private Image[] statusIndicator;

	private bool needLoadPatch;

	private bool isNewAccount = true;

	private bool lockLogin;

	private GameServerZoneInfo gameServerZoneInfo;

	protected override void Awake()
	{
		base.Awake();
		isNewAccount = true;
		textZoneName.text = "- - - - - -";
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_PLAYER_IDENTIFY, UpdatePlayerId);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.LOGIN_CANCEL, LoginCancel);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_PLAYER_IDENTIFY, UpdatePlayerId);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.LOGIN_CANCEL, LoginCancel);
	}

	private void UpdatePlayerId()
	{
		textPlayerID.text = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DisplayPlayerId();
	}

	private void LoginCancel()
	{
		lockLogin = false;
	}

	public void Setup()
	{
		needLoadPatch = false;
		MonoBehaviourSingleton<LocateManager>.Instance.UpdatePublicIPAddress(null);
		base._EscapeEvent = EscapeEvent.CLOSE_GAME;
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", 1);
		UpdateVisualInfo();
		ManagedSingleton<ServerStatusHelper>.Instance.UpdateServerStatus(delegate
		{
			UpdateZoneInfo(!isNewAccount);
		});
	}

	private void UpdateVisualInfo()
	{
		textVersion.text = MonoBehaviourSingleton<OrangeGameManager>.Instance.AppVersion;
		isNewAccount = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.IsNewAccount();
		UpdateZoneInfo(!isNewAccount);
		textPlayerID.text = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DisplayPlayerId();
	}

	private void UpdateZoneInfo(bool active)
	{
		if (this == null || base.gameObject == null)
		{
			return;
		}
		foreach (GameServerGameInfo item in ManagedSingleton<ServerConfig>.Instance.ServerSetting.Game)
		{
			foreach (GameServerZoneInfo item2 in item.Zone)
			{
				if (item2.ID != MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChoseServiceZoneID)
				{
					continue;
				}
				gameServerZoneInfo = item2;
				if ((bool)textZoneName)
				{
					textZoneName.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetNameFromGameServerNameInfo(gameServerZoneInfo.Name);
				}
				if (statusIndicator != null && statusIndicator.Length == 3)
				{
					statusIndicator[0].gameObject.SetActive(false);
					statusIndicator[1].gameObject.SetActive(false);
					statusIndicator[2].gameObject.SetActive(false);
					if (ManagedSingleton<ServerStatusHelper>.Instance.IsZoneOffline(item2.ID))
					{
						statusIndicator[2].gameObject.SetActive(true);
					}
					else if (ManagedSingleton<ServerStatusHelper>.Instance.IsZoneOverload(item2.ID))
					{
						statusIndicator[1].gameObject.SetActive(true);
					}
					else
					{
						statusIndicator[0].gameObject.SetActive(true);
					}
				}
				break;
			}
		}
		btnZone.gameObject.SetActive(active);
	}

	public void OnClickZoneSelectUI(bool NeedSE = true)
	{
		if (lockLogin)
		{
			return;
		}
		if (NeedSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		}
		ManagedSingleton<ServerStatusHelper>.Instance.UpdateServerStatus(delegate
		{
			UpdateZoneInfo(!isNewAccount);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ServerSelect_2", delegate(ServerSelectMainUI ui)
			{
				ui._EscapeEvent = EscapeEvent.CLOSE_UI;
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateVisualInfo));
				ui.Setup(ManagedSingleton<ServerConfig>.Instance.ServerSetting);
			});
		});
	}

	public void OnClickBtnSubMenu()
	{
		if (!lockLogin)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TitleSub", delegate(TitleSubUI ui)
			{
				ui._EscapeEvent = EscapeEvent.CLOSE_UI;
				ui.closeCB = UpdateVisualInfo;
			});
		}
	}

	public void OnClickLogin()
	{
		if (!lockLogin)
		{
			lockLogin = true;
			ManagedSingleton<ServerStatusHelper>.Instance.UpdateServerStatus(delegate
			{
				UpdateZoneInfo(!isNewAccount);
				LoginHelper();
			});
		}
	}

	private void LoginHelper()
	{
		if (!MonoBehaviourSingleton<SteamManager>.Instance.TrustedAccount)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("STEAM_VAC_BANNED");
			LoginCancel();
			return;
		}
		if (needLoadPatch)
		{
			needLoadPatch = MonoBehaviourSingleton<PatchLoadHelper>.Instance.NeedLoadPatchData();
			if (needLoadPatch)
			{
				lockLogin = false;
				return;
			}
		}
		if (isNewAccount)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_SNS", delegate(SnsUI snsUI)
			{
				snsUI._EscapeEvent = EscapeEvent.CLOSE_UI;
				SnsUI snsUI2 = snsUI;
				snsUI2.closeCB = (Callback)Delegate.Combine(snsUI2.closeCB, (Callback)delegate
				{
					lockLogin = false;
					if (snsUI.IsLoginSuccess)
					{
						UpdateVisualInfo();
						ShowTermsOfUseUI(delegate
						{
							ShowAgeConfirmUI(null);
						});
					}
				});
			});
			return;
		}
		if (gameServerZoneInfo == null)
		{
			OnClickZoneSelectUI();
			lockLogin = false;
			return;
		}
		if (!ManagedSingleton<ServerStatusHelper>.Instance.IsWhitelistedUser())
		{
			if (ManagedSingleton<ServerStatusHelper>.Instance.IsZoneOffline(gameServerZoneInfo.ID))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					string str3 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP");
					string p_desc = string.Format("{0}\n{1} ~ {2}\n{3}", MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SYSTEM_SERVER_MAINTENANCE"), ManagedSingleton<ServerStatusHelper>.Instance.GetMaintenanceStartTime(), ManagedSingleton<ServerStatusHelper>.Instance.GetMaintenanceEndTime(), ManagedSingleton<ServerStatusHelper>.Instance.GetTimeZone());
					ui.SetupConfirm(str3, p_desc, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
					{
					});
				});
				lockLogin = false;
				return;
			}
			if (ManagedSingleton<ServerStatusHelper>.Instance.IsZoneOverload(gameServerZoneInfo.ID))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP");
					string str2 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("LOGIN_USER_OVER");
					ui.SetupConfirm(str, str2, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
					{
					});
				});
				lockLogin = false;
				return;
			}
		}
		if (string.IsNullOrEmpty(ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.ID))
		{
			ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.ID = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.UID;
			ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.Secret = SystemInfo.deviceUniqueIdentifier;
			ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.SourceType = AccountSourceType.Unity;
		}
		StartLogin();
	}

	private void StartLogin()
	{
		ShowTermsOfUseUI(delegate
		{
			ShowAgeConfirmUI(delegate
			{
				if (!MonoBehaviourSingleton<GameServerService>.Instance.IsSending)
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.LoginToGameService(gameServerZoneInfo.Host, delegate
					{
						if (ManagedSingleton<SNSHelper>.Instance.MyLinkData != null)
						{
							ManagedSingleton<SNSHelper>.Instance.SNSLinkReq(ManagedSingleton<SNSHelper>.Instance.MyLinkData.UserID, ManagedSingleton<SNSHelper>.Instance.MyLinkData.AccessToken, ManagedSingleton<SNSHelper>.Instance.MyLinkData.AccountSourceType, string.Empty, delegate
							{
								ManagedSingleton<SNSHelper>.Instance.MyLinkData = null;
								LoginFlowComplete();
							}, delegate
							{
								LoginFlowComplete();
							});
						}
						else
						{
							LoginFlowComplete();
						}
						MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK06);
					});
				}
			}, true);
		}, true);
	}

	private void LoginFlowComplete()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SetToDevice();
		if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.Count == 0)
		{
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("WorldView", OrangeSceneManager.LoadingType.WHITE);
			OnClickCloseBtn();
			return;
		}
		needLoadPatch = MonoBehaviourSingleton<PatchLoadHelper>.Instance.NeedLoadPatchData();
		if (needLoadPatch)
		{
			lockLogin = false;
			return;
		}
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.LOGIN_BONUS;
		MonoBehaviourSingleton<TutorialFlagChk>.Instance.SetTutorialAllFlag(delegate
		{
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.TIP);
			OnClickCloseBtn();
		});
	}

	public void ShowTermsOfUseUI(Action p_cb, bool NeedSE = false)
	{
		if (string.IsNullOrEmpty(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Locate))
		{
			if (NeedSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TermsOfUse", delegate(TermsOfUseUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					if (!string.IsNullOrEmpty(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Locate))
					{
						p_cb.CheckTargetToInvoke();
					}
					else
					{
						lockLogin = false;
					}
				});
				ui.Setup();
			});
		}
		else
		{
			p_cb.CheckTargetToInvoke();
		}
	}

	public void ShowAgeConfirmUI(Action p_cb, bool NeedSE = false)
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		p_cb.CheckTargetToInvoke();
	}
}
