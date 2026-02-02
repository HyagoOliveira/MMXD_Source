using UnityEngine;
using UnityEngine.UI;

public static class RectTransformExtension
{
	public static void Left(this RectTransform p_rectTransform, float p_value)
	{
		p_rectTransform.offsetMin = new Vector2(p_value, p_rectTransform.offsetMin.y);
	}

	public static void Right(this RectTransform rTrans, float value)
	{
		rTrans.offsetMax = new Vector2(0f - value, rTrans.offsetMax.y);
	}

	public static void Bottom(this RectTransform rTrans, float value)
	{
		rTrans.offsetMin = new Vector2(rTrans.offsetMin.x, value);
	}

	public static void Top(this RectTransform rTrans, float value)
	{
		rTrans.offsetMax = new Vector2(rTrans.offsetMax.x, 0f - value);
	}

	public static void RebuildLayout(this RectTransform rt)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
		Vector2 sizeDelta = new Vector2(LayoutUtility.GetPreferredSize(rt, 0), LayoutUtility.GetPreferredSize(rt, 1));
		rt.sizeDelta = sizeDelta;
	}
}
