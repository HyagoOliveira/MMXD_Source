using System;
using UnityEngine;

namespace Line.LineSDK
{
	public class LineSDK : MonoBehaviour
	{
		private static LineSDK instance;

		public string channelID;

		public string universalLinkURL;

		public static LineSDK Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new GameObject("LineSDK").AddComponent<LineSDK>();
				}
				return instance;
			}
		}

		public StoredAccessToken CurrentAccessToken
		{
			get
			{
				string currentAccessToken = NativeInterface.GetCurrentAccessToken();
				if (string.IsNullOrEmpty(currentAccessToken))
				{
					return null;
				}
				return JsonUtility.FromJson<StoredAccessToken>(currentAccessToken);
			}
		}

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			SetupSDK();
		}

		private void SetupSDK()
		{
			if (string.IsNullOrEmpty(channelID))
			{
				throw new Exception("LINE SDK channel ID is not set.");
			}
			NativeInterface.SetupSDK(channelID, universalLinkURL);
		}

		public void Login(string[] scopes, Action<Result<LoginResult>> action)
		{
			Login(scopes, null, action);
		}

		public void Login(string[] scopes, LoginOption option, Action<Result<LoginResult>> action)
		{
			LineAPI.Login(scopes, option, action);
		}

		public void Logout(Action<Result<Unit>> action)
		{
			LineAPI.Logout(action);
		}

		internal void OnApiOk(string result)
		{
			LineAPI._OnApiOk(result);
		}

		internal void OnApiError(string result)
		{
			LineAPI._OnApiError(result);
		}

		private void OnDestroy()
		{
			if (instance == this)
			{
				instance = null;
			}
		}
	}
}
