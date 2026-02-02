using System.Linq;
using Better;
using UnityEngine;

public class NeutralAutoAimSystem : MonoBehaviour
{
	[HideInInspector]
	public readonly Dictionary<Collider2D, IAimTarget> TargetDictionary = new Dictionary<Collider2D, IAimTarget>();

	[HideInInspector]
	private CircleCollider2D _circleCollider;

	public float _range = 10f;

	private static readonly float deadzone = 0.5f;

	private Transform _transform;

	private LayerMask neutralMask;

	private LayerMask playerMask;

	private LayerMask enemyMask;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		TargetDictionary.Clear();
		if (!(null != _circleCollider))
		{
			base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.AISLayer;
			_transform = base.transform;
			_circleCollider = base.gameObject.AddOrGetComponent<CircleCollider2D>();
			_circleCollider.offset = Vector2.zero;
			_circleCollider.radius = _range;
			_circleCollider.isTrigger = true;
			base.gameObject.AddOrGetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
			playerMask = ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
			enemyMask = ManagedSingleton<OrangeLayerManager>.Instance.EnemyUseMask;
			neutralMask = (int)playerMask | (int)enemyMask;
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (!col.isTrigger && 1 << col.gameObject.layer == ((1 << col.gameObject.layer) & (int)neutralMask) && !TargetDictionary.ContainsKey(col))
		{
			IAimTarget componentInParent = col.gameObject.GetComponentInParent<IAimTarget>();
			if (componentInParent != null)
			{
				TargetDictionary.Add(col, componentInParent);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (!col.isTrigger && 1 << col.gameObject.layer == ((1 << col.gameObject.layer) & (int)neutralMask) && TargetDictionary.ContainsKey(col))
		{
			IAimTarget component = col.gameObject.GetComponent<IAimTarget>();
			if (component != null && TargetDictionary.ContainsValue(component))
			{
				TargetDictionary.Remove(col);
			}
		}
	}

	public void UpdateAimRange(float val)
	{
		Init();
		_range = Mathf.Max(1f, val - deadzone);
		_circleCollider.radius = _range;
	}

	public IAimTarget GetClosetPlayer(Transform sourceTransform = null)
	{
		if (TargetDictionary.Count == 0)
		{
			return null;
		}
		if (sourceTransform == null)
		{
			sourceTransform = _transform;
		}
		IAimTarget aimTarget = null;
		float num = float.MaxValue;
		IAimTarget[] array = TargetDictionary.Values.ToArray();
		foreach (IAimTarget aimTarget2 in array)
		{
			if (aimTarget2.AutoAimType == AimTargetType.Player && aimTarget2.AllowAutoAim)
			{
				OrangeCharacter orangeCharacter = aimTarget2 as OrangeCharacter;
				if (!(orangeCharacter == null) && orangeCharacter.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer && (aimTarget == null || Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position) < num))
				{
					aimTarget = aimTarget2;
					num = Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position);
				}
			}
		}
		return aimTarget;
	}

	public IAimTarget GetClosetEnemy(Transform sourceTransform = null)
	{
		if (TargetDictionary.Count == 0)
		{
			return null;
		}
		if (sourceTransform == null)
		{
			sourceTransform = _transform;
		}
		IAimTarget aimTarget = null;
		float num = float.MaxValue;
		IAimTarget[] array = TargetDictionary.Values.ToArray();
		foreach (IAimTarget aimTarget2 in array)
		{
			if (aimTarget2.AutoAimType == AimTargetType.Enemy && aimTarget2.AllowAutoAim && (aimTarget == null || Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position) < num))
			{
				aimTarget = aimTarget2;
				num = Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position);
			}
		}
		return aimTarget;
	}

	public IAimTarget GetClosetTarget(Transform sourceTransform = null)
	{
		if (TargetDictionary.Count == 0)
		{
			return null;
		}
		if (sourceTransform == null)
		{
			sourceTransform = _transform;
		}
		IAimTarget aimTarget = null;
		float num = float.MaxValue;
		IAimTarget[] array = TargetDictionary.Values.ToArray();
		foreach (IAimTarget aimTarget2 in array)
		{
			if (aimTarget2.AllowAutoAim && (aimTarget == null || Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position) < num))
			{
				aimTarget = aimTarget2;
				num = Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position);
			}
		}
		return aimTarget;
	}

	public IAimTarget GetClosetPvpPlayer(Transform sourceTransform = null)
	{
		if (TargetDictionary.Count == 0)
		{
			return null;
		}
		if (sourceTransform == null)
		{
			sourceTransform = _transform;
		}
		IAimTarget aimTarget = null;
		float num = float.MaxValue;
		IAimTarget[] array = TargetDictionary.Values.ToArray();
		foreach (IAimTarget aimTarget2 in array)
		{
			if (aimTarget2.AllowAutoAim)
			{
				OrangeCharacter orangeCharacter = aimTarget2 as OrangeCharacter;
				if (!(orangeCharacter == null) && orangeCharacter.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer && (aimTarget == null || Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position) < num))
				{
					aimTarget = aimTarget2;
					num = Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position);
				}
			}
		}
		return aimTarget;
	}

	public IAimTarget GetClosetTarget(int targetType, int buffId, Transform sourceTransform = null)
	{
		if (TargetDictionary.Count == 0)
		{
			return null;
		}
		IAimTarget aimTarget = null;
		float num = float.MaxValue;
		if (sourceTransform == null)
		{
			sourceTransform = _transform;
		}
		IAimTarget[] array = TargetDictionary.Values.ToArray();
		foreach (IAimTarget aimTarget2 in array)
		{
			if (!aimTarget2.AllowAutoAim || (targetType == 1 && aimTarget2.AutoAimType != AimTargetType.Player) || (targetType == 2 && aimTarget2.AutoAimType != AimTargetType.Enemy))
			{
				continue;
			}
			if (targetType == 3)
			{
				OrangeCharacter orangeCharacter = aimTarget2 as OrangeCharacter;
				if (orangeCharacter == null || orangeCharacter.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer)
				{
					continue;
				}
			}
			if ((buffId == 0 || (aimTarget2.BuffManager != null && aimTarget2.BuffManager.CheckHasEffectByCONDITIONID(buffId))) && (aimTarget == null || Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position) < num))
			{
				aimTarget = aimTarget2;
				num = Vector3.Distance(sourceTransform.position, aimTarget2.AimTransform.position);
			}
		}
		return aimTarget;
	}
}
