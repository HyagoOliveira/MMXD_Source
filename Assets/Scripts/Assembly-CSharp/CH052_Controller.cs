#define RELEASE
using System;
using UnityEngine;

public class CH052_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected OrangeTimer NOVASTRIKETimer;

	protected ParticleSystem fxDuringSkill0;

	protected Vector3 Skill0Directioon = Vector3.right;

	protected ParticleSystem fxUseSkill1;

	protected Vector3 vLockShootDirection = Vector3.zero;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch052_skill_01_stand_start" };
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch052_skill_01_stand_start", "ch052_skill_01_stand_loop" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch052_skill_02_stand_up", "ch052_skill_02_stand_mid", "ch052_skill_02_stand_down" };
		string[] array2 = new string[3] { "ch052_skill_02_jump_up", "ch052_skill_02_jump_mid", "ch052_skill_02_jump_down" };
		string[] array3 = new string[3] { "ch052_skill_02_crouch_up", "ch052_skill_02_crouch_mid", "ch052_skill_02_crouch_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		NOVASTRIKETimer = OrangeTimerManager.GetTimer();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_vajurila_ff_000", true);
		fxDuringSkill0 = transform.GetComponentInChildren<ParticleSystem>();
		fxDuringSkill0.Stop();
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_vajurila_ff_002", true);
		fxUseSkill1 = transform2.GetComponentInChildren<ParticleSystem>();
		fxUseSkill1.Stop();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch052_ff_002", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.SkillEnd = true;
			_refEntity.BulletCollider.BackToPool();
			CancelSkill0(false);
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			_refEntity.SkillEnd = true;
			CancelSkill1();
		}
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged())
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
		case OrangeCharacter.SubStatus.RIDE_ARMOR:
			if (_refEntity.CurrentActiveSkill != 0)
			{
				Debug.LogError("_CurrentActiveSkill != 0 => " + _refEntity.CurrentActiveSkill);
				_refEntity.CurrentActiveSkill = 0;
			}
			if (_refEntity.Velocity.y <= 0)
			{
				Debug.Log("Trigger Skill!");
				bool flag = false;
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				SKILL_TABLE sKILL_TABLE = currentSkillObj.BulletData;
				if (currentSkillObj.ComboCheckDatas.Length != 0 && currentSkillObj.ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
				{
					flag = true;
					sKILL_TABLE = currentSkillObj.FastBulletDatas[currentSkillObj.Reload_index];
				}
				OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
				NOVASTRIKETimer.TimerStart();
				PlaySkillSE("vj_rush01");
				PlaySkillSE("vj_rush02_lg");
				_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 2.5f), 0);
				_refEntity.BulletCollider.UpdateBulletData(sKILL_TABLE, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(currentSkillObj.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = currentSkillObj.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.SetStatus(_refEntity.CurMainStatus, OrangeCharacter.SubStatus.IDLE);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[0], null, currentSkillObj.Reload_index);
				if (flag)
				{
					fxDuringSkill0.Play();
					_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
				}
				else
				{
					fxDuringSkill0.Play();
				}
			}
			break;
		case OrangeCharacter.SubStatus.IDLE:
			if (_refEntity.CurrentActiveSkill != 0)
			{
				Debug.LogError("_CurrentActiveSkill != 0 => " + _refEntity.CurrentActiveSkill);
				_refEntity.CurrentActiveSkill = 0;
			}
			if (NOVASTRIKETimer.GetMillisecond() > 417)
			{
				PlaySkillSE("vj_rush02_stop");
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SetSpeed(0, 0);
				_refEntity.SkillEnd = true;
				_refEntity.BulletCollider.BackToPool();
				fxDuringSkill0.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.ShootDirection = vLockShootDirection;
			_refEntity.Animator.SetAttackLayerActive(vLockShootDirection);
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.25f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (!bInSkill && _refEntity.CurrentFrame > 0.3f && CheckCancelAnimate(1))
			{
				CancelSkill1();
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			if (_refEntity.Dashing)
			{
				_refEntity.PlayerStopDashing();
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			_refEntity.SetSpeed((int)_refEntity._characterDirection * OrangeCharacter.WalkSpeed, (int)((float)OrangeCharacter.JumpSpeed * 0.5f));
			_refEntity.StopShootTimer();
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.WALK && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.RIDE_ARMOR)
			{
				Skill0Directioon = Vector3.right * (0 - _refEntity._characterDirection);
			}
			else
			{
				Skill0Directioon = Vector3.right * (float)_refEntity._characterDirection;
			}
			PlayCharaSE("vj_jump");
			ToggleWeapon(1);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.RIDE_ARMOR);
			_refEntity.StartJumpThroughCorutine();
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 1 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlaySkillSE("vj_ring01");
			bInSkill = true;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.IsShoot = 1;
			vLockShootDirection = _refEntity.ShootDirection;
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_ch052_ff_002", _refEntity.BulletCollider.transform, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, vLockShootDirection)), Array.Empty<object>());
			ToggleWeapon(2);
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			else if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus != 0)
			{
				int num = 1;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				_refEntity.IgnoreGravity = true;
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && mainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SkillEndChnageToIdle(true);
				break;
			}
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus != OrangeCharacter.SubStatus.SKILL0 && (uint)(curSubStatus - 49) <= 2u)
			{
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[1], wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ExtraTransforms[1]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
			}
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 2) <= 1u)
		{
			_refEntity._characterDirection = ((Skill0Directioon.x >= 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public void StageTeleportInCharacterDepend()
	{
		fxDuringSkill0.Stop();
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch052_startin_000";
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void CancelSkill0(bool isCrouch)
	{
		fxDuringSkill0.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		SkillEndChnageToIdle();
	}

	private void CancelSkill1()
	{
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1)
		{
			SkillEndChnageToIdle();
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		bInSkill = false;
		ToggleWeapon(0);
		if (isCrouch)
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
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void ToggleWeapon(int style)
	{
		if ((uint)(style - 1) <= 1u)
		{
			_refEntity.DisableCurrentWeapon();
		}
		else
		{
			_refEntity.EnableCurrentWeapon();
		}
	}
}
