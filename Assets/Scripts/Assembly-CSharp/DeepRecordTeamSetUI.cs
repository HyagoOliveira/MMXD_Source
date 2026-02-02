using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;

public class DeepRecordTeamSetUI : OrangeUIBase
{
	public enum Status
	{
		Unknow = 0,
		View = 1,
		Edit = 2
	}

	private const int SET_MIN_NUM = 1;

	private readonly string emptyRecordStr = "000000";

	[SerializeField]
	private Transform stParent;

	[SerializeField]
	private Transform characterCellParent;

	[SerializeField]
	private Transform weaponCellParent;

	[SerializeField]
	private Text textRemainTime;

	[SerializeField]
	private Canvas canvasRemainTime;

	[SerializeField]
	private Text textRecordTotalVal;

	[SerializeField]
	private Text textRecordBattleVal;

	[SerializeField]
	private Text textRecordExploreVal;

	[SerializeField]
	private Text textRecordActionVal;

	[SerializeField]
	private Text textProgressVal;

	[SerializeField]
	private ItemBoxTab tabCharacter;

	[SerializeField]
	private ItemBoxTab tabWeapon;

	[SerializeField]
	private Button btnStart;

	[SerializeField]
	private DeepRecordTeamSetUIUnit cellPrefab;

	[SerializeField]
	private Toggle[] togglesOnOff;

	private DeepRecordTeamSetUIUnit[] unitCharacter = new DeepRecordTeamSetUIUnit[0];

	private DeepRecordTeamSetUIUnit[] unitWeapon = new DeepRecordTeamSetUIUnit[0];

	private Status status;

	private int recordBattleVal;

	private int recordExploreVal;

	private int recordActionVal;

	private int isLoading;

	private bool toggleInitiated;

	public List<CharacterInfo> ListSetCharacter { get; private set; } = new List<CharacterInfo>();


	public List<WeaponInfo> ListSetWeapon { get; private set; } = new List<WeaponInfo>();


	private int SET_LIMIT
	{
		get
		{
			return 10;
		}
	}

	public int RecordTotalVal
	{
		get
		{
			return recordBattleVal + recordExploreVal + recordActionVal;
		}
	}

	protected override bool IsEscapeVisible()
	{
		if (isLoading > 0)
		{
			return false;
		}
		return base.IsEscapeVisible();
	}

