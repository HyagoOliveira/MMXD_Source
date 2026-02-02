using System;

public struct Fixed64 : IEquatable<Fixed64>, IComparable<Fixed64>
{
	private readonly long m_rawValue;

	public static readonly decimal Precision = (decimal)new Fixed64(1L);

	public static readonly Fixed64 One = new Fixed64(4096L);

	public static readonly Fixed64 Zero = default(Fixed64);

	public static readonly Fixed64 PI = new Fixed64(12868L);

	public static readonly Fixed64 PITimes2 = new Fixed64(25736L);

	public static readonly Fixed64 PIOver180 = new Fixed64(72L);

	public static readonly Fixed64 Rad2Deg = 12868f * (Fixed64)2L / (Fixed64)360L;

	public static readonly Fixed64 Deg2Rad = (Fixed64)360L / (12868f * (Fixed64)2L);

	private const long Pi = 12868L;

	private const long PiTimes2 = 25736L;

	public const int FRACTIONAL_PLACES = 12;

	private const long ONE = 4096L;

	private static int[] SIN_TABLE = new int[91]
	{
		0, 71, 142, 214, 285, 357, 428, 499, 570, 641,
		711, 781, 851, 921, 990, 1060, 1128, 1197, 1265, 1333,
		1400, 1468, 1534, 1600, 1665, 1730, 1795, 1859, 1922, 1985,
		2048, 2109, 2170, 2230, 2290, 2349, 2407, 2464, 2521, 2577,
		2632, 2686, 2740, 2793, 2845, 2896, 2946, 2995, 3043, 3091,
		3137, 3183, 3227, 3271, 3313, 3355, 3395, 3434, 3473, 3510,
		3547, 3582, 3616, 3649, 3681, 3712, 3741, 3770, 3797, 3823,
		3849, 3872, 3895, 3917, 3937, 3956, 3974, 3991, 4006, 4020,
		4033, 4045, 4056, 4065, 4073, 4080, 4086, 4090, 4093, 4095,
		4096
	};

	public long RawValue
	{
		get
		{
			return m_rawValue;
		}
	}

	public static int Sign(Fixed64 value)
	{
		if (value.m_rawValue >= 0)
		{
			if (value.m_rawValue <= 0)
			{
				return 0;
			}
			return 1;
		}
		return -1;
	}

	public static Fixed64 Abs(Fixed64 value)
	{
		return new Fixed64((value.m_rawValue > 0) ? value.m_rawValue : (-value.m_rawValue));
	}

	public static Fixed64 Floor(Fixed64 value)
	{
		return new Fixed64(value.m_rawValue & -4096);
	}

	public static Fixed64 Ceiling(Fixed64 value)
	{
		if ((value.m_rawValue & 0xFFF) == 0)
		{
			return value;
		}
		return Floor(value) + One;
	}

	public static Fixed64 operator +(Fixed64 x, Fixed64 y)
	{
		return new Fixed64(x.m_rawValue + y.m_rawValue);
	}

	public static Fixed64 operator +(Fixed64 x, int y)
	{
		return x + (Fixed64)y;
	}

	public static Fixed64 operator +(int x, Fixed64 y)
	{
		return (Fixed64)x + y;
	}

	public static Fixed64 operator +(Fixed64 x, float y)
	{
		return x + (Fixed64)y;
	}

	public static Fixed64 operator +(float x, Fixed64 y)
	{
		return (Fixed64)x + y;
	}

	public static Fixed64 operator +(Fixed64 x, double y)
	{
		return x + (Fixed64)y;
	}

	public static Fixed64 operator +(double x, Fixed64 y)
	{
		return (Fixed64)x + y;
	}

	public static Fixed64 operator -(Fixed64 x, Fixed64 y)
	{
		return new Fixed64(x.m_rawValue - y.m_rawValue);
	}

	public static Fixed64 operator -(Fixed64 x, int y)
	{
		return x - (Fixed64)y;
	}

	public static Fixed64 operator -(int x, Fixed64 y)
	{
		return (Fixed64)x - y;
	}

	public static Fixed64 operator -(Fixed64 x, float y)
	{
		return x - (Fixed64)y;
	}

	public static Fixed64 operator -(float x, Fixed64 y)
	{
		return (Fixed64)x + y;
	}

	public static Fixed64 operator -(Fixed64 x, double y)
	{
		return x - (Fixed64)y;
	}

	public static Fixed64 operator -(double x, Fixed64 y)
	{
		return (Fixed64)x - y;
	}

	public static Fixed64 operator *(Fixed64 x, Fixed64 y)
	{
		return new Fixed64(x.m_rawValue * y.m_rawValue >> 12);
	}

	public static Fixed64 operator *(Fixed64 x, int y)
	{
		return x * (Fixed64)y;
	}

