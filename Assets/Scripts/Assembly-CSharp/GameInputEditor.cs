using System;
using UnityEngine;
using UnityEngine.UI;

public class GameInputEditor : OrangeUIBase
{
	[Serializable]
	private class MappingOptionData
	{
		public ButtonId buttonID;

		public Text textKeyboard;

		public Text textGamepad;

		public Image imageGamepad;
	}

	[SerializeField]
	private GameObject selectionFrame;

	[SerializeField]
	private MappingOptionData[] mappingOptions;

	private CursorSettingUI cursorSettingPanel;

	private ButtonId currentMappingButton;

	private int inputType;

	private float blockTimer;

	public void Setup()
	{
		MonoBehaviourSingleton<InputManager>.Instance.gamepadMappingEvent += GamepadChangeEventHandler;
		cInput.OnKeyChanged += OnKeyChangedEventHandler;
		UpdateAllKeysDisplay();
		inputType = 1;
		MonoBehaviourSingleton<InputManager>.Instance.ManualGamepadCheck();
	}

	private void Update()
	{
		if (blockTimer > 0f)
		{
			blockTimer -= Time.deltaTime;
		}
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<InputManager>.Instance.gamepadMappingEvent -= GamepadChangeEventHandler;
		cInput.OnKeyChanged -= OnKeyChangedEventHandler;
	}

