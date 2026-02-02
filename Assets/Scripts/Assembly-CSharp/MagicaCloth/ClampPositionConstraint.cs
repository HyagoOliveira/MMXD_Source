using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class ClampPositionConstraint : PhysicsManagerConstraint
	{
		public struct GroupData
		{
			public int teamId;

			public int active;

			public CurveParam limitLength;

			public float3 axisRatio;

			public float velocityInfluence;

			public bool IsAxisCheck()
			{
				if (!(axisRatio.x < 0.999f) && !(axisRatio.y < 0.999f))
				{
					return axisRatio.z < 0.999f;
				}
				return true;
			}
		}

		[BurstCompile]
		private struct ClampPositionJob : IJobParallelFor
		{
			public float maxMoveLength;

			[ReadOnly]
			public NativeArray<GroupData> clampPositionGroupList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float> depthList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			[ReadOnly]
			public NativeArray<float> frictionList;

			public NativeArray<float3> nextPosList;

			public NativeArray<float3> posList;

			public void Execute(int index)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid() || particleFlag.IsFixed())
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[teamIdList[index]];
				if (!teamData.IsActive() || teamData.clampPositionGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				GroupData groupData = clampPositionGroupList[teamData.clampPositionGroupIndex];
				if (groupData.active != 0)
				{
					float3 @float = nextPosList[index];
					float t = depthList[index];
					float num = groupData.limitLength.Evaluate(t);
					num *= teamData.scaleRatio;
					float3 float2 = basePosList[index];
					float3 v = @float - float2;
					float num2 = frictionList[index];
					float s = math.saturate(1f - num2 * 1f);
					if (groupData.IsAxisCheck())
					{
						float3 axisRatio = groupData.axisRatio;
						quaternion q = baseRotList[index];
						float3 x = math.mul(math.inverse(q), v);
						float3 float3 = axisRatio * num;
						x = math.clamp(x, -float3, float3);
						v = math.mul(q, x);
					}
					v = MathUtility.ClampVector(v, 0f, num);
					v = float2 + v - @float;
					float3 float4 = @float;
					float3 y = float4 + v;
					@float = math.lerp(float4, y, s);
					nextPosList[index] = @float;
					float3 float5 = (@float - float4) * (1f - groupData.velocityInfluence);
					posList[index] += float5;
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

		public int AddGroup(int teamId, bool active, BezierParam limitLength, float3 axisRatio, float velocityInfluence)
		{
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.limitLength.Setup(limitLength);
			element.axisRatio = axisRatio;
			element.velocityInfluence = velocityInfluence;
			return groupList.Add(element);
		}

		public override void RemoveTeam(int teamId)
		{
			int clampPositionGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].clampPositionGroupIndex;
			if (clampPositionGroupIndex >= 0)
			{
				groupList.Remove(clampPositionGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, BezierParam limitLength, float3 axisRatio, float velocityInfluence)
		{
			int clampPositionGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].clampPositionGroupIndex;
			if (clampPositionGroupIndex >= 0)
			{
				GroupData value = groupList[clampPositionGroupIndex];
				value.active = (active ? 1 : 0);
				value.limitLength.Setup(limitLength);
				value.axisRatio = axisRatio;
				value.velocityInfluence = velocityInfluence;
				groupList[clampPositionGroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			ClampPositionJob jobData = default(ClampPositionJob);
			jobData.maxMoveLength = dtime * 1f;
			jobData.clampPositionGroupList = groupList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.depthList = base.Manager.Particle.depthList.ToJobArray();
			jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.baseRotList = base.Manager.Particle.baseRotList.ToJobArray();
			jobData.frictionList = base.Manager.Particle.frictionList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			return jobHandle;
		}
	}
}
