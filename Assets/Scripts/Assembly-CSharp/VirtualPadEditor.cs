using UnityEngine;
using UnityEngine.UI;

public class VirtualPadEditor : OrangeUIBase
{
	public Slider SliderScale;

	public OrangeText MainGamePadMappingText;

	public OrangeText SubGamePadMappingText;

	private VirtualPadSystem _virtualPadSystem;

	[SerializeField]
	private Transform _padPos;

	private float oldSlider;

	private float sumSliderDis;

	public void Setup()
	{
		MonoBehaviourSingleton<InputManager>.Instance.LoadVirtualPad(1, true, InitializeVps);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
	}

	private void InitializeVps(object p_param)
	{
		_virtualPadSystem = p_param as VirtualPadSystem;
		_virtualPadSystem.transform.SetParent(_padPos, true);
		_virtualPadSystem.SliderScale = SliderScale;
		_virtualPadSystem.MainGamePadMappingText = MainGamePadMappingText;
		_virtualPadSystem.SubGamePadMappingText = SubGamePadMappingText;
		SliderScale.onValueChanged.AddListener(delegate
		{
			OnScaleChange();
		});
		_virtualPadSystem.UpdateKeyDisplay();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnSaveBtn()
	{
		cInput.scanning = false;
		_virtualPadSystem.UpdateKeyDisplay();
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			ui.alertSE = 18;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SAVE_COMPLETE"));
		});
		foreach (Transform item in _virtualPadSystem.transform)
		{
			VirtualButton component = item.GetComponent<VirtualButton>();
			VirtualAnalogStick component2 = item.GetComponent<VirtualAnalogStick>();
			Transform transform = null;
			string text = "";
			if ((bool)component)
			{
				transform = component.transform;
				text = component.KeyMapping.ToString();
			}
			else if ((bool)component2)
			{
				transform = component2.Radius.transform;
				text = "ANALOG";
			}
			if ((bool)transform)
			{
				PlayerPrefs.SetFloat("CustomizedLayout_" + text + "X", transform.localPosition.x);
				PlayerPrefs.SetFloat("CustomizedLayout_" + text + "Y", transform.localPosition.y);
				PlayerPrefs.SetFloat("CustomizedLayout_" + text + "Scale", transform.localScale.x);
			}
		}
		if (MonoBehaviourSingleton<InputManager>.Instance.IsJoystickConnected)
		{
			string[] joystickNames = Input.GetJoystickNames();
			MonoBehaviourSingleton<RogManager>.Instance.SaveJoystickSetting(joystickNames[0]);
		}
	}

	public override void OnClickCloseBtn()
	{
		cInput.scanning = false;
		_virtualPadSystem.UpdateKeyDisplay();
		_virtualPadSystem.DefaultValueDictionary.Clear();
		MonoBehaviourSingleton<InputManager>.Instance.InitializeVirtualPad();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		base.OnClickCloseBtn();
	}

	public void OnTypePad(int type)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		_virtualPadSystem.DefaultValueDictionary.Clear();
		_virtualPadSystem.DestroyVirtualPad();
		MonoBehaviourSingleton<InputManager>.Instance.LoadVirtualPad(type, true, delegate(object p_param)
		{
			InitializeVps(p_param);
			LoadPadData();
		});
	}

	public void LoadPadData()
	{
		foreach (Transform item in _virtualPadSystem.transform)
		{
			VirtualButton component = item.GetComponent<VirtualButton>();
			VirtualAnalogStick component2 = item.GetComponent<VirtualAnalogStick>();
			Transform transform = null;
			string text = "";
			if ((bool)component)
			{
				transform = component.transform;
				text = component.KeyMapping.ToString();
			}
			else if ((bool)component2)
			{
				transform = component2.Radius.transform;
				text = "ANALOG";
			}
			if ((bool)transform)
			{
				transform.localPosition = new Vector2(_virtualPadSystem.DefaultValueDictionary["CustomizedLayout_" + text + "X"], _virtualPadSystem.DefaultValueDictionary["CustomizedLayout_" + text + "Y"]);
				transform.localScale = new Vector2(_virtualPadSystem.DefaultValueDictionary["CustomizedLayout_" + text + "Scale"], _virtualPadSystem.DefaultValueDictionary["CustomizedLayout_" + text + "Scale"]);
			}
		}
		for (int i = 0; i < 19; i++)
		{
			PlayerPrefs.SetInt("PADCONF_" + (ButtonId)i, 0);
		}
	}

	public void OnDefaultBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTORE_DEFAULT_COMPLETE"));
		});
		MonoBehaviourSingleton<RogManager>.Instance.ResetJoystickSetting();
		_virtualPadSystem.DefaultValueDictionary.Clear();
		_virtualPadSystem.DestroyVirtualPad();
		MonoBehaviourSingleton<InputManager>.Instance.LoadVirtualPad(1, true, delegate(object p_param)
		{
			InitializeVps(p_param);
			LoadPadData();
		});
		cInput.scanning = false;
		cInput.ResetInputs();
		MonoBehaviourSingleton<InputManager>.Instance.IsJoystickConnected = false;
		_virtualPadSystem.UpdateKeyDisplay();
	}

	private void OnUpdateSliderValue(float value)
	{
		SliderScale.value = value;
	}

	private void OnScaleChange()
	{
		_virtualPadSystem.UpdateTargetScale(SliderScale.value);
	}

	public void OnSliderPointerDownSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
	}

	public void OnSliderSE()
	{
		sumSliderDis += SliderScale.value - oldSlider;
		if (Mathf.Abs(sumSliderDis) > 0.07f)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
			sumSliderDis = 0f;
		}
		oldSlider = SliderScale.value;
	}

	public void OnSetButton(bool isMain)
	{
		_virtualPadSystem.SetButton(isMain);
	}
}
