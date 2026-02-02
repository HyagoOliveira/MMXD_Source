using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
	public class ReductionData : ReductionMeshAccess
	{
		public void ReductionZeroDistance(float radius = 0.0001f)
		{
			NearPointReduction nearPointReduction = new NearPointReduction(radius);
			nearPointReduction.Create(base.MeshData);
			nearPointReduction.Reduction();
		}

		public void ReductionRadius(float radius)
		{
			NearPointReduction nearPointReduction = new NearPointReduction(radius);
			nearPointReduction.Create(base.MeshData);
			nearPointReduction.Reduction();
		}

		public void ReductionPolygonLink(float length)
		{
			PolygonLinkReduction polygonLinkReduction = new PolygonLinkReduction(length);
			polygonLinkReduction.Create(base.MeshData);
			polygonLinkReduction.Reduction();
		}

		public void ReductionBone()
		{
			HashSet<Transform> hashSet = new HashSet<Transform>();
			foreach (MeshData.ShareVertex shareVertex in base.MeshData.shareVertexList)
			{
				for (int i = 0; i < shareVertex.boneWeightList.Count; i++)
				{
					MeshData.WeightData weightData = shareVertex.boneWeightList[i];
					if (weightData.boneWeight > 0f)
					{
						hashSet.Add(base.MeshData.boneList[weightData.boneIndex]);
					}
				}
			}
			List<Transform> list = new List<Transform>(hashSet);
			foreach (MeshData.ShareVertex shareVertex2 in base.MeshData.shareVertexList)
			{
				for (int j = 0; j < shareVertex2.boneWeightList.Count; j++)
				{
					MeshData.WeightData weightData2 = shareVertex2.boneWeightList[j];
					if (weightData2.boneWeight > 0f)
					{
						weightData2.boneIndex = list.IndexOf(base.MeshData.boneList[weightData2.boneIndex]);
					}
				}
			}
			base.MeshData.boneList = list;
		}
	}
}
