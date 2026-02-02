using System;
using UnityEngine.UI;

public static class TextExtension
{
	public static bool TweenValue(this Text text, ref int nowVal, int newVal)
	{
		if (nowVal == newVal)
		{
			text.text = newVal.ToString();
			return false;
		}
		LeanTween.value(text.gameObject, nowVal, newVal, 0.2f).setOnUpdate(delegate(float v)
		{
			text.text = ((int)v).ToString("0");
		}).setOnComplete((Action)delegate
		{
			text.text = newVal.ToString();
		});
		nowVal = newVal;
		return true;
	}

	public static void TryTweenValue(this Text text, int newVal)
	{
		int result;
		int.TryParse(text.text, out result);
		text.TweenValue(ref result, newVal);
	}
}
