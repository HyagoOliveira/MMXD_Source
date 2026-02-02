using System.Collections.Generic;

public class WeaponInfo
{
	public NetWeaponInfo netInfo;

	public List<NetWeaponExpertInfo> netExpertInfos;

	public List<NetWeaponSkillInfo> netSkillInfos;

	public NetWeaponDiVESkillInfo netDiveSkillInfo;

	public void AddNetWeaponExpertInfo(NetWeaponExpertInfo info)
	{
		if (netInfo == null)
		{
			return;
		}
		if (netExpertInfos == null)
		{
			netExpertInfos = new List<NetWeaponExpertInfo>();
		}
		if (netInfo.WeaponID != info.WeaponID)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < netExpertInfos.Count; i++)
		{
			if (netExpertInfos[i].ExpertType == info.ExpertType)
			{
				netExpertInfos[i].ExpertLevel = info.ExpertLevel;
				flag = false;
				break;
			}
		}
		if (flag)
		{
			netExpertInfos.Add(info);
		}
	}

	public void AddNetWeaponSkillInfo(NetWeaponSkillInfo info)
	{
		if (netSkillInfos == null)
		{
			netSkillInfos = new List<NetWeaponSkillInfo>();
		}
		bool flag = true;
		for (int i = 0; i < netSkillInfos.Count; i++)
		{
			if (netSkillInfos[i].Slot == info.Slot)
			{
				netSkillInfos[i].Level = info.Level;
				flag = false;
				break;
			}
		}
		if (flag)
		{
			netSkillInfos.Add(info);
		}
	}
}
