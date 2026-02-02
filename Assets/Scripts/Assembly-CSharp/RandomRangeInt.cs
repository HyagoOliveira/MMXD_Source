using System;
using UnityEngine;

[Serializable]
public class RandomRangeInt
{
	public bool useRandom = true;

	public int min = 10;

	public int max = 1000;

	public int def = 24;

	public int test;

	public int get_int
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

	public RandomRangeInt(int def = 26, int min = 10, int max = 100)
	{
		useRandom = true;
		this.min = min;
		this.max = max;
		this.def = def;
		test = def;
	}
}
