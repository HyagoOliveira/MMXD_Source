using System;
using UnityEngine;

public class CH055_Controller : CharacterControlBase
{
	private readonly int SKILL0_START = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_END = (int)(0.75f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_CANCEL = (int)(0.25f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_START = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_LOOP = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_END = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_CANCEL = (int)(0.45f / GameLogicUpdateManager.m_fFrameLen);

	private int skillEndFrame;

	private int skillCancelFrame;

	private Transform shootPointTransform;

	private Vector2 risingSpeed;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "CustomShootPoint", true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_ch055_fire_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch055_fire_000", 2);
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			PlaySkillSE("dg_hadoken");
			UpdateDirection();
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
			}
			_refEntity.Animator._animator.speed = 2f;
			skillEndFrame = gameFrame + SKILL0_START;
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			_refEntity.Animator._animator.speed = 1f;
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
			CreateSkillBullet(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
			skillEndFrame = gameFrame + SKILL0_END;
			skillCancelFrame = gameFrame + SKILL0_CANCEL;
			Vector3 shootDirection = _refEntity.ShootDirection;
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_ch055_fire_000", _refEntity.ExtraTransforms[0].position, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, shootDirection)), Array.Empty<object>());
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1:
			PlaySkillSE("dg_rising");
			UpdateDirection();
			_refEntity.IgnoreGravity = true;
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			skillEndFrame = gameFrame + SKILL1_START;
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxduring_ch055_fire_001", _refEntity.transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			risingSpeed = new Vector2((int)_refEntity._characterDirection * 5000, 12000f);
			_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[currentActiveSkill].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
			_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[currentActiveSkill].SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			skillEndFrame = gameFrame + SKILL1_LOOP;
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			skillEndFrame = gameFrame + SKILL1_END;
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			skillEndFrame = gameFrame + SKILL1_END;
			skillCancelFrame = gameFrame + SKILL1_CANCEL;
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		int gameFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (gameFrame < skillEndFrame && (!CheckCancelAnimate(_refEntity.CurrentActiveSkill) || gameFrame < skillCancelFrame))
			{
				break;
			}
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_BTSKILL_START)
				{
					if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
					}
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
					}
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
			_refEntity.BulletCollider.BackToPool();
			ResetSkillStatus();
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetSpeed((int)risingSpeed.x, (int)risingSpeed.y);
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.Dashing = false;
				_refEntity.IgnoreGravity = false;
				_refEntity.BulletCollider.BackToPool();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (gameFrame >= skillEndFrame || (CheckCancelAnimate(_refEntity.CurrentActiveSkill) && gameFrame >= skillCancelFrame))
			{
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				ResetSkillStatus();
			}
			break;
		}
	}

	private void UpdateDirection()
	{
		if (_refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
		{
			int num = Math.Sign((_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition - _refEntity._transform.position).x);
			_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			switch (_refEntity.CurrentActiveSkill)
			{
			case 0:
				_refEntity.BulletCollider.BackToPool();
				_refEntity.SetSpeed(0, 0);
				_refEntity.Animator._animator.speed = 1f;
				break;
			case 1:
				_refEntity.SetSpeed(0, 0);
				break;
			}
		}
		ResetSkillStatus();
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, shootPointTransform, weaponStruct.SkillLV);
	}

	private bool CheckCancelAnimate(int skillId)
	{
		return ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID);
	}

	public override void ControlCharacterDead()
	{
		_refEntity.Animator._animator.speed = 1f;
	}

	private void ResetSkillStatus()
	{
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.SetSpeed(0, 0);
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("dg_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch055_startin_000";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch055_skill_01_start", "ch055_skill_01_loop", "ch055_skill_01_end" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch055_skill_02_crouch_up", "ch055_skill_02_crouch_mid", "ch055_skill_02_crouch_down" };
		string[] array2 = new string[3] { "ch055_skill_02_stand_up", "ch055_skill_02_stand_mid", "ch055_skill_02_stand_down" };
		string[] array3 = new string[3] { "ch055_skill_02_jump_up", "ch055_skill_02_jump_mid", "ch055_skill_02_jump_down" };
		return new string[3][] { array, array2, array3 };
	}
}
