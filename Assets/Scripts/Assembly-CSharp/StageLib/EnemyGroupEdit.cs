using System.Collections.Generic;
using UnityEngine;

namespace StageLib
{
	public class EnemyGroupEdit : MonoBehaviour
	{
		public List<EnemyEventPoint.EnemySpawnData> ptSpawnDatas = new List<EnemyEventPoint.EnemySpawnData>();

		private int[] _nStageCustomParams = new int[3];

		public float fTime { get; set; }

		public int nID { get; set; }

		public int nDeadID { get; set; }

		public int nPosID { get; set; }

		public int nPatrolID { get; set; }

		public int nEventCtrlID { get; set; }

		public int nSummonEventID { get; set; }

		public int nStageCustomType { get; set; }

		public int[] nStageCustomParams
		{
			get
			{
				return _nStageCustomParams;
			}
			set
			{
				_nStageCustomParams = value;
			}
		}

		public float nIntParam { get; set; }

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void AddSpawnData(EnemyEventPoint.EnemySpawnData tAdd)
		{
			ptSpawnDatas.Add(tAdd);
		}

		public void CleanSpawnData()
		{
			ptSpawnDatas.Clear();
		}

		public List<EnemyEventPoint.EnemySpawnData> GetSpawnDatas()
		{
			return ptSpawnDatas;
		}
	}
}
