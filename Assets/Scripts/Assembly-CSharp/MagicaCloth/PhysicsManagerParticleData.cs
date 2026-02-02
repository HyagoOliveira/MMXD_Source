using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public class PhysicsManagerParticleData : PhysicsManagerAccess
	{
		public struct ParticleFlag
		{
			public uint flag;

			public ParticleFlag(params uint[] flags)
			{
				flag = 0u;
				foreach (uint num in flags)
				{
					flag |= num;
				}
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

			public bool IsValid()
			{
				return (flag & 1) != 0;
			}

			public void SetEnable(bool sw)
			{
				if (sw)
				{
					flag |= 1u;
				}
				else
				{
					flag &= 4294967294u;
				}
			}

			public bool IsFixed()
			{
				return (flag & 6) != 0;
			}

			public bool IsKinematic()
			{
				return (flag & 2) != 0;
			}

			public bool IsHold()
			{
				return (flag & 4) != 0;
			}

			public bool IsCollider()
			{
				return (flag & 0x10) != 0;
			}

			public bool IsReadTransform()
			{
				return (flag & 0x7000) != 0;
			}

			public bool IsWriteTransform()
			{
				return (flag & 0x20000) != 0;
			}

			public bool IsRestoreTransform()
			{
				return (flag & 0x40000) != 0;
			}

			public bool IsReadSclTransform()
			{
				return (flag & 0x10000) != 0;
			}

			public bool IsParentTransform()
			{
				return (flag & 0x80000) != 0;
			}
		}

		[BurstCompile]
		private struct CopyBoneToParticleJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamData;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.WorldInfluence> teamWorldInfluenceList;

			[ReadOnly]
			public NativeArray<ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float> depthList;

			[ReadOnly]
			public NativeArray<int> transformIndexList;

			[ReadOnly]
			public NativeArray<float3> localPosList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<float3> bonePosList;

			[ReadOnly]
			public NativeArray<quaternion> boneRotList;

			[ReadOnly]
			public NativeArray<float3> boneSclList;

			public NativeArray<float3> posList;

			public NativeArray<float3> oldPosList;

			public NativeArray<float3> oldSlowPosList;

			[WriteOnly]
			public NativeArray<quaternion> rotList;

			[WriteOnly]
			public NativeArray<float3> basePosList;

			[WriteOnly]
			public NativeArray<quaternion> baseRotList;

			[WriteOnly]
			public NativeArray<float3> nextPosList;

			public void Execute(int index)
			{
				ParticleFlag particleFlag = flagList[index];
				if (!particleFlag.IsValid())
				{
					return;
				}
				float t = depthList[index];
				int index2 = teamIdList[index];
				PhysicsManagerTeamData.TeamData teamDatum = teamData[index2];
				PhysicsManagerTeamData.WorldInfluence worldInfluence = teamWorldInfluenceList[index2];
				float num = worldInfluence.moveInfluence.Evaluate(t);
				float num2 = worldInfluence.rotInfluence.Evaluate(t);
				float3 @float = oldPosList[index];
				float3 float2 = 0;
				if (num < 0.99f || num2 < 0.99f)
				{
					quaternion q = math.slerp(quaternion.identity, worldInfluence.rotationOffset, 1f - num2);
					float3 float3 = worldInfluence.moveOffset * (1f - num);
					float3 v = @float - worldInfluence.oldPosition;
					v = math.mul(q, v);
					float2 = worldInfluence.oldPosition + v + float3 - @float;
				}
				float2 += worldInfluence.moveIgnoreOffset;
				oldPosList[index] = @float + float2;
				oldSlowPosList[index] += float2;
				if (particleFlag.IsFixed())
				{
					nextPosList[index] = @float + float2;
				}
				if (particleFlag.IsReadTransform())
				{
					int index3 = transformIndexList[index];
					float3 value = bonePosList[index3];
					quaternion quaternion = boneRotList[index3];
					if (particleFlag.IsFlag(32768u))
					{
						float3 float4 = boneSclList[index3];
						value += math.mul(quaternion, localPosList[index] * float4);
					}
					if (particleFlag.IsFlag(16384u))
					{
						basePosList[index] = value;
						baseRotList[index] = quaternion;
					}
					if (particleFlag.IsFlag(4096u))
					{
						posList[index] = value;
					}
					if (particleFlag.IsFlag(8192u))
					{
						rotList[index] = quaternion;
					}
				}
			}
		}

		[BurstCompile]
		private struct ResetParticleJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamData;

			public NativeArray<ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<float3> basePosList;

			[ReadOnly]
			public NativeArray<quaternion> baseRotList;

			[WriteOnly]
			public NativeArray<float3> posList;

			[WriteOnly]
			public NativeArray<quaternion> rotList;

			[WriteOnly]
			public NativeArray<float3> oldPosList;

			[WriteOnly]
			public NativeArray<quaternion> oldRotList;

			[WriteOnly]
			public NativeArray<float3> oldSlowPosList;

			[WriteOnly]
			public NativeArray<float3> velocityList;

			[WriteOnly]
			public NativeArray<float3> nextPosList;

			[WriteOnly]
			public NativeArray<quaternion> nextRotList;

			public void Execute(int index)
			{
				ParticleFlag value = flagList[index];
				if (value.IsValid())
				{
					int index2 = teamIdList[index];
					if (teamData[index2].IsFlag(131072u) || value.IsFlag(33554432u))
					{
						float3 value2 = basePosList[index];
						quaternion value3 = baseRotList[index];
						posList[index] = value2;
						rotList[index] = value3;
						oldPosList[index] = value2;
						oldRotList[index] = value3;
						oldSlowPosList[index] = value2;
						velocityList[index] = 0;
						nextPosList[index] = value2;
						nextRotList[index] = value3;
						value.SetFlag(33554432u, false);
						flagList[index] = value;
					}
				}
			}
		}

		[BurstCompile]
		private struct CopyParticleToBoneJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float3> posList;

			[ReadOnly]
			public NativeArray<quaternion> rotList;

			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> transformParticleIndexMap;

			[ReadOnly]
			public NativeArray<int> writeBoneIndexList;

			[NativeDisableParallelForRestriction]
			[WriteOnly]
			public NativeArray<float3> bonePosList;

			[NativeDisableParallelForRestriction]
			[WriteOnly]
			public NativeArray<quaternion> boneRotList;

			public void Execute(int index)
			{
				int item;
				NativeParallelMultiHashMapIterator<int> it;
				if (transformParticleIndexMap.TryGetFirstValue(index, out item, out it) && flagList[item].IsValid())
				{
					float3 value = posList[item];
					quaternion value2 = rotList[item];
					int index2 = writeBoneIndexList[index] - 1;
					bonePosList[index2] = value;
					boneRotList[index2] = value2;
				}
			}
		}

		public const uint Flag_Enable = 1u;

		public const uint Flag_Kinematic = 2u;

		public const uint Flag_Hold = 4u;

		public const uint Flag_Collider = 16u;

		public const uint Flag_Plane = 32u;

		public const uint Flag_CapsuleX = 64u;

		public const uint Flag_CapsuleY = 128u;

		public const uint Flag_CapsuleZ = 256u;

		public const uint Flag_Box = 512u;

		public const uint Flag_TriangleRotation = 1024u;

		public const uint Flag_Transform_Read_Pos = 4096u;

		public const uint Flag_Transform_Read_Rot = 8192u;

		public const uint Flag_Transform_Read_Base = 16384u;

		public const uint Flag_Transform_Read_Local = 32768u;

		public const uint Flag_Transform_Read_Scl = 65536u;

		public const uint Flag_Transform_Write = 131072u;

		public const uint Flag_Transform_Restore = 262144u;

		public const uint Flag_Transform_Parent = 524288u;

		public const uint Flag_Step_Update = 16777216u;

		public const uint Flag_Reset_Position = 33554432u;

		public FixedChunkNativeArray<ParticleFlag> flagList;

		public FixedChunkNativeArray<int> teamIdList;

		public FixedChunkNativeArray<float3> posList;

		public FixedChunkNativeArray<quaternion> rotList;

		public FixedChunkNativeArray<float3> oldPosList;

		public FixedChunkNativeArray<quaternion> oldRotList;

		public FixedChunkNativeArray<float3> oldSlowPosList;

		public FixedChunkNativeArray<float3> localPosList;

		public FixedChunkNativeArray<float3> basePosList;

		public FixedChunkNativeArray<quaternion> baseRotList;

		public FixedChunkNativeArray<float> depthList;

		public FixedChunkNativeArray<float3> radiusList;

		public FixedChunkNativeArray<int> restoreTransformIndexList;

		public FixedChunkNativeArray<int> transformIndexList;

		public FixedChunkNativeArray<float> frictionList;

		public FixedChunkNativeArray<float3> velocityList;

		public FixedChunkNativeArray<int> collisionLinkIdList;

		public FixedChunkNativeArray<float> collisionDistList;

		private FixedChunkNativeArray<float3> nextPos0List;

		private FixedChunkNativeArray<float3> nextPos1List;

		private int nextPosSwitch;

		private FixedChunkNativeArray<quaternion> nextRot0List;

		private FixedChunkNativeArray<quaternion> nextRot1List;

		private int nextRotSwitch;

		private int colliderCount;

		public int Count
		{
			get
			{
				if (flagList == null)
				{
					return 0;
				}
				return Mathf.Max(flagList.Count - 1, 0);
			}
		}

		public int Length
		{
			get
			{
				if (flagList == null)
				{
					return 0;
				}
				return flagList.Length;
			}
		}

		public int ColliderCount
		{
			get
			{
				return colliderCount;
			}
		}

		public FixedChunkNativeArray<float3> InNextPosList
		{
			get
			{
				if (nextPosSwitch != 0)
				{
					return nextPos1List;
				}
				return nextPos0List;
			}
		}

		public FixedChunkNativeArray<float3> OutNextPosList
		{
			get
			{
				if (nextPosSwitch != 0)
				{
					return nextPos0List;
				}
				return nextPos1List;
			}
		}

		public FixedChunkNativeArray<quaternion> InNextRotList
		{
			get
			{
				if (nextRotSwitch != 0)
				{
					return nextRot1List;
				}
				return nextRot0List;
			}
		}

		public FixedChunkNativeArray<quaternion> OutNextRotList
		{
			get
			{
				if (nextRotSwitch != 0)
				{
					return nextRot0List;
				}
				return nextRot1List;
			}
		}

		public override void Create()
		{
			flagList = new FixedChunkNativeArray<ParticleFlag>();
			teamIdList = new FixedChunkNativeArray<int>();
			posList = new FixedChunkNativeArray<float3>();
			rotList = new FixedChunkNativeArray<quaternion>();
			oldPosList = new FixedChunkNativeArray<float3>();
			oldRotList = new FixedChunkNativeArray<quaternion>();
			oldSlowPosList = new FixedChunkNativeArray<float3>();
			localPosList = new FixedChunkNativeArray<float3>();
			basePosList = new FixedChunkNativeArray<float3>();
			baseRotList = new FixedChunkNativeArray<quaternion>();
			depthList = new FixedChunkNativeArray<float>();
			radiusList = new FixedChunkNativeArray<float3>();
			restoreTransformIndexList = new FixedChunkNativeArray<int>();
			transformIndexList = new FixedChunkNativeArray<int>();
			frictionList = new FixedChunkNativeArray<float>();
			velocityList = new FixedChunkNativeArray<float3>();
			collisionLinkIdList = new FixedChunkNativeArray<int>();
			collisionDistList = new FixedChunkNativeArray<float>();
			nextPos0List = new FixedChunkNativeArray<float3>();
			nextPos1List = new FixedChunkNativeArray<float3>();
			nextRot0List = new FixedChunkNativeArray<quaternion>();
			nextRot1List = new FixedChunkNativeArray<quaternion>();
			ChunkData c = CreateParticle(2u, 0, 0, quaternion.identity, 0f, 1f, 0);
			SetEnable(c, false, null, null, null);
		}

		public override void Dispose()
		{
			if (flagList != null)
			{
				flagList.Dispose();
				teamIdList.Dispose();
				posList.Dispose();
				rotList.Dispose();
				oldPosList.Dispose();
				oldRotList.Dispose();
				oldSlowPosList.Dispose();
				localPosList.Dispose();
				basePosList.Dispose();
				baseRotList.Dispose();
				depthList.Dispose();
				radiusList.Dispose();
				restoreTransformIndexList.Dispose();
				transformIndexList.Dispose();
				frictionList.Dispose();
				velocityList.Dispose();
				collisionLinkIdList.Dispose();
				collisionDistList.Dispose();
				nextPos0List.Dispose();
				nextPos1List.Dispose();
				nextRot0List.Dispose();
				nextRot1List.Dispose();
			}
		}

		public ChunkData CreateParticle(uint flag, int team, float3 wpos, quaternion wrot, float depth, float3 radius, float3 targetLocalPos)
		{
			flag |= 1u;
			ParticleFlag data = new ParticleFlag(flag);
			ChunkData result = flagList.Add(data);
			teamIdList.Add(team);
			posList.Add(wpos);
			rotList.Add(wrot);
			oldPosList.Add(wpos);
			oldRotList.Add(wrot);
			oldSlowPosList.Add(wpos);
			localPosList.Add(targetLocalPos);
			basePosList.Add(wpos);
			baseRotList.Add(wrot);
			depthList.Add(depth);
			radiusList.Add(radius);
			frictionList.Add(0f);
			velocityList.Add(0);
			collisionLinkIdList.Add(0);
			collisionDistList.Add(0f);
			nextPos0List.Add(0);
			nextPos1List.Add(0);
			nextRot0List.Add(quaternion.identity);
			nextRot1List.Add(quaternion.identity);
			int data2 = -1;
			int data3 = -1;
			restoreTransformIndexList.Add(data2);
			transformIndexList.Add(data3);
			if (data.IsCollider())
			{
				colliderCount++;
			}
			return result;
		}

		public ChunkData CreateParticle(int team, int count, Func<int, uint> funcFlag, Func<int, float3> funcWpos, Func<int, quaternion> funcWrot, Func<int, float> funcDepth, Func<int, float3> funcRadius, Func<int, float3> funcTargetLocalPos)
		{
			ChunkData chunkData = flagList.AddChunk(count);
			teamIdList.AddChunk(count);
			posList.AddChunk(count);
			rotList.AddChunk(count);
			oldPosList.AddChunk(count);
			oldRotList.AddChunk(count);
			oldSlowPosList.AddChunk(count);
			localPosList.AddChunk(count);
			basePosList.AddChunk(count);
			baseRotList.AddChunk(count);
			depthList.AddChunk(count);
			radiusList.AddChunk(count);
			frictionList.AddChunk(count);
			velocityList.AddChunk(count);
			collisionLinkIdList.AddChunk(count);
			collisionDistList.AddChunk(count);
			nextPos0List.AddChunk(count);
			nextPos1List.AddChunk(count);
			nextRot0List.AddChunk(count);
			nextRot1List.AddChunk(count);
			restoreTransformIndexList.AddChunk(count);
			transformIndexList.AddChunk(count);
			teamIdList.Fill(chunkData, team);
			nextRot0List.Fill(chunkData, quaternion.identity);
			nextRot1List.Fill(chunkData, quaternion.identity);
			for (int i = 0; i < count; i++)
			{
				int index = chunkData.startIndex + i;
				uint num = 1u;
				float3 value = 0;
				quaternion value2 = quaternion.identity;
				float3 value3 = 0;
				float value4 = 0f;
				float3 value5 = 0;
				int value6 = -1;
				int value7 = -1;
				if (funcFlag != null)
				{
					num |= funcFlag(i);
				}
				ParticleFlag value8 = new ParticleFlag(num);
				if (funcWpos != null)
				{
					value = funcWpos(i);
				}
				if (funcWrot != null)
				{
					value2 = funcWrot(i);
				}
				if (funcTargetLocalPos != null)
				{
					value3 = funcTargetLocalPos(i);
				}
				if (funcDepth != null)
				{
					value4 = funcDepth(i);
				}
				if (funcRadius != null)
				{
					value5 = funcRadius(i);
				}
				flagList[index] = value8;
				posList[index] = value;
				rotList[index] = value2;
				oldPosList[index] = value;
				oldRotList[index] = value2;
				oldSlowPosList[index] = value;
				localPosList[index] = value3;
				basePosList[index] = value;
				baseRotList[index] = value2;
				depthList[index] = value4;
				radiusList[index] = value5;
				restoreTransformIndexList[index] = value6;
				transformIndexList[index] = value7;
				if (value8.IsCollider())
				{
					colliderCount++;
				}
			}
			return chunkData;
		}

		public void RemoveParticle(ChunkData c)
		{
			for (int i = 0; i < c.dataLength; i++)
			{
				int index = c.startIndex + i;
				if (flagList[index].IsCollider())
				{
					colliderCount--;
				}
			}
			flagList.RemoveChunk(c);
			teamIdList.RemoveChunk(c);
			posList.RemoveChunk(c);
			rotList.RemoveChunk(c);
			oldPosList.RemoveChunk(c);
			oldRotList.RemoveChunk(c);
			oldSlowPosList.RemoveChunk(c);
			localPosList.RemoveChunk(c);
			basePosList.RemoveChunk(c);
			baseRotList.RemoveChunk(c);
			depthList.RemoveChunk(c);
			radiusList.RemoveChunk(c);
			frictionList.RemoveChunk(c);
			velocityList.RemoveChunk(c);
			collisionLinkIdList.RemoveChunk(c);
			collisionDistList.RemoveChunk(c);
			nextPos0List.RemoveChunk(c);
			nextPos1List.RemoveChunk(c);
			nextRot0List.RemoveChunk(c);
			nextRot1List.RemoveChunk(c);
			restoreTransformIndexList.RemoveChunk(c);
			transformIndexList.RemoveChunk(c);
		}

		public void SetEnable(ChunkData c, bool sw, Func<int, Transform> funcTarget, Func<int, float3> funcLpos, Func<int, quaternion> funcLrot)
		{
			for (int i = 0; i < c.dataLength; i++)
			{
				int num = c.startIndex + i;
				ParticleFlag value = flagList[num];
				value.SetEnable(sw);
				if (sw)
				{
					value.SetFlag(33554432u, true);
					if (funcTarget != null)
					{
						Transform transform = funcTarget(i);
						if (transform != null)
						{
							if (value.IsRestoreTransform() && restoreTransformIndexList[num] == -1)
							{
								float3 lpos = ((funcLpos != null) ? funcLpos(i) : ((float3)0));
								quaternion lrot = ((funcLrot != null) ? funcLrot(i) : quaternion.identity);
								restoreTransformIndexList[num] = base.Bone.AddRestoreBone(transform, lpos, lrot);
							}
							if (value.IsReadTransform() && transformIndexList[num] == -1)
							{
								int pindex = (value.IsWriteTransform() ? num : (-1));
								bool addParent = value.IsParentTransform();
								transformIndexList[num] = base.Bone.AddBone(transform, pindex, addParent);
							}
						}
					}
				}
				else
				{
					if (value.IsRestoreTransform())
					{
						int num2 = restoreTransformIndexList[num];
						if (num2 >= 0)
						{
							base.Bone.RemoveRestoreBone(num2);
							restoreTransformIndexList[num] = -1;
						}
					}
					if (value.IsReadTransform())
					{
						int num3 = transformIndexList[num];
						if (num3 >= 0)
						{
							int pindex2 = (value.IsWriteTransform() ? num : (-1));
							base.Bone.RemoveBone(num3, pindex2);
							transformIndexList[num] = -1;
						}
					}
				}
				flagList[num] = value;
			}
		}

		public void SetRadius(int index, float3 radius)
		{
			radiusList[index] = radius;
		}

		public void SetLocalPos(int index, Vector3 lpos)
		{
			localPosList[index] = lpos;
		}

		public bool IsValid(int index)
		{
			if (index < 0 || index >= Length)
			{
				return false;
			}
			return flagList[index].IsValid();
		}

		public int GetTeamId(int index)
		{
			return teamIdList[index];
		}

		public void ResetFuturePredictionTransform(int index)
		{
			int num = transformIndexList[index];
			if (num >= 0)
			{
				base.Bone.ResetFuturePrediction(num);
			}
		}

		public void ResetFuturePredictionTransform(ChunkData c)
		{
			for (int i = 0; i < c.dataLength; i++)
			{
				int index = c.startIndex + i;
				ResetFuturePredictionTransform(index);
			}
		}

		public void SwitchingNextPosList()
		{
			nextPosSwitch = (nextPosSwitch + 1) % 2;
		}

		public void SwitchingNextRotList()
		{
			nextRotSwitch = (nextRotSwitch + 1) % 2;
		}

		public void UpdateBoneToParticle()
		{
			if (Count != 0)
			{
				CopyBoneToParticleJob copyBoneToParticleJob = default(CopyBoneToParticleJob);
				copyBoneToParticleJob.teamData = base.Team.teamDataList.ToJobArray();
				copyBoneToParticleJob.teamWorldInfluenceList = base.Team.teamWorldInfluenceList.ToJobArray();
				copyBoneToParticleJob.flagList = flagList.ToJobArray();
				copyBoneToParticleJob.depthList = depthList.ToJobArray();
				copyBoneToParticleJob.transformIndexList = transformIndexList.ToJobArray();
				copyBoneToParticleJob.localPosList = localPosList.ToJobArray();
				copyBoneToParticleJob.teamIdList = teamIdList.ToJobArray();
				copyBoneToParticleJob.bonePosList = base.Bone.bonePosList.ToJobArray();
				copyBoneToParticleJob.boneRotList = base.Bone.boneRotList.ToJobArray();
				copyBoneToParticleJob.boneSclList = base.Bone.boneSclList.ToJobArray();
				copyBoneToParticleJob.posList = posList.ToJobArray();
				copyBoneToParticleJob.oldPosList = oldPosList.ToJobArray();
				copyBoneToParticleJob.oldSlowPosList = oldSlowPosList.ToJobArray();
				copyBoneToParticleJob.rotList = rotList.ToJobArray();
				copyBoneToParticleJob.basePosList = basePosList.ToJobArray();
				copyBoneToParticleJob.baseRotList = baseRotList.ToJobArray();
				copyBoneToParticleJob.nextPosList = InNextPosList.ToJobArray();
				CopyBoneToParticleJob jobData = copyBoneToParticleJob;
				base.Compute.MasterJob = jobData.Schedule(base.Particle.Length, 64, base.Compute.MasterJob);
			}
		}

		public void UpdateResetParticle()
		{
			if (Count != 0)
			{
				ResetParticleJob resetParticleJob = default(ResetParticleJob);
				resetParticleJob.teamData = base.Team.teamDataList.ToJobArray();
				resetParticleJob.flagList = flagList.ToJobArray();
				resetParticleJob.teamIdList = teamIdList.ToJobArray();
				resetParticleJob.basePosList = basePosList.ToJobArray();
				resetParticleJob.baseRotList = baseRotList.ToJobArray();
				resetParticleJob.posList = posList.ToJobArray();
				resetParticleJob.rotList = rotList.ToJobArray();
				resetParticleJob.oldPosList = oldPosList.ToJobArray();
				resetParticleJob.oldRotList = oldRotList.ToJobArray();
				resetParticleJob.oldSlowPosList = oldSlowPosList.ToJobArray();
				resetParticleJob.velocityList = velocityList.ToJobArray();
				resetParticleJob.nextPosList = InNextPosList.ToJobArray();
				resetParticleJob.nextRotList = InNextRotList.ToJobArray();
				ResetParticleJob jobData = resetParticleJob;
				base.Compute.MasterJob = jobData.Schedule(base.Particle.Length, 64, base.Compute.MasterJob);
			}
		}

		public bool UpdateParticleToBone()
		{
			if (Count > 0 && base.Bone.WriteBoneCount > 0)
			{
				CopyParticleToBoneJob copyParticleToBoneJob = default(CopyParticleToBoneJob);
				copyParticleToBoneJob.flagList = flagList.ToJobArray();
				copyParticleToBoneJob.posList = posList.ToJobArray();
				copyParticleToBoneJob.rotList = rotList.ToJobArray();
				copyParticleToBoneJob.transformParticleIndexMap = base.Bone.writeBoneParticleIndexMap.Map;
				copyParticleToBoneJob.writeBoneIndexList = base.Bone.writeBoneIndexList.ToJobArray();
				copyParticleToBoneJob.bonePosList = base.Bone.bonePosList.ToJobArray();
				copyParticleToBoneJob.boneRotList = base.Bone.boneRotList.ToJobArray();
				CopyParticleToBoneJob jobData = copyParticleToBoneJob;
				base.Compute.MasterJob = jobData.Schedule(base.Bone.writeBoneList.Length, 64, base.Compute.MasterJob);
				return true;
			}
			return false;
		}
	}
}
