using System;
using UnityEngine;

namespace Line.LineSDK
{
	[Serializable]
	internal class CallbackPayload
	{
		[SerializeField]
		private string identifier;

		[SerializeField]
		private string value;

		internal string Identifier
		{
			get
			{
				return identifier;
			}
		}

		internal string Value
		{
			get
			{
				return value;
			}
		}

		internal static CallbackPayload FromJson(string json)
		{
			return JsonUtility.FromJson<CallbackPayload>(json);
		}

		private CallbackPayload(string identifier, string value)
		{
			this.identifier = identifier;
			this.value = value;
		}

		private string ToJson()
		{
			return JsonUtility.ToJson(this);
		}

		internal static string WrapValue(string identifier, string value)
		{
			return new CallbackPayload(identifier, value).ToJson();
		}
	}
}
