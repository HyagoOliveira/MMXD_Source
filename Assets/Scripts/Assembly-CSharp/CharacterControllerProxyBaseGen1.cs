using System;
using UnityEngine;

public abstract class CharacterControllerProxyBaseGen1 : CharacterControlBase
{
	protected enum WeaponState
	{
		NONE = -1,
		TELEPORT_IN = 0,
		TELEPORT_OUT = 1,
		NORMAL = 2,
		SKILL_0 = 3,
		SKILL_1 = 4
	}

	protected enum SkillID
	{
		NONE = -1,
		SKILL_0 = 0,
		SKILL_1 = 1
	}

	protected enum ShootLevel : sbyte
	{
		NoCharge = 0,
		SmallCharge = 1,
		MiddleCharge = 2,
		MaxCharge = 3
	}

	public sealed override void CheckSkill()
	{
		OnLogicUpdate();
		if (!_refEntity.IsAnimateIDChanged() && _refEntity.CurrentActiveSkill != -1)
		{
			int gameFrame = GameLogicUpdateManager.GameFrame;
			OnCheckSkill(gameFrame);
		}
	}

	protected virtual void OnLogicUpdate()
	{
	}

	protected virtual void OnCheckSkill(int nowFrame)
	{
	}

	public override void ClearSkill()
	{
	}

	public override void PlayerPressSkillCharacterCall(int skillID)
	{
		if (_refEntity.CurrentActiveSkill == -1)
		{
			OnPlayerPressSkillCharacterCall((SkillID)skillID);
		}
	}

	protected virtual void OnPlayerPressSkillCharacterCall(SkillID skillID)
	{
	}

	public sealed override void PlayerReleaseSkillCharacterCall(int skillID)
	{
		if (_refEntity.CurrentActiveSkill == -1)
		{
			OnPlayerReleaseSkillCharacterCall((SkillID)skillID);
		}
	}

	protected virtual void OnPlayerReleaseSkillCharacterCall(SkillID skillID)
	{
	}

	protected virtual void ToggleWeapon(WeaponState weaponState)
	{
		if (weaponState == WeaponState.NONE)
		{
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
		}
		else if (_refEntity.CheckCurrentWeaponIndex())
		{
			_refEntity.EnableCurrentWeapon();
		}
	}

	public sealed override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.EnterRideArmorEvt = OnEnterRideArmor;
		_refEntity.ChangeComboSkillEventEvt = OnChangeComboSkill;
		_refEntity.CheckPetActiveEvt = OnCheckPetActive;
		_refEntity.PlayTeleportOutEffectEvt = OnPlayTeleportOutEffect;
	}

	protected virtual void AnimationEndCharacterDepend(OrangeCharacter.MainStatus arg1, OrangeCharacter.SubStatus arg2)
	{
	}

	protected virtual void SetStatusCharacterDepend(OrangeCharacter.MainStatus arg1, OrangeCharacter.SubStatus arg2)
	{
	}

	protected virtual void TeleportOutCharacterDepend()
	{
	}

	protected virtual void StageTeleportOutCharacterDepend()
	{
	}

	protected virtual void StageTeleportInCharacterDepend()
	{
	}

	protected virtual void TeleportInCharacterDepend()
	{
	}

	protected virtual void TeleportInExtraEffect()
	{
	}

	protected virtual bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	private void OnChangeComboSkill(object[] parameters)
	{
		if (parameters.Length == 2)
		{
			SkillID skillId = (SkillID)(int)parameters[0];
			int reloadIndex = (int)parameters[1];
			OnChangeComboSkill(skillId, reloadIndex);
		}
	}

	protected virtual void OnChangeComboSkill(SkillID skillId, int reloadIndex)
	{
	}

	protected virtual bool OnCheckPetActive(int petId)
	{
		return false;
	}

	protected virtual void OnPlayTeleportOutEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_OUT", base.transform.position, Quaternion.identity, Array.Empty<object>());
	}

	protected virtual void SetSkillEnd()
	{
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.SetSpeed(0, 0);
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.BulletCollider.BackToPool();
		ToggleWeapon(WeaponState.NORMAL);
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

	protected virtual bool CheckCanTriggerSkill(SkillID skillId)
	{
		return _refEntity.CheckUseSkillKeyTrigger((int)skillId);
	}
}
