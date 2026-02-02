using System.Collections.Generic;
using UnityEngine;

namespace Curve
{
	public class CatmullRomCurve : Curve
	{
		public CatmullRomCurve(List<Vector3> points, bool closed = false)
			: base(points, closed)
		{
		}

		protected override Vector3 GetPoint(float t)
		{
			List<Vector3> list = points;
			int count = list.Count;
			float num = (float)(count - ((!closed) ? 1 : 0)) * t;
			int num2 = Mathf.FloorToInt(num);
			float num3 = num - (float)num2;
			if (closed)
			{
				num2 += ((num2 <= 0) ? ((Mathf.FloorToInt(Mathf.Abs(num2) / list.Count) + 1) * list.Count) : 0);
			}
			else if (num3 == 0f && num2 == count - 1)
			{
				num2 = count - 2;
				num3 = 1f;
			}
			Vector3 v = ((!closed && num2 <= 0) ? (list[0] - list[1] + list[0]) : list[(num2 - 1) % count]);
			Vector3 v2 = list[num2 % count];
			Vector3 v3 = list[(num2 + 1) % count];
			Vector3 v4 = ((!closed && num2 + 2 >= count) ? (list[count - 1] - list[count - 2] + list[count - 1]) : list[(num2 + 2) % count]);
			return new CubicPoly3D(v, v2, v3, v4).Calculate(num3);
		}
	}
}
