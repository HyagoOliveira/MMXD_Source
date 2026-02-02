using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/magica-cloth-capsule-collider/")]
	[AddComponentMenu("MagicaCloth/MagicaCapsuleCollider")]
	public class MagicaCapsuleCollider : ColliderComponent
	{
		public enum Axis
		{
			X = 0,
			Y = 1,
			Z = 2
		}

		[SerializeField]
		private Axis axis;

		[SerializeField]
		[Range(0f, 1f)]
		private float length = 0.2f;

		[SerializeField]
		[Range(0f, 0.5f)]
		private float startRadius = 0.1f;

		[SerializeField]
		[Range(0f, 0.5f)]
		private float endRadius = 0.1f;

		public Axis AxisMode
		{
			get
			{
				return axis;
			}
			set
			{
				axis = value;
			}
		}

		public float Length
		{
			get
			{
				return length;
			}
			set
			{
				length = value;
			}
		}

		public float StartRadius
		{
			get
			{
				return startRadius;
			}
			set
			{
				startRadius = value;
			}
		}

		public float EndRadius
		{
			get
			{
				return endRadius;
			}
			set
			{
				endRadius = value;
			}
		}

		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (ChunkData value2 in particleDict.Values)
			{
				for (int i = 0; i < value2.dataLength; i++)
				{
					int index = value2.startIndex + i;
					float3 radius = new float3(length, startRadius, endRadius);
					CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetRadius(index, radius);
					CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetLocalPos(index, base.Center);
					PhysicsManagerParticleData.ParticleFlag value = CreateSingleton<MagicaPhysicsManager>.Instance.Particle.flagList[index];
					value.SetFlag(64u, false);
					value.SetFlag(128u, false);
					value.SetFlag(256u, false);
					value.SetFlag(GetCapsuleFlag(), true);
					CreateSingleton<MagicaPhysicsManager>.Instance.Particle.flagList[index] = value;
				}
			}
		}

		public override int GetDataHash()
		{
			return base.GetDataHash() + axis.GetDataHash() + length.GetDataHash() + startRadius.GetDataHash() + endRadius.GetDataHash();
		}

		protected override ChunkData CreateColliderParticleReal(int teamId)
		{
			uint num = 0u;
			num |= 2u;
			num |= 0x10u;
			num |= GetCapsuleFlag();
			num |= 0x4000u;
			num |= 0x1000000u;
			num |= 0x2000000u;
			num |= 0x8000u;
			float3 radius = new float3(length, startRadius, endRadius);
			ChunkData result = CreateParticle(num, teamId, 0f, radius, base.Center);
			CreateSingleton<MagicaPhysicsManager>.Instance.Team.AddCollider(teamId, result.startIndex);
			return result;
		}

		private uint GetCapsuleFlag()
		{
			if (axis == Axis.X)
			{
				return 64u;
			}
			if (axis == Axis.Y)
			{
				return 128u;
			}
			return 256u;
		}

		public Vector3 GetLocalDir()
		{
			if (axis == Axis.X)
			{
				return Vector3.right;
			}
			if (axis == Axis.Y)
			{
				return Vector3.up;
			}
			return Vector3.forward;
		}

		public Vector3 GetLocalUp()
		{
			if (axis == Axis.X)
			{
				return Vector3.up;
			}
			if (axis == Axis.Y)
			{
				return Vector3.forward;
			}
			return Vector3.up;
		}

		public float GetScale()
		{
			Vector3 lossyScale = base.transform.lossyScale;
			if (axis == Axis.X)
			{
				return lossyScale.x;
			}
			if (axis == Axis.Y)
			{
				return lossyScale.y;
			}
			return lossyScale.z;
		}

		public override bool CalcNearPoint(Vector3 pos, out Vector3 p, out Vector3 dir, out Vector3 d, bool skinning)
		{
			dir = Vector3.zero;
			Vector3 localDir = GetLocalDir();
			Vector3 vector = localDir * Length;
			Vector3 vector2 = base.transform.TransformPoint(base.Center);
			Quaternion rotation = base.transform.rotation;
			float scale = GetScale();
			vector *= scale;
			Vector3 vector3 = rotation * -vector + vector2;
			Vector3 vector4 = rotation * vector + vector2;
			if (!skinning)
			{
				vector3 = rotation * (-vector - localDir * StartRadius * scale * 0.5f) + vector2;
				vector4 = rotation * (vector + localDir * EndRadius * scale * 0.5f) + vector2;
			}
			float num = MathUtility.ClosestPtPointSegmentRatio(pos, vector3, vector4);
			if (!skinning && (num < 0.0001f || num > 0.9999f))
			{
				p = Vector3.zero;
				d = Vector3.zero;
				return false;
			}
			float num2 = Mathf.Lerp(StartRadius * scale, EndRadius * scale, num);
			d = vector3 + (vector4 - vector3) * num;
			Vector3 vector5 = pos - d;
			float magnitude = vector5.magnitude;
			if (magnitude < num2)
			{
				p = pos;
				if (magnitude > 0f)
				{
					dir = vector5.normalized;
				}
			}
			else
			{
				dir = vector5.normalized;
				p = d + dir * num2;
			}
			return true;
		}
	}
}
