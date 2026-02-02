using System;
using OrangeAudio;
using UnityEngine;

public class CH037_Controller : CharacterControlBase
{
	private bool toggleTeleportInFlg;

	private bool toggleTeleportOutFlg;

	private CH037_Armor _refArmor;

	private CH037_Kobun _refKobun;

	private int conditionId = -1;

	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private readonly string FX_LINK_01 = "fxuse_tron_skill2_001";

	private readonly string FX_LINK_02 = "fxuse_tron_skill2_002";

	private int skl2CraeteCount;

	private Vector3 bulletDirection = Vector3.zero;

	private void Awake()
	{
		_refArmor = GetComponentInChildren<CH037_Armor>();
		_refKobun = GetComponentInChildren<CH037_Kobun>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_LINK_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_LINK_02, 2);
	}

	public override void Start()
	{
		base.Start();
		conditionId = _refEntity.PlayerSkills[0].FastBulletDatas[0].n_CONDITION_ID;
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "Sub_Tron_ShootPoint2", true);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.PlayerHeldShootCB = PlayerHeldShoot;
		_refEntity.PlayerPressSelectCB = PlayerPressSelect;
		_refEntity.PlayerPressSkillCB = PlayerPressSkill;
		_refEntity.PlayerReleaseSkillCB = PlayerReleaseSkill;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
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

	public void TeleportInCharacterDepend()
	{
		if (toggleTeleportInFlg)
		{
			return;
		}
		if (_refEntity.CurrentFrame >= 0.8f)
		{
			toggleTeleportInFlg = true;
			_refKobun.gameObject.SetActive(false);
			PlayVoiceSE("v_ch037_start02");
			return;
		}
		if (!_refKobun.gameObject.activeSelf)
		{
			_refKobun.gameObject.SetActive(true);
		}
		_refKobun.Play(_refEntity.AnimateID);
	}

	public void TeleportOutCharacterDepend()
	{
		if (!toggleTeleportOutFlg)
		{
			toggleTeleportOutFlg = true;
			if (_refArmor.IsLink)
			{
				_refArmor.TeleportOutCharacterDepend();
				return;
			}
			_refKobun.gameObject.SetActive(true);
			_refEntity.CharacterMaterials.SetSubCharacterMaterial(_refKobun.gameObject);
			_refKobun.Play(_refEntity.AnimateID);
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
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (!_refEntity.CheckActStatusEvt(15, -1) && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlaySkillSE("ch037_gustav");
				PlayVoiceSE("v_ch037_skill01");
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[0]);
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity._transform);
			}
			break;
		case 1:
			if ((_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
				_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity._transform);
				_refEntity.SetHorizontalSpeed(0);
				_refEntity.PlayerStopDashing();
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IsShoot = 1;
				_refEntity.CheckLockDirection();
				_refEntity.IsShoot = 0;
				skl2CraeteCount = 8;
				SetLogicFrame(CH037_SKL_02.FRAME_TRIGGER, CH037_SKL_02.FRAME_END);
				bulletDirection = new Vector3(_refEntity.direction, 0f, 0f);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_LINK_01, base.transform.position, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_LINK_02, base.transform.position, (_refEntity._characterDirection == CharacterDirection.RIGHT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				PlaySkillSE("ch037_rush");
				PlayVoiceSE("v_ch037_skill02");
				_refEntity.SoundSource.PlaySE(_refEntity.VoiceID, "v_ch037_skill02_01", 0.8f);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
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
			if (!_refEntity.bLockInputCtrl && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionId))
			{
				_refArmor.CancelLink();
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
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= skillEventFrame)
			{
				if (skl2CraeteCount > 0)
				{
					_refArmor.CreateSkillBullet(_refEntity.PlayerSkills[1].BulletData, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.ExtraTransforms[0], bulletDirection, _refEntity, _refEntity.PlayerSkills[1].SkillLV);
					skl2CraeteCount--;
					skillEventFrame++;
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				_refEntity.EnableCurrentWeapon();
				ResetLastStatus();
			}
			break;
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
		}
		else if (_refEntity.CurrentActiveSkill != -1)
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

	public override void SetStun(bool enable)
	{
		if (!_refArmor.IsLink)
		{
			_refEntity.EnableCurrentWeapon();
		}
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
		return new string[1] { "ch037_skill_02_stand" };
	}
}
