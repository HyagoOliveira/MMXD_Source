using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class LanguageUI : OrangeUIBase
{
	[HideInInspector]
	public bool isFirst;

	[SerializeField]
	private Text textTitle;

	[SerializeField]
	private Text btnDesc;

	[SerializeField]
	private LanguageUIUnit languageUnit;

	[SerializeField]
	private Transform unitParent;

	private LocalizationScriptableObject localizationScriptableObject;

	private Language oldLanguage;

	private Language newLanguage;

	private Callback closeCb;

	private List<LanguageUIUnit> listUnit = new List<LanguageUIUnit>();

	public void Setup(Callback p_cb)
	{
		closeCb = p_cb;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		localizationScriptableObject = LocalizationScriptableObject.Instance;
		oldLanguage = localizationScriptableObject.m_Language;
		List<LanguagePopupInfo> languagePopupInfos = localizationScriptableObject.languagePopupInfos;
		for (int i = 0; i < languagePopupInfos.Count; i++)
		{
			if (languagePopupInfos[i].IsOpen)
			{
				LanguageUIUnit languageUIUnit = Object.Instantiate(languageUnit, unitParent);
				languageUIUnit.gameObject.SetActive(true);
				languageUIUnit.Setup(languagePopupInfos[i], UpdatePopupInfo);
				listUnit.Add(languageUIUnit);
			}
		}
		if (isFirst)
		{
			switch (Application.systemLanguage)
			{
			case SystemLanguage.Chinese:
			case SystemLanguage.ChineseSimplified:
			case SystemLanguage.ChineseTraditional:
				UpdatePopupInfo(Language.ChineseTraditional);
				break;
			case SystemLanguage.Japanese:
				UpdatePopupInfo(Language.Japanese);
				break;
			case SystemLanguage.Thai:
				UpdatePopupInfo(Language.Thai);
				break;
			default:
				UpdatePopupInfo(Language.English);
				break;
			}
		}
		else
		{
			UpdatePopupInfo(oldLanguage);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
	}

	public void UpdatePopupInfo(object p_param)
	{
		newLanguage = (Language)p_param;
		LanguagePopupInfo popupInfo = localizationScriptableObject.GetPopupInfo(newLanguage);
		if (popupInfo.loadFromRes)
		{
			Font resFont = MonoBehaviourSingleton<LocalizationManager>.Instance.GetResFont(popupInfo.fontName);
			btnDesc.font = resFont;
			textTitle.font = resFont;
		}
		else
		{
			btnDesc.font = MonoBehaviourSingleton<LocalizationManager>.Instance.ArialFont;
			textTitle.font = MonoBehaviourSingleton<LocalizationManager>.Instance.ArialFont;
		}
		btnDesc.text = popupInfo.BtnOK;
		textTitle.text = popupInfo.LanguageTitle;
		foreach (LanguageUIUnit item in listUnit)
		{
			item.SetBtnInteractable((newLanguage != item.language) ? true : false);
		}
	}

	public override void OnClickCloseBtn()
	{
		if (newLanguage != oldLanguage)
		{
			localizationScriptableObject.m_Language = newLanguage;
			if (closeCb != null)
			{
				closeCb();
			}
		}
		base.OnClickCloseBtn();
	}
}
