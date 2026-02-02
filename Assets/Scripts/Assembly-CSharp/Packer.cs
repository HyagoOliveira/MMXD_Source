using UnityEngine;

public static class Packer
{
	public static float ToFloat(float x, float y, float z, float w)
	{
		x = ((x < 0f) ? 0f : ((1f < x) ? 1f : x));
		y = ((y < 0f) ? 0f : ((1f < y) ? 1f : y));
		z = ((z < 0f) ? 0f : ((1f < z) ? 1f : z));
		w = ((w < 0f) ? 0f : ((1f < w) ? 1f : w));
		return (Mathf.FloorToInt(w * 63f) << 18) + (Mathf.FloorToInt(z * 63f) << 12) + (Mathf.FloorToInt(y * 63f) << 6) + Mathf.FloorToInt(x * 63f);
	}

	public static float ToFloat(Vector4 factor)
	{
		return ToFloat(Mathf.Clamp01(factor.x), Mathf.Clamp01(factor.y), Mathf.Clamp01(factor.z), Mathf.Clamp01(factor.w));
	}

	public static float ToFloat(float x, float y, float z)
	{
		x = ((x < 0f) ? 0f : ((1f < x) ? 1f : x));
		y = ((y < 0f) ? 0f : ((1f < y) ? 1f : y));
		z = ((z < 0f) ? 0f : ((1f < z) ? 1f : z));
		return (Mathf.FloorToInt(z * 255f) << 16) + (Mathf.FloorToInt(y * 255f) << 8) + Mathf.FloorToInt(x * 255f);
	}

	public static float ToFloat(float x, float y)
	{
		x = ((x < 0f) ? 0f : ((1f < x) ? 1f : x));
		y = ((y < 0f) ? 0f : ((1f < y) ? 1f : y));
		return (Mathf.FloorToInt(y * 4095f) << 12) + Mathf.FloorToInt(x * 4095f);
	}
}
