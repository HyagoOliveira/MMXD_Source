using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using enums;

public static class OrangeGameUtility
{
	public enum PeriodFormat
	{
		ORIGINAL = 0,
		ROTATE = 1,
		PLUSGMT = 2
	}

	private static readonly int DaysTotalSeconds = 86400;

	private static readonly int HoursTotalSeconds = 3600;

	private static readonly int MinutesTotalSeconds = 60;

	public static T AddOrGetComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (null == val)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static List<int> RangeList(int seed, int min, int max, int length)
	{
		List<int> list = new List<int>();
		UnityEngine.Random.InitState(seed);
		for (int i = 0; i < length; i++)
		{
			list.Add(UnityEngine.Random.Range(min, max));
		}
		return list;
	}

	public static string GetL10nValue(this Dictionary<string, LOCALIZATION_TABLE> p_dict, string p_key)
	{
		LOCALIZATION_TABLE value = null;
		if (p_dict.TryGetValue(p_key, out value))
		{
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetText(value);
		}
		return string.Empty;
	}

	public static SKILL_TABLE GetSkillTableByValue(this SKILL_TABLE pSkillTable)
	{
		SKILL_TABLE sKILL_TABLE = new SKILL_TABLE();
		sKILL_TABLE.ConvertFromString(pSkillTable.ConvertToString());
		return sKILL_TABLE;
	}

	public static bool TryGetNewValue(this Dictionary<int, SKILL_TABLE> p_dict, int p_key, out SKILL_TABLE value)
	{
		SKILL_TABLE value2 = null;
		if (p_dict.TryGetValue(p_key, out value2))
		{
			value2.GetSkillTableByValue();
			value = value2;
			return true;
		}
		value = value2;
		return false;
	}

