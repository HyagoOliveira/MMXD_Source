using System;
using System.Collections;
using UnityEngine;

public class SeaHorseSkill04Bullet : BasicBullet
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

	public OrangeCharacter _myTarget;

	public bool _divisionFlag;

	public int _direction;

	public int _upDirection;

	[SerializeField]
	protected float _springValue = 9.6f;

	private int _jumpCount;

	private int _hitCount;

	private float _hitFrame;

	[SerializeField]
	protected string HitBlockFX = "fxhit_seahorse_hit_ground_m";

	protected override void Awake()
	{
		base.Awake();
	}

	public override void OnStart()
	{
		base.OnStart();
		_subStatus = SubStatus.Phase0;
		_myTarget = null;
		_jumpCount = 0;
		_hitCount = 0;
		_hitFrame = 0f;
		_divisionFlag = false;
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
		_myTarget = OrangeBattleUtility.GetRandomPlayer();
		if (Phase == BulletPhase.Normal && _subStatus == SubStatus.Phase0)
		{
			base.SoundSource.PlaySE("BossSE02", "bs013_toxic06");
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
				UpdateDirection();
				break;
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
					base.SoundSource.PlaySE("BossSE02", "bs013_toxic07");
				}
				RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.down, DefaultRadiusY + 0.3f, BlockMask);
				if ((bool)raycastHit2D)
				{
					_jumpCount++;
					if (_jumpCount <= 12)
					{
						if (Velocity.x >= 0f)
						{
							Velocity.x = BulletData.n_SPEED;
						}
						else
						{
							Velocity.x = -BulletData.n_SPEED;
						}
						Velocity.y = _springValue * (float)_upDirection;
					}
					FxBase fxBase = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(HitBlockFX, base.transform.position - Vector3.up * raycastHit2D.distance, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					if ((bool)fxBase)
					{
						ParticleSystem.MainModule main = fxBase.GetComponentInChildren<ParticleSystem>().main;
						main.simulationSpeed = 2f;
					}
					base.SoundSource.PlaySE("BossSE02", "bs013_toxic07");
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
					base.SoundSource.PlaySE("BossSE02", "bs013_toxic07");
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
			else if (!_divisionFlag && _hitFrame > 0.5f)
			{
				_hitCount++;
				_hitList.Remove(col.transform);
			}
		}
		base.Hit(col);
		if ((isHitBlock && _jumpCount > 8) || _hitCount >= BulletData.n_DAMAGE_COUNT || _divisionFlag)
		{
			Phase = BulletPhase.BackToPool;
		}
	}

	public override void BackToPool()
	{
		_subStatus = SubStatus.Phase0;
		_myTarget = null;
		_jumpCount = 0;
		_hitCount = 0;
		_hitFrame = 0f;
		_divisionFlag = false;
		base.BackToPool();
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	protected void DivideBullet()
	{
		_divisionFlag = true;
		SeaHorseSkill04Bullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SeaHorseSkill04Bullet>(BulletData.s_MODEL);
		poolObj.UpdateBulletData(BulletData, Owner);
		poolObj.BulletLevel = BulletLevel;
		poolObj.fMissFactor = fMissFactor;
		poolObj.refPBMShoter = refPBMShoter;
		poolObj.refPSShoter = refPSShoter;
		poolObj.isSubBullet = true;
		poolObj.HitCount = HitCount;
		poolObj.Active(base.transform, Direction, TargetMask);
		poolObj._subStatus = SubStatus.Phase2;
		poolObj.UpdateDirection();
		poolObj._divisionFlag = true;
		poolObj._myTarget = _myTarget;
		poolObj._direction = _direction * -1;
		poolObj.Velocity.x = Velocity.x * 0.5f * -1f;
		poolObj.Velocity.y = _springValue * (float)_upDirection;
	}
}
