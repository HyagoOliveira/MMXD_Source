using UnityEngine;

namespace Curve
{
	public class CubicPoly3D
	{
		private Vector3 c0;

		private Vector3 c1;

		private Vector3 c2;

		private Vector3 c3;

		public CubicPoly3D(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float tension = 0.5f)
		{
			Vector3 vector = tension * (v2 - v0);
			Vector3 vector2 = tension * (v3 - v1);
			c0 = v1;
			c1 = vector;
			c2 = -3f * v1 + 3f * v2 - 2f * vector - vector2;
			c3 = 2f * v1 - 2f * v2 + vector + vector2;
		}

		public Vector3 Calculate(float t)
		{
			float num = t * t;
			float num2 = num * t;
			return c0 + c1 * t + c2 * num + c3 * num2;
		}
	}
}
