using System;
using UnityEngine;

public class CH083_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected CharacterMaterial _cmWeapon;

	protected Transform _tfRHand;

	protected OrangeTimer _otDisappearTime;

	protected OrangeTimer _otSkill0Timer;

	protected FxBase _fbSkill0;

	protected Vector3 _vSkillStartPosition;

	protected Vector2 _vSkillVelocity;

	protected Vector3 _vSkill1ShootDirection;

	protected ParticleSystem fxResident;

	protected Transform _tfFxLogoutPoint;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch083_skill_01_stand_step1", "ch083_skill_01_jump_step1", "ch083_skill_01_stand_step2", "ch083_skill_01_jump_step2", "ch083_skill_02_stand", "ch083_skill_02_jump", "ch083_skill_02_crouch" };
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
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_m", true);
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = true;
		_tfRHand = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_R_m", true);
		CharacterMaterial[] components = _refEntity.ModelTransform.GetComponents<CharacterMaterial>();
		if (components.Length >= 2 && components[1] != null)
		{
			_cmWeapon = components[1];
		}
		_otSkill0Timer = OrangeTimerManager.GetTimer();
		_otDisappearTime = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_buraiart_000", 5);
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "CH083_000_FX", true);
		fxResident = transform2.GetComponent<ParticleSystem>();
		if (null != fxResident)
		{
			fxResident.Stop(true);
		}
		_tfFxLogoutPoint = OrangeBattleUtility.FindChildRecursive(ref target, "FxLogoutPoint", true);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
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
			if (_refEntity.PlayerSkills[0].Reload_index == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
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
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.PlayerSkills[0].Reload_index == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
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
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_6:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
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
			case OrangeCharacter.SubStatus.SKILL0_3:
			case OrangeCharacter.SubStatus.SKILL0_4:
				SkillEndChnageToIdle();
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
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0_2:
				wsSkill.LastUseTimer.TimerStart();
				_refEntity.BulletCollider.UpdateBulletData(wsSkill.FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				_refEntity.BulletCollider.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = wsSkill.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.BulletCollider.HitCallback = null;
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
			case OrangeCharacter.SubStatus.SKILL0_6:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[wsSkill.Reload_index], wsSkill.weaponStatus, _refEntity.AimTransform, wsSkill.SkillLV, _vSkill1ShootDirection);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0], null, 1);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				_refEntity.RemoveComboSkillBuff(wsSkill.FastBulletDatas[wsSkill.Reload_index].n_ID);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[1]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) > 4u)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public void TeleportOutCharacterDepend()
	{
		if (fxResident != null && _tfFxLogoutPoint != null)
		{
			fxResident.transform.parent = _tfFxLogoutPoint;
			fxResident.transform.localPosition = Vector3.zero;
		}
		if (_otDisappearTime.GetMillisecond() > 300)
		{
			_otDisappearTime.TimerStop();
			if (_tfRHand != null)
			{
				_tfRHand.gameObject.SetActive(false);
			}
		}
	}

	public void StageTeleportOutCharacterDepend()
	{
		if (fxResident != null && _tfFxLogoutPoint != null)
		{
			fxResident.gameObject.SetActive(false);
		}
		if (_tfRHand != null)
		{
			_tfRHand.gameObject.SetActive(true);
			_otDisappearTime.TimerStart();
		}
		if (_cmWeapon != null)
		{
			_cmWeapon.Disappear();
		}
	}

	public void StageTeleportInCharacterDepend()
	{
		if (fxResident != null && _tfFxLogoutPoint != null)
		{
			fxResident.gameObject.SetActive(true);
			fxResident.Play(true);
		}
	}

	public void TeleportInCharacterDepend()
	{
		if (fxResident != null && !fxResident.isPlaying)
		{
			fxResident.Play(true);
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
				orangeConsoleCharacter.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, false);
				orangeConsoleCharacter.ClearVirtualButtonStick(VirtualButtonId.SKILL0);
			}
			else
			{
				orangeConsoleCharacter.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, true);
			}
		}
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
			if (_refEntity.CurrentFrame > 0.15f)
			{
				_fbSkill0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_buraiart_000", _refEntity.ExtraTransforms[1], OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_vSkillVelocity = Vector3.right * OrangeCharacter.DashSpeed * 4f * _refEntity.direction;
				_vSkillStartPosition = _refEntity.AimPosition;
				_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (Vector2.Distance(_vSkillStartPosition, _refEntity.AimPosition) > _refEntity.PlayerSkills[0].BulletData.f_DISTANCE || _refEntity.PlayerSkills[0].LastUseTimer.GetMillisecond() > 500)
			{
				_otSkill0Timer.TimerStart();
				_refEntity.BulletCollider.BackToPool();
				_refEntity.SetSpeed(OrangeCharacter.WalkSpeed * _refEntity.direction, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (CheckCancelAnimate(0))
			{
				_refEntity.IsShoot = 0;
				SkipSkill0Animation();
				break;
			}
			if (_otSkill0Timer.GetMillisecond() < 400)
			{
				int x = Mathf.RoundToInt(OrangeCharacter.WalkSpeed - OrangeCharacter.WalkSpeed / 400 * _otSkill0Timer.GetMillisecond()) * _refEntity.direction;
				_refEntity.SetSpeed(x, 0);
				break;
			}
			_refEntity.IgnoreGravity = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.IsShoot = 0;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_4);
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (CheckCancelAnimate(0))
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_5:
		case OrangeCharacter.SubStatus.SKILL0_6:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.2f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.3f)
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.2f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.35f)
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
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
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
			if (_refEntity.UseAutoAim)
			{
				TurnToAimTarget();
			}
			_vSkill1ShootDirection = _refEntity.ShootDirection;
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_5);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_6);
			}
		}
		else
		{
			PlaySkillSE("bu_buraiarts01_1");
			_refEntity.IgnoreGravity = true;
			if (_refEntity.Controller.Collisions.below)
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
		if (!_refEntity.BulletCollider.bIsEnd)
		{
			_refEntity.BulletCollider.BackToPool();
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
		PlaySkillSE("bu_buraibrake01");
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
			_tfWeaponMesh.enabled = true;
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfWeaponMesh.enabled = false;
			break;
		}
	}
}
