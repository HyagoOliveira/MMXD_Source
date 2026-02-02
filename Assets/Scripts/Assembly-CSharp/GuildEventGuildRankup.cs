public class GuildEventGuildRankup : GuildEventDataBase
{
	public int GuildRank;

	public int Score;

	public int Money;

	public GuildEventGuildRankup()
	{
		OpCode = SocketGuildEventType.GuildRankup;
	}
}
