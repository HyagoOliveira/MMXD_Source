using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChipWeaponSelectColume : ScrollIndexCallback
{
	public class PerChipWeaponSelectCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public CommonIconBase tWeaponIconBase;

		public GameObject refCheckObj;

		public GameObject refCheckObj2;

		public void OnClick(int p_param)
		{
			if (nID == -1)
			{
				return;
			}
			ChipInfoUI chipInfoUI = null;
			Transform transform = refRoot.transform;
			while (chipInfoUI == null)
			{
				transform = transform.parent;
				chipInfoUI = transform.GetComponent<ChipInfoUI>();
			}
			if (!chipInfoUI.bLockInfo0)
			{
				if (chipInfoUI.listWeaponChipEquiped.Contains(nID))
				{
					chipInfoUI.listWeaponChipEquiped.Remove(nID);
					refCheckObj.SetActive(false);
					refCheckObj2.SetActive(false);
				}
				else
				{
					chipInfoUI.listWeaponChipEquiped.Add(nID);
					refCheckObj.SetActive(true);
					refCheckObj2.SetActive(true);
				}
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			}
		}
	}

	public const int nCount = 5;

	[SerializeField]
	private Transform[] cellroot = new Transform[5];

	public PerChipWeaponSelectCell[] allPerWeaponCells;

	public GameObject refPrefab;

	private int nNowIndex = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		if (allPerWeaponCells == null)
		{
			allPerWeaponCells = new PerChipWeaponSelectCell[5];
			for (int i = 0; i < 5; i++)
			{
				allPerWeaponCells[i] = new PerChipWeaponSelectCell();
				allPerWeaponCells[i].tWeaponIconBase = cellroot[i].GetComponentInChildren<CommonIconBase>();
				if (allPerWeaponCells[i].tWeaponIconBase != null)
				{
					Object.Destroy(allPerWeaponCells[i].tWeaponIconBase.gameObject);
				}
				allPerWeaponCells[i].refRoot = Object.Instantiate(refPrefab, cellroot[i]);
				allPerWeaponCells[i].refRoot.transform.SetSiblingIndex(1);
				allPerWeaponCells[i].refCheckObj = cellroot[i].transform.Find("SelectedImg").gameObject;
				allPerWeaponCells[i].refCheckObj2 = cellroot[i].transform.Find("SelectedImg2").gameObject;
				allPerWeaponCells[i].tWeaponIconBase = allPerWeaponCells[i].refRoot.GetComponent<CommonIconBase>();
			}
		}
		nNowIndex = p_idx;
		ChipInfoUI chipInfoUI = null;
		Transform parent = base.transform;
		while (chipInfoUI == null)
		{
			parent = parent.parent;
			chipInfoUI = parent.GetComponent<ChipInfoUI>();
		}
		DISC_TABLE tDISC_TABLE = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(chipInfoUI.nTargetChipID, out tDISC_TABLE))
		{
			return;
		}
		KeyValuePair<int, WeaponInfo>[] array = (from obj in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon
			where (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[obj.Value.netInfo.WeaponID].n_TYPE & tDISC_TABLE.n_WEAPON_TYPE) > 0
			orderby ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(obj.Value.netInfo.WeaponID).nATK descending
			select obj).ToArray();
		int j;
		for (j = 0; j < 5 && p_idx * 5 + j < array.Length; j++)
		{
			WeaponInfo value = array[p_idx * 5 + j].Value;
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[value.netInfo.WeaponID];
			allPerWeaponCells[j].refRoot.SetActive(true);
			allPerWeaponCells[j].nID = wEAPON_TABLE.n_ID;
			CommonIconBase tWeaponIconBase = allPerWeaponCells[j].tWeaponIconBase;
			tWeaponIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, allPerWeaponCells[j].OnClick);
			if (chipInfoUI.listWeaponChipEquiped.Contains(wEAPON_TABLE.n_ID))
			{
				allPerWeaponCells[j].refCheckObj.SetActive(true);
				allPerWeaponCells[j].refCheckObj2.SetActive(true);
			}
			else if (value.netInfo.Chip == tDISC_TABLE.n_ID)
			{
				allPerWeaponCells[j].refCheckObj.SetActive(true);
				allPerWeaponCells[j].refCheckObj2.SetActive(true);
				chipInfoUI.listWeaponChipEquiped.Add(wEAPON_TABLE.n_ID);
			}
			else
			{
				allPerWeaponCells[j].refCheckObj.SetActive(false);
				allPerWeaponCells[j].refCheckObj2.SetActive(false);
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID == wEAPON_TABLE.n_ID)
				{
					tWeaponIconBase.SetOtherInfo(value.netInfo, CommonIconBase.WeaponEquipType.Main);
				}
				else if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID == wEAPON_TABLE.n_ID)
				{
					tWeaponIconBase.SetOtherInfo(value.netInfo, CommonIconBase.WeaponEquipType.Sub);
				}
				else
				{
					tWeaponIconBase.SetOtherInfo(value.netInfo, CommonIconBase.WeaponEquipType.UnEquip);
				}
			}
			else
			{
				tWeaponIconBase.SetOtherInfo(value.netInfo, CommonIconBase.WeaponEquipType.UnEquip);
			}
		}
		for (; j < 5; j++)
		{
			allPerWeaponCells[j].refRoot.SetActive(false);
			allPerWeaponCells[j].nID = -1;
			allPerWeaponCells[j].refCheckObj.SetActive(false);
			allPerWeaponCells[j].refCheckObj2.SetActive(false);
		}
	}
}
