using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

namespace MagicaCloth
{
	public class FixedTransformAccessArray : IDisposable
	{
		private TransformAccessArray transformArray;

		private int nativeLength;

		private Queue<int> emptyStack = new Queue<int>();

		private Dictionary<int, int> useIndexDict = new Dictionary<int, int>();

		private Dictionary<int, int> indexDict = new Dictionary<int, int>();

		private Dictionary<int, int> referenceDict = new Dictionary<int, int>();

		public int Count
		{
			get
			{
				return useIndexDict.Count;
			}
		}

		public int Length
		{
			get
			{
				return nativeLength;
			}
		}

		public Transform this[int index]
		{
			get
			{
				return transformArray[index];
			}
		}

		public FixedTransformAccessArray(int desiredJobCount = -1)
		{
			transformArray = new TransformAccessArray(0, desiredJobCount);
			nativeLength = transformArray.length;
		}

		public FixedTransformAccessArray(int capacity, int desiredJobCount)
		{
			transformArray = new TransformAccessArray(capacity, desiredJobCount);
			nativeLength = transformArray.length;
		}

		public int Add(Transform element)
		{
			int num = 0;
			int instanceID = element.GetInstanceID();
			if (referenceDict.ContainsKey(instanceID))
			{
				referenceDict[instanceID] += 1;
				return indexDict[instanceID];
			}
			if (emptyStack.Count > 0)
			{
				num = emptyStack.Dequeue();
				transformArray[num] = element;
			}
			else
			{
				num = transformArray.length;
				transformArray.Add(element);
			}
			useIndexDict.Add(num, instanceID);
			indexDict.Add(instanceID, num);
			referenceDict.Add(instanceID, 1);
			nativeLength = transformArray.length;
			return num;
		}

		public void Remove(int index)
		{
			if (useIndexDict.ContainsKey(index))
			{
				int key = useIndexDict[index];
				int num = referenceDict[key] - 1;
				if (num > 0)
				{
					referenceDict[key] = num;
					return;
				}
				transformArray[index] = null;
				emptyStack.Enqueue(index);
				useIndexDict.Remove(index);
				indexDict.Remove(key);
				referenceDict.Remove(key);
				nativeLength = transformArray.length;
			}
		}

		public bool Exist(int index)
		{
			return useIndexDict.ContainsKey(index);
		}

		public bool Exist(Transform element)
		{
			if (element == null)
			{
				return false;
			}
			return indexDict.ContainsKey(element.GetInstanceID());
		}

		public int GetIndex(Transform element)
		{
			if (element == null)
			{
				return -1;
			}
			int instanceID = element.GetInstanceID();
			if (indexDict.ContainsKey(instanceID))
			{
				return indexDict[instanceID];
			}
			return -1;
		}

		public void Clear()
		{
			foreach (int key in useIndexDict.Keys)
			{
				emptyStack.Enqueue(key);
			}
			useIndexDict.Clear();
			int i = 0;
			for (int length = Length; i < length; i++)
			{
				transformArray[i] = null;
			}
			indexDict.Clear();
			referenceDict.Clear();
			nativeLength = 0;
		}

		public void Dispose()
		{
			if (transformArray.isCreated)
			{
				transformArray.Dispose();
			}
			emptyStack.Clear();
			useIndexDict.Clear();
			indexDict.Clear();
			referenceDict.Clear();
			nativeLength = 0;
		}

		public TransformAccessArray GetTransformAccessArray()
		{
			return transformArray;
		}
	}
}
