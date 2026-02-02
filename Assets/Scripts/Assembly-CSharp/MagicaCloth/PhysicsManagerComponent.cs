using System;
using System.Collections.Generic;

namespace MagicaCloth
{
	public class PhysicsManagerComponent : PhysicsManagerAccess
	{
		private HashSet<CoreComponent> componentSet = new HashSet<CoreComponent>();

		public int ComponentCount
		{
			get
			{
				return componentSet.Count;
			}
		}

		public override void Create()
		{
		}

		public override void Dispose()
		{
		}

		public void ComponentAction(Action<CoreComponent> act)
		{
			foreach (CoreComponent item in componentSet)
			{
				if (item != null)
				{
					act(item);
				}
			}
		}

		public void UpdateComponentStatus()
		{
			foreach (CoreComponent item in componentSet)
			{
				if (!(item == null) && item.Status.IsInitSuccess)
				{
					item.Status.UpdateStatus();
				}
			}
		}

		public void AddComponent(CoreComponent comp)
		{
			componentSet.Add(comp);
		}

		public void RemoveComponent(CoreComponent comp)
		{
			if (componentSet.Contains(comp))
			{
				componentSet.Remove(comp);
			}
		}
	}
}
