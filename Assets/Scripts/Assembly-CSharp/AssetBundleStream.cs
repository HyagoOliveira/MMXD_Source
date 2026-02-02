using System;
using System.IO;
using System.Linq;

public class AssetBundleStream : FileStream
{
	public byte[] keys;

	public AssetBundleStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
		: base(path, mode, access, share, bufferSize, useAsync)
	{
	}

	public AssetBundleStream(string path, FileMode mode)
		: base(path, mode)
	{
	}

	public override int Read(byte[] array, int offset, int count)
	{
		int num = keys.Length;
		int result = base.Read(array, offset, count);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] ^= keys[i % num];
		}
		return result;
	}

	public void Write(byte[] array, int offset, int count, byte[] keys)
	{
		ReadBytes(keys, ref array);
		Write(array, offset, count);
	}

	public static void ReadBytes(byte[] keys, ref byte[] bytes)
	{
		int num = keys.Length;
		for (int i = 0; i < bytes.Length; i++)
		{
			bytes[i] ^= keys[i % num];
		}
	}

	public static byte[] GenerateBytes(uint key)
	{
		uint[] array = (from x in key.ToString()
			select Convert.ToUInt32(x) - 48).ToArray();
		byte[] array2 = new byte[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 7)
			{
				array[i] = 7u;
			}
			array2[i] = (byte)Math.Pow(2.0, array[i]);
		}
		return array2;
	}
}
