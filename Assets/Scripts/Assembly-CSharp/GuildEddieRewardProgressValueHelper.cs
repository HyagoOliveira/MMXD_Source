using UnityEngine;
using UnityEngine.UI;

public class GuildEddieRewardProgressValueHelper : MonoBehaviour
{
	[SerializeField]
	private ImageSpriteSwitcher _eddieIconSwitcher;

	[SerializeField]
	private Text _textRewardValue;

	[SerializeField]
	private GameObject _panelProgress;

	[SerializeField]
	private Image _imageProgress;

	public void Setup(float minValue, float maxValue, float currentValue, int eddieIndex, bool enableProgress = true)
	{
		if (minValue == maxValue || !enableProgress)
		{
			_panelProgress.SetActive(false);
		}
		else
		{
			_panelProgress.SetActive(true);
			_imageProgress.fillAmount = (Mathf.Min(currentValue, maxValue) - minValue) / (maxValue - minValue);
		}
		_textRewardValue.text = ((maxValue >= 1000f) ? string.Format("{0}K", maxValue / 1000f) : maxValue.ToString());
		_eddieIconSwitcher.ChangeImage(eddieIndex);
	}
}
