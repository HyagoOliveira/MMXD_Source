#define RELEASE
using UnityEngine;
using UnityEngine.UI;
using enums;

public class WantedConditionHelper : MonoBehaviour
{
	[SerializeField]
	private Text _textCondition;

	[SerializeField]
	private GameObject _goStar;

	public bool IsOn
	{
		get
		{
			return _goStar.activeInHierarchy;
		}
		set
		{
			_goStar.SetActive(value);
		}
	}

	public void Setup(WantedGoCondition condition, int paramX, int paramY)
	{
		string p_key = string.Empty;
		string text = paramX.ToString();
		string text2 = paramY.ToString();
		switch (condition)
		{
		case WantedGoCondition.NStarCharacterCount:
			p_key = "GUILD_WANTED_CONDITION_1";
			break;
		case WantedGoCondition.TotalStarCount:
			p_key = "GUILD_WANTED_CONDITION_2";
			break;
		case WantedGoCondition.NStarByCharacterId:
		{
			p_key = "GUILD_WANTED_CONDITION_3";
			CHARACTER_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(paramY, out value))
			{
				text2 = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
				break;
			}
			Debug.LogError(string.Format("Invalid CharaID : {0} of {1}", paramY, "CHARACTER_TABLE_DICT"));
			text2 = "--";
			break;
		}
		case WantedGoCondition.GoCharacterCount:
			p_key = "GUILD_WANTED_CONDITION_4";
			break;
		}
		_textCondition.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_key, text, text2);
	}
}
