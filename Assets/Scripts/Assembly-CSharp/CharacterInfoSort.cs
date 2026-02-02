using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoSort : OrangeUIBase
{
	public enum SortType
	{
		RARITY = 0,
		STAR = 1
	}

	public enum StatusType
	{
		OBTAINED = 0,
		FRAGMENT = 1,
		ALL = 2
	}

	[SerializeField]
	private Toggle[] m_sortToggles;

	[SerializeField]
	private Toggle[] m_statusToggles;

	private CharacterHelper.SortType m_sortType;

	private CharacterHelper.SortStatus m_sortStatus;

	private bool m_sortDescend = true;

	private bool initiated;

	public void Setup()
	{
		initiated = false;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		m_sortType = ManagedSingleton<CharacterHelper>.Instance.GetCharacterUISortType();
		if ((int)m_sortType < m_sortToggles.Length)
		{
			m_sortToggles[(int)m_sortType].isOn = true;
		}
		m_sortStatus = ManagedSingleton<CharacterHelper>.Instance.GetCharacterUISortStatus();
		if ((int)m_sortStatus < m_statusToggles.Length)
		{
			m_statusToggles[(int)m_sortStatus].isOn = true;
		}
		m_sortDescend = ManagedSingleton<CharacterHelper>.Instance.GetCharacterUISortDescend();
		initiated = true;
	}

	public void OnClickSortType(int index)
	{
		if (index != (int)m_sortType && initiated)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		m_sortType = (CharacterHelper.SortType)index;
	}

	public void OnClickStatusType(int index)
	{
		if (index != (int)m_sortStatus && initiated)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		m_sortStatus = (CharacterHelper.SortStatus)index;
	}

	public void OnClickOkBtn()
	{
		ManagedSingleton<CharacterHelper>.Instance.SortCharacterList(m_sortType, m_sortStatus, m_sortDescend);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
	}

	protected override bool IsEscapeVisible()
	{
		if (!initiated)
		{
			return false;
		}
		return base.IsEscapeVisible();
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}
}
