using UnityEngine;

public class LawBtn : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.SetActive(false);
	}

	public void OnClickLinkJPLaw()
	{
		Application.OpenURL(string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.JPLaw, Application.platform.ToString()).Replace(" ", "%20"));
	}

	public void OnClickLinkJPFund()
	{
		Application.OpenURL(string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.JPFund, Application.platform.ToString()).Replace(" ", "%20"));
	}
}
