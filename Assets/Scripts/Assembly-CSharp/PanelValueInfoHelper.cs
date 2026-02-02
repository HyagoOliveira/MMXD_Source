using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class PanelValueInfoHelper : MonoBehaviour
{
	[SerializeField]
	private Text _textName;

	[SerializeField]
	private Text _textValue;

	[SerializeField]
	private UIShadow _textValueOutline;

	[SerializeField]
	private Color _colorTextEnough;

	[SerializeField]
	private Color _colorTextNotEnough;

	[SerializeField]
	private Color _colorOutlineEnough;

	[SerializeField]
	private Color _colorOutlineNotEnough;

	public void Setup(string name, int value, bool checkValue = true)
	{
		_textName.text = name;
		Setup(value, checkValue);
	}

	public void Setup(string name, string value)
	{
		_textName.text = name;
		Setup(value);
	}

	public void Setup(int value, bool checkValue = true)
	{
		Setup(value.ToString("#,0"));
		if (checkValue)
		{
			if (value >= 0)
			{
				_textValue.color = _colorTextEnough;
				_textValueOutline.effectColor = _colorOutlineEnough;
			}
			else
			{
				_textValue.color = _colorTextNotEnough;
				_textValueOutline.effectColor = _colorOutlineNotEnough;
			}
		}
	}

	public void Setup(string value)
	{
		_textValue.text = value;
	}
}
