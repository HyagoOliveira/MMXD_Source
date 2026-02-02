using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class DeepRecordUseSetUI : OrangeUIBase
{
	public enum SetType
	{
		Character = 0,
		Weapon = 1
	}

	public enum AutoConfigType
	{
		Total = 0,
		Battle = 1,
		Explore = 2,
		Action = 3,
		None = 4
	}

	[SerializeField]
	private OrangeText textTopTitle;

	[SerializeField]
	private OrangeText textSubTitle;

	[SerializeField]
	private OrangeText textTotalVal;

	[SerializeField]
	private OrangeText textBattleVal;

	[SerializeField]
	private OrangeText textExploreVal;

	[SerializeField]
	private OrangeText textActionVal;

	[SerializeField]
	private LoopHorizontalScrollRect scrollRectTop;

	[SerializeField]
	private ScrollRect scrollRectUnder;

	[SerializeField]
	private DeepRecordUseSetUITopUnit prefabTop;

	[SerializeField]
	private DeepRecordUseSetUIUnit prefabSetUnit;

	private const int TOP_VISUAL_COUNT = 12;

	private AutoConfigType autoConfigType = AutoConfigType.None;

	private DeepRecordUseSetUIUnit[] arrSetUnit;

	private CharacterHelper.SortType currentSortType = CharacterHelper.SortType.TOTAL;

	private SetType useSetType;

	private int SET_LIMIT
	{
		get
		{
			return 10;
		}
	}

	public bool IsSetupLimited
	{
		get
		{
			SetType setType = UseSetType;
			if (setType == SetType.Character || setType != SetType.Weapon)
			{
				return ListSetCharacter.Count >= SET_LIMIT;
			}
			return ListSetWeapon.Count >= SET_LIMIT;
		}
	}

	public SetType UseSetType
	{
		get
		{
			return useSetType;
		}
	}

	public List<CharacterInfo> ListAllCharacter { get; private set; } = new List<CharacterInfo>();


	public List<WeaponInfo> ListAllWeapon { get; private set; } = new List<WeaponInfo>();


	public List<CharacterInfo> ListSetCharacter { get; private set; } = new List<CharacterInfo>();


	public List<WeaponInfo> ListSetWeapon { get; private set; } = new List<WeaponInfo>();


	private void Setup(SetType p_setType)
	{
		InitUnderUnit();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		useSetType = p_setType;
		currentSortType = DeepRecordHelper.GetLastSortType();
		SetType setType = useSetType;
		if (setType == SetType.Character || setType != SetType.Weapon)
		{
			UpdateCharacterList(currentSortType);
			scrollRectTop.OrangeInit(prefabTop, 12, ListAllCharacter.Count);
			textTopTitle.UpdateText("RECORD_TITLE_CHARACTER");
			textSubTitle.UpdateText("RECORD_SELECT_CHARACTER");
		}
		else
		{
			UpdateWeaponList(currentSortType);
			scrollRectTop.OrangeInit(prefabTop, 12, ListAllWeapon.Count);
			textTopTitle.UpdateText("RECORD_TITLE_WEAPON");
			textSubTitle.UpdateText("RECORD_SELECT_WEAPON");
		}
		string text = "0";
		textTotalVal.text = text;
		textBattleVal.text = text;
		textExploreVal.text = text;
		textActionVal.text = text;
	}

	public void SetupByList(List<CharacterInfo> p_listSetCharacter)
	{
		Setup(SetType.Character);
		ListSetCharacter = new List<CharacterInfo>();
		ListSetCharacter.AddRange(p_listSetCharacter);
		for (int i = 0; i < ListSetCharacter.Count; i++)
		{
			UpdateSetupCharacter(i, p_listSetCharacter[i].netInfo.CharacterID);
		}
		UpdateRecordVal();
		scrollRectTop.RefreshCellsNew();
	}

	public void SetupByList(List<WeaponInfo> p_listSetWeapon)
	{
		Setup(SetType.Weapon);
		ListSetWeapon = new List<WeaponInfo>();
		ListSetWeapon.AddRange(p_listSetWeapon);
		for (int i = 0; i < ListSetWeapon.Count; i++)
		{
			UpdateSetupWeapon(i, ListSetWeapon[i].netInfo.WeaponID);
		}
		UpdateRecordVal();
		scrollRectTop.RefreshCellsNew();
	}

	private void InitUnderUnit()
	{
		if (arrSetUnit == null)
		{
			RectTransform content = scrollRectUnder.content;
			arrSetUnit = new DeepRecordUseSetUIUnit[SET_LIMIT];
			for (int i = 0; i < SET_LIMIT; i++)
			{
				arrSetUnit[i] = UnityEngine.Object.Instantiate(prefabSetUnit, content);
			}
		}
	}

	private void UpdateSortList()
	{
		CharacterHelper.SortType lastSortType = DeepRecordHelper.GetLastSortType();
		if (currentSortType != lastSortType)
		{
			SetType setType = useSetType;
			if (setType == SetType.Character || setType != SetType.Weapon)
			{
				UpdateCharacterList(lastSortType);
			}
			else
			{
				UpdateWeaponList(lastSortType);
			}
			scrollRectTop.RefreshCellsNew();
		}
	}

	private void UpdateCharacterList(CharacterHelper.SortType p_sortType)
	{
		ListAllCharacter = ManagedSingleton<CharacterHelper>.Instance.SortCharacterList(p_sortType, CharacterHelper.SortStatus.OBTAINED, true, false);
		currentSortType = p_sortType;
	}

	private void UpdateWeaponList(CharacterHelper.SortType p_sortType)
	{
		ListAllWeapon = (from x in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Values
			orderby DeepRecordHelper.GetWeaponRecordVal(p_sortType, x) descending, x.netInfo.WeaponID
			select x).ToList();
		currentSortType = p_sortType;
	}

	public bool AddSelect(int p_idx)
	{
		SetType setType = useSetType;
		if (setType == SetType.Character || setType != SetType.Weapon)
		{
			CharacterInfo characterInfo = ListAllCharacter[p_idx];
			if (ListSetCharacter.Contains(characterInfo))
			{
				RemoveSelectCharacter(characterInfo, false);
				return false;
			}
			if (IsSetupLimited)
			{
				return false;
			}
			ListSetCharacter.Add(characterInfo);
			UpdateSetupCharacter(ListSetCharacter.Count - 1, characterInfo.netInfo.CharacterID);
		}
		else
		{
			WeaponInfo weaponInfo = ListAllWeapon[p_idx];
			if (ListSetWeapon.Contains(weaponInfo))
			{
				RemoveSelectWeapon(weaponInfo, false);
				return false;
			}
			if (IsSetupLimited)
			{
				return false;
			}
			ListSetWeapon.Add(ListAllWeapon[p_idx]);
			UpdateSetupWeapon(ListSetWeapon.Count - 1, weaponInfo.netInfo.WeaponID);
		}
		UpdateRecordVal();
		return true;
	}

	public void RemoveSelectCharacter(CharacterInfo p_characterInfo, bool updateScroll)
	{
		if (ListSetCharacter.Contains(p_characterInfo))
		{
			ListSetCharacter.Remove(p_characterInfo);
			int i;
			for (i = 0; i < ListSetCharacter.Count; i++)
			{
				UpdateSetupCharacter(i, ListSetCharacter[i].netInfo.CharacterID);
			}
			for (; i < SET_LIMIT; i++)
			{
				UpdateSetupCharacter(i, -1);
			}
			UpdateRecordVal();
			if (updateScroll)
			{
				scrollRectTop.RefreshCellsNew();
			}
			autoConfigType = AutoConfigType.None;
		}
	}

	public void RemoveSelectWeapon(WeaponInfo p_weaponInfo, bool updateScroll)
	{
		if (ListSetWeapon.Contains(p_weaponInfo))
		{
			ListSetWeapon.Remove(p_weaponInfo);
			int i;
			for (i = 0; i < ListSetWeapon.Count; i++)
			{
				UpdateSetupWeapon(i, ListSetWeapon[i].netInfo.WeaponID);
			}
			for (; i < SET_LIMIT; i++)
			{
				UpdateSetupWeapon(i, -1);
			}
			UpdateRecordVal();
			if (updateScroll)
			{
				scrollRectTop.RefreshCellsNew();
			}
			autoConfigType = AutoConfigType.None;
		}
	}

	private void UpdateSetupCharacter(int p_targetUnitIdx, int characterId)
	{
		arrSetUnit[p_targetUnitIdx].UpdateCharacterInfo(characterId);
	}

	private void UpdateSetupWeapon(int p_targetUnitIdx, int weaponId)
	{
		arrSetUnit[p_targetUnitIdx].UpdateWeaponInfo(weaponId);
	}

	private void UpdateRecordVal()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < SET_LIMIT; i++)
		{
			num += arrSetUnit[i].TotalVal;
			num2 += arrSetUnit[i].BattleVal;
			num3 += arrSetUnit[i].ExploreVal;
			num4 += arrSetUnit[i].ActionVal;
		}
		textTotalVal.TryTweenValue(num);
		textBattleVal.TryTweenValue(num2);
		textExploreVal.TryTweenValue(num3);
		textActionVal.TryTweenValue(num4);
	}

	private void SetAutoConfigBtn(AutoConfigType p_autoConfigType)
	{
		SetType setType = UseSetType;
		if (setType == SetType.Character || setType != SetType.Weapon)
		{
			ListSetCharacter.Clear();
			CharacterHelper.SortType sortType2 = GetSortTypeByConfigType(p_autoConfigType);
			IOrderedEnumerable<CharacterInfo> source = from x in ListAllCharacter
				orderby DeepRecordHelper.GetCharacterRecordVal(sortType2, x) descending, x.netInfo.CharacterID
				select x;
			int num = Mathf.Min(ListAllCharacter.Count, SET_LIMIT);
			int i;
			for (i = 0; i < num; i++)
			{
				CharacterInfo characterInfo = source.ElementAt(i);
				ListSetCharacter.Add(characterInfo);
				UpdateSetupCharacter(i, characterInfo.netInfo.CharacterID);
			}
			for (; i < SET_LIMIT; i++)
			{
				UpdateSetupCharacter(i, -1);
			}
		}
		else
		{
			ListSetWeapon.Clear();
			CharacterHelper.SortType sortType = GetSortTypeByConfigType(p_autoConfigType);
			IOrderedEnumerable<WeaponInfo> source2 = from x in ListAllWeapon
				orderby DeepRecordHelper.GetWeaponRecordVal(sortType, x) descending, x.netInfo.WeaponID
				select x;
			int num2 = Mathf.Min(ListAllWeapon.Count, SET_LIMIT);
			int j;
			for (j = 0; j < num2; j++)
			{
				WeaponInfo weaponInfo = source2.ElementAt(j);
				ListSetWeapon.Add(weaponInfo);
				UpdateSetupWeapon(j, weaponInfo.netInfo.WeaponID);
			}
			for (; j < SET_LIMIT; j++)
			{
				UpdateSetupWeapon(j, -1);
			}
		}
		UpdateRecordVal();
		scrollRectTop.RefreshCellsNew();
		autoConfigType = p_autoConfigType;
	}

	private CharacterHelper.SortType GetSortTypeByConfigType(AutoConfigType p_autoConfigType)
	{
		switch (p_autoConfigType)
		{
		default:
			return CharacterHelper.SortType.TOTAL;
		case AutoConfigType.Battle:
			return CharacterHelper.SortType.BATTLE;
		case AutoConfigType.Explore:
			return CharacterHelper.SortType.EXPLORE;
		case AutoConfigType.Action:
			return CharacterHelper.SortType.ACTION;
		}
	}

	public void OnClickAutoTotalBtn()
	{
		if (autoConfigType != 0)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			SetAutoConfigBtn(AutoConfigType.Total);
		}
	}

	public void OnClickAutoBattleBtn()
	{
		if (autoConfigType != AutoConfigType.Battle)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			SetAutoConfigBtn(AutoConfigType.Battle);
		}
	}

	public void OnClickAutoExploreBtn()
	{
		if (autoConfigType != AutoConfigType.Explore)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			SetAutoConfigBtn(AutoConfigType.Explore);
		}
	}

	public void OnClickAutoActionBtn()
	{
		if (autoConfigType != AutoConfigType.Action)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			SetAutoConfigBtn(AutoConfigType.Action);
		}
	}

	public void OnClickSortBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordUseSort", delegate(DeepRecordUseSortUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup();
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateSortList));
		});
	}

	public void OnClickOK()
	{
		DeepRecordTeamSetUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<DeepRecordTeamSetUI>("UI_DeepRecordTeamSet");
		if ((bool)uI)
		{
			SetType setType = useSetType;
			if (setType == SetType.Character || setType != SetType.Weapon)
			{
				uI.RefreashCharacter(ListSetCharacter);
			}
			else
			{
				uI.RefreashWeaponList(ListSetWeapon);
			}
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
	}
}
