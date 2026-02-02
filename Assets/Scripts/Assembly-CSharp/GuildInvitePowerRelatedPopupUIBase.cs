public abstract class GuildInvitePowerRelatedPopupUIBase : GuildPrivilegeRelatedPopupUIBase
{
	protected override void OnSocketMemberPrivilegeChangedEvent(bool isSelfPrivilegeChanged)
	{
		if (isSelfPrivilegeChanged && !Singleton<GuildSystem>.Instance.CheckHasInvitePower())
		{
			OnClickCloseBtn();
		}
	}

	protected override void OnSocketHeaderPowerChangedEvent()
	{
		if (!Singleton<GuildSystem>.Instance.CheckHasInvitePower())
		{
			OnClickCloseBtn();
		}
	}
}
