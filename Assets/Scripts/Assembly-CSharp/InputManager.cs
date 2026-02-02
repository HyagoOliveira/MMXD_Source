#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using InControl;
using UnityEngine;

public class InputManager : MonoBehaviourSingleton<InputManager>, IManagedUpdateBehavior
{
	private const string CURRENT_GAMEPAD = "CURRENT_GAMEPAD";

	public List<VirtualButton> TouchChain = new List<VirtualButton>();

	private readonly int DeadZoneLX = 100;

	private readonly int DeadZoneLY = 400;

	private readonly int DeadZoneR = 400;

	public bool ForceDisplayAnalog;

	private bool _initializedMapping;

	public int[] ButtonHash;

	private static readonly string Horizontal = "Horizontal";

	private static readonly string Vertical = "Vertical";

	private static readonly string Horizontal2 = "Horizontal2";

	private static readonly string Vertical2 = "Vertical2";

	public readonly int HorizontalHash = Horizontal.GetHashCode();

	public readonly int VerticalHash = Vertical.GetHashCode();

	public readonly int HorizontalHash2 = Horizontal2.GetHashCode();

	public readonly int VerticalHash2 = Vertical2.GetHashCode();

	private bool isUsingCursor;

	private readonly ButtonId[] _buttonStatusCacheID = new ButtonId[1] { ButtonId.SHOOT };

	private int[] _buttonStatusCache;

	private InputInfo currentInputInfo;

	public VirtualPadSystem VirtualPadSystem { get; private set; }

	public string currentGamepad { get; private set; }

	public bool UsingCursor
	{
		get
		{
			return isUsingCursor;
		}
		set
		{
			if (isUsingCursor != value)
			{
				isUsingCursor = value;
				MonoBehaviourSingleton<CursorController>.Instance.IsEnable = isUsingCursor;
			}
		}
	}

	public bool IsJoystickConnected { get; set; }

	public event Action inputKeyChangeEvent;

	public event Action<GamepadType> gamepadMappingEvent;

	public event Action gamepadDetachedEvent;

	private void Awake()
	{
		cInput.Init();
	}

