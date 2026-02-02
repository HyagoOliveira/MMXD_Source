using System;
using TwitterKit.Internal;
using TwitterKit.Unity.Settings;
using UnityEngine;

namespace TwitterKit.Unity
{
	public sealed class Twitter : ScriptableObject
	{
		private static ITwitter twitter;

		private static GameObject twitterGameObject;

		public static TwitterSession Session
		{
			get
			{
				return twitter.Session();
			}
		}

		public static void AwakeInit()
		{
			twitterGameObject = new GameObject("TwitterGameObject");
			twitterGameObject.AddComponent<TwitterComponent>();
		}

		public static void Init()
		{
			if (string.IsNullOrEmpty(TwitterSettings.ConsumerKey))
			{
				Utils.LogError("Your Twitter App API Key has not been set. To Set: In the main Unity editor navigate to 'Twitter Kit -> Settings' (make sure the Inspector tab is open).");
				return;
			}
			if (string.IsNullOrEmpty(TwitterSettings.ConsumerSecret))
			{
				Utils.LogError("Your Twitter App API Secret has not been set. To Set: In the main Unity editor navigate to 'Twitter Kit -> Settings' (make sure the Inspector tab is open).");
				return;
			}
			if (twitter == null)
			{
				UnityEngine.Debug.Log("twitter is null.");
			}
			twitter.Init(TwitterSettings.ConsumerKey, TwitterSettings.ConsumerSecret);
		}

		public static void LogIn(Action<TwitterSession> successCallback = null, Action<ApiError> failureCallback = null)
		{
			twitterGameObject.GetComponent<TwitterComponent>().loginSuccessAction = successCallback;
			twitterGameObject.GetComponent<TwitterComponent>().loginFailureAction = failureCallback;
			twitter.LogIn();
		}

		public static void LogOut()
		{
			twitter.LogOut();
		}

		public static void RequestEmail(TwitterSession session, Action<string> successCallback = null, Action<ApiError> failureCallback = null)
		{
			twitterGameObject.GetComponent<TwitterComponent>().emailSuccessAction = successCallback;
			twitterGameObject.GetComponent<TwitterComponent>().emailFailureAction = failureCallback;
			twitter.RequestEmail(session);
		}

		public static void Compose(TwitterSession session, string imageUri, string text, string[] hashtags = null, Action<string> successCallback = null, Action<ApiError> failureCallback = null, Action cancelCallback = null)
		{
			twitterGameObject.GetComponent<TwitterComponent>().tweetSuccessAction = successCallback;
			twitterGameObject.GetComponent<TwitterComponent>().tweetFailureAction = failureCallback;
			twitterGameObject.GetComponent<TwitterComponent>().tweetCancelAction = cancelCallback;
			twitter.Compose(session, imageUri, text, hashtags);
		}
	}
}
