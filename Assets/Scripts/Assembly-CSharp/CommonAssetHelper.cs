using System;
using CallbackDefs;
using UnityEngine;

public static class CommonAssetHelper
{
	private const string PLAYER_ICON_NAME_POSTFIX = "PlayerIconBase";

	private const string COMMON_ICON_SMALL_NAME_POSTFIX = "CommonIconBaseSmall";

	public static void LoadPlayerIcon(GameObject iconRoot, Action<PlayerIconBase> p_cb = null)
	{
		LoadPlayerIcon(iconRoot.transform, p_cb);
	}

	public static void LoadPlayerIcon(Transform iconRoot, Action<PlayerIconBase> p_cb = null)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "PlayerIconBase", "PlayerIconBase", delegate(GameObject asset)
		{
			PlayerIconBase component = UnityEngine.Object.Instantiate(asset, iconRoot).GetComponent<PlayerIconBase>();
			Action<PlayerIconBase> action = p_cb;
			if (action != null)
			{
				action(component);
			}
		});
	}

	public static void LoadCommonIconSmall(GameObject iconRoot, Action<CommonIconBase> p_cb = null)
	{
		LoadCommonIconSmall(iconRoot.transform, p_cb);
	}

	public static void LoadCommonIconSmall(Transform iconRoot, Action<CommonIconBase> p_cb = null)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate(GameObject asset)
		{
			CommonIconBase component = UnityEngine.Object.Instantiate(asset, iconRoot).GetComponent<CommonIconBase>();
			Action<CommonIconBase> action = p_cb;
			if (action != null)
			{
				action(component);
			}
		});
	}

	public static void LoadLawObj(string assetName, Transform p_parentRoot, Vector3 offset, Callback<Transform> p_cb = null)
	{
	}
}
