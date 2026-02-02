using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageLib
{
	[Serializable]
	public class StageObjGroupData
	{
		private class StageRednerData
		{
			public Renderer tRender;

			public Bounds tBounds;

			public Vector3 tPos;
		}

		public float fClipMinx;

		public float fClipMaxx;

		public bool bInRun;

		public List<GameObject> Datas = new List<GameObject>();

		[NonSerialized]
		public List<StageEnemy> EnemyDatas = new List<StageEnemy>();

		[NonSerialized]
		public List<List<StageSceneObjParam>> DataParams;

		public void CheckDataParams()
		{
			if (DataParams != null)
			{
				return;
			}
			DataParams = new List<List<StageSceneObjParam>>();
			for (int i = 0; i < Datas.Count; i++)
			{
				StageSceneObjParam[] componentsInChildren = Datas[i].GetComponentsInChildren<StageSceneObjParam>();
				List<StageSceneObjParam> list = new List<StageSceneObjParam>();
				if (componentsInChildren.Length != 0)
				{
					list.AddRange(componentsInChildren);
				}
				for (int num = list.Count - 1; num >= 0; num--)
				{
					if (list[num].IsIgnoreEvent())
					{
						list.RemoveAt(num);
					}
				}
				DataParams.Add(list);
			}
		}

		public void AddDatas(GameObject tGO)
		{
			Datas.Add(tGO);
		}

		public void UpdateRender(Vector3 p)
		{
		}

		public void SetActuveAll(bool bAct)
		{
			for (int i = 0; i < Datas.Count; i++)
			{
				Datas[i].SetActive(bAct);
			}
			for (int j = 0; j < EnemyDatas.Count; j++)
			{
				EnemyDatas[j].gameObject.SetActive(bAct);
			}
		}

		public void SetActuveNoEnemy(bool bAct)
		{
			for (int i = 0; i < Datas.Count; i++)
			{
				Datas[i].SetActive(bAct);
			}
		}
	}
}
