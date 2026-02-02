using UnityEngine;

public class AliaController : CharacterControlBase
{
	private int _skillBulletCreated = -1;

	private Vector3 _skillDirection = Vector3.zero;

	private Vector3 _skillPosition = Vector3.zero;

	private OrangeTimer _meltCreeperTimer;

	private bool _meltCreeperFlag;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch003_skill_02" };
	}

	public override void Start()
	{
		base.Start();
		_meltCreeperTimer = OrangeTimerManager.GetTimer();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.EnterRideArmorEvt = OnEnterRideArmor;
	}

	public override void ClearSkill()
	{
	}

	public override void CheckSkill()
	{
		if (_meltCreeperFlag && _skillBulletCreated < _refEntity.PlayerSkills[1].BulletData.n_NUM_SHOOT)
		{
			if (_meltCreeperTimer.GetMillisecond() > _skillBulletCreated * 300)
			{
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0]);
				_skillBulletCreated++;
			}
		}
		else
		{
			_meltCreeperFlag = false;
		}
		if (!_refEntity.IsAnimateIDChanged() && _refEntity.CurrentActiveSkill != -1 && _refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.CheckSkillEndByShootTimer();
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0 && id == 1 && !_meltCreeperFlag && _refEntity.CurrentActiveSkill != id && (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_meltCreeperFlag = true;
			_refEntity.PlaySE(_refEntity.VoiceID, 8);
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.WALK && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.RIDE_ARMOR)
			{
				_skillDirection = Vector3.right * (0 - _refEntity._characterDirection);
			}
			else
			{
				_skillDirection = Vector3.right * (float)_refEntity._characterDirection;
			}
			_skillPosition = _refEntity._transform.position;
			_refEntity.CurrentActiveSkill = id;
			_meltCreeperTimer.TimerStart();
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.SetSpeed(0, 0);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			_skillBulletCreated = 0;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.WIN_POSE);
			_refEntity.StopShootTimer();
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel);
			_refEntity.PlaySE(_refEntity.VoiceID, 9);
			_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		SKILL_TABLE bulletData = weaponStruct.BulletData;
		BulletBase poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(bulletData.s_MODEL);
		if ((bool)poolObj)
		{
			poolObj.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			poolObj.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			poolObj.BulletLevel = weaponStruct.SkillLV;
			((CollideBullet)poolObj).Active(_skillPosition + _skillDirection * (1.25f + (float)_skillBulletCreated * weaponStruct.BulletData.f_DISTANCE), Vector3.right, _refEntity.TargetMask);
			poolObj.GetComponentInChildren<ParticleSystem>().Play();
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.WIN_POSE)
		{
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
		}
	}

	private bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_meltCreeperFlag)
		{
			_skillBulletCreated = -1;
			_meltCreeperFlag = false;
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}
}
