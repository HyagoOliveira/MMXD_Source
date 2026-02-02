using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
	public class Utility
	{
		public static uint PackPair(int v0, int v1)
		{
			if (v0 > v1)
			{
				return (uint)(v1 << 16) | ((uint)v0 & 0xFFFFu);
			}
			return (uint)(v0 << 16) | ((uint)v1 & 0xFFFFu);
		}

		public static void UnpackPair(uint pack, out int v0, out int v1)
		{
			v0 = (int)((pack >> 16) & 0xFFFF);
			v1 = (int)(pack & 0xFFFF);
		}

		public static ulong PackTriple(int v0, int v1, int v2)
		{
			List<ulong> list = new List<ulong>();
			list.Add((ulong)v0);
			list.Add((ulong)v1);
			list.Add((ulong)v2);
			list.Sort();
			return (list[0] << 32) | (list[1] << 16) | list[2];
		}

		public static void UnpackTriple(ulong pack, out int v0, out int v1, out int v2)
		{
			v0 = (int)((pack >> 32) & 0xFFFF);
			v1 = (int)((pack >> 16) & 0xFFFF);
			v2 = (int)(pack & 0xFFFF);
		}

		public static ulong PackQuater(int v0, int v1, int v2, int v3)
		{
			List<ulong> list = new List<ulong>();
			list.Add((ulong)v0);
			list.Add((ulong)v1);
			list.Add((ulong)v2);
			list.Add((ulong)v3);
			list.Sort();
			return (list[0] << 48) | (list[1] << 32) | (list[2] << 16) | list[3];
		}

		public static void UnpackQuater(ulong pack, out int v0, out int v1, out int v2, out int v3)
		{
			v0 = (int)((pack >> 48) & 0xFFFF);
			v1 = (int)((pack >> 32) & 0xFFFF);
			v2 = (int)((pack >> 16) & 0xFFFF);
			v3 = (int)(pack & 0xFFFF);
		}

		public static uint Pack16(int hi, int low)
		{
			return (uint)(hi << 16) | ((uint)low & 0xFFFFu);
		}

		public static int Unpack16Hi(uint pack)
		{
			return (int)((pack >> 16) & 0xFFFF);
		}

		public static int Unpack16Low(uint pack)
		{
			return (int)(pack & 0xFFFF);
		}

		public static ulong Pack32(int hi, int low)
		{
			return (ulong)(((long)hi << 32) | (low & 0xFFFFFFFFu));
		}

		public static int Unpack32Hi(ulong pack)
		{
			return (int)((pack >> 32) & 0xFFFFFFFFu);
		}

		public static int Unpack32Low(ulong pack)
		{
			return (int)(pack & 0xFFFFFFFFu);
		}

		public static void CalcFinalDataWorldPositionNormalTangent(FinalData final, out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector4> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector4>();
			if (final.VertexCount == 0 || final.BoneCount == 0)
			{
				return;
			}
			int vertexCount = final.VertexCount;
			if (!final.IsSkinning)
			{
				Transform transform = final.bones[0];
				for (int i = 0; i < vertexCount; i++)
				{
					Vector3 item = transform.TransformPoint(final.vertices[i]);
					wposList.Add(item);
					Vector3 item2 = transform.TransformDirection(final.normals[i]);
					item2.Normalize();
					wnorList.Add(item2);
					Vector3 vector = transform.TransformDirection(final.tangents[i]);
					vector.Normalize();
					wtanList.Add(new Vector4(vector.x, vector.y, vector.z, final.tangents[i].w));
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
				array[0] = final.boneWeights[j].weight0;
				array[1] = final.boneWeights[j].weight1;
				array[2] = final.boneWeights[j].weight2;
				array[3] = final.boneWeights[j].weight3;
				array2[0] = final.boneWeights[j].boneIndex0;
				array2[1] = final.boneWeights[j].boneIndex1;
				array2[2] = final.boneWeights[j].boneIndex2;
				array2[3] = final.boneWeights[j].boneIndex3;
				for (int k = 0; k < 4; k++)
				{
					float num = array[k];
					if (num > 0f)
					{
						int index = array2[k];
						Transform transform2 = final.bones[index];
						Vector3 position = final.bindPoses[index].MultiplyPoint3x4(final.vertices[j]);
						position = transform2.TransformPoint(position);
						position *= num;
						zero += position;
						position = final.bindPoses[index].MultiplyVector(final.normals[j]);
						zero2 += transform2.TransformVector(position).normalized * num;
						position = final.bindPoses[index].MultiplyVector(final.tangents[j]);
						zero3 += transform2.TransformVector(position).normalized * num;
					}
				}
				wposList.Add(zero);
				wnorList.Add(zero2);
				wtanList.Add(new Vector4(zero3.x, zero3.y, zero3.z, final.tangents[j].w));
			}
		}

		public static void CalcFinalDataChildWorldPositionNormalTangent(FinalData final, int meshIndex, List<Vector3> sposList, List<Vector3> snorList, List<Vector4> stanList, out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector4> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector4>();
			List<Quaternion> list = new List<Quaternion>();
			for (int i = 0; i < sposList.Count; i++)
			{
				Quaternion item = Quaternion.LookRotation(snorList[i], stanList[i]);
				list.Add(item);
			}
			FinalData.MeshInfo meshInfo = final.meshList[meshIndex];
			float[] array = new float[4];
			int[] array2 = new int[4];
			for (int j = 0; j < meshInfo.VertexCount; j++)
			{
				Vector3 zero = Vector3.zero;
				Vector3 zero2 = Vector3.zero;
				Vector3 zero3 = Vector3.zero;
				array[0] = meshInfo.boneWeights[j].weight0;
				array[1] = meshInfo.boneWeights[j].weight1;
				array[2] = meshInfo.boneWeights[j].weight2;
				array[3] = meshInfo.boneWeights[j].weight3;
				array2[0] = meshInfo.boneWeights[j].boneIndex0;
				array2[1] = meshInfo.boneWeights[j].boneIndex1;
				array2[2] = meshInfo.boneWeights[j].boneIndex2;
				array2[3] = meshInfo.boneWeights[j].boneIndex3;
				for (int k = 0; k < 4; k++)
				{
					float num = array[k];
					if (num > 0f)
					{
						int index = array2[k];
						Quaternion quaternion = list[index];
						Vector3 vector = final.vertexBindPoses[index].MultiplyPoint3x4(meshInfo.vertices[j]);
						vector = quaternion * vector + sposList[index];
						vector *= num;
						zero += vector;
						vector = final.vertexBindPoses[index].MultiplyVector(meshInfo.normals[j]);
						zero2 += (quaternion * vector).normalized * num;
						vector = final.vertexBindPoses[index].MultiplyVector(meshInfo.tangents[j]);
						zero3 += (quaternion * vector).normalized * num;
					}
				}
				wposList.Add(zero);
				wnorList.Add(zero2);
				wtanList.Add(new Vector4(zero3.x, zero3.y, zero3.z, -1f));
			}
		}

		public static void CalcLocalPositionNormalTangent(Transform root, List<Vector3> wposList, List<Vector3> wnorList, List<Vector4> wtanList)
		{
			for (int i = 0; i < wposList.Count; i++)
			{
				wposList[i] = root.InverseTransformPoint(wposList[i]);
			}
			for (int j = 0; j < wnorList.Count; j++)
			{
				wnorList[j] = root.InverseTransformDirection(wnorList[j]);
			}
			for (int k = 0; k < wtanList.Count; k++)
			{
				Vector3 direction = wtanList[k];
				float w = wtanList[k].w;
				direction = root.InverseTransformDirection(direction);
				wtanList[k] = new Vector4(direction.x, direction.y, direction.z, w);
			}
		}
	}
}
