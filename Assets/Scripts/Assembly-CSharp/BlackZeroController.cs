#define RELEASE
using System;
using UnityEngine;

public class BlackZeroController : CharacterControlBase
{
	private bool bInSkill;

	private bool isShowSkillWeapon;

	private FxBase tSkill0Fx;

	private Transform R_Hand;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch004_skill_01_alt", "ch004_skill_02_stand_start", "ch004_skill_02_stand_loop", "ch004_skill_02_stand_end", "ch004_skill_02_jump_start", "ch004_skill_02_jump_loop", "ch004_skill_02_jump_end" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_lightning_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_fallen_001");
		R_Hand = OrangeBattleUtility.FindChildRecursive(_refEntity._transform, "HandMesh_R");
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
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.TELEPORT_POSE:
			if ((double)_refEntity.CurrentFrame > 0.38)
			{
				bInSkill = false;
				Debug.Log("Trigger Skill!");
				_refEntity.PlaySE(_refEntity.SkillSEID, 3);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_fallen_001", R_Hand.position, Quaternion.identity, Array.Empty<object>());
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0]);
				_refEntity.GravityMultiplier = new VInt(1f);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 0.6f && isShowSkillWeapon)
			{
				isShowSkillWeapon = false;
				_refEntity.DisableWeaponMesh(_refEntity.PlayerSkills[0]);
			}
			break;
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
			_refEntity.IgnoreGravity = true;
			_refEntity.SetAnimateId((HumanBase.AnimateId)(66 + ((!_refEntity.PreBelow) ? 3 : 0)));
			_refEntity.PlaySE(_refEntity.VoiceID, 8);
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 4f), 0);
			_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[0].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[0].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[0].SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			_refEntity.SetAnimateId((HumanBase.AnimateId)(67 + ((!_refEntity.PreBelow) ? 3 : 0)));
			tSkill0Fx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_lightning_000", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			ParticleSystem[] componentsInChildren = tSkill0Fx.transform.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ParticleSystem.MainModule main = componentsInChildren[i].main;
				main.simulationSpeed = 4f;
			}
			_refEntity.PlaySE(_refEntity.SkillSEID, 1);
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SetAnimateId((HumanBase.AnimateId)(68 + ((!_refEntity.PreBelow) ? 3 : 0)));
			break;
		}
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.GetCurrentSkillObj());
			_refEntity.CharacterMaterials.Appear();
			_refEntity.BulletCollider.BackToPool();
			break;
		case 1:
			_refEntity.EnableCurrentWeapon();
			_refEntity.GravityMultiplier = new VInt(1f);
			break;
		}
		_refEntity.Dashing = false;
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.CurrentActiveSkill = -1;
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			_refEntity.CharacterMaterials.Disappear();
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if ((bool)_refEntity.Controller.BelowInBypassRange)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				ClearSkill();
			}
			_refEntity.IgnoreGravity = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.CharacterMaterials.Appear();
			_refEntity.BulletCollider.BackToPool();
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SkillEnd = true;
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
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
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerSkills[0].MagazineRemain -= _refEntity.PlayerSkills[0].BulletData.n_USE_COST;
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
				_refEntity.PlayerSkills[0].LastUseTimer.TimerStart();
				_refEntity.UpdateWeaponMesh(_refEntity.PlayerSkills[id], _refEntity.GetCurrentWeaponObj());
				isShowSkillWeapon = true;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerSkills[1].LastUseTimer.TimerStart();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.GravityMultiplier = new VInt(3f);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		SKILL_TABLE bulletData = weaponStruct.BulletData;
		BulletBase poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>(bulletData.s_MODEL);
		if ((bool)poolObj)
		{
			Vector3 position = _refEntity.ExtraTransforms[0].position;
			position.x += (float)_refEntity.direction * -0.25f;
			poolObj.UpdateBulletData(bulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			poolObj.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			poolObj.BulletLevel = weaponStruct.SkillLV;
			poolObj.Active(position, Vector3.up, _refEntity.TargetMask);
		}
	}
}
