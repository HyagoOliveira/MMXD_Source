using System;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class CommonConsumeMsgUI : OrangeUIBase
{
	public SystemSE OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;

	public SystemSE YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	public SystemSE NoSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;

	[SerializeField]
	private Color _colorOutlineEnougth = new Color(0.14509805f, 0.16862746f, 10f / 51f);

	[SerializeField]
	private Color _colorOutlineNotEnougth = Color.black;

	[SerializeField]
	private Color _colorTextEnougth = Color.white;

	[SerializeField]
	private Color _colorTextNotEnougth = new Color(1f, 0.12156863f, 0.101960786f);

	[SerializeField]
	private Text _textTitle;

	[SerializeField]
	private RectTransform _rectContent;

	[SerializeField]
	private Text _textDesc;

	[SerializeField]
	private Text _textAmountBefore;

	[SerializeField]
	private Text _textAmountAfter;

	[SerializeField]
	private UIShadow _textAmountAfterOutline;

	[SerializeField]
	private Button _buttonYes;

	[SerializeField]
	private Text _textYes;

	[SerializeField]
	private Text _textNo;

	[SerializeField]
	private Image _imageCost;

	private int _ownAmount;

	private int _costAmount;

	private Action _onYesEvent;

	private Action _onNoEvent;

	private bool AmountEnough
	{
		get
		{
			return _ownAmount >= _costAmount;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		UpdateFont();
	}

	private void UpdateFont()
	{
		Font languageFont = MonoBehaviourSingleton<LocalizationManager>.Instance.LanguageFont;
		_textTitle.font = languageFont;
		_textDesc.font = languageFont;
		_textYes.font = languageFont;
		_textNo.font = languageFont;
	}

	public void Setup(string title, string desc, string buttonYes, string buttonNo, int ownAmount, int costAmount, Action onYesCB, Action onNoCB = null)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		SetupText(title, desc, buttonYes, buttonNo);
		_ownAmount = ownAmount;
		_costAmount = costAmount;
		int num = _ownAmount - _costAmount;
		bool flag = num >= 0;
		_textAmountBefore.text = _ownAmount.ToString("#,0");
		_textAmountAfter.text = num.ToString("#,0");
		_textAmountAfter.color = (flag ? _colorTextEnougth : _colorTextNotEnougth);
		_textAmountAfterOutline.effectColor = (flag ? _colorOutlineEnougth : _colorOutlineNotEnougth);
		_onYesEvent = onYesCB;
		_onNoEvent = onNoCB;
		if (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(OpenSE);
		}
	}

	public void SetupByKey(string titleKey, string descKey, string buttonYesKey, string buttonNoKey, int ownAmount, int costAmount, Action onYesCB, Action onNoCB = null)
	{
		Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(titleKey), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(descKey).Replace("\\n", "\n"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(buttonYesKey), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(buttonNoKey), ownAmount, costAmount, onYesCB, onNoCB);
	}

	private void SetupText(string title, string desc, string buttonYes, string buttonNo = "")
	{
		_textTitle.text = title;
		_textDesc.text = desc.Replace("\\n", "\n");
		_textYes.text = buttonYes;
		_textNo.text = buttonNo;
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectContent);
	}

	public void OnClickConfirmBtn()
	{
		if (AmountEnough)
		{
			if (!MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE)
			{
				base.CloseSE = SystemSE.NONE;
			}
			else
			{
				base.CloseSE = YesSE;
			}
			Action onYesEvent = _onYesEvent;
			if (onYesEvent != null)
			{
				onYesEvent();
			}
			base.OnClickCloseBtn();
		}
		else
		{
			LinkShopUIConfirm();
		}
	}

	public override void OnClickCloseBtn()
	{
		base.CloseSE = (MonoBehaviourSingleton<AudioManager>.Instance.IsInitSystemSE ? SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL : SystemSE.NONE);
		Action onNoEvent = _onNoEvent;
		if (onNoEvent != null)
		{
			onNoEvent();
		}
		base.OnClickCloseBtn();
	}

	private void LinkShopUIConfirm()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", OnLinkShopUIConfirmUILoaded);
	}

	private void OnLinkShopUIConfirmUILoaded(CommonUI ui)
	{
		ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		ui.SetupYesNoByKey("COMMON_TIP", "DIAMOND_OUT", "COMMON_OK", "COMMON_CANCEL", OnLinkShopUIConfirm);
		OnClickCloseBtn();
	}

	private void OnLinkShopUIConfirm()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<ShopTopUI>("UI_ShopTop", OnShopUILoaded);
	}

	private void OnShopUILoaded(ShopTopUI ui)
	{
		ui.EnableBackToHometop = false;
		ui.Setup(ShopTopUI.ShopSelectTab.directproduct);
	}
}
