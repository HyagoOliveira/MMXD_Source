using System;
using UnityEngine;

public class CH091_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private int SKL0_EX3_LOOP_FRAME;

	private int SKL1_LOOP_FRAME;

	private bool GuardActive;

	private Transform shootPointTransform;

	private Transform shootPointTransform2;

	private Transform shootPointTransform3;

	private Transform shootPointTransform4;

	private GameObject WeaponMesh_c;

	private GameObject WhipMesh_Sub_e;

	private FxBase chargeFx;

	private FxBase counterFx;

	private readonly int[] arrGuardCondtion = new int[2] { 1205, 1206 };

	protected int _enhanceSlot;

	private readonly string sFxuse000_EX3_0 = "fxuse_bowgun_003";

	private readonly string sFxuse000_EX3_1 = "fxuse_bowgun_002";

	private readonly string sFxuse001_0 = "fxuse_countershot_000";

	private readonly string sFxuse001_1 = "fxuse_countershot_001";

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly string sBipProp1 = "Bip Prop1";

	private readonly string sWeaponMesh_c = "WeaponMesh_c";

	private readonly string sWhipMesh_Sub_e = "WhipMesh_Sub_e";

	private readonly int SKL0_TRIGGER = (int)(0.18207f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.867f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.36f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_EX3_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_EX3_END_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_EX3_END_BREAK = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_0_START_TRIGGER = 1;

	private readonly int SKL1_0_START_END = (int)(0.167f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_0_LOOP_BREAK = 1;

	private readonly int SKL1_0_END_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_0_END_END_AIR = (int)(0.222f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_TRIGGER = (int)(0.323793f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_END = (int)(1.133f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_END_BREAK = (int)(0.83f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_OFFSET_START = (int)(0.37389f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_1_OFFSET_END = (int)(0.81576f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		InitEnhanceSkill();
	}

	private void InitializeSkill()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000_EX3_0);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse000_EX3_1);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001_0);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse001_1);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransform = new GameObject(sCustomShootPoint).transform;
		shootPointTransform.SetParent(base.transform);
		shootPointTransform.localPosition = new Vector3(0f, 0.85f, 0f);
		_refEntity.PlayerSkills[0].ShootTransform[0] = shootPointTransform;
		shootPointTransform2 = new GameObject(sCustomShootPoint + "2").transform;
		shootPointTransform2.SetParent(OrangeBattleUtility.FindChildRecursive(ref target, sBipProp1, true));
		shootPointTransform2.localPosition = new Vector3(0f, 0f, 1.6f);
		shootPointTransform2.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, -90f));
		shootPointTransform2.transform.localScale = new Vector3(1f, 1f, 1f);
		shootPointTransform3 = new GameObject(sCustomShootPoint + "3").transform;
		shootPointTransform3.SetParent(base.transform);
		shootPointTransform3.localPosition = new Vector3(0f, 0f, 0f);
		shootPointTransform4 = new GameObject(sCustomShootPoint + "4").transform;
		shootPointTransform4.SetParent(shootPointTransform3);
		shootPointTransform4.localPosition = new Vector3(0f, 0f, 0f);
		WeaponMesh_c = OrangeBattleUtility.FindChildRecursive(ref target, sWeaponMesh_c, true).gameObject;
		WhipMesh_Sub_e = OrangeBattleUtility.FindChildRecursive(ref target, sWhipMesh_Sub_e, true).gameObject;
		SKL1_LOOP_FRAME = (int)(GetSklTime(1) / GameLogicUpdateManager.m_fFrameLen) - (SKL1_0_START_END - SKL1_0_START_TRIGGER);
		GuardActive = false;
		WhipMesh_Sub_e.SetActive(false);
		_refEntity.PlayerSkills[1].LastUseTimer.SetTime(9999f);
	}

	private float GetSklTime(int idx)
	{
		return (float)_refEntity.PlayerSkills[idx].FastBulletDatas[0].n_FIRE_SPEED / 1000f;
	}

	private void InitEnhanceSkill()
	{
		_enhanceSlot = _refEntity.PlayerSkills[0].EnhanceEXIndex;
		int skillId = (new int[4] { 17101, 17102, 17103, 17104 })[_enhanceSlot];
		_refEntity.ReInitSkillStruct(0, skillId);
		for (int i = 0; i < _refEntity.PlayerSkills[0].FastBulletDatas.Length; i++)
		{
			if (!MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(_refEntity.PlayerSkills[0].FastBulletDatas[i].s_MODEL))
			{
				BulletBase.PreloadBullet<BasicBullet>(_refEntity.PlayerSkills[0].FastBulletDatas[i]);
			}
		}
		SKL0_EX3_LOOP_FRAME = (int)(GetSklTime(0) / GameLogicUpdateManager.m_fFrameLen) - SKL0_EX3_START_END;
		float f_RANGE = _refEntity.PlayerSkills[0].FastBulletDatas[0].f_RANGE;
		shootPointTransform4.localPosition = new Vector3(f_RANGE + f_RANGE * 0.2f, 0f, 0f);
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
		float currentFrame = _refEntity.CurrentFrame;
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_WIN_POSE)
		{
			if (currentFrame >= 0.95f)
			{
				WhipMesh_Sub_e.SetActive(false);
			}
			else if (currentFrame >= 0.45f)
			{
				WhipMesh_Sub_e.SetActive(true);
			}
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id != 0 && id == 1 && _refEntity.PlayerSkills[id].Reload_index == 0 && _refEntity.CheckUseSkillKeyTriggerEX2(id))
		{
			SetGuardInactive();
			PlayVoiceSE("v_mh2_skill01_1");
			_refEntity.CurrentActiveSkill = id;
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_0_START_TRIGGER, SKL1_0_START_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
			UpdateAnalog(true);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
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
				OrangeCharacter.SubStatus p_nextStatus = OrangeCharacter.SubStatus.SKILL0;
				int p_sklTriggerFrame = SKL0_TRIGGER;
				int p_endFrame = SKL0_END;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				switch (_enhanceSlot)
				{
				case 1:
					p_nextStatus = OrangeCharacter.SubStatus.SKILL0_1;
					break;
				case 2:
					p_nextStatus = OrangeCharacter.SubStatus.SKILL0_2;
					break;
				case 3:
					p_nextStatus = OrangeCharacter.SubStatus.SKILL0_3;
					p_sklTriggerFrame = SKL0_EX3_START_END;
					p_endFrame = SKL0_EX3_START_END;
					break;
				}
				PlayVoiceSE("v_mh2_skill02");
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IsShoot = 1;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, p_sklTriggerFrame, p_endFrame, p_nextStatus, out skillEventFrame, out endFrame);
			}
			break;
		case 1:
		{
			int reload_index = _refEntity.PlayerSkills[id].Reload_index;
			if (reload_index == 1 && _refEntity.CheckUseSkillKeyTriggerEX2(id))
			{
				SetGuardInactive();
				PlayVoiceSE("v_mh2_skill01_2");
				_refEntity.CurrentActiveSkill = id;
				_refEntity.IsShoot = 1;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_1_TRIGGER, SKL1_1_OFFSET_START, OrangeCharacter.SubStatus.SKILL1_3, out skillEventFrame, out endFrame);
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, currentSkillObj.weaponStatus, currentSkillObj.ShootTransform[_refEntity.CurrentActiveSkill], null, currentSkillObj.Reload_index);
				UpdateAnalog(false);
			}
			break;
		}
		}
	}

	private void UpdateAnalog(bool active)
	{
		if (_refEntity is OrangeConsoleCharacter)
		{
			(_refEntity as OrangeConsoleCharacter).SetVirtualButtonAnalog(VirtualButtonId.SKILL1, active);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.FreshBullet = true;
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, HumanBase.AnimateId.ANI_BTSKILL_START, HumanBase.AnimateId.ANI_BTSKILL_START);
				UpdateCustomWeaponRenderer(true, true);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.FreshBullet = true;
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, HumanBase.AnimateId.ANI_BTSKILL_START, HumanBase.AnimateId.ANI_BTSKILL_START);
				UpdateCustomWeaponRenderer(true, true);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.FreshBullet = true;
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_BTSKILL_START, HumanBase.AnimateId.ANI_BTSKILL_START, HumanBase.AnimateId.ANI_BTSKILL_START);
				UpdateCustomWeaponRenderer(true, true);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.FreshBullet = true;
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)129u, (HumanBase.AnimateId)129u, (HumanBase.AnimateId)129u);
				UpdateCustomWeaponRenderer(true, true);
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)130u, (HumanBase.AnimateId)130u, (HumanBase.AnimateId)130u);
				chargeFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(sFxuse000_EX3_0, shootPointTransform2, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse000_EX3_1, shootPointTransform2.position, shootPointTransform2.rotation, Array.Empty<object>());
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)131u, (HumanBase.AnimateId)131u, (HumanBase.AnimateId)131u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START);
				UpdateCustomWeaponRenderer(true, true);
				counterFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(sFxuse001_0, _refEntity.ModelTransform.position, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.FreshBullet = true;
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)128u, (HumanBase.AnimateId)128u, (HumanBase.AnimateId)128u);
				UpdateCustomWeaponRenderer(true, true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse001_1, shootPointTransform2, OrangeBattleUtility.QuaternionNormal, Array.Empty<object>());
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
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
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
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (nowFrame >= endFrame)
			{
				_refEntity.IsShoot = 1;
				PlaySkillSE("mh2_tama05");
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 0, SKL0_EX3_LOOP_FRAME, SKL0_EX3_LOOP_FRAME, OrangeCharacter.SubStatus.SKILL0_4, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			if (nowFrame >= endFrame)
			{
				if (chargeFx != null && chargeFx.gameObject.activeSelf)
				{
					chargeFx.BackToPool();
					chargeFx = null;
				}
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_EX3_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 0, SKL0_EX3_END_END, SKL0_EX3_END_END, OrangeCharacter.SubStatus.SKILL0_5, out skillEventFrame, out endFrame);
				isSkillEventEnd = true;
				Vector2.SignedAngle(Vector2.right, _refEntity.ShootDirection);
				shootPointTransform3.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, _refEntity.ShootDirection));
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform4, MagazineType.ENERGY, -1, 0);
				SetRecoil(0.1f);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_5:
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (nowFrame >= endBreakFrame)
			{
				SetRecoil(0f);
				if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
				{
					endFrame = nowFrame + 1;
				}
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
				isSkillEventEnd = true;
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
				int p_endFrame = SKL1_0_END_END;
				if (_refEntity.IgnoreGravity)
				{
					p_endFrame = SKL1_0_END_END_AIR;
				}
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 1, SKL1_0_START_TRIGGER, p_endFrame, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
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
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_1_END_BREAK - SKL1_1_OFFSET_START;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 1, SKL1_1_OFFSET_END - SKL1_1_OFFSET_START, SKL1_1_END - SKL1_1_OFFSET_START, OrangeCharacter.SubStatus.SKILL1_4, out skillEventFrame, out endFrame);
				isSkillEventEnd = false;
				SetRecoil(0.8f);
				if (_refEntity.IgnoreGravity)
				{
					_refEntity.IgnoreGravity = false;
				}
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				int reload_index = currentSkillObj.Reload_index;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, shootPointTransform2, MagazineType.ENERGY, reload_index, 1, false);
				ComboCheckData[] comboCheckDatas = currentSkillObj.ComboCheckDatas;
				for (int i = 0; i < comboCheckDatas.Length; i++)
				{
					_refEntity.RemoveComboSkillBuff(comboCheckDatas[i].nComboSkillID);
				}
				currentSkillObj.Reload_index = 0;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			if (nowFrame >= endFrame)
			{
				_refEntity.CurrentActiveSkill = -1;
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				SetRecoil(0f);
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
		if (ManagedSingleton<InputStorage>.Instance.IsReleased(_refEntity.UserID, ButtonId.SKILL0))
		{
			if (_refEntity.CanPlayerPressSkill(0, false))
			{
				SetGuardInactive();
				isSkillEventEnd = false;
				_refEntity.CurrentActiveSkill = -1;
				_refEntity.PlayerReleaseSkill(0);
				return;
			}
		}
		else if (ManagedSingleton<InputStorage>.Instance.IsReleased(_refEntity.UserID, ButtonId.SKILL1))
		{
			isSkillEventEnd = false;
			_refEntity.CurrentActiveSkill = -1;
			_refEntity.PlayerReleaseSkill(1);
			return;
		}
		if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
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

	private void SetRecoil(float rate)
	{
		_refEntity.SetHorizontalSpeed(Mathf.RoundToInt((float)(OrangeCharacter.WalkSpeed * _refEntity.direction * -1) * rate));
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
		WeaponMesh_c.SetActive(enableWeapon);
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
		if (counterFx != null && counterFx.gameObject.activeSelf)
		{
			counterFx.BackToPool();
			counterFx = null;
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
		return new string[3] { "ch091_skill_02_step1_start", "ch091_skill_02_step1_loop", "ch091_skill_02_step1_end" };
	}

	public override string[][] GetCharacterDependAnimationsBlendTree()
	{
		string[] array = new string[3] { "ch091_skill_01_up", "ch091_skill_01_mid", "ch091_skill_01_down" };
		string[] array2 = new string[3] { "ch091_skill_02_step2_shot_up", "ch091_skill_02_step2_shot_mid", "ch091_skill_02_step2_shot_down" };
		string[] array3 = new string[3] { "ch091_skill_01_charging_up_start", "ch091_skill_01_charging_mid_start", "ch091_skill_01_charging_down_start" };
		string[] array4 = new string[3] { "ch091_skill_01_charging_up_loop", "ch091_skill_01_charging_mid_loop", "ch091_skill_01_charging_down_loop" };
		string[] array5 = new string[3] { "ch091_skill_01_charging_up_end", "ch091_skill_01_charging_mid_end", "ch091_skill_01_charging_down_end" };
		return new string[5][] { array, array2, array3, array4, array5 };
	}
}
