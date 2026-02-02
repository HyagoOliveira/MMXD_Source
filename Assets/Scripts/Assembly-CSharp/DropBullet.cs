using System;
using System.Collections;
using UnityEngine;

public class DropBullet : BasicBullet
{
	[SerializeField]
	protected VInt _gravity = OrangeBattleUtility.FP_Gravity;

	protected Vector3 _speed = Vector3.zero;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void OnStart()
	{
		base.OnStart();
		if (_speed == Vector3.zero)
		{
			Velocity = BulletData.n_SPEED * Direction;
		}
		else
		{
			Velocity = _speed;
		}
	}

	public void SetSpeed(Vector3 spd)
	{
		_speed = spd;
	}

	public void SetVelocity(Vector3 spd)
	{
		Velocity = spd;
	}

	public override void LateUpdateFunc()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		switch (Phase)
		{
		case BulletPhase.Normal:
		{
			Velocity.y += (float)_gravity.i * 0.001f * Time.deltaTime;
			oldPos = _transform.position;
			Vector3 translation = Velocity * Time.deltaTime;
			offset = Math.Max(translation.magnitude, DefaultRadiusX * 2f);
			_transform.Translate(translation);
			if (checkLoopSE)
			{
				PlayUseSE();
				checkLoopSE = false;
			}
			float num = BulletData.f_DISTANCE;
			if (FreeDISTANCE > 0f)
			{
				num = FreeDISTANCE;
			}
			if (BulletData.n_SHOTLINE == 6)
			{
				heapDistance = lineDistance;
			}
			else if (BulletData.n_SHOTLINE == 3 || BulletData.n_SHOTLINE == 4)
			{
				heapDistance = lineDistance / 0.9125f;
			}
			else
			{
				heapDistance += Vector2.Distance(lastPosition, _transform.position);
			}
			lastPosition = _transform.position;
			if (heapDistance > num)
			{
				CheckRollBack();
			}
			break;
		}
		case BulletPhase.Result:
			if (BulletData.n_THROUGH == 0)
			{
				foreach (Transform hit in _hitList)
				{
					CaluDmg(BulletData, hit);
					if (nThrough > 0)
					{
						nThrough--;
					}
				}
			}
			if (BulletData.n_TYPE != 3)
			{
				if (BulletData.f_RANGE != 0f)
				{
					GenerateImpactFx();
				}
				else
				{
					GenerateEndFx();
				}
			}
			Phase = BulletPhase.BackToPool;
			break;
		case BulletPhase.Splash:
			PhaseSplash();
			break;
		case BulletPhase.BackToPool:
			Stop();
			BackToPool();
			break;
		case BulletPhase.Boomerang:
			break;
		}
	}

	public override void Hit(Collider2D col)
	{
		base.Hit(col);
	}

	public override void BackToPool()
	{
		base.BackToPool();
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	protected override void GenerateImpactFx(bool bPlaySE = true)
	{
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(oldPos, Direction, offset, UseMask);
		if ((bool)raycastHit2D)
		{
			_transform.position = raycastHit2D.point;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, raycastHit2D.point, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position, quaternion * BulletQuaternion, Array.Empty<object>());
		}
		if (isHitBlock || needPlayEndSE || needWeaponImpactSE)
		{
			PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
		}
	}
}
