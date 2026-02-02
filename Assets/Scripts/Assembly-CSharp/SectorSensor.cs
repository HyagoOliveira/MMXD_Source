using UnityEngine;

public static class SectorSensor
{
	public static bool Look(Vector2 origin, int direction, float angle, float distance, LayerMask layerMask, out RaycastHit2D hit)
	{
		if (LookAround(origin, direction, Quaternion.identity, distance, layerMask, out hit))
		{
			return true;
		}
		float num = 0f;
		int num2 = 0;
		if (distance < 4f)
		{
			num = angle / 4f;
			num2 = 2;
		}
		else
		{
			num = angle / 8f;
			num2 = 4;
		}
		for (int i = 0; i < num2; i++)
		{
			if (LookAround(origin, direction, Quaternion.Euler(0f, 0f, -1f * num * (float)(i + 1)), distance, layerMask, out hit) || LookAround(origin, direction, Quaternion.Euler(0f, 0f, num * (float)(i + 1)), distance, layerMask, out hit))
			{
				return true;
			}
		}
		return false;
	}

	private static bool LookAround(Vector2 origin, int direction, Quaternion eulerAnger, float length, LayerMask layerMask, out RaycastHit2D hit)
	{
		hit = Physics2D.Raycast(origin, eulerAnger * (direction * Vector2.right), length, layerMask);
		if ((bool)hit)
		{
			return true;
		}
		return false;
	}

	public static bool Look(Vector2 origin, Vector2 direction, float angle, float distance, LayerMask layerMask, out RaycastHit2D hit)
	{
		if (LookAround(origin, direction, Quaternion.identity, distance, layerMask, out hit))
		{
			return true;
		}
		float num = angle / 4f;
		for (int i = 0; i < 2; i++)
		{
			if (LookAround(origin, direction, Quaternion.Euler(0f, 0f, -1f * num * (float)(i + 1)), distance, layerMask, out hit) || LookAround(origin, direction, Quaternion.Euler(0f, 0f, num * (float)(i + 1)), distance, layerMask, out hit))
			{
				return true;
			}
		}
		return false;
	}

	private static bool LookAround(Vector2 origin, Vector2 direction, Quaternion eulerAnger, float length, LayerMask layerMask, out RaycastHit2D hit)
	{
		hit = Physics2D.Raycast(origin, eulerAnger * direction, length, layerMask);
		if ((bool)hit)
		{
			return true;
		}
		return false;
	}
}
