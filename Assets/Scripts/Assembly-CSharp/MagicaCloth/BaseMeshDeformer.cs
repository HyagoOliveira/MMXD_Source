using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public abstract class BaseMeshDeformer : IEditorMesh, IDataVerify, IDataHash
	{
		[SerializeField]
		private MeshData meshData;

		[SerializeField]
		private GameObject targetObject;

		[SerializeField]
		protected int dataHash;

		[SerializeField]
		protected int dataVersion;

		protected RuntimeStatus status = new RuntimeStatus();

		private MonoBehaviour parent;

		public MonoBehaviour Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}

		public virtual MeshData MeshData
		{
			get
			{
				return meshData;
			}
			set
			{
				meshData = value;
			}
		}

		public GameObject TargetObject
		{
			get
			{
				return targetObject;
			}
			set
			{
				targetObject = value;
			}
		}

		public RuntimeStatus Status
		{
			get
			{
				return status;
			}
		}

		public int MeshIndex { get; protected set; } = -1;


		public int VertexCount { get; protected set; }

		public int SkinningVertexCount { get; protected set; }

		public int TriangleCount { get; protected set; }

		public bool IsSkinning
		{
			get
			{
				if (MeshData != null)
				{
					return MeshData.isSkinning;
				}
				return false;
			}
		}

		public int BoneCount
		{
			get
			{
				if (MeshData != null)
				{
					if (MeshData.isSkinning)
					{
						return MeshData.BoneCount;
					}
					return 1;
				}
				return 0;
			}
		}

		public int SaveDataHash
		{
			get
			{
				return dataHash;
			}
		}

		public int SaveDataVersion
		{
			get
			{
				return dataVersion;
			}
		}

		public virtual void Init()
		{
			status.updateStatusAction = OnUpdateStatus;
			if (!status.IsInitComplete && !status.IsInitStart)
			{
				status.SetInitStart();
				OnInit();
				if (VerifyData() != 0)
				{
					status.SetInitError();
					return;
				}
				status.SetInitComplete();
				status.UpdateStatus();
			}
		}

		protected virtual void OnInit()
		{
			MeshIndex = -1;
			CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.AddMesh(this);
		}

		public virtual void Dispose()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Mesh.RemoveMesh(this);
			}
			status.SetDispose();
		}

		public virtual void OnEnable()
		{
			status.SetEnable(true);
			status.UpdateStatus();
		}

		public virtual void OnDisable()
		{
			status.SetEnable(false);
			status.UpdateStatus();
		}

		public virtual void Update()
		{
			bool runtimeError = VerifyData() != Define.Error.None;
			status.SetRuntimeError(runtimeError);
			status.UpdateStatus();
		}

		public abstract void Finish(int bufferIndex);

		protected void OnUpdateStatus()
		{
			if (status.IsActive)
			{
				OnActive();
			}
			else
			{
				OnInactive();
			}
		}

		protected virtual void OnActive()
		{
		}

		protected virtual void OnInactive()
		{
		}

		public virtual bool IsMeshUse()
		{
			return false;
		}

		public virtual bool IsActiveMesh()
		{
			return false;
		}

		public virtual void AddUseMesh(object parent)
		{
		}

		public virtual void RemoveUseMesh(object parent)
		{
		}

		public virtual bool AddUseVertex(int vindex, bool fix)
		{
			return false;
		}

		public virtual bool RemoveUseVertex(int vindex, bool fix)
		{
			return false;
		}

		public virtual void ResetFuturePrediction()
		{
		}

		public virtual int GetDataHash()
		{
			int num = 0;
			if (MeshData != null)
			{
				num += MeshData.GetDataHash();
			}
			if ((bool)targetObject)
			{
				num += targetObject.GetDataHash();
			}
			return num;
		}

		public abstract int GetVersion();

		public virtual Define.Error VerifyData()
		{
			if (dataVersion == 0)
			{
				return Define.Error.EmptyData;
			}
			if (dataHash == 0)
			{
				return Define.Error.InvalidDataHash;
			}
			if (MeshData == null)
			{
				return Define.Error.MeshDataNull;
			}
			if (targetObject == null)
			{
				return Define.Error.TargetObjectNull;
			}
			Define.Error error = MeshData.VerifyData();
			if (error != 0)
			{
				return error;
			}
			return Define.Error.None;
		}

		public virtual void CreateVerifyData()
		{
			dataHash = GetDataHash();
			dataVersion = GetVersion();
		}

		public virtual string GetInformation()
		{
			return "No information.";
		}

		public abstract int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList);

		public abstract List<int> GetEditorTriangleList();

		public abstract List<int> GetEditorLineList();
	}
}
