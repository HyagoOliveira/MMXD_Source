using UnityEngine;

public class BulletExtraCollision : MonoBehaviour
{
	private BoxCollider2D boxCollider;

	public BasicBullet MasterBullet;

	private Rigidbody2D _rigidbody2D;

	[SerializeField]
	private Vector2 colliderOffset = new Vector2(-0.6f, 0f);

	[SerializeField]
	private Vector2 colliderSize = new Vector2(0.3f, 0.3f);

	private int _aliveFrame;

	public int AliveFrame
	{
		get
		{
			return _aliveFrame;
		}
		set
		{
			_aliveFrame = value;
			if (value <= 0)
			{
				_aliveFrame = 0;
				_rigidbody2D.Sleep();
				boxCollider.enabled = false;
			}
			else
			{
				_rigidbody2D.WakeUp();
				boxCollider.enabled = true;
			}
		}
	}

	private void Awake()
	{
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		boxCollider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
		boxCollider.offset = colliderOffset;
		boxCollider.size = colliderSize;
		boxCollider.isTrigger = true;
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
		if (((1 << col.gameObject.layer) & (int)MasterBullet.BlockMask) == 0 && !col.isTrigger && ((1 << col.gameObject.layer) & (int)MasterBullet.UseMask) != 0)
		{
			MasterBullet.OnTriggerHit(col);
		}
	}
}
