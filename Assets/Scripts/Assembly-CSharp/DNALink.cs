#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class DNALink : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect scrollRectSkills;

	[SerializeField]
	private LoopVerticalScrollRect scrollRectCharacter;

	[SerializeField]
	private DNALinkCharacterUnit characterUnitRef;

	[SerializeField]
	private DNALinkSkillUnit skillUnitRef;

	[SerializeField]
	private OrangeText totalSPCount;

	[SerializeField]
	private OrangeText totalDNACount;

	[SerializeField]
	private Button linkButton;

	[SerializeField]
	private GameObject linkerInfoPanel;

	[SerializeField]
	private CommonIconBase linkerPortrait;

	[SerializeField]
	private OrangeText linkerDescription;

	private List<CharacterInfo> _characterInfoList = new List<CharacterInfo>();

	private List<NetCharacterDNAInfo> _netCharacterDNAInfoList = new List<NetCharacterDNAInfo>();

	private Dictionary<int, int> _linkedByCharacterDict = new Dictionary<int, int>();

	private CharacterInfo _characterInfo;

	private CharacterInfo _targetCharacterInfo;

	private DNALinkCharacterUnit _selectedCharacterUnit;

	private int _characterVisualCount = 24;

	private int _skillVisualCount = 4;

	private int _linkerPanelDelayTween;

	private int _linkerPanelAlphaTween;

	private List<sbyte> _SPItemSlotId = new List<sbyte>();

	private bool _bSufficientSP;

	private bool _bSufficientDNA;

	private bool _bIsCurrentLinkedTarget;

	private bool _bControlLock;

	private int _controlLockTweenID;

	public bool bMuteOk01;

	public void PlaySEOK01()
	{
		if (!bMuteOk01)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
	}

	public void Setup(CharacterInfo characterInfo)
	{
		_characterInfo = characterInfo;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		bMuteOk01 = true;
		UpdateNetCharacterInfoCache();
		UpdateCharacterScrollRect(true);
		UpdateItemCount();
		bMuteOk01 = false;
		linkerInfoPanel.SetActive(false);
		if (_targetCharacterInfo == null)
		{
			linkButton.interactable = false;
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref _linkerPanelDelayTween, false);
		LeanTween.cancel(ref _linkerPanelAlphaTween, false);
	}

	private void UpdateItemCount()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		string format = "<color=#FF0000>{0}</color>/{1}";
		string format2 = "{0}/{1}";
		ItemInfo value;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(OrangeConst.ITEMID_DNA_POINT, out value))
		{
			num3 = value.netItemInfo.Stack;
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(OrangeConst.ITEMID_DNA_SP_ITEM, out value))
		{
			num4 = value.netItemInfo.Stack;
		}
		if (_targetCharacterInfo != null)
		{
			num = OrangeConst.DNA_IMPORT_COST;
			num2 = _SPItemSlotId.Count;
		}
		_bSufficientSP = num4 >= num2;
		_bSufficientDNA = num3 >= num;
		if (num3 < num)
		{
			totalDNACount.text = string.Format(format, num3, num);
		}
		else
		{
			totalDNACount.text = string.Format(format2, num3, num);
		}
		if (num4 < num2)
		{
			totalSPCount.text = string.Format(format, num4, num2);
		}
		else
		{
			totalSPCount.text = string.Format(format2, num4, num2);
		}
	}

	private void UpdateNetCharacterInfoCache()
	{
		_characterInfoList.Clear();
		_linkedByCharacterDict.Clear();
		foreach (KeyValuePair<int, CharacterInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicCharacter)
		{
			CharacterInfo value = item.Value;
			if (value.netDNAInfoDic.Count > 0 && (value.netDNAInfoDic.ContainsKey(4) || value.netDNAInfoDic.ContainsKey(5) || value.netDNAInfoDic.ContainsKey(6) || value.netDNAInfoDic.ContainsKey(7) || value.netDNAInfoDic.ContainsKey(8)) && value.netInfo.CharacterID != _characterInfo.netInfo.CharacterID)
			{
				_characterInfoList.Add(value);
			}
			if (value.netDNALinkInfo != null && value.netDNALinkInfo.LinkedCharacterID != 0)
			{
				_linkedByCharacterDict.ContainsAdd(value.netDNALinkInfo.LinkedCharacterID, value.netDNALinkInfo.CharacterID);
			}
		}
	}

	public int GetCharacterLinkerID(int characterID)
	{
		int value = 0;
		_linkedByCharacterDict.TryGetValue(characterID, out value);
		return value;
	}

	private void UpdateSkillCache(CharacterInfo characterInfo)
	{
		_netCharacterDNAInfoList.Clear();
		if (characterInfo.netDNAInfoDic != null)
		{
			NetCharacterDNAInfo value;
			if (characterInfo.netDNAInfoDic.TryGetValue(4, out value))
			{
				_netCharacterDNAInfoList.Add(value);
			}
			if (characterInfo.netDNAInfoDic.TryGetValue(5, out value))
			{
				_netCharacterDNAInfoList.Add(value);
			}
			if (characterInfo.netDNAInfoDic.TryGetValue(6, out value))
			{
				_netCharacterDNAInfoList.Add(value);
			}
			if (characterInfo.netDNAInfoDic.TryGetValue(7, out value))
			{
				_netCharacterDNAInfoList.Add(value);
			}
			if (characterInfo.netDNAInfoDic.TryGetValue(8, out value))
			{
				_netCharacterDNAInfoList.Add(value);
			}
		}
	}

	public CharacterInfo GetCharacterInfo(int index)
	{
		if (_characterInfoList.Count > index)
		{
			return _characterInfoList[index];
		}
		return null;
	}

	public NetCharacterDNAInfo GetSkillInfo(int index)
	{
		if (_netCharacterDNAInfoList.Count > index)
		{
			return _netCharacterDNAInfoList[index];
		}
		return null;
	}

	public void UpdateCharacterScrollRect(bool bRebuild = false)
	{
		if (bRebuild)
		{
			scrollRectCharacter.ClearCells();
			scrollRectCharacter.OrangeInit(characterUnitRef, _characterVisualCount, _characterInfoList.Count);
		}
		else
		{
			scrollRectCharacter.RefreshCells();
		}
	}

	public void SetCurrentCharacterSelection(DNALinkCharacterUnit characterUnit = null)
	{
		_selectedCharacterUnit = characterUnit;
		if (_selectedCharacterUnit == null)
		{
			scrollRectSkills.ClearCells();
			_targetCharacterInfo = null;
		}
		if (_targetCharacterInfo != null)
		{
			if (_characterInfo.netDNALinkInfo != null && _characterInfo.netDNALinkInfo.LinkedCharacterID == _targetCharacterInfo.netInfo.CharacterID)
			{
				linkButton.interactable = true;
				linkButton.GetComponentInChildren<OrangeText>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_RELINK_BTN");
			}
			else
			{
				bool flag = _linkedByCharacterDict.ContainsKey(_targetCharacterInfo.netInfo.CharacterID);
				linkButton.interactable = !flag;
				linkButton.GetComponentInChildren<OrangeText>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_LINK_BTN");
			}
		}
		else
		{
			linkButton.interactable = false;
			linkButton.GetComponentInChildren<OrangeText>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_LINK_BTN");
		}
		UpdateItemCount();
	}

	public DNALinkCharacterUnit GetCurrentCharacterSelection()
	{
		return _selectedCharacterUnit;
	}

	public void OnClickCharacterUnit(int index)
	{
		if (_bControlLock)
		{
			return;
		}
		scrollRectSkills.ClearCells();
		if (_characterInfoList.Count > index)
		{
			_targetCharacterInfo = _characterInfoList[index];
			UpdateSkillCache(_targetCharacterInfo);
			scrollRectSkills.OrangeInit(skillUnitRef, _skillVisualCount, _netCharacterDNAInfoList.Count);
			if (_characterInfo.netDNALinkInfo != null && _characterInfo.netDNALinkInfo.LinkedCharacterID == _targetCharacterInfo.netInfo.CharacterID)
			{
				_bIsCurrentLinkedTarget = true;
			}
			else
			{
				_bIsCurrentLinkedTarget = false;
			}
		}
		_SPItemSlotId.Clear();
		UpdateItemCount();
	}

	public void DisplayLinkerPanel()
	{
		CharacterInfo characterInfo = GetCharacterInfo(_selectedCharacterUnit.GetIndex());
		linkerInfoPanel.SetActive(false);
		int value = 0;
		if (!_linkedByCharacterDict.TryGetValue(characterInfo.netInfo.CharacterID, out value))
		{
			return;
		}
		CharacterInfo value2 = null;
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(value, out value2))
		{
			return;
		}
		LeanTween.cancel(ref _linkerPanelDelayTween, true);
		LeanTween.cancel(ref _linkerPanelAlphaTween, true);
		linkerInfoPanel.SetActive(true);
		linkerInfoPanel.transform.position = _selectedCharacterUnit.transform.position;
		AlignToRightEdge(ref linkerInfoPanel);
		linkerPortrait.SetupCharacter(value2.netInfo);
		linkerPortrait.EnableRedDot(false);
		float delayTime = 1f;
		CanvasGroup alphaCanvasGroup = linkerInfoPanel.GetComponent<CanvasGroup>();
		_linkerPanelDelayTween = LeanTween.delayedCall(delayTime, (Action)delegate
		{
			_linkerPanelAlphaTween = LeanTween.value(1f, 0f, 1f).setOnUpdate(delegate(float alpha)
			{
				alphaCanvasGroup.alpha = alpha;
			}).setOnComplete((Action)delegate
			{
				alphaCanvasGroup.alpha = 1f;
				linkerInfoPanel.SetActive(false);
			})
				.uniqueId;
		}).uniqueId;
	}

	private void AlignToRightEdge(ref GameObject target)
	{
		float num = 580f;
		if (target.transform.localPosition.x > num)
		{
			target.transform.localPosition = new Vector3(num, target.transform.localPosition.y, target.transform.localPosition.z);
		}
	}

	public void OnClickSkillUseSPItem(int slotID)
	{
		sbyte item = (sbyte)slotID;
		if (_SPItemSlotId.Contains(item))
		{
			_SPItemSlotId.Remove(item);
		}
		else
		{
			_SPItemSlotId.Add(item);
		}
		UpdateItemCount();
	}

	public void OnClickDNALink()
	{
		if (_targetCharacterInfo == null)
		{
			Debug.Log("No character selected.");
		}
		else
		{
			if (_bControlLock)
			{
				return;
			}
			SetControlLock(5f);
			int targetCharacterID = _targetCharacterInfo.netInfo.CharacterID;
			if (!_bSufficientSP)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.SetupYesNoByKey("COMMON_TIP", "DNA_SP_ITEM_NOT_ENOUGH", "GO_TO_SHOP", "COMMON_CLOSE", delegate
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI shopUI)
						{
							shopUI.closeCB = (Callback)Delegate.Combine(shopUI.closeCB, (Callback)delegate
							{
								UpdateItemCount();
							});
							shopUI.Setup(ShopTopUI.ShopSelectTab.item_shop);
						});
					}, delegate
					{
					});
				});
				SetControlLock();
			}
			else if (!_bSufficientDNA)
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
								UpdateItemCount();
							});
							shopUI.Setup(ShopTopUI.ShopSelectTab.directproduct);
						});
					}, delegate
					{
						OnClickConvertDNA();
					}, delegate
					{
					});
				});
				SetControlLock();
			}
			else if (_characterInfo.netDNALinkInfo != null && _characterInfo.netDNALinkInfo.LinkedCharacterID != 0)
			{
				if (_bIsCurrentLinkedTarget)
				{
					ManagedSingleton<PlayerNetManager>.Instance.CharacterDNALinkReq(_characterInfo.netInfo.CharacterID, 0, _SPItemSlotId, delegate
					{
						DNALinkHelper(targetCharacterID);
					});
					return;
				}
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.SetupYesNoByKey("COMMON_TIP", "DNA_LINK_TIP_CHANGE_TARGET", "COMMON_OK", "COMMON_CANCEL", delegate
					{
						ManagedSingleton<PlayerNetManager>.Instance.CharacterDNALinkReq(_characterInfo.netInfo.CharacterID, 0, _SPItemSlotId, delegate
						{
							DNALinkHelper(targetCharacterID);
						});
					}, delegate
					{
						SetControlLock();
					});
				});
			}
			else
			{
				DNALinkHelper(targetCharacterID);
			}
		}
	}

	public void OnClickConvertDNA()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNAConvert", delegate(DNAConvert ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				UpdateItemCount();
			});
			ui.Setup();
		});
	}

	private void DNALinkHelper(int targetCharacterID)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		ManagedSingleton<PlayerNetManager>.Instance.CharacterDNALinkReq(_characterInfo.netInfo.CharacterID, targetCharacterID, _SPItemSlotId, delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNALinkResult", delegate(DNALinkResult ui)
			{
				ui.Setup(_characterInfo);
				CharacterInfoDNA uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoDNA>("UI_CharacterInfo_DNA");
				if (uI != null)
				{
					uI.RefreshInheritSkillIcons();
				}
				SetControlLock();
				base.CloseSE = SystemSE.NONE;
				OnClickCloseBtn();
				base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			});
		});
	}

	public CharacterInfo GetLinkerCharacterInfo()
	{
		return _characterInfo;
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

	public bool IsControlLocked()
	{
		return _bControlLock;
	}
}
