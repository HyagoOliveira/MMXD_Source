using System;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class CH053_Controller : CharacterControlBase
{
	private bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected Vector3 vLockShootDirection = Vector3.zero;

	protected int nPetID = -1;

	protected long nPetTime;

	protected long nPetDebutTime;

	protected int nMineCount;

	protected List<SCH004Controller> _liPets = new List<SCH004Controller>();

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch053_skill_02_stand", "ch053_skill_02_jump", "ch053_skill_02_crouch" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch053_skill_01_stand_up", "ch053_skill_01_stand_mid", "ch053_skill_01_stand_down" };
		string[] array2 = new string[3] { "ch053_skill_01_jump_up", "ch053_skill_01_jump_mid", "ch053_skill_01_jump_down" };
		string[] array3 = new string[3] { "ch053_skill_01_crouch_up", "ch053_skill_01_crouch_mid", "ch053_skill_01_crouch_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		InitPetMode();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[4];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill0_ShootPoint", true);
		_refEntity.ExtraTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill1_ShootPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "WeaponMesh_m");
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<IceRingBullet>("prefab/bullet/p_ch053_000", "p_ch053_000", 8, null);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch053_000", 2);
	}

	private void InitPetMode()
	{
		bool flag = false;
		int petID = 0;
		int follow_skill_id = -1;
		nPetDebutTime = 850L;
		if (_refEntity.tRefPassiveskill.listUsePassiveskill.Count > 0)
		{
			for (int i = 0; i < _refEntity.tRefPassiveskill.listUsePassiveskill.Count; i++)
			{
				if (_refEntity.tRefPassiveskill.listUsePassiveskill[i].tSKILL_TABLE.n_EFFECT == 16)
				{
					flag = true;
					petID = (int)_refEntity.tRefPassiveskill.listUsePassiveskill[i].tSKILL_TABLE.f_EFFECT_X;
					nPetTime = (long)(_refEntity.tRefPassiveskill.listUsePassiveskill[i].tSKILL_TABLE.f_EFFECT_Z * 1000f);
					follow_skill_id = 1;
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
				obj.SetParams(pET_TABLE.s_MODEL, nPetTime, 0, null, nPetDebutTime);
			});
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.CheckPetActiveEvt = CheckPetActive;
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			CancelSkill0();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			CancelSkill1();
		}
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void CheckSkill()
	{
		if (!_refEntity.IsAnimateIDChanged())
		{
			UpdateSkill();
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
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill1(id);
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus != 0)
			{
				int num = 1;
			}
			else
			{
				DebutOrClearStageToggleWeapon(false);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_BTSKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)128u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)129u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
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
				ToggleWeapon(0);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SkillEndChnageToIdle(true);
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[2]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[3], wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ExtraTransforms[3]);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch053_000", _refEntity.ExtraTransforms[3].position, Quaternion.identity, Array.Empty<object>());
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
		}
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
		Vector3 vector = _refEntity.ExtraTransforms[3].position;
		if (vSetPos.HasValue)
		{
			vector = vSetPos ?? vector;
		}
		poolObj.SetPositionAndRotation(vector, false);
		if (nSetNumID == -1)
		{
			StageUpdate.SyncStageObj(4, 4, _refEntity.sNetSerialID + "," + petID + "," + nMineCount + "," + vector.x + "," + vector.y + "," + vector.z, true);
		}
		poolObj.activeSE = new string[2] { "SkillSE_Leviathan", "lv_crystal01" };
		poolObj.boomSE = new string[2] { "SkillSE_Leviathan", "lv_crystal02" };
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

	public void StageTeleportOutCharacterDepend()
	{
		_tfWeaponMesh.enabled = false;
	}

	public void TeleportInCharacterDepend()
	{
		DebutOrClearStageToggleWeapon(true);
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("lv_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch053_startin_000";
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

	private void UpdateSkill()
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.ShootDirection = vLockShootDirection;
				_refEntity.Animator.SetAttackLayerActive(vLockShootDirection);
				if (_refEntity.CurrentFrame > 1f)
				{
					SkillEndChnageToIdle();
				}
				else if (bInSkill && _refEntity.CurrentFrame > 0.23f)
				{
					bInSkill = false;
					CreateSkillBullet(_refEntity.PlayerSkills[0]);
				}
				else if (!bInSkill && CheckCancelAnimate(0))
				{
					SkipSkill0Animation();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (_refEntity.CurrentFrame > 1f)
				{
					SkillEndChnageToIdle();
				}
				else if (bInSkill && _refEntity.CurrentFrame > 0.1f)
				{
					bInSkill = false;
					CreateSkillBullet(_refEntity.PlayerSkills[1]);
				}
				else if (!bInSkill && _refEntity.CurrentFrame > 0.65f && CheckCancelAnimate(1))
				{
					SkipSkill1Animation();
				}
				break;
			}
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.WIN_POSE && _refEntity.CurrentFrame > 0.1f)
			{
				DebutOrClearStageToggleWeapon(false);
			}
			break;
		}
		}
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		vLockShootDirection = _refEntity.ShootDirection;
		ToggleWeapon(1);
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
		}
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1)
		{
			SkillEndChnageToIdle();
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private void UseSkill1(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
		}
		PlayVoiceSE("v_lv_skill01_1");
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_1)
		{
			SkillEndChnageToIdle();
		}
		else if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		bInSkill = false;
		ToggleWeapon(0);
		if (isCrouch)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void DebutOrClearStageToggleWeapon(bool bDebut)
	{
		if (!_tfWeaponMesh.enabled)
		{
			ToggleWeapon(-1);
		}
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -1:
			if (!_tfWeaponMesh.enabled)
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				_tfWeaponMesh.enabled = true;
			}
			break;
		case 1:
		case 2:
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			_tfWeaponMesh.enabled = true;
			break;
		default:
			_refEntity.EnableCurrentWeapon();
			_tfWeaponMesh.enabled = false;
			break;
		}
	}
}
