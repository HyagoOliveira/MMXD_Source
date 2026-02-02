using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
	public class VirtualMeshWorker : PhysicsManagerWorker
	{
		[BurstCompile]
		private struct ReadMeshPositionJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<PhysicsManagerMeshData.VirtualMeshInfo> virtualMeshInfoList;

			[ReadOnly]
			public NativeArray<PhysicsManagerMeshData.SharedVirtualMeshInfo> sharedVirtualMeshInfoList;

			[ReadOnly]
			public NativeArray<short> virtualVertexMeshIndexList;

			[ReadOnly]
			public NativeArray<byte> virtualVertexUseList;

			[ReadOnly]
			public NativeArray<int> virtualTransformIndexList;

			[ReadOnly]
			public NativeArray<uint> sharedVirtualVertexInfoList;

			[ReadOnly]
			public NativeArray<MeshData.VertexWeight> sharedVirtualWeightList;

			[ReadOnly]
			public NativeArray<float3> transformPosList;

			[ReadOnly]
			public NativeArray<quaternion> transformRotList;

			[ReadOnly]
			public NativeArray<float3> transformSclList;

			[WriteOnly]
			public NativeArray<float3> virtualPosList;

			[WriteOnly]
			public NativeArray<quaternion> virtualRotList;

			[WriteOnly]
			public NativeArray<byte> virtualVertexFlagList;

			public void Execute(int vindex)
			{
				virtualVertexFlagList[vindex] = 0;
				if (virtualVertexUseList[vindex] == 0)
				{
					return;
				}
				int num = virtualVertexMeshIndexList[vindex];
				PhysicsManagerMeshData.VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[num - 1];
				if (!virtualMeshInfo.IsUse())
				{
					return;
				}
				PhysicsManagerMeshData.SharedVirtualMeshInfo sharedVirtualMeshInfo = sharedVirtualMeshInfoList[virtualMeshInfo.sharedVirtualMeshIndex];
				int num2 = vindex - virtualMeshInfo.vertexChunk.startIndex;
				int index = sharedVirtualMeshInfo.vertexChunk.startIndex + num2;
				int startIndex = sharedVirtualMeshInfo.weightChunk.startIndex;
				int startIndex2 = virtualMeshInfo.boneChunk.startIndex;
				float3 value = 0;
				float3 forward = 0;
				float3 up = 0;
				uint pack = sharedVirtualVertexInfoList[index];
				int num3 = DataUtility.Unpack4_28Hi(pack);
				int num4 = DataUtility.Unpack4_28Low(pack);
				for (int i = 0; i < num3; i++)
				{
					MeshData.VertexWeight vertexWeight = sharedVirtualWeightList[startIndex + num4 + i];
					int index2 = virtualTransformIndexList[startIndex2 + vertexWeight.parentIndex];
					float3 @float = transformPosList[index2];
					quaternion q = transformRotList[index2];
					float3 float2 = transformSclList[index2];
					value += (@float + math.mul(q, vertexWeight.localPos * float2)) * vertexWeight.weight;
					if (float2.x < 0f || float2.y < 0f || float2.z < 0f)
					{
						quaternion q2 = new quaternion(quaternion.LookRotation(vertexWeight.localNor, vertexWeight.localTan).value * new float4(-math.sign(float2), 1f));
						forward += math.mul(q, math.mul(q2, new float3(0f, 0f, 1f))) * vertexWeight.weight;
						up += math.mul(q, math.mul(q2, new float3(0f, 1f, 0f))) * vertexWeight.weight;
					}
					else
					{
						forward += math.mul(q, vertexWeight.localNor) * vertexWeight.weight;
						up += math.mul(q, vertexWeight.localTan) * vertexWeight.weight;
					}
				}
				virtualPosList[vindex] = value;
				virtualRotList[vindex] = quaternion.LookRotation(forward, up);
			}
		}

		[BurstCompile]
		private struct CalcMeshTriangleNormalTangentJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<PhysicsManagerMeshData.VirtualMeshInfo> virtualMeshInfoList;

			[ReadOnly]
			public NativeArray<PhysicsManagerMeshData.SharedVirtualMeshInfo> sharedVirtualMeshInfoList;

			[ReadOnly]
			public NativeArray<ushort> virtualTriangleMeshIndexList;

			[ReadOnly]
			public NativeArray<byte> virtualVertexUseList;

			[ReadOnly]
			public NativeArray<float3> virtualPosList;

			[ReadOnly]
			public NativeArray<int> sharedTriangles;

			[ReadOnly]
			public NativeArray<float2> sharedMeshUv;

			[WriteOnly]
			public NativeArray<float3> virtualTriangleNormalList;

			[WriteOnly]
			public NativeArray<float3> virtualTriangleTangentList;

			[ReadOnly]
			public NativeArray<float3> transformSclList;

			public void Execute(int tindex)
			{
				virtualTriangleNormalList[tindex] = 0;
				virtualTriangleTangentList[tindex] = 0;
				int num = virtualTriangleMeshIndexList[tindex];
				if (num == 0)
				{
					return;
				}
				PhysicsManagerMeshData.VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[num - 1];
				if (!virtualMeshInfo.IsUse())
				{
					return;
				}
				PhysicsManagerMeshData.SharedVirtualMeshInfo sharedVirtualMeshInfo = sharedVirtualMeshInfoList[virtualMeshInfo.sharedVirtualMeshIndex];
				int startIndex = virtualMeshInfo.vertexChunk.startIndex;
				int startIndex2 = sharedVirtualMeshInfo.vertexChunk.startIndex;
				int startIndex3 = sharedVirtualMeshInfo.triangleChunk.startIndex;
				int num2 = tindex - virtualMeshInfo.triangleChunk.startIndex;
				int num3 = sharedTriangles[startIndex3 + num2 * 3];
				int num4 = sharedTriangles[startIndex3 + num2 * 3 + 1];
				int num5 = sharedTriangles[startIndex3 + num2 * 3 + 2];
				int index = startIndex + num3;
				int index2 = startIndex + num4;
				int index3 = startIndex + num5;
				if (virtualVertexUseList[index] != 0 && virtualVertexUseList[index2] != 0 && virtualVertexUseList[index3] != 0)
				{
					float3 @float = virtualPosList[index];
					float3 float2 = virtualPosList[index2];
					float3 float3 = virtualPosList[index3];
					float3 float4 = float2 - @float;
					float3 y = float3 - @float;
					float3 x = float4 * 1000f;
					y *= 1000f;
					float3 float5 = math.normalize(math.cross(x, y));
					float2 float6 = sharedMeshUv[startIndex2 + num3];
					float2 float7 = sharedMeshUv[startIndex2 + num4];
					float2 float8 = sharedMeshUv[startIndex2 + num5];
					float3 float9 = float2 - @float;
					float3 float10 = float3 - @float;
					float2 float11 = float7 - float6;
					float2 float12 = float8 - float6;
					float num6 = float11.x * float12.y - float11.y * float12.x;
					float3 float13 = 1;
					if (num6 != 0f)
					{
						float num7 = 1f / num6;
						float13 = new float3(float9.x * float12.y + float10.x * (0f - float11.y), float9.y * float12.y + float10.y * (0f - float11.y), float9.z * float12.y + float10.z * (0f - float11.y)) * num7;
						float13 = -float13;
					}
					float3 float14 = transformSclList[virtualMeshInfo.transformIndex];
					if (float14.x < 0f)
					{
						float5 = -float5;
					}
					else if (float14.y < 0f)
					{
						float5 = -float5;
						float13 = -float13;
					}
					virtualTriangleNormalList[tindex] = float5;
					virtualTriangleTangentList[tindex] = float13;
				}
			}
		}

		[BurstCompile]
		private struct CalcVertexNormalTangentFromTriangleJob : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<PhysicsManagerMeshData.VirtualMeshInfo> virtualMeshInfoList;

			[ReadOnly]
			public NativeArray<PhysicsManagerMeshData.SharedVirtualMeshInfo> sharedVirtualMeshInfoList;

			[ReadOnly]
			public NativeArray<short> virtualVertexMeshIndexList;

			[ReadOnly]
			public NativeArray<byte> virtualVertexUseList;

			[ReadOnly]
			public NativeArray<byte> virtualVertexFlagList;

			[ReadOnly]
			public NativeArray<uint> sharedVirtualVertexToTriangleInfoList;

			[ReadOnly]
			public NativeArray<int> sharedVirtualVertexToTriangleIndexList;

			[ReadOnly]
			public NativeArray<float3> virtualTriangleNormalList;

			[ReadOnly]
			public NativeArray<float3> virtualTriangleTangentList;

			[WriteOnly]
			public NativeArray<quaternion> virtualRotList;

			public void Execute(int vindex)
			{
				if (virtualVertexFlagList[vindex] == 0 || virtualVertexUseList[vindex] == 0)
				{
					return;
				}
				int num = virtualVertexMeshIndexList[vindex];
				PhysicsManagerMeshData.VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[num - 1];
				if (!virtualMeshInfo.IsUse())
				{
					return;
				}
				PhysicsManagerMeshData.SharedVirtualMeshInfo sharedVirtualMeshInfo = sharedVirtualMeshInfoList[virtualMeshInfo.sharedVirtualMeshIndex];
				int startIndex = sharedVirtualMeshInfo.vertexChunk.startIndex;
				int num2 = vindex - virtualMeshInfo.vertexChunk.startIndex;
				int startIndex2 = virtualMeshInfo.triangleChunk.startIndex;
				uint pack = sharedVirtualVertexToTriangleInfoList[startIndex + num2];
				int num3 = DataUtility.Unpack8_24Hi(pack);
				int num4 = DataUtility.Unpack8_24Low(pack);
				if (num3 != 0)
				{
					num4 += sharedVirtualMeshInfo.vertexToTriangleChunk.startIndex;
					float3 x = 0;
					float3 x2 = 0;
					for (int i = 0; i < num3; i++)
					{
						int index = sharedVirtualVertexToTriangleIndexList[num4 + i] + startIndex2;
						x += virtualTriangleNormalList[index];
						x2 += virtualTriangleTangentList[index];
					}
					x = math.normalize(x);
					x2 = math.normalize(x2);
					virtualRotList[vindex] = quaternion.LookRotation(x, x2);
				}
			}
		}

		public override void Create()
		{
		}

		public override void Release()
		{
		}

		public override void RemoveGroup(int group)
		{
		}

		public override void Warmup()
		{
		}

		public override JobHandle PreUpdate(JobHandle jobHandle)
		{
			if (base.Manager.Mesh.VirtualMeshUseCount == 0)
			{
				return jobHandle;
			}
			ReadMeshPositionJob jobData = default(ReadMeshPositionJob);
			jobData.virtualMeshInfoList = base.Manager.Mesh.virtualMeshInfoList.ToJobArray();
			jobData.sharedVirtualMeshInfoList = base.Manager.Mesh.sharedVirtualMeshInfoList.ToJobArray();
			jobData.virtualVertexMeshIndexList = base.Manager.Mesh.virtualVertexMeshIndexList.ToJobArray();
			jobData.virtualVertexUseList = base.Manager.Mesh.virtualVertexUseList.ToJobArray();
			jobData.virtualTransformIndexList = base.Manager.Mesh.virtualTransformIndexList.ToJobArray();
			jobData.sharedVirtualVertexInfoList = base.Manager.Mesh.sharedVirtualVertexInfoList.ToJobArray();
			jobData.sharedVirtualWeightList = base.Manager.Mesh.sharedVirtualWeightList.ToJobArray();
			jobData.transformPosList = base.Manager.Bone.bonePosList.ToJobArray();
			jobData.transformRotList = base.Manager.Bone.boneRotList.ToJobArray();
			jobData.transformSclList = base.Manager.Bone.boneSclList.ToJobArray();
			jobData.virtualPosList = base.Manager.Mesh.virtualPosList.ToJobArray();
			jobData.virtualRotList = base.Manager.Mesh.virtualRotList.ToJobArray();
			jobData.virtualVertexFlagList = base.Manager.Mesh.virtualVertexFlagList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Mesh.virtualPosList.Length, 64, jobHandle);
			return jobHandle;
		}

		public override JobHandle PostUpdate(JobHandle jobHandle)
		{
			if (base.Manager.Mesh.VirtualMeshUseCount == 0)
			{
				return jobHandle;
			}
			CalcMeshTriangleNormalTangentJob jobData = default(CalcMeshTriangleNormalTangentJob);
			jobData.virtualMeshInfoList = base.Manager.Mesh.virtualMeshInfoList.ToJobArray();
			jobData.sharedVirtualMeshInfoList = base.Manager.Mesh.sharedVirtualMeshInfoList.ToJobArray();
			jobData.virtualTriangleMeshIndexList = base.Manager.Mesh.virtualTriangleMeshIndexList.ToJobArray();
			jobData.virtualVertexUseList = base.Manager.Mesh.virtualVertexUseList.ToJobArray();
			jobData.virtualPosList = base.Manager.Mesh.virtualPosList.ToJobArray();
			jobData.sharedTriangles = base.Manager.Mesh.sharedVirtualTriangleList.ToJobArray();
			jobData.sharedMeshUv = base.Manager.Mesh.sharedVirtualUvList.ToJobArray();
			jobData.virtualTriangleNormalList = base.Manager.Mesh.virtualTriangleNormalList.ToJobArray();
			jobData.virtualTriangleTangentList = base.Manager.Mesh.virtualTriangleTangentList.ToJobArray();
			jobData.transformSclList = base.Manager.Bone.boneSclList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Mesh.virtualTriangleMeshIndexList.Length, 128, jobHandle);
			CalcVertexNormalTangentFromTriangleJob jobData2 = default(CalcVertexNormalTangentFromTriangleJob);
			jobData2.virtualMeshInfoList = base.Manager.Mesh.virtualMeshInfoList.ToJobArray();
			jobData2.sharedVirtualMeshInfoList = base.Manager.Mesh.sharedVirtualMeshInfoList.ToJobArray();
			jobData2.virtualVertexMeshIndexList = base.Manager.Mesh.virtualVertexMeshIndexList.ToJobArray();
			jobData2.virtualVertexUseList = base.Manager.Mesh.virtualVertexUseList.ToJobArray();
			jobData2.virtualVertexFlagList = base.Manager.Mesh.virtualVertexFlagList.ToJobArray();
			jobData2.sharedVirtualVertexToTriangleInfoList = base.Manager.Mesh.sharedVirtualVertexToTriangleInfoList.ToJobArray();
			jobData2.sharedVirtualVertexToTriangleIndexList = base.Manager.Mesh.sharedVirtualVertexToTriangleIndexList.ToJobArray();
			jobData2.virtualTriangleNormalList = base.Manager.Mesh.virtualTriangleNormalList.ToJobArray();
			jobData2.virtualTriangleTangentList = base.Manager.Mesh.virtualTriangleTangentList.ToJobArray();
			jobData2.virtualRotList = base.Manager.Mesh.virtualRotList.ToJobArray();
			jobHandle = jobData2.Schedule(base.Manager.Mesh.virtualPosList.Length, 128, jobHandle);
			return jobHandle;
		}
	}
}
