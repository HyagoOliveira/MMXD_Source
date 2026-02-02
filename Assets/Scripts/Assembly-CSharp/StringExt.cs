public static class StringExt
{
	public static bool IsNullString(this string source)
	{
		if (!string.IsNullOrWhiteSpace(source))
		{
			return source.ToLower() == "null";
		}
		return true;
	}
}
