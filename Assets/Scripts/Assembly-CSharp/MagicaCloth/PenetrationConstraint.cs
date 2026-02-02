using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class PenetrationConstraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct PenetrationData
		{
			public short vertexIndex;

			public short colliderIndex;

			public float3 localPos;

			public float3 localDir;

			public float distance;

			public bool IsValid()
			{
				return vertexIndex >= 0;
			}
		}

		public struct GroupData
		{
			public int teamId;

			public int active;

			public int mode;

			public ChunkData dataChunk;

			public ChunkData refDataChunk;

			public CurveParam radius;

			public CurveParam distance;
		}

		[BurstCompile]
		private struct PenetrationJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeArray<PenetrationData> dataList;

			[ReadOnly]
			public NativeArray<ReferenceDataIndex> refDataList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[ReadOnly]
			public NativeArray<quaternion> nextRotList;

			[ReadOnly]
			public NativeArray<int> transformIndexList;

			[ReadOnly]
			public NativeArray<float> depthList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			[ReadOnly]
			public NativeArray<int> colliderList;

			[ReadOnly]
			public NativeArray<float3> boneSclList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[WriteOnly]
			public NativeArray<float3> outNextPosList;

			public NativeArray<float3> posList;

			public void Execute(int index)
			{
				float3 @float = nextPosList[index];
				outNextPosList[index] = @float;
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid() || particleFlag.IsFixed() || particleFlag.IsCollider())
				{
					return;
				}
				int index2 = teamIdList[index];
				PhysicsManagerTeamData.TeamData teamData = teamDataList[index2];
				if (!teamData.IsActive() || teamData.penetrationGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				GroupData groupData = groupList[teamData.penetrationGroupIndex];
				if (groupData.active == 0)
				{
					return;
				}
				int num = index - teamData.particleChunk.startIndex;
				ReferenceDataIndex referenceDataIndex = refDataList[groupData.refDataChunk.startIndex + num];
				if (referenceDataIndex.count == 0)
				{
					return;
				}
				float t = depthList[index];
				float num2 = groupData.radius.Evaluate(t);
				float num3 = groupData.distance.Evaluate(t);
				float3 scaleDirection = teamData.scaleDirection;
				float scaleRatio = teamData.scaleRatio;
				num3 *= scaleRatio;
				num2 *= scaleRatio;
				if (groupData.mode == 0)
				{
					float3 float2 = basePosList[index];
					quaternion q = baseRotList[index];
					int startIndex = referenceDataIndex.startIndex;
					PenetrationData penetrationData = dataList[groupData.dataChunk.startIndex + startIndex];
					if (penetrationData.IsValid())
					{
						float3 float3 = math.mul(q, penetrationData.localDir * scaleDirection);
						float3 float4 = float2 + float3 * (num3 - num2);
						float3 float5 = @float - float4;
						float num4 = math.length(float5);
						if (num4 > num2)
						{
							float5 *= num2 / num4;
							@float = float4 + float5;
						}
					}
				}
				else if (groupData.mode == 1)
				{
					float3 c = 0;
					int num5 = 0;
					int num6 = referenceDataIndex.startIndex;
					int num7 = 0;
					while (num7 < referenceDataIndex.count)
					{
						PenetrationData data = dataList[groupData.dataChunk.startIndex + num6];
						if (data.IsValid())
						{
							int num8 = colliderList[teamData.colliderChunk.startIndex + data.colliderIndex];
							if (flagList[num8].IsValid())
							{
								c += InverseSpherePosition(ref data, scaleRatio, scaleDirection, num3, num8, num2);
								num5++;
							}
						}
						num7++;
						num6++;
					}
					if (num5 > 0)
					{
						c /= (float)num5;
						float3 float6 = InverseSpherePenetration(c, num2, @float) - @float;
						@float += float6;
					}
				}
				outNextPosList[index] = @float;
			}

			private float3 InverseSpherePosition(ref PenetrationData data, float teamScale, float3 scaleDirection, float distance, int cindex, float cr)
			{
				float3 @float = nextPosList[cindex];
				quaternion q = nextRotList[cindex];
				int index = transformIndexList[cindex];
				float3 float2 = boneSclList[index];
				float3 float3 = math.mul(q, data.localPos * float2) + @float;
				float3 float4 = math.mul(q, data.localDir * scaleDirection);
				return float3 + float4 * (data.distance * teamScale - distance + cr);
			}

			private float3 InverseSpherePenetration(float3 c, float cr, float3 nextpos)
			{
				float3 @float = nextpos - c;
				float num = math.length(@float);
				if (num > cr)
				{
					@float *= cr / num;
					return c + @float;
				}
				return nextpos;
			}
		}

		private FixedChunkNativeArray<PenetrationData> dataList;

		private FixedChunkNativeArray<ReferenceDataIndex> refDataList;

		public FixedNativeList<GroupData> groupList;

		public override void Create()
		{
			groupList = new FixedNativeList<GroupData>();
			dataList = new FixedChunkNativeArray<PenetrationData>();
			refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
		}

		public override void Release()
		{
			groupList.Dispose();
			dataList.Dispose();
			refDataList.Dispose();
		}

		public int AddGroup(int teamId, bool active, ClothParams.PenetrationMode mode, BezierParam distance, BezierParam radius, PenetrationData[] moveLimitDataList, ReferenceDataIndex[] refDataArray)
		{
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.mode = (int)mode;
			element.distance.Setup(distance);
			element.radius.Setup(radius);
			element.dataChunk = dataList.AddChunk(moveLimitDataList.Length);
			element.refDataChunk = refDataList.AddChunk(refDataArray.Length);
			dataList.ToJobArray().CopyFromFast(element.dataChunk.startIndex, moveLimitDataList);
			refDataList.ToJobArray().CopyFromFast(element.refDataChunk.startIndex, refDataArray);
			return groupList.Add(element);
		}

		public override void RemoveTeam(int teamId)
		{
			int penetrationGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].penetrationGroupIndex;
			if (penetrationGroupIndex >= 0)
			{
				GroupData groupData = groupList[penetrationGroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				refDataList.RemoveChunk(groupData.refDataChunk);
				groupList.Remove(penetrationGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, BezierParam distance, BezierParam radius)
		{
			int penetrationGroupIndex = base.Manager.Team.teamDataList[teamId].penetrationGroupIndex;
			if (penetrationGroupIndex >= 0)
			{
				GroupData value = groupList[penetrationGroupIndex];
				value.active = (active ? 1 : 0);
				value.distance.Setup(distance);
				value.radius.Setup(radius);
				groupList[penetrationGroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			PenetrationJob jobData = default(PenetrationJob);
			jobData.groupList = groupList.ToJobArray();
			jobData.dataList = dataList.ToJobArray();
			jobData.refDataList = refDataList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.nextRotList = base.Manager.Particle.InNextRotList.ToJobArray();
			jobData.transformIndexList = base.Manager.Particle.transformIndexList.ToJobArray();
			jobData.depthList = base.Manager.Particle.depthList.ToJobArray();
			jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.baseRotList = base.Manager.Particle.baseRotList.ToJobArray();
			jobData.colliderList = base.Manager.Team.colliderList.ToJobArray();
			jobData.boneSclList = base.Manager.Bone.boneSclList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.outNextPosList = base.Manager.Particle.OutNextPosList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			base.Manager.Particle.SwitchingNextPosList();
			return jobHandle;
		}
	}
}
