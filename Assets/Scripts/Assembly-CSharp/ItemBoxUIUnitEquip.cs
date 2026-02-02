using UnityEngine;

public class ItemBoxUIUnitEquip : ScrollIndexCallback
{
	public EquipIcon EquipIcon;

	[SerializeField]
	protected ItemBoxUI parent;

	[HideInInspector]
	public int NowIdx = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		parent.SetEquipIcon(this);
	}
}
