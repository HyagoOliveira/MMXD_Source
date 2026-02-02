using UnityEngine;

public class CH048_BeamExtraCollider : MonoBehaviour
{
	public BoxCollider2D _boxCollider;

	public Rigidbody2D _rigidbody2D;

	public BeamBullet _mainBullet;

	private void Awake()
	{
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		_boxCollider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
		_boxCollider.isTrigger = true;
	}

	protected virtual void OnTriggerEnter2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	protected virtual void OnTriggerStay2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	protected virtual void OnTriggerHit(Collider2D col)
	{
		if (!(_mainBullet == null) && !(col == null) && _mainBullet.IsActivate && !_mainBullet.IsDestroy && !col.isTrigger)
		{
			_mainBullet.OnTriggerHit(col);
		}
	}
}
