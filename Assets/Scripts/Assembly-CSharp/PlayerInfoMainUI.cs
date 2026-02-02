#define RELEASE
using System;
using System.Collections.Generic;
using System.Text;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using OrangeSocket;
using UnityEngine;
using UnityEngine.UI;
using cc;

public class PlayerInfoMainUI : OrangeUIBase
{
	[SerializeField]
	public GameObject PlayerBasicInfo;

	[SerializeField]
	public RawImage StandbyModel;

	[SerializeField]
	private GameObject[] passiveSkillMaterialPos;

	[SerializeField]
	private GameObject PlayerInfoScrollCell;

	[SerializeField]
	private GameObject PlayerNameEdit;

	[SerializeField]
	private GameObject PlayerNameEdit_5_3;

	[SerializeField]
	private GameObject PlayerNameEditRoot;

	[SerializeField]
	private GameObject PlayerNameEditRoot_5_3;

	[SerializeField]
	private GameObject PlayerIDDisplayRoot;

	[SerializeField]
	private GameObject PlayerIDDisplayRoot_5_3;

	[SerializeField]
	private GameObject InputObject;

	[SerializeField]
	private Text NewNameText;

	private bool EditNameSelected;

	[SerializeField]
	private GameObject ContentRoot;

	[SerializeField]
	private GameObject PlayerCharInfoObj;

	[SerializeField]
	private GridLayoutGroup CharGridLayoutGroup;

	[SerializeField]
	private GameObject CharInfoBtnON;

	[SerializeField]
	private GameObject CharInfoBtnOFF;

	[SerializeField]
	private GameObject PlayerWeaponInfoObj;

	[SerializeField]
	private GridLayoutGroup WeaponGridLayoutGroup;

	[SerializeField]
	private GameObject WeaponInfoBtnON;

	[SerializeField]
	private GameObject WeaponInfoBtnOFF;

	[SerializeField]
	private GameObject PlayerChipInfoObj;

	[SerializeField]
	private GridLayoutGroup ChipGridLayoutGroup;

	[SerializeField]
	private GameObject ChipInfoBtnON;

	[SerializeField]
	private GameObject ChipInfoBtnOFF;

	[SerializeField]
	private GameObject PlayerEquipInfoObj;

	[SerializeField]
	private GridLayoutGroup EquipGridLayoutGroup;

	[SerializeField]
	private GameObject EquipInfoBtnON;

	[SerializeField]
	private GameObject EquipInfoBtnOFF;

	[SerializeField]
	private GameObject PlayerSignInfoObj;

	[SerializeField]
	private GridLayoutGroup SignGridLayoutGroup;

	[SerializeField]
	private GameObject SignInfoBtnON;

	[SerializeField]
	private GameObject SignInfoBtnOFF;

	[SerializeField]
	private GameObject PlayerBaseInfoObj;

	[SerializeField]
	private Text PlayerNameText;

	[SerializeField]
	private Text PlayerIDText;

	[SerializeField]
	private Text PlayerIDText_5_3;

	[SerializeField]
	private Text PlayerPowerText;

	[SerializeField]
	private Text TextHP;

	[SerializeField]
	private Text TextATK;

	[SerializeField]
	private Text TextDEF;

	[SerializeField]
	private Text TextHIT;

	[SerializeField]
	private Text TextBLK;

	[SerializeField]
	private Text TextAVD;

	[SerializeField]
	private Text TextCRI_Rate;

	[SerializeField]
	private Text TextCRI_DMG;

	[SerializeField]
	private Text TextCRI_OFT;

	[SerializeField]
	private Text TextCRI_DeRate;

	[SerializeField]
	private Text TextBLK_DeRate;

	[SerializeField]
	private Text TextBLK_OFT;

	[SerializeField]
	private Text PercentText;

	[SerializeField]
	private Text LevelText;

	[SerializeField]
	private GameObject[] ExpBarSub;

	[SerializeField]
	private Text ExpDebugText;

	[SerializeField]
	private Text NowExpText;

	[SerializeField]
	private Text LastExpText;

	[SerializeField]
	private GameObject StandbyCharIcon;

	[SerializeField]
	private GameObject MainWeaponIcon;

	[SerializeField]
	private GameObject SubWeaponIcon;

	[SerializeField]
	private GameObject PlayerIcon;

	[SerializeField]
	private GameObject PlayerSettingObject;

	[SerializeField]
	private GameObject GuildObject;

