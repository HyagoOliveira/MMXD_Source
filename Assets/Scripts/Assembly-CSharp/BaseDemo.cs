using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class BaseDemo : MonoBehaviour
{
	private readonly List<GameObject> _dragTargets = new List<GameObject>();

	protected bool _isCreateBackground = true;

	protected bool _isTouched;

	private GameObject _currentDragTarget;

	private Vector3 _startDragWorldPosition;

	private Vector3 _startDragScreenPosition;

	private Vector3 _currentDragWorldPosition;

	private Vector3 _dragOffset;

	private void Start()
	{
		if (_isCreateBackground)
		{
			CreateBackground();
		}
		OnStart();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			StartCoroutine("DragDelay");
			_currentDragTarget = GetClickTarget();
			if (_currentDragTarget != null)
			{
				_startDragWorldPosition = _currentDragTarget.transform.localPosition;
				_startDragScreenPosition = Camera.main.WorldToScreenPoint(_currentDragTarget.transform.localPosition);
				_dragOffset = _currentDragTarget.transform.localPosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _startDragScreenPosition.z));
				UnityArmatureComponent component = _currentDragTarget.transform.parent.GetComponent<UnityArmatureComponent>();
				if (component != null)
				{
					Bone boneByDisplay = component.armature.GetBoneByDisplay(_currentDragTarget);
					if (boneByDisplay != null && boneByDisplay.offsetMode != OffsetMode.Override)
					{
						boneByDisplay.offsetMode = OffsetMode.Override;
						boneByDisplay.offset.x = boneByDisplay.global.x;
						boneByDisplay.offset.y = boneByDisplay.global.y;
					}
				}
				else
				{
					_currentDragTarget.transform.localPosition = _currentDragWorldPosition;
				}
			}
			OnTouch(TouchType.TOUCH_BEGIN);
		}
		if (Input.GetMouseButtonUp(0))
		{
			StopCoroutine("DragDelay");
			_isTouched = false;
			_currentDragTarget = null;
			_startDragWorldPosition = Vector3.zero;
			_currentDragWorldPosition = Vector3.zero;
			OnTouch(TouchType.TOUCH_END);
		}
		if (_isTouched)
		{
			if (_currentDragTarget != null)
			{
				Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _startDragScreenPosition.z);
				_currentDragWorldPosition = Camera.main.ScreenToWorldPoint(position) + _dragOffset;
				_currentDragWorldPosition.z = _currentDragTarget.transform.localPosition.z;
				UnityArmatureComponent component2 = _currentDragTarget.transform.parent.GetComponent<UnityArmatureComponent>();
				if (component2 != null)
				{
					Bone boneByDisplay2 = component2.armature.GetBoneByDisplay(_currentDragTarget);
					if (boneByDisplay2 != null)
					{
						Vector3 vector = component2.transform.InverseTransformPoint(_currentDragWorldPosition);
						boneByDisplay2.offset.x = vector.x;
						boneByDisplay2.offset.y = 0f - vector.y;
						boneByDisplay2.InvalidUpdate();
					}
				}
				else
				{
					_currentDragTarget.transform.localPosition = _currentDragWorldPosition;
				}
				OnDrag(_currentDragTarget, _startDragWorldPosition, _currentDragWorldPosition);
			}
			OnTouch(TouchType.TOUCH_MOVE);
		}
		OnUpdate();
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnTouch(TouchType type)
	{
	}

	protected virtual void OnDrag(GameObject target, Vector3 startDragPos, Vector3 currentDragPos)
	{
	}

	protected void EnableDrag(GameObject target)
	{
		if (!_dragTargets.Contains(target))
		{
			_dragTargets.Add(target);
			if (target.GetComponent<BoxCollider>() == null)
			{
				target.AddComponent<BoxCollider>();
			}
		}
	}

	protected void DisableDrag(GameObject target)
	{
		if (_dragTargets.Contains(target))
		{
			_dragTargets.Remove(target);
			BoxCollider component = target.GetComponent<BoxCollider>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
	}

	private void CreateBackground()
	{
		GameObject obj = new GameObject("Background");
		obj.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("background");
		obj.transform.localPosition = new Vector3(0f, 0f, 1f);
		obj.transform.SetSiblingIndex(base.transform.GetSiblingIndex() + 1);
	}

	private GameObject GetClickTarget()
	{
		if (_dragTargets.Count == 0)
		{
			return null;
		}
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		if (Physics.Raycast(ray.origin, ray.direction * 10f, out hitInfo))
		{
			foreach (GameObject dragTarget in _dragTargets)
			{
				if (dragTarget == hitInfo.collider.gameObject)
				{
					return dragTarget;
				}
			}
		}
		return null;
	}

	private IEnumerator DragDelay()
	{
		yield return new WaitForSeconds(0.16f);
		_isTouched = true;
	}
}
