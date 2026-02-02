using System;
using System.Collections;
using UnityEngine;

public class BS102_WaterBullet : BasicBullet
{
	public enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3
	}

	private SubStatus _subStatus;

	[SerializeField]
	protected VInt _gravity = OrangeBattleUtility.FP_Gravity;

	public int _direction;

	public int _upDirection;

	[SerializeField]
	protected float _springValue = 9.6f;

	[SerializeField]
	private int MaxJumpTime = 3;

	private int _jumpCount;

	private int _hitCount;

	private float _hitFrame;

	private float SpeedMultiplier = 1f;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void OnStart()
	{
		base.OnStart();
		_subStatus = SubStatus.Phase0;
		_jumpCount = 0;
		_hitCount = 0;
		_hitFrame = 0f;
		UpdateDirection();
	}

	public bool IsIdle()
	{
		if (Phase == BulletPhase.Normal && _subStatus == SubStatus.Phase0)
		{
			return true;
		}
		return false;
	}

	public void Shoot()
	{
		if (Phase == BulletPhase.Normal && _subStatus == SubStatus.Phase0)
		{
			base.SoundSource.PlaySE("BossSE04", "bs036_mrmammo05");
			_subStatus = SubStatus.Phase1;
		}
	}

	public void UpdateDirection()
	{
		_direction = ((Direction.x > 0f) ? 1 : (-1));
		_upDirection = ((Direction.x > 0f) ? 1 : (-1));
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
		switch (Phase)
		{
		case BulletPhase.Normal:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			case SubStatus.Phase1:
			{
				if (_hitCount > 0)
				{
					_hitFrame += Time.deltaTime;
				}
				if ((bool)Physics2D.Raycast(_transform.position, Vector2.right * _direction, DefaultRadiusX + 0.3f, BlockMask))
				{
					_direction *= -1;
					Velocity.x *= -1f;
					base.SoundSource.PlaySE("BossSE04", "bs036_mrmammo02");
				}
				RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.down, DefaultRadiusY + 0.3f, BlockMask);
				if ((bool)raycastHit2D)
				{
					_jumpCount++;
					if (_jumpCount <= MaxJumpTime)
					{
						if (Velocity.x >= 0f)
						{
							Velocity.x = (float)BulletData.n_SPEED * SpeedMultiplier;
						}
						else
						{
							Velocity.x = (float)(-BulletData.n_SPEED) * SpeedMultiplier;
						}
						Velocity.y = _springValue * (float)_upDirection;
						if (HitBlockCallback != null)
						{
							HitBlockCallback(_transform.position);
						}
					}
					FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxhit_seahorse_hit_ground_m", base.transform.position - Vector3.up * raycastHit2D.distance, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					if ((bool)fxBase)
					{
						ParticleSystem.MainModule main = fxBase.GetComponentInChildren<ParticleSystem>().main;
						main.simulationSpeed = 2f;
					}
					base.SoundSource.PlaySE("BossSE04", "bs036_mrmammo02");
				}
				Velocity.y += (float)(_gravity.i * _upDirection) * 0.001f * Time.deltaTime;
				MyMoveBullet();
				break;
			}
			case SubStatus.Phase2:
				_subStatus = SubStatus.Phase1;
				if ((bool)Physics2D.Raycast(_transform.position, Vector2.right * _direction, DefaultRadiusX + 0.3f, BlockMask))
				{
					_direction *= -1;
					Velocity.x *= -1f;
					base.SoundSource.PlaySE("BossSE04", "bs036_mrmammo02");
				}
				Velocity.y += (float)(_gravity.i * _upDirection) * 0.001f * Time.deltaTime;
				MyMoveBullet();
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
		if (((1 << col.gameObject.layer) & (int)TargetMask) != 0)
		{
			if (_hitCount == 0)
			{
				_hitCount++;
				_hitFrame = 0f;
			}
			else if (_hitFrame > 0.5f)
			{
				_hitCount++;
				_hitList.Remove(col.transform);
			}
		}
		base.Hit(col);
		if (isHitBlock && (bool)Physics2D.Raycast(_transform.position, Vector2.down, DefaultRadiusY + 0.3f, BlockMask) && HitBlockCallback != null)
		{
			HitBlockCallback(_transform.position);
		}
		if ((isHitBlock && _jumpCount > MaxJumpTime) || _hitCount >= BulletData.n_DAMAGE_COUNT)
		{
			Phase = BulletPhase.BackToPool;
		}
	}

	public override void BackToPool()
	{
		_subStatus = SubStatus.Phase0;
		_jumpCount = 0;
		_hitCount = 0;
		_hitFrame = 0f;
		base.BackToPool();
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	public void SetSpeedMultiplier(float multip)
	{
		SpeedMultiplier = multip;
	}
}
