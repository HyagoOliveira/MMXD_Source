using System;

public static class DelegateExt
{
	public static void CheckTargetToInvoke(this Action p_cb)
	{
		if (p_cb != null && (p_cb.Target != null || p_cb.Method != null))
		{
			p_cb();
		}
	}

	public static void CheckTargetToInvoke<T>(this Action<T> p_cb, T p_param)
	{
		if (p_cb != null && (p_cb.Target != null || p_cb.Method != null))
		{
			p_cb(p_param);
		}
	}

	public static void CheckTargetToInvoke<T>(this Action<T[]> p_cb, params T[] p_params)
	{
		if (p_cb != null && (p_cb.Target != null || p_cb.Method != null))
		{
			p_cb(p_params);
		}
	}

	public static void CheckTargetToInvoke<T1, T2>(this Action<T1, T2> p_cb, T1 p_param1, T2 p_param2)
	{
		if (p_cb != null && (p_cb.Target != null || p_cb.Method != null))
		{
			p_cb(p_param1, p_param2);
		}
	}

	public static void CheckTargetToInvoke<T1, T2, T3>(this Action<T1, T2, T3> p_cb, T1 p_param1, T2 p_param2, T3 p_param3)
	{
		if (p_cb != null && (p_cb.Target != null || p_cb.Method != null))
		{
			p_cb(p_param1, p_param2, p_param3);
		}
	}

	public static void CheckTargetToInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> p_cb, T1 p_param1, T2 p_param2, T3 p_param3, T4 p_param4)
	{
		if (p_cb != null && (p_cb.Target != null || p_cb.Method != null))
		{
			p_cb(p_param1, p_param2, p_param3, p_param4);
		}
	}

	public static void CheckTargetToInvoke<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> p_cb, T1 p_param1, T2 p_param2, T3 p_param3, T4 p_param4, T5 p_param5)
	{
		if (p_cb != null && (p_cb.Target != null || p_cb.Method != null))
		{
			p_cb(p_param1, p_param2, p_param3, p_param4, p_param5);
		}
	}
}
