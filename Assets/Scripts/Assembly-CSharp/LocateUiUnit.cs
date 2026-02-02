using UnityEngine;
using UnityEngine.UI;

public class LocateUiUnit : ScrollIndexCallback
{
	[SerializeField]
	private OrangeText text;

	[SerializeField]
	private Button btnClick;

	[SerializeField]
	protected LocateUI parent;

	[HideInInspector]
	public int NowIdx = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		btnClick.interactable = NowIdx > 0;
		text.text = ManagedSingleton<OrangeTextDataManager>.Instance.AREATEXT_TABLE_DICT.GetL10nValue(parent.ListOpenArea[NowIdx].s_TEXT);
	}

	public void OnClickSelectLocate()
	{
		parent.OnClickUnit(NowIdx);
	}
}
