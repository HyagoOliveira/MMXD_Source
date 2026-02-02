using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;

public class CH093_Controller : CharacterControllerProxyBaseGen3
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL0 = 65u,
		ANI_SKILL1_SLASH = 66u
	}

	private enum FxName
	{
		fxuse_chargecut_000 = 0,
		fxuse_chargecut_001 = 1,
		fxuse_chargecut_002 = 2,
		fxuse_dragonslash_000 = 3
	}

	public readonly float TIME_SKILL_0_HITPAUSE = 0.3f;

	private readonly int SPEED_SKILL_0_BASE = Mathf.RoundToInt(OrangeBattleUtility.PlayerWalkSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	private readonly int SPEED_SKILL_1_BASE = Mathf.RoundToInt(OrangeBattleUtility.PlayerWalkSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	public float Shift_Skill_0_FX = 1.5f;

	public float Speed_0_Ratio_Chop_1 = 1f;

	public float Speed_0_Ratio_Roll = 1.5f;

	public float Speed_0_Ratio_Chop_2 = 2f;

	public float Time_Skill_0_Raise = 0.4f;

	public float Time_Skill_0_ChopShift_1 = 0.1f;

	public float Time_Skill_0_Roll = 0.3f;

	public float Time_Skill_0_ChopShift_2 = 0.1f;

	public float Time_Skill_0_Chop_Cancel = 0.2f;

	public float Speed_1_Ratio_Slash = 0.5f;

	public float Time_Skill_1_Raise = 0.1f;

	public float Time_Skill_1_Slash = 0.3f;

	public float Time_Skill_1_Slash_Cancel = 0.15f;

	private bool _isSkillShooting;

	private SkinnedMeshRenderer _busterMesh;

	private SkinnedMeshRenderer _saberMesh;

	private SkinnedMeshRenderer _backSaberMesh;

	private bool _isMeleeBulletSetted;

	public float SkillSpeed = 1f;

	private SKILL_TABLE _linkSkillData;

	private OrangeConsoleCharacter _refConsolePlayer;

	private void InitializeLinkSkillData()
	{
		int n_LINK_SKILL = _refEntity.PlayerSkills[0].BulletData.n_LINK_SKILL;
		SKILL_TABLE value;
		if (n_LINK_SKILL != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(n_LINK_SKILL, out value))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref value);
			_linkSkillData = value;
		}
	}

	private void ToggleMeleeBullet(bool isActive, bool useLinkSkill = false)
	{
		if (isActive)
		{
			if (!_isMeleeBulletSetted)
			{
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				if (useLinkSkill && _linkSkillData != null)
				{
					_refEntity.BulletCollider.UpdateBulletData(_linkSkillData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				}
				else
				{
					_refEntity.BulletCollider.UpdateBulletData(currentSkillObj.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				}
				_refEntity.BulletCollider.SetBulletAtk(currentSkillObj.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = currentSkillObj.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_isMeleeBulletSetted = true;
			}
		}
		else
		{
			_refEntity.BulletCollider.BackToPool();
			_isMeleeBulletSetted = false;
		}
	}

	private void OnBulletColliderHit(object obj)
	{
		StartHitPause(TIME_SKILL_0_HITPAUSE);
	}

	private void ActionStatusChanged_0_0()
	{
		SetIgnoreGravity();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
		base.AnimatorSpeed = SkillSpeed;
		SetSkillFrame(Time_Skill_0_Raise / SkillSpeed);
	}

	private void ActionStatusChanged_0_1()
	{
		SetSpeed((float)SPEED_SKILL_0_BASE * Speed_0_Ratio_Chop_1 * SkillSpeed * (float)_refEntity.direction, 0f);
		SetSkillFrame(Time_Skill_0_ChopShift_1 / SkillSpeed);
	}

	private void ActionStatusChanged_0_2()
	{
		ToggleMeleeBullet(false);
		SetSpeed((float)SPEED_SKILL_0_BASE * Speed_0_Ratio_Roll * (float)_refEntity.direction, 0f);
		SetSkillFrame(Time_Skill_0_Roll / SkillSpeed);
	}

	private void ActionStatusChanged_0_3()
	{
		SetSpeed((float)SPEED_SKILL_0_BASE * Speed_0_Ratio_Chop_2 * SkillSpeed * (float)_refEntity.direction, 0f);
		SetSkillFrame(Time_Skill_0_ChopShift_2 / SkillSpeed);
	}

	private void ActionStatusChanged_0_4()
	{
		ToggleMeleeBullet(false);
		ResetSpeed();
		SetSkillFrame(Time_Skill_0_Chop_Cancel / SkillSpeed);
	}

	private void ActionStatusChanged_1_0()
	{
		SetIgnoreGravity();
		_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
		SetSkillFrame(Time_Skill_1_Raise / SkillSpeed);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_dragonslash_000.ToString(), _refEntity._transform, OrangeCharacter.NormalQuaternion, new Vector3(base.SkillFXDirection, 1f, 1f), Array.Empty<object>());
	}

	private void ActionStatusChanged_1_1()
	{
		SetSpeed((float)SPEED_SKILL_1_BASE * Speed_1_Ratio_Slash * (float)_refEntity.direction, 0f);
		SetSkillFrame(Time_Skill_1_Slash);
	}

	private void ActionStatusChanged_1_2()
	{
		ResetSpeed();
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[base.CurActiveSkill];
		SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[weaponStruct.Reload_index];
		_refEntity.CheckUsePassiveSkill(base.CurActiveSkill, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.IsShoot = 0;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(sKILL_TABLE, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, Vector2.right * _refEntity.direction);
		SetSkillCancelFrame(Time_Skill_1_Slash_Cancel);
	}

	private void ActionLogicUpdate_0()
	{
		if (base.IsHitPauseStarted)
		{
			AddSkillEndFrame();
		}
		ToggleMeleeBullet(true, base.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_3);
		if (CheckSkillFrameEnd())
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>((base.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_3) ? FxName.fxuse_chargecut_002.ToString() : FxName.fxuse_chargecut_001.ToString(), _refEntity._transform.position + Vector3.right * Shift_Skill_0_FX * _refEntity.direction, (_refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			ShiftSkillStatus();
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (_refEntity is OrangeConsoleCharacter)
		{
			_refConsolePlayer = _refEntity as OrangeConsoleCharacter;
		}
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "R WeaponPoint", true)
		};
		_busterMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "BusterMesh_m").GetComponent<SkinnedMeshRenderer>();
		_saberMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "SaberMesh_m", true).GetComponent<SkinnedMeshRenderer>();
		_backSaberMesh = OrangeBattleUtility.FindChildRecursive(base.ChildTransforms, "BackSaberMesh_m").GetComponent<SkinnedMeshRenderer>();
	}

    [Obsolete]
    public override void Start()
	{
		base.Start();
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		CollideBullet bulletCollider = _refEntity.BulletCollider;
		bulletCollider.HitCallback = (CallbackObj)Delegate.Combine(bulletCollider.HitCallback, new CallbackObj(OnBulletColliderHit));
		InitializeLinkSkillData();
		InitializeSkillDependDelegators(new Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData>
		{
			{
				OrangeCharacter.SubStatus.SKILL0,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_0,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_1,
					OnLogicUpdate = ActionLogicUpdate_0
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_2,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_2,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_3,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_3,
					OnLogicUpdate = ActionLogicUpdate_0
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL0_4,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_0_4,
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_0,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_1,
				new SkillStateDelegateData
				{
					OnStatusChanged = ActionStatusChanged_1_1,
					OnLogicUpdate = base.ActionCheckNextSkillStatus
				}
			},
			{
				OrangeCharacter.SubStatus.SKILL1_2,
				new SkillStateDelegateData
				{
					OnAnimationEnd = base.ActionSetSkillEnd,
					OnStatusChanged = ActionStatusChanged_1_2,
					OnLogicUpdate = base.ActionCheckSkillCancel
				}
			}
		});
	}

    [Obsolete]
    public override void OnDestroy()
	{
		base.OnDestroy();
		CollideBullet bulletCollider = _refEntity.BulletCollider;
		bulletCollider.HitCallback = (CallbackObj)Delegate.Remove(bulletCollider.HitCallback, new CallbackObj(OnBulletColliderHit));
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch093_skill_01", "ch093_skill_02_step2" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[8] { "login", "logout", "win", "buster_stand_charge_atk", "buster_crouch_charge_atk", "buster_jump_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk" };
		target = new string[8] { "ch093_login", "ch093_logout", "ch093_win", "ch093_skill_02_step1_stand", "ch093_skill_02_step1_Crouch", "ch093_skill_02_step1_Jump", "ch093_skill_02_step1_Fall", "ch093_skill_02_step1_Wallgrab" };
	}

	protected override void TeleportInCharacterDepend()
	{
	}

	protected override void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				_busterMesh.enabled = false;
				_backSaberMesh.enabled = false;
				_saberMesh.enabled = false;
			}
		}
	}

	protected override void StageTeleportInCharacterDepend()
	{
		StartCoroutine(ToggleExtraTransforms(true, 0.5f));
	}

	protected override void StageTeleportOutCharacterDepend()
	{
		StartCoroutine(ToggleExtraTransforms(false, 0.3f));
	}

	public override void ControlCharacterDead()
	{
		ForceStopHitPause();
		StartCoroutine(ToggleExtraTransforms(false, 0.5f));
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(ToggleExtraTransforms(true, 0.6f));
	}

	protected override void ToggleExtraTransforms(bool isActive)
	{
		_backSaberMesh.enabled = isActive;
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			ForceStopHitPause();
		}
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		switch (weaponState)
		{
		case WeaponState.TELEPORT_IN:
		case WeaponState.TELEPORT_OUT:
			ToggleNormalWeapon(false);
			_busterMesh.enabled = true;
			_backSaberMesh.enabled = true;
			_saberMesh.enabled = false;
			_refEntity.EnableHandMesh(false);
			break;
		case WeaponState.SKILL_0:
			ToggleNormalWeapon(false);
			_busterMesh.enabled = false;
			_backSaberMesh.enabled = false;
			_saberMesh.enabled = true;
			break;
		case WeaponState.SKILL_1:
			ToggleNormalWeapon(false);
			switch (_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill].Reload_index)
			{
			case 0:
				_busterMesh.enabled = true;
				_backSaberMesh.enabled = true;
				_saberMesh.enabled = false;
				_refEntity.EnableHandMesh(false);
				break;
			case 1:
				_busterMesh.enabled = false;
				_backSaberMesh.enabled = false;
				_saberMesh.enabled = true;
				break;
			}
			break;
		default:
			ToggleNormalWeapon(true);
			_busterMesh.enabled = false;
			_backSaberMesh.enabled = true;
			_saberMesh.enabled = false;
			break;
		}
	}

	protected override void SetSkillEnd()
	{
		_isSkillShooting = false;
		base.SetSkillEnd();
	}

	protected override void OnChangeComboSkill(SkillID skillId, int reloadIndex)
	{
		if (skillId != SkillID.SKILL_1)
		{
			return;
		}
		switch (reloadIndex)
		{
		case 0:
		{
			OrangeConsoleCharacter refConsolePlayer3 = _refConsolePlayer;
			if ((object)refConsolePlayer3 != null)
			{
				refConsolePlayer3.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
			}
			break;
		}
		case 1:
		{
			OrangeConsoleCharacter refConsolePlayer = _refConsolePlayer;
			if ((object)refConsolePlayer != null)
			{
				refConsolePlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
			}
			OrangeConsoleCharacter refConsolePlayer2 = _refConsolePlayer;
			if ((object)refConsolePlayer2 != null)
			{
				refConsolePlayer2.ClearVirtualButtonStick(VirtualButtonId.SKILL1);
			}
			break;
		}
		}
	}

	protected override void AttachSkillDelegateEvent()
	{
		OnPlayerPressSkill0Events[0] = OnPlayerPressSkill0;
		OnPlayerPressSkill1Events[1] = OnPlayerPressSkill1_1;
		OnPlayerReleaseSkill1Events[0] = OnPlayerReleaseSkill1_0;
	}

	protected override void OnPlayerPressSkill0(SkillID skillID)
	{
		base.OnPlayerPressSkill0(skillID);
		PlayVoiceSE("v_xm_skill01");
		PlaySkillSE("xm_tame01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_chargecut_000.ToString(), _refEntity._transform.position, (_refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
	}

	protected void OnPlayerPressSkill1_1(SkillID skillID)
	{
		PlayVoiceSE("v_xm_skill02_2");
		base.OnPlayerPressSkill1(skillID);
	}

	private void OnPlayerReleaseSkill1_0(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		SetSkillAndWeapon(skillID);
		_refEntity.IsShoot = 3;
		_refEntity.StartShootTimer();
		_refEntity.Animator.SetAnimatorEquip(1);
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, _refEntity.ShootDirection);
		_isSkillShooting = true;
		_refEntity.CheckUsePassiveSkill((int)skillID, weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0]);
		PlayVoiceSE("v_xm_skill02_1");
	}

	protected override void LogicUpdateCharacterDepend()
	{
		SkillID curActiveSkill = (SkillID)base.CurActiveSkill;
		if (curActiveSkill == SkillID.SKILL_1 && _isSkillShooting && _refEntity.CheckSkillEndByShootTimer())
		{
			_isSkillShooting = false;
			ToggleWeapon(WeaponState.NORMAL);
		}
		base.LogicUpdateCharacterDepend();
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}

	public override void ClearSkill()
	{
		switch ((SkillID)_refEntity.CurrentActiveSkill)
		{
		case SkillID.SKILL_0:
			SetSkillEnd();
			break;
		case SkillID.SKILL_1:
			if (_isSkillShooting)
			{
				_refEntity.CancelBusterChargeAtk();
				_isSkillShooting = false;
				ToggleWeapon(WeaponState.NORMAL);
			}
			SetSkillEnd();
			break;
		}
	}
}
