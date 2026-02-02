using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public abstract class BaseCloth : PhysicsTeam
	{
		[SerializeField]
		protected ClothParams clothParams = new ClothParams();

		[SerializeField]
		protected List<int> clothParamDataHashList = new List<int>();

		[SerializeField]
		private ClothData clothData;

		[SerializeField]
		protected int clothDataHash;

		[SerializeField]
		protected int clothDataVersion;

		[SerializeField]
		private SelectionData clothSelection;

		[SerializeField]
		private int clothSelectionHash;

		[SerializeField]
		private int clothSelectionVersion;

		protected ClothSetup setup = new ClothSetup();

		private float oldBlendRatio = -1f;

		public float BlendWeight
		{
			get
			{
				return base.UserBlendWeight;
			}
			set
			{
				base.UserBlendWeight = value;
			}
		}

		public bool DistanceDisable_Active
		{
			get
			{
				return clothParams.UseDistanceDisable;
			}
			set
			{
				clothParams.UseDistanceDisable = value;
			}
		}

		public Transform DistanceDisable_ReferenceObject
		{
			get
			{
				return clothParams.DisableReferenceObject;
			}
			set
			{
				clothParams.DisableReferenceObject = value;
			}
		}

		public float DistanceDisable_Distance
		{
			get
			{
				return clothParams.DisableDistance;
			}
			set
			{
				clothParams.DisableDistance = Mathf.Max(value, 0f);
			}
		}

		public float DistanceDisable_FadeDistance
		{
			get
			{
				return clothParams.DisableFadeDistance;
			}
			set
			{
				clothParams.DisableFadeDistance = Mathf.Max(value, 0f);
			}
		}

		public float ExternalForce_MassInfluence
		{
			get
			{
				return clothParams.MassInfluence;
			}
			set
			{
				clothParams.MassInfluence = value;
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetExternalForce(base.TeamId, clothParams.MassInfluence, clothParams.WindInfluence, clothParams.WindRandomScale);
				}
			}
		}

		public float ExternalForce_WindInfluence
		{
			get
			{
				return clothParams.WindInfluence;
			}
			set
			{
				clothParams.WindInfluence = value;
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetExternalForce(base.TeamId, clothParams.MassInfluence, clothParams.WindInfluence, clothParams.WindRandomScale);
				}
			}
		}

		public float ExternalForce_WindRandomScale
		{
			get
			{
				return clothParams.WindRandomScale;
			}
			set
			{
				clothParams.WindRandomScale = value;
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetExternalForce(base.TeamId, clothParams.MassInfluence, clothParams.WindInfluence, clothParams.WindRandomScale);
				}
			}
		}

		public float WorldInfluence_MaxMoveSpeed
		{
			get
			{
				return clothParams.MaxMoveSpeed;
			}
			set
			{
				clothParams.MaxMoveSpeed = Mathf.Max(value, 0f);
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetWorldInfluence(base.TeamId, clothParams.MaxMoveSpeed, clothParams.GetWorldMoveInfluence(), clothParams.GetWorldRotationInfluence());
				}
			}
		}

		public bool WorldInfluence_ResetAfterTeleport
		{
			get
			{
				return clothParams.UseResetTeleport;
			}
			set
			{
				clothParams.UseResetTeleport = value;
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetAfterTeleport(base.TeamId, clothParams.UseResetTeleport, clothParams.TeleportDistance, clothParams.TeleportRotation);
				}
			}
		}

		public float WorldInfluence_TeleportDistance
		{
			get
			{
				return clothParams.TeleportDistance;
			}
			set
			{
				clothParams.TeleportDistance = value;
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetAfterTeleport(base.TeamId, clothParams.UseResetTeleport, clothParams.TeleportDistance, clothParams.TeleportRotation);
				}
			}
		}

		public float WorldInfluence_TeleportRotation
		{
			get
			{
				return clothParams.TeleportRotation;
			}
			set
			{
				clothParams.TeleportRotation = value;
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetAfterTeleport(base.TeamId, clothParams.UseResetTeleport, clothParams.TeleportDistance, clothParams.TeleportRotation);
				}
			}
		}

		public float WorldInfluence_StabilizationTime
		{
			get
			{
				return clothParams.ResetStabilizationTime;
			}
			set
			{
				clothParams.ResetStabilizationTime = Mathf.Max(value, 0f);
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetStabilizationTime(base.TeamId, clothParams.ResetStabilizationTime);
				}
			}
		}

		public bool ColliderCollision_Active
		{
			get
			{
				return clothParams.UseCollision;
			}
			set
			{
				clothParams.SetCollision(value, clothParams.Friction);
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetFlag(base.TeamId, 262144u, clothParams.KeepInitialShape);
					CreateSingleton<MagicaPhysicsManager>.Instance.Compute.Collision.ChangeParam(base.TeamId, clothParams.UseCollision);
				}
			}
		}

		public bool Penetration_Active
		{
			get
			{
				return clothParams.UsePenetration;
			}
			set
			{
				clothParams.UsePenetration = value;
				if (IsValid())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Compute.Penetration.ChangeParam(base.TeamId, clothParams.UsePenetration, clothParams.GetPenetrationDistance(), clothParams.GetPenetrationRadius());
				}
			}
		}

		public ClothParams Params
		{
			get
			{
				return clothParams;
			}
		}

		public ClothData ClothData
		{
			get
			{
				return clothData;
			}
			set
			{
				clothData = value;
			}
		}

		public SelectionData ClothSelection
		{
			get
			{
				return clothSelection;
			}
		}

		public ClothSetup Setup
		{
			get
			{
				return setup;
			}
		}

		public void ResetCloth()
		{
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetFlag(teamId, 65536u, true);
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetFlag(teamId, 131072u, true);
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.ResetStabilizationTime(teamId);
			}
		}

		public void ResetCloth(float resetStabilizationTime)
		{
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetFlag(teamId, 65536u, true);
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetFlag(teamId, 131072u, true);
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.ResetStabilizationTime(teamId, Mathf.Max(resetStabilizationTime, 0f));
			}
		}

		public void SetTimeScale(float timeScale)
		{
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetTimeScale(teamId, Mathf.Clamp01(timeScale));
			}
		}

		public float GetTimeScale()
		{
			if (IsValid())
			{
				return CreateSingleton<MagicaPhysicsManager>.Instance.Team.GetTimeScale(teamId);
			}
			return 1f;
		}

		public void AddForce(Vector3 force, PhysicsManagerTeamData.ForceMode mode)
		{
			if (IsValid() && IsActive())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetImpactForce(teamId, force, mode);
			}
		}

		public void AddCollider(ColliderComponent collider)
		{
			Init();
			if (IsValid() && (bool)collider)
			{
				collider.CreateColliderParticle(teamId);
				base.TeamData.AddCollider(collider);
			}
		}

		public void RemoveCollider(ColliderComponent collider)
		{
			if (IsValid() && (bool)collider)
			{
				collider.RemoveColliderParticle(teamId);
				base.TeamData.RemoveCollider(collider);
			}
		}

		public void Radius_SetRadius(float startVal, float endVal, float curveVal = 0f)
		{
			BezierParam bezierParam = clothParams.GetRadius().AutoSetup(Mathf.Max(startVal, 0.001f), Mathf.Max(endVal, 0.001f), curveVal);
			MagicaPhysicsManager instance = CreateSingleton<MagicaPhysicsManager>.Instance;
			for (int i = 0; i < base.ParticleChunk.dataLength; i++)
			{
				int index = base.ParticleChunk.startIndex + i;
				float x = instance.Particle.depthList[index];
				float num = bezierParam.Evaluate(x);
				instance.Particle.SetRadius(index, num);
			}
		}

		public void Mass_SetMass(float startVal, float endVal, float curveVal = 0f)
		{
			BezierParam mass = clothParams.GetMass().AutoSetup(Mathf.Max(startVal, 1f), Mathf.Max(endVal, 1f), curveVal);
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetMass(base.TeamId, mass);
				CreateSingleton<MagicaPhysicsManager>.Instance.Compute.RestoreDistance.ChangeParam(base.TeamId, clothParams.GetMass(), clothParams.RestoreDistanceVelocityInfluence, clothParams.GetStructDistanceStiffness(), clothParams.UseBendDistance, clothParams.GetBendDistanceStiffness(), clothParams.UseNearDistance, clothParams.GetNearDistanceStiffness());
			}
		}

		public void Gravity_SetGravity(float startVal, float endVal, float curveVal = 0f)
		{
			BezierParam gravity = clothParams.GetGravity().AutoSetup(startVal, endVal, curveVal);
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetGravity(base.TeamId, gravity);
			}
		}

		public void Drag_SetDrag(float startVal, float endVal, float curveVal = 0f)
		{
			BezierParam drag = clothParams.GetDrag().AutoSetup(startVal, endVal, curveVal);
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetDrag(base.TeamId, drag);
			}
		}

		public void WorldInfluence_SetMovementInfluence(float startVal, float endVal, float curveVal = 0f)
		{
			BezierParam moveInfluence = clothParams.GetWorldMoveInfluence().AutoSetup(startVal, endVal, curveVal);
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetWorldInfluence(base.TeamId, clothParams.MaxMoveSpeed, moveInfluence, clothParams.GetWorldRotationInfluence());
			}
		}

		public void WorldInfluence_SetRotationInfluence(float startVal, float endVal, float curveVal = 0f)
		{
			BezierParam rotInfluence = clothParams.GetWorldRotationInfluence().AutoSetup(startVal, endVal, curveVal);
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetWorldInfluence(base.TeamId, clothParams.MaxMoveSpeed, clothParams.GetWorldMoveInfluence(), rotInfluence);
			}
		}

		public void Penetration_SetMovingRadius(float startVal, float endVal, float curveVal = 0f)
		{
			clothParams.GetPenetrationRadius().AutoSetup(Mathf.Max(startVal, 0f), Mathf.Max(endVal, 0f), curveVal);
			if (IsValid())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Compute.Penetration.ChangeParam(base.TeamId, clothParams.UsePenetration, clothParams.GetPenetrationDistance(), clothParams.GetPenetrationRadius());
			}
		}

		public override int GetDataHash()
		{
			int num = base.GetDataHash();
			if (ClothData != null)
			{
				num += ClothData.GetDataHash();
			}
			if (ClothSelection != null)
			{
				num += ClothSelection.GetDataHash();
			}
			return num;
		}

		protected virtual void Reset()
		{
		}

		protected virtual void OnValidate()
		{
			if (Application.isPlaying)
			{
				setup.ChangeData(this, clothParams);
			}
		}

		protected override void OnInit()
		{
			base.OnInit();
			BaseClothInit();
		}

		protected override void OnActive()
		{
			base.OnActive();
			EnableParticle(UserTransform, UserTransformLocalPosition, UserTransformLocalRotation);
			SetUseMesh(true);
			ClothActive();
		}

		protected override void OnInactive()
		{
			base.OnInactive();
			DisableParticle(UserTransform, UserTransformLocalPosition, UserTransformLocalRotation);
			SetUseMesh(false);
			ClothInactive();
		}

		protected override void OnDispose()
		{
			BaseClothDispose();
			base.OnDispose();
		}

		private void BaseClothInit()
		{
			int deformerCount = GetDeformerCount();
			for (int i = 0; i < deformerCount; i++)
			{
				BaseMeshDeformer deformer = GetDeformer(i);
				if (deformer == null)
				{
					base.Status.SetInitError();
					return;
				}
				CoreComponent coreComponent = deformer.Parent as CoreComponent;
				base.Status.LinkParentStatus(coreComponent.Status);
				coreComponent.Init();
				if (coreComponent.Status.IsInitError)
				{
					base.Status.SetInitError();
					return;
				}
			}
			if (VerifyData() != 0)
			{
				base.Status.SetInitError();
				return;
			}
			ClothInit();
			WorkerInit();
			SetUseVertex(true);
		}

		private void BaseClothDispose()
		{
			if (!CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				return;
			}
			int deformerCount = GetDeformerCount();
			for (int i = 0; i < deformerCount; i++)
			{
				BaseMeshDeformer deformer = GetDeformer(i);
				if (deformer != null)
				{
					CoreComponent coreComponent = deformer.Parent as CoreComponent;
					base.Status.UnlinkParentStatus(coreComponent.Status);
				}
			}
			if (base.Status.IsInitSuccess)
			{
				SetUseVertex(false);
				setup.ClothDispose(this);
				ClothDispose();
			}
		}

		protected virtual void ClothInit()
		{
			setup.ClothInit(this, GetMeshData(), ClothData, clothParams, UserFlag);
		}

		protected virtual void ClothActive()
		{
			setup.ClothActive(this, clothParams, ClothData);
			if (!CreateSingleton<MagicaPhysicsManager>.Instance.IsDelay || base.ActiveCount <= 1)
			{
				return;
			}
			int deformerCount = GetDeformerCount();
			for (int i = 0; i < deformerCount; i++)
			{
				BaseMeshDeformer deformer = GetDeformer(i);
				if (deformer != null)
				{
					deformer.ResetFuturePrediction();
				}
			}
		}

		protected virtual void ClothInactive()
		{
			setup.ClothInactive(this);
		}

		protected virtual void ClothDispose()
		{
		}

		protected abstract uint UserFlag(int vindex);

		protected abstract Transform UserTransform(int vindex);

		protected abstract float3 UserTransformLocalPosition(int vindex);

		protected abstract quaternion UserTransformLocalRotation(int vindex);

		public abstract int GetDeformerCount();

		public abstract BaseMeshDeformer GetDeformer(int index);

		protected abstract MeshData GetMeshData();

		protected abstract void WorkerInit();

		private void SetUseMesh(bool sw)
		{
			if (!CreateSingleton<MagicaPhysicsManager>.IsInstance() || !base.Status.IsInitSuccess)
			{
				return;
			}
			int deformerCount = GetDeformerCount();
			for (int i = 0; i < deformerCount; i++)
			{
				BaseMeshDeformer deformer = GetDeformer(i);
				if (deformer != null)
				{
					if (sw)
					{
						deformer.AddUseMesh(this);
					}
					else
					{
						deformer.RemoveUseMesh(this);
					}
				}
			}
		}

		private void SetUseVertex(bool sw)
		{
			if (!CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				return;
			}
			int deformerCount = GetDeformerCount();
			for (int i = 0; i < deformerCount; i++)
			{
				BaseMeshDeformer deformer = GetDeformer(i);
				if (deformer != null)
				{
					SetDeformerUseVertex(sw, deformer, i);
				}
			}
		}

		protected abstract void SetDeformerUseVertex(bool sw, BaseMeshDeformer deformer, int deformerIndex);

		public void UpdateBlend()
		{
			if (teamId > 0)
			{
				float num = base.UserBlendWeight;
				num *= setup.DistanceBlendRatio;
				num = Mathf.Clamp01(num);
				if (num != oldBlendRatio)
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetBlendRatio(teamId, num);
					SetUserEnable(num > 0.01f);
					oldBlendRatio = num;
				}
			}
		}

		public override void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
			base.ReplaceBone(boneReplaceDict);
			setup.ReplaceBone(this, clothParams, boneReplaceDict);
		}

		public override void CreateVerifyData()
		{
			base.CreateVerifyData();
			clothDataHash = ((ClothData != null) ? ClothData.SaveDataHash : 0);
			clothDataVersion = ((ClothData != null) ? ClothData.SaveDataVersion : 0);
			clothSelectionHash = ((ClothSelection != null) ? ClothSelection.SaveDataHash : 0);
			clothSelectionVersion = ((ClothSelection != null) ? ClothSelection.SaveDataVersion : 0);
			clothParamDataHashList.Clear();
			for (int i = 0; i < 21; i++)
			{
				int paramHash = clothParams.GetParamHash(this, (ClothParams.ParamType)i);
				clothParamDataHashList.Add(paramHash);
			}
		}

		public override Define.Error VerifyData()
		{
			Define.Error error = base.VerifyData();
			if (error != 0)
			{
				return error;
			}
			if (ClothData != null)
			{
				Define.Error error2 = ClothData.VerifyData();
				if (error2 != 0)
				{
					return error2;
				}
				if (clothDataHash != ClothData.SaveDataHash)
				{
					return Define.Error.ClothDataHashMismatch;
				}
				if (clothDataVersion != ClothData.SaveDataVersion)
				{
					return Define.Error.ClothDataVersionMismatch;
				}
			}
			if (ClothSelection != null)
			{
				Define.Error error3 = ClothSelection.VerifyData();
				if (error3 != 0)
				{
					return error3;
				}
				if (clothSelectionHash != ClothSelection.SaveDataHash)
				{
					return Define.Error.ClothSelectionHashMismatch;
				}
				if (clothSelectionVersion != ClothSelection.SaveDataVersion)
				{
					return Define.Error.ClothSelectionVersionMismatch;
				}
			}
			return Define.Error.None;
		}

		public bool HasChangedParam(ClothParams.ParamType ptype)
		{
			if ((int)ptype >= clothParamDataHashList.Count)
			{
				return false;
			}
			int paramHash = clothParams.GetParamHash(this, ptype);
			if (paramHash == 0)
			{
				return false;
			}
			return clothParamDataHashList[(int)ptype] != paramHash;
		}

		public override List<ShareDataObject> GetAllShareDataObject()
		{
			List<ShareDataObject> allShareDataObject = base.GetAllShareDataObject();
			allShareDataObject.Add(ClothData);
			allShareDataObject.Add(ClothSelection);
			return allShareDataObject;
		}

		public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
		{
			if (ClothData == source)
			{
				clothData = ShareDataObject.Clone(ClothData);
				return clothData;
			}
			if (ClothSelection == source)
			{
				clothSelection = ShareDataObject.Clone(ClothSelection);
				return clothSelection;
			}
			return null;
		}
	}
}
