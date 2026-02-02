using UnityEngine;
using UnityEngine.UI;

public class CrusadeBossHpBarHelper : MonoBehaviour
{
	[SerializeField]
	private Image _imageHpBar;

	[SerializeField]
	private Text _textHpBar;

	[SerializeField]
	private Text _textStep;

	[SerializeField]
	private Text _textTh;

	public void Setup(int step, long hpNow, int hpSet, int setNum)
	{
		int num = hpSet * setNum;
		_imageHpBar.fillAmount = Mathf.Min(1f, (float)hpNow / (float)num);
		_textHpBar.text = string.Format("{0} / {1}", hpNow, num);
		_textStep.text = step.ToString();
		int num2 = step % 10;
		int num3 = (((step < 10 || step > 20) && num2 > 0 && num2 < 4) ? num2 : 4);
		_textTh.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(string.Format("RAID_STEP_{0}", num3));
	}
}
