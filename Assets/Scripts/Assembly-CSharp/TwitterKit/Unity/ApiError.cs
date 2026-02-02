using System;
using System.Collections.Generic;
using TwitterKit.ThirdParty.MiniJSON;

namespace TwitterKit.Unity
{
	public class ApiError
	{
		public int code { get; private set; }

		public string message { get; private set; }

		internal ApiError(int code, string message)
		{
			this.code = code;
			this.message = message;
		}

		internal static ApiError Deserialize(string error)
		{
			if (error == null || error.Length == 0)
			{
				return null;
			}
			Dictionary<string, object> obj = Json.Deserialize(error) as Dictionary<string, object>;
			int num = Convert.ToInt32(obj["code"]);
			string text = obj["message"] as string;
			return new ApiError(num, text);
		}
	}
}
