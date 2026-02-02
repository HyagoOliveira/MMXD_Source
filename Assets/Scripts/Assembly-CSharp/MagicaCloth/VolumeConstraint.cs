using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class VolumeConstraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct VolumeData
		{
			public int vindex0;

			public int vindex1;

			public int vindex2;

			public int vindex3;

			public float3x3 ivMat;

			public float depth;

			public int direction;

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

		public struct GroupData
		{
			public int teamId;

			public int active;

			public CurveParam stretchStiffness;

			public CurveParam shearStiffness;

			public ChunkData dataChunk;

			public ChunkData groupIndexChunk;

			public ChunkData refDataChunk;

			public ChunkData writeDataChunk;
		}

		[BurstCompile]
		private struct VolumeCalcJob : IJobParallelFor
		{
			public float updatePower;

			[ReadOnly]
			public NativeArray<GroupData> groupDataList;

			[ReadOnly]
			public NativeArray<VolumeData> dataList;

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
				VolumeData volumeData = dataList[index];
				if (!volumeData.IsValid())
				{
					return;
				}
				int index2 = groupIndexList[index];
				GroupData groupData = groupDataList[index2];
				if (groupData.teamId == 0 || groupData.active == 0)
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[groupData.teamId];
				if (!teamData.IsActive() || !teamData.IsUpdate())
				{
					return;
				}
				int startIndex = teamData.particleChunk.startIndex;
				float3 @float = 0;
				float3 float2 = 0;
				float3 float3 = 0;
				float3 float4 = 0;
				int index3 = volumeData.vindex0 + startIndex;
				int index4 = volumeData.vindex1 + startIndex;
				int index5 = volumeData.vindex2 + startIndex;
				int index6 = volumeData.vindex3 + startIndex;
				float3 float5 = nextPosList[index3];
				float3 float6 = nextPosList[index4];
				float3 float7 = nextPosList[index5];
				float3 float8 = nextPosList[index6];
				float num = 1f - math.pow(1f - groupData.stretchStiffness.Evaluate(volumeData.depth), updatePower);
				float num2 = 1f - math.pow(1f - groupData.shearStiffness.Evaluate(volumeData.depth), updatePower);
				float3 float9 = 0;
				float3x3 ivMat = volumeData.ivMat;
				float3x3 float3x = 0;
				float3x[0] = ivMat.c0;
				float3x[1] = ivMat.c1;
				float3x[2] = ivMat.c2;
				float3x3 a = default(float3x3);
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j <= i; j++)
					{
						a.c0 = float6 + float2 - (float5 + @float);
						a.c1 = float7 + float3 - (float5 + @float);
						a.c2 = float8 + float4 - (float5 + @float);
						float3 float10 = math.mul(a, float3x[i]);
						float3 float11 = math.mul(a, float3x[j]);
						float num3 = math.dot(float10, float11);
						float3x4 float3x2 = 0;
						float3x2[0] = 0;
						for (int k = 0; k < 3; k++)
						{
							float3x2[k + 1] = float11 * ivMat[i][k] + float10 * ivMat[j][k];
							float3x2[0] -= float3x2[k + 1];
						}
						float num4 = math.lengthsq(float3x2[0]) + math.lengthsq(float3x2[1]) + math.lengthsq(float3x2[2]) + math.lengthsq(float3x2[3]);
						if (!(math.abs(num4) < 1E-06f))
						{
							num4 = ((i != j) ? (num3 / num4 * num2) : ((num3 - 1f) / num4 * num));
							@float -= num4 * float3x2[0];
							float2 -= num4 * float3x2[1];
							float3 -= num4 * float3x2[2];
							float4 -= num4 * float3x2[3];
						}
					}
				}
				int startIndex2 = groupData.writeDataChunk.startIndex;
				int index7 = volumeData.writeIndex0 + startIndex2;
				int index8 = volumeData.writeIndex1 + startIndex2;
				int index9 = volumeData.writeIndex2 + startIndex2;
				int index10 = volumeData.writeIndex3 + startIndex2;
				writeBuffer[index7] = @float;
				writeBuffer[index8] = float2;
				writeBuffer[index9] = float3;
				writeBuffer[index10] = float4 + float9;
			}
		}

		[BurstCompile]
		private struct VolumeSumJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<GroupData> groupDataList;

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
				if (!teamData.IsActive() || teamData.volumeGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				GroupData groupData = groupDataList[teamData.volumeGroupIndex];
				if (groupData.active == 0)
				{
					return;
				}
				int startIndex = teamData.particleChunk.startIndex;
				int num = pindex - startIndex;
				ReferenceDataIndex referenceDataIndex = refDataList[groupData.refDataChunk.startIndex + num];
				if (referenceDataIndex.count > 0)
				{
					float3 @float = 0;
					int num2 = groupData.writeDataChunk.startIndex + referenceDataIndex.startIndex;
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

		private FixedChunkNativeArray<VolumeData> dataList;

		private FixedChunkNativeArray<short> groupIndexList;

		private FixedChunkNativeArray<ReferenceDataIndex> refDataList;

		private FixedChunkNativeArray<float3> writeBuffer;

		private FixedNativeList<GroupData> groupList;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<VolumeData>();
			groupIndexList = new FixedChunkNativeArray<short>();
			refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
			writeBuffer = new FixedChunkNativeArray<float3>();
			groupList = new FixedNativeList<GroupData>();
		}

		public override void Release()
		{
			dataList.Dispose();
			groupIndexList.Dispose();
			refDataList.Dispose();
			writeBuffer.Dispose();
			groupList.Dispose();
		}

		public int AddGroup(int teamId, bool active, BezierParam stretchStiffness, BezierParam shearStiffness, VolumeData[] dataArray, ReferenceDataIndex[] refDataArray, int writeBufferCount)
		{
			if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0 || writeBufferCount == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.stretchStiffness.Setup(stretchStiffness);
			element.shearStiffness.Setup(shearStiffness);
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
			int volumeGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].volumeGroupIndex;
			if (volumeGroupIndex >= 0)
			{
				GroupData groupData = groupList[volumeGroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				refDataList.RemoveChunk(groupData.refDataChunk);
				writeBuffer.RemoveChunk(groupData.writeDataChunk);
				groupIndexList.RemoveChunk(groupData.groupIndexChunk);
				groupList.Remove(volumeGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, BezierParam stretchStiffness, BezierParam shearStiffness)
		{
			int volumeGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].volumeGroupIndex;
			if (volumeGroupIndex >= 0)
			{
				GroupData value = groupList[volumeGroupIndex];
				value.active = (active ? 1 : 0);
				value.stretchStiffness.Setup(stretchStiffness);
				value.shearStiffness.Setup(shearStiffness);
				groupList[volumeGroupIndex] = value;
			}
		}

		public override int GetIterationCount()
		{
			return base.GetIterationCount();
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			VolumeCalcJob jobData = default(VolumeCalcJob);
			jobData.updatePower = updatePower;
			jobData.groupDataList = groupList.ToJobArray();
			jobData.dataList = dataList.ToJobArray();
			jobData.groupIndexList = groupIndexList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.writeBuffer = writeBuffer.ToJobArray();
			jobHandle = jobData.Schedule(dataList.Length, 64, jobHandle);
			VolumeSumJob jobData2 = default(VolumeSumJob);
			jobData2.groupDataList = groupList.ToJobArray();
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
