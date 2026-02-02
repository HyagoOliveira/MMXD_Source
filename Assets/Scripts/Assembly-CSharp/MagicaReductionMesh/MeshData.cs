#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaReductionMesh
{
	public class MeshData : ReductionMeshAccess
	{
		public class WeightData
		{
			public int boneIndex;

			public float boneWeight;
		}

		public class Vertex
		{
			public int meshIndex;

			public int vertexIndex;

			public Vector3 wpos;

			public Vector3 wnor;

			public Vector3 wtan;

			public float tanw;

			public Vector2 uv;

			public int parentIndex;

			public List<WeightData> boneWeightList = new List<WeightData>();
		}

		public class MeshInfo
		{
			public int index;

			public Mesh mesh;

			public int vertexCount;

			public List<Vertex> vertexList = new List<Vertex>();
		}

		public class ShareVertex
		{
			public int sindex;

			public Vector3 wpos;

			public Vector3 wnor;

			public Vector3 wtan;

			public float tanw;

			public Vector2 uv;

			public Matrix4x4 worldToLocalMatrix;

			public Matrix4x4 bindpose;

			public List<WeightData> boneWeightList = new List<WeightData>();

			public List<Vertex> vertexList = new List<Vertex>();

			public HashSet<ShareVertex> linkShareVertexSet = new HashSet<ShareVertex>();

			public HashSet<Triangle> linkTriangleSet = new HashSet<Triangle>();

			public void AddLink(ShareVertex mv)
			{
				linkShareVertexSet.Add(mv);
			}

			public void ReplaseLink(ShareVertex old, ShareVertex mv)
			{
				if (linkShareVertexSet.Contains(old))
				{
					linkShareVertexSet.Remove(old);
					linkShareVertexSet.Add(mv);
				}
			}

			public void RecalcCoordinate()
			{
				int num = 0;
				wpos = Vector3.zero;
				wnor = Vector3.zero;
				wtan = Vector3.zero;
				uv = Vector2.zero;
				foreach (Vertex vertex in vertexList)
				{
					wpos += vertex.wpos;
					num++;
				}
				if (num >= 1)
				{
					wpos /= (float)num;
					wnor = vertexList[0].wnor;
					wtan = vertexList[0].wtan;
					uv = vertexList[0].uv;
				}
				Debug.Assert(wnor.magnitude >= 0.0001f);
			}

			public void CalcNormalTangentFromTriangle()
			{
				if (linkTriangleSet.Count <= 0)
				{
					return;
				}
				wnor = Vector3.zero;
				wtan = Vector3.zero;
				foreach (Triangle item in linkTriangleSet)
				{
					wnor += item.wnor;
					wtan += item.wtan;
				}
				wnor.Normalize();
				wtan.Normalize();
				if (wnor.sqrMagnitude == 0f)
				{
					Debug.LogAssertion("Calc triangle normal = 0!");
				}
			}

			public Vector3 CalcLocalPos(Vector3 pos)
			{
				Quaternion quaternion = Quaternion.Inverse(Quaternion.LookRotation(wnor, wtan));
				Vector3 vector = pos - wpos;
				return quaternion * vector;
			}

			public Vector3 CalcLocalDir(Vector3 dir)
			{
				return Quaternion.Inverse(Quaternion.LookRotation(wnor, wtan)) * dir;
			}

			public Matrix4x4 CalcWorldToLocalMatrix()
			{
				Quaternion q = Quaternion.LookRotation(wnor, wtan);
				worldToLocalMatrix = Matrix4x4.TRS(wpos, q, Vector3.one).inverse;
				return worldToLocalMatrix;
			}

			public void CalcBoneWeight(ReductionMesh.ReductionWeightMode weightMode, float weightPow)
			{
				switch (weightMode)
				{
				case ReductionMesh.ReductionWeightMode.Distance:
					CalcBoneWeight_Distance(weightPow);
					break;
				case ReductionMesh.ReductionWeightMode.Average:
					CalcBoneWeight_Average();
					break;
				}
			}

			private void CalcBoneWeight_Average()
			{
				List<WeightData> list = new List<WeightData>();
				foreach (Vertex vertex in vertexList)
				{
					foreach (WeightData w in vertex.boneWeightList)
					{
						WeightData weightData = list.Find((WeightData wdata) => wdata.boneIndex == w.boneIndex);
						if (weightData == null)
						{
							weightData = new WeightData();
							weightData.boneIndex = w.boneIndex;
							list.Add(weightData);
						}
						weightData.boneWeight += w.boneWeight;
					}
				}
				list.Sort((WeightData a, WeightData b) => (!(a.boneWeight - b.boneWeight > 0f)) ? 1 : (-1));
				if (list.Count > 4)
				{
					list.RemoveRange(4, list.Count - 4);
				}
				AdjustWeight(list);
				int num = 0;
				while (num < list.Count)
				{
					if (list[num].boneWeight < 0.01f)
					{
						list.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
				AdjustWeight(list);
				boneWeightList = list;
			}

			private void AdjustWeight(List<WeightData> sumlist)
			{
				float num = 0f;
				foreach (WeightData item in sumlist)
				{
					num += item.boneWeight;
				}
				float num2 = 1f / num;
				foreach (WeightData item2 in sumlist)
				{
					item2.boneWeight *= num2;
				}
			}

			private void CalcBoneWeight_Distance(float weightPow)
			{
				float num = 0f;
				foreach (Vertex vertex in vertexList)
				{
					float b2 = Vector3.Distance(wpos, vertex.wpos);
					num = Mathf.Max(num, b2);
				}
				List<WeightData> list = new List<WeightData>();
				foreach (Vertex vertex2 in vertexList)
				{
					float num2 = 1f;
					if (num > 0f)
					{
						float num3 = Vector3.Distance(wpos, vertex2.wpos);
						num2 = Mathf.Clamp01(1f - num3 / num + 0.001f);
						num2 = Mathf.Pow(num2, weightPow);
					}
					foreach (WeightData w in vertex2.boneWeightList)
					{
						WeightData weightData = list.Find((WeightData wdata) => wdata.boneIndex == w.boneIndex);
						if (weightData == null)
						{
							weightData = new WeightData();
							weightData.boneIndex = w.boneIndex;
							list.Add(weightData);
						}
						weightData.boneWeight = Mathf.Clamp01(weightData.boneWeight + w.boneWeight * num2);
					}
				}
				list.Sort((WeightData a, WeightData b) => (!(a.boneWeight - b.boneWeight > 0f)) ? 1 : (-1));
				if (list.Count > 4)
				{
					list.RemoveRange(4, list.Count - 4);
				}
				AdjustWeight(list);
				boneWeightList = list;
			}

			public BoneWeight GetBoneWeight()
			{
				BoneWeight result = default(BoneWeight);
				for (int i = 0; i < boneWeightList.Count; i++)
				{
					WeightData weightData = boneWeightList[i];
					if (i == 0)
					{
						result.boneIndex0 = weightData.boneIndex;
						result.weight0 = weightData.boneWeight;
					}
					if (i == 1)
					{
						result.boneIndex1 = weightData.boneIndex;
						result.weight1 = weightData.boneWeight;
					}
					if (i == 2)
					{
						result.boneIndex2 = weightData.boneIndex;
						result.weight2 = weightData.boneWeight;
					}
					if (i == 3)
					{
						result.boneIndex3 = weightData.boneIndex;
						result.weight3 = weightData.boneWeight;
					}
				}
				return result;
			}
		}

		public class Triangle
		{
			public int tindex;

			public List<ShareVertex> shareVertexList = new List<ShareVertex>();

			public Vector3 wnor;

			public Vector3 wtan;

			public bool flipLock;

			public void GetEdge(out uint edge0, out uint edge1, out uint edge2)
			{
				edge0 = Utility.PackPair(shareVertexList[0].sindex, shareVertexList[1].sindex);
				edge1 = Utility.PackPair(shareVertexList[1].sindex, shareVertexList[2].sindex);
				edge2 = Utility.PackPair(shareVertexList[2].sindex, shareVertexList[0].sindex);
			}

			public Vector3 CalcTriangleNormal()
			{
				Vector3 lhs = shareVertexList[1].wpos - shareVertexList[0].wpos;
				Vector3 rhs = shareVertexList[2].wpos - shareVertexList[0].wpos;
				lhs *= 1000f;
				rhs *= 1000f;
				wnor = Vector3.Cross(lhs, rhs).normalized;
				if (wnor.magnitude <= 0.001f)
				{
					Debug.LogError(string.Format("CalcTriangleNormal Invalid! ({0},{1},{2})", shareVertexList[0].sindex, shareVertexList[1].sindex, shareVertexList[2].sindex));
				}
				return wnor;
			}

			public void Flip()
			{
				ShareVertex value = shareVertexList[1];
				shareVertexList[1] = shareVertexList[2];
				shareVertexList[2] = value;
				wnor = -wnor;
			}

			public Vector3 CalcTriangleTangent()
			{
				Vector3 wpos = shareVertexList[0].wpos;
				Vector3 wpos2 = shareVertexList[1].wpos;
				Vector3 wpos3 = shareVertexList[2].wpos;
				Vector2 uv = shareVertexList[0].uv;
				Vector2 uv2 = shareVertexList[1].uv;
				Vector2 uv3 = shareVertexList[2].uv;
				Vector3 vector = wpos2 - wpos;
				Vector3 vector2 = wpos3 - wpos;
				Vector2 vector3 = uv2 - uv;
				Vector2 vector4 = uv3 - uv;
				float num = vector3.x * vector4.y - vector3.y * vector4.x;
				Vector3 vector5 = Vector3.zero;
				if (num == 0f)
				{
					Debug.LogError("Calc tangent area = 0!");
				}
				else
				{
					float num2 = 1f / num;
					vector5 = new Vector3(vector.x * vector4.y + vector2.x * (0f - vector3.y), vector.y * vector4.y + vector2.y * (0f - vector3.y), vector.z * vector4.y + vector2.z * (0f - vector3.y)) * num2;
					vector5 = -vector5;
				}
				wtan = vector5;
				return wtan;
			}

			public ShareVertex GetNonEdgeVertex(int edgev0, int edgev1)
			{
				return shareVertexList.Find((ShareVertex sv) => sv.sindex != edgev0 && sv.sindex != edgev1);
			}

			public ulong GetTriangleHash()
			{
				return Utility.PackTriple(shareVertexList[0].sindex, shareVertexList[1].sindex, shareVertexList[2].sindex);
			}

			public static float GetTriangleArea(ShareVertex sv0, ShareVertex sv1, ShareVertex sv2)
			{
				return Vector3.Cross(sv1.wpos - sv0.wpos, sv2.wpos - sv0.wpos).magnitude;
			}

			public override string ToString()
			{
				return string.Format("<{0}>({1},{2},{3})", tindex, shareVertexList[0].sindex, shareVertexList[1].sindex, shareVertexList[2].sindex);
			}
		}

		private class Line
		{
			public List<ShareVertex> shareVertexList = new List<ShareVertex>();
		}

		public enum UvWrapMode
		{
			None = 0,
			Sphere = 1
		}

		public class Square
		{
			public ulong shash;

			public List<Triangle> triangleList = new List<Triangle>();

			public float angle;

			public override string ToString()
			{
				return string.Format("[{0}] {1} - {2} ang:{3}", shash, triangleList[0], triangleList[1], angle);
			}
		}

		public class Tetra
		{
			public List<ShareVertex> shareVertexList = new List<ShareVertex>();

			public Vector3 circumCenter;

			public float circumRadius;

			public Vector3 tetraCenter;

			public float tetraSize;

			public Tetra()
			{
			}

			public Tetra(ShareVertex a, ShareVertex b, ShareVertex c, ShareVertex d)
			{
				shareVertexList.Add(a);
				shareVertexList.Add(b);
				shareVertexList.Add(c);
				shareVertexList.Add(d);
				CalcSize();
			}

			public ulong GetTetraHash()
			{
				return Utility.PackQuater(shareVertexList[0].sindex, shareVertexList[1].sindex, shareVertexList[2].sindex, shareVertexList[3].sindex);
			}

			public void CalcCircumcircle()
			{
				Vector3 wpos = shareVertexList[0].wpos;
				Vector3 wpos2 = shareVertexList[1].wpos;
				Vector3 wpos3 = shareVertexList[2].wpos;
				Vector3 wpos4 = shareVertexList[3].wpos;
				float4x4 m = new float4x4(new float4(wpos.x, wpos.y, wpos.z, 1f), new float4(wpos2.x, wpos2.y, wpos2.z, 1f), new float4(wpos3.x, wpos3.y, wpos3.z, 1f), new float4(wpos4.x, wpos4.y, wpos4.z, 1f));
				float x = Mathf.Pow(wpos.x, 2f) + Mathf.Pow(wpos.y, 2f) + Mathf.Pow(wpos.z, 2f);
				float x2 = Mathf.Pow(wpos2.x, 2f) + Mathf.Pow(wpos2.y, 2f) + Mathf.Pow(wpos2.z, 2f);
				float x3 = Mathf.Pow(wpos3.x, 2f) + Mathf.Pow(wpos3.y, 2f) + Mathf.Pow(wpos3.z, 2f);
				float x4 = Mathf.Pow(wpos4.x, 2f) + Mathf.Pow(wpos4.y, 2f) + Mathf.Pow(wpos4.z, 2f);
				float4x4 m2 = new float4x4(new float4(x, wpos.y, wpos.z, 1f), new float4(x2, wpos2.y, wpos2.z, 1f), new float4(x3, wpos3.y, wpos3.z, 1f), new float4(x4, wpos4.y, wpos4.z, 1f));
				float4x4 m3 = new float4x4(new float4(x, wpos.x, wpos.z, 1f), new float4(x2, wpos2.x, wpos2.z, 1f), new float4(x3, wpos3.x, wpos3.z, 1f), new float4(x4, wpos4.x, wpos4.z, 1f));
				float4x4 m4 = new float4x4(new float4(x, wpos.x, wpos.y, 1f), new float4(x2, wpos2.x, wpos2.y, 1f), new float4(x3, wpos3.x, wpos3.y, 1f), new float4(x4, wpos4.x, wpos4.y, 1f));
				float4x4 m5 = new float4x4(new float4(x, wpos.x, wpos.y, wpos.z), new float4(x2, wpos2.x, wpos2.y, wpos2.z), new float4(x3, wpos3.x, wpos3.y, wpos3.z), new float4(x4, wpos4.x, wpos4.y, wpos4.z));
				float num = math.determinant(m);
				float num2 = math.determinant(m2);
				float num3 = 0f - math.determinant(m3);
				float num4 = math.determinant(m4);
				float num5 = math.determinant(m5);
				circumCenter = new Vector3(num2 / (2f * num), num3 / (2f * num), num4 / (2f * num));
				circumRadius = Mathf.Sqrt(num2 * num2 + num3 * num3 + num4 * num4 - 4f * num * num5) / (2f * Mathf.Abs(num));
			}

			public bool IntersectCircumcircle(Vector3 pos)
			{
				return Vector3.Distance(pos, circumCenter) <= circumRadius;
			}

			public bool CheckSame(Tetra tri)
			{
				if (circumCenter == tri.circumCenter)
				{
					return circumRadius == tri.circumRadius;
				}
				return false;
			}

			public bool ContainsPoint(ShareVertex p1)
			{
				return shareVertexList.Contains(p1);
			}

			public bool ContainsPoint(ShareVertex p1, ShareVertex p2, ShareVertex p3, ShareVertex p4)
			{
				if (!shareVertexList.Contains(p1) && !shareVertexList.Contains(p2) && !shareVertexList.Contains(p3))
				{
					return shareVertexList.Contains(p4);
				}
				return true;
			}

			public void CalcSize()
			{
				Vector3 wpos = shareVertexList[0].wpos;
				Vector3 wpos2 = shareVertexList[1].wpos;
				Vector3 wpos3 = shareVertexList[2].wpos;
				Vector3 wpos4 = shareVertexList[3].wpos;
				tetraCenter = (wpos + wpos2 + wpos3 + wpos4) / 4f;
				float a = Vector3.Distance(wpos, tetraCenter);
				float b = Vector3.Distance(wpos2, tetraCenter);
				float a2 = Vector3.Distance(wpos3, tetraCenter);
				float b2 = Vector3.Distance(wpos4, tetraCenter);
				tetraSize = Mathf.Max(Mathf.Max(a, b), Mathf.Max(a2, b2));
			}

			public bool Verification()
			{
				Vector3 wpos = shareVertexList[0].wpos;
				Vector3 wpos2 = shareVertexList[1].wpos;
				Vector3 wpos3 = shareVertexList[2].wpos;
				Vector3 wpos4 = shareVertexList[3].wpos;
				Vector3 lhs = Vector3.Cross(wpos - wpos2, wpos - wpos3);
				if (lhs.magnitude < 1E-05f)
				{
					return false;
				}
				lhs.Normalize();
				Vector3 rhs = wpos4 - wpos;
				if (Mathf.Abs(Vector3.Dot(lhs, rhs)) < tetraSize * 0.2f)
				{
					return false;
				}
				return true;
			}
		}

		private class LinkInfo
		{
			public ShareVertex sv;

			public float length;

			public int count;
		}

		private class VertexLengthInfo
		{
			public ShareVertex sv;

			public float length;
		}

		public List<Vertex> originalVertexList = new List<Vertex>();

		public List<MeshInfo> meshInfoList = new List<MeshInfo>();

		public List<ShareVertex> shareVertexList = new List<ShareVertex>();

		private Dictionary<ulong, Triangle> triangleDict = new Dictionary<ulong, Triangle>();

		private Dictionary<uint, Line> lineDict = new Dictionary<uint, Line>();

		public List<Transform> boneList = new List<Transform>();

		private List<Tetra> tetraList = new List<Tetra>();

		private float weightPow = 1.5f;

		private int maxWeightCount = 4;

		private float sameSurfaceAngle = 80f;

		private bool removeSameTrianglePair = true;

		private float removeSameTrianglePairAngle = 10f;

		public int VertexCount
		{
			get
			{
				return shareVertexList.Count;
			}
		}

		public int LineCount
		{
			get
			{
				return lineDict.Count;
			}
		}

		public int TriangleCount
		{
			get
			{
				return triangleDict.Count;
			}
		}

		public int TetraCount
		{
			get
			{
				return tetraList.Count;
			}
		}

		public int MeshCount
		{
			get
			{
				return meshInfoList.Count;
			}
		}

		public float WeightPow
		{
			get
			{
				return weightPow;
			}
			set
			{
				weightPow = value;
			}
		}

		public int MaxWeightCount
		{
			get
			{
				return maxWeightCount;
			}
			set
			{
				maxWeightCount = value;
			}
		}

		public float SameSurfaceAngle
		{
			get
			{
				return sameSurfaceAngle;
			}
			set
			{
				sameSurfaceAngle = value;
			}
		}

		public bool RemoveSameTrianglePair
		{
			get
			{
				return removeSameTrianglePair;
			}
			set
			{
				removeSameTrianglePair = value;
			}
		}

		public float RemoveSameTrianglePairAngle
		{
			get
			{
				return removeSameTrianglePairAngle;
			}
			set
			{
				removeSameTrianglePairAngle = value;
			}
		}

		public int AddMesh(bool isSkinning, Mesh mesh, List<Transform> bones, Matrix4x4[] bindPoseList, BoneWeight[] boneWeightList)
		{
			Debug.Assert(mesh);
			int num = meshInfoList.Count();
			MeshInfo meshInfo = new MeshInfo();
			meshInfo.index = num;
			meshInfo.mesh = mesh;
			meshInfo.vertexCount = mesh.vertexCount;
			meshInfoList.Add(meshInfo);
			List<Vector3> wposList;
			List<Vector3> wnorList;
			List<Vector4> wtanList;
			CalcMeshWorldPositionNormalTangent(isSkinning, mesh, bones, bindPoseList, boneWeightList, out wposList, out wnorList, out wtanList);
			bool flag = wnorList.Count > 0;
			bool flag2 = wtanList.Count > 0;
			List<int> list = new List<int>();
			if (bones != null)
			{
				foreach (Transform bone in bones)
				{
					int num2 = boneList.IndexOf(bone);
					if (num2 < 0)
					{
						boneList.Add(bone);
						num2 = boneList.Count - 1;
					}
					list.Add(num2);
				}
			}
			Vector2[] uv = mesh.uv;
			bool flag3 = uv != null && uv.Length == wposList.Count;
			int count = shareVertexList.Count;
			for (int i = 0; i < wposList.Count; i++)
			{
				Vertex vertex = new Vertex();
				vertex.meshIndex = num;
				vertex.vertexIndex = i;
				vertex.wpos = wposList[i];
				if (flag)
				{
					vertex.wnor = wnorList[i];
				}
				if (flag2)
				{
					vertex.wtan = wtanList[i];
					vertex.tanw = wtanList[i].w;
				}
				if (flag3)
				{
					vertex.uv = uv[i];
				}
				originalVertexList.Add(vertex);
				meshInfo.vertexList.Add(vertex);
				if (isSkinning)
				{
					BoneWeight boneWeight = boneWeightList[i];
					if (boneWeight.weight0 > 0f)
					{
						WeightData item = new WeightData
						{
							boneIndex = list[boneWeight.boneIndex0],
							boneWeight = boneWeight.weight0
						};
						vertex.boneWeightList.Add(item);
					}
					if (boneWeight.weight1 > 0f)
					{
						WeightData item2 = new WeightData
						{
							boneIndex = list[boneWeight.boneIndex1],
							boneWeight = boneWeight.weight1
						};
						vertex.boneWeightList.Add(item2);
					}
					if (boneWeight.weight2 > 0f)
					{
						WeightData item3 = new WeightData
						{
							boneIndex = list[boneWeight.boneIndex2],
							boneWeight = boneWeight.weight2
						};
						vertex.boneWeightList.Add(item3);
					}
					if (boneWeight.weight3 > 0f)
					{
						WeightData item4 = new WeightData
						{
							boneIndex = list[boneWeight.boneIndex3],
							boneWeight = boneWeight.weight3
						};
						vertex.boneWeightList.Add(item4);
					}
				}
				else
				{
					WeightData item5 = new WeightData
					{
						boneIndex = 0,
						boneWeight = 1f
					};
					vertex.boneWeightList.Add(item5);
				}
				ShareVertex shareVertex = new ShareVertex();
				shareVertex.wpos = vertex.wpos;
				shareVertex.wnor = vertex.wnor;
				shareVertex.wtan = vertex.wtan;
				shareVertex.tanw = -1f;
				shareVertex.uv = vertex.uv;
				shareVertex.sindex = count + i;
				shareVertex.vertexList.Add(vertex);
				vertex.parentIndex = shareVertex.sindex;
				shareVertex.CalcBoneWeight(parent.WeightMode, weightPow);
				shareVertexList.Add(shareVertex);
			}
			int[] triangles = mesh.triangles;
			int num3 = triangles.Length / 3;
			for (int j = 0; j < num3; j++)
			{
				int num4 = j * 3;
				int num5 = triangles[num4];
				int num6 = triangles[num4 + 1];
				int num7 = triangles[num4 + 2];
				ShareVertex shareVertex2 = shareVertexList[count + num5];
				ShareVertex shareVertex3 = shareVertexList[count + num6];
				ShareVertex shareVertex4 = shareVertexList[count + num7];
				ulong key = Utility.PackTriple(shareVertex2.sindex, shareVertex3.sindex, shareVertex4.sindex);
				if (!triangleDict.ContainsKey(key))
				{
					shareVertex2.AddLink(shareVertex3);
					shareVertex2.AddLink(shareVertex4);
					shareVertex3.AddLink(shareVertex2);
					shareVertex3.AddLink(shareVertex4);
					shareVertex4.AddLink(shareVertex2);
					shareVertex4.AddLink(shareVertex3);
					Triangle triangle = new Triangle();
					triangle.shareVertexList.Add(shareVertex2);
					triangle.shareVertexList.Add(shareVertex3);
					triangle.shareVertexList.Add(shareVertex4);
					triangleDict.Add(key, triangle);
				}
			}
			return num;
		}

		public int AddMesh(Transform root, List<Vector3> posList, List<Vector3> norList = null, List<Vector4> tanList = null, List<Vector2> uvList = null, List<int> triangleList = null)
		{
			Debug.Assert(root != null);
			Debug.Assert(posList != null);
			Debug.Assert(posList.Count > 0);
			int num = meshInfoList.Count();
			MeshInfo meshInfo = new MeshInfo();
			meshInfo.index = num;
			meshInfo.mesh = null;
			meshInfo.vertexCount = posList.Count;
			meshInfoList.Add(meshInfo);
			if (boneList.IndexOf(root) < 0)
			{
				boneList.Add(root);
				int count2 = boneList.Count;
			}
			int count = shareVertexList.Count;
			for (int i = 0; i < posList.Count; i++)
			{
				Vertex vertex = new Vertex();
				vertex.meshIndex = num;
				vertex.vertexIndex = i;
				vertex.wpos = posList[i];
				vertex.wnor = ((norList != null) ? norList[i] : Vector3.up);
				vertex.wtan = ((tanList != null) ? tanList[i] : new Vector4(1f, 0f, 0f, 1f));
				vertex.tanw = ((tanList != null) ? tanList[i].w : (-1f));
				vertex.uv = ((uvList != null) ? uvList[i] : Vector2.zero);
				originalVertexList.Add(vertex);
				meshInfo.vertexList.Add(vertex);
				WeightData item = new WeightData
				{
					boneIndex = 0,
					boneWeight = 1f
				};
				vertex.boneWeightList.Add(item);
				ShareVertex shareVertex = new ShareVertex();
				shareVertex.wpos = vertex.wpos;
				shareVertex.wnor = vertex.wnor;
				shareVertex.wtan = vertex.wtan;
				shareVertex.tanw = -1f;
				shareVertex.uv = vertex.uv;
				shareVertex.sindex = count + i;
				shareVertex.vertexList.Add(vertex);
				vertex.parentIndex = shareVertex.sindex;
				shareVertex.CalcBoneWeight(parent.WeightMode, weightPow);
				shareVertexList.Add(shareVertex);
			}
			if (triangleList != null)
			{
				int num2 = triangleList.Count / 3;
				for (int j = 0; j < num2; j++)
				{
					int num3 = j * 3;
					int num4 = triangleList[num3];
					int num5 = triangleList[num3 + 1];
					int num6 = triangleList[num3 + 2];
					ShareVertex shareVertex2 = shareVertexList[count + num4];
					ShareVertex shareVertex3 = shareVertexList[count + num5];
					ShareVertex shareVertex4 = shareVertexList[count + num6];
					ulong key = Utility.PackTriple(shareVertex2.sindex, shareVertex3.sindex, shareVertex4.sindex);
					if (!triangleDict.ContainsKey(key))
					{
						shareVertex2.AddLink(shareVertex3);
						shareVertex2.AddLink(shareVertex4);
						shareVertex3.AddLink(shareVertex2);
						shareVertex3.AddLink(shareVertex4);
						shareVertex4.AddLink(shareVertex2);
						shareVertex4.AddLink(shareVertex3);
						Triangle triangle = new Triangle();
						triangle.shareVertexList.Add(shareVertex2);
						triangle.shareVertexList.Add(shareVertex3);
						triangle.shareVertexList.Add(shareVertex4);
						triangleDict.Add(key, triangle);
					}
				}
			}
			return num;
		}

		public void CombineVertex(ShareVertex sv0, ShareVertex sv1)
		{
			sv0.vertexList.AddRange(sv1.vertexList);
			sv0.linkShareVertexSet.Remove(sv1);
			foreach (ShareVertex item in sv1.linkShareVertexSet)
			{
				if (item != sv0)
				{
					sv0.linkShareVertexSet.Add(item);
				}
			}
			foreach (ShareVertex item2 in sv0.linkShareVertexSet)
			{
				item2.ReplaseLink(sv1, sv0);
			}
			shareVertexList.Remove(sv1);
			sv0.RecalcCoordinate();
		}

		public void UpdateMeshData(bool createTetra)
		{
			CalcVertexIndex();
			CalcUV(UvWrapMode.Sphere);
			CreateTriangleAndLine();
			CalcShareVertexWeight();
			AdjustTriangleNormal();
			CalcVertexNormalFromTriangle();
			if (createTetra)
			{
				CreateTetraMesh();
			}
		}

		private void CalcVertexIndex()
		{
			for (int i = 0; i < shareVertexList.Count; i++)
			{
				ShareVertex shareVertex = shareVertexList[i];
				shareVertex.sindex = i;
				foreach (Vertex vertex in shareVertex.vertexList)
				{
					vertex.parentIndex = i;
				}
			}
		}

		private void CalcUV(UvWrapMode wrapMode)
		{
			if (wrapMode != UvWrapMode.Sphere)
			{
				return;
			}
			Vector3 zero = Vector3.zero;
			foreach (ShareVertex shareVertex in shareVertexList)
			{
				zero += shareVertex.wpos;
			}
			zero /= (float)VertexCount;
			float num = 0f;
			foreach (ShareVertex shareVertex2 in shareVertexList)
			{
				Vector3 rhs = shareVertex2.wpos - zero;
				float magnitude = rhs.magnitude;
				rhs.Normalize();
				float value = Mathf.Atan2(rhs.x, rhs.z);
				value = Mathf.Clamp01(Mathf.InverseLerp(-(float)Math.PI, (float)Math.PI, value));
				float value2 = Vector3.Dot(Vector3.up, rhs);
				value2 = Mathf.Clamp01(Mathf.InverseLerp(1f, -1f, value2));
				Vector2 uv = new Vector2(value + magnitude * 0.01f + num, value2 + magnitude * 0.01f + num);
				num += 0.001234f;
				shareVertex2.uv = uv;
			}
		}

		private void CreateTriangleAndLine()
		{
			triangleDict.Clear();
			lineDict.Clear();
			HashSet<uint> hashSet = new HashSet<uint>();
			foreach (ShareVertex shareVertex3 in shareVertexList)
			{
				foreach (ShareVertex item7 in shareVertex3.linkShareVertexSet)
				{
					uint item = Utility.PackPair(shareVertex3.sindex, item7.sindex);
					hashSet.Add(item);
				}
			}
			foreach (ShareVertex shareVertex4 in shareVertexList)
			{
				ShareVertex[] array = shareVertex4.linkShareVertexSet.ToArray();
				for (int i = 0; i < array.Length - 1; i++)
				{
					ShareVertex shareVertex = array[i];
					for (int j = i + 1; j < array.Length; j++)
					{
						ShareVertex shareVertex2 = array[j];
						if (shareVertex.linkShareVertexSet.Contains(shareVertex2) && shareVertex2.linkShareVertexSet.Contains(shareVertex) && !(Triangle.GetTriangleArea(shareVertex4, shareVertex, shareVertex2) < 1E-06f))
						{
							ulong key = Utility.PackTriple(shareVertex4.sindex, shareVertex.sindex, shareVertex2.sindex);
							if (!triangleDict.ContainsKey(key))
							{
								Triangle triangle = new Triangle();
								triangle.shareVertexList.Add(shareVertex4);
								triangle.shareVertexList.Add(shareVertex);
								triangle.shareVertexList.Add(shareVertex2);
								triangleDict.Add(key, triangle);
								uint item2 = Utility.PackPair(shareVertex4.sindex, shareVertex.sindex);
								uint item3 = Utility.PackPair(shareVertex.sindex, shareVertex2.sindex);
								uint item4 = Utility.PackPair(shareVertex2.sindex, shareVertex4.sindex);
								hashSet.Remove(item2);
								hashSet.Remove(item3);
								hashSet.Remove(item4);
							}
						}
					}
				}
			}
			if (RemoveSameTrianglePair)
			{
				foreach (KeyValuePair<ulong, List<Square>> item8 in GetSquareDict())
				{
					List<Square> value = item8.Value;
					HashSet<Square> hashSet2 = new HashSet<Square>();
					for (int k = 0; k < value.Count - 1; k++)
					{
						Square square = value[k];
						if (hashSet2.Contains(square))
						{
							continue;
						}
						for (int l = k + 1; l < value.Count; l++)
						{
							Square square2 = value[l];
							if (!hashSet2.Contains(square2) && math.abs(square.angle - square2.angle) <= RemoveSameTrianglePairAngle)
							{
								hashSet2.Add(square2);
							}
						}
					}
					foreach (Square item9 in hashSet2)
					{
						value.Remove(item9);
						foreach (Triangle triangle2 in item9.triangleList)
						{
							RemoveTriangle(triangle2.GetTriangleHash());
						}
					}
				}
			}
			foreach (uint item10 in hashSet)
			{
				if (!lineDict.ContainsKey(item10))
				{
					int v;
					int v2;
					Utility.UnpackPair(item10, out v, out v2);
					ShareVertex item5 = shareVertexList[v];
					ShareVertex item6 = shareVertexList[v2];
					Line line = new Line();
					line.shareVertexList.Add(item5);
					line.shareVertexList.Add(item6);
					lineDict.Add(item10, line);
				}
			}
			int num = 0;
			foreach (Triangle value2 in triangleDict.Values)
			{
				value2.tindex = num;
				num++;
			}
			foreach (ShareVertex shareVertex5 in shareVertexList)
			{
				shareVertex5.linkTriangleSet.Clear();
			}
			foreach (Triangle value3 in triangleDict.Values)
			{
				foreach (ShareVertex shareVertex6 in value3.shareVertexList)
				{
					shareVertex6.linkTriangleSet.Add(value3);
				}
			}
		}

		private void CalcShareVertexWeight()
		{
			foreach (ShareVertex shareVertex in shareVertexList)
			{
				shareVertex.CalcBoneWeight(parent.WeightMode, weightPow);
			}
		}

		private void AdjustTriangleNormal()
		{
			Dictionary<uint, List<ulong>> edgeToTriangleDict = GetEdgeToTriangleDict();
			List<ulong> list = new List<ulong>();
			foreach (ulong key in triangleDict.Keys)
			{
				list.Add(key);
			}
			HashSet<ulong> hashSet = new HashSet<ulong>();
			List<List<ulong>> list2 = new List<List<ulong>>();
			while (list.Count > 0)
			{
				List<ulong> list3 = new List<ulong>();
				int num = 0;
				int num2 = 0;
				Queue<ulong> queue = new Queue<ulong>();
				queue.Enqueue(list[0]);
				while (queue.Count > 0)
				{
					ulong num3 = queue.Dequeue();
					if (hashSet.Contains(num3))
					{
						continue;
					}
					list3.Add(num3);
					list.Remove(num3);
					hashSet.Add(num3);
					Triangle triangle = triangleDict[num3];
					Vector3 to = triangle.CalcTriangleNormal();
					triangle.CalcTriangleTangent();
					uint edge;
					uint edge2;
					uint edge3;
					triangle.GetEdge(out edge, out edge2, out edge3);
					uint[] array = new uint[3] { edge, edge2, edge3 };
					foreach (uint num4 in array)
					{
						List<ulong> list4 = edgeToTriangleDict[num4];
						if (list4.Count == 0)
						{
							continue;
						}
						foreach (ulong item in list4)
						{
							if (item == num3 || hashSet.Contains(item))
							{
								continue;
							}
							Triangle triangle2 = triangleDict[item];
							if (CalcTwoTriangleAngle(triangle, triangle2, num4) > SameSurfaceAngle)
							{
								continue;
							}
							float num5 = Vector3.Angle(triangle2.CalcTriangleNormal(), to);
							if (num5 >= 90f && !triangle2.flipLock)
							{
								triangle2.Flip();
								if (num5 == 180f)
								{
									triangle2.flipLock = true;
								}
							}
							if (CheckTwoTriangleOpen(triangle, triangle2, num4))
							{
								num++;
							}
							else
							{
								num2++;
							}
							queue.Enqueue(item);
						}
					}
				}
				if (num2 > num)
				{
					foreach (ulong item2 in list3)
					{
						Triangle triangle3 = triangleDict[item2];
						triangle3.Flip();
						triangle3.CalcTriangleTangent();
					}
				}
				list2.Add(list3);
			}
		}

		private bool CheckTwoTriangleOpen(Triangle tri1, Triangle tri2, uint edge)
		{
			int v;
			int v2;
			Utility.UnpackPair(edge, out v, out v2);
			Vector3 rhs = Vector3.Normalize(tri2.GetNonEdgeVertex(v, v2).wpos - shareVertexList[v].wpos);
			return Vector3.Dot(tri1.wnor, rhs) <= 0f;
		}

		private float CalcTwoTriangleAngle(Triangle tri1, Triangle tri2, uint edge)
		{
			int v;
			int v2;
			Utility.UnpackPair(edge, out v, out v2);
			ShareVertex shareVertex = shareVertexList[v];
			ShareVertex nonEdgeVertex = tri1.GetNonEdgeVertex(v, v2);
			ShareVertex nonEdgeVertex2 = tri2.GetNonEdgeVertex(v, v2);
			Vector3 vector = shareVertexList[v2].wpos - shareVertexList[v].wpos;
			Vector3 rhs = nonEdgeVertex.wpos - shareVertexList[v].wpos;
			Vector3 lhs = nonEdgeVertex2.wpos - shareVertexList[v].wpos;
			Vector3 from = Vector3.Cross(vector, rhs);
			Vector3 to = Vector3.Cross(lhs, vector);
			return Vector3.Angle(from, to);
		}

		private Dictionary<uint, List<ulong>> GetEdgeToTriangleDict()
		{
			Dictionary<uint, List<ulong>> dictionary = new Dictionary<uint, List<ulong>>();
			List<uint> list = new List<uint>();
			foreach (KeyValuePair<ulong, Triangle> item in triangleDict)
			{
				ulong key = item.Key;
				Triangle value = item.Value;
				int sindex = value.shareVertexList[0].sindex;
				int sindex2 = value.shareVertexList[1].sindex;
				int sindex3 = value.shareVertexList[2].sindex;
				list.Clear();
				list.Add(Utility.PackPair(sindex, sindex2));
				list.Add(Utility.PackPair(sindex2, sindex3));
				list.Add(Utility.PackPair(sindex3, sindex));
				foreach (uint item2 in list)
				{
					if (!dictionary.ContainsKey(item2))
					{
						dictionary.Add(item2, new List<ulong>());
					}
					dictionary[item2].Add(key);
				}
			}
			return dictionary;
		}

		private void CalcVertexNormalFromTriangle()
		{
			foreach (ShareVertex shareVertex in shareVertexList)
			{
				shareVertex.CalcNormalTangentFromTriangle();
			}
		}

		private void RemoveTriangle(ulong thash)
		{
			if (!triangleDict.ContainsKey(thash))
			{
				return;
			}
			Triangle triangle = triangleDict[thash];
			foreach (ShareVertex shareVertex in triangle.shareVertexList)
			{
				shareVertex.linkTriangleSet.Remove(triangle);
			}
			triangleDict.Remove(thash);
		}

		private Dictionary<ulong, List<Square>> GetSquareDict()
		{
			Dictionary<ulong, List<Square>> dictionary = new Dictionary<ulong, List<Square>>();
			foreach (KeyValuePair<uint, List<ulong>> item in GetEdgeToTriangleDict())
			{
				int v;
				int v2;
				Utility.UnpackPair(item.Key, out v, out v2);
				List<ulong> value = item.Value;
				for (int i = 0; i < value.Count - 1; i++)
				{
					for (int j = i + 1; j < value.Count; j++)
					{
						Triangle triangle = triangleDict[value[i]];
						Triangle triangle2 = triangleDict[value[j]];
						int sindex = triangle.GetNonEdgeVertex(v, v2).sindex;
						int sindex2 = triangle2.GetNonEdgeVertex(v, v2).sindex;
						ulong num = Utility.PackQuater(sindex, sindex2, v, v2);
						Vector3 lhs = shareVertexList[sindex].wpos - shareVertexList[v].wpos;
						Vector3 lhs2 = shareVertexList[sindex2].wpos - shareVertexList[v].wpos;
						Vector3 rhs = shareVertexList[v2].wpos - shareVertexList[v].wpos;
						Vector3 from = Vector3.Cross(lhs, rhs);
						Vector3 to = Vector3.Cross(lhs2, rhs);
						float num2 = Vector3.Angle(from, to);
						if (!(num2 <= 135f))
						{
							Square square = new Square();
							square.shash = num;
							square.angle = num2;
							square.triangleList.Add(triangle);
							square.triangleList.Add(triangle2);
							if (!dictionary.ContainsKey(num))
							{
								dictionary.Add(num, new List<Square>());
							}
							dictionary[num].Add(square);
						}
					}
				}
			}
			return dictionary;
		}

		private void RemoveOverlappingSquareTriangles()
		{
			foreach (KeyValuePair<ulong, List<Square>> item in GetSquareDict())
			{
				KeyValuePair<ulong, List<Square>> keyValuePair = item;
			}
		}

		private void CreateTetraMesh()
		{
			tetraList.Clear();
			if (VertexCount < 4)
			{
				return;
			}
			Bounds bounds = CalcBounding();
			float num = Mathf.Max(Mathf.Max(bounds.extents.x, bounds.extents.y), bounds.extents.z) * 100f;
			ShareVertex shareVertex = new ShareVertex();
			ShareVertex shareVertex2 = new ShareVertex();
			ShareVertex shareVertex3 = new ShareVertex();
			ShareVertex shareVertex4 = new ShareVertex();
			shareVertex.wpos = bounds.center + new Vector3(0f, 0f - num, 0f);
			shareVertex2.wpos = bounds.center + new Vector3(0f - num, num, num);
			shareVertex3.wpos = bounds.center + new Vector3(num, num, num);
			shareVertex4.wpos = bounds.center + new Vector3(0f, num, 0f - num);
			int count = shareVertexList.Count;
			shareVertex.sindex = count++;
			shareVertex2.sindex = count++;
			shareVertex3.sindex = count++;
			shareVertex4.sindex = count++;
			shareVertexList.Add(shareVertex);
			shareVertexList.Add(shareVertex2);
			shareVertexList.Add(shareVertex3);
			shareVertexList.Add(shareVertex4);
			List<Tetra> list = new List<Tetra>();
			Tetra tetra = new Tetra(shareVertex, shareVertex2, shareVertex3, shareVertex4);
			tetra.CalcCircumcircle();
			list.Add(tetra);
			Dictionary<ulong, Tetra> dictionary = new Dictionary<ulong, Tetra>();
			dictionary.Add(tetra.GetTetraHash(), tetra);
			for (int i = 0; i < shareVertexList.Count - 4; i++)
			{
				ShareVertex shareVertex5 = shareVertexList[i];
				List<Tetra> list2 = new List<Tetra>();
				int num2 = 0;
				while (num2 < list.Count)
				{
					Tetra tetra2 = list[num2];
					if (!tetra2.ContainsPoint(shareVertex5) && tetra2.IntersectCircumcircle(shareVertex5.wpos))
					{
						Tetra item = new Tetra(tetra2.shareVertexList[0], tetra2.shareVertexList[1], tetra2.shareVertexList[2], shareVertex5);
						Tetra item2 = new Tetra(tetra2.shareVertexList[0], tetra2.shareVertexList[2], tetra2.shareVertexList[3], shareVertex5);
						Tetra item3 = new Tetra(tetra2.shareVertexList[0], tetra2.shareVertexList[3], tetra2.shareVertexList[1], shareVertex5);
						Tetra item4 = new Tetra(tetra2.shareVertexList[1], tetra2.shareVertexList[2], tetra2.shareVertexList[3], shareVertex5);
						list2.Add(item);
						list2.Add(item2);
						list2.Add(item3);
						list2.Add(item4);
						dictionary.Remove(tetra2.GetTetraHash());
						list.RemoveAt(num2);
					}
					else
					{
						num2++;
					}
				}
				foreach (Tetra item6 in list2)
				{
					ulong tetraHash = item6.GetTetraHash();
					if (!dictionary.ContainsKey(tetraHash))
					{
						item6.CalcCircumcircle();
						dictionary.Add(tetraHash, item6);
						list.Add(item6);
					}
					else
					{
						Tetra item5 = dictionary[tetraHash];
						dictionary.Remove(tetraHash);
						list.Remove(item5);
					}
				}
			}
			int num3 = 0;
			while (num3 < list.Count)
			{
				Tetra tetra3 = list[num3];
				if (tetra3.ContainsPoint(shareVertex, shareVertex2, shareVertex3, shareVertex4))
				{
					dictionary.Remove(tetra3.GetTetraHash());
					list.RemoveAt(num3);
				}
				else
				{
					num3++;
				}
			}
			shareVertexList.Remove(shareVertex);
			shareVertexList.Remove(shareVertex2);
			shareVertexList.Remove(shareVertex3);
			shareVertexList.Remove(shareVertex4);
			int num4 = 0;
			while (num4 < list.Count)
			{
				if (!list[num4].Verification())
				{
					list.RemoveAt(num4);
				}
				else
				{
					num4++;
				}
			}
			tetraList = list;
		}

		private Bounds CalcBounding()
		{
			Bounds result = new Bounds(shareVertexList[0].wpos, Vector3.one * 0.01f);
			foreach (ShareVertex shareVertex in shareVertexList)
			{
				result.Encapsulate(shareVertex.wpos);
			}
			return result;
		}

		public FinalData GetFinalData(Transform root)
		{
			Debug.Assert(root);
			FinalData finalData = new FinalData();
			for (int i = 0; i < shareVertexList.Count; i++)
			{
				ShareVertex shareVertex = shareVertexList[i];
				Vector3 item = root.InverseTransformPoint(shareVertex.wpos);
				Vector3 normalized = root.InverseTransformDirection(shareVertex.wnor).normalized;
				Vector4 item2 = root.InverseTransformDirection(shareVertex.wtan).normalized;
				item2.w = shareVertex.tanw;
				finalData.vertices.Add(item);
				finalData.normals.Add(normalized);
				finalData.tangents.Add(item2);
				finalData.uvs.Add(shareVertex.uv);
				finalData.vertexToTriangleCountList.Add(0);
				finalData.vertexToTriangleStartList.Add(0);
				shareVertex.CalcWorldToLocalMatrix();
			}
			foreach (ShareVertex shareVertex3 in shareVertexList)
			{
				finalData.boneWeights.Add(shareVertex3.GetBoneWeight());
			}
			finalData.bones = new List<Transform>(boneList);
			Matrix4x4 localToWorldMatrix = root.localToWorldMatrix;
			foreach (Transform bone in finalData.bones)
			{
				if ((bool)bone)
				{
					Matrix4x4 item3 = bone.worldToLocalMatrix * localToWorldMatrix;
					finalData.bindPoses.Add(item3);
				}
				else
				{
					finalData.bindPoses.Add(Matrix4x4.identity);
				}
			}
			foreach (ShareVertex shareVertex4 in shareVertexList)
			{
				Matrix4x4 item4 = (shareVertex4.bindpose = shareVertex4.worldToLocalMatrix * localToWorldMatrix);
				finalData.vertexBindPoses.Add(item4);
			}
			foreach (Triangle value in triangleDict.Values)
			{
				for (int j = 0; j < 3; j++)
				{
					int sindex = value.shareVertexList[j].sindex;
					finalData.triangles.Add(sindex);
				}
			}
			for (int k = 0; k < VertexCount; k++)
			{
				ShareVertex shareVertex2 = shareVertexList[k];
				if (shareVertex2.linkTriangleSet.Count == 0)
				{
					continue;
				}
				finalData.vertexToTriangleCountList[k] = shareVertex2.linkTriangleSet.Count;
				finalData.vertexToTriangleStartList[k] = finalData.vertexToTriangleIndexList.Count;
				foreach (Triangle item5 in shareVertex2.linkTriangleSet)
				{
					finalData.vertexToTriangleIndexList.Add(item5.tindex);
				}
			}
			foreach (Line value2 in lineDict.Values)
			{
				for (int l = 0; l < 2; l++)
				{
					finalData.lines.Add(value2.shareVertexList[l].sindex);
				}
			}
			foreach (Tetra tetra in tetraList)
			{
				for (int m = 0; m < 4; m++)
				{
					int sindex2 = tetra.shareVertexList[m].sindex;
					finalData.tetras.Add(sindex2);
				}
				finalData.tetraSizes.Add(tetra.tetraSize);
			}
			float num = 0f;
			int num2 = 0;
			foreach (Triangle value3 in triangleDict.Values)
			{
				num += Vector3.Distance(value3.shareVertexList[0].wpos, value3.shareVertexList[1].wpos);
				num += Vector3.Distance(value3.shareVertexList[1].wpos, value3.shareVertexList[2].wpos);
				num += Vector3.Distance(value3.shareVertexList[2].wpos, value3.shareVertexList[0].wpos);
				num2 += 3;
			}
			foreach (Line value4 in lineDict.Values)
			{
				num += Vector3.Distance(value4.shareVertexList[0].wpos, value4.shareVertexList[1].wpos);
				num2++;
			}
			num /= (float)num2;
			for (int n = 0; n < VertexCount; n++)
			{
				finalData.vertexToMeshIndexList.Add(new FinalData.MeshIndexData());
			}
			CreateOriginalMeshInfo(finalData, root, num * 1.5f);
			return finalData;
		}

		private void CreateOriginalMeshInfo(FinalData final, Transform root, float weightLength)
		{
			foreach (MeshInfo meshInfo2 in meshInfoList)
			{
				FinalData.MeshInfo meshInfo = new FinalData.MeshInfo();
				meshInfo.mesh = meshInfo2.mesh;
				meshInfo.meshIndex = meshInfo2.index;
				foreach (Vertex vertex in meshInfo2.vertexList)
				{
					Vector3 item = root.InverseTransformPoint(vertex.wpos);
					Vector3 normalized = root.InverseTransformDirection(vertex.wnor).normalized;
					Vector4 item2 = root.InverseTransformDirection(vertex.wtan).normalized;
					item2.w = vertex.tanw;
					meshInfo.vertices.Add(item);
					meshInfo.normals.Add(normalized);
					meshInfo.tangents.Add(item2);
					meshInfo.parents.Add(vertex.parentIndex);
					meshInfo.boneWeights.Add(default(BoneWeight));
				}
				final.meshList.Add(meshInfo);
			}
			foreach (Vertex vt in originalVertexList)
			{
				ShareVertex sv2 = shareVertexList[vt.parentIndex];
				List<ShareVertex> list = SearchNearPointList(vt.wpos, sv2, weightLength * 2f, 100);
				Debug.Assert(list.Count > 0);
				List<ShareVertex> list2 = list.FindAll((ShareVertex sv) => Vector3.Distance(vt.wpos, sv.wpos) <= weightLength);
				Debug.Assert(list2.Count > 0);
				if (list2.Count > maxWeightCount)
				{
					list2.RemoveRange(maxWeightCount, list2.Count - maxWeightCount);
				}
				float num = weightLength;
				List<float> list3 = new List<float>();
				foreach (ShareVertex item4 in list2)
				{
					float item3 = 1f;
					if (num > 0f)
					{
						float num2 = Vector3.Distance(vt.wpos, item4.wpos);
						item3 = Mathf.Clamp01(1f - num2 / num + 0.001f);
						item3 = Mathf.Pow(item3, weightPow);
					}
					list3.Add(item3);
				}
				float num3 = 0f;
				foreach (float item5 in list3)
				{
					num3 += item5;
				}
				float num4 = 1f / num3;
				for (int i = 0; i < list3.Count; i++)
				{
					list3[i] *= num4;
				}
				BoneWeight value = default(BoneWeight);
				for (int j = 0; j < list2.Count; j++)
				{
					ShareVertex shareVertex = list2[j];
					switch (j)
					{
					case 0:
						value.boneIndex0 = shareVertex.sindex;
						value.weight0 = list3[j];
						break;
					case 1:
						value.boneIndex1 = shareVertex.sindex;
						value.weight1 = list3[j];
						break;
					case 2:
						value.boneIndex2 = shareVertex.sindex;
						value.weight2 = list3[j];
						break;
					case 3:
						value.boneIndex3 = shareVertex.sindex;
						value.weight3 = list3[j];
						break;
					}
					if (j < 4 && list3[j] > 0f)
					{
						AddVertexToMeshIndexData(final, shareVertex.sindex, vt.meshIndex, vt.vertexIndex);
					}
				}
				final.meshList[vt.meshIndex].boneWeights[vt.vertexIndex] = value;
			}
		}

		private void AddVertexToMeshIndexData(FinalData final, int sindex, int meshIndex, int meshVertexIndex)
		{
			final.vertexToMeshIndexList[sindex].meshIndexPackList.Add(Utility.Pack16(meshIndex, meshVertexIndex));
		}

		private List<ShareVertex> SearchNearPointList(Vector3 basePos, ShareVertex sv, float weightLength, int maxCount)
		{
			LinkInfo linkInfo = new LinkInfo();
			linkInfo.sv = sv;
			linkInfo.length = 0f;
			linkInfo.count = 0;
			Stack<LinkInfo> stack = new Stack<LinkInfo>();
			stack.Push(linkInfo);
			HashSet<ShareVertex> hashSet = new HashSet<ShareVertex>();
			List<VertexLengthInfo> list = new List<VertexLengthInfo>();
			while (stack.Count > 0)
			{
				linkInfo = stack.Pop();
				if (hashSet.Contains(linkInfo.sv))
				{
					continue;
				}
				VertexLengthInfo vertexLengthInfo = new VertexLengthInfo();
				vertexLengthInfo.sv = linkInfo.sv;
				vertexLengthInfo.length = Vector3.Distance(basePos, linkInfo.sv.wpos);
				list.Add(vertexLengthInfo);
				hashSet.Add(linkInfo.sv);
				if (linkInfo.count >= 2)
				{
					continue;
				}
				foreach (ShareVertex item in linkInfo.sv.linkShareVertexSet)
				{
					if (!hashSet.Contains(item))
					{
						float num = Vector3.Distance(basePos, item.wpos);
						if (!(num > weightLength))
						{
							LinkInfo linkInfo2 = new LinkInfo();
							linkInfo2.sv = item;
							linkInfo2.length = num;
							linkInfo2.count = linkInfo.count + 1;
							stack.Push(linkInfo2);
						}
					}
				}
			}
			list.Sort((VertexLengthInfo a, VertexLengthInfo b) => (!(a.length < b.length)) ? 1 : (-1));
			List<ShareVertex> list2 = new List<ShareVertex>();
			for (int i = 0; i < list.Count && i < maxCount; i++)
			{
				list2.Add(list[i].sv);
			}
			return list2;
		}

		public void CalcMeshWorldPositionNormalTangent(bool isSkinning, Mesh mesh, List<Transform> bones, Matrix4x4[] bindPoseList, BoneWeight[] boneWeightList, out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector4> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector4>();
			if (mesh == null)
			{
				return;
			}
			int vertexCount = mesh.vertexCount;
			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;
			Vector4[] tangents = mesh.tangents;
			bool flag = normals != null && normals.Length != 0;
			bool flag2 = tangents != null && tangents.Length != 0;
			if (!isSkinning)
			{
				Transform transform = bones[0];
				for (int i = 0; i < vertexCount; i++)
				{
					Vector3 item = transform.TransformPoint(vertices[i]);
					wposList.Add(item);
					if (flag)
					{
						Vector3 item2 = transform.TransformDirection(normals[i]);
						item2.Normalize();
						wnorList.Add(item2);
					}
					if (flag2)
					{
						Vector3 vector = transform.TransformDirection(tangents[i]);
						vector.Normalize();
						wtanList.Add(new Vector4(vector.x, vector.y, vector.z, tangents[i].w));
					}
				}
				return;
			}
			float[] array = new float[4];
			int[] array2 = new int[4];
			for (int j = 0; j < vertexCount; j++)
			{
				Vector3 zero = Vector3.zero;
				Vector3 zero2 = Vector3.zero;
				Vector3 zero3 = Vector3.zero;
				array[0] = boneWeightList[j].weight0;
				array[1] = boneWeightList[j].weight1;
				array[2] = boneWeightList[j].weight2;
				array[3] = boneWeightList[j].weight3;
				array2[0] = boneWeightList[j].boneIndex0;
				array2[1] = boneWeightList[j].boneIndex1;
				array2[2] = boneWeightList[j].boneIndex2;
				array2[3] = boneWeightList[j].boneIndex3;
				for (int k = 0; k < 4; k++)
				{
					float num = array[k];
					if (num > 0f)
					{
						int num2 = array2[k];
						Transform transform2 = bones[num2];
						Vector3 position = bindPoseList[num2].MultiplyPoint3x4(vertices[j]);
						position = transform2.TransformPoint(position);
						position *= num;
						zero += position;
						if (flag)
						{
							position = bindPoseList[num2].MultiplyVector(normals[j]);
							zero2 += transform2.TransformVector(position).normalized * num;
						}
						if (flag2)
						{
							position = bindPoseList[num2].MultiplyVector(tangents[j]);
							zero3 += transform2.TransformVector(position).normalized * num;
						}
					}
				}
				wposList.Add(zero);
				if (flag)
				{
					wnorList.Add(zero2);
				}
				if (flag2)
				{
					wtanList.Add(new Vector4(zero3.x, zero3.y, zero3.z, tangents[j].w));
				}
			}
		}
	}
}
