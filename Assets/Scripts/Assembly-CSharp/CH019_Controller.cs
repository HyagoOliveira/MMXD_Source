using System;
using UnityEngine;

public class CH019_Controller : CharacterControlBase
{
	private readonly int SKILL0_START = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_LOOP = (int)(0.25f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_END = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_START = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_LOOP = (int)(0.7f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_END = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_CANCEL = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private int skillEndFrame;

	private int skillCancelFrame;

	protected ObjInfoBar mEffect_Hide_obj;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	private void InitializeSkill()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch019_skill02_temp", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch019_skill01_temp", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch019_skill03_temp", 2);
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
				PlayVoiceSE("v_zb_skill01");
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerStopDashing();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SkillEnd = false;
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_zb_skill02");
				_refEntity.SetSpeed(0, 0);
				_refEntity.PlayerStopDashing();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SkillEnd = false;
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN && subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE && (bool)mEffect_Hide_obj)
		{
			mEffect_Hide_obj.gameObject.SetActive(true);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.IgnoreGravity = true;
			_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			skillEndFrame = gameFrame + SKILL0_START;
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
			Vector2 vector = new Vector2((int)_refEntity._characterDirection * OrangeCharacter.DashSpeed * 4, 0f);
			_refEntity.SetSpeed((int)vector.x, (int)vector.y);
			PlaySkillSE("zb_crimzon01");
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_ch019_skill01_temp", _refEntity.transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			_refEntity.Animator._animator.speed = 2f;
			_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[currentActiveSkill].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
			_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[currentActiveSkill].SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			skillEndFrame = gameFrame + SKILL0_LOOP;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (!_refEntity.Controller.Collisions.below && !_refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			_refEntity.IgnoreGravity = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.Animator._animator.speed = 1f;
			skillEndFrame = gameFrame + SKILL0_END;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			UpdateDirection();
			_refEntity.IgnoreGravity = true;
			_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			_refEntity.Animator._animator.speed = 1.4f;
			skillEndFrame = gameFrame + SKILL1_START;
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			PlaySkillSE("zb_kara01");
			_refEntity.CheckUsePassiveSkill(currentActiveSkill, _refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.PlayerSkills[currentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[currentActiveSkill]);
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_ch019_skill02_temp", _refEntity.transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_ch019_skill03_temp", _refEntity.transform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			_refEntity.Animator._animator.speed = 1f;
			_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[currentActiveSkill].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
			_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[currentActiveSkill].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[currentActiveSkill].SkillLV;
			_refEntity.BulletCollider.Active(_refEntity.TargetMask);
			skillEndFrame = gameFrame + SKILL1_LOOP;
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			skillEndFrame = gameFrame + SKILL1_END;
			skillCancelFrame = gameFrame + SKILL1_CANCEL;
			break;
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
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (gameFrame >= skillEndFrame)
			{
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
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
			if (gameFrame >= skillEndFrame)
			{
				_refEntity.BulletCollider.BackToPool();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (gameFrame >= skillEndFrame || (CheckCancelAnimate(_refEntity.CurrentActiveSkill) && gameFrame >= skillCancelFrame))
			{
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				ResetSkillStatus();
			}
			break;
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
				_refEntity.Animator._animator.speed = 1f;
				_refEntity.SetSpeed(0, 0);
				break;
			case 1:
				_refEntity.BulletCollider.BackToPool();
				_refEntity.Animator._animator.speed = 1f;
				_refEntity.SetSpeed(0, 0);
				break;
			}
		}
		ResetSkillStatus();
	}

	private bool CheckCancelAnimate(int skillId)
	{
		return ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID);
	}

	public override void ControlCharacterDead()
	{
		_refEntity.Animator._animator.speed = 1f;
	}

	private void ResetSkillStatus()
	{
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.SetSpeed(0, 0);
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInExtraEffect()
	{
		mEffect_Hide_obj = _refEntity.transform.GetComponentInChildren<ObjInfoBar>();
		if ((bool)mEffect_Hide_obj)
		{
			mEffect_Hide_obj.gameObject.SetActive(false);
		}
		PlaySkillSE("zb_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ch019_startin_000";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[4] { "ch019_skill_01_start", "ch019_skill_01_loop", "ch019_skill_01_end", "ch019_skill_02" };
	}
}
