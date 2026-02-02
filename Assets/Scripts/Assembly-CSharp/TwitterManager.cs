#define RELEASE
using System;
using CallbackDefs;
using TwitterKit.Unity;
using UnityEngine;

public class TwitterManager : ManagedSingleton<TwitterManager>
{
	public const string prefTwitterLastTimeLogin = "TwitterLastTimeLogin";

	private TwitterUser _twitterOwner;

	public Action<TwitterUser> OnLoginRetrieveInfoSuccess;

	public Callback OnLoginRetrieveInfoCancel;

	public override void Initialize()
	{
		Twitter.AwakeInit();
	}

	public override void Dispose()
	{
	}

	public void LoginWithInitialize()
	{
		Twitter.Init();
		Twitter.LogIn(LoginSuccessHandler, delegate(ApiError error)
		{
			Debug.Log(error.message);
			OnLoginRetrieveInfoCancel.CheckTargetToInvoke();
		});
	}

	public void Logout()
	{
		Twitter.Init();
		Twitter.LogOut();
		PlayerPrefs.SetInt("TwitterLastTimeLogin", 0);
	}

	public void LoginSuccessHandler(TwitterSession session)
	{
		_twitterOwner = new TwitterUser(session);
		if (OnLoginRetrieveInfoSuccess != null)
		{
			OnLoginRetrieveInfoSuccess(_twitterOwner);
		}
	}

	public void LoginCompleteWithEmail(TwitterSession session)
	{
		Debug.Log("LoginCompleteWithEmail()");
		Twitter.RequestEmail(session, RequestEmailComplete, delegate(ApiError error)
		{
			UnityEngine.Debug.Log(error.message);
		});
	}

	public void RequestEmailComplete(string email)
	{
		Debug.Log("email=" + email);
		LoginCompleteWithCompose(Twitter.Session);
	}

	public void LoginCompleteWithCompose(TwitterSession session)
	{
		Debug.Log("Screenshot location=" + Application.persistentDataPath + "/Screenshot.png");
		string imageUri = "file://" + Application.persistentDataPath + "/Screenshot.png";
		Twitter.Compose(session, imageUri, "Welcome to", new string[1] { "#TwitterKitUnity" }, delegate(string tweetId)
		{
			UnityEngine.Debug.Log("Tweet Success, tweetId=" + tweetId);
		}, delegate(ApiError error)
		{
			UnityEngine.Debug.Log("Tweet Failed " + error.message);
		}, delegate
		{
			Debug.Log("Compose cancelled");
		});
	}
}
