using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class ColliderExtrusionConstraint : PhysicsManagerConstraint
	{
		[BurstCompile]
		private struct CollisionExtrusionJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[ReadOnly]
			public NativeArray<quaternion> nextRotList;

			[WriteOnly]
			public NativeArray<float3> outNextPosList;

			public NativeArray<int> collisionLinkIdList;

			[ReadOnly]
			public NativeArray<float> collisionDistList;

			[NativeDisableParallelForRestriction]
			public NativeArray<float3> posList;

			[ReadOnly]
			public NativeArray<quaternion> rotList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			public void Execute(int index)
			{
				float3 @float = nextPosList[index];
				outNextPosList[index] = @float;
				int num = collisionLinkIdList[index];
				float num2 = collisionDistList[index];
				collisionLinkIdList[index] = 0;
				if (num <= 0)
				{
					return;
				}
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid() || particleFlag.IsFixed() || particleFlag.IsCollider())
				{
					return;
				}
				int index2 = teamIdList[index];
				PhysicsManagerTeamData.TeamData teamData = teamDataList[index2];
				if (!teamData.IsActive() || !teamData.IsFlag(32u) || !teamData.IsUpdate())
				{
					return;
				}
				float3 float2 = posList[num];
				quaternion q = rotList[num];
				float3 float3 = @float - float2;
				float3 v = math.mul(math.inverse(q), float3);
				float3 float4 = nextPosList[num];
				float3 float5 = math.mul(nextRotList[num], v) + float4;
				float3 x = float5 - @float;
				if (!(math.length(x) < 1E-06f))
				{
					float num3 = math.dot(math.normalize(x), math.normalize(float3));
					if (!(num3 <= 0f))
					{
						num3 = math.pow(num3, 0.5f);
						float x2 = math.saturate((0.02f - num2) / 0.02f);
						x2 = math.pow(x2, 2f);
						num3 *= x2;
						float3 float6 = @float;
						@float = math.lerp(@float, float5, num3);
						outNextPosList[index] = @float;
						float3 float7 = (@float - float6) * 0.75f;
						posList[index] += float7;
					}
				}
			}
		}

		public override void Create()
		{
		}

		public override void RemoveTeam(int teamId)
		{
		}

		public override void Release()
		{
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (base.Manager.Particle.ColliderCount <= 0)
			{
				return jobHandle;
			}
			CollisionExtrusionJob jobData = default(CollisionExtrusionJob);
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.nextRotList = base.Manager.Particle.InNextRotList.ToJobArray();
			jobData.collisionLinkIdList = base.Manager.Particle.collisionLinkIdList.ToJobArray();
			jobData.collisionDistList = base.Manager.Particle.collisionDistList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobData.rotList = base.Manager.Particle.rotList.ToJobArray();
			jobData.outNextPosList = base.Manager.Particle.OutNextPosList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			base.Manager.Particle.SwitchingNextPosList();
			return jobHandle;
		}
	}
}
