public static class DecimalExtension
{
	public static decimal Normalize(this decimal value)
	{
		return value / 1.0000000000000000000000000000m;
	}
}
