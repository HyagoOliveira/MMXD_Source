using System;
using System.Collections.Generic;
using UnityEngine;

public class CH069_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private int SKL1_LOOP_FRAME;

	private bool GuardActive;

	private Transform shootPointTransform;

	private GameObject saberMesh_m;

	private GameObject SaberFX;

	private MeleeWeaponTrail saberTrail;

	private ParticleSystem fxWin;

	private readonly int[] arrGuardCondtion = new int[4] { 1182, 1183, 1184, 1297 };

	private readonly List<HumanBase.AnimateId> listCrouchAnim = new List<HumanBase.AnimateId>
	{
		(HumanBase.AnimateId)68u,
		(HumanBase.AnimateId)71u,
		(HumanBase.AnimateId)74u
	};

	private readonly string sFxuse000 = "fxuse_thunderslash_000";

	private readonly string sFxuse001_0 = "fxuse_counterslash_000";

	private readonly string sFxuse001_1 = "fxuse_counterslash_001";

	private readonly string sFxWin = "p_iris_core";

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly string sSaberMesh_m = "SaberMesh_m";

	private readonly string sSaberFX = "SaberFX";

	private readonly string sSaberTrail = "SaberTrail";

	private readonly int SKL0_TRIGGER = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_0_START_TRIGGER = 1;

	private readonly int SKL1_0_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_0_LOOP_BREAK = 1;

	private readonly int SKL1_0_END_END = (int)(0.42f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_TRIGGER = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_END = (int)(0.8f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_END_BREAK = (int)(0.45f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001_0);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001_1);
		shootPointTransform = new GameObject(sCustomShootPoint).transform;
		shootPointTransform.SetParent(base.transform);
		shootPointTransform.localPosition = new Vector3(0f, 0.85f, 0f);
		_refEntity.PlayerSkills[0].ShootTransform[0] = shootPointTransform;
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		saberMesh_m = OrangeBattleUtility.FindChildRecursive(ref target, sSaberMesh_m, true).gameObject;
		SaberFX = OrangeBattleUtility.FindChildRecursive(ref target, sSaberFX, true).gameObject;
		saberTrail = OrangeBattleUtility.FindChildRecursive(ref target, sSaberTrail, true).GetComponent<MeleeWeaponTrail>();
		fxWin = OrangeBattleUtility.FindChildRecursive(ref target, sFxWin, true).GetComponent<ParticleSystem>();
		float num = (float)_refEntity.PlayerSkills[1].BulletData.n_FIRE_SPEED / 1000f;
		SKL1_LOOP_FRAME = (int)(num / GameLogicUpdateManager.m_fFrameLen) - (SKL1_0_START_END - SKL1_0_START_TRIGGER);
		GuardActive = false;
		_refEntity.PlayerSkills[1].LastUseTimer.SetTime(9999f);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.GuardCalculateEvt = GuardCalculate;
		_refEntity.GuardHurtEvt = GuardHurt;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.9f)
		{
			UpdateCustomWeaponRenderer(false);
		}
	}

	public void TeleportOutCharacterDepend()
	{
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_WIN_POSE)
		{
			if (fxWin.isPlaying)
			{
				fxWin.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		else if (!fxWin.isPlaying)
		{
			fxWin.Play();
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
			if (_refEntity.CheckUseSkillKeyTriggerEX2(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IsShoot = 1;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTriggerEX2(id))
			{
				SetGuardInactive();
				switch (_refEntity.PlayerSkills[id].Reload_index)
				{
				case 0:
					_refEntity.CurrentActiveSkill = id;
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_0_START_TRIGGER, SKL1_0_START_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
					break;
				case 1:
					_refEntity.CurrentActiveSkill = id;
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_1_END_BREAK;
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_1_TRIGGER, SKL1_1_END, OrangeCharacter.SubStatus.SKILL1_3, out skillEventFrame, out endFrame);
					break;
				}
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.FreshBullet = true;
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u, listCrouchAnim);
				UpdateCustomWeaponRenderer(true, true);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				UpdateCustomWeaponRenderer(true, true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001_0, _refEntity.ModelTransform.position, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)71u, (HumanBase.AnimateId)72u, (HumanBase.AnimateId)73u, listCrouchAnim);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)74u, (HumanBase.AnimateId)75u, (HumanBase.AnimateId)76u, listCrouchAnim);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)77u, (HumanBase.AnimateId)78u, (HumanBase.AnimateId)79u, listCrouchAnim);
				UpdateCustomWeaponRenderer(true, true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001_1, _refEntity.ModelTransform.position, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				break;
			}
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
		SetGuardInactive();
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			UpdateCustomWeaponRenderer(false);
			_refEntity.EnableCurrentWeapon();
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
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				Vector3 fxOffset = GetFxOffset(shootPointTransform.position);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse000, fxOffset, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, base.transform, MagazineType.ENERGY, -1, 0);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_0_LOOP_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 1, SKL1_0_START_TRIGGER, SKL1_LOOP_FRAME, OrangeCharacter.SubStatus.SKILL1_1, out skillEventFrame, out endFrame);
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				WeaponStruct currentSkillObj2 = _refEntity.GetCurrentSkillObj();
				int reload_index2 = currentSkillObj2.Reload_index;
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj2.weaponStatus, base.transform, null, reload_index2);
				GuardActive = true;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				SetGuardInactive();
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 1, SKL1_0_START_TRIGGER, SKL1_0_END_END, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
			}
			else if (nowFrame >= endBreakFrame)
			{
				ChkCounterStatus();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else
			{
				ChkCounterStatus();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				int reload_index = currentSkillObj.Reload_index;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, base.transform, MagazineType.ENERGY, reload_index, 0);
				ComboCheckData[] comboCheckDatas = currentSkillObj.ComboCheckDatas;
				for (int i = 0; i < comboCheckDatas.Length; i++)
				{
					_refEntity.RemoveComboSkillBuff(comboCheckDatas[i].nComboSkillID);
				}
				currentSkillObj.Reload_index = 0;
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		}
	}

	private void ChkCounterStatus()
	{
		if (ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.SKILL0))
		{
			if (_refEntity.CanPlayerPressSkill(0, false))
			{
				isSkillEventEnd = false;
				_refEntity.CurrentActiveSkill = -1;
				_refEntity.PlayerPressSkill(0);
				return;
			}
		}
		else if (ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.SKILL1))
		{
			isSkillEventEnd = false;
			_refEntity.CurrentActiveSkill = -1;
			_refEntity.PlayerPressSkill(1);
			return;
		}
		if (_refEntity.AnimateID == (HumanBase.AnimateId)71u && ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
		{
			if (ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.SHOOT))
			{
				OnSkillEnd();
			}
		}
		else if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT) || ManagedSingleton<InputStorage>.Instance.IsPressed(_refEntity.UserID, ButtonId.SHOOT))
		{
			OnSkillEnd();
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
		SetGuardInactive();
		switch (_refEntity.AnimateID)
		{
		default:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)66u:
		case (HumanBase.AnimateId)69u:
		case (HumanBase.AnimateId)72u:
		case (HumanBase.AnimateId)75u:
		case (HumanBase.AnimateId)78u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
		case (HumanBase.AnimateId)68u:
		case (HumanBase.AnimateId)71u:
		case (HumanBase.AnimateId)74u:
		case (HumanBase.AnimateId)77u:
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

	private void UpdateCustomWeaponRenderer(bool enableWeapon, bool enableTrail = false)
	{
		saberMesh_m.SetActive(enableWeapon);
		SaberFX.SetActive(enableWeapon);
		saberTrail.Emit = enableTrail;
	}

	private Vector3 GetFxOffset(Vector3 fxPosistion)
	{
		switch (_refEntity.AnimateID)
		{
		case HumanBase.AnimateId.ANI_SKILL_START:
		case (HumanBase.AnimateId)68u:
		case (HumanBase.AnimateId)71u:
		case (HumanBase.AnimateId)74u:
		case (HumanBase.AnimateId)77u:
			fxPosistion.y -= 0.25f;
			break;
		}
		return fxPosistion;
	}

	private void SetGuardInactive()
	{
		GuardActive = false;
		PerBuffManager selfBuffManager = _refEntity.selfBuffManager;
		int[] array = arrGuardCondtion;
		foreach (int cONDITIONID in array)
		{
			selfBuffManager.RemoveBuffByCONDITIONID(cONDITIONID);
		}
	}

	public override bool GuardCalculate(HurtPassParam tHurtPassParam)
	{
		if ((int)_refEntity.Hp > 0)
		{
			return GuardActive;
		}
		return false;
	}

	public void GuardHurt(HurtPassParam tHurtPassParam)
	{
		tHurtPassParam.dmg = 0;
		if (_refEntity.IsLocalPlayer)
		{
			_refEntity.tRefPassiveskill.HurtTrigger(ref tHurtPassParam.dmg, _refEntity.GetCurrentWeaponObj().weaponStatus.nWeaponCheck, ref _refEntity.selfBuffManager, _refEntity.CreateBulletByLastWSTranform);
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[15]
		{
			"ch069_skill_01_crouch", "ch069_skill_01_stand", "ch069_skill_01_jump", "ch069_skill_02_step1_crouch_start", "ch069_skill_02_step1_stand_start", "ch069_skill_02_step1_jump_start", "ch069_skill_02_step1_crouch_loop", "ch069_skill_02_step1_stand_loop", "ch069_skill_02_step1_jump_loop", "ch069_skill_02_step1_crouch_end",
			"ch069_skill_02_step1_stand_end", "ch069_skill_02_step1_jump_end", "ch069_skill_02_step2_crouch", "ch069_skill_02_step2_stand", "ch069_skill_02_step2_jump"
		};
	}
}
