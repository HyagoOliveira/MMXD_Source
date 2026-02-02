public class HttpSetting
{
	private static double minimumRequestDuration = 100.0;

	private static int timeout = 30;

	private static int retryLimit = 2;

	public static double MinRequestDuration
	{
		get
		{
			return minimumRequestDuration;
		}
	}

	public static int Timeout
	{
		get
		{
			return timeout;
		}
	}

	public static int RetryLimit
	{
		get
		{
			return retryLimit;
		}
	}
}
