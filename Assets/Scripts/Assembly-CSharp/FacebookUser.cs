public class FacebookUser
{
	private string _identify;

	private string _portrait;

	private string _nickname;

	private string _accessToken;

	private int _score;

	public string Identify
	{
		get
		{
			return _identify;
		}
	}

	public string Portrait
	{
		get
		{
			return _portrait;
		}
		set
		{
			_portrait = value;
		}
	}

	public string Nickname
	{
		get
		{
			return _nickname;
		}
		set
		{
			_nickname = value;
		}
	}

	public string AccessToken
	{
		get
		{
			return _accessToken;
		}
		set
		{
			_accessToken = value;
		}
	}

	public int Score
	{
		get
		{
			return _score;
		}
		set
		{
			_score = value;
		}
	}

	public FacebookUser(string identify)
	{
		_identify = identify;
	}
}
