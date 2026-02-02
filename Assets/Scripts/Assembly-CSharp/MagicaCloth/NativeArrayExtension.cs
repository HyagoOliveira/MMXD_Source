using System.Runtime.CompilerServices;
using Unity.Collections;

namespace MagicaCloth
{
	public static class NativeArrayExtension
	{
		public static void CopyToFast<T, T2>(this NativeArray<T> nativeArray, int startIndex, T2[] array) where T : struct where T2 : struct
		{
			T[] dst = Unsafe.As<T2[], T[]>(ref array);
			NativeArray<T>.Copy(nativeArray, startIndex, dst, 0, array.Length);
		}

		public static void CopyToFast<T>(this NativeArray<T> nativeArray, int startIndex, NativeArray<T> array) where T : struct
		{
			NativeArray<T>.Copy(nativeArray, startIndex, array, 0, array.Length);
		}

		public static void CopyBlock<T>(this NativeArray<T> nativeArray, int sourceIndex, int destinationIndex, int count) where T : struct
		{
			NativeArray<T>.Copy(nativeArray, sourceIndex, nativeArray, destinationIndex, count);
		}

		public static void CopyFromFast<T>(this NativeArray<T> nativeArray, NativeArray<T> array) where T : struct
		{
			NativeArray<T>.Copy(array, 0, nativeArray, 0, array.Length);
		}

		public static void CopyFromFast<T, T2>(this NativeArray<T> nativeArray, int startIndex, T2[] array) where T : struct where T2 : struct
		{
			NativeArray<T>.Copy(Unsafe.As<T2[], T[]>(ref array), 0, nativeArray, startIndex, array.Length);
		}

		public static void SetValue<T>(this NativeArray<T> nativeArray, int startIndex, int count, T value) where T : struct
		{
			int num = 0;
			while (num < count)
			{
				nativeArray[startIndex] = value;
				num++;
				startIndex++;
			}
		}
	}
}
