using UnityEngine;

public class UIOpenChk : MonoBehaviour
{
	public enum ChkUIEnum
	{
		OPENRANK_SKILL_LVUP = 0,
		OPENRANK_STARUP = 1,
		OPENRANK_WEAPON_LVUP = 2,
		OPENRANK_WEAPON_UPGRADE = 3,
		OPENRANK_DISC = 4,
		OPENRANK_EQUIP = 5,
		OPENRANK_EQUIP_LVUP = 6,
		OPENRANK_RESEARCH = 7,
		OPENRANK_FS = 8,
		OPENRANK_STAGE_CHALLENGE = 9,
		OPENRANK_STAGE_CORP = 10,
		OPENRANK_STAGE_EVENT = 11,
		OPENRANK_PVP = 12,
		OPENRANK_PVP_RANKING = 13,
		OPENRANK_DAILY_MISSION = 14,
		OPENRANK_MONTHLY_ACTIVE = 15,
		OPENRANK_BACKUP = 16,
		OPENRANK_GUILD = 17
	}

	public enum ChkBanEnum
	{
		OPENBAN_PVP = 0,
		OPENBAN_CORP = 1,
		OPENBAN_BOSSRUSH = 2,
		OPENBAN_EVENT = 3,
		OPENBAN_RAIDBOSS = 4,
		OPENBAN_SPEED = 5,
		OEPNBAN_DEEP_RECORD = 6
	}

	public enum OpenStateEnum
	{
		LOCK = 0,
		OPEN = 1
	}

	public static OpenStateEnum GetOpenState(ChkUIEnum chkUI, out int openRank)
	{
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		openRank = GetOpenRank(ref chkUI);
		if (openRank > lV)
		{
			return OpenStateEnum.LOCK;
		}
		return OpenStateEnum.OPEN;
	}

	private static int GetOpenRank(ref ChkUIEnum chkUI)
	{
		switch (chkUI)
		{
		case ChkUIEnum.OPENRANK_SKILL_LVUP:
			return OrangeConst.OPENRANK_SKILL_LVUP;
		case ChkUIEnum.OPENRANK_STARUP:
			return OrangeConst.OPENRANK_STARUP;
		case ChkUIEnum.OPENRANK_WEAPON_LVUP:
			return OrangeConst.OPENRANK_WEAPON_LVUP;
		case ChkUIEnum.OPENRANK_WEAPON_UPGRADE:
			return OrangeConst.OPENRANK_WEAPON_UPGRADE;
		case ChkUIEnum.OPENRANK_DISC:
			return OrangeConst.OPENRANK_DISC;
		case ChkUIEnum.OPENRANK_EQUIP:
			return OrangeConst.OPENRANK_EQUIP;
		case ChkUIEnum.OPENRANK_EQUIP_LVUP:
			return OrangeConst.OPENRANK_EQUIP_LVUP;
		case ChkUIEnum.OPENRANK_RESEARCH:
			return OrangeConst.OPENRANK_RESEARCH;
		case ChkUIEnum.OPENRANK_FS:
			return OrangeConst.OPENRANK_FS;
		case ChkUIEnum.OPENRANK_STAGE_CHALLENGE:
			return OrangeConst.OPENRANK_STAGE_CHALLENGE;
		case ChkUIEnum.OPENRANK_STAGE_CORP:
			return OrangeConst.OPENRANK_STAGE_CORP;
		case ChkUIEnum.OPENRANK_STAGE_EVENT:
			return OrangeConst.OPENRANK_STAGE_EVENT;
		case ChkUIEnum.OPENRANK_PVP:
			return OrangeConst.OPENRANK_PVP;
		case ChkUIEnum.OPENRANK_PVP_RANKING:
			return OrangeConst.OPENRANK_PVP_RANKING;
		case ChkUIEnum.OPENRANK_DAILY_MISSION:
			return OrangeConst.OPENRANK_DAILY_MISSION;
		case ChkUIEnum.OPENRANK_MONTHLY_ACTIVE:
			return OrangeConst.OPENRANK_MONTHLY_ACTIVE;
		case ChkUIEnum.OPENRANK_BACKUP:
			return OrangeConst.OPENRANK_BACKUP;
		case ChkUIEnum.OPENRANK_GUILD:
			return OrangeConst.OPENRANK_GUILD;
		default:
			return 0;
		}
	}
}
