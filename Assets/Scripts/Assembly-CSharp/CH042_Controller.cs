using System;
using StageLib;
using UnityEngine;

public class CH042_Controller : CharacterControlBase
{
	private bool bInSkill;

	public SCH008Controller mPCB;

	public SCH008Controller mPCB2;

	private int nPetID = -1;

	private long nPetTime;

	private int nPet2ID = -1;

	private long nPet2Time;

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_irisswimsuit_in";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[12]
		{
			"ch042_skill_01_stand_mid", "ch042_skill_01_jump_mid", "ch042_skill_01_crouch_mid", "ch042_skill_01_stand_up", "ch042_skill_01_jump_up", "ch042_skill_01_crouch_up", "ch042_skill_01_stand_down", "ch042_skill_01_jump_down", "ch042_skill_01_crouch_down", "ch042_skill_02_stand",
			"ch042_skill_02_jump", "ch042_skill_02_crouch"
		};
	}

	public void TeleportInExtraEffect()
	{
		_refEntity.PlaySE(_refEntity.SkillSEID, "irs_chara01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[2];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_Swimsuit_Iris_skill01_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_Swimsuit_Iris_skill00_000", 2);
		InitPetMode();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	private void InitPetMode()
	{
		bool flag = false;
		int num = 0;
		int skillIndex = 0;
		if (_refEntity.PlayerSkills[0].BulletData.n_EFFECT == 16)
		{
			flag = true;
			num = (int)_refEntity.PlayerSkills[0].BulletData.f_EFFECT_X;
			nPetTime = (long)(_refEntity.PlayerSkills[0].BulletData.f_EFFECT_Z * 1000f);
			skillIndex = 0;
		}
		else if (_refEntity.PlayerSkills[1].BulletData.n_EFFECT == 16)
		{
			flag = true;
			num = (int)_refEntity.PlayerSkills[1].BulletData.f_EFFECT_X;
			nPetTime = (long)(_refEntity.PlayerSkills[1].BulletData.f_EFFECT_Z * 1000f);
			skillIndex = 1;
		}
		if (flag)
		{
			nPetID = num;
			CreatePet(0, nPetID, skillIndex);
		}
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[_refEntity.CharacterData.n_PASSIVE_5];
		if (sKILL_TABLE.n_EFFECT == 16)
		{
			nPet2ID = (int)sKILL_TABLE.f_EFFECT_X;
			nPet2Time = (long)(sKILL_TABLE.f_EFFECT_Z * 1000f);
			CreatePet(1, nPet2ID, skillIndex);
		}
	}

	protected void CreatePet(int petIndex, int petID, int skillIndex)
	{
		PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
		petBuilder.PetID = petID;
		petBuilder.follow_skill_id = skillIndex;
		petBuilder.CreatePet(delegate(SCH008Controller obj)
		{
			if (petIndex == 0)
			{
				mPCB = obj;
				mPCB.Set_follow_Player(_refEntity);
				mPCB.SetFollowEnabled(false);
			}
			else
			{
				mPCB2 = obj;
				mPCB2.Set_follow_Player(_refEntity);
				mPCB2.SetFollowEnabled(false);
			}
		});
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			SkillEndChnageToIdle();
			break;
		case 1:
			SkillEndChnageToIdle();
			if ((bool)mPCB)
			{
				mPCB.SetFollowEnabled(true);
			}
			break;
		}
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
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.CurrentFrame > 0.22f && bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity._transform);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.CurrentFrame > 0.25f && bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity._transform);
			}
			else if (_refEntity.CurrentFrame > 0.5f && checkCancelAnimate(1))
			{
				AnimationEndCharacterDepend(_refEntity.CurMainStatus, _refEntity.CurSubStatus);
			}
			break;
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
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.StopShootTimer();
				_refEntity.DisableCurrentWeapon();
				int num = Math.Sign(_refEntity.ShootDirection.x);
				if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(_refEntity.ShootDirection.x) > 0.05f)
				{
					_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				}
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				}
				_refEntity.PlaySE(_refEntity.VoiceID, "v_irs_skill01");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_Swimsuit_Iris_skill00_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.StopShootTimer();
				_refEntity.DisableCurrentWeapon();
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				}
				_refEntity.PlaySE(_refEntity.VoiceID, "v_irs_skill02");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_Swimsuit_Iris_skill01_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
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
		{
			_refEntity.SetSpeed(0, 0);
			float num2 = Vector2.SignedAngle(Vector2.right, _refEntity.ShootDirection);
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.Dashing = false;
				if (num2 > 60f && num2 < 120f)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				}
				else if (num2 < -60f && num2 < -120f)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				}
				else
				{
					_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				}
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				if (num2 > 60f && num2 < 120f)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				}
				else if (num2 < -60f && num2 < -120f)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				}
			}
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.Dashing = false;
			float num = Vector2.SignedAngle(Vector2.right, _refEntity.ShootDirection);
			if (num > 60f && num < 120f)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			else if (num < -60f && num < -120f)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetSpeed(0, 0);
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetSpeed(0, 0);
			_refEntity.Dashing = false;
			_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				_refEntity.EnableCurrentWeapon();
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SkillEnd = true;
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.IgnoreGravity = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SkillEnd = true;
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				_refEntity.Dashing = false;
				if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SkillEnd = true;
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				mPCB.SetFollowEnabled(true);
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.IgnoreGravity = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SkillEnd = true;
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				mPCB.SetFollowEnabled(true);
				_refEntity.Dashing = false;
				if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
				}
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[1], wsSkill.SkillLV);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.IsLocalPlayer)
			{
				CallPet(nPetID, false, -1, null);
			}
			break;
		}
	}

	public override void ControlCharacterDead()
	{
		if (mPCB.Activate)
		{
			SCH008Controller sCH008Controller = mPCB;
			if ((bool)sCH008Controller)
			{
				sCH008Controller.FollowPlayerDead();
			}
		}
		if (mPCB2.Activate)
		{
			SCH008Controller sCH008Controller2 = mPCB2;
			if ((bool)sCH008Controller2)
			{
				sCH008Controller2.FollowPlayerDead();
			}
		}
	}

	private bool checkCancelAnimate(int skilliD)
	{
		if (skilliD == 1 && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
		{
			return true;
		}
		return false;
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		float moveSpeed = _refEntity.PlayerSkills[1].BulletData.n_SPEED;
		if (petID == nPetID)
		{
			SCH008Controller sCH008Controller = mPCB;
			if ((bool)sCH008Controller)
			{
				_refEntity.PlaySE(_refEntity.SkillSEID, "irs_crystal01");
				sCH008Controller.SetParam(nPetTime, false, moveSpeed);
				sCH008Controller.SetFollowEnabled(false);
				sCH008Controller.SetActive(true);
				sCH008Controller.SetPositionAndRotation(_refEntity._transform.position + Vector3.up * 2.5f, false);
				if (_refEntity.IsLocalPlayer)
				{
					StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + 0, true);
				}
			}
		}
		else
		{
			if (petID != nPet2ID)
			{
				return;
			}
			SCH008Controller sCH008Controller2 = mPCB2;
			if ((bool)sCH008Controller2)
			{
				sCH008Controller2.SetPositionAndRotation(_refEntity._transform.position + Vector3.up * 2.5f, false);
				sCH008Controller2.SetParam(nPet2Time, true, moveSpeed, _refEntity.IAimTargetLogicUpdate);
				sCH008Controller2.SetFollowEnabled(false);
				sCH008Controller2.SetActive(true);
				if (_refEntity.IsLocalPlayer)
				{
					StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + 0, true);
				}
			}
		}
	}

	private void SkillEndChnageToIdle()
	{
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	public bool CheckPetActive(int petId)
	{
		if (mPCB != null && mPCB.Activate && mPCB.PetID == petId)
		{
			return true;
		}
		if (mPCB2 != null && mPCB2.Activate && mPCB2.PetID == petId)
		{
			return true;
		}
		return false;
	}
}
