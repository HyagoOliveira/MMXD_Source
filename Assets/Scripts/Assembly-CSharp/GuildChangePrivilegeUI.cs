using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuildChangePrivilegeUI : OrangeUIBase
{
	private string _playerId;

	private GuildPrivilege _originPrivilege;

	[SerializeField]
	private GuildPlayerInfoBeforeAfterHelper _playerInfoController;

	[SerializeField]
	private GuildChangePrivilegeToggleGroupHelper _privilegeGroupHelper;

	[SerializeField]
	private Button _confirmButton;

	private bool bMuteOnce;

	public event Action<string> OnChangeGuildPrivilegeEvent;

	public void OnEnable()
	{
		Singleton<GuildSystem>.Instance.OnChangeMemberPrivilegeEvent += OnChangeMemberPrivilegeEvent;
	}

	public void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnChangeMemberPrivilegeEvent -= OnChangeMemberPrivilegeEvent;
	}

	public void Setup(string playerId, GuildPrivilege privilegeOptions)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_playerId = playerId;
		NetMemberInfo memberInfo;
		if (Singleton<GuildSystem>.Instance.TryGetMemberInfo(_playerId, out memberInfo))
		{
			_originPrivilege = (GuildPrivilege)memberInfo.Privilege;
			_privilegeGroupHelper.OnSelectionChangedEvent += OnSelectionChangedEvent;
			_privilegeGroupHelper.Setup(_originPrivilege, privilegeOptions);
		}
		_playerInfoController.SetPlayerInfoBefore(_playerId);
		OnSelectionChangedEvent();
		_playerInfoController.OnPlayerInfoAfterChange = PlayToggleSE;
	}

	private void PlayToggleSE()
	{
		if (bMuteOnce)
		{
			bMuteOnce = false;
		}
		else
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
		}
	}

	public void OnClickConfirmBtn()
	{
		if (_privilegeGroupHelper.PrivilegeSelected != GuildPrivilege.GuildMember)
		{
			NetMemberInfo memberReplaced = Singleton<GuildSystem>.Instance.MemberInfoListCache.Find((NetMemberInfo memberInfo) => memberInfo.Privilege == (int)_privilegeGroupHelper.PrivilegeSelected && memberInfo.MemberId != _playerId);
			if (memberReplaced != null)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildChangePrivilegeReplaceConfirm", delegate(GuildChangePrivilegeReplaceConfirmUI ui)
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.Setup(memberReplaced.MemberId);
					ui.OnConfirmEvent += ConfirmChangePrivilege;
				});
				return;
			}
		}
		ConfirmChangePrivilege();
	}

	private void ConfirmChangePrivilege()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		Singleton<GuildSystem>.Instance.ReqChangeMemberPrivilege(_playerId, (int)_privilegeGroupHelper.PrivilegeSelected);
	}

	private void OnSelectionChangedEvent()
	{
		_playerInfoController.SetPlayerInfoAfter(_playerId, _privilegeGroupHelper.PrivilegeSelected);
		_confirmButton.interactable = _originPrivilege != _privilegeGroupHelper.PrivilegeSelected;
	}

	private void OnChangeMemberPrivilegeEvent(Code ackCode)
	{
		Action<string> onChangeGuildPrivilegeEvent = this.OnChangeGuildPrivilegeEvent;
		if (onChangeGuildPrivilegeEvent != null)
		{
			onChangeGuildPrivilegeEvent(_playerId);
		}
		OnClickCloseBtn();
	}
}