	public static Fixed64 operator *(int x, Fixed64 y)
	{
		return (Fixed64)x * y;
	}

	public static Fixed64 operator *(Fixed64 x, float y)
	{
		return x * (Fixed64)y;
	}

	public static Fixed64 operator *(float x, Fixed64 y)
	{
		return (Fixed64)x * y;
	}

	public static Fixed64 operator *(Fixed64 x, double y)
	{
		return x * (Fixed64)y;
	}

	public static Fixed64 operator *(double x, Fixed64 y)
	{
		return (Fixed64)x * y;
	}

	public static Fixed64 operator /(Fixed64 x, Fixed64 y)
	{
		return new Fixed64((x.m_rawValue << 12) / y.m_rawValue);
	}

	public static Fixed64 operator /(Fixed64 x, int y)
	{
		return x / (Fixed64)y;
	}

	public static Fixed64 operator /(int x, Fixed64 y)
	{
		return (Fixed64)x / y;
	}

	public static Fixed64 operator /(Fixed64 x, float y)
	{
		return x / (Fixed64)y;
	}

	public static Fixed64 operator /(float x, Fixed64 y)
	{
		return (Fixed64)x / y;
	}

	public static Fixed64 operator /(double x, Fixed64 y)
	{
		return (Fixed64)x / y;
	}

	public static Fixed64 operator /(Fixed64 x, double y)
	{
		return x / (Fixed64)y;
	}

	public static Fixed64 operator %(Fixed64 x, Fixed64 y)
	{
		return new Fixed64(x.m_rawValue % y.m_rawValue);
	}

	public static Fixed64 operator -(Fixed64 x)
	{
		return new Fixed64(-x.m_rawValue);
	}

	public static bool operator ==(Fixed64 x, Fixed64 y)
	{
		return x.m_rawValue == y.m_rawValue;
	}

	public static bool operator !=(Fixed64 x, Fixed64 y)
	{
		return x.m_rawValue != y.m_rawValue;
	}

	public static bool operator >(Fixed64 x, Fixed64 y)
	{
		return x.m_rawValue > y.m_rawValue;
	}

	public static bool operator >(Fixed64 x, int y)
	{
		return x > (Fixed64)y;
	}

	public static bool operator <(Fixed64 x, int y)
	{
		return x < (Fixed64)y;
	}

	public static bool operator >(Fixed64 x, float y)
	{
		return x > (Fixed64)y;
	}

	public static bool operator <(Fixed64 x, float y)
	{
		return x < (Fixed64)y;
	}

	public static bool operator <(Fixed64 x, Fixed64 y)
	{
		return x.m_rawValue < y.m_rawValue;
	}

	public static bool operator >=(Fixed64 x, Fixed64 y)
	{
		return x.m_rawValue >= y.m_rawValue;
	}

	public static bool operator <=(Fixed64 x, Fixed64 y)
	{
		return x.m_rawValue <= y.m_rawValue;
	}

	public static Fixed64 operator >>(Fixed64 x, int amount)
	{
		return new Fixed64(x.RawValue >> amount);
	}

	public static Fixed64 operator <<(Fixed64 x, int amount)
	{
		return new Fixed64(x.RawValue << amount);
	}

	public static explicit operator Fixed64(long value)
	{
		return new Fixed64(value * 4096);
	}

	public static explicit operator long(Fixed64 value)
	{
		return value.m_rawValue >> 12;
	}

	public static explicit operator Fixed64(float value)
	{
		return new Fixed64((long)(value * 4096f));
	}

	public static explicit operator float(Fixed64 value)
	{
		return (float)value.m_rawValue / 4096f;
	}

	public static explicit operator int(Fixed64 value)
	{
		return (int)(float)value;
	}

	public static explicit operator Fixed64(double value)
	{
		return new Fixed64((long)(value * 4096.0));
	}

	public static explicit operator double(Fixed64 value)
	{
		return (double)value.m_rawValue / 4096.0;
	}

	public static explicit operator Fixed64(decimal value)
	{
		return new Fixed64((long)(value * 4096m));
	}

	public static explicit operator decimal(Fixed64 value)
	{
		return (decimal)value.m_rawValue / 4096m;
	}

