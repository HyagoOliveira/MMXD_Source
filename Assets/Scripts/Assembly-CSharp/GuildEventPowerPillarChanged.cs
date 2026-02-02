using System.Collections.Generic;

public class GuildEventPowerPillarChanged : GuildEventDataBase
{
	public int Money;

	public List<PowerPillarInfoData> PillarInfoDataList;

	public GuildEventPowerPillarChanged()
	{
		OpCode = SocketGuildEventType.PowerPillarChanged;
	}
}
