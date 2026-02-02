using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

internal class CharacterInfoUpgrade : OrangeUIBase
{
	[SerializeField]
	private GameObject[] starObjectArray;

	[SerializeField]
	private GameObject[] starFrameObjectArray;

	[SerializeField]
	private GameObject skillIconPos;

	[SerializeField]
	private GameObject materialIconPos;

	[SerializeField]
	private Text materialDescription;

	[SerializeField]
	private GameObject upgradeButton;

	[SerializeField]
	private GameObject materialPanel;

	[SerializeField]
	private Image materialCountBar;

	[SerializeField]
	private Text materialCountText;

	[SerializeField]
	private Button m_nextSkill;

	[SerializeField]
	private Button m_previousSkill;

	[SerializeField]
	private Transform m_skillInfoPanel;

	[SerializeField]
	private OrangeText m_skillName;

	[SerializeField]
	private Image m_skillShowcase;

	private CharacterInfo characterInfo;

	private CHARACTER_TABLE characterTable;

	private STAR_TABLE starTable;

	private CharacterInfoUI characterInfoUI;

	private List<int> m_unlockableSkillList = new List<int>();

	private int m_currentSkillInfoIndex;

	private StarUpEffect m_starUpEffect;

	private int materialRequired;

	private int materialOwned;

	private int moneyRequired;

	private int moneyOwned;

	public bool bEffectLock;

	public override void SetCanvas(bool enable)
	{
		if (enable && !base.IsVisible)
		{
			SetUpgradeInfo();
		}
		base.SetCanvas(enable);
	}

