using System;
using UnityEngine;
using UnityEngine.Events;

public class KochavaConfiguration : MonoBehaviour
{
	[Serializable]
	public class ConsentStatusChangeEvent : UnityEvent
	{
	}

	[Serializable]
	public class AttributionChangeEvent : UnityEvent<string>
	{
	}

	[NonSerialized]
	[Header("App GUID Configuration")]
	public string appGUID_UnityEditor = "";

	[NonSerialized]
	public string appGUID_iOS = "";

	[NonSerialized]
	public string appGUID_Android = "";

	[NonSerialized]
	public string appGUID_WindowsStoreUWP = "";

	[NonSerialized]
	public string appGUID_WindowsPC = "";

	[NonSerialized]
	public string appGUID_MacOS = "";

	[NonSerialized]
	public string appGUID_Linux = "";

	[NonSerialized]
	public string appGUID_WebGL = "";

	[Header("Partner Configuration (Optional)")]
	public string kochavaPartnerName = "";

	public string kochavaPartnerApp = "";

	[Header("Instant Apps / App Clips (Optional")]
	public string instantAppGUID_Android = "";

	public string containerAppGroupIdentifier_iOS = "";

	[NonSerialized]
	[Header("App Tracking Transparency (iOS)")]
	public bool attEnabled;

	[NonSerialized]
	public double attWaitTime = 30.0;

	[NonSerialized]
	public bool attAutoRequest = true;

	[Header("Settings")]
	public bool appAdLimitTracking;

	public Kochava.DebugLogLevel logLevel;

	public bool sleepOnStart;

	[Header("Attribution Callback")]
	public bool requestAttributionCallback;

	public AttributionChangeEvent attributionChangeEvent;

	[Header("Intelligent Consent Management")]
	public bool intelligentConsentManagement;

	public bool manualManagedConsentRequirements;

	public ConsentStatusChangeEvent consentStatusChangeEvent;

	private void Awake()
	{
		attEnabled = true;
		attWaitTime = 90.0;
		attAutoRequest = true;
		appGUID_iOS = "korxd-ios-1uahpey0v";
		appGUID_Android = "korxd-android-k36o1o";
		string appGuid = appGUID_UnityEditor;
		if (appGUID_WindowsPC.Length > 0)
		{
			appGuid = appGUID_WindowsPC;
		}
		Kochava.Tracker.Config.SetLogLevel(logLevel);
		Kochava.Tracker.Config.SetAppGuid(appGuid);
		Kochava.Tracker.Config.SetInstantAppGuid(instantAppGUID_Android);
		Kochava.Tracker.Config.SetContainerAppGroupIdentifier(containerAppGroupIdentifier_iOS);
		Kochava.Tracker.Config.SetRetrieveAttribution(requestAttributionCallback);
		Kochava.Tracker.Config.SetIntelligentConsentManagement(intelligentConsentManagement);
		Kochava.Tracker.Config.SetPartnerName(kochavaPartnerName);
		Kochava.Tracker.Config.SetPartnerApp(kochavaPartnerApp);
		Kochava.Tracker.Config.SetSleep(sleepOnStart);
		Kochava.Tracker.Config.SetAppTrackingTransparency(attEnabled, attWaitTime, attAutoRequest);
		if (!PlayerPrefs.HasKey("kou__latset"))
		{
			Kochava.Tracker.Config.SetAppLimitAdTracking(appAdLimitTracking);
			PlayerPrefs.SetInt("kou__latset", 1);
		}
		Kochava.Tracker.Initialize();
		if (requestAttributionCallback && attributionChangeEvent != null)
		{
			Kochava.Tracker.SetAttributionHandler(delegate(string attribution)
			{
				attributionChangeEvent.Invoke(attribution);
			});
		}
		if (intelligentConsentManagement && consentStatusChangeEvent != null)
		{
			Kochava.Tracker.SetConsentStatusChangeHandler(delegate
			{
				consentStatusChangeEvent.Invoke();
			});
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}
}
