using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/magica-cloth-mesh-cloth/")]
	[AddComponentMenu("MagicaCloth/MagicaMeshCloth", 100)]
	public class MagicaMeshCloth : BaseCloth
	{
		private const int DATA_VERSION = 6;

		private const int ERR_DATA_VERSION = 3;

		[SerializeField]
		private MagicaVirtualDeformer virtualDeformer;

		[SerializeField]
		private int virtualDeformerHash;

		[SerializeField]
		private int virtualDeformerVersion;

		public VirtualMeshDeformer Deformer
		{
			get
			{
				if (virtualDeformer != null)
				{
					return virtualDeformer.Deformer;
				}
				return null;
			}
		}

		public override int GetDataHash()
		{
			return base.GetDataHash() + virtualDeformer.GetDataHash();
		}

		protected override void Reset()
		{
			base.Reset();
			ResetParams();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
		}

		protected override void OnInit()
		{
			base.OnInit();
		}

		protected override void OnActive()
		{
			base.OnActive();
		}

		protected override void OnInactive()
		{
			base.OnInactive();
		}

		protected override void OnDispose()
		{
			base.OnDispose();
		}

		protected override uint UserFlag(int index)
		{
			return 0u;
		}

		protected override Transform UserTransform(int index)
		{
			return null;
		}

		protected override float3 UserTransformLocalPosition(int vindex)
		{
			return 0;
		}

		protected override quaternion UserTransformLocalRotation(int vindex)
		{
			return quaternion.identity;
		}

		public override int GetDeformerCount()
		{
			return 1;
		}

		public override BaseMeshDeformer GetDeformer(int index)
		{
			return Deformer;
		}

		protected override MeshData GetMeshData()
		{
			return Deformer.MeshData;
		}

		protected override void WorkerInit()
		{
			MeshParticleWorker meshParticleWorker = CreateSingleton<MagicaPhysicsManager>.Instance.Compute.MeshParticleWorker;
			PhysicsManagerMeshData.VirtualMeshInfo virtualMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.GetVirtualMeshInfo(Deformer.MeshIndex);
			ClothData clothData = base.ClothData;
			for (int i = 0; i < clothData.VertexUseCount; i++)
			{
				int num = particleChunk.startIndex + i;
				int vindex = virtualMeshInfo.vertexChunk.startIndex + clothData.useVertexList[i];
				if (num >= 0)
				{
					meshParticleWorker.Add(base.TeamId, vindex, num);
				}
			}
		}

		protected override void SetDeformerUseVertex(bool sw, BaseMeshDeformer deformer, int deformerIndex)
		{
			ClothData clothData = base.ClothData;
			for (int i = 0; i < clothData.VertexUseCount; i++)
			{
				if (!base.ClothData.IsInvalidVertex(i))
				{
					int vindex = clothData.useVertexList[i];
					bool fix = !base.ClothData.IsMoveVertex(i);
					if (sw)
					{
						deformer.AddUseVertex(vindex, fix);
					}
					else
					{
						deformer.RemoveUseVertex(vindex, fix);
					}
				}
			}
		}

		public override int GetVersion()
		{
			return 6;
		}

		public override int GetErrorVersion()
		{
			return 3;
		}

		public override void CreateVerifyData()
		{
			base.CreateVerifyData();
			virtualDeformerHash = virtualDeformer.SaveDataHash;
			virtualDeformerVersion = virtualDeformer.SaveDataVersion;
		}

		public override Define.Error VerifyData()
		{
			Define.Error error = base.VerifyData();
			if (error != 0)
			{
				return error;
			}
			if (base.ClothData == null)
			{
				return Define.Error.ClothDataNull;
			}
			if (virtualDeformer == null)
			{
				return Define.Error.DeformerNull;
			}
			Define.Error error2 = virtualDeformer.VerifyData();
			if (error2 != 0)
			{
				return error2;
			}
			if (virtualDeformerHash != virtualDeformer.SaveDataHash)
			{
				return Define.Error.DeformerHashMismatch;
			}
			if (virtualDeformerVersion != virtualDeformer.SaveDataVersion)
			{
				return Define.Error.DeformerVersionMismatch;
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
			{
				ClothData clothData = base.ClothData;
				StaticStringBuilder.AppendLine("Active: ", base.Status.IsActive);
				StaticStringBuilder.AppendLine("Vertex: ", clothData.VertexUseCount);
				StaticStringBuilder.AppendLine("Clamp Distance: ", clothData.ClampDistanceConstraintCount);
				StaticStringBuilder.AppendLine("Clamp Position: ", clothParams.UseClampPositionLength ? clothData.VertexUseCount : 0);
				StaticStringBuilder.AppendLine("Clamp Rotation: ", clothData.ClampRotationConstraintRootCount, " - ", clothData.ClampRotationConstraintDataCount);
				StaticStringBuilder.AppendLine("Struct Distance: ", clothData.StructDistanceConstraintCount / 2);
				StaticStringBuilder.AppendLine("Bend Distance: ", clothData.BendDistanceConstraintCount / 2);
				StaticStringBuilder.AppendLine("Near Distance: ", clothData.NearDistanceConstraintCount / 2);
				StaticStringBuilder.AppendLine("Restore Rotation: ", clothData.RestoreRotationConstraintCount);
				StaticStringBuilder.AppendLine("Triangle Bend: ", clothData.TriangleBendConstraintCount);
				StaticStringBuilder.AppendLine("Collider: ", teamData.ColliderCount);
				StaticStringBuilder.Append("Line Rotation: ", clothData.LineRotationWorkerCount);
				break;
			}
			case Define.Error.EmptyData:
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			default:
				StaticStringBuilder.AppendLine("This mesh cloth is in a state error!");
				if (Application.isPlaying)
				{
					StaticStringBuilder.AppendLine("Execution stopped.");
				}
				else
				{
					StaticStringBuilder.AppendLine("Please recreate the cloth data.");
				}
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			}
			return StaticStringBuilder.ToString();
		}

		public bool IsValidPointSelect()
		{
			if (base.ClothSelection == null)
			{
				return false;
			}
			if (Deformer.MeshData.ChildCount != base.ClothSelection.DeformerCount)
			{
				return false;
			}
			return true;
		}

		public override int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
		{
			return Deformer.GetEditorPositionNormalTangent(out wposList, out wnorList, out wtanList);
		}

		public override List<int> GetEditorTriangleList()
		{
			return Deformer.GetEditorTriangleList();
		}

		public override List<int> GetEditorLineList()
		{
			return Deformer.GetEditorLineList();
		}

		public override List<int> GetSelectionList()
		{
			if (base.ClothSelection != null && virtualDeformer != null && Deformer.MeshData != null)
			{
				return base.ClothSelection.GetSelectionData(Deformer.MeshData, Deformer.GetRenderDeformerMeshList());
			}
			return null;
		}

		public override List<int> GetUseList()
		{
			if (Application.isPlaying && virtualDeformer != null && Deformer != null)
			{
				PhysicsManagerMeshData.VirtualMeshInfo virtualMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.GetVirtualMeshInfo(Deformer.MeshIndex);
				FixedChunkNativeArray<byte> virtualVertexUseList = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.virtualVertexUseList;
				List<int> list = new List<int>();
				for (int i = 0; i < virtualMeshInfo.vertexChunk.dataLength; i++)
				{
					list.Add(virtualVertexUseList[virtualMeshInfo.vertexChunk.startIndex + i]);
				}
				return list;
			}
			return null;
		}

		public override List<ShareDataObject> GetAllShareDataObject()
		{
			List<ShareDataObject> allShareDataObject = base.GetAllShareDataObject();
			if (Deformer != null)
			{
				allShareDataObject.Add(Deformer.MeshData);
			}
			return allShareDataObject;
		}

		public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
		{
			ShareDataObject shareDataObject = base.DuplicateShareDataObject(source);
			if (shareDataObject != null)
			{
				return shareDataObject;
			}
			if (Deformer.MeshData == source)
			{
				Deformer.MeshData = ShareDataObject.Clone(Deformer.MeshData);
				return Deformer.MeshData;
			}
			return null;
		}

		private void ResetParams()
		{
			clothParams.SetRadius(0.02f, 0.02f);
			clothParams.SetMass(10f, 1f, true, -0.5f, true);
			clothParams.SetGravity(true);
			clothParams.SetDrag(true, 0.01f, 0.01f);
			clothParams.SetMaxVelocity(true);
			clothParams.SetWorldInfluence(10f, 0.5f, 0.5f);
			clothParams.SetTeleport(false);
			clothParams.SetClampDistanceRatio(true, 0.5f, 1.2f);
			clothParams.SetClampPositionLength(false, 0f, 0.4f);
			clothParams.SetClampRotationAngle(false);
			clothParams.SetRestoreDistance();
			clothParams.SetRestoreRotation(false, 0.01f, 0f, 0.5f);
			clothParams.SetSpring(false);
			clothParams.SetAdjustRotation();
			clothParams.SetTriangleBend(true, 0.9f, 0.9f);
			clothParams.SetVolume(false);
			clothParams.SetCollision(false);
			clothParams.SetExternalForce(0.3f, 1f, 0.7f);
		}
	}
}
