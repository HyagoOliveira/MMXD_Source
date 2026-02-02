using System;
using UnityEngine;
using enums;

public class CH066_Controller : CharacterControlBase
{
	private readonly float CH066_MIN_WEAPON_HEIGHT = 3f;

	protected bool bInSkill;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected int _nSkill1MaxRand;

	protected bool bShootBullet;

	protected readonly float fJumpSpeed = 6.0242186f;

	protected readonly float fExtendClimbingUp = 0.25f;

	protected int _nJumpSpeed;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch066_skill_01_stand", "ch066_skill_01_jump", "ch066_skill_01_crouch", "ch066_skill_02_stand", "ch066_skill_02_jump", "ch066_skill_02_crouch" };
	}

	public override int JumpSpeed()
	{
		return _nJumpSpeed;
	}

	public override void Start()
	{
		base.Start();
		InitExtraMeshData();
		_nJumpSpeed = Mathf.RoundToInt(fJumpSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);
		_refEntity.Controller._extendClimbingUp = fExtendClimbingUp;
	}

	private void InitExtraMeshData()
	{
		_refEntity.AimExtendPosition = new Vector3(0f, 0.42f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "CurryMesh_c");
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = false;
		for (int i = 0; i < _refEntity.PlayerSkills[1].FastBulletDatas.Length; i++)
		{
			if (_nSkill1MaxRand < _refEntity.PlayerSkills[1].FastBulletDatas[i].n_TRIGGER_RATE)
			{
				_nSkill1MaxRand = _refEntity.PlayerSkills[1].FastBulletDatas[i].n_TRIGGER_RATE;
			}
		}
		WeaponStruct[] playerWeapons = _refEntity.PlayerWeapons;
		foreach (WeaponStruct weaponStruct in playerWeapons)
		{
			if (weaponStruct == null)
			{
				continue;
			}
			CharacterMaterial[] weaponMesh = weaponStruct.WeaponMesh;
			foreach (CharacterMaterial characterMaterial in weaponMesh)
			{
				if (characterMaterial != null && characterMaterial.GetDissolveModelHeight() < CH066_MIN_WEAPON_HEIGHT)
				{
					characterMaterial.ChangeDissolveModelModelHeight(CH066_MIN_WEAPON_HEIGHT);
				}
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch066_skill2_000", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.PlayerHeldSkillCB = PlayerHeldSkill;
		_refEntity.PlayerHeldShootCB = PlayerHeldShoot;
		_refEntity.PlayerReleaseShootCB = PlayerReleaseShoot;
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
				DebutOrClearStageToggleWeapon(false);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				DebutOrClearStageToggleWeapon(false);
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
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
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
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
			_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ModelTransform);
			OrangeBattleUtility.UpdateSkillCD(wsSkill);
			PlayVoiceSE("v_kb_skill01");
			PlaySkillSE("kb_curry");
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
		case OrangeCharacter.SubStatus.SKILL1_2:
		{
			for (int i = 0; i < wsSkill.FastBulletDatas.Length; i++)
			{
				if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-(i + 1)))
				{
					PlayVoiceSE("v_kb_skill0" + (i + 2));
					break;
				}
			}
			if (!_refEntity.IsLocalPlayer)
			{
				break;
			}
			for (int j = 0; j < wsSkill.FastBulletDatas.Length; j++)
			{
				if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-(j + 1)))
				{
					_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[j], wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV, (_refEntity._characterDirection == CharacterDirection.LEFT) ? Vector3.left : Vector3.right, false, 1);
					_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ModelTransform, null, j);
					OrangeBattleUtility.UpdateSkillCD(wsSkill);
					_refEntity.selfBuffManager.RemoveBuffByBuffID(-(j + 1));
					break;
				}
				_refEntity.selfBuffManager.RemoveBuffByBuffID(-(j + 1));
			}
			break;
		}
		}
	}

	protected void PlayerHeldSkill(int id)
	{
	}

	protected void PlayerHeldShoot()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.HURT || curMainStatus == OrangeCharacter.MainStatus.SKILL || _refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		WeaponStruct currentWeaponObj = _refEntity.GetCurrentWeaponObj();
		switch ((WeaponType)(short)_refEntity.GetCurrentWeaponObj().WeaponData.n_TYPE)
		{
		case WeaponType.Buster:
		case WeaponType.Spray:
		case WeaponType.SprayHeavy:
		case WeaponType.DualGun:
		case WeaponType.MGun:
		case WeaponType.Gatling:
		case WeaponType.Launcher:
			if (currentWeaponObj.LastUseTimer.GetMillisecond() >= currentWeaponObj.BulletData.n_FIRE_SPEED && !(currentWeaponObj.MagazineRemain <= 0f))
			{
				if (_refEntity.IsShoot == 0)
				{
					_refEntity.IsShoot = 1;
					break;
				}
				bShootBullet = true;
				_refEntity.PlayerHeldShoot();
			}
			break;
		default:
			_refEntity.PlayerHeldShoot();
			break;
		}
	}

	protected void PlayerReleaseShoot()
	{
		if (!bShootBullet && _refEntity.IsShoot > 0)
		{
			_refEntity.IsShoot = 0;
		}
		bShootBullet = false;
		_refEntity.PlayerReleaseShoot();
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
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.3f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (!bInSkill && CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.7f)
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
			else if (bInSkill && _refEntity.CurrentFrame > 0.2f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (!bInSkill && CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.7f)
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
			}
		}
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
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
		ToggleWeapon(2);
		TurnToAimTarget();
		if (_refEntity.IsLocalPlayer)
		{
			int num = OrangeBattleUtility.Random(0, _nSkill1MaxRand);
			for (int i = 0; i < _refEntity.PlayerSkills[1].FastBulletDatas.Length; i++)
			{
				if (num < _refEntity.PlayerSkills[1].FastBulletDatas[i].n_TRIGGER_RATE)
				{
					_refEntity.selfBuffManager.AddBuff(-(i + 1), 0, 0, 0, false, _refEntity.sNetSerialID);
					break;
				}
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch066_skill2_000", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
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
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
		for (int i = 0; i < _refEntity.PlayerSkills[1].FastBulletDatas.Length; i++)
		{
			_refEntity.selfBuffManager.RemoveBuffByBuffID(-(i + 1));
		}
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
		ToggleWeapon(-1);
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
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
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = false;
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
}
