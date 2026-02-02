using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public abstract class WantedMemberSelectUIBase<U, C> : OrangeUIBase where U : WantedMemberSelectUIBase<U, C> where C : WantedMemberSelectCellHelperBase
{
	[SerializeField]
	protected LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	protected C _scrollCell;

	[SerializeField]
	protected Transform _iconOrderArrow;

	protected CharacterHelper.SortType _sortType;

	protected bool _isSortDescend;

	private CommonCharacterSortFilterUI _characterSortFilterUI;

	public event Action<CharacterHelper.SortType, bool> OnSortTypeChangedEvent;

	private void OnDestroy()
	{
		if (_characterSortFilterUI != null)
		{
			_characterSortFilterUI.OnClickCloseBtn();
		}
	}

	public virtual void Setup(CharacterHelper.SortType sortType, bool isSortDescend)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_sortType = sortType;
		_isSortDescend = isSortDescend;
		_iconOrderArrow.localRotation = new Quaternion(0f, 0f, _isSortDescend ? 180f : 0f, 0f);
		SortCharacterAndRefresh();
	}

	public virtual void OnClickConfirmButton()
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK08;
		OnClickCloseBtn();
	}

	public void OnClickOrderButton()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		_isSortDescend = !_isSortDescend;
		_iconOrderArrow.localRotation = new Quaternion(0f, 0f, _isSortDescend ? 180f : 0f, 0f);
		Action<CharacterHelper.SortType, bool> onSortTypeChangedEvent = this.OnSortTypeChangedEvent;
		if (onSortTypeChangedEvent != null)
		{
			onSortTypeChangedEvent(_sortType, _isSortDescend);
		}
		SortCharacterAndRefresh();
	}

	public void OnClickSortButton()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<CommonCharacterSortFilterUI>("UI_CommonCharacterSortFilter", OnCharacterSortFilterUILoaded);
	}

	private void OnCharacterSortFilterUILoaded(CommonCharacterSortFilterUI ui)
	{
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		ui.Setup(_sortType);
		ui.OnConfirmEvent += OnConfirmSortFilter;
		ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnCharacterSortFilterUIClosed));
		_characterSortFilterUI = ui;
	}

	private void OnCharacterSortFilterUIClosed()
	{
		_characterSortFilterUI = null;
	}

	private void OnConfirmSortFilter(CharacterHelper.SortType sortType, CharacterHelper.SortStatus sortStatus)
	{
		if (sortType != _sortType)
		{
			_sortType = sortType;
			Action<CharacterHelper.SortType, bool> onSortTypeChangedEvent = this.OnSortTypeChangedEvent;
			if (onSortTypeChangedEvent != null)
			{
				onSortTypeChangedEvent(_sortType, _isSortDescend);
			}
			SortCharacterAndRefresh();
		}
	}

	protected virtual void SortCharacterAndRefresh()
	{
		RefreshScrollRect();
	}

	protected virtual void RefreshScrollRect()
	{
	}
}
