using System;
using UnityEngine;

public class AxlController : CharacterControlBase, ILogicUpdate
{
	private bool _rollEnd;

	private bool isSetupReady;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch006_skill_01_stand_loop", "ch006_skill_01_stand_end", "ch006_skill_01_jump_loop", "ch006_skill_01_jump_end", "ch006_skill_02_start", "ch006_skill_02_loop", "ch006_skill_02_end" };
	}

	public void LogicUpdate()
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 49) <= 2u)
			{
				_refEntity.ToggleExtraMesh(true);
			}
			else
			{
				_refEntity.ToggleExtraMesh(!_refEntity.Controller.BelowInBypassRange);
			}
			break;
		}
		case OrangeCharacter.MainStatus.DASH:
			_refEntity.ToggleExtraMesh(true);
			break;
		case OrangeCharacter.MainStatus.WALLGRAB:
		case OrangeCharacter.MainStatus.RIDE_ARMOR:
			_refEntity.ToggleExtraMesh(false);
			break;
		default:
			_refEntity.ToggleExtraMesh(!_refEntity.Controller.BelowInBypassRange);
			break;
		}
	}

	public override void Start()
	{
		base.Start();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	private new void Setup()
	{
		if (isSetupReady)
		{
			return;
		}
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		OrangeCharacter refEntity = _refEntity;
		Renderer[] extraMeshClose = new SkinnedMeshRenderer[0];
		refEntity.ExtraMeshClose = extraMeshClose;
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "WingMesh_m");
		if (array != null)
		{
			OrangeCharacter refEntity2 = _refEntity;
			extraMeshClose = new SkinnedMeshRenderer[array.Length];
			refEntity2.ExtraMeshOpen = extraMeshClose;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshOpen[i] = array[i].GetComponent<SkinnedMeshRenderer>();
			}
		}
		_refEntity.ToggleExtraMesh(false);
		isSetupReady = true;
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void PlayRollEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_axlskill1", _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_axlskill2", _refEntity._transform.position, Quaternion.Euler(0f, (_refEntity.direction != 1) ? 180 : 0, _refEntity.AimTransform.localRotation.eulerAngles.x), Array.Empty<object>());
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.CurrentActiveSkill = -1;
			Skill0EndChnageToIdle();
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.JUMP))
				{
					Skill0EndChnageToIdle(true);
				}
				else
				{
					Skill0EndChnageToIdle();
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			PlayRollEffect();
			if ((double)_refEntity.CurrentFrame >= 1.0)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			PlayRollEffect();
			if ((double)_refEntity.CurrentFrame >= 1.0 && (_rollEnd || _refEntity.bLockInputCtrl || _refEntity.LockInput || _refEntity.LockSkill))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if ((double)_refEntity.CurrentFrame >= 1.0)
			{
				if (_refEntity.Dashing)
				{
					_refEntity.PlayerStopDashing();
					_refEntity.Dashing = false;
				}
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
			}
			else if (_refEntity.CurrentFrame > 0.2f)
			{
				_refEntity.SetHorizontalSpeed(0);
			}
			break;
		}
	}

	protected void PlayHurtSE()
	{
		_refEntity.PlaySE(_refEntity.SkillSEID, "a_magnum");
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				_refEntity.UpdateWeaponMesh(_refEntity.PlayerSkills[id], _refEntity.GetCurrentWeaponObj());
				_refEntity.PushBulletDetail(_refEntity.PlayerSkills[id].BulletData, _refEntity.PlayerSkills[id].weaponStatus, base.transform.position, _refEntity.PlayerSkills[id].SkillLV, null, true, 1);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[id]);
				_refEntity.PreBelow = _refEntity.Controller.Collisions.below;
				_refEntity.SetSpeed(0, 0);
				_refEntity.IgnoreGravity = true;
				_refEntity.StopShootTimer();
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				if (_refEntity.IsShoot != 0)
				{
					_refEntity._characterDirection = ((_refEntity.ShootDirection.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
				}
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("obj_fxuse_rolling_000", _refEntity._transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[id]);
				_refEntity.SetSpeed((int)_refEntity._characterDirection * OrangeCharacter.DashSpeed, 0);
				_refEntity.StopShootTimer();
				_rollEnd = false;
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.selfBuffManager.AddBuff(_refEntity.PlayerSkills[id].BulletData.n_CONDITION_ID, 0, 0, _refEntity.PlayerSkills[id].BulletData.n_ID);
				}
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public void PlayerReleaseSkill(int id)
	{
		if (id != 0 && id == 1 && _refEntity.CurrentActiveSkill == id)
		{
			_rollEnd = true;
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.FALL:
			if (subStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId((HumanBase.AnimateId)(65 + ((!_refEntity.PreBelow) ? 2 : 0)));
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (_refEntity.PreBelow)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[0]);
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			}
			break;
		}
	}

	public override void ControlCharacterDead()
	{
		_refEntity.DisableSkillWeapon(0);
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				if (_refEntity.PreBelow)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				}
				else
				{
					Skill0EndChnageToIdle();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				Skill0EndChnageToIdle();
				break;
			}
			break;
		case OrangeCharacter.MainStatus.FALL:
			if (subStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[0]);
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
			}
			break;
		}
	}

	private void Skill0EndChnageToIdle(bool doJump = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.BulletCollider.BackToPool();
		_refEntity.IgnoreGravity = false;
		_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[0]);
		if (_refEntity.PreBelow)
		{
			_refEntity.Dashing = false;
			if (doJump)
			{
				_refEntity.SetSpeed(0, OrangeCharacter.JumpSpeed);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.JUMP, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			else
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}
}
