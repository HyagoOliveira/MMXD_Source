#define RELEASE
using System;
using CallbackDefs;
using Line.LineSDK;

public class LINEManager : ManagedSingleton<LINEManager>
{
	private class LoginResponseBody
	{
		public string access_token;

		public string token_type;

		public string refresh_token;

		public int expires_in;

		public string scope;

		public string id_token;
	}

	private readonly string _channelID = "1653560165";

	private LINEUser _lineOwner;

	public Action<LINEUser> OnLoginRetrieveInfoSuccess;

	public Callback OnLoginRetrieveInfoCancel;

	public string ChannelID
	{
		get
		{
			return _channelID;
		}
	}

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public void Login()
	{
		string[] scopes = new string[2] { "profile", "openid" };
		LineSDK.Instance.Login(scopes, delegate(Result<LoginResult> result)
		{
			result.Match(delegate(LoginResult value)
			{
				Debug.Log("Login OK. User display name: " + value.UserProfile.DisplayName);
				_lineOwner = new LINEUser(value);
			}, delegate(Error error)
			{
				Debug.Log("Login failed, reason: " + error.Message);
			});
		});
	}

	public void LoginWithInitialize()
	{
		string[] scopes = new string[2] { "profile", "openid" };
		LineSDK.Instance.Login(scopes, delegate(Result<LoginResult> result)
		{
			result.Match(delegate(LoginResult value)
			{
				Debug.Log("Login OK. User display name: " + value.UserProfile.DisplayName);
				_lineOwner = new LINEUser(value);
				OnLoginRetrieveInfoSuccess(_lineOwner);
			}, delegate(Error error)
			{
				Debug.Log("Login failed, reason: " + error.Message);
				OnLoginRetrieveInfoCancel.CheckTargetToInvoke();
			});
		});
	}

	public void Logout()
	{
		LineSDK.Instance.Logout(delegate(Result<Unit> result)
		{
			result.Match(delegate
			{
				Debug.Log("Log out OK.");
			}, delegate(Error error)
			{
				Debug.Log("Logout failed, reson: " + error.Message);
			});
		});
	}
}
