#define RELEASE
using System;
using UnityEngine;

public class UltimateXController : CharacterControlBase
{
	private OrangeTimer NOVASTRIKETimer;

	private ChargeShootObj _refChargeShootObj;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch008_skill_02_start_pose_loop" };
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch008_skill_02_start", "ch008_skill_02_loop" };
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.CheckSkillEndByShootTimer();
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.RIDE_ARMOR:
			if (_refEntity.CurrentActiveSkill != 1)
			{
				Debug.LogError("_CurrentActiveSkill != 1 => " + _refEntity.CurrentActiveSkill);
				_refEntity.CurrentActiveSkill = 1;
			}
			if (_refEntity.Velocity.y <= 0)
			{
				Debug.Log("Trigger Skill!");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_novastrike_000", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_novastrike_002", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
				NOVASTRIKETimer.TimerStart();
				_refEntity.SetSpeed((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 2.5f), 0);
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.GetCurrentSkillObj().SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.SetStatus(_refEntity.CurMainStatus, OrangeCharacter.SubStatus.IDLE);
				_refEntity.ToggleExtraMesh(true);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			}
			break;
		case OrangeCharacter.SubStatus.IDLE:
			if (_refEntity.CurrentActiveSkill != 1)
			{
				Debug.LogError("_CurrentActiveSkill != 1 => " + _refEntity.CurrentActiveSkill);
				_refEntity.CurrentActiveSkill = 1;
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_novastrike_001", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			if (NOVASTRIKETimer.GetMillisecond() > 417)
			{
				_refEntity.ToggleExtraMesh(false);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SetSpeed(0, 0);
				_refEntity.SkillEnd = true;
				_refEntity.BulletCollider.BackToPool();
			}
			break;
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
				_refEntity.ToggleExtraMesh(false);
				_refEntity.BulletCollider.BackToPool();
				_refEntity.EnableCurrentWeapon();
				break;
			}
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
		}
	}

	public override void Start()
	{
		base.Start();
		NOVASTRIKETimer = OrangeTimerManager.GetTimer();
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "WingCloseMesh");
		if (array != null)
		{
			OrangeCharacter refEntity = _refEntity;
			Renderer[] extraMeshClose = new SkinnedMeshRenderer[array.Length];
			refEntity.ExtraMeshClose = extraMeshClose;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshClose[i] = array[i].GetComponent<SkinnedMeshRenderer>();
			}
		}
		Transform[] array2 = OrangeBattleUtility.FindAllChildRecursive(ref target, "WingMesh");
		if (array2 != null)
		{
			OrangeCharacter refEntity2 = _refEntity;
			Renderer[] extraMeshClose = new SkinnedMeshRenderer[array2.Length];
			refEntity2.ExtraMeshOpen = extraMeshClose;
			for (int j = 0; j < array2.Length; j++)
			{
				_refEntity.ExtraMeshOpen[j] = array2[j].GetComponent<SkinnedMeshRenderer>();
			}
		}
		_refEntity.ToggleExtraMesh(false);
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_UltimateX", "xu_charge_lp", "xu_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE", "bt_xu_charge_lp", "bt_xu_charge_stop" };
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
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
					_refChargeShootObj.ShootChargeBuster(id);
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, 0);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				if (_refEntity.Dashing)
				{
					_refEntity.PlayerStopDashing();
				}
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				_refEntity.SetSpeed((int)_refEntity._characterDirection * OrangeCharacter.WalkSpeed, (int)((float)OrangeCharacter.JumpSpeed * 0.5f));
				_refEntity.StopShootTimer();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.RIDE_ARMOR);
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.StartJumpThroughCorutine();
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
		}
		else if (_refEntity.CurrentActiveSkill == -1 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refChargeShootObj.ShootChargeBuster(id);
		}
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[4] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[4] { "ch008_skill_01_stand", "ch008_skill_01_fall", "ch008_skill_01_wallgrab", "ch008_skill_01_crouch" };
	}
}
