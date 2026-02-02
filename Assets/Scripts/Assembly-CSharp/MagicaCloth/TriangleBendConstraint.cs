using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class TriangleBendConstraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct TriangleBendData
		{
			public int vindex0;

			public int vindex1;

			public int vindex2;

			public int vindex3;

			public float restAngle;

			public float depth;

			public int writeIndex0;

			public int writeIndex1;

			public int writeIndex2;

			public int writeIndex3;

			public bool IsValid()
			{
				if (vindex0 > 0)
				{
					return vindex1 > 0;
				}
				return false;
			}
		}

		public struct TriangleBendGroupData
		{
			public int teamId;

			public int active;

			public CurveParam stiffness;

			public ChunkData dataChunk;

			public ChunkData groupIndexChunk;

			public ChunkData refDataChunk;

			public ChunkData writeDataChunk;
		}

		[BurstCompile]
		private struct TriangleBendCalcJob : IJobParallelFor
		{
			public float updatePower;

			[ReadOnly]
			public NativeArray<TriangleBendGroupData> triangleBendGroupDataList;

			[ReadOnly]
			public NativeArray<TriangleBendData> triangleBendList;

			[ReadOnly]
			public NativeArray<short> groupIndexList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> writeBuffer;

			public void Execute(int index)
			{
				TriangleBendData triangleBendData = triangleBendList[index];
				if (!triangleBendData.IsValid())
				{
					return;
				}
				int index2 = groupIndexList[index];
				TriangleBendGroupData triangleBendGroupData = triangleBendGroupDataList[index2];
				if (triangleBendGroupData.teamId == 0 || triangleBendGroupData.active == 0)
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[triangleBendGroupData.teamId];
				if (!teamData.IsActive() || !teamData.IsUpdate())
				{
					return;
				}
				int startIndex = teamData.particleChunk.startIndex;
				float3 value = 0;
				float3 value2 = 0;
				float3 value3 = 0;
				float3 value4 = 0;
				int index3 = triangleBendData.vindex0 + startIndex;
				int index4 = triangleBendData.vindex1 + startIndex;
				int index5 = triangleBendData.vindex2 + startIndex;
				int index6 = triangleBendData.vindex3 + startIndex;
				float3 @float = nextPosList[index3];
				float3 float2 = nextPosList[index4];
				float3 float3 = nextPosList[index5];
				float3 float4 = nextPosList[index6];
				float num = 1f - math.pow(1f - triangleBendGroupData.stiffness.Evaluate(triangleBendData.depth), updatePower);
				float3 float5 = float4 - float3;
				float num2 = math.length(float5);
				if (num2 > 1E-06f)
				{
					float num3 = 1f / num2;
					float3 float6 = math.cross(float3 - @float, float4 - @float);
					float6 /= math.lengthsq(float6);
					float3 float7 = math.cross(float4 - float2, float3 - float2);
					float7 /= math.lengthsq(float7);
					float3 float8 = num2 * float6;
					float3 float9 = num2 * float7;
					float3 float10 = math.dot(@float - float4, float5) * num3 * float6 + math.dot(float2 - float4, float5) * num3 * float7;
					float3 float11 = math.dot(float3 - @float, float5) * num3 * float6 + math.dot(float3 - float2, float5) * num3 * float7;
					float6 = math.normalize(float6);
					float7 = math.normalize(float7);
					float num4 = math.acos(math.clamp(math.dot(float6, float7), -1f, 1f));
					float num5 = math.lengthsq(float8) + math.lengthsq(float9) + math.lengthsq(float10) + math.lengthsq(float11);
					if (num5 != 0f)
					{
						num5 = (num4 - triangleBendData.restAngle) / num5 * num;
						if (math.dot(math.cross(float6, float7), float5) > 0f)
						{
							num5 = 0f - num5;
						}
						value = (0f - num5) * float8;
						value2 = (0f - num5) * float9;
						value3 = (0f - num5) * float10;
						value4 = (0f - num5) * float11;
					}
				}
				int startIndex2 = triangleBendGroupData.writeDataChunk.startIndex;
				int index7 = triangleBendData.writeIndex0 + startIndex2;
				int index8 = triangleBendData.writeIndex1 + startIndex2;
				int index9 = triangleBendData.writeIndex2 + startIndex2;
				int index10 = triangleBendData.writeIndex3 + startIndex2;
				writeBuffer[index7] = value;
				writeBuffer[index8] = value2;
				writeBuffer[index9] = value3;
				writeBuffer[index10] = value4;
			}
		}

		[BurstCompile]
		private struct TriangleBendSumJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<TriangleBendGroupData> triangleBendGroupDataList;

			[ReadOnly]
			public NativeArray<ReferenceDataIndex> refDataList;

			[ReadOnly]
			public NativeArray<float3> writeBuffer;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			public NativeArray<float3> inoutNextPosList;

			public void Execute(int pindex)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[pindex];
				if (!particleFlag.IsValid() || particleFlag.IsFixed())
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[teamIdList[pindex]];
				if (!teamData.IsActive() || teamData.triangleBendGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				TriangleBendGroupData triangleBendGroupData = triangleBendGroupDataList[teamData.triangleBendGroupIndex];
				if (triangleBendGroupData.active == 0)
				{
					return;
				}
				int startIndex = teamData.particleChunk.startIndex;
				int num = pindex - startIndex;
				ReferenceDataIndex referenceDataIndex = refDataList[triangleBendGroupData.refDataChunk.startIndex + num];
				if (referenceDataIndex.count > 0)
				{
					float3 @float = 0;
					int num2 = triangleBendGroupData.writeDataChunk.startIndex + referenceDataIndex.startIndex;
					for (int i = 0; i < referenceDataIndex.count; i++)
					{
						@float += writeBuffer[num2];
						num2++;
					}
					@float /= (float)referenceDataIndex.count;
					inoutNextPosList[pindex] += @float;
				}
			}
		}

		private FixedChunkNativeArray<TriangleBendData> dataList;

		private FixedChunkNativeArray<short> groupIndexList;

		private FixedChunkNativeArray<ReferenceDataIndex> refDataList;

		private FixedChunkNativeArray<float3> writeBuffer;

		private FixedNativeList<TriangleBendGroupData> groupList;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<TriangleBendData>();
			groupIndexList = new FixedChunkNativeArray<short>();
			refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
			writeBuffer = new FixedChunkNativeArray<float3>();
			groupList = new FixedNativeList<TriangleBendGroupData>();
		}

		public override void Release()
		{
			dataList.Dispose();
			groupIndexList.Dispose();
			refDataList.Dispose();
			writeBuffer.Dispose();
			groupList.Dispose();
		}

		public int AddGroup(int teamId, bool active, BezierParam stiffness, TriangleBendData[] dataArray, ReferenceDataIndex[] refDataArray, int writeBufferCount)
		{
			if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0 || writeBufferCount == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			TriangleBendGroupData element = default(TriangleBendGroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.stiffness.Setup(stiffness);
			element.dataChunk = dataList.AddChunk(dataArray.Length);
			element.groupIndexChunk = groupIndexList.AddChunk(dataArray.Length);
			element.refDataChunk = refDataList.AddChunk(refDataArray.Length);
			element.writeDataChunk = writeBuffer.AddChunk(writeBufferCount);
			dataList.ToJobArray().CopyFromFast(element.dataChunk.startIndex, dataArray);
			refDataList.ToJobArray().CopyFromFast(element.refDataChunk.startIndex, refDataArray);
			int num = groupList.Add(element);
			groupIndexList.Fill(element.groupIndexChunk, (short)num);
			return num;
		}

		public override void RemoveTeam(int teamId)
		{
			int triangleBendGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].triangleBendGroupIndex;
			if (triangleBendGroupIndex >= 0)
			{
				TriangleBendGroupData triangleBendGroupData = groupList[triangleBendGroupIndex];
				dataList.RemoveChunk(triangleBendGroupData.dataChunk);
				refDataList.RemoveChunk(triangleBendGroupData.refDataChunk);
				writeBuffer.RemoveChunk(triangleBendGroupData.writeDataChunk);
				groupIndexList.RemoveChunk(triangleBendGroupData.groupIndexChunk);
				groupList.Remove(triangleBendGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, BezierParam stiffness)
		{
			int triangleBendGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].triangleBendGroupIndex;
			if (triangleBendGroupIndex >= 0)
			{
				TriangleBendGroupData value = groupList[triangleBendGroupIndex];
				value.active = (active ? 1 : 0);
				value.stiffness.Setup(stiffness);
				groupList[triangleBendGroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			TriangleBendCalcJob jobData = default(TriangleBendCalcJob);
			jobData.updatePower = updatePower;
			jobData.triangleBendGroupDataList = groupList.ToJobArray();
			jobData.triangleBendList = dataList.ToJobArray();
			jobData.groupIndexList = groupIndexList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.writeBuffer = writeBuffer.ToJobArray();
			jobHandle = jobData.Schedule(dataList.Length, 64, jobHandle);
			TriangleBendSumJob jobData2 = default(TriangleBendSumJob);
			jobData2.triangleBendGroupDataList = groupList.ToJobArray();
			jobData2.refDataList = refDataList.ToJobArray();
			jobData2.writeBuffer = writeBuffer.ToJobArray();
			jobData2.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData2.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData2.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData2.inoutNextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobHandle = jobData2.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			return jobHandle;
		}
	}
}
