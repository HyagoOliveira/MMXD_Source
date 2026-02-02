using UnityEngine;
using UnityEngine.UI;

public class CursorSettingUI : OrangeUIBase
{
	[SerializeField]
	private Slider sliderOfSensitivity;

	public void Setup()
	{
		sliderOfSensitivity.value = MonoBehaviourSingleton<CursorController>.Instance.Sensitivity;
	}

	private void OnDestroy()
	{
	}

	public void OnSensitivityChange()
	{
		MonoBehaviourSingleton<CursorController>.Instance.Sensitivity = (int)sliderOfSensitivity.value;
	}
}
