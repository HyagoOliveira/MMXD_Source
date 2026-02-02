using UnityEngine;

public class IntMath
{
	public static VFactor atan2(int y, int x)
	{
		int num;
		int num2;
		if (x < 0)
		{
			if (y < 0)
			{
				x = -x;
				y = -y;
				num = 1;
			}
			else
			{
				x = -x;
				num = -1;
			}
			num2 = -31416;
		}
		else
		{
			if (y < 0)
			{
				y = -y;
				num = -1;
			}
			else
			{
				num = 1;
			}
			num2 = 0;
		}
		int dIM = Atan2LookupTable.DIM;
		long num3 = dIM - 1;
		long b = ((x >= y) ? x : y);
		int num4 = (int)Divide(x * num3, b);
		int num5 = (int)Divide(y * num3, b);
		int num6 = Atan2LookupTable.table[num5 * dIM + num4];
		VFactor result = default(VFactor);
		result.nom = (num6 + num2) * num;
		result.den = 10000L;
		return result;
	}

	public static VFactor acos(long nom, long den)
	{
		int value = (int)Divide(nom * AcosLookupTable.HALF_COUNT, den) + AcosLookupTable.HALF_COUNT;
		value = Mathf.Clamp(value, 0, AcosLookupTable.COUNT);
		VFactor result = default(VFactor);
		result.nom = AcosLookupTable.table[value];
		result.den = 10000L;
		return result;
	}

	public static VFactor sin(long nom, long den)
	{
		int index = SinCosLookupTable.getIndex(nom, den);
		return new VFactor(SinCosLookupTable.sin_table[index], SinCosLookupTable.FACTOR);
	}

	public static VFactor cos(long nom, long den)
	{
		int index = SinCosLookupTable.getIndex(nom, den);
		return new VFactor(SinCosLookupTable.cos_table[index], SinCosLookupTable.FACTOR);
	}

	public static void sincos(out VFactor s, out VFactor c, long nom, long den)
	{
		int index = SinCosLookupTable.getIndex(nom, den);
		s = new VFactor(SinCosLookupTable.sin_table[index], SinCosLookupTable.FACTOR);
		c = new VFactor(SinCosLookupTable.cos_table[index], SinCosLookupTable.FACTOR);
	}

	public static void sincos(out VFactor s, out VFactor c, VFactor angle)
	{
		int index = SinCosLookupTable.getIndex(angle.nom, angle.den);
		s = new VFactor(SinCosLookupTable.sin_table[index], SinCosLookupTable.FACTOR);
		c = new VFactor(SinCosLookupTable.cos_table[index], SinCosLookupTable.FACTOR);
	}

	public static int Sign(int i)
	{
		if (i <= 0)
		{
			return -1;
		}
		return 1;
	}

	public static long Divide(long a, long b)
	{
		long num = (long)((ulong)((a ^ b) & long.MinValue) >> 63) * -2L + 1;
		return (a + b / 2 * num) / b;
	}

	public static int Divide(int a, int b)
	{
		int num = (int)((uint)((a ^ b) & int.MinValue) >> 31) * -2 + 1;
		return (a + b / 2 * num) / b;
	}

	public static VInt3 Divide(VInt3 a, long m, long b)
	{
		a.x = (int)Divide(a.x * m, b);
		a.y = (int)Divide(a.y * m, b);
		a.z = (int)Divide(a.z * m, b);
		return a;
	}

	public static VInt2 Divide(VInt2 a, long m, long b)
	{
		a.x = (int)Divide(a.x * m, b);
		a.y = (int)Divide(a.y * m, b);
		return a;
	}

	public static VInt3 Divide(VInt3 a, int b)
	{
		a.x = Divide(a.x, b);
		a.y = Divide(a.y, b);
		a.z = Divide(a.z, b);
		return a;
	}

	public static VInt3 Divide(VInt3 a, long b)
	{
		a.x = (int)Divide(a.x, b);
		a.y = (int)Divide(a.y, b);
		a.z = (int)Divide(a.z, b);
		return a;
	}

	public static VInt2 Divide(VInt2 a, long b)
	{
		a.x = (int)Divide(a.x, b);
		a.y = (int)Divide(a.y, b);
		return a;
	}

	public static uint Sqrt32(uint a)
	{
		uint num = 0u;
		uint num2 = 0u;
		for (int i = 0; i < 16; i++)
		{
			num2 <<= 1;
			num <<= 2;
			num += a >> 30;
			a <<= 2;
			if (num2 < num)
			{
				num2++;
				num -= num2;
				num2++;
			}
		}
		return (num2 >> 1) & 0xFFFFu;
	}

