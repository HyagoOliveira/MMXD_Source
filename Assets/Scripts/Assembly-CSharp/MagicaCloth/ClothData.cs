#define RELEASE
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
    [Serializable]
    public class ClothData : ShareDataObject
    {
        private class RestoreDistanceWork
        {
            public uint vertexPair;

            public float dist;
        }

        private class VertexInfo
        {
            public int vertexIndex;

            public int parentVertexIndex = -1;

            public List<int> childVertexList = new List<int>();

            public VertexInfo parentInfo;

            public List<VertexInfo> childInfoList = new List<VertexInfo>();
        }

        private const int DATA_VERSION = 5;

        public const uint VertexFlag_End = 65536u;

        public const uint VertexFlag_TriangleRotation = 131072u;

        public List<int> useVertexList = new List<int>();

        public List<int> selectionData = new List<int>();

        public uint maxLevel;

        public List<uint> vertexFlagLevelList = new List<uint>();

        public List<float> vertexDepthList = new List<float>();

        public List<int> rootList = new List<int>();

        public List<int> parentList = new List<int>();

        public RestoreDistanceConstraint.RestoreDistanceData[] structDistanceDataList;

        public ReferenceDataIndex[] structDistanceReferenceList;

        public RestoreDistanceConstraint.RestoreDistanceData[] bendDistanceDataList;

        public ReferenceDataIndex[] bendDistanceReferenceList;

        public RestoreDistanceConstraint.RestoreDistanceData[] nearDistanceDataList;

        public ReferenceDataIndex[] nearDistanceReferenceList;

        public ClampDistanceConstraint.ClampDistanceData[] rootDistanceDataList;

        public ReferenceDataIndex[] rootDistanceReferenceList;

        public ClampDistance2Constraint.ClampDistance2Data[] clampDistance2DataList;

        public ClampDistance2Constraint.ClampDistance2RootInfo[] clampDistance2RootInfoList;

        public RestoreRotationConstraint.RotationData[] restoreRotationDataList;

        public ReferenceDataIndex[] restoreRotationReferenceList;

        public ClampRotationConstraint.ClampRotationData[] clampRotationDataList;

        public ClampRotationConstraint.ClampRotationRootInfo[] clampRotationRootInfoList;

        public AdjustRotationWorker.AdjustRotationData[] adjustRotationDataList;

        public TriangleBendConstraint.TriangleBendData[] triangleBendDataList;

        public ReferenceDataIndex[] triangleBendReferenceList;

        public int triangleBendWriteBufferCount;

        public VolumeConstraint.VolumeData[] volumeDataList;

        public ReferenceDataIndex[] volumeReferenceList;

        public int volumeWriteBufferCount;

        public LineWorker.LineRotationData[] lineRotationDataList;

        public LineWorker.LineRotationRootInfo[] lineRotationRootInfoList;

        public TriangleWorker.TriangleRotationData[] triangleRotationDataList;

        public int[] triangleRotationIndexList;

        public EdgeCollisionConstraint.EdgeCollisionData[] edgeCollisionDataList;

        public ReferenceDataIndex[] edgeCollisionReferenceList;

        public int edgeCollisionWriteBufferCount;

        public PenetrationConstraint.PenetrationData[] penetrationDataList;

        public ReferenceDataIndex[] penetrationReferenceList;

        public ClothParams.PenetrationMode penetrationMode;

        public Vector3 initScale;

        public int VertexUseCount
        {
            get
            {
                return useVertexList.Count;
            }
        }

        public int StructDistanceConstraintCount
        {
            get
            {
                if (structDistanceDataList == null)
                {
                    return 0;
                }
                return structDistanceDataList.Length;
            }
        }

        public int BendDistanceConstraintCount
        {
            get
            {
                if (bendDistanceDataList == null)
                {
                    return 0;
                }
                return bendDistanceDataList.Length;
            }
        }

        public int NearDistanceConstraintCount
        {
            get
            {
                if (nearDistanceDataList == null)
                {
                    return 0;
                }
                return nearDistanceDataList.Length;
            }
        }

        public int ClampDistanceConstraintCount
        {
            get
            {
                if (rootDistanceDataList == null)
                {
                    return 0;
                }
                return rootDistanceDataList.Length;
            }
        }

        public int ClampDistance2ConstraintCount
        {
            get
            {
                if (clampDistance2DataList == null)
                {
                    return 0;
                }
                return clampDistance2DataList.Length;
            }
        }

        public int RestoreRotationConstraintCount
        {
            get
            {
                if (restoreRotationDataList == null)
                {
                    return 0;
                }
                return restoreRotationDataList.Length;
            }
        }

        public int ClampRotationConstraintDataCount
        {
            get
            {
                if (clampRotationDataList == null)
                {
                    return 0;
                }
                return clampRotationDataList.Length;
            }
        }

        public int ClampRotationConstraintRootCount
        {
            get
            {
                if (clampRotationRootInfoList == null)
                {
                    return 0;
                }
                return clampRotationRootInfoList.Length;
            }
        }

        public int AdjustRotationConstraintCount
        {
            get
            {
                if (adjustRotationDataList == null)
                {
                    return 0;
                }
                return adjustRotationDataList.Length;
            }
        }

        public int TriangleBendConstraintCount
        {
            get
            {
                if (triangleBendDataList == null)
                {
                    return 0;
                }
                return triangleBendDataList.Length;
            }
        }

        public int EdgeCollisionConstraintCount
        {
            get
            {
                if (edgeCollisionDataList == null)
                {
                    return 0;
                }
                return edgeCollisionDataList.Length;
            }
        }

        public int VolumeConstraintCount
        {
            get
            {
                if (volumeDataList == null)
                {
                    return 0;
                }
                return volumeDataList.Length;
            }
        }

        public int LineRotationWorkerCount
        {
            get
            {
                if (lineRotationDataList == null)
                {
                    return 0;
                }
                return lineRotationDataList.Length;
            }
        }

        public int TriangleRotationWorkerCount
        {
            get
            {
                if (triangleRotationDataList == null)
                {
                    return 0;
                }
                return triangleRotationDataList.Length;
            }
        }

        public int PenetrationCount
        {
            get
            {
                if (penetrationDataList == null)
                {
                    return 0;
                }
                return penetrationDataList.Length;
            }
        }

        public override int GetDataHash()
        {
            return 0 + VertexUseCount.GetDataHash() + selectionData.Count.GetDataHash() + maxLevel.GetDataHash() + vertexFlagLevelList.Count.GetDataHash() + vertexDepthList.Count.GetDataHash() + rootList.Count.GetDataHash() + parentList.Count.GetDataHash() + StructDistanceConstraintCount.GetDataHash() + BendDistanceConstraintCount.GetDataHash() + NearDistanceConstraintCount.GetDataHash() + ClampDistanceConstraintCount.GetDataHash() + ClampDistance2ConstraintCount.GetDataHash() + RestoreRotationConstraintCount.GetDataHash() + ClampRotationConstraintDataCount.GetDataHash() + AdjustRotationConstraintCount.GetDataHash() + TriangleBendConstraintCount.GetDataHash() + VolumeConstraintCount.GetDataHash() + LineRotationWorkerCount.GetDataHash() + initScale.GetDataHash();
        }

        public bool IsInvalidVertex(int vindex)
        {
            Debug.Assert(selectionData != null && vindex < selectionData.Count);
            return selectionData[vindex] == 0;
        }

        public bool IsFixedVertex(int vindex)
        {
            Debug.Assert(selectionData != null && vindex < selectionData.Count);
            return selectionData[vindex] == 2;
        }

        public bool IsMoveVertex(int vindex)
        {
            Debug.Assert(selectionData != null && vindex < selectionData.Count);
            return selectionData[vindex] == 1;
        }

        public bool IsExtendVertex(int vindex)
        {
            Debug.Assert(selectionData != null && vindex < selectionData.Count);
            return selectionData[vindex] == 3;
        }

        public bool IsLastLevel(int vindex)
        {
            return IsFlag(vindex, 65536u);
        }

        public bool IsFlag(int vindex, uint flag)
        {
            return (vertexFlagLevelList[vindex] & flag) != 0;
        }

        public void SetFlag(int vindex, uint flag)
        {
            vertexFlagLevelList[vindex] |= flag;
        }

        public int GetLevel(int vindex)
        {
            return (int)(vertexFlagLevelList[vindex] & 0xFFFF);
        }

        public override int GetVersion()
        {
            return 5;
        }

        public override Define.Error VerifyData()
        {
            if (dataHash == 0)
            {
                return Define.Error.InvalidDataHash;
            }
            int vertexUseCount = VertexUseCount;
            if (vertexUseCount == 0)
            {
                return Define.Error.VertexUseCountZero;
            }
            if (selectionData.Count == 0 || selectionData.Count != vertexUseCount)
            {
                return Define.Error.SelectionDataCountMismatch;
            }
            if (vertexFlagLevelList.Count == 0 || vertexFlagLevelList.Count != vertexUseCount)
            {
                return Define.Error.VertexCountMismatch;
            }
            if (vertexDepthList.Count == 0 || vertexDepthList.Count != vertexUseCount)
            {
                return Define.Error.VertexCountMismatch;
            }
            if (rootList.Count == 0 || rootList.Count != vertexUseCount)
            {
                return Define.Error.RootListCountMismatch;
            }
            return Define.Error.None;
        }

        public void CreateData(PhysicsTeam team, ClothParams clothParams, PhysicsTeamData teamData, MeshData meshData, IEditorMesh editMesh, List<int> selData, Action<List<int>, List<int>, List<Vector3>, List<Vector3>, List<Vector3>, List<int>, List<int>> extensionAction = null)
        {
            Debug.Assert(teamData != null);
            Debug.Assert(editMesh != null);
            int num = 0;
            List<Vector3> wposList;
            List<Vector3> wnorList;
            List<Vector3> wtanList;
            num = editMesh.GetEditorPositionNormalTangent(out wposList, out wnorList, out wtanList);
            List<int> editorTriangleList = editMesh.GetEditorTriangleList();
            List<int> editorLineList = editMesh.GetEditorLineList();
            useVertexList.Clear();
            Debug.Assert(num == selData.Count);
            for (int i = 0; i < num; i++)
            {
                if (selData[i] != 0)
                {
                    useVertexList.Add(i);
                    selectionData.Add(selData[i]);
                }
            }
            if (extensionAction != null)
            {
                extensionAction(useVertexList, selectionData, wposList, wnorList, wtanList, editorTriangleList, editorLineList);
                num = wposList.Count;
            }
            CreateVertexData(num, editorLineList, editorTriangleList);
            CreateConstraintData(team, clothParams, teamData, num, wposList, wnorList, wtanList, editorLineList, editorTriangleList);
            CreateVerifyData();
        }

        private void CreateVertexData(int vertexCount, List<int> lineList, List<int> triangleList)
        {
            vertexDepthList.Clear();
            vertexFlagLevelList.Clear();
            maxLevel = 0u;
            if (vertexCount == 0)
            {
                return;
            }
            float[] array = new float[vertexCount];
            uint[] array2 = new uint[vertexCount];
            List<HashSet<int>> triangleToVertexLinkList = MeshUtility.GetTriangleToVertexLinkList(vertexCount, lineList, triangleList);
            uint num = 1u;
            HashSet<int> hashSet = new HashSet<int>();
            for (int i = 0; i < vertexCount; i++)
            {
                int num2 = useVertexList.IndexOf(i);
                if (num2 < 0)
                {
                    hashSet.Add(i);
                }
                else if (IsInvalidVertex(num2) || IsExtendVertex(num2))
                {
                    hashSet.Add(i);
                }
                else if (IsFixedVertex(num2))
                {
                    hashSet.Add(i);
                    array2[i] = num;
                }
            }
            int num3;
            do
            {
                num3 = 0;
                for (int j = 0; j < vertexCount; j++)
                {
                    if (hashSet.Contains(j))
                    {
                        continue;
                    }
                    foreach (int item in triangleToVertexLinkList[j])
                    {
                        if (array2[item] == num)
                        {
                            array2[j] = num + 1;
                            hashSet.Add(j);
                            num3++;
                            break;
                        }
                    }
                }
                if (num3 > 0)
                {
                    num++;
                }
            }
            while (num3 > 0);
            if (num > 1)
            {
                for (int k = 0; k < vertexCount; k++)
                {
                    uint num4 = array2[k];
                    if (num4 != 0)
                    {
                        array[k] = Mathf.Clamp01((float)(num4 - 1) / (float)(num - 1));
                    }
                }
            }
            foreach (int useVertex in useVertexList)
            {
                vertexDepthList.Add(array[useVertex]);
                vertexFlagLevelList.Add(array2[useVertex]);
            }
            maxLevel = num;
        }

        private void CreateConstraintData(PhysicsTeam team, ClothParams clothParams, PhysicsTeamData teamData, int vertexCount, List<Vector3> wposList, List<Vector3> wnorList, List<Vector3> wtanList, List<int> lineList, List<int> triangleList)
        {
            parentList.Clear();
            rootList.Clear();
            structDistanceDataList = null;
            structDistanceReferenceList = null;
            bendDistanceDataList = null;
            bendDistanceReferenceList = null;
            nearDistanceDataList = null;
            nearDistanceReferenceList = null;
            rootDistanceDataList = null;
            rootDistanceReferenceList = null;
            restoreRotationDataList = null;
            restoreRotationReferenceList = null;
            adjustRotationDataList = null;
            triangleBendDataList = null;
            triangleBendReferenceList = null;
            triangleBendWriteBufferCount = 0;
            volumeDataList = null;
            volumeReferenceList = null;
            volumeWriteBufferCount = 0;
            clampRotationDataList = null;
            clampRotationRootInfoList = null;
            lineRotationDataList = null;
            lineRotationRootInfoList = null;
            triangleRotationDataList = null;
            triangleRotationIndexList = null;
            edgeCollisionDataList = null;
            edgeCollisionReferenceList = null;
            edgeCollisionWriteBufferCount = 0;
            if (vertexCount == 0)
            {
                return;
            }
            Transform transform = (clothParams.GetInfluenceTarget() ? clothParams.GetInfluenceTarget() : team.transform);
            initScale = transform.lossyScale;
            List<HashSet<int>> triangleToVertexLinkList = MeshUtility.GetTriangleToVertexLinkList(vertexCount, lineList, triangleList);
            List<HashSet<int>> list = new List<HashSet<int>>();
            for (int i = 0; i < useVertexList.Count; i++)
            {
                int index = useVertexList[i];
                HashSet<int> hashSet = new HashSet<int>();
                foreach (int item31 in triangleToVertexLinkList[index])
                {
                    int num = useVertexList.IndexOf(item31);
                    if (num >= 0)
                    {
                        hashSet.Add(num);
                    }
                }
                list.Add(hashSet);
            }
            if (team is MagicaBoneCloth)
            {
                List<HashSet<int>> triangleToVertexLinkList2 = MeshUtility.GetTriangleToVertexLinkList(vertexCount, lineList, null);
                parentList = GetUseParentVertexList(vertexCount, triangleToVertexLinkList2, wposList, vertexDepthList);
            }
            else
            {
                parentList = GetUseParentVertexList(vertexCount, triangleToVertexLinkList, wposList, vertexDepthList);
            }
            for (int j = 0; j < useVertexList.Count; j++)
            {
                if (!parentList.Contains(j) && selectionData[j] != 3)
                {
                    SetFlag(j, 65536u);
                }
            }
            rootList = GetUseRootVertexList(parentList);
            GetMeshVertexDepthList(vertexCount, vertexDepthList);
            List<List<int>> useRootLineList = GetUseRootLineList(parentList);
            List<RestoreDistanceConstraint.RestoreDistanceData> list2 = new List<RestoreDistanceConstraint.RestoreDistanceData>();
            HashSet<uint> hashSet2 = new HashSet<uint>();
            for (int k = 0; k < useVertexList.Count; k++)
            {
                if (IsInvalidVertex(k) || IsExtendVertex(k))
                {
                    continue;
                }
                int index2 = useVertexList[k];
                foreach (int item32 in triangleToVertexLinkList[index2])
                {
                    int num2 = useVertexList.IndexOf(item32);
                    if (num2 >= 0 && !IsInvalidVertex(num2) && !IsExtendVertex(num2) && ((!IsFixedVertex(k) && !IsExtendVertex(k)) || (!IsFixedVertex(num2) && !IsExtendVertex(num2))))
                    {
                        uint item = DataUtility.PackPair(k, num2);
                        if (!hashSet2.Contains(item))
                        {
                            float length = Vector3.Distance(wposList[index2], wposList[item32]);
                            RestoreDistanceConstraint.RestoreDistanceData item2 = default(RestoreDistanceConstraint.RestoreDistanceData);
                            item2.vertexIndex = (ushort)k;
                            item2.targetVertexIndex = (ushort)num2;
                            item2.length = length;
                            list2.Add(item2);
                            item2 = default(RestoreDistanceConstraint.RestoreDistanceData);
                            item2.vertexIndex = (ushort)num2;
                            item2.targetVertexIndex = (ushort)k;
                            item2.length = length;
                            list2.Add(item2);
                            hashSet2.Add(item);
                        }
                    }
                }
            }
            if (list2.Count > 0)
            {
                ReferenceDataBuilder<RestoreDistanceConstraint.RestoreDistanceData> referenceDataBuilder = new ReferenceDataBuilder<RestoreDistanceConstraint.RestoreDistanceData>();
                referenceDataBuilder.Init(useVertexList.Count);
                foreach (RestoreDistanceConstraint.RestoreDistanceData item33 in list2)
                {
                    referenceDataBuilder.AddData(item33, item33.vertexIndex);
                }
                ValueTuple<List<ReferenceDataIndex>, List<RestoreDistanceConstraint.RestoreDistanceData>> directReferenceData = referenceDataBuilder.GetDirectReferenceData();
                List<ReferenceDataIndex> item3 = directReferenceData.Item1;
                List<RestoreDistanceConstraint.RestoreDistanceData> item4 = directReferenceData.Item2;
                structDistanceDataList = item4.ToArray();
                structDistanceReferenceList = item3.ToArray();
            }
            if (clothParams.UseBendDistance)
            {
                list2.Clear();
                for (int l = 0; l < useVertexList.Count; l++)
                {
                    if (IsInvalidVertex(l) || IsExtendVertex(l))
                    {
                        continue;
                    }
                    int index3 = useVertexList[l];
                    Vector3 vector = wposList[index3];
                    List<RestoreDistanceWork> list3 = new List<RestoreDistanceWork>();
                    List<int> list4 = new List<int>(triangleToVertexLinkList[index3]);
                    for (int m = 0; m < list4.Count - 1; m++)
                    {
                        int num3 = list4[m];
                        int num4 = useVertexList.IndexOf(num3);
                        if (num4 < 0 || IsInvalidVertex(num4) || IsExtendVertex(num4))
                        {
                            continue;
                        }
                        for (int n = m + 1; n < list4.Count; n++)
                        {
                            int num5 = list4[n];
                            int num6 = useVertexList.IndexOf(num5);
                            if (num6 >= 0 && !IsInvalidVertex(num6) && !IsExtendVertex(num6) && ((!IsFixedVertex(num4) && !IsExtendVertex(num4)) || (!IsFixedVertex(num6) && !IsExtendVertex(num6))))
                            {
                                uint vertexPair = DataUtility.PackPair(num4, num6);
                                RestoreDistanceWork restoreDistanceWork = new RestoreDistanceWork();
                                restoreDistanceWork.vertexPair = vertexPair;
                                Vector3 vector2 = wposList[num3];
                                Vector3 vector3 = wposList[num5];
                                float3 y = MathUtility.ClosestPtPointSegment(vector, vector2, vector3);
                                restoreDistanceWork.dist = math.distance(vector, y);
                                list3.Add(restoreDistanceWork);
                            }
                        }
                    }
                    list3.Sort((RestoreDistanceWork a, RestoreDistanceWork b) => (!(a.dist < b.dist)) ? 1 : (-1));
                    for (int num7 = 0; num7 < list3.Count && num7 < clothParams.BendDistanceMaxCount; num7++)
                    {
                        RestoreDistanceWork restoreDistanceWork2 = list3[num7];
                        if (!hashSet2.Contains(restoreDistanceWork2.vertexPair))
                        {
                            int v;
                            int v2;
                            DataUtility.UnpackPair(restoreDistanceWork2.vertexPair, out v, out v2);
                            int index4 = useVertexList[v];
                            int index5 = useVertexList[v2];
                            float length2 = Vector3.Distance(wposList[index4], wposList[index5]);
                            RestoreDistanceConstraint.RestoreDistanceData item5 = default(RestoreDistanceConstraint.RestoreDistanceData);
                            item5.vertexIndex = (ushort)v;
                            item5.targetVertexIndex = (ushort)v2;
                            item5.length = length2;
                            list2.Add(item5);
                            item5 = default(RestoreDistanceConstraint.RestoreDistanceData);
                            item5.vertexIndex = (ushort)v2;
                            item5.targetVertexIndex = (ushort)v;
                            item5.length = length2;
                            list2.Add(item5);
                            hashSet2.Add(restoreDistanceWork2.vertexPair);
                        }
                    }
                }
                if (list2.Count > 0)
                {
                    ReferenceDataBuilder<RestoreDistanceConstraint.RestoreDistanceData> referenceDataBuilder2 = new ReferenceDataBuilder<RestoreDistanceConstraint.RestoreDistanceData>();
                    referenceDataBuilder2.Init(useVertexList.Count);
                    foreach (RestoreDistanceConstraint.RestoreDistanceData item34 in list2)
                    {
                        referenceDataBuilder2.AddData(item34, item34.vertexIndex);
                    }
                    ValueTuple<List<ReferenceDataIndex>, List<RestoreDistanceConstraint.RestoreDistanceData>> directReferenceData2 = referenceDataBuilder2.GetDirectReferenceData();
                    List<ReferenceDataIndex> item6 = directReferenceData2.Item1;
                    List<RestoreDistanceConstraint.RestoreDistanceData> item7 = directReferenceData2.Item2;
                    bendDistanceDataList = item7.ToArray();
                    bendDistanceReferenceList = item6.ToArray();
                }
            }
            if (clothParams.UseNearDistance)
            {
                list2.Clear();
                for (int num8 = 0; num8 < useVertexList.Count; num8++)
                {
                    if (!IsMoveVertex(num8))
                    {
                        continue;
                    }
                    int index6 = useVertexList[num8];
                    Vector3 a2 = wposList[index6];
                    float num51 = vertexDepthList[num8];
                    if (vertexDepthList[num8] > clothParams.NearDistanceMaxDepth)
                    {
                        continue;
                    }
                    List<RestoreDistanceWork> list5 = new List<RestoreDistanceWork>();
                    HashSet<int> hashSet3 = list[num8];
                    for (int num9 = 0; num9 < useVertexList.Count; num9++)
                    {
                        if (num8 == num9 || hashSet3.Contains(num9) || vertexDepthList[num9] > clothParams.NearDistanceMaxDepth)
                        {
                            continue;
                        }
                        uint num10 = DataUtility.PackPair(num8, num9);
                        if (!hashSet2.Contains(num10))
                        {
                            int index7 = useVertexList[num9];
                            Vector3 b2 = wposList[index7];
                            float num11 = Vector3.Distance(a2, b2);
                            if (num11 <= clothParams.GetNearDistanceLength().Evaluate(vertexDepthList[num8]))
                            {
                                RestoreDistanceWork restoreDistanceWork3 = new RestoreDistanceWork();
                                restoreDistanceWork3.vertexPair = num10;
                                restoreDistanceWork3.dist = num11;
                                list5.Add(restoreDistanceWork3);
                            }
                        }
                    }
                    list5.Sort((RestoreDistanceWork a, RestoreDistanceWork b) => (!(a.dist < b.dist)) ? 1 : (-1));
                    for (int num12 = 0; num12 < list5.Count && num12 < clothParams.NearDistanceMaxCount; num12++)
                    {
                        RestoreDistanceWork restoreDistanceWork4 = list5[num12];
                        if (!hashSet2.Contains(restoreDistanceWork4.vertexPair))
                        {
                            int v3;
                            int v4;
                            DataUtility.UnpackPair(restoreDistanceWork4.vertexPair, out v3, out v4);
                            RestoreDistanceConstraint.RestoreDistanceData item8 = default(RestoreDistanceConstraint.RestoreDistanceData);
                            item8.vertexIndex = (ushort)v3;
                            item8.targetVertexIndex = (ushort)v4;
                            item8.length = restoreDistanceWork4.dist;
                            list2.Add(item8);
                            item8 = default(RestoreDistanceConstraint.RestoreDistanceData);
                            item8.vertexIndex = (ushort)v4;
                            item8.targetVertexIndex = (ushort)v3;
                            item8.length = restoreDistanceWork4.dist;
                            list2.Add(item8);
                            hashSet2.Add(restoreDistanceWork4.vertexPair);
                        }
                    }
                }
                if (list2.Count > 0)
                {
                    ReferenceDataBuilder<RestoreDistanceConstraint.RestoreDistanceData> referenceDataBuilder3 = new ReferenceDataBuilder<RestoreDistanceConstraint.RestoreDistanceData>();
                    referenceDataBuilder3.Init(useVertexList.Count);
                    foreach (RestoreDistanceConstraint.RestoreDistanceData item35 in list2)
                    {
                        referenceDataBuilder3.AddData(item35, item35.vertexIndex);
                    }
                    ValueTuple<List<ReferenceDataIndex>, List<RestoreDistanceConstraint.RestoreDistanceData>> directReferenceData3 = referenceDataBuilder3.GetDirectReferenceData();
                    List<ReferenceDataIndex> item9 = directReferenceData3.Item1;
                    List<RestoreDistanceConstraint.RestoreDistanceData> item10 = directReferenceData3.Item2;
                    nearDistanceDataList = item10.ToArray();
                    nearDistanceReferenceList = item9.ToArray();
                }
            }
            List<ClampDistanceConstraint.ClampDistanceData> list6 = new List<ClampDistanceConstraint.ClampDistanceData>();
            if (clothParams.UseClampDistanceRatio)
            {
                for (int num13 = 0; num13 < useVertexList.Count; num13++)
                {
                    if (!IsInvalidVertex(num13) && IsMoveVertex(num13) && rootList[num13] >= 0)
                    {
                        int index8 = useVertexList[num13];
                        int index9 = useVertexList[rootList[num13]];
                        float length3 = Vector3.Distance(wposList[index8], wposList[index9]);
                        ClampDistanceConstraint.ClampDistanceData item11 = default(ClampDistanceConstraint.ClampDistanceData);
                        item11.vertexIndex = (ushort)num13;
                        item11.targetVertexIndex = (ushort)rootList[num13];
                        item11.length = length3;
                        list6.Add(item11);
                    }
                }
            }
            if (list6.Count > 0)
            {
                ReferenceDataBuilder<ClampDistanceConstraint.ClampDistanceData> referenceDataBuilder4 = new ReferenceDataBuilder<ClampDistanceConstraint.ClampDistanceData>();
                referenceDataBuilder4.Init(useVertexList.Count);
                foreach (ClampDistanceConstraint.ClampDistanceData item36 in list6)
                {
                    referenceDataBuilder4.AddData(item36, item36.vertexIndex);
                }
                ValueTuple<List<ReferenceDataIndex>, List<ClampDistanceConstraint.ClampDistanceData>> directReferenceData4 = referenceDataBuilder4.GetDirectReferenceData();
                List<ReferenceDataIndex> item12 = directReferenceData4.Item1;
                List<ClampDistanceConstraint.ClampDistanceData> item13 = directReferenceData4.Item2;
                rootDistanceDataList = item13.ToArray();
                rootDistanceReferenceList = item12.ToArray();
            }
            List<RestoreRotationConstraint.RotationData> list7 = new List<RestoreRotationConstraint.RotationData>();
            if (clothParams.UseRestoreRotation)
            {
                for (int num14 = 0; num14 < useVertexList.Count; num14++)
                {
                    if (!IsInvalidVertex(num14) && !IsExtendVertex(num14) && IsMoveVertex(num14))
                    {
                        int num15 = parentList[num14];
                        if (num15 >= 0)
                        {
                            int index10 = useVertexList[num14];
                            int index11 = useVertexList[num15];
                            Vector3 vector4 = wposList[index10] - wposList[index11];
                            Vector3 vector5 = Quaternion.Inverse(Quaternion.LookRotation(wnorList[index11], wtanList[index11])) * vector4;
                            RestoreRotationConstraint.RotationData item14 = default(RestoreRotationConstraint.RotationData);
                            item14.vertexIndex = (ushort)num14;
                            item14.targetVertexIndex = (ushort)num15;
                            item14.localPos = vector5;
                            list7.Add(item14);
                        }
                    }
                }
            }
            if (list7.Count > 0)
            {
                ReferenceDataBuilder<RestoreRotationConstraint.RotationData> referenceDataBuilder5 = new ReferenceDataBuilder<RestoreRotationConstraint.RotationData>();
                referenceDataBuilder5.Init(useVertexList.Count);
                foreach (RestoreRotationConstraint.RotationData item37 in list7)
                {
                    referenceDataBuilder5.AddData(item37, item37.vertexIndex);
                }
                ValueTuple<List<ReferenceDataIndex>, List<RestoreRotationConstraint.RotationData>> directReferenceData5 = referenceDataBuilder5.GetDirectReferenceData();
                List<ReferenceDataIndex> item15 = directReferenceData5.Item1;
                List<RestoreRotationConstraint.RotationData> item16 = directReferenceData5.Item2;
                restoreRotationDataList = item16.ToArray();
                restoreRotationReferenceList = item15.ToArray();
            }
            List<ClampRotationConstraint.ClampRotationData> list8 = new List<ClampRotationConstraint.ClampRotationData>();
            List<ClampRotationConstraint.ClampRotationRootInfo> list9 = new List<ClampRotationConstraint.ClampRotationRootInfo>();
            if (clothParams.UseClampRotation)
            {
                foreach (List<int> item38 in useRootLineList)
                {
                    if (item38.Count <= 1)
                    {
                        continue;
                    }
                    ClampRotationConstraint.ClampRotationRootInfo item17 = default(ClampRotationConstraint.ClampRotationRootInfo);
                    item17.startIndex = (ushort)list8.Count;
                    item17.dataLength = (ushort)item38.Count;
                    for (int num16 = 0; num16 < item38.Count; num16++)
                    {
                        int num17 = item38[num16];
                        int num18 = parentList[num17];
                        ClampRotationConstraint.ClampRotationData item18 = default(ClampRotationConstraint.ClampRotationData);
                        item18.vertexIndex = num17;
                        item18.parentVertexIndex = num18;
                        if (num18 >= 0)
                        {
                            int index12 = useVertexList[num17];
                            int index13 = useVertexList[num18];
                            Vector3 vector6 = wposList[index12] - wposList[index13];
                            vector6.Normalize();
                            Quaternion quaternion = Quaternion.Inverse(Quaternion.LookRotation(wnorList[index13], wtanList[index13]));
                            Vector3 vector7 = quaternion * vector6;
                            Quaternion quaternion2 = Quaternion.LookRotation(wnorList[index12], wtanList[index12]);
                            Quaternion quaternion3 = quaternion * quaternion2;
                            item18.localPos = vector7;
                            item18.localRot = quaternion3;
                        }
                        list8.Add(item18);
                    }
                    list9.Add(item17);
                }
            }
            if (list8.Count > 0)
            {
                clampRotationDataList = list8.ToArray();
                clampRotationRootInfoList = list9.ToArray();
            }
            MagicaBoneCloth magicaBoneCloth = team as MagicaBoneCloth;
            if (magicaBoneCloth != null && magicaBoneCloth.ClothTarget.Connection == BoneClothTarget.ConnectionMode.Mesh && triangleList.Count > 0)
            {
                List<TriangleWorker.TriangleRotationData> list10 = new List<TriangleWorker.TriangleRotationData>();
                List<int> list11 = new List<int>();
                List<VertexInfo> useVertexInfoList = GetUseVertexInfoList(parentList);
                int num19 = triangleList.Count / 3;
                for (int num20 = 0; num20 < useVertexList.Count; num20++)
                {
                    int num21 = useVertexList[num20];
                    VertexInfo vertexInfo = useVertexInfoList[num20];
                    TriangleWorker.TriangleRotationData item19 = default(TriangleWorker.TriangleRotationData);
                    int num22 = 0;
                    int count = list11.Count;
                    Vector3 zero = Vector3.zero;
                    int num23 = -1;
                    if (vertexInfo.parentVertexIndex >= 0)
                    {
                        num23 = vertexInfo.parentVertexIndex;
                    }
                    else if (vertexInfo.childVertexList.Count > 0)
                    {
                        num23 = vertexInfo.childVertexList[0];
                    }
                    if (num23 >= 0)
                    {
                        List<int> list12 = new List<int>();
                        list12.Add(vertexInfo.parentVertexIndex);
                        list12.AddRange(vertexInfo.childVertexList);
                        for (int num24 = 0; num24 < num19; num24++)
                        {
                            int num25 = triangleList[num24 * 3];
                            int num26 = triangleList[num24 * 3 + 1];
                            int num27 = triangleList[num24 * 3 + 2];
                            int num28 = useVertexList.IndexOf(num25);
                            int num29 = useVertexList.IndexOf(num26);
                            int num30 = useVertexList.IndexOf(num27);
                            if ((num21 == num25 || num21 == num26 || num21 == num27) && num28 >= 0 && num29 >= 0 && num30 >= 0)
                            {
                                list11.Add(num28);
                                list11.Add(num29);
                                list11.Add(num30);
                                num22++;
                                zero += Vector3.Cross(wposList[num26] - wposList[num25], wposList[num27] - wposList[num25]).normalized;
                            }
                        }
                    }
                    Quaternion quaternion4 = Quaternion.identity;
                    if (num22 > 0)
                    {
                        zero.Normalize();
                        Vector3 upwards = wposList[useVertexList[num23]] - wposList[num21];
                        upwards.Normalize();
                        Quaternion quaternion5 = Quaternion.Inverse(Quaternion.LookRotation(zero, upwards));
                        Quaternion quaternion6 = Quaternion.LookRotation(wnorList[num21], wtanList[num21]);
                        quaternion4 = quaternion5 * quaternion6;
                    }
                    item19.targetIndex = ((num23 >= 0) ? num23 : (-1));
                    item19.triangleCount = num22;
                    item19.triangleStartIndex = count;
                    item19.localRot = quaternion4;
                    if (num22 > 0)
                    {
                        SetFlag(num20, 131072u);
                    }
                    list10.Add(item19);
                }
                if (list10.Count > 0)
                {
                    triangleRotationDataList = list10.ToArray();
                    triangleRotationIndexList = list11.ToArray();
                }
            }
            List<LineWorker.LineRotationData> list13 = new List<LineWorker.LineRotationData>();
            List<LineWorker.LineRotationRootInfo> list14 = new List<LineWorker.LineRotationRootInfo>();
            if (lineList.Count > 0)
            {
                List<VertexInfo> useVertexInfoList2 = GetUseVertexInfoList(parentList);
                HashSet<int> hashSet4 = new HashSet<int>();
                for (int num31 = 0; num31 < lineList.Count; num31++)
                {
                    int item20 = lineList[num31];
                    if (hashSet4.Contains(item20))
                    {
                        continue;
                    }
                    hashSet4.Add(item20);
                    int num32 = useVertexList.IndexOf(item20);
                    if (num32 < 0)
                    {
                        continue;
                    }
                    VertexInfo vertexInfo2 = useVertexInfoList2[num32];
                    if (vertexInfo2.parentVertexIndex >= 0)
                    {
                        continue;
                    }
                    LineWorker.LineRotationRootInfo item21 = default(LineWorker.LineRotationRootInfo);
                    item21.startIndex = (ushort)list13.Count;
                    int num33 = list13.Count;
                    int num34 = 0;
                    List<LineWorker.LineRotationData> list15 = new List<LineWorker.LineRotationData>();
                    Queue<VertexInfo> queue = new Queue<VertexInfo>();
                    queue.Enqueue(vertexInfo2);
                    while (queue.Count > 0)
                    {
                        vertexInfo2 = queue.Dequeue();
                        LineWorker.LineRotationData item22 = default(LineWorker.LineRotationData);
                        num32 = (item22.vertexIndex = vertexInfo2.vertexIndex);
                        item22.localPos = float3.zero;
                        item22.localRot = Unity.Mathematics.quaternion.identity;
                        if (vertexInfo2.parentInfo != null)
                        {
                            int parentVertexIndex = vertexInfo2.parentVertexIndex;
                            int index14 = useVertexList[num32];
                            int index15 = useVertexList[parentVertexIndex];
                            Vector3 vector8 = wposList[index14] - wposList[index15];
                            Quaternion quaternion7 = Quaternion.Inverse(Quaternion.LookRotation(wnorList[index15], wtanList[index15]));
                            Vector3 vector9 = quaternion7 * vector8;
                            Quaternion quaternion8 = Quaternion.LookRotation(wnorList[index14], wtanList[index14]);
                            Quaternion quaternion9 = quaternion7 * quaternion8;
                            item22.localPos = vector9;
                            item22.localRot = quaternion9;
                        }
                        if (vertexInfo2.childInfoList.Count > 0)
                        {
                            item22.childCount = vertexInfo2.childInfoList.Count;
                            item22.childStartDataIndex = num33 + 1;
                        }
                        list15.Add(item22);
                        num34++;
                        num33 += vertexInfo2.childInfoList.Count;
                        foreach (VertexInfo childInfo in vertexInfo2.childInfoList)
                        {
                            queue.Enqueue(childInfo);
                        }
                    }
                    item21.dataLength = (ushort)num34;
                    if (num34 <= 1)
                    {
                        continue;
                    }
                    bool flag = false;
                    foreach (LineWorker.LineRotationData item39 in list15)
                    {
                        if (!IsFlag(item39.vertexIndex, 131072u))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        list13.AddRange(list15);
                        list14.Add(item21);
                    }
                }
            }
            if (list13.Count > 0)
            {
                lineRotationDataList = list13.ToArray();
                lineRotationRootInfoList = list14.ToArray();
            }
            List<TriangleBendConstraint.TriangleBendData> list16 = new List<TriangleBendConstraint.TriangleBendData>();
            if (clothParams.UseTriangleBend)
            {
                foreach (KeyValuePair<uint, List<int>> item40 in MeshUtility.GetTriangleEdgePair(triangleList))
                {
                    int v5;
                    int v6;
                    DataUtility.UnpackPair(item40.Key, out v5, out v6);
                    int num35 = useVertexList.IndexOf(v5);
                    int num36 = useVertexList.IndexOf(v6);
                    if (num35 < 0 || num36 < 0 || IsInvalidVertex(num35) || IsExtendVertex(num35) || IsInvalidVertex(num36) || IsExtendVertex(num36) || ((IsFixedVertex(num35) || IsExtendVertex(num35)) && (IsFixedVertex(num36) || IsExtendVertex(num36))))
                    {
                        continue;
                    }
                    List<int> value = item40.Value;
                    for (int num37 = 0; num37 < value.Count - 1; num37++)
                    {
                        int tindex = value[num37];
                        int num38 = RestTriangleVertex(tindex, v5, v6, triangleList);
                        int num39 = useVertexList.IndexOf(num38);
                        if (num39 < 0 || IsInvalidVertex(num39) || IsExtendVertex(num39))
                        {
                            continue;
                        }
                        for (int num40 = num37 + 1; num40 < value.Count; num40++)
                        {
                            int tindex2 = value[num40];
                            int num41 = RestTriangleVertex(tindex2, v5, v6, triangleList);
                            int num42 = useVertexList.IndexOf(num41);
                            float restAngle;
                            if (num42 >= 0 && !IsInvalidVertex(num42) && !IsExtendVertex(num42) && CalcTriangleBendRestAngle(wposList[num38], wposList[num41], wposList[v5], wposList[v6], out restAngle))
                            {
                                TriangleBendConstraint.TriangleBendData item23 = default(TriangleBendConstraint.TriangleBendData);
                                item23.vindex0 = num39;
                                item23.vindex1 = num42;
                                item23.vindex2 = num35;
                                item23.vindex3 = num36;
                                item23.restAngle = restAngle;
                                item23.depth = (vertexDepthList[num35] + vertexDepthList[num36]) * 0.5f;
                                list16.Add(item23);
                            }
                        }
                    }
                }
            }
            if (list16.Count > 0)
            {
                ReferenceDataBuilder<TriangleBendConstraint.TriangleBendData> referenceDataBuilder6 = new ReferenceDataBuilder<TriangleBendConstraint.TriangleBendData>();
                referenceDataBuilder6.Init(useVertexList.Count);
                foreach (TriangleBendConstraint.TriangleBendData item41 in list16)
                {
                    referenceDataBuilder6.AddData(item41, item41.vindex0, item41.vindex1, item41.vindex2, item41.vindex3);
                }
                ValueTuple<List<ReferenceDataIndex>, List<int>, List<List<int>>> indirectReferenceData = referenceDataBuilder6.GetIndirectReferenceData();
                List<ReferenceDataIndex> item24 = indirectReferenceData.Item1;
                List<int> item25 = indirectReferenceData.Item2;
                List<List<int>> item26 = indirectReferenceData.Item3;
                for (int num43 = 0; num43 < list16.Count; num43++)
                {
                    TriangleBendConstraint.TriangleBendData value2 = list16[num43];
                    value2.writeIndex0 = item26[num43][0];
                    value2.writeIndex1 = item26[num43][1];
                    value2.writeIndex2 = item26[num43][2];
                    value2.writeIndex3 = item26[num43][3];
                    list16[num43] = value2;
                }
                triangleBendDataList = list16.ToArray();
                triangleBendReferenceList = item24.ToArray();
                triangleBendWriteBufferCount = item25.Count;
            }
            List<PenetrationConstraint.PenetrationData> list17 = new List<PenetrationConstraint.PenetrationData>();
            if (clothParams.UsePenetration)
            {
                for (int num44 = 0; num44 < useVertexList.Count; num44++)
                {
                    if (!IsMoveVertex(num44))
                    {
                        continue;
                    }
                    int index16 = useVertexList[num44];
                    Vector3 vector10 = wposList[index16];
                    float num45 = vertexDepthList[num44];
                    List<PenetrationConstraint.PenetrationData> list18 = new List<PenetrationConstraint.PenetrationData>();
                    if (clothParams.GetPenetrationMode() == ClothParams.PenetrationMode.SurfacePenetration)
                    {
                        if (num45 <= clothParams.PenetrationMaxDepth)
                        {
                            Vector3 vector11 = Vector3.zero;
                            switch (clothParams.GetPenetrationAxis())
                            {
                                case ClothParams.PenetrationAxis.X:
                                    vector11 = Vector3.right;
                                    break;
                                case ClothParams.PenetrationAxis.Y:
                                    vector11 = Vector3.up;
                                    break;
                                case ClothParams.PenetrationAxis.Z:
                                    vector11 = Vector3.forward;
                                    break;
                                case ClothParams.PenetrationAxis.InverseX:
                                    vector11 = Vector3.left;
                                    break;
                                case ClothParams.PenetrationAxis.InverseY:
                                    vector11 = Vector3.down;
                                    break;
                                case ClothParams.PenetrationAxis.InverseZ:
                                    vector11 = Vector3.back;
                                    break;
                            }
                            PenetrationConstraint.PenetrationData item27 = default(PenetrationConstraint.PenetrationData);
                            item27.vertexIndex = (short)num44;
                            item27.localDir = vector11;
                            list18.Add(item27);
                        }
                    }
                    else if (clothParams.GetPenetrationMode() == ClothParams.PenetrationMode.ColliderPenetration)
                    {
                        if (num45 <= clothParams.PenetrationMaxDepth)
                        {
                            for (int num46 = 0; num46 < teamData.ColliderCount; num46++)
                            {
                                ColliderComponent colliderComponent = teamData.ColliderList[num46];
                                Vector3 p;
                                Vector3 dir;
                                Vector3 d;
                                if (!(colliderComponent == null) && !teamData.PenetrationIgnoreColliderList.Contains(colliderComponent) && colliderComponent.CalcNearPoint(vector10, out p, out dir, out d, false))
                                {
                                    float num47 = Vector3.Distance(vector10, p);
                                    if (!(num47 > clothParams.GetPenetrationConnectDistance().Evaluate(num45)))
                                    {
                                        PenetrationConstraint.PenetrationData item28 = default(PenetrationConstraint.PenetrationData);
                                        item28.vertexIndex = (short)num44;
                                        item28.colliderIndex = (short)num46;
                                        item28.localPos = colliderComponent.transform.InverseTransformPoint(d);
                                        item28.localDir = colliderComponent.transform.InverseTransformDirection(vector10 - d);
                                        item28.distance = num47;
                                        list18.Add(item28);
                                    }
                                }
                            }
                            if (list18.Count >= 2)
                            {
                                list18.Sort((PenetrationConstraint.PenetrationData a, PenetrationConstraint.PenetrationData b) => (!(a.distance < b.distance)) ? 1 : (-1));
                            }
                        }
                        if (list18.Count >= 2)
                        {
                            float num48 = list18[0].distance * 1.5f;
                            int num49 = 1;
                            while (num49 < list18.Count)
                            {
                                if (list18[num49].distance > num48)
                                {
                                    list18.RemoveAt(num49);
                                }
                                else
                                {
                                    num49++;
                                }
                            }
                        }
                        if (list18.Count > 2)
                        {
                            list18.RemoveRange(2, list18.Count - 2);
                        }
                        for (int num50 = 0; num50 < list18.Count; num50++)
                        {
                            PenetrationConstraint.PenetrationData value3 = list18[num50];
                            value3.distance = math.length(value3.localDir);
                            value3.localDir = math.normalize(value3.localDir);
                            list18[num50] = value3;
                        }
                    }
                    foreach (PenetrationConstraint.PenetrationData item42 in list18)
                    {
                        list17.Add(item42);
                    }
                }
            }
            if (list17.Count <= 0)
            {
                return;
            }
            ReferenceDataBuilder<PenetrationConstraint.PenetrationData> referenceDataBuilder7 = new ReferenceDataBuilder<PenetrationConstraint.PenetrationData>();
            referenceDataBuilder7.Init(useVertexList.Count);
            foreach (PenetrationConstraint.PenetrationData item43 in list17)
            {
                referenceDataBuilder7.AddData(item43, item43.vertexIndex);
            }
            ValueTuple<List<ReferenceDataIndex>, List<PenetrationConstraint.PenetrationData>> directReferenceData6 = referenceDataBuilder7.GetDirectReferenceData();
            List<ReferenceDataIndex> item29 = directReferenceData6.Item1;
            List<PenetrationConstraint.PenetrationData> item30 = directReferenceData6.Item2;
            penetrationDataList = item30.ToArray();
            penetrationReferenceList = item29.ToArray();
            penetrationMode = clothParams.GetPenetrationMode();
        }

        private List<int> SortTetra(int v0, int v1, int v2, int v3, List<float> meshVertexDepthList)
        {
            List<int> list = new List<int>();
            list.Add(v0);
            list.Add(v1);
            list.Add(v2);
            list.Add(v3);
            list.Sort((int a, int b) => (!(meshVertexDepthList[a] < meshVertexDepthList[b])) ? 1 : (-1));
            return list;
        }

        private List<int> CheckTetraDirection(int v0, int v1, int v2, int v3, HashSet<ulong> trianglePackSet, List<float> meshVertexDepthList)
        {
            ulong item = DataUtility.PackTriple(v0, v1, v2);
            ulong item2 = DataUtility.PackTriple(v0, v2, v3);
            ulong item3 = DataUtility.PackTriple(v0, v3, v1);
            ulong item4 = DataUtility.PackTriple(v1, v2, v3);
            List<ulong> list = new List<ulong>();
            list.Add(item);
            list.Add(item2);
            list.Add(item3);
            list.Add(item4);
            int num = 0;
            while (num < list.Count)
            {
                if (!trianglePackSet.Contains(list[num]))
                {
                    list.RemoveAt(num);
                }
                else
                {
                    num++;
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            if (list.Count >= 2)
            {
                list.Sort(delegate (ulong a, ulong b)
                {
                    int v4;
                    int v5;
                    int v6;
                    DataUtility.UnpackTriple(a, out v4, out v5, out v6);
                    int v7;
                    int v8;
                    int v9;
                    DataUtility.UnpackTriple(b, out v7, out v8, out v9);
                    float num2 = (meshVertexDepthList[v4] + meshVertexDepthList[v5] + meshVertexDepthList[v6]) / 3f;
                    float num3 = (meshVertexDepthList[v7] + meshVertexDepthList[v8] + meshVertexDepthList[v9]) / 3f;
                    return (!(num2 > num3)) ? 1 : (-1);
                });
            }
            List<int> obj = new List<int> { v0, v1, v2, v3 };
            DataUtility.UnpackTriple(list[0], out v0, out v1, out v2);
            obj.Remove(v0);
            obj.Remove(v1);
            obj.Remove(v2);
            v3 = obj[0];
            if (meshVertexDepthList[v3] == 0f)
            {
                return null;
            }
            return new List<int> { v0, v1, v2, v3 };
        }

        private bool CalcTriangleBendRestAngle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, out float restAngle)
        {
            restAngle = 0f;
            if ((p3 - p2).magnitude < 1E-06f)
            {
                return false;
            }
            Vector3 lhs = Vector3.Cross(p2 - p0, p3 - p0);
            Vector3 rhs = Vector3.Cross(p3 - p1, p2 - p1);
            lhs /= lhs.sqrMagnitude;
            rhs /= rhs.sqrMagnitude;
            lhs.Normalize();
            rhs.Normalize();
            float num = Vector3.Dot(lhs, rhs);
            if (num < -1f)
            {
                num = -1f;
            }
            if (num > 1f)
            {
                num = 1f;
            }
            restAngle = Mathf.Acos(num);
            return true;
        }

        private int RestTriangleVertex(int tindex, int v0, int v1, List<int> triangleList)
        {
            int num = tindex * 3;
            for (int i = 0; i < 3; i++)
            {
                int num2 = triangleList[num + i];
                if (num2 != v0 && num2 != v1)
                {
                    return num2;
                }
            }
            return 0;
        }

        private List<float> GetMeshVertexDepthList(int vertexCount, List<float> depthList)
        {
            List<float> list = new List<float>();
            for (int i = 0; i < vertexCount; i++)
            {
                float item = 0f;
                int item2 = i;
                int num = useVertexList.IndexOf(item2);
                if (num >= 0)
                {
                    item = depthList[num];
                }
                list.Add(item);
            }
            return list;
        }

        private List<int> GetUseParentVertexList(int vertexCount, List<HashSet<int>> vlink, List<Vector3> wposList, List<float> depthList)
        {
            List<int> list = new List<int>();
            List<int> list2 = new List<int>();
            List<int> list3 = new List<int>();
            for (int i = 0; i < vertexCount; i++)
            {
                list.Add(-1);
                int num = useVertexList.IndexOf(i);
                if (num >= 0)
                {
                    list2.Add(num);
                    if (IsFixedVertex(num))
                    {
                        list3.Add(num);
                    }
                }
            }
            List<Vector3> list4 = new List<Vector3>();
            List<float> nearFixedDistList = new List<float>();
            for (int j = 0; j < depthList.Count; j++)
            {
                Vector3 item = Vector3.zero;
                float item2 = 0f;
                int index = useVertexList[j];
                Vector3 a2 = wposList[index];
                float num2 = 10000f;
                Vector3 zero = Vector3.zero;
                foreach (int item3 in list3)
                {
                    int index2 = useVertexList[item3];
                    float num3 = Vector3.Distance(a2, wposList[index2]);
                    if (num3 < num2)
                    {
                        num2 = num3;
                        item = wposList[index2];
                        item2 = num3;
                    }
                }
                list4.Add(item);
                nearFixedDistList.Add(item2);
            }
            if (list3.Count > 0)
            {
                list2.Sort(delegate (int a, int b)
                {
                    if (depthList[a] > depthList[b])
                    {
                        return -1;
                    }
                    if (depthList[a] < depthList[b])
                    {
                        return 1;
                    }
                    return (!(nearFixedDistList[a] > nearFixedDistList[b])) ? 1 : (-1);
                });
                new HashSet<int>();
                foreach (int item4 in list2)
                {
                    if (!IsMoveVertex(item4))
                    {
                        continue;
                    }
                    int num4 = useVertexList[item4];
                    Vector3 vector = wposList[num4];
                    Vector3 normalized = (list4[item4] - vector).normalized;
                    HashSet<int> hashSet = vlink[num4];
                    int value = -1;
                    int num5 = -1;
                    Vector3 zero2 = Vector3.zero;
                    float num6 = -10000f;
                    foreach (int item5 in hashSet)
                    {
                        int num7 = useVertexList.IndexOf(item5);
                        if (num7 < 0 || depthList[num7] > depthList[item4])
                        {
                            continue;
                        }
                        int num8 = item5;
                        while (list[num8] >= 0)
                        {
                            num8 = list[num8];
                            if (num8 == num4)
                            {
                                break;
                            }
                        }
                        if (num8 != num4)
                        {
                            Vector3 vector2 = wposList[item5];
                            Vector3 normalized2 = (vector2 - vector).normalized;
                            float num9 = Vector3.Dot(normalized, normalized2);
                            if (num9 > num6)
                            {
                                num6 = num9;
                                value = item5;
                                num5 = num7;
                            }
                        }
                    }
                    if (num5 >= 0)
                    {
                        list[num4] = value;
                    }
                }
            }
            List<int> list5 = new List<int>();
            foreach (int useVertex in useVertexList)
            {
                list5.Add(useVertexList.IndexOf(list[useVertex]));
            }
            return list5;
        }

        private List<int> GetUseRootVertexList(List<int> parentVertexList)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < useVertexList.Count; i++)
            {
                int item = -1;
                for (int num = parentVertexList[i]; num >= 0; num = parentVertexList[num])
                {
                    item = num;
                }
                list.Add(item);
            }
            return list;
        }

        private List<VertexInfo> GetUseVertexInfoList(List<int> parentVertexList)
        {
            List<VertexInfo> list = new List<VertexInfo>();
            for (int i = 0; i < useVertexList.Count; i++)
            {
                VertexInfo vertexInfo = new VertexInfo();
                vertexInfo.vertexIndex = i;
                list.Add(vertexInfo);
            }
            for (int j = 0; j < useVertexList.Count; j++)
            {
                int num = parentVertexList[j];
                if (num >= 0)
                {
                    VertexInfo vertexInfo2 = list[j];
                    VertexInfo vertexInfo3 = list[num];
                    vertexInfo2.parentVertexIndex = num;
                    vertexInfo2.parentInfo = vertexInfo3;
                    vertexInfo3.childVertexList.Add(j);
                    vertexInfo3.childInfoList.Add(vertexInfo2);
                }
            }
            return list;
        }

        private List<List<int>> GetUseRootLineList(List<int> parentVertexList)
        {
            List<VertexInfo> useVertexInfoList = GetUseVertexInfoList(parentVertexList);
            List<List<int>> list = new List<List<int>>();
            foreach (VertexInfo item in useVertexInfoList)
            {
                if (item.parentVertexIndex >= 0)
                {
                    continue;
                }
                for (int i = 0; i < item.childVertexList.Count; i++)
                {
                    List<int> list2 = new List<int>();
                    Queue<VertexInfo> queue = new Queue<VertexInfo>();
                    list2.Add(item.vertexIndex);
                    queue.Enqueue(useVertexInfoList[item.childVertexList[i]]);
                    while (queue.Count > 0)
                    {
                        VertexInfo vertexInfo = queue.Dequeue();
                        list2.Add(vertexInfo.vertexIndex);
                        foreach (int childVertex in vertexInfo.childVertexList)
                        {
                            queue.Enqueue(useVertexInfoList[childVertex]);
                        }
                    }
                    list.Add(list2);
                }
            }
            return list;
        }
    }
}
