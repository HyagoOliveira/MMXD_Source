using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class thunderboltBullet : BasicBullet
{
	[SerializeField]
	protected Vector3 eulerAngles = new Vector3(0f, 0f, 0f);

	protected Controller2D _controller;

	protected void UpdateGravity()
	{
		if ((Velocity.y < 0f && _controller.Collisions.below) || (Velocity.y > 0f && _controller.Collisions.above))
		{
			Velocity.y = 0f;
		}
		Velocity.y += OrangeBattleUtility.Gravity * Time.deltaTime;
		Velocity.y = Mathf.Sign(Velocity.y) * Mathf.Min(Mathf.Abs(Velocity.y), Mathf.Abs(OrangeBattleUtility.MaxGravity));
	}

	protected override void Awake()
	{
		base.Awake();
		_controller = GetComponent<Controller2D>();
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Hp = MaxHp;
		MasterTransform = pTransform;
		float num = Vector2.SignedAngle(Vector2.right, pDirection.normalized);
		_transform.eulerAngles = new Vector3(eulerAngles.x * num, eulerAngles.y * num, eulerAngles.z * num);
		_transform.position = pTransform.position;
		_controller.LogicPosition = new VInt3(_transform.localPosition);
		Direction = pDirection;
		Velocity = BulletData.n_SPEED * pDirection;
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

	protected override IEnumerator OnStartMove()
	{
		_capsuleCollider.enabled = true;
		_capsuleCollider.size = Vector2.one * DefaultRadiusX * 2f;
		Vector3 beginPosition = _transform.position;
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
				_controller.Move(Velocity * Time.deltaTime);
				bool flag = ((Velocity.x > 0f) ? _controller.Collisions.right : _controller.Collisions.left);
				if (Mathf.Abs(Vector3.Distance(beginPosition, _transform.position)) > BulletData.f_DISTANCE || flag)
				{
					if (BulletData.f_RANGE == 0f)
					{
						Phase = BulletPhase.Result;
						break;
					}
					_capsuleCollider.size = Vector2.one * BulletData.f_RANGE * 2f;
					Phase = BulletPhase.Splash;
				}
				break;
			}
			case BulletPhase.Splash:
				yield return CoroutineDefine._waitForEndOfFrame;
				yield return CoroutineDefine._waitForEndOfFrame;
				Phase = BulletPhase.Result;
				break;
			case BulletPhase.Result:
			{
				foreach (Transform hit in _hitList)
				{
					CaluDmg(BulletData, hit);
				}
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.back, Direction);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxImpact, _transform.position.xy(), quaternion * BulletQuaternion, Array.Empty<object>());
				if (isHitBlock || needWeaponImpactSE)
				{
					PlaySE(_HitGuardSE[0], _HitGuardSE[1], isForceSE);
				}
				Phase = BulletPhase.BackToPool;
				break;
			}
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		while (Phase != BulletPhase.BackToPool);
		Stop();
		BackToPool();
	}

	protected override void OnTriggerEnter2D(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !col.isTrigger && ((1 << col.gameObject.layer) & (int)UseMask) != 0)
		{
			StageObjParam component = col.GetComponent<StageObjParam>();
			if (component != null && component.tLinkSOB != null && (int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
	}

	protected override void OnTriggerStay2D(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !col.isTrigger && ((1 << col.gameObject.layer) & (int)UseMask) != 0)
		{
			StageObjParam component = col.GetComponent<StageObjParam>();
			if (component != null && component.tLinkSOB != null && (int)component.tLinkSOB.Hp > 0)
			{
				Hit(col);
			}
		}
	}
}
