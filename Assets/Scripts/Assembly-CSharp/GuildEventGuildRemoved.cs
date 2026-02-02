public class GuildEventGuildRemoved : GuildEventDataBase
{
	public GuildEventGuildRemoved()
	{
		OpCode = SocketGuildEventType.GuildRemoved;
	}
}
