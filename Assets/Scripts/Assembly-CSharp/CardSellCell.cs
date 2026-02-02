using UnityEngine;

public class CardSellCell : ScrollIndexCallback
{
	public CardIcon CardIcon;

	private int m_cardID = -1;

	private int m_cardSeqID = -1;

	[SerializeField]
	protected CardSellUI parent;

	[SerializeField]
	protected Transform m_selectedTick;

	[HideInInspector]
	public int NowIdx = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		parent = GetComponentInParent<CardSellUI>();
		if ((bool)parent)
		{
			parent.SetCardIcon(this);
			SetSelection(parent.IsSelected(p_idx));
			if (m_cardSeqID != -1)
			{
				SetSelection(parent.IsSelected(m_cardSeqID));
			}
		}
	}

	public void SetCardID(int cardSeqID, int cardID)
	{
		m_cardSeqID = cardSeqID;
		m_cardID = cardID;
	}

	public void SetSelection(bool selection)
	{
		m_selectedTick.gameObject.SetActive(selection);
	}

	public bool GetSelection()
	{
		return m_selectedTick.gameObject.activeSelf;
	}

	public bool ToggleSelection()
	{
		m_selectedTick.gameObject.SetActive(!m_selectedTick.gameObject.activeSelf);
		return m_selectedTick.gameObject.activeSelf;
	}
}
