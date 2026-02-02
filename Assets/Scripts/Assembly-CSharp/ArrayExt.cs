using System;

public static class ArrayExt
{
	public static void ForEach<T>(this T[] collection, Action<T> action)
	{
		foreach (T obj in collection)
		{
			if (action != null)
			{
				action(obj);
			}
		}
	}

	public static void ForEach<T>(this T[] collection, Action<T, int> action)
	{
		for (int i = 0; i < collection.Length; i++)
		{
			if (action != null)
			{
				action(collection[i], i);
			}
		}
	}
}
