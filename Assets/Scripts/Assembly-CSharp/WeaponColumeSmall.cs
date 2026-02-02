#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class WeaponColumeSmall : ScrollIndexCallback
{
	public class PerWeaponSmallCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public CommonIconBase tWeaponIconBase;

		public NetData tNetWeaponInfo;

		public Image selimage;

		public void OnClick(int p_param)
		{
			if (nID != -1)
			{
				GoCheckUI goCheckUI = null;
				Transform transform = refRoot.transform;
				while (transform != null && goCheckUI == null)
				{
					goCheckUI = transform.GetComponent<GoCheckUI>();
					transform = transform.parent;
				}
				if (!(goCheckUI == null))
				{
					goCheckUI.SetSelectWeapon(nID);
				}
			}
		}
	}

	private const int nCellCount = 2;

	[SerializeField]
	private Transform[] cellroot = new Transform[2];

	public GameObject refWeaponIconBase;

	private int nLastIndex = -1;

	public PerWeaponSmallCell[] allPerPerWeaponSmallCells;

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, UpdateCheckMainSunWeapon);
	}

	public void UpdateCheckMainSunWeapon()
	{
		if (nLastIndex != -1)
		{
			ScrollCellIndex(nLastIndex);
		}
	}

	public override void BackToPool()
	{
		allPerPerWeaponSmallCells = null;
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, UpdateCheckMainSunWeapon);
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (allPerPerWeaponSmallCells == null)
		{
			allPerPerWeaponSmallCells = new PerWeaponSmallCell[2];
			for (int i = 0; i < 2; i++)
			{
				allPerPerWeaponSmallCells[i] = new PerWeaponSmallCell();
				if (cellroot == null)
				{
					continue;
				}
				CommonIconBase[] componentsInChildren = cellroot[i].transform.GetComponentsInChildren<CommonIconBase>();
				if (componentsInChildren.Length != 0)
				{
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						Object.Destroy(componentsInChildren[j].gameObject);
					}
				}
				CommonIconBase component = Object.Instantiate(refWeaponIconBase, cellroot[i]).GetComponent<CommonIconBase>();
				allPerPerWeaponSmallCells[i].refRoot = cellroot[i].gameObject;
				allPerPerWeaponSmallCells[i].tWeaponIconBase = component;
				allPerPerWeaponSmallCells[i].selimage = cellroot[i].transform.Find("Image").GetComponent<Image>();
				allPerPerWeaponSmallCells[i].selimage.gameObject.SetActive(false);
			}
			Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UI_UPDATEMAINSUBWEAPON, UpdateCheckMainSunWeapon);
		}
		GoCheckUI goCheckUI = null;
		Transform parent = base.transform;
		while (parent != null && goCheckUI == null)
		{
			goCheckUI = parent.GetComponent<GoCheckUI>();
			parent = parent.parent;
		}
		if (goCheckUI == null)
		{
			return;
		}
		nLastIndex = p_idx;
		int num = p_idx * 2;
		int num2 = num;
		if (goCheckUI.listHasWeapons.Count <= num2)
		{
			num2 = -1;
		}
		if (num2 < 0)
		{
			return;
		}
		num2 = 0;
		while (num2 < 2)
		{
			allPerPerWeaponSmallCells[num2].refRoot.SetActive(true);
			NetWeaponInfo netInfo = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[goCheckUI.listHasWeapons[num + num2]].netInfo;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.ContainsKey(netInfo.WeaponID))
			{
				WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netInfo.WeaponID];
				bool flag = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(netInfo.WeaponID);
				bool flag2 = goCheckUI.listUsedWeaponID.Contains(netInfo.WeaponID);
				allPerPerWeaponSmallCells[num2].tWeaponIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, allPerPerWeaponSmallCells[num2].OnClick, flag && !flag2);
				allPerPerWeaponSmallCells[num2].nID = netInfo.WeaponID;
				allPerPerWeaponSmallCells[num2].tNetWeaponInfo = netInfo;
				allPerPerWeaponSmallCells[num2].selimage.gameObject.SetActive(false);
				if (netInfo.WeaponID == goCheckUI.nMainWeaponID)
				{
					if (goCheckUI.NowSelectMode == 1)
					{
						allPerPerWeaponSmallCells[num2].selimage.gameObject.SetActive(true);
					}
					allPerPerWeaponSmallCells[num2].tWeaponIconBase.SetOtherInfo(netInfo, CommonIconBase.WeaponEquipType.Main, false, -1, true, flag2);
				}
				else if (netInfo.WeaponID == goCheckUI.nSubWeaponID)
				{
					if (goCheckUI.NowSelectMode == 2)
					{
						allPerPerWeaponSmallCells[num2].selimage.gameObject.SetActive(true);
					}
					allPerPerWeaponSmallCells[num2].tWeaponIconBase.SetOtherInfo(netInfo, CommonIconBase.WeaponEquipType.Sub, false, -1, true, flag2);
				}
				else
				{
					allPerPerWeaponSmallCells[num2].tWeaponIconBase.SetOtherInfo(netInfo, CommonIconBase.WeaponEquipType.UnEquip, false, -1, true, flag2);
				}
				goCheckUI.BonusSub.SetCommonIcon(allPerPerWeaponSmallCells[num2].tWeaponIconBase, netInfo.WeaponID);
			}
			else
			{
				Debug.LogError("Use Invalid WeaponID :" + netInfo.WeaponID);
			}
			num2++;
			if (num2 + num >= goCheckUI.listHasWeapons.Count)
			{
				break;
			}
		}
		for (; num2 < 2; num2++)
		{
			allPerPerWeaponSmallCells[num2].refRoot.SetActive(false);
		}
	}
}
