using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace MagicaCloth
{
	public class FixedMultiNativeList<T> : IDisposable where T : struct
	{
		private NativeArray<T> nativeArray;

		private int nativeLength;

		private List<ChunkData> emptyChunkList = new List<ChunkData>();

		private List<ChunkData> useChunkList = new List<ChunkData>();

		private int chunkSeed;

		private int initLength = 64;

		private T emptyElement;

		private int useLength;

		public int Length
		{
			get
			{
				return nativeLength;
			}
		}

		public int ChunkCount
		{
			get
			{
				return useChunkList.Count;
			}
		}

		public int Count
		{
			get
			{
				int num = 0;
				foreach (ChunkData useChunk in useChunkList)
				{
					num += useChunk.dataLength;
				}
				return num;
			}
		}

		public T this[int index]
		{
			get
			{
				return nativeArray[index];
			}
		}

		public FixedMultiNativeList()
		{
			nativeArray = new NativeArray<T>(initLength, Allocator.Persistent);
			nativeLength = nativeArray.Length;
			useLength = 0;
		}

		public void Dispose()
		{
			if (nativeArray.IsCreated)
			{
				nativeArray.Dispose();
			}
			nativeLength = 0;
			useLength = 0;
			emptyChunkList.Clear();
			useChunkList.Clear();
		}

		public void SetEmptyElement(T empty)
		{
			emptyElement = empty;
		}

		public ChunkData AddChunk(int length)
		{
			ChunkData chunkData2;
			for (int i = 0; i < emptyChunkList.Count; i++)
			{
				ChunkData chunkData = emptyChunkList[i];
				if (chunkData.dataLength >= length)
				{
					int num = chunkData.dataLength - length;
					if (num > 0)
					{
						chunkData2 = default(ChunkData);
						chunkData2.chunkNo = ++chunkSeed;
						chunkData2.startIndex = chunkData.startIndex + length;
						chunkData2.dataLength = num;
						ChunkData value = chunkData2;
						emptyChunkList[i] = value;
					}
					else
					{
						emptyChunkList.RemoveAt(i);
					}
					chunkData.dataLength = length;
					useChunkList.Add(chunkData);
					return chunkData;
				}
			}
			chunkData2 = default(ChunkData);
			chunkData2.chunkNo = ++chunkSeed;
			chunkData2.startIndex = useLength;
			chunkData2.dataLength = length;
			chunkData2.useLength = 0;
			ChunkData chunkData3 = chunkData2;
			useChunkList.Add(chunkData3);
			useLength += length;
			if (this.nativeArray.Length < useLength)
			{
				int j;
				for (j = this.nativeArray.Length; j < useLength; j += Mathf.Min(j, 4096))
				{
				}
				NativeArray<T> nativeArray = new NativeArray<T>(j, Allocator.Persistent);
				nativeArray.CopyFromFast(this.nativeArray);
				this.nativeArray.Dispose();
				this.nativeArray = nativeArray;
				nativeLength = this.nativeArray.Length;
			}
			return chunkData3;
		}

		public void RemoveChunk(int chunkNo)
		{
			for (int i = 0; i < useChunkList.Count; i++)
			{
				if (useChunkList[i].chunkNo != chunkNo)
				{
					continue;
				}
				ChunkData item = useChunkList[i];
				useChunkList.RemoveAt(i);
				nativeArray.SetValue(item.startIndex, item.dataLength, emptyElement);
				int num = 0;
				while (num < emptyChunkList.Count)
				{
					ChunkData chunkData = emptyChunkList[num];
					if (chunkData.startIndex + chunkData.dataLength == item.startIndex)
					{
						chunkData.dataLength += item.dataLength;
						item = chunkData;
						emptyChunkList.RemoveAt(num);
					}
					else if (chunkData.startIndex == item.startIndex + item.dataLength)
					{
						item.dataLength += chunkData.dataLength;
						emptyChunkList.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
				emptyChunkList.Add(item);
				break;
			}
		}

		public void RemoveChunk(ChunkData chunk)
		{
			RemoveChunk(chunk.chunkNo);
		}

		public ChunkData AddData(ChunkData chunk, T data)
		{
			if (chunk.useLength == chunk.dataLength)
			{
				int dataLength = chunk.dataLength;
				dataLength += Mathf.Min(dataLength, 4096);
				ChunkData chunkData = AddChunk(dataLength);
				nativeArray.CopyBlock(chunk.startIndex, chunkData.startIndex, chunk.dataLength);
				chunkData.useLength = chunk.useLength;
				RemoveChunk(chunk);
				chunk = chunkData;
			}
			nativeArray[chunk.startIndex + chunk.useLength] = data;
			chunk.useLength++;
			return chunk;
		}

		public ChunkData RemoveData(ChunkData chunk, T data)
		{
			int num = chunk.startIndex;
			int num2 = 0;
			while (num2 < chunk.useLength)
			{
				if (data.Equals(nativeArray[num]))
				{
					if (num2 < chunk.useLength - 1)
					{
						nativeArray[num] = nativeArray[chunk.startIndex + chunk.useLength - 1];
						nativeArray[chunk.startIndex + chunk.useLength - 1] = emptyElement;
					}
					chunk.useLength--;
				}
				num2++;
				num++;
			}
			return chunk;
		}

		public ChunkData ClearData(ChunkData chunk)
		{
			nativeArray.SetValue(chunk.startIndex, chunk.dataLength, emptyElement);
			chunk.useLength = 0;
			return chunk;
		}

		public void Process(ChunkData chunk, Action<T> act)
		{
			int num = chunk.startIndex;
			int num2 = 0;
			while (num2 < chunk.useLength)
			{
				act(nativeArray[num]);
				num2++;
				num++;
			}
		}

		public NativeArray<T> ToJobArray()
		{
			return nativeArray;
		}

		public override string ToString()
		{
			string empty = string.Empty;
			empty = empty + "nativeList length=" + Length + "\n";
			empty = empty + "use chunk count=" + ChunkCount + "\n";
			empty = empty + "empty chunk count=" + emptyChunkList.Count + "\n";
			empty += "<< use chunks >>\n";
			foreach (ChunkData useChunk in useChunkList)
			{
				empty += useChunk;
			}
			empty += "<< empty chunks >>\n";
			foreach (ChunkData emptyChunk in emptyChunkList)
			{
				empty += emptyChunk;
			}
			return empty;
		}
	}
}
