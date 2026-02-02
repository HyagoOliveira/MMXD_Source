using UnityEngine;
using UnityEngine.UI;

public class OreSelectionCell : OreCellBase
{
	[SerializeField]
	private Text _textStageInfo;

	[SerializeField]
	private Text _textMoneyInfo;

	[SerializeField]
	private GameObject _goButtonSelect;

	[SerializeField]
	private GameObject _goImageSelected;

	[SerializeField]
	private GameObject _goImageOpening;

	private OreSelectionUI _parentUI;

	public override void ScrollCellIndex(int p_idx)
	{
		base.ScrollCellIndex(p_idx);
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<OreSelectionUI>();
			_parentUI.OnItemSelected += RefreshButtonStatus;
		}
		OreInfoData oreInfoData = _parentUI.OreInfoDataList[p_idx];
		InitOreInfo(oreInfoData.ItemID, oreInfoData.MainSkillAttrData);
		_textStageInfo.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(oreInfoData.StageInfo);
		_textMoneyInfo.text = oreInfoData.AttrData.n_ORE_MONEY.ToString("#,0");
		RefreshButtonStatus();
	}

	public void OnClickSelectBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
		_parentUI.OnClickCellSelectBtn(_idx);
	}

	private void RefreshButtonStatus()
	{
		bool flag = _parentUI.SelectedIndex == _idx;
		bool flag2 = _parentUI.OreIDOpening.Contains(_parentUI.OreInfoDataList[_idx].ID);
		_goButtonSelect.SetActive(!flag2 && !flag);
		_goImageSelected.SetActive(!flag2 && flag);
		_goImageOpening.SetActive(flag2);
	}
}
