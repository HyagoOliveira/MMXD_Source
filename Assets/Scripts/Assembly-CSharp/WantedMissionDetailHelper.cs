using System;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class WantedMissionDetailHelper : OrangeChildUIBase
{
	[SerializeField]
	private Text _textTitle;

	[SerializeField]
	private WantedConditionHelper _basicConditionHelper;

	[SerializeField]
	private WantedConditionHelper[] _bonusConditionHelpers;

	public event Action OnClickCloseEvent;

	public void Setup(WANTED_TABLE wantedAttrData)
	{
		_textTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.WANTEDTEXT_TABLE_DICT.GetL10nValue(wantedAttrData.w_NAME);
		_basicConditionHelper.Setup((WantedGoCondition)wantedAttrData.n_BASIS_EFFECT, wantedAttrData.n_EFFECT_X, wantedAttrData.n_EFFECT_Y);
		_bonusConditionHelpers[0].Setup((WantedGoCondition)wantedAttrData.n_EXTRA_1, wantedAttrData.n_EXTRAX_1, wantedAttrData.n_EXTRAY_1);
		_bonusConditionHelpers[1].Setup((WantedGoCondition)wantedAttrData.n_EXTRA_2, wantedAttrData.n_EXTRAX_2, wantedAttrData.n_EXTRAY_2);
		_bonusConditionHelpers[2].Setup((WantedGoCondition)wantedAttrData.n_EXTRA_3, wantedAttrData.n_EXTRAX_3, wantedAttrData.n_EXTRAY_3);
	}

	public void SetConditionFlag(WantedConditionFlag conditionFlag)
	{
		_basicConditionHelper.IsOn = conditionFlag.HasFlag(WantedConditionFlag.BasicCondition);
		_bonusConditionHelpers[0].IsOn = conditionFlag.HasFlag(WantedConditionFlag.BonusCondition1);
		_bonusConditionHelpers[1].IsOn = conditionFlag.HasFlag(WantedConditionFlag.BonusCondition2);
		_bonusConditionHelpers[2].IsOn = conditionFlag.HasFlag(WantedConditionFlag.BonusCondition3);
	}

	public void OnClickCloseButton()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
		Action onClickCloseEvent = this.OnClickCloseEvent;
		if (onClickCloseEvent != null)
		{
			onClickCloseEvent();
		}
	}
}
