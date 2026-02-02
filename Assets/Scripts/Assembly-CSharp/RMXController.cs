public class RMXController : CharacterControlBase
{
	protected ChargeShootObj _refChargeShootObj;

	public override void Start()
	{
		base.Start();
		_refEntity.DisableWeaponMesh(_refEntity.PlayerSkills[0]);
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_X", "x_charge_lp", "x_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE", "bt_x_charge_lp", "bt_x_charge_stop" };
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
	}

	public override void CheckSkill()
	{
		if (!_refEntity.IsAnimateIDChanged() && _refEntity.CurrentActiveSkill != -1)
		{
			int currentActiveSkill = _refEntity.CurrentActiveSkill;
			if ((uint)currentActiveSkill <= 1u)
			{
				_refEntity.CheckSkillEndByShootTimer();
			}
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if ((uint)currentActiveSkill <= 1u)
		{
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentSkillObj());
			_refEntity.EnableCurrentWeapon();
			if (_refEntity.CurrentActiveSkill == 0)
			{
				_refEntity.CancelBusterChargeAtk();
			}
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
		}
		_refEntity.Dashing = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
		}
		else
		{
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				return;
			}
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!_refEntity.PlayerSkills[id].ChargeTimer.IsStarted())
				{
					_refEntity.PlayerSkills[id].ChargeTimer.TimerStart();
					_refChargeShootObj.StartCharge();
				}
				else
				{
					_refChargeShootObj.ShootChargeBuster(id);
				}
			}
			else
			{
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, 0);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refChargeShootObj.ShootChargeBuster(id);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel);
				_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
			}
			break;
		}
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[4] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[4] { "ch001_skill_01_stand", "ch001_skill_01_fall", "ch001_skill_01_wallgrab", "ch001_skill_01_crouch" };
	}
}
