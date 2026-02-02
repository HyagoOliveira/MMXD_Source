using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class SpringConstraint : PhysicsManagerConstraint
	{
		public struct GroupData
		{
			public int teamId;

			public int active;

			public float spring;
		}

		[BurstCompile]
		private struct SpringJob : IJobParallelFor
		{
			public float updatePower;

			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			public NativeArray<float3> nextPosList;

			public void Execute(int index)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid() || particleFlag.IsFixed())
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[teamIdList[index]];
				if (teamData.IsActive() && teamData.springGroupIndex >= 0 && teamData.IsUpdate())
				{
					GroupData groupData = groupList[teamData.springGroupIndex];
					if (groupData.active != 0)
					{
						float3 x = nextPosList[index];
						float3 y = basePosList[index];
						float s = 1f - math.pow(1f - groupData.spring, updatePower);
						x = math.lerp(x, y, s);
						nextPosList[index] = x;
					}
				}
			}
		}

		public FixedNativeList<GroupData> groupList;

		public override void Create()
		{
			groupList = new FixedNativeList<GroupData>();
		}

		public override void Release()
		{
			groupList.Dispose();
		}

		public int AddGroup(int teamId, bool active, float spring)
		{
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.spring = spring;
			return groupList.Add(element);
		}

		public override void RemoveTeam(int teamId)
		{
			int springGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].springGroupIndex;
			if (springGroupIndex >= 0)
			{
				groupList.Remove(springGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, float spring)
		{
			int springGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].springGroupIndex;
			if (springGroupIndex >= 0)
			{
				GroupData value = groupList[springGroupIndex];
				value.active = (active ? 1 : 0);
				value.spring = spring;
				groupList[springGroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			SpringJob jobData = default(SpringJob);
			jobData.updatePower = updatePower;
			jobData.groupList = groupList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			return jobHandle;
		}
	}
}
