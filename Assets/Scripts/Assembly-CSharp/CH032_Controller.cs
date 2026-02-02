using System;
using StageLib;
using UnityEngine;

public class CH032_Controller : CharacterControlBase
{
	private bool bInSkill;

	private CharacterMaterial cmRodMesh;

	public SCH009Controller mIceBit;

	public SCH009Controller mBoltBit;

	private int nIceBitPetID = -1;

	private long nIceBitPetTime;

	private int nBoltBitPetID = -1;

	private long nBoltBitPetTime;

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_Pandora_TeleportIn_000";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[12]
		{
			"ch032_skill_01_stand_start", "ch032_skill_01_stand_loop", "ch032_skill_01_stand_end", "ch032_skill_01_jump_start", "ch032_skill_01_jump_loop", "ch032_skill_01_jump_end", "ch032_skill_02_stand_start", "ch032_skill_02_stand_loop", "ch032_skill_02_stand_end", "ch032_skill_02_jump_start",
			"ch032_skill_02_jump_loop", "ch032_skill_02_jump_end"
		};
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[4];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill00ShootPoint", true);
		_refEntity.ExtraTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill01ShootPoint", true);
		OrangeBattleUtility.FindChildRecursive(ref target, "WeaponMesh_m", true);
		CharacterMaterial[] components = _refEntity.ModelTransform.GetComponents<CharacterMaterial>();
		if (components.Length >= 2 && components[1] != null)
		{
			cmRodMesh = components[1];
		}
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[3];
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_icedoll_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_finale_000", 2);
		InitPetMode();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	private void InitPetMode()
	{
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[_refEntity.CharacterData.n_PASSIVE_3];
		if (sKILL_TABLE.n_EFFECT == 16)
		{
			nIceBitPetID = (int)sKILL_TABLE.f_EFFECT_X;
			nIceBitPetTime = (long)(sKILL_TABLE.f_EFFECT_Z * 1000f);
			CreatePet(0, nIceBitPetID, 0);
		}
		sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[_refEntity.CharacterData.n_PASSIVE_4];
		if (sKILL_TABLE.n_EFFECT == 16)
		{
			nBoltBitPetID = (int)sKILL_TABLE.f_EFFECT_X;
			nBoltBitPetTime = (long)(sKILL_TABLE.f_EFFECT_Z * 1000f);
			CreatePet(1, nBoltBitPetID, 1);
		}
	}

	protected void CreatePet(int petIndex, int petID, int skillIndex)
	{
		PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
		petBuilder.PetID = petID;
		petBuilder.follow_skill_id = skillIndex;
		petBuilder.CreatePet(delegate(SCH009Controller obj)
		{
			if (petIndex == 0)
			{
				mIceBit = obj;
				mIceBit.Set_follow_Player(_refEntity);
				mIceBit.SetFollowEnabled(false);
			}
			else
			{
				mBoltBit = obj;
				mBoltBit.Set_follow_Player(_refEntity);
				mBoltBit.SetFollowEnabled(false);
			}
		});
	}

	public override void ClearSkill()
	{
		if (cmRodMesh != null)
		{
			cmRodMesh.Disappear(null, 0f);
		}
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if ((uint)currentActiveSkill <= 1u)
		{
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.GetCurrentSkillObj());
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
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.CurrentFrame > 0.25f)
			{
				if (bInSkill)
				{
					bInSkill = false;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
					CreateSkillBullet(_refEntity.GetCurrentSkillObj());
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity._transform);
					_refEntity.PlaySE(_refEntity.VoiceID, "v_ch032_skill01");
				}
				else if (CheckCancelAnimate(0))
				{
					SkillEndChnageToIdle();
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (CheckCancelAnimate(0))
			{
				SkillEndChnageToIdle();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.CurrentFrame > 0.25f)
			{
				if (bInSkill)
				{
					bInSkill = false;
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
					CreateSkillBullet(_refEntity.GetCurrentSkillObj());
					_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity._transform);
					_refEntity.PlaySE(_refEntity.VoiceID, "v_ch032_skill02");
				}
				else if (CheckCancelAnimate(1))
				{
					SkillEndChnageToIdle();
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (CheckCancelAnimate(1))
			{
				SkillEndChnageToIdle();
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.StopShootTimer();
			_refEntity.DisableCurrentWeapon();
			if (_refEntity is OrangeConsoleCharacter)
			{
				(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL0, false);
			}
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_icedoll_000", _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 1 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.StopShootTimer();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_finale_000", _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				if (cmRodMesh != null)
				{
					cmRodMesh.Appear(null, 0f);
				}
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				if (cmRodMesh != null)
				{
					cmRodMesh.Appear(null, 0f);
				}
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetSpeed(0, 0);
				if (cmRodMesh != null)
				{
					cmRodMesh.Appear(null, 0f);
				}
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetSpeed(0, 0);
				if (cmRodMesh != null)
				{
					cmRodMesh.Appear(null, 0f);
				}
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
				}
				break;
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				if (cmRodMesh != null)
				{
					cmRodMesh.Disappear(null, 0f);
				}
				_refEntity.EnableCurrentWeapon();
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SkillEndChnageToIdle();
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
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV, Vector3.right * Mathf.Sign(_refEntity.ShootDirection.x));
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[3], wsSkill.SkillLV);
				break;
			}
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		if (petID == nIceBitPetID)
		{
			SCH009Controller sCH009Controller = mIceBit;
			if ((bool)sCH009Controller)
			{
				sCH009Controller.SetParam(nIceBitPetTime, 7f);
				sCH009Controller.SetFollowEnabled(false);
				sCH009Controller.SetPositionAndRotation(_refEntity.ExtraTransforms[3].position, false);
				sCH009Controller.SetActive(true);
				if (_refEntity.IsLocalPlayer)
				{
					StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + 0, true);
				}
			}
			_refEntity.PlaySE("SkillSE_CH032_000", "ch032_cannon");
		}
		else
		{
			if (petID != nBoltBitPetID)
			{
				return;
			}
			SCH009Controller sCH009Controller2 = mBoltBit;
			if ((bool)sCH009Controller2)
			{
				sCH009Controller2.SetParam(nIceBitPetTime, 7f);
				sCH009Controller2.SetFollowEnabled(false);
				sCH009Controller2.SetPositionAndRotation(_refEntity.ExtraTransforms[3].position, false);
				sCH009Controller2.SetActive(true);
				if (_refEntity.IsLocalPlayer)
				{
					StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + 0, true);
				}
			}
			_refEntity.PlaySE("SkillSE_CH032_000", "ch032_cannon");
		}
	}

	public override void ControlCharacterDead()
	{
		if (cmRodMesh != null)
		{
			cmRodMesh.Disappear(null, 0f);
		}
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		_refEntity.PlaySE(_refEntity.SkillSEID, "ch032_chara01");
	}

	public void StageTeleportOutCharacterDepend()
	{
		if (cmRodMesh != null)
		{
			cmRodMesh.Disappear();
		}
	}

	public bool CheckPetActive(int petId)
	{
		if (mIceBit != null && mIceBit.Activate && mIceBit.PetID == petId)
		{
			return true;
		}
		if (mBoltBit != null && mBoltBit.Activate && mBoltBit.PetID == petId)
		{
			return true;
		}
		return false;
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void SkillEndChnageToIdle()
	{
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		if (cmRodMesh != null)
		{
			cmRodMesh.Disappear(null, 0f);
		}
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
}
