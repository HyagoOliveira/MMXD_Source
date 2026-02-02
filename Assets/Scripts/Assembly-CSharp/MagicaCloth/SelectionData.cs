using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    [Serializable]
    public class SelectionData : ShareDataObject
    {
        [Serializable]
        public class DeformerSelection : IDataHash
        {
            public List<int> selectData = new List<int>();

            public List<ulong> vertexHashList = new List<ulong>();

            public int GetDataHash()
            {
                return selectData.GetDataHash();
            }

            public bool Compare(DeformerSelection data)
            {
                if (selectData.Count != data.selectData.Count)
                {
                    return false;
                }
                for (int i = 0; i < selectData.Count; i++)
                {
                    if (selectData[i] != data.selectData[i])
                    {
                        return false;
                    }
                }
                if (vertexHashList.Count != data.vertexHashList.Count)
                {
                    return false;
                }
                for (int j = 0; j < vertexHashList.Count; j++)
                {
                    if (vertexHashList[j] != data.vertexHashList[j])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private const int DATA_VERSION = 2;

        public const int Invalid = 0;

        public const int Move = 1;

        public const int Fixed = 2;

        public const int Extend = 3;

        public List<DeformerSelection> selectionList = new List<DeformerSelection>();

        public int DeformerCount
        {
            get
            {
                return selectionList.Count;
            }
        }

        public override int GetDataHash()
        {
            return 0 + selectionList.GetDataHash();
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
            if (selectionList.Count == 0)
            {
                return Define.Error.SelectionCountZero;
            }
            return Define.Error.None;
        }

        public bool Compare(SelectionData sel)
        {
            if (selectionList.Count != sel.selectionList.Count)
            {
                return false;
            }
            for (int i = 0; i < selectionList.Count; i++)
            {
                if (!selectionList[i].Compare(sel.selectionList[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public List<int> GetSelectionData(MeshData meshData, List<MeshData> childMeshDataList)
        {
            List<int> list = new List<int>();
            if (meshData != null)
            {
                Dictionary<int, List<uint>> virtualToChildVertexDict = meshData.GetVirtualToChildVertexDict();
                List<Dictionary<ulong, int>> selectionVertexHashList = GetSelectionVertexHashList();
                int vertexCount = meshData.VertexCount;
                for (int i = 0; i < vertexCount; i++)
                {
                    int selection = GetSelection(meshData, i, virtualToChildVertexDict, childMeshDataList, selectionVertexHashList);
                    list.Add(selection);
                }
            }
            else if (selectionList.Count > 0)
            {
                list = new List<int>(selectionList[0].selectData);
            }
            return list;
        }

        private int GetSelection(MeshData meshData, int vindex, Dictionary<int, List<uint>> dict, List<MeshData> childMeshDataList, List<Dictionary<ulong, int>> hashList)
        {
            int num = 0;
            if (meshData != null && meshData.ChildCount > 0)
            {
                if (dict.ContainsKey(vindex))
                {
                    foreach (uint item in dict[vindex])
                    {
                        int num2 = DataUtility.Unpack16Hi(item);
                        int num3 = DataUtility.Unpack16Low(item);
                        if (num2 >= selectionList.Count || num3 >= selectionList[num2].selectData.Count)
                        {
                            continue;
                        }
                        ulong num4 = 0uL;
                        if (childMeshDataList != null && num2 < childMeshDataList.Count)
                        {
                            MeshData meshData2 = childMeshDataList[num2];
                            if (meshData2 != null && num3 < meshData2.VertexHashCount)
                            {
                                num4 = meshData2.vertexHashList[num3];
                            }
                        }
                        if (num4 != 0L && num2 < hashList.Count && hashList[num2].ContainsKey(num4))
                        {
                            num3 = hashList[num2][num4];
                        }
                        num = Mathf.Max(selectionList[num2].selectData[num3], num);
                    }
                }
            }
            else
            {
                int num5 = 0;
                if (num5 < selectionList.Count && vindex < selectionList[num5].selectData.Count)
                {
                    num = selectionList[num5].selectData[vindex];
                }
            }
            return num;
        }

        public void SetSelectionData(MeshData meshData, List<int> selects, List<MeshData> childMeshDataList)
        {
            selectionList.Clear();
            if (meshData != null && meshData.ChildCount > 0)
            {
                for (int i = 0; i < meshData.ChildCount; i++)
                {
                    DeformerSelection deformerSelection = new DeformerSelection();
                    int vertexCount = meshData.childDataList[i].VertexCount;
                    for (int j = 0; j < vertexCount; j++)
                    {
                        deformerSelection.selectData.Add(0);
                        deformerSelection.vertexHashList.Add(0uL);
                    }
                    selectionList.Add(deformerSelection);
                }
            }
            else
            {
                DeformerSelection deformerSelection2 = new DeformerSelection();
                int count = selects.Count;
                for (int k = 0; k < count; k++)
                {
                    deformerSelection2.selectData.Add(0);
                    deformerSelection2.vertexHashList.Add(0uL);
                }
                selectionList.Add(deformerSelection2);
            }
            for (int l = 0; l < selects.Count; l++)
            {
                int value = selects[l];
                if (meshData != null && meshData.ChildCount > 0)
                {
                    Dictionary<int, List<uint>> virtualToChildVertexDict = meshData.GetVirtualToChildVertexDict();
                    if (!virtualToChildVertexDict.ContainsKey(l))
                    {
                        continue;
                    }
                    foreach (uint item in virtualToChildVertexDict[l])
                    {
                        int num = DataUtility.Unpack16Hi(item);
                        int num2 = DataUtility.Unpack16Low(item);
                        selectionList[num].selectData[num2] = value;
                        if (num < childMeshDataList.Count)
                        {
                            MeshData meshData2 = childMeshDataList[num];
                            if (meshData2 != null && num2 < meshData2.VertexHashCount)
                            {
                                selectionList[num].vertexHashList[num2] = meshData2.vertexHashList[num2];
                            }
                        }
                    }
                }
                else
                {
                    selectionList[0].selectData[l] = value;
                }
            }
            CreateVerifyData();
        }

        private List<Dictionary<ulong, int>> GetSelectionVertexHashList()
        {
            List<Dictionary<ulong, int>> list = new List<Dictionary<ulong, int>>();
            foreach (DeformerSelection selection in selectionList)
            {
                Dictionary<ulong, int> dictionary = new Dictionary<ulong, int>();
                for (int i = 0; i < selection.vertexHashList.Count; i++)
                {
                    dictionary[selection.vertexHashList[i]] = i;
                }
                list.Add(dictionary);
            }
            return list;
        }
    }
}
