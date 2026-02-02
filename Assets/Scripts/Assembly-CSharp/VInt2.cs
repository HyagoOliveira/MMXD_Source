using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct VInt2
{
	[JsonProperty("!a")]
	public int x;

	[JsonProperty("!b")]
	public int y;

	[JsonIgnore]
	public static VInt2 zero = default(VInt2);

	[JsonIgnore]
	private static readonly int[] Rotations = new int[16]
	{
		1, 0, 0, 1, 0, 1, -1, 0, -1, 0,
		0, -1, 0, -1, 1, 0
	};

	[JsonIgnore]
	public int sqrMagnitude
	{
		get
		{
			return x * x + y * y;
		}
	}

	[JsonIgnore]
	public long sqrMagnitudeLong
	{
		get
		{
			long num = x;
			long num2 = y;
			return num * num + num2 * num2;
		}
	}

	[JsonIgnore]
	public int magnitude
	{
		get
		{
			long num = x;
			long num2 = y;
			return IntMath.Sqrt(num * num + num2 * num2);
		}
	}

	[JsonIgnore]
	public VInt2 normalized
	{
		get
		{
			VInt2 result = new VInt2(x, y);
			result.Normalize();
			return result;
		}
	}

	[JsonIgnore]
	public Vector2 vec2
	{
		get
		{
			return new Vector2((float)x * 0.001f, (float)y * 0.001f);
		}
	}

	public VInt2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public static int Dot(VInt2 a, VInt2 b)
	{
		return a.x * b.x + a.y * b.y;
	}

	public static long DotLong(ref VInt2 a, ref VInt2 b)
	{
		return (long)a.x * (long)b.x + (long)a.y * (long)b.y;
	}

	public static long DotLong(VInt2 a, VInt2 b)
	{
		return (long)a.x * (long)b.x + (long)a.y * (long)b.y;
	}

	public static long DetLong(ref VInt2 a, ref VInt2 b)
	{
		return (long)a.x * (long)b.y - (long)a.y * (long)b.x;
	}

	public static long DetLong(VInt2 a, VInt2 b)
	{
		return (long)a.x * (long)b.y - (long)a.y * (long)b.x;
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		VInt2 vInt = (VInt2)o;
		if (x == vInt.x)
		{
			return y == vInt.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x * 49157 + y * 98317;
	}

	public static VInt2 Rotate(VInt2 v, int r)
	{
		r %= 4;
		return new VInt2(v.x * Rotations[r * 4] + v.y * Rotations[r * 4 + 1], v.x * Rotations[r * 4 + 2] + v.y * Rotations[r * 4 + 3]);
	}

	public static VInt2 Min(VInt2 a, VInt2 b)
	{
		return new VInt2(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
	}

	public static VInt2 Max(VInt2 a, VInt2 b)
	{
		return new VInt2(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
	}

	public static VInt2 FromInt3XZ(VInt3 o)
	{
		return new VInt2(o.x, o.z);
	}

	public static VInt3 ToInt3XZ(VInt2 o)
	{
		return new VInt3(o.x, 0, o.y);
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}

	public void Min(ref VInt2 r)
	{
		x = Mathf.Min(x, r.x);
		y = Mathf.Min(y, r.y);
	}

	public void Max(ref VInt2 r)
	{
		x = Mathf.Max(x, r.x);
		y = Mathf.Max(y, r.y);
	}

	public void Normalize()
	{
		long num = x * 100;
		long num2 = y * 100;
		long num3 = num * num + num2 * num2;
		if (num3 != 0L)
		{
			long b = IntMath.Sqrt(num3);
			x = (int)IntMath.Divide(num * 1000, b);
			y = (int)IntMath.Divide(num2 * 1000, b);
		}
	}

	public static VInt2 ClampMagnitude(VInt2 v, int maxLength)
	{
		long num = v.sqrMagnitudeLong;
		long num2 = maxLength;
		if (num > num2 * num2)
		{
			long b = IntMath.Sqrt(num);
			int num3 = (int)IntMath.Divide(v.x * maxLength, b);
			int num4 = (int)IntMath.Divide(v.x * maxLength, b);
			return new VInt2(num3, num4);
		}
		return v;
	}

	public static explicit operator Vector2(VInt2 ob)
	{
		return new Vector2((float)ob.x * 0.001f, (float)ob.y * 0.001f);
	}

	public static explicit operator VInt2(Vector2 ob)
	{
		return new VInt2((int)Math.Round(ob.x * 1000f), (int)Math.Round(ob.y * 1000f));
	}

	public static VInt2 operator +(VInt2 a, VInt2 b)
	{
		return new VInt2(a.x + b.x, a.y + b.y);
	}

	public static VInt2 operator -(VInt2 a, VInt2 b)
	{
		return new VInt2(a.x - b.x, a.y - b.y);
	}

	public static bool operator ==(VInt2 a, VInt2 b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(VInt2 a, VInt2 b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public static VInt2 operator -(VInt2 lhs)
	{
		lhs.x = -lhs.x;
		lhs.y = -lhs.y;
		return lhs;
	}

	public static VInt2 operator *(VInt2 lhs, int rhs)
	{
		lhs.x *= rhs;
		lhs.y *= rhs;
		return lhs;
	}

	public VInt2(Vector2 position)
	{
		x = (int)Math.Round(position.x * 1000f);
		y = (int)Math.Round(position.y * 1000f);
	}

	public static VInt2 MoveTowards(VInt2 from, VInt2 to, int dt)
	{
		if ((to - from).sqrMagnitudeLong <= dt * dt)
		{
			return to;
		}
		return from + (to - from).NormalizeTo(dt);
	}

	public VInt2 NormalizeTo(int newMagn)
	{
		long num = x * 100;
		long num2 = y * 100;
		long num3 = num * num + num2 * num2;
		if (num3 == 0L)
		{
			return this;
		}
		long b = IntMath.Sqrt(num3);
		long num4 = newMagn;
		x = (int)IntMath.Divide(num * num4, b);
		y = (int)IntMath.Divide(num2 * num4, b);
		return this;
	}
}
