using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeAudio;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GoCheckUI : OrangeUIBase
{
	private LoopHorizontalScrollRect[] tLVSRs;

	public GameObject refCommonIconBase;

	public GameObject mainweaponroot;

	public GameObject subweaponroot;

	public GameObject characterroot;

	public CharacterColumeSmall.PerCharacterSmallCell refSelectCharacter;

	public string sGoToStageName = "NewStage01_e3";

	public int nGoToDifficult = 1;

	private StageType _stageType;

	private StageMode _stageMode;

	private STAGE_TABLE _stageTable;

	private int nStageID;

	private int nStageCP = 200;

	private CommonIconBase mainweaponicon;

	private CommonIconBase subweaponicon;

	private CommonIconBase charatericon;

	public Text[] s_NAME;

	public Text BattlePower;

	public Text OkPower;

	private GameObject[] StarImgOns;

	public Text sBattleScore;

	public GameObject[] SelImages;

	public Color tNoOkColor;

	private Button[] TapBtns;

	private Text[] TapTexts;

	private Color backColor = Color.blue;

	public RawImage tModeImg;

	private Canvas tModeImgCanvas;

	public CanvasGroup BgCharacter;

	public StageLoadIcon characertimg;

	private Animator tShowModelAniCtrl;

	private RenderTextureObj textureObj;

	public int nMainWeaponID;

	public int nSubWeaponID;

	public int nUseCharacter;

	public bool bJustReturnToLastUI;

	public bool bFromQualifingUI;

	public bool bUseBlackOut;

	private bool bLockNet;

	private float fLetfTime;

	private bool bNeedReInit;

	private Action changecb;

	private Coroutine SoundCoroutine;

	private int ChangeBackupPowerPoint;

	private float delayCharacterVoiceTime = 0.5f;

	public bool bIgnoreSE = true;

	public bool bIsHaveRoom;

	private int epRatio = 1;

	[HideInInspector]
	public List<int> listHasWeapons = new List<int>();

	[HideInInspector]
	public List<int> listFragWeapons = new List<int>();

	[SerializeField]
	private GameObject FatiguedImageRoot;

	[SerializeField]
	private Image[] FatiguedImages;

	public bool bTowerFlag;

	public bool bIsFatigued;

	public int refSelectCharacterID = -1;

	private int[] FATIGUED_RANK_MIN = new int[4]
	{
		0,
		OrangeConst.FATIGUE_RANGE_1 + 1,
		OrangeConst.FATIGUE_RANGE_2 + 1,
		OrangeConst.FATIGUE_RANGE_3 + 1
	};

	private int[] FATIGUED_RANK_MAX = new int[4]
	{
		OrangeConst.FATIGUE_RANGE_1,
		OrangeConst.FATIGUE_RANGE_2,
		OrangeConst.FATIGUE_RANGE_3,
		2147483647
	};

	[HideInInspector]
	public List<int> listUsedPlayerID = new List<int>();

	[HideInInspector]
	public List<int> listUsedWeaponID = new List<int>();

	[HideInInspector]
	public int nStartTime;

	[HideInInspector]
	public int nEndTime;

	[SerializeField]
	private Button GoButton;

	[SerializeField]
	private GameObject SeasonBase;

	[SerializeField]
	private GameObject PVPBase;

	[SerializeField]
	private GameObject[] SeasonMemberList;

	public Callback destroyCB;

	private int[] SeasonCharIDs;

	private CharacterColumeSmall.PerCharacterSmallCell[] SeasonCharCells = new CharacterColumeSmall.PerCharacterSmallCell[3];

	[SerializeField]
	private Transform friendBattleRoot;

	[SerializeField]
	private Transform friendBattleNewRoot;

	[SerializeField]
	private Button CardBtn;

	[SerializeField]
	private GameObject GoCheckRoot;

	[SerializeField]
	private GameObject DeployRoot;

	[SerializeField]
	private CardDeployGoCheck CardDeployGoCheckRoot;

	public CardDeployGoCheck CardDeployScriptRoot;

	[SerializeField]
	private GameObject _goGuildBuffButton;

	private List<PowerPillarInfoData> _powerPillarInfoDataList;

	private GuildBuffFloatInfoUI _guildBuffUFloatUI;

	private DateTime _guildBuffRefreshTime;

	private Coroutine _guildBuffRefreshCheckCoroutine;

	private Dictionary<int, EVENT_TABLE> eventList = new Dictionary<int, EVENT_TABLE>();

	[SerializeField]
	public BonusInfoSub BonusSub;

	private Dictionary<int, string> Character4UniqueSE = new Dictionary<int, string>
	{
		{ 90, "v_rl_unique02" },
		{ 98, "v_ir_chara02" },
		{ 107, "v_tr2_unique02" },
		{ 114, "v_z_unique02" },
		{ 115, "v_x2_unique02" },
		{ 97, "v_cl2_unique02" },
		{ 78, "v_fo_unique02" },
		{ 93, "v_fg_unique02" },
		{ 118, "v_ic_unique02" },
		{ 122, "v_a_unique02" },
		{ 123, "v_sg_unique03" },
		{ 128, "v_re_unique02" },
		{ 129, "v_za_unique02" },
		{ 130, "v_vi_unique02" },
		{ 132, "v_ic_unique02" },
		{ 133, "v_x_unique02" },
		{ 134, "v_z_unique03" },
		{ 138, "v_ic_unique03" }
	};

	private bool _bNewFriendPVP;

	public int NowSelectMode { get; private set; }

	public void SetTowerFlag(bool b)
	{
		bTowerFlag = b;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bTowerBase = b;
	}

	private void UpdateFatiguedImage(int FatiguedValue)
	{
		FatiguedImageRoot.SetActive(bTowerFlag);
		for (int i = 0; i < FATIGUED_RANK_MIN.Length; i++)
		{
			bool flag = FatiguedValue <= FATIGUED_RANK_MAX[i] && FatiguedValue >= FATIGUED_RANK_MIN[i];
			FatiguedImages[i].gameObject.SetActive(flag);
			if (flag)
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.Fatigued = i;
			}
		}
		bIsFatigued = FatiguedValue >= FATIGUED_RANK_MIN[3];
	}

	public void SetSeasonBase(bool b)
	{
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase = b;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nMainWeaponID = 0;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nSubWeaponID = 0;
	}

	private void OnUpdateGoButton()
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
		{
			for (int i = 0; i < SeasonCharIDs.Length; i++)
			{
				if (SeasonCharIDs[i] <= 0)
				{
					GoButton.interactable = false;
					return;
				}
			}
		}
		GoButton.interactable = true;
	}

	public void SetSeasonInfo(int[] CharacterIDs)
	{
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase = true;
		SeasonCharIDs = CharacterIDs;
		PVPBase.SetActive(!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase);
		SeasonBase.SetActive(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase);
		int num = -1;
		for (int i = 0; i < SeasonMemberList.Length; i++)
		{
			if (CharacterIDs[i] > 0)
			{
				SeasonMemberList[i].GetComponent<SeasonMember>().Setup(i, CharacterIDs[i]);
				if (num == -1)
				{
					num = CharacterIDs[i];
				}
			}
		}
		CHARACTER_TABLE tCHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[num];
		StartCoroutine(PlayCharacterVoice(tCHARACTER_TABLE));
		OnUpdateGoButton();
	}

	public bool OnSetCharacter(int cid, CharacterColumeSmall.PerCharacterSmallCell cell)
	{
		Dictionary<int, int> dicSeasonPrepareCharacterIDs = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicSeasonPrepareCharacterIDs;
		for (int i = 0; i < SeasonCharIDs.Length; i++)
		{
			if (SeasonCharIDs[i] == cid)
			{
				SeasonCharIDs[i] = 0;
				SeasonCharCells[i] = null;
				SeasonMemberList[i].GetComponent<SeasonMember>().OnRemoveCharacter();
				dicSeasonPrepareCharacterIDs[i] = 0;
				OnUpdateGoButton();
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList[i] = 0;
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ModifySeasonIconFlag(false, cid, i + 1);
				return false;
			}
		}
		for (int j = 0; j < SeasonCharIDs.Length; j++)
		{
			if (SeasonCharIDs[j] == 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
				SeasonCharIDs[j] = cid;
				SeasonCharCells[j] = cell;
				SeasonMemberList[j].GetComponent<SeasonMember>().Setup(j, cid);
				dicSeasonPrepareCharacterIDs[j] = cid;
				OnUpdateGoButton();
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList[j] = cid;
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ModifySeasonIconFlag(true, cid, j + 1);
				CHARACTER_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(cid, out value))
				{
					StartCoroutine(PlayCharacterVoice(value));
				}
				return true;
			}
		}
		return false;
	}

	public void OnRemoveCharacter(int idx)
	{
		SeasonCharIDs[idx] = 0;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicSeasonPrepareCharacterIDs[idx] = 0;
		OnUpdateGoButton();
	}

	protected override void Awake()
	{
		base.Awake();
		float renderTextureRate = MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRenderTextureRate();
		tModeImg.transform.localScale = new Vector3(renderTextureRate, renderTextureRate, 1f);
		tModeImgCanvas = tModeImg.gameObject.AddOrGetComponent<Canvas>();
		tModeImgCanvas.enabled = false;
	}

	private void Setup(StageType stageType, StageMode stageMode = StageMode.Normal)
	{
		BonusSub.Clear();
		BonusSub.ReCreateInfo(nStageID);
		epRatio = 1;
		SetSeasonBase(stageType == StageType.Season);
		SetTowerFlag(stageType == StageType.Tower);
		_stageType = stageType;
		_stageMode = stageMode;
		RefreshGuildBuffInfo();
		CardBtn.gameObject.SetActive(stageType != StageType.Season);
		tLVSRs = new LoopHorizontalScrollRect[2];
		for (int i = 0; i < 2; i++)
		{
			tLVSRs[i] = base.transform.Find("GoCheckRoot/LVSR" + i).GetComponent<LoopHorizontalScrollRect>();
		}
		TapBtns = new Button[4];
		TapTexts = new Text[4];
		for (int j = 0; j < 4; j++)
		{
			TapBtns[j] = base.transform.Find("TapBtnHLG/TapButton" + j).GetComponent<Button>();
			TapTexts[j] = TapBtns[j].transform.Find("Text").GetComponent<Text>();
		}
		TapBtns[0].interactable = false;
		backColor = TapTexts[0].color;
		TapTexts[0].color = new Color(0.20392157f, 0.18431373f, 0.22745098f);
		SelImages[1].SetActive(false);
		SelImages[2].SetActive(false);
		NowSelectMode = 0;
		StarImgOns = new GameObject[5];
		for (int k = 0; k < 5; k++)
		{
			StarImgOns[k] = base.transform.Find("PVPBase/StarRoot/StarImageOn" + k).gameObject;
			StarImgOns[k].SetActive(false);
		}
		if (mainweaponroot == null)
		{
			mainweaponroot = base.transform.Find("GoCheckRoot/mainweaponroot").gameObject;
		}
		if (subweaponroot == null)
		{
			subweaponroot = base.transform.Find("GoCheckRoot/subweaponroot").gameObject;
		}
		if (characterroot == null)
		{
			characterroot = base.transform.Find("GoCheckRoot/characterroot").gameObject;
		}
		mainweaponicon = UnityEngine.Object.Instantiate(refCommonIconBase, mainweaponroot.transform).GetComponent<CommonIconBase>();
		subweaponicon = UnityEngine.Object.Instantiate(refCommonIconBase, subweaponroot.transform).GetComponent<CommonIconBase>();
		charatericon = UnityEngine.Object.Instantiate(refCommonIconBase, characterroot.transform).GetComponent<CommonIconBase>();
		CharacterInfo characterInfo = null;
		nMainWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		nSubWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
		nUseCharacter = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara;
		characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nUseCharacter];
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.netInfo.CharacterID];
		for (int l = 0; l < s_NAME.Length; l++)
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			s_NAME[l].text = l10nValue;
			s_NAME[l].transform.parent.gameObject.SetActive(false);
		}
		string text = "St_" + cHARACTER_TABLE.s_ICON;
		BgCharacter.alpha = 0f;
		((RectTransform)BgCharacter.transform).anchoredPosition = new Vector2(-1000f, 0f);
		((RectTransform)characertimg.transform).anchoredPosition = ((RectTransform)characertimg.transform).anchoredPosition + new Vector2(-60f, 120f);
		characertimg.CheckLoadPerfab("texture/2d/stand/" + text, text, ShowCharacterImg);
		SetStarCount(characterInfo.netInfo.Star);
		s_NAME[cHARACTER_TABLE.n_RARITY - 1].transform.parent.gameObject.SetActive(true);
		charatericon.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON);
		charatericon.SetOtherInfo(characterInfo.netInfo, true, false, listUsedPlayerID.Contains(characterInfo.netInfo.CharacterID));
		BonusSub.SetCommonIcon(charatericon, nUseCharacter);
		if (TurtorialUI.IsTutorialing())
		{
			tLVSRs[1].horizontal = false;
		}
		ManagedSingleton<CharacterHelper>.Instance.SortCharacterListNoFragmentNoSave();
		ManagedSingleton<EquipHelper>.Instance.SortWeaponListForGoCheck();
		listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
		listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
		refSelectCharacter = null;
		refSelectCharacterID = -1;
		int num = (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Count - ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Count % 2) / 2;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Count % 2 > 0)
		{
			num++;
		}
		UpdateFatiguedImage(characterInfo.netInfo.FatiguedValue);
		tLVSRs[0].totalCount = num;
		num = GetCharacterCurrentIndex();
		if (tLVSRs[0].totalCount <= 5)
		{
			num = 0;
		}
		else if (tLVSRs[0].totalCount - num < 5)
		{
			num = tLVSRs[0].totalCount - 5;
		}
		tLVSRs[0].RefillCells(num);
		num = (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Count - ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Count % 2) / 2;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Count % 2 > 0)
		{
			num++;
		}
		tLVSRs[1].totalCount = num;
		tLVSRs[1].RefillCells();
		SetMainSubWeapon(false);
		tLVSRs[0].gameObject.SetActive(true);
		tLVSRs[1].gameObject.SetActive(false);
		int battlePower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower();
		sBattleScore.text = battlePower.ToString();
		BattlePower.text = battlePower.ToString();
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.ContainsKey(nStageID))
		{
			STAGE_TABLE sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[nStageID];
			nStageCP = sTAGE_TABLE.n_CP;
		}
		OkPower.text = "/" + nStageCP;
		if (battlePower < nStageCP)
		{
			BattlePower.color = tNoOkColor;
		}
		else
		{
			BattlePower.color = Color.white;
		}
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
		{
			InitTextureObj();
		}
		if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status <= 0)
		{
			return;
		}
		STAGE_RULE_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status, out value) || !(value.s_TIP != "null"))
		{
			return;
		}
		string ruleMsg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(value.s_TIP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), ruleMsg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			});
		}, true);
	}

	public void FriendBattleMode(bool bEnable = false, bool bNewFriendPVP = false)
	{
		EnableFriendBattlePanel(bEnable, bNewFriendPVP);
	}

	private void InitTextureObj()
	{
		if (textureObj == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/RenderTextureObj", "RenderTextureObj", ManagedSingleton<PlayerHelper>.Instance.GoCheckRenderTextureCB);
		}
	}

	public void RenderTextureCB(UnityEngine.Object obj)
	{
		if (!(textureObj != null))
		{
			ModelRotateDrag component = tModeImg.GetComponent<ModelRotateDrag>();
			CharacterInfo characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
			SKIN_TABLE skin = null;
			if (characterInfo.netInfo.Skin > 0)
			{
				skin = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[characterInfo.netInfo.Skin];
			}
			textureObj = UnityEngine.Object.Instantiate((GameObject)obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
			textureObj.AssignNewRender(ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[nUseCharacter], ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID], skin, new Vector3(0f, -0.71f, 4.29f), tModeImg);
			if ((bool)component)
			{
				component.SetModelTransform(textureObj.RenderPosition);
			}
			tModeImgCanvas.enabled = true;
		}
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, ResetWeaponID);
		Singleton<GenericEventManager>.Instance.AttachEvent<CHARACTER_TABLE, WEAPON_TABLE, SKIN_TABLE>(EventManager.ID.UPDATE_RENDER_CHARACTER, ResetCharacterID);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<CrusadeSystem>.Instance.OnCrusadeStartEvent += OnCrusadeStartEvent;
		Singleton<PowerTowerSystem>.Instance.OnPowerPillarChangedEvent += OnPowerPillarChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerPillarChangedEvent += OnSocketPowerPillarChangedEvent;
		CheckUIReFocus();
	}

	private void CheckUIReFocus()
	{
		if (MonoBehaviourSingleton<UIManager>.Instance.GetUI<GoCheckUI>("UI_GoCheck") == null)
		{
			return;
		}
		bLockNet = false;
		if (!bNeedReInit)
		{
			return;
		}
		nMainWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		nSubWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
		nUseCharacter = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara;
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[nUseCharacter];
		string text = "St_" + cHARACTER_TABLE.s_ICON;
		characertimg.enabled = false;
		BgCharacter.alpha = 0f;
		((RectTransform)BgCharacter.transform).anchoredPosition = new Vector2(-1000f, 0f);
		characertimg.CheckLoadPerfab("texture/2d/stand/" + text, text, ShowCharacterImg);
		SetMainSubWeapon(false);
		ManagedSingleton<CharacterHelper>.Instance.SortCharacterListNoFragmentNoSave();
		ManagedSingleton<EquipHelper>.Instance.SortWeaponListForGoCheck();
		listHasWeapons = ManagedSingleton<EquipHelper>.Instance.GetUnlockedWeaponList();
		listFragWeapons = ManagedSingleton<EquipHelper>.Instance.GetFragmentWeaponList();
		int num = 0;
		if (NowSelectMode == 0)
		{
			tLVSRs[0].gameObject.SetActive(true);
			tLVSRs[1].gameObject.SetActive(false);
			num = GetCharacterCurrentIndex();
			int num2 = (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Count - ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Count % 2) / 2;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Count % 2 > 0)
			{
				num2++;
			}
			tLVSRs[0].totalCount = num2;
			if (num2 <= 5)
			{
				num = 0;
			}
			else if (num2 - num < 5)
			{
				num = num2 - 5;
			}
			tLVSRs[0].RefillCells(num);
		}
		else
		{
			tLVSRs[0].gameObject.SetActive(false);
			tLVSRs[1].gameObject.SetActive(true);
			int num3 = (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Count - ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Count % 2) / 2;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.Count % 2 > 0)
			{
				num3++;
			}
			tLVSRs[1].totalCount = num3;
			int weaponID = nMainWeaponID;
			if (NowSelectMode == 2)
			{
				weaponID = nSubWeaponID;
			}
			num = GetWeaponPosIdx(weaponID);
			num = (num - num % 2) / 2;
			if (tLVSRs[1].totalCount <= 5)
			{
				num = 0;
			}
			else if (tLVSRs[1].totalCount - num < 5)
			{
				num = tLVSRs[1].totalCount - 5;
			}
			tLVSRs[1].RefillCells(num);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON);
		}
		if (textureObj != null)
		{
			textureObj.gameObject.SetActive(true);
			tModeImgCanvas.enabled = true;
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[nMainWeaponID];
			int skin = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nUseCharacter].netInfo.Skin;
			if (skin > 0)
			{
				SKIN_TABLE sKIN_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[skin];
			}
		}
		else
		{
			InitTextureObj();
		}
		bNeedReInit = false;
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, ResetWeaponID);
		Singleton<GenericEventManager>.Instance.DetachEvent<CHARACTER_TABLE, WEAPON_TABLE, SKIN_TABLE>(EventManager.ID.UPDATE_RENDER_CHARACTER, ResetCharacterID);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.BACK_TO_HOMETOP, OnBackToHometop);
		Singleton<CrusadeSystem>.Instance.OnCrusadeStartEvent -= OnCrusadeStartEvent;
		Singleton<PowerTowerSystem>.Instance.OnPowerPillarChangedEvent -= OnPowerPillarChangedEvent;
		Singleton<PowerTowerSystem>.Instance.OnSocketPowerPillarChangedEvent -= OnSocketPowerPillarChangedEvent;
		if (_guildBuffRefreshCheckCoroutine != null)
		{
			StopCoroutine(_guildBuffRefreshCheckCoroutine);
			_guildBuffRefreshCheckCoroutine = null;
		}
		bNeedReInit = true;
		if (textureObj != null)
		{
			textureObj.gameObject.SetActive(false);
			tModeImgCanvas.enabled = false;
		}
	}

	private void OnDestroy()
	{
		changecb = null;
		if (bJustReturnToLastUI && !bFromQualifingUI)
		{
			CHARACTER_TABLE p_param = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
			CharacterInfo characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
			SKIN_TABLE p_param2 = null;
			if (characterInfo.netInfo.Skin > 0)
			{
				p_param2 = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[characterInfo.netInfo.Skin];
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_CHARACTER, p_param, ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID], p_param2);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_WEAPON, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID);
			CallChangeCB();
		}
		if (null != textureObj)
		{
			UnityEngine.Object.Destroy(textureObj.gameObject);
			textureObj = null;
		}
	}

	private void LateUpdate()
	{
		if (fLetfTime > 0f)
		{
			fLetfTime -= Time.deltaTime;
			if (fLetfTime <= 0f)
			{
				bLockNet = false;
			}
		}
	}

	public static void InitTempPlayerData()
	{
	}

	public void UpdateBonusInfo()
	{
		if (eventList.ContainsKey(nUseCharacter))
		{
			EVENT_TABLE eVENT_TABLE = eventList[nUseCharacter];
		}
		if (eventList.ContainsKey(nMainWeaponID))
		{
			EVENT_TABLE eVENT_TABLE2 = eventList[nMainWeaponID];
		}
		if (eventList.ContainsKey(nSubWeaponID))
		{
			EVENT_TABLE eVENT_TABLE3 = eventList[nSubWeaponID];
		}
	}

	public void SetMainSubWeapon(bool bUpdateCell = true)
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo == null)
		{
			return;
		}
		SetWeaponIcon(mainweaponicon, nMainWeaponID, CommonIconBase.WeaponEquipType.Main);
		SetWeaponIcon(subweaponicon, nSubWeaponID, CommonIconBase.WeaponEquipType.Sub);
		BonusSub.SetCommonIcon(mainweaponicon, nMainWeaponID);
		BonusSub.SetCommonIcon(subweaponicon, nSubWeaponID);
		PlayerStatus playerStatusWithEquip = ManagedSingleton<StatusHelper>.Instance.GetPlayerStatusWithEquip();
		WeaponStatus weaponStatus = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(nMainWeaponID);
		WeaponStatus weaponStatus2 = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(nSubWeaponID);
		WeaponStatus weaponStatus3 = new WeaponStatus();
		Dictionary<int, ChipInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicChip.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if ((nMainWeaponID != 0 && enumerator.Current.Key == ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nMainWeaponID].netInfo.Chip) || (nSubWeaponID != 0 && enumerator.Current.Key == ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[nSubWeaponID].netInfo.Chip))
			{
				weaponStatus3 += ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(enumerator.Current.Value, 0, false, false, null, true);
			}
			else
			{
				weaponStatus3 += ManagedSingleton<StatusHelper>.Instance.GetChipStatusX(enumerator.Current.Value, 0, false, false, null, false);
			}
		}
		int battlePower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(weaponStatus + weaponStatus3, weaponStatus2, playerStatusWithEquip);
		int backupWeaponAllPower = ManagedSingleton<PlayerHelper>.Instance.GetBackupWeaponAllPower();
		ChangeBackupPowerPoint = backupWeaponAllPower;
		sBattleScore.text = battlePower.ToString();
		BattlePower.text = battlePower.ToString();
		if (battlePower < nStageCP)
		{
			BattlePower.color = tNoOkColor;
		}
		else
		{
			BattlePower.color = Color.white;
		}
		if (bUpdateCell)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON);
		}
	}

	private void SetWeaponIcon(CommonIconBase tIcon, int nID, CommonIconBase.WeaponEquipType tType)
	{
		WEAPON_TABLE value = null;
		WeaponInfo value2 = null;
		tIcon.gameObject.SetActive(true);
		if (nID == 0 || !ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(nID, out value) || !ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(nID, out value2))
		{
			tIcon.Setup(0, "", "", OnSelectSubWeaponIcon);
			tIcon.SetOtherInfo(null, tType);
			return;
		}
		bool flag = false;
		flag = listUsedWeaponID.Contains(value2.netInfo.WeaponID);
		if (tType == CommonIconBase.WeaponEquipType.Main)
		{
			tIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON, OnSelectMainWeaponIcon, !flag);
		}
		else
		{
			tIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON, OnSelectSubWeaponIcon, !flag);
		}
		if (value2 != null)
		{
			tIcon.SetOtherInfo(value2.netInfo, tType, false, -1, true, flag);
		}
		else
		{
			tIcon.SetOtherInfo(null, tType);
		}
	}

	private void SetStarCount(int nC)
	{
		int i;
		for (i = 0; i < nC; i++)
		{
			StarImgOns[i].SetActive(true);
		}
		for (; i < 5; i++)
		{
			StarImgOns[i].SetActive(false);
		}
	}

	public void Setup(int stageID, StageMode stageMode = StageMode.Normal)
	{
		STAGE_TABLE value;
		ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(stageID, out value);
		Setup(value, stageMode);
	}

	public void Setup(STAGE_TABLE stageTable, StageMode stageMode = StageMode.Normal)
	{
		if (stageTable != null)
		{
			_stageTable = stageTable;
			nStageID = stageTable.n_ID;
			ManagedSingleton<StageHelper>.Instance.nLastStageID = stageTable.n_ID;
			if (stageTable.n_STAGE_RULE > 0)
			{
				ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = stageTable.n_STAGE_RULE;
			}
			else
			{
				ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = 0;
			}
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bCurrentCoopChallengeMode && stageTable.n_TYPE == 5 && stageTable.n_SECRET != 0)
			{
				ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = stageTable.n_SECRET;
			}
			sGoToStageName = stageTable.s_STAGE;
			nGoToDifficult = stageTable.n_DIFFICULTY;
			Setup((StageType)stageTable.n_TYPE, stageMode);
		}
		else
		{
			Setup(StageType.None, stageMode);
		}
	}

	public void ShowCharacterPos()
	{
		if (tShowModelAniCtrl != null)
		{
			tShowModelAniCtrl.Play("showpos");
		}
	}

	public void OnExchangeWeapon()
	{
		int num = nMainWeaponID;
		int num2 = nSubWeaponID;
		if (num2 == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("WEAPONWIELD_NO_EMPTY"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
			});
		}
		else
		{
			nMainWeaponID = num2;
			nSubWeaponID = num;
			SetMainSubWeapon();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_WEAPON, nMainWeaponID);
		}
	}

	public void OnGoBattle()
	{
		if (SoundCoroutine != null)
		{
			return;
		}
		STAGE_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(nStageID, out value))
		{
			StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
			if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(value, ref condition))
			{
				ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(value, condition);
				return;
			}
		}
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCardCountMax() || (!bIsHaveRoom && ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog()))
		{
			return;
		}
		if (bTowerFlag && bIsFatigued)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str5 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FATIGUE_OVER");
				tipUI.Setup(str5, true);
			});
			return;
		}
		if (MonoBehaviourSingleton<UIManager>.Instance.GetUI<ItemHowToGetUI>("UI_ItemHowToGet") == null || value == null || value.n_TYPE != 1)
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HowToGetStageID = 0;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HowToGetStageDifficulty = 0;
		}
		else
		{
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HowToGetStageID = value.n_MAIN;
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HowToGetStageDifficulty = value.n_DIFFICULTY;
		}
		if (_stageType == StageType.TWSuppress || _stageType == StageType.TWLightning || _stageType == StageType.TWCrusade)
		{
			if (listUsedPlayerID.Contains(nUseCharacter) || listUsedWeaponID.Contains(nMainWeaponID))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI tCommonUI)
				{
					tCommonUI.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					tCommonUI.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOTALWAR_HITMSG1"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
					{
						if (!bLockNet)
						{
							bLockNet = true;
							fLetfTime = 50f;
							if (bIsHaveRoom)
							{
								MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
							}
							else
							{
								tCommonUI.CloseSE = SystemSE.NONE;
								MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK07);
							}
							SoundCoroutine = StartCoroutine(SoundePlayGOCoroutine());
						}
					});
				});
				return;
			}
		}
		else
		{
			if (listUsedPlayerID.Contains(nUseCharacter))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
				{
					string str4 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_PREPARE_ERROR");
					tipUI.Setup(str4, true);
				});
				return;
			}
			if (listUsedWeaponID.Contains(nMainWeaponID))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
				{
					string str3 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_PREPARE_ERROR");
					tipUI.Setup(str3, true);
				});
				return;
			}
			if (listUsedWeaponID.Contains(nSubWeaponID))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
				{
					string str2 = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_PREPARE_ERROR");
					tipUI.Setup(str2, true);
				});
				return;
			}
		}
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase && !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckSeasonCharaterList())
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PREPARE_FAILED");
				tipUI.Setup(str, true);
			});
		}
		else if (!bLockNet)
		{
			bLockNet = true;
			fLetfTime = 50f;
			if (bIsHaveRoom)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK07);
			}
			SoundCoroutine = StartCoroutine(SoundePlayGOCoroutine());
		}
	}

	private IEnumerator SoundePlayGOCoroutine()
	{
		yield return new WaitForSeconds(1f);
		if (bJustReturnToLastUI && bUseBlackOut)
		{
			MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(null, OrangeSceneManager.LoadingType.BLACK, 0f);
			yield return new WaitForSeconds(0.5f);
		}
		STAGE_TABLE value;
		if (bJustReturnToLastUI)
		{
			changecb = CloseUICall;
			CheckChangeUseCharacter();
		}
		else if (nStageID != 0 && ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(nStageID, out value))
		{
			StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
			if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(value, ref condition))
			{
				ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(value, condition);
				yield break;
			}
			changecb = ChangeToStageNet;
			CheckChangeUseCharacter();
		}
	}

	private void CheckChangeUseCharacter()
	{
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
		{
			if (nUseCharacter != ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.CharacterStandby(nUseCharacter, CheckChangeMainWeapon, false);
			}
			else
			{
				CheckChangeMainWeapon();
			}
		}
		else
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nMainWeaponID = nMainWeaponID;
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nSubWeaponID = nSubWeaponID;
			CheckChangeMainWeapon();
		}
	}

	private void CheckChangeMainWeapon()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID != nMainWeaponID)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nMainWeaponID, WeaponWieldType.MainWeapon, CheckChangeSubWeapon, false);
		}
		else
		{
			CheckChangeSubWeapon();
		}
	}

	private void CheckChangeSubWeapon()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID != nSubWeaponID)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nSubWeaponID, WeaponWieldType.SubWeapon, CallChangeCB, false);
		}
		else
		{
			CallChangeCB();
		}
	}

	private void CallChangeCB()
	{
		if (changecb != null)
		{
			changecb();
		}
	}

	private void ChangeToStageNet()
	{
		STAGE_TABLE stage;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetStage(nStageID, out stage))
		{
			return;
		}
		switch (_stageType)
		{
		case StageType.RaidBoss:
			if (nStartTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC || MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC > nEndTime)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.SetupConfirmByKey("COMMON_TIP", "EVENT_OUTDATE", "COMMON_OK", delegate
					{
					});
				}, true);
			}
			else
			{
				ManagedSingleton<PlayerNetManager>.Instance.RaidBossStartReq(nStageID, ChangeToStage);
			}
			break;
		case StageType.TWSuppress:
		case StageType.TWLightning:
		case StageType.TWCrusade:
			if (nStartTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC || MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC > nEndTime)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.SetupConfirmByKey("COMMON_TIP", "EVENT_OUTDATE", "COMMON_OK", delegate
					{
					});
				}, true);
			}
			else
			{
				ManagedSingleton<PlayerNetManager>.Instance.StageStartReq(nStageID, stage.s_STAGE, ManagedSingleton<StageHelper>.Instance.GetStageCrc(nStageID), epRatio, ChangeToStage);
			}
			break;
		case StageType.Crusade:
			Singleton<CrusadeSystem>.Instance.StartCrusade(nStageID);
			break;
		default:
			ManagedSingleton<PlayerNetManager>.Instance.StageStartReq(nStageID, stage.s_STAGE, ManagedSingleton<StageHelper>.Instance.GetStageCrc(nStageID), epRatio, ChangeToStage);
			break;
		}
	}

	private void OnCrusadeStartEvent()
	{
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.CRUSADE;
		ChangeToStage();
	}

	private void ChangeToStage()
	{
		ChangeToStage(sGoToStageName);
	}

	private void ChangeToStage(string S_STAGE)
	{
		bLockNet = false;
		ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID = nMainWeaponID;
		ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID = nSubWeaponID;
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
		{
			ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara = nUseCharacter;
		}
		StageUpdate.bIsHost = true;
		StageUpdate.SetStageName(S_STAGE, nGoToDifficult);
		StageUpdate.StageMode = _stageMode;
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", OrangeSceneManager.LoadingType.STAGE, null, false);
		SoundCoroutine = null;
		base.OnClickCloseBtn();
	}

	private void OnSelectCharacterIcon(int p_param)
	{
		OnSelectCharacter();
	}

	private void SwitchSelectModeTap(int nNextMode)
	{
		TapBtns[NowSelectMode].interactable = true;
		TapTexts[NowSelectMode].color = backColor;
		SelImages[NowSelectMode].SetActive(false);
		NowSelectMode = nNextMode;
		TapBtns[NowSelectMode].interactable = false;
		TapTexts[NowSelectMode].color = new Color(0.20392157f, 0.18431373f, 0.22745098f);
		SelImages[NowSelectMode].SetActive(true);
	}

	public void OnSelectCharacter()
	{
		if (bLockNet)
		{
			return;
		}
		OpenCardDeployRoot();
		if (NowSelectMode == 0)
		{
			if (refSelectCharacterID > 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
				ChangeCharacterAndOpenUI();
				bIgnoreSE = true;
			}
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		SwitchSelectModeTap(0);
		tLVSRs[0].gameObject.SetActive(true);
		tLVSRs[1].gameObject.SetActive(false);
		int num = 0;
		List<CharacterInfo> sortedCharacterList = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList();
		for (int i = 0; i < sortedCharacterList.Count && sortedCharacterList[i].netInfo.CharacterID != nUseCharacter; i++)
		{
			num++;
		}
		num = (num - num % 2) / 2;
		if (tLVSRs[0].totalCount <= 5)
		{
			num = 0;
		}
		else if (tLVSRs[0].totalCount - num < 5)
		{
			num = tLVSRs[0].totalCount - 5;
		}
		tLVSRs[0].RefillCells(num);
	}

	public int GetWeaponPosIdx(int weaponID)
	{
		int num = 0;
		for (num = 0; num < listHasWeapons.Count; num++)
		{
			if (listHasWeapons[num] == weaponID)
			{
				return num;
			}
		}
		return 0;
	}

	private void OnSelectMainWeaponIcon(int p_param)
	{
		OnSelectMainWeapon();
	}

	public void OnSelectMainWeapon()
	{
		if (bLockNet)
		{
			return;
		}
		OpenCardDeployRoot();
		if (NowSelectMode == 1)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
			ChangeMainWeaponAndOpenUI(true);
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		SwitchSelectModeTap(1);
		tLVSRs[0].gameObject.SetActive(false);
		tLVSRs[1].gameObject.SetActive(true);
		int weaponPosIdx = GetWeaponPosIdx(nMainWeaponID);
		weaponPosIdx = (weaponPosIdx - weaponPosIdx % 2) / 2;
		if (tLVSRs[1].totalCount <= 5)
		{
			weaponPosIdx = 0;
		}
		else if (tLVSRs[1].totalCount - weaponPosIdx < 5)
		{
			weaponPosIdx = tLVSRs[1].totalCount - 5;
		}
		if (!TurtorialUI.IsTutorialing())
		{
			tLVSRs[1].RefillCells(weaponPosIdx);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON);
	}

	private void OnSelectSubWeaponIcon(int p_param)
	{
		OnSelectSubWeapon();
	}

	public void OnSelectSubWeapon()
	{
		if (bLockNet)
		{
			return;
		}
		OpenCardDeployRoot();
		if (NowSelectMode == 2)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
			ChangeMainWeaponAndOpenUI(false);
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
		SwitchSelectModeTap(2);
		tLVSRs[0].gameObject.SetActive(false);
		tLVSRs[1].gameObject.SetActive(true);
		int weaponPosIdx = GetWeaponPosIdx(nSubWeaponID);
		weaponPosIdx = (weaponPosIdx - weaponPosIdx % 2) / 2;
		if (tLVSRs[1].totalCount <= 5)
		{
			weaponPosIdx = 0;
		}
		else if (tLVSRs[1].totalCount - weaponPosIdx < 5)
		{
			weaponPosIdx = tLVSRs[1].totalCount - 5;
		}
		if (!TurtorialUI.IsTutorialing())
		{
			tLVSRs[1].RefillCells(weaponPosIdx);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON);
	}

	public void SetSelectWeapon(int nID)
	{
		if (bLockNet)
		{
			return;
		}
		if (NowSelectMode == 1)
		{
			if (nMainWeaponID == nID)
			{
				return;
			}
			if (nSubWeaponID == nID)
			{
				nSubWeaponID = nMainWeaponID;
			}
			int nMainWeaponID2 = nMainWeaponID;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
			nMainWeaponID = nID;
			if (ManagedSingleton<EquipHelper>.Instance.GetWeaponBenchSlot(nMainWeaponID) == 0)
			{
				ChangeBackupPowerPoint = 0;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID == nMainWeaponID)
			{
				SetMainSubWeapon();
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_WEAPON, ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID);
				return;
			}
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nMainWeaponID, WeaponWieldType.MainWeapon, delegate
			{
				SetMainSubWeapon();
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_WEAPON, nMainWeaponID);
			}, false);
		}
		else
		{
			if (NowSelectMode != 2 || nSubWeaponID == nID)
			{
				return;
			}
			if (nMainWeaponID == nID)
			{
				if (nSubWeaponID == 0)
				{
					if (!TurtorialUI.IsTutorialing())
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
						{
							ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
							ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
							ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("WEAPONWIELD_NO_EMPTY"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), null);
						});
					}
					return;
				}
				nMainWeaponID = nSubWeaponID;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_WEAPON, nMainWeaponID);
			}
			int nSubWeaponID2 = nSubWeaponID;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
			nSubWeaponID = nID;
			if (ManagedSingleton<EquipHelper>.Instance.GetWeaponBenchSlot(nSubWeaponID) == 0)
			{
				ChangeBackupPowerPoint = 0;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID == nSubWeaponID)
			{
				SetMainSubWeapon();
				return;
			}
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nSubWeaponID, WeaponWieldType.SubWeapon, delegate
			{
				SetMainSubWeapon();
			}, false);
		}
	}

	public void SetSelectCharacter(CharacterColumeSmall.PerCharacterSmallCell tPerCharacterCell)
	{
		if (bLockNet)
		{
			return;
		}
		if (refSelectCharacterID > 0 && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase && (refSelectCharacter != null || refSelectCharacterID != tPerCharacterCell.nID))
		{
			NetCharacterInfo netCharacterInfo = (NetCharacterInfo)tPerCharacterCell.tNetCharacterInfo;
			bool flag = OnSetCharacter(netCharacterInfo.CharacterID, refSelectCharacter);
			if (refSelectCharacterID == tPerCharacterCell.nID && flag)
			{
				return;
			}
		}
		if (refSelectCharacter != null)
		{
			if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
			{
				refSelectCharacter.tCharacterIconBase.SetOtherInfo((NetCharacterInfo)refSelectCharacter.tNetCharacterInfo, false, false, listUsedPlayerID.Contains(((NetCharacterInfo)refSelectCharacter.tNetCharacterInfo).CharacterID));
			}
			refSelectCharacter.selimage.gameObject.SetActive(false);
		}
		refSelectCharacter = tPerCharacterCell;
		if (refSelectCharacterID == tPerCharacterCell.nID)
		{
			if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
			{
				refSelectCharacter.tCharacterIconBase.SetOtherInfo((NetCharacterInfo)refSelectCharacter.tNetCharacterInfo, true, false, listUsedPlayerID.Contains(((NetCharacterInfo)refSelectCharacter.tNetCharacterInfo).CharacterID));
			}
			refSelectCharacter.selimage.gameObject.SetActive(true);
			return;
		}
		refSelectCharacterID = tPerCharacterCell.nID;
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase && !bIgnoreSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
		}
		else
		{
			bIgnoreSE = false;
		}
		NetCharacterInfo netCharacterInfo2 = (NetCharacterInfo)tPerCharacterCell.tNetCharacterInfo;
		UpdateFatiguedImage(netCharacterInfo2.FatiguedValue);
		if (refSelectCharacter == null)
		{
			return;
		}
		NetCharacterInfo netCharacterInfo3 = (NetCharacterInfo)refSelectCharacter.tNetCharacterInfo;
		if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify != "")
		{
			nUseCharacter = netCharacterInfo3.CharacterID;
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara != nUseCharacter && !bFromQualifingUI)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.CharacterStandby(nUseCharacter, delegate
				{
					CharacterSetByNet();
				}, false);
			}
			else
			{
				CharacterSetByNet();
			}
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara = netCharacterInfo3.CharacterID;
		nUseCharacter = netCharacterInfo3.CharacterID;
		if (!bFromQualifingUI)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.CharacterStandby(nUseCharacter, delegate
			{
				CharacterSetByNet();
			}, false);
		}
		else
		{
			CharacterSetByNet();
		}
	}

	private void OpenCardDeployRoot(bool atv = false)
	{
		GoCheckRoot.gameObject.SetActive(!atv);
		DeployRoot.gameObject.SetActive(atv);
	}

	public void OnSelectCardDeploy()
	{
		if (!bLockNet)
		{
			if (NowSelectMode == 3)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			SwitchSelectModeTap(3);
			OpenCardDeployRoot(true);
			CharacterInfo info = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nUseCharacter];
			CardDeployGoCheckRoot.Setup(info);
		}
	}

	public int GetNowRefSelectCharacter()
	{
		if (refSelectCharacter == null)
		{
			return -1;
		}
		return refSelectCharacter.nID;
	}

	private void CharacterSetByNet()
	{
		NetCharacterInfo netCharacterInfo = (NetCharacterInfo)refSelectCharacter.tNetCharacterInfo;
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[netCharacterInfo.CharacterID];
		SKIN_TABLE sKIN_TABLE = null;
		if (netCharacterInfo.Skin > 0)
		{
			sKIN_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[netCharacterInfo.Skin];
		}
		if ((!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase || SeasonCharIDs == null) && !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
		{
			StartCoroutine(PlayCharacterVoice(cHARACTER_TABLE));
		}
		for (int i = 0; i < s_NAME.Length; i++)
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			s_NAME[i].text = l10nValue;
			s_NAME[i].transform.parent.gameObject.SetActive(false);
		}
		s_NAME[cHARACTER_TABLE.n_RARITY - 1].transform.parent.gameObject.SetActive(true);
		if (netCharacterInfo.Skin > 0)
		{
			charatericon.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + sKIN_TABLE.s_ICON), "icon_" + sKIN_TABLE.s_ICON, OnSelectCharacterIcon, !listUsedPlayerID.Contains(netCharacterInfo.CharacterID));
		}
		else
		{
			charatericon.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON, OnSelectCharacterIcon, !listUsedPlayerID.Contains(netCharacterInfo.CharacterID));
		}
		charatericon.SetOtherInfo((NetCharacterInfo)refSelectCharacter.tNetCharacterInfo, !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase, false, listUsedPlayerID.Contains(netCharacterInfo.CharacterID));
		charatericon.EnableLevel(false);
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
		{
			refSelectCharacter.tCharacterIconBase.SetOtherInfo((NetCharacterInfo)refSelectCharacter.tNetCharacterInfo, true, false, listUsedPlayerID.Contains(((NetCharacterInfo)refSelectCharacter.tNetCharacterInfo).CharacterID));
		}
		refSelectCharacter.selimage.gameObject.SetActive(true);
		SKIN_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(netCharacterInfo.Skin, out value);
		string text = string.Format("St_{0}", cHARACTER_TABLE.s_ICON);
		if (value != null)
		{
			text = string.Format("St_{0}", value.s_ICON);
		}
		characertimg.enabled = false;
		BgCharacter.alpha = 0f;
		((RectTransform)BgCharacter.transform).anchoredPosition = new Vector2(-1000f, 0f);
		characertimg.CheckLoadPerfab(string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, text), text, ShowCharacterImg);
		if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
		{
			if (textureObj == null)
			{
				InitTextureObj();
			}
			else
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_RENDER_CHARACTER, cHARACTER_TABLE, ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[nMainWeaponID], sKIN_TABLE);
			}
		}
		SetStarCount(netCharacterInfo.Star);
		BonusSub.SetCommonIcon(charatericon, netCharacterInfo.CharacterID);
		ChangeBackupPowerPoint = 0;
		SetMainSubWeapon();
	}

	public IEnumerator PlayCharacterVoice(CHARACTER_TABLE tCHARACTER_TABLE)
	{
		bool loaded = false;
		AudioLib.LoadVoice(ref tCHARACTER_TABLE, delegate
		{
			loaded = true;
		});
		yield return new WaitForSeconds(delayCharacterVoiceTime);
		while (!loaded)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
		string value;
		if (Character4UniqueSE.TryGetValue(tCHARACTER_TABLE.n_ID, out value))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Play(AudioLib.GetVoice(ref tCHARACTER_TABLE), value);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Play(AudioLib.GetVoice(ref tCHARACTER_TABLE), 6);
		}
		delayCharacterVoiceTime = 0f;
	}

	private void ShowCharacterImg()
	{
		LeanTween.value(BgCharacter.gameObject, 1000f, 0f, 0.5f).setOnUpdate(delegate(float f)
		{
			BgCharacter.alpha = (1000f - f) / 1000f;
			((RectTransform)BgCharacter.transform).anchoredPosition = new Vector2(0f - f, 0f);
		}).setOnComplete((Action)delegate
		{
			BgCharacter.alpha = 1f;
			((RectTransform)BgCharacter.transform).anchoredPosition = new Vector2(0f, 0f);
		})
			.setEase(LeanTweenType.easeOutCubic);
	}

	private void ChangeCharacterAndOpenUI()
	{
		if (!bLockNet)
		{
			bLockNet = true;
			fLetfTime = 2f;
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara != nUseCharacter)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.CharacterStandby(nUseCharacter, OnOpenCharacterUp, false);
			}
			else
			{
				OnOpenCharacterUp();
			}
		}
	}

	public void OnOpenCharacterUp()
	{
		CharacterInfoUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoUI>("UI_CharacterInfo_Main");
		if (uI != null)
		{
			uI.closeCB = null;
			uI.closeCB = OnOpenCharacterInfoUI;
			uI.OnClickCloseBtn();
			ItemHowToGetUI uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ItemHowToGetUI>("UI_ItemHowToGet");
			if ((bool)uI2)
			{
				uI2.closeCB = null;
				uI2.OnClickCloseBtn();
			}
		}
		else
		{
			OnOpenCharacterInfoUI();
		}
	}

	private void OnOpenCharacterInfoUI()
	{
		ManagedSingleton<CharacterHelper>.Instance.SortCharacterListNoFragmentNoSave();
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Main", delegate(CharacterInfoUI ui)
		{
			int key = refSelectCharacterID;
			List<CharacterInfo> sortedCharacterList = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList();
			for (int i = 0; i < sortedCharacterList.Count; i++)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[key].netInfo.CharacterID == sortedCharacterList[i].netInfo.CharacterID)
				{
					ui.Setup(ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[key], i);
				}
			}
			bNeedReInit = true;
			if (null != textureObj)
			{
				UnityEngine.Object.Destroy(textureObj.gameObject);
				textureObj = null;
			}
			if (refSelectCharacter != null)
			{
				refSelectCharacter.selimage.gameObject.SetActive(false);
			}
			refSelectCharacter = null;
			refSelectCharacterID = -1;
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(CheckUIReFocus));
			CheckChangeUseCharacter();
		});
	}

	private void ChangeMainWeaponAndOpenUI(bool bMain)
	{
		if (bLockNet || (!bMain && nSubWeaponID == 0))
		{
			return;
		}
		bLockNet = true;
		fLetfTime = 2f;
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara != nUseCharacter)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.CharacterStandby(nUseCharacter, delegate
			{
				if (bMain)
				{
					if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID != nMainWeaponID)
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nMainWeaponID, WeaponWieldType.MainWeapon, ChangeSubWeaponAndOpenUI0);
					}
					else
					{
						ChangeSubWeaponAndOpenUI0();
					}
				}
				else if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID != nMainWeaponID)
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nMainWeaponID, WeaponWieldType.MainWeapon, ChangeSubWeaponAndOpenUI1);
				}
				else
				{
					ChangeSubWeaponAndOpenUI1();
				}
			}, false);
		}
		else if (bMain)
		{
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID != nMainWeaponID)
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nMainWeaponID, WeaponWieldType.MainWeapon, ChangeSubWeaponAndOpenUI0);
			}
			else
			{
				ChangeSubWeaponAndOpenUI0();
			}
		}
		else if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID != nMainWeaponID)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nMainWeaponID, WeaponWieldType.MainWeapon, ChangeSubWeaponAndOpenUI1);
		}
		else
		{
			ChangeSubWeaponAndOpenUI1();
		}
	}

	private void ChangeSubWeaponAndOpenUI0()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID != nSubWeaponID)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nSubWeaponID, WeaponWieldType.SubWeapon, OnOpenWeaponUp0);
		}
		else
		{
			OnOpenWeaponUp0();
		}
	}

	private void ChangeSubWeaponAndOpenUI1()
	{
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID != nSubWeaponID)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.WeaponWield(nSubWeaponID, WeaponWieldType.SubWeapon, OnOpenWeaponUp1);
		}
		else
		{
			OnOpenWeaponUp1();
		}
	}

	public void OnOpenWeaponUp0()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
		{
			ui.nTargetWeaponID = nMainWeaponID;
			ui.bNeedInitList = true;
			ui.bUseGoCheckUISort = true;
			bNeedReInit = true;
			if (textureObj != null)
			{
				textureObj.SetCameraActive(false);
				tModeImgCanvas.enabled = false;
			}
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(CheckUIReFocus));
		});
	}

	public void OnOpenWeaponUp1()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
		{
			ui.nTargetWeaponID = nSubWeaponID;
			ui.bNeedInitList = true;
			ui.bUseGoCheckUISort = true;
			bNeedReInit = true;
			if (textureObj != null)
			{
				textureObj.SetCameraActive(false);
				tModeImgCanvas.enabled = false;
			}
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(CheckUIReFocus));
		});
	}

	public void OnOpenWeaponChange0()
	{
	}

	public void OnOpenWeaponChange1()
	{
	}

	protected override void OnBackToHometop()
	{
		changecb = CloseUICall;
		closeCB = null;
		if (bJustReturnToLastUI && !bFromQualifingUI)
		{
			CallChangeCB();
		}
		else
		{
			CheckChangeUseCharacter();
		}
	}

	public override void OnClickCloseBtn()
	{
		if (!bLockNet)
		{
			bLockNet = true;
			changecb = CloseUICall;
			closeCB = null;
			if (bJustReturnToLastUI && !bFromQualifingUI)
			{
				CallChangeCB();
			}
			else
			{
				CheckChangeUseCharacter();
			}
			destroyCB.CheckTargetToInvoke();
		}
	}

	private void CloseUICall()
	{
		if (null != textureObj)
		{
			UnityEngine.Object.Destroy(textureObj.gameObject);
			textureObj = null;
		}
		base.OnClickCloseBtn();
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.HOMETOP;
		SoundCoroutine = null;
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (textureObj != null)
		{
			textureObj.SetCameraActive(enable);
			tModeImgCanvas.enabled = enable;
		}
	}

	private int GetCharacterCurrentIndex()
	{
		int num = 0;
		List<CharacterInfo> sortedCharacterList = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList();
		for (int i = 0; i < sortedCharacterList.Count && sortedCharacterList[i].netInfo.CharacterID != nUseCharacter; i++)
		{
			num++;
		}
		return (num - num % 2) / 2;
	}

	public void SetEPRatio(int ratio)
	{
		if (ratio < 0)
		{
			epRatio = 0;
		}
		else if (ratio > OrangeConst.EPCOST_RATIO_MAX)
		{
			epRatio = OrangeConst.EPCOST_RATIO_MAX;
		}
		else
		{
			epRatio = ratio;
		}
	}

	private void EnableFriendBattlePanel(bool bEnable, bool bNewFriendPVP)
	{
		_bNewFriendPVP = bNewFriendPVP;
		if (_bNewFriendPVP)
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(null);
			friendBattleNewRoot.gameObject.SetActive(bEnable);
			BattlePower.transform.parent.gameObject.SetActive(!bEnable);
			GoButton.gameObject.SetActive(!bEnable);
		}
		else
		{
			friendBattleRoot.gameObject.SetActive(bEnable);
			BattlePower.transform.parent.gameObject.SetActive(!bEnable);
			GoButton.gameObject.SetActive(!bEnable);
		}
	}

	private bool IsCardEquipLimitReached()
	{
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCardCountMax())
		{
			return true;
		}
		if (ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog())
		{
			return true;
		}
		return false;
	}

	public void OnCreateRoomBtn()
	{
		if (bLockNet)
		{
			return;
		}
		EnableBlock();
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		if (IsCardEquipLimitReached())
		{
			DisableBlock();
			return;
		}
		if (_bNewFriendPVP)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPCreateRoom", delegate(FriendPVPCreateRoom friendPVPCreateRoom)
			{
				CheckSelfData(delegate
				{
					friendPVPCreateRoom.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					friendPVPCreateRoom.Setup(nStageID);
					DisableBlock();
				});
			});
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendBattle", delegate(FriendBattleUI friendBattleUI)
		{
			friendBattleUI.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			friendBattleUI.Setup(FriendBattleUI.RoleType.HOST);
			CheckSelfData(delegate
			{
				DisableBlock();
			});
		});
	}

	public void OnJoinRoomBtn()
	{
		if (bLockNet)
		{
			return;
		}
		EnableBlock();
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		if (IsCardEquipLimitReached())
		{
			DisableBlock();
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendBattle", delegate(FriendBattleUI friendBattleUI)
		{
			CheckSelfData(delegate
			{
				friendBattleUI.Setup(FriendBattleUI.RoleType.GUEST);
				DisableBlock();
			});
		});
	}

	public void OnListRoomBtn()
	{
		if (bLockNet)
		{
			return;
		}
		EnableBlock();
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		if (IsCardEquipLimitReached())
		{
			DisableBlock();
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPSelectRoom", delegate(FriendPVPSelectRoom friendPVPSelectRoom)
		{
			CheckSelfData(delegate
			{
				friendPVPSelectRoom.Setup(ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[nStageID]);
				DisableBlock();
			});
		});
	}

	private void CheckSelfData(Action p_cb = null)
	{
		CheckChangeUseCharacter();
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
		{
			ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
			{
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
				if (p_cb != null)
				{
					p_cb();
				}
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
		}, false);
	}

	private void EnableBlock()
	{
		bLockNet = true;
		fLetfTime = 0f;
	}

	private void DisableBlock()
	{
		bLockNet = false;
		fLetfTime = 0f;
	}

	private void ResetWeaponID()
	{
		if (!(MonoBehaviourSingleton<UIManager>.Instance.LastUI == this) && !(MonoBehaviourSingleton<UIManager>.Instance.LastUI == MonoBehaviourSingleton<UIManager>.Instance.GetUI<EmptyBlockUI>("UI_EmptyBlock")))
		{
			nMainWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
			nSubWeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
		}
	}

	private void ResetCharacterID(CHARACTER_TABLE character, WEAPON_TABLE equipWeapon, SKIN_TABLE skinTable)
	{
		if (!(MonoBehaviourSingleton<UIManager>.Instance.LastUI == this))
		{
			nUseCharacter = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara;
		}
	}

	public void OpenCharacterInfoCardUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Card", delegate(CharacterInfoCard ui)
		{
			CharacterInfo info = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[nUseCharacter];
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01;
			bNeedReInit = true;
			ui.Setup(info);
			ui.SetCloseBackgroundRoot(true);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(CheckUIReFocus));
		});
	}

	private void OnPowerPillarChangedEvent()
	{
		RefreshGuildBuffInfo();
	}

	private void OnSocketPowerPillarChangedEvent()
	{
		RefreshGuildBuffInfo();
	}

	private void RefreshGuildBuffInfo()
	{
		if (_guildBuffRefreshCheckCoroutine != null)
		{
			StopCoroutine(_guildBuffRefreshCheckCoroutine);
			_guildBuffRefreshCheckCoroutine = null;
		}
		_powerPillarInfoDataList = Singleton<PowerTowerSystem>.Instance.GetEffectivePowerPillarInfoDataList(_stageTable);
		_goGuildBuffButton.SetActive(_powerPillarInfoDataList.Count > 0);
		_guildBuffRefreshTime = ((_powerPillarInfoDataList.Count > 0) ? _powerPillarInfoDataList.Min((PowerPillarInfoData pillarInfo) => pillarInfo.ExpireTime) : DateTime.MaxValue);
		GuildBuffFloatInfoUI guildBuffUFloatUI = _guildBuffUFloatUI;
		if ((object)guildBuffUFloatUI != null)
		{
			guildBuffUFloatUI.RefreshGuildBuffList(_powerPillarInfoDataList);
		}
		if (_powerPillarInfoDataList.Count > 0)
		{
			_guildBuffRefreshCheckCoroutine = StartCoroutine(CheckGuildBuffCoroutine());
		}
	}

	private IEnumerator CheckGuildBuffCoroutine()
	{
		while (!(MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC >= _guildBuffRefreshTime))
		{
			yield return CoroutineDefine._1sec;
		}
		Singleton<PowerTowerSystem>.Instance.ReqGetPowerPillarInfo();
	}

	public void OnClickGuildBuffBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildBuffFloatInfoUI>("UI_GuildBuffFloatInfo", OnGuildBuffFloatUILoaded);
	}

	private void OnGuildBuffFloatUILoaded(GuildBuffFloatInfoUI ui)
	{
		Vector3 position = _goGuildBuffButton.GetComponent<RectTransform>().position;
		ui.Setup(_powerPillarInfoDataList, position);
		ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnGuildBuffFloatUIClosed));
		_guildBuffUFloatUI = ui;
	}

	private void OnGuildBuffFloatUIClosed()
	{
		_guildBuffUFloatUI = null;
	}
}
