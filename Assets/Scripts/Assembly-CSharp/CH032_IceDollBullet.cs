using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class CH032_IceDollBullet : thunderboltBullet
{
	private float _moveDistance;

	[SerializeField]
	private ParticleSystem[] rollingPS;

	[SerializeField]
	private bool allowRollingBack = true;

	protected int twoSideTouchCount;

	protected int twoSideTouchLimit = 6;

	[SerializeField]
	protected float _springValue = 4f;

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Hp = MaxHp;
		MasterTransform = pTransform;
		_transform.eulerAngles = new Vector3(0f, Vector2.SignedAngle(Vector2.right, pDirection), 0f);
		_transform.position = pTransform.position;
		_controller.LogicPosition = new VInt3(_transform.localPosition);
		Direction = pDirection;
		Velocity = BulletData.n_SPEED * pDirection;
		UpdateParticleRotationDirection(Velocity.x > 0f);
		_moveDistance = 0f;
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
		}
		CoroutineMove = StartCoroutine(OnStartMove());
		base.transform.position = pTransform.position;
		base.SoundSource.UpdateDistanceCall();
		PlayUseSE();
		needWeaponImpactSE = true;
	}

	public override void BackToPool()
	{
		_controller.Collisions.left = (_controller.Collisions.right = false);
		_controller.Collisions.below = (_controller.Collisions.above = false);
		twoSideTouchCount = 0;
		base.BackToPool();
	}

	protected override IEnumerator OnStartMove()
	{
		_capsuleCollider.enabled = true;
		_capsuleCollider.size = Vector2.one * DefaultRadiusX * 2f;
		if (BulletData.n_THROUGH > 0)
		{
			nThrough = BulletData.n_THROUGH / 100;
		}
		Vector3 position = _transform.position;
		if ((bool)MasterTransform && !string.IsNullOrEmpty(FxMuzzleFlare))
		{
			if (BulletData.n_USE_FX_FOLLOW == 0)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform.position, _transform.rotation * BulletQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxMuzzleFlare, MasterTransform, BulletQuaternion, Array.Empty<object>());
			}
		}
		do
		{
			if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				continue;
			}
			switch (Phase)
			{
			case BulletPhase.Normal:
			{
				UpdateGravity();
				oldPos = _transform.position;
				bool num = ((Velocity.x > 0f) ? _controller.Collisions.left : _controller.Collisions.right);
				_moveDistance += _controller.Move(Velocity * Time.deltaTime);
				bool flag = ((Velocity.x > 0f) ? _controller.Collisions.right : _controller.Collisions.left);
				if (flag)
				{
					if (allowRollingBack)
					{
						Velocity.x *= -1f;
						Direction.x *= -1f;
						_transform.eulerAngles = new Vector3(0f, Vector2.SignedAngle(Vector2.right, Direction), 0f);
						UpdateParticleRotationDirection(Velocity.x > 0f);
						_hitList.Clear();
						PlaySE("SkillSE_CH032_000", "ch032_icedoll01");
					}
					else
					{
						_moveDistance = BulletData.f_DISTANCE + 1f;
					}
				}
				if (_controller.Collisions.below || _controller.Collisions.JSB_below)
				{
					PlaySE("SkillSE_CH032_000", "ch032_icedoll01");
					Velocity.y = _springValue;
				}
				if (num && flag)
				{
					twoSideTouchCount++;
					if (twoSideTouchCount > twoSideTouchLimit)
					{
						_moveDistance = BulletData.f_DISTANCE + 1f;
					}
				}
				else
				{
					twoSideTouchCount = 0;
				}
				if (_moveDistance > BulletData.f_DISTANCE)
				{
					if (BulletData.f_RANGE == 0f)
					{
						Phase = BulletPhase.Result;
					}
					else
					{
						SetPhaseToSplash();
					}
				}
				break;
			}
			case BulletPhase.Splash:
				PhaseSplash();
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
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (Phase != BulletPhase.BackToPool);
		Stop();
		BackToPool();
	}

	private void UpdateParticleRotationDirection(bool isRight)
	{
		float flipRotation = ((!isRight) ? 1 : 0);
		for (int i = 0; i < rollingPS.Length; i++)
		{
			ParticleSystem.MainModule main = rollingPS[i].main;
			main.flipRotation = flipRotation;
		}
	}

	protected override void OnTriggerEnter2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0)
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else if (_controller.UseIgnoreHurtObject && IsStageHurtObject(col))
		{
			Hit(col);
		}
	}

	protected override void OnTriggerStay2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause || col.isTrigger || ((1 << col.gameObject.layer) & (int)UseMask) == 0)
		{
			return;
		}
		StageObjParam component = col.GetComponent<StageObjParam>();
		if (component != null && component.tLinkSOB != null)
		{
			if ((int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
		else if (_controller.UseIgnoreHurtObject && IsStageHurtObject(col))
		{
			Hit(col);
		}
	}

	protected new bool IsStageHurtObject(Collider2D collider)
	{
		if ((bool)collider)
		{
			if (collider.GetComponent<StageHurtObj>() != null)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
