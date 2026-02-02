using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public class ClothParams
	{
		public enum AdjustMode
		{
			Fixed = 0,
			XYMove = 1,
			XZMove = 2,
			YZMove = 3
		}

		public enum PenetrationMode
		{
			SurfacePenetration = 0,
			ColliderPenetration = 1
		}

		public enum PenetrationAxis
		{
			X = 0,
			Y = 1,
			Z = 2,
			InverseX = 3,
			InverseY = 4,
			InverseZ = 5
		}

		public enum ParamType
		{
			Radius = 0,
			Mass = 1,
			Gravity = 2,
			Drag = 3,
			MaxVelocity = 4,
			WorldInfluence = 5,
			ClampDistance = 6,
			ClampPosition = 7,
			ClampRotation = 8,
			RestoreDistance = 9,
			RestoreRotation = 10,
			Spring = 11,
			AdjustRotation = 12,
			AirLine = 13,
			TriangleBend = 14,
			Volume = 15,
			ColliderCollision = 16,
			RotationInterpolation = 17,
			DistanceDisable = 18,
			ExternalForce = 19,
			Penetration = 20,
			Max = 21
		}

		[SerializeField]
		private BezierParam radius = new BezierParam(0.02f, 0.02f, true, 0f, false);

		[SerializeField]
		private BezierParam mass = new BezierParam(1f, 1f, true, 0f, false);

		[SerializeField]
		private bool useGravity = true;

		[SerializeField]
		private BezierParam gravity = new BezierParam(-9.8f, -9.8f, false, 0f, false);

		[SerializeField]
		private bool useDrag = true;

		[SerializeField]
		private BezierParam drag = new BezierParam(0.02f, 0.02f, true, 0f, false);

		[SerializeField]
		private bool useMaxVelocity = true;

		[SerializeField]
		private BezierParam maxVelocity = new BezierParam(3f, 3f, false, 0f, false);

		[SerializeField]
		private Transform influenceTarget;

		[SerializeField]
		private float maxMoveSpeed = 10f;

		[SerializeField]
		private BezierParam worldMoveInfluence = new BezierParam(0.5f, 0.5f, false, 0f, false);

		[SerializeField]
		private BezierParam worldRotationInfluence = new BezierParam(0.5f, 0.5f, false, 0f, false);

		[SerializeField]
		private float massInfluence = 0.3f;

		[SerializeField]
		private float windInfluence = 1f;

		[SerializeField]
		private float windRandomScale = 0.7f;

		[SerializeField]
		private bool useDistanceDisable;

		[SerializeField]
		private Transform disableReferenceObject;

		[SerializeField]
		private float disableDistance = 20f;

		[SerializeField]
		private float disableFadeDistance = 5f;

		[SerializeField]
		private bool useResetTeleport;

		[SerializeField]
		private float teleportDistance = 0.2f;

		[SerializeField]
		private float teleportRotation = 45f;

		[SerializeField]
		private float resetStabilizationTime = 0.1f;

		[SerializeField]
		private bool useClampDistanceRatio = true;

		[SerializeField]
		private float clampDistanceMinRatio = 0.7f;

		[SerializeField]
		private float clampDistanceMaxRatio = 1.1f;

		[SerializeField]
		private float clampDistanceVelocityInfluence = 0.2f;

		[SerializeField]
		private bool useClampPositionLength;

		[SerializeField]
		private BezierParam clampPositionLength = new BezierParam(0.03f, 0.2f, true, 0f, false);

		[SerializeField]
		private float clampPositionRatioX = 1f;

		[SerializeField]
		private float clampPositionRatioY = 1f;

		[SerializeField]
		private float clampPositionRatioZ = 1f;

		[SerializeField]
		private float clampPositionVelocityInfluence = 0.2f;

		[SerializeField]
		private bool useClampRotation;

		[SerializeField]
		private BezierParam clampRotationAngle = new BezierParam(30f, 30f, true, 0f, false);

		[SerializeField]
		private float clampRotationVelocityInfluence = 0.2f;

		[SerializeField]
		private float restoreDistanceVelocityInfluence = 1f;

		[SerializeField]
		private BezierParam structDistanceStiffness = new BezierParam(1f, 1f, false, 0f, false);

		[SerializeField]
		private bool useBendDistance;

		[SerializeField]
		private int bendDistanceMaxCount = 2;

		[SerializeField]
		private BezierParam bendDistanceStiffness = new BezierParam(0.5f, 0.5f, false, 0f, false);

		[SerializeField]
		private bool useNearDistance;

		[SerializeField]
		private int nearDistanceMaxCount = 3;

		[SerializeField]
		private float nearDistanceMaxDepth = 1f;

		[SerializeField]
		private BezierParam nearDistanceLength = new BezierParam(0.1f, 0.1f, true, 0f, false);

		[SerializeField]
		private BezierParam nearDistanceStiffness = new BezierParam(0.3f, 0.3f, false, 0f, false);

		[SerializeField]
		private bool useRestoreRotation;

		[SerializeField]
		private BezierParam restoreRotation = new BezierParam(0.3f, 0.1f, true, 0f, false);

		[SerializeField]
		private float restoreRotationVelocityInfluence = 0.2f;

		[SerializeField]
		private bool useSpring;

		[SerializeField]
		private float springPower = 0.017f;

		[SerializeField]
		private float springRadius = 0.1f;

		[SerializeField]
		private float springScaleX = 1f;

		[SerializeField]
		private float springScaleY = 1f;

		[SerializeField]
		private float springScaleZ = 1f;

		[SerializeField]
		private float springIntensity = 1f;

		[SerializeField]
		private BezierParam springDirectionAtten = new BezierParam(1f, 0f, true, 0.234f, true);

		[SerializeField]
		private BezierParam springDistanceAtten = new BezierParam(1f, 0f, true, 0.395f, true);

		[SerializeField]
		private AdjustMode adjustMode;

		[SerializeField]
		private float adjustRotationPower = 5f;

		[SerializeField]
		private bool useTriangleBend;

		[SerializeField]
		private BezierParam triangleBend = new BezierParam(0.5f, 0.5f, true, 0f, false);

		[SerializeField]
		private bool useVolume;

		[SerializeField]
		private float maxVolumeLength = 0.1f;

		[SerializeField]
		private BezierParam volumeStretchStiffness = new BezierParam(0.5f, 0.5f, true, 0f, false);

		[SerializeField]
		private BezierParam volumeShearStiffness = new BezierParam(0.5f, 0.5f, true, 0f, false);

		[SerializeField]
		private bool useCollision;

		[SerializeField]
		private float friction = 0.2f;

		[SerializeField]
		private bool keepInitialShape;

		[SerializeField]
		private bool usePenetration;

		[SerializeField]
		private PenetrationMode penetrationMode;

		[SerializeField]
		private PenetrationAxis penetrationAxis = PenetrationAxis.InverseZ;

		[SerializeField]
		private float penetrationMaxDepth = 1f;

		[SerializeField]
		private BezierParam penetrationConnectDistance = new BezierParam(0.1f, 0.2f, true, 0f, false);

		[SerializeField]
		private BezierParam penetrationDistance = new BezierParam(0.02f, 0.05f, true, 0f, false);

		[SerializeField]
		private BezierParam penetrationRadius = new BezierParam(0.3f, 1f, true, 0f, false);

		[SerializeField]
		private bool useLineAvarageRotation = true;

		[SerializeField]
		private bool useFixedNonRotation;

		private HashSet<ParamType> changeSet = new HashSet<ParamType>();

		public bool UseGravity
		{
			get
			{
				return useGravity;
			}
		}

		public bool UseDrag
		{
			get
			{
				return useDrag;
			}
		}

		public bool UseMaxVelocity
		{
			get
			{
				return useMaxVelocity;
			}
		}

		public float MassInfluence
		{
			get
			{
				return massInfluence;
			}
			set
			{
				massInfluence = value;
			}
		}

		public float WindInfluence
		{
			get
			{
				return windInfluence;
			}
			set
			{
				windInfluence = value;
			}
		}

		public float WindRandomScale
		{
			get
			{
				return windRandomScale;
			}
			set
			{
				windRandomScale = value;
			}
		}

		public float MaxMoveSpeed
		{
			get
			{
				return maxMoveSpeed;
			}
			set
			{
				maxMoveSpeed = value;
			}
		}

		public bool UseResetTeleport
		{
			get
			{
				return useResetTeleport;
			}
			set
			{
				useResetTeleport = value;
			}
		}

		public float TeleportDistance
		{
			get
			{
				return teleportDistance;
			}
			set
			{
				teleportDistance = value;
			}
		}

		public float TeleportRotation
		{
			get
			{
				return teleportRotation;
			}
			set
			{
				teleportRotation = value;
			}
		}

		public float ResetStabilizationTime
		{
			get
			{
				return resetStabilizationTime;
			}
			set
			{
				resetStabilizationTime = value;
			}
		}

		public bool UseDistanceDisable
		{
			get
			{
				return useDistanceDisable;
			}
			set
			{
				useDistanceDisable = value;
			}
		}

		public Transform DisableReferenceObject
		{
			get
			{
				return disableReferenceObject;
			}
			set
			{
				disableReferenceObject = value;
			}
		}

		public float DisableDistance
		{
			get
			{
				return disableDistance;
			}
			set
			{
				disableDistance = value;
			}
		}

		public float DisableFadeDistance
		{
			get
			{
				return disableFadeDistance;
			}
			set
			{
				disableFadeDistance = value;
			}
		}

		public bool UseClampDistanceRatio
		{
			get
			{
				return useClampDistanceRatio;
			}
		}

		public float ClampDistanceMinRatio
		{
			get
			{
				if (!useClampDistanceRatio)
				{
					return 0f;
				}
				return clampDistanceMinRatio;
			}
		}

		public float ClampDistanceMaxRatio
		{
			get
			{
				if (!useClampDistanceRatio)
				{
					return 0f;
				}
				return clampDistanceMaxRatio;
			}
		}

		public float ClampDistanceVelocityInfluence
		{
			get
			{
				if (!useClampDistanceRatio)
				{
					return 1f;
				}
				return clampDistanceVelocityInfluence;
			}
		}

		public bool UseClampPositionLength
		{
			get
			{
				return useClampPositionLength;
			}
		}

		public Vector3 ClampPositionAxisRatio
		{
			get
			{
				return new Vector3(clampPositionRatioX, clampPositionRatioY, clampPositionRatioZ);
			}
		}

		public float ClampPositionVelocityInfluence
		{
			get
			{
				if (!useClampPositionLength)
				{
					return 1f;
				}
				return clampPositionVelocityInfluence;
			}
		}

		public bool UseClampRotation
		{
			get
			{
				return useClampRotation;
			}
		}

		public float ClampRotationVelocityInfluence
		{
			get
			{
				if (!useClampRotation)
				{
					return 1f;
				}
				return clampRotationVelocityInfluence;
			}
		}

		public float RestoreDistanceVelocityInfluence
		{
			get
			{
				return restoreDistanceVelocityInfluence;
			}
		}

		public bool UseBendDistance
		{
			get
			{
				return useBendDistance;
			}
		}

		public int BendDistanceMaxCount
		{
			get
			{
				return bendDistanceMaxCount;
			}
		}

		public bool UseNearDistance
		{
			get
			{
				return useNearDistance;
			}
		}

		public int NearDistanceMaxCount
		{
			get
			{
				return nearDistanceMaxCount;
			}
		}

		public float NearDistanceMaxDepth
		{
			get
			{
				return nearDistanceMaxDepth;
			}
		}

		public bool UseRestoreRotation
		{
			get
			{
				return useRestoreRotation;
			}
		}

		public float RestoreRotationVelocityInfluence
		{
			get
			{
				if (!useRestoreRotation)
				{
					return 1f;
				}
				return restoreRotationVelocityInfluence;
			}
		}

		public bool UseSpring
		{
			get
			{
				return useSpring;
			}
		}

		public float SpringRadius
		{
			get
			{
				return springRadius;
			}
		}

		public Vector3 SpringRadiusScale
		{
			get
			{
				return new Vector3(springScaleX, springScaleY, springScaleZ);
			}
		}

		public float SpringIntensity
		{
			get
			{
				return springIntensity;
			}
		}

		public AdjustMode AdjustRotationMode
		{
			get
			{
				return adjustMode;
			}
		}

		public Vector3 AdjustRotationVector
		{
			get
			{
				return Vector3.one * adjustRotationPower;
			}
		}

		public bool UseTriangleBend
		{
			get
			{
				return useTriangleBend;
			}
		}

		public bool UseVolume
		{
			get
			{
				return useVolume;
			}
		}

		public bool UseCollision
		{
			get
			{
				return useCollision;
			}
		}

		public float Friction
		{
			get
			{
				return friction;
			}
		}

		public bool KeepInitialShape
		{
			get
			{
				return keepInitialShape;
			}
		}

		public bool UsePenetration
		{
			get
			{
				return usePenetration;
			}
			set
			{
				usePenetration = value;
			}
		}

		public float PenetrationMaxDepth
		{
			get
			{
				return penetrationMaxDepth;
			}
		}

		public bool UseLineAvarageRotation
		{
			get
			{
				return useLineAvarageRotation;
			}
		}

		public bool UseFixedNonRotation
		{
			get
			{
				return useFixedNonRotation;
			}
		}

		public void SetChangeParam(ParamType ptype)
		{
			changeSet.Add(ptype);
		}

		public bool ChangedParam(ParamType ptype)
		{
			return changeSet.Contains(ptype);
		}

		public void ClearChangeParam()
		{
			changeSet.Clear();
		}

		public int GetParamHash(BaseCloth cloth, ParamType ptype)
		{
			int num = 0;
			switch (ptype)
			{
			case ParamType.WorldInfluence:
				num += (influenceTarget ? influenceTarget.GetDataHash() : 0);
				break;
			case ParamType.RestoreDistance:
				if (useBendDistance)
				{
					num += useBendDistance.GetDataHash();
					num += bendDistanceMaxCount.GetDataHash();
				}
				if (useNearDistance)
				{
					num += useNearDistance.GetDataHash();
					num += nearDistanceMaxCount.GetDataHash();
					num += nearDistanceMaxDepth.GetDataHash();
				}
				break;
			case ParamType.ClampDistance:
				if (useClampDistanceRatio)
				{
					num += useClampDistanceRatio.GetDataHash();
				}
				break;
			case ParamType.ClampPosition:
				if (useClampPositionLength)
				{
					num += useClampPositionLength.GetDataHash();
				}
				break;
			case ParamType.RestoreRotation:
				if (useRestoreRotation)
				{
					num += useRestoreRotation.GetDataHash();
				}
				break;
			case ParamType.ClampRotation:
				if (useClampRotation)
				{
					num += useClampRotation.GetDataHash();
				}
				break;
			case ParamType.TriangleBend:
				if (useTriangleBend)
				{
					num += useTriangleBend.GetDataHash();
				}
				break;
			case ParamType.Penetration:
				if (usePenetration)
				{
					num += usePenetration.GetDataHash();
					num += penetrationMode.GetDataHash();
					if (penetrationMode == PenetrationMode.SurfacePenetration)
					{
						num += penetrationMaxDepth.GetDataHash();
						num += penetrationAxis.GetDataHash();
					}
					if (penetrationMode == PenetrationMode.ColliderPenetration)
					{
						num += penetrationMaxDepth.GetDataHash();
						num += penetrationConnectDistance.GetDataHash();
						num += cloth.TeamData.ColliderList.GetDataHash();
						num += cloth.TeamData.PenetrationIgnoreColliderList.GetDataHash();
					}
				}
				break;
			case ParamType.Spring:
				if (useSpring)
				{
					num += useSpring.GetDataHash();
					num += springRadius.GetDataHash();
					num += springScaleX.GetDataHash();
					num += springScaleY.GetDataHash();
					num += springScaleZ.GetDataHash();
					num += springDirectionAtten.GetDataHash();
					num += springDistanceAtten.GetDataHash();
					num += springIntensity.GetDataHash();
				}
				break;
			case ParamType.ColliderCollision:
				if (useCollision)
				{
					num += cloth.TeamData.ColliderList.GetDataHash();
				}
				break;
			}
			return num;
		}

		public void SetRadius(float sval, float eval)
		{
			radius.SetParam(sval, eval);
		}

		public float GetRadius(float depth)
		{
			return radius.Evaluate(depth);
		}

		public BezierParam GetRadius()
		{
			return radius;
		}

		public void SetMass(float sval, float eval, bool useEval = true, float cval = 0f, bool useCval = false)
		{
			mass.SetParam(sval, eval, useEval, cval, useCval);
		}

		public BezierParam GetMass()
		{
			return mass;
		}

		public void SetGravity(bool sw, float sval = -9.8f, float eval = -9.8f)
		{
			useGravity = sw;
			gravity.SetParam(sval, eval);
		}

		public BezierParam GetGravity()
		{
			if (useGravity)
			{
				return gravity;
			}
			return new BezierParam(0f);
		}

		public void SetDrag(bool sw, float sval = 0.015f, float eval = 0.015f)
		{
			useDrag = sw;
			drag.SetParam(sval, eval);
		}

		public BezierParam GetDrag()
		{
			if (useDrag)
			{
				return drag;
			}
			return new BezierParam(0f);
		}

		public void SetMaxVelocity(bool sw, float sval = 3f, float eval = 3f)
		{
			useMaxVelocity = sw;
			maxVelocity.SetParam(sval, eval);
		}

		public BezierParam GetMaxVelocity()
		{
			if (useMaxVelocity)
			{
				return maxVelocity;
			}
			return new BezierParam(1000f);
		}

		public void SetExternalForce(float massInfluence, float windInfluence, float windRandomScale)
		{
			this.massInfluence = massInfluence;
			this.windInfluence = windInfluence;
			this.windRandomScale = windRandomScale;
		}

		public void SetWorldInfluence(float maxspeed, float moveval, float rotval)
		{
			maxMoveSpeed = maxspeed;
			worldMoveInfluence.SetParam(moveval, moveval, false);
			worldRotationInfluence.SetParam(rotval, rotval, false);
		}

		public BezierParam GetWorldMoveInfluence()
		{
			return worldMoveInfluence;
		}

		public BezierParam GetWorldRotationInfluence()
		{
			return worldRotationInfluence;
		}

		public Transform GetInfluenceTarget()
		{
			return influenceTarget;
		}

		public void SetInfluenceTarget(Transform t)
		{
			influenceTarget = t;
		}

		public void SetTeleport(bool sw, float distance = 0.2f, float rotation = 45f)
		{
			useResetTeleport = sw;
			teleportDistance = distance;
			teleportRotation = rotation;
		}

		public void SetDistanceDisable(bool sw, float distance = 20f, float fadeDistance = 5f, Transform referenceObject = null)
		{
			useDistanceDisable = sw;
			disableReferenceObject = referenceObject;
			disableDistance = distance;
			disableFadeDistance = fadeDistance;
		}

		public void SetClampDistanceRatio(bool sw, float minval = 0.1f, float maxval = 1.05f, float influence = 0.2f)
		{
			useClampDistanceRatio = sw;
			clampDistanceMinRatio = minval;
			clampDistanceMaxRatio = maxval;
			clampDistanceVelocityInfluence = influence;
		}

		public void SetClampPositionLength(bool sw, float sval = 0.03f, float eval = 0.2f, float ratioX = 1f, float ratioY = 1f, float ratioZ = 1f, float influence = 0.2f)
		{
			useClampPositionLength = sw;
			clampPositionLength.SetParam(sval, eval);
			clampPositionRatioX = ratioX;
			clampPositionRatioY = ratioY;
			clampPositionRatioZ = ratioZ;
			clampPositionVelocityInfluence = influence;
		}

		public BezierParam GetClampPositionLength()
		{
			return clampPositionLength;
		}

		public void SetClampRotationAngle(bool sw, float sval = 30f, float eval = 30f, float influence = 0.2f)
		{
			useClampRotation = sw;
			clampRotationAngle.SetParam(sval, eval);
			clampRotationVelocityInfluence = influence;
		}

		public BezierParam GetClampRotationAngle()
		{
			return clampRotationAngle;
		}

		public void SetRestoreDistance(float influence = 1f, float structStiffness = 1f)
		{
			restoreDistanceVelocityInfluence = influence;
			structDistanceStiffness.SetParam(structStiffness, structStiffness, false);
		}

		public BezierParam GetStructDistanceStiffness()
		{
			return structDistanceStiffness;
		}

		public BezierParam GetBendDistanceStiffness()
		{
			return bendDistanceStiffness;
		}

		public BezierParam GetNearDistanceLength()
		{
			return nearDistanceLength;
		}

		public BezierParam GetNearDistanceStiffness()
		{
			return nearDistanceStiffness;
		}

		public void SetRestoreRotation(bool sw, float sval = 0.02f, float eval = 0.001f, float influence = 0.3f)
		{
			useRestoreRotation = sw;
			restoreRotation.SetParam(sval, eval);
			restoreRotationVelocityInfluence = influence;
		}

		public BezierParam GetRotationPower()
		{
			return restoreRotation;
		}

		public void SetSpring(bool sw, float power = 0f, float r = 0f, float sclx = 1f, float scly = 1f, float sclz = 1f, float intensity = 1f)
		{
			useSpring = sw;
			springPower = power;
			springRadius = r;
			springScaleX = sclx;
			springScaleY = scly;
			springScaleZ = sclz;
			springIntensity = intensity;
		}

		public void SetSpringDirectionAtten(float sval, float eval, float cval)
		{
			springDirectionAtten.SetParam(sval, eval, true, cval, true);
		}

		public void SetSpringDistanceAtten(float sval, float eval, float cval)
		{
			springDistanceAtten.SetParam(sval, eval, true, cval, true);
		}

		public float GetSpringPower()
		{
			if (useSpring)
			{
				return springPower;
			}
			return 0f;
		}

		public float GetSpringDirectionAtten(float ratio)
		{
			return springDirectionAtten.Evaluate(ratio);
		}

		public float GetSpringDistanceAtten(float ratio)
		{
			return springDistanceAtten.Evaluate(ratio);
		}

		public void SetAdjustRotation(AdjustMode amode = AdjustMode.Fixed, float power = 0f)
		{
			adjustMode = amode;
			adjustRotationPower = power;
		}

		public void SetTriangleBend(bool sw, float sval = 0.03f, float eval = 0.03f)
		{
			useTriangleBend = sw;
			triangleBend.SetParam(sval, eval);
		}

		public float GetTriangleBendPower(float depth)
		{
			if (useTriangleBend)
			{
				return triangleBend.Evaluate(depth);
			}
			return 0f;
		}

		public BezierParam GetTriangleBendStiffness()
		{
			return triangleBend;
		}

		public void SetVolume(bool sw, float maxLength = 0.05f, float stiffness = 0.5f, float shear = 0.5f)
		{
			useVolume = sw;
			maxVolumeLength = maxLength;
			volumeShearStiffness.SetParam(stiffness, stiffness, false);
			volumeShearStiffness.SetParam(shear, shear, false);
		}

		public float GetMaxVolumeLength()
		{
			if (useVolume)
			{
				return maxVolumeLength;
			}
			return 0f;
		}

		public BezierParam GetVolumeStretchStiffness()
		{
			return volumeStretchStiffness;
		}

		public BezierParam GetVolumeShearStiffness()
		{
			return volumeShearStiffness;
		}

		public void SetCollision(bool sw, float friction = 0.2f)
		{
			useCollision = sw;
			this.friction = friction;
		}

		public PenetrationMode GetPenetrationMode()
		{
			return penetrationMode;
		}

		public PenetrationAxis GetPenetrationAxis()
		{
			return penetrationAxis;
		}

		public BezierParam GetPenetrationConnectDistance()
		{
			return penetrationConnectDistance;
		}

		public BezierParam GetPenetrationRadius()
		{
			return penetrationRadius;
		}

		public BezierParam GetPenetrationDistance()
		{
			return penetrationDistance;
		}
	}
}
