using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public class PhysicsManagerWindData : PhysicsManagerAccess
	{
		public enum WindType
		{
			None = 0,
			Direction = 1
		}

		public struct WindData
		{
			public WindType windType;

			public uint flag;

			public int transformIndex;

			public float main;

			public float turbulence;

			public float3 direction;

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

			public bool IsActive()
			{
				return (flag & 1) != 0;
			}
		}

		[BurstCompile]
		private struct UpdateWindJob : IJobParallelFor
		{
			public float dtime;

			public float elapsedTime;

			[ReadOnly]
			public NativeArray<float3> bonePosList;

			[ReadOnly]
			public NativeArray<quaternion> boneRotList;

			public NativeArray<WindData> windData;

			public void Execute(int index)
			{
				WindData value = windData[index];
				if (value.IsActive() && value.transformIndex >= 0)
				{
					float3 @float = bonePosList[value.transformIndex];
					quaternion a = boneRotList[value.transformIndex];
					float num = value.main / 30f;
					float num2 = 1f + 2f * num;
					float num3 = 15f + 15f * num;
					float2 v = new float2(@float.x, @float.z) * 0.1f;
					float2 v2 = new float2(@float.x, @float.z) * 0.1f;
					v.x += elapsedTime * num2;
					v2.y += elapsedTime * num2;
					float num4 = noise.snoise(v);
					float num5 = noise.snoise(v2);
					float num6 = math.radians(num4 * num3);
					float num7 = math.radians(num5 * num3);
					float x = num6 * value.turbulence;
					num7 *= value.turbulence;
					quaternion b = quaternion.Euler(x, num7, 0f);
					float3 direction = math.forward(math.mul(a, b));
					value.direction = direction;
					windData[index] = value;
				}
			}
		}

		public const uint Flag_Enable = 1u;

		public FixedNativeList<WindData> windDataList;

		private List<int> directionalWindList = new List<int>();

		public int DirectionalWindId
		{
			get
			{
				if (directionalWindList.Count > 0)
				{
					return directionalWindList[directionalWindList.Count - 1];
				}
				return -1;
			}
		}

		public override void Create()
		{
			windDataList = new FixedNativeList<WindData>();
		}

		public override void Dispose()
		{
			if (windDataList != null)
			{
				windDataList.Dispose();
			}
		}

		public int CreateWind(WindType windType, float main, float turbulence)
		{
			WindData element = default(WindData);
			uint flag = 1u;
			element.flag = flag;
			element.windType = windType;
			element.transformIndex = -1;
			element.main = main;
			element.turbulence = turbulence;
			int num = windDataList.Add(element);
			if (windType == WindType.Direction)
			{
				directionalWindList.Add(num);
			}
			return num;
		}

		public void RemoveWind(int windId)
		{
			if (windId >= 0)
			{
				windDataList.Remove(windId);
				directionalWindList.Remove(windId);
			}
		}

		public void SetEnable(int windId, bool sw, Transform target)
		{
			if (windId < 0)
			{
				return;
			}
			WindData value = windDataList[windId];
			value.SetEnable(sw);
			if (sw)
			{
				if (value.transformIndex == -1)
				{
					value.transformIndex = base.Bone.AddBone(target);
				}
			}
			else if (value.transformIndex >= 0)
			{
				base.Bone.RemoveBone(value.transformIndex);
				value.transformIndex = -1;
			}
			windDataList[windId] = value;
		}

		public bool IsActive(int windId)
		{
			if (windId >= 0)
			{
				return windDataList[windId].IsActive();
			}
			return false;
		}

		public void SetFlag(int windId, uint flag, bool sw)
		{
			if (windId >= 0)
			{
				WindData value = windDataList[windId];
				value.SetFlag(flag, sw);
				windDataList[windId] = value;
			}
		}

		public void SetParameter(int windId, float main, float turbulence)
		{
			if (windId >= 0)
			{
				WindData value = windDataList[windId];
				value.main = main;
				value.turbulence = turbulence;
				windDataList[windId] = value;
			}
		}

		public void UpdateWind()
		{
			UpdateWindJob updateWindJob = default(UpdateWindJob);
			updateWindJob.dtime = manager.UpdateTime.DeltaTime;
			updateWindJob.elapsedTime = Time.time;
			updateWindJob.bonePosList = base.Bone.bonePosList.ToJobArray();
			updateWindJob.boneRotList = base.Bone.boneRotList.ToJobArray();
			updateWindJob.windData = windDataList.ToJobArray();
			UpdateWindJob jobData = updateWindJob;
			base.Compute.MasterJob = jobData.Schedule(windDataList.Length, 8, base.Compute.MasterJob);
		}
	}
}
