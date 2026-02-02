using System.Collections.Generic;
using UnityEngine;

public class IllustrColume : ScrollIndexCallback
{
	[SerializeField]
	public IllustrTargetCell[] m_cells;

	private IllustrationTargetUI tIllustrationTargetUI;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void BackToPool()
	{
		m_cells[0].ResetActive();
		m_cells[1].ResetActive();
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		tIllustrationTargetUI = GetComponentInParent<IllustrationTargetUI>();
		List<IllustrationTargetUI.ConditionCell> cellList = tIllustrationTargetUI.cellList;
		int num = p_idx * 2;
		base.gameObject.SetActive(true);
		if (num < cellList.Count)
		{
			m_cells[0].Setup(cellList[num]);
		}
		num++;
		if (num < cellList.Count)
		{
			m_cells[1].Setup(cellList[num]);
		}
	}
}
