using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GuideUI : OrangeUIBase
{
	public enum GuideType
	{
		POWER = 0,
		OBTAIN = 1
	}

	private enum ToggleType
	{
		SHOP = 1,
		EXP = 2,
		CHARACTER = 3,
		WEAPON = 4,
		CHIP = 5,
		EQUIP = 6
	}

	private enum NaviVoiceType
	{
		SS = 0,
		S = 1,
		A = 2,
		B = 3,
		C = 4,
		none = 5
	}

	public class GuidePowerUnitInfo
	{
		public int Power;

		public int SuggestPower;

		public bool IsVaild;

		public GUIDE_TABLE GuideTable;

		public Callback ClickCB;

		public GuidePowerUnitInfo(int Power, int SuggestPower, bool IsVaild, GUIDE_TABLE GuideTable, Callback ClickCB)
		{
			this.Power = Power;
			this.SuggestPower = SuggestPower;
			this.IsVaild = IsVaild;
			this.GuideTable = GuideTable;
			this.ClickCB = ClickCB;
		}
	}

	public class GuideObtainUnitInfo
	{
		public GUIDE_TABLE GuideTable;

		public Callback ClickCB;

		public GuideObtainUnitInfo(GUIDE_TABLE GuideTable, Callback ClickCB)
		{
			this.GuideTable = GuideTable;
			this.ClickCB = ClickCB;
		}
	}

	[SerializeField]
	private Button tabPower;

	[SerializeField]
	private Button tabObtain;

	[SerializeField]
	private LoopHorizontalScrollRect scrollRect;

	[SerializeField]
	private GuideUiUnit guideUiUnit;

	[SerializeField]
	private GuideToggleUnit guideToggleUnit;

	[SerializeField]
	private Image imgTotlaRank;

	[SerializeField]
	private Image imgTotlaRankProgress;

	[SerializeField]
	private OrangeText textTotalRankVale;

	[SerializeField]
	private OrangeText textRankSug;

	[SerializeField]
	private GameObject goGridToggle;

	[SerializeField]
	private GameObject goDialog;

	[SerializeField]
	private Transform stParent;

	[SerializeField]
	private OrangeText textDialog;

	private StandNaviDb naviDb;

	private Vector2 rankRtDefSize = new Vector2(526f, 16f);

	private int playerRank;

	private List<GuidePowerUnitInfo> listGuideUnitInfo = new List<GuidePowerUnitInfo>();

	private List<GuideObtainUnitInfo> listObtainUnitInfo = new List<GuideObtainUnitInfo>();

	private List<GuideToggleUnit> listGuideToggleUnit = new List<GuideToggleUnit>();

	private CanvasGroup stParentCanvas;

	private NaviVoiceType currentNaviVoiceType = NaviVoiceType.none;

	private OrangeUIBase linkUI;

	private readonly int visualCount = 6;

	private Coroutine coroutineDisplayDialog;

	private readonly WaitForSeconds sec = new WaitForSeconds(0.05f);

	public GuideType _GuideType { get; private set; }

	public int ToggleSelectIdx { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		stParentCanvas = stParent.GetComponent<CanvasGroup>();
		stParentCanvas.alpha = 0f;
		imgTotlaRank.color = Color.clear;
		tabPower.onClick.AddListener(OnClickTabPower);
		tabObtain.onClick.AddListener(OnClickTabObtain);
	}

	public void Setup(GuideType p_guideType = GuideType.POWER)
	{
		closeCB = (Callback)Delegate.Combine(closeCB, new Callback(Clear));
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Clear));
		_GuideType = p_guideType;
		switch (_GuideType)
		{
		case GuideType.POWER:
			goGridToggle.SetActive(false);
			goDialog.SetActive(true);
			break;
		case GuideType.OBTAIN:
			goGridToggle.SetActive(true);
			goDialog.SetActive(false);
			break;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_0"), "ch_navi_0_db", delegate(GameObject obj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj, stParent, false);
			naviDb = gameObject.GetComponent<StandNaviDb>();
			if ((bool)naviDb)
			{
				naviDb.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL);
			}
			StartCoroutine(SetUnitDats());
		});
	}

	private IEnumerator SetUnitDats()
	{
		playerRank = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		int count = ManagedSingleton<OrangeDataManager>.Instance.BPGUIDE_TABLE_DICT.Count;
		if (playerRank > count)
		{
			playerRank = count;
		}
		ToggleSelectIdx = 1;
		Dictionary<int, GUIDE_TABLE> gUIDE_TABLE_DICT = ManagedSingleton<OrangeDataManager>.Instance.GUIDE_TABLE_DICT;
		BPGUIDE_TABLE bpGuide = ManagedSingleton<OrangeDataManager>.Instance.BPGUIDE_TABLE_DICT[playerRank];
		int totalChildPower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower();
		foreach (GUIDE_TABLE value in gUIDE_TABLE_DICT.Values)
		{
			if (value.n_TYPE == 0)
			{
				Callback clickCB = null;
				int currentPower = 0;
				int suggestPower = 0;
				bool flag = value.n_PLAYER_RANK <= playerRank;
				if (flag)
				{
					clickCB = SetChildGuideInfo_Power(value.n_ID, ref bpGuide, out currentPower, out suggestPower);
				}
				listGuideUnitInfo.Add(new GuidePowerUnitInfo(currentPower, suggestPower, flag, value, clickCB));
			}
			else
			{
				if (value.n_PLAYER_RANK > playerRank)
				{
					continue;
				}
				Callback clickCB2 = SetChildGuideInfo_Obtain(value.n_UILINK);
				listObtainUnitInfo.Add(new GuideObtainUnitInfo(value, clickCB2));
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		textTotalRankVale.text = totalChildPower.ToString();
		float num = SetRankProgress(totalChildPower, bpGuide.n_TOTAL_BP, ref imgTotlaRank, true);
		imgTotlaRankProgress.rectTransform.sizeDelta = new Vector2(rankRtDefSize.x * num, rankRtDefSize.y);
		textRankSug.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUIDE_SUGGEST") + bpGuide.n_TOTAL_BP;
		stParentCanvas.alpha = 1f;
		if (listGuideToggleUnit.Count == 0)
		{
			foreach (ToggleType value2 in Enum.GetValues(typeof(ToggleType)))
			{
				GuideToggleUnit guideToggleUnit = UnityEngine.Object.Instantiate(this.guideToggleUnit, goGridToggle.transform);
				guideToggleUnit.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(GetTogglel10nKey(value2)), (int)value2, ToggleSelectIdx == (int)value2);
				listGuideToggleUnit.Add(guideToggleUnit);
			}
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		SetScrollRect();
	}

	private string GetTogglel10nKey(ToggleType toggleType)
	{
		switch (toggleType)
		{
		case ToggleType.SHOP:
			return "GET_GUIDE_1";
		case ToggleType.EXP:
			return "GET_GUIDE_2";
		case ToggleType.CHARACTER:
			return "GET_GUIDE_3";
		case ToggleType.WEAPON:
			return "GET_GUIDE_4";
		case ToggleType.CHIP:
			return "GET_GUIDE_5";
		case ToggleType.EQUIP:
			return "GET_GUIDE_6";
		default:
			return string.Empty;
		}
	}

	private void SetScrollRect()
	{
		int p_totalCount = 0;
		switch (_GuideType)
		{
		case GuideType.POWER:
			p_totalCount = listGuideUnitInfo.Count;
			break;
		case GuideType.OBTAIN:
			p_totalCount = listObtainUnitInfo.Count;
			break;
		}
		scrollRect.OrangeInit(guideUiUnit, visualCount, p_totalCount);
	}

	public GuidePowerUnitInfo GetPowerChildInfo(int idx)
	{
		return listGuideUnitInfo[idx];
	}

	public GuideObtainUnitInfo GetObtainChildInfo(int idx)
	{
		return listObtainUnitInfo[idx];
	}

	private void UpdateRankPowerInfo()
	{
		listGuideUnitInfo.Clear();
		listObtainUnitInfo.Clear();
		StartCoroutine(SetUnitDats());
	}

	public void OnClickTabPower()
	{
		tabObtain.interactable = true;
		tabPower.interactable = false;
		goGridToggle.SetActive(false);
		_GuideType = GuideType.POWER;
		scrollRect.ClearCells();
		SetScrollRect();
	}

	public void OnClickTabObtain()
	{
		tabObtain.interactable = false;
		tabPower.interactable = true;
		goGridToggle.SetActive(true);
		_GuideType = GuideType.OBTAIN;
		scrollRect.ClearCells();
		SetScrollRect();
		OnClickToggleUnit(ToggleSelectIdx);
	}

	public void OnClickToggleUnit(int p_idx)
	{
		ToggleSelectIdx = p_idx;
		foreach (GuideToggleUnit item in listGuideToggleUnit)
		{
			item.SetToggleStage(ToggleSelectIdx);
		}
		bool flag = false;
		Transform transform = scrollRect.content.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			if (!transform.GetChild(i).GetComponentInChildren<GuideUiUnit>(true).UpdateActiveState(p_idx))
			{
				flag = true;
			}
		}
		if (flag)
		{
			scrollRect.content.RebuildLayout();
			scrollRect.ForceUpdateBonds();
		}
	}

	private Callback SetChildGuideInfo_Power(int id, ref BPGUIDE_TABLE bpGuide, out int currentPower, out int suggestPower)
	{
		switch (id)
		{
		default:
			currentPower = 0;
			suggestPower = 0;
			return null;
		case 1:
		{
			int mainWeapon2 = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
			currentPower = ManagedSingleton<PlayerHelper>.Instance.GetCurrentWeaponLvUpPower(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[mainWeapon2].netInfo);
			suggestPower = bpGuide.n_WEAPON_LVUP;
			return delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = mainWeapon2;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(1);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateRankPowerInfo));
					linkUI = ui;
				});
			};
		}
		case 2:
		{
			int mainWeapon = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
			currentPower = ManagedSingleton<PlayerHelper>.Instance.GetCurrentWeaponExpertPower(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[mainWeapon].netExpertInfos);
			suggestPower = bpGuide.n_WEAPON_UPGRADE;
			return delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = mainWeapon;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(2);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateRankPowerInfo));
					linkUI = ui;
				});
			};
		}
		case 3:
		{
			int subWeapon2 = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
			if (subWeapon2 == 0)
			{
				currentPower = 0;
				suggestPower = 0;
				return null;
			}
			currentPower = ManagedSingleton<PlayerHelper>.Instance.GetCurrentWeaponLvUpPower(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[subWeapon2].netInfo);
			suggestPower = bpGuide.n_WEAPON_LVUP;
			return delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = subWeapon2;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(1);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateRankPowerInfo));
					linkUI = ui;
				});
			};
		}
		case 4:
		{
			int subWeapon = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID;
			if (subWeapon == 0)
			{
				currentPower = 0;
				suggestPower = 0;
				return null;
			}
			currentPower = ManagedSingleton<PlayerHelper>.Instance.GetCurrentWeaponExpertPower(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[subWeapon].netExpertInfos);
			suggestPower = bpGuide.n_WEAPON_UPGRADE;
			return delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
				{
					ui.nTargetWeaponID = subWeapon;
					ui.bNeedInitList = true;
					ui.initalization_data();
					ui.OnInfoBtnCB(2);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateRankPowerInfo));
					linkUI = ui;
				});
			};
		}
		case 5:
			currentPower = ManagedSingleton<PlayerHelper>.Instance.GetChipPower();
			suggestPower = bpGuide.n_DISC;
			return delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CHIPMAIN", delegate(ChipMainUI ui)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateRankPowerInfo));
					linkUI = ui;
				});
			};
		case 6:
			currentPower = ManagedSingleton<PlayerHelper>.Instance.GetFsPower();
			suggestPower = bpGuide.n_FS;
			return delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FinalStrikeMain", delegate(FinalStrikeMain ui)
				{
					ui.Setup();
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateRankPowerInfo));
					linkUI = ui;
				});
			};
		case 7:
			currentPower = ManagedSingleton<PlayerHelper>.Instance.GetEquipPower();
			suggestPower = bpGuide.n_EQUIP_MAIN;
			return delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemBox", delegate(ItemBoxUI ui)
				{
					ui.Setup(ItemType.Currency);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateRankPowerInfo));
					linkUI = ui;
				});
			};
		case 8:
			currentPower = ManagedSingleton<PlayerHelper>.Instance.GetEquipEnhancePower();
			suggestPower = bpGuide.n_EQUIP_POWERUP;
			return delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemBox", delegate(ItemBoxUI ui)
				{
					ui.Setup(ItemType.Consumption);
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UpdateRankPowerInfo));
					linkUI = ui;
				});
			};
		}
	}

	private Callback SetChildGuideInfo_Obtain(int p_link)
	{
		return delegate
		{
			HOWTOGET_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.HOWTOGET_TABLE_DICT.TryGetValue(p_link, out value))
			{
				ManagedSingleton<UILinkHelper>.Instance.LoadUI(value);
			}
		};
	}

	public float SetRankProgress(int currentPower, int suggestPower, ref Image rankImg, bool showText = false)
	{
		string empty = string.Empty;
		float num = (float)currentPower / (float)suggestPower;
		if (num > 1f)
		{
			empty = "UI_BattleEnd_rank_SS";
			if (showText)
			{
				SetDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BPGUIDE_RANK_SS"));
				naviDb.Play(1);
				if (currentNaviVoiceType != 0)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 18);
					currentNaviVoiceType = NaviVoiceType.SS;
				}
			}
		}
		else if (num >= 0.85f)
		{
			empty = "UI_BattleEnd_rank_S";
			if (showText)
			{
				SetDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BPGUIDE_RANK_S"));
				naviDb.Play(2);
				if (currentNaviVoiceType != NaviVoiceType.S)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 19);
					currentNaviVoiceType = NaviVoiceType.S;
				}
			}
		}
		else if (num >= 0.7f)
		{
			empty = "UI_BattleEnd_rank_A";
			if (showText)
			{
				SetDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BPGUIDE_RANK_A"));
				naviDb.Play(3);
				if (currentNaviVoiceType != NaviVoiceType.A)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 20);
					currentNaviVoiceType = NaviVoiceType.A;
				}
			}
		}
		else if (num >= 0.5f)
		{
			empty = "UI_BattleEnd_rank_B";
			if (showText)
			{
				SetDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BPGUIDE_RANK_B"));
				naviDb.Play(0);
				if (currentNaviVoiceType != NaviVoiceType.B)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 21);
					currentNaviVoiceType = NaviVoiceType.B;
				}
			}
		}
		else
		{
			empty = "UI_BattleEnd_rank_C";
			if (showText)
			{
				SetDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BPGUIDE_RANK_C"));
				naviDb.Play(4);
				if (currentNaviVoiceType != NaviVoiceType.C)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 22);
					currentNaviVoiceType = NaviVoiceType.C;
				}
			}
		}
		rankImg.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, empty);
		rankImg.color = Color.white;
		return Mathf.Clamp01(num);
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (!enable)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
		}
	}

	private void Clear()
	{
		StopAllCoroutines();
		if (linkUI != null)
		{
			OrangeUIBase orangeUIBase = linkUI;
			orangeUIBase.closeCB = (Callback)Delegate.Remove(orangeUIBase.closeCB, new Callback(UpdateRankPowerInfo));
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
	}

	private void SetDialog(string p_str)
	{
		if (coroutineDisplayDialog != null)
		{
			StopCoroutine(coroutineDisplayDialog);
		}
		coroutineDisplayDialog = StartCoroutine(OnStartDisplayDialog(p_str));
	}

	private IEnumerator OnStartDisplayDialog(string val)
	{
		textDialog.alignByGeometry = false;
		textDialog.text = string.Empty;
		for (int i = 0; i < val.Length; i++)
		{
			yield return sec;
			textDialog.text += val[i];
		}
	}
}
