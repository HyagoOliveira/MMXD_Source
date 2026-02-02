using System;
using System.Collections;
using UnityEngine;

public abstract class CharacterControllerProxyBaseGen2 : CharacterControllerProxyBaseGen1
{
	private Coroutine _hitPauseCoroutine;

	private VInt3 _lastPosition;

	private VInt3 _lastVelocity;

	protected bool IsHitPauseStarted { get; private set; }

	protected event Action<bool> OnHitPauseStateChangedEvent;

	protected override void SetSkillEnd()
	{
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.IgnoreGravity = false;
		PlayerStopDashing();
		SetSkillAndWeapon(SkillID.NONE);
		_refEntity.BulletCollider.BackToPool();
		if (_refEntity.IsInGround)
		{
			if (_refEntity.IsCrouching)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	public override void ClearSkill()
	{
		SkillID currentActiveSkill = (SkillID)_refEntity.CurrentActiveSkill;
		if ((uint)currentActiveSkill <= 1u)
		{
			SetSkillEnd();
		}
	}

	protected void PlayerStopDashing()
	{
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.Dashing = false;
	}

	protected void SetSkillAndWeapon(SkillID skillId)
	{
		_refEntity.SkillEnd = skillId == SkillID.NONE;
		_refEntity.CurrentActiveSkill = (int)skillId;
		switch (skillId)
		{
		case SkillID.NONE:
			ToggleWeapon(WeaponState.NORMAL);
			break;
		case SkillID.SKILL_0:
			ToggleWeapon(WeaponState.SKILL_0);
			break;
		case SkillID.SKILL_1:
			ToggleWeapon(WeaponState.SKILL_1);
			break;
		}
	}

	protected void StartHitPause(float pauseTime = 1f)
	{
		if (!IsHitPauseStarted)
		{
			IsHitPauseStarted = true;
			_lastVelocity = _refEntity.Velocity;
			_lastPosition = _refEntity.Controller.LogicPosition;
			_refEntity.SetSpeed(0, 0);
			_hitPauseCoroutine = StartCoroutine(HitPauseCoroutine(pauseTime));
		}
	}

	protected void ForceStopHitPause()
	{
		if (_hitPauseCoroutine != null)
		{
			_lastVelocity = VInt3.zero;
			StopCoroutine(_hitPauseCoroutine);
			_hitPauseCoroutine = null;
			_refEntity.Animator._animator.speed = 1f;
			IsHitPauseStarted = false;
		}
	}

	private IEnumerator HitPauseCoroutine(float pauseTime)
	{
		Action<bool> onHitPauseStateChangedEvent = this.OnHitPauseStateChangedEvent;
		if (onHitPauseStateChangedEvent != null)
		{
			onHitPauseStateChangedEvent(true);
		}
		_refEntity.Animator._animator.speed = 0f;
		yield return new WaitForSeconds(pauseTime);
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.SetSpeed(_lastVelocity.x, _lastVelocity.y);
		Action<bool> onHitPauseStateChangedEvent2 = this.OnHitPauseStateChangedEvent;
		if (onHitPauseStateChangedEvent2 != null)
		{
			onHitPauseStateChangedEvent2(false);
		}
		IsHitPauseStarted = false;
	}

	protected void CheckIsHitPause()
	{
		if (IsHitPauseStarted)
		{
			_refEntity.Controller.LogicPosition = _lastPosition;
		}
	}
}
