using UnityEngine;

public class ItemBoxUIUnit : ScrollIndexCallback
{
	public ItemIconWithAmount ItemIcon;

	[SerializeField]
	protected ItemBoxUI parent;

	[HideInInspector]
	public int NowIdx = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		parent.SetItemIcon(this);
	}
}
