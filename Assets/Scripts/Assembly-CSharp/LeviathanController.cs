using System;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class LeviathanController : CharacterControlBase
{
	private bool bInSkill;

	private Transform tfSpearMeshTransform;

	private Renderer tfSpearMesh;

	private int nLastSkill0Index;

	private FxBase fxCharge;

	private FxBase fxLogin;

	private int nPetID = -1;

	private long nPetTime;

	private long nPetDebutTime;

	private int nMineCount;

	protected List<SCH004Controller> _liPets = new List<SCH004Controller>();

	public override string[] GetCharacterDependAnimations()
	{
		return new string[14]
		{
			"ch020_skill_01_first_stand_start", "ch020_skill_01_first_stand_end", "ch020_skill_01_first_jump_start", "ch020_skill_01_first_jump_end", "ch020_skill_01_second_stand_start", "ch020_skill_01_second_stand_end", "ch020_skill_01_second_jump_start", "ch020_skill_01_second_jump_end", "ch020_skill_02_stand_start", "ch020_skill_02_stand_loop",
			"ch020_skill_02_stand_end", "ch020_skill_02_jump_start", "ch020_skill_02_jump_loop", "ch020_skill_02_jump_end"
		};
	}

	protected void OnEnable()
	{
		if ((bool)fxLogin)
		{
			fxLogin.BackToPool();
			fxLogin = null;
		}
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxlogin_leviathan_001";
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("lv_chara02");
		fxLogin = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[3];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "oceanspirit_shootpoint", true);
		tfSpearMeshTransform = OrangeBattleUtility.FindChildRecursive(ref target, "WeaponMesh_m", true);
		tfSpearMesh = tfSpearMeshTransform.GetComponent<Renderer>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_oceanspirit_skill1_start_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch020_skill1_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch020_skill1_000_1", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch020_skill1_001", 2);
		if (_refEntity.PlayerSkills[1].BulletData.n_LINK_SKILL != 0)
		{
			SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[_refEntity.PlayerSkills[1].BulletData.n_LINK_SKILL];
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<OceanSpiritBullet>("prefab/bullet/" + sKILL_TABLE.s_MODEL, sKILL_TABLE.s_MODEL, 10, null);
			if (sKILL_TABLE.n_LINK_SKILL != 0)
			{
				SKILL_TABLE sKILL_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[sKILL_TABLE.n_LINK_SKILL];
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<OceanSpiritBullet>("prefab/bullet/" + sKILL_TABLE2.s_MODEL, sKILL_TABLE2.s_MODEL, 2, null);
			}
		}
		InitPetMode();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	private void InitPetMode()
	{
		bool flag = false;
		int petID = 0;
		int follow_skill_id = 0;
		nPetDebutTime = 850L;
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
		else if (!flag && _refEntity.tRefPassiveskill.listHurtPassiveskill.Count > 0)
		{
			for (int i = 0; i < _refEntity.tRefPassiveskill.listHurtPassiveskill.Count; i++)
			{
				if (_refEntity.tRefPassiveskill.listHurtPassiveskill[i].tSKILL_TABLE.n_EFFECT == 16)
				{
					flag = true;
					petID = (int)_refEntity.tRefPassiveskill.listHurtPassiveskill[i].tSKILL_TABLE.f_EFFECT_X;
					nPetTime = (long)(_refEntity.tRefPassiveskill.listHurtPassiveskill[i].tSKILL_TABLE.f_EFFECT_Z * 1000f);
					follow_skill_id = -1;
					break;
				}
			}
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
				obj.SetParams(pET_TABLE.s_MODEL, 0L, 0, null, nPetDebutTime);
			});
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
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if (curSubStatus != OrangeCharacter.SubStatus.SKILL0_1 && curSubStatus != OrangeCharacter.SubStatus.SKILL0_3)
		{
			int num = 49;
		}
		else if (checkCancelAnimate(0))
		{
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			tfSpearMesh.enabled = false;
			_refEntity.EnableCurrentWeapon();
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
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[0].ComboCheckDatas[0].nComboSkillID);
			_refEntity.EnableCurrentWeapon();
			break;
		}
		tfSpearMesh.enabled = false;
		_refEntity.Dashing = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void PlayerPressSkillCharacterCall(int id)
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
				nLastSkill0Index = _refEntity.GetCurrentSkillObj().Reload_index;
				_refEntity.DisableCurrentWeapon();
				if (nLastSkill0Index == 0)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
					PlayVoiceSE("v_lv_skill01_1");
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
					PlayVoiceSE("v_lv_skill01_2");
				}
				PlaySkillSE("lv_slash");
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
				if (_refEntity is OrangeConsoleCharacter)
				{
					(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL1, false);
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				PlayVoiceSE("v_lv_skill02");
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus == OrangeCharacter.SubStatus.WIN_POSE)
			{
				tfSpearMesh.enabled = true;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				tfSpearMesh.enabled = true;
				_refEntity.SetSpeed(0, 0);
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[2]);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.Dashing = false;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch020_skill1_000", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch020_skill1_000_1", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
					_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch020_skill1_000", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
					_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (_refEntity.PreBelow || _refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				tfSpearMesh.enabled = true;
				_refEntity.SetSpeed(0, 0);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[2]);
				_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkill0Index].n_ID);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.Dashing = false;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch020_skill1_001", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch020_skill1_000_1", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
					_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch020_skill1_001", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
					_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				if (_refEntity.PreBelow || _refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				fxCharge = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_oceanspirit_skill1_start_000", _refEntity._transform.position, Quaternion.identity, Array.Empty<object>());
				PlaySkillSE("lv_ocean01");
				tfSpearMesh.enabled = true;
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				PlaySkillSE("lv_ocean02");
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)77u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				fxCharge.GetComponentInChildren<ParticleSystem>().Stop();
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)78u);
				}
				break;
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN && subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			tfSpearMesh.enabled = false;
			_refEntity.EnableCurrentWeapon();
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SkillEnd = true;
			tfSpearMesh.enabled = false;
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
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.SkillEnd = true;
			tfSpearMesh.enabled = false;
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
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SkillEnd = true;
			bInSkill = false;
			tfSpearMesh.enabled = false;
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
			fxCharge.BackToPool();
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
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.BulletCollider.UpdateBulletData(weaponStruct.FastBulletDatas[nLastSkill0Index], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[2], weaponStruct.SkillLV);
				break;
			}
		}
	}

	private bool checkCancelAnimate(int skilliD)
	{
		if (skilliD == 0 && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
		{
			return true;
		}
		return false;
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		if (petID != nPetID || (!_refEntity.IsLocalPlayer && nSetNumID == -1))
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
		poolObj.PetID = nPetID;
		poolObj.SetOwner(_refEntity, nMineCount);
		poolObj.SetParams(pET_TABLE.s_MODEL, nPetTime, 0, null, nPetDebutTime);
		poolObj.SetPositionAndRotation(_refEntity._transform.position + Vector3.up, false);
		if (nSetNumID == -1)
		{
			StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + nMineCount, true);
		}
		PlaySkillSE("lv_crystal01");
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

	public override void ControlCharacterDead()
	{
		if (fxCharge != null)
		{
			fxCharge.BackToPool();
		}
	}

	public void TeleportOutCharacterDepend()
	{
		if (!tfSpearMesh.enabled)
		{
			tfSpearMesh.enabled = true;
		}
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
