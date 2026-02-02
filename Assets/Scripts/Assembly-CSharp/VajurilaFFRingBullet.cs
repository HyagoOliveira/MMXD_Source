using System.Collections;
using UnityEngine;

public class VajurilaFFRingBullet : BasicBullet
{
	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3
	}

	private SubStatus _subStatus;

	private int _direction;

	private int _turnCount;

	private StageObjBase _owner;

	public void SetOwner(StageObjBase owner)
	{
		_owner = owner;
	}

	public void Shoot()
	{
		if (Phase == BulletPhase.Normal && _subStatus == SubStatus.Phase0)
		{
			_subStatus = SubStatus.Phase1;
		}
	}

	private void MyMoveBullet()
	{
		oldPos = _transform.position;
		Vector3 translation = Velocity * Time.deltaTime;
		_transform.Translate(translation);
		if (BulletData.n_SHOTLINE == 3 || BulletData.n_SHOTLINE == 4)
		{
			_transform.position += base.transform.up * amplitude * Mathf.Sin(omega * Time.fixedTime) * Time.timeScale;
		}
	}

	public override void LateUpdateFunc()
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
		{
			return;
		}
		if (_owner == null || !_owner.Activate)
		{
			Phase = BulletPhase.BackToPool;
			Stop();
			BackToPool();
			return;
		}
		switch (Phase)
		{
		case BulletPhase.Normal:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				_direction = ((Direction.x > 0f) ? 1 : (-1));
				if ((bool)Physics2D.Raycast(_transform.position, Vector2.right * _direction, DefaultRadiusX + 0.3f, BlockMask))
				{
					_subStatus = SubStatus.Phase2;
					_turnCount++;
					Velocity = new Vector3(0f, BulletData.n_SPEED * -_direction, 0f);
				}
				MyMoveBullet();
				break;
			case SubStatus.Phase2:
				if ((bool)Physics2D.Raycast(_transform.position, Vector2.down, DefaultRadiusY + 0.3f, BlockMask))
				{
					_subStatus = SubStatus.Phase3;
					_turnCount++;
					_direction *= -1;
					Velocity = new Vector3(-BulletData.n_SPEED, 0f, 0f);
				}
				MyMoveBullet();
				break;
			case SubStatus.Phase3:
				if (_turnCount < 3 && (bool)Physics2D.Raycast(_transform.position, Vector2.right * _direction, DefaultRadiusX + 0.3f, BlockMask))
				{
					_turnCount++;
					if (Velocity.x > 0f)
					{
						Velocity = new Vector3(-BulletData.n_SPEED, 0f, 0f);
					}
					else
					{
						Velocity = new Vector3(BulletData.n_SPEED, 0f, 0f);
					}
				}
				MyMoveBullet();
				break;
			case SubStatus.Phase0:
				break;
			}
			break;
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
				if (_hitList.Count != 0 || BulletData.f_RANGE != 0f)
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
		case BulletPhase.BackToPool:
			Stop();
			BackToPool();
			break;
		case BulletPhase.Splash:
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
		_subStatus = SubStatus.Phase0;
		_turnCount = 0;
		_owner = null;
		base.BackToPool();
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}
}
