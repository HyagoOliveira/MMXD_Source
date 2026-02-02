using System;
using UnityEngine;

namespace Line.LineSDK
{
	internal class FlattenAction
	{
		private Action<string> successAction;

		private Action<string> failureAction;

		private FlattenAction(Action<string> successAction, Action<string> failureAction)
		{
			this.successAction = successAction;
			this.failureAction = failureAction;
		}

		internal static FlattenAction JsonFlatten<T>(Action<Result<T>> action)
		{
			return new FlattenAction(delegate(string value)
			{
				Result<T> obj2 = Result<T>.Ok(JsonUtility.FromJson<T>(value));
				action(obj2);
			}, delegate(string error)
			{
				Result<T> obj = Result<T>.Error(JsonUtility.FromJson<Error>(error));
				action(obj);
			});
		}

		internal static FlattenAction UnitFlatten(Action<Result<Unit>> action)
		{
			return new FlattenAction(delegate
			{
				Result<Unit> obj2 = Result<Unit>.Ok(Unit.Value);
				action(obj2);
			}, delegate(string error)
			{
				Result<Unit> obj = Result<Unit>.Error(JsonUtility.FromJson<Error>(error));
				action(obj);
			});
		}

		internal void CallOk(string s)
		{
			successAction(s);
		}

		internal void CallError(string s)
		{
			failureAction(s);
		}
	}
}
