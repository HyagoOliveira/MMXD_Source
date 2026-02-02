using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class MeshParticleWorker : PhysicsManagerWorker
	{
		private struct CreateData
		{
			public int vertexIndex;

			public int particleIndex;
		}

		[BurstCompile]
		private struct VertexToParticleJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<int> vertexToParticleList;

			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> vertexToParticleMap;

			[ReadOnly]
			public NativeArray<float3> posList;

			[ReadOnly]
			public NativeArray<quaternion> rotList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> basePosList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<quaternion> baseRotList;

			private NativeParallelMultiHashMapIterator<int> iterator;

			public void Execute(int index)
			{
				int num = vertexToParticleList[index];
				int item;
				if (num >= 0 && vertexToParticleMap.TryGetFirstValue(num, out item, out iterator))
				{
					float3 value = posList[num];
					quaternion value2 = rotList[num];
					do
					{
						basePosList[item] = value;
						baseRotList[item] = value2;
					}
					while (vertexToParticleMap.TryGetNextValue(out item, ref iterator));
				}
			}
		}

		[BurstCompile]
		private struct ParticleToVertexJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<int> vertexToParticleList;

			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> vertexToParticleMap;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> virtualPosList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<quaternion> virtualRotList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<byte> virtualVertexFlagList;

			[ReadOnly]
			public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

			[ReadOnly]
			public NativeArray<int> teamIdList;

			[ReadOnly]
			public NativeArray<PhysicsManagerParticleData.ParticleFlag> particleFlagList;

			[ReadOnly]
			public NativeArray<float3> particlePosList;

			[ReadOnly]
			public NativeArray<quaternion> particleRotList;

			private NativeParallelMultiHashMapIterator<int> iterator;

			public void Execute(int index)
			{
				int num = vertexToParticleList[index];
				int item;
				if (num < 0 || !vertexToParticleMap.TryGetFirstValue(num, out item, out iterator))
				{
					return;
				}
				float3 value = 0;
				float3 x = 0;
				float3 x2 = 0;
				int num2 = 0;
				do
				{
					if (particleFlagList[item].IsKinematic() && teamDataList[teamIdList[item]].IsFlag(4u))
					{
						return;
					}
					float3 @float = particlePosList[item];
					quaternion q = particleRotList[item];
					value += @float;
					x += math.mul(q, new float3(0f, 0f, 1f));
					x2 += math.mul(q, new float3(0f, 1f, 0f));
					num2++;
				}
				while (vertexToParticleMap.TryGetNextValue(out item, ref iterator));
				if (num2 > 0)
				{
					value /= (float)num2;
					x = math.normalize(x);
					x2 = math.normalize(x2);
					virtualPosList[num] = value;
					virtualRotList[num] = quaternion.LookRotation(x, x2);
					virtualVertexFlagList[num] = 1;
				}
			}
		}

		private ExNativeMultiHashMap<int, int> vertexToParticleMap;

		private FixedNativeListWithCount<int> vertexToParticleList;

		private Dictionary<int, List<CreateData>> groupCreateDict = new Dictionary<int, List<CreateData>>();

		public override void Create()
		{
			vertexToParticleMap = new ExNativeMultiHashMap<int, int>();
			vertexToParticleList = new FixedNativeListWithCount<int>();
			vertexToParticleList.SetEmptyElement(-1);
		}

		public override void Release()
		{
			vertexToParticleMap.Dispose();
			vertexToParticleList.Dispose();
		}

		public void Add(int group, int vindex, int pindex)
		{
			vertexToParticleMap.Add(vindex, pindex);
			vertexToParticleList.Add(vindex);
			if (!groupCreateDict.ContainsKey(group))
			{
				groupCreateDict.Add(group, new List<CreateData>());
			}
			groupCreateDict[group].Add(new CreateData
			{
				vertexIndex = vindex,
				particleIndex = pindex
			});
		}

		public override void RemoveGroup(int group)
		{
			if (!groupCreateDict.ContainsKey(group))
			{
				return;
			}
			foreach (CreateData item in groupCreateDict[group])
			{
				vertexToParticleMap.Remove(item.vertexIndex, item.particleIndex);
				vertexToParticleList.Remove(item.vertexIndex);
			}
			groupCreateDict.Remove(group);
		}

		public override void Warmup()
		{
		}

		public override JobHandle PreUpdate(JobHandle jobHandle)
		{
			if (vertexToParticleList.Count == 0)
			{
				return jobHandle;
			}
			VertexToParticleJob jobData = default(VertexToParticleJob);
			jobData.vertexToParticleList = vertexToParticleList.ToJobArray();
			jobData.vertexToParticleMap = vertexToParticleMap.Map;
			jobData.posList = base.Manager.Mesh.virtualPosList.ToJobArray();
			jobData.rotList = base.Manager.Mesh.virtualRotList.ToJobArray();
			jobData.basePosList = base.Manager.Particle.basePosList.ToJobArray();
			jobData.baseRotList = base.Manager.Particle.baseRotList.ToJobArray();
			jobHandle = jobData.Schedule(vertexToParticleList.Length, 64, jobHandle);
			return jobHandle;
		}

		public override JobHandle PostUpdate(JobHandle jobHandle)
		{
			if (vertexToParticleList.Count == 0)
			{
				return jobHandle;
			}
			ParticleToVertexJob jobData = default(ParticleToVertexJob);
			jobData.vertexToParticleList = vertexToParticleList.ToJobArray();
			jobData.vertexToParticleMap = vertexToParticleMap.Map;
			jobData.virtualPosList = base.Manager.Mesh.virtualPosList.ToJobArray();
			jobData.virtualRotList = base.Manager.Mesh.virtualRotList.ToJobArray();
			jobData.virtualVertexFlagList = base.Manager.Mesh.virtualVertexFlagList.ToJobArray();
			jobData.teamDataList = base.Manager.Team.teamDataList.ToJobArray();
			jobData.teamIdList = base.Manager.Particle.teamIdList.ToJobArray();
			jobData.particleFlagList = base.Manager.Particle.flagList.ToJobArray();
			jobData.particlePosList = base.Manager.Particle.posList.ToJobArray();
			jobData.particleRotList = base.Manager.Particle.rotList.ToJobArray();
			jobHandle = jobData.Schedule(vertexToParticleList.Length, 64, jobHandle);
			return jobHandle;
		}
	}
}
