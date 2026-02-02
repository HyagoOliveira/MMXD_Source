using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class LanguageUIUnit : MonoBehaviour
{
	[SerializeField]
	private Text desc;

	[SerializeField]
	private Button btn;

	public Language language;
    [System.Obsolete]
    private CallbackObj m_cb;

    [System.Obsolete]
    public void Setup(LanguagePopupInfo p_languageInfo, CallbackObj p_cb)
	{
		if (p_languageInfo.loadFromRes)
		{
			desc.font = MonoBehaviourSingleton<LocalizationManager>.Instance.GetResFont(p_languageInfo.fontName);
		}
		else
		{
			desc.font = MonoBehaviourSingleton<LocalizationManager>.Instance.ArialFont;
		}
		language = p_languageInfo.languageEnum;
		desc.text = p_languageInfo.Language;
		m_cb = p_cb;
	}

	public void LanguageSelectBtnClick()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		m_cb(language);
	}

	public void SetBtnInteractable(bool enable)
	{
		btn.interactable = enable;
	}
}
