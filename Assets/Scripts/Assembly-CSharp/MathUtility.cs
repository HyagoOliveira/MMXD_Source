using System.Collections.Generic;
using System.Linq;

public class MathUtility
{
	public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> source, int length)
	{
		if (length == 1)
		{
			return source.Select((T t) => new List<T> { t });
		}
		return GetPermutations(source, length - 1).SelectMany((IEnumerable<T> list) => source.Where((T type) => !list.Contains(type)), (IEnumerable<T> t1, T t2) => t1.Concat(new List<T> { t2 })).ToList();
	}

	public static List<List<T>> GetPermutations<T>(List<T> source, int length)
	{
		if (length == 1)
		{
			return source.Select((T t) => new List<T> { t }).ToList();
		}
		return GetPermutations(source, length - 1).SelectMany((List<T> list) => source.Where((T type) => !list.Contains(type)).ToList(), (List<T> t1, T t2) => t1.Concat(new List<T> { t2 }).ToList()).ToList();
	}
}
