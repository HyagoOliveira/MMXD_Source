using CodeStage.AntiCheat.ObscuredTypes;

public class PlayerStatus
{
	public ObscuredInt nLV = 0;

	public ObscuredInt nHP = 0;

	public ObscuredInt nATK = 0;

	public ObscuredInt nCRI = 0;

	public ObscuredInt nHIT = 0;

	public ObscuredInt nDEF = 0;

	public ObscuredInt nDOD = 0;

	public ObscuredInt nLuck = 0;

	public ObscuredInt nCriDmgPercent = 0;

	public ObscuredInt nReduceCriPercent = 0;

	public ObscuredInt nBlockDmgPercent = 0;

	public ObscuredInt nBlockPercent = 0;

	public ObscuredInt nReduceBlockPercent = 0;

	public static PlayerStatus operator +(PlayerStatus a, WeaponStatus b)
	{
		PlayerStatus playerStatus = new PlayerStatus();
		playerStatus.nHP = (int)a.nHP + (int)b.nHP;
		playerStatus.nATK = (int)a.nATK + (int)b.nATK;
		playerStatus.nDEF = (int)a.nDEF + (int)b.nDEF;
		playerStatus.nCRI = (int)a.nCRI + (int)b.nCRI;
		playerStatus.nHIT = (int)a.nHIT + (int)b.nHIT;
		playerStatus.nLuck = (int)a.nLuck + (int)b.nLuck;
		playerStatus.nLuck = a.nDOD;
		playerStatus.nLV = a.nLV;
		playerStatus.nCriDmgPercent = (int)a.nCriDmgPercent + (int)b.nCriDmgPercent;
		playerStatus.nReduceCriPercent = (int)a.nReduceCriPercent + (int)b.nReduceCriPercent;
		playerStatus.nBlockDmgPercent = (int)a.nBlockDmgPercent + (int)b.nBlockDmgPercent;
		playerStatus.nBlockPercent = (int)a.nBlockPercent + (int)b.nBlockPercent;
		playerStatus.nReduceBlockPercent = (int)a.nReduceBlockPercent + (int)b.nReduceBlockPercent;
		return playerStatus;
	}

	public static PlayerStatus operator +(PlayerStatus a, PlayerStatus b)
	{
		PlayerStatus playerStatus = new PlayerStatus();
		playerStatus.nHP = (int)a.nHP + (int)b.nHP;
		playerStatus.nATK = (int)a.nATK + (int)b.nATK;
		playerStatus.nDEF = (int)a.nDEF + (int)b.nDEF;
		playerStatus.nCRI = (int)a.nCRI + (int)b.nCRI;
		playerStatus.nHIT = (int)a.nHIT + (int)b.nHIT;
		playerStatus.nLuck = (int)a.nLuck + (int)b.nLuck;
		playerStatus.nLuck = (int)a.nDOD + (int)b.nDOD;
		playerStatus.nLV = a.nLV;
		playerStatus.nCriDmgPercent = (int)a.nCriDmgPercent + (int)b.nCriDmgPercent;
		playerStatus.nReduceCriPercent = (int)a.nReduceCriPercent + (int)b.nReduceCriPercent;
		playerStatus.nBlockDmgPercent = (int)a.nBlockDmgPercent + (int)b.nBlockDmgPercent;
		playerStatus.nBlockPercent = (int)a.nBlockPercent + (int)b.nBlockPercent;
		playerStatus.nReduceBlockPercent = (int)a.nReduceBlockPercent + (int)b.nReduceBlockPercent;
		return playerStatus;
	}
}
