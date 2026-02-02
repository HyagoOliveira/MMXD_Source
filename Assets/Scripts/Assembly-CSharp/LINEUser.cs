using Line.LineSDK;

public class LINEUser
{
	private LoginResult _loginResult;

	public string AccessToken
	{
		get
		{
			return _loginResult.AccessToken.Value;
		}
	}

	public string TokenType
	{
		get
		{
			return _loginResult.AccessToken.TokenType;
		}
	}

	public string RefreshToken
	{
		get
		{
			return _loginResult.AccessToken.RefreshToken;
		}
	}

	public long ExpiresIn
	{
		get
		{
			return _loginResult.AccessToken.ExpiresIn;
		}
	}

	public string Scope
	{
		get
		{
			return _loginResult.AccessToken.Scope;
		}
	}

	public string IDToken
	{
		get
		{
			return _loginResult.AccessToken.IdTokenRaw;
		}
	}

	public string UserID
	{
		get
		{
			return _loginResult.UserProfile.UserId;
		}
	}

	public string DisplayName
	{
		get
		{
			return _loginResult.UserProfile.DisplayName;
		}
	}

	public string PictureUrl
	{
		get
		{
			return _loginResult.UserProfile.PictureUrl;
		}
	}

	public string StatusMessage
	{
		get
		{
			return _loginResult.UserProfile.StatusMessage;
		}
	}

	public LINEUser(LoginResult result)
	{
		_loginResult = result;
	}
}
