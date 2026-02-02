using System;

public class SRandom
{
	public static int count;

	private ulong randSeed = 1uL;

	public SRandom(uint seed)
	{
		randSeed = seed;
	}

	public uint Next()
	{
		randSeed = randSeed * 1103515245 + 12345;
		return (uint)(randSeed / 65536);
	}

	public uint Next(uint max)
	{
		return Next() % max;
	}

	public uint Range(uint min, uint max)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("minValue", string.Format("'{0}' cannot be greater than {1}.", min, max));
		}
		uint max2 = max - min;
		return Next(max2) + min;
	}

	public int Next(int max)
	{
		return (int)(Next() % max);
	}

	public int Range(int min, int max)
	{
		count++;
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("minValue", string.Format("'{0}' cannot be greater than {1}.", min, max));
		}
		int max2 = max - min;
		return Next(max2) + min;
	}

	public Fixed64 Range(Fixed64 min, Fixed64 max)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("minValue", string.Format("'{0}' cannot be greater than {1}.", min, max));
		}
		uint max2 = (uint)(max.RawValue - min.RawValue);
		return Fixed64.FromRaw(Next(max2) + min.RawValue);
	}
}
