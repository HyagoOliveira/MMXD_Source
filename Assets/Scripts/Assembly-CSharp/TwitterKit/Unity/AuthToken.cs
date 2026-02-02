using System.Collections.Generic;

namespace TwitterKit.Unity
{
	public class AuthToken
	{
		public string token { get; private set; }

		public string secret { get; private set; }

		internal AuthToken(string token, string secret)
		{
			this.token = token;
			this.secret = secret;
		}

		internal Dictionary<string, object> ToDictionary()
		{
			return new Dictionary<string, object>
			{
				{ "token", token },
				{ "secret", secret }
			};
		}
	}
}
