using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class DNAUnlock : OrangeUIBase
{
	public enum UNLOCK_TYPE
	{
		ANALYZE = 0,
		REBUILD = 1,
		MAX = 2
	}

	[SerializeField]
	private OrangeText dialogTitle;

	[SerializeField]
	private GameObject skillBtnDecorator;

	[SerializeField]
	private GameObject skillIconPos;

	[SerializeField]
	private OrangeText skillDescription;

	[SerializeField]
	private Text skillName;

	[SerializeField]
	private CommonIconBase materialIconRef;

	[SerializeField]
	private GameObject[] materialIconPos;

	[SerializeField]
	private GameObject materialIconGroup;

	[SerializeField]
	private Button analyzeBtn;

	[SerializeField]
	private Button rebuildBtn;

	[SerializeField]
	private Button convertBtn;

	[SerializeField]
	private GameObject unlockFX;

	[SerializeField]
	private WrapRectComponent descriptionRoot;

	private int nCharacterID;

	private int _DNAID;

	private int _skillID;

	private int _materialTableID;

	private int _starLock;

	private bool _bIsObtained;

	private UNLOCK_TYPE _unlockType;

	private CommonIconBase[] _materialIcons = new CommonIconBase[5];

	private SkillButton _skillButton;

	private SkillButtonDecorator _skillButtonDecorator;

	private MATERIAL_TABLE _materialTable;

	private bool _bControlLock;

	private int _unlockFXTweenID;

	private int _controlLockTweenID;

	private CharacterInfoUI characterInfoUI;

	private Dictionary<int, int> dicMaterialNeeded = new Dictionary<int, int>();

	public void Setup(int CharacterID, int DNAID, int skillID, int materialTableID, UNLOCK_TYPE unlockType, int starLockDisplay, bool bIsObtained = false)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		nCharacterID = CharacterID;
		_DNAID = DNAID;
		_skillID = skillID;
		_materialTableID = materialTableID;
		_unlockType = unlockType;
		_starLock = starLockDisplay;
		_bIsObtained = bIsObtained;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (_unlockType == UNLOCK_TYPE.ANALYZE)
		{
			dialogTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_RESEARCH");
			analyzeBtn.gameObject.SetActive(true);
			rebuildBtn.gameObject.SetActive(false);
		}
		else if (_unlockType == UNLOCK_TYPE.REBUILD)
		{
			dialogTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_RECOMPOSE");
			analyzeBtn.gameObject.SetActive(false);
			rebuildBtn.gameObject.SetActive(true);
		}
		characterInfoUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoUI>("UI_CharacterInfo_Main");
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("ui/skillbutton", "SkillButton", delegate(UnityEngine.Object asset)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(asset as GameObject, skillIconPos.transform, false);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(skillBtnDecorator, skillIconPos.transform, false);
			_skillButton = gameObject.GetComponent<SkillButton>();
			_skillButtonDecorator = gameObject2.GetComponent<SkillButtonDecorator>();
			gameObject2.SetActive(true);
			if (_skillID == 0)
			{
				SetStarLock();
				_skillButton.Setup(_skillID, SkillButton.StatusType.DEFAULT);
				skillDescription.text = "";
				skillName.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_UNKNOW_NAME");
			}
			else
			{
				SetStarLock();
				_skillButton.Setup(_skillID, SkillButton.StatusType.DEFAULT);
				skillDescription.text = string.Format(_skillButton.GetSkillDescription(), _skillButton.GetSkillEffect().ToString("0.00"));
				if ((bool)descriptionRoot)
				{
					descriptionRoot.gameObject.SetActive(false);
					descriptionRoot.gameObject.SetActive(true);
				}
				skillName.text = _skillButton.GetSkillName();
			}
		});
		if (ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(_materialTableID, out _materialTable))
		{
			if (_materialTable.n_MATERIAL_1 != 0)
			{
				_materialIcons[0] = CreateMaterialIcon(0, materialIconPos[0].transform);
				dicMaterialNeeded.ContainsAdd(_materialTable.n_MATERIAL_1, _materialTable.n_MATERIAL_MOUNT1);
			}
			if (_materialTable.n_MATERIAL_2 != 0)
			{
				_materialIcons[1] = CreateMaterialIcon(1, materialIconPos[1].transform);
				dicMaterialNeeded.ContainsAdd(_materialTable.n_MATERIAL_2, _materialTable.n_MATERIAL_MOUNT2);
			}
			if (_materialTable.n_MATERIAL_3 != 0)
			{
				_materialIcons[2] = CreateMaterialIcon(2, materialIconPos[2].transform);
				dicMaterialNeeded.ContainsAdd(_materialTable.n_MATERIAL_3, _materialTable.n_MATERIAL_MOUNT3);
			}
			if (_materialTable.n_MATERIAL_4 != 0)
			{
				_materialIcons[3] = CreateMaterialIcon(3, materialIconPos[3].transform);
				dicMaterialNeeded.ContainsAdd(_materialTable.n_MATERIAL_4, _materialTable.n_MATERIAL_MOUNT4);
			}
			if (_materialTable.n_MATERIAL_5 != 0)
			{
				_materialIcons[4] = CreateMaterialIcon(4, materialIconPos[4].transform);
				dicMaterialNeeded.ContainsAdd(_materialTable.n_MATERIAL_5, _materialTable.n_MATERIAL_MOUNT5);
			}
		}
		UpdateButtonStatus();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref _unlockFXTweenID);
		LeanTween.cancel(ref _controlLockTweenID);
	}

	private void SetStarLock()
	{
		if (_skillID == 0)
		{
			if (_starLock != 0)
			{
				_skillButtonDecorator.Setup(SkillButtonDecorator.StyleType.LOCKED_UNKNOW_SKILL);
				_skillButtonDecorator.SetUnlockStarCount(_starLock);
			}
			else
			{
				_skillButtonDecorator.Setup(SkillButtonDecorator.StyleType.UNKNOW_ONLY);
			}
		}
		else if (_starLock != 0)
		{
			_skillButtonDecorator.Setup(SkillButtonDecorator.StyleType.LOCKED_SKILL);
			_skillButtonDecorator.SetUnlockStarCount(_starLock);
		}
		else
		{
			_skillButtonDecorator.Setup(SkillButtonDecorator.StyleType.UNLOCKED);
		}
	}

	private void UpdateButtonStatus()
	{
		bool interactable = _starLock == 0;
		analyzeBtn.interactable = interactable;
		rebuildBtn.interactable = interactable;
		if (_unlockType == UNLOCK_TYPE.ANALYZE && _bIsObtained)
		{
			materialIconGroup.SetActive(false);
			analyzeBtn.interactable = false;
		}
		StartCoroutine(ManagedSingleton<CharacterHelper>.Instance.CheckCharacterUpgradesCBByID(nCharacterID, delegate
		{
			characterInfoUI.RefreshSideButtonRedDots();
			characterInfoUI.RefreshQuickSelectBar();
		}));
	}

	private CommonIconBase CreateMaterialIcon(int index, Transform pos)
	{
		CommonIconBase commonIconBase = UnityEngine.Object.Instantiate(materialIconRef, pos);
		commonIconBase.SetupMaterial(_materialTableID, index, OnClickItemIcon);
		return commonIconBase;
	}

	private void OnClickItemIcon(int p_idx)
	{
		ITEM_TABLE itemTable;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(p_idx, out itemTable))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
			{
				int value;
				dicMaterialNeeded.TryGetValue(p_idx, out value);
				ui.Setup(itemTable, value);
			});
		}
	}

	private bool IsMaterialSufficient()
	{
		int firstNotEnoughItemID = 0;
		if (!ManagedSingleton<PlayerHelper>.Instance.CheckMaterialEnough(_materialTableID, out firstNotEnoughItemID))
		{
			if (firstNotEnoughItemID == OrangeConst.ITEMID_DNA_POINT)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNAPurchase", delegate(DNAPurchase ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.MuteSE = true;
					ui.Setup(delegate
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI shopUI)
						{
							shopUI.closeCB = (Callback)Delegate.Combine(shopUI.closeCB, (Callback)delegate
							{
								UpdateButtonStatus();
								UpdateMaterialIcons();
							});
							shopUI.Setup(ShopTopUI.ShopSelectTab.directproduct);
						});
					}, delegate
					{
						OnClickConvert();
					}, delegate
					{
					});
				});
			}
			else
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.SetupConfirmByKey("COMMON_TIP", "MATERIAL_NOT_ENOUGH", "COMMON_CLOSE", delegate
					{
					});
				});
			}
			return false;
		}
		return true;
	}

	private void UpdateMaterialIcons()
	{
		if (_materialTable.n_MATERIAL_1 != 0)
		{
			_materialIcons[0].SetupMaterial(_materialTableID, 0, OnClickItemIcon);
		}
		if (_materialTable.n_MATERIAL_2 != 0)
		{
			_materialIcons[1].SetupMaterial(_materialTableID, 1, OnClickItemIcon);
		}
		if (_materialTable.n_MATERIAL_3 != 0)
		{
			_materialIcons[2].SetupMaterial(_materialTableID, 2, OnClickItemIcon);
		}
		if (_materialTable.n_MATERIAL_4 != 0)
		{
			_materialIcons[3].SetupMaterial(_materialTableID, 3, OnClickItemIcon);
		}
		if (_materialTable.n_MATERIAL_5 != 0)
		{
			_materialIcons[4].SetupMaterial(_materialTableID, 4, OnClickItemIcon);
		}
	}

	public void OnClickAnalyze()
	{
		if (!_bControlLock && IsMaterialSufficient())
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
			SetControlLock(5f);
			analyzeBtn.interactable = false;
			ManagedSingleton<PlayerNetManager>.Instance.CharacterDNAAnalyzeReq(_DNAID, delegate
			{
				_skillButtonDecorator.Setup(SkillButtonDecorator.StyleType.UNLOCKED);
				UpdateMaterialIcons();
				SetControlLock();
				analyzeBtn.interactable = true;
				PlayUnlockEffect(_skillButton.transform.position);
				_bIsObtained = true;
				UpdateButtonStatus();
			});
		}
	}

	public void OnClickRebuild()
	{
		if (_bControlLock || !IsMaterialSufficient())
		{
			return;
		}
		SetControlLock(5f);
		rebuildBtn.interactable = false;
		ManagedSingleton<PlayerNetManager>.Instance.CharacterDNAAnalyzeReq(_DNAID, delegate(NetCharacterDNAInfo DNAInfo)
		{
			if (DNAInfo.PulledSkillID != 0)
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNAExchange", delegate(DNAExchange ui)
				{
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					DNAExchange dNAExchange = ui;
					dNAExchange.closeCB = (Callback)Delegate.Combine(dNAExchange.closeCB, (Callback)delegate
					{
						if (ui.refNCDNAI != null)
						{
							PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
							UpdateRebuild(ui.refNCDNAI);
						}
						else
						{
							UpdateMaterialIcons();
							SetControlLock();
							rebuildBtn.interactable = true;
						}
					});
					ui.Setup(DNAInfo);
				});
			}
			else
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
				UpdateRebuild(DNAInfo);
			}
		});
	}

	private void UpdateRebuild(NetCharacterDNAInfo DNAInfo)
	{
		_skillButton.Setup(DNAInfo.SkillID, SkillButton.StatusType.DEFAULT);
		_skillButtonDecorator.Setup(SkillButtonDecorator.StyleType.UNLOCKED);
		skillDescription.text = string.Format(_skillButton.GetSkillDescription(), _skillButton.GetSkillEffect().ToString("0.00"));
		skillDescription.gameObject.SetActive(false);
		skillDescription.gameObject.SetActive(true);
		skillName.text = _skillButton.GetSkillName();
		UpdateMaterialIcons();
		SetControlLock();
		rebuildBtn.interactable = true;
		PlayUnlockEffect(_skillButton.transform.position);
		_bIsObtained = true;
		UpdateButtonStatus();
	}

	public override void OnClickCloseBtn()
	{
		if (!_bControlLock)
		{
			base.OnClickCloseBtn();
		}
	}

	public void OnClickConvert()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNAConvert", delegate(DNAConvert ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				UpdateButtonStatus();
				UpdateMaterialIcons();
			});
			ui.Setup();
		});
	}

	public void PlayUnlockEffect(Vector3 position)
	{
		unlockFX.SetActive(true);
		unlockFX.transform.position = position;
		unlockFX.GetComponent<Animator>().Play("UI_lockFX", 0, 0f);
		float length = unlockFX.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
		StartCoroutine(PlayUnlockSE());
		LeanTween.cancel(ref _unlockFXTweenID);
		_unlockFXTweenID = LeanTween.delayedCall(length, (Action)delegate
		{
			unlockFX.SetActive(false);
		}).uniqueId;
	}

	private IEnumerator PlayUnlockSE()
	{
		yield return new WaitForSeconds(0.2f);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
	}

	private void SetControlLock(float unlockTime = 0f)
	{
		LeanTween.cancel(ref _controlLockTweenID);
		if (unlockTime <= 0f)
		{
			_bControlLock = false;
			return;
		}
		_bControlLock = true;
		_controlLockTweenID = LeanTween.delayedCall(unlockTime, (Action)delegate
		{
			_bControlLock = false;
		}).uniqueId;
	}

	public void SetQuickSkillInfoMode()
	{
		dialogTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_INHERIT");
		analyzeBtn.gameObject.SetActive(false);
		rebuildBtn.gameObject.SetActive(false);
	}
}
