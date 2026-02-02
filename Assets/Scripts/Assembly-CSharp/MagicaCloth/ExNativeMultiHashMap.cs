using System;
using System.Collections.Generic;
using Unity.Collections;

namespace MagicaCloth
{
	public class ExNativeMultiHashMap<TKey, TValue> where TKey : struct, IEquatable<TKey> where TValue : struct
	{
		private NativeParallelMultiHashMap<TKey, TValue> nativeMultiHashMap;

		private int nativeLength;

		private Dictionary<TKey, int> useKeyDict = new Dictionary<TKey, int>();

		private int NativeCount
		{
			get
			{
				return nativeMultiHashMap.Count();
			}
		}

		public bool IsCreated
		{
			get
			{
				return nativeMultiHashMap.IsCreated;
			}
		}

		public int Count
		{
			get
			{
				return nativeLength;
			}
		}

		public NativeParallelMultiHashMap<TKey, TValue> Map
		{
			get
			{
				return nativeMultiHashMap;
			}
		}

		public ExNativeMultiHashMap()
		{
			nativeMultiHashMap = new NativeParallelMultiHashMap<TKey, TValue>(1, Allocator.Persistent);
			nativeLength = NativeCount;
		}

		public void Dispose()
		{
			if (nativeMultiHashMap.IsCreated)
			{
				nativeMultiHashMap.Dispose();
			}
			nativeLength = 0;
		}

		public void Add(TKey key, TValue value)
		{
			nativeMultiHashMap.Add(key, value);
			if (useKeyDict.ContainsKey(key))
			{
				useKeyDict[key] += 1;
			}
			else
			{
				useKeyDict[key] = 1;
			}
			nativeLength = NativeCount;
		}

		public void Remove(TKey key, TValue value)
		{
			TValue item;
			NativeParallelMultiHashMapIterator<TKey> it;
			if (nativeMultiHashMap.TryGetFirstValue(key, out item, out it))
			{
				do
				{
					if (item.Equals(value))
					{
						nativeMultiHashMap.Remove(it);
						if (useKeyDict[key] - 1 == 0)
						{
							useKeyDict.Remove(key);
						}
						break;
					}
				}
				while (nativeMultiHashMap.TryGetNextValue(out item, ref it));
			}
			nativeLength = NativeCount;
		}

		public void Remove(Func<TKey, TValue, bool> func)
		{
			List<TKey> list = new List<TKey>();
			foreach (TKey key in useKeyDict.Keys)
			{
				TValue item;
				NativeParallelMultiHashMapIterator<TKey> it;
				if (!nativeMultiHashMap.TryGetFirstValue(key, out item, out it))
				{
					continue;
				}
				do
				{
					if (func(key, item))
					{
						nativeMultiHashMap.Remove(it);
						if (useKeyDict[key] - 1 == 0)
						{
							list.Add(key);
						}
					}
				}
				while (nativeMultiHashMap.TryGetNextValue(out item, ref it));
			}
			foreach (TKey item2 in list)
			{
				useKeyDict.Remove(item2);
			}
			nativeLength = NativeCount;
		}

		public void Replace(Func<TKey, TValue, bool> func, Func<TValue, TValue> datafunc)
		{
			foreach (TKey key in useKeyDict.Keys)
			{
				TValue item;
				NativeParallelMultiHashMapIterator<TKey> it;
				if (!nativeMultiHashMap.TryGetFirstValue(key, out item, out it))
				{
					continue;
				}
				do
				{
					if (func(key, item))
					{
						nativeMultiHashMap.SetValue(datafunc(item), it);
						return;
					}
				}
				while (nativeMultiHashMap.TryGetNextValue(out item, ref it));
			}
			nativeLength = NativeCount;
		}

		public void Process(Action<TKey, TValue> act)
		{
			foreach (TKey key in useKeyDict.Keys)
			{
				TValue item;
				NativeParallelMultiHashMapIterator<TKey> it;
				if (nativeMultiHashMap.TryGetFirstValue(key, out item, out it))
				{
					do
					{
						act(key, item);
					}
					while (nativeMultiHashMap.TryGetNextValue(out item, ref it));
				}
			}
		}

		public void Process(TKey key, Action<TValue> act)
		{
			TValue item;
			NativeParallelMultiHashMapIterator<TKey> it;
			if (nativeMultiHashMap.TryGetFirstValue(key, out item, out it))
			{
				do
				{
					act(item);
				}
				while (nativeMultiHashMap.TryGetNextValue(out item, ref it));
			}
		}

		public bool Contains(TKey key, TValue value)
		{
			TValue item;
			NativeParallelMultiHashMapIterator<TKey> it;
			if (nativeMultiHashMap.TryGetFirstValue(key, out item, out it))
			{
				do
				{
					if (item.Equals(value))
					{
						return true;
					}
				}
				while (nativeMultiHashMap.TryGetNextValue(out item, ref it));
			}
			return false;
		}

		public bool Contains(TKey key)
		{
			return useKeyDict.ContainsKey(key);
		}

		public void Remove(TKey key)
		{
			nativeMultiHashMap.Remove(key);
			useKeyDict.Remove(key);
			nativeLength = NativeCount;
		}

		public void Clear()
		{
			nativeMultiHashMap.Clear();
			nativeLength = 0;
			useKeyDict.Clear();
		}
	}
}
