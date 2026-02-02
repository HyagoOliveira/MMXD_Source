using System;
using UnityEngine;

[Serializable]
public class RandomColor
{
	public bool useRandom = true;

	public float minHue;

	public float minSaturation;

	public float minValue;

	public float maxHue = 1f;

	public float maxSaturation = 1f;

	public float maxValue = 1f;

	public Color fixedColor = Color.white;

	public Color testcolor = Color.clear;

	public Color get_color
	{
		get
		{
			float h = UnityEngine.Random.Range(minHue, maxHue);
			float s = UnityEngine.Random.Range(minSaturation, maxSaturation);
			float v = UnityEngine.Random.Range(minValue, maxValue);
			return Color.HSVToRGB(h, s, v);
		}
	}

	public RandomColor(float minHue = 0f, float maxHue = 1f, float minSaturation = 0f, float maxSaturation = 1f, float minValue = 0f, float maxValue = 1f)
	{
		useRandom = true;
		this.minHue = minHue;
		this.maxHue = maxHue;
		this.minSaturation = minSaturation;
		this.maxSaturation = maxSaturation;
		this.minValue = minValue;
		this.maxValue = maxValue;
		testcolor = Color.clear;
	}
}
