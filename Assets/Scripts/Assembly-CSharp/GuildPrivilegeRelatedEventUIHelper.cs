using UnityEngine;
using enums;

public class GuildPrivilegeRelatedEventUIHelper : OrangePartialUIHelperBase
{
	[SerializeField]
	private GuildHeaderPower _headerPowerFilter;

	protected virtual void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent += OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent += OnSocketHeaderPowerChangedEvent;
	}

	protected virtual void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnSocketMemberPrivilegeChangedEvent -= OnSocketMemberPrivilegeChangedEvent;
		Singleton<GuildSystem>.Instance.OnSocketHeaderPowerChangedEvent -= OnSocketHeaderPowerChangedEvent;
	}

	protected virtual void OnSocketMemberPrivilegeChangedEvent(bool isSelfPrivilegeChanged)
	{
		if (isSelfPrivilegeChanged)
		{
			CheckHeaderPower();
		}
	}

	protected virtual void OnSocketHeaderPowerChangedEvent()
	{
		CheckHeaderPower();
	}

	private void CheckHeaderPower()
	{
		if (_headerPowerFilter.HasFlag(GuildHeaderPower.Invite))
		{
			if (!Singleton<GuildSystem>.Instance.CheckHasInvitePower())
			{
				_mainUI.OnClickCloseBtn();
			}
		}
		else
		{
			_mainUI.OnClickCloseBtn();
		}
	}
}
