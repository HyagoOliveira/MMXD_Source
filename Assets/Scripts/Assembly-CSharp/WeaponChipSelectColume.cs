using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponChipSelectColume : ScrollIndexCallback
{
	public class PerWeaponChipSelectCell
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
			WeaponInfoUI weaponInfoUI = null;
			Transform transform = refRoot.transform;
			while (transform != null && weaponInfoUI == null)
			{
				weaponInfoUI = transform.GetComponent<WeaponInfoUI>();
				transform = transform.parent;
			}
			if (weaponInfoUI.nTmpChipSet == nID)
			{
				weaponInfoUI.nTmpChipSet = 0;
				weaponInfoUI.refPerWeaponChipSelectCell.refCheckObj.SetActive(false);
				weaponInfoUI.refPerWeaponChipSelectCell.refCheckObj2.SetActive(false);
				weaponInfoUI.refPerWeaponChipSelectCell = null;
			}
			else
			{
				weaponInfoUI.nTmpChipSet = nID;
				if (weaponInfoUI.refPerWeaponChipSelectCell != null)
				{
					weaponInfoUI.refPerWeaponChipSelectCell.refCheckObj.SetActive(false);
					weaponInfoUI.refPerWeaponChipSelectCell.refCheckObj2.SetActive(false);
				}
				weaponInfoUI.refPerWeaponChipSelectCell = this;
				weaponInfoUI.refPerWeaponChipSelectCell.refCheckObj.SetActive(true);
				weaponInfoUI.refPerWeaponChipSelectCell.refCheckObj2.SetActive(true);
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
	}

	public const int nCount = 5;

	[SerializeField]
	private Transform[] cellroot = new Transform[5];

	public PerWeaponChipSelectCell[] allPerChipCells;

	public GameObject refPrefab;

	private int nNowIndex = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		if (allPerChipCells == null)
		{
			allPerChipCells = new PerWeaponChipSelectCell[5];
			for (int i = 0; i < 5; i++)
			{
				allPerChipCells[i] = new PerWeaponChipSelectCell();
				allPerChipCells[i].tWeaponIconBase = cellroot[i].GetComponentInChildren<CommonIconBase>();
				if (allPerChipCells[i].tWeaponIconBase != null)
				{
					Object.Destroy(allPerChipCells[i].tWeaponIconBase.gameObject);
				}
				allPerChipCells[i].refRoot = Object.Instantiate(refPrefab, cellroot[i]);
				allPerChipCells[i].refRoot.transform.SetSiblingIndex(1);
				allPerChipCells[i].refCheckObj = cellroot[i].transform.Find("SelectedImg").gameObject;
				allPerChipCells[i].refCheckObj2 = cellroot[i].transform.Find("SelectedImg2").gameObject;
				allPerChipCells[i].tWeaponIconBase = allPerChipCells[i].refRoot.GetComponent<CommonIconBase>();
			}
		}
		nNowIndex = p_idx;
		WeaponInfoUI weaponInfoUI = null;
		Transform parent = base.transform;
		while (parent != null && weaponInfoUI == null)
		{
			weaponInfoUI = parent.GetComponent<WeaponInfoUI>();
			parent = parent.parent;
		}
		WEAPON_TABLE tWEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponInfoUI.nTargetWeaponID];
		IEnumerable<KeyValuePair<int, ChipInfo>> source = ManagedSingleton<PlayerNetManager>.Instance.dicChip.Where((KeyValuePair<int, ChipInfo> obj) => (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[obj.Key].n_WEAPON_TYPE & tWEAPON_TABLE.n_TYPE) != 0);
		int j = 0;
		for (int num = source.Count(); j < 5 && p_idx * 5 + j < num; j++)
		{
			KeyValuePair<int, ChipInfo> keyValuePair = source.ElementAt(p_idx * 5 + j);
			DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[keyValuePair.Key];
			allPerChipCells[j].refRoot.SetActive(true);
			allPerChipCells[j].nID = keyValuePair.Key;
			CommonIconBase tWeaponIconBase = allPerChipCells[j].tWeaponIconBase;
			tWeaponIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconChip, dISC_TABLE.s_ICON, allPerChipCells[j].OnClick);
			tWeaponIconBase.SetOtherInfo(keyValuePair.Value.netChipInfo);
			if (weaponInfoUI.nTmpChipSet == keyValuePair.Key)
			{
				allPerChipCells[j].refCheckObj.SetActive(true);
				allPerChipCells[j].refCheckObj2.SetActive(true);
				weaponInfoUI.refPerWeaponChipSelectCell = allPerChipCells[j];
			}
			else
			{
				allPerChipCells[j].refCheckObj.SetActive(false);
				allPerChipCells[j].refCheckObj2.SetActive(false);
			}
		}
		for (; j < 5; j++)
		{
			allPerChipCells[j].refRoot.SetActive(false);
			allPerChipCells[j].nID = -1;
			allPerChipCells[j].refCheckObj.SetActive(false);
			allPerChipCells[j].refCheckObj2.SetActive(false);
		}
	}
}
