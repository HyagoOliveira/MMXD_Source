using System;
using UnityEngine;

public class CH099_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private Transform shootPointTransform0;

	private CharacterDirection skl1Direction = CharacterDirection.LEFT;

	private int offsetY;

	private readonly string sFxuse000_000 = "fxuse_spiningbirdkick_000";

	private readonly string sFxuse001_001 = "fxuse_hoyokusen_001";

	private readonly string sFxuse001_002 = "fxuse_hoyokusen_002";

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly int SKL0_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_END = (int)(0.156f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.08f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.43f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_TRIGGER = (int)(0.08f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_END = (int)(0.33f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_END_BREAK = (int)(0.16f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		shootPointTransform0 = new GameObject(sCustomShootPoint + "0").transform;
		shootPointTransform0.SetParent(base.transform);
		shootPointTransform0.localPosition = new Vector3(0f, 0.8f, 0f);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000_000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001_001);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001_002);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
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
				PlayVoiceSE("v_ch_skill01");
				PlaySkillSE("ch_spining_lg");
				_refEntity.SoundSource.PlaySE("SkillSE_CHUNLI", "ch_spining_stop", 0.8f);
				skl1Direction = _refEntity._characterDirection;
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IsShoot = 0;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_START_END, SKL0_START_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)68u);
				if (_refEntity.IgnoreGravity)
				{
					offsetY = -1;
				}
				else
				{
					offsetY = _refEntity.Controller.LogicPosition.y;
				}
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ch_skill02");
				PlaySkillSE("ch_hoyokusen");
				_refEntity.CurrentActiveSkill = id;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)71u, (HumanBase.AnimateId)71u, (HumanBase.AnimateId)71u);
				SetKickMovement(-0.2f);
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
				if (_refEntity.Dashing)
				{
					_refEntity.PlayerStopDashing();
				}
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				SKILL_TABLE bulletData = currentSkillObj.BulletData;
				_refEntity.BulletCollider.UpdateBulletData(bulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				_refEntity.BulletCollider.SetBulletAtk(currentSkillObj.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = currentSkillObj.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[_refEntity.CurrentActiveSkill]);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_LOOP_END, SKL0_LOOP_END, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)69u);
				_refEntity.SetSpeed((int)skl1Direction * (int)((float)OrangeCharacter.DashSpeed * 2.5f), 0);
				OrangeBattleUtility.UpdateSkillCD(currentSkillObj);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse000_000, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				_refEntity.IgnoreGravity = true;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.BulletCollider.BackToPool();
				int num = (_refEntity.IsInGround ? 1 : SKL0_END_END);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, num, num, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)70u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				endBreakFrame = GameLogicUpdateManager.GameFrame;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_END_BREAK, SKL0_END_BREAK, OrangeCharacter.SubStatus.SKILL0_3, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 0, SKL1_1_TRIGGER, SKL1_1_END, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)72u, (HumanBase.AnimateId)72u, (HumanBase.AnimateId)72u);
				SetKickMovement(0.2f);
				isSkillEventEnd = false;
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				SetKickMovement(0f);
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform0, MagazineType.ENERGY, -1, 0);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001_001, _refEntity.ModelTransform.position, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				SetKickMovement(0f);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001_002, _refEntity.ModelTransform.position, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		}
	}

	private void SetKickMovement(float rate)
	{
		_refEntity.SetHorizontalSpeed(Mathf.RoundToInt((float)(OrangeCharacter.WalkSpeed * (int)skl1Direction) * rate));
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		bool flag = _refEntity.CurrentActiveSkill == 0 && offsetY == _refEntity.Controller.LogicPosition.y;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID - 65 > HumanBase.AnimateId.ANI_WALK && animateID - 71 > HumanBase.AnimateId.ANI_RIDEARMOR)
		{
			if (flag)
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
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
	}

	public override void ClearSkill()
	{
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.BulletCollider.BackToPool();
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			_refEntity.EnableCurrentWeapon();
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) <= 3u)
		{
			_refEntity._characterDirection = skl1Direction;
		}
		else
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[8] { "ch099_skill_01_start", "ch099_skill_01_loop", "ch099_skill_01_end", "ch099_skill_01_start", "ch099_skill_01_loop", "ch099_skill_01_end", "ch099_skill_02_step1", "ch099_skill_02_step2" };
	}
}
