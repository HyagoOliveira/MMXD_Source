using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class SpringMeshWorker : PhysicsManagerWorker
	{
		public struct SpringData
		{
			public int particleIndex;

			public float weight;
		}

		[BurstCompile]
		private struct SpringJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<int> springVertexList;

			[ReadOnly]
			public NativeParallelMultiHashMap<int, SpringData> springMap;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

			[ReadOnly]
			public NativeArray<float3> particlePosList;

			[ReadOnly]
			public NativeArray<quaternion> particleRotList;

			[ReadOnly]
			public NativeArray<float3> particleBasePosList;

			[ReadOnly]
			public NativeArray<quaternion> particleBaseRotList;

			[NativeDisableParallelForRestriction]
			public NativeArray<float3> virtualPosList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<byte> virtualVertexFlagList;

			private NativeParallelMultiHashMapIterator<int> iterator;

			public void Execute(int index)
			{
				int num = springVertexList[index];
				if (num < 0)
				{
					return;
				}
				float num2 = 0f;
				SpringData item;
				if (springMap.TryGetFirstValue(num, out item, out iterator))
				{
					do
					{
						if (flagList[item.particleIndex].IsValid())
						{
							num2 += item.weight;
						}
					}
					while (springMap.TryGetNextValue(out item, ref iterator));
				}
				if (!(num2 > 0f) || !springMap.TryGetFirstValue(num, out item, out iterator))
				{
					return;
				}
				float3 @float = virtualPosList[num];
				float3 value = 0;
				do
				{
					int particleIndex = item.particleIndex;
					if (flagList[item.particleIndex].IsValid())
					{
						float3 float2 = particlePosList[particleIndex];
						quaternion q = particleRotList[particleIndex];
						float3 v2 = math.mul(v: @float - particleBasePosList[particleIndex], q: math.inverse(particleBaseRotList[particleIndex]));
						float3 y = math.mul(q, v2) + float2;
						y = math.lerp(@float, y, item.weight);
						value += y * (item.weight / num2);
					}
				}
				while (springMap.TryGetNextValue(out item, ref iterator));
				virtualPosList[num] = value;
				virtualVertexFlagList[num] = 1;
			}
		}

		private ExNativeMultiHashMap<int, SpringData> springMap;

		private FixedNativeListWithCount<int> springVertexList;

		private Dictionary<int, List<int>> groupIndexDict = new Dictionary<int, List<int>>();

		public override void Create()
		{
			springMap = new ExNativeMultiHashMap<int, SpringData>();
			springVertexList = new FixedNativeListWithCount<int>();
			springVertexList.SetEmptyElement(-1);
		}

		public override void Release()
		{
			springMap.Dispose();
			springVertexList.Dispose();
		}

		public void Add(int group, int vertexIndex, int particleIndex, float weight)
		{
			SpringData springData = default(SpringData);
			springData.particleIndex = particleIndex;
			springData.weight = math.saturate(weight);
			SpringData value = springData;
			springMap.Add(vertexIndex, value);
			springVertexList.Add(vertexIndex);
			if (!groupIndexDict.ContainsKey(group))
			{
				groupIndexDict.Add(group, new List<int>());
			}
			groupIndexDict[group].Add(vertexIndex);
		}

		public override void RemoveGroup(int group)
		{
			if (!groupIndexDict.ContainsKey(group))
			{
				return;
			}
			foreach (int item in groupIndexDict[group])
			{
				springVertexList.Remove(item);
				springMap.Remove(item);
			}
			groupIndexDict.Remove(group);
		}

		public override void Warmup()
		{
		}

		public override JobHandle PreUpdate(JobHandle jobHandle)
		{
			return jobHandle;
		}

		public override JobHandle PostUpdate(JobHandle jobHandle)
		{
			if (springMap.Count == 0)
			{
				return jobHandle;
			}
			SpringJob jobData = default(SpringJob);
			jobData.springVertexList = springVertexList.ToJobArray();
			jobData.springMap = springMap.Map;
			jobData.flagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.particlePosList = base.Manager.Particle.posList.ToJobArray();
			jobData.particleRotList = base.Manager.Particle.rotList.ToJobArray();
			jobData.particleBasePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.particleBaseRotList = base.Manager.Particle.baseRotList.ToJobArray();
			jobData.virtualPosList = base.Manager.Mesh.virtualPosList.ToJobArray();
			jobData.virtualVertexFlagList = base.Manager.Mesh.virtualVertexFlagList.ToJobArray();
			jobHandle = jobData.Schedule(springVertexList.Length, 64, jobHandle);
			return jobHandle;
		}
	}
}