	[SerializeField]
	private CommonGuildBadge GuildBadge;

	[SerializeField]
	private Text GuildName;

	[SerializeField]
	private GuildPrivilegeHelper GuildPrivilege;

	[SerializeField]
	private Button BasicBtn;

	[SerializeField]
	private Button CollectBtn;

	[SerializeField]
	private Button TrophyBtn;

	[SerializeField]
	private Text BasicText;

	[SerializeField]
	private Text CollectText;

	[SerializeField]
	private Text TrophyText;

	[SerializeField]
	private Text PlayerNameText_5_3;

	[SerializeField]
	private Transform PlayerSignRoot;

	[SerializeField]
	private GameObject SignObject;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_openSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_tabSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_personalitySE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_collectionOPSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_collectionCLSE;

	private RenderTextureObj textureCharObject;

	private string CharJSON;

	private string WeaponJSON;

	private string ChipJSON;

	private string EquipJSON;

	private string FinalStrikeJSON;

	private string SignJSON;

	private int StandbyCharID;

	private int MainWeaponID;

	private int SubWeaponID;

	private string playerID;

	private Color32[] colors = new Color32[2]
	{
		new Color32(52, 47, 58, byte.MaxValue),
		new Color32(185, 234, byte.MaxValue, byte.MaxValue)
	};

	protected override void Awake()
	{
		base.Awake();
		GuildObject.SetActive(false);
	}

	private void OnEnable()
	{
		if (Singleton<GuildSystem>.Instance.HasGuild)
		{
			if (MonoBehaviourSingleton<UIManager>.Instance.IsActive("UI_Channel"))
			{
				Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent += OnSocketMemberKickedEvent;
				Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent += OnSocketGuildRemovedEvent;
			}
		}
		else
		{
			Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent += OnConfirmChangeSceneEvent;
		}
	}

	private void OnDisable()
	{
		Singleton<GuildSystem>.Instance.OnConfirmChangeSceneEvent -= OnConfirmChangeSceneEvent;
		Singleton<GuildSystem>.Instance.OnSocketMemberKickedEvent -= OnSocketMemberKickedEvent;
		Singleton<GuildSystem>.Instance.OnSocketGuildRemovedEvent -= OnSocketGuildRemovedEvent;
	}

