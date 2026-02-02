#define RELEASE
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
	public class ReductionMesh
	{
		public enum ReductionWeightMode
		{
			Distance = 0,
			Average = 1
		}

		private MeshData meshData = new MeshData();

		private ReductionData reductionData = new ReductionData();

		private DebugData debugData = new DebugData();

		public ReductionWeightMode WeightMode { get; set; }

		public MeshData MeshData
		{
			get
			{
				meshData.SetParent(this);
				return meshData;
			}
		}

		public ReductionData ReductionData
		{
			get
			{
				reductionData.SetParent(this);
				return reductionData;
			}
		}

		public DebugData DebugData
		{
			get
			{
				debugData.SetParent(this);
				return debugData;
			}
		}

		public int AddMesh(bool isSkinning, Mesh mesh, List<Transform> bones, Matrix4x4[] bindPoseList, BoneWeight[] boneWeightList)
		{
			return MeshData.AddMesh(isSkinning, mesh, bones, bindPoseList, boneWeightList);
		}

		public int AddMesh(Renderer ren)
		{
			if (ren == null)
			{
				Debug.LogError("Renderer is NUll!");
				return -1;
			}
			if (ren is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = ren as SkinnedMeshRenderer;
				return MeshData.AddMesh(true, skinnedMeshRenderer.sharedMesh, new List<Transform>(skinnedMeshRenderer.bones), skinnedMeshRenderer.sharedMesh.bindposes, skinnedMeshRenderer.sharedMesh.boneWeights);
			}
			MeshFilter component = ren.GetComponent<MeshFilter>();
			List<Transform> list = new List<Transform>();
			list.Add(ren.transform);
			return MeshData.AddMesh(false, component.sharedMesh, list, null, null);
		}

		public int AddMesh(Transform root, List<Vector3> posList, List<Vector3> norList = null, List<Vector4> tanList = null, List<Vector2> uvList = null, List<int> triangleList = null)
		{
			return MeshData.AddMesh(root, posList, norList, tanList, uvList, triangleList);
		}

		public void Reduction(float zeroRadius, float radius, float polygonLength, bool createTetra)
		{
			if (zeroRadius > 0f)
			{
				ReductionData.ReductionZeroDistance(zeroRadius);
			}
			if (radius > 0f)
			{
				ReductionData.ReductionRadius(radius);
			}
			if (polygonLength > 0f)
			{
				ReductionData.ReductionPolygonLink(polygonLength);
			}
			MeshData.UpdateMeshData(createTetra);
			ReductionData.ReductionBone();
		}

		public FinalData GetFinalData(Transform root)
		{
			return MeshData.GetFinalData(root);
		}
	}
}
