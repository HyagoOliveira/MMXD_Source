using System;

namespace Line.LineSDK
{
	public class Result<T>
	{
		private T value;

		private Error error;

		private bool success;

		public bool IsSuccess
		{
			get
			{
				return success;
			}
		}

		public bool IsFailure
		{
			get
			{
				return !success;
			}
		}

		public static Result<T> Ok(T value)
		{
			return new Result<T>
			{
				value = value,
				success = true
			};
		}

		public static Result<T> Error(Error error)
		{
			return new Result<T>
			{
				error = error,
				success = false
			};
		}

		public void MatchOk(Action<T> onMatched)
		{
			if (onMatched == null)
			{
				throw new Exception("Match callback is null!");
			}
			if (IsSuccess)
			{
				onMatched(value);
			}
		}

		public void MatchError(Action<Error> onMatched)
		{
			if (onMatched == null)
			{
				throw new Exception("Match callback is null!");
			}
			if (IsFailure)
			{
				onMatched(error);
			}
		}

		public void Match(Action<T> onMatchedOk, Action<Error> onMatchedError)
		{
			MatchOk(onMatchedOk);
			MatchError(onMatchedError);
		}
	}
}
