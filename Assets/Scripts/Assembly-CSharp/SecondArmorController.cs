#define RELEASE
using System;
using UnityEngine;

public class SecondArmorController : CharacterControlBase
{
	private enum SkillAnimationID
	{
		ANI_SKILL0_stand_first_shot_start = 65,
		ANI_SKILL0_jump_first_shot_start = 66,
		ANI_SKILL0_crouch_first_shot_start = 67,
		ANI_SKILL0_stand_first_shot_end = 68,
		ANI_SKILL0_jump_first_shot_end = 69,
		ANI_SKILL0_crouch_first_shot_end = 70,
		ANI_SKILL0_stand_second_shot_start = 71,
		ANI_SKILL0_jump_second_shot_start = 72,
		ANI_SKILL0_crouch_second_shot_start = 73,
		ANI_SKILL0_stand_second_shot_end = 74,
		ANI_SKILL0_jump_second_shot_end = 75,
		ANI_SKILL0_crouch_second_shot_end = 76,
		ANI_SKILL1_prepare_loop = 77,
		ANI_SKILL1_prepare_to_charge = 78,
		ANI_SKILL1_charge_loop = 79,
		ANI_SKILL1_charge_to_burst = 80,
		ANI_SKILL1_burst_loop = 81,
		ANI_SKILL1_brust_to_fall = 82
	}

	private ChargeShootObj _refChargeShootObj;

	private SkinnedMeshRenderer[] _RhandMesh;

	private bool _chargeShotTrigger;

	private int _gigaCrushHoverFrame;

	private CollideBullet _gigaCrushCollider;

	public ch_007_Skill_effect GigaCrushEffect;

	private OrangeTimer _chargePoseDelayTimer;

	private int nLastSkill;

