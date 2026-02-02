using UnityEngine;

public static class GameObjectExt
{
	public static void SetLayer(this GameObject gameObject, int layer, bool recursively = false)
	{
		gameObject.layer = layer;
		if (recursively)
		{
			Transform[] componentsInChildren = gameObject.transform.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = layer;
			}
		}
	}

	public static string GetFullPath(this GameObject gameObject, bool includeSceneName = false)
	{
		if (!includeSceneName)
		{
			return gameObject.transform.GetFullPath();
		}
		return gameObject.scene.name + ">" + gameObject.transform.GetFullPath();
	}
}
