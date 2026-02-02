using System;
using System.Collections.Generic;
using Curve;
using UnityEngine;

namespace Tubular
{
	public class Tubular
	{
		private const float PI2 = (float)Math.PI * 2f;

		public static Mesh Build(global::Curve.Curve curve, int tubularSegments, float radius, int radialSegments, bool closed)
		{
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			List<Vector4> list3 = new List<Vector4>();
			List<Vector2> list4 = new List<Vector2>();
			List<int> list5 = new List<int>();
			List<FrenetFrame> frames = curve.ComputeFrenetFrames(tubularSegments, closed);
			for (int i = 0; i < tubularSegments; i++)
			{
				GenerateSegment(curve, frames, tubularSegments, radius, radialSegments, list, list2, list3, i);
			}
			GenerateSegment(curve, frames, tubularSegments, radius, radialSegments, list, list2, list3, (!closed) ? tubularSegments : 0);
			for (int j = 0; j <= tubularSegments; j++)
			{
				for (int k = 0; k <= radialSegments; k++)
				{
					float x = 1f * (float)k / (float)radialSegments;
					float y = 1f * (float)j / (float)tubularSegments;
					list4.Add(new Vector2(x, y));
				}
			}
			for (int l = 1; l <= tubularSegments; l++)
			{
				for (int m = 1; m <= radialSegments; m++)
				{
					int item = (radialSegments + 1) * (l - 1) + (m - 1);
					int item2 = (radialSegments + 1) * l + (m - 1);
					int item3 = (radialSegments + 1) * l + m;
					int item4 = (radialSegments + 1) * (l - 1) + m;
					list5.Add(item);
					list5.Add(item4);
					list5.Add(item2);
					list5.Add(item2);
					list5.Add(item4);
					list5.Add(item3);
				}
			}
			Mesh mesh = new Mesh();
			mesh.vertices = list.ToArray();
			mesh.normals = list2.ToArray();
			mesh.tangents = list3.ToArray();
			mesh.uv = list4.ToArray();
			mesh.SetIndices(list5.ToArray(), MeshTopology.Triangles, 0);
			return mesh;
		}

		private static void GenerateSegment(global::Curve.Curve curve, List<FrenetFrame> frames, int tubularSegments, float radius, int radialSegments, List<Vector3> vertices, List<Vector3> normals, List<Vector4> tangents, int i)
		{
			float u = 1f * (float)i / (float)tubularSegments;
			Vector3 pointAt = curve.GetPointAt(u);
			FrenetFrame frenetFrame = frames[i];
			Vector3 normal = frenetFrame.Normal;
			Vector3 binormal = frenetFrame.Binormal;
			for (int j = 0; j <= radialSegments; j++)
			{
				float f = 1f * (float)j / (float)radialSegments * ((float)Math.PI * 2f);
				float num = Mathf.Sin(f);
				Vector3 normalized = (Mathf.Cos(f) * normal + num * binormal).normalized;
				vertices.Add(pointAt + radius * normalized);
				normals.Add(normalized);
				Vector3 tangent = frenetFrame.Tangent;
				tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 0f));
			}
		}
	}
}