	private float _overrideAppearTime = 0.01f;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[18]
		{
			"ch009_skill_01_stand_first_shot_start", "ch009_skill_01_jump_first_shot_start", "ch009_skill_01_crouch_first_shot_start", "ch009_skill_01_stand_first_shot_end", "ch009_skill_01_jump_first_shot_end", "ch009_skill_01_crouch_first_shot_end", "ch009_skill_01_stand_second_shot_start", "ch009_skill_01_jump_second_shot_start", "ch009_skill_01_crouch_second_shot_start", "ch009_skill_01_stand_second_shot_end",
			"ch009_skill_01_jump_second_shot_end", "ch009_skill_01_crouch_second_shot_end", "ch009_skill_02_prepare_loop", "ch009_skill_02_prepare_to_charge", "ch009_skill_02_charge_loop", "ch009_skill_02_charge_to_burst", "ch009_skill_02_burst_loop", "ch009_skill_02_brust_to_fall"
		};
	}

	public override void Start()
	{
		base.Start();
		if (_gigaCrushCollider == null)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = "GigaCrushCollider";
			_gigaCrushCollider = gameObject.AddComponent<CollideBullet>();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_gigacrush_000");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_gigacrush_001");
		GigaCrushEffect = OrangeBattleUtility.FindChildRecursive(base.transform, "mesheffect").GetComponent<ch_007_Skill_effect>();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip", true);
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_X2ndArmor", "x2_charge_lp", "x2_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE", "bt_x2_charge_lp", "bt_x2_charge_stop" };
		}
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(base.transform, "HandMesh_R");
		_RhandMesh = new SkinnedMeshRenderer[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_RhandMesh[i] = array[i].GetComponent<SkinnedMeshRenderer>();
		}
		_chargePoseDelayTimer = OrangeTimerManager.GetTimer();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
		_refEntity.PlayerHeldLeftRightSkillCB = PlayerHeldLeftRightSkill;
		_refEntity.PlayerReleaseLeftRightSkillCB = PlayerReleaseLeftRightSkill;
		_refEntity.PlayerSkillLandCB = PlayerSkillLand;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			ToggleRightBuster(false);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentSkillObj());
			break;
		case 1:
			_refEntity.EnableCurrentWeapon();
			break;
		}
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged())
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.PlayerSkills[0].Reload_index == 0 && _refEntity.IsShoot == 0)
		{
			ToggleRightBuster(false);
			_refEntity.SkillEnd = true;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				if (_refEntity.CurrentFrame > 1f)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
					ToggleRightBuster(false);
					_refEntity.SkillEnd = true;
				}
				else
				{
					if (!((double)_refEntity.CurrentFrame > 0.3))
					{
						break;
					}
					Debug.Log("Trigger Skill!");
					if (!_chargeShotTrigger)
					{
						_chargeShotTrigger = true;
						_refChargeShootObj.StopCharge();
						_refEntity.PlaySE(_refEntity.VoiceID, 8);
						_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[0], true, 0, 2, (float)_refEntity._characterDirection * Vector3.right, false);
						_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[1]);
						if (_refEntity.IsLocalPlayer)
						{
							_refEntity.TriggerComboSkillBuff(_refEntity.PlayerSkills[0].FastBulletDatas[2].n_ID);
						}
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
				{
					ToggleRightBuster(false);
					_refEntity.SkillEnd = true;
					if (_refEntity.Controller.Collisions.below)
					{
						_refEntity.Dashing = false;
						_refEntity.SetHorizontalSpeed(0);
						_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
					}
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				if (_refEntity.CurrentFrame > 1f)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
					ToggleRightBuster(false);
					_refEntity.SkillEnd = true;
				}
				else if ((double)_refEntity.CurrentFrame > 0.3 && !_chargeShotTrigger)
				{
					_chargeShotTrigger = true;
					Debug.Log("Trigger Skill2!");
					_refEntity.PlaySE(_refEntity.VoiceID, 8);
					_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[0], true, 0, (sbyte)nLastSkill, (float)_refEntity._characterDirection * Vector3.right, false);
					_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
					_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[0].FastBulletDatas[nLastSkill].n_ID);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
				{
					_refEntity.SkillEnd = true;
					if (_refEntity.Controller.Collisions.below)
					{
						_refEntity.Dashing = false;
						_refEntity.SetHorizontalSpeed(0);
						_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
					}
					else
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				if (_gigaCrushHoverFrame > 0)
				{
					_gigaCrushHoverFrame--;
					if (_gigaCrushHoverFrame > 0)
					{
						_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.4f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.4f));
						break;
					}
					_refEntity.IgnoreGravity = true;
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
					_gigaCrushHoverFrame = 5;
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (_gigaCrushHoverFrame > 0)
				{
					_gigaCrushHoverFrame--;
					if (_gigaCrushHoverFrame <= 0)
					{
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				if (_gigaCrushHoverFrame <= 0)
				{
					break;
				}
				_gigaCrushHoverFrame--;
				if (_gigaCrushHoverFrame == 10)
				{
					int nowRecordNO = _refEntity.GetNowRecordNO();
					_gigaCrushCollider.UpdateBulletData(_refEntity.PlayerSkills[1].BulletData, _refEntity.sPlayerName, nowRecordNO, _refEntity.nBulletRecordID++);
					_gigaCrushCollider.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
					_gigaCrushCollider.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
					_gigaCrushCollider.Active(base.transform, Vector2.right * (float)_refEntity._characterDirection, _refEntity.TargetMask);
				}
				if (_gigaCrushHoverFrame <= 0)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
					GigaCrushEffect.ActiveEffect(false);
					if (_refEntity.GetCurrentWeaponObj().ChipEfx != null && _refEntity.GetCurrentWeaponObj().chip_switch)
					{
						_refEntity.GetCurrentWeaponObj().ChipEfx.isActive = false;
						_refEntity.GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(_refEntity.tRefPassiveskill.bUsePassiveskill);
					}
				}
				break;
			}
		}
		else if (GigaCrushEffect.isActive)
		{
			GigaCrushEffect.ActiveEffect(false);
			if (_refEntity.GetCurrentWeaponObj().ChipEfx != null && _refEntity.GetCurrentWeaponObj().chip_switch)
			{
				_refEntity.GetCurrentWeaponObj().ChipEfx.isActive = false;
				_refEntity.GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(_refEntity.tRefPassiveskill.bUsePassiveskill);
			}
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_chargeShotTrigger = false;
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), _overrideAppearTime);
			ToggleRightBuster(true);
			if (!_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			}
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (!_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			}
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_chargeShotTrigger = false;
			_refEntity.UpdateWeaponMesh(_refEntity.PlayerSkills[0], _refEntity.GetCurrentWeaponObj(), _overrideAppearTime);
			if (!_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			}
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (!_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
				break;
			}
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), _overrideAppearTime);
			_refEntity.SetAnimateId((HumanBase.AnimateId)77u);
			_refEntity.PlaySE(_refEntity.VoiceID, 9);
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetAnimateId((HumanBase.AnimateId)78u);
			_refEntity.PlaySE(_refEntity.SkillSEID, 5);
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SetAnimateId((HumanBase.AnimateId)79u);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_gigacrush_000", base.transform.position + new Vector3(0f, 0.7f, 0f), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			_refEntity.SetAnimateId((HumanBase.AnimateId)80u);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_gigacrush_001", base.transform.position + new Vector3(0f, 0.7f, 0f), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			_refEntity.SetAnimateId((HumanBase.AnimateId)81u);
			GigaCrushEffect.ActiveEffect(true);
			_gigaCrushHoverFrame = 15;
			_refEntity.PlaySE(_refEntity.SkillSEID, 6);
			break;
		case OrangeCharacter.SubStatus.SKILL1_5:
			_refEntity.EnableCurrentWeapon();
			_refEntity.SetAnimateId((HumanBase.AnimateId)82u);
			_refEntity.IgnoreGravity = false;
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (!_chargePoseDelayTimer.IsStarted())
			{
				_chargePoseDelayTimer.TimerStart();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1)
			{
				ToggleRightBuster(false);
			}
			_refEntity.SkillEnd = true;
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			GigaCrushEffect.ActiveEffect(false);
			if (_refEntity.GetCurrentWeaponObj().ChipEfx != null && _refEntity.GetCurrentWeaponObj().chip_switch)
			{
				_refEntity.GetCurrentWeaponObj().ChipEfx.isActive = false;
				_refEntity.GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(_refEntity.tRefPassiveskill.bUsePassiveskill);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_5:
			_refEntity.SkillEnd = true;
			GigaCrushEffect.ActiveEffect(false);
			if (_refEntity.GetCurrentWeaponObj().ChipEfx != null && _refEntity.GetCurrentWeaponObj().chip_switch)
			{
				_refEntity.GetCurrentWeaponObj().ChipEfx.isActive = false;
				_refEntity.GetCurrentWeaponObj().ChipEfx.ActiveChipSkill(_refEntity.tRefPassiveskill.bUsePassiveskill);
			}
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		}
	}

	private void ChargeStart(int id)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.HURT:
			return;
		case OrangeCharacter.MainStatus.SKILL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL0_1)
			{
				return;
			}
			break;
		}
		}
		if (!_refEntity.PlayerSkills[id].ForceLock && _refEntity.PlayerSkills[0].Reload_index == 0 && !_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[id].FastBulletDatas[0].n_CHARGE_MAX_LEVEL != 0 && _refEntity.CheckUseSkillKeyTriggerEX(id))
		{
			_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp > 0)
		{
			if (id != 0)
			{
				int num = 1;
			}
			else if (_refEntity.PlayerSetting.AutoCharge == 1 && !_refEntity.PlayerSkills[id].ChargeTimer.IsStarted() && _refEntity.PlayerSkills[0].Reload_index == 0)
			{
				ChargeStart(id);
				_refChargeShootObj.StartCharge();
			}
			else if ((_refEntity.CurrentActiveSkill == -1 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1) && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ShootChargeBuster(id);
			}
		}
	}

	public void PlayerHeldSkill(int id)
	{
		if (_refEntity.PlayerSetting.AutoCharge == 0)
		{
			ChargeStart(id);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp <= 0)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.PlayerSetting.AutoCharge == 0 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				ShootChargeBuster(id);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.selfBuffManager.nMeasureNow >= _refEntity.PlayerSkills[id].BulletData.n_USE_COST && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.selfBuffManager.AddMeasure(-_refEntity.PlayerSkills[id].BulletData.n_USE_COST);
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
				_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f));
				_gigaCrushHoverFrame = 3;
			}
			break;
		}
	}

	private void ShootChargeBuster(int id)
	{
		_refEntity.CurrentActiveSkill = id;
		nLastSkill = _refEntity.PlayerSkills[id].Reload_index;
		if (id == 0)
		{
			if (_refEntity.PlayerSkills[id].ChargeLevel == 2)
			{
				_refEntity.SkillEnd = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				return;
			}
			if (_refEntity.PlayerSkills[0].Reload_index != 0)
			{
				_refEntity.SkillEnd = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				return;
			}
			_refEntity.SkillEnd = false;
			_refChargeShootObj.StopCharge(id);
			_refEntity.PlaySE(_refEntity.VoiceID, 8);
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel);
			_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
		}
	}

	public override void ControlCharacterDead()
	{
		_chargePoseDelayTimer.TimerStop();
		_chargeShotTrigger = false;
		_gigaCrushHoverFrame = 0;
		nLastSkill = 0;
	}

	private void ToggleRightBuster(bool enable)
	{
		if (enable)
		{
			_refEntity.PlayerSkills[0].WeaponMesh[1].Appear(null, _overrideAppearTime);
		}
		else
		{
			_refEntity.PlayerSkills[0].WeaponMesh[1].Disappear(null, _overrideAppearTime);
		}
		SkinnedMeshRenderer[] rhandMesh = _RhandMesh;
		for (int i = 0; i < rhandMesh.Length; i++)
		{
			rhandMesh[i].enabled = !enable;
		}
	}

	public void PlayerSkillLand()
	{
		_refEntity.SetHorizontalSpeed(0);
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
			break;
		}
	}

	public void PlayerHeldLeftRightSkill(object param)
	{
		CharacterDirection characterDirection = (CharacterDirection)param;
		int num = _refEntity.CalculateMoveSpeed();
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) <= 3u && !_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetHorizontalSpeed((int)characterDirection * num);
		}
	}

	public void PlayerReleaseLeftRightSkill(object param)
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) <= 3u && !_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetHorizontalSpeed(0);
		}
	}

	protected void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) > 3u)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}
}
