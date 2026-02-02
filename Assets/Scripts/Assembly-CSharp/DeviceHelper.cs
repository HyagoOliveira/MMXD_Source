using UnityEngine;

public static class DeviceHelper
{
	private static string[] ROGPhone = new string[1] { "ASUS_Z01QD" };

	private static string[] ROGPhone2 = new string[7] { "ASUS_I001D", "ASUS_I001DA", "ASUS_I001DB", "ASUS_I001DC", "ASUS_I001DD", "ASUS_I001DE", "ZS660KL" };

	private static string[] ROGPhone3 = new string[3] { "ASUS_I003D", "ASUS_I003DD", "ZS661KS" };

	private static string[] Other = new string[2] { "Lenovo Lenovo L79031", "Lenovo Lenovo L70081" };

	public static bool IsROGPhone(out int frameRate)
	{
		string deviceModel = SystemInfo.deviceModel;
		for (int i = 0; i < ROGPhone.Length; i++)
		{
			if (deviceModel.Contains(ROGPhone[i]))
			{
				frameRate = 120;
				return true;
			}
		}
		for (int j = 0; j < ROGPhone2.Length; j++)
		{
			if (deviceModel.Contains(ROGPhone2[j]))
			{
				frameRate = 120;
				return true;
			}
		}
		for (int k = 0; k < ROGPhone3.Length; k++)
		{
			if (deviceModel.Contains(ROGPhone3[k]))
			{
				frameRate = 144;
				return true;
			}
		}
		for (int l = 0; l < Other.Length; l++)
		{
			if (deviceModel.Contains(Other[l]))
			{
				frameRate = 144;
				return true;
			}
		}
		frameRate = 60;
		return false;
	}

	public static string GetDeviceName()
	{
		return "device";
	}

	public static string GetActivityName()
	{
		string empty = string.Empty;
		return Application.identifier;
	}

	public static string GetProxy()
	{
		return string.Empty;
	}

	public static sbyte IsSimulator()
	{
		return 0;
	}

	public static string ReadCpuInfo()
	{
		if (string.IsNullOrEmpty(SystemInfo.processorType))
		{
			return "unsupport";
		}
		return SystemInfo.processorType.ToLower();
	}
}
