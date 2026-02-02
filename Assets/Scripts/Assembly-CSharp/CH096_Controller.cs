using System;
using System.Collections.Generic;
using UnityEngine;

public class CH096_Controller : CharacterControlBase, IPetSummoner
{
	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected CharacterMaterial _cmWeapon;

	protected PET_TABLE _tPetTable;

	protected List<SCH020Controller> _liPets = new List<SCH020Controller>();

	protected FxBase _fxUseSkill;

	protected Vector3 _vBeamShootDir = Vector3.right;

	protected OrangeTimer _otPetShootTime;

	protected int _nPetCount = 1;

	protected int _nPetShootTime = 100;

	public int PetID { get; set; } = -1;


	public long PetTime { get; set; }

	public int PetCount { get; set; }

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch096_skill_01_stand", "ch096_skill_02_stand" };
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
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "WeaponMesh_m", true);
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		CharacterMaterial[] components = _refEntity.ModelTransform.GetComponents<CharacterMaterial>();
		if (components.Length >= 2 && components[1] != null)
		{
			_cmWeapon = components[1];
		}
		_otPetShootTime = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_magicfield_004", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_magiccannon_000", 3);
	}

	private void InitLinkSkill()
	{
	}

	private void InitPet()
	{
		ManagedSingleton<CharacterControlHelper>.Instance.PetInit<SCH020Controller>(this, _refEntity, 0, _refEntity.PlayerSkills[1].BulletData);
		PET_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT.TryGetValue(PetID, out value))
		{
			_tPetTable = value;
		}
		string[] array = _refEntity.PlayerSkills[1].BulletData.s_CONTI.Split(',');
		float result = 0.1f;
		float.TryParse(array[0], out result);
		int.TryParse(array[1], out _nPetCount);
		_nPetShootTime = Mathf.RoundToInt(result * 1000f);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
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
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_rl_skill02");
			PlaySkillSE("rl_sphere01");
			UseSkill1(id);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_rl_skill03");
			PlaySkillSE("rl_beam");
			UseSkill0(id);
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
				ToggleWeapon(-2);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				ToggleWeapon(-3);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.HURT:
		{
			OrangeCharacter.SubStatus subStatus2 = subStatus - 4;
			int num = 1;
			break;
		}
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
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
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			Vector3 vector;
			if (!_refEntity.UseAutoAim)
			{
				vector = _vBeamShootDir;
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV, _vBeamShootDir);
			}
			else
			{
				vector = _refEntity.ShootDirection;
				if (_refEntity.IAimTargetLogicUpdate != null)
				{
					Vector3? vector2 = _refEntity.CalibrateAimDirection(_refEntity.ExtraTransforms[0].position, _refEntity.IAimTargetLogicUpdate);
					if (vector2.HasValue)
					{
						_refEntity._characterDirection = ((vector2.Value.x > 0f) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
						_refEntity.UpdateDirection();
						vector = vector2.Value;
					}
				}
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0], wsSkill.SkillLV);
			}
			Quaternion p_quaternion = Quaternion.Euler(new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector)));
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_magiccannon_000", _refEntity.ExtraTransforms[0].position, p_quaternion, Array.Empty<object>());
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.AimTransform);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			CreatePets(wsSkill);
			_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.AimTransform);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			break;
		}
	}

	protected void CreatePets(WeaponStruct wsSkill)
	{
		if (_refEntity.IsLocalPlayer)
		{
			float num = 360 / _nPetCount;
			for (int i = 0; i < _nPetCount; i++)
			{
				float y = (float)i * num;
				Vector3 value = _refEntity.AimPosition + Quaternion.Euler(0f, y, 0f) * Vector3.right;
				CallPet(PetID, false, -1, value);
			}
			_otPetShootTime.TimerStart();
		}
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
		bool followPlayer = _tPetTable.n_MODE == 1;
		SCH020Controller sCH020Controller = ManagedSingleton<CharacterControlHelper>.Instance.CallPet<SCH020Controller>(this, _refEntity, petID, nSetNumID, false, followPlayer, false, null, null, vSetPos);
		if (!sCH020Controller)
		{
			return;
		}
		Vector3 pos = vSetPos ?? _refEntity.AimPosition;
		sCH020Controller.activeSE = new string[2] { "SkillSE_ROLL", "rl_sphere02_lp" };
		sCH020Controller.unactiveSE = new string[2] { "SkillSE_ROLL", "rl_sphere02_stop" };
		sCH020Controller.SetSkillLv(_refEntity.PlayerSkills[1].SkillLV);
		sCH020Controller.SetParams(_tPetTable.s_MODEL, PetTime, _tPetTable.n_SKILL_0, _refEntity.PlayerSkills[1].weaponStatus, 166L);
		sCH020Controller.SetActive(true);
		sCH020Controller.SetPositionAndRotation(pos, false);
		sCH020Controller.SetFollowAngle(PetCount % _nPetCount * 360 / _nPetCount);
		sCH020Controller._cbShoot = DelShootCnt;
		sCH020Controller._cbCanShoot = CanShootPet;
		for (int num = _liPets.Count - 1; num >= 0; num--)
		{
			if (_liPets[num] == null || !_liPets[num].Activate)
			{
				_liPets.RemoveAt(num);
			}
		}
		_liPets.Add(sCH020Controller);
	}

	private void DelShootCnt()
	{
	}

	public bool CanShootPet()
	{
		if (_refEntity.IsLocalPlayer)
		{
			if (_otPetShootTime.GetMillisecond() > _nPetShootTime)
			{
				_otPetShootTime.TimerStart();
				return true;
			}
			return false;
		}
		return true;
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
	}

	public void StageTeleportOutCharacterDepend()
	{
		if (_cmWeapon != null)
		{
			_cmWeapon.Disappear();
		}
	}

	private void UpdateSkill()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.25f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.75f)
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill)
			{
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				bInSkill = false;
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.5f)
			{
				SkipSkill1Animation();
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
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		_vBeamShootDir = _refEntity.ShootDirection;
		_fxUseSkill = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_magicfield_004", _refEntity.AimPosition + Vector3.forward, Quaternion.identity, Array.Empty<object>());
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
		_fxUseSkill = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_magicfield_004", _refEntity.AimPosition + Vector3.forward, Quaternion.identity, Array.Empty<object>());
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
		if (!_refEntity.BulletCollider.bIsEnd)
		{
			_refEntity.BulletCollider.BackToPool();
			_refEntity.BulletCollider.HitCallback = null;
		}
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
			_tfWeaponMesh.enabled = true;
			if (_cmWeapon != null)
			{
				_cmWeapon.Appear();
			}
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			if (_cmWeapon != null)
			{
				_cmWeapon.Appear();
			}
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			if (_cmWeapon != null)
			{
				_cmWeapon.Appear();
			}
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			if (_cmWeapon != null)
			{
				_cmWeapon.Appear();
			}
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfWeaponMesh.enabled = false;
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