	public void Setup()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		status = ManagedSingleton<DeepRecordHelper>.Instance.Status;
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Save));
		InitCell();
		bool lastRecordMoveChk = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRecordMoveChk;
		togglesOnOff[0].isOn = lastRecordMoveChk;
		togglesOnOff[1].isOn = !lastRecordMoveChk;
		togglesOnOff[0].onValueChanged.Invoke(lastRecordMoveChk);
		togglesOnOff[1].onValueChanged.Invoke(!lastRecordMoveChk);
		toggleInitiated = true;
		switch (status)
		{
		case Status.Edit:
			ListSetWeapon.Clear();
			ListSetCharacter.Clear();
			UpdateRecordVal();
			UpdateResetTime(false);
			UpdateBtnStartStatus();
			textProgressVal.text = "---";
			break;
		case Status.View:
			foreach (int character in ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.CharacterList)
			{
				CharacterInfo value;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(character, out value))
				{
					ListSetCharacter.Add(value);
				}
			}
			foreach (int weapon in ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.WeaponList)
			{
				WeaponInfo value2;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(weapon, out value2))
				{
					ListSetWeapon.Add(value2);
				}
			}
			SetCharacterUnits();
			SetWeaponUnits();
			UpdateResetTime(true);
			UpdateBtnStartStatus();
			textProgressVal.text = string.Format("{0}/{1}", ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.FinishPositionList.Count, ManagedSingleton<DeepRecordHelper>.Instance.MapInfo.GridPositionList.Count);
			break;
		}
	}

	private void InitCell()
	{
		if (unitWeapon.Length == 0)
		{
			unitCharacter = new DeepRecordTeamSetUIUnit[SET_LIMIT];
			unitWeapon = new DeepRecordTeamSetUIUnit[SET_LIMIT];
			for (int i = 0; i < SET_LIMIT; i++)
			{
				DeepRecordTeamSetUIUnit deepRecordTeamSetUIUnit = UnityEngine.Object.Instantiate(cellPrefab, characterCellParent);
				DeepRecordTeamSetUIUnit deepRecordTeamSetUIUnit2 = UnityEngine.Object.Instantiate(cellPrefab, weaponCellParent);
				deepRecordTeamSetUIUnit.Init(DeepRecordUseSetUI.SetType.Character);
				deepRecordTeamSetUIUnit2.Init(DeepRecordUseSetUI.SetType.Weapon);
				unitCharacter[i] = deepRecordTeamSetUIUnit;
				unitWeapon[i] = deepRecordTeamSetUIUnit2;
			}
		}
	}

	private void UpdateBtnStartStatus()
	{
		Status status = this.status;
		if (status == Status.Edit)
		{
			btnStart.interactable = ListSetCharacter.Count >= 1 && ListSetWeapon.Count >= 1;
		}
		else
		{
			btnStart.interactable = false;
		}
	}

	private void UpdateRecordVal()
	{
		int count = ListSetCharacter.Count;
		int count2 = ListSetWeapon.Count;
		recordBattleVal = 0;
		recordExploreVal = 0;
		recordActionVal = 0;
		if (count == 0)
		{
			tabCharacter.SetTextStr(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_NODEPLOY_CHARACTER"));
			tabCharacter.UpdateState(true);
		}
		else
		{
			tabCharacter.SetTextStr(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_DEPLOY_CHARACTER"));
			tabCharacter.UpdateState(false);
		}
		if (count2 == 0)
		{
			tabWeapon.SetTextStr(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_NODEPLOY_WEAPON"));
			tabWeapon.UpdateState(true);
		}
		else
		{
			tabWeapon.SetTextStr(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_DEPLOY_WEAPON"));
			tabWeapon.UpdateState(false);
		}
		if (count + count2 == 0)
		{
			textRecordTotalVal.text = emptyRecordStr;
			textRecordBattleVal.text = emptyRecordStr;
			textRecordExploreVal.text = emptyRecordStr;
			textRecordActionVal.text = emptyRecordStr;
			return;
		}
		foreach (CharacterInfo item in ListSetCharacter)
		{
			recordBattleVal += DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.BATTLE, item);
			recordExploreVal += DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.EXPLORE, item);
			recordActionVal += DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.ACTION, item);
		}
		foreach (WeaponInfo item2 in ListSetWeapon)
		{
			recordBattleVal += DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.BATTLE, item2);
			recordExploreVal += DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.EXPLORE, item2);
			recordActionVal += DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.ACTION, item2);
		}
		textRecordTotalVal.text = RecordTotalVal.ToString();
		textRecordBattleVal.text = recordBattleVal.ToString();
		textRecordExploreVal.text = recordExploreVal.ToString();
		textRecordActionVal.text = recordActionVal.ToString();
	}

	private void SetCharacterUnits()
	{
		int i;
		for (i = 0; i < ListSetCharacter.Count; i++)
		{
			unitCharacter[i].SetCharacterIcon(ListSetCharacter[i]);
		}
		for (; i < SET_LIMIT; i++)
		{
			unitCharacter[i].Clear();
		}
		UpdateRecordVal();
		LoadStandCharacter();
		UpdateBtnStartStatus();
	}

	private void SetWeaponUnits()
	{
		int i;
		for (i = 0; i < ListSetWeapon.Count; i++)
		{
			unitWeapon[i].SetWeaponIcon(ListSetWeapon[i]);
		}
		for (; i < SET_LIMIT; i++)
		{
			unitWeapon[i].Clear();
		}
		UpdateRecordVal();
		UpdateBtnStartStatus();
	}

	private void LoadStandCharacter()
	{
		if (ListSetCharacter.Count <= 0)
		{
			return;
		}
		CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(ListSetCharacter[0].netInfo.CharacterID);
		if (characterTable == null)
		{
			return;
		}
		isLoading++;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, characterTable.s_ICON), characterTable.s_ICON + "_db", delegate(GameObject obj)
		{
			for (int num = stParent.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(stParent.GetChild(num).gameObject);
			}
			if (obj != null)
			{
				UnityEngine.Object.Instantiate(obj, stParent, false);
			}
			isLoading--;
		});
	}

	private void UpdateResetTime(bool enable)
	{
		canvasRemainTime.enabled = enable;
		if (enable)
		{
			int num = Convert.ToInt32(ManagedSingleton<DeepRecordHelper>.Instance.MapInfo.ResetTime - MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC);
			textRemainTime.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_TIME"), OrangeGameUtility.GetTimeText(num));
		}
	}

	public void RefreashCharacter(List<CharacterInfo> p_listCharacter)
	{
		ListSetCharacter.Clear();
		ListSetCharacter.AddRange(p_listCharacter);
		SetCharacterUnits();
	}

	public void RefreashWeaponList(List<WeaponInfo> p_listSetWeapon)
	{
		ListSetWeapon.Clear();
		ListSetWeapon.AddRange(p_listSetWeapon);
		SetWeaponUnits();
	}

	public void OnClickOpenUseSetUI_Character_Btn()
	{
		if (status == Status.Edit)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordUseSet", delegate(DeepRecordUseSetUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupByList(ListSetCharacter);
			});
		}
	}

	public void OnClickOpenUseSetUI_Weapon_Btn()
	{
		if (status == Status.Edit)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordUseSet", delegate(DeepRecordUseSetUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupByList(ListSetWeapon);
			});
		}
	}

	public void OnClickOpenRuleBtn()
	{
		ManagedSingleton<DeepRecordHelper>.Instance.OpenRuleUI();
	}

	public void OnClickBtnStart()
	{
		if (OrangeConst.RECORD_CHARACTER_MAX != ListSetCharacter.Count && OrangeConst.RECORD_CHARACTER_MAX != ListSetWeapon.Count)
		{
			ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_WARN3"), delegate
			{
				OnStartConfirm();
			});
		}
		else if (OrangeConst.RECORD_CHARACTER_MAX != ListSetCharacter.Count)
		{
			ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_WARN1"), delegate
			{
				OnStartConfirm();
			});
		}
		else if (OrangeConst.RECORD_CHARACTER_MAX != ListSetWeapon.Count)
		{
			ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_WARN2"), delegate
			{
				OnStartConfirm();
			});
		}
		else
		{
			OnStartConfirm();
		}
	}

	private void OnStartConfirm()
	{
		ShowCommonMsg(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_NOTICE"), InitializeRecordGrid, SystemSE.CRI_SYSTEMSE_SYS_OK07);
	}

	private void ShowCommonMsg(string p_msg, Callback p_cbYes, SystemSE yesSE = SystemSE.NONE)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
			ui.YesSE = yesSE;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_DEPLOY_CONFIRM"), p_msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), p_cbYes);
		}, true);
	}

	private void InitializeRecordGrid()
	{
		ManagedSingleton<PlayerNetManager>.Instance.InitializeRecordGrid(ListSetCharacter.Select((CharacterInfo x) => x.netInfo.CharacterID).ToList(), ListSetWeapon.Select((WeaponInfo x) => x.netInfo.WeaponID).ToList(), delegate(InitializeRecordGridRes res)
		{
			Code code = (Code)res.Code;
			if ((uint)(code - 110003) > 1u && (uint)(code - 110101) > 2u && code != Code.RECORDGRID_MAP_NOT_NEIGHBOR_POSITION)
			{
				ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo = res.PlayerInfo;
				ManagedSingleton<DeepRecordHelper>.Instance.OtherPlayerList = res.OtherPlayerList;
				ManagedSingleton<DeepRecordHelper>.Instance.MapInfo = res.MapInfo;
				ManagedSingleton<DeepRecordHelper>.Instance.Status = Status.View;
				ManagedSingleton<DeepRecordHelper>.Instance.ClearLog();
				ManagedSingleton<DeepRecordHelper>.Instance.OpenDeepRecordMainUI();
				OnClickCloseBtn();
			}
			else
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(code);
			}
		});
	}

	public void OnToggleMoveChkOn()
	{
		if (!togglesOnOff[0].isOn && toggleInitiated)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
	}

	public void OnToggleMoveChkOff()
	{
		if (!togglesOnOff[1].isOn && toggleInitiated)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
	}

	private void Save()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRecordMoveChk = togglesOnOff[0].isOn;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}

	public override void OnClickCloseBtn()
	{
		Save();
		base.OnClickCloseBtn();
	}

	public override void TopbarInitComplete(Topbar topbar)
	{
		switch (status)
		{
		case Status.Edit:
			topbar.RefreashName("RECORD_TITLE_BATTLE");
			break;
		case Status.View:
			topbar.RefreashName("RECORD_TITLE_DEPLOY");
			break;
		}
	}
}
