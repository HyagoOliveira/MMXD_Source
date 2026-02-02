using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	public abstract class CoreComponent : MonoBehaviour, IShareDataObject, IDataVerify, IEditorMesh, IEditorCloth, IDataHash, IBoneReplace
	{
		[SerializeField]
		protected int dataHash;

		[SerializeField]
		protected int dataVersion;

		protected RuntimeStatus status = new RuntimeStatus();

		public RuntimeStatus Status
		{
			get
			{
				return status;
			}
		}

		protected int ActiveCount { get; private set; }

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

		public void ReplaceComponentBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
			ChangeAvatar(boneReplaceDict);
		}

		public abstract int GetDataHash();

		protected virtual void Start()
		{
			Init();
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

		protected virtual void OnDestroy()
		{
			if (!Status.IsDispose)
			{
				status.SetDispose();
				OnDispose();
				if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Component.RemoveComponent(this);
				}
			}
		}

		public void Init()
		{
			status.updateStatusAction = OnUpdateStatus;
			status.disconnectedAction = OnDisconnectedStatus;
			if (status.IsInitComplete || status.IsInitStart)
			{
				return;
			}
			status.SetInitStart();
			CreateSingleton<MagicaPhysicsManager>.Instance.Component.AddComponent(this);
			if (VerifyData() != 0)
			{
				status.SetInitError();
				return;
			}
			OnInit();
			if (!status.IsInitError)
			{
				status.SetInitComplete();
				status.UpdateStatus();
			}
		}

		protected abstract void OnInit();

		protected abstract void OnDispose();

		protected abstract void OnUpdate();

		protected abstract void OnActive();

		protected abstract void OnInactive();

		protected virtual void OnUpdateStatus()
		{
			if (status.IsActive)
			{
				ActiveCount++;
				OnActive();
			}
			else
			{
				OnInactive();
			}
		}

		protected virtual void OnDisconnectedStatus()
		{
			OnDestroy();
		}

		public virtual List<ShareDataObject> GetAllShareDataObject()
		{
			return new List<ShareDataObject>();
		}

		public abstract ShareDataObject DuplicateShareDataObject(ShareDataObject source);

		protected void SetUserEnable(bool sw)
		{
			if (status.SetUserEnable(sw))
			{
				status.UpdateStatus();
			}
		}

		public abstract int GetVersion();

		public abstract int GetErrorVersion();

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
			if (dataVersion > 0 && GetErrorVersion() > 0 && dataVersion <= GetErrorVersion())
			{
				return Define.Error.TooOldDataVersion;
			}
			return Define.Error.None;
		}

		public Define.Error VerityDataVersion()
		{
			if (dataVersion == 0)
			{
				return Define.Error.None;
			}
			if (dataVersion != GetVersion())
			{
				return Define.Error.OldDataVersion;
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

		public void ChangeAvatar(Dictionary<Transform, Transform> boneReplaceDict)
		{
			bool isActive = status.IsActive;
			if (isActive)
			{
				status.SetEnable(false);
				status.UpdateStatus();
			}
			ReplaceBone(boneReplaceDict);
			if (isActive)
			{
				status.SetEnable(true);
				status.UpdateStatus();
			}
		}

		public virtual void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
		}

		public virtual int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
		{
			throw new NotImplementedException();
		}

		public virtual List<int> GetEditorTriangleList()
		{
			throw new NotImplementedException();
		}

		public virtual List<int> GetEditorLineList()
		{
			throw new NotImplementedException();
		}

		public virtual List<int> GetSelectionList()
		{
			return null;
		}

		public virtual List<int> GetUseList()
		{
			return null;
		}
	}
}
