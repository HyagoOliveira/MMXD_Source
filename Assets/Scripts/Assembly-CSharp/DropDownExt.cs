#define RELEASE
using UnityEngine.UI;

public static class DropDownExt
{
	public static string CurrentValue(this Dropdown dropDown)
	{
		return dropDown.options[dropDown.value].text;
	}

	public static void SelectValue(this Dropdown dropDown, int value)
	{
		dropDown.SelectValue(value.ToString());
	}

	public static void SelectValue(this Dropdown dropDown, string value)
	{
		int num = dropDown.options.FindIndex((Dropdown.OptionData option) => option.text == value);
		if (num >= 0)
		{
			dropDown.value = num;
			return;
		}
		Debug.LogError("Option not found : " + value);
		dropDown.value = 0;
	}
}
