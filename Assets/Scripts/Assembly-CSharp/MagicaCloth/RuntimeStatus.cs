using System;
using System.Collections.Generic;

namespace MagicaCloth
{
	public class RuntimeStatus
	{
		private bool initStart;

		private bool init;

		private bool initError;

		private bool enable;

		private bool userEnable = true;

		private bool runtimeError;

		private bool dispose;

		private bool isActive;

		private HashSet<RuntimeStatus> parentStatusSet = new HashSet<RuntimeStatus>();

		private HashSet<RuntimeStatus> childStatusSet = new HashSet<RuntimeStatus>();

		public Action updateStatusAction;

		public Action disconnectedAction;

		public bool IsActive
		{
			get
			{
				if (isActive)
				{
					return !dispose;
				}
				return false;
			}
		}

		public bool IsInitStart
		{
			get
			{
				return initStart;
			}
		}

		public bool IsInitComplete
		{
			get
			{
				return init;
			}
		}

		public bool IsInitSuccess
		{
			get
			{
				if (init)
				{
					return !initError;
				}
				return false;
			}
		}

		public bool IsInitError
		{
			get
			{
				if (init)
				{
					return initError;
				}
				return false;
			}
		}

		public bool IsDispose
		{
			get
			{
				return dispose;
			}
		}

		public void SetInitStart()
		{
			initStart = true;
		}

		public void SetInitComplete()
		{
			init = true;
		}

		public void SetInitError()
		{
			initError = true;
		}

		public bool SetEnable(bool sw)
		{
			bool result = enable != sw;
			enable = sw;
			return result;
		}

		public bool SetUserEnable(bool sw)
		{
			bool result = userEnable != sw;
			userEnable = sw;
			return result;
		}

		public bool SetRuntimeError(bool sw)
		{
			bool result = runtimeError != sw;
			runtimeError = sw;
			return result;
		}

		public void SetDispose()
		{
			dispose = true;
		}

		public bool UpdateStatus()
		{
			if (dispose)
			{
				return false;
			}
			bool flag = init && !initError && enable && userEnable && !runtimeError && IsParentStatusActive();
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				flag = flag && CreateSingleton<MagicaPhysicsManager>.Instance.IsActive;
			}
			if (flag != isActive)
			{
				isActive = flag;
				Action action = updateStatusAction;
				if (action != null)
				{
					action();
				}
				foreach (RuntimeStatus item in childStatusSet)
				{
					if (item != null)
					{
						item.UpdateStatus();
					}
				}
				return true;
			}
			return false;
		}

		public void AddParentStatus(RuntimeStatus status)
		{
			parentStatusSet.Add(status);
		}

		public void RemoveParentStatus(RuntimeStatus status)
		{
			parentStatusSet.Remove(status);
			parentStatusSet.Remove(null);
			if (parentStatusSet.Count == 0 && childStatusSet.Count == 0)
			{
				Action action = disconnectedAction;
				if (action != null)
				{
					action();
				}
			}
		}

		public void AddChildStatus(RuntimeStatus status)
		{
			childStatusSet.Add(status);
		}

		public void RemoveChildStatus(RuntimeStatus status)
		{
			childStatusSet.Remove(status);
			childStatusSet.Remove(null);
			if (parentStatusSet.Count == 0 && childStatusSet.Count == 0)
			{
				Action action = disconnectedAction;
				if (action != null)
				{
					action();
				}
			}
		}

		public void LinkParentStatus(RuntimeStatus parent)
		{
			AddParentStatus(parent);
			parent.AddChildStatus(this);
		}

		public void UnlinkParentStatus(RuntimeStatus parent)
		{
			RemoveParentStatus(parent);
			parent.RemoveChildStatus(this);
		}

		private bool IsParentStatusActive()
		{
			if (parentStatusSet.Count == 0)
			{
				return true;
			}
			foreach (RuntimeStatus item in parentStatusSet)
			{
				if (item != null && item.IsActive)
				{
					return true;
				}
			}
			return false;
		}
	}
}
