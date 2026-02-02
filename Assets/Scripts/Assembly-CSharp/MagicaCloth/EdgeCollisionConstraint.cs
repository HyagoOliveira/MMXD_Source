using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class EdgeCollisionConstraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct EdgeCollisionData
		{
			public ushort vindex0;

			public ushort vindex1;

			public int writeIndex0;

			public int writeIndex1;

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

			public float edgeRadius;

			public ChunkData dataChunk;

			public ChunkData groupIndexChunk;

			public ChunkData refDataChunk;

			public ChunkData writeDataChunk;
		}

		[BurstCompile]
		private struct EdgeCollisionCalcJob : IJobParallelFor
		{
			public float updatePower;

			[ReadOnly]
			public NativeArray<GroupData> groupDataList;

			[ReadOnly]
			public NativeArray<EdgeCollisionData> dataList;

			[ReadOnly]
			public NativeArray<short> groupIndexList;

			[ReadOnly]
			public NativeArray<int> colliderList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float3> radiusList;

			[ReadOnly]
			public NativeArray<float3> posList;

			[ReadOnly]
			public NativeArray<quaternion> rotList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[ReadOnly]
			public NativeArray<quaternion> nextRotList;

			[ReadOnly]
			public NativeArray<float3> localPosList;

			[ReadOnly]
			public NativeArray<int> transformIndexList;

			[ReadOnly]
			public NativeArray<float3> boneSclList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> writeBuffer;

			public void Execute(int index)
			{
				EdgeCollisionData edgeCollisionData = dataList[index];
				if (!edgeCollisionData.IsValid())
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
				float3 corr = 0;
				float3 corr2 = 0;
				int index3 = edgeCollisionData.vindex0 + startIndex;
				int index4 = edgeCollisionData.vindex1 + startIndex;
				float3 nextpos = nextPosList[index3];
				float3 nextpos2 = nextPosList[index4];
				float edgeRadius = groupData.edgeRadius;
				int index5 = 0;
				bool flag = false;
				for (int i = 0; i < 2; i++)
				{
					ChunkData colliderChunk = teamDataList[index5].colliderChunk;
					int num = colliderChunk.startIndex;
					int num2 = 0;
					while (num2 < colliderChunk.useLength)
					{
						int num3 = colliderList[num];
						PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[num3];
						if (particleFlag.IsValid())
						{
							bool flag2 = false;
							if (!particleFlag.IsFlag(32u))
							{
								if (particleFlag.IsFlag(64u))
								{
									flag2 = CapsuleColliderDetection(nextpos, nextpos2, ref corr, ref corr2, edgeRadius, num3, new float3(1f, 0f, 0f));
								}
								else if (particleFlag.IsFlag(128u))
								{
									flag2 = CapsuleColliderDetection(nextpos, nextpos2, ref corr, ref corr2, edgeRadius, num3, new float3(0f, 1f, 0f));
								}
								else if (particleFlag.IsFlag(256u))
								{
									flag2 = CapsuleColliderDetection(nextpos, nextpos2, ref corr, ref corr2, edgeRadius, num3, new float3(0f, 0f, 1f));
								}
								else if (!particleFlag.IsFlag(512u))
								{
									flag2 = SphereColliderDetection(nextpos, nextpos2, ref corr, ref corr2, edgeRadius, num3);
								}
							}
							flag = flag2 || flag;
						}
						num2++;
						num++;
					}
					index5 = groupData.teamId;
				}
				int startIndex2 = groupData.writeDataChunk.startIndex;
				int index6 = edgeCollisionData.writeIndex0 + startIndex2;
				int index7 = edgeCollisionData.writeIndex1 + startIndex2;
				writeBuffer[index6] = corr;
				writeBuffer[index7] = corr2;
			}

			private bool SphereColliderDetection(float3 nextpos0, float3 nextpos1, ref float3 corr0, ref float3 corr1, float radius, int cindex)
			{
				float3 @float = nextPosList[cindex];
				float3 float2 = posList[cindex];
				float3 float3 = radiusList[cindex];
				int index = transformIndexList[cindex];
				float3 *= boneSclList[index].x;
				float3 float4 = math.normalize(MathUtility.ClosestPtPointSegment(float2, nextpos0, nextpos1) - float2);
				float3 planePos = @float + float4 * (float3.x + radius);
				float3 outpos;
				bool flag = MathUtility.IntersectPointPlane(planePos, float4, nextpos0, out outpos);
				float3 outpos2;
				bool flag2 = MathUtility.IntersectPointPlane(planePos, float4, nextpos1, out outpos2);
				if (flag)
				{
					corr0 += outpos - nextpos0;
				}
				if (flag2)
				{
					corr1 += outpos2 - nextpos1;
				}
				return flag || flag2;
			}

			private bool CapsuleColliderDetection(float3 nextpos0, float3 nextpos1, ref float3 corr0, ref float3 corr1, float radius, int cindex, float3 dir)
			{
				float3 @float = nextPosList[cindex];
				quaternion q = nextRotList[cindex];
				float3 float2 = posList[cindex];
				quaternion q2 = rotList[cindex];
				float3 float3 = radiusList[cindex];
				int index = transformIndexList[cindex];
				float num = math.dot(boneSclList[index], dir);
				float3 *= num;
				float3 float4 = math.mul(q2, dir * float3.x);
				float3 p = float2 - float4;
				float3 q3 = float2 + float4;
				float s;
				float t;
				float3 c;
				float3 c2;
				MathUtility.ClosestPtSegmentSegment(p, q3, nextpos0, nextpos1, out s, out t, out c, out c2);
				float3 v = c2 - c;
				float3 float5 = math.mul(q, dir * float3.x);
				float3 x = @float - float5;
				float3 y = @float + float5;
				float y2 = float3.y;
				float z = float3.z;
				float3 v2 = math.mul(math.inverse(q2), v);
				v = math.mul(q, v2);
				float num2 = math.lerp(y2, z, s);
				float3 float6 = math.normalize(v);
				float3 planePos = math.lerp(x, y, s) + float6 * (num2 + radius);
				float3 outpos;
				bool flag = MathUtility.IntersectPointPlane(planePos, float6, nextpos0, out outpos);
				float3 outpos2;
				bool flag2 = MathUtility.IntersectPointPlane(planePos, float6, nextpos1, out outpos2);
				if (flag)
				{
					corr0 += outpos - nextpos0;
				}
				if (flag2)
				{
					corr1 += outpos2 - nextpos1;
				}
				return flag || flag2;
			}
		}

		[BurstCompile]
		private struct EdgeCollisionSumJob : IJobParallelFor
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

			public NativeArray<float> frictionList;

			public void Execute(int pindex)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[pindex];
				if (!particleFlag.IsValid() || particleFlag.IsFixed())
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[teamIdList[pindex]];
				if (!teamData.IsActive() || teamData.edgeCollisionGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				GroupData groupData = groupDataList[teamData.edgeCollisionGroupIndex];
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

		private FixedChunkNativeArray<EdgeCollisionData> dataList;

		private FixedChunkNativeArray<short> groupIndexList;

		private FixedChunkNativeArray<ReferenceDataIndex> refDataList;

		private FixedChunkNativeArray<float3> writeBuffer;

		private FixedNativeList<GroupData> groupList;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<EdgeCollisionData>();
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

		public int AddGroup(int teamId, bool active, float edgeRadius, EdgeCollisionData[] dataArray, ReferenceDataIndex[] refDataArray, int writeBufferCount)
		{
			if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0 || writeBufferCount == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.edgeRadius = edgeRadius;
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
			int edgeCollisionGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].edgeCollisionGroupIndex;
			if (edgeCollisionGroupIndex >= 0)
			{
				GroupData groupData = groupList[edgeCollisionGroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				refDataList.RemoveChunk(groupData.refDataChunk);
				writeBuffer.RemoveChunk(groupData.writeDataChunk);
				groupIndexList.RemoveChunk(groupData.groupIndexChunk);
				groupList.Remove(edgeCollisionGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, float edgeRadius)
		{
			int edgeCollisionGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].edgeCollisionGroupIndex;
			if (edgeCollisionGroupIndex >= 0)
			{
				GroupData value = groupList[edgeCollisionGroupIndex];
				value.active = (active ? 1 : 0);
				value.edgeRadius = edgeRadius;
				groupList[edgeCollisionGroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			EdgeCollisionCalcJob jobData = default(EdgeCollisionCalcJob);
			jobData.updatePower = updatePower;
			jobData.groupDataList = groupList.ToJobArray();
			jobData.dataList = dataList.ToJobArray();
			jobData.groupIndexList = groupIndexList.ToJobArray();
			jobData.colliderList = base.Manager.Team.colliderList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.radiusList = base.Manager.Particle.radiusList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobData.rotList = base.Manager.Particle.rotList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.nextRotList = base.Manager.Particle.InNextRotList.ToJobArray();
			jobData.localPosList = base.Manager.Particle.localPosList.ToJobArray();
			jobData.transformIndexList = base.Manager.Particle.transformIndexList.ToJobArray();
			jobData.boneSclList = base.Manager.Bone.boneSclList.ToJobArray();
			jobData.writeBuffer = writeBuffer.ToJobArray();
			jobHandle = jobData.Schedule(dataList.Length, 64, jobHandle);
			EdgeCollisionSumJob jobData2 = default(EdgeCollisionSumJob);
			jobData2.groupDataList = groupList.ToJobArray();
			jobData2.refDataList = refDataList.ToJobArray();
			jobData2.writeBuffer = writeBuffer.ToJobArray();
			jobData2.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData2.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData2.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData2.inoutNextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData2.frictionList = base.Manager.Particle.frictionList.ToJobArray();
			jobHandle = jobData2.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			return jobHandle;
		}
	}
}
