using System;

public class WantedFriendSelectUI : WantedMemberSelectUIBase<WantedFriendSelectUI, WantedFriendSelectCellHelper>
{
	public WantedMemberInfo SelectedFriendInfoCache { get; private set; }

	public event Action<WantedMemberInfo> OnConfirmFriendEvent;

	public void Setup(WantedMemberInfo selectedFriendInfo, CharacterHelper.SortType sortType, bool isSortDescend)
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		SelectedFriendInfoCache = selectedFriendInfo;
		_scrollCell.gameObject.SetActive(false);
		base.Setup(sortType, isSortDescend);
	}

	public override void OnClickConfirmButton()
	{
		Action<WantedMemberInfo> onConfirmFriendEvent = this.OnConfirmFriendEvent;
		if (onConfirmFriendEvent != null)
		{
			onConfirmFriendEvent(SelectedFriendInfoCache);
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		base.OnClickConfirmButton();
	}

	public void OnClickCellButton(WantedMemberInfo friendInfo)
	{
		if (SelectedFriendInfoCache != null)
		{
			SelectedFriendInfoCache = null;
		}
		else
		{
			SelectedFriendInfoCache = friendInfo;
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		_scrollRect.RefreshCellsNew();
	}

	protected override void SortCharacterAndRefresh()
	{
		Singleton<WantedSystem>.Instance.SortFriendCharacterList(_sortType, _isSortDescend);
		base.SortCharacterAndRefresh();
	}

	protected override void RefreshScrollRect()
	{
		_scrollRect.OrangeInit(_scrollCell, 10, Singleton<WantedSystem>.Instance.SortedFriendInfoList.Count);
	}
}
