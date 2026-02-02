using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BackupWeaponSelectColume : ScrollIndexCallback
{
	public class PerBackupWeaponSelectCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public CommonIconBase tWeaponIconBase;

		public GameObject refCheckObj;

		public GameObject refFrameObj;

		public GameObject refMarkImage;

		public Text refMarkText;

		public bool bLock;

		public BackupSystemUI tBackupSystemUI;

		public bool CheckWeaponID()
		{
			return nID == tBackupSystemUI.tempEquipSelectCheckWeaponID;
		}

		public bool CheckFrameWeaponID()
		{
			return nID == tBackupSystemUI.tempEquipSelectFrameWeaponID;
		}

		public void OnClick(int p_param)
		{
			if (nID != -1 && !bLock)
			{
				Transform transform = refRoot.transform;
				while (tBackupSystemUI == null)
				{
					transform = transform.parent;
					tBackupSystemUI = transform.GetComponent<BackupSystemUI>();
				}
				tBackupSystemUI.tempEquipSelectFrameWeaponID = nID;
				if (tBackupSystemUI.tempEquipSelectCheckWeaponID == nID)
				{
					tBackupSystemUI.tempEquipSelectCheckWeaponID = 0;
					refCheckObj.SetActive(false);
				}
				else
				{
					tBackupSystemUI.tempEquipSelectCheckWeaponID = nID;
					refCheckObj.SetActive(true);
				}
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			}
		}
	}

	public const int nCount = 5;

	[SerializeField]
	private Transform[] cellroot = new Transform[5];

	private Color32[] colors = new Color32[3]
	{
		new Color32(220, 132, 4, byte.MaxValue),
		new Color32(126, 204, 9, byte.MaxValue),
		new Color32(107, 194, 222, byte.MaxValue)
	};

	public PerBackupWeaponSelectCell[] allPerWeaponCells;

	public GameObject refPrefab;

	private int nNowIndex = -1;

	private void Update()
	{
		if (allPerWeaponCells == null)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (allPerWeaponCells[i] != null && allPerWeaponCells[i].tBackupSystemUI != null)
			{
				allPerWeaponCells[i].refFrameObj.SetActive(allPerWeaponCells[i].CheckFrameWeaponID());
				allPerWeaponCells[i].refCheckObj.SetActive(allPerWeaponCells[i].CheckWeaponID());
			}
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (allPerWeaponCells == null)
		{
			BackupSystemUI backupSystemUI = null;
			Transform parent = base.transform;
			while (backupSystemUI == null)
			{
				parent = parent.parent;
				backupSystemUI = parent.GetComponent<BackupSystemUI>();
			}
			allPerWeaponCells = new PerBackupWeaponSelectCell[5];
			for (int i = 0; i < 5; i++)
			{
				allPerWeaponCells[i] = new PerBackupWeaponSelectCell();
				allPerWeaponCells[i].tWeaponIconBase = cellroot[i].GetComponentInChildren<CommonIconBase>();
				if (allPerWeaponCells[i].tWeaponIconBase != null)
				{
					Object.Destroy(allPerWeaponCells[i].tWeaponIconBase.gameObject);
				}
				allPerWeaponCells[i].refRoot = Object.Instantiate(refPrefab, cellroot[i]);
				allPerWeaponCells[i].refRoot.transform.SetSiblingIndex(1);
				allPerWeaponCells[i].refCheckObj = cellroot[i].transform.Find("SelectedImg").gameObject;
				allPerWeaponCells[i].refFrameObj = cellroot[i].transform.Find("SelectedImg2").gameObject;
				allPerWeaponCells[i].tWeaponIconBase = allPerWeaponCells[i].refRoot.GetComponent<CommonIconBase>();
				allPerWeaponCells[i].refMarkImage = cellroot[i].transform.Find("MarkImage").gameObject;
				allPerWeaponCells[i].refMarkText = cellroot[i].transform.Find("MarkImage/Text").gameObject.GetComponent<Text>();
				allPerWeaponCells[i].tBackupSystemUI = backupSystemUI;
				allPerWeaponCells[i].refCheckObj.SetActive(false);
				allPerWeaponCells[i].refFrameObj.SetActive(false);
				allPerWeaponCells[i].refMarkImage.SetActive(false);
			}
		}
		nNowIndex = p_idx;
		KeyValuePair<int, WeaponInfo>[] array = (from obj in ManagedSingleton<PlayerNetManager>.Instance.dicWeapon
			where ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[obj.Value.netInfo.WeaponID].n_TYPE > 0
			orderby ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(obj.Value.netInfo.WeaponID).nATK descending
			select obj).ToArray();
		int j;
		for (j = 0; j < 5 && p_idx * 5 + j < array.Length; j++)
		{
			WeaponInfo value = array[p_idx * 5 + j].Value;
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[value.netInfo.WeaponID];
			allPerWeaponCells[j].refRoot.SetActive(true);
			allPerWeaponCells[j].refMarkImage.SetActive(false);
			allPerWeaponCells[j].nID = wEAPON_TABLE.n_ID;
			CommonIconBase tWeaponIconBase = allPerWeaponCells[j].tWeaponIconBase;
			tWeaponIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, allPerWeaponCells[j].OnClick);
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
			{
				allPerWeaponCells[j].refMarkImage.SetActive(false);
				allPerWeaponCells[j].bLock = false;
				if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID == wEAPON_TABLE.n_ID)
				{
					tWeaponIconBase.SetOtherInfo(value.netInfo, CommonIconBase.WeaponEquipType.Main);
					allPerWeaponCells[j].refMarkImage.SetActive(true);
					allPerWeaponCells[j].refMarkText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_IS_EQUIP");
					allPerWeaponCells[j].refMarkText.color = colors[0];
					allPerWeaponCells[j].bLock = true;
					continue;
				}
				if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.SubWeaponID == wEAPON_TABLE.n_ID)
				{
					tWeaponIconBase.SetOtherInfo(value.netInfo, CommonIconBase.WeaponEquipType.Sub);
					allPerWeaponCells[j].refMarkImage.SetActive(true);
					allPerWeaponCells[j].refMarkText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_IS_EQUIP");
					allPerWeaponCells[j].refMarkText.color = colors[0];
					allPerWeaponCells[j].bLock = true;
					continue;
				}
				List<NetBenchInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.Values.Select((BenchInfo x) => x.netBenchInfo).ToList();
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].WeaponID == wEAPON_TABLE.n_ID && allPerWeaponCells[j].tBackupSystemUI.CurrentSelectSlot != list[k].BenchSlot)
					{
						tWeaponIconBase.SetOtherInfo(value.netInfo, CommonIconBase.WeaponEquipType.UnEquip);
						allPerWeaponCells[j].refMarkImage.SetActive(true);
						allPerWeaponCells[j].refMarkText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("BACKUP_SETTING");
						allPerWeaponCells[j].refMarkText.color = colors[1];
						allPerWeaponCells[j].bLock = true;
					}
				}
				if (!allPerWeaponCells[j].bLock)
				{
					tWeaponIconBase.SetOtherInfo(value.netInfo, CommonIconBase.WeaponEquipType.UnEquip);
					allPerWeaponCells[j].refMarkImage.SetActive(false);
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
			allPerWeaponCells[j].refFrameObj.SetActive(false);
			allPerWeaponCells[j].refMarkImage.SetActive(false);
		}
	}
}
