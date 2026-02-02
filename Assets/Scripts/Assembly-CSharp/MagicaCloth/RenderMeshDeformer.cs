#define RELEASE
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public class RenderMeshDeformer : BaseMeshDeformer, IBoneReplace
	{
		public enum RecalculateMode
		{
			None = 0,
			UpdateNormalPerFrame = 1,
			UpdateNormalAndTangentPerFrame = 2
		}

		private const int DATA_VERSION = 2;

		[SerializeField]
		private RecalculateMode normalAndTangentUpdateMode = RecalculateMode.UpdateNormalPerFrame;

		[SerializeField]
		private Mesh sharedMesh;

		[SerializeField]
		private int meshOptimize;

		private MeshFilter meshFilter;

		private SkinnedMeshRenderer skinMeshRenderer;

		private Transform[] originalBones;

		private Transform[] boneList;

		private Mesh mesh;

		private bool oldUse;

		public bool IsChangePosition { get; set; }

		public bool IsChangeNormalTangent { get; set; }

		public bool IsChangeBoneWeights { get; set; }

		public Mesh SharedMesh
		{
			get
			{
				return sharedMesh;
			}
		}

		public override int GetDataHash()
		{
			int num = base.GetDataHash();
			num += sharedMesh.GetDataHash();
			if (meshOptimize != 0)
			{
				num += meshOptimize.GetDataHash();
			}
			return num;
		}

		public void OnValidate()
		{
			if (Application.isPlaying && status.IsActive)
			{
				SetRecalculateNormalAndTangentMode();
			}
		}

		protected override void OnInit()
		{
			base.OnInit();
			if (status.IsInitError)
			{
				return;
			}
			if (base.TargetObject == null)
			{
				status.SetInitError();
				return;
			}
			Renderer component = base.TargetObject.GetComponent<Renderer>();
			if (component == null)
			{
				status.SetInitError();
				return;
			}
			if (MeshData.VerifyData() != 0)
			{
				status.SetInitError();
				return;
			}
			base.VertexCount = MeshData.VertexCount;
			base.TriangleCount = MeshData.TriangleCount;
			mesh = null;
			if (component is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = (skinMeshRenderer = component as SkinnedMeshRenderer);
				mesh = UnityEngine.Object.Instantiate(sharedMesh);
				mesh.MarkDynamic();
				originalBones = skinnedMeshRenderer.bones;
				List<Transform> list = new List<Transform>(originalBones);
				list.Add(component.transform);
				boneList = list.ToArray();
				List<Matrix4x4> list2 = new List<Matrix4x4>(sharedMesh.bindposes);
				list2.Add(Matrix4x4.identity);
				mesh.bindposes = list2.ToArray();
			}
			else
			{
				mesh = UnityEngine.Object.Instantiate(sharedMesh);
				mesh.MarkDynamic();
				meshFilter = base.TargetObject.GetComponent<MeshFilter>();
				Debug.Assert(meshFilter);
			}
			oldUse = false;
			int instanceID = sharedMesh.GetInstanceID();
			bool num = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.IsEmptySharedRenderMesh(instanceID);
			base.MeshIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddRenderMesh(instanceID, MeshData.isSkinning, MeshData.baseScale, MeshData.VertexCount, base.IsSkinning ? (boneList.Length - 1) : 0, base.IsSkinning ? MeshData.VertexCount : 0);
			if (num)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetRenderSharedMeshData(base.MeshIndex, base.IsSkinning, mesh.vertices, mesh.normals, mesh.tangents, base.IsSkinning ? mesh.boneWeights : null);
			}
			CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.UpdateMeshState(base.MeshIndex);
			SetRecalculateNormalAndTangentMode();
		}

		protected override void OnActive()
		{
			base.OnActive();
			if (status.IsInitSuccess)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetRenderMeshActive(base.MeshIndex, true);
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddRenderMeshBone(base.MeshIndex, base.TargetObject.transform);
			}
		}

		protected override void OnInactive()
		{
			base.OnInactive();
			if (status.IsInitSuccess && CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveRenderMeshBone(base.MeshIndex);
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetRenderMeshActive(base.MeshIndex, false);
			}
		}

		public override void Dispose()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveRenderMesh(base.MeshIndex);
			}
			base.Dispose();
		}

		private void SetRecalculateNormalAndTangentMode()
		{
			bool sw = false;
			bool sw2 = false;
			if (normalAndTangentUpdateMode == RecalculateMode.UpdateNormalPerFrame)
			{
				sw = true;
			}
			else if (normalAndTangentUpdateMode == RecalculateMode.UpdateNormalAndTangentPerFrame)
			{
				sw = (sw2 = true);
			}
			CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetRenderMeshFlag(base.MeshIndex, 8u, sw);
			CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.SetRenderMeshFlag(base.MeshIndex, 16u, sw2);
		}

		public override bool IsMeshUse()
		{
			if (status.IsInitSuccess)
			{
				return CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.IsUseRenderMesh(base.MeshIndex);
			}
			return false;
		}

		public override bool IsActiveMesh()
		{
			if (status.IsInitSuccess)
			{
				return CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.IsActiveRenderMesh(base.MeshIndex);
			}
			return false;
		}

		public override void AddUseMesh(object parent)
		{
			VirtualMeshDeformer virtualMeshDeformer = parent as VirtualMeshDeformer;
			Debug.Assert(virtualMeshDeformer != null);
			if (status.IsInitSuccess)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddUseRenderMesh(base.MeshIndex);
				IsChangePosition = true;
				IsChangeNormalTangent = true;
				IsChangeBoneWeights = true;
				int meshIndex = virtualMeshDeformer.MeshIndex;
				PhysicsManagerMeshData.VirtualMeshInfo virtualMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.virtualMeshInfoList[meshIndex];
				PhysicsManagerMeshData.SharedVirtualMeshInfo sharedVirtualMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.sharedVirtualMeshInfoList[virtualMeshInfo.sharedVirtualMeshIndex];
				int renderMeshDeformerIndex = virtualMeshDeformer.GetRenderMeshDeformerIndex(this);
				long key = (long)sharedVirtualMeshInfo.uid << 16 + renderMeshDeformerIndex;
				int index = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.sharedChildMeshIdToSharedVirtualMeshIndexDict[key];
				PhysicsManagerMeshData.SharedChildMeshInfo sharedChildMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.sharedChildMeshInfoList[index];
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.LinkRenderMesh(base.MeshIndex, sharedChildMeshInfo.vertexChunk.startIndex, sharedChildMeshInfo.weightChunk.startIndex, virtualMeshInfo.vertexChunk.startIndex, sharedVirtualMeshInfo.vertexChunk.startIndex);
			}
		}

		public override void RemoveUseMesh(object parent)
		{
			VirtualMeshDeformer virtualMeshDeformer = parent as VirtualMeshDeformer;
			Debug.Assert(virtualMeshDeformer != null);
			if (status.IsInitSuccess)
			{
				int meshIndex = virtualMeshDeformer.MeshIndex;
				PhysicsManagerMeshData.VirtualMeshInfo virtualMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.virtualMeshInfoList[meshIndex];
				PhysicsManagerMeshData.SharedVirtualMeshInfo sharedVirtualMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.sharedVirtualMeshInfoList[virtualMeshInfo.sharedVirtualMeshIndex];
				int renderMeshDeformerIndex = virtualMeshDeformer.GetRenderMeshDeformerIndex(this);
				long key = (long)sharedVirtualMeshInfo.uid << 16 + renderMeshDeformerIndex;
				int index = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.sharedChildMeshIdToSharedVirtualMeshIndexDict[key];
				PhysicsManagerMeshData.SharedChildMeshInfo sharedChildMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.sharedChildMeshInfoList[index];
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.UnlinkRenderMesh(base.MeshIndex, sharedChildMeshInfo.vertexChunk.startIndex, sharedChildMeshInfo.weightChunk.startIndex, virtualMeshInfo.vertexChunk.startIndex, sharedVirtualMeshInfo.vertexChunk.startIndex);
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveUseRenderMesh(base.MeshIndex);
				IsChangePosition = true;
				IsChangeNormalTangent = true;
				IsChangeBoneWeights = true;
			}
		}

		public override void Finish(int bufferIndex)
		{
			bool flag = IsMeshUse();
			bool flag2 = true;
			if (flag && bufferIndex == 1)
			{
				flag2 = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.renderMeshStateDict[base.MeshIndex].IsFlag(256u);
				if (!flag2)
				{
					return;
				}
			}
			if (flag != oldUse)
			{
				if ((bool)meshFilter)
				{
					meshFilter.mesh = (flag ? mesh : sharedMesh);
				}
				else if ((bool)skinMeshRenderer)
				{
					skinMeshRenderer.sharedMesh = (flag ? mesh : sharedMesh);
					skinMeshRenderer.bones = (flag ? boneList : originalBones);
				}
				oldUse = flag;
				if (flag)
				{
					IsChangePosition = true;
					IsChangeNormalTangent = true;
					IsChangeBoneWeights = true;
				}
			}
			if ((flag || IsChangePosition || IsChangeNormalTangent) && flag2)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.CopyToRenderMeshLocalPositionData(base.MeshIndex, mesh, bufferIndex);
				bool flag3 = normalAndTangentUpdateMode == RecalculateMode.UpdateNormalPerFrame || normalAndTangentUpdateMode == RecalculateMode.UpdateNormalAndTangentPerFrame;
				bool flag4 = normalAndTangentUpdateMode == RecalculateMode.UpdateNormalAndTangentPerFrame;
				if (flag3 || flag4)
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.CopyToRenderMeshLocalNormalTangentData(base.MeshIndex, mesh, bufferIndex, flag3, flag4);
				}
				else if (IsChangeNormalTangent)
				{
					mesh.normals = sharedMesh.normals;
					mesh.tangents = sharedMesh.tangents;
				}
				IsChangePosition = false;
				IsChangeNormalTangent = false;
			}
			if (flag && base.IsSkinning && IsChangeBoneWeights && flag2)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.CopyToRenderMeshBoneWeightData(base.MeshIndex, mesh, sharedMesh, bufferIndex);
				IsChangeBoneWeights = false;
			}
		}

		public void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
			if (originalBones != null)
			{
				for (int i = 0; i < originalBones.Length; i++)
				{
					originalBones[i] = MeshUtility.GetReplaceBone(originalBones[i], boneReplaceDict);
				}
			}
			if (boneList != null)
			{
				for (int j = 0; j < boneList.Length; j++)
				{
					boneList[j] = MeshUtility.GetReplaceBone(boneList[j], boneReplaceDict);
				}
			}
		}

		public override int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector3>();
			if (Application.isPlaying)
			{
				if (base.Status.IsDispose)
				{
					return 0;
				}
				if (!IsMeshUse() || base.TargetObject == null)
				{
					return 0;
				}
				Vector3[] array = new Vector3[base.VertexCount];
				Vector3[] array2 = new Vector3[base.VertexCount];
				Vector3[] array3 = new Vector3[base.VertexCount];
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.CopyToRenderMeshWorldData(base.MeshIndex, base.TargetObject.transform, array, array2, array3);
				wposList = new List<Vector3>(array);
				wnorList = new List<Vector3>(array2);
				wtanList = new List<Vector3>(array3);
				return base.VertexCount;
			}
			if (base.TargetObject == null)
			{
				return 0;
			}
			MeshUtility.CalcMeshWorldPositionNormalTangent(base.TargetObject.GetComponent<Renderer>(), sharedMesh, out wposList, out wnorList, out wtanList);
			return wposList.Count;
		}

		public override List<int> GetEditorTriangleList()
		{
			if ((bool)sharedMesh)
			{
				return new List<int>(sharedMesh.triangles);
			}
			return null;
		}

		public override List<int> GetEditorLineList()
		{
			return null;
		}

		public override int GetVersion()
		{
			return 2;
		}

		public override Define.Error VerifyData()
		{
			Define.Error error = base.VerifyData();
			if (error != 0)
			{
				return error;
			}
			if (sharedMesh == null)
			{
				return Define.Error.SharedMeshNull;
			}
			if (!sharedMesh.isReadable)
			{
				return Define.Error.SharedMeshCannotRead;
			}
			if (sharedMesh.vertexCount > 65535)
			{
				return Define.Error.MeshVertexCount65535Over;
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
				StaticStringBuilder.AppendLine("Skinning: ", MeshData.isSkinning);
				StaticStringBuilder.AppendLine("Vertex: ", MeshData.VertexCount);
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
					StaticStringBuilder.AppendLine("Please create the mesh data.");
				}
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			}
			return StaticStringBuilder.ToString();
		}
	}
}
