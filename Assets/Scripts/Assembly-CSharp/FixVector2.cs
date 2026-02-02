using UnityEngine;

public struct FixVector2
{
	public Fixed64 x;

	public Fixed64 y;

	public Fixed64 this[int index]
	{
		get
		{
			if (index != 0)
			{
				return y;
			}
			return x;
		}
		set
		{
			if (index == 0)
			{
				x = value;
			}
			else
			{
				y = value;
			}
		}
	}

	public static FixVector2 Zero
	{
		get
		{
			return new FixVector2(Fixed64.Zero, Fixed64.Zero);
		}
	}

	public FixVector2(Fixed64 x, Fixed64 y)
	{
		this.x = x;
		this.y = y;
	}

	public FixVector2(Fixed64 x, int y)
	{
		this.x = x;
		this.y = (Fixed64)y;
	}

	public FixVector2(int x, int y)
	{
		this.x = (Fixed64)x;
		this.y = (Fixed64)y;
	}

	public FixVector2(FixVector2 v)
	{
		x = v.x;
		y = v.y;
	}

	public static FixVector2 operator -(FixVector2 a, int b)
	{
		Fixed64 @fixed = a.x - b;
		Fixed64 fixed2 = a.y - b;
		return new FixVector2(@fixed, fixed2);
	}

	public static FixVector2 operator +(FixVector2 a, FixVector2 b)
	{
		Fixed64 @fixed = a.x + b.x;
		Fixed64 fixed2 = a.y + b.y;
		return new FixVector2(@fixed, fixed2);
	}

	public static FixVector2 operator -(FixVector2 a, FixVector2 b)
	{
		Fixed64 @fixed = a.x - b.x;
		Fixed64 fixed2 = a.y - b.y;
		return new FixVector2(@fixed, fixed2);
	}

	public static FixVector2 operator *(Fixed64 d, FixVector2 a)
	{
		Fixed64 @fixed = a.x * d;
		Fixed64 fixed2 = a.y * d;
		return new FixVector2(@fixed, fixed2);
	}

	public static FixVector2 operator *(FixVector2 a, Fixed64 d)
	{
		Fixed64 @fixed = a.x * d;
		Fixed64 fixed2 = a.y * d;
		return new FixVector2(@fixed, fixed2);
	}

	public static FixVector2 operator /(FixVector2 a, Fixed64 d)
	{
		Fixed64 @fixed = a.x / d;
		Fixed64 fixed2 = a.y / d;
		return new FixVector2(@fixed, fixed2);
	}

	public static bool operator ==(FixVector2 lhs, FixVector2 rhs)
	{
		if (lhs.x == rhs.x)
		{
			return lhs.y == rhs.y;
		}
		return false;
	}

	public static bool operator !=(FixVector2 lhs, FixVector2 rhs)
	{
		if (!(lhs.x != rhs.x))
		{
			return lhs.y != rhs.y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is FixVector2)
		{
			return (FixVector2)obj == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() + y.GetHashCode();
	}

	public static Fixed64 SqrMagnitude(FixVector2 a)
	{
		return a.x * a.x + a.y * a.y;
	}

	public static Fixed64 Distance(FixVector2 a, FixVector2 b)
	{
		return Magnitude(a - b);
	}

	public static Fixed64 Magnitude(FixVector2 a)
	{
		return Fixed64.Sqrt(SqrMagnitude(a));
	}

	public void Normalize()
	{
		Fixed64 @fixed = x * x + y * y;
		if (!(@fixed == Fixed64.Zero))
		{
			@fixed = Fixed64.Sqrt(@fixed);
			if (!(@fixed < (Fixed64)0.0001))
			{
				@fixed = 1 / @fixed;
				x *= @fixed;
				y *= @fixed;
			}
		}
	}

	public FixVector2 GetNormalized()
	{
		FixVector2 result = new FixVector2(this);
		result.Normalize();
		return result;
	}

	public override string ToString()
	{
		return string.Format("x:{0} y:{1}", x, y);
	}

	public Vector2 ToVector2()
	{
		return new Vector2((float)x, (float)y);
	}
}
