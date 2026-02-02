using System.Collections.Generic;

public class GuildEventPowerTowerRankup : GuildEventDataBase
{
	public int PowerTowerRank;

	public int Score;

	public int Money;

	public List<PowerPillarInfoData> PillarInfoDataList;

	public GuildEventPowerTowerRankup()
	{
		OpCode = SocketGuildEventType.PowerTowerRankup;
	}
}
