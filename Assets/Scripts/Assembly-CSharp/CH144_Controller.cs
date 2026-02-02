using System;
using UnityEngine;

public class CH144_Controller : ZeroController
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	[SerializeField]
	private int risingSpdX = 4000;

	[SerializeField]
	private int risingSpdY = 8000;

	[SerializeField]
	private float risingTime = 0.5f;

	private void Awake()
	{
		ryuenjinEffect = "fxuse_SZflameblade_000";
		ryuenjinEffectEX = "fxuse_SZflameblade_000";
		fallingEffect = "fxuse_SZfallen_000";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch144_skill_02", "ch144_skill_01_end" };
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch144_skill_01_start", "ch144_skill_01_Loop" };
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
		_refEntity.LeaveRideArmorEvt = LeaveRideArmor;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.SkillEnd = false;
				_refEntity.SetSpeed(0, 0);
				fx_ryuennjin = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(ryuenjinEffect, _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				fx_ryuennjin_blade = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxController>(ryuenjinBladeEff, _refEntity.PlayerSkills[id].WeaponMesh[0].transform, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				_refEntity.CurrentActiveSkill = id;
				_refEntity.StopShootTimer();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				int p_endFrame = (int)(risingTime / GameLogicUpdateManager.m_fFrameLen);
				isSkillEventEnd = false;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, 1, p_endFrame, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				CreateColliderBullet();
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BLEND_SKILL_START, HumanBase.AnimateId.ANI_BLEND_SKILL_START, HumanBase.AnimateId.ANI_BLEND_SKILL_START, false);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 10);
				_refEntity.CurrentActiveSkill = id;
				bInkill = true;
				_refEntity.SkillEnd = false;
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.StopShootTimer();
				if (!_refEntity.Controller.Collisions.below && !_refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.GravityMultiplier = new VInt(0f);
				}
			}
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
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
				if ((bool)fx_ryuennjin)
				{
					fx_ryuennjin.transform.SetParentNull();
				}
				_refEntity.IgnoreGravity = false;
				_refEntity.BulletCollider.BackToPool();
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, 1, 1, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				_refEntity.IgnoreGravity = true;
				_refEntity.SetSpeed((int)_refEntity._characterDirection * risingSpdX, risingSpdY);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			break;
		case OrangeCharacter.SubStatus.TELEPORT_POSE:
			if ((double)_refEntity.CurrentFrame > 0.38 && bInkill)
			{
				bInkill = false;
				_refEntity.PlaySE(_refEntity.SkillSEID, 7);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fallingEffect, OrangeBattleUtility.FindChildRecursive(_refEntity._transform, "HandMesh_R"), (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				if (_refEntity.IsLocalPlayer)
				{
					CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				}
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			}
			break;
		}
	}

	private void CreateColliderBullet()
	{
		int reload_index = _refEntity.PlayerSkills[0].Reload_index;
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		_refEntity.FreshBullet = true;
		_refEntity.BulletCollider.UpdateBulletData(weaponStruct.FastBulletDatas[reload_index], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
		_refEntity.BulletCollider.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = weaponStruct.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0], _refEntity.direction * Vector2.right, reload_index);
		if (reload_index > 0)
		{
			_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_ID);
		}
	}

	public override void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE && !_refEntity.Controller.Collisions.below && !_refEntity.Controller.Collisions.JSB_below)
		{
			_refEntity.GravityMultiplier = new VInt(1f);
		}
		base.AnimationEndCharacterDepend(mainStatus, subStatus);
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.BulletCollider.BackToPool();
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
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

	public override void ControlCharacterDead()
	{
		_refEntity.GravityMultiplier = new VInt(1f);
	}

	public override void ClearSkill()
	{
		_refEntity.GravityMultiplier = new VInt(1f);
		base.ClearSkill();
	}

	public override void SetStun(bool enable)
	{
		_refEntity.GravityMultiplier = new VInt(1f);
		base.SetStun(enable);
	}

	private bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		_refEntity.GravityMultiplier = new VInt(1f);
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public void LeaveRideArmor(RideBaseObj targetRideArmor)
	{
		_refEntity.GravityMultiplier = new VInt(1f);
		_refEntity.LeaveRideArmor(targetRideArmor);
	}
}
