#define RELEASE
using UnityEngine;

public class ChipCell : ScrollIndexCallback
{
	public GameObject refWeaponBase;

	public int nID = -1;

	private NetChipInfo TargetNetChipInfo;

	public GameObject refRoot;

	public CommonIconBase tChipIconBase;

	private GameObject tMark;

	private int nLastIndex = -1;

	public void OnClick(int p_param)
	{
		if (nID != -1)
		{
			ChipInfoUI chipInfoUI = null;
			Transform parent = refRoot.transform;
			while (parent != null && chipInfoUI == null)
			{
				parent = parent.parent;
				chipInfoUI = parent.GetComponent<ChipInfoUI>();
			}
			if (chipInfoUI != null && chipInfoUI.nTargetChipID != nID && !chipInfoUI.IsEffectPlaying())
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
				chipInfoUI.ChangeChip(nID);
			}
		}
	}

	public void UpdateData()
	{
		if (nLastIndex != -1)
		{
			ScrollCellIndex(nLastIndex);
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (refRoot == null)
		{
			CommonIconBase[] componentsInChildren = base.transform.GetComponentsInChildren<CommonIconBase>();
			if (componentsInChildren.Length != 0)
			{
				for (int num = componentsInChildren.Length - 1; num >= 0; num--)
				{
					Object.Destroy(componentsInChildren[num].transform.gameObject);
				}
			}
			refRoot = Object.Instantiate(refWeaponBase, base.gameObject.transform);
			tChipIconBase = refRoot.GetComponent<CommonIconBase>();
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
		nLastIndex = p_idx;
		ChipInfoUI chipInfoUI = null;
		Transform parent = base.transform;
		while (parent != null && chipInfoUI == null)
		{
			chipInfoUI = parent.GetComponent<ChipInfoUI>();
			parent = parent.parent;
		}
		if (chipInfoUI == null)
		{
			TargetNetChipInfo = null;
			refRoot.SetActive(false);
		}
		else if (p_idx >= 0)
		{
			NetChipInfo netChipInfo = null;
			if (p_idx >= chipInfoUI.listHasChips.Count)
			{
				netChipInfo = new NetChipInfo();
				netChipInfo.ChipID = chipInfoUI.listFragChips[p_idx - chipInfoUI.listHasChips.Count];
			}
			else
			{
				ChipInfo value;
				if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(chipInfoUI.listHasChips[p_idx], out value))
				{
					value = new ChipInfo();
					value.netChipInfo = new NetChipInfo();
					value.netChipInfo.ChipID = chipInfoUI.listHasChips[p_idx];
				}
				netChipInfo = value.netChipInfo;
			}
			DISC_TABLE value2;
			if (ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(netChipInfo.ChipID, out value2))
			{
				refRoot.SetActive(true);
				bool whiteColor = ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(netChipInfo.ChipID);
				tChipIconBase.Setup(0, AssetBundleScriptableObject.Instance.m_iconChip, value2.s_ICON, OnClick, whiteColor);
				nID = netChipInfo.ChipID;
				TargetNetChipInfo = netChipInfo;
				if (chipInfoUI.nTargetChipID == netChipInfo.ChipID)
				{
					tMark.SetActive(true);
				}
				else
				{
					tMark.SetActive(false);
				}
				tChipIconBase.SetOtherInfo(netChipInfo, true);
			}
			else
			{
				TargetNetChipInfo = null;
				refRoot.SetActive(false);
				Debug.LogError("Use Invalid ChipID :" + netChipInfo.ChipID);
			}
		}
		else
		{
			TargetNetChipInfo = null;
			refRoot.SetActive(false);
		}
	}
}
