#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using JsonFx.Json;
using UnityEngine;
using UnityEngine.Networking;

public class Kochava : MonoBehaviour
{
	public enum DebugLogLevel
	{
		none = 0,
		error = 1,
		warn = 2,
		info = 3,
		debug = 4,
		trace = 5
	}

	public enum EventType
	{
		Achievement = 0,
		AddToCart = 1,
		AddToWishList = 2,
		CheckoutStart = 3,
		LevelComplete = 4,
		Purchase = 5,
		Rating = 6,
		RegistrationComplete = 7,
		Search = 8,
		TutorialComplete = 9,
		View = 10,
		AdView = 11,
		PushReceived = 12,
		PushOpened = 13,
		ConsentGranted = 14,
		DeepLink = 15,
		AdClick = 16,
		StartTrial = 17,
		Subscribe = 18
	}

	public delegate void AttributionCallbackDelegate(string response);

	public delegate void ConsentStatusChangeDelegate();

	public static class Tracker
	{
		public static class Config
		{
			public static void SetAppGuid(string value)
			{
				if (!CheckInitialized() && !string.IsNullOrEmpty(value))
				{
					_appGUID = value;
					LOG("Config: appGUID = " + _appGUID, 4);
				}
			}

			public static void SetInstantAppGuid(string value)
			{
				if (!CheckInitialized() && !string.IsNullOrEmpty(value))
				{
					_instantAppGUID = value;
					LOG("Config: instantAppGUID = " + _instantAppGUID, 4);
				}
			}

			public static void SetPartnerName(string value)
			{
				if (!CheckInitialized() && !string.IsNullOrEmpty(value))
				{
					_partnerName = value;
					LOG("Config: partnerName = " + value, 4);
				}
			}

			public static void SetPartnerApp(string value)
			{
				if (!CheckInitialized() && !string.IsNullOrEmpty(value))
				{
					_partnerApp = value;
					LOG("Config: partnerApp = " + value, 4);
				}
			}

			public static void SetLogLevel(DebugLogLevel value)
			{
				if (!CheckInitialized())
				{
					_logLevel = (int)value;
					LOG("Config: debugLogLevel = " + _logLevel, 4);
				}
			}

			public static void SetAppLimitAdTracking(bool value)
			{
				if (!CheckInitialized())
				{
					_appAdLimitTracking = value;
					_appAdLimitTrackingSet = true;
					LOG("Config: appAdLimitTracking = " + _appAdLimitTracking, 4);
				}
			}

			public static void SetRetrieveAttribution(bool value)
			{
				if (!CheckInitialized())
				{
					_requestAttributionCallback = value;
					LOG("Config: requestAttributionCallback = " + _requestAttributionCallback, 4);
				}
			}

			public static void SetContainerAppGroupIdentifier(string value)
			{
				if (!CheckInitialized() && !string.IsNullOrEmpty(value))
				{
					_containerAppGroupIdentifier = value;
					LOG("Config: containerAppGroupIdentifier = " + _containerAppGroupIdentifier, 4);
				}
			}

			public static void SetCustomKeyValuePair(string key, object value)
			{
				if (!CheckInitialized() && !string.IsNullOrEmpty(key) && value != null)
				{
					_dictionaryCustom[key] = value;
					LOG("Config: custom: " + key + " = " + value.ToString(), 4);
				}
			}

			public static void SetIntelligentConsentManagement(bool value)
			{
				if (!CheckInitialized())
				{
					_intelligentConsentManagement = value;
					LOG("Config: consentIntelligentManagement = " + _intelligentConsentManagement, 4);
				}
			}

			public static void SetManualManagedConsentRequirements(bool value)
			{
				if (!CheckInitialized())
				{
					_manualManagedConsentRequirements = value;
					LOG("Config: consentManualManagedRequirements = " + _manualManagedConsentRequirements, 4);
				}
			}

			public static void SetSleep(bool value)
			{
				if (!CheckInitialized())
				{
					_sleep = value;
					LOG("Config: sleep = " + _sleep, 4);
				}
			}

			public static void SetAppTrackingTransparency(bool enabled, double waitTime, bool autoRequest)
			{
				if (!CheckInitialized())
				{
					_attEnabled = enabled;
					_attWaitTime = waitTime;
					_attAutoRequest = autoRequest;
					LOG("Config: AppTrackingTransparency = {enabled=" + _attEnabled + ", waitTime=" + _attWaitTime + ", autoRequest=" + _attAutoRequest + "}", 4);
				}
			}

			private static bool CheckInitialized()
			{
				if (instance == null)
				{
					return false;
				}
				LOG("Config: Kochava Already Initialized.", 4);
				return true;
			}
		}

		public static void Initialize()
		{
			if (string.IsNullOrEmpty(_appGUID) && (string.IsNullOrEmpty(_partnerName) || string.IsNullOrEmpty(_partnerApp)))
			{
				LOG("CANNOT INITIALIZE KOCHAVA - App GUID or Partner Name and App Not Set", 1);
			}
			else if (instance == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = "KochavaTracker";
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				instance = gameObject.AddComponent<Kochava>();
				instance.kochavaGameObject = gameObject;
				LOG("Initializing Kochava Unity Net", 3);
				instance.UnityNetStart();
				initialized = true;
			}
			else
			{
				LOG("ALREADY INITIALIZED KOCHAVA - Kochava can only be initialized once.", 4);
			}
		}

		private static void DeInitialize()
		{
			_appGUID = "";
			_instantAppGUID = "";
			_appAdLimitTracking = false;
			_appAdLimitTrackingSet = false;
			_sleep = false;
			_logLevel = 3;
			_partnerName = "";
			_partnerApp = "";
			_requestAttributionCallback = false;
			_containerAppGroupIdentifier = "";
			_intelligentConsentManagement = false;
			_manualManagedConsentRequirements = false;
			_dictionaryCustom = new Dictionary<string, object>();
			_attributionCallbackDelegate = null;
			_consentStatusChangeDelegate = null;
			_platformEventHandler = null;
			_productName = "";
			Settings.apiURLInit = "https://kvinit-prod.api.kochava.com";
			Settings.apiURLTracker = "https://control.kochava.com";
			Settings.apiURLQuery = "https://control.kochava.com";
			Settings.trackerURL = "/track/json";
			Settings.initURL = "/track/kvinit";
			Settings.queryURL = "/track/kvquery";
			initialized = false;
			if (instance != null && instance.kochavaGameObject != null)
			{
				UnityEngine.Object.Destroy(instance.kochavaGameObject);
				instance.kochavaGameObject = null;
			}
			instance = null;
		}

		public static void SetAttributionHandler(AttributionCallbackDelegate callback)
		{
			_attributionCallbackDelegate = callback;
			LOG("Config: attribution handler set", 4);
		}

		public static void SetConsentStatusChangeHandler(ConsentStatusChangeDelegate callback)
		{
			_consentStatusChangeDelegate = callback;
			LOG("Config: consent status change handler set", 4);
		}

		public static void SetPlatformEventHandler(Action<PlatformEvent> callback)
		{
			_platformEventHandler = callback;
			LOG("Config: platform event handler set", 4);
		}

		public static void SendEvent(string eventName)
		{
			SendEvent(eventName, "");
		}

		public static void SendEvent(string eventName, string eventData)
		{
			if (!CheckTrackerInitialized("SendEvent()"))
			{
				if (string.IsNullOrEmpty(eventName))
				{
					LOG("Invalid Event Name - Cannot send event", 2);
				}
				else
				{
					instance.IngestEvent(eventName, eventData ?? "", "");
				}
			}
		}

		public static void SendEvent(Event kochavaEvent)
		{
			if (!CheckTrackerInitialized("SendEvent(kochavaEvent)"))
			{
				if (kochavaEvent == null || string.IsNullOrEmpty(kochavaEvent.getEventName()))
				{
					LOG("Invalid Event Name - Cannot send event", 2);
				}
				else
				{
					instance.IngestEvent(kochavaEvent);
				}
			}
		}

		public static void SendDeepLink(string deepLinkURI, string sourceApplication = "")
		{
			if (!CheckTrackerInitialized("SendDeepLink()"))
			{
				instance.IngestDeeplink(deepLinkURI, sourceApplication);
			}
		}

		public static string GetAttribution()
		{
			if (CheckTrackerInitialized("GetAttribution()"))
			{
				return "";
			}
			if (PlayerPrefs.HasKey("kou__attribution"))
			{
				return PlayerPrefs.GetString("kou__attribution");
			}
			return "";
		}

		public static string GetDeviceId()
		{
			if (CheckTrackerInitialized("GetDeviceId()"))
			{
				return "";
			}
			if (instance.profile != null && instance.profile.kochavaDeviceID != null)
			{
				return instance.profile.kochavaDeviceID;
			}
			return "";
		}

		public static void SetAppLimitAdTracking(bool desiredAppLimitAdTracking)
		{
			if (!CheckTrackerInitialized("SetAppLimitAdTracking()"))
			{
				_appAdLimitTracking = desiredAppLimitAdTracking;
				_appAdLimitTrackingSet = true;
				instance.CheckWatchlist();
			}
		}

		public static void SetSleep(bool sleep)
		{
			if (!CheckTrackerInitialized("SetSleep()"))
			{
				LOG("SetSleep() Sleep is not supported on this platform.", 1);
			}
		}

		public static bool GetSleep()
		{
			if (CheckTrackerInitialized("GetSleep()"))
			{
				return false;
			}
			LOG("GetSleep() Sleep is not supported on this platform.", 1);
			return false;
		}

		public static void SetIdentityLink(Dictionary<string, string> identityLinkDictionary)
		{
			if (!CheckTrackerInitialized("SetIdentityLink()"))
			{
				instance.IngestIdentityLink(identityLinkDictionary);
			}
		}

