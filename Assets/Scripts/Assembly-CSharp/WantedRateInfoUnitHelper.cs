using UnityEngine;
using UnityEngine.UI;

public class WantedRateInfoUnitHelper : MonoBehaviour
{
	[SerializeField]
	private LeanTweenType _tweenType;

	[SerializeField]
	private float _tweenTime;

	[SerializeField]
	private LeanTweenType _tweenTypeRandom;

	[SerializeField]
	private float _tweenTimeRandom;

	[SerializeField]
	private string[] _typeKeys;

	[SerializeField]
	private Text _textType;

	[SerializeField]
	private Text _textRate;

	[SerializeField]
	private ImageSpriteSwitcher _imageRateBarSwitcher;

	[SerializeField]
	private Image _imageRateBar;

	private int _tweenId;

	private int _rate;

	private bool _showRate;

	private void OnDestroy()
	{
		LeanTween.cancel(ref _tweenId);
	}

	public void Setup(int type)
	{
		_textType.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(_typeKeys[type]);
		_imageRateBarSwitcher.ChangeImage(type);
	}

	public void SetRate(int rate)
	{
		SetRate(rate, true, _tweenTime, _tweenType);
	}

	public void SetRandomRate(int rate)
	{
		SetRate(rate, false, _tweenTimeRandom, _tweenTypeRandom);
	}

	private void SetRate(int rate, bool showRate, float tweenTime, LeanTweenType tweenType)
	{
		LeanTween.cancel(ref _tweenId);
		_showRate = showRate;
		_tweenId = LeanTween.value(base.gameObject, _rate, rate, tweenTime).setOnUpdate(OnTweenUpdate).setOnComplete(OnTweenComplete)
			.setEase(tweenType)
			.uniqueId;
		_rate = rate;
	}

	private void OnTweenUpdate(float value)
	{
		_textRate.text = (_showRate ? value.ToString("f0") : "???");
		_imageRateBar.fillAmount = value / 100f;
	}

	private void OnTweenComplete()
	{
		_textRate.text = (_showRate ? _rate.ToString("f0") : "???");
		_imageRateBar.fillAmount = (float)_rate / 100f;
	}
}
