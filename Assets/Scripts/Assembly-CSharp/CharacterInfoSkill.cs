#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

internal class CharacterInfoSkill : OrangeUIBase
{
	public enum SKILLTAB_TYPE
	{
		ACTIVE = 0,
		PASSIVE = 1
	}

	[SerializeField]
	private Transform unlockMaterialGroup;

	[SerializeField]
	private GameObject activeRequirement;

	[SerializeField]
	private GameObject passiveRequirement;

	[SerializeField]
	private GameObject activeSkillButtons;

	[SerializeField]
	private GameObject passiveSkillButtons;

	[SerializeField]
	private GameObject selectedSkillIconPos;

	[SerializeField]
	private GameObject[] activeSkill1ButtonPos;

	[SerializeField]
	private GameObject[] activeSkill2ButtonPos;

	[SerializeField]
	private GameObject[] passiveSkillButtonPos;

	[SerializeField]
	private Text selectedSkillName;

	[SerializeField]
	private WrapRectComponent selectedSkillDescription;

	[SerializeField]
	private Text UnlockRequirement;

	[SerializeField]
	private Button UpgradeUnlockButton;

	[SerializeField]
	private Button UpgradeUnlockButton2;

	[SerializeField]
	private Button QUpgradeButton;

	[SerializeField]
	private Text ActiveSkillUpgradeMoney;

	[SerializeField]
	private Text CurrentMoney;

	[SerializeField]
	private Text ActiveSkillUpgradePoint;

	[SerializeField]
	private Text CurrentPoint;

	[SerializeField]
	private GameObject[] passiveSkillMaterialPos;

	[SerializeField]
	private Text PassiveSkillUnlockMoney;

	[SerializeField]
	private GameObject[] activeSkillCards;

	[SerializeField]
	private Image activeSkillGlow1;

	[SerializeField]
	private Image activeSkillGlow2;

	[SerializeField]
	private CanvasGroup insertSlotEffect;

	[SerializeField]
	private CanvasGroup insertEffect;

	[SerializeField]
	private Image skillShowcase;

	[SerializeField]
	private Toggle activeSkill1Toggle;

	[SerializeField]
	private Toggle activeSkill2Toggle;

	[SerializeField]
	private CommonTabParent skillTabParent;

	[SerializeField]
	private Transform requirementGroup;

	[SerializeField]
	private Image imgUnlockedBg;

	private bool[] passiveSkillSelected = new bool[5];

	private Vector3[] activeSkillCardsDefaultPos = new Vector3[3];

	private CharacterInfo characterInfo;

	private CHARACTER_TABLE characterTable;

	private SkillButton selectedSkillButton;

	private int selectedSkillIndex;

	private bool bIsCharacterUnlocked;

	private CharacterInfoUI characterInfoUI;

	private CommonTabParent tabParent;

	private MATERIAL_TABLE passiveSkillUnlockMaterialTable;

	private bool bCardInsertAnimRunning;

	private CharacterSkillSlot m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill1;

	private CharacterSkillEnhanceSlot m_curSkillEnhanceSelected;

	private CharacterSkillSlot m_curSkillSlotEquipped;

	private CharacterSkillEnhanceSlot m_curSkillEnhanceEquipped;

	private List<SkillButton> m_activeSkill1Buttons = new List<SkillButton>();

	private List<SkillButton> m_activeSkill2Buttons = new List<SkillButton>();

	private UpgradeEffect m_upgradeEffect;

	private SKILLTAB_TYPE currentTab;

	private bool bEffectLock;

	public override void SetCanvas(bool enable)
	{
		if (enable)
		{
			Refresh();
		}
		base.SetCanvas(enable);
	}

	public void Setup(CharacterInfo info, SKILLTAB_TYPE skillTab = SKILLTAB_TYPE.ACTIVE)
	{
		activeSkillCardsDefaultPos[0] = activeSkillCards[0].transform.localPosition;
		activeSkillCardsDefaultPos[1] = activeSkillCards[1].transform.localPosition;
		activeSkillCardsDefaultPos[2] = activeSkillCards[2].transform.localPosition;
		characterInfo = info;
		characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.netInfo.CharacterID];
		characterInfoUI = base.transform.parent.GetComponentInChildren<CharacterInfoUI>();
		tabParent = GetComponentInChildren<CommonTabParent>();
		bIsCharacterUnlocked = IsCharacterUnlocked();
		CreateSkillButtons();
		CreatePassiveSkillMaterialButtons();
		currentTab = skillTab;
		if (skillTab == SKILLTAB_TYPE.ACTIVE)
		{
			OnClickActiveSkillTab();
			tabParent.SetDefaultTabIndex(0);
			OnClickActiveSkill1Toggle(true);
		}
		else
		{
			OnClickPassiveSkillTab();
			tabParent.SetDefaultTabIndex(1);
		}
		InitLoopEffects();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void Refresh()
	{
		if (currentTab == SKILLTAB_TYPE.ACTIVE)
		{
			for (int i = 0; i < 4; i++)
			{
				SetupActiveSkillButton(CharacterSkillSlot.ActiveSkill1, (CharacterSkillEnhanceSlot)i, m_activeSkill1Buttons[i]);
				SetupActiveSkillButton(CharacterSkillSlot.ActiveSkill2, (CharacterSkillEnhanceSlot)i, m_activeSkill2Buttons[i]);
			}
			DisplayActiveSkillInfo(selectedSkillButton, selectedSkillIndex);
		}
		else
		{
			DisplayPassiveSkillInfo(selectedSkillButton, selectedSkillIndex);
		}
	}

	private void InitLoopEffects()
	{
		LeanTween.scale(activeSkillGlow1.gameObject, new Vector3(2f, 2f), 4f).setLoopClamp();
		LeanTween.value(activeSkillGlow1.gameObject, 1f, 0f, 4f).setOnUpdate(delegate(float alpha)
		{
			activeSkillGlow1.color = new Color(1f, 1f, 1f, alpha);
		}).setLoopClamp();
		LeanTween.scale(activeSkillGlow2.gameObject, new Vector3(2f, 2f), 4f).setLoopClamp().setDelay(2f);
		LeanTween.value(activeSkillGlow2.gameObject, 1f, 0f, 4f).setOnUpdate(delegate(float alpha)
		{
			activeSkillGlow2.color = new Color(1f, 1f, 1f, alpha);
		}).setLoopClamp()
			.setDelay(2f);
		LeanTween.value(insertSlotEffect.gameObject, 1f, 0.3f, 2f).setOnUpdate(delegate(float alpha)
		{
			insertSlotEffect.alpha = alpha;
		}).setLoopPingPong();
	}

