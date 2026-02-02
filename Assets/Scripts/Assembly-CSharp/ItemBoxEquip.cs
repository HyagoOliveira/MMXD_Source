#define RELEASE
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using enums;

public class ItemBoxEquip : MonoBehaviour
{
	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickSynthesisSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickStrengthenSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickDismantleSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_openInfoSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_StrengthenChangeEauip;

	private bool isSetting;

	[SerializeField]
	private EquipIcon[] equipIcons;

	[SerializeField]
	private Transform equipSelection;

	private Dictionary<int, NetEquipmentInfo> dicNetEquipInfo;

	private ItemBoxUI itemBoxUI;

	private int[] order = new int[6] { 3, 0, 4, 5, 1, 2 };

	private int selectedIdx = -1;

	public void Awake()
	{
		isSetting = false;
		InitEquipData();
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_PLAYER_EQUIPMENT, SetupFromEvent);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_PLAYER_EQUIPMENT, SetupFromEvent);
	}

	private void SetupFromEvent()
	{
		isSetting = false;
		InitEquipData();
	}

	private NetEquipmentInfo GetSelectedEquipment()
	{
		NetEquipmentInfo value = null;
		if (dicNetEquipInfo == null)
		{
			dicNetEquipInfo = ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip();
		}
		if (dicNetEquipInfo != null)
		{
			if (selectedIdx == -1)
			{
				for (int i = 0; i < order.Length; i++)
				{
					if (dicNetEquipInfo.TryGetValue(order[i] + 1, out value))
					{
						selectedIdx = order[i];
						return value;
					}
				}
			}
			else if (dicNetEquipInfo.TryGetValue(selectedIdx + 1, out value))
			{
				return value;
			}
		}
		return null;
	}

	private void InitEquipData()
	{
		dicNetEquipInfo = ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip();
		itemBoxUI = GetComponentInParent<ItemBoxUI>();
		NetEquipmentInfo value = null;
		EQUIP_TABLE equip = null;
		int[] array = null;
		for (int i = 0; i < equipIcons.Length; i++)
		{
			if (dicNetEquipInfo.TryGetValue(i + 1, out value))
			{
				if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(value.EquipItemID, out equip))
				{
					EquipEnhanceInfo value2 = null;
					ManagedSingleton<PlayerNetManager>.Instance.dicEquipEnhance.TryGetValue((EquipPartType)equip.n_PARTS, out value2);
					array = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(value);
					if (value2 != null)
					{
						equipIcons[i].SetStarAndLv(array[3], equip.n_LV, value2.netPlayerEquipInfo.EnhanceLv);
					}
					else
					{
						equipIcons[i].SetStarAndLv(array[3], equip.n_LV);
					}
					equipIcons[i].SetRare(equip.n_RARE);
					equipIcons[i].Setup(i, AssetBundleScriptableObject.Instance.m_iconEquip, equip.s_ICON, OnClickEquip);
				}
				else
				{
					equipIcons[i].Clear();
				}
			}
			else
			{
				equipIcons[i].Clear();
			}
		}
		isSetting = true;
	}

	public void EnableSelectionFrame(bool bEnable)
	{
		if (bEnable)
		{
			if (selectedIdx < equipIcons.Length && selectedIdx >= 0)
			{
				equipSelection.gameObject.SetActive(true);
				equipSelection.localPosition = equipIcons[selectedIdx].transform.localPosition;
			}
		}
		else
		{
			equipSelection.gameObject.SetActive(false);
		}
	}

	private void OnClickEquip(int idx)
	{
		NetEquipmentInfo netEquipment = null;
		bool flag = idx != selectedIdx;
		if (!dicNetEquipInfo.TryGetValue(idx + 1, out netEquipment))
		{
			return;
		}
		Debug.Log("OnClick equip:" + netEquipment.EquipmentID + ", table ID:" + netEquipment.EquipItemID);
		selectedIdx = idx;
		ItemBoxEquipUpgrade2 uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ItemBoxEquipUpgrade2>("UI_ItemBoxEquipUpgrade2");
		if ((bool)uI)
		{
			uI.Setup(selectedIdx);
			EnableSelectionFrame(true);
			if (flag)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_StrengthenChangeEauip);
			}
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemBoxEquipInfo", delegate(ItemBoxEquipInfo ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_openInfoSE);
				ui.Setup(netEquipment);
			});
		}
	}

	public void OnClickBtnEquipUpperBody()
	{
		bool isSetting2 = isSetting;
	}

	public void OnClickBtnEquipLowerBody()
	{
		bool isSetting2 = isSetting;
	}

	public void OnClickBtnEquipShoes()
	{
		bool isSetting2 = isSetting;
	}

	public void OnClickBtnEquipHead()
	{
		bool isSetting2 = isSetting;
	}

	public void OnClickBtnEquipHand()
	{
		bool isSetting2 = isSetting;
	}

	public void OnClickBtnEquipPart()
	{
		bool isSetting2 = isSetting;
	}

	public void OnClickBtnSynthesis()
	{
		if (!ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog())
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickSynthesisSE);
			ItemBoxEquipUpgrade2 uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ItemBoxEquipUpgrade2>("UI_ItemBoxEquipUpgrade2");
			if (uI != null)
			{
				uI.OnClickCloseBtn();
			}
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemBoxEquipCompose", delegate(ItemBoxEquipCompose ui)
			{
				ui.Setup();
			});
		}
	}

	public void SetSelectedPart(int part)
	{
		selectedIdx = part;
	}

	public void OnClickBtnStrengthen()
	{
		if (MonoBehaviourSingleton<UIManager>.Instance.GetUI<ItemBoxEquipUpgrade2>("UI_ItemBoxEquipUpgrade2") != null || dicNetEquipInfo.Count <= 0)
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemBoxEquipUpgrade2", delegate(ItemBoxEquipUpgrade2 ui)
		{
			if (GetSelectedEquipment() != null)
			{
				ui.Setup(selectedIdx);
				EnableSelectionFrame(true);
			}
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickStrengthenSE);
	}

	public void OnClickBtnDismantle()
	{
		ItemBoxEquipUpgrade2 uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ItemBoxEquipUpgrade2>("UI_ItemBoxEquipUpgrade2");
		if (uI != null)
		{
			uI.OnClickCloseBtn();
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EquipmentDismantle", delegate(EquipmentDismantleUI ui)
		{
			ui.Setup();
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_clickDismantleSE);
	}
}
