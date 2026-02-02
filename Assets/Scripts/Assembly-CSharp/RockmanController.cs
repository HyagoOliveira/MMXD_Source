using System;
using StageLib;
using UnityEngine;

public class RockmanController : CharacterControlBase
{
	private Vector3 CtrlShotDir;

	private int nStopFrame;

	private bool bInShootBullet;

	private CollideBullet mCollideBullet;

	private int back_skill_id;

	private int nLastSkill1Index;

	private ChargeShootObj _refChargeShootObj;

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch018_skill_01_stand_up", "ch018_skill_01_stand", "ch018_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch018_skill_01_jump_up", "ch018_skill_01_jump", "ch018_skill_01_jump_down" };
		return new string[2][] { array, array2 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[8] { "ch018_skill_02_jump_first_start", "ch018_skill_02_jump_first_end", "ch018_skill_02_jump_second_start", "ch018_skill_02_jump_second_end", "ch018_skill_02_stand_first_start", "ch018_skill_02_stand_first_end", "ch018_skill_02_stand_second_start", "ch018_skill_02_stand_second_end" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		mCollideBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(_refEntity.PlayerSkills[1].BulletData.s_MODEL);
		StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			BulletBase component = ((GameObject)obj).GetComponent<BulletBase>();
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BasicBullet>(UnityEngine.Object.Instantiate(component), "p_rockbuster_003", 5);
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/p_rockbuster_003", "p_rockbuster_003", loadCallBackObj.LoadCB);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_rockbuster_003", 2);
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		_refChargeShootObj.ChargeMaxSE = AudioManager.FormatEnum2Name(SkillSE_ROCKMAN.CRI_SKILLSE_ROCKMAN_RM_CHARGEMAX.ToString());
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeSE = new string[3] { "SkillSE_ROCKMAN", "rm_charge_lp", "rm_charge_stop" };
		}
		else
		{
			_refChargeShootObj.ChargeSE = new string[3] { "BattleSE02", "bt_rm_charge_lp", "bt_rm_charge_stop" };
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
		_refEntity.EnterRideArmorEvt = OnEnterRideArmor;
	}

	private Vector3 GetShotDir(Vector3 tShotPos)
	{
		return CtrlShotDir;
	}