	public void SetPlayerSignIcon(int n_ID = 0, bool bOwner = false)
	{
		if (PlayerSignRoot != null && SignObject != null)
		{
			int childCount = PlayerSignRoot.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(PlayerSignRoot.transform.GetChild(i).gameObject);
			}
			GameObject obj = UnityEngine.Object.Instantiate(SignObject, PlayerSignRoot.position, new Quaternion(0f, 0f, 0f, 0f));
			obj.transform.SetParent(PlayerSignRoot);
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.GetComponent<CommonSignBase>().SetupSign(n_ID, bOwner);
		}
	}

	private void OnPlayerGuildInfoRefreshed()
	{
		if (GuildBadge == null)
		{
			Debug.LogError("GuildBadge null in AssetBundle Mode ??");
		}
		else if (GuildPrivilege == null)
		{
			Debug.LogError("GuildPrivilege null in AssetBundle Mode ??");
		}
		else if (GuildUIHelper.SetCommunitySocketGuildInfo(playerID, GuildBadge, GuildName, GuildPrivilege))
		{
			GuildObject.SetActive(true);
		}
		else
		{
			GuildObject.SetActive(false);
		}
	}

	public void OnSelectTopInfo(int p)
	{
		setSelectTopInfo(p);
		OnClickSystemSE(m_tabSE);
	}

	private void setSelectTopInfo(int p)
	{
		BasicBtn.interactable = true;
		BasicText.color = colors[1];
		CollectBtn.interactable = true;
		CollectText.color = colors[1];
		TrophyBtn.interactable = true;
		TrophyText.color = colors[1];
		switch (p)
		{
		case 0:
			PlayerCharInfoObj.SetActive(false);
			PlayerWeaponInfoObj.SetActive(false);
			PlayerChipInfoObj.SetActive(false);
			PlayerEquipInfoObj.SetActive(false);
			PlayerSignInfoObj.SetActive(false);
			BasicBtn.interactable = false;
			BasicText.color = colors[0];
			PlayerBaseInfoObj.SetActive(true);
			UpdateContentHeight(p);
			break;
		case 1:
			PlayerBaseInfoObj.SetActive(false);
			CollectBtn.interactable = false;
			CollectText.color = colors[0];
			PlayerCharInfoObj.SetActive(true);
			PlayerWeaponInfoObj.SetActive(true);
			PlayerChipInfoObj.SetActive(true);
			PlayerEquipInfoObj.SetActive(true);
			PlayerSignInfoObj.SetActive(true);
			UpdateContentHeight();
			break;
		default:
			TrophyBtn.interactable = false;
			TrophyText.color = colors[0];
			break;
		}
	}

	public void OnSelectViewUI(int p)
	{
		PlayerBasicInfo.SetActive(false);
		if (p != 2)
		{
			PlayerBasicInfo.SetActive(true);
		}
	}

	public void OnShowPlayerFrameInfo(bool b)
	{
		OnClickSystemSE(m_personalitySE);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PlayerCustomize", delegate(PlayerCustomizeUI ui)
		{
			ui.bFirstOpen = true;
			ui.OnSelectType();
		});
	}

	private void PresetDetailInfo()
	{
		PlayerStatus playerFinalStatus = ManagedSingleton<StatusHelper>.Instance.GetPlayerFinalStatus();
		TextHP.text = playerFinalStatus.nHP.ToString();
		TextATK.text = playerFinalStatus.nATK.ToString();
		TextDEF.text = playerFinalStatus.nDEF.ToString();
		TextHIT.text = playerFinalStatus.nHIT.ToString();
		TextBLK.text = playerFinalStatus.nDOD.ToString();
		TextAVD.text = playerFinalStatus.nLuck.ToString();
		TextCRI_Rate.text = ((OrangeConst.PLAYER_CRI_BASE + (int)playerFinalStatus.nCRI) / 100).ToString();
		TextCRI_DMG.text = (100 + (OrangeConst.PLAYER_CRIDMG_BASE + (int)playerFinalStatus.nCriDmgPercent) / 100).ToString();
		TextCRI_OFT.text = playerFinalStatus.nReduceCriPercent.ToString();
		TextCRI_DeRate.text = playerFinalStatus.nBlockPercent.ToString();
		TextBLK_DeRate.text = playerFinalStatus.nReduceBlockPercent.ToString();
		TextBLK_OFT.text = ((OrangeConst.PLAYER_PARRYDEF_BASE + (int)playerFinalStatus.nBlockDmgPercent) / 100).ToString();
	}

	private int GetWeaponPower(int wid)
	{
		return ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(wid).nBattlePower;
	}

	private string GetWeaponName(int wid)
	{
		WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[wid];
		return ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(wEAPON_TABLE.w_NAME);
	}

	private RankNameStars.RANK GetWeaponRarity(int wid)
	{
		return (RankNameStars.RANK)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[wid].n_RARITY;
	}

	private int GetWeaponSatr(int wid)
	{
		return ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[wid].netInfo.Star;
	}

	public void Setup(string tid)
	{
		PlayerNameEditRoot.SetActive(false);
		PlayerNameEditRoot_5_3.SetActive(true);
		PlayerIDDisplayRoot.SetActive(false);
		PlayerIDDisplayRoot_5_3.SetActive(true);
		if (tid == null)
		{
			tid = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		}
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		Invoke("OnCloseConnecting", 3f);
		if (MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == tid)
		{
			PlayerNameEdit.SetActive(false);
			PlayerNameEdit_5_3.SetActive(true);
			PlayerSettingObject.SetActive(true);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.UpdateCommunityPlayerInfo();
			SetPlayerInfo(MonoBehaviourSingleton<OrangeCommunityManager>.Instance.tmpPlayerInfo);
		}
		else
		{
			PlayerSettingObject.SetActive(false);
			PlayerNameEdit.SetActive(false);
			PlayerNameEdit_5_3.SetActive(false);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetTargetInfo, OnCreateRSGetTargetInfoCallback);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetTargetInfo(tid));
		}
		EditNameSelected = false;
		InputObject.SetActive(false);
		setSelectTopInfo(0);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnCloseConnecting()
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	private void Start()
	{
	}

	public void OnTestClick()
	{
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSGetTargetInfo, OnCreateRSGetTargetInfoCallback);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_PLAYER_INFO_MAIN_WEAPON_CHANGE, NotifyMainWeaponChange);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.UpdatePlayerHUD();
	}

	public override void OnClickCloseBtn()
	{
		if (null != textureCharObject)
		{
			UnityEngine.Object.Destroy(textureCharObject.gameObject);
		}
		base.OnClickCloseBtn();
	}

	public void UpdateExpBar(int lv, int exp)
	{
		LevelText.text = string.Concat(lv);
		int num = 0;
		int num2 = 0;
		if (lv <= 0)
		{
			lv = 1;
		}
		if (lv >= ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.Count)
		{
			lv = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.Count - 1;
		}
		EXP_TABLE eXP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[lv - 1];
		EXP_TABLE eXP_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[lv];
		EXP_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT.TryGetValue(lv, out value);
		if (value == null)
		{
			num = 100;
		}
		else
		{
			num2 = exp - eXP_TABLE.n_TOTAL_RANKEXP;
			num = num2 * 100 / value.n_RANKEXP;
		}
		PercentText.text = num + "%";
		NowExpText.text = num2.ToString();
		LastExpText.text = value.n_RANKEXP.ToString();
		int num3 = num / 5;
		for (int i = 0; i < 20; i++)
		{
			ExpBarSub[i].SetActive(i <= num3);
		}
	}

	private void DrawCharModel(int CharID, int SkinID)
	{
		if (null != textureCharObject)
		{
			UnityEngine.Object.Destroy(textureCharObject.gameObject);
			Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_PLAYER_INFO_MAIN_WEAPON_CHANGE, NotifyMainWeaponChange);
		}
		SKIN_TABLE skinTable = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.ContainsKey(SkinID))
		{
			skinTable = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[SkinID];
		}
		ModelRotateDrag objDrag = StandbyModel.GetComponent<ModelRotateDrag>();
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(UnityEngine.Object obj)
		{
			int num = MainWeaponID;
			textureCharObject = UnityEngine.Object.Instantiate((GameObject)obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
			if (!ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.ContainsKey(CharID) || CharID <= 0)
			{
				CharID = 1;
			}
			if (!ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(num) || num <= 0)
			{
				num = 1;
			}
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(num) && ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num].n_ENABLE_FLAG == 0)
			{
				num = 1;
			}
			textureCharObject.CharacterDebutForceLoop = true;
			textureCharObject.AssignNewRender(ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[CharID], ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num], skinTable, new Vector3(0f, -0.6f, 5f), StandbyModel, 1);
			if ((bool)objDrag)
			{
				objDrag.SetModelTransform(textureCharObject.RenderPosition);
			}
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
		});
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UI_PLAYER_INFO_MAIN_WEAPON_CHANGE, NotifyMainWeaponChange);
	}

	public void NotifyMainWeaponChange()
	{
	}

	public void OnOpenCharacterInfoSelectUI()
	{
		OnClickSystemSE(m_openSE);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Select", delegate(CharacterInfoSelect ui)
		{
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(Start));
			ui.Setup();
		});
	}

	private void SetPlayerInfo(SocketPlayerInfoTmp rs)
	{
		SocketPlayerHUD ph = JsonHelper.Deserialize<SocketPlayerHUD>(rs.PlayerHUD);
		if (ph == null)
		{
			ph = new SocketPlayerHUD();
		}
		SocketPlayerInfo socketPlayerInfo = JsonHelper.Deserialize<SocketPlayerInfo>(rs.InfoJSON);
		if (socketPlayerInfo == null)
		{
			socketPlayerInfo = new SocketPlayerInfo();
		}
		playerID = ph.m_PlayerId;
		PlayerNameText.text = ph.m_Name;
		PlayerNameText_5_3.text = ph.m_Name;
		PlayerIDText_5_3.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("USER_ID"), ph.m_PlayerId);
		PlayerIDText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("USER_ID"), ph.m_PlayerId);
		PlayerPowerText.text = ph.m_Power.ToString();
		Singleton<GuildSystem>.Instance.RefreshCommunityPlayerGuildInfoCache(new List<string> { playerID }, OnPlayerGuildInfoRefreshed);
		UpdateExpBar(ph.m_Level, ph.m_Exp);
		if (playerID == null || playerID == "")
		{
			return;
		}
		byte[] bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(rs.CharJSON));
		string @string = Encoding.UTF8.GetString(bytes);
		CharJSON = @string;
		bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(rs.WeaponJSON));
		@string = Encoding.UTF8.GetString(bytes);
		WeaponJSON = @string;
		bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(rs.ChipJSON));
		@string = Encoding.UTF8.GetString(bytes);
		ChipJSON = @string;
		bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(rs.EquipJSON));
		@string = Encoding.UTF8.GetString(bytes);
		EquipJSON = @string;
		bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(rs.FinalStrikeJSON));
		@string = Encoding.UTF8.GetString(bytes);
		FinalStrikeJSON = @string;
		SetPlayerSignIcon(ph.m_TitleNumber, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == ph.m_PlayerId);
		if (rs.TitleJSON == null || rs.TitleJSON == "")
		{
			@string = JsonConvert.SerializeObject(new List<int>());
			bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(Convert.ToBase64String(LZ4Helper.EncodeWithHeader(Encoding.UTF8.GetBytes(@string)))));
			SignJSON = Encoding.UTF8.GetString(bytes);
		}
		else
		{
			bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(rs.TitleJSON));
			@string = Encoding.UTF8.GetString(bytes);
			SignJSON = @string;
		}
		StandbyCharID = ph.m_StandbyCharID;
		MainWeaponID = ph.m_MainWeaponID;
		SubWeaponID = ph.m_SubWeaponID;
		Dictionary<int, CharacterInfo> dictionary = JsonConvert.DeserializeObject<Dictionary<int, CharacterInfo>>(CharJSON);
		DrawCharModel(ph.m_StandbyCharID, dictionary[ph.m_StandbyCharID].netInfo.Skin);
		Dictionary<int, CharacterInfo> dicCharData = JsonConvert.DeserializeObject<Dictionary<int, CharacterInfo>>(CharJSON);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "PlayerIconBase", "PlayerIconBase", delegate(GameObject asset)
		{
			GameObject obj = UnityEngine.Object.Instantiate(asset, PlayerIcon.transform);
			obj.GetComponent<PlayerIconBase>().Setup(ph.m_IconNumber, MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == ph.m_PlayerId);
			obj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		});
		if (!dicCharData.ContainsKey(ph.m_StandbyCharID))
		{
			ph.m_StandbyCharID = 1;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseBig", "CommonIconBaseBig", delegate(GameObject asset)
		{
			UnityEngine.Object.Instantiate(asset, StandbyCharIcon.transform).GetComponent<CommonIconBase>().SetPlayerCharacterInfo(dicCharData[ph.m_StandbyCharID].netInfo);
		});
		if (ph.m_MainWeaponID != 0)
		{
			WEAPON_TABLE tWeapon_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[ph.m_MainWeaponID];
			Dictionary<int, WeaponInfo> dicWeaponData2 = JsonConvert.DeserializeObject<Dictionary<int, WeaponInfo>>(WeaponJSON);
			if (!dicWeaponData2.ContainsKey(ph.m_MainWeaponID))
			{
				ph.m_MainWeaponID = 100001;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseBig", "CommonIconBaseBig", delegate(GameObject asset)
			{
				CommonIconBase component2 = UnityEngine.Object.Instantiate(asset, MainWeaponIcon.transform).GetComponent<CommonIconBase>();
				component2.SetPlayerWeaponInfo(dicWeaponData2[ph.m_MainWeaponID], dicWeaponData2[ph.m_MainWeaponID].netInfo, CommonIconBase.WeaponEquipType.Main);
				component2.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, tWeapon_TABLE2.s_ICON);
			});
		}
		else
		{
			MainWeaponIcon.gameObject.SetActive(false);
		}
		if (ph.m_SubWeaponID != 0)
		{
			WEAPON_TABLE tWeapon_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[ph.m_SubWeaponID];
			Dictionary<int, WeaponInfo> dicWeaponData = JsonConvert.DeserializeObject<Dictionary<int, WeaponInfo>>(WeaponJSON);
			if (!dicWeaponData.ContainsKey(ph.m_SubWeaponID))
			{
				ph.m_SubWeaponID = 100001;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseBig", "CommonIconBaseBig", delegate(GameObject asset)
			{
				CommonIconBase component = UnityEngine.Object.Instantiate(asset, SubWeaponIcon.transform).GetComponent<CommonIconBase>();
				component.SetPlayerWeaponInfo(dicWeaponData[ph.m_SubWeaponID], dicWeaponData[ph.m_SubWeaponID].netInfo, CommonIconBase.WeaponEquipType.Sub);
				component.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, tWeapon_TABLE.s_ICON);
			});
		}
		else
		{
			SubWeaponIcon.gameObject.SetActive(false);
		}
		TextHP.text = socketPlayerInfo.m_HP.ToString();
		TextATK.text = socketPlayerInfo.m_ATK.ToString();
		TextDEF.text = socketPlayerInfo.m_DEF.ToString();
		TextHIT.text = socketPlayerInfo.m_HIT.ToString();
		TextBLK.text = socketPlayerInfo.m_BLK + "%";
		TextAVD.text = socketPlayerInfo.m_AVD.ToString();
		TextCRI_Rate.text = socketPlayerInfo.m_CRI_Rate + "%";
		TextCRI_DMG.text = socketPlayerInfo.m_CRI_DMG + "%";
		TextCRI_OFT.text = socketPlayerInfo.m_CRI_OFT + "%";
		TextCRI_DeRate.text = socketPlayerInfo.m_CRI_DeRate + "%";
		TextBLK_DeRate.text = socketPlayerInfo.m_BLK_DeRate + "%";
		TextBLK_OFT.text = socketPlayerInfo.m_BLK_OFT + "%";
		CharGLG(false);
		WeaponGLG(false);
		ChipGLG(false);
		EquipGLG(false);
		SignGLG(false);
	}

	private void UpdateContentHeight(int p = 1)
	{
		float num = 0f;
		if (p == 0)
		{
			num += PlayerBaseInfoObj.GetComponent<RectTransform>().sizeDelta.y;
		}
		else
		{
			num += PlayerCharInfoObj.GetComponent<RectTransform>().sizeDelta.y;
			num += PlayerWeaponInfoObj.GetComponent<RectTransform>().sizeDelta.y;
			num += PlayerChipInfoObj.GetComponent<RectTransform>().sizeDelta.y;
			num += PlayerEquipInfoObj.GetComponent<RectTransform>().sizeDelta.y;
			num += PlayerSignInfoObj.GetComponent<RectTransform>().sizeDelta.y;
		}
		Vector2 sizeDelta = ContentRoot.GetComponent<RectTransform>().sizeDelta;
		ContentRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, num);
	}

	public void OnCreateRSGetTargetInfoCallback(object res)
	{
		if (res is RSGetTargetInfo)
		{
			RSGetTargetInfo rSGetTargetInfo = (RSGetTargetInfo)res;
			SocketPlayerInfoTmp playerInfo = default(SocketPlayerInfoTmp);
			playerInfo.PlayerHUD = rSGetTargetInfo.PlayerHUD;
			playerInfo.InfoJSON = rSGetTargetInfo.InfoJSON;
			playerInfo.CharJSON = rSGetTargetInfo.CharJSON;
			playerInfo.WeaponJSON = rSGetTargetInfo.WeaponJSON;
			playerInfo.ChipJSON = rSGetTargetInfo.ChipJSON;
			playerInfo.EquipJSON = rSGetTargetInfo.EquipJSON;
			playerInfo.FinalStrikeJSON = rSGetTargetInfo.FinalStrikeJSON;
			playerInfo.TitleJSON = rSGetTargetInfo.TitleJSON;
			SetPlayerInfo(playerInfo);
		}
	}

	private int CalculateRows(bool bShow, int lineCount, ref int count)
	{
		int num = count / lineCount;
		if (count % lineCount > 0)
		{
			num++;
		}
		if (!bShow)
		{
			num = 1;
			if (count > lineCount)
			{
				count = lineCount;
			}
		}
		return (num <= 0) ? 1 : num;
	}

	private CommonIconBase.WeaponEquipType CheckWeaponEquipType(int wid)
	{
		if (wid == MainWeaponID)
		{
			return CommonIconBase.WeaponEquipType.Main;
		}
		if (wid == SubWeaponID)
		{
			return CommonIconBase.WeaponEquipType.Sub;
		}
		return CommonIconBase.WeaponEquipType.UnEquip;
	}

	public void SetCharGLG(bool bShow)
	{
		if (bShow)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01);
		}
		CharGLG(bShow);
		UpdateContentHeight();
	}

	public void CharGLG(bool bShow)
	{
		int count = JsonConvert.DeserializeObject<Dictionary<int, CharacterInfo>>(CharJSON).Count;
		CharInfoBtnON.SetActive(!bShow && count > 6);
		CharInfoBtnOFF.SetActive(bShow && count > 6);
		int num = CalculateRows(bShow, 6, ref count);
		float y = 170 * num + 110;
		Vector2 sizeDelta = PlayerCharInfoObj.GetComponent<RectTransform>().sizeDelta;
		PlayerCharInfoObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, y);
		int childCount = CharGridLayoutGroup.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(CharGridLayoutGroup.transform.GetChild(i).gameObject);
		}
		Dictionary<int, CharacterInfo>.Enumerator enumerator = JsonConvert.DeserializeObject<Dictionary<int, CharacterInfo>>(CharJSON).GetEnumerator();
		int num2 = 0;
		while (enumerator.MoveNext() && num2 + 1 <= count)
		{
			CharacterInfo value = enumerator.Current.Value;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.ContainsKey(value.netInfo.CharacterID))
			{
				GameObject obj = UnityEngine.Object.Instantiate(PlayerInfoScrollCell, CharGridLayoutGroup.transform.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(CharGridLayoutGroup.transform);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<PlayerInfoScrollCell>().SetupCharacter(value, StandbyCharID);
				num2++;
			}
		}
	}

	public void SetWeaponGLG(bool bShow)
	{
		if (bShow)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01);
		}
		WeaponGLG(bShow);
		UpdateContentHeight();
	}

	public void WeaponGLG(bool bShow)
	{
		int count = JsonConvert.DeserializeObject<Dictionary<int, WeaponInfo>>(WeaponJSON).Count;
		WeaponInfoBtnON.SetActive(!bShow && count > 6);
		WeaponInfoBtnOFF.SetActive(bShow && count > 6);
		int num = CalculateRows(bShow, 6, ref count);
		float y = 170 * num + 110;
		Vector2 sizeDelta = PlayerWeaponInfoObj.GetComponent<RectTransform>().sizeDelta;
		PlayerWeaponInfoObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, y);
		int childCount = WeaponGridLayoutGroup.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(WeaponGridLayoutGroup.transform.GetChild(i).gameObject);
		}
		Dictionary<int, WeaponInfo>.Enumerator enumerator = JsonConvert.DeserializeObject<Dictionary<int, WeaponInfo>>(WeaponJSON).GetEnumerator();
		int num2 = 0;
		while (enumerator.MoveNext() && num2 + 1 <= count)
		{
			WeaponInfo value = enumerator.Current.Value;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(value.netInfo.WeaponID))
			{
				GameObject obj = UnityEngine.Object.Instantiate(PlayerInfoScrollCell, WeaponGridLayoutGroup.transform.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(WeaponGridLayoutGroup.transform);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<PlayerInfoScrollCell>().SetupWeapon(value, value.netInfo, CheckWeaponEquipType(value.netInfo.WeaponID));
				num2++;
			}
		}
	}

	public void SetChipGLG(bool bShow)
	{
		if (bShow)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01);
		}
		ChipGLG(bShow);
		UpdateContentHeight();
	}

	public void ChipGLG(bool bShow)
	{
		int count = JsonConvert.DeserializeObject<Dictionary<int, ChipInfo>>(ChipJSON).Count;
		ChipInfoBtnON.SetActive(!bShow && count > 6);
		ChipInfoBtnOFF.SetActive(bShow && count > 6);
		int num = CalculateRows(bShow, 6, ref count);
		float y = 170 * num + 110;
		Vector2 sizeDelta = PlayerChipInfoObj.GetComponent<RectTransform>().sizeDelta;
		PlayerChipInfoObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, y);
		int childCount = ChipGridLayoutGroup.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(ChipGridLayoutGroup.transform.GetChild(i).gameObject);
		}
		Dictionary<int, ChipInfo>.Enumerator enumerator = JsonConvert.DeserializeObject<Dictionary<int, ChipInfo>>(ChipJSON).GetEnumerator();
		int num2 = 0;
		while (enumerator.MoveNext() && num2 + 1 <= count)
		{
			ChipInfo value = enumerator.Current.Value;
			if (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.ContainsKey(value.netChipInfo.ChipID))
			{
				GameObject obj = UnityEngine.Object.Instantiate(PlayerInfoScrollCell, ChipGridLayoutGroup.transform.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(ChipGridLayoutGroup.transform);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<PlayerInfoScrollCell>().SetupChip(value, value.netChipInfo);
				num2++;
			}
		}
	}

	public void SetEquipGLG(bool bShow)
	{
		if (bShow)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01);
		}
		EquipGLG(bShow);
		UpdateContentHeight();
	}

	public void EquipGLG(bool bShow)
	{
		int count = JsonConvert.DeserializeObject<Dictionary<int, EquipInfo>>(EquipJSON).Count;
		EquipInfoBtnON.SetActive(!bShow && count > 6);
		EquipInfoBtnOFF.SetActive(bShow && count > 6);
		int num = CalculateRows(bShow, 6, ref count);
		float y = 170 * num + 110;
		Vector2 sizeDelta = PlayerEquipInfoObj.GetComponent<RectTransform>().sizeDelta;
		PlayerEquipInfoObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, y);
		int childCount = EquipGridLayoutGroup.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(EquipGridLayoutGroup.transform.GetChild(i).gameObject);
		}
		Dictionary<int, EquipInfo>.Enumerator enumerator = JsonConvert.DeserializeObject<Dictionary<int, EquipInfo>>(EquipJSON).GetEnumerator();
		int num2 = 0;
		while (enumerator.MoveNext() && num2 + 1 <= count)
		{
			EquipInfo value = enumerator.Current.Value;
			if (ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.ContainsKey(value.netEquipmentInfo.EquipItemID))
			{
				GameObject obj = UnityEngine.Object.Instantiate(PlayerInfoScrollCell, EquipGridLayoutGroup.transform.position, new Quaternion(0f, 0f, 0f, 0f));
				obj.transform.SetParent(EquipGridLayoutGroup.transform);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.GetComponent<PlayerInfoScrollCell>().SetupEquip(value.netEquipmentInfo);
				num2++;
			}
		}
	}

	public void SetSignGLG(bool bShow)
	{
		if (bShow)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01);
		}
		SignGLG(bShow);
		UpdateContentHeight();
	}

	public void SignGLG(bool bShow)
	{
		int count = JsonConvert.DeserializeObject<List<int>>(SignJSON).Count;
		SignInfoBtnON.SetActive(!bShow && count > 6);
		SignInfoBtnOFF.SetActive(bShow && count > 6);
		int num = count;
		int num2 = CalculateRows(bShow, 3, ref count);
		float num3 = 80 * num2 + 200;
		if (!bShow && num > 6)
		{
			num = 6;
			num3 += 80f;
		}
		Vector2 sizeDelta = PlayerSignInfoObj.GetComponent<RectTransform>().sizeDelta;
		PlayerSignInfoObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, num3);
		int childCount = SignGridLayoutGroup.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(SignGridLayoutGroup.transform.GetChild(i).gameObject);
		}
		List<int> list = JsonConvert.DeserializeObject<List<int>>(SignJSON);
		for (int j = 0; j < num; j++)
		{
			GameObject obj = UnityEngine.Object.Instantiate(SignObject, PlayerSignRoot.position, new Quaternion(0f, 0f, 0f, 0f));
			obj.transform.SetParent(SignGridLayoutGroup.transform);
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.GetComponent<CommonSignBase>().SetupSign(list[j]);
		}
	}

	private void ChangeNickName(string name)
	{
		if (OrangeDataReader.Instance.IsContainForbiddenName(name))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("NAME_ERROR");
			Debug.LogWarning("Contain forbidden name!!!!!");
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.ChangeNickNameReq(name, delegate
		{
			PlayerNameText.text = name;
			PlayerNameText_5_3.text = name;
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD[MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify].m_Name = name;
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.UpdateCommunityPlayerInfo();
		});
	}

	public void OnChangeNickNameClick()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_InputPlayerName", delegate(InputNameUI ui)
		{
			ui.Setup(delegate(object name)
			{
				ChangeNickName((string)name);
			}, PlayerNameText.text);
		});
	}

	public void OnClickCopyPlayerID()
	{
		GUIUtility.systemCopyBuffer = playerID;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
		{
			tipUI.alertSE = 18;
			tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COPY_ID"), true);
		});
	}

	public void OnClickPlayerCustomizeToSign()
	{
		OnClickSystemSE(m_personalitySE);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PlayerCustomize", delegate(PlayerCustomizeUI ui)
		{
			ui.OnSelectType(1);
		});
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (textureCharObject != null)
		{
			textureCharObject.SetCameraActive(enable);
		}
	}

	public void OnClickSystemSE(SystemSE cuid)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(cuid);
	}

	private void OnConfirmChangeSceneEvent()
	{
		OnClickCloseBtn();
	}

	private void OnSocketMemberKickedEvent(string memberId, bool isSelf)
	{
		if (isSelf)
		{
			OnClickCloseBtn();
		}
	}

	private void OnSocketGuildRemovedEvent()
	{
		OnClickCloseBtn();
	}
}
