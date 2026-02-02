using System;
using StageLib;
using UnityEngine;

public class CH061_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected OrangeTimer skill0Timer;

	private int nLastReloadIndex0;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch061_skill_02_stand", "ch061_skill_02_jump", "ch061_skill_02_crouch" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch061_skill_01_stand_up", "ch061_skill_01_stand_mid", "ch061_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch061_skill_01_jump_up", "ch061_skill_01_jump_mid", "ch061_skill_01_jump_down" };
		string[] array3 = new string[3] { "ch061_skill_01_crouch_up", "ch061_skill_01_crouch_mid", "ch061_skill_01_crouch_down" };
		return new string[3][] { array, array2, array3 };
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
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "GunMesh_m");
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		skill0Timer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch061_skill_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch061_skill_001", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
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
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
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
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				_refEntity.Animator._animator.speed = 1.3f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				_refEntity.Animator._animator.speed = 1.3f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				_refEntity.Animator._animator.speed = 1.3f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				_refEntity.Animator._animator.speed = 1.3f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				_refEntity.Animator._animator.speed = 1.3f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				_refEntity.Animator._animator.speed = 1.3f;
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
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

	private void ForceUpdateSkillDirection(Transform shootTransform)
	{
		if (_refEntity.IAimTargetLogicUpdate != null && shootTransform != null)
		{
			Vector3? vector = _refEntity.CalibrateAimDirection(shootTransform.position, _refEntity.IAimTargetLogicUpdate);
			if (vector.HasValue)
			{
				_refEntity._characterDirection = ((vector.Value.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
				_refEntity.UpdateDirection();
			}
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
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
			skill0Timer.TimerStart();
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_4:
		case OrangeCharacter.SubStatus.SKILL0_5:
		{
			SKILL_TABLE sKILL_TABLE = wsSkill.FastBulletDatas[nLastReloadIndex0];
			int n_CONDITION_ID = wsSkill.BulletData.n_CONDITION_ID;
			if (_refEntity.IsLocalPlayer)
			{
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					OrangeCharacter orangeCharacter = StageUpdate.runPlayers[num];
					PerBuff perBuff = null;
					if (orangeCharacter != null && !orangeCharacter.IsDead() && orangeCharacter.selfBuffManager != null && orangeCharacter.selfBuffManager.CheckHasMarkedEffect(117, _refEntity.sPlayerID, out perBuff) && perBuff.nBuffID == n_CONDITION_ID && perBuff.sPlayerID == _refEntity.sPlayerID)
					{
						if (!orangeCharacter.selfBuffManager.CheckBuffIdHasNotRemovable(n_CONDITION_ID))
						{
							orangeCharacter.selfBuffManager.RemoveMarkedEffect(perBuff.nBuffID, perBuff.sPlayerID, true);
						}
						_refEntity.PushBulletDetail(sKILL_TABLE, wsSkill.weaponStatus, orangeCharacter.AimPosition, wsSkill.SkillLV);
					}
				}
				for (int num2 = StageUpdate.runEnemys.Count - 1; num2 >= 0; num2--)
				{
					EnemyControllerBase mEnemy = StageUpdate.runEnemys[num2].mEnemy;
					PerBuff perBuff2 = null;
					if (mEnemy != null && mEnemy.selfBuffManager != null && mEnemy.selfBuffManager.CheckHasMarkedEffect(117, _refEntity.sPlayerID, out perBuff2) && perBuff2.nBuffID == n_CONDITION_ID && perBuff2.sPlayerID == _refEntity.sPlayerID)
					{
						if (!mEnemy.selfBuffManager.CheckBuffIdHasNotRemovable(n_CONDITION_ID))
						{
							mEnemy.selfBuffManager.RemoveMarkedEffect(perBuff2.nBuffID, perBuff2.sPlayerID, true);
						}
						_refEntity.PushBulletDetail(sKILL_TABLE, wsSkill.weaponStatus, mEnemy.AimPosition, wsSkill.SkillLV);
					}
				}
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch061_skill_001", _refEntity.ExtraTransforms[0].position, Quaternion.identity, Array.Empty<object>());
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
			ForceUpdateSkillDirection(_refEntity.ModelTransform);
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV, Vector3.right * _refEntity.direction);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ModelTransform);
			break;
		}
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch061_startin_000";
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
		if (_refEntity.PlayerSkills[0].Reload_index > 0 && (float)skill0Timer.GetMillisecond() > _refEntity.PlayerSkills[0].BulletData.f_COMBO * 1000f)
		{
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[0].ComboCheckDatas[0].nComboSkillID);
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
			else if (bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0))
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
			else if (bInSkill && _refEntity.CurrentFrame > 0.1f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (!bInSkill && CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.3f)
			{
				SkipSkill0Animation();
			}
			break;
		}
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		nLastReloadIndex0 = _refEntity.PlayerSkills[skillId].Reload_index;
		ToggleWeapon(1);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch061_skill_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		int num = 0;
		if (_refEntity.PlayerSkills[skillId].Reload_index == 1)
		{
			_refEntity.IsShoot = 0;
			Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
			if (vector.HasValue)
			{
				int num2 = Math.Sign(vector.Value.x);
				if (_refEntity._characterDirection != (CharacterDirection)num2 && Mathf.Abs(vector.Value.x) > 0.05f)
				{
					_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				}
			}
			num = 3;
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
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_4))
		{
			SkillEndChnageToIdle();
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5))
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
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
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
		bInSkill = false;
		_refEntity.Animator._animator.speed = 1f;
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
		if (bDebut)
		{
			ToggleWeapon(-1);
		}
		else
		{
			ToggleWeapon(-2);
		}
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
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
			_tfWeaponMesh.enabled = true;
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
