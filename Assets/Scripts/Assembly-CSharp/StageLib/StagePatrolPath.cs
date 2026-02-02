using System.Collections.Generic;
using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StagePatrolPath : StageSLBase
	{
		public class StagePatrolPathSL
		{
			public int _iSetId;

			public bool _isLoop;

			public int _nMoveSpeed;

			public List<Vector3> _PatrolPaths = new List<Vector3>();
		}

		public int _iSetId;

		public bool _isLoop;

		public int _nMoveSpeed;

		public List<Vector3> _PatrolPaths = new List<Vector3>();

		public override int GetTypeID()
		{
			return 17;
		}

		public override string GetTypeString()
		{
			return StageObjType.PATROLPATH_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			string text = JsonUtility.ToJson(new StagePatrolPathSL
			{
				_iSetId = _iSetId,
				_isLoop = _isLoop,
				_nMoveSpeed = _nMoveSpeed,
				_PatrolPaths = _PatrolPaths
			});
			text = text.Replace(",", ";");
			return typeString + text;
		}

		public override void LoadByString(string sLoad)
		{
			StagePatrolPathSL stagePatrolPathSL = JsonUtility.FromJson<StagePatrolPathSL>(sLoad.Substring(GetTypeString().Length).Replace(";", ","));
			_iSetId = stagePatrolPathSL._iSetId;
			_isLoop = stagePatrolPathSL._isLoop;
			_nMoveSpeed = stagePatrolPathSL._nMoveSpeed;
			_PatrolPaths = stagePatrolPathSL._PatrolPaths;
		}

		public override void SyncNowStatus()
		{
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 0f, 0f);
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		private void DrawArrow(Vector3 vPos, Vector3 vDir)
		{
			Vector3 vector = vPos;
			vector += vDir;
			Vector3 to = vector;
			float x = vDir.x;
			vDir.x = vDir.y;
			vDir.y = 0f - x;
			vector += vDir * 0.5f;
			to -= vDir * 0.5f;
			Gizmos.DrawLine(vPos, vector);
			Gizmos.DrawLine(vPos, to);
			Gizmos.DrawLine(vector, to);
		}

		public void AutoSetId()
		{
			int num = 1;
			StagePatrolPath[] array = OrangeSceneManager.FindObjectsOfTypeCustom<StagePatrolPath>();
			for (int i = 0; i < array.Length; i++)
			{
				if (num <= array[i]._iSetId)
				{
					num = array[i]._iSetId + 1;
				}
			}
			_iSetId = num;
		}
	}
}
