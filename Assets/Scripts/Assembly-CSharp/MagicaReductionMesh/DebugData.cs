#define RELEASE
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
	public class DebugData : ReductionMeshAccess
	{
		public void DispMeshInfo(string header = "")
		{
			Debug.Log(header + (string.IsNullOrEmpty(header) ? "" : "\n") + " Mesh:" + base.MeshData.MeshCount + " VertexCnt:" + base.MeshData.VertexCount + " TriangleCnt:" + base.MeshData.TriangleCount + " LineCnt:" + base.MeshData.LineCount + " TetraCnt:" + base.MeshData.TetraCount);
		}

		public static void DebugDrawShared(FinalData final, bool drawTriangle = true, bool drawLine = true, bool drawTetra = true, bool drawVertexNormal = true, bool drawVertexTangent = true, bool drawNumber = false, int maxPolygonCount = int.MaxValue, int layer = -1, int tetraIndex = -1, float tetraSize = 1f, List<int> drawNumberList = null, float axisSize = 0.01f)
		{
			if (final == null)
			{
				return;
			}
			List<Vector3> wposList;
			List<Vector3> wnorList;
			List<Vector4> wtanList;
			Utility.CalcFinalDataWorldPositionNormalTangent(final, out wposList, out wnorList, out wtanList);
			if (drawTriangle)
			{
				int triangleCount = final.TriangleCount;
				for (int i = 0; i < triangleCount && i < maxPolygonCount; i++)
				{
					int num = i * 3;
					int num2 = final.triangles[num];
					int num3 = final.triangles[num + 1];
					int num4 = final.triangles[num + 2];
					if (drawNumberList == null || drawNumberList.Count <= 0 || drawNumberList.Contains(num2) || drawNumberList.Contains(num3) || drawNumberList.Contains(num4))
					{
						Vector3 from = wposList[num2];
						Vector3 vector = wposList[num3];
						Vector3 to = wposList[num4];
						Gizmos.color = Color.magenta;
						Gizmos.DrawLine(from, vector);
						Gizmos.DrawLine(vector, to);
						Gizmos.DrawLine(from, to);
					}
				}
			}
			if (drawLine)
			{
				Gizmos.color = Color.cyan;
				int lineCount = final.LineCount;
				for (int j = 0; j < lineCount; j++)
				{
					int num5 = j * 2;
					int index = final.lines[num5];
					int index2 = final.lines[num5 + 1];
					Gizmos.DrawLine(wposList[index], wposList[index2]);
				}
			}
			if (drawTetra)
			{
				Gizmos.color = Color.green;
				int tetraCount = final.TetraCount;
				for (int k = 0; k < tetraCount; k++)
				{
					DrawTetra(final, k, wposList, tetraSize);
				}
			}
			if (tetraIndex >= 0 && tetraIndex < final.TetraCount)
			{
				Gizmos.color = Color.red;
				DrawTetra(final, tetraIndex, wposList, tetraSize);
			}
			if (drawVertexNormal)
			{
				Gizmos.color = Color.blue;
				if (drawNumberList != null && drawNumberList.Count > 0)
				{
					foreach (int drawNumber2 in drawNumberList)
					{
						Vector3 vector2 = wposList[drawNumber2];
						Gizmos.DrawLine(vector2, vector2 + wnorList[drawNumber2] * axisSize);
					}
				}
				else
				{
					for (int l = 0; l < final.VertexCount; l++)
					{
						Vector3 vector3 = wposList[l];
						Gizmos.DrawLine(vector3, vector3 + wnorList[l] * axisSize);
					}
				}
			}
			if (!drawVertexTangent)
			{
				return;
			}
			Gizmos.color = Color.red;
			if (drawNumberList != null && drawNumberList.Count > 0)
			{
				foreach (int drawNumber3 in drawNumberList)
				{
					Vector3 vector4 = wposList[drawNumber3];
					Vector3 vector5 = wtanList[drawNumber3];
					Gizmos.DrawLine(vector4, vector4 + vector5 * axisSize);
				}
				return;
			}
			for (int m = 0; m < final.VertexCount; m++)
			{
				Vector3 vector6 = wposList[m];
				Vector3 vector7 = wtanList[m];
				Gizmos.DrawLine(vector6, vector6 + vector7 * axisSize);
			}
		}

		private static void DrawTetra(FinalData final, int tetraIndex, List<Vector3> wposList, float tetraSize)
		{
			if (!(final.tetraSizes[tetraIndex] > tetraSize))
			{
				int num = tetraIndex * 4;
				int index = final.tetras[num];
				int index2 = final.tetras[num + 1];
				int index3 = final.tetras[num + 2];
				int index4 = final.tetras[num + 3];
				Vector3 from = wposList[index];
				Vector3 vector = wposList[index2];
				Vector3 vector2 = wposList[index3];
				Vector3 vector3 = wposList[index4];
				Gizmos.DrawLine(from, vector);
				Gizmos.DrawLine(from, vector2);
				Gizmos.DrawLine(from, vector3);
				Gizmos.DrawLine(vector, vector2);
				Gizmos.DrawLine(vector2, vector3);
				Gizmos.DrawLine(vector3, vector);
			}
		}

		public static void DebugDrawChild(FinalData final, bool drawPosition = false, bool drawNormal = false, bool drawTriangle = false, bool drawNumber = false, int maxVertexCount = int.MaxValue, float positionSize = 0.001f, float axisSize = 0.01f)
		{
			if (final == null || (!drawPosition && !drawNormal && !drawTriangle && !drawNumber))
			{
				return;
			}
			List<Vector3> wposList;
			List<Vector3> wnorList;
			List<Vector4> wtanList;
			Utility.CalcFinalDataWorldPositionNormalTangent(final, out wposList, out wnorList, out wtanList);
			for (int i = 0; i < final.MeshCount; i++)
			{
				List<Vector3> wposList2;
				List<Vector3> wnorList2;
				List<Vector4> wtanList2;
				Utility.CalcFinalDataChildWorldPositionNormalTangent(final, i, wposList, wnorList, wtanList, out wposList2, out wnorList2, out wtanList2);
				for (int j = 0; j < wposList2.Count && j < maxVertexCount; j++)
				{
					Vector3 vector = wposList2[j];
					if (drawPosition)
					{
						Gizmos.color = Color.red;
						Gizmos.DrawSphere(vector, positionSize);
					}
					if (drawNormal)
					{
						Gizmos.color = Color.blue;
						Gizmos.DrawLine(vector, vector + wnorList2[j] * axisSize);
					}
				}
				if (drawTriangle)
				{
					Gizmos.color = Color.magenta;
					int[] triangles = final.meshList[i].mesh.triangles;
					for (int k = 0; k < triangles.Length / 3; k++)
					{
						int index = triangles[k * 3];
						int index2 = triangles[k * 3 + 1];
						int index3 = triangles[k * 3 + 2];
						Vector3 vector2 = wposList2[index];
						Vector3 vector3 = wposList2[index2];
						Vector3 vector4 = wposList2[index3];
						Gizmos.DrawLine(vector2, vector3);
						Gizmos.DrawLine(vector3, vector4);
						Gizmos.DrawLine(vector4, vector2);
					}
				}
			}
		}
	}
}
