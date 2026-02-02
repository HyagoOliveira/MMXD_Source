using System;
using UnityEngine;

public class CH133_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private SKILL_TABLE linkSkl0;

	private SKILL_TABLE linkSkl1;

	private readonly int SKL0_TRIGGER = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_001 = "fxuse_RedBurst_000";

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		_refEntity.PlayTeleportInVoice = false;
	}

	private void InitializeSkill()
	{
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<BasicBullet>(_refEntity, 0, out linkSkl0);
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<ShingetsurinBullet>(_refEntity, 1, out linkSkl1);
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ModelTransform;
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_001, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
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
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, _refEntity.ModelTransform);
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_001, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				PlayVoiceSE("v_za_skill03");
				PlaySkillSE("za_red01");
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				PlayVoiceSE("v_za_skill01");
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
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
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.NORMAL, -1, 0);
				if (linkSkl0 != null)
				{
					WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
					_refEntity.PushBulletDetail(linkSkl0, weaponStruct2.weaponStatus, weaponStruct2.ShootTransform[0], weaponStruct2.SkillLV);
				}
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				PlaySkillSE("za_ring01");
				isSkillEventEnd = true;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				_refEntity.ShootDirection = ((_refEntity.direction == 1) ? Vector3.right : Vector3.left);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct.ShootTransform[0], MagazineType.ENERGY, -1, 0);
				if (linkSkl1 != null)
				{
					Vector3 value = ((_refEntity.direction == 1) ? Vector3.left : Vector3.right);
					_refEntity.PushBulletDetail(linkSkl1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0], weaponStruct.SkillLV, value);
				}
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
		if (animateID != HumanBase.AnimateId.ANI_SKILL_START && animateID != (HumanBase.AnimateId)68u)
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
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch133_skill_01_crouch", "ch133_skill_01_stand", "ch133_skill_01_jump", "ch133_skill_02_crouch", "ch133_skill_02_stand", "ch133_skill_02_jump" };
	}
}
