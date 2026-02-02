using System;
using UnityEngine;

public class CH127_Controller : CharacterControlBase
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private Vector3? _targetPos;

	private bool _isTeleporation;

	private CharacterMaterial saberCM;

	private MeleeWeaponTrail saberTrail;

	private ParticleSystem psLandburst;

	private Vector3 psLandBurstOffeset = new Vector3(1.2f, 0f, 0f);

	private OrangeConsoleCharacter _refPlayer;

	private readonly string FX_0_00 = "fxuse_dashslash_000";

	private readonly string FX_0_01 = "fxuse_dashslash_001";

	private readonly string FX_1_00 = "fxuse_landburst_000";

	private readonly string FX_1_01 = "fxhit_landburst_000";

	private readonly int SKL0_START_END = (int)(0.133f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_LOOP_END = (int)(0.417f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_TRIGGER = (int)(0.234f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.434f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_END = (int)(0.444f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_TRIGGER = (int)(0.155f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_1_END_BREAK = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.8f / GameLogicUpdateManager.m_fFrameLen);

	private bool IsPVPMode
	{
		get
		{
			return MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp;
		}
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		_refPlayer = _refEntity as OrangeConsoleCharacter;
	}

	private void InitializeSkill()
	{
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_L", true);
		_refEntity.ExtraTransforms[1] = transform;
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		GameObject gameObject = OrangeBattleUtility.FindChildRecursive(ref target, "StickMesh_c", true).gameObject;
		if ((bool)gameObject)
		{
			saberCM = gameObject.GetComponent<CharacterMaterial>();
			saberCM.Disappear();
		}
		GameObject gameObject2 = OrangeBattleUtility.FindChildRecursive(ref target, "Trail", true).gameObject;
		if ((bool)gameObject2)
		{
			saberTrail = gameObject2.GetComponent<MeleeWeaponTrail>();
			saberTrail.Emit = false;
			psLandburst = gameObject2.GetComponentInChildren<ParticleSystem>();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_01, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_01, 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.ChangeComboSkillEventEvt = ChangeComboSkillEvent;
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
		{
			if (_refEntity.PlayerSkills[0].Reload_index != 0 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			UpdateCustomWeaponRenderer(true, true);
			PlayVoiceSE("v_er2_skill01");
			PlaySkillSE("er2_empress01");
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_START_END, SKL0_START_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
			PlayerAutoAimSystem playerAutoAimSystem = _refEntity.PlayerAutoAimSystem;
			IAimTarget aimTarget = playerAutoAimSystem.AutoAimTarget;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[id];
			if (IsPVPMode && aimTarget != null)
			{
				float magnitude = (aimTarget.AimPosition - _refEntity.AimPosition).magnitude;
				if (!playerAutoAimSystem.IsInsideScreenExactly(aimTarget.AimPosition) || magnitude > weaponStruct.BulletData.f_DISTANCE)
				{
					aimTarget = null;
				}
			}
			_targetPos = null;
			_isTeleporation = false;
			if (IsPVPMode)
			{
				if (aimTarget != null)
				{
					_targetPos = aimTarget.AimPosition;
					_isTeleporation = true;
					OrangeCharacter orangeCharacter = aimTarget as OrangeCharacter;
					if (orangeCharacter != null)
					{
						_targetPos = orangeCharacter._transform.position;
					}
				}
			}
			else
			{
				float f_DISTANCE = weaponStruct.BulletData.f_DISTANCE;
				Vector3 aimPosition;
				if (aimTarget == null)
				{
					aimPosition = _refEntity.AimPosition;
					aimPosition.x += Mathf.Sign(_refEntity.ShootDirection.x) * f_DISTANCE;
				}
				else
				{
					aimPosition = aimTarget.AimPosition;
				}
				_targetPos = Vector3.MoveTowards(_refEntity.AimPosition, aimPosition, f_DISTANCE);
			}
			if (_targetPos.HasValue)
			{
				int num = Math.Sign((_targetPos.Value - _refEntity.AimPosition).normalized.x);
				_refEntity.direction = ((num != 0) ? num : _refEntity.direction);
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_00, _refEntity.ModelTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_0_01, _refEntity.AimTransform.position, (_refEntity.direction == 1) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			break;
		}
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_er2_skill02");
				PlaySkillSE("er2_bancho");
				_refEntity.CurrentActiveSkill = id;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)72u, (HumanBase.AnimateId)73u, (HumanBase.AnimateId)74u);
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
				UpdateCustomWeaponRenderer(true, true);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				psLandburst.Play(true);
				_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0]);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		if (id != 0)
		{
			int num = 1;
			return;
		}
		int reload_index = _refEntity.PlayerSkills[0].Reload_index;
		if (reload_index != 0 && reload_index == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_er2_skill03");
			endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_1_END_BREAK;
			ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
			ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_1_TRIGGER, SKL0_1_END, OrangeCharacter.SubStatus.SKILL0_3, out skillEventFrame, out endFrame);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)71u);
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			UpdateCustomWeaponRenderer(true, true);
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
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
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				isSkillEventEnd = false;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_END_TRIGGER, SKL0_END_END, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= endFrame)
			{
				_refEntity.BulletCollider.BackToPool();
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ModelTransform, MagazineType.NORMAL, 0, 0, false);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endBreakFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, 1, 1);
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
				_refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[weaponStruct.Reload_index].n_ID);
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
				Vector3 p_worldPos = ((_refEntity.direction == 1) ? (_refEntity.ModelTransform.position + psLandBurstOffeset) : (_refEntity.ModelTransform.position - psLandBurstOffeset));
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, -1, 0, false);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_01, p_worldPos, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endBreakFrame);
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
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != (HumanBase.AnimateId)72u)
		{
			if (_refEntity.IsInGround)
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

	private void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		{
			_refEntity.IgnoreGravity = true;
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0], _refEntity.direction * Vector2.right, 0);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, HumanBase.AnimateId.ANI_SKILL_START, false);
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			if (_targetPos.HasValue)
			{
				Vector3 vector = _targetPos.Value - _refEntity.AimPosition;
				if (_isTeleporation)
				{
					Vector3 value = _targetPos.Value;
					if (_refEntity.IsLocalPlayer)
					{
						_refEntity.Controller.LogicPosition = new VInt3(value);
						_refEntity.transform.position = value;
					}
				}
				else
				{
					VInt2 vInt = new VInt2(vector / OrangeBattleUtility.PPU / OrangeBattleUtility.FPS / GameLogicUpdateManager.m_fFrameLen);
					_refEntity.SetSpeed(vInt.x, vInt.y);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)66u, false);
				}
			}
			int num = 0;
			endFrame = nowFrame + num;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_2:
			PlaySkillSE("er2_empress02");
			_refEntity.SetSpeed(0, 0);
			ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)67u, (HumanBase.AnimateId)68u);
			break;
		}
	}

	protected void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.SKILL && subStatus == OrangeCharacter.SubStatus.SKILL0)
		{
			_refEntity.IgnoreGravity = true;
			if (_isTeleporation && !_targetPos.HasValue)
			{
				isSkillEventEnd = false;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_END_TRIGGER, SKL0_END_END, OrangeCharacter.SubStatus.SKILL0_2, out skillEventFrame, out endFrame);
			}
			else
			{
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(_refEntity, SKL0_LOOP_END, SKL0_LOOP_END, OrangeCharacter.SubStatus.SKILL0_1, out skillEventFrame, out endFrame);
			}
		}
	}

	public override void ClearSkill()
	{
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		UpdateCustomWeaponRenderer(false);
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.BulletCollider.IsActivate)
		{
			_refEntity.BulletCollider.BackToPool();
		}
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			UpdateCustomWeaponRenderer(false);
			_refEntity.EnableCurrentWeapon();
		}
	}

	private void UpdateCustomWeaponRenderer(bool enableWeapon, bool enableTrail = false)
	{
		if (enableWeapon)
		{
			saberCM.Appear(null, 0f);
		}
		else
		{
			saberCM.Disappear(null, 0f);
		}
		saberTrail.Emit = enableTrail;
	}

	public void ChangeComboSkillEvent(object[] parameters)
	{
		if (parameters.Length != 2)
		{
			return;
		}
		int num = (int)parameters[0];
		int num2 = (int)parameters[1];
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.TELEPORT_IN || _refEntity.CurMainStatus == OrangeCharacter.MainStatus.TELEPORT_OUT || (int)_refEntity.Hp <= 0 || num != 0)
		{
			return;
		}
		if (_refEntity.PlayerSkills[0].Reload_index != num2)
		{
			_refEntity.PlayerSkills[0].Reload_index = num2;
		}
		if (_refPlayer != null)
		{
			switch (num2)
			{
			case 0:
				_refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, false);
				_refPlayer.ClearVirtualButtonStick(VirtualButtonId.SKILL0);
				break;
			case 1:
				_refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL0, true);
				break;
			}
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[10] { "ch127_skill_01_step1_stand_start", "ch127_skill_01_step1_stand_loop", "ch127_skill_01_step1_stand_end", "ch127_skill_01_step1_jump_end", "ch127_skill_01_step2_end", "ch127_skill_01_step2_stand", "ch127_skill_01_step2_jump", "ch127_skill_02_crouch", "ch127_skill_02_stand", "ch127_skill_02_jump" };
	}
}
