using System;
using System.Collections.Generic;
using Unity.Collections;

namespace MagicaCloth
{
	public class FixedNativeListWithCount<T> : IDisposable where T : unmanaged
	{
		private NativeList<T> nativeList;

		private int nativeLength;

		private Queue<int> emptyStack = new Queue<int>();

		private HashSet<int> useIndexSet = new HashSet<int>();

		private Dictionary<T, int> indexDict = new Dictionary<T, int>();

		private Dictionary<T, int> countDict = new Dictionary<T, int>();

		private T emptyElement;

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
				return nativeList[index];
			}
			set
			{
				nativeList[index] = value;
			}
		}

		public FixedNativeListWithCount()
		{
			nativeList = new NativeList<T>(Allocator.Persistent);
			nativeLength = nativeList.Length;
			emptyElement = new T();
		}

		public FixedNativeListWithCount(int capacity)
		{
			nativeList = new NativeList<T>(capacity, Allocator.Persistent);
			nativeLength = nativeList.Length;
		}

		public void Dispose()
		{
			if (nativeList.IsCreated)
			{
				nativeList.Dispose();
			}
			nativeLength = 0;
			emptyStack.Clear();
			useIndexSet.Clear();
			indexDict.Clear();
			countDict.Clear();
		}

		public void SetEmptyElement(T empty)
		{
			emptyElement = empty;
		}

		public int Add(T element)
		{
			int num = 0;
			if (indexDict.ContainsKey(element))
			{
				num = indexDict[element];
				countDict[element] += 1;
			}
			else
			{
				if (emptyStack.Count > 0)
				{
					num = emptyStack.Dequeue();
					nativeList[num] = element;
				}
				else
				{
					num = nativeList.Length;
					nativeList.Add(element);
					nativeLength = nativeList.Length;
				}
				useIndexSet.Add(num);
				indexDict[element] = num;
				countDict[element] = 1;
			}
			return num;
		}

		public void Remove(T element)
		{
			if (indexDict.ContainsKey(element))
			{
				int num = countDict[element];
				if (num <= 1)
				{
					int num2 = indexDict[element];
					nativeList[num2] = emptyElement;
					emptyStack.Enqueue(num2);
					useIndexSet.Remove(num2);
					indexDict.Remove(element);
					countDict.Remove(element);
				}
				else
				{
					countDict[element] = num - 1;
				}
			}
		}

		public bool Exist(T element)
		{
			return indexDict.ContainsKey(element);
		}

		public int GetUseCount(T element)
		{
			if (countDict.ContainsKey(element))
			{
				return countDict[element];
			}
			return 0;
		}

		public void Clear()
		{
			nativeList.Clear();
			nativeLength = 0;
			emptyStack.Clear();
			useIndexSet.Clear();
			indexDict.Clear();
			countDict.Clear();
		}

		public T[] ToArray()
		{
			return nativeList.ToArray();
		}

		public NativeArray<T> ToJobArray()
		{
			return nativeList.AsArray();
		}
	}
}
