using System;
using UnityEngine;

public class CH085_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	protected int _enhanceSlot;

	private CharacterMaterial cmGem;

	protected ObjInfoBar mEffect_Hide_obj;

	private readonly string FX0_000 = "fxuse_nightmareball_007";

	private readonly string FX0_001 = "fxuse_nightmareball_006";

	private readonly string FX0_002 = "fxuse_nightmareball_008";

	private readonly string FX0_003 = "fxuse_nightmareball_009";

	private readonly string FX1_000 = "fxuse_nightmare_000";

	private readonly string FX1_001 = "fxuse_nightmare_001";

	private readonly int SKL0_TRIGGER = (int)(0.21f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.556f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.26f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.689f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.62f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitEnhanceSkill();
		InitializeSkill();
	}

	protected virtual void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[5];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0f, 0f);
		_refEntity.ExtraTransforms[2] = transform;
		_refEntity.ExtraTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip R Forearm", true);
		_refEntity.ExtraTransforms[4] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip L Forearm", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX0_000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX0_001);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX0_002);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX1_000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX1_001);
		CharacterMaterial[] components = _refEntity.ModelTransform.GetComponents<CharacterMaterial>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].GetTexturesCount > 0)
			{
				cmGem = components[i];
				break;
			}
		}
	}

	private void InitEnhanceSkill()
	{
		_enhanceSlot = _refEntity.PlayerSkills[0].EnhanceEXIndex;
		int skillId = (new int[4] { 20501, 20502, 20503, 20504 })[_enhanceSlot];
		_refEntity.ReInitSkillStruct(0, skillId);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
	}

	public void TeleportInExtraEffect()
	{
		mEffect_Hide_obj = _refEntity.transform.GetComponentInChildren<ObjInfoBar>();
		if ((bool)mEffect_Hide_obj)
		{
			mEffect_Hide_obj.gameObject.SetActive(false);
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN && subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE && (bool)mEffect_Hide_obj)
		{
			mEffect_Hide_obj.gameObject.SetActive(true);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_gt_skill02");
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			PlaySkillSE("gt_strike");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX1_000, _refEntity.ExtraTransforms[4], Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX1_001, _refEntity.ModelTransform.position, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_gt_skill01");
			OrangeCharacter.SubStatus p_nextStatus = OrangeCharacter.SubStatus.SKILL0;
			string pFxName = FX0_000;
			switch (_enhanceSlot)
			{
			case 1:
				p_nextStatus = OrangeCharacter.SubStatus.SKILL0_1;
				pFxName = FX0_001;
				break;
			case 2:
				p_nextStatus = OrangeCharacter.SubStatus.SKILL0_2;
				pFxName = FX0_002;
				break;
			case 3:
				p_nextStatus = OrangeCharacter.SubStatus.SKILL0_3;
				pFxName = FX0_003;
				break;
			}
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, p_nextStatus, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, (HumanBase.AnimateId)128u, (HumanBase.AnimateId)129u);
			PlaySkillSE("gt_hole01");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(pFxName, _refEntity.ExtraTransforms[3], Quaternion.identity, Array.Empty<object>());
			cmGem.UpdateTex(_enhanceSlot);
			cmGem.UpdateEmission(2.5f);
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
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 1);
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
				_refEntity.ExtraTransforms[2].localPosition = new Vector3(1.5f * (float)_refEntity._characterDirection, 0.8f, 0f);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[2], MagazineType.ENERGY, -1, 0);
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
		if ((bool)cmGem)
		{
			cmGem.UpdateTex(3);
			cmGem.UpdateEmission(0f);
		}
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
		case (HumanBase.AnimateId)128u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
		case HumanBase.AnimateId.ANI_BTSKILL_START:
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

	public override void ClearSkill()
	{
		_refEntity.EnableCurrentWeapon();
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		if ((bool)cmGem)
		{
			cmGem.UpdateTex(3);
			cmGem.UpdateEmission(0f);
		}
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch085_skill_01_crouch_up", "ch085_skill_01_crouch_front", "ch085_skill_01_crouch_down" };
		string[] array2 = new string[3] { "ch085_skill_01_stand_up", "ch085_skill_01_stand_front", "ch085_skill_01_stand_down" };
		string[] array3 = new string[3] { "ch085_skill_01_jump_up", "ch085_skill_01_jump_front", "ch085_skill_01_jump_down" };
		return new string[3][] { array, array2, array3 };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[3] { "ch085_skill_02_crouch", "ch085_skill_02_stand", "ch085_skill_02_jump" };
	}
}
