using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class AdjustRotationWorker : PhysicsManagerWorker
	{
		[Serializable]
		public struct AdjustRotationData
		{
			public int keyIndex;

			public int targetIndex;

			public float3 localPos;

			public bool IsValid()
			{
				if (keyIndex == 0)
				{
					return targetIndex != 0;
				}
				return true;
			}
		}

		public struct GroupData
		{
			public int teamId;

			public int active;

			public int adjustMode;

			public float3 axisRotationPower;

			public ChunkData chunk;
		}

		[BurstCompile]
		private struct AdjustRotationJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<AdjustRotationData> dataList;

			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> particleMap;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			[ReadOnly]
			public NativeArray<float3> posList;

			[WriteOnly]
			public NativeArray<quaternion> rotList;

			public void Execute(int index)
			{
				if (!flagList[index].IsValid())
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[teamIdList[index]];
				if (!teamData.IsActive() || teamData.adjustRotationGroupIndex < 0)
				{
					return;
				}
				GroupData groupData = groupList[teamData.adjustRotationGroupIndex];
				if (groupData.active == 0)
				{
					return;
				}
				quaternion quaternion = baseRotList[index];
				quaternion quaternion2 = quaternion;
				float3 @float = posList[index];
				if (groupData.adjustMode != 0)
				{
					float3 v = @float - basePosList[index];
					v /= teamData.scaleRatio;
					v = math.mul(math.inverse(quaternion), v);
					v *= groupData.axisRotationPower;
					quaternion b = quaternion.identity;
					if (groupData.adjustMode == 1)
					{
						b = quaternion.EulerZXY(0f - v.y, v.x, 0f);
					}
					else if (groupData.adjustMode == 2)
					{
						b = quaternion.EulerZXY(v.z, 0f, 0f - v.x);
					}
					else if (groupData.adjustMode == 3)
					{
						b = quaternion.EulerZXY(0f, v.z, 0f - v.y);
					}
					quaternion2 = math.mul(quaternion2, b);
					quaternion2 = math.normalize(quaternion2);
				}
				rotList[index] = quaternion2;
			}
		}

		private const int AdjustMode_Fixed = 0;

		private const int AdjustMode_XYMove = 1;

		private const int AdjustMode_XZMove = 2;

		private const int AdjustMode_YZMove = 3;

		private FixedChunkNativeArray<AdjustRotationData> dataList;

		public FixedNativeList<GroupData> groupList;

		private ExNativeMultiHashMap<int, int> particleMap;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<AdjustRotationData>();
			groupList = new FixedNativeList<GroupData>();
			particleMap = new ExNativeMultiHashMap<int, int>();
		}

		public override void Release()
		{
			dataList.Dispose();
			groupList.Dispose();
			particleMap.Dispose();
		}

		public int AddGroup(int teamId, bool active, int adjustMode, float3 axisRotationPower, AdjustRotationData[] dataArray)
		{
			PhysicsManagerTeamData.TeamData teamData = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.adjustMode = adjustMode;
			element.axisRotationPower = axisRotationPower;
			if (dataArray != null && dataArray.Length != 0)
			{
				ChunkData chunkData = (element.chunk = dataList.AddChunk(dataArray.Length));
				dataList.ToJobArray().CopyFromFast(chunkData.startIndex, dataArray);
				int startIndex = teamData.particleChunk.startIndex;
				for (int i = 0; i < dataArray.Length; i++)
				{
					AdjustRotationData adjustRotationData = dataArray[i];
					int value = chunkData.startIndex + i;
					particleMap.Add(startIndex + adjustRotationData.keyIndex, value);
				}
			}
			return groupList.Add(element);
		}

		public override void RemoveGroup(int teamId)
		{
			PhysicsManagerTeamData.TeamData teamData = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			int adjustRotationGroupIndex = teamData.adjustRotationGroupIndex;
			if (adjustRotationGroupIndex < 0)
			{
				return;
			}
			GroupData groupData = groupList[adjustRotationGroupIndex];
			if (groupData.chunk.dataLength > 0)
			{
				int startIndex = groupData.chunk.startIndex;
				int startIndex2 = teamData.particleChunk.startIndex;
				for (int i = 0; i < groupData.chunk.dataLength; i++)
				{
					int num = startIndex + i;
					AdjustRotationData adjustRotationData = dataList[num];
					particleMap.Remove(startIndex2 + adjustRotationData.keyIndex, num);
				}
				dataList.RemoveChunk(groupData.chunk);
			}
			groupList.Remove(adjustRotationGroupIndex);
		}

		public void ChangeParam(int teamId, bool active, int adjustMode, float3 axisRotationPower)
		{
			int adjustRotationGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].adjustRotationGroupIndex;
			if (adjustRotationGroupIndex >= 0)
			{
				GroupData value = groupList[adjustRotationGroupIndex];
				value.active = (active ? 1 : 0);
				value.adjustMode = adjustMode;
				value.axisRotationPower = axisRotationPower;
				groupList[adjustRotationGroupIndex] = value;
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
			AdjustRotationJob jobData = default(AdjustRotationJob);
			jobData.dataList = dataList.ToJobArray();
			jobData.groupList = groupList.ToJobArray();
			jobData.particleMap = particleMap.Map;
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.baseRotList = base.Manager.Particle.baseRotList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobData.rotList = base.Manager.Particle.rotList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			return jobHandle;
		}
	}
}
