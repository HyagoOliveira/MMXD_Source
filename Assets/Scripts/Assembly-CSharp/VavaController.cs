using UnityEngine;

public class VavaController : CharacterControlBase
{
	private bool bInSkill;

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch010_skill_01_stand_up", "ch010_skill_01_stand_mid", "ch010_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch010_skill_01_fall_up", "ch010_skill_01_fall_mid", "ch010_skill_01_fall_down" };
		return new string[2][] { array, array2 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch010_skill_02_jump", "ch010_skill_02_jump" };
	}

	public override void ClearSkill()
	{
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
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
		case OrangeCharacter.SubStatus.VAVA_CANNON_GROUND:
		case OrangeCharacter.SubStatus.VAVA_CANNON_AIR:
			if ((double)_refEntity.CurrentFrame > 0.0 && bInSkill)
			{
				bInSkill = false;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			}
			break;
		case OrangeCharacter.SubStatus.VAVA_KNEE_GROUND:
		case OrangeCharacter.SubStatus.VAVA_KNEE_AIR:
			if ((double)_refEntity.CurrentFrame > 0.0 && bInSkill)
			{
				bInSkill = false;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			}
			break;
		}
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[3];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "sub_weapon_shootpoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "sub_L_knee_shootpoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "sub_R_knee_shootpoint", true);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
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
				_refEntity.DisableCurrentWeapon();
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.VAVA_CANNON_GROUND);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.VAVA_CANNON_AIR);
					_refEntity.IgnoreGravity = true;
				}
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.DisableCurrentWeapon();
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.VAVA_KNEE_GROUND);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.VAVA_KNEE_AIR);
				}
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		string s_USE_MOTION = weaponStruct.BulletData.s_USE_MOTION;
		if (!(s_USE_MOTION == "VAVA_CANNON"))
		{
			if (s_USE_MOTION == "VAVA_KNEE")
			{
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[1], weaponStruct.SkillLV, null, false);
			}
		}
		else
		{
			_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
		}
	}
}
