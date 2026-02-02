using System;

[Serializable]
public struct VFactor
{
	public long nom;

	public long den;

	[NonSerialized]
	public static VFactor zero = new VFactor(0L, 1L);

	[NonSerialized]
	public static VFactor one = new VFactor(1L, 1L);

	[NonSerialized]
	public static VFactor pi = new VFactor(31416L, 10000L);

	[NonSerialized]
	public static VFactor twoPi = new VFactor(62832L, 10000L);

	private static long mask_ = long.MaxValue;

	private static long upper_ = 16777215L;

	public int roundInt
	{
		get
		{
			return (int)IntMath.Divide(nom, den);
		}
	}

	public int integer
	{
		get
		{
			return (int)(nom / den);
		}
	}

	public float single
	{
		get
		{
			return (float)((double)nom / (double)den);
		}
	}

	public bool IsPositive
	{
		get
		{
			if (nom == 0L)
			{
				return false;
			}
			bool num = nom > 0;
			bool flag = den > 0;
			return num == flag;
		}
	}

	public bool IsNegative
	{
		get
		{
			if (nom == 0L)
			{
				return false;
			}
			bool num = nom > 0;
			bool flag = den > 0;
			return num ^ flag;
		}
	}

	public bool IsZero
	{
		get
		{
			return nom == 0;
		}
	}

	public VFactor Inverse
	{
		get
		{
			return new VFactor(den, nom);
		}
	}

	public VFactor(long n, long d)
	{
		nom = n;
		den = d;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && GetType() == obj.GetType())
		{
			return this == (VFactor)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return single.ToString();
	}

	public void strip()
	{
		while ((nom & mask_) > upper_ && (den & mask_) > upper_)
		{
			nom >>= 1;
			den >>= 1;
		}
	}

	public static bool operator <(VFactor a, VFactor b)
	{
		long num = a.nom * b.den;
		long num2 = b.nom * a.den;
		if ((b.den > 0) ^ (a.den > 0))
		{
			return num > num2;
		}
		return num < num2;
	}

	public static bool operator >(VFactor a, VFactor b)
	{
		long num = a.nom * b.den;
		long num2 = b.nom * a.den;
		if ((b.den > 0) ^ (a.den > 0))
		{
			return num < num2;
		}
		return num > num2;
	}

	public static bool operator <=(VFactor a, VFactor b)
	{
		long num = a.nom * b.den;
		long num2 = b.nom * a.den;
		if ((b.den > 0) ^ (a.den > 0))
		{
			return num >= num2;
		}
		return num <= num2;
	}

	public static bool operator >=(VFactor a, VFactor b)
	{
		long num = a.nom * b.den;
		long num2 = b.nom * a.den;
		if ((b.den > 0) ^ (a.den > 0))
		{
			return num <= num2;
		}
		return num >= num2;
	}

	public static bool operator ==(VFactor a, VFactor b)
	{
		return a.nom * b.den == b.nom * a.den;
	}

	public static bool operator !=(VFactor a, VFactor b)
	{
		return a.nom * b.den != b.nom * a.den;
	}

	public static bool operator <(VFactor a, long b)
	{
		long num = a.nom;
		long num2 = b * a.den;
		if (a.den > 0)
		{
			return num < num2;
		}
		return num > num2;
	}

	public static bool operator >(VFactor a, long b)
	{
		long num = a.nom;
		long num2 = b * a.den;
		if (a.den > 0)
		{
			return num > num2;
		}
		return num < num2;
	}

	public static bool operator <=(VFactor a, long b)
	{
		long num = a.nom;
		long num2 = b * a.den;
		if (a.den > 0)
		{
			return num <= num2;
		}
		return num >= num2;
	}

	public static bool operator >=(VFactor a, long b)
	{
		long num = a.nom;
		long num2 = b * a.den;
		if (a.den > 0)
		{
			return num >= num2;
		}
		return num <= num2;
	}

	public static bool operator ==(VFactor a, long b)
	{
		return a.nom == b * a.den;
	}

	public static bool operator !=(VFactor a, long b)
	{
		return a.nom != b * a.den;
	}

	public static VFactor operator +(VFactor a, VFactor b)
	{
		VFactor result = default(VFactor);
		result.nom = a.nom * b.den + b.nom * a.den;
		result.den = a.den * b.den;
		return result;
	}

	public static VFactor operator +(VFactor a, long b)
	{
		a.nom += b * a.den;
		return a;
	}

	public static VFactor operator -(VFactor a, VFactor b)
	{
		VFactor result = default(VFactor);
		result.nom = a.nom * b.den - b.nom * a.den;
		result.den = a.den * b.den;
		return result;
	}

	public static VFactor operator -(VFactor a, long b)
	{
		a.nom -= b * a.den;
		return a;
	}

	public static VFactor operator *(VFactor a, long b)
	{
		a.nom *= b;
		return a;
	}

	public static VFactor operator /(VFactor a, long b)
	{
		a.den *= b;
		return a;
	}

	public static VInt3 operator *(VInt3 v, VFactor f)
	{
		return IntMath.Divide(v, f.nom, f.den);
	}

	public static VInt2 operator *(VInt2 v, VFactor f)
	{
		return IntMath.Divide(v, f.nom, f.den);
	}

	public static VInt3 operator /(VInt3 v, VFactor f)
	{
		return IntMath.Divide(v, f.den, f.nom);
	}

	public static VInt2 operator /(VInt2 v, VFactor f)
	{
		return IntMath.Divide(v, f.den, f.nom);
	}

	public static int operator *(int i, VFactor f)
	{
		return (int)IntMath.Divide(i * f.nom, f.den);
	}

	public static VFactor operator -(VFactor a)
	{
		a.nom = -a.nom;
		return a;
	}
}
