using System;
using System.Collections.Generic;
using UnityEngine;

public class CH103_Controller : CharacterControlBase, IPetSummoner
{
	protected bool bInSkill;

	protected int _nPetID01;

	protected long _nPetTime01;

	protected PET_TABLE _tPetTable01;

	protected int _nPetID02;

	protected long _nPetTime02;

	protected PET_TABLE _tPetTable02;

	protected List<SCH024Controller> _liPets = new List<SCH024Controller>();

	protected Vector3 _vPetDeactivePos;

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch103_skill_02_stand", "ch103_skill_02_jump", "ch103_skill_02_crouch" };
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
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_xmastree_001", 5);
	}

	private void InitLinkSkill()
	{
	}

	private void InitPet()
	{
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH024Controller>(this, _refEntity, 0, _refEntity.PlayerSkills[0].BulletData);
		if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(PetID, out _tPetTable01))
		{
			_nPetID01 = PetID;
			_nPetTime01 = PetTime;
		}
		for (int i = 0; i < _refEntity.tRefPassiveskill.listEquipPassiveskill.Count; i++)
		{
			if (_refEntity.tRefPassiveskill.listEquipPassiveskill[i].tSKILL_TABLE.n_EFFECT == 16)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH024Controller>(this, _refEntity, 0, _refEntity.tRefPassiveskill.listEquipPassiveskill[i].tSKILL_TABLE);
				if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(PetID, out _tPetTable02))
				{
					_nPetID02 = PetID;
					_nPetTime02 = PetTime;
				}
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
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
				if (_refEntity.IsLocalPlayer)
				{
					PlayVoiceSE("v_ri_skill03");
					PlaySkillSE("ri_sock01");
				}
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ri_skill04");
				UseSkill1(id);
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
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
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
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
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
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if ((uint)(curSubStatus - 49) <= 2u)
			{
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
			}
		}
	}

	protected void CreatePets(WeaponStruct wsSkill)
	{
		if (_refEntity.IsLocalPlayer)
		{
			CallPet(_nPetID01, false, -1, null);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		SCH024Controller sCH024Controller = null;
		if (petID == _nPetID01)
		{
			bool followPlayer = _tPetTable01.n_MODE == 1;
			Vector3 vector = vSetPos ?? _refEntity.ModelTransform.position;
			PetID = _nPetID01;
			PetTime = _nPetTime01;
			sCH024Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH024Controller>(this, _refEntity, petID, nSetNumID, true, followPlayer, false, null, null, vector);
			if ((bool)sCH024Controller)
			{
				sCH024Controller.transform.SetParentNull();
				sCH024Controller._cbDeactive = PetDeactiveCallBack;
				sCH024Controller.sActiveSE2 = new string[2] { "", "" };
				sCH024Controller.activeSE = new string[2] { "SkillSE_RICO", "ri_sock01_lp" };
				sCH024Controller.unactiveSE = new string[2] { "SkillSE_RICO", "ri_sock01_stop" };
				sCH024Controller.boomSE = new string[2] { "HitSE", "ht_guard04" };
				sCH024Controller.SetSkillLv(_refEntity.PlayerSkills[0].SkillLV);
				sCH024Controller.SetActive(true);
				sCH024Controller.SetPositionAndRotation(vector, false);
			}
		}
		else if (petID == _nPetID02)
		{
			bool followPlayer2 = _tPetTable02.n_MODE == 1;
			Vector3 vector2 = vSetPos ?? _vPetDeactivePos;
			PetID = _nPetID02;
			PetTime = _nPetTime02;
			sCH024Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH024Controller>(this, _refEntity, petID, nSetNumID, true, followPlayer2, false, null, null, vector2);
			if ((bool)sCH024Controller)
			{
				sCH024Controller.transform.SetParentNull();
				if (OrangeBattleUtility.IsInsideScreen(vector2))
				{
					sCH024Controller.sActiveSE2 = new string[2] { "SkillSE_RICO", "ri_sock02" };
					sCH024Controller.activeSE = new string[2] { "SkillSE_RICO", "ri_sock01_lp" };
					sCH024Controller.unactiveSE = new string[2] { "SkillSE_RICO", "ri_sock01_stop" };
				}
				else
				{
					sCH024Controller.sActiveSE2 = new string[2] { "", "" };
					sCH024Controller.activeSE = new string[2] { "", "" };
					sCH024Controller.unactiveSE = new string[2] { "", "" };
				}
				sCH024Controller.boomSE = new string[2] { "HitSE", "ht_guard04" };
				sCH024Controller.SetSkillLv(_refEntity.PlayerSkills[0].SkillLV);
				sCH024Controller.SetActive(true);
				sCH024Controller.SetPositionAndRotation(vector2, false);
			}
		}
		if (!sCH024Controller)
		{
			return;
		}
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(sCH024Controller);
	}

	public void PetDeactiveCallBack(Vector3 pos)
	{
		if (_refEntity.IsLocalPlayer)
		{
			_vPetDeactivePos = pos;
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
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
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) <= 2u)
		{
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.15f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.4f)
			{
				SkipSkill1Animation();
			}
		}
	}

	private void TurnToAimTarget()
	{
		Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
		if (vector.HasValue)
		{
			int num = Math.Sign(vector.Value.x);
			if (_refEntity.direction != num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void TurnToShootDirection(Vector3 dir)
	{
		int num = Math.Sign(dir.x);
		if (_refEntity.direction != num && Mathf.Abs(dir.x) > 0.05f)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			_refEntity.ShootDirection = dir;
		}
	}

	private void UseSkill0(int skillId)
	{
		CreatePets(_refEntity.PlayerSkills[0]);
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		SkillEndChnageToIdle();
	}

	private void UseSkill1(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		TurnToAimTarget();
		PlaySkillSE("ri_star01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_xmastree_001", _refEntity.transform.position, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
		}
		else if (_refEntity.Controller.Collisions.below)
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
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
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
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1_2)
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
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
		_refEntity.GravityMultiplier = new VInt(1f);
		_refEntity.Animator._animator.speed = 1f;
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

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -3:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			break;
		}
	}
}
