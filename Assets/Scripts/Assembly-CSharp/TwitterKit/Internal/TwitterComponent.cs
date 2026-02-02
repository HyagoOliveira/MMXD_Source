using System;
using TwitterKit.Unity;
using UnityEngine;

namespace TwitterKit.Internal
{
	internal class TwitterComponent : MonoBehaviour
	{
		public Action<TwitterSession> loginSuccessAction { get; set; }

		public Action<ApiError> loginFailureAction { get; set; }

		public Action<string> emailSuccessAction { get; set; }

		public Action<ApiError> emailFailureAction { get; set; }

		public Action<string> tweetSuccessAction { get; set; }

		public Action<ApiError> tweetFailureAction { get; set; }

		public Action tweetCancelAction { get; set; }

		public void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}

		public void LoginComplete(string session)
		{
			UnityEngine.Debug.Log("Login request completed");
			if (loginSuccessAction != null)
			{
				UnityEngine.Debug.Log("calling login success action");
				loginSuccessAction(TwitterSession.Deserialize(session));
			}
			else
			{
				UnityEngine.Debug.Log("FAILED calling login success action");
			}
		}

		public void LoginFailed(string error)
		{
			UnityEngine.Debug.Log("Login request failed");
			if (loginFailureAction != null)
			{
				loginFailureAction(ApiError.Deserialize(error));
			}
		}

		public void RequestEmailComplete(string email)
		{
			UnityEngine.Debug.Log("Email request completed");
			if (emailSuccessAction != null)
			{
				emailSuccessAction(email);
			}
		}

		public void RequestEmailFailed(string error)
		{
			UnityEngine.Debug.Log("Email request failed");
			if (emailFailureAction != null)
			{
				emailFailureAction(ApiError.Deserialize(error));
			}
		}

		public void TweetComplete(string tweetId)
		{
			UnityEngine.Debug.Log("Tweet completed");
			if (tweetSuccessAction != null)
			{
				tweetSuccessAction(tweetId);
			}
		}

		public void TweetFailed(string error)
		{
			UnityEngine.Debug.Log("Tweet failed");
			if (tweetFailureAction != null)
			{
				tweetFailureAction(ApiError.Deserialize(error));
			}
		}

		public void TweetCancelled()
		{
			UnityEngine.Debug.Log("Tweet cancelled");
			if (tweetCancelAction != null)
			{
				tweetCancelAction();
			}
		}
	}
}
