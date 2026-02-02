#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterControllerProxyBaseGen3 : CharacterControllerProxyBaseGen2
{
	protected class SkillStateDelegateData
	{
		public Action OnAnimationEnd;

		public Action OnStatusChanged;

		public Action OnLogicUpdate;
	}

	[SerializeField]
	private float _skillSpeed;

	private bool _isSkillDependDelegatorsInitialized;

	private Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData> _skillDependDelegators;

	protected Dictionary<int, Action<SkillID>> OnPlayerPressSkill0Events = new Dictionary<int, Action<SkillID>>();

	protected Dictionary<int, Action<SkillID>> OnPlayerPressSkill1Events = new Dictionary<int, Action<SkillID>>();

	protected Dictionary<int, Action<SkillID>> OnPlayerReleaseSkill0Events = new Dictionary<int, Action<SkillID>>();

	protected Dictionary<int, Action<SkillID>> OnPlayerReleaseSkill1Events = new Dictionary<int, Action<SkillID>>();

	protected int SkillEndFrame { get; private set; }

	protected int SkillCancelFrame { get; private set; }

	protected int NowFrame
	{
		get
		{
			return GameLogicUpdateManager.GameFrame;
		}
	}

	protected OrangeCharacter.MainStatus CurMainStatus
	{
		get
		{
			return _refEntity.CurMainStatus;
		}
	}

	protected OrangeCharacter.SubStatus CurSubStatus
	{
		get
		{
			return _refEntity.CurSubStatus;
		}
	}

	protected int CurActiveSkill
	{
		get
		{
			return _refEntity.CurrentActiveSkill;
		}
	}

	protected PlayerAutoAimSystem AimSystem
	{
		get
		{
			return _refEntity.PlayerAutoAimSystem;
		}
	}

	protected Transform[] ChildTransforms { get; private set; }

	protected int SkillFXDirection
	{
		get
		{
			if (AimSystem.GetClosestTarget() != null)
			{
				if (_refEntity.IsShootPrev > 0)
				{
					return Math.Sign(_refEntity.ShootDirection.x);
				}
				return Math.Sign(_refEntity.direction);
			}
			return Math.Sign(_refEntity.ShootDirection.x);
		}
	}

	protected float AnimatorSpeed
	{
		get
		{
			return _refEntity.Animator._animator.speed;
		}
		set
		{
			_refEntity.Animator._animator.speed = value;
		}
	}

	protected void InitializeSkillDependDelegators(Dictionary<OrangeCharacter.SubStatus, SkillStateDelegateData> skillDependDelegators)
	{
		if (!_isSkillDependDelegatorsInitialized && skillDependDelegators != null)
		{
			_skillDependDelegators = skillDependDelegators;
			_isSkillDependDelegatorsInitialized = true;
		}
	}

	private bool TryGetDelegator(out SkillStateDelegateData delegator)
	{
		if (!_isSkillDependDelegatorsInitialized)
		{
			delegator = null;
			return false;
		}
		if (!_skillDependDelegators.TryGetValue(CurSubStatus, out delegator))
		{
			delegator = null;
			return false;
		}
		return true;
	}

	protected int ConvertTimeToFrame(float time)
	{
		return (int)(time / GameLogicUpdateManager.m_fFrameLen);
	}

	protected void SetIgnoreGravity(bool checkIsInGround = true)
	{
		if (!checkIsInGround || !_refEntity.IsInGround)
		{
			_refEntity.IgnoreGravity = true;
		}
	}

	protected void EnableColliderBullet()
	{
		WeaponStruct skillData = _refEntity.PlayerSkills[CurActiveSkill];
		EnableColliderBullet(skillData);
	}

	protected void EnableColliderBullet(WeaponStruct skillData)
	{
		_refEntity.BulletCollider.UpdateBulletData(skillData.BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		_refEntity.BulletCollider.SetBulletAtk(skillData.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_refEntity.BulletCollider.BulletLevel = skillData.SkillLV;
		_refEntity.BulletCollider.Active(_refEntity.TargetMask);
	}

	protected void DisableColliderBullet()
	{
		_refEntity.BulletCollider.BackToPool();
	}

	protected void ToggleNormalWeapon(bool isEnable)
	{
		if (_refEntity.CheckCurrentWeaponIndex())
		{
			if (isEnable)
			{
				_refEntity.EnableCurrentWeapon();
			}
			else
			{
				_refEntity.DisableCurrentWeapon();
			}
		}
	}

	protected void SetSkillStatus(OrangeCharacter.SubStatus subStatus)
	{
		if (!subStatus.ToString().StartsWith("SKILL"))
		{
			Debug.LogError(string.Format("Invalid Skill Status : {0}", subStatus));
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, subStatus);
		}
	}

	protected void ShiftSkillStatus()
	{
		_refEntity.SetStatus(_refEntity.CurMainStatus, _refEntity.CurSubStatus + 1);
	}

	protected bool CheckFrameEnd(int targetFrame)
	{
		return NowFrame >= targetFrame;
	}

	protected bool CheckSkillFrameEnd()
	{
		return CheckFrameEnd(SkillEndFrame);
	}

	protected bool CheckSkillCancel()
	{
		return CheckFrameEnd(SkillCancelFrame);
	}

	protected bool CheckIsAnyHeld()
	{
		return ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID);
	}

	protected void AddSkillEndFrame(int frame = 1)
	{
		SkillEndFrame += frame;
	}

	protected void SetSkillFrame(float skillTime)
	{
		SetSkillFrame(ConvertTimeToFrame(skillTime));
	}

	protected void SetSkillFrame(int skillFrame)
	{
		SkillEndFrame = NowFrame + skillFrame;
	}

	protected void SetSkillCancelFrame(float skillCancelTime)
	{
		SetSkillCancelFrame(ConvertTimeToFrame(skillCancelTime));
	}

	protected void SetSkillCancelFrame(int skillCancelFrame)
	{
		SkillCancelFrame = NowFrame + skillCancelFrame;
	}

	protected void SetSpeed(float speedX, float speedY)
	{
		_refEntity.SetSpeed(Mathf.RoundToInt(speedX), Mathf.RoundToInt(speedY));
	}

	protected void ResetSpeed()
	{
		SetSpeed(0f, 0f);
	}

	protected void ActionSetSkillEnd()
	{
		SetSkillEnd();
	}

	protected void ActionSetNextSkillStatus()
	{
		ShiftSkillStatus();
	}

	protected void ActionCheckNextSkillStatus()
	{
		if (CheckSkillFrameEnd())
		{
			ActionSetNextSkillStatus();
		}
	}

	protected void ActionCheckSkillEnd()
	{
		if (CheckSkillFrameEnd())
		{
			ActionSetSkillEnd();
		}
	}

	protected void ActionCheckSkillCancel()
	{
		if (CheckSkillCancel() && CheckIsAnyHeld())
		{
			ActionSetSkillEnd();
		}
	}

	protected void ActionCheckSkillCancelOrEnd()
	{
		ActionCheckSkillCancel();
		ActionCheckSkillEnd();
	}

	public virtual void Awake()
	{
		ChildTransforms = _refEntity.transform.GetComponentsInChildren<Transform>(true);
	}

	public override void Start()
	{
		base.Start();
		AttachSkillDelegateEvent();
	}

	public virtual void OnDestroy()
	{
		_skillDependDelegators.Clear();
		_skillDependDelegators = null;
		OnPlayerPressSkill0Events.Clear();
		OnPlayerPressSkill1Events.Clear();
		OnPlayerReleaseSkill0Events.Clear();
		OnPlayerReleaseSkill1Events.Clear();
		OnPlayerPressSkill0Events = null;
		OnPlayerPressSkill1Events = null;
		OnPlayerReleaseSkill0Events = null;
		OnPlayerReleaseSkill1Events = null;
	}

	protected IEnumerator ToggleExtraTransforms(bool isActive, float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleExtraTransforms(isActive);
	}

	protected virtual void ToggleExtraTransforms(bool isActive)
	{
	}

	protected virtual void AttachSkillDelegateEvent()
	{
		OnPlayerPressSkill0Events[0] = OnPlayerPressSkill0;
		OnPlayerPressSkill1Events[0] = OnPlayerPressSkill1;
	}

	protected sealed override void OnPlayerPressSkillCharacterCall(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		switch (skillID)
		{
		case SkillID.SKILL_0:
		{
			Action<SkillID> value2;
			if (OnPlayerPressSkill0Events.TryGetValue(weaponStruct.Reload_index, out value2) && value2 != null && CheckCanTriggerSkill(skillID))
			{
				value2(skillID);
			}
			break;
		}
		case SkillID.SKILL_1:
		{
			Action<SkillID> value;
			if (OnPlayerPressSkill1Events.TryGetValue(weaponStruct.Reload_index, out value) && value != null && CheckCanTriggerSkill(skillID))
			{
				value(skillID);
			}
			break;
		}
		}
	}

	protected virtual void OnPlayerPressSkill0(SkillID skillID)
	{
		PlayerStopDashing();
		SetSkillAndWeapon(skillID);
		SetSkillStatus(OrangeCharacter.SubStatus.SKILL0);
	}

	protected virtual void OnPlayerPressSkill1(SkillID skillID)
	{
		PlayerStopDashing();
		SetSkillAndWeapon(skillID);
		SetSkillStatus(OrangeCharacter.SubStatus.SKILL1);
	}

	protected sealed override void OnPlayerReleaseSkillCharacterCall(SkillID skillID)
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[(int)skillID];
		switch (skillID)
		{
		case SkillID.SKILL_0:
		{
			Action<SkillID> value2;
			if (OnPlayerReleaseSkill0Events.TryGetValue(weaponStruct.Reload_index, out value2) && value2 != null && CheckCanTriggerSkill(skillID))
			{
				value2(skillID);
			}
			break;
		}
		case SkillID.SKILL_1:
		{
			Action<SkillID> value;
			if (OnPlayerReleaseSkill1Events.TryGetValue(weaponStruct.Reload_index, out value) && value != null && CheckCanTriggerSkill(skillID))
			{
				value(skillID);
			}
			break;
		}
		}
	}

	protected virtual void OnPlayerReleaseSkill0(SkillID skillID)
	{
		PlayerStopDashing();
		SetSkillAndWeapon(skillID);
		SetSkillStatus(OrangeCharacter.SubStatus.SKILL0);
	}

	protected virtual void OnPlayerReleaseSkill1(SkillID skillID)
	{
		PlayerStopDashing();
		SetSkillAndWeapon(skillID);
		SetSkillStatus(OrangeCharacter.SubStatus.SKILL1);
	}

	protected override void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			ToggleWeapon(WeaponState.NORMAL);
			break;
		case OrangeCharacter.MainStatus.SKILL:
		{
			SkillStateDelegateData delegator;
			if (TryGetDelegator(out delegator))
			{
				Action onAnimationEnd = delegator.OnAnimationEnd;
				if (onAnimationEnd != null)
				{
					onAnimationEnd();
				}
			}
			break;
		}
		}
	}

	protected override void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				ToggleWeapon(WeaponState.TELEPORT_IN);
			}
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if ((uint)subStatus <= 1u)
			{
				ToggleWeapon(WeaponState.TELEPORT_OUT);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
		{
			SkillStateDelegateData delegator;
			if (TryGetDelegator(out delegator))
			{
				Action onStatusChanged = delegator.OnStatusChanged;
				if (onStatusChanged != null)
				{
					onStatusChanged();
				}
			}
			break;
		}
		case OrangeCharacter.MainStatus.SLASH:
			break;
		}
	}

	protected sealed override void OnCheckSkill(int nowFrame)
	{
		LogicUpdateCharacterDepend();
	}

	protected virtual void LogicUpdateCharacterDepend()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		SkillStateDelegateData delegator;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL && TryGetDelegator(out delegator))
		{
			Action onLogicUpdate = delegator.OnLogicUpdate;
			if (onLogicUpdate != null)
			{
				onLogicUpdate();
			}
		}
	}
}
