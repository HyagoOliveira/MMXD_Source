using System;
using UnityEngine;

public class MassimoController : CharacterControlBase
{
	private bool bInSkill;

	private int movecount;

	private CharacterMaterial mAxe_000_G;

	private FxBase fx_berserk;

	private FxBase fx_whirlwind;

	private bool isPlayTeleportOut;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[8] { "ch005_skill_01_start", "ch005_skill_01_mid_1", "ch005_skill_01_mid_2", "ch005_skill_01_mid_3", "ch005_skill_01_end", "ch005_skill_02_start", "ch005_skill_02_loop", "ch005_skill_02_end" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "Axe_000_G", true);
		if ((bool)transform)
		{
			mAxe_000_G = transform.GetComponent<CharacterMaterial>();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_berserk_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_whirlwind_000", 2);
		WeaponStruct[] playerWeapons = _refEntity.PlayerWeapons;
		foreach (WeaponStruct weaponStruct in playerWeapons)
		{
			if (weaponStruct == null)
			{
				continue;
			}
			CharacterMaterial[] weaponMesh = weaponStruct.WeaponMesh;
			foreach (CharacterMaterial characterMaterial in weaponMesh)
			{
				if (characterMaterial != null)
				{
					characterMaterial.ChangeDissolveModelModelHeight(characterMaterial.GetDissolveModelHeight() * 1.3f);
				}
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
	}

	public void TeleportOutCharacterDepend()
	{
		if (!isPlayTeleportOut)
		{
			UpdateWeaponMesh(true);
			isPlayTeleportOut = true;
			CharacterMaterial[] componentsInChildren = _refEntity.GetComponentsInChildren<CharacterMaterial>();
			if (componentsInChildren != null && componentsInChildren.Length > 1)
			{
				componentsInChildren[0].SetSubCharacterMaterial(mAxe_000_G);
			}
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || !bInSkill)
		{
			return;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus != OrangeCharacter.SubStatus.SKILL0_3)
			{
				int num = 50;
			}
			else if (_refEntity.CurrentFrame > 0.2f && bInSkill)
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.BulletCollider.BackToPool();
			}
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			if (_refEntity.CurrentActiveSkill == 0)
			{
				UpdateWeaponMesh(false);
				_refEntity.BulletCollider.BackToPool();
			}
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.GetCurrentSkillObj());
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				fx_whirlwind = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_whirlwind_000", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 0.5f), 0);
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[0].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[0].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[0].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 0.5f), 0);
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[0].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[0].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[0].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.PlaySE(_refEntity.SkillSEID, 1);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				fx_berserk = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_berserk_000", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			}
		}
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
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			_refEntity.BulletCollider.BackToPool();
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (movecount < 1)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			}
			movecount++;
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_4);
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			_refEntity.SkillEnd = true;
			UpdateWeaponMesh(false);
			_refEntity.SetSpeed(0, 0);
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			_refEntity.PlayerAutoAimSystem.UpdateAimRange(_refEntity.PlayerSkills[1].BulletData.f_DISTANCE);
			_refEntity.UpdateAimDirection();
			CreateSkillBullet(_refEntity.PlayerSkills[1]);
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SkillEnd = true;
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
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
			if (_refEntity.CurrentActiveSkill == -1 && (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IgnoreGravity = false;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				movecount = 0;
				_refEntity.PlayerSkills[0].MagazineRemain -= _refEntity.PlayerSkills[0].BulletData.n_USE_COST;
				_refEntity.PlayerSkills[0].LastUseTimer.TimerStart();
				_refEntity.DisableCurrentWeapon();
				UpdateWeaponMesh(true);
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IgnoreGravity = false;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerSkills[1].MagazineRemain -= _refEntity.PlayerSkills[1].BulletData.n_USE_COST;
				_refEntity.PlayerSkills[1].LastUseTimer.TimerStart();
				_refEntity.DisableCurrentWeapon();
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
				int num = Math.Sign(_refEntity.ShootDirection.x);
				if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(_refEntity.ShootDirection.x) > 0.05f)
				{
					_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		float num = 1f;
		if (_refEntity._characterDirection == CharacterDirection.LEFT)
		{
			num = -1f;
		}
		Vector3 shootPosition = new Vector3(_refEntity.ExtraTransforms[0].position.x + num, _refEntity.ExtraTransforms[0].position.y + 0.5f, _refEntity.ExtraTransforms[0].position.z);
		_refEntity.PushBulletDetail(_refEntity.PlayerSkills[1].BulletData, _refEntity.PlayerSkills[1].weaponStatus, shootPosition, _refEntity.PlayerSkills[1].SkillLV, _refEntity.ShootDirection);
	}

	public override void SetStun(bool enable)
	{
		if (fx_berserk != null)
		{
			fx_berserk.BackToPool();
			fx_berserk = null;
		}
		if (fx_whirlwind != null)
		{
			fx_whirlwind.BackToPool();
			fx_whirlwind = null;
		}
	}

	private void UpdateWeaponMesh(bool enable)
	{
		if ((bool)mAxe_000_G)
		{
			if (enable)
			{
				mAxe_000_G.Appear();
			}
			else
			{
				mAxe_000_G.Disappear();
			}
		}
	}
}