	private void SetupActiveSkillButton(CharacterSkillSlot skillSlot, CharacterSkillEnhanceSlot enhancement, SkillButton skillButton)
	{
		SkillButton.StatusType status = SkillButton.StatusType.LOCKED;
		SkillButton.StyleType style = SkillButton.StyleType.SQUARE;
		int skillID = 0;
		int skillLevel = 0;
		switch (skillSlot)
		{
		case CharacterSkillSlot.ActiveSkill1:
			if (bIsCharacterUnlocked)
			{
				short extra = characterInfo.netSkillDic[CharacterSkillSlot.ActiveSkill1].Extra;
			}
			switch (enhancement)
			{
			case CharacterSkillEnhanceSlot.None:
				skillID = characterTable.n_SKILL1;
				status = SkillButton.StatusType.EQUIPPED;
				style = SkillButton.StyleType.CIRCLE;
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL1:
				skillID = characterTable.n_SKILL1_EX1;
				if (characterInfo.netInfo.Star >= characterTable.n_SKILL1_UNLOCK1)
				{
					status = SkillButton.StatusType.DEFAULT;
				}
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL2:
				skillID = characterTable.n_SKILL1_EX2;
				if (characterInfo.netInfo.Star >= characterTable.n_SKILL1_UNLOCK2)
				{
					status = SkillButton.StatusType.DEFAULT;
				}
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL3:
				skillID = characterTable.n_SKILL1_EX3;
				if (characterInfo.netInfo.Star >= characterTable.n_SKILL1_UNLOCK3)
				{
					status = SkillButton.StatusType.DEFAULT;
				}
				break;
			}
			break;
		case CharacterSkillSlot.ActiveSkill2:
			if (bIsCharacterUnlocked)
			{
				short extra2 = characterInfo.netSkillDic[CharacterSkillSlot.ActiveSkill2].Extra;
			}
			switch (enhancement)
			{
			case CharacterSkillEnhanceSlot.None:
				skillID = characterTable.n_SKILL2;
				status = SkillButton.StatusType.EQUIPPED;
				style = SkillButton.StyleType.CIRCLE;
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL1:
				skillID = characterTable.n_SKILL2_EX1;
				if (characterInfo.netInfo.Star >= characterTable.n_SKILL2_UNLOCK1)
				{
					status = SkillButton.StatusType.DEFAULT;
				}
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL2:
				skillID = characterTable.n_SKILL2_EX2;
				if (characterInfo.netInfo.Star >= characterTable.n_SKILL2_UNLOCK2)
				{
					status = SkillButton.StatusType.DEFAULT;
				}
				break;
			case CharacterSkillEnhanceSlot.EX_SKILL3:
				skillID = characterTable.n_SKILL2_EX3;
				if (characterInfo.netInfo.Star >= characterTable.n_SKILL2_UNLOCK3)
				{
					status = SkillButton.StatusType.DEFAULT;
				}
				break;
			}
			break;
		}
		if (!bIsCharacterUnlocked)
		{
			status = SkillButton.StatusType.LOCKED;
		}
		if (bIsCharacterUnlocked)
		{
			skillLevel = characterInfo.netSkillDic[skillSlot].Level;
		}
		skillButton.SetStyle(style);
		skillButton.Setup(skillSlot, enhancement, skillID, skillLevel, status);
		if (enhancement != 0)
		{
			return;
		}
		switch (skillSlot)
		{
		case CharacterSkillSlot.ActiveSkill1:
			if (bIsCharacterUnlocked)
			{
				int n_SKILL1_LV = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[ManagedSingleton<PlayerHelper>.Instance.GetLV()].n_SKILL1_LV;
				int n_LVMAX2 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[characterTable.n_SKILL1].n_LVMAX;
				string arg2 = ((n_SKILL1_LV <= n_LVMAX2) ? n_SKILL1_LV.ToString() : n_LVMAX2.ToString());
				skillButton.OverrideText(string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_LV"), skillLevel.ToString(), arg2));
			}
			break;
		case CharacterSkillSlot.ActiveSkill2:
			if (bIsCharacterUnlocked)
			{
				int n_SKILL2_LV = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[ManagedSingleton<PlayerHelper>.Instance.GetLV()].n_SKILL2_LV;
				int n_LVMAX = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[characterTable.n_SKILL2].n_LVMAX;
				string arg = ((n_SKILL2_LV <= n_LVMAX) ? n_SKILL2_LV.ToString() : n_LVMAX.ToString());
				skillButton.OverrideText(string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_LV"), skillLevel.ToString(), arg));
			}
			break;
		}
	}

	private void SetupPassiveSkillButton(CharacterSkillSlot skillSlot, SkillButton skillButton)
	{
		SkillButton.StatusType statusType = SkillButton.StatusType.LOCKED;
		if (IsSkillUnlocked(skillSlot))
		{
			statusType = SkillButton.StatusType.UNLOCKED;
		}
		skillButton.SetStyle(SkillButton.StyleType.WIDE);
		switch (skillSlot)
		{
		case CharacterSkillSlot.PassiveSkill1:
			skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, characterTable.n_PASSIVE_1, 1, statusType);
			break;
		case CharacterSkillSlot.PassiveSkill2:
			skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, characterTable.n_PASSIVE_2, 1, statusType);
			break;
		case CharacterSkillSlot.PassiveSkill3:
			skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, characterTable.n_PASSIVE_3, 1, statusType);
			break;
		case CharacterSkillSlot.PassiveSkill4:
			skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, characterTable.n_PASSIVE_4, 1, statusType);
			break;
		case CharacterSkillSlot.PassiveSkill5:
			skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, characterTable.n_PASSIVE_5, 1, statusType);
			break;
		}
		skillButton.EnableRedDot(bIsCharacterUnlocked && statusType == SkillButton.StatusType.LOCKED && IsPassiveSkillUnlockable(skillSlot));
		NetCharacterSkillInfo value;
		if (characterInfo.netSkillDic.TryGetValue(skillSlot, out value))
		{
			string text = string.Format("{0}:{1}", MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PERSONAL_LEVEL"), value.Level);
			skillButton.OverrideText(text);
		}
		else
		{
			string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_LOCKED");
			skillButton.OverrideText(str);
		}
	}

	private bool IsPassiveSkillUnlockable(CharacterSkillSlot skillSlot)
	{
		int num = 0;
		bool flag = false;
		int firstNotEnoughItemID;
		switch (skillSlot)
		{
		case CharacterSkillSlot.PassiveSkill1:
			num = characterTable.n_PASSIVE_UNLOCK1;
			flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(characterTable.n_PASSIVE_MATERIAL1, out firstNotEnoughItemID);
			break;
		case CharacterSkillSlot.PassiveSkill2:
			num = characterTable.n_PASSIVE_UNLOCK2;
			flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(characterTable.n_PASSIVE_MATERIAL2, out firstNotEnoughItemID);
			break;
		case CharacterSkillSlot.PassiveSkill3:
			num = characterTable.n_PASSIVE_UNLOCK3;
			flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(characterTable.n_PASSIVE_MATERIAL3, out firstNotEnoughItemID);
			break;
		case CharacterSkillSlot.PassiveSkill4:
			num = characterTable.n_PASSIVE_UNLOCK4;
			flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(characterTable.n_PASSIVE_MATERIAL4, out firstNotEnoughItemID);
			break;
		case CharacterSkillSlot.PassiveSkill5:
			num = characterTable.n_PASSIVE_UNLOCK5;
			flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(characterTable.n_PASSIVE_MATERIAL5, out firstNotEnoughItemID);
			break;
		case CharacterSkillSlot.PassiveSkill6:
			num = characterTable.n_PASSIVE_UNLOCK6;
			flag = ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(characterTable.n_PASSIVE_MATERIAL6, out firstNotEnoughItemID);
			break;
		}
		return num <= characterInfo.netInfo.Star && flag;
	}

	private void CreateSkillButtons()
	{
		m_activeSkill1Buttons.Clear();
		m_activeSkill2Buttons.Clear();
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("ui/skillbutton", "SkillButton", delegate(GameObject asset)
		{
			if (asset != null)
			{
				GameObject obj = UnityEngine.Object.Instantiate(asset, selectedSkillIconPos.transform, false);
				obj.GetComponent<SkillButton>().SetStyle(SkillButton.StyleType.SQUARE);
				obj.GetComponent<SkillButton>().Setup(10001, SkillButton.StatusType.DEFAULT);
				for (int i = 0; i < 4; i++)
				{
					int indexForDelegate2 = i;
					GameObject gameObject = UnityEngine.Object.Instantiate(asset, activeSkill1ButtonPos[i].transform, false);
					gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
					{
						OnClickActiveSkillButton(indexForDelegate2);
					});
					GameObject gameObject2 = UnityEngine.Object.Instantiate(asset, activeSkill2ButtonPos[i].transform, false);
					gameObject2.GetComponentInChildren<Button>().onClick.AddListener(delegate
					{
						OnClickActiveSkillButton(indexForDelegate2 + 4);
					});
					m_activeSkill1Buttons.Add(gameObject.GetComponent<SkillButton>());
					m_activeSkill2Buttons.Add(gameObject2.GetComponent<SkillButton>());
					SetupActiveSkillButton(CharacterSkillSlot.ActiveSkill1, (CharacterSkillEnhanceSlot)i, gameObject.GetComponent<SkillButton>());
					SetupActiveSkillButton(CharacterSkillSlot.ActiveSkill2, (CharacterSkillEnhanceSlot)i, gameObject2.GetComponent<SkillButton>());
				}
				for (int j = 0; j < 5; j++)
				{
					int indexForDelegate = j;
					GameObject gameObject3 = UnityEngine.Object.Instantiate(asset, passiveSkillButtonPos[j].transform, false);
					gameObject3.GetComponentInChildren<Button>().onClick.AddListener(delegate
					{
						OnClickPassiveSkillButton(indexForDelegate);
					});
					CharacterSkillSlot skillSlot = (CharacterSkillSlot)(3 + j);
					SetupPassiveSkillButton(skillSlot, gameObject3.GetComponent<SkillButton>());
				}
			}
		});
	}

	private void CreatePassiveSkillMaterialButtons()
	{
		GameObject[] array = passiveSkillMaterialPos;
		foreach (GameObject obj in array)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
			{
				UnityEngine.Object.Instantiate(asset, obj.transform);
			});
		}
	}

	public void OnClickActiveSkillTab()
	{
		if (currentTab != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		unlockMaterialGroup.gameObject.SetActive(true);
		activeRequirement.SetActive(true);
		activeSkillButtons.SetActive(true);
		passiveRequirement.SetActive(false);
		passiveSkillButtons.SetActive(false);
		currentTab = SKILLTAB_TYPE.ACTIVE;
		if (m_curSkillSlotSelected == CharacterSkillSlot.ActiveSkill1)
		{
			selectedSkillIndex = 0;
			activeSkill1Toggle.isOn = true;
			OnClickActiveSkillButton(0);
		}
		else
		{
			selectedSkillIndex = 4;
			activeSkill2Toggle.isOn = true;
			OnClickActiveSkillButton(4);
		}
	}

	private void ResetCardPosition()
	{
		activeSkillCards[0].transform.localPosition = activeSkillCardsDefaultPos[0];
		activeSkillCards[1].transform.localPosition = activeSkillCardsDefaultPos[1];
		activeSkillCards[2].transform.localPosition = activeSkillCardsDefaultPos[2];
	}

	public void OnClickActiveSkill1Toggle(bool bOn)
	{
		if (characterInfo != null && bOn)
		{
			NetCharacterSkillInfo value;
			if (characterInfo.netSkillDic.TryGetValue(CharacterSkillSlot.ActiveSkill1, out value))
			{
				m_curSkillSlotEquipped = CharacterSkillSlot.ActiveSkill1;
				m_curSkillEnhanceEquipped = (CharacterSkillEnhanceSlot)value.Extra;
			}
			activeSkill1ButtonPos[0].SetActive(true);
			activeSkill1ButtonPos[1].SetActive(true);
			activeSkill1ButtonPos[2].SetActive(true);
			activeSkill1ButtonPos[3].SetActive(true);
			activeSkill2ButtonPos[0].SetActive(false);
			activeSkill2ButtonPos[1].SetActive(false);
			activeSkill2ButtonPos[2].SetActive(false);
			activeSkill2ButtonPos[3].SetActive(false);
			ResetCardPosition();
			PlaybackActiveCardAnimation(m_curSkillEnhanceEquipped, 1f);
			OnClickActiveSkillButton(0);
		}
	}

	public void OnClickActiveSkill2Toggle(bool bOn)
	{
		if (characterInfo != null && bOn)
		{
			NetCharacterSkillInfo value;
			if (characterInfo.netSkillDic.TryGetValue(CharacterSkillSlot.ActiveSkill2, out value))
			{
				m_curSkillSlotEquipped = CharacterSkillSlot.ActiveSkill2;
				m_curSkillEnhanceEquipped = (CharacterSkillEnhanceSlot)value.Extra;
			}
			activeSkill2ButtonPos[0].SetActive(true);
			activeSkill2ButtonPos[1].SetActive(true);
			activeSkill2ButtonPos[2].SetActive(true);
			activeSkill2ButtonPos[3].SetActive(true);
			activeSkill1ButtonPos[0].SetActive(false);
			activeSkill1ButtonPos[1].SetActive(false);
			activeSkill1ButtonPos[2].SetActive(false);
			activeSkill1ButtonPos[3].SetActive(false);
			ResetCardPosition();
			PlaybackActiveCardAnimation(m_curSkillEnhanceEquipped, 1f);
			OnClickActiveSkillButton(4);
		}
	}

	public void OnClickPassiveSkillTab()
	{
		if (currentTab != SKILLTAB_TYPE.PASSIVE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		unlockMaterialGroup.gameObject.SetActive(true);
		activeRequirement.SetActive(false);
		activeSkillButtons.SetActive(false);
		passiveRequirement.SetActive(true);
		passiveSkillButtons.SetActive(true);
		currentTab = SKILLTAB_TYPE.PASSIVE;
		selectedSkillIndex = 0;
		OnClickPassiveSkillButton(0);
	}

	private bool IsValidSkill(int index)
	{
		if (currentTab == SKILLTAB_TYPE.ACTIVE)
		{
			if (index >= 0 && index < 4)
			{
				return activeSkill1ButtonPos[index].GetComponentInChildren<SkillButton>().IsValidSkill();
			}
			if (index >= 4 && index < 8)
			{
				return activeSkill1ButtonPos[index - 4].GetComponentInChildren<SkillButton>().IsValidSkill();
			}
			return false;
		}
		return passiveSkillButtonPos[index].GetComponentInChildren<SkillButton>().IsValidSkill();
	}

	private void SaveActiveSkillSelection(int index)
	{
		switch (index)
		{
		case 0:
			m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill1;
			m_curSkillEnhanceSelected = CharacterSkillEnhanceSlot.None;
			break;
		case 1:
			m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill1;
			m_curSkillEnhanceSelected = CharacterSkillEnhanceSlot.EX_SKILL1;
			break;
		case 2:
			m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill1;
			m_curSkillEnhanceSelected = CharacterSkillEnhanceSlot.EX_SKILL2;
			break;
		case 3:
			m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill1;
			m_curSkillEnhanceSelected = CharacterSkillEnhanceSlot.EX_SKILL3;
			break;
		case 4:
			m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill2;
			m_curSkillEnhanceSelected = CharacterSkillEnhanceSlot.None;
			break;
		case 5:
			m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill2;
			m_curSkillEnhanceSelected = CharacterSkillEnhanceSlot.EX_SKILL1;
			break;
		case 6:
			m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill2;
			m_curSkillEnhanceSelected = CharacterSkillEnhanceSlot.EX_SKILL2;
			break;
		case 7:
			m_curSkillSlotSelected = CharacterSkillSlot.ActiveSkill2;
			m_curSkillEnhanceSelected = CharacterSkillEnhanceSlot.EX_SKILL3;
			break;
		}
	}

	public CharacterSkillSlot GetSelectedSkillSlot()
	{
		return m_curSkillSlotSelected;
	}

	public void OnClickActiveSkillButton(int index)
	{
		int num = 0;
		SkillButton skillButton = null;
		if (!IsValidSkill(index))
		{
			return;
		}
		if (selectedSkillIndex != index)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		SaveActiveSkillSelection(index);
		GameObject[] array = activeSkill1ButtonPos;
		foreach (GameObject obj in array)
		{
			bool flag = num == index;
			skillButton = obj.GetComponentInChildren<SkillButton>();
			if (skillButton.IsValidSkill())
			{
				skillButton.SetSelected(flag);
				if (flag)
				{
					DisplayActiveSkillInfo(skillButton, index);
					selectedSkillButton = skillButton;
				}
			}
			num++;
		}
		array = activeSkill2ButtonPos;
		foreach (GameObject obj2 in array)
		{
			bool flag2 = num == index;
			skillButton = obj2.GetComponentInChildren<SkillButton>();
			if (skillButton.IsValidSkill())
			{
				skillButton.SetSelected(flag2);
				if (flag2)
				{
					DisplayActiveSkillInfo(skillButton, index);
					selectedSkillButton = skillButton;
				}
			}
			num++;
		}
		if (index != 0 && index != 4 && m_curSkillSlotEquipped == m_curSkillSlotSelected && m_curSkillEnhanceEquipped == m_curSkillEnhanceSelected)
		{
			UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_REMOVE_EQUIP");
			UpgradeUnlockButton.gameObject.SetActive(true);
			UpgradeUnlockButton2.gameObject.SetActive(false);
			QUpgradeButton.gameObject.SetActive(false);
		}
		if (index != 0 && index != 4 && selectedSkillIndex == index)
		{
			OnClickEquipSkill();
		}
		selectedSkillIndex = index;
	}

	private bool CheckActiveSkillUpgradeRequirement()
	{
		List<EXP_TABLE> list = null;
		int num = 0;
		int targetLvl = 0;
		int num2 = 0;
		if (m_curSkillSlotSelected == CharacterSkillSlot.ActiveSkill1 && m_curSkillEnhanceSelected == CharacterSkillEnhanceSlot.None)
		{
			num2 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[characterTable.n_SKILL1].n_LVMAX;
			num = characterInfo.netSkillDic[CharacterSkillSlot.ActiveSkill1].Level;
			targetLvl = num + 1;
			list = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.Values.Where((EXP_TABLE x) => x.n_SKILL1_LV == targetLvl).ToList();
		}
		else if (m_curSkillSlotSelected == CharacterSkillSlot.ActiveSkill2 && m_curSkillEnhanceSelected == CharacterSkillEnhanceSlot.None)
		{
			num2 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[characterTable.n_SKILL2].n_LVMAX;
			num = characterInfo.netSkillDic[CharacterSkillSlot.ActiveSkill2].Level;
			targetLvl = num + 1;
			list = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.Values.Where((EXP_TABLE x) => x.n_SKILL2_LV == targetLvl).ToList();
		}
		if (list.Count > 0)
		{
			if (targetLvl > num2)
			{
				UnlockRequirement.text = "";
				return false;
			}
			UnlockRequirement.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), list[0].n_ID);
			return ManagedSingleton<PlayerHelper>.Instance.GetLV() >= list[0].n_ID;
		}
		OrangeText componentInChildren = UpgradeUnlockButton.GetComponentInChildren<OrangeText>();
		if ((bool)componentInChildren)
		{
			componentInChildren.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REACH_MAX");
		}
		return false;
	}

	private void DisplayActiveSkillInfo(SkillButton skillButton, int index)
	{
		IsSkillUnlocked(skillButton.GetCharacterSkillSlot());
		Color color = new Color(0.98f, 0.24f, 0.23f);
		requirementGroup.gameObject.SetActive(true);
		if (!string.IsNullOrEmpty(skillButton.GetSkillName()))
		{
			selectedSkillName.text = skillButton.GetSkillName();
		}
		if (!string.IsNullOrEmpty(skillButton.GetSkillDescription()))
		{
			selectedSkillDescription.SetText(string.Format(skillButton.GetSkillDescription(), skillButton.GetSkillEffect().ToString("0.00")));
		}
		string text = skillButton.GetSkillShowcase();
		if (!string.IsNullOrEmpty(text))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetShowcase(text), text, delegate(Sprite asset)
			{
				if (asset != null)
				{
					skillShowcase.sprite = asset;
				}
			});
		}
		SkillButton componentInChildren = selectedSkillIconPos.GetComponentInChildren<SkillButton>();
		if (index == 0 || index == 4)
		{
			UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_LVUP");
			UpgradeUnlockButton.gameObject.SetActive(false);
			UpgradeUnlockButton2.gameObject.SetActive(true);
			QUpgradeButton.gameObject.SetActive(true);
			unlockMaterialGroup.gameObject.SetActive(true);
			componentInChildren.SetStyle(SkillButton.StyleType.CIRCLE);
			if (IsCharacterUnlocked())
			{
				short level = characterInfo.netSkillDic[skillButton.GetCharacterSkillSlot()].Level;
				EXP_TABLE eXP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[level];
				ActiveSkillUpgradeMoney.text = eXP_TABLE.n_SKILLUP_MONEY.ToString();
				ActiveSkillUpgradePoint.text = eXP_TABLE.n_SKILLUP_SP.ToString();
				bool flag = CheckActiveSkillUpgradeRequirement();
				requirementGroup.gameObject.SetActive(flag);
				imgUnlockedBg.gameObject.SetActive(!flag);
				bool flag2 = ManagedSingleton<PlayerHelper>.Instance.GetSkillPoint() >= eXP_TABLE.n_SKILLUP_SP && ManagedSingleton<PlayerHelper>.Instance.GetZenny() >= eXP_TABLE.n_SKILLUP_MONEY;
				UpgradeUnlockButton.interactable = flag && flag2;
				UpgradeUnlockButton2.interactable = flag && flag2;
				QUpgradeButton.interactable = flag && flag2;
				Text unlockRequirement = UnlockRequirement;
				Color color4;
				if (!flag)
				{
					Color color3 = (UnlockRequirement.color = color);
					color4 = color3;
				}
				else
				{
					Color color3 = (UnlockRequirement.color = Color.white);
					color4 = color3;
				}
				unlockRequirement.color = color4;
				Text currentMoney = CurrentMoney;
				Color color6;
				if (ManagedSingleton<PlayerHelper>.Instance.GetZenny() < eXP_TABLE.n_SKILLUP_MONEY)
				{
					Color color3 = (CurrentMoney.color = color);
					color6 = color3;
				}
				else
				{
					Color color3 = (CurrentMoney.color = Color.white);
					color6 = color3;
				}
				currentMoney.color = color6;
				Text currentPoint = CurrentPoint;
				Color color8;
				if (ManagedSingleton<PlayerHelper>.Instance.GetSkillPoint() < eXP_TABLE.n_SKILLUP_SP)
				{
					Color color3 = (CurrentPoint.color = color);
					color8 = color3;
				}
				else
				{
					Color color3 = (CurrentPoint.color = Color.white);
					color8 = color3;
				}
				currentPoint.color = color8;
			}
			else
			{
				UpgradeUnlockButton.interactable = false;
				UpgradeUnlockButton2.interactable = false;
				QUpgradeButton.interactable = false;
				requirementGroup.gameObject.SetActive(false);
				imgUnlockedBg.gameObject.SetActive(true);
			}
			CurrentMoney.text = ManagedSingleton<PlayerHelper>.Instance.GetZenny().ToString();
			CurrentPoint.text = ManagedSingleton<PlayerHelper>.Instance.GetSkillPoint().ToString();
		}
		else
		{
			UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_EQUIP");
			UpgradeUnlockButton.gameObject.SetActive(true);
			UpgradeUnlockButton2.gameObject.SetActive(false);
			QUpgradeButton.gameObject.SetActive(false);
			unlockMaterialGroup.gameObject.SetActive(false);
			componentInChildren.SetStyle(SkillButton.StyleType.SQUARE);
			int num = 0;
			switch (index)
			{
			case 1:
				num = characterTable.n_SKILL1_UNLOCK1;
				break;
			case 2:
				num = characterTable.n_SKILL1_UNLOCK2;
				break;
			case 3:
				num = characterTable.n_SKILL1_UNLOCK3;
				break;
			case 5:
				num = characterTable.n_SKILL2_UNLOCK1;
				break;
			case 6:
				num = characterTable.n_SKILL2_UNLOCK2;
				break;
			case 7:
				num = characterTable.n_SKILL2_UNLOCK3;
				break;
			}
			UnlockRequirement.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_CHARA_STAR"), num);
			bool flag3 = num <= characterInfo.netInfo.Star;
			if (flag3)
			{
				UnlockRequirement.color = new Color(1f, 1f, 1f);
			}
			else
			{
				UnlockRequirement.color = new Color(0.98f, 0.24f, 0.23f);
			}
			requirementGroup.gameObject.SetActive(!flag3);
			imgUnlockedBg.gameObject.SetActive(flag3);
			UpgradeUnlockButton.interactable = flag3;
		}
		componentInChildren.Setup(skillButton.GetSkillID(), SkillButton.StatusType.UNLOCKED);
		componentInChildren.OverrideText("");
	}

	private bool IsCharacterUnlocked()
	{
		return characterInfo.netInfo.State == 1;
	}

	public void OnClickPassiveSkillButton(int index)
	{
		SkillButton skillButton = null;
		if (!IsValidSkill(index))
		{
			return;
		}
		if (selectedSkillIndex != index)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		int num = 0;
		GameObject[] array = passiveSkillButtonPos;
		foreach (GameObject obj in array)
		{
			bool flag = num == index;
			skillButton = obj.GetComponentInChildren<SkillButton>();
			skillButton.SetSelected(flag);
			num++;
			if (flag)
			{
				DisplayPassiveSkillInfo(skillButton, index);
				selectedSkillButton = skillButton;
			}
		}
		selectedSkillIndex = index;
	}

	private void DisplayPassiveSkillInfo(SkillButton skillButton, int index)
	{
		requirementGroup.gameObject.SetActive(true);
		if (skillButton.GetSkillName() != null)
		{
			selectedSkillName.text = skillButton.GetSkillName();
		}
		if (skillButton.GetSkillDescription() != null)
		{
			selectedSkillDescription.SetText(string.Format(skillButton.GetSkillDescription(), skillButton.GetSkillEffect()));
		}
		SkillButton componentInChildren = selectedSkillIconPos.GetComponentInChildren<SkillButton>();
		componentInChildren.SetStyle(SkillButton.StyleType.SQUARE);
		componentInChildren.Setup(skillButton.GetSkillID(), SkillButton.StatusType.DEFAULT);
		componentInChildren.OverrideText("");
		int num = 0;
		switch (index)
		{
		case 0:
			num = characterTable.n_PASSIVE_UNLOCK1;
			break;
		case 1:
			num = characterTable.n_PASSIVE_UNLOCK2;
			break;
		case 2:
			num = characterTable.n_PASSIVE_UNLOCK3;
			break;
		case 3:
			num = characterTable.n_PASSIVE_UNLOCK4;
			break;
		case 4:
			num = characterTable.n_PASSIVE_UNLOCK5;
			break;
		}
		UnlockRequirement.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_CHARA_STAR"), num);
		if (num > characterInfo.netInfo.Star)
		{
			UnlockRequirement.color = new Color(0.98f, 0.24f, 0.23f);
		}
		else
		{
			UnlockRequirement.color = new Color(1f, 1f, 1f);
		}
		switch (index)
		{
		case 0:
			ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(characterTable.n_PASSIVE_MATERIAL1, out passiveSkillUnlockMaterialTable);
			break;
		case 1:
			ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(characterTable.n_PASSIVE_MATERIAL2, out passiveSkillUnlockMaterialTable);
			break;
		case 2:
			ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(characterTable.n_PASSIVE_MATERIAL3, out passiveSkillUnlockMaterialTable);
			break;
		case 3:
			ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(characterTable.n_PASSIVE_MATERIAL4, out passiveSkillUnlockMaterialTable);
			break;
		case 4:
			ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(characterTable.n_PASSIVE_MATERIAL5, out passiveSkillUnlockMaterialTable);
			break;
		}
		ITEM_TABLE value = null;
		bool flag = true;
		for (int i = 0; i < 4; i++)
		{
			switch (i)
			{
			case 0:
				ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(passiveSkillUnlockMaterialTable.n_MATERIAL_1, out value);
				break;
			case 1:
				ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(passiveSkillUnlockMaterialTable.n_MATERIAL_2, out value);
				break;
			case 2:
				ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(passiveSkillUnlockMaterialTable.n_MATERIAL_3, out value);
				break;
			case 3:
				ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(passiveSkillUnlockMaterialTable.n_MATERIAL_4, out value);
				break;
			}
			CommonIconBase componentInChildren2 = passiveSkillMaterialPos[i].GetComponentInChildren<CommonIconBase>();
			componentInChildren2.transform.parent.gameObject.SetActive((value != null) ? true : false);
			if (value != null)
			{
				flag &= componentInChildren2.SetupMaterialEx(passiveSkillUnlockMaterialTable.n_ID, i, OnClickMaterialIcon);
			}
		}
		PassiveSkillUnlockMoney.text = passiveSkillUnlockMaterialTable.n_MONEY.ToString();
		flag &= passiveSkillUnlockMaterialTable.n_MONEY <= ManagedSingleton<PlayerHelper>.Instance.GetZenny();
		flag &= num <= characterInfo.netInfo.Star;
		UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_UNLOCK");
		UpgradeUnlockButton.gameObject.SetActive(true);
		UpgradeUnlockButton2.gameObject.SetActive(false);
		QUpgradeButton.gameObject.SetActive(false);
		if (bIsCharacterUnlocked)
		{
			bool flag2 = IsSkillUnlocked(skillButton.GetCharacterSkillSlot());
			UpgradeUnlockButton.interactable = !flag2 && flag;
			requirementGroup.gameObject.SetActive(!flag2);
			imgUnlockedBg.gameObject.SetActive(flag2);
			if (flag2)
			{
				UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REACH_MAX");
			}
		}
		else
		{
			UpgradeUnlockButton.interactable = false;
			requirementGroup.gameObject.SetActive(false);
			imgUnlockedBg.gameObject.SetActive(true);
		}
	}

	private void OnClickMaterialIcon(int p_idx)
	{
		ITEM_TABLE itemTable = null;
		bool flag = false;
		int itemNeeded = 0;
		switch (p_idx)
		{
		case 0:
			flag = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(passiveSkillUnlockMaterialTable.n_MATERIAL_1, out itemTable);
			itemNeeded = passiveSkillUnlockMaterialTable.n_MATERIAL_MOUNT1;
			break;
		case 1:
			flag = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(passiveSkillUnlockMaterialTable.n_MATERIAL_2, out itemTable);
			itemNeeded = passiveSkillUnlockMaterialTable.n_MATERIAL_MOUNT2;
			break;
		case 2:
			flag = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(passiveSkillUnlockMaterialTable.n_MATERIAL_3, out itemTable);
			itemNeeded = passiveSkillUnlockMaterialTable.n_MATERIAL_MOUNT3;
			break;
		case 3:
			flag = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(passiveSkillUnlockMaterialTable.n_MATERIAL_4, out itemTable);
			itemNeeded = passiveSkillUnlockMaterialTable.n_MATERIAL_MOUNT4;
			break;
		}
		if (!flag)
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
		{
			ui.Setup(itemTable, itemNeeded);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				if ((bool)this)
				{
					Refresh();
				}
			});
		});
	}

	private bool IsSkillUnlocked(CharacterSkillSlot skillSlot, CharacterSkillEnhanceSlot skillEnhanceSlot = CharacterSkillEnhanceSlot.None)
	{
		if (skillEnhanceSlot == CharacterSkillEnhanceSlot.None && (skillSlot == CharacterSkillSlot.ActiveSkill1 || skillSlot == CharacterSkillSlot.ActiveSkill2))
		{
			return true;
		}
		if (skillSlot == CharacterSkillSlot.ActiveSkill1 && skillEnhanceSlot == CharacterSkillEnhanceSlot.EX_SKILL1)
		{
			return characterInfo.netInfo.Star >= characterTable.n_SKILL1_UNLOCK1;
		}
		if (skillSlot == CharacterSkillSlot.ActiveSkill1 && skillEnhanceSlot == CharacterSkillEnhanceSlot.EX_SKILL2)
		{
			return characterInfo.netInfo.Star >= characterTable.n_SKILL1_UNLOCK2;
		}
		if (skillSlot == CharacterSkillSlot.ActiveSkill1 && skillEnhanceSlot == CharacterSkillEnhanceSlot.EX_SKILL3)
		{
			return characterInfo.netInfo.Star >= characterTable.n_SKILL1_UNLOCK3;
		}
		if (skillSlot == CharacterSkillSlot.ActiveSkill2 && skillEnhanceSlot == CharacterSkillEnhanceSlot.EX_SKILL1)
		{
			return characterInfo.netInfo.Star >= characterTable.n_SKILL2_UNLOCK1;
		}
		if (skillSlot == CharacterSkillSlot.ActiveSkill2 && skillEnhanceSlot == CharacterSkillEnhanceSlot.EX_SKILL2)
		{
			return characterInfo.netInfo.Star >= characterTable.n_SKILL2_UNLOCK2;
		}
		if (skillSlot == CharacterSkillSlot.ActiveSkill2 && skillEnhanceSlot == CharacterSkillEnhanceSlot.EX_SKILL3)
		{
			return characterInfo.netInfo.Star >= characterTable.n_SKILL2_UNLOCK3;
		}
		NetCharacterSkillInfo value = null;
		if (characterInfo.netSkillDic.TryGetValue(skillSlot, out value))
		{
			return true;
		}
		return false;
	}

	private void PlaybackInsertEffect()
	{
		LeanTween.value(base.gameObject, 1f, 0f, 1f).setOnUpdate(delegate(float alpha)
		{
			if ((bool)insertEffect)
			{
				insertEffect.alpha = alpha;
			}
		});
	}

	public void PlaybackActiveCardAnimation(CharacterSkillEnhanceSlot enhanceSlot, float timePassedNormalized = 0f, Callback p_cb = null)
	{
		bCardInsertAnimRunning = true;
		switch (enhanceSlot)
		{
		case CharacterSkillEnhanceSlot.EX_SKILL1:
			LeanTween.moveLocalX(activeSkillCards[0].gameObject, -170f, 0.2f).setOnComplete((Action)delegate
			{
				LeanTween.moveLocalY(activeSkillCards[0].gameObject, 100f, 0.2f).setOnComplete((Action)delegate
				{
					LeanTween.moveLocalY(activeSkillCards[0].gameObject, 120f, 0.1f).setOnComplete((Action)delegate
					{
						bCardInsertAnimRunning = false;
						p_cb.CheckTargetToInvoke();
					}).setPassed(0.1f * timePassedNormalized);
				}).setPassed(0.2f * timePassedNormalized);
			}).setPassed(0.2f * timePassedNormalized);
			break;
		case CharacterSkillEnhanceSlot.EX_SKILL2:
			LeanTween.moveLocalY(activeSkillCards[1].gameObject, 100f, 0.2f).setOnComplete((Action)delegate
			{
				LeanTween.moveLocalY(activeSkillCards[1].gameObject, 120f, 0.1f).setOnComplete((Action)delegate
				{
					bCardInsertAnimRunning = false;
					p_cb.CheckTargetToInvoke();
				}).setPassed(0.1f * timePassedNormalized);
			}).setPassed(0.2f * timePassedNormalized);
			break;
		case CharacterSkillEnhanceSlot.EX_SKILL3:
			LeanTween.moveLocalX(activeSkillCards[2].gameObject, -170f, 0.2f).setOnComplete((Action)delegate
			{
				LeanTween.moveLocalY(activeSkillCards[2].gameObject, 100f, 0.2f).setOnComplete((Action)delegate
				{
					LeanTween.moveLocalY(activeSkillCards[2].gameObject, 120f, 0.1f).setOnComplete((Action)delegate
					{
						bCardInsertAnimRunning = false;
						p_cb.CheckTargetToInvoke();
					}).setPassed(0.1f * timePassedNormalized);
				}).setPassed(0.2f * timePassedNormalized);
			}).setPassed(0.2f * timePassedNormalized);
			break;
		default:
			p_cb.CheckTargetToInvoke();
			break;
		}
	}

	public void PlaybackActiveCardAnimationReverse(CharacterSkillEnhanceSlot enhanceSlot, float timePassedNormalized = 0f, Callback p_cb = null)
	{
		bCardInsertAnimRunning = true;
		switch (enhanceSlot)
		{
		case CharacterSkillEnhanceSlot.EX_SKILL1:
			LeanTween.moveLocalY(activeSkillCards[0].gameObject, 311f, 0.2f).setOnComplete((Action)delegate
			{
				LeanTween.moveLocalX(activeSkillCards[0].gameObject, -328f, 0.2f).setOnComplete((Action)delegate
				{
					bCardInsertAnimRunning = false;
					p_cb.CheckTargetToInvoke();
				}).setPassed(0.2f * timePassedNormalized);
			}).setPassed(0.2f * timePassedNormalized);
			break;
		case CharacterSkillEnhanceSlot.EX_SKILL2:
			LeanTween.moveLocalY(activeSkillCards[1].gameObject, 311f, 0.2f).setOnComplete((Action)delegate
			{
				bCardInsertAnimRunning = false;
				p_cb.CheckTargetToInvoke();
			}).setPassed(0.2f * timePassedNormalized);
			break;
		case CharacterSkillEnhanceSlot.EX_SKILL3:
			LeanTween.moveLocalY(activeSkillCards[2].gameObject, 311f, 0.2f).setOnComplete((Action)delegate
			{
				LeanTween.moveLocalX(activeSkillCards[2].gameObject, -12f, 0.2f).setOnComplete((Action)delegate
				{
					bCardInsertAnimRunning = false;
					p_cb.CheckTargetToInvoke();
				}).setPassed(0.2f * timePassedNormalized);
			}).setPassed(0.2f * timePassedNormalized);
			break;
		default:
			p_cb.CheckTargetToInvoke();
			break;
		}
	}

	public void OnClickQuickSkillUp()
	{
		if (currentTab != 0 || selectedSkillButton.IsSkillLocked())
		{
			return;
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_SkillQUp", delegate(CharacterInfoSkillQUp ui)
		{
			ui.Setup(characterInfo.netSkillDic[selectedSkillButton.GetCharacterSkillSlot()]);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				if ((bool)this)
				{
					Refresh();
				}
			});
		});
	}

	public void OnClickEquipSkill()
	{
		if (currentTab == SKILLTAB_TYPE.ACTIVE)
		{
			if (selectedSkillButton.IsSkillLocked())
			{
				return;
			}
			activeSkill1Toggle.interactable = false;
			activeSkill2Toggle.interactable = false;
			UpgradeUnlockButton.interactable = false;
			UpgradeUnlockButton2.interactable = false;
			if (selectedSkillButton.GetCharacterSkillEnhanceSlot() == CharacterSkillEnhanceSlot.None)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
				ManagedSingleton<PlayerNetManager>.Instance.CharacterUpgradeSkillReq(characterInfo.netInfo.CharacterID, selectedSkillButton.GetCharacterSkillSlot(), selectedSkillButton.GetCharacterSkillLV() + 1, delegate
				{
					activeSkill1Toggle.interactable = true;
					activeSkill2Toggle.interactable = true;
					UpgradeUnlockButton.interactable = true;
					UpgradeUnlockButton2.interactable = true;
					PlayUpgradeEffect();
					Refresh();
				});
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
				if (m_curSkillSlotEquipped == m_curSkillSlotSelected && m_curSkillEnhanceEquipped == m_curSkillEnhanceSelected)
				{
					RemoveSkill();
				}
				else
				{
					EquipSkill();
				}
			}
		}
		else
		{
			ManagedSingleton<PlayerNetManager>.Instance.CharacterUnlockSkillSlotReq(characterInfo.netInfo.CharacterID, selectedSkillButton.GetCharacterSkillSlot(), delegate
			{
				Debug.Log("被動技解鎖完成");
				characterInfoUI.CheckCharacterUpgrades();
				characterInfoUI.RefreshSideButtonRedDots();
				characterInfoUI.RefreshQuickSelectBar();
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
				characterInfoUI.RefreshMenuSkillPassive();
			});
		}
	}

	private void EquipSkill()
	{
		ManagedSingleton<PlayerNetManager>.Instance.CharacterSetEnhanceSkillReq(characterInfo.netInfo.CharacterID, selectedSkillButton.GetCharacterSkillSlot(), selectedSkillButton.GetCharacterSkillEnhanceSlot(), delegate
		{
			SetActiveSkillCardsInteractable(false);
			PlaybackActiveCardAnimationReverse(m_curSkillEnhanceEquipped, 0f, delegate
			{
				PlaybackActiveCardAnimation(m_curSkillEnhanceSelected, 0f, delegate
				{
					activeSkill1Toggle.interactable = true;
					activeSkill2Toggle.interactable = true;
					UpgradeUnlockButton.interactable = true;
					PlaybackInsertEffect();
					SetActiveSkillCardsInteractable(true);
					UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_REMOVE_EQUIP");
				});
			});
			m_curSkillSlotEquipped = m_curSkillSlotSelected;
			m_curSkillEnhanceEquipped = m_curSkillEnhanceSelected;
		});
	}

	private void RemoveSkill()
	{
		ManagedSingleton<PlayerNetManager>.Instance.CharacterSetEnhanceSkillReq(characterInfo.netInfo.CharacterID, selectedSkillButton.GetCharacterSkillSlot(), CharacterSkillEnhanceSlot.None, delegate
		{
			SetActiveSkillCardsInteractable(false);
			PlaybackActiveCardAnimationReverse(m_curSkillEnhanceEquipped, 0f, delegate
			{
				activeSkill1Toggle.interactable = true;
				activeSkill2Toggle.interactable = true;
				UpgradeUnlockButton.interactable = true;
				SetActiveSkillCardsInteractable(true);
				UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_EQUIP");
			});
			m_curSkillSlotEquipped = m_curSkillSlotSelected;
			m_curSkillEnhanceEquipped = CharacterSkillEnhanceSlot.None;
		});
	}

	private void SetActiveSkillCardsInteractable(bool bActive)
	{
		if (activeSkillCards.Length >= 3)
		{
			activeSkillCards[0].GetComponentInChildren<Button>().interactable = bActive;
			activeSkillCards[1].GetComponentInChildren<Button>().interactable = bActive;
			activeSkillCards[2].GetComponentInChildren<Button>().interactable = bActive;
		}
	}

	private void PlayUpgradeEffect()
	{
		if (m_upgradeEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgradeeffect", "UpgradeEffect", delegate(GameObject asset)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_upgradeEffect = gameObject.GetComponent<UpgradeEffect>();
				m_upgradeEffect.Play(activeSkill1ButtonPos[0].transform.position);
			});
		}
		else
		{
			m_upgradeEffect.Play(activeSkill1ButtonPos[0].transform.position);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
		}
	}

	public bool IsEffectPlaying()
	{
		bool flag = false;
		flag |= bEffectLock;
		if (m_upgradeEffect != null)
		{
			flag |= m_upgradeEffect.gameObject.activeSelf;
		}
		return flag;
	}
}
