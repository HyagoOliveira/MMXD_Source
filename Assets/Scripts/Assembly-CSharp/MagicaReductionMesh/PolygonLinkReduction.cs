#define RELEASE
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
	public class PolygonLinkReduction
	{
		public class Point
		{
			public MeshData.ShareVertex shareVertex;

			public Point nearPoint;

			public float nearDist;
		}

		protected MeshData meshData;

		private float reductionLength;

		private List<Point> pointList = new List<Point>();

		private Dictionary<MeshData.ShareVertex, Point> pointDict = new Dictionary<MeshData.ShareVertex, Point>();

		public int PointCount
		{
			get
			{
				return pointList.Count;
			}
		}

		public PolygonLinkReduction(float length)
		{
			reductionLength = length;
		}

		public void Create(MeshData meshData)
		{
			this.meshData = meshData;
			foreach (MeshData.ShareVertex shareVertex in meshData.shareVertexList)
			{
				AddPoint(shareVertex);
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
				foreach (MeshData.ShareVertex item in shareVertex.linkShareVertexSet)
				{
					list.Add(pointDict[item]);
				}
				foreach (MeshData.ShareVertex item2 in shareVertex2.linkShareVertexSet)
				{
					list.Add(pointDict[item2]);
				}
				list.Add(point);
				foreach (Point item3 in list)
				{
					item3.nearPoint = null;
					item3.nearDist = 100000f;
				}
				Remove(nearPoint);
				nearPoint = null;
				meshData.CombineVertex(shareVertex, shareVertex2);
				foreach (Point item4 in list)
				{
					SearchNearPoint(item4);
				}
			}
		}

		private void AddPoint(MeshData.ShareVertex sv)
		{
			Point point = new Point();
			point.shareVertex = sv;
			pointList.Add(point);
			pointDict.Add(sv, point);
		}

		private Point GetPoint(MeshData.ShareVertex sv)
		{
			if (pointDict.ContainsKey(sv))
			{
				return pointDict[sv];
			}
			return null;
		}

		private void Remove(Point p)
		{
			pointDict.Remove(p.shareVertex);
			pointList.Remove(p);
		}

		private void SearchNearPointAll()
		{
			foreach (Point point in pointList)
			{
				SearchNearPoint(point);
			}
		}

		private void SearchNearPoint(Point p)
		{
			p.nearPoint = null;
			p.nearDist = 100000f;
			Vector3 wpos = p.shareVertex.wpos;
			foreach (MeshData.ShareVertex item in p.shareVertex.linkShareVertexSet)
			{
				float num = Vector3.Distance(wpos, item.wpos);
				if (num < p.nearDist && num <= reductionLength)
				{
					p.nearDist = num;
					p.nearPoint = pointDict[item];
				}
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
