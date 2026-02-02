using System;
using System.Collections.Generic;

public class Signal<T> : IDisposable
{
	private List<SignalData<T>> dataList;

	private bool isExecuting;

	private List<SignalData<T>> removeList;

	public int Count
	{
		get
		{
			return dataList.Count;
		}
	}

	public Signal()
	{
		dataList = new List<SignalData<T>>();
	}

	public void AddHandler(Action<T> handler, int priority = 0, bool isOnce = false)
	{
		if (Contains(handler))
		{
			return;
		}
		SignalData<T> signalData = new SignalData<T>(handler, priority, isOnce);
		int count = dataList.Count;
		if (count != 0)
		{
			for (int i = 0; i < count; i++)
			{
				SignalData<T> signalData2 = dataList[i];
				if (signalData.priority >= signalData2.priority)
				{
					dataList.Insert(i, signalData);
					break;
				}
			}
		}
		else
		{
			dataList.Add(signalData);
		}
	}

	public void RemoveHandler(Action<T> handler)
	{
		if (dataList == null)
		{
			return;
		}
		int count = dataList.Count;
		for (int i = 0; i < count; i++)
		{
			SignalData<T> signalData = dataList[i];
			if (!signalData.handler.Equals(handler))
			{
				continue;
			}
			if (isExecuting)
			{
				if (removeList == null)
				{
					removeList = new List<SignalData<T>>();
				}
				removeList.Add(signalData);
			}
			else
			{
				dataList.RemoveAt(i);
			}
			break;
		}
	}

	public void Dispatch(T param)
	{
		if (dataList.Count == 0)
		{
			return;
		}
		isExecuting = true;
		List<SignalData<T>> list = dataList;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			SignalData<T> signalData = list[i];
			signalData.handler(param);
			if (signalData.isOnce)
			{
				if (removeList == null)
				{
					removeList = new List<SignalData<T>>();
				}
				removeList.Add(signalData);
			}
		}
		if (removeList != null)
		{
			int count2 = removeList.Count;
			for (int j = 0; j < count2; j++)
			{
				dataList.Remove(removeList[j]);
			}
			removeList = null;
		}
		isExecuting = false;
	}

	public bool Contains(Action<T> handler)
	{
		foreach (SignalData<T> data in dataList)
		{
			if (data.handler.Equals(handler))
			{
				return true;
			}
		}
		return false;
	}

	public void Dispose()
	{
		dataList = null;
	}
}
