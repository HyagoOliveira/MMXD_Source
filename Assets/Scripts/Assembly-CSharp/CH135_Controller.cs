using System;
using System.Collections;
using UnityEngine;

public class CH135_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private Transform _tfWind;

	private ParticleSystem _psWindBitL;

	private ParticleSystem _psWindBitR;

	private FxBase fxUseSkl0;

	private readonly int SKL0_TRIGGER = (int)(0.34f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

	private readonly string FX_000 = "fxuse_GiftElk_000";

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		_refEntity.ExtraTransforms = new Transform[1];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ModelTransform;
		_tfWind = OrangeBattleUtility.FindChildRecursive(ref target, "WingsMesh_a", true);
		_psWindBitL = OrangeBattleUtility.FindChildRecursive(ref target, "bitsL", true).GetComponent<ParticleSystem>();
		_psWindBitR = OrangeBattleUtility.FindChildRecursive(ref target, "bitsR", true).GetComponent<ParticleSystem>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_000, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.PlayTeleportOutEffectEvt = PlayTeleportOutEffect;
	}

	public void TeleportInCharacterDepend()
	{
		_tfWind.gameObject.SetActive(true);
		_psWindBitL.Play(true);
		_psWindBitR.Play(true);
	}

	private void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.7f && currentFrame <= 2f)
			{
				ToggleWing(false);
			}
		}
	}

	public override void ControlCharacterDead()
	{
		ToggleWing(false);
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	protected void StageTeleportInCharacterDepend()
	{
		ToggleWing(false);
		StopAllCoroutines();
		StartCoroutine(OnToggleWing(true, 0.6f));
	}

	protected void StageTeleportOutCharacterDepend()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT)
		{
			StartCoroutine(OnToggleWing(false, 0.2f));
		}
		else if (!_refEntity.Animator.IsDefaultAnimator)
		{
			StartCoroutine(OnToggleWing(false, 0.2f));
		}
	}

	private IEnumerator OnToggleWing(bool isActive, float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleWing(isActive);
	}

	private void ToggleWing(bool isActive)
	{
		_tfWind.gameObject.SetActive(isActive);
		if (isActive)
		{
			_psWindBitL.Play(true);
			_psWindBitR.Play(true);
		}
		else
		{
			_psWindBitL.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			_psWindBitR.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	private void PlayTeleportOutEffect()
	{
		Vector3 p_worldPos = base.transform.position;
		if (_refEntity != null)
		{
			p_worldPos = _refEntity.AimPosition;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_OUT", p_worldPos, Quaternion.identity, Array.Empty<object>());
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id != 0 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
			_refEntity.CheckUsePassiveSkill(1, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
			PlayVoiceSE("v_ic_skill04");
			PlaySkillSE("ic_box01");
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1)
		{
			if (id != 0)
			{
				int num = 1;
			}
			else if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(0, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
				_refEntity.IsShoot = 1;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				fxUseSkl0 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(FX_000, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				PlayVoiceSE("v_ic_skill03");
				PlaySkillSE("ic_reindeer01");
			}
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
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct.ShootTransform[0], MagazineType.NORMAL, -1, 1, false);
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
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.NORMAL, -1, 0, false);
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
		fxUseSkl0 = null;
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
		if (fxUseSkl0 != null)
		{
			fxUseSkl0.BackToPool();
		}
		fxUseSkl0 = null;
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
		return new string[6] { "ch135_skill_01_crouch", "ch135_skill_01_stand", "ch135_skill_01_jump", "ch135_skill_02_crouch", "ch135_skill_02_stand", "ch135_skill_02_jump" };
	}
}