	public override bool Equals(object obj)
	{
		if (obj is Fixed64)
		{
			return ((Fixed64)obj).m_rawValue == m_rawValue;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_rawValue.GetHashCode();
	}

	public bool Equals(Fixed64 other)
	{
		return m_rawValue == other.m_rawValue;
	}

	public int CompareTo(Fixed64 other)
	{
		return m_rawValue.CompareTo(other.m_rawValue);
	}

	public override string ToString()
	{
		return ((decimal)this).ToString();
	}

	public string ToStringRound(int round = 2)
	{
		return string.Concat((float)Math.Round((float)this, round));
	}

	public static Fixed64 FromRaw(long rawValue)
	{
		return new Fixed64(rawValue);
	}

	public static Fixed64 Pow(Fixed64 x, int y)
	{
		if (y == 1)
		{
			return x;
		}
		Fixed64 zero = Zero;
		Fixed64 @fixed = Pow(x, y / 2);
		if (((uint)y & (true ? 1u : 0u)) != 0)
		{
			return x * @fixed * @fixed;
		}
		return @fixed * @fixed;
	}

	private Fixed64(long rawValue)
	{
		m_rawValue = rawValue;
	}

	public Fixed64(int value)
	{
		m_rawValue = (long)value * 4096L;
	}

	public static Fixed64 Sqrt(Fixed64 f, int numberIterations)
	{
		if (f.RawValue < 0)
		{
			throw new ArithmeticException("sqrt error");
		}
		if (f.RawValue == 0L)
		{
			return Zero;
		}
		Fixed64 @fixed = f + One >> 1;
		for (int i = 0; i < numberIterations; i++)
		{
			@fixed = @fixed + f / @fixed >> 1;
		}
		if (@fixed.RawValue < 0)
		{
			throw new ArithmeticException("Overflow");
		}
		return @fixed;
	}

	public static Fixed64 Sqrt(Fixed64 f)
	{
		byte numberIterations = 8;
		if (f.RawValue > 409600)
		{
			numberIterations = 12;
		}
		if (f.RawValue > 4096000)
		{
			numberIterations = 16;
		}
		return Sqrt(f, numberIterations);
	}

	public static Fixed64 Sin(Fixed64 i)
	{
		Fixed64 j = (Fixed64)0L;
		while (i < Zero)
		{
			i += FromRaw(25736L);
		}
		if (i > FromRaw(25736L))
		{
			i %= FromRaw(25736L);
		}
		Fixed64 @fixed = i * FromRaw(10L) / FromRaw(714L);
		if (i != Zero && i != FromRaw(6434L) && i != FromRaw(12868L) && i != FromRaw(19302L) && i != FromRaw(25736L))
		{
			j = i * FromRaw(100L) / FromRaw(714L) - @fixed * FromRaw(10L);
		}
		if (@fixed <= FromRaw(90L))
		{
			return sin_lookup(@fixed, j);
		}
		if (@fixed <= FromRaw(180L))
		{
			return sin_lookup(FromRaw(180L) - @fixed, j);
		}
		if (@fixed <= FromRaw(270L))
		{
			return -sin_lookup(@fixed - FromRaw(180L), j);
		}
		return -sin_lookup(FromRaw(360L) - @fixed, j);
	}

	private static Fixed64 sin_lookup(Fixed64 i, Fixed64 j)
	{
		if (j > 0 && j < FromRaw(10L) && i < FromRaw(90L))
		{
			return FromRaw(SIN_TABLE[i.RawValue]) + (FromRaw(SIN_TABLE[i.RawValue + 1]) - FromRaw(SIN_TABLE[i.RawValue])) / FromRaw(10L) * j;
		}
		return FromRaw(SIN_TABLE[i.RawValue]);
	}

	public static Fixed64 Cos(Fixed64 i)
	{
		return Sin(i + FromRaw(6435L));
	}

	public static Fixed64 Tan(Fixed64 i)
	{
		return Sin(i) / Cos(i);
	}

	public static Fixed64 Asin(Fixed64 F)
	{
		bool num = F < 0;
		F = Abs(F);
		if (F > One)
		{
			throw new ArithmeticException("Bad Asin Input:" + (double)F);
		}
		Fixed64 @fixed = FromRaw(35L) * F - FromRaw(146L) * F + FromRaw(346L) * F - FromRaw(877L) * F + FromRaw(6433L);
		Fixed64 fixed2 = PI / (Fixed64)2L - Sqrt(One - F) * @fixed;
		if (!num)
		{
			return fixed2;
		}
		return -fixed2;
	}

	public static Fixed64 Atan(Fixed64 F)
	{
		return Asin(F / Sqrt(One + F * F));
	}

	public static Fixed64 Atan2(Fixed64 F1, Fixed64 F2)
	{
		if (F2.RawValue == 0L && F1.RawValue == 0L)
		{
			return (Fixed64)0L;
		}
		Fixed64 @fixed = (Fixed64)0L;
		if (F2 > 0)
		{
			return Atan(F1 / F2);
		}
		if (F2 < 0)
		{
			if (F1 >= (Fixed64)0L)
			{
				return PI - Atan(Abs(F1 / F2));
			}
			return -(PI - Atan(Abs(F1 / F2)));
		}
		return ((F1 >= (Fixed64)0L) ? PI : (-PI)) / (Fixed64)2L;
	}
}
