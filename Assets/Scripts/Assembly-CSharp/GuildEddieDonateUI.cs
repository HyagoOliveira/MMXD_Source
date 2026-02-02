using UnityEngine;
using UnityEngine.UI;

public class GuildEddieDonateUI : GuildDonateUIBase
{
	[SerializeField]
	private Text _textItemInfo;

	[SerializeField]
	private Text _textAmountBefore;

	[SerializeField]
	private Text _textAmountAfter;

	private int _originValue;

	private void Start()
	{
		Singleton<GuildSystem>.Instance.OnEddieDonateEvent += OnEddieDonateEvent;
	}

	private void OnDestroy()
	{
		Singleton<GuildSystem>.Instance.OnEddieDonateEvent -= OnEddieDonateEvent;
	}

	public new void Setup(int maxValue, int originValue)
	{
		_originValue = originValue;
		base.Setup(0, maxValue);
		ITEM_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.ITEM_TABLE_DICT.TryGetValue(1, out value))
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
			_textItemInfo.text = l10nValue;
		}
		else
		{
			_textItemInfo.text = string.Empty;
		}
	}

	public void OnClickConfirmBtn()
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_STORE02;
		Singleton<GuildSystem>.Instance.ReqEddieDonate(IntMath.Min(_currentValue, _maxValue));
	}

	protected override void SetSliderValue(int value)
	{
		base.SetSliderValue(value);
		_textAmountBefore.text = _originValue.ToString("#,0");
		_textAmountAfter.text = IntMath.Max(_originValue - value, 0).ToString("#,0");
	}

	private void OnEddieDonateEvent(Code ackCode, NetGuildInfo guildInfo)
	{
		OnClickCloseBtn();
	}
}
