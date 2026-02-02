using UnityEngine;
using UnityEngine.UI;

public class MyUGUIEnhanceItem : EnhanceItem
{
	private Button uButton;

	private RawImage rawImage;

	protected override void OnStart()
	{
		rawImage = GetComponent<RawImage>();
		uButton = GetComponent<Button>();
		uButton.onClick.AddListener(OnClickUGUIButton);
	}

	private void OnClickUGUIButton()
	{
		OnClickEnhanceItem();
	}

	protected override void SetItemDepth(float depthCurveValue, int depthFactor, float itemCount)
	{
		int siblingIndex = (int)(depthCurveValue * itemCount);
		base.transform.SetSiblingIndex(siblingIndex);
	}

	public override void SetSelectState(bool isCenter)
	{
		if (rawImage == null)
		{
			rawImage = GetComponent<RawImage>();
		}
		rawImage.color = (isCenter ? Color.white : Color.gray);
	}
}
