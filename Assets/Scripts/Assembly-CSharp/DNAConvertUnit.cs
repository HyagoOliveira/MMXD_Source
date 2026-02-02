using UnityEngine;

public class DNAConvertUnit : ScrollIndexCallback
{
	[SerializeField]
	private ItemIconWithAmount itemIconWithAmount;

	[SerializeField]
	private DNAConvert parentUI;

	private int _idx;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		if ((bool)parentUI)
		{
			base.gameObject.SetActive(true);
			NetItemInfo itemInfo = parentUI.GetItemInfo(_idx);
			ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[itemInfo.ItemID];
			string iconItem = AssetBundleScriptableObject.Instance.GetIconItem(iTEM_TABLE.s_ICON);
			string s_ICON = iTEM_TABLE.s_ICON;
			int n_RARE = iTEM_TABLE.n_RARE;
			itemIconWithAmount.Setup(_idx, iconItem, s_ICON, parentUI.OnClickUnit);
			itemIconWithAmount.SetRare(n_RARE);
			itemIconWithAmount.SetAmount(itemInfo.Stack);
		}
	}
}
