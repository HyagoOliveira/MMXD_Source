using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class LineWorker : PhysicsManagerWorker
	{
		[Serializable]
		public struct LineRotationData
		{
			public int vertexIndex;

			public int childCount;

			public int childStartDataIndex;

			public float3 localPos;

			public quaternion localRot;
		}

		[Serializable]
		public struct LineRotationRootInfo
		{
			public ushort startIndex;

			public ushort dataLength;
		}

		public struct GroupData
		{
			public int teamId;

			public int avarage;

			public ChunkData dataChunk;

			public ChunkData rootInfoChunk;
		}

		[BurstCompile]
		private struct LineRotationJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<LineRotationData> dataList;

			[ReadOnly]
			public NativeArray<LineRotationRootInfo> rootInfoList;

			[ReadOnly]
			public NativeArray<int> rootTeamList;

			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float3> posList;

			[NativeDisableParallelForRestriction]
			public NativeArray<quaternion> rotList;

			public void Execute(int rootIndex)
			{
				int num = rootTeamList[rootIndex];
				if (num == 0)
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[num];
				if (!teamData.IsActive() || teamData.lineWorkerGroupIndex < 0)
				{
					return;
				}
				GroupData groupData = groupList[teamData.lineWorkerGroupIndex];
				LineRotationRootInfo lineRotationRootInfo = rootInfoList[rootIndex];
				int startIndex = groupData.dataChunk.startIndex;
				int num2 = lineRotationRootInfo.startIndex + startIndex;
				int dataLength = lineRotationRootInfo.dataLength;
				int startIndex2 = teamData.particleChunk.startIndex;
				if (dataLength <= 1)
				{
					return;
				}
				for (int i = 0; i < dataLength; i++)
				{
					LineRotationData lineRotationData = dataList[num2 + i];
					int vertexIndex = lineRotationData.vertexIndex;
					vertexIndex += startIndex2;
					PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[vertexIndex];
					if (!particleFlag.IsValid())
					{
						continue;
					}
					float3 @float = posList[vertexIndex];
					quaternion quaternion = rotList[vertexIndex];
					if (lineRotationData.childCount <= 0)
					{
						continue;
					}
					float3 from = 0;
					float3 to = 0;
					for (int j = 0; j < lineRotationData.childCount; j++)
					{
						LineRotationData lineRotationData2 = dataList[lineRotationData.childStartDataIndex + startIndex + j];
						int index = lineRotationData2.vertexIndex + startIndex2;
						PhysicsManagerParticleData.ParticleFlag particleFlag2 = flagList[index];
						float3 float2 = posList[index];
						float3 float3 = math.normalize(math.mul(quaternion, lineRotationData2.localPos * teamData.scaleDirection));
						from += float3;
						float3 float4 = math.normalize(float2 - @float);
						to += float4;
						if (!particleFlag2.IsFlag(1024u))
						{
							quaternion a = MathUtility.FromToRotation(float3, float4);
							quaternion b = math.mul(quaternion, new quaternion(lineRotationData2.localRot.value * teamData.quaternionScale));
							b = math.mul(a, b);
							rotList[index] = b;
						}
					}
					if (!particleFlag.IsFlag(1024u) && (!teamData.IsFlag(4u) || !particleFlag.IsKinematic()))
					{
						quaternion quaternion2 = MathUtility.FromToRotation(from, to);
						if (groupData.avarage == 1)
						{
							quaternion2 = math.slerp(quaternion.identity, quaternion2, 0.5f);
						}
						quaternion = math.mul(quaternion2, quaternion);
						rotList[vertexIndex] = math.normalize(quaternion);
					}
				}
			}
		}

		private FixedChunkNativeArray<LineRotationData> dataList;

		private FixedChunkNativeArray<LineRotationRootInfo> rootInfoList;

		public FixedNativeList<GroupData> groupList;

		private FixedChunkNativeArray<int> rootTeamList;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<LineRotationData>();
			rootInfoList = new FixedChunkNativeArray<LineRotationRootInfo>();
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

		public int AddGroup(int teamId, bool avarage, LineRotationData[] dataArray, LineRotationRootInfo[] rootInfoArray)
		{
			if (dataArray == null || dataArray.Length == 0 || rootInfoArray == null || rootInfoArray.Length == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.avarage = (avarage ? 1 : 0);
			element.dataChunk = dataList.AddChunk(dataArray.Length);
			element.rootInfoChunk = rootInfoList.AddChunk(rootInfoArray.Length);
			dataList.ToJobArray().CopyFromFast(element.dataChunk.startIndex, dataArray);
			rootInfoList.ToJobArray().CopyFromFast(element.rootInfoChunk.startIndex, rootInfoArray);
			int result = groupList.Add(element);
			ChunkData chunk = rootTeamList.AddChunk(rootInfoArray.Length);
			rootTeamList.Fill(chunk, teamId);
			return result;
		}

		public override void RemoveGroup(int teamId)
		{
			int lineWorkerGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].lineWorkerGroupIndex;
			if (lineWorkerGroupIndex >= 0)
			{
				GroupData groupData = groupList[lineWorkerGroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				rootInfoList.RemoveChunk(groupData.rootInfoChunk);
				rootTeamList.RemoveChunk(groupData.rootInfoChunk);
				groupList.Remove(lineWorkerGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool avarage)
		{
			int lineWorkerGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].lineWorkerGroupIndex;
			if (lineWorkerGroupIndex >= 0)
			{
				GroupData value = groupList[lineWorkerGroupIndex];
				value.avarage = (avarage ? 1 : 0);
				groupList[lineWorkerGroupIndex] = value;
			}
		}

		public override void Warmup()
		{
		}

		public override JobHandle PreUpdate(JobHandle jobHandle)
		{
			return jobHandle;
		}

		public override JobHandle PostUpdate(JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			LineRotationJob jobData = default(LineRotationJob);
			jobData.dataList = dataList.ToJobArray();
			jobData.rootInfoList = rootInfoList.ToJobArray();
			jobData.rootTeamList = rootTeamList.ToJobArray();
			jobData.groupList = groupList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobData.rotList = base.Manager.Particle.rotList.ToJobArray();
			jobHandle = jobData.Schedule(rootTeamList.Length, 8, jobHandle);
			return jobHandle;
		}
	}
}
