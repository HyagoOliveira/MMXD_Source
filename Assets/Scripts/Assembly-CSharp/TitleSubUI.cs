#define RELEASE
using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class TitleSubUI : OrangeUIBase
{
	[SerializeField]
	private Button btnLogout;

	[SerializeField]
	private Button btnQuitGame;

	private SystemSE m_clickSE = SystemSE.CRI_SYSTEMSE_SYS_OK01;

	protected override void Awake()
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base.Awake();
		SetBtnState();
	}

	public void OnClickBtnNotice()
	{
		string platformLan = MonoBehaviourSingleton<LocalizationManager>.Instance.GetPlatformLan();
		string url = string.Format(ManagedSingleton<ServerConfig>.Instance.ServerSetting.Platform.Notice, platformLan);
		CtcWebView webView = null;
		CtcWebView.Create<CtcWebView>(out webView, url);
	}

	public void OnClickBtnLanguage()
	{
		bool first = LocalizationScriptableObject.Instance.m_Language == Language.Unknown;
		MonoBehaviourSingleton<LocalizationManager>.Instance.OpenLanguageUI(first);
	}

	public void OnClickBtnClearCache()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REBOOT_MSG"), delegate
		{
			base.CloseSE = SystemSE.NONE;
			MonoBehaviourSingleton<EventManager>.Instance.DetachAllEvent();
			MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DeletePersistentDataAll();
			MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
				{
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadAllBundleCache(delegate
					{
						new Action(MonoBehaviourSingleton<OrangeGameManager>.Instance.BackSplashAction)();
					}, true);
				});
			});
		}, null, SystemSE.NONE);
	}

	public void OnClickBtnReport()
	{
		Application.OpenURL(ManagedSingleton<ServerConfig>.Instance.GetCustomerUrl());
	}

	public void OnClickBtnBinding()
	{
	}

	public void OnClickBtnLogout()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REBOOT_MSG_CONFIRM"), delegate
		{
			Debug.Log("Confirmed Logout and lost data!!");
			base.CloseSE = SystemSE.NONE;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Logout();
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch");
		}, null, SystemSE.CRI_SYSTEMSE_SYS_OK17);
	}

	public void OnQuitGame()
	{
		Debug.Log("Steam Quit Game!");
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MESSAGE_QUITGAME_CONFIRM"), delegate
		{
			Application.Quit();
		}, null, SystemSE.CRI_SYSTEMSE_SYS_OK17);
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	private void SetBtnState()
	{
		btnLogout.gameObject.SetActive(!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.IsNewAccount());
		if ((bool)btnQuitGame)
		{
			btnQuitGame.gameObject.SetActive(true);
		}
	}
}
