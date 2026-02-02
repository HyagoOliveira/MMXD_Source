using Unity.Jobs;
using UnityEngine;

namespace MagicaCloth
{
	public abstract class PhysicsManagerConstraint
	{
		[Range(1f, 4f)]
		public int iteration = 1;

		public MagicaPhysicsManager Manager { get; set; }

		public void Init(MagicaPhysicsManager manager)
		{
			Manager = manager;
			Create();
		}

		public abstract void Create();

		public abstract void RemoveTeam(int teamId);

		public abstract void Release();

		public virtual int GetIterationCount()
		{
			return iteration;
		}

		public abstract JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle);
	}
}
