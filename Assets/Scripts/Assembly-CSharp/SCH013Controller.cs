#define RELEASE
using CallbackDefs;
using Newtonsoft.Json;

public class SCH013Controller : SCH006Controller
{
	public override string[] GetPetDependAnimations()
	{
		return null;
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		if (!Activate || CheckIsLocalPlayer())
		{
			return;
		}
		receiveSyncData.Clear();
		receiveSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
		Controller.LogicPosition.x = receiveSyncData.SelfPosX;
		Controller.LogicPosition.y = receiveSyncData.SelfPosY;
		Controller.LogicPosition.z = receiveSyncData.SelfPosZ;
		m_targetPos.x = receiveSyncData.TargetPosX;
		m_targetPos.y = receiveSyncData.TargetPosY;
		m_targetPos.z = receiveSyncData.TargetPosZ;
		m_bulletSkillId = receiveSyncData.nParam0;
		if (m_bulletSkillTable == null && !ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(m_bulletSkillId, out m_bulletSkillTable))
		{
			Debug.Log("Error retrieving skill table");
		}
		switch (nSet)
		{
		default:
			SetStatus((MainStatus)nSet);
			break;
		case 1:
			SetPositionAndRotation(Controller.LogicPosition.vec3, false);
			break;
		case 2:
			if (receiveSyncData.sParam0 != "")
			{
				_autoAim.SetTargetByNetSerialID(receiveSyncData.sParam0);
				_autoAim.SetUpdate(false);
			}
			SetStatus((MainStatus)nSet);
			if (base.SyncBulletIdx == -1)
			{
				m_bulletSkillId = receiveSyncData.nParam0;
				for (int i = 0; i < listBulletSkillTable.Count; i++)
				{
					if (listBulletSkillTable[i].n_ID == m_bulletSkillId)
					{
						base.SyncBulletIdx = i;
						break;
					}
				}
				useRandomSkl = true;
				m_bulletSkillTable = null;
				RandCurrentSkill();
			}
			Sync_Status_Ready();
			break;
		}
	}
}
