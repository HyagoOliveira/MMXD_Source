using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class EM005_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private ParticleSystem _iceEffect;

	private OrangeTimer _destroyTimer;

	private BS031_Controller _parentController;

	private BS108_Controller _parentController2;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Collider").gameObject.AddOrGetComponent<CollideBullet>();
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "AimTransform");
		_iceEffect = OrangeBattleUtility.FindChildRecursive(ref target, "Root").GetComponent<ParticleSystem>();
		_destroyTimer = OrangeTimerManager.GetTimer();
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		if (((_velocity + _velocityExtra + OrangeBattleUtility.GlobalVelocityExtra).x > 0 && Controller.Collisions.right) || ((_velocity + _velocityExtra + OrangeBattleUtility.GlobalVelocityExtra).x < 0 && Controller.Collisions.left))
		{
			Hp = 0;
			Hurt(new HurtPassParam());
		}
		else if (_parentController != null && _parentController.IsSliding())
		{
			if (Controller.Collider2D.bounds.Intersects(_parentController.Controller.Collider2D.bounds))
			{
				Hp = 0;
				Hurt(new HurtPassParam());
			}
		}
		else if (_parentController2 != null && _parentController2.IsSliding() && Controller.Collider2D.bounds.Intersects(_parentController2.Controller.Collider2D.bounds))
		{
			Hp = 0;
			Hurt(new HurtPassParam());
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_destroyTimer.TimerStart();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_iceEffect.Play(true);
		}
		else
		{
			_destroyTimer.TimerStop();
			_collideBullet.BackToPool();
			_iceEffect.Stop(true);
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if ((int)obscuredInt <= 0)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_penguin_000", base.AimTransform.position, _transform.rotation, Array.Empty<object>());
			PlaySE("BossSE", 48);
		}
		return obscuredInt;
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		base.transform.localScale = new Vector3(_transform.localScale.x * (float)base.direction, _transform.localScale.y, _transform.localScale.z);
		base.transform.position = pos;
	}

	public void SetParentPenguin(BS031_Controller parent)
	{
		_parentController = parent;
	}

	public void SetParentPenguin2(BS108_Controller parent)
	{
		_parentController2 = parent;
	}
}
