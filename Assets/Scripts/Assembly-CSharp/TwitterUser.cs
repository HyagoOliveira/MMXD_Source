using TwitterKit.Unity;

public class TwitterUser
{
	private TwitterSession _session;

	public long Identify
	{
		get
		{
			return _session.id;
		}
	}

	public string Portrait
	{
		get
		{
			return string.Format("https://twitter.com/{0}/profile_image?size=original", _session.userName);
		}
	}

	public string Nickname
	{
		get
		{
			return _session.userName;
		}
	}

	public string AccessToken
	{
		get
		{
			return _session.authToken.token;
		}
	}

	public string AccessSecret
	{
		get
		{
			return _session.authToken.secret;
		}
	}

	public TwitterUser(TwitterSession session)
	{
		_session = session;
	}
}
