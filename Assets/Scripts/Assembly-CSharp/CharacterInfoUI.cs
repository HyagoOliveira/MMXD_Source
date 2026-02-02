#define RELEASE
using System;
using System.Collections;
using System.Reflection;
using CallbackDefs;
using Coffee.UIExtensions;
using OrangeAudio;
using UnityEngine;
using UnityEngine.UI;

internal class CharacterInfoUI : OrangeUIBase
{
	private enum TAB_TYPE
	{
		BASIC = 0,
		SKILL_ACTIVE = 1,
		SKILL_PASSIVE = 2,
		UPGRADE = 3,
		SKIN = 4,
		CARD = 5,
		DNA = 6,
		MAX = 7
	}

	private TAB_TYPE currentTab;

	private bool bIsUIActive;

	private CHARACTER_TABLE characterTable;

	private SKIN_TABLE m_skinTable;

	private CharacterInfo characterInfo;

	private RenderTextureObj textureObj;

	private CharacterInfoSelect characterInfoSelect;

	private int? currentSelectionIndex;

	private RankNameStars rankNameStars;

	private OrangeUIBase m_currentUI;

	private StandBase m_standBase;

	private Upgrade3DEffect m_upgrade3DEffect;

	private GameObject m_unlockEffect;

	private float m_unlockEffectLength;

	private bool m_bIsInitialized;

	private int m_portraitPosTweenId;

	private int m_portraitColorTweenId;

	private int m_unlockEffectTweenId;

	private int m_modelDelayTweenId;

	private string m_currentPortraitFileName = string.Empty;

	private bool bEffectLock;

	private bool IgnoreFristTime = true;

	private bool m_bCalledByBackToHometop;

	[SerializeField]
	private CommonTab[] pageTabButtonArray;

	[SerializeField]
	private LoopHorizontalScrollRect scrollRect;

	[SerializeField]
	private RawImage tModelImg;

	[SerializeField]
	private Transform rankNameStarsRoot;

	[SerializeField]
	private GameObject m_refRankNameStars;

	[SerializeField]
	private Transform m_portraitGroup;

	[SerializeField]
	private Image m_portraitImage;

	[SerializeField]
	private GameObject m_cardBtnRoot;

	[SerializeField]
	private GameObject m_DNABtnRoot;

	[SerializeField]
	private Image m_skillRedDot;

	[SerializeField]
	private Image m_starRedDot;

	[SerializeField]
	private Image m_skinRedDot;

	[SerializeField]
	private Image m_skinDNADot;

	[SerializeField]
	private CommonTabParent commonTabParent;

