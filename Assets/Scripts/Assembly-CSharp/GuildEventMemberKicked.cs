public class GuildEventMemberKicked : GuildEventDataBase
{
	public string MemberId;

	public GuildEventMemberKicked()
	{
		OpCode = SocketGuildEventType.MemberKicked;
	}
}
