using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageEndPoint : StageSLBase
	{
		private void Start()
		{
		}

		private void Update()
		{
		}

		public override int GetTypeID()
		{
			return 2;
		}

		public override string GetTypeString()
		{
			return StageObjType.END_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			return GetTypeString();
		}

		public override void LoadByString(string sLoad)
		{
		}

		public override void SyncNowStatus()
		{
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
		}
	}
}
