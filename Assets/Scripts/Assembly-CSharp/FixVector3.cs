using UnityEngine;

public struct FixVector3
{
	public Fixed64 x;

	public Fixed64 y;

	public Fixed64 z;

	public Fixed64 this[int index]
	{
		get
		{
			switch (index)
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
			switch (index)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			default:
				y = value;
				break;
			}
		}
	}

	public static FixVector3 Zero
	{
		get
		{
			return new FixVector3(Fixed64.Zero, Fixed64.Zero, Fixed64.Zero);
		}
	}

	public FixVector3(Fixed64 x, Fixed64 y, Fixed64 z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public FixVector3(FixVector3 v)
	{
		x = v.x;
		y = v.y;
		z = v.z;
	}

	public static FixVector3 operator +(FixVector3 a, FixVector3 b)
	{
		Fixed64 @fixed = a.x + b.x;
		Fixed64 fixed2 = a.y + b.y;
		Fixed64 fixed3 = a.z + b.z;
		return new FixVector3(@fixed, fixed2, fixed3);
	}

	public static FixVector3 operator -(FixVector3 a, FixVector3 b)
	{
		Fixed64 @fixed = a.x - b.x;
		Fixed64 fixed2 = a.y - b.y;
		Fixed64 fixed3 = a.z - b.z;
		return new FixVector3(@fixed, fixed2, fixed3);
	}

	public static FixVector3 operator *(Fixed64 d, FixVector3 a)
	{
		Fixed64 @fixed = a.x * d;
		Fixed64 fixed2 = a.y * d;
		Fixed64 fixed3 = a.z * d;
		return new FixVector3(@fixed, fixed2, fixed3);
	}

	public static FixVector3 operator *(FixVector3 a, Fixed64 d)
	{
		Fixed64 @fixed = a.x * d;
		Fixed64 fixed2 = a.y * d;
		Fixed64 fixed3 = a.z * d;
		return new FixVector3(@fixed, fixed2, fixed3);
	}

	public static FixVector3 operator /(FixVector3 a, Fixed64 d)
	{
		Fixed64 @fixed = a.x / d;
		Fixed64 fixed2 = a.y / d;
		Fixed64 fixed3 = a.z / d;
		return new FixVector3(@fixed, fixed2, fixed3);
	}

	public static bool operator ==(FixVector3 lhs, FixVector3 rhs)
	{
		if (lhs.x == rhs.x && lhs.y == rhs.y)
		{
			return lhs.z == rhs.z;
		}
		return false;
	}

	public static bool operator !=(FixVector3 lhs, FixVector3 rhs)
	{
		if (!(lhs.x != rhs.x) && !(lhs.y != rhs.y))
		{
			return lhs.z != rhs.z;
		}
		return true;
	}

	public static Fixed64 SqrMagnitude(FixVector3 a)
	{
		return a.x * a.x + a.y * a.y + a.z * a.z;
	}

	public static Fixed64 Distance(FixVector3 a, FixVector3 b)
	{
		return Magnitude(a - b);
	}

	public static Fixed64 Magnitude(FixVector3 a)
	{
		return Fixed64.Sqrt(SqrMagnitude(a));
	}

	public void Normalize()
	{
		Fixed64 @fixed = x * x + y * y + z * z;
		if (!(@fixed == Fixed64.Zero))
		{
			@fixed = Fixed64.Sqrt(@fixed);
			if (!(@fixed < (Fixed64)0.0001))
			{
				@fixed = 1 / @fixed;
				x *= @fixed;
				y *= @fixed;
				z *= @fixed;
			}
		}
	}

	public FixVector3 GetNormalized()
	{
		FixVector3 result = new FixVector3(this);
		result.Normalize();
		return result;
	}

	public override string ToString()
	{
		return string.Format("x:{0} y:{1} z:{2}", x, y, z);
	}

	public override bool Equals(object obj)
	{
		if (obj is FixVector2)
		{
			return (FixVector3)obj == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
	}

	public static FixVector3 Lerp(FixVector3 from, FixVector3 to, Fixed64 factor)
	{
		return from * (1 - factor) + to * factor;
	}

	public Vector3 ToVector3()
	{
		return new Vector3((float)x, (float)y, (float)z);
	}
}
