using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class ClampDistance2Constraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct ClampDistance2Data
		{
			public int vertexIndex;

			public int parentVertexIndex;

			public float length;
		}

		[Serializable]
		public struct ClampDistance2RootInfo
		{
			public ushort startIndex;

			public ushort dataLength;
		}

		public struct GroupData
		{
			public int teamId;

			public int active;

			public float minRatio;

			public float maxRatio;

			public float velocityInfluence;

			public ChunkData dataChunk;

			public ChunkData rootInfoChunk;
		}

		[BurstCompile]
		private struct ClampDistance2Job : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<ClampDistance2Data> dataList;

			[ReadOnly]
			public NativeArray<ClampDistance2RootInfo> rootInfoList;

			[ReadOnly]
			public NativeArray<int> rootTeamList;

			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float> frictionList;

			[NativeDisableParallelForRestriction]
			public NativeArray<float3> nextPosList;

			[NativeDisableParallelForRestriction]
			public NativeArray<float3> posList;

			public void Execute(int rootIndex)
			{
				int num = rootTeamList[rootIndex];
				if (num == 0)
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[num];
				if (!teamData.IsActive() || teamData.clampDistance2GroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				GroupData groupData = groupList[teamData.clampDistance2GroupIndex];
				if (groupData.active == 0)
				{
					return;
				}
				ClampDistance2RootInfo clampDistance2RootInfo = rootInfoList[rootIndex];
				int num2 = clampDistance2RootInfo.startIndex + groupData.dataChunk.startIndex;
				int dataLength = clampDistance2RootInfo.dataLength;
				int startIndex = teamData.particleChunk.startIndex;
				for (int i = 0; i < dataLength; i++)
				{
					ClampDistance2Data clampDistance2Data = dataList[num2 + i];
					int parentVertexIndex = clampDistance2Data.parentVertexIndex;
					if (parentVertexIndex >= 0)
					{
						int vertexIndex = clampDistance2Data.vertexIndex;
						vertexIndex += startIndex;
						parentVertexIndex += startIndex;
						PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[vertexIndex];
						if (particleFlag.IsValid() && !particleFlag.IsFixed())
						{
							float3 @float = nextPosList[vertexIndex];
							float3 float2 = @float;
							float3 float3 = nextPosList[parentVertexIndex];
							float3 v = @float - float3;
							float num3 = clampDistance2Data.length * teamData.scaleRatio;
							v = MathUtility.ClampVector(v, num3 * groupData.minRatio, num3 * groupData.maxRatio);
							@float = float3 + v;
							nextPosList[vertexIndex] = @float;
							float3 float4 = (@float - float2) * (1f - groupData.velocityInfluence);
							posList[vertexIndex] += float4;
						}
					}
				}
			}
		}

		private FixedChunkNativeArray<ClampDistance2Data> dataList;

		private FixedChunkNativeArray<ClampDistance2RootInfo> rootInfoList;

		public FixedNativeList<GroupData> groupList;

		private FixedChunkNativeArray<int> rootTeamList;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<ClampDistance2Data>();
			rootInfoList = new FixedChunkNativeArray<ClampDistance2RootInfo>();
			groupList = new FixedNativeList<GroupData>();
			rootTeamList = new FixedChunkNativeArray<int>();
		}

		public override void Release()
		{
			dataList.Dispose();
			rootInfoList.Dispose();
			groupList.Dispose();
			rootTeamList.Dispose();
		}

		public int AddGroup(int teamId, bool active, float minRatio, float maxRatio, float velocityInfluence, ClampDistance2Data[] dataArray, ClampDistance2RootInfo[] rootInfoArray)
		{
			if (dataArray == null || dataArray.Length == 0 || rootInfoArray == null || rootInfoArray.Length == 0)
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
			element.rootInfoChunk = rootInfoList.AddChunk(rootInfoArray.Length);
			dataList.ToJobArray().CopyFromFast(element.dataChunk.startIndex, dataArray);
			rootInfoList.ToJobArray().CopyFromFast(element.rootInfoChunk.startIndex, rootInfoArray);
			int result = groupList.Add(element);
			ChunkData chunk = rootTeamList.AddChunk(rootInfoArray.Length);
			rootTeamList.Fill(chunk, teamId);
			return result;
		}

		public override void RemoveTeam(int teamId)
		{
			int clampDistance2GroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].clampDistance2GroupIndex;
			if (clampDistance2GroupIndex >= 0)
			{
				GroupData groupData = groupList[clampDistance2GroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				rootInfoList.RemoveChunk(groupData.rootInfoChunk);
				rootTeamList.RemoveChunk(groupData.rootInfoChunk);
				groupList.Remove(clampDistance2GroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, float minRatio, float maxRatio, float velocityInfluence)
		{
			int clampDistance2GroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].clampDistance2GroupIndex;
			if (clampDistance2GroupIndex >= 0)
			{
				GroupData value = groupList[clampDistance2GroupIndex];
				value.active = (active ? 1 : 0);
				value.minRatio = minRatio;
				value.maxRatio = maxRatio;
				value.velocityInfluence = velocityInfluence;
				groupList[clampDistance2GroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			ClampDistance2Job jobData = default(ClampDistance2Job);
			jobData.dataList = dataList.ToJobArray();
			jobData.rootInfoList = rootInfoList.ToJobArray();
			jobData.rootTeamList = rootTeamList.ToJobArray();
			jobData.groupList = groupList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.frictionList = base.Manager.Particle.frictionList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobHandle = jobData.Schedule(rootTeamList.Length, 8, jobHandle);
			return jobHandle;
		}
	}
}
