using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCustomizeUI : OrangeUIBase
{
	public enum PlayerCustomizeTab
	{
		PC_AVATAR = 0,
		PC_SIGN = 1,
		PC_END = 2
	}

	public class BtnClickCB
	{
		public int nBtnID;

		public bool bIsLock;

		public Action<int> action;

		public void OnClick()
		{
			if (!bIsLock && action != null)
			{
				action(nBtnID);
			}
		}
	}

	[Header("Avatar Tab")]
	[SerializeField]
	private GameObject AvatarRoot;

	[SerializeField]
	private GameObject ScrollCell;

	[SerializeField]
	private GridLayoutGroup AvatarInfoGridLayoutGroup;

	[SerializeField]
	private Button UseIconBtn;

	[SerializeField]
	private Image ImgIconSelected;

	[SerializeField]
	private Image ImgIconBGSelected;

	[SerializeField]
	private Image ImgIconUsed;

	[SerializeField]
	private Image ImgIconBGUsed;

	[SerializeField]
	private GameObject QuestTitle;

	[SerializeField]
	private Text QuestText;

	[SerializeField]
	private ScrollRect ScrollView;

	[Header("Sign Tab")]
	[SerializeField]
	private GameObject SignTabRoot;

	[SerializeField]
	private GameObject SignRoot;

	[SerializeField]
	private Text SignCountText;

	[SerializeField]
	private GameObject SignScrollCell;

	[SerializeField]
	private ScrollRect SignScrollView;

	[SerializeField]
	private GridLayoutGroup SignInfoGridLayoutGroup;

	[SerializeField]
	private Button UseSignBtn;

	[SerializeField]
	private Button UnlockSignBtn;

	[SerializeField]
	private GameObject SortRoot;

	[SerializeField]
	private Image MaskImage;

	[SerializeField]
	private GameObject CurrentUsedSignObject;

	[SerializeField]
	private GameObject CurrentSelectedSignObject;

	[SerializeField]
	private Text UsedSignText;

	[SerializeField]
	private Text SelectedSignText;

	[SerializeField]
	private Text CurrentSelectedSignConditionText;

	[SerializeField]
	private GameObject SortOrderImage;

	[SerializeField]
	private Button[] SortTypeBtn;

	[SerializeField]
	private Button[] GetTypeBtn;

	public LoopVerticalScrollRect m_ScrollRect;

	private Image[] SortTypeImg;

	private Image[] GetTypeBtnImg;

	[SerializeField]
	private GameObject[] TargetTabRoots;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_tab;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_equipSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickIconSE;

	private Dictionary<int, CUSTOMIZE_TABLE> idcSignTable;

	public List<int> listSignFlag = new List<int>();

	private List<BtnClickCB> BtnClickCBs = new List<BtnClickCB>();

	[HideInInspector]
	public List<CUSTOMIZE_TABLE> m_listItemTableFiltered = new List<CUSTOMIZE_TABLE>();

	private int CurrentSelectedSignID;

	private int CurrentUsedSignID;

	private int CurrentSelectedType = -1;

	private int CurrentSelectedIcon;

	private int CurrentUsedIcon = 900001;

	public bool bFirstOpen = true;

	private Dictionary<int, ITEM_TABLE> idcPortraitTable;

	public Dictionary<int, bool> idcPortraitFlag = new Dictionary<int, bool>();

	protected Color white = Color.white;

	protected Color grey = Color.grey;

	protected Color clear = Color.clear;

	private string rare_asset_name = "UI_iconsource_{0}_{1}";

	private string frameName = "frame";

	private string bgName = "BG";

	private string small = "_small";

	private string[] strRarity = new string[7] { "Dummy", "D", "C", "B", "A", "S", "SS" };

	public CUSTOMIZE_TABLE GetSignTable(int n_ID)
	{
		CUSTOMIZE_TABLE value = null;
		idcSignTable.TryGetValue(n_ID, out value);
		return value;
	}

	public bool CheckSignFlag(int n_ID)
	{
		return listSignFlag.Contains(n_ID);
	}

	private void UpdateSignFlag()
	{
		Dictionary<int, CUSTOMIZE_TABLE>.Enumerator enumerator = idcSignTable.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CUSTOMIZE_TABLE value = enumerator.Current.Value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_GET_VALUE1) && !listSignFlag.Contains(value.n_ID))
			{
				listSignFlag.Add(value.n_ID);
			}
		}
		SignCountText.text = listSignFlag.Count.ToString();
	}

	public ITEM_TABLE GetPortraitTable(int n_ID)
	{
		return idcPortraitTable[n_ID];
	}

	public int GetCurrentUsedIcon()
	{
		return CurrentUsedIcon;
	}

	public bool CheckPlayerIconFlag(int n_ID)
	{
		return idcPortraitFlag.ContainsKey(n_ID);
	}

	private void UpdateCustomizeFlag()
	{
		Dictionary<int, ITEM_TABLE>.Enumerator enumerator = idcPortraitTable.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ITEM_TABLE value = enumerator.Current.Value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_ID) && !idcPortraitFlag.ContainsKey(value.n_ID))
			{
				idcPortraitFlag.Add(value.n_ID, true);
			}
		}
	}

	private void MakeCustomizeTable()
	{
		idcPortraitTable = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.Where((KeyValuePair<int, ITEM_TABLE> q) => q.Value.n_TYPE == 6).ToDictionary((KeyValuePair<int, ITEM_TABLE> q) => q.Value.n_ID, (KeyValuePair<int, ITEM_TABLE> q) => q.Value);
	}

	private void SetRareInfo(Image image, string assetName, bool whiteColor = true)
	{
		if (null == image)
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
		{
			if (!(null == image))
			{
				image.gameObject.SetActive(true);
				image.sprite = obj;
				image.color = (whiteColor ? white : grey);
			}
		});
	}

	public void SetCurrentUsedIcon(int n_ID, string assetName)
	{
		CurrentUsedIcon = n_ID;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter(assetName), assetName, delegate(Sprite obj)
		{
			if ((bool)obj)
			{
				ImgIconUsed.sprite = obj;
			}
		});
		ITEM_TABLE iTEM_TABLE = idcPortraitTable[n_ID];
		SetRareInfo(ImgIconBGUsed, string.Format(rare_asset_name, bgName, strRarity[iTEM_TABLE.n_RARE] + small));
	}

	public int GetCurrentSelectedIcon()
	{
		return CurrentSelectedIcon;
	}

	public void SetCurrentSelectedIcon(int n_ID, string assetName, Sprite sp, Sprite re, bool bIsGet)
	{
		if (CurrentSelectedIcon != n_ID)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickIconSE);
		}
		CurrentSelectedIcon = n_ID;
		ImgIconSelected.sprite = sp;
		ImgIconBGSelected.sprite = re;
		ImgIconSelected.gameObject.SetActive(true);
		ImgIconBGSelected.gameObject.SetActive(bIsGet);
		UseIconBtn.interactable = CurrentUsedIcon != CurrentSelectedIcon && CheckPlayerIconFlag(n_ID);
		QuestTitle.SetActive(true);
		ITEM_TABLE iTEM_TABLE = idcPortraitTable[n_ID];
		int num = (int)iTEM_TABLE.f_VALUE_X;
		if (num == 1 || num != 2)
		{
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[(int)iTEM_TABLE.f_VALUE_Y];
			QuestText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CUSTOMIZE_GET_CHARA"), ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME));
		}
		else if (ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT.ContainsKey((int)iTEM_TABLE.f_VALUE_Y))
		{
			MISSION_TABLE mISSION_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MISSION_TABLE_DICT[(int)iTEM_TABLE.f_VALUE_Y];
			QuestText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CUSTOMIZE_FINISH_MISSION"), ManagedSingleton<OrangeTextDataManager>.Instance.MISSIONTEXT_TABLE_DICT.GetL10nValue(mISSION_TABLE.w_NAME));
		}
	}

	private void Start()
	{
		SignTabRoot.SetActive(true);
		UseSignBtn.gameObject.SetActive(false);
		UnlockSignBtn.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HashSignShowNewHint.Clear();
		for (int i = 0; i < listSignFlag.Count; i++)
		{
			if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HashSignShowNewHint.Contains(listSignFlag[i]))
			{
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HashSignShowNewHint.Add(listSignFlag[i]);
			}
		}
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.UpdatePlayerHUD();
	}

	public void Setup()
	{
		MakeCustomizeTable();
		UpdateCustomizeFlag();
		CurrentUsedIcon = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.PortraitID;
		List<ITEM_TABLE> list = idcPortraitTable.Values.ToList();
		int count = list.Count;
		int num = (count - count % 7) / 7;
		if (count % 7 > 0)
		{
			num++;
		}
		float y = 200 * num + 60;
		Vector2 sizeDelta = AvatarInfoGridLayoutGroup.GetComponent<RectTransform>().sizeDelta;
		AvatarInfoGridLayoutGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, y);
		int childCount = AvatarInfoGridLayoutGroup.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(AvatarInfoGridLayoutGroup.transform.GetChild(i).gameObject);
		}
		for (int j = 0; j < list.Count; j++)
		{
			ITEM_TABLE iTEM_TABLE = idcPortraitTable[list[j].n_ID];
			GameObject obj = UnityEngine.Object.Instantiate(ScrollCell, AvatarInfoGridLayoutGroup.transform.position, new Quaternion(0f, 0f, 0f, 0f));
			obj.transform.SetParent(AvatarInfoGridLayoutGroup.transform);
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.GetComponent<PlayerIconBaseScrollCell>().Setup(iTEM_TABLE.n_ID);
			if (iTEM_TABLE.n_ID == CurrentUsedIcon && CheckPlayerIconFlag(iTEM_TABLE.n_ID))
			{
				SetCurrentUsedIcon(iTEM_TABLE.n_ID, iTEM_TABLE.s_ICON);
				UseIconBtn.interactable = false;
			}
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnClickUseIcon()
	{
		CurrentUsedIcon = CurrentSelectedIcon;
		UseIconBtn.interactable = CurrentUsedIcon != CurrentSelectedIcon;
		ImgIconUsed.sprite = ImgIconSelected.sprite;
		ImgIconBGUsed.sprite = ImgIconBGSelected.sprite;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_equipSE);
		ManagedSingleton<PlayerNetManager>.Instance.SetPortraitReq(CurrentSelectedIcon, delegate
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD[MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify].m_IconNumber = CurrentSelectedIcon;
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.UpdateCommunityPlayerInfo();
		});
	}

	public void OnSelectType(int typ = 0)
	{
		if (CurrentSelectedType != typ)
		{
			CurrentSelectedType = typ;
			AvatarRoot.SetActive(typ == 0);
			SignRoot.SetActive(typ == 1);
			PlayerCustomizeTab currentSelectedType = (PlayerCustomizeTab)CurrentSelectedType;
			if (currentSelectedType == PlayerCustomizeTab.PC_AVATAR || currentSelectedType != PlayerCustomizeTab.PC_SIGN)
			{
				TargetTabRoots[0].SetActive(true);
				TargetTabRoots[1].SetActive(false);
				Setup();
			}
			else
			{
				TargetTabRoots[0].SetActive(false);
				TargetTabRoots[1].SetActive(true);
				SignSetup();
			}
			if (!bFirstOpen)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_tab);
			}
			else
			{
				bFirstOpen = false;
			}
		}
	}

	public override void OnClickCloseBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		base.OnClickCloseBtn();
	}

	public int GetCurrentSelectedSignID()
	{
		return CurrentSelectedSignID;
	}

	public void SetCurrentSelectedSignID(int id)
	{
		if (CurrentSelectedSignID == id)
		{
			return;
		}
		CurrentSelectedSignID = id;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
		int childCount = CurrentSelectedSignObject.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(CurrentSelectedSignObject.transform.GetChild(i).gameObject);
		}
		CUSTOMIZE_TABLE value = null;
		if (idcSignTable.TryGetValue(CurrentSelectedSignID, out value))
		{
			GameObject obj = UnityEngine.Object.Instantiate(SignScrollCell, CurrentSelectedSignObject.transform.position, new Quaternion(0f, 0f, 0f, 0f));
			obj.transform.SetParent(CurrentSelectedSignObject.transform);
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.GetComponent<CommonSignBase>().Setup(value.n_ID);
			ITEM_TABLE value2 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.ITEM_TABLE_DICT.TryGetValue(value.n_GET_VALUE1, out value2))
			{
				SelectedSignText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value2.w_NAME);
				CurrentSelectedSignConditionText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value2.w_TIP);
			}
		}
		bool flag = CheckSignFlag(id);
		UseSignBtn.gameObject.SetActive(flag);
		if (flag)
		{
			UseSignBtn.interactable = CurrentUsedSignID != CurrentSelectedSignID && CheckSignFlag(id);
			return;
		}
		UnlockSignBtn.interactable = false;
		SelectedSignText.text = "";
	}

	public void SignSetup()
	{
		InitSortBtn();
		if (idcSignTable == null)
		{
			idcSignTable = ManagedSingleton<OrangeDataManager>.Instance.CUSTOMIZE_TABLE_DICT.Where((KeyValuePair<int, CUSTOMIZE_TABLE> q) => q.Value.n_TYPE == 1).ToDictionary((KeyValuePair<int, CUSTOMIZE_TABLE> q) => q.Value.n_ID, (KeyValuePair<int, CUSTOMIZE_TABLE> q) => q.Value);
		}
		CurrentUsedSignID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID;
		UpdateSignFlag();
		if (ManagedSingleton<EquipHelper>.Instance.SignSortDescend == 1)
		{
			SortOrderImage.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImage.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		OnSortGo();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnClickSign()
	{
		CurrentUsedSignID = CurrentSelectedSignID;
		UseSignBtn.interactable = false;
		UnlockSignBtn.interactable = false;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_equipSE);
		ManagedSingleton<PlayerNetManager>.Instance.SetTitleReq(CurrentSelectedSignID, delegate
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD[MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify].m_TitleNumber = CurrentSelectedSignID;
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.UpdateCommunityPlayerInfo();
			OnSortGo();
		});
	}

	public void OnClickSortPanelBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		SortRoot.SetActive(true);
		MaskImage.gameObject.SetActive(true);
	}

	public void OnCloseSortRoot()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
	}

	public void SetSortType(int nBID)
	{
		int num = 1 << nBID;
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nSignSortKey & (uint)num) == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		ManagedSingleton<EquipHelper>.Instance.nSignSortKey = (EquipHelper.SIGN_SORT_KEY)num;
		for (int i = 0; i < SortTypeBtn.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nSignSortKey & (uint)(1 << i)) != 0)
			{
				SortTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	public void SetGetBtnType(int nBID)
	{
		int num = 1 << nBID;
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nSignGetKey & (uint)num) == 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		ManagedSingleton<EquipHelper>.Instance.nSignGetKey = (EquipHelper.SIGN_GET_TYPE)num;
		for (int i = 0; i < GetTypeBtn.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nSignGetKey & (uint)(1 << i)) != 0)
			{
				GetTypeBtnImg[i].gameObject.SetActive(true);
			}
			else
			{
				GetTypeBtnImg[i].gameObject.SetActive(false);
			}
		}
	}

	private void UpdateButtonType()
	{
		for (int i = 0; i < SortTypeBtn.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nSignSortKey & (uint)(1 << i)) != 0)
			{
				SortTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[i].gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < GetTypeBtn.Length; j++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nSignGetKey & (uint)(1 << j)) != 0)
			{
				GetTypeBtnImg[j].gameObject.SetActive(true);
			}
			else
			{
				GetTypeBtnImg[j].gameObject.SetActive(false);
			}
		}
	}

	private void InitSortBtn()
	{
		BtnClickCBs.Clear();
		SortTypeImg = new Image[SortTypeBtn.Length];
		for (int i = 0; i < SortTypeBtn.Length; i++)
		{
			BtnClickCB btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = i;
			btnClickCB.action = (Action<int>)Delegate.Combine(btnClickCB.action, new Action<int>(SetSortType));
			SortTypeBtn[i].onClick.RemoveAllListeners();
			SortTypeBtn[i].onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			SortTypeImg[i] = SortTypeBtn[i].transform.Find("Image").GetComponent<Image>();
		}
		GetTypeBtnImg = new Image[GetTypeBtn.Length];
		for (int j = 0; j < GetTypeBtn.Length; j++)
		{
			BtnClickCB btnClickCB2 = new BtnClickCB();
			btnClickCB2.nBtnID = j;
			btnClickCB2.action = (Action<int>)Delegate.Combine(btnClickCB2.action, new Action<int>(SetGetBtnType));
			GetTypeBtn[j].onClick.RemoveAllListeners();
			GetTypeBtn[j].onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(btnClickCB2);
			GetTypeBtnImg[j] = GetTypeBtn[j].transform.Find("Image").GetComponent<Image>();
		}
		UpdateButtonType();
		SortRoot.SetActive(false);
	}

	public void OnSortGo(bool bUpdateScrollRect = true, bool bOffset = false)
	{
		List<CUSTOMIZE_TABLE> list = idcSignTable.Values.ToList();
		m_listItemTableFiltered.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			if ((ManagedSingleton<EquipHelper>.Instance.nSignGetKey & EquipHelper.SIGN_GET_TYPE.SIGN_GETED) == EquipHelper.SIGN_GET_TYPE.SIGN_GETED)
			{
				if (!CheckSignFlag(list[i].n_ID))
				{
					continue;
				}
			}
			else if ((ManagedSingleton<EquipHelper>.Instance.nSignGetKey & EquipHelper.SIGN_GET_TYPE.SIGN_FRAGS) == EquipHelper.SIGN_GET_TYPE.SIGN_FRAGS && CheckSignFlag(list[i].n_ID))
			{
				continue;
			}
			m_listItemTableFiltered.Add(list[i]);
		}
		m_listItemTableFiltered.Sort((CUSTOMIZE_TABLE x, CUSTOMIZE_TABLE y) => x.n_ID.CompareTo(y.n_ID));
		if ((ManagedSingleton<EquipHelper>.Instance.nSignSortKey & EquipHelper.SIGN_SORT_KEY.SIGN_SORT_RARITY) == EquipHelper.SIGN_SORT_KEY.SIGN_SORT_RARITY)
		{
			m_listItemTableFiltered.Sort(delegate(CUSTOMIZE_TABLE x, CUSTOMIZE_TABLE y)
			{
				CUSTOMIZE_TABLE value5 = null;
				CUSTOMIZE_TABLE value6 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CUSTOMIZE_TABLE_DICT.TryGetValue(x.n_ID, out value5);
				ManagedSingleton<OrangeDataManager>.Instance.CUSTOMIZE_TABLE_DICT.TryGetValue(y.n_ID, out value6);
				if (value5 == null || value6 == null)
				{
					return 0;
				}
				int num2 = value5.n_RARITY.CompareTo(value6.n_RARITY);
				if (num2 == 0)
				{
					num2 = x.n_ID.CompareTo(y.n_ID);
				}
				return num2;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nSignSortKey & EquipHelper.SIGN_SORT_KEY.SIGN_SORT_ID) == EquipHelper.SIGN_SORT_KEY.SIGN_SORT_ID)
		{
			m_listItemTableFiltered.Sort(delegate(CUSTOMIZE_TABLE x, CUSTOMIZE_TABLE y)
			{
				CUSTOMIZE_TABLE value3 = null;
				CUSTOMIZE_TABLE value4 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CUSTOMIZE_TABLE_DICT.TryGetValue(x.n_ID, out value3);
				ManagedSingleton<OrangeDataManager>.Instance.CUSTOMIZE_TABLE_DICT.TryGetValue(y.n_ID, out value4);
				return (value3 != null && value4 != null) ? x.n_ID.CompareTo(y.n_ID) : 0;
			});
		}
		if (ManagedSingleton<EquipHelper>.Instance.SignSortDescend == 1)
		{
			m_listItemTableFiltered.Reverse();
		}
		int childCount = CurrentUsedSignObject.transform.childCount;
		for (int j = 0; j < childCount; j++)
		{
			UnityEngine.Object.Destroy(CurrentUsedSignObject.transform.GetChild(j).gameObject);
		}
		int count = m_listItemTableFiltered.Count;
		int num = (count - count % 3) / 3;
		if (count % 3 > 0)
		{
			num++;
		}
		m_ScrollRect.totalCount = num;
		m_ScrollRect.RefillCells();
		GameObject obj = UnityEngine.Object.Instantiate(SignScrollCell, CurrentUsedSignObject.transform.position, new Quaternion(0f, 0f, 0f, 0f));
		obj.transform.SetParent(CurrentUsedSignObject.transform);
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.GetComponent<CommonSignBase>().SetupSign(CurrentUsedSignID);
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
		if (CurrentUsedSignID <= 0)
		{
			UsedSignText.text = "";
			return;
		}
		CUSTOMIZE_TABLE value = null;
		if (idcSignTable.TryGetValue(CurrentUsedSignID, out value))
		{
			ITEM_TABLE value2 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.ITEM_TABLE_DICT.TryGetValue(value.n_GET_VALUE1, out value2))
			{
				UsedSignText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(value2.w_NAME);
			}
		}
	}

	public void OnSortOrder()
	{
		ManagedSingleton<EquipHelper>.Instance.SignSortDescend = ((ManagedSingleton<EquipHelper>.Instance.SignSortDescend != 1) ? 1 : 0);
		if (ManagedSingleton<EquipHelper>.Instance.SignSortDescend == 1)
		{
			SortOrderImage.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			SortOrderImage.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		OnSortGo();
	}

	public void OnClickSortBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		OnSortGo();
	}
}
