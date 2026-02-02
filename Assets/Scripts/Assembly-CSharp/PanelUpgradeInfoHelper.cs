using UnityEngine;

public class PanelUpgradeInfoHelper : MonoBehaviour
{
	[SerializeField]
	private PanelValueInfoHelper _valueInfoHelper;

	[SerializeField]
	private PanelValueBeforeAfterHelper _valueBeforeAfterHelper;

	public void Setup(string name, int value)
	{
		Setup(name, value.ToString("#,0"));
	}

	public void Setup(string name, string value)
	{
		_valueInfoHelper.gameObject.SetActive(true);
		_valueBeforeAfterHelper.gameObject.SetActive(false);
		_valueInfoHelper.Setup(name, value);
	}

	public void Setup(string name, int valueBefore, int valueAfter)
	{
		Setup(name, valueBefore.ToString("#,0"), valueAfter.ToString("#,0"));
	}

	public void Setup(string name, string valueBefore, string valueAfter)
	{
		_valueInfoHelper.gameObject.SetActive(false);
		_valueBeforeAfterHelper.gameObject.SetActive(true);
		_valueBeforeAfterHelper.Setup(name, valueBefore, valueAfter);
	}
}
