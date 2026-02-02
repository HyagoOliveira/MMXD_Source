using System;
using UnityEngine;

public class CH044_Controller : CharacterControlBase
{
	private bool bInSkill;

	private bool bWallGrab;

	private Transform tfFanMeshTransform;

	private Renderer tfFanMesh;

	private ParticleSystem fxSkill01;

	private ParticleSystem fxSkill02;

	[SerializeField]
	private int nHitStop = 4;

	private bool bSkill0Hit;

	private int nNowFrame;

	private int nHitStopFrame;

	private int nOriSpeedY;

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_Zero_SummerFestival_in";
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch044_skill_02_stand_up", "ch044_skill_02_stand_mid", "ch044_skill_02_stand_down" };
		string[] array2 = new string[3] { "ch044_skill_02_jump_up", "ch044_skill_02_jump_mid", "ch044_skill_02_jump_down" };
		string[] array3 = new string[3] { "ch044_skill_02_crouch_up", "ch044_skill_02_crouch_mid", "ch044_skill_02_crouch_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[4] { "ch044_skill_01_stand_start", "ch044_skill_01_crouch_start", "ch044_skill_01_loop", "ch044_skill_01_end" };
	}

	public void TeleportInExtraEffect()
	{
		_refEntity.PlaySE(_refEntity.SkillSEID, "ch044_chara03");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[3];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill01ShootPoint", true);
		tfFanMeshTransform = OrangeBattleUtility.FindChildRecursive(ref target, "FanMesh_c", true);
		tfFanMesh = tfFanMeshTransform.GetComponent<Renderer>();
		fxSkill01 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_dragonrise_000", true).GetComponent<ParticleSystem>();
		fxSkill01.Stop();
		fxSkill02 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_swallowdance_000", true).GetComponent<ParticleSystem>();
		fxSkill02.Stop();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
	}

	protected void OnEnable()
	{
		if ((bool)fxSkill01 && fxSkill01.isPlaying)
		{
			fxSkill01.Stop();
		}
		if ((bool)fxSkill02 && fxSkill02.isPlaying)
		{
			fxSkill02.Stop();
		}
	}

