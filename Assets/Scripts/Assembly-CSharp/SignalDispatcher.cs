using System;
using System.Collections.Generic;
using Better;

public class SignalDispatcher : MonoBehaviourSingleton<SignalDispatcher>
{
	private System.Collections.Generic.Dictionary<object, Signal<object>> signalMap;

	public SignalDispatcher()
	{
		signalMap = new Better.Dictionary<object, Signal<object>>();
	}

	public void AddHandler(object type, Action<object> handler, int priority = 0, bool isOnce = false)
	{
		Signal<object> signal;
		if (signalMap.ContainsKey(type))
		{
			signal = signalMap[type];
		}
		else
		{
			signal = new Signal<object>();
			signalMap.Add(type, signal);
		}
		signal.AddHandler(handler, priority, isOnce);
	}

	public void RemoveHandler(object type, Action<object> handler)
	{
		if (signalMap.ContainsKey(type))
		{
			signalMap[type].RemoveHandler(handler);
		}
	}

	public void Dispatch(object type, object param)
	{
		if (signalMap.ContainsKey(type))
		{
			signalMap[type].Dispatch(param);
		}
	}

	public bool ContainListener(object type)
	{
		if (!signalMap.ContainsKey(type))
		{
			return false;
		}
		return signalMap[type].Count > 0;
	}

	public void Clean()
	{
		List<object> list = new List<object>();
		foreach (KeyValuePair<object, Signal<object>> item in signalMap)
		{
			if (item.Value.Count == 0)
			{
				list.Add(item.Key);
			}
		}
		foreach (object item2 in list)
		{
			signalMap.Remove(item2);
		}
	}

	public void Dispose()
	{
		foreach (KeyValuePair<object, Signal<object>> item in signalMap)
		{
			item.Value.Dispose();
		}
		signalMap = null;
	}
}
