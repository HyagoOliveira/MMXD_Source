using System;
using UnityEngine;

[Serializable]
public struct HexCoordinates
{
	[SerializeField]
	private int x;

	[SerializeField]
	private int z;

	public int X
	{
		get
		{
			return x;
		}
	}

	public int Z
	{
		get
		{
			return z;
		}
	}

	public int Y
	{
		get
		{
			return -X - Z;
		}
	}

	public HexCoordinates(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	public static HexCoordinates FromOffestCoordinates(int x, int z)
	{
		return new HexCoordinates(x - z / 2, z);
	}

	public static HexCoordinates FromPosition(Vector3 position)
	{
		float num = position.x / 1.7840122f;
		float num2 = 0f - num;
		float num3 = position.z / 3.09f;
		num -= num3;
		num2 -= num3;
		int num4 = Mathf.RoundToInt(num);
		int num5 = Mathf.RoundToInt(num2);
		int num6 = Mathf.RoundToInt(0f - num - num2);
		if (num4 + num5 + num6 != 0)
		{
			float num7 = Mathf.Abs(num - (float)num4);
			float num8 = Mathf.Abs(num2 - (float)num5);
			float num9 = Mathf.Abs(0f - num - num2 - (float)num6);
			if (num7 > num8 && num7 > num9)
			{
				num4 = num5 - num6;
			}
			else if (num9 > num8)
			{
				num6 = -num4 - num5;
			}
		}
		return new HexCoordinates(num4, num6);
	}

	public override string ToString()
	{
		return string.Format("({0},{1},{2})", X, Y, Z);
	}

	public string ToStringOnSeparateLines()
	{
		return string.Format("<color=#FF0000>{0}</color>\n<color=#00FF00>{1}</color>\n<color=#0000FF>{2}</color>", X, Y, Z);
	}
}
