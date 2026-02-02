using UnityEngine;

public class RecordInfoCell : ScrollIndexCallback
{
	private int idx;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
	}

	public void SetupCharacter(int n_ID, int n_STAR, int n_SKIN)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			GameObject obj = Object.Instantiate(asset, base.transform);
			obj.GetComponent<CommonIconBase>().SetupSeasonIcon(n_ID, n_STAR, n_SKIN, 0);
			obj.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
		});
	}

	public void SetupWeapon(int n_ID, int n_STAR, int n_SKIN, int idx)
	{
		WEAPON_TABLE tWeapon_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[n_ID];
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			GameObject obj = Object.Instantiate(asset, base.transform);
			CommonIconBase component = obj.GetComponent<CommonIconBase>();
			component.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, tWeapon_TABLE.s_ICON);
			component.SetupSeasonIcon(n_ID, n_STAR, n_SKIN, idx, true);
			obj.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
		});
	}
}
