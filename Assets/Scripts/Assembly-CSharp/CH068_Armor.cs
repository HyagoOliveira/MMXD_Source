using System;
using OrangeAudio;
using UnityEngine;

public class CH068_Armor : ArmorBase
{
	private enum ArmorSkill
	{
		SPAWN = 63,
		PUNCH = 64,
		PUNCH_END = 65,
		SKILL_01_START = 66,
		SKILL_01_END = 67,
		SKILL_02_START = 68,
		SKILL_02_LOOP = 69,
		SKILL_02_END = 70
	}

	[SerializeField]
	private Transform fxSkl1Transform;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private bool IsBossPose;

	private readonly int SKL_SPAWN_END = (int)(0.52f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_PUNCH_START_TRIGGER = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_PUNCH_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_PUNCH_END_END = (int)(0.2776f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_0_START_END = (int)(0.16f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_0_END_END = (int)(0.389f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_0_END_BREAK = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_1_START_END = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_1_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_1_END_END = (int)(0.3534f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_SKL_1_DURING_001 = "fxduring_vava_mk2_001";

	private readonly string FX_SKL_1_DURING_002 = "fxduring_vava_mk2_002";

	private readonly string FX_SKL_1_DURING_003 = "fxduring_vava_mk2_003";

	private readonly string FX_SKL_0_USE = "fxuse_mk2_003";

	private readonly string ARMOSE_ACB = "BattleSE";

	protected override void Awake()
	{
		InitShootTransforms();
		InitSkillFx();
		base.Awake();
	}

	private void InitSkillFx()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_SKL_0_USE);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_SKL_1_DURING_001);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_SKL_1_DURING_002);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_SKL_1_DURING_003);
	}

	private void InitShootTransforms()
	{
		if (shootTransforms[0] == null)
		{
			shootTransforms[0] = base.transform.Find("ShootTransform0");
			if (shootTransforms[0] == null)
			{
				shootTransforms[0] = base.transform;
			}
		}
		if (shootTransforms[1] == null)
		{
			shootTransforms[1] = base.transform.Find("ShootTransform1");
			if (shootTransforms[1] == null)
			{
				shootTransforms[1] = base.transform;
			}
		}
		if (shootTransforms[2] == null)
		{
			shootTransforms[2] = base.transform.Find("Sub_Tron_ShootPoint2");
			if (shootTransforms[2] == null)
			{
				shootTransforms[2] = base.transform;
			}
		}
		if (shootTransforms[3] == null)
		{
			shootTransforms[3] = base.transform.Find("ShootTransform3");
			if (shootTransforms[3] == null)
			{
				shootTransforms[3] = base.transform;
			}
		}
	}

