using UnityEngine;

public class OrangeMathf : ManagedSingleton<OrangeMathf>
{
	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public string GetWarpString(string p_inputStr, OrangeText p_text)
	{
		string text = "";
		int num = 0;
		num = ((!p_text.resizeTextForBestFit) ? p_text.fontSize : p_text.resizeTextMaxSize);
		float width = p_text.rectTransform.rect.width;
		Font languageFont = MonoBehaviourSingleton<LocalizationManager>.Instance.LanguageFont;
		languageFont.RequestCharactersInTexture(p_inputStr, num, FontStyle.Normal);
		float num2 = 0f;
		bool flag = false;
		for (int i = 0; i < p_inputStr.Length; i++)
		{
			char c = p_inputStr[i];
			UnityEngine.CharacterInfo info;
			languageFont.GetCharacterInfo(c, out info, num);
			if (!flag)
			{
				flag = c == '<';
				bool flag2 = c == '\n';
				if (!flag2 && num2 + (float)info.advance <= width)
				{
					num2 += (float)info.advance;
				}
				else
				{
					text += (flag2 ? "" : "\n");
					num2 = info.advance;
				}
				text += c;
			}
			else
			{
				if (c == '>')
				{
					flag = false;
				}
				text += c;
			}
		}
		return text;
	}
}
