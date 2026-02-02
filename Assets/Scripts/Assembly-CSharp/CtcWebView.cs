#define RELEASE
using System;
using System.Collections;
using System.IO;
using Steamworks;
using UnityEngine;

public class CtcWebView : OrangeUIBase
{
	protected readonly int designWidth = 1920;

	protected readonly int designHeight = 1080;

	[SerializeField]
	protected RectTransform webViewParent;

	protected WebViewObject webViewObject;

	private string Url;

	private Action<string> m_cb;

	private Action<string> m_err;

	private Action<string> m_httpErr;

	private Action<string> m_ld;

	private Action<string> m_started;

	public static void Create<T>(out T webView, string Url, Action<string> cb = null, bool transparent = false, string ua = "", Action<string> err = null, Action<string> httpErr = null, Action<string> ld = null, Action<string> started = null, SystemSE OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP, string uiName = "UI_WebView") where T : CtcWebView
	{
		webView = null;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(OpenSE);
		SteamFriends.OpenWebOverlay(Url, true);
	}

	private void Start()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public bool CanGoBack()
	{
		return false;
	}

	public void GoBack()
	{
	}

	public bool CanGoForward()
	{
		return false;
	}

	public void GoForward()
	{
	}

	public void EvaluateJS(string js)
	{
	}

	public override void OnClickCloseBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		base.OnClickCloseBtn();
	}

    [Obsolete]
    public IEnumerator OnStart()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		webViewObject = webViewParent.gameObject.AddComponent<WebViewObject>();
		webViewObject.Init(delegate(string msg)
		{
			Debug.Log(string.Format("CallFromJS[{0}]", msg));
			if (m_cb != null)
			{
				m_cb(msg);
			}
		}, false, "", delegate(string msg)
		{
			Debug.Log(string.Format("CallOnError[{0}]", msg));
			if (m_err != null)
			{
				m_err(msg);
			}
		}, null, delegate(string msg)
		{
			Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
			webViewObject.EvaluateJS("\r\n                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {\r\n                    window.Unity = {\r\n                      call: function(msg) {\r\n                        window.webkit.messageHandlers.unityControl.postMessage(msg);\r\n                      }\r\n                    }\r\n                  } else {\r\n                    window.Unity = {\r\n                      call: function(msg) {\r\n                        window.location = 'unity:' + msg;\r\n                      }\r\n                    }\r\n                  }\r\n                ");
			webViewObject.EvaluateJS("Unity.call('ua=' + navigator.userAgent)");
			if (m_ld != null)
			{
				m_ld(msg);
			}
		}, true, delegate(string msg)
		{
			Debug.Log(string.Format("CallOnStarted[{0}]", msg));
			if (m_started != null)
			{
				m_started(msg);
			}
		});
		SetMargins();
		webViewObject.SetVisibility(true);
		if (Url.StartsWith("http"))
		{
			webViewObject.LoadURL(Url.Replace(" ", "%20"));
			yield break;
		}
		string[] array = new string[3] { ".jpg", ".js", ".html" };
		string[] array2 = array;
		foreach (string ext in array2)
		{
			string path = Url.Replace(".html", ext);
			string text = Path.Combine(Application.streamingAssetsPath, path);
			string dst = Path.Combine(Application.persistentDataPath, path);
			byte[] bytes;
			if (text.Contains("://"))
			{
				WWW www = new WWW(text);
				yield return www;
				bytes = www.bytes;
			}
			else
			{
				bytes = File.ReadAllBytes(text);
			}
			File.WriteAllBytes(dst, bytes);
			if (ext == ".html")
			{
				webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
				break;
			}
		}
	}

	protected virtual void SetMargins()
	{
		float num = ((MonoBehaviourSingleton<UIManager>.Instance.MatchWidthOrHeight == 0) ? ((float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth / (float)designWidth) : ((float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight / (float)designHeight));
		webViewObject.SetMargins((int)(num * webViewParent.offsetMin.x), (int)(num * (0f - webViewParent.offsetMax.y)), (int)(num * (0f - webViewParent.offsetMax.x)), (int)(num * webViewParent.offsetMin.y));
	}
}
