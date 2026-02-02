using System;
using CallbackDefs;
using UnityEngine;

public class WantedFriendSelectCellHelper : WantedMemberSelectCellHelperBase
{
	private WantedFriendSelectUI _parentUI;

	private int _idx;

	private CommonPlayerInfoMiniUI _playerInfoMiniUI;

	private void OnDestroy()
	{
		if (_playerInfoMiniUI != null)
		{
			_playerInfoMiniUI.OnClickCloseBtn();
		}
	}

	public override void ScrollCellIndex(int idx)
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<WantedFriendSelectUI>();
		}
		_idx = idx;
		_playerIcon.gameObject.SetActive(true);
		RefreshCell();
	}

	public override void RefreshCell()
	{
		WantedMemberInfo selectedFriendInfoCache = _parentUI.SelectedFriendInfoCache;
		WantedMemberInfo friendInfo = Singleton<WantedSystem>.Instance.SortedFriendInfoList[_idx];
		if (selectedFriendInfoCache != null && selectedFriendInfoCache.HelpInfo.PlayerID == friendInfo.HelpInfo.PlayerID)
		{
			_goSelectBorder.SetActive(true);
			_imageSelectIndex.gameObject.SetActive(true);
			_imageSelectIndex.ChangeImage(0);
		}
		else
		{
			_goSelectBorder.SetActive(false);
			_imageSelectIndex.gameObject.SetActive(false);
		}
		bool flag = Singleton<WantedSystem>.Instance.WantedHelpInfoCacheList.FindIndex((NetWantedHelpInfo helpInfo) => helpInfo.PlayerID == friendInfo.HelpInfo.PlayerID) >= 0;
		_goTipDepartured.SetActive(flag);
		_playerIcon.Setup(friendInfo.PlayerHUD.m_IconNumber, false, OnClickPlayerIcon);
		_commonIcon.SetupWanted(_idx, friendInfo.CharacterInfo, OnClickIcon, !flag && (_parentUI.SelectedFriendInfoCache == null || _parentUI.SelectedFriendInfoCache.HelpInfo.PlayerID == friendInfo.HelpInfo.PlayerID));
	}

	private void OnClickIcon(int idx)
	{
		if (!_commonIcon.IsEnabled)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WANTED_WARN_7"));
			return;
		}
		WantedMemberInfo friendInfo = Singleton<WantedSystem>.Instance.SortedFriendInfoList[idx];
		_parentUI.OnClickCellButton(friendInfo);
	}

	private void OnClickPlayerIcon()
	{
		WantedMemberInfo friendInfo = Singleton<WantedSystem>.Instance.SortedFriendInfoList[_idx];
		Vector3 tarPos = _playerIcon.GetComponent<RectTransform>().position;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WantedPlayerInfoMini", delegate(CommonPlayerInfoMiniUI ui)
		{
			ui.Setup(friendInfo.PlayerHUD.m_PlayerId, tarPos);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnPlayerInfoMiniUIClosed));
			_playerInfoMiniUI = ui;
		});
	}

	private void OnPlayerInfoMiniUIClosed()
	{
		_playerInfoMiniUI = null;
	}
}
