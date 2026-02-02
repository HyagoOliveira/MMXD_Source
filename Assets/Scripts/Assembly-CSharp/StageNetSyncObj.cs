using System.Collections.Generic;

public class StageNetSyncObj : StageObjBase
{
	public class NetSyncData
	{
		public int TargetPosX;

		public int TargetPosY;

		public int TargetPosZ;

		public int SelfPosX;

		public int SelfPosY;

		public int SelfPosZ;

		public bool bSetHP;

		public int nProtocol;

		public int nHP;

		public int nParam0;

		public string sParam0;

		public void Clear()
		{
			TargetPosX = 0;
			TargetPosY = 0;
			TargetPosZ = 0;
			SelfPosX = 0;
			SelfPosY = 0;
			SelfPosZ = 0;
			bSetHP = false;
			nProtocol = 0;
			nHP = 0;
			nParam0 = 0;
			sParam0 = string.Empty;
		}
	}

	public class NexStatusSave
	{
		public int MainStatus;

		public NetSyncData tSend;
	}

	protected bool bWaitNetStatus;

	protected bool bCanNextStatus;

	protected List<NexStatusSave> listNexStatusSave = new List<NexStatusSave>();
}
