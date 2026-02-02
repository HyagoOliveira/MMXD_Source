using System.Collections;
using UnityEngine;

public class PaletteController : CharacterControlBase
{
	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch013_skill_01_stand_up", "ch013_skill_01_stand_mid", "ch013_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch013_skill_01_jump_up", "ch013_skill_01_jump_mid", "ch013_skill_01_jump_down" };
		string[] array3 = new string[3] { "ch013_skill_02_stand_up", "ch013_skill_02_stand_mid", "ch013_skill_02_stand_down" };
		string[] array4 = new string[3] { "ch013_skill_02_jump_up", "ch013_skill_02_jump_mid", "ch013_skill_02_jump_down" };
		return new string[4][] { array, array2, array3, array4 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch013_skill_02_end" };
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
		case OrangeCharacter.SubStatus.PALETTE_ATTACK_GROUND:
		case OrangeCharacter.SubStatus.PALETTE_ATTACK_AIR:
			if (_refEntity.CurrentFrame > 1f)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				StartCoroutine(PlasmaGunSparkSE());
				if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.PALETTE_ATTACK_AIR)
				{
					_refEntity.SkillEnd = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.PALETTE_ATTACK_END);
				}
				_refEntity.IgnoreGravity = false;
			}
			break;
		case OrangeCharacter.SubStatus.PALETTE_ATTACK_END:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SkillEnd = true;
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			break;
		case OrangeCharacter.SubStatus.PALETTE_ARROW_GROUND:
		case OrangeCharacter.SubStatus.PALETTE_ARROW_AIR:
			if (_refEntity.CurrentFrame > 1f && _refEntity.GetCurrentSkillObj().LastUseTimer.GetMillisecond() > 300)
			{
				if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.PALETTE_ARROW_AIR)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				else
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			int currentActiveSkill = _refEntity.CurrentActiveSkill;
			if ((uint)currentActiveSkill <= 1u)
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentSkillObj());
				_refEntity.EnableCurrentWeapon();
			}
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
		}
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
				_refEntity.IsShoot = 1;
				_refEntity.CheckLockDirection();
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.PALETTE_ARROW_GROUND);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.PALETTE_ARROW_AIR);
				}
				_refEntity.DisableCurrentWeapon();
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[id]);
				CreateSkillBullet(_refEntity.PlayerSkills[id]);
				_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				_refEntity.IsShoot = 1;
				_refEntity.CheckLockDirection();
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.PALETTE_ATTACK_GROUND);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.PALETTE_ATTACK_AIR);
				}
				_refEntity.UpdateWeaponMesh(_refEntity.PlayerSkills[id], _refEntity.GetCurrentWeaponObj());
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
			}
			break;
		}
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[2];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L BusterPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R BusterPoint", true);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		SKILL_TABLE bulletData = weaponStruct.BulletData;
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		string s_USE_MOTION = bulletData.s_USE_MOTION;
		if (!(s_USE_MOTION == "PALETTE_ARROW"))
		{
			if (s_USE_MOTION == "PALETTE_ATTACK")
			{
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, weaponStruct.ShootTransform[0].position, weaponStruct.SkillLV);
			}
		}
		else
		{
			_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN && subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			_refEntity.EnableSkillWeapon(0);
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN && subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			_refEntity.DisableSkillWeapon(0);
		}
	}

	private IEnumerator PlasmaGunSparkSE()
	{
		yield return new WaitForSeconds(0.2f);
		_refEntity.PlaySE(_refEntity.SkillSEID, 2);
		yield return new WaitForSeconds(1f);
		_refEntity.PlaySE(_refEntity.SkillSEID, 2);
	}
}
