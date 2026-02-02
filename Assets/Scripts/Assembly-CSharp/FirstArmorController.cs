using System;
using UnityEngine;

public class FirstArmorController : CharacterControlBase
{
	private bool bInSkill;

	private int nFlyFrame;

	private int nFlyStatus;

	private bool bisbelow;

	private ChargeShootObj _refChargeShootObj;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch023_skill_02_stand_start", "ch023_skill_02_jump_start", "ch023_skill_02_end" };
	}

	public override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_headcrush_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_headcrush_001", 2);
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip", true);
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_X1stArmor", "x1_charge_lp", "x1_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE", "bt_x1_charge_lp", "bt_x1_charge_stop" };
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || !bInSkill)
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CheckSkillEndByShootTimer())
		{
			bInSkill = false;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) > 1u)
		{
			return;
		}
		if (nFlyStatus == 0)
		{
			if (nFlyFrame > 0)
			{
				if (!bisbelow)
				{
					_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.5f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f));
				}
				nFlyFrame--;
				return;
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_000", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			}
			_refEntity.StartJumpThroughCorutine();
			_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.5f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f));
			nFlyStatus = 1;
			nFlyFrame = 5;
		}
		else if (nFlyStatus == 1)
		{
			if (nFlyFrame > 0)
			{
				nFlyFrame--;
				_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.5f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f));
				return;
			}
			_refEntity.SetSpeed(0, 0);
			_refEntity.SkillEnd = true;
			bInSkill = false;
			_refEntity.BulletCollider.BackToPool();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[1].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[1].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			}
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			switch (_refEntity.CurrentActiveSkill)
			{
			case 0:
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentSkillObj());
				_refEntity.EnableCurrentWeapon();
				_refEntity.CancelBusterChargeAtk();
				break;
			case 1:
				_refEntity.BulletCollider.BackToPool();
				_refEntity.EnableCurrentWeapon();
				break;
			}
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.SKILL1_2)
		{
			_refEntity.SkillEnd = true;
			_refEntity.BulletCollider.BackToPool();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp <= 0)
		{
			return;
		}
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
					bInSkill = true;
					_refChargeShootObj.ShootChargeBuster(id);
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				bInSkill = true;
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, 0);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CanUseDash() && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.UseDashChance();
				_refEntity.PlayerSkills[1].MagazineRemain -= _refEntity.PlayerSkills[1].BulletData.n_USE_COST;
				_refEntity.PlayerSkills[1].LastUseTimer.TimerStart();
				_refEntity.DisableCurrentWeapon();
				nFlyFrame = 4;
				_refEntity.SetSpeed(0, 0);
				_refEntity.IgnoreGravity = false;
				if (_refEntity.PreBelow)
				{
					bisbelow = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				}
				else
				{
					bisbelow = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_headcrush_001", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				}
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				nFlyStatus = 0;
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp > 0)
		{
			if (id != 0)
			{
				int num = 1;
			}
			else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				bInSkill = true;
				_refChargeShootObj.ShootChargeBuster(id);
			}
		}
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[4] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[4] { "ch023_skill_01_stand", "ch023_skill_01_fall", "ch023_skill_01_wallgrab", "ch023_skill_01_crouch" };
	}
}
