using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDismantleUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect m_scrollRect;

	[SerializeField]
	private EquipmentDismantleIcon m_equipmentIcon;

	[SerializeField]
	private Text m_normalMaterialCount;

	[SerializeField]
	private Text m_superiorMaterialCount;

	[SerializeField]
	private Text m_moneyCount;

	[SerializeField]
	private LoopVerticalScrollRect m_levelToggleScrollRect;

	[SerializeField]
	private EquipmentDismantleToggle m_levelToggleRef;

	[SerializeField]
	private Button m_confirmBtn;

	private List<NetEquipmentInfo> m_listNetEquipmentInfo = new List<NetEquipmentInfo>();

	private List<NetEquipmentInfo> m_listNetEquipmentInfoFiltered = new List<NetEquipmentInfo>();

	private List<int> m_sortedLevelList = new List<int>();

	private Dictionary<int, NetEquipmentInfo> m_dictSelectedEquipmentInfo = new Dictionary<int, NetEquipmentInfo>();

	private Dictionary<int, bool> m_dictLevelToggleStatus = new Dictionary<int, bool>();

	private Dictionary<int, EquipmentDismantleToggle> m_dictLevelToggle = new Dictionary<int, EquipmentDismantleToggle>();

	private int m_maxComposableLevel;

	public void Setup(bool bForceSelectAll = false)
	{
		m_listNetEquipmentInfo = ManagedSingleton<EquipHelper>.Instance.GetListNetEquipmentExceptEquipped();
		m_listNetEquipmentInfo.Reverse();
		m_maxComposableLevel = GetMaxComposableLevel();
		CollectEquipmentLevels(bForceSelectAll);
		RefreshEquipmentList();
		m_normalMaterialCount.text = "x0";
		m_superiorMaterialCount.text = "x0";
		m_moneyCount.text = "x0";
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void SetEquipIcon(EquipmentDismantleIcon p_unit)
	{
		NetEquipmentInfo netEquipmentInfo = m_listNetEquipmentInfoFiltered[p_unit.NowIdx];
		EQUIP_TABLE equip = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(netEquipmentInfo.EquipItemID, out equip))
		{
			int[] equipRank = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(netEquipmentInfo);
			p_unit.EquipIcon.SetStarAndLv(equipRank[3], equip.n_LV);
			p_unit.EquipIcon.SetRare(equip.n_RARE);
			p_unit.EquipIcon.Setup(p_unit.NowIdx, AssetBundleScriptableObject.Instance.m_iconEquip, equip.s_ICON, OnClickEquip);
			p_unit.SetEquipmentID(netEquipmentInfo.EquipmentID);
			p_unit.SetSelection(false);
		}
		else
		{
			p_unit.EquipIcon.Clear();
		}
	}

	public void SetupLevelToggle(EquipmentDismantleToggle levelToggle)
	{
		levelToggle.GetToggle().onValueChanged.RemoveAllListeners();
		levelToggle.Setup(m_sortedLevelList[levelToggle.GetIndex()]);
		levelToggle.GetToggle().isOn = m_dictLevelToggleStatus[levelToggle.GetLevel()];
		m_dictLevelToggle.Add(levelToggle.GetLevel(), levelToggle);
		levelToggle.GetToggle().onValueChanged.AddListener(delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			m_dictLevelToggleStatus[levelToggle.GetLevel()] = !m_dictLevelToggleStatus[levelToggle.GetLevel()];
			GroupToggle(levelToggle.GetLevel(), m_dictLevelToggleStatus[levelToggle.GetLevel()]);
		});
	}

	private void RemoveLevelFromEquipmentList(int level)
	{
		KeyValuePair<int, NetEquipmentInfo>[] array = m_dictSelectedEquipmentInfo.Where((KeyValuePair<int, NetEquipmentInfo> x) => ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[x.Value.EquipItemID].n_LV == level).ToArray();
		foreach (KeyValuePair<int, NetEquipmentInfo> keyValuePair in array)
		{
			m_dictSelectedEquipmentInfo.Remove(keyValuePair.Key);
		}
	}

	private void GroupToggle(int level, bool bOn)
	{
		int num = 0;
		foreach (NetEquipmentInfo item in m_listNetEquipmentInfoFiltered)
		{
			if (ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[item.EquipItemID].n_LV == level)
			{
				NetEquipmentInfo netEquipmentInfo = m_listNetEquipmentInfoFiltered[num];
				bool flag = m_dictSelectedEquipmentInfo.ContainsKey(netEquipmentInfo.EquipmentID);
				if (bOn)
				{
					if (!flag)
					{
						m_dictSelectedEquipmentInfo.Add(netEquipmentInfo.EquipmentID, netEquipmentInfo);
					}
				}
				else if (flag)
				{
					m_dictSelectedEquipmentInfo.Remove(netEquipmentInfo.EquipmentID);
				}
			}
			num++;
		}
		UpdateDismantleResult(0.3f);
		m_scrollRect.RefreshCells();
		UpdateDismantleResult(0.3f);
	}

	private void RefreshEquipmentList()
	{
		m_listNetEquipmentInfoFiltered.Clear();
		foreach (NetEquipmentInfo item in m_listNetEquipmentInfo)
		{
			m_listNetEquipmentInfoFiltered.Add(item);
		}
		m_scrollRect.OrangeInit(m_equipmentIcon, m_listNetEquipmentInfoFiltered.Count, m_listNetEquipmentInfoFiltered.Count);
		UpdateDismantleResult(0.3f);
	}

	private void CollectEquipmentLevels(bool bForceSelectAll = false)
	{
		bool value = true;
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>(m_dictLevelToggleStatus);
		m_dictLevelToggle.Clear();
		m_dictLevelToggleStatus.Clear();
		foreach (NetEquipmentInfo item in m_listNetEquipmentInfo)
		{
			EQUIP_TABLE value2;
			if (ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.TryGetValue(item.EquipItemID, out value2) && !m_dictLevelToggleStatus.ContainsKey(value2.n_LV))
			{
				if (dictionary.TryGetValue(value2.n_LV, out value))
				{
					m_dictLevelToggleStatus.Add(value2.n_LV, bForceSelectAll || value);
				}
				else
				{
					m_dictLevelToggleStatus.Add(value2.n_LV, false);
				}
			}
		}
		m_dictLevelToggleStatus.Keys.ToList().Sort((int x, int y) => -x.CompareTo(y));
		m_sortedLevelList = m_dictLevelToggleStatus.Keys.ToList();
		m_sortedLevelList.Sort((int x, int y) => -x.CompareTo(y));
		if ((bool)m_levelToggleRef)
		{
			m_levelToggleScrollRect.OrangeInit(m_levelToggleRef, m_dictLevelToggleStatus.Count, m_dictLevelToggleStatus.Count);
		}
	}

	private void OnClickEquip(int p_idx)
	{
		NetEquipmentInfo netEquipmentInfo = m_listNetEquipmentInfoFiltered[p_idx];
		EQUIP_TABLE equip = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(netEquipmentInfo.EquipItemID, out equip))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			if (m_dictSelectedEquipmentInfo.ContainsKey(netEquipmentInfo.EquipmentID))
			{
				m_dictSelectedEquipmentInfo.Remove(netEquipmentInfo.EquipmentID);
			}
			else
			{
				m_dictSelectedEquipmentInfo.Add(netEquipmentInfo.EquipmentID, netEquipmentInfo);
			}
			if (m_dictSelectedEquipmentInfo.Where((KeyValuePair<int, NetEquipmentInfo> x) => ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[x.Value.EquipItemID].n_LV == equip.n_LV).ToArray().Length == 0)
			{
				m_dictLevelToggle[equip.n_LV].GetToggle().isOn = false;
			}
			UpdateDismantleResult(0.3f);
		}
	}

	private void UpdateDismantleResult(float timer = 0f)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 440001;
		int num5 = 440002;
		foreach (KeyValuePair<int, NetEquipmentInfo> item in m_dictSelectedEquipmentInfo)
		{
			NetEquipmentInfo value = item.Value;
			int[] equipRank = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(value);
			float num6 = 0f;
			if (equipRank[3] == 2)
			{
				num6 = (float)OrangeConst.EQUIP_DESTROY_BONUS_2 / 100f;
			}
			else if (equipRank[3] == 3)
			{
				num6 = (float)OrangeConst.EQUIP_DESTROY_BONUS_3 / 100f;
			}
			EQUIP_TABLE equip = null;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(value.EquipItemID, out equip))
			{
				continue;
			}
			int num7 = Mathf.CeilToInt((float)equip.n_DESTROY_MATERIAL * num6);
			num += equip.n_DESTROY_MONEY;
			if (equip.n_UNLOCK_ID != 0)
			{
				if (equip.n_UNLOCK_ID == num4)
				{
					num2 += equip.n_DESTROY_MATERIAL + num7;
				}
				if (equip.n_UNLOCK_ID == num5)
				{
					num3 += equip.n_DESTROY_MATERIAL + num7;
				}
			}
		}
		int num8 = int.Parse(m_moneyCount.text.Substring(1));
		int num9 = int.Parse(m_normalMaterialCount.text.Substring(1));
		int num10 = int.Parse(m_superiorMaterialCount.text.Substring(1));
		LeanTween.value(num8, num, timer).setOnUpdate(delegate(float val)
		{
			m_moneyCount.text = string.Format("x{0}", (int)val);
		});
		LeanTween.value(num9, num2, timer).setOnUpdate(delegate(float val)
		{
			m_normalMaterialCount.text = string.Format("x{0}", (int)val);
		});
		LeanTween.value(num10, num3, timer).setOnUpdate(delegate(float val)
		{
			m_superiorMaterialCount.text = string.Format("x{0}", (int)val);
		});
		m_confirmBtn.interactable = m_dictSelectedEquipmentInfo.Count > 0;
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
		m_dictSelectedEquipmentInfo.Clear();
		m_dictLevelToggleStatus.Clear();
		m_dictLevelToggle.Clear();
	}

	public void OnClickDismantleBtn()
	{
		if (m_dictSelectedEquipmentInfo.Count == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EQUIP_DESTROY_NOSELECT"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
				{
				});
			});
			return;
		}
		if (m_sortedLevelList.Count > 0)
		{
			int maxLvl = m_maxComposableLevel;
			List<NetEquipmentInfo> maxLvlEquipList = (from x in m_dictSelectedEquipmentInfo
				where ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[x.Value.EquipItemID].n_LV == maxLvl
				select x into kvp
				select kvp.Value).ToList();
			if (maxLvlEquipList.Count > 0)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EquipmentDismantleConfirm", delegate(EquipmentDismantleConfirm ui)
				{
					ui.Setup(maxLvlEquipList, StartDismantle);
					ui.closeCB = delegate
					{
					};
				});
				return;
			}
		}
		StartDismantle();
	}

	private int GetMaxComposableLevel()
	{
		int playerLevel = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		List<EQUIP_TABLE> list = ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.Values.Where((EQUIP_TABLE x) => x.n_LV <= playerLevel).ToList();
		list.Sort((EQUIP_TABLE x, EQUIP_TABLE y) => x.n_LV.CompareTo(y.n_LV));
		if (list.Count() == 0)
		{
			return 1;
		}
		return list.LastOrDefault().n_LV;
	}

	private void StartDismantle()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, NetEquipmentInfo> item in m_dictSelectedEquipmentInfo)
		{
			NetEquipmentInfo value = item.Value;
			list.Add(value.EquipmentID);
		}
		ManagedSingleton<PlayerNetManager>.Instance.DecomposeEquipmentReq(list, delegate(NetRewardsEntity res)
		{
			bool bForceSelectAll = m_dictSelectedEquipmentInfo.Count == m_listNetEquipmentInfoFiltered.Count;
			m_dictSelectedEquipmentInfo.Clear();
			Setup(bForceSelectAll);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				ui.Setup(res.RewardList);
			});
		});
	}

	public bool IsLevelToggleSelected(int level)
	{
		bool value = false;
		m_dictLevelToggleStatus.TryGetValue(level, out value);
		return value;
	}

	public bool IsSelected(int equipmentId)
	{
		return m_dictSelectedEquipmentInfo.ContainsKey(equipmentId);
	}

	public void OnSelectEquipment(EquipmentDismantleIcon icon)
	{
		icon.ToggleSelection();
	}
}
