public class EnemyHumanPoolObject : PoolBaseObject
{
	public string poolName;

	public override void BackToPool()
	{
		base.transform.name = poolName;
		base.BackToPool();
	}

	public override void ResetStatus()
	{
		base.ResetStatus();
		base.transform.name = poolName;
	}
}
