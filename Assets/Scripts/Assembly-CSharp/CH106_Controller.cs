using System;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class CH106_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected bool bPlayRedBalls;

	protected List<ParticleSystem> _liFxWindBursts = new List<ParticleSystem>();

	protected List<ParticleSystem> _liFxRedBalls = new List<ParticleSystem>();

	protected List<ParticleSystem> _liFxWindFires = new List<ParticleSystem>();

	protected ParticleSystem fxuse_dragonbreath_main;

	protected CH106_BeamBullet _pBeam;

	protected Vector3 _vSkill0ShootDirection;

	protected int _nLockSkill1Direction;

	protected FxBase _fxUseSkill0;

	protected IAimTarget _pRushTarget;

	protected Vector3 _vSkillStartPosition;

	protected Vector2 _vSkillVelocity;

	protected RushCollideBullet _pRushCollideBullet;

	protected bool _bSyncRushSkillCompleted;

	protected Vector3 _vSyncDirection;

	private OrangeConsoleCharacter _refPlayer;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[13]
		{
			"ch106_skill_01_stand_start", "ch106_skill_01_stand_loop", "ch106_skill_01_stand_end", "ch106_skill_01_jump_start", "ch106_skill_01_jump_loop", "ch106_skill_01_jump_end", "ch106_skill_01_crouch_start", "ch106_skill_01_crouch_loop", "ch106_skill_01_crouch_end", "ch106_skill_02_start",
			"ch106_skill_02_loop", "ch106_skill_02_stand_end", "ch106_skill_02_jump_end"
		};
	}

	public override void Start()
	{
		base.Start();
		InitLinkSkill();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip R UpperArm", true);
		Transform[] array = OrangeBattleUtility.FindMultiChildRecursive(target, "fxdemo_valstrax_burst", true);
		_liFxWindBursts.Clear();
		Transform[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ParticleSystem component = array2[i].GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				_liFxWindBursts.Add(component);
			}
		}
		fxuse_dragonbreath_main = OrangeBattleUtility.FindChildRecursive(target, "fxuse_dragonbreath_main", true).GetComponent<ParticleSystem>();
		Transform[] array3 = OrangeBattleUtility.FindMultiChildRecursive(target, "fxuse_valstraxlaser_001", true);
		_liFxRedBalls.Clear();
		array2 = array3;
		for (int i = 0; i < array2.Length; i++)
		{
			ParticleSystem component2 = array2[i].GetComponent<ParticleSystem>();
			if ((bool)component2)
			{
				_liFxRedBalls.Add(component2);
			}
		}
		EnableRedBalls(false);
		Transform[] array4 = OrangeBattleUtility.FindMultiChildRecursive(target, "fxuse_dragonbreath_000_(work)", true);
		_liFxWindFires.Clear();
		array2 = array4;
		for (int i = 0; i < array2.Length; i++)
		{
			ParticleSystem component3 = array2[i].GetComponent<ParticleSystem>();
			if ((bool)component3)
			{
				_liFxWindFires.Add(component3);
			}
		}
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<CH106_BeamBullet>("prefab/bullet/p_valstraxlaser_000_01", "p_valstraxlaser_000_01", 4, null);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_valstraxlaser_000", 4);
	}

	private void InitLinkSkill()
	{
		_refPlayer = _refEntity as OrangeConsoleCharacter;
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
	}

	public override void SetRushBullet(RushCollideBullet rushCollideBullet)
	{
		_pRushCollideBullet = rushCollideBullet;
		if (_refEntity.UsingVehicle)
		{
			_pRushCollideBullet.BackToPool();
			_pRushCollideBullet = null;
		}
	}

	public override void SyncSkillDirection(Vector3 dir, IAimTarget target)
	{
		if (_refEntity.UsingVehicle)
		{
			if ((bool)_pRushCollideBullet)
			{
				_pRushCollideBullet.BackToPool();
				_pRushCollideBullet = null;
			}
			_pRushTarget = null;
			return;
		}
		_bSyncRushSkillCompleted = true;
		_vSyncDirection = dir;
		_pRushTarget = target;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			_vSkillVelocity = _vSyncDirection.normalized * (OrangeCharacter.DashSpeed * 4);
			UpdateSkill1Direction(_vSkillVelocity.x);
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			CancelSkill0();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			CancelSkill1();
		}
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void CheckSkill()
	{
		if (!_refEntity.IsAnimateIDChanged())
		{
			UpdateVirtualButtonAnalog();
			UpdateSkill();
		}
	}

	private void UpdateVirtualButtonAnalog()
	{
		if (_refEntity.IsLocalPlayer && _refPlayer != null)
		{
			_refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill1(id);
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				ToggleWeapon(-2);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				EnableWindBrustFx(true, true);
				ToggleWeapon(-3);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				_refEntity.Animator._animator.speed = 1f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				_refEntity.Animator._animator.speed = 1f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				_refEntity.Animator._animator.speed = 1f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_6:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				_refEntity.Animator._animator.speed = 2.25f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_7:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				_refEntity.Animator._animator.speed = 2.25f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_8:
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				_refEntity.Animator._animator.speed = 2.25f;
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
				_refEntity.Animator._animator.speed = 1f;
				_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
				UpdateSkill1Rotation(_vSkillVelocity);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.IgnoreGravity = false;
				UpdateSkill1Rotation(Vector3.right * _refEntity.direction);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
					_refEntity.Animator._animator.speed = 1f;
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)77u);
					_refEntity.Animator._animator.speed = 1f;
				}
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
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
				EnableWindBrustFx(false);
				ToggleWeapon(0);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0_6:
			case OrangeCharacter.SubStatus.SKILL0_7:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_8:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				if (_refEntity.IsLocalPlayer)
				{
					CreateSkillBullet(_refEntity.PlayerSkills[1]);
					_vSkillVelocity = _vSkillVelocity.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
					UpdateSkill1Direction(_vSkillVelocity.x);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetSpeed(0, 0);
				SkillEndChnageToIdle();
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[1], wsSkill.SkillLV, _vSkill0ShootDirection);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.AimTransform, wsSkill.SkillLV, _vSkillVelocity);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.BulletData, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
		}
	}

	public bool BeamStartTurn()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 22) <= 2u)
			{
				EnableRedBalls(false);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, _refEntity.CurSubStatus + 3);
				return true;
			}
		}
		return false;
	}

	public float GetBeamAngle()
	{
		Vector3 v = _refEntity.ExtraTransforms[1].position - _refEntity.ExtraTransforms[2].position;
		if (_refEntity._characterDirection == CharacterDirection.LEFT)
		{
			return Vector3.Angle(v.xy(), Vector3.left);
		}
		return Vector3.Angle(v.xy(), Vector3.right);
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) <= 8u)
		{
			if (_nLockSkill1Direction != 0)
			{
				_refEntity._characterDirection = (CharacterDirection)_nLockSkill1Direction;
			}
			else
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public void StageTeleportOutCharacterDepend()
	{
		EnableWindBrustFx(false);
		EnableWindFire(false);
	}

	public void StageTeleportInCharacterDepend()
	{
		EnableWindFire(true);
	}

	private void UpdateSkill()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_2:
				if (_refEntity.CurrentFrame > 1f && bInSkill)
				{
					bInSkill = false;
					CreateSkillBullet(_refEntity.PlayerSkills[0]);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, _refEntity.CurSubStatus + 3);
				}
				else if (bPlayRedBalls && _refEntity.CurrentFrame > 0.7f)
				{
					bPlayRedBalls = false;
					EnableRedBalls(true);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_6:
			case OrangeCharacter.SubStatus.SKILL0_7:
			case OrangeCharacter.SubStatus.SKILL0_8:
				if (_refEntity.CurrentFrame > 1f)
				{
					bool isCrouch = _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8;
					SkillEndChnageToIdle(isCrouch);
				}
				else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.65f)
				{
					SkipSkill0Animation();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				if (_refEntity.CurrentFrame > 1f)
				{
					bool flag = false;
					if (_refEntity.IsLocalPlayer)
					{
						flag = true;
					}
					else if (_bSyncRushSkillCompleted)
					{
						_bSyncRushSkillCompleted = false;
						flag = true;
					}
					if (flag)
					{
						CreateSkillBullet(_refEntity.PlayerSkills[1]);
						_vSkillVelocity = _vSkillVelocity.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
						UpdateSkill1Direction(_vSkillVelocity.x);
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
			{
				bool flag2 = false;
				if (Vector2.Distance(_vSkillStartPosition, _refEntity.AimPosition) > _refEntity.PlayerSkills[1].BulletData.f_DISTANCE || _refEntity.PlayerSkills[1].LastUseTimer.GetMillisecond() > 500)
				{
					flag2 = true;
				}
				else if (_pRushTarget != null)
				{
					float num = Vector2.Distance(_refEntity.AimPosition, _pRushTarget.AimPosition);
					float num2 = (float)_refEntity.Velocity.magnitude * 0.001f * GameLogicUpdateManager.m_fFrameLen;
					if (num < num2 * 1.5f)
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					if (!_refEntity.Controller.Collisions.below)
					{
						_refEntity.SetSpeed(_refEntity.direction * OrangeCharacter.DashSpeed, 0);
					}
					else
					{
						_refEntity.SetSpeed(0, 0);
					}
					EnableWindBrustFx(false);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				}
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (_refEntity.CurrentFrame > 1f)
				{
					if ((bool)_pRushCollideBullet)
					{
						_pRushCollideBullet.BackToPool();
						_pRushCollideBullet = null;
					}
					SkillEndChnageToIdle();
				}
				else if (_refEntity.CurrentFrame > 0.45f && (CheckCancelAnimate(1) || _refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below))
				{
					if ((bool)_pRushCollideBullet)
					{
						_pRushCollideBullet.BackToPool();
						_pRushCollideBullet = null;
					}
					SkipSkill1Animation();
				}
				break;
			}
		}
		else if ((bool)_pRushCollideBullet)
		{
			_pRushCollideBullet.BackToPool();
			_pRushCollideBullet = null;
		}
	}

	private void TurnToAimTarget()
	{
		Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
		if (vector.HasValue)
		{
			int num = Math.Sign(vector.Value.x);
			if (_refEntity.direction != num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void TurnToShootDirection(Vector3 dir)
	{
		int num = Math.Sign(dir.x);
		if (_refEntity.direction != num && Mathf.Abs(dir.x) > 0.05f)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			_refEntity.ShootDirection = dir;
		}
	}

	private void UseSkill0(int skillId)
	{
		PlayVoiceSE("v_zm_skill01");
		PlaySkillSE("zm_kakuyou");
		bInSkill = true;
		bPlayRedBalls = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		if (_refEntity.UseAutoAim)
		{
			TurnToAimTarget();
		}
		_vSkill0ShootDirection = _refEntity.ShootDirection;
		_nLockSkill1Direction = (int)_refEntity._characterDirection;
		UpdateSkill1Direction(_vSkill0ShootDirection.x);
		_fxUseSkill0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_valstraxlaser_000", _refEntity.AimTransform.position + Vector3.right * 0.2f * _refEntity.direction, Quaternion.identity, Array.Empty<object>());
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
		}
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		EnableRedBalls(false);
		if ((bool)_fxUseSkill0)
		{
			_fxUseSkill0.BackToPool();
			_fxUseSkill0 = null;
		}
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private void UseSkill1(int skillId)
	{
		PlayVoiceSE("v_zm_skill02");
		PlaySkillSE("zm_dash");
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		_bSyncRushSkillCompleted = false;
		if (_refEntity.IsLocalPlayer)
		{
			if (_refEntity.UseAutoAim && _refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
			{
				TurnToAimTarget();
				_pRushTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
				_vSkillVelocity = (_pRushTarget.AimPosition - _refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * 4f);
				UpdateSkill1Direction(_vSkillVelocity.x);
			}
			else
			{
				_pRushTarget = null;
				_vSkillVelocity = _refEntity.ShootDirection.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
				UpdateSkill1Direction(_vSkillVelocity.x);
			}
		}
		EnableWindBrustFx(true);
		_vSkillStartPosition = _refEntity.AimPosition;
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
	}

	private void UpdateSkill1Direction(float dirX)
	{
		if (Mathf.Abs(dirX) > 0.05f)
		{
			int num = Math.Sign(dirX);
			if (num != _refEntity.direction)
			{
				_refEntity.direction = num;
			}
		}
	}

	protected void UpdateSkill1Rotation(Vector3 dir)
	{
		Vector3 to = ((_refEntity._characterDirection == CharacterDirection.LEFT) ? Vector3.left : Vector3.right);
		float num = Vector3.Angle(dir.xy(), to);
		if (dir.y < 0f)
		{
			num = 0f - num;
		}
		_refEntity.ModelTransform.localEulerAngles = new Vector3(_refEntity.ModelTransform.localEulerAngles.x, _refEntity.ModelTransform.localEulerAngles.y, num);
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		if ((bool)_pRushCollideBullet)
		{
			_pRushCollideBullet.BackToPool();
			_pRushCollideBullet = null;
		}
		EnableWindBrustFx(false);
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		_refEntity.SetSpeed(0, 0);
		EnableWindBrustFx(false);
		UpdateSkill1Rotation(Vector3.right * _refEntity.direction);
		SkillEndChnageToIdle();
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8))
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.GravityMultiplier = new VInt(1f);
		_refEntity.Animator._animator.speed = 1f;
		bInSkill = false;
		_pRushTarget = null;
		if ((bool)_pRushCollideBullet)
		{
			_pRushCollideBullet.BackToPool();
			_pRushCollideBullet = null;
		}
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
		switch (style)
		{
		case -3:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			break;
		}
	}

	protected void EnableWindBrustFx(bool enable, bool isTELEPORT_POSE = false)
	{
		foreach (ParticleSystem liFxWindBurst in _liFxWindBursts)
		{
			if (enable)
			{
				liFxWindBurst.Play(true);
				if (!isTELEPORT_POSE)
				{
					fxuse_dragonbreath_main.Play(true);
				}
			}
			else
			{
				liFxWindBurst.Stop(true);
				if (!isTELEPORT_POSE)
				{
					fxuse_dragonbreath_main.Stop(true);
				}
			}
		}
	}

	protected void EnableWindFire(bool enable)
	{
		foreach (ParticleSystem liFxWindFire in _liFxWindFires)
		{
			if (enable)
			{
				liFxWindFire.Play(true);
			}
			else
			{
				liFxWindFire.Stop(true);
			}
		}
	}

	protected void EnableRedBalls(bool enable)
	{
		foreach (ParticleSystem liFxRedBall in _liFxRedBalls)
		{
			if (enable)
			{
				liFxRedBall.Play(true);
			}
			else
			{
				liFxRedBall.Stop(true);
			}
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (StageUpdate.gbIsNetGame && _refPlayer != null)
		{
			ClearSkill();
		}
	}
}
