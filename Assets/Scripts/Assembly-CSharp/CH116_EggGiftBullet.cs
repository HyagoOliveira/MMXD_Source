public class CH116_EggGiftBullet : BasicBullet
{
	private CH116_EggMeshController _meshController;

	protected override void Awake()
	{
		base.Awake();
		_meshController = GetComponentInChildren<CH116_EggMeshController>();
	}

	public void SetMeshIndex(int index)
	{
		_meshController.SetMeshIndex(index);
	}
}
