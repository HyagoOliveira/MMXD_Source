using System;
using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
	public struct Bound
	{
		public float Left;

		public float Right;

		public float Top;

		public float Bottom;

		public int Hit;

		public bool IsHitLeft
		{
			get
			{
				return (Hit & 1) != 0;
			}
		}

		public bool IsHitRight
		{
			get
			{
				return (Hit & 2) != 0;
			}
		}

		public bool IsHitTop
		{
			get
			{
				return (Hit & 4) != 0;
			}
		}

		public bool IsHitBottom
		{
			get
			{
				return (Hit & 8) != 0;
			}
		}

		public Bound(float Left, float Right, float Top, float Bottom)
		{
			this.Left = Left;
			this.Right = Right;
			this.Top = Top;
			this.Bottom = Bottom;
			Hit = 0;
		}
	}

	public readonly float MaxStickZ = -30f;

	public readonly float StickLimit = -999f;

	public readonly float StickRate = -5f;

	public readonly float DefaultStickZ = -30f;

	public float MinStickZ = -100f;

	[SerializeField]
	private Transform swivel;

	[SerializeField]
	private Transform stick;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private LeanTweenType moveType = LeanTweenType.easeInOutBack;

	private float cameraMoveTime = 1.5f;

	private bool isInit;

	private bool drag;

	private Vector3 origin;

	private Vector3 difference;

	private Bound bound = new Bound(2.1474836E+09f, -2.1474836E+09f, -2.1474836E+09f, 2.1474836E+09f);

	public Camera Camera
	{
		get
		{
			return _camera;
		}
	}

	public Bound _Bound
	{
		get
		{
			return bound;
		}
	}

	public void Init(Vector3 p_pos, float p_stickZ)
	{
		swivel.position = p_pos;
		MinStickZ = Mathf.Clamp(p_stickZ * StickRate, StickLimit, MaxStickZ);
		isInit = true;
	}

	public void ResetCameraPosition(Vector3 pos)
	{
		_camera.transform.localPosition = Vector3.zero;
	}

	public void Move(Vector3 pos)
	{
		_camera.transform.localPosition = Vector3.zero;
		swivel.position = pos;
	}

	public void MoveTo(Vector3 pos, Action p_cb)
	{
		_camera.transform.localPosition = Vector3.zero;
		LeanTween.move(swivel.gameObject, pos, cameraMoveTime).setEase(moveType).setOnComplete(p_cb);
	}

	private void LateUpdate()
	{
		if (!isInit)
		{
			return;
		}
		int touchCount = Input.touchCount;
		if (AllowZoom() && touchCount == 2)
		{
			Touch touch = Input.GetTouch(0);
			Touch touch2 = Input.GetTouch(1);
			Vector2 vector = touch.position - touch.deltaPosition;
			Vector2 vector2 = touch2.position - touch2.deltaPosition;
			float magnitude = (vector - vector2).magnitude;
			float num = ((touch.position - touch2.position).magnitude - magnitude) * 0.35f;
			float z = Mathf.Clamp(stick.localPosition.z + num, MinStickZ, MaxStickZ);
			stick.localPosition = new Vector3(stick.localPosition.x, stick.localPosition.y, z);
		}
		if (Input.GetMouseButton(0))
		{
			Vector3 position = new Vector3(0f - Input.mousePosition.x, 0f - Input.mousePosition.y, -30f);
			difference = _camera.ScreenToWorldPoint(position) - _camera.transform.position;
			if (!drag)
			{
				drag = true;
				origin = _camera.ScreenToWorldPoint(position);
			}
		}
		else
		{
			drag = false;
		}
		if (drag)
		{
			if (!AllowDrag())
			{
				return;
			}
			Vector3 position2 = origin - difference;
			if (position2.x <= bound.Left)
			{
				position2.x = bound.Left;
				bound.Hit |= 1;
			}
			else
			{
				bound.Hit &= -2;
			}
			if (position2.x >= bound.Right)
			{
				position2.x = bound.Right;
				bound.Hit |= 2;
			}
			else
			{
				bound.Hit &= -3;
			}
			if (position2.z >= bound.Top)
			{
				position2.z = bound.Top;
				bound.Hit |= 4;
			}
			else
			{
				bound.Hit &= -5;
			}
			if (position2.z <= bound.Bottom)
			{
				position2.z = bound.Bottom;
				bound.Hit |= 8;
			}
			else
			{
				bound.Hit &= -9;
			}
			_camera.transform.position = position2;
		}
		else
		{
			_camera.transform.position = new Vector3(Mathf.Clamp(_camera.transform.position.x, bound.Left, bound.Right), _camera.transform.position.y, Mathf.Clamp(_camera.transform.position.z, bound.Bottom, bound.Top));
		}
		if (AllowZoom() && Input.mouseScrollDelta.y != 0f)
		{
			float y = Input.mouseScrollDelta.y;
			float num2 = Mathf.Clamp(stick.localPosition.z + y, MinStickZ, MaxStickZ);
			if (num2 != stick.localPosition.z)
			{
				stick.localPosition = new Vector3(stick.localPosition.x, stick.localPosition.y, num2);
			}
		}
	}

	private bool AllowDrag()
	{
		DeepRecordMainUI mainUI = ManagedSingleton<DeepRecordHelper>.Instance.MainUI;
		if (MonoBehaviourSingleton<UIManager>.Instance.LastUI != mainUI)
		{
			return false;
		}
		if ((bool)mainUI)
		{
			return mainUI.AllowDrag;
		}
		return false;
	}

	private bool AllowZoom()
	{
		if (TurtorialUI.IsTutorialing())
		{
			return false;
		}
		return MonoBehaviourSingleton<UIManager>.Instance.LastUI == ManagedSingleton<DeepRecordHelper>.Instance.MainUI;
	}

	public void UpdateBound(Vector3 pos)
	{
		bound.Left = Mathf.Min(bound.Left, pos.x);
		bound.Right = Mathf.Max(bound.Right, pos.x);
		bound.Bottom = Mathf.Min(bound.Bottom, pos.z);
		bound.Top = Mathf.Max(bound.Top, pos.z);
	}
}
