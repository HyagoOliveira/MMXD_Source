using System.Collections.Generic;
using UnityEngine;

public class WeaponColume : ScrollIndexCallback
{
	public class PerWeaponCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public CommonIconBase tWeaponIconBase;

		public void OnClick(int p_param)
		{
			if (nID == -1)
			{
				return;
			}
			WeaponMainUI tWeaponMainUI = refRoot.transform.GetComponentInParent<WeaponMainUI>();
			if (tWeaponMainUI != null && tWeaponMainUI.IsSettingFavorite())
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(nID))
				{
					ManagedSingleton<PlayerNetManager>.Instance.WeaponFavoriteChange(nID, delegate
					{
						tWeaponMainUI.ReFresh();
					});
				}
				else
				{
					MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NOT_HAVE"), 42);
				}
				return;
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WEAPONINFO", delegate(WeaponInfoUI ui)
			{
				if (tWeaponMainUI != null)
				{
					ui.listHasWeapons = tWeaponMainUI.listHasWeapons;
					ui.listFragWeapons = tWeaponMainUI.listFragWeapons;
				}
				ui.nTargetWeaponID = nID;
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		}
	}

	public const int nCount = 6;

	[SerializeField]
	private Transform[] cellroot = new Transform[6];

	public PerWeaponCell[] allPerWeaponCells;

	public GameObject refPrefab;

	private int nNowIndex = -1;

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, UpdateCheckMainSunWeapon);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, UpdateCheckMainSunWeapon);
	}

	public void UpdateCheckMainSunWeapon()
	{
		if (nNowIndex != -1)
		{
			ScrollCellIndex(nNowIndex);
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (allPerWeaponCells == null)
		{
			allPerWeaponCells = new PerWeaponCell[6];
			for (int i = 0; i < 6; i++)
			{
				allPerWeaponCells[i] = new PerWeaponCell();
				if (cellroot[i].transform.childCount > 0)
				{
					for (int num = cellroot[i].transform.childCount - 1; num >= 0; num--)
					{
						Object.Destroy(cellroot[i].transform.GetChild(num).gameObject);
					}
				}
				allPerWeaponCells[i].refRoot = Object.Instantiate(refPrefab, cellroot[i]);
				allPerWeaponCells[i].tWeaponIconBase = allPerWeaponCells[i].refRoot.GetComponent<CommonIconBase>();
			}
		}
		WeaponMainUI componentInParent = base.transform.GetComponentInParent<WeaponMainUI>();
		if (componentInParent == null)
		{
			return;
		}
		nNowIndex = p_idx;
		bool bFavoriteSet = componentInParent.IsSettingFavorite();
		int j = 0;
		bool flag = false;
		for (int num2 = componentInParent.listHasWeapons.Count + componentInParent.listFragWeapons.Count; j < 6 && p_idx * 6 + j < num2; j++)
		{
			WeaponInfo weaponInfo = ManagedSingleton<EquipHelper>.Instance.GetSortedWeaponInfo(p_idx * 6 + j);
			if (weaponInfo == null)
			{
				weaponInfo = new WeaponInfo();
				weaponInfo.netInfo = new NetWeaponInfo();
				weaponInfo.netExpertInfos = new List<NetWeaponExpertInfo>();
				weaponInfo.netSkillInfos = new List<NetWeaponSkillInfo>();
				weaponInfo.netInfo.WeaponID = componentInParent.listHasWeapons[p_idx * 6 + j];
			}
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponInfo.netInfo.WeaponID];
			flag = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(weaponInfo.netInfo.WeaponID);
			allPerWeaponCells[j].refRoot.SetActive(true);
			allPerWeaponCells[j].nID = wEAPON_TABLE.n_ID;
			CommonIconBase tWeaponIconBase = allPerWeaponCells[j].tWeaponIconBase;
			tWeaponIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, allPerWeaponCells[j].OnClick, flag);
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID == wEAPON_TABLE.n_ID)
				{
					tWeaponIconBase.SetOtherInfo(weaponInfo.netInfo, CommonIconBase.WeaponEquipType.Main, false, -1, true, false, true, bFavoriteSet);
				}
				else if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID == wEAPON_TABLE.n_ID)
				{
					tWeaponIconBase.SetOtherInfo(weaponInfo.netInfo, CommonIconBase.WeaponEquipType.Sub, false, -1, true, false, true, bFavoriteSet);
				}
				else
				{
					tWeaponIconBase.SetOtherInfo(weaponInfo.netInfo, CommonIconBase.WeaponEquipType.UnEquip, false, -1, true, false, true, bFavoriteSet);
				}
			}
			else
			{
				tWeaponIconBase.SetOtherInfo(weaponInfo.netInfo, CommonIconBase.WeaponEquipType.UnEquip);
			}
		}
		for (; j < 6; j++)
		{
			allPerWeaponCells[j].refRoot.SetActive(false);
		}
	}
}
