using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public class RenderMeshWorker : PhysicsManagerWorker
	{
		[BurstCompile]
		private struct CalcVertexUseFlagJob : IJobParallelFor
		{
			public uint updateFlag;

			public NativeArray<PhysicsManagerMeshData.RenderMeshInfo> renderMeshInfoList;

			[ReadOnly]
			public NativeArray<PhysicsManagerMeshData.SharedRenderMeshInfo> sharedRenderMeshInfoList;

			[ReadOnly]
			public NativeArray<byte> virtualVertexUseList;

			[ReadOnly]
			public NativeArray<byte> virtualVertexFixList;

			[ReadOnly]
			public NativeArray<uint> sharedChildVertexInfoList;

			[ReadOnly]
			public NativeArray<MeshData.VertexWeight> sharedChildVertexWeightList;

			[ReadOnly]
			public NativeArray<float3> sharedRenderVertices;

			[ReadOnly]
			public NativeArray<float3> sharedRenderNormals;

			[ReadOnly]
			public NativeArray<float4> sharedRenderTangents;

			[ReadOnly]
			public NativeArray<BoneWeight> sharedBoneWeightList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> renderPosList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> renderNormalList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float4> renderTangentList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<BoneWeight> renderBoneWeightList;

			[NativeDisableParallelForRestriction]
			public NativeArray<uint> renderVertexFlagList;

			public void Execute(int rmindex)
			{
				PhysicsManagerMeshData.RenderMeshInfo value = renderMeshInfoList[rmindex];
				if (!value.InUse() || !value.IsFlag(updateFlag))
				{
					return;
				}
				PhysicsManagerMeshData.SharedRenderMeshInfo sharedRenderMeshInfo = sharedRenderMeshInfoList[value.renderSharedMeshIndex];
				int4 @int = default(int4);
				for (int i = 0; i < value.vertexChunk.dataLength; i++)
				{
					int index = value.vertexChunk.startIndex + i;
					uint num = renderVertexFlagList[index];
					num &= 0xFFFFu;
					uint num2 = 65536u;
					for (int j = 0; j < 4; j++)
					{
						if (value.IsLinkMesh(j))
						{
							@int.x = value.childMeshVertexStartIndex[j];
							@int.y = value.childMeshWeightStartIndex[j];
							@int.z = value.virtualMeshVertexStartIndex[j];
							@int.w = value.sharedVirtualMeshVertexStartIndex[j];
							int y = @int.y;
							int z = @int.z;
							int index2 = @int.x + i;
							uint pack = sharedChildVertexInfoList[index2];
							int num3 = DataUtility.Unpack4_28Hi(pack);
							int num4 = DataUtility.Unpack4_28Low(pack);
							int num5 = 0;
							int num6 = num3 * 75 / 100;
							int k;
							for (k = 0; k < num3; k++)
							{
								int index3 = z + sharedChildVertexWeightList[y + num4 + k].parentIndex;
								if (virtualVertexUseList[index3] == 0)
								{
									break;
								}
								if (virtualVertexFixList[index3] > 0)
								{
									num5++;
									if (num5 > num6)
									{
										break;
									}
								}
							}
							if (num3 == k)
							{
								num |= num2;
							}
						}
						num2 <<= 1;
					}
					renderVertexFlagList[index] = num;
					int index4 = value.sharedRenderMeshVertexStartIndex + i;
					if ((num & 0xFFFF0000u) == 0)
					{
						renderPosList[index] = sharedRenderVertices[index4];
						renderNormalList[index] = sharedRenderNormals[index4];
						renderTangentList[index] = sharedRenderTangents[index4];
					}
					if (sharedRenderMeshInfo.IsSkinning())
					{
						int index5 = value.boneWeightsChunk.startIndex + i;
						if ((num & 0xFFFF0000u) == 0)
						{
							int index6 = sharedRenderMeshInfo.boneWeightsChunk.startIndex + i;
							renderBoneWeightList[index5] = sharedBoneWeightList[index6];
							continue;
						}
						int rendererBoneIndex = sharedRenderMeshInfo.rendererBoneIndex;
						BoneWeight value2 = default(BoneWeight);
						value2.boneIndex0 = rendererBoneIndex;
						value2.weight0 = 1f;
						renderBoneWeightList[index5] = value2;
					}
				}
				value.SetFlag(updateFlag, false);
				renderMeshInfoList[rmindex] = value;
			}
		}

		[BurstCompile]
		private struct CollectLocalPositionNormalTangentJob3 : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<PhysicsManagerMeshData.RenderMeshInfo> renderMeshInfoList;

			[ReadOnly]
			public NativeArray<float3> transformPosList;

			[ReadOnly]
			public NativeArray<quaternion> transformRotList;

			[ReadOnly]
			public NativeArray<float3> transformSclList;

			[ReadOnly]
			public NativeArray<uint> sharedChildVertexInfoList;

			[ReadOnly]
			public NativeArray<MeshData.VertexWeight> sharedChildVertexWeightList;

			[ReadOnly]
			public NativeArray<float3> virtualPosList;

			[ReadOnly]
			public NativeArray<quaternion> virtualRotList;

			[ReadOnly]
			public NativeArray<uint> renderVertexFlagList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> renderPosList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float3> renderNormalList;

			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<float4> renderTangentList;

			public void Execute(int rmindex)
			{
				PhysicsManagerMeshData.RenderMeshInfo renderMeshInfo = renderMeshInfoList[rmindex];
				if (!renderMeshInfo.InUse())
				{
					return;
				}
				int transformIndex = renderMeshInfo.transformIndex;
				float3 @float = transformPosList[transformIndex];
				quaternion q = transformRotList[transformIndex];
				float3 float2 = transformSclList[transformIndex];
				quaternion q2 = math.inverse(q);
				bool flag = renderMeshInfo.IsFlag(8u);
				bool flag2 = renderMeshInfo.IsFlag(16u);
				float num = ((renderMeshInfo.baseScale > 0f) ? (math.length(float2) / renderMeshInfo.baseScale) : 1f);
				float3 float3 = math.sign(float2);
				int4 @int = default(int4);
				for (int i = 0; i < renderMeshInfo.vertexChunk.dataLength; i++)
				{
					int index = renderMeshInfo.vertexChunk.startIndex + i;
					uint num2 = renderVertexFlagList[index];
					if ((num2 & 0xFFFF0000u) == 0)
					{
						continue;
					}
					float3 float4 = 0;
					float3 float5 = 0;
					float3 float6 = 0;
					float4 value = 0;
					value.w = -1f;
					int num3 = 0;
					uint num4 = 65536u;
					for (int j = 0; j < 4; j++)
					{
						if (renderMeshInfo.IsLinkMesh(j))
						{
							@int.x = renderMeshInfo.childMeshVertexStartIndex[j];
							@int.y = renderMeshInfo.childMeshWeightStartIndex[j];
							@int.z = renderMeshInfo.virtualMeshVertexStartIndex[j];
							@int.w = renderMeshInfo.sharedVirtualMeshVertexStartIndex[j];
							if ((num2 & num4) == 0)
							{
								num4 <<= 1;
								continue;
							}
							float3 float7 = 0;
							float3 v = 0;
							float3 v2 = 0;
							int index2 = @int.x + i;
							int y = @int.y;
							int z = @int.z;
							uint pack = sharedChildVertexInfoList[index2];
							int num5 = DataUtility.Unpack4_28Hi(pack);
							int num6 = DataUtility.Unpack4_28Low(pack);
							if (flag2)
							{
								for (int k = 0; k < num5; k++)
								{
									MeshData.VertexWeight vertexWeight = sharedChildVertexWeightList[y + num6 + k];
									float3 float8 = virtualPosList[z + vertexWeight.parentIndex];
									quaternion q3 = virtualRotList[z + vertexWeight.parentIndex];
									float7 += (float8 + math.mul(q3, vertexWeight.localPos * float3 * num)) * vertexWeight.weight;
									v += math.mul(q3, vertexWeight.localNor * float3) * vertexWeight.weight;
									v2 += math.mul(q3, vertexWeight.localTan * float3) * vertexWeight.weight;
								}
								float7 = math.mul(q2, float7 - @float) / float2;
								v = math.mul(q2, v);
								v2 = math.mul(q2, v2);
								v *= float3;
								v2 *= float3;
								float4 += float7;
								float5 += v;
								float6 += v2;
							}
							else if (flag)
							{
								for (int l = 0; l < num5; l++)
								{
									MeshData.VertexWeight vertexWeight2 = sharedChildVertexWeightList[y + num6 + l];
									float3 float9 = virtualPosList[z + vertexWeight2.parentIndex];
									quaternion q4 = virtualRotList[z + vertexWeight2.parentIndex];
									float7 += (float9 + math.mul(q4, vertexWeight2.localPos * float3 * num)) * vertexWeight2.weight;
									v += math.mul(q4, vertexWeight2.localNor * float3) * vertexWeight2.weight;
								}
								float7 = math.mul(q2, float7 - @float) / float2;
								v = math.mul(q2, v);
								v *= float3;
								float4 += float7;
								float5 += v;
							}
							else
							{
								for (int m = 0; m < num5; m++)
								{
									MeshData.VertexWeight vertexWeight3 = sharedChildVertexWeightList[y + num6 + m];
									float3 float10 = virtualPosList[z + vertexWeight3.parentIndex];
									quaternion q5 = virtualRotList[z + vertexWeight3.parentIndex];
									float7 += (float10 + math.mul(q5, vertexWeight3.localPos * float3 * num)) * vertexWeight3.weight;
								}
								float7 = math.mul(q2, float7 - @float) / float2;
								float4 += float7;
							}
							num3++;
						}
						num4 <<= 1;
					}
					if (num3 > 0)
					{
						renderPosList[index] = float4 / num3;
						if (flag2)
						{
							renderNormalList[index] = float5 / num3;
							value.xyz = float6 / num3;
							renderTangentList[index] = value;
						}
						else if (flag)
						{
							renderNormalList[index] = float5 / num3;
						}
					}
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
			if (base.Manager.Mesh.renderMeshInfoList.Count != 0)
			{
				CalcVertexUseFlagJob calcVertexUseFlagJob = default(CalcVertexUseFlagJob);
				calcVertexUseFlagJob.updateFlag = (uint)(16777216 << base.Manager.Compute.SwapIndex);
				calcVertexUseFlagJob.renderMeshInfoList = base.Manager.Mesh.renderMeshInfoList.ToJobArray();
				calcVertexUseFlagJob.sharedRenderMeshInfoList = base.Manager.Mesh.sharedRenderMeshInfoList.ToJobArray();
				calcVertexUseFlagJob.virtualVertexUseList = base.Manager.Mesh.virtualVertexUseList.ToJobArray();
				calcVertexUseFlagJob.virtualVertexFixList = base.Manager.Mesh.virtualVertexFixList.ToJobArray();
				calcVertexUseFlagJob.sharedChildVertexInfoList = base.Manager.Mesh.sharedChildVertexInfoList.ToJobArray();
				calcVertexUseFlagJob.sharedChildVertexWeightList = base.Manager.Mesh.sharedChildWeightList.ToJobArray();
				calcVertexUseFlagJob.sharedRenderVertices = base.Manager.Mesh.sharedRenderVertices.ToJobArray();
				calcVertexUseFlagJob.sharedRenderNormals = base.Manager.Mesh.sharedRenderNormals.ToJobArray();
				calcVertexUseFlagJob.sharedRenderTangents = base.Manager.Mesh.sharedRenderTangents.ToJobArray();
				calcVertexUseFlagJob.sharedBoneWeightList = base.Manager.Mesh.sharedBoneWeightList.ToJobArray();
				calcVertexUseFlagJob.renderPosList = base.Manager.Mesh.renderPosList.ToJobArray();
				calcVertexUseFlagJob.renderNormalList = base.Manager.Mesh.renderNormalList.ToJobArray();
				calcVertexUseFlagJob.renderTangentList = base.Manager.Mesh.renderTangentList.ToJobArray();
				calcVertexUseFlagJob.renderBoneWeightList = base.Manager.Mesh.renderBoneWeightList.ToJobArray();
				calcVertexUseFlagJob.renderVertexFlagList = base.Manager.Mesh.renderVertexFlagList.ToJobArray();
				CalcVertexUseFlagJob jobData = calcVertexUseFlagJob;
				base.Manager.Compute.MasterJob = jobData.Schedule(base.Manager.Mesh.renderMeshInfoList.Length, 1, base.Manager.Compute.MasterJob);
			}
		}

		public override JobHandle PreUpdate(JobHandle jobHandle)
		{
			return jobHandle;
		}

		public override JobHandle PostUpdate(JobHandle jobHandle)
		{
			if (base.Manager.Mesh.renderMeshInfoList.Count == 0)
			{
				return jobHandle;
			}
			CollectLocalPositionNormalTangentJob3 jobData = default(CollectLocalPositionNormalTangentJob3);
			jobData.renderMeshInfoList = base.Manager.Mesh.renderMeshInfoList.ToJobArray();
			jobData.transformPosList = base.Manager.Bone.bonePosList.ToJobArray();
			jobData.transformRotList = base.Manager.Bone.boneRotList.ToJobArray();
			jobData.transformSclList = base.Manager.Bone.boneSclList.ToJobArray();
			jobData.sharedChildVertexInfoList = base.Manager.Mesh.sharedChildVertexInfoList.ToJobArray();
			jobData.sharedChildVertexWeightList = base.Manager.Mesh.sharedChildWeightList.ToJobArray();
			jobData.virtualPosList = base.Manager.Mesh.virtualPosList.ToJobArray();
			jobData.virtualRotList = base.Manager.Mesh.virtualRotList.ToJobArray();
			jobData.renderVertexFlagList = base.Manager.Mesh.renderVertexFlagList.ToJobArray();
			jobData.renderPosList = base.Manager.Mesh.renderPosList.ToJobArray();
			jobData.renderNormalList = base.Manager.Mesh.renderNormalList.ToJobArray();
			jobData.renderTangentList = base.Manager.Mesh.renderTangentList.ToJobArray();
			jobHandle = jobData.Schedule(base.Manager.Mesh.renderMeshInfoList.Length, 1, jobHandle);
			return jobHandle;
		}
	}
}
