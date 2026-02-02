using UnityEngine;

public struct NormalVector2
{
	public float x;

	public float y;

	public float this[int index]
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

	public static NormalVector2 Zero
	{
		get
		{
			return new NormalVector2(0, 0);
		}
	}

	public NormalVector2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public NormalVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public NormalVector2(NormalVector2 v)
	{
		x = v.x;
		y = v.y;
	}

	public static NormalVector2 operator -(NormalVector2 a, int b)
	{
		float num = a.x - (float)b;
		float num2 = a.y - (float)b;
		return new NormalVector2(num, num2);
	}

	public static NormalVector2 operator +(NormalVector2 a, NormalVector2 b)
	{
		float num = a.x + b.x;
		float num2 = a.y + b.y;
		return new NormalVector2(num, num2);
	}

	public static NormalVector2 operator -(NormalVector2 a, NormalVector2 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		return new NormalVector2(num, num2);
	}

	public static NormalVector2 operator *(float d, NormalVector2 a)
	{
		float num = a.x * d;
		float num2 = a.y * d;
		return new NormalVector2(num, num2);
	}

	public static NormalVector2 operator *(NormalVector2 a, float d)
	{
		float num = a.x * d;
		float num2 = a.y * d;
		return new NormalVector2(num, num2);
	}

	public static NormalVector2 operator /(NormalVector2 a, float d)
	{
		float num = a.x / d;
		float num2 = a.y / d;
		return new NormalVector2(num, num2);
	}

	public static bool operator ==(NormalVector2 lhs, NormalVector2 rhs)
	{
		if (lhs.x == rhs.x)
		{
			return lhs.y == rhs.y;
		}
		return false;
	}

	public static bool operator !=(NormalVector2 lhs, NormalVector2 rhs)
	{
		if (lhs.x == rhs.x)
		{
			return lhs.y != rhs.y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is NormalVector2)
		{
			return (NormalVector2)obj == this;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() + y.GetHashCode();
	}

	public static float SqrMagnitude(NormalVector2 a)
	{
		return a.x * a.x + a.y * a.y;
	}

	public static float Distance(NormalVector2 a, NormalVector2 b)
	{
		return Magnitude(a - b);
	}

	public static float Magnitude(NormalVector2 a)
	{
		return SqrMagnitude(a);
	}

	public void Normalize()
	{
		float num = x * x + y * y;
		if (num != 0f && !(num < 0.0001f))
		{
			num = 1f / num;
			x *= num;
			y *= num;
		}
	}

	public NormalVector2 GetNormalized()
	{
		NormalVector2 result = new NormalVector2(this);
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
