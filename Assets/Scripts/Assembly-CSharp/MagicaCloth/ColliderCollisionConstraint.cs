using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class ColliderCollisionConstraint : PhysicsManagerConstraint
	{
		[BurstCompile]
		private struct CollisionJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<float3> radiusList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[ReadOnly]
			public NativeArray<quaternion> nextRotList;

			[ReadOnly]
			public NativeArray<float3> posList;

			[ReadOnly]
			public NativeArray<quaternion> rotList;

			[ReadOnly]
			public NativeArray<float3> localPosList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			[ReadOnly]
			public NativeArray<int> transformIndexList;

			[ReadOnly]
			public NativeArray<int> colliderList;

			[ReadOnly]
			public NativeArray<float3> boneSclList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[WriteOnly]
			public NativeArray<float3> outNextPosList;

			public NativeArray<float> frictionList;

			[WriteOnly]
			public NativeArray<int> collisionLinkIdList;

			[WriteOnly]
			public NativeArray<float> collisionDistList;

			public void Execute(int index)
			{
				float3 nextpos = nextPosList[index];
				outNextPosList[index] = nextpos;
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid() || particleFlag.IsFixed() || particleFlag.IsCollider())
				{
					return;
				}
				int num = teamIdList[index];
				PhysicsManagerTeamData.TeamData teamData = teamDataList[num];
				if (!teamData.IsActive() || !teamData.IsFlag(32u) || !teamData.IsUpdate())
				{
					return;
				}
				float x = radiusList[index].x;
				float3 basepos = basePosList[index];
				x *= teamData.scaleRatio;
				int index2 = 0;
				float num2 = 100f;
				int num3 = 0;
				for (int i = 0; i < 2; i++)
				{
					ChunkData colliderChunk = teamDataList[index2].colliderChunk;
					bool keep = i > 0 && teamData.IsFlag(262144u);
					int num4 = colliderChunk.startIndex;
					int num5 = 0;
					while (num5 < colliderChunk.useLength)
					{
						int num6 = colliderList[num4];
						PhysicsManagerParticleData.ParticleFlag particleFlag2 = flagList[num6];
						if (particleFlag2.IsValid())
						{
							float num7 = 100f;
							if (particleFlag2.IsFlag(32u))
							{
								num7 = PlaneColliderDetection(ref nextpos, x, num6);
							}
							else if (particleFlag2.IsFlag(64u))
							{
								num7 = CapsuleColliderDetection(ref nextpos, basepos, x, num6, new float3(1f, 0f, 0f), keep);
							}
							else if (particleFlag2.IsFlag(128u))
							{
								num7 = CapsuleColliderDetection(ref nextpos, basepos, x, num6, new float3(0f, 1f, 0f), keep);
							}
							else if (particleFlag2.IsFlag(256u))
							{
								num7 = CapsuleColliderDetection(ref nextpos, basepos, x, num6, new float3(0f, 0f, 1f), keep);
							}
							else if (!particleFlag2.IsFlag(512u))
							{
								num7 = SphereColliderDetection(ref nextpos, basepos, x, num6, keep);
							}
							if (num7 < num2 && num7 <= 0.02f)
							{
								num3 = num6;
							}
							num2 = math.min(num2, num7);
						}
						num5++;
						num4++;
					}
					if (num <= 0)
					{
						break;
					}
					index2 = num;
				}
				if (num2 < 0.03f)
				{
					float x2 = math.max(1f - num2 * 33.333336f, 0f);
					x2 = math.pow(x2, (num2 > 0f) ? 4f : 1f);
					x2 *= teamData.friction;
					frictionList[index] = math.max(x2, frictionList[index]);
				}
				if (num3 > 0)
				{
					collisionLinkIdList[index] = num3;
					collisionDistList[index] = num2;
				}
				outNextPosList[index] = nextpos;
			}

			private float SphereColliderDetection(ref float3 nextpos, float3 basepos, float radius, int cindex, bool keep)
			{
				float3 @float = nextPosList[cindex];
				float3 float2 = radiusList[cindex];
				int index = transformIndexList[cindex];
				float2 *= math.abs(boneSclList[index].x);
				float3 float7 = (float3)0;
				float3 float3 = 0;
				float3 float4 = 0;
				if (keep)
				{
					float3 float5 = basePosList[cindex];
					float4 = math.mul(v: math.mul(v: basepos - float5, q: math.inverse(baseRotList[cindex])), q: nextRotList[cindex]);
				}
				else
				{
					float3 float6 = posList[cindex];
					float4 = nextpos - float6;
				}
				float3 = math.normalize(float4);
				return MathUtility.IntersectPointPlaneDist(@float + float3 * (float2.x + radius), float3, nextpos, out nextpos);
			}

			private float CapsuleColliderDetection(ref float3 nextpos, float3 basepos, float radius, int cindex, float3 dir, bool keep)
			{
				float3 @float = nextPosList[cindex];
				quaternion q = nextRotList[cindex];
				float3 float2 = radiusList[cindex];
				int index = transformIndexList[cindex];
				float num = math.dot(math.abs(boneSclList[index]), dir);
				float2 *= num;
				float3 float3 = 0;
				float3 float4 = 0;
				if (keep)
				{
					float3 float5 = basePosList[cindex];
					quaternion q2 = baseRotList[cindex];
					float3 float6 = math.mul(q2, dir * float2.x);
					float3 float7 = float5 - float6;
					float3 float8 = float5 + float6;
					float y = float2.y;
					float z = float2.z;
					float s = MathUtility.ClosestPtPointSegmentRatio(basepos, float7, float8);
					float num2 = math.lerp(y, z, s);
					float3 float9 = math.lerp(float7, float8, s);
					float3 v = basepos - float9;
					float3 v2 = math.mul(math.inverse(q2), v);
					float6 = math.mul(q, dir * float2.x);
					float7 = @float - float6;
					float8 = @float + float6;
					float9 = math.lerp(float7, float8, s);
					v = math.mul(q, v2);
					float4 = math.normalize(v);
					float3 = float9 + float4 * (num2 + radius);
				}
				else
				{
					float3 float10 = posList[cindex];
					quaternion q3 = rotList[cindex];
					float3 float11 = math.mul(q3, dir * float2.x);
					float3 float12 = float10 - float11;
					float3 float13 = float10 + float11;
					float y2 = float2.y;
					float z2 = float2.z;
					float s2 = MathUtility.ClosestPtPointSegmentRatio(nextpos, float12, float13);
					float num3 = math.lerp(y2, z2, s2);
					float3 float14 = math.lerp(float12, float13, s2);
					float3 v3 = nextpos - float14;
					float3 v4 = math.mul(math.inverse(q3), v3);
					float11 = math.mul(q, dir * float2.x);
					float12 = @float - float11;
					float13 = @float + float11;
					float14 = math.lerp(float12, float13, s2);
					v3 = math.mul(q, v4);
					float4 = math.normalize(v3);
					float3 = float14 + float4 * (num3 + radius);
				}
				return MathUtility.IntersectPointPlaneDist(float3, float4, nextpos, out nextpos);
			}

			private float PlaneColliderDetection(ref float3 nextpos, float radius, int cindex)
			{
				float3 @float = nextPosList[cindex];
				float3 float2 = math.mul(nextRotList[cindex], math.up());
				return MathUtility.IntersectPointPlaneDist(@float + float2 * radius, float2, nextpos, out nextpos);
			}
		}

		public override void Create()
		{
		}

		public override void RemoveTeam(int teamId)
		{
		}

		public void ChangeParam(int teamId, bool useCollision)
		{
			base.Manager.Team.SetFlag(teamId, 32u, useCollision);
		}

		public override void Release()
		{
		}

		public override JobHandle SolverConstraint(float dtime, float updatePower, int iteration, JobHandle jobHandle)
		{
			if (base.Manager.Particle.ColliderCount <= 0)
			{
				return jobHandle;
			}
			CollisionJob jobData = default(CollisionJob);
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.radiusList = base.Manager.Particle.radiusList.ToJobArray();
			jobData.nextPosList = base.Manager.Particle.InNextPosList.ToJobArray();
			jobData.nextRotList = base.Manager.Particle.InNextRotList.ToJobArray();
			jobData.posList = base.Manager.Particle.posList.ToJobArray();
			jobData.rotList = base.Manager.Particle.rotList.ToJobArray();
			jobData.localPosList = base.Manager.Particle.localPosList.ToJobArray();
			jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.baseRotList = base.Manager.Particle.baseRotList.ToJobArray();
			jobData.transformIndexList = base.Manager.Particle.transformIndexList.ToJobArray();
			jobData.colliderList = base.Manager.Team.colliderList.ToJobArray();
			jobData.boneSclList = base.Manager.Bone.boneSclList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.outNextPosList = base.Manager.Particle.OutNextPosList.ToJobArray();
			jobData.frictionList = base.Manager.Particle.frictionList.ToJobArray();
			jobData.collisionLinkIdList = base.Manager.Particle.collisionLinkIdList.ToJobArray();
			jobData.collisionDistList = base.Manager.Particle.collisionDistList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Particle.Length, 64, jobHandle);
			base.Manager.Particle.SwitchingNextPosList();
			return jobHandle;
		}
	}
}
