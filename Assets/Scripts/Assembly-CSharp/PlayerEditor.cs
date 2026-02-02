using UnityEngine;
using UnityEngine.UI;

public class PlayerEditor : OrangeUIBase
{
	public Slider WalkSlider;

	public Slider JumpSlider;

	public Slider DashSlider;

	public Toggle ClassicToggle;

	public OrangeText WalkValueText;

	public OrangeText JumpValueText;

	public OrangeText DashValueText;

	public void Setup()
	{
		WalkSlider.onValueChanged.AddListener(OnUpdateWalkSliderValue);
		JumpSlider.onValueChanged.AddListener(OnUpdateJumpSliderValue);
		DashSlider.onValueChanged.AddListener(OnUpdateDashSliderValue);
		UpdateValue();
	}

	private void UpdateValue()
	{
		WalkSlider.value = PlayerPrefs.GetFloat("DEBUG_WALKSPEED", 1.75f);
		JumpSlider.value = PlayerPrefs.GetFloat("DEBUG_JUMPSPEED", 5.3242188f);
		DashSlider.value = PlayerPrefs.GetFloat("DEBUG_DASHSPEED", 3.4570312f);
		ClassicToggle.isOn = PlayerPrefs.GetInt("SETTING_CLASSIC_CONTROL", 0) != 0;
	}

	public void OnDefaultBtn()
	{
		PlayerPrefs.SetFloat("DEBUG_WALKSPEED", 1.75f);
		PlayerPrefs.SetFloat("DEBUG_JUMPSPEED", 5.3242188f);
		PlayerPrefs.SetFloat("DEBUG_DASHSPEED", 3.4570312f);
		PlayerPrefs.SetInt("SETTING_CLASSIC_CONTROL", 0);
		UpdateValue();
	}

	public void OnSaveBtn()
	{
		PlayerPrefs.SetFloat("DEBUG_WALKSPEED", WalkSlider.value);
		PlayerPrefs.SetFloat("DEBUG_JUMPSPEED", JumpSlider.value);
		PlayerPrefs.SetFloat("DEBUG_DASHSPEED", DashSlider.value);
		PlayerPrefs.SetInt("SETTING_CLASSIC_CONTROL", ClassicToggle.isOn ? 1 : 0);
		UpdateValue();
		OrangeBattleUtility.UpdatePlayerParameters();
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	private void OnUpdateWalkSliderValue(float value)
	{
		WalkValueText.text = value.ToString();
	}

	private void OnUpdateJumpSliderValue(float value)
	{
		JumpValueText.text = value.ToString();
	}

	private void OnUpdateDashSliderValue(float value)
	{
		DashValueText.text = value.ToString();
	}
}
