using DigitalRuby.RainMaker;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
	public enum Weath_Type
	{
		NONE = 0,
		RAIN = 1,
		CLOUD = 2
	}

	public Weath_Type WeatherType;

	public bool isStartActive;

	public int StartActiveLivel;

	public bool isStartActiveFade;

	public string AssetsName;

	private Weath_Type mType;

	private GameObject WeatherEffect;

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<Weath_Type, Transform>(EventManager.ID.WEATHER_SYSTEM_INIT, Init_System);
		Singleton<GenericEventManager>.Instance.AttachEvent<bool, int, bool>(EventManager.ID.WEATHER_SYSTEM_CTRL, EffectCTRL);
		if (WeatherType != 0)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.WEATHER_SYSTEM_INIT, WeatherType, base.transform);
		}
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<Weath_Type, Transform>(EventManager.ID.WEATHER_SYSTEM_INIT, Init_System);
		Singleton<GenericEventManager>.Instance.DetachEvent<bool, int, bool>(EventManager.ID.WEATHER_SYSTEM_CTRL, EffectCTRL);
	}

	private void Awake()
	{
	}

	private void EffectCTRL(bool isOpen, int intensityLevel, bool isFade)
	{
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect == 0 || MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.ResolutionRate == 0.5f)
		{
			return;
		}
		switch (mType)
		{
		case Weath_Type.RAIN:
			if (isOpen)
			{
				WeatherEffect.GetComponent<BaseRainScript>().EffectSubjoin(intensityLevel, isFade);
			}
			else
			{
				WeatherEffect.GetComponent<BaseRainScript>().EffectLessen(intensityLevel, isFade);
			}
			break;
		case Weath_Type.CLOUD:
			if (isOpen)
			{
				WeatherEffect.GetComponent<UVScrollMaterial>().EffectSubjoin(intensityLevel, isFade);
			}
			else
			{
				WeatherEffect.GetComponent<UVScrollMaterial>().EffectLessen(intensityLevel, isFade);
			}
			break;
		}
	}

	private void Init_System(Weath_Type mWType, Transform parent)
	{
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.scrEffect == 0 || MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.ResolutionRate == 0.5f)
		{
			return;
		}
		mType = mWType;
		switch (mType)
		{
		case Weath_Type.RAIN:
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/weather/rainprefab2d", "RainPrefab2D", delegate(Object obj)
			{
				if (!(obj == null))
				{
					WeatherEffect = Object.Instantiate((GameObject)obj);
					WeatherEffect.transform.SetParent(parent, false);
				}
			});
			break;
		case Weath_Type.CLOUD:
		{
			string text = "multicloud";
			if (!AssetsName.Equals(""))
			{
				text = AssetsName;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/weather/" + text, text, delegate(Object obj)
			{
				if (!(obj == null))
				{
					WeatherEffect = Object.Instantiate((GameObject)obj);
					WeatherEffect.transform.SetParent(parent, false);
				}
			});
			break;
		}
		}
	}
}
