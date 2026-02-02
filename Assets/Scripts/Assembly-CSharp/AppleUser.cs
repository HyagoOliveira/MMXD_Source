public class AppleUser
{
	private string _userId;

	private string _identityToken;

	public string UserId
	{
		get
		{
			return _userId;
		}
	}

	public string IdentityToken
	{
		get
		{
			return _identityToken;
		}
		set
		{
			_identityToken = value;
		}
	}

	public AppleUser(string userId, string identityToken)
	{
		_userId = userId;
		_identityToken = identityToken;
	}
}
