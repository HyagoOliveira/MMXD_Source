using System;
using UnityEngine;

public class CH104_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private Transform shootPointTransform0;

	private FxBase fxUseSkl1;

	private int SeUseSkl1 = -1;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly string sFxuseCutIn = "fxlogin_ch104_000";

	private readonly string Fxuse000 = "fxuse_hamaya_000";

	private readonly string Fxuse002 = "fxuse_hamaya_001";

	private readonly string Fxuse001 = "fxuse_ch104sill_000";

	private readonly int SKL0_TRIGGER = (int)(0.116f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.16f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.31f / GameLogicUpdateManager.m_fFrameLen);

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
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Fxuse000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Fxuse001);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Fxuse002);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			PlayVoiceSE("v_ir_skill01");
			PlaySkillSE("ir_hinode01");
			fxUseSkl1 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(Fxuse001, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			SeUseSkl1 = LeanTween.delayedCall(base.gameObject, 1.3f, (Action)delegate
			{
				_refEntity.SoundSource.PlaySE(_refEntity.SkillSEID, "ir_hinode02");
			}).uniqueId;
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Fxuse000, _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Fxuse002, _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			PlayVoiceSE("v_ir_skill02");
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
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
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform0, MagazineType.ENERGY, -1, 1);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				CheckBreakFrame();
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
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform0, MagazineType.ENERGY, -1, 1);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				CheckBreakFrame();
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		if (fxUseSkl1 != null)
		{
			fxUseSkl1.BackToPool();
		}
		if (SeUseSkl1 != -1)
		{
			LeanTween.cancel(ref SeUseSkl1, false);
		}
		fxUseSkl1 = null;
		SeUseSkl1 = -1;
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			_refEntity.EnableCurrentWeapon();
		}
	}

	private void CheckBreakFrame()
	{
		if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
		{
			endFrame = nowFrame + 1;
		}
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
		case (HumanBase.AnimateId)69u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
		case (HumanBase.AnimateId)68u:
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

	public override string GetTeleportInExtraEffect()
	{
		return sFxuseCutIn;
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch104_skill_02_crouch", "ch104_skill_02_stand", "ch104_skill_02_jump", "ch104_skill_01_crouch", "ch104_skill_01_stand", "ch104_skill_01_jump" };
	}
}
