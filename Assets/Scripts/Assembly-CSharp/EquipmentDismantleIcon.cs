#define RELEASE
using UnityEngine;

public class EquipmentDismantleIcon : ScrollIndexCallback
{
	public EquipIcon EquipIcon;

	private int m_equipmentID = -1;

	[SerializeField]
	protected EquipmentDismantleUI parent;

	[SerializeField]
	protected Transform m_selectedTick;

	[HideInInspector]
	public int NowIdx = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		parent = GetComponentInParent<EquipmentDismantleUI>();
		if (!parent)
		{
			return;
		}
		parent.SetEquipIcon(this);
		if (m_equipmentID != -1)
		{
			if (parent.IsSelected(m_equipmentID))
			{
				Debug.Log("Selected!");
			}
			SetSelection(parent.IsSelected(m_equipmentID));
		}
	}

	public void SetEquipmentID(int equipmentID)
	{
		m_equipmentID = equipmentID;
	}

	public void SetSelection(bool selection)
	{
		if (GetSelection() != selection)
		{
			m_selectedTick.gameObject.SetActive(selection);
		}
	}

	public bool GetSelection()
	{
		return m_selectedTick.gameObject.activeSelf;
	}

	public void ToggleSelection()
	{
		SetSelection(parent.IsSelected(m_equipmentID));
	}
}
