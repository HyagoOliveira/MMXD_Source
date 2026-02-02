using System.Collections.Generic;
using UnityEngine;

namespace TwitterKit.Unity.Settings
{
	public class TwitterSettings : ScriptableObject
	{
		public delegate void OnChangeCallback();

		public const string TWITTER_KIT_SETTINGS_ASSET_NAME = "TwitterKitSettings";

		public const string TWITTER_KIT_SETTINGS_PATH = "Twitter/Resources";

		public const string API_KEY_NOT_SET = "Your Twitter App API Key has not been set. To Set: In the main Unity editor navigate to 'Twitter Kit -> Settings' (make sure the Inspector tab is open).";

		public const string API_SECRET_NOT_SET = "Your Twitter App API Secret has not been set. To Set: In the main Unity editor navigate to 'Twitter Kit -> Settings' (make sure the Inspector tab is open).";

		private const string SET_SETTINGS_INFO = "To Set: In the main Unity editor navigate to 'Twitter Kit -> Settings' (make sure the Inspector tab is open).";

		private static TwitterSettings instance;

		private static List<OnChangeCallback> onChangeCallbacks = new List<OnChangeCallback>();

		[SerializeField]
		private string consumerKey = string.Empty;

		[SerializeField]
		private string consumerSecret = string.Empty;

		public static TwitterSettings Instance
		{
			get
			{
				instance = NullableInstance;
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<TwitterSettings>();
				}
				return instance;
			}
		}

		public static TwitterSettings NullableInstance
		{
			get
			{
				if (instance == null)
				{
					instance = Resources.Load("TwitterKitSettings") as TwitterSettings;
				}
				return instance;
			}
		}

		public static string ConsumerKey
		{
			get
			{
				return Instance.consumerKey;
			}
			set
			{
				if (Instance.consumerKey != value)
				{
					Instance.consumerKey = value;
					SettingsChanged();
				}
			}
		}

		public static string ConsumerSecret
		{
			get
			{
				return Instance.consumerSecret;
			}
			set
			{
				if (Instance.consumerSecret != value)
				{
					Instance.consumerSecret = value;
					SettingsChanged();
				}
			}
		}

		public static void RegisterChangeEventCallback(OnChangeCallback callback)
		{
			onChangeCallbacks.Add(callback);
		}

		public static void UnregisterChangeEventCallback(OnChangeCallback callback)
		{
			onChangeCallbacks.Remove(callback);
		}

		private static void SettingsChanged()
		{
			onChangeCallbacks.ForEach(delegate(OnChangeCallback callback)
			{
				callback();
			});
		}
	}
}
