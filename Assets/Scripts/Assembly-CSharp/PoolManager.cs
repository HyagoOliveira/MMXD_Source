#define RELEASE
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Better;
using CallbackDefs;
using UnityEngine;

public class PoolManager : MonoBehaviourSingleton<PoolManager>
{
	private class PoolInfo
	{
		public Vector2 itemPosition;

		public Queue<PoolBaseObject> queue;

		public PoolInfo(Vector2 itemPosition, Queue<PoolBaseObject> queue)
		{
			this.itemPosition = itemPosition;
			this.queue = queue;
		}
	}

	private System.Collections.Generic.Dictionary<string, PoolInfo> pool = new Better.Dictionary<string, PoolInfo>();

	private Transform _transform;

	private readonly Vector3 outSidePos = new Vector3(10000f, 10000f, 0f);

	private readonly Vector3 zero = new Vector3(0f, 0f, 0f);

	private readonly Vector3 one = new Vector3(1f, 1f, 1f);

	private void Awake()
	{
		InitPool();
	}

	private void InitPool()
	{
		_transform = base.transform;
		_transform.localPosition = outSidePos;
	}

	public bool IsPreload(string p_name)
	{
		return pool.ContainsKey(p_name);
	}

	public void CreatePoolBase<T>(string bunldeName, string itemName, int count, Callback p_cb) where T : PoolBaseObject
	{
		if (!IsPreload(itemName))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(bunldeName, itemName, delegate(GameObject obj)
			{
				T component = obj.GetComponent<T>();
				if (component == null)
				{
					Debug.Log(string.Concat(obj.gameObject, "is null"));
					p_cb.CheckTargetToInvoke();
				}
				else
				{
					StartCoroutine(OnStartCreatePoolBase(component, itemName, itemName, count, p_cb));
				}
			});
		}
		else
		{
			p_cb.CheckTargetToInvoke();
		}
	}

	public void CreatePoolBaseLocal<T>(PoolBaseObject go, string itemName, int count = 2) where T : PoolBaseObject
	{
		go.transform.localPosition = outSidePos;
		if (IsPreload(itemName))
		{
			Object.Destroy(go.gameObject);
			return;
		}
		T component = go.GetComponent<T>();
		if (component == null)
		{
			Debug.Log(string.Concat(go.gameObject, "is null"));
			Object.Destroy(go.gameObject);
		}
		else
		{
			OnCreatePoolItem(component, itemName, itemName, count);
			Object.Destroy(go.gameObject);
		}
	}

	public void CreatePoolBaseLocal<T>(PoolBaseObject go, string itemName, string instanceName, int count = 2) where T : PoolBaseObject
	{
		if (IsPreload(itemName))
		{
			Object.Destroy(go.gameObject);
			return;
		}
		T component = go.GetComponent<T>();
		if (component == null)
		{
			Debug.Log(string.Concat(go.gameObject, "is null"));
			Object.Destroy(go.gameObject);
		}
		else
		{
			OnCreatePoolItem(component, itemName, instanceName, count);
			Object.Destroy(go.gameObject);
		}
	}

	private IEnumerator OnStartCreatePoolBase<T>(T obj, string itemName, string instanceName, int count, Callback p_cb) where T : PoolBaseObject
	{
		OnCreatePoolItem(obj, itemName, instanceName, count);
		yield return CoroutineDefine._waitForEndOfFrame;
		p_cb.CheckTargetToInvoke();
	}

	private void OnCreatePoolItem<T>(T obj, string itemName, string instanceName, int count) where T : PoolBaseObject
	{
		if (!IsPreload(itemName))
		{
			Vector2 itemPosition = new Vector2(pool.Count * 100, 0f);
			Queue<PoolBaseObject> queue = new Queue<PoolBaseObject>();
			PoolInfo poolInfo = new PoolInfo(itemPosition, queue);
			pool.Add(itemName, poolInfo);
			for (int i = 0; i < count; i++)
			{
				T val = Object.Instantiate(obj, _transform);
				val.itemName = itemName;
				val.name = instanceName;
				val.transform.localPosition = poolInfo.itemPosition;
				queue.Enqueue(val);
			}
		}
	}

	public bool ExistsInPool<T>(string itemName) where T : PoolBaseObject
	{
		PoolInfo value = null;
		return pool.TryGetValue(itemName, out value);
	}

	public T GetPoolObj<T>(string itemName) where T : PoolBaseObject
	{
		PoolInfo value = null;
		if (pool.TryGetValue(itemName, out value))
		{
			if (value.queue.Count > 1)
			{
				T obj = value.queue.Dequeue() as T;
				obj.ResetStatus();
				return obj;
			}
			return CreatePoolObjDynamic<T>(value);
		}
		Debug.LogError(string.Format("Can't find {0} in pool...please check is alreay call 'CraetePoolBase'", itemName));
		return null;
	}

	public bool ExpandPoolItem<T>(string itemName, int countMax) where T : PoolBaseObject
	{
		PoolInfo value = null;
		if (pool.TryGetValue(itemName, out value))
		{
			int num = countMax - value.queue.Count();
			if (num > 0)
			{
				StartCoroutine(OnStartExpandPoolItem<T>(value, num));
			}
			return true;
		}
		return false;
	}

	private IEnumerator OnStartExpandPoolItem<T>(PoolInfo poolInfo, int count) where T : PoolBaseObject
	{
		for (int i = 0; i < count; i++)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			poolInfo.queue.Enqueue(CreatePoolObjDynamic<T>(poolInfo));
		}
	}

	private T CreatePoolObjDynamic<T>(PoolInfo poolInfo) where T : PoolBaseObject
	{
		PoolBaseObject poolBaseObject = poolInfo.queue.Peek();
		bool activeSelf = poolBaseObject.gameObject.activeSelf;
		poolBaseObject.gameObject.SetActive(true);
		T obj = Object.Instantiate(poolBaseObject, _transform) as T;
		obj.ResetStatus();
		obj.name = poolBaseObject.name;
		poolBaseObject.gameObject.SetActive(activeSelf);
		return obj;
	}

	public void BackToPool<T>(T t, string itemName) where T : PoolBaseObject
	{
		PoolInfo value = null;
		if (pool.TryGetValue(itemName, out value))
		{
			value.queue.Enqueue(t);
			t.transform.SetParent(_transform);
			t.transform.localPosition = value.itemPosition;
			t.transform.localScale = one;
		}
		else
		{
			Debug.LogWarning("Can't find pool key:" + itemName);
			Object.Destroy(t.gameObject);
		}
	}

	public void ClearPoolItem(string itemName)
	{
		PoolInfo value = null;
		if (!pool.TryGetValue(itemName, out value))
		{
			return;
		}
		PoolBaseObject poolBaseObject = null;
		while (value.queue.Count > 0)
		{
			poolBaseObject = value.queue.Dequeue();
			if (null != poolBaseObject)
			{
				Object.Destroy(poolBaseObject.gameObject);
			}
		}
		value.queue.Clear();
		pool.Remove(itemName);
	}

	public void AsyncClearPoolAll(Callback p_cb)
	{
		StartCoroutine(OnStartAsyncClearPoolAll(p_cb));
	}

	private IEnumerator OnStartAsyncClearPoolAll(Callback p_cb)
	{
		string[] keys = pool.Keys.ToArray();
		for (int i = 0; i < keys.Length; i++)
		{
			ClearPoolItem(keys[i]);
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		pool.Clear();
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		if (componentsInChildren != null)
		{
			base.transform.DetachChildren();
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				if (!(transform == base.transform))
				{
					Object.Destroy(transform.gameObject);
				}
			}
		}
		if (p_cb != null && p_cb.Target != null)
		{
			p_cb();
		}
	}
}
