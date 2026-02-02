using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class BaseSkinningWorker : PhysicsManagerWorker
	{
		[Serializable]
		public struct BaseSkinningData
		{
			public int boneIndex;

			public float3 localPos;

			public float3 localNor;

			public float3 localTan;

			public float weight;

			public bool IsValid()
			{
				return weight > 0f;
			}
		}

		public struct GroupData
		{
			public int teamId;

			public int active;

			public ChunkData dataChunk;
		}

		[BurstCompile]
		private struct BaseSkinningJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeArray<BaseSkinningData> dataList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<int> transformIndexList;

			[WriteOnly]
			public NativeArray<float3> basePosList;

			[WriteOnly]
			public NativeArray<quaternion> baseRotList;

			[ReadOnly]
			public NativeArray<int> colliderList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<float3> bonePosList;

			[ReadOnly]
			public NativeArray<quaternion> boneRotList;

			[ReadOnly]
			public NativeArray<float3> boneSclList;

			public void Execute(int index)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid() || particleFlag.IsFixed() || particleFlag.IsCollider())
				{
					return;
				}
				int index2 = teamIdList[index];
				PhysicsManagerTeamData.TeamData teamData = teamDataList[index2];
				if (!teamData.IsActive() || teamData.baseSkinningGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				GroupData groupData = groupList[teamData.baseSkinningGroupIndex];
				if (groupData.active == 0)
				{
					return;
				}
				float3 value = 0;
				float3 forward = 0;
				float3 up = 0;
				int num = (index - teamData.particleChunk.startIndex) * 4;
				int num2 = 0;
				while (num2 < 4)
				{
					BaseSkinningData baseSkinningData = dataList[groupData.dataChunk.startIndex + num];
					if (baseSkinningData.IsValid())
					{
						int index3 = colliderList[teamData.colliderChunk.startIndex + baseSkinningData.boneIndex];
						int index4 = transformIndexList[index3];
						float3 @float = bonePosList[index4];
						quaternion q = boneRotList[index4];
						float3 float2 = boneSclList[index4];
						value += (@float + math.mul(q, baseSkinningData.localPos * float2)) * baseSkinningData.weight;
						forward += math.mul(q, baseSkinningData.localNor) * baseSkinningData.weight;
						up += math.mul(q, baseSkinningData.localTan) * baseSkinningData.weight;
					}
					num2++;
					num++;
				}
				basePosList[index] = value;
				baseRotList[index] = quaternion.LookRotation(forward, up);
			}
		}

		private FixedChunkNativeArray<BaseSkinningData> dataList;

		public FixedNativeList<GroupData> groupList;

		public override void Create()
		{
			groupList = new FixedNativeList<GroupData>();
			dataList = new FixedChunkNativeArray<BaseSkinningData>();
		}

		public override void Release()
		{
			groupList.Dispose();
			dataList.Dispose();
		}

		public int AddGroup(int teamId, bool active, BaseSkinningData[] skinningDataList)
		{
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.dataChunk = dataList.AddChunk(skinningDataList.Length);
			dataList.ToJobArray().CopyFromFast(element.dataChunk.startIndex, skinningDataList);
			return groupList.Add(element);
		}

		public override void RemoveGroup(int teamId)
		{
			int baseSkinningGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].baseSkinningGroupIndex;
			if (baseSkinningGroupIndex >= 0)
			{
				GroupData groupData = groupList[baseSkinningGroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				groupList.Remove(baseSkinningGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active)
		{
			int baseSkinningGroupIndex = base.Manager.Team.teamDataList[teamId].baseSkinningGroupIndex;
			if (baseSkinningGroupIndex >= 0)
			{
				GroupData value = groupList[baseSkinningGroupIndex];
				value.active = (active ? 1 : 0);
				groupList[baseSkinningGroupIndex] = value;
			}
		}

		public override void Warmup()
		{
		}

		public override JobHandle PreUpdate(JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			BaseSkinningJob jobData = default(BaseSkinningJob);
			jobData.groupList = groupList.ToJobArray();
			jobData.dataList = dataList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.transformIndexList = base.Manager.Particle.transformIndexList.ToJobArray();
			jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.baseRotList = base.Manager.Particle.baseRotList.ToJobArray();
			jobData.colliderList = base.Manager.Team.colliderList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.bonePosList = base.Manager.Bone.bonePosList.ToJobArray();
			jobData.boneRotList = base.Manager.Bone.boneRotList.ToJobArray();
			jobData.boneSclList = base.Manager.Bone.boneSclList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			return jobHandle;
		}

		public override JobHandle PostUpdate(JobHandle jobHandle)
		{
			return jobHandle;
		}
	}
}
