using UnityEngine;
using UnityEngine.UI;

public class DeepRecordUseSortUI : OrangeUIBase
{
	private enum Sort
	{
		BATTLE = 0,
		EXPLORE = 1,
		ACTION = 2,
		TOTAL = 3
	}

	[SerializeField]
	private Toggle[] m_sortToggles;

	private bool initiated;

	private CharacterHelper.SortType sortType = CharacterHelper.SortType.BATTLE;

	private Sort GetSortBySortType(CharacterHelper.SortType sortType)
	{
		switch (sortType)
		{
		default:
			return Sort.BATTLE;
		case CharacterHelper.SortType.EXPLORE:
			return Sort.EXPLORE;
		case CharacterHelper.SortType.ACTION:
			return Sort.ACTION;
		case CharacterHelper.SortType.TOTAL:
			return Sort.TOTAL;
		}
	}

	public void Setup()
	{
		initiated = false;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		sortType = DeepRecordHelper.GetLastSortType();
		int sortBySortType = (int)GetSortBySortType(sortType);
		if (sortBySortType < m_sortToggles.Length)
		{
			m_sortToggles[sortBySortType].isOn = true;
		}
		initiated = true;
	}

	public void OnClickSortType(int index)
	{
		if (index != (int)sortType && initiated)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		sortType = (CharacterHelper.SortType)index;
	}

	public void OnClickSortType_Battle()
	{
		SortToggleUpdate(CharacterHelper.SortType.BATTLE);
	}

	public void OnClickSortType_Explore()
	{
		SortToggleUpdate(CharacterHelper.SortType.EXPLORE);
	}

	public void OnClickSortType_Action()
	{
		SortToggleUpdate(CharacterHelper.SortType.ACTION);
	}

	public void OnClickSortType_Total()
	{
		SortToggleUpdate(CharacterHelper.SortType.TOTAL);
	}

	private void SortToggleUpdate(CharacterHelper.SortType p_sortType)
	{
		if (p_sortType != sortType && initiated)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		sortType = p_sortType;
	}

	protected override bool IsEscapeVisible()
	{
		if (!initiated)
		{
			return false;
		}
		return base.IsEscapeVisible();
	}

	public void OnClickBtnOK()
	{
		DeepRecordHelper.SetLastSortType(sortType);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		OnClickCloseBtn();
	}
}
