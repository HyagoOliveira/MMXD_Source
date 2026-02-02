using UnityEngine;

public class EquipmentDismantleConfirmIcon : ScrollIndexCallback
{
	public EquipIcon EquipIcon;

	private int m_equipmentID = -1;

	[SerializeField]
	protected EquipmentDismantleConfirm parent;

	[SerializeField]
	protected Transform m_selectedTick;

	[HideInInspector]
	public int NowIdx = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		if ((bool)parent)
		{
			parent.SetEquipIcon(this);
			m_selectedTick.gameObject.SetActive(false);
		}
	}

	public void SetEquipmentID(int equipmentID)
	{
		m_equipmentID = equipmentID;
	}
}
