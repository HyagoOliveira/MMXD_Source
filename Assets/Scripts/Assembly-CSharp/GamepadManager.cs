using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GamepadManager : MonoBehaviourSingleton<GamepadManager>
{
	public const string GAMEPAD_NAME_PS4 = "PlayStation 4 Controller";

	public const string GAMEPAD_NAME_PS4_WIRED = "PlayStation 4 Controller Wired";

	public const string GAMEPAD_NAME_PS4_WIRELESS = "PlayStation 4 Controller Wireless";

	private const string JOYSTICK_BUTTON_NAME = "JoystickButton";

	private const string JOYSTICK_AXIS_NAME = "Joy Axis ";

	private const string BUTTON = "Button";

	private const string AXIS = "Axis ";

	private bool isInitialized;

	private Dictionary<GamepadType, GamepadData> gamepadDataList = new Dictionary<GamepadType, GamepadData>();

	private Dictionary<GamepadType, string> gamepadNameList = new Dictionary<GamepadType, string>();

	public string GetMappingName(GamepadType mappingSource, GamepadType mappingTarget, string joystickName)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		joystickName = RebuildJoystickName(joystickName);
		GamepadKey gamepadKey = GamepadKey.Unknown;
		GamepadData value;
		if (gamepadDataList.TryGetValue(mappingSource, out value))
		{
			gamepadKey = value.GetGamepadKey(joystickName);
		}
		if (gamepadKey == GamepadKey.Unknown)
		{
			return string.Empty;
		}
		string result = string.Empty;
		GamepadData value2;
		if (gamepadDataList.TryGetValue(mappingTarget, out value2))
		{
			result = value2.GetJoystickName(gamepadKey);
		}
		return result;
	}

	public GamepadKey JoystickNameToKey(GamepadType gamepadType, string joystickName)
	{
		joystickName = RebuildJoystickName(joystickName);
		GamepadData value;
		if (gamepadDataList.TryGetValue(gamepadType, out value))
		{
			return value.GetGamepadKey(joystickName);
		}
		return GamepadKey.Unknown;
	}

	public string KeyToJoystickName(GamepadType gamepadType, GamepadKey key)
	{
		GamepadData value;
		if (gamepadDataList.TryGetValue(gamepadType, out value))
		{
			return value.GetJoystickName(key);
		}
		return "None";
	}

	public GamepadType GamepadNameToGamepadType(string gamepadName)
	{
		foreach (KeyValuePair<GamepadType, string> gamepadName2 in gamepadNameList)
		{
			if (gamepadName2.Value == gamepadName)
			{
				return gamepadName2.Key;
			}
		}
		return GamepadType.Unknown;
	}

	public string GamepadTypeToGamepadName(GamepadType gamepadType)
	{
		string value;
		if (gamepadNameList.TryGetValue(gamepadType, out value))
		{
			return value;
		}
		return string.Empty;
	}

	public Sprite GetGamepadKeyIcon(GamepadType gamepadType, GamepadKey key)
	{
		GamepadData value;
		if (gamepadDataList.TryGetValue(gamepadType, out value))
		{
			return value.GetKeyIcon(key);
		}
		return null;
	}

	public Sprite GetGamepadKeyIcon(GamepadType gamepadType, string joystickName)
	{
		joystickName = RebuildJoystickName(joystickName);
		GamepadData value;
		if (gamepadDataList.TryGetValue(gamepadType, out value))
		{
			return value.GetKeyIcon(joystickName);
		}
		return null;
	}

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (!isInitialized)
		{
			GamepadData value = BuildGamepad_XBOX();
			gamepadDataList.Add(GamepadType.Xbox, value);
			gamepadDataList.Add(GamepadType.XboxOne, value);
			gamepadDataList.Add(GamepadType.PS4_Wired, BuildGamepad_PS4_Wired());
			gamepadDataList.Add(GamepadType.PS4_Wireless, BuildGamepad_PS4_Wireless());
			gamepadDataList.Add(GamepadType.NSPro, BuildGamepad_NS());
			gamepadDataList.Add(GamepadType.XInput, BuildGamepad_XInput());
			SetupGamepadName();
			isInitialized = true;
		}
	}

	private void SetupGamepadName()
	{
		gamepadNameList.Add(GamepadType.Xbox, "Xbox Controller");
		gamepadNameList.Add(GamepadType.XboxOne, "Xbox One Controller");
		gamepadNameList.Add(GamepadType.PS4_Wired, "PlayStation 4 Controller Wired");
		gamepadNameList.Add(GamepadType.PS4_Wireless, "PlayStation 4 Controller Wireless");
		gamepadNameList.Add(GamepadType.NSPro, "Nintendo Switch Pro Controller");
		gamepadNameList.Add(GamepadType.XInput, "XInput Controller");
	}

	public string RebuildJoystickName(string joystickName)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (joystickName.Contains("Button"))
		{
			empty = "Button";
			empty2 = "JoystickButton";
		}
		else
		{
			if (!joystickName.Contains("Axis "))
			{
				return string.Empty;
			}
			empty = "Axis ";
			empty2 = "Joy Axis ";
		}
		int num = joystickName.LastIndexOf(empty);
		string value = joystickName.Substring(num + empty.Length);
		return new StringBuilder(empty2).Append(value).ToString();
	}

	private GamepadData BuildGamepad_PS4_Wired()
	{
		GamepadData gamepadData = new GamepadData();
		gamepadData.joystickNames.Add(GamepadKey.Btn_Up, "JoystickButton3");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Down, "JoystickButton1");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Left, "JoystickButton0");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Right, "JoystickButton2");
		gamepadData.joystickNames.Add(GamepadKey.TriggerR1, "JoystickButton5");
		gamepadData.joystickNames.Add(GamepadKey.TriggerR2, "JoystickButton7");
		gamepadData.joystickNames.Add(GamepadKey.TriggerL1, "JoystickButton4");
		gamepadData.joystickNames.Add(GamepadKey.TriggerL2, "JoystickButton6");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL, "JoystickButton10");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR, "JoystickButton11");
		gamepadData.joystickNames.Add(GamepadKey.Special, "JoystickButton13");
		gamepadData.joystickNames.Add(GamepadKey.Start, "JoystickButton9");
		gamepadData.joystickNames.Add(GamepadKey.Option, "JoystickButton8");
		gamepadData.joystickNames.Add(GamepadKey.Home, "JoystickButton12");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Up, "Joy Axis 8+");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Down, "Joy Axis 8-");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Left, "Joy Axis 7-");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Right, "Joy Axis 7+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Up, "Joy Axis 2-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Down, "Joy Axis 2+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Left, "Joy Axis 1-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Right, "Joy Axis 1+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Up, "Joy Axis 6-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Down, "Joy Axis 6+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Left, "Joy Axis 3-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Right, "Joy Axis 3+");
		PrepareIcon(gamepadData, "icon_controller_ps");
		return gamepadData;
	}

	private GamepadData BuildGamepad_PS4_Wireless()
	{
		GamepadData gamepadData = new GamepadData();
		gamepadData.joystickNames.Add(GamepadKey.Btn_Up, "JoystickButton3");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Down, "JoystickButton1");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Left, "JoystickButton0");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Right, "JoystickButton2");
		gamepadData.joystickNames.Add(GamepadKey.TriggerR1, "JoystickButton5");
		gamepadData.joystickNames.Add(GamepadKey.TriggerR2, "JoystickButton7");
		gamepadData.joystickNames.Add(GamepadKey.TriggerL1, "JoystickButton4");
		gamepadData.joystickNames.Add(GamepadKey.TriggerL2, "JoystickButton6");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL, "JoystickButton10");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR, "JoystickButton11");
		gamepadData.joystickNames.Add(GamepadKey.Special, "JoystickButton13");
		gamepadData.joystickNames.Add(GamepadKey.Start, "JoystickButton9");
		gamepadData.joystickNames.Add(GamepadKey.Option, "JoystickButton8");
		gamepadData.joystickNames.Add(GamepadKey.Home, "JoystickButton12");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Up, "Joy Axis 9+");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Down, "Joy Axis 9-");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Left, "Joy Axis 8-");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Right, "Joy Axis 8+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Up, "Joy Axis 3-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Down, "Joy Axis 3+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Left, "Joy Axis 1-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Right, "Joy Axis 1+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Up, "Joy Axis 7-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Down, "Joy Axis 7+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Left, "Joy Axis 4-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Right, "Joy Axis 4+");
		PrepareIcon(gamepadData, "icon_controller_ps");
		return gamepadData;
	}

	private GamepadData BuildGamepad_NS()
	{
		GamepadData gamepadData = new GamepadData();
		gamepadData.joystickNames.Add(GamepadKey.Btn_Up, "JoystickButton3");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Down, "JoystickButton0");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Left, "JoystickButton2");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Right, "JoystickButton1");
		gamepadData.joystickNames.Add(GamepadKey.TriggerR1, "JoystickButton5");
		gamepadData.joystickNames.Add(GamepadKey.TriggerR2, "JoystickButton7");
		gamepadData.joystickNames.Add(GamepadKey.TriggerL1, "JoystickButton4");
		gamepadData.joystickNames.Add(GamepadKey.TriggerL2, "JoystickButton6");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL, "JoystickButton10");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR, "JoystickButton11");
		gamepadData.joystickNames.Add(GamepadKey.Special, "JoystickButton13");
		gamepadData.joystickNames.Add(GamepadKey.Start, "JoystickButton9");
		gamepadData.joystickNames.Add(GamepadKey.Option, "JoystickButton8");
		gamepadData.joystickNames.Add(GamepadKey.Home, "JoystickButton12");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Up, "Joy Axis 10+");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Down, "Joy Axis 10-");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Left, "Joy Axis 9-");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Right, "Joy Axis 9+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Up, "Joy Axis 4-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Down, "Joy Axis 4+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Left, "Joy Axis 2-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Right, "Joy Axis 2+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Up, "Joy Axis 8-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Down, "Joy Axis 8+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Left, "Joy Axis 7-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Right, "Joy Axis 7+");
		PrepareIcon(gamepadData, "icon_controller_ns");
		return gamepadData;
	}

	private GamepadData BuildGamepad_XBOX()
	{
		GamepadData gamepadData = new GamepadData();
		PrepareIcon(gamepadData, "icon_controller_xbox");
		return gamepadData;
	}

	private GamepadData BuildGamepad_XInput()
	{
		GamepadData gamepadData = new GamepadData();
		gamepadData.joystickNames.Add(GamepadKey.Btn_Up, "JoystickButton3");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Down, "JoystickButton0");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Left, "JoystickButton2");
		gamepadData.joystickNames.Add(GamepadKey.Btn_Right, "JoystickButton1");
		gamepadData.joystickNames.Add(GamepadKey.TriggerR1, "JoystickButton5");
		gamepadData.joystickNames.Add(GamepadKey.TriggerR2, "Joy Axis 3+");
		gamepadData.joystickNames.Add(GamepadKey.TriggerL1, "JoystickButton4");
		gamepadData.joystickNames.Add(GamepadKey.TriggerL2, "Joy Axis 3-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL, "JoystickButton8");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR, "JoystickButton9");
		gamepadData.joystickNames.Add(GamepadKey.Start, "JoystickButton7");
		gamepadData.joystickNames.Add(GamepadKey.Option, "JoystickButton6");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Up, "Joy Axis 7+");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Down, "Joy Axis 7-");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Left, "Joy Axis 6-");
		gamepadData.joystickNames.Add(GamepadKey.Arrow_Right, "Joy Axis 6+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Up, "Joy Axis 2-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Down, "Joy Axis 2+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Left, "Joy Axis 1-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogL_Right, "Joy Axis 1+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Up, "Joy Axis 5-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Down, "Joy Axis 5+");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Left, "Joy Axis 4-");
		gamepadData.joystickNames.Add(GamepadKey.AnalogR_Right, "Joy Axis 4+");
		PrepareIcon(gamepadData, "icon_controller_xbox");
		return gamepadData;
	}

	private void PrepareIcon(GamepadData data, string name)
	{
		string assetBundleName = "texture/2d/icon/steam/" + name;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[1] { assetBundleName }, delegate
		{
			for (int i = 0; i < 27; i++)
			{
				GamepadKey key = (GamepadKey)i;
				StringBuilder stringBuilder = new StringBuilder(name).Append("_");
				GamepadKey gamepadKey = (GamepadKey)i;
				StringBuilder stringBuilder2 = stringBuilder.Append(gamepadKey.ToString());
				data.icons.Add(key, MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(assetBundleName, stringBuilder2.ToString()));
			}
		}, AssetsBundleManager.AssetKeepMode.KEEP_ALWAYS);
	}
}
