using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoBackground : OrangeBgBase
{
	public enum CHARACTERINFO_BG_TYPE
	{
		SELECTION = 0,
		MAIN = 1,
		MAIN_BLURRED = 2,
		NONE = 3
	}

	private enum FADE_TYPE
	{
		FADE_IN = 0,
		FADE_OUT = 1
	}

	[SerializeField]
	private Image mainBlurredImage;

	[SerializeField]
	private Image selectionImage;

	private Image mainImage;

	private CHARACTERINFO_BG_TYPE currentBgType = CHARACTERINFO_BG_TYPE.NONE;

	private void Start()
	{
		mainImage = GetComponent<Image>();
		SetBackground(CHARACTERINFO_BG_TYPE.SELECTION, 0f);
	}

	public void SetBackground(CHARACTERINFO_BG_TYPE type, float transitionTime)
	{
		if (type != currentBgType)
		{
			switch (type)
			{
			case CHARACTERINFO_BG_TYPE.SELECTION:
				ImageFadeEffect(FADE_TYPE.FADE_IN, selectionImage, transitionTime);
				ImageFadeEffect(FADE_TYPE.FADE_OUT, mainBlurredImage, transitionTime);
				break;
			case CHARACTERINFO_BG_TYPE.MAIN:
				ImageFadeEffect(FADE_TYPE.FADE_OUT, selectionImage, transitionTime);
				ImageFadeEffect(FADE_TYPE.FADE_OUT, mainBlurredImage, transitionTime);
				break;
			case CHARACTERINFO_BG_TYPE.MAIN_BLURRED:
				ImageFadeEffect(FADE_TYPE.FADE_OUT, selectionImage, transitionTime);
				ImageFadeEffect(FADE_TYPE.FADE_IN, mainBlurredImage, transitionTime);
				break;
			}
			currentBgType = type;
		}
	}

	private void ImageFadeEffect(FADE_TYPE fadeType, Image image, float transitionTime)
	{
		image.enabled = true;
		if (fadeType == FADE_TYPE.FADE_OUT)
		{
			LeanTween.value(image.color.a, 0f, transitionTime).setOnUpdate(delegate(float val)
			{
				image.color = new Color(1f, 1f, 1f, val);
			}).setOnComplete((Action)delegate
			{
				image.enabled = false;
			});
		}
		else
		{
			LeanTween.value(image.color.a, 1f, transitionTime).setOnUpdate(delegate(float val)
			{
				image.color = new Color(1f, 1f, 1f, val);
			}).setOnComplete((Action)delegate
			{
			});
		}
	}
}
