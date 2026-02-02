using UnityEngine;
using UnityEngine.UI;

public class PanelPowerDemandHelper : MonoBehaviour
{
	[SerializeField]
	public InputField _inputField;

	[SerializeField]
	private Text _text;

	[SerializeField]
	private Color _colorTextSelection;

	[SerializeField]
	private OrangeInputTextSoundHelper _soundHelper;

	private Color _colorTextOrigin;

	private int _maxValue;

	private bool _isPointerHovering;

	public int Value
	{
		get
		{
			return int.Parse(_inputField.text);
		}
		set
		{
			_inputField.text = value.ToString();
		}
	}

	public void Setup(int value, int maxValue = 999)
	{
		_colorTextOrigin = _text.color;
		_maxValue = maxValue;
		_inputField.text = IntMath.Min(value, maxValue).ToString();
	}

	public void OnClickPanel()
	{
		_inputField.Select();
		_text.color = _colorTextSelection;
	}

	public void OnValueChanged(string value)
	{
		int result;
		if (!int.TryParse(value, out result))
		{
			result = 0;
		}
		if (!string.IsNullOrEmpty(value) && result.ToString() != value)
		{
			_inputField.text = result.ToString();
		}
		else if (result > _maxValue)
		{
			CommonUIHelper.ShowCommonTipUI(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWER_WARN", _maxValue), false, OnConfirmMaxValue);
			_inputField.text = _maxValue.ToString();
			_inputField.interactable = false;
			_isPointerHovering = false;
			_soundHelper.OnEndEditInput();
		}
	}

	public void OnEndInput()
	{
		if (!_isPointerHovering)
		{
			_soundHelper.OnEndEditInput();
		}
	}

	public void OnDeselected()
	{
		_text.color = _colorTextOrigin;
		if (string.IsNullOrEmpty(_inputField.text))
		{
			_inputField.text = "0";
		}
	}

	public void OnPointerEnter()
	{
		_isPointerHovering = true;
	}

	public void OnPointerExit()
	{
		_isPointerHovering = false;
	}

	private void OnConfirmMaxValue()
	{
		_inputField.interactable = true;
	}
}
