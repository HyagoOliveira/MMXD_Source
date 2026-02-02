using UnityEngine;
using UnityEngine.UI;

public class GuideToggleUnit : MonoBehaviour
{
	[SerializeField]
	private GuideUI parent;

	[SerializeField]
	private Button btnOn;

	[SerializeField]
	private OrangeText textToggle;

	[SerializeField]
	private Image imgCheckMark;

	private int type;

	public void Setup(string p_str, int p_type, bool p_isOn)
	{
		textToggle.text = p_str;
		type = p_type;
		btnOn.interactable = !p_isOn;
		imgCheckMark.enabled = p_isOn;
	}

	public void SetToggleStage(int nowType)
	{
		btnOn.interactable = type != nowType;
		imgCheckMark.enabled = type == nowType;
	}

	public void OnClickToggle()
	{
		parent.OnClickToggleUnit(type);
	}
}
