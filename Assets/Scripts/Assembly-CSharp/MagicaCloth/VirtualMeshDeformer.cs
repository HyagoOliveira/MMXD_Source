using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public class VirtualMeshDeformer : BaseMeshDeformer, IBoneReplace
	{
		private const int DATA_VERSION = 1;

		[SerializeField]
		private List<MagicaRenderDeformer> renderDeformerList = new List<MagicaRenderDeformer>();

		[SerializeField]
		private List<int> renderDeformerHashList = new List<int>();

		[SerializeField]
		private int renderDeformerVersion;

		[SerializeField]
		[Range(0f, 0.1f)]
		private float mergeVertexDistance = 0.001f;

		[SerializeField]
		[Range(0f, 0.1f)]
		private float mergeTriangleDistance;

		[SerializeField]
		[Range(10f, 90f)]
		private float sameSurfaceAngle = 80f;

		[SerializeField]
		private bool useSkinning = true;

		[SerializeField]
		[Range(1f, 4f)]
		private int maxWeightCount = 3;

		[SerializeField]
		[Range(1f, 5f)]
		private float weightPow = 3f;

		[SerializeField]
		private List<Transform> boneList = new List<Transform>();

		private List<int> sharedChildMeshIndexList = new List<int>();

		public float MergeVertexDistance
		{
			get
			{
				return mergeVertexDistance;
			}
		}

		public float MergeTriangleDistance
		{
			get
			{
				return mergeTriangleDistance;
			}
		}

		public float SameSurfaceAngle
		{
			get
			{
				return sameSurfaceAngle;
			}
		}

		public int MaxWeightCount
		{
			get
			{
				if (useSkinning)
				{
					if (mergeVertexDistance <= 0.001f && mergeTriangleDistance <= 0.001f)
					{
						return 1;
					}
					return maxWeightCount;
				}
				return 1;
			}
		}

		public float WeightPow
		{
			get
			{
				return weightPow;
			}
		}

		public int RenderDeformerCount
		{
			get
			{
				return renderDeformerList.Count;
			}
		}

		public override int GetDataHash()
		{
			return base.GetDataHash() + RenderDeformerCount.GetDataHash() + renderDeformerList.GetDataHash() + base.BoneCount.GetDataHash() + boneList.GetDataHash();
		}

		protected override void OnInit()
		{
			base.OnInit();
			if (status.IsInitError)
			{
				return;
			}
			if (MeshData == null || MeshData.VerifyData() != 0)
			{
				status.SetInitError();
				return;
			}
			for (int i = 0; i < MeshData.ChildCount; i++)
			{
				if (renderDeformerList[i] == null)
				{
					status.SetInitError();
					return;
				}
				MagicaRenderDeformer magicaRenderDeformer = renderDeformerList[i];
				if (magicaRenderDeformer == null)
				{
					status.SetInitError();
					return;
				}
				magicaRenderDeformer.Init();
				if (magicaRenderDeformer.Deformer.Status.IsInitError)
				{
					status.SetInitError();
					return;
				}
			}
			base.VertexCount = MeshData.VertexCount;
			base.TriangleCount = MeshData.TriangleCount;
			int vertexToTriangleIndexCount = ((MeshData.vertexToTriangleIndexList != null) ? MeshData.vertexToTriangleIndexList.Length : 0);
			int saveDataHash = MeshData.SaveDataHash;
			bool num = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.IsEmptySharedVirtualMesh(saveDataHash);
			base.MeshIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddVirtualMesh(saveDataHash, MeshData.VertexCount, MeshData.WeightCount, MeshData.BoneCount, MeshData.TriangleCount, vertexToTriangleIndexCount, base.TargetObject.transform);
			base.SkinningVertexCount = MeshData.VertexCount;
			if (num)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetSharedVirtualMeshData(base.MeshIndex, MeshData.vertexInfoList, MeshData.vertexWeightList, MeshData.uvList, MeshData.triangleList, MeshData.vertexToTriangleInfoList, MeshData.vertexToTriangleIndexList);
			}
			for (int j = 0; j < MeshData.ChildCount; j++)
			{
				MeshData.ChildData childData = MeshData.childDataList[j];
				long cuid = (long)saveDataHash << 16 + j;
				bool num2 = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.IsEmptySharedChildMesh(cuid);
				int num3 = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddSharedChildMesh(cuid, base.MeshIndex, childData.VertexCount, childData.vertexWeightList.Length);
				if (num2)
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetSharedChildMeshData(num3, childData.vertexInfoList, childData.vertexWeightList);
				}
				sharedChildMeshIndexList.Add(num3);
			}
		}

		public override void Dispose()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance() && CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.ExistsVirtualMesh(base.MeshIndex))
			{
				foreach (int sharedChildMeshIndex in sharedChildMeshIndexList)
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveSharedChildMesh(sharedChildMeshIndex);
				}
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveVirtualMesh(base.MeshIndex);
			}
			base.Dispose();
		}

		protected override void OnActive()
		{
			base.OnActive();
			if (status.IsInitSuccess)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddVirtualMeshBone(base.MeshIndex, boneList);
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetVirtualMeshActive(base.MeshIndex, true);
			}
		}

		protected override void OnInactive()
		{
			base.OnInactive();
			if (status.IsInitSuccess && CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetVirtualMeshActive(base.MeshIndex, false);
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveVirtualMeshBone(base.MeshIndex);
			}
		}

		public override void Finish(int bufferIndex)
		{
		}

		public void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
			for (int i = 0; i < boneList.Count; i++)
			{
				boneList[i] = MeshUtility.GetReplaceBone(boneList[i], boneReplaceDict);
			}
		}

		public BaseMeshDeformer GetDeformer()
		{
			return this;
		}

		public MagicaRenderDeformer GetRenderDeformer(int index)
		{
			return renderDeformerList[index];
		}

		public int GetRenderMeshDeformerIndex(RenderMeshDeformer deformer)
		{
			return renderDeformerList.FindIndex((MagicaRenderDeformer d) => d.Deformer == deformer);
		}

		public List<MeshData> GetRenderDeformerMeshList()
		{
			List<MeshData> list = new List<MeshData>();
			for (int i = 0; i < renderDeformerList.Count; i++)
			{
				MeshData item = null;
				if (renderDeformerList[i] != null)
				{
					item = renderDeformerList[i].Deformer.MeshData;
				}
				list.Add(item);
			}
			return list;
		}

		public override bool IsMeshUse()
		{
			if (status.IsInitSuccess)
			{
				return CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.IsUseVirtualMesh(base.MeshIndex);
			}
			return false;
		}

		public override bool IsActiveMesh()
		{
			if (status.IsInitSuccess)
			{
				return CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.IsActiveVirtualMesh(base.MeshIndex);
			}
			return false;
		}

		public override void AddUseMesh(object parent)
		{
			if (status.IsInitSuccess)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddUseVirtualMesh(base.MeshIndex);
				for (int i = 0; i < renderDeformerList.Count; i++)
				{
					renderDeformerList[i].Deformer.AddUseMesh(this);
				}
			}
		}

		public override void RemoveUseMesh(object parent)
		{
			if (status.IsInitSuccess)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveUseVirtualMesh(base.MeshIndex);
				for (int i = 0; i < renderDeformerList.Count; i++)
				{
					renderDeformerList[i].Deformer.RemoveUseMesh(this);
				}
			}
		}

		public override bool AddUseVertex(int vindex, bool fix)
		{
			if (!status.IsInitSuccess)
			{
				return false;
			}
			return CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddUseVirtualVertex(base.MeshIndex, vindex, fix);
		}

		public override bool RemoveUseVertex(int vindex, bool fix)
		{
			if (!status.IsInitSuccess)
			{
				return false;
			}
			return CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveUseVirtualVertex(base.MeshIndex, vindex, fix);
		}

		public override void ResetFuturePrediction()
		{
			base.ResetFuturePrediction();
			CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.ResetFuturePredictionVirtualMeshBone(base.MeshIndex);
		}

		public override int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector3>();
			if (Application.isPlaying)
			{
				if (!IsMeshUse())
				{
					return 0;
				}
				Vector3[] array = new Vector3[base.VertexCount];
				Vector3[] array2 = new Vector3[base.VertexCount];
				Vector3[] array3 = new Vector3[base.VertexCount];
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.CopyToVirtualMeshWorldData(base.MeshIndex, array, array2, array3);
				wposList = new List<Vector3>(array);
				wnorList = new List<Vector3>(array2);
				wtanList = new List<Vector3>(array3);
				return base.VertexCount;
			}
			if (MeshData == null || base.TargetObject == null || boneList.Count == 0)
			{
				return 0;
			}
			MeshUtility.CalcMeshWorldPositionNormalTangent(MeshData, boneList, out wposList, out wnorList, out wtanList);
			return MeshData.VertexCount;
		}

		public override List<int> GetEditorTriangleList()
		{
			if (MeshData != null && MeshData.triangleList != null)
			{
				return new List<int>(MeshData.triangleList);
			}
			return null;
		}

		public override List<int> GetEditorLineList()
		{
			if (MeshData != null && MeshData.lineList != null)
			{
				return new List<int>(MeshData.lineList);
			}
			return null;
		}

		public override int GetVersion()
		{
			return 1;
		}

		public override void CreateVerifyData()
		{
			base.CreateVerifyData();
			renderDeformerHashList.Clear();
			renderDeformerVersion = 0;
			foreach (MagicaRenderDeformer renderDeformer in renderDeformerList)
			{
				renderDeformerHashList.Add(renderDeformer.SaveDataHash);
				renderDeformerVersion = renderDeformer.SaveDataVersion;
			}
		}

		public override Define.Error VerifyData()
		{
			Define.Error error = base.VerifyData();
			if (error != 0)
			{
				return error;
			}
			if (renderDeformerList.Count == 0)
			{
				return Define.Error.DeformerCountZero;
			}
			foreach (MagicaRenderDeformer renderDeformer in renderDeformerList)
			{
				if (renderDeformer == null)
				{
					return Define.Error.DeformerNull;
				}
				Define.Error error2 = renderDeformer.VerifyData();
				if (error2 != 0)
				{
					return error2;
				}
			}
			if (renderDeformerHashList.Count != renderDeformerList.Count)
			{
				return Define.Error.DeformerCountMismatch;
			}
			for (int i = 0; i < renderDeformerHashList.Count; i++)
			{
				MagicaRenderDeformer magicaRenderDeformer = renderDeformerList[i];
				if (magicaRenderDeformer.SaveDataHash != renderDeformerHashList[i])
				{
					return Define.Error.DeformerHashMismatch;
				}
				if (magicaRenderDeformer.SaveDataVersion != renderDeformerVersion)
				{
					return Define.Error.DeformerVersionMismatch;
				}
			}
			if (boneList.Count == 0)
			{
				return Define.Error.BoneListZero;
			}
			foreach (Transform bone in boneList)
			{
				if (bone == null)
				{
					return Define.Error.BoneListNull;
				}
			}
			if (renderDeformerList.Count != MeshData.ChildCount)
			{
				return Define.Error.DeformerCountMismatch;
			}
			return Define.Error.None;
		}

		public override string GetInformation()
		{
			StaticStringBuilder.Clear();
			Define.Error error = VerifyData();
			switch (error)
			{
			case Define.Error.None:
				StaticStringBuilder.AppendLine("Active: ", base.Status.IsActive);
				StaticStringBuilder.AppendLine("Vertex: ", MeshData.VertexCount);
				StaticStringBuilder.AppendLine("Line: ", MeshData.LineCount);
				StaticStringBuilder.AppendLine("Triangle: ", MeshData.TriangleCount);
				StaticStringBuilder.Append("Bone: ", MeshData.BoneCount);
				break;
			case Define.Error.EmptyData:
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			default:
				StaticStringBuilder.AppendLine("This mesh data is Invalid!");
				if (Application.isPlaying)
				{
					StaticStringBuilder.AppendLine("Execution stopped.");
				}
				else
				{
					StaticStringBuilder.AppendLine("Please recreate the mesh data.");
				}
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			}
			return StaticStringBuilder.ToString();
		}
	}
}
