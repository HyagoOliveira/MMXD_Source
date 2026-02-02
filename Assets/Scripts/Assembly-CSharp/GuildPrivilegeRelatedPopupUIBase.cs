public abstract class GuildPrivilegeRelatedPopupUIBase : GuildMemberRelatedUIBase
{
	protected override void OnEnable()
	{
		base.OnEnable();
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent += OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent += OnSocketHeaderPowerChangedEvent;
	}

	protected override void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent -= OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent -= OnSocketHeaderPowerChangedEvent;
		base.OnDisable();
	}

	protected virtual void OnSocketMemberPrivilegeChangedEvent(bool isSelfPrivilegeChanged)
	{
		if (isSelfPrivilegeChanged)
		{
			OnClickCloseBtn();
		}
	}

	protected virtual void OnSocketHeaderPowerChangedEvent()
	{
		OnClickCloseBtn();
	}
}
