#define RELEASE
using System.Collections.Generic;
using UnityEngine;

public class WeaponCell : ScrollIndexCallback
{
	public GameObject refWeaponBase;

	public int nID = -1;

	private NetWeaponInfo TargetNetWeaponInfo;

	public GameObject refRoot;

	public CommonIconBase tWeaponIconBase;

	private GameObject tMark;

	private int nLastIndex = -1;

	private WeaponInfoUI tWeaponInfoUI;

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
		ScrollCellIndex(nLastIndex);
	}

	public void OnClick(int p_param)
	{
		if (nID != -1)
		{
			WeaponInfoUI weaponInfoUI = null;
			Transform parent = base.transform;
			while ((bool)parent && weaponInfoUI == null)
			{
				weaponInfoUI = parent.GetComponent<WeaponInfoUI>();
				parent = parent.parent;
			}
			if (weaponInfoUI != null)
			{
				weaponInfoUI.ChangeWeapon(nID);
			}
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (refRoot == null)
		{
			tWeaponIconBase = base.gameObject.transform.GetComponentInChildren<CommonIconBase>();
			if (tWeaponIconBase != null)
			{
				Object.Destroy(tWeaponIconBase.gameObject);
			}
			refRoot = Object.Instantiate(refWeaponBase, base.gameObject.transform);
			tWeaponIconBase = refRoot.GetComponent<CommonIconBase>();
			tMark = base.transform.Find("Image").gameObject;
			tMark.SetActive(false);
		}
		if (p_idx == -1)
		{
			return;
		}
		if (tMark == null)
		{
			tMark = base.transform.Find("Image").gameObject;
		}
		tWeaponInfoUI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<WeaponInfoUI>("UI_WEAPONINFO");
		nLastIndex = p_idx;
		if (tWeaponInfoUI == null)
		{
			TargetNetWeaponInfo = null;
			refRoot.SetActive(false);
			return;
		}
		int num = tWeaponInfoUI.listFragWeapons.Count + tWeaponInfoUI.listHasWeapons.Count;
		if (nLastIndex >= 0 && nLastIndex < num)
		{
			WeaponInfo weaponInfo = ManagedSingleton<EquipHelper>.Instance.GetSortedWeaponInfo(nLastIndex);
			if (weaponInfo == null)
			{
				weaponInfo = new WeaponInfo();
				weaponInfo.netInfo = new NetWeaponInfo();
				weaponInfo.netExpertInfos = new List<NetWeaponExpertInfo>();
				weaponInfo.netSkillInfos = new List<NetWeaponSkillInfo>();
				weaponInfo.netInfo.WeaponID = tWeaponInfoUI.listHasWeapons[nLastIndex];
			}
			NetWeaponInfo netInfo = weaponInfo.netInfo;
			if (weaponInfo == null)
			{
				TargetNetWeaponInfo = null;
				refRoot.SetActive(false);
			}
			else if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(netInfo.WeaponID))
			{
				refRoot.SetActive(true);
				WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netInfo.WeaponID];
				bool whiteColor = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(netInfo.WeaponID);
				tWeaponIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, OnClick, whiteColor);
				nID = netInfo.WeaponID;
				TargetNetWeaponInfo = netInfo;
				if (tWeaponInfoUI.nTargetWeaponID == netInfo.WeaponID)
				{
					tMark.SetActive(true);
				}
				else
				{
					tMark.SetActive(false);
				}
				if (netInfo.WeaponID == ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID)
				{
					tWeaponIconBase.SetOtherInfo(netInfo, CommonIconBase.WeaponEquipType.Main, false, -1, true, false, true);
				}
				else if (netInfo.WeaponID == ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID)
				{
					tWeaponIconBase.SetOtherInfo(netInfo, CommonIconBase.WeaponEquipType.Sub, false, -1, true, false, true);
				}
				else
				{
					tWeaponIconBase.SetOtherInfo(netInfo, CommonIconBase.WeaponEquipType.UnEquip, false, -1, true, false, true);
				}
			}
			else
			{
				TargetNetWeaponInfo = null;
				refRoot.SetActive(false);
				Debug.LogError("Use Invalid WeaponID :" + netInfo.WeaponID);
			}
		}
		else
		{
			TargetNetWeaponInfo = null;
			refRoot.SetActive(false);
		}
	}
}
