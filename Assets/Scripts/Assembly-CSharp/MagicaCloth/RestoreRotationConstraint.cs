using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class RestoreRotationConstraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct RotationData
		{
			public ushort vertexIndex;

			public ushort targetVertexIndex;

			public float3 localPos;

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

			public CurveParam restorePower;

			public float velocityInfluence;

			public ChunkData dataChunk;

			public ChunkData refChunk;
		}

		[BurstCompile]
		private struct RotationJob : IJobParallelFor
		{
			public float updatePower;

			[ReadOnly]
			public NativeArray<RotationData> dataList;

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
			public NativeArray<float> depthList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[ReadOnly]
			public NativeArray<float> frictionList;

			[WriteOnly]
			public NativeArray<float3> outNextPosList;

			public NativeArray<float3> posList;

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
				if (!teamData.IsActive() || teamData.restoreRotationGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				int startIndex = teamData.particleChunk.startIndex;
				int num = index - startIndex;
				GroupData groupData = groupList[teamData.restoreRotationGroupIndex];
				if (groupData.active == 0)
				{
					return;
				}
				ReferenceDataIndex referenceDataIndex = refDataList[groupData.refChunk.startIndex + num];
				if (referenceDataIndex.count <= 0)
				{
					return;
				}
				float t = depthList[index];
				float num2 = groupData.restorePower.Evaluate(t);
				float t2 = 1f - math.pow(1f - num2, updatePower);
				float3 float2 = 0;
				int num3 = groupData.dataChunk.startIndex + referenceDataIndex.startIndex;
				int num4 = 0;
				while (num4 < referenceDataIndex.count)
				{
					RotationData rotationData = dataList[num3];
					if (rotationData.IsValid())
					{
						int index2 = startIndex + rotationData.targetVertexIndex;
						quaternion q = baseRotList[index2];
						float3 float3 = nextPosList[index2];
						float3 to = math.mul(q, rotationData.localPos * teamData.scaleDirection);
						float3 float4 = @float - float3;
						float4 = math.mul(MathUtility.FromToRotation(float4, to, t2), float4);
						float3 float5 = float3 + float4;
						float2 += float5 - @float;
					}
					num4++;
					num3++;
				}
				float num5 = frictionList[index];
				float num6 = math.saturate(1f - num5 * 1f);
				float2 *= num6;
				float3 float6 = @float;
				@float += float2 / referenceDataIndex.count;
				outNextPosList[index] = @float;
				float3 float7 = (@float - float6) * (1f - groupData.velocityInfluence);
				posList[index] += float7;
			}
		}

		private FixedChunkNativeArray<RotationData> dataList;

		private FixedChunkNativeArray<ReferenceDataIndex> refDataList;

		public FixedNativeList<GroupData> groupList;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<RotationData>();
			refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
			groupList = new FixedNativeList<GroupData>();
		}

		public override void Release()
		{
			dataList.Dispose();
			refDataList.Dispose();
			groupList.Dispose();
		}

		public int AddGroup(int teamId, bool active, BezierParam power, float velocityInfluence, RotationData[] dataArray, ReferenceDataIndex[] refDataArray)
		{
			if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.restorePower.Setup(power);
			element.velocityInfluence = velocityInfluence;
			element.dataChunk = dataList.AddChunk(dataArray.Length);
			element.refChunk = refDataList.AddChunk(refDataArray.Length);
			dataList.ToJobArray().CopyFromFast(element.dataChunk.startIndex, dataArray);
			refDataList.ToJobArray().CopyFromFast(element.refChunk.startIndex, refDataArray);
			return groupList.Add(element);
		}

		public override void RemoveTeam(int teamId)
		{
			int restoreRotationGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].restoreRotationGroupIndex;
			if (restoreRotationGroupIndex >= 0)
			{
				GroupData groupData = groupList[restoreRotationGroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				refDataList.RemoveChunk(groupData.refChunk);
				groupList.Remove(restoreRotationGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, BezierParam power, float velocityInfluence)
		{
			int restoreRotationGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].restoreRotationGroupIndex;
			if (restoreRotationGroupIndex >= 0)
			{
				GroupData value = groupList[restoreRotationGroupIndex];
				value.active = (active ? 1 : 0);
				value.restorePower.Setup(power);
				value.velocityInfluence = velocityInfluence;
				groupList[restoreRotationGroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			RotationJob jobData = default(RotationJob);
			jobData.updatePower = updatePower;
			jobData.dataList = dataList.ToJobArray();
			jobData.groupList = groupList.ToJobArray();
			jobData.refDataList = refDataList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.depthList = base.Manager.Particle.depthList.ToJobArray();
			jobData.baseRotList = base.Manager.Particle.baseRotList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.frictionList = base.Manager.Particle.frictionList.ToJobArray();
			jobData.outNextPosList = base.Manager.Particle.OutNextPosList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			base.Manager.Particle.SwitchingNextPosList();
			return jobHandle;
		}
	}
}
