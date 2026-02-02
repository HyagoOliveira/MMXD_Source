using System;
using UnityEngine;

public class CH098_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private readonly int conditionId = 1232;

	private readonly string sFxuse000_000 = "fxuse_charge_hadoken_000";

	private readonly string sFxuse000_001 = "fxuse_hadoken_000";

	private readonly string sFxuse000_002 = "fxuse_charge_hadoken_001";

	private readonly string sFxuse000_003 = "fxuse_hadoken_001";

	private readonly string sFxuse001_000 = "fxuse_denjin_000";

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly int SKL0_TRIGGER = (int)(0.16f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.867f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.38f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		Transform[] componentsInChildren = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform transform = new GameObject(sCustomShootPoint + "1").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0f, 0f);
		Transform transform2 = new GameObject(sCustomShootPoint + "2").transform;
		transform2.SetParent(base.transform);
		transform2.localPosition = new Vector3(2f, 0f, 0f);
		_refEntity.ExtraTransforms = new Transform[3]
		{
			OrangeBattleUtility.FindChildRecursive(componentsInChildren, "L WeaponPoint", true),
			transform,
			transform2
		};
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000_000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000_001);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000_002);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000_003);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001_000);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_ry_skill02");
			PlaySkillSE("ry_denzinrenki");
			_refEntity.CurrentActiveSkill = id;
			_refEntity.IsShoot = 1;
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001_000, _refEntity.ExtraTransforms[1].position, OrangeBattleUtility.QuaternionNormal, Array.Empty<object>());
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.IsShoot = 1;
			OrangeCharacter.SubStatus subStatus = OrangeCharacter.SubStatus.SKILL0;
			int reload_index = _refEntity.GetCurrentSkillObj().Reload_index;
			string empty = string.Empty;
			if (reload_index == 0 || reload_index != 1)
			{
				subStatus = OrangeCharacter.SubStatus.SKILL0;
				empty = sFxuse000_000;
				PlayVoiceSE("v_ry_skill01_1");
				PlaySkillSE("ry_shinkuhado");
			}
			else
			{
				subStatus = OrangeCharacter.SubStatus.SKILL0_1;
				empty = sFxuse000_002;
				PlayVoiceSE("v_ry_skill01_2");
				PlaySkillSE("ry_denzinhado");
			}
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, subStatus, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, HumanBase.AnimateId.ANI_BTSKILL_START, HumanBase.AnimateId.ANI_BTSKILL_START);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(empty, _refEntity.ExtraTransforms[0], Quaternion.identity, Array.Empty<object>());
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsLocalPlayer && _refEntity.PlayerSkills[0].Reload_index > 0 && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionId))
		{
			ResetComboIndex(_refEntity.PlayerSkills[0]);
		}
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
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				int reload_index2 = currentSkillObj.Reload_index;
				currentSkillObj.ShootTransform[0] = _refEntity.ExtraTransforms[0];
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[0], MagazineType.ENERGY, reload_index2, 0, false);
				_refEntity.CheckUsePassiveSkill(0, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[0], null, reload_index2);
				_refEntity.ExtraTransforms[2].localPosition = GetGxOffset();
				ManagedSingleton<CharacterControlHelper>.Instance.Play360ShootEft(_refEntity, sFxuse000_001, _refEntity.ExtraTransforms[2].position);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct currentSkillObj2 = _refEntity.GetCurrentSkillObj();
				int reload_index3 = currentSkillObj2.Reload_index;
				currentSkillObj2.ShootTransform[0] = _refEntity.ExtraTransforms[0];
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[0], MagazineType.ENERGY, reload_index3, 0, false);
				_refEntity.CheckUsePassiveSkill(0, currentSkillObj2.weaponStatus, currentSkillObj2.ShootTransform[0], null, reload_index3);
				_refEntity.ExtraTransforms[2].localPosition = GetGxOffset();
				ManagedSingleton<CharacterControlHelper>.Instance.Play360ShootEft(_refEntity, sFxuse000_003, _refEntity.ExtraTransforms[2].position);
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
				int reload_index = _refEntity.GetCurrentSkillObj().Reload_index;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, reload_index, 0);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		}
	}

	private Vector3 GetGxOffset()
	{
		Vector3 vector = _refEntity.ModelTransform.InverseTransformPoint(_refEntity.ExtraTransforms[0].position);
		return new Vector3((float)_refEntity.direction * vector.x, vector.y, vector.z);
	}

	private void ResetComboIndex(WeaponStruct currentSkill)
	{
		ComboCheckData[] comboCheckDatas = currentSkill.ComboCheckDatas;
		for (int i = 0; i < comboCheckDatas.Length; i++)
		{
			_refEntity.RemoveComboSkillBuff(comboCheckDatas[i].nComboSkillID);
		}
		currentSkill.Reload_index = 0;
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.Dashing = false;
			_refEntity.PlayerStopDashing();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			return;
		}
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
	}

	public override void ClearSkill()
	{
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

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length == 2)
		{
			int num = (int)parameters[0];
			int num2 = (int)parameters[1];
			if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT && (int)_refEntity.Hp > 0 && num == 0 && _refEntity.PlayerSkills[0].Reload_index != num2)
			{
				_refEntity.PlayerSkills[0].Reload_index = num2;
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[1] { "ch098_skill_02_stand" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch098_skill_01_stand_up", "ch098_skill_01_stand_mid", "ch098_skill_01_stand_down" };
		return new string[1][] { array };
	}
}
