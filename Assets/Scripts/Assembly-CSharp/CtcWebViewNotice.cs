using UnityEngine;
using UnityEngine.UI;

public class CtcWebViewNotice : CtcWebView
{
	public static readonly string FLAG_NOTICE_NEVER_SHOWING_TODAY = "FLAG_NOTICE_NEVER_SHOWING_TODAY";

	[SerializeField]
	private RectTransform rtParentBg;

	[SerializeField]
	private Image imgNeverShowing;

	[SerializeField]
	private RectTransform bottomBg;

	private int offsetLR;

	private int offsetTB;

	private int neverShowingToday;

	protected override void Awake()
	{
		base.Awake();
		neverShowingToday = PlayerPrefs.GetInt(FLAG_NOTICE_NEVER_SHOWING_TODAY, 0);
		UpdateNeverShowing();
		offsetLR = 0;
		offsetTB = 0;
		if (rtParentBg.anchorMin.x != 0f && rtParentBg.anchorMin.y != 0f)
		{
			float y = MonoBehaviourSingleton<UIManager>.Instance.SafeAreaRect.y;
			if (y > 0f)
			{
				bottomBg.anchoredPosition = new Vector2(bottomBg.anchoredPosition.x, bottomBg.anchoredPosition.y + y);
				webViewParent.offsetMin = new Vector2(webViewParent.offsetMin.x, webViewParent.offsetMin.y + y);
			}
			int screenWidth = MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth;
			int screenHeight = MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight;
			if (MonoBehaviourSingleton<UIManager>.Instance.MatchWidthOrHeight == 1)
			{
				float num = (float)screenWidth * (float)designHeight / (float)screenHeight;
				offsetLR = (int)(num - (float)designWidth) / 2;
			}
			else
			{
				float num2 = (float)screenHeight * (float)designWidth / (float)screenWidth;
				offsetTB = (int)(num2 - (float)designHeight) / 2;
			}
		}
	}

	protected override void SetMargins()
	{
		float num = ((MonoBehaviourSingleton<UIManager>.Instance.MatchWidthOrHeight == 0) ? ((float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth / (float)designWidth) : ((float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight / (float)designHeight));
		webViewObject.SetMargins((int)(num * (webViewParent.offsetMin.x + (float)offsetLR)), (int)(num * (0f - webViewParent.offsetMax.y + (float)offsetTB)), (int)(num * (0f - webViewParent.offsetMax.x + (float)offsetLR)), (int)(num * (webViewParent.offsetMin.y + (float)offsetTB)));
	}

	public void OnClickNeverShowing()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		neverShowingToday = ((neverShowingToday != 1) ? 1 : 0);
		UpdateNeverShowing();
	}

	private void UpdateNeverShowing()
	{
		imgNeverShowing.color = ((neverShowingToday == 1) ? Color.white : Color.clear);
		PlayerPrefs.SetInt(FLAG_NOTICE_NEVER_SHOWING_TODAY, neverShowingToday);
	}
}
