using UnityEngine;

public class LegionManager : MonoBehaviourSingleton<LegionManager>
{
	private string[] LegionPhone = new string[2] { "Lenovo Lenovo L79031", "Lenovo Lenovo L70081" };

	private AndroidJavaObject mVibratorManager;

	private AndroidJavaObject mLightManager;

	private AndroidJavaObject unityActivity;

	public bool isLegionPhone { get; private set; }

	public bool IsLegionSdkPhone()
	{
		string deviceModel = SystemInfo.deviceModel;
		for (int i = 0; i < LegionPhone.Length; i++)
		{
			if (deviceModel.Contains(LegionPhone[i]))
			{
				isLegionPhone = true;
				return true;
			}
		}
		isLegionPhone = false;
		return false;
	}

	private void Awake()
	{
		isLegionPhone = false;
	}

	public void Init()
	{
	}

	public void callVibrator(int time = 1000)
	{
	}

	public void callLight(bool flashon, int color)
	{
	}
}
