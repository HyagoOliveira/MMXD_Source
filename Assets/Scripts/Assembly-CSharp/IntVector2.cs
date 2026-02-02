using UnityEngine;

public struct IntVector2
{
	public int x;

	public int y;

	public int this[int index]
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

	public static IntVector2 Zero
	{
		get
		{
			return new IntVector2(0, 0);
		}
	}

	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public IntVector2(IntVector2 v)
	{
		x = v.x;
		y = v.y;
	}

	public static IntVector2 operator -(IntVector2 a, int b)
	{
		int num = a.x - b;
		int num2 = a.y - b;
		return new IntVector2(num, num2);
	}

	public static IntVector2 operator +(IntVector2 a, IntVector2 b)
	{
		int num = a.x + b.x;
		int num2 = a.y + b.y;
		return new IntVector2(num, num2);
	}

	public static IntVector2 operator -(IntVector2 a, IntVector2 b)
	{
		int num = a.x - b.x;
		int num2 = a.y - b.y;
		return new IntVector2(num, num2);
	}

	public static IntVector2 operator *(int d, IntVector2 a)
	{
		int num = a.x * d;
		int num2 = a.y * d;
		return new IntVector2(num, num2);
	}

	public static IntVector2 operator *(IntVector2 a, int d)
	{
		int num = a.x * d;
		int num2 = a.y * d;
		return new IntVector2(num, num2);
	}

	public static IntVector2 operator /(IntVector2 a, int d)
	{
		int num = a.x / d;
		int num2 = a.y / d;
		return new IntVector2(num, num2);
	}

	public static bool operator ==(IntVector2 lhs, IntVector2 rhs)
	{
		if (lhs.x == rhs.x)
		{
			return lhs.y == rhs.y;
		}
		return false;
	}

	public static bool operator !=(IntVector2 lhs, IntVector2 rhs)
	{
		if (lhs.x == rhs.x)
		{
			return lhs.y != rhs.y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is IntVector2)
		{
			return (IntVector2)obj == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() + y.GetHashCode();
	}

	public static int SqrMagnitude(IntVector2 a)
	{
		return a.x * a.x + a.y * a.y;
	}

	public static int Distance(IntVector2 a, IntVector2 b)
	{
		return Magnitude(a - b);
	}

	public static int Magnitude(IntVector2 a)
	{
		return SqrMagnitude(a);
	}

	public void Normalize()
	{
		int num = x * x + y * y;
		if (num != 0 && num >= 0)
		{
			num = 1 / num;
			x *= num;
			y *= num;
		}
	}

	public IntVector2 GetNormalized()
	{
		IntVector2 result = new IntVector2(this);
		result.Normalize();
		return result;
	}

	public override string ToString()
	{
		return string.Format("x:{0} y:{1}", x, y);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}
}
