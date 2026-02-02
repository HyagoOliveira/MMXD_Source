#define RELEASE
using UnityEngine;

namespace Line.LineSDK
{
	public static class Helpers
	{
		internal static bool IsInvalidRuntime(string identifier, RuntimePlatform platform)
		{
			if (Application.platform != platform)
			{
				Debug.LogWarning("[LINE SDK] This RuntimePlatform is not supported. Only iOS and Android devices are supported.");
				string value = "{\"code\":-1, \"message\":\"Platform not supported.\"}";
				string result = CallbackPayload.WrapValue(identifier, value);
				LineSDK.Instance.OnApiError(result);
				return true;
			}
			return false;
		}
	}
}
