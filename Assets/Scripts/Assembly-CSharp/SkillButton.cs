#define RELEASE
using UnityEngine;
using UnityEngine.UI;
using enums;

internal class SkillButton : MonoBehaviour
{
	public enum StatusType
	{
		DEFAULT = 0,
		EQUIPPED = 1,
		LOCKED = 2,
		UNLOCKED = 3
	}

	public enum SelectionType
	{
		UNSELECTED = 0,
		SELECTED = 1
	}

	public enum StyleType
	{
		CIRCLE = 0,
		SQUARE = 1,
		WIDE = 2
	}

	[SerializeField]
	private GameObject buttonEquippedCircle;

	[SerializeField]
	private GameObject buttonEquippedSquare;

	[SerializeField]
	private GameObject buttonBGCircle;

	[SerializeField]
	private GameObject buttonBGSquare;

	[SerializeField]
	private GameObject buttonWidePanelOff;

	[SerializeField]
	private GameObject buttonWidePanelOn;

	[SerializeField]
	private OrangeText textLevel;

	[SerializeField]
	private GameObject selectionFrame;

	[SerializeField]
	private GameObject selectionFrameWide;

	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private Image lockIcon;

	[SerializeField]
	private GameObject textGroup;

	[SerializeField]
	private GameObject textGroupWide;

	[SerializeField]
	private OrangeText textSkillNameWide;

	[SerializeField]
	private OrangeText textSkillLevelWide;

	[SerializeField]
	private Image imgRedDot;

	private float buttonRotate;

	private int currentSkillID = 1;

	private int currentSkillLevel;

	private StatusType currentStatus;

	private SelectionType currentSelection;

	private StyleType currentStyle;

	private SKILL_TABLE skillTable;

	private string skillName;

	private string skillDescription;

	private float skillEffect = 100f;

	private bool bEmptySkill;

	private CharacterSkillSlot characterSkillSlot;

	private CharacterSkillEnhanceSlot characterSkillEnchanceSlot;

	private void Start()
	{
	}

	public void Setup(CharacterSkillSlot skillSlot, CharacterSkillEnhanceSlot skillEnhanceSlot, int skillID, int skillLevel, StatusType status)
	{
		characterSkillSlot = skillSlot;
		characterSkillEnchanceSlot = skillEnhanceSlot;
		currentSkillLevel = skillLevel;
		Setup(skillID, status);
		LeanTween.moveLocalY(selectionFrame.gameObject, -30f, 1f).setLoopPingPong().setEaseInCubic();
	}

	public void Setup(int skillID, StatusType status)
	{
		lockIcon.enabled = status == StatusType.LOCKED;
		if (currentStyle == StyleType.CIRCLE)
		{
			buttonEquippedCircle.SetActive(status == StatusType.EQUIPPED);
			buttonBGCircle.SetActive(true);
			buttonEquippedSquare.SetActive(false);
			buttonBGSquare.SetActive(false);
			textGroupWide.SetActive(false);
		}
		else if (currentStyle == StyleType.WIDE)
		{
			buttonEquippedSquare.SetActive(status == StatusType.EQUIPPED);
			buttonEquippedCircle.SetActive(false);
			buttonBGCircle.SetActive(false);
			textGroupWide.SetActive(true);
			buttonWidePanelOff.SetActive(status == StatusType.LOCKED);
			buttonWidePanelOn.SetActive(status != StatusType.LOCKED);
		}
		else
		{
			buttonEquippedSquare.SetActive(status == StatusType.EQUIPPED);
			buttonBGSquare.SetActive(true);
			buttonEquippedCircle.SetActive(false);
			buttonBGCircle.SetActive(false);
			textGroupWide.SetActive(false);
		}
		currentSkillID = skillID;
		currentStatus = status;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(currentSkillID, out skillTable))
		{
			skillIcon.enabled = true;
			skillName = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTable.w_NAME);
			skillDescription = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTable.w_TIP);
			textSkillNameWide.text = skillName;
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(skillTable.s_ICON), skillTable.s_ICON, delegate(Sprite asset)
			{
				if (asset != null)
				{
					skillIcon.sprite = asset;
				}
				else
				{
					Debug.LogWarning("SkillButton.Setup: unable to load sprite " + skillTable.s_ICON);
				}
			});
			if (status == StatusType.LOCKED)
			{
				skillIcon.color = new Color(1f, 1f, 1f, 0.5f);
			}
			int n_EFFECT = skillTable.n_EFFECT;
			int num = 1;
			skillEffect = skillTable.f_EFFECT_X + (float)currentSkillLevel * skillTable.f_EFFECT_Y;
		}
		else
		{
			skillIcon.enabled = false;
			lockIcon.enabled = true;
			bEmptySkill = true;
			buttonWidePanelOff.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
		}
		SetSelected(false);
	}

	public void SetSelected(bool bSelected)
	{
		if (currentStyle == StyleType.WIDE)
		{
			selectionFrameWide.SetActive(bSelected);
			selectionFrame.SetActive(false);
		}
		else
		{
			selectionFrameWide.SetActive(false);
			selectionFrame.SetActive(bSelected);
		}
	}

	public void OverrideText(string text)
	{
		bool flag = string.IsNullOrEmpty(text);
		if (bEmptySkill)
		{
			text = "----";
		}
		if (currentStyle == StyleType.WIDE)
		{
			textGroup.SetActive(false);
			textSkillLevelWide.text = text;
		}
		else
		{
			textGroup.SetActive(!flag);
			textLevel.text = text;
		}
	}

	public string GetSkillName()
	{
		return skillName;
	}

	public string GetSkillDescription()
	{
		return skillDescription;
	}

	public float GetSkillEffect()
	{
		return skillEffect;
	}

	public string GetSkillIcon()
	{
		return skillTable.s_ICON;
	}

	public string GetSkillShowcase()
	{
		return skillTable.s_SHOWCASE;
	}

	public bool IsValidSkill()
	{
		return skillIcon.enabled;
	}

	public int GetSkillID()
	{
		return currentSkillID;
	}

	public bool IsSkillLocked()
	{
		return lockIcon.enabled;
	}

	public CharacterSkillSlot GetCharacterSkillSlot()
	{
		return characterSkillSlot;
	}

	public int GetCharacterSkillLV()
	{
		return currentSkillLevel;
	}

	public CharacterSkillEnhanceSlot GetCharacterSkillEnhanceSlot()
	{
		return characterSkillEnchanceSlot;
	}

	public void SetStyle(StyleType style)
	{
		currentStyle = style;
	}

	public void EnableRedDot(bool bEnable)
	{
		if ((bool)imgRedDot)
		{
			imgRedDot.gameObject.SetActive(bEnable);
		}
	}
}