	public static ulong Sqrt64(ulong a)
	{
		ulong num = 0uL;
		ulong num2 = 0uL;
		for (int i = 0; i < 32; i++)
		{
			num2 <<= 1;
			num <<= 2;
			num += a >> 62;
			a <<= 2;
			if (num2 < num)
			{
				num2++;
				num -= num2;
				num2++;
			}
		}
		return (num2 >> 1) & 0xFFFFFFFFu;
	}

	public static long SqrtLong(long a)
	{
		if (a <= 0)
		{
			return 0L;
		}
		if (a <= uint.MaxValue)
		{
			return Sqrt32((uint)a);
		}
		return (long)Sqrt64((ulong)a);
	}

	public static int Sqrt(long a)
	{
		if (a <= 0)
		{
			return 0;
		}
		if (a <= uint.MaxValue)
		{
			return (int)Sqrt32((uint)a);
		}
		return (int)Sqrt64((ulong)a);
	}

	public static long Clamp(long a, long min, long max)
	{
		if (a < min)
		{
			return min;
		}
		if (a > max)
		{
			return max;
		}
		return a;
	}

	public static long Max(long a, long b)
	{
		if (a > b)
		{
			return a;
		}
		return b;
	}

	public static VInt3 Transform(ref VInt3 point, ref VInt3 axis_x, ref VInt3 axis_y, ref VInt3 axis_z, ref VInt3 trans)
	{
		return new VInt3(Divide(axis_x.x * point.x + axis_y.x * point.y + axis_z.x * point.z, 1000) + trans.x, Divide(axis_x.y * point.x + axis_y.y * point.y + axis_z.y * point.z, 1000) + trans.y, Divide(axis_x.z * point.x + axis_y.z * point.y + axis_z.z * point.z, 1000) + trans.z);
	}

	public static VInt3 Transform(VInt3 point, ref VInt3 axis_x, ref VInt3 axis_y, ref VInt3 axis_z, ref VInt3 trans)
	{
		return new VInt3(Divide(axis_x.x * point.x + axis_y.x * point.y + axis_z.x * point.z, 1000) + trans.x, Divide(axis_x.y * point.x + axis_y.y * point.y + axis_z.y * point.z, 1000) + trans.y, Divide(axis_x.z * point.x + axis_y.z * point.y + axis_z.z * point.z, 1000) + trans.z);
	}

	public static VInt3 Transform(ref VInt3 point, ref VInt3 axis_x, ref VInt3 axis_y, ref VInt3 axis_z, ref VInt3 trans, ref VInt3 scale)
	{
		long num = (long)point.x * (long)scale.x;
		long num2 = (long)point.y * (long)scale.x;
		long num3 = (long)point.z * (long)scale.x;
		return new VInt3((int)Divide(axis_x.x * num + axis_y.x * num2 + axis_z.x * num3, 1000000L) + trans.x, (int)Divide(axis_x.y * num + axis_y.y * num2 + axis_z.y * num3, 1000000L) + trans.y, (int)Divide(axis_x.z * num + axis_y.z * num2 + axis_z.z * num3, 1000000L) + trans.z);
	}

	public static VInt3 Transform(ref VInt3 point, ref VInt3 forward, ref VInt3 trans)
	{
		VInt3 axis_y = VInt3.up;
		VInt3 axis_x = VInt3.Cross(VInt3.up, forward);
		return Transform(ref point, ref axis_x, ref axis_y, ref forward, ref trans);
	}

	public static VInt3 Transform(VInt3 point, VInt3 forward, VInt3 trans)
	{
		VInt3 axis_y = VInt3.up;
		VInt3 axis_x = VInt3.Cross(VInt3.up, forward);
		return Transform(ref point, ref axis_x, ref axis_y, ref forward, ref trans);
	}

	public static VInt3 Transform(VInt3 point, VInt3 forward, VInt3 trans, VInt3 scale)
	{
		VInt3 axis_y = VInt3.up;
		VInt3 axis_x = VInt3.Cross(VInt3.up, forward);
		return Transform(ref point, ref axis_x, ref axis_y, ref forward, ref trans, ref scale);
	}

	public static int Lerp(int src, int dest, int nom, int den)
	{
		return Divide(src * den + (dest - src) * nom, den);
	}

	public static long Lerp(long src, long dest, long nom, long den)
	{
		return Divide(src * den + (dest - src) * nom, den);
	}

	public static bool IsPowerOfTwo(int x)
	{
		return (x & (x - 1)) == 0;
	}

	public static int CeilPowerOfTwo(int x)
	{
		x--;
		x |= x >> 1;
		x |= x >> 2;
		x |= x >> 4;
		x |= x >> 8;
		x |= x >> 16;
		x++;
		return x;
	}

