using System;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class IrisController : CharacterControlBase
{
	private bool bInSkill;

	private int nPetID = -1;

	private long nPetTime;

	private Vector3 minePosition;

	private int nMineCount;

	protected List<SCH004Controller> _liPets = new List<SCH004Controller>();

	public override string[] GetCharacterDependAnimations()
	{
		return new string[4] { "ch024_skill_01_stand", "ch024_skill_01_jump", "ch024_skill_02_stand", "ch024_skill_02_jump" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_pray_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_mine_000", 2);
		InitPetMode();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
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
			nPetTime = (long)(_refEntity.PlayerSkills[0].BulletData.f_EFFECT_Z * 1000f);
			follow_skill_id = 0;
		}
		else if (_refEntity.PlayerSkills[1].BulletData.n_EFFECT == 16)
		{
			flag = true;
			petID = (int)_refEntity.PlayerSkills[1].BulletData.f_EFFECT_X;
			nPetTime = (long)(_refEntity.PlayerSkills[1].BulletData.f_EFFECT_Z * 1000f);
			follow_skill_id = 1;
		}
		if (flag)
		{
			nPetID = petID;
			PetBuilder petBuilder = new GameObject().AddComponent<PetBuilder>();
			petBuilder.PetID = petID;
			petBuilder.follow_skill_id = follow_skill_id;
			petBuilder.CreatePet(delegate(SCH004Controller obj)
			{
				obj.Set_follow_Player(_refEntity, false);
				obj.SetFollowEnabled(false);
				PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[nPetID];
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SCH004Controller>(obj, pET_TABLE.s_MODEL, 6);
				obj.activeSE = new string[4] { "SkillSE_IRIS", "ir_fuyu02", "", "0.6" };
				obj.SetParams(pET_TABLE.s_MODEL, 0L, 0, null, 0L);
			});
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.EnableCurrentWeapon();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			_refEntity.EnableCurrentWeapon();
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
			if (!(_refEntity.CurrentFrame > 0.5f))
			{
				break;
			}
			if (bInSkill)
			{
				bInSkill = false;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			}
			else if (checkCancelAnimate(0))
			{
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (_refEntity.CurrentFrame > 0.5f && bInSkill)
			{
				bInSkill = false;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ir_skill01");
				PlaySkillSE(1);
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				minePosition = _refEntity._transform.position;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_mine_000", minePosition, Quaternion.identity, Array.Empty<object>());
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ir_skill02");
				PlaySkillSE(3);
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
				_refEntity.DisableCurrentWeapon();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_pray_001", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
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
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.IgnoreGravity = true;
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			break;
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
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			if (_refEntity.PreBelow)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			if (_refEntity.PreBelow)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		SKILL_TABLE bulletDatum = weaponStruct.BulletData;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				CallPet(nPetID, false, -1, null);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity._transform, weaponStruct.SkillLV, null, false, 1);
				break;
			}
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID, Vector3? vSetPos = null)
	{
		if (petID != nPetID || (nSetNumID != -1 && nMineCount >= nSetNumID) || (!_refEntity.IsLocalPlayer && nSetNumID == -1))
		{
			return;
		}
		if (nSetNumID == -1)
		{
			nMineCount++;
			if (nMineCount < 0)
			{
				nMineCount = 0;
			}
		}
		else
		{
			nMineCount = nSetNumID;
		}
		PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[nPetID];
		SCH004Controller poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SCH004Controller>(pET_TABLE.s_MODEL);
		poolObj.activeSE = new string[4] { "SkillSE_IRIS", "ir_fuyu02", "", "0.6" };
		poolObj.PetID = nPetID;
		poolObj.SetOwner(_refEntity, nMineCount);
		poolObj.SetParams(pET_TABLE.s_MODEL, nPetTime, 0, null, 0L);
		if (isHurt)
		{
			poolObj.SetPositionAndRotation(_refEntity._transform.position + Vector3.up * 2.5f, false);
		}
		else
		{
			poolObj.SetPositionAndRotation(minePosition + Vector3.up * 2.5f, false);
		}
		if (nSetNumID == -1)
		{
			StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + nMineCount, true);
		}
		poolObj.SetActive(true);
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(poolObj);
	}

	private bool checkCancelAnimate(int skilliD)
	{
		if (skilliD == 0 && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
		{
			return true;
		}
		return false;
	}

	public bool CheckPetActive(int petId)
	{
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
			else if (_liPets[num].PetID == petId)
			{
				return true;
			}
		}
		return false;
	}
}
