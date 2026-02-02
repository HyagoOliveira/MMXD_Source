using System;
using System.Globalization;
using enums;

public static class DateTimeExt
{
	public static string ToFullDateString(this DateTime date)
	{
		return date.ToString("yyyy/MM/dd hh:mm tt");
	}

	public static string ToFullDateString(this DateTime date, Language language)
	{
		switch (language)
		{
		default:
			return date.ToFullDateString();
		case Language.English:
			return date.ToString("g", CultureInfo.CreateSpecificCulture("en-US"));
		case Language.ChineseTraditional:
			return date.ToString("g", CultureInfo.CreateSpecificCulture("zh-TW"));
		case Language.Japanese:
			return date.ToString("g", CultureInfo.CreateSpecificCulture("ja-JP"));
		case Language.Thai:
			return date.ToString("g", CultureInfo.CreateSpecificCulture("th-TH"));
		}
	}

	public static string ToTimeString(this DateTime date)
	{
		return date.ToString("HH:mm");
	}

	public static int ToUnixTimeSeconds(this DateTime date)
	{
		return (int)date.Subtract(DateTimeHelper.UNIX_EPOCH_TIME).TotalSeconds;
	}

	public static int ToLocalTimeSeconds(this DateTime date)
	{
		return (int)date.Subtract(DateTimeHelper.UNIX_EPOCH_TIME_LOCAL).TotalSeconds;
	}
}
