using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class ClampDistanceConstraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct ClampDistanceData
		{
			public ushort vertexIndex;

			public ushort targetVertexIndex;

			public float length;

			public bool IsValid()
			{
				if (vertexIndex <= 0)
				{
					return targetVertexIndex > 0;
				}
				return true;
			}
		}

		public struct GroupData
		{
			public int teamId;

			public int active;

			public float minRatio;

			public float maxRatio;

			public float velocityInfluence;

			public ChunkData dataChunk;

			public ChunkData refChunk;
		}

		[BurstCompile]
		private struct ClampDistanceJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<ClampDistanceData> clampDistanceList;

			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeArray<ReferenceDataIndex> refDataList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[WriteOnly]
			public NativeArray<float3> outNextPosList;

			public NativeArray<float3> posList;

			[ReadOnly]
			public NativeArray<float> frictionList;

			public void Execute(int index)
			{
				float3 @float = nextPosList[index];
				outNextPosList[index] = @float;
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid() || particleFlag.IsFixed())
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[teamIdList[index]];
				if (!teamData.IsActive() || teamData.clampDistanceGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				int startIndex = teamData.particleChunk.startIndex;
				int num = index - startIndex;
				GroupData groupData = groupList[teamData.clampDistanceGroupIndex];
				if (groupData.active == 0)
				{
					return;
				}
				ReferenceDataIndex referenceDataIndex = refDataList[groupData.refChunk.startIndex + num];
				if (referenceDataIndex.count > 0)
				{
					int index2 = groupData.dataChunk.startIndex + referenceDataIndex.startIndex;
					ClampDistanceData clampDistanceData = clampDistanceList[index2];
					if (clampDistanceData.IsValid())
					{
						int index3 = startIndex + clampDistanceData.targetVertexIndex;
						float3 float2 = nextPosList[index3];
						float3 v = @float - float2;
						float length = clampDistanceData.length;
						length *= teamData.scaleRatio;
						v = MathUtility.ClampVector(v, length * groupData.minRatio, length * groupData.maxRatio);
						float3 float3 = @float;
						@float = float2 + v;
						float num2 = frictionList[index];
						float s = math.saturate(1f - num2 * 1f);
						@float = math.lerp(float3, @float, s);
						outNextPosList[index] = @float;
						float3 float4 = (@float - float3) * (1f - groupData.velocityInfluence);
						posList[index] += float4;
					}
				}
			}
		}

		private FixedChunkNativeArray<ClampDistanceData> dataList;

		private FixedChunkNativeArray<ReferenceDataIndex> refDataList;

		public FixedNativeList<GroupData> groupList;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<ClampDistanceData>();
			refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
			groupList = new FixedNativeList<GroupData>();
		}

		public override void Release()
		{
			dataList.Dispose();
			refDataList.Dispose();
			groupList.Dispose();
		}

		public int AddGroup(int teamId, bool active, float minRatio, float maxRatio, float velocityInfluence, ClampDistanceData[] dataArray, ReferenceDataIndex[] refDataArray)
		{
			if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.minRatio = minRatio;
			element.maxRatio = maxRatio;
			element.velocityInfluence = velocityInfluence;
			element.dataChunk = dataList.AddChunk(dataArray.Length);
			element.refChunk = refDataList.AddChunk(refDataArray.Length);
			dataList.ToJobArray().CopyFromFast(element.dataChunk.startIndex, dataArray);
			refDataList.ToJobArray().CopyFromFast(element.refChunk.startIndex, refDataArray);
			return groupList.Add(element);
		}

		public override void RemoveTeam(int teamId)
		{
			int clampDistanceGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].clampDistanceGroupIndex;
			if (clampDistanceGroupIndex >= 0)
			{
				GroupData groupData = groupList[clampDistanceGroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				refDataList.RemoveChunk(groupData.refChunk);
				groupList.Remove(clampDistanceGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, float minRatio, float maxRatio, float velocityInfluence)
		{
			int clampDistanceGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].clampDistanceGroupIndex;
			if (clampDistanceGroupIndex >= 0)
			{
				GroupData value = groupList[clampDistanceGroupIndex];
				value.active = (active ? 1 : 0);
				value.minRatio = minRatio;
				value.maxRatio = maxRatio;
				value.velocityInfluence = velocityInfluence;
				groupList[clampDistanceGroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			ClampDistanceJob jobData = default(ClampDistanceJob);
			jobData.clampDistanceList = dataList.ToJobArray();
			jobData.groupList = groupList.ToJobArray();
			jobData.refDataList = refDataList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.outNextPosList = base.Manager.Particle.OutNextPosList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobData.frictionList = base.Manager.Particle.frictionList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			base.Manager.Particle.SwitchingNextPosList();
			return jobHandle;
		}
	}
}
