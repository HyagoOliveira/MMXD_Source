using UnityEngine;

public class RogManager : MonoBehaviourSingleton<RogManager>
{
	private static string[] JoystickNames = new string[3] { "ROG Kunai Gamepad", "ROG Kunai GamePad II", "ROG Kunai 3 Gamepad" };

	private int joystickNamesLength = JoystickNames.Length;

	private int joystickSearchIdx = -1;

	public bool IsSleep { get; private set; }

	private void Awake()
	{
		IsSleep = true;
	}

	public void Init()
	{
		IsSleep = true;
	}

	public void UpdateDefaultJoystickMapping(string joystickName)
	{
		joystickSearchIdx = 0;
		while (joystickSearchIdx < joystickNamesLength && !(joystickName == JoystickNames[joystickSearchIdx]))
		{
			joystickSearchIdx++;
		}
		if (joystickSearchIdx < joystickNamesLength && PlayerPrefs.GetInt(JoystickNames[joystickSearchIdx], 0) != 1)
		{
			cInput.ChangeKey(ButtonId.UP.ToString(), "Joy Axis 6+", "Joy1 Axis 2-");
			cInput.ChangeKey(ButtonId.DOWN.ToString(), "Joy1 Axis 6-", "Joy1 Axis 2+");
			cInput.ChangeKey(ButtonId.LEFT.ToString(), "Joy Axis 5-", "Joy1 Axis 1-");
			cInput.ChangeKey(ButtonId.RIGHT.ToString(), "Joy Axis 5+", "Joy1 Axis 1+");
			cInput.ChangeKey(ButtonId.AIM_UP.ToString(), "Joy Axis 4-", "None");
			cInput.ChangeKey(ButtonId.AIM_DOWN.ToString(), "Joy Axis 4+", "None");
			cInput.ChangeKey(ButtonId.AIM_LEFT.ToString(), "Joy Axis 3-", "None");
			cInput.ChangeKey(ButtonId.AIM_RIGHT.ToString(), "Joy Axis 3+", "None");
			cInput.ChangeKey(ButtonId.SHOOT.ToString(), "Joystick1Button3", "None");
			cInput.ChangeKey(ButtonId.JUMP.ToString(), "Joystick1Button0", "None");
			cInput.ChangeKey(ButtonId.DASH.ToString(), "Joystick1Button1", "None");
			cInput.ChangeKey(ButtonId.SKILL0.ToString(), "Joystick1Button8", "None");
			cInput.ChangeKey(ButtonId.SKILL1.ToString(), "Joystick1Button9", "None");
			cInput.ChangeKey(ButtonId.FS_SKILL.ToString(), "Joystick1Button4", "None");
			cInput.ChangeKey(ButtonId.CHIP_SWITCH.ToString(), "None", "None");
			cInput.ChangeKey(ButtonId.SELECT.ToString(), "Joystick1Button6", "Joystick1Button7");
		}
	}

	public void SaveJoystickSetting(string joystick)
	{
		for (int i = 0; i < joystickNamesLength; i++)
		{
			if (joystick == JoystickNames[i])
			{
				PlayerPrefs.SetInt(JoystickNames[i], 1);
				break;
			}
		}
	}

	public void ResetJoystickSetting()
	{
		int num = 0;
		if (num < joystickNamesLength)
		{
			PlayerPrefs.DeleteKey(JoystickNames[num]);
		}
	}
}
