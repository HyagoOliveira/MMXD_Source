using System;
using System.Diagnostics;
using UnityEngine;

public static class Debug
{
	[Conditional("RELEASE")]
	public static void Break()
	{
	}

	[Conditional("RELEASE")]
	public static void ClearDeveloperConsole()
	{
	}

	[Conditional("RELEASE")]
	public static void DebugBreak()
	{
	}

	[Conditional("RELEASE")]
	public static void DrawLine(Vector3 start, Vector3 end)
	{
	}

	[Conditional("RELEASE")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
	}

	[Conditional("RELEASE")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
	{
	}

	[Conditional("RELEASE")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest)
	{
	}

	[Conditional("RELEASE")]
	public static void DrawRay(Vector3 start, Vector3 dir)
	{
	}

	[Conditional("RELEASE")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color)
	{
	}

	[Conditional("RELEASE")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
	{
	}

	[Conditional("RELEASE")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration, bool depthTest)
	{
	}

	[Conditional("RELEASE")]
	public static void Log(object message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("RELEASE")]
	public static void Log(object message, UnityEngine.Object context)
	{
	}

	[Conditional("RELEASE")]
	public static void LogError(object message)
	{
		UnityEngine.Debug.LogError(message);
	}

	[Conditional("RELEASE")]
	public static void LogError(object message, UnityEngine.Object context)
	{
		UnityEngine.Debug.LogError(message, context);
	}

	[Conditional("RELEASE")]
	public static void LogException(Exception exception)
	{
	}

	[Conditional("RELEASE")]
	public static void LogException(Exception exception, UnityEngine.Object context)
	{
	}

	[Conditional("RELEASE")]
	public static void LogWarning(object message)
	{
    }

    [Conditional("RELEASE")]
	public static void LogWarning(object message, UnityEngine.Object context)
	{
	}

	[Conditional("RELEASE")]
	public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
	{
	}

	[Conditional("RELEASE")]
	public static void LogFormat(string format, params object[] args)
	{
	}

	[Conditional("RELEASE")]
	public static void LogWarningFormat(string format, params object[] args)
	{
	}

	[Conditional("RELEASE")]
	public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
	{
	}

	[Conditional("RELEASE")]
	public static void Assert(bool condition)
	{
	}

	[Conditional("RELEASE")]
	public static void LogAssertion(object message)
	{
	}
}
