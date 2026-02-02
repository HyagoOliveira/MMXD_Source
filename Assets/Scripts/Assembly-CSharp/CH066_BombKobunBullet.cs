using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class CH066_BombKobunBullet : thunderboltBullet
{
	[SerializeField]
	protected Transform ModelTransform;

	[SerializeField]
	protected Animator _animator;

	protected int[] _animationHash;

	[SerializeField]
	protected GameObject[] Emotions;

	[SerializeField]
	protected GameObject[] Weapons;

	protected float _moveDistance;

	[SerializeField]
	protected bool allowRollingBack;

	protected int twoSideTouchCount;

	protected int twoSideTouchLimit = 6;

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		HashAnimation();
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[1];
		_animationHash[0] = Animator.StringToHash("BS067@run_loop");
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Hp = MaxHp;
		MasterTransform = pTransform;
		_transform.position = pTransform.position;
		_controller.LogicPosition = new VInt3(_transform.localPosition);
		Direction = pDirection;
		Velocity = BulletData.n_SPEED * pDirection;
		UpdateParticleRotationDirection(Velocity.x > 0f);
		_moveDistance = 0f;
		TargetMask = pTargetMask;
		UseMask = (int)BlockMask | (int)pTargetMask;
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(bulletFxArray, true);
		SetAnimation();
		SetEmotions(4);
		SetWeapon(0);
		if (CoroutineMove != null)
		{
			StopCoroutine(CoroutineMove);
		}
		CoroutineMove = StartCoroutine(OnStartMove());
		base.SoundSource.UpdateDistanceCall();
		PlayUseSE();
		needWeaponImpactSE = true;
	}

	protected void SetAnimation()
	{
		_animator.Play(_animationHash[0], 0, 0f);
	}

	protected void SetEmotions(int type)
	{
		for (int i = 0; i < Emotions.Length; i++)
		{
			if (i == type)
			{
				Emotions[i].SetActive(true);
			}
			else
			{
				Emotions[i].SetActive(false);
			}
		}
	}

	protected void SetWeapon(int type)
	{
		for (int i = 0; i < Weapons.Length; i++)
		{
			if (i == type)
			{
				Weapons[i].SetActive(true);
			}
			else
			{
				Weapons[i].SetActive(false);
			}
		}
	}

	protected override IEnumerator OnStartMove()
	{
		_capsuleCollider.enabled = true;
		_capsuleCollider.direction = CapsuleDirection2D.Vertical;
		_capsuleCollider.size = new Vector2(DefaultRadiusX * 2f, DefaultRadiusY * 2f);
		if (BulletData.n_THROUGH > 0)
		{
			nThrough = BulletData.n_THROUGH / 100;
		}
		else
		{
			nThrough = 0;
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
						UpdateParticleRotationDirection(Velocity.x > 0f);
						_hitList.Clear();
					}
					else
					{
						_moveDistance = BulletData.f_DISTANCE + 1f;
					}
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

	protected void UpdateParticleRotationDirection(bool isRight)
	{
		if (isRight)
		{
			_transform.eulerAngles = Vector3.zero;
			_transform.localScale = Vector3.one;
		}
		else
		{
			_transform.eulerAngles = new Vector3(0f, 180f, 0f);
			_transform.localScale = new Vector3(1f, 1f, -1f);
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

	public override void BackToPool()
	{
		_controller.Collisions.left = (_controller.Collisions.right = false);
		_controller.Collisions.below = (_controller.Collisions.above = false);
		twoSideTouchCount = 0;
		base.BackToPool();
	}
}
