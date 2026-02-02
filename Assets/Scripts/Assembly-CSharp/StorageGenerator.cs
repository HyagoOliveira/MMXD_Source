using System;
using System.Collections.Generic;
using UnityEngine;

public static class StorageGenerator
{
	public static void Load(string p_compName, List<StorageInfo> p_listStorages, int p_defaultIdx, int p_defaultSubIdx, Transform p_parentRoot, Action<GameObject> p_completeCb = null)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("UI/" + p_compName, p_compName, delegate(GameObject obj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj, p_parentRoot);
			gameObject.GetComponent<StorageComponent>().Setup(p_listStorages, p_defaultIdx, p_defaultSubIdx);
			p_completeCb.CheckTargetToInvoke(gameObject);
		});
	}
}