	public override void ClearSkill()
	{
		tfFanMesh.enabled = false;
		if (_refEntity.CurrentActiveSkill == 0)
		{
			CancelSkill0();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			CancelSkill1();
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		nNowFrame = GameLogicUpdateManager.GameFrame;
		if (_refEntity.CurrentActiveSkill == 0)
		{
			if (bSkill0Hit)
			{
				if (nNowFrame > nHitStopFrame)
				{
					bSkill0Hit = false;
					_refEntity.IgnoreGravity = false;
					_refEntity.SetSpeed(0, nOriSpeedY);
					_refEntity.BulletCollider.GetClearTimer().TimerStart();
					_refEntity.Animator._animator.speed = 1f;
				}
			}
			else if (_refEntity.Animator._animator.speed < 1f)
			{
				_refEntity.Animator._animator.speed = 1f;
			}
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.CurrentFrame > 0.7f && bInSkill)
			{
				ShootSkill0();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (CheckCancelAnimate(0))
			{
				CancelSkill0();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (_refEntity.CurrentFrame > 0.25f)
			{
				if (bInSkill)
				{
					bInSkill = false;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
					CreateSkillBullet(_refEntity.GetCurrentSkillObj());
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity._transform);
					fxSkill02.Play();
					tfFanMesh.enabled = false;
				}
				else if (CheckCancelAnimate(1))
				{
					CancelSkill1();
				}
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill == -1 && (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.StopShootTimer();
			_refEntity.DisableCurrentWeapon();
			if (_refEntity is OrangeConsoleCharacter)
			{
				(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL0, false);
			}
			_refEntity.StartJumpThroughCorutine();
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			_refEntity.PlaySE(_refEntity.VoiceID, "v_ch044_skill01");
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id != 1 || _refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
		{
			return;
		}
		_refEntity.CurrentActiveSkill = id;
		_refEntity.SkillEnd = false;
		bInSkill = true;
		bWallGrab = false;
		_refEntity.StopShootTimer();
		_refEntity.DisableCurrentWeapon();
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
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.WALLGRAB)
			{
				bWallGrab = true;
			}
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
		}
		_refEntity.PlaySE(_refEntity.VoiceID, "v_ch044_skill02");
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				tfFanMesh.enabled = true;
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				tfFanMesh.enabled = true;
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetSpeed(0, 0);
				tfFanMesh.enabled = true;
				_refEntity.Dashing = false;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetSpeed(0, 0);
				tfFanMesh.enabled = true;
				_refEntity.Dashing = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetSpeed(0, 0);
				tfFanMesh.enabled = true;
				_refEntity.Dashing = false;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				_refEntity.Animator._animator.speed = 1.2f;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetSpeed(0, 0);
				tfFanMesh.enabled = true;
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				_refEntity.Animator._animator.speed = 1.2f;
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetSpeed(0, 0);
				tfFanMesh.enabled = true;
				_refEntity.Dashing = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				_refEntity.Animator._animator.speed = 1.2f;
				break;
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				tfFanMesh.enabled = false;
				_refEntity.EnableCurrentWeapon();
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				if (bInSkill)
				{
					ShootSkill0();
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (bInSkill)
				{
					ShootSkill0();
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				if (_refEntity.Velocity.y <= 6000 && !bSkill0Hit)
				{
					if (_refEntity.BulletCollider.IsActivate)
					{
						_refEntity.BulletCollider.HitCallback = null;
						_refEntity.BulletCollider.BackToPool();
					}
					fxSkill01.Stop();
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
				fxSkill02.Stop();
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				fxSkill02.Stop();
				SkillEndChnageToIdle(true);
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.BulletCollider.HitCallback = OnSkill0Hit;
			_refEntity.BulletCollider.UpdateBulletData(wsSkill.FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = wsSkill.SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (bWallGrab)
			{
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV);
			}
			else
			{
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			}
			break;
		}
	}

	public override void ControlCharacterDead()
	{
		tfFanMesh.enabled = false;
	}

	public void TeleportOutCharacterDepend()
	{
	}

	public void StageTeleportOutCharacterDepend()
	{
		if (tfFanMesh.enabled)
		{
			tfFanMesh.enabled = false;
		}
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
			if (ManagedSingleton<InputStorage>.Instance.IsReleased(_refEntity.UserID, ButtonId.DOWN))
			{
				return true;
			}
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				if (_refEntity.CurSubStatus != OrangeCharacter.SubStatus.SKILL1_2)
				{
					return true;
				}
				if (!ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SHOOT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.JUMP) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DASH) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
				{
					return true;
				}
			}
			break;
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		tfFanMesh.enabled = false;
		_refEntity.EnableCurrentWeapon();
		_refEntity.Animator._animator.speed = 1f;
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

	private void ShootSkill0()
	{
		bInSkill = false;
		_refEntity.SetSpeed(0, Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 1.1f));
		OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
		CreateSkillBullet(_refEntity.GetCurrentSkillObj());
		_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity._transform);
		fxSkill01.Play();
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		tfFanMesh.enabled = false;
		_refEntity.EnableCurrentWeapon();
		bSkill0Hit = false;
		if (_refEntity.Animator._animator.speed < 1f)
		{
			_refEntity.Animator._animator.speed = 1f;
		}
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackCallback = null;
			_refEntity.BulletCollider.BackToPool();
		}
		if (fxSkill01.IsAlive())
		{
			fxSkill01.Stop();
		}
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void OnSkill0Hit(object obj)
	{
		if (_refEntity.CurrentActiveSkill == 0 && !bSkill0Hit)
		{
			bSkill0Hit = true;
			nOriSpeedY = _refEntity.Velocity.y;
			_refEntity.IgnoreGravity = true;
			nHitStopFrame = nNowFrame + nHitStop;
			_refEntity.BulletCollider.GetClearTimer().TimerStop();
			_refEntity.Animator._animator.speed = 0.1f;
		}
	}

	private void CancelSkill1()
	{
		if (fxSkill02.IsAlive())
		{
			fxSkill02.Stop();
		}
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}
}
