using System;
using System.Collections.Generic;
using Better;
using OrangeAudio;
using UnityEngine;

public class CH037_Armor : ArmorBase
{
	private enum ArmorSkill
	{
		SPAWN = 0,
		SHOOT = 1,
		SKILL_01 = 2,
		SKILL_02 = 3
	}

	private int skl2CraeteCount;

	private readonly int SKL_SPAWN_END = (int)(0.98f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_SPAWN_JUMP_END = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_SKL_1_TRIGGER = (int)(0.375f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_SKL_1_END = (int)(0.88f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_LINK_01 = "fxuse_tron_skill2_000";

	private readonly string FX_LINK_02 = "fxuse_tron_skill2_002";

	private System.Collections.Generic.Dictionary<ArmorSkill, OrangeCharacter.SubStatus> dictSkillStatus = new Better.Dictionary<ArmorSkill, OrangeCharacter.SubStatus>();

	protected override void Awake()
	{
		InitShootTransforms();
		InitSkillStatus();
		base.Awake();
	}

	protected override void SetLinkAnimationAndStatus()
	{
		if (PlayAnimation((HumanBase.AnimateId)70u, (HumanBase.AnimateId)71u))
		{
			SetLogicFrame(SKL_SPAWN_END, SKL_SPAWN_END);
		}
		else
		{
			SetLogicFrame(SKL_SPAWN_JUMP_END, SKL_SPAWN_JUMP_END);
		}
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, dictSkillStatus[ArmorSkill.SPAWN]);
	}

	protected override void ResetLastStatus()
	{
		switch (_refEntity.AnimateID)
		{
		default:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.WALK, OrangeCharacter.SubStatus.WIN_POSE);
				break;
			}
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case (HumanBase.AnimateId)66u:
		case (HumanBase.AnimateId)68u:
		case (HumanBase.AnimateId)71u:
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.PlayerStopDashing();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				break;
			}
			if (_refEntity.IgnoreGravity)
			{
				_refEntity.IgnoreGravity = false;
			}
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		}
		characterMaterialWeapon.Disappear();
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
			SetLogicFrame(3, 12);
			UpdateEntityDirection();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, dictSkillStatus[ArmorSkill.SHOOT]);
			PlayAnimation(HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && curMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.CurrentActiveSkill == -1 && id == 1 && (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && CanUseSkl(ButtonId.SKILL1))
		{
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.CurrentActiveSkill = id;
			skl2CraeteCount = 8;
			SetLogicFrame(CH037_SKL_02.FRAME_TRIGGER, CH037_SKL_02.FRAME_END);
			bulletDirection = new Vector3(_refEntity.direction, 0f, 0f);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, dictSkillStatus[ArmorSkill.SKILL_02]);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_LINK_01, base.transform, Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_LINK_02, base.transform.position, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			PlayAnimation((HumanBase.AnimateId)69u, (HumanBase.AnimateId)69u);
			_refEntity.SoundSource.PlaySE(_refEntity.VoiceID, "v_ch037_skill02_01", 0.8f);
			_refEntity.PlaySE(_refEntity.SkillSEID, "ch037_rush");
			_refEntity.PlaySE(_refEntity.VoiceID, "v_ch037_skill02");
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.HURT && curMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.CurrentActiveSkill == -1 && id == 0 && CanUseSkl(ButtonId.SKILL0))
		{
			characterMaterialWeapon.Appear();
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.CurrentActiveSkill = id;
			SetLogicFrame(SKL_SKL_1_TRIGGER, SKL_SKL_1_END);
			UpdateEntityDirection();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, dictSkillStatus[ArmorSkill.SKILL_01]);
			PlayAnimation((HumanBase.AnimateId)67u, (HumanBase.AnimateId)68u);
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
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				ResetLastStatus();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_11:
			if (nowFrame >= skillEventFrame)
			{
				_refEntity.PlaySE(_refEntity.VoiceID, "v_ch037_skill01_01");
				bulletDirection = MonoBehaviourSingleton<OrangeBattleUtility>.Instance.GetShootDirection(_refEntity, shootTransforms[0]);
				CreateSkillBullet(ButtonId.SHOOT, shootTransforms[0], _refEntity.PlayerSkills[0].SkillLV);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_12);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_12:
			if (nowFrame >= endFrame)
			{
				ResetLastStatus();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= skillEventFrame)
			{
				_refEntity.PlaySE(_refEntity.VoiceID, "v_ch037_skill01_02");
				_refEntity.SoundSource.PlaySE(_refEntity.VoiceID, "v_ch037_skill01_03", 0.5f);
				bulletDirection = MonoBehaviourSingleton<OrangeBattleUtility>.Instance.GetShootDirection(_refEntity, shootTransforms[1]);
				CreateSkillBullet(ButtonId.SKILL0, shootTransforms[1], _refEntity.PlayerSkills[0].SkillLV);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				ResetLastStatus();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_11:
			if (nowFrame >= skillEventFrame)
			{
				if (skl2CraeteCount > 0)
				{
					CreateSkillBullet(ButtonId.SKILL1, shootTransforms[2], _refEntity.PlayerSkills[1].SkillLV);
					skl2CraeteCount--;
					skillEventFrame++;
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_12);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_12:
			if (nowFrame >= endFrame)
			{
				ResetLastStatus();
			}
			break;
		}
	}

	protected override void InitAnimateHash()
	{
		dictAnimateHash.Clear();
		int value = Animator.StringToHash("ch037_veh_stand_loop");
		foreach (HumanBase.AnimateId value2 in Enum.GetValues(typeof(HumanBase.AnimateId)))
		{
			dictAnimateHash.Add(value2, value);
		}
		dictAnimateHash[HumanBase.AnimateId.ANI_JUMP] = Animator.StringToHash("ch037_veh_jump_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_FALL] = Animator.StringToHash("ch037_veh_fall");
		dictAnimateHash[HumanBase.AnimateId.ANI_LAND] = Animator.StringToHash("ch037_veh_landing");
		dictAnimateHash[HumanBase.AnimateId.ANI_STEP] = Animator.StringToHash("ch037_veh_run_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALK] = Animator.StringToHash("ch037_veh_run_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALKBACK] = Animator.StringToHash("ch037_veh_run_back");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASH] = Animator.StringToHash("ch037_veh_dash_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASH_END] = Animator.StringToHash("ch037_veh_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_SLIDE] = Animator.StringToHash("ch037_veh_dash_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_SLIDE_END] = Animator.StringToHash("ch037_veh_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_AIRDASH_END] = Animator.StringToHash("ch037_veh_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_WIN_POSE] = Animator.StringToHash("ch037_veh_logout1");
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE] = Animator.StringToHash("ch037_veh_logout");
		dictAnimateHash[HumanBase.AnimateId.ANI_LOGOUT2] = Animator.StringToHash("ch037_veh_logout");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_STAND_START] = Animator.StringToHash("ch037_veh_dive_trigger_stand_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_JUMP_START] = Animator.StringToHash("ch037_veh_dive_trigger_jump_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_STAND_END] = Animator.StringToHash("ch037_veh_dive_trigger_stand_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_JUMP_END] = Animator.StringToHash("ch037_veh_dive_trigger_jump_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_SKILL_START] = Animator.StringToHash("shoot");
		dictAnimateHash[(HumanBase.AnimateId)66u] = Animator.StringToHash("shoot_jump");
		dictAnimateHash[(HumanBase.AnimateId)67u] = Animator.StringToHash("skill01");
		dictAnimateHash[(HumanBase.AnimateId)68u] = Animator.StringToHash("skill01_jump");
		dictAnimateHash[(HumanBase.AnimateId)69u] = Animator.StringToHash("skill02");
		dictAnimateHash[(HumanBase.AnimateId)70u] = Animator.StringToHash("ch037_veh_spawn_stand");
		dictAnimateHash[(HumanBase.AnimateId)71u] = Animator.StringToHash("ch037_veh_spawn_jump");
	}

	private void InitShootTransforms()
	{
		if (shootTransforms[0] == null)
		{
			shootTransforms[0] = base.transform.Find("Sub_Vehicle_ShootPoint");
			if (shootTransforms[0] == null)
			{
				shootTransforms[0] = base.transform;
			}
		}
		if (shootTransforms[1] == null)
		{
			shootTransforms[1] = base.transform.Find("Sub_Tron_ShootPoint");
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
			shootTransforms[3] = base.transform.Find("Sub_Vehicle_AimPoint");
			if (shootTransforms[3] == null)
			{
				shootTransforms[3] = base.transform;
			}
		}
	}

	private void InitSkillStatus()
	{
		dictSkillStatus.Clear();
		dictSkillStatus.Add(ArmorSkill.SPAWN, OrangeCharacter.SubStatus.SKILL0);
		dictSkillStatus.Add(ArmorSkill.SHOOT, OrangeCharacter.SubStatus.SKILL0_11);
		dictSkillStatus.Add(ArmorSkill.SKILL_01, OrangeCharacter.SubStatus.SKILL1);
		dictSkillStatus.Add(ArmorSkill.SKILL_02, OrangeCharacter.SubStatus.SKILL1_11);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_LINK);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_LINK_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_LINK_02, 2);
	}

	public override void PlayCharaSE(CharaSE seId)
	{
		switch (seId)
		{
		default:
			_refEntity.PlayCharaSE(seId);
			break;
		case CharaSE.JUMP:
		case CharaSE.JUMPHIGH:
			_refEntity.PlaySE("BattleSE", "bt_ridearmor04");
			break;
		case CharaSE.STEP:
			_refEntity.PlaySE("BattleSE", "bt_ridearmor01");
			break;
		case CharaSE.DASH:
			_refEntity.PlaySE("BattleSE", "bt_ridearmor03");
			break;
		case CharaSE.DASHEND:
			_refEntity.PlaySE("BattleSE", "bt_ridearmor05");
			break;
		case CharaSE.CHAKUCHI:
			_refEntity.PlaySE("BattleSE", "bt_ridearmor02");
			break;
		}
	}
}
