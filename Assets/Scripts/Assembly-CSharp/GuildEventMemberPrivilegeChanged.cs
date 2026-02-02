public class GuildEventMemberPrivilegeChanged : GuildEventDataBase
{
	public GuildEventMemberPrivilegeChangedInfo[] ChangedPrivilegeInfos;

	public GuildEventMemberPrivilegeChanged()
	{
		OpCode = SocketGuildEventType.MemberPrivilegeChanged;
	}
}
