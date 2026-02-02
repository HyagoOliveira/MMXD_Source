using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DragonBones;
using StageLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChipInfoUI : OrangeUIBase
{
	private enum CHIPINFO_ATTR
	{
		CHIPINFO_ATK = 0,
		CHIPINFO_HP = 1,
		CHIPINFO_DEF = 2,
		CHIPINFO_ANALYSE = 3,
		CHIPINFO_SHOTRANGE = 4,
		CHIPINFO_RELOAD = 5
	}

	private List<WeaponInfoUI.BtnClickCB> BtnClickCBs = new List<WeaponInfoUI.BtnClickCB>();

	private int nNowExp;

	private int nNeedExp;

	private int nExpNowLV;

	private int nNowStar;

	public GameObject refWeaponIconBase;

	public GameObject refWeaponIconBaseBig;

	public int nTargetChipID = 3;

	private int nNowChipID = -1;

	private ChipInfo tChipInfo;

	private DISC_TABLE tDISC_TABLE;

	private STAR_TABLE tSTAR_TABLE;

	private EXP_TABLE tEXP_TABLE;

	public Button[] InfoBtn;

	public LoopHorizontalScrollRect tLHSR;

	public GameObject[] info;

	private int nNowInfoIndex;

	private Text[] InfoBtnText;

	private CanvasGroup[] InfoCanvasGroups;

	private Coroutine tShowCoroutine;

	private Coroutine tCloseCoroutine;

	public Text[] BeforePower;

	public Text[] AfterPower;

	public Text[] BeforeHP;

	public Text[] AfterHP;

	public Text[] BeforeDEF;

	public Text[] AfterDEF;

	public Image[] analysebar;

	public Text[] analysetext;

	public Text[] BeforeAnalyseLV;

	public Text[] AfterAnalyseLV;

	public Text[] BeforeAnalyse;

	public Text[] AfterAnalyse;

	public Text AllPower;

	public Text AllHP;

	public Text AllDEF;

	public GameObject StarRoot0;

	public GameObject[] TopNoStar;

	public GameObject[] TopOnStar;

	public GameObject[] TopAddStar;

	public Text[] s_NAME;

	public Text sBattleScore;

	public Button EquipBtn;

	public Button TakeOutBtn;

	public Text CompatibleWeapon;

	public RawImage WeaponModel;

	public GameObject takeoutroot;

	public Image[] takeouttypemask;

	public ExpButtonRef[] takeoutitems;

	public Button GoTakeOutBtn;

	public Text[] takeoutlbl;

	public GameObject DetailRoot;

	public GameObject ChipWeaponSelectBGClick;

	public Image[] redDot;

	private int nTakeOutType;

	private bool bLockChangeChip;

	[Header("info0")]
	public GameObject EquipSelectRoot;

	public LoopVerticalScrollRect tInfo0LVSR;

	public Text[] AttributrText;

	public FillSliceImg[] AttributrBar;

	public Text n_WEAPON_SKILL0;

	public Text n_WEAPON_Name;

	public Text s_SkillName0;

	public StageLoadIcon info0needimg;

	public StageLoadIcon info0needbg;

	public StageLoadIcon info0needfrm;

	public StageLoadIcon info0skillimg;

	public Text neednameinfo0;

	public Image needbar1;

	public Text needtext1;

	public Button UnLockBtn;

	public GameObject needrootinfo0;

	[HideInInspector]
	public List<int> listWeaponChipEquiped = new List<int>();

	[HideInInspector]
	public bool bLockInfo0;

	[Header("info1")]
	public ExpButtonRef[] expitems;

	public ExpButtonRef[] expqucilitems;

	public Text explabel;

	public Text explv;

	public Image[] expbarsub;

	public GameObject quickupgradeinfo;

	public Button QuickExpUpgradeGo;

	public Button upgradeBtn;

	public Button quickupgradeBtn;

	public Text TargetLvText;

	public Slider expSlider;

	public StageLoadIcon chipicon;

	public StageLoadIcon chipiconfrm;

	public StageLoadIcon chipiconImg;

	private const int nExpItemNum = 6;

	private WeaponInfoUI.expiteminfo[] expiteminfos;

	private int nTotalExpItemAddExp;

	private Color disablecolor = new Color(0.39f, 0.39f, 0.39f);

	private int nQucikLV;

	[Header("info2")]
	public GameObject[] Star;

	public GameObject[] NoStar;

	public GameObject[] AddStar;

	public Button StarButton;

	public GameObject needrootinfo2;

	public Text n_WEAPON_SKILL2;

	public Text s_SkillName2;

	public StageLoadIcon info2skillimg;

	public StageLoadIcon info2msgimg;

	public StageLoadIcon info2needimg;

	public StageLoadIcon info2needbg;

	public StageLoadIcon info2needfrm;

	public Text info2needz;

	public Image needbar2;

	public Text needtext2;

	public Text neednameinfo2;

	[Header("info3")]
	public Button AnalyseBtn;

	public ExpButtonRef[] analyseitems;

	public Text[] analyselvmsg;

	public Text analyseratemsg;

	[HideInInspector]
	public List<int> listHasChips = new List<int>();

	[HideInInspector]
	public List<int> listFragChips = new List<int>();

	[HideInInspector]
	public bool bNeedInitList;

	private RenderTextureObj textureObj;

	private bool useLVUpSE;

	private StarUpEffect m_starUpEffect;

	private Upgrade3DEffect m_upgrade3DEffect;

	private GameObject m_levelUpEffect;

	private GameObject m_levelUpWordEffect;

	private GameObject m_unlockEffect;

	private float m_unlockEffectLength;

	private bool needSE = true;

	private ChipMainUI tChipMainUI;

	private int iOldLv;

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_PLAYER_BOX, UpdateChipData);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_PLAYER_BOX, UpdateChipData);
	}

	private void UpdateChipData()
	{
		nNowChipID = -1;
		InitChip();
	}

	private void Start()
	{
		if (redDot != null)
		{
			redDot[0].gameObject.SetActive(false);
			redDot[1].gameObject.SetActive(false);
		}
		BtnClickCBs.Clear();
		InfoBtnText = new Text[InfoBtn.Length];
		InfoCanvasGroups = new CanvasGroup[InfoBtn.Length];
		for (int i = 0; i < InfoBtn.Length; i++)
		{
			WeaponInfoUI.BtnClickCB btnClickCB = new WeaponInfoUI.BtnClickCB();
			btnClickCB.nBtnID = i;
			btnClickCB.action = (Action<int>)Delegate.Combine(btnClickCB.action, new Action<int>(OnInfoBtnCB));
			InfoBtn[i].onClick.AddListener(btnClickCB.OnClick);
			BtnClickCBs.Add(btnClickCB);
			InfoBtnText[i] = InfoBtn[i].transform.Find("Text").GetComponent<Text>();
			InfoCanvasGroups[i] = info[i].transform.GetComponent<CanvasGroup>();
		}
		InfoBtn[0].interactable = false;
		InfoBtnText[0].color = new Color(0.20392157f, 0.18431373f, 0.22745098f);
		expiteminfos = new WeaponInfoUI.expiteminfo[6];
		for (int j = 0; j < 6; j++)
		{
			expiteminfos[j] = new WeaponInfoUI.expiteminfo();
		}
		for (int k = 0; k < expitems.Length; k++)
		{
			WeaponInfoUI.BtnClickCB btnClickCB2 = new WeaponInfoUI.BtnClickCB();
			btnClickCB2.nBtnID = k;
			WeaponInfoUI.BtnClickCB btnClickCB3 = btnClickCB2;
			btnClickCB3.action = (Action<int>)Delegate.Combine(btnClickCB3.action, new Action<int>(OnExpItemBtnCB));
			expitems[k].Button.onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(btnClickCB2);
			btnClickCB2 = new WeaponInfoUI.BtnClickCB();
			btnClickCB2.nBtnID = k;
			WeaponInfoUI.BtnClickCB btnClickCB4 = btnClickCB2;
			btnClickCB4.action = (Action<int>)Delegate.Combine(btnClickCB4.action, new Action<int>(OnExpItemUnuseBtnCB));
			expitems[k].UnuseBtn.onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(btnClickCB2);
			btnClickCB2 = new WeaponInfoUI.BtnClickCB();
			btnClickCB2.nBtnID = k;
			WeaponInfoUI.BtnClickCB btnClickCB5 = btnClickCB2;
			btnClickCB5.action = (Action<int>)Delegate.Combine(btnClickCB5.action, new Action<int>(OnExpItemAddBtnCB));
			expitems[k].AddBtn.onClick.AddListener(btnClickCB2.OnClick);
			BtnClickCBs.Add(btnClickCB2);
		}
		for (int l = 0; l < analyseitems.Length; l++)
		{
			WeaponInfoUI.BtnClickCB btnClickCB6 = new WeaponInfoUI.BtnClickCB();
			btnClickCB6.nBtnID = l;
			btnClickCB6.action = (Action<int>)Delegate.Combine(btnClickCB6.action, new Action<int>(OnChipAnalyseAddBtnCB));
			analyseitems[l].AddBtn.onClick.AddListener(btnClickCB6.OnClick);
			analyseitems[l].Button.onClick.AddListener(btnClickCB6.OnClick);
			BtnClickCBs.Add(btnClickCB6);
		}
		InitLVSR();
		ChipWeaponSelectBGClick.SetActive(false);
		bLockChangeChip = false;
		EquipSelectRoot.SetActive(false);
		InitChip();
		ModelRotateDrag objDrag = WeaponModel.GetComponent<ModelRotateDrag>();
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(GameObject obj)
		{
			textureObj = UnityEngine.Object.Instantiate(obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
			textureObj.AssignNewEnemyRender(ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[nTargetChipID].s_MODEL, new Vector3(0f, 0f, 60f), WeaponModel);
			if ((bool)objDrag)
			{
				objDrag.SetModelTransform(textureObj.RenderPosition);
			}
		});
		tChipMainUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ChipMainUI>("UI_CHIPMAIN");
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
		Vector3 offset = new Vector3(0f, 5f, 0f);
		if (m_levelUpEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "levelupeffect", "LevelUpEffect", delegate(GameObject asset)
			{
				m_levelUpEffect = UnityEngine.Object.Instantiate(asset, base.transform);
				m_levelUpEffect.transform.position = WeaponModel.transform.position + offset;
				m_levelUpEffect.SetActive(false);
			});
		}
		if (m_levelUpWordEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "levelupwordeffect", "LevelUpWordEffect", delegate(GameObject asset)
			{
				m_levelUpWordEffect = UnityEngine.Object.Instantiate(asset, base.transform);
				m_levelUpWordEffect.transform.position = WeaponModel.transform.position + offset;
				m_levelUpWordEffect.SetActive(false);
			});
		}
		tShowCoroutine = StartCoroutine(ShowInfoCoroutine(0, 0.5f));
	}

	private void OnDestroy()
	{
		if (textureObj != null && textureObj.gameObject != null)
		{
			UnityEngine.Object.Destroy(textureObj.gameObject);
		}
	}

	private void LateUpdate()
	{
		n_WEAPON_SKILL0.alignByGeometry = false;
		n_WEAPON_SKILL2.alignByGeometry = false;
	}

	public void Sorted()
	{
		listHasChips.Clear();
		listFragChips.Clear();
		Dictionary<int, DISC_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.n_ENABLE_FLAG == 0)
			{
				continue;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(enumerator.Current.Key))
			{
				if (ManagedSingleton<EquipHelper>.Instance.bIsShowGetedChip() && ((uint)enumerator.Current.Value.n_WEAPON_TYPE & (uint)ManagedSingleton<EquipHelper>.Instance.nChipSortType) != 0)
				{
					listHasChips.Add(enumerator.Current.Key);
				}
			}
			else if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(enumerator.Current.Value.n_UNLOCK_ID) && ManagedSingleton<EquipHelper>.Instance.bIsShowFragChip() && ((uint)enumerator.Current.Value.n_WEAPON_TYPE & (uint)ManagedSingleton<EquipHelper>.Instance.nChipSortType) != 0)
			{
				listFragChips.Add(enumerator.Current.Key);
			}
		}
		IEnumerable<KeyValuePair<int, ChipInfo>> source = ManagedSingleton<PlayerNetManager>.Instance.dicChip.Where((KeyValuePair<int, ChipInfo> obj) => listHasChips.Contains(obj.Key));
		IOrderedEnumerable<KeyValuePair<int, ChipInfo>> orderedEnumerable = null;
		if ((ManagedSingleton<EquipHelper>.Instance.nChipSortKey & EquipHelper.WEAPON_SORT_KEY.WEAPON_SORT_RARITY) != 0)
		{
			orderedEnumerable = ((orderedEnumerable != null) ? orderedEnumerable.ThenByDescending((KeyValuePair<int, ChipInfo> obj) => ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[obj.Key].n_RARITY) : source.OrderByDescending((KeyValuePair<int, ChipInfo> obj) => ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[obj.Key].n_RARITY));
		}
		if ((ManagedSingleton<EquipHelper>.Instance.nChipSortKey & EquipHelper.WEAPON_SORT_KEY.WEAPON_SORT_STAR) != 0)
		{
			orderedEnumerable = ((orderedEnumerable != null) ? orderedEnumerable.ThenByDescending((KeyValuePair<int, ChipInfo> obj) => obj.Value.netChipInfo.Star) : source.OrderByDescending((KeyValuePair<int, ChipInfo> obj) => obj.Value.netChipInfo.Star));
		}
		if ((ManagedSingleton<EquipHelper>.Instance.nChipSortKey & EquipHelper.WEAPON_SORT_KEY.WEAPON_SORT_LV) != 0)
		{
			orderedEnumerable = ((orderedEnumerable != null) ? orderedEnumerable.ThenByDescending((KeyValuePair<int, ChipInfo> obj) => obj.Value.netChipInfo.Exp) : source.OrderByDescending((KeyValuePair<int, ChipInfo> obj) => obj.Value.netChipInfo.Exp));
		}
		if ((ManagedSingleton<EquipHelper>.Instance.nChipSortKey & EquipHelper.WEAPON_SORT_KEY.WEAPON_SORT_UPGRADE) != 0)
		{
			orderedEnumerable = ((orderedEnumerable != null) ? orderedEnumerable.ThenByDescending((KeyValuePair<int, ChipInfo> obj) => obj.Value.netChipInfo.Analyse) : source.OrderByDescending((KeyValuePair<int, ChipInfo> obj) => obj.Value.netChipInfo.Analyse));
		}
		if (orderedEnumerable == null)
		{
			orderedEnumerable = source.OrderByDescending((KeyValuePair<int, ChipInfo> obj) => obj.Key);
		}
		KeyValuePair<int, ChipInfo>[] array = orderedEnumerable.ToArray();
		listHasChips.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			listHasChips.Add(array[i].Key);
		}
	}

	public void OnInfoBtnCB(int nBtnID)
	{
		if (tShowCoroutine != null || tCloseCoroutine != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		for (int i = 0; i < info.Length; i++)
		{
			if (nBtnID != i && nNowInfoIndex != i)
			{
				info[i].SetActive(false);
			}
		}
		for (int j = 0; j < InfoBtn.Length; j++)
		{
			if (nBtnID != j)
			{
				InfoBtn[j].interactable = true;
				InfoBtnText[j].color = Color.white;
			}
			else
			{
				InfoBtn[j].interactable = false;
				InfoBtnText[j].color = new Color(0.20392157f, 0.18431373f, 0.22745098f);
			}
		}
		tCloseCoroutine = StartCoroutine(CloseInfoCoroutine(nNowInfoIndex, 0.2f, false, nBtnID));
		nNowInfoIndex = nBtnID;
		InitChip();
	}

	private IEnumerator ShowInfoCoroutine(int nInfoIndex, float fTime)
	{
		List<UnityEngine.Transform> listRoots = new List<UnityEngine.Transform>();
		int num = 0;
		while (true)
		{
			UnityEngine.Transform transform = info[nInfoIndex].transform.Find("BgRoot" + (num + 1));
			if (!(transform != null))
			{
				break;
			}
			listRoots.Add(transform);
			num++;
		}
		for (int i = 0; i < listRoots.Count; i++)
		{
			listRoots[i].localScale = new Vector3(1f, 0f, 1f);
		}
		float fd = 1f / fTime;
		float[] fFactors = new float[listRoots.Count];
		float fLeftTime = fTime;
		info[nInfoIndex].SetActive(true);
		if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID))
		{
			TakeOutBtn.gameObject.SetActive(false);
			if (info[nNowInfoIndex].activeSelf)
			{
				EquipBtn.gameObject.SetActive(true);
			}
		}
		else
		{
			TakeOutBtn.gameObject.SetActive(false);
			EquipBtn.gameObject.SetActive(false);
		}
		while (true)
		{
			float deltaTime = Time.deltaTime;
			fLeftTime -= deltaTime;
			bool flag = true;
			for (int j = 0; j < listRoots.Count; j++)
			{
				fFactors[j] += fd * deltaTime;
				if (fFactors[j] >= 1f)
				{
					fFactors[j] = 1f;
				}
				else
				{
					flag = false;
				}
				listRoots[j].localScale = new Vector3(1f, fFactors[j], 1f);
				if (fFactors[j] < 0.5f)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		tShowCoroutine = null;
	}

	private IEnumerator CloseInfoCoroutine(int nInfoIndex, float fTime, bool ReShow, int nNexIndex)
	{
		if (ReShow && nNexIndex == -1)
		{
			EquipBtn.gameObject.SetActive(false);
		}
		List<UnityEngine.Transform> listRoots = new List<UnityEngine.Transform>();
		int num = 0;
		while (true)
		{
			UnityEngine.Transform transform = info[nInfoIndex].transform.Find("BgRoot" + (num + 1));
			if (!(transform != null))
			{
				break;
			}
			listRoots.Add(transform);
			num++;
		}
		float fd = 1f / fTime;
		float[] fFactors = new float[listRoots.Count];
		float fLeftTime = fTime;
		while (true)
		{
			float deltaTime = Time.deltaTime;
			fLeftTime -= deltaTime;
			bool flag = true;
			for (int i = 0; i < listRoots.Count; i++)
			{
				fFactors[i] += fd * deltaTime;
				if (fFactors[i] >= 1f)
				{
					fFactors[i] = 1f;
				}
				else
				{
					flag = false;
				}
				listRoots[i].localScale = new Vector3(1f, 1f - fFactors[i], 1f);
				if (fFactors[i] < 0.5f)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		info[nInfoIndex].SetActive(false);
		tCloseCoroutine = null;
		InitChip();
		if (ReShow)
		{
			tShowCoroutine = StartCoroutine(ShowInfoCoroutine(nInfoIndex, 0.2f));
		}
		else if (nNexIndex != -1)
		{
			tShowCoroutine = StartCoroutine(ShowInfoCoroutine(nNexIndex, 0.2f));
		}
	}

	public bool IsEffectPlaying()
	{
		bool flag = false;
		if (m_starUpEffect != null)
		{
			flag = flag || m_starUpEffect.gameObject.activeSelf;
		}
		if (m_upgrade3DEffect != null)
		{
			flag = flag || m_upgrade3DEffect.gameObject.activeSelf;
		}
		if (m_levelUpEffect != null)
		{
			flag = flag || m_levelUpEffect.activeSelf;
		}
		if (m_levelUpWordEffect != null)
		{
			flag = flag || m_levelUpWordEffect.activeSelf;
		}
		if (m_unlockEffect != null)
		{
			flag = flag || m_unlockEffect.activeSelf;
		}
		return flag;
	}

	public void EffectAllStop()
	{
		StopAllCoroutines();
		if (m_starUpEffect != null)
		{
			m_starUpEffect.gameObject.SetActive(false);
		}
		if (m_upgrade3DEffect != null)
		{
			m_upgrade3DEffect.gameObject.SetActive(false);
		}
		if (m_levelUpEffect != null && m_levelUpWordEffect != null)
		{
			m_levelUpEffect.SetActive(false);
			m_levelUpWordEffect.SetActive(false);
		}
		if (m_unlockEffect != null)
		{
			m_unlockEffect.SetActive(false);
		}
	}

	public void ChangeChip(int nNewID)
	{
		if (tShowCoroutine == null && tCloseCoroutine == null && !bLockChangeChip)
		{
			EffectAllStop();
			nTargetChipID = nNewID;
			nNowChipID = -1;
			tLHSR.RefreshCells();
			if (textureObj != null)
			{
				textureObj.UpdateModelName(ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[nTargetChipID].s_MODEL);
			}
			if (info[nNowInfoIndex].activeSelf)
			{
				tCloseCoroutine = StartCoroutine(CloseInfoCoroutine(nNowInfoIndex, 0.2f, true, -1));
			}
			else
			{
				InitChip();
			}
		}
	}

	public void OnSwitchOnlyShowModel()
	{
		if (tShowCoroutine != null || tCloseCoroutine != null)
		{
			return;
		}
		TakeOutBtn.gameObject.SetActive(false);
		EquipBtn.gameObject.SetActive(!EquipBtn.gameObject.activeSelf);
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID))
		{
			EquipBtn.gameObject.SetActive(false);
		}
		if (info[nNowInfoIndex].activeSelf)
		{
			tCloseCoroutine = StartCoroutine(CloseInfoCoroutine(nNowInfoIndex, 0.2f, false, -1));
			Button[] infoBtn = InfoBtn;
			for (int i = 0; i < infoBtn.Length; i++)
			{
				infoBtn[i].gameObject.SetActive(false);
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01);
		}
		else
		{
			tShowCoroutine = StartCoroutine(ShowInfoCoroutine(nNowInfoIndex, 0.2f));
			Button[] infoBtn = InfoBtn;
			for (int i = 0; i < infoBtn.Length; i++)
			{
				infoBtn[i].gameObject.SetActive(true);
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
		}
	}

	public void OnDetailShow()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		DetailRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		StartCoroutine(ObjScaleCoroutine(0f, 1f, 0.2f, DetailRoot, null));
		DetailRoot.SetActive(true);
	}

	public void OnDetailClose()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		StartCoroutine(ObjScaleCoroutine(1f, 0f, 0.2f, DetailRoot, delegate
		{
			DetailRoot.SetActive(false);
		}));
	}

	public void OnTakeOutBtn()
	{
		ChipWeaponSelectBGClick.SetActive(true);
		bLockChangeChip = true;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		InitTakeoutPopup();
		takeoutroot.transform.localScale = new Vector3(0f, 0f, 1f);
		StartCoroutine(ObjScaleCoroutine(0f, 1f, 0.2f, takeoutroot, null));
		takeoutroot.SetActive(true);
	}

	public void OnCloseTakeoutDetailPopup()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		StartCoroutine(ObjScaleCoroutine(1f, 0f, 0.2f, takeoutroot, delegate
		{
			takeoutroot.SetActive(false);
			ChipWeaponSelectBGClick.SetActive(false);
			bLockChangeChip = false;
		}));
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
		takeoutlbl[0].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DISC_TAKEOUT_TIP");
		takeoutlbl[1].text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TAKEOUT_COST"), OrangeConst.TAKEOUT_COST);
		GoTakeOutBtn.interactable = false;
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

	public void SetTakeOutType(int nType)
	{
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
		List<WeaponInfoUI.expiteminfo> rtItems = new List<WeaponInfoUI.expiteminfo>();
		if (((uint)nTakeOutType & (true ? 1u : 0u)) != 0)
		{
			int exp = tChipInfo.netChipInfo.Exp;
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_DISC_TAKEOUT];
			int num = (int)iTEM_TABLE.f_VALUE_X;
			if (exp > 0)
			{
				WeaponInfoUI.expiteminfo tItem = new WeaponInfoUI.expiteminfo();
				tItem.nUseNum = (exp - exp % num) / num;
				if (exp % num > 0)
				{
					tItem.nUseNum++;
				}
				tItem.tITEM_TABLE = iTEM_TABLE;
				AddRtItem(ref rtItems, ref tItem);
			}
		}
		if (((uint)nTakeOutType & 2u) != 0)
		{
			for (int num2 = tChipInfo.netChipInfo.Star; num2 > 0; num2--)
			{
				Dictionary<int, STAR_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.GetEnumerator();
				STAR_TABLE sTAR_TABLE = null;
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Value.n_TYPE == 4 && enumerator.Current.Value.n_MAINID == nTargetChipID && num2 - 1 == enumerator.Current.Value.n_STAR)
					{
						sTAR_TABLE = enumerator.Current.Value;
						break;
					}
				}
				if (sTAR_TABLE != null && sTAR_TABLE.n_MATERIAL != 0)
				{
					MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[sTAR_TABLE.n_MATERIAL];
					ITEM_TABLE tITEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[mATERIAL_TABLE.n_MATERIAL_1];
					WeaponInfoUI.expiteminfo tItem2 = new WeaponInfoUI.expiteminfo();
					tItem2.nUseNum = mATERIAL_TABLE.n_MATERIAL_MOUNT1;
					tItem2.tITEM_TABLE = tITEM_TABLE;
					AddRtItem(ref rtItems, ref tItem2);
				}
			}
		}
		if (((uint)nTakeOutType & 4u) != 0)
		{
			int num3 = tChipInfo.netChipInfo.Analyse;
			int[] array = new int[5] { tDISC_TABLE.n_ANALYSE_1, tDISC_TABLE.n_ANALYSE_2, tDISC_TABLE.n_ANALYSE_3, tDISC_TABLE.n_ANALYSE_4, tDISC_TABLE.n_ANALYSE_5 };
			while (num3 > 0)
			{
				MATERIAL_TABLE mATERIAL_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[array[num3 - 1]];
				int[] array2 = new int[5] { mATERIAL_TABLE2.n_MATERIAL_1, mATERIAL_TABLE2.n_MATERIAL_2, mATERIAL_TABLE2.n_MATERIAL_3, mATERIAL_TABLE2.n_MATERIAL_4, mATERIAL_TABLE2.n_MATERIAL_5 };
				int[] array3 = new int[5] { mATERIAL_TABLE2.n_MATERIAL_MOUNT1, mATERIAL_TABLE2.n_MATERIAL_MOUNT2, mATERIAL_TABLE2.n_MATERIAL_MOUNT3, mATERIAL_TABLE2.n_MATERIAL_MOUNT4, mATERIAL_TABLE2.n_MATERIAL_MOUNT5 };
				for (int i = 0; i < analyseitems.Length; i++)
				{
					if (array2[i] != 0)
					{
						ITEM_TABLE tITEM_TABLE2 = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[array2[i]];
						WeaponInfoUI.expiteminfo tItem3 = new WeaponInfoUI.expiteminfo();
						tItem3.nUseNum = array3[i];
						tItem3.tITEM_TABLE = tITEM_TABLE2;
						AddRtItem(ref rtItems, ref tItem3);
					}
				}
				num3--;
			}
		}
		for (int j = 0; j < rtItems.Count && j < takeoutitems.Length; j++)
		{
			UpdateItemNeedInfo(rtItems[j].tITEM_TABLE, takeoutitems[j].BtnImgae, takeoutitems[j].frmimg, takeoutitems[j].bgimg, null);
			takeoutitems[j].BtnLabel.text = rtItems[j].nUseNum.ToString();
			takeoutitems[j].Button.gameObject.SetActive(true);
		}
		for (int k = rtItems.Count; k < takeoutitems.Length; k++)
		{
			takeoutitems[k].Button.gameObject.SetActive(false);
		}
		GoTakeOutBtn.interactable = rtItems.Count > 0 && ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel() >= OrangeConst.TAKEOUT_COST;
	}

	public void TakeOutChip()
	{
		int nTakeOutType2 = nTakeOutType;
	}

	public EXP_TABLE GetNowLvWithAddExp(int nAddExp)
	{
		int num = nAddExp;
		if (tChipInfo != null)
		{
			num += tChipInfo.netChipInfo.Exp;
		}
		Dictionary<int, EXP_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.GetEnumerator();
		EXP_TABLE eXP_TABLE = null;
		while (enumerator.MoveNext())
		{
			if (num < enumerator.Current.Value.n_TOTAL_DISCEXP && enumerator.Current.Value.n_TOTAL_DISCEXP - num <= enumerator.Current.Value.n_DISCEXP)
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
		return ManagedSingleton<PlayerHelper>.Instance.GetExpTable().n_TOTAL_DISCEXP;
	}

	public int GetMaxLvExp()
	{
		return ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.OrderByDescending((KeyValuePair<int, EXP_TABLE> obj) => obj.Key).First().Value.n_TOTAL_WEAPONEXP;
	}

	public void InitInfo0()
	{
		int[] array = new int[10] { tDISC_TABLE.n_SKILL_0, tDISC_TABLE.n_SKILL_1, tDISC_TABLE.n_SKILL_2, tDISC_TABLE.n_SKILL_3, tDISC_TABLE.n_SKILL_4, tDISC_TABLE.n_SKILL_5, tDISC_TABLE.n_SKILL_6, 0, 0, 0 };
		int num = nNowStar;
		while (num > 0 && array[num] == 0)
		{
			num--;
		}
		for (int i = 0; i < s_NAME.Length; i++)
		{
			s_NAME[i].text = ManagedSingleton<OrangeTextDataManager>.Instance.DISCTEXT_TABLE_DICT.GetL10nValue(tDISC_TABLE.w_NAME);
			s_NAME[i].transform.parent.gameObject.SetActive(false);
		}
		CompatibleWeapon.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DISC_EQUIP_WEAPON") + MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("WEAPON_TYPE_" + tDISC_TABLE.n_WEAPON_TYPE);
		OrangeRareText.Rare n_RARITY = (OrangeRareText.Rare)tDISC_TABLE.n_RARITY;
		s_NAME[(int)(n_RARITY - 1)].transform.parent.gameObject.SetActive(true);
		if (tDISC_TABLE.n_UNLOCK_ID != 0)
		{
			needrootinfo0.SetActive(true);
			if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID))
			{
				UnLockBtn.transform.parent.gameObject.SetActive(false);
			}
			else
			{
				UnLockBtn.transform.parent.gameObject.SetActive(true);
			}
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[tDISC_TABLE.n_UNLOCK_ID];
			UpdateChipNeedInfo(iTEM_TABLE, info0needimg, info0needfrm, info0needbg, neednameinfo0);
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
			{
				needbar1.fillAmount = (float)ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack / (float)tDISC_TABLE.n_UNLOCK_COUNT;
				needtext1.text = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack + "/" + tDISC_TABLE.n_UNLOCK_COUNT;
				if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID) && ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack >= tDISC_TABLE.n_UNLOCK_COUNT)
				{
					UnLockBtn.interactable = true;
				}
				else
				{
					UnLockBtn.interactable = false;
				}
				UnLockBtn.transform.Find("Image").gameObject.SetActive(!UnLockBtn.interactable);
			}
			else
			{
				needbar1.fillAmount = 0f;
				needtext1.text = "<color=#FF0000>0</color>/" + tDISC_TABLE.n_UNLOCK_COUNT;
				UnLockBtn.interactable = false;
				UnLockBtn.transform.Find("Image").gameObject.SetActive(!UnLockBtn.interactable);
			}
		}
		else
		{
			needrootinfo0.SetActive(false);
		}
		SKILL_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(array[num], out value))
		{
			s_SkillName0.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			info0skillimg.gameObject.SetActive(true);
			info0skillimg.CheckLoad(AssetBundleScriptableObject.Instance.GetShowcase(value.s_SHOWCASE), value.s_SHOWCASE);
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
			float num2 = 100f;
			int n_EFFECT = value.n_EFFECT;
			if (n_EFFECT == 1)
			{
				num2 = value.f_EFFECT_X + 0f * value.f_EFFECT_Y;
			}
			n_WEAPON_SKILL0.text = string.Format(l10nValue.Replace("{0}", "{0:0.00}"), num2);
			n_WEAPON_SKILL0.alignByGeometry = false;
		}
		else
		{
			s_SkillName0.text = "";
			info0skillimg.gameObject.SetActive(false);
			n_WEAPON_SKILL0.text = "";
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID))
		{
			TakeOutBtn.gameObject.SetActive(false);
			if (info[nNowInfoIndex].activeSelf)
			{
				EquipBtn.gameObject.SetActive(true);
			}
		}
		else
		{
			TakeOutBtn.gameObject.SetActive(false);
			EquipBtn.gameObject.SetActive(false);
		}
	}

	public void OnEquipBtn()
	{
		if (tDISC_TABLE == null)
		{
			return;
		}
		listWeaponChipEquiped.Clear();
		ChipWeaponSelectBGClick.SetActive(true);
		bLockChangeChip = true;
		EquipSelectRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		StartCoroutine(ObjScaleCoroutine(0f, 1f, 0.2f, EquipSelectRoot, delegate
		{
			tInfo0LVSR.SrollToCell(0, 2000f);
			if (TurtorialUI.IsTutorialing())
			{
				tInfo0LVSR.vertical = false;
			}
			else
			{
				tInfo0LVSR.vertical = true;
			}
		}));
		EquipSelectRoot.SetActive(true);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		int num = 0;
		Dictionary<int, WeaponInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.GetEnumerator();
		while (enumerator.MoveNext())
		{
			WEAPON_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(enumerator.Current.Value.netInfo.WeaponID, out value) && (value.n_TYPE & tDISC_TABLE.n_WEAPON_TYPE) > 0)
			{
				num++;
			}
		}
		int num2 = (num - num % 5) / 5;
		if (num % 5 > 0)
		{
			num2++;
		}
		tInfo0LVSR.totalCount = num2;
		tInfo0LVSR.RefillCells();
	}

	private void InitLVSR()
	{
		if (bNeedInitList)
		{
			Sorted();
		}
		tLHSR.totalCount = listHasChips.Count + listFragChips.Count;
		tLHSR.RefillCells();
	}

	public void OnCloseEquipBtn()
	{
		if (!bLockInfo0)
		{
			StartCoroutine(ObjScaleCoroutine(1f, 0f, 0.2f, EquipSelectRoot, delegate
			{
				EquipSelectRoot.SetActive(false);
				ChipWeaponSelectBGClick.SetActive(false);
				bLockChangeChip = false;
			}));
		}
	}

	public void OnSelectEquipOk()
	{
		if (!bLockInfo0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
			bLockInfo0 = true;
			KeyValuePair<int, WeaponInfo>[] array = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Where((KeyValuePair<int, WeaponInfo> obj) => obj.Value.netInfo.Chip == nTargetChipID && !listWeaponChipEquiped.Contains(obj.Value.netInfo.WeaponID)).ToArray();
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = array[i].Value.netInfo.WeaponID;
			}
			KeyValuePair<int, WeaponInfo>[] array3 = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Where((KeyValuePair<int, WeaponInfo> obj) => obj.Value.netInfo.Chip != nTargetChipID && listWeaponChipEquiped.Contains(obj.Value.netInfo.WeaponID)).ToArray();
			int[] array4 = new int[array3.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array4[j] = array3[j].Value.netInfo.WeaponID;
			}
			if (array4.Length != 0 || array2.Length != 0)
			{
				ManagedSingleton<PlayerNetManager>.Instance.WeaponChipSetReqs(array4, nTargetChipID, array2, EndUpdateWeaponChip);
			}
			else
			{
				EndUpdateWeaponChip();
			}
		}
	}

	public void EndUpdateWeaponChip()
	{
		StartCoroutine(ObjScaleCoroutine(1f, 0f, 0.2f, EquipSelectRoot, delegate
		{
			EquipSelectRoot.SetActive(false);
			bLockInfo0 = false;
			ChipWeaponSelectBGClick.SetActive(false);
			bLockChangeChip = false;
		}));
		nNowChipID = -1;
		InitChip();
	}

	public void OnNeed0AddFrag()
	{
		MATERIAL_TABLE tMATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tSTAR_TABLE.n_MATERIAL];
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(tMATERIAL_TABLE.n_MATERIAL_1, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item, null, tMATERIAL_TABLE.n_MATERIAL_MOUNT1);
			});
		}
	}

	public void UnLockChipRequest()
	{
		ManagedSingleton<PlayerNetManager>.Instance.UnlockChipReq(nTargetChipID, delegate
		{
			if ((bool)tChipMainUI)
			{
				tChipMainUI.SortChips();
			}
			PlayUnlockEffect();
			InitLVSR();
			nNowChipID = -1;
			InitChip();
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
	}

	public void InitExpItem()
	{
		nTotalExpItemAddExp = 0;
		IEnumerable<KeyValuePair<int, ITEM_TABLE>> source = from tOO in ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT
			where tOO.Value.n_TYPE == 2 && tOO.Value.n_TYPE_X == 2
			orderby tOO.Value.n_RARE
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
		nTotalExpItemAddExp = 0;
		quickupgradeinfo.SetActive(false);
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
		quickupgradeBtn.interactable = ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID);
		quickupgradeBtn.transform.Find("Image").gameObject.SetActive(!quickupgradeBtn.interactable);
		chipiconImg.CheckLoad(AssetBundleScriptableObject.Instance.m_iconChip, tDISC_TABLE.s_ICON);
		OrangeRareText.Rare n_RARITY = (OrangeRareText.Rare)tDISC_TABLE.n_RARITY;
		chipiconfrm.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall((int)n_RARITY));
		chipicon.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareBgSmall((int)n_RARITY));
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

	public void UpdateExpBar()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(nTotalExpItemAddExp);
		int num = nNowExp * 100 / nNeedExp;
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
		if (GetNowLvWithAddExp(0).n_ID < nowLvWithAddExp.n_ID)
		{
			useLVUpSE = true;
		}
		else
		{
			useLVUpSE = false;
		}
		explv.text = nowLvWithAddExp.n_ID.ToString();
	}

	public void OnExpItemAddBtnCB(int nBtnID)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(expiteminfos[nBtnID].tITEM_TABLE.n_ID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item);
			});
		}
	}

	private void ResetExpItems()
	{
		for (int i = 0; i < expitems.Length; i++)
		{
			expitems[i].BtnLabel.text = expiteminfos[i].nUseNum + "/" + expiteminfos[i].mHaveNum;
			if (expiteminfos[i].nUseNum > 0)
			{
				expitems[i].UnuseBtn.gameObject.SetActive(true);
			}
			else
			{
				expitems[i].UnuseBtn.gameObject.SetActive(false);
			}
		}
	}

	public void OnExpItemBtnCB(int nBtnID)
	{
		bool flag = false;
		int totalAddExp = GetTotalAddExp();
		if (tChipInfo.netChipInfo.Exp + totalAddExp + (int)expiteminfos[nBtnID].tITEM_TABLE.f_VALUE_X >= GetTotalExpByPlayerLV())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_WEAPON_LV"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
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
			nTotalExpItemAddExp = totalAddExp + (int)expiteminfos[nBtnID].tITEM_TABLE.f_VALUE_X;
			flag = true;
		}
		if (flag)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
		ResetExpItems();
		InitChip();
	}

	public void OnExpItemUnuseBtnCB(int nBtnID)
	{
		expiteminfos[nBtnID].nUseNum = 0;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
		ResetExpItems();
		nTotalExpItemAddExp = GetTotalAddExp();
		InitChip();
	}

	public void OnUseExpItemUpgrade()
	{
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
		ChipWeaponSelectBGClick.SetActive(false);
		bLockChangeChip = false;
		quickupgradeinfo.SetActive(false);
		if (list.Count == 0)
		{
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.UpgradeLevelChipReq(nTargetChipID, list, delegate
		{
			if (useLVUpSE)
			{
				PlayLevelUp3DEffect();
				useLVUpSE = false;
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
			}
			else
			{
				PlayUpgrade3DEffect();
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
			}
			if ((bool)tChipMainUI)
			{
				tChipMainUI.SortChips();
			}
			needSE = false;
			expSlider.value = 0f;
			nNowChipID = -1;
			nTotalExpItemAddExp = 0;
			InitChip();
			UpdateChipCell();
		});
	}

	private void UpdateChipCell()
	{
		ChipCell[] componentsInChildren = base.transform.GetComponentsInChildren<ChipCell>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateData();
		}
	}

	public void OnQuickExpGrade()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		int n_ID = nowLvWithAddExp.n_ID;
		nowLvWithAddExp = ManagedSingleton<PlayerHelper>.Instance.GetExpTable();
		int maxLvExp = GetMaxLvExp();
		int num = tChipInfo.netChipInfo.Exp;
		for (int num2 = expiteminfos.Length - 1; num2 >= 0; num2--)
		{
			if (expiteminfos[num2].tITEM_TABLE != null)
			{
				int num3 = (int)Mathf.Ceil((float)(maxLvExp - num) / expiteminfos[num2].tITEM_TABLE.f_VALUE_X);
				if (num3 < expiteminfos[num2].mHaveNum)
				{
					num += num3 * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X;
					break;
				}
				num += expiteminfos[num2].mHaveNum * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X;
			}
		}
		while (num < nowLvWithAddExp.n_TOTAL_DISCEXP - nowLvWithAddExp.n_DISCEXP && nowLvWithAddExp.n_ID != n_ID)
		{
			nowLvWithAddExp = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[nowLvWithAddExp.n_ID - 1];
		}
		expSlider.minValue = 0f;
		expSlider.maxValue = nowLvWithAddExp.n_ID - n_ID;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		iOldLv = 0;
		nQucikLV = nExpNowLV;
		InitQuickExpitem(nQucikLV);
		ChipWeaponSelectBGClick.SetActive(true);
		bLockChangeChip = true;
		quickupgradeinfo.transform.localScale = new Vector3(0f, 0f, 1f);
		StartCoroutine(ObjScaleCoroutine(0f, 1f, 0.2f, quickupgradeinfo, null));
		quickupgradeinfo.SetActive(true);
	}

	public void OnQuickExpGradeClose()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		StartCoroutine(ObjScaleCoroutine(1f, 0f, 0.2f, quickupgradeinfo, delegate
		{
			quickupgradeinfo.SetActive(false);
			ChipWeaponSelectBGClick.SetActive(false);
			bLockChangeChip = false;
		}));
	}

	public void OnAddQuickLV()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		if ((float)nQucikLV < expSlider.maxValue + (float)nowLvWithAddExp.n_ID)
		{
			nQucikLV++;
			InitQuickExpitem(nQucikLV);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
	}

	public void OnDecreaseQuickLV()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		if (nQucikLV > nowLvWithAddExp.n_ID)
		{
			nQucikLV--;
			InitQuickExpitem(nQucikLV);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
	}

	public void OnMaxQuickLV()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		if ((float)nQucikLV != expSlider.maxValue + (float)nowLvWithAddExp.n_ID)
		{
			nQucikLV = (int)expSlider.maxValue + nowLvWithAddExp.n_ID;
			InitQuickExpitem(nQucikLV);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
	}

	public void OnMinQuickLV()
	{
		EXP_TABLE nowLvWithAddExp = GetNowLvWithAddExp(0);
		if (nQucikLV != nowLvWithAddExp.n_ID)
		{
			nQucikLV = nowLvWithAddExp.n_ID;
			InitQuickExpitem(nQucikLV);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
		}
	}

	private void ReSetQuickItems(int nMaxExp)
	{
		int num = tChipInfo.netChipInfo.Exp;
		EXP_TABLE expTable = ManagedSingleton<PlayerHelper>.Instance.GetExpTable();
		for (int num2 = expiteminfos.Length - 1; num2 >= 0; num2--)
		{
			if (expiteminfos[num2].tITEM_TABLE != null)
			{
				int num3 = (int)Mathf.Floor((float)(nMaxExp - num) / expiteminfos[num2].tITEM_TABLE.f_VALUE_X);
				if (expiteminfos[num2].mHaveNum >= num3)
				{
					expiteminfos[num2].nUseNum = 0;
					while ((expiteminfos[num2].nUseNum + 1) * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X + num < nMaxExp && (expiteminfos[num2].nUseNum + 1) * (int)expiteminfos[num2].tITEM_TABLE.f_VALUE_X + num < expTable.n_TOTAL_DISCEXP)
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

	private void ResetQuickExpItems()
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

	public void InitQuickExpitem(int nLV)
	{
		EXP_TABLE value = GetNowLvWithAddExp(0);
		int n_ID = value.n_ID;
		expSlider.value = nLV - n_ID;
		TargetLvText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TARGET_LV"), nLV - n_ID, expSlider.maxValue);
		int nMaxExp = 0;
		if (nLV != n_ID && ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(nLV, out value))
		{
			nMaxExp = value.n_TOTAL_DISCEXP;
		}
		ReSetQuickItems(nMaxExp);
		ResetQuickExpItems();
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
		InitChip();
	}

	public void OnSilderChange(float value)
	{
		EXP_TABLE value2 = GetNowLvWithAddExp(0);
		int n_ID = value2.n_ID;
		if (expSlider.value == (float)(nQucikLV - n_ID))
		{
			return;
		}
		int num = (int)Mathf.Round(expSlider.value);
		if (num != iOldLv)
		{
			expSlider.value = (iOldLv = num);
			nQucikLV = num + n_ID;
			TargetLvText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TARGET_LV"), num, expSlider.maxValue);
			if (needSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK03);
			}
			else
			{
				needSE = true;
			}
			int nMaxExp = 0;
			if (num != 0 && ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(nQucikLV, out value2))
			{
				nMaxExp = value2.n_TOTAL_DISCEXP;
			}
			ReSetQuickItems(nMaxExp);
			ResetQuickExpItems();
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
			InitChip();
		}
	}

	public void OnUpgradeStar()
	{
		if (tSTAR_TABLE != null && tSTAR_TABLE.n_MATERIAL != 0)
		{
			MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tSTAR_TABLE.n_MATERIAL];
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[mATERIAL_TABLE.n_MATERIAL_1];
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack < mATERIAL_TABLE.n_MATERIAL_MOUNT1)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MATERIAL_NOT_ENOUGH"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
				});
				return;
			}
		}
		StarButton.interactable = false;
		ManagedSingleton<PlayerNetManager>.Instance.UpgradeStarChipReq(nTargetChipID, delegate
		{
			if ((bool)tChipMainUI)
			{
				tChipMainUI.SortChips();
			}
			StartCoroutine(PlayStarUpEffectAndRefreshMenu());
		});
	}

	private void InitInfo2()
	{
		int[] array = new int[10] { tDISC_TABLE.n_SKILL_0, tDISC_TABLE.n_SKILL_1, tDISC_TABLE.n_SKILL_2, tDISC_TABLE.n_SKILL_3, tDISC_TABLE.n_SKILL_4, tDISC_TABLE.n_SKILL_5, tDISC_TABLE.n_SKILL_6, 0, 0, 0 };
		int num = nNowStar + 1;
		while (num > 0 && array[num] == 0)
		{
			num--;
		}
		SKILL_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(array[num], out value))
		{
			s_SkillName2.text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			info2skillimg.gameObject.SetActive(true);
			info2skillimg.CheckLoad(AssetBundleScriptableObject.Instance.GetShowcase(value.s_SHOWCASE), value.s_SHOWCASE);
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(value.w_TIP);
			float num2 = 100f;
			int n_EFFECT = value.n_EFFECT;
			if (n_EFFECT == 1)
			{
				num2 = value.f_EFFECT_X + 0f * value.f_EFFECT_Y;
			}
			n_WEAPON_SKILL2.text = string.Format(l10nValue.Replace("{0}", "{0:0.00}"), num2);
			n_WEAPON_SKILL2.alignByGeometry = false;
		}
		else
		{
			s_SkillName2.text = "";
			info2skillimg.gameObject.SetActive(false);
			n_WEAPON_SKILL2.text = "";
		}
		if (tSTAR_TABLE != null && tSTAR_TABLE.n_MATERIAL != 0)
		{
			StarButton.transform.parent.gameObject.SetActive(true);
			needrootinfo2.SetActive(true);
			MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tSTAR_TABLE.n_MATERIAL];
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[mATERIAL_TABLE.n_MATERIAL_1];
			UpdateChipNeedInfo(iTEM_TABLE, info2needimg, info2needfrm, info2needbg, neednameinfo2);
			if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
			{
				needbar2.fillAmount = (float)ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack / (float)mATERIAL_TABLE.n_MATERIAL_MOUNT1;
				needtext2.text = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack + "/" + mATERIAL_TABLE.n_MATERIAL_MOUNT1;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID) && ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack >= mATERIAL_TABLE.n_MATERIAL_MOUNT1)
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
			StarButton.transform.Find("Image").gameObject.SetActive(!StarButton.interactable);
		}
		else
		{
			StarButton.transform.parent.gameObject.SetActive(false);
		}
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

	private void UpdateChipNeedInfo(ITEM_TABLE tITEM_TABLE, StageLoadIcon img, StageLoadIcon frm, StageLoadIcon bg, Text text)
	{
		img.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconItem(tITEM_TABLE.s_ICON), tITEM_TABLE.s_ICON);
		OrangeRareText.Rare n_RARE = (OrangeRareText.Rare)tITEM_TABLE.n_RARE;
		frm.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall((int)n_RARE));
		bg.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareBgSmall((int)n_RARE));
		if (text != null)
		{
			text.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(tITEM_TABLE.w_NAME);
		}
	}

	public void OnNeed2AddFrag()
	{
		MATERIAL_TABLE tMATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tSTAR_TABLE.n_MATERIAL];
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(tMATERIAL_TABLE.n_MATERIAL_1, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item, null, tMATERIAL_TABLE.n_MATERIAL_MOUNT1);
			});
		}
	}

	private void InitInfo3()
	{
		int[] array = new int[5] { tDISC_TABLE.n_ANALYSE_1, tDISC_TABLE.n_ANALYSE_2, tDISC_TABLE.n_ANALYSE_3, tDISC_TABLE.n_ANALYSE_4, tDISC_TABLE.n_ANALYSE_5 };
		int[] array2 = new int[6]
		{
			0,
			OrangeConst.DISC_ANALYSE_1,
			OrangeConst.DISC_ANALYSE_2,
			OrangeConst.DISC_ANALYSE_3,
			OrangeConst.DISC_ANALYSE_4,
			OrangeConst.DISC_ANALYSE_5
		};
		if (tChipInfo.netChipInfo.Analyse == 0)
		{
			analyselvmsg[0].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PERSONAL_LEVEL") + "??";
		}
		else
		{
			analyselvmsg[0].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PERSONAL_LEVEL") + (tChipInfo.netChipInfo.Analyse - 1);
		}
		if (tChipInfo.netChipInfo.Analyse >= 5)
		{
			analyselvmsg[1].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PERSONAL_LEVEL") + "MAX";
			analyselvmsg[2].text = "";
		}
		else
		{
			analyselvmsg[1].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PERSONAL_LEVEL") + tChipInfo.netChipInfo.Analyse;
			if (tChipInfo.netChipInfo.Analyse + 1 >= 5)
			{
				analyselvmsg[2].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PERSONAL_LEVEL") + "MAX";
			}
			else
			{
				analyselvmsg[2].text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PERSONAL_LEVEL") + (tChipInfo.netChipInfo.Analyse + 1);
			}
		}
		analyseratemsg.text = "+" + array2[tChipInfo.netChipInfo.Analyse] + "%";
		if (tChipInfo.netChipInfo.Analyse >= 5)
		{
			if (AnalyseBtn.transform.parent != null)
			{
				AnalyseBtn.transform.parent.gameObject.SetActive(false);
			}
			return;
		}
		if (AnalyseBtn.transform.parent != null)
		{
			AnalyseBtn.transform.parent.gameObject.SetActive(true);
		}
		bool flag = true;
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[array[tChipInfo.netChipInfo.Analyse]];
		int[] array3 = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] array4 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		for (int i = 0; i < analyseitems.Length; i++)
		{
			if (array3[i] != 0)
			{
				analyseitems[i].Button.gameObject.SetActive(true);
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[array3[i]];
				UpdateItemNeedInfo(iTEM_TABLE, analyseitems[i].BtnImgae, analyseitems[i].frmimg, analyseitems[i].bgimg, null);
				analyseitems[i].UnuseBtn.gameObject.SetActive(false);
				int num = 0;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
				{
					num = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
				}
				analyseitems[i].BtnLabel.text = num + "/" + array4[i];
				if (num >= array4[i])
				{
					analyseitems[i].AddBtn.gameObject.SetActive(false);
					analyseitems[i].BtnLabel.color = Color.white;
					analyseitems[i].Button.interactable = true;
					analyseitems[i].frmimg.color = Color.white;
					analyseitems[i].bgimg.color = Color.white;
				}
				else
				{
					analyseitems[i].AddBtn.gameObject.SetActive(true);
					analyseitems[i].BtnLabel.color = Color.red;
					analyseitems[i].Button.interactable = false;
					analyseitems[i].frmimg.color = disablecolor;
					analyseitems[i].bgimg.color = disablecolor;
					flag = false;
				}
			}
			else
			{
				analyseitems[i].Button.gameObject.SetActive(false);
			}
		}
		AnalyseBtn.interactable = flag && ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID);
		AnalyseBtn.transform.Find("Image").gameObject.SetActive(!AnalyseBtn.interactable);
	}

	public void OnChipAnalyse()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		ManagedSingleton<PlayerNetManager>.Instance.ChipAnalyseReq(nTargetChipID, delegate
		{
			if ((bool)tChipMainUI)
			{
				tChipMainUI.SortChips();
			}
			PlayUnlockEffect();
			nNowChipID = -1;
			InitChip();
			UpdateChipCell();
		});
	}

	public void OnChipAnalyseAddBtnCB(int nBtnID)
	{
		int[] array = new int[5] { tDISC_TABLE.n_ANALYSE_1, tDISC_TABLE.n_ANALYSE_2, tDISC_TABLE.n_ANALYSE_3, tDISC_TABLE.n_ANALYSE_4, tDISC_TABLE.n_ANALYSE_5 };
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[array[tChipInfo.netChipInfo.Analyse]];
		int[] array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] materialcounts = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(array2[nBtnID], out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item, null, materialcounts[nBtnID]);
			});
		}
	}

	private void GetChipStatus()
	{
		nNowExp = 0;
		nNeedExp = 0;
		nExpNowLV = 0;
		nNowStar = 0;
		tDISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[nTargetChipID];
		ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(nTargetChipID, out tChipInfo);
		tEXP_TABLE = GetNowLvWithAddExp(nTotalExpItemAddExp);
		if (tChipInfo == null)
		{
			tChipInfo = new ChipInfo();
			tChipInfo.netChipInfo = new NetChipInfo();
			tChipInfo.netChipInfo.ChipID = nTargetChipID;
		}
		nNowExp = nTotalExpItemAddExp + tChipInfo.netChipInfo.Exp - (tEXP_TABLE.n_TOTAL_DISCEXP - tEXP_TABLE.n_DISCEXP);
		nNeedExp = tEXP_TABLE.n_DISCEXP;
		nExpNowLV = tEXP_TABLE.n_ID;
		nNowStar = tChipInfo.netChipInfo.Star;
		Dictionary<int, STAR_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.STAR_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.n_TYPE == 4 && enumerator.Current.Value.n_MAINID == nTargetChipID && nNowStar == enumerator.Current.Value.n_STAR)
			{
				tSTAR_TABLE = enumerator.Current.Value;
				break;
			}
		}
	}

	public void InitChip()
	{
		GetChipStatus();
		if (nNowChipID != nTargetChipID)
		{
			redDot[0].gameObject.SetActive(ManagedSingleton<EquipHelper>.Instance.IsCanChipUpgradeStart(tChipInfo.netChipInfo));
			redDot[1].gameObject.SetActive(ManagedSingleton<EquipHelper>.Instance.IsCanChipAnalyse(tChipInfo.netChipInfo));
			InitInfo0();
			InitExpItem();
			InitInfo2();
			InitInfo3();
		}
		nNowChipID = nTargetChipID;
		int[] array = new int[7]
		{
			0,
			OrangeConst.DISC_ANALYSE_1,
			OrangeConst.DISC_ANALYSE_2,
			OrangeConst.DISC_ANALYSE_3,
			OrangeConst.DISC_ANALYSE_4,
			OrangeConst.DISC_ANALYSE_5,
			OrangeConst.DISC_ANALYSE_5
		};
		int num = array[tChipInfo.netChipInfo.Analyse];
		for (int i = 0; i < analysebar.Length; i++)
		{
			analysebar[i].fillAmount = (float)num / 100f;
			analysetext[i].text = num + "%";
		}
		for (int j = 0; j < BeforeAnalyseLV.Length; j++)
		{
			if (tChipInfo.netChipInfo.Analyse >= 5)
			{
				BeforeAnalyseLV[j].text = "MAX";
			}
			else
			{
				BeforeAnalyseLV[j].text = tChipInfo.netChipInfo.Analyse.ToString();
			}
		}
		for (int k = 0; k < AfterAnalyseLV.Length; k++)
		{
			if (tChipInfo.netChipInfo.Analyse >= 5)
			{
				AfterAnalyseLV[k].text = "MAX";
			}
			else
			{
				AfterAnalyseLV[k].text = (tChipInfo.netChipInfo.Analyse + 1).ToString();
			}
		}
		for (int l = 0; l < BeforeAnalyse.Length; l++)
		{
			BeforeAnalyse[l].text = num.ToString();
		}
		for (int m = 0; m < AfterAnalyse.Length; m++)
		{
			AfterAnalyse[m].text = array[tChipInfo.netChipInfo.Analyse + 1].ToString();
		}
		WeaponStatus chipStatusX = ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(tChipInfo, 0, false, false, null, true);
		WeaponStatus chipStatusX2 = ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(tChipInfo);
		WeaponStatus wsubstatus = new WeaponStatus();
		PlayerStatus pallstatus = new PlayerStatus();
		WeaponStatus weaponStatus = chipStatusX;
		WeaponStatus allChipStatus = ManagedSingleton<StatusHelper>.Instance.GetAllChipStatus();
		if (nNowInfoIndex == 1)
		{
			weaponStatus = ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(tChipInfo, nTotalExpItemAddExp, false, false, null, true);
			if (nTotalExpItemAddExp > 0 && ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(nTargetChipID))
			{
				upgradeBtn.interactable = true;
				upgradeBtn.transform.Find("Image").gameObject.SetActive(!upgradeBtn.interactable);
			}
			else
			{
				upgradeBtn.interactable = false;
				upgradeBtn.transform.Find("Image").gameObject.SetActive(!upgradeBtn.interactable);
			}
		}
		else if (nNowInfoIndex == 2)
		{
			weaponStatus = ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(tChipInfo, 0, true, false, null, true);
			InitInfo2();
		}
		else if (nNowInfoIndex == 3)
		{
			chipStatusX = ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(tChipInfo, 0, false, false, null, false);
			weaponStatus = ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(tChipInfo, 0, false, true, null, false);
		}
		int battlePower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(chipStatusX, wsubstatus, pallstatus);
		ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(weaponStatus, wsubstatus, pallstatus);
		for (int n = 0; n < BeforePower.Length; n++)
		{
			if (n == 3)
			{
				BeforePower[n].text = "+" + chipStatusX2.nATK.ToString();
			}
			else
			{
				BeforePower[n].text = chipStatusX.nATK.ToString();
			}
		}
		for (int num2 = 0; num2 < AfterPower.Length; num2++)
		{
			AfterPower[num2].text = weaponStatus.nATK.ToString();
		}
		for (int num3 = 0; num3 < BeforeHP.Length; num3++)
		{
			if (num3 == 3)
			{
				BeforeHP[num3].text = "+" + chipStatusX2.nHP.ToString();
			}
			else
			{
				BeforeHP[num3].text = chipStatusX.nHP.ToString();
			}
		}
		for (int num4 = 0; num4 < AfterHP.Length; num4++)
		{
			AfterHP[num4].text = weaponStatus.nHP.ToString();
		}
		for (int num5 = 0; num5 < BeforeDEF.Length; num5++)
		{
			if (num5 == 3)
			{
				BeforeDEF[num5].text = "+" + chipStatusX2.nDEF.ToString();
			}
			else
			{
				BeforeDEF[num5].text = chipStatusX.nDEF.ToString();
			}
		}
		for (int num6 = 0; num6 < AfterDEF.Length; num6++)
		{
			AfterDEF[num6].text = weaponStatus.nDEF.ToString();
		}
		AllPower.text = "+" + allChipStatus.nATK.ToString();
		AllHP.text = "+" + allChipStatus.nHP.ToString();
		AllDEF.text = "+" + allChipStatus.nDEF.ToString();
		OrangeRareText.Rare n_RARITY = (OrangeRareText.Rare)tDISC_TABLE.n_RARITY;
		s_NAME[(int)(n_RARITY - 1)].transform.parent.gameObject.SetActive(true);
		StarRoot0.SetActive(true);
		sBattleScore.text = battlePower.ToString();
		if (nNowInfoIndex == 0)
		{
			InitInfo0();
			AttributrText[0].text = chipStatusX.nATK.ToString();
			AttributrBar[0].SetFValue((float)(int)chipStatusX.nATK / (float)OrangeConst.DISC_ATK_MAX);
			AttributrText[1].text = chipStatusX.nHP.ToString();
			AttributrBar[1].SetFValue((float)(int)chipStatusX.nHP / (float)OrangeConst.DISC_HP_MAX);
			AttributrText[2].text = chipStatusX.nDEF.ToString();
			AttributrBar[2].SetFValue((float)(int)chipStatusX.nDEF / (float)OrangeConst.DISC_DEF_MAX);
			AttributrText[3].text = tChipInfo.netChipInfo.Analyse.ToString();
			AttributrBar[3].SetFValue((float)tChipInfo.netChipInfo.Analyse / 5f);
			SKILL_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(tDISC_TABLE.n_SKILL_0, out value))
			{
				AttributrText[4].text = value.f_DISTANCE.ToString();
				AttributrBar[4].SetFValue(value.f_DISTANCE / (float)OrangeConst.WEAPON_DISTANCE_MAX);
				AttributrText[5].text = value.n_RELOAD.ToString();
				AttributrBar[5].SetFValue((float)value.n_RELOAD / (float)OrangeConst.WEAPON_CD_MAX);
			}
			else
			{
				AttributrText[4].text = "";
				AttributrBar[4].SetFValue(0f);
				AttributrText[5].text = "";
				AttributrBar[5].SetFValue(0f);
			}
		}
		else if (nNowInfoIndex == 1)
		{
			UpdateExpBar();
		}
		if (nNowInfoIndex == 2)
		{
			ReSetStar(tChipInfo.netChipInfo.Star, true);
		}
		else
		{
			ReSetStar(tChipInfo.netChipInfo.Star);
		}
	}

	public override void OnClickCloseBtn()
	{
		WeaponModel.gameObject.SetActive(false);
		if (textureObj != null && textureObj.gameObject != null)
		{
			UnityEngine.Object.Destroy(textureObj.gameObject);
			textureObj = null;
		}
		base.OnClickCloseBtn();
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (textureObj != null)
		{
			textureObj.SetCameraActive(enable);
		}
	}

	private IEnumerator ObjScaleCoroutine(float fStart, float fEnd, float fTime, GameObject tObj, Action endcb)
	{
		float fNowValue = fStart;
		float fLeftTime = fTime;
		float fD = (fEnd - fStart) / fTime;
		Vector3 nowScale = new Vector3(fNowValue, fNowValue, 1f);
		tObj.transform.localScale = nowScale;
		while (fLeftTime > 0f)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			float deltaTime = Time.deltaTime;
			fLeftTime -= deltaTime;
			fNowValue = (nowScale.y = (nowScale.x = fNowValue + fD * deltaTime));
			tObj.transform.localScale = nowScale;
		}
		nowScale.x = fEnd;
		nowScale.y = fEnd;
		tObj.transform.localScale = nowScale;
		if (endcb != null)
		{
			endcb();
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
				m_starUpEffect.Play(AddStar[tChipInfo.netChipInfo.Star - 1].transform.position);
			});
		}
		else
		{
			m_starUpEffect.Play(AddStar[tChipInfo.netChipInfo.Star - 1].transform.position);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP02);
	}

	private IEnumerator PlayStarUpEffectAndRefreshMenu()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		yield return new WaitForSeconds(0.5f);
		PlayUpgrade3DEffect();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW02);
		yield return new WaitForSeconds(1f);
		PlayStarUpEffect();
		yield return new WaitForSeconds(0.25f);
		StarButton.interactable = true;
		nNowChipID = -1;
		InitChip();
		UpdateChipCell();
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

	public void PlayUpgrade3DEffect()
	{
		Vector3 offset = new Vector3(0f, 5f, 0f);
		if (m_upgrade3DEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgrade3deffect", "Upgrade3DEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_upgrade3DEffect = gameObject.GetComponent<Upgrade3DEffect>();
				m_upgrade3DEffect.Play(WeaponModel.transform.position + offset);
			});
		}
		else
		{
			m_upgrade3DEffect.Play(WeaponModel.transform.position + offset);
		}
	}

	public void PlayLevelUp3DEffect()
	{
		if (m_levelUpEffect != null && m_levelUpWordEffect != null)
		{
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
				m_levelUpEffect.SetActive(false);
			});
			LeanTween.delayedCall(component2.animation.GetState("newAnimation").totalTime, (Action)delegate
			{
				m_levelUpWordEffect.SetActive(false);
			});
		}
	}
}
