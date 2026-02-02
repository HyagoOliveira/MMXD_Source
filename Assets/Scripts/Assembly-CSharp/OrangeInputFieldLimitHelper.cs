using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class OrangeInputFieldLimitHelper : MonoBehaviour
{
	[SerializeField]
	private int _maxLine;

	[SerializeField]
	private bool _disallowEmoji;

	private InputField _inputField;

	private int _characterLimit;

	public void Awake()
	{
		_inputField = GetComponent<InputField>();
		_characterLimit = _inputField.characterLimit;
		_inputField.characterLimit = 0;
		InputField inputField = _inputField;
		inputField.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new InputField.OnValidateInput(OnValidateInput));
		_inputField.onValueChanged.AddListener(OnValueChanged);
	}

	public void OnDestroy()
	{
		_inputField.onValueChanged.RemoveListener(OnValueChanged);
	}

	private char OnValidateInput(string oldText, int charIndex, char addedChar)
	{
		if (_characterLimit > 0 && charIndex >= _characterLimit)
		{
			return '\0';
		}
		char newChar;
		if (_disallowEmoji && !InputFieldHelper.CheckForEmoji(addedChar, out newChar))
		{
			addedChar = newChar;
		}
		return addedChar;
	}

	private void OnValueChanged(string value)
	{
		if (_maxLine > 0)
		{
			value = value.Replace("\\n", "\n");
			string[] array = value.Split('\n');
			if (array.Length > _maxLine)
			{
				List<string> list = array.ToList();
				list.RemoveRange(_maxLine, list.Count - _maxLine);
				_inputField.text = string.Join("\n", list.ToArray());
			}
		}
	}
}
