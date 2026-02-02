using System;
using System.Collections.Generic;
using CallbackDefs;

public class UpdateManager : MonoBehaviourSingleton<UpdateManager>
{
	private bool pause;

	private List<IManagedUpdateBehavior> ListUpdate = new List<IManagedUpdateBehavior>();

	private List<IManagedFixedUpdateBehavior> ListFixedUpdate = new List<IManagedFixedUpdateBehavior>();

	private List<IManagedLateUpdateBehavior> ListLateUpdate = new List<IManagedLateUpdateBehavior>();

	private Queue<Callback> MainThreadTask = new Queue<Callback>();

	public bool Pause
	{
		get
		{
			return pause;
		}
		set
		{
			if (pause != value)
			{
				pause = value;
				Action<bool> action = this.onPauseEvent;
				if (action != null)
				{
					action(pause);
				}
			}
		}
	}

	public event Action<bool> onPauseEvent;

	public void AddUpdate<T>(T p_update) where T : IManagedUpdateBehavior
	{
		if (!ListUpdate.Contains(p_update))
		{
			ListUpdate.Add(p_update);
		}
	}

	public void AddFixedUpdate<T>(T p_fixedUpdate) where T : IManagedFixedUpdateBehavior
	{
		if (!ListFixedUpdate.Contains(p_fixedUpdate))
		{
			ListFixedUpdate.Add(p_fixedUpdate);
		}
	}

	public void AddLateUpdate<T>(T p_lateUpdate) where T : IManagedLateUpdateBehavior
	{
		if (!ListLateUpdate.Contains(p_lateUpdate))
		{
			ListLateUpdate.Add(p_lateUpdate);
		}
	}

	public bool CheckUpdateContain<T>(T p_update) where T : IManagedUpdateBehavior
	{
		if (ListUpdate.Contains(p_update))
		{
			return true;
		}
		return false;
	}

	public void RemoveUpdate<T>(T p_update) where T : IManagedUpdateBehavior
	{
		if (ListUpdate.Contains(p_update))
		{
			ListUpdate[ListUpdate.IndexOf(p_update)] = null;
		}
	}

	public void RemoveFixedUpdate<T>(T p_fixedUpdate) where T : IManagedFixedUpdateBehavior
	{
		if (ListFixedUpdate.Contains(p_fixedUpdate))
		{
			ListFixedUpdate[ListFixedUpdate.IndexOf(p_fixedUpdate)] = null;
		}
	}

	public void RemoveLateUpdate<T>(T p_lateUpdate) where T : IManagedLateUpdateBehavior
	{
		if (ListLateUpdate.Contains(p_lateUpdate))
		{
			ListLateUpdate[ListLateUpdate.IndexOf(p_lateUpdate)] = null;
		}
	}

	private void Update()
	{
		UpdateMainThreadTask();
		if (Pause)
		{
			return;
		}
		for (int i = 0; i < ListUpdate.Count; i++)
		{
			if (ListUpdate[i] == null)
			{
				ListUpdate.RemoveAt(i);
				i--;
			}
			else
			{
				ListUpdate[i].UpdateFunc();
			}
		}
	}

	private void FixedUpdate()
	{
		if (Pause)
		{
			return;
		}
		for (int i = 0; i < ListFixedUpdate.Count; i++)
		{
			if (ListFixedUpdate[i] == null)
			{
				ListFixedUpdate.RemoveAt(i);
				i--;
			}
			else
			{
				ListFixedUpdate[i].FixedUpdateFunc();
			}
		}
	}

	private void LateUpdate()
	{
		if (Pause)
		{
			return;
		}
		for (int i = 0; i < ListLateUpdate.Count; i++)
		{
			if (ListLateUpdate[i] == null)
			{
				ListLateUpdate.RemoveAt(i);
				i--;
			}
			else
			{
				ListLateUpdate[i].LateUpdateFunc();
			}
		}
	}

	public void AddMainThreadTask(Callback p_cb)
	{
		lock (MainThreadTask)
		{
			MainThreadTask.Enqueue(p_cb);
		}
	}

	private void UpdateMainThreadTask()
	{
		lock (MainThreadTask)
		{
			if (MainThreadTask.Count >= 1)
			{
				MainThreadTask.Dequeue().CheckTargetToInvoke();
			}
		}
	}
}
