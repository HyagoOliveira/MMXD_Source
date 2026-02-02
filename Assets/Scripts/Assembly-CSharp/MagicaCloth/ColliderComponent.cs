using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	public abstract class ColliderComponent : ParticleComponent
	{
		[SerializeField]
		protected bool isGlobal;

		[SerializeField]
		private Vector3 center;

		public Vector3 Center
		{
			get
			{
				return center;
			}
			set
			{
				center = value;
			}
		}

		protected override void OnInit()
		{
			base.OnInit();
			if (isGlobal)
			{
				CreateColliderParticle(0);
			}
		}

		protected override void OnDispose()
		{
			List<int> list = new List<int>();
			foreach (int key in particleDict.Keys)
			{
				list.Add(key);
			}
			foreach (int item in list)
			{
				RemoveColliderParticle(item);
			}
			base.OnDispose();
		}

		public override int GetDataHash()
		{
			return isGlobal.GetDataHash();
		}

		public abstract bool CalcNearPoint(Vector3 pos, out Vector3 p, out Vector3 dir, out Vector3 d, bool skinning);

		public Vector3 CalcLocalPos(Vector3 pos)
		{
			Quaternion rotation = base.transform.rotation;
			Vector3 vector = pos - base.transform.position;
			return Quaternion.Inverse(rotation) * vector;
		}

		public Vector3 CalcLocalDir(Vector3 dir)
		{
			return base.transform.InverseTransformDirection(dir);
		}

		public ChunkData CreateColliderParticle(int teamId)
		{
			ChunkData result = CreateColliderParticleReal(teamId);
			if (base.Status.IsActive)
			{
				EnableTeamParticle(teamId);
			}
			return result;
		}

		public void RemoveColliderParticle(int teamId)
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance() && particleDict.ContainsKey(teamId))
			{
				ChunkData chunkData = particleDict[teamId];
				for (int i = 0; i < chunkData.dataLength; i++)
				{
					int particleIndex = chunkData.startIndex + i;
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.RemoveCollider(teamId, particleIndex);
				}
				RemoveTeamParticle(teamId);
			}
		}

		protected abstract ChunkData CreateColliderParticleReal(int teamId);
	}
}
