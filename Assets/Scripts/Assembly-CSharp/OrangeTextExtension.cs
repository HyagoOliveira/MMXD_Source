public static class OrangeTextExtension
{
	public static void UpdateText(this OrangeText text, string key)
	{
		text.IsLocalizationText = true;
		text.LocalizationKey = key;
		text.UpdateTextImmediate();
	}
}
