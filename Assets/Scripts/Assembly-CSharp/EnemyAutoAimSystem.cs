#define RELEASE
using System;
using System.Linq;
using Better;
using UnityEngine;

public class EnemyAutoAimSystem : MonoBehaviour
{
	public enum ColliderType
	{
		Null = 0,
		Circle = 1,
		Box = 2,
		Capsule = 3
	}

	private bool _initialized;

	[HideInInspector]
	public readonly Dictionary<Collider2D, OrangeCharacter> AutoAimTarget = new Dictionary<Collider2D, OrangeCharacter>();

	private ColliderType _currentColliderType;

	[SerializeField]
	private Collider2D _collider2D;

	private EnemyControllerBase _enemyController;

	public Vector2 Range = new Vector2(10f, 10f);

	public Vector2 Offset = Vector2.zero;

	private Transform _transform;

	private LayerMask _targetMask;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (!_initialized)
		{
			_transform = base.transform;
			_enemyController = GetComponentInParent<EnemyControllerBase>();
			base.gameObject.AddOrGetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
			base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.AISLayer;
			_targetMask = ManagedSingleton<OrangeLayerManager>.Instance.EnemyUseMask;
			_initialized = true;
		}
	}

	public void SetColliderType(ColliderType colliderType)
	{
		if (_collider2D != null)
		{
			switch (colliderType)
			{
			case ColliderType.Null:
				Debug.LogWarning("Invalid type");
				break;
			case ColliderType.Circle:
			case ColliderType.Capsule:
				if ((bool)(_collider2D as CapsuleCollider2D))
				{
					_currentColliderType = colliderType;
					return;
				}
				break;
			case ColliderType.Box:
				if ((bool)(_collider2D as BoxCollider2D))
				{
					_currentColliderType = colliderType;
					return;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException("colliderType", colliderType, null);
			}
			UnityEngine.Object.Destroy(_collider2D);
			_collider2D = null;
		}
		switch (colliderType)
		{
		case ColliderType.Null:
			Debug.LogWarning("Invalid type");
			break;
		case ColliderType.Box:
			_collider2D = base.gameObject.AddComponent<BoxCollider2D>();
			break;
		default:
			_collider2D = base.gameObject.AddComponent<CapsuleCollider2D>();
			((CapsuleCollider2D)_collider2D).direction = CapsuleDirection2D.Horizontal;
			break;
		}
		_collider2D.isTrigger = true;
		_currentColliderType = colliderType;
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (AutoAimTarget.ContainsKey(col) || 1 << col.gameObject.layer != ((1 << col.gameObject.layer) & (int)_targetMask) || col.isTrigger)
		{
			return;
		}
		OrangeCharacter orangeCharacter = col.gameObject.GetComponent<OrangeCharacter>();
		if (orangeCharacter == null)
		{
			RideArmorController component = col.gameObject.GetComponent<RideArmorController>();
			if ((bool)component && (bool)component.MasterPilot)
			{
				orangeCharacter = component.MasterPilot;
			}
		}
		if ((bool)orangeCharacter)
		{
			if ((bool)_enemyController)
			{
				_enemyController.OnTargetEnter(orangeCharacter);
			}
			AutoAimTarget.Add(col, orangeCharacter);
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (!AutoAimTarget.ContainsKey(col) || 1 << col.gameObject.layer != ((1 << col.gameObject.layer) & (int)_targetMask) || col.isTrigger)
		{
			return;
		}
		OrangeCharacter orangeCharacter = col.gameObject.GetComponent<OrangeCharacter>();
		if (orangeCharacter == null)
		{
			RideArmorController component = col.gameObject.GetComponent<RideArmorController>();
			if ((bool)component && (bool)component.MasterPilot)
			{
				orangeCharacter = component.MasterPilot;
			}
		}
		if ((bool)orangeCharacter && AutoAimTarget.ContainsValue(orangeCharacter))
		{
			AutoAimTarget.Remove(col);
			if ((bool)_enemyController)
			{
				_enemyController.OnTargetExit(orangeCharacter);
			}
		}
	}

	public void UpdateAimRange(float val)
	{
		Vector2 size = Vector2.right * val;
		UpdateAimRange(Vector2.zero, size);
	}

	public void UpdateAimRange(float x, float y)
	{
		UpdateAimRange(Vector2.zero, new Vector2(x, y));
	}

	public void UpdateAimRange(Vector2 offset, Vector2 size)
	{
		if (size.x <= 0f)
		{
			Debug.LogWarning("EnemyAIS : range is invalid");
			return;
		}
		SetColliderType((size.y == 0f) ? ColliderType.Circle : ColliderType.Box);
		Offset = offset;
		Range = size;
		if (!_collider2D)
		{
			Debug.LogWarning("no EnemyAIS ");
		}
		_collider2D.offset = Offset;
		switch (_currentColliderType)
		{
		default:
			((CapsuleCollider2D)_collider2D).size = new Vector2(Range.x * 2f, Range.x * 2f);
			break;
		case ColliderType.Box:
			((BoxCollider2D)_collider2D).size = Range * 2f;
			break;
		case ColliderType.Capsule:
			((CapsuleCollider2D)_collider2D).size = Range * 2f;
			break;
		case ColliderType.Null:
			Debug.LogWarning("Invalid type");
			break;
		}
	}

	public OrangeCharacter GetClosetPlayer(Transform sourceTransform = null)
	{
		if (AutoAimTarget.Count == 0)
		{
			return null;
		}
		if (sourceTransform == null)
		{
			sourceTransform = _transform;
		}
		OrangeCharacter orangeCharacter = null;
		float num = float.MaxValue;
		OrangeCharacter[] array = AutoAimTarget.Values.ToArray();
		foreach (OrangeCharacter orangeCharacter2 in array)
		{
			if (orangeCharacter2.AllowAutoAim && orangeCharacter2.isActiveAndEnabled && (orangeCharacter == null || Vector3.Distance(sourceTransform.position, orangeCharacter2._transform.position) < num))
			{
				orangeCharacter = orangeCharacter2;
				num = Vector3.Distance(sourceTransform.position, orangeCharacter2._transform.position);
			}
		}
		return orangeCharacter;
	}

	public bool CheckContainPlayer(Vector3 playerpos)
	{
		if (_collider2D == null)
		{
			return false;
		}
		return _collider2D.bounds.Contains(playerpos);
	}
}
