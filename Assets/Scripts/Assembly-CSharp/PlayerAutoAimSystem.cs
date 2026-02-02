using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoAimSystem : MonoBehaviour
{
	public bool UpdateTarget;

	public bool PriorityTarget;

	[HideInInspector]
	public IAimTarget AutoAimTarget;

	private Transform _transform;

	private Vector3 _targetScreenPos = Vector2.zero;

	private List<IAimTarget> _targetList;

	private CircleCollider2D _circleCollider;

	private AimSystemItem _aimIconInstance;

	private float _range;

	private const float DeadZone = 0.5f;

	private Camera _mainCamera;

	private float _mouseZ;

	private Vector2 _rayPos;

	private Vector2 _screenResolution;

	private Vector2 _releativeResolution;

	private StageObjBase tLinkSOB;

	public bool Enabled;

	private int _bindSkillId;

	[HideInInspector]
	public LayerMask targetMask;

	private bool _instantiateIcon;

	public float Range
	{
		get
		{
			return _range;
		}
	}

	public int BindSkillId
	{
		get
		{
			return _bindSkillId;
		}
	}

	public void Init(bool p_instantiateIcon, bool bEnable)
	{
		tLinkSOB = GetComponentInParent<StageObjBase>();
		_transform = base.transform;
		_mainCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
		_mouseZ = 0f - _mainCamera.transform.position.z;
		_targetList = new List<IAimTarget>();
		_screenResolution = new Vector2(_mainCamera.pixelWidth, _mainCamera.pixelHeight);
		_releativeResolution = new Vector2(_screenResolution.x, _screenResolution.x * 0.75f);
		base.gameObject.AddOrGetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
		_circleCollider = base.gameObject.AddComponent<CircleCollider2D>();
		_circleCollider.offset = Vector2.zero;
		_circleCollider.isTrigger = true;
		Enabled = bEnable;
		if (p_instantiateIcon)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/aimicon2", "aimicon2", delegate(GameObject obj)
			{
				_aimIconInstance = Object.Instantiate(obj, Vector3.forward, Quaternion.identity).GetComponent<AimSystemItem>();
				_instantiateIcon = true;
			});
		}
	}

	private void SetTarget(Collider2D col, bool isPriority = false)
	{
		if (col == null)
		{
			SetTarget((IAimTarget)null, false);
			return;
		}
		IAimTarget componentInParent = col.gameObject.GetComponentInParent<IAimTarget>();
		SetTarget(componentInParent, isPriority);
	}

	private void SetTarget(IAimTarget target, bool isPriority = false)
	{
		if (target == null)
		{
			PriorityTarget = false;
			AutoAimTarget = null;
			if (_instantiateIcon)
			{
				_aimIconInstance.UpdateTargetAnim();
				_aimIconInstance.transform.SetParent(null);
				_aimIconInstance.transform.localPosition = Vector3.forward;
				_aimIconInstance.Show(false);
			}
		}
		else if (AutoAimTarget != target && target.Activate && target.AllowAutoAim && !(target.AimTransform == null) && (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp || IsInsideScreen(target.AimTransform.position + target.AimPoint)))
		{
			AutoAimTarget = target;
			if (isPriority)
			{
				PriorityTarget = true;
			}
			if (_instantiateIcon)
			{
				_aimIconInstance.UpdateTargetAnim();
				_aimIconInstance.transform.SetParent(AutoAimTarget.AimTransform);
				_aimIconInstance.transform.localPosition = AutoAimTarget.AimPoint;
				_aimIconInstance.Show(true);
			}
		}
	}

	private void ManualTarget()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = _mouseZ;
		_rayPos = _mainCamera.ScreenToWorldPoint(mousePosition);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(_rayPos, Vector2.zero, 0f, targetMask);
		if ((bool)raycastHit2D)
		{
			SetTarget(raycastHit2D.collider, true);
		}
	}

	public void RemovePlayer(OrangeCharacter tOC)
	{
		IAimTarget componentInParent = tOC.gameObject.GetComponentInParent<IAimTarget>();
		if (AutoAimTarget == componentInParent)
		{
			AutoAimTarget = null;
		}
		if (componentInParent != null && _targetList.Contains(componentInParent))
		{
			_targetList.Remove(componentInParent);
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (1 << col.gameObject.layer == ((1 << col.gameObject.layer) & (int)targetMask))
		{
			IAimTarget componentInParent = col.gameObject.GetComponentInParent<IAimTarget>();
			if (componentInParent != null && !_targetList.Contains(componentInParent))
			{
				_targetList.Add(componentInParent);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (1 << col.gameObject.layer == ((1 << col.gameObject.layer) & (int)targetMask))
		{
			IAimTarget componentInParent = col.gameObject.GetComponentInParent<IAimTarget>();
			if (componentInParent != null && _targetList.Contains(componentInParent))
			{
				_targetList.Remove(componentInParent);
			}
		}
	}

	private void Update()
	{
		if (!Enabled)
		{
			return;
		}
		if ((int)tLinkSOB.Hp <= 0)
		{
			IAimTarget target = null;
			SetTarget(target);
			return;
		}
		if (AutoAimTarget != null && (!IsInsideScreen(GetTargetPoint()) || !AutoAimTarget.Activate || !AutoAimTarget.AllowAutoAim))
		{
			AutoAimTarget = null;
			PriorityTarget = false;
		}
		if (!PriorityTarget && (UpdateTarget || AutoAimTarget == null))
		{
			IAimTarget closestTarget = GetClosestTarget();
			SetTarget(closestTarget);
		}
		if (_instantiateIcon)
		{
			_aimIconInstance.SetEnable(AutoAimTarget != null);
		}
		if (Input.GetMouseButtonDown(0))
		{
			ManualTarget();
		}
	}

	public IAimTarget GetClosestTarget()
	{
		float num = float.MaxValue;
		IAimTarget result = null;
		foreach (IAimTarget target in _targetList)
		{
			if (target.Activate && target.AllowAutoAim && (bool)target.AimTransform && (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp || IsInsideScreen(target.AimTransform.position + target.AimPoint)))
			{
				float num2 = Vector3.Distance(target.AimTransform.position + target.AimPoint, base.transform.position);
				if (num2 < num && target.AllowAutoAim)
				{
					num = num2;
					result = target;
				}
			}
		}
		return result;
	}

	public void UpdateAimRange(float val)
	{
		_range = Mathf.Max(1f, val - 0.5f);
		_circleCollider.radius = _range;
	}

	public Vector3 GetTargetPoint()
	{
		if (AutoAimTarget.AimTransform == null)
		{
			SetTarget((IAimTarget)null, false);
			return Vector3.zero;
		}
		return AutoAimTarget.AimPosition;
	}

	private void OnDestroy()
	{
		if ((bool)_aimIconInstance)
		{
			Object.Destroy(_aimIconInstance.gameObject);
		}
	}

	public bool IsInsideScreen(Vector3 targetWorldPosition)
	{
		_targetScreenPos = _mainCamera.WorldToScreenPoint(targetWorldPosition);
		if (!(_targetScreenPos.x < 0f) && !(_targetScreenPos.x > _releativeResolution.x) && !(_targetScreenPos.y < 0f))
		{
			return !(_targetScreenPos.y > _releativeResolution.y);
		}
		return false;
	}

	public bool IsInsideScreenExactly(Vector3 targetWorldPosition)
	{
		_targetScreenPos = _mainCamera.WorldToScreenPoint(targetWorldPosition);
		int pixelWidth = _mainCamera.pixelWidth;
		int pixelHeight = _mainCamera.pixelHeight;
		if (_targetScreenPos.x > 0f && _targetScreenPos.x < (float)pixelWidth && _targetScreenPos.y > 0f)
		{
			return _targetScreenPos.y < (float)pixelHeight;
		}
		return false;
	}

	public void SetUpdate(bool status)
	{
		if (UpdateTarget && !status)
		{
			PriorityTarget = false;
		}
		UpdateTarget = status;
	}

	public void SetEnable(bool enable)
	{
		Enabled = enable;
		AutoAimTarget = null;
	}

	public void BindSkill(int skillId)
	{
		_bindSkillId = skillId;
	}
}
