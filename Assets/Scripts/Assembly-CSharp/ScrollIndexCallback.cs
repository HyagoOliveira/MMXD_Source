#define RELEASE
public abstract class ScrollIndexCallback : PoolBaseObject
{
	public virtual void ScrollCellIndex(int idx)
	{
		Debug.Log(idx);
	}

	public virtual void RefreshCell()
	{
	}

	public override void BackToPool()
	{
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}
}
