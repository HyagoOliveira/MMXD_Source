#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Better;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;
using enums;

public class LocalizationManager : MonoBehaviourSingleton<LocalizationManager>
{
	public const string LOCALIZAION_FILE_INFO = "localizationfileinfo";

	public bool InitOK;

	private System.Collections.Generic.Dictionary<string, string> localizationDict = new Better.Dictionary<string, string>();

	public System.Collections.Generic.Dictionary<string, Texture> dictCacheTexture = new Better.Dictionary<string, Texture>();

	private Font languageFont;

	private Font arialFont;

	private Font DefaultFont;

	private Type type = typeof(LOCALIZATION_TABLE);

	private string keyFld = "";

	private LanguagePopupInfo languageInfo;

	private string emptyStr = string.Empty;

	private System.Collections.Generic.Dictionary<string, OrangeCrcInfo> CrcDict = new Better.Dictionary<string, OrangeCrcInfo>();

	public Font LanguageFont
	{
		get
		{
			if (languageFont == null)
			{
				return ArialFont;
			}
			return languageFont;
		}
	}

	public Font ArialFont
	{
		get
		{
			if (arialFont == null)
			{
				arialFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
			}
			return arialFont;
		}
	}

	public string FilePath { get; private set; }

	public bool IsCrcLoad { get; set; }

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.PATCH_CHANGE, LoadCRC);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.PATCH_CHANGE, LoadCRC);
	}

	public bool Setup()
	{
		IsCrcLoad = false;
		if (LocalizationScriptableObject.Instance.m_Language == Language.Unknown)
		{
			LocalizationScriptableObject.Instance.m_Language = GetGameLanguageBySystem();
		}
		LoadOrangeTextTable();
		return false;
	}

	public void LoadFontAndDefaultAssets()
	{
		List<string> list = new List<string>();
		list.Add("font/battle_number");
		list.Add("font/dft_9");
		list.Add("font/number");
		list.Add("font/exo-semibold");
		if (!languageInfo.loadFromRes)
		{
			list.Add("font/" + languageInfo.fontName);
		}
		list.Add(AssetBundleScriptableObject.Instance.m_texture_ui_common);
		list.Add(AssetBundleScriptableObject.Instance.m_texture_ui_sub_common);
		list.Add("texture/2d/ui/ui_novice_teach");
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(list.ToArray(), delegate
		{
			SetFontAssets();
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		}, AssetsBundleManager.AssetKeepMode.KEEP_ALWAYS, false);
	}

	private void SetFontAssets()
	{
		if (languageInfo.loadFromRes)
		{
			DefaultFont = GetResFont(languageInfo.fontName);
		}
		else
		{
			DefaultFont = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Font>("font/" + languageInfo.fontName, languageInfo.fontName);
		}
		if (DefaultFont != null)
		{
			languageFont = DefaultFont;
		}
		StartCoroutine(OnStartUpdateLanguageData());
	}

	public void OpenLanguageUI(bool first = false)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_Language", delegate(LanguageUI ui)
		{
			ui.isFirst = first;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				HometopUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<HometopUI>("UI_Hometop");
				if (uI != null)
				{
					uI.objChat.GetComponent<HometopChatUI>().Clear();
				}
			});
			ui.Setup(delegate
			{
				Debug.Log("Player Select language :" + LocalizationScriptableObject.Instance.m_Language);
				PlayerPrefs.SetString("ORANGE_L10N_KEY_LANGUAGE", LocalizationScriptableObject.Instance.m_Language.ToString());
				PlayerPrefs.Save();
				LoadOrangeTextTable();
				LoadFontAndDefaultAssets();
			});
		});
	}

	public void LoadOrangeTextTable()
	{
		localizationDict.Clear();
		localizationDict.Add("NONE", "");
		Language language = LocalizationScriptableObject.Instance.m_Language;
		switch (language)
		{
		default:
			keyFld = "w_CHT";
			FilePath = "Localization/CHT/";
			break;
		case Language.ChineseSimplified:
			keyFld = "w_CHS";
			FilePath = "Localization/CHS/";
			break;
		case Language.Japanese:
			keyFld = "w_JP";
			FilePath = "Localization/JP/";
			break;
		case Language.Unknown:
		case Language.English:
			keyFld = "w_ENG";
			FilePath = "Localization/EN/";
			break;
		case Language.Thai:
			keyFld = "w_THA";
			FilePath = "Localization/TH/";
			break;
		}
		languageInfo = LocalizationScriptableObject.Instance.GetPopupInfo(language);
		PropertyInfo property = type.GetProperty(keyFld, BindingFlags.Instance | BindingFlags.Public);
		Debug.Log("[LocalizationManager] Start load table, count:" + ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.Count);
		foreach (KeyValuePair<string, LOCALIZATION_TABLE> item in ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT)
		{
			string value = property.GetValue(item.Value, null).ToString();
			localizationDict.Add(item.Key, value);
		}
	}

	public Language GetGameLanguageBySystem()
	{
		Language language = Language.Unknown;
		switch (Application.systemLanguage)
		{
		case SystemLanguage.Chinese:
		case SystemLanguage.ChineseSimplified:
		case SystemLanguage.ChineseTraditional:
			return Language.ChineseTraditional;
		case SystemLanguage.Japanese:
			return Language.Japanese;
		default:
			return Language.English;
		case SystemLanguage.Thai:
			return Language.Thai;
		}
	}

	public string GetNameFromGameServerNameInfo(GameServerNameInfo nameInfo)
	{
		switch (LocalizationScriptableObject.Instance.m_Language)
		{
		default:
			return nameInfo.ChineseTraditional;
		case Language.ChineseSimplified:
			return nameInfo.ChineseSimplified;
		case Language.Japanese:
			return nameInfo.Japanese;
		case Language.Thai:
			return nameInfo.Thai;
		case Language.Unknown:
		case Language.English:
			return nameInfo.English;
		}
	}

	private IEnumerator OnStartUpdateLanguageData()
	{
		OrangeText[] array = Resources.FindObjectsOfTypeAll<OrangeText>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.scene.name != null)
			{
				array[i].UpdateTextImmediate();
			}
		}
		ClearTextureCache();
		L10nRawImage[] array2 = Resources.FindObjectsOfTypeAll<L10nRawImage>();
		for (int i = 0; i < array2.Length; i++)
		{
			if (array[i].gameObject.scene.name != null)
			{
				array2[i].UpdateImageImmediate();
			}
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		InitOK = true;
		Debug.Log("localization warm up complete.");
	}

	public void ClearTextureCache(bool gc = true)
	{
		foreach (Texture value in dictCacheTexture.Values)
		{
			UnityEngine.Object.Destroy(value);
		}
		dictCacheTexture.Clear();
		if (gc)
		{
			GC.Collect();
		}
	}

	public void ClearSingleTextureCache(string key)
	{
		Texture value = null;
		if (dictCacheTexture.TryGetValue(key, out value))
		{
			UnityEngine.Object.Destroy(value);
			dictCacheTexture.Remove(key);
		}
	}

	public void ReloadLocalizationDict(Language p_language)
	{
		LocalizationScriptableObject.Instance.m_Language = p_language;
		localizationDict.Clear();
		SetFontAssets();
	}

	public string GetStr(string p_key, params object[] args)
	{
		string value;
		if (localizationDict.TryGetValue(p_key, out value))
		{
			if (args.Length == 0)
			{
				return value;
			}
			return string.Format(value, args);
		}
		return emptyStr;
	}

	public bool IsValidKey(string p_key)
	{
		return localizationDict.ContainsKey(p_key);
	}

	public string GetText(LOCALIZATION_TABLE t)
	{
		object value = type.GetProperty(keyFld, BindingFlags.Instance | BindingFlags.Public).GetValue(t, null);
		if (value == null)
		{
			return emptyStr;
		}
		return value.ToString();
	}

	public void GetL10nRawImage(L10nRawImage.ImageType imageType, OrangeWebRequestLoad.LoadType p_loadType, bool p_save2Local, string p_textureName, Callback<byte[], string> p_cb)
	{
		OrangeWebRequestLoad.LoadingFlg loadingFlg = OrangeWebRequestLoad.LoadingFlg.READ_ALL_BYTE;
		if (p_save2Local)
		{
			loadingFlg |= OrangeWebRequestLoad.LoadingFlg.SAVE_TO_LOCAL;
		}
		string text = FilePath + imageType.ToString() + "/" + p_textureName;
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(p_loadType, text, loadingFlg, p_cb, GetCrc(text));
	}

	public Font GetResFont(string fontName)
	{
		Font font = Resources.Load<Font>("font/" + fontName);
		if (font != null)
		{
			return font;
		}
		return ArialFont;
	}

	public string GetUrlLanguage()
	{
		switch (LocalizationScriptableObject.Instance.m_Language)
		{
		case Language.ChineseTraditional:
			return "zh-TW";
		case Language.English:
			return "en-US";
		case Language.Japanese:
			return "ja-JP";
		case Language.Thai:
			return "th-TH";
		default:
			return "en-US";
		}
	}

	public string GetPlatformLan()
	{
		switch (LocalizationScriptableObject.Instance.m_Language)
		{
		case Language.ChineseTraditional:
			return "cht";
		case Language.English:
			return "en";
		case Language.Japanese:
			return "jp";
		case Language.Thai:
			return "tai";
		default:
			return "en";
		}
	}

	public string GetOfficalLan()
	{
		switch (LocalizationScriptableObject.Instance.m_Language)
		{
		case Language.ChineseTraditional:
			return "zh";
		case Language.English:
			return "en";
		case Language.Japanese:
			return "en";
		case Language.Thai:
			return "en";
		default:
			return "en";
		}
	}

	public string GetUrlLanSupport()
	{
		switch (LocalizationScriptableObject.Instance.m_Language)
		{
		case Language.ChineseTraditional:
		case Language.ChineseSimplified:
			return "cht";
		default:
			return "en";
		case Language.Unknown:
		case Language.English:
			return "en";
		}
	}

	public void LoadCRC()
	{
		string p_fileName = ManagedSingleton<ServerConfig>.Instance.PatchUrl + "Localization/localizationfileinfo";
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.UNIQUE, p_fileName, OrangeWebRequestLoad.LoadingFlg.TEXT_DEFAULT, delegate(byte[] config, string path)
		{
			CrcDict.Clear();
			foreach (OrangeCrcInfo item in JsonConvert.DeserializeObject<List<OrangeCrcInfo>>(AesCrypto.Decode(Encoding.UTF8.GetString(config))))
			{
				CrcDict.Add(item.Name, item);
			}
			IsCrcLoad = true;
		});
	}

	private string GetCrc(string key)
	{
		OrangeCrcInfo value = null;
		if (CrcDict.TryGetValue(key, out value))
		{
			return value.Crc;
		}
		return string.Empty;
	}
}
