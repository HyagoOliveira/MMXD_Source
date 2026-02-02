public class GuildEventHeaderPowerChanged : GuildEventDataBase
{
	public int NewHeaderPower;

	public GuildEventHeaderPowerChanged()
	{
		OpCode = SocketGuildEventType.HeaderPowerChanged;
	}
}
