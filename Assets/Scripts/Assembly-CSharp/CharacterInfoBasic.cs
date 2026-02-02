#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using DragonBones;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

internal class CharacterInfoBasic : OrangeUIBase
{
	[SerializeField]
	private GameObject btnDeploy;

	[SerializeField]
	private GameObject btnUnlock;

	[SerializeField]
	private GameObject unlockItemPos;

	[SerializeField]
	private GameObject[] skillButtonPositions = new GameObject[2];

	[SerializeField]
	private GameObject unlockGroup;

	[SerializeField]
	private DiamondShape m_abilityChart;

	[SerializeField]
	private UnityEngine.Transform m_deployedFrame;

	[SerializeField]
	private Button m_bookButton;

	[SerializeField]
	private GameObject m_bookFrame;

	[SerializeField]
	private UnityArmatureComponent BookUPEffect;

	[SerializeField]
	private OrangeText m_galleryProgressText;

	[SerializeField]
	private RectTransform m_galleryProgressBar;

	[SerializeField]
	private Image m_progressBarImg;

	[SerializeField]
	private Image m_unlockItemCountBar;

	[SerializeField]
	private OrangeText m_unlockItemCountText;

	[SerializeField]
	private OrangeText m_unlockMaterialName;

	[SerializeField]
	private OrangeText m_playerTip1;

	[SerializeField]
	private OrangeText m_playerTip2;

	[SerializeField]
	private OrangeText m_playerTip3;

	[SerializeField]
	public Image[] m_colorBar;

	[SerializeField]
	private RectTransform scrollContent;

	[SerializeField]
	private RectTransform recordContent;

	[SerializeField]
	private Text[] attributrText;

	[SerializeField]
	private FillSliceImg[] attributrBar;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_unlockSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_switch2DSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_bookBtn;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_bookLVUPBtn;

	private UnityEngine.Transform[,] skillExtensionArrays = new UnityEngine.Transform[2, 3];

	private CharacterInfo characterInfo;

	private bool characterUnlocked;

	private CharacterInfoUI characterInfoUI;

	public CHARACTER_TABLE characterTable;

	private GameObject m_materialIcon;

	private bool bEffectLock;

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryInfo = new List<GALLERY_TABLE>();

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryUnlock = new List<GALLERY_TABLE>();

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryLock = new List<GALLERY_TABLE>();

