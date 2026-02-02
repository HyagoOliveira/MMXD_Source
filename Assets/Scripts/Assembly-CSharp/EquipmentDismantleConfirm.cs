using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDismantleConfirm : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect m_scrollRect;

	[SerializeField]
	private EquipmentDismantleConfirmIcon m_equipmentIcon;

	private List<NetEquipmentInfo> m_listNetEquipmentInfo = new List<NetEquipmentInfo>();

	private Callback tCallback;

	public void Setup(List<NetEquipmentInfo> _list, Callback p_cb = null)
	{
		m_listNetEquipmentInfo = _list;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		tCallback = p_cb;
		m_scrollRect.OrangeInit(m_equipmentIcon, m_listNetEquipmentInfo.Count, m_listNetEquipmentInfo.Count);
	}

	public void OnClickConfirmBtn()
	{
		base.CloseSE = SystemSE.NONE;
		tCallback();
		OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
		if (m_scrollRect.prefabSource.pbo != null)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(m_scrollRect.prefabSource.pbo.itemName);
		}
	}

	public void SetEquipIcon(EquipmentDismantleConfirmIcon p_unit)
	{
		NetEquipmentInfo netEquipmentInfo = m_listNetEquipmentInfo[p_unit.NowIdx];
		EQUIP_TABLE equip = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(netEquipmentInfo.EquipItemID, out equip))
		{
			int[] equipRank = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(netEquipmentInfo);
			p_unit.EquipIcon.SetStarAndLv(equipRank[3], equip.n_LV);
			p_unit.EquipIcon.SetRare(equip.n_RARE);
			p_unit.EquipIcon.Setup(p_unit.NowIdx, AssetBundleScriptableObject.Instance.m_iconEquip, equip.s_ICON);
			p_unit.SetEquipmentID(netEquipmentInfo.EquipmentID);
		}
		else
		{
			p_unit.EquipIcon.Clear();
		}
	}
}
