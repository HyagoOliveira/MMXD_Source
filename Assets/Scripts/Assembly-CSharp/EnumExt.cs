using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumExt<T> where T : Enum
{
	public static List<T> ValuesCache { get; private set; }

	static EnumExt()
	{
		ValuesCache = Enum.GetValues(typeof(T)).Cast<T>().ToList();
	}
}
public static class EnumExt
{
	public static T Next<T>(this T value) where T : Enum
	{
		List<T> valuesCache = EnumExt<T>.ValuesCache;
		int num = valuesCache.IndexOf(value);
		num++;
		return valuesCache[Math.Min(num, valuesCache.Count - 1)];
	}

	public static T Previous<T>(this T value) where T : Enum
	{
		List<T> valuesCache = EnumExt<T>.ValuesCache;
		int num = valuesCache.IndexOf(value);
		num--;
		return valuesCache[Math.Max(num, 0)];
	}
}
