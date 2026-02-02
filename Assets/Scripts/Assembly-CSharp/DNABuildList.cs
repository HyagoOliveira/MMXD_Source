using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class DNABuildList : OrangeUIBase
{
	private CharacterInfo _characterInfo;

	private int _characterID;

	private DNA_TABLE[] _randomDNATables;

	public GameObject DescriptDataGroup;

	public RectTransform SVContent01;

	public Toggle[] Toggle;

	[SerializeField]
	[ReadOnly]
	private Toggle CurrentToggle;

	public Vector2 spacing = Vector2.zero;

	public Vector3 ToggleSpacing = Vector2.zero;

	private Vector2 currentPos = Vector2.zero;

	private Vector2 lastsizeDelta;

	[BoxGroup("Sound")]
	[Tooltip("切換tab")]
	[SerializeField]
	private SystemSE m_ToggleBtn = SystemSE.CRI_SYSTEMSE_SYS_CURSOR07;

	private bool b_first = true;

	public void Setup(CharacterInfo characterInfo)
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		_characterInfo = characterInfo;
		_characterID = characterInfo.netInfo.CharacterID;
		_randomDNATables = ManagedSingleton<OrangeDataManager>.Instance.DNA_TABLE_DICT.Values.Where((DNA_TABLE x) => x.n_CHARACTER == _characterID && x.n_TYPE == 1).ToArray();
		SwitchToggleBtns(31);
		OneRType0Page(true);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void SwitchToggleBtns(int nSwitchBits)
	{
		if (Toggle.Length == 0)
		{
			return;
		}
		Vector3 localPosition = Toggle[0].transform.localPosition;
		bool flag = false;
		for (int i = 0; i < Toggle.Length; i++)
		{
			if ((nSwitchBits & (1 << i)) != 0)
			{
				Toggle[i].gameObject.SetActive(true);
				Toggle[i].transform.localPosition = localPosition;
				localPosition += ToggleSpacing;
				if (!flag)
				{
					Toggle[i].isOn = true;
					flag = true;
				}
			}
			else
			{
				Toggle[i].gameObject.SetActive(false);
			}
		}
	}

	public void OneRType0Page(bool bIsOn)
	{
		if (_randomDNATables != null && bIsOn)
		{
			if (CurrentToggle != Toggle[0])
			{
				PlaySystemSECheckFirst(m_ToggleBtn);
				CurrentToggle = Toggle[0];
			}
			for (int num = SVContent01.transform.childCount - 1; num >= 0; num--)
			{
				Object.Destroy(SVContent01.transform.GetChild(num).gameObject);
			}
			currentPos = Vector2.zero;
			if (_randomDNATables.Length != 0)
			{
				AddAllBuildData(_randomDNATables[0].n_GROUP);
			}
		}
	}

	public void OneRType1Page(bool bIsOn)
	{
		if (bIsOn)
		{
			if (CurrentToggle != Toggle[1])
			{
				PlaySystemSECheckFirst(m_ToggleBtn);
				CurrentToggle = Toggle[1];
			}
			for (int num = SVContent01.transform.childCount - 1; num >= 0; num--)
			{
				Object.Destroy(SVContent01.transform.GetChild(num).gameObject);
			}
			currentPos = Vector2.zero;
			if (_randomDNATables.Length > 1)
			{
				AddAllBuildData(_randomDNATables[1].n_GROUP);
			}
		}
	}

	public void OneRType2Page(bool bIsOn)
	{
		if (bIsOn)
		{
			if (CurrentToggle != Toggle[2])
			{
				PlaySystemSECheckFirst(m_ToggleBtn);
				CurrentToggle = Toggle[2];
			}
			for (int num = SVContent01.transform.childCount - 1; num >= 0; num--)
			{
				Object.Destroy(SVContent01.transform.GetChild(num).gameObject);
			}
			currentPos = Vector2.zero;
			if (_randomDNATables.Length > 2)
			{
				AddAllBuildData(_randomDNATables[2].n_GROUP);
			}
		}
	}

	public void OneRType3Page(bool bIsOn)
	{
		if (bIsOn)
		{
			if (CurrentToggle != Toggle[3])
			{
				PlaySystemSECheckFirst(m_ToggleBtn);
				CurrentToggle = Toggle[3];
			}
			for (int num = SVContent01.transform.childCount - 1; num >= 0; num--)
			{
				Object.Destroy(SVContent01.transform.GetChild(num).gameObject);
			}
			currentPos = Vector2.zero;
			if (_randomDNATables.Length > 3)
			{
				AddAllBuildData(_randomDNATables[3].n_GROUP);
			}
		}
	}

	public void OneRType4Page(bool bIsOn)
	{
		if (bIsOn)
		{
			if (CurrentToggle != Toggle[4])
			{
				PlaySystemSECheckFirst(m_ToggleBtn);
				CurrentToggle = Toggle[4];
			}
			for (int num = SVContent01.transform.childCount - 1; num >= 0; num--)
			{
				Object.Destroy(SVContent01.transform.GetChild(num).gameObject);
			}
			currentPos = Vector2.zero;
			if (_randomDNATables.Length > 4)
			{
				AddAllBuildData(_randomDNATables[4].n_GROUP);
			}
		}
	}

	private void AddAllBuildData(int nGroupID)
	{
		RANDOMSKILL_TABLE[] array = ManagedSingleton<OrangeDataManager>.Instance.RANDOMSKILL_TABLE_DICT.Values.Where((RANDOMSKILL_TABLE x) => x.n_GROUP == nGroupID).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			SKILL_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(array[i].n_SKILL, out value))
			{
				AddBuildData(value);
			}
		}
	}

	private void AddBuildData(SKILL_TABLE tSKILL_TABLE)
	{
		lastsizeDelta = ((RectTransform)DescriptDataGroup.transform).sizeDelta;
		Transform obj = Object.Instantiate(DescriptDataGroup.transform, SVContent01);
		obj.gameObject.SetActive(true);
		obj.localPosition = currentPos;
		Text component = obj.transform.Find("SkillNameText").GetComponent<Text>();
		Text component2 = obj.transform.Find("SkillMsg").GetComponent<Text>();
		component.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(tSKILL_TABLE.w_NAME);
		float num = 100f;
		int n_EFFECT = tSKILL_TABLE.n_EFFECT;
		if (n_EFFECT == 1)
		{
			num = tSKILL_TABLE.f_EFFECT_X + 0f * tSKILL_TABLE.f_EFFECT_Y;
		}
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(tSKILL_TABLE.w_TIP);
		component2.text = string.Format(l10nValue, num.ToString("0.00"));
		SVContent01.sizeDelta = new Vector2(SVContent01.sizeDelta.x, Mathf.Abs(currentPos.y - lastsizeDelta.y - spacing.y));
		currentPos.y -= lastsizeDelta.y + spacing.y;
	}

	private void PlaySystemSECheckFirst(SystemSE cueid)
	{
		if (b_first)
		{
			b_first = false;
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(cueid);
		}
	}
}
