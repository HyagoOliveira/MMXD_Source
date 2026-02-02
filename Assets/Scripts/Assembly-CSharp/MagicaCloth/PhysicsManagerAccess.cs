using System;

namespace MagicaCloth
{
	public abstract class PhysicsManagerAccess : IDisposable
	{
		protected MagicaPhysicsManager manager;

		public UpdateTimeManager UpdateTime
		{
			get
			{
				return manager.UpdateTime;
			}
		}

		protected PhysicsManagerParticleData Particle
		{
			get
			{
				return manager.Particle;
			}
		}

		protected PhysicsManagerBoneData Bone
		{
			get
			{
				return manager.Bone;
			}
		}

		protected PhysicsManagerMeshData Mesh
		{
			get
			{
				return manager.Mesh;
			}
		}

		protected PhysicsManagerTeamData Team
		{
			get
			{
				return manager.Team;
			}
		}

		protected PhysicsManagerWindData Wind
		{
			get
			{
				return manager.Wind;
			}
		}

		protected PhysicsManagerComponent Component
		{
			get
			{
				return manager.Component;
			}
		}

		protected PhysicsManagerCompute Compute
		{
			get
			{
				return manager.Compute;
			}
		}

		public void SetParent(MagicaPhysicsManager manager)
		{
			this.manager = manager;
		}

		public abstract void Create();

		public abstract void Dispose();
	}
}
