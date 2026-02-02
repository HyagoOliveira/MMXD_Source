using System;
using StageLib;
using UnityEngine;

public class CH105_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected Vector3 _vSkill0ShootDirection;

	protected FxBase _fxUseSkill0;

	protected FxBase _fxSkill1;

	protected ShieldBullet _pShieldBullet;

	protected ParticleSystem _psBody;

	private EventManager.StageCameraFocus stageCameraFocus;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch105_skill_01_step1_stand", "ch105_skill_01_step1_jump", "ch105_skill_01_step1_crouch", "ch105_skill_01_step2_stand", "ch105_skill_01_step2_jump", "ch105_skill_01_step2_crouch" };
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_psBody = OrangeBattleUtility.FindChildRecursive(ref target, "Meash_FX", true).GetComponent<ParticleSystem>();
		if ((bool)_psBody)
		{
			_psBody.Stop(true);
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_summondemon_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_summondemon_001", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch105sill_000", 5);
		stageCameraFocus = new EventManager.StageCameraFocus();
		stageCameraFocus.bLock = true;
		stageCameraFocus.bRightNow = true;
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
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
		int n_CONDITION_ID = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		if (_pShieldBullet != null)
		{
			if (_pShieldBullet.bIsEnd)
			{
				_pShieldBullet = null;
			}
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID) && _pShieldBullet == null)
		{
			CreateShieldBullet(_refEntity.PlayerSkills[1]);
		}
		if (!_refEntity.IsAnimateIDChanged())
		{
			UpdateSkill();
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.PlayerSkills[0].Reload_index == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
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

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.PlayerSkills[0].Reload_index == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill0(id);
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
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				_refEntity.Animator._animator.speed = 2f;
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
				ToggleWeapon(0);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_3:
			case OrangeCharacter.SubStatus.SKILL0_4:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
			case OrangeCharacter.SubStatus.SKILL0_5:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SkillEndChnageToIdle(true);
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.AimTransform, wsSkill.SkillLV, _vSkill0ShootDirection);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_4:
		case OrangeCharacter.SubStatus.SKILL0_5:
			if (wsSkill.Reload_index == 1)
			{
				_refEntity.BulletCollider.UpdateBulletData(wsSkill.FastBulletDatas[wsSkill.Reload_index], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = wsSkill.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.FastBulletDatas[wsSkill.Reload_index], wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				_refEntity.RemoveComboSkillBuff(wsSkill.FastBulletDatas[wsSkill.Reload_index].n_ID);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
		{
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
			int n_CONDITION_ID = wsSkill.BulletData.n_CONDITION_ID;
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
			{
				CreateShieldBullet(wsSkill);
			}
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			break;
		}
		}
	}

	protected void CreateShieldBullet(WeaponStruct wsSkill)
	{
		_pShieldBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ShieldBullet>(wsSkill.BulletData.s_MODEL);
		_pShieldBullet.UpdateBulletData(wsSkill.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		_pShieldBullet.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_pShieldBullet.BulletLevel = wsSkill.SkillLV;
		_pShieldBullet.BindBuffId(wsSkill.BulletData.n_CONDITION_ID, _refEntity.IsLocalPlayer);
		_pShieldBullet.Active(_refEntity.transform, Quaternion.identity, _refEntity.TargetMask, true);
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
	}

	public override void ControlCharacterDead()
	{
		if ((bool)_psBody)
		{
			_psBody.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	public override void ControlCharacterContinue()
	{
		if ((bool)_psBody)
		{
			_psBody.Play(true);
		}
	}

	public void StageTeleportOutCharacterDepend()
	{
		if ((bool)_psBody)
		{
			_psBody.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	public void StageTeleportInCharacterDepend()
	{
		if ((bool)_psBody)
		{
			_psBody.Play(true);
		}
	}

	public void TeleportInCharacterDepend()
	{
		if ((bool)_psBody)
		{
			_psBody.Play(true);
		}
	}

	public void TeleportInExtraEffect()
	{
		if ((bool)_psBody)
		{
			_psBody.Play(true);
		}
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length != 2)
		{
			return;
		}
		int num = (int)parameters[0];
		int num2 = (int)parameters[1];
		if (num == 0 && _refEntity is OrangeConsoleCharacter)
		{
			OrangeConsoleCharacter orangeConsoleCharacter = _refEntity as OrangeConsoleCharacter;
			if (_refEntity.PlayerSkills[0].Reload_index == 0)
			{
				orangeConsoleCharacter.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, true);
				return;
			}
			orangeConsoleCharacter.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, false);
			orangeConsoleCharacter.ClearVirtualButtonStick(VirtualButtonId.SKILL0);
		}
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		int n_CONDITION_ID = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(n_CONDITION_ID);
		}
		if (_pShieldBullet != null)
		{
			_pShieldBullet.BackToPool();
			_pShieldBullet = null;
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	private void UpdateSkill()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.15f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.4f)
			{
				SkipSkill1Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_4:
		case OrangeCharacter.SubStatus.SKILL0_5:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.15f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.4f)
			{
				SkipSkill1Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.15f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.4f)
			{
				SkipSkill1Animation();
			}
			break;
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
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		if (_refEntity.PlayerSkills[skillId].Reload_index == 1)
		{
			PlayVoiceSE("v_dr_skill01_2");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch105sill_000", _refEntity.transform.position, OrangeBattleUtility.QuaternionNormal, Array.Empty<object>());
			PerBuff perBuff = null;
			StageObjBase stageObjBase = null;
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				if (StageUpdate.runPlayers[num].selfBuffManager.CheckHasMarkedEffect(115, _refEntity.sNetSerialID, out perBuff))
				{
					if (perBuff.sPlayerID == _refEntity.sPlayerID && !StageUpdate.runPlayers[num].IsDead())
					{
						stageObjBase = StageUpdate.runPlayers[num];
						break;
					}
					perBuff = null;
				}
			}
			if (perBuff != null && stageObjBase != null)
			{
				_refEntity.Controller.LogicPosition = new VInt3(stageObjBase._transform.position);
				_refEntity._transform.position = stageObjBase._transform.position;
				if (_refEntity.IsLocalPlayer)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
				}
			}
			TurnToAimTarget();
			_fxSkill1 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_summondemon_001", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_5);
			}
			else if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_4);
			}
		}
		else
		{
			PlayVoiceSE("v_dr_skill01_1");
			if (_refEntity.UseAutoAim)
			{
				TurnToAimTarget();
			}
			_vSkill0ShootDirection = _refEntity.ShootDirection;
			_fxUseSkill0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_summondemon_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
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
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		if ((bool)_fxUseSkill0 && _fxUseSkill0.gameObject.activeSelf)
		{
			_fxUseSkill0.BackToPool();
			_fxUseSkill0 = null;
		}
		if (!_refEntity.BulletCollider.bIsEnd)
		{
			_refEntity.BulletCollider.BackToPool();
			if ((bool)_fxSkill1 && _fxSkill1.gameObject.activeSelf)
			{
				_fxSkill1.BackToPool();
				_fxSkill1 = null;
			}
		}
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		SkillEndChnageToIdle();
	}

	private void UseSkill1(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		TurnToAimTarget();
		PlayVoiceSE("v_dr_skill02");
		PlaySkillSE("dr_refus01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch105sill_000", _refEntity.transform.position, OrangeBattleUtility.QuaternionNormal, Array.Empty<object>());
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

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5))
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

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.GravityMultiplier = new VInt(1f);
		_refEntity.Animator._animator.speed = 1f;
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
}
