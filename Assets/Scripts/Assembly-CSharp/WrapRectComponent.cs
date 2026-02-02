using UnityEngine;
using UnityEngine.UI;

public class WrapRectComponent : MonoBehaviour
{
	[SerializeField]
	private OrangeText text;

	private RectTransform[] rectTransforms = new RectTransform[0];

	private ScrollRect scrollRect;

	public OrangeText Text
	{
		get
		{
			return text;
		}
	}

	public void Start()
	{
		rectTransforms = GetComponentsInChildren<RectTransform>();
		scrollRect = GetComponent<ScrollRect>();
	}

	public void SetText(string p_str)
	{
		text.text = p_str;
	}

	public void VerticalNormalizedPosition(float val)
	{
		if ((bool)scrollRect)
		{
			scrollRect.verticalNormalizedPosition = Mathf.Clamp01(val);
		}
	}

	public void MovementType(ScrollRect.MovementType movementType)
	{
		if ((bool)scrollRect)
		{
			scrollRect.movementType = movementType;
		}
	}
}
