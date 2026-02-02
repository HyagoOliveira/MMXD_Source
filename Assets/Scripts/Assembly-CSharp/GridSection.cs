using UnityEngine;
using UnityEngine.UI;

public class GridSection : ScrollIndexCallback
{
	[SerializeField]
	private Image m_sectionBG;

	[SerializeField]
	private Image m_sectionFG;

	[SerializeField]
	private OrangeText m_sectionText;

	[SerializeField]
	private Transform m_sectionContent;

	private Minigame01UI m_parentUI;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetRewardTAndSctionT(Minigame01UI.RewardT rt, Minigame01UI.SectionT st)
	{
	}

	public override void BackToPool()
	{
		for (int num = m_sectionContent.transform.childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate(m_sectionContent.transform.GetChild(num).gameObject);
		}
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}

	public override void ScrollCellIndex(int p_idx)
	{
	}
}
