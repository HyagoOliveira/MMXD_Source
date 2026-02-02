using System.Text.RegularExpressions;

public class InputFieldHelper
{
	public static bool CheckForEmoji(char addedChar, out char newChar)
	{
		if (new Regex("[\\p{Cs}]|[\\u00a9]|[\\u00ae]|[\\u2000-\\u2e7f]|[\\ud83c[\\ud000-\\udfff]]|[\\ud83d[\\ud000-\\udfff]]|[\\ud83e[\\ud000-\\udfff]]").IsMatch(addedChar.ToString()))
		{
			newChar = '\0';
			return false;
		}
		newChar = addedChar;
		return true;
	}
}
