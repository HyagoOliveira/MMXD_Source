using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using NaughtyAttributes;
using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class BackupSystemUI : OrangeUIBase
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

	private WEAPON_TABLE tWEAPON_TABLE;

	private BACKUP_TABLE tBACKUP_TABLE;

	[SerializeField]
	private Text sBattleScore;

	[SerializeField]
	private GameObject WeaponSelectBGClick;

	[SerializeField]
	private GameObject EquipSelectRoot;

	[SerializeField]
	private LoopVerticalScrollRect ScrollRect;

	[SerializeField]
	private EquipBackupCell BackupScrollCell;

	[SerializeField]
	private GameObject InfoRoot;

	[SerializeField]
	private GameObject ChangeInfoRoot;

	[SerializeField]
	private CanvasGroup InfoRootCanvasGroup;

	[SerializeField]
	private CanvasGroup ChangeInfoRootCanvasGroup;

	[SerializeField]
	private CanvasGroup LightImageCanvasGroup;

	[SerializeField]
	private Image FrameBigImage;

	[SerializeField]
	private Image FrameSmallImage;

	[SerializeField]
	private GameObject SlotLevelRoot;

	[SerializeField]
	private Image[] SlotNextLevelImage;

	[SerializeField]
	private Image[] SlotLevelImage;

	[SerializeField]
	private Text StatusRateText1;

	[SerializeField]
	private Text StatusRateText2;

	[SerializeField]
	private Text sAtk;

	[SerializeField]
	private Text sHp;

	[SerializeField]
	private Text sDef;

	[SerializeField]
	private Image IconRoot;

	[SerializeField]
	private Image IconRootLock;

	[SerializeField]
	private Image IconRootUsed;

	[SerializeField]
	private Image IconRootUnlock;

	[SerializeField]
	private Text LevelupInfoText;

	[SerializeField]
	private Text IconNameText;

	[SerializeField]
	private Button UnLockBtn;

	[SerializeField]
	private Button LevelupBtn;

	[SerializeField]
	private Button EquipBtn;

	[SerializeField]
	private Button WeaponSetBtn;

	[SerializeField]
	private GameObject UpgradeRoot;

	[SerializeField]
	private Button LimitBtn;

	[SerializeField]
	private LoopVerticalScrollRect tInfo0LVSR;

	[SerializeField]
	private GameObject EffectRoot;

	[SerializeField]
	private Image EffectImage1;

	[SerializeField]
	private Image EffectImage2;

	[SerializeField]
	private GameObject LevelUpEffectRoot;

	[BoxGroup("Sound")]
	[Tooltip("武器詳細按鈕音效")]
	public SystemSE m_toWeapon = SystemSE.CRI_SYSTEMSE_SYS_OK17;

	[BoxGroup("Sound")]
	[Tooltip("解鎖後備格音效")]
	public SystemSE m_UnlockSE = SystemSE.CRI_SYSTEMSE_SYS_OK05;

	[BoxGroup("Sound")]
	[Tooltip("選中後備格音效")]
	public SystemSE m_SelectedSE = SystemSE.CRI_SYSTEMSE_SYS_OK08;

	[BoxGroup("Sound")]
	[Tooltip("後備升級特效音效")]
	public SystemSE m_UpgradeEfxSE = SystemSE.CRI_SYSTEMSE_SYS_GLOW02;

	[BoxGroup("Sound")]
	[Tooltip("後備格裝備武器音效")]
	public SystemSE m_ChangedWepSE = SystemSE.CRI_SYSTEMSE_SYS_GLOW03;

	[BoxGroup("Sound")]
	[Tooltip("確定選擇武器音效")]
	public SystemSE m_SelectWepOKSE = SystemSE.CRI_SYSTEMSE_SYS_OK08;

	[BoxGroup("Sound")]
	[Tooltip("後備格裝備解除音效")]
	public SystemSE m_RemoveWepSE = SystemSE.CRI_SYSTEMSE_SYS_OK10;

	public ExpButtonRef[] skillmaterials;

	private Color valuegreen = new Color(0.03137255f, 0.827451f, 0.007843138f);

	private Color disablecolor = new Color(0.39f, 0.39f, 0.39f);

	public int CurrentSelectSlot = 1;

	private int CurrentSelectSlotLevel;

	private NetBenchInfo CurrentSelectBenchInfo;

	private BACKUP_TABLE CurrentSelectBackupTable;

	public EquipBackupCell CurrentSelectBackupCell;

	private bool CurrentMaterialFlag = true;

	private OrangeScrollSePlayer ScrollSEPlayer;

	private UpgradeEffect m_upgradeEffect;

	private bool bEffectLock;

	private bool bFirstOpen = true;

	private Color32[] colors = new Color32[5]
	{
		new Color32(252, 242, 0, byte.MaxValue),
		new Color32(117, 111, 29, byte.MaxValue),
		new Color32(183, 183, 183, byte.MaxValue),
		new Color32(92, byte.MaxValue, 222, byte.MaxValue),
		new Color32(107, 194, 222, byte.MaxValue)
	};

	private Color clear = Color.clear;

	private Color white = Color.white;

	private bool bInitFrameImage;

	private int tweenID1;

	private int tweenID2;

	private int tweenID3;

	private int tweenID4;

	public int tempWeaponID;

	public int tempEquipSelectCheckWeaponID;

	public int tempEquipSelectFrameWeaponID;

	public string GetWeaponName(int wid)
	{
		return ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[wid].w_NAME);
	}

	public int GetCurrentSelectSlot()
	{
		return CurrentSelectSlot;
	}

	public void SetCurrentSelectSlot(int _slot, int _lv, NetBenchInfo _info, BACKUP_TABLE _tbl, EquipBackupCell _cell)
	{
		CurrentSelectSlot = _slot;
		CurrentSelectSlotLevel = _lv;
		CurrentSelectBenchInfo = _info;
		CurrentSelectBackupTable = _tbl;
		CurrentSelectBackupCell = _cell;
		tBACKUP_TABLE = _tbl;
		if (bFirstOpen)
		{
			bFirstOpen = false;
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_SelectedSE);
		}
		StartCoroutine(StageResManager.TweenFloatCoroutine(ChangeInfoRootCanvasGroup.alpha, 0f, 0.2f, delegate(float f)
		{
			ChangeInfoRootCanvasGroup.alpha = f;
		}, delegate
		{
			ChangeInfoRoot.SetActive(false);
			ChangeInfoRootCanvasGroup.alpha = 0f;
			ChangeInfoRoot.SetActive(true);
			UpdateBackupPowerInfo();
			StartCoroutine(StageResManager.TweenFloatCoroutine(ChangeInfoRootCanvasGroup.alpha, 1f, 0.2f, delegate(float f)
			{
				ChangeInfoRootCanvasGroup.alpha = f;
			}, null));
		}));
	}

	public void OnSkillItemAddBtnCB(int nBtnID)
	{
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tBACKUP_TABLE.n_MATERIAL];
		int[] array = new int[4] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4 };
		int[] materialcounts = new int[4] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4 };
		ITEM_TABLE item = null;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[array[nBtnID]].n_ID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item, null, materialcounts[nBtnID]);
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
				});
			});
		}
	}

	private void Start()
	{
		ScrollSEPlayer = ScrollRect.content.GetComponent<OrangeScrollSePlayer>();
	}

	public void Setup()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_KIDOU01);
		for (int i = 0; i < skillmaterials.Length; i++)
		{
			BtnClickCB btnClickCB = new BtnClickCB();
			btnClickCB.nBtnID = i;
			btnClickCB.action = (Action<int>)Delegate.Combine(btnClickCB.action, new Action<int>(OnSkillItemAddBtnCB));
			skillmaterials[i].AddBtn.onClick.AddListener(btnClickCB.OnClick);
			skillmaterials[i].Button.onClick.AddListener(btnClickCB.OnClick);
		}
		UpdateBackupPowerInfo();
		UpdateScrollRect();
		EffectImage1.color = new Color(1f, 1f, 1f, 0f);
		EffectImage2.color = new Color(1f, 1f, 1f, 0f);
		FrameBigImage.color = clear;
		FrameSmallImage.color = clear;
		LightImageCanvasGroup.alpha = 0f;
		StartCoroutine(StageResManager.TweenFloatCoroutine(LightImageCanvasGroup.alpha, 1f, 0.8f, delegate(float f)
		{
			LightImageCanvasGroup.alpha = f;
		}, delegate
		{
		}));
		InfoRootCanvasGroup.alpha = 0f;
		Vector3 Pos = ScrollRect.transform.localPosition;
		StartCoroutine(StageResManager.TweenFloatCoroutine(Pos.x - 100f, Pos.x, 0.6f, delegate(float f)
		{
			Pos.x = f;
			ScrollRect.transform.localPosition = Pos;
		}, delegate
		{
			StartCoroutine(StageResManager.TweenFloatCoroutine(InfoRootCanvasGroup.alpha, 1f, 0.4f, delegate(float f)
			{
				InfoRootCanvasGroup.alpha = f;
			}, delegate
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_KIDOU02);
				FrameBigImage.color = white;
				FrameSmallImage.color = white;
				bInitFrameImage = true;
				if (CurrentSelectBackupCell != null)
				{
					CurrentSelectBackupCell.OnClickBtn();
				}
			}));
		}));
	}

	public void UpdateScrollRect()
	{
		int num = 0;
		Dictionary<int, BACKUP_TABLE>.Enumerator enumerator = ManagedSingleton<OrangeDataManager>.Instance.BACKUP_TABLE_DICT.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.n_SLOT_LV == 0)
			{
				num++;
			}
		}
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		ScrollRect.OrangeInit(BackupScrollCell, num, num);
	}

	public void UpdateCurrentSelectBackupTable()
	{
		List<BACKUP_TABLE> list = (from p in ManagedSingleton<OrangeDataManager>.Instance.BACKUP_TABLE_DICT
			where p.Value.n_SLOT == CurrentSelectSlot && p.Value.n_SLOT_LV == CurrentSelectSlotLevel
			select p into o
			select o.Value).ToList();
		if (list.Count > 0)
		{
			tBACKUP_TABLE = list[0];
			CurrentSelectBackupTable = list[0];
		}
	}

	public void UpdateBackupPowerInfo()
	{
		CurrentMaterialFlag = true;
		IconRootLock.gameObject.SetActive(false);
		IconRootUsed.gameObject.SetActive(false);
		IconRootUnlock.gameObject.SetActive(false);
		IconRoot.gameObject.SetActive(false);
		tempWeaponID = 0;
		sAtk.text = "+0";
		sHp.text = "+0";
		sDef.text = "+0";
		sBattleScore.text = ManagedSingleton<PlayerHelper>.Instance.GetBackupWeaponAllPower().ToString();
		UpdateCurrentSelectBackupTable();
		if (tBACKUP_TABLE != null)
		{
			LevelupInfoText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), tBACKUP_TABLE.n_PLAYER_RANK.ToString());
			CurrentSelectBackupTable = tBACKUP_TABLE;
		}
		IconNameText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NO_EQUIP");
		if (ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.ContainsKey(CurrentSelectSlot))
		{
			CurrentSelectBenchInfo = ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo[CurrentSelectSlot].netBenchInfo;
			CurrentSelectSlotLevel = CurrentSelectBenchInfo.Level;
			UpdateCurrentSelectBackupTable();
			UnLockBtn.gameObject.SetActive(false);
			LevelupBtn.gameObject.SetActive(true);
			LevelupBtn.interactable = true;
			EquipBtn.gameObject.SetActive(true);
			WeaponSetBtn.gameObject.SetActive(false);
			if (CurrentSelectBenchInfo.WeaponID > 0)
			{
				IconRootUsed.gameObject.SetActive(true);
				WeaponSetBtn.gameObject.SetActive(true);
				tempWeaponID = CurrentSelectBenchInfo.WeaponID;
				WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[tempWeaponID];
				IconRoot.gameObject.SetActive(true);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, delegate(Sprite obj)
				{
					IconRoot.sprite = obj;
				});
				IconNameText.text = GetWeaponName(tempWeaponID);
				WeaponInfo tWeaponInfo = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[tempWeaponID];
				WeaponStatus weaponStatusX = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(tWeaponInfo, 0, false, null, delegate
				{
				});
				int n_STATUS_RATE = CurrentSelectBackupTable.n_STATUS_RATE;
				sAtk.text = "+" + Convert.ToInt32((int)weaponStatusX.nATK * n_STATUS_RATE / 100);
				sHp.text = "+" + Convert.ToInt32((int)weaponStatusX.nHP * n_STATUS_RATE / 100);
				sDef.text = "+" + Convert.ToInt32((int)weaponStatusX.nDEF * n_STATUS_RATE / 100);
			}
			else
			{
				IconRootUnlock.gameObject.SetActive(true);
			}
			bool flag = tBACKUP_TABLE.n_MATERIAL == 0;
			UpgradeRoot.SetActive(!flag);
			LimitBtn.gameObject.SetActive(flag);
			for (int i = 0; i < SlotLevelImage.Length; i++)
			{
				SlotLevelImage[i].gameObject.SetActive(i < CurrentSelectSlotLevel);
				SlotNextLevelImage[i].gameObject.SetActive(false);
			}
			if (CurrentSelectSlotLevel < SlotLevelImage.Length)
			{
				SlotNextLevelImage[CurrentSelectSlotLevel].gameObject.SetActive(true);
			}
			if (CurrentSelectBackupTable.n_STATUS_RATE > 0)
			{
				StatusRateText1.text = CurrentSelectBackupTable.n_STATUS_RATE + "%";
				StatusRateText2.text = "+" + CurrentSelectBackupTable.n_STATUS_RATE + "%";
			}
			else
			{
				StatusRateText1.text = CurrentSelectBackupTable.n_STATUS_RATE + "%";
				StatusRateText2.text = CurrentSelectBackupTable.n_STATUS_RATE + "%";
			}
		}
		else
		{
			IconRootLock.gameObject.SetActive(true);
			UpgradeRoot.SetActive(true);
			UnLockBtn.gameObject.SetActive(true);
			LevelupBtn.gameObject.SetActive(false);
			EquipBtn.gameObject.SetActive(false);
			LimitBtn.gameObject.SetActive(false);
			WeaponSetBtn.gameObject.SetActive(false);
			StatusRateText1.text = "0%";
			StatusRateText2.text = "0%";
			for (int j = 0; j < SlotLevelImage.Length; j++)
			{
				SlotLevelImage[j].gameObject.SetActive(false);
				SlotNextLevelImage[j].gameObject.SetActive(false);
			}
		}
		UpdateMaterialItem();
	}

	private void UpdateMaterialItem()
	{
		if (!ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.ContainsKey(tBACKUP_TABLE.n_MATERIAL))
		{
			return;
		}
		MATERIAL_TABLE mATERIAL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT[tBACKUP_TABLE.n_MATERIAL];
		int[] array = new int[5] { mATERIAL_TABLE.n_MATERIAL_1, mATERIAL_TABLE.n_MATERIAL_2, mATERIAL_TABLE.n_MATERIAL_3, mATERIAL_TABLE.n_MATERIAL_4, mATERIAL_TABLE.n_MATERIAL_5 };
		int[] array2 = new int[5] { mATERIAL_TABLE.n_MATERIAL_MOUNT1, mATERIAL_TABLE.n_MATERIAL_MOUNT2, mATERIAL_TABLE.n_MATERIAL_MOUNT3, mATERIAL_TABLE.n_MATERIAL_MOUNT4, mATERIAL_TABLE.n_MATERIAL_MOUNT5 };
		LevelupBtn.interactable = true;
		UnLockBtn.interactable = true;
		CurrentMaterialFlag = true;
		for (int i = 0; i < skillmaterials.Length; i++)
		{
			if (array[i] != 0)
			{
				skillmaterials[i].Button.gameObject.SetActive(true);
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[array[i]];
				UpdateItemNeedInfo(iTEM_TABLE, skillmaterials[i].BtnImgae, skillmaterials[i].frmimg, skillmaterials[i].bgimg, null);
				skillmaterials[i].UnuseBtn.gameObject.SetActive(false);
				int num = 0;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(iTEM_TABLE.n_ID))
				{
					num = ManagedSingleton<PlayerNetManager>.Instance.dicItem[iTEM_TABLE.n_ID].netItemInfo.Stack;
				}
				skillmaterials[i].BtnLabel.text = num + "/" + array2[i];
				if (num >= array2[i])
				{
					skillmaterials[i].AddBtn.gameObject.SetActive(false);
					skillmaterials[i].BtnLabel.color = Color.white;
					skillmaterials[i].Button.interactable = true;
					skillmaterials[i].frmimg.color = Color.white;
					skillmaterials[i].bgimg.color = Color.white;
				}
				else
				{
					skillmaterials[i].AddBtn.gameObject.SetActive(true);
					skillmaterials[i].BtnLabel.color = Color.red;
					skillmaterials[i].Button.interactable = false;
					skillmaterials[i].frmimg.color = disablecolor;
					skillmaterials[i].bgimg.color = disablecolor;
					CurrentMaterialFlag = false;
					LevelupBtn.interactable = false;
					UnLockBtn.interactable = false;
				}
			}
			else
			{
				skillmaterials[i].Button.gameObject.SetActive(false);
			}
		}
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

	public void OnUpgradeBenchSlot()
	{
		if (ManagedSingleton<PlayerHelper>.Instance.GetLV() < tBACKUP_TABLE.n_PLAYER_RANK)
		{
			OnLevelLockTip();
		}
		else if (CurrentMaterialFlag)
		{
			ManagedSingleton<PlayerNetManager>.Instance.UpgradeBenchSlotReq(tBACKUP_TABLE.n_ID, delegate
			{
				CurrentSelectBackupCell.OnUpdateInfo();
				UpdateBackupPowerInfo();
				PlayUpgradeEffect(LevelUpEffectRoot.transform.position);
			});
		}
	}

	public void OnSetBenchWeapon()
	{
		if (CurrentSelectBenchInfo.WeaponID == tempEquipSelectCheckWeaponID)
		{
			OnClickEquipSelectClose();
			return;
		}
		if (tempEquipSelectCheckWeaponID == ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID || tempEquipSelectCheckWeaponID == ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
			{
				string p_msg = "BENCHWEAPON_SETUP_WEAPON_WIELDED";
				ui.Setup(p_msg);
			});
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.SetBenchWeaponReq(CurrentSelectSlot, tempEquipSelectCheckWeaponID, delegate
		{
			if (tempEquipSelectCheckWeaponID != 0)
			{
				PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK08);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_RemoveWepSE);
			}
			CloseEquipBackupWep();
			OnPlayerLoopEffect();
		});
	}

	public void OnClickInfoBtnCB(int nBtnID)
	{
	}

	public void UpdateExpBar()
	{
	}

	public void OnLevelLockTip()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), tBACKUP_TABLE.n_PLAYER_RANK.ToString());
			ui.Setup(p_msg);
		});
	}

	public void OnClickEquipSelect()
	{
		WeaponSelectBGClick.SetActive(true);
		EquipSelectRoot.SetActive(true);
		tempEquipSelectCheckWeaponID = tempWeaponID;
		tempEquipSelectFrameWeaponID = tempWeaponID;
		OnEquipBtn();
	}

	public void CloseEquipBackupWep()
	{
		WeaponSelectBGClick.SetActive(false);
		EquipSelectRoot.SetActive(false);
	}

	public void OnClickEquipSelectClose()
	{
		CloseEquipBackupWep();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
	}

	public void OnEquipBtn()
	{
		EquipSelectRoot.transform.localScale = new Vector3(0f, 0f, 1f);
		StartCoroutine(ObjScaleCoroutine(0f, 1f, 0.2f, EquipSelectRoot, delegate
		{
			tInfo0LVSR.SrollToCell(0, 2000f);
		}));
		EquipSelectRoot.SetActive(true);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		int num = 0;
		Dictionary<int, WeaponInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.GetEnumerator();
		while (enumerator.MoveNext())
		{
			WEAPON_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(enumerator.Current.Value.netInfo.WeaponID, out value) && value.n_TYPE > 0)
			{
				num++;
			}
		}
		int num2 = (num - num % 5) / 5;
		if (num % 5 > 0)
		{
			num2++;
		}
		if (tInfo0LVSR != null)
		{
			tInfo0LVSR.ClearCells();
		}
		tInfo0LVSR.totalCount = num2;
		tInfo0LVSR.RefillCells();
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

	public void OnSetFramePivot(bool bWeapon, Image BigOb, Image SmallOb)
	{
		if (bInitFrameImage)
		{
			if (bWeapon)
			{
				FrameSmallImage.color = clear;
				FrameBigImage.color = white;
				FrameBigImage.rectTransform.position = BigOb.rectTransform.position;
			}
			else
			{
				FrameSmallImage.color = white;
				FrameBigImage.color = clear;
				FrameSmallImage.rectTransform.position = SmallOb.rectTransform.position;
			}
		}
	}

	public void OnPlayerLoopEffect()
	{
		LeanTween.cancel(ref tweenID1);
		LeanTween.cancel(ref tweenID2);
		LeanTween.cancel(ref tweenID3);
		LeanTween.cancel(ref tweenID4);
		CurrentSelectBackupCell.OnPlayerLoopEffect(tempEquipSelectCheckWeaponID);
		EffectImage1.transform.localScale = new Vector3(2f, 2f);
		EffectImage1.color = new Color(1f, 1f, 1f, 1f);
		tweenID1 = LeanTween.scale(EffectImage1.gameObject, new Vector3(3f, 3f), 1f).setLoopClamp(1).uniqueId;
		tweenID2 = LeanTween.value(EffectImage1.gameObject, 1f, 0f, 1.2f).setOnUpdate(delegate(float alpha)
		{
			EffectImage1.color = new Color(1f, 1f, 1f, alpha);
		}).setLoopClamp(1)
			.uniqueId;
		EffectImage2.transform.localScale = new Vector3(2f, 2f);
		EffectImage2.color = new Color(1f, 1f, 1f, 1f);
		tweenID3 = LeanTween.scale(EffectImage2.gameObject, new Vector3(12f, 12f), 1f).setLoopClamp(1).uniqueId;
		tweenID4 = LeanTween.value(EffectImage2.gameObject, 1f, 0f, 1.2f).setOnUpdate(delegate(float alpha)
		{
			EffectImage2.color = new Color(1f, 1f, 1f, alpha);
		}).setLoopClamp(1)
			.uniqueId;
	}

	public void OnCloseWeaponInfoUI()
	{
		CurrentSelectBackupCell.OnUpdateInfo();
		UpdateBackupPowerInfo();
	}

	public void OnOpenWeaponInfoUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_toWeapon);
			ui.nTargetWeaponID = tempWeaponID;
			ui.bNeedInitList = true;
			ui.initalization_data();
			ui.OnInfoBtnCB(1);
			ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(OnCloseWeaponInfoUI));
		});
	}

	public void OnShowEffectRoot()
	{
		EffectRoot.SetActive(true);
		Invoke("OnCloseEffectRoot", 2f);
	}

	public void OnCloseEffectRoot()
	{
		EffectRoot.SetActive(false);
	}

	public void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_PLAYER_BOX, UpdateMaterialRequestData);
	}

	private void OnDisable()
	{
		LeanTween.cancel(ref tweenID1);
		LeanTween.cancel(ref tweenID2);
		LeanTween.cancel(ref tweenID3);
		LeanTween.cancel(ref tweenID4);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_PLAYER_BOX, UpdateMaterialRequestData);
	}

	private void PlayUpgradeEffect(Vector3 effectPos)
	{
		if (m_upgradeEffect == null)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "upgradeeffect", "UpgradeEffect", delegate(GameObject asset)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, base.transform);
				m_upgradeEffect = gameObject.GetComponent<UpgradeEffect>();
				m_upgradeEffect.transform.gameObject.SetActive(true);
				m_upgradeEffect.Play(effectPos);
			});
		}
		else
		{
			m_upgradeEffect.transform.gameObject.SetActive(true);
			m_upgradeEffect.Play(effectPos);
		}
		if (CurrentSelectSlotLevel <= 1)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_UnlockSE);
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_UpgradeEfxSE);
		}
	}

	public bool IsEffectPlaying()
	{
		bool flag = false;
		flag |= bEffectLock;
		if (m_upgradeEffect != null)
		{
			flag |= m_upgradeEffect.gameObject.activeSelf;
		}
		return flag;
	}

	private void UpdateMaterialRequestData()
	{
		UpdateMaterialItem();
	}
}
