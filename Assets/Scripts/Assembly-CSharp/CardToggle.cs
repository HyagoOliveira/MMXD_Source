using UnityEngine;
using UnityEngine.UI;

public class CardToggle : ScrollIndexCallback
{
	[SerializeField]
	private Toggle m_toggle;

	[SerializeField]
	protected CardSellUI parent;

	[SerializeField]
	private OrangeText levelLabel;

	private int m_Index = -1;

	private int m_Rarity;

	private string[] str = new string[7] { "", "D", "C", "B", "A", "S", "SS" };

	private void Start()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
		m_Index = p_idx;
		parent = GetComponentInParent<CardSellUI>();
		if ((bool)parent)
		{
			parent.SetupCardToggle(this);
		}
	}

	public void Setup(int nRarity)
	{
		m_Rarity = nRarity;
		levelLabel.text = str[m_Rarity];
	}

	public int GetRarity()
	{
		return m_Rarity;
	}

	public int GetIndex()
	{
		return m_Index;
	}

	public Toggle GetToggle()
	{
		return m_toggle;
	}
}
