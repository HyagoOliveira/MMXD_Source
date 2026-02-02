using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class PanelValueBeforeAfterHelper : MonoBehaviour
{
	[SerializeField]
	private Text _textName;

	[SerializeField]
	private Text _textValueBefore;

	[SerializeField]
	private Text _textValueAfter;

	[SerializeField]
	private UIShadow _textValueAfterOutline;

	[SerializeField]
	private Color _colorTextEnough;

	[SerializeField]
	private Color _colorTextNotEnough;

	[SerializeField]
	private Color _colorOutlineEnough;

	[SerializeField]
	private Color _colorOutlineNotEnough;

	public void Setup(string name, int valueBefore, int valueAfter, bool checkValueAfter = true)
	{
		_textName.text = name;
		Setup(valueBefore, valueAfter, checkValueAfter);
	}

	public void Setup(string name, string valueBefore, string valueAfter)
	{
		_textName.text = name;
		Setup(valueBefore, valueAfter);
	}

	public void Setup(int valueBefore, int valueAfter, bool checkValueAfter = true)
	{
		Setup(valueBefore.ToString("#,0"), valueAfter.ToString("#,0"));
		if (checkValueAfter)
		{
			if (valueAfter >= 0)
			{
				_textValueAfter.color = _colorTextEnough;
				_textValueAfterOutline.effectColor = _colorOutlineEnough;
			}
			else
			{
				_textValueAfter.color = _colorTextNotEnough;
				_textValueAfterOutline.effectColor = _colorOutlineNotEnough;
			}
		}
	}

	public void Setup(string valueBefore, string valueAfter)
	{
		_textValueBefore.text = valueBefore;
		_textValueAfter.text = valueAfter;
	}
}