		public static string GetVersion()
		{
			if (CheckTrackerInitialized("getVersion()"))
			{
				return "";
			}
			return "Unity NET 4.4.1";
		}

		public static void AddPushToken(byte[] token)
		{
			if (!CheckTrackerInitialized("AddPushToken(byte[])"))
			{
				AddPushToken(BitConverter.ToString(token).Replace("-", string.Empty));
			}
		}

		public static void AddPushToken(string token)
		{
			if (!CheckTrackerInitialized("AddPushToken(string)"))
			{
				LOG("AddPushToken(string) Push tokens are not supported on this platform.", 1);
			}
		}

		public static void RemovePushToken(byte[] token)
		{
			if (!CheckTrackerInitialized("RemovePushToken(byte[])"))
			{
				RemovePushToken(BitConverter.ToString(token).Replace("-", string.Empty));
			}
		}

		public static void RemovePushToken(string token)
		{
			if (!CheckTrackerInitialized("RemovePushToken(string)"))
			{
				LOG("RemovePushToken(string) Push tokens are not supported on this platform.", 1);
			}
		}

		public static bool GetConsentRequired()
		{
			if (CheckTrackerInitialized("GetConsentRequired()"))
			{
				return true;
			}
			LOG("GetConsentRequired() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			return true;
		}

		public static void SetConsentRequired(bool isRequired)
		{
			if (!CheckTrackerInitialized("SetConsentRequired(bool)"))
			{
				LOG("SetConsentRequired(bool) Kochava Intelligent Consent Management is not supported on this platform.", 1);
			}
		}

		public static void SetConsentGranted(bool isGranted)
		{
			if (!CheckTrackerInitialized("SetConsentGranted(bool)"))
			{
				LOG("SetConsentGranted(bool) Kochava Intelligent Consent Management is not supported on this platform.", 1);
			}
		}

		public static bool GetConsentGranted()
		{
			if (CheckTrackerInitialized("GetConsentGranted()"))
			{
				return false;
			}
			LOG("GetConsentGranted() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			return false;
		}

		public static bool GetConsentGrantedOrNotRequired()
		{
			if (!GetConsentGranted())
			{
				return !GetConsentRequired();
			}
			return true;
		}

		public static bool GetConsentShouldPrompt()
		{
			if (CheckTrackerInitialized("GetConsentShouldPrompt()"))
			{
				return false;
			}
			LOG("GetConsentShouldPrompt() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			return false;
		}

		public static void SetConsentPrompted()
		{
			if (!CheckTrackerInitialized("SetConsentPrompted()"))
			{
				LOG("SetConsentPrompted() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			}
		}

		public static string GetConsentPartnersJson()
		{
			if (CheckTrackerInitialized("GetConsentPartnersJson()"))
			{
				return "[]";
			}
			LOG("GetConsentPartnersJson() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			return "";
		}

		public static ConsentPartner[] GetConsentPartners()
		{
			if (CheckTrackerInitialized("GetConsentPartners()"))
			{
				return new ConsentPartner[0];
			}
			LOG("GetConsentPartners() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			return new ConsentPartner[0];
		}

		public static string GetConsentDescription()
		{
			if (CheckTrackerInitialized("GetConsentDescription()"))
			{
				return "";
			}
			LOG("GetConsentDescription() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			return "";
		}

		public static long GetConsentResponseTime()
		{
			if (CheckTrackerInitialized("GetConsentResponseTime()"))
			{
				return 0L;
			}
			LOG("GetConsentPartners() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			return 0L;
		}

		public static bool GetConsentRequirementsKnown()
		{
			if (CheckTrackerInitialized("GetConsentRequirementsKnown()"))
			{
				return false;
			}
			LOG("GetConsentRequirementsKnown() Kochava Intelligent Consent Management is not supported on this platform.", 1);
			return false;
		}

		public static void ProcessDeeplink(string path, Action<Deeplink> callback)
		{
			ProcessDeeplink(path, 10.0, callback);
		}

		public static void ProcessDeeplink(string path, double timeout, Action<Deeplink> callback)
		{
			if (!CheckTrackerInitialized("ProcessDeeplink()"))
			{
				LOG("ProcessDeeplink() ProcessDeeplink is not supported on this platform.", 1);
			}
		}

		public static void EnableAppTrackingTransparencyAutoRequest()
		{
			LOG("EnableAppTrackingTransparencyAutoRequest() EnableAppTrackingTransparencyAutoRequest is not supported on this platform.", 1);
		}

		public static void ExecuteAdvancedInstruction(string key, string command)
		{
			LOG("ExecuteAdvancedInstruction() ExecuteAdvancedInstruction is not supported on this platform.", 1);
		}

		private static bool CheckTrackerInitialized(string sourceName)
		{
			if (!initialized)
			{
				Debug.Log("KOCHAVA ERROR: NOT YET INITIALIZED: " + sourceName);
				return true;
			}
			return false;
		}
	}

	public class ConsentPartner
	{
		public readonly string name;

		public readonly string description;

		public readonly bool granted;

		public readonly long responseTime;

		internal ConsentPartner(Dictionary<string, object> json)
		{
			object value;
			json.TryGetValue("name", out value);
			object value2;
			json.TryGetValue("description", out value2);
			object value3;
			json.TryGetValue("granted", out value3);
			object value4;
			json.TryGetValue("response_time", out value4);
			name = ((string)value) ?? "";
			description = ((string)value2) ?? "";
			granted = (bool)(value3 ?? ((object)false));
			if (value4 is int)
			{
				responseTime = (int)(value4 ?? ((object)0));
			}
			else if (value4 is long)
			{
				responseTime = (long)(value4 ?? ((object)0L));
			}
			else
			{
				responseTime = 0L;
			}
		}
	}

	public class Event
	{
		private string eventName;

		private Dictionary<string, object> dictionary;

		private string appStoreReceiptBase64EncodedString;

		private string receiptDataFromGoogleStore;

		private string receiptDataSignatureFromGoogleStore;

		public string checkoutAsGuest
		{
			set
			{
				dictionary.Add("checkout_as_guest", value);
			}
		}

		public string contentId
		{
			set
			{
				dictionary.Add("content_id", value);
			}
		}

		public string contentType
		{
			set
			{
				dictionary.Add("content_type", value);
			}
		}

		public string currency
		{
			set
			{
				dictionary.Add("currency", value);
			}
		}

		public string dateString
		{
			set
			{
				dictionary.Add("date", value);
			}
		}

		public DateTime date
		{
			set
			{
				dictionary.Add("date", Util.DateTimeToIso8601(value));
			}
		}

		public string description
		{
			set
			{
				dictionary.Add("description", value);
			}
		}

		public string destination
		{
			set
			{
				dictionary.Add("destination", value);
			}
		}

		public double duration
		{
			set
			{
				dictionary.Add("duration", value);
			}
		}

		public string endDateString
		{
			set
			{
				dictionary.Add("end_date", value);
			}
		}

		public DateTime endDate
		{
			set
			{
				dictionary.Add("end_date", Util.DateTimeToIso8601(value));
			}
		}

		public string itemAddedFrom
		{
			set
			{
				dictionary.Add("item_added_from", value);
			}
		}

		public string level
		{
			set
			{
				dictionary.Add("level", value);
			}
		}

		public double maxRatingValue
		{
			set
			{
				dictionary.Add("max_rating_value", value);
			}
		}

		public string name
		{
			set
			{
				dictionary.Add("name", value);
			}
		}

		public string orderId
		{
			set
			{
				dictionary.Add("order_id", value);
			}
		}

		public string origin
		{
			set
			{
				dictionary.Add("origin", value);
			}
		}

		public double price
		{
			set
			{
				dictionary.Add("price", value);
			}
		}

		public double quantity
		{
			set
			{
				dictionary.Add("quantity", value);
			}
		}

		public double ratingValue
		{
			set
			{
				dictionary.Add("rating_value", value);
			}
		}

		public string receiptId
		{
			set
			{
				dictionary.Add("receipt_id", value);
			}
		}

		public string referralFrom
		{
			set
			{
				dictionary.Add("referral_from", value);
			}
		}

		public string registrationMethod
		{
			set
			{
				dictionary.Add("registration_method", value);
			}
		}

		public string results
		{
			set
			{
				dictionary.Add("results", value);
			}
		}

		public string score
		{
			set
			{
				dictionary.Add("score", value);
			}
		}

		public string searchTerm
		{
			set
			{
				dictionary.Add("search_term", value);
			}
		}

		public double spatialX
		{
			set
			{
				dictionary.Add("spatial_x", value);
			}
		}

		public double spatialY
		{
			set
			{
				dictionary.Add("spatial_y", value);
			}
		}

		public double spatialZ
		{
			set
			{
				dictionary.Add("spatial_z", value);
			}
		}

		public string startDateString
		{
			set
			{
				dictionary.Add("start_date", value);
			}
		}

		public DateTime startDate
		{
			set
			{
				dictionary.Add("start_date", Util.DateTimeToIso8601(value));
			}
		}

		public string success
		{
			set
			{
				dictionary.Add("success", value);
			}
		}

		public string userId
		{
			set
			{
				dictionary.Add("user_id", value);
			}
		}

		public string userName
		{
			set
			{
				dictionary.Add("user_name", value);
			}
		}

		public string validated
		{
			set
			{
				dictionary.Add("validated", value);
			}
		}

		public bool background
		{
			set
			{
				dictionary.Add("background", value);
			}
		}

		public string action
		{
			set
			{
				dictionary.Add("action", value);
			}
		}

		public bool completed
		{
			set
			{
				dictionary.Add("completed", value);
			}
		}

		public Dictionary<string, object> payload
		{
			set
			{
				dictionary.Add("payload", value);
			}
		}

		public string uri
		{
			set
			{
				dictionary.Add("uri", value);
			}
		}

		public string source
		{
			set
			{
				dictionary.Add("source", value);
			}
		}

		public string adNetworkName
		{
			set
			{
				dictionary.Add("ad_network_name", value);
			}
		}

		public string adMediationName
		{
			set
			{
				dictionary.Add("ad_mediation_name", value);
			}
		}

		public string adDeviceType
		{
			set
			{
				dictionary.Add("device_type", value);
			}
		}

		public string adPlacement
		{
			set
			{
				dictionary.Add("placement", value);
			}
		}

		public string adType
		{
			set
			{
				dictionary.Add("ad_type", value);
			}
		}

		public string adCampaignID
		{
			set
			{
				dictionary.Add("ad_campaign_id", value);
			}
		}

		public string adCampiagnName
		{
			set
			{
				dictionary.Add("ad_campaign_name", value);
			}
		}

		public string adSize
		{
			set
			{
				dictionary.Add("ad_size", value);
			}
		}

		public string adGroupName
		{
			set
			{
				dictionary.Add("ad_group_name", value);
			}
		}

		public string adGroupID
		{
			set
			{
				dictionary.Add("ad_group_id", value);
			}
		}

		public string getEventName()
		{
			return eventName;
		}

		public Dictionary<string, object> getDictionary()
		{
			return dictionary;
		}

		public string getAppStoreReceiptBase64EncodedString()
		{
			return appStoreReceiptBase64EncodedString;
		}

		public string getReceiptDataFromGoogleStore()
		{
			return receiptDataFromGoogleStore;
		}

		public string getReceiptDataSignatureFromGoogleStore()
		{
			return receiptDataSignatureFromGoogleStore;
		}

		public Event(EventType standardEventType)
		{
			dictionary = new Dictionary<string, object>();
			switch (standardEventType)
			{
			case EventType.Achievement:
				eventName = "Achievement";
				break;
			case EventType.AddToCart:
				eventName = "Add to Cart";
				break;
			case EventType.AddToWishList:
				eventName = "Add to Wish List";
				break;
			case EventType.CheckoutStart:
				eventName = "Checkout Start";
				break;
			case EventType.LevelComplete:
				eventName = "Level Complete";
				break;
			case EventType.Purchase:
				eventName = "Purchase";
				break;
			case EventType.Rating:
				eventName = "Rating";
				break;
			case EventType.RegistrationComplete:
				eventName = "Registration Complete";
				break;
			case EventType.Search:
				eventName = "Search";
				break;
			case EventType.TutorialComplete:
				eventName = "Tutorial Complete";
				break;
			case EventType.View:
				eventName = "View";
				break;
			case EventType.AdView:
				eventName = "Ad View";
				break;
			case EventType.PushReceived:
				eventName = "Push Received";
				break;
			case EventType.PushOpened:
				eventName = "Push Opened";
				break;
			case EventType.ConsentGranted:
				eventName = "Consent Granted";
				break;
			case EventType.DeepLink:
				eventName = "_Deeplink";
				break;
			case EventType.AdClick:
				eventName = "Ad Click";
				break;
			case EventType.StartTrial:
				eventName = "Start Trial";
				break;
			case EventType.Subscribe:
				eventName = "Subscribe";
				break;
			}
		}

		public Event(string customEventType)
		{
			dictionary = new Dictionary<string, object>();
			eventName = customEventType ?? "";
		}

		public void SetCustomValue(string key, double value)
		{
			dictionary.Add(key, value);
		}

		public void SetCustomValue(string key, bool value)
		{
			dictionary.Add(key, value);
		}

		public void SetCustomValue(string key, string value)
		{
			dictionary.Add(key, value);
		}

		public void setReceiptFromGooglePlayStore(string receiptDataFromStore, string receiptDataSignatureFromStore)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				receiptDataFromGoogleStore = receiptDataFromStore;
				receiptDataSignatureFromGoogleStore = receiptDataSignatureFromStore;
			}
			else
			{
				LOG("Event safely ignoring setReceiptFromGooglePlayStore() (only available on Android)", 2);
			}
		}

		public void setReceiptFromAppleAppStoreBase64EncodedString(string value)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				appStoreReceiptBase64EncodedString = value;
			}
			else
			{
				LOG("Event safely ignoring setReceiptFromAppleAppStoreBase64EncodedString() (only available on iOS)", 2);
			}
		}
	}

	public class Deeplink
	{
		public readonly string destination;

		public readonly Dictionary<string, object> raw;

		internal Deeplink(string destination, Dictionary<string, object> raw)
		{
			this.destination = destination;
			this.raw = raw;
		}

		internal Deeplink(Dictionary<string, object> json)
		{
			object value;
			json.TryGetValue("destination", out value);
			object value2;
			json.TryGetValue("raw", out value2);
			destination = (string)value;
			raw = (Dictionary<string, object>)value2;
		}
	}

	public class PlatformEvent
	{
		public readonly string name;

		public readonly Dictionary<string, object> value;

		internal PlatformEvent(string name, Dictionary<string, object> value)
		{
			this.name = name;
			this.value = value;
		}

		internal PlatformEvent(Dictionary<string, object> json)
		{
			object obj;
			json.TryGetValue("name", out obj);
			object obj2;
			json.TryGetValue("value", out obj2);
			name = (string)obj;
			value = (Dictionary<string, object>)obj2;
		}
	}

	private class Util
	{
		private static bool unableToFormatTime;

		public static double GetNetworkRetryTime(int failedCount)
		{
			if (failedCount <= 1)
			{
				return 3.0;
			}
			switch (failedCount)
			{
			case 2:
				return 10.0;
			case 3:
				return 30.0;
			case 4:
				return 60.0;
			default:
				return 300.0;
			}
		}

		public static string DateTimeToIso8601(DateTime dateTime)
		{
			try
			{
				return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				return "";
			}
		}

		public static string TimeStamp()
		{
			try
			{
				return unableToFormatTime ? "" : string.Format(CultureInfo.InvariantCulture, "{0:hh:mm:ss.fff}", DateTime.Now);
			}
			catch (Exception)
			{
				unableToFormatTime = true;
				return "";
			}
		}

		public static double UnixTime()
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return (DateTime.UtcNow - dateTime).TotalSeconds;
		}

		public static void SetValue(ref Dictionary<string, object> source, string key, object value, bool doNotReplace = true, bool doNotAllowEmptyString = true)
		{
			if (source == null || value == null)
			{
				return;
			}
			try
			{
				if (source.ContainsKey(key))
				{
					if (doNotReplace)
					{
						return;
					}
					source.Remove(key);
				}
				if (!doNotAllowEmptyString || !(value is string) || value.ToString().Length >= 1)
				{
					source.Add(key, value);
				}
			}
			catch (Exception ex)
			{
				LOG("SetValue() failed for key=" + key + ": " + ex.Message, 1);
			}
		}

		public static byte[] hexStringToByteArr(string hexString)
		{
			if (hexString == null || hexString.Length < 2 || hexString.Length % 2 != 0)
			{
				return null;
			}
			try
			{
				byte[] array = new byte[hexString.Length / 2];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
				}
				return array;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}

	private static class Json
	{
		public static T ToObj<T>(string jsonString)
		{
			return JsonReader.Deserialize<T>(jsonString);
		}

		public static string ToString(object jsonObject)
		{
			return JsonWriter.Serialize(jsonObject);
		}
	}

	public delegate bool NetworkSuccessCallback(string response);

	public delegate void NetworkFailCallback(string errorMessage);

	private static class Settings
	{
		public const string sdkVersion = "Unity NET 4.4.1";

		public const string sdkProtocol = "7";

		public const double initWait = 60.0;

		public const double getAttributionWait = 7.0;

		public const double flushWait = 0.0;

		public const float WakeTime = 0.1f;

		public const double PauseHold = 60.0;

		public const double PauseUpdate = 10.0;

		public const int maxQueueSize = 500;

		public static string apiURLInit = "https://kvinit-prod.api.kochava.com";

		public static string apiURLTracker = "https://control.kochava.com";

		public static string apiURLQuery = "https://control.kochava.com";

		public static string trackerURL = "/track/json";

		public static string initURL = "/track/kvinit";

		public static string queryURL = "/track/kvquery";

		public const string keyProfile = "kou__profile";

		public const string keyQueue = "kou__queue";

		public const string keyAttribution = "kou__attribution";

		public static string advertisingIDKey = "ko_advertising_id";

		public const string temporaryQueueKey = "ko_temp_queue";
	}

	private class Profile
	{
		public bool persisted;

		public string kochavaDeviceID;

		public bool attributionReceived;

		public bool initialComplete;

		public double initLastSent;

		public bool initComplete;

		public double initWait;

		public double flushWait;

		public string[] blacklist;

		public string[] eventname_blacklist;

		public string[] whitelist;

		public bool sessionTracking;

		public double getAttributionWait;

		public bool sendUpdates;

		public Dictionary<string, object> watchlist;

		public string appGUID;

		public string overrideAppGUID;

		public string sentIdentityLinks;

		public Profile()
		{
			sentIdentityLinks = "";
			appGUID = _appGUID;
			overrideAppGUID = "";
			sessionTracking = true;
			sendUpdates = true;
			initWait = 60.0;
			flushWait = 0.0;
			getAttributionWait = 7.0;
			watchlist = new Dictionary<string, object>
			{
				{
					Settings.advertisingIDKey,
					""
				},
				{ "os_version", "" },
				{ "language", "" },
				{ "app_limit_tracking", false },
				{ "app_version", "" },
				{ "device_limit_tracking", false }
			};
		}

		public void Save()
		{
			string text = Json.ToString(this);
			PlayerPrefs.SetString("kou__profile", text);
			LOG("WRITE PERSISTED (kou__profile): " + text, 5);
		}
	}

	private class NetworkManager : MonoBehaviour
	{
		private List<NetworkRequest> networkRequests;

		public NetworkManager()
		{
			networkRequests = new List<NetworkRequest>();
		}

		public void NewRequest(string _name, string _data)
		{
			switch (_name)
			{
			case "init":
				networkRequests.Add(new NetworkRequest(instance, _name, Settings.apiURLInit + Settings.initURL, _data, instance.NetSuccessInit, instance.NetFailInit));
				break;
			case "get_attribution":
				networkRequests.Add(new NetworkRequest(instance, _name, Settings.apiURLQuery + Settings.queryURL, _data, instance.NetSuccessAttribution, instance.NetFailAttribution));
				break;
			case "initial":
				networkRequests.Add(new NetworkRequest(instance, _name, Settings.apiURLTracker + Settings.trackerURL, _data, instance.NetSuccessInitial, instance.NetFailInitial));
				break;
			default:
				networkRequests.Add(new NetworkRequest(instance, _name, Settings.apiURLTracker + Settings.trackerURL, _data, instance.NetSuccessQueue, instance.NetFailQueue));
				break;
			}
		}

		public int requestCount(int requestStatus)
		{
			return networkRequests.FindAll((NetworkRequest it) => it.status == requestStatus).Count;
		}

		public int requestCount()
		{
			return networkRequests.Count;
		}

		public void Process()
		{
			networkRequests.RemoveAll((NetworkRequest it) => it.status == 2);
			if (networkRequests.FindAll((NetworkRequest it) => it.status == 1).Count != 0)
			{
				return;
			}
			foreach (NetworkRequest networkRequest in networkRequests)
			{
				if (networkRequest.status == 0)
				{
					networkRequest.status = 1;
					LOG("Starting next queued network request: " + networkRequest.name, 5);
					StartCoroutine(StartHttpRequest(networkRequest));
					break;
				}
			}
		}

        [Obsolete]
        private IEnumerator StartHttpRequest(NetworkRequest sourceRequest)
		{
			sourceRequest.status = 1;
			string data = sourceRequest.data;
			LOG("HTTP " + sourceRequest.name + " PAYLOAD: " + data, 4);
			UnityWebRequest www = new UnityWebRequest(sourceRequest.url, "POST");
			www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
			www.downloadHandler = new DownloadHandlerBuffer();
			www.SetRequestHeader("Content-Type", "application/json");
			yield return www.SendWebRequest();
			if (www.isNetworkError || www.isHttpError || !string.IsNullOrEmpty(www.error))
			{
				LOG("HTTP " + sourceRequest.name + " ERROR: " + www.error, 4);
				NetFailed();
				sourceRequest.networkFailCallback(www.error ?? "");
			}
			else if (www.downloadHandler.text != "")
			{
				try
				{
					LOG("HTTP " + sourceRequest.name + " SUCCESS", 5);
					if (sourceRequest.networkSuccessCallback(www.downloadHandler.text))
					{
						NetSuccess();
					}
					else
					{
						NetFailed();
					}
				}
				catch (Exception ex)
				{
					LOG("HTTP " + sourceRequest.name + " FAILED Json deserialization: " + ex.Message, 4);
					sourceRequest.networkFailCallback(ex.Message);
				}
			}
			sourceRequest.status = 2;
			WakeController();
		}
	}

	private class NetworkRequest
	{
		public int status;

		public string name;

		public string url;

		public string data;

		public Kochava thisTracker;

		public NetworkSuccessCallback networkSuccessCallback;

		public NetworkFailCallback networkFailCallback;

		public NetworkRequest(Kochava _thisTracker, string _name, string _url, string _data, NetworkSuccessCallback successCallback, NetworkFailCallback failCallback)
		{
			status = 0;
			name = _name;
			url = _url;
			data = _data;
			thisTracker = _thisTracker;
			LOG("New network request: " + name, 5);
			networkSuccessCallback = successCallback;
			networkFailCallback = failCallback;
		}

		~NetworkRequest()
		{
			LOG("networkRequest destructor", 5);
		}
	}

	private class Item
	{
		public class Properties
		{
			public string action;

			public bool wait;

			public bool instant;

			public bool unique;

			public bool temporary;

			public double hold;

			public int sendStatus;

			public bool incomplete = true;
		}

		public Properties properties;

		public Dictionary<string, object> top;

		public Dictionary<string, object> data;

		public Item(string _action)
		{
			properties = new Properties();
			properties.action = _action;
			top = new Dictionary<string, object>();
			data = new Dictionary<string, object>();
		}

		public string Stringify()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> item in top)
			{
				dictionary.Add(item.Key, item.Value);
			}
			if (data.Count > 0)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				foreach (KeyValuePair<string, object> datum in data)
				{
					dictionary2.Add(datum.Key, datum.Value);
				}
				dictionary.Add("data", dictionary2);
			}
			return Json.ToString(dictionary);
		}
	}

	private class ItemQueue
	{
		public List<Item> queue;

		public bool flush;

		public double nextFlush;

		public bool mutated;

		public void Add(Item ev)
		{
			if (queue.Count > 500)
			{
				LOG("Event queue limit reached (" + queue.Count + "/" + 500 + ") event dropped", 1);
				return;
			}
			if (ev.properties.unique)
			{
				foreach (Item item in queue)
				{
					if (item.properties.action == ev.properties.action)
					{
						LOG("Unique event already exists (" + ev.properties.action + ")", 4);
						return;
					}
				}
			}
			queue.Add(ev);
			Save();
			WakeController();
		}

		public void Resend()
		{
			LOG("Resetting all sent items status to ready", 4);
			foreach (Item item in queue)
			{
				if (item.properties.sendStatus == 2)
				{
					item.properties.sendStatus = 1;
				}
			}
			Save();
		}

		public void Save()
		{
			mutated = false;
			try
			{
				ItemPersistedAll itemPersistedAll = new ItemPersistedAll();
				int num = 0;
				foreach (Item item in queue)
				{
					if (!item.properties.temporary)
					{
						ItemPersisted itemPersisted = new ItemPersisted();
						itemPersisted.properties = Json.ToString(item.properties);
						itemPersisted.top = Json.ToString(item.top);
						itemPersisted.data = Json.ToString(item.data);
						Array.Resize(ref itemPersistedAll.items, num + 1);
						itemPersistedAll.items[num++] = Json.ToString(itemPersisted);
					}
				}
				if (num > 0)
				{
					LOG("Saving (" + num + ") events to persisted queue...", 5);
					string value = Json.ToString(itemPersistedAll);
					PlayerPrefs.SetString("kou__queue", value);
				}
				else
				{
					LOG("No events to persist, clearing persisted key", 5);
					PlayerPrefs.DeleteKey("kou__queue");
				}
			}
			catch (Exception ex)
			{
				LOG("EventQueue.Save() failed: " + ex.Message, 1);
			}
		}

		public void Load()
		{
			try
			{
				if (PlayerPrefs.HasKey("kou__queue"))
				{
					ItemPersistedAll itemPersistedAll = Json.ToObj<ItemPersistedAll>(PlayerPrefs.GetString("kou__queue"));
					LOG("Loading (" + itemPersistedAll.items.Length + ") items from persisted queue...", 4);
					for (int i = 0; i < itemPersistedAll.items.Length; i++)
					{
						ItemPersisted itemPersisted = Json.ToObj<ItemPersisted>(itemPersistedAll.items[i]);
						Item item = new Item("");
						item.properties = Json.ToObj<Item.Properties>(itemPersisted.properties);
						item.top = Json.ToObj<Dictionary<string, object>>(itemPersisted.top);
						item.data = Json.ToObj<Dictionary<string, object>>(itemPersisted.data);
						item.properties.sendStatus = 1;
						item.properties.incomplete = false;
						if (i < 3)
						{
							LOG(("Loaded item: " + item.properties.action) ?? "", 4);
						}
						queue.Add(item);
					}
				}
				else
				{
					LOG("Persisted queue is empty (no items found)", 4);
				}
			}
			catch (Exception ex)
			{
				LOG("EventQueue.Load() failed: " + ex.Message, 1);
			}
		}

		public ItemQueue()
		{
			queue = new List<Item>();
		}

		public void Process()
		{
			if (instance.itemQueue != null && instance.itemQueue.queue.Count > 0 && instance.networkManager.requestCount(1) == 0)
			{
				if (instance.itemQueue.nextFlush <= Util.UnixTime() && instance.itemQueue.nextFlush > 0.0)
				{
					LOG("Batch timer reached, flush queue now", 4);
					flush = true;
					nextFlush = 0.0;
				}
				bool flag = false;
				bool flag2 = false;
				foreach (Item item in queue)
				{
					if (instance.profile.initComplete)
					{
						if (item.properties.incomplete)
						{
							instance.ConstructPayload(item);
							if (!item.properties.incomplete)
							{
								mutated = true;
							}
						}
						if (item.properties.incomplete)
						{
							LOG(("QUEUE: Item payload still incomplete: " + item.properties.action) ?? "", 4);
							WakeController(2.0);
						}
					}
					if (item.properties.wait)
					{
						flag = true;
						LOG(("QUEUE: Awaiting item: " + item.properties.action) ?? "", 5);
					}
					if (item.properties.sendStatus == 0 && !item.properties.incomplete)
					{
						flag2 = true;
						if (flush || item.properties.instant)
						{
							item.properties.sendStatus = 1;
							LOG(("QUEUE: Item ready: " + item.properties.action) ?? "", 5);
							mutated = true;
						}
						else if (!flush && nextFlush == 0.0)
						{
							LOG("QUEUE: Item(s) are queued, will flush " + instance.profile.flushWait + "s from now...", 4);
							nextFlush = Util.UnixTime() + instance.profile.flushWait;
						}
					}
				}
				if (!flag2 && flush)
				{
					LOG("QUEUE: Done flushing", 4);
					flush = false;
				}
				foreach (Item item2 in queue)
				{
					if (item2.properties.sendStatus == 1 && (!flag || item2.properties.wait))
					{
						if (Util.UnixTime() >= item2.properties.hold)
						{
							LOG(("QUEUE: Sending event: " + item2.properties.action) ?? "", 4);
							item2.properties.sendStatus = 2;
							mutated = true;
							string data = item2.Stringify();
							string action = item2.properties.action;
							instance.networkManager.NewRequest(action, data);
							break;
						}
						WakeController(item2.properties.hold - Util.UnixTime());
						if (flag)
						{
							break;
						}
					}
				}
				if (mutated)
				{
					queue.RemoveAll((Item it) => it.properties.sendStatus == 3);
					Save();
				}
			}
			if (nextFlush > 0.0)
			{
				WakeController(nextFlush - Util.UnixTime());
			}
		}
	}

	private class ItemPersisted
	{
		public string properties;

		public string top;

		public string data;
	}

	private class ItemPersistedAll
	{
		public string[] items;
	}

	private class CacheManager
	{
		public string key = "";

		public double expires;

		public double staleness;

		public object value;

		public CacheManager(string _key, double _staleness)
		{
			key = _key;
			expires = 0.0;
			staleness = _staleness;
		}
	}

	private class DeviceInfo
	{
		private static bool asyncComplete = false;

		private static bool asynGotAdvertisingID = false;

		private static string advertisingId = "";

		public static object advertisingIdentifier
		{
			get
			{
				if (!asynGotAdvertisingID)
				{
					return "";
				}
				return advertisingId;
			}
		}

		public static object uniqueIdentifier
		{
			get
			{
				return SystemInfo.deviceUniqueIdentifier;
			}
		}

		public static object dispW
		{
			get
			{
				return Screen.width;
			}
		}

		public static object dispH
		{
			get
			{
				return Screen.height;
			}
		}

		public static object platform
		{
			get
			{
				if (Application.isEditor)
				{
					return "UnityEditor";
				}
				return "WindowsDesktop";
			}
		}

		public static object architecture
		{
			get
			{
				return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
			}
		}

		public static object orientation
		{
			get
			{
				if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
				{
					return "landscape";
				}
				if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || Screen.width < Screen.height)
				{
					return "portrait";
				}
				return "landscape";
			}
		}

		[DllImport("KochavaShimWindows")]
		private static extern IntPtr getWaid();

		public static void Initialize()
		{
			LOG("DeviceInfo initialization started...", 4);
			string text = "";
			try
			{
				LOG("WAID collection started..", 4);
				text = Marshal.PtrToStringAuto(getWaid());
			}
			catch (Exception ex)
			{
				LOG("WAID collection failed: " + ex.Message, 4);
				text = "";
			}
			gotAdIdCallback(text, true, "");
		}

		private static void gotAdIdCallback(string _advertisingId, bool _trackingEnabled, string errorMsg)
		{
			LOG("GOT ADVERTISING ID: " + _advertisingId + " Enabled:" + _trackingEnabled + " " + errorMsg, 4);
			advertisingId = _advertisingId;
			asynGotAdvertisingID = true;
			isReady();
		}

		public static bool isReady()
		{
			if (!asynGotAdvertisingID)
			{
				return false;
			}
			if (!asyncComplete)
			{
				asyncComplete = true;
				LOG("DeviceInfo initialization complete", 4);
			}
			return true;
		}
	}

