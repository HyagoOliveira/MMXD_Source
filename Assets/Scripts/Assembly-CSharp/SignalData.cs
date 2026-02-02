using System;

internal class SignalData<T>
{
	public Action<T> handler;

	public int priority;

	public bool isOnce;

	public SignalData(Action<T> handler, int priority, bool isOnce)
	{
		this.handler = handler;
		this.priority = priority;
		this.isOnce = isOnce;
	}
}
