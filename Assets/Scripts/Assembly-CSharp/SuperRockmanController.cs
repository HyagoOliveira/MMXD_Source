using System;
using CallbackDefs;
using UnityEngine;

public class SuperRockmanController : CharacterControlBase
{
	private Vector3 CtrlShotDir;

	public SCH002Controller mPCB;

	private long nPet_time;

	private OrangeTimer _skillTime;

	private bool bPet_Active;

	private int nFlyFrame;

	private int nFlyStatus;

	private bool bJumpflag;

	private bool bCheckJumpFlag = true;

	private ParticleSystem mFX_Booster_L;

	private ParticleSystem mFX_Booster_R;

	private ChargeShootObj _refChargeShootObj;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[5] { "ch025_skill_02_call_jump", "ch025_skill_02_call_stand", "ch025_skill_02_jump_start", "ch025_skill_02_jump_loop", "ch025_skill_02_jump_end" };
	}

	private void InitPetMode()
	{
		bool flag = false;
		int petID = 0;
		int follow_skill_id = 0;
		if (_refEntity.PlayerSkills[0].BulletData.n_EFFECT == 16)
		{
			flag = true;
			petID = (int)_refEntity.PlayerSkills[0].BulletData.f_EFFECT_X;
			nPet_time = (long)(_refEntity.PlayerSkills[0].BulletData.f_EFFECT_Z * 1000f);
			follow_skill_id = 0;
		}
		else if (_refEntity.PlayerSkills[1].BulletData.n_EFFECT == 16)
		{
			flag = true;
			petID = (int)_refEntity.PlayerSkills[1].BulletData.f_EFFECT_X;
			nPet_time = (long)(_refEntity.PlayerSkills[1].BulletData.f_EFFECT_Z * 1000f);
			follow_skill_id = 1;
		}
		if (flag)
		{
			PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
			petBuilder.PetID = petID;
			petBuilder.follow_skill_id = follow_skill_id;
			petBuilder.CreatePet(delegate(SCH002Controller obj)
			{
				mPCB = obj;
				mPCB.Set_follow_Player(_refEntity);
			});
		}
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		mFX_Booster_L = OrangeBattleUtility.FindChildRecursive(ref target, "FX_Booster_L", true).GetComponent<ParticleSystem>();
		mFX_Booster_R = OrangeBattleUtility.FindChildRecursive(ref target, "FX_Booster_R", true).GetComponent<ParticleSystem>();
		_skillTime = OrangeTimerManager.GetTimer();
		InitPetMode();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_pet_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_pet_001", 2);
		_refChargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
		_refChargeShootObj.ChargeSE = ((!_refEntity.IsLocalPlayer) ? new string[3] { "BattleSE02", "bt_rm2_charge_lp", "bt_rm2_charge_stop" } : new string[3] { "SkillSE_ROCKMAN2", "rm2_charge_lp", "rm2_charge_stop" });
		_refChargeShootObj.ChargeLV3SE = AudioManager.FormatEnum2Name(SkillSE_ROCKMAN2.CRI_SKILLSE_ROCKMAN2_RM2_CHARGEMAX.ToString());
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	private Vector3 GetShotDir(Vector3 tShotPos)
	{
		return CtrlShotDir;
	}

	private void CheckJumpSkill()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.WIN_POSE && _refEntity.CurMainStatus == OrangeCharacter.MainStatus.WALLKICK && !_refEntity.CanUseDash())
		{
			bJumpflag = true;
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.AIRDASH)
		{
			if (!mFX_Booster_L.isPlaying)
			{
				mFX_Booster_L.Play();
				mFX_Booster_R.Play();
			}
		}
		else if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus != OrangeCharacter.SubStatus.SKILL1_2 && mFX_Booster_L.isPlaying)
		{
			mFX_Booster_L.Stop();
			mFX_Booster_R.Stop();
		}
		if (!_refEntity.Controller.Collisions.below && !_refEntity.CanUseDash() && bCheckJumpFlag)
		{
			if (!_refEntity.IsStun && (ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.JUMP) || ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.DASH)))
			{
				if (!bJumpflag)
				{
					bJumpflag = true;
				}
				else
				{
					bool flag = _refEntity.PlayerPressJumpCB == new Callback(_refEntity.PlayerPressJump);
					if (_refEntity.CurrentActiveSkill == -1 && flag)
					{
						_refEntity.CurrentActiveSkill = 1;
						_refEntity.SkillEnd = false;
						nFlyFrame = 2;
						_refEntity.SetSpeed(0, 0);
						_refEntity.IgnoreGravity = false;
						nFlyStatus = 0;
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
						mFX_Booster_L.Play();
						mFX_Booster_R.Play();
						bCheckJumpFlag = false;
						_refEntity.PlaySE(_refEntity.SkillSEID, 6);
						_refEntity.Check3rdJumpTrigger(false);
					}
				}
			}
		}
		else if (bJumpflag)
		{
			bJumpflag = false;
		}
		if (_refEntity.Controller.Collisions.below)
		{
			bCheckJumpFlag = true;
		}
	}

	public override void CheckSkill()
	{
		CheckJumpSkill();
		if ((_refEntity.bLockInputCtrl || _skillTime.GetMillisecond() > nPet_time) && bPet_Active)
		{
			bPet_Active = false;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_pet_001", mPCB.transform.position, Quaternion.identity, Array.Empty<object>());
			mPCB.SetActive(false);
		}
		if (_refEntity.IsAnimateIDChanged() || _refEntity.SkillEnd)
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
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nFlyStatus != 0)
			{
				break;
			}
			if (nFlyFrame > 0)
			{
				_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.5f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f));
				nFlyFrame--;
				break;
			}
			nFlyStatus = 1;
			nFlyFrame = 3;
			if ((int)_refEntity.Hp > 0)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (nFlyStatus != 1)
			{
				break;
			}
			if (nFlyFrame > 0)
			{
				nFlyFrame--;
				_refEntity.SetSpeed((int)_refEntity._characterDirection * Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.5f), Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 0.8f));
				break;
			}
			_refEntity.SetSpeed(0, 0);
			if ((int)_refEntity.Hp > 0)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			bJumpflag = false;
			mFX_Booster_L.Stop();
			mFX_Booster_R.Stop();
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentSkillObj());
			_refEntity.EnableCurrentWeapon();
			_refEntity.CancelBusterChargeAtk();
		}
		if (bPet_Active)
		{
			bPet_Active = false;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_pet_001", mPCB.transform.position, Quaternion.identity, Array.Empty<object>());
			_skillTime.TimerStop();
			mPCB.SetActive(false);
		}
		if (mFX_Booster_L.isPlaying)
		{
			mFX_Booster_L.Stop();
		}
		if (mFX_Booster_R.isPlaying)
		{
			mFX_Booster_R.Stop();
		}
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
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
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
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
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
			case OrangeCharacter.SubStatus.SKILL1_3:
				break;
			}
		}
	}

	private void ShootChargeBuster(int id)
	{
		_refChargeShootObj.StopCharge(id);
		if (_refEntity.PlayerSkills[id].ChargeLevel > 0)
		{
			_refEntity.PlaySE(_refEntity.VoiceID, 8);
			_refEntity.CurrentActiveSkill = id;
			if (_refEntity.PlayerSkills[id].ChargeLevel == 2)
			{
				_refEntity.DisableCurrentWeapon();
				_refEntity.EnableHandMesh(false);
				_refEntity.Animator.SetAnimatorEquip(_refEntity.PlayerSkills[id].WeaponData.n_TYPE);
			}
			_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, _refEntity.PlayerSkills[id].ChargeLevel, null, _refEntity.PlayerSkills[id].ChargeLevel != 2);
			_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
		}
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
					ShootChargeBuster(id);
				}
			}
			else if (_refEntity.CurrentActiveSkill == -1)
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				_refEntity.PlayerShootBuster(_refEntity.PlayerSkills[id], true, id, 0);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bPet_Active = true;
				_skillTime.TimerStart();
				mPCB.SetActive(true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_pet_000", mPCB.transform, Quaternion.identity, Array.Empty<object>());
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
					_refEntity.SetSpeed(0, 0);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				}
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(id, _refEntity.PlayerSkills[id].weaponStatus, _refEntity.PlayerSkills[id].ShootTransform[0]);
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
				ShootChargeBuster(id);
			}
		}
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[2] { "buster_stand_charge_atk", "buster_fall_charge_atk" };
		target = new string[2] { "ch025_skill_01_stand", "ch025_skill_01_jump" };
	}

	public bool CheckPetActive(int petId)
	{
		if (mPCB != null && mPCB.Activate && mPCB.PetID == petId)
		{
			return true;
		}
		return false;
	}
}