	private const string sdkVersionNumber = "4.4.1";

	private const string sdkNativeName = "Unity NET";

	private const string sdkName = "Unity";

	private const string sdkBuildDate = "2022-10-11T18:18:06Z";

	private static Kochava instance = null;

	private string instanceId = Guid.NewGuid().ToString().Substring(0, 5) + "-";

	private GameObject kochavaGameObject;

	private static bool initialized = false;

	private static string _appGUID = "";

	private static string _instantAppGUID = "";

	private static bool _appAdLimitTracking = false;

	private static bool _appAdLimitTrackingSet = false;

	private static bool _sleep = false;

	private static int _logLevel;

	private static string _partnerName = "";

	private static string _partnerApp = "";

	private static bool _requestAttributionCallback = false;

	private static string _containerAppGroupIdentifier = "";

	private static bool _intelligentConsentManagement = false;

	private static bool _manualManagedConsentRequirements = false;

	private static Dictionary<string, object> _dictionaryCustom = new Dictionary<string, object>();

	private static AttributionCallbackDelegate _attributionCallbackDelegate;

	private static ConsentStatusChangeDelegate _consentStatusChangeDelegate;

	private static Action<PlatformEvent> _platformEventHandler = null;

	private static bool _attEnabled = false;

	private static double _attWaitTime = 30.0;

