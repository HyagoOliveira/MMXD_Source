#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using DragonBones;
using NaughtyAttributes;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class CardInfoUI : OrangeUIBase
{
	[Serializable]
	public class expiteminfo
	{
		public int mHaveNum;

		public int nUseNum;

		public ITEM_TABLE tITEM_TABLE;
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

	public enum CardInfoType : short
	{
		Info = 0,
		Fusion = 1,
		Upgrade = 2
	}

	[Header("global")]
	public int nTargetCardSeqID;

	public int nTargetCardID;

	private int nNowWeaponID = -1;

	private STAR_TABLE tSTAR_TABLE;

	private SKILL_TABLE tSKILL_TABLE;

	private int nNowExp;

	private int nNeedExp;

	private int nExpNowLV;

	private int nNowStar;

	private int nNowInfoIndex = -1;

	private int nNextInfoIndex = -1;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_bookBtn;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_bookLVUPBtn;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_closeWindowBtn;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickExpItem;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_addLVUp;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_addLVExp;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickTab;

	private bool useLVUpSE;

	public Button[] buttons;

	private CanvasGroup[] InfoCanvasGroups;

	public Text[] s_NAME;

	public GameObject[] Ctrlbtns;

	private StageLoadIcon MainBG0;

	[SerializeField]
	private Button[] TabButtons;

	[SerializeField]
	private GameObject[] InfoRoots;

	[SerializeField]
	private Image MaskImage;

	[SerializeField]
	private GameObject SortRoot;

	[SerializeField]
	private Button[] CardType;

	[SerializeField]
	private Button[] SortType;

	[SerializeField]
	private Button[] GetTypeBtn;

	private Image[] CardTypeImg;

	private Image[] SortTypeImg;

	private Image[] GetTypeBtnImg;

	[SerializeField]
	private Canvas RootInfo0;

	[SerializeField]
	private Canvas RootInfo1;

	[SerializeField]
	private Canvas RootInfo2;

	[SerializeField]
	private Canvas QuickUpgradeRoot;

	[SerializeField]
	private Slider expSlider;

	[SerializeField]
	private Text TargetLvText;

	[SerializeField]
	private Text RootInfoText1;

	[SerializeField]
	private Text RootInfoText2;

	[SerializeField]
	private Text RootInfoText3;

	[SerializeField]
	private GameObject QuickIconRoot;

	[SerializeField]
	private GameObject refPrefabSmall;

	[SerializeField]
	private OrangeText m_galleryProgressText;

	[SerializeField]
	private RectTransform m_galleryProgressBar;

	[SerializeField]
	private Image[] m_colorBar;

	[SerializeField]
	private Image m_progressBarImg;

	[SerializeField]
	private GameObject m_bookFrame;

	[SerializeField]
	private UnityArmatureComponent BookUPEffect;

	[SerializeField]
	private Button BookBtn;

	[Header("card info")]
	[SerializeField]
	private Button SetLockBtn;

	[SerializeField]
	private CardBase CardBaseRoot;

	[Header("info0")]
	[SerializeField]
	private OrangeText CardAtkText;

	[SerializeField]
	private OrangeText CardHpText;

	[SerializeField]
	private OrangeText CardDefText;

	[SerializeField]
	private GameObject CardSkillRoot1;

	[SerializeField]
	private GameObject CardSkillRoot2;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot1;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot2;

	[SerializeField]
	private OrangeText CardSkillText1;

	[SerializeField]
	private OrangeText CardSkillText2;

	[SerializeField]
	private Image CardSkillImage1;

	[SerializeField]
	private Image CardSkillImage2;

	[SerializeField]
	private OrangeText CardSkillNameText1;

	[SerializeField]
	private OrangeText CardSkillNameText2;

	[SerializeField]
	private OrangeText CardSkillRuleText1;

	[SerializeField]
	private OrangeText CardSkillRuleText2;

	[SerializeField]
	private GameObject[] starObjectArrayCB;

	[SerializeField]
	private GameObject[] starFrameObjectArrayCB;

	[Header("info1")]
	[SerializeField]
	private LoopVerticalScrollRect m_ScrollRect;

	[SerializeField]
	private CardFusionCell m_CardFusionCell;

	[SerializeField]
	private Button m_FusionBtn;

	[SerializeField]
	private OrangeText FusionExpText;

	[SerializeField]
	private OrangeText FusionSelectedText;

	[SerializeField]
	private OrangeText FusionLevelText;

	[SerializeField]
	private OrangeText FusionCurrentLevelText;

	[SerializeField]
	private Image[] FusionStarImageRoots;

	[SerializeField]
	private Image[] FusionStarFrameImageRoots;

	[SerializeField]
	private GameObject[] starObjectArray1;

	[SerializeField]
	private GameObject[] starFrameObjectArray1;

	[Header("info2")]
	[SerializeField]
	private ExpButtonRef[] expitems;

	[SerializeField]
	private ExpButtonRef[] expqucilitems;

	[SerializeField]
	private ExpButtonRef[] staritems;

	[SerializeField]
	private OrangeText UpgradeExpText;

	[SerializeField]
	private OrangeText UpgradeLevelText;

	[SerializeField]
	private OrangeText UpgradeCurrentLevelText;

	[SerializeField]
	private Image[] UpgradeStarImageRoots;

	[SerializeField]
	private Image[] UpgradeStarFrameImageRoots;

	[SerializeField]
	private Button UpgradeBtn;

	[SerializeField]
	private Button QuickExpUpgradeGo;

	[SerializeField]
	private GameObject[] starObjectArray2;

	[SerializeField]
	private GameObject[] starFrameObjectArray2;

	[HideInInspector]
	public List<CardInfo> listHasCards = new List<CardInfo>();

	[HideInInspector]
	public bool bNeedInitList;

	[HideInInspector]
	public bool bOnlyShowBasic;

	[HideInInspector]
	public bool bUseGoCheckUISort;

	private expiteminfo[] expiteminfos;

	private expiteminfo[] stariteminfos;

	private Color disablecolor = new Color(0.39f, 0.39f, 0.39f);

	[SerializeField]
	private GameObject BtnL;

	[SerializeField]
	private GameObject BtnR;

	private StarUpEffect m_starUpEffectCB;

	private StarUpEffect m_starUpEffect;

	private UpgradeEffect m_upgradeEffect;

	private Upgrade3DEffect m_upgrade3DEffect;

	private GameObject m_unlockEffect;

	private float m_unlockEffectLength;

	private GameObject m_levelUpEffect;

	private GameObject m_levelUpWordEffect;

	private Vector3 m_effectOffset = new Vector3(0f, -5f, 0f);

	private bool bEffectLock;

	private int nQucikLV;

	private bool playStarUp;

	private bool bFormGallery;

	private bool bOverStarCount;

	private CardMainUI tCardMainUI;

	private CardInfoType CurrentCardInfoType;

	private List<BtnClickCB> BtnClickCBs = new List<BtnClickCB>();

	private int[] ntyps = new int[6] { 1, 2, 8, 4, 16, 0 };

	private int CurrentNetCardInfoFilteredIndex;

	private List<NetCardInfo> m_listNetCardInfo = new List<NetCardInfo>();

	private List<NetCardInfo> m_listNetCardInfoFiltered = new List<NetCardInfo>();

	private List<int> m_sortedLevelList = new List<int>();

	private Dictionary<int, NetCardInfo> m_dictSelectedCardInfo = new Dictionary<int, NetCardInfo>();

	private int ShowReconfirmCount;

	private Dictionary<int, CardFusionCell> m_dictCardFusionCell = new Dictionary<int, CardFusionCell>();

	public NetCardInfo CurrentNetCardInfo;

	public EXP_TABLE CurrentExpTable;

	public CARD_TABLE CurrentCardTable;

	private int CurrentTabButtonType;

	private CardColorType tmpCardSortType;

	private EquipHelper.CARD_SORT_KEY tmpCardSortKey;

	private List<NetCharacterCardSlotInfo> tCharacterCardSlotInfoList = new List<NetCharacterCardSlotInfo>();

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryInfo = new List<GALLERY_TABLE>();

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryUnlock = new List<GALLERY_TABLE>();

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryLock = new List<GALLERY_TABLE>();

	public void SetCardType(int nBID)
	{
		if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType & (uint)ntyps[nBID]) == (uint)ntyps[nBID])
		{
			ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType = (CardColorType)((int)ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType & ~ntyps[nBID]);
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType = (CardColorType)((int)ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType | ntyps[nBID]);
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		for (int i = 0; i < CardType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType & (uint)ntyps[i]) == (uint)ntyps[i])
			{
				CardTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				CardTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	public void SetSortType(int nBID)
	{
		int num = 1 << nBID;
		if (ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey != (EquipHelper.CARD_SORT_KEY)num)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey = (EquipHelper.CARD_SORT_KEY)num;
		}
		for (int i = 0; i < SortType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey & (uint)(1 << i)) != 0)
			{
				SortTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[i].gameObject.SetActive(false);
			}
		}
	}

	private void UpdateButtonType()
	{
		for (int i = 0; i < CardType.Length; i++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType & (uint)ntyps[i]) == (uint)ntyps[i])
			{
				CardTypeImg[i].gameObject.SetActive(true);
			}
			else
			{
				CardTypeImg[i].gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < SortType.Length; j++)
		{
			if (((uint)ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey & (uint)(1 << j)) != 0)
			{
				SortTypeImg[j].gameObject.SetActive(true);
			}
			else
			{
				SortTypeImg[j].gameObject.SetActive(false);
			}
		}
	}

	private void InitSortBtn()
	{
		BtnClickCBs.Clear();
		CardTypeImg = new Image[CardType.Length];
		for (int i = 0; i < CardType.Length; i++)
		{
			BtnClickCB btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = i;
			btnClickCB.action = (Action<int>)Delegate.Combine(btnClickCB.action, new Action<int>(SetCardType));
			CardType[i].onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			CardTypeImg[i] = CardType[i].transform.Find("Image").GetComponent<Image>();
		}
		SortTypeImg = new Image[SortType.Length];
		for (int j = 0; j < SortType.Length; j++)
		{
			BtnClickCB btnClickCB2 = new BtnClickCB();
			btnClickCB2.nBtnID = j;
			btnClickCB2.action = (Action<int>)Delegate.Combine(btnClickCB2.action, new Action<int>(SetSortType));
			SortType[j].onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(btnClickCB2);
			SortTypeImg[j] = SortType[j].transform.Find("Image").GetComponent<Image>();
		}
		UpdateButtonType();
		SortRoot.SetActive(false);
	}

	private void InitQuickExpItem()
	{
		for (int i = 0; i < expqucilitems.Length; i++)
		{
			if (expiteminfos[i].mHaveNum > 0)
			{
				expqucilitems[i].BtnLabel.text = expiteminfos[i].nUseNum + "/" + expiteminfos[i].mHaveNum;
				expqucilitems[i].BtnLabel.color = Color.white;
				expqucilitems[i].Button.interactable = true;
				expqucilitems[i].AddBtn.gameObject.SetActive(false);
				if (expiteminfos[i].nUseNum > 0)
				{
					expqucilitems[i].UnuseBtn.gameObject.SetActive(true);
				}
				else
				{
					expqucilitems[i].UnuseBtn.gameObject.SetActive(false);
				}
				expqucilitems[i].frmimg.color = Color.white;
				expqucilitems[i].bgimg.color = Color.white;
			}
			else
			{
				expqucilitems[i].BtnLabel.text = "0/0";
				expqucilitems[i].BtnLabel.color = Color.red;
				expqucilitems[i].Button.interactable = false;
				expqucilitems[i].AddBtn.gameObject.SetActive(true);
				expqucilitems[i].UnuseBtn.gameObject.SetActive(false);
				expqucilitems[i].frmimg.color = disablecolor;
				expqucilitems[i].bgimg.color = disablecolor;
			}
		}
	}

	private void InitExpItem()
	{
		IEnumerable<KeyValuePair<int, ITEM_TABLE>> source = from tOO in ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT
			where tOO.Value.n_TYPE == 2 && tOO.Value.n_TYPE_X == 3
			orderby tOO.Value.n_ID
			select tOO;
		int num = source.Count();
		for (int i = 0; i < num; i++)
		{
			expiteminfos[i].tITEM_TABLE = source.ElementAt(i).Value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(expiteminfos[i].tITEM_TABLE.n_ID))
			{
				ItemInfo itemInfo = ManagedSingleton<PlayerNetManager>.Instance.dicItem[expiteminfos[i].tITEM_TABLE.n_ID];
				expiteminfos[i].mHaveNum = itemInfo.netItemInfo.Stack;
				expiteminfos[i].nUseNum = 0;
			}
			else
			{
				expiteminfos[i].mHaveNum = 0;
				expiteminfos[i].nUseNum = 0;
			}
		}
		for (int j = 0; j < expitems.Length; j++)
		{
			int num2 = 0;
			if (expiteminfos[j].tITEM_TABLE != null)
			{
				num2 = (int)expiteminfos[j].tITEM_TABLE.f_VALUE_X;
				expitems[j].Button.gameObject.SetActive(true);
				UpdateItemNeedInfo(expiteminfos[j].tITEM_TABLE, expitems[j].BtnImgae, expitems[j].frmimg, expitems[j].bgimg, null);
			}
			else
			{
				expitems[j].Button.gameObject.SetActive(false);
			}
			expitems[j].MsgText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EXP_ITEM") + num2;
			if (expiteminfos[j].mHaveNum > 0)
			{
				expitems[j].BtnLabel.text = expiteminfos[j].nUseNum + "/" + expiteminfos[j].mHaveNum;
				expitems[j].BtnLabel.color = Color.white;
				expitems[j].Button.interactable = true;
				expitems[j].AddBtn.gameObject.SetActive(false);
				if (expiteminfos[j].nUseNum > 0)
				{
					expitems[j].UnuseBtn.gameObject.SetActive(true);
				}
				else
				{
					expitems[j].UnuseBtn.gameObject.SetActive(false);
				}
				expitems[j].frmimg.color = Color.white;
				expitems[j].bgimg.color = Color.white;
			}
			else
			{
				expitems[j].BtnLabel.text = "0/0";
				expitems[j].BtnLabel.color = Color.red;
				expitems[j].Button.interactable = false;
				expitems[j].AddBtn.gameObject.SetActive(true);
				expitems[j].UnuseBtn.gameObject.SetActive(false);
				expitems[j].frmimg.color = disablecolor;
				expitems[j].bgimg.color = disablecolor;
			}
		}
		for (int k = 0; k < expqucilitems.Length; k++)
		{
			if (expiteminfos[k].tITEM_TABLE != null)
			{
				expqucilitems[k].Button.gameObject.SetActive(true);
				UpdateItemNeedInfo(expiteminfos[k].tITEM_TABLE, expqucilitems[k].BtnImgae, expqucilitems[k].frmimg, expqucilitems[k].bgimg, null);
			}
			else
			{
				expqucilitems[k].Button.gameObject.SetActive(false);
			}
		}
	}

	private void InitStarItem()
	{
		List<ITEM_TABLE> list = (from item in ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.Values
			where item.n_TYPE == 2 && item.n_TYPE_X == 7
			orderby item.n_ID
			select item).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			expiteminfo expiteminfo = stariteminfos[i];
			expiteminfo.tITEM_TABLE = list[i];
			expiteminfo.nUseNum = 0;
			ItemInfo value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(expiteminfo.tITEM_TABLE.n_ID, out value))
			{
				expiteminfo.mHaveNum = value.netItemInfo.Stack;
			}
			else
			{
				expiteminfo.mHaveNum = 0;
			}
		}
		for (int j = 0; j < staritems.Length; j++)
		{
			int num = 0;
			if (stariteminfos[j].tITEM_TABLE != null)
			{
				if (stariteminfos[j].tITEM_TABLE.n_RARE >= CurrentCardTable.n_RARITY)
				{
					num = (int)stariteminfos[j].tITEM_TABLE.f_VALUE_X;
					staritems[j].Button.gameObject.SetActive(true);
					UpdateItemNeedInfo(stariteminfos[j].tITEM_TABLE, staritems[j].BtnImgae, staritems[j].frmimg, staritems[j].bgimg, null);
				}
				else
				{
					staritems[j].Button.gameObject.SetActive(false);
				}
			}
			else
			{
				staritems[j].Button.gameObject.SetActive(false);
			}
			staritems[j].MsgText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EXP_ITEM") + num;
			if (stariteminfos[j].mHaveNum > 0)
			{
				staritems[j].BtnLabel.text = stariteminfos[j].nUseNum + "/" + stariteminfos[j].mHaveNum;
				staritems[j].BtnLabel.color = Color.white;
				staritems[j].Button.interactable = true;
				staritems[j].AddBtn.gameObject.SetActive(false);
				if (stariteminfos[j].nUseNum > 0)
				{
					staritems[j].UnuseBtn.gameObject.SetActive(true);
				}
				else
				{
					staritems[j].UnuseBtn.gameObject.SetActive(false);
				}
				staritems[j].frmimg.color = Color.white;
				staritems[j].bgimg.color = Color.white;
			}
			else
			{
				staritems[j].BtnLabel.text = "0/0";
				staritems[j].BtnLabel.color = Color.red;
				staritems[j].Button.interactable = false;
				staritems[j].AddBtn.gameObject.SetActive(true);
				staritems[j].UnuseBtn.gameObject.SetActive(false);
				staritems[j].frmimg.color = disablecolor;
				staritems[j].bgimg.color = disablecolor;
			}
		}
	}

	public void InitData()
	{
		expiteminfos = new expiteminfo[expitems.Length];
		for (int i = 0; i < expitems.Length; i++)
		{
			expiteminfos[i] = new expiteminfo();
		}
		stariteminfos = new expiteminfo[staritems.Length];
		for (int j = 0; j < staritems.Length; j++)
		{
			stariteminfos[j] = new expiteminfo();
		}
		InitSortBtn();
		for (int k = 0; k < expitems.Length; k++)
		{
			BtnClickCB btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = k;
			BtnClickCB btnClickCB2 = btnClickCB;
			btnClickCB2.action = (Action<int>)Delegate.Combine(btnClickCB2.action, new Action<int>(OnExpItemBtnCB));
			expitems[k].Button.onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = k;
			BtnClickCB btnClickCB3 = btnClickCB;
			btnClickCB3.action = (Action<int>)Delegate.Combine(btnClickCB3.action, new Action<int>(OnExpItemUnuseBtnCB));
			expitems[k].UnuseBtn.onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = k;
			BtnClickCB btnClickCB4 = btnClickCB;
			btnClickCB4.action = (Action<int>)Delegate.Combine(btnClickCB4.action, new Action<int>(OnExpItemAddBtnCB));
			expitems[k].AddBtn.onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = k;
			BtnClickCB btnClickCB5 = btnClickCB;
			btnClickCB5.action = (Action<int>)Delegate.Combine(btnClickCB5.action, new Action<int>(OnQuickExpItemAddBtnCB));
			expqucilitems[k].AddBtn.onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
		}
		for (int l = 0; l < staritems.Length; l++)
		{
			BtnClickCB btnClickCB6 = new BtnClickCB();
			btnClickCB6.nBtnID = l;
			BtnClickCB btnClickCB7 = btnClickCB6;
			btnClickCB7.action = (Action<int>)Delegate.Combine(btnClickCB7.action, new Action<int>(OnStarItemBtnCB));
			staritems[l].Button.onClick.AddListener(btnClickCB6.OnClick);
			BtnClickCBs.Add(btnClickCB6);
			btnClickCB6 = new BtnClickCB();
			btnClickCB6.nBtnID = l;
			BtnClickCB btnClickCB8 = btnClickCB6;
			btnClickCB8.action = (Action<int>)Delegate.Combine(btnClickCB8.action, new Action<int>(OnStarItemUnuseBtnCB));
			staritems[l].UnuseBtn.onClick.AddListener(btnClickCB6.OnClick);
			BtnClickCBs.Add(btnClickCB6);
			btnClickCB6 = new BtnClickCB();
			btnClickCB6.nBtnID = l;
			BtnClickCB btnClickCB9 = btnClickCB6;
			btnClickCB9.action = (Action<int>)Delegate.Combine(btnClickCB9.action, new Action<int>(OnStarItemAddBtnCB));
			staritems[l].AddBtn.onClick.AddListener(btnClickCB6.OnClick);
			BtnClickCBs.Add(btnClickCB6);
		}
		QuickUpgradeRoot.enabled = false;
		if (m_levelUpEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "levelupeffect", "LevelUpEffect", delegate(GameObject asset)
			{
				m_levelUpEffect = UnityEngine.Object.Instantiate(asset, base.transform);
				m_levelUpEffect.transform.position = CardBaseRoot.transform.position + m_effectOffset;
				m_levelUpEffect.SetActive(false);
			});
		}
		if (m_levelUpWordEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "levelupwordeffect", "LevelUpWordEffect", delegate(GameObject asset)
			{
				m_levelUpWordEffect = UnityEngine.Object.Instantiate(asset, base.transform);
				m_levelUpWordEffect.transform.position = CardBaseRoot.transform.position + m_effectOffset;
				m_levelUpWordEffect.SetActive(false);
			});
		}
	}

	public void Setup()
	{
		if (tCardMainUI == null)
		{
			tCardMainUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
		}
		if (MonoBehaviourSingleton<UIManager>.Instance.GetUI<IllustrationTargetUI>("UI_IllustrationTarget") != null)
		{
			bFormGallery = true;
			BookBtn.gameObject.SetActive(false);
		}
		nNowInfoIndex = 0;
		if (bOnlyShowBasic)
		{
			SetLockBtn.gameObject.SetActive(false);
			TabButtons[1].gameObject.SetActive(false);
			TabButtons[2].gameObject.SetActive(false);
			BookBtn.gameObject.SetActive(false);
		}
		if (bNeedInitList)
		{
			listHasCards = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
		}
		InitCardInfo();
		if (tCardMainUI != null)
		{
			for (int i = 0; i < tCardMainUI.m_listNetCardInfoFiltered.Count; i++)
			{
				NetCardInfo netCardInfo = tCardMainUI.m_listNetCardInfoFiltered[i];
				if (nTargetCardSeqID == netCardInfo.CardSeqID)
				{
					BtnL.SetActive(i > 0);
					BtnR.SetActive(i + 1 < tCardMainUI.m_listNetCardInfoFiltered.Count);
					CurrentNetCardInfoFilteredIndex = i;
					OnTabButtonClick(CurrentTabButtonType);
					break;
				}
			}
		}
		else
		{
			BtnL.SetActive(false);
			BtnR.SetActive(false);
		}
		base._EscapeEvent = EscapeEvent.CUSTOM;
	}

	private void Start()
	{
		InitData();
		Setup();
	}

	private void OnDestroy()
	{
	}

	private void ResetExpItems(bool bUpgradeBtn = true)
	{
		UpgradeExpText.text = "0";
		bOverStarCount = false;
		UpgradeLevelText.text = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(CurrentNetCardInfo.Exp).ToString();
		UpdateStarImage(UpgradeStarImageRoots, UpgradeStarFrameImageRoots, CurrentNetCardInfo.Star);
		bool flag = false;
		for (int i = 0; i < expitems.Length; i++)
		{
			expitems[i].BtnLabel.text = expiteminfos[i].nUseNum + "/" + expiteminfos[i].mHaveNum;
			if (expiteminfos[i].nUseNum > 0)
			{
				expitems[i].UnuseBtn.gameObject.SetActive(true);
				flag = true;
			}
			else
			{
				expitems[i].UnuseBtn.gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < staritems.Length; j++)
		{
			staritems[j].BtnLabel.text = stariteminfos[j].nUseNum + "/" + stariteminfos[j].mHaveNum;
			if (stariteminfos[j].nUseNum > 0)
			{
				staritems[j].UnuseBtn.gameObject.SetActive(true);
				flag = true;
			}
			else
			{
				staritems[j].UnuseBtn.gameObject.SetActive(false);
			}
		}
		UpgradeBtn.interactable = flag && bUpgradeBtn;
	}

	private void UpdateStarImage(Image[] obs, Image[] fobs, int star)
	{
		for (int i = 0; i < obs.Length; i++)
		{
			if (star > i)
			{
				bool flag = i < CurrentNetCardInfo.Star;
				obs[i].gameObject.SetActive(flag);
				fobs[i].gameObject.SetActive(!flag);
			}
			else
			{
				obs[i].gameObject.SetActive(false);
				fobs[i].gameObject.SetActive(false);
			}
		}
	}

	private void ResetScrollRect()
	{
		FusionExpText.text = "0";
		FusionSelectedText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ALREADY_SELECTED_COUNT"), 0, OrangeConst.CARD_FUSION_LIMIT);
		bOverStarCount = false;
		FusionLevelText.text = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(CurrentNetCardInfo.Exp).ToString();
		UpdateStarImage(FusionStarImageRoots, FusionStarFrameImageRoots, CurrentNetCardInfo.Star);
		ShowReconfirmCount = 0;
		m_listNetCardInfo.Clear();
		List<CardInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].netCardInfo.CardSeqID != CurrentNetCardInfo.CardSeqID && list[i].netCardInfo.Protected == 0 && list[i].netCardInfo.Favorite == 0)
			{
				m_listNetCardInfo.Add(list[i].netCardInfo);
			}
		}
		m_dictSelectedCardInfo.Clear();
		m_FusionBtn.interactable = false;
		OnSortGo();
	}

	private void OnClickCard(int p_idx)
	{
		NetCardInfo netCardInfo = m_listNetCardInfoFiltered[p_idx];
		CARD_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netCardInfo.CardID, out value);
		if (value == null)
		{
			return;
		}
		bool flag = false;
		flag = value.n_RARITY >= 4;
		if (m_dictSelectedCardInfo.ContainsKey(netCardInfo.CardSeqID))
		{
			m_dictSelectedCardInfo.Remove(netCardInfo.CardSeqID);
			if (flag)
			{
				ShowReconfirmCount--;
			}
		}
		else
		{
			if (m_dictSelectedCardInfo.Values.Count >= OrangeConst.CARD_FUSION_LIMIT)
			{
				return;
			}
			m_dictSelectedCardInfo.Add(netCardInfo.CardSeqID, netCardInfo);
			if (flag)
			{
				ShowReconfirmCount++;
			}
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		m_FusionBtn.interactable = m_dictSelectedCardInfo.Count > 0;
		CardFusionCell value2;
		if (m_dictCardFusionCell.TryGetValue(p_idx, out value2))
		{
			value2.ToggleSelection();
		}
		UpdateFusionResult(0.3f);
	}

	private void UpdateFusionResult(float timer = 0f)
	{
		int exp = CurrentNetCardInfo.Exp;
		int num = CurrentNetCardInfo.Star;
		int num2 = 0;
		foreach (KeyValuePair<int, NetCardInfo> item in m_dictSelectedCardInfo)
		{
			NetCardInfo value = item.Value;
			CARD_TABLE value2 = null;
			ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(value.CardID, out value2);
			if (value2 != null)
			{
				int cardRank = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(value.Exp);
				num2 += value2.n_EXP * cardRank;
				if (value2.n_ID == CurrentCardTable.n_ID)
				{
					num++;
				}
			}
		}
		exp += num2;
		LeanTween.value(int.Parse(FusionExpText.text), num2, timer).setOnUpdate(delegate(float val)
		{
			FusionExpText.text = ((int)val).ToString();
		});
		FusionSelectedText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ALREADY_SELECTED_COUNT"), m_dictSelectedCardInfo.Values.Count, OrangeConst.CARD_FUSION_LIMIT);
		int cardRank2 = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(exp);
		LeanTween.value(int.Parse(FusionLevelText.text), cardRank2, timer).setOnUpdate(delegate(float val)
		{
			int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
			int num3 = (((int)val > lV) ? lV : ((int)val));
			OrangeText fusionLevelText = FusionLevelText;
			int num4 = num3;
			fusionLevelText.text = num4.ToString();
		});
		if (num > 5)
		{
			num = 5;
			bOverStarCount = true;
		}
		else
		{
			bOverStarCount = false;
		}
		UpdateStarImage(FusionStarImageRoots, FusionStarFrameImageRoots, num);
	}

	private void UpdateUpgradeResult(float timer = 0f)
	{
		int exp = CurrentNetCardInfo.Exp;
		int star = CurrentNetCardInfo.Star;
		exp += GetTotalAddExp();
		LeanTween.value(int.Parse(UpgradeExpText.text), GetTotalAddExp(), timer).setOnUpdate(delegate(float val)
		{
			UpgradeExpText.text = ((int)val).ToString();
		});
		int cardRank = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(exp);
		LeanTween.value(int.Parse(UpgradeLevelText.text), cardRank, timer).setOnUpdate(delegate(float val)
		{
			int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
			int num2 = (((int)val > lV) ? lV : ((int)val));
			OrangeText upgradeLevelText = UpgradeLevelText;
			int num3 = num2;
			upgradeLevelText.text = num3.ToString();
		});
		int num = 0;
		for (int i = 0; i < stariteminfos.Length; i++)
		{
			if (stariteminfos[i].tITEM_TABLE != null && stariteminfos[i].tITEM_TABLE.n_RARE >= CurrentCardTable.n_RARITY)
			{
				num += stariteminfos[i].nUseNum;
			}
		}
		star += num;
		if (star > 5)
		{
			star = 5;
			bOverStarCount = true;
		}
		else
		{
			bOverStarCount = false;
		}
		UpdateStarImage(UpgradeStarImageRoots, UpgradeStarFrameImageRoots, star);
	}

	public void SetCardIcon(CardFusionCell p_unit)
	{
		NetCardInfo netCardInfo = m_listNetCardInfoFiltered[p_unit.NowIdx];
		m_dictCardFusionCell.Remove(p_unit.NowIdx);
		m_dictCardFusionCell.Add(p_unit.NowIdx, p_unit);
		CARD_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netCardInfo.CardID, out value);
		if (value != null)
		{
			int cardRank = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(netCardInfo.Exp);
			p_unit.CardIcon.SetStarAndLv(netCardInfo.Star, cardRank);
			p_unit.CardIcon.SetRare(value.n_RARITY);
			p_unit.CardIcon.SetTypeImage(value.n_TYPE);
			p_unit.CardIcon.SetLockImage(netCardInfo.Protected == 1);
			p_unit.CardIcon.SetFavoriteImage(netCardInfo.Favorite == 1);
			string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
			string s_ICON = value.s_ICON;
			p_unit.CardIcon.Setup(p_unit.NowIdx, p_bundleName, s_ICON, OnClickCard);
			p_unit.SetCardID(netCardInfo.CardSeqID, netCardInfo.CardID);
			p_unit.SetSelection(false);
		}
		else
		{
			p_unit.CardIcon.Clear();
		}
	}

	public bool IsSelected(int CardSeqID)
	{
		return m_dictSelectedCardInfo.ContainsKey(CardSeqID);
	}

	public void ChangeCard(params object[] p_params)
	{
		if (p_params.Length < 1)
		{
			return;
		}
		int? num = p_params[0] as int?;
		if (num.HasValue && nTargetCardSeqID != num)
		{
			nTargetCardSeqID = num ?? 0;
			InitCardInfo();
			if (CurrentCardInfoType == CardInfoType.Fusion)
			{
				ResetScrollRect();
			}
			if (CurrentCardInfoType == CardInfoType.Upgrade)
			{
				ResetExpItems();
				InitExpItem();
				InitStarItem();
				InitQuickExpItem();
			}
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
		}
	}

	public void InitCardInfo()
	{
		CardInfo value = null;
		if (nTargetCardID > 0)
		{
			value = new CardInfo();
			value.netCardInfo = new NetCardInfo();
			value.netCardInfo.CardID = nTargetCardID;
		}
		else
		{
			ManagedSingleton<PlayerNetManager>.Instance.dicCard.TryGetValue(nTargetCardSeqID, out value);
		}
		if (value == null)
		{
			return;
		}
		int num = 99;
		if (CurrentNetCardInfo != null)
		{
			num = CurrentNetCardInfo.Star;
		}
		CurrentNetCardInfo = value.netCardInfo;
		if (CurrentNetCardInfo.Star > num)
		{
			playStarUp = true;
		}
		CurrentCardTable = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[CurrentNetCardInfo.CardID];
		int num2 = 65535;
		if (CurrentExpTable != null)
		{
			num2 = CurrentExpTable.n_ID;
		}
		CurrentExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(CurrentNetCardInfo.Exp);
		if (num2 < CurrentExpTable.n_ID)
		{
			useLVUpSE = true;
		}
		else
		{
			useLVUpSE = false;
		}
		CardBaseRoot.CardSetup(CurrentNetCardInfo);
		FusionCurrentLevelText.text = CurrentExpTable.n_ID.ToString();
		UpgradeCurrentLevelText.text = CurrentExpTable.n_ID.ToString();
		CardAtkText.text = "+" + (int)((float)CurrentExpTable.n_CARD_ATK * CurrentCardTable.f_PARAM_ATK * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
		CardHpText.text = "+" + (int)((float)CurrentExpTable.n_CARD_HP * CurrentCardTable.f_PARAM_HP * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
		CardDefText.text = "+" + (int)((float)CurrentExpTable.n_CARD_DEF * CurrentCardTable.f_PARAM_DEF * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
		int[] array = new int[6] { CurrentCardTable.n_SKILL1_RANK0, CurrentCardTable.n_SKILL1_RANK1, CurrentCardTable.n_SKILL1_RANK2, CurrentCardTable.n_SKILL1_RANK3, CurrentCardTable.n_SKILL1_RANK4, CurrentCardTable.n_SKILL1_RANK5 };
		int[] array2 = new int[6] { CurrentCardTable.n_SKILL2_RANK0, CurrentCardTable.n_SKILL2_RANK1, CurrentCardTable.n_SKILL2_RANK2, CurrentCardTable.n_SKILL2_RANK3, CurrentCardTable.n_SKILL2_RANK4, CurrentCardTable.n_SKILL2_RANK5 };
		if (CurrentNetCardInfo.Star > 5 || CurrentNetCardInfo.Star < 0)
		{
			CurrentNetCardInfo.Star = 0;
		}
		CardSkillRoot1.SetActive(false);
		CardSkillRoot2.SetActive(false);
		int num3 = array[CurrentNetCardInfo.Star];
		if (num3 != 0)
		{
			CardSkillRoot1.SetActive(true);
			CardSkillText1.gameObject.SetActive(false);
			for (int i = 0; i < CardSkillColorRoot1.Length; i++)
			{
				CardSkillColorRoot1[i].SetImage(-1);
			}
			SKILL_TABLE skillTbl2 = null;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(num3, out skillTbl2);
			if (skillTbl2 != null)
			{
				CardSkillNameText1.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl2.w_NAME);
				CardSkillRuleText1.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl2.w_TIP);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(skillTbl2.s_ICON), skillTbl2.s_ICON, delegate(Sprite asset)
				{
					if (asset != null)
					{
						CardSkillImage1.sprite = asset;
					}
					else
					{
						Debug.LogWarning("SkillButton.Setup: unable to load sprite " + skillTbl2.s_ICON);
					}
				});
			}
			if (CurrentCardTable.n_SKILL1_CHARAID != 0)
			{
				CHARACTER_TABLE value2 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(CurrentCardTable.n_SKILL1_CHARAID, out value2);
				if (value2 != null)
				{
					CardSkillText1.gameObject.SetActive(true);
					CardSkillText1.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(value2.w_NAME);
				}
			}
			else if (CurrentCardTable.s_SKILL1_COMBINATION != "null")
			{
				string[] array3 = CurrentCardTable.s_SKILL1_COMBINATION.Split(',');
				for (int j = 0; j < array3.Length; j++)
				{
					int image = int.Parse(array3[j]);
					CardSkillColorRoot1[j].SetImage(image);
				}
			}
		}
		num3 = array2[CurrentNetCardInfo.Star];
		if (num3 != 0)
		{
			CardSkillRoot2.SetActive(true);
			CardSkillText2.gameObject.SetActive(false);
			for (int k = 0; k < CardSkillColorRoot2.Length; k++)
			{
				CardSkillColorRoot2[k].SetImage(-1);
			}
			SKILL_TABLE skillTbl = null;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(num3, out skillTbl);
			if (skillTbl != null)
			{
				CardSkillNameText2.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl.w_NAME);
				CardSkillRuleText2.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl.w_TIP);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(skillTbl.s_ICON), skillTbl.s_ICON, delegate(Sprite asset)
				{
					if (asset != null)
					{
						CardSkillImage2.sprite = asset;
					}
					else
					{
						Debug.LogWarning("SkillButton.Setup: unable to load sprite " + skillTbl.s_ICON);
					}
				});
			}
			if (CurrentCardTable.n_SKILL2_CHARAID != 0)
			{
				CHARACTER_TABLE value3 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(CurrentCardTable.n_SKILL2_CHARAID, out value3);
				if (value3 != null)
				{
					CardSkillText2.gameObject.SetActive(true);
					CardSkillText2.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(value3.w_NAME);
				}
			}
			else if (CurrentCardTable.s_SKILL2_COMBINATION != "null")
			{
				string[] array4 = CurrentCardTable.s_SKILL2_COMBINATION.Split(',');
				for (int l = 0; l < array4.Length; l++)
				{
					int image2 = int.Parse(array4[l]);
					CardSkillColorRoot2[l].SetImage(image2);
				}
			}
		}
		BookBtn.gameObject.SetActive(!bFormGallery);
		CheckGalleryUnlock();
	}

	private void AddRtItem(ref List<WeaponInfoUI.expiteminfo> rtItems, ref WeaponInfoUI.expiteminfo tItem)
	{
		for (int i = 0; i < rtItems.Count; i++)
		{
			if (rtItems[i].tITEM_TABLE.n_ID == tItem.tITEM_TABLE.n_ID)
			{
				rtItems[i].nUseNum += tItem.nUseNum;
				return;
			}
		}
		rtItems.Add(tItem);
	}

	public void OnTabButtonClick(int typ)
	{
		if (CurrentTabButtonType != typ)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		}
		CurrentTabButtonType = typ;
		for (int i = 0; i < TabButtons.Length; i++)
		{
			bool interactable = i != typ;
			TabButtons[i].interactable = interactable;
		}
		RootInfo0.enabled = false;
		RootInfo1.enabled = false;
		RootInfo2.enabled = false;
		Color32[] array = new Color32[2]
		{
			new Color32(179, 224, 248, byte.MaxValue),
			new Color32(65, 65, 65, byte.MaxValue)
		};
		RootInfoText1.color = array[0];
		RootInfoText2.color = array[0];
		RootInfoText3.color = array[0];
		switch ((CardInfoType)(short)typ)
		{
		case CardInfoType.Info:
			CurrentCardInfoType = CardInfoType.Info;
			RootInfoText1.color = array[1];
			RootInfo0.enabled = true;
			UpgradeBtn.interactable = false;
			m_FusionBtn.interactable = false;
			break;
		case CardInfoType.Fusion:
			CurrentCardInfoType = CardInfoType.Fusion;
			RootInfoText2.color = array[1];
			RootInfo1.enabled = true;
			ResetScrollRect();
			UpgradeBtn.interactable = false;
			break;
		case CardInfoType.Upgrade:
			CurrentCardInfoType = CardInfoType.Upgrade;
			RootInfoText3.color = array[1];
			RootInfo2.enabled = true;
			ResetExpItems(false);
			InitExpItem();
			InitStarItem();
			InitQuickExpItem();
			m_FusionBtn.interactable = false;
			break;
		}
	}

	public void OnClickQuickBtn()
	{
		ResetExpItems(false);
		InitExpItem();
		InitStarItem();
		InitQuickExpItem();
		OnQuickExpGrade();
		QuickUpgradeRoot.enabled = true;
	}

	public void OnClickQuickBtnClose()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ResetExpItems(false);
		InitExpItem();
		InitStarItem();
		InitQuickExpItem();
		QuickUpgradeRoot.enabled = false;
	}

	public EXP_TABLE GetNowLvWithAddExp(int nAddExp)
	{
		int num = nAddExp;
		if (CurrentNetCardInfo != null)
		{
			num += CurrentNetCardInfo.Exp;
		}
		Dictionary<int, EXP_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.GetEnumerator();
		EXP_TABLE eXP_TABLE = null;
		while (enumerator.MoveNext())
		{
			if (num < enumerator.Current.Value.n_TOTAL_CARDEXP && enumerator.Current.Value.n_TOTAL_CARDEXP - num <= enumerator.Current.Value.n_CARDEXP)
			{
				eXP_TABLE = enumerator.Current.Value;
				break;
			}
		}
		if (eXP_TABLE == null)
		{
			eXP_TABLE = new EXP_TABLE();
		}
		return ManagedSingleton<OrangeTableHelper>.Instance.ReduceLVByCheckPlayerExp(ManagedSingleton<PlayerHelper>.Instance.GetExp(), eXP_TABLE);
	}

	public void OnAddQuickLV()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		if ((float)nQucikLV < expSlider.maxValue + (float)nowLvWithAddExp.n_ID)
		{
			PlayUISE(m_clickExpItem);
			nQucikLV++;
			InitQuickExpitem(nQucikLV);
		}
	}

	public void OnDecreaseQuickLV()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		if (nQucikLV > nowLvWithAddExp.n_ID)
		{
			PlayUISE(m_clickExpItem);
			nQucikLV--;
			InitQuickExpitem(nQucikLV);
		}
	}

	public void OnMaxQuickLV()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		if ((float)nQucikLV != expSlider.maxValue + (float)nowLvWithAddExp.n_ID)
		{
			PlayUISE(m_clickExpItem);
			nQucikLV = (int)expSlider.maxValue + nowLvWithAddExp.n_ID;
			InitQuickExpitem(nQucikLV);
		}
	}

	public void OnMinQuickLV()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		if (nQucikLV != nowLvWithAddExp.n_ID)
		{
			PlayUISE(m_clickExpItem);
			nQucikLV = nowLvWithAddExp.n_ID;
			InitQuickExpitem(nQucikLV);
		}
	}

	public void InitQuickExpitem(int nLV)
	{
		EXP_TABLE value = GetNowLvWithAddExp(0);
		int n_ID = value.n_ID;
		expSlider.value = nLV - n_ID;
		TargetLvText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TARGET_LV"), nLV - n_ID, expSlider.maxValue);
		ResetCardIconRoot(QuickIconRoot.transform, CurrentNetCardInfo.CardSeqID, nLV - n_ID + CurrentExpTable.n_ID);
		int nMaxExp = 0;
		if (nLV != n_ID && ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(nLV, out value))
		{
			nMaxExp = value.n_TOTAL_CARDEXP;
		}
		ReSetQuickItems(nMaxExp);
		InitQuickExpItem();
		int num = 0;
		for (int i = 0; i < expiteminfos.Length; i++)
		{
			if (expiteminfos[i].nUseNum > 0)
			{
				num += expiteminfos[i].nUseNum;
			}
		}
		if (num > 0)
		{
			QuickExpUpgradeGo.interactable = true;
		}
		else
		{
			QuickExpUpgradeGo.interactable = false;
		}
	}

	public int GetTotalAddExp()
	{
		int num = 0;
		for (int i = 0; i < expiteminfos.Length; i++)
		{
			if (expiteminfos[i].tITEM_TABLE != null)
			{
				num += expiteminfos[i].nUseNum * (int)expiteminfos[i].tITEM_TABLE.f_VALUE_X;
			}
		}
		return num;
	}

	public int GetTotalExpByPlayerLV()
	{
		EXP_TABLE expTable = ManagedSingleton<PlayerHelper>.Instance.GetExpTable();
		return expTable.n_TOTAL_CARDEXP - expTable.n_CARDEXP;
	}

	public int GetMaxLvExp()
	{
		return ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.OrderByDescending((KeyValuePair<int, EXP_TABLE> obj) => obj.Key).First().Value.n_TOTAL_CARDEXP;
	}

	public void OnSilderChange(float value)
	{
		EXP_TABLE value2 = GetNowLvWithAddExp(0);
		int n_ID = value2.n_ID;
		if (expSlider.value != (float)(nQucikLV - n_ID))
		{
			int num = (int)Mathf.Round(expSlider.value);
			if (expSlider.value == (float)num)
			{
				PlayUISE(m_clickExpItem);
			}
			expSlider.value = num;
			nQucikLV = num + n_ID;
			TargetLvText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TARGET_LV"), num, expSlider.maxValue);
			ResetCardIconRoot(QuickIconRoot.transform, CurrentNetCardInfo.CardSeqID, num + CurrentExpTable.n_ID);
			int nMaxExp = 0;
			if (num != 0 && ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(nQucikLV, out value2))
			{
				nMaxExp = value2.n_TOTAL_CARDEXP;
			}
			ReSetQuickItems(nMaxExp);
			InitQuickExpItem();
			if (GetTotalAddExp() > 0)
			{
				QuickExpUpgradeGo.interactable = true;
			}
			else
			{
				QuickExpUpgradeGo.interactable = false;
			}
		}
	}

	private void ReSetQuickItems(int nMaxExp)
	{
		int num = CurrentNetCardInfo.Exp;
		EXP_TABLE expTable = ManagedSingleton<PlayerHelper>.Instance.GetExpTable();
		for (int num2 = expiteminfos.Length - 1; num2 >= 0; num2--)
		{
			if (expiteminfos[num2].tITEM_TABLE != null)
			{
				int num3 = (int)Mathf.Floor((float)(nMaxExp - num) / expiteminfos[num2].tITEM_TABLE.f_VALUE_X);
				if (expiteminfos[num2].mHaveNum >= num3)
				{
					expiteminfos[num2].nUseNum = 0;
					while ((expiteminfos[num2].nUseNum + 1) * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X + num < nMaxExp && (expiteminfos[num2].nUseNum + 1) * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X + num < expTable.n_TOTAL_WEAPONEXP)
					{
						expiteminfos[num2].nUseNum++;
					}
					num += expiteminfos[num2].nUseNum * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X;
				}
				else
				{
					expiteminfos[num2].nUseNum = expiteminfos[num2].mHaveNum;
					num += expiteminfos[num2].nUseNum * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X;
				}
			}
		}
	}

	public void OnSelectCardFusionIcon(CardFusionCell icon)
	{
		icon.ToggleSelection();
	}

	public void OnExecuteCardFusion()
	{
		List<int> list = new List<int>();
		List<NetCardInfo> list2 = m_dictSelectedCardInfo.Values.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			list.Add(list2[i].CardSeqID);
		}
		ManagedSingleton<PlayerNetManager>.Instance.CardFusionReq(CurrentNetCardInfo.CardSeqID, list, delegate
		{
			CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
			if (uI != null)
			{
				uI.Setup(false, true);
			}
			Setup();
			ResetScrollRect();
			if (useLVUpSE)
			{
				PlayUISE(m_addLVUp);
				PlayLevelUp3DEffect();
				if (playStarUp)
				{
					PlayStarUpEffect();
					PlayStarUpEffectCB();
					playStarUp = false;
				}
				useLVUpSE = false;
			}
			else
			{
				PlayUISE(m_addLVExp);
				PlayUpgrade3DEffect();
				if (playStarUp)
				{
					PlayStarUpEffect();
					PlayStarUpEffectCB();
					playStarUp = false;
				}
			}
		});
	}

	public void OnFusionBtn()
	{
		List<int> list = new List<int>();
		List<NetCardInfo> list2 = m_dictSelectedCardInfo.Values.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			list.Add(list2[i].CardSeqID);
		}
		if (ShowReconfirmCount > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardSellConfirm", delegate(CardSellConfirmUI ui)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_POWERUP_CONFIRM");
				string str2 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_POWERUP_RARITY_CONFIRM");
				ui.Setup(m_dictSelectedCardInfo.Values.ToList(), str, str2, false, FusionCheckStarCount);
			});
		}
		else
		{
			FusionCheckStarCount();
		}
	}

	private void FusionCheckStarCount()
	{
		if (bOverStarCount)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_STAR_MAX"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					OnExecuteCardFusion();
				});
			});
		}
		else
		{
			OnExecuteCardFusion();
		}
	}

	public void OnCancelSelectedBtn()
	{
		if (m_dictSelectedCardInfo.Count != 0)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
		}
		ResetScrollRect();
	}

	public void OnProtectBtn()
	{
		if (CurrentNetCardInfo.Protected == 1)
		{
			OnSetUnprotectBtn();
		}
		else
		{
			OnSetProtectBtn();
		}
	}

	public void OnSetProtectBtn()
	{
		ManagedSingleton<PlayerNetManager>.Instance.ProtectedCardReq(CurrentNetCardInfo.CardSeqID, 1, delegate
		{
			CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
			if (uI != null)
			{
				uI.Setup(false, true);
			}
			listHasCards = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
			InitCardInfo();
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		});
	}

	public void OnSetUnprotectBtn()
	{
		ManagedSingleton<PlayerNetManager>.Instance.ProtectedCardReq(CurrentNetCardInfo.CardSeqID, 0, delegate
		{
			CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
			if (uI != null)
			{
				uI.Setup(false, true);
			}
			listHasCards = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
			InitCardInfo();
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
		});
	}

	private void UpdateItemNeedInfo(ITEM_TABLE tITEM_TABLE, StageLoadIcon img, StageLoadIcon frm, StageLoadIcon bg, Text text)
	{
		img.CheckLoad(AssetBundleScriptableObject.Instance.GetIconItem(tITEM_TABLE.s_ICON), tITEM_TABLE.s_ICON);
		OrangeRareText.Rare n_RARE = (OrangeRareText.Rare)tITEM_TABLE.n_RARE;
		frm.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall((int)n_RARE));
		bg.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareBgSmall((int)n_RARE));
		if (text != null)
		{
			text.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(tITEM_TABLE.w_NAME);
		}
	}

	public void OnQuickExpGrade()
	{
		nQucikLV = CurrentExpTable.n_ID;
		EXP_TABLE currentExpTable = CurrentExpTable;
		int n_ID = currentExpTable.n_ID;
		currentExpTable = ManagedSingleton<PlayerHelper>.Instance.GetExpTable();
		int n_ID2 = currentExpTable.n_ID;
		int maxLvExp = GetMaxLvExp();
		QuickExpUpgradeGo.interactable = false;
		int num = CurrentNetCardInfo.Exp;
		for (int num2 = expiteminfos.Length - 1; num2 >= 0; num2--)
		{
			if (expiteminfos[num2].tITEM_TABLE != null)
			{
				int num3 = (int)Mathf.Floor((float)(maxLvExp - num) / expiteminfos[num2].tITEM_TABLE.f_VALUE_X);
				if (num3 < expiteminfos[num2].mHaveNum)
				{
					num += num3 * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X;
					break;
				}
				num += expiteminfos[num2].mHaveNum * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X;
			}
		}
		while (num < currentExpTable.n_TOTAL_CARDEXP - currentExpTable.n_CARDEXP && currentExpTable.n_ID != n_ID)
		{
			currentExpTable = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[currentExpTable.n_ID - 1];
		}
		expSlider.minValue = 0f;
		expSlider.maxValue = currentExpTable.n_ID - n_ID;
		InitQuickExpitem(nQucikLV);
		string[] array = new string[7] { "Dummy", "D", "C", "B", "A", "S", "SS" };
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
	}

	public void OnClickInfoBtnCB(int nBtnID)
	{
		if (nNextInfoIndex == -1)
		{
			PlayUISE(m_clickTab);
		}
		OnInfoBtnCB(nBtnID);
	}

	public void OnInfoBtnCB(int nBtnID)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (nBtnID != i)
			{
				buttons[i].interactable = true;
			}
			else
			{
				buttons[i].interactable = false;
			}
		}
		if (nNextInfoIndex == -1)
		{
			nNextInfoIndex = nBtnID;
			StartCoroutine(StageResManager.TweenFloatCoroutine(InfoCanvasGroups[nNowInfoIndex].alpha, 0f, 0.2f, delegate(float f)
			{
				InfoCanvasGroups[nNowInfoIndex].alpha = f;
			}, delegate
			{
				nNowInfoIndex = nNextInfoIndex;
				InfoCanvasGroups[nNowInfoIndex].alpha = 0f;
				nNextInfoIndex = -1;
				StartCoroutine(StageResManager.TweenFloatCoroutine(InfoCanvasGroups[nNowInfoIndex].alpha, 1f, 0.2f, delegate(float f)
				{
					InfoCanvasGroups[nNowInfoIndex].alpha = f;
				}, null));
			}));
		}
		else
		{
			nNextInfoIndex = nBtnID;
		}
	}

	public void OnQuickExpItemAddBtnCB(int nBtnID)
	{
		ITEM_TABLE item = null;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(expiteminfos[nBtnID].tITEM_TABLE.n_ID, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.Setup(item);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				InitQuickExpItem();
			});
		});
	}

	public void OnExpItemAddBtnCB(int nBtnID)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(expiteminfos[nBtnID].tITEM_TABLE.n_ID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item);
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
				});
			});
		}
	}

	public void OnStarItemAddBtnCB(int nBtnID)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(stariteminfos[nBtnID].tITEM_TABLE.n_ID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item);
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
				});
			});
		}
	}

	public void OnExpItemUnuseBtnCB(int nBtnID)
	{
		expiteminfos[nBtnID].nUseNum = 0;
		ResetExpItems();
		UpdateUpgradeResult(0.3f);
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
	}

	public void OnStarItemUnuseBtnCB(int nBtnID)
	{
		stariteminfos[nBtnID].nUseNum = 0;
		ResetExpItems();
		UpdateUpgradeResult(0.3f);
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
	}

	public void OnExpItemBtnCB(int nBtnID)
	{
		bool flag = false;
		int num = GetTotalAddExp();
		if (CurrentNetCardInfo.Exp + num >= GetTotalExpByPlayerLV() && CurrentNetCardInfo.Exp + num + (int)expiteminfos[nBtnID].tITEM_TABLE.f_VALUE_X >= GetTotalExpByPlayerLV())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_CARD_LV"), true);
			});
			return;
		}
		expiteminfos[nBtnID].nUseNum++;
		if (expiteminfos[nBtnID].nUseNum > expiteminfos[nBtnID].mHaveNum)
		{
			expiteminfos[nBtnID].nUseNum = expiteminfos[nBtnID].mHaveNum;
		}
		else
		{
			num += (int)expiteminfos[nBtnID].tITEM_TABLE.f_VALUE_X;
			flag = true;
		}
		if (flag)
		{
			PlayUISE(m_clickExpItem);
		}
		ResetExpItems();
		if (num > 0)
		{
			UpgradeBtn.interactable = true;
		}
		else
		{
			UpgradeBtn.interactable = false;
		}
		UpdateUpgradeResult(0.3f);
	}

	public void OnStarItemBtnCB(int nBtnID)
	{
		bool flag = false;
		int num = 0;
		for (int i = 0; i < stariteminfos.Length; i++)
		{
			num += stariteminfos[i].nUseNum;
		}
		if (CurrentNetCardInfo.Star + num >= 5)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_STAR_MAX_TIP"), true);
			});
			return;
		}
		stariteminfos[nBtnID].nUseNum++;
		if (stariteminfos[nBtnID].nUseNum > stariteminfos[nBtnID].mHaveNum)
		{
			stariteminfos[nBtnID].nUseNum = stariteminfos[nBtnID].mHaveNum;
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			PlayUISE(m_clickExpItem);
		}
		ResetExpItems();
		UpdateUpgradeResult(0.3f);
	}

	public void OnUpgradeBtn()
	{
		if (bOverStarCount)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_STAR_MAX"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					ExecuteUpgrade();
				});
			});
		}
		else
		{
			ExecuteUpgrade();
		}
	}

	public void ExecuteUpgrade()
	{
		UpgradeBtn.interactable = false;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		List<ItemConsumptionInfo> list = new List<ItemConsumptionInfo>();
		for (int i = 0; i < expiteminfos.Length; i++)
		{
			if (expiteminfos[i].nUseNum > 0 && expiteminfos[i].tITEM_TABLE != null)
			{
				ItemConsumptionInfo itemConsumptionInfo = new ItemConsumptionInfo();
				itemConsumptionInfo.Amount = expiteminfos[i].nUseNum;
				itemConsumptionInfo.ItemID = expiteminfos[i].tITEM_TABLE.n_ID;
				list.Add(itemConsumptionInfo);
			}
		}
		for (int j = 0; j < stariteminfos.Length; j++)
		{
			if (stariteminfos[j].nUseNum > 0 && stariteminfos[j].tITEM_TABLE != null)
			{
				ItemConsumptionInfo itemConsumptionInfo2 = new ItemConsumptionInfo();
				itemConsumptionInfo2.Amount = stariteminfos[j].nUseNum;
				itemConsumptionInfo2.ItemID = stariteminfos[j].tITEM_TABLE.n_ID;
				list.Add(itemConsumptionInfo2);
			}
		}
		ManagedSingleton<PlayerNetManager>.Instance.CardEnhanceByItemReq(CurrentNetCardInfo.CardSeqID, list, delegate
		{
			CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
			if (uI != null)
			{
				uI.Setup(false, true);
			}
			Setup();
			InitExpItem();
			InitStarItem();
			ResetExpItems();
			if (useLVUpSE)
			{
				PlayUISE(m_addLVUp);
				PlayLevelUp3DEffect();
				if (playStarUp)
				{
					PlayStarUpEffect();
					PlayStarUpEffectCB();
					playStarUp = false;
				}
				useLVUpSE = false;
			}
			else
			{
				PlayUISE(m_addLVExp);
				PlayUpgrade3DEffect();
				if (playStarUp)
				{
					PlayStarUpEffect();
					PlayStarUpEffectCB();
					playStarUp = false;
				}
			}
		});
	}

	public void OnQuickUpgradeBtn()
	{
		QuickUpgradeRoot.enabled = false;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		List<ItemConsumptionInfo> list = new List<ItemConsumptionInfo>();
		for (int i = 0; i < expiteminfos.Length; i++)
		{
			if (expiteminfos[i].nUseNum > 0 && expiteminfos[i].tITEM_TABLE != null)
			{
				ItemConsumptionInfo itemConsumptionInfo = new ItemConsumptionInfo();
				itemConsumptionInfo.Amount = expiteminfos[i].nUseNum;
				itemConsumptionInfo.ItemID = expiteminfos[i].tITEM_TABLE.n_ID;
				list.Add(itemConsumptionInfo);
			}
		}
		ManagedSingleton<PlayerNetManager>.Instance.CardEnhanceByItemReq(CurrentNetCardInfo.CardSeqID, list, delegate
		{
			CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
			if (uI != null)
			{
				uI.Setup(false, true);
			}
			Setup();
			InitExpItem();
			InitStarItem();
			ResetExpItems();
			if (useLVUpSE)
			{
				PlayUISE(m_addLVUp);
				PlayLevelUp3DEffect();
				useLVUpSE = false;
			}
			else
			{
				PlayUISE(m_addLVExp);
				PlayUpgrade3DEffect();
			}
		});
	}

	private void ResetCardIconRoot(UnityEngine.Transform ob, int SeqID, int lv)
	{
		int childCount = ob.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(ob.transform.GetChild(i).gameObject);
		}
		CardIcon componentInChildren = UnityEngine.Object.Instantiate(refPrefabSmall, ob).GetComponentInChildren<CardIcon>();
		componentInChildren.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		componentInChildren.gameObject.SetActive(true);
		ob.gameObject.SetActive(true);
		CardInfo value = null;
		ManagedSingleton<PlayerNetManager>.Instance.dicCard.TryGetValue(SeqID, out value);
		if (value != null)
		{
			CARD_TABLE value2 = null;
			ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(value.netCardInfo.CardID, out value2);
			if (value2 != null)
			{
				string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value2.n_PATCH);
				string s_ICON = value2.s_ICON;
				componentInChildren.Setup(0, p_bundleName, s_ICON);
				componentInChildren.CardSetup(SeqID);
				componentInChildren.SetLv(lv);
			}
		}
	}

	public void OnClickSortPanelBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		tmpCardSortType = ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType;
		tmpCardSortKey = ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey;
		SortRoot.SetActive(true);
		MaskImage.gameObject.SetActive(true);
	}

	public void OnCloseSortRoot()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType = tmpCardSortType;
		ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey = tmpCardSortKey;
		UpdateButtonType();
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
	}

	public void OnClickSortGo()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ResetScrollRect();
	}

	public void OnSortGo()
	{
		UpdateCheckCardUesdData();
		m_listNetCardInfoFiltered.Clear();
		m_dictCardFusionCell.Clear();
		foreach (NetCardInfo item in m_listNetCardInfo)
		{
			CARD_TABLE value = null;
			ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(item.CardID, out value);
			if (value != null && !CheckCardUesd(item.CardSeqID) && ((uint)ManagedSingleton<EquipHelper>.Instance.nCardInfoSortType & (uint)value.n_TYPE) == (uint)value.n_TYPE)
			{
				m_listNetCardInfoFiltered.Add(item);
			}
		}
		m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardID.CompareTo(y.CardID));
		if ((ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY) == EquipHelper.CARD_SORT_KEY.CARD_SORT_RARITY)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				CARD_TABLE value2 = null;
				CARD_TABLE value3 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(x.CardID, out value2);
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(y.CardID, out value3);
				if (value2 == null || value3 == null)
				{
					return 0;
				}
				int num3 = value2.n_RARITY.CompareTo(value3.n_RARITY);
				if (num3 == 0)
				{
					num3 = x.CardID.CompareTo(y.CardID);
				}
				return num3;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR) == EquipHelper.CARD_SORT_KEY.CARD_SORT_STAR)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num2 = x.Star.CompareTo(y.Star);
				if (num2 == 0)
				{
					num2 = x.CardID.CompareTo(y.CardID);
				}
				return num2;
			});
		}
		else if ((ManagedSingleton<EquipHelper>.Instance.nCardInfoSortKey & EquipHelper.CARD_SORT_KEY.CARD_SORT_LV) == EquipHelper.CARD_SORT_KEY.CARD_SORT_LV)
		{
			m_listNetCardInfoFiltered.Sort(delegate(NetCardInfo x, NetCardInfo y)
			{
				int num = x.Exp.CompareTo(y.Exp);
				if (num == 0)
				{
					num = x.CardID.CompareTo(y.CardID);
				}
				return num;
			});
		}
		else
		{
			m_listNetCardInfoFiltered.Sort((NetCardInfo x, NetCardInfo y) => x.CardSeqID.CompareTo(y.CardSeqID));
		}
		m_listNetCardInfoFiltered.Reverse();
		m_ScrollRect.ClearCells();
		m_ScrollRect.OrangeInit(m_CardFusionCell, m_listNetCardInfoFiltered.Count, m_listNetCardInfoFiltered.Count);
		SortRoot.SetActive(false);
		MaskImage.gameObject.SetActive(false);
	}

	private void UpdateCheckCardUesdData()
	{
		List<int> list = ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.Keys.ToList();
		tCharacterCardSlotInfoList.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			int key = list[i];
			Dictionary<int, NetCharacterCardSlotInfo> value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.TryGetValue(key, out value))
			{
				tCharacterCardSlotInfoList.AddRange(value.Values.ToList());
			}
		}
	}

	private bool CheckCardUesd(int SeqID)
	{
		for (int i = 0; i < tCharacterCardSlotInfoList.Count; i++)
		{
			if (tCharacterCardSlotInfoList[i].CardSeqID == SeqID)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnClickCloseBtn()
	{
		CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
		if (uI != null)
		{
			uI.Setup(false, true);
		}
		CharacterInfoCard uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoCard>("UI_CharacterInfo_Card");
		if (uI2 != null)
		{
			uI2.OnCardInfoUIClose();
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base.OnClickCloseBtn();
	}

	private void PlayStarUpEffect()
	{
		if (CurrentNetCardInfo.Star <= 0)
		{
			return;
		}
		if (m_starUpEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "starupeffect", "StarUpEffect", delegate(GameObject asset)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(asset, base.transform);
				m_starUpEffect = gameObject2.GetComponent<StarUpEffect>();
				m_starUpEffect.Play(starFrameObjectArray1[CurrentNetCardInfo.Star - 1].transform.position);
			});
		}
		else
		{
			m_starUpEffect.Play(starFrameObjectArray1[CurrentNetCardInfo.Star - 1].transform.position);
		}
		if (m_starUpEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "starupeffect", "StarUpEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_starUpEffect = gameObject.GetComponent<StarUpEffect>();
				m_starUpEffect.Play(starFrameObjectArray2[CurrentNetCardInfo.Star - 1].transform.position);
			});
		}
		else
		{
			m_starUpEffect.Play(starFrameObjectArray2[CurrentNetCardInfo.Star - 1].transform.position);
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_STAMP02);
	}

	private void PlayStarUpEffectCB()
	{
		if (CurrentNetCardInfo.Star <= 0)
		{
			return;
		}
		if (m_starUpEffectCB == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "starupeffect", "StarUpEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_starUpEffectCB = gameObject.GetComponent<StarUpEffect>();
				m_starUpEffectCB.Play(starFrameObjectArrayCB[CurrentNetCardInfo.Star - 1].transform.position);
			});
		}
		else
		{
			m_starUpEffectCB.Play(starFrameObjectArrayCB[CurrentNetCardInfo.Star - 1].transform.position);
		}
	}

	public bool IsEffectPlaying()
	{
		bool flag = false;
		flag |= bEffectLock;
		if (m_starUpEffectCB != null)
		{
			flag |= m_starUpEffectCB.gameObject.activeSelf;
		}
		if (m_starUpEffect != null)
		{
			flag |= m_starUpEffect.gameObject.activeSelf;
		}
		if (m_upgradeEffect != null)
		{
			flag |= m_upgradeEffect.gameObject.activeSelf;
		}
		if (m_levelUpEffect != null)
		{
			flag |= m_levelUpEffect.activeSelf;
		}
		if (m_levelUpWordEffect != null)
		{
			flag |= m_levelUpWordEffect.activeSelf;
		}
		return flag;
	}

	public void PlayLevelUp3DEffect()
	{
		if (!(m_levelUpEffect != null) || !(m_levelUpWordEffect != null))
		{
			return;
		}
		m_levelUpEffect.SetActive(true);
		m_levelUpWordEffect.SetActive(true);
		UnityArmatureComponent component = m_levelUpEffect.GetComponent<UnityArmatureComponent>();
		UnityArmatureComponent component2 = m_levelUpWordEffect.GetComponent<UnityArmatureComponent>();
		component.animation.Reset();
		component.animation.Play("newAnimation", 1);
		component2.animation.Reset();
		component2.animation.Play("newAnimation", 1);
		LeanTween.delayedCall(component.animation.GetState("newAnimation").totalTime, (Action)delegate
		{
			if ((bool)m_levelUpEffect)
			{
				m_levelUpEffect.SetActive(false);
			}
		});
		LeanTween.delayedCall(component2.animation.GetState("newAnimation").totalTime, (Action)delegate
		{
			if ((bool)m_levelUpWordEffect)
			{
				m_levelUpWordEffect.SetActive(false);
			}
		});
	}

	public void PlayUpgrade3DEffect()
	{
		if (m_upgrade3DEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgrade3deffect", "Upgrade3DEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_upgrade3DEffect = gameObject.GetComponent<Upgrade3DEffect>();
				m_upgrade3DEffect.Play(CardBaseRoot.transform.position + m_effectOffset);
			});
		}
		else
		{
			m_upgrade3DEffect.Play(CardBaseRoot.transform.position + m_effectOffset);
		}
	}

	public void OnClickBookBtn()
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
				if (ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(tbl.n_ID, CurrentNetCardInfo.CardID))
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
				List<NetGalleryMainIdInfo> list = new List<NetGalleryMainIdInfo>();
				NetGalleryMainIdInfo netGalleryMainIdInfo = new NetGalleryMainIdInfo();
				netGalleryMainIdInfo.GalleryMainID = CurrentNetCardInfo.CardID;
				netGalleryMainIdInfo.GalleryIDList = galleryIDs;
				list.Add(netGalleryMainIdInfo);
				ManagedSingleton<PlayerNetManager>.Instance.GalleryCardUnlockReq(list, delegate
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

	private IEnumerator WaitEffectPlayEnd()
	{
		while (BookUPEffect.gameObject.activeSelf)
		{
			yield return new WaitForSeconds(0.2f);
		}
		CheckGalleryUnlock();
	}

	private void CheckGalleryUnlock()
	{
		if (CurrentNetCardInfo == null || nTargetCardSeqID == 0 || bFormGallery)
		{
			BookBtn.gameObject.SetActive(false);
			return;
		}
		int targetID = CurrentNetCardInfo.CardID;
		ManagedSingleton<GalleryHelper>.Instance.GalleryGetCardTableAll(targetID, out listGalleryInfo, out listGalleryUnlock, out listGalleryLock);
		bool active = listGalleryLock.Any((GALLERY_TABLE p) => ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(p.n_ID, targetID));
		m_bookFrame.gameObject.SetActive(active);
		float num = 194f;
		GalleryCalcResult galleryCalcResult = ManagedSingleton<GalleryHelper>.Instance.GalleryCalculationProgress(targetID, GalleryType.Card);
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

	public void OnClickNextCard()
	{
		if (tCardMainUI != null && CurrentNetCardInfoFilteredIndex + 1 < tCardMainUI.m_listNetCardInfoFiltered.Count)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
			nTargetCardSeqID = tCardMainUI.m_listNetCardInfoFiltered[CurrentNetCardInfoFilteredIndex + 1].CardSeqID;
			Setup();
		}
	}

	public void OnClickPreviousCard()
	{
		if (tCardMainUI != null && CurrentNetCardInfoFilteredIndex > 0)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
			nTargetCardSeqID = tCardMainUI.m_listNetCardInfoFiltered[CurrentNetCardInfoFilteredIndex - 1].CardSeqID;
			Setup();
		}
	}

	protected override void DoCustomEscapeEvent()
	{
		if (SortRoot.activeSelf)
		{
			OnCloseSortRoot();
		}
		else
		{
			OnClickCloseBtn();
		}
	}
}
