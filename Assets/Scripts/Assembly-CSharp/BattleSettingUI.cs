using OrangeAudio;
using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class BattleSettingUI : OrangeUIBase
{
	[SerializeField]
	private Image[] imgClear;

	[SerializeField]
	private OrangeText[] textClear;

	[SerializeField]
	private Sprite[] sprImgClear;

	private Color[] clearTextColor = new Color[2]
	{
		new Color(13f / 15f, 13f / 15f, 0.8862745f),
		new Color(0.95686275f, 0.654902f, 1f / 85f)
	};

	[SerializeField]
	private Slider sliderSound;

	[SerializeField]
	private OrangeText textSoundVol;

	[SerializeField]
	private Slider sliderBgm;

	[SerializeField]
	private OrangeText textBgmVol;

	[SerializeField]
	private Slider sliderVse;

	[SerializeField]
	private OrangeText textVseVol;

	[SerializeField]
	private Slider sliderUITrans;

	[SerializeField]
	private OrangeText textUITrans;

	[SerializeField]
	private Toggle[] toggleDmg;

	[SerializeField]
	private Toggle[] toggleHp;

	[SerializeField]
	private Toggle[] toggleJump;

	[SerializeField]
	private Toggle[] toggleCharge;

	[SerializeField]
	private Toggle[] toggleAimAuto;

	[SerializeField]
	private Toggle[] toggleAimLine;

	[SerializeField]
	private Toggle[] toggleAimFirst;

	[SerializeField]
	private Toggle[] toggleAimManual;

	[SerializeField]
	private Toggle[] toggleDoubleTapThrough;

	[SerializeField]
	private Toggle[] toggleDoubleTapDash;

	[SerializeField]
	private Toggle[] toggleButtonTip;

	[SerializeField]
	private ScrollRect SliderView;

	private Setting setting;

	public bool Quit { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Quit = false;
		setting = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting;
		EnableButtonTipOption(true);
		MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = true;
		InitSliderVal();
		SetSliderValueChange();
		InitToggleVal();
	}

	private void Start()
	{
		STAGE_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value))
		{
			Setup(value);
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = false;
	}

	private void Setup(STAGE_TABLE stageTable)
	{
		StageInfo value = null;
		bool flag = ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(stageTable.n_ID, out value);
		string[] stageClearMsg = ManagedSingleton<StageHelper>.Instance.GetStageClearMsg(stageTable);
		for (int i = 0; i < imgClear.Length; i++)
		{
			textClear[i].text = stageClearMsg[i];
			if (flag)
			{
				if ((value.netStageInfo.Star & (1 << i)) != 0)
				{
					imgClear[i].sprite = sprImgClear[1];
					textClear[i].color = clearTextColor[1];
				}
				else
				{
					imgClear[i].sprite = sprImgClear[0];
					textClear[i].color = clearTextColor[0];
				}
			}
			else
			{
				imgClear[i].sprite = sprImgClear[0];
				textClear[i].color = clearTextColor[0];
			}
		}
	}

	private void InitSliderVal()
	{
		sliderSound.value = setting.SoundVol;
		sliderBgm.value = setting.BgmVol;
		sliderVse.value = setting.VseVol;
		sliderUITrans.value = setting.UITrans;
		textSoundVol.text = (100f * sliderSound.value).ToString("0\\%");
		textBgmVol.text = (100f * sliderBgm.value).ToString("0\\%");
		textVseVol.text = (100f * sliderVse.value).ToString("0\\%");
		textUITrans.text = (100f * sliderUITrans.value).ToString("0\\%");
	}

	private void SetSliderValueChange()
	{
		sliderSound.onValueChanged.AddListener(delegate(float f)
		{
			Set_Volume(AudioChannelType.Sound, f);
		});
		sliderBgm.onValueChanged.AddListener(delegate(float f)
		{
			Set_Volume(AudioChannelType.BGM, f);
		});
		sliderVse.onValueChanged.AddListener(delegate(float f)
		{
			Set_Volume(AudioChannelType.Voice, f);
		});
		sliderUITrans.onValueChanged.AddListener(delegate(float f)
		{
			Set_UITrans(f);
		});
	}

	private void InitToggleVal()
	{
		for (int i = 0; i < 2; i++)
		{
			toggleDmg[i].isOn = setting.DmgVisible == i;
			toggleHp[i].isOn = setting.HpVisible == i;
			toggleJump[i].isOn = setting.JumpClassic == i;
			toggleCharge[i].isOn = setting.AutoCharge == i;
			toggleAimAuto[i].isOn = setting.AutoAim == i;
			toggleAimLine[i].isOn = setting.AimLine == i;
			toggleAimFirst[i].isOn = setting.AimFirst == i;
			toggleAimManual[i].isOn = setting.AimManual == i;
			toggleDoubleTapThrough[i].isOn = setting.DoubleTapThrough == i;
			toggleDoubleTapDash[i].isOn = setting.DoubleTapDash == i;
			toggleButtonTip[i].isOn = setting.ButtonTip == i;
		}
	}

	private void Set_UITrans(float val)
	{
		sliderUITrans.value = val;
		textUITrans.text = (100f * sliderUITrans.value).ToString("0\\%");
		Transform canvasUI = MonoBehaviourSingleton<UIManager>.Instance.CanvasUI;
		if (!(canvasUI == null))
		{
			canvasUI = MonoBehaviourSingleton<UIManager>.Instance.JoystickPanelParent;
			if (!(canvasUI == null))
			{
				canvasUI.gameObject.AddOrGetComponent<CanvasGroup>().alpha = 1f - val;
			}
		}
	}

	private void Set_Volume(AudioChannelType channelType, float val)
	{
		switch (channelType)
		{
		case AudioChannelType.BGM:
			sliderBgm.value = val;
			textBgmVol.text = (100f * sliderBgm.value).ToString("0\\%");
			MonoBehaviourSingleton<AudioManager>.Instance.SetVol(channelType, sliderBgm.value);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.BgmVol = sliderBgm.value;
			break;
		case AudioChannelType.Sound:
			sliderSound.value = val;
			textSoundVol.text = (100f * sliderSound.value).ToString("0\\%");
			MonoBehaviourSingleton<AudioManager>.Instance.SetVol(channelType, sliderSound.value);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.SoundVol = sliderSound.value;
			break;
		case AudioChannelType.Voice:
			sliderVse.value = val;
			textVseVol.text = (100f * sliderVse.value).ToString("0\\%");
			MonoBehaviourSingleton<AudioManager>.Instance.SetVol(channelType, sliderVse.value);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.VseVol = sliderVse.value;
			break;
		}
	}

	public void OnSliderPointerUPSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
	}

	public void OnSliderPointerUPVoice()
	{
		OrangeCharacter mainPlayerOC = StageUpdate.GetMainPlayerOC();
		if (mainPlayerOC != null)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Play(AudioLib.GetVoice(ref mainPlayerOC.CharacterData), 25);
		}
	}

	public void OnSliderPointerUPTrans()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
	}

	private void SaveData(bool Notify = true)
	{
		setting.BgmVol = sliderBgm.value;
		setting.SoundVol = sliderSound.value;
		setting.VseVol = sliderVse.value;
		setting.DmgVisible = ((!toggleDmg[0].isOn) ? 1 : 0);
		setting.HpVisible = ((!toggleHp[0].isOn) ? 1 : 0);
		setting.JumpClassic = ((!toggleJump[0].isOn) ? 1 : 0);
		setting.AutoCharge = ((!toggleCharge[0].isOn) ? 1 : 0);
		setting.AutoAim = ((!toggleAimAuto[0].isOn) ? 1 : 0);
		setting.AimLine = ((!toggleAimLine[0].isOn) ? 1 : 0);
		setting.AimFirst = ((!toggleAimFirst[0].isOn) ? 1 : 0);
		setting.AimManual = ((!toggleAimManual[0].isOn) ? 1 : 0);
		setting.DoubleTapThrough = ((!toggleDoubleTapThrough[0].isOn) ? 1 : 0);
		setting.DoubleTapDash = ((!toggleDoubleTapDash[0].isOn) ? 1 : 0);
		setting.UITrans = sliderUITrans.value;
		setting.ButtonTip = ((!toggleButtonTip[0].isOn) ? 1 : 0);
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting = setting;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		if (Notify)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SETTING);
		}
	}

	public void OnClickSetDefault()
	{
		OrangePlayerLocalData.SetBattleSettingDefault(ref setting);
		InitSliderVal();
		InitToggleVal();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
	}

	public void OnClickSetController()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GameInputEditor", delegate(GameInputEditor ui)
		{
			ui.Setup();
		});
	}

	public override void OnClickCloseBtn()
	{
		MonoBehaviourSingleton<InputManager>.Instance.UsingCursor = false;
		MonoBehaviourSingleton<AudioManager>.Instance.Resume(AudioChannelType.Sound);
		SaveData();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		base.OnClickCloseBtn();
	}

	public void OnClickQuit()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.SetupYesNoByKey("COMMON_TIP", "BATTLE_LEAVE_MSG", "COMMON_OK", "COMMON_CANCEL", delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
				MonoBehaviourSingleton<AudioManager>.Instance.Resume(AudioChannelType.Sound);
				MonoBehaviourSingleton<AudioManager>.Instance.StopAllExceptSE();
				Quit = true;
				SaveData(false);
				base.OnClickCloseBtn();
			});
		});
	}

	public void OnClickDmgVisible()
	{
		if (toggleDmg[0].isOn)
		{
			if (setting.DmgVisible == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.DmgVisible = 0;
			}
		}
		else if (setting.DmgVisible == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.DmgVisible = 1;
		}
	}

	public void OnClickHpVisible()
	{
		if (toggleHp[0].isOn)
		{
			if (setting.HpVisible == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.HpVisible = 0;
			}
		}
		else if (setting.HpVisible == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.HpVisible = 1;
		}
	}

	public void OnClickJumpClassic()
	{
		if (toggleJump[0].isOn)
		{
			if (setting.JumpClassic == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.JumpClassic = 0;
			}
		}
		else if (setting.JumpClassic == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.JumpClassic = 1;
		}
	}

	public void OnClickAutoCharge()
	{
		if (toggleCharge[0].isOn)
		{
			if (setting.AutoCharge == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.AutoCharge = 0;
			}
		}
		else if (setting.AutoCharge == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.AutoCharge = 1;
		}
	}

	public void OnClickAutoAim()
	{
		if (toggleAimAuto[0].isOn)
		{
			if (setting.AutoAim == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.AutoAim = 0;
			}
		}
		else if (setting.AutoAim == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.AutoAim = 1;
		}
	}

	public void OnClickAimLine()
	{
		if (toggleAimLine[0].isOn)
		{
			if (setting.AimLine == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.AimLine = 0;
			}
		}
		else if (setting.AimLine == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.AimLine = 1;
		}
	}

	public void OnClickAimFirst()
	{
		if (toggleAimFirst[0].isOn)
		{
			if (setting.AimFirst == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.AimFirst = 0;
			}
		}
		else if (setting.AimFirst == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.AimFirst = 1;
		}
	}

	public void OnClickAimManual()
	{
		if (toggleAimManual[0].isOn)
		{
			if (setting.AimManual == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.AimManual = 0;
			}
		}
		else if (setting.AimManual == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.AimManual = 1;
		}
	}

	public void OnClickDoubleTapThrough()
	{
		if (toggleDoubleTapThrough[0].isOn)
		{
			if (setting.DoubleTapThrough == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.DoubleTapThrough = 0;
			}
		}
		else if (setting.DoubleTapThrough == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.DoubleTapThrough = 1;
		}
	}

	public void OnClickDoubleTapDashh()
	{
		if (toggleDoubleTapDash[0].isOn)
		{
			if (setting.DoubleTapDash == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.DoubleTapDash = 0;
			}
		}
		else if (setting.DoubleTapDash == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.DoubleTapDash = 1;
		}
	}

	public void OnClickButtonTip()
	{
		if (toggleButtonTip[0].isOn)
		{
			if (setting.ButtonTip == 1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				setting.ButtonTip = 0;
			}
		}
		else if (setting.ButtonTip == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			setting.ButtonTip = 1;
		}
		MonoBehaviourSingleton<InputManager>.Instance.VirtualPadSystem.EnableButtonTips((setting.ButtonTip != 0) ? true : false);
	}

	private void EnableButtonTipOption(bool bEnable)
	{
		RectTransform component = SliderView.content.GetComponent<RectTransform>();
		if ((bool)component)
		{
			component.sizeDelta = new Vector2(0f, component.sizeDelta.y + 144f);
		}
		Transform parent = sliderUITrans.transform.parent;
		Transform parent2 = toggleButtonTip[0].transform.parent;
		parent2.gameObject.SetActive(true);
		Vector3 position = parent.transform.position;
		Quaternion quaternion = parent.transform.rotation;
		parent.transform.SetPositionAndRotation(parent2.transform.position, parent2.transform.rotation);
		parent2.transform.SetPositionAndRotation(position, quaternion);
	}
}
