using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using enums;

public sealed class ItemBoxUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private ItemBoxUIUnit itemBoxUiUnit;

	[SerializeField]
	private ItemBoxUIUnitEquip itemBoxUiUnitEquip;

	[SerializeField]
	private ItemBoxTab[] itemBoxTab;

	[SerializeField]
	private Transform itemPanelRoot;

	[SerializeField]
	private RawImage modeImg;

	[SerializeField]
	private Button suitEffectBtn;

	[SerializeField]
	private GameObject suitEffectDialogGroup;

	[SerializeField]
	private RectTransform suitEffectDialog;

	[SerializeField]
	private OrangeText[] suitEffectText;

	[SerializeField]
	private Image[] suitEffectIcon;

	[SerializeField]
	private Button[] suitFunctionBtns;

	[SerializeField]
	private ItemBoxEquip equipRoot;

	[SerializeField]
	private Transform equipmentCountRoot;

	[SerializeField]
	private OrangeText equipmentCountText;

	private List<NetItemInfo> listNetItemInfo = new List<NetItemInfo>();

	private List<NetEquipmentInfo> listNetEquipmentInfo = new List<NetEquipmentInfo>();

	private RenderTextureObj textureObj;

	private List<int> listEffectID = new List<int>();

	private ItemType itemType;

	private int visualCount = 30;

	public void Setup(ItemType p_type)
	{
		itemType = p_type;
		switch (itemType)
		{
		case ItemType.Currency:
			listNetEquipmentInfo.Clear();
			listNetEquipmentInfo = ManagedSingleton<EquipHelper>.Instance.GetListNetEquipmentExceptEquipped();
			AddEmptyData(visualCount - listNetEquipmentInfo.Count, ref listNetEquipmentInfo);
			scrollRect.OrangeInit(itemBoxUiUnitEquip, visualCount, listNetEquipmentInfo.Count);
			UpdateEquipmentCount();
			break;
		case ItemType.Consumption:
		case ItemType.Material:
		case ItemType.Shard:
		case ItemType.GiftPack:
			listNetItemInfo.Clear();
			listNetItemInfo = ManagedSingleton<PlayerHelper>.Instance.GetListNetItemByType(itemType);
			AddEmptyData(visualCount - listNetItemInfo.Count, ref listNetItemInfo);
			scrollRect.OrangeInit(itemBoxUiUnit, visualCount, listNetItemInfo.Count);
			UpdateEquipmentCount(false);
			break;
		}
		int num = (int)(itemType - 1);
		for (int i = 0; i < itemBoxTab.Length; i++)
		{
			itemBoxTab[i].UpdateState(num != i);
		}
		if (textureObj == null)
		{
			DrawCharacterModel(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara);
		}
		itemPanelRoot.gameObject.SetActive(true);
		CheckSuitEffect();
		suitEffectBtn.gameObject.SetActive(listEffectID.Count != 0);
		EnableEquipmentButtons();
	}

	private void UpdateEquipmentCount(bool bEnable = true)
	{
		if ((bool)equipmentCountRoot)
		{
			equipmentCountRoot.gameObject.SetActive(bEnable);
		}
		if (bEnable && !(equipmentCountText == null))
		{
			int count = ManagedSingleton<EquipHelper>.Instance.GetListNetEquipmentExceptEquipped().Count;
			if (count >= OrangeConst.EQUIP_MAX_SLOT)
			{
				equipmentCountText.text = "<color=#ff0000>" + count + "</Color>/" + OrangeConst.EQUIP_MAX_SLOT;
			}
			else
			{
				equipmentCountText.text = count + "/" + OrangeConst.EQUIP_MAX_SLOT;
			}
		}
	}

	public void EnableEquipSelectionFrame(bool bEnable)
	{
		if (equipRoot != null)
		{
			equipRoot.EnableSelectionFrame(bEnable);
		}
		EnableEquipmentButtons();
	}

	private void EnableEquipmentButtons()
	{
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		bool flag = lV >= OrangeConst.OPENRANK_EQUIP;
		bool flag2 = lV >= OrangeConst.OPENRANK_EQUIP_LVUP;
		Text componentInChildren = suitFunctionBtns[0].GetComponentInChildren<Text>();
		Text componentInChildren2 = suitFunctionBtns[1].GetComponentInChildren<Text>();
		if (ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip().Count <= 0)
		{
			flag2 = false;
		}
		suitFunctionBtns[0].interactable = flag;
		suitFunctionBtns[1].interactable = flag2;
		if (flag)
		{
			componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 1f);
		}
		else
		{
			componentInChildren.color = new Color(componentInChildren.color.r, componentInChildren.color.g, componentInChildren.color.b, 0.5f);
		}
		if (flag2)
		{
			componentInChildren2.color = new Color(componentInChildren2.color.r, componentInChildren2.color.g, componentInChildren2.color.b, 1f);
		}
		else
		{
			componentInChildren2.color = new Color(componentInChildren2.color.r, componentInChildren2.color.g, componentInChildren2.color.b, 0.5f);
		}
	}

	private void OnDestroy()
	{
		if (null != textureObj)
		{
			Object.Destroy(textureObj.gameObject);
		}
	}

	private void DrawCharacterModel(int CharID)
	{
		if (!(null != textureObj))
		{
			SKIN_TABLE skinTable = null;
			CharacterInfo characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.Value(CharID);
			ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(characterInfo.netInfo.Skin, out skinTable);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(GameObject obj)
			{
				modeImg.gameObject.SetActive(true);
				textureObj = Object.Instantiate(obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
				textureObj.AssignNewRender(ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[CharID], ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID], skinTable, new Vector3(0f, -0.6f, 5f), modeImg);
			});
		}
	}

	private void AddEmptyData<T>(int diff, ref List<T> list) where T : NetData, new()
	{
		T item = new T();
		for (int i = 0; i < diff; i++)
		{
			list.Add(item);
		}
	}

	public void SetItemIcon(ItemBoxUIUnit p_unit)
	{
		ITEM_TABLE item = null;
		NetItemInfo netItemInfo = listNetItemInfo[p_unit.NowIdx];
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(netItemInfo.ItemID, out item))
		{
			p_unit.ItemIcon.SetRare(item.n_RARE);
			p_unit.ItemIcon.Setup(p_unit.NowIdx, AssetBundleScriptableObject.Instance.GetIconItem(item.s_ICON), item.s_ICON, OnClickItem);
			p_unit.ItemIcon.SetAmount(netItemInfo.Stack);
			p_unit.ItemIcon.SetRareItemEffect(item.n_RARE == 5);
		}
		else
		{
			p_unit.ItemIcon.Clear();
		}
	}

	public void SetEquipIcon(ItemBoxUIUnitEquip p_unit)
	{
		NetEquipmentInfo netEquipmentInfo = listNetEquipmentInfo[p_unit.NowIdx];
		EQUIP_TABLE equip = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(netEquipmentInfo.EquipItemID, out equip))
		{
			int[] equipRank = ManagedSingleton<EquipHelper>.Instance.GetEquipRank(netEquipmentInfo);
			p_unit.EquipIcon.SetStarAndLv(equipRank[3], equip.n_LV);
			p_unit.EquipIcon.SetRare(equip.n_RARE);
			p_unit.EquipIcon.Setup(p_unit.NowIdx, AssetBundleScriptableObject.Instance.m_iconEquip, equip.s_ICON, OnClickEquip);
			p_unit.EquipIcon.SetRareEquipEffect(equip.n_RARE == 5);
		}
		else
		{
			p_unit.EquipIcon.Clear();
		}
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(listNetItemInfo[p_idx].ItemID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.Setup(item, listNetItemInfo[p_idx]);
			});
		}
	}

	private void OnClickEquip(int p_idx)
	{
		NetEquipmentInfo netEquip = listNetEquipmentInfo[p_idx];
		EQUIP_TABLE equip = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetEquip(netEquip.EquipItemID, out equip))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemBoxEquipInfo", delegate(ItemBoxEquipInfo ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(netEquip);
			});
		}
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_PLAYER_BOX, SetupFromEvent);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_PLAYER_BOX, SetupFromEvent);
		MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(itemBoxUiUnit.itemName);
		MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(itemBoxUiUnitEquip.itemName);
		MonoBehaviourSingleton<AudioManager>.Instance.Stop("NAVI_MENU");
	}

	private void SetupFromEvent()
	{
		if (itemPanelRoot.gameObject.activeSelf)
		{
			Setup(itemType);
		}
	}

	public void OnClickItemBoxCurrency()
	{
		Setup(ItemType.Currency);
	}

	public void OnClickItemBoxConsumption()
	{
		Setup(ItemType.Consumption);
	}

	public void OnClickItemBoxMaterial()
	{
		Setup(ItemType.Material);
	}

	public void OnClickItemBoxShard()
	{
		Setup(ItemType.Shard);
	}

	public void OnClickItemBoxGiftPack()
	{
		Setup(ItemType.GiftPack);
	}

	public void OnClickSuitEffectBtn()
	{
		CheckSuitEffect();
		suitEffectDialogGroup.SetActive(true);
		int effectCount = 0;
		foreach (int item in listEffectID)
		{
			SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[item];
			suitEffectText[effectCount].text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(sKILL_TABLE.w_TIP);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(sKILL_TABLE.s_ICON), sKILL_TABLE.s_ICON, delegate(Sprite obj)
			{
				suitEffectIcon[effectCount].sprite = obj;
			});
			effectCount++;
		}
		switch (listEffectID.Count)
		{
		case 1:
			suitEffectDialog.sizeDelta = new Vector2(510f, 210f);
			break;
		case 2:
			suitEffectDialog.sizeDelta = new Vector2(510f, 370f);
			break;
		default:
			suitEffectDialog.sizeDelta = new Vector2(510f, 540f);
			break;
		}
	}

	private void CheckSuitEffect()
	{
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		NetEquipmentInfo netEquipment = null;
		SUIT_TABLE sUIT_TABLE = null;
		Dictionary<int, NetEquipmentInfo> dicEquipmentIsEquip = ManagedSingleton<EquipHelper>.Instance.GetDicEquipmentIsEquip();
		for (int i = 1; i <= 6; i++)
		{
			if (!dicEquipmentIsEquip.TryGetValue(i, out netEquipment))
			{
				continue;
			}
			switch (i)
			{
			case 1:
				sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_1 == netEquipment.EquipItemID).FirstOrDefault();
				break;
			case 2:
				sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_2 == netEquipment.EquipItemID).FirstOrDefault();
				break;
			case 3:
				sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_3 == netEquipment.EquipItemID).FirstOrDefault();
				break;
			case 4:
				sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_4 == netEquipment.EquipItemID).FirstOrDefault();
				break;
			case 5:
				sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_5 == netEquipment.EquipItemID).FirstOrDefault();
				break;
			case 6:
				sUIT_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.Values.Where((SUIT_TABLE x) => x.n_EQUIP_6 == netEquipment.EquipItemID).FirstOrDefault();
				break;
			}
			if (sUIT_TABLE != null)
			{
				List<int> value;
				if (!dictionary.TryGetValue(sUIT_TABLE.n_ID, out value))
				{
					List<int> list = new List<int>();
					list.Add(netEquipment.EquipItemID);
					dictionary.Add(sUIT_TABLE.n_ID, list);
				}
				else
				{
					value.Add(netEquipment.EquipItemID);
				}
			}
		}
		listEffectID.Clear();
		foreach (KeyValuePair<int, List<int>> item in dictionary)
		{
			int key = item.Key;
			List<int> value2 = item.Value;
			SUIT_TABLE sUIT_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT[key];
			if (value2.Count >= sUIT_TABLE2.n_SUIT_1)
			{
				listEffectID.Add(sUIT_TABLE2.n_EFFECT_1);
			}
			if (value2.Count >= sUIT_TABLE2.n_SUIT_2)
			{
				listEffectID.Add(sUIT_TABLE2.n_EFFECT_2);
			}
			if (value2.Count >= sUIT_TABLE2.n_SUIT_3)
			{
				listEffectID.Add(sUIT_TABLE2.n_EFFECT_3);
			}
		}
	}

	public void OnClickSuitEffectCloseBtn()
	{
		suitEffectDialogGroup.SetActive(false);
	}

	public void MoveBox(bool active)
	{
	}

	public void EnableItemPanel(bool bActive)
	{
		itemPanelRoot.gameObject.SetActive(bActive);
	}

	public override void OnClickCloseBtn()
	{
		if (null != textureObj)
		{
			Object.Destroy(textureObj.gameObject);
		}
		base.OnClickCloseBtn();
	}

	public override void SetCanvas(bool enable)
	{
		base.SetCanvas(enable);
		if (textureObj != null)
		{
			textureObj.SetCameraActive(enable);
		}
	}
}
