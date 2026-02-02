using System;
using UnityEngine;

public class AliaLoverController : CharacterControlBase
{
	private bool bInSkill;

	private Vector3 CtrlShotDir;

	private bool throwKissSE02;

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_AliaLover_in";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[8] { "ch027_skill_01_jump", "ch027_skill_01_stand", "ch027_skill_02_jump_down", "ch027_skill_02_jump_mid", "ch027_skill_02_jump_up", "ch027_skill_02_stand_down", "ch027_skill_02_stand_mid", "ch027_skill_02_stand_up" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_lovebullet_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_lovebullet_001", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	private Vector3 GetShotDir(Vector3 tShotPos)
	{
		return CtrlShotDir;
	}

	private Vector3 GetShotDirByWeaponDir()
	{
		Vector3 result = _refEntity.ShootDirection;
		if (_refEntity.UseAutoAim && _refEntity.CurrentActiveSkill < 2 && _refEntity.IAimTargetLogicUpdate != null && _refEntity.IAimTargetLogicUpdate.AimTransform != null)
		{
			Transform transform = _refEntity.ExtraTransforms[0];
			result = ((_refEntity.IAimTargetLogicUpdate.AimTransform.position + _refEntity.IAimTargetLogicUpdate.AimPoint).xy() - transform.position.xy()).normalized;
		}
		return result;
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || !bInSkill)
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
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.CurrentFrame > 0.25f)
			{
				bInSkill = false;
				_refEntity.MuteBullet = true;
				_refEntity.PlaySE(_refEntity.SkillSEID, 6);
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[0], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0].position, _refEntity.GetCurrentSkillObj().SkillLV, Vector3.up);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_4:
			if (_refEntity.CurrentFrame > 0.15f && !throwKissSE02)
			{
				_refEntity.PlaySE(_refEntity.SkillSEID, 5);
				throwKissSE02 = true;
			}
			if (_refEntity.CurrentFrame > 0.25f)
			{
				bInSkill = false;
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[0], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0].position, _refEntity.GetCurrentSkillObj().SkillLV, GetShotDirByWeaponDir());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.CurrentActiveSkill = -1;
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			}
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL1_3:
			case OrangeCharacter.SubStatus.SKILL1_4:
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
				break;
			}
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
		}
		else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.SetSpeed(0, 0);
			_refEntity.StopShootTimer();
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_lovebullet_001", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			_refEntity.DisableCurrentWeapon();
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			_refEntity.PlaySE(_refEntity.VoiceID, 10);
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		SKILL_TABLE bulletDatum = weaponStruct.BulletData;
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp > 0 && id != 0 && id == 1 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.SetSpeed(0, 0);
			CtrlShotDir = _refEntity.ShootDirection;
			int num = Math.Sign(CtrlShotDir.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(CtrlShotDir.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
			_refEntity.StopShootTimer();
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_lovebullet_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			_refEntity.DisableCurrentWeapon();
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			_refEntity.PlaySE(_refEntity.SkillSEID, 4);
			throwKissSE02 = false;
		}
	}
}
