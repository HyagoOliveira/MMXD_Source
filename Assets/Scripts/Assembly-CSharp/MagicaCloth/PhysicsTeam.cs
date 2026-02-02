using System;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public abstract class PhysicsTeam : CoreComponent
	{
		[SerializeField]
		protected PhysicsTeamData teamData = new PhysicsTeamData();

		[SerializeField]
		private float userBlendWeight = 1f;

		protected int teamId = -1;

		protected ChunkData particleChunk;

		protected Transform influenceTarget;

		public int TeamId
		{
			get
			{
				return teamId;
			}
		}

		public PhysicsTeamData TeamData
		{
			get
			{
				return teamData;
			}
		}

		public ChunkData ParticleChunk
		{
			get
			{
				return particleChunk;
			}
		}

		public Transform InfluenceTarget
		{
			get
			{
				return influenceTarget;
			}
			set
			{
				influenceTarget = value;
			}
		}

		public float UserBlendWeight
		{
			get
			{
				return userBlendWeight;
			}
			set
			{
				userBlendWeight = value;
			}
		}

		public override int GetDataHash()
		{
			return teamData.GetDataHash();
		}

		protected override void OnInit()
		{
			teamId = CreateSingleton<MagicaPhysicsManager>.Instance.Team.CreateTeam(this, 0u);
			TeamData.Init(TeamId);
		}

		protected override void OnDispose()
		{
			if (TeamId >= 0)
			{
				TeamData.Dispose(TeamId);
				if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Team.RemoveTeam(teamId);
				}
			}
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnActive()
		{
			CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetEnable(teamId, true);
			if (CreateSingleton<MagicaPhysicsManager>.Instance.IsDelay && base.ActiveCount > 1)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.ResetFuturePredictionCollidere(TeamId);
			}
		}

		protected override void OnInactive()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Team.SetEnable(teamId, false);
			}
		}

		public bool IsActive()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				return CreateSingleton<MagicaPhysicsManager>.Instance.Team.IsActive(teamId);
			}
			return false;
		}

		public bool IsValid()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				return CreateSingleton<MagicaPhysicsManager>.Instance.Team.IsValid(teamId);
			}
			return false;
		}

		public ChunkData CreateParticle(int team, int count, Func<int, uint> funcFlag, Func<int, float3> funcWpos, Func<int, quaternion> funcWrot, Func<int, float> funcDepth, Func<int, float3> funcRadius, Func<int, float3> funcTargetLocalPos)
		{
			return particleChunk = CreateSingleton<MagicaPhysicsManager>.Instance.Particle.CreateParticle(team, count, funcFlag, funcWpos, funcWrot, funcDepth, funcRadius, funcTargetLocalPos);
		}

		public void RemoveAllParticle()
		{
			CreateSingleton<MagicaPhysicsManager>.Instance.Particle.RemoveParticle(particleChunk);
			particleChunk.Clear();
		}

		public void EnableParticle(Func<int, Transform> funcTarget, Func<int, float3> funcLpos, Func<int, quaternion> funcLrot)
		{
			CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetEnable(particleChunk, true, funcTarget, funcLpos, funcLrot);
		}

		public void DisableParticle(Func<int, Transform> funcTarget, Func<int, float3> funcLpos, Func<int, quaternion> funcLrot)
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetEnable(particleChunk, false, funcTarget, funcLpos, funcLrot);
			}
		}

		public override Define.Error VerifyData()
		{
			return base.VerifyData();
		}
	}
}
