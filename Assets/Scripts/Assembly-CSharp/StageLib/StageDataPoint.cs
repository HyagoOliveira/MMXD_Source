namespace StageLib
{
	public class StageDataPoint : StageSLBase
	{
		public int nID;

		public int nType;

		public int nDifficulty;

		public int[] IntDatas = new int[0];

		public override int GetTypeID()
		{
			return 18;
		}

		public override string GetTypeString()
		{
			return StageObjType.DATAPOING_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			typeString += nID;
			typeString = typeString + ":" + nType;
			typeString = typeString + ":" + nDifficulty;
			typeString = typeString + ":" + IntDatas.Length;
			if (IntDatas.Length != 0)
			{
				for (int i = 0; i < IntDatas.Length; i++)
				{
					typeString = typeString + ":" + IntDatas[i];
				}
			}
			return typeString;
		}

		public override void LoadByString(string sLoad)
		{
			string[] array = sLoad.Substring(GetTypeString().Length).Split(':');
			int num = 0;
			nID = int.Parse(array[num++]);
			nType = int.Parse(array[num++]);
			nDifficulty = int.Parse(array[num++]);
			int num2 = int.Parse(array[num++]);
			IntDatas = new int[num2];
			for (int i = 0; i < num2; i++)
			{
				IntDatas[i] = int.Parse(array[num++]);
			}
			if (nDifficulty != 0)
			{
				int gDifficulty = StageUpdate.gDifficulty;
				int nDifficulty2 = nDifficulty;
			}
		}

		public override void SyncNowStatus()
		{
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
		}
	}
}
