using System;
using System.Collections.Generic;
using UnityEngine;

public class CH088_Controller : CharacterControlBase, IPetSummoner
{
	protected bool bInSkill;

	protected bool bInPullSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected SkinnedMeshRenderer _tfWeaponSubMesh;

	protected SKILL_TABLE _tSkill0_LinkSkill_A;

	protected SKILL_TABLE _tSkill0_LinkSkill_B;

	protected PET_TABLE _tPetTable;

	protected List<SCH016Controller> _liPets = new List<SCH016Controller>();

	private bool isBossPose;

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch088_skill_01_stand", "ch088_win" };
	}

	public override void Start()
	{
		base.Start();
		InitLinkSkill();
		InitPet();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "WhipMesh_Main_c", true);
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "WhipMesh_Sub_e", true);
		_tfWeaponSubMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponSubMesh.enabled = false;
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_whipstorm_000", 5);
	}

	private void InitLinkSkill()
	{
		if (_tSkill0_LinkSkill_A == null && _refEntity.PlayerSkills[0].BulletData.n_LINK_SKILL != 0)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out _tSkill0_LinkSkill_A))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref _tSkill0_LinkSkill_A);
				GameObject obj = new GameObject();
				CollideBullet go = obj.AddComponent<CollideBullet>();
				obj.name = _tSkill0_LinkSkill_A.s_MODEL;
				obj.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(go, _tSkill0_LinkSkill_A.s_MODEL);
			}
		}
		if (_tSkill0_LinkSkill_B == null && _tSkill0_LinkSkill_A.n_LINK_SKILL != 0)
		{
			WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(_tSkill0_LinkSkill_A.n_LINK_SKILL, out _tSkill0_LinkSkill_B))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref _tSkill0_LinkSkill_B);
				GameObject obj2 = new GameObject();
				CollideBullet go2 = obj2.AddComponent<CollideBullet>();
				obj2.name = _tSkill0_LinkSkill_B.s_MODEL;
				obj2.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(go2, _tSkill0_LinkSkill_B.s_MODEL);
			}
		}
	}

	private void InitPet()
	{
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH016Controller>(this, _refEntity);
		PET_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(PetID, out value))
		{
			_tPetTable = value;
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
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

	public override void RemovePet()
	{
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
			else
			{
				_liPets[num].BackToPool();
			}
		}
		_liPets.Clear();
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
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				if (_refEntity.IsLocalPlayer)
				{
					PlayVoiceSE("v_fm_skill03");
				}
				CreateTrap(_refEntity.PlayerSkills[1]);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				ToggleWeapon(-2);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				ToggleWeapon(-3);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.Animator._animator.speed = 1.2f;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.Animator._animator.speed = 1.2f;
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
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
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
				SkillEndChnageToIdle();
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
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_whipstorm_000", _refEntity.ModelTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_refEntity.BulletCollider.UpdateBulletData(wsSkill.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, (int)_refEntity._characterDirection);
				_refEntity.BulletCollider.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.Active(_refEntity.ModelTransform, _refEntity.ShootDirection, _refEntity.TargetMask);
				_refEntity.BulletCollider.BulletLevel = wsSkill.SkillLV;
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
				CallPet(PetID, false, -1, null);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[1]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
		}
	}

	protected void CreateTrap(WeaponStruct wsSkill)
	{
		CallPet(PetID, false, -1, null);
		_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[1]);
		OrangeBattleUtility.UpdateSkillCD(wsSkill);
	}

	protected void CreatePullBullet(WeaponStruct wsSkill)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 19) <= 1u)
			{
				_refEntity.PushBulletDetail(_tSkill0_LinkSkill_A, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
				_refEntity.PushBulletDetail(_tSkill0_LinkSkill_B, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
			}
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		bool followPlayer = _tPetTable.n_MODE == 1;
		SCH016Controller sCH016Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH016Controller>(this, _refEntity, petID, nSetNumID, false, followPlayer, false);
		if (!sCH016Controller)
		{
			return;
		}
		sCH016Controller.sActiveSE2 = new string[2] { "SkillSE_FERHAM", "fm_chaintrap01" };
		sCH016Controller.activeSE = new string[2] { "SkillSE_FERHAM", "fm_chaintrap01_lp" };
		sCH016Controller.unactiveSE = new string[2] { "SkillSE_FERHAM", "fm_chaintrap01_stop" };
		sCH016Controller.SetSkillLv(_refEntity.PlayerSkills[1].SkillLV);
		sCH016Controller.SetActive(true);
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(sCH016Controller);
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
	}

	public void TeleportOutCharacterDepend()
	{
		float currentFrame = _refEntity.CurrentFrame;
		switch (_refEntity.AnimateIDPrev)
		{
		case HumanBase.AnimateId.ANI_WIN_POSE:
			isBossPose = true;
			break;
		case HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE:
			if (isBossPose && currentFrame >= 0.6f)
			{
				isBossPose = false;
				PlaySE("BattleSE", "bt_kime01");
			}
			break;
		}
	}

	private void UpdateSkill()
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE && !_tfWeaponMesh.enabled && _refEntity.CurrentFrame > 0.55f)
			{
				ToggleWeapon(1);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (bInPullSkill)
				{
					CreatePullBullet(_refEntity.PlayerSkills[0]);
					bInPullSkill = false;
				}
				if (bInSkill && _refEntity.CurrentFrame > 0.13f)
				{
					bInSkill = false;
					CreateSkillBullet(_refEntity.PlayerSkills[0]);
				}
				else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.65f)
				{
					SkipSkill0Animation();
				}
				else if (_refEntity.CurrentFrame > 1f)
				{
					SkillEndChnageToIdle();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (bInSkill)
				{
					bInSkill = false;
					CreateSkillBullet(_refEntity.PlayerSkills[1]);
				}
				SkillEndChnageToIdle();
				break;
			}
			break;
		}
	}

	private void TurnToAimTarget()
	{
		Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
		if (vector.HasValue)
		{
			int num = Math.Sign(vector.Value.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void UseSkill0(int skillId)
	{
		PlayVoiceSE("v_fm_skill04");
		bInSkill = true;
		bInPullSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		TurnToAimTarget();
		if (_refEntity.Controller.Collisions.below)
		{
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
		if (!_refEntity.BulletCollider.bIsEnd)
		{
			_refEntity.BulletCollider.BackToPool();
		}
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		SkillEndChnageToIdle();
	}

	private void UseSkill1(int skillId)
	{
		PlayVoiceSE("v_fm_skill03");
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		TurnToAimTarget();
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
		}
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		SkillEndChnageToIdle();
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

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.GravityMultiplier = new VInt(1f);
		_refEntity.Animator._animator.speed = 1f;
		bInSkill = false;
		bInPullSkill = false;
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

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -3:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			_tfWeaponSubMesh.enabled = false;
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			_tfWeaponSubMesh.enabled = false;
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_tfWeaponSubMesh.enabled = true;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_tfWeaponSubMesh.enabled = true;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			_tfWeaponSubMesh.enabled = false;
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfWeaponMesh.enabled = false;
			_tfWeaponSubMesh.enabled = false;
			break;
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