	private static bool _attAutoRequest = true;

	private static string _productName = "";

	private const int LOG_NONE = 0;

	private const int LOG_ERROR = 1;

	private const int LOG_WARN = 2;

	private const int LOG_INFO = 3;

	private const int LOG_DEBUG = 4;

	private const int LOG_TRACE = 5;

	private const int NR_QUEUED = 0;

	private const int NR_BUSY = 1;

	private const int NR_ENDED = 2;

	private const int SEND_QUEUED = 0;

	private const int SEND_READY = 1;

	private const int SEND_SENT = 2;

	private const int SEND_COMPLETE = 3;

	private bool sessionInitComplete;

	private bool sessionPaused;

	private double sessionPausedTime;

	private double sessionPauseLastUpdated;

	private double sessionPauseUpdateWait;

	private NetworkManager networkManager;

	private List<Item> intakeQueue;

	private Profile profile;

	private ItemQueue itemQueue;

	private double wakeTime;

	private double timeStart;

	private double networkFailedWait;

	private int networkFailedCount;

	private List<CacheManager> cache;

	public static bool IsInitialized()
	{
		return initialized;
	}

	private static void LOG(string msg, int msglogLevel)
	{
		if (_logLevel >= msglogLevel)
		{
			string text = "[Kochava][" + Util.TimeStamp() + "]";
			switch (msglogLevel)
			{
			case 1:
				text += "[ERROR]";
				break;
			case 2:
				text += "[WARNING]";
				break;
			case 3:
				text += "[I]";
				break;
			case 4:
				text += "[D]";
				break;
			case 5:
				text += "[T]";
				break;
			}
			text = text + " " + msg;
			UnityEngine.Debug.Log(text);
		}
	}

