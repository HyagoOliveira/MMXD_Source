public class GuildEventMemberJoined : GuildEventDataBase
{
	public int GuildId;

	public GuildEventMemberJoined()
	{
		OpCode = SocketGuildEventType.MemberJoined;
	}
}
