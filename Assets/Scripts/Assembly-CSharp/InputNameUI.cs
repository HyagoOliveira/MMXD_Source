#define RELEASE
using System.Text.RegularExpressions;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class InputNameUI : OrangeUIBase
{
	[SerializeField]
	private InputField inputField;

	[SerializeField]
	private OrangeText textDesc;
    [System.Obsolete]
    private CallbackObj m_cb;

	private int inputCharLimit = 10;

    [System.Obsolete]
    public void Setup(CallbackObj p_cb, string p_name = "")
	{
		m_cb = p_cb;
		if (p_name == "")
		{
			inputField.text = CreateRandomName();
			textDesc.color = Color.grey;
		}
		else
		{
			inputField.text = p_name;
			textDesc.color = Color.clear;
		}
		inputField.onValidateInput = CheckForEmoji;
		inputCharLimit = inputField.characterLimit;
		inputField.characterLimit = 0;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
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

	private string CreateRandomName()
	{
		int max = ManagedSingleton<OrangeTextDataManager>.Instance.RANDOMNAME_TABLE_DICT.Count / 2 + 1;
		string p_key = "PREFIX_" + Random.Range(1, max);
		string p_key2 = "POSTFIX_" + Random.Range(1, max);
		return ManagedSingleton<OrangeTextDataManager>.Instance.RANDOMNAME_TABLE_DICT.GetL10nValue(p_key) + ManagedSingleton<OrangeTextDataManager>.Instance.RANDOMNAME_TABLE_DICT.GetL10nValue(p_key2);
	}

	public void OnClickRandomBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		inputField.text = CreateRandomName();
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
				OnClickCloseBtn();
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			});
		});
	}

	public override void OnClickCloseBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		base.OnClickCloseBtn();
	}
}
