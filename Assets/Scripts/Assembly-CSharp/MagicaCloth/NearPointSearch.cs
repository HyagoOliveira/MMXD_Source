using System.Collections.Generic;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class NearPointSearch : GridHash
	{
		private float radius;

		private Dictionary<int, int> nearDict = new Dictionary<int, int>();

		private Dictionary<int, float> distDict = new Dictionary<int, float>();

		private HashSet<uint> lockPairSet = new HashSet<uint>();

		public void Create(float3[] positionList, float radius)
		{
			base.Create(radius);
			this.radius = radius;
			for (int i = 0; i < positionList.Length; i++)
			{
				AddPoint(positionList[i], i);
			}
		}

		public void SearchNearPointAll()
		{
			foreach (List<Point> value in gridMap.Values)
			{
				foreach (Point item in value)
				{
					SearchNearPoint(item.id, item.pos);
				}
			}
		}

		public void SearchNearPoint(int id, float3 pos)
		{
			int num = -1;
			float num2 = 100000f;
			int3 gridPos = GridHash.GetGridPos(pos - radius, gridSize);
			int3 gridPos2 = GridHash.GetGridPos(pos + radius, gridSize);
			for (int i = gridPos.x; i <= gridPos2.x; i++)
			{
				for (int j = gridPos.y; j <= gridPos2.y; j++)
				{
					for (int k = gridPos.z; k <= gridPos2.z; k++)
					{
						uint gridHash = GridHash.GetGridHash(new int3(i, j, k));
						if (!gridMap.ContainsKey(gridHash))
						{
							continue;
						}
						foreach (Point item in gridMap[gridHash])
						{
							if (item.id != id)
							{
								float num3 = math.length(pos - item.pos);
								if (num3 < num2)
								{
									num = item.id;
									num2 = num3;
								}
							}
						}
					}
				}
			}
			if (num >= 0)
			{
				nearDict[id] = num;
				distDict[id] = num2;
			}
			else if (nearDict.ContainsKey(id))
			{
				nearDict.Remove(id);
				distDict.Remove(id);
			}
		}

		public void SearchNearPoint(float3 pos, float r)
		{
			int3 gridPos = GridHash.GetGridPos(pos - r, gridSize);
			int3 gridPos2 = GridHash.GetGridPos(pos + r, gridSize);
			for (int i = gridPos.x; i <= gridPos2.x; i++)
			{
				for (int j = gridPos.y; j <= gridPos2.y; j++)
				{
					for (int k = gridPos.z; k <= gridPos2.z; k++)
					{
						uint gridHash = GridHash.GetGridHash(new int3(i, j, k));
						if (!gridMap.ContainsKey(gridHash))
						{
							continue;
						}
						foreach (Point item in gridMap[gridHash])
						{
							SearchNearPoint(item.id, item.pos);
						}
					}
				}
			}
		}

		public override void AddPoint(float3 pos, int id)
		{
			base.AddPoint(pos, id);
		}

		public override void Remove(float3 pos, int id)
		{
			base.Remove(pos, id);
			if (nearDict.ContainsKey(id))
			{
				nearDict.Remove(id);
				distDict.Remove(id);
			}
		}

		public void AddLockPair(int id1, int id2)
		{
			uint item = DataUtility.PackPair(id1, id2);
			lockPairSet.Add(item);
		}

		public bool GetNearPointPair(out int id1, out int id2)
		{
			int num = -1;
			int num2 = -1;
			float num3 = 100000f;
			foreach (KeyValuePair<int, int> item2 in nearDict)
			{
				int key = item2.Key;
				int value = item2.Value;
				if (value == -1)
				{
					continue;
				}
				uint item = DataUtility.PackPair(key, value);
				if (!lockPairSet.Contains(item))
				{
					float num4 = distDict[key];
					if (!(num4 > radius) && num4 < num3)
					{
						num = key;
						num2 = value;
						num3 = num4;
					}
				}
			}
			if (num >= 0 && num2 >= 0)
			{
				id1 = num;
				id2 = num2;
				return true;
			}
			id1 = -1;
			id2 = -1;
			return false;
		}

		public override string ToString()
		{
			string text = "";
			foreach (KeyValuePair<int, int> item in nearDict)
			{
				text += string.Format("[{0}] -> {1} {2}\n", item.Key, item.Value, distDict[item.Key]);
			}
			return text;
		}
	}
}
