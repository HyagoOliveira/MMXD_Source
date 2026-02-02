#define RELEASE
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class InputTextUI : OrangeUIBase
{
	[SerializeField]
	private InputField inputField;

	[SerializeField]
	private Text TextTitle;

	private Action<string> m_cb;

	private int inputCharLimit = 10;

	public void Setup(Action<string> p_cb, string t_name, string p_name = "")
	{
		m_cb = p_cb;
		TextTitle.text = t_name;
		inputField.text = p_name;
		inputField.onValidateInput = CheckForEmoji;
		inputCharLimit = inputField.characterLimit;
		inputField.characterLimit = 0;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void SetupCardDeploy(Action<string> p_cb, string t_name, string p_name = "", int i_count = 10)
	{
		m_cb = p_cb;
		TextTitle.text = t_name;
		inputField.text = p_name;
		inputField.onValidateInput = CheckForEmoji;
		inputCharLimit = i_count;
		inputField.characterLimit = 0;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private char CheckForEmoji(string test, int charIndex, char addedChar)
	{
		if (inputCharLimit <= charIndex)
		{
			return '\0';
		}
		if (new Regex("[\\p{Cs}]|[\\u00a9]|[\\u00ae]|[\\u2000-\\u2e7f]|[\\ud83c[\\ud000-\\udfff]]|[\\ud83d[\\ud000-\\udfff]]|[\\ud83e[\\ud000-\\udfff]]").IsMatch(addedChar.ToString()))
		{
			return '\0';
		}
		return addedChar;
	}

	public void OnClickOKBtn()
	{
		if (inputField.text == string.Empty)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ERROR);
			Debug.LogWarning("Empty Name!!!!!");
			return;
		}
		string text = inputField.text;
		text = text.Replace(" ", "").Replace("\u3000", "").Replace(" ", "")
			.Replace("\u00a0", "");
		if (text == string.Empty || text == "")
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_ERROR);
			Debug.LogWarning("Empty Name!!!!!!!");
			return;
		}
		if (OrangeDataReader.Instance.IsContainForbiddenName(inputField.text))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("NAME_ERROR");
			Debug.LogWarning("Contain forbidden name!!!!!");
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			string p_desc = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INPUT_NAME_CONFIRM_TIP"), inputField.text);
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INPUT_NAME_CONFIRM"), p_desc, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				if (m_cb != null)
				{
					m_cb(inputField.text);
				}
				ui.CloseSE = SystemSE.NONE;
				base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				OnClickCloseBtn();
			});
		});
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}
}
