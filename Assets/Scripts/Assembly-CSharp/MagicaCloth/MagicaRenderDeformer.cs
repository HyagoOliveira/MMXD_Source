using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/magica-cloth-render-deformer/")]
	[AddComponentMenu("MagicaCloth/MagicaRenderDeformer")]
	public class MagicaRenderDeformer : CoreComponent
	{
		private const int DATA_VERSION = 2;

		private const int ERR_DATA_VERSION = 0;

		[SerializeField]
		private RenderMeshDeformer deformer = new RenderMeshDeformer();

		[SerializeField]
		private int deformerHash;

		[SerializeField]
		private int deformerVersion;

		public RenderMeshDeformer Deformer
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

		private void Reset()
		{
		}

		private void OnValidate()
		{
			Deformer.OnValidate();
		}

		protected override void OnInit()
		{
			Deformer.Init();
		}

		protected override void OnDispose()
		{
			Deformer.Dispose();
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
			if (Deformer == null)
			{
				return Define.Error.DeformerNull;
			}
			Define.Error error2 = Deformer.VerifyData();
			if (error2 != 0)
			{
				return error2;
			}
			if (deformerHash != Deformer.SaveDataHash)
			{
				return Define.Error.DeformerHashMismatch;
			}
			if (deformerVersion != Deformer.SaveDataVersion)
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