	private bool checkCancelAnimate(int skilliD)
	{
		if (skilliD == 0)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
		}
		else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
		{
			return true;
		}
		return false;
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length != 2)
		{
			return;
		}
		int num = (int)parameters[0];
		int num2 = (int)parameters[1];
		if (num != 1)
		{
			return;
		}
		if (num2 == 0)
		{
			if (mCollideBullet == null)
			{
				mCollideBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(_refEntity.PlayerSkills[1].BulletData.s_MODEL);
			}
			if (mCollideBullet.IsActivate)
			{
				mCollideBullet.Reset_Duration_Time();
				mCollideBullet.BackToPool();
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[num]);
			}
			if (_refEntity is OrangeConsoleCharacter)
			{
				OrangeConsoleCharacter obj = _refEntity as OrangeConsoleCharacter;
				obj.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
				obj.ClearVirtualButtonStick(VirtualButtonId.SKILL1);
			}
		}
		else
		{
			if (_refEntity is OrangeConsoleCharacter)
			{
				(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
			}
			if (!mCollideBullet.IsActivate)
			{
				mCollideBullet.UpdateBulletData(_refEntity.PlayerSkills[num].FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				mCollideBullet.UseExtraCollider = true;
				mCollideBullet.SetBulletAtk(_refEntity.PlayerSkills[num].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				mCollideBullet.BulletLevel = _refEntity.PlayerSkills[num].SkillLV;
				mCollideBullet.Active(_refEntity.transform, Quaternion.identity, _refEntity.TargetMask, true, (_refEntity.PlayerSkills[num].BulletData.n_TRACKING > 0) ? _refEntity.PlayerAutoAimSystem.AutoAimTarget : null);
			}
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.SkillEnd)
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.CurSubStatus != OrangeCharacter.SubStatus.SKILL0 && _refEntity.CurSubStatus != OrangeCharacter.SubStatus.SKILL0_1)
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
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nStopFrame > 0)
			{
				nStopFrame--;
				if (nStopFrame == 9)
				{
					Quaternion p_quaternion = Quaternion.FromToRotation(Vector3.right, GetShotDir(Vector3.zero));
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_rockbuster_003", _refEntity.PlayerSkills[back_skill_id].ShootTransform[0].position, p_quaternion, Array.Empty<object>());
				}
				else if (nStopFrame == 0)
				{
					_refEntity.Animator._animator.speed = 1f;
					_refEntity.PushBulletDetail(_refEntity.PlayerSkills[back_skill_id].FastBulletDatas[3], _refEntity.PlayerSkills[back_skill_id].weaponStatus, _refEntity.ExtraTransforms[0].position, _refEntity.PlayerSkills[back_skill_id].SkillLV, GetShotDir(Vector3.zero));
				}
			}
			else if (checkCancelAnimate(0))
			{
				SkillEndChnageToIdle();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_4:
			if ((double)_refEntity.CurrentFrame > 0.35 && !bInShootBullet)
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				bInShootBullet = true;
			}
			else if ((double)_refEntity.CurrentFrame > 0.35 && bInShootBullet && checkCancelAnimate(1))
			{
				SkillEndChnageToIdle();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
		case OrangeCharacter.SubStatus.SKILL1_6:
			if ((double)_refEntity.CurrentFrame > 0.5 && checkCancelAnimate(1))
			{
				SkillEndChnageToIdle();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
		case OrangeCharacter.SubStatus.SKILL1_7:
			if (checkCancelAnimate(1))
			{
				SkillEndChnageToIdle();
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentSkillObj());
			_refEntity.EnableCurrentWeapon();
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.IDLE)
			{
				_refEntity.CancelBusterChargeAtk();
			}
			break;
		case 1:
			if (_refEntity is OrangeConsoleCharacter)
			{
				(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
			}
			_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkill1Index].n_ID);
			_refEntity.GetCurrentSkillObj().Reload_index = 0;
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			break;
		}
		if (_refEntity.PlayerSkills[1].ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
		{
			if (_refEntity is OrangeConsoleCharacter)
			{
				(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
			}
			_refEntity.PlayerSkills[1].Reload_index = 0;
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[1].FastBulletDatas[1].n_ID);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
		}
		if (_refEntity.Animator._animator.speed == 0f)
		{
			_refEntity.Animator._animator.speed = 1f;
		}
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	private void ShotShield()
	{
		_refEntity.PlaySE(_refEntity.VoiceID, 26);
		_refEntity.PlaySE(_refEntity.SkillSEID, 10);
		_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkill1Index], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ModelTransform.position + new Vector3(0f, 0.64f, 0f), _refEntity.GetCurrentSkillObj().SkillLV);
		_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
		_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkill1Index].n_ID);
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				ShotShield();
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_6:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				ShotShield();
				break;
			case OrangeCharacter.SubStatus.SKILL1_7:
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
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_5);
				break;
			case OrangeCharacter.SubStatus.SKILL1_5:
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
				break;
			case OrangeCharacter.SubStatus.SKILL1_6:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_7);
				break;
			case OrangeCharacter.SubStatus.SKILL1_7:
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
				else if (_refEntity.PlayerSkills[id].ChargeLevel < 3)
				{
					ShootChargeBuster(id);
				}
				else
				{
					ShootChargeBusterMAX(id);
				}
			}
			else
			{
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, 0);
				_refEntity.PlaySE(_refEntity.VoiceID, 7);
			}
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
			if (_refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				if (_refEntity.PlayerSkills[id].ChargeLevel < 3)
				{
					ShootChargeBuster(id);
				}
				else
				{
					ShootChargeBusterMAX(id);
				}
			}
			break;
		case 1:
		{
			if (_refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInShootBullet = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.StopShootTimer();
			_refEntity.DisableCurrentWeapon();
			nLastSkill1Index = _refEntity.GetCurrentSkillObj().Reload_index;
			if (_refEntity.GetCurrentSkillObj().Reload_index == 0)
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
					break;
				}
				_refEntity.IgnoreGravity = true;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				break;
			}
			CtrlShotDir = _refEntity.ShootDirection;
			int num = Math.Sign(CtrlShotDir.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(CtrlShotDir.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_6);
				break;
			}
			_refEntity.IgnoreGravity = true;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			break;
		}
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
	}

	private void ShootChargeBuster(int id)
	{
		_refChargeShootObj.StopCharge(id);
		if (_refEntity.PlayerSkills[id].ChargeLevel <= 0)
		{
			return;
		}
		if (_refEntity.PlayerSkills[id].ChargeLevel == 2)
		{
			bool flag = false;
			foreach (RefPassiveskill.PassiveskillStatus item in _refEntity.tRefPassiveskill.listUsePassiveskill)
			{
				if (item.tSKILL_TABLE.n_ID == 11861)
				{
					flag = true;
				}
			}
			if (flag)
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			else
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 7);
			}
		}
		else
		{
			_refEntity.PlaySE(_refEntity.VoiceID, 7);
		}
		_refEntity.CurrentActiveSkill = id;
		_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel);
		_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0], null, _refEntity.PlayerSkills[id].ChargeLevel);
	}

	private void ShootChargeBusterMAX(int id)
	{
		_refEntity.PlaySE(_refEntity.VoiceID, 8);
		_refChargeShootObj.StopCharge(id);
		_refEntity.SetSpeed(0, 0);
		_refEntity.SkillEnd = false;
		_refEntity.StopShootTimer();
		_refEntity.CheckUsePassiveSkill(back_skill_id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.ExtraTransforms[0], null, 3);
		OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[id], _refEntity.PlayerSkills[id].FastBulletDatas[3].n_USE_COST, -1f);
		CtrlShotDir = _refEntity.ShootDirection;
		int num2 = (_refEntity.CurrentActiveSkill = id);
		back_skill_id = num2;
		_refEntity.UpdateWeaponMesh(_refEntity.PlayerSkills[id], _refEntity.GetCurrentWeaponObj());
		int num3 = Math.Sign(_refEntity.ShootDirection.x);
		if (_refEntity.direction != num3 && Mathf.Abs(_refEntity.ShootDirection.x) > 0.05f)
		{
			_refEntity.direction *= -1;
		}
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
		else
		{
			_refEntity.IgnoreGravity = true;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
		}
		_refEntity.Animator._animator.speed = 0f;
		nStopFrame = 10;
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, _refEntity.ShootDirection)) / 180f;
		_refEntity.Animator._animator.SetFloat(_refEntity.Animator.hashDirection, value);
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[4] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[4] { "ch018_skill_01_stand", "ch018_skill_01_fall", "ch018_skill_01_wallgrab", "ch018_skill_01_crouch" };
	}

	public void StageTeleportInCharacterDepend()
	{
		if (mCollideBullet.IsActivate)
		{
			mCollideBullet.UpdateFx();
		}
	}

	private void SkillEndChnageToIdle()
	{
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			_refEntity.SkillEnd = true;
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
		}
	}

	private bool OnEnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (mCollideBullet != null && mCollideBullet.IsActivate)
		{
			mCollideBullet.Reset_Duration_Time();
			mCollideBullet.BackToPool();
			if (_refEntity is OrangeConsoleCharacter)
			{
				OrangeConsoleCharacter obj = _refEntity as OrangeConsoleCharacter;
				obj.UpdateSkillIconByBuff(1, 0);
				obj.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
				obj.ClearVirtualButtonStick(VirtualButtonId.SKILL1);
			}
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}
}
