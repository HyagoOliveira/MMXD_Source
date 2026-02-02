using UnityEngine;
using UnityEngine.UI;

public abstract class GuildDonateUIBase : OrangeUIBase
{
	[SerializeField]
	protected Slider SliderDonate;

	[SerializeField]
	protected Text TextDonate;

	[SerializeField]
	private Button _btnConfirm;

	protected int _minValue;

	protected int _maxValue;

	protected int _currentValue;

	private bool _valueChangedByButton;

	public bool bMuteSE;

	protected virtual void Setup(int minValue, int maxValue)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_minValue = minValue;
		_maxValue = maxValue;
		SliderDonate.minValue = minValue;
		SliderDonate.maxValue = maxValue;
		OnSliderValueChangedEvent(SliderDonate.value);
	}

	public void OnClickMinValueBtn()
	{
		SetSliderValueByButton(_minValue);
	}

	public void OnClickMaxValueBtn()
	{
		SetSliderValueByButton(_maxValue);
	}

	public void OnClickPlusOneThousandBtn()
	{
		StepSliderOffset(1000);
	}

	public void OnClickMinusOneThousandBtn()
	{
		StepSliderOffset(-1000);
	}

	public void OnHoldingValueChangedEvent(float offset)
	{
		StepSliderOffset((int)offset);
	}

	private void StepSliderOffset(int offset)
	{
		SetSliderValueByButton(IntMath.Max(_minValue, IntMath.Min(_currentValue + offset, _maxValue)));
	}

	private void SetSliderValueByButton(int value)
	{
		_valueChangedByButton = true;
		SetSliderValue(value);
		SliderDonate.value = _currentValue;
	}

	public void OnSliderValueChangedEvent(float value)
	{
		if (_valueChangedByButton)
		{
			_valueChangedByButton = false;
			return;
		}
		int num = (int)value;
		if (num == (int)SliderDonate.minValue)
		{
			SetSliderValueByButton(_minValue);
		}
		else if (num == (int)SliderDonate.maxValue)
		{
			SetSliderValueByButton(_maxValue);
		}
		else
		{
			SetSliderValue((int)value);
		}
	}

	protected virtual void SetSliderValue(int value)
	{
		if (_currentValue != value)
		{
			PlaySE();
		}
		_currentValue = value;
		TextDonate.text = value.ToString("#,0");
		_btnConfirm.interactable = value > 0;
	}

	private void PlaySE()
	{
		if (bMuteSE)
		{
			bMuteSE = false;
		}
		else
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
	}
}
