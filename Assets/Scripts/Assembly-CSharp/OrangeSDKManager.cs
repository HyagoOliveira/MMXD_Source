using CallbackDefs;
using UnityEngine;

public class OrangeSDKManager : MonoBehaviourSingleton<OrangeSDKManager>
{
	private Callback UpdateCB;

	public string GetDeviceName()
	{
		return string.Empty;
	}

	public string GetID()
	{
		return string.Empty;
	}

	public void Init(Callback p_cb)
	{
		UpdateCB = p_cb;
		UpdateData("!$%@#$%@#$%@#$%@!#$!^%@#$^$%!#@!@$%^&%@#$*^");
	}

	public void UpdateData(string str)
	{
		try
		{
			int.Parse(str);
			Application.Quit();
		}
		catch
		{
			UpdateCB.CheckTargetToInvoke();
			UpdateCB = null;
		}
	}
}
