using UnityEngine;

namespace TwitterKit.Internal
{
	public static class Utils
	{
		private const string TWITTER_KIT = "TwitterKit";

		public static void Log(string message)
		{
			Log(message, "d");
		}

		public static void LogError(string message)
		{
			Log(message, "e");
		}

		private static void Log(string message, string logType)
		{
			if (logType == "e")
			{
				UnityEngine.Debug.LogError("TwitterKit: " + message);
			}
			else
			{
				UnityEngine.Debug.Log("TwitterKit: " + message);
			}
		}
	}
}
