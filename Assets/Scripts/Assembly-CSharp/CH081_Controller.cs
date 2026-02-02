using System;
using UnityEngine;

public class CH081_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected ChargeShootObj _refChargeShootObj;

	[Header("空氣爆破")]
	protected Vector3 Skill1Directioon = Vector3.right;

	protected Vector3 Skill1Position = Vector3.zero;

	[SerializeField]
	protected float Skill1FireSpeed = 0.6f;

	[SerializeField]
	protected float Skill1Distance = 1f;

	protected int Skill1ShootCount;

	protected OrangeTimer Skill1Timer;

	protected ParticleSystem LBusterFireFX;

	private readonly int hashDirection = Animator.StringToHash("fDirection");

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch081_skill_02_stand", "ch081_skill_02_jump", "ch081_skill_02_crouch" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[3] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_crouch_charge_atk" };
		target = new string[3] { "ch081_skill_01_stand", "ch081_skill_01_fall", "ch081_skill_01_crouch" };
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		Skill1Timer = OrangeTimerManager.GetTimer();
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
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_airburst_000_L", true);
		LBusterFireFX = transform2.GetComponentInChildren<ParticleSystem>();
		LBusterFireFX.Stop();
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			CancelSkill0();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			_refEntity.SkillEnd = true;
			LBusterFireFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		_refEntity.CurrentActiveSkill = -1;
		ToggleWeapon(0);
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
		switch (id)
		{
		case 0:
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted())
				{
					_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
					_refChargeShootObj.StartCharge();
				}
				else if (_refEntity.CurrentActiveSkill == -1)
				{
					UseSkill0(id);
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				UseSkill0(id);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerStopDashing();
				_refEntity.SkillEnd = false;
				_refEntity.CurrentActiveSkill = id;
				ToggleWeapon(2);
				bInSkill = true;
				if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.WALK && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.RIDE_ARMOR)
				{
					Skill1Directioon = Vector3.right * (0 - _refEntity._characterDirection);
				}
				else
				{
					Skill1Directioon = Vector3.right * (float)_refEntity._characterDirection;
				}
				_refEntity.IsShoot = 1;
				_refEntity.ShootDirection = Skill1Directioon;
				Skill1Position = _refEntity._transform.position + Skill1Directioon;
				Skill1ShootCount = 0;
				Skill1Timer.TimerStart();
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
				PlayVoiceSE("v_rs_skill02");
				PlaySkillSE("rs_airspread01");
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
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
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) <= 2u)
		{
			_refEntity.IsShoot = 1;
			Vector3 shootPosition = Skill1Position + Skill1Directioon * (Skill1Distance + (float)Skill1ShootCount * wsSkill.BulletData.f_DISTANCE);
			_refEntity.ShootDirection = Skill1Directioon;
			_refEntity._characterDirection = ((Skill1Directioon.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, shootPosition, wsSkill.SkillLV, (float)_refEntity._characterDirection * Vector3.right);
			if (Skill1ShootCount == 0)
			{
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
			}
			Skill1ShootCount++;
			int n_NUM_SHOOT = _refEntity.PlayerSkills[1].BulletData.n_NUM_SHOOT;
			if (Skill1ShootCount >= n_NUM_SHOOT)
			{
				bInSkill = false;
				return;
			}
			if (Skill1ShootCount + 2 >= n_NUM_SHOOT)
			{
				LBusterFireFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				return;
			}
			LBusterFireFX.Simulate(0f);
			LBusterFireFX.Play();
		}
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		return _refEntity.EnterRideArmor(targetRideArmor);
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
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) > 2u)
		{
			return;
		}
		if (_refEntity.CurrentFrame > 1f)
		{
			OrangeCharacter.SubStatus curSubStatus2 = _refEntity.CurSubStatus;
			int num = 51;
			SkillEndChnageToIdle();
		}
		else if (bInSkill && _refEntity.CurrentFrame > 0.07f)
		{
			if (Skill1ShootCount < _refEntity.PlayerSkills[1].BulletData.n_NUM_SHOOT && (float)Skill1Timer.GetMillisecond() > (float)Skill1ShootCount * Skill1FireSpeed)
			{
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
		}
		else if (!bInSkill && CheckCancelAnimate(1))
		{
			bool isCrouch = _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2;
			CancelSkill1(isCrouch);
		}
	}

	private void UseSkill0(int skillId)
	{
		PlayVoiceSE("v_rs_skill01");
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		if (_refEntity.PlayerSkills[0].WeaponMesh[0] != null)
		{
			_refEntity.PlayerSkills[0].WeaponMesh[0].Disappear();
		}
		_refChargeShootObj.StopCharge();
		ToggleWeapon(1);
		_refEntity.CurrentActiveSkill = 0;
		_refEntity.IsShoot = (sbyte)(weaponStruct.ChargeLevel + 1);
		_refEntity.StartShootTimer();
		Vector3 vector = _refEntity.ShootDirection;
		if (IsDashStatus())
		{
			vector = Vector3.right * _refEntity.direction;
		}
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, vector);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct, weaponStruct.FastBulletDatas[weaponStruct.ChargeLevel].n_USE_COST, -1f);
		_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
		_refEntity.Animator.SetAnimatorEquip(1);
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, vector)) / 180f;
		_refEntity.Animator._animator.SetFloat(hashDirection, value);
	}

	private bool IsDashStatus()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if ((uint)(curMainStatus - 4) <= 1u && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			return true;
		}
		return false;
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
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 0;
		ToggleWeapon(2);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch067_skill1_000", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
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

	private void CancelSkill1(bool isCrouch)
	{
		LBusterFireFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		SkillEndChnageToIdle(isCrouch);
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
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 49) <= 2u)
			{
				_refEntity.ShootDirection = ((_refEntity._characterDirection == CharacterDirection.RIGHT) ? Vector3.right : Vector3.left);
			}
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
			_tfWeaponMesh.enabled = false;
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

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) <= 2u)
		{
			_refEntity._characterDirection = ((Skill1Directioon.x >= 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}
}
