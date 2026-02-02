#define RELEASE
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public static class MeshUtility
	{
		private class TetraVertex
		{
			public int index;

			public Vector3 pos;

			public TetraVertex()
			{
			}

			public TetraVertex(Vector3 pos, int index)
			{
				this.pos = pos;
				this.index = index;
			}
		}

		private class Tetra
		{
			public List<TetraVertex> vertexList = new List<TetraVertex>();

			public Vector3 circumCenter;

			public float circumRadius;

			public Vector3 tetraCenter;

			public float tetraSize;

			public Tetra()
			{
			}

			public Tetra(TetraVertex a, TetraVertex b, TetraVertex c, TetraVertex d)
			{
				vertexList.Add(a);
				vertexList.Add(b);
				vertexList.Add(c);
				vertexList.Add(d);
				CalcSize();
			}

			public ulong GetTetraHash()
			{
				return DataUtility.PackQuater(vertexList[0].index, vertexList[1].index, vertexList[2].index, vertexList[3].index);
			}

			public void CalcCircumcircle()
			{
				Vector3 pos = vertexList[0].pos;
				Vector3 pos2 = vertexList[1].pos;
				Vector3 pos3 = vertexList[2].pos;
				Vector3 pos4 = vertexList[3].pos;
				float4x4 m = new float4x4(new float4(pos.x, pos.y, pos.z, 1f), new float4(pos2.x, pos2.y, pos2.z, 1f), new float4(pos3.x, pos3.y, pos3.z, 1f), new float4(pos4.x, pos4.y, pos4.z, 1f));
				float x = Mathf.Pow(pos.x, 2f) + Mathf.Pow(pos.y, 2f) + Mathf.Pow(pos.z, 2f);
				float x2 = Mathf.Pow(pos2.x, 2f) + Mathf.Pow(pos2.y, 2f) + Mathf.Pow(pos2.z, 2f);
				float x3 = Mathf.Pow(pos3.x, 2f) + Mathf.Pow(pos3.y, 2f) + Mathf.Pow(pos3.z, 2f);
				float x4 = Mathf.Pow(pos4.x, 2f) + Mathf.Pow(pos4.y, 2f) + Mathf.Pow(pos4.z, 2f);
				float4x4 m2 = new float4x4(new float4(x, pos.y, pos.z, 1f), new float4(x2, pos2.y, pos2.z, 1f), new float4(x3, pos3.y, pos3.z, 1f), new float4(x4, pos4.y, pos4.z, 1f));
				float4x4 m3 = new float4x4(new float4(x, pos.x, pos.z, 1f), new float4(x2, pos2.x, pos2.z, 1f), new float4(x3, pos3.x, pos3.z, 1f), new float4(x4, pos4.x, pos4.z, 1f));
				float4x4 m4 = new float4x4(new float4(x, pos.x, pos.y, 1f), new float4(x2, pos2.x, pos2.y, 1f), new float4(x3, pos3.x, pos3.y, 1f), new float4(x4, pos4.x, pos4.y, 1f));
				float4x4 m5 = new float4x4(new float4(x, pos.x, pos.y, pos.z), new float4(x2, pos2.x, pos2.y, pos2.z), new float4(x3, pos3.x, pos3.y, pos3.z), new float4(x4, pos4.x, pos4.y, pos4.z));
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

			public bool ContainsPoint(TetraVertex p1)
			{
				return vertexList.Contains(p1);
			}

			public bool ContainsPoint(TetraVertex p1, TetraVertex p2, TetraVertex p3, TetraVertex p4)
			{
				if (!vertexList.Contains(p1) && !vertexList.Contains(p2) && !vertexList.Contains(p3))
				{
					return vertexList.Contains(p4);
				}
				return true;
			}

			public void CalcSize()
			{
				Vector3 pos = vertexList[0].pos;
				Vector3 pos2 = vertexList[1].pos;
				Vector3 pos3 = vertexList[2].pos;
				Vector3 pos4 = vertexList[3].pos;
				tetraCenter = (pos + pos2 + pos3 + pos4) / 4f;
				float a = Vector3.Distance(pos, tetraCenter);
				float b = Vector3.Distance(pos2, tetraCenter);
				float a2 = Vector3.Distance(pos3, tetraCenter);
				float b2 = Vector3.Distance(pos4, tetraCenter);
				tetraSize = Mathf.Max(Mathf.Max(a, b), Mathf.Max(a2, b2));
			}

			public bool Verification()
			{
				Vector3 pos = vertexList[0].pos;
				Vector3 pos2 = vertexList[1].pos;
				Vector3 pos3 = vertexList[2].pos;
				Vector3 pos4 = vertexList[3].pos;
				Vector3 lhs = Vector3.Cross(pos - pos2, pos - pos3);
				if (lhs.magnitude < 1E-05f)
				{
					return false;
				}
				lhs.Normalize();
				Vector3 rhs = pos4 - pos;
				if (Mathf.Abs(Vector3.Dot(lhs, rhs)) < tetraSize * 0.2f)
				{
					return false;
				}
				return true;
			}
		}

		public static GameObject ReplaceSkinnedMeshRendererToMeshRenderer(SkinnedMeshRenderer sren, bool replaceSkinnedMeshRenderer)
		{
			GameObject gameObject = sren.gameObject;
			sren.enabled = false;
			GameObject gameObject2 = gameObject;
			if (!replaceSkinnedMeshRenderer)
			{
				gameObject2 = new GameObject(gameObject.name + "[work mesh]");
				Transform transform = gameObject2.transform;
				transform.SetParent(gameObject.transform);
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
			}
			gameObject2.AddComponent<MeshFilter>().sharedMesh = sren.sharedMesh;
			MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterials = sren.sharedMaterials;
			meshRenderer.lightProbeUsage = sren.lightProbeUsage;
			meshRenderer.probeAnchor = sren.probeAnchor;
			meshRenderer.reflectionProbeUsage = sren.reflectionProbeUsage;
			meshRenderer.shadowCastingMode = sren.shadowCastingMode;
			meshRenderer.receiveShadows = sren.receiveShadows;
			meshRenderer.motionVectorGenerationMode = sren.motionVectorGenerationMode;
			meshRenderer.allowOcclusionWhenDynamic = sren.allowOcclusionWhenDynamic;
			if (replaceSkinnedMeshRenderer)
			{
				Object.Destroy(sren);
			}
			return gameObject2;
		}

		public static bool CalcMeshWorldPositionNormalTangent(MeshData meshData, List<Transform> boneList, out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector3>();
			if (meshData == null || boneList == null)
			{
				return false;
			}
			if (!meshData.isSkinning)
			{
				Transform transform = boneList[0];
				for (int i = 0; i < meshData.VertexCount; i++)
				{
					MeshData.VertexWeight vertexWeight = meshData.vertexWeightList[i];
					Vector3 item = transform.TransformPoint(vertexWeight.localPos);
					Vector3 item2 = transform.TransformDirection(vertexWeight.localNor);
					Vector3 item3 = transform.TransformDirection(vertexWeight.localTan);
					wposList.Add(item);
					wnorList.Add(item2);
					wtanList.Add(item3);
				}
			}
			else
			{
				float[] array = new float[4];
				int[] array2 = new int[4];
				for (int j = 0; j < meshData.VertexCount; j++)
				{
					Vector3 zero = Vector3.zero;
					Vector3 zero2 = Vector3.zero;
					Vector3 zero3 = Vector3.zero;
					uint pack = meshData.vertexInfoList[j];
					int num = DataUtility.Unpack4_28Hi(pack);
					int num2 = DataUtility.Unpack4_28Low(pack);
					for (int k = 0; k < num; k++)
					{
						MeshData.VertexWeight vertexWeight2 = meshData.vertexWeightList[num2 + k];
						Transform transform2 = boneList[vertexWeight2.parentIndex];
						zero += transform2.TransformPoint(vertexWeight2.localPos) * vertexWeight2.weight;
						zero2 += transform2.TransformDirection(vertexWeight2.localNor) * vertexWeight2.weight;
						zero3 += transform2.TransformDirection(vertexWeight2.localTan) * vertexWeight2.weight;
					}
					wposList.Add(zero);
					wnorList.Add(zero2);
					wtanList.Add(zero3);
				}
			}
			return true;
		}

		public static bool CalcMeshWorldPositionNormalTangent(Renderer ren, Mesh mesh, out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector3>();
			if (ren == null || mesh == null)
			{
				return false;
			}
			int vertexCount = mesh.vertexCount;
			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;
			bool flag = normals != null && normals.Length != 0;
			Vector4[] tangents = mesh.tangents;
			bool flag2 = tangents != null && tangents.Length != 0;
			if (ren is MeshRenderer)
			{
				Transform transform = ren.transform;
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
						Vector3 item3 = transform.TransformDirection(tangents[i]);
						item3.Normalize();
						wtanList.Add(item3);
					}
				}
			}
			else if (ren is SkinnedMeshRenderer)
			{
				Transform[] bones = (ren as SkinnedMeshRenderer).bones;
				Matrix4x4[] bindposes = mesh.bindposes;
				BoneWeight[] boneWeights = mesh.boneWeights;
				float[] array = new float[4];
				int[] array2 = new int[4];
				for (int j = 0; j < vertexCount; j++)
				{
					Vector3 zero = Vector3.zero;
					Vector3 zero2 = Vector3.zero;
					Vector3 zero3 = Vector3.zero;
					array[0] = boneWeights[j].weight0;
					array[1] = boneWeights[j].weight1;
					array[2] = boneWeights[j].weight2;
					array[3] = boneWeights[j].weight3;
					array2[0] = boneWeights[j].boneIndex0;
					array2[1] = boneWeights[j].boneIndex1;
					array2[2] = boneWeights[j].boneIndex2;
					array2[3] = boneWeights[j].boneIndex3;
					for (int k = 0; k < 4; k++)
					{
						float num = array[k];
						if (num > 0f)
						{
							int num2 = array2[k];
							Transform transform2 = bones[num2];
							Vector3 position = bindposes[num2].MultiplyPoint3x4(vertices[j]);
							position = transform2.TransformPoint(position);
							position *= num;
							zero += position;
							if (flag)
							{
								position = bindposes[num2].MultiplyVector(normals[j]);
								zero2 += transform2.TransformVector(position).normalized * num;
							}
							if (flag2)
							{
								position = bindposes[num2].MultiplyVector(tangents[j]);
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
						wtanList.Add(zero3);
					}
				}
			}
			return true;
		}

		public static bool CalcMeshLocalNormalTangent(List<int> selectList, Vector3[] vlist, Vector2[] uvlist, int[] triangles, out List<Vector3> lnorList, out List<Vector3> ltanList)
		{
			lnorList = new List<Vector3>();
			ltanList = new List<Vector3>();
			int num = vlist.Length;
			int num2 = triangles.Length / 3;
			for (int i = 0; i < num; i++)
			{
				lnorList.Add(Vector3.zero);
				ltanList.Add(Vector3.zero);
			}
			for (int j = 0; j < num2; j++)
			{
				int num3 = j * 3;
				int num4 = triangles[num3];
				int num5 = triangles[num3 + 1];
				int num6 = triangles[num3 + 2];
				Vector3 vector = vlist[num4];
				Vector3 vector2 = vlist[num5];
				Vector3 vector3 = vlist[num6];
				Vector2 vector4 = uvlist[num4];
				Vector2 vector5 = uvlist[num5];
				Vector2 vector6 = uvlist[num6];
				if (selectList != null)
				{
					int num7 = selectList[num4];
					int num8 = selectList[num5];
					int num9 = selectList[num6];
					if (num7 == 0 || num8 == 0 || num9 == 0)
					{
						continue;
					}
				}
				Vector3 vector7 = vector2 - vector;
				Vector3 rhs = vector3 - vector;
				Vector3 lhs = vector7 * 1000f;
				rhs *= 1000f;
				Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
				lnorList[num4] += normalized;
				lnorList[num5] += normalized;
				lnorList[num6] += normalized;
				Vector3 vector8 = vector2 - vector;
				Vector3 vector9 = vector3 - vector;
				Vector2 vector10 = vector5 - vector4;
				Vector2 vector11 = vector6 - vector4;
				float num10 = vector10.x * vector11.y - vector10.y * vector11.x;
				Vector3 vector12 = Vector3.zero;
				if (num10 == 0f)
				{
					Debug.LogError("area = 0!");
				}
				else
				{
					float num11 = 1f / num10;
					vector12 = new Vector3(vector8.x * vector11.y + vector9.x * (0f - vector10.y), vector8.y * vector11.y + vector9.y * (0f - vector10.y), vector8.z * vector11.y + vector9.z * (0f - vector10.y)) * num11;
					vector12 = -vector12;
				}
				ltanList[num4] += vector12;
				ltanList[num5] += vector12;
				ltanList[num6] += vector12;
			}
			for (int k = 0; k < num; k++)
			{
				if (lnorList[k] != Vector3.zero && ltanList[k] != Vector3.zero)
				{
					ltanList[k] = ltanList[k].normalized;
					lnorList[k] = lnorList[k].normalized;
				}
				else
				{
					ltanList[k] = new Vector3(0f, 1f, 0f);
					lnorList[k] = new Vector4(1f, 0f, 0f, 1f);
				}
			}
			return true;
		}

		public static List<HashSet<int>> GetTriangleToVertexLinkList(int vcnt, List<int> lineList, List<int> triangleList)
		{
			List<HashSet<int>> list = new List<HashSet<int>>();
			for (int i = 0; i < vcnt; i++)
			{
				list.Add(new HashSet<int>());
			}
			if (lineList != null && lineList.Count > 0)
			{
				int num = lineList.Count / 2;
				for (int j = 0; j < num; j++)
				{
					int num2 = j * 2;
					int num3 = lineList[num2];
					int num4 = lineList[num2 + 1];
					list[num3].Add(num4);
					list[num4].Add(num3);
				}
			}
			if (triangleList != null && triangleList.Count > 0)
			{
				int num5 = triangleList.Count / 3;
				for (int k = 0; k < num5; k++)
				{
					int num6 = k * 3;
					int num7 = triangleList[num6];
					int num8 = triangleList[num6 + 1];
					int num9 = triangleList[num6 + 2];
					list[num7].Add(num8);
					list[num7].Add(num9);
					list[num8].Add(num7);
					list[num8].Add(num9);
					list[num9].Add(num7);
					list[num9].Add(num8);
				}
			}
			return list;
		}

		public static List<HashSet<int>> GetVertexLinkList(int vcnt, HashSet<uint> lineSet)
		{
			List<HashSet<int>> list = new List<HashSet<int>>();
			for (int i = 0; i < vcnt; i++)
			{
				list.Add(new HashSet<int>());
			}
			foreach (uint item in lineSet)
			{
				int v;
				int v2;
				DataUtility.UnpackPair(item, out v, out v2);
				list[v].Add(v2);
				list[v2].Add(v);
			}
			return list;
		}

		public static Dictionary<uint, List<int>> GetTriangleEdgePair(List<int> triangleList)
		{
			Dictionary<uint, List<int>> dictionary = new Dictionary<uint, List<int>>();
			if (triangleList != null && triangleList.Count >= 3)
			{
				int num = triangleList.Count / 3;
				for (int i = 0; i < num; i++)
				{
					int num2 = i * 3;
					int v = triangleList[num2];
					int num3 = triangleList[num2 + 1];
					int v2 = triangleList[num2 + 2];
					AddTriangleEdge(v, num3, i, dictionary);
					AddTriangleEdge(v, v2, i, dictionary);
					AddTriangleEdge(num3, v2, i, dictionary);
				}
			}
			return dictionary;
		}

		private static void AddTriangleEdge(int v0, int v1, int tindex, Dictionary<uint, List<int>> triangleEdgeDict)
		{
			uint key = DataUtility.PackPair(v0, v1);
			List<int> list;
			if (triangleEdgeDict.ContainsKey(key))
			{
				list = triangleEdgeDict[key];
			}
			else
			{
				list = new List<int>();
				triangleEdgeDict.Add(key, list);
			}
			list.Add(tindex);
		}

		public static List<ulong> GetTrianglePackList(List<int> triangleList)
		{
			List<ulong> list = new List<ulong>();
			if (triangleList != null && triangleList.Count > 0)
			{
				int num = triangleList.Count / 3;
				for (int i = 0; i < num; i++)
				{
					int num2 = i * 3;
					int v = triangleList[num2];
					int v2 = triangleList[num2 + 1];
					int v3 = triangleList[num2 + 2];
					ulong item = DataUtility.PackTriple(v, v2, v3);
					list.Add(item);
				}
			}
			return list;
		}

		public static float ClosestPtBoneLine(Vector3 pos, Transform bone, float lineWidth, out Vector3 d)
		{
			float num = 10000f;
			d = bone.position;
			if (bone.childCount == 0)
			{
				return Mathf.Max(Vector3.Distance(pos, bone.position) - lineWidth, 0f);
			}
			for (int i = 0; i < bone.childCount; i++)
			{
				Transform child = bone.GetChild(i);
				Vector3 position = bone.position;
				Vector3 position2 = child.position;
				float3 @float = MathUtility.ClosestPtPointSegment(pos, position, position2);
				float num2 = Mathf.Max(Vector3.Distance(pos, @float) - lineWidth, 0f);
				if (num2 < num)
				{
					num = num2;
					d = @float;
				}
			}
			return num;
		}

		public static void CalcTetraMesh(List<Vector3> posList, out int tetraCount, out List<int> tetraIndexList, out List<float> tetraSizeList)
		{
			tetraCount = 0;
			tetraIndexList = new List<int>();
			tetraSizeList = new List<float>();
			List<TetraVertex> list = new List<TetraVertex>();
			for (int i = 0; i < posList.Count; i++)
			{
				list.Add(new TetraVertex(posList[i], i));
			}
			Bounds bounds = new Bounds(posList[0], Vector3.one * 0.01f);
			foreach (Vector3 pos in posList)
			{
				bounds.Encapsulate(pos);
			}
			float num = Mathf.Max(Mathf.Max(bounds.extents.x, bounds.extents.y), bounds.extents.z) * 100f;
			TetraVertex tetraVertex = new TetraVertex();
			TetraVertex tetraVertex2 = new TetraVertex();
			TetraVertex tetraVertex3 = new TetraVertex();
			TetraVertex tetraVertex4 = new TetraVertex();
			tetraVertex.pos = bounds.center + new Vector3(0f, 0f - num, 0f);
			tetraVertex2.pos = bounds.center + new Vector3(0f - num, num, num);
			tetraVertex3.pos = bounds.center + new Vector3(num, num, num);
			tetraVertex4.pos = bounds.center + new Vector3(0f, num, 0f - num);
			int count = list.Count;
			tetraVertex.index = count++;
			tetraVertex2.index = count++;
			tetraVertex3.index = count++;
			tetraVertex4.index = count++;
			list.Add(tetraVertex);
			list.Add(tetraVertex2);
			list.Add(tetraVertex3);
			list.Add(tetraVertex4);
			List<Tetra> list2 = new List<Tetra>();
			Tetra tetra = new Tetra(tetraVertex, tetraVertex2, tetraVertex3, tetraVertex4);
			tetra.CalcCircumcircle();
			list2.Add(tetra);
			Dictionary<ulong, Tetra> dictionary = new Dictionary<ulong, Tetra>();
			dictionary.Add(tetra.GetTetraHash(), tetra);
			for (int j = 0; j < list.Count - 4; j++)
			{
				TetraVertex tetraVertex5 = list[j];
				List<Tetra> list3 = new List<Tetra>();
				int num2 = 0;
				while (num2 < list2.Count)
				{
					Tetra tetra2 = list2[num2];
					if (!tetra2.ContainsPoint(tetraVertex5) && tetra2.IntersectCircumcircle(tetraVertex5.pos))
					{
						Tetra item = new Tetra(tetra2.vertexList[0], tetra2.vertexList[1], tetra2.vertexList[2], tetraVertex5);
						Tetra item2 = new Tetra(tetra2.vertexList[0], tetra2.vertexList[2], tetra2.vertexList[3], tetraVertex5);
						Tetra item3 = new Tetra(tetra2.vertexList[0], tetra2.vertexList[3], tetra2.vertexList[1], tetraVertex5);
						Tetra item4 = new Tetra(tetra2.vertexList[1], tetra2.vertexList[2], tetra2.vertexList[3], tetraVertex5);
						list3.Add(item);
						list3.Add(item2);
						list3.Add(item3);
						list3.Add(item4);
						dictionary.Remove(tetra2.GetTetraHash());
						list2.RemoveAt(num2);
					}
					else
					{
						num2++;
					}
				}
				foreach (Tetra item6 in list3)
				{
					ulong tetraHash = item6.GetTetraHash();
					if (!dictionary.ContainsKey(tetraHash))
					{
						item6.CalcCircumcircle();
						dictionary.Add(tetraHash, item6);
						list2.Add(item6);
					}
					else
					{
						Tetra item5 = dictionary[tetraHash];
						dictionary.Remove(tetraHash);
						list2.Remove(item5);
					}
				}
			}
			int num3 = 0;
			while (num3 < list2.Count)
			{
				Tetra tetra3 = list2[num3];
				if (tetra3.ContainsPoint(tetraVertex, tetraVertex2, tetraVertex3, tetraVertex4))
				{
					dictionary.Remove(tetra3.GetTetraHash());
					list2.RemoveAt(num3);
				}
				else
				{
					num3++;
				}
			}
			list.Remove(tetraVertex);
			list.Remove(tetraVertex2);
			list.Remove(tetraVertex3);
			list.Remove(tetraVertex4);
			int num4 = 0;
			while (num4 < list2.Count)
			{
				if (!list2[num4].Verification())
				{
					list2.RemoveAt(num4);
				}
				else
				{
					num4++;
				}
			}
			tetraCount = list2.Count;
			foreach (Tetra item7 in list2)
			{
				for (int k = 0; k < 4; k++)
				{
					tetraIndexList.Add(item7.vertexList[k].index);
				}
				tetraSizeList.Add(item7.tetraSize);
			}
		}

		public static Transform GetReplaceBone(Transform now, Dictionary<Transform, Transform> boneReplaceDict)
		{
			if (!boneReplaceDict.ContainsKey(now))
			{
				return now;
			}
			return boneReplaceDict[now];
		}
	}
}