	public void Setup(CharacterInfo info)
	{
		characterInfo = info;
		characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.netInfo.CharacterID];
		characterInfoUI = base.transform.parent.GetComponentInChildren<CharacterInfoUI>();
		CreateSkillIcon();
		SetStars(characterInfo.netInfo.Star);
		SetUpgradeInfo();
		m_currentSkillInfoIndex = 0;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void CreateSkillIcon()
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("ui/skillbutton", "SkillButton", delegate(GameObject asset)
		{
			if (asset != null)
			{
				GameObject obj = UnityEngine.Object.Instantiate(asset, skillIconPos.transform, false);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<SkillButton>().Setup(10001, SkillButton.StatusType.DEFAULT);
			}
		});
	}

	private void PlayStarUpEffect()
	{
		if (m_starUpEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "starupeffect", "StarUpEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_starUpEffect = gameObject.GetComponent<StarUpEffect>();
				m_starUpEffect.Play(starFrameObjectArray[characterInfo.netInfo.Star - 1].transform.position);
			});
		}
		else
		{
			m_starUpEffect.Play(starFrameObjectArray[characterInfo.netInfo.Star - 1].transform.position);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP02);
	}

	private void SetStars(int num)
	{
		int num2 = 0;
		GameObject[] array = starObjectArray;
		foreach (GameObject obj in array)
		{
			starFrameObjectArray[num2].SetActive(false);
			obj.SetActive(num2 < num);
			num2++;
		}
		if (num < 5)
		{
			starFrameObjectArray[num].SetActive(true);
		}
	}

	private void AddUnlockableSkillToList(int skillID)
	{
		if (skillID != 0)
		{
			m_unlockableSkillList.Add(skillID);
		}
	}

	private void SetupSkillPreviewInfo(int index)
	{
		SkillButton componentInChildren = skillIconPos.GetComponentInChildren<SkillButton>();
		if (index < m_unlockableSkillList.Count)
		{
			int num = m_unlockableSkillList[index];
			componentInChildren.SetStyle(SkillButton.StyleType.SQUARE);
			componentInChildren.Setup(num, SkillButton.StatusType.EQUIPPED);
			componentInChildren.OverrideText("");
			SKILL_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(num, out value))
			{
				m_skillName.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetShowcase(value.s_SHOWCASE), value.s_SHOWCASE, delegate(Sprite asset)
				{
					if (asset != null)
					{
						m_skillShowcase.sprite = asset;
					}
				});
			}
			m_skillShowcase.gameObject.SetActive(true);
			m_previousSkill.interactable = index > 0;
			m_nextSkill.interactable = index < m_unlockableSkillList.Count - 1;
		}
		else
		{
			componentInChildren.SetStyle(SkillButton.StyleType.SQUARE);
			componentInChildren.Setup(0, SkillButton.StatusType.EQUIPPED);
			componentInChildren.OverrideText("");
			m_skillName.text = "-";
			m_skillShowcase.gameObject.SetActive(false);
			m_nextSkill.interactable = false;
			m_previousSkill.interactable = false;
		}
	}

	private void SetUpgradeInfo()
	{
		int num = characterInfo.netInfo.Star + 1;
		m_unlockableSkillList.Clear();
		if (num == characterTable.n_PASSIVE_UNLOCK1)
		{
			AddUnlockableSkillToList(characterTable.n_PASSIVE_1);
		}
		if (num == characterTable.n_PASSIVE_UNLOCK2)
		{
			AddUnlockableSkillToList(characterTable.n_PASSIVE_2);
		}
		if (num == characterTable.n_PASSIVE_UNLOCK3)
		{
			AddUnlockableSkillToList(characterTable.n_PASSIVE_3);
		}
		if (num == characterTable.n_PASSIVE_UNLOCK4)
		{
			AddUnlockableSkillToList(characterTable.n_PASSIVE_4);
		}
		if (num == characterTable.n_PASSIVE_UNLOCK5)
		{
			AddUnlockableSkillToList(characterTable.n_PASSIVE_5);
		}
		if (num == characterTable.n_SKILL1_UNLOCK1)
		{
			AddUnlockableSkillToList(characterTable.n_SKILL1_EX1);
		}
		if (num == characterTable.n_SKILL1_UNLOCK2)
		{
			AddUnlockableSkillToList(characterTable.n_SKILL1_EX2);
		}
		if (num == characterTable.n_SKILL1_UNLOCK3)
		{
			AddUnlockableSkillToList(characterTable.n_SKILL1_EX3);
		}
		if (num == characterTable.n_SKILL2_UNLOCK1)
		{
			AddUnlockableSkillToList(characterTable.n_SKILL2_EX1);
		}
		if (num == characterTable.n_SKILL2_UNLOCK2)
		{
			AddUnlockableSkillToList(characterTable.n_SKILL2_EX2);
		}
		if (num == characterTable.n_SKILL2_UNLOCK3)
		{
			AddUnlockableSkillToList(characterTable.n_SKILL2_EX3);
		}
		SetupSkillPreviewInfo(m_currentSkillInfoIndex);
		Dictionary<int, STAR_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext() && (enumerator.Current.Value.n_TYPE != 1 || enumerator.Current.Value.n_MAINID != characterInfo.netInfo.CharacterID || enumerator.Current.Value.n_STAR != characterInfo.netInfo.Star))
		{
		}
		starTable = enumerator.Current.Value;
		bool flag = characterInfo.netInfo.State == 1;
		materialPanel.SetActive(true);
		MATERIAL_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(starTable.n_MATERIAL, out value) && flag)
		{
			ITEM_TABLE itemTable;
			if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(value.n_MATERIAL_1, out itemTable))
			{
				materialDescription.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemTable.w_NAME);
			}
			else
			{
				materialDescription.text = "???";
			}
			materialOwned = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(itemTable.n_ID);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
			{
				UnityEngine.Object.Instantiate(asset, materialIconPos.transform).GetComponent<CommonIconBase>().SetupItem(itemTable.n_ID, 0, OnClickMaterialIcon);
			});
			materialRequired = value.n_MATERIAL_MOUNT1;
			materialCountText.text = string.Format("{0}/{1}", materialOwned, materialRequired);
			moneyRequired = value.n_MONEY;
			moneyOwned = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
			float num2 = (float)materialOwned / (float)materialRequired;
			if (num2 > 1f)
			{
				num2 = 1f;
			}
			materialCountBar.transform.localScale = new Vector3(num2, 1f, 1f);
			bool interactable = moneyRequired <= moneyOwned && materialRequired <= materialOwned;
			upgradeButton.GetComponent<Button>().interactable = interactable;
		}
		else
		{
			upgradeButton.GetComponent<Button>().interactable = false;
			materialPanel.SetActive(false);
		}
	}

	public void OnClickAddMaterial()
	{
		MATERIAL_TABLE value;
		if (starTable == null || !ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(starTable.n_MATERIAL, out value))
		{
			return;
		}
		ITEM_TABLE item;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(value.n_MATERIAL_1, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.Setup(item);
			ItemInfoUI itemInfoUI = ui;
			itemInfoUI.closeCB = (Callback)Delegate.Combine(itemInfoUI.closeCB, (Callback)delegate
			{
				ui.NeedSE = false;
				ui.Setup(item);
				ui.NeedSE = true;
			});
		});
	}

	private void OnClickMaterialIcon(int p_idx)
	{
		ITEM_TABLE itemTable;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(characterTable.n_UNLOCK_ID, out itemTable))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
			{
				ui.Setup(itemTable, materialRequired);
			});
		}
	}

	private IEnumerator PlayStarUpEffectAndRefreshMenu()
	{
		if (characterInfo.netInfo.Star > 0)
		{
			characterInfoUI.CheckCharacterUpgrades();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
			yield return new WaitForSeconds(0.5f);
			characterInfoUI.PlayUpgrade3DEffect();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
			yield return new WaitForSeconds(1f);
			PlayStarUpEffect();
			yield return new WaitForSeconds(0.25f);
			m_currentSkillInfoIndex = 0;
			SetStars(characterInfo.netInfo.Star);
			upgradeButton.GetComponent<Button>().interactable = true;
			SetUpgradeInfo();
			characterInfoUI.RefreshQuickSelectBar();
			characterInfoUI.RefreshBadges();
			bEffectLock = false;
			characterInfoUI.RefreshSideButtonRedDots();
		}
	}

	public void OnClickUpgradeStarBtn()
	{
		if (starTable == null)
		{
			return;
		}
		if (moneyRequired <= moneyOwned && materialRequired <= materialOwned)
		{
			upgradeButton.GetComponent<Button>().interactable = false;
			bEffectLock = true;
			ManagedSingleton<PlayerNetManager>.Instance.CharacterUpgradeStarReq(characterInfo.netInfo.CharacterID, starTable.n_ID, delegate
			{
				starTable = null;
				StartCoroutine(PlayStarUpEffectAndRefreshMenu());
				if ((bool)MonoBehaviourSingleton<UIManager>.Instance.GetUI<GoCheckUI>("UI_GoCheck"))
				{
					ManagedSingleton<CharacterHelper>.Instance.SortCharacterListNoFragmentNoSave();
				}
				else
				{
					ManagedSingleton<CharacterHelper>.Instance.SortCharacterList();
				}
				ManagedSingleton<DeepRecordHelper>.Instance.ApiRefreashTime = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC;
			});
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MATERIAL_NOT_ENOUGH"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
	}

	private void ChangeSkillPage(bool bNext)
	{
		float time = 0.3f;
		float num = (bNext ? 600f : (-600f));
		Vector3 localPosition = m_skillInfoPanel.localPosition;
		Vector3 localPosition2 = new Vector3(localPosition.x + num, localPosition.y, localPosition.z);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		if (bNext)
		{
			m_currentSkillInfoIndex = ((m_currentSkillInfoIndex + 1 >= m_unlockableSkillList.Count) ? m_currentSkillInfoIndex : (m_currentSkillInfoIndex + 1));
		}
		else
		{
			m_currentSkillInfoIndex = ((m_currentSkillInfoIndex - 1 >= 0) ? (m_currentSkillInfoIndex - 1) : 0);
		}
		SetupSkillPreviewInfo(m_currentSkillInfoIndex);
		bool bNextBtnInteractable = m_nextSkill.IsInteractable();
		bool bPreviousBtnInteractable = m_previousSkill.IsInteractable();
		m_nextSkill.interactable = false;
		m_previousSkill.interactable = false;
		Transform tempSkillInfoPanel = UnityEngine.Object.Instantiate(m_skillInfoPanel, m_skillInfoPanel.parent);
		LeanTween.moveLocalX(tempSkillInfoPanel.gameObject, localPosition.x - num, time).setEase(LeanTweenType.easeOutQuart);
		SkillButton componentInChildren = skillIconPos.GetComponentInChildren<SkillButton>();
		if (m_currentSkillInfoIndex < m_unlockableSkillList.Count)
		{
			int skillID = m_unlockableSkillList[m_currentSkillInfoIndex];
			componentInChildren.SetStyle(SkillButton.StyleType.SQUARE);
			componentInChildren.Setup(skillID, SkillButton.StatusType.EQUIPPED);
			componentInChildren.OverrideText("");
		}
		m_skillInfoPanel.localPosition = localPosition2;
		LeanTween.moveLocalX(m_skillInfoPanel.gameObject, localPosition.x, time).setEase(LeanTweenType.easeOutQuart).setOnComplete((Action)delegate
		{
			UnityEngine.Object.Destroy(tempSkillInfoPanel.gameObject);
			m_nextSkill.interactable = bNextBtnInteractable;
			m_previousSkill.interactable = bPreviousBtnInteractable;
		});
	}

	public void OnClickNextSkill()
	{
		ChangeSkillPage(true);
	}

	public void OnClickPreviousSkill()
	{
		ChangeSkillPage(false);
	}

	public bool IsEffectPlaying()
	{
		bool flag = false;
		flag |= bEffectLock;
		if (m_starUpEffect != null)
		{
			flag |= m_starUpEffect.gameObject.activeSelf;
		}
		return flag;
	}
}