	public static string GetOperationText(int systemTextID)
	{
		Language language = LocalizationScriptableObject.Instance.m_Language;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicSystemContext.ContainsKey(systemTextID))
		{
			Dictionary<Language, SystemContext> dictionary = ManagedSingleton<PlayerNetManager>.Instance.dicSystemContext[systemTextID];
			if (dictionary.ContainsKey(language) && dictionary[language].netSystemContext != null)
			{
				return dictionary[language].netSystemContext.Context;
			}
		}
		return string.Empty;
	}

	public static string GetRemainTimeTextDetail(long timestamp, bool skipDay = false, bool skipHour = false)
	{
		string text = string.Empty;
		int num = Convert.ToInt32(timestamp);
		if (!skipDay)
		{
			int num2 = num / DaysTotalSeconds;
			if (num2 != 0)
			{
				text = num2 + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_DAY");
				num -= num2 * DaysTotalSeconds;
			}
		}
		if (!skipHour)
		{
			int num3 = num / HoursTotalSeconds;
			if (num3 != 0)
			{
				text = text + num3 + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_HOUR");
				num -= num3 * HoursTotalSeconds;
			}
		}
		int num4 = num / MinutesTotalSeconds;
		if (num4 != 0)
		{
			text = text + num4 + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_MINUTE");
			num -= num4 * MinutesTotalSeconds;
		}
		return text + num + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_SECOND");
	}

	public static string GetRemainTimeText(long timestamp, bool skipDay = false, bool skipHour = false)
	{
		string empty = string.Empty;
		int num = Convert.ToInt32(timestamp - MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
		if (num > DaysTotalSeconds && !skipDay)
		{
			return string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_REMAIN"), num / DaysTotalSeconds) + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_DAY");
		}
		if (num > HoursTotalSeconds && !skipHour)
		{
			return string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_REMAIN"), num / HoursTotalSeconds) + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_HOUR");
		}
		if (num > MinutesTotalSeconds)
		{
			return string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_REMAIN"), num / MinutesTotalSeconds) + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_MINUTE");
		}
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_LESS_THAN_MINUTE");
	}

	public static string GetRemainTimeText(long timestamp, long now, out bool remain, bool skipDay = false, bool skipHour = false)
	{
		string result = string.Empty;
		remain = true;
		int num = Convert.ToInt32(timestamp - now);
		if (num > DaysTotalSeconds && !skipDay)
		{
			result = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_REMAIN"), num / DaysTotalSeconds) + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_DAY");
		}
		else if (num > HoursTotalSeconds && !skipHour)
		{
			result = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_REMAIN"), num / HoursTotalSeconds) + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_HOUR");
		}
		else if (num > MinutesTotalSeconds)
		{
			result = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_REMAIN"), num / MinutesTotalSeconds) + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_MINUTE");
		}
		else if (num > 0)
		{
			result = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_LESS_THAN_MINUTE");
		}
		else
		{
			remain = false;
		}
		return result;
	}

	public static string GetTimeText(long duration, bool skipDay = false, bool skipHour = false)
	{
		string empty = string.Empty;
		if (duration > DaysTotalSeconds && !skipDay)
		{
			return string.Format("{0}{1}", duration / DaysTotalSeconds, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_DAY"));
		}
		if (duration > HoursTotalSeconds && !skipHour)
		{
			return string.Format("{0}{1}", duration / HoursTotalSeconds, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_HOUR"));
		}
		if (duration > MinutesTotalSeconds)
		{
			return string.Format("{0}{1}", duration / MinutesTotalSeconds, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_MINUTE"));
		}
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_LESS_THAN_MINUTE");
	}

	public static string DisplayDatePeriod(string beginTime, string endTime, PeriodFormat format = PeriodFormat.ROTATE)
	{
		switch (format)
		{
		default:
			return string.Format("{0}~{1}", ManagedSingleton<OrangeTableHelper>.Instance.ServerDateToLocalDate(beginTime), ManagedSingleton<OrangeTableHelper>.Instance.ServerDateToLocalDate(endTime));
		case PeriodFormat.ORIGINAL:
			return string.Format("{0}~{1}", beginTime, endTime);
		case PeriodFormat.PLUSGMT:
			return string.Format("{0}~{1}(GMT{2}{3})", beginTime, endTime, (ManagedSingleton<OrangeTableHelper>.Instance.ServerTimeZone >= 0) ? "+" : "-", Mathf.Abs(ManagedSingleton<OrangeTableHelper>.Instance.ServerTimeZone));
		}
	}

	public static bool IsResidentEvent(string p_beginTime, string p_endTime)
	{
		if (ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(p_beginTime) || ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(p_endTime))
		{
			return true;
		}
		DateTime dateTime = ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(p_beginTime);
		if (ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(p_endTime).Year - dateTime.Year >= 10)
		{
			return true;
		}
		return false;
	}

	public static Renderer AddOrGetRenderer<T>(GameObject go) where T : Renderer
	{
		T val = null;
		val = go.GetComponent<T>();
		if (val == null)
		{
			MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
			meshRenderer.lightProbeUsage = LightProbeUsage.Off;
			meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;
			return meshRenderer;
		}
		return val;
	}

	public static string GetFileCRC(byte[] data)
	{
		byte[] array = HashAlgorithm.Create().ComputeHash(data);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static bool SetNewFov(ref float designFov)
	{
		if (1920f / (float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth * (float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight < 1080f)
		{
			float num = designFov * ((float)Math.PI / 180f);
			float num2 = 1.7777778f;
			float num3 = 2f * Mathf.Atan(Mathf.Tan(num / 2f) * num2);
			float num4 = (float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth / (float)MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight;
			float num5 = 2f * Mathf.Atan(Mathf.Tan(num3 / 2f) / num4);
			designFov = num5 * 57.29578f;
			return true;
		}
		return false;
	}

	public static void SetParentNull(this Transform p_trans)
	{
		p_trans.SetParent(null);
		SceneManager.MoveGameObjectToScene(p_trans.gameObject, MonoBehaviourSingleton<OrangeSceneManager>.Instance.Scene);
	}

	public static char Asc2Chr(int num)
	{
		return Convert.ToChar(num);
	}

	public static int Chr2Asc(char c)
	{
		return Convert.ToInt32(c);
	}

	public static string UrlAntiCache(this StringBuilder stringBuilder)
	{
		return stringBuilder.Append("?").Append(DateTime.Now.ToString("yyyyMMddHHmmss")).ToString();
	}

	public static string AppendEscapeDataString(this StringBuilder stringBuilder, string escapeDataString)
	{
		return stringBuilder.Append(Uri.EscapeDataString(escapeDataString)).ToString();
	}

	public static void DeleteLockObj(Transform parent)
	{
		Transform transform = OrangeBattleUtility.FindChildRecursive(parent, "LockObj", true);
		if ((bool)transform)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
	}

	public static void CreateLockObj(Transform parent, UIOpenChk.ChkBanEnum UIType = UIOpenChk.ChkBanEnum.OPENBAN_PVP)
	{
		GameObject parent2 = new GameObject("LockObj");
		RectTransform rectTransform = parent2.AddOrGetComponent<RectTransform>();
		rectTransform.SetParent(parent);
		switch (UIType)
		{
		case UIOpenChk.ChkBanEnum.OPENBAN_BOSSRUSH:
		case UIOpenChk.ChkBanEnum.OPENBAN_EVENT:
			rectTransform.localPosition = new Vector3(-69f, 5f, 0f);
			rectTransform.anchorMin = new Vector2(0f, 0.5f);
			rectTransform.anchorMax = new Vector2(1f, 0.5f);
			rectTransform.sizeDelta = new Vector2(0f, 100f);
			rectTransform.localScale = Vector3.one;
			break;
		case UIOpenChk.ChkBanEnum.OPENBAN_PVP:
		{
			Texture texture2 = null;
			RawImage rawImage2 = parent2.AddComponent<RawImage>();
			texture2 = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Texture>(AssetBundleScriptableObject.Instance.m_texture_ui_hometop, "UI_Main_Rightbutton_01_PVP");
			rawImage2.texture = texture2;
			rawImage2.color = new Color(0.3372549f, 62f / 85f, 81f / 85f, 2f / 3f);
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.sizeDelta = new Vector2(0f, 0f);
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.localScale = Vector3.one;
			break;
		}
		case UIOpenChk.ChkBanEnum.OPENBAN_CORP:
		{
			Texture texture = null;
			RawImage rawImage = parent2.AddComponent<RawImage>();
			texture = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Texture>(AssetBundleScriptableObject.Instance.m_texture_ui_hometop, "UI_Main_Rightbutton_03_stage_team");
			rawImage.texture = texture;
			rawImage.color = new Color(0.3372549f, 62f / 85f, 0f, 2f / 3f);
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.sizeDelta = new Vector2(0f, 0f);
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.localScale = Vector3.one;
			break;
		}
		default:
			rectTransform.anchorMin = new Vector2(0f, 0.5f);
			rectTransform.anchorMax = new Vector2(1f, 0.5f);
			rectTransform.sizeDelta = new Vector2(0f, 100f);
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.localScale = Vector3.one;
			break;
		}
		switch (UIType)
		{
		case UIOpenChk.ChkBanEnum.OPENBAN_BOSSRUSH:
			CreateLock(ref parent2, Vector3.zero, Vector3.zero);
			break;
		case UIOpenChk.ChkBanEnum.OPENBAN_EVENT:
			CreateLock(ref parent2, Vector3.zero, Vector3.zero);
			break;
		case UIOpenChk.ChkBanEnum.OPENBAN_PVP:
		{
			CreateLock(ref parent2, Vector3.zero, Vector3.zero);
			Image image2 = CreateLockBG(ref parent2, new Color(0.38f, 0.19f, 0.21f, 0.9f), new Vector2(6f, 81f));
			image2.type = Image.Type.Tiled;
			image2.fillCenter = true;
			image2.material = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Material>("ui/fx/ui_fxbase", "UI_commonMati");
			break;
		}
		case UIOpenChk.ChkBanEnum.OPENBAN_CORP:
		{
			CreateLock(ref parent2, Vector3.zero, Vector3.zero);
			Image image = CreateLockBG(ref parent2, new Color(0.76f, 0.72f, 0.25f, 0.2f), new Vector2(6f, 81f));
			image.type = Image.Type.Tiled;
			image.fillCenter = true;
			image.material = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Material>("ui/fx/ui_fxbase", "UI_commonMati");
			break;
		}
		case UIOpenChk.ChkBanEnum.OPENBAN_RAIDBOSS:
			CreateLock(ref parent2, new Vector3(0f, 30f, 0f), Vector3.zero);
			break;
		default:
			CreateLock(ref parent2, Vector3.zero, Vector3.zero);
			break;
		}
		parent2.AddComponent<Button>().onClick.AddListener(delegate
		{
			ShowLockMsg();
		});
		parent2.SetActive(true);
	}

	public static void ShowLockMsg()
	{
		string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("PLUGIN_BANPLAY");
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			ui.Setup(msg);
		});
	}

	private static void CreateLock(ref GameObject parent, Vector3 localpos, Vector3 localrotate)
	{
		GameObject gameObject = new GameObject("LockImg");
		Sprite sprite = null;
		Image image = gameObject.AddComponent<Image>();
		sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_hometop, "UI_Main_lock");
		image.sprite = sprite;
		RectTransform rectTransform = gameObject.AddOrGetComponent<RectTransform>();
		rectTransform.SetParent(parent.transform);
		rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		rectTransform.sizeDelta = new Vector2(80f, 100f);
		rectTransform.localPosition = localpos;
		rectTransform.localRotation = Quaternion.Euler(localrotate);
		rectTransform.localScale = Vector3.one;
	}

	private static Image CreateLockBG(ref GameObject parent, Color bgcolor, Vector2 sizedelta)
	{
		Sprite sprite = null;
		GameObject gameObject = new GameObject("LockBG");
		Image image = gameObject.AddComponent<Image>();
		sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_hometop, "UI_Main_lock02");
		image.sprite = sprite;
		image.color = bgcolor;
		RectTransform rectTransform = gameObject.AddOrGetComponent<RectTransform>();
		rectTransform.SetParent(parent.transform);
		rectTransform.localPosition = new Vector3(0f, 0f, 0f);
		rectTransform.anchorMin = new Vector2(0f, 0.5f);
		rectTransform.anchorMax = new Vector2(1f, 0.5f);
		rectTransform.sizeDelta = sizedelta;
		rectTransform.localScale = Vector3.one;
		return image;
	}
}
