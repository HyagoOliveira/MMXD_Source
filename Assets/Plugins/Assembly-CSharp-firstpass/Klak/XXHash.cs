namespace Klak
{
	public struct XXHash
	{
		private const uint PRIME32_1 = 2654435761u;

		private const uint PRIME32_2 = 2246822519u;

		private const uint PRIME32_3 = 3266489917u;

		private const uint PRIME32_4 = 668265263u;

		private const uint PRIME32_5 = 374761393u;

		private static int _counter;

		public int seed;

		public static XXHash RandomHash
		{
			get
			{
				return new XXHash((int)GetHash(51966, _counter++));
			}
		}

		private static uint rotl32(uint x, int r)
		{
			return (x << r) | (x >> 32 - r);
		}

		public static uint GetHash(int data, int seed)
		{
			uint num = rotl32((uint)(seed + 374761393 + 4 + data * -1028477379), 17) * 668265263;
			int num2 = (int)(num ^ (num >> 15)) * -2048144777;
			int num3 = (int)((uint)num2 ^ ((uint)num2 >> 13)) * -1028477379;
			return (uint)num3 ^ ((uint)num3 >> 16);
		}

		public XXHash(int seed)
		{
			this.seed = seed;
		}

		public uint GetHash(int data)
		{
			return GetHash(data, seed);
		}

		public int Range(int max, int data)
		{
			return (int)GetHash(data) % max;
		}

		public int Range(int min, int max, int data)
		{
			return (int)GetHash(data) % (max - min) + min;
		}

		public float Value01(int data)
		{
			return (float)GetHash(data) / 4.2949673E+09f;
		}

		public float Range(float min, float max, int data)
		{
			return Value01(data) % (max - min) + min;
		}
	}
}
