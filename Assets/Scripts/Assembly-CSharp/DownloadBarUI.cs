using System;
using UnityEngine;
using UnityEngine.UI;

public class DownloadBarUI : MonoBehaviour
{
	private const float tweenTime = 0.1f;

	[SerializeField]
	private Canvas infoCanvas;

	[SerializeField]
	private Image FillFg;

	[SerializeField]
	private NonDrawingGraphic block;

	private float maxWidth;

	private bool isTweening;

	private Canvas canvas;

	private void Awake()
	{
		canvas = GetComponent<Canvas>();
		if (null != canvas)
		{
			canvas.enabled = true;
		}
		UpdateLoadingBlock(false);
	}

	private void Start()
	{
		Setup();
	}

	private void Setup()
	{
		maxWidth = FillFg.rectTransform.sizeDelta.x;
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.UPDATE_DOWNLOAD_BAR, UpdateDownloadBar);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.UPDATE_DOWNLOAD_BAR, UpdateDownloadBar);
	}

	private void UpdateDownloadBar(bool isActive)
	{
		if (isTweening)
		{
			return;
		}
		if (!isActive && infoCanvas.enabled)
		{
			isTweening = true;
			LeanTween.value(FillFg.fillAmount, 1f, 0.1f).setOnUpdate(delegate(float val)
			{
				FillFg.fillAmount = val;
			}).setOnComplete((Action)delegate
			{
				infoCanvas.enabled = false;
				isTweening = false;
			});
		}
		else if (isActive)
		{
			if (!infoCanvas.isActiveAndEnabled)
			{
				infoCanvas.enabled = true;
			}
			FillFg.fillAmount = Mathf.Clamp01(MonoBehaviourSingleton<AssetsBundleManager>.Instance.DownloadProgress * maxWidth / maxWidth);
		}
	}

	public void UpdateLoadingBlock(bool p_active)
	{
		block.raycastTarget = p_active;
	}
}
