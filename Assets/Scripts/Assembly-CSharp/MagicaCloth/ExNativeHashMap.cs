using System;
using System.Collections.Generic;
using Unity.Collections;

namespace MagicaCloth
{
	public class ExNativeHashMap<TKey, TValue> where TKey : struct, IEquatable<TKey> where TValue : struct
	{
		private NativeParallelHashMap<TKey, TValue> nativeHashMap;

		private int nativeLength;

		private HashSet<TKey> useKeySet = new HashSet<TKey>();

		private int NativeCount
		{
			get
			{
				return nativeHashMap.Count();
			}
		}

		public int Count
		{
			get
			{
				return nativeLength;
			}
		}

		public NativeParallelHashMap<TKey, TValue> Map
		{
			get
			{
				return nativeHashMap;
			}
		}

		public HashSet<TKey> UseKeySet
		{
			get
			{
				return useKeySet;
			}
		}

		public ExNativeHashMap()
		{
			nativeHashMap = new NativeParallelHashMap<TKey, TValue>(1, Allocator.Persistent);
			nativeLength = NativeCount;
		}

		public void Dispose()
		{
			if (nativeHashMap.IsCreated)
			{
				nativeHashMap.Dispose();
			}
		}

		public void Add(TKey key, TValue value)
		{
			if (!nativeHashMap.TryAdd(key, value))
			{
				nativeHashMap.Remove(key);
				nativeHashMap.TryAdd(key, value);
			}
			useKeySet.Add(key);
			nativeLength = NativeCount;
		}

		public TValue Get(TKey key)
		{
			TValue item;
			nativeHashMap.TryGetValue(key, out item);
			return item;
		}

		public void Remove(Func<TKey, TValue, bool> func)
		{
			List<TKey> list = new List<TKey>();
			foreach (TKey item2 in useKeySet)
			{
				TValue item;
				if (nativeHashMap.TryGetValue(item2, out item) && func(item2, item))
				{
					nativeHashMap.Remove(item2);
					list.Add(item2);
				}
			}
			foreach (TKey item3 in list)
			{
				useKeySet.Remove(item3);
			}
			nativeLength = NativeCount;
		}

		public void Replace(Func<TKey, TValue, bool> func, Func<TValue, TValue> datafunc)
		{
			foreach (TKey item3 in useKeySet)
			{
				TValue item;
				if (nativeHashMap.TryGetValue(item3, out item) && func(item3, item))
				{
					TValue item2 = datafunc(item);
					nativeHashMap.Remove(item3);
					nativeHashMap.TryAdd(item3, item2);
					break;
				}
			}
		}

		public void Remove(TKey key)
		{
			nativeHashMap.Remove(key);
			nativeLength = 0;
			useKeySet.Remove(key);
		}

		public void Clear()
		{
			nativeHashMap.Clear();
			nativeLength = 0;
			useKeySet.Clear();
		}
	}
}
