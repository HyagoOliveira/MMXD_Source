using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageLib
{
	[Serializable]
	public class PosShowSet
	{
		public List<GameObject> ShowGameObjs = new List<GameObject>();

		private List<StageSceneObjParam>[] ArrayLinkStageObjs;

		public int Count
		{
			get
			{
				return ShowGameObjs.Count;
			}
		}

		public void Clear()
		{
			ShowGameObjs.Clear();
		}

		public Vector3 GetPosByIndex(int nIndex)
		{
			return ShowGameObjs[nIndex].transform.position;
		}

		public void CheckPosIsInSceneObjB2D(int nIndex, ref List<StageSceneObjParam> tOutList, Vector3? tPos = null)
		{
			if (ArrayLinkStageObjs == null)
			{
				ArrayLinkStageObjs = new List<StageSceneObjParam>[ShowGameObjs.Count];
			}
			if (ArrayLinkStageObjs[nIndex] != null)
			{
				tOutList.AddRange(ArrayLinkStageObjs[nIndex]);
				return;
			}
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (!(stageUpdate == null))
			{
				ArrayLinkStageObjs[nIndex] = new List<StageSceneObjParam>();
				stageUpdate.GetStageSceneObjContainPoint(tPos ?? ShowGameObjs[nIndex].transform.position, ref ArrayLinkStageObjs[nIndex]);
				tOutList.AddRange(ArrayLinkStageObjs[nIndex]);
			}
		}

		public GameObject GetObjByIndex(int nIndex)
		{
			return ShowGameObjs[nIndex];
		}

		public void RemoveAt(int nIndex)
		{
			if (nIndex >= 0 && nIndex < ShowGameObjs.Count)
			{
				UnityEngine.Object.Destroy(ShowGameObjs[nIndex]);
				ShowGameObjs.RemoveAt(nIndex);
			}
		}

		public void CheckSetByGameBoj(GameObject tRoot)
		{
		}

		public void AddShowObj(GameObject tRoot, string name, Color showcolor, Vector3? setPos = null)
		{
			if (!(tRoot == null))
			{
				Transform transform = tRoot.transform;
				GameObject gameObject = new GameObject();
				gameObject.transform.parent = transform;
				if (!setPos.HasValue)
				{
					gameObject.transform.localPosition = Vector3.zero;
				}
				else
				{
					gameObject.transform.position = setPos ?? Vector3.zero;
				}
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localRotation = Quaternion.identity;
				ShowGameObjs.Add(gameObject);
				gameObject.name = name;
			}
		}

		private float DistanceNoSqrt(Vector3 a, Vector3 b)
		{
			Vector2 vector = new Vector2(a.x - b.x, a.y - b.y);
			return Vector2.Dot(vector, vector);
		}

		public Vector3 GetCloseReturnPos(Vector3 vPlayerPos, Vector3 DefaultPos)
		{
			if (Count == 0)
			{
				return DefaultPos;
			}
			float num = 0f;
			float num2 = 0f;
			int nIndex = 0;
			num = DistanceNoSqrt(GetPosByIndex(0), vPlayerPos);
			for (int i = 1; i < Count; i++)
			{
				num2 = DistanceNoSqrt(GetPosByIndex(i), vPlayerPos);
				if (num > num2)
				{
					num = num2;
					nIndex = i;
				}
			}
			return GetPosByIndex(nIndex);
		}
	}
}
