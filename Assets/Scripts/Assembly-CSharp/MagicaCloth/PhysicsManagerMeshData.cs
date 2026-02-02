#define RELEASE
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public class PhysicsManagerMeshData : PhysicsManagerAccess
	{
		public struct SharedVirtualMeshInfo
		{
			public int uid;

			public int useCount;

			public int sharedChildMeshStartIndex;

			public int sharedChildMeshCount;

			public ChunkData vertexChunk;

			public ChunkData weightChunk;

			public ChunkData triangleChunk;

			public ChunkData vertexToTriangleChunk;
		}

		public struct VirtualMeshInfo
		{
			public uint flag;

			public int sharedVirtualMeshIndex;

			public int meshUseCount;

			public int vertexUseCount;

			public ChunkData vertexChunk;

			public ChunkData boneChunk;

			public ChunkData triangleChunk;

			public int transformIndex;

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

			public bool IsActive()
			{
				return IsFlag(1u);
			}

			public bool IsUse()
			{
				if (IsFlag(1u) && meshUseCount > 0)
				{
					return vertexUseCount > 0;
				}
				return false;
			}
		}

		public struct SharedChildMeshInfo
		{
			public long cuid;

			public int sharedVirtualMeshIndex;

			public int virtualMeshIndex;

			public int meshUseCount;

			public ChunkData vertexChunk;

			public ChunkData weightChunk;
		}

		public struct SharedRenderMeshInfo
		{
			public int uid;

			public int useCount;

			public uint flag;

			public ChunkData vertexChunk;

			public ChunkData bonePerVertexChunk;

			public ChunkData boneWeightsChunk;

			public int rendererBoneIndex;

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

			public bool IsSkinning()
			{
				return IsFlag(4u);
			}
		}

		public class SharedRenderMeshBuffer
		{
			public Vector3[] vertices;

			public Vector3[] normals;

			public Vector4[] tangents;

			public BoneWeight[] boneWeights;
		}

		public struct RenderMeshInfo
		{
			public uint flag;

			public int renderSharedMeshIndex;

			public int sharedRenderMeshVertexStartIndex;

			public int meshUseCount;

			public int4 childMeshVertexStartIndex;

			public int4 childMeshWeightStartIndex;

			public int4 virtualMeshVertexStartIndex;

			public int4 sharedVirtualMeshVertexStartIndex;

			public int4 linkMeshCount;

			public ChunkData vertexChunk;

			public ChunkData boneWeightsChunk;

			public int transformIndex;

			public float baseScale;

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

			public bool IsActive()
			{
				return IsFlag(1u);
			}

			public bool InUse()
			{
				if (IsFlag(1u))
				{
					return meshUseCount > 0;
				}
				return false;
			}

			public bool IsLinkMesh(int index)
			{
				return (flag & (uint)(268435456 << index)) != 0;
			}

			public bool AddLinkMesh(int renderMeshIndex, int childMeshVertexStart, int childMeshWeightStart, int virtualMeshVertexStart, int sharedVirtualMeshVertexStart)
			{
				for (int i = 0; i < 4; i++)
				{
					if (IsLinkMesh(i) && childMeshVertexStartIndex[i] == childMeshVertexStart && virtualMeshVertexStartIndex[i] == virtualMeshVertexStart)
					{
						linkMeshCount[i]++;
						SetFlag((uint)(268435456 << i), true);
						return true;
					}
				}
				for (int j = 0; j < 4; j++)
				{
					if (!IsLinkMesh(j))
					{
						childMeshVertexStartIndex[j] = childMeshVertexStart;
						childMeshWeightStartIndex[j] = childMeshWeightStart;
						virtualMeshVertexStartIndex[j] = virtualMeshVertexStart;
						sharedVirtualMeshVertexStartIndex[j] = sharedVirtualMeshVertexStart;
						linkMeshCount[j] = 1;
						SetFlag((uint)(268435456 << j), true);
						return true;
					}
				}
				return false;
			}

			public bool RemoveLinkMesh(int renderMeshIndex, int childMeshVertexStart, int childMeshWeightStart, int virtualMeshVertexStart, int sharedVirtualMeshVertexStart)
			{
				for (int i = 0; i < 4; i++)
				{
					if (IsLinkMesh(i) && childMeshVertexStartIndex[i] == childMeshVertexStart && virtualMeshVertexStartIndex[i] == virtualMeshVertexStart)
					{
						linkMeshCount[i]--;
						if (linkMeshCount[i] == 0)
						{
							childMeshVertexStartIndex[i] = 0;
							childMeshWeightStartIndex[i] = 0;
							virtualMeshVertexStartIndex[i] = 0;
							sharedVirtualMeshVertexStartIndex[i] = 0;
							SetFlag((uint)(268435456 << i), false);
						}
						return true;
					}
				}
				return false;
			}
		}

		public class RenderMeshState
		{
			public uint flag;

			public int RenderSharedMeshIndex;

			public int RenderSharedMeshId;

			public int VertexChunkStart;

			public int BoneWeightChunkStart;

			public int BoneWeightChunkLength;

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
		}

		private HashSet<BaseMeshDeformer> meshSet = new HashSet<BaseMeshDeformer>();

		public const uint MeshFlag_Active = 1u;

		public const uint MeshFlag_Skinning = 4u;

		public const uint Meshflag_CalcNormal = 8u;

		public const uint Meshflag_CalcTangent = 16u;

		public const uint MeshFlag_ExistNormals = 65536u;

		public const uint MeshFlag_ExistTangents = 131072u;

		public const uint MeshFlag_ExistWeights = 262144u;

		public const uint MeshFlag_UpdateUseVertexFront = 16777216u;

		public const uint MeshFlag_UpdateUseVertexBack = 33554432u;

		public const uint MeshFlag_MeshLink = 268435456u;

		public FixedNativeList<SharedVirtualMeshInfo> sharedVirtualMeshInfoList;

		public Dictionary<int, int> sharedVirtualMeshIdToIndexDict = new Dictionary<int, int>();

		public FixedChunkNativeArray<float2> sharedVirtualUvList;

		public FixedChunkNativeArray<uint> sharedVirtualVertexInfoList;

		public FixedChunkNativeArray<MeshData.VertexWeight> sharedVirtualWeightList;

		public FixedChunkNativeArray<int> sharedVirtualTriangleList;

		public FixedChunkNativeArray<uint> sharedVirtualVertexToTriangleInfoList;

		public FixedChunkNativeArray<int> sharedVirtualVertexToTriangleIndexList;

		public const byte VirtualVertexFlag_Use = 1;

		public FixedNativeList<VirtualMeshInfo> virtualMeshInfoList;

		public FixedChunkNativeArray<short> virtualVertexMeshIndexList;

		public FixedChunkNativeArray<byte> virtualVertexUseList;

		public FixedChunkNativeArray<byte> virtualVertexFixList;

		public FixedChunkNativeArray<byte> virtualVertexFlagList;

		public FixedChunkNativeArray<float3> virtualPosList;

		public FixedChunkNativeArray<quaternion> virtualRotList;

		public FixedChunkNativeArray<int> virtualTransformIndexList;

		public FixedChunkNativeArray<float3> virtualTriangleNormalList;

		public FixedChunkNativeArray<float3> virtualTriangleTangentList;

		public FixedChunkNativeArray<ushort> virtualTriangleMeshIndexList;

		public FixedNativeList<SharedChildMeshInfo> sharedChildMeshInfoList;

		public Dictionary<long, int> sharedChildMeshIdToSharedVirtualMeshIndexDict = new Dictionary<long, int>();

		public FixedChunkNativeArray<uint> sharedChildVertexInfoList;

		public FixedChunkNativeArray<MeshData.VertexWeight> sharedChildWeightList;

		public FixedNativeList<SharedRenderMeshInfo> sharedRenderMeshInfoList;

		public Dictionary<int, int> sharedRenderMeshIdToIndexDict = new Dictionary<int, int>();

		public FixedChunkNativeArray<float3> sharedRenderVertices;

		public FixedChunkNativeArray<float3> sharedRenderNormals;

		public FixedChunkNativeArray<float4> sharedRenderTangents;

		public FixedChunkNativeArray<BoneWeight> sharedBoneWeightList;

		public Dictionary<int, SharedRenderMeshBuffer> sharedRenderMeshIdToBufferDict = new Dictionary<int, SharedRenderMeshBuffer>();

		public const uint RenderVertexFlag_Use = 65536u;

		public const int MaxRenderMeshLinkCount = 4;

		public const uint RenderStateFlag_Use = 1u;

		public const uint RenderStateFlag_ExistNormal = 2u;

		public const uint RenderStateFlag_ExistTangent = 4u;

		public const uint RenderStateFlag_DelayedCalculated = 256u;

		public FixedNativeList<RenderMeshInfo> renderMeshInfoList;

		public Dictionary<int, RenderMeshState> renderMeshStateDict = new Dictionary<int, RenderMeshState>();

		public FixedChunkNativeArray<uint> renderVertexFlagList;

		public FixedChunkNativeArray<float3> renderPosList;

		public FixedChunkNativeArray<float3> renderNormalList;

		public FixedChunkNativeArray<float4> renderTangentList;

		public FixedChunkNativeArray<BoneWeight> renderBoneWeightList;

		public int SharedVirtualMeshCount
		{
			get
			{
				return sharedVirtualMeshInfoList.Count;
			}
		}

		public int VirtualMeshCount
		{
			get
			{
				return virtualMeshInfoList.Count;
			}
		}

		public int VirtualMeshVertexCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < virtualMeshInfoList.Length; i++)
				{
					num += virtualMeshInfoList[i].vertexChunk.dataLength;
				}
				return num;
			}
		}

		public int VirtualMeshTriangleCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < virtualMeshInfoList.Length; i++)
				{
					num += virtualMeshInfoList[i].triangleChunk.dataLength;
				}
				return num;
			}
		}

		public int VirtualMeshVertexUseCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < virtualMeshInfoList.Length; i++)
				{
					if (virtualMeshInfoList[i].IsActive())
					{
						num += virtualMeshInfoList[i].vertexChunk.dataLength;
					}
				}
				return num;
			}
		}

		public int VirtualMeshUseCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < virtualMeshInfoList.Length; i++)
				{
					num += (virtualMeshInfoList[i].IsUse() ? 1 : 0);
				}
				return num;
			}
		}

		public int SharedRenderMeshCount
		{
			get
			{
				return sharedRenderMeshInfoList.Count;
			}
		}

		public int SharedChildMeshCount
		{
			get
			{
				return sharedChildMeshInfoList.Count;
			}
		}

		public int RenderMeshCount
		{
			get
			{
				return renderMeshInfoList.Count;
			}
		}

		public int RenderMeshVertexCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < renderMeshInfoList.Length; i++)
				{
					num += renderMeshInfoList[i].vertexChunk.dataLength;
				}
				return num;
			}
		}

		public int RenderMeshUseCount
		{
			get
			{
				int num = 0;
				foreach (RenderMeshState value in renderMeshStateDict.Values)
				{
					num += (value.IsFlag(1u) ? 1 : 0);
				}
				return num;
			}
		}

		public int RenderMeshVertexUseCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < renderMeshInfoList.Length; i++)
				{
					if (renderMeshInfoList[i].IsActive())
					{
						num += renderMeshInfoList[i].vertexChunk.dataLength;
					}
				}
				return num;
			}
		}

		public override void Create()
		{
			sharedVirtualMeshInfoList = new FixedNativeList<SharedVirtualMeshInfo>();
			sharedVirtualVertexInfoList = new FixedChunkNativeArray<uint>();
			sharedVirtualWeightList = new FixedChunkNativeArray<MeshData.VertexWeight>();
			sharedVirtualUvList = new FixedChunkNativeArray<float2>();
			sharedVirtualTriangleList = new FixedChunkNativeArray<int>();
			sharedVirtualVertexToTriangleInfoList = new FixedChunkNativeArray<uint>();
			sharedVirtualVertexToTriangleIndexList = new FixedChunkNativeArray<int>();
			virtualMeshInfoList = new FixedNativeList<VirtualMeshInfo>();
			virtualVertexMeshIndexList = new FixedChunkNativeArray<short>();
			virtualVertexUseList = new FixedChunkNativeArray<byte>();
			virtualVertexFixList = new FixedChunkNativeArray<byte>();
			virtualVertexFlagList = new FixedChunkNativeArray<byte>();
			virtualPosList = new FixedChunkNativeArray<float3>();
			virtualRotList = new FixedChunkNativeArray<quaternion>();
			virtualTransformIndexList = new FixedChunkNativeArray<int>();
			virtualTriangleNormalList = new FixedChunkNativeArray<float3>();
			virtualTriangleTangentList = new FixedChunkNativeArray<float3>();
			virtualTriangleMeshIndexList = new FixedChunkNativeArray<ushort>();
			sharedChildMeshInfoList = new FixedNativeList<SharedChildMeshInfo>();
			sharedChildVertexInfoList = new FixedChunkNativeArray<uint>();
			sharedChildWeightList = new FixedChunkNativeArray<MeshData.VertexWeight>();
			sharedRenderMeshInfoList = new FixedNativeList<SharedRenderMeshInfo>();
			sharedRenderVertices = new FixedChunkNativeArray<float3>();
			sharedRenderNormals = new FixedChunkNativeArray<float3>();
			sharedRenderTangents = new FixedChunkNativeArray<float4>();
			sharedBoneWeightList = new FixedChunkNativeArray<BoneWeight>();
			renderMeshInfoList = new FixedNativeList<RenderMeshInfo>();
			renderVertexFlagList = new FixedChunkNativeArray<uint>();
			renderPosList = new FixedChunkNativeArray<float3>();
			renderNormalList = new FixedChunkNativeArray<float3>();
			renderTangentList = new FixedChunkNativeArray<float4>();
			renderBoneWeightList = new FixedChunkNativeArray<BoneWeight>();
		}

		public override void Dispose()
		{
			if (sharedVirtualMeshInfoList != null)
			{
				sharedVirtualMeshInfoList.Dispose();
				sharedVirtualVertexInfoList.Dispose();
				sharedVirtualWeightList.Dispose();
				sharedVirtualUvList.Dispose();
				sharedVirtualTriangleList.Dispose();
				sharedVirtualVertexToTriangleInfoList.Dispose();
				sharedVirtualVertexToTriangleIndexList.Dispose();
				virtualMeshInfoList.Dispose();
				virtualVertexMeshIndexList.Dispose();
				virtualVertexUseList.Dispose();
				virtualVertexFixList.Dispose();
				virtualVertexFlagList.Dispose();
				virtualPosList.Dispose();
				virtualRotList.Dispose();
				virtualTransformIndexList.Dispose();
				virtualTriangleNormalList.Dispose();
				virtualTriangleTangentList.Dispose();
				virtualTriangleMeshIndexList.Dispose();
				sharedChildMeshInfoList.Dispose();
				sharedChildVertexInfoList.Dispose();
				sharedChildWeightList.Dispose();
				sharedRenderMeshInfoList.Dispose();
				sharedRenderVertices.Dispose();
				sharedRenderNormals.Dispose();
				sharedRenderTangents.Dispose();
				sharedBoneWeightList.Dispose();
				renderMeshInfoList.Dispose();
				renderVertexFlagList.Dispose();
				renderPosList.Dispose();
				renderNormalList.Dispose();
				renderTangentList.Dispose();
				renderBoneWeightList.Dispose();
			}
		}

		public void AddMesh(BaseMeshDeformer bmesh)
		{
			meshSet.Add(bmesh);
		}

		public void RemoveMesh(BaseMeshDeformer bmesh)
		{
			if (meshSet.Contains(bmesh))
			{
				meshSet.Remove(bmesh);
			}
		}

		public bool ContainsMesh(BaseMeshDeformer bmesh)
		{
			return meshSet.Contains(bmesh);
		}

		public int AddVirtualMesh(int uid, int vertexCount, int weightCount, int boneCount, int triangleCount, int vertexToTriangleIndexCount, Transform transform)
		{
			int num = -1;
			if (uid != 0)
			{
				if (sharedVirtualMeshIdToIndexDict.ContainsKey(uid))
				{
					num = sharedVirtualMeshIdToIndexDict[uid];
					SharedVirtualMeshInfo value = sharedVirtualMeshInfoList[num];
					value.useCount++;
					sharedVirtualMeshInfoList[num] = value;
				}
				else
				{
					SharedVirtualMeshInfo element = default(SharedVirtualMeshInfo);
					element.uid = uid;
					element.useCount = 1;
					ChunkData vertexChunk = sharedVirtualVertexInfoList.AddChunk(vertexCount);
					sharedVirtualUvList.AddChunk(vertexCount);
					sharedVirtualVertexToTriangleInfoList.AddChunk(vertexCount);
					element.vertexChunk = vertexChunk;
					vertexChunk = sharedVirtualWeightList.AddChunk(weightCount);
					element.weightChunk = vertexChunk;
					if (triangleCount > 0)
					{
						vertexChunk = sharedVirtualTriangleList.AddChunk(triangleCount * 3);
						element.triangleChunk = vertexChunk;
					}
					if (vertexToTriangleIndexCount > 0)
					{
						vertexChunk = sharedVirtualVertexToTriangleIndexList.AddChunk(vertexToTriangleIndexCount);
						element.vertexToTriangleChunk = vertexChunk;
					}
					num = sharedVirtualMeshInfoList.Add(element);
					sharedVirtualMeshIdToIndexDict.Add(uid, num);
				}
			}
			VirtualMeshInfo element2 = default(VirtualMeshInfo);
			element2.sharedVirtualMeshIndex = num;
			ChunkData vertexChunk2 = virtualVertexUseList.AddChunk(vertexCount);
			virtualVertexMeshIndexList.AddChunk(vertexCount);
			virtualVertexFixList.AddChunk(vertexCount);
			virtualVertexFlagList.AddChunk(vertexCount);
			virtualPosList.AddChunk(vertexCount);
			virtualRotList.AddChunk(vertexCount);
			element2.vertexChunk = vertexChunk2;
			Debug.Assert(boneCount > 0);
			vertexChunk2 = virtualTransformIndexList.AddChunk(boneCount);
			element2.boneChunk = vertexChunk2;
			if (triangleCount > 0)
			{
				vertexChunk2 = virtualTriangleNormalList.AddChunk(triangleCount);
				virtualTriangleTangentList.AddChunk(triangleCount);
				virtualTriangleMeshIndexList.AddChunk(triangleCount);
				element2.triangleChunk = vertexChunk2;
			}
			element2.transformIndex = base.Bone.AddBone(transform);
			int num2 = virtualMeshInfoList.Add(element2);
			virtualVertexMeshIndexList.Fill(element2.vertexChunk, (short)(num2 + 1));
			if (triangleCount > 0)
			{
				virtualTriangleMeshIndexList.Fill(element2.triangleChunk, (ushort)(num2 + 1));
			}
			return num2;
		}

		public bool IsEmptySharedVirtualMesh(int uid)
		{
			return !sharedVirtualMeshIdToIndexDict.ContainsKey(uid);
		}

		public void SetSharedVirtualMeshData(int virtualMeshIndex, uint[] sharedVertexInfoList, MeshData.VertexWeight[] sharedWeightList, Vector2[] sharedUv, int[] sharedTriangles, uint[] vertexToTriangleInfoList, int[] vertexToTriangleIndexList)
		{
			VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[virtualMeshIndex];
			Debug.Assert(virtualMeshInfo.sharedVirtualMeshIndex >= 0);
			SharedVirtualMeshInfo sharedVirtualMeshInfo = sharedVirtualMeshInfoList[virtualMeshInfo.sharedVirtualMeshIndex];
			if (sharedVirtualMeshInfo.useCount == 1)
			{
				sharedVirtualVertexInfoList.ToJobArray().CopyFromFast(sharedVirtualMeshInfo.vertexChunk.startIndex, sharedVertexInfoList);
				sharedVirtualWeightList.ToJobArray().CopyFromFast(sharedVirtualMeshInfo.weightChunk.startIndex, sharedWeightList);
				if (sharedUv != null && sharedUv.Length != 0)
				{
					sharedVirtualUvList.ToJobArray().CopyFromFast(sharedVirtualMeshInfo.vertexChunk.startIndex, sharedUv);
				}
				if (vertexToTriangleInfoList != null && vertexToTriangleInfoList.Length != 0)
				{
					sharedVirtualVertexToTriangleInfoList.ToJobArray().CopyFromFast(sharedVirtualMeshInfo.vertexChunk.startIndex, vertexToTriangleInfoList);
				}
				if (vertexToTriangleIndexList != null && vertexToTriangleIndexList.Length != 0)
				{
					sharedVirtualVertexToTriangleIndexList.ToJobArray().CopyFromFast(sharedVirtualMeshInfo.vertexToTriangleChunk.startIndex, vertexToTriangleIndexList);
				}
				if (sharedTriangles != null && sharedTriangles.Length != 0)
				{
					sharedVirtualTriangleList.ToJobArray().CopyFromFast(sharedVirtualMeshInfo.triangleChunk.startIndex, sharedTriangles);
				}
			}
		}

		public void RemoveVirtualMesh(int virtualMeshIndex)
		{
			if (virtualMeshIndex < 0 || !virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				return;
			}
			VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[virtualMeshIndex];
			int sharedVirtualMeshIndex = virtualMeshInfo.sharedVirtualMeshIndex;
			if (sharedVirtualMeshIndex >= 0)
			{
				SharedVirtualMeshInfo value = sharedVirtualMeshInfoList[sharedVirtualMeshIndex];
				value.useCount--;
				if (value.useCount == 0)
				{
					sharedVirtualVertexInfoList.RemoveChunk(value.vertexChunk.chunkNo);
					sharedVirtualWeightList.RemoveChunk(value.weightChunk.chunkNo);
					sharedVirtualUvList.RemoveChunk(value.vertexChunk.chunkNo);
					sharedVirtualVertexToTriangleInfoList.RemoveChunk(value.vertexChunk.chunkNo);
					if (value.triangleChunk.dataLength > 0)
					{
						sharedVirtualTriangleList.RemoveChunk(value.triangleChunk.chunkNo);
					}
					if (value.vertexToTriangleChunk.dataLength > 0)
					{
						sharedVirtualVertexToTriangleIndexList.RemoveChunk(value.vertexToTriangleChunk.chunkNo);
					}
					sharedVirtualMeshInfoList.Remove(sharedVirtualMeshIndex);
					sharedVirtualMeshIdToIndexDict.Remove(value.uid);
				}
				else
				{
					sharedVirtualMeshInfoList[sharedVirtualMeshIndex] = value;
				}
			}
			virtualVertexMeshIndexList.RemoveChunk(virtualMeshInfo.vertexChunk.chunkNo);
			virtualVertexUseList.RemoveChunk(virtualMeshInfo.vertexChunk.chunkNo);
			virtualVertexFixList.RemoveChunk(virtualMeshInfo.vertexChunk.chunkNo);
			virtualVertexFlagList.RemoveChunk(virtualMeshInfo.vertexChunk.chunkNo);
			virtualPosList.RemoveChunk(virtualMeshInfo.vertexChunk.chunkNo);
			virtualRotList.RemoveChunk(virtualMeshInfo.vertexChunk.chunkNo);
			virtualTransformIndexList.RemoveChunk(virtualMeshInfo.boneChunk.chunkNo);
			if (virtualMeshInfo.triangleChunk.dataLength > 0)
			{
				virtualTriangleNormalList.RemoveChunk(virtualMeshInfo.triangleChunk.chunkNo);
				virtualTriangleTangentList.RemoveChunk(virtualMeshInfo.triangleChunk.chunkNo);
				virtualTriangleMeshIndexList.RemoveChunk(virtualMeshInfo.triangleChunk.chunkNo);
			}
			base.Bone.RemoveBone(virtualMeshInfo.transformIndex);
			virtualMeshInfo.transformIndex = 0;
			virtualMeshInfoList.Remove(virtualMeshIndex);
		}

		public bool ExistsVirtualMesh(int virtualMeshIndex)
		{
			return virtualMeshInfoList.Exists(virtualMeshIndex);
		}

		public VirtualMeshInfo GetVirtualMeshInfo(int virtualMeshIndex)
		{
			return virtualMeshInfoList[virtualMeshIndex];
		}

		public bool IsUseVirtualMesh(int virtualMeshIndex)
		{
			return virtualMeshInfoList[virtualMeshIndex].IsUse();
		}

		public bool IsActiveVirtualMesh(int virtualMeshIndex)
		{
			return virtualMeshInfoList[virtualMeshIndex].IsActive();
		}

		public void SetVirtualMeshActive(int virtualMeshIndex, bool sw)
		{
			if (virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				VirtualMeshInfo value = virtualMeshInfoList[virtualMeshIndex];
				value.SetFlag(1u, sw);
				virtualMeshInfoList[virtualMeshIndex] = value;
			}
		}

		public void AddUseVirtualMesh(int virtualMeshIndex)
		{
			if (virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				VirtualMeshInfo value = virtualMeshInfoList[virtualMeshIndex];
				value.meshUseCount++;
				virtualMeshInfoList[virtualMeshIndex] = value;
			}
		}

		public void RemoveUseVirtualMesh(int virtualMeshIndex)
		{
			if (virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				VirtualMeshInfo value = virtualMeshInfoList[virtualMeshIndex];
				value.meshUseCount--;
				Debug.Assert(value.meshUseCount >= 0);
				virtualMeshInfoList[virtualMeshIndex] = value;
			}
		}

		public bool AddUseVirtualVertex(int virtualMeshIndex, int vindex, bool fix)
		{
			if (virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				VirtualMeshInfo value = virtualMeshInfoList[virtualMeshIndex];
				value.vertexUseCount++;
				virtualMeshInfoList[virtualMeshIndex] = value;
				int index = value.vertexChunk.startIndex + vindex;
				byte b = (byte)(virtualVertexUseList[index] + 1);
				virtualVertexUseList[index] = b;
				if (fix)
				{
					virtualVertexFixList[index]++;
				}
				return b == 1;
			}
			return false;
		}

		public bool RemoveUseVirtualVertex(int virtualMeshIndex, int vindex, bool fix)
		{
			if (virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				VirtualMeshInfo value = virtualMeshInfoList[virtualMeshIndex];
				value.vertexUseCount--;
				virtualMeshInfoList[virtualMeshIndex] = value;
				int index = value.vertexChunk.startIndex + vindex;
				byte b = (byte)(virtualVertexUseList[index] - 1);
				virtualVertexUseList[index] = b;
				if (fix)
				{
					virtualVertexFixList[index]--;
				}
				return b == 0;
			}
			return false;
		}

		public void CopyToVirtualMeshWorldData(int virtualMeshIndex, Vector3[] vertices, Vector3[] normals, Vector3[] tangents)
		{
			VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[virtualMeshIndex];
			int startIndex = virtualMeshInfo.vertexChunk.startIndex;
			virtualPosList.ToJobArray().CopyToFast(startIndex, vertices);
			float3 v = new float3(0f, 0f, 1f);
			float3 v2 = new float3(0f, 1f, 0f);
			for (int i = 0; i < virtualMeshInfo.vertexChunk.dataLength; i++)
			{
				quaternion q = virtualRotList[startIndex + i];
				normals[i] = math.mul(q, v);
				tangents[i] = math.mul(q, v2);
			}
		}

		public int AddSharedChildMesh(long cuid, int virtualMeshIndex, int vertexCount, int weightCount)
		{
			int sharedVirtualMeshIndex = virtualMeshInfoList[virtualMeshIndex].sharedVirtualMeshIndex;
			int num = -1;
			if (sharedChildMeshIdToSharedVirtualMeshIndexDict.ContainsKey(cuid))
			{
				num = sharedChildMeshIdToSharedVirtualMeshIndexDict[cuid];
				SharedChildMeshInfo value = sharedChildMeshInfoList[num];
				value.meshUseCount++;
				sharedChildMeshInfoList[num] = value;
			}
			else
			{
				SharedChildMeshInfo element = default(SharedChildMeshInfo);
				element.cuid = cuid;
				element.sharedVirtualMeshIndex = sharedVirtualMeshIndex;
				element.virtualMeshIndex = virtualMeshIndex;
				element.meshUseCount = 1;
				ChunkData vertexChunk = sharedChildVertexInfoList.AddChunk(vertexCount);
				element.vertexChunk = vertexChunk;
				vertexChunk = sharedChildWeightList.AddChunk(weightCount);
				element.weightChunk = vertexChunk;
				num = sharedChildMeshInfoList.Add(element);
				sharedChildMeshIdToSharedVirtualMeshIndexDict.Add(cuid, num);
			}
			return num;
		}

		public bool IsEmptySharedChildMesh(long cuid)
		{
			return !sharedChildMeshIdToSharedVirtualMeshIndexDict.ContainsKey(cuid);
		}

		public void SetSharedChildMeshData(int sharedMeshIndex, uint[] sharedVertexInfoList, MeshData.VertexWeight[] sharedVertexWeightList)
		{
			SharedChildMeshInfo sharedChildMeshInfo = sharedChildMeshInfoList[sharedMeshIndex];
			if (sharedChildMeshInfo.meshUseCount == 1)
			{
				sharedChildVertexInfoList.ToJobArray().CopyFromFast(sharedChildMeshInfo.vertexChunk.startIndex, sharedVertexInfoList);
				sharedChildWeightList.ToJobArray().CopyFromFast(sharedChildMeshInfo.weightChunk.startIndex, sharedVertexWeightList);
			}
		}

		public void RemoveSharedChildMesh(int sharedChildMeshIndex)
		{
			SharedChildMeshInfo value = sharedChildMeshInfoList[sharedChildMeshIndex];
			value.meshUseCount--;
			if (value.meshUseCount == 0)
			{
				sharedChildVertexInfoList.RemoveChunk(value.vertexChunk.chunkNo);
				sharedChildWeightList.RemoveChunk(value.weightChunk.chunkNo);
				sharedChildMeshInfoList.Remove(sharedChildMeshIndex);
				sharedChildMeshIdToSharedVirtualMeshIndexDict.Remove(value.cuid);
			}
			else
			{
				sharedChildMeshInfoList[sharedChildMeshIndex] = value;
			}
		}

		public int AddRenderMesh(int uid, bool isSkinning, Vector3 baseScale, int vertexCount, int rendererBoneIndex, int boneWeightCount)
		{
			int num = -1;
			if (uid != 0)
			{
				if (sharedRenderMeshIdToIndexDict.ContainsKey(uid))
				{
					num = sharedRenderMeshIdToIndexDict[uid];
					SharedRenderMeshInfo value = sharedRenderMeshInfoList[num];
					value.useCount++;
					sharedRenderMeshInfoList[num] = value;
				}
				else
				{
					SharedRenderMeshInfo element = default(SharedRenderMeshInfo);
					element.uid = uid;
					element.useCount = 1;
					element.rendererBoneIndex = rendererBoneIndex;
					if (isSkinning)
					{
						element.SetFlag(4u, true);
					}
					ChunkData vertexChunk = sharedRenderVertices.AddChunk(vertexCount);
					sharedRenderNormals.AddChunk(vertexCount);
					sharedRenderTangents.AddChunk(vertexCount);
					element.vertexChunk = vertexChunk;
					if (isSkinning)
					{
						ChunkData bonePerVertexChunk = default(ChunkData);
						ChunkData boneWeightsChunk = sharedBoneWeightList.AddChunk(boneWeightCount);
						element.bonePerVertexChunk = bonePerVertexChunk;
						element.boneWeightsChunk = boneWeightsChunk;
					}
					num = sharedRenderMeshInfoList.Add(element);
					sharedRenderMeshIdToIndexDict.Add(uid, num);
					SharedRenderMeshBuffer sharedRenderMeshBuffer = new SharedRenderMeshBuffer();
					sharedRenderMeshBuffer.vertices = new Vector3[vertexCount];
					sharedRenderMeshBuffer.normals = new Vector3[vertexCount];
					sharedRenderMeshBuffer.tangents = new Vector4[vertexCount];
					if (isSkinning)
					{
						sharedRenderMeshBuffer.boneWeights = new BoneWeight[vertexCount];
					}
					sharedRenderMeshIdToBufferDict.Add(uid, sharedRenderMeshBuffer);
				}
			}
			RenderMeshInfo element2 = default(RenderMeshInfo);
			element2.renderSharedMeshIndex = num;
			SharedRenderMeshInfo sharedRenderMeshInfo = sharedRenderMeshInfoList[num];
			element2.sharedRenderMeshVertexStartIndex = sharedRenderMeshInfo.vertexChunk.startIndex;
			ChunkData vertexChunk2 = renderVertexFlagList.AddChunk(vertexCount);
			renderPosList.AddChunk(vertexCount);
			renderNormalList.AddChunk(vertexCount);
			renderTangentList.AddChunk(vertexCount);
			if (isSkinning)
			{
				element2.boneWeightsChunk = renderBoneWeightList.AddChunk(boneWeightCount);
			}
			element2.vertexChunk = vertexChunk2;
			element2.baseScale = baseScale.magnitude;
			int num2 = renderMeshInfoList.Add(element2);
			RenderMeshState renderMeshState = new RenderMeshState();
			renderMeshState.SetFlag(1u, element2.InUse());
			renderMeshState.RenderSharedMeshIndex = num;
			renderMeshState.RenderSharedMeshId = sharedRenderMeshInfo.uid;
			renderMeshState.VertexChunkStart = vertexChunk2.startIndex;
			renderMeshState.BoneWeightChunkStart = element2.boneWeightsChunk.startIndex;
			renderMeshState.BoneWeightChunkLength = element2.boneWeightsChunk.dataLength;
			renderMeshStateDict.Add(num2, renderMeshState);
			uint data = (uint)(num2 + 1);
			renderVertexFlagList.Fill(element2.vertexChunk, data);
			return num2;
		}

		public void UpdateMeshState(int renderMeshIndex)
		{
			RenderMeshState renderMeshState = renderMeshStateDict[renderMeshIndex];
			SharedRenderMeshInfo sharedRenderMeshInfo = sharedRenderMeshInfoList[renderMeshState.RenderSharedMeshIndex];
			renderMeshState.SetFlag(2u, sharedRenderMeshInfo.IsFlag(65536u));
			renderMeshState.SetFlag(4u, sharedRenderMeshInfo.IsFlag(131072u));
		}

		public void AddRenderMeshBone(int renderMeshIndex, Transform rendererTransform)
		{
			RenderMeshInfo value = renderMeshInfoList[renderMeshIndex];
			value.transformIndex = base.Bone.AddBone(rendererTransform);
			renderMeshInfoList[renderMeshIndex] = value;
		}

		public bool IsEmptySharedRenderMesh(int uid)
		{
			return !sharedRenderMeshIdToIndexDict.ContainsKey(uid);
		}

		public void SetRenderSharedMeshData(int renderMeshIndex, bool isSkinning, Vector3[] sharedVertices, Vector3[] sharedNormals, Vector4[] sharedTangents, BoneWeight[] sharedBoneWeights)
		{
			RenderMeshInfo renderMeshInfo = renderMeshInfoList[renderMeshIndex];
			Debug.Assert(renderMeshInfo.renderSharedMeshIndex >= 0);
			SharedRenderMeshInfo value = sharedRenderMeshInfoList[renderMeshInfo.renderSharedMeshIndex];
			if (value.useCount == 1)
			{
				sharedRenderVertices.ToJobArray().CopyFromFast(value.vertexChunk.startIndex, sharedVertices);
				if (sharedNormals != null && sharedNormals.Length != 0)
				{
					sharedRenderNormals.ToJobArray().CopyFromFast(value.vertexChunk.startIndex, sharedNormals);
					value.SetFlag(65536u, true);
				}
				if (sharedTangents != null && sharedTangents.Length != 0)
				{
					sharedRenderTangents.ToJobArray().CopyFromFast(value.vertexChunk.startIndex, sharedTangents);
					value.SetFlag(131072u, true);
				}
				if (isSkinning && sharedBoneWeights != null && sharedBoneWeights.Length != 0)
				{
					sharedBoneWeightList.ToJobArray().CopyFromFast(value.boneWeightsChunk.startIndex, sharedBoneWeights);
				}
				sharedRenderMeshInfoList[renderMeshInfo.renderSharedMeshIndex] = value;
			}
		}

		public void RemoveRenderMesh(int renderMeshIndex)
		{
			if (renderMeshIndex < 0 || !renderMeshInfoList.Exists(renderMeshIndex))
			{
				return;
			}
			RenderMeshInfo renderMeshInfo = renderMeshInfoList[renderMeshIndex];
			int renderSharedMeshIndex = renderMeshInfo.renderSharedMeshIndex;
			if (renderSharedMeshIndex >= 0)
			{
				SharedRenderMeshInfo value = sharedRenderMeshInfoList[renderSharedMeshIndex];
				value.useCount--;
				if (value.useCount == 0)
				{
					sharedRenderVertices.RemoveChunk(value.vertexChunk.chunkNo);
					sharedRenderNormals.RemoveChunk(value.vertexChunk.chunkNo);
					sharedRenderTangents.RemoveChunk(value.vertexChunk.chunkNo);
					if (value.boneWeightsChunk.dataLength > 0)
					{
						sharedBoneWeightList.RemoveChunk(value.boneWeightsChunk);
					}
					sharedRenderMeshInfoList.Remove(renderSharedMeshIndex);
					sharedRenderMeshIdToIndexDict.Remove(value.uid);
					sharedRenderMeshIdToBufferDict.Remove(value.uid);
				}
				else
				{
					sharedRenderMeshInfoList[renderSharedMeshIndex] = value;
				}
			}
			renderVertexFlagList.RemoveChunk(renderMeshInfo.vertexChunk.chunkNo);
			renderPosList.RemoveChunk(renderMeshInfo.vertexChunk.chunkNo);
			renderNormalList.RemoveChunk(renderMeshInfo.vertexChunk.chunkNo);
			renderTangentList.RemoveChunk(renderMeshInfo.vertexChunk.chunkNo);
			if (renderMeshInfo.boneWeightsChunk.dataLength > 0)
			{
				renderBoneWeightList.RemoveChunk(renderMeshInfo.boneWeightsChunk);
			}
			renderMeshStateDict.Remove(renderMeshIndex);
			renderMeshInfoList.Remove(renderMeshIndex);
		}

		public void RemoveRenderMeshBone(int renderMeshIndex)
		{
			RenderMeshInfo value = renderMeshInfoList[renderMeshIndex];
			base.Bone.RemoveBone(value.transformIndex);
			value.transformIndex = 0;
			renderMeshInfoList[renderMeshIndex] = value;
		}

		public bool IsUseRenderMesh(int renderMeshIndex)
		{
			return renderMeshStateDict[renderMeshIndex].IsFlag(1u);
		}

		public bool IsActiveRenderMesh(int renderMeshIndex)
		{
			return renderMeshInfoList[renderMeshIndex].IsActive();
		}

		public void SetRenderMeshFlag(int renderMeshIndex, uint flag, bool sw)
		{
			if (renderMeshInfoList.Exists(renderMeshIndex))
			{
				RenderMeshInfo value = renderMeshInfoList[renderMeshIndex];
				value.SetFlag(flag, sw);
				renderMeshInfoList[renderMeshIndex] = value;
			}
		}

		public bool IsRenderMeshFlag(int renderMeshIndex, uint flag)
		{
			if (renderMeshInfoList.Exists(renderMeshIndex))
			{
				return renderMeshInfoList[renderMeshIndex].IsFlag(flag);
			}
			return false;
		}

		public void SetRenderMeshActive(int renderMeshIndex, bool sw)
		{
			if (renderMeshInfoList.Exists(renderMeshIndex))
			{
				RenderMeshInfo value = renderMeshInfoList[renderMeshIndex];
				value.SetFlag(1u, sw);
				value.SetFlag(16777216u, true);
				value.SetFlag(33554432u, true);
				renderMeshInfoList[renderMeshIndex] = value;
				renderMeshStateDict[renderMeshIndex].SetFlag(1u, value.InUse());
				renderMeshStateDict[renderMeshIndex].SetFlag(256u, false);
			}
		}

		public void AddUseRenderMesh(int renderMeshIndex)
		{
			if (renderMeshInfoList.Exists(renderMeshIndex))
			{
				RenderMeshInfo value = renderMeshInfoList[renderMeshIndex];
				value.meshUseCount++;
				renderMeshInfoList[renderMeshIndex] = value;
				renderMeshStateDict[renderMeshIndex].SetFlag(1u, value.InUse());
			}
		}

		public void RemoveUseRenderMesh(int renderMeshIndex)
		{
			if (renderMeshInfoList.Exists(renderMeshIndex))
			{
				RenderMeshInfo value = renderMeshInfoList[renderMeshIndex];
				value.meshUseCount--;
				Debug.Assert(value.meshUseCount >= 0);
				renderMeshInfoList[renderMeshIndex] = value;
				renderMeshStateDict[renderMeshIndex].SetFlag(1u, value.InUse());
			}
		}

		public void LinkRenderMesh(int renderMeshIndex, int childMeshVertexStart, int childMeshWeightStart, int virtualMeshVertexStart, int sharedVirtualMeshVertexStart)
		{
			RenderMeshInfo value = renderMeshInfoList[renderMeshIndex];
			value.AddLinkMesh(renderMeshIndex, childMeshVertexStart, childMeshWeightStart, virtualMeshVertexStart, sharedVirtualMeshVertexStart);
			value.SetFlag(16777216u, true);
			value.SetFlag(33554432u, true);
			renderMeshInfoList[renderMeshIndex] = value;
			renderMeshStateDict[renderMeshIndex].SetFlag(1u, value.InUse());
			renderMeshStateDict[renderMeshIndex].SetFlag(256u, false);
		}

		public void UnlinkRenderMesh(int renderMeshIndex, int childMeshVertexStart, int childMeshWeightStart, int virtualMeshVertexStart, int sharedVirtualMeshVertexStart)
		{
			RenderMeshInfo value = renderMeshInfoList[renderMeshIndex];
			value.RemoveLinkMesh(renderMeshIndex, childMeshVertexStart, childMeshWeightStart, virtualMeshVertexStart, sharedVirtualMeshVertexStart);
			value.SetFlag(16777216u, true);
			value.SetFlag(33554432u, true);
			renderMeshInfoList[renderMeshIndex] = value;
			if (renderMeshStateDict.ContainsKey(renderMeshIndex))
			{
				renderMeshStateDict[renderMeshIndex].SetFlag(1u, value.InUse());
				renderMeshStateDict[renderMeshIndex].SetFlag(256u, false);
			}
		}

		public void CopyToRenderMeshLocalPositionData(int renderMeshIndex, Mesh mesh, int bufferIndex)
		{
			RenderMeshState renderMeshState = renderMeshStateDict[renderMeshIndex];
			SharedRenderMeshBuffer sharedRenderMeshBuffer = sharedRenderMeshIdToBufferDict[renderMeshState.RenderSharedMeshId];
			renderPosList.ToJobArray(bufferIndex).CopyToFast(renderMeshState.VertexChunkStart, sharedRenderMeshBuffer.vertices);
			mesh.vertices = sharedRenderMeshBuffer.vertices;
		}

		public void CopyToRenderMeshLocalNormalTangentData(int renderMeshIndex, Mesh mesh, int bufferIndex, bool normal, bool tangent)
		{
			RenderMeshState renderMeshState = renderMeshStateDict[renderMeshIndex];
			SharedRenderMeshBuffer sharedRenderMeshBuffer = sharedRenderMeshIdToBufferDict[renderMeshState.RenderSharedMeshId];
			if (renderMeshState.IsFlag(2u) && normal)
			{
				renderNormalList.ToJobArray(bufferIndex).CopyToFast(renderMeshState.VertexChunkStart, sharedRenderMeshBuffer.normals);
				mesh.normals = sharedRenderMeshBuffer.normals;
			}
			if (renderMeshState.IsFlag(4u) && tangent)
			{
				renderTangentList.ToJobArray(bufferIndex).CopyToFast(renderMeshState.VertexChunkStart, sharedRenderMeshBuffer.tangents);
				mesh.tangents = sharedRenderMeshBuffer.tangents;
			}
		}

		public void CopyToRenderMeshBoneWeightData(int renderMeshIndex, Mesh mesh, Mesh sharedMesh, int bufferIndex)
		{
			RenderMeshState renderMeshState = renderMeshStateDict[renderMeshIndex];
			SharedRenderMeshBuffer sharedRenderMeshBuffer = sharedRenderMeshIdToBufferDict[renderMeshState.RenderSharedMeshId];
			renderBoneWeightList.ToJobArray(bufferIndex).CopyToFast(renderMeshState.BoneWeightChunkStart, sharedRenderMeshBuffer.boneWeights);
			mesh.boneWeights = sharedRenderMeshBuffer.boneWeights;
		}

		public void CopyToRenderMeshWorldData(int renderMeshIndex, Transform target, Vector3[] vertices, Vector3[] normals, Vector3[] tangents)
		{
			RenderMeshInfo renderMeshInfo = renderMeshInfoList[renderMeshIndex];
			renderPosList.ToJobArray().CopyToFast(renderMeshInfo.vertexChunk.startIndex, vertices);
			renderNormalList.ToJobArray().CopyToFast(renderMeshInfo.vertexChunk.startIndex, normals);
			Vector4[] array = new Vector4[renderMeshInfo.vertexChunk.dataLength];
			renderTangentList.ToJobArray().CopyToFast(renderMeshInfo.vertexChunk.startIndex, array);
			for (int i = 0; i < renderMeshInfo.vertexChunk.dataLength; i++)
			{
				vertices[i] = target.TransformPoint(vertices[i]);
				normals[i] = target.InverseTransformDirection(normals[i]);
				tangents[i] = target.InverseTransformDirection(array[i]);
			}
		}

		public void AddVirtualMeshBone(int virtualMeshIndex, List<Transform> boneList)
		{
			if (virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[virtualMeshIndex];
				for (int i = 0; i < boneList.Count; i++)
				{
					virtualTransformIndexList[virtualMeshInfo.boneChunk.startIndex + i] = base.Bone.AddBone(boneList[i]);
				}
			}
		}

		public void RemoveVirtualMeshBone(int virtualMeshIndex)
		{
			if (virtualMeshIndex >= 0 && virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[virtualMeshIndex];
				for (int i = 0; i < virtualMeshInfo.boneChunk.dataLength; i++)
				{
					int boneIndex = virtualTransformIndexList[virtualMeshInfo.boneChunk.startIndex + i];
					base.Bone.RemoveBone(boneIndex);
					virtualTransformIndexList[virtualMeshInfo.boneChunk.startIndex + i] = 0;
				}
			}
		}

		public void ResetFuturePredictionVirtualMeshBone(int virtualMeshIndex)
		{
			if (virtualMeshIndex >= 0 && virtualMeshInfoList.Exists(virtualMeshIndex))
			{
				VirtualMeshInfo virtualMeshInfo = virtualMeshInfoList[virtualMeshIndex];
				for (int i = 0; i < virtualMeshInfo.boneChunk.dataLength; i++)
				{
					int boneIndex = virtualTransformIndexList[virtualMeshInfo.boneChunk.startIndex + i];
					base.Bone.ResetFuturePrediction(boneIndex);
				}
			}
		}

		public void FinishMesh(int bufferIndex)
		{
			foreach (BaseMeshDeformer item in meshSet)
			{
				if (item != null)
				{
					item.Finish(bufferIndex);
				}
			}
		}
	}
}
