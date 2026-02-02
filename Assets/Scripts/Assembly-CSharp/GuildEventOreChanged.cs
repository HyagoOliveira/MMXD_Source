using System.Collections.Generic;

public class GuildEventOreChanged : GuildEventDataBase
{
	public int Money;

	public List<NetOreInfo> ChangedOreInfoList;

	public GuildEventOreChanged()
	{
		OpCode = SocketGuildEventType.OreChanged;
	}
}
