using System;
using UnityEngine;

[Serializable]
public class boolchance
{
	public bool boolean;

	public float chance;

	public bool check
	{
		get
		{
			if (boolean)
			{
				return UnityEngine.Random.value <= chance;
			}
			return false;
		}
	}

	public boolchance(bool boolean, float chance)
	{
		this.boolean = boolean;
		this.chance = chance;
	}
}
