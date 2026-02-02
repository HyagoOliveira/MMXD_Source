using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using enums;

[CreateAssetMenu]
public class LocalizationScriptableObject : ScriptableObject
{
	private static LocalizationScriptableObject m_instance;

	public Language m_Language;

	[SerializeField]
	public List<LanguagePopupInfo> languagePopupInfos = new List<LanguagePopupInfo>();

	public static LocalizationScriptableObject Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = Resources.Load<LocalizationScriptableObject>("LocalizationScriptableObject");
			}
			return m_instance;
		}
	}

	public LanguagePopupInfo GetPopupInfo(Language p_Language)
	{
		LanguagePopupInfo languagePopupInfo = languagePopupInfos.FirstOrDefault((LanguagePopupInfo x) => x.languageEnum == p_Language);
		if (languagePopupInfo != null)
		{
			return languagePopupInfo;
		}
		return languagePopupInfos[0];
	}
}
