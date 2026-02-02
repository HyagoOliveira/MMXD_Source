using System;
using UnityEngine;
using UnityEngine.UI;

public class CommonCharacterSortFilterUI : OrangeUIBase
{
	[SerializeField]
	private RectTransform _rectContainer;

	[SerializeField]
	private GameObject _panelSortType;

	[SerializeField]
	private GameObject _panelSortStatus;

	[SerializeField]
	private Toggle[] _toggleSortType;

	[SerializeField]
	private Toggle[] _toggleSortStatus;

	private CharacterHelper.SortType _sortType;

	private CharacterHelper.SortStatus _sortStatus;

	private bool _isInitialized;

	public event Action<CharacterHelper.SortType, CharacterHelper.SortStatus> OnConfirmEvent;

	public void Setup(CharacterHelper.SortType sortType)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		Init(sortType, CharacterHelper.SortStatus.ALL);
		_panelSortStatus.SetActive(false);
		_rectContainer.sizeDelta = new Vector2(_rectContainer.sizeDelta.x, 483f);
		_isInitialized = true;
	}

	public void Setup(CharacterHelper.SortType sortType, CharacterHelper.SortStatus sortStatus)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		Init(sortType, sortStatus);
		_rectContainer.sizeDelta = new Vector2(_rectContainer.sizeDelta.x, 678f);
		_isInitialized = true;
	}

	private void Init(CharacterHelper.SortType sortType, CharacterHelper.SortStatus sortStatus)
	{
		InitSortType(sortType);
		InitSortStatus(sortStatus);
	}

	private void InitSortType(CharacterHelper.SortType sortType)
	{
		Toggle[] toggleSortType = _toggleSortType;
		for (int i = 0; i < toggleSortType.Length; i++)
		{
			toggleSortType[i].isOn = false;
		}
		_toggleSortType[(int)sortType].isOn = true;
	}

	private void InitSortStatus(CharacterHelper.SortStatus sortStatus)
	{
		Toggle[] toggleSortStatus = _toggleSortStatus;
		for (int i = 0; i < toggleSortStatus.Length; i++)
		{
			toggleSortStatus[i].isOn = false;
		}
		_toggleSortStatus[(int)sortStatus].isOn = true;
	}

	public void OnToggleSortTypeSelected(int param)
	{
		if (_isInitialized && _sortType != (CharacterHelper.SortType)param)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		_sortType = (CharacterHelper.SortType)param;
	}

	public void OnToggleSortStatusSelected(int param)
	{
		if (_isInitialized && _sortStatus != (CharacterHelper.SortStatus)param)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		_sortStatus = (CharacterHelper.SortStatus)param;
	}

	public void OnClickConfirmButton()
	{
		Action<CharacterHelper.SortType, CharacterHelper.SortStatus> onConfirmEvent = this.OnConfirmEvent;
		if (onConfirmEvent != null)
		{
			onConfirmEvent(_sortType, _sortStatus);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
	}
}
