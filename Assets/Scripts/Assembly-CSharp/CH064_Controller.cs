using System;
using UnityEngine;

public class CH064_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfBackBoxMesh;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected SkinnedMeshRenderer _tfBusterMesh;

	protected FxBase _fxVM;

	protected Vector3 _vVMOffset = new Vector3(1.5f, 0f, 0f);

	protected Vector3 _vSkill0ShootDir = Vector3.right;

	private int nLastSkill0ReloadIndex;

	private OrangeCharacter.MainStatus lastmainStatus = OrangeCharacter.MainStatus.NONE;

	private OrangeCharacter.SubStatus lastsubStatus = OrangeCharacter.SubStatus.NONE;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[9] { "ch064_skill_01_kick_stand", "ch064_skill_01_kick_jump", "ch064_skill_01_kick_crouch", "ch064_skill_01_drink_stand", "ch064_skill_01_drink_jump", "ch064_skill_01_drink_crouch", "ch064_skill_02_stand", "ch064_skill_02_jump", "ch064_skill_02_crouch" };
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
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BackBoxMesh_m");
		_tfBackBoxMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfBackBoxMesh.enabled = true;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "FreeBoxMesh_m");
		_tfWeaponMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m");
		_tfBusterMesh = transform3.GetComponent<SkinnedMeshRenderer>();
		_tfBusterMesh.enabled = false;
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_vm_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_vmkick_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_suitcase_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch064drink_000", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
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
		if (id == 1 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill1(id);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill0(id);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus mainStatus2 = lastmainStatus;
		if (mainStatus2 == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus subStatus2 = lastsubStatus;
			if ((uint)(subStatus2 - 19) <= 2u && (mainStatus != lastmainStatus || subStatus != lastsubStatus) && _fxVM != null)
			{
				_fxVM.BackToPool();
				_fxVM = null;
			}
		}
		lastmainStatus = mainStatus;
		lastsubStatus = subStatus;
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				DebutOrClearStageToggleWeapon(false);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				DebutOrClearStageToggleWeapon(false);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				_refEntity.IgnoreGravity = true;
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				_refEntity.Animator._animator.speed = 1.5f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.Animator._animator.speed = 2f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.Animator._animator.speed = 2f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.Animator._animator.speed = 2f;
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
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
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				SkillEndChnageToIdle(true);
				break;
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
			if (_fxVM != null)
			{
				_fxVM.BackToPool();
				_fxVM = null;
			}
			if (_refEntity._characterDirection == CharacterDirection.RIGHT)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_vmkick_000", _refEntity.ModelTransform.position + new Vector3(1.5f, 1f, 0f), Quaternion.identity, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_vmkick_000", _refEntity.ModelTransform.position + new Vector3(-1.5f, 1f, 0f), Quaternion.identity, Array.Empty<object>());
			}
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform.position + _vVMOffset * (float)_refEntity._characterDirection, wsSkill.SkillLV, _vSkill0ShootDir, false, 1);
			PlayVoiceSE("v_rv_skill03");
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_4:
		case OrangeCharacter.SubStatus.SKILL0_5:
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch064drink_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			SKILL_TABLE bulletData = wsSkill.FastBulletDatas[nLastSkill0ReloadIndex];
			_refEntity.PushBulletDetail(bulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], null, nLastSkill0ReloadIndex);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			PlayVoiceSE("v_rv_skill04");
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ModelTransform);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			PlayVoiceSE("v_rv_skill01");
			break;
		}
	}

	public void StageTeleportOutCharacterDepend()
	{
		if ((bool)_tfBackBoxMesh)
		{
			_tfBackBoxMesh.enabled = false;
		}
	}

	public void StageTeleportInCharacterDepend()
	{
		if ((bool)_tfBackBoxMesh)
		{
			_tfBackBoxMesh.enabled = true;
		}
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch064_startin_000";
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
			if (_refEntity.PlayerSkills[0].Reload_index == 1)
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
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.25f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.5f)
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_4:
		case OrangeCharacter.SubStatus.SKILL0_5:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.25f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.3f)
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
			else if (bInSkill && _refEntity.CurrentFrame > 0.25f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (!bInSkill && CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.7f)
			{
				SkipSkill0Animation();
			}
			break;
		}
	}

	private void TurnToAimTarget()
	{
		Vector3 vSkill0ShootDir = _refEntity.ShootDirection;
		if (_refEntity.UseAutoAim)
		{
			Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
			if (vector.HasValue)
			{
				vSkill0ShootDir = vector.Value;
			}
		}
		int num = Math.Sign(vSkill0ShootDir.x);
		if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(vSkill0ShootDir.x) > 0.05f)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
		_vSkill0ShootDir = vSkill0ShootDir;
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		int num = 0;
		nLastSkill0ReloadIndex = _refEntity.PlayerSkills[skillId].Reload_index;
		if (_fxVM != null)
		{
			_fxVM.BackToPool();
			_fxVM = null;
		}
		if (nLastSkill0ReloadIndex == 1)
		{
			num += 3;
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[skillId].FastBulletDatas[nLastSkill0ReloadIndex].n_ID);
		}
		else
		{
			TurnToAimTarget();
			_refEntity.IsShoot = 0;
			Vector3 vector = _refEntity.ModelTransform.position + _vVMOffset * (float)_refEntity._characterDirection;
			if (_refEntity.IsLocalPlayer)
			{
				_fxVM = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_vm_000", vector, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			if (_refEntity.UseAutoAim)
			{
				if (_refEntity.PlayerAutoAimSystem == null || _refEntity.PlayerAutoAimSystem.AutoAimTarget == null)
				{
					_vSkill0ShootDir = _refEntity.ShootDirection;
				}
				else
				{
					Vector3 vector2 = Vector3.zero;
					if (_refEntity.PlayerAutoAimSystem.AutoAimTarget as OrangeCharacter != null)
					{
						vector2 = (_refEntity.PlayerAutoAimSystem.AutoAimTarget as OrangeCharacter).AimExtendPosition;
					}
					_vSkill0ShootDir = (_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition - vector2 - vector).normalized;
				}
			}
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.ModelTransform, null, 0);
		}
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, (OrangeCharacter.SubStatus)(21 + num));
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, (OrangeCharacter.SubStatus)(19 + num));
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, (OrangeCharacter.SubStatus)(20 + num));
		}
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1)
		{
			SkillEndChnageToIdle();
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
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
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 0;
		ToggleWeapon(2);
		TurnToAimTarget();
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_suitcase_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.PlayerStopDashing();
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
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
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

	private void DebutOrClearStageToggleWeapon(bool bDebut)
	{
		ToggleWeapon(-1);
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfBackBoxMesh.enabled = true;
			_tfWeaponMesh.enabled = false;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfBackBoxMesh.enabled = true;
			_tfWeaponMesh.enabled = false;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfBackBoxMesh.enabled = false;
			_tfWeaponMesh.enabled = true;
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfBackBoxMesh.enabled = true;
			_tfWeaponMesh.enabled = false;
			break;
		}
	}
}
