#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using Facebook.MiniJSON;
using Facebook.Unity;
using Newtonsoft.Json;
using UnityEngine;

public class FacebookManager : ManagedSingleton<FacebookManager>
{
	private string AppID = "581210249080101";

	public const string prefFBLastTimeLogin = "FBLastTimeLogin";

	private FacebookUser _fbOwner;

	private Dictionary<string, FacebookUser> dicFriends = new Dictionary<string, FacebookUser>();

	public Action<FacebookUser> OnLoginRetrieveInfoSuccess;

	public Callback OnLoginRetrieveInfoCancel;

	public string message;

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	private void OnInitComplete()
	{
		Debug.Log(string.Format("OnInitCompleteCalled IsLoggedIn='{0}' IsInitialized='{1}'", FB.IsLoggedIn, FB.IsInitialized));
	}

	private void OnHideUnity(bool isGameShown)
	{
		Debug.Log("Success - Check log for details");
	}

	public void LoginWithInitialize()
	{
		if (!FB.IsInitialized)
		{
			FB.Init(AppID, null, true, true, true, false, true, null, "en_US", OnHideUnity, Login);
		}
		else
		{
			Login();
		}
	}

	private void Login()
	{
		FB.ActivateApp();
		Debug.Log("FB LoginWithReadPermissions.");
		FB.LogInWithReadPermissions(new List<string> { "public_profile", "email" }, authCallback);
	}

	private void authCallback(IResult result)
	{
		Debug.Log("FB authCallback.");
		if (result.Error != null)
		{
			Debug.Log("result.Error != null, " + result.Error);
		}
		if (FB.IsLoggedIn)
		{
			Debug.Log("UserID=" + AccessToken.CurrentAccessToken.UserId);
			Debug.Log("TokenString=" + AccessToken.CurrentAccessToken.TokenString);
			_fbOwner = new FacebookUser(AccessToken.CurrentAccessToken.UserId);
			_fbOwner.AccessToken = AccessToken.CurrentAccessToken.TokenString;
			FB.API("/me?fields=picture,id,first_name", HttpMethod.GET, infoCallback);
		}
		else
		{
			Debug.Log("Login cancelled.");
			if (OnLoginRetrieveInfoCancel != null)
			{
				OnLoginRetrieveInfoCancel();
			}
		}
		message = result.RawResult;
	}

	private void infoCallback(IResult result)
	{
		Debug.Log("FB infoCallback");
		if (_fbOwner == null)
		{
			Debug.Log("_fbOwner == null");
		}
		Dictionary<string, object> dictionary = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
		if (!dictionary.ContainsKey("picture") || !dictionary.ContainsKey("first_name") || !dictionary.ContainsKey("id"))
		{
			return;
		}
		Dictionary<string, object> dictionary2 = dictionary["picture"] as Dictionary<string, object>;
		if (!dictionary2.ContainsKey("data"))
		{
			return;
		}
		Dictionary<string, object> dictionary3 = dictionary2["data"] as Dictionary<string, object>;
		if (dictionary3.ContainsKey("url"))
		{
			_fbOwner.Portrait = dictionary3["url"].ToString();
			_fbOwner.Nickname = dictionary["first_name"].ToString();
			PlayerPrefs.SetInt("FBLastTimeLogin", 1);
			if (OnLoginRetrieveInfoSuccess != null)
			{
				OnLoginRetrieveInfoSuccess(_fbOwner);
			}
		}
	}

	public void Logout()
	{
		if (!FB.IsInitialized)
		{
			FB.Init(Logout, OnHideUnity);
			return;
		}
		FB.LogOut();
		PlayerPrefs.SetInt("FBLastTimeLogin", 0);
	}

	public void ShareGameToFacebook()
	{
		FB.FeedShare(string.Empty, new Uri("http://www.capcom.co.jp/rockman11/"), "Rockman11!!", "運命の歯車！！", "「ロックマンX」&「ロックマン11」", new Uri("https://enterimagehere.com"), string.Empty, ShareCallback);
	}

	private void ShareCallback(IResult result)
	{
		message = result.RawResult;
		Dictionary<string, object> dictionary = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
		if (dictionary.ContainsKey("cancelled"))
		{
			object obj = dictionary["cancelled"];
		}
	}

	public void InviteFriend()
	{
		FB.AppRequest("This game is awesome, come and join!", null, null, null, null, null, "Rockman11", Invitecallback);
	}

	private void Invitecallback(IResult result)
	{
		message = result.RawResult;
		Dictionary<string, object> dictionary = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
		if (dictionary.ContainsKey("cancelled"))
		{
			object obj = dictionary["cancelled"];
		}
	}

	public void SetNewScore(int score)
	{
		int @int = PlayerPrefs.GetInt("Score", 0);
		if (score > @int)
		{
			PlayerPrefs.SetInt("Score", score);
			if (_fbOwner != null)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["score"] = score.ToString();
				FB.API("me/scores", HttpMethod.POST, SetScoreCallback, dictionary);
			}
		}
	}

	private void SetScoreCallback(IResult result)
	{
		message = result.RawResult;
	}

	public void QueryFriendsScore()
	{
		FB.API("/app/scores?fields=score,user.limit(100)", HttpMethod.GET, QueryScoreCallback);
	}

	private void QueryScoreCallback(IResult result)
	{
		message = result.RawResult;
		dicFriends.Clear();
		foreach (FacebookQueryUser datum in (JsonConvert.DeserializeObject(result.RawResult, typeof(FacebookQueryData)) as FacebookQueryData).data)
		{
			FacebookUser facebookUser = new FacebookUser(datum.user.id);
			facebookUser.Score = datum.score;
			facebookUser.Nickname = datum.user.name;
			dicFriends.Add(facebookUser.Identify, facebookUser);
		}
		string text = "";
		foreach (KeyValuePair<string, FacebookUser> dicFriend in dicFriends)
		{
			text += string.Format("ID={0} , Name={1} Score={2}\n", dicFriend.Value.Identify, dicFriend.Value.Nickname, dicFriend.Value.Score);
		}
		message = text + "Total=" + dicFriends.Count;
	}

	public List<FacebookUser> GetFriendsByArea(int areaID)
	{
		List<FacebookUser> list = new List<FacebookUser>();
		foreach (KeyValuePair<string, FacebookUser> dicFriend in dicFriends)
		{
			if (dicFriend.Value.Score / 10 == areaID)
			{
				list.Add(dicFriend.Value);
			}
		}
		return list;
	}

	public List<FacebookUser> GetFriendsByStage(int stageID)
	{
		List<FacebookUser> list = new List<FacebookUser>();
		foreach (KeyValuePair<string, FacebookUser> dicFriend in dicFriends)
		{
			if (dicFriend.Value.Score == stageID)
			{
				list.Add(dicFriend.Value);
			}
		}
		return list;
	}

	public List<FacebookUser> GetFriends()
	{
		List<FacebookUser> list = new List<FacebookUser>();
		foreach (KeyValuePair<string, FacebookUser> dicFriend in dicFriends)
		{
			list.Add(dicFriend.Value);
		}
		return list;
	}
}
