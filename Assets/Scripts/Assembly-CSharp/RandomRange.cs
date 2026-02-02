using System;
using UnityEngine;

[Serializable]
public class RandomRange
{
	public bool useRandom = true;

	public float min;

	public float max = 1f;

	public float def = 1f;

	public float test;

	public float get_float
	{
		get
		{
			if (!useRandom)
			{
				return def;
			}
			return UnityEngine.Random.Range(min, max);
		}
	}

	public RandomRange(float def = 1f, float min = 0f, float max = 1f)
	{
		useRandom = true;
		this.min = min;
		this.max = max;
		this.def = def;
		test = def;
	}
}
