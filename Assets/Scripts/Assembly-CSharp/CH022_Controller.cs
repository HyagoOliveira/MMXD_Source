using System;
using UnityEngine;

public class CH022_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	protected int _enhanceSlot;

	private CharacterDirection characterDirectionCache = CharacterDirection.RIGHT;

	private readonly string sFxuse_skl000 = "fxuse_overdistance_000";

	private readonly string sFxuse_skl100 = "fxuse_precise_000";

	private readonly int SKL0_TRIGGER = (int)(0.21f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.511f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.49f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = 1;

	private readonly int SKL1_END = (int)(0.9f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.86f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitEnhanceSkill();
		InitializeSkill();
	}

	protected virtual void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl100);
	}

	private void InitEnhanceSkill()
	{
		_enhanceSlot = _refEntity.PlayerSkills[0].EnhanceEXIndex;
		int skillId = (new int[4] { 21201, 21202, 21203, 21201 })[_enhanceSlot];
		_refEntity.ReInitSkillStruct(0, skillId);
		for (int i = 0; i < _refEntity.PlayerSkills[0].FastBulletDatas.Length; i++)
		{
			string s_MODEL = _refEntity.PlayerSkills[0].FastBulletDatas[i].s_MODEL;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(s_MODEL) && !MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(s_MODEL))
			{
				BulletBase.PreloadBullet<BulletBase>(_refEntity.PlayerSkills[0].FastBulletDatas[i]);
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != 0)
		{
			return;
		}
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if (curSubStatus == OrangeCharacter.SubStatus.IDLE && characterDirectionCache != _refEntity._characterDirection)
		{
			characterDirectionCache = _refEntity._characterDirection;
			if (characterDirectionCache == CharacterDirection.RIGHT)
			{
				_refEntity.CharacterMaterials.UpdateTex(0);
			}
			else
			{
				_refEntity.CharacterMaterials.UpdateTex(1);
			}
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
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_na_skill01");
				string text = "";
				OrangeCharacter.SubStatus subStatus = OrangeCharacter.SubStatus.SKILL0;
				switch (_enhanceSlot)
				{
				default:
					text = "na_domega02";
					subStatus = OrangeCharacter.SubStatus.SKILL0;
					break;
				case 1:
					text = "na_domega03";
					subStatus = OrangeCharacter.SubStatus.SKILL0_1;
					break;
				case 2:
					text = "na_domega04";
					subStatus = OrangeCharacter.SubStatus.SKILL0_2;
					break;
				}
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, subStatus, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
				_refEntity.DisableCurrentWeapon();
				PlaySkillSE("na_domega01");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl000, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				PlaySkillSE(text);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_na_skill02");
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				PlaySkillSE("na_scan01");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl100, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
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
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 0);
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
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 0);
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

	public override void ClearSkill()
	{
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch022_skill_01_crouch", "ch022_skill_01_stand", "ch022_skill_01_jump", "ch022_skill_02_crouch", "ch022_skill_02_stand", "ch022_skill_02_jump" };
	}
}
