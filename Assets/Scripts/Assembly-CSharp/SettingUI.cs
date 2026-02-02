#define RELEASE
using System;
using System.Collections.Generic;
using System.Text;
using CallbackDefs;
using NaughtyAttributes;
using OrangeApi;
using OrangeAudio;
using OrangeSocket;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class SettingUI : OrangeUIBase
{
	private enum SettingSelection
	{
		quality = 0,
		sound = 1,
		game = 2,
		notify = 3,
		account = 4,
		count = 5
	}

	public enum SNS_Type
	{
		st_line = 0,
		st_twitter = 1,
		st_facebook = 2
	}

	[SerializeField]
	private GameObject[] arraySelection = new GameObject[5];

	[SerializeField]
	private Transform storageRoot;

	[SerializeField]
	private Slider sliderBgm;

	[SerializeField]
	private Slider sliderSe;

	[SerializeField]
	private Slider sliderVse;

	[SerializeField]
	private Text textBgmValue;

	[SerializeField]
	private Text textSeValue;

	[SerializeField]
	private Text textVseValue;

	[SerializeField]
	private Text textAccountID;

	[SerializeField]
	private InputField inputPassword;

	[SerializeField]
	private OrangeText textInfo;

	[SerializeField]
	private Button btnInheritConfirm;

	[SerializeField]
	private Text textFpsExtra;

	[SerializeField]
	private VerticalLayoutGroup layoutGroupQuality;

	[SerializeField]
	private Button btnCopyrightJP;

	[SerializeField]
	private Button btnQuitGame;

	[SerializeField]
	private GameObject objDeleteAccountRoot;

	[SerializeField]
	private Button btnDeleteAccount;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickSE;

	private bool canPlayClickInputSE = true;

	private CHARACTER_TABLE ocTable;

	private bool isROG;

	private int frameRate = 60;

	private SettingSelection nowSetting;

	private int resolutionSelect;

	[SerializeField]
	private Toggle[] resolutionToggles;

	[SerializeField]
	private Toggle[] fpsToggle;

	[SerializeField]
	private Toggle[] swToggle;

	[SerializeField]
	private Toggle[] scrEffectToggle;

	[SerializeField]
	private Toggle[] vsyncToggle;

	[SerializeField]
	private Toggle[] fullscreenToggle;

	[SerializeField]
	private Toggle[] communityToggle;

	[SerializeField]
	private Toggle[] isNoSleepToggle;

	private bool addCB;

	[SerializeField]
	private Toggle[] notifyToggleAP;

	[SerializeField]
	private Toggle[] notifyToggleEP;

	[SerializeField]
	private Toggle[] notifyToggleResearch;

	[SerializeField]
	private Button lineBtn;

	[SerializeField]
	private Button facebookBtn;

	[SerializeField]
	private Button twitterBtn;

	[SerializeField]
	private Button appleBtn;

	[SerializeField]
	private Button steamBtn;

	[SerializeField]
	private OrangeText lineBtnText;

	[SerializeField]
	private OrangeText facebookBtnText;

	[SerializeField]
	private OrangeText twitterBtnText;

	[SerializeField]
	private OrangeText appleBtnText;

	[SerializeField]
	private OrangeText steamBtnText;

	private List<SNSLinkInfo> linkInfo;

	protected override void Awake()
	{
		base.Awake();
		GameObject[] array = arraySelection;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		isROG = DeviceHelper.IsROGPhone(out frameRate);
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.UPDATE_FULLSCREEN, FullscreenSwitch);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.UPDATE_FULLSCREEN, FullscreenSwitch);
	}

	private void InitInterface()
	{
		InitQualitySetting();
		InitVolumeSetting();
		InitNotifySetting();
		InitAccountSetting();
	}

	public void Setup()
	{
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Save));
		closeCB = (Callback)Delegate.Combine(closeCB, new Callback(Save));
		CreateNewStorageTab();
		InitInterface();
		int standbyChara = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara;
		ocTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[standbyChara];
		if (btnCopyrightJP != null)
		{
			btnCopyrightJP.gameObject.SetActive(true);
		}
		if ((bool)btnQuitGame)
		{
			btnQuitGame.gameObject.SetActive(true);
		}
		if ((bool)objDeleteAccountRoot)
		{
			objDeleteAccountRoot.SetActive(false);
		}
	}

	private void CreateNewStorageTab()
	{
		List<StorageInfo> list = new List<StorageInfo>();
		StorageInfo storageInfo = new StorageInfo("SETTING_VISUAL", false, 0, OnClickTab);
		StorageInfo storageInfo2 = new StorageInfo("SETTING_COMMON", false, 0, OnClickTab);
		StorageInfo storageInfo3 = new StorageInfo("SETTING_GAME", false, 0, OnClickTab);
		new StorageInfo("SETTING_PUSH", false, 0, OnClickTab);
		StorageInfo storageInfo4 = new StorageInfo("SETTING_ACCOUNT", false, 0, OnClickTab);
		storageInfo.Param = new object[1] { SettingSelection.quality };
		storageInfo2.Param = new object[1] { SettingSelection.sound };
		storageInfo3.Param = new object[1] { SettingSelection.game };
		storageInfo4.Param = new object[1] { SettingSelection.account };
		list.Add(storageInfo);
		list.Add(storageInfo2);
		list.Add(storageInfo3);
		list.Add(storageInfo4);
		StorageGenerator.Load("StorageComp00", list, 0, 0, storageRoot, delegate
		{
		});
	}

	private void OnClickTab(object p_param)
	{
		StorageInfo storageInfo = (StorageInfo)p_param;
		UpdateTabInfo((SettingSelection)storageInfo.Param[0]);
	}

	private void UpdateTabInfo(SettingSelection p_setting)
	{
		arraySelection[(int)nowSetting].SetActive(false);
		arraySelection[(int)p_setting].SetActive(true);
		nowSetting = p_setting;
		if (p_setting == SettingSelection.account)
		{
			if (string.IsNullOrEmpty(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode))
			{
				textAccountID.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_ACCOUNT_UNAVAILABLE");
			}
			else
			{
				textAccountID.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_ACCOUNT_INFORMATION"), MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode);
			}
			UpdateSNSButtonText();
		}
		if (p_setting == SettingSelection.quality)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_FULLSCREEN, Screen.fullScreen);
		}
	}

	public void OnClickClearCache()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Clear();
		Application.Quit();
	}

	public void OnQuitGame()
	{
		Debug.Log("Steam Quit Game!");
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MESSAGE_QUITGAME_CONFIRM"), delegate
		{
			Application.Quit();
		}, null, SystemSE.CRI_SYSTEMSE_SYS_OK17);
	}

	public void OnDeleteAccount()
	{
		if (Singleton<GuildSystem>.Instance.SelfMemberInfo != null && Singleton<GuildSystem>.Instance.SelfMemberInfo.Privilege == 1)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("DELAC_GUILDLEADER_TIP");
			return;
		}
		string p_key = "DELAC_TIP";
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_key), delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_InputPassword", delegate(InputPasswordUI uiDelPass)
			{
				uiDelPass.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				uiDelPass._EscapeEvent = EscapeEvent.CLOSE_UI;
				uiDelPass.SetupDeletePassword(delegate
				{
					uiDelPass.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
					ManagedSingleton<PlayerNetManager>.Instance.DeleteAccount(delegate(DeleteAccountRes res)
					{
						if (res.Code == 1151)
						{
							MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("DELAC_GUILDLEADER_TIP");
						}
						else
						{
							MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
							{
								ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
								ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_HOME;
								ui.SetupConfirmByKey("COMMON_TIP", "DELAC_DONE_TIP", "COMMON_OK", delegate
								{
									ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_HOME;
									MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
								});
							}, true);
						}
					});
				});
			});
		}, null, SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	private void Save()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep = true;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.AP = false;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.EP = false;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.Research = false;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		Screen.sleepTimeout = (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep ? (-2) : (-1));
		MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQChatSetShield((!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community) ? 1 : 0));
	}

	private void InitQualitySetting()
	{
		float resolutionRate = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.ResolutionRate;
		if (resolutionRate == 0.5f)
		{
			resolutionSelect = 0;
		}
		else if (resolutionRate == 0.75f)
		{
			resolutionSelect = 1;
		}
		else if (resolutionRate == 1f)
		{
			resolutionSelect = 2;
		}
		for (int i = 0; i < resolutionToggles.Length; i++)
		{
			int idx = i;
			resolutionToggles[idx].onValueChanged.RemoveAllListeners();
			resolutionToggles[idx].isOn = resolutionSelect == idx;
			resolutionToggles[idx].onValueChanged.AddListener(delegate(bool toggle)
			{
				if (toggle)
				{
					OnClickUpdateResolution(idx);
					DisplayResolutionMsg();
				}
			});
		}
		fpsToggle[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate == 30;
		fpsToggle[1].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate == 60;
		if (isROG)
		{
			fpsToggle[2].GetComponent<Canvas>().enabled = true;
			fpsToggle[2].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate == frameRate;
			if (textFpsExtra != null)
			{
				textFpsExtra.text = frameRate.ToString();
			}
		}
		else
		{
			fpsToggle[2].isOn = false;
		}
		for (int j = 0; j < fpsToggle.Length; j++)
		{
			fpsToggle[j].onValueChanged.RemoveAllListeners();
			fpsToggle[j].onValueChanged.AddListener(delegate
			{
				OnClickUpdateFrameRate();
			});
		}
		EnableVSyncFPSToggle();
		swToggle[0].onValueChanged.RemoveAllListeners();
		swToggle[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.sw;
		swToggle[0].onValueChanged.AddListener(delegate
		{
			OnClickShadow();
		});
		swToggle[1].onValueChanged.RemoveAllListeners();
		swToggle[1].isOn = !MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.sw;
		swToggle[1].onValueChanged.AddListener(delegate
		{
			OnClickShadow();
		});
		for (int k = 0; k < scrEffectToggle.Length; k++)
		{
			int idx2 = k;
			scrEffectToggle[k].onValueChanged.RemoveAllListeners();
			scrEffectToggle[k].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect == k;
			scrEffectToggle[k].onValueChanged.AddListener(delegate(bool toggle)
			{
				if (toggle)
				{
					OnClickUpdateScrEffect(idx2);
				}
			});
		}
		vsyncToggle[0].transform.parent.gameObject.SetActive(true);
		fullscreenToggle[0].transform.parent.gameObject.SetActive(true);
		if ((bool)layoutGroupQuality)
		{
			layoutGroupQuality.spacing = 25f;
		}
		vsyncToggle[0].onValueChanged.RemoveAllListeners();
		vsyncToggle[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync;
		vsyncToggle[0].onValueChanged.AddListener(delegate
		{
			OnClickVsync();
		});
		vsyncToggle[1].onValueChanged.RemoveAllListeners();
		vsyncToggle[1].isOn = !MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync;
		vsyncToggle[1].onValueChanged.AddListener(delegate
		{
			OnClickVsync();
		});
		fullscreenToggle[0].onValueChanged.RemoveAllListeners();
		fullscreenToggle[1].onValueChanged.RemoveAllListeners();
		fullscreenToggle[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.fullscreen;
		fullscreenToggle[1].isOn = !MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.fullscreen;
		fullscreenToggle[0].onValueChanged.AddListener(delegate
		{
			OnClickFullscreen();
		});
		fullscreenToggle[1].onValueChanged.AddListener(delegate
		{
			OnClickFullscreen();
		});
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.fullscreen != Screen.fullScreen)
		{
			Screen.fullScreen = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.fullscreen;
		}
		if (!Screen.fullScreen)
		{
			Vector2 windowModeResolution = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.WindowModeResolution;
			Screen.SetResolution((int)windowModeResolution.x, (int)windowModeResolution.y, false);
		}
	}

	private void EnableVSyncFPSToggle()
	{
		if (fpsToggle.Length != 4)
		{
			return;
		}
		OrangeText componentInChildren = fpsToggle[0].GetComponentInChildren<OrangeText>();
		OrangeText componentInChildren2 = fpsToggle[1].GetComponentInChildren<OrangeText>();
		OrangeText componentInChildren3 = fpsToggle[3].GetComponentInChildren<OrangeText>();
		componentInChildren.text = (Screen.currentResolution.refreshRate / 2).ToString();
		componentInChildren2.text = Screen.currentResolution.refreshRate.ToString();
		if (!fpsToggle[3])
		{
			return;
		}
		fpsToggle[2].gameObject.SetActive(false);
		fpsToggle[3].gameObject.SetActive(true);
		if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync)
		{
			fpsToggle[0].isOn = false;
			fpsToggle[1].isOn = false;
			fpsToggle[3].isOn = true;
			fpsToggle[0].interactable = false;
			fpsToggle[1].interactable = false;
			fpsToggle[3].interactable = false;
			componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 0.5f);
			componentInChildren2.color = new Color(componentInChildren2.color.r, componentInChildren2.color.g, componentInChildren2.color.b, 0.5f);
			componentInChildren3.color = new Color(componentInChildren3.color.r, componentInChildren3.color.g, componentInChildren3.color.b, 1f);
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = -1;
			return;
		}
		fpsToggle[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate == 30;
		fpsToggle[1].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate == 60;
		fpsToggle[3].isOn = false;
		fpsToggle[0].interactable = true;
		fpsToggle[1].interactable = true;
		fpsToggle[3].interactable = false;
		componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 1f);
		componentInChildren2.color = new Color(componentInChildren2.color.r, componentInChildren2.color.g, componentInChildren2.color.b, 1f);
		componentInChildren3.color = new Color(componentInChildren3.color.r, componentInChildren3.color.g, componentInChildren3.color.b, 0.5f);
		Application.targetFrameRate = 60;
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate == 30)
		{
			QualitySettings.vSyncCount = 2;
		}
		else
		{
			QualitySettings.vSyncCount = 1;
		}
	}

	public void OnClickUpdateResolution(int select)
	{
		float[] array = new float[3] { 0.5f, 0.75f, 1f };
		if (resolutionSelect != select)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
			resolutionSelect = select;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.ResolutionRate = array[select];
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenRate = array[select];
			MonoBehaviourSingleton<OrangeGameManager>.Instance.SetDesignContentScale();
		}
	}

	private void DisplayResolutionMsg()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			ui.Delay = 2f;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SETTING_RESOLUTION_TIP"));
		});
	}

	public void OnClickUpdateFrameRate()
	{
		int num = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate;
		if (fpsToggle[0].isOn)
		{
			if (num != 30)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				num = 30;
			}
		}
		else if (fpsToggle[1].isOn && num != 60)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
			num = 60;
		}
		if (isROG && fpsToggle[2].isOn && num != frameRate)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
			num = frameRate;
		}
		Application.targetFrameRate = num;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.FrameRate = num;
		if (fpsToggle[0].isOn)
		{
			Application.targetFrameRate = 60;
			QualitySettings.vSyncCount = 2;
		}
		else if (fpsToggle[1].isOn)
		{
			Application.targetFrameRate = 60;
			QualitySettings.vSyncCount = 1;
		}
		else if (fpsToggle.Length == 4 && fpsToggle[3].isOn)
		{
			Application.targetFrameRate = -1;
			QualitySettings.vSyncCount = -1;
		}
	}

	public void OnClickShadow()
	{
		if (swToggle[0].isOn)
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.sw)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.sw = true;
			}
		}
		else if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.sw)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.sw = false;
		}
	}

	public void OnClickUpdateScrEffect(int select)
	{
		float[] array = new float[3] { 0.5f, 0.75f, 1f };
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect != select)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect = select;
		}
	}

	public void OnClickVsync()
	{
		if (vsyncToggle[0].isOn)
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync = true;
			}
		}
		else if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.vsync = false;
		}
		EnableVSyncFPSToggle();
	}

	public void OnClickFullscreen()
	{
		if (fullscreenToggle[0].isOn)
		{
			if (!Screen.fullScreen)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				Screen.fullScreen = true;
			}
			if (fullscreenToggle[1].isOn)
			{
				fullscreenToggle[1].isOn = false;
			}
		}
		if (fullscreenToggle[1].isOn)
		{
			if (Screen.fullScreen)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				Screen.fullScreen = false;
			}
			if (fullscreenToggle[0].isOn)
			{
				fullscreenToggle[0].isOn = false;
			}
		}
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.fullscreen != Screen.fullScreen)
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.fullscreen = Screen.fullScreen;
		}
	}

	public void FullscreenSwitch(bool bFullscreen)
	{
		if (nowSetting == SettingSelection.quality && (fullscreenToggle[0].isOn != bFullscreen || fullscreenToggle[1].isOn == bFullscreen))
		{
			if (bFullscreen)
			{
				fullscreenToggle[0].isOn = true;
			}
			else
			{
				fullscreenToggle[1].isOn = true;
			}
		}
	}

	private void InitVolumeSetting()
	{
		sliderBgm.onValueChanged.RemoveAllListeners();
		sliderSe.onValueChanged.RemoveAllListeners();
		sliderVse.onValueChanged.RemoveAllListeners();
		sliderBgm.value = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.BgmVol;
		sliderSe.value = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SoundVol;
		sliderVse.value = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.VseVol;
		UpdateSliderBgm();
		UpdateSliderSE();
		UpdateSliderVSE();
		communityToggle[0].onValueChanged.RemoveAllListeners();
		communityToggle[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community;
		communityToggle[0].onValueChanged.AddListener(delegate
		{
			OnClickCommunity();
		});
		communityToggle[1].isOn = !MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community;
		isNoSleepToggle[0].onValueChanged.RemoveAllListeners();
		isNoSleepToggle[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep;
		isNoSleepToggle[0].onValueChanged.AddListener(delegate
		{
			OnClickScreenSleep();
		});
		isNoSleepToggle[1].isOn = !MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep;
		sliderBgm.onValueChanged.AddListener(delegate
		{
			UpdateSliderBgm();
		});
		sliderSe.onValueChanged.AddListener(delegate
		{
			UpdateSliderSE();
		});
		sliderVse.onValueChanged.AddListener(delegate
		{
			UpdateSliderVSE();
		});
		isNoSleepToggle[0].transform.parent.gameObject.SetActive(false);
	}

	public void OnSliderPointerDownSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
	}

	public void OnSliderPointerUPVoice()
	{
		string voiceID = AudioLib.GetVoice(ref ocTable);
		MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(voiceID, 3, delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Play(voiceID, 25);
		});
	}

	private void UpdateSliderBgm()
	{
		float value = sliderBgm.value;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.BgmVol = value;
		textBgmValue.text = (100f * value).ToString("0\\%");
		MonoBehaviourSingleton<AudioManager>.Instance.SetVol(AudioChannelType.BGM, value);
	}

	private void UpdateSliderSE()
	{
		float value = sliderSe.value;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SoundVol = value;
		textSeValue.text = (100f * value).ToString("0\\%");
		MonoBehaviourSingleton<AudioManager>.Instance.SetVol(AudioChannelType.Sound, value);
	}

	private void UpdateSliderVSE()
	{
		float value = sliderVse.value;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.VseVol = value;
		textVseValue.text = (100f * value).ToString("0\\%");
		MonoBehaviourSingleton<AudioManager>.Instance.SetVol(AudioChannelType.Voice, value);
	}

	public void OnClickCommunity()
	{
		if (communityToggle[0].isOn)
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community = true;
			}
		}
		else if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community = false;
		}
	}

	public void OnClickScreenSleep()
	{
		if (isNoSleepToggle[0].isOn)
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep = true;
			}
		}
		else if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.IsNoSleep = false;
		}
	}

	public void OnClickBtnVirtualPad()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GameInputEditor", delegate(GameInputEditor ui)
		{
			ui.Setup();
		});
	}

	public void OnClickBtnLanguage()
	{
		MonoBehaviourSingleton<LocalizationManager>.Instance.OpenLanguageUI();
	}

	public void OnClickOfficialPage()
	{
		Application.OpenURL(string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Homepage, MonoBehaviourSingleton<LocalizationManager>.Instance.GetOfficalLan()));
	}

	public void OnClickServiceBtn()
	{
		AREA_TABLE saveArea = ManagedSingleton<OrangeTableHelper>.Instance.GetSaveArea();
		Application.OpenURL(string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Service, saveArea.s_RULE).Replace(" ", "%20"));
	}

	public void OnClickPolicyBtn()
	{
		AREA_TABLE saveArea = ManagedSingleton<OrangeTableHelper>.Instance.GetSaveArea();
		Application.OpenURL(string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Policy, saveArea.s_PRIVACY).Replace(" ", "%20"));
	}

	public void OnClickBugReport()
	{
		Application.OpenURL(ManagedSingleton<ServerConfig>.Instance.GetCustomerUrl());
	}

	public void OnClickDeveloper()
	{
		MonoBehaviourSingleton<UIManager>.Instance.NotOpenMsgUI();
	}

	public void OnClickDefaultSetting()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RETURN_DEFAULT_CONFIRM"), delegate
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting = new Setting();
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SetToDevice();
			InitInterface();
		}, null, SystemSE.CRI_SYSTEMSE_SYS_OK05);
	}

	public void OnClickReturnTitle()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RETURN_LOGIN_CONFIRM"), delegate
		{
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
		}, null, SystemSE.CRI_SYSTEMSE_SYS_HOME, UIManager.EffectType.NONE);
	}

	public void OnClickCopyright()
	{
		Application.OpenURL(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Copyright);
	}

	private void InitNotifySetting()
	{
		notifyToggleAP[0].onValueChanged.RemoveAllListeners();
		notifyToggleAP[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.AP;
		notifyToggleAP[1].isOn = !MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.AP;
		notifyToggleAP[0].onValueChanged.AddListener(delegate(bool toggle)
		{
			notifyToggleAP[0].isOn = toggle;
			notifyToggleAP[1].isOn = !toggle;
			if (toggle != MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.AP)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.AP = toggle;
			}
		});
		notifyToggleEP[0].onValueChanged.RemoveAllListeners();
		notifyToggleEP[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.EP;
		notifyToggleEP[1].isOn = !MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.EP;
		notifyToggleEP[0].onValueChanged.AddListener(delegate(bool toggle)
		{
			notifyToggleEP[0].isOn = toggle;
			notifyToggleEP[1].isOn = !toggle;
			if (toggle != MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.EP)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.EP = toggle;
			}
		});
		notifyToggleResearch[0].onValueChanged.RemoveAllListeners();
		notifyToggleResearch[0].isOn = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.Research;
		notifyToggleResearch[1].isOn = !MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.Research;
		notifyToggleResearch[0].onValueChanged.AddListener(delegate(bool toggle)
		{
			notifyToggleResearch[0].isOn = toggle;
			notifyToggleResearch[1].isOn = !toggle;
			if (toggle != MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.Research)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSE);
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SettingNotify.Research = toggle;
			}
		});
	}

	private void InitAccountSetting()
	{
		UpdateBindInfo();
	}

	private bool IsSNSBinded(AccountSourceType sourceType)
	{
		if (linkInfo != null)
		{
			foreach (SNSLinkInfo item in linkInfo)
			{
				if (item.SourceType == (sbyte)sourceType)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UpdateBindInfo(Callback p_cb = null)
	{
		lineBtn.gameObject.SetActive(false);
		facebookBtn.gameObject.SetActive(false);
		twitterBtn.gameObject.SetActive(false);
		appleBtn.gameObject.SetActive(false);
		steamBtn.gameObject.SetActive(false);
		steamBtn.gameObject.SetActive(true);
		if (!MonoBehaviourSingleton<AppleLoginManager>.Instance.IsSupportAppleLogin)
		{
			appleBtn.gameObject.SetActive(false);
		}
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode = string.Empty;
		ManagedSingleton<SNSHelper>.Instance.RetrieveSNSLinkInfoReq(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.UID, delegate(object p_param)
		{
			linkInfo = p_param as List<SNSLinkInfo>;
			UpdateSNSButtonText();
			p_cb.CheckTargetToInvoke();
		});
	}

	private void UpdateSNSButtonText()
	{
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_BINDING");
		lineBtnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_LINE");
		facebookBtnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_FACEBOOK");
		twitterBtnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_TWITTER");
		appleBtnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_APPLE");
		steamBtnText.text = "";
		textAccountID.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_ACCOUNT_UNAVAILABLE");
		if (linkInfo == null)
		{
			return;
		}
		foreach (SNSLinkInfo item in linkInfo)
		{
			switch ((AccountSourceType)item.SourceType)
			{
			case AccountSourceType.Line:
				lineBtnText.text = str;
				break;
			case AccountSourceType.Facebook:
				facebookBtnText.text = str;
				break;
			case AccountSourceType.Twitter:
				twitterBtnText.text = str;
				break;
			case AccountSourceType.Apple:
				appleBtn.gameObject.SetActive(true);
				appleBtnText.text = str;
				break;
			case AccountSourceType.Unity:
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode = item.Name;
				textAccountID.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_ACCOUNT_INFORMATION"), MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode);
				break;
			}
		}
	}

	public void OnClickLINEBind()
	{
		if (IsSNSBinded(AccountSourceType.Line))
		{
			string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL_CONFIRM");
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					LogoutFromSNS(AccountSourceType.Line);
				});
			});
		}
		else
		{
			ManagedSingleton<LINEManager>.Instance.OnLoginRetrieveInfoSuccess = OnLINELoginRetrieveInfoSuccess;
			ManagedSingleton<LINEManager>.Instance.LoginWithInitialize();
		}
	}

	public void OnClickTwitterBind()
	{
		if (IsSNSBinded(AccountSourceType.Twitter))
		{
			string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL_CONFIRM");
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					LogoutFromSNS(AccountSourceType.Twitter);
				});
			});
		}
		else
		{
			ManagedSingleton<TwitterManager>.Instance.OnLoginRetrieveInfoSuccess = OnTwitterLoginRetrieveInfoSuccess;
			ManagedSingleton<TwitterManager>.Instance.LoginWithInitialize();
		}
	}

	public void OnClickFacebookBind()
	{
		if (IsSNSBinded(AccountSourceType.Facebook))
		{
			string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL_CONFIRM");
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					LogoutFromSNS(AccountSourceType.Facebook);
				});
			});
		}
		else
		{
			ManagedSingleton<FacebookManager>.Instance.OnLoginRetrieveInfoSuccess = OnFacebookLoginRetrieveInfoSuccess;
			ManagedSingleton<FacebookManager>.Instance.LoginWithInitialize();
		}
	}

	public void OnClickAppleBind()
	{
		if (IsSNSBinded(AccountSourceType.Apple))
		{
			string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL_CONFIRM");
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					LogoutFromSNS(AccountSourceType.Apple);
				});
			});
		}
		else
		{
			MonoBehaviourSingleton<AppleLoginManager>.Instance.OnLoginRetrieveInfoSuccess = OnAppleLoginRetrieveInfoSuccess;
			MonoBehaviourSingleton<AppleLoginManager>.Instance.LoginWithInitialize();
		}
	}

	public void OnClickSteamBind()
	{
		if (IsSNSBinded(AccountSourceType.Steam))
		{
			string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL_CONFIRM");
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					LogoutFromSNS(AccountSourceType.Steam);
				});
			});
		}
		else
		{
			OnSteamLoginRetrieveInfoSuccess();
		}
	}

	private void LogoutFromSNS(AccountSourceType sourceType)
	{
		if (sourceType == AccountSourceType.Unity)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<SNSHelper>.Instance.CancelSNSLinkReq((sbyte)sourceType, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.UID, delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				UpdateBindInfo();
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.OpenSE = SystemSE.NONE;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_CANCEL_DONE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		});
	}

	private void OnAppleLoginRetrieveInfoSuccess(params object[] p_param)
	{
		byte[] bytes = (byte[])p_param[1];
		string userId = p_param[0] as string;
		string @string = Encoding.ASCII.GetString(bytes);
		Debug.Log("Identity token in UTF8: " + Encoding.UTF8.GetString(bytes));
		AppleUser player = new AppleUser(userId, @string);
		OnAppleLoginRetrieveInfoSuccessHelper(player);
	}

	private void OnAppleLoginRetrieveInfoSuccessHelper(AppleUser player)
	{
		if (player != null)
		{
			string userId = player.UserId;
		}
		ManagedSingleton<SNSHelper>.Instance.SNSLinkReq(player.UserId, player.IdentityToken, 7, string.Empty, delegate
		{
			UpdateBindInfo();
			DisplayResultMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_BINDING_DONE"), SystemSE.CRI_SYSTEMSE_SYS_OK17);
		}, delegate(object p_param)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)p_param, false);
		});
	}

	private void OnLINELoginRetrieveInfoSuccess(LINEUser player)
	{
		string arg = "DisplayName";
		if (player != null)
		{
			arg = player.DisplayName;
		}
		string msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_LINE_CONFIRM"), arg);
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.NONE;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_LINE"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				ManagedSingleton<SNSHelper>.Instance.SNSLinkReq(player.UserID, player.AccessToken, 5, string.Empty, delegate
				{
					UpdateBindInfo();
					DisplayResultMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_BINDING_DONE"), SystemSE.CRI_SYSTEMSE_SYS_OK17);
				}, delegate(object p_param)
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)p_param, false);
					ManagedSingleton<LINEManager>.Instance.Logout();
				});
			});
		});
	}

	private void OnTwitterLoginRetrieveInfoSuccess(TwitterUser player)
	{
		string arg = "NickName";
		if (player != null)
		{
			arg = player.Nickname;
		}
		string msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_TWITTER_CONFIRM"), arg);
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.NONE;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_TWITTER"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				ManagedSingleton<SNSHelper>.Instance.SNSLinkReq(player.AccessToken, player.AccessSecret, 4, string.Empty, delegate
				{
					UpdateBindInfo();
					DisplayResultMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_BINDING_DONE"), SystemSE.CRI_SYSTEMSE_SYS_OK17);
				}, delegate(object p_param)
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)p_param, false);
					ManagedSingleton<TwitterManager>.Instance.Logout();
				});
			});
		});
	}

	private void OnFacebookLoginRetrieveInfoSuccess(FacebookUser player)
	{
		string arg = "NickName";
		if (player != null)
		{
			arg = player.Nickname;
		}
		string msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_FACEBOOK_CONFIRM"), arg);
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.NONE;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_FACEBOOK"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				ManagedSingleton<SNSHelper>.Instance.SNSLinkReq(player.Identify, player.AccessToken, 3, string.Empty, delegate
				{
					UpdateBindInfo();
					DisplayResultMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_BINDING_DONE"), SystemSE.CRI_SYSTEMSE_SYS_OK17);
				}, delegate(object p_param)
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)p_param, false);
					ManagedSingleton<FacebookManager>.Instance.Logout();
				});
			});
		});
	}

	private void OnSteamLoginRetrieveInfoSuccess()
	{
		string steamID = MonoBehaviourSingleton<SteamManager>.Instance.GetUserSteamID();
		string ticket = MonoBehaviourSingleton<SteamManager>.Instance.GetUserTicket();
		string arg = SteamClient.Name;
		string msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_STEAM_CONFIRM"), arg);
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.NONE;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_STEAM"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				ManagedSingleton<SNSHelper>.Instance.SNSLinkReq(steamID, ticket, 8, string.Empty, delegate
				{
					UpdateBindInfo();
					DisplayResultMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACCOUNT_SNS_BINDING_DONE"), SystemSE.CRI_SYSTEMSE_SYS_OK17);
				}, delegate(object p_param)
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)p_param, false);
				});
			});
		});
	}

    [Obsolete]
    public void GetAccountInheritCode(string newPassword, CallbackObj p_cb = null)
	{
		ManagedSingleton<SNSHelper>.Instance.GetInheritCode(newPassword, delegate(object p_param)
		{
			p_cb.CheckTargetToInvoke((string)p_param);
		});
	}

	private void ChangePasswordHelper()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_InputPassword", delegate(InputPasswordUI uiChangePass)
		{
			uiChangePass.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			uiChangePass.SetupChangePassword(delegate(object[] p_params)
			{
				uiChangePass.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				string password = (string)p_params[0];
				string text = (string)p_params[1];
				Debug.Log("New Password = " + text);
				ManagedSingleton<SNSHelper>.Instance.SNSLinkReq(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode, password, 1, text, delegate
				{
					DisplayResultMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_PASSWORD_DONE"));
				}, delegate
				{
					DisplayResultMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_PASSWORD_FAILED"), SystemSE.CRI_SYSTEMSE_SYS_ERROR);
				});
			});
		});
	}

	private void DisplayResultMsg(string msg, SystemSE openSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = openSE;
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
			});
		});
	}

	public void OnClickAccountInherit()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		if (string.IsNullOrEmpty(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_InputPassword", delegate(InputPasswordUI uiNewPass)
			{
				uiNewPass.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				uiNewPass._EscapeEvent = EscapeEvent.CLOSE_UI;
				uiNewPass.SetupNewPassword(delegate(object[] p_params)
				{
					uiNewPass.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
					string newPassword = (string)p_params[0];
					GetAccountInheritCode(newPassword, delegate(object p_param)
					{
						string text = (string)p_param;
						MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.InheritCode = text;
						textAccountID.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_ACCOUNT_INFORMATION"), text);
						DisplayResultMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FINISH_SETTING"));
						UpdateBindInfo();
					});
				});
			});
		}
		else
		{
			ChangePasswordHelper();
		}
	}

	public void OnClickInputArea()
	{
		if (canPlayClickInputSE && inputPassword.interactable)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			canPlayClickInputSE = false;
		}
	}

	public void OnEndEditInput()
	{
		canPlayClickInputSE = true;
	}
}
