using System;
using System.Collections.Generic;

namespace Line.LineSDK
{
	public class LineAPI
	{
		internal static Dictionary<string, FlattenAction> actions = new Dictionary<string, FlattenAction>();

		public static void RefreshAccessToken(Action<Result<AccessToken>> action)
		{
			NativeInterface.RefreshAccessToken(AddAction(FlattenAction.JsonFlatten(action)));
		}

		public static void RevokeAccessToken(Action<Result<Unit>> action)
		{
			NativeInterface.RevokeAccessToken(AddAction(FlattenAction.UnitFlatten(action)));
		}

		public static void VerifyAccessToken(Action<Result<AccessTokenVerifyResult>> action)
		{
			NativeInterface.VerifyAccessToken(AddAction(FlattenAction.JsonFlatten(action)));
		}

		public static void GetProfile(Action<Result<UserProfile>> action)
		{
			NativeInterface.GetProfile(AddAction(FlattenAction.JsonFlatten(action)));
		}

		public static void GetBotFriendshipStatus(Action<Result<BotFriendshipStatus>> action)
		{
			NativeInterface.GetBotFriendshipStatus(AddAction(FlattenAction.JsonFlatten(action)));
		}

		internal static void Login(string[] scopes, LoginOption option, Action<Result<LoginResult>> action)
		{
			string identifier = AddAction(FlattenAction.JsonFlatten(action));
			if (scopes == null || scopes.Length == 0)
			{
				scopes = new string[1] { "profile" };
			}
			bool onlyWebLogin = false;
			string botPrompt = null;
			if (option != null)
			{
				onlyWebLogin = option.OnlyWebLogin;
				botPrompt = option.BotPrompt;
			}
			NativeInterface.Login(string.Join(" ", scopes), onlyWebLogin, botPrompt, identifier);
		}

		internal static void Logout(Action<Result<Unit>> action)
		{
			NativeInterface.Logout(AddAction(FlattenAction.UnitFlatten(action)));
		}

		private static string AddAction(FlattenAction action)
		{
			string text = Guid.NewGuid().ToString();
			actions.Add(text, action);
			return text;
		}

		private static FlattenAction PopActionFromPayload(CallbackPayload payload)
		{
			string identifier = payload.Identifier;
			if (identifier == null)
			{
				return null;
			}
			FlattenAction value = null;
			if (actions.TryGetValue(identifier, out value))
			{
				actions.Remove(identifier);
				return value;
			}
			return null;
		}

		internal static void _OnApiOk(string result)
		{
			CallbackPayload callbackPayload = CallbackPayload.FromJson(result);
			FlattenAction flattenAction = PopActionFromPayload(callbackPayload);
			if (flattenAction != null)
			{
				flattenAction.CallOk(callbackPayload.Value);
			}
		}

		internal static void _OnApiError(string result)
		{
			CallbackPayload callbackPayload = CallbackPayload.FromJson(result);
			FlattenAction flattenAction = PopActionFromPayload(callbackPayload);
			if (flattenAction != null)
			{
				flattenAction.CallError(callbackPayload.Value);
			}
		}
	}
}
