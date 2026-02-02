using System;
using System.Collections.Generic;

namespace MagicaCloth
{
	public class ReferenceDataBuilder<T> where T : struct
	{
		private int indexCount;

		private List<T> dataList = new List<T>();

		private List<List<int>> indexToDataIndexList = new List<List<int>>();

		private List<List<int>> dataIndexToIndexList = new List<List<int>>();

		public void Init(int indexCount)
		{
			this.indexCount = indexCount;
			for (int i = 0; i < indexCount; i++)
			{
				indexToDataIndexList.Add(new List<int>());
			}
		}

		public void AddData(T data, params int[] indexes)
		{
			int count = dataList.Count;
			dataList.Add(data);
			dataIndexToIndexList.Add(new List<int>());
			foreach (int num in indexes)
			{
				indexToDataIndexList[num].Add(count);
				dataIndexToIndexList[count].Add(num);
			}
		}

		public ValueTuple<List<ReferenceDataIndex>, List<T>> GetDirectReferenceData()
		{
			List<ReferenceDataIndex> list = new List<ReferenceDataIndex>();
			List<T> list2 = new List<T>();
			int num = 0;
			for (int i = 0; i < indexToDataIndexList.Count; i++)
			{
				List<int> list3 = indexToDataIndexList[i];
				ReferenceDataIndex item = default(ReferenceDataIndex);
				item.startIndex = num;
				item.count = list3.Count;
				list.Add(item);
				foreach (int item2 in list3)
				{
					list2.Add(dataList[item2]);
				}
				num += list3.Count;
			}
			return new ValueTuple<List<ReferenceDataIndex>, List<T>>(list, list2);
		}

		public ValueTuple<List<ReferenceDataIndex>, List<int>, List<List<int>>> GetIndirectReferenceData()
		{
			List<ReferenceDataIndex> list = new List<ReferenceDataIndex>();
			int num = 0;
			for (int i = 0; i < indexToDataIndexList.Count; i++)
			{
				List<int> list2 = indexToDataIndexList[i];
				ReferenceDataIndex item = default(ReferenceDataIndex);
				item.startIndex = num;
				item.count = list2.Count;
				list.Add(item);
				num += list2.Count;
			}
			List<int> list3 = new List<int>();
			foreach (List<int> indexToDataIndex in indexToDataIndexList)
			{
				foreach (int item2 in indexToDataIndex)
				{
					list3.Add(item2);
				}
			}
			List<List<int>> list4 = new List<List<int>>();
			for (int j = 0; j < dataIndexToIndexList.Count; j++)
			{
				List<int> list5 = dataIndexToIndexList[j];
				List<int> list6 = new List<int>();
				foreach (int item3 in list5)
				{
					num = list[item3].startIndex;
					int num2 = indexToDataIndexList[item3].IndexOf(j);
					list6.Add(num + num2);
				}
				list4.Add(list6);
			}
			return new ValueTuple<List<ReferenceDataIndex>, List<int>, List<List<int>>>(list, list3, list4);
		}
	}
}
