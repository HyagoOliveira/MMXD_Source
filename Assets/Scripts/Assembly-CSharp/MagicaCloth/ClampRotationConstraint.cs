using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class ClampRotationConstraint : PhysicsManagerConstraint
	{
		[Serializable]
		public struct ClampRotationData
		{
			public int vertexIndex;

			public int parentVertexIndex;

			public float3 localPos;

			public quaternion localRot;

			public bool IsValid()
			{
				if (vertexIndex <= 0)
				{
					return parentVertexIndex > 0;
				}
				return true;
			}
		}

		[Serializable]
		public struct ClampRotationRootInfo
		{
			public ushort startIndex;

			public ushort dataLength;
		}

		public struct GroupData
		{
			public int teamId;

			public int active;

			public CurveParam maxAngle;

			public float velocityInfluence;

			public ChunkData dataChunk;

			public ChunkData rootInfoChunk;
		}

		[BurstCompile]
		private struct ClampRotationJob : IJobParallelFor
		{
			public float maxMoveLength;

			[ReadOnly]
			public NativeArray<ClampRotationData> dataList;

			[ReadOnly]
			public NativeArray<ClampRotationRootInfo> rootInfoList;

			[ReadOnly]
			public NativeArray<int> rootTeamList;

			[ReadOnly]
			public NativeArray<GroupData> groupList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<float> depthList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float> frictionList;

			[NativeDisableParallelForRestriction]
			public NativeArray<float3> nextPosList;

			[NativeDisableParallelForRestriction]
			public NativeArray<quaternion> nextRotList;

			[NativeDisableParallelForRestriction]
			public NativeArray<float3> posList;

			[NativeDisableParallelForRestriction]
			public NativeArray<float> lengthBuffer;

			public void Execute(int rootIndex)
			{
				int num = rootTeamList[rootIndex];
				if (num == 0)
				{
					return;
				}
				PhysicsManagerTeamData.TeamData teamData = teamDataList[num];
				if (!teamData.IsActive() || teamData.clampRotationGroupIndex < 0 || !teamData.IsUpdate())
				{
					return;
				}
				GroupData groupData = groupList[teamData.clampRotationGroupIndex];
				if (groupData.active == 0)
				{
					return;
				}
				ClampRotationRootInfo clampRotationRootInfo = rootInfoList[rootIndex];
				int num2 = clampRotationRootInfo.startIndex + groupData.dataChunk.startIndex;
				int dataLength = clampRotationRootInfo.dataLength;
				int startIndex = teamData.particleChunk.startIndex;
				for (int i = 0; i < dataLength; i++)
				{
					ClampRotationData clampRotationData = dataList[num2 + i];
					int parentVertexIndex = clampRotationData.parentVertexIndex;
					if (parentVertexIndex >= 0)
					{
						int vertexIndex = clampRotationData.vertexIndex;
						vertexIndex += startIndex;
						parentVertexIndex += startIndex;
						float3 x = nextPosList[vertexIndex];
						float3 y = nextPosList[parentVertexIndex];
						float value = math.distance(x, y);
						lengthBuffer[num2 + i] = value;
					}
				}
				for (int j = 0; j < dataLength; j++)
				{
					ClampRotationData clampRotationData2 = dataList[num2 + j];
					int parentVertexIndex2 = clampRotationData2.parentVertexIndex;
					if (parentVertexIndex2 < 0)
					{
						continue;
					}
					int vertexIndex2 = clampRotationData2.vertexIndex;
					vertexIndex2 += startIndex;
					parentVertexIndex2 += startIndex;
					PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[vertexIndex2];
					if (!particleFlag.IsValid())
					{
						continue;
					}
					float3 @float = nextPosList[vertexIndex2];
					quaternion quaternion = nextRotList[vertexIndex2];
					float3 float2 = @float;
					float3 float3 = nextPosList[parentVertexIndex2];
					quaternion quaternion2 = nextRotList[parentVertexIndex2];
					float t = depthList[vertexIndex2];
					float3 float4 = math.mul(quaternion2, clampRotationData2.localPos * teamData.scaleDirection);
					float x2 = math.distance(@float, float3);
					float num3 = lengthBuffer[num2 + j];
					x2 = math.clamp(x2, 0f, num3 * 1.2f);
					float3 outdir = math.normalize(@float - float3);
					float x3 = groupData.maxAngle.Evaluate(t);
					x3 = math.radians(x3);
					float num4 = math.acos(math.dot(outdir, float4));
					if (!particleFlag.IsFixed())
					{
						if (num4 > x3)
						{
							MathUtility.ClampAngle(outdir, float4, x3, out outdir);
						}
						float3 v = float3 + outdir * x2 - @float;
						v = MathUtility.ClampVector(v, 0f, maxMoveLength);
						float3 y2 = @float + v;
						float num5 = frictionList[vertexIndex2];
						float s = math.saturate(1f - num5 * 1f);
						@float = math.lerp(@float, y2, s);
						nextPosList[vertexIndex2] = @float;
						outdir = math.normalize(@float - float3);
						float3 float5 = (@float - float2) * (1f - groupData.velocityInfluence);
						posList[vertexIndex2] += float5;
					}
					quaternion = math.mul(quaternion2, new quaternion(clampRotationData2.localRot.value * teamData.quaternionScale));
					quaternion = math.mul(MathUtility.FromToRotation(float4, outdir), quaternion);
					nextRotList[vertexIndex2] = quaternion;
				}
			}
		}

		private FixedChunkNativeArray<ClampRotationData> dataList;

		private FixedChunkNativeArray<ClampRotationRootInfo> rootInfoList;

		public FixedNativeList<GroupData> groupList;

		private FixedChunkNativeArray<int> rootTeamList;

		private FixedChunkNativeArray<float> lengthBuffer;

		public override void Create()
		{
			dataList = new FixedChunkNativeArray<ClampRotationData>();
			rootInfoList = new FixedChunkNativeArray<ClampRotationRootInfo>();
			groupList = new FixedNativeList<GroupData>();
			rootTeamList = new FixedChunkNativeArray<int>();
			lengthBuffer = new FixedChunkNativeArray<float>();
		}

		public override void Release()
		{
			dataList.Dispose();
			rootInfoList.Dispose();
			groupList.Dispose();
			rootTeamList.Dispose();
			lengthBuffer.Dispose();
		}

		public int AddGroup(int teamId, bool active, BezierParam maxAngle, float velocityInfluence, ClampRotationData[] dataArray, ClampRotationRootInfo[] rootInfoArray)
		{
			if (dataArray == null || dataArray.Length == 0 || rootInfoArray == null || rootInfoArray.Length == 0)
			{
				return -1;
			}
			PhysicsManagerTeamData.TeamData teamDatum = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId];
			GroupData element = default(GroupData);
			element.teamId = teamId;
			element.active = (active ? 1 : 0);
			element.maxAngle.Setup(maxAngle);
			element.velocityInfluence = velocityInfluence;
			element.dataChunk = dataList.AddChunk(dataArray.Length);
			element.rootInfoChunk = rootInfoList.AddChunk(rootInfoArray.Length);
			dataList.ToJobArray().CopyFromFast(element.dataChunk.startIndex, dataArray);
			rootInfoList.ToJobArray().CopyFromFast(element.rootInfoChunk.startIndex, rootInfoArray);
			int result = groupList.Add(element);
			ChunkData chunk = rootTeamList.AddChunk(rootInfoArray.Length);
			rootTeamList.Fill(chunk, teamId);
			lengthBuffer.AddChunk(dataArray.Length);
			return result;
		}

		public override void RemoveTeam(int teamId)
		{
			int clampRotationGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].clampRotationGroupIndex;
			if (clampRotationGroupIndex >= 0)
			{
				GroupData groupData = groupList[clampRotationGroupIndex];
				dataList.RemoveChunk(groupData.dataChunk);
				rootInfoList.RemoveChunk(groupData.rootInfoChunk);
				rootTeamList.RemoveChunk(groupData.rootInfoChunk);
				lengthBuffer.RemoveChunk(groupData.dataChunk);
				groupList.Remove(clampRotationGroupIndex);
			}
		}

		public void ChangeParam(int teamId, bool active, BezierParam maxAngle, float velocityInfluence)
		{
			int clampRotationGroupIndex = CreateSingleton<MagicaPhysicsManager>.Instance.Team.teamDataList[teamId].clampRotationGroupIndex;
			if (clampRotationGroupIndex >= 0)
			{
				GroupData value = groupList[clampRotationGroupIndex];
				value.active = (active ? 1 : 0);
				value.maxAngle.Setup(maxAngle);
				value.velocityInfluence = velocityInfluence;
				groupList[clampRotationGroupIndex] = value;
			}
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (groupList.Count == 0)
			{
				return jobHandle;
			}
			ClampRotationJob jobData = default(ClampRotationJob);
			jobData.maxMoveLength = dtime * 1f;
			jobData.dataList = dataList.ToJobArray();
			jobData.rootInfoList = rootInfoList.ToJobArray();
			jobData.rootTeamList = rootTeamList.ToJobArray();
			jobData.groupList = groupList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.depthList = base.Manager.Particle.depthList.ToJobArray();
			jobData.frictionList = base.Manager.Particle.frictionList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.nextRotList = base.Manager.Particle.InNextRotList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobData.lengthBuffer = lengthBuffer.ToJobArray();
			jobHandle = jobData.Schedule(rootTeamList.Length, 8, jobHandle);
			return jobHandle;
		}
	}
}
