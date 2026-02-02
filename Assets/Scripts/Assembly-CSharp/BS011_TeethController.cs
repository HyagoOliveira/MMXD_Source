public class BS011_TeethController : BS011_PartsController
{
	private bool _isVisible;

	private CharacterMaterial _characterMaterial;

	private void Start()
	{
		_characterMaterial = GetComponent<CharacterMaterial>();
	}

	public override int SetVisible(bool visible = true)
	{
		_isVisible = visible;
		if ((bool)_enemyCollider)
		{
			_enemyCollider.SetColliderEnable(visible);
		}
		if (!visible)
		{
			_collideBullet.BackToPool();
		}
		if (!visible)
		{
			return _characterMaterial.Disappear(delegate
			{
			});
		}
		return _characterMaterial.Appear(delegate
		{
			if (_isVisible)
			{
				_collideBullet.SetBulletAtk(null, MasterController.selfBuffManager.sBuffStatus, MasterController.EnemyData);
				_collideBullet.Active(_targetMask);
			}
		});
	}
}
