using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class TriangleWorker : PhysicsManagerWorker
	{
		[Serializable]
		public struct TriangleRotationData
		{
			public int targetIndex;

			public int triangleCount;

			public int triangleStartIndex;

			public quaternion localRot;

			public bool IsValid()
			{
				return triangleCount > 0;
			}
		}

		public struct GroupData
		{
			public int teamId;

			public ChunkData triangleDataChunk;

			public ChunkData triangleIndexChunk;
		}

		[BurstCompile]
		private struct TriangleRotationJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<TriangleRotationData> triangleDataList;

			[ReadOnly]
			public NativeArray<int> triangleIndexList;

			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[WriteOnly]
			public NativeArray<quaternion> rotList;

			public void Execute(int index)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid() || !particleFlag.IsFlag(1024u))
				{
					return;
				}
				int index2 = teamIdList[index];
				PhysicsManagerTeamData.TeamData teamData = teamDataList[index2];
				if (!teamData.IsActive() || teamData.triangleWorkerGroupIndex < 0 || (teamData.IsFlag(4u) && particleFlag.IsKinematic()))
				{
					return;
				}
				int startIndex = teamData.particleChunk.startIndex;
				int num = index - startIndex;
				if (num < 0)
				{
					return;
				}
				GroupData groupData = groupList[teamData.triangleWorkerGroupIndex];
				TriangleRotationData triangleRotationData = triangleDataList[groupData.triangleDataChunk.startIndex + num];
				if (triangleRotationData.IsValid())
				{
					float3 x = 0;
					int num2 = triangleRotationData.triangleStartIndex;
					int startIndex2 = groupData.triangleIndexChunk.startIndex;
					for (int i = 0; i < triangleRotationData.triangleCount; i++)
					{
						int num3 = triangleIndexList[startIndex2 + num2];
						int num4 = triangleIndexList[startIndex2 + num2 + 1];
						int num5 = triangleIndexList[startIndex2 + num2 + 2];
						num2 += 3;
						float3 @float = nextPosList[startIndex + num3];
						float3 float2 = nextPosList[startIndex + num4];
						float3 x2 = math.cross(y: nextPosList[startIndex + num5] - @float, x: float2 - @float);
						x += math.normalize(x2);
					}
					x = math.normalize(x);
					float3 float3 = nextPosList[index];
					float3 up = math.normalize(nextPosList[startIndex + triangleRotationData.targetIndex] - float3);
					x *= teamData.scaleDirection.x * teamData.scaleDirection.y;
					up *= teamData.scaleDirection.y;
					quaternion a = quaternion.LookRotation(x, up);
					a = math.mul(a, new quaternion(triangleRotationData.localRot.value * teamData.quaternionScale));
					rotList[index] = a;
				}
			}
		}

		private FixedChunkNativeArray<TriangleRotationData> triangleDataList;

		private FixedChunkNativeArray<int> triangleIndexList;

		public FixedNativeList<GroupData> groupList;

		public override void Create()
		{
			triangleDataList = new FixedChunkNativeArray<TriangleRotationData>();
			triangleIndexList = new FixedChunkNativeArray<int>();
			groupList = new FixedNativeList<GroupData>();
		}

		public override void Release()
		{
			triangleDataList.Dispose();
			triangleIndexList.Dispose();
			groupList.Dispose();
		}

		public int AddGroup(int teamId, TriangleRotationData[] dataArray, int[] indexArray)
		{
			if (dataArray == null || dataArray.Length == 0 || indexArray == null || indexArray.Length == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.triangleDataChunk = triangleDataList.AddChunk(dataArray.Length);
			element.triangleIndexChunk = triangleIndexList.AddChunk(indexArray.Length);
			triangleDataList.ToJobArray().CopyFromFast(element.triangleDataChunk.startIndex, dataArray);
			triangleIndexList.ToJobArray().CopyFromFast(element.triangleIndexChunk.startIndex, indexArray);
			return groupList.Add(element);
		}

		public override void RemoveGroup(int teamId)
		{
			int triangleWorkerGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].triangleWorkerGroupIndex;
			if (triangleWorkerGroupIndex >= 0)
			{
				GroupData groupData = groupList[triangleWorkerGroupIndex];
				triangleDataList.RemoveChunk(groupData.triangleDataChunk);
				triangleIndexList.RemoveChunk(groupData.triangleIndexChunk);
				groupList.Remove(triangleWorkerGroupIndex);
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
			TriangleRotationJob jobData = default(TriangleRotationJob);
			jobData.triangleDataList = triangleDataList.ToJobArray();
			jobData.triangleIndexList = triangleIndexList.ToJobArray();
			jobData.groupList = groupList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.rotList = base.Manager.Particle.rotList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			return jobHandle;
		}
	}
}
