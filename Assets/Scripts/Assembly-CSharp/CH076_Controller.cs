using System;
using UnityEngine;

public class CH076_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private GameObject saberMesh_main_m;

	private GameObject saberMesh_sub_g;

	private MeleeWeaponTrail saberTrail_R;

	private GameObject flowerMesh_c;

	private readonly string sSaberMesh_main_m = "SaberMesh_main_m";

	private readonly string sSaberMesh_sub_g = "SaberMesh_sub_g";

	private readonly string sWeaponBone_R = "WeaponBone_R";

	private readonly string sFlowerMesh_c = "FlowerMesh_c";

	private readonly string sFxuse000 = "fxuse_promisering_000";

	private readonly string sFxuse100 = "fxuse_weddingcake_000";

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly int SKL0_TRIGGER = (int)(0.2668f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.225f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.9f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.75f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = new GameObject(sCustomShootPoint).transform;
		_refEntity.ExtraTransforms[1].SetParent(base.transform);
		_refEntity.ExtraTransforms[1].localPosition = new Vector3(0f, 0.85f, 0f);
		_refEntity.ExtraTransforms[2] = new GameObject(sCustomShootPoint).transform;
		_refEntity.ExtraTransforms[2].SetParent(base.transform);
		_refEntity.ExtraTransforms[2].localPosition = new Vector3(0f, 0f, 0f);
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[2];
		saberMesh_main_m = OrangeBattleUtility.FindChildRecursive(ref target, sSaberMesh_main_m, true).gameObject;
		saberMesh_sub_g = OrangeBattleUtility.FindChildRecursive(ref target, sSaberMesh_sub_g, true).gameObject;
		saberTrail_R = OrangeBattleUtility.FindChildRecursive(ref target, sWeaponBone_R, true).GetComponent<MeleeWeaponTrail>();
		flowerMesh_c = OrangeBattleUtility.FindChildRecursive(ref target, sFlowerMesh_c, true).gameObject;
		UpdateCustomWeaponRenderer(false);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse100);
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.95f)
		{
			flowerMesh_c.SetActive(false);
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
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				UpdateCustomWeaponRenderer(true, true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse100, _refEntity.ExtraTransforms[2].position + new Vector3(2f, 0f, 0f) * (float)_refEntity._characterDirection, Quaternion.identity, Array.Empty<object>());
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
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 0);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse000, _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[2], MagazineType.ENERGY, -1, 0);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		UpdateCustomWeaponRenderer(false);
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
			UpdateCustomWeaponRenderer(false);
			_refEntity.EnableCurrentWeapon();
		}
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		UpdateCustomWeaponRenderer(false);
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

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if (curSubStatus != OrangeCharacter.SubStatus.SKILL1)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	private void UpdateCustomWeaponRenderer(bool enableWeapon, bool enableTrail = false)
	{
		saberMesh_main_m.SetActive(enableWeapon);
		saberMesh_sub_g.SetActive(enableWeapon);
		saberTrail_R.Emit = enableTrail;
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch076_skill_01_crouch", "ch076_skill_01_stand", "ch076_skill_01_jump", "ch076_skill_02_crouch", "ch076_skill_02_stand", "ch076_skill_02_jump" };
	}
}
