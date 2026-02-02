using System;
using UnityEngine;

public class CH048_BeamSectorCollider : MonoBehaviour
{
	[SerializeField]
	private float _sectorAngle = 5f;

	public BeamBullet _mainBullet;

	protected Rigidbody2D _rigidbody2D;

	protected CircleCollider2D _circleCollider2D;

	protected float _startAngle;

	protected float _endAngle;

	private void Awake()
	{
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		_circleCollider2D = base.gameObject.AddOrGetComponent<CircleCollider2D>();
		_circleCollider2D.isTrigger = true;
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
		if (!(_mainBullet == null) && !(col == null) && _mainBullet.IsActivate && !col.isTrigger && IsInsideOfSector(col))
		{
			_mainBullet.OnTriggerHit(col);
		}
	}

	public void Active(BeamBullet beam, float radius)
	{
		_mainBullet = beam;
		_circleCollider2D.radius = radius;
		_circleCollider2D.enabled = true;
	}

	public void Disable()
	{
		_startAngle = 0f;
		_endAngle = 0f;
		_circleCollider2D.enabled = false;
	}

	public void UpdateAngle(float start, float end)
	{
		_startAngle = ClampAngle(start);
		_endAngle = ClampAngle(end);
	}

	protected bool IsInsideOfSector(Collider2D col)
	{
		if (Mathf.Abs(_startAngle - _endAngle) < _sectorAngle)
		{
			return false;
		}
		Vector2 b = (col.transform.position - base.transform.position).xy();
		float f = _startAngle * ((float)Math.PI / 180f);
		float f2 = _endAngle * ((float)Math.PI / 180f);
		Vector2 a = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		Vector2 vector = new Vector2(Mathf.Cos(f2), Mathf.Sin(f2));
		if (GetCross2d(a, vector) > 0f)
		{
			if (GetCross2d(a, b) >= 0f && GetCross2d(vector, b) <= 0f)
			{
				return true;
			}
		}
		else if (GetCross2d(a, b) >= 0f || GetCross2d(vector, b) <= 0f)
		{
			return true;
		}
		return false;
	}

	protected float GetCross2d(Vector2 a, Vector2 b)
	{
		return GetCross2d(a.x, a.y, b.x, b.y);
	}

	protected float GetCross2d(float ax, float ay, float bx, float by)
	{
		return ax * by - bx * ay;
	}

	protected float ClampAngle(float eulerAngles)
	{
		float num = eulerAngles - (float)Mathf.CeilToInt(eulerAngles / 360f) * 360f;
		if (num < 0f)
		{
			num += 360f;
		}
		return num;
	}
}
