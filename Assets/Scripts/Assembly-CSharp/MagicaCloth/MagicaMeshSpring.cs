#define RELEASE
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/magica-cloth-mesh-spring/")]
	[AddComponentMenu("MagicaCloth/MagicaMeshSpring", 100)]
	public class MagicaMeshSpring : BaseCloth
	{
		public enum Axis
		{
			X = 0,
			Y = 1,
			Z = 2,
			InverseX = 3,
			InverseY = 4,
			InverseZ = 5
		}

		private const int DATA_VERSION = 5;

		private const int ERR_DATA_VERSION = 3;

		[SerializeField]
		private MagicaVirtualDeformer virtualDeformer;

		[SerializeField]
		private int virtualDeformerHash;

		[SerializeField]
		private int virtualDeformerVersion;

		[SerializeField]
		private Transform centerTransform;

		[SerializeField]
		private Axis directionAxis;

		[SerializeField]
		private SpringData springData;

		[SerializeField]
		private int springDataHash;

		[SerializeField]
		private int springDataVersion;

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

		public SpringData SpringData
		{
			get
			{
				return springData;
			}
		}

		public int UseVertexCount
		{
			get
			{
				if (SpringData == null)
				{
					return 0;
				}
				return SpringData.UseVertexCount;
			}
		}

		public Transform CenterTransform
		{
			get
			{
				return centerTransform;
			}
			set
			{
				centerTransform = value;
			}
		}

		public Axis DirectionAxis
		{
			get
			{
				return directionAxis;
			}
			set
			{
				directionAxis = value;
			}
		}

		public Vector3 CenterTransformDirection
		{
			get
			{
				Vector3 result = Vector3.forward;
				if ((bool)centerTransform)
				{
					switch (directionAxis)
					{
					case Axis.X:
						result = centerTransform.right;
						break;
					case Axis.Y:
						result = centerTransform.up;
						break;
					case Axis.Z:
						result = centerTransform.forward;
						break;
					case Axis.InverseX:
						result = -centerTransform.right;
						break;
					case Axis.InverseY:
						result = -centerTransform.up;
						break;
					case Axis.InverseZ:
						result = -centerTransform.forward;
						break;
					}
				}
				return result;
			}
		}

		public override int GetDataHash()
		{
			return base.GetDataHash() + virtualDeformer.GetDataHash() + centerTransform.GetDataHash() + SpringData.GetDataHash();
		}

		public SpringData.DeformerData GetDeformerData()
		{
			return SpringData.deformerData;
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

		protected override void ClothInit()
		{
			ClothData clothData = ShareDataObject.CreateShareData<ClothData>("ClothData_work");
			clothData.selectionData.Add(1);
			clothData.vertexFlagLevelList.Add(0u);
			clothData.vertexDepthList.Add(0f);
			clothData.rootList.Add(0);
			clothData.useVertexList.Add(0);
			clothData.initScale = SpringData.initScale;
			clothData.SaveDataHash = 1;
			clothData.SaveDataVersion = clothData.GetVersion();
			base.ClothData = clothData;
			clothDataHash = clothData.SaveDataHash;
			clothDataVersion = clothData.SaveDataVersion;
			base.ClothInit();
			CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetFlag(base.TeamId, 16u, true);
		}

		protected override void ClothActive()
		{
			base.ClothActive();
		}

		protected override uint UserFlag(int index)
		{
			return 0x4000u | 0x2000u;
		}

		protected override Transform UserTransform(int index)
		{
			return CenterTransform;
		}

		protected override float3 UserTransformLocalPosition(int vindex)
		{
			return CenterTransform.localPosition;
		}

		protected override quaternion UserTransformLocalRotation(int vindex)
		{
			return CenterTransform.localRotation;
		}

		public override int GetDeformerCount()
		{
			if (!(virtualDeformer != null))
			{
				return 0;
			}
			return 1;
		}

		public override BaseMeshDeformer GetDeformer(int index)
		{
			if (virtualDeformer != null)
			{
				return virtualDeformer.Deformer;
			}
			return null;
		}

		protected override MeshData GetMeshData()
		{
			return null;
		}

		protected override void WorkerInit()
		{
			int startIndex = base.ParticleChunk.startIndex;
			SpringMeshWorker springMeshWorker = CreateSingleton<MagicaPhysicsManager>.Instance.Compute.SpringMeshWorker;
			BaseMeshDeformer deformer = GetDeformer(0);
			Debug.Assert(deformer != null);
			deformer.Init();
			SpringData.DeformerData deformerData = GetDeformerData();
			Debug.Assert(deformerData != null);
			PhysicsManagerMeshData.VirtualMeshInfo virtualMeshInfo = CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.GetVirtualMeshInfo(deformer.MeshIndex);
			for (int i = 0; i < deformerData.UseVertexCount; i++)
			{
				int num = deformerData.useVertexIndexList[i];
				springMeshWorker.Add(base.TeamId, virtualMeshInfo.vertexChunk.startIndex + num, startIndex, deformerData.weightList[i]);
			}
		}

		protected override void SetDeformerUseVertex(bool sw, BaseMeshDeformer deformer, int deformerIndex)
		{
			SpringData.DeformerData deformerData = GetDeformerData();
			int useVertexCount = deformerData.UseVertexCount;
			for (int i = 0; i < useVertexCount; i++)
			{
				int vindex = deformerData.useVertexIndexList[i];
				if (sw)
				{
					deformer.AddUseVertex(vindex, false);
				}
				else
				{
					deformer.RemoveUseVertex(vindex, false);
				}
			}
		}

		public override int GetVersion()
		{
			return 5;
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
			springDataHash = SpringData.SaveDataHash;
			springDataVersion = SpringData.SaveDataVersion;
		}

		public override Define.Error VerifyData()
		{
			Define.Error error = base.VerifyData();
			if (error != 0)
			{
				return error;
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
			if (centerTransform == null)
			{
				return Define.Error.CenterTransformNull;
			}
			SpringData springData = SpringData;
			if (springData == null)
			{
				return Define.Error.SpringDataNull;
			}
			Define.Error error3 = springData.VerifyData();
			if (error3 != 0)
			{
				return error3;
			}
			if (springDataHash != springData.SaveDataHash)
			{
				return Define.Error.SpringDataHashMismatch;
			}
			if (springDataVersion != springData.SaveDataVersion)
			{
				return Define.Error.SpringDataVersionMismatch;
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
				StaticStringBuilder.Append("Use Deformer Vertex: ", UseVertexCount);
				break;
			case Define.Error.EmptyData:
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			default:
				StaticStringBuilder.AppendLine("This mesh spring is in a state error!");
				if (Application.isPlaying)
				{
					StaticStringBuilder.AppendLine("Execution stopped.");
				}
				else
				{
					StaticStringBuilder.AppendLine("Please recreate the mesh spring data.");
				}
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			}
			return StaticStringBuilder.ToString();
		}

		public void VerifyDeformer()
		{
		}

		public override void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
			if ((bool)centerTransform)
			{
				centerTransform = MeshUtility.GetReplaceBone(centerTransform, boneReplaceDict);
			}
		}

		public override int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector3>();
			Transform transform = CenterTransform;
			if (transform == null)
			{
				return 0;
			}
			wposList.Add(transform.position);
			wnorList.Add(transform.forward);
			Vector3 up = transform.up;
			wtanList.Add(up);
			return 1;
		}

		public override List<int> GetEditorTriangleList()
		{
			return null;
		}

		public override List<int> GetEditorLineList()
		{
			return null;
		}

		public override List<int> GetSelectionList()
		{
			return null;
		}

		public override List<int> GetUseList()
		{
			return null;
		}

		public override List<ShareDataObject> GetAllShareDataObject()
		{
			List<ShareDataObject> allShareDataObject = base.GetAllShareDataObject();
			allShareDataObject.Add(SpringData);
			return allShareDataObject;
		}

		public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
		{
			ShareDataObject shareDataObject = base.DuplicateShareDataObject(source);
			if (shareDataObject != null)
			{
				return shareDataObject;
			}
			if (SpringData == source)
			{
				springData = ShareDataObject.Clone(SpringData);
				return springData;
			}
			return null;
		}

		private void ResetParams()
		{
			clothParams.SetRadius(0.02f, 0.02f);
			clothParams.SetMass(1f, 1f, false);
			clothParams.SetGravity(false);
			clothParams.SetDrag(true, 0.01f, 0.01f);
			clothParams.SetMaxVelocity(true);
			clothParams.SetWorldInfluence(10f, 0.5f, 0.5f);
			clothParams.SetTeleport(false);
			clothParams.SetClampDistanceRatio(false);
			clothParams.SetClampPositionLength(true, 0.1f, 0.1f);
			clothParams.SetClampRotationAngle(false);
			clothParams.SetRestoreDistance();
			clothParams.SetRestoreRotation(false);
			clothParams.SetSpring(true, 0.02f, 0.14f);
			clothParams.SetSpringDirectionAtten(1f, 0f, 0.6f);
			clothParams.SetSpringDistanceAtten(1f, 0f, 0.4f);
			clothParams.SetAdjustRotation(ClothParams.AdjustMode.Fixed, 5f);
			clothParams.SetTriangleBend(false);
			clothParams.SetVolume(false);
			clothParams.SetCollision(false);
			clothParams.SetExternalForce(0.2f, 0f, 0f);
		}
	}
}
