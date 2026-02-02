using UnityEngine;

public class StarConditionPopupUI : OrangeUIBase
{
	[SerializeField]
	private RewardPopupUIUnit rewardUnit;

	[SerializeField]
	private OrangeText textMsg;

	[SerializeField]
	private OrangeText textReceive;

	private Transform rewardParent;

	private MISSION_TABLE missionTable;

	private Vector3 rewardSpacing = new Vector3(160f, 0f);

	public void Setup(MISSION_TABLE p_missionTable, bool p_finish)
	{
		missionTable = p_missionTable;
		rewardParent = rewardUnit.transform.parent;
		if (p_finish)
		{
			UpdateText(ref textReceive, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ALREADY_GET"));
		}
		else
		{
			UpdateText(ref textReceive, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NOT_FINISHED"));
		}
		UpdateText(ref textMsg, ManagedSingleton<OrangeTextDataManager>.Instance.MISSIONTEXT_TABLE_DICT.GetL10nValue(p_missionTable.w_NAME));
		SetRewardUnit();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE[] arrayReward = new ITEM_TABLE[3]
		{
			ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(missionTable.n_ITEMID_1) ? ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[missionTable.n_ITEMID_1] : null,
			ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(missionTable.n_ITEMID_2) ? ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[missionTable.n_ITEMID_2] : null,
			ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(missionTable.n_ITEMID_3) ? ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[missionTable.n_ITEMID_3] : null
		};
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.CanShowHow2Get = false;
			ui.Setup(arrayReward[p_idx]);
		});
	}

	private void SetRewardUnit()
	{
		ITEM_TABLE[] array = new ITEM_TABLE[3]
		{
			ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(missionTable.n_ITEMID_1) ? ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[missionTable.n_ITEMID_1] : null,
			ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(missionTable.n_ITEMID_2) ? ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[missionTable.n_ITEMID_2] : null,
			ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.ContainsKey(missionTable.n_ITEMID_3) ? ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[missionTable.n_ITEMID_3] : null
		};
		int[] array2 = new int[3] { missionTable.n_ITEMCOUNT_1, missionTable.n_ITEMCOUNT_2, missionTable.n_ITEMCOUNT_3 };
		for (int i = 0; i < array.Length; i++)
		{
			ITEM_TABLE iTEM_TABLE = array[i];
			if (iTEM_TABLE != null)
			{
				ITEM_TABLE iTEM_TABLE2 = array[i];
				RewardPopupUIUnit rewardPopupUIUnit = Object.Instantiate(rewardUnit, rewardParent);
				rewardPopupUIUnit.transform.localPosition += i * rewardSpacing;
				rewardPopupUIUnit.Setup(i, AssetBundleScriptableObject.Instance.GetIconItem(iTEM_TABLE2.s_ICON), iTEM_TABLE2.s_ICON, iTEM_TABLE2.n_RARE, array2[i], OnClickItem);
				rewardPopupUIUnit.SetPieceActive(iTEM_TABLE2.n_TYPE == 4);
			}
		}
	}

	private void UpdateText(ref OrangeText text, string l10nValue)
	{
		text.text = l10nValue;
		text.UpdateTextImmediate();
	}
}
