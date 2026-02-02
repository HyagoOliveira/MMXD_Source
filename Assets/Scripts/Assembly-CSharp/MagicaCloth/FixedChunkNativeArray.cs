using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace MagicaCloth
{
	public class FixedChunkNativeArray<T> : IDisposable where T : struct
	{
		private NativeArray<T> nativeArray0;

		private NativeArray<T> nativeArray1;

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
				return nativeArray0[index];
			}
			set
			{
				nativeArray0[index] = value;
			}
		}

		public FixedChunkNativeArray()
		{
			nativeArray0 = new NativeArray<T>(initLength, Allocator.Persistent);
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
			ChunkData chunkData3 = chunkData2;
			useChunkList.Add(chunkData3);
			useLength += length;
			if (nativeArray0.Length < useLength)
			{
				int j;
				for (j = nativeArray0.Length; j < useLength; j += Mathf.Min(j, 4096))
				{
				}
				NativeArray<T> nativeArray = new NativeArray<T>(j, Allocator.Persistent);
				nativeArray.CopyFromFast(nativeArray0);
				nativeArray0.Dispose();
				nativeArray0 = nativeArray;
				nativeLength = nativeArray0.Length;
			}
			return chunkData3;
		}

		public ChunkData Add(T data)
		{
			ChunkData result = AddChunk(1);
			nativeArray0[result.startIndex] = data;
			return result;
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
				nativeArray0.SetValue(item.startIndex, item.dataLength, emptyElement);
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

		public void Fill(ChunkData chunk, T data)
		{
			nativeArray0.SetValue(chunk.startIndex, chunk.dataLength, data);
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

		public void SwapBuffer()
		{
			NativeArray<T> nativeArray = nativeArray1;
			nativeArray1 = nativeArray0;
			if (!nativeArray.IsCreated || nativeArray.Length != nativeArray0.Length)
			{
				if (nativeArray.IsCreated)
				{
					nativeArray.Dispose();
				}
				nativeArray = new NativeArray<T>(nativeArray0.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
				nativeArray.CopyFromFast(nativeArray0);
			}
			nativeArray0 = nativeArray;
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