	public void OnDefaultBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTORE_DEFAULT_COMPLETE"));
		});
		cInput.scanning = false;
		currentMappingButton = ButtonId.NONE;
		MonoBehaviourSingleton<InputManager>.Instance.SetupDefaultAll(false, false, MonoBehaviourSingleton<GamepadManager>.Instance.GamepadNameToGamepadType(MonoBehaviourSingleton<InputManager>.Instance.currentGamepad));
		UpdateAllKeysDisplay();
		MonoBehaviourSingleton<InputManager>.Instance.InputKeyChangeHandle();
	}

	public void OnClickCursorSetting()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CursorEditor", delegate(CursorSettingUI ui)
		{
			cursorSettingPanel = ui;
			ui.Setup();
		});
	}

	public override void OnClickCloseBtn()
	{
		cInput.scanning = false;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		if (cursorSettingPanel != null)
		{
			cursorSettingPanel.OnClickCloseBtn();
		}
		base.OnClickCloseBtn();
	}

	public void OnSetKeyForKeyboard(int id)
	{
		if (IsSetKeyAllowed())
		{
			inputType = 1;
			ButtonId id2 = currentMappingButton;
			currentMappingButton = (ButtonId)id;
			cInput.ChangeKey(currentMappingButton.ToString(), 1, false, false, false, false, true);
			UpdateKeyDisplay(id2);
			UpdateKeyDisplay(currentMappingButton);
		}
	}

	public void OnSetKeyForGamepad(int id)
	{
		if (IsSetKeyAllowed())
		{
			inputType = 2;
			ButtonId id2 = currentMappingButton;
			currentMappingButton = (ButtonId)id;
			cInput.ChangeKey(currentMappingButton.ToString(), 2, false, false, true, true, false);
			UpdateKeyDisplay(id2);
			UpdateKeyDisplay(currentMappingButton);
		}
	}

	private int FindButtonIndex(int id)
	{
		for (int i = 0; i < mappingOptions.Length; i++)
		{
			if (mappingOptions[i].buttonID == (ButtonId)id)
			{
				return i;
			}
		}
		return -1;
	}

	private bool IsSetKeyAllowed()
	{
		return blockTimer <= 0f;
	}

	private void UpdateKeyDisplay(ButtonId id)
	{
		for (int i = 0; i < mappingOptions.Length; i++)
		{
			if (mappingOptions[i].buttonID.Equals(id))
			{
				UpdateKeyContent(mappingOptions[i]);
			}
		}
	}

	private void UpdateAllKeysDisplay()
	{
		for (int i = 0; i < mappingOptions.Length; i++)
		{
			UpdateKeyContent(mappingOptions[i]);
		}
	}

	private void UpdateKeyContent(MappingOptionData option)
	{
		option.textKeyboard.text = cInput.GetText(option.buttonID.ToString(), 1);
		Sprite buttonIcon = MonoBehaviourSingleton<InputManager>.Instance.GetButtonIcon(option.buttonID);
		if (buttonIcon != null)
		{
			option.imageGamepad.gameObject.SetActive(true);
			option.imageGamepad.sprite = buttonIcon;
			option.textGamepad.gameObject.SetActive(false);
		}
		else
		{
			option.textGamepad.gameObject.SetActive(true);
			option.textGamepad.text = cInput.GetText(option.buttonID.ToString(), 2);
			option.imageGamepad.gameObject.SetActive(false);
		}
		if (currentMappingButton != 0)
		{
			int num = FindButtonIndex((int)currentMappingButton);
			if (num >= 0)
			{
				selectionFrame.SetActive(true);
				MappingOptionData mappingOptionData = mappingOptions[num];
				if (inputType == 1)
				{
					selectionFrame.transform.position = mappingOptionData.textKeyboard.transform.parent.position;
				}
				else if (inputType == 2)
				{
					selectionFrame.transform.position = mappingOptionData.textGamepad.transform.parent.position;
				}
			}
		}
		else
		{
			selectionFrame.SetActive(false);
		}
	}

	private void GamepadChangeEventHandler(GamepadType gamepad)
	{
		UpdateAllKeysDisplay();
	}

	private void OnKeyChangedEventHandler()
	{
		if (currentMappingButton == ButtonId.NONE)
		{
			return;
		}
		ButtonId buttonId = currentMappingButton;
		currentMappingButton = ButtonId.NONE;
		string messageKey = string.Empty;
		string action = buttonId.ToString();
		string text = cInput.GetText(action, inputType);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (inputType == 1)
		{
			if (text.Equals("LeftWindows") || text.Equals("RightWindows") || text.Equals("LeftCommand") || text.Equals("RightCommand") || text.Equals("Print") || text.Equals("Numlock") || text.Equals("SysReq") || text.Equals("ScrollLock"))
			{
				messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP_INVALID");
			}
			if (text.Equals("Return"))
			{
				messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
				cInput.ChangeKey(ButtonId.START.ToString(), "Return", cInput.GetText(ButtonId.START.ToString(), 2));
			}
			else if (text.Equals("Keypad8"))
			{
				messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
				cInput.ChangeKey(ButtonId.AIM_UP.ToString(), "Keypad8", cInput.GetText(ButtonId.AIM_UP.ToString(), 2));
			}
			else if (text.Equals("Keypad2"))
			{
				messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
				cInput.ChangeKey(ButtonId.AIM_DOWN.ToString(), "Keypad2", cInput.GetText(ButtonId.AIM_DOWN.ToString(), 2));
			}
			else if (text.Equals("Keypad4"))
			{
				messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
				cInput.ChangeKey(ButtonId.AIM_LEFT.ToString(), "Keypad4", cInput.GetText(ButtonId.AIM_LEFT.ToString(), 2));
			}
			else if (text.Equals("Keypad6"))
			{
				messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
				cInput.ChangeKey(ButtonId.AIM_RIGHT.ToString(), "Keypad6", cInput.GetText(ButtonId.AIM_RIGHT.ToString(), 2));
			}
			if (!string.IsNullOrEmpty(messageKey))
			{
				cInput.ChangeKey(action, "None", cInput.GetText(action, 2));
			}
		}
		else
		{
			text = MonoBehaviourSingleton<GamepadManager>.Instance.RebuildJoystickName(text);
			for (int i = 1; i < 19; i++)
			{
				ButtonId buttonId2 = (ButtonId)i;
				string text2 = cInput.GetText(buttonId2.ToString(), 2);
				if (text.Equals(text2) && buttonId != (ButtonId)i)
				{
					buttonId2 = (ButtonId)i;
					string action2 = buttonId2.ToString();
					cInput.ChangeKey(action2, cInput.GetText(action2, 1), "None");
				}
			}
			if (!string.IsNullOrEmpty(MonoBehaviourSingleton<InputManager>.Instance.currentGamepad))
			{
				string empty = string.Empty;
				GamepadType gamepadType = MonoBehaviourSingleton<GamepadManager>.Instance.GamepadNameToGamepadType(MonoBehaviourSingleton<InputManager>.Instance.currentGamepad);
				if ((empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.Start)).Equals(text))
				{
					messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
					cInput.ChangeKey(ButtonId.START.ToString(), cInput.GetText(ButtonId.START.ToString(), 1), empty);
				}
				else if ((empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.Option)).Equals(text))
				{
					messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
					cInput.ChangeKey(ButtonId.OPTION.ToString(), cInput.GetText(ButtonId.OPTION.ToString(), 1), empty);
				}
				else if ((empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.AnalogR_Up)).Equals(text))
				{
					messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
					cInput.ChangeKey(ButtonId.AIM_UP.ToString(), cInput.GetText(ButtonId.AIM_UP.ToString(), 1), empty);
				}
				else if ((empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.AnalogR_Down)).Equals(text))
				{
					messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
					cInput.ChangeKey(ButtonId.AIM_DOWN.ToString(), cInput.GetText(ButtonId.AIM_DOWN.ToString(), 1), empty);
				}
				else if ((empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.AnalogR_Left)).Equals(text))
				{
					messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
					cInput.ChangeKey(ButtonId.AIM_LEFT.ToString(), cInput.GetText(ButtonId.AIM_LEFT.ToString(), 1), empty);
				}
				else if ((empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.AnalogR_Right)).Equals(text))
				{
					messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP");
					cInput.ChangeKey(ButtonId.AIM_RIGHT.ToString(), cInput.GetText(ButtonId.AIM_RIGHT.ToString(), 1), empty);
				}
				else if ((empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.AnalogL)).Equals(text) || (empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.AnalogR)).Equals(text) || (empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.Home)).Equals(text) || (empty = MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepadType, GamepadKey.Special)).Equals(text))
				{
					messageKey = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("KEYCONFIG_TIP_INVALID");
				}
				if (!string.IsNullOrEmpty(messageKey))
				{
					cInput.ChangeKey(action, cInput.GetText(action, 1), "None");
				}
			}
		}
		if (!string.IsNullOrEmpty(messageKey))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
			{
				ui.Setup(messageKey);
			});
		}
		UpdateAllKeysDisplay();
		MonoBehaviourSingleton<InputManager>.Instance.InputKeyChangeHandle();
		blockTimer = 0.5f;
	}
}
