using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoDNA : OrangeUIBase
{
	[SerializeField]
	private GameObject[] panel1SkillBtnPos;

	[SerializeField]
	private GameObject[] panel2SkillBtnPos;

	[SerializeField]
	private GameObject[] panel3SkillBtnPos;

	[SerializeField]
	private GameObject fxDNA;

	[SerializeField]
	private GameObject fxDNA2;

	[SerializeField]
	private GameObject skillBtnDecorator;

	[SerializeField]
	private Button connectDNA;

	[SerializeField]
	private Button disconnectDNA;

	[SerializeField]
	private CommonIconBase linkedPortrait;

	private CharacterInfo _characterInfo;

	private int _characterID;

	private DNA_TABLE[] _defaultDNATables;

	private DNA_TABLE[] _randomDNATables;

	private int _tweenDNAFX;

	private int _tweenFadeIn;

	private int _currentSkillCount;

	public void Setup(CharacterInfo characterInfo)
	{
		_characterInfo = characterInfo;
		_characterID = characterInfo.netInfo.CharacterID;
		_defaultDNATables = ManagedSingleton<OrangeDataManager>.Instance.DNA_TABLE_DICT.Values.Where((DNA_TABLE x) => x.n_CHARACTER == _characterID && x.n_TYPE == 0).ToArray();
		_randomDNATables = ManagedSingleton<OrangeDataManager>.Instance.DNA_TABLE_DICT.Values.Where((DNA_TABLE x) => x.n_CHARACTER == _characterID && x.n_TYPE == 1).ToArray();
		_currentSkillCount = _characterInfo.netDNAInfoDic.Count;
		InitSkillButtons();
		UpdateDNAFX();
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.ContainsKey(_characterID))
		{
			connectDNA.interactable = false;
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref _tweenDNAFX);
		LeanTween.cancel(ref _tweenFadeIn);
	}

	private void UpdateDNAFX()
	{
		if (_characterInfo.netDNAInfoDic.Count == 8)
		{
			EnableDNAFX(false);
			EnableDNAFX2(true);
		}
		else
		{
			EnableDNAFX2(false);
			EnableDNAFX(true);
		}
	}

	private void EnableDNAFX(bool bEnable)
	{
		float delayTime = 0.5f;
		float fadeInTime = 1.5f;
		LeanTween.cancel(ref _tweenDNAFX);
		LeanTween.cancel(ref _tweenFadeIn);
		if (!bEnable)
		{
			fxDNA.SetActive(false);
			return;
		}
		_tweenDNAFX = LeanTween.delayedCall(delayTime, (Action)delegate
		{
			if (!(this == null))
			{
				fxDNA.SetActive(true);
				CanvasGroup canvasGroup = fxDNA.GetComponent<CanvasGroup>();
				_tweenFadeIn = LeanTween.value(0f, 1f, fadeInTime).setOnUpdate(delegate(float val)
				{
					canvasGroup.alpha = val;
				}).uniqueId;
			}
		}).uniqueId;
	}

	private void EnableDNAFX2(bool bEnable)
	{
		float delayTime = 0.5f;
		float fadeInTime = 1.5f;
		LeanTween.cancel(ref _tweenDNAFX);
		LeanTween.cancel(ref _tweenFadeIn);
		if (!bEnable)
		{
			fxDNA2.SetActive(false);
			return;
		}
		_tweenDNAFX = LeanTween.delayedCall(delayTime, (Action)delegate
		{
			if (!(this == null))
			{
				fxDNA2.SetActive(true);
				CanvasGroup canvasGroup = fxDNA2.GetComponent<CanvasGroup>();
				_tweenFadeIn = LeanTween.value(0f, 1f, fadeInTime).setOnUpdate(delegate(float val)
				{
					canvasGroup.alpha = val;
				}).uniqueId;
			}
		}).uniqueId;
	}

	private void InitSkillButtons()
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("ui/skillbutton", "SkillButton", delegate(UnityEngine.Object asset)
		{
			GameObject buttonAsset = asset as GameObject;
			InitDefaultSkills(buttonAsset);
			InitRandomSkills(buttonAsset);
			InitInheritSkills(buttonAsset);
		});
	}

	private void InitDefaultSkills(GameObject buttonAsset)
	{
		GameObject[] array = panel1SkillBtnPos;
		foreach (GameObject gameObject in array)
		{
			GameObject obj = UnityEngine.Object.Instantiate(buttonAsset, gameObject.transform, false);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(skillBtnDecorator, gameObject.transform, false);
			obj.GetComponent<SkillButton>().transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			gameObject2.SetActive(true);
		}
		RefreshDefaultSkillIcons();
	}

	private void InitRandomSkills(GameObject buttonAsset)
	{
		GameObject[] array = panel2SkillBtnPos;
		foreach (GameObject gameObject in array)
		{
			GameObject obj = UnityEngine.Object.Instantiate(buttonAsset, gameObject.transform, false);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(skillBtnDecorator, gameObject.transform, false);
			obj.GetComponent<SkillButton>().transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			gameObject2.SetActive(true);
		}
		RefreshRandomSkillIcons();
	}

	private void InitInheritSkills(GameObject buttonAsset)
	{
		GameObject[] array = panel3SkillBtnPos;
		foreach (GameObject gameObject in array)
		{
			GameObject obj = UnityEngine.Object.Instantiate(buttonAsset, gameObject.transform, false);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(skillBtnDecorator, gameObject.transform, false);
			obj.GetComponent<SkillButton>().transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			gameObject2.SetActive(true);
		}
		RefreshInheritSkillIcons();
	}

	private void RefreshDefaultSkillIcons()
	{
		int num = 0;
		GameObject[] array = panel1SkillBtnPos;
		foreach (GameObject obj in array)
		{
			SkillButtonDecorator componentInChildren = obj.GetComponentInChildren<SkillButtonDecorator>();
			SkillButton componentInChildren2 = obj.GetComponentInChildren<SkillButton>();
			componentInChildren2.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			componentInChildren2.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
			DNA_TABLE dnaTable = null;
			if (num < _defaultDNATables.Length)
			{
				dnaTable = _defaultDNATables[num];
				RANDOMSKILL_TABLE value;
				if (ManagedSingleton<OrangeDataManager>.Instance.RANDOMSKILL_TABLE_DICT.TryGetValue(dnaTable.n_GROUP, out value))
				{
					componentInChildren2.Setup(value.n_SKILL, SkillButton.StatusType.DEFAULT);
					int skillID = value.n_SKILL;
					componentInChildren2.GetComponentInChildren<Button>().onClick.AddListener(delegate
					{
						OnClickDefaultSkill(skillID, dnaTable);
					});
					if (_characterInfo.netDNAInfoDic.ContainsKey(num + 1))
					{
						if (_characterInfo.netDNAInfoDic.Count == 8)
						{
							componentInChildren.Setup(SkillButtonDecorator.StyleType.GOLD);
						}
						else
						{
							componentInChildren.Setup(SkillButtonDecorator.StyleType.UNLOCKED);
						}
					}
					else if (_characterInfo.netInfo.Star >= _defaultDNATables[num].n_STAR)
					{
						componentInChildren.Setup(SkillButtonDecorator.StyleType.UNLOCKABLE);
					}
					else
					{
						int n_STAR = _defaultDNATables[num].n_STAR;
						componentInChildren.Setup(SkillButtonDecorator.StyleType.LOCKED_SKILL);
						componentInChildren.SetUnlockStarCount(n_STAR);
					}
				}
				num++;
			}
			else
			{
				componentInChildren2.Setup(0, SkillButton.StatusType.DEFAULT);
				componentInChildren.Setup(SkillButtonDecorator.StyleType.NOT_AVAILABLE);
				num++;
			}
		}
	}

	private void RefreshRandomSkillIcons()
	{
		int num = 0;
		GameObject[] array = panel2SkillBtnPos;
		foreach (GameObject obj in array)
		{
			int key = num + 4;
			SkillButton componentInChildren = obj.GetComponentInChildren<SkillButton>();
			SkillButtonDecorator componentInChildren2 = obj.GetComponentInChildren<SkillButtonDecorator>();
			Button componentInChildren3 = componentInChildren.GetComponentInChildren<Button>();
			componentInChildren3.onClick.RemoveAllListeners();
			DNA_TABLE dnaTable = null;
			if (num < _randomDNATables.Length)
			{
				dnaTable = _randomDNATables[num];
				if (_characterInfo.netDNAInfoDic.ContainsKey(key))
				{
					int skillID2 = _characterInfo.netDNAInfoDic[key].SkillID;
					componentInChildren.Setup(skillID2, SkillButton.StatusType.DEFAULT);
					componentInChildren3.onClick.AddListener(delegate
					{
						OnClickRandomSkill(skillID2, dnaTable);
					});
					if (_characterInfo.netDNAInfoDic.Count == 8)
					{
						componentInChildren2.Setup(SkillButtonDecorator.StyleType.GOLD);
					}
					else
					{
						componentInChildren2.Setup(SkillButtonDecorator.StyleType.UNLOCKED);
					}
				}
				else
				{
					int skillID = 0;
					componentInChildren.Setup(skillID, SkillButton.StatusType.DEFAULT);
					componentInChildren3.onClick.AddListener(delegate
					{
						OnClickRandomSkill(skillID, dnaTable);
					});
					if (_characterInfo.netInfo.Star >= dnaTable.n_STAR)
					{
						componentInChildren2.Setup(SkillButtonDecorator.StyleType.DEFRAGABLE);
					}
					else
					{
						componentInChildren2.Setup(SkillButtonDecorator.StyleType.LOCKED_UNKNOW_SKILL);
						componentInChildren2.SetUnlockStarCount(dnaTable.n_STAR);
					}
				}
				num++;
			}
			else
			{
				componentInChildren.Setup(0, SkillButton.StatusType.DEFAULT);
				componentInChildren2.Setup(SkillButtonDecorator.StyleType.NOT_AVAILABLE);
				num++;
			}
		}
	}

	public void RefreshInheritSkillIcons()
	{
		int num = 0;
		int linkedCharacterID = 0;
		CharacterInfo value = null;
		bool flag = false;
		DNA_TABLE[] array = null;
		if (_characterInfo.netDNALinkInfo != null && _characterInfo.netDNALinkInfo.LinkedCharacterID != 0)
		{
			flag = true;
		}
		disconnectDNA.gameObject.SetActive(flag);
		linkedPortrait.gameObject.SetActive(flag);
		connectDNA.GetComponentInChildren<OrangeText>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_LINK_BTN");
		if (flag)
		{
			linkedCharacterID = _characterInfo.netDNALinkInfo.LinkedCharacterID;
			ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(linkedCharacterID, out value);
			if (value != null)
			{
				linkedPortrait.SetupCharacter(value.netInfo);
				linkedPortrait.EnableRedDot(false);
			}
			array = ManagedSingleton<OrangeDataManager>.Instance.DNA_TABLE_DICT.Values.Where((DNA_TABLE x) => x.n_CHARACTER == linkedCharacterID && x.n_TYPE == 1).ToArray();
			connectDNA.GetComponentInChildren<OrangeText>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_RELINK_BTN");
		}
		GameObject[] array2 = panel3SkillBtnPos;
		foreach (GameObject obj in array2)
		{
			SkillButtonDecorator componentInChildren = obj.GetComponentInChildren<SkillButtonDecorator>();
			SkillButton componentInChildren2 = obj.GetComponentInChildren<SkillButton>();
			componentInChildren2.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
			Button componentInChildren3 = componentInChildren2.GetComponentInChildren<Button>();
			componentInChildren3.onClick.RemoveAllListeners();
			if (value != null && num < _characterInfo.netDNALinkInfo.LinkedSlotID.Count)
			{
				int key = _characterInfo.netDNALinkInfo.LinkedSlotID[num];
				NetCharacterDNAInfo netDNAInfo = value.netDNAInfoDic[key];
				componentInChildren2.Setup(netDNAInfo.SkillID, SkillButton.StatusType.DEFAULT);
				if (_characterInfo.netDNAInfoDic.Count == 8)
				{
					componentInChildren.Setup(SkillButtonDecorator.StyleType.GOLD);
				}
				else
				{
					componentInChildren.Setup(SkillButtonDecorator.StyleType.UNLOCKED);
				}
				DNA_TABLE dnaTable = array[num];
				componentInChildren3.onClick.AddListener(delegate
				{
					OnClickLinkedSkill(netDNAInfo.SkillID, dnaTable);
				});
				num++;
			}
			else
			{
				int skillID = 0;
				componentInChildren2.Setup(skillID, SkillButton.StatusType.DEFAULT);
				componentInChildren.Setup(SkillButtonDecorator.StyleType.UNKNOW_ONLY);
				num++;
			}
		}
	}

	private void OnClickDefaultSkill(int skillID, DNA_TABLE dnaTable)
	{
		int materialTableID = dnaTable.n_COST_ID;
		bool bIsObtained = _characterInfo.netDNAInfoDic.ContainsKey(dnaTable.n_SLOT);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNAUnlock", delegate(DNAUnlock ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				if (_currentSkillCount != _characterInfo.netDNAInfoDic.Count && _characterInfo.netDNAInfoDic.Count == 8)
				{
					RefreshDefaultSkillIcons();
					RefreshRandomSkillIcons();
					UpdateDNAFX();
					CharacterInfoUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoUI>("UI_CharacterInfo_Main");
					if ((bool)uI)
					{
						uI.RefreshQuickSelectBar();
					}
				}
				else
				{
					RefreshDefaultSkillIcons();
				}
				_currentSkillCount = _characterInfo.netDNAInfoDic.Count;
			});
			if (_characterInfo.netInfo.Star >= dnaTable.n_STAR)
			{
				ui.Setup(_characterInfo.netInfo.CharacterID, dnaTable.n_ID, skillID, materialTableID, DNAUnlock.UNLOCK_TYPE.ANALYZE, 0, bIsObtained);
			}
			else
			{
				ui.Setup(_characterInfo.netInfo.CharacterID, dnaTable.n_ID, skillID, materialTableID, DNAUnlock.UNLOCK_TYPE.ANALYZE, dnaTable.n_STAR);
			}
		});
	}

	private void OnClickRandomSkill(int skillID, DNA_TABLE dnaTable)
	{
		int materialTableID = dnaTable.n_COST_ID;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNAUnlock", delegate(DNAUnlock ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				if (_currentSkillCount != _characterInfo.netDNAInfoDic.Count && _characterInfo.netDNAInfoDic.Count == 8)
				{
					RefreshDefaultSkillIcons();
					RefreshRandomSkillIcons();
					UpdateDNAFX();
					CharacterInfoUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoUI>("UI_CharacterInfo_Main");
					if ((bool)uI)
					{
						uI.RefreshQuickSelectBar();
					}
				}
				else
				{
					RefreshRandomSkillIcons();
				}
				_currentSkillCount = _characterInfo.netDNAInfoDic.Count;
			});
			if (_characterInfo.netInfo.Star >= dnaTable.n_STAR)
			{
				ui.Setup(_characterInfo.netInfo.CharacterID, dnaTable.n_ID, skillID, materialTableID, DNAUnlock.UNLOCK_TYPE.REBUILD, 0);
			}
			else
			{
				ui.Setup(_characterInfo.netInfo.CharacterID, dnaTable.n_ID, skillID, materialTableID, DNAUnlock.UNLOCK_TYPE.REBUILD, dnaTable.n_STAR);
			}
		});
	}

	private void OnClickLinkedSkill(int skillID, DNA_TABLE dnaTable)
	{
		int materialTableID = dnaTable.n_COST_ID;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNAQuickSkillInfo", delegate(DNAUnlock ui)
		{
			ui.Setup(_characterInfo.netInfo.CharacterID, dnaTable.n_ID, skillID, materialTableID, DNAUnlock.UNLOCK_TYPE.REBUILD, 0);
			ui.SetQuickSkillInfoMode();
		});
	}

	public void OnClickLinkDNA()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNALink", delegate(DNALink ui)
		{
			ui.Setup(_characterInfo);
		});
	}

	public void OnClickBuildList()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNABuildList", delegate(DNABuildList ui)
		{
			ui.Setup(_characterInfo);
		});
	}

	public void OnClickDisconnectDNA()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
			ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.SetupYesNoByKey("COMMON_TIP", "DNA_UNLINK_TIP", "COMMON_OK", "COMMON_CANCEL", delegate
			{
				List<sbyte> useSPItemSlotId = new List<sbyte>();
				ManagedSingleton<PlayerNetManager>.Instance.CharacterDNALinkReq(_characterInfo.netInfo.CharacterID, 0, useSPItemSlotId, delegate
				{
					RefreshInheritSkillIcons();
				});
			});
		});
	}

	public void OnClickRules()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RULE_DNA"));
		});
	}
}
