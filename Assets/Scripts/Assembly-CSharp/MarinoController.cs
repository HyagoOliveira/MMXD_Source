#define RELEASE
using System;
using UnityEngine;

public class MarinoController : CharacterControlBase
{
	private bool _shot;

	private OrangeCharacter.MainStatus LastMainStatus;

	private OrangeCharacter.SubStatus LastSubStatus;

	public override void Start()
	{
		base.Start();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch012_skill_01_stand_up", "ch012_skill_01_stand_mid", "ch012_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch012_skill_01_fall_up", "ch012_skill_01_fall_mid", "ch012_skill_01_fall_down" };
		return new string[2][] { array, array2 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch012_skill_02_stand_start", "ch012_skill_02_stand_loop", "ch012_skill_02_stand_end", "ch012_skill_02_jump_start", "ch012_skill_02_jump_loop", "ch012_skill_02_jump_end" };
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.LAND:
			case OrangeCharacter.SubStatus.DASH_END:
				if (_refEntity.CurrentFrame >= 1f)
				{
					_refEntity.IgnoreGravity = false;
					_refEntity.SkillEnd = true;
					if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.LAND)
					{
						_refEntity.Dashing = false;
						_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
					}
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
					}
				}
				else if ((double)_refEntity.CurrentFrame > 0.23 && !_shot)
				{
					Debug.Log("Trigger Skill!");
					_shot = true;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
					CreateSkillBullet(_refEntity.GetCurrentSkillObj());
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				}
				break;
			case OrangeCharacter.SubStatus.CROUCH_UP:
				if ((double)_refEntity.CurrentFrame >= 1.0)
				{
					Debug.Log("Trigger Skill!");
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_dash_000", _refEntity._transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_dashdx_000", _refEntity.ModelTransform, _refEntity.ModelTransform.rotation, Array.Empty<object>());
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
					_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 3.5f), 0);
					_refEntity.BulletCollider.UpdateBulletData(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
					_refEntity.BulletCollider.SetBulletAtk(_refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
					_refEntity.BulletCollider.BulletLevel = _refEntity.GetCurrentSkillObj().SkillLV;
					_refEntity.BulletCollider.Active(_refEntity.TargetMask);
					_refEntity.SetStatus(_refEntity.CurMainStatus, OrangeCharacter.SubStatus.SKILL_IDLE);
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL_IDLE:
				if (_refEntity.GetCurrentSkillObj().LastUseTimer.GetMillisecond() > 170)
				{
					_refEntity.SetSpeed(0, 0);
					_refEntity.BulletCollider.BackToPool();
					_refEntity.IgnoreGravity = false;
					if (_refEntity.PreBelow)
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.SKILL_IDLE);
					}
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.IDLE);
					}
					_refEntity.Dashing = false;
					_refEntity.SkillEnd = true;
				}
				break;
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			int num = 7;
			break;
		}
		default:
			Debug.Log("Skill Failed!");
			_refEntity.SkillEnd = true;
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			int currentActiveSkill = _refEntity.CurrentActiveSkill;
			if (currentActiveSkill != 0 && currentActiveSkill == 1)
			{
				_refEntity.BulletCollider.BackToPool();
			}
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.GetCurrentSkillObj());
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0 && id == 1 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			if (_refEntity.Dashing)
			{
				_refEntity.PlayerStopDashing();
			}
			_refEntity.IgnoreGravity = true;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			_refEntity.ResetVelocity();
			_refEntity.PlaySE(_refEntity.VoiceID, 8);
			_refEntity.DisableCurrentWeapon();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.CROUCH_UP);
			_refEntity.StopShootTimer();
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.IgnoreGravity = true;
			_refEntity.ResetVelocity();
			_refEntity.IsShoot = 1;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_shot = false;
			_refEntity.CheckLockDirection();
			_refEntity.PlaySE(_refEntity.VoiceID, 9);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, _refEntity.Solid_meeting(0f, -1f, (int)_refEntity.Controller.collisionMask | (int)_refEntity.Controller.collisionMaskThrough) ? OrangeCharacter.SubStatus.LAND : OrangeCharacter.SubStatus.DASH_END);
			_refEntity.StopShootTimer();
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		SKILL_TABLE bulletData = weaponStruct.BulletData;
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		string s_USE_MOTION = bulletData.s_USE_MOTION;
		if (s_USE_MOTION == "MARINO_DARTS")
		{
			float num = Vector2.SignedAngle(Vector2.right * (float)_refEntity._characterDirection, _refEntity.ShootDirection);
			_refEntity.CheckLockDirection();
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_darts_000a", _refEntity.ModelTransform.position, Quaternion.Euler(0f, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? 180f : 0f, (float)_refEntity._characterDirection * (0f - num)), Array.Empty<object>());
			_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, weaponStruct.ShootTransform[0], weaponStruct.SkillLV);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (LastMainStatus)
		{
		case OrangeCharacter.MainStatus.IDLE:
		{
			OrangeCharacter.SubStatus lastSubStatus = LastSubStatus;
			if (lastSubStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[1]);
			}
			break;
		}
		case OrangeCharacter.MainStatus.FALL:
		{
			OrangeCharacter.SubStatus lastSubStatus = LastSubStatus;
			if (lastSubStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[1]);
			}
			break;
		}
		}
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.IDLE:
			if (subStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			break;
		case OrangeCharacter.MainStatus.FALL:
			if (subStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			break;
		case OrangeCharacter.MainStatus.RIDE_ARMOR:
			_refEntity.DisableCurrentWeapon();
			break;
		}
		LastMainStatus = _refEntity.CurMainStatus;
		LastSubStatus = _refEntity.CurSubStatus;
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.IDLE:
			if (subStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[1]);
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
		case OrangeCharacter.MainStatus.FALL:
			if (subStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[1]);
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
}
