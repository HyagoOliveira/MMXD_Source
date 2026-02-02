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

public class WeaponInfoUI : OrangeUIBase
{
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

	[Serializable]
	public class expiteminfo
	{
		public int mHaveNum;

		public int nUseNum;

		public ITEM_TABLE tITEM_TABLE;
	}

	private enum WEAPONINFO_ATTR
	{
		WEAPONINFO_ATK = 0,
		WEAPONINFO_HP = 1,
		WEAPONINFO_CRI = 2,
		WEAPONINFO_HIT = 3,
		WEAPONINFO_ATKSPEED = 4,
		WEAPONINFO_ATKRANGE = 5,
		WEAPONINFO_ENERGY = 6,
		WEAPONINFO_MOVESPEED = 7,
		WEAPONINFO_RECHARGETIME = 8,
		WEAPONINFO_DEEP_RECORD_BATTLE = 9,
		WEAPONINFO_DEEP_RECORD_EXPLORE = 10,
		WEAPONINFO_DEEP_RECORD_ACTION = 11
	}

	[Header("global")]
	public GameObject refWeaponIconBase;

	public GameObject refWeaponIconBaseBig;

	public int nTargetWeaponID = 3;

	private int nNowWeaponID = -1;

	private WEAPON_TABLE tWEAPON_TABLE;

	private WeaponInfo tWeaponInfo;

	private UPGRADE_TABLE tUPGRADE_TABLE;

	private STAR_TABLE tSTAR_TABLE;

	private SKILL_TABLE tSKILL_TABLE;

	private int nNowExp;

	private int nNeedExp;

	private int nExpNowLV;

	private int nNowStar;

	private int nHP;

	private int nATK;

	private int nCRI;

	private int nHIT;

	private int nLuk;

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

	private Dictionary<GameObject, BtnClickCB> BtnClickCBs = new Dictionary<GameObject, BtnClickCB>();

	private bool useLVUpSE;

	public Button[] buttons;

	public GameObject[] infos;

	private CanvasGroup[] InfoCanvasGroups;

	public RawImage WeaponModel;

	private Canvas weaponModelCanvas;

	public GameObject StarRoot0;

	public GameObject[] TopNoStar;

	public GameObject[] TopOnStar;

	public GameObject[] TopAddStar;

	public Text[] s_NAME;

	public Text sBattleScore;

	public LoopHorizontalScrollRect tLHSR;

	public GameObject EquipRoot;

	public GameObject detailpopop;

	public GameObject takeoutroot;

	public Image[] takeouttypemask;

	public ExpButtonRef[] takeoutitems;

	public Text[] takeoutlbl;

	public Button TakeOutBtn;

	public GameObject[] Ctrlbtns;

	private CommonIconBase mainweaponic;

	private CommonIconBase subweaponic;

	private StageLoadIcon MainBG0;

	private int nTakeOutType;

	public Text DebugInfoText;

	public Text[] BeforePower;

	public Text[] AfterPower;

	public Text[] beforevalue;

	public Text[] aftervalue;

	private Color valuegreen = new Color(0.03137255f, 0.827451f, 0.007843138f);

	private Coroutine tObjScaleCoroutine;

	[Header("info0")]
	public Text[] AttributrText;

	public FillSliceImg[] AttributrBar;

	public Text n_WEAPON_SKILL;

	public Text n_WEAPON_Name;

	public GameObject info0skillimg;

	public StageLoadIcon info0needimg;

	public StageLoadIcon info0needbg;

	public StageLoadIcon info0needfrm;

	public Text neednameinfo0;

	public Image needbar1;

	public Text needtext1;

	public Button UnLockBtn;

	public GameObject needrootinfo0;

	public Button BookBtn;

	public GameObject BookFrame;

	public RectTransform BookBar;

	public Image BookBarImg;

	public OrangeText BookText;

	public UnityArmatureComponent BookUPEffect;

	public Image[] m_colorBar;

	[Header("info1")]
	public ExpButtonRef[] expitems;

	public ExpButtonRef[] expqucilitems;

	private Color disablecolor = new Color(0.39f, 0.39f, 0.39f);

	private expiteminfo[] expiteminfos;

	private int nTotalExpItemAddExp;

	public Text explabel;

	public Text explv;

	public Image[] expbarsub;

	public GameObject quickupgradeinfo;

	public Button QuickExpUpgradeGo;

	public Button upgradeBtn;

	public Button quickupgradeBtn;

	public Text TargetLvText;

	public Slider expSlider;

	public StageLoadIcon weaponicon;

	public StageLoadIcon weaponiconfrm;

	public StageLoadIcon weaponiconImg;

	private int nQucikLV;

	public GameObject objRecordBattle;

	public GameObject objRecordExplore;

	public GameObject objRecordAction;

	[Header("info2")]
	public StageLoadIcon pnowlvbg;

	public StageLoadIcon pnextlvbg;

	public Text pnowlv;

	public Text pnextlv;

	public Image[] NextArrow;

	public Text[] info2havenum;

	public Text[] info2usenum;

	public Button AdvancedBtn;

	public Text info2condition;

	public Button[] AttriBtn;

	public GameObject[] AttriSBG;

	[HideInInspector]
	public GameObject[][] info2attrbargreen;

	[HideInInspector]
	public GameObject[][] info2attrbarpurple;

	public GameObject[] info2attrarrow;

	public GameObject[] info2nsbg;

	public Text AttrNowText;

	public Text BeforeAttr0;

	public Text AfterAttr0;

	public GameObject info2weapon;

	public GameObject ProfChangeRoot;

	public Button ChangeProfBtn;

	public Slider ProfSlider;

	public Text[] beforepcard;

	public Text[] afterpcard;

	public Text[] beforeprof;

	public Text[] afterprof;

	public Text TargetProfText;

	public GameObject AutoProfChangeRoot;

	public Text AutoChangeProfText;

	private CommonIconBase info2CommonIconBase;

	private int nNowUpgradeType;

	private int nChangeProf;

	private bool bHasNextLV;

	private bool bIgnoreFristSE = true;

	[Header("info2 QuickProfUP")]
	public GameObject QuickProfUp;

	public Button GoQuickProfUp;

	public Button[] AttriQBtn;

	public StageLoadIcon[] AttriQLvBg;

	public Text[] AttriQLvText;

	public GameObject[] AttriQBg;

	public Text qProftitlemsg;

	public Slider ProfLvSlider;

	public GameObject[] AttriQSBG;

	public StageLoadIcon qpnowlvbg;

	public StageLoadIcon qpnextlvbg;

	public Text qpnowlv;

	public Text qpnextlv;

	public Text info2qcondition;

	public Text QAttrNowText;

	public Text[] info2numtext;

	private bool bSliderSE = true;

	[Header("info3")]
	public GameObject[] Star;

	public GameObject[] NoStar;

	public GameObject[] AddStar;

	public Button StarButton;

	public GameObject needrootinfo3;

	public StageLoadIcon info3skillimg;

	public StageLoadIcon info3msgimg;

	public StageLoadIcon info3needimg;

	public StageLoadIcon info3needbg;

	public StageLoadIcon info3needfrm;

	public Text info3skillname;

	public Image needbar2;

	public Text needtext2;

	public Text neednameinfo3;

	public GameObject StartRedDot;

	[Header("info4")]
	public Text skillname;

	public Text skilldesc;

	public StageLoadIcon skillImage;

	public Text lvcondition;

	public Text CostLabel0;

	public GameObject SkillLvUpRoot;

	public GameObject UnLockRoot;

	public GameObject maxlvroot;

	public UnlockButtonRef[] skillitems;

	public ExpButtonRef[] skillmaterials;

	public Button UnLockBtninfo4;

	public Button SkillLvUpBtninfo4;

	public Button QSkillLvUpBtninfo4;

	public Text[] info4havenum;

	public Text[] info4usenum;

	private int nNowSelectAutoSkill;

	private int nNowSelectSkillIndex;

	[Header("info4 RSkill")]
	public UnlockButtonRef[] rskillitems;

	public GameObject RSkillRoot;

	public GameObject SdSkillRoot;

	public GameObject ShowRSkillRoot;

	public LoopVerticalScrollRect tInfo4LVSR;

	public Button ShowRSKills;

	public Button ReChangeRSkillBtn;

	public Button QuickReChangeRSkill;

	public Button QuickUnLockRSkill;

	public Button NoQuickItemBtn;

	public Button UnLockRSkill;

	public GameObject QChangeRSkillRoot;

	public Text QChangeRSkillText;

	public ExpButtonRef[] QChangeRSkillItem;

	public GameObject ReChangeRSkillRoot;

	public StageLoadIcon[] RSkillImg;

	public Text[] RSkillText;

	public Text[] RSkillMsgText;

	public Text ChangeDesc;

	public GameObject CheckReChangeRSkillRoot;

	public ExpButtonRef[] checkskillmaterials;

	private bool bUseSpecialItem;

	[Header("info4 QSkillUp")]
	public GameObject QuickSkillUp;

	public Button GoQuickSkillUp;

	public Slider SkillLvSlider;

	public Text qlvtitlemsg;

	public StageLoadIcon[] QSkillImage;

	public Text QSkillText;

	public Text QSkillMsgText;

	public Text QSkillScribt;

	public Text qlvcondition;

	public Text[] info4numtext;

	[Header("info5")]
	public Text[] chipvalue;

	public FillSliceImg[] chipBar;

	public Text chipname;

	public GameObject chiproot0;

	public Text chipskillname;

	public Text chipskilldesc;

	public StageLoadIcon chipskillimg;

	public Button changechipbtn;

	public Text changechipbtntext;

	public GameObject ChipSelectRoot;

	public LoopVerticalScrollRect tInfo5LVSR;

	public Text[] ChipSelectHint;

	[HideInInspector]
	public int nTmpChipSet;

	[HideInInspector]
	public WeaponChipSelectColume.PerWeaponChipSelectCell refPerWeaponChipSelectCell;

	[HideInInspector]
	public List<int> listHasWeapons = new List<int>();

	[HideInInspector]
	public List<int> listFragWeapons = new List<int>();

	[HideInInspector]
	public bool bNeedInitList;

	[HideInInspector]
	public bool bUseGoCheckUISort;

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryInfo = new List<GALLERY_TABLE>();

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryUnlock = new List<GALLERY_TABLE>();

	[HideInInspector]
	public List<GALLERY_TABLE> listGalleryLock = new List<GALLERY_TABLE>();

	private RenderTextureObj textureObj;

	private StarUpEffect m_starUpEffect;

	private UpgradeEffect m_upgradeEffect;

	private Upgrade3DEffect m_upgrade3DEffect;

	private GameObject m_unlockEffect;

	private float m_unlockEffectLength;

	private GameObject m_levelUpEffect;

	private GameObject m_levelUpWordEffect;

	private Vector3 m_effectOffset = new Vector3(0f, -5f, 0f);

	private bool bEffectLock;

	private WeaponMainUI tWeaponMainUI;

	private OrangeUIBase linkUI;

	private float fNowValue;

