using System;

namespace UnityEngine.UI
{
	[Serializable]
	public class LoopScrollPrefabSource
	{
		public ScrollIndexCallback pbo;

		public int count;

		public bool Inited { get; set; }

		public virtual GameObject GetObject()
		{
			Init();
			return MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ScrollIndexCallback>(pbo.itemName).gameObject;
		}

		public void Init()
		{
			if (!Inited)
			{
				pbo.itemName = pbo.GetInstanceID().ToString();
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<ScrollIndexCallback>(Object.Instantiate(pbo), pbo.itemName, pbo.name, count);
				Inited = true;
			}
		}

		public void ReturnObject(Transform p_transform)
		{
			p_transform.GetComponent<ScrollIndexCallback>().BackToPool();
		}
	}
}
