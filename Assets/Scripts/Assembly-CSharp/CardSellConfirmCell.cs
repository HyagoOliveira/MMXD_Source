using UnityEngine;

public class CardSellConfirmCell : ScrollIndexCallback
{
	public CardIcon CardIcon;

	private int m_cardID = -1;

	private int m_cardSeqID = -1;

	[SerializeField]
	protected CardSellConfirmUI parent;

	[HideInInspector]
	public int NowIdx = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		parent = GetComponentInParent<CardSellConfirmUI>();
		if ((bool)parent)
		{
			parent.SetCardIcon(this);
		}
	}

	public void SetCardID(int cardSeqID, int cardID)
	{
		m_cardSeqID = cardSeqID;
		m_cardID = cardID;
	}
}
