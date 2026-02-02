using System;

public static class DateTimeHelper
{
	public static DateTime UNIX_EPOCH_TIME { get; private set; } = new DateTime(1970, 1, 1);


	public static DateTime UNIX_EPOCH_TIME_LOCAL { get; private set; } = UNIX_EPOCH_TIME.ToLocalTime();


	public static DateTime FromEpochLocalTime(double timeSpanSecs)
	{
		return UNIX_EPOCH_TIME_LOCAL + TimeSpan.FromSeconds(timeSpanSecs);
	}
}
