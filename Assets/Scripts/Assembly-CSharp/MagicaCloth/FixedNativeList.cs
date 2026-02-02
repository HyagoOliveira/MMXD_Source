using System;
using System.Collections.Generic;
using Unity.Collections;

namespace MagicaCloth
{
	public class FixedNativeList<T> : IDisposable where T : struct
	{
		private NativeArray<T> nativeArray0;

		private NativeArray<T> nativeArray1;

		private int nativeLength;

		private Queue<int> emptyStack = new Queue<int>();

		private HashSet<int> useIndexSet = new HashSet<int>();

		private int useLength;

		public int Length
		{
			get
			{
				return nativeLength;
			}
		}

		public int Count
		{
			get
			{
				return useIndexSet.Count;
			}
		}

		public T this[int index]
		{
			get
			{
				return nativeArray0[index];
			}
			set
			{
				nativeArray0[index] = value;
			}
		}

		public FixedNativeList()
		{
			nativeArray0 = new NativeArray<T>(8, Allocator.Persistent);
			nativeLength = nativeArray0.Length;
			useLength = 0;
		}

		public void Dispose()
		{
			if (nativeArray0.IsCreated)
			{
				nativeArray0.Dispose();
			}
			if (nativeArray1.IsCreated)
			{
				nativeArray1.Dispose();
			}
			nativeLength = 0;
			emptyStack.Clear();
			useIndexSet.Clear();
			useLength = 0;
		}

		public int Add(T element)
		{
			int num = 0;
			if (emptyStack.Count > 0)
			{
				num = emptyStack.Dequeue();
				nativeArray0[num] = element;
			}
			else
			{
				if (nativeArray0.Length <= useLength)
				{
					int i;
					for (i = nativeArray0.Length; i <= useLength; i += i)
					{
					}
					NativeArray<T> nativeArray = new NativeArray<T>(i, Allocator.Persistent);
					nativeArray.CopyFromFast(nativeArray0);
					nativeArray0.Dispose();
					nativeArray0 = nativeArray;
				}
				num = useLength;
				nativeArray0[num] = element;
				nativeLength = nativeArray0.Length;
				useLength++;
			}
			useIndexSet.Add(num);
			return num;
		}

		public void Remove(int index)
		{
			if (useIndexSet.Contains(index))
			{
				nativeArray0[index] = default(T);
				emptyStack.Enqueue(index);
				useIndexSet.Remove(index);
			}
		}

		public bool Exists(int index)
		{
			return useIndexSet.Contains(index);
		}

		public void Clear()
		{
			nativeLength = 0;
			emptyStack.Clear();
			useIndexSet.Clear();
			useLength = 0;
		}

		public NativeArray<T> ToJobArray()
		{
			return nativeArray0;
		}

		public NativeArray<T> ToJobArray(int bufferIndex)
		{
			if (bufferIndex != 0)
			{
				return nativeArray1;
			}
			return nativeArray0;
		}

		public void SyncBuffer()
		{
			if (!nativeArray1.IsCreated || nativeArray1.Length != nativeArray0.Length)
			{
				if (nativeArray1.IsCreated)
				{
					nativeArray1.Dispose();
				}
				nativeArray1 = new NativeArray<T>(nativeArray0.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			}
		}
	}
}