	private void UnityNetStart()
	{
		if (_dictionaryCustom != null)
		{
			foreach (KeyValuePair<string, object> item in _dictionaryCustom)
			{
				try
				{
					if ("apiURLInit" == item.Key && item.Value is string)
					{
						Settings.apiURLInit = (string)item.Value;
					}
					else if ("apiURLQuery" == item.Key && item.Value is string)
					{
						Settings.apiURLQuery = (string)item.Value;
					}
					else if ("apiURLTracker" == item.Key && item.Value is string)
					{
						Settings.apiURLTracker = (string)item.Value;
					}
					else if ("initURL" == item.Key && item.Value is string)
					{
						Settings.initURL = (string)item.Value;
					}
					else if ("trackerURL" == item.Key && item.Value is string)
					{
						Settings.trackerURL = (string)item.Value;
					}
					else if ("queryURL" == item.Key && item.Value is string)
					{
						Settings.queryURL = (string)item.Value;
					}
				}
				catch (Exception ex)
				{
					LOG("Custom value payload error: " + ex.Message, 1);
				}
			}
		}
		StartCoroutine(MainLoop());
	}

	public IEnumerator MainLoop()
	{
		while (true)
		{
			if (Util.UnixTime() >= wakeTime && wakeTime >= 0.0 && instance != null)
			{
				Controller();
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			SessionPause();
		}
		else
		{
			SessionResume();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			SessionPause();
		}
		else
		{
			SessionResume();
		}
	}

	private void OnApplicationQuit()
	{
		SessionPause(true);
	}

	private void SessionPause(bool exiting = false)
	{
		if (initialized && profile != null && profile.sessionTracking && !sessionPaused)
		{
			sessionPaused = true;
			sessionPausedTime = Util.UnixTime();
			CreateSessionPauseEvent();
			LOG("SESSION PAUSE - EXIT", 4);
		}
	}

	private void CreateSessionPauseEvent()
	{
		Item item = new Item("session");
		item.data.Add("state", "pause");
		ConstructPayload(item);
		PlayerPrefs.SetString("ko_temp_queue", item.Stringify());
		LOG("UPDATING SESSION PAUSE (to temporary queue)", 4);
	}

	private void SessionResume()
	{
		if (!initialized || profile == null || !profile.sessionTracking || !sessionPaused)
		{
			return;
		}
		sessionPaused = false;
		itemQueue.flush = false;
		double num = Util.UnixTime() - sessionPausedTime;
		if (num < 60.0)
		{
			LOG("Skipping pause/resume (too fast @ " + (int)num + "/" + 60.0 + "s)", 4);
			WakeController();
			if (PlayerPrefs.HasKey("ko_temp_queue"))
			{
				PlayerPrefs.DeleteKey("ko_temp_queue");
			}
			sessionPauseUpdateWait = 0.0;
		}
		else
		{
			ImportTemporaryQueue();
			LOG("SESSION RESUME (" + (int)num + " s)", 4);
			timeStart = Util.UnixTime();
			Item item = new Item("session");
			item.data.Add("state", "resume");
			intakeQueue.Add(item);
			itemQueue.flush = true;
			WakeController();
		}
	}

	private void ImportTemporaryQueue(bool intoEventQueue = false)
	{
		if (!PlayerPrefs.HasKey("ko_temp_queue"))
		{
			return;
		}
		string @string = PlayerPrefs.GetString("ko_temp_queue", "");
		PlayerPrefs.DeleteKey("ko_temp_queue");
		Dictionary<string, object> dictionary = Json.ToObj<Dictionary<string, object>>(@string);
		if (dictionary.ContainsKey("action") && dictionary.ContainsKey("data"))
		{
			string text = dictionary["action"].ToString();
			LOG("Temporary queued item found (" + text + ") " + @string, 4);
			Item item = new Item(text);
			item.data = (Dictionary<string, object>)dictionary["data"];
			dictionary.Remove("data");
			item.top = dictionary;
			item.properties.sendStatus = 1;
			if (!intoEventQueue)
			{
				intakeQueue.Add(item);
			}
			else
			{
				itemQueue.queue.Add(item);
			}
			WakeController();
		}
	}

	private void IngestEvent(string name, string data, string receipt)
	{
		Item item = new Item("event");
		item.data.Add("event_name", name);
		item.data.Add("event_data", data);
		if (receipt.Length > 0)
		{
			item.data.Add("receipt", receipt);
		}
		intakeQueue.Add(item);
		if (initialized)
		{
			WakeController();
		}
	}

	private void IngestEvent(Event kochavaEvent)
	{
		Item item = new Item("event");
		item.data.Add("event_name", kochavaEvent.getEventName());
		item.data.Add("event_data", kochavaEvent.getDictionary());
		intakeQueue.Add(item);
		if (initialized)
		{
			WakeController();
		}
	}

	private void IngestDeeplink(string deepLinkURI, string sourceApplication)
	{
		Event @event = new Event(EventType.DeepLink);
		if (deepLinkURI != null)
		{
			@event.uri = deepLinkURI;
		}
		if (sourceApplication != null)
		{
			@event.source = sourceApplication;
		}
		IngestEvent(@event);
		if (initialized)
		{
			WakeController();
		}
	}

	private void IngestIdentityLink(Dictionary<string, string> identityLinkDictionary)
	{
		Item item = new Item("identityLink");
		item.data = Json.ToObj<Dictionary<string, object>>(Json.ToString(identityLinkDictionary));
		item.properties.instant = true;
		intakeQueue.Add(item);
		if (initialized)
		{
			WakeController();
		}
	}

	private static void WakeController(double secondsFromNow = 0.0)
	{
		if (secondsFromNow < 0.0)
		{
			secondsFromNow = 0.0;
		}
		if (instance.wakeTime < 0.0 || Util.UnixTime() + secondsFromNow < instance.wakeTime)
		{
			instance.wakeTime = Util.UnixTime() + secondsFromNow;
		}
	}

	private void Controller()
	{
		wakeTime = -1.0;
		LOG("CONTROLLER RUN", 5);
		ProcessInitialize();
		if (!initialized)
		{
			return;
		}
		if (!DeviceInfo.isReady())
		{
			WakeController();
			return;
		}
		if (profile.sessionTracking && profile.initComplete)
		{
			UpdatePauseSession();
		}
		ProcessIntakeQueue();
		if (networkFailedWait > Util.UnixTime())
		{
			double num = networkFailedWait - Util.UnixTime();
			LOG("Controller holding, retry in " + num + " s...", 5);
			WakeController(num);
			return;
		}
		itemQueue.Process();
		networkManager.Process();
		if (wakeTime < 0.0 && networkManager.requestCount() < 1)
		{
			LOG("Controller IDLE", 4);
		}
		else if (networkManager.requestCount() < 1)
		{
			LOG("Controller WAITING (wake again in " + (wakeTime - Util.UnixTime()) + " s)", 4);
		}
	}

	private void CompleteInit()
	{
		sessionInitComplete = true;
		LOG("INIT: Completing", 4);
		CheckWatchlist();
	}

	private static void NetFailed()
	{
		instance.networkFailedCount++;
		double networkRetryTime = Util.GetNetworkRetryTime(instance.networkFailedCount);
		instance.networkFailedWait = Util.UnixTime() + networkRetryTime;
		LOG("Network call failed (" + instance.networkFailedCount + ") next retry in " + networkRetryTime + "s...", 4);
		instance.itemQueue.Resend();
	}

	private static void NetSuccess()
	{
		instance.networkFailedCount = 0;
		instance.networkFailedWait = 0.0;
		instance.itemQueue.queue.RemoveAll((Item it) => it.properties.sendStatus == 2);
		instance.itemQueue.Save();
	}

	private bool NetSuccessInit(string result)
	{
		LOG("HTTP INIT RESPONSE: " + result, 4);
		Dictionary<string, object> dictionary = Json.ToObj<Dictionary<string, object>>(result);
		try
		{
			if (dictionary.ContainsKey("error"))
			{
				LOG("INIT ERROR: " + dictionary["error"], 1);
				return false;
			}
		}
		catch (Exception ex)
		{
			LOG("Failed to parse init response error: " + ex.Message, 2);
			return false;
		}
		try
		{
			if (dictionary.ContainsKey("success") && (string)dictionary["success"] != "1")
			{
				return false;
			}
		}
		catch (Exception ex2)
		{
			LOG("Failed to parse init response success: " + ex2.Message, 2);
			return false;
		}
		try
		{
			if (dictionary.ContainsKey("msg") && dictionary["msg"].GetType().GetElementType() == typeof(string))
			{
				string[] array = (string[])dictionary["msg"];
				foreach (string text in array)
				{
					LOG("INIT Message: " + text, 2);
				}
			}
		}
		catch (Exception ex3)
		{
			LOG("Failed to parse init response msg[]: " + ex3.Message, 2);
		}
		try
		{
			if (dictionary.ContainsKey("flags"))
			{
				Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary["flags"];
				if (dictionary2.ContainsKey("kochava_app_id"))
				{
					profile.appGUID = dictionary2["kochava_app_id"].ToString();
					profile.overrideAppGUID = profile.appGUID;
					LOG("Updated (override) kochava_app_id to: " + profile.appGUID, 4);
				}
				if (dictionary2.ContainsKey("kochava_device_id"))
				{
					profile.kochavaDeviceID = dictionary2["kochava_device_id"].ToString();
					LOG("Updated kochava_device_id to: " + profile.kochavaDeviceID, 4);
				}
				if (dictionary2.ContainsKey("resend_initial") && (bool)dictionary2["resend_initial"])
				{
					profile.initialComplete = false;
					LOG("Resend initial: true", 4);
					LOG("Resending initial...", 4);
					Item item = new Item("initial");
					item.properties.unique = true;
					item.properties.wait = true;
					item.properties.instant = true;
					ConstructPayload(item);
					intakeQueue.Add(item);
				}
				if (dictionary2.ContainsKey("session_tracking") && dictionary2["session_tracking"].ToString() == "none")
				{
					profile.sessionTracking = false;
					LOG("Session tracking OFF", 4);
				}
				else
				{
					profile.sessionTracking = true;
				}
				if (dictionary2.ContainsKey("getattribution_wait"))
				{
					profile.getAttributionWait = double.Parse(dictionary2["getattribution_wait"].ToString());
					LOG("Get attribution wait: " + profile.getAttributionWait + " s", 4);
				}
				if (dictionary2.ContainsKey("kvinit_wait"))
				{
					profile.initWait = double.Parse(dictionary2["kvinit_wait"].ToString());
					LOG("Init wait: " + profile.initWait + " s", 4);
				}
				if (dictionary2.ContainsKey("kvtracker_wait"))
				{
					profile.flushWait = double.Parse(dictionary2["kvtracker_wait"].ToString());
					LOG("Tracker wait: " + profile.flushWait + " s", 4);
				}
				if (dictionary2.ContainsKey("send_updates"))
				{
					profile.sendUpdates = (bool)dictionary2["send_updates"];
					LOG("Send Updates: " + profile.sendUpdates, 4);
				}
			}
		}
		catch (Exception ex4)
		{
			LOG("Failed to parse init response flags: " + ex4.Message, 2);
		}
		try
		{
			if (dictionary.ContainsKey("blacklist") && dictionary["blacklist"].GetType().GetElementType() == typeof(string))
			{
				profile.blacklist = (string[])dictionary["blacklist"];
				LOG("Blacklist items: " + profile.blacklist.Length, 5);
			}
		}
		catch (Exception ex5)
		{
			LOG("Failed to parse init response blacklist[]: " + ex5.Message, 2);
		}
		try
		{
			if (dictionary.ContainsKey("eventname_blacklist") && dictionary["eventname_blacklist"].GetType().GetElementType() == typeof(string))
			{
				profile.eventname_blacklist = (string[])dictionary["eventname_blacklist"];
				LOG("Eventname_blacklist items: " + profile.eventname_blacklist.Length, 5);
			}
		}
		catch (Exception ex6)
		{
			LOG("Failed to parse init response eventname_blacklist[]: " + ex6.Message, 2);
		}
		try
		{
			if (dictionary.ContainsKey("whitelist") && dictionary["whitelist"].GetType().GetElementType() == typeof(string))
			{
				profile.whitelist = (string[])dictionary["whitelist"];
				for (int j = 0; j < profile.whitelist.Length; j++)
				{
					LOG("Whitelisted: " + profile.whitelist[j], 4);
				}
			}
		}
		catch (Exception ex7)
		{
			LOG("Failed to parse init response whitelist[]: " + ex7.Message, 2);
		}
		profile.initLastSent = Util.UnixTime();
		profile.initComplete = true;
		profile.Save();
		CompleteInit();
		return true;
	}

	private void NetFailInit(string errorMessage)
	{
		LOG("HTTP INIT FAILED: " + errorMessage, 4);
		if (profile.initComplete)
		{
			LOG("Using previous init response...", 4);
			itemQueue.queue.RemoveAll((Item it) => it.properties.action == "init");
			NetSuccess();
			CompleteInit();
		}
	}

	private bool NetSuccessInitial(string result)
	{
		LOG("HTTP INITIAL RESPONSE: " + result, 4);
		try
		{
			Dictionary<string, object> dictionary = Json.ToObj<Dictionary<string, object>>(result);
			if (dictionary.ContainsKey("success") && (string)dictionary["success"] != "1")
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			LOG("Failed to parse initial response success: " + ex.Message, 2);
			return false;
		}
		profile.initialComplete = true;
		profile.Save();
		foreach (Item item in itemQueue.queue)
		{
			if (item.properties.action == "get_attribution")
			{
				LOG("Adjusting attribution request from now", 4);
				item.properties.hold = Util.UnixTime() + profile.getAttributionWait;
				break;
			}
		}
		return true;
	}

	private void NetFailInitial(string errorMessage)
	{
		LOG("HTTP INITIAL FAILED: " + errorMessage, 4);
	}

	private void ResetAttributionRequest(double secs)
	{
		foreach (Item item in itemQueue.queue)
		{
			if (item.properties.action == "get_attribution")
			{
				LOG("Adjusting attribution request to " + secs + " s from now", 4);
				item.properties.sendStatus = 1;
				item.properties.hold = Util.UnixTime() + secs;
				break;
			}
		}
	}

	private bool NetSuccessAttribution(string result)
	{
		LOG("HTTP ATTRIBUTION RESPONSE: " + result, 4);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		string text = "";
		try
		{
			dictionary = Json.ToObj<Dictionary<string, object>>(result);
			int num = int.Parse(dictionary["success"].ToString());
			if (!dictionary.ContainsKey("success") || num != 1 || !dictionary.ContainsKey("data"))
			{
				return false;
			}
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2 = (Dictionary<string, object>)dictionary["data"];
			int num2 = 0;
			if (dictionary2.ContainsKey("retry"))
			{
				num2 = int.Parse(dictionary2["retry"].ToString());
				LOG("Request attribution retry: " + num2, 4);
			}
			if (num2 == -1)
			{
				if (dictionary2.ContainsKey("attribution"))
				{
					text = Json.ToString(new Dictionary<string, object> { 
					{
						"attribution",
						dictionary2["attribution"]
					} });
					LOG("Persisting attribution: " + text, 4);
					PlayerPrefs.SetString("kou__attribution", text);
					profile.attributionReceived = true;
					profile.Save();
					try
					{
						LOG("Attribution result received (will pass along to host): " + text, 4);
						if (_attributionCallbackDelegate != null)
						{
							_attributionCallbackDelegate(text);
						}
					}
					catch (Exception ex)
					{
						LOG("NetSuccessAttribution() Failed to trigger attribution callback" + ex.Message, 4);
					}
				}
			}
			else if (num2 >= 0)
			{
				if (num2 == 0)
				{
					ResetAttributionRequest(profile.getAttributionWait);
				}
				else
				{
					ResetAttributionRequest(num2);
				}
				return true;
			}
		}
		catch (Exception ex2)
		{
			LOG("NetSuccessAttribution() Failed Deserialize JSON attribution response: " + ex2.Message, 2);
			profile.attributionReceived = false;
		}
		return true;
	}

	private void NetFailAttribution(string errorMessage)
	{
		LOG("HTTP ATTRIBUTION FAILED: " + errorMessage, 4);
	}

	private bool NetSuccessQueue(string result)
	{
		LOG("HTTP QUEUE RESPONSE: " + result, 4);
		try
		{
			Dictionary<string, object> dictionary = Json.ToObj<Dictionary<string, object>>(result);
			if (dictionary.ContainsKey("success") && (string)dictionary["success"] != "1")
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			LOG("Failed to parse tracker response success: " + ex.Message, 2);
			return false;
		}
		return true;
	}

	private void NetFailQueue(string errorMessage)
	{
		LOG("HTTP QUEUE FAILED: " + errorMessage, 4);
	}

	private object getDataPoint(string key)
	{
		if (cache == null)
		{
			cache = new List<CacheManager>();
			cache.Add(new CacheManager(Settings.advertisingIDKey, -1.0));
			cache.Add(new CacheManager("os_version", -1.0));
			cache.Add(new CacheManager("platform", -1.0));
			cache.Add(new CacheManager("package", -1.0));
			cache.Add(new CacheManager("architecture", -1.0));
			cache.Add(new CacheManager("processor", -1.0));
			cache.Add(new CacheManager("app_short_string", -1.0));
			cache.Add(new CacheManager("app_version", -1.0));
			cache.Add(new CacheManager("device", -1.0));
			cache.Add(new CacheManager("device_cores", -1.0));
			cache.Add(new CacheManager("device_memory", -1.0));
			cache.Add(new CacheManager("graphics_device_name", -1.0));
			cache.Add(new CacheManager("graphics_device_vendor", -1.0));
			cache.Add(new CacheManager("graphics_memory_size", -1.0));
			cache.Add(new CacheManager("graphics_device_version", -1.0));
			cache.Add(new CacheManager("graphics_shader_level", -1.0));
			cache.Add(new CacheManager("language", 300.0));
			cache.Add(new CacheManager("disp_count", 300.0));
		}
		if (profile.blacklist != null && Array.FindAll(profile.blacklist, (string s) => s.Equals(key)).Length != 0)
		{
			LOG("Blacklisted key (skipped): " + key, 4);
			return null;
		}
		foreach (CacheManager item in cache)
		{
			if (key == item.key)
			{
				if (item.expires <= Util.UnixTime() && (item.staleness >= 0.0 || item.value == null))
				{
					item.expires = Util.UnixTime() + item.staleness;
					item.value = getDataPointDirect(key);
				}
				return item.value;
			}
		}
		return getDataPointDirect(key);
	}

	private void ConstructPayload(Item ev)
	{
		LOG("Constructing payload for " + ev.properties.action, 5);
		Util.SetValue(ref ev.top, "action", ev.properties.action);
		ev.properties.incomplete = false;
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		if (ev.properties.action == "init" || profile.initComplete)
		{
			list.Add("kochava_app_id");
			list.Add("kochava_device_id");
		}
		else
		{
			ev.properties.incomplete = true;
		}
		list.Add("sdk_version");
		list.Add("sdk_protocol");
		list.Add("nt_id");
		list2.Add("usertime");
		list2.Add("uptime");
		if (ev.properties.action == "init")
		{
			list2.Add("partner_name");
			list2.Add("package");
			list2.Add("platform");
			list2.Add("os_version");
			list.Add("sdk_build_date");
		}
		if (ev.properties.action == "initial" || ev.properties.action == "event" || ev.properties.action == "session" || ev.properties.action == "deeplink")
		{
			if (!profile.initComplete)
			{
				ev.properties.incomplete = true;
			}
			else
			{
				if (ev.properties.action == "initial")
				{
					if (_dictionaryCustom != null)
					{
						foreach (KeyValuePair<string, object> it in _dictionaryCustom)
						{
							try
							{
								if (profile.whitelist != null && Array.FindAll(profile.whitelist, (string s) => s.Equals(it.Key)).Length != 0)
								{
									LOG("Sending whitelisted key: " + it.Key, 4);
									Util.SetValue(ref ev.data, it.Key, it.Value);
								}
								else
								{
									LOG("Custom key not whitelisted: " + it.Key, 1);
								}
							}
							catch (Exception ex)
							{
								LOG("Custom value payload error: " + ex.Message, 1);
							}
						}
					}
					list2.Add(Settings.advertisingIDKey);
					list2.Add("package");
					list2.Add("language");
					list2.Add("app_limit_tracking");
					list2.Add("device_limit_tracking");
				}
				list2.Add("os_version");
				list2.Add("disp_w");
				list2.Add("disp_h");
				list2.Add("architecture");
				list2.Add("processor");
				list2.Add("app_short_string");
				list2.Add("app_version");
				list2.Add("processor");
				list2.Add("device");
				list2.Add("device_cores");
				list2.Add("device_memory");
				list2.Add("graphics_device_name");
				list2.Add("graphics_device_vendor");
				list2.Add("graphics_memory_size");
				list2.Add("graphics_device_version");
				list2.Add("graphics_shader_level");
				list2.Add("device_orientation");
				list2.Add("product_name");
				list2.Add("connected_devices");
				list2.Add("disp_count");
			}
		}
		foreach (string item in list)
		{
			try
			{
				Util.SetValue(ref ev.top, item, getDataPoint(item));
			}
			catch (Exception ex2)
			{
				LOG("ConstructPayload - keys failed (" + item + "): " + ex2.Message, 4);
			}
		}
		if (list2.Count <= 0)
		{
			return;
		}
		foreach (string item2 in list2)
		{
			try
			{
				Util.SetValue(ref ev.data, item2, getDataPoint(item2));
			}
			catch (Exception ex3)
			{
				LOG("ConstructPayload - keysData failed (" + item2 + "): " + ex3.Message, 4);
			}
		}
	}

	private object getDataPointDirect(string key)
	{
		if (key == Settings.advertisingIDKey)
		{
			return DeviceInfo.advertisingIdentifier;
		}
		switch (key)
		{
		case "disp_count":
			return Display.displays.Length;
		case "uptime":
			return Util.UnixTime() - timeStart;
		case "usertime":
			return (int)Util.UnixTime();
		case "kochava_app_id":
			return profile.appGUID;
		case "app_limit_tracking":
			return _appAdLimitTracking;
		case "nt_id":
			return instance.instanceId + Guid.NewGuid().ToString();
		case "partner_name":
			return _partnerName;
		case "kochava_device_id":
			return profile.kochavaDeviceID;
		case "sdk_protocol":
			return "7";
		case "sdk_build_date":
			return "2022-10-11T18:18:06Z";
		case "sdk_version":
			return "Unity NET 4.4.1";
		case "platform":
			return DeviceInfo.platform;
		case "architecture":
			return DeviceInfo.architecture;
		case "processor":
			return SystemInfo.processorType;
		case "device":
			return SystemInfo.deviceModel;
		case "device_cores":
			return SystemInfo.processorCount;
		case "device_memory":
			return SystemInfo.systemMemorySize;
		case "graphics_device_name":
			return SystemInfo.graphicsDeviceName;
		case "graphics_device_vendor":
			return SystemInfo.graphicsDeviceVendor;
		case "graphics_memory_size":
			return SystemInfo.graphicsMemorySize;
		case "graphics_device_version":
			return SystemInfo.graphicsDeviceVersion;
		case "graphics_shader_level":
			return SystemInfo.graphicsShaderLevel;
		case "disp_w":
			return DeviceInfo.dispW;
		case "disp_h":
			return DeviceInfo.dispH;
		case "app_short_string":
			return _productName;
		case "app_version":
			return _productName;
		case "product_name":
			return SystemInfo.deviceName;
		case "package":
			return _productName;
		case "language":
			return Application.systemLanguage.ToString();
		case "device_orientation":
			return DeviceInfo.orientation;
		case "device_limit_tracking":
			if (((string)DeviceInfo.advertisingIdentifier).Length < 1)
			{
				return true;
			}
			return false;
		case "connected_devices":
		{
			string[] array = Input.GetJoystickNames();
			if (Input.mousePresent)
			{
				Array.Resize(ref array, array.Length + 1);
				array[array.Length - 1] = "Mouse";
			}
			if (array.Length != 0)
			{
				return array;
			}
			return null;
		}
		case "os_version":
			return SystemInfo.operatingSystem;
		default:
			return null;
		}
	}

	private void CheckWatchlist()
	{
		if (!sessionInitComplete || !profile.sendUpdates)
		{
			return;
		}
		LOG("Checking watchlist...", 4);
		try
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> item2 in profile.watchlist)
			{
				if (!(item2.Key == "app_limit_tracking") || _appAdLimitTrackingSet)
				{
					object dataPoint = getDataPoint(item2.Key);
					if (dataPoint != null && !item2.Value.Equals(dataPoint))
					{
						LOG("Watchlist key updated: " + item2.Key + " -> " + dataPoint.ToString(), 4);
						dictionary.Add(item2.Key, dataPoint);
					}
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			Item item = new Item("update");
			item.properties.sendStatus = 1;
			foreach (KeyValuePair<string, object> item3 in dictionary)
			{
				Util.SetValue(ref profile.watchlist, item3.Key, item3.Value, false, false);
				Util.SetValue(ref item.data, item3.Key, item3.Value);
			}
			if (profile.initialComplete)
			{
				LOG("Sending watchlist update...", 4);
				ConstructPayload(item);
				itemQueue.Add(item);
			}
			else
			{
				LOG("Watchlist update skipped (initial not complete)", 4);
			}
			profile.Save();
		}
		catch (Exception ex)
		{
			LOG("Watchlist update error: " + ex.Message, 1);
		}
	}

	private void UpdatePauseSession()
	{
		if (Util.UnixTime() >= sessionPauseLastUpdated + sessionPauseUpdateWait)
		{
			LOG("Updating Pause Event", 4);
			CreateSessionPauseEvent();
			sessionPauseLastUpdated = Util.UnixTime();
			sessionPauseUpdateWait += 10.0;
			if (sessionPauseUpdateWait > 300.0)
			{
				sessionPauseUpdateWait = 300.0;
			}
		}
		WakeController(sessionPauseLastUpdated + sessionPauseUpdateWait - Util.UnixTime());
	}

	public void ProcessIntakeQueue()
	{
		while (intakeQueue.Count > 0)
		{
			LOG("Adding item from intakeQueue: " + intakeQueue[0].properties.action, 5);
			Item intakeItem = intakeQueue[0];
			intakeQueue.RemoveAt(0);
			if (profile.eventname_blacklist != null && intakeItem.properties.action == "event" && Array.FindAll(profile.eventname_blacklist, (string s) => s.Equals(intakeItem.data["event_name"])).Length != 0)
			{
				LOG("Blacklisted event name (dropping it): " + intakeItem.properties.action, 4);
				continue;
			}
			if (intakeItem.properties.action == "identityLink")
			{
				try
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					LOG("Processing Identity Link values...", 4);
					Dictionary<string, string> dictionary2 = Json.ToObj<Dictionary<string, string>>(profile.sentIdentityLinks);
					if (dictionary2 == null)
					{
						dictionary2 = new Dictionary<string, string>();
					}
					foreach (KeyValuePair<string, object> datum in intakeItem.data)
					{
						bool flag = false;
						string value;
						if (dictionary2.ContainsKey(datum.Key) && dictionary2.TryGetValue(datum.Key, out value) && value != null && value == (string)datum.Value)
						{
							flag = true;
						}
						if (!flag)
						{
							dictionary.Add(datum.Key, (string)datum.Value);
						}
					}
					if (dictionary.Count == 0)
					{
						LOG("No updated Identity Link values found (" + dictionary2.Count + " previous)", 4);
						intakeItem = null;
					}
					else
					{
						intakeItem.data.Clear();
						foreach (KeyValuePair<string, string> item in dictionary)
						{
							if (dictionary2.ContainsKey(item.Key))
							{
								dictionary2.Remove(item.Key);
							}
							dictionary2.Add(item.Key, item.Value);
							intakeItem.data.Add(item.Key, item.Value);
						}
						if (dictionary2.Count < 100)
						{
							profile.sentIdentityLinks = Json.ToString(dictionary2);
							profile.Save();
						}
						foreach (Item item2 in itemQueue.queue)
						{
							if (!(item2.properties.action == "initial"))
							{
								continue;
							}
							LOG("Adding new identity link values to Inital", 4);
							Dictionary<string, object> data = intakeItem.data;
							if (item2.data.ContainsKey("identity_link"))
							{
								object value2;
								item2.data.TryGetValue("identity_link", out value2);
								foreach (KeyValuePair<string, object> item3 in (Dictionary<string, object>)value2)
								{
									if (!data.ContainsKey(item3.Key))
									{
										data.Add(item3.Key, (string)item3.Value);
									}
								}
								item2.data.Remove("identity_link");
							}
							item2.data.Add("identity_link", data);
							intakeItem = null;
							break;
						}
					}
				}
				catch (Exception ex)
				{
					LOG("Failed to parse identity link: " + ex.Message, 1);
				}
			}
			if (intakeItem != null)
			{
				ConstructPayload(intakeItem);
				itemQueue.Add(intakeItem);
			}
		}
	}

