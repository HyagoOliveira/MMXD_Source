using UnityEngine;

public class ShadowBuilder
{
	public static void CreateShadow(Transform parent)
	{
		if (!MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.sw)
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/shadowprojector", "shadowprojector", delegate(GameObject go)
		{
			if (null != go)
			{
				Object.Instantiate(go, parent, false).transform.localPosition = new Vector3(0f, 0f, 0f);
			}
		});
	}
}
