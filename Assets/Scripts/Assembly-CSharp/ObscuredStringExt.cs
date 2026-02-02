using CodeStage.AntiCheat.ObscuredTypes;

public static class ObscuredStringExt
{
	public static string ToUpper(this ObscuredString s)
	{
		return s.ToString().ToUpper();
	}

	public static string ToLower(this ObscuredString s)
	{
		return s.ToString().ToLower();
	}

	public static string[] Split(this ObscuredString s, params char[] separator)
	{
		return s.ToString().Split(separator);
	}

	public static bool Contains(this ObscuredString s, string value)
	{
		return s.ToString().Contains(value);
	}

	public static bool IsNullString(this ObscuredString s)
	{
		return s.ToString().IsNullString();
	}
}
