using UnityEngine;
using UnityEngine.UI;

public class RankRewardScrollCell : ScrollIndexCallback
{
	[SerializeField]
	private Text ItemNameText;

	[SerializeField]
	private Text ItemCountText;

	[SerializeField]
	private GameObject IconTransform;

	[SerializeField]
	private GameObject[] BGImgs;

	private int idx;

	private Color32[] colors = new Color32[8]
	{
		new Color32(0, 186, byte.MaxValue, byte.MaxValue),
		new Color32(0, 186, byte.MaxValue, byte.MaxValue),
		new Color32(0, 186, byte.MaxValue, byte.MaxValue),
		new Color32(251, 219, 214, byte.MaxValue),
		new Color32(216, 192, byte.MaxValue, byte.MaxValue),
		new Color32(216, 192, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, 230, 93, byte.MaxValue),
		new Color32(byte.MaxValue, 230, 93, byte.MaxValue)
	};

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
		idx = p_idx;
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			});
		}
	}

	public void SetupItem(int rank, int idx, int cnt, ITEM_TABLE tbl)
	{
		rank = ((rank <= 0) ? 1 : rank);
		ItemNameText.color = colors[rank - 1];
		ItemNameText.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(tbl.w_NAME);
		ItemCountText.text = "x" + cnt;
		for (int i = 0; i < BGImgs.Length; i++)
		{
			bool active = i + 1 == rank;
			BGImgs[i].SetActive(active);
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			Object.Instantiate(asset, IconTransform.transform).GetComponent<CommonIconBase>().SetupItem(tbl.n_ID, tbl.n_ID, OnClickItem);
		});
	}
}
