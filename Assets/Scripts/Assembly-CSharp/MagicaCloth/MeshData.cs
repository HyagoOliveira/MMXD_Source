using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    [Serializable]
    public class MeshData : ShareDataObject
    {
        [Serializable]
        public struct VertexWeight
        {
            public Vector3 localPos;

            public Vector3 localNor;

            public Vector3 localTan;

            public int parentIndex;

            public float weight;
        }

        [Serializable]
        public class ChildData : IDataHash
        {
            public int childDataHash;

            public int vertexCount;

            public uint[] vertexInfoList;

            public VertexWeight[] vertexWeightList;

            public int[] parentIndexList;

            public int VertexCount
            {
                get
                {
                    return vertexCount;
                }
            }

            public int GetDataHash()
            {
                return 0 + childDataHash + vertexCount.GetDataHash();
            }
        }

        private const int DATA_VERSION = 2;

        public bool isSkinning;

        public int vertexCount;

        public uint[] vertexInfoList;

        public VertexWeight[] vertexWeightList;

        public ulong[] vertexHashList;

        public Vector2[] uvList;

        public int lineCount;

        public int[] lineList;

        public int triangleCount;

        public int[] triangleList;

        public int boneCount;

        public uint[] vertexToTriangleInfoList;

        public int[] vertexToTriangleIndexList;

        public List<ChildData> childDataList = new List<ChildData>();

        public Vector3 baseScale;

        public int VertexCount
        {
            get
            {
                return vertexCount;
            }
        }

        public int VertexHashCount
        {
            get
            {
                if (vertexHashList != null)
                {
                    return vertexHashList.Length;
                }
                return 0;
            }
        }

        public int WeightCount
        {
            get
            {
                if (vertexWeightList != null)
                {
                    return vertexWeightList.Length;
                }
                return 0;
            }
        }

        public int LineCount
        {
            get
            {
                return lineCount;
            }
        }

        public int TriangleCount
        {
            get
            {
                return triangleCount;
            }
        }

        public int BoneCount
        {
            get
            {
                return boneCount;
            }
        }

        public int ChildCount
        {
            get
            {
                return childDataList.Count;
            }
        }

        public override int GetDataHash()
        {
            int num = 0;
            num += isSkinning.GetDataHash();
            num += vertexCount.GetDataHash();
            num += lineCount.GetDataHash();
            num += triangleCount.GetDataHash();
            num += boneCount.GetDataHash();
            num += ChildCount.GetDataHash();
            num += vertexInfoList.GetDataCountHash();
            num += vertexWeightList.GetDataCountHash();
            num += uvList.GetDataCountHash();
            num += lineList.GetDataCountHash();
            num += triangleList.GetDataCountHash();
            num += childDataList.GetDataHash();
            if (vertexHashList != null && vertexHashList.Length != 0)
            {
                num += vertexHashList.GetDataCountHash();
            }
            return num;
        }

        public override int GetVersion()
        {
            return 2;
        }

        public override Define.Error VerifyData()
        {
            if (dataHash == 0)
            {
                return Define.Error.InvalidDataHash;
            }
            if (vertexCount == 0)
            {
                return Define.Error.VertexCountZero;
            }
            return Define.Error.None;
        }

        public Dictionary<int, List<uint>> GetVirtualToChildVertexDict()
        {
            Dictionary<int, List<uint>> dictionary = new Dictionary<int, List<uint>>();
            for (int i = 0; i < VertexCount; i++)
            {
                dictionary.Add(i, new List<uint>());
            }
            for (int j = 0; j < childDataList.Count; j++)
            {
                ChildData childData = childDataList[j];
                for (int k = 0; k < childData.VertexCount; k++)
                {
                    if (k < childData.parentIndexList.Length)
                    {
                        int key = childData.parentIndexList[k];
                        dictionary[key].Add(DataUtility.Pack16(j, k));
                    }
                }
            }
            return dictionary;
        }

        public List<int> ExtendSelection(List<int> originalSelection, bool extendNext, bool extendWeight)
        {
            List<int> list = new List<int>(originalSelection);
            if (extendNext)
            {
                List<HashSet<int>> triangleToVertexLinkList = MeshUtility.GetTriangleToVertexLinkList(vertexCount, new List<int>(lineList), new List<int>(triangleList));
                new List<int>();
                for (int i = 0; i < vertexCount; i++)
                {
                    if (list[i] != 0)
                    {
                        continue;
                    }
                    foreach (int item in triangleToVertexLinkList[i])
                    {
                        if (list[item] == 1 || list[item] == 2)
                        {
                            list[i] = 3;
                        }
                    }
                }
            }
            if (extendWeight)
            {
                HashSet<int> hashSet = new HashSet<int>();
                foreach (ChildData childData in childDataList)
                {
                    for (int j = 0; j < childData.VertexCount; j++)
                    {
                        uint pack = childData.vertexInfoList[j];
                        int num = DataUtility.Unpack4_28Hi(pack);
                        int num2 = DataUtility.Unpack4_28Low(pack);
                        bool flag = false;
                        for (int k = 0; k < num; k++)
                        {
                            int num3 = num2 + k;
                            VertexWeight vertexWeight = childData.vertexWeightList[num3];
                            if (vertexWeight.weight > 0f && (list[vertexWeight.parentIndex] == 1 || list[vertexWeight.parentIndex] == 2))
                            {
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            continue;
                        }
                        for (int l = 0; l < num; l++)
                        {
                            int num4 = num2 + l;
                            VertexWeight vertexWeight2 = childData.vertexWeightList[num4];
                            if (vertexWeight2.weight > 0f && list[vertexWeight2.parentIndex] == 0)
                            {
                                hashSet.Add(vertexWeight2.parentIndex);
                            }
                        }
                    }
                }
                foreach (int item2 in hashSet)
                {
                    list[item2] = 3;
                }
            }
            return list;
        }
    }
}
