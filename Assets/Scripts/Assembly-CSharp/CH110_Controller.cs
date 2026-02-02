using System;
using UnityEngine;

public class CH110_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private OrangeTimer NOVASTRIKETimer;

	private SkinnedMeshRenderer tfBusterMesh;

	private SkinnedMeshRenderer tfHandMesh;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly string sFxuse_skl000 = "fxuse_rift_000";

	private readonly string sFxuse_skl001 = "fxuse_rift_001";

	private readonly string sFxuse_skl100 = "fxuse_lightarrow_001";

	private readonly int SKL1_TRIGGER = (int)(0.07f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.45f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	protected virtual void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[3];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BusterMesh_m", true);
		tfBusterMesh = transform.GetComponent<SkinnedMeshRenderer>();
		tfBusterMesh.enabled = false;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "HandMesh_L_m", true);
		tfHandMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		NOVASTRIKETimer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl001);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl100);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.PlayTeleportOutEffectEvt = PlayTeleportOutEffect;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_an_skill01");
			PlaySkillSE("an_matinee01");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(_refEntity.JumpUpFx, _refEntity._transform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Prepare(_refEntity, 0);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_an_skill02");
			_refEntity.CurrentActiveSkill = id;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, (HumanBase.AnimateId)128u, (HumanBase.AnimateId)129u);
			_refEntity.DisableCurrentWeapon();
			SetCustomWeapon(true);
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
		case OrangeCharacter.SubStatus.RIDE_ARMOR:
			ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Begin(_refEntity, NOVASTRIKETimer, 0);
			break;
		case OrangeCharacter.SubStatus.IDLE:
			ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Loop(_refEntity, NOVASTRIKETimer, 0);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				_refEntity.ShootDirection = Vector3.right * _refEntity.direction;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 0);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl100, _refEntity.ExtraTransforms[1].position, Quaternion.identity, Array.Empty<object>());
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.IDLE)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl000, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			Quaternion p_quaternion = ((_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl001, _refEntity.transform.position, p_quaternion, Array.Empty<object>());
		}
	}

	private void OnSkillEnd()
	{
		switch (_refEntity.AnimateID)
		{
		default:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)128u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
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
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		SetCustomWeapon(false);
		_refEntity.EnableCurrentWeapon();
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0 && _refEntity.BulletCollider != null)
		{
			_refEntity.BulletCollider.BackToPool();
		}
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
			tfBusterMesh.enabled = false;
			_refEntity.EnableCurrentWeapon();
		}
	}

	private void SetCustomWeapon(bool active)
	{
		tfBusterMesh.enabled = active;
		tfHandMesh.enabled = !active;
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			_refEntity.SkillEnd = true;
			_refEntity.BulletCollider.BackToPool();
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
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

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch110_skill_01_start" };
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch110_skill_01_start", "ch110_skill_01_loop" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch110_skill_02_crouch_up", "ch110_skill_02_crouch_mid", "ch110_skill_02_crouch_down" };
		string[] array2 = new string[3] { "ch110_skill_02_stand_up", "ch110_skill_02_stand_mid", "ch110_skill_02_stand_down" };
		string[] array3 = new string[3] { "ch110_skill_02_jump_up", "ch110_skill_02_jump_mid", "ch110_skill_02_jump_down" };
		return new string[3][] { array, array2, array3 };
	}
}
