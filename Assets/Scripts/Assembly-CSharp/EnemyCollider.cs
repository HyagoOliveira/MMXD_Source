using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
	public enum ColliderType
	{
		Box = 0,
		Circle = 1,
		Capsule = 2
	}

	private Collider2D _collider;

	public ColliderType colliderType = ColliderType.Circle;

	public Vector2 Offset = Vector2.zero;

	public Vector2 Size = new Vector2(0.5f, 0.5f);

	private void Awake()
	{
		switch (colliderType)
		{
		case ColliderType.Circle:
			_collider = base.gameObject.AddOrGetComponent<CircleCollider2D>();
			((CircleCollider2D)_collider).radius = Size.x;
			break;
		case ColliderType.Capsule:
			_collider = base.gameObject.AddOrGetComponent<CapsuleCollider2D>();
			((CapsuleCollider2D)_collider).size = Size;
			break;
		case ColliderType.Box:
			_collider = base.gameObject.AddOrGetComponent<BoxCollider2D>();
			((BoxCollider2D)_collider).size = Size;
			break;
		}
		_collider.offset = Offset;
		base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer;
		_collider.enabled = false;
	}

	public void SetOffset(Vector2 v)
	{
		if ((bool)_collider)
		{
			Offset = v;
			_collider.offset = Offset;
		}
	}

	public void SetSize(Vector2 v)
	{
		if ((bool)_collider)
		{
			Size = v;
			switch (colliderType)
			{
			case ColliderType.Circle:
				((CircleCollider2D)_collider).radius = Size.x;
				break;
			case ColliderType.Capsule:
				((CapsuleCollider2D)_collider).size = Size;
				break;
			case ColliderType.Box:
				((BoxCollider2D)_collider).size = Size;
				break;
			}
		}
	}

	public bool IsColliderEnable()
	{
		return _collider.enabled;
	}

	public void SetColliderEnable(bool enabled = true)
	{
		if ((bool)_collider)
		{
			_collider.enabled = enabled;
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		switch (colliderType)
		{
		case ColliderType.Circle:
			Gizmos.DrawWireSphere(base.transform.position + (Vector3)Offset, Size.x * base.transform.lossyScale.x);
			break;
		case ColliderType.Box:
		case ColliderType.Capsule:
			Gizmos.DrawWireCube(base.transform.position + (Vector3)Offset, Size);
			break;
		}
	}

	public bool IsTouching(Collider2D col)
	{
		if ((bool)col)
		{
			return _collider.IsTouching(col);
		}
		return false;
	}
}
