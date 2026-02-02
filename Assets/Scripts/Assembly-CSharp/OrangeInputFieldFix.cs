using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class OrangeInputFieldFix : MonoBehaviour
{
	public void Start()
	{
		InputField component = GetComponent<InputField>();
		Text textComponent = component.textComponent;
		if (textComponent != null)
		{
			textComponent.alignByGeometry = false;
		}
		Text text = component.placeholder as Text;
		if (text != null)
		{
			text.alignByGeometry = false;
		}
	}
}
