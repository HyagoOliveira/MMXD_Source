using System;
using System.Collections.Generic;

public class WantedMemberSelectUI : WantedMemberSelectUIBase<WantedMemberSelectUI, WantedMemberSelectCellHelper>
{
	public List<NetCharacterInfo> SelectedCharacterListCache { get; private set; }

	public int MemberSelectLimit { get; private set; }

	public event Action<List<NetCharacterInfo>> OnConfirmMembersEvent;

	public void Setup(List<NetCharacterInfo> selectedCharacterList, CharacterHelper.SortType sortType, bool isSortDescend)
	{
		SelectedCharacterListCache = selectedCharacterList;
		MemberSelectLimit = 4;
		_scrollCell.gameObject.SetActive(false);
		base.Setup(sortType, isSortDescend);
	}

	public override void OnClickConfirmButton()
	{
		Action<List<NetCharacterInfo>> onConfirmMembersEvent = this.OnConfirmMembersEvent;
		if (onConfirmMembersEvent != null)
		{
			onConfirmMembersEvent(SelectedCharacterListCache);
		}
		base.OnClickConfirmButton();
	}

	public void OnClickCellButton(NetCharacterInfo characterInfo)
	{
		int num = SelectedCharacterListCache.FindIndex((NetCharacterInfo info) => info.CharacterID == characterInfo.CharacterID);
		if (num >= 0)
		{
			SelectedCharacterListCache.RemoveAt(num);
		}
		else
		{
			SelectedCharacterListCache.Add(characterInfo);
		}
		_scrollRect.RefreshCellsNew();
	}

	protected override void SortCharacterAndRefresh()
	{
		Singleton<WantedSystem>.Instance.SortCharacterList(_sortType, _isSortDescend);
		base.SortCharacterAndRefresh();
	}

	protected override void RefreshScrollRect()
	{
		_scrollRect.OrangeInit(_scrollCell, 10, ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList().Count);
	}
}
