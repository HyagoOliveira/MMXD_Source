using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace MagicaCloth
{
	public class PhysicsManagerCompute : PhysicsManagerAccess
	{
		[BurstCompile]
		private struct ForceAndVelocityJob : IJobParallelFor
		{
			public float updateDeltaTime;

			public float updatePower;

			public int loopIndex;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<CurveParam> teamMassList;

			[ReadOnly]
			public NativeArray<CurveParam> teamGravityList;

			[ReadOnly]
			public NativeArray<CurveParam> teamDragList;

			[ReadOnly]
			public NativeArray<CurveParam> teamMaxVelocityList;

			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<float> depthList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			public NativeArray<float3> nextPosList;

			public NativeArray<quaternion> nextRotList;

			public NativeArray<float> frictionList;

			public NativeArray<float3> posList;

			public NativeArray<quaternion> rotList;

			public NativeArray<float3> velocityList;

			public NativeArray<float3> oldPosList;

			public NativeArray<quaternion> oldRotList;

			[WriteOnly]
			public NativeArray<float3> oldSlowPosList;

			public void Execute(int index)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid())
				{
					return;
				}
				int num = teamIdList[index];
				PhysicsManagerTeamData.TeamData teamData = teamDataList[num];
				if (num != 0 && !teamData.IsUpdate())
				{
					return;
				}
				float3 @float = oldPosList[index];
				quaternion quaternion = oldRotList[index];
				float3 float2 = @float;
				quaternion quaternion2 = quaternion;
				if (particleFlag.IsFixed())
				{
					float3 float3 = nextPosList[index];
					quaternion quaternion3 = nextRotList[index];
					float num2 = teamData.startTime + updateDeltaTime * (float)teamData.runCount;
					float oldTime = teamData.oldTime;
					float num3 = teamData.time - oldTime;
					float num4 = math.saturate((num2 - oldTime) / num3);
					float2 = math.lerp(@float, basePosList[index], num4);
					quaternion2 = math.slerp(quaternion, baseRotList[index], num4);
					if (particleFlag.IsCollider() && num == 0)
					{
						@float = MathUtility.ClampDistance(float2, float3, 0.2f);
						quaternion = MathUtility.ClampAngle(quaternion2, quaternion3, math.radians(10f));
					}
					else
					{
						@float = float3;
						quaternion = quaternion3;
					}
				}
				else
				{
					float t = depthList[index];
					float num5 = teamMaxVelocityList[num].Evaluate(t);
					float num6 = teamDragList[num].Evaluate(t);
					float num7 = teamGravityList[num].Evaluate(t);
					float num8 = teamMassList[num].Evaluate(t);
					float3 v = velocityList[index];
					num5 *= teamData.scaleRatio;
					num8 = (num8 - 1f) * teamData.forceMassInfluence + 1f;
					v *= teamData.velocityWeight;
					v = MathUtility.ClampVector(v, 0f, num5);
					v *= math.pow(1f - num6, updatePower);
					float3 float4 = 0;
					float4.y += num7 * num8;
					if (loopIndex == 0)
					{
						switch (teamData.forceMode)
						{
						case PhysicsManagerTeamData.ForceMode.VelocityAdd:
							float4 += teamData.impactForce;
							break;
						case PhysicsManagerTeamData.ForceMode.VelocityAddWithoutMass:
							float4 += teamData.impactForce * num8;
							break;
						case PhysicsManagerTeamData.ForceMode.VelocityChange:
							float4 += teamData.impactForce;
							v = 0;
							break;
						case PhysicsManagerTeamData.ForceMode.VelocityChangeWithoutMass:
							float4 += teamData.impactForce * num8;
							v = 0;
							break;
						}
						float4 += teamData.externalForce;
					}
					float4 *= teamData.scaleRatio;
					v += float4 / num8 * updateDeltaTime;
					float2 = @float + v * updateDeltaTime;
				}
				float num9 = frictionList[index];
				num9 *= 0.6f;
				frictionList[index] = num9;
				posList[index] = @float;
				rotList[index] = quaternion;
				nextPosList[index] = float2;
				nextRotList[index] = quaternion2;
			}
		}

		[BurstCompile]
		private struct FixPositionJob : IJobParallelFor
		{
			public float updatePower;

			public float updateDeltaTime;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<float3> nextPosList;

			[ReadOnly]
			public NativeArray<quaternion> nextRotList;

			[ReadOnly]
			public NativeArray<float> frictionList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			public NativeArray<float3> velocityList;

			[WriteOnly]
			public NativeArray<quaternion> rotList;

			public NativeArray<float3> oldPosList;

			public NativeArray<quaternion> oldRotList;

			public NativeArray<float3> oldSlowPosList;

			public NativeArray<float3> posList;

			public void Execute(int index)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (particleFlag.IsValid())
				{
					int index2 = teamIdList[index];
					PhysicsManagerTeamData.TeamData teamData = teamDataList[index2];
					if (teamData.IsUpdate() && !particleFlag.IsFixed())
					{
						float3 @float = nextPosList[index];
						quaternion q = nextRotList[index];
						q = math.normalize(q);
						float3 float2 = 0;
						float3 float3 = posList[index];
						float2 = (@float - float3) / updateDeltaTime;
						float2 *= teamData.velocityWeight;
						float x = frictionList[index];
						float2 *= math.pow(1f - math.saturate(x), updatePower);
						posList[index] = (@float - oldPosList[index]) / updateDeltaTime;
						velocityList[index] = float2;
						oldPosList[index] = @float;
						oldRotList[index] = q;
					}
				}
			}
		}

		[BurstCompile]
		private struct PostUpdatePhysicsJob : IJobParallelFor
		{
			public float updateIntervalTime;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			[ReadOnly]
			public NativeArray<float3> velocityList;

			public NativeArray<float3> oldPosList;

			public NativeArray<quaternion> oldRotList;

			[WriteOnly]
			public NativeArray<float3> posList;

			[WriteOnly]
			public NativeArray<quaternion> rotList;

			[WriteOnly]
			public NativeArray<float3> nextPosList;

			public NativeArray<float3> oldSlowPosList;

			public void Execute(int index)
			{
				PhysicsManagerParticleData.ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid())
				{
					return;
				}
				int index2 = teamIdList[index];
				PhysicsManagerTeamData.TeamData teamData = teamDataList[index2];
				float3 @float = 0;
                quaternion quaternion = baseRotList[index];
                quaternion identity = quaternion.identity;
				float3 float2 = basePosList[index];
				if (!particleFlag.IsFixed())
				{
					float3 float3 = velocityList[index];
					float3 y = oldPosList[index] + float3 * updateIntervalTime;
					float3 x = oldSlowPosList[index];
					float num = teamData.time - teamData.addTime;
					float num2 = teamData.time + (updateIntervalTime - teamData.nowTime) - num;
					float s = teamData.addTime / num2;
					@float = math.lerp(x, y, s);
					identity = oldRotList[index];
					oldSlowPosList[index] = @float;
				}
				else
				{
					@float = float2;
					identity = quaternion;
					if (teamData.IsRunning())
					{
						oldPosList[index] = @float;
						oldRotList[index] = identity;
					}
				}
				if (teamData.blendRatio < 0.99f)
				{
					@float = math.lerp(float2, @float, teamData.blendRatio);
					identity = math.slerp(quaternion, identity, teamData.blendRatio);
					identity = math.normalize(identity);
				}
				posList[index] = @float;
				rotList[index] = identity;
				nextPosList[index] = @float;
			}
		}

		private int solverIteration = 1;

		private List<PhysicsManagerConstraint> constraints = new List<PhysicsManagerConstraint>();

		private List<PhysicsManagerWorker> workers = new List<PhysicsManagerWorker>();

		private JobHandle jobHandle;

		private bool runMasterJob;

		private int swapIndex;

		public ClampPositionConstraint ClampPosition { get; private set; }

		public ClampDistanceConstraint ClampDistance { get; private set; }

		public ClampRotationConstraint ClampRotation { get; private set; }

		public SpringConstraint Spring { get; private set; }

		public RestoreDistanceConstraint RestoreDistance { get; private set; }

		public RestoreRotationConstraint RestoreRotation { get; private set; }

		public TriangleBendConstraint TriangleBend { get; private set; }

		public ColliderCollisionConstraint Collision { get; private set; }

		public PenetrationConstraint Penetration { get; private set; }

		public ColliderExtrusionConstraint ColliderExtrusion { get; private set; }

		public RenderMeshWorker RenderMeshWorker { get; private set; }

		public VirtualMeshWorker VirtualMeshWorker { get; private set; }

		public MeshParticleWorker MeshParticleWorker { get; private set; }

		public SpringMeshWorker SpringMeshWorker { get; private set; }

		public AdjustRotationWorker AdjustRotationWorker { get; private set; }

		public LineWorker LineWorker { get; private set; }

		public TriangleWorker TriangleWorker { get; private set; }

		public CustomSampler SamplerWriteMesh { get; set; }

		public JobHandle MasterJob
		{
			get
			{
				return jobHandle;
			}
			set
			{
				jobHandle = value;
			}
		}

		public int SwapIndex
		{
			get
			{
				return swapIndex;
			}
		}

		public override void Create()
		{
			ColliderExtrusion = new ColliderExtrusionConstraint();
			constraints.Add(ColliderExtrusion);
			Penetration = new PenetrationConstraint();
			constraints.Add(Penetration);
			Collision = new ColliderCollisionConstraint();
			constraints.Add(Collision);
			ClampDistance = new ClampDistanceConstraint();
			constraints.Add(ClampDistance);
			Spring = new SpringConstraint();
			constraints.Add(Spring);
			RestoreDistance = new RestoreDistanceConstraint();
			constraints.Add(RestoreDistance);
			RestoreRotation = new RestoreRotationConstraint();
			constraints.Add(RestoreRotation);
			TriangleBend = new TriangleBendConstraint();
			constraints.Add(TriangleBend);
			ClampPosition = new ClampPositionConstraint();
			constraints.Add(ClampPosition);
			ClampRotation = new ClampRotationConstraint();
			constraints.Add(ClampRotation);
			foreach (PhysicsManagerConstraint constraint in constraints)
			{
				constraint.Init(manager);
			}
			RenderMeshWorker = new RenderMeshWorker();
			workers.Add(RenderMeshWorker);
			VirtualMeshWorker = new VirtualMeshWorker();
			workers.Add(VirtualMeshWorker);
			MeshParticleWorker = new MeshParticleWorker();
			workers.Add(MeshParticleWorker);
			SpringMeshWorker = new SpringMeshWorker();
			workers.Add(SpringMeshWorker);
			AdjustRotationWorker = new AdjustRotationWorker();
			workers.Add(AdjustRotationWorker);
			LineWorker = new LineWorker();
			workers.Add(LineWorker);
			TriangleWorker = new TriangleWorker();
			workers.Add(TriangleWorker);
			foreach (PhysicsManagerWorker worker in workers)
			{
				worker.Init(manager);
			}
			SamplerWriteMesh = CustomSampler.Create("WriteMesh");
		}

		public override void Dispose()
		{
			if (constraints != null)
			{
				foreach (PhysicsManagerConstraint constraint in constraints)
				{
					constraint.Release();
				}
			}
			if (workers == null)
			{
				return;
			}
			foreach (PhysicsManagerWorker worker in workers)
			{
				worker.Release();
			}
		}

		public void RemoveTeam(int teamId)
		{
			if (!CreateSingleton<MagicaPhysicsManager>.Instance.Team.IsValidData(teamId))
			{
				return;
			}
			if (constraints != null)
			{
				foreach (PhysicsManagerConstraint constraint in constraints)
				{
					constraint.RemoveTeam(teamId);
				}
			}
			if (workers == null)
			{
				return;
			}
			foreach (PhysicsManagerWorker worker in workers)
			{
				worker.RemoveGroup(teamId);
			}
		}

		public void UpdateRestoreBone()
		{
			if (base.Team.ActiveTeamCount > 0)
			{
				base.Bone.ResetBoneFromTransform();
			}
		}

		public void UpdateReadBone()
		{
			if (base.Team.ActiveTeamCount > 0)
			{
				base.Bone.ReadBoneFromTransform();
			}
		}

		public void UpdateReadBoneScale()
		{
			if (base.Team.ActiveTeamCount > 0 && base.UpdateTime.UpdateBoneScale)
			{
				base.Bone.ReadBoneScaleFromTransform();
			}
		}

		public void UpdateTeamAlways()
		{
			if (manager.IsActive)
			{
				base.Team.PreUpdateTeamAlways();
			}
		}

		public void UpdateStartSimulation(UpdateTimeManager update)
		{
			float deltaTime = update.DeltaTime;
			float updatePower = update.UpdatePower;
			float updateIntervalTime = update.UpdateIntervalTime;
			int updatePerSecond = update.UpdatePerSecond;
			if (base.Team.ActiveTeamCount > 0)
			{
				int num = base.Team.CalcMaxUpdateCount(updatePerSecond, deltaTime, updateIntervalTime);
				base.Wind.UpdateWind();
				base.Team.PreUpdateTeamData(deltaTime, updateIntervalTime, updatePerSecond, num);
				WarmupWorker();
				base.Particle.UpdateBoneToParticle();
				MasterJob = VirtualMeshWorker.PreUpdate(MasterJob);
				MasterJob = MeshParticleWorker.PreUpdate(MasterJob);
				base.Particle.UpdateResetParticle();
				int i = 0;
				for (int num2 = num; i < num2; i++)
				{
					UpdatePhysics(num, i, updatePower, updateIntervalTime);
				}
				PostUpdatePhysics(updateIntervalTime);
				MasterJob = TriangleWorker.PostUpdate(MasterJob);
				MasterJob = LineWorker.PostUpdate(MasterJob);
				MasterJob = AdjustRotationWorker.PostUpdate(MasterJob);
				base.Particle.UpdateParticleToBone();
				MasterJob = SpringMeshWorker.PostUpdate(MasterJob);
				MasterJob = MeshParticleWorker.PostUpdate(MasterJob);
				MasterJob = VirtualMeshWorker.PostUpdate(MasterJob);
				MasterJob = RenderMeshWorker.PostUpdate(MasterJob);
				base.Bone.ConvertWorldToLocal();
				base.Team.PostUpdateTeamData();
			}
		}

		public void UpdateCompleteSimulation()
		{
			CompleteJob();
			runMasterJob = true;
		}

		public void UpdateWriteBone()
		{
			base.Bone.WriteBoneToTransform(manager.IsDelay ? 1 : 0);
		}

		public void UpdateWriteMesh()
		{
			if (base.Mesh.VirtualMeshCount > 0 && runMasterJob)
			{
				base.Mesh.FinishMesh(manager.IsDelay ? 1 : 0);
			}
		}

		public void UpdateReadWriteBone()
		{
			if (base.Team.ActiveTeamCount > 0)
			{
				base.Bone.ReadBoneFromTransform();
				if (runMasterJob)
				{
					base.Bone.WriteBoneToTransform(manager.IsDelay ? 1 : 0);
				}
			}
		}

		public void UpdateSyncBuffer()
		{
			base.Bone.writeBoneIndexList.SyncBuffer();
			base.Bone.writeBonePosList.SyncBuffer();
			base.Bone.writeBoneRotList.SyncBuffer();
			InitJob();
			base.Bone.CopyBoneBuffer();
			CompleteJob();
		}

		public void UpdateSwapBuffer()
		{
			base.Mesh.renderPosList.SwapBuffer();
			base.Mesh.renderNormalList.SwapBuffer();
			base.Mesh.renderTangentList.SwapBuffer();
			base.Mesh.renderBoneWeightList.SwapBuffer();
			swapIndex ^= 1;
			foreach (PhysicsManagerMeshData.RenderMeshState value in base.Mesh.renderMeshStateDict.Values)
			{
				value.SetFlag(256u, true);
			}
		}

		public void InitJob()
		{
			jobHandle = default(JobHandle);
		}

		public void ScheduleJob()
		{
			JobHandle.ScheduleBatchedJobs();
		}

		public void CompleteJob()
		{
			jobHandle.Complete();
			jobHandle = default(JobHandle);
		}

		private void UpdatePhysics(int updateCount, int loopIndex, float updatePower, float updateDeltaTime)
		{
			if (base.Particle.Count == 0)
			{
				return;
			}
			ForceAndVelocityJob forceAndVelocityJob = default(ForceAndVelocityJob);
			forceAndVelocityJob.updateDeltaTime = updateDeltaTime;
			forceAndVelocityJob.updatePower = updatePower;
			forceAndVelocityJob.loopIndex = loopIndex;
			forceAndVelocityJob.teamDataList = base.Team.teamDataList.ToJobArray();
			forceAndVelocityJob.teamMassList = base.Team.teamMassList.ToJobArray();
			forceAndVelocityJob.teamGravityList = base.Team.teamGravityList.ToJobArray();
			forceAndVelocityJob.teamDragList = base.Team.teamDragList.ToJobArray();
			forceAndVelocityJob.teamMaxVelocityList = base.Team.teamMaxVelocityList.ToJobArray();
			forceAndVelocityJob.flagList = base.Particle.flagList.ToJobArray();
			forceAndVelocityJob.teamIdList = base.Particle.teamIdList.ToJobArray();
			forceAndVelocityJob.depthList = base.Particle.depthList.ToJobArray();
			forceAndVelocityJob.basePosList = base.Particle.basePosList.ToJobArray();
			forceAndVelocityJob.baseRotList = base.Particle.baseRotList.ToJobArray();
			forceAndVelocityJob.nextPosList = base.Particle.InNextPosList.ToJobArray();
			forceAndVelocityJob.nextRotList = base.Particle.InNextRotList.ToJobArray();
			forceAndVelocityJob.oldPosList = base.Particle.oldPosList.ToJobArray();
			forceAndVelocityJob.oldRotList = base.Particle.oldRotList.ToJobArray();
			forceAndVelocityJob.frictionList = base.Particle.frictionList.ToJobArray();
			forceAndVelocityJob.oldSlowPosList = base.Particle.oldSlowPosList.ToJobArray();
			forceAndVelocityJob.posList = base.Particle.posList.ToJobArray();
			forceAndVelocityJob.rotList = base.Particle.rotList.ToJobArray();
			forceAndVelocityJob.velocityList = base.Particle.velocityList.ToJobArray();
			ForceAndVelocityJob jobData = forceAndVelocityJob;
			jobHandle = jobData.Schedule(base.Particle.Length, 64, jobHandle);
			if (constraints != null)
			{
				for (int i = 0; i < solverIteration; i++)
				{
					foreach (PhysicsManagerConstraint constraint in constraints)
					{
						if (constraint != null)
						{
							for (int j = 0; j < constraint.GetIterationCount(); j++)
							{
								jobHandle = constraint.SolverConstraint(updateDeltaTime, updatePower, j, jobHandle);
							}
						}
					}
				}
			}
			FixPositionJob fixPositionJob = default(FixPositionJob);
			fixPositionJob.updatePower = updatePower;
			fixPositionJob.updateDeltaTime = updateDeltaTime;
			fixPositionJob.teamDataList = base.Team.teamDataList.ToJobArray();
			fixPositionJob.flagList = base.Particle.flagList.ToJobArray();
			fixPositionJob.teamIdList = base.Particle.teamIdList.ToJobArray();
			fixPositionJob.nextPosList = base.Particle.InNextPosList.ToJobArray();
			fixPositionJob.nextRotList = base.Particle.InNextRotList.ToJobArray();
			fixPositionJob.basePosList = base.Particle.basePosList.ToJobArray();
			fixPositionJob.baseRotList = base.Particle.baseRotList.ToJobArray();
			fixPositionJob.oldPosList = base.Particle.oldPosList.ToJobArray();
			fixPositionJob.oldRotList = base.Particle.oldRotList.ToJobArray();
			fixPositionJob.oldSlowPosList = base.Particle.oldSlowPosList.ToJobArray();
			fixPositionJob.frictionList = base.Particle.frictionList.ToJobArray();
			fixPositionJob.velocityList = base.Particle.velocityList.ToJobArray();
			fixPositionJob.rotList = base.Particle.rotList.ToJobArray();
			fixPositionJob.posList = base.Particle.posList.ToJobArray();
			FixPositionJob jobData2 = fixPositionJob;
			jobHandle = jobData2.Schedule(base.Particle.Length, 64, jobHandle);
			base.Team.UpdateTeamUpdateCount();
		}

		private void PostUpdatePhysics(float updateIntervalTime)
		{
			if (base.Particle.Count != 0)
			{
				PostUpdatePhysicsJob postUpdatePhysicsJob = default(PostUpdatePhysicsJob);
				postUpdatePhysicsJob.updateIntervalTime = updateIntervalTime;
				postUpdatePhysicsJob.teamDataList = base.Team.teamDataList.ToJobArray();
				postUpdatePhysicsJob.flagList = base.Particle.flagList.ToJobArray();
				postUpdatePhysicsJob.teamIdList = base.Particle.teamIdList.ToJobArray();
				postUpdatePhysicsJob.basePosList = base.Particle.basePosList.ToJobArray();
				postUpdatePhysicsJob.baseRotList = base.Particle.baseRotList.ToJobArray();
				postUpdatePhysicsJob.oldPosList = base.Particle.oldPosList.ToJobArray();
				postUpdatePhysicsJob.oldRotList = base.Particle.oldRotList.ToJobArray();
				postUpdatePhysicsJob.velocityList = base.Particle.velocityList.ToJobArray();
				postUpdatePhysicsJob.posList = base.Particle.posList.ToJobArray();
				postUpdatePhysicsJob.rotList = base.Particle.rotList.ToJobArray();
				postUpdatePhysicsJob.nextPosList = base.Particle.InNextPosList.ToJobArray();
				postUpdatePhysicsJob.oldSlowPosList = base.Particle.oldSlowPosList.ToJobArray();
				PostUpdatePhysicsJob jobData = postUpdatePhysicsJob;
				jobHandle = jobData.Schedule(base.Particle.Length, 64, jobHandle);
			}
		}

		private void WarmupWorker()
		{
			if (workers != null && workers.Count != 0)
			{
				for (int i = 0; i < workers.Count; i++)
				{
					workers[i].Warmup();
				}
			}
		}
	}
}
