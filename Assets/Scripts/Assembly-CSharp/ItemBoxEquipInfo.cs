using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ItemBoxEquipInfo : OrangeUIBase
{
	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickEquipSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickChangeSE;

	[SerializeField]
	private ItemBoxEquipPanel m_selectionPanel;

	private ItemBoxEquipPanel m_equippedPanel;

	private Dictionary<int, NetEquipmentInfo> m_equippedInfoDict;

	private NetEquipmentInfo m_netEquipSelected;

	private EQUIP_TABLE m_equipTableSelected;

	private bool b_isClickChange;

	private void Start()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public override void OnClickCloseBtn()
	{
		ItemBoxUI componentInChildren = base.transform.parent.GetComponentInChildren<ItemBoxUI>();
		if ((bool)componentInChildren)
		{
			componentInChildren.EnableEquipSelectionFrame(false);
		}
		base.OnClickCloseBtn();
	}

	public void Setup(NetEquipmentInfo netEquip)
	{
		if ((bool)m_selectionPanel)
		{
			m_selectionPanel.Setup(netEquip);
		}
		m_equippedPanel = Object.Instantiate(m_selectionPanel.gameObject, m_selectionPanel.transform.parent).GetComponent<ItemBoxEquipPanel>();
		m_equippedPanel.gameObject.SetActive(false);
		m_netEquipSelected = netEquip;
		ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(m_netEquipSelected.EquipItemID, out m_equipTableSelected);
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		int oPENRANK_EQUIP_LVUP = OrangeConst.OPENRANK_EQUIP_LVUP;
		m_equippedInfoDict = ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip();
		if (m_netEquipSelected.Equip > 0)
		{
			m_selectionPanel.SetButtonFunction(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_EQUIP_POWERUP"), OnClickUpgradeBtn, lV >= OrangeConst.OPENRANK_EQUIP_LVUP);
		}
		else
		{
			foreach (KeyValuePair<int, NetEquipmentInfo> item in m_equippedInfoDict)
			{
				NetEquipmentInfo value = item.Value;
				EQUIP_TABLE equip = null;
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(value.EquipItemID, out equip) && equip.n_PARTS == m_equipTableSelected.n_PARTS)
				{
					m_equippedPanel.gameObject.SetActive(true);
					m_equippedPanel.Setup(value);
					m_selectionPanel.transform.localPosition = new Vector3(240f, 0f, 0f);
					m_equippedPanel.transform.localPosition = new Vector3(-400f, 0f, 0f);
					m_selectionPanel.SetButtonFunction(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_CHANGE"), OnClickChangeBtn);
					m_equippedPanel.SetButtonFunction(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_EQUIP_POWERUP"), OnClickUpgradeBtn, lV >= OrangeConst.OPENRANK_EQUIP_LVUP);
					m_selectionPanel.ShowDiff(value);
					return;
				}
			}
			m_selectionPanel.SetButtonFunction(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_EQUIP"), OnClickEquipBtn);
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
	}

	public void OnClickChangeBtn()
	{
		b_isClickChange = true;
		OnClickEquipBtn();
	}

	public void OnClickEquipBtn()
	{
		if (!base.IsLock)
		{
			ManagedSingleton<PlayerNetManager>.Instance.EquipEquipmentReq(m_netEquipSelected.EquipmentID, delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_PLAYER_EQUIPMENT);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_PLAYER_BOX);
			});
			if (b_isClickChange)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickChangeSE);
				b_isClickChange = false;
				base.CloseSE = SystemSE.NONE;
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickEquipSE);
				base.CloseSE = SystemSE.NONE;
			}
			OnClickCloseBtn();
		}
	}

	public void OnClickUpgradeBtn()
	{
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
		ItemBoxEquip componentInChildren = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ItemBoxUI>("UI_ItemBox").GetComponentInChildren<ItemBoxEquip>();
		componentInChildren.SetSelectedPart(m_equipTableSelected.n_PARTS - 1);
		componentInChildren.OnClickBtnStrengthen();
	}
}
