using UnityEngine;
using UnityEngine.UI;

public class OreCellBase : ScrollIndexCallback
{
	[SerializeField]
	private CommonIconBase _commonIcon;

	[SerializeField]
	private Text _textName;

	[SerializeField]
	private Text _textBuffInfo;

	protected int _idx;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
	}

	protected void InitOreInfo(int itemID, SKILL_TABLE skillAttrData)
	{
		_commonIcon.SetupItem(itemID);
		_textName.text = ((skillAttrData != null) ? ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillAttrData.w_NAME) : "---");
		_textBuffInfo.text = ((skillAttrData != null) ? ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillAttrData.w_TIP) : "---");
	}
}
