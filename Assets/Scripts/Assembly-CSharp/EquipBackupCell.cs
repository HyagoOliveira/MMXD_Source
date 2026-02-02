using System.Collections.Generic;
using System.Linq;
using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class EquipBackupCell : ScrollIndexCallback
{
	private BackupSystemUI parentBackupSystemUI;

	[SerializeField]
	private GameObject[] BackupLevelIcon;

	[SerializeField]
	private GameObject EquipIconRoot;

	[SerializeField]
	private Text EquipNameText;

	[SerializeField]
	private Image BGBigImage;

	[SerializeField]
	private Image BGSmallImage;

	[SerializeField]
	private Image FrameBigImage;

	[SerializeField]
	private Image FrameSmallImage;

	[SerializeField]
	private Image UnlockImage;

	[SerializeField]
	private Image LockImage;

	[SerializeField]
	private GameObject SlotInfoRoot;

	[SerializeField]
	private GameObject NoWeaponRoot;

	[SerializeField]
	private CanvasGroup NoWeaponRootCanvasGroup;

	[SerializeField]
	private CanvasGroup BattlePowerRootCanvasGroup;

	[SerializeField]
	private Text NeedLevelText;

	[SerializeField]
	private Text PowerText;

	[SerializeField]
	private Image EffectImage;

	[SerializeField]
	private Image EffectImage2;

	[SerializeField]
	private GameObject LevelInfoRoot;

	[SerializeField]
	private GameObject Pivot1;

	[SerializeField]
	private GameObject Pivot2;

	private int idx;

	private int slot;

	private NetBenchInfo netBenchInfo;

	private BACKUP_TABLE tBACKUP_TABLE;

	private bool bWeapon;

	private int tweenID1;

	private int tweenID2;

	private Color32[] colors = new Color32[3]
	{
		new Color32(247, 123, 123, byte.MaxValue),
		new Color32(92, byte.MaxValue, 222, byte.MaxValue),
		new Color32(107, 194, 222, byte.MaxValue)
	};

	private void Start()
	{
	}

	private void Update()
	{
		if (null != parentBackupSystemUI && parentBackupSystemUI.GetCurrentSelectSlot() == slot)
		{
			parentBackupSystemUI.OnSetFramePivot(bWeapon, FrameBigImage, FrameSmallImage);
		}
	}

	private int GetWeaponPower(int wid)
	{
		return ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(wid).nBattlePower;
	}

	public override void ScrollCellIndex(int p_idx)
	{
		idx = p_idx;
		slot = idx + 1;
		parentBackupSystemUI = GetComponentInParent<BackupSystemUI>();
		if (idx == 0 && parentBackupSystemUI.CurrentSelectBackupCell == null)
		{
			parentBackupSystemUI.CurrentSelectBackupCell = this;
		}
		EffectImage.color = new Color(1f, 1f, 1f, 0f);
		EffectImage2.color = new Color(1f, 1f, 1f, 0f);
		OnUpdateInfo();
	}

	public void OnUpdateInfo()
	{
		int lv = 0;
		bWeapon = false;
		BGBigImage.gameObject.SetActive(false);
		BGSmallImage.gameObject.SetActive(true);
		if (ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.ContainsKey(slot))
		{
			netBenchInfo = ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo[slot].netBenchInfo;
			lv = netBenchInfo.Level;
			for (int i = 0; i < BackupLevelIcon.Length; i++)
			{
				BackupLevelIcon[i].SetActive(i < netBenchInfo.Level);
			}
		}
		List<BACKUP_TABLE> list = (from p in ManagedSingleton<OrangeDataManager>.Instance.BACKUP_TABLE_DICT
			where p.Value.n_SLOT == slot && p.Value.n_SLOT_LV == lv
			select p into o
			select o.Value).ToList();
		if (list.Count > 0)
		{
			tBACKUP_TABLE = list[0];
		}
		bool num = netBenchInfo != null;
		PowerText.text = "0";
		if (!num)
		{
			SlotInfoRoot.SetActive(false);
			NoWeaponRoot.SetActive(true);
			LevelInfoRoot.SetActive(false);
			UnlockImage.gameObject.SetActive(false);
			LockImage.gameObject.SetActive(true);
			NeedLevelText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), tBACKUP_TABLE.n_PLAYER_RANK.ToString());
			if (ManagedSingleton<PlayerHelper>.Instance.GetLV() >= tBACKUP_TABLE.n_PLAYER_RANK)
			{
				NeedLevelText.color = colors[1];
			}
			else
			{
				NeedLevelText.color = colors[0];
			}
			NeedLevelText.gameObject.SetActive(true);
			return;
		}
		NeedLevelText.gameObject.SetActive(false);
		LevelInfoRoot.SetActive(true);
		UnlockImage.gameObject.SetActive(true);
		LockImage.gameObject.SetActive(false);
		int childCount = EquipIconRoot.transform.childCount;
		for (int j = 0; j < childCount; j++)
		{
			Object.Destroy(EquipIconRoot.transform.GetChild(j).gameObject);
		}
		if (netBenchInfo.WeaponID > 0)
		{
			bWeapon = true;
			BGBigImage.gameObject.SetActive(true);
			BGSmallImage.gameObject.SetActive(false);
			SlotInfoRoot.SetActive(true);
			NoWeaponRoot.SetActive(false);
			LevelInfoRoot.transform.localPosition = Pivot1.transform.localPosition;
			int tempWeaponID = netBenchInfo.WeaponID;
			WEAPON_TABLE tWeapon_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[tempWeaponID];
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
			{
				CommonIconBase component = Object.Instantiate(asset, EquipIconRoot.transform).GetComponent<CommonIconBase>();
				component.SetPlayerWeaponInfo(ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[tempWeaponID], ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[tempWeaponID].netInfo, CommonIconBase.WeaponEquipType.UnEquip);
				component.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, tWeapon_TABLE.s_ICON);
				component.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			});
			EquipNameText.text = parentBackupSystemUI.GetWeaponName(tempWeaponID);
			PowerText.text = ManagedSingleton<PlayerHelper>.Instance.GetBackupWeaponSlotPower(slot).ToString();
		}
		else
		{
			SlotInfoRoot.SetActive(false);
			NoWeaponRoot.SetActive(true);
			LevelInfoRoot.transform.localPosition = Pivot2.transform.localPosition;
		}
	}

	public void OnClickBtn()
	{
		if (null != parentBackupSystemUI)
		{
			int lv = 0;
			if (netBenchInfo != null)
			{
				lv = netBenchInfo.Level;
			}
			parentBackupSystemUI.SetCurrentSelectSlot(slot, lv, netBenchInfo, tBACKUP_TABLE, this);
		}
	}

	public void OnPlayerLoopEffect(int wid)
	{
		LeanTween.cancel(ref tweenID1);
		LeanTween.cancel(ref tweenID2);
		if (netBenchInfo.WeaponID > 0)
		{
			if (wid > 0)
			{
				OnPlayerLoopEffect3();
			}
			else
			{
				OnPlayerLoopEffect2();
			}
		}
		else if (wid > 0)
		{
			OnPlayerLoopEffect1();
		}
	}

	public void OnPlayerLoopEffect1()
	{
		EffectImage2.color = new Color(1f, 1f, 1f, 0f);
		BattlePowerRootCanvasGroup.alpha = 0f;
		LevelInfoRoot.SetActive(false);
		Vector3 Pos = NoWeaponRootCanvasGroup.transform.localPosition;
		StartCoroutine(StageResManager.TweenFloatCoroutine(Pos.x, Pos.x - 210f, 0.2f, delegate(float f)
		{
			Pos.x = f;
			NoWeaponRootCanvasGroup.transform.localPosition = Pos;
		}, delegate
		{
			EffectImage.color = new Color(1f, 1f, 1f, 1f);
			tweenID1 = LeanTween.value(EffectImage.gameObject, 1f, 0f, 1f).setOnUpdate(delegate(float alpha)
			{
				EffectImage.color = new Color(1f, 1f, 1f, alpha);
			}).setLoopClamp(1)
				.uniqueId;
			if (null != parentBackupSystemUI)
			{
				parentBackupSystemUI.OnShowEffectRoot();
			}
			EffectImage.transform.localScale = new Vector3(1f, 1f, 1f);
			tweenID2 = LeanTween.value(EffectImage2.gameObject, 1f, 1.28f, 0.2f).setOnUpdate(delegate(float scale)
			{
				EffectImage.transform.localScale = new Vector3(scale, 1f, 1f);
			}).setLoopClamp(1)
				.uniqueId;
			Vector3 Pos2 = BGBigImage.transform.localPosition;
			StartCoroutine(StageResManager.TweenFloatCoroutine(Pos2.x - 210f, Pos2.x, 0.2f, delegate(float f)
			{
				Pos2.x = f;
				BGBigImage.transform.localPosition = Pos2;
			}, delegate
			{
				OnUpdateInfo();
				if (null != parentBackupSystemUI)
				{
					parentBackupSystemUI.UpdateBackupPowerInfo();
				}
				StartCoroutine(StageResManager.TweenFloatCoroutine(BattlePowerRootCanvasGroup.alpha, 1f, 1f, delegate(float f)
				{
					BattlePowerRootCanvasGroup.alpha = f;
				}, null));
				Pos.x += 210f;
				NoWeaponRootCanvasGroup.transform.localPosition = Pos;
				LevelInfoRoot.SetActive(true);
			}));
		}));
	}

	public void OnPlayerLoopEffect2()
	{
		EffectImage.color = new Color(1f, 1f, 1f, 0f);
		EffectImage2.color = new Color(1f, 1f, 1f, 1f);
		EffectImage2.transform.localScale = new Vector3(1f, 1f, 1f);
		tweenID1 = LeanTween.value(EffectImage2.gameObject, 0.8f, 0f, 0.3f).setOnUpdate(delegate(float alpha)
		{
			EffectImage2.color = new Color(1f, 1f, 1f, alpha);
		}).setLoopClamp(1)
			.setEase(LeanTweenType.easeInQuint)
			.uniqueId;
		tweenID2 = LeanTween.value(EffectImage2.gameObject, 1f, 0.78f, 0.3f).setOnUpdate(delegate(float scale)
		{
			EffectImage2.transform.localScale = new Vector3(scale, 1f, 1f);
		}).setLoopClamp(1)
			.uniqueId;
		if (null != parentBackupSystemUI)
		{
			parentBackupSystemUI.OnShowEffectRoot();
		}
		OnUpdateInfo();
		if (null != parentBackupSystemUI)
		{
			parentBackupSystemUI.UpdateBackupPowerInfo();
		}
	}

	public void OnPlayerLoopEffect3()
	{
		EffectImage.color = new Color(1f, 1f, 1f, 0f);
		EffectImage2.color = new Color(1f, 1f, 1f, 1f);
		EffectImage2.transform.localScale = new Vector3(1f, 1f, 1f);
		LeanTween.value(EffectImage2.gameObject, 1f, 0f, 0.5f).setOnUpdate(delegate(float alpha)
		{
			EffectImage2.color = new Color(1f, 1f, 1f, alpha);
		}).setLoopClamp(1);
		if (null != parentBackupSystemUI)
		{
			parentBackupSystemUI.OnShowEffectRoot();
		}
		OnUpdateInfo();
		if (null != parentBackupSystemUI)
		{
			parentBackupSystemUI.UpdateBackupPowerInfo();
		}
	}

	private void OnDisable()
	{
		LeanTween.cancel(ref tweenID1);
		LeanTween.cancel(ref tweenID2);
	}
}
