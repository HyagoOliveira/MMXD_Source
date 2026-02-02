#define RELEASE
using System.Collections.Generic;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class GridHash
	{
		public class Point
		{
			public int id;

			public float3 pos;
		}

		protected Dictionary<uint, List<Point>> gridMap;

		protected float gridSize = 0.1f;

		public virtual void Create(float gridSize = 0.1f)
		{
			gridMap = new Dictionary<uint, List<Point>>();
			this.gridSize = gridSize;
		}

		public virtual void AddPoint(float3 pos, int id)
		{
			Point item = new Point
			{
				id = id,
				pos = pos
			};
			uint gridHash = GetGridHash(pos, gridSize);
			if (gridMap.ContainsKey(gridHash))
			{
				gridMap[gridHash].Add(item);
				return;
			}
			List<Point> list = new List<Point>();
			list.Add(item);
			gridMap.Add(gridHash, list);
		}

		public virtual void Remove(float3 pos, int id)
		{
			uint gridHash = GetGridHash(pos, gridSize);
			if (gridMap.ContainsKey(gridHash))
			{
				List<Point> list = gridMap[gridHash];
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].id == id)
					{
						list.RemoveAt(i);
						break;
					}
				}
			}
			else
			{
				Debug.LogError("remove faild!");
			}
		}

		public void Clear()
		{
			gridMap.Clear();
		}

		public static int3 GetGridPos(float3 pos, float gridSize)
		{
			return math.int3(math.floor(pos / gridSize));
		}

		public static uint GetGridHash(int3 pos)
		{
			return ((uint)pos.x & 0x3FFu) | (uint)((pos.y & 0x3FF) << 10) | (uint)((pos.z & 0x3FF) << 20);
		}

		public static uint GetGridHash(float3 pos, float gridSize)
		{
			return GetGridHash(GetGridPos(pos, gridSize));
		}
	}
}
