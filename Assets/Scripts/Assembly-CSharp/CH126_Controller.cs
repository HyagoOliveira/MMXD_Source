using System;
using UnityEngine;

public class CH126_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private OrangeTimer NOVASTRIKETimer;

	private SKILL_TABLE linkSkl;

	private bool bNeedAddVoiceFlag;

	protected PlayerAutoAimSystem _pSkill0AimSystem;

	protected IAimTarget _pSkill0Target;

	protected bool bInSkill;

	private readonly string sFxuse_skl100 = "fxuse_flyfist_000";

	private readonly string sFxuse_skl101 = "fxuse_lightbowgun_000";

	private readonly int SKL0_TRIGGER = 1;

	private readonly int SKL0_END = (int)(0.233f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.233f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.18f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.66f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.37f / GameLogicUpdateManager.m_fFrameLen);

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	private void InitializeSkill()
	{
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 1.5f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_L", true);
		_refEntity.ExtraTransforms[1] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		InitSkl0AimSystem();
		NOVASTRIKETimer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl100);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sFxuse_skl101);
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<CollideBullet>(_refEntity, 0, out linkSkl);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
	}

	public void SyncSkillVoice()
	{
		if (bNeedAddVoiceFlag)
		{
			_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0, _refEntity.sNetSerialID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify);
			bNeedAddVoiceFlag = false;
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			PlayVoiceSE("v_sg_skill01");
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
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
				UseSkill0(0);
			}
			break;
		case 1:
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			switch (_refEntity.PlayerSkills[1].Reload_index)
			{
			case 0:
			{
				PlayVoiceSE("v_sg_skill03");
				PlaySkillSE("sg_combo01");
				ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Prepare(_refEntity, 1);
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				_refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0], null, weaponStruct.Reload_index);
				break;
			}
			case 1:
			{
				HumanBase.AnimateId animateId = (HumanBase.AnimateId)69u;
				if (!MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsMultiply)
				{
					animateId = ((UnityEngine.Random.Range(0, 2) != 0) ? ((HumanBase.AnimateId)70u) : ((HumanBase.AnimateId)69u));
				}
				PlayVoiceSE("v_sg_skill02");
				PlaySkillSE("sg_combo02");
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, animateId, animateId, animateId);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				break;
			}
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public override void CheckSkill()
	{
		SyncSkillVoice();
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
			else if (nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endBreakFrame);
			}
			break;
		case OrangeCharacter.SubStatus.RIDE_ARMOR:
			ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Begin(_refEntity, NOVASTRIKETimer, 1, false, false);
			break;
		case OrangeCharacter.SubStatus.IDLE:
			ManagedSingleton<CharacterControlHelper>.Instance.NOVASTRIKE_Loop(_refEntity, NOVASTRIKETimer, 1);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct currentSkillObj = _refEntity.GetCurrentSkillObj();
				int reload_index = currentSkillObj.Reload_index;
				_refEntity.ShootDirection = Vector2.right * _refEntity.direction;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, reload_index, 0, false);
				_refEntity.CheckUsePassiveSkill(1, currentSkillObj.weaponStatus, _refEntity.ModelTransform, _refEntity.direction * Vector2.right, reload_index);
				SKILL_TABLE sKILL_TABLE = _refEntity.PlayerSkills[1].FastBulletDatas[_refEntity.PlayerSkills[1].Reload_index];
				_refEntity.RemoveComboSkillBuff(sKILL_TABLE.n_ID);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl101, _refEntity.AimTransform.position, Quaternion.identity, Array.Empty<object>());
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endBreakFrame);
			}
			break;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.IDLE)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sFxuse_skl100, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
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
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != (HumanBase.AnimateId)66u)
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
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		_refEntity.EnableCurrentWeapon();
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length == 2)
		{
			int num = (int)parameters[0];
			int num2 = (int)parameters[1];
			if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_IN && _refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT && (int)_refEntity.Hp > 0 && num == 1 && _refEntity.PlayerSkills[1].Reload_index != num2)
			{
				_refEntity.PlayerSkills[1].Reload_index = num2;
			}
		}
	}

	private void InitSkl0AimSystem()
	{
		GameObject gameObject = new GameObject("Skill0AutoAimSystem");
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		_pSkill0AimSystem = gameObject.AddOrGetComponent<PlayerAutoAimSystem>();
		_pSkill0AimSystem.targetMask = _refEntity.PlayerAutoAimSystem.targetMask;
		_pSkill0AimSystem.Init(false, _refEntity.IsLocalPlayer);
		_pSkill0AimSystem.UpdateAimRange(_refEntity.PlayerSkills[0].BulletData.f_DISTANCE);
	}

	private void FindSkill0Target()
	{
		_pSkill0Target = null;
		if (_refEntity.PlayerAutoAimSystem.AutoAimTarget != null && Vector2.Distance(_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition, _refEntity.AimPosition) < _refEntity.PlayerSkills[0].BulletData.f_DISTANCE)
		{
			_pSkill0Target = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
		}
		if (_pSkill0Target == null)
		{
			_pSkill0Target = _pSkill0AimSystem.GetClosestTarget();
		}
	}

	private void UseSkill0(int skillId)
	{
		if (_refEntity.IsLocalPlayer)
		{
			FindSkill0Target();
			if (_pSkill0Target != null)
			{
				DoSkill0(skillId);
			}
		}
	}

	private void DoSkill0(int skillId)
	{
		isSkillEventEnd = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		if (_pSkill0Target != null)
		{
			Vector3 normalized = (_pSkill0Target.AimPosition - _refEntity.AimPosition).normalized;
			int num = Math.Sign(normalized.x);
			if (_refEntity.direction != num && Mathf.Abs(normalized.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = normalized;
			}
		}
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[skillId];
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.AimTransform, _pSkill0Target, weaponStruct.SkillLV);
		_refEntity.CheckUsePassiveSkill(skillId, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
		ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, skillId, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
		ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)68u);
		if (linkSkl != null)
		{
			bNeedAddVoiceFlag = true;
			_refEntity.PushBulletDetail(linkSkl, weaponStruct.weaponStatus, _refEntity.ModelTransform, weaponStruct.SkillLV);
		}
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch126_skill_02_jump_start", "ch126_skill_02_jump_loop" };
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch126_skill_02_jump_start", "ch126_skill_01_crouch", "ch126_skill_01_stand", "ch126_skill_01_jump", "ch126_skill_02_stand_step2_punch", "ch126_skill_02_stand_step2_step" };
	}
}