	protected override void InitAnimateHash()
	{
		dictAnimateHash.Clear();
		int value = Animator.StringToHash("ch068_veh_idle_loop");
		foreach (HumanBase.AnimateId value2 in Enum.GetValues(typeof(HumanBase.AnimateId)))
		{
			dictAnimateHash.Add(value2, value);
		}
		dictAnimateHash[HumanBase.AnimateId.ANI_JUMP] = Animator.StringToHash("ch068_veh_jump_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_FALL] = Animator.StringToHash("ch068_veh_fall");
		dictAnimateHash[HumanBase.AnimateId.ANI_LAND] = Animator.StringToHash("ch068_veh_landing");
		dictAnimateHash[HumanBase.AnimateId.ANI_STEP] = Animator.StringToHash("ch068_veh_move_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALK] = Animator.StringToHash("ch068_veh_move_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALKBACK] = Animator.StringToHash("ch068_veh_move_loop_back");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASH] = Animator.StringToHash("ch068_veh_dash_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASH_END] = Animator.StringToHash("ch068_veh_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_SLIDE] = Animator.StringToHash("ch068_veh_dash_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_SLIDE_END] = Animator.StringToHash("ch068_veh_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_AIRDASH_END] = Animator.StringToHash("ch068_veh_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_WIN_POSE] = Animator.StringToHash("ch068_veh_logout1");
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE] = Animator.StringToHash("ch068_veh_logout");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_STAND_START] = Animator.StringToHash("ch068_veh_dive_trigger_stand_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_JUMP_START] = Animator.StringToHash("ch068_veh_dive_trigger_jump_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_STAND_END] = Animator.StringToHash("ch068_veh_dive_trigger_stand_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_JUMP_END] = Animator.StringToHash("ch068_veh_dive_trigger_jump_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_SKILL_START] = Animator.StringToHash("ch068_veh_atk0_start");
		dictAnimateHash[(HumanBase.AnimateId)66u] = Animator.StringToHash("ch068_veh_atk0_lefthand_end");
		dictAnimateHash[(HumanBase.AnimateId)67u] = Animator.StringToHash("ch068_veh_atk2_start");
		dictAnimateHash[(HumanBase.AnimateId)68u] = Animator.StringToHash("ch068_veh_atk2_end");
		dictAnimateHash[(HumanBase.AnimateId)69u] = Animator.StringToHash("ch068_veh_atk1_start");
		dictAnimateHash[(HumanBase.AnimateId)70u] = Animator.StringToHash("ch068_veh_atk1_loop");
		dictAnimateHash[(HumanBase.AnimateId)71u] = Animator.StringToHash("ch068_veh_atk1_end");
		dictAnimateHash[(HumanBase.AnimateId)72u] = Animator.StringToHash("ch068_veh_login_down");
		dictAnimateHash[(HumanBase.AnimateId)73u] = Animator.StringToHash("ch068_veh_login_jump");
		dictAnimateHash[(HumanBase.AnimateId)74u] = Animator.StringToHash("ch068_veh_win");
	}

	protected override void SetLinkAnimationAndStatus()
	{
		PlayAnimation((HumanBase.AnimateId)72u, (HumanBase.AnimateId)73u);
		SetLogicFrame(SKL_SPAWN_END, SKL_SPAWN_END);
		_refEntity.UpdateAimRangeByWeapon(dictSkill[ButtonId.SKILL0].WeaponStruct);
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_14);
	}

	protected override void ResetLastStatus()
	{
		if (_refEntity.IsInGround)
		{
			_refEntity.IgnoreGravity = false;
			_refEntity.Dashing = false;
			_refEntity.PlayerStopDashing();
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.WALK, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerStopDashing();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
		}
		else
		{
			if (_refEntity.IgnoreGravity)
			{
				_refEntity.IgnoreGravity = false;
			}
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
		isSkillEventEnd = false;
		_refEntity.BulletCollider.BackToPool();
		_refEntity.IsShoot = 0;
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void PlayerHeldShoot()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && curMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.CurrentActiveSkill == -1 && CanUseSkl(ButtonId.SHOOT))
		{
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			SetLogicFrame(SKL_PUNCH_START_TRIGGER, SKL_PUNCH_START_END);
			_refEntity.CurrentActiveSkill = 1;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_15);
			PlayAnimation(HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && curMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.CurrentActiveSkill == -1 && id == 1 && CanUseSkl(ButtonId.SKILL1, false, false))
		{
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			SetLogicFrame(SKL_1_START_END, SKL_1_START_END);
			_refEntity.CurrentActiveSkill = id;
			bulletDirection = new Vector3(_refEntity.direction, 0f, 0f);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_19);
			PlayAnimation((HumanBase.AnimateId)69u, (HumanBase.AnimateId)69u);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && curMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.CurrentActiveSkill == -1 && id == 0 && CanUseSkl(ButtonId.SKILL0))
		{
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.CurrentActiveSkill = id;
			SetLogicFrame(SKL_0_START_END, SKL_0_START_END);
			UpdateEntityDirection();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_17);
			PlayAnimation((HumanBase.AnimateId)67u, (HumanBase.AnimateId)67u);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_SKL_0_USE, shootTransforms[1], Quaternion.identity, Array.Empty<object>());
		}
	}

	public override void CheckSkill()
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (!base.IsLink || _refEntity.IsAnimateIDChanged())
		{
			return;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch ((ArmorSkill)_refEntity.CurSubStatus)
		{
		case ArmorSkill.SPAWN:
			if (nowFrame >= endFrame)
			{
				ResetLastStatus();
			}
			break;
		case ArmorSkill.PUNCH:
			if (nowFrame >= endFrame)
			{
				SetLogicFrame(SKL_PUNCH_END_END, SKL_PUNCH_END_END);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_16);
				PlayAnimation((HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				CreateSkillBullet(ButtonId.SHOOT, shootTransforms[0], _refEntity.PlayerSkills[0].SkillLV);
			}
			break;
		case ArmorSkill.PUNCH_END:
			if (nowFrame >= endFrame)
			{
				ResetLastStatus();
			}
			break;
		case ArmorSkill.SKILL_01_START:
			if (nowFrame >= endFrame)
			{
				SetLogicFrame(SKL_0_END_END, SKL_0_END_END);
				endBreakFrame = nowFrame + SKL_0_END_BREAK;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_18);
				bulletDirection = MonoBehaviourSingleton<OrangeBattleUtility>.Instance.GetShootDirection(_refEntity, shootTransforms[1]);
				CreateSkillBullet(ButtonId.SKILL0, shootTransforms[1], _refEntity.PlayerSkills[0].SkillLV);
				PlayAnimation((HumanBase.AnimateId)68u, (HumanBase.AnimateId)68u);
			}
			break;
		case ArmorSkill.SKILL_01_END:
			if (nowFrame >= endFrame)
			{
				ResetLastStatus();
			}
			else if (nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case ArmorSkill.SKILL_02_START:
			if (nowFrame >= endFrame)
			{
				if (_refEntity.Dashing)
				{
					_refEntity.PlayerStopDashing();
				}
				_refEntity.SetSpeed((int)_refEntity._characterDirection * OrangeCharacter.WalkSpeed, (int)((float)OrangeCharacter.JumpSpeed * 0.5f));
				_refEntity.StopShootTimer();
				WeaponStruct weaponStruct = dictSkill[ButtonId.SKILL1].WeaponStruct;
				SKILL_TABLE bulletData = weaponStruct.BulletData;
				_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 2.5f), 0);
				_refEntity.BulletCollider.UpdateBulletData(bulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[0].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.CheckUsePassiveSkill(0, bulletData, weaponStruct.weaponStatus, shootTransforms[2]);
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				SetLogicFrame(SKL_1_LOOP_END, SKL_1_LOOP_END);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_20);
				PlayAnimation((HumanBase.AnimateId)70u, (HumanBase.AnimateId)70u);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_SKL_1_DURING_001, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_SKL_1_DURING_002, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_SKL_1_DURING_003, fxSkl1Transform, Quaternion.identity, Array.Empty<object>());
			}
			break;
		case ArmorSkill.SKILL_02_LOOP:
			if (nowFrame >= endFrame)
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.BulletCollider.BackToPool();
				SetLogicFrame(SKL_1_END_END, SKL_1_END_END);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_21);
				PlayAnimation((HumanBase.AnimateId)71u, (HumanBase.AnimateId)71u);
			}
			break;
		case ArmorSkill.SKILL_02_END:
			if (nowFrame >= endFrame)
			{
				ResetLastStatus();
			}
			break;
		}
	}

	public override void OverrideAnimator(HumanBase.AnimateId animateId)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			return;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (_refEntity.AnimateIDPrev)
			{
			case HumanBase.AnimateId.ANI_WIN_POSE:
				IsBossPose = true;
				animator.Play(dictAnimateHash[(HumanBase.AnimateId)74u], 0);
				break;
			case HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE:
			case HumanBase.AnimateId.ANI_LOGOUT2:
				if (IsBossPose)
				{
					animator.Play(dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE], 0);
				}
				else
				{
					animator.Play(dictAnimateHash[HumanBase.AnimateId.ANI_WIN_POSE], 0);
				}
				break;
			}
			return;
		}
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			animator.SetFloat(hashVelocityY, _refEntity.Velocity.y);
			if (lastID != animateId)
			{
				animator.Play(dictAnimateHash[animateId], 0);
			}
		}
	}

	public override void PlayCharaSE(CharaSE seId)
	{
		switch (seId)
		{
		default:
			_refEntity.PlayCharaSE(seId);
			break;
		case CharaSE.JUMP:
			_refEntity.PlaySE(ARMOSE_ACB, "bt_ridearmor04");
			break;
		case CharaSE.JUMPHIGH:
			_refEntity.PlaySE(ARMOSE_ACB, "bt_ridearmor04");
			break;
		case CharaSE.STEP:
			_refEntity.PlaySE(ARMOSE_ACB, "bt_ridearmor01");
			break;
		case CharaSE.DASH:
			_refEntity.PlaySE(ARMOSE_ACB, "bt_ridearmor03");
			break;
		case CharaSE.DASHEND:
			_refEntity.PlaySE(ARMOSE_ACB, "bt_ridearmor05");
			break;
		case CharaSE.CHAKUCHI:
			_refEntity.PlaySE(ARMOSE_ACB, "bt_ridearmor02");
			break;
		}
	}
}
