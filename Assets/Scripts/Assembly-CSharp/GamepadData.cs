using System.Collections.Generic;
using UnityEngine;

public class GamepadData
{
	public Dictionary<GamepadKey, string> joystickNames = new Dictionary<GamepadKey, string>();

	public Dictionary<GamepadKey, Sprite> icons = new Dictionary<GamepadKey, Sprite>();

	public GamepadKey GetGamepadKey(string name)
	{
		foreach (KeyValuePair<GamepadKey, string> joystickName in joystickNames)
		{
			if (joystickName.Value.Equals(name))
			{
				return joystickName.Key;
			}
		}
		return GamepadKey.Unknown;
	}

	public string GetJoystickName(GamepadKey key)
	{
		string value;
		if (joystickNames.TryGetValue(key, out value))
		{
			return value;
		}
		return string.Empty;
	}

	public Sprite GetKeyIcon(GamepadKey key)
	{
		Sprite value;
		if (icons.TryGetValue(key, out value))
		{
			return value;
		}
		return null;
	}

	public Sprite GetKeyIcon(string joystickName)
	{
		GamepadKey gamepadKey = GetGamepadKey(joystickName);
		Sprite value;
		if (icons.TryGetValue(gamepadKey, out value))
		{
			return value;
		}
		return null;
	}
}