	protected override void Awake()
	{
		base.Awake();
		weaponModelCanvas = WeaponModel.gameObject.AddOrGetComponent<Canvas>();
		weaponModelCanvas.enabled = false;
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(ReleaseMemory));
	}

	public void initalization_data()
	{
		expiteminfos = new expiteminfo[expitems.Length];
		for (int i = 0; i < expitems.Length; i++)
		{
			expiteminfos[i] = new expiteminfo();
		}
		EquipRoot.SetActive(false);
		mainweaponic = UnityEngine.Object.Instantiate(refWeaponIconBaseBig, EquipRoot.transform.Find("mainweapon")).GetComponent<CommonIconBase>();
		subweaponic = UnityEngine.Object.Instantiate(refWeaponIconBaseBig, EquipRoot.transform.Find("subweapon")).GetComponent<CommonIconBase>();
		info2CommonIconBase = UnityEngine.Object.Instantiate(refWeaponIconBase, info2weapon.transform).GetComponent<CommonIconBase>();
		BtnClickCBs.Clear();
		InfoCanvasGroups = new CanvasGroup[buttons.Length];
		for (int j = 0; j < buttons.Length; j++)
		{
			Button button = buttons[j];
			BtnClickCB btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = j;
			btnClickCB.action = (Action<int>)Delegate.Combine(btnClickCB.action, new Action<int>(OnClickInfoBtnCB));
			button.onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(button.gameObject, btnClickCB);
			InfoCanvasGroups[j] = infos[j].transform.GetComponent<CanvasGroup>();
		}
		buttons[0].interactable = false;
		for (int k = 0; k < expitems.Length; k++)
		{
			ExpButtonRef expButtonRef = expitems[k];
			BtnClickCB btnClickCB2 = new BtnClickCB();
			btnClickCB2.nBtnID = k;
			BtnClickCB btnClickCB3 = btnClickCB2;
			btnClickCB3.action = (Action<int>)Delegate.Combine(btnClickCB3.action, new Action<int>(OnExpItemBtnCB));
			expButtonRef.Button.onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(expButtonRef.Button.gameObject, btnClickCB2);
			btnClickCB2 = new BtnClickCB();
			btnClickCB2.nBtnID = k;
			BtnClickCB btnClickCB4 = btnClickCB2;
			btnClickCB4.action = (Action<int>)Delegate.Combine(btnClickCB4.action, new Action<int>(OnExpItemUnuseBtnCB));
			expButtonRef.UnuseBtn.onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(expButtonRef.UnuseBtn.gameObject, btnClickCB2);
			btnClickCB2 = new BtnClickCB();
			btnClickCB2.nBtnID = k;
			BtnClickCB btnClickCB5 = btnClickCB2;
			btnClickCB5.action = (Action<int>)Delegate.Combine(btnClickCB5.action, new Action<int>(OnExpItemAddBtnCB));
			expButtonRef.AddBtn.onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(expButtonRef.AddBtn.gameObject, btnClickCB2);
			btnClickCB2 = new BtnClickCB();
			btnClickCB2.nBtnID = k;
			BtnClickCB btnClickCB6 = btnClickCB2;
			btnClickCB6.action = (Action<int>)Delegate.Combine(btnClickCB6.action, new Action<int>(OnQuickExpItemAddBtnCB));
			expqucilitems[k].AddBtn.onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(expqucilitems[k].AddBtn.gameObject, btnClickCB2);
		}
		for (int l = 0; l < AttriBtn.Length; l++)
		{
			BtnClickCB btnClickCB7 = new BtnClickCB();
			btnClickCB7.nBtnID = l;
			btnClickCB7.action = (Action<int>)Delegate.Combine(btnClickCB7.action, new Action<int>(OnSelectUpdradeType));
			AttriBtn[l].onClick.AddListener(btnClickCB7.OnClick);
			if (AttriQBtn != null && AttriQBtn.Length > l)
			{
				AttriQBtn[l].onClick.AddListener(btnClickCB7.OnClick);
			}
			if (AttriQBg != null && AttriQBg.Length > l)
			{
				AttriQBg[l].SetActive(false);
			}
			if (AttriQSBG != null && AttriQSBG.Length > l)
			{
				AttriQSBG[l].SetActive(false);
			}
			BtnClickCBs.Add(AttriBtn[l].gameObject, btnClickCB7);
		}
		info2attrbargreen = new GameObject[5][];
		info2attrbarpurple = new GameObject[5][];
		for (int m = 0; m < 5; m++)
		{
			string[] array = string.Format("info2/AttrImage/AttrBar{0}", m).Split('/');
			UnityEngine.Transform transform = base.transform;
			for (int n = 0; n < array.Length; n++)
			{
				transform = transform.Find(array[n]);
			}
			info2attrbargreen[m] = new GameObject[10];
			info2attrbarpurple[m] = new GameObject[10];
			for (int num = 0; num < 10; num++)
			{
				info2attrbargreen[m][num] = transform.Find("green" + num).gameObject;
				info2attrbargreen[m][num].SetActive(false);
				info2attrbarpurple[m][num] = transform.Find("purple" + num).gameObject;
				info2attrbarpurple[m][num].SetActive(false);
			}
			AttriSBG[m].SetActive(false);
			info2attrarrow[m].SetActive(false);
			info2nsbg[m].SetActive(false);
		}
		for (int num2 = 0; num2 < skillitems.Length; num2++)
		{
			BtnClickCB btnClickCB8 = new BtnClickCB();
			btnClickCB8.nBtnID = num2;
			btnClickCB8.action = (Action<int>)Delegate.Combine(btnClickCB8.action, new Action<int>(OnClickSkillBtn));
			skillitems[num2].Button.onClick.AddListener(btnClickCB8.OnClick);
			BtnClickCBs.Add(skillitems[num2].Button.gameObject, btnClickCB8);
			skillitems[num2].SelectObj.SetActive(false);
		}
		for (int num3 = 0; num3 < rskillitems.Length; num3++)
		{
			BtnClickCB btnClickCB9 = new BtnClickCB();
			btnClickCB9.nBtnID = num3;
			btnClickCB9.action = (Action<int>)Delegate.Combine(btnClickCB9.action, new Action<int>(OnClickSkillBtn));
			rskillitems[num3].Button.onClick.AddListener(btnClickCB9.OnClick);
			BtnClickCBs.Add(rskillitems[num3].Button.gameObject, btnClickCB9);
			rskillitems[num3].SelectObj.SetActive(false);
		}
		if (QSkillLvUpBtninfo4 != null)
		{
			SkillLvUpBtninfo4.transform.localPosition = SkillLvUpBtninfo4.transform.localPosition + new Vector3(150f, 0f, 0f);
			((RectTransform)SkillLvUpBtninfo4.transform).sizeDelta = new Vector2(265f, 96.5f);
			QSkillLvUpBtninfo4.gameObject.SetActive(true);
		}
		for (int num4 = 0; num4 < skillmaterials.Length; num4++)
		{
			BtnClickCB btnClickCB10 = new BtnClickCB();
			btnClickCB10.nBtnID = num4;
			btnClickCB10.action = (Action<int>)Delegate.Combine(btnClickCB10.action, new Action<int>(OnSkillItemAddBtnCB));
			skillmaterials[num4].AddBtn.onClick.AddListener(btnClickCB10.OnClick);
			skillmaterials[num4].Button.onClick.AddListener(btnClickCB10.OnClick);
			BtnClickCBs.Add(skillmaterials[num4].AddBtn.gameObject, btnClickCB10);
		}
		detailpopop.SetActive(false);
		takeoutroot.SetActive(false);
		ChipSelectRoot.SetActive(false);
		ProfChangeRoot.SetActive(false);
		AutoProfChangeRoot.SetActive(false);
		if (RSkillRoot != null)
		{
			RSkillRoot.SetActive(false);
		}
		if (ShowRSkillRoot != null)
		{
			ShowRSkillRoot.SetActive(false);
		}
		if (QChangeRSkillRoot != null)
		{
			QChangeRSkillRoot.SetActive(false);
		}
		if (ReChangeRSkillRoot != null)
		{
			ReChangeRSkillRoot.SetActive(false);
		}
		if (CheckReChangeRSkillRoot != null)
		{
			CheckReChangeRSkillRoot.SetActive(false);
		}
		if (QuickSkillUp != null)
		{
			QuickSkillUp.SetActive(false);
		}
		if (QuickProfUp != null)
		{
			QuickProfUp.SetActive(false);
		}
		if (ReChangeRSkillBtn != null)
		{
			ReChangeRSkillBtn.gameObject.SetActive(false);
		}
		if (QuickReChangeRSkill != null)
		{
			QuickReChangeRSkill.gameObject.SetActive(false);
		}
		if (QuickUnLockRSkill != null)
		{
			QuickUnLockRSkill.gameObject.SetActive(false);
		}
		if (UnLockRSkill != null)
		{
			UnLockRSkill.gameObject.SetActive(false);
		}
		if (NoQuickItemBtn != null)
		{
			NoQuickItemBtn.gameObject.SetActive(false);
		}
		if (skillitems.Length == 7)
		{
			skillitems[6].Button.gameObject.SetActive(false);
			ShowRSKills.gameObject.SetActive(false);
		}
		tWeaponMainUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<WeaponMainUI>("UI_WEAPONMAIN");
		nNowInfoIndex = 0;
		InitWeapon();
		if (bNeedInitList)
		{
			SortWeaponList();
			listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
			listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
		}
		tLHSR.totalCount = listHasWeapons.Count + listFragWeapons.Count;
		bool flag = false;
		int num5 = 10;
		for (int num6 = listHasWeapons.Count - 1; num6 >= 0; num6--)
		{
			if (nTargetWeaponID == listHasWeapons[num6])
			{
				int num7 = ((ManagedSingleton<EquipHelper>.Instance.WeaponSortDescend == 0) ? (num6 + listFragWeapons.Count) : num6);
				if (tLHSR.totalCount > num5 && num7 >= tLHSR.totalCount - num5)
				{
					tLHSR.RefillCells(tLHSR.totalCount - num5);
				}
				else
				{
					tLHSR.RefillCells(num7);
				}
				flag = true;
				break;
			}
		}
		for (int num8 = listFragWeapons.Count - 1; num8 >= 0; num8--)
		{
			if (nTargetWeaponID == listFragWeapons[num8])
			{
				int num9 = ((ManagedSingleton<EquipHelper>.Instance.WeaponSortDescend == 1) ? (num8 + listHasWeapons.Count) : num8);
				if (tLHSR.totalCount > num5 && num9 >= tLHSR.totalCount - num5)
				{
					tLHSR.RefillCells(tLHSR.totalCount - num5);
				}
				else
				{
					tLHSR.RefillCells(num9);
				}
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			tLHSR.RefillCells();
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(GameObject obj)
		{
			textureObj = UnityEngine.Object.Instantiate(obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
			textureObj.AssignNewWeaponRender(ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[nTargetWeaponID], new Vector3(0f, 0f, 2.5f), WeaponModel);
			weaponModelCanvas.enabled = true;
		});
		if (m_unlockEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "ui_lockfx", "ui_lockfx", delegate(GameObject asset)
			{
				m_unlockEffect = UnityEngine.Object.Instantiate(asset, base.transform);
				m_unlockEffect.transform.position = WeaponModel.transform.position;
				m_unlockEffectLength = m_unlockEffect.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
				m_unlockEffect.SetActive(false);
			});
		}
		if (m_levelUpEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "levelupeffect", "LevelUpEffect", delegate(GameObject asset)
			{
				m_levelUpEffect = UnityEngine.Object.Instantiate(asset, base.transform);
				m_levelUpEffect.transform.position = WeaponModel.transform.position + m_effectOffset;
				m_levelUpEffect.SetActive(false);
			});
		}
		if (m_levelUpWordEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "levelupwordeffect", "LevelUpWordEffect", delegate(GameObject asset)
			{
				m_levelUpWordEffect = UnityEngine.Object.Instantiate(asset, base.transform);
				m_levelUpWordEffect.transform.position = WeaponModel.transform.position + m_effectOffset;
				m_levelUpWordEffect.SetActive(false);
			});
		}
	}

	private void Start()
	{
		if (expiteminfos == null)
		{
			initalization_data();
		}
	}

	private void OnDestroy()
	{
		if (textureObj != null)
		{
			UnityEngine.Object.Destroy(textureObj.gameObject);
			textureObj = null;
		}
	}

	public void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		if (textureObj != null)
		{
			textureObj.gameObject.SetActive(true);
			weaponModelCanvas.enabled = true;
		}
	}

	public void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		if (textureObj != null)
		{
			textureObj.gameObject.SetActive(false);
			weaponModelCanvas.enabled = false;
		}
	}

	public void LateUpdate()
	{
		if (skilldesc.alignByGeometry)
		{
			skilldesc.alignByGeometry = false;
		}
		if (QSkillScribt.alignByGeometry)
		{
			QSkillScribt.alignByGeometry = false;
		}
		Text[] rSkillMsgText = RSkillMsgText;
		foreach (Text text in rSkillMsgText)
		{
			if (text.alignByGeometry)
			{
				text.alignByGeometry = false;
			}
		}
	}

	public void UnlockWeaponGo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		ManagedSingleton<PlayerNetManager>.Instance.WeaponUnlockReq(nTargetWeaponID, delegate
		{
			SortWeaponList();
			listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
			listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
			PlayUnlockEffect();
			nNowWeaponID = -1;
			InitWeapon();
		});
	}

	public void ChangeWeapon(params object[] p_params)
	{
		if (!IsEffectPlaying() && p_params.Length >= 1)
		{
			int? num = p_params[0] as int?;
			if (num.HasValue && nTargetWeaponID != num)
			{
				nTotalExpItemAddExp = 0;
				nTargetWeaponID = num ?? 0;
				InitWeapon();
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
			}
		}
	}

	public override void TopOpenUICloseCB()
	{
		InitExpItem();
		InitWeapon();
	}

	public void OnClickInfoBtnCB(int nBtnID)
	{
		if (nNextInfoIndex == -1)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickTab);
		}
		OnInfoBtnCB(nBtnID);
	}

	public void OnInfoBtnCB(int nBtnID)
	{
		for (int i = 0; i < infos.Length; i++)
		{
			if (nBtnID != i && nNowInfoIndex != i)
			{
				infos[i].SetActive(false);
			}
		}
		for (int j = 0; j < buttons.Length; j++)
		{
			if (nBtnID != j)
			{
				buttons[j].interactable = true;
			}
			else
			{
				buttons[j].interactable = false;
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
				infos[nNowInfoIndex].SetActive(false);
				nNowInfoIndex = nNextInfoIndex;
				InfoCanvasGroups[nNowInfoIndex].alpha = 0f;
				infos[nNowInfoIndex].SetActive(true);
				InitWeapon();
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

	private void ResetExpItems()
	{
		for (int i = 0; i < expitems.Length; i++)
		{
			ExpButtonRef expButtonRef = expitems[i];
			expiteminfo expiteminfo = expiteminfos[i];
			expButtonRef.BtnLabel.text = expiteminfo.nUseNum + "/" + expiteminfo.mHaveNum;
			if (expiteminfo.nUseNum > 0)
			{
				expButtonRef.UnuseBtn.gameObject.SetActive(true);
			}
			else
			{
				expButtonRef.UnuseBtn.gameObject.SetActive(false);
			}
		}
	}

	public void OnExpItemBtnCB(int nBtnID)
	{
		bool flag = false;
		int totalAddExp = GetTotalAddExp();
		expiteminfo expiteminfo = expiteminfos[nBtnID];
		if (tWeaponInfo.netInfo.Exp + totalAddExp >= GetTotalExpByPlayerLV() && tWeaponInfo.netInfo.Exp + totalAddExp + (int)expiteminfo.tITEM_TABLE.f_VALUE_X >= GetTotalExpByPlayerLV())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_WEAPON_LV"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
				ui.CloseSE = m_closeWindowBtn;
			});
			return;
		}
		expiteminfo.nUseNum++;
		if (expiteminfo.nUseNum > expiteminfo.mHaveNum)
		{
			expiteminfo.nUseNum = expiteminfo.mHaveNum;
		}
		else
		{
			nTotalExpItemAddExp = totalAddExp + (int)expiteminfo.tITEM_TABLE.f_VALUE_X;
			flag = true;
		}
		if (flag)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
		}
		ResetExpItems();
		if (nTotalExpItemAddExp > 0 && ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID))
		{
			upgradeBtn.interactable = true;
		}
		else
		{
			upgradeBtn.interactable = false;
		}
		InitWeapon();
	}

	public void OnExpItemUnuseBtnCB(int nBtnID)
	{
		expiteminfos[nBtnID].nUseNum = 0;
		ResetExpItems();
		nTotalExpItemAddExp = GetTotalAddExp();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
		if (nTotalExpItemAddExp > 0 && ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID))
		{
			upgradeBtn.interactable = true;
		}
		else
		{
			upgradeBtn.interactable = false;
		}
		InitWeapon();
	}

	public void OnQuickExpItemAddBtnCB(int nBtnID)
	{
		ITEM_TABLE item;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(expiteminfos[nBtnID].tITEM_TABLE.n_ID, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.Setup(item);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				InitExpItem();
				InitWeapon();
				OnQuickExpGrade();
			});
		});
	}

	public void OnExpItemAddBtnCB(int nBtnID)
	{
		ITEM_TABLE item;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(expiteminfos[nBtnID].tITEM_TABLE.n_ID, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.Setup(item);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				InitExpItem();
				InitWeapon();
			});
		});
	}

	public void UpdateExpBar()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(nTotalExpItemAddExp);
		int num = nNowExp * 100 / nNeedExp;
		if (nowLvWithAddExp.n_TOTAL_WEAPONEXP == GetMaxLvExp())
		{
			num = 100;
		}
		if (num > 100)
		{
			num = 100;
		}
		explabel.text = num + "%";
		int num2 = 100 / expbarsub.Length;
		int i;
		for (i = 0; i < expbarsub.Length && i * num2 < num; i++)
		{
			expbarsub[i].gameObject.SetActive(true);
		}
		for (; i < expbarsub.Length; i++)
		{
			expbarsub[i].gameObject.SetActive(false);
		}
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(tWeaponInfo.netInfo.Exp) < nowLvWithAddExp.n_ID)
		{
			useLVUpSE = true;
		}
		else
		{
			useLVUpSE = false;
		}
		explv.text = nowLvWithAddExp.n_ID.ToString();
	}

	public EXP_TABLE GetNowLvWithAddExp(int nAddExp)
	{
		int num = nAddExp;
		if (tWeaponInfo != null)
		{
			num += tWeaponInfo.netInfo.Exp;
		}
		EXP_TABLE eXP_TABLE = null;
		Dictionary<int, EXP_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EXP_TABLE value = enumerator.Current.Value;
			if (num < value.n_TOTAL_WEAPONEXP && value.n_TOTAL_WEAPONEXP - num <= value.n_WEAPONEXP)
			{
				eXP_TABLE = value;
				break;
			}
		}
		enumerator.Dispose();
		if (eXP_TABLE == null)
		{
			eXP_TABLE = new EXP_TABLE();
		}
		return ManagedSingleton<OrangeTableHelper>.Instance.ReduceLVByCheckPlayerExp(ManagedSingleton<PlayerHelper>.Instance.GetExp(), eXP_TABLE);
	}

	public int GetTotalAddExp()
	{
		int num = 0;
		for (int i = 0; i < expiteminfos.Length; i++)
		{
			expiteminfo expiteminfo = expiteminfos[i];
			if (expiteminfo.tITEM_TABLE != null)
			{
				num += expiteminfo.nUseNum * (int)expiteminfo.tITEM_TABLE.f_VALUE_X;
			}
		}
		return num;
	}

	public int GetTotalExpByPlayerLV()
	{
		EXP_TABLE expTable = ManagedSingleton<PlayerHelper>.Instance.GetExpTable();
		return expTable.n_TOTAL_WEAPONEXP - expTable.n_WEAPONEXP;
	}

	public int GetMaxLvExp()
	{
		return ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.OrderByDescending((KeyValuePair<int, EXP_TABLE> obj) => obj.Key).First().Value.n_TOTAL_WEAPONEXP;
	}

	public void OnSelectUpdradeType(int nBtnID)
	{
		if (nNowUpgradeType != nBtnID)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			GetUpdrageTypeDict(nNowUpgradeType, true);
			nNowUpgradeType = nBtnID;
			GetUpdrageTypeDict(nNowUpgradeType, false);
			InitWeapon();
		}
	}

	public void OnUpgradeGo()
	{
		List<ItemConsumptionInfo> list = new List<ItemConsumptionInfo>();
		for (int i = 0; i < expitems.Length; i++)
		{
			expiteminfo expiteminfo = expiteminfos[i];
			if (expiteminfo.nUseNum > 0)
			{
				ItemConsumptionInfo item = new ItemConsumptionInfo
				{
					Amount = expiteminfo.nUseNum,
					ItemID = expiteminfo.tITEM_TABLE.n_ID
				};
				list.Add(item);
			}
		}
		ObjScale(1f, 0f, 0.2f, quickupgradeinfo, delegate
		{
			quickupgradeinfo.SetActive(false);
		});
		if (list.Count == 0)
		{
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<PlayerNetManager>.Instance.WeaponAddExpReq(nTargetWeaponID, list, delegate
		{
			if (useLVUpSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addLVUp);
				PlayLevelUp3DEffect();
				useLVUpSE = false;
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addLVExp);
				PlayUpgrade3DEffect();
			}
			SortWeaponList();
			listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
			listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
			expSlider.value = 0f;
			InitExpItem();
			nNowWeaponID = -1;
			InitWeapon();
		});
	}

	private void ObjScale(float fStart, float fEnd, float fTime, GameObject tObj, Action endcb)
	{
		UnityEngine.Transform transform = base.gameObject.transform.Find("BGBanClick");
		if (transform != null)
		{
			transform.gameObject.SetActive(true);
		}
		if (tObjScaleCoroutine != null)
		{
			StopCoroutine(tObjScaleCoroutine);
		}
		if (fStart > fEnd)
		{
			if (fStart >= fNowValue && fNowValue >= fEnd)
			{
				fStart = fNowValue;
			}
		}
		else if (fStart <= fNowValue && fNowValue <= fEnd)
		{
			fStart = fNowValue;
		}
		tObjScaleCoroutine = StartCoroutine(ObjScaleCoroutine(fStart, fEnd, fTime, tObj, endcb));
	}

	public void OnQuickExpGrade()
	{
		nQucikLV = 0;
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		int n_ID = nowLvWithAddExp.n_ID;
		nowLvWithAddExp = ManagedSingleton<PlayerHelper>.Instance.GetExpTable();
		int maxLvExp = GetMaxLvExp();
		int num = tWeaponInfo.netInfo.Exp;
		for (int num2 = expiteminfos.Length - 1; num2 >= 0; num2--)
		{
			expiteminfo expiteminfo = expiteminfos[num2];
			if (expiteminfo.tITEM_TABLE != null)
			{
				int num3 = (int)Mathf.Floor((float)(maxLvExp - num) / expiteminfo.tITEM_TABLE.f_VALUE_X);
				if (num3 < expiteminfo.mHaveNum)
				{
					num += num3 * (int)expiteminfo.tITEM_TABLE.f_VALUE_X;
					break;
				}
				num += expiteminfo.mHaveNum * (int)expiteminfo.tITEM_TABLE.f_VALUE_X;
			}
		}
		while (num < nowLvWithAddExp.n_TOTAL_WEAPONEXP - nowLvWithAddExp.n_WEAPONEXP && nowLvWithAddExp.n_ID != n_ID)
		{
			nowLvWithAddExp = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[nowLvWithAddExp.n_ID - 1];
		}
		expSlider.minValue = 0f;
		expSlider.maxValue = nowLvWithAddExp.n_ID - n_ID;
		InitQuickExpitem();
		quickupgradeinfo.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, quickupgradeinfo, null);
		weaponiconImg.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.m_iconWeapon, tWEAPON_TABLE.s_ICON);
		string[] array = new string[7] { "Dummy", "D", "C", "B", "A", "S", "SS" };
		weaponiconfrm.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, "UI_iconsource_frame_" + array[tWEAPON_TABLE.n_RARITY]);
		weaponicon.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, "UI_iconsource_BG_" + array[tWEAPON_TABLE.n_RARITY] + "_small");
		quickupgradeinfo.SetActive(true);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
	}

	public void OnQuickExpGradeClose()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, quickupgradeinfo, delegate
		{
			quickupgradeinfo.SetActive(false);
		});
		if (nTotalExpItemAddExp > 0 && ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID))
		{
			upgradeBtn.interactable = true;
		}
		else
		{
			upgradeBtn.interactable = false;
		}
	}

	public void OnAddQuickLV()
	{
		GetNowLvWithAddExp(0);
		if ((float)nQucikLV < expSlider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV++;
			InitQuickExpitem();
		}
	}

	public void OnDecreaseQuickLV()
	{
		GetNowLvWithAddExp(0);
		if (nQucikLV > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV--;
			InitQuickExpitem();
		}
	}

	public void OnMaxQuickLV()
	{
		GetNowLvWithAddExp(0);
		if ((float)nQucikLV != expSlider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV = (int)expSlider.maxValue;
			InitQuickExpitem();
		}
	}

	public void OnMinQuickLV()
	{
		GetNowLvWithAddExp(0);
		if (nQucikLV != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV = 0;
			InitQuickExpitem();
		}
	}

	private void InitQuickExpItem()
	{
		for (int i = 0; i < expqucilitems.Length; i++)
		{
			expiteminfo expiteminfo = expiteminfos[i];
			ExpButtonRef expButtonRef = expqucilitems[i];
			if (expiteminfo.mHaveNum > 0)
			{
				expButtonRef.BtnLabel.text = expiteminfo.nUseNum + "/" + expiteminfo.mHaveNum;
				expButtonRef.BtnLabel.color = Color.white;
				expButtonRef.Button.interactable = true;
				expButtonRef.AddBtn.gameObject.SetActive(false);
				expButtonRef.UnuseBtn.gameObject.SetActive(expiteminfo.nUseNum > 0);
				expButtonRef.frmimg.color = Color.white;
				expButtonRef.bgimg.color = Color.white;
			}
			else
			{
				expButtonRef.BtnLabel.text = "0/0";
				expButtonRef.BtnLabel.color = Color.red;
				expButtonRef.Button.interactable = false;
				expButtonRef.AddBtn.gameObject.SetActive(true);
				expButtonRef.UnuseBtn.gameObject.SetActive(false);
				expButtonRef.frmimg.color = disablecolor;
				expButtonRef.bgimg.color = disablecolor;
			}
		}
	}

	private void ReSetQuickItems(int nMaxExp)
	{
		int num = tWeaponInfo.netInfo.Exp;
		EXP_TABLE expTable = ManagedSingleton<PlayerHelper>.Instance.GetExpTable();
		for (int num2 = expiteminfos.Length - 1; num2 >= 0; num2--)
		{
			expiteminfo expiteminfo = expiteminfos[num2];
			if (expiteminfo.tITEM_TABLE != null)
			{
				int num3 = (int)Mathf.Floor((float)(nMaxExp - num) / expiteminfo.tITEM_TABLE.f_VALUE_X);
				if (expiteminfo.mHaveNum >= num3)
				{
					expiteminfo.nUseNum = 0;
					while ((expiteminfo.nUseNum + 1) * (int)expiteminfo.tITEM_TABLE.f_VALUE_X + num < nMaxExp && (expiteminfo.nUseNum + 1) * (int)expiteminfo.tITEM_TABLE.f_VALUE_X + num < expTable.n_TOTAL_WEAPONEXP)
					{
						expiteminfo.nUseNum++;
					}
					num += expiteminfo.nUseNum * (int)expiteminfo.tITEM_TABLE.f_VALUE_X;
				}
				else
				{
					expiteminfo.nUseNum = expiteminfo.mHaveNum;
					num += expiteminfo.nUseNum * (int)expiteminfo.tITEM_TABLE.f_VALUE_X;
				}
			}
		}
	}

	public void InitQuickExpitem()
	{
		EXP_TABLE value = GetNowLvWithAddExp(0);
		int n_ID = value.n_ID;
		expSlider.value = nQucikLV;
		TargetLvText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TARGET_LV"), nQucikLV, expSlider.maxValue);
		int nMaxExp = 0;
		if (nQucikLV > 0 && ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(nQucikLV + n_ID, out value))
		{
			nMaxExp = value.n_TOTAL_WEAPONEXP;
		}
		ReSetQuickItems(nMaxExp);
		InitQuickExpItem();
		nTotalExpItemAddExp = GetTotalAddExp();
		if (nTotalExpItemAddExp > 0)
		{
			QuickExpUpgradeGo.interactable = true;
		}
		else
		{
			QuickExpUpgradeGo.interactable = false;
		}
		ResetExpItems();
		InitWeapon();
	}

	public void OnSilderChange(float value)
	{
		if ((int)Mathf.Round(expSlider.value) != nQucikLV)
		{
			EXP_TABLE value2 = GetNowLvWithAddExp(0);
			int n_ID = value2.n_ID;
			if ((int)Mathf.Round(expSlider.value) != nQucikLV)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			}
			nQucikLV = (int)Mathf.Round(expSlider.value);
			TargetLvText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TARGET_LV"), nQucikLV, expSlider.maxValue);
			int nMaxExp = 0;
			if (nQucikLV != 0 && ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(nQucikLV + n_ID, out value2))
			{
				nMaxExp = value2.n_TOTAL_WEAPONEXP;
			}
			ReSetQuickItems(nMaxExp);
			InitQuickExpItem();
			nTotalExpItemAddExp = GetTotalAddExp();
			if (nTotalExpItemAddExp > 0)
			{
				QuickExpUpgradeGo.interactable = true;
			}
			else
			{
				QuickExpUpgradeGo.interactable = false;
			}
			ResetExpItems();
			InitWeapon();
		}
	}

	public void OnUpgradeStar()
	{
		if (tSTAR_TABLE != null && !bEffectLock)
		{
			bEffectLock = true;
			ManagedSingleton<PlayerNetManager>.Instance.WeaponUpgradeStarReq(nTargetWeaponID, tSTAR_TABLE.n_ID, delegate
			{
				SortWeaponList();
				listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
				listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
				tSTAR_TABLE = null;
				StartCoroutine(PlayStarUpEffectAndRefreshMenu());
				ManagedSingleton<DeepRecordHelper>.Instance.ApiRefreashTime = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC;
			});
		}
	}

	public void OnSelectAutoSkill(int nBtnID)
	{
		int[] array = new int[9] { tWEAPON_TABLE.n_PASSIVE_1, tWEAPON_TABLE.n_PASSIVE_2, tWEAPON_TABLE.n_PASSIVE_3, tWEAPON_TABLE.n_PASSIVE_4, tWEAPON_TABLE.n_PASSIVE_5, tWEAPON_TABLE.n_PASSIVE_6, tWEAPON_TABLE.n_DIVE, 0, 0 };
		int[] array2 = new int[9] { tWEAPON_TABLE.n_PASSIVE_MATERIAL1, tWEAPON_TABLE.n_PASSIVE_MATERIAL2, tWEAPON_TABLE.n_PASSIVE_MATERIAL3, tWEAPON_TABLE.n_PASSIVE_MATERIAL4, tWEAPON_TABLE.n_PASSIVE_MATERIAL5, tWEAPON_TABLE.n_PASSIVE_MATERIAL6, tWEAPON_TABLE.n_DIVE_MATERIAL, 0, 0 };
		int[] array3 = new int[9] { tWEAPON_TABLE.n_PASSIVE_UNLOCK1, tWEAPON_TABLE.n_PASSIVE_UNLOCK2, tWEAPON_TABLE.n_PASSIVE_UNLOCK3, tWEAPON_TABLE.n_PASSIVE_UNLOCK4, tWEAPON_TABLE.n_PASSIVE_UNLOCK5, tWEAPON_TABLE.n_PASSIVE_UNLOCK6, tWEAPON_TABLE.n_DIVE_UNLOCK, 0, 0 };
		if ((array[nBtnID] == 0 && nBtnID != 6) || (nBtnID == 6 && tWEAPON_TABLE.n_DIVE == 0))
		{
			return;
		}
		UnlockButtonRef[] array4 = skillitems;
		if (tWEAPON_TABLE.n_DIVE != 0)
		{
			array4 = rskillitems;
		}
		array4[nNowSelectSkillIndex].SelectObj.SetActive(false);
		nNowSelectSkillIndex = nBtnID;
		nNowSelectAutoSkill = array[nBtnID];
		QuickUnLockRSkill.gameObject.SetActive(false);
		UnLockRSkill.gameObject.SetActive(false);
		int num = 0;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(OrangeConst.ARMSSKILL_UNLOCK_ITEM))
		{
			num = ManagedSingleton<PlayerNetManager>.Instance.dicItem[OrangeConst.ARMSSKILL_UNLOCK_ITEM].netItemInfo.Stack;
		}
		if (nBtnID == 6)
		{
			ShowRSKills.gameObject.SetActive(true);
			nNowSelectAutoSkill = 0;
			if (tWeaponInfo.netDiveSkillInfo != null)
			{
				nNowSelectAutoSkill = tWeaponInfo.netDiveSkillInfo.SkillID;
			}
			else
			{
				QuickUnLockRSkill.gameObject.SetActive(true);
				UnLockRSkill.gameObject.SetActive(true);
				QuickUnLockRSkill.interactable = num >= OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER;
				NoQuickItemBtn.gameObject.SetActive(num < OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER);
			}
		}
		else
		{
			ShowRSKills.gameObject.SetActive(false);
		}
		array4[nNowSelectSkillIndex].SelectObj.SetActive(true);
		SKILL_TABLE value;
		ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nNowSelectAutoSkill, out value);
		float num2 = 100f;
		string text = "";
		string format = "";
		List<NetWeaponSkillInfo> netSkillInfos = tWeaponInfo.netSkillInfos;
		bool flag = false;
		int num3 = 0;
		if (netSkillInfos != null)
		{
			for (int i = 0; i < netSkillInfos.Count; i++)
			{
				if (netSkillInfos[i].Slot == nBtnID + 1)
				{
					flag = true;
					num3 = netSkillInfos[i].Level;
				}
			}
		}
		if (value != null)
		{
			text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			format = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
			int num4 = num3;
			if (num4 == 0)
			{
				num4 = 1;
			}
			int n_EFFECT = value.n_EFFECT;
			if (n_EFFECT == 1 || n_EFFECT != 3)
			{
				num2 = value.f_EFFECT_X + (float)num4 * value.f_EFFECT_Y;
				skilldesc.text = string.Format(format, num2.ToString("0.00"));
			}
			else
			{
				num2 = value.f_EFFECT_Y + value.f_EFFECT_Z * (float)num4;
				skilldesc.text = string.Format(format, num2.ToString("0"));
			}
			skillImage.gameObject.SetActive(true);
			skillImage.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON);
		}
		else if (nBtnID == 6 && nNowSelectAutoSkill == 0)
		{
			text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ARMSSKILL_TITLE");
			skilldesc.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ARMSSKILL_TITLE");
			skillImage.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill("icon_passive_000"), "icon_passive_000");
		}
		else
		{
			skillImage.gameObject.SetActive(false);
			skilldesc.text = string.Format(format, num2.ToString("0.00"));
		}
		skillname.text = text;
		bool flag2 = false;
		if (flag)
		{
			lvcondition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_WEAPON_UPGRADE"), num3 + 1);
			if (GetNowLvWithAddExp(0).n_ID < num3 + 1)
			{
				lvcondition.color = Color.red;
			}
			else
			{
				lvcondition.color = Color.white;
				flag2 = true;
			}
		}
		else
		{
			int num5 = array3[nBtnID];
			lvcondition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_WEAPON_STAR"), num5);
			if (tWeaponInfo.netInfo.Star < num5)
			{
				lvcondition.color = Color.red;
			}
			else
			{
				lvcondition.color = Color.white;
				flag2 = true;
			}
		}
		if (value != null && value.n_LVMAX == num3)
		{
			maxlvroot.SetActive(true);
			UnLockRoot.transform.parent.gameObject.SetActive(false);
			return;
		}
		if (flag)
		{
			maxlvroot.SetActive(false);
			UnLockRoot.transform.parent.gameObject.SetActive(true);
			UnLockRoot.SetActive(false);
			SkillLvUpRoot.SetActive(true);
			EXP_TABLE value2 = null;
			if (!ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(num3, out value2))
			{
				value2 = new EXP_TABLE();
				value2.n_SKILLUP_SP = 999999;
				value2.n_SKILLUP_MONEY = 999999;
			}
			int skillPoint = ManagedSingleton<PlayerHelper>.Instance.GetSkillPoint();
			int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
			info4havenum[0].text = "x" + skillPoint;
			info4havenum[1].text = "x" + zenny;
			info4usenum[0].text = "x" + value2.n_SKILLUP_SP;
			info4usenum[1].text = "x" + value2.n_SKILLUP_MONEY;
			int num6 = 0;
			if (value != null)
			{
				num6 = Math.Min(value.n_LVMAX, nExpNowLV);
			}
			if (skillPoint >= value2.n_SKILLUP_SP && zenny >= value2.n_SKILLUP_MONEY && num6 > num3 && flag2)
			{
				SkillLvUpBtninfo4.interactable = true;
				QSkillLvUpBtninfo4.interactable = true;
			}
			else
			{
				SkillLvUpBtninfo4.interactable = false;
				QSkillLvUpBtninfo4.interactable = false;
			}
			if (skillPoint >= value2.n_SKILLUP_SP)
			{
				info4havenum[0].color = Color.white;
			}
			else
			{
				info4havenum[0].color = Color.red;
			}
			if (zenny >= value2.n_SKILLUP_MONEY)
			{
				info4havenum[1].color = Color.white;
			}
			else
			{
				info4havenum[1].color = Color.red;
			}
			return;
		}
		if (array2[nBtnID] == 0 || !ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.ContainsKey(array2[nBtnID]))
		{
			maxlvroot.SetActive(false);
			UnLockRoot.transform.parent.gameObject.SetActive(false);
			return;
		}
		maxlvroot.SetActive(false);
		UnLockRoot.transform.parent.gameObject.SetActive(true);
		UnLockRoot.SetActive(true);
		SkillLvUpRoot.SetActive(false);
		CostLabel0.transform.parent.gameObject.SetActive(true);
		UnLockBtninfo4.gameObject.SetActive(true);
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[array2[nBtnID]];
		CostLabel0.text = mATERIAL_TABLE.n_MONEY.ToString();
		int[] array5 = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] array6 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		bool interactable = true;
		for (int j = 0; j < skillmaterials.Length; j++)
		{
			ExpButtonRef expButtonRef = skillmaterials[j];
			int num7 = array5[j];
			if (num7 == 0)
			{
				expButtonRef.Button.gameObject.SetActive(false);
				continue;
			}
			expButtonRef.Button.gameObject.SetActive(true);
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[num7];
			UpdateItemNeedInfo(iTEM_TABLE, expButtonRef.BtnImgae, expButtonRef.frmimg, expButtonRef.bgimg, null);
			expButtonRef.UnuseBtn.gameObject.SetActive(false);
			int num8 = 0;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
			{
				num8 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
			}
			expButtonRef.BtnLabel.text = num8 + "/" + array6[j];
			if (num8 >= array6[j])
			{
				expButtonRef.AddBtn.gameObject.SetActive(false);
				expButtonRef.BtnLabel.color = Color.white;
				expButtonRef.Button.interactable = true;
				expButtonRef.frmimg.color = Color.white;
				expButtonRef.bgimg.color = Color.white;
			}
			else
			{
				expButtonRef.AddBtn.gameObject.SetActive(true);
				expButtonRef.BtnLabel.color = Color.red;
				expButtonRef.Button.interactable = false;
				expButtonRef.frmimg.color = disablecolor;
				expButtonRef.bgimg.color = disablecolor;
				interactable = false;
			}
		}
		if (!flag2)
		{
			interactable = false;
		}
		if (nBtnID == 6)
		{
			if (tWeaponInfo.netDiveSkillInfo == null)
			{
				ReChangeRSkillBtn.gameObject.SetActive(false);
				QuickReChangeRSkill.gameObject.SetActive(false);
				NoQuickItemBtn.gameObject.SetActive(num < OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER);
			}
			else
			{
				ReChangeRSkillBtn.gameObject.SetActive(true);
				QuickReChangeRSkill.gameObject.SetActive(true);
				NoQuickItemBtn.gameObject.SetActive(num < OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER);
			}
			UnLockBtninfo4.gameObject.SetActive(false);
		}
		else
		{
			ReChangeRSkillBtn.gameObject.SetActive(false);
			QuickReChangeRSkill.gameObject.SetActive(false);
			NoQuickItemBtn.gameObject.SetActive(false);
			UnLockBtninfo4.gameObject.SetActive(true);
		}
		ReChangeRSkillBtn.interactable = interactable;
		UnLockRSkill.interactable = interactable;
		QuickReChangeRSkill.interactable = num >= OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER;
		UnLockBtninfo4.interactable = interactable;
	}

	public void OnClickSkillBtn(int nBtnID)
	{
		if (nBtnID != nNowSelectSkillIndex && (new int[7] { tWEAPON_TABLE.n_PASSIVE_1, tWEAPON_TABLE.n_PASSIVE_2, tWEAPON_TABLE.n_PASSIVE_3, tWEAPON_TABLE.n_PASSIVE_4, tWEAPON_TABLE.n_PASSIVE_5, tWEAPON_TABLE.n_PASSIVE_6, tWEAPON_TABLE.n_DIVE })[nBtnID] != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			OnSelectAutoSkill(nBtnID);
		}
	}

	public void OnSkillItemAddBtnCB(int nBtnID)
	{
		int[] array = new int[9] { tWEAPON_TABLE.n_PASSIVE_MATERIAL1, tWEAPON_TABLE.n_PASSIVE_MATERIAL2, tWEAPON_TABLE.n_PASSIVE_MATERIAL3, tWEAPON_TABLE.n_PASSIVE_MATERIAL4, tWEAPON_TABLE.n_PASSIVE_MATERIAL5, tWEAPON_TABLE.n_PASSIVE_MATERIAL6, tWEAPON_TABLE.n_DIVE_MATERIAL, 0, 0 };
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[array[nNowSelectSkillIndex]];
		int[] array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] materialcounts = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		ITEM_TABLE item = null;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[array2[nBtnID]].n_ID, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.Setup(item, null, materialcounts[nBtnID]);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				OnSelectAutoSkill(nNowSelectSkillIndex);
			});
		});
	}

	public void UnLockSkill()
	{
		if (nNowSelectSkillIndex == 6)
		{
			if (tWeaponInfo.netDiveSkillInfo != null)
			{
				OnCheckReChangeRSkill(0);
				return;
			}
			ManagedSingleton<PlayerNetManager>.Instance.WeaponRandomRSkill(nTargetWeaponID, false, delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
				int nBtnID2 = nNowSelectSkillIndex;
				nNowWeaponID = -1;
				InitWeapon();
				OnSelectAutoSkill(nBtnID2);
			});
		}
		else
		{
			ManagedSingleton<PlayerNetManager>.Instance.WeaponUnlockSkillSlotReq(nTargetWeaponID, (WeaponSkillSlot)(nNowSelectSkillIndex + 1), delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
				int nBtnID = nNowSelectSkillIndex;
				nNowWeaponID = -1;
				InitWeapon();
				OnSelectAutoSkill(nBtnID);
			});
		}
	}

	public void LevellUpSkill()
	{
		int num = 0;
		List<NetWeaponSkillInfo> netSkillInfos = tWeaponInfo.netSkillInfos;
		if (netSkillInfos != null)
		{
			for (int i = 0; i < netSkillInfos.Count; i++)
			{
				if (netSkillInfos[i].Slot == nNowSelectSkillIndex + 1)
				{
					num = netSkillInfos[i].Level;
				}
			}
		}
		SkillLvUpBtninfo4.interactable = false;
		QSkillLvUpBtninfo4.interactable = false;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<PlayerNetManager>.Instance.WeaponSkillUpReq(nTargetWeaponID, (WeaponSkillSlot)(nNowSelectSkillIndex + 1), num + 1, delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addLVUp);
			int nBtnID = nNowSelectSkillIndex;
			nNowWeaponID = -1;
			SkillLvUpBtninfo4.interactable = false;
			QSkillLvUpBtninfo4.interactable = false;
			InitWeapon();
			OnSelectAutoSkill(nBtnID);
			if (SdSkillRoot == null || SdSkillRoot.activeSelf)
			{
				PlayUpgradeEffect(skillitems[nNowSelectSkillIndex].Button.transform.position);
			}
			if (RSkillRoot != null && RSkillRoot.activeSelf)
			{
				PlayUpgradeEffect(rskillitems[nNowSelectSkillIndex].Button.transform.position);
			}
		});
	}

	public void OnEquipBtn()
	{
		if (ManagedSingleton<EquipHelper>.Instance.GetWeaponBenchSlot(nNowWeaponID) != 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BACKUP_WEAPON_EQUIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		EquipRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, EquipRoot, null);
		EquipRoot.SetActive(true);
		WEAPON_TABLE wEAPON_TABLE = null;
		NetPlayerInfo netPlayerInfo = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo;
		if (netPlayerInfo.MainWeaponID != 0 && ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(netPlayerInfo.MainWeaponID))
		{
			wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netPlayerInfo.MainWeaponID];
			mainweaponic.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, OnEquipMainWeaponIcon);
			mainweaponic.SetOtherInfo(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[netPlayerInfo.MainWeaponID].netInfo, CommonIconBase.WeaponEquipType.Main);
		}
		if (netPlayerInfo.SubWeaponID != 0 && ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(netPlayerInfo.SubWeaponID))
		{
			wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netPlayerInfo.SubWeaponID];
			subweaponic.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, OnEquipSubWeaponIcon);
			subweaponic.SetOtherInfo(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[netPlayerInfo.SubWeaponID].netInfo, CommonIconBase.WeaponEquipType.Sub);
		}
		else
		{
			subweaponic.Setup(0, "", "", OnEquipSubWeaponIcon);
			subweaponic.SetOtherInfo(null, CommonIconBase.WeaponEquipType.Sub);
		}
	}

	public void OnTakeOutBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		InitTakeoutPopup();
		takeoutroot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, takeoutroot, null);
		takeoutroot.SetActive(true);
	}

	public void OnCloseTakeoutDetailPopup()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		CloseTakeoutDetailPopup();
	}

	private void CloseTakeoutDetailPopup()
	{
		ObjScale(1f, 0f, 0.2f, takeoutroot, delegate
		{
			takeoutroot.SetActive(false);
		});
	}

	private void InitTakeoutPopup()
	{
		for (int i = 0; i < takeoutitems.Length; i++)
		{
			takeoutitems[i].Button.gameObject.SetActive(false);
		}
		for (int j = 0; j < takeouttypemask.Length; j++)
		{
			takeouttypemask[j].gameObject.SetActive(false);
		}
		nTakeOutType = 0;
		takeoutlbl[0].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TAKEOUT_TIP");
		takeoutlbl[1].text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TAKEOUT_COST"), 0);
		TakeOutBtn.interactable = false;
	}

	private void AddRtItem(ref List<expiteminfo> rtItems, ref expiteminfo tItem)
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

	public void SetTakeOutType(int nType)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		if ((nTakeOutType & nType) != 0)
		{
			nTakeOutType &= ~nType;
			takeouttypemask[nType >> 1].gameObject.SetActive(false);
		}
		else
		{
			nTakeOutType |= nType;
			takeouttypemask[nType >> 1].gameObject.SetActive(true);
		}
		int num = 0;
		bool flag = false;
		List<expiteminfo> rtItems = new List<expiteminfo>();
		if (((uint)nTakeOutType & (true ? 1u : 0u)) != 0)
		{
			int exp = tWeaponInfo.netInfo.Exp;
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_WEAPON_TAKEOUT];
			int num2 = (int)iTEM_TABLE.f_VALUE_X;
			if (exp > 0)
			{
				expiteminfo tItem = new expiteminfo();
				tItem.nUseNum = (exp - exp % num2) / num2;
				if (exp % num2 > 0)
				{
					tItem.nUseNum++;
				}
				tItem.tITEM_TABLE = iTEM_TABLE;
				AddRtItem(ref rtItems, ref tItem);
				num += OrangeConst.TAKEOUT_COST;
			}
		}
		if (((uint)nTakeOutType & 2u) != 0)
		{
			int num3 = 0;
			int num4 = 0;
			flag = false;
			NetWeaponExpertInfo tNetWeaponExpertInfo;
			for (int i = 0; i < tWeaponInfo.netExpertInfos.Count; i++)
			{
				NetWeaponExpertInfo netWeaponExpertInfo = tWeaponInfo.netExpertInfos[i];
				if (netWeaponExpertInfo.ExpertLevel > 0)
				{
					tNetWeaponExpertInfo = netWeaponExpertInfo;
					IEnumerable<KeyValuePair<int, UPGRADE_TABLE>> source = ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT.Where((KeyValuePair<int, UPGRADE_TABLE> obj) => obj.Value.n_GROUP == tWEAPON_TABLE.n_UPGRADE && obj.Value.n_LV < tNetWeaponExpertInfo.ExpertLevel);
					int num5 = source.Count();
					for (int j = 0; j < num5; j++)
					{
						KeyValuePair<int, UPGRADE_TABLE> keyValuePair = source.ElementAt(j);
						num3 += keyValuePair.Value.n_MONEY;
						num4 += keyValuePair.Value.n_PROF;
					}
				}
			}
			if (num3 > 0)
			{
				expiteminfo tItem2 = new expiteminfo();
				tItem2.tITEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_MONEY];
				tItem2.nUseNum = num3;
				AddRtItem(ref rtItems, ref tItem2);
				flag = true;
			}
			if (num4 > 0)
			{
				expiteminfo tItem3 = new expiteminfo();
				tItem3.tITEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_SHARE_PROF];
				tItem3.nUseNum = num4;
				AddRtItem(ref rtItems, ref tItem3);
				flag = true;
			}
			if (flag)
			{
				num += OrangeConst.TAKEOUT_COST;
			}
		}
		if (((uint)nTakeOutType & 4u) != 0)
		{
			int[] array = new int[9] { tWEAPON_TABLE.n_PASSIVE_MATERIAL1, tWEAPON_TABLE.n_PASSIVE_MATERIAL2, tWEAPON_TABLE.n_PASSIVE_MATERIAL3, tWEAPON_TABLE.n_PASSIVE_MATERIAL4, tWEAPON_TABLE.n_PASSIVE_MATERIAL5, tWEAPON_TABLE.n_PASSIVE_MATERIAL6, 0, 0, 0 };
			List<NetWeaponSkillInfo> netSkillInfos = tWeaponInfo.netSkillInfos;
			int num6 = 0;
			int num7 = 0;
			flag = false;
			if (netSkillInfos != null)
			{
				for (int k = 0; k < netSkillInfos.Count; k++)
				{
					if (netSkillInfos[k].Level > 1)
					{
						for (int l = 1; l < netSkillInfos[k].Level; l++)
						{
							EXP_TABLE eXP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[l];
							num6 += eXP_TABLE.n_SKILLUP_MONEY;
							num7 += eXP_TABLE.n_SKILLUP_SP;
						}
					}
				}
				if (num6 > 0)
				{
					expiteminfo tItem4 = new expiteminfo();
					tItem4.tITEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_MONEY];
					tItem4.nUseNum = num6;
					AddRtItem(ref rtItems, ref tItem4);
					flag = true;
				}
				if (num7 > 0)
				{
					expiteminfo tItem5 = new expiteminfo();
					tItem5.tITEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_SKILL_POINT];
					tItem5.nUseNum = num7;
					AddRtItem(ref rtItems, ref tItem5);
					flag = true;
				}
			}
			if (flag)
			{
				num += OrangeConst.TAKEOUT_COST;
			}
		}
		takeoutlbl[1].text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TAKEOUT_COST"), num);
		for (int m = 0; m < rtItems.Count && m < takeoutitems.Length; m++)
		{
			UpdateItemNeedInfo(rtItems[m].tITEM_TABLE, takeoutitems[m].BtnImgae, takeoutitems[m].frmimg, takeoutitems[m].bgimg, null);
			takeoutitems[m].BtnLabel.text = rtItems[m].nUseNum.ToString();
			takeoutitems[m].Button.gameObject.SetActive(true);
		}
		for (int n = rtItems.Count; n < takeoutitems.Length; n++)
		{
			takeoutitems[n].Button.gameObject.SetActive(false);
		}
		TakeOutBtn.interactable = rtItems.Count > 0 && ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel() >= num;
	}

	public void TakeOutWeapon()
	{
		if (nTakeOutType == 0)
		{
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.WeaponResetReq(nTargetWeaponID, (WeaponResetType)nTakeOutType, delegate(List<NetRewardInfo> p_param)
		{
			if (p_param != null && p_param.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(p_param);
				});
			}
			nNowWeaponID = -1;
			InitWeapon();
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		CloseTakeoutDetailPopup();
	}

	private void OnEquipMainWeaponIcon(int p_param)
	{
		OnEquipMainWeapon();
	}

	public void OnEquipMainWeapon()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		ObjScale(1f, 0f, 0.2f, EquipRoot, delegate
		{
			EquipRoot.SetActive(false);
		});
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID != nTargetWeaponID)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nTargetWeaponID, WeaponWieldType.MainWeapon, delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON);
				nNowWeaponID = -1;
				InitWeapon();
			});
		}
	}

	private void OnEquipSubWeaponIcon(int p_param)
	{
		OnEquipSubWeapon();
	}

	public void OnEquipSubWeapon()
	{
		ObjScale(1f, 0f, 0.2f, EquipRoot, delegate
		{
			EquipRoot.SetActive(false);
		});
		NetPlayerInfo netPlayerInfo = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo;
		if (netPlayerInfo.SubWeaponID == 0 && netPlayerInfo.MainWeaponID == nTargetWeaponID)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("WEAPONWIELD_NO_EMPTY"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		if (netPlayerInfo.SubWeaponID != nTargetWeaponID)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nTargetWeaponID, WeaponWieldType.SubWeapon, delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON);
				nNowWeaponID = -1;
				InitWeapon();
			});
		}
	}

	public void OnEquipRootClose()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, EquipRoot, delegate
		{
			EquipRoot.SetActive(false);
		});
	}

	public void OnAddWeaponPiece()
	{
	}

	public void OnAddStarPiece()
	{
		if (tSTAR_TABLE == null)
		{
			return;
		}
		ITEM_TABLE item = null;
		MATERIAL_TABLE tMATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tSTAR_TABLE.n_MATERIAL];
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[tMATERIAL_TABLE.n_MATERIAL_1].n_ID, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.Setup(item, null, tMATERIAL_TABLE.n_MATERIAL_MOUNT1);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				InitInfo3();
			});
		});
	}

	public void InitWeapon()
	{
		if (!GetWeaponStatus())
		{
			return;
		}
		if (nNowWeaponID != nTargetWeaponID)
		{
			InitInfo0OrGlobal();
			InitExpItem();
			InitInfo2();
			InitInfo3();
			InitInfo4();
			InitInfo5();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON);
		}
		nNowWeaponID = nTargetWeaponID;
		WeaponStatus weaponStatus = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(nTargetWeaponID);
		WeaponStatus wsubstatus = new WeaponStatus();
		PlayerStatus pallstatus = new PlayerStatus();
		WeaponStatus weaponStatus2 = weaponStatus;
		DISC_TABLE value;
		if (nNowInfoIndex == 1)
		{
			weaponStatus2 = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(tWeaponInfo, nTotalExpItemAddExp, false, null, delegate(string str)
			{
				DebugInfoText.text = str;
			});
		}
		else if (nNowInfoIndex == 2)
		{
			int[] array = new int[5];
			array[nNowUpgradeType] = 1;
			weaponStatus2 = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(tWeaponInfo, 0, false, array, delegate(string str)
			{
				DebugInfoText.text = str;
			});
		}
		else if (nNowInfoIndex == 3)
		{
			weaponStatus2 = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(tWeaponInfo, 0, true, null, delegate(string str)
			{
				DebugInfoText.text = str;
			});
		}
		else if (nNowInfoIndex == 5 && ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(tWeaponInfo.netInfo.Chip, out value))
		{
			ChipInfo value2;
			ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(tWeaponInfo.netInfo.Chip, out value2);
		}
		int battlePower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(weaponStatus, wsubstatus, pallstatus, true);
		int num = battlePower;
		num = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(weaponStatus2, wsubstatus, pallstatus, true);
		if (nNowInfoIndex != 2 && nNowInfoIndex != 4)
		{
			OrangeRareText.Rare n_RARITY = (OrangeRareText.Rare)tWEAPON_TABLE.n_RARITY;
			s_NAME[(int)(n_RARITY - 1)].transform.parent.gameObject.SetActive(true);
			StarRoot0.SetActive(true);
			sBattleScore.transform.parent.gameObject.SetActive(true);
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID) && ManagedSingleton<PlayerHelper>.Instance.GetLV() >= 5)
			{
				if (nTargetWeaponID == ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID || nTargetWeaponID == ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID)
				{
					Ctrlbtns[0].SetActive(false);
					Ctrlbtns[1].SetActive(true);
				}
				else
				{
					Ctrlbtns[0].SetActive(true);
					Ctrlbtns[1].SetActive(false);
				}
			}
			else
			{
				for (int i = 0; i < Ctrlbtns.Length; i++)
				{
					Ctrlbtns[i].SetActive(false);
				}
			}
			WeaponModel.gameObject.SetActive(true);
		}
		else
		{
			OrangeRareText.Rare n_RARITY2 = (OrangeRareText.Rare)tWEAPON_TABLE.n_RARITY;
			s_NAME[(int)(n_RARITY2 - 1)].transform.parent.gameObject.SetActive(false);
			StarRoot0.SetActive(false);
			sBattleScore.transform.parent.gameObject.SetActive(false);
			for (int j = 0; j < Ctrlbtns.Length; j++)
			{
				Ctrlbtns[j].SetActive(false);
			}
			WeaponModel.gameObject.SetActive(false);
		}
		sBattleScore.text = battlePower.ToString();
		for (int k = 0; k < BeforePower.Length; k++)
		{
			BeforePower[k].text = battlePower.ToString();
		}
		for (int l = 0; l < AfterPower.Length; l++)
		{
			AfterPower[l].text = num.ToString();
		}
		beforevalue[0].text = weaponStatus.nATK.ToString();
		beforevalue[1].text = weaponStatus.nHP.ToString();
		beforevalue[2].text = weaponStatus.nCRI.ToString();
		beforevalue[3].text = weaponStatus.nHIT.ToString();
		aftervalue[0].text = weaponStatus2.nATK.ToString();
		aftervalue[1].text = weaponStatus2.nHP.ToString();
		aftervalue[2].text = weaponStatus2.nCRI.ToString();
		aftervalue[3].text = weaponStatus2.nHIT.ToString();
		for (int m = 0; m < 4; m++)
		{
			if (beforevalue[m].text != aftervalue[m].text)
			{
				aftervalue[m].color = valuegreen;
			}
			else
			{
				aftervalue[m].color = Color.white;
			}
		}
		if (nNowInfoIndex == 3)
		{
			ReSetStar(tWeaponInfo.netInfo.Star, true);
		}
		else
		{
			ReSetStar(tWeaponInfo.netInfo.Star);
		}
		if (nNowInfoIndex == 0)
		{
			AttributrText[0].text = nATK.ToString();
			AttributrBar[0].SetFValue((float)nATK / (float)OrangeConst.WEAPON_ATK_MAX);
			AttributrText[1].text = nHP.ToString();
			AttributrBar[1].SetFValue((float)nHP / (float)OrangeConst.WEAPON_HP_MAX);
			AttributrText[2].text = nCRI.ToString();
			AttributrBar[2].SetFValue((float)nCRI / (float)OrangeConst.WEAPON_CRI_MAX);
			AttributrText[3].text = nHIT.ToString();
			AttributrBar[3].SetFValue((float)nHIT / (float)OrangeConst.WEAPON_HIT_MAX);
			AttributrText[4].text = tSKILL_TABLE.n_FIRE_SPEED.ToString();
			AttributrBar[4].SetFValue((float)tSKILL_TABLE.n_FIRE_SPEED / (float)OrangeConst.WEAPON_FIRESPEED_MAX);
			AttributrText[5].text = tSKILL_TABLE.f_DISTANCE.ToString("0.0");
			AttributrBar[5].SetFValue(tSKILL_TABLE.f_DISTANCE / (float)OrangeConst.WEAPON_DISTANCE_MAX);
			AttributrText[6].text = tSKILL_TABLE.n_MAGAZINE.ToString();
			AttributrBar[6].SetFValue((float)tSKILL_TABLE.n_MAGAZINE / (float)OrangeConst.WEAPON_MAGAZINE_MAX);
			AttributrText[7].text = tWEAPON_TABLE.n_SPEED.ToString();
			AttributrBar[7].SetFValue((float)tWEAPON_TABLE.n_SPEED / (float)OrangeConst.WEAPON_MOVE_MAX);
			AttributrText[8].text = tSKILL_TABLE.n_RELOAD.ToString();
			AttributrBar[8].SetFValue((float)tSKILL_TABLE.n_RELOAD / (float)OrangeConst.WEAPON_CD_MAX);
			int weaponRecordVal = DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.BATTLE, tWeaponInfo);
			int weaponRecordVal2 = DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.EXPLORE, tWeaponInfo);
			int weaponRecordVal3 = DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.ACTION, tWeaponInfo);
			AttributrText[9].text = weaponRecordVal.ToString();
			AttributrBar[9].SetFValue((float)weaponRecordVal / (float)OrangeConst.RECORD_BATTLE_MAX);
			AttributrText[10].text = weaponRecordVal2.ToString();
			AttributrBar[10].SetFValue((float)weaponRecordVal2 / (float)OrangeConst.RECORD_EXPLORE_MAX);
			AttributrText[11].text = weaponRecordVal3.ToString();
			AttributrBar[11].SetFValue((float)weaponRecordVal3 / (float)OrangeConst.RECORD_ACTION_MAX);
		}
		if (nNowInfoIndex == 1)
		{
			UpdateExpBar();
		}
		if (nNowInfoIndex == 2)
		{
			if (!bHasNextLV)
			{
				for (int num2 = NextArrow.Length - 1; num2 >= 0; num2--)
				{
					NextArrow[num2].gameObject.SetActive(false);
				}
				pnextlv.gameObject.SetActive(false);
				AfterAttr0.gameObject.SetActive(false);
				AfterPower[1].gameObject.SetActive(false);
				pnextlvbg.gameObject.SetActive(false);
			}
			else
			{
				for (int num3 = NextArrow.Length - 1; num3 >= 0; num3--)
				{
					NextArrow[num3].gameObject.SetActive(true);
				}
				pnextlv.gameObject.SetActive(true);
				AfterAttr0.gameObject.SetActive(true);
				AfterPower[1].gameObject.SetActive(true);
				pnextlvbg.gameObject.SetActive(true);
			}
			if (info2numtext != null && info2numtext.Length > 5)
			{
				info2numtext[4].text = battlePower.ToString();
			}
			for (int n = 0; n < tWeaponInfo.netExpertInfos.Count; n++)
			{
				NetWeaponExpertInfo netWeaponExpertInfo = tWeaponInfo.netExpertInfos[n];
				if (netWeaponExpertInfo.ExpertType == nNowUpgradeType + 1)
				{
					pnowlv.text = netWeaponExpertInfo.ExpertLevel.ToString();
					pnextlv.text = (netWeaponExpertInfo.ExpertLevel + 1).ToString();
					int num4 = tWEAPON_TABLE.n_RARITY;
					if (num4 > 5)
					{
						num4 = 5;
					}
					pnowlvbg.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, "UI_iconsource_powerupicon_Lv" + num4);
					pnextlvbg.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, "UI_iconsource_powerupicon_Lv" + num4);
					break;
				}
			}
			int[] array2 = new int[5] { nATK, nHP, nCRI, nHIT, nLuk };
			int[] array3 = new int[5] { nATK, nHP, nCRI, nHIT, nLuk };
			if (weaponStatus2 != null)
			{
				array3[0] = weaponStatus2.nATK;
				array3[1] = weaponStatus2.nHP;
				array3[2] = weaponStatus2.nCRI;
				array3[3] = weaponStatus2.nHIT;
				array3[4] = weaponStatus2.nLuck;
			}
			string[] array4 = new string[5] { "STATUS_ATK", "STATUS_HP", "STATUS_CRI", "STATUS_HIT", "STATUS_LUK" };
			AttrNowText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(array4[nNowUpgradeType]);
			BeforeAttr0.text = array2[nNowUpgradeType].ToString();
			AfterAttr0.text = array3[nNowUpgradeType].ToString();
			info2condition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REQUIRE_WEAPON_LV"), tUPGRADE_TABLE.n_WEAPON_LV);
			info2condition.supportRichText = true;
			bool interactable = true;
			if (GetNowLvWithAddExp(0).n_ID >= tUPGRADE_TABLE.n_WEAPON_LV)
			{
				info2condition.color = Color.white;
			}
			else
			{
				info2condition.color = Color.red;
				interactable = false;
			}
			if (tWeaponInfo.netInfo.Prof < tUPGRADE_TABLE.n_PROF)
			{
				info2havenum[0].text = "<color=#ff0000>" + tWeaponInfo.netInfo.Prof + "</color>";
			}
			else
			{
				info2havenum[0].text = tWeaponInfo.netInfo.Prof.ToString();
			}
			if (ManagedSingleton<PlayerHelper>.Instance.GetZenny() < tUPGRADE_TABLE.n_MONEY)
			{
				info2havenum[1].text = "<color=#ff0000>" + ManagedSingleton<PlayerHelper>.Instance.GetZenny() + "</color>";
			}
			else
			{
				info2havenum[1].text = ManagedSingleton<PlayerHelper>.Instance.GetZenny().ToString();
			}
			info2usenum[0].text = tUPGRADE_TABLE.n_PROF.ToString();
			info2usenum[1].text = tUPGRADE_TABLE.n_MONEY.ToString();
			if (ManagedSingleton<PlayerHelper>.Instance.GetZenny() < tUPGRADE_TABLE.n_MONEY)
			{
				interactable = false;
			}
			if (tWeaponInfo.netInfo.Prof + ManagedSingleton<PlayerHelper>.Instance.GetProfPoint() < tUPGRADE_TABLE.n_PROF)
			{
				interactable = false;
			}
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID))
			{
				interactable = false;
			}
			if (!bHasNextLV)
			{
				info2condition.color = Color.white;
				info2condition.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MASTERY_LEVELMAX");
				interactable = false;
			}
			AdvancedBtn.interactable = interactable;
		}
		if (nNowInfoIndex == 3 && tSTAR_TABLE == null)
		{
			StarButton.interactable = false;
		}
	}

	private void UpdateWeaponNeedInfo(ITEM_TABLE tITEM_TABLE, StageLoadIcon img, StageLoadIcon frm, StageLoadIcon bg, Text text)
	{
		img.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.m_iconWeapon, tITEM_TABLE.s_ICON);
		OrangeRareText.Rare n_RARE = (OrangeRareText.Rare)tITEM_TABLE.n_RARE;
		frm.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall((int)n_RARE));
		bg.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareBgSmall((int)n_RARE));
		if (text != null)
		{
			text.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(tITEM_TABLE.w_NAME);
		}
	}

	private void UpdateItemNeedInfo(ITEM_TABLE tITEM_TABLE, StageLoadIcon img, StageLoadIcon frm, StageLoadIcon bg, Text text)
	{
		img.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconItem(tITEM_TABLE.s_ICON), tITEM_TABLE.s_ICON);
		OrangeRareText.Rare n_RARE = (OrangeRareText.Rare)tITEM_TABLE.n_RARE;
		frm.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall((int)n_RARE));
		bg.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareBgSmall((int)n_RARE));
		if (text != null)
		{
			text.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(tITEM_TABLE.w_NAME);
		}
	}

	private void InitInfo0OrGlobal()
	{
		nTotalExpItemAddExp = 0;
		for (int i = 0; i < s_NAME.Length; i++)
		{
			s_NAME[i].text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(tWEAPON_TABLE.w_NAME);
			s_NAME[i].transform.parent.gameObject.SetActive(false);
		}
		float num = 100f;
		int n_EFFECT = tSKILL_TABLE.n_EFFECT;
		if (n_EFFECT == 1)
		{
			num = tSKILL_TABLE.f_EFFECT_X + 0f * tSKILL_TABLE.f_EFFECT_Y;
		}
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(tSKILL_TABLE.w_TIP);
		n_WEAPON_SKILL.text = string.Format(l10nValue, num.ToString("0.00"));
		n_WEAPON_SKILL.alignByGeometry = false;
		n_WEAPON_Name.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(tSKILL_TABLE.w_NAME);
		OrangeRareText.Rare n_RARITY = (OrangeRareText.Rare)tWEAPON_TABLE.n_RARITY;
		s_NAME[(int)(n_RARITY - 1)].transform.parent.gameObject.SetActive(true);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.RT_UPDATE_WEAPON, tWEAPON_TABLE);
		for (int num2 = info0skillimg.transform.childCount - 1; num2 >= 0; num2--)
		{
			UnityEngine.Object.Destroy(info0skillimg.transform.GetChild(num2).gameObject);
		}
		CommonIconBase component = UnityEngine.Object.Instantiate(refWeaponIconBase, info0skillimg.transform).GetComponent<CommonIconBase>();
		component.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, tWEAPON_TABLE.s_ICON);
		CommonIconBase.WeaponEquipType p_weaponEquipType = CommonIconBase.WeaponEquipType.UnEquip;
		NetPlayerInfo netPlayerInfo = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo;
		if (tWeaponInfo.netInfo.WeaponID == netPlayerInfo.MainWeaponID)
		{
			p_weaponEquipType = CommonIconBase.WeaponEquipType.Main;
		}
		else if (tWeaponInfo.netInfo.WeaponID == netPlayerInfo.SubWeaponID)
		{
			p_weaponEquipType = CommonIconBase.WeaponEquipType.Sub;
		}
		component.SetOtherInfo(tWeaponInfo.netInfo, p_weaponEquipType);
		if (StartRedDot != null)
		{
			StartRedDot.SetActive(ManagedSingleton<EquipHelper>.Instance.IsCanWeaponUpgradeStart(tWeaponInfo.netInfo));
		}
		if (tWEAPON_TABLE.n_UNLOCK_ID != 0)
		{
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID))
			{
				BookBtn.gameObject.SetActive(false);
				BookFrame.SetActive(false);
				needrootinfo0.SetActive(true);
				UnLockBtn.gameObject.SetActive(true);
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[tWEAPON_TABLE.n_UNLOCK_ID];
				UpdateWeaponNeedInfo(iTEM_TABLE, info0needimg, info0needfrm, info0needbg, neednameinfo0);
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
				{
					ItemInfo itemInfo = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID];
					needbar1.fillAmount = (float)itemInfo.netItemInfo.Stack / (float)tWEAPON_TABLE.n_UNLOCK_COUNT;
					needtext1.text = itemInfo.netItemInfo.Stack + "/" + tWEAPON_TABLE.n_UNLOCK_COUNT;
					UnLockBtn.interactable = itemInfo.netItemInfo.Stack >= tWEAPON_TABLE.n_UNLOCK_COUNT;
				}
				else
				{
					needbar1.fillAmount = 0f;
					needtext1.text = "<color=#FF0000>0</color>/" + tWEAPON_TABLE.n_UNLOCK_COUNT;
					UnLockBtn.interactable = false;
				}
			}
			else
			{
				needrootinfo0.SetActive(false);
				UnLockBtn.gameObject.SetActive(false);
				BookBtn.gameObject.SetActive(true);
				if (MonoBehaviourSingleton<UIManager>.Instance.GetUI<IllustrationTargetUI>("UI_IllustrationTarget") != null)
				{
					BookBtn.gameObject.SetActive(false);
				}
				CheckGalleryInfo();
			}
		}
		else
		{
			needrootinfo0.SetActive(false);
		}
		int[] array = new int[6]
		{
			0,
			OrangeConst.OPENRANK_WEAPON_LVUP,
			OrangeConst.OPENRANK_WEAPON_UPGRADE,
			OrangeConst.OPENRANK_STARUP,
			OrangeConst.OPENRANK_SKILL_LVUP,
			OrangeConst.OPENRANK_DISC
		};
		bool flag = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID);
		BtnClickCB btnClickCB = null;
		for (int j = 0; j < buttons.Length; j++)
		{
			UnityEngine.Transform transform = buttons[j].transform.Find("InfoBtnLock");
			if (!(transform != null))
			{
				continue;
			}
			btnClickCB = BtnClickCBs[buttons[j].gameObject];
			if (btnClickCB != null)
			{
				if (ManagedSingleton<PlayerHelper>.Instance.GetLV() >= array[j] && flag)
				{
					transform.gameObject.SetActive(false);
					btnClickCB.bIsLock = false;
				}
				else
				{
					transform.gameObject.SetActive(true);
					buttons[j].transform.Find("Text").GetComponent<Text>().color = new Color(0.80784315f, 0.8235294f, 0.8509804f);
					btnClickCB.bIsLock = true;
				}
			}
		}
		if (!flag && nNowInfoIndex != 0)
		{
			OnInfoBtnCB(0);
		}
	}

	private void CheckGalleryInfo()
	{
		ManagedSingleton<GalleryHelper>.Instance.GalleryGetTableAll(nTargetWeaponID, GalleryType.Weapon, out listGalleryInfo, out listGalleryUnlock, out listGalleryLock);
		bool active = listGalleryLock.Any((GALLERY_TABLE p) => ManagedSingleton<GalleryHelper>.Instance.GalleryCheckUnlock(p.n_ID));
		BookFrame.SetActive(active);
		float num = 191f;
		GalleryCalcResult galleryCalcResult = ManagedSingleton<GalleryHelper>.Instance.GalleryCalculationProgress(nTargetWeaponID, GalleryType.Weapon);
		float num2 = (float)galleryCalcResult.m_a / (float)galleryCalcResult.m_b;
		if (num2 < 0.333f)
		{
			BookBarImg.sprite = m_colorBar[0].sprite;
		}
		else if (num2 < 0.666f)
		{
			BookBarImg.sprite = m_colorBar[1].sprite;
		}
		else if (num2 < 0.999f)
		{
			BookBarImg.sprite = m_colorBar[2].sprite;
		}
		else
		{
			BookBarImg.sprite = m_colorBar[3].sprite;
		}
		num *= num2;
		BookBar.sizeDelta = new Vector2(num, BookBar.sizeDelta.y);
		BookText.text = (int)(num2 * 100f) + "%";
	}

	private IEnumerator WaitEffectPlayEnd()
	{
		while (BookUPEffect.gameObject.activeSelf)
		{
			yield return new WaitForSeconds(0.2f);
		}
		CheckGalleryInfo();
	}

	public void OnOpenGalleryTargetUI()
	{
		if (BookUPEffect.isActiveAndEnabled)
		{
			return;
		}
		if (BookFrame.activeSelf)
		{
			BookFrame.SetActive(false);
			BookUPEffect.transform.gameObject.SetActive(true);
			BookUPEffect.animation.Reset();
			BookUPEffect.animation.Play("newAnimation", 1);
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
		else if (MonoBehaviourSingleton<UIManager>.Instance.GetUI<IllustrationUI>("UI_Illustration") != null)
		{
			OnClickCloseBtn();
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_IllustrationTarget", delegate(IllustrationTargetUI ui)
			{
				ui.Setup(this);
				ui.CloseSE = m_closeWindowBtn;
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_bookBtn);
			});
		}
	}

	private void UpdateGalleryLock()
	{
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
			ManagedSingleton<PlayerNetManager>.Instance.GalleryUnlockReq(galleryIDs);
		}
		if (lockInfo.Count != 0)
		{
			listGalleryLock = lockInfo;
		}
	}

	public void InitExpItem()
	{
		List<ITEM_TABLE> list = (from item in ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.Values
			where item.n_TYPE == 2 && item.n_TYPE_X == 1
			orderby item.n_TYPE_X
			select item).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			expiteminfo expiteminfo = expiteminfos[i];
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
		nTotalExpItemAddExp = 0;
		quickupgradeinfo.SetActive(false);
		upgradeBtn.interactable = false;
		for (int j = 0; j < expitems.Length; j++)
		{
			expiteminfo expiteminfo2 = expiteminfos[j];
			ExpButtonRef expButtonRef = expitems[j];
			int num = 0;
			if (expiteminfo2.tITEM_TABLE != null)
			{
				num = (int)expiteminfo2.tITEM_TABLE.f_VALUE_X;
				expButtonRef.Button.gameObject.SetActive(true);
				UpdateItemNeedInfo(expiteminfo2.tITEM_TABLE, expButtonRef.BtnImgae, expButtonRef.frmimg, expButtonRef.bgimg, null);
			}
			else
			{
				expButtonRef.Button.gameObject.SetActive(false);
			}
			expButtonRef.MsgText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EXP_ITEM") + num;
			if (expiteminfo2.mHaveNum > 0)
			{
				expButtonRef.BtnLabel.text = expiteminfo2.nUseNum + "/" + expiteminfo2.mHaveNum;
				expButtonRef.BtnLabel.color = Color.white;
				expButtonRef.Button.interactable = true;
				expButtonRef.AddBtn.gameObject.SetActive(false);
				expButtonRef.UnuseBtn.gameObject.SetActive(expiteminfo2.nUseNum > 0);
				expButtonRef.frmimg.color = Color.white;
				expButtonRef.bgimg.color = Color.white;
			}
			else
			{
				expButtonRef.BtnLabel.text = "0/0";
				expButtonRef.BtnLabel.color = Color.red;
				expButtonRef.Button.interactable = false;
				expButtonRef.AddBtn.gameObject.SetActive(true);
				expButtonRef.UnuseBtn.gameObject.SetActive(false);
				expButtonRef.frmimg.color = disablecolor;
				expButtonRef.bgimg.color = disablecolor;
			}
		}
		for (int k = 0; k < expqucilitems.Length; k++)
		{
			ExpButtonRef expButtonRef2 = expqucilitems[k];
			expiteminfo expiteminfo3 = expiteminfos[k];
			if (expiteminfo3.tITEM_TABLE != null)
			{
				expButtonRef2.Button.gameObject.SetActive(true);
				UpdateItemNeedInfo(expiteminfo3.tITEM_TABLE, expButtonRef2.BtnImgae, expButtonRef2.frmimg, expButtonRef2.bgimg, null);
			}
			else
			{
				expButtonRef2.Button.gameObject.SetActive(false);
			}
		}
	}

	private void InitInfo2()
	{
		info2CommonIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, tWEAPON_TABLE.s_ICON);
		NetPlayerInfo netPlayerInfo = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo;
		if (netPlayerInfo.MainWeaponID == tWEAPON_TABLE.n_ID)
		{
			info2CommonIconBase.SetOtherInfo(tWeaponInfo.netInfo, CommonIconBase.WeaponEquipType.Main);
		}
		else if (netPlayerInfo.SubWeaponID == tWEAPON_TABLE.n_ID)
		{
			info2CommonIconBase.SetOtherInfo(tWeaponInfo.netInfo, CommonIconBase.WeaponEquipType.Sub);
		}
		else
		{
			info2CommonIconBase.SetOtherInfo(tWeaponInfo.netInfo, CommonIconBase.WeaponEquipType.UnEquip);
		}
		for (int num = 4; num >= 0; num--)
		{
			if (nNowUpgradeType != num)
			{
				GetUpdrageTypeDict(num, true);
			}
		}
		GetUpdrageTypeDict(nNowUpgradeType, false);
	}

	public void ShowProfChangeRoot()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		bIgnoreFristSE = true;
		ProfChangeRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, ProfChangeRoot, null);
		ProfChangeRoot.SetActive(true);
		ProfSlider.maxValue = ManagedSingleton<PlayerHelper>.Instance.GetProfPoint();
		ProfSlider.value = 0f;
		InitChangeProf(0);
		bIgnoreFristSE = false;
	}

	public void CloseProfChangeRoot()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, ProfChangeRoot, delegate
		{
			ProfChangeRoot.SetActive(false);
		});
	}

	public void OnAddChangeProf()
	{
		int profPoint = ManagedSingleton<PlayerHelper>.Instance.GetProfPoint();
		if (nChangeProf < profPoint)
		{
			nChangeProf++;
			InitChangeProf(nChangeProf);
		}
	}

	public void OnDeAddChangeProf()
	{
		if (nChangeProf > 0)
		{
			nChangeProf--;
			InitChangeProf(nChangeProf);
		}
	}

	public void OnMinChangeProf()
	{
		InitChangeProf(0);
	}

	public void OnMaxChangeProf()
	{
		int profPoint = ManagedSingleton<PlayerHelper>.Instance.GetProfPoint();
		InitChangeProf(profPoint);
	}

	public void CloseAtuoProfChangeRoot()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, AutoProfChangeRoot, delegate
		{
			AutoProfChangeRoot.SetActive(false);
		});
	}

	public void OnChangeProfGo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addLVExp);
		ObjScale(1f, 0f, 0.2f, ProfChangeRoot, delegate
		{
			ProfChangeRoot.SetActive(false);
		});
		ManagedSingleton<PlayerNetManager>.Instance.WeaponExpertTransferReq(nTargetWeaponID, nChangeProf, delegate
		{
			nNowWeaponID = -1;
			InitWeapon();
		});
	}

	public void OnAutoChangeProfGo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addLVExp);
		ObjScale(1f, 0f, 0.2f, AutoProfChangeRoot, delegate
		{
			AutoProfChangeRoot.SetActive(false);
		});
		ManagedSingleton<PlayerNetManager>.Instance.WeaponExpertTransferReq(nTargetWeaponID, nChangeProf, delegate
		{
			nNowWeaponID = -1;
			InitWeapon();
			OnUpgradeTypeGo();
		});
	}

	private void InitChangeProf(int nProf, bool bCheckSlider = true)
	{
		int profPoint = ManagedSingleton<PlayerHelper>.Instance.GetProfPoint();
		int prof = tWeaponInfo.netInfo.Prof;
		for (int i = 0; i < beforepcard.Length; i++)
		{
			beforepcard[i].text = profPoint.ToString();
			afterpcard[i].text = (profPoint - nProf).ToString();
			beforeprof[i].text = prof.ToString();
			afterprof[i].text = (prof + nProf).ToString();
		}
		string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MASTERY_EXCHANGEAUTO_TEXT"), nProf);
		TargetProfText.text = text;
		AutoChangeProfText.text = text;
		nChangeProf = nProf;
		if (bCheckSlider)
		{
			ProfSlider.value = nChangeProf;
		}
		ChangeProfBtn.interactable = nProf > 0;
	}

	public void OnChangeSliderProf()
	{
		nChangeProf = (int)ProfSlider.value;
		InitChangeProf(nChangeProf);
		if (!bIgnoreFristSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
		}
	}

	private void InitInfo3()
	{
		int[] array = new int[10] { tWEAPON_TABLE.n_PASSIVE_1, tWEAPON_TABLE.n_PASSIVE_2, tWEAPON_TABLE.n_PASSIVE_3, tWEAPON_TABLE.n_PASSIVE_4, tWEAPON_TABLE.n_PASSIVE_5, tWEAPON_TABLE.n_PASSIVE_6, 0, 0, 0, 0 };
		int[] array2 = new int[10] { tWEAPON_TABLE.n_PASSIVE_UNLOCK1, tWEAPON_TABLE.n_PASSIVE_UNLOCK2, tWEAPON_TABLE.n_PASSIVE_UNLOCK3, tWEAPON_TABLE.n_PASSIVE_UNLOCK4, tWEAPON_TABLE.n_PASSIVE_UNLOCK5, tWEAPON_TABLE.n_PASSIVE_UNLOCK6, 0, 0, 0, 0 };
		SKILL_TABLE value = null;
		int num = -1;
		for (int i = 0; i < array.Length && array[i] != 0; i++)
		{
			if (array2[i] == nNowStar + 1)
			{
				num = i;
				break;
			}
		}
		if (num != -1 && array[num] != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(array[num], out value))
		{
			if (value.s_ICON != null)
			{
				info3skillimg.gameObject.SetActive(true);
				info3skillimg.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON);
			}
			else
			{
				info3skillimg.gameObject.SetActive(false);
			}
			if (value.s_SHOWCASE != "null")
			{
				info3msgimg.gameObject.SetActive(true);
				info3msgimg.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetShowcase(value.s_SHOWCASE), value.s_SHOWCASE);
			}
			else
			{
				info3msgimg.gameObject.SetActive(false);
			}
			info3skillname.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
		}
		else
		{
			info3skillimg.gameObject.SetActive(false);
			info3msgimg.gameObject.SetActive(false);
			info3skillname.text = "";
		}
		if (tSTAR_TABLE != null && tSTAR_TABLE.n_MATERIAL != 0)
		{
			needrootinfo3.SetActive(true);
			MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tSTAR_TABLE.n_MATERIAL];
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[mATERIAL_TABLE.n_MATERIAL_1];
			UpdateWeaponNeedInfo(iTEM_TABLE, info3needimg, info3needfrm, info3needbg, neednameinfo3);
			ItemInfo value2;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(iTEM_TABLE.n_ID, out value2))
			{
				needbar2.fillAmount = (float)value2.netItemInfo.Stack / (float)mATERIAL_TABLE.n_MATERIAL_MOUNT1;
				needtext2.text = value2.netItemInfo.Stack + "/" + mATERIAL_TABLE.n_MATERIAL_MOUNT1;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID) && value2.netItemInfo.Stack >= mATERIAL_TABLE.n_MATERIAL_MOUNT1)
				{
					StarButton.interactable = true;
				}
				else
				{
					StarButton.interactable = false;
				}
			}
			else
			{
				needbar2.fillAmount = 0f;
				needtext2.text = "<color=#FF0000>0</color>/" + mATERIAL_TABLE.n_MATERIAL_MOUNT1;
				StarButton.interactable = false;
			}
		}
		else
		{
			needrootinfo3.SetActive(false);
			StarButton.interactable = false;
		}
	}

	private void InitInfo4()
	{
		int[] array = new int[6] { tWEAPON_TABLE.n_PASSIVE_1, tWEAPON_TABLE.n_PASSIVE_2, tWEAPON_TABLE.n_PASSIVE_3, tWEAPON_TABLE.n_PASSIVE_4, tWEAPON_TABLE.n_PASSIVE_5, tWEAPON_TABLE.n_PASSIVE_6 };
		if (tWEAPON_TABLE.n_DIVE == 0 && nNowSelectSkillIndex == 6)
		{
			rskillitems[nNowSelectSkillIndex].SelectObj.SetActive(false);
			nNowSelectSkillIndex = 0;
		}
		else
		{
			rskillitems[nNowSelectSkillIndex].SelectObj.SetActive(false);
			if (nNowSelectSkillIndex < 6)
			{
				skillitems[nNowSelectSkillIndex].SelectObj.SetActive(false);
			}
		}
		OnSelectAutoSkill(0);
		for (int i = 0; i < 6; i++)
		{
			InitInfo4Autoskill(i, array[i]);
		}
		if (rskillitems.Length == 7)
		{
			UnlockButtonRef unlockButtonRef = rskillitems[6];
			if (tWEAPON_TABLE.n_DIVE == 0)
			{
				if (RSkillRoot != null)
				{
					RSkillRoot.SetActive(false);
				}
				if (SdSkillRoot != null)
				{
					SdSkillRoot.SetActive(true);
				}
				return;
			}
			if (RSkillRoot != null)
			{
				RSkillRoot.SetActive(true);
			}
			if (SdSkillRoot != null)
			{
				SdSkillRoot.SetActive(false);
			}
			unlockButtonRef.MsgText.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("UI_LOCKED");
			unlockButtonRef.Button.targetGraphic = unlockButtonRef.LockImg;
			if (tWeaponInfo.netDiveSkillInfo != null)
			{
				unlockButtonRef.BtnImgae.gameObject.SetActive(true);
				unlockButtonRef.LockImg.gameObject.SetActive(false);
				unlockButtonRef.HintObj.SetActive(false);
				SKILL_TABLE value;
				ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(tWeaponInfo.netDiveSkillInfo.SkillID, out value);
				unlockButtonRef.BtnImgae.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON);
				unlockButtonRef.BtnLabel.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
				unlockButtonRef.MsgText.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("RANKING_PERSONAL_LEVEL") + ":1";
				return;
			}
			unlockButtonRef.BtnImgae.gameObject.SetActive(false);
			unlockButtonRef.BtnLabel.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("ARMSSKILL_RANDOM");
			unlockButtonRef.MsgText.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("UI_LOCKED");
			unlockButtonRef.LockImg.gameObject.SetActive(true);
			unlockButtonRef.LockImg.color = Color.white;
			MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tWEAPON_TABLE.n_DIVE_MATERIAL];
			int[] array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
			int[] array3 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
			bool flag = true;
			for (int j = 0; j < array2.Length; j++)
			{
				int num = array2[j];
				if (num != 0)
				{
					ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[num];
					int num2 = 0;
					if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
					{
						num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
					}
					if (num2 < array3[j])
					{
						flag = false;
						break;
					}
				}
			}
			unlockButtonRef.HintObj.SetActive(flag && tWeaponInfo.netInfo.Star >= tWEAPON_TABLE.n_DIVE_UNLOCK);
		}
		else
		{
			if (RSkillRoot != null)
			{
				RSkillRoot.SetActive(false);
			}
			if (SdSkillRoot != null)
			{
				SdSkillRoot.SetActive(true);
			}
		}
	}

	public void CalcuQProfLVRange()
	{
		NetWeaponExpertInfo netWeaponExpertInfo = null;
		for (int i = 0; i < tWeaponInfo.netExpertInfos.Count; i++)
		{
			NetWeaponExpertInfo netWeaponExpertInfo2 = tWeaponInfo.netExpertInfos[i];
			if (netWeaponExpertInfo2.ExpertType == nNowUpgradeType + 1)
			{
				netWeaponExpertInfo = netWeaponExpertInfo2;
				break;
			}
		}
		int nELV = 0;
		if (netWeaponExpertInfo != null)
		{
			nELV = netWeaponExpertInfo.ExpertLevel;
		}
		IEnumerable<UPGRADE_TABLE> source = from obj in ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT.Values.Where(delegate(UPGRADE_TABLE obj)
			{
				if (obj.n_GROUP != tWEAPON_TABLE.n_UPGRADE)
				{
					return false;
				}
				return (obj.n_LV >= nELV) ? true : false;
			})
			orderby obj.n_LV
			select obj;
		int profPoint = ManagedSingleton<PlayerHelper>.Instance.GetProfPoint();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		UPGRADE_TABLE[] array = source.ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			UPGRADE_TABLE uPGRADE_TABLE = source.ElementAt(j);
			if (tWeaponInfo.netInfo.Prof + profPoint < uPGRADE_TABLE.n_PROF + num || ManagedSingleton<PlayerHelper>.Instance.GetZenny() < uPGRADE_TABLE.n_MONEY + num2 || GetNowLvWithAddExp(0).n_ID < uPGRADE_TABLE.n_WEAPON_LV)
			{
				break;
			}
			num += uPGRADE_TABLE.n_PROF;
			num2 += uPGRADE_TABLE.n_MONEY;
			num3 = ((uPGRADE_TABLE.n_PROF != 0 || uPGRADE_TABLE.n_MONEY != 0) ? (j + 1) : j);
		}
		bSliderSE = false;
		ProfLvSlider.minValue = 0f;
		ProfLvSlider.maxValue = num3;
		ProfLvSlider.value = 0f;
		bSliderSE = true;
	}

	public void OnShowQuickProfUp()
	{
		if (!QuickProfUp.activeSelf)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			QuickProfUp.transform.localScale = new Vector3(0f, 0f, 1f);
			ObjScale(0f, 1f, 0.2f, QuickProfUp, null);
			QuickProfUp.SetActive(true);
		}
		int num = tWEAPON_TABLE.n_RARITY;
		if (num > 5)
		{
			num = 5;
		}
		qpnowlvbg.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, "UI_iconsource_powerupicon_Lv" + num);
		qpnextlvbg.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, "UI_iconsource_powerupicon_Lv" + num);
		for (int i = 0; i < tWeaponInfo.netExpertInfos.Count; i++)
		{
			NetWeaponExpertInfo netWeaponExpertInfo = tWeaponInfo.netExpertInfos[i];
			AttriQLvBg[netWeaponExpertInfo.ExpertType - 1].CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, "UI_iconsource_powerupicon_Lv" + num);
			AttriQLvText[netWeaponExpertInfo.ExpertType - 1].text = netWeaponExpertInfo.ExpertLevel.ToString();
		}
		CalcuQProfLVRange();
		nQucikLV = 0;
		GoQuickProfUp.interactable = false;
		InitQProfData();
	}

	public void CloseShowQuickProfUp()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, QuickProfUp, delegate
		{
			QuickProfUp.SetActive(false);
		});
	}

	public void InitQProfData()
	{
		int nELV = 0;
		int nNowLV = 0;
		for (int i = 0; i < tWeaponInfo.netExpertInfos.Count; i++)
		{
			NetWeaponExpertInfo netWeaponExpertInfo = tWeaponInfo.netExpertInfos[i];
			if (netWeaponExpertInfo.ExpertType == nNowUpgradeType + 1)
			{
				qpnowlv.text = netWeaponExpertInfo.ExpertLevel.ToString();
				nNowLV = netWeaponExpertInfo.ExpertLevel;
				nELV = nNowLV + nQucikLV;
				qpnextlv.text = (netWeaponExpertInfo.ExpertLevel + nQucikLV).ToString();
			}
		}
		int[] array = new int[5];
		array[nNowUpgradeType] = nQucikLV;
		WeaponStatus weaponStatusX = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(tWeaponInfo, 0, false, array, delegate(string str)
		{
			DebugInfoText.text = str;
		});
		WeaponStatus wsubstatus = new WeaponStatus();
		PlayerStatus pallstatus = new PlayerStatus();
		int battlePower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(weaponStatusX, wsubstatus, pallstatus, true);
		info2numtext[5].text = battlePower.ToString();
		int[] array2 = new int[5] { nATK, nHP, nCRI, nHIT, nLuk };
		int[] array3 = new int[5] { nATK, nHP, nCRI, nHIT, nLuk };
		if (weaponStatusX != null)
		{
			array3[0] = weaponStatusX.nATK;
			array3[1] = weaponStatusX.nHP;
			array3[2] = weaponStatusX.nCRI;
			array3[3] = weaponStatusX.nHIT;
			array3[4] = weaponStatusX.nLuck;
		}
		string[] array4 = new string[5] { "STATUS_ATK", "STATUS_HP", "STATUS_CRI", "STATUS_HIT", "STATUS_LUK" };
		QAttrNowText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(array4[nNowUpgradeType]);
		info2numtext[6].text = array2[nNowUpgradeType].ToString();
		info2numtext[7].text = array3[nNowUpgradeType].ToString();
		IEnumerable<UPGRADE_TABLE> source = from obj in ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT.Values.Where(delegate(UPGRADE_TABLE obj)
			{
				if (obj.n_GROUP != tWEAPON_TABLE.n_UPGRADE)
				{
					return false;
				}
				if (obj.n_LV > nELV)
				{
					return false;
				}
				return (obj.n_LV >= nNowLV) ? true : false;
			})
			orderby obj.n_LV
			select obj;
		UPGRADE_TABLE uPGRADE_TABLE = null;
		if (source.Count() >= 2)
		{
			uPGRADE_TABLE = source.ElementAt(source.Count() - 2);
		}
		if (uPGRADE_TABLE != null)
		{
			if (GetNowLvWithAddExp(0).n_ID >= uPGRADE_TABLE.n_WEAPON_LV)
			{
				info2qcondition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REQUIRE_WEAPON_LV"), uPGRADE_TABLE.n_WEAPON_LV);
			}
			else
			{
				info2qcondition.text = "<color=#ff0000>" + string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REQUIRE_WEAPON_LV"), uPGRADE_TABLE.n_WEAPON_LV) + "</color>";
			}
		}
		else if (nELV == nNowLV)
		{
			if (!bHasNextLV)
			{
				info2qcondition.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MASTERY_LEVELMAX");
			}
			else if (GetNowLvWithAddExp(0).n_ID < tUPGRADE_TABLE.n_WEAPON_LV)
			{
				info2qcondition.text = "<color=#ff0000>" + string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REQUIRE_WEAPON_LV"), tUPGRADE_TABLE.n_WEAPON_LV) + "</color>";
			}
			else
			{
				info2qcondition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REQUIRE_WEAPON_LV"), tUPGRADE_TABLE.n_WEAPON_LV);
			}
		}
		else
		{
			info2qcondition.text = "";
		}
		info2qcondition.supportRichText = true;
		int num = 0;
		int num2 = 0;
		if (nELV == nNowLV)
		{
			if (bHasNextLV)
			{
				num = tUPGRADE_TABLE.n_PROF;
				num2 = tUPGRADE_TABLE.n_MONEY;
			}
		}
		else
		{
			for (int j = 0; j < source.Count() - 1; j++)
			{
				UPGRADE_TABLE uPGRADE_TABLE2 = source.ElementAt(j);
				num += uPGRADE_TABLE2.n_PROF;
				num2 += uPGRADE_TABLE2.n_MONEY;
			}
		}
		if (tWeaponInfo.netInfo.Prof < num)
		{
			info2numtext[0].text = "<color=#ff0000>" + tWeaponInfo.netInfo.Prof + "</color>";
		}
		else
		{
			info2numtext[0].text = tWeaponInfo.netInfo.Prof.ToString();
		}
		if (ManagedSingleton<PlayerHelper>.Instance.GetZenny() < num2)
		{
			info2numtext[1].text = "<color=#ff0000>" + ManagedSingleton<PlayerHelper>.Instance.GetZenny() + "</color>";
		}
		else
		{
			info2numtext[1].text = ManagedSingleton<PlayerHelper>.Instance.GetZenny().ToString();
		}
		info2numtext[2].text = num.ToString();
		info2numtext[3].text = num2.ToString();
		qProftitlemsg.text = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("TARGET_LV"), "<color=#4eff00>" + nQucikLV + "</color>", Mathf.Round(ProfLvSlider.maxValue));
		bSliderSE = false;
		ProfLvSlider.value = nQucikLV;
		bSliderSE = true;
		GoQuickProfUp.interactable = nQucikLV > 0;
	}

	public void OnAddQProfUP()
	{
		if ((float)nQucikLV < ProfLvSlider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV++;
			InitQProfData();
		}
	}

	public void OnDecreaseQProfUP()
	{
		if (nQucikLV > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV--;
			InitQProfData();
		}
	}

	public void OnMaxQProfUP()
	{
		if ((float)nQucikLV != ProfLvSlider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV = (int)ProfLvSlider.maxValue;
			InitQProfData();
		}
	}

	public void OnMinQProfUP()
	{
		if (nQucikLV != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV = 0;
			InitQProfData();
		}
	}

	public void OnQProfSilderChange(float value)
	{
		if ((int)Mathf.Round(ProfLvSlider.value) != nQucikLV)
		{
			nQucikLV = (int)Mathf.Round(ProfLvSlider.value);
			if (bSliderSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			}
			InitQProfData();
		}
	}

	public void OnUpgradeTypeGo()
	{
		if (nQucikLV == 0)
		{
			return;
		}
		NetWeaponExpertInfo netWeaponExpertInfo = null;
		for (int i = 0; i < tWeaponInfo.netExpertInfos.Count; i++)
		{
			NetWeaponExpertInfo netWeaponExpertInfo2 = tWeaponInfo.netExpertInfos[i];
			if (netWeaponExpertInfo2.ExpertType == nNowUpgradeType + 1)
			{
				netWeaponExpertInfo = netWeaponExpertInfo2;
				break;
			}
		}
		int nELV = 0;
		if (netWeaponExpertInfo != null)
		{
			nELV = netWeaponExpertInfo.ExpertLevel;
		}
		IEnumerable<UPGRADE_TABLE> source = from obj in ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT.Values.Where(delegate(UPGRADE_TABLE obj)
			{
				if (obj.n_GROUP != tWEAPON_TABLE.n_UPGRADE)
				{
					return false;
				}
				return (obj.n_LV >= nELV && obj.n_LV <= nELV + nQucikLV) ? true : false;
			})
			orderby obj.n_LV
			select obj;
		int nTotalProf = 0;
		List<int> list = new List<int>();
		for (int j = 0; j < source.Count(); j++)
		{
			UPGRADE_TABLE uPGRADE_TABLE = source.ElementAt(j);
			if (j < source.Count() - 1)
			{
				list.Add(source.ElementAt(j).n_ID);
				nTotalProf += uPGRADE_TABLE.n_PROF;
			}
		}
		if (tWeaponInfo.netInfo.Prof < nTotalProf)
		{
			ObjScale(1f, 0f, 0.2f, QuickProfUp, delegate
			{
				QuickProfUp.SetActive(false);
				ShowAutoChangeProf(nTotalProf);
			});
			return;
		}
		AdvancedBtn.interactable = false;
		GoQuickProfUp.interactable = false;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<PlayerNetManager>.Instance.WeaponExpertUpReq(nTargetWeaponID, (WeaponExpertType)(nNowUpgradeType + 1), list, delegate
		{
			SortWeaponList();
			listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
			listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addLVUp);
			PlayUpgradeEffect(AttriQSBG[nNowUpgradeType].transform.position);
			nNowWeaponID = -1;
			AdvancedBtn.interactable = true;
			InitWeapon();
			nQucikLV = 0;
			OnShowQuickProfUp();
		});
	}

	public void InitQSkillData()
	{
		SKILL_TABLE value;
		ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nNowSelectAutoSkill, out value);
		List<NetWeaponSkillInfo> netSkillInfos = tWeaponInfo.netSkillInfos;
		int num = 0;
		if (netSkillInfos != null)
		{
			for (int i = 0; i < netSkillInfos.Count; i++)
			{
				if (netSkillInfos[i].Slot == nNowSelectSkillIndex + 1)
				{
					num = netSkillInfos[i].Level;
				}
			}
		}
		int num2 = 0;
		int num3 = 0;
		EXP_TABLE value2 = null;
		int skillPoint = ManagedSingleton<PlayerHelper>.Instance.GetSkillPoint();
		int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		while (true)
		{
			num3 = num + num2 + 1;
			if (GetNowLvWithAddExp(0).n_ID < num3 || value.n_LVMAX < num3)
			{
				break;
			}
			if (!ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(num + num2, out value2))
			{
				value2 = new EXP_TABLE();
				value2.n_SKILLUP_SP = 999999;
				value2.n_SKILLUP_MONEY = 999999;
			}
			if (num4 + value2.n_SKILLUP_SP > skillPoint || num5 + value2.n_SKILLUP_MONEY > zenny)
			{
				break;
			}
			num2++;
			num4 += value2.n_SKILLUP_SP;
			num5 += value2.n_SKILLUP_MONEY;
			if (nQucikLV >= num2)
			{
				num6 += value2.n_SKILLUP_SP;
				num7 += value2.n_SKILLUP_MONEY;
			}
		}
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
		int n_EFFECT = value.n_EFFECT;
		if (n_EFFECT == 1 || n_EFFECT != 3)
		{
			float num8 = value.f_EFFECT_X + (float)(num + nQucikLV) * value.f_EFFECT_Y;
			QSkillScribt.text = string.Format(l10nValue, num8.ToString("0.00"));
		}
		else
		{
			float num8 = value.f_EFFECT_Y + value.f_EFFECT_Z * (float)(num + nQucikLV);
			QSkillScribt.text = string.Format(l10nValue, num8.ToString("0"));
		}
		QSkillMsgText.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("RANKING_PERSONAL_LEVEL") + ":" + num;
		string format = "{0}";
		string format2 = "{0}";
		if (num2 == 0)
		{
			if (GetNowLvWithAddExp(0).n_ID < num + 1)
			{
				qlvcondition.text = "<color=#ff0000>" + string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_WEAPON_UPGRADE"), num + 1) + "</color>";
			}
			else if (value.n_LVMAX < num + 1)
			{
				qlvcondition.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MASTERY_SKILL_LEVELMAX");
			}
			else if (value2 != null && (value2.n_SKILLUP_SP > skillPoint || value2.n_SKILLUP_MONEY > zenny))
			{
				num6 += value2.n_SKILLUP_SP;
				num7 += value2.n_SKILLUP_MONEY;
				if (value2.n_SKILLUP_SP > skillPoint)
				{
					format = "<color=#ff0000>{0}</color>";
				}
				if (value2.n_SKILLUP_MONEY > zenny)
				{
					format2 = "<color=#ff0000>{0}</color>";
				}
			}
			else
			{
				qlvcondition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_WEAPON_UPGRADE"), num + nQucikLV);
			}
		}
		else
		{
			qlvcondition.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_WEAPON_UPGRADE"), num + nQucikLV);
		}
		qlvtitlemsg.text = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("TARGET_LV"), "<color=#4eff00>" + nQucikLV + "</color>", num2);
		SkillLvSlider.minValue = 0f;
		SkillLvSlider.maxValue = num2;
		SkillLvSlider.value = nQucikLV;
		info4numtext[0].text = string.Format(format, skillPoint);
		info4numtext[1].text = string.Format(format2, zenny);
		info4numtext[2].text = num6.ToString();
		info4numtext[3].text = num7.ToString();
		GoQuickSkillUp.interactable = nQucikLV > 0;
	}

	public void OnShowQSkillUp()
	{
		QuickSkillUp.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, QuickSkillUp, null);
		QuickSkillUp.SetActive(true);
		SKILL_TABLE value;
		ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nNowSelectAutoSkill, out value);
		StageLoadIcon[] qSkillImage = QSkillImage;
		for (int i = 0; i < qSkillImage.Length; i++)
		{
			qSkillImage[i].CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON);
		}
		QSkillText.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
		nQucikLV = 0;
		GoQuickSkillUp.interactable = false;
		InitQSkillData();
	}

	public void OnAddQSkillLV()
	{
		if ((float)nQucikLV < SkillLvSlider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV++;
			InitQSkillData();
		}
	}

	public void OnDecreaseQSkillLV()
	{
		if (nQucikLV > 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV--;
			InitQSkillData();
		}
	}

	public void OnMaxQSkillLV()
	{
		if ((float)nQucikLV != SkillLvSlider.maxValue)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV = (int)SkillLvSlider.maxValue;
			InitQSkillData();
		}
	}

	public void OnMinQSkillLV()
	{
		if (nQucikLV != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			nQucikLV = 0;
			InitQSkillData();
		}
	}

	public void OnQSkillSilderChange(float value)
	{
		if ((int)Mathf.Round(SkillLvSlider.value) != nQucikLV)
		{
			nQucikLV = (int)Mathf.Round(SkillLvSlider.value);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickExpItem);
			InitQSkillData();
		}
	}

	public void CloseShowQSkillUp()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, QuickSkillUp, delegate
		{
			QuickSkillUp.SetActive(false);
		});
	}

	public void OnQSkillUp()
	{
		int num = 0;
		List<NetWeaponSkillInfo> netSkillInfos = tWeaponInfo.netSkillInfos;
		if (netSkillInfos != null)
		{
			for (int i = 0; i < netSkillInfos.Count; i++)
			{
				if (netSkillInfos[i].Slot == nNowSelectSkillIndex + 1)
				{
					num = netSkillInfos[i].Level;
				}
			}
		}
		SkillLvUpBtninfo4.interactable = false;
		QSkillLvUpBtninfo4.interactable = false;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<PlayerNetManager>.Instance.WeaponSkillUpReq(nTargetWeaponID, (WeaponSkillSlot)(nNowSelectSkillIndex + 1), num + nQucikLV, delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addLVUp);
			int nBtnID = nNowSelectSkillIndex;
			nNowWeaponID = -1;
			SkillLvUpBtninfo4.interactable = false;
			QSkillLvUpBtninfo4.interactable = false;
			InitWeapon();
			OnSelectAutoSkill(nBtnID);
			nQucikLV = 0;
			InitQSkillData();
			PlayUpgradeEffect(QSkillText.transform.position);
		});
	}

	public void OnNoQuickItemCB()
	{
		ITEM_TABLE item;
		if (!ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(OrangeConst.ARMSSKILL_UNLOCK_ITEM, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.Setup(item, null, OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
			{
				InitWeapon();
			});
		});
	}

	public void OnShowRSkillAll()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ShowRSkillRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, ShowRSkillRoot, null);
		ShowRSkillRoot.SetActive(true);
		int num = 0;
		if (tWEAPON_TABLE.n_DIVE != 0)
		{
			num = ManagedSingleton<OrangeDataManager>.Instance.RANDOMSKILL_TABLE_DICT.Values.Where((RANDOMSKILL_TABLE obj) => obj.n_GROUP == tWEAPON_TABLE.n_DIVE).Count();
		}
		num = ((num % 2 != 0) ? ((num - num % 2) / 2 + 1) : (num / 2));
		tInfo4LVSR.totalCount = num;
		tInfo4LVSR.RefillCells();
	}

	public void CloseShowRSkillAll()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, ShowRSkillRoot, delegate
		{
			ShowRSkillRoot.SetActive(false);
		});
	}

	public void OnQChangeRSkill()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		QChangeRSkillRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, QChangeRSkillRoot, null);
		QChangeRSkillRoot.SetActive(true);
		ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[OrangeConst.ARMSSKILL_UNLOCK_ITEM];
		int num = 0;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
		{
			num = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
		}
		for (int i = 0; i < QChangeRSkillItem.Length; i++)
		{
			ExpButtonRef expButtonRef = QChangeRSkillItem[i];
			expButtonRef.Button.gameObject.SetActive(true);
			UpdateItemNeedInfo(iTEM_TABLE, expButtonRef.BtnImgae, expButtonRef.frmimg, expButtonRef.bgimg, null);
			expButtonRef.UnuseBtn.gameObject.SetActive(false);
			expButtonRef.BtnLabel.text = num + "/" + OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER;
			if (num >= OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER)
			{
				expButtonRef.AddBtn.gameObject.SetActive(false);
				expButtonRef.BtnLabel.color = Color.white;
				expButtonRef.Button.interactable = true;
				expButtonRef.frmimg.color = Color.white;
				expButtonRef.bgimg.color = Color.white;
			}
			else
			{
				expButtonRef.AddBtn.gameObject.SetActive(true);
				expButtonRef.BtnLabel.color = Color.red;
				expButtonRef.Button.interactable = false;
				expButtonRef.frmimg.color = disablecolor;
				expButtonRef.bgimg.color = disablecolor;
			}
		}
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(iTEM_TABLE.w_NAME);
		QChangeRSkillText.text = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("ARMSSKILL_WARN_1"), OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER, l10nValue) + "\n" + ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("BATTLE_CONTINUE_ITEM_COUNT") + ":<color=#0080C4>" + num + "</color>";
	}

	public void CloseQChangeRSkill()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, QChangeRSkillRoot, delegate
		{
			QChangeRSkillRoot.SetActive(false);
		});
	}

	public void QChangeRSkillGo()
	{
		ManagedSingleton<PlayerNetManager>.Instance.WeaponRandomRSkill(nTargetWeaponID, true, delegate
		{
			ObjScale(1f, 0f, 0.2f, QChangeRSkillRoot, delegate
			{
				QChangeRSkillRoot.SetActive(false);
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
			int nBtnID = nNowSelectSkillIndex;
			nNowWeaponID = -1;
			InitWeapon();
			OnSelectAutoSkill(nBtnID);
		});
	}

	public void OnReChangeRSkill()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		ReChangeRSkillRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, ReChangeRSkillRoot, null);
		ReChangeRSkillRoot.SetActive(true);
		int[] array = new int[2]
		{
			tWeaponInfo.netDiveSkillInfo.SkillID,
			tWeaponInfo.netDiveSkillInfo.PulledSkillID
		};
		string[] array2 = new string[2];
		for (int i = 0; i < 2; i++)
		{
			SKILL_TABLE value;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(array[i], out value);
			RSkillImg[i].CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON);
			array2[i] = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			RSkillText[i].text = array2[i];
			array2[i] = "<color=#0080C4>" + array2[i] + "</color>";
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
			float num = 100f;
			int n_EFFECT = value.n_EFFECT;
			if (n_EFFECT == 1)
			{
				num = value.f_EFFECT_X + 0f * value.f_EFFECT_Y;
			}
			RSkillMsgText[i].text = string.Format(l10nValue.Replace("{0}", "{0:0.00}"), num);
		}
		ChangeDesc.text = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("ARMSSKILL_WARN_2"), array2[0], array2[1]);
	}

	public void ReChangeRSkillGo()
	{
		ManagedSingleton<PlayerNetManager>.Instance.WeaponChangeRSkill(nTargetWeaponID, true, delegate
		{
			ObjScale(1f, 0f, 0.2f, ReChangeRSkillRoot, delegate
			{
				ReChangeRSkillRoot.SetActive(false);
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
			int nBtnID = nNowSelectSkillIndex;
			nNowWeaponID = -1;
			InitWeapon();
			OnSelectAutoSkill(nBtnID);
		});
	}

	public void CloseReChangeRSkill()
	{
		ManagedSingleton<PlayerNetManager>.Instance.WeaponChangeRSkill(nTargetWeaponID, false, delegate
		{
			ObjScale(1f, 0f, 0.2f, ReChangeRSkillRoot, delegate
			{
				ReChangeRSkillRoot.SetActive(false);
			});
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
			int nBtnID = nNowSelectSkillIndex;
			nNowWeaponID = -1;
			InitWeapon();
			OnSelectAutoSkill(nBtnID);
		});
	}

	public void OnCheckReChangeRSkill(int nBtnValue)
	{
		CheckReChangeRSkillRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, CheckReChangeRSkillRoot, null);
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		CheckReChangeRSkillRoot.SetActive(true);
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tWEAPON_TABLE.n_DIVE_MATERIAL];
		int[] array = null;
		int[] array2 = null;
		if (nBtnValue == 1)
		{
			array = new int[5]
			{
				OrangeConst.ARMSSKILL_UNLOCK_ITEM,
				0,
				0,
				0,
				0
			};
			array2 = new int[5]
			{
				OrangeConst.ARMSSKILL_UNLOCK_ITEMNUMBER,
				0,
				0,
				0,
				0
			};
			bUseSpecialItem = true;
		}
		else
		{
			array = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
			array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
			bUseSpecialItem = false;
		}
		for (int i = 0; i < checkskillmaterials.Length; i++)
		{
			ExpButtonRef expButtonRef = checkskillmaterials[i];
			int num = array[i];
			if (num == 0)
			{
				expButtonRef.Button.gameObject.SetActive(false);
				continue;
			}
			expButtonRef.Button.gameObject.SetActive(true);
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[num];
			UpdateItemNeedInfo(iTEM_TABLE, expButtonRef.BtnImgae, expButtonRef.frmimg, expButtonRef.bgimg, null);
			expButtonRef.UnuseBtn.gameObject.SetActive(false);
			int num2 = 0;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
			{
				num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
			}
			expButtonRef.BtnLabel.text = num2 + "/" + array2[i];
			if (num2 >= array2[i])
			{
				expButtonRef.AddBtn.gameObject.SetActive(false);
				expButtonRef.BtnLabel.color = Color.white;
				expButtonRef.Button.interactable = true;
				expButtonRef.frmimg.color = Color.white;
				expButtonRef.bgimg.color = Color.white;
			}
			else
			{
				expButtonRef.AddBtn.gameObject.SetActive(true);
				expButtonRef.BtnLabel.color = Color.red;
				expButtonRef.Button.interactable = false;
				expButtonRef.frmimg.color = disablecolor;
				expButtonRef.bgimg.color = disablecolor;
			}
		}
	}

	public void CheckReChangeRSkillGo()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<PlayerNetManager>.Instance.WeaponRandomRSkill(nTargetWeaponID, bUseSpecialItem, delegate
		{
			if (tWeaponInfo.netDiveSkillInfo.PulledSkillID != 0)
			{
				ObjScale(1f, 0f, 0.2f, CheckReChangeRSkillRoot, delegate
				{
					CheckReChangeRSkillRoot.SetActive(false);
					OnReChangeRSkill();
				});
			}
			else
			{
				ObjScale(1f, 0f, 0.2f, CheckReChangeRSkillRoot, delegate
				{
					CheckReChangeRSkillRoot.SetActive(false);
				});
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
				int nBtnID = nNowSelectSkillIndex;
				nNowWeaponID = -1;
				InitWeapon();
				OnSelectAutoSkill(nBtnID);
			}
		});
	}

	public void CloseCheckReChangeRSkill()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, CheckReChangeRSkillRoot, delegate
		{
			CheckReChangeRSkillRoot.SetActive(false);
		});
	}

	private void GetUpdrageTypeDict(int nType, bool bGreen)
	{
		Dictionary<int, UPGRADE_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT.GetEnumerator();
		NetWeaponExpertInfo netWeaponExpertInfo = null;
		for (int i = 0; i < tWeaponInfo.netExpertInfos.Count; i++)
		{
			NetWeaponExpertInfo netWeaponExpertInfo2 = tWeaponInfo.netExpertInfos[i];
			if (netWeaponExpertInfo2.ExpertType == nType + 1)
			{
				netWeaponExpertInfo = netWeaponExpertInfo2;
				break;
			}
		}
		int num = 0;
		tUPGRADE_TABLE = null;
		if (netWeaponExpertInfo != null)
		{
			num = netWeaponExpertInfo.ExpertLevel;
		}
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.n_GROUP == tWEAPON_TABLE.n_UPGRADE && enumerator.Current.Value.n_LV == num)
			{
				tUPGRADE_TABLE = enumerator.Current.Value;
				break;
			}
		}
		enumerator.Dispose();
		enumerator = ManagedSingleton<OrangeDataManager>.Instance.UPGRADE_TABLE_DICT.GetEnumerator();
		bHasNextLV = false;
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.n_GROUP == tWEAPON_TABLE.n_UPGRADE && enumerator.Current.Value.n_LV == num + 1)
			{
				bHasNextLV = true;
				break;
			}
		}
		if (tUPGRADE_TABLE == null)
		{
			tUPGRADE_TABLE = new UPGRADE_TABLE();
			Debug.LogError("ExpertType = " + (nType + 1) + " LV = " + num);
		}
		int num2;
		for (num2 = tUPGRADE_TABLE.n_LV; num2 > 10; num2 -= 10)
		{
		}
		int j;
		for (j = 0; j < num2; j++)
		{
			info2attrbargreen[nType][j].SetActive(tUPGRADE_TABLE.n_LV <= 10);
			info2attrbarpurple[nType][j].SetActive(tUPGRADE_TABLE.n_LV > 10);
		}
		for (; j < info2attrbargreen[nType].Length; j++)
		{
			info2attrbargreen[nType][j].SetActive(tUPGRADE_TABLE.n_LV >= 10);
			info2attrbarpurple[nType][j].SetActive(false);
		}
		AttriBtn[nType].interactable = bGreen;
		AttriSBG[nType].SetActive(!bGreen);
		info2attrarrow[nType].SetActive(!bGreen);
		info2nsbg[nType].SetActive(bGreen);
		if (AttriQBg != null && AttriQBg.Length > nType)
		{
			AttriQBg[nType].SetActive(!bGreen);
		}
		if (AttriQSBG != null && AttriQSBG.Length > nType)
		{
			AttriQSBG[nType].SetActive(!bGreen);
		}
		if (QuickProfUp != null && QuickProfUp.activeSelf)
		{
			nQucikLV = 0;
			CalcuQProfLVRange();
			InitQProfData();
		}
	}

	private void ShowAutoChangeProf(int nProf)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		bIgnoreFristSE = true;
		AutoProfChangeRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, AutoProfChangeRoot, null);
		AutoProfChangeRoot.SetActive(true);
		InitChangeProf(nProf - tWeaponInfo.netInfo.Prof, false);
		bIgnoreFristSE = false;
	}

	public void OnUpgradeType()
	{
		OnShowQuickProfUp();
	}

	private void ReSetStar(int nStar, bool bShowAdd = false)
	{
		if (TopNoStar == null || TopOnStar == null || TopAddStar == null || TopNoStar.Length != TopOnStar.Length || TopNoStar.Length != TopAddStar.Length)
		{
			return;
		}
		for (int i = 0; i < nStar; i++)
		{
			TopNoStar[i].SetActive(false);
			TopOnStar[i].SetActive(true);
			TopAddStar[i].SetActive(false);
		}
		for (int j = nStar; j < 5; j++)
		{
			TopNoStar[j].SetActive(true);
			TopOnStar[j].SetActive(false);
			TopAddStar[j].SetActive(false);
		}
		if (bShowAdd && nStar < 5)
		{
			TopAddStar[nStar].SetActive(true);
		}
		int num = Star.Length;
		for (int k = 0; k < num; k++)
		{
			if (nStar > k)
			{
				Star[k].SetActive(true);
				NoStar[k].SetActive(false);
				AddStar[k].SetActive(false);
			}
			else
			{
				Star[k].SetActive(false);
				NoStar[k].SetActive(true);
				AddStar[k].SetActive(false);
			}
		}
		if (bShowAdd && nStar < 5)
		{
			AddStar[nStar].SetActive(true);
		}
	}

	private bool CheckMaterialIsOK(int nMaterialID)
	{
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[nMaterialID];
		int[] array = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		bool result = true;
		for (int i = 0; i < skillmaterials.Length; i++)
		{
			int num = array[i];
			if (num != 0)
			{
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[num];
				int num2 = 0;
				ItemInfo value;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.TryGetValue(iTEM_TABLE.n_ID, out value))
				{
					num2 = value.netItemInfo.Stack;
				}
				if (num2 < array2[i])
				{
					result = false;
				}
			}
		}
		return result;
	}

	private bool CheckSkillLvUP(int nLV)
	{
		EXP_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(nLV, out value))
		{
			value = new EXP_TABLE();
			value.n_SKILLUP_SP = 999999;
			value.n_SKILLUP_MONEY = 999999;
		}
		int skillPoint = ManagedSingleton<PlayerHelper>.Instance.GetSkillPoint();
		int zenny = ManagedSingleton<PlayerHelper>.Instance.GetZenny();
		if (skillPoint > value.n_SKILLUP_SP && zenny > value.n_SKILLUP_MONEY)
		{
			return true;
		}
		return false;
	}

	private void InitInfo4Autoskill(int nautoskill, int nskillid)
	{
		UnlockButtonRef unlockButtonRef = null;
		unlockButtonRef = ((tWEAPON_TABLE.n_DIVE != 0) ? rskillitems[nautoskill] : skillitems[nautoskill]);
		SKILL_TABLE value;
		if (nskillid > 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nskillid, out value))
		{
			List<NetWeaponSkillInfo> netSkillInfos = tWeaponInfo.netSkillInfos;
			bool flag = false;
			int num = 0;
			if (netSkillInfos != null)
			{
				for (int i = 0; i < netSkillInfos.Count; i++)
				{
					if (netSkillInfos[i].Slot == nautoskill + 1)
					{
						flag = true;
						num = netSkillInfos[i].Level;
					}
				}
			}
			unlockButtonRef.BtnImgae.gameObject.SetActive(true);
			unlockButtonRef.BtnImgae.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON);
			unlockButtonRef.BtnLabel.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			unlockButtonRef.HintObj.SetActive(false);
			if (flag)
			{
				unlockButtonRef.Button.targetGraphic = unlockButtonRef.Button.GetComponent<Image>();
				unlockButtonRef.LockImg.gameObject.SetActive(false);
				unlockButtonRef.MsgText.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("RANKING_PERSONAL_LEVEL") + ":" + num;
				if (value.n_LVMAX > num && GetNowLvWithAddExp(0).n_ID >= num + 1 && CheckSkillLvUP(num))
				{
					unlockButtonRef.HintObj.SetActive(true);
				}
				unlockButtonRef.BtnImgae.transform.SetParent(unlockButtonRef.Button.transform, true);
				return;
			}
			unlockButtonRef.MsgText.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue("UI_LOCKED");
			unlockButtonRef.LockImg.gameObject.SetActive(true);
			unlockButtonRef.LockImg.color = Color.white;
			unlockButtonRef.Button.targetGraphic = unlockButtonRef.LockImg;
			int[] array = new int[6] { tWEAPON_TABLE.n_PASSIVE_MATERIAL1, tWEAPON_TABLE.n_PASSIVE_MATERIAL2, tWEAPON_TABLE.n_PASSIVE_MATERIAL3, tWEAPON_TABLE.n_PASSIVE_MATERIAL4, tWEAPON_TABLE.n_PASSIVE_MATERIAL5, tWEAPON_TABLE.n_PASSIVE_MATERIAL6 };
			if (CheckMaterialIsOK(array[nautoskill]))
			{
				unlockButtonRef.HintObj.SetActive(true);
			}
			unlockButtonRef.BtnImgae.transform.SetParent(unlockButtonRef.LockImg.transform, true);
			unlockButtonRef.BtnImgae.transform.SetSiblingIndex(0);
		}
		else
		{
			unlockButtonRef.BtnImgae.gameObject.SetActive(false);
			unlockButtonRef.BtnLabel.text = "----";
			unlockButtonRef.MsgText.text = "----";
			unlockButtonRef.LockImg.gameObject.SetActive(true);
			unlockButtonRef.LockImg.color = Color.gray;
			unlockButtonRef.Button.targetGraphic = unlockButtonRef.LockImg;
			unlockButtonRef.HintObj.SetActive(false);
		}
	}

	private void InitInfo5()
	{
		if (tWeaponInfo.netInfo.Chip == 0)
		{
			for (int i = 0; i < 3; i++)
			{
				chipvalue[i].text = "";
				chipBar[i].SetFValue(0f);
			}
			for (int num = chiproot0.transform.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(chiproot0.transform.GetChild(num).gameObject);
			}
			chipskillname.transform.gameObject.SetActive(false);
			changechipbtntext.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_EQUIP");
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID))
			{
				changechipbtn.interactable = true;
			}
			else
			{
				changechipbtn.interactable = false;
			}
			return;
		}
		changechipbtntext.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_CHANGE");
		if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nTargetWeaponID))
		{
			changechipbtn.interactable = true;
		}
		else
		{
			changechipbtn.interactable = false;
		}
		for (int num2 = chiproot0.transform.childCount - 1; num2 >= 0; num2--)
		{
			UnityEngine.Object.Destroy(chiproot0.transform.GetChild(num2).gameObject);
		}
		CommonIconBase component = UnityEngine.Object.Instantiate(refWeaponIconBase, chiproot0.transform).GetComponent<CommonIconBase>();
		int num3 = 0;
		DISC_TABLE value;
		ChipInfo value2;
		if (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(tWeaponInfo.netInfo.Chip, out value) && ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(tWeaponInfo.netInfo.Chip, out value2))
		{
			component.Setup(0, AssetBundleScriptableObject.Instance.m_iconChip, value.s_ICON);
			component.SetOtherInfo(value2.netChipInfo);
			WeaponStatus chipStatusX = ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(value2, 0, false, false, null, true);
			chipvalue[0].text = chipStatusX.nATK.ToString();
			chipvalue[1].text = chipStatusX.nHP.ToString();
			chipvalue[2].text = chipStatusX.nDEF.ToString();
			chipBar[0].SetFValue((float)(int)chipStatusX.nATK / (float)OrangeConst.DISC_ATK_MAX);
			chipBar[1].SetFValue((float)(int)chipStatusX.nHP / (float)OrangeConst.DISC_HP_MAX);
			chipBar[2].SetFValue((float)(int)chipStatusX.nDEF / (float)OrangeConst.DISC_DEF_MAX);
			num3 = value2.netChipInfo.Star;
		}
		int[] array = new int[10] { value.n_SKILL_0, value.n_SKILL_1, value.n_SKILL_2, value.n_SKILL_3, value.n_SKILL_4, value.n_SKILL_5, value.n_SKILL_6, 0, 0, 0 };
		SKILL_TABLE value3;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(array[num3], out value3))
		{
			chipskillname.transform.gameObject.SetActive(true);
			chipskillname.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value3.w_NAME);
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value3.w_TIP);
			float num4 = 100f;
			int n_EFFECT = value3.n_EFFECT;
			if (n_EFFECT == 1)
			{
				num4 = value3.f_EFFECT_X + 0f * value3.f_EFFECT_Y;
			}
			chipskilldesc.text = string.Format(l10nValue.Replace("{0}", "{0:0.00}"), num4);
			chipskillimg.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetShowcase(value3.s_SHOWCASE), value3.s_SHOWCASE);
		}
	}

	public void OnGoToChipInfoUI()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		if (tWeaponInfo.netInfo.Chip != 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CHIPINFO", delegate(ChipInfoUI ui)
			{
				ui.bNeedInitList = true;
				ui.nTargetChipID = tWeaponInfo.netInfo.Chip;
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(InitInfo5));
				linkUI = ui;
			});
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CHIPMAIN", delegate(ChipMainUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(InitInfo5));
				linkUI = ui;
			});
		}
	}

	public void ChangeWeaponChip()
	{
		IEnumerable<KeyValuePair<int, ChipInfo>> source = ManagedSingleton<PlayerNetManager>.Instance.dicChip.Where((KeyValuePair<int, ChipInfo> obj) => (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[obj.Key].n_WEAPON_TYPE & tWEAPON_TABLE.n_TYPE) != 0);
		ChipSelectRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, ChipSelectRoot, null);
		ChipSelectRoot.SetActive(true);
		int num = (source.Count() - source.Count() % 5) / 5;
		if (source.Count() % 5 > 0)
		{
			num++;
		}
		nTmpChipSet = tWeaponInfo.netInfo.Chip;
		refPerWeaponChipSelectCell = null;
		tInfo5LVSR.totalCount = num;
		tInfo5LVSR.RefillCells();
	}

	public void ChangeWeaponChipGo()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
		ObjScale(1f, 0f, 0.2f, ChipSelectRoot, delegate
		{
			ChipSelectRoot.SetActive(false);
		});
		NetWeaponInfo netInfo = tWeaponInfo.netInfo;
		if (netInfo.Chip != nTmpChipSet)
		{
			ChipSetState nSetState = ChipSetState.Set;
			int chip = nTmpChipSet;
			if (chip == 0 && netInfo.Chip != 0)
			{
				chip = netInfo.Chip;
				nSetState = ChipSetState.NotSet;
			}
			ManagedSingleton<PlayerNetManager>.Instance.WeaponChipSetReq(nTargetWeaponID, chip, nSetState, delegate
			{
				nNowWeaponID = -1;
				InitWeapon();
			});
		}
	}

	public void CloseChangeWeaponChip()
	{
		ObjScale(1f, 0f, 0.2f, ChipSelectRoot, delegate
		{
			ChipSelectRoot.SetActive(false);
		});
	}

	public void ShowDetailPopop()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		detailpopop.transform.localScale = new Vector3(0f, 0f, 1f);
		ObjScale(0f, 1f, 0.2f, detailpopop, null);
		detailpopop.SetActive(true);
	}

	public void OnCloseDetailPopup()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		ObjScale(1f, 0f, 0.2f, detailpopop, delegate
		{
			detailpopop.SetActive(false);
		});
	}

	private bool GetWeaponStatus()
	{
		nHP = 0;
		nATK = 0;
		nCRI = 0;
		nHIT = 0;
		nLuk = 0;
		DebugInfoText.text = "";
		tWEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[nTargetWeaponID];
		tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[tWEAPON_TABLE.n_SKILL];
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(nTargetWeaponID, out tWeaponInfo))
		{
			tWeaponInfo = new WeaponInfo
			{
				netInfo = new NetWeaponInfo
				{
					WeaponID = nTargetWeaponID
				},
				netExpertInfos = new List<NetWeaponExpertInfo>(),
				netSkillInfos = new List<NetWeaponSkillInfo>()
			};
		}
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(nTotalExpItemAddExp);
		NetWeaponInfo netInfo = tWeaponInfo.netInfo;
		nNowExp = nTotalExpItemAddExp + netInfo.Exp - (nowLvWithAddExp.n_TOTAL_WEAPONEXP - nowLvWithAddExp.n_WEAPONEXP);
		nNeedExp = nowLvWithAddExp.n_WEAPONEXP;
		nExpNowLV = nowLvWithAddExp.n_ID;
		nNowStar = netInfo.Star;
		Dictionary<int, STAR_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext())
		{
			STAR_TABLE value = enumerator.Current.Value;
			if (value.n_TYPE == 2 && value.n_MAINID == nTargetWeaponID && nNowStar == value.n_STAR)
			{
				tSTAR_TABLE = value;
				break;
			}
		}
		WeaponStatus weaponStatusX = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(tWeaponInfo, 0, false, null, delegate(string str)
		{
			DebugInfoText.text = str;
		});
		nHP = weaponStatusX.nHP;
		nATK = weaponStatusX.nATK;
		nCRI = weaponStatusX.nCRI;
		nHIT = weaponStatusX.nHIT;
		nLuk = weaponStatusX.nLuck;
		return true;
	}

	public static int SortByScore(UPGRADE_TABLE p1, UPGRADE_TABLE p2)
	{
		return p1.n_LV.CompareTo(p2.n_LV);
	}

	public override void OnClickCloseBtn()
	{
		if (WeaponModel != null)
		{
			WeaponModel.transform.gameObject.SetActive(false);
		}
		base.OnClickCloseBtn();
		ReleaseMemory();
	}

	private void ReleaseMemory()
	{
		if (linkUI != null)
		{
			OrangeUIBase orangeUIBase = linkUI;
			orangeUIBase.closeCB = (Callback)Delegate.Remove(orangeUIBase.closeCB, new Callback(InitInfo5));
		}
		if (textureObj != null)
		{
			UnityEngine.Object.Destroy(textureObj.gameObject);
			textureObj = null;
		}
		tLHSR.StopAllCoroutines();
		tLHSR.enabled = false;
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (textureObj != null)
		{
			textureObj.SetCameraActive(enable);
			weaponModelCanvas.enabled = enable;
		}
	}

	private IEnumerator ObjScaleCoroutine(float fStart, float fEnd, float fTime, GameObject tObj, Action endcb)
	{
		fNowValue = fStart;
		float fLeftTime = fTime;
		float fD = (fEnd - fStart) / fTime;
		Vector3 nowScale = new Vector3(fNowValue, fNowValue, 1f);
		tObj.transform.localScale = nowScale;
		UnityEngine.Transform tBGClick = tObj.transform.Find("BGClick");
		if (tBGClick != null)
		{
			tBGClick.gameObject.SetActive(false);
		}
		while (fLeftTime > 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			float deltaTime = Time.deltaTime;
			fLeftTime -= deltaTime;
			fNowValue += fD * deltaTime;
			nowScale.x = fNowValue;
			nowScale.y = fNowValue;
			tObj.transform.localScale = nowScale;
		}
		nowScale.x = fEnd;
		nowScale.y = fEnd;
		tObj.transform.localScale = nowScale;
		if (tBGClick != null)
		{
			tBGClick.gameObject.SetActive(true);
		}
		if (fEnd == 0f)
		{
			UnityEngine.Transform transform = base.gameObject.transform.Find("BGBanClick");
			if (transform != null)
			{
				transform.gameObject.SetActive(false);
			}
		}
		if (endcb != null)
		{
			endcb();
		}
	}

	public void onClickBookBtn()
	{
		OnOpenGalleryTargetUI();
	}

	private void PlayUpgradeEffect(Vector3 effectPos)
	{
		if (m_upgradeEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgradeeffect", "UpgradeEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_upgradeEffect = gameObject.GetComponent<UpgradeEffect>();
				m_upgradeEffect.Play(effectPos);
			});
		}
		else
		{
			m_upgradeEffect.Play(effectPos);
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
				m_upgrade3DEffect.Play(WeaponModel.transform.position + m_effectOffset);
			});
		}
		else
		{
			m_upgrade3DEffect.Play(WeaponModel.transform.position + m_effectOffset);
		}
	}

	private void PlayStarUpEffect()
	{
		if (m_starUpEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "starupeffect", "StarUpEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_starUpEffect = gameObject.GetComponent<StarUpEffect>();
				m_starUpEffect.Play(AddStar[tWeaponInfo.netInfo.Star - 1].transform.position);
			});
		}
		else
		{
			m_starUpEffect.Play(AddStar[tWeaponInfo.netInfo.Star - 1].transform.position);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP02);
	}

	private IEnumerator PlayStarUpEffectAndRefreshMenu()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		yield return new WaitForSeconds(0.5f);
		PlayUpgrade3DEffect();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_addLVUp);
		yield return new WaitForSeconds(1f);
		PlayStarUpEffect();
		yield return new WaitForSeconds(0.25f);
		nNowWeaponID = -1;
		InitWeapon();
		bEffectLock = false;
	}

	public bool IsEffectPlaying()
	{
		bool flag = false;
		flag |= bEffectLock;
		if (m_unlockEffect != null)
		{
			flag |= m_unlockEffect.activeSelf;
		}
		if (m_levelUpEffect != null)
		{
			flag |= m_levelUpEffect.activeSelf;
		}
		if (m_levelUpWordEffect != null)
		{
			flag |= m_levelUpWordEffect.activeSelf;
		}
		if (BookUPEffect != null)
		{
			flag |= BookUPEffect.gameObject.activeSelf;
		}
		if (m_upgradeEffect != null)
		{
			flag |= m_upgradeEffect.gameObject.activeSelf;
		}
		if (m_upgrade3DEffect != null)
		{
			flag |= m_upgrade3DEffect.gameObject.activeSelf;
		}
		if (m_starUpEffect != null)
		{
			flag |= m_starUpEffect.gameObject.activeSelf;
		}
		return flag;
	}

	public void PlayUnlockEffect()
	{
		if (m_unlockEffect != null)
		{
			StartCoroutine(PlayUnlockSE());
			m_unlockEffect.SetActive(true);
			m_unlockEffect.GetComponent<Animator>().Play("UI_lockFX", 0, 0f);
			LeanTween.delayedCall(m_unlockEffectLength, (Action)delegate
			{
				m_unlockEffect.SetActive(false);
			});
		}
	}

	private IEnumerator PlayUnlockSE()
	{
		yield return new WaitForSeconds(0.2f);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
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

	public void tLHSRvalueChange()
	{
	}

	private void SortWeaponList()
	{
		if (bUseGoCheckUISort)
		{
			ManagedSingleton<EquipHelper>.Instance.SortWeaponListForGoCheck();
		}
		else
		{
			ManagedSingleton<EquipHelper>.Instance.SortWeaponList();
		}
	}
}
