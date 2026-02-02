using System;
using System.Collections.Generic;
using TwitterKit.ThirdParty.MiniJSON;

namespace TwitterKit.Unity
{
	public class TwitterSession
	{
		public long id { get; private set; }

		public string userName { get; private set; }

		public AuthToken authToken { get; private set; }

		internal TwitterSession(long id, string userName, AuthToken authToken)
		{
			this.id = id;
			this.userName = userName;
			this.authToken = authToken;
		}

		internal Dictionary<string, object> ToDictionary()
		{
			return new Dictionary<string, object>
			{
				{ "id", id },
				{ "user_name", userName },
				{
					"auth_token",
					authToken.ToDictionary()
				}
			};
		}

		internal static string Serialize(TwitterSession session)
		{
			return Json.Serialize(session.ToDictionary());
		}

		internal static TwitterSession Deserialize(string session)
		{
			if (session == null || session.Length == 0)
			{
				return null;
			}
			Dictionary<string, object> obj = Json.Deserialize(session) as Dictionary<string, object>;
			long num = Convert.ToInt64(obj["id"]);
			string text = obj["user_name"] as string;
			Dictionary<string, object> obj2 = obj["auth_token"] as Dictionary<string, object>;
			string token = obj2["token"] as string;
			string secret = obj2["secret"] as string;
			return new TwitterSession(num, text, new AuthToken(token, secret));
		}
	}
}
