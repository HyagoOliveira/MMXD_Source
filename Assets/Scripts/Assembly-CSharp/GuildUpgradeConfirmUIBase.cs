#define RELEASE
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildUpgradeConfirmUIBase : OrangeUIBase
{
	protected class UpgradeInfo
	{
		public string Name { get; private set; }

		public string Value { get; private set; }

		public string ValueBefore { get; private set; }

		public string ValueAfter { get; private set; }

		public UpgradeInfo(string name, int value)
			: this(name, value.ToString("#,0"))
		{
		}

		public UpgradeInfo(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public UpgradeInfo(string name, int valueBefore, int valueAfter)
			: this(name, valueBefore.ToString("#,0"), valueAfter.ToString("#,0"))
		{
		}

		public UpgradeInfo(string name, string valueBefore, string valueAfter)
		{
			Name = name;
			ValueBefore = valueBefore;
			ValueAfter = valueAfter;
		}
	}

	[SerializeField]
	private RectTransform _rectContainer;

	[SerializeField]
	private Text _textTitle;

	[SerializeField]
	private GameObject _goPanelUpgradeInfoList;

	[SerializeField]
	private PanelUpgradeInfoHelper[] _upgradeInfoHelper;

	[SerializeField]
	private PanelValueInfoHelper _scoreValueInfoHelper;

	[SerializeField]
	private PanelValueInfoHelper _moneyValueInfoHelper;

	[SerializeField]
	private PanelValueBeforeAfterHelper _scoreValueBeforeAfterHelper;

	[SerializeField]
	private PanelValueBeforeAfterHelper _moneyValueBeforeAfterHelper;

	[SerializeField]
	private Button _buttonYes;

	private Action _onYesEvent;

	private Action _onNoEvent;

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected void Setup(string title, int ownScore, int ownMoney, int requireScore, int requireMoney, List<UpgradeInfo> upgradeInfoList, Action onYesCB, Action onNoCB = null)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_textTitle.text = title;
		SetupUpgradeInfo(upgradeInfoList);
		_rectContainer.ForceUpdateRectTransforms();
		int num = ownScore - requireScore;
		int num2 = ownMoney - requireMoney;
		bool flag = num >= 0;
		bool flag2 = num2 >= 0;
		_scoreValueInfoHelper.Setup(requireScore);
		_moneyValueInfoHelper.Setup(requireMoney);
		_scoreValueBeforeAfterHelper.Setup(ownScore, num);
		_moneyValueBeforeAfterHelper.Setup(ownMoney, num2);
		_onYesEvent = onYesCB;
		_onNoEvent = onNoCB;
		_buttonYes.interactable = flag && flag2;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
	}

	private void SetupUpgradeInfo(List<UpgradeInfo> upgradeInfoList)
	{
		if (upgradeInfoList.Count > 0)
		{
			for (int i = 0; i < upgradeInfoList.Count; i++)
			{
				if (i >= _upgradeInfoHelper.Length)
				{
					Debug.LogError("No more UpgradeInfoHelper");
					break;
				}
				UpgradeInfo upgradeInfo = upgradeInfoList[i];
				PanelUpgradeInfoHelper panelUpgradeInfoHelper = _upgradeInfoHelper[i];
				if (!string.IsNullOrEmpty(upgradeInfo.Value))
				{
					panelUpgradeInfoHelper.Setup(upgradeInfo.Name, upgradeInfo.Value);
				}
				else if (!string.IsNullOrEmpty(upgradeInfo.ValueBefore) && !string.IsNullOrEmpty(upgradeInfo.ValueAfter))
				{
					panelUpgradeInfoHelper.Setup(upgradeInfo.Name, upgradeInfo.ValueBefore, upgradeInfo.ValueAfter);
				}
				else
				{
					panelUpgradeInfoHelper.gameObject.SetActive(false);
				}
			}
			for (int j = upgradeInfoList.Count; j < _upgradeInfoHelper.Length; j++)
			{
				_upgradeInfoHelper[j].gameObject.SetActive(false);
			}
		}
		else
		{
			_goPanelUpgradeInfoList.SetActive(false);
		}
	}

	public void OnClickConfirmBtn()
	{
		Action onYesEvent = _onYesEvent;
		if (onYesEvent != null)
		{
			onYesEvent();
		}
		base.CloseSE = SystemSE.NONE;
		base.OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		Action onNoEvent = _onNoEvent;
		if (onNoEvent != null)
		{
			onNoEvent();
		}
		base.OnClickCloseBtn();
	}
}
