using StageLib;
using UnityEngine;

public class SyncBullet : MonoBehaviour
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

	protected bool bWaitNetStatus;

	public string sNetSerialID = "";

	protected BulletBase _pBulletBase;

	public static int nCount;

	protected void Awake()
	{
		_pBulletBase = GetComponent<BulletBase>();
		Singleton<GenericEventManager>.Instance.AttachEvent<string, int, string>(EventManager.ID.STAGE_OBJ_CTRL_BULLET_ACTION, ObjCtrlBulletAction);
		StageResManager.GetStageUpdate().RegisterSyncBullet(this);
	}

	protected void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<string, int, string>(EventManager.ID.STAGE_OBJ_CTRL_BULLET_ACTION, ObjCtrlBulletAction);
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if ((object)stageUpdate != null)
		{
			stageUpdate.UnRegisterSyncBullet(this);
		}
	}

	public static int GetSyncBulletCount()
	{
		return nCount++;
	}

	public void ObjCtrlBulletAction(string sTmpNetID, int nTmpID, string nTmpMsg)
	{
		if (!string.IsNullOrEmpty(sNetSerialID) && sTmpNetID == sNetSerialID)
		{
			SyncStatus(nTmpID, nTmpMsg);
		}
	}

	public virtual void SyncStatus(int nSet, string smsg)
	{
		if ((bool)_pBulletBase)
		{
			_pBulletBase.SyncBulletStatus(nSet, smsg);
		}
	}

	public virtual void SendSyncEvent(int nSetKey, string sMsg)
	{
		StageUpdate.RegisterSyncBulletSendAndRun(sNetSerialID, nSetKey, sMsg, true);
	}
}
