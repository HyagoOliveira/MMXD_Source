using System;
using System.Linq;
using UnityEngine;

public class CH021_Controller : CharacterControllerProxyBaseGen2
{
	private enum SkillAnimationId : uint
	{
		ANI_SKILL1_STAND_START = 65u,
		ANI_SKILL1_STAND_LOOP = 66u,
		ANI_SKILL1_STAND_END = 67u
	}

	private enum FxName
	{
		fxuse_boosterkick_000 = 0
	}

	private readonly float SKILL_SPEED_X = 3f;

	private readonly float SKILL_SPEED_Y = 3f;

	private readonly int FRAME_SKILL_1_LOOP = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private int _skillEndFrame;

	private ChargeShootObj _chargeShootObj;

	private string[] _extraMeshes = new string[1] { "BusterMesh_m" };

	private void ToggleExtraMesh(bool isOpen)
	{
		_refEntity.ToggleExtraMesh(isOpen);
	}

	private void ShootChargeBuster(int skillID)
	{
		_chargeShootObj.ShootChargeBuster(skillID);
		if (_refEntity.PlayerSkills[skillID].ChargeLevel > 0)
		{
			_refEntity.Animator.SetAnimatorEquip(1);
			_refEntity.CurrentActiveSkill = skillID;
			ToggleWeapon(WeaponState.SKILL_0);
		}
	}

	public override void Start()
	{
		base.Start();
		Transform[] childs = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2]
		{
			OrangeBattleUtility.FindChildRecursive(ref childs, "L WeaponPoint", true),
			OrangeBattleUtility.FindChildRecursive(ref childs, "R WeaponPoint", true)
		};
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		OrangeCharacter refEntity = _refEntity;
		Renderer[] extraMeshOpen = (from meshName in _extraMeshes
			select OrangeBattleUtility.FindChildRecursive(ref childs, meshName) into transform
			select transform.GetComponent<SkinnedMeshRenderer>()).ToArray();
		refEntity.ExtraMeshOpen = extraMeshOpen;
		_refEntity.ExtraMeshClose = new Renderer[0];
		Enum.GetNames(typeof(FxName)).ForEach(delegate(string fxName)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);
		});
		_chargeShootObj = _refEntity.GetComponent<ChargeShootObj>();
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch021_skill_02_stand_start", "ch021_skill_02_loop", "ch021_skill_02_end" };
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[6] { "login", "logout", "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[6] { "ch021_login", "ch021_logout", "ch021_skill_01_stand", "ch021_skill_01_fall", "ch021_skill_01_wallgrab", "ch021_skill_01_crouch" };
	}

	protected override void TeleportInCharacterDepend()
	{
		ToggleWeapon(WeaponState.TELEPORT_IN);
	}

	protected override void ToggleWeapon(WeaponState weaponState)
	{
		switch (weaponState)
		{
		case WeaponState.TELEPORT_IN:
		case WeaponState.TELEPORT_OUT:
		case WeaponState.SKILL_1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
			ToggleExtraMesh(false);
			break;
		case WeaponState.SKILL_0:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableCurrentWeapon();
			}
			ToggleExtraMesh(true);
			_refEntity.EnableHandMesh(false);
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			ToggleExtraMesh(false);
			break;
		}
	}

	public override void ControlCharacterDead()
	{
	}

	protected override void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			ToggleWeapon(WeaponState.NORMAL);
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				SetSkillEnd();
				break;
			}
			break;
		}
	}

	protected override void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if ((uint)subStatus <= 1u)
			{
				ToggleWeapon(WeaponState.TELEPORT_OUT);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
			{
				Vector2 vector = new Vector2((float)_refEntity.direction * SKILL_SPEED_X, SKILL_SPEED_Y);
				VInt2 vInt = new VInt2(vector / FRAME_SKILL_1_LOOP / GameLogicUpdateManager.m_fFrameLen);
				_refEntity.SetSpeed(vInt.x, vInt.y);
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[currentActiveSkill];
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FxName.fxuse_boosterkick_000.ToString(), _refEntity.transform, Quaternion.Euler(0f, 90 * _refEntity.direction, 0f), Array.Empty<object>());
				_refEntity.SetAnimateId(_refEntity.AnimateID + 1);
				_refEntity.BulletCollider.UpdateBulletData(weaponStruct.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_skillEndFrame = gameFrame + FRAME_SKILL_1_LOOP;
				break;
			}
			}
			break;
		}
	}

	protected override void OnPlayerPressSkillCharacterCall(SkillID skillID)
	{
		PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
		switch (skillID)
		{
		case SkillID.SKILL_0:
		{
			if (!CheckCanTriggerSkill(skillID))
			{
				break;
			}
			bool isInGround = _refEntity.IsInGround;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
			if (_refEntity.PlayerSetting.AutoCharge == 1)
			{
				if (!weaponStruct.ChargeTimer.IsStarted())
				{
					weaponStruct.ChargeTimer.TimerStart();
					_chargeShootObj.StartCharge();
				}
				else
				{
					ShootChargeBuster((int)skillID);
				}
			}
			else
			{
				PlayVoiceSE("v_fo_skill01");
				_refEntity.Animator.SetAnimatorEquip(1);
				ToggleWeapon(WeaponState.SKILL_0);
				_refEntity.PlayerShootBuster(weaponStruct, true, (int)skillID, 0);
			}
			break;
		}
		case SkillID.SKILL_1:
			if (CheckCanTriggerSkill(skillID) && _refEntity.IsInGround)
			{
				PlayVoiceSE("v_fo_skill02");
				PlayerStopDashing();
				SetSkillAndWeapon(skillID);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				IAimTarget autoAimTarget = playerAutoAimSystem.AutoAimTarget;
				if (autoAimTarget != null)
				{
					int num = Math.Sign((autoAimTarget.AimPosition - _refEntity.transform.position).x);
					_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
				}
			}
			break;
		}
	}

	protected override void OnPlayerReleaseSkillCharacterCall(SkillID skillID)
	{
		if (skillID == SkillID.SKILL_0 && _refEntity.PlayerSetting.AutoCharge != 1 && _refEntity.CheckUseSkillKeyTrigger((int)skillID))
		{
			ShootChargeBuster((int)skillID);
		}
	}

	protected override void OnCheckSkill(int nowFrame)
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.CheckSkillEndByShootTimer();
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.SKILL1_1 && nowFrame >= _skillEndFrame)
			{
				SetSkillEnd();
			}
		}
	}

	public override void ClearSkill()
	{
		switch ((SkillID)_refEntity.CurrentActiveSkill)
		{
		case SkillID.SKILL_0:
			_refEntity.CancelBusterChargeAtk();
			SetSkillEnd();
			break;
		case SkillID.SKILL_1:
			SetSkillEnd();
			break;
		}
	}

	public override int GetUniqueWeaponType()
	{
		return 1;
	}
}
