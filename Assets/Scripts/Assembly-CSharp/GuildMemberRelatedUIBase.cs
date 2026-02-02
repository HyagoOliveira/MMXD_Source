public abstract class GuildMemberRelatedUIBase : GuildUIBase
{
	protected override void OnEnable()
	{
		base.OnEnable();
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent += OnSocketMemberKickedEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent += OnSocketGuildRemovedEvent;
	}

	protected override void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent -= OnSocketMemberKickedEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent -= OnSocketGuildRemovedEvent;
		base.OnDisable();
	}

	private void OnSocketMemberKickedEvent(string memberId, bool isSelf)
	{
		if (isSelf)
		{
			OnClickCloseBtn();
		}
	}

	private void OnSocketGuildRemovedEvent()
	{
		OnClickCloseBtn();
	}
}
