using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
	[Serializable]
	public class FinalData
	{
		[Serializable]
		public class MeshIndexData
		{
			public List<uint> meshIndexPackList = new List<uint>();
		}

		[Serializable]
		public class MeshInfo
		{
			public int meshIndex;

			public Mesh mesh;

			public List<Vector3> vertices = new List<Vector3>();

			public List<Vector3> normals = new List<Vector3>();

			public List<Vector4> tangents = new List<Vector4>();

			public List<BoneWeight> boneWeights = new List<BoneWeight>();

			public List<int> parents = new List<int>();

			public int VertexCount
			{
				get
				{
					return vertices.Count;
				}
			}
		}

		public List<Vector3> vertices = new List<Vector3>();

		public List<Vector3> normals = new List<Vector3>();

		public List<Vector4> tangents = new List<Vector4>();

		public List<Vector2> uvs = new List<Vector2>();

		public List<BoneWeight> boneWeights = new List<BoneWeight>();

		public List<Matrix4x4> bindPoses = new List<Matrix4x4>();

		public List<Transform> bones = new List<Transform>();

		public List<int> lines = new List<int>();

		public List<int> triangles = new List<int>();

		public List<int> tetras = new List<int>();

		public List<float> tetraSizes = new List<float>();

		public List<Matrix4x4> vertexBindPoses = new List<Matrix4x4>();

		public List<MeshIndexData> vertexToMeshIndexList = new List<MeshIndexData>();

		public List<int> vertexToTriangleCountList = new List<int>();

		public List<int> vertexToTriangleStartList = new List<int>();

		public List<int> vertexToTriangleIndexList = new List<int>();

		public List<MeshInfo> meshList = new List<MeshInfo>();

		public bool IsValid
		{
			get
			{
				return vertices.Count > 0;
			}
		}

		public int VertexCount
		{
			get
			{
				return vertices.Count;
			}
		}

		public int LineCount
		{
			get
			{
				return lines.Count / 2;
			}
		}

		public int TriangleCount
		{
			get
			{
				return triangles.Count / 3;
			}
		}

		public int TetraCount
		{
			get
			{
				return tetras.Count / 4;
			}
		}

		public int BoneCount
		{
			get
			{
				return bones.Count;
			}
		}

		public bool IsSkinning
		{
			get
			{
				return true;
			}
		}

		public int MeshCount
		{
			get
			{
				return meshList.Count;
			}
		}
	}
}
