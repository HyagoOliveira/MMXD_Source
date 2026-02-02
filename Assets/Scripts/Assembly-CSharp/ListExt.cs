using System;
using System.Collections.Generic;

public static class ListExt
{
	public static void ForEach<T>(this List<T> collection, Action<T, int> action)
	{
		for (int i = 0; i < collection.Count; i++)
		{
			if (action != null)
			{
				action(collection[i], i);
			}
		}
	}

	public static bool TryGetValue<T>(this List<T> collection, Predicate<T> match, out T value)
	{
		value = collection.Find(match);
		return value != null;
	}
}
