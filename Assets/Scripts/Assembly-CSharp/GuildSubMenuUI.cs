using System;
using CallbackDefs;
using UnityEngine;
using enums;

public class GuildSubMenuUI : CommonSubMenuUI
{
	[SerializeField]
	protected GameObject _buttonChangeGuildPrivilege;

	[SerializeField]
	protected GameObject _buttonKickMember;

	private GuildPrivilege _selfPrivilege;

	private GuildPrivilege _targetPrivilege;

	private SystemSE eClickSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;

	private CommonUI _lastCommonUI;

	protected override void OnEnable()
	{
		base.OnEnable();
		Singleton<GuildSystem>.Instance.OnKickMemberEvent += OnKickMemberRes;
	}

	protected override void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnKickMemberEvent -= OnKickMemberRes;
		base.OnDisable();
	}

	public void Setup(string playerId, Vector3 tarPos, bool isLobby)
	{
		Setup(playerId, tarPos);
		_buttonPrivateChat.SetActive(false);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (isLobby)
		{
			bool flag = ManagedSingleton<PlayerHelper>.Instance.CheckPlayerIsSelf(playerId);
			GuildHeaderPower guildHeaderPower = (GuildHeaderPower)Singleton<GuildSystem>.Instance.GuildInfoCache.HeaderPower;
			_selfPrivilege = (GuildPrivilege)Singleton<GuildSystem>.Instance.SelfMemberInfo.Privilege;
			NetMemberInfo memberInfo;
			_targetPrivilege = ((!Singleton<GuildSystem>.Instance.TryGetMemberInfo(_playerId, out memberInfo)) ? GuildPrivilege.GuildLeader : ((GuildPrivilege)memberInfo.Privilege));
			_buttonChangeGuildPrivilege.SetActive(!flag && _selfPrivilege < _targetPrivilege);
			_buttonKickMember.SetActive(!flag && _selfPrivilege < _targetPrivilege && (_selfPrivilege == GuildPrivilege.GuildLeader || guildHeaderPower.HasFlag(GuildHeaderPower.KickMember)));
		}
		else
		{
			_buttonChangeGuildPrivilege.SetActive(false);
			_buttonKickMember.SetActive(false);
		}
	}

	public override void OnClickPlayerInfoButton()
	{
		base.OnClickPlayerInfoButton();
	}

	public void OnClickChangeGuildPrivilegeButton()
	{
		PlayUISE(eClickSE);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildChangePrivilegeUI>("UI_GuildChangePrivilege", OnGuildChangePrivilegeUILoaded);
	}

	public void OnClickKickMemberButton()
	{
		if (Singleton<CrusadeSystem>.Instance.CheckInEventTime())
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WARN_OUSTER"));
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnKickConfirmUILoaded);
		}
	}

	private void OnKickConfirmUILoaded(CommonUI ui)
	{
		_lastCommonUI = ui;
		SocketPlayerHUD value;
		string text = (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(_playerId, out value) ? value.m_Name : "---");
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ClearOpenUIRef));
		ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SETUP_KICKCONFIRM", text), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), OnKickMemberConfirm);
	}

	private void OnKickMemberConfirm()
	{
		_lastCommonUI.CloseSE = SystemSE.NONE;
		base.CloseSE = SystemSE.NONE;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		Singleton<GuildSystem>.Instance.ReqKickMember(_playerId);
	}

	private void OnGuildChangePrivilegeUILoaded(GuildChangePrivilegeUI ui)
	{
		GuildPrivilege guildPrivilege = Singleton<GuildSystem>.Instance.GuildSetting.OwnPrivilege;
		foreach (GuildPrivilege item in EnumExt<GuildPrivilege>.ValuesCache)
		{
			if (guildPrivilege.HasFlag(item) && item <= _selfPrivilege)
			{
				guildPrivilege &= (GuildPrivilege)(~(int)item);
			}
		}
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_playerId, guildPrivilege);
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
	}

	private void ClearOpenUIRef()
	{
		_lastCommonUI = null;
	}

	private void OnKickMemberRes(Code ackCode, string playerId)
	{
		OnClickCloseBtn();
	}

	private void CloseCommonUI()
	{
		if (_lastCommonUI != null)
		{
			_lastCommonUI.OnClickCloseBtn();
		}
	}

	public override void OnClickCloseBtn()
	{
		CloseCommonUI();
		base.OnClickCloseBtn();
	}
}
