using System;
using UnityEngine;
using UnityEngine.UI;

public class OrangeScrollingPvpImg : OrangeScrollingImg
{
	private int tweenUid = -1;

	[SerializeField]
	private float MaxScaleValue = 2f;

	protected override void OnScale()
	{
		base.OnScale();
		if (!(imgScroll.transform.localScale.x > MaxScaleValue))
		{
			return;
		}
		GameObject obj = new GameObject
		{
			layer = base.gameObject.layer
		};
		AspectRatioFitter aspectRatioFitter = obj.AddComponent<AspectRatioFitter>();
		aspectRatioFitter.aspectRatio = 1.777778f;
		aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
		Image image = obj.AddComponent<Image>();
		image.sprite = imgScroll.sprite;
		image.color = Color.white;
		image.SetNativeSize();
		obj.transform.SetParent(base.transform.parent, false);
		obj.transform.SetAsFirstSibling();
		Image oldScrolling = imgScroll;
		imgScroll = image;
		tweenUid = LeanTween.color(oldScrolling.rectTransform, Color.clear, 1f).setOnComplete((Action)delegate
		{
			tweenUid = -1;
			if (oldScrolling.gameObject == base.gameObject)
			{
				UnityEngine.Object.Destroy(oldScrolling);
			}
			else
			{
				UnityEngine.Object.Destroy(oldScrolling.gameObject);
			}
		}).uniqueId;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		LeanTween.cancel(ref tweenUid);
	}
}