	private void Start()
	{
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public override void SetCanvas(bool enable)
	{
		if (enable)
		{
			RefreshUnlockGroup();
		}
		base.SetCanvas(enable);
	}

	public void Setup(CharacterInfo info)
	{
		characterInfo = info;
		characterInfoUI = base.transform.parent.GetComponentInChildren<CharacterInfoUI>();
		characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.netInfo.CharacterID];
		RefreshUnlockGroup();
		List<int> list = new List<int>();
		list.Add(characterInfo.netInfo.CharacterID);
		new List<NetCharacterSkillInfo>();
		ManagedSingleton<PlayerNetManager>.Instance.GetNetCharacterSkillInfos(list.ToArray());
		int num = 2;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			foreach (UnityEngine.Transform item in skillButtonPositions[i].transform)
			{
				skillExtensionArrays[i, num2] = item;
				num2++;
			}
			num2 = 0;
		}
		CreateSkillButton(0);
		CreateSkillButton(1);
		RefreshDeployButton();
		RefreshAbilityChart();
		RefreshTips();
		characterInfoUI.RefreshBadges();
		RefreshRecordVal();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void CreateSkillButton(int index)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("ui/skillbutton", "SkillButton", delegate(UnityEngine.Object asset)
		{
			GameObject gameObject = asset as GameObject;
			if (gameObject != null)
			{
				SkillButton component = UnityEngine.Object.Instantiate(gameObject, skillButtonPositions[index].transform, false).GetComponent<SkillButton>();
				if ((bool)component)
				{
					if (index == 0)
					{
						component.Setup(characterTable.n_SKILL1, SkillButton.StatusType.DEFAULT);
						if (characterUnlocked)
						{
							ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[characterTable.n_SKILL1].n_LVMAX.ToString();
							string text = characterInfo.netSkillDic[CharacterSkillSlot.ActiveSkill1].Level.ToString();
							component.OverrideText("Lv" + text);
						}
						CreateSkillExtButton(index, gameObject);
					}
					else
					{
						component.Setup(characterTable.n_SKILL2, SkillButton.StatusType.DEFAULT);
						if (characterUnlocked)
						{
							ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[characterTable.n_SKILL2].n_LVMAX.ToString();
							string text2 = characterInfo.netSkillDic[CharacterSkillSlot.ActiveSkill2].Level.ToString();
							component.OverrideText("Lv" + text2);
						}
						CreateSkillExtButton(index, gameObject);
					}
				}
			}
		});
	}

	private void CreateSkillExtButton(int index, GameObject buttonAsset)
	{
		SkillButton.StatusType status = SkillButton.StatusType.LOCKED;
		int length = skillExtensionArrays.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			SkillButton component = UnityEngine.Object.Instantiate(buttonAsset, skillExtensionArrays[index, i], false).GetComponent<SkillButton>();
			CharacterSkillSlot skillSlot = ((index == 0) ? CharacterSkillSlot.ActiveSkill1 : CharacterSkillSlot.ActiveSkill2);
			CharacterSkillEnhanceSlot skillEnhanceSlot = (CharacterSkillEnhanceSlot)(i + 1);
			int num;
			switch (i)
			{
			default:
				num = ((index == 0) ? characterTable.n_SKILL1_EX1 : characterTable.n_SKILL2_EX1);
				break;
			case 1:
				num = ((index == 0) ? characterTable.n_SKILL1_EX2 : characterTable.n_SKILL2_EX2);
				break;
			case 2:
				num = ((index == 0) ? characterTable.n_SKILL1_EX3 : characterTable.n_SKILL2_EX3);
				break;
			}
			if (num == characterTable.n_SKILL1_EX1)
			{
				status = ((characterInfo.netInfo.Star < characterTable.n_SKILL1_UNLOCK1) ? SkillButton.StatusType.LOCKED : SkillButton.StatusType.DEFAULT);
			}
			else if (num == characterTable.n_SKILL2_EX1)
			{
				status = ((characterInfo.netInfo.Star < characterTable.n_SKILL2_UNLOCK1) ? SkillButton.StatusType.LOCKED : SkillButton.StatusType.DEFAULT);
			}
			else if (num == characterTable.n_SKILL1_EX2)
			{
				status = ((characterInfo.netInfo.Star < characterTable.n_SKILL1_UNLOCK2) ? SkillButton.StatusType.LOCKED : SkillButton.StatusType.DEFAULT);
			}
			else if (num == characterTable.n_SKILL2_EX2)
			{
				status = ((characterInfo.netInfo.Star < characterTable.n_SKILL2_UNLOCK2) ? SkillButton.StatusType.LOCKED : SkillButton.StatusType.DEFAULT);
			}
			else if (num == characterTable.n_SKILL1_EX3)
			{
				status = ((characterInfo.netInfo.Star < characterTable.n_SKILL1_UNLOCK3) ? SkillButton.StatusType.LOCKED : SkillButton.StatusType.DEFAULT);
			}
			else if (num == characterTable.n_SKILL2_EX3)
			{
				status = ((characterInfo.netInfo.Star < characterTable.n_SKILL2_UNLOCK3) ? SkillButton.StatusType.LOCKED : SkillButton.StatusType.DEFAULT);
			}
			component.SetStyle(SkillButton.StyleType.SQUARE);
			component.Setup(skillSlot, skillEnhanceSlot, num, 1, status);
			component.OverrideText("");
		}
	}

	public void Setup()
	{
	}

	private void RefreshAbilityChart()
	{
		string text = characterTable.n_TREND.ToString();
		float num = 1f;
		float num2 = (float)char.GetNumericValue(text[0]) / 5f;
		float num3 = (float)char.GetNumericValue(text[1]) / 5f;
		float num4 = (float)char.GetNumericValue(text[2]) / 5f;
		float num5 = (float)char.GetNumericValue(text[3]) / 5f;
		float num6 = (float)char.GetNumericValue(text[4]) / 5f;
		LeanTween.value(0f, num2, num * num2).setOnUpdate(delegate(float val)
		{
			m_abilityChart.SetPointValue(0, val);
		});
		LeanTween.value(0f, num3, num * num3).setOnUpdate(delegate(float val)
		{
			m_abilityChart.SetPointValue(1, val);
		});
		LeanTween.value(0f, num4, num * num4).setOnUpdate(delegate(float val)
		{
			m_abilityChart.SetPointValue(2, val);
		});
		LeanTween.value(0f, num5, num * num5).setOnUpdate(delegate(float val)
		{
			m_abilityChart.SetPointValue(3, val);
		});
		LeanTween.value(0f, num6, num * num6).setOnUpdate(delegate(float val)
		{
			m_abilityChart.SetPointValue(4, val);
		});
	}

	private void RefreshDeployButton()
	{
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetStandByChara(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara) == characterTable)
		{
			Button component = btnDeploy.GetComponent<Button>();
			component.interactable = false;
			component.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_STARTERING");
			if ((bool)m_deployedFrame)
			{
				m_deployedFrame.gameObject.SetActive(true);
			}
		}
		else
		{
			Button component2 = btnDeploy.GetComponent<Button>();
			component2.interactable = true;
			component2.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_STARTER");
			if ((bool)m_deployedFrame)
			{
				m_deployedFrame.gameObject.SetActive(false);
			}
		}
	}

	private void RefreshTips()
	{
		string[] array = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(characterTable.w_TYPETIP).Split('\n');
		if (array.Length >= 1)
		{
			m_playerTip1.text = array[0];
		}
		if (array.Length >= 2)
		{
			m_playerTip2.text = array[1];
		}
		if (array.Length >= 3)
		{
			m_playerTip3.text = array[2];
		}
	}

	private void RefreshUnlockGroup()
	{
		int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(characterTable.n_UNLOCK_ID);
		int n_UNLOCK_COUNT = characterTable.n_UNLOCK_COUNT;
		float num = Mathf.Clamp((float)itemValue / (float)n_UNLOCK_COUNT, 0f, 1f);
		characterUnlocked = characterInfo.netInfo.State == 1;
		ITEM_TABLE value = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(characterTable.n_UNLOCK_ID, out value))
		{
			m_unlockMaterialName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
		}
		m_unlockItemCountText.text = string.Format("{0}/{1}", itemValue, characterTable.n_UNLOCK_COUNT);
		m_unlockItemCountBar.transform.localScale = new Vector3(num, 1f, 1f);
		btnUnlock.GetComponent<Button>().interactable = num >= 1f;
		if (m_materialIcon == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
			{
				m_materialIcon = UnityEngine.Object.Instantiate(asset, unlockItemPos.transform);
				CommonIconBase component = m_materialIcon.GetComponent<CommonIconBase>();
				if (!characterUnlocked)
				{
					component.SetupItem(characterTable.n_UNLOCK_ID, 0, OnClickMaterialIcon);
				}
			});
		}
		btnDeploy.SetActive(characterUnlocked);
		unlockGroup.SetActive(!characterUnlocked);
		m_bookButton.gameObject.SetActive(characterUnlocked);
		if (MonoBehaviourSingleton<UIManager>.Instance.GetUI<IllustrationTargetUI>("UI_IllustrationTarget") != null)
		{
			m_bookButton.gameObject.SetActive(false);
		}
		if (characterUnlocked)
		{
			CheckGalleryUnlock();
		}
	}

	public void OnClickAddMaterial()
	{
		ITEM_TABLE item;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(characterTable.n_UNLOCK_ID, out item))
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
				ui.Setup(itemTable, characterTable.n_UNLOCK_COUNT);
			});
		}
	}

	private IEnumerator WaitEffectPlayEnd()
	{
		while (BookUPEffect.gameObject.activeSelf)
		{
			yield return new WaitForSeconds(0.2f);
		}
		CheckGalleryUnlock();
	}

	public void onClickBookBtn()
	{
		Debug.Log("onClickBookBtn");
		if (BookUPEffect.isActiveAndEnabled)
		{
			return;
		}
		if (m_bookFrame.activeSelf)
		{
			m_bookFrame.SetActive(false);
			BookUPEffect.transform.gameObject.SetActive(true);
			BookUPEffect.animation.Reset();
			BookUPEffect.animation.Play("newAnimation", 1);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_bookLVUPBtn);
			LeanTween.delayedCall(BookUPEffect.animation.GetState("newAnimation").totalTime, (Action)delegate
			{
				BookUPEffect.transform.gameObject.SetActive(false);
			});
			List<int> galleryIDs = new List<int>();
			List<GALLERY_TABLE> lockInfo = new List<GALLERY_TABLE>();
			listGalleryLock.ForEach(delegate(GALLERY_TABLE tbl)
			{
				if (ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(tbl.n_ID))
				{
					listGalleryUnlock.Add(tbl);
					galleryIDs.Add(tbl.n_ID);
				}
				else
				{
					lockInfo.Add(tbl);
				}
			});
			if (galleryIDs.Count != 0)
			{
				ManagedSingleton<PlayerNetManager>.Instance.GalleryUnlockReq(galleryIDs, delegate
				{
					ManagedSingleton<GalleryHelper>.Instance.BuildGalleryInfo();
					StartCoroutine(WaitEffectPlayEnd());
				});
			}
			if (lockInfo.Count != 0)
			{
				listGalleryLock = lockInfo;
			}
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_IllustrationTarget", delegate(IllustrationTargetUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_bookBtn);
				ui.Setup(this);
			});
		}
	}

	public void OnClickUnlockBtn()
	{
		bEffectLock = true;
		ManagedSingleton<PlayerNetManager>.Instance.CharacterUnlockReq(characterInfo.netInfo.CharacterID, delegate
		{
			characterInfoUI.CheckCharacterUpgrades();
			characterInfoUI.RefreshSideButtonRedDots();
			ManagedSingleton<CharacterHelper>.Instance.SortCharacterList();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_unlockSE);
			ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(characterInfo.netInfo.CharacterID, out characterInfo);
			RefreshDeployButton();
			RefreshUnlockGroup();
			characterInfoUI.PlayUnlockEffect();
			characterInfoUI.RefreshBadges();
			characterInfoUI.RefreshQuickSelectBar(true);
			bEffectLock = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SHOP);
		});
	}

	public void OnClickDeployBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		MonoBehaviourSingleton<OrangeGameManager>.Instance.CharacterStandby(characterInfo.netInfo.CharacterID, delegate
		{
			RefreshDeployButton();
			characterInfoUI.RefreshBadges();
			characterInfoUI.RefreshQuickSelectBar();
		});
	}

	public void OnClick2DSwitch()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Portrait", delegate(CharacterInfoPortrait ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_switch2DSE);
			ui.Setup(characterTable, characterInfo);
		});
	}

	private void CheckGalleryUnlock()
	{
		int characterID = characterInfo.netInfo.CharacterID;
		ManagedSingleton<GalleryHelper>.Instance.GalleryGetTableAll(characterID, GalleryType.Character, out listGalleryInfo, out listGalleryUnlock, out listGalleryLock);
		bool active = listGalleryLock.Any((GALLERY_TABLE p) => ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(p.n_ID));
		m_bookFrame.gameObject.SetActive(active);
		float num = 194f;
		GalleryCalcResult galleryCalcResult = ManagedSingleton<GalleryHelper>.Instance.GalleryCalculationProgress(characterID, GalleryType.Character);
		float num2 = (float)galleryCalcResult.m_a / (float)galleryCalcResult.m_b;
		if (num2 < 0.333f)
		{
			m_progressBarImg.sprite = m_colorBar[0].sprite;
		}
		else if (num2 < 0.666f)
		{
			m_progressBarImg.sprite = m_colorBar[1].sprite;
		}
		else if (num2 < 0.999f)
		{
			m_progressBarImg.sprite = m_colorBar[2].sprite;
		}
		else
		{
			m_progressBarImg.sprite = m_colorBar[3].sprite;
		}
		num *= num2;
		m_galleryProgressBar.sizeDelta = new Vector2(num, m_galleryProgressBar.sizeDelta.y);
		m_galleryProgressText.text = (int)(num2 * 100f) + "%";
	}

	public bool IsEffectPlaying()
	{
		return false | bEffectLock | BookUPEffect.isActiveAndEnabled;
	}

	private void RefreshRecordVal()
	{
		int characterRecordVal = DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.BATTLE, characterInfo);
		int characterRecordVal2 = DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.EXPLORE, characterInfo);
		int characterRecordVal3 = DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.ACTION, characterInfo);
		attributrText[0].text = characterRecordVal.ToString();
		attributrBar[0].SetFValue((float)characterRecordVal / (float)OrangeConst.RECORD_BATTLE_MAX);
		attributrText[1].text = characterRecordVal2.ToString();
		attributrBar[1].SetFValue((float)characterRecordVal2 / (float)OrangeConst.RECORD_EXPLORE_MAX);
		attributrText[2].text = characterRecordVal3.ToString();
		attributrBar[2].SetFValue((float)characterRecordVal3 / (float)OrangeConst.RECORD_ACTION_MAX);
		recordContent.gameObject.SetActive(true);
		scrollContent.gameObject.SetActive(true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent);
	}
}
