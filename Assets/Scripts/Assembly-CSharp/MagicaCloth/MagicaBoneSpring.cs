using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/magica-cloth-bone-spring/")]
	[AddComponentMenu("MagicaCloth/MagicaBoneSpring")]
	public class MagicaBoneSpring : BaseCloth
	{
		private const int DATA_VERSION = 5;

		private const int ERR_DATA_VERSION = 3;

		[SerializeField]
		private MeshData meshData;

		[SerializeField]
		private int meshDataHash;

		[SerializeField]
		private int meshDataVersion;

		[SerializeField]
		private BoneClothTarget clothTarget = new BoneClothTarget();

		[SerializeField]
		private List<Transform> useTransformList = new List<Transform>();

		[SerializeField]
		private List<Vector3> useTransformPositionList = new List<Vector3>();

		[SerializeField]
		private List<Quaternion> useTransformRotationList = new List<Quaternion>();

		[SerializeField]
		private List<Vector3> useTransformScaleList = new List<Vector3>();

		public BoneClothTarget ClothTarget
		{
			get
			{
				return clothTarget;
			}
		}

		public MeshData MeshData
		{
			get
			{
				return meshData;
			}
		}

		private int UseTransformCount
		{
			get
			{
				return useTransformList.Count;
			}
		}

		public override int GetDataHash()
		{
			return base.GetDataHash() + MeshData.GetDataHash() + clothTarget.GetDataHash() + useTransformList.GetDataHash() + useTransformPositionList.GetDataHash() + useTransformRotationList.GetDataHash() + useTransformScaleList.GetDataHash();
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
			ClothTarget.AddParentTransform();
			base.ClothInit();
			CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetFlag(base.TeamId, 16u, true);
		}

		protected override void ClothDispose()
		{
			ClothTarget.RemoveParentTransform();
			base.ClothDispose();
		}

		protected override void ClothActive()
		{
			base.ClothActive();
			if (CreateSingleton<MagicaPhysicsManager>.Instance.IsDelay && base.ActiveCount > 1)
			{
				ClothTarget.ResetFuturePredictionParentTransform();
			}
		}

		protected override uint UserFlag(int index)
		{
			bool flag = base.ClothData.IsFixedVertex(index);
			return 0x40000u | (flag ? 12288u : 0u) | 0x4000u | 0x20000u | 0x80000u;
		}

		protected override Transform UserTransform(int index)
		{
			return GetUseTransform(index);
		}

		protected override float3 UserTransformLocalPosition(int vindex)
		{
			int index = base.ClothData.useVertexList[vindex];
			return useTransformPositionList[index];
		}

		protected override quaternion UserTransformLocalRotation(int vindex)
		{
			int index = base.ClothData.useVertexList[vindex];
			return useTransformRotationList[index];
		}

		public override int GetDeformerCount()
		{
			return 0;
		}

		public override BaseMeshDeformer GetDeformer(int index)
		{
			return null;
		}

		protected override MeshData GetMeshData()
		{
			return MeshData;
		}

		protected override void WorkerInit()
		{
		}

		protected override void SetDeformerUseVertex(bool sw, BaseMeshDeformer deformer, int deformerIndex)
		{
		}

		public List<Transform> GetTransformList()
		{
			List<Transform> list = new List<Transform>();
			int rootCount = clothTarget.RootCount;
			for (int i = 0; i < rootCount; i++)
			{
				Transform root = clothTarget.GetRoot(i);
				if (root != null)
				{
					list.Add(root);
				}
			}
			return list;
		}

		private Transform GetUseTransform(int index)
		{
			int index2 = base.ClothData.useVertexList[index];
			return useTransformList[index2];
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
			meshDataHash = MeshData.SaveDataHash;
			meshDataVersion = MeshData.SaveDataVersion;
		}

		public override Define.Error VerifyData()
		{
			Define.Error error = base.VerifyData();
			if (error != 0)
			{
				return error;
			}
			MeshData meshData = MeshData;
			if (meshData == null)
			{
				return Define.Error.MeshDataNull;
			}
			Define.Error error2 = meshData.VerifyData();
			if (error2 != 0)
			{
				return error2;
			}
			if (meshDataHash != meshData.SaveDataHash)
			{
				return Define.Error.MeshDataHashMismatch;
			}
			if (meshDataVersion != meshData.SaveDataVersion)
			{
				return Define.Error.MeshDataVersionMismatch;
			}
			if (useTransformList.Count == 0)
			{
				return Define.Error.UseTransformCountZero;
			}
			if (UseTransformCount != meshData.VertexCount)
			{
				return Define.Error.UseTransformCountMismatch;
			}
			if (clothTarget.RootCount != meshData.VertexCount)
			{
				return Define.Error.ClothTargetRootCountMismatch;
			}
			if (useTransformPositionList.Count != useTransformList.Count)
			{
				return Define.Error.UseTransformCountMismatch;
			}
			if (useTransformRotationList.Count != useTransformList.Count)
			{
				return Define.Error.UseTransformCountMismatch;
			}
			if (useTransformScaleList.Count != useTransformList.Count)
			{
				return Define.Error.UseTransformCountMismatch;
			}
			foreach (Transform useTransform in useTransformList)
			{
				if (useTransform == null)
				{
					return Define.Error.UseTransformNull;
				}
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
				StaticStringBuilder.AppendLine("Transform: ", MeshData.VertexCount);
				StaticStringBuilder.AppendLine("Clamp Position: ", clothParams.UseClampPositionLength ? clothData.VertexUseCount : 0);
				StaticStringBuilder.AppendLine("Spring: ", clothParams.UseSpring ? clothData.VertexUseCount : 0);
				StaticStringBuilder.Append("Adjust Rotation: ", clothData.VertexUseCount);
				break;
			}
			case Define.Error.EmptyData:
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			default:
				StaticStringBuilder.AppendLine("This bone spring is in a state error!");
				if (Application.isPlaying)
				{
					StaticStringBuilder.AppendLine("Execution stopped.");
				}
				else
				{
					StaticStringBuilder.AppendLine("Please recreate the bone spring data.");
				}
				StaticStringBuilder.Append(Define.GetErrorMessage(error));
				break;
			}
			return StaticStringBuilder.ToString();
		}

		public override void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
			base.ReplaceBone(boneReplaceDict);
			for (int i = 0; i < useTransformList.Count; i++)
			{
				useTransformList[i] = MeshUtility.GetReplaceBone(useTransformList[i], boneReplaceDict);
			}
			clothTarget.ReplaceBone(boneReplaceDict);
		}

		public override int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
		{
			wposList = new List<Vector3>();
			wnorList = new List<Vector3>();
			wtanList = new List<Vector3>();
			foreach (Transform transform in GetTransformList())
			{
				wposList.Add(transform.position);
				wnorList.Add(transform.TransformDirection(Vector3.forward));
				Vector3 item = transform.TransformDirection(Vector3.up);
				wtanList.Add(item);
			}
			return wposList.Count;
		}

		public override List<int> GetEditorTriangleList()
		{
			List<int> result = new List<int>();
			MeshData meshData = MeshData;
			if (meshData != null && meshData.triangleList != null)
			{
				result = new List<int>(meshData.triangleList);
			}
			return result;
		}

		public override List<int> GetEditorLineList()
		{
			List<int> result = new List<int>();
			MeshData meshData = MeshData;
			if (meshData != null && meshData.lineList != null)
			{
				result = new List<int>(meshData.lineList);
			}
			return result;
		}

		public override List<int> GetSelectionList()
		{
			if (base.ClothSelection != null && MeshData != null)
			{
				return base.ClothSelection.GetSelectionData(MeshData, null);
			}
			return null;
		}

		public override List<int> GetUseList()
		{
			if (Application.isPlaying && base.ClothData != null)
			{
				List<int> list = new List<int>();
				{
					foreach (int selectionDatum in base.ClothData.selectionData)
					{
						list.Add((selectionDatum != 0) ? 1 : 0);
					}
					return list;
				}
			}
			return null;
		}

		public override List<ShareDataObject> GetAllShareDataObject()
		{
			List<ShareDataObject> allShareDataObject = base.GetAllShareDataObject();
			allShareDataObject.Add(MeshData);
			return allShareDataObject;
		}

		public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
		{
			ShareDataObject shareDataObject = base.DuplicateShareDataObject(source);
			if (shareDataObject != null)
			{
				return shareDataObject;
			}
			if (MeshData == source)
			{
				meshData = ShareDataObject.Clone(MeshData);
				return meshData;
			}
			return null;
		}

		private void ResetParams()
		{
			clothParams.SetRadius(0.02f, 0.02f);
			clothParams.SetMass(1f, 1f, false);
			clothParams.SetGravity(false);
			clothParams.SetDrag(true, 0.03f, 0.03f);
			clothParams.SetMaxVelocity(true);
			clothParams.SetWorldInfluence(10f, 0.5f, 0.5f);
			clothParams.SetTeleport(false);
			clothParams.SetClampDistanceRatio(false);
			clothParams.SetClampPositionLength(true, 0.2f, 0.2f, 1f, 1f, 1f, 1f);
			clothParams.SetClampRotationAngle(false);
			clothParams.SetRestoreDistance();
			clothParams.SetRestoreRotation(false);
			clothParams.SetSpring(true, 0.02f, 0.1f);
			clothParams.SetAdjustRotation(ClothParams.AdjustMode.Fixed, 3f);
			clothParams.SetTriangleBend(false);
			clothParams.SetVolume(false);
			clothParams.SetCollision(false);
			clothParams.SetExternalForce(0.2f, 0f, 0f);
		}
	}
}
