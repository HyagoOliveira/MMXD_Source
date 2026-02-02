using UnityEngine;

public class PoolBaseObject : MonoBehaviour
{
	[HideInInspector]
	public string itemName;

	public virtual void ResetStatus()
	{
	}

	public virtual void BackToPool()
	{
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}
}
