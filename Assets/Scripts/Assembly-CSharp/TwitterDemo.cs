#define RELEASE
using TwitterKit.Unity;
using UnityEngine;

public class TwitterDemo : MonoBehaviour
{
	private void Start()
	{
	}

	public void startLogin()
	{
		UnityEngine.Debug.Log("startLogin()");
		Twitter.Init();
		Twitter.LogIn(LoginCompleteWithCompose, delegate(ApiError error)
		{
			UnityEngine.Debug.Log(error.message);
		});
	}

	public void LoginCompleteWithEmail(TwitterSession session)
	{
		UnityEngine.Debug.Log("LoginCompleteWithEmail()");
		Twitter.RequestEmail(session, RequestEmailComplete, delegate(ApiError error)
		{
			UnityEngine.Debug.Log(error.message);
		});
	}

	public void RequestEmailComplete(string email)
	{
		UnityEngine.Debug.Log("email=" + email);
		LoginCompleteWithCompose(Twitter.Session);
	}

	public void LoginCompleteWithCompose(TwitterSession session)
	{
		UnityEngine.Debug.Log("Screenshot location=" + Application.persistentDataPath + "/Screenshot.png");
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
