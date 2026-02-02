using System;
using UnityEngine;

[Serializable]
public struct VInt3
{
	public const int Precision = 1000;

	public const float FloatPrecision = 1000f;

	public const float PrecisionFactor = 0.001f;

	public int x;

	public int y;

	public int z;

	public static readonly VInt3 zero = new VInt3(0, 0, 0);

	public static readonly VInt3 one = new VInt3(1000, 1000, 1000);

	public static readonly VInt3 half = new VInt3(500, 500, 500);

	public static readonly VInt3 forward = new VInt3(0, 0, 1000);

	public static readonly VInt3 up = new VInt3(0, 1000, 0);

	public static readonly VInt3 right = new VInt3(1000, 0, 0);

	public static readonly VInt3 signRight = new VInt3(1, 0, 0);

	public static readonly VInt3 signLeft = new VInt3(-1, 0, 0);

	public static readonly VInt3 signUp = new VInt3(0, 1, 0);

	public static readonly VInt3 signDown = new VInt3(0, -1, 0);

	public static readonly VInt3 signForward = new VInt3(0, 0, 1);

	public static readonly VInt3 signBack = new VInt3(0, 0, -1);

	public int this[int i]
	{
		get
		{
			switch (i)
			{
			case 0:
				return x;
			case 1:
				return y;
			default:
				return z;
			}
		}
		set
		{
			switch (i)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			default:
				z = value;
				break;
			}
		}
	}

	public Vector3 vec3
	{
		get
		{
			return new Vector3((float)x * 0.001f, (float)y * 0.001f, (float)z * 0.001f);
		}
	}

	public VInt2 xz
	{
		get
		{
			return new VInt2(x, z);
		}
	}

	public int magnitude
	{
		get
		{
			long num = x;
			long num2 = y;
			long num3 = z;
			return IntMath.Sqrt(num * num + num2 * num2 + num3 * num3);
		}
	}

	public int magnitude2D
	{
		get
		{
			long num = x;
			long num2 = z;
			return IntMath.Sqrt(num * num + num2 * num2);
		}
	}

	public int costMagnitude
	{
		get
		{
			return magnitude;
		}
	}

	public float worldMagnitude
	{
		get
		{
			double num = x;
			double num2 = y;
			double num3 = z;
			return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3) * 0.001f;
		}
	}

	public double sqrMagnitude
	{
		get
		{
			double num = x;
			double num2 = y;
			double num3 = z;
			return num * num + num2 * num2 + num3 * num3;
		}
	}

	public long sqrMagnitudeLong
	{
		get
		{
			long num = x;
			long num2 = y;
			long num3 = z;
			return num * num + num2 * num2 + num3 * num3;
		}
	}

	public long sqrMagnitudeLong2D
	{
		get
		{
			long num = x;
			long num2 = z;
			return num * num + num2 * num2;
		}
	}

	public int unsafeSqrMagnitude
	{
		get
		{
			return x * x + y * y + z * z;
		}
	}

	public VInt3 abs
	{
		get
		{
			return new VInt3(Math.Abs(x), Math.Abs(y), Math.Abs(z));
		}
	}

	[Obsolete("Same implementation as .magnitude")]
	public float safeMagnitude
	{
		get
		{
			double num = x;
			double num2 = y;
			double num3 = z;
			return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
		}
	}

	[Obsolete(".sqrMagnitude is now per default safe (.unsafeSqrMagnitude can be used for unsafe operations)")]
	public float safeSqrMagnitude
	{
		get
		{
			float num = (float)x * 0.001f;
			float num2 = (float)y * 0.001f;
			float num3 = (float)z * 0.001f;
			return num * num + num2 * num2 + num3 * num3;
		}
	}

	public VInt3(Vector3 position)
	{
		x = (int)Math.Round(position.x * 1000f);
		y = (int)Math.Round(position.y * 1000f);
		z = (int)Math.Round(position.z * 1000f);
	}

	public VInt3(int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public VInt3 DivBy2()
	{
		x >>= 1;
		y >>= 1;
		z >>= 1;
		return this;
	}

	public static float Angle(VInt3 lhs, VInt3 rhs)
	{
		double num = (double)Dot(lhs, rhs) / ((double)lhs.magnitude * (double)rhs.magnitude);
		num = ((!(num >= -1.0)) ? (-1.0) : ((num <= 1.0) ? num : 1.0));
		return (float)Math.Acos(num);
	}

	public static VFactor AngleInt(VInt3 lhs, VInt3 rhs)
	{
		long den = (long)lhs.magnitude * (long)rhs.magnitude;
		return IntMath.acos(Dot(ref lhs, ref rhs), den);
	}

	public static int Dot(ref VInt3 lhs, ref VInt3 rhs)
	{
		return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
	}

	public static int Dot(VInt3 lhs, VInt3 rhs)
	{
		return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
	}

	public static long DotLong(VInt3 lhs, VInt3 rhs)
	{
		return (long)lhs.x * (long)rhs.x + (long)lhs.y * (long)rhs.y + (long)lhs.z * (long)rhs.z;
	}

	public static long DotLong(ref VInt3 lhs, ref VInt3 rhs)
	{
		return (long)lhs.x * (long)rhs.x + (long)lhs.y * (long)rhs.y + (long)lhs.z * (long)rhs.z;
	}

	public static long DotXZLong(ref VInt3 lhs, ref VInt3 rhs)
	{
		return (long)lhs.x * (long)rhs.x + (long)lhs.z * (long)rhs.z;
	}

	public static long DotXZLong(VInt3 lhs, VInt3 rhs)
	{
		return (long)lhs.x * (long)rhs.x + (long)lhs.z * (long)rhs.z;
	}

	public static VInt3 Cross(ref VInt3 lhs, ref VInt3 rhs)
	{
		return new VInt3(IntMath.Divide(lhs.y * rhs.z - lhs.z * rhs.y, 1000), IntMath.Divide(lhs.z * rhs.x - lhs.x * rhs.z, 1000), IntMath.Divide(lhs.x * rhs.y - lhs.y * rhs.x, 1000));
	}

	public static VInt3 Cross(VInt3 lhs, VInt3 rhs)
	{
		return new VInt3(IntMath.Divide(lhs.y * rhs.z - lhs.z * rhs.y, 1000), IntMath.Divide(lhs.z * rhs.x - lhs.x * rhs.z, 1000), IntMath.Divide(lhs.x * rhs.y - lhs.y * rhs.x, 1000));
	}

	public static VInt3 MoveTowards(VInt3 from, VInt3 to, int dt)
	{
		if ((to - from).sqrMagnitudeLong <= dt * dt)
		{
			return to;
		}
		return from + (to - from).NormalizeTo(dt);
	}

	public VInt3 Normal2D()
	{
		return new VInt3(z, y, -x);
	}

	public VInt3 NormalizeTo(int newMagn)
	{
		long num = x * 100;
		long num2 = y * 100;
		long num3 = z * 100;
		long num4 = num * num + num2 * num2 + num3 * num3;
		if (num4 == 0L)
		{
			return this;
		}
		long b = IntMath.Sqrt(num4);
		long num5 = newMagn;
		x = (int)IntMath.Divide(num * num5, b);
		y = (int)IntMath.Divide(num2 * num5, b);
		z = (int)IntMath.Divide(num3 * num5, b);
		return this;
	}

	public long Normalize()
	{
		long num = (long)x << 7;
		long num2 = (long)y << 7;
		long num3 = (long)z << 7;
		long num4 = num * num + num2 * num2 + num3 * num3;
		if (num4 == 0L)
		{
			return 0L;
		}
		long num5 = IntMath.Sqrt(num4);
		long num6 = 1000L;
		x = (int)IntMath.Divide(num * num6, num5);
		y = (int)IntMath.Divide(num2 * num6, num5);
		z = (int)IntMath.Divide(num3 * num6, num5);
		return num5 >> 7;
	}

	public VInt3 RotateY(ref VFactor radians)
	{
		VFactor s;
		VFactor c;
		IntMath.sincos(out s, out c, radians.nom, radians.den);
		long num = c.nom * s.den;
		long num2 = c.den * s.nom;
		long b = c.den * s.den;
		VInt3 vInt = default(VInt3);
		vInt.x = (int)IntMath.Divide(x * num + z * num2, b);
		vInt.z = (int)IntMath.Divide(-x * num2 + z * num, b);
		vInt.y = 0;
		return vInt.NormalizeTo(1000);
	}

	public VInt3 RotateY(int degree)
	{
		VFactor s;
		VFactor c;
		IntMath.sincos(out s, out c, 31416 * degree, 1800000L);
		long num = c.nom * s.den;
		long num2 = c.den * s.nom;
		long b = c.den * s.den;
		VInt3 vInt = default(VInt3);
		vInt.x = (int)IntMath.Divide(x * num + z * num2, b);
		vInt.z = (int)IntMath.Divide(-x * num2 + z * num, b);
		vInt.y = 0;
		return vInt.NormalizeTo(1000);
	}

	public override string ToString()
	{
		return "( " + x + ", " + y + ", " + z + ")";
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		VInt3 vInt = (VInt3)o;
		if (x == vInt.x && y == vInt.y)
		{
			return z == vInt.z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (x * 73856093) ^ (y * 19349663) ^ (z * 83492791);
	}

	public static VInt3 Lerp(VInt3 a, VInt3 b, float f)
	{
		return new VInt3(Mathf.RoundToInt((float)a.x * (1f - f)) + Mathf.RoundToInt((float)b.x * f), Mathf.RoundToInt((float)a.y * (1f - f)) + Mathf.RoundToInt((float)b.y * f), Mathf.RoundToInt((float)a.z * (1f - f)) + Mathf.RoundToInt((float)b.z * f));
	}

	public static VInt3 Lerp(VInt3 a, VInt3 b, VFactor f)
	{
		return new VInt3((int)IntMath.Divide((b.x - a.x) * f.nom, f.den) + a.x, (int)IntMath.Divide((b.y - a.y) * f.nom, f.den) + a.y, (int)IntMath.Divide((b.z - a.z) * f.nom, f.den) + a.z);
	}

	public static VInt3 Lerp(VInt3 a, VInt3 b, int factorNom, int factorDen)
	{
		return new VInt3(IntMath.Divide((b.x - a.x) * factorNom, factorDen) + a.x, IntMath.Divide((b.y - a.y) * factorNom, factorDen) + a.y, IntMath.Divide((b.z - a.z) * factorNom, factorDen) + a.z);
	}

	public long XZSqrMagnitude(VInt3 rhs)
	{
		long num = x - rhs.x;
		long num2 = z - rhs.z;
		return num * num + num2 * num2;
	}

	public long XZSqrMagnitude(ref VInt3 rhs)
	{
		long num = x - rhs.x;
		long num2 = z - rhs.z;
		return num * num + num2 * num2;
	}

	public bool IsEqualXZ(VInt3 rhs)
	{
		if (x == rhs.x)
		{
			return z == rhs.z;
		}
		return false;
	}

	public bool IsEqualXZ(ref VInt3 rhs)
	{
		if (x == rhs.x)
		{
			return z == rhs.z;
		}
		return false;
	}

	public static bool operator ==(VInt3 lhs, VInt3 rhs)
	{
		if (lhs.x == rhs.x && lhs.y == rhs.y)
		{
			return lhs.z == rhs.z;
		}
		return false;
	}

	public static bool operator !=(VInt3 lhs, VInt3 rhs)
	{
		if (lhs.x == rhs.x && lhs.y == rhs.y)
		{
			return lhs.z != rhs.z;
		}
		return true;
	}

	public static explicit operator VInt3(Vector3 ob)
	{
		return new VInt3((int)Math.Round(ob.x * 1000f), (int)Math.Round(ob.y * 1000f), (int)Math.Round(ob.z * 1000f));
	}

	public static explicit operator Vector3(VInt3 ob)
	{
		return new Vector3((float)ob.x * 0.001f, (float)ob.y * 0.001f, (float)ob.z * 0.001f);
	}

	public static VInt3 operator -(VInt3 lhs, VInt3 rhs)
	{
		lhs.x -= rhs.x;
		lhs.y -= rhs.y;
		lhs.z -= rhs.z;
		return lhs;
	}

	public static VInt3 operator -(VInt3 lhs)
	{
		lhs.x = -lhs.x;
		lhs.y = -lhs.y;
		lhs.z = -lhs.z;
		return lhs;
	}

	public static VInt3 operator +(VInt3 lhs, VInt3 rhs)
	{
		lhs.x += rhs.x;
		lhs.y += rhs.y;
		lhs.z += rhs.z;
		return lhs;
	}

	public static VInt3 operator *(VInt3 lhs, int rhs)
	{
		lhs.x *= rhs;
		lhs.y *= rhs;
		lhs.z *= rhs;
		return lhs;
	}

	public static VInt3 operator *(VInt3 lhs, float rhs)
	{
		lhs.x = (int)Math.Round((float)lhs.x * rhs);
		lhs.y = (int)Math.Round((float)lhs.y * rhs);
		lhs.z = (int)Math.Round((float)lhs.z * rhs);
		return lhs;
	}

	public static VInt3 operator *(VInt3 lhs, double rhs)
	{
		lhs.x = (int)Math.Round((double)lhs.x * rhs);
		lhs.y = (int)Math.Round((double)lhs.y * rhs);
		lhs.z = (int)Math.Round((double)lhs.z * rhs);
		return lhs;
	}

	public static VInt3 operator *(VInt3 lhs, Vector3 rhs)
	{
		lhs.x = (int)Math.Round((float)lhs.x * rhs.x);
		lhs.y = (int)Math.Round((float)lhs.y * rhs.y);
		lhs.z = (int)Math.Round((float)lhs.z * rhs.z);
		return lhs;
	}

	public static VInt3 operator *(VInt3 lhs, VInt3 rhs)
	{
		lhs.x *= rhs.x;
		lhs.y *= rhs.y;
		lhs.z *= rhs.z;
		return lhs;
	}

	public static VInt3 operator /(VInt3 lhs, float rhs)
	{
		lhs.x = (int)Math.Round((float)lhs.x / rhs);
		lhs.y = (int)Math.Round((float)lhs.y / rhs);
		lhs.z = (int)Math.Round((float)lhs.z / rhs);
		return lhs;
	}

	public static implicit operator string(VInt3 ob)
	{
		return ob.ToString();
	}

	public static VInt3 operator *(VInt3 lhs, VInt rhs)
	{
		ref int reference = ref lhs.x;
		reference *= rhs;
		ref int reference2 = ref lhs.y;
		reference2 *= rhs;
		ref int reference3 = ref lhs.z;
		reference3 *= rhs;
		return lhs;
	}
}
