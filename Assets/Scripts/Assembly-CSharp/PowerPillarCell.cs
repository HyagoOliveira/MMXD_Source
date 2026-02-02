#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class PowerPillarCell : ScrollIndexCallback
{
	[SerializeField]
	private GameObject _goPanelEnable;

	[SerializeField]
	private GameObject _goPanelDisable;

	[SerializeField]
	private GameObject _goPanelLockInfo;

	[SerializeField]
	private CommonIconBase _commonIcon;

	[SerializeField]
	private Text _textOreName;

	[SerializeField]
	private Text _textBuffInfo;

	[SerializeField]
	private Text _textStageInfo;

	[SerializeField]
	private RectTransform _rectTimeInfo;

	[SerializeField]
	private Text _textTimeInfo;

	[SerializeField]
	private Text _textLockInfo;

	[SerializeField]
	private RotateHelper _effectRotateHelperOuter;

	[SerializeField]
	private RotateHelper _effectRotateHelperInner;

	private PowerPillarChildUI _parentUI;

	private int _idx;

	private PowerPillarInfoData _powerPillarInfoData;

	private string _lastTimeText;

	public override void ScrollCellIndex(int idx)
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<PowerPillarChildUI>();
		}
		_idx = idx;
		RefreshCell();
	}

	public override void RefreshCell()
	{
		if (_idx < _parentUI.PowerPillarInfoDataList.Count)
		{
			PowerPillarInfoData powerPillarInfoData = _parentUI.PowerPillarInfoDataList[_idx];
			if (_powerPillarInfoData == powerPillarInfoData)
			{
				_powerPillarInfoData = powerPillarInfoData;
				RefreshPillarTime();
				return;
			}
		}
		if (_idx < _parentUI.PowerPillarInfoDataList.Count)
		{
			_goPanelLockInfo.SetActive(false);
			_powerPillarInfoData = _parentUI.PowerPillarInfoDataList[_idx];
			OreInfoData oreInfo = _powerPillarInfoData.OreInfo;
			if (oreInfo != null)
			{
				SKILL_TABLE mainSkillAttrData = oreInfo.MainSkillAttrData;
				_textOreName.text = ((mainSkillAttrData != null) ? ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(mainSkillAttrData.w_NAME) : "---");
				_textBuffInfo.text = ((mainSkillAttrData != null) ? ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(mainSkillAttrData.w_TIP) : "---");
				_textStageInfo.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(oreInfo.StageInfo);
				_goPanelEnable.SetActive(true);
				_goPanelDisable.SetActive(false);
				_commonIcon.SetupItem(oreInfo.ItemID);
				RefreshPillarTime();
				ToggleEffect(true);
			}
			else
			{
				_goPanelEnable.SetActive(false);
				_goPanelDisable.SetActive(true);
				ToggleEffect(false);
			}
		}
		else
		{
			_powerPillarInfoData = null;
			_goPanelEnable.SetActive(false);
			_goPanelDisable.SetActive(false);
			_goPanelLockInfo.SetActive(true);
			POWER_TABLE pOWER_TABLE = Singleton<PowerTowerSystem>.Instance.PowerTowerRankupSettings[_idx];
			_textLockInfo.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERTOWER_WARN_3", pOWER_TABLE.n_ID);
			ToggleEffect(false);
		}
	}

	private void RefreshPillarTime()
	{
		string timeText = OrangeGameUtility.GetTimeText(Mathf.FloorToInt((float)(_powerPillarInfoData.ExpireTime - MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC).TotalSeconds));
		_textTimeInfo.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_REMAIN", timeText);
		if (_lastTimeText != timeText)
		{
			_lastTimeText = timeText;
			LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTimeInfo);
		}
	}

	public void OnClickEnableBtn()
	{
		Debug.Log("[OnClickEnableBtn]");
		_parentUI.OnClickOneOpenPillarBtn(_powerPillarInfoData.PillarID);
	}

	public void OnClickDisableBtn()
	{
		Debug.Log("[OnClickDisableBtn]");
		_parentUI.OnClickOneClosePillarBtn(_powerPillarInfoData.OreInfo);
	}

	private void ToggleEffect(bool isActive)
	{
		if (isActive)
		{
			_effectRotateHelperOuter.StartRotate();
			_effectRotateHelperInner.StartRotate();
		}
		else
		{
			_effectRotateHelperOuter.StopRotate();
			_effectRotateHelperInner.StopRotate();
		}
	}
}
