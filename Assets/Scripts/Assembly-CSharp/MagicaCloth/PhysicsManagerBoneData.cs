using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace MagicaCloth
{
	public class PhysicsManagerBoneData : PhysicsManagerAccess
	{
		[BurstCompile]
		private struct RestoreBoneJob : IJobParallelForTransform
		{
			[ReadOnly]
			public NativeArray<float3> localPosList;

			[ReadOnly]
			public NativeArray<quaternion> localRotList;

			public void Execute(int index, TransformAccess transform)
			{
				transform.localPosition = localPosList[index];
				transform.localRotation = localRotList[index];
			}
		}

		[BurstCompile]
		private struct ReadBoneJob0 : IJobParallelForTransform
		{
			[WriteOnly]
			public NativeArray<float3> bonePosList;

			[WriteOnly]
			public NativeArray<quaternion> boneRotList;

			[WriteOnly]
			public NativeArray<float3> basePosList;

			[WriteOnly]
			public NativeArray<quaternion> baseRotList;

			public void Execute(int index, TransformAccess transform)
			{
				float3 value = transform.position;
				quaternion value2 = transform.rotation;
				bonePosList[index] = value;
				boneRotList[index] = value2;
				basePosList[index] = value;
				baseRotList[index] = value2;
			}
		}

		[BurstCompile]
		private struct ConvertWorldToLocalJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<int> writeBoneIndexList;

			[ReadOnly]
			public NativeArray<float3> bonePosList;

			[ReadOnly]
			public NativeArray<quaternion> boneRotList;

			[ReadOnly]
			public NativeArray<float3> boneSclList;

			[ReadOnly]
			public NativeArray<int> boneParentIndexList;

			[WriteOnly]
			public NativeArray<float3> writeBonePosList;

			[WriteOnly]
			public NativeArray<quaternion> writeBoneRotList;

			public void Execute(int index)
			{
				int num = writeBoneIndexList[index];
				if (num == 0)
				{
					return;
				}
				num--;
				float3 @float = bonePosList[num];
				quaternion quaternion = boneRotList[num];
				int num2 = boneParentIndexList[num];
				if (num2 >= 0)
				{
					float3 float2 = bonePosList[num2];
					quaternion q = boneRotList[num2];
					float3 float3 = boneSclList[num2];
					quaternion quaternion2 = math.inverse(q);
					float3 v = @float - float2;
					float3 value = math.mul(quaternion2, v);
					value /= float3;
					quaternion value2 = math.mul(quaternion2, quaternion);
					if (float3.x < 0f || float3.y < 0f || float3.z < 0f)
					{
						value2 = new quaternion(value2.value * new float4(-math.sign(float3), 1f));
					}
					writeBonePosList[index] = value;
					writeBoneRotList[index] = value2;
				}
				else
				{
					writeBonePosList[index] = @float;
					writeBoneRotList[index] = quaternion;
				}
			}
		}

		[BurstCompile]
		private struct WriteBontToTransformJob2 : IJobParallelForTransform
		{
			[ReadOnly]
			public NativeArray<int> writeBoneIndexList;

			[ReadOnly]
			public NativeArray<int> boneParentIndexList;

			[ReadOnly]
			public NativeArray<float3> writeBonePosList;

			[ReadOnly]
			public NativeArray<quaternion> writeBoneRotList;

			public void Execute(int index, TransformAccess transform)
			{
				if (index >= writeBoneIndexList.Length)
				{
					return;
				}
				int num = writeBoneIndexList[index];
				if (num != 0)
				{
					num--;
					float3 @float = writeBonePosList[index];
					quaternion quaternion = writeBoneRotList[index];
					if (boneParentIndexList[num] >= 0)
					{
						transform.localPosition = @float;
						transform.localRotation = quaternion;
					}
					else
					{
						transform.position = @float;
						transform.rotation = quaternion;
					}
				}
			}
		}

		[BurstCompile]
		private struct CopyBoneJob0 : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<float3> bonePosList;

			[ReadOnly]
			public NativeArray<quaternion> boneRotList;

			[WriteOnly]
			public NativeArray<float3> backBonePosList;

			[WriteOnly]
			public NativeArray<quaternion> backBoneRotList;

			public void Execute(int index)
			{
				backBonePosList[index] = bonePosList[index];
				backBoneRotList[index] = boneRotList[index];
			}
		}

		[BurstCompile]
		private struct CopyBoneJob1 : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<int> writeBoneIndexList;

			[WriteOnly]
			public NativeArray<int> backWriteBoneIndexList;

			public void Execute(int index)
			{
				backWriteBoneIndexList[index] = writeBoneIndexList[index];
			}
		}

		public FixedTransformAccessArray boneList;

		public FixedNativeList<float3> bonePosList;

		public FixedNativeList<quaternion> boneRotList;

		public FixedNativeList<float3> boneSclList;

		public FixedNativeList<int> boneParentIndexList;

		public FixedNativeList<float3> basePosList;

		public FixedNativeList<quaternion> baseRotList;

		public FixedTransformAccessArray restoreBoneList;

		public FixedNativeList<float3> restoreBoneLocalPosList;

		public FixedNativeList<quaternion> restoreBoneLocalRotList;

		public FixedTransformAccessArray writeBoneList;

		public FixedNativeList<int> writeBoneIndexList;

		public ExNativeMultiHashMap<int, int> writeBoneParticleIndexMap;

		private Dictionary<int, int> boneToWriteIndexDict = new Dictionary<int, int>();

		public FixedNativeList<float3> writeBonePosList;

		public FixedNativeList<quaternion> writeBoneRotList;

		public bool hasBoneChanged { get; private set; }

		private CustomSampler SamplerReadBoneScale { get; set; }

		public int RestoreBoneCount
		{
			get
			{
				return restoreBoneList.Count;
			}
		}

		public int ReadBoneCount
		{
			get
			{
				return boneList.Count;
			}
		}

		public int WriteBoneCount
		{
			get
			{
				return writeBoneList.Count;
			}
		}

		public override void Create()
		{
			boneList = new FixedTransformAccessArray();
			bonePosList = new FixedNativeList<float3>();
			boneRotList = new FixedNativeList<quaternion>();
			boneSclList = new FixedNativeList<float3>();
			boneParentIndexList = new FixedNativeList<int>();
			basePosList = new FixedNativeList<float3>();
			baseRotList = new FixedNativeList<quaternion>();
			restoreBoneList = new FixedTransformAccessArray();
			restoreBoneLocalPosList = new FixedNativeList<float3>();
			restoreBoneLocalRotList = new FixedNativeList<quaternion>();
			writeBoneList = new FixedTransformAccessArray();
			writeBoneIndexList = new FixedNativeList<int>();
			writeBoneParticleIndexMap = new ExNativeMultiHashMap<int, int>();
			writeBonePosList = new FixedNativeList<float3>();
			writeBoneRotList = new FixedNativeList<quaternion>();
			SamplerReadBoneScale = CustomSampler.Create("ReadBoneScale");
		}

		public override void Dispose()
		{
			if (boneList != null)
			{
				boneList.Dispose();
				bonePosList.Dispose();
				boneRotList.Dispose();
				boneSclList.Dispose();
				boneParentIndexList.Dispose();
				basePosList.Dispose();
				baseRotList.Dispose();
				restoreBoneList.Dispose();
				restoreBoneLocalPosList.Dispose();
				restoreBoneLocalRotList.Dispose();
				writeBoneList.Dispose();
				writeBoneIndexList.Dispose();
				writeBoneParticleIndexMap.Dispose();
				writeBonePosList.Dispose();
				writeBoneRotList.Dispose();
			}
		}

		public int AddRestoreBone(Transform target, float3 lpos, quaternion lrot)
		{
			int result;
			if (restoreBoneList.Exist(target))
			{
				result = restoreBoneList.Add(target);
			}
			else
			{
				result = restoreBoneList.Add(target);
				restoreBoneLocalPosList.Add(lpos);
				restoreBoneLocalRotList.Add(lrot);
				hasBoneChanged = true;
			}
			return result;
		}

		public void RemoveRestoreBone(int restoreBoneIndex)
		{
			restoreBoneList.Remove(restoreBoneIndex);
			if (!restoreBoneList.Exist(restoreBoneIndex))
			{
				restoreBoneLocalPosList.Remove(restoreBoneIndex);
				restoreBoneLocalRotList.Remove(restoreBoneIndex);
				hasBoneChanged = true;
			}
		}

		public int AddBone(Transform target, int pindex = -1, bool addParent = false)
		{
			int num;
			if (boneList.Exist(target))
			{
				num = boneList.Add(target);
				if (addParent && boneParentIndexList[num] < 0)
				{
					boneParentIndexList[num] = boneList.GetIndex(target.parent);
				}
				basePosList[num] = new float3(0f, -1000000f, 0f);
			}
			else
			{
				float3 zero = float3.zero;
				quaternion identity = quaternion.identity;
				num = boneList.Add(target);
				bonePosList.Add(zero);
				boneRotList.Add(identity);
				boneSclList.Add(target.lossyScale);
				if (addParent)
				{
					boneParentIndexList.Add(boneList.GetIndex(target.parent));
				}
				else
				{
					boneParentIndexList.Add(-1);
				}
				basePosList.Add(new float3(0f, -1000000f, 0f));
				baseRotList.Add(identity);
				hasBoneChanged = true;
			}
			if (pindex >= 0)
			{
				if (writeBoneList.Exist(target))
				{
					writeBoneList.Add(target);
				}
				else
				{
					writeBoneList.Add(target);
					writeBoneIndexList.Add(num + 1);
					writeBonePosList.Add(float3.zero);
					writeBoneRotList.Add(quaternion.identity);
					hasBoneChanged = true;
				}
				int index = writeBoneList.GetIndex(target);
				boneToWriteIndexDict.Add(num, index);
				writeBoneParticleIndexMap.Add(index, pindex);
			}
			return num;
		}

		public bool RemoveBone(int boneIndex, int pindex = -1)
		{
			bool result = false;
			boneList.Remove(boneIndex);
			if (!boneList.Exist(boneIndex))
			{
				bonePosList.Remove(boneIndex);
				boneRotList.Remove(boneIndex);
				boneSclList.Remove(boneIndex);
				boneParentIndexList.Remove(boneIndex);
				basePosList.Remove(boneIndex);
				baseRotList.Remove(boneIndex);
				hasBoneChanged = true;
				result = true;
			}
			if (pindex >= 0)
			{
				int num = boneToWriteIndexDict[boneIndex];
				writeBoneList.Remove(num);
				writeBoneIndexList.Remove(num);
				writeBoneParticleIndexMap.Remove(num, pindex);
				writeBonePosList.Remove(num);
				writeBoneRotList.Remove(num);
				hasBoneChanged = true;
				if (!writeBoneList.Exist(num))
				{
					boneToWriteIndexDict.Remove(boneIndex);
				}
			}
			return result;
		}

		public void ResetFuturePrediction(int boneIndex)
		{
			basePosList[boneIndex] = new float3(0f, -1000000f, 0f);
		}

		public void ResetBoneFromTransform()
		{
			if (RestoreBoneCount > 0)
			{
				RestoreBoneJob restoreBoneJob = default(RestoreBoneJob);
				restoreBoneJob.localPosList = restoreBoneLocalPosList.ToJobArray();
				restoreBoneJob.localRotList = restoreBoneLocalRotList.ToJobArray();
				RestoreBoneJob jobData = restoreBoneJob;
				base.Compute.MasterJob = jobData.Schedule(restoreBoneList.GetTransformAccessArray(), base.Compute.MasterJob);
			}
		}

		public void ReadBoneFromTransform()
		{
			if (ReadBoneCount > 0)
			{
				UpdateTimeManager updateTime = manager.UpdateTime;
				ReadBoneJob0 readBoneJob = default(ReadBoneJob0);
				readBoneJob.bonePosList = bonePosList.ToJobArray();
				readBoneJob.boneRotList = boneRotList.ToJobArray();
				readBoneJob.basePosList = basePosList.ToJobArray();
				readBoneJob.baseRotList = baseRotList.ToJobArray();
				ReadBoneJob0 jobData = readBoneJob;
				base.Compute.MasterJob = jobData.Schedule(boneList.GetTransformAccessArray(), base.Compute.MasterJob);
			}
		}

		public void ReadBoneScaleFromTransform()
		{
			if (ReadBoneCount <= 0)
			{
				return;
			}
			int i = 0;
			for (int length = boneList.Length; i < length; i++)
			{
				Transform transform = boneList[i];
				if ((bool)transform)
				{
					boneSclList[i] = transform.lossyScale;
				}
			}
		}

		public void ConvertWorldToLocal()
		{
			if (WriteBoneCount > 0)
			{
				ConvertWorldToLocalJob convertWorldToLocalJob = default(ConvertWorldToLocalJob);
				convertWorldToLocalJob.writeBoneIndexList = writeBoneIndexList.ToJobArray();
				convertWorldToLocalJob.bonePosList = bonePosList.ToJobArray();
				convertWorldToLocalJob.boneRotList = boneRotList.ToJobArray();
				convertWorldToLocalJob.boneSclList = boneSclList.ToJobArray();
				convertWorldToLocalJob.boneParentIndexList = boneParentIndexList.ToJobArray();
				convertWorldToLocalJob.writeBonePosList = writeBonePosList.ToJobArray();
				convertWorldToLocalJob.writeBoneRotList = writeBoneRotList.ToJobArray();
				ConvertWorldToLocalJob jobData = convertWorldToLocalJob;
				base.Compute.MasterJob = jobData.Schedule(writeBoneIndexList.Length, 16, base.Compute.MasterJob);
			}
		}

		public void WriteBoneToTransform(int bufferIndex)
		{
			if (WriteBoneCount > 0)
			{
				WriteBontToTransformJob2 writeBontToTransformJob = default(WriteBontToTransformJob2);
				writeBontToTransformJob.writeBoneIndexList = writeBoneIndexList.ToJobArray(bufferIndex);
				writeBontToTransformJob.boneParentIndexList = boneParentIndexList.ToJobArray();
				writeBontToTransformJob.writeBonePosList = writeBonePosList.ToJobArray(bufferIndex);
				writeBontToTransformJob.writeBoneRotList = writeBoneRotList.ToJobArray(bufferIndex);
				WriteBontToTransformJob2 jobData = writeBontToTransformJob;
				base.Compute.MasterJob = jobData.Schedule(writeBoneList.GetTransformAccessArray(), base.Compute.MasterJob);
			}
		}

		public void CopyBoneBuffer()
		{
			CopyBoneJob0 jobData = default(CopyBoneJob0);
			jobData.bonePosList = writeBonePosList.ToJobArray();
			jobData.boneRotList = writeBoneRotList.ToJobArray();
			jobData.backBonePosList = writeBonePosList.ToJobArray(1);
			jobData.backBoneRotList = writeBoneRotList.ToJobArray(1);
			JobHandle job = jobData.Schedule(writeBonePosList.Length, 16);
			CopyBoneJob1 jobData2 = default(CopyBoneJob1);
			jobData2.writeBoneIndexList = writeBoneIndexList.ToJobArray();
			jobData2.backWriteBoneIndexList = writeBoneIndexList.ToJobArray(1);
			JobHandle job2 = jobData2.Schedule(writeBoneIndexList.Length, 16);
			base.Compute.MasterJob = JobHandle.CombineDependencies(job, job2);
		}
	}
}
