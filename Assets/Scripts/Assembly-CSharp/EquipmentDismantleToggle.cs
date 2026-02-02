using UnityEngine;
using UnityEngine.UI;

public class EquipmentDismantleToggle : ScrollIndexCallback
{
	[SerializeField]
	private Toggle m_toggle;

	[SerializeField]
	protected EquipmentDismantleUI parent;

	[SerializeField]
	private OrangeText levelLabel;

	private int m_toggleIndex = -1;

	private int m_toggleLevel;

	private void Start()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
		m_toggleIndex = p_idx;
		parent = GetComponentInParent<EquipmentDismantleUI>();
		if ((bool)parent)
		{
			parent.SetupLevelToggle(this);
		}
	}

	public void Setup(int inputLevel)
	{
		m_toggleLevel = inputLevel;
		string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_LV_TAG"), m_toggleLevel);
		levelLabel.text = text;
	}

	public int GetLevel()
	{
		return m_toggleLevel;
	}

	public int GetIndex()
	{
		return m_toggleIndex;
	}

	public Toggle GetToggle()
	{
		return m_toggle;
	}
}
