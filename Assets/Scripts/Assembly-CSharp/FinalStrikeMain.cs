#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class FinalStrikeMain : OrangeUIBase
{
	[SerializeField]
	private FinalStrikeIcon m_finalStrikeIconRef;

	[SerializeField]
	private RankNameStars m_rankNameStarsRef;

	[SerializeField]
	private GameObject m_rankNameStarsPos;

	[SerializeField]
	private Toggle m_basicTab;

	[SerializeField]
	private GameObject m_basicPage;

	[SerializeField]
	private GameObject m_upgradePage;

	[SerializeField]
	private GameObject m_starPage;

	[SerializeField]
	private GameObject m_iconStartRoot;

	[SerializeField]
	private GameObject m_scrollViewContentRoot;

	[SerializeField]
	private ScrollRect m_scrollRect;

	[SerializeField]
	private GameObject m_basicPageUnlockSection;

	[SerializeField]
	private GameObject m_basicPageEquipSection;

	[SerializeField]
	private FinalStrikeTierConnector m_tierConnector_1;

	[SerializeField]
	private FinalStrikeTierConnector m_tierConnector_2;

	[SerializeField]
	private FinalStrikeTierConnector m_tierConnector_3;

	[SerializeField]
	private CommonIconBase m_materialIconRef;

	[SerializeField]
	private GameObject m_equipDialog;

	[SerializeField]
	private GameObject m_equipDialogMainIcon;

	[SerializeField]
	private GameObject m_equipDialogSubIcon;

	[SerializeField]
	private OrangeText m_currentLevel;

	[SerializeField]
	private OrangeText m_currentPower;

	[SerializeField]
	private OrangeText m_currentHP;

	[SerializeField]
	private OrangeText m_currentDefense;

	[SerializeField]
	private OrangeText m_basicSkillDesc;

	[SerializeField]
	private Image m_basicSkillShowcase;

	[SerializeField]
	private OrangeText m_basicLevel;

	[SerializeField]
	private OrangeText m_basicAttack;

	[SerializeField]
	private OrangeText m_basicHP;

	[SerializeField]
	private OrangeText m_basicDefense;

	[SerializeField]
	private OrangeText m_basicUnlockDescription;

	[SerializeField]
	private Image m_basicLevelBar;

	[SerializeField]
	private Image m_basicAttackBar;

	[SerializeField]
	private Image m_basicHPBar;

	[SerializeField]
	private Image m_basicDefenseBar;

	[SerializeField]
	private GameObject m_basicUnlockItem;

	[SerializeField]
	private OrangeText m_basicUnlockItemCount;

	[SerializeField]
	private Image m_basicUnlockItemCountBar;

	[SerializeField]
	private OrangeText m_basicUnlockItemName;

	[SerializeField]
	private GameObject m_basicUnlockItemIconPos;

	[SerializeField]
	private Button m_basicUnlockItemAddBtn;

	[SerializeField]
	private Button m_basicUnlockEquipBtn;

	[SerializeField]
	private OrangeText m_basicUnlockEquipBtnText;

	[SerializeField]
	private OrangeText m_lvlUpLvlText;

	[SerializeField]
	private Image m_lvlUpLvlBar;

	[SerializeField]
	private OrangeText m_lvlUpLvlPercentage;

	[SerializeField]
	private OrangeText m_lvlUpAttackOriginal;

	[SerializeField]
	private OrangeText m_lvlUpAttackNew;

	[SerializeField]
	private OrangeText m_lvlUpHPOriginal;

	[SerializeField]
	private OrangeText m_lvlUpHPNew;

	[SerializeField]
	private OrangeText m_lvlUpDefenseOriginal;

	[SerializeField]
	private OrangeText m_lvlUpDefenseNew;

	[SerializeField]
	private OrangeText m_lvlUpMaterialRequired;

	[SerializeField]
	private OrangeText m_lvlUpMoneyRequired;

	[SerializeField]
	private OrangeText m_lvlUpMaterialOwned;

	[SerializeField]
	private OrangeText m_lvlUpMoneyOwned;

	[SerializeField]
	private Button m_lvlUpBtn;

	[SerializeField]
	private OrangeText m_starSkillDesc;

	[SerializeField]
	private Image m_starSkillShowcase;

	[SerializeField]
	private OrangeText m_starPowerOriginal;

	[SerializeField]
	private OrangeText m_starPowerNew;

	[SerializeField]
	private GameObject m_starMaterialIconPos;

	[SerializeField]
	private OrangeText m_starMaterialName;

	[SerializeField]
	private OrangeText m_starMaterialCount;

	[SerializeField]
	private Image m_starMaterialCountBar;

	[SerializeField]
	private GameObject m_starUnlockMaterial;

	[SerializeField]
	private Button m_starMaterialAddBtn;

	[SerializeField]
	private Button m_starUpgradeBtn;

	[SerializeField]
	private Transform m_detailInfo;

	[SerializeField]
	private OrangeText m_atkBefore;

	[SerializeField]
	private OrangeText m_atkAfter;

	[SerializeField]
	private OrangeText m_hpBefore;

	[SerializeField]
	private OrangeText m_hpAfter;

	[SerializeField]
	private OrangeText m_defBefore;

	[SerializeField]
	private OrangeText m_defAfter;

	[SerializeField]
	private GameObject m_redDotUnLock;

	[SerializeField]
	private GameObject m_redDotStart;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_equipSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_tabSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_strengthSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_rankUPSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_unlockSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickInfoSE;

	private Vector2 m_defaultLvlBarSize;

	private int m_selectedFSID;

	private bool m_isSkillUnlocked;

	private Dictionary<int, FinalStrikeIcon> m_fsIconDict = new Dictionary<int, FinalStrikeIcon>();

	private Dictionary<int, FinalStrikeTierConnector> m_fsConnectorDict = new Dictionary<int, FinalStrikeTierConnector>();

	private FinalStrikeIcon m_mainIcon;

	private FinalStrikeIcon m_subIcon;

	private UpgradeEffect m_upgradeEffect;

	private StarUpEffect m_starUpEffect;

	private IEnumerator starUpEffectCoroutine;

	private int m_totalFSLevel;

	private int m_totalPower;

	private int m_totalHP;

	private int m_totalDefense;

	private FinalStrikeInfo m_currentFSInfo;

	private FS_TABLE m_currentFSTable;

	private SKILL_TABLE m_currentSkillTable;

	private STAR_TABLE m_currentStarTable;

	private NetPlayerInfo m_currentNetPlayerInfo;

	private int m_starCount;

	private int m_skillLevel = 1;

	private int itemNeeded;

	public override void SetCanvas(bool enable)
	{
		if (enable)
		{
			if (m_basicPage.activeSelf)
			{
				RefreshBasicPage();
			}
			else if (m_upgradePage.activeSelf)
			{
				RefreshLevelUpPage();
			}
			else if (m_starPage.activeSelf)
			{
				RefreshStarPage();
			}
		}
		base.SetCanvas(enable);
	}

	public void Setup()
	{
		InitLayout();
		m_defaultLvlBarSize = m_lvlUpLvlBar.rectTransform.sizeDelta;
		m_isSkillUnlocked = false;
		FSIconFunc(m_selectedFSID);
		BasicTab();
		RefreshLayout();
		RefreshCurrentAttributes();
	}

	public void OnClickEquipDialog(bool bEnable)
	{
		if (bEnable)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_equipSE);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		}
		EnableEquipDialog(bEnable);
	}

	public void EnableEquipDialog(bool bEnable)
	{
		if (bEnable)
		{
			m_mainIcon.gameObject.SetActive(false);
			m_subIcon.gameObject.SetActive(false);
			FinalStrikeInfo value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(m_currentNetPlayerInfo.MainWeaponFSID, out value))
			{
				m_mainIcon.Setup(value);
				m_mainIcon.SetButtonStatus(FinalStrikeIcon.BUTTON_STATUS.PRIMARY);
				m_mainIcon.gameObject.SetActive(true);
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(m_currentNetPlayerInfo.SubWeaponFSID, out value))
			{
				m_subIcon.Setup(value);
				m_subIcon.SetButtonStatus(FinalStrikeIcon.BUTTON_STATUS.SECONDARY);
				m_subIcon.gameObject.SetActive(true);
			}
		}
		m_equipDialog.SetActive(bEnable);
	}

	public void OnClickBasicTab()
	{
		if (!m_basicPage.activeSelf)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_tabSE);
			BasicTab();
		}
	}

	public void BasicTab()
	{
		m_basicPage.SetActive(true);
		m_upgradePage.SetActive(false);
		m_starPage.SetActive(false);
		RefreshBasicPage();
		m_basicPageUnlockSection.SetActive(!m_isSkillUnlocked);
		m_basicPageEquipSection.SetActive(m_isSkillUnlocked);
	}

	public void OnClickLevelUpTab()
	{
		if (!m_upgradePage.activeSelf)
		{
			m_basicPage.SetActive(false);
			m_upgradePage.SetActive(true);
			m_starPage.SetActive(false);
			RefreshLevelUpPage();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_tabSE);
		}
	}

	public void OnClickStarTab()
	{
		if (!m_starPage.activeSelf)
		{
			m_basicPage.SetActive(false);
			m_upgradePage.SetActive(false);
			m_starPage.SetActive(true);
			RefreshStarPage();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_tabSE);
		}
	}

	private FS_TABLE GetMaxLvlFS(int fsID)
	{
		List<FS_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == fsID).ToList();
		if (list.Count > 0)
		{
			return list[list.Count - 1];
		}
		return null;
	}

	private bool GetSkillTable(int fsID, int skillLvl, int starCount, out SKILL_TABLE skillTable)
	{
		bool result = false;
		skillTable = null;
		List<FS_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == fsID && x.n_LV == skillLvl).ToList();
		if (list.Count > 0)
		{
			FS_TABLE fS_TABLE = list[0];
			switch (starCount)
			{
			case 1:
				result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_1, out skillTable);
				break;
			case 2:
				result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_2, out skillTable);
				break;
			case 3:
				result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_3, out skillTable);
				break;
			case 4:
				result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_4, out skillTable);
				break;
			case 5:
				result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_5, out skillTable);
				break;
			default:
				result = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(fS_TABLE.n_SKILL_0, out skillTable);
				break;
			}
		}
		return result;
	}

	private void InitLayout()
	{
		UnityEngine.Object.Instantiate(m_rankNameStarsRef, m_rankNameStarsPos.transform);
		if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike != null)
		{
			Debug.Log("dicFinalStrike size = " + ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.Count);
		}
		float num = 310f;
		float num2 = 220f;
		float num3 = 350f;
		float num4 = 440f;
		float num5 = 380f;
		List<FS_TABLE> list = null;
		List<FS_TABLE> list2 = null;
		m_fsIconDict.Clear();
		m_fsConnectorDict.Clear();
		list2 = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_LV == 1).ToList();
		int curDepth = 1;
		for (int i = 0; i < list2.Count; i++)
		{
			list = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_LV == 1 && x.n_DEPTH == curDepth).ToList();
			Vector3 localPosition = Vector3.zero;
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				int fsID = list[j].n_FS_ID;
				if (curDepth == 1 && j == 0)
				{
					m_selectedFSID = fsID;
				}
				localPosition.y = (float)(curDepth - 1) * (0f - num);
				switch (count)
				{
				case 3:
					localPosition.x = 0f - num3 + (float)j * num5;
					break;
				case 2:
					localPosition.x = 0f - num2 + (float)j * num4;
					break;
				}
				FinalStrikeIcon finalStrikeIcon = UnityEngine.Object.Instantiate(m_finalStrikeIconRef, m_iconStartRoot.transform);
				m_fsIconDict.Add(fsID, finalStrikeIcon);
				finalStrikeIcon.GetComponentInChildren<Button>().onClick.AddListener(delegate
				{
					OnClickFSIcon(fsID);
				});
				FinalStrikeInfo finalStrikeInfo = new FinalStrikeInfo();
				finalStrikeInfo.netFinalStrikeInfo = new NetFinalStrikeInfo();
				finalStrikeInfo.netFinalStrikeInfo.FinalStrikeID = list[j].n_FS_ID;
				finalStrikeInfo.netFinalStrikeInfo.Level = 0;
				finalStrikeIcon.Setup(finalStrikeInfo);
				finalStrikeIcon.transform.localPosition = localPosition;
				finalStrikeIcon.gameObject.SetActive(true);
			}
			if (curDepth > 1)
			{
				FinalStrikeTierConnector finalStrikeTierConnector = null;
				float num6 = 150f;
				Vector3 localPosition2 = new Vector3(0f, (float)(curDepth - 2) * (0f - num) - num6, 0f);
				switch (count)
				{
				case 1:
					finalStrikeTierConnector = UnityEngine.Object.Instantiate(m_tierConnector_1, m_iconStartRoot.transform);
					break;
				case 2:
					finalStrikeTierConnector = UnityEngine.Object.Instantiate(m_tierConnector_2, m_iconStartRoot.transform);
					break;
				case 3:
					finalStrikeTierConnector = UnityEngine.Object.Instantiate(m_tierConnector_3, m_iconStartRoot.transform);
					break;
				}
				if ((bool)finalStrikeTierConnector)
				{
					finalStrikeTierConnector.gameObject.SetActive(true);
					finalStrikeTierConnector.transform.localPosition = localPosition2;
					finalStrikeTierConnector.SetLevel(list[0].n_UNLOCK_LV);
					finalStrikeTierConnector.SetEnable(false);
					m_fsConnectorDict.Add(list[0].n_UNLOCK_LV, finalStrikeTierConnector);
				}
			}
			curDepth++;
		}
		RectTransform component = m_iconStartRoot.GetComponent<RectTransform>();
		Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(component);
		Debug.Log("FinalStrike count = " + list2.Count);
		m_scrollRect.content.GetComponent<GridLayoutGroup>().cellSize = bounds.size;
		m_iconStartRoot.transform.localPosition = -bounds.center;
		component.sizeDelta = bounds.size;
		m_mainIcon = m_equipDialogMainIcon.GetComponentInChildren<FinalStrikeIcon>();
		m_subIcon = m_equipDialogSubIcon.GetComponentInChildren<FinalStrikeIcon>();
		if (m_mainIcon == null)
		{
			m_mainIcon = UnityEngine.Object.Instantiate(m_finalStrikeIconRef, m_equipDialogMainIcon.transform);
		}
		if (m_subIcon == null)
		{
			m_subIcon = UnityEngine.Object.Instantiate(m_finalStrikeIconRef, m_equipDialogSubIcon.transform);
		}
	}

	private void RefreshCurrentAttributes()
	{
		m_currentLevel.text = m_totalFSLevel.ToString();
		m_currentPower.text = m_totalPower.ToString();
		m_currentHP.text = m_totalHP.ToString();
		m_currentDefense.text = m_totalDefense.ToString();
	}

	public void RefreshLayout()
	{
		foreach (KeyValuePair<int, FinalStrikeIcon> item in m_fsIconDict)
		{
			int key = item.Key;
			FinalStrikeIcon value3 = item.Value;
			FinalStrikeInfo value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(key, out value))
			{
				m_fsIconDict[key].Setup(value);
				m_fsIconDict[key].SetButtonStatus(FinalStrikeIcon.BUTTON_STATUS.UNLOCKED);
				if (m_currentNetPlayerInfo != null)
				{
					if (key == m_currentNetPlayerInfo.MainWeaponFSID)
					{
						m_fsIconDict[key].SetButtonStatus(FinalStrikeIcon.BUTTON_STATUS.PRIMARY);
					}
					else if (key == m_currentNetPlayerInfo.SubWeaponFSID)
					{
						m_fsIconDict[key].SetButtonStatus(FinalStrikeIcon.BUTTON_STATUS.SECONDARY);
					}
				}
			}
			else
			{
				value = new FinalStrikeInfo();
				value.netFinalStrikeInfo = new NetFinalStrikeInfo();
				value.netFinalStrikeInfo.FinalStrikeID = key;
				value.netFinalStrikeInfo.Level = 1;
				value.netFinalStrikeInfo.Star = 0;
				m_fsIconDict[key].Setup(value);
				m_fsIconDict[key].SetButtonStatus(FinalStrikeIcon.BUTTON_STATUS.LOCKED);
			}
		}
		foreach (KeyValuePair<int, FinalStrikeTierConnector> item2 in m_fsConnectorDict)
		{
			int key2 = item2.Key;
			FinalStrikeTierConnector value2 = item2.Value;
			ManagedSingleton<PlayerHelper>.Instance.GetLV();
			value2.SetEnable(m_totalFSLevel >= key2);
		}
	}

	private void RefreshBasicPage()
	{
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(m_currentSkillTable.w_TIP);
		float num = 0f;
		if (m_currentSkillTable.n_EFFECT == 1)
		{
			num = m_currentSkillTable.f_EFFECT_X;
		}
		m_basicSkillDesc.text = string.Format(l10nValue, num.ToString("0.00"));
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetShowcase(m_currentSkillTable.s_SHOWCASE), m_currentSkillTable.s_SHOWCASE, delegate(Sprite asset)
		{
			if (asset != null)
			{
				m_basicSkillShowcase.sprite = asset;
			}
			m_basicSkillShowcase.gameObject.SetActive(asset != null);
		});
		List<FS_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == m_selectedFSID).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		FS_TABLE fS_TABLE = list[m_skillLevel - 1];
		FS_TABLE fS_TABLE2 = list[list.Count - 1];
		int count = list.Count;
		m_basicLevel.text = m_skillLevel.ToString();
		m_basicLevelBar.transform.localScale = new Vector3((float)m_skillLevel / (float)count, 1f, 1f);
		m_basicAttack.text = fS_TABLE.n_ATK.ToString();
		m_basicAttackBar.transform.localScale = CalcBarRatio(fS_TABLE.n_ATK, fS_TABLE2.n_ATK);
		m_basicHP.text = fS_TABLE.n_HP.ToString();
		m_basicHPBar.transform.localScale = CalcBarRatio(fS_TABLE.n_HP, fS_TABLE2.n_HP);
		m_basicDefense.text = fS_TABLE.n_DEF.ToString();
		m_basicDefenseBar.transform.localScale = CalcBarRatio(fS_TABLE.n_DEF, fS_TABLE2.n_DEF);
		if (m_isSkillUnlocked)
		{
			m_basicUnlockEquipBtnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_EQUIP");
			m_basicUnlockEquipBtn.interactable = true;
		}
		else
		{
			string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REQUIRE_FS_TOTALLV"), m_currentFSTable.n_UNLOCK_LV);
			m_basicUnlockEquipBtnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_UNLOCK");
			m_basicUnlockDescription.text = text;
			if (m_totalFSLevel < m_currentFSTable.n_UNLOCK_LV)
			{
				m_basicUnlockDescription.color = Color.red;
			}
			else
			{
				m_basicUnlockDescription.color = Color.white;
			}
			RefreshUnlockRequirement(fS_TABLE);
		}
		m_basicPageUnlockSection.SetActive(!m_isSkillUnlocked);
		m_basicPageEquipSection.SetActive(m_isSkillUnlocked);
		if (m_currentNetPlayerInfo != null && (m_selectedFSID == m_currentNetPlayerInfo.MainWeaponFSID || m_selectedFSID == m_currentNetPlayerInfo.SubWeaponFSID))
		{
			m_basicUnlockEquipBtnText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_CHANGE");
		}
	}

	private void RefreshUnlockRequirement(FS_TABLE fsTableTargetLvl)
	{
		itemNeeded = 0;
		if (fsTableTargetLvl.n_UNLOCK_ID == 0)
		{
			m_basicUnlockItem.SetActive(false);
			bool interactable = m_totalFSLevel >= fsTableTargetLvl.n_UNLOCK_LV;
			m_basicUnlockEquipBtn.interactable = interactable;
			return;
		}
		m_basicUnlockItem.SetActive(true);
		itemNeeded = fsTableTargetLvl.n_UNLOCK_COUNT;
		int num = 0;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(fsTableTargetLvl.n_UNLOCK_ID))
		{
			num = ManagedSingleton<PlayerNetManager>.Instance.dicItem[fsTableTargetLvl.n_UNLOCK_ID].netItemInfo.Stack;
		}
		string w_NAME = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[fsTableTargetLvl.n_UNLOCK_ID].w_NAME;
		m_basicUnlockItemCount.text = string.Format("{0}/{1}", num, itemNeeded);
		m_basicUnlockItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(w_NAME);
		m_basicUnlockItemCountBar.transform.localScale = CalcBarRatio(num, itemNeeded);
		m_basicUnlockItemAddBtn.onClick.RemoveAllListeners();
		m_basicUnlockItemAddBtn.onClick.AddListener(delegate
		{
			OnClickItemIcon(fsTableTargetLvl.n_UNLOCK_ID);
		});
		ITEM_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.ITEM_TABLE_DICT.TryGetValue(fsTableTargetLvl.n_UNLOCK_ID, out value) && m_materialIconRef != null)
		{
			foreach (Transform item in m_basicUnlockItemIconPos.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			UnityEngine.Object.Instantiate(m_materialIconRef, m_basicUnlockItemIconPos.transform).SetItemWithAmount(fsTableTargetLvl.n_UNLOCK_ID, num, OnClickItemIcon);
		}
		bool interactable2 = m_totalFSLevel >= fsTableTargetLvl.n_UNLOCK_LV && num >= itemNeeded;
		m_basicUnlockEquipBtn.interactable = interactable2;
	}

	private void RefreshLevelUpPage()
	{
		int num = 0;
		if (m_currentFSInfo != null)
		{
			num = m_currentFSInfo.netFinalStrikeInfo.Level;
		}
		m_lvlUpLvlText.text = num.ToString();
		FS_TABLE maxLvlFS = GetMaxLvlFS(m_selectedFSID);
		Vector3 vector = CalcBarRatio(num, maxLvlFS.n_LV);
		m_lvlUpLvlBar.rectTransform.sizeDelta = new Vector2(m_defaultLvlBarSize.x * vector.x, m_defaultLvlBarSize.y);
		m_lvlUpLvlPercentage.text = (int)(vector.x * 100f) + "%";
		m_lvlUpAttackOriginal.text = "---";
		m_lvlUpHPOriginal.text = "---";
		m_lvlUpDefenseOriginal.text = "---";
		m_lvlUpAttackNew.text = "---";
		m_lvlUpHPNew.text = "---";
		m_lvlUpDefenseNew.text = "---";
		if (m_currentStarTable != null)
		{
			m_lvlUpAttackOriginal.text = ((int)((float)m_currentFSTable.n_ATK * (1f + m_currentStarTable.f_ATK))).ToString();
			m_lvlUpHPOriginal.text = ((int)((float)m_currentFSTable.n_HP * (1f + m_currentStarTable.f_HP))).ToString();
			m_lvlUpDefenseOriginal.text = ((int)((float)m_currentFSTable.n_DEF * (1f + m_currentStarTable.f_DEF))).ToString();
			List<FS_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == m_currentFSTable.n_FS_ID && x.n_LV == m_currentFSTable.n_LV + 1).ToList();
			if (list.Count > 0)
			{
				m_lvlUpAttackNew.text = ((int)((float)list[0].n_ATK * (1f + m_currentStarTable.f_ATK))).ToString();
				m_lvlUpHPNew.text = ((int)((float)list[0].n_HP * (1f + m_currentStarTable.f_HP))).ToString();
				m_lvlUpDefenseNew.text = ((int)((float)list[0].n_DEF * (1f + m_currentStarTable.f_DEF))).ToString();
			}
		}
		int n_MATERIAL = m_currentFSTable.n_MATERIAL;
		int n_MONEY = m_currentFSTable.n_MONEY;
		int num2 = 0;
		int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(OrangeConst.ITEMID_FS_POWERUP))
		{
			num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[OrangeConst.ITEMID_FS_POWERUP].netItemInfo.Stack;
		}
		m_lvlUpMaterialRequired.text = n_MATERIAL.ToString();
		m_lvlUpMoneyRequired.text = n_MONEY.ToString();
		m_lvlUpMoneyOwned.text = zenny.ToString();
		m_lvlUpMaterialOwned.text = num2.ToString();
		m_lvlUpMaterialOwned.color = Color.white;
		m_lvlUpMoneyOwned.color = Color.white;
		if (n_MATERIAL > num2)
		{
			m_lvlUpMaterialOwned.color = Color.red;
		}
		if (n_MONEY > zenny)
		{
			m_lvlUpMoneyOwned.color = Color.red;
		}
		m_lvlUpBtn.interactable = m_isSkillUnlocked;
		if (!m_isSkillUnlocked || zenny < n_MONEY || num2 < n_MATERIAL)
		{
			m_lvlUpBtn.interactable = false;
		}
		if (maxLvlFS.n_ID == m_currentFSTable.n_ID)
		{
			m_lvlUpBtn.interactable = false;
		}
	}

	private void RefreshStarPage()
	{
		bool flag = false;
		m_basicPageUnlockSection.SetActive(!m_isSkillUnlocked);
		m_basicPageEquipSection.SetActive(m_isSkillUnlocked);
		RankNameStars componentInChildren = m_rankNameStarsPos.GetComponentInChildren<RankNameStars>();
		if ((bool)componentInChildren)
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(m_currentSkillTable.w_NAME);
			componentInChildren.Setup((RankNameStars.RANK)m_currentFSTable.n_RARITY, l10nValue, m_starCount);
		}
		string l10nValue2 = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(m_currentSkillTable.w_TIP);
		float num = 0f;
		if (m_currentSkillTable.n_EFFECT == 1)
		{
			num = m_currentSkillTable.f_EFFECT_X;
		}
		m_starSkillDesc.text = string.Format(l10nValue2, num.ToString("0.00"));
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetShowcase(m_currentSkillTable.s_SHOWCASE), m_currentSkillTable.s_SHOWCASE, delegate(Sprite asset)
		{
			if (asset != null)
			{
				m_basicSkillShowcase.sprite = asset;
				m_starSkillShowcase.sprite = asset;
			}
			m_basicSkillShowcase.gameObject.SetActive(asset != null);
			m_starSkillShowcase.gameObject.SetActive(asset != null);
		});
		m_starPowerOriginal.text = "---";
		m_starPowerNew.text = "---";
		m_atkAfter.text = "---";
		m_hpAfter.text = "---";
		m_defAfter.text = "---";
		List<STAR_TABLE> list = (from x in ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values
			where x.n_TYPE == 3 && x.n_MAINID == m_currentFSTable.n_FS_ID
			where x.n_STAR == m_starCount + 1
			select x).ToList();
		if (m_currentStarTable != null)
		{
			int num2 = (int)((float)m_currentFSTable.n_ATK * (1f + m_currentStarTable.f_ATK));
			int num3 = (int)((float)m_currentFSTable.n_HP * (1f + m_currentStarTable.f_HP));
			int num4 = (int)((float)m_currentFSTable.n_DEF * (1f + m_currentStarTable.f_DEF));
			m_atkBefore.text = num2.ToString();
			m_hpBefore.text = num3.ToString();
			m_defBefore.text = num4.ToString();
			m_starPowerOriginal.text = (num2 * OrangeConst.BP_ATK + num3 * OrangeConst.BP_HP + num4 * OrangeConst.BP_DEF).ToString();
		}
		if (list.Count > 0)
		{
			int num5 = (int)((float)m_currentFSTable.n_ATK * (1f + list[0].f_ATK));
			int num6 = (int)((float)m_currentFSTable.n_HP * (1f + list[0].f_HP));
			int num7 = (int)((float)m_currentFSTable.n_DEF * (1f + list[0].f_DEF));
			m_atkAfter.text = num5.ToString();
			m_hpAfter.text = num6.ToString();
			m_defAfter.text = num7.ToString();
			m_starPowerNew.text = (num5 * OrangeConst.BP_ATK + num6 * OrangeConst.BP_HP + num7 * OrangeConst.BP_DEF).ToString();
		}
		List<FS_TABLE> fsTable_lvl1 = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == m_currentFSTable.n_FS_ID && x.n_LV == 1).ToList();
		List<STAR_TABLE> list2 = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values.Where((STAR_TABLE x) => x.n_TYPE == 3 && x.n_MAINID == fsTable_lvl1[0].n_FS_ID && x.n_STAR == m_starCount).ToList();
		if (list2.Count > 0)
		{
			if (list2[0].n_MATERIAL == 0)
			{
				m_starUnlockMaterial.SetActive(false);
			}
			else
			{
				m_starUnlockMaterial.SetActive(true);
				flag = true;
				MATERIAL_TABLE materialTable;
				if (ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(list2[0].n_MATERIAL, out materialTable) && m_materialIconRef != null)
				{
					foreach (Transform item in m_starMaterialIconPos.transform)
					{
						UnityEngine.Object.Destroy(item.gameObject);
					}
					UnityEngine.Object.Instantiate(m_materialIconRef, m_starMaterialIconPos.transform).SetupMaterial(list2[0].n_MATERIAL, materialTable.n_MATERIAL_1, OnClickItemIcon);
					m_starMaterialAddBtn.onClick.RemoveAllListeners();
					m_starMaterialAddBtn.onClick.AddListener(delegate
					{
						OnClickItemIcon(materialTable.n_MATERIAL_1);
					});
					int n_MATERIAL_MOUNT = materialTable.n_MATERIAL_MOUNT1;
					int num8 = 0;
					if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(materialTable.n_MATERIAL_1))
					{
						num8 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[materialTable.n_MATERIAL_1].netItemInfo.Stack;
					}
					string w_NAME = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[materialTable.n_MATERIAL_1].w_NAME;
					m_starMaterialCount.text = string.Format("{0}/{1}", num8, n_MATERIAL_MOUNT);
					m_starMaterialName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(w_NAME);
					m_starMaterialCountBar.transform.localScale = CalcBarRatio(num8, n_MATERIAL_MOUNT);
				}
			}
			int firstNotEnoughItemID = 0;
			flag &= ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(list2[0].n_MATERIAL, out firstNotEnoughItemID);
		}
		if (flag && m_isSkillUnlocked)
		{
			m_starUpgradeBtn.interactable = true;
		}
		else
		{
			m_starUpgradeBtn.interactable = false;
		}
	}

	private void OnClickItemIcon(int p_idx)
	{
		ITEM_TABLE itemTable;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(p_idx, out itemTable))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
			{
				ui.Setup(itemTable, itemNeeded);
			});
		}
	}

	private void UpdateCacheValues()
	{
		m_skillLevel = 1;
		m_starCount = 0;
		m_isSkillUnlocked = ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(m_selectedFSID, out m_currentFSInfo);
		if (m_isSkillUnlocked)
		{
			m_skillLevel = m_currentFSInfo.netFinalStrikeInfo.Level;
			m_starCount = m_currentFSInfo.netFinalStrikeInfo.Star;
		}
		List<FS_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == m_selectedFSID && x.n_LV == m_skillLevel).ToList();
		m_currentFSTable = null;
		if (list.Count > 0)
		{
			m_currentFSTable = list[0];
		}
		GetSkillTable(m_selectedFSID, m_skillLevel, m_starCount, out m_currentSkillTable);
		m_currentStarTable = null;
		if (m_currentFSTable != null)
		{
			List<STAR_TABLE> list2 = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values.Where((STAR_TABLE x) => x.n_TYPE == 3 && x.n_MAINID == m_currentFSTable.n_FS_ID && x.n_STAR == m_starCount).ToList();
			if (list2.Count > 0)
			{
				m_currentStarTable = list2[0];
			}
		}
		m_totalFSLevel = 0;
		m_totalPower = 0;
		m_totalHP = 0;
		m_totalDefense = 0;
		foreach (KeyValuePair<int, FinalStrikeInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike)
		{
			int key = item.Key;
			FinalStrikeInfo fsInfo = item.Value;
			m_totalFSLevel += fsInfo.netFinalStrikeInfo.Level;
			List<FS_TABLE> list3 = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == fsInfo.netFinalStrikeInfo.FinalStrikeID && x.n_LV == fsInfo.netFinalStrikeInfo.Level).ToList();
			if (list3.Count > 0)
			{
				m_totalPower += list3[0].n_ATK;
				m_totalHP += list3[0].n_HP;
				m_totalDefense += list3[0].n_DEF;
			}
		}
		m_currentNetPlayerInfo = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo;
		RefreshCurrentAttributes();
		if (m_redDotUnLock != null)
		{
			m_redDotUnLock.gameObject.SetActive(ManagedSingleton<HintHelper>.Instance.IsFinalStrikeCanUnLock(m_selectedFSID));
		}
		if (m_redDotStart != null)
		{
			m_redDotStart.gameObject.SetActive(ManagedSingleton<HintHelper>.Instance.IsFinalStrikeCanStarUp(m_selectedFSID));
		}
	}

	public void OnClickFSIcon(int fsID)
	{
		if (m_selectedFSID != fsID)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		FSIconFunc(fsID);
	}

	public void FSIconFunc(int fsID)
	{
		if (starUpEffectCoroutine != null)
		{
			StopCoroutine(starUpEffectCoroutine);
			starUpEffectCoroutine = null;
			m_starUpgradeBtn.interactable = true;
			RefreshLayout();
			RefreshStarPage();
		}
		m_fsIconDict[m_selectedFSID].SetSelected(false);
		m_fsIconDict[fsID].SetSelected(true);
		m_selectedFSID = fsID;
		UpdateCacheValues();
		RankNameStars componentInChildren = m_rankNameStarsPos.GetComponentInChildren<RankNameStars>();
		if ((bool)componentInChildren)
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(m_currentSkillTable.w_NAME);
			componentInChildren.Setup((RankNameStars.RANK)m_currentFSTable.n_RARITY, l10nValue, m_starCount);
		}
		if (m_basicPage.activeSelf)
		{
			RefreshBasicPage();
		}
		else if (m_upgradePage.activeSelf)
		{
			RefreshLevelUpPage();
		}
		else if (m_starPage.activeSelf)
		{
			RefreshStarPage();
		}
	}

	public void OnClickUnlockEquipBtn()
	{
		if (!m_basicUnlockEquipBtn.IsInteractable())
		{
			return;
		}
		if (m_isSkillUnlocked)
		{
			if (m_currentNetPlayerInfo != null)
			{
				OnClickEquipDialog(true);
			}
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_unlockSE);
		ManagedSingleton<PlayerNetManager>.Instance.FSUnlockReq(m_currentFSTable.n_ID, delegate
		{
			UpdateCacheValues();
			RefreshBasicPage();
			RefreshLayout();
			StartCoroutine(PlayUnlockTier(false));
		});
	}

	public void OnClickEquipPrimary()
	{
		EnableEquipDialog(false);
		if (m_selectedFSID == m_currentNetPlayerInfo.SubWeaponFSID)
		{
			ManagedSingleton<PlayerNetManager>.Instance.FSSetupReq(m_currentNetPlayerInfo.MainWeaponFSID, WeaponWieldType.SubWeapon, delegate
			{
			});
		}
		ManagedSingleton<PlayerNetManager>.Instance.FSSetupReq(m_selectedFSID, WeaponWieldType.MainWeapon, delegate
		{
			UpdateCacheValues();
			RefreshLayout();
			RefreshBasicPage();
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
	}

	public void OnClickEquipSecondary()
	{
		EnableEquipDialog(false);
		if (m_selectedFSID == m_currentNetPlayerInfo.MainWeaponFSID)
		{
			ManagedSingleton<PlayerNetManager>.Instance.FSSetupReq(m_currentNetPlayerInfo.SubWeaponFSID, WeaponWieldType.MainWeapon, delegate
			{
			});
		}
		ManagedSingleton<PlayerNetManager>.Instance.FSSetupReq(m_selectedFSID, WeaponWieldType.SubWeapon, delegate
		{
			UpdateCacheValues();
			RefreshLayout();
			RefreshBasicPage();
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
	}

	public void OnClickUpgradeStarBtn()
	{
		m_starUpgradeBtn.interactable = false;
		List<FS_TABLE> fsTable_lvl1 = ManagedSingleton<OrangeDataManager>.Instance.FS_TABLE_DICT.Values.Where((FS_TABLE x) => x.n_FS_ID == m_currentFSTable.n_FS_ID && x.n_LV == 1).ToList();
		List<STAR_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.Values.Where((STAR_TABLE x) => x.n_TYPE == 3 && x.n_MAINID == fsTable_lvl1[0].n_FS_ID && x.n_STAR == m_starCount).ToList();
		ManagedSingleton<PlayerNetManager>.Instance.FSUpgradeStarReq(m_selectedFSID, list[0].n_ID, delegate
		{
			UpdateCacheValues();
			FSIconFunc(m_selectedFSID);
			PlayUpgradeEffect();
			starUpEffectCoroutine = PlayStarUpEffectAndRefreshMenu();
			StartCoroutine(starUpEffectCoroutine);
		});
	}

	public void OnClickDetailInfo()
	{
		m_detailInfo.gameObject.SetActive(true);
		m_detailInfo.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
		LeanTween.scale(m_detailInfo.gameObject, new Vector3(1f, 1f, 1f), 0.5f).setEaseOutExpo();
		CanvasGroup canvasGroup = m_detailInfo.GetComponent<CanvasGroup>();
		if ((bool)canvasGroup)
		{
			LeanTween.value(0f, 1f, 0.5f).setOnUpdate(delegate(float f)
			{
				canvasGroup.alpha = f;
			}).setEaseOutExpo();
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickInfoSE);
	}

	public void OnCloseDetailInfo()
	{
		m_detailInfo.transform.localScale = new Vector3(1f, 1f, 1f);
		LeanTween.scale(m_detailInfo.gameObject, new Vector3(0.01f, 0.01f, 1f), 0.5f).setEaseOutExpo().setOnComplete((Action)delegate
		{
			m_detailInfo.gameObject.SetActive(false);
		});
		CanvasGroup canvasGroup = m_detailInfo.GetComponent<CanvasGroup>();
		if ((bool)canvasGroup)
		{
			LeanTween.value(1f, 0f, 0.5f).setOnUpdate(delegate(float f)
			{
				canvasGroup.alpha = f;
			}).setEaseOutExpo();
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
	}

	private void PlayUpgradeEffect()
	{
		if (m_upgradeEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgradeeffect", "UpgradeEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_upgradeEffect = gameObject.GetComponent<UpgradeEffect>();
				m_upgradeEffect.Play(m_fsIconDict[m_selectedFSID].transform.position);
			});
		}
		else
		{
			m_upgradeEffect.Play(m_fsIconDict[m_selectedFSID].transform.position);
		}
	}

	private IEnumerator PlayStarUpEffectAndRefreshMenu()
	{
		if (m_starCount >= 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
			yield return new WaitForSeconds(0.5f);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_rankUPSE);
			yield return new WaitForSeconds(0.5f);
			PlayStarUpEffect();
			yield return new WaitForSeconds(0.25f);
			m_starUpgradeBtn.interactable = true;
			RefreshLayout();
			RefreshStarPage();
		}
	}

	private void PlayStarUpEffect()
	{
		RankNameStars componentInChildren = m_rankNameStarsPos.GetComponentInChildren<RankNameStars>();
		if (componentInChildren == null)
		{
			return;
		}
		Vector3 starPos = componentInChildren.GetStarPosition(m_starCount - 1);
		if (m_starUpEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "starupeffect", "StarUpEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_starUpEffect = gameObject.GetComponent<StarUpEffect>();
				m_starUpEffect.Play(starPos);
			});
		}
		else
		{
			m_starUpEffect.Play(starPos);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP02);
	}

	public void OnClickLevelUpBtn()
	{
		m_lvlUpBtn.interactable = false;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<PlayerNetManager>.Instance.FSLevelUpReq(m_currentFSTable.n_ID, delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_strengthSE);
			UpdateCacheValues();
			m_lvlUpBtn.interactable = true;
			RefreshLevelUpPage();
			PlayUpgradeEffect();
			StartCoroutine(PlayUnlockTier());
		});
	}

	private IEnumerator PlayUnlockTier(bool refresh = true)
	{
		yield return new WaitForSeconds(0.5f);
		if (refresh)
		{
			RefreshLayout();
		}
		foreach (KeyValuePair<int, FinalStrikeTierConnector> item in m_fsConnectorDict)
		{
			int key = item.Key;
			if (m_totalFSLevel == key)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OPEN01);
			}
		}
	}

	private Vector3 CalcBarRatio(float current, float max)
	{
		return new Vector3(Mathf.Clamp(current / max, 0f, 1f), 1f, 1f);
	}
}