	public static void SegvecToLinegen(ref VInt2 segSrc, ref VInt2 segVec, out long a, out long b, out long c)
	{
		a = segVec.y;
		b = -segVec.x;
		c = (long)segVec.x * (long)segSrc.y - (long)segSrc.x * (long)segVec.y;
	}

	private static bool IsPointOnSegment(ref VInt2 segSrc, ref VInt2 segVec, long x, long y)
	{
		long num = x - segSrc.x;
		long num2 = y - segSrc.y;
		if (segVec.x * num + segVec.y * num2 >= 0)
		{
			return num * num + num2 * num2 <= segVec.sqrMagnitudeLong;
		}
		return false;
	}

	public static bool IntersectSegment(ref VInt2 seg1Src, ref VInt2 seg1Vec, ref VInt2 seg2Src, ref VInt2 seg2Vec, out VInt2 interPoint)
	{
		long a;
		long b;
		long c;
		SegvecToLinegen(ref seg1Src, ref seg1Vec, out a, out b, out c);
		long a2;
		long b2;
		long c2;
		SegvecToLinegen(ref seg2Src, ref seg2Vec, out a2, out b2, out c2);
		long num = a * b2 - a2 * b;
		if (num != 0L)
		{
			long num2 = Divide(b * c2 - b2 * c, num);
			long num3 = Divide(a2 * c - a * c2, num);
			bool result = IsPointOnSegment(ref seg1Src, ref seg1Vec, num2, num3) && IsPointOnSegment(ref seg2Src, ref seg2Vec, num2, num3);
			interPoint.x = (int)num2;
			interPoint.y = (int)num3;
			return result;
		}
		interPoint = VInt2.zero;
		return false;
	}

	public static bool PointInPolygon(ref VInt2 pnt, VInt2[] plg)
	{
		if (plg == null || plg.Length < 3)
		{
			return false;
		}
		bool flag = false;
		int num = 0;
		int num2 = plg.Length - 1;
		while (num < plg.Length)
		{
			VInt2 vInt = plg[num];
			VInt2 vInt2 = plg[num2];
			if ((vInt.y <= pnt.y && pnt.y < vInt2.y) || (vInt2.y <= pnt.y && pnt.y < vInt.y))
			{
				int num3 = vInt2.y - vInt.y;
				long num4 = (long)(pnt.y - vInt.y) * (long)(vInt2.x - vInt.x) - (long)(pnt.x - vInt.x) * (long)num3;
				if (num3 > 0)
				{
					if (num4 > 0)
					{
						flag = !flag;
					}
				}
				else if (num4 < 0)
				{
					flag = !flag;
				}
			}
			num2 = num++;
		}
		return flag;
	}

	public static bool SegIntersectPlg(ref VInt2 segSrc, ref VInt2 segVec, VInt2[] plg, out VInt2 nearPoint, out VInt2 projectVec)
	{
		nearPoint = VInt2.zero;
		projectVec = VInt2.zero;
		if (plg == null || plg.Length < 2)
		{
			return false;
		}
		bool result = false;
		long num = -1L;
		int num2 = -1;
		for (int i = 0; i < plg.Length; i++)
		{
			VInt2 seg2Vec = plg[(i + 1) % plg.Length] - plg[i];
			VInt2 interPoint;
			if (IntersectSegment(ref segSrc, ref segVec, ref plg[i], ref seg2Vec, out interPoint))
			{
				long sqrMagnitudeLong = (interPoint - segSrc).sqrMagnitudeLong;
				if (num < 0 || sqrMagnitudeLong < num)
				{
					nearPoint = interPoint;
					num = sqrMagnitudeLong;
					num2 = i;
					result = true;
				}
			}
		}
		if (num2 >= 0)
		{
			VInt2 vInt = plg[(num2 + 1) % plg.Length] - plg[num2];
			VInt2 vInt2 = segSrc + segVec - nearPoint;
			long num3 = (long)vInt2.x * (long)vInt.x + (long)vInt2.y * (long)vInt.y;
			if (num3 < 0)
			{
				num3 = -num3;
				vInt = -vInt;
			}
			long sqrMagnitudeLong2 = vInt.sqrMagnitudeLong;
			projectVec.x = (int)Divide(vInt.x * num3, sqrMagnitudeLong2);
			projectVec.y = (int)Divide(vInt.y * num3, sqrMagnitudeLong2);
		}
		return result;
	}

	public static int Abs(int i)
	{
		if (i <= 0)
		{
			return -i;
		}
		return i;
	}

	public static int Min(int a, int b)
	{
		if (a <= b)
		{
			return a;
		}
		return b;
	}

	public static int Max(int a, int b)
	{
		if (a <= b)
		{
			return b;
		}
		return a;
	}
}
