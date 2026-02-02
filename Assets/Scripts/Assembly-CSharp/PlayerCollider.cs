using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
	public enum ColliderType
	{
		Box = 0,
		Circle = 1,
		Capsule = 2
	}

	protected StageObjParam Owner;

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
	}

	private void Start()
	{
		if (base.gameObject.transform.root.gameObject.layer != base.gameObject.layer)
		{
			base.gameObject.layer = base.gameObject.transform.root.gameObject.layer;
		}
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

	private void Update()
	{
		if (base.gameObject.transform.root.gameObject.layer != base.gameObject.layer)
		{
			base.gameObject.layer = base.gameObject.transform.root.gameObject.layer;
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

	public Collider2D GetCollider2D()
	{
		return _collider;
	}

	public void SetDmgReduceOwner(StageObjParam owner)
	{
		Owner = owner;
	}

	public StageObjParam GetDmgReduceOwner()
	{
		return Owner;
	}

	public Transform GetDmgReduceOwnerTransform()
	{
		if ((bool)Owner)
		{
			return Owner.transform;
		}
		return null;
	}

	public bool IsDmgReduceShield()
	{
		if (Owner != null)
		{
			return true;
		}
		return false;
	}
}