	protected override void Awake()
	{
		base.Awake();
		float renderTextureRate = MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRenderTextureRate();
		tModelImg.transform.localScale = new Vector3(renderTextureRate, renderTextureRate, 1f);
		m_cardBtnRoot.SetActive(true);
		if ((bool)m_DNABtnRoot)
		{
			m_DNABtnRoot.SetActive(true);
		}
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref m_portraitPosTweenId);
		LeanTween.cancel(ref m_portraitColorTweenId);
		LeanTween.cancel(ref m_unlockEffectTweenId);
		LeanTween.cancel(ref m_modelDelayTweenId);
		if (null != textureObj)
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<int>(EventManager.ID.UI_CHARACTERINFO_BONUS_COUNT, textureObj.BonusCount);
			UnityEngine.Object.Destroy(textureObj.gameObject);
			textureObj = null;
		}
		m_bIsInitialized = false;
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
	}

	public int GetCurrentSelectionIndex()
	{
		if (!currentSelectionIndex.HasValue)
		{
			return 0;
		}
		return currentSelectionIndex.Value;
	}

	public void NotifyCharacterChange(CharacterInfo newCharacterInfo, int newSelectionIndex)
	{
		if (m_bIsInitialized && !base.IsLock && newSelectionIndex != currentSelectionIndex)
		{
			currentSelectionIndex = newSelectionIndex;
			characterInfo = newCharacterInfo;
			characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.netInfo.CharacterID];
			ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(characterInfo.netInfo.Skin, out m_skinTable);
			RefreshMenu();
		}
	}

	public void RefreshModel(SKIN_TABLE skinTable)
	{
		if (tModelImg == null)
		{
			return;
		}
		if (null != textureObj)
		{
			RefreshModelHelper(skinTable);
			return;
		}
		ModelRotateDrag objDrag = tModelImg.GetComponent<ModelRotateDrag>();
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(UnityEngine.Object obj)
		{
			textureObj = UnityEngine.Object.Instantiate((GameObject)obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
			textureObj.OnlyDebut = true;
			RefreshModelHelper(skinTable);
			Singleton<GenericEventManager>.Instance.AttachEvent<int>(EventManager.ID.UI_CHARACTERINFO_BONUS_COUNT, textureObj.BonusCount);
			if ((bool)objDrag)
			{
				objDrag.SetModelTransform(textureObj.RenderPosition);
			}
		});
	}

	private void RefreshModelHelper(SKIN_TABLE skinTable)
	{
		textureObj.CanCount = characterInfo.netInfo.State == 1;
		textureObj.AssignNewRender(characterTable, null, skinTable, new Vector3(0f, -0.6f, 5f), tModelImg);
	}

	public void RefreshPortrait(SKIN_TABLE skinTable)
	{
		if (m_portraitGroup == null)
		{
			return;
		}
		string imgFileName = string.Format("St_{0}", characterTable.s_ICON);
		if (skinTable != null)
		{
			imgFileName = string.Format("St_{0}", skinTable.s_ICON);
		}
		if (string.Compare(m_currentPortraitFileName, imgFileName) == 0)
		{
			return;
		}
		m_currentPortraitFileName = imgFileName;
		float num = 150f;
		float animTime = 0.5f;
		Vector3 startPos = m_portraitGroup.localPosition + new Vector3(0f - num, 0f, 0f);
		Vector3 targetPos = m_portraitGroup.localPosition;
		CanvasGroup portraitCanvas = m_portraitGroup.GetComponent<CanvasGroup>();
		if ((bool)m_standBase)
		{
			UnityEngine.Object.Destroy(m_standBase.gameObject);
		}
		string bundleName = string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, imgFileName);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetGameObjectAndAsyncLoad(bundleName, imgFileName, delegate(GameObject obj)
		{
			m_standBase = obj.GetComponent<StandBase>();
			if (m_standBase == null)
			{
				Debug.LogWarning("Error loading asset " + imgFileName + ", null obj returned.");
			}
			else
			{
				m_standBase.Setup(m_portraitImage.transform);
				UIEffect componentInChildren = m_portraitImage.GetComponentInChildren<UIEffect>();
				if ((bool)componentInChildren)
				{
					Image portraitImage = componentInChildren.gameObject.GetComponent<Image>();
					portraitImage.sprite = m_standBase.GetComponent<Image>().sprite;
					CopyComponent(m_standBase.GetComponent<StandBase>(), componentInChildren.gameObject);
					CopyComponent(m_standBase.GetComponent<RectTransform>(), componentInChildren.gameObject);
					componentInChildren.effectColor = new Color(0.3019f, 0.3843f, 0.4784f);
					portraitImage.color = new Color(0.3019f, 0.3843f, 0.4784f, 0f);
					m_standBase.gameObject.SetActive(false);
					LeanTween.cancel(ref m_portraitPosTweenId);
					m_portraitPosTweenId = LeanTween.value(m_portraitGroup.gameObject, startPos.x, targetPos.x, animTime).setOnUpdate(delegate(float val)
					{
						m_portraitGroup.localPosition = new Vector3(val, targetPos.y, targetPos.z);
					}).setEase(LeanTweenType.easeOutCubic)
						.uniqueId;
					LeanTween.cancel(ref m_portraitColorTweenId);
					m_portraitColorTweenId = LeanTween.value(portraitCanvas.gameObject, 0f, 1f, animTime * 1.5f).setOnUpdate(delegate(float val)
					{
						portraitImage.color = new Color(0.3019f, 0.3843f, 0.4784f, val);
					}).uniqueId;
				}
			}
		});
	}

	private T CopyComponent<T>(T original, GameObject destination) where T : Component
	{
		Type type = original.GetType();
		T val = destination.GetComponent(type) as T;
		if (!val)
		{
			val = destination.AddComponent(type) as T;
		}
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsStatic)
			{
				fieldInfo.SetValue(val, fieldInfo.GetValue(original));
			}
		}
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanWrite && propertyInfo.CanWrite && !(propertyInfo.Name == "name"))
			{
				propertyInfo.SetValue(val, propertyInfo.GetValue(original, null), null);
			}
		}
		return val;
	}

	public void Setup(CharacterInfo info, int selectionIndex, int selectTab = 0, int skill = 0)
	{
		characterInfo = info;
		characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.netInfo.CharacterID];
		ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(info.netInfo.Skin, out m_skinTable);
		currentSelectionIndex = selectionIndex;
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		if (pageTabButtonArray.Length >= 4)
		{
			pageTabButtonArray[1].SetButtonLock(lV < OrangeConst.OPENRANK_SKILL_LVUP);
			pageTabButtonArray[2].SetButtonLock(lV < OrangeConst.OPENRANK_STARUP);
			pageTabButtonArray[3].SetButtonLock(false);
		}
		characterInfoSelect = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoSelect>("UI_CharacterInfo_Select");
		rankNameStars = UnityEngine.Object.Instantiate(m_refRankNameStars, rankNameStarsRoot).GetComponent<RankNameStars>();
		if ((bool)characterInfoSelect)
		{
			characterInfoSelect.SetActive(false);
		}
		Singleton<GenericEventManager>.Instance.AttachEvent<CharacterInfo, int>(EventManager.ID.UI_CHARACTERINFO_CHARACTER_CHANGE, NotifyCharacterChange);
		float num = 174.5f;
		scrollRect.totalCount = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList().Count;
		scrollRect.content.sizeDelta = new Vector2(num * (float)scrollRect.totalCount, scrollRect.content.sizeDelta.y);
		int num2 = 11;
		if (scrollRect.totalCount > num2 && currentSelectionIndex.Value >= scrollRect.totalCount - num2)
		{
			scrollRect.RefillCells(scrollRect.totalCount - num2);
		}
		else
		{
			scrollRect.RefillCells(currentSelectionIndex.Value);
		}
		Vector3 offset = new Vector3(0f, 5f, 0f);
		if (m_unlockEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "ui_lockfx", "ui_lockfx", delegate(GameObject asset)
			{
				m_unlockEffect = UnityEngine.Object.Instantiate(asset, base.transform);
				m_unlockEffect.transform.position = tModelImg.transform.position + offset;
				m_unlockEffectLength = m_unlockEffect.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
				m_unlockEffect.SetActive(false);
			});
		}
		m_bIsInitialized = true;
		commonTabParent.SetDefaultTabIndex(selectTab);
	}

	public void DisplayBasicInfoMenu()
	{
		if (!IgnoreFristTime)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		}
		else
		{
			IgnoreFristTime = false;
		}
		currentTab = TAB_TYPE.BASIC;
		RefreshMenu();
	}

	public void DisplaySkillMenu()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		currentTab = TAB_TYPE.SKILL_ACTIVE;
		RefreshMenu();
	}

	public void DisplayUpgradeMenu()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		currentTab = TAB_TYPE.UPGRADE;
		RefreshMenu();
	}

	public void DisplaySkinMenu()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		currentTab = TAB_TYPE.SKIN;
		RefreshMenu();
	}

	public void DisplayCardMenu()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		currentTab = TAB_TYPE.CARD;
		RefreshMenu();
	}

	public void DisplayDNAMenu()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		currentTab = TAB_TYPE.DNA;
		RefreshMenu();
	}

	public void RefreshMenuSkillPassive()
	{
		currentTab = TAB_TYPE.SKILL_PASSIVE;
		RefreshMenu();
	}

	private void RefreshCharacterInfo()
	{
		CharacterInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(characterInfo.netInfo.CharacterID, out value) && value.netInfo != null)
		{
			characterInfo = value;
		}
	}

	public void RefreshMenu(bool bFullRefresh = false)
	{
		CloseSubMenu();
		RefreshCharacterInfo();
		CommonTab component = m_DNABtnRoot.GetComponent<CommonTab>();
		component.SetButtonLock(false);
		if (ManagedSingleton<PlayerHelper>.Instance.GetLV() < OrangeConst.DNA_LV_LIMIT)
		{
			component.SetButtonLock(true);
		}
		ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(characterInfo.netInfo.Skin, out m_skinTable);
		RefreshPortrait(m_skinTable);
		int siblingIndex = base.transform.GetSiblingIndex();
		switch (currentTab)
		{
		default:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Basic", delegate(CharacterInfoBasic ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					OnClickCloseBtn();
				});
				ui.Setup(characterInfo);
				SetModelPreview(true);
				m_currentUI = ui;
				bIsUIActive = true;
				RefreshModel(m_skinTable);
				ui.transform.SetSiblingIndex(siblingIndex + 1);
			});
			break;
		case TAB_TYPE.SKILL_ACTIVE:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Skill", delegate(CharacterInfoSkill ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					OnClickCloseBtn();
				});
				ui.Setup(characterInfo);
				SetModelPreview(false);
				m_currentUI = ui;
				bIsUIActive = true;
				ui.transform.SetSiblingIndex(siblingIndex + 1);
			});
			break;
		case TAB_TYPE.SKILL_PASSIVE:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Skill", delegate(CharacterInfoSkill ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					OnClickCloseBtn();
				});
				ui.Setup(characterInfo, CharacterInfoSkill.SKILLTAB_TYPE.PASSIVE);
				SetModelPreview(false);
				m_currentUI = ui;
				bIsUIActive = true;
				ui.transform.SetSiblingIndex(siblingIndex + 1);
			});
			break;
		case TAB_TYPE.UPGRADE:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Upgrade", delegate(CharacterInfoUpgrade ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					OnClickCloseBtn();
				});
				ui.Setup(characterInfo);
				SetModelPreview(true);
				m_currentUI = ui;
				bIsUIActive = true;
				RefreshModel(m_skinTable);
				ui.transform.SetSiblingIndex(siblingIndex + 1);
			});
			break;
		case TAB_TYPE.SKIN:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Skin", delegate(CharacterInfoSkin ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					OnClickCloseBtn();
				});
				ui.Setup(characterInfo);
				SetModelPreview(true);
				m_currentUI = ui;
				bIsUIActive = true;
				RefreshModel(m_skinTable);
				ui.transform.SetSiblingIndex(siblingIndex + 1);
			});
			break;
		case TAB_TYPE.CARD:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Card", delegate(CharacterInfoCard ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					OnClickCloseBtn();
				});
				ui.Setup(characterInfo);
				SetModelPreview(true);
				m_currentUI = ui;
				bIsUIActive = true;
				RefreshModel(m_skinTable);
				ui.transform.SetSiblingIndex(siblingIndex + 1);
			});
			break;
		case TAB_TYPE.DNA:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_DNA", delegate(CharacterInfoDNA ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					OnClickCloseBtn();
				});
				ui.Setup(characterInfo);
				SetModelPreview(false);
				m_currentUI = ui;
				bIsUIActive = true;
				ui.transform.SetSiblingIndex(siblingIndex + 1);
			});
			break;
		}
		RefreshBadges();
		if (bFullRefresh)
		{
			RefreshQuickSelectBar();
		}
		RefreshSideButtonRedDots();
	}

	public void RefreshSideButtonRedDots()
	{
		StartCoroutine(RefreshSideButtonRedDotsCoroutine(characterInfo.netInfo));
	}

	private IEnumerator RefreshSideButtonRedDotsCoroutine(NetCharacterInfo info)
	{
		while (ManagedSingleton<CharacterHelper>.Instance.IsUpgradeChecking())
		{
			yield return null;
		}
		CharacterHelper.UpgradesFlag characterUpgradesFlag = ManagedSingleton<CharacterHelper>.Instance.GetCharacterUpgradesFlag(info.CharacterID);
		if (m_skillRedDot != null)
		{
			m_skillRedDot.gameObject.SetActive((characterUpgradesFlag & CharacterHelper.UpgradesFlag.SKILL) != 0);
		}
		if (m_starRedDot != null)
		{
			m_starRedDot.gameObject.SetActive((characterUpgradesFlag & CharacterHelper.UpgradesFlag.STAR) != 0);
		}
		if (m_skinRedDot != null)
		{
			m_skinRedDot.gameObject.SetActive((characterUpgradesFlag & CharacterHelper.UpgradesFlag.SKIN) != 0);
		}
		if (m_skinDNADot != null)
		{
			m_skinDNADot.gameObject.SetActive((characterUpgradesFlag & CharacterHelper.UpgradesFlag.DNA) != 0);
		}
	}

	public void RefreshBadges()
	{
		if (characterTable != null)
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(characterTable.w_NAME);
			rankNameStars.Setup((RankNameStars.RANK)characterTable.n_RARITY, l10nValue, characterInfo.netInfo.Star);
		}
	}

	public void RefreshQuickSelectBar(bool bScrollToSelection = false)
	{
		StartCoroutine(RefreshQuickSelectBarCoroutine(bScrollToSelection));
	}

	private IEnumerator RefreshQuickSelectBarCoroutine(bool bScrollToSelection)
	{
		while (ManagedSingleton<CharacterHelper>.Instance.IsUpgradeChecking())
		{
			yield return null;
		}
		int num = 0;
		foreach (CharacterInfo sortedCharacter in ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList())
		{
			if (sortedCharacter.netInfo.CharacterID == characterInfo.netInfo.CharacterID)
			{
				currentSelectionIndex = num;
				break;
			}
			num++;
		}
		if (bScrollToSelection)
		{
			int num2 = 11;
			if (scrollRect.totalCount > num2 && num >= scrollRect.totalCount - num2)
			{
				scrollRect.RefillCells(scrollRect.totalCount - num2);
			}
			else
			{
				scrollRect.RefillCells(num);
			}
		}
		else
		{
			scrollRect.RefreshCells();
		}
	}

	public void CheckCharacterUpgrades()
	{
		StartCoroutine(ManagedSingleton<CharacterHelper>.Instance.CheckCharacterUpgrades());
	}

	private void CloseSubMenu()
	{
		if (!bIsUIActive)
		{
			return;
		}
		if ((bool)m_currentUI)
		{
			SetInteractable(false);
			if ((bool)(m_currentUI as CharacterInfoSkin))
			{
				(m_currentUI as CharacterInfoSkin).StopLeanTween();
			}
			MonoBehaviourSingleton<UIManager>.Instance.CloseUI(m_currentUI);
			m_currentUI = null;
		}
		bIsUIActive = false;
	}

	protected override void OnBackToHometop()
	{
		m_bCalledByBackToHometop = true;
		closeCB = null;
		base.OnBackToHometop();
	}

	public override void OnClickCloseBtn()
	{
		if (!m_bCalledByBackToHometop)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
		}
		base.OnClickCloseBtn();
		m_bIsInitialized = false;
		CloseSubMenu();
		SetModelPreview(false);
		Singleton<GenericEventManager>.Instance.DetachEvent<CharacterInfo, int>(EventManager.ID.UI_CHARACTERINFO_CHARACTER_CHANGE, NotifyCharacterChange);
		CharacterInfoSelect uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoSelect>("UI_CharacterInfo_Select");
		if ((bool)uI)
		{
			uI.SetActive(true);
		}
		if (null != textureObj)
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<int>(EventManager.ID.UI_CHARACTERINFO_BONUS_COUNT, textureObj.BonusCount);
			UnityEngine.Object.Destroy(textureObj.gameObject);
			textureObj = null;
		}
	}

	private void SetModelPreview(bool bIsVisible)
	{
		float alpha = (bIsVisible ? 1f : 0f);
		CanvasGroup component = rankNameStars.GetComponent<CanvasGroup>();
		CanvasGroup component2 = tModelImg.GetComponent<CanvasGroup>();
		component.alpha = alpha;
		component2.alpha = alpha;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHARACTER_RT_VISIBLE, bIsVisible);
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (textureObj != null)
		{
			textureObj.SetCameraActive(enable);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CHARACTER_RT_VISIBLE, enable);
		}
	}

	public void PlayUpgrade3DEffect()
	{
		if (m_upgrade3DEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgrade3deffect", "Upgrade3DEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_upgrade3DEffect = gameObject.GetComponent<Upgrade3DEffect>();
				m_upgrade3DEffect.Play(tModelImg.transform.position);
			});
		}
		else
		{
			m_upgrade3DEffect.Play(tModelImg.transform.position);
		}
	}

	public void PlayUnlockEffect()
	{
		if (m_unlockEffect != null)
		{
			StartCoroutine(PlayUnlockSE());
			m_unlockEffect.SetActive(true);
			m_unlockEffect.GetComponent<Animator>().Play("UI_lockFX", 0, 0f);
			LeanTween.cancel(ref m_unlockEffectTweenId);
			m_unlockEffectTweenId = LeanTween.delayedCall(m_unlockEffectLength, (Action)delegate
			{
				m_unlockEffect.SetActive(false);
			}).uniqueId;
		}
	}

	private IEnumerator PlayUnlockSE()
	{
		yield return new WaitForSeconds(0.2f);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
	}

	public void OnLhsrValueChange()
	{
	}

	public bool IsEffectPlaying()
	{
		bool flag = false;
		flag |= bEffectLock;
		if (m_unlockEffect != null)
		{
			flag |= m_unlockEffect.activeSelf;
		}
		if (m_upgrade3DEffect != null)
		{
			flag |= m_upgrade3DEffect.gameObject.activeSelf;
		}
		return flag;
	}
}
