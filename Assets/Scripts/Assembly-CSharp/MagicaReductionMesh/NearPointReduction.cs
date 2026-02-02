#define RELEASE
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
	public class NearPointReduction
	{
		public class Point
		{
			public MeshData.ShareVertex shareVertex;

			public Vector3 pos;

			public Vector3Int grid;

			public Point nearPoint;

			public float nearDist;
		}

		protected MeshData meshData;

		private List<Point> pointList = new List<Point>();

		protected Dictionary<Vector3Int, List<Point>> gridMap = new Dictionary<Vector3Int, List<Point>>();

		protected float gridSize = 0.05f;

		private float searchRadius;

		private Dictionary<Point, List<Point>> nearPointDict = new Dictionary<Point, List<Point>>();

		public int PointCount
		{
			get
			{
				return pointList.Count;
			}
		}

		public NearPointReduction(float radius = 0.05f)
		{
			searchRadius = radius;
			gridSize = radius * 2f;
		}

		public void Create(MeshData meshData)
		{
			this.meshData = meshData;
			foreach (MeshData.ShareVertex shareVertex in meshData.shareVertexList)
			{
				AddPoint(shareVertex, shareVertex.wpos);
			}
			SearchNearPointAll();
		}

		public void Reduction()
		{
			Point point = null;
			List<Point> list = new List<Point>();
			while ((point = GetNearPointPair()) != null)
			{
				Point nearPoint = point.nearPoint;
				Debug.Assert(nearPoint != null);
				MeshData.ShareVertex shareVertex = point.shareVertex;
				MeshData.ShareVertex shareVertex2 = nearPoint.shareVertex;
				list.Clear();
				if (nearPointDict.ContainsKey(point))
				{
					list.AddRange(nearPointDict[point]);
					nearPointDict.Remove(point);
				}
				if (nearPointDict.ContainsKey(nearPoint))
				{
					list.AddRange(nearPointDict[nearPoint]);
					nearPointDict.Remove(nearPoint);
				}
				list.Add(point);
				foreach (Point item in list)
				{
					item.nearPoint = null;
					item.nearDist = 100000f;
				}
				Remove(nearPoint);
				nearPoint = null;
				meshData.CombineVertex(shareVertex, shareVertex2);
				Move(point, shareVertex.wpos);
				foreach (Point item2 in list)
				{
					SearchNearPoint(item2, searchRadius, null);
				}
			}
		}

		private Point AddPoint(MeshData.ShareVertex sv, Vector3 pos)
		{
			Point point = new Point
			{
				shareVertex = sv,
				pos = pos
			};
			pointList.Add(point);
			AddGrid(point);
			return point;
		}

		private void AddGrid(Point p)
		{
			Vector3Int key = (p.grid = GetGridPos(p.pos));
			if (gridMap.ContainsKey(key))
			{
				gridMap[key].Add(p);
				return;
			}
			List<Point> list = new List<Point>();
			list.Add(p);
			gridMap.Add(key, list);
		}

		private void RemoveGrid(Point p)
		{
			Vector3Int grid = p.grid;
			if (gridMap.ContainsKey(grid))
			{
				List<Point> list = gridMap[grid];
				list.Remove(p);
				if (list.Count == 0)
				{
					gridMap.Remove(grid);
				}
			}
			else
			{
				Debug.LogError("remove faild!");
			}
			p.grid = Vector3Int.zero;
		}

		private void Move(Point p, Vector3 newpos)
		{
			RemoveGrid(p);
			p.pos = newpos;
			AddGrid(p);
		}

		private void Remove(Point p)
		{
			RemoveGrid(p);
			pointList.Remove(p);
		}

		protected Vector3Int GetGridPos(Vector3 pos)
		{
			Vector3 vector = pos / gridSize;
			return new Vector3Int((int)Mathf.Floor(vector.x), (int)Mathf.Floor(vector.y), (int)Mathf.Floor(vector.z));
		}

		private void SearchNearPointAll()
		{
			nearPointDict.Clear();
			foreach (List<Point> value in gridMap.Values)
			{
				foreach (Point item in value)
				{
					SearchNearPoint(item, searchRadius, null);
				}
			}
		}

		private void SearchNearPoint(Point p, float radius, Point ignorePoint)
		{
			Point point = null;
			float num = 100000f;
			if (p.nearPoint != null && nearPointDict.ContainsKey(p.nearPoint))
			{
				nearPointDict[p.nearPoint].Remove(p);
			}
			Vector3Int grid = p.grid;
			int num2 = (int)(radius / gridSize) + 1;
			Vector3Int vector3Int = new Vector3Int(num2, num2, num2);
			Vector3Int vector3Int2 = grid - vector3Int;
			Vector3Int vector3Int3 = grid + vector3Int;
			Vector3Int zero = Vector3Int.zero;
			for (int i = vector3Int2.x; i <= vector3Int3.x; i++)
			{
				zero.x = i;
				for (int j = vector3Int2.y; j <= vector3Int3.y; j++)
				{
					zero.y = j;
					for (int k = vector3Int2.z; k <= vector3Int3.z; k++)
					{
						zero.z = k;
						if (!gridMap.ContainsKey(zero))
						{
							continue;
						}
						foreach (Point item in gridMap[zero])
						{
							if (item != p && item != ignorePoint)
							{
								float num3 = Vector3.Distance(item.pos, p.pos);
								if (num3 < num && num3 <= radius)
								{
									point = item;
									num = num3;
								}
							}
						}
					}
				}
			}
			if (point != null)
			{
				p.nearPoint = point;
				p.nearDist = num;
				if (!nearPointDict.ContainsKey(point))
				{
					nearPointDict.Add(point, new List<Point>());
				}
				nearPointDict[point].Add(p);
			}
			else
			{
				p.nearPoint = null;
				p.nearDist = 100000f;
			}
		}

		private Point GetNearPointPair()
		{
			float num = 10000f;
			Point result = null;
			foreach (Point point in pointList)
			{
				if (point.nearPoint != null && point.nearDist < num)
				{
					num = point.nearDist;
					result = point;
				}
			}
			return result;
		}
	}
}
