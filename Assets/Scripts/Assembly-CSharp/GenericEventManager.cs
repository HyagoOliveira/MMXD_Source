#define RELEASE
using System;
using System.Collections.Generic;
using Better;

public class GenericEventManager : Singleton<GenericEventManager>
{
	private System.Collections.Generic.Dictionary<EventManager.ID, Delegate> _dictEvents = new Better.Dictionary<EventManager.ID, Delegate>();

	public void AttachEvent(EventManager.ID p_eventId, Action p_cb)
	{
		Delegate value;
		_dictEvents.TryGetValue(p_eventId, out value);
		_dictEvents[p_eventId] = (Action)Delegate.Combine((Action)value, p_cb);
	}

	public void AttachEvent<T>(EventManager.ID p_eventId, Action<T> p_cb)
	{
		Delegate value;
		_dictEvents.TryGetValue(p_eventId, out value);
		_dictEvents[p_eventId] = (Action<T>)Delegate.Combine((Action<T>)value, p_cb);
	}

	public void AttachEvent<T1, T2>(EventManager.ID p_eventId, Action<T1, T2> p_cb)
	{
		Delegate value;
		_dictEvents.TryGetValue(p_eventId, out value);
		_dictEvents[p_eventId] = (Action<T1, T2>)Delegate.Combine((Action<T1, T2>)value, p_cb);
	}

	public void AttachEvent<T1, T2, T3>(EventManager.ID p_eventId, Action<T1, T2, T3> p_cb)
	{
		Delegate value;
		_dictEvents.TryGetValue(p_eventId, out value);
		_dictEvents[p_eventId] = (Action<T1, T2, T3>)Delegate.Combine((Action<T1, T2, T3>)value, p_cb);
	}

	public void AttachEvent<T1, T2, T3, T4>(EventManager.ID p_eventId, Action<T1, T2, T3, T4> p_cb)
	{
		Delegate value;
		_dictEvents.TryGetValue(p_eventId, out value);
		_dictEvents[p_eventId] = (Action<T1, T2, T3, T4>)Delegate.Combine((Action<T1, T2, T3, T4>)value, p_cb);
	}

	public void AttachEvent<T1, T2, T3, T4, T5>(EventManager.ID p_eventId, Action<T1, T2, T3, T4, T5> p_cb)
	{
		Delegate value;
		_dictEvents.TryGetValue(p_eventId, out value);
		_dictEvents[p_eventId] = (Action<T1, T2, T3, T4, T5>)Delegate.Combine((Action<T1, T2, T3, T4, T5>)value, p_cb);
	}

	public void DetachEvent(EventManager.ID p_eventId, Action p_cb)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				_dictEvents[p_eventId] = (Action)Delegate.Remove((Action)value, p_cb);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError(string.Format("DetachEvent : {0} Failed, Exception : {1}", p_eventId, arg));
		}
	}

	public void DetachEvent<T>(EventManager.ID p_eventId, Action<T> p_cb)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				_dictEvents[p_eventId] = (Action<T>)Delegate.Remove((Action<T>)value, p_cb);
			}
		}
		catch (Exception)
		{
		}
	}

	public void DetachEvent<T1, T2>(EventManager.ID p_eventId, Action<T1, T2> p_cb)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				_dictEvents[p_eventId] = (Action<T1, T2>)Delegate.Remove((Action<T1, T2>)value, p_cb);
			}
		}
		catch (Exception)
		{
		}
	}

	public void DetachEvent<T1, T2, T3>(EventManager.ID p_eventId, Action<T1, T2, T3> p_cb)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				_dictEvents[p_eventId] = (Action<T1, T2, T3>)Delegate.Remove((Action<T1, T2, T3>)value, p_cb);
			}
		}
		catch (Exception)
		{
		}
	}

	public void DetachEvent<T1, T2, T3, T4>(EventManager.ID p_eventId, Action<T1, T2, T3, T4> p_cb)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				_dictEvents[p_eventId] = (Action<T1, T2, T3, T4>)Delegate.Remove((Action<T1, T2, T3, T4>)value, p_cb);
			}
		}
		catch (Exception)
		{
		}
	}

	public void DetachEvent<T1, T2, T3, T4, T5>(EventManager.ID p_eventId, Action<T1, T2, T3, T4, T5> p_cb)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				_dictEvents[p_eventId] = (Action<T1, T2, T3, T4, T5>)Delegate.Remove((Action<T1, T2, T3, T4, T5>)value, p_cb);
			}
		}
		catch (Exception)
		{
		}
	}

	public void NotifyEvent(EventManager.ID p_eventId)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				Action obj = (Action)value;
				if (obj != null)
				{
					obj();
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void NotifyEvent<T>(EventManager.ID p_eventId, T p_param)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				Action<T> obj = (Action<T>)value;
				if (obj != null)
				{
					obj(p_param);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void NotifyEvent<T1, T2>(EventManager.ID p_eventId, T1 p_param1, T2 p_param2)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				Action<T1, T2> obj = (Action<T1, T2>)value;
				if (obj != null)
				{
					obj(p_param1, p_param2);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void NotifyEvent<T1, T2, T3>(EventManager.ID p_eventId, T1 p_param1, T2 p_param2, T3 p_param3)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				Action<T1, T2, T3> obj = (Action<T1, T2, T3>)value;
				if (obj != null)
				{
					obj(p_param1, p_param2, p_param3);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void NotifyEvent<T1, T2, T3, T4>(EventManager.ID p_eventId, T1 p_param1, T2 p_param2, T3 p_param3, T4 p_param4)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				Action<T1, T2, T3, T4> obj = (Action<T1, T2, T3, T4>)value;
				if (obj != null)
				{
					obj(p_param1, p_param2, p_param3, p_param4);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void NotifyEvent<T1, T2, T3, T4, T5>(EventManager.ID p_eventId, T1 p_param1, T2 p_param2, T3 p_param3, T4 p_param4, T5 p_param5)
	{
		try
		{
			Delegate value;
			if (_dictEvents.TryGetValue(p_eventId, out value))
			{
				Action<T1, T2, T3, T4, T5> obj = (Action<T1, T2, T3, T4, T5>)value;
				if (obj != null)
				{
					obj(p_param1, p_param2, p_param3, p_param4, p_param5);
				}
			}
		}
		catch (Exception)
		{
		}
	}
}
