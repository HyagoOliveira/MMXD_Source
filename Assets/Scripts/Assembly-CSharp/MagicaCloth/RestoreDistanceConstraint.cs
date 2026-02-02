using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class RestoreDistanceConstraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct RestoreDistanceData
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

		public struct RestoreDistanceGroupData
		{
			public int teamId;

			public CurveParam mass;

			public float velocityInfluence;

			public CurveParam structStiffness;

			public ChunkData structDataChunk;

			public ChunkData structRefChunk;

			public int useBend;

			public CurveParam bendStiffness;

			public ChunkData bendDataChunk;

			public ChunkData bendRefChunk;

			public int useNear;

			public CurveParam nearStiffness;

			public ChunkData nearDataChunk;

			public ChunkData nearRefChunk;

			public bool IsValid(int type)
			{
				switch (type)
				{
				case 0:
					return true;
				case 1:
					return useBend == 1;
				default:
					return useNear == 1;
				}
			}

			public CurveParam GetStiffness(int type)
			{
				switch (type)
				{
				case 0:
					return structStiffness;
				case 1:
					return bendStiffness;
				default:
					return nearStiffness;
				}
			}

			public ChunkData GetDataChunk(int type)
			{
				switch (type)
				{
				case 0:
					return structDataChunk;
				case 1:
					return bendDataChunk;
				default:
					return nearDataChunk;
				}
			}

			public ChunkData GetRefChunk(int type)
			{
				switch (type)
				{
				case 0:
					return structRefChunk;
				case 1:
					return bendRefChunk;
				default:
					return nearRefChunk;
				}
			}
		}

		[BurstCompile]
		private struct DistanceJob : IJobParallelFor
		{
			public float updatePower;

			public int type;

			[ReadOnly]
			public NativeArray<RestoreDistanceData> restoreDistanceDataList;

			[ReadOnly]
			public NativeArray<RestoreDistanceGroupData> restoreDistanceGroupDataList;

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
			public NativeArray<float> frictionList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

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
				if (teamData.restoreDistanceGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				int startIndex = teamData.particleChunk.startIndex;
				int num = index - startIndex;
				RestoreDistanceGroupData restoreDistanceGroupData = restoreDistanceGroupDataList[teamData.restoreDistanceGroupIndex];
				if (!restoreDistanceGroupData.IsValid(type))
				{
					return;
				}
				ChunkData dataChunk = restoreDistanceGroupData.GetDataChunk(type);
				ChunkData refChunk = restoreDistanceGroupData.GetRefChunk(type);
				CurveParam stiffness = restoreDistanceGroupData.GetStiffness(type);
				float num2 = frictionList[index];
				ReferenceDataIndex referenceDataIndex = refDataList[refChunk.startIndex + num];
				if (referenceDataIndex.count <= 0)
				{
					return;
				}
				float t = depthList[index];
				float num3 = stiffness.Evaluate(t);
				num3 = math.saturate(num3 * updatePower);
				float3 float2 = 0;
				float num4 = restoreDistanceGroupData.mass.Evaluate(t);
				num4 += num2 * 20f;
				float3 float7 = basePosList[index];
				int num5 = dataChunk.startIndex + referenceDataIndex.startIndex;
				int num6 = 0;
				while (num6 < referenceDataIndex.count)
				{
					RestoreDistanceData restoreDistanceData = restoreDistanceDataList[num5];
					if (restoreDistanceData.IsValid())
					{
						int index2 = startIndex + restoreDistanceData.targetVertexIndex;
						float3 float3 = nextPosList[index2] - @float;
						float num7 = math.length(float3);
						if (!(num7 < 1E-05f))
						{
							float length = restoreDistanceData.length;
							length *= teamData.scaleRatio;
							float num8 = num7 - length;
							float t2 = depthList[index2];
							float num9 = restoreDistanceGroupData.mass.Evaluate(t2);
							float num10 = frictionList[index2];
							float num11 = num9 + num10 * 20f;
							float num12 = num11 / (num11 + num4);
							num12 *= num3;
							float3 float4 = float3 * (num12 * num8 / num7);
							float2 += float4;
						}
					}
					num6++;
					num5++;
				}
				float3 float5 = @float;
				@float += float2 / referenceDataIndex.count;
				outNextPosList[index] = @float;
				float3 float6 = (@float - float5) * (1f - restoreDistanceGroupData.velocityInfluence);
				posList[index] += float6;
			}
		}

		public const int StructType = 0;

		public const int BendType = 1;

		public const int NearType = 2;

		public const int TypeCount = 3;

		private FixedChunkNativeArray<RestoreDistanceData>[] dataList;

		private FixedChunkNativeArray<ReferenceDataIndex>[] refDataList;

		public FixedNativeList<RestoreDistanceGroupData> groupList;

		public override void Create()
		{
			groupList = new FixedNativeList<RestoreDistanceGroupData>();
			dataList = new FixedChunkNativeArray<RestoreDistanceData>[3];
			refDataList = new FixedChunkNativeArray<ReferenceDataIndex>[3];
			for (int i = 0; i < 3; i++)
			{
				dataList[i] = new FixedChunkNativeArray<RestoreDistanceData>();
				refDataList[i] = new FixedChunkNativeArray<ReferenceDataIndex>();
			}
		}

		public override void Release()
		{
			groupList.Dispose();
			for (int i = 0; i < 3; i++)
			{
				dataList[i].Dispose();
				refDataList[i].Dispose();
			}
			dataList = null;
			refDataList = null;
		}

		public int AddGroup(int teamId, BezierParam mass, float velocityInfluence, BezierParam structStiffness, RestoreDistanceData[] structDataArray, ReferenceDataIndex[] structRefDataArray, bool useBend, BezierParam bendStiffness, RestoreDistanceData[] bendDataArray, ReferenceDataIndex[] bendRefDataArray, bool useNear, BezierParam nearStiffness, RestoreDistanceData[] nearDataArray, ReferenceDataIndex[] nearRefDataArray)
		{
			if (structDataArray == null || structDataArray.Length == 0 || structRefDataArray == null || structRefDataArray.Length == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			RestoreDistanceGroupData element = default(RestoreDistanceGroupData);
			element.teamId = teamId;
			element.mass.Setup(mass);
			element.velocityInfluence = velocityInfluence;
			element.useBend = (useBend ? 1 : 0);
			element.useNear = (useNear ? 1 : 0);
			element.structStiffness.Setup(structStiffness);
			element.structDataChunk = dataList[0].AddChunk(structDataArray.Length);
			element.structRefChunk = refDataList[0].AddChunk(structRefDataArray.Length);
			dataList[0].ToJobArray().CopyFromFast(element.structDataChunk.startIndex, structDataArray);
			refDataList[0].ToJobArray().CopyFromFast(element.structRefChunk.startIndex, structRefDataArray);
			if (bendDataArray != null && bendDataArray.Length != 0)
			{
				element.bendStiffness.Setup(bendStiffness);
				element.bendDataChunk = dataList[1].AddChunk(bendDataArray.Length);
				element.bendRefChunk = refDataList[1].AddChunk(bendRefDataArray.Length);
				dataList[1].ToJobArray().CopyFromFast(element.bendDataChunk.startIndex, bendDataArray);
				refDataList[1].ToJobArray().CopyFromFast(element.bendRefChunk.startIndex, bendRefDataArray);
			}
			if (nearDataArray != null && nearDataArray.Length != 0)
			{
				element.nearStiffness.Setup(nearStiffness);
				element.nearDataChunk = dataList[2].AddChunk(nearDataArray.Length);
				element.nearRefChunk = refDataList[2].AddChunk(nearRefDataArray.Length);
				dataList[2].ToJobArray().CopyFromFast(element.nearDataChunk.startIndex, nearDataArray);
				refDataList[2].ToJobArray().CopyFromFast(element.nearRefChunk.startIndex, nearRefDataArray);
			}
			return groupList.Add(element);
		}

		public override void RemoveTeam(int teamId)
		{
			int restoreDistanceGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].restoreDistanceGroupIndex;
			if (restoreDistanceGroupIndex < 0)
			{
				return;
			}
			RestoreDistanceGroupData restoreDistanceGroupData = groupList[restoreDistanceGroupIndex];
			for (int i = 0; i < 3; i++)
			{
				ChunkData dataChunk = restoreDistanceGroupData.GetDataChunk(i);
				ChunkData refChunk = restoreDistanceGroupData.GetRefChunk(i);
				if (dataChunk.dataLength > 0)
				{
					dataList[i].RemoveChunk(dataChunk);
				}
				if (refChunk.dataLength > 0)
				{
					refDataList[i].RemoveChunk(refChunk);
				}
			}
			groupList.Remove(restoreDistanceGroupIndex);
		}

		public void ChangeParam(int teamId, BezierParam mass, float velocityInfluence, BezierParam structStiffness, bool useBend, BezierParam bendStiffness, bool useNear, BezierParam nearStiffness)
		{
			int restoreDistanceGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].restoreDistanceGroupIndex;
			if (restoreDistanceGroupIndex >= 0)
			{
				RestoreDistanceGroupData value = groupList[restoreDistanceGroupIndex];
				value.mass.Setup(mass);
				value.velocityInfluence = velocityInfluence;
				value.structStiffness.Setup(structStiffness);
				value.bendStiffness.Setup(bendStiffness);
				value.nearStiffness.Setup(nearStiffness);
				value.useBend = (useBend ? 1 : 0);
				value.useNear = (useNear ? 1 : 0);
				groupList[restoreDistanceGroupIndex] = value;
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
			for (int num = 2; num >= 0; num--)
			{
				if (dataList[num].Count != 0)
				{
					DistanceJob jobData = default(DistanceJob);
					jobData.updatePower = updatePower;
					jobData.type = num;
					jobData.restoreDistanceDataList = dataList[num].ToJobArray();
					jobData.restoreDistanceGroupDataList = groupList.ToJobArray();
					jobData.refDataList = refDataList[num].ToJobArray();
					jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
					jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
					jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
					jobData.depthList = base.Manager.Particle.depthList.ToJobArray();
					jobData.frictionList = base.Manager.Particle.frictionList.ToJobArray();
					jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
					jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
					jobData.outNextPosList = base.Manager.Particle.OutNextPosList.ToJobArray();
					jobData.posList = base.Manager.Particle.posList.ToJobArray();
					jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
					base.Manager.Particle.SwitchingNextPosList();
				}
			}
			return jobHandle;
		}
	}
}