	private void Start()
	{
		IsJoystickConnected = false;
		ButtonHash = new int[19];
		for (int i = 0; i < 19; i++)
		{
			int[] buttonHash = ButtonHash;
			int num = i;
			ButtonId buttonId = (ButtonId)i;
			buttonHash[num] = buttonId.ToString().GetHashCode();
		}
		_buttonStatusCache = new int[_buttonStatusCacheID.Length];
		base.gameObject.name = typeof(InputManager).Name;
		base.transform.localPosition = new Vector3(10000f, 10000f);
		UpdateMapping();
		InvokeRepeating("UpdateJoystickStatus", 0.1f, 5f);
		InitializeInputSettingForPC();
		MonoBehaviourSingleton<CursorController>.Instance.IsEnable = true;
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	public void UpdateMapping()
	{
		InitializeDefaultKeys();
		cInput.SetAxis(Horizontal, "LEFT", "RIGHT");
		cInput.SetAxis(Vertical, "UP", "DOWN");
		cInput.SetAxis(Horizontal2, "AIM_LEFT", "AIM_RIGHT");
		cInput.SetAxis(Vertical2, "AIM_UP", "AIM_DOWN");
		cInput.SetAxisDeadzone(Horizontal, 0.2f);
		cInput.SetAxisDeadzone(Vertical, 0.6f);
		cInput.SetAxisDeadzone(Horizontal2, 0.4f);
		cInput.SetAxisDeadzone(Vertical2, 0.4f);
		_initializedMapping = true;
	}

	private void UpdateButtonStatus(ButtonId btnID)
	{
		if (currentInputInfo == null || btnID > ButtonId.MAX_BUTTON_ID || btnID < ButtonId.NONE)
		{
			return;
		}
		ButtonStatus buttonStatus = (ButtonStatus)currentInputInfo._buttonStatus[(int)btnID];
		int btnStatusCacheVal = GetBtnStatusCacheVal(btnID);
		bool flag = (btnID == ButtonId.UP && currentInputInfo._analogStickValue[0].y > DeadZoneLY) || (btnID == ButtonId.DOWN && currentInputInfo._analogStickValue[0].y < -DeadZoneLY) || (btnID == ButtonId.LEFT && currentInputInfo._analogStickValue[0].x < -DeadZoneLX) || (btnID == ButtonId.RIGHT && currentInputInfo._analogStickValue[0].x > DeadZoneLX);
		if (!flag)
		{
			VirtualButton virtualButton = GetVirtualButton(btnID);
			if (virtualButton != null)
			{
				flag = virtualButton.GetButtonStatus();
				bool activeSelf = virtualButton.gameObject.activeSelf;
				bool isFixed = virtualButton.isFixed;
			}
		}
		if (UsingCursor)
		{
			switch (buttonStatus)
			{
			case ButtonStatus.NONE:
			case ButtonStatus.RELEASED:
				buttonStatus = ButtonStatus.NONE;
				break;
			case ButtonStatus.PRESSED:
			case ButtonStatus.HELD:
				buttonStatus = ButtonStatus.RELEASED;
				break;
			}
			currentInputInfo._pressTimer[(int)btnID] = 0f;
			currentInputInfo._buttonStatus[(int)btnID] = (int)buttonStatus;
			return;
		}
		if (GetKey(ButtonHash[(int)btnID]) || flag || btnStatusCacheVal != 0)
		{
			switch (buttonStatus)
			{
			case ButtonStatus.NONE:
			case ButtonStatus.RELEASED:
				buttonStatus = ButtonStatus.PRESSED;
				break;
			case ButtonStatus.PRESSED:
			case ButtonStatus.HELD:
				buttonStatus = ButtonStatus.HELD;
				currentInputInfo._pressTimer[(int)btnID] += 1f;
				break;
			}
		}
		else
		{
			switch (buttonStatus)
			{
			case ButtonStatus.NONE:
			case ButtonStatus.RELEASED:
				buttonStatus = ButtonStatus.NONE;
				break;
			case ButtonStatus.PRESSED:
			case ButtonStatus.HELD:
				buttonStatus = ButtonStatus.RELEASED;
				break;
			}
			currentInputInfo._pressTimer[(int)btnID] = 0f;
		}
		currentInputInfo._buttonStatus[(int)btnID] = (int)buttonStatus;
	}

	private void UpdateButtonStatusCache(int idx)
	{
		ButtonId buttonId = _buttonStatusCacheID[idx];
		ButtonStatus buttonStatus = (ButtonStatus)_buttonStatusCache[idx];
		bool flag = true;
		VirtualButton virtualButton = GetVirtualButton(buttonId);
		if ((bool)virtualButton)
		{
			flag = virtualButton.gameObject.activeSelf;
		}
		if ((flag || virtualButton.isFixed) && cInput.GetKey(ButtonHash[(int)buttonId]) && (buttonStatus == ButtonStatus.NONE || buttonStatus == ButtonStatus.RELEASED))
		{
			buttonStatus = ButtonStatus.PRESSED;
			_buttonStatusCache[idx] = (int)buttonStatus;
		}
	}

	private int GetBtnStatusCacheVal(ButtonId buttonId)
	{
		for (int i = 0; i < _buttonStatusCacheID.Length; i++)
		{
			if (buttonId == _buttonStatusCacheID[i])
			{
				return _buttonStatusCache[i];
			}
		}
		return 0;
	}

	private void ClearButtonStatusCache()
	{
		for (int i = 0; i < _buttonStatusCacheID.Length; i++)
		{
			_buttonStatusCache[i] = 0;
		}
	}

	public float GetAxis(int hashCode)
	{
		float axisDeadzone = cInput.GetAxisDeadzone(hashCode);
		float axisRaw = cInput.GetAxisRaw(hashCode);
		if (Mathf.Abs(axisRaw) > axisDeadzone)
		{
			return axisRaw;
		}
		return 0f;
	}

	public bool GetKey(int hashCode)
	{
		if (ButtonHash[1] == hashCode || ButtonHash[2] == hashCode || ButtonHash[3] == hashCode || ButtonHash[4] == hashCode)
		{
			return false;
		}
		return cInput.GetKey(hashCode);
	}

	public void InitializeDefaultKeys()
	{
		CheckAndSetDefaultKey(ButtonId.UP.ToString(), "UpArrow");
		CheckAndSetDefaultKey(ButtonId.DOWN.ToString(), "DownArrow");
		CheckAndSetDefaultKey(ButtonId.LEFT.ToString(), "LeftArrow");
		CheckAndSetDefaultKey(ButtonId.RIGHT.ToString(), "RightArrow");
		CheckAndSetDefaultKey(ButtonId.SHOOT.ToString(), "A");
		CheckAndSetDefaultKey(ButtonId.JUMP.ToString(), "S");
		CheckAndSetDefaultKey(ButtonId.DASH.ToString(), "D");
		CheckAndSetDefaultKey(ButtonId.SKILL0.ToString(), "Q");
		CheckAndSetDefaultKey(ButtonId.SKILL1.ToString(), "E");
		CheckAndSetDefaultKey(ButtonId.FS_SKILL.ToString(), "F");
		CheckAndSetDefaultKey(ButtonId.CHIP_SWITCH.ToString(), "C");
		CheckAndSetDefaultKey(ButtonId.START.ToString(), "Return");
		CheckAndSetDefaultKey(ButtonId.SELECT.ToString(), "R");
		CheckAndSetDefaultKey(ButtonId.OPTION.ToString(), "Escape");
		CheckAndSetDefaultKey(ButtonId.AIM_UP.ToString(), "Keypad8");
		CheckAndSetDefaultKey(ButtonId.AIM_DOWN.ToString(), "Keypad2");
		CheckAndSetDefaultKey(ButtonId.AIM_LEFT.ToString(), "Keypad4");
		CheckAndSetDefaultKey(ButtonId.AIM_RIGHT.ToString(), "Keypad6");
		CheckAndSetDefaultKey(ButtonId.NONE.ToString(), "None");
		CheckAndSetDefaultKey(ButtonId.MAX_BUTTON_ID.ToString(), "None");
	}

	private void CheckAndSetDefaultKey(string buttonID, string key)
	{
		if (!cInput.IsKeyDefined(buttonID))
		{
			cInput.SetKey(buttonID, key, string.Empty);
		}
	}

	private void InitializeInputSettingForPC()
	{
		new GameObject("InControl").AddComponent<InControlManager>().dontDestroyOnLoad = true;
		currentGamepad = PlayerPrefs.GetString("CURRENT_GAMEPAD");
		GamepadType gamepad = MonoBehaviourSingleton<GamepadManager>.Instance.GamepadNameToGamepadType(currentGamepad);
		if (!string.IsNullOrEmpty(currentGamepad))
		{
			SetupDefaultAll(true, true);
		}
		else
		{
			SetupDefaultAll(true, false, gamepad);
		}
		InControl.InputManager.OnActiveDeviceChanged += GamepadChange;
		InControl.InputManager.OnDeviceDetached += GamepadDetached;
	}

	public void InputKeyChangeHandle()
	{
		Action action = this.inputKeyChangeEvent;
		if (action != null)
		{
			action();
		}
	}

	public Sprite GetButtonIcon(ButtonId buttonID)
	{
		GamepadType gamepadType = MonoBehaviourSingleton<GamepadManager>.Instance.GamepadNameToGamepadType(currentGamepad);
		return MonoBehaviourSingleton<GamepadManager>.Instance.GetGamepadKeyIcon(gamepadType, cInput.GetText(buttonID.ToString(), 2));
	}

	public void GamepadChange(InputDevice device)
	{
		if (device.DeviceClass != InputDeviceClass.Controller)
		{
			return;
		}
		string text = device.Name;
		if (text == "PlayStation 4 Controller")
		{
			text = ((!(Mathf.Abs(Input.GetAxis("PS4 Gamepad Wired Detect") * 1000f) > 0f)) ? "PlayStation 4 Controller Wireless" : "PlayStation 4 Controller Wired");
		}
		if (!string.IsNullOrEmpty(currentGamepad))
		{
			if (!currentGamepad.Equals(text))
			{
				GamepadType source = MonoBehaviourSingleton<GamepadManager>.Instance.GamepadNameToGamepadType(currentGamepad);
				GamepadType target = MonoBehaviourSingleton<GamepadManager>.Instance.GamepadNameToGamepadType(text);
				StartGamepadMapping(source, target);
			}
		}
		else
		{
			SetupDefaultAll(true, false, MonoBehaviourSingleton<GamepadManager>.Instance.GamepadNameToGamepadType(text));
		}
		currentGamepad = text;
		PlayerPrefs.SetString("CURRENT_GAMEPAD", currentGamepad);
		Action<GamepadType> action = this.gamepadMappingEvent;
		if (action != null)
		{
			action(MonoBehaviourSingleton<GamepadManager>.Instance.GamepadNameToGamepadType(currentGamepad));
		}
	}

	private void GamepadDetached(InputDevice inputDevice)
	{
		if (inputDevice.DeviceClass != InputDeviceClass.Controller)
		{
			return;
		}
		if (VirtualPadSystem != null)
		{
			Action action = this.gamepadDetachedEvent;
			if (action != null)
			{
				action();
			}
		}
		Debug.Log("Gamepad Disconnected");
	}

	private void StartGamepadMapping(GamepadType source, GamepadType target)
	{
		int num = 19;
		for (int i = 0; i < num; i++)
		{
			ButtonId buttonId = (ButtonId)i;
			string action = buttonId.ToString();
			string text = cInput.GetText(action, 2);
			cInput.ChangeKey(action, cInput.GetText(action, 1), MonoBehaviourSingleton<GamepadManager>.Instance.GetMappingName(source, target, text));
		}
	}

	public void SetupDefaultAll(bool keepKeyboard, bool keepGamepad, GamepadType gamepad = GamepadType.Unknown)
	{
		string action = ButtonId.UP.ToString();
		string action2 = ButtonId.DOWN.ToString();
		string action3 = ButtonId.LEFT.ToString();
		string action4 = ButtonId.RIGHT.ToString();
		string action5 = ButtonId.AIM_UP.ToString();
		string action6 = ButtonId.AIM_DOWN.ToString();
		string action7 = ButtonId.AIM_LEFT.ToString();
		string action8 = ButtonId.AIM_RIGHT.ToString();
		string action9 = ButtonId.SHOOT.ToString();
		string action10 = ButtonId.JUMP.ToString();
		string action11 = ButtonId.DASH.ToString();
		string action12 = ButtonId.SKILL0.ToString();
		string action13 = ButtonId.SKILL1.ToString();
		string action14 = ButtonId.FS_SKILL.ToString();
		string action15 = ButtonId.START.ToString();
		string action16 = ButtonId.OPTION.ToString();
		string action17 = ButtonId.SELECT.ToString();
		string action18 = ButtonId.CHIP_SWITCH.ToString();
		cInput.ChangeKey(action, keepKeyboard ? cInput.GetText(action, 1) : "UpArrow", keepGamepad ? cInput.GetText(action, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.AnalogL_Up));
		cInput.ChangeKey(action2, keepKeyboard ? cInput.GetText(action2, 1) : "DownArrow", keepGamepad ? cInput.GetText(action2, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.AnalogL_Down));
		cInput.ChangeKey(action3, keepKeyboard ? cInput.GetText(action3, 1) : "LeftArrow", keepGamepad ? cInput.GetText(action3, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.AnalogL_Left));
		cInput.ChangeKey(action4, keepKeyboard ? cInput.GetText(action4, 1) : "RightArrow", keepGamepad ? cInput.GetText(action4, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.AnalogL_Right));
		cInput.ChangeKey(action5, keepKeyboard ? cInput.GetText(action5, 1) : "Keypad8", keepGamepad ? cInput.GetText(action5, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.AnalogR_Up));
		cInput.ChangeKey(action6, keepKeyboard ? cInput.GetText(action6, 1) : "Keypad2", keepGamepad ? cInput.GetText(action6, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.AnalogR_Down));
		cInput.ChangeKey(action7, keepKeyboard ? cInput.GetText(action7, 1) : "Keypad4", keepGamepad ? cInput.GetText(action7, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.AnalogR_Left));
		cInput.ChangeKey(action8, keepKeyboard ? cInput.GetText(action8, 1) : "Keypad6", keepGamepad ? cInput.GetText(action8, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.AnalogR_Right));
		cInput.ChangeKey(action9, keepKeyboard ? cInput.GetText(action9, 1) : "A", keepGamepad ? cInput.GetText(action9, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.Btn_Left));
		cInput.ChangeKey(action10, keepKeyboard ? cInput.GetText(action10, 1) : "S", keepGamepad ? cInput.GetText(action10, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.Btn_Down));
		cInput.ChangeKey(action11, keepKeyboard ? cInput.GetText(action11, 1) : "D", keepGamepad ? cInput.GetText(action11, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.Btn_Right));
		cInput.ChangeKey(action12, keepKeyboard ? cInput.GetText(action12, 1) : "Q", keepGamepad ? cInput.GetText(action12, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.TriggerL1));
		cInput.ChangeKey(action13, keepKeyboard ? cInput.GetText(action13, 1) : "E", keepGamepad ? cInput.GetText(action13, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.TriggerL2));
		cInput.ChangeKey(action14, keepKeyboard ? cInput.GetText(action14, 1) : "F", keepGamepad ? cInput.GetText(action14, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.Btn_Up));
		cInput.ChangeKey(action15, keepKeyboard ? cInput.GetText(action15, 1) : "Return", keepGamepad ? cInput.GetText(action15, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.Start));
		cInput.ChangeKey(action16, keepKeyboard ? cInput.GetText(action16, 1) : "Escape", keepGamepad ? cInput.GetText(action16, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.Option));
		cInput.ChangeKey(action17, keepKeyboard ? cInput.GetText(action17, 1) : "R", keepGamepad ? cInput.GetText(action17, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.TriggerR1));
		cInput.ChangeKey(action18, keepKeyboard ? cInput.GetText(action18, 1) : "C", keepGamepad ? cInput.GetText(action18, 2) : MonoBehaviourSingleton<GamepadManager>.Instance.KeyToJoystickName(gamepad, GamepadKey.TriggerR2));
	}

	public void ManualGamepadCheck()
	{
		if (InControl.InputManager.ActiveDevice.Name.Equals("PlayStation 4 Controller"))
		{
			float num = Mathf.Abs(Input.GetAxis("PS4 Gamepad Wired Detect") * 1000f);
			if (num > 0f && !currentGamepad.Equals("PlayStation 4 Controller Wired"))
			{
				GamepadChange(InControl.InputManager.ActiveDevice);
			}
			else if (num == 0f && !currentGamepad.Equals("PlayStation 4 Controller Wireless"))
			{
				GamepadChange(InControl.InputManager.ActiveDevice);
			}
		}
	}

	public void ManualUpdateStartButton()
	{
		UpdateButtonStatus(ButtonId.START);
	}

	public void ManualUpdate()
	{
		if (!VirtualPadSystem)
		{
			return;
		}
		VirtualButton[] listVirtualButtonInstances;
		if (UsingCursor)
		{
			if (currentInputInfo == null)
			{
				currentInputInfo = ManagedSingleton<InputStorage>.Instance.GetInputInfo(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
			}
			currentInputInfo._analogStickValue[0] = VInt2.zero;
			for (int i = 0; i < 19; i++)
			{
				UpdateButtonStatus((ButtonId)i);
			}
			ClearButtonStatusCache();
			listVirtualButtonInstances = VirtualPadSystem.ListVirtualButtonInstances;
			foreach (VirtualButton virtualButton in listVirtualButtonInstances)
			{
				currentInputInfo._analogStickValue[(int)virtualButton.AnalogID] = VInt2.zero;
			}
			return;
		}
		if (currentInputInfo == null)
		{
			currentInputInfo = ManagedSingleton<InputStorage>.Instance.GetInputInfo(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
		}
		currentInputInfo._analogStickValue[0] = new VInt2(VirtualPadSystem.VirtualAnalogStickInstance ? VirtualPadSystem.VirtualAnalogStickInstance.GetStickValue() : Vector2.zero);
		if (currentInputInfo._analogStickValue[0] == VInt2.zero)
		{
			currentInputInfo._analogStickValue[0] = new VInt2(Mathf.RoundToInt(GetAxis(HorizontalHash) * 1000f), Mathf.RoundToInt((0f - GetAxis(VerticalHash)) * 1000f));
		}
		if (currentInputInfo._analogStickValue[0].sqrMagnitude < DeadZoneLX * DeadZoneLX)
		{
			currentInputInfo._analogStickValue[0] = VInt2.zero;
		}
		for (int k = 0; k < 19; k++)
		{
			UpdateButtonStatus((ButtonId)k);
		}
		ClearButtonStatusCache();
		listVirtualButtonInstances = VirtualPadSystem.ListVirtualButtonInstances;
		foreach (VirtualButton virtualButton2 in listVirtualButtonInstances)
		{
			if (virtualButton2.AllowAnalog)
			{
				Vector2 relativeStickValue = virtualButton2.GetRelativeStickValue();
				currentInputInfo._analogStickValue[(int)virtualButton2.AnalogID] = new VInt2((relativeStickValue != Vector2.zero) ? relativeStickValue : new Vector2(cInput.GetAxis(HorizontalHash2), 0f - cInput.GetAxis(VerticalHash2)));
				if (currentInputInfo._analogStickValue[(int)virtualButton2.AnalogID].sqrMagnitude < DeadZoneR * DeadZoneR)
				{
					if (!virtualButton2.ActivateAnalog)
					{
						currentInputInfo._analogStickValue[(int)virtualButton2.AnalogID] = VInt2.zero;
					}
					else if (!virtualButton2.GetButtonStatus() && currentInputInfo._analogStickValue[(int)virtualButton2.AnalogID] != VInt2.zero)
					{
						virtualButton2.ActivateAnalog = false;
					}
				}
				else
				{
					virtualButton2.ActivateAnalog = true;
				}
				virtualButton2.SetAnalogVisible(virtualButton2.GetButtonStatus() && virtualButton2.ActivateAnalog && ForceDisplayAnalog);
			}
			else
			{
				currentInputInfo._analogStickValue[(int)virtualButton2.AnalogID] = VInt2.zero;
			}
		}
	}

	public void ClearManualInput()
	{
		if (VirtualPadSystem != null && VirtualPadSystem.VirtualButtonInstances != null)
		{
			VirtualButton[] listVirtualButtonInstances = VirtualPadSystem.ListVirtualButtonInstances;
			for (int i = 0; i < listVirtualButtonInstances.Length; i++)
			{
				listVirtualButtonInstances[i].ClearButton();
			}
		}
		ManagedSingleton<InputStorage>.Instance.ResetPlayerInput(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
	}

    [Obsolete]
    public void LoadVirtualPad(int type = 1, bool p_editorMode = false, CallbackObj getVpsCallback = null)
	{
		Transform canvasUI = MonoBehaviourSingleton<UIManager>.Instance.CanvasUI;
		if (canvasUI == null)
		{
			return;
		}
		canvasUI = MonoBehaviourSingleton<UIManager>.Instance.JoystickPanelParent;
		if (canvasUI == null)
		{
			return;
		}
		canvasUI.gameObject.AddOrGetComponent<CanvasGroup>().alpha = 1f - MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.UITrans;
		if (!p_editorMode && null != VirtualPadSystem)
		{
			return;
		}
		string text = "prefab/virtualpadsystem";
		string text2 = "VirtualPadSystem";
		if (type == 0)
		{
			text = "prefab/virtualpadsystem";
			text2 = "VirtualPadSystem";
		}
		else
		{
			text = "prefab/virtualpadsystem" + type;
			text2 = "VirtualPadSystem" + type;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(text, text2, delegate(UnityEngine.Object obj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)obj, Vector3.zero, Quaternion.identity);
			gameObject.transform.SetParent(MonoBehaviourSingleton<UIManager>.Instance.JoystickPanelParent, false);
			gameObject.transform.SetSiblingIndex(0);
			MonoBehaviourSingleton<UIManager>.Instance.AddSafeArea(gameObject);
			VirtualPadSystem component = gameObject.GetComponent<VirtualPadSystem>();
			component.InitializeVirtualPad(p_editorMode);
			if (!p_editorMode)
			{
				VirtualPadSystem = component;
				currentInputInfo = ManagedSingleton<InputStorage>.Instance.GetInputInfo(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
				MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
				MonoBehaviourSingleton<UpdateManager>.Instance.onPauseEvent -= OnUpdatePause;
				MonoBehaviourSingleton<UpdateManager>.Instance.onPauseEvent += OnUpdatePause;
				MonoBehaviourSingleton<CursorController>.Instance.IsEnable = false;
			}
			if (getVpsCallback != null)
			{
				getVpsCallback(component);
			}
		});
	}

	private void OnUpdatePause(bool isPause)
	{
		if (VirtualPadSystem != null)
		{
			MonoBehaviourSingleton<CursorController>.Instance.IsEnable = isPause;
		}
	}

	public void DestroyVirtualPad()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		if (VirtualPadSystem != null)
		{
			VirtualPadSystem.DestroyVirtualPad();
			VirtualPadSystem = null;
			MonoBehaviourSingleton<UpdateManager>.Instance.onPauseEvent -= OnUpdatePause;
			MonoBehaviourSingleton<CursorController>.Instance.IsEnable = true;
		}
		currentInputInfo = null;
		ManagedSingleton<InputStorage>.Instance.ResetPlayerInput(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
	}

	public void InitializeVirtualPad()
	{
		if (VirtualPadSystem != null)
		{
			UpdateMapping();
			VirtualPadSystem.InitializeVirtualPad();
		}
	}

	public void AddTouchChain(VirtualButton vBtn)
	{
		if (!TouchChain.Contains(vBtn))
		{
			TouchChain.Add(vBtn);
			ManagedSingleton<InputStorage>.Instance.GetInputInfo(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify).TouchChainCount = (byte)TouchChain.Count;
		}
	}

	public void ClearTouchChain()
	{
		foreach (VirtualButton item in TouchChain)
		{
			if ((bool)item)
			{
				item.ClearButton();
			}
		}
		TouchChain.Clear();
		ManagedSingleton<InputStorage>.Instance.GetInputInfo(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify).TouchChainCount = 0;
	}

	public VirtualButton GetVirtualButton(ButtonId id)
	{
		if (!VirtualPadSystem)
		{
			return null;
		}
		return VirtualPadSystem.GetButton(id);
	}

	public void UpdateFunc()
	{
		for (int i = 0; i < _buttonStatusCacheID.Length; i++)
		{
			UpdateButtonStatusCache(i);
		}
	}

	public void UpdateJoystickStatus()
	{
		string[] joystickNames = Input.GetJoystickNames();
		if (joystickNames.Length >= 1)
		{
			if (IsJoystickConnected && string.IsNullOrEmpty(joystickNames[0]))
			{
				IsJoystickConnected = false;
			}
			else if (!string.IsNullOrEmpty(joystickNames[0]) && !IsJoystickConnected && _initializedMapping)
			{
				MonoBehaviourSingleton<RogManager>.Instance.UpdateDefaultJoystickMapping(joystickNames[0]);
				IsJoystickConnected = true;
			}
		}
	}
}
