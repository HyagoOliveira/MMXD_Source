using System.Collections.Generic;

namespace MagicaCloth
{
	public static class DataUtility
	{
		public static uint PackPair(int v0, int v1)
		{
			if (v0 > v1)
			{
				return (uint)(v1 << 16) | ((uint)v0 & 0xFFFFu);
			}
			return (uint)(v0 << 16) | ((uint)v1 & 0xFFFFu);
		}

		public static void UnpackPair(uint pack, out int v0, out int v1)
		{
			v0 = (int)((pack >> 16) & 0xFFFF);
			v1 = (int)(pack & 0xFFFF);
		}

		public static uint Pack16(int hi, int low)
		{
			return (uint)(hi << 16) | ((uint)low & 0xFFFFu);
		}

		public static int Unpack16Hi(uint pack)
		{
			return (int)((pack >> 16) & 0xFFFF);
		}

		public static int Unpack16Low(uint pack)
		{
			return (int)(pack & 0xFFFF);
		}

		public static uint Pack4_28(int hi, int low)
		{
			return (uint)(hi << 28) | ((uint)low & 0xFFFFFFFu);
		}

		public static int Unpack4_28Hi(uint pack)
		{
			return (int)((pack >> 28) & 0xF);
		}

		public static int Unpack4_28Low(uint pack)
		{
			return (int)(pack & 0xFFFFFFF);
		}

		public static uint Pack8_24(int hi, int low)
		{
			return (uint)(hi << 24) | ((uint)low & 0xFFFFFFu);
		}

		public static int Unpack8_24Hi(uint pack)
		{
			return (int)((pack >> 24) & 0xF);
		}

		public static int Unpack8_24Low(uint pack)
		{
			return (int)(pack & 0xFFFFFF);
		}

		public static ulong Pack32(int hi, int low)
		{
			return (ulong)(((long)hi << 32) | (low & 0xFFFFFFFFu));
		}

		public static int Unpack32Hi(ulong pack)
		{
			return (int)((pack >> 32) & 0xFFFFFFFFu);
		}

		public static int Unpack32Low(ulong pack)
		{
			return (int)(pack & 0xFFFFFFFFu);
		}

		public static ulong PackTriple(int v0, int v1, int v2)
		{
			List<ulong> list = new List<ulong>();
			list.Add((ulong)v0);
			list.Add((ulong)v1);
			list.Add((ulong)v2);
			list.Sort();
			return (list[0] << 32) | (list[1] << 16) | list[2];
		}

		public static void UnpackTriple(ulong pack, out int v0, out int v1, out int v2)
		{
			v0 = (int)((pack >> 32) & 0xFFFF);
			v1 = (int)((pack >> 16) & 0xFFFF);
			v2 = (int)(pack & 0xFFFF);
		}

		public static ulong PackQuater(int v0, int v1, int v2, int v3)
		{
			List<ulong> list = new List<ulong>();
			list.Add((ulong)v0);
			list.Add((ulong)v1);
			list.Add((ulong)v2);
			list.Add((ulong)v3);
			list.Sort();
			return (list[0] << 48) | (list[1] << 32) | (list[2] << 16) | list[3];
		}

		public static void UnpackQuater(ulong pack, out int v0, out int v1, out int v2, out int v3)
		{
			v0 = (int)((pack >> 48) & 0xFFFF);
			v1 = (int)((pack >> 32) & 0xFFFF);
			v2 = (int)((pack >> 16) & 0xFFFF);
			v3 = (int)(pack & 0xFFFF);
		}
	}
}
