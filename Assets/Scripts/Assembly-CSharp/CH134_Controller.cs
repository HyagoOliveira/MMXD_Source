using System;
using UnityEngine;

public class CH134_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private CH134_Gospel_Controller gospelController;

	private bool toggleTeleportOutFlg;

	private readonly int SKL_TRIGGER = (int)(0.19f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_END = (int)(0.625f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL_END_BREAK = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_000 = "fxuse_GospelDash_000";

	private readonly string FX_100 = "fxuse_GospelAttack_000";

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		_refEntity.PlayTeleportInVoice = false;
	}

	private void InitializeSkill()
	{
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_000, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_100, 2);
		gospelController = GetComponentInChildren<CH134_Gospel_Controller>();
		gospelController.SetEntity(_refEntity);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.OverrideAnimatorParamtersEvt = PlayGospelAnim;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
		_refEntity.LeaveRideArmorEvt = LeaveRideArmor;
		_refEntity.LockAnimatorEvt = LockAnimator;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.DeadAreaLockEvt = DeadAreaEventLock;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && gospelController.CanUseSkill && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, _refEntity.ModelTransform);
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL_END_BREAK;
			_refEntity.IsShoot = 1;
			ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL_TRIGGER, SKL_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			gospelController.UseSkill02();
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_100, _refEntity.ExtraTransforms[0].position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			PlayVoiceSE("v_fo2_skill02");
			PlaySkillSE("fo2_gosattack01");
			_refEntity.IsShoot = 0;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && gospelController.CanUseSkill && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, _refEntity.ModelTransform);
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL_END_BREAK;
			_refEntity.IsShoot = 1;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL_TRIGGER, SKL_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, (HumanBase.AnimateId)128u, (HumanBase.AnimateId)129u);
			gospelController.UseSkill01();
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_000, weaponStruct.ShootTransform[0].position, Quaternion.identity, Array.Empty<object>());
			PlayVoiceSE("v_fo2_skill01");
			PlaySkillSE("fo2_gosrush");
		}
	}

	public void TeleportOutCharacterDepend()
	{
		if (!toggleTeleportOutFlg)
		{
			toggleTeleportOutFlg = true;
			gospelController.Logout();
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.GIGA_ATTACK)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.GIGA_ATTACK_START:
			if ((bool)gospelController)
			{
				gospelController.Disappear();
			}
			break;
		case OrangeCharacter.SubStatus.GIGA_ATTACK_END:
			if ((bool)gospelController)
			{
				gospelController.Appear();
			}
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		nowFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.PlayerSkills[0].ShootTransform[0], MagazineType.NORMAL, -1, 1, false);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.PlayerSkills[1].ShootTransform[0], MagazineType.NORMAL, -1, 0, false);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		}
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_SKILL_START && animateID != HumanBase.AnimateId.ANI_BTSKILL_START)
		{
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
		}
		else
		{
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
	}

	public void PlayGospelAnim()
	{
		gospelController.TryPlayAnimation();
	}

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		_refEntity.EnableCurrentWeapon();
		if (enable)
		{
			gospelController.Disappear();
		}
		else
		{
			gospelController.Appear();
		}
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		gospelController.Disappear();
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public void LeaveRideArmor(RideBaseObj targetRideArmor)
	{
		_refEntity.LeaveRideArmor(targetRideArmor);
		gospelController.Appear();
	}

	public override void ControlCharacterDead()
	{
		if ((bool)gospelController)
		{
			gospelController.Disappear();
		}
	}

	private void OnDisable()
	{
		if ((bool)gospelController)
		{
			gospelController.gameObject.SetActive(false);
		}
	}

	private void LockAnimator(bool bLock)
	{
		_refEntity.LockCurrentAnimator(bLock);
		if ((bool)gospelController)
		{
			gospelController.LockAnimator(bLock);
		}
	}

	private void DeadAreaEventLock(bool bLock)
	{
		if ((bool)gospelController)
		{
			if (bLock)
			{
				gospelController.Disappear();
			}
			else
			{
				gospelController.Appear();
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch134_skill_crouch_mid", "ch134_skill_stand_mid", "ch134_skill_jump_mid" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch134_skill_crouch_up", "ch134_skill_crouch_mid", "ch134_skill_crouch_down" };
		string[] array2 = new string[3] { "ch134_skill_stand_up", "ch134_skill_stand_mid", "ch134_skill_stand_down" };
		string[] array3 = new string[3] { "ch134_skill_jump_up", "ch134_skill_jump_mid", "ch134_skill_jump_down" };
		return new string[3][] { array, array2, array3 };
	}
}
