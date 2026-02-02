using UnityEngine;
using UnityEngine.UI;

public class GuildBuffFloatInfoCell : ScrollIndexCallback
{
	[SerializeField]
	private CommonIconBase _commonIcon;

	[SerializeField]
	private Text _textOreName;

	[SerializeField]
	private Text _textBuffInfo;

	private GuildBuffFloatInfoUI _parentUI;

	private int _idx;

	private PowerPillarInfoData _powerPillarInfoData;

	public override void ScrollCellIndex(int idx)
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildBuffFloatInfoUI>();
		}
		_idx = idx;
		RefreshCell();
	}

	public override void RefreshCell()
	{
		_powerPillarInfoData = null;
		if (_idx < _parentUI.PowerPillarInfoDataList.Count)
		{
			_powerPillarInfoData = _parentUI.PowerPillarInfoDataList[_idx];
			OreInfoData oreInfo = _powerPillarInfoData.OreInfo;
			SKILL_TABLE mainSkillAttrData = oreInfo.MainSkillAttrData;
			_textOreName.text = ((mainSkillAttrData != null) ? ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(mainSkillAttrData.w_NAME) : "---");
			_textBuffInfo.text = ((mainSkillAttrData != null) ? ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(mainSkillAttrData.w_TIP) : "---");
			_commonIcon.SetupItem(oreInfo.ItemID);
		}
	}
}
