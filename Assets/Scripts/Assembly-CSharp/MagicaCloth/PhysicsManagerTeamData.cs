using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public class PhysicsManagerTeamData : PhysicsManagerAccess
	{
		public enum ForceMode
		{
			None = 0,
			VelocityAdd = 1,
			VelocityChange = 2,
			VelocityAddWithoutMass = 10,
			VelocityChangeWithoutMass = 11
		}

		public struct TeamData
		{
			public ChunkData particleChunk;

			public ChunkData colliderChunk;

			public uint flag;

			public float friction;

			public float selfCollisionRange;

			public int boneIndex;

			public float3 initScale;

			public float scaleRatio;

			public float3 scaleDirection;

			public float4 quaternionScale;

			public float time;

			public float oldTime;

			public float addTime;

			public float timeScale;

			public float nowTime;

			public float startTime;

			public int updateCount;

			public int runCount;

			public float blendRatio;

			public float3 externalForce;

			public float forceMassInfluence;

			public float forceWindInfluence;

			public float forceWindRandomScale;

			public float velocityWeight;

			public float velocityRecoverySpeed;

			public ForceMode forceMode;

			public float3 impactForce;

			public short restoreDistanceGroupIndex;

			public short triangleBendGroupIndex;

			public short clampDistanceGroupIndex;

			public short clampDistance2GroupIndex;

			public short clampPositionGroupIndex;

			public short clampRotationGroupIndex;

			public short restoreRotationGroupIndex;

			public short adjustRotationGroupIndex;

			public short springGroupIndex;

			public short volumeGroupIndex;

			public short airLineGroupIndex;

			public short lineWorkerGroupIndex;

			public short triangleWorkerGroupIndex;

			public short selfCollisionGroupIndex;

			public short edgeCollisionGroupIndex;

			public short penetrationGroupIndex;

			public short baseSkinningGroupIndex;

			public bool IsActive()
			{
				return (flag & 1) != 0;
			}

			public bool IsRunning()
			{
				return updateCount > 0;
			}

			public bool IsUpdate()
			{
				return runCount < updateCount;
			}

			public bool IsInterpolate()
			{
				return (flag & 2) != 0;
			}

			public bool IsFlag(uint flag)
			{
				return (this.flag & flag) != 0;
			}

			public void SetFlag(uint flag, bool sw)
			{
				if (sw)
				{
					this.flag |= flag;
				}
				else
				{
					this.flag &= ~flag;
				}
			}

			public bool IsReset()
			{
				return (flag & 0x30000) != 0;
			}
		}

		public struct WorldInfluence
		{
			public CurveParam moveInfluence;

			public CurveParam rotInfluence;

			public float maxMoveSpeed;

			public float3 nowPosition;

			public float3 oldPosition;

			public float3 moveIgnoreOffset;

			public float3 moveOffset;

			public quaternion nowRotation;

			public quaternion oldRotation;

			public quaternion rotationOffset;

			public int resetTeleport;

			public float teleportDistance;

			public float teleportRotation;

			public float stabilizationTime;
		}

		[BurstCompile]
		private struct PreProcessTeamDataJob : IJobParallelFor
		{
			public float dtime;

			public float updateDeltaTime;

			public float globalTimeScale;

			public int maxUpdateCount;

			public float unityTimeScale;

			public float elapsedTime;

			public NativeArray<TeamData> teamData;

			public NativeArray<WorldInfluence> teamWorldInfluenceList;

			[ReadOnly]
			public NativeArray<float3> bonePosList;

			[ReadOnly]
			public NativeArray<quaternion> boneRotList;

			[ReadOnly]
			public NativeArray<float3> boneSclList;

			[ReadOnly]
			public NativeArray<PhysicsManagerWindData.WindData> windData;

			[ReadOnly]
			public int directionalWindId;

			public void Execute(int teamId)
			{
				TeamData tdata = teamData[teamId];
				bool flag = teamId == 0;
				if (!tdata.IsActive() || (!flag && tdata.boneIndex < 0))
				{
					tdata.updateCount = 0;
					tdata.runCount = 0;
					teamData[teamId] = tdata;
					return;
				}
				if (flag)
				{
					UpdateTime(ref tdata, false);
				}
				else
				{
					float3 @float = bonePosList[tdata.boneIndex];
					quaternion quaternion = boneRotList[tdata.boneIndex];
					float3 x = boneSclList[tdata.boneIndex];
					if (tdata.initScale.x > 0f)
					{
						tdata.scaleRatio = math.length(x) / math.length(tdata.initScale);
					}
					tdata.scaleDirection = math.sign(x);
					if (x.x < 0f || x.y < 0f || x.z < 0f)
					{
						tdata.quaternionScale = new float4(-math.sign(x), 1f);
					}
					else
					{
						tdata.quaternionScale = 1;
					}
					WorldInfluence value = teamWorldInfluenceList[teamId];
					float3 float2 = @float - value.oldPosition;
					quaternion quaternion2 = MathUtility.FromToRotation(value.oldRotation, quaternion);
					float num = math.length(float2);
					if (num > 1E-06f && dtime > 1E-06f)
					{
						float num2 = num / dtime;
						float num3 = math.max(num2 - value.maxMoveSpeed, 0f) / num2;
						value.moveIgnoreOffset = float2 * num3;
						value.moveOffset = float2 - value.moveIgnoreOffset;
					}
					else
					{
						value.moveIgnoreOffset = 0;
						value.moveOffset = 0;
					}
					value.rotationOffset = quaternion2;
					if (tdata.velocityWeight < 1f)
					{
						float num4 = ((tdata.velocityRecoverySpeed > 1E-06f) ? (dtime / tdata.velocityRecoverySpeed) : 1f);
						tdata.velocityWeight = math.saturate(tdata.velocityWeight + num4);
					}
					if (value.resetTeleport == 1 && (num >= value.teleportDistance * tdata.scaleRatio || math.degrees(MathUtility.Angle(quaternion2)) >= value.teleportRotation))
					{
						tdata.SetFlag(65536u, true);
						tdata.SetFlag(131072u, true);
					}
					bool reset = false;
					if (tdata.IsFlag(65536u) || tdata.IsFlag(131072u))
					{
						value.moveOffset = 0;
						value.moveIgnoreOffset = 0;
						value.rotationOffset = quaternion.identity;
						value.oldPosition = @float;
						value.oldRotation = quaternion;
						tdata.nowTime = updateDeltaTime;
						tdata.velocityWeight = ((value.stabilizationTime > 1E-06f) ? 0f : 1f);
						tdata.velocityRecoverySpeed = value.stabilizationTime;
						reset = true;
					}
					value.nowPosition = @float;
					value.nowRotation = quaternion;
					teamWorldInfluenceList[teamId] = value;
					UpdateTime(ref tdata, reset);
					tdata.SetFlag(65536u, false);
					Wind(ref tdata, @float);
				}
				teamData[teamId] = tdata;
			}

			private void UpdateTime(ref TeamData tdata, bool reset)
			{
				tdata.updateCount = 0;
				tdata.runCount = 0;
				float num = tdata.timeScale * globalTimeScale;
				float num2 = dtime * num;
				tdata.time += num2;
				tdata.addTime = num2;
				float num3 = tdata.nowTime + num2;
				while (num3 >= updateDeltaTime)
				{
					num3 -= updateDeltaTime;
					tdata.updateCount++;
				}
				if (reset)
				{
					tdata.updateCount = Mathf.Max(tdata.updateCount, 1);
					tdata.oldTime = tdata.time;
				}
				tdata.updateCount = math.min(tdata.updateCount, maxUpdateCount);
				tdata.nowTime = num3;
				tdata.startTime = tdata.time - num3 - updateDeltaTime * (float)math.max(tdata.updateCount - 1, 0);
				if (num < 0.99f || unityTimeScale < 0.99f)
				{
					tdata.SetFlag(2u, true);
				}
				else
				{
					tdata.SetFlag(2u, false);
				}
			}

			private void Wind(ref TeamData tdata, float3 pos)
			{
				float3 externalForce = 0;
				if (tdata.forceWindInfluence >= 0.01f)
				{
					float2 v = new float2(pos.x, pos.z) * 0.1f;
					v.x += elapsedTime * 1f;
					float num = noise.snoise(v);
					if (directionalWindId >= 0)
					{
						PhysicsManagerWindData.WindData windData = this.windData[directionalWindId];
						if (windData.IsActive())
						{
							float3 direction = windData.direction;
							direction *= windData.main;
							float num2 = math.max(num * tdata.forceWindRandomScale, -1f);
							externalForce += direction + direction * num2;
						}
					}
					externalForce *= tdata.forceWindInfluence;
				}
				tdata.externalForce = externalForce;
			}
		}

		[BurstCompile]
		private struct PostProcessTeamDataJob : IJobParallelFor
		{
			public NativeArray<TeamData> teamData;

			public NativeArray<WorldInfluence> teamWorldInfluenceList;

			public void Execute(int index)
			{
				TeamData value = teamData[index];
				if (value.IsActive())
				{
					WorldInfluence value2 = teamWorldInfluenceList[index];
					value2.oldPosition = value2.nowPosition;
					value2.oldRotation = value2.nowRotation;
					if (value.IsRunning())
					{
						value.impactForce = 0;
						value.forceMode = ForceMode.None;
						value.oldTime = value.time;
					}
					value.SetFlag(131072u, false);
					teamData[index] = value;
					teamWorldInfluenceList[index] = value2;
				}
			}
		}

		[BurstCompile]
		private struct UpdateTeamUpdateCountJob : IJobParallelFor
		{
			public NativeArray<TeamData> teamData;

			public void Execute(int index)
			{
				TeamData value = teamData[index];
				if (value.IsActive())
				{
					value.runCount++;
					teamData[index] = value;
				}
			}
		}

		public const uint Flag_Enable = 1u;

		public const uint Flag_Interpolate = 2u;

		public const uint Flag_FixedNonRotation = 4u;

		public const uint Flag_IgnoreClampPositionVelocity = 16u;

		public const uint Flag_Collision = 32u;

		public const uint Flag_AfterCollision = 64u;

		public const uint Flag_Reset_WorldInfluence = 65536u;

		public const uint Flag_Reset_Position = 131072u;

		public const uint Flag_Collision_KeepShape = 262144u;

		public FixedNativeList<TeamData> teamDataList;

		public FixedNativeList<CurveParam> teamMassList;

		public FixedNativeList<CurveParam> teamGravityList;

		public FixedNativeList<CurveParam> teamDragList;

		public FixedNativeList<CurveParam> teamMaxVelocityList;

		public FixedNativeList<WorldInfluence> teamWorldInfluenceList;

		public FixedMultiNativeList<int> colliderList;

		private Dictionary<int, PhysicsTeam> teamComponentDict = new Dictionary<int, PhysicsTeam>();

		private int activeTeamCount;

		public int TeamCount
		{
			get
			{
				return teamDataList.Count - 1;
			}
		}

		public int TeamLength
		{
			get
			{
				return teamDataList.Length;
			}
		}

		public int ActiveTeamCount
		{
			get
			{
				return activeTeamCount;
			}
		}

		public int ColliderCount
		{
			get
			{
				if (colliderList == null)
				{
					return 0;
				}
				return colliderList.Count;
			}
		}

		public override void Create()
		{
			teamDataList = new FixedNativeList<TeamData>();
			teamMassList = new FixedNativeList<CurveParam>();
			teamGravityList = new FixedNativeList<CurveParam>();
			teamDragList = new FixedNativeList<CurveParam>();
			teamMaxVelocityList = new FixedNativeList<CurveParam>();
			teamWorldInfluenceList = new FixedNativeList<WorldInfluence>();
			colliderList = new FixedMultiNativeList<int>();
			CreateTeam(null, 0u);
		}

		public override void Dispose()
		{
			if (teamDataList != null)
			{
				colliderList.Dispose();
				teamMassList.Dispose();
				teamGravityList.Dispose();
				teamDragList.Dispose();
				teamMaxVelocityList.Dispose();
				teamWorldInfluenceList.Dispose();
				teamDataList.Dispose();
				teamComponentDict.Clear();
			}
		}

		public int CreateTeam(PhysicsTeam team, uint flag)
		{
			TeamData element = default(TeamData);
			flag |= 1u;
			flag |= 0x10000u;
			element.flag = flag;
			element.friction = 0f;
			element.boneIndex = ((!(team != null)) ? (-1) : 0);
			element.initScale = 0;
			element.scaleDirection = 1;
			element.scaleRatio = 1f;
			element.quaternionScale = 1;
			element.timeScale = 1f;
			element.blendRatio = 1f;
			element.forceMassInfluence = 1f;
			element.forceWindInfluence = 1f;
			element.forceWindRandomScale = 0f;
			element.restoreDistanceGroupIndex = -1;
			element.triangleBendGroupIndex = -1;
			element.clampDistanceGroupIndex = -1;
			element.clampDistance2GroupIndex = -1;
			element.clampPositionGroupIndex = -1;
			element.clampRotationGroupIndex = -1;
			element.restoreRotationGroupIndex = -1;
			element.adjustRotationGroupIndex = -1;
			element.springGroupIndex = -1;
			element.volumeGroupIndex = -1;
			element.airLineGroupIndex = -1;
			element.lineWorkerGroupIndex = -1;
			element.triangleWorkerGroupIndex = -1;
			element.selfCollisionGroupIndex = -1;
			element.edgeCollisionGroupIndex = -1;
			element.penetrationGroupIndex = -1;
			element.baseSkinningGroupIndex = -1;
			int num = teamDataList.Add(element);
			teamMassList.Add(new CurveParam(1f));
			teamGravityList.Add(default(CurveParam));
			teamDragList.Add(default(CurveParam));
			teamMaxVelocityList.Add(default(CurveParam));
			teamWorldInfluenceList.Add(default(WorldInfluence));
			teamComponentDict.Add(num, team);
			if (team != null)
			{
				activeTeamCount++;
			}
			return num;
		}

		public void RemoveTeam(int teamId)
		{
			if (teamId >= 0)
			{
				teamDataList.Remove(teamId);
				teamMassList.Remove(teamId);
				teamGravityList.Remove(teamId);
				teamDragList.Remove(teamId);
				teamMaxVelocityList.Remove(teamId);
				teamWorldInfluenceList.Remove(teamId);
				teamComponentDict.Remove(teamId);
			}
		}

		public void SetEnable(int teamId, bool sw)
		{
			if (teamId >= 0)
			{
				SetFlag(teamId, 1u, sw);
				SetFlag(teamId, 131072u, sw);
				SetFlag(teamId, 65536u, sw);
			}
		}

		public bool IsValid(int teamId)
		{
			return teamId >= 0;
		}

		public bool IsValidData(int teamId)
		{
			if (teamId >= 0)
			{
				return teamComponentDict.ContainsKey(teamId);
			}
			return false;
		}

		public bool IsActive(int teamId)
		{
			if (teamId >= 0)
			{
				return teamDataList[teamId].IsActive();
			}
			return false;
		}

		public void SetFlag(int teamId, uint flag, bool sw)
		{
			if (teamId >= 0)
			{
				TeamData value = teamDataList[teamId];
				bool num = value.IsActive();
				value.SetFlag(flag, sw);
				bool flag2 = value.IsActive();
				if (num != flag2)
				{
					activeTeamCount += (flag2 ? 1 : (-1));
				}
				teamDataList[teamId] = value;
			}
		}

		public void SetParticleChunk(int teamId, ChunkData chunk)
		{
			TeamData value = teamDataList[teamId];
			value.particleChunk = chunk;
			teamDataList[teamId] = value;
		}

		public void SetFriction(int teamId, float friction)
		{
			TeamData value = teamDataList[teamId];
			value.friction = friction;
			teamDataList[teamId] = value;
		}

		public void SetMass(int teamId, BezierParam mass)
		{
			teamMassList[teamId] = new CurveParam(mass);
		}

		public void SetGravity(int teamId, BezierParam gravity)
		{
			teamGravityList[teamId] = new CurveParam(gravity);
		}

		public void SetDrag(int teamId, BezierParam drag)
		{
			teamDragList[teamId] = new CurveParam(drag);
		}

		public void SetMaxVelocity(int teamId, BezierParam maxVelocity)
		{
			teamMaxVelocityList[teamId] = new CurveParam(maxVelocity);
		}

		public void SetExternalForce(int teamId, float massInfluence, float windInfluence, float windRandomScale)
		{
			TeamData value = teamDataList[teamId];
			value.forceMassInfluence = massInfluence;
			value.forceWindInfluence = windInfluence;
			value.forceWindRandomScale = windRandomScale;
			teamDataList[teamId] = value;
		}

		public void SetWorldInfluence(int teamId, float maxSpeed, BezierParam moveInfluence, BezierParam rotInfluence, bool resetTeleport, float teleportDistance, float teleportRotation, float resetStabilizationTime)
		{
			WorldInfluence value = teamWorldInfluenceList[teamId];
			value.maxMoveSpeed = maxSpeed;
			value.moveInfluence = new CurveParam(moveInfluence);
			value.rotInfluence = new CurveParam(rotInfluence);
			value.resetTeleport = (resetTeleport ? 1 : 0);
			value.teleportDistance = teleportDistance;
			value.teleportRotation = teleportRotation;
			value.stabilizationTime = resetStabilizationTime;
			teamWorldInfluenceList[teamId] = value;
		}

		public void SetWorldInfluence(int teamId, float maxSpeed, BezierParam moveInfluence, BezierParam rotInfluence)
		{
			WorldInfluence value = teamWorldInfluenceList[teamId];
			value.maxMoveSpeed = maxSpeed;
			value.moveInfluence = new CurveParam(moveInfluence);
			value.rotInfluence = new CurveParam(rotInfluence);
			teamWorldInfluenceList[teamId] = value;
		}

		public void SetAfterTeleport(int teamId, bool resetTeleport, float teleportDistance, float teleportRotation)
		{
			WorldInfluence value = teamWorldInfluenceList[teamId];
			value.resetTeleport = (resetTeleport ? 1 : 0);
			value.teleportDistance = teleportDistance;
			value.teleportRotation = teleportRotation;
			teamWorldInfluenceList[teamId] = value;
		}

		public void SetStabilizationTime(int teamId, float resetStabilizationTime)
		{
			WorldInfluence value = teamWorldInfluenceList[teamId];
			value.stabilizationTime = resetStabilizationTime;
			teamWorldInfluenceList[teamId] = value;
		}

		public void SetSelfCollisionRange(int teamId, float range)
		{
			TeamData value = teamDataList[teamId];
			value.selfCollisionRange = range;
			teamDataList[teamId] = value;
		}

		public void SetBoneIndex(int teamId, int boneIndex, Vector3 initScale)
		{
			TeamData value = teamDataList[teamId];
			value.boneIndex = boneIndex;
			value.initScale = initScale;
			teamDataList[teamId] = value;
		}

		public void AddCollider(int teamId, int particleIndex)
		{
			TeamData value = teamDataList[teamId];
			ChunkData chunk = value.colliderChunk;
			if (!chunk.IsValid())
			{
				chunk = colliderList.AddChunk(4);
			}
			chunk = colliderList.AddData(chunk, particleIndex);
			value.colliderChunk = chunk;
			teamDataList[teamId] = value;
		}

		public void RemoveCollider(int teamId, int particleIndex)
		{
			TeamData value = teamDataList[teamId];
			ChunkData colliderChunk = value.colliderChunk;
			if (colliderChunk.IsValid())
			{
				colliderChunk = colliderList.RemoveData(colliderChunk, particleIndex);
				value.colliderChunk = colliderChunk;
				teamDataList[teamId] = value;
			}
		}

		public void RemoveCollider(int teamId)
		{
			TeamData value = teamDataList[teamId];
			ChunkData colliderChunk = value.colliderChunk;
			if (colliderChunk.IsValid())
			{
				colliderList.RemoveChunk(colliderChunk);
				value.colliderChunk = default(ChunkData);
				teamDataList[teamId] = value;
			}
		}

		public void ResetFuturePredictionCollidere(int teamId)
		{
			ChunkData colliderChunk = teamDataList[teamId].colliderChunk;
			if (colliderChunk.IsValid())
			{
				colliderList.Process(colliderChunk, delegate(int pindex)
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Particle.ResetFuturePredictionTransform(pindex);
				});
			}
		}

		public void SetTimeScale(int teamId, float timeScale)
		{
			TeamData value = teamDataList[teamId];
			value.timeScale = Mathf.Clamp01(timeScale);
			teamDataList[teamId] = value;
		}

		public float GetTimeScale(int teamId)
		{
			return teamDataList[teamId].timeScale;
		}

		public void SetBlendRatio(int teamId, float blendRatio)
		{
			TeamData value = teamDataList[teamId];
			value.blendRatio = Mathf.Clamp01(blendRatio);
			teamDataList[teamId] = value;
		}

		public float GetBlendRatio(int teamId)
		{
			return teamDataList[teamId].blendRatio;
		}

		public void SetImpactForce(int teamId, float3 force, ForceMode mode)
		{
			TeamData value = teamDataList[teamId];
			value.impactForce = force;
			value.forceMode = mode;
			teamDataList[teamId] = value;
		}

		public void ResetStabilizationTime(int teamId, float resetStabilizationTime = -1f)
		{
			TeamData value = teamDataList[teamId];
			value.velocityWeight = 0f;
			if (resetStabilizationTime >= 0f)
			{
				value.velocityRecoverySpeed = resetStabilizationTime;
			}
			else
			{
				value.velocityRecoverySpeed = teamWorldInfluenceList[teamId].stabilizationTime;
			}
			teamDataList[teamId] = value;
		}

		public void PreUpdateTeamAlways()
		{
			Transform transform = ((Camera.main != null) ? Camera.main.transform : manager.transform);
			foreach (PhysicsTeam value2 in base.Team.teamComponentDict.Values)
			{
				BaseCloth baseCloth = value2 as BaseCloth;
				if (baseCloth == null)
				{
					continue;
				}
				float distanceBlendRatio = 1f;
				if (baseCloth.Params.UseDistanceDisable)
				{
					Transform transform2 = baseCloth.Params.DisableReferenceObject;
					if (transform2 == null)
					{
						transform2 = transform;
					}
					float value = Vector3.Distance(value2.transform.position, transform2.position);
					float disableDistance = baseCloth.Params.DisableDistance;
					float b = Mathf.Max(disableDistance - baseCloth.Params.DisableFadeDistance, 0f);
					distanceBlendRatio = Mathf.InverseLerp(disableDistance, b, value);
				}
				baseCloth.Setup.DistanceBlendRatio = distanceBlendRatio;
				baseCloth.UpdateBlend();
			}
		}

		public int CalcMaxUpdateCount(int ups, float dtime, float updateDeltaTime)
		{
			if (manager.UpdateTime.GetUpdateMode() == UpdateTimeManager.UpdateMode.OncePerFrame)
			{
				return 1;
			}
			float globalTimeScale = manager.GetGlobalTimeScale();
			int a = 0;
			foreach (KeyValuePair<int, PhysicsTeam> item in teamComponentDict)
			{
				if (item.Value == null)
				{
					continue;
				}
				int key = item.Key;
				if (key > 0)
				{
					TeamData teamData = teamDataList[key];
					int num = 0;
					float num2 = teamData.timeScale * globalTimeScale;
					float num3 = dtime * num2;
					num = (int)((teamData.nowTime + num3) / updateDeltaTime);
					if (teamData.IsReset())
					{
						num = Mathf.Max(num, 1);
					}
					a = Mathf.Max(a, num);
				}
			}
			return Mathf.Min(a, 4);
		}

		public void PreUpdateTeamData(float dtime, float updateDeltaTime, int ups, int maxUpdateCount)
		{
			bool isUnscaledUpdate = manager.UpdateTime.IsUnscaledUpdate;
			float globalTimeScale = manager.GetGlobalTimeScale();
			if (!isUnscaledUpdate)
			{
				dtime = updateDeltaTime;
			}
			PreProcessTeamDataJob preProcessTeamDataJob = default(PreProcessTeamDataJob);
			preProcessTeamDataJob.dtime = dtime;
			preProcessTeamDataJob.updateDeltaTime = updateDeltaTime;
			preProcessTeamDataJob.globalTimeScale = globalTimeScale;
			preProcessTeamDataJob.maxUpdateCount = maxUpdateCount;
			preProcessTeamDataJob.unityTimeScale = Time.timeScale;
			preProcessTeamDataJob.elapsedTime = Time.time;
			preProcessTeamDataJob.teamData = base.Team.teamDataList.ToJobArray();
			preProcessTeamDataJob.teamWorldInfluenceList = base.Team.teamWorldInfluenceList.ToJobArray();
			preProcessTeamDataJob.bonePosList = base.Bone.bonePosList.ToJobArray();
			preProcessTeamDataJob.boneRotList = base.Bone.boneRotList.ToJobArray();
			preProcessTeamDataJob.boneSclList = base.Bone.boneSclList.ToJobArray();
			preProcessTeamDataJob.windData = base.Wind.windDataList.ToJobArray();
			preProcessTeamDataJob.directionalWindId = base.Wind.DirectionalWindId;
			PreProcessTeamDataJob jobData = preProcessTeamDataJob;
			base.Compute.MasterJob = jobData.Schedule(base.Team.teamDataList.Length, 8, base.Compute.MasterJob);
		}

		public void PostUpdateTeamData()
		{
			PostProcessTeamDataJob postProcessTeamDataJob = default(PostProcessTeamDataJob);
			postProcessTeamDataJob.teamData = base.Team.teamDataList.ToJobArray();
			postProcessTeamDataJob.teamWorldInfluenceList = base.Team.teamWorldInfluenceList.ToJobArray();
			PostProcessTeamDataJob jobData = postProcessTeamDataJob;
			base.Compute.MasterJob = jobData.Schedule(base.Team.teamDataList.Length, 8, base.Compute.MasterJob);
		}

		public void UpdateTeamUpdateCount()
		{
			UpdateTeamUpdateCountJob updateTeamUpdateCountJob = default(UpdateTeamUpdateCountJob);
			updateTeamUpdateCountJob.teamData = base.Team.teamDataList.ToJobArray();
			UpdateTeamUpdateCountJob jobData = updateTeamUpdateCountJob;
			base.Compute.MasterJob = jobData.Schedule(base.Team.teamDataList.Length, 8, base.Compute.MasterJob);
		}
	}
}