	public void ProcessInitialize()
	{
		if (initialized)
		{
			return;
		}
		LOG("Initialization sequence started...", 4);
		timeStart = Util.UnixTime();
		DeviceInfo.Initialize();
		if (_productName.Length < 1)
		{
			if (!string.IsNullOrEmpty(_appGUID))
			{
				_productName = _appGUID;
			}
			else
			{
				string input = (_partnerName + "." + _partnerApp + "." + DeviceInfo.platform.ToString()).Replace(" ", "_");
				_productName = new Regex("[^a-zA-Z0-9._]").Replace(input, "");
			}
		}
		Settings.advertisingIDKey = "waid";
		networkManager = base.gameObject.AddComponent<NetworkManager>();
		itemQueue = new ItemQueue();
		intakeQueue = new List<Item>();
		profile = new Profile();
		try
		{
			string @string = PlayerPrefs.GetString("kou__profile");
			if (@string != null && @string.Length > 0)
			{
				profile = Json.ToObj<Profile>(@string);
				LOG("READ PERSISTED DATA (kou__profile): " + @string, 5);
				profile.persisted = true;
				profile.appGUID = _appGUID;
				if (profile.overrideAppGUID.Length > 0)
				{
					profile.appGUID = profile.overrideAppGUID;
				}
				LOG("Loaded persisted profile", 4);
			}
		}
		catch (Exception ex)
		{
			LOG("Error reading persisted data: " + ex.Message, 1);
			profile = new Profile();
		}
		ImportTemporaryQueue(true);
		if (profile.persisted)
		{
			itemQueue.Load();
		}
		else if (itemQueue.queue.Count > 0)
		{
			itemQueue = new ItemQueue();
		}
		bool num = Util.UnixTime() - profile.initLastSent < profile.initWait;
		if (!num && profile.persisted)
		{
			LOG("Using appGUID from previous session: " + profile.appGUID, 5);
		}
		if (!profile.persisted)
		{
			LOG("Creating new profile...", 4);
			profile.kochavaDeviceID = "KU" + Guid.NewGuid().ToString().Replace("-", "");
			LOG("Created new kdid: " + profile.kochavaDeviceID, 4);
		}
		initialized = true;
		LOG("Kochava initialized @ " + (Util.UnixTime() - timeStart) + "s", 3);
		if (num)
		{
			LOG("INIT: Skipping (last was " + (int)(Util.UnixTime() - profile.initLastSent) + " s ago)", 4);
			CompleteInit();
		}
		else
		{
			Item item = new Item("init");
			item.properties.temporary = true;
			item.properties.wait = true;
			item.properties.sendStatus = 1;
			ConstructPayload(item);
			itemQueue.Add(item);
		}
		if (!profile.initialComplete)
		{
			LOG("Initial will send...", 4);
			Item item2 = new Item("initial");
			item2.properties.unique = true;
			item2.properties.wait = true;
			item2.properties.instant = true;
			ConstructPayload(item2);
			itemQueue.Add(item2);
		}
		if (_requestAttributionCallback && !profile.attributionReceived)
		{
			LOG("Attribution will send...", 4);
			Item item3 = new Item("get_attribution");
			item3.properties.temporary = true;
			item3.properties.unique = true;
			item3.properties.sendStatus = 1;
			item3.properties.hold = Util.UnixTime() + profile.getAttributionWait;
			ConstructPayload(item3);
			itemQueue.Add(item3);
		}
		if (profile.persisted && profile.sessionTracking)
		{
			Item item4 = new Item("session");
			item4.data.Add("state", "resume");
			ConstructPayload(item4);
			itemQueue.Add(item4);
		}
	}
}
