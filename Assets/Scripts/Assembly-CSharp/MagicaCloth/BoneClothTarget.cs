using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public class BoneClothTarget : IDataHash, IBoneReplace
	{
		public enum ConnectionMode
		{
			Line = 0,
			Mesh = 1
		}

		[SerializeField]
		private List<Transform> rootList = new List<Transform>();

		[SerializeField]
		private ConnectionMode connection;

		[SerializeField]
		[Range(10f, 90f)]
		private float sameSurfaceAngle = 80f;

		private int[] parentIndexList;

		public int RootCount
		{
			get
			{
				return rootList.Count;
			}
		}

		public ConnectionMode Connection
		{
			get
			{
				return connection;
			}
		}

		public float SameSurfaceAngle
		{
			get
			{
				return sameSurfaceAngle;
			}
		}

		public int GetDataHash()
		{
			return 0 + rootList.GetDataHash();
		}

		public Transform GetRoot(int index)
		{
			if (index < rootList.Count)
			{
				return rootList[index];
			}
			return null;
		}

		public int GetRootIndex(Transform root)
		{
			return rootList.IndexOf(root);
		}

		public void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
		{
			for (int i = 0; i < rootList.Count; i++)
			{
				rootList[i] = MeshUtility.GetReplaceBone(rootList[i], boneReplaceDict);
			}
		}

		public void AddParentTransform()
		{
			if (rootList.Count <= 0)
			{
				return;
			}
			HashSet<Transform> hashSet = new HashSet<Transform>();
			foreach (Transform root in rootList)
			{
				if ((bool)root && (bool)root.parent)
				{
					hashSet.Add(root.parent);
				}
			}
			parentIndexList = new int[hashSet.Count];
			int num = 0;
			foreach (Transform item in hashSet)
			{
				int num2 = -1;
				if ((bool)item)
				{
					num2 = CreateSingleton<MagicaPhysicsManager>.Instance.Bone.AddBone(item);
				}
				parentIndexList[num] = num2;
				num++;
			}
		}

		public void RemoveParentTransform()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance() && parentIndexList != null && parentIndexList.Length != 0)
			{
				for (int i = 0; i < parentIndexList.Length; i++)
				{
					int num = parentIndexList[i];
					if (num >= 0)
					{
						CreateSingleton<MagicaPhysicsManager>.Instance.Bone.RemoveBone(num);
					}
				}
			}
			parentIndexList = null;
		}

		public void ResetFuturePredictionParentTransform()
		{
			if (parentIndexList == null || parentIndexList.Length == 0)
			{
				return;
			}
			for (int i = 0; i < parentIndexList.Length; i++)
			{
				int num = parentIndexList[i];
				if (num >= 0)
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.Bone.ResetFuturePrediction(num);
				}
			}
		}
	}
}
