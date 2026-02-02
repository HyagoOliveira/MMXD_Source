using UnityEngine;

public static class TransformExt
{
	public static string GetFullPath(this Transform transform)
	{
		if (!(transform.parent != null))
		{
			return transform.name;
		}
		return transform.parent.GetFullPath() + "/" + transform.name;
	}
}
