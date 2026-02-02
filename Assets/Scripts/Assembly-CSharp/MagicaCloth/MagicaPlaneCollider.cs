using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/magica-cloth-plane-collider/")]
	[AddComponentMenu("MagicaCloth/MagicaPlaneCollider")]
	public class MagicaPlaneCollider : ColliderComponent
	{
		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			foreach (ChunkData value in particleDict.Values)
			{
				for (int i = 0; i < value.dataLength; i++)
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetLocalPos(value.startIndex + i, base.Center);
				}
			}
		}

		protected override ChunkData CreateColliderParticleReal(int teamId)
		{
			uint num = 0u;
			num |= 2u;
			num |= 0x10u;
			num |= 0x1000u;
			num |= 0x2000u;
			num |= 0x4000u;
			num |= 0x20u;
			num |= 0x2000000u;
			num |= 0x8000u;
			ChunkData result = CreateParticle(num, teamId, 0f, 1f, base.Center);
			CreateSingleton<MagicaPhysicsManager>.Instance.Team.AddCollider(teamId, result.startIndex);
			return result;
		}

		public override bool CalcNearPoint(Vector3 pos, out Vector3 p, out Vector3 dir, out Vector3 d, bool skinning)
		{
			dir = Vector3.zero;
			Vector3 vector = base.transform.TransformPoint(base.Center);
			Vector3 up = base.transform.up;
			Vector3 vector2 = pos - vector;
			Vector3 vector3 = Vector3.Project(vector2, up);
			p = pos - vector3;
			d = p;
			dir = vector3.normalized;
			return Vector3.Dot(vector2, vector3) <= 0f;
		}
	}
}
