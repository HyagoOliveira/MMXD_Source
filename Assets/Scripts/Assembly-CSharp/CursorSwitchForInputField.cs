using UnityEngine;
using UnityEngine.UI;

public class CursorSwitchForInputField : MonoBehaviour
{
	private InputField inputField;

	private bool isFocus;

	private void Start()
	{
		inputField = base.gameObject.GetComponent<InputField>();
		if (inputField == null)
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (isFocus != inputField.isFocused)
		{
			isFocus = inputField.isFocused;
			MonoBehaviourSingleton<CursorController>.Instance.IsEnable = !isFocus;
		}
	}
}
