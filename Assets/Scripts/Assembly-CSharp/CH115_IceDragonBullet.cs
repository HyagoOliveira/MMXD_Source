public class CH115_IceDragonBullet : DragonBulletBase<CH115_IceDragonBullet>
{
	protected override void Awake()
	{
		base.Awake();
		DelayTime = 0f;
	}

	public override void SetDistance()
	{
		switch (base.PartitionType)
		{
		case Partition.Head:
			DistanceToPrevBullet = 0f;
			break;
		case Partition.Body:
			if ((bool)base.PrevBullet && base.PrevBullet.PartitionType == Partition.Head)
			{
				DistanceToPrevBullet = 1f;
			}
			else
			{
				DistanceToPrevBullet = 0.75f;
			}
			DistanceToPrevBullet = 0.75f;
			break;
		case Partition.Tail:
			DistanceToPrevBullet = 0.85f;
			break;
		}
	}
}
