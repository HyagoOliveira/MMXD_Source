public class MemberInfo
{
	public string PlayerId;

	public string Nickname;

	public NetSealBattleSettingInfo netSealBattleSettingInfo;

	public sbyte Team;

	public int nScore;

	public int nALLDMG;

	public int nLifePercent;

	public bool bInGame = true;

	public bool bInPause;

	public bool bPrepared;

	public int nKillNum;

	public int nKillEnemyNum;

	public int nNowCharacterID;

	public float fLastCheckConnectTime;

	public bool bLoadEnd;

	public MemberInfo(string Playerid, string NickName, sbyte Team, NetSealBattleSettingInfo netSealBattleSettingInfo)
	{
		PlayerId = Playerid;
		Nickname = NickName;
		this.Team = Team;
		this.netSealBattleSettingInfo = netSealBattleSettingInfo;
	}
}
