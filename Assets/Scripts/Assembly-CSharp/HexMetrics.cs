using UnityEngine;

public static class HexMetrics
{
	public const float OuterRadius = 1.03f;

	public const float InnerRadius = 0.8920061f;

	public const float VisualRate = 0.98f;

	public static Vector3[] Corners = new Vector3[7]
	{
		new Vector3(0f, 0f, 1.03f) * 0.98f,
		new Vector3(0.8920061f, 0f, 0.515f) * 0.98f,
		new Vector3(0.8920061f, 0f, -0.515f) * 0.98f,
		new Vector3(0f, 0f, -1.03f) * 0.98f,
		new Vector3(-0.8920061f, 0f, -0.515f) * 0.98f,
		new Vector3(-0.8920061f, 0f, 0.515f) * 0.98f,
		new Vector3(0f, 0f, 1.03f) * 0.98f
	};

	public static int Distance(HexCoordinates cubePos1, HexCoordinates cubePos2)
	{
		return (Mathf.Abs(cubePos1.X - cubePos2.X) + Mathf.Abs(cubePos1.Y - cubePos2.Y) + Mathf.Abs(cubePos1.Z - cubePos2.Z)) / 2;
	}

	public static Vector3 GetPosition(int x, int z)
	{
		return new Vector3(((float)x + (float)z * 0.5f - (float)(z / 2)) * 0.8920061f * 2f, 0f, (float)z * 1.03f * 1.5f);
	}

	public static HexCoordinates[] NearCoordinates(HexCoordinates currentCoordinates)
	{
		return new HexCoordinates[6]
		{
			new HexCoordinates(currentCoordinates.X, currentCoordinates.Z + 1),
			new HexCoordinates(currentCoordinates.X + 1, currentCoordinates.Z),
			new HexCoordinates(currentCoordinates.X + 1, currentCoordinates.Z - 1),
			new HexCoordinates(currentCoordinates.X, currentCoordinates.Z - 1),
			new HexCoordinates(currentCoordinates.X - 1, currentCoordinates.Z),
			new HexCoordinates(currentCoordinates.X - 1, currentCoordinates.Z + 1)
		};
	}
}
