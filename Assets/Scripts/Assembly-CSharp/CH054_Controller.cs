using System;
using UnityEngine;

public class CH054_Controller : CharacterControlBase
{
	private readonly int SKILL0_START = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_END = (int)(0.75f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_CANCEL = (int)(0.55f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_START = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_END = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_CANCEL = (int)(0.45f / GameLogicUpdateManager.m_fFrameLen);

	private int skillEndFrame;

	private int skillCancelFrame;

	private Transform shootPointTransform;

	private Transform umbrellaMesh;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		InitializeExtraMesh();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "CustomShootPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_punishgame_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_cricket_000", 2);
	}

	private void InitializeExtraMesh()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		umbrellaMesh = OrangeBattleUtility.FindChildRecursive(ref target, "UmbrellaMesh_c");
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "WeaponMesh_c");
		Renderer[] extraMeshOpen;
		if (array != null)
		{
			OrangeCharacter refEntity = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[array.Length];
			refEntity.ExtraMeshOpen = extraMeshOpen;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshOpen[i] = array[i].GetComponent<SkinnedMeshRenderer>();
			}
		}
		else
		{
			OrangeCharacter refEntity2 = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[0];
			refEntity2.ExtraMeshOpen = extraMeshOpen;
		}
		OrangeCharacter refEntity3 = _refEntity;
		extraMeshOpen = new SkinnedMeshRenderer[0];
		refEntity3.ExtraMeshClose = extraMeshOpen;
		if (umbrellaMesh != null)
		{
			umbrellaMesh.gameObject.SetActive(false);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.ToggleExtraMesh(true);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlayerStopDashing();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SkillEnd = false;
			_refEntity.CurrentActiveSkill = id;
			_refEntity.ToggleExtraMesh(true);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if (mainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				UpdateDirection();
				GeneralSetSkillAnimation(HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_punishgame_000", _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				skillEndFrame = gameFrame + SKILL0_START;
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[currentActiveSkill].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[currentActiveSkill].SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				PlayVoiceSE("v_cm_skill03");
				PlaySkillSE("cm_batsu");
				skillEndFrame = gameFrame + SKILL0_END;
				skillCancelFrame = gameFrame + SKILL0_CANCEL;
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				UpdateDirection();
				GeneralSetSkillAnimation((HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				skillEndFrame = gameFrame + SKILL1_START;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_cricket_000", _refEntity.BulletCollider.transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				CreateSkillBullet(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
				PlayVoiceSE("v_cm_skill02");
				PlaySkillSE("cm_hane");
				skillEndFrame = gameFrame + SKILL1_END;
				skillCancelFrame = gameFrame + SKILL1_CANCEL;
				break;
			}
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		int gameFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (gameFrame >= skillEndFrame || (CheckCancelAnimate(_refEntity.CurrentActiveSkill) && gameFrame >= skillCancelFrame))
			{
				GeneralResetSkillAnimation(HumanBase.AnimateId.ANI_SKILL_START);
				_refEntity.BulletCollider.BackToPool();
				ResetSkillStatus();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (gameFrame >= skillEndFrame || (CheckCancelAnimate(_refEntity.CurrentActiveSkill) && gameFrame >= skillCancelFrame))
			{
				GeneralResetSkillAnimation((HumanBase.AnimateId)68u);
				ResetSkillStatus();
			}
			break;
		}
	}

	private void GeneralSetSkillAnimation(HumanBase.AnimateId crouch, HumanBase.AnimateId stand, HumanBase.AnimateId jump)
	{
		if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
		{
			_refEntity.SetAnimateId(crouch);
			return;
		}
		if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
		{
			_refEntity.SetAnimateId(stand);
			return;
		}
		_refEntity.IgnoreGravity = true;
		_refEntity.SetAnimateId(jump);
	}

	private void GeneralResetSkillAnimation(HumanBase.AnimateId currentCrouch)
	{
		if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
		{
			if (currentCrouch != 0 && _refEntity.AnimateID == currentCrouch)
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
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void UpdateDirection()
	{
		if (_refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
		{
			int num = Math.Sign((_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition - _refEntity._transform.position).x);
			_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			switch (_refEntity.CurrentActiveSkill)
			{
			case 0:
				_refEntity.BulletCollider.BackToPool();
				_refEntity.SetSpeed(0, 0);
				break;
			case 1:
				_refEntity.SetSpeed(0, 0);
				break;
			}
		}
		ResetSkillStatus();
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, shootPointTransform, weaponStruct.SkillLV);
	}

	private bool CheckCancelAnimate(int skillId)
	{
		return ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID);
	}

	public override void ControlCharacterDead()
	{
		if (umbrellaMesh != null)
		{
			umbrellaMesh.gameObject.SetActive(false);
		}
	}

	public void TeleportInCharacterDepend()
	{
		if (umbrellaMesh != null)
		{
			umbrellaMesh.gameObject.SetActive(true);
		}
		if (_refEntity.CurrentFrame >= 0.8f)
		{
			_refEntity.ToggleExtraMesh(false);
			if (umbrellaMesh != null)
			{
				umbrellaMesh.gameObject.SetActive(false);
			}
		}
	}

	private void ResetSkillStatus()
	{
		_refEntity.ToggleExtraMesh(false);
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("cm_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch054_startin_000";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch054_skill_01_crouch", "ch054_skill_01_stand", "ch054_skill_01_jump", "ch054_skill_02_crouch", "ch054_skill_02_stand", "ch054_skill_02_jump" };
	}
}
