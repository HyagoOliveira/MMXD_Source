using UnityEngine;
using UnityEngine.UI;
using enums;

public class OrangeSplashText : OrangeSplash
{
	[SerializeField]
	private string l10nKey = string.Empty;

	[SerializeField]
	private Text text;

	private void Start()
	{
		if (l10nKey != string.Empty)
		{
			Language language = LocalizationScriptableObject.Instance.m_Language;
			if (language == Language.Unknown)
			{
				language = MonoBehaviourSingleton<LocalizationManager>.Instance.GetGameLanguageBySystem();
			}
			LanguagePopupInfo popupInfo = LocalizationScriptableObject.Instance.GetPopupInfo(language);
			if (popupInfo.loadFromRes)
			{
				text.font = MonoBehaviourSingleton<LocalizationManager>.Instance.GetResFont(popupInfo.fontName);
			}
			text.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(l10nKey);
		}
	}

	public override void SetSplashParam()
	{
		if (string.IsNullOrEmpty(text.text))
		{
			text.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(l10nKey);
		}
	}
}
