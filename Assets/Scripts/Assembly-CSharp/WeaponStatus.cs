using CodeStage.AntiCheat.ObscuredTypes;

public class WeaponStatus
{
	public enum WEAPON_CHECK_ENUM
	{
		WEAPON_CHECK_NONE = 0,
		WEAPON_CHECK_SKILL1 = 1,
		WEAPON_CHECK_SKILL2 = 2,
		WEAPON_CHECK_MAIN = 4,
		WEAPON_CHECK_SUB = 8,
		WEAPON_CHECK_FSKILL1 = 16,
		WEAPON_CHECK_FSKILL2 = 32,
		WEAPON_CHECK_ALLADD = 64,
		WEAPON_CHECK_RIDEARMOR = 128,
		WEAPON_CHECK_ALL = 16777215
	}

	public ObscuredInt nHP = 0;

	public ObscuredInt nATK = 0;

	public ObscuredInt nDEF = 0;

	public ObscuredInt nCRI = 0;

	public ObscuredInt nHIT = 0;

	public ObscuredInt nLuck = 0;

	public ObscuredInt nCriDmgPercent = 0;

	public ObscuredInt nReduceCriPercent = 0;

	public ObscuredInt nBlockDmgPercent = 0;

	public ObscuredInt nBlockPercent = 0;

	public ObscuredInt nReduceBlockPercent = 0;

	public ObscuredInt nBattlePower = 0;

	public ObscuredInt nWeaponCheck = 0;

	public ObscuredInt nWeaponType = 0;

	public void CopyWeaponStatus(WeaponStatus tWeaponStatus, int tWeaponCheck, int tWeaponType = 0)
	{
		nHP = tWeaponStatus.nHP;
		nATK = tWeaponStatus.nATK;
		nCRI = tWeaponStatus.nCRI;
		nHIT = tWeaponStatus.nHIT;
		nLuck = tWeaponStatus.nLuck;
		nBattlePower = tWeaponStatus.nBattlePower;
		nCriDmgPercent = tWeaponStatus.nCriDmgPercent;
		nReduceCriPercent = tWeaponStatus.nReduceCriPercent;
		nBlockDmgPercent = tWeaponStatus.nBlockDmgPercent;
		nBlockPercent = tWeaponStatus.nBlockPercent;
		nReduceBlockPercent = tWeaponStatus.nReduceBlockPercent;
		nWeaponCheck = tWeaponCheck;
		nWeaponType = tWeaponType;
	}

	public static WeaponStatus operator +(WeaponStatus a, WeaponStatus b)
	{
		return new WeaponStatus
		{
			nHP = (int)a.nHP + (int)b.nHP,
			nATK = (int)a.nATK + (int)b.nATK,
			nDEF = (int)a.nDEF + (int)b.nDEF,
			nCRI = (int)a.nCRI + (int)b.nCRI,
			nHIT = (int)a.nHIT + (int)b.nHIT,
			nLuck = (int)a.nLuck + (int)b.nLuck,
			nBattlePower = (int)a.nBattlePower + (int)b.nBattlePower,
			nCriDmgPercent = (int)a.nCriDmgPercent + (int)b.nCriDmgPercent,
			nReduceCriPercent = (int)a.nReduceCriPercent + (int)b.nReduceCriPercent,
			nBlockDmgPercent = (int)a.nBlockDmgPercent + (int)b.nBlockDmgPercent,
			nBlockPercent = (int)a.nBlockPercent + (int)b.nBlockPercent,
			nReduceBlockPercent = (int)a.nReduceBlockPercent + (int)b.nReduceBlockPercent,
			nWeaponCheck = ((int)a.nWeaponCheck | (int)b.nWeaponCheck),
			nWeaponType = ((int)a.nWeaponType | (int)b.nWeaponType)
		};
	}
}
