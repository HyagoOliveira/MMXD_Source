using UnityEngine;

public class CardEquipCell : ScrollIndexCallback
{
	public CardIcon CardIcon;

	private int m_cardID = -1;

	private int m_cardSeqID = -1;

	[SerializeField]
	protected CardEquipUI parent;

	[SerializeField]
	protected Transform m_selectedTick;

	[HideInInspector]
	public int NowIdx = -1;

	public void Update()
	{
		if (parent != null)
		{
			bool active = m_cardSeqID == parent.OnGetTargetCardSeqID();
			m_selectedTick.gameObject.SetActive(active);
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		parent = GetComponentInParent<CardEquipUI>();
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
