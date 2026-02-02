using System;
using UnityEngine;
using UnityEngine.UI;

public class GuildBadgeEditorUI : OrangeUIBase
{
	[SerializeField]
	private Button _buttonConfirm;

	[SerializeField]
	private CommonGuildBadge _guildBadge;

	[SerializeField]
	private Scrollbar _badgeColorScroll;

	[SerializeField]
	private GuildBadgeScrollbarHelper _badgeColorSwitchHelper;

	[SerializeField]
	private GuildBadgeButtonSwitchHelper[] _badgeImageSwitchHelpers;

	private int _badgeIndexOrigin;

	private bool _isCreate;

	private int oldBadgeIndex;

	public event Action<int, float> OnConfirmEvent;

	public void OnDestroy()
	{
		this.OnConfirmEvent = null;
		if (!_isCreate)
		{
			Singleton<GuildSystem>.Instance.OnEditBadgeEvent -= OnEditBadgeEvent;
		}
	}

	public void Setup(int badgeIndex, float badgeColor, bool isCreate = true)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_isCreate = isCreate;
		_badgeIndexOrigin = badgeIndex;
		if (!_isCreate)
		{
			Singleton<GuildSystem>.Instance.OnEditBadgeEvent += OnEditBadgeEvent;
		}
		_guildBadge.Setup(badgeIndex, badgeColor);
		GuildBadgeButtonSwitchHelper[] badgeImageSwitchHelpers = _badgeImageSwitchHelpers;
		for (int i = 0; i < badgeImageSwitchHelpers.Length; i++)
		{
			badgeImageSwitchHelpers[i].OnChangeImageEvent += OnBadgeIndexChanged;
		}
		oldBadgeIndex = badgeIndex;
		SetSelectionMark(badgeIndex);
	}

	private void OnBadgeIndexChanged(int badgeIndex)
	{
		if (oldBadgeIndex != badgeIndex)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
			oldBadgeIndex = badgeIndex;
		}
		SetSelectionMark(badgeIndex);
		_guildBadge.SetBadgeIndex(badgeIndex);
	}

	private void SetSelectionMark(int badgeIndex)
	{
		GuildBadgeButtonSwitchHelper[] badgeImageSwitchHelpers = _badgeImageSwitchHelpers;
		for (int i = 0; i < badgeImageSwitchHelpers.Length; i++)
		{
			badgeImageSwitchHelpers[i].ToggleSelectionMark(false);
		}
		_badgeImageSwitchHelpers[badgeIndex].ToggleSelectionMark(true);
		if (!_isCreate)
		{
			_buttonConfirm.interactable = badgeIndex != _badgeIndexOrigin;
		}
	}

	public void OnClickConfirmBtn()
	{
		if (_isCreate)
		{
			Action<int, float> onConfirmEvent = this.OnConfirmEvent;
			if (onConfirmEvent != null)
			{
				onConfirmEvent(_guildBadge.BadgeIndex, _guildBadge.BadgeColor);
			}
			base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
			OnClickCloseBtn();
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CommonConsumeMsgUI>("UI_CommonConsumeMsg", OnEditBadgeConfirmUILoaded);
		}
	}

	private void OnEditBadgeConfirmUILoaded(CommonConsumeMsgUI ui)
	{
		int gUILD_CHANGE_BADGE = OrangeConst.GUILD_CHANGE_BADGE;
		ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_STORE01;
		ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_SETUP_REVISE", gUILD_CHANGE_BADGE), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_NO"), ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel(), gUILD_CHANGE_BADGE, OnEditBadgeConfirm);
	}

	private void OnEditBadgeConfirm()
	{
		base.CloseSE = SystemSE.NONE;
		Singleton<GuildSystem>.Instance.ReqEditBadge(_guildBadge.BadgeIndex, _guildBadge.BadgeColor);
	}

	private void OnEditBadgeEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		if (ackCode == Code.GUILD_EDIT_BADGE_SUCCESS)
		{
			Action<int, float> onConfirmEvent = this.OnConfirmEvent;
			if (onConfirmEvent != null)
			{
				onConfirmEvent(guildInfo.Badge, (float)guildInfo.BadgeColor / 360f);
			}
			OnClickCloseBtn();
		}
	}
}
