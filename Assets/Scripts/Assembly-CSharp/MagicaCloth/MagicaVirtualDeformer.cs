using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/magica-cloth-virtual-deformer/")]
	[AddComponentMenu("MagicaCloth/MagicaVirtualDeformer")]
	public class MagicaVirtualDeformer : CoreComponent
	{
		private const int DATA_VERSION = 2;

		private const int ERR_DATA_VERSION = 0;

		[SerializeField]
		private VirtualMeshDeformer deformer = new VirtualMeshDeformer();

		[SerializeField]
		private int deformerHash;

		[SerializeField]
		private int deformerVersion;

		public VirtualMeshDeformer Deformer
		{
			get
			{
				deformer.Parent = this;
				return deformer;
			}
		}

		public override int GetDataHash()
		{
			return 0 + Deformer.GetDataHash();
		}

		private void OnValidate()
		{
		}

		protected override void OnInit()
		{
			LinkRenderDeformerStatus(true);
			Deformer.Init();
		}

		protected override void OnDispose()
		{
			Deformer.Dispose();
			LinkRenderDeformerStatus(false);
		}

		protected override void OnUpdate()
		{
			Deformer.Update();
		}

		protected override void OnActive()
		{
			Deformer.OnEnable();
		}

		protected override void OnInactive()
		{
			Deformer.OnDisable();
		}

		private void LinkRenderDeformerStatus(bool sw)
		{
			int renderDeformerCount = Deformer.RenderDeformerCount;
			for (int i = 0; i < renderDeformerCount; i++)
			{
				MagicaRenderDeformer renderDeformer = Deformer.GetRenderDeformer(i);
				if (renderDeformer != null)
				{
					if (sw)
					{
						renderDeformer.Status.LinkParentStatus(status);
					}
					else
					{
						renderDeformer.Status.UnlinkParentStatus(status);
					}
				}
			}
		}

		public override int GetVersion()
		{
			return 2;
		}

		public override int GetErrorVersion()
		{
			return 0;
		}

		public override void CreateVerifyData()
		{
			base.CreateVerifyData();
			deformerHash = Deformer.SaveDataHash;
			deformerVersion = Deformer.SaveDataVersion;
		}

		public override Define.Error VerifyData()
		{
			Define.Error error = base.VerifyData();
			if (error != 0)
			{
				return error;
			}
			VirtualMeshDeformer virtualMeshDeformer = Deformer;
			if (virtualMeshDeformer == null)
			{
				return Define.Error.DeformerNull;
			}
			Define.Error error2 = virtualMeshDeformer.VerifyData();
			if (error2 != 0)
			{
				return error2;
			}
			if (deformerHash != virtualMeshDeformer.SaveDataHash)
			{
				return Define.Error.DeformerHashMismatch;
			}
			if (deformerVersion != virtualMeshDeformer.SaveDataVersion)
			{
				return Define.Error.DeformerVersionMismatch;
			}
			return Define.Error.None;
		}

		public override string GetInformation()
		{
			if (Deformer != null)
			{
				return Deformer.GetInformation();
			}
			return base.GetInformation();
		}

		public override void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
			base.ReplaceBone(boneReplaceDict);
			Deformer.ReplaceBone(boneReplaceDict);
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

		public override List<int> GetUseList()
		{
			if (Application.isPlaying)
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
			allShareDataObject.Add(Deformer.MeshData);
			return allShareDataObject;
		}

		public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
		{
			if (Deformer.MeshData == source)
			{
				Deformer.MeshData = ShareDataObject.Clone(Deformer.MeshData);
				return Deformer.MeshData;
			}
			return null;
		}
	}
}
