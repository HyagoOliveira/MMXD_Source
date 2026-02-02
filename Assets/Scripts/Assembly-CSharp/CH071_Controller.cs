using System;
using UnityEngine;

public class CH071_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected Vector2 _RisingSpeed;

	protected int _RisingEndFrame;

	protected FxBase _fxRigsing;

	[SerializeField]
	private float _RisingTime = 0.4f;

	private readonly int hashDirection = Animator.StringToHash("fDirection");

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch071_skill_02_start", "ch071_skill_02_loop", "ch071_skill_02_end" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[4] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[4] { "ch071_skill_01_stand", "ch071_skill_01_fall", "ch071_skill_01_wallgrab", "ch071_skill_01_crouch" };
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
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m");
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "HandMesh_L_m");
		_refEntity._handMesh = new SkinnedMeshRenderer[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_refEntity._handMesh[i] = array[i].GetComponent<SkinnedMeshRenderer>();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_risingfire_000", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
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
		if (id == 1 && _refEntity.CurrentActiveSkill != id)
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
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.IgnoreGravity = false;
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
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetSpeed(0, 0);
				_refEntity.BulletCollider.BackToPool();
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
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL1)
			{
				_refEntity.BulletCollider.UpdateBulletData(wsSkill.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				_refEntity.BulletCollider.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = wsSkill.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_fxRigsing = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_risingfire_000", _refEntity.ModelTransform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ModelTransform);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
			}
		}
	}

	private void UpdateSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.CheckSkillEndByShootTimer();
			if (_refEntity.SkillEnd)
			{
				ToggleWeapon(0);
			}
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL1:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.5f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				_RisingSpeed = new Vector2((int)_refEntity._characterDirection * 5000, 12000f);
				_RisingEndFrame = GameLogicUpdateManager.GameFrame + (int)(_RisingTime / GameLogicUpdateManager.m_fFrameLen);
				_refEntity.SetSpeed((int)_RisingSpeed.x, (int)_RisingSpeed.y);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (GameLogicUpdateManager.GameFrame >= _RisingEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if ((CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.2f) || _refEntity.Controller.Collisions.below)
			{
				SkipSkill1Animation();
			}
			break;
		}
	}

	private void UseSkill0(int skillId)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		ToggleWeapon(1);
		_refEntity.CurrentActiveSkill = 0;
		_refEntity.IsShoot = 3;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
		_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.Animator.SetAnimatorEquip(1);
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, _refEntity.ShootDirection)) / 180f;
		_refEntity.Animator._animator.SetFloat(hashDirection, value);
		PlayVoiceSE("v_x_skill03");
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		_refEntity.CancelBusterChargeAtk();
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		SkillEndChnageToIdle();
	}

	private void UseSkill1(int skillId)
	{
		if ((_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(skillId))
		{
			bInSkill = true;
			_refEntity.CurrentActiveSkill = skillId;
			_refEntity.SkillEnd = false;
			_refEntity.SetSpeed(0, 0);
			ToggleWeapon(2);
			UpdateDirection();
			_RisingSpeed = Vector2.zero;
			_refEntity.PlayerStopDashing();
			_refEntity.StartJumpThroughCorutine();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		_refEntity.SetSpeed(0, 0);
		_refEntity.BulletCollider.BackToPool();
		SkillEndChnageToIdle();
	}

	private void UpdateDirection()
	{
		if (_refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
		{
			int num = Math.Sign((_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition - _refEntity._transform.position).x);
			_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
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
				_refEntity.EnableHandMesh(true);
			}
			_tfWeaponMesh.enabled = false;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				_refEntity.EnableHandMesh(false);
			}
			_tfWeaponMesh.enabled = true;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				_refEntity.EnableHandMesh(false);
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
