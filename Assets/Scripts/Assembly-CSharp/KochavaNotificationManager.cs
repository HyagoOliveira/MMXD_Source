#define RELEASE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
using UnityEngine;

public class KochavaNotificationManager : MonoBehaviourSingleton<KochavaNotificationManager>
{
	private FirebaseApp firebaseApp;

	public bool IsInit { get; set; }

	public string Token { get; private set; }

	private void Awake()
	{
		IsInit = false;
	}

	private void Start()
	{
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(delegate(Task<DependencyStatus> task)
		{
			DependencyStatus result = task.Result;
			if (result == DependencyStatus.Available)
			{
				InitializeFirebase();
			}
			else
			{
				UnityEngine.Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", result));
			}
		});
	}

	private void InitializeFirebase()
	{
		firebaseApp = FirebaseApp.DefaultInstance;
		FirebaseMessaging.TokenReceived += OnTokenReceived;
		FirebaseMessaging.MessageReceived += OnMessageReceived;
		if (Kochava.IsInitialized())
		{
			Debug.Log("[Kochava][GetDeviceId] : " + Kochava.Tracker.GetDeviceId());
		}
		Debug.Log("[FirebaseApp][Project ID] : " + firebaseApp.Options.ProjectId);
		IsInit = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
	{
		Token = token.Token;
		AddPushToken();
	}

	public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
	{
		IDictionary<string, string> data = e.Message.Data;
		if (e.Message.Data != null)
		{
			string value = string.Empty;
			string value2 = string.Empty;
			data.TryGetValue("title", out value);
			data.TryGetValue("body", out value2);
			if (value2 != string.Empty)
			{
				MonoBehaviourSingleton<NotificationManager>.Instance.NotificationMessage(value, value2, DateTime.Now);
			}
		}
	}

	public void AddPushToken()
	{
		Debug.Log("[FirebaseApp][OnTokenReceived]:");
		Debug.Log(Token);
		IsTokenExist();
	}

	public void RemovePushToken()
	{
		IsTokenExist();
	}

	private bool IsTokenExist()
	{
		return false;
	}
}
