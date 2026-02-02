using OrangeAudio;
using UnityEngine;

public class CH068_Controller : CharacterControlBase
{
	private ArmorBase _refArmor;

	private int conditionId = -1;

	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private Transform shootPointTransform;

	private bool toggleTeleportOutFlg;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly int SKL1_TRIGGER = (int)(0.08f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.533f / GameLogicUpdateManager.m_fFrameLen);

	private void Awake()
	{
		_refArmor = GetComponentInChildren<ArmorBase>();
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		conditionId = _refEntity.PlayerSkills[0].FastBulletDatas[0].n_CONDITION_ID;
		shootPointTransform = new GameObject(sCustomShootPoint).transform;
		shootPointTransform.SetParent(base.transform);
		shootPointTransform.localPosition = new Vector3(0f, 0.5f, 0f);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerHeldShootCB = PlayerHeldShoot;
		_refEntity.PlayerPressSelectCB = PlayerPressSelect;
		_refEntity.PlayerPressSkillCB = PlayerPressSkill;
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.OverrideAnimatorParamtersEvt = OverrideAnimatorParamters;
		_refEntity.PlWallJumpCheckEvt = PlWallJumpCheck;
		_refEntity.PlWallStopCheckEvt = PlWallStopCheck;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
		_refEntity.CheckAvalibaleForRideArmorEvt = CheckAvalibaleForRideArmor;
		_refEntity.CheckActStatusEvt = CheckActStatus;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.GetCurrentWeaponObjEvt = GetCurrentWeaponObj;
		_refEntity.PlayVoiceCB = PlayVoice;
		_refEntity.PlayCharaSeCB = PlayCharaSE;
	}

	public void TeleportOutCharacterDepend()
	{
		if (!toggleTeleportOutFlg)
		{
			toggleTeleportOutFlg = true;
			if (_refArmor.IsLink)
			{
				_refArmor.TeleportOutCharacterDepend();
			}
		}
	}

	public void PlayerPressSkill(int id)
	{
		if (_refArmor.IsLink)
		{
			_refArmor.PlayerPressSkillCharacterCall(id);
		}
		else
		{
			_refEntity.PlayerPressSkill(id);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && !_refEntity.CheckActStatusEvt(15, -1) && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[0]);
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity._transform);
			PlayVoiceSE("v_va_skill03");
			PlaySkillSE("va_bb01");
			PlaySE("BattleSE", "bt_ridearmor00");
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.IsShoot = 1;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			PlayVoiceSE("v_va_skill04");
		}
	}

	public void PlayerReleaseSkill(int id)
	{
		if (_refArmor.IsLink)
		{
			_refArmor.PlayerReleaseSkillCharacterCall(id);
		}
		else
		{
			_refEntity.PlayerReleaseSkill(id);
		}
	}

	public override void CheckSkill()
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (_refArmor.IsLink)
		{
			if (!_refEntity.bLockInputCtrl && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionId) && _refArmor.CancelLink())
			{
				PlaySkillSE("va_bb02");
			}
			_refArmor.CheckSkill();
			return;
		}
		if (!_refArmor.IsLink && _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionId))
		{
			_refArmor.Link(_refEntity);
			return;
		}
		_refArmor.UpdateSkillUseTimer();
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1 || _refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if (curSubStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform, MagazineType.ENERGY, -1, 1);
			}
		}
	}

	public void OverrideAnimatorParamters()
	{
		if (_refArmor.IsLink)
		{
			HumanBase.AnimateId animateUpperID = (HumanBase.AnimateId)_refEntity.AnimationParams.AnimateUpperID;
			_refArmor.OverrideAnimator(animateUpperID);
		}
	}

	public override void ControlCharacterDead()
	{
		if (_refArmor.IsLink)
		{
			_refArmor.CancelLink();
			_refEntity.ForceSetAnimateId(HumanBase.AnimateId.ANI_HURT_LOOP);
			_refEntity.DisableCurrentWeapon();
			_refEntity.CharacterMaterials.Disappear(null, 0.3f);
		}
	}

	public override void ClearSkill()
	{
		if (_refArmor.IsLink)
		{
			_refArmor.ClearSkill();
			return;
		}
		isSkillEventEnd = false;
		if (_refEntity.CurrentActiveSkill != -1)
		{
			if (_refEntity.IgnoreGravity)
			{
				_refEntity.IgnoreGravity = false;
			}
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
			ResetLastStatus();
		}
	}

	private void ResetLastStatus()
	{
		_refEntity.EnableCurrentWeapon();
		_refEntity.Dashing = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.PlayerStopDashing();
		_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		_refEntity.Animator._animator.speed = 1f;
		_refEntity.CurrentActiveSkill = -1;
	}

	private void SetLogicFrame(int p_triggerFrame, int p_endFrame)
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		skillEventFrame = nowFrame + p_triggerFrame;
		endFrame = nowFrame + p_endFrame;
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		switch (_refEntity.AnimateID)
		{
		default:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)66u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			break;
		}
	}

	public bool PlWallStopCheck(int dir)
	{
		if (_refArmor.IsLink)
		{
			return false;
		}
		return _refEntity.PlWallStopCheck(dir);
	}

	public bool PlWallJumpCheck()
	{
		if (_refArmor.IsLink)
		{
			return false;
		}
		return _refEntity.PlWallJumpCheck();
	}

	public void PlayerHeldShoot()
	{
		if (_refArmor.IsLink)
		{
			_refArmor.PlayerHeldShoot();
		}
		else
		{
			_refEntity.PlayerHeldShoot();
		}
	}

	public void PlayerPressSelect()
	{
		if (_refArmor.IsLink)
		{
			_refArmor.PlayerPressSelect();
		}
		else
		{
			_refEntity.PlayerPressSelect();
		}
	}

	public bool CheckAvalibaleForRideArmor()
	{
		bool isLink = _refArmor.IsLink;
		return _refEntity.CheckAvalibaleForRideArmor();
	}

	public bool CheckActStatus(int mainstatus, int substatus)
	{
		if (_refArmor.IsLink && mainstatus == 15)
		{
			return true;
		}
		return _refEntity.CheckActStatus(mainstatus, substatus);
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_refArmor.IsLink)
		{
			return false;
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	protected void CheckSkillLockDirection()
	{
		if (!_refArmor.IsLink)
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public WeaponStruct GetCurrentWeaponObj()
	{
		if (_refArmor.IsLink)
		{
			return _refArmor.GetCurrentWeaponStruct();
		}
		return _refEntity.GetCurrentWeaponObj();
	}

	public void PlayVoice(Voice seId)
	{
		if (_refArmor.IsLink)
		{
			_refArmor.PlayVoice(seId);
		}
		else
		{
			_refEntity.PlayVoice(seId);
		}
	}

	public void PlayCharaSE(CharaSE seId)
	{
		if (_refArmor.IsLink)
		{
			_refArmor.PlayCharaSE(seId);
		}
		else
		{
			_refEntity.PlayCharaSE(seId);
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch068_skill_02_crouch", "ch068_skill_02_stand", "ch068_skill_02_jump" };
	}
}
