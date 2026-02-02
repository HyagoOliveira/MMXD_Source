public class GuildMemberRelatedEventUIHelper : OrangePartialUIHelperBase
{
	protected virtual void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent += OnSocketMemberKickedEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent += OnSocketGuildRemovedEvent;
	}

	protected virtual void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent -= OnSocketMemberKickedEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent -= OnSocketGuildRemovedEvent;
	}

	private void OnSocketMemberKickedEvent(string memberId, bool isSelf)
	{
		if (isSelf)
		{
			_mainUI.OnClickCloseBtn();
		}
	}

	private void OnSocketGuildRemovedEvent()
	{
		_mainUI.OnClickCloseBtn();
	}
}
